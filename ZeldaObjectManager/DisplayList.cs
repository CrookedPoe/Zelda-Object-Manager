using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ZeldaObjectManager
{
    class GfxInstruction
    {
        public string Operation;
        public string[] Arguments;

        public GfxInstruction(string i)
        {
            int head = 0;
            while (i[head] != '(')
                head++;

            Operation = i.Substring(0, head);
            Arguments = i.Substring(head + 1, i.Length - (head + 2)).Split(',');
        }
    }
    class VertexData
    {
        public int Segment;
        public int Offset;
        public byte[] Raw;

        public VertexData(byte[] src, int seg, int o, int size)
        {
            Segment = seg;
            Offset = o;
            Raw = Utility.ByteCopy(src, o, size);
        }
    }

    class Palette
    {
        public int Segment;
        public int Offset;
        public int Colors;
        public byte[] Raw;

        public Palette()
        {
            Segment = 0;
            Offset = 0;
            Colors = 0;
            Raw = new byte[0];
        }
        public Palette(byte[] src, int seg, int o, int size)
        {
            Segment = seg;
            Colors = size;
            size *= 2;
            Offset = o;
            Raw = Utility.ByteCopy(src, o, size);
        }
    }
    class Texture
    {
        public int Segment;
        public int Offset;
        public int Width, Height;
        public int BitSize;
        public N64Codec Codec;
        public byte[] Raw;
        public Palette Palette;

        public Texture()
        {
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
    class DisplayList
    {
        public int Segment;
        public int Offset;
        public List<GfxInstruction> Gfx;
        public string[] GfxStr;
        public List<VertexData> Vertices;
        public List<Texture> Textures;
        public int Size;
        public byte[] Raw;

        public DisplayList()
        {
        }

        public DisplayList(byte[] src, UInt32 o)
        {
            // Find Display List Size
            Segment = (int)(o & 0xFF000000) >> 24;
            Offset = (int)(o & 0x00FFFFFF);
            for (int i = Offset; i < src.Length; i += 8)
            {
                if (Utility.GfxDis(Utility.ByteCopy(src, i, 8)) == "gsSPEndDisplayList()")
                {
                    Size = ((i + 8) - Offset);
                    break;
                }
            }
            Raw = Utility.ByteCopy(src, Offset, Size);

            Gfx = new List<GfxInstruction>();
            GfxStr = Utility.GfxDis(Raw).Split('!');
            for (int i = 0; i < GfxStr.Length; i++)
            {
                Gfx.Add(new GfxInstruction(GfxStr[i]));
                //GfxStr[i] = GfxStr[i].Replace(",", ", ");
                //Console.WriteLine(GfxStr[i]);
            }


            Vertices = VerticesGet(src, this);
            //Textures = TexturesGet(src, this);
        }

        public static string[] MakeGfxStr(List<GfxInstruction> gfx)
        {
            StringBuilder sb = new StringBuilder();
            string[] _g = new string[gfx.Count()];

            for (int i = 0; i < gfx.Count(); i++)
            {
                sb.Clear();
                for (int j = 0; j < gfx[i].Arguments.Length; j++)
                {
                    if (j > 0)
                        sb.Append(", ");
                    sb.Append(gfx[i].Arguments[j]);
                }
                _g[i] = String.Format("{0}({1});", gfx[i].Operation, sb.ToString()); ;
            }

            return _g;
        }
        public static List<VertexData> VerticesGet(byte[] src, DisplayList dl)
        {
            List<VertexData> v = new List<VertexData>();

            for (int i = 0; i < dl.Gfx.Count(); i++)
            {
                if (dl.Gfx[i].Operation == "gsSPVertex")
                {
                    int seg = Convert.ToInt32(dl.Gfx[i].Arguments[0].Substring(0, 4), 16);
                    int o = Convert.ToInt32(dl.Gfx[i].Arguments[0].Substring(4, 6), 16);
                    int nv = Convert.ToInt32(dl.Gfx[i].Arguments[1]);
                    v.Add(new VertexData(src, seg, o, nv * 0x10));
                }
            }

            return v;
        }

        public static void ExportBinary(BinaryWriter f, params DisplayList[] dl)
        {
            for (int d = 0; d < dl.Count(); d++)
            {
                int old_offset = 0;
                int new_offset = 0;

                // Vertices
                for (int i = 0; i < dl[d].Vertices.Count(); i++)
                {
                    old_offset = dl[d].Vertices[i].Offset;
                    new_offset = (int)f.BaseStream.Position;
                    f.Write(dl[d].Vertices[i].Raw);

                    // Update Instructions
                    for (int j = 0; j < dl[d].Gfx.Count(); j++)
                    {
                        if (dl[d].Gfx[j].Operation == "gsSPVertex")
                        {
                            if (Convert.ToInt32(dl[d].Gfx[j].Arguments[0].Substring(4, 6), 16) == old_offset)
                            {
                                dl[d].Gfx[j].Arguments[0] = String.Format("0x{0:X2}{1:X6}", dl[d].Vertices[i].Segment, new_offset);
                                Console.WriteLine("G_VTX: 0x{0:X8} -> 0x{1:X8}", old_offset, new_offset);
                            }
                        }
                    }
                }

                // Display List
                Console.WriteLine("G_DPL: 0x{0:X8} -> 0x{1:X8}", dl[d].Offset, (int)f.BaseStream.Position);
                f.Write(Utility.GfxAsm(MakeGfxStr(dl[d].Gfx)));
            }
        }

        public static List<DisplayList> Optimize(byte[] src, params UInt32[] o)
        {
            List<DisplayList> dls = new List<DisplayList>();

            for (int i = 0; i < o.Count(); i++)
            {
                dls.Add(new DisplayList(src, o[i]));
            }

            return dls;
        }
    }
}
