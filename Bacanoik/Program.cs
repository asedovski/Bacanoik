using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Bacanoik
{
    class Program
    {
        private const int ivSize = 128; // block size is 128-bit
        private const int keySize = 256;

        private const string LocalStorageArgValue = "LOCAL";
        //private const string AzureStorageArgValue = "AZURE";

        private const string EncryptionModeArgValue = "E";
        private const string DecryptionModeArgValue = "D";

        static void Main(string[] args)
        {
            try
            {
                var storage = ValidateArgumentValue(args, 0, new string[] { LocalStorageArgValue/*, AzureStorageArgValue*/ });
                var mode = ValidateArgumentValue(args, 1, new string[] { EncryptionModeArgValue, DecryptionModeArgValue });
                var fileName = ValidateArgumentValue(args, 2);

                if (mode == EncryptionModeArgValue)
                {
                    var password1 = ValidateArgumentValue(args, 3);
                    var password2 = ValidateArgumentValue(args, 4);
                    Encrypt(storage, fileName, password1, password2);
                }
                else if (mode == DecryptionModeArgValue)
                {
                    DecryptFile(storage, fileName);
                }
                    
                else
                    throw new NotSupportedException("Specified mode is not support. Use 'e' - for encryption or 'd' for decryption");

                Console.WriteLine("Done.");
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Error: " + ex.ToString());
            }
        }

        private static void Encrypt(string storage, string decryptedFullFilePath, string password1, string password2)
        {
            var secret = new SecretData();

            // generate a 128-bit salt and IV using a secure PRNG
            byte[] iv1 = new byte[ivSize / 8];
            byte[] salt1 = new byte[ivSize / 8];
            byte[] key1;

            byte[] iv2 = new byte[ivSize / 8];
            byte[] salt2 = new byte[ivSize / 8];
            byte[] key2;

            var randomFileName = new byte[ivSize / 8];

            // generate key`s
            using (var rng = RandomNumberGenerator.Create())
            using (var kd1 = new Rfc2898DeriveBytes(password1, salt1, 10000 + salt1[0] + salt1[1] + salt1[2] + salt1[3]))
            using (var kd2 = new Rfc2898DeriveBytes(password2, salt2, 10000 + salt2[4] + salt2[5] + salt2[6] + salt2[7]))
            {
                rng.GetBytes(iv1);
                rng.GetBytes(salt1);

                rng.GetBytes(iv2);
                rng.GetBytes(salt2);

                rng.GetBytes(randomFileName);

                key1 = kd1.GetBytes(keySize / 8);
                key2 = kd2.GetBytes(keySize / 8);

                //byte[] key1 = Microsoft.AspNetCore.Cryptography.KeyDerivationKeyDerivation.Pbkdf2(password1, salt1, KeyDerivationPrf.HMACSHA512, 10000 + salt1[0] + salt1[1] + salt1[2] + salt1[3], keySize / 8);
                //byte[] key2 = Microsoft.AspNetCore.Cryptography.KeyDerivationKeyDerivation.Pbkdf2(password2, salt2, KeyDerivationPrf.HMACSHA512, 10000 + salt2[4] + salt2[5] + salt2[6] + salt2[7], keySize / 8);
            }

            // prepare
            secret.IV1 = Convert.ToBase64String(iv1);
            secret.Key1 = Convert.ToBase64String(key1);
            secret.IV2 = Convert.ToBase64String(iv2);
            secret.Key2 = Convert.ToBase64String(key2);

            secret.DecryptedFileName = Path.GetFileName(decryptedFullFilePath);
            //secret.EncryptedFileName = Guid.NewGuid().ToString("N"); // may disclouse information about TimeStamp and PC MAC address
            secret.EncryptedFileName = BitConverter.ToString(randomFileName).Replace("-", string.Empty);

            var baseFolder = Path.GetDirectoryName(decryptedFullFilePath);

            HashAlgorithm decryptedHashAlg, encryptedHashAlg;
            GetHashAlgorithms(out decryptedHashAlg, out encryptedHashAlg);

            using (FileStream decryptedFileStream = new FileStream(decryptedFullFilePath, FileMode.Open))
            using (CryptoStream decryptedHashStream = new CryptoStream(decryptedFileStream, decryptedHashAlg, CryptoStreamMode.Read))
            using (CryptoStream encryptor1 = new CryptoStream(decryptedHashStream, new RijndaelManaged { KeySize = keySize }.CreateEncryptor(key1, iv1), CryptoStreamMode.Read))
            using (CryptoStream encryptor2 = new CryptoStream(encryptor1, new RijndaelManaged { KeySize = keySize }.CreateEncryptor(key2, iv2), CryptoStreamMode.Read))
            using (CryptoStream encryptedHashStream = new CryptoStream(encryptor2, encryptedHashAlg, CryptoStreamMode.Read))
            {
                var backupStorage = GetBackupStorage(storage, baseFolder);
                if (backupStorage.ProgressReporter != null)
                    backupStorage.ProgressReporter.Init(decryptedFileStream.Length);
                backupStorage.Upload(secret.EncryptedFileName, encryptedHashStream);
            }

            secret.DecryptedMD5 = Convert.ToBase64String(decryptedHashAlg.Hash);
            secret.EncryptedMD5 = Convert.ToBase64String(encryptedHashAlg.Hash);

            secret.Serialize(decryptedFullFilePath + ".secret");
        }

        private static void DecryptFile(string storage, string fullInfoFilePath)
        {
            // load parameters
            var secret = SecretData.Deserialize(fullInfoFilePath);
            var basePath = Path.GetDirectoryName(fullInfoFilePath);
            var decryptedFullFileName = Path.Combine(basePath, secret.DecryptedFileName);

            byte[] iv1 = Convert.FromBase64String(secret.IV1);
            byte[] key1 = Convert.FromBase64String(secret.Key1);

            byte[] iv2 = Convert.FromBase64String(secret.IV2);
            byte[] key2 = Convert.FromBase64String(secret.Key2);

            HashAlgorithm decryptedHashAlg, encryptedHashAlg;
            GetHashAlgorithms(out decryptedHashAlg, out encryptedHashAlg);

            try
            {
                using (FileStream decryptedFileStream = new FileStream(decryptedFullFileName, FileMode.Create))
                using (CryptoStream decryptedHashStream = new CryptoStream(decryptedFileStream, decryptedHashAlg, CryptoStreamMode.Write))
                using (CryptoStream decryptor1 = new CryptoStream(decryptedHashStream, new RijndaelManaged { KeySize = keySize }.CreateDecryptor(key1, iv1), CryptoStreamMode.Write))
                using (CryptoStream decryptor2 = new CryptoStream(decryptor1, new RijndaelManaged { KeySize = keySize }.CreateDecryptor(key2, iv2), CryptoStreamMode.Write))
                using (CryptoStream encryptedHashStream = new CryptoStream(decryptor2, encryptedHashAlg, CryptoStreamMode.Write))
                {
                    var backupStorage = GetBackupStorage(storage, basePath);
                    backupStorage.Download(secret.EncryptedFileName, encryptedHashStream);
                }

                if (secret.DecryptedMD5 != Convert.ToBase64String(decryptedHashAlg.Hash))
                    throw new InvalidDataException("Decrypted hash mismatch");
                if (secret.EncryptedMD5 != Convert.ToBase64String(encryptedHashAlg.Hash))
                    throw new InvalidDataException("Encrypted hash mismatch");
            }
            catch
            {
                try
                {
                    if (File.Exists(decryptedFullFileName))
                        File.Delete(decryptedFullFileName);
                }
                catch {/* we just trying to cleanup, but probably better processing required...*/ }
                throw;
            }
        }

        private static void GetHashAlgorithms(out HashAlgorithm decryptedHashAlg, out HashAlgorithm encryptedHashAlg)
        {
            decryptedHashAlg = MD5.Create();
            encryptedHashAlg = MD5.Create();// Azure use MD5, so in future I`d like to check if upload were valid
        }

        private static IBackupStorage GetBackupStorage(string storageName, string basePath)
        {
            switch (storageName)
            {
                case LocalStorageArgValue:
                    return new LocalStorage(basePath) { ProgressReporter = new ConsoleProgressReporter()};
                //case AzureStorageArgValue:
                //    return new AzureStorage(ConfigurationManager.AppSettings["AzureConnectionString"]);
                default:
                    throw new NotImplementedException($"Storage type '{storageName}' support is not implemented.");
            }
        }

        private static string ValidateArgumentValue(string[] args, int argNumber, string[] availableValues = null)
        {
            var availableValuesMessage = availableValues == null ? "" : $" Availale values are: {string.Join(" | ", availableValues)}";

            if (argNumber >= args.Length)
                throw new ArgumentException($"Argumnet number '{argNumber}' is not specified.{availableValuesMessage}");

            var argValue = args[argNumber];

            if (availableValues != null && !availableValues.Contains(argValue))
            {
                throw new ArgumentException($"Argument number '{argNumber}' contains invalid value '{argValue}'.{availableValuesMessage}");
            }

            return argValue;
        }
    }
}