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
            var headers = ((IDictionary<string, object>)records.First()).Keys.ToList();

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
                        dataMap[header].Add(0);
                    }
                }
            }

            return dataMap;
        }
    }

    static void Main(string[] args)
    {
        string filePath = "dow_returns_2024_h2.csv";
        var returns = readCsv(filePath);
        var assetNames = new List<string>(returns.Keys);
        int numberAssets = 6;

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

        var portfolio = Core.getBestSharpeSeq(fsharpAssetNames, fsharpReturns, numberAssets);

        stopwatch.Stop();

        Console.WriteLine($"\nExecution Time: {stopwatch.Elapsed.TotalSeconds:F3} seconds\n");

        var assets = portfolio.Assets.ToList();
        var weights = portfolio.Weights.ToList();
        var sharpeValue = portfolio.Sharpe;

        Console.WriteLine("Best Portfolio:");
        for (int i = 0; i < assets.Count; i++)
        {
            Console.WriteLine($"  {assets[i]}: {weights[i]:F4}");
        }

        Console.WriteLine($"Sharpe Ratio: {sharpeValue:F4}");
    }
}
