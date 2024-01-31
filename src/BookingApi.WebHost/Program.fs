namespace Dustech.BookingApi.WebHost

#nowarn "20"

open System // for Console
open System.Collections.Concurrent // for ConcurrentBag
open System.IO // for DirectoryInfo
open System.Reactive.Subjects // for reservationsSubject
open System.Text.Json // for JsonSerializer
open Microsoft.AspNetCore.Builder // for WebApplication
open Dustech.BookingApi.Messages // for Envelop, Reservation
open Dustech.BookingApi.Infrastructure // for ConfigureBuilder
open Dustech.BookingApi.DomainModel.Reservations // for ToReservations and Handle
open Dustech.BookingApi.DomainModel.Notifications // for ToNotifications

module Program =
    let exitCode = 0

    type ReservationsInFiles(directory: DirectoryInfo) =
        let toReservation (f: FileInfo) =
            let json = File.ReadAllText f.FullName
            JsonSerializer.Deserialize<Envelope<Reservation>>(json)

        let toEnumerator (s: seq<'a>) = s.GetEnumerator()

        let getContainingDirectory (d: DateTime) =
            Path.Combine(directory.FullName, d.Year.ToString(), d.Month.ToString(), d.Day.ToString())

        let appendPath p2 p1 = Path.Combine(p1, p2)

        let getJsonFiles (dir: DirectoryInfo) =
            if Directory.Exists dir.FullName then
                dir.EnumerateFiles("*.json", SearchOption.AllDirectories)
            else
                Seq.empty<FileInfo>

        let toJson (r: Envelope<Reservation>) =
            let json = JsonSerializer.Serialize(r)
            json

        member this.Write(reservation: Envelope<Reservation>) =
            let withExtension extension path = Path.ChangeExtension(path, extension)
            let directoryName = reservation.Item.Date |> getContainingDirectory

            let fileName =
                directoryName
                |> appendPath (reservation.Id.ToString())
                |> withExtension "json"

            let json = reservation |> toJson
            Directory.CreateDirectory directoryName |> ignore
            File.WriteAllText(fileName, json)


    type HttpRouteDefaults = { Controller: string; Id: obj }

    type Agent<'T> = MailboxProcessor<'T>
    let seatingCapacity = 10

    [<EntryPoint>]
    let main args =

        let builder = WebApplication.CreateBuilder(args)

        let reservations = ConcurrentBag<Envelope<Reservation>>()
        let notifications = ConcurrentBag<Envelope<Notification>>()


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
                        let rs = reservations |> ToReservations
                        let handle = Handle seatingCapacity rs
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

                        Seq.toList rs |> PrintAll

                        return! loop ()
                    }

                loop ())

        do agent.Start()

        ConfigureServices
            builder
            (reservations |> ToReservations)
            (notifications |> ToNotifications)
            seatingCapacity
            agent.Post

        let app = builder.Build()

        Console.WriteLine($"{Environment.NewLine} {app.Environment.EnvironmentName} Environment {Environment.NewLine}")


        Configure app


        app.Run()
        reservationsSubject.Dispose()
        exitCode
