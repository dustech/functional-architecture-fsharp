namespace Dustech.BookingApi.Controllers

open System
open System.Globalization // for CultureInfo
open System.Reactive.Subjects // for Subject
open Dustech.BookingApi.DomainModel.Reservations // for IReservations
open Microsoft.AspNetCore.Mvc //for ApiController Attribute
open Dustech.BookingApi.Messages // for all Messages types
open Dustech.BookingApi.Renditions // for MakeReservationRendition cmd
open Dustech.BookingApi.DomainModel.Notifications // for INotifications
open Dustech.BookingApi.DomainModel.Dates // for dates manipulation
open Dustech.BookingApi.DomainModel //for Period sum type

type SampleJson = { Message: string }

[<ApiController>]
[<Route("[controller]")>]
type HomeController() =
    inherit ControllerBase()

    [<HttpGet>]
    member _.Get() =
        ``base``.Ok("Hello from F# Controller!")


[<ApiController>]
[<Route("[controller]")>]
type ReservationController() =
    inherit Controller()
    let subject = new Subject<Envelope<MakeReservation>>()

    [<HttpPost>]
    member _.Post(rendition: MakeReservationRendition) =
        let cmd =
            { MakeReservation.Date = DateTime.Parse(rendition.Date, CultureInfo.InvariantCulture)
              Name = rendition.Name
              Email = rendition.Email
              Quantity = rendition.Quantity }
            |> EnvelopWithDefaults

        subject.OnNext cmd
        ``base``.Accepted({ Links = [| AtomLinkRenditionWithDefaults("notifications/" + cmd.Id.ToString "N") |] })

    [<HttpGet>]
    member _.Get() =
        ``base``.Ok("Time to get all reservations")

    interface IObservable<Envelope<MakeReservation>> with
        member _.Subscribe(observer) = subject.Subscribe observer

    override _.Dispose disposing =
        if disposing then subject.Dispose()
        base.Dispose disposing

[<ApiController>]
[<Route("[controller]")>]
type NotificationController(notifications: INotifications) =
    inherit ControllerBase()

    let toRendition (n: Envelope<Notification>) =
        { About = n.Item.About.ToString()
          Type = n.Item.Type
          Message = n.Item.Message }

    [<HttpGet>]
    member this.Get() =

        let matches =
            this.Notifications
            |> Seq.map toRendition
            |> Seq.toArray

        ``base``.Ok({ Notifications = matches })

    [<HttpGet("{id:guid}")>]
    member this.Get id =

        let matches =
            this.Notifications
            |> About id
            |> Seq.map toRendition
            |> Seq.toArray

        ``base``.Ok({ Notifications = matches })

    member this.Notifications = notifications

[<ApiController>]
[<Route("[controller]")>]
type AvailabilityController(reservations: IReservations, seatingCapacity: int) =
    inherit ControllerBase()

    [<HttpGet("{year}")>]
    member this.Get year =
        let now = DateTimeOffset.Now

        let openings =
            In(Year(year))
            |> Seq.map (fun d ->
                { Date = d.ToString "yyyy.MM.dd"
                  Seats =
                    if d < now.Date then
                        0
                    else
                        seatingCapacity })
            |> Seq.toArray

        ``base``.Ok({ Openings = openings })

    [<HttpGet("{year}/{month}")>]
    member this.Get(year, month) =
        let now = DateTimeOffset.Now

        let openings =
            In(Month(year, month))
            |> Seq.map (fun d ->
                { Date = d.ToString "yyyy.MM.dd"
                  Seats =
                    if d < now.Date then
                        0
                    else
                        seatingCapacity })
            |> Seq.toArray

        ``base``.Ok({ Openings = openings })

    [<HttpGet("{year}/{month}/{day}")>]
    member this.Get(year: int, month, day) =
        let now = DateTimeOffset.Now
        let requestedDate = DateTimeOffset(DateTime(year, month, day), now.Offset)

        let opening =
            { Date = requestedDate.ToString "yyyy.MM.dd"
              Seats =
                if requestedDate.Date < now.Date then
                    0
                else
                    seatingCapacity }

        ``base``.Ok({ Openings = [| opening |] })

    member this.SeatingCapacity = seatingCapacity
