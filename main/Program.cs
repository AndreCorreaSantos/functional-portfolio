using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using CoreLib;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;

class Program
{
    static Dictionary<string, List<float>> readCsv(string filePath)
    {
        Console.WriteLine("Reading CSV...");
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

            Console.WriteLine("Finished reading CSV.");
            return dataMap;
        }
    }

    static void Main(string[] args)
    {
        Console.WriteLine("Starting program...");

        string filePath = "dow_returns_2024_h2.csv";
        var returns = readCsv(filePath);
        Console.WriteLine("Loaded returns.");

        var assetNames = new List<string>(returns.Keys);
        Console.WriteLine($"Found {assetNames.Count} asset names.");

        int numberAssets = 25;

        Console.WriteLine("Converting asset names to F# list...");
        var fsharpAssetNames = ListModule.OfSeq(assetNames);

        Console.WriteLine("Generating random weights...");
        var fsharpWeights = Core.getRandomWeights(numberAssets);
        Console.WriteLine("Random weights generated.");

        // Console.WriteLine("Getting combinations...");
        // var combinations = Core.getCombinations(fsharpAssetNames, numberAssets);
        // Console.WriteLine("Combinations retrieved.");

        var selectedAssets = ListModule.OfSeq(assetNames.GetRange(0,25));
        Console.WriteLine("Selected assets extracted.");

        Console.WriteLine("Converting returns to F# map...");
        var fsharpReturns = MapModule.OfSeq(
            returns.Select(kvp =>
                new Tuple<string, FSharpList<double>>(
                    kvp.Key,
                    ListModule.OfSeq(kvp.Value.Select(v => (double)v))
                )
            )
        );
        Console.WriteLine("Converted returns to F# map.");

        Console.WriteLine("Calculating Sharpe ratio...");
        var sharpe = Core.getSharpe(selectedAssets, fsharpWeights, 0.0, fsharpReturns);
        Console.WriteLine($"Sharpe ratio: {sharpe}");
    }
}
