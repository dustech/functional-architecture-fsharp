namespace BookingApi.WebHost.Controllers

open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open System.Net.Http
open System.Net
open BookingApi.WebHost.Models



type Cannolo =
    { MyCannolo: int }


[<ApiController>]
[<Route("[controller]")>]
type ReservationsController (logger : ILogger<ReservationsController>) =
    inherit ControllerBase()


    [<HttpGet>]
    member _.Get() =
        [|
            {MyCannolo=15}
        |]

    [<HttpPost>]
    member _.Post (rendition:MakeReservationRendition)  = 
        // printfn "%A" rendition
        new HttpResponseMessage (HttpStatusCode.Accepted)