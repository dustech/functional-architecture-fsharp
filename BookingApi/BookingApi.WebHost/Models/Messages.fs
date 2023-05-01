namespace BookingApi.WebHost.Models

open System

module Envelope =
    type Envelope<'T> = 
        {
            Id      : Guid
            Created : DateTimeOffset
            Item    : 'T
        }