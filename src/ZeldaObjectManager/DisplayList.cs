using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ZeldaObjectManager
{
    public enum BitSizes
    {
        ONEBPP = 1,
        G_IM_SIZ_4b = 4,
        G_IM_SIZ_8b = 8,
        G_IM_SIZ_16b = 16,
        G_IM_SIZ_32b = 32
    }
    class gsSPVertex
    {
        public GfxInstruction Input;
        public string Instruction;
        public int Segment;
        public int Offset;
        public int Vertices;
        public byte[] DataBlock;

        public gsSPVertex()
        {
            Instruction = "gsSPVertex";
            Segment = 0;
            Offset = 0;
            Vertices = 0;
            DataBlock = new byte[0];
        }
        public gsSPVertex(string i)
        {
            Input = new GfxInstruction(i);
            Instruction = Input.Operation;
            Segment = Convert.ToInt32(Input.Arguments[0].Substring(0, 4), 16);
            Offset = Convert.ToInt32(Input.Arguments[0].Substring(4, 6), 16);
            Vertices = Convert.ToInt32(Input.Arguments[1]);
            DataBlock = new byte[0];
        } 
        public gsSPVertex(byte[] src, string i)
        {
            Input = new GfxInstruction(i);
            Instruction = Input.Operation;
            Segment = Convert.ToInt32(Input.Arguments[0].Substring(0, 4), 16);
            Offset = Convert.ToInt32(Input.Arguments[0].Substring(4, 6), 16);
            Vertices = Convert.ToInt32(Input.Arguments[1]);
            DataBlock = Utility.ByteCopy(src, Offset, Vertices * 0x10);
        }
        public gsSPVertex(byte[] src, gsSPVertex v)
        {
            Input = v.Input;
            Instruction = Input.Operation;
            Segment = v.Segment;
            Offset = v.Offset;
            Vertices = v.Vertices;
            DataBlock = Utility.ByteCopy(src, Offset, Vertices * 0x10);
        }

        public static string MakeInstruction(gsSPVertex t)
        {
            return String.Format("{0}(0x{1:X2}{2:X6}, {3}, {4})", t.Input.Operation, t.Segment, t.Offset, t.Vertices, t.Input.Arguments[2]);
        }
    }
    class gsDPLoadTLUT
    {
        public GfxInstruction Input;
        public string Instruction;
        public int Segment;
        public int Offset;
        public int Colors;
        public byte[] DataBlock;
        public gsDPLoadTLUT()
        {
        }
        public gsDPLoadTLUT(string i)
        {
            Input = new GfxInstruction(i);
            Instruction = Input.Operation;
            int alt = 0;
            if (Instruction.Contains("_pal256"))
            {
                alt = 0;
                Colors = 256;
            }
            else if (Instruction.Contains("_pal16"))
            {
                alt = 1;
                Colors = 16;
            }
            else
            {
                Colors = Convert.ToInt32(Input.Arguments[0]);
                alt = 3;
            }
            Segment = Convert.ToInt32(Input.Arguments[alt].Substring(0, 4), 16);
            Offset = Convert.ToInt32(Input.Arguments[alt].Substring(4, 6), 16);
            DataBlock = new byte[0];
        }
        public gsDPLoadTLUT(byte[] src, string i)
        {
            Input = new GfxInstruction(i);
            Instruction = Input.Operation;
            int alt = 0;
            if (Instruction.Contains("_pal256"))
            {
                alt = 0;
                Colors = 256;
            }
            else if (Instruction.Contains("_pal16"))
            {
                alt = 1;
                Colors = 16;
            }
            else
            {
                Colors = Convert.ToInt32(Input.Arguments[0]);
                alt = 3;
            }
            Segment = Convert.ToInt32(Input.Arguments[alt].Substring(0, 4), 16);
            Offset = Convert.ToInt32(Input.Arguments[alt].Substring(4, 6), 16);
            DataBlock = Utility.ByteCopy(src, Offset, Colors * 2);
        }
        public gsDPLoadTLUT(byte[] src, gsDPLoadTLUT p)
        {
            Input = p.Input;
            Instruction = Input.Operation;
            Segment = p.Segment;
            Offset = p.Offset;
            Colors = p.Colors;
            DataBlock = Utility.ByteCopy(src, Offset, Colors * 2);
        }

        public static string MakeInstruction(gsDPLoadTLUT t)
        {
            if (t.Input.Operation.Contains("256"))
            {
                return String.Format("{0}(0x{1:X2}{2:X6})", t.Input.Operation, t.Segment, t.Offset);
            }
            else if (t.Input.Operation.Contains("16"))
            {
                return String.Format("{0}({3}, 0x{1:X2}{2:X6})", t.Input.Operation, t.Segment, t.Offset, t.Input.Arguments[0]);
            }
            else
                return String.Format("{0}({3}, {4}, 0x{1:X2}{2:X6})", t.Input.Operation, t.Segment, t.Offset, t.Input.Arguments[0], t.Input.Arguments[1]);
        }
    }
    class gsDPSetTextureImage
    {
        public GfxInstruction Input;
        public string Instruction;
        public int Segment;
        public int Offset;
        public string Format;
        public int BitSize;
        public N64Codec Codec;
        public int BlockSize;

        public gsDPSetTextureImage()
        {
        }
        public gsDPSetTextureImage(string i)
        {
            Input = new GfxInstruction(i);
            Instruction = Input.Operation;
            Segment = Convert.ToInt32(Input.Arguments[3].Substring(0, 4), 16);
            Offset = Convert.ToInt32(Input.Arguments[3].Substring(4, 6), 16);
        }
        public static string MakeInstruction(gsDPSetTextureImage t)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 1; i < t.Input.Arguments.Length; i++)
            {
                sb.Append(t.Input.Arguments[i]);
                if (i < t.Input.Arguments.Length - 1)
                    sb.Append(", ");
            }

            return String.Format("{0}({3}, 0x{1:X2}{2:X6})", t.Input.Operation, t.Segment, t.Offset, sb.ToString());
        }
    }
    class gsDPLoadTextureBlock
    {
        public GfxInstruction Input;
        public string Instruction;
        public int Segment;
        public int Offset;
        public string Format;
        public int BitSize;
        public int Width;
        public int Height;
        public N64Codec Codec;
        public gsDPLoadTLUT Palette;
        public byte[] DataBlock;

        public gsDPLoadTextureBlock()
        {
            DataBlock = new byte[0];
        }
        public gsDPLoadTextureBlock(string i)
        {
            Input = new GfxInstruction(i);
            Instruction = Input.Operation;
            int alt = Instruction.Contains("_4") ? 1 : 0;
            Segment = Convert.ToInt32(Input.Arguments[0].Substring(0, 4), 16);
            Offset = Convert.ToInt32(Input.Arguments[0].Substring(4, 6), 16);
            Format = Input.Arguments[1];
            BitSize = (alt == 1) ? 4 : (int)((BitSizes)Enum.Parse(typeof(BitSizes), Input.Arguments[2]));
            Width = Convert.ToInt32(Input.Arguments[3 - alt]);
            Height = Convert.ToInt32(Input.Arguments[4 - alt]);
            Codec = ParseCodec(Format, BitSize);
            Palette = new gsDPLoadTLUT();
            DataBlock = new byte[0];
        }
        public gsDPLoadTextureBlock(byte[] src, string i)
        {
            Input = new GfxInstruction(i);
            Instruction = Input.Operation;
            int alt = Instruction.Contains("_4") ? 1 : 0;
            Segment = Convert.ToInt32(Input.Arguments[0].Substring(0, 4), 16);
            Offset = Convert.ToInt32(Input.Arguments[0].Substring(4, 6), 16);
            Format = Input.Arguments[1];
            BitSize = (alt == 1) ? 4 : (int)((BitSizes)Enum.Parse(typeof(BitSizes), Input.Arguments[2]));
            Width = Convert.ToInt32(Input.Arguments[3 - alt]);
            Height = Convert.ToInt32(Input.Arguments[4 - alt]);
            Codec = ParseCodec(Format, BitSize);
            Palette = new gsDPLoadTLUT();
            DataBlock = Utility.ByteCopy(src, Offset, N64Graphics.PixelsToBytes(Codec, (Width * Height)));
        }
        public gsDPLoadTextureBlock(byte[] src, gsDPLoadTextureBlock t)
        {
            Input = t.Input;
            Instruction = Input.Operation;
            Segment = t.Segment;
            Offset = t.Offset;
            Format = t.Format;
            BitSize = t.BitSize;
            Width = t.Width;
            Height = t.Height;
            Codec = t.Codec;
            Palette = t.Palette;
            DataBlock = Utility.ByteCopy(src, Offset, N64Graphics.PixelsToBytes(Codec, (Width * Height)));
        }
        public gsDPLoadTextureBlock(byte[] src, gsDPSetTextureImage t)
        {
            Input = t.Input;
            Instruction = Input.Operation;
            Segment = t.Segment;
            Offset = t.Offset;
            Format = t.Format;
            BitSize = t.BitSize;
            //Width = t.Width;
            //Height = t.Height;
            Codec = t.Codec;
            Palette = new gsDPLoadTLUT();
            DataBlock = Utility.ByteCopy(src, Offset, t.BlockSize);
        }
        public gsDPLoadTextureBlock(byte[] src, gsDPLoadTLUT p, string i)
        {
            Input = new GfxInstruction(i);
            Instruction = Input.Operation;
            int alt = Instruction.Contains("_4") ? 1 : 0;
            Segment = Convert.ToInt32(Input.Arguments[0].Substring(0, 4), 16);
            Offset = Convert.ToInt32(Input.Arguments[0].Substring(4, 6), 16);
            Format = Input.Arguments[1];
            BitSize = (alt == 1) ? 4 : (int)((BitSizes)Enum.Parse(typeof(BitSizes), Input.Arguments[2]));
            Width = Convert.ToInt32(Input.Arguments[3 - alt]);
            Height = Convert.ToInt32(Input.Arguments[4 - alt]);
            Codec = ParseCodec(Format, BitSize);
            Palette = p;
            DataBlock = Utility.ByteCopy(src, Offset, N64Graphics.PixelsToBytes(Codec, (Width * Height)));
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
        public static string MakeInstruction(gsDPLoadTextureBlock t)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 1; i < t.Input.Arguments.Length; i++)
            {
                sb.Append(t.Input.Arguments[i]);
                if (i < t.Input.Arguments.Length - 1)
                    sb.Append(", ");
            }

            return String.Format("{0}(0x{1:X2}{2:X6}, {3})", t.Input.Operation, t.Segment, t.Offset, sb.ToString());
        }
    }
    class TextureCompare : IEqualityComparer<gsDPLoadTextureBlock>
    {
        public bool Equals(gsDPLoadTextureBlock a, gsDPLoadTextureBlock b)
        {
            return (a.DataBlock.SequenceEqual(b.DataBlock) && (a.Offset == b.Offset) && (a.Segment == b.Segment));
        }

        public int GetHashCode(gsDPLoadTextureBlock a)
        {
            return a.DataBlock.GetHashCode();
        }
    }
    class PaletteCompare : IEqualityComparer<gsDPLoadTLUT>
    {
        public bool Equals(gsDPLoadTLUT a, gsDPLoadTLUT b)
        {
            return (a.DataBlock.SequenceEqual(b.DataBlock) && (a.Offset == b.Offset) && (a.Segment == b.Segment));
        }

        public int GetHashCode(gsDPLoadTLUT a)
        {
            return a.DataBlock.GetHashCode();
        }
    }
    class GfxInstruction
    {
        public string Input;
        public string Operation;
        public string[] Arguments;

        public GfxInstruction(string i)
        {
            Input = i;
            int head = 0;
            while (i[head] != '(')
                head++;

            Operation = Input.Substring(0, head);
            Arguments = Input.Substring(head + 1, Input.Length - (head + 2)).Split(',');
        }
    }
    class DisplayList
    {
        public int Segment;
        public int Offset;
        public List<GfxInstruction> Gfx;
        public string[] GfxStr;
        public List<gsSPVertex> VertexBlocks;
        public List<gsDPLoadTextureBlock> Textures;
        public List<gsDPLoadTLUT> Palettes;
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
            GfxStr = Utility.GfxDis(Program.Segments[Segment].FilePath, Offset).Split('!');
            for (int i = 0; i < GfxStr.Length; i++)
            {
                Gfx.Add(new GfxInstruction(GfxStr[i]));
            }

            VertexBlocks = GetVertexBlock(src, this);
            Textures = GetTextures(src, this);
            Palettes = GetPalettes(this.Textures);
        }

        public static List<DisplayList> OptimizeList(byte[] src, params UInt32[] o)
        {
            List<DisplayList> dls = new List<DisplayList>();

            for (int i = 0; i < o.Count(); i++)
                dls.Add(new DisplayList(src, o[i]));

            return dls;
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
                _g[i] = String.Format("{0}({1});", gfx[i].Operation, sb.ToString());
            }

            return _g;
        }
        public static List<gsSPVertex> GetVertexBlock(byte[] src, DisplayList dl)
        {
            List<gsSPVertex> v = new List<gsSPVertex>();

            for (int i = 0; i < dl.Gfx.Count(); i++)
            {
                if (dl.Gfx[i].Operation == "gsSPVertex")
                {
                    gsSPVertex _v = new gsSPVertex(dl.Gfx[i].Input);

                    if (Program.Segments[_v.Segment].Occupied == true)
                    {
                        v.Add(new gsSPVertex(Program.Segments[_v.Segment].DataBlock, _v));
                    }
                }
            }

            return v;
        }
        public static List<gsDPLoadTextureBlock> GetTextures(byte[] src, DisplayList dl)
        {
            List<gsDPLoadTextureBlock> t = new List<gsDPLoadTextureBlock>();

            for (int i = 0; i < dl.Gfx.Count(); i++)
            {
                gsDPLoadTextureBlock _t = new gsDPLoadTextureBlock();
                if (dl.Gfx[i].Operation.Contains("gsDPLoadTextureBlock"))
                {
                    _t = new gsDPLoadTextureBlock(dl.Gfx[i].Input);

                    if (Program.Segments[_t.Segment].Occupied == true)
                    {
                        _t = new gsDPLoadTextureBlock(Program.Segments[_t.Segment].DataBlock, _t);
                    }

                }
                else if (dl.Gfx[i].Operation == "gsDPSetTextureImage")
                {
                    gsDPSetTextureImage _st = new gsDPSetTextureImage(dl.Gfx[i].Input);
                    int st_pos = i;
                    int _st_pos = i;

                    // SetTile Madness
                    int st_i = 0;
                    while (st_i < 2)
                    {
                        while (dl.Gfx[st_pos].Operation != "gsDPSetTile" && (st_pos < dl.Gfx.Count() - 1))
                            st_pos++;

                        if (dl.Gfx[st_pos].Operation == "gsDPSetTile")
                        {
                            if (st_i < 1)
                                st_pos++;
                            st_i++;
                        }
                    }

                    if (dl.Gfx[st_pos].Operation == "gsDPSetTile")
                    {
                        _st.Format = dl.Gfx[st_pos].Arguments[0];
                        _st.BitSize = (int)(BitSizes)Enum.Parse(typeof(BitSizes), dl.Gfx[st_pos].Arguments[1]);
                        _st.Codec = gsDPLoadTextureBlock.ParseCodec(_st.Format, _st.BitSize);
                    }

                    // LoadBlock Madness
                    while (dl.Gfx[_st_pos].Operation != "gsDPLoadBlock" && (_st_pos < dl.Gfx.Count() - 1))
                        _st_pos++;

                    if (dl.Gfx[_st_pos].Operation == "gsDPLoadBlock")
                        _st.BlockSize = Convert.ToInt32(dl.Gfx[_st_pos].Arguments[3]) + 1;

                    if (Program.Segments[_st.Segment].Occupied == true)
                    {
                        _t = new gsDPLoadTextureBlock(Program.Segments[_st.Segment].DataBlock, _st);
                    }

                }

                // Palette
                if (_t.Format == "G_IM_FMT_CI")
                {
                    int p = i;
                    while (!dl.Gfx[p].Operation.Contains("gsDPLoadTLUT") && (p < dl.Gfx.Count() - 1))
                        p++;

                    if (dl.Gfx[p].Operation.Contains("gsDPLoadTLUT"))
                    {
                        gsDPLoadTLUT _p = new gsDPLoadTLUT(dl.Gfx[p].Input);

                        if (Program.Segments[_p.Segment].Occupied == true)
                        {
                            _t.Palette = new gsDPLoadTLUT(Program.Segments[_p.Segment].DataBlock, _p);
                        }
                    }
                }

                if (_t.DataBlock.Length > 0)
                    t.Add(_t);
            }

            if (t.Count() > 0)
                t = t.Distinct(new TextureCompare()).ToList();

            return t;
        }
        public static List<gsDPLoadTLUT> GetPalettes(List<gsDPLoadTextureBlock> t)
        {
            List<gsDPLoadTLUT> p = new List<gsDPLoadTLUT>();

            for (int i = 0; i < t.Count(); i++)
            {
                if (t[i].Palette.DataBlock != null)
                    p.Add(t[i].Palette);
            }

            if (p.Count() > 0)
                p = p.Distinct(new PaletteCompare()).ToList();

            return p;
        }
        public static int Export(BinaryWriter f, params DisplayList[] dls)
        {
            List<gsDPLoadTextureBlock> t_all = new List<gsDPLoadTextureBlock>();
            List<gsDPLoadTLUT> p_all = new List<gsDPLoadTLUT>();

            // Collect Textures and Palettes
            for (int i = 0; i < dls.Count(); i++)
            {
                for (int j = 0; j < dls[i].Textures.Count(); j++) // Texture Data
                    t_all.Add(dls[i].Textures[j]);
                for (int j = 0; j < dls[i].Palettes.Count(); j++) // Palette Data
                    p_all.Add(dls[i].Palettes[j]);
            }

            // Write Textures
            if (t_all.Count > 0)
            {
                t_all = t_all.Distinct(new TextureCompare()).ToList();
                bool ltb = false;
                dynamic _t;
                for (int i = 0; i < t_all.Count(); i++)
                {
                    if (t_all[i].Input.Operation.Contains("gsDPLoadTextureBlock"))
                    {
                        ltb = true;
                        _t = new gsDPLoadTextureBlock(t_all[i].Input.Input);
                        _t.Segment = Program.OutputSegment;
                        _t.Offset = (int)f.BaseStream.Position;
                    }
                    else
                    {
                        ltb = false;
                        _t = new gsDPSetTextureImage(t_all[i].Input.Input);
                        _t.Segment = Program.OutputSegment;
                        _t.Offset = (int)f.BaseStream.Position;
                    }

                    // Modify Instruction Offsets
                    for (int j = 0; j < dls.Count(); j++)
                    {
                        for (int k = 0; k < dls[j].Gfx.Count(); k++)
                        {
                            if (dls[j].Gfx[k].Input == t_all[i].Input.Input)
                            {
                                if (ltb)
                                    dls[j].Gfx[k] = new GfxInstruction(gsDPLoadTextureBlock.MakeInstruction(_t));
                                else
                                    dls[j].Gfx[k] = new GfxInstruction(gsDPSetTextureImage.MakeInstruction(_t));
                                Console.WriteLine("0x{2:X2}{0:X6} -> 0x{3:X2}{1:X6} (Texture)", t_all[i].Offset, _t.Offset, t_all[i].Segment, _t.Segment);
                            }
                        }
                    }

                    // Write Data
                    f.Write(t_all[i].DataBlock);
                }
            }

            // Write Palettes
            if (p_all.Count > 0)
            {
                p_all = p_all.Distinct(new PaletteCompare()).ToList();
                for (int i = 0; i < p_all.Count(); i++)
                {
                    gsDPLoadTLUT _p = new gsDPLoadTLUT(p_all[i].Input.Input);
                    _p.Segment = Program.OutputSegment;
                    _p.Offset = (int)f.BaseStream.Position;

                    // Modify Instruction Offsets
                    for (int j = 0; j < dls.Count(); j++)
                    {
                        for (int k = 0; k < dls[j].Gfx.Count(); k++)
                        {
                            if (dls[j].Gfx[k].Input == p_all[i].Input.Input)
                            {
                                dls[j].Gfx[k] = new GfxInstruction(gsDPLoadTLUT.MakeInstruction(_p));
                                Console.WriteLine("0x{2:X2}{0:X6} -> 0x{3:X2}{1:X6} (Palette)", p_all[i].Offset, _p.Offset, p_all[i].Segment, _p.Segment);
                            }
                        }
                    }

                    // Write Data
                    f.Write(p_all[i].DataBlock);
                }
            }

            // Vertex Data and Display Lists
            for (int i = 0; i < dls.Count(); i++)
            {
                // Vertex Data
                for (int j = 0; j < dls[i].VertexBlocks.Count(); j++)
                {
                    gsSPVertex _v = new gsSPVertex(dls[i].VertexBlocks[j].Input.Input);

                    // Modify Instruction Offsets
                    _v.Segment = Program.OutputSegment;
                    _v.Offset = (int)f.BaseStream.Position;

                    for (int k = 0; k < dls[i].Gfx.Count(); k++)
                    {
                        if (dls[i].Gfx[k].Input == dls[i].VertexBlocks[j].Input.Input)
                        {
                            dls[i].Gfx[k] = new GfxInstruction(gsSPVertex.MakeInstruction(_v));
                            Console.WriteLine("0x{2:X2}{0:X6} -> 0x{3:X2}{1:X6} (Vertex Data)", dls[i].VertexBlocks[j].Offset, _v.Offset, dls[i].VertexBlocks[j].Segment, _v.Segment);
                            break;
                        }
                    }
                    // Write Data
                    f.Write(dls[i].VertexBlocks[j].DataBlock);
                }

                // Display List
                Console.WriteLine("0x{2:X2}{0:X6} -> 0x{3:X2}{1:X6} (Display List)", dls[i].Offset, (int)f.BaseStream.Position, dls[i].Segment, Program.OutputSegment);
                dls[i].GfxStr = MakeGfxStr(dls[i].Gfx);
                dls[i].Raw = Utility.GfxAsm(dls[i].GfxStr);
                f.Write(dls[i].Raw);
            }

            return 0;
        }
        public static int Export(StreamWriter f, params DisplayList[] dls)
        {
            return 0;
        }

    }
}
