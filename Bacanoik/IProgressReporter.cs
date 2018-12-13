namespace Bacanoik
{
    public interface IProgressReporter
    {
        void Init(long totalSize);
        void NextBytesProcessed(int nextBytesCount);
    }
}