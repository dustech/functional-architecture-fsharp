open System // for Console
open System.IO // for DirectoryInfo
open System.Text.Json // for JsonSerializer

type Envelope<'T> =
    { Id: Guid
      Created: DateTimeOffset
      Item: 'T }

type Reservation =
    { Date: DateTime
      Name: string
      Email: string
      Quantity: int }

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

let myRes =
    { Reservation.Date = DateTime.Now
      Name = "Omar Khan"
      Email = "omar@example.com"
      Quantity = 2 }

let myEnv =
    { Id = Guid.NewGuid()
      Created = DateTimeOffset.Now
      Item = myRes }


let pwd = DirectoryInfo(Directory.GetCurrentDirectory()) 