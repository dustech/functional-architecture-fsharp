namespace Dustech.BookingApi.DomainModel

open System
open Dustech.BookingApi.Messages

module Reservations =
    
    type IReservations =
        inherit seq<Envelope<Reservation>>
        abstract Between : DateTime -> DateTime -> seq<Envelope<Reservation>>
    
    type ReservationsInMemory(reservations) =
        interface IReservations with
            member this.Between min max =
                reservations
                |> Seq.filter (fun r -> min <= r.Item.Date && r.Item.Date <= max)
            
            member _.GetEnumerator() =
                reservations.GetEnumerator()
            member self.GetEnumerator() =
                (self :> seq<Envelope<Reservation>>).GetEnumerator() :> System.Collections.IEnumerator
    let ToReservations reservations = ReservationsInMemory(reservations)
    
    let Between min max (reservations : IReservations) = 
        reservations.Between min max
    
    let On (date:DateTime) reservations =
        let min = date.Date
        let max = (min.AddDays 1.0) - TimeSpan.FromTicks 1L
        reservations |> Between min max