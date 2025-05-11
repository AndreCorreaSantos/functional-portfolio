namespace CoreLib

[<CLIMutable>]
type Portfolio = {
    Assets: string[]
    Weights: float[]
    Sharpe: float
}
