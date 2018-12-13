/*
using System.IO;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Bacanoik
{
    public class AzureStorage : IBackupStorage
    {
        private string _storageConnectionString;

        public AzureStorage(string storageConnectionString)
        {
            _storageConnectionString = storageConnectionString;
        }

        public IProgressReporter ProgressReporter { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public void Download(string encryptedName, Stream targetStream)
        {
            GetCloudBlockBlob(encryptedName).DownloadToStreamAsync(targetStream).GetAwaiter().GetResult();
        }
        public void Upload(string encryptedName, Stream encryptedStream)
        {
            GetCloudBlockBlob(encryptedName).UploadFromStreamAsync(encryptedStream).GetAwaiter().GetResult();
        }

        private CloudBlockBlob GetCloudBlockBlob(string encryptedName)
        {
            var storageAccount = CloudStorageAccount.Parse(_storageConnectionString);
            var cloudBlobClient = storageAccount.CreateCloudBlobClient();
            var cloudBlobContainer = cloudBlobClient.GetContainerReference("Storage");

            cloudBlobContainer.CreateIfNotExistsAsync().GetAwaiter().GetResult();
            return cloudBlobContainer.GetBlockBlobReference(encryptedName);
        }
    }
}
*/