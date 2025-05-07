import numpy as np
import matplotlib.pyplot as plt
import seaborn as sns
import pandas as pd
# receives number of days and n_assets and returns 
def mock_data(n_days,n_assets):
    return np.random.rand(n_assets,n_days,)*100.0



all_data = {}
n_days = 252 # 252 dias de pregao
n_assets = 30 # 30 ativos
data = mock_data(n_days,n_assets)
for i,asset in enumerate(data):
    all_data[f'a{i}'] = asset


data_df = pd.DataFrame(all_data)
data_df.to_csv("mock.csv")


