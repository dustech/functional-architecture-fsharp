namespace Dustech.BookingApi.Controllers

open System
open System.Globalization // for CultureInfo
open System.Reactive.Subjects // for Subject
open Microsoft.AspNetCore.Mvc //for ApiController Attribute
open Dustech.BookingApi.Utility // for curry e uncurry
open Dustech.BookingApi.Messages // for all Messages types
open Dustech.BookingApi.Renditions // for MakeReservationRendition cmd
open Dustech.BookingApi.DomainModel //for Period sum type
open Dustech.BookingApi.DomainModel.Notifications // for INotifications
open Dustech.BookingApi.DomainModel.Dates // for dates manipulation
open Dustech.BookingApi.DomainModel.Reservations // for IReservations


[<ApiController>]
[<Route("[controller]")>]
type HomeController() =
    inherit ControllerBase()

    [<HttpGet>]
    member _.Get() =
        ``base``.Ok("Hello from F# Controller!")


[<ApiController>]
[<Route("[controller]")>]
type ReservationController(reservations: IReservations) =
    inherit Controller()
    let subject = new Subject<Envelope<MakeReservation>>()
    
    let getReservationsIn period =
            BoundariesIn period
            |> uncurry (reservations.Between )                 
            
            
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

    [<HttpGet("{year}/{month}/{day}")>]
    member this.Get(year: int, month, day) =
        let reservations = getReservationsIn <| Day(year,month,day) 
        ``base``.Ok(reservations)

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

    let getAvailableSeats map (now : DateTimeOffset) date =
        if date < now.Date then 0
        elif map |> Map.containsKey date then
            seatingCapacity - (map |> Map.find date)
        else
            seatingCapacity
    let toMapOfDatesAndQuantities (min,max) reservations =
        reservations
            |> Between min max
            |> Seq.groupBy (_.Item.Date)
            |> Seq.map (fun (d,rs) -> (d, rs |> Seq.sumBy (_.Item.Quantity)))
            |> Map.ofSeq
    
    let toOpening ((d: DateTime),seats :int ) =
            { Date = d.ToString "yyyy.MM.dd"
              Seats = seats }
    let getOpeningsIn period =
        let boundaries = (BoundariesIn period)
        let map = reservations |> toMapOfDatesAndQuantities boundaries
        let getAvailable = getAvailableSeats map DateTimeOffset.Now
        
        In period
        |> Seq.map (fun d -> (d, getAvailable d))
        |> Seq.map toOpening
        |> Seq.toArray
        
    
    [<HttpGet("{year}")>]
    member this.Get year =
        let openings = getOpeningsIn <| Year(year)

        ``base``.Ok({ Openings = openings })

    [<HttpGet("{year}/{month}")>]
    member this.Get(year, month) =
        let openings = getOpeningsIn <| Month(year,month)

        ``base``.Ok({ Openings = openings })

    [<HttpGet("{year}/{month}/{day}")>]
    member this.Get(year: int, month, day) =
        let openings = getOpeningsIn <| Day(year,month,day) 

        ``base``.Ok({ Openings = openings })

    member this.SeatingCapacity = seatingCapacity
