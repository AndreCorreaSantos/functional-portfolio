namespace CoreLib
module Core = 

    open System
    open System.Collections.Generic
    open System.Linq
    
    let getSharpe (assets: string list) (weights: float list) (rfr: float) (data: Map<string, float list>) =
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

        (annualizedReturn - rfr) / annualizedStd


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


    let getBestSharpe (assets: string list) (returns : Map<string, float list>) (rfr : float) (n : int) : float = 
        // get all 142k combinations --> skipping for now to test
        // let combinations = getCombinations assets n
        // get 25 first elements of string list
        let combination = assets.[..24]
        let w = getRandomWeights n
        getSharpe combination w rfr returns
