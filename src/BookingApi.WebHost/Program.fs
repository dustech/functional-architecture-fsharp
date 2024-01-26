namespace Dustech.BookingApi.WebHost
#nowarn "20"
open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Dustech.BookingApi.Infrastructure
module Program =
    let exitCode = 0
    
    type HttpRouteDefaults = {Controller:string; Id :obj}
    
    
    [<EntryPoint>]
    let main args =

        let builder = WebApplication.CreateBuilder(args)
        
        builder.Services.AddControllers() |> ignore
        
        
        
        let app = builder.Build()
        
        Console.WriteLine($"{Environment.NewLine} {app.Environment.EnvironmentName} Environment {Environment.NewLine}")
        
        
        app.UseHttpsRedirection()
                
        Configure app
        
        
        app.Run()

        exitCode