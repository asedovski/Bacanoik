using System.IO;

namespace Bacanoik
{
    public class LocalStorage : IBackupStorage
    {
        private string _baseFolder;

        public IProgressReporter ProgressReporter { get; set; }

        public LocalStorage(string baseFolder)
        {
            _baseFolder = baseFolder;
        }

        public void Download(string encryptedName, Stream targetStream)
        {
            using (FileStream fileStream = new FileStream(Path.Combine(_baseFolder, encryptedName), FileMode.Open))
            {
                //fileStream.CopyTo(targetStream);
                if (ProgressReporter !=null)
                {
                    ProgressReporter.Init(fileStream.Length);
                }
                CopyStreamWithProgressNotification(fileStream, targetStream);
            }
        }

        public void Upload(string encryptedName, Stream encryptedStream)
        {
            using (FileStream fileStream = new FileStream(Path.Combine(_baseFolder, encryptedName), FileMode.Create))
            {
                //encryptedStream.CopyTo(fileStream);
                CopyStreamWithProgressNotification(encryptedStream, fileStream);
            }
        }

        private void CopyStreamWithProgressNotification(Stream source, Stream destination)
        {
            int read;
            byte[] buffer = new byte[1024 * 1024 * 10];// 10 MB

            while ((read = source.Read(buffer, 0, buffer.Length)) > 0)
            {
                destination.Write(buffer, 0, read);
                var localReference = ProgressReporter;
                if (localReference != null)
                    localReference.NextBytesProcessed(read);
            }
        }
    }
}