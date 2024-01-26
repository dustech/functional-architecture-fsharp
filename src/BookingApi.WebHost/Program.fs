namespace BookingApi.WebHost
#nowarn "20"
open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open BookingApi.Infrastructure

module Program =
    let exitCode = 0
    
    type HttpRouteDefaults = {Controller:string; Id :obj}
    
    [<EntryPoint>]
    let main args =

        let builder = WebApplication.CreateBuilder(args)

        builder.Services.AddControllers()

        let app = builder.Build()
        
        Console.WriteLine($"{Environment.NewLine} {app.Environment.EnvironmentName} Environment {Environment.NewLine}")
        
        
        app.UseHttpsRedirection()
        
        Configure app
        //app.UseAuthorization()
        //app.MapControllers()
        // app.MapControllerRoute(
        //     "DefaultAPI","{controller}/{id}",{Controller = "WeatherForecast"; Id = OptionalRouteConstraint}) |> ignore
        
        app.Run()

        exitCode