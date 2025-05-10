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
        float rfr = 0.0f;
        int numberAssets = 25;

        var fsharpAssetNames = ListModule.OfSeq(assetNames);
        var fsharpWeights = Core.getRandomWeights(numberAssets);
        // var combinations = Core.getCombinations(fsharpAssetNames, numberAssets);
        var selectedAssets = ListModule.OfSeq(assetNames.GetRange(0, 25));
        var fsharpReturns = MapModule.OfSeq(
            returns.Select(kvp =>
                new Tuple<string, FSharpList<double>>(
                    kvp.Key,
                    ListModule.OfSeq(kvp.Value.Select(v => (double)v))
                )
            )
        );

        var sharpe = Core.getBestSharpe(fsharpAssetNames, fsharpReturns, rfr, numberAssets);
        Console.WriteLine($"SHARPE {sharpe}");
    }
}
