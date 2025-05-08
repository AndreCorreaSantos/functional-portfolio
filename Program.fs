module Program

open System.IO
open Parser
open Logic
open System

[<EntryPoint>]
let main _ =
    let filePath = "dow_returns_2024_h2.csv"
    
    if File.Exists(filePath) then
        let csvContent = File.ReadAllText(filePath)
        match parseAssetPrices csvContent with
        | Ok priceMap ->
            let allAssets = priceMap |> Map.toList |> List.map fst
            let rfr = 0.0

            let combinations = getCombinations allAssets 25
            let mutable bestSharpe = Double.MinValue
            let mutable bestAssets = []
            let mutable bestWeights = [||]

            for combo in combinations do
                for _ in 1 .. 1000 do
                    let weights = getRandomWeights combo.Length
                    let sharpe = getSharpe combo weights rfr priceMap
                    if sharpe > bestSharpe then
                        bestSharpe <- sharpe
                        bestAssets <- combo
                        bestWeights <- weights

            printfn "Best Sharpe Ratio: %f" bestSharpe
            printfn "Assets: %A" bestAssets
            printfn "Weights: %A" bestWeights

            0

        | Error e ->
            printfn "Error parsing CSV: %s" e
            1
    else
        printfn "File not found at '%s'." filePath
        1
