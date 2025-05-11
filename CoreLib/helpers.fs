namespace CoreLib
module Helpers =
    open System
    open System.Threading

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

    // thread safe random array generator
    let rnd = System.Threading.ThreadLocal(fun () -> new Random())


    let getRandomWeights (n: int) : float[] =
        let rawWeights = Array.init n (fun _ -> rnd.Value.NextDouble())
        let total = Array.sum rawWeights
        rawWeights |> Array.map (fun w -> w / total)


    let transpose (matrix: 'a[][]) : 'a[][] =
        let rowCount = matrix.Length
        let colCount = matrix.[0].Length
        Array.init colCount (fun j ->
            Array.init rowCount (fun i -> matrix.[i].[j]))

