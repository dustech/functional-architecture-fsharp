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
    inherit Controller()
    let subject = new Subject<Envelope<MakeReservation>>()

    [<HttpGet>]
    member _.Get() =
        [|
            {MyCannolo=15}
        |]

    [<HttpPost>]
    member _.Post (rendition:MakeReservationRendition)  = 
        // printfn "%A" rendition
        //predispongo il comando da inviare nella pipe
        let cmd =   {
                        MakeReservation.Date    = DateTime.Parse rendition.Date
                        Name                    = rendition.Name
                        Email                   = rendition.Email
                        Quantity                = rendition.Quantity
                    }
                    |> EnvelopWithDefaults
        //pubblico il comando nella pipeline
        subject.OnNext cmd 
        new HttpResponseMessage (HttpStatusCode.Accepted)

    interface IObservable<Envelope<MakeReservation>> with
        member this.Subscribe observer = subject.Subscribe observer

    override this.Dispose disposing = 
        if disposing then 
            subject.Dispose()
        base.Dispose disposing
