using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace ZeldaObjectManager
{
    public static class Extensions
    {
        // String Extensions
        public static string ParseArgumentFile(this String str)
        {
            string[] split = str.Split('=');
            split[1] = split[1].Replace("\"", String.Empty);

            return split[1];
        }

        // Byte[] Extensions
        public static string HashMD5(this byte[] b)
        {
            StringBuilder sb = new StringBuilder();
            MD5 md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(b);

            for (int i = 0; i < hash.Length; i++)
                sb.Append(hash[i].ToString("X2"));

            return sb.ToString();
        }
        public static byte[] GetSection(this byte[] b, int offset, int size)
        {
            byte[] _b = new byte[size];

            Array.Copy(b, offset, _b, 0, size);

            return _b;
        }
        /*public static UInt64 ReadUInt64(this byte[] b)
        {
            int p = 0;
            return (UInt64)(
                ((b[p++] & 0xFF) << 56) |
                ((b[p++] & 0xFF) << 48) |
                ((b[p++] & 0xFF) << 40) |
                ((b[p++] & 0xFF) << 32) |
                ((b[p++] & 0xFF) << 24) |
                ((b[p++] & 0xFF) << 18) |
                ((b[p++] & 0xFF) << 8) |
                ((b[p++] & 0xFF)));
        }*/

        // DisplayList.Instruction Extensions
        public static gsSPVertex ParseVertexBuffer(this DisplayList.Instruction i)
        {
            return new gsSPVertex(i.ToString());
        }
        public static gsSPMatrix ParseMatrix(this DisplayList.Instruction i)
        {
            return new gsSPMatrix(i.ToString());
        }
        public static gsDPLoadTextureBlock ParseTextureBlock(this DisplayList.Instruction i)
        {
            return new gsDPLoadTextureBlock(i.ToString());
        }
        public static gsDPLoadTLUT ParseTLUT(this DisplayList.Instruction i)
        {
            return new gsDPLoadTLUT(i.ToString());
        }
    }
}
