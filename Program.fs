module Program

open System.IO
open Parser

[<EntryPoint>]
let main _ =
    let filePath = "dow_returns_2024_h2.csv"
    
    if File.Exists(filePath) then
        let csvContent = File.ReadAllText(filePath)
        match parseAssetPrices csvContent with
        | Ok map ->
            map |> Map.iter (fun asset prices -> printfn "%s: %A" asset prices)
            0
        | Error e ->
            printfn "Error: %s" e
            1
    else
        printfn "Arquivo '%s' não encontrado." filePath
        1
