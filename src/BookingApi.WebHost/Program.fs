namespace Dustech.BookingApi.WebHost

#nowarn "20"

open System // for Console
open System.Collections.Concurrent // for ConcurrentBag
open System.IO // for DirectoryInfo
open System.Reactive.Subjects // for reservationsSubject

open Microsoft.AspNetCore.Builder // for WebApplication

open Dustech.BookingApi.Messages // for Envelop, Reservation
open Dustech.BookingApi.Infrastructure // for ConfigureBuilder
open Dustech.BookingApi.DomainModel.Reservations // for ToReservations and Handle
open Dustech.BookingApi.DomainModel.StorageInFiles // for ReservationsInFiles, NotificationInFiles

module Program =
    let exitCode = 0

    type HttpRouteDefaults = { Controller: string; Id: obj }

    type Agent<'T> = MailboxProcessor<'T>
    let seatingCapacity = 10

    [<EntryPoint>]
    let main args =

        let builder = WebApplication.CreateBuilder(args)

        // let reservations = ConcurrentBag<Envelope<Reservation>>()

        let dirReservationStore =
            DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "../../ReservationStore"))

        let reservations = ReservationsInFiles(dirReservationStore)


        //let notifications = ConcurrentBag<Envelope<Notification>>()
        
        let dirNotificationStore =
            DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "../../NotificationStore"))
        
        let notifications = NotificationsInFiles(dirNotificationStore)

        let reservationsSubject = new Subject<Envelope<Reservation>>()

        reservationsSubject.Subscribe reservations.Add
        |> ignore

        let notificationsSubject = new Subject<Notification>()

        notificationsSubject
        |> Observable.map EnvelopWithDefaults
        |> Observable.subscribe notifications.Add
        |> ignore

        let agent =
            new Agent<Envelope<MakeReservation>>(fun inbox ->
                let rec loop () =
                    async {
                        let! cmd = inbox.Receive()
                        //let rs = reservations |> ToReservations
                        let handle = Handle seatingCapacity reservations
                        let newReservations = handle cmd

                        match newReservations with
                        | Some r ->
                            reservationsSubject.OnNext r

                            notificationsSubject.OnNext
                                { About = cmd.Id
                                  Type = "Success"
                                  Message =
                                    sprintf
                                        "Your reservation for %s was completed. We look forward to see you."
                                        (cmd.Item.Date.ToString "yyyy.MM.dd") }

                        | None ->
                            notificationsSubject.OnNext
                                { About = cmd.Id
                                  Type = "Failure"
                                  Message =
                                    sprintf
                                        "We regret to inform you that your reservation for %s could not be completed, because we are already fully booked."
                                        (cmd.Item.Date.ToString "yyyy.MM.dd") }

                        Seq.toList reservations |> PrintAll

                        return! loop ()
                    }

                loop ())

        do agent.Start()

        ConfigureServices builder reservations notifications seatingCapacity agent.Post

        let app = builder.Build()

        Console.WriteLine($"{Environment.NewLine} {app.Environment.EnvironmentName} Environment {Environment.NewLine}")


        Configure app


        app.Run()
        reservationsSubject.Dispose()
        exitCode
