namespace CoreLib

open System.Collections.Generic

[<CLIMutable>]
type Portfolio = {
    Assets: string[]
    Weights: float[]
    Sharpe: float
}

type ReturnsData = {
    AssetNames: string list
    Returns: Map<string, float list>
}
