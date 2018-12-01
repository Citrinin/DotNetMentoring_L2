namespace KeyGenerator
{
    public sealed class Encryptor
    {
        public byte[] salt;

        public int[] inputKey;

        internal int ByteEncryptor(byte inputByte, int saltIndex)
        {
            return inputByte ^ salt[saltIndex];
        }

        internal int CheckKeyPart(int keyPart, int keyIndex)
        {
            return keyPart - inputKey[keyIndex];
        }
    }
}