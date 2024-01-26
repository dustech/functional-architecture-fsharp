namespace Dustech.BookingApi.DomainModel

open System
open Dustech.BookingApi.Messages

module Reservations =
    
    type IReservations =
        inherit seq<Envelope<Reservation>>
        abstract Between : DateTime -> DateTime -> seq<Envelope<Reservation>>
    
    type ReservationsInMemory(reservations) =
        interface IReservations with
            member this.Between(var0) (var1) = failwith "todo"
            member this.GetEnumerator(): Collections.Generic.IEnumerator<Envelope<Reservation>> = failwith "todo"
            member this.GetEnumerator(): Collections.IEnumerator = failwith "todo"
        