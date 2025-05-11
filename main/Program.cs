using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CoreLib;
using System.Threading.Tasks;

namespace Main
{
    class Program
    {

        static void Main(string[] args)
        {
            string filePath = "dow_returns_2024_h2.csv";

            Dictionary<string, List<float>> rawReturns = DataLoad.ReadCsv(filePath);
            ReturnsData returnsData = DataLoad.ConvertToReturnsData(rawReturns);

            int numberAssets = 25;
            int numberW = 2;

            // // parallel profiling getbestsharpeseq
            Profiler.ParallelProfile(Core.getBestSharpeSeq, returnsData, numberAssets, numberW, "log/sequential", "sequential");

            // sequentially profiling getbestsharpepar
            Profiler.SequentialProfile(Core.getBestSharpePar, returnsData, numberAssets, numberW, "log/parallel", "parallel");
        }
    }
}
