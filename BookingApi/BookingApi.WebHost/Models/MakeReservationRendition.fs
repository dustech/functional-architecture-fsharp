namespace BookingApi.WebHost.Models

open System

type MakeReservationRendition =
    {   
        Date:       DateTime
        Name:       String
        Email:      String
        Quantity:   int
    }