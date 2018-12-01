using System;
using System.Linq;
using System.Net.NetworkInformation;

namespace KeyGenerator
{
    class KeyCheck
    {
        public bool CheckKey(string inputString)
        {
            var keyParts = inputString.Split('-');
            Encryptor encryptor = new Encryptor();
            NetworkInterface networkInterface = NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault();
            if (networkInterface == null)
            {
                return false;
            }
            byte[] addressBytes = networkInterface.GetPhysicalAddress().GetAddressBytes();

            DateTime dateTime = DateTime.Now.Date;
            encryptor.salt = BitConverter.GetBytes(dateTime.ToBinary());
            int[] originalKey = addressBytes.Select(encryptor.ByteEncryptor).Select(HelperClass2.ConditionalMultiply).ToArray();
            encryptor.inputKey = keyParts.Select(int.Parse).ToArray();
            return originalKey.Select(encryptor.CheckKeyPart).All(HelperClass2.IfNumberEqualsZero);
        }

        public string GenerateKey()
        {
            NetworkInterface networkInterface = NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault();
            if (networkInterface == null)
            {
                return "";
            }
            byte[] addressBytes = networkInterface.GetPhysicalAddress().GetAddressBytes();
            Encryptor encryptor = new Encryptor();
            DateTime dateTime = DateTime.Now.Date;
            encryptor.salt = BitConverter.GetBytes(dateTime.ToBinary());

            int[] originalKey = addressBytes.Select(encryptor.ByteEncryptor).Select(HelperClass2.ConditionalMultiply).ToArray();

            return string.Join("-", originalKey.Select(x => x.ToString()));
        }
    }
}