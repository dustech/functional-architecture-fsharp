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
    
    [<HttpGet>]
    [<Route("{id}")>]
    member _.Get(id : int) =
        {Message = $"My Get but with {id}"}
        
