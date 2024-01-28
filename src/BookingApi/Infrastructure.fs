module Dustech.BookingApi.Infrastructure

open System // for Type
open System.Threading.Tasks
open Dustech.BookingApi.Controllers // for custom controllers
open Microsoft.AspNetCore.Builder // for WebApplication
open Microsoft.AspNetCore.Mvc // for ControllerContext
open Microsoft.AspNetCore.Mvc.Controllers // for IControllerActivator
open Microsoft.AspNetCore.Routing.Constraints // for OptionalRouteConstraint


type HttpRouteDefaults = { Controller: string; Id: obj }

type BookingApiControllerActivator() =
    let Make (context: ControllerContext, controllerType: Type) : obj =
        match controllerType.Name with
        | nameof HomeController -> HomeController()
        | nameof ReservationController -> new ReservationController()
        | _ -> raise (InvalidOperationException($"Unknown controller {controllerType}"))

    interface IControllerActivator with
        member me.Create(context) =
            Make(context, context.ActionDescriptor.ControllerTypeInfo.AsType())

        member _.Release(context, controller) =
            match controller with
            | :? IDisposable as c -> c.Dispose()
            | _ -> ()

        member me.ReleaseAsync(context, controller) =
            (me :> IControllerActivator).Release(context, controller) |> ValueTask







let ConfigureRoutes (app: WebApplication) =
    app.MapControllerRoute(
        "DefaultAPI",
        "{controller}/{id}",
        { Controller = "Home"
          Id = OptionalRouteConstraint }
    )
    |> ignore

//let ConfigureFormatting (config: HttpConfiguration)

let Configure = ConfigureRoutes
