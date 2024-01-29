﻿module Dustech.BookingApi.Infrastructure

open System // for Type
open System.Threading.Tasks //  for ValueTask
open Microsoft.AspNetCore.Builder // for WebApplication
open Microsoft.AspNetCore.Mvc // for ControllerContext
open Microsoft.AspNetCore.Mvc.Controllers // for IControllerActivator
open Microsoft.AspNetCore.Routing.Constraints // for OptionalRouteConstraint
open Microsoft.Extensions.DependencyInjection // for AddSingleton
open Dustech.BookingApi.Controllers // for custom controllers
open Dustech.BookingApi.DomainModel.Notifications

type HttpRouteDefaults = { Controller: string; Id: obj }

type CompositionRoot(notifications, reservationRequestObserver) =

    let Make (context: ControllerContext, controllerType: Type) : obj =
        match controllerType.Name with
        | nameof HomeController -> HomeController()
        | nameof ReservationController ->
            let c = new ReservationController()

            c
            |> Observable.subscribe reservationRequestObserver
            |> context.HttpContext.Response.RegisterForDispose

            c
        | nameof NotificationController -> NotificationController(notifications)
        | _ ->
            raise
            <| InvalidOperationException($"Unknown controller {controllerType}")


    interface IControllerActivator with
        member me.Create(context) =
            Make(context, context.ActionDescriptor.ControllerTypeInfo.AsType())

        member _.Release(_, controller) =
            match controller with
            | :? IDisposable as c -> c.Dispose()
            | _ -> ()

        member me.ReleaseAsync(context, controller) =
            (me :> IControllerActivator)
                .Release(context, controller)
            |> ValueTask


let ConfigureServices (builder: WebApplicationBuilder) (notifications: INotifications)  reservationRequestObserver =
    builder.Services.AddControllers() |> ignore

    builder.Services.AddSingleton<IControllerActivator> (fun _ ->
        CompositionRoot(notifications,reservationRequestObserver) :> IControllerActivator)
    |> ignore



let ConfigureRoutes (app: WebApplication) =
    app.UseHttpsRedirection() |> ignore

    app.MapControllerRoute(
        "DefaultAPI",
        "{controller}/{id}",
        { Controller = "Home"
          Id = OptionalRouteConstraint }
    )
    |> ignore


let Configure = ConfigureRoutes
