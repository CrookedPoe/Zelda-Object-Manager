using System;
using System.IO;
using System.Security.Cryptography;

namespace ZeldaObjectManager
{
    public class Data
    {
        private static readonly string lorem =
            "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua." +
            " Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat." +
            " Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur." +
            " Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";

        public string DummyText
        {
            get { return lorem; }
        }

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
