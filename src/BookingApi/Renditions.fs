module Dustech.BookingApi.Renditions

open System

type MakeReservationRendition =
    { Date: string
      Name: string
      Email: string
      Quantity: int }

type NotificationRendition =
    { About: string
      Type: string
      Message: string }

type NotificationListRendition =
    { Notifications: NotificationRendition array }
