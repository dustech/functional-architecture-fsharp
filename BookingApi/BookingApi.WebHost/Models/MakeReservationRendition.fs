namespace BookingApi.WebHost.Models

open System

type MakeReservationRendition =
    {   
        Date:       string
        Name:       string
        Email:      string
        Quantity:   int
    }