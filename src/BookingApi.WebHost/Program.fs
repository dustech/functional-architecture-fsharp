namespace Dustech.BookingApi.WebHost

#nowarn "20"

open System // for Console
open System.Collections.Concurrent // for ConcurrentBag
open Dustech.BookingApi.Messages // for Envelop, Reservation
open Microsoft.AspNetCore.Builder // for WebApplication
open Dustech.BookingApi.Infrastructure // for ConfigureBuilder
open Dustech.BookingApi.DomainModel.Reservations // for ToReservations and Handle

module Program =
    let exitCode = 0

    type HttpRouteDefaults = { Controller: string; Id: obj }

    type Agent<'T> = MailboxProcessor<'T>
    let seatingCapacity = 10
    
    [<EntryPoint>]
    let main args =

        let builder = WebApplication.CreateBuilder(args)

        //builder.Services.AddControllers() |> ignore
        //builder.Services.AddSingleton<IControllerActivator>(fun _ -> new BookingApiControllerActivator() :> IControllerActivator) |> ignore
        //builder.Services.AddSingleton<IControllerActivator,BookingApiControllerActivator>() |> ignore
        let db = ConcurrentBag<Envelope<Reservation>>()
        let agent =
            new Agent<Envelope<MakeReservation>>(fun inbox ->
                let rec loop () =
                    async {
                        let! cmd = inbox.Receive()
                        let rs = db |> ToReservations
                        let handle = Handle seatingCapacity rs
                        let newReservations = handle cmd

                        match newReservations with
                        | Some (r) -> db.Add r
                        | None -> ()

                        Seq.toList rs |> PrintAll

                        return! loop ()
                    }

                loop ())

        do agent.Start()
        ConfigureBuilder
            builder
            (db |> ToReservations)
            agent.Post

        let app = builder.Build()

        Console.WriteLine($"{Environment.NewLine} {app.Environment.EnvironmentName} Environment {Environment.NewLine}")


        Configure app


        app.Run()

        exitCode
