namespace Dustech.BookingApi.WebHost
#nowarn "20"
open System
open System.Collections.Concurrent // for ConcurrentBag
open Dustech.BookingApi.Messages // for Envelop, Reservation
open Microsoft.AspNetCore.Builder // for WebApplication
open Dustech.BookingApi.Infrastructure // for ConfigureBuilder
module Program =
    let exitCode = 0
    
    type HttpRouteDefaults = {Controller:string; Id :obj}
    
    
    [<EntryPoint>]
    let main args =

        let builder = WebApplication.CreateBuilder(args)
        
        //builder.Services.AddControllers() |> ignore
        //builder.Services.AddSingleton<IControllerActivator>(fun _ -> new BookingApiControllerActivator() :> IControllerActivator) |> ignore
        //builder.Services.AddSingleton<IControllerActivator,BookingApiControllerActivator>() |> ignore
        let db = ConcurrentBag<Envelope<Reservation>>()
        ConfigureBuilder builder db
        
        let app = builder.Build()
        
        Console.WriteLine($"{Environment.NewLine} {app.Environment.EnvironmentName} Environment {Environment.NewLine}")
                
                
        Configure app
        
        
        app.Run()

        exitCode