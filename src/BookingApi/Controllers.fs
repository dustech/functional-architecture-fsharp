namespace Dustech.BookingApi.Controllers

open System
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
    inherit ControllerBase()
    
    [<HttpPost>]
    member _.Post (rendition : MakeReservationRendition) =
        Console.WriteLine(rendition)
        base.Accepted()
    
    