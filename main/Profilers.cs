using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CoreLib;

namespace Main
{
    public static class Profiler
    {
        public static void SequentialProfile(Func<ReturnsData, int, int, Portfolio> func, ReturnsData returnsData, int numberAssets, int numberW, string outputDir, string prefix)
        {
            Directory.CreateDirectory(outputDir);

            for (int i = 1; i <= 5; i++)
            {
                var stopwatch = Stopwatch.StartNew();

                Portfolio portfolio = func(returnsData, numberAssets, numberW);

                stopwatch.Stop();

                string outputCsvPath = Path.Combine(outputDir, $"{prefix}_{i}.csv");
                DataLoad.WritePortfolioToCsv(
                    outputCsvPath,
                    portfolio.Assets.ToList(),
                    portfolio.Weights.ToList(),
                    portfolio.Sharpe,
                    stopwatch.Elapsed.TotalSeconds
                );

                Console.WriteLine($"{prefix} Run {i}: Portfolio written to {outputCsvPath}");
                Console.WriteLine($"{prefix} Run {i}: Execution Time: {stopwatch.Elapsed.TotalSeconds:F3} seconds");
            }
        }

        public static void ParallelProfile(Func<ReturnsData, int, int, Portfolio> func, ReturnsData returnsData, int numberAssets, int numberW, string outputDir, string prefix)
        {
            Directory.CreateDirectory(outputDir);

            Parallel.For(1, 6, i =>
            {
                var stopwatch = Stopwatch.StartNew();

                Portfolio portfolio = func(returnsData, numberAssets, numberW);

                stopwatch.Stop();

                string outputCsvPath = Path.Combine(outputDir, $"{prefix}_{i}.csv");
                DataLoad.WritePortfolioToCsv(
                    outputCsvPath,
                    portfolio.Assets.ToList(),
                    portfolio.Weights.ToList(),
                    portfolio.Sharpe,
                    stopwatch.Elapsed.TotalSeconds
                );

                Console.WriteLine($"{prefix} Run {i}: Portfolio written to {outputCsvPath}");
                Console.WriteLine($"{prefix} Run {i}: Execution Time: {stopwatch.Elapsed.TotalSeconds:F3} seconds");
            });
        }
    }
}
