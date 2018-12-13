using System.IO;
using System.Xml.Serialization;

namespace Bacanoik
{
    public class SecretData
    {
        public string DecryptedFileName { get; set; }
        public string DecryptedMD5 { get; set; }

        public string IV1 { get; set; }
        public string Key1 { get; set; }

        public string IV2 { get; set; }
        public string Key2 { get; set; }
        
        public string EncryptedFileName { get; set; }
        public string EncryptedMD5 { get; set; }

        public void Serialize(string fullInfoFilePath)
        {
            var xml = new XmlSerializer(typeof(SecretData));
            using (var info = new FileStream(fullInfoFilePath, FileMode.Create))
            {
                xml.Serialize(info, this);
            }
        }

        public static SecretData Deserialize(string fullInfoFilePath)
        {
            var xml = new XmlSerializer(typeof(SecretData));
            using (var info = new FileStream(fullInfoFilePath, FileMode.Open))
            {
                return (SecretData)xml.Deserialize(info);
            }
        }
    }
}
