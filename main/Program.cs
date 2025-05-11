using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CoreLib;
using Microsoft.FSharp.Collections;

namespace Main{
    class Program
    {
        static void Main(string[] args)
        {
            string filePath = "dow_returns_2024_h2.csv";
            
            // Read CSV data using DataLoad.ReadCsv
            Dictionary<string,List<float>> returns = DataLoad.ReadCsv(filePath);
            List<string> assetNames = new List<string>(returns.Keys);
            int numberAssets = 27;
            int numberW = 1000;

            var fsharpAssetNames = ListModule.OfSeq(assetNames);
            var selectedAssets = ListModule.OfSeq(assetNames.GetRange(0, numberAssets));
            var fsharpReturns = MapModule.OfSeq(
                returns.Select(kvp =>
                    new Tuple<string, FSharpList<double>>(
                        kvp.Key,
                        ListModule.OfSeq(kvp.Value.Select(v => (double)v))
                    )
                )
            );
        
            var stopwatch = Stopwatch.StartNew(); 

            Portfolio portfolio = Core.getBestSharpeSeq(fsharpAssetNames, fsharpReturns, numberAssets, numberW);

            stopwatch.Stop();
            
            // Convert portfolio data to lists for writing to CSV
            var assets = portfolio.Assets.ToList();
            var weights = portfolio.Weights.ToList();
            var sharpeValue = portfolio.Sharpe;

            // Write the portfolio information to a CSV file, including execution time
            string outputCsvPath = "portfolio_output.csv";
            DataLoad.WritePortfolioToCsv(outputCsvPath, assets, weights, sharpeValue, stopwatch.Elapsed.TotalSeconds);
            
            Console.WriteLine($"Portfolio written to {outputCsvPath}");
            Console.WriteLine($"Execution Time: {stopwatch.Elapsed.TotalSeconds:F3} seconds");
        }
    }
}
