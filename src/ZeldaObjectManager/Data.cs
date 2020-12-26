using System;
using System.IO;
using System.Security.Cryptography;

namespace ZeldaObjectManager
{
    public class Data
    {
        public byte[] Bytes;
        public string MD5;

        public Data()
        {
            Bytes = new byte[0];
            MD5 = String.Empty;
        }

        public Data(string file)
        {
            Bytes = File.ReadAllBytes(file);
            MD5 = Bytes.HashMD5();
        }

        public Data(byte[] b)
        {
            Bytes = b;
            MD5 = Bytes.HashMD5();
        }
    }
}
