using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ZeldaObjectManager
{
    public class gsSPVertex : DisplayList.Instruction
    {
        public Segment.Address Address;
        public Data Data;
        public int Vertices;
        public int BufferIndex;

        public gsSPVertex()
        {
            String = String.Empty;
            Operation = String.Empty;
            Arguments = new string[0];
            Address = new Segment.Address();
            Data = new Data();
            Vertices = 0;
            BufferIndex = 0;
        }

        public gsSPVertex(string str)
        {
            String = str;
            Parse(String);
            Address = new Segment.Address(Arguments[0]);
            Vertices = Convert.ToInt32(Arguments[1]);
            Data = new Data(Segment.Buffer.BlockCopy(Address, Vertices * 0x10));
            BufferIndex = Convert.ToInt32(Arguments[2]);
        }

        public gsSPVertex(UInt64 b)
        {
            String = Gfx.Disassemble(String.Format("{0:X16}", b));
            Parse(String);
            Address = new Segment.Address((UInt32)(b & 0xFFFFFFFF));
            Vertices = (Int32)((b >> 44) & 0xFFF);
            Data = new Data(Segment.Buffer.BlockCopy(Address, Vertices * 0x10));
            BufferIndex = 0;
        }
    }

    public class gsDPLoadTLUT : DisplayList.Instruction
    {
        public Segment.Address Address;
        public Data Data;
        public int Colors;

        public gsDPLoadTLUT()
        {
            String = String.Empty;
            Operation = String.Empty;
            Arguments = new string[0];
            Address = new Segment.Address();
            Data = new Data();
            Colors = 0;
        }

        public gsDPLoadTLUT(string str)
        {
            String = str;
            Parse(String);

            switch(Operation)
            {
                case "gsDPLoadTLUT":
                    Colors = Convert.ToInt32(Arguments[0]);
                    Address = new Segment.Address(Arguments[2]);
                    break;
                case "gsDPLoadTLUTCmd":
                    Colors = Convert.ToInt32(Arguments[1]);
                    break;
                case "gsDPLoadTLUT_pal16":
                    Colors = 16;
                    Address = new Segment.Address(Arguments[1]);
                    break;
                case "gsDPLoadTLUT_pal256":
                    Colors = 256;
                    Address = new Segment.Address(Arguments[0]);
                    break;
            }

            Data = new Data(Segment.Buffer.BlockCopy(Address, Colors * 2));
        }
    }

    public class gsDPLoadTextureBlock : DisplayList.Instruction
    {
        public Segment.Address Address;
        public Data Data;
        public int Width;
        public int Height;
        public string Format;
        public string BitSize;
        public N64Codec Codec;

        public gsDPLoadTextureBlock()
        {
            String = String.Empty;
            Operation = String.Empty;
            Arguments = new string[0];
            Address = new Segment.Address();
            Data = new Data();
            Width = 0;
            Height = 0;
            Format = String.Empty;
            BitSize = String.Empty;
            Codec = N64Codec.ONEBPP;
        }
        public gsDPLoadTextureBlock(string str)
        {
            String = str;
            Parse(String);
            int alt = (Arguments.Length < 12) ? 1 : 0;
            Address = new Segment.Address(Arguments[0]);
            Format = Arguments[1];
            BitSize = (alt == 1) ? "G_IM_SIZ_4b" : Arguments[2];
            Width = Convert.ToInt32(Arguments[3 - alt]);
            Height = Convert.ToInt32(Arguments[4 - alt]);
            Codec = ParseCodec(Format, BitSize);
            Data = new Data(Segment.Buffer.BlockCopy(Address, N64Graphics.PixelsToBytes(Codec, Width * Height)));
        }
        public static N64Codec ParseCodec(string fmt, string siz)
        {
            string concat = String.Format("{0}|{1}", fmt, siz);
            N64Codec codec = N64Codec.RGBA16;
            switch(concat)
            {
                case "G_IM_FMT_RGBA|G_IM_SIZ_16b":
                    codec = N64Codec.RGBA16;
                    break;
                case "G_IM_FMT_RGBA|G_IM_SIZ_32b":
                    codec = N64Codec.RGBA32;
                    break;
                case "G_IM_FMT_IA|G_IM_SIZ_16b":
                    codec = N64Codec.IA16;
                    break;
                case "G_IM_FMT_IA|G_IM_SIZ_8b":
                    codec = N64Codec.IA8;
                    break;
                case "G_IM_FMT_IA|G_IM_SIZ_4b":
                    codec = N64Codec.IA4;
                    break;
                case "G_IM_FMT_I|G_IM_SIZ_8b":
                    codec = N64Codec.I8;
                    break;
                case "G_IM_FMT_I|G_IM_SIZ_4b":
                    codec = N64Codec.I4;
                    break;
                case "G_IM_FMT_CI|G_IM_SIZ_8b":
                    codec = N64Codec.CI8;
                    break;
                case "G_IM_FMT_CI|G_IM_SIZ_4b":
                    codec = N64Codec.CI4;
                    break;
            }

            return codec;
        }
    }
    public class DisplayList
    {
        public Segment.Address Address;
        public Data Data;
        public List<Instruction> Instructions;
        public List<gsSPVertex> VertexBuffers;
        public List<Texture> Textures;
        public List<gsDPLoadTLUT> Palettes;

        public class Instruction
        {
            public bool Modified;
            public string String;
            public string Operation;
            public string[] Arguments;

            public Instruction()
            {
                Modified = false;
                String = String.Empty;
                Operation = String.Empty;
                Arguments = new string[0];
            }

            public Instruction(string str)
            {
                // Parse
                Modified = false;
                String = str;
                Parse(String);
            }

            public void Parse(string str)
            {
                int head = 0;
                while (String[head] != '(')
                    head++;

                Operation = String.Substring(0, head);
                String = String.Replace(" ", String.Empty);
                String = String.Replace("|", " | ");
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
        public class Texture
        {
            public Segment.Address Address;
            public Data Data;
            public gsDPLoadTextureBlock Image;
            public gsDPLoadTLUT Palette;

            public Texture()
            {
                Image = new gsDPLoadTextureBlock();
                Address = Image.Address;
                Data = Image.Data;
                Palette = new gsDPLoadTLUT();
            }

            public Texture(gsDPLoadTextureBlock t)
            {
                Image = t;
                Address = Image.Address;
                Data = Image.Data;
                Palette = new gsDPLoadTLUT();
            }

            public Texture(gsDPLoadTLUT p)
            {
                Image = new gsDPLoadTextureBlock();
                Address = Image.Address;
                Data = Image.Data;
                Palette = p;
            }

            public Texture(gsDPLoadTextureBlock t, gsDPLoadTLUT p)
            {
                Image = t;
                Address = Image.Address;
                Data = Image.Data;
                Palette = p;
            }
        }
        public DisplayList()
        {
            Address = new Segment.Address();
            Data = new Data();
            Instructions = new List<Instruction>();
            VertexBuffers = new List<gsSPVertex>();
            Textures = new List<Texture>();
            Palettes = new List<gsDPLoadTLUT>();
        }
        public DisplayList(UInt32 o)
        {
            string[] temp = new string[0];
            Address = new Segment.Address(o);
            Data = new Data();
            Instructions = new List<Instruction>();
            VertexBuffers = new List<gsSPVertex>();
            Textures = new List<Texture>();
            Palettes = new List<gsDPLoadTLUT>();

            temp = Gfx.Disassemble(Program.Segments[Address.Index].FilePath, Address.Offset).Split('!');
            for (int i = 0; i < temp.Length; i++)
            {
                Instruction _i = new Instruction(temp[i]);

                // Vertex Data
                if (_i.Operation == "gsSPVertex")
                    VertexBuffers.Add(_i.ParseVertexBuffer());

                // Textures
                Texture t = new Texture();
                if (_i.Operation.Contains("gsDPLoadTextureBlock") || _i.Operation == "gsDPSetTextureImage")
                {
                    int p = i;
                    if (_i.Operation.Contains("gsDPLoadTextureBlock"))
                        t.Image = _i.ParseTextureBlock();
                    else // gsDPSetTextureImage
                    {
                        Instruction[] gLTB = new Instruction[7];
                        gLTB[0] = new Instruction(temp[p + 0]);
                        gLTB[1] = new Instruction(temp[p + 1]);
                        gLTB[2] = new Instruction(temp[p + 2]);
                        gLTB[3] = new Instruction(temp[p + 3]);
                        gLTB[4] = new Instruction(temp[p + 4]);
                        gLTB[5] = new Instruction(temp[p + 5]);
                        gLTB[6] = new Instruction(temp[p + 6]);
                        t.Image.Address = new Segment.Address(gLTB[0].Arguments[3]);
                        t.Image.Format = gLTB[5].Arguments[0];
                        t.Image.BitSize = gLTB[5].Arguments[1];
                        t.Image.Codec = gsDPLoadTextureBlock.ParseCodec(t.Image.Format, t.Image.BitSize);
                        if (gLTB[6].Operation == "gsDPSetTileSize")
                        {
                            string[] dim = gLTB[6].Arguments[3].Split('(', ')');
                            t.Image.Width = Convert.ToInt32(dim[1]);
                            dim = gLTB[6].Arguments[4].Split('(', ')');
                            t.Image.Height = Convert.ToInt32(dim[1]);
                            t.Image.Data = new Data(Segment.Buffer.BlockCopy(t.Image.Address, N64Graphics.PixelsToBytes(t.Image.Codec, t.Image.Width * t.Image.Height)));
                        }
                        else
                            t.Image.Data = new Data(Segment.Buffer.BlockCopy(t.Image.Address, (Convert.ToInt32(gLTB[3].Arguments[3]) + 1) * 2));
                    }

                    if (t.Image.Format.Contains("CI"))
                    {
                        p = i;
                        while (!temp[p].Contains("gsDPLoadTLUT") && (p < (temp.Length - 1)))
                            p++;

                        if (temp[p].Contains("gsDPLoadTLUT"))
                        {
                            t.Palette = new Instruction(temp[p]).ParseTLUT();
                            Palettes.Add(t.Palette);
                        }
                    }
                }

                if (t.Image.Data.Bytes.Length > 0 || t.Palette.Data.Bytes.Length > 0)
                    Textures.Add(new Texture(t.Image, t.Palette));

                Instructions.Add(_i);
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Instructions.Count(); i++)
            {
                if (i > 0)
                    sb.Append("!");
                sb.Append(Instructions[i].ToString());
            }

            return sb.ToString();
        }

        public static int Export(BinaryWriter f, params DisplayList[] dls)
        {
            int pass = 0;
            int d = 0; // Display List Iterator
            int a = 0; // Asset Iterator

            List<string> MapInfo = new List<string>();
            List<string> AssetHashTable = new List<string>();
            List<int> AssetWriteTable = new List<int>();
            List<int[]> AssetRef = new List<int[]>();
            string[] AssetString = new string[] { "Texture", "Palette", "Vertex Data", "Display List" };
            int WriteOffset = 0;

            // Padding?
            if (Program.ZOBJProperties.OutputAddress.Offset > 0)
            {
                if (Program.ZOBJProperties.PadOutput)
                    f.Write(new byte[Program.ZOBJProperties.OutputAddress.Offset]);
                else
                    WriteOffset = Program.ZOBJProperties.OutputAddress.Offset;
            }

            // Collect Assets
            while (pass < 2)
            {
                for (d = 0; d < dls.Count(); d++)
                {
                    a = (pass > 0) ? dls[d].Palettes.Count() : dls[d].Textures.Count();
                    if (a > 0)
                    {
                        while (a > 0)
                        {
                            dynamic Asset = 0;
                            switch(pass)
                            {
                                case 0:
                                    Asset = dls[d].Textures[--a];
                                    break;
                                case 1:
                                    Asset = dls[d].Palettes[--a];
                                    break;
                            }

                            // If asset hash does not exist in the table:
                            if (AssetHashTable.IndexOf(Asset.Data.MD5) <= -1)
                            {
                                AssetWriteTable.Add((int)f.BaseStream.Position + WriteOffset);
                                f.Write(Asset.Data.Bytes);
                                AssetHashTable.Add(Asset.Data.MD5);
                            }
                            AssetRef.Add(new int[] { Asset.Address.Index, Asset.Address.Offset, AssetHashTable.IndexOf(Asset.Data.MD5) });
                        }
                    }
                }
                pass++;
            }

            // Modify Instruction Offsets
            for (a = 0; a < AssetRef.Count(); a++)
            {
                for (d = 0; d < dls.Count(); d++)
                {
                    for (int i = 0; i < dls[d].Instructions.Count(); i++)
                    {
                        int type = (dls[d].Instructions[i].Operation.Contains("gsDPLoadTLUT")) ? 1 : 0;

                        if (!dls[d].Instructions[i].Modified)
                        {
                            for (int arg = 0; arg < dls[d].Instructions[i].Arguments.Count(); arg++)
                            {
                                Segment.Address old_offset = new Segment.Address(AssetRef[a][0], AssetRef[a][1]);
                                Segment.Address new_offset = new Segment.Address(Program.ZOBJProperties.OutputAddress.Index, AssetWriteTable[AssetRef[a][2]]);

                                if (dls[d].Instructions[i].Arguments[arg] == old_offset.ToString())
                                {
                                    dls[d].Instructions[i].Arguments[arg] = new_offset.ToString();
                                    //Console.WriteLine("{0} -> {1} ({2})", old_offset.ToString(), new_offset.ToString(), AssetString[type]);
                                    MapInfo.Add(String.Format("{0} -> {1} ({2})", old_offset.ToString(), new_offset.ToString(), AssetString[type]));
                                    dls[d].Instructions[i].Modified = true;
                                }
                            }
                        }

                        if (dls[d].Instructions[i].Modified)
                        {
                            dls[d].Instructions[i] = new Instruction(dls[d].Instructions[i].ToString());
                            dls[d].Instructions[i].Modified = true;
                        }
                    }
                }
            }

            // Write Vertex Data and Display Lists
            for (d = 0; d < dls.Count(); d++)
            {
                for (int v = 0; v < dls[d].VertexBuffers.Count(); v++)
                {
                    for (int i = 0; i < dls[d].Instructions.Count(); i++)
                    {
                        if (dls[d].Instructions[i].ToString() == dls[d].VertexBuffers[v].ToString())
                        {
                            Instruction _v = new Instruction(dls[d].VertexBuffers[v].String);
                            _v.Arguments[0] = new Segment.Address(Program.ZOBJProperties.OutputAddress.Index, (int)f.BaseStream.Position + WriteOffset).ToString();
                            dls[d].Instructions[i] = new Instruction(_v.ToString());
                            //Console.WriteLine("{0} -> {1} ({2})", dls[d].VertexBuffers[v].Address.ToString(), _v.Arguments[0], AssetString[2]);
                            MapInfo.Add(String.Format("{0} -> {1} ({2})", dls[d].VertexBuffers[v].Address.ToString(), _v.Arguments[0], AssetString[2]));
                        }
                    }
                    f.Write(dls[d].VertexBuffers[v].Data.Bytes);
                }
                //Console.WriteLine("{0} -> {1} ({2})", dls[d].Address.ToString(), new Segment.Address(Program.ZOBJProperties.OutputAddress.Index, (int)f.BaseStream.Position + WriteOffset).ToString(), AssetString[3]);
                MapInfo.Add(String.Format("{0} -> {1} ({2})", dls[d].Address.ToString(), new Segment.Address(Program.ZOBJProperties.OutputAddress.Index, (int)f.BaseStream.Position + WriteOffset).ToString(), AssetString[3]));
                f.Write(Gfx.Assemble(dls[d].ToString().Split('!')));
            }

            // Export Map
            if (Program.ZOBJProperties.ExportMap)
            {
                if (MapInfo.Count() > 0)
                {
                    using (StreamWriter sw = new StreamWriter(File.Create(String.Format("{0}_map.txt", Path.GetFileNameWithoutExtension(Program.ZOBJProperties.OutputFile)))))
                    {
                        foreach (string l in MapInfo)
                        {
                            sw.WriteLine($"{l}");
                            Console.WriteLine($"{l}");
                        }
                    }
                }
            }

            return 0;
        }
    }
}
