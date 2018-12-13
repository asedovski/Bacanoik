using System.IO;

namespace Bacanoik
{
    public interface IBackupStorage
    {
        IProgressReporter ProgressReporter { get; set; }

        void Upload(string encryptedName, Stream encryptedStream);
        void Download(string encryptedName, Stream encryptedStream);
    }
}