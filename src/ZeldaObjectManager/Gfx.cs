using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace ZeldaObjectManager
{
    public class Gfx
    {
        public static string Disassemble(string file, int offset)
        {
            // Use gfxdis
            ProcessStartInfo gfxdis = new ProcessStartInfo();
            gfxdis.FileName = Program.GfxDisPath;
            gfxdis.Arguments = String.Format("-a 0x{0:X8} \"{1}\"", offset, Path.GetFullPath(file));
            gfxdis.UseShellExecute = false;
            gfxdis.RedirectStandardOutput = true;

            using (Process process = Process.Start(gfxdis))
            {
                using (StreamReader sr = process.StandardOutput)
                {
                    return sr.ReadToEnd();
                }
            }
        }
        public static string Disassemble(byte[] input)
        {
            // Convert byte array to a string.
            StringBuilder sb = new StringBuilder();
            for (int d = 0; d < input.Length; d++)
                sb.Append(String.Format("{0:X2}", input[d]));

            // Use gfxdis
            ProcessStartInfo gfxdis = new ProcessStartInfo();
            gfxdis.FileName = Program.GfxDisPath;
            gfxdis.Arguments = String.Format("-d {0}", sb.ToString());
            gfxdis.UseShellExecute = false;
            gfxdis.RedirectStandardOutput = true;

            using (Process process = Process.Start(gfxdis))
            {
                using (StreamReader sr = process.StandardOutput)
                {
                    return sr.ReadToEnd();
                }
            }
        }
        public static byte[] Assemble(params string[] stdin)
        {
            // Create temporary output file.
            using (StreamWriter sw = new StreamWriter(File.Create("temp.txt")))
            {
                for (int i = 0; i < stdin.Length; i++)
                {
                    sw.WriteLine(stdin[i]);
                    //Console.WriteLine(input[i]);
                }
            }

            // Execute gfxasm
            string[] stderr = new string[0];
            ProcessStartInfo gfxasm = new ProcessStartInfo();
            gfxasm.FileName = Program.GfxAsmPath;
            gfxasm.Arguments = "temp.txt";
            gfxasm.UseShellExecute = false;
            gfxasm.RedirectStandardInput = true;
            gfxasm.RedirectStandardError = true;

            using (Process proc = Process.Start(gfxasm))
            {
                using (StreamReader sr = proc.StandardError)
                {
                    stderr = sr.ReadToEnd().Split(',');
                }
            }

            // Cleanup
            File.Delete("temp.txt");
            byte[] _stderr = new byte[stderr.Length];
            for (int i = 0; i < stderr.Length; i++)
                _stderr[i] = Convert.ToByte(stderr[i], 16);

            return _stderr;
        }
    }

    public class Gbi
    {
        public enum Tiles
        {
            G_TX_RENDERTILE = 0,
            G_TX_LOADTILE = 7
        };
        public enum BitSizes
        {
            ONEBPP = 1,
            G_IM_SIZ_4b = 4,
            G_IM_SIZ_8b = 8,
            G_IM_SIZ_16b = 16,
            G_IM_SIZ_32b = 32
        }
        public class Vertex
        {
            public static int Size = 16;
            public float X;
            public float Y;
            public float Z;
            public int Flag;
            public ushort S;
            public float U;
            public float V;
            public ushort T;
            public byte[] Attributes;
            public byte[] DataBlock;

            public Vertex()
            {
                X = Y = Z = 0;
                S = T = 0;
                Flag = 0;
                U = V = 0;
                Attributes = new byte[4];
            }
            public Vertex(byte[] v)
            {
                DataBlock = v;
                X = Convert.ToSingle(Utility.ByteConcat(false, v[0], v[1]));
                Y = Convert.ToSingle(Utility.ByteConcat(false, v[2], v[3]));
                Z = Convert.ToSingle(Utility.ByteConcat(false, v[4], v[5]));
                S = Utility.ByteConcat(false, v[6], v[7]); // float Vertex.U = (S > 1024.0f || S < -1023.0f) ? 0 : ((Vertex.S / Texture.Width) / 32)
                T = Utility.ByteConcat(false, v[10], v[11]);// float Vertex.V = (T > 1024.0f || T < -1023.0f) ? 0 : (1 - ((Vertex.T / Texture.Height) / 32))
                Flag = Utility.ByteConcat(false, v[8], v[9]);
                U = V = 0;
                Attributes = new byte[4] { v[12], v[13], v[14], v[15] };
            }
            public Vertex(float[] v, float[] vt, float[] vn)
            {
                X = Convert.ToUInt16(v[0]);
                Y = Convert.ToUInt16(v[1]);
                Z = Convert.ToUInt16(v[2]);
                Flag = 0;
                S = Convert.ToUInt16(vt[0]);
                T = Convert.ToUInt16(vt[1]);
                U = vt[0];
                V = vt[0];
                Attributes = new byte[]
                {
                    Convert.ToByte(vn[0] * 128), Convert.ToByte(vn[1] * 128), Convert.ToByte(vn[2] * 128), 0xFF
                };
            }
            public Vertex(float[] v)
            {
                X = Convert.ToUInt16(v[0]);
                Y = Convert.ToUInt16(v[1]);
                Z = Convert.ToUInt16(v[2]);
                Flag = S = T = 0;
                U = V = 0;
                Attributes = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF };
            }
        }
        public class Triangle
        {
            public gsSPVertex Buffer;
            public int BufferIndex;
            public int[] Index;
            public Vertex[] Vertex;

            public Triangle()
            {
                Buffer = new gsSPVertex();
                BufferIndex = 0;
                Index = new int[0];
                Vertex = new Vertex[0];
            }
            public Triangle(params int[] v)
            {
                Buffer = new gsSPVertex();
                BufferIndex = 0;
                Index = new int[3];
                Index[0] = v[0];
                Index[1] = v[1];
                Index[2] = v[2];
                Vertex = new Vertex[0];
            }
            public Triangle(gsSPVertex buf, params int[] v)
            {
                Buffer = buf;
                BufferIndex = 0;
                Index = new int[3];
                Index[0] = v[0];
                Index[1] = v[1];
                Index[2] = v[2];
                Vertex = new Vertex[] { Buffer.VertexBlock[Index[0]], Buffer.VertexBlock[Index[1]], Buffer.VertexBlock[Index[2]] };
            }
            public Triangle(gsSPVertex buf, int idx, params int[] v)
            {
                Buffer = buf;
                BufferIndex = idx;
                Index = new int[3];
                Index[0] = v[0];
                Index[1] = v[1];
                Index[2] = v[2];
                Vertex = new Vertex[] { Buffer.VertexBlock[Index[0]], Buffer.VertexBlock[Index[1]], Buffer.VertexBlock[Index[2]] };
            }
        }
        public class Quadrangle
        {
            public gsSPVertex Buffer;
            public int BufferIndex;
            public int[] Index;
            public Vertex[] Vertex;

            public Quadrangle()
            {
                Buffer = new gsSPVertex();
                BufferIndex = 0;
                Index = new int[0];
                Vertex = new Vertex[0];
            }
            public Quadrangle(params int[] v)
            {
                Buffer = new gsSPVertex();
                BufferIndex = 0;
                Index = new int[4];
                Index[0] = v[0];
                Index[1] = v[1];
                Index[2] = v[2];
                Index[3] = v[3];
                Vertex = new Vertex[0];
            }
            public Quadrangle(gsSPVertex buf, params int[] v)
            {
                Buffer = buf;
                BufferIndex = 0;
                Index = new int[4];
                Index[0] = v[0];
                Index[1] = v[1];
                Index[2] = v[2];
                Index[3] = v[3];
                Vertex = new Vertex[] { Buffer.VertexBlock[Index[0]], Buffer.VertexBlock[Index[1]], Buffer.VertexBlock[Index[2]], Buffer.VertexBlock[Index[3]] };
            }
            public Quadrangle(gsSPVertex buf, int idx, params int[] v)
            {
                Buffer = buf;
                BufferIndex = idx;
                Index = new int[4];
                Index[0] = v[0];
                Index[1] = v[1];
                Index[2] = v[2];
                Index[3] = v[3];
                Vertex = new Vertex[] { Buffer.VertexBlock[Index[0]], Buffer.VertexBlock[Index[1]], Buffer.VertexBlock[Index[2]], Buffer.VertexBlock[Index[3]] };
            }
        }
        public class Instruction
        {
            public string String;
            public string Operation;
            public string[] Arguments;
            public Instruction()
            {
                String = Operation = String.Empty;
                Arguments = new string[0];
            }
            public Instruction(string str)
            {
                String = str;

                int head = 0;
                while (String[head] != '(')
                    head++;

                Operation = String.Substring(0, head);

                String = String.Replace(" ", String.Empty).ToString();
                String = String.Replace("|", " | ").ToString();
                Arguments = String.Substring(head + 1, String.Length - (head + 2)).Split(',');
            }
            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < Arguments.Length; i++)
                {
                    if (i > 0)
                        sb.Append(", ");
                    sb.Append(Arguments[i]);
                }

                return String.Format("{0}({1})", Operation, sb.ToString());
            }
        }
        public class Data
        {
            public int Segment;
            public int Offset;
            public byte[] Block;

            public Data()
            {
                Segment = Offset = 0;
                Block = new byte[0];
            }

            public static int IsolateOffset(Data d, Instruction i, int arg_idx)
            {
                if (i.Arguments[arg_idx].Contains("0x"))
                {
                    d.Segment = Convert.ToInt32(i.Arguments[arg_idx].Substring(0, 4), 16);
                    d.Offset = Convert.ToInt32(i.Arguments[arg_idx].Substring(4, 6), 16);
                    return 0; // Success
                }
                else
                {
                    d.Segment = 0;
                    d.Offset = 0;
                    return 1; // Failure
                }
            }
            public static int BlockCopy(Data d, int seg, int offset, int size)
            {
                if (Program.Segments[seg].Occupied == true)
                {
                    d.Block = Utility.ByteCopy(Program.Segments[seg].DataBlock, offset, size);
                    return 0; // Success
                }
                else
                {
                    d.Block = new byte[0];
                    return 1; // Failure
                }
            }
        }
        /*public class DataComparer : IEqualityComparer<byte[]>
        {
            public bool Equals(byte[] x, byte[] y)
            {
                return x.SequenceEqual(y);
            }

            public int GetHashCode(byte[] obj)
            {
                return obj.GetHashCode();
            }
        }*/
        public class gsSPVertex
        {
            public int Index;
            public Instruction Instruction;
            public Data Data;
            public int Vertices;
            public Vertex[] VertexBlock;

            public gsSPVertex()
            {
                Instruction = new Instruction();
                Data = new Data();
                Index = 0;
                Vertices = 0;
            }
            public gsSPVertex(string str)
            {
                Index = 0;
                Instruction = new Instruction(str);
                Data = new Data();
                Data.IsolateOffset(Data, Instruction, 0);
                Vertices = Convert.ToInt32(Instruction.Arguments[1]);
                Data.BlockCopy(Data, Data.Segment, Data.Offset, Vertices * Vertex.Size);
                VertexBlock = new Vertex[Vertices];
                for (int i = 0; i < Vertices; i++)
                {
                    VertexBlock[i] = new Vertex(Utility.ByteCopy(Data.Block, i * Vertex.Size, Vertex.Size));
                }
            }
            public override string ToString()
            {
                return String.Format("{0}(0x{1:X2}{2:X6}, {3}, {4})", Instruction.Operation, Data.Segment, Data.Offset, Vertices, Instruction.Arguments[2]);
            }
        }
        public class gsSP1Triangle
        {
            public Instruction Instruction;
            public Triangle Triangle;

            public gsSP1Triangle()
            {
                Instruction = new Instruction();
                Triangle = new Triangle();
            }
            public gsSP1Triangle(string str)
            {
                Instruction = new Instruction(str);
                Triangle = new Triangle(Convert.ToInt32(Instruction.Arguments[0]) / 2, Convert.ToInt32(Instruction.Arguments[1]) / 2, Convert.ToInt32(Instruction.Arguments[2]) / 2);
            }

            public override string ToString()
            {
                return String.Format("{0}({1}, {2}, {3}, {4})", Instruction.Operation, Triangle.Index[0] * 2, Triangle.Index[1] * 2, Triangle.Index[2] * 2, Instruction.Arguments[3]);
            }
        }
        public class gsSP2Triangles
        {
            public Instruction Instruction;
            public Triangle[] Triangle;

            public gsSP2Triangles()
            {
                Instruction = new Instruction();
                Triangle = new Triangle[0];
            }
            public gsSP2Triangles(string str)
            {
                Instruction = new Instruction(str);
                Triangle = new Triangle[2];
                Triangle[0] = new Triangle(Convert.ToInt32(Instruction.Arguments[0]) / 2, Convert.ToInt32(Instruction.Arguments[1]) / 2, Convert.ToInt32(Instruction.Arguments[2]) / 2);
                Triangle[1] = new Triangle(Convert.ToInt32(Instruction.Arguments[4]) / 2, Convert.ToInt32(Instruction.Arguments[5]) / 2, Convert.ToInt32(Instruction.Arguments[6]) / 2);
            }
            public override string ToString()
            {
                return String.Format("{0}({1}, {2}, {3}, {4}, {5}, {6}, {7}, {8})", Instruction.Operation, Triangle[0].Index[0] * 2, Triangle[0].Index[1] * 2, Triangle[0].Index[2] * 2, Instruction.Arguments[3], Triangle[1].Index[0] * 2, Triangle[1].Index[1] * 2, Triangle[1].Index[2] * 2, Instruction.Arguments[7]);
            }
        }
        public class gsSP1Quadrangle
        {
            public Instruction Instruction;
            public Quadrangle Quadrangle;

            public gsSP1Quadrangle()
            {
                Instruction = new Instruction();
                Quadrangle = new Quadrangle();
            }
            public gsSP1Quadrangle(string str)
            {
                Instruction = new Instruction(str);
                Quadrangle = new Quadrangle(Convert.ToInt32(Instruction.Arguments[0]) / 2, Convert.ToInt32(Instruction.Arguments[1]) / 2, Convert.ToInt32(Instruction.Arguments[2]) / 2, Convert.ToInt32(Instruction.Arguments[3]) / 2);
            }

            public override string ToString()
            {
                return String.Format("{0}({1}, {2}, {3}, {4})", Instruction.Operation, Quadrangle.Index[0] * 2, Quadrangle.Index[1] * 2, Quadrangle.Index[2] * 2, Quadrangle.Index[3] * 2, Instruction.Arguments[4]);
            }
        }
        public class gsSPDisplayList
        {
            public Instruction Instruction;
            public Data Data;
            public bool Branch;

            public gsSPDisplayList()
            {
                Instruction = new Instruction();
                Data = new Data();
                Branch = false;
            }

            public gsSPDisplayList(string str)
            {
                Instruction = new Instruction(str);
                Data.IsolateOffset(Data, Instruction, 0);
                Branch = (Instruction.Operation == "gsSPBranchList") ? true : false;
            }

            public override string ToString()
            {
                return String.Format("{0}(0x{1:X2}{2:X6})", Instruction.Operation, Data.Segment, Data.Offset);
            }
        }
        public class gsSPEndDisplayList
        {
            public string Instruction = "gsSPEndDisplayList";
        }
        public class gsDPLoadSync
        {
            public string Instruction = "gsDPLoadSync";
        }
        public class gsDPPipeSync
        {
            public string Instruction = "gsDPPipeSync";
        }
        public class gsDPTileSync
        {
            public string Instruction = "gsDPTileSync";
        }
        public class gsDPLoadTLUT
        {
            public Instruction Instruction;
            public Data Data;
            public int Colors;

            public gsDPLoadTLUT()
            {
                Instruction = new Instruction();
                Data = new Data();
                Colors = 0;
            }
            public gsDPLoadTLUT(string str)
            {
                Instruction = new Instruction(str);
                Data = new Data();
                switch (Instruction.Operation)
                {
                    case "gsDPLoadTLUT":
                        Colors = Convert.ToInt32(Instruction.Arguments[0]);
                        Data.IsolateOffset(Data, Instruction, 2);
                        break;
                    case "gsDPLoadTLUTCmd":
                        Colors = Convert.ToInt32(Instruction.Arguments[1]) + 1;
                        break;
                    case "gsDPLoadTLUT_pal16":
                        Colors = 16;
                        Data.IsolateOffset(Data, Instruction, 1);
                        break;
                    case "gsDPLoadTLUT_pal256":
                        Colors = 256;
                        Data.IsolateOffset(Data, Instruction, 0);
                        break;
                }
                Data.BlockCopy(Data, Data.Segment, Data.Offset, Colors * sizeof(short));
            }
            public override string ToString()
            {
                string tlut = String.Empty;
                switch (Instruction.Operation)
                {
                    case "gsDPLoadTLUT":
                        tlut = String.Format("{0}({3}, {4}, 0x{1:X2}{2:X6})", Instruction.Operation, Data.Segment, Data.Offset, Colors, Instruction.Arguments[1]);
                        break;
                    case "gsDPLoadTLUTCmd":
                        tlut = String.Format("{0}({1}, {2})", Instruction.Operation, Instruction.Arguments[0], Colors - 1);
                        break;
                    case "gsDPLoadTLUT_pal16":
                        tlut = String.Format("{0}({3}, 0x{1:X2}{2:X6})", Instruction.Operation, Data.Segment, Data.Offset, Instruction.Arguments[0]);
                        break;
                    case "gsDPLoadTLUT_pal256":
                        tlut = String.Format("{0}(0x{1:X2}{2:X6})", Instruction.Operation, Data.Segment, Data.Offset);
                        break;
                }
                return tlut;
            }
        }
        public class gsDPSetTileSize
        {
            public Instruction Instruction;
            public int Width, Height;

            public gsDPSetTileSize()
            {
                Instruction = new Instruction();
                Width = Height = 0;
            }
            public gsDPSetTileSize(string str)
            {
                Instruction = new Instruction(str);
                Width = Convert.ToInt32(Instruction.Arguments[3].Split('(', ')')[1]) + 1;
                Height = Convert.ToInt32(Instruction.Arguments[4].Split('(', ')')[1]) + 1;
            }

            public override string ToString()
            {
                return String.Format("{0}({1}, {2}, {3}, qu102({4}), qu102({5})", Instruction.Operation, Instruction.Arguments[0], Instruction.Arguments[1], Instruction.Arguments[2], Width - 1, Height - 1);
            }
        }
        public class gsDPLoadBlock
        {
            public Instruction Instruction;
            public int BlockSize;

            public gsDPLoadBlock()
            {
                Instruction = new Instruction();
                BlockSize = 0;
            }
            public gsDPLoadBlock(string str)
            {
                Instruction = new Instruction(str);
                BlockSize = Convert.ToInt32(Instruction.Arguments[3]) + 1;
            }

            public override string ToString()
            {
                return String.Format("{0}({1}, {2}, {3}, {4}, {5})", Instruction.Operation, Instruction.Arguments[0], Instruction.Arguments[1], Instruction.Arguments[2], BlockSize - 1, Instruction.Arguments[4]);
            }
        }
        public class gsDPSetTile
        {
            public Instruction Instruction;
            public string Format;
            public int BitSize;

            public gsDPSetTile()
            {
                Instruction = new Instruction();
                Format = String.Empty;
                BitSize = 0;
            }

            public gsDPSetTile(string str)
            {
                Instruction = new Instruction(str);
                Format = Instruction.Arguments[0];
                BitSize = (int)(BitSizes)Enum.Parse(typeof(BitSizes), Instruction.Arguments[1]); ;
            }
        }
        public class gsDPSetTextureImage
        {
            public Instruction Instruction;
            public Data Data;

            public gsDPSetTextureImage()
            {
                Instruction = new Instruction();
                Data = new Data();
            }

            public gsDPSetTextureImage(string str)
            {
                Data = new Data();
                Instruction = new Instruction(str);
                Data.IsolateOffset(Data, Instruction, 3);
            }

            public override string ToString()
            {
                return String.Format("{0}({3}, {4}, {5}, 0x{1:X2}{2:X6})", Instruction.Operation, Data.Segment, Data.Offset, Instruction.Arguments[0], Instruction.Arguments[1], Instruction.Arguments[2]);
            }
        }
        public class gsDPLoadTextureBlock
        {
            public Instruction Instruction;
            public gsDPSetTextureImage TextureImage;
            public gsDPSetTileSize TileSize;
            public gsDPSetTile TileB;
            public gsDPLoadBlock LoadBlock;
            public gsDPLoadTLUT Palette;
            public N64Codec Codec;

            public gsDPLoadTextureBlock()
            {
                Instruction = new Instruction();
                TextureImage = new gsDPSetTextureImage();
                TileSize = new gsDPSetTileSize();
                TileB = new gsDPSetTile();
                LoadBlock = new gsDPLoadBlock();
                Palette = new gsDPLoadTLUT();
            }

            public gsDPLoadTextureBlock(string str)
            { 
                Instruction = new Instruction(str);
                TextureImage = new gsDPSetTextureImage();
                TileSize = new gsDPSetTileSize();
                TileB = new gsDPSetTile();
                LoadBlock = new gsDPLoadBlock();
                Palette = new gsDPLoadTLUT();

                bool alt = (Instruction.Arguments.Length < 12) ? true : false;
                Data.IsolateOffset(TextureImage.Data, Instruction, 0);
                TileB.Format = Instruction.Arguments[1];
                TileB.BitSize = (alt) ? 4 : (int)(BitSizes)Enum.Parse(typeof(BitSizes), Instruction.Arguments[2]);
                TileSize.Width = Convert.ToInt32(Instruction.Arguments[3 - Convert.ToInt32(alt)]);
                TileSize.Height = Convert.ToInt32(Instruction.Arguments[4 - Convert.ToInt32(alt)]);
                Codec = ParseCodec(TileB.Format, TileB.BitSize);
                Data.BlockCopy(TextureImage.Data, TextureImage.Data.Segment, TextureImage.Data.Offset, N64Graphics.PixelsToBytes(Codec, (TileSize.Width * TileSize.Height)));
            }
            public override string ToString()
            {
                string ltb = String.Empty;
                switch(Instruction.Arguments.Length < 12)
                {
                    case true:
                        ltb = String.Format("{0}(0x{1:X2}{2:X6}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12})"
                                , Instruction.Operation
                                , TextureImage.Data.Segment, TextureImage.Data.Offset
                                , TileB.Format, TileSize.Width, TileSize.Height
                                , Instruction.Arguments[4], Instruction.Arguments[5], Instruction.Arguments[6]
                                , Instruction.Arguments[7], Instruction.Arguments[8], Instruction.Arguments[9]
                                , Instruction.Arguments[10]
                            );
                        break;
                    case false:
                        ltb = String.Format("{0}(0x{1:X2}{2:X6}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13})"
                                , Instruction.Operation
                                , TextureImage.Data.Segment, TextureImage.Data.Offset
                                , TileB.Format, Enum.GetName(typeof(BitSizes), TileB.BitSize)
                                , TileSize.Width, TileSize.Height
                                , Instruction.Arguments[5], Instruction.Arguments[6], Instruction.Arguments[7]
                                , Instruction.Arguments[8], Instruction.Arguments[9], Instruction.Arguments[10]
                                , Instruction.Arguments[11]
                            );
                        break;
                }
                return ltb;
            }

            public static void SetPalette(gsDPLoadTextureBlock t, string str)
            {
                t.Palette =  new gsDPLoadTLUT(str);
            }

            public static N64Codec ParseCodec(string fmt, int siz)
            {
                string _Codec = String.Format("{0}!{1}", fmt, siz);
                N64Codec codec = N64Codec.ONEBPP;

                switch (_Codec)
                {
                    case "G_IM_FMT_RGBA!16":
                        codec = N64Codec.RGBA16;
                        break;
                    case "G_IM_FMT_RGBA!32":
                        codec = N64Codec.RGBA32;
                        break;
                    case "G_IM_FMT_IA!16":
                        codec = N64Codec.IA16;
                        break;
                    case "G_IM_FMT_IA!8":
                        codec = N64Codec.IA8;
                        break;
                    case "G_IM_FMT_IA!4":
                        codec = N64Codec.IA4;
                        break;
                    case "G_IM_FMT_I!8":
                        codec = N64Codec.I8;
                        break;
                    case "G_IM_FMT_I!4":
                        codec = N64Codec.I4;
                        break;
                    case "G_IM_FMT_CI!8":
                        codec = N64Codec.CI8;
                        break;
                    case "G_IM_FMT_CI!4":
                        codec = N64Codec.CI4;
                        break;
                }

                return codec;
            }
        }
    }
}
