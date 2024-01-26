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