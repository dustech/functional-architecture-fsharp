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

type AtomLinkRendition =
    { Rel: string
      Href: string
      MType: string option
      Hreflang: string option
      Title: string option
      Length: int option }

type LinkListRendition = { Links: AtomLinkRendition array }
