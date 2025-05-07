module Logic

open System
open System.Collections.Generic

let getSharpe (assets: string list) (weights: float[]) (rfr: float) (data: Map<string, float list>) =
    let assetReturns =
        assets
        |> List.map (fun asset ->
            match Map.tryFind asset data with
            | Some prices -> prices
            | None -> failwithf "Asset %s not found in data" asset)

    let dailyReturns =
        assetReturns
        |> List.map List.toArray
        |> List.toArray
        |> Array.transpose
        |> Array.map (fun dailyRow ->
            Array.zip dailyRow weights
            |> Array.sumBy (fun (ret, w) -> ret * w))

    let mean = dailyReturns |> Array.average
    let stdDev =
        dailyReturns
        |> Array.map (fun x -> (x - mean) ** 2.0)
        |> Array.average
        |> sqrt

    let annualizedReturn = mean * 252.0
    let annualizedStd = stdDev * sqrt 252.0

    (annualizedReturn - rfr) / annualizedStd
