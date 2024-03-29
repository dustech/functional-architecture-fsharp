namespace Dustech.BookingApi.DomainModel

open System
open Dustech.BookingApi.Messages

type Period =
    | Year of int
    | Month of int * int
    | Day of int * int * int

module Dates =
    let InitInfinite (date: DateTime) =
        date
        |> Seq.unfold (fun d -> Some(d, d.AddDays 1.0))

    let In period =
        let generate dt predicate =
            dt |> InitInfinite |> Seq.takeWhile predicate

        match period with
        | Year (y) -> generate (DateTime(y, 1, 1)) (fun d -> d.Year = y)
        | Month (y, m) -> generate (DateTime(y, m, 1)) (fun d -> d.Month = m)
        | Day (y, m, d) -> DateTime(y, m, d) |> Seq.singleton
    
    let BoundariesIn period =
        let getBoundaries firstTick (forward:DateTime -> DateTime) =
            let lastTick = forward(firstTick).AddTicks -1L
            (firstTick,lastTick)
        
        match period with
        | Year(y) -> getBoundaries (DateTime(y,1,1)) (fun d -> d.AddYears 1)
        | Month(y,m) -> getBoundaries (DateTime(y,m,1)) (fun d -> d.AddMonths 1)
        | Day(y,m,d) -> getBoundaries (DateTime(y,m,d)) (fun d -> d.AddDays 1.0)
    
module Reservations =

    type IReservations =
        inherit seq<Envelope<Reservation>>
        abstract Between: DateTime -> DateTime -> seq<Envelope<Reservation>>

    type ReservationsInMemory(reservations) =
        interface IReservations with
            member _.Between min max =
                reservations
                |> Seq.filter (fun r -> min <= r.Item.Date && r.Item.Date <= max)

            member _.GetEnumerator() = reservations.GetEnumerator()

            member this.GetEnumerator() =
                (this :> seq<Envelope<Reservation>>)
                    .GetEnumerator()
                :> System.Collections.IEnumerator

    let ToReservations reservations = ReservationsInMemory(reservations)

    let Between min max (reservations: IReservations) = reservations.Between min max

    let On (date: DateTime) reservations =
        let min = date.Date
        let max = (min.AddDays 1.0) - TimeSpan.FromTicks 1L
        reservations |> Between min max

    let Print reservation =
        printfn
            $"
            Envelope data:
                Created: %A{reservation.Created}
                Id: %A{reservation.Id}
                Reservation data:
                    Date: %A{reservation.Item.Date},
                    Name: %s{reservation.Item.Name},
                    Email: %s{reservation.Item.Email},
                    Quantity: %d{reservation.Item.Quantity}"

    let rec PrintAll reservations =
        match reservations with
        | [] -> printfn "No more reservations"
        | reservation :: tail ->
            Print reservation
            PrintAll tail

    let Handle capacity reservations (cmd: Envelope<MakeReservation>) =
        let reservedSeatsOnDate =
            reservations
            |> On cmd.Item.Date
            |> Seq.sumBy (fun r -> r.Item.Quantity)

        if capacity - reservedSeatsOnDate < cmd.Item.Quantity then
            None
        else
            MessagesOperations.toReservation cmd.Item
            |> EnvelopWithDefaults
            |> Some

module Notifications =

    type INotifications =
        inherit seq<Envelope<Notification>>
        abstract About: Guid -> seq<Envelope<Notification>>

    type NotificationsInMemory(notifications) =
        interface INotifications with
            member _.About id =
                notifications
                |> Seq.filter (fun n -> n.Item.About = id)

            member _.GetEnumerator() = notifications.GetEnumerator()

            member this.GetEnumerator() =
                (this :> seq<Envelope<Notification>>)
                    .GetEnumerator()
                :> System.Collections.IEnumerator

    let ToNotifications notifications = NotificationsInMemory(notifications)

    let About id (notifications: INotifications) = notifications.About id
