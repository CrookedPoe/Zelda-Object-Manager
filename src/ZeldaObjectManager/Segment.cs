using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ZeldaObjectManager
{
    public class SegmentIsNullException : Exception
    {
        public SegmentIsNullException()
        {
        }

        public SegmentIsNullException(string message) : base(message)
        {
        }

        public SegmentIsNullException(string message, Exception inner) : base(message, inner)
        {
        }
    }
    public class Segment
    {
        public class Address
        {
            public int Index;
            public int Offset;

            public Address()
            {
                Index = 0;
                Offset = 0;
            }
            public Address(string s)
            {
                Index = Convert.ToInt32(s.Substring(0, 4), 16);
                Offset = Convert.ToInt32(s.Substring(4, 6), 16);
            }
            public Address(UInt32 o)
            {
                Index = (int)(o >> 24) & 0xFF;
                Offset = (int)(o & 0xFFFFFF);
            }
            public Address(int s, int o)
            {
                Index = s;
                Offset = o;
            }
            public override string ToString()
            {
                return String.Format("0x{0:X2}{1:X6}", Index, Offset);
            }
        }
        public class Buffer
        {
            public string FilePath;
            public string FileName;
            public int Occupied;
            public Data Data;

            public Buffer()
            {
                FilePath = String.Empty;
                FileName = String.Empty;
                Data = new Data();
                Occupied = 0;
            }
            public Buffer(string file)
            {
                FileName = Path.GetFileNameWithoutExtension(file);
                FilePath = Path.GetFullPath(file);
                Data = new Data(FilePath);
                Occupied = Data.Bytes.Length;
            }

            public static byte[] BlockCopy(Address a, int size)
            {
                if (Program.Segments[a.Index] != null && Program.Segments[a.Index].Occupied > 0)
                    return Program.Segments[a.Index].Data.Bytes.GetSection(a.Offset, size);
                else
                    return new byte[0];
            }
        }
    }
}
