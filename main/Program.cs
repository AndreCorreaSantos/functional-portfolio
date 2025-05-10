using System;
using CsvHelper;
using CsvHelper.Configuration;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

class Program
{


    static Dictionary<string, List<float>> readCsv(string filePath){
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

        Dictionary<string,List<float>> returns = readCsv(filePath);

        foreach (var entry in returns)
        {
            Console.WriteLine($"{entry.Key}: {string.Join(", ", entry.Value)}");
        }
    }
}

