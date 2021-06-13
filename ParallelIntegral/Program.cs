using System;
using System.Diagnostics;
using System.Threading;

namespace ParallelIntegral
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            // View the function https://tinyurl.com/EgopFunction

            var solver = new Solver(
                equation: x => Math.Sin(Math.Sqrt(x)) * Math.Pow(Math.E, Math.Sqrt(x)) / Math.Sqrt(x),
                left: 0,
                right: 9,
                precision: 0.0000001
            );

            var stopwatchParallel = Stopwatch.StartNew();
            solver.SolveParallel();

            Console.WriteLine($"Parallel: Result {solver.Result} in {stopwatchParallel.ElapsedMilliseconds} ms");

            var stopwatchSync = Stopwatch.StartNew();
            var syncResult = solver.SolveSync();
            Console.WriteLine($"Sync: Result {syncResult} in {stopwatchSync.ElapsedMilliseconds} ms");

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}