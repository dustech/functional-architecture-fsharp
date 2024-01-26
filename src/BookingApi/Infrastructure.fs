module BookingApi.Infrastructure 

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Routing.Constraints


type HttpRouteDefaults = {Controller:string; Id :obj}

let ConfigureRoutes (app : WebApplication) =
    app.MapControllerRoute(
            "DefaultAPI","{controller}/{id}",
            {Controller = "WeatherForecast"; Id = OptionalRouteConstraint}
            )
            |> ignore

let Configure = ConfigureRoutes