import requests
import pandas as pd

# API endpoint URL
API_URL = "http://127.0.0.1:8000/returns"

# time range for the second half of 2024
payload = {
    "start": "2024-08-01",
    "end": "2024-12-31"
}

try:
    response = requests.post(API_URL, json=payload)
    response.raise_for_status()  

    data = response.json()
    dates = data["dates"]
    returns = data["returns"]

    df = pd.DataFrame(returns, index=dates)

    df.index = pd.to_datetime(df.index)

    output_file = "../dow_returns_2024_h2.csv"
    df.to_csv(output_file, index=True, index_label="Date")

    print(f"CSV file saved as {output_file}")

except requests.exceptions.HTTPError as http_err:
    print(f"HTTP error occurred: {http_err}")
except requests.exceptions.RequestException as req_err:
    print(f"Error fetching data from API: {req_err}")
except KeyError as key_err:
    print(f"Error processing response data: Missing key {key_err}")
except Exception as err:
    print(f"An unexpected error occurred: {err}")