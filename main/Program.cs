using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CoreLib;

namespace Main
{
    class Program
    {
        static void Main(string[] args)
        {
            string filePath = "dow_returns_2024_h2.csv";

            // Read CSV data using DataLoad.ReadCsv
            var rawReturns = DataLoad.ReadCsv(filePath);

            // Convert to F#-compatible ReturnsData using the function in DataLoad
            ReturnsData returnsData = DataLoad.ConvertToReturnsData(rawReturns);

            int numberAssets = 27;
            int numberW = 1000;

            var stopwatch = Stopwatch.StartNew();

            // Calling the F# function with the returnsData
            Portfolio portfolio = Core.getBestSharpeSeq(returnsData.AssetNames,returnsData.Returns,numberAssets,numberW);

            stopwatch.Stop();

            // Write the portfolio information to a CSV file, including execution time
            string outputCsvPath = "portfolio_output.csv";
            DataLoad.WritePortfolioToCsv(
                outputCsvPath,
                portfolio.Assets.ToList(),
                portfolio.Weights.ToList(),
                portfolio.Sharpe,
                stopwatch.Elapsed.TotalSeconds
            );

            Console.WriteLine($"Portfolio written to {outputCsvPath}");
            Console.WriteLine($"Execution Time: {stopwatch.Elapsed.TotalSeconds:F3} seconds");
        }
    }
}
