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

let AtomLinkRenditionWithDefaults href =
    { Rel = "https://localhost:7045/notification"
      Href = href
      MType = None
      Hreflang = None
      Title = None
      Length = None }

type LinkListRendition = { Links: AtomLinkRendition array }

type OpeningsRendition = { Date: string; Seats: int }

type AvailabilityRendition = { Openings: OpeningsRendition array }
