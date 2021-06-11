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

        private volatile uint _started;
        private volatile uint _finished;

        public Solver(Func<double, double> equation, double left, double right, double precision)
        {
            _equation = equation;
            _semaphore = new Semaphore(Environment.ProcessorCount, Environment.ProcessorCount);
            _left = left;
            _right = right;
            _precision = precision;
        }

        public bool HasFinished => _started == _finished;

        public void SolveParallel()
        {
            var thread = new Thread(SolveRoutine);
            Interlocked.Increment(ref _started);
            thread.Start(new object[] {_left, _right});
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

        private void SolveRoutine(object args)
        {
            _semaphore.WaitOne();
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
                _result += (_equation(left) + _equation(right)) / 2 * distance;
            }
            else
            {
                if (_started < Environment.ProcessorCount)
                {
                    Interlocked.Add(ref _started, 2);
                    var threadLeft = new Thread(SolveRoutine);
                    threadLeft.Start(new object[] {left, center});

                    var threadRight = new Thread(SolveRoutine);
                    threadRight.Start(new object[] {center, right});
                }
                else
                {
                    double res = 0;
                    for (double l = left; l < right; l += _precision)
                    {
                        double r = l + _precision;
                        double c = l + (r - l) / 2;
                        res += _equation(c) * _precision;
                    }

                    _result += res;
                }
            }

            Interlocked.Increment(ref _finished);
            _semaphore.Release();
        }
    }
}