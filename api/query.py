import requests
import pandas as pd

# API endpoint URL
API_URL = "http://127.0.0.1:8000/returns"

# Define the time range for the second half of 2024
payload = {
    "start": "2024-08-01",
    "end": "2024-12-31"
}

try:
    # Send POST request to the API
    response = requests.post(API_URL, json=payload)
    response.raise_for_status()  # Raise an error for bad status codes

    # Parse the JSON response
    data = response.json()
    dates = data["dates"]
    returns = data["returns"]

    # Create a DataFrame with dates as the index and stocks as columns
    df = pd.DataFrame(returns, index=dates)

    # Convert index to datetime for cleaner CSV formatting
    df.index = pd.to_datetime(df.index)

    # Save the DataFrame to a CSV file
    output_file = "dow_returns_2024_h2.csv"
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