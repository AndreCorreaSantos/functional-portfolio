using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using CoreLib;
using Microsoft.FSharp.Collections;

class Program
{
    static Dictionary<string, List<float>> readCsv(string filePath)
    {
        using (var reader = new StreamReader(filePath))
        using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null
        }))
        {
            var records = csv.GetRecords<dynamic>().ToList();
            var dataMap = new Dictionary<string, List<float>>();

            // get all headers skipping the first one - date
            var headers = ((IDictionary<string, object>)records.First()).Keys.Skip(1).ToList();

            foreach (var header in headers)
            {
                dataMap[header] = new List<float>();
            }

            foreach (var record in records)
            {
                var recordDict = (IDictionary<string, object>)record;
                foreach (var header in headers)
                {
                    if (float.TryParse(recordDict[header]?.ToString(), out float value))
                    {
                        dataMap[header].Add(value);
                    }
                    else
                    {
                        dataMap[header].Add(0); // or throw/log warning if needed
                    }
                }
            }

            return dataMap;
        }
    }

    static void WritePortfolioToCsv(string filePath, List<string> assetNames, List<double> weights, double sharpeRatio, double executionTime)
    {
        using (var writer = new StreamWriter(filePath))
        using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)))
        {
            // Write header
            csv.WriteField("Asset Name");
            csv.WriteField("Weight");
            csv.WriteField("Sharpe Ratio");
            csv.WriteField("Execution Time (seconds)");
            csv.NextRecord();

            // Write asset names, weights, Sharpe ratio, and execution time
            for (int i = 0; i < assetNames.Count; i++)
            {
                csv.WriteField(assetNames[i]);
                csv.WriteField(weights[i]);
                csv.WriteField(sharpeRatio);
                csv.WriteField(executionTime);
                csv.NextRecord();
            }
        }
    }


    static void Main(string[] args)
    {
        string filePath = "dow_returns_2024_h2.csv";
        var returns = readCsv(filePath);
        var assetNames = new List<string>(returns.Keys);
        int numberAssets = 25;
        int numberW = 20;

        var fsharpAssetNames = ListModule.OfSeq(assetNames);
        var fsharpWeights = Core.getRandomWeights(numberAssets);
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

        var portfolio = Core.getBestSharpePar(fsharpAssetNames, fsharpReturns, numberAssets, numberW);

        stopwatch.Stop();
        
        // Convert portfolio data to lists for writing to CSV
        var assets = portfolio.Assets.ToList();
        var weights = portfolio.Weights.ToList();
        var sharpeValue = portfolio.Sharpe;

        // Write the portfolio information to a CSV file, including execution time
        string outputCsvPath = "portfolio_output.csv";
        WritePortfolioToCsv(outputCsvPath, assets, weights, sharpeValue, stopwatch.Elapsed.TotalSeconds);
        
        Console.WriteLine($"Portfolio written to {outputCsvPath}");
        Console.WriteLine($"Execution Time: {stopwatch.Elapsed.TotalSeconds:F3} seconds");
    }
}
