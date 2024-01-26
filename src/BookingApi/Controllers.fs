namespace Dustech.BookingApi.Controllers

open System.Net.Http
open Microsoft.AspNetCore.Mvc //for ApiController Attribute

type SampleJson = {Message:string}

[<ApiController>]
[<Route("[controller]")>]
type HomeController () =
    inherit ControllerBase()

    [<HttpGet>]
    member _.Get() =
        base.Ok("Hello from F# Controller!")
        
[<ApiController>]
[<Route("[controller]")>]
type ReservationController () =
    inherit ControllerBase()
    
    [<HttpPost>]
    member _.Post (rendition:string) =
        base.Accepted()
    
    