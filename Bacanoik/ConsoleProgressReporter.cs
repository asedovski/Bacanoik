using System;

namespace Bacanoik
{
    public class ConsoleProgressReporter : IProgressReporter
    {
        private long _totalSize;
        private long _processedSize;
        private System.Diagnostics.Stopwatch _stopwatch = null;

        public void Init(long totalSize)
        {
            _processedSize = 0;
            _totalSize = totalSize;
            _stopwatch = System.Diagnostics.Stopwatch.StartNew();
        }

        public void NextBytesProcessed(int nextBytesCount)
        {
            if (_stopwatch == null)
                return;// not initialized...

            _processedSize += nextBytesCount;
            var l = Console.CursorLeft;
            double p = Math.Round((_processedSize / (double)_totalSize) * 100, 2);

            var eta = "unknown";
            if (p > 0)
            {
                var etav = (100 - p) * _stopwatch.ElapsedMilliseconds / (p * 1000 * 60 * 60);
                if (etav > 2)
                {
                    eta = $"{Math.Round(etav, 2)} h.";
                }
                else
                {
                    etav = etav * 60;
                    if (etav > 2)
                    {
                        eta = $"{Math.Round(etav, 2)} min.";
                    }
                    else
                    {
                        etav = etav * 60;
                        eta = $"{Math.Round(etav, 2)} sec.";
                    }
                }
            }

            Console.Write($"{p}% processed... Remaining time: {eta}     ");
            Console.CursorLeft = l;
        }
    }
}
