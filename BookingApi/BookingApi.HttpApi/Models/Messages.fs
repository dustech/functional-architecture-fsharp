namespace BookingApi.HttpApi.Models

open System

type MakeReservation =
    {   
        Date:       DateTime
        Name:       string
        Email:      string
        Quantity:   int
    }



[<AutoOpen>]
module Envelope =
    type Envelope<'T> = 
        {
            Id      : Guid
            Created : DateTimeOffset
            Item    : 'T
        }
    
    let Envelop id created item = {
            Id = id
            Created = created
            Item = item
        }
    
    let EnvelopWithDefaults item = 
        Envelop (Guid.NewGuid()) (DateTimeOffset.Now) item

type Reservation =
    {   
        Date:       DateTime
        Name:       string
        Email:      string
        Quantity:   int
    }