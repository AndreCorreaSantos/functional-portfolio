module Program

open System
open System.IO
open System.Threading
open System.Threading.Tasks
open Parser
open Logic

/// número máximo de combinações processadas em paralelo
let maxDegreeOfParallelism = 8

/// gera pesos aleatórios normalizados
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
            let rfr = 0.0
            let combinations = getCombinations allAssets 25

            let semaphore = new SemaphoreSlim(maxDegreeOfParallelism)
            let bestLock = obj()
            let mutable bestSharpe = Double.MinValue
            let mutable bestAssets = []
            let mutable bestWeights = [||]

            let tasks =
                combinations
                |> List.map (fun combo ->
                    async {
                        do! semaphore.WaitAsync() |> Async.AwaitTask
                        try
                            let mutable localBestSharpe = Double.MinValue
                            let mutable localBestWeights = [||]

                            let innerTasks =
                                [| for _ in 1 .. 1000 ->
                                    Task.Run(fun () ->
                                        let weights = getRandomWeights combo.Length
                                        let sharpe = getSharpe combo weights rfr priceMap
                                        (weights, sharpe))
                                |]

                            let! results = innerTasks |> Task.WhenAll |> Async.AwaitTask
                            for (weights, sharpe) in results do
                                if sharpe > localBestSharpe then
                                    localBestSharpe <- sharpe
                                    localBestWeights <- weights

                            lock bestLock (fun () ->
                                if localBestSharpe > bestSharpe then
                                    bestSharpe <- localBestSharpe
                                    bestAssets <- combo
                                    bestWeights <- localBestWeights
                            )
                        finally
                            semaphore.Release() |> ignore
                    })

            tasks
            |> Async.Parallel
            |> Async.RunSynchronously
            |> ignore

            printfn "✅ Best Sharpe Ratio: %f" bestSharpe
            printfn "Assets: %A" bestAssets
            printfn "Weights: %A" bestWeights
            0

        | Error e ->
            printfn "❌ Error parsing CSV: %s" e
            1
    else
        printfn "❌ File not found at '%s'." filePath
        1
