namespace BookingApi.WebHost
#nowarn "20"
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.AspNetCore.Mvc.Controllers
open Microsoft.AspNetCore.Mvc


type CompositionRoot() =
    let seatingCapacity = 1
    
    interface IControllerActivator with
        member this.Create(context: ControllerContext): obj = 
            let myType = context.ActionDescriptor.ControllerTypeInfo
            1 // TODO finire di implementare il custom activator ASPNET CORE
        member this.Release(context: ControllerContext, controller: obj): unit = 
            failwith "Not Implemented"
        member this.ReleaseAsync(context: ControllerContext, controller: obj): System.Threading.Tasks.ValueTask = 
            failwith "Not Implemented" 




module Program =
    open Microsoft.AspNetCore.Mvc.Controllers
    let exitCode = 0

    [<EntryPoint>]
    let main args =

        let builder = WebApplication.CreateBuilder(args)


        
        //provo ad andare in override con un custom activator
        builder.Services.AddControllers()
        //builder.Services.AddSingleton<IControllerActivator>
        //builder.Services.AddEndpointsApiExplorer()
        builder.Services.AddSwaggerGen()


        let app = builder.Build()        
        printf "IsDevelopment = %b\n\n" <| app.Environment.IsDevelopment()

        if app.Environment.IsDevelopment() then
            app.UseSwagger() |> ignore
            app.UseSwaggerUI() |> ignore


        app.UseAuthorization()
        
        app.MapControllers()
        
        app.Run()

        exitCode
