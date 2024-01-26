namespace Dustech.BookingApi.Controllers

open System
open System.Globalization
open System.Reactive.Subjects
open Dustech.BookingApi.Messages
open Dustech.BookingApi.Renditions
open Microsoft.AspNetCore.Mvc //for ApiController Attribute

type SampleJson = {Message:string}

[<ApiController>]
[<Route("[controller]")>]
type HomeController () =
    inherit ControllerBase()

    [<HttpGet>]
    member _.Get() =
        failwith "Tutto rotto male"
        base.Ok("Hello from F# Controller!")




[<ApiController>]
[<Route("[controller]")>]
type ReservationController () =
    inherit Controller()
    
    let subject = new Subject<Envelope<MakeReservation>>()
    
    [<HttpPost>]
    member _.Post (rendition : MakeReservationRendition) =
        let cmd =
            {
                MakeReservation.Date = DateTime.Parse(rendition.Date,CultureInfo.InvariantCulture)                        
                Name = rendition.Name
                Email = rendition.Email
                Quantity = rendition.Quantity 
            }
            |> EnvelopWithDefaults
        subject.OnNext cmd
        Console.WriteLine(rendition)
        Console.WriteLine(cmd)
        base.Accepted()
    
    interface IObservable<Envelope<MakeReservation>> with
        member this.Subscribe(observer) = subject.Subscribe observer
    
    override self.Dispose disposing =
        if disposing then subject.Dispose()
        base.Dispose disposing