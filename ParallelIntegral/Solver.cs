using System;
using System.Linq;
using System.Threading;

namespace ParallelIntegral
{
    public class Solver
    {
        private readonly Func<double, double> _equation;

        private readonly double _left;
        private readonly double _right;
        private readonly double _precision;

        private double _result;

        public double Result
        {
            get { return _result; }
        }

        public Solver(Func<double, double> equation, double left, double right, double precision)
        {
            _equation = equation;
            _left = left;
            _right = right;
            _precision = precision;
        }

        public void SolveParallel()
        {
            var processorCount = Environment.ProcessorCount;
            var distance = _right - _left;
            var distancePerThread = distance / processorCount;
            var threads = new Thread[processorCount];

            double[] results = new double[processorCount];
            
            for (int i = 0; i < processorCount; i++)
            {
                int localI = i;
                var left = _left + distancePerThread * localI;
                var right = _left + distancePerThread * (localI + 1);

                var thread = new Thread(() => SolveRoutine(left, right, out results[localI]));
                thread.Start();
                threads[i] = thread;
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }

            _result = results.Sum();
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

        private void SolveRoutine(double left, double right, out double result)
        {
            var distance = right - left;

            var half = distance / 2;
            var center = left + half;

            if (distance < _precision)
            {
                // Change parameter to 'left', 'right' or 'center' for three offered methods of calculation
                result = _equation(center) * distance;
            }
            else
            {
                SolveRoutine(left, center, out var r1);
                SolveRoutine(center, right, out var r2);
                result = r1 + r2;
            }
        }
    }
}