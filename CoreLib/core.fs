namespace CoreLib

module Core =

    open System
    open System.Collections.Generic

    type Portfolio = {
        Assets: string list
        Weights: float list
        Sharpe: float
    }

    // function to get sharpe for a given set of assets and weights, rfr is not considered as sharpe is being used to compare different portfolios 
    let getSharpe (assets: string list) (weights: float list) (data: Map<string, float list>) =
        let weightsArr = weights |> List.toArray
        let assetReturns =
            assets
            |> List.map (fun asset -> Map.find asset data |> List.toArray)
            |> Array.ofList

        let days = assetReturns.[0].Length
        let dailyReturns = Array.zeroCreate days

        for day = 0 to days - 1 do
            let mutable sum = 0.0
            for asset = 0 to weightsArr.Length - 1 do
                sum <- sum + weightsArr.[asset] * assetReturns.[asset].[day]
            dailyReturns.[day] <- sum

        let mean = Array.average dailyReturns
        let stdDev = dailyReturns |> Array.averageBy (fun x -> (x - mean) ** 2.0) |> sqrt

        let annualizedReturn = mean * 252.0
        let annualizedStd = stdDev * sqrt 252.0

        annualizedReturn / annualizedStd

    // recursive function to transpose a list of lists
    let rec transpose (matrix: float list list) : float list list =
        let matrixArr = matrix |> List.map List.toArray |> Array.ofList
        let rows = matrixArr.Length
        let cols = matrixArr.[0].Length
        [ for j in 0 .. cols - 1 ->
            [ for i in 0 .. rows - 1 -> matrixArr.[i].[j] ] ]

    // wrapper to inner recursive function
    let getCombinations (assets: string list) (n: int) : string list list =
        let rec comb k list =
            match k, list with
            | 0, _ -> [ [] ]
            | _, [] -> []
            | k, x::xs ->
                let withX = comb (k - 1) xs |> List.map (fun tail -> x :: tail)
                let withoutX = comb k xs
                withX @ withoutX
        comb n assets

    // function to generate random normalized weights
    let getRandomWeights (n: int) : float list =
        let rnd = Random()
        let rawWeights = [for _ in 1 .. n -> rnd.NextDouble()]
        let total = rawWeights |> List.sum
        rawWeights |> List.map (fun w -> w / total)

    // recursive function to iterate over all combinations of assets and return the best sharpe
    let getBestSharpeSeq (assets: string list) (returns : Map<string, float list>) (n : int) : Portfolio = 

        let rec getBestSharpe (combinations : string list list) (best: Portfolio) : Portfolio  =
            match combinations with
            | [] -> best
            | combination::rest -> 
                let w = getRandomWeights n
                let sharpe = getSharpe combination w returns
                let newBest =
                    if sharpe > best.Sharpe then
                        { Assets = combination; Weights = w; Sharpe = sharpe }
                    else
                        best
                getBestSharpe rest newBest

        let combinations = getCombinations assets n

        let initialPortfolio = { Assets = []; Weights = []; Sharpe = System.Double.NegativeInfinity }
        getBestSharpe combinations initialPortfolio
