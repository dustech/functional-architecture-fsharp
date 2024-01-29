namespace Dustech.BookingApi.Controllers

open System
open System.Globalization // for CultureInfo
open System.Reactive.Subjects // for Subject
open Microsoft.AspNetCore.Mvc //for ApiController Attribute
open Dustech.BookingApi.Messages // for all Messages types
open Dustech.BookingApi.Renditions // for MakeReservationRendition cmd
open Dustech.BookingApi.DomainModel.Notifications // for INotifications

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
        base.Accepted()

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
        
        base.Ok({Notifications = matches })
        
    [<HttpGet("{id:guid}")>]
    member this.Get id =

        let matches =
            this.Notifications
            |> About id
            |> Seq.map toRendition
            |> Seq.toArray
        
        base.Ok({Notifications = matches })

    member this.Notifications = notifications
