using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using System.Linq;
using Microsoft.FSharp.Collections;
using CoreLib;

namespace Main
{
    public static class DataLoad
    {
        public static Dictionary<string, List<float>> ReadCsv(string filePath)
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
                            dataMap[header].Add(0); 
                        }
                    }
                }

                return dataMap;
            }
        }

        public static void WritePortfolioToCsv(string filePath, List<string> assetNames, List<double> weights, double sharpeRatio, double executionTime)
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

        public static ReturnsData ConvertToReturnsData(Dictionary<string, List<float>> rawReturns)
        {
            var fsharpAssetNames = ListModule.OfSeq(rawReturns.Keys);

            var fsharpReturns = MapModule.OfSeq(
                rawReturns.Select(kvp =>
                    new Tuple<string, FSharpList<double>>(
                        kvp.Key,
                        ListModule.OfSeq(kvp.Value.Select(v => (double)v)) 
                    )
                )
            );

            return new ReturnsData(fsharpAssetNames, fsharpReturns);
        }
    }
}
