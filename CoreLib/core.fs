namespace CoreLib
module Core = 

    open System
    open System.Collections.Generic
    open System.Linq

    type Portfolio = {
        Assets: string[]
        Weights: float[]
        Sharpe: float
    }


    let transpose (matrix: 'a[][]) : 'a[][] =
        if matrix.Length = 0 then [||]
        else
            let rowCount = matrix.Length
            let colCount = matrix.[0].Length
            Array.init colCount (fun j ->
                Array.init rowCount (fun i -> matrix.[i].[j]))


    let getSharpe (weights: float[]) (transposedReturns: float[][]) =

        // compute daily returns in a single pass
        let dailyReturns = 
            Array.init transposedReturns.Length (fun i ->
                let dailyRow = transposedReturns.[i]
                Array.fold2 (fun acc r w -> acc + r * w) 0.0 dailyRow weights
            )

        // compute mean and standard deviation in one pass
        let sum, sumSq = 
            Array.fold (fun (s, s2) r -> 
                let s' = s + r
                (s', s2 + r * r)) (0.0, 0.0) dailyReturns

        let n = float dailyReturns.Length
        let mean = sum / n
        let variance = (sumSq / n) - (mean * mean)
        let stdDev = sqrt variance

        // Annualize
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

    let rnd = System.Threading.ThreadLocal(fun () -> Random())

    let getRandomWeights (n: int) : float[] =
        let rawWeights = Array.init n (fun _ -> rnd.Value.NextDouble())
        let total = Array.sum rawWeights
        rawWeights |> Array.map (fun w -> w / total)



    let getBestSharpePar (assets: string list) (returns: Map<string, float list>) (nAssets: int) (nWeights: int) : Portfolio =
        let combinations = getCombinations assets nAssets |> List.map List.toArray |> List.toArray

        let initialPortfolio = {
            Assets = [||]
            Weights = [||]
            Sharpe = System.Double.NegativeInfinity
        }

        let portfolios =
            combinations
            |> Array.Parallel.map (fun combination ->
                // Precompute and transpose returns
                let assetReturns = combination |> Array.map (fun asset -> Map.find asset returns |> List.toArray)
                let transposedReturns = transpose assetReturns  

                let bestPortfolio =
                    Array.init nWeights (fun _ ->
                        let weights = getRandomWeights combination.Length
                        let sharpe = getSharpe weights transposedReturns
                        { Assets = combination; Weights = weights; Sharpe = sharpe })
                    |> Array.maxBy (fun p -> p.Sharpe)

                bestPortfolio
            )

        portfolios
        |> Array.fold (fun best p -> if p.Sharpe > best.Sharpe then p else best) initialPortfolio

