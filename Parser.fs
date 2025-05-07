module Parser

open FSharp.Data
open System

let parseFloat (str: string) =
    match Double.TryParse(str) with
    | true, value -> Ok value
    | false, _ -> Error "Invalid float"

let parseAssetPrices (csvStr: string) =
    let csv = CsvFile.Parse(csvStr, hasHeaders = true)
    let headers = csv.Headers |> Option.defaultValue [||] |> Array.skip 1
    let rows = csv.Rows

    let initMap =
        headers
        |> Array.fold (fun map header -> Map.add header [] map) Map.empty

    let processRow (map: Map<string, float list>) (row: CsvRow) =
        headers
        |> Array.mapi (fun i header ->
            let value = row.[i + 1]
            match parseFloat value with
            | Ok price -> Ok (header, price)
            | Error e -> Error e)
        |> Array.fold
            (fun acc result ->
                match acc, result with
                | Ok map, Ok (header, price) ->
                    Ok (Map.change header (Option.map (fun lst -> price :: lst)) map)
                | Error e, _ | _, Error e -> Error e)
            (Ok map)

    let finalMap =
        rows
        |> Seq.fold
            (fun acc row ->
                match acc with
                | Ok map -> processRow map row
                | Error e -> Error e)
            (Ok initMap)

    match finalMap with
    | Ok map -> Ok (Map.map (fun _ lst -> List.rev lst) map)
    | Error e -> Error e
