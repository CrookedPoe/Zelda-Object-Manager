using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Security.Cryptography;
using System.IO;

namespace ZeldaObjectManager
{
    class Utility
    {
        public static dynamic ByteConcat(bool signed, params byte[] b)
        {
            dynamic output = 0;
            int shift = (b.Length * 8);

            for (int i = 0; i < b.Length; i++)
            {
                output |= (b[i] << (shift -= 8));
            }

            if (signed)
            {
                switch (b.Length)
                {
                    case 2:
                        return (Int16)output;
                    case 3:
                        return (Int32)output;
                    case 4:
                        return (Int32)output;
                    case 5:
                        return (Int64)output;
                    case 6:
                        return (Int64)output;
                    case 7:
                        return (Int64)output;
                    case 8:
                        return (Int64)output;
                }
            }
            else if (!signed)
            {
                switch (b.Length)
                {
                    case 2:
                        return (UInt16)output;
                    case 3:
                        return (UInt32)output;
                    case 4:
                        return (UInt32)output;
                    case 5:
                        return (UInt64)output;
                    case 6:
                        return (UInt64)output;
                    case 7:
                        return (UInt64)output;
                    case 8:
                        return (UInt64)output;
                }
            }

            return 0;
        }
        public static byte[] ByteCopy(byte[] src, int start, int size)
        {
            byte[] output = new byte[size];
            for (int i = 0; i < size; i++)
            {
                output[i] = src[i + start];
            }
            return output;
        }
        public static string ByteMD5(byte[] input)
        {
            MD5 md5 = MD5.Create();
            byte[] hashBytes = md5.ComputeHash(input);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
                sb.Append(hashBytes[i].ToString("X2"));

            return sb.ToString();
        }
    }
}
