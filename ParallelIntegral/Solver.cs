using System;
using System.Threading;

namespace ParallelIntegral
{
    public class Solver
    {
        private readonly Func<double, double> _equation;

        private readonly double _left;
        private readonly double _right;
        private readonly double _precision;

        private readonly Semaphore _semaphore;

        private double _result;

        public double Result
        {
            get { return _result; }
        }

        private volatile uint _activeThreads;

        public Solver(Func<double, double> equation, double left, double right, double precision)
        {
            _equation = equation;
            _semaphore = new Semaphore(Environment.ProcessorCount, Environment.ProcessorCount);
            _left = left;
            _right = right;
            _precision = precision;

            _activeThreads = 0;
        }

        public bool HasFinished => _activeThreads == 0;

        public void Solve()
        {
            var thread = new Thread(SolveRoutine);
            thread.Start(new object[] {_left, _right});
        }

        private void SolveRoutine(object args)
        {
            _semaphore.WaitOne();
            Interlocked.Increment(ref _activeThreads);
            var left = (double)((object[])args)[0];
            var right = (double)((object[])args)[1];

            // Some debugging
            // Console.WriteLine($"Processing: {left.ToString("0.0000")}..{right.ToString("0.0000")}");

            var distance = right - left;

            var half = distance / 2;
            var center = left + half;

            if (distance < _precision)
            {
                // Change parameter to 'left', 'right' or 'center' for three offered methods of calculation
                _result += _equation(center) * distance;
            }
            else
            {
                var threadLeft = new Thread(SolveRoutine);
                threadLeft.Start(new object[] {left, center});

                var threadRight = new Thread(SolveRoutine);
                threadRight.Start(new object[] {center, right});
            }

            Interlocked.Decrement(ref _activeThreads);
            _semaphore.Release();
        }
    }
}