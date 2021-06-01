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
                precision: 0.01
            );

            // Test
            // var solver = new Solver(x => x * x, left: 1, right: 6, precision: 0.001);

            solver.Solve();

            // Wait a bit, so solver can warm up
            Thread.Sleep(1000);

            int dots = 1;
            while (!solver.HasFinished)
            {
                Console.Clear();
                Console.WriteLine($"Waiting{new string('.', dots)}");
                Thread.Sleep(300);

                dots %= 3;
                dots++;
            }

            Console.WriteLine($"Result: {solver.Result}");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}