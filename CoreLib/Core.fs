namespace CoreLib
module Core = 

    open System
    open System.Collections.Generic
    open System.Linq
    open CoreLib.Helpers

    

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
    
    let getBestSharpeSeq (returnsData:ReturnsData) (nAssets: int) (nWeights: int) : Portfolio =
            let assets = returnsData.AssetNames
            let returns = returnsData.Returns
            let combinations = getCombinations assets nAssets |> List.map List.toArray |> List.toArray

            let initialPortfolio = {
                Assets = [||]
                Weights = [||]
                Sharpe = System.Double.NegativeInfinity
            }

            let portfolios =
                combinations
                |> Array.map (fun combination ->
                    // precompute and transpose returns
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




    let getBestSharpePar (returnsData:ReturnsData) (nAssets: int) (nWeights: int) : Portfolio =
        let assets = returnsData.AssetNames
        let returns = returnsData.Returns
        let combinations = getCombinations assets nAssets |> List.map List.toArray |> List.toArray

        let initialPortfolio = {
            Assets = [||]
            Weights = [||]
            Sharpe = System.Double.NegativeInfinity
        }

        let portfolios =
            combinations
            |> Array.Parallel.map (fun combination ->
                // precompute and transpose returns
                let assetReturns = combination |> Array.map (fun asset -> Map.find asset returns |> List.toArray)
                let transposedReturns = transpose assetReturns  

                let bestPortfolio =
                    Array.Parallel.init nWeights (fun _ ->
                        let weights = getRandomWeights combination.Length
                        let sharpe = getSharpe weights transposedReturns
                        { Assets = combination; Weights = weights; Sharpe = sharpe })
                    |> Array.maxBy (fun p -> p.Sharpe)

                bestPortfolio
            )

        portfolios
        |> Array.fold (fun best p -> if p.Sharpe > best.Sharpe then p else best) initialPortfolio

