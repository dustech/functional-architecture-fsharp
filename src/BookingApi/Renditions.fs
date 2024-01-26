module Dustech.BookingApi.Renditions

open System

type MakeReservationRendition =
    {
        Date      : string
        Name      : string
        Email     : string
        Quantity  : int
    }