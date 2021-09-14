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
                precision: 0.001
            );

            var stopwatchParallel = Stopwatch.StartNew();
            solver.SolveParallel();

            int dots = 1;
            while (!solver.HasFinished)
            {
                Console.Clear();
                Console.WriteLine($"Waiting{new string('.', dots)}");
                Thread.Sleep(30);

                dots %= 3;
                dots++;
            }

            Console.WriteLine($"Parallel: Result {solver.Result} in {stopwatchParallel.ElapsedMilliseconds} ms");

            var stopwatchSync = Stopwatch.StartNew();
            var syncResult = solver.SolveSync();
            Console.WriteLine($"Sync: Result {syncResult} in {stopwatchSync.ElapsedMilliseconds} ms");

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}