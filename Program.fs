module Program

open System.IO
open Parser
open Logic
open System

/// returns normalized random weights array 
let getRandomWeights n =
    let rng = Random()
    let raw = Array.init n (fun _ -> rng.NextDouble())
    let total = Array.sum raw
    raw |> Array.map (fun x -> x / total)

[<EntryPoint>]
let main _ =
    let filePath = "dow_returns_2024_h2.csv"
    
    if File.Exists(filePath) then
        let csvContent = File.ReadAllText(filePath)
        match parseAssetPrices csvContent with
        | Ok priceMap ->
            let allAssets = priceMap |> Map.toList |> List.map fst
            let selectedAssets = allAssets |> List.take 25
            let weights = getRandomWeights selectedAssets.Length
            let rfr = 0.0

            let sharpe = getSharpe selectedAssets weights rfr priceMap

            printfn "Sharpe ratio for selected assets: %f" sharpe
            printfn "Assets: %A" selectedAssets
            printfn "Weights: %A" weights

            0

        | Error e ->
            printfn "Error parsing CSV: %s" e
            1
    else
        printfn "File not found at '%s'." filePath
        1
