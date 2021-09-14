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

        private volatile uint _startedThreads;
        private volatile uint _endedThreads;

        public Solver(Func<double, double> equation, double left, double right, double precision)
        {
            _equation = equation;
            var processorCount = Environment.ProcessorCount;
            _semaphore = new Semaphore(processorCount, processorCount);
            _left = left;
            _right = right;
            _precision = precision;

            _startedThreads = 0;
        }

        public bool HasFinished => _startedThreads == _endedThreads;

        public void SolveParallel()
        {
            var thread = new Thread(() => SolveRoutine(_left, _right));
            Interlocked.Increment(ref _startedThreads);
            thread.Start();
        }

        public double SolveSync()
        {
            double res = 0;
            for (double left = _left; left < _right; left += _precision)
            {
                double right = left + _precision;
                double center = left + (right - left) / 2;
                res += _equation(center) * _precision;
            }

            return res;
        }

        private void SolveRoutine(double left, double right)
        {
            _semaphore.WaitOne();

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
                var threadLeft = new Thread(() => SolveRoutine(left, center));
                Interlocked.Increment(ref _startedThreads);
                threadLeft.Start();

                var threadRight = new Thread(() => SolveRoutine(center, right));
                Interlocked.Increment(ref _startedThreads);
                threadRight.Start();
            }

            Interlocked.Increment(ref _endedThreads);
            _semaphore.Release();
        }
    }
}