import itertools
from tqdm import tqdm
import numpy as np
import pandas as pd
from numba import njit, prange

def get_random_w(n_assets):
    return np.random.dirichlet(np.ones(n_assets))


# SEQUENTIAL FUNCTIONS -----------------------------------------------------------

def get_sharpe(assets, w, data):
    returns = data.loc[:, assets] 
    weighted_rets = returns @ w   
    mean_daily_rets = weighted_rets.mean()
    std_daily_rets = weighted_rets.std()
    annualized_ret = mean_daily_rets * 252
    annualized_std = std_daily_rets * np.sqrt(252)
    return annualized_ret / annualized_std

def sequentialSharpe(returns, n_assets, n_w):
    best = ([], 0, 0)  
    max_sharpe = -np.inf
    asset_combinations = list(itertools.combinations(returns.columns, n_assets))
    for combo in tqdm(asset_combinations):
        for _ in range(n_w):
            w = get_random_w(n_assets)
            sharpe = get_sharpe(combo, w, returns)
            if sharpe > max_sharpe:
                max_sharpe = sharpe
                best = (combo, w, sharpe)
    return best


# PARALLEL FUNCTIONS -----------------------------------------------------------------------

@njit(parallel=True)
def batch_sharpe(returns, all_weights):
    n_weights = all_weights.shape[0]
    sharpes = np.empty(n_weights)
    for i in prange(n_weights):
        w = all_weights[i]
        weighted_returns = returns @ w
        mean_ret = weighted_returns.mean()
        std_ret = np.sqrt(np.mean((weighted_returns - mean_ret) ** 2))
        annualized_ret = mean_ret * 252
        annualized_std = std_ret * np.sqrt(252)
        sharpes[i] = annualized_ret / annualized_std
    return sharpes

def parallelSharpe(returns, n_assets, n_w):
    best = ([], 0, 0)  
    max_sharpe = -np.inf
    asset_combinations = list(itertools.combinations(returns.columns, n_assets))
    returns_np = returns.to_numpy()
    for combo in tqdm(asset_combinations):
        idxs = [returns.columns.get_loc(col) for col in combo]
        sub_returns = returns_np[:, idxs]  
        # pre fetching 
        weights = np.random.dirichlet(np.ones(n_assets), size=n_w)
        sharpes = batch_sharpe(sub_returns, weights)
        max_idx = np.argmax(sharpes)
        if sharpes[max_idx] > max_sharpe:
            max_sharpe = sharpes[max_idx]
            best = (combo, weights[max_idx], sharpes[max_idx])
    return best