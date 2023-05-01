namespace BookingApi.WebHost.Models

open System

type MakeReservationRendition =
    {   
        Date:       DateTime
        Name:       string
        Email:      string
        Quantity:   int
    }