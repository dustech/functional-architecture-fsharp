namespace BookingApi.WebHost.Controllers

open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open System.Net.Http
open System.Net
open System
open BookingApi.HttpApi.Models
open BookingApi.WebHost.Models
open System.Reactive.Subjects

type Cannolo =
    { MyCannolo: int }


[<ApiController>]
[<Route("[controller]")>]
type ReservationsController (logger : ILogger<ReservationsController>) =
    inherit ControllerBase()
    let subject = new Subject<Envelope<MakeReservation>>()

    [<HttpGet>]
    member _.Get() =
        [|
            {MyCannolo=15}
        |]

    [<HttpPost>]
    member _.Post (rendition:MakeReservationRendition)  = 
        // printfn "%A" rendition
        new HttpResponseMessage (HttpStatusCode.Accepted)

    interface IObservable<Envelope<MakeReservation>> with
        member this.Subscribe observer = subject.Subscribe observer

