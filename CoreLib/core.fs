namespace CoreLib
module Core = 

    open System
    open System.Collections.Generic
    open System.Linq

    type Portfolio = {
        Assets: string list
        Weights: float list
        Sharpe: float
    }

    
    let getSharpe (assets: string list) (weights: float list) (data: Map<string, float list>) =
        let assetReturns =
            assets
            |> List.map (fun asset -> Map.find asset data)

        // recursive function to transpose a list of lists
        let rec transpose matrix =
            match List.filter (fun row -> row <> []) matrix with
            | [] -> []
            | rows -> List.map List.head rows :: transpose (List.map List.tail rows)
        // understand better why we have to transpose the matrix here   
        let dailyReturns =
            assetReturns
            |> transpose
            |> List.map (fun dailyRow ->
                List.zip dailyRow weights
                |> List.sumBy (fun (ret, w) -> ret * w))

        let mean = List.average dailyReturns

        let stdDev =
            dailyReturns
            |> List.map (fun x -> (x - mean) ** 2.0)
            |> List.average
            |> sqrt

        let annualizedReturn = mean * 252.0
        let annualizedStd = stdDev * sqrt 252.0

        annualizedReturn / annualizedStd


    // wrapper to inner recursive function
    let getCombinations (assets: string list) (n: int) : string list list =
        //  recursive function to generate combinations -> sort of backtracking that generates all combinations without their permutations
        // k is the number of elements to choose from
        let rec comb k list =
            match k, list with
            | 0, _ -> [ [] ]
            | _, [] -> []
            | k, x::xs ->
                let withX = comb (k - 1) xs |> List.map (fun tail -> x :: tail)
                let withoutX = comb k xs
                withX @ withoutX
        comb n assets

    let getRandomWeights (n: int) : float list =
        let rnd = Random()
        let rawWeights = [for _ in 1 .. n -> rnd.NextDouble()]
        let total = rawWeights |> List.sum
        rawWeights |> List.map (fun w -> w / total)


    let getBestSharpeSeq (assets: string list) (returns : Map<string, float list>) (n : int) : Portfolio = 
        // recursive function to iterate over all combinations of assets and return the best sharpe
        let rec getBestSharpe (assets: string list) (combinations : string list list) (n : int) (best: Portfolio) : Portfolio  =
            match combinations with
            | [] -> best
            | combination::rest -> 
                let w = getRandomWeights n // for now only one weight per combination COME BACK LATER AND TEST 1000 RANDOM W PER COMBINATION
                let sharpe = getSharpe combination w returns
                if sharpe > best.Sharpe then
                    let newBest = { Assets = combination; Weights = w; Sharpe = sharpe }
                    getBestSharpe assets rest n newBest
                else
                    getBestSharpe assets rest n best
                
        let combinations = getCombinations assets n

        let initialPortfolio = {
            Assets = []
            Weights = []
            Sharpe = System.Double.NegativeInfinity
        }

        getBestSharpe assets combinations n initialPortfolio
