module Dustech.BookingApi.Infrastructure

open System // for Type
open System.Collections.Concurrent // for ConcurrentBag
open System.Threading.Tasks //  for ValueTask
open Dustech.BookingApi.Controllers // for custom controllers
open Dustech.BookingApi.Messages // for Envelope
open Dustech.BookingApi.DomainModel.Reservations // for ToReservations and Handle
open Microsoft.AspNetCore.Builder // for WebApplication
open Microsoft.AspNetCore.Mvc // for ControllerContext
open Microsoft.AspNetCore.Mvc.Controllers // for IControllerActivator
open Microsoft.AspNetCore.Routing.Constraints // for OptionalRouteConstraint
open Microsoft.Extensions.DependencyInjection // for AddSingleton

type Agent<'T> = MailboxProcessor<'T>

type HttpRouteDefaults = { Controller: string; Id: obj }

type CompositionRoot() =

    let seatingCapacity = 10
    let reservations = ConcurrentBag<Envelope<Reservation>>()

    let agent =
        new Agent<Envelope<MakeReservation>>(fun inbox ->
            let rec loop () =
                async {
                    let! cmd = inbox.Receive()
                    let rs = reservations |> ToReservations
                    let handle = Handle seatingCapacity rs
                    let newReservations = handle cmd

                    match newReservations with
                    | Some(r) -> reservations.Add r
                    | None -> ()
                    
                    Seq.toList rs 
                    |>  PrintAll
                    
                    return! loop ()
                }

            loop ())

    do agent.Start()

    let Make (context: ControllerContext, controllerType: Type) : obj =
        match controllerType.Name with
        | nameof HomeController -> HomeController()
        | nameof ReservationController ->
            let c = new ReservationController()
            let sub = c.Subscribe agent.Post
            context.HttpContext.Response.RegisterForDispose sub
            c
        | _ -> raise <| InvalidOperationException($"Unknown controller {controllerType}")


    interface IControllerActivator with
        member me.Create(context) =
            Make(context, context.ActionDescriptor.ControllerTypeInfo.AsType())

        member _.Release(context, controller) =
            match controller with
            | :? IDisposable as c -> c.Dispose()
            | _ -> ()

        member me.ReleaseAsync(context, controller) =
            (me :> IControllerActivator).Release(context, controller) |> ValueTask


let ConfigureServices (builder: WebApplicationBuilder) =
    builder.Services.AddControllers() |> ignore
    builder.Services.AddSingleton<IControllerActivator, CompositionRoot>() |> ignore

let ConfigureBuilder = ConfigureServices

let ConfigureRoutes (app: WebApplication) =
    app.UseHttpsRedirection() |> ignore

    app.MapControllerRoute(
        "DefaultAPI",
        "{controller}/{id}",
        { Controller = "Home"
          Id = OptionalRouteConstraint }
    )
    |> ignore

//let ConfigureFormatting (config: HttpConfiguration)

let Configure = ConfigureRoutes
