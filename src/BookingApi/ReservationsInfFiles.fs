namespace Dustech.BookingApi.DomainModel

open System // for DateTime
open System.IO // for DirectoryInfo
open System.Text.Json // for JsonSerializer
open Dustech.BookingApi.DomainModel.Notifications
open Dustech.BookingApi.Messages // for Envelop, Reservation, Notification
open Dustech.BookingApi.DomainModel // for Dates
open Dustech.BookingApi.DomainModel.Reservations // for IReservations

module StorageInFiles =

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

        member this.Add = this.Write

        interface IReservations with
            member this.Between min max =
                Dates.InitInfinite min
                |> Seq.takeWhile (fun d -> d <= max)
                |> Seq.map getContainingDirectory
                |> Seq.collect (fun dir -> DirectoryInfo(dir) |> getJsonFiles)
                |> Seq.map toReservation

            member this.GetEnumerator() =
                directory
                |> getJsonFiles
                |> Seq.map toReservation
                |> toEnumerator

            member this.GetEnumerator() =
                (this :> seq<Envelope<Reservation>>)
                    .GetEnumerator()
                :> System.Collections.IEnumerator

    type NotificationsInFiles(directory: DirectoryInfo) =
        let toNotification (f: FileInfo) =
            let json = File.ReadAllText f.FullName
            JsonSerializer.Deserialize<Envelope<Notification>>(json)

        let toEnumerator (s: seq<'a>) = s.GetEnumerator()

        let getContainingDirectory id =
            Path.Combine(directory.FullName, id.ToString())

        let appendPath p2 p1 = Path.Combine(p1, p2)

        let getJsonFiles (dir: DirectoryInfo) =
            if Directory.Exists dir.FullName then
                dir.EnumerateFiles("*.json", SearchOption.AllDirectories)
            else
                Seq.empty<FileInfo>

        let toJson (r: Envelope<Notification>) =
            let json = JsonSerializer.Serialize(r)
            json

        member this.Write(notification: Envelope<Notification>) =
            let withExtension extension path = Path.ChangeExtension(path, extension)
            let directoryName = notification.Item.About |> getContainingDirectory

            let fileName =
                directoryName
                |> appendPath (notification.Id.ToString())
                |> withExtension "json"

            let json = notification |> toJson

            Directory.CreateDirectory directoryName |> ignore
            File.WriteAllText(fileName, json)

        member this.Add = this.Write

        interface INotifications with
            member this.About id =
                id
                |> getContainingDirectory
                |> (fun dir -> DirectoryInfo(dir))
                |> getJsonFiles
                |> Seq.map toNotification

            member this.GetEnumerator() =
                directory
                |> getJsonFiles
                |> Seq.map toNotification
                |> toEnumerator

            member this.GetEnumerator() =
                (this :> seq<Envelope<Notification>>)
                    .GetEnumerator()
                :> System.Collections.IEnumerator
