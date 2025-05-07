from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
from datetime import date
import yfinance as yf

app = FastAPI(
    title="My Dow API",
    description="API to fetch daily percentage returns for Dow stocks",
    version="1.0.0"
)

DOW_STOCKS = [
    "AAPL", "AMGN", "AXP", "BA", "CAT", "CRM", "CSCO", "CVX", "DIS", "DOW",
    "GS", "HD", "HON", "IBM", "INTC", "JNJ", "JPM", "KO", "MCD", "MMM",
    "MRK", "MSFT", "NKE", "PG", "TRV", "UNH", "V", "VZ", "WBA", "WMT"
]

class TimeRange(BaseModel):
    start: date
    end: date

class DailyReturns(BaseModel):
    returns: dict
    dates: list

@app.post("/returns", response_model=DailyReturns)
async def compute_returns(range: TimeRange):
    try:
        # Fetch stock data for the given time period
        raw_data = yf.download(
            tickers=DOW_STOCKS,
            start=range.start,
            end=range.end,
            group_by='ticker'
        )
        
        results = {"returns": {}, "dates": []}
        # Use the dates from the first stock encountered as the index
        base_dates = None

        for stock in DOW_STOCKS:
            if stock in raw_data:
                close = raw_data[stock]['Close']
                pct_change = close.pct_change().dropna()
                results["returns"][stock] = pct_change.tolist()
                if base_dates is None:
                    base_dates = pct_change.index.date.tolist()
        
        results["dates"] = base_dates if base_dates else []
        return DailyReturns(**results)

    except Exception as err:
        raise HTTPException(status_code=500, detail=str(err))

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)
