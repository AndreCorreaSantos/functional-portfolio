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

    let transpose (matrix: 'a list list) : 'a list list =
                if matrix.IsEmpty then []
                else
                    let matrixArr = matrix |> List.map List.toArray |> List.toArray
                    let rowCount = matrixArr.Length
                    let colCount = matrixArr.[0].Length
                    [ for j in 0 .. colCount - 1 ->
                        [ for i in 0 .. rowCount - 1 -> matrixArr.[i].[j] ] ]

    let getSharpe (weights: float list) (transposedReturns: float list list) =

        let dailyReturns =
            transposedReturns
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
        let rec comb (k: int) (list: string list) (current: string list) (acc: string list list) =
            match k, list with
            | 0, _ -> current :: acc
            | _, [] -> acc
            | k, x::xs ->
                if xs.Length + 1 < k then comb k xs current acc
                else
                    let withX = comb (k - 1) xs (x :: current) acc
                    let withoutX = comb k xs current withX
                    withoutX
        comb n assets [] []

    let getRandomWeights (n: int) : float list =
        let rnd = Random()
        let rawWeights = [for _ in 1 .. n -> rnd.NextDouble()]
        let total = rawWeights |> List.sum
        rawWeights |> List.map (fun w -> w / total)


    let getBestSharpePar (assets: string list) (returns: Map<string, float list>) (nAssets: int) (nWeights: int) : Portfolio =
        let combinations = getCombinations assets nAssets |> List.toArray

        let initialPortfolio = {
            Assets = []
            Weights = []
            Sharpe = System.Double.NegativeInfinity
        }

        let portfolios =
            combinations
            // parallelize the computation of combinations
            |> Array.Parallel.map (fun combination ->
                // sequentially compute the best sharpe (over weights)
                let assetReturns = combination|> List.map (fun asset -> Map.find asset returns) // precomputing asset returns for this combination
                let transposedReturns = transpose assetReturns

                let bestPortfolio = 
                    let portfolios = 
                        [| for _ in 1 .. nWeights do
                            let weights = getRandomWeights nAssets
                            let sharpe = getSharpe weights transposedReturns
                            { Assets = combination; Weights = weights; Sharpe = sharpe } |]
                            
                    portfolios |> Array.maxBy (fun p -> p.Sharpe)
                bestPortfolio
            )

        portfolios
        |> Array.fold (fun best p -> if p.Sharpe > best.Sharpe then p else best) initialPortfolio
