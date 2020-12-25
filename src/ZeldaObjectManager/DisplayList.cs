using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ZeldaObjectManager
{
    class DisplayList
    {
        public int Segment;
        public int Offset;
        public string[] String;

        public List<Gbi.Instruction> Instructions;

        // Geometry
        public List<Gbi.gsSPVertex> VertexBuffers;
        //public List<Gbi.Triangle> TriBuffers;
        //public List<Gbi.Quadrangle> QuadBuffers;

        // Textures
        public List<Gbi.gsDPLoadTextureBlock> TextureBlocks;
        public List<Gbi.gsDPLoadTLUT> Palettes;

        public DisplayList()
        {
            Instructions = new List<Gbi.Instruction>();
        }
        public DisplayList(UInt32 o)
        {
            Instructions = new List<Gbi.Instruction>();
            Segment = (int)(o >> 24) & 0xFF;
            Offset = (int)(o & 0x00FFFFFF);
            String = Gfx.Disassemble(Program.Segments[Segment].FilePath, Offset).Split('!');
            for (int i = 0; i < String.Length; i++)
            {
                Instructions.Add(new Gbi.Instruction(String[i]));
            }
            VertexBuffers = GetVertexBuffers(this);
            TextureBlocks = GetTextures(this);

            Palettes = new List<Gbi.gsDPLoadTLUT>();
            for (int i = 0; i < TextureBlocks.Count(); i++)
            {
                if (TextureBlocks[i].Palette.Data.Block.Length > 0)
                    Palettes.Add(TextureBlocks[i].Palette);
            }
        }
        public static List<Gbi.gsSPVertex> GetVertexBuffers(DisplayList dl)
        {
            List<Gbi.gsSPVertex> v = new List<Gbi.gsSPVertex>();
            for (int i = 0; i < dl.Instructions.Count(); i++)
            {
                if (dl.Instructions[i].Operation == "gsSPVertex")
                    v.Add(new Gbi.gsSPVertex(dl.Instructions[i].ToString()));
            }
            return v;
        }
        public static List<Gbi.gsDPLoadTextureBlock> GetTextures(DisplayList dl)
        {
            List<Gbi.gsDPLoadTextureBlock> t = new List<Gbi.gsDPLoadTextureBlock>();
            for (int i = 0; i < dl.Instructions.Count(); i++)
            {
                int _i = 0;
                Gbi.gsDPLoadTextureBlock _t = new Gbi.gsDPLoadTextureBlock();

                // Texture Image
                if (dl.Instructions[i].Operation.Contains("gsDPLoadTextureBlock"))
                {
                    _t = new Gbi.gsDPLoadTextureBlock(dl.Instructions[i].ToString());

                    // Palette
                    _i = i;
                    if (_t.TileB.Format == "G_IM_FMT_CI")
                    {
                        while (!dl.Instructions[_i].Operation.Contains("gsDPLoadTLUT"))
                            _i++;
                        if (dl.Instructions[_i].Operation.Contains("gsDPLoadTLUT"))
                            _t.Palette = new Gbi.gsDPLoadTLUT(dl.Instructions[_i].ToString());
                    }
                }
                else if (dl.Instructions[i].Operation == "gsDPSetTextureImage")
                {
                    _t.TextureImage = new Gbi.gsDPSetTextureImage(dl.Instructions[i].ToString());

                    int tile = 2;
                    _i = i;
                    while (tile > 0)
                    {
                        _i++;
                        if (dl.Instructions[_i].Operation == "gsDPLoadBlock")
                        {
                            _t.LoadBlock = new Gbi.gsDPLoadBlock(dl.Instructions[_i].ToString());
                            Gbi.Data.BlockCopy(_t.TextureImage.Data, _t.TextureImage.Data.Segment, _t.TextureImage.Data.Offset, N64Graphics.PixelsToBytes(_t.Codec, _t.LoadBlock.BlockSize));
                        }

                        if (dl.Instructions[_i].Operation == "gsDPSetTile")
                        {
                            if (--tile > 0)
                                continue;
                            else
                            {
                                _t.TileB = new Gbi.gsDPSetTile(dl.Instructions[_i].ToString());
                                _t.Codec = Gbi.gsDPLoadTextureBlock.ParseCodec(_t.TileB.Format, _t.TileB.BitSize);
                            }
                        }
                    }

                    // Palette
                    _i = i;
                    if (_t.TileB.Format == "G_IM_FMT_CI")
                    {
                        while (!dl.Instructions[_i].Operation.Contains("gsDPLoadTLUT"))
                            _i++;
                        if (dl.Instructions[_i].Operation.Contains("gsDPLoadTLUT"))
                            _t.Palette = new Gbi.gsDPLoadTLUT(dl.Instructions[_i].ToString());
                    }
                }

                if (_t.TextureImage.Data.Block.Length > 0)
                    t.Add(_t);
            }
            return t;
        }
        public static string[] Restring(DisplayList dl)
        {
            string[] c = new string[dl.Instructions.Count()];
            for (int i = 0; i < dl.Instructions.Count(); i++)
                c[i] = dl.Instructions[i].ToString();
            return c;
        }
        public static int Export(BinaryWriter f, params DisplayList[] dls)
        {
            int total = 0;
            int t = 0;
            int p = 0;

            // Collect Textures and Palettes
            List<Gbi.gsDPLoadTextureBlock> t_all = new List<Gbi.gsDPLoadTextureBlock>();
            List<Gbi.gsDPLoadTLUT> p_all = new List<Gbi.gsDPLoadTLUT>();
            List<string> t_hash = new List<string>();
            List<string> p_hash = new List<string>();
            List<string> map = new List<string>();
            total = dls.Count();
            while (total > 0)
            {
                --total;
                t = dls[total].TextureBlocks.Count();
                p = dls[total].Palettes.Count();
                if (t > 0)
                    while (t > 0)
                    {
                        if (!t_hash.Contains(Utility.ByteMD5(dls[total].TextureBlocks[--t].TextureImage.Data.Block)))
                        {
                            t_all.Add(dls[total].TextureBlocks[t]);
                            t_hash.Add(Utility.ByteMD5(dls[total].TextureBlocks[t].TextureImage.Data.Block));
                        }
                    }
                if (p > 0)
                    while (p > 0)
                    {
                        if (!p_hash.Contains(Utility.ByteMD5(dls[total].Palettes[--p].Data.Block)))
                        {
                            p_all.Add(dls[total].Palettes[p]);
                            p_hash.Add(Utility.ByteMD5(dls[total].Palettes[p].Data.Block));
                        }
                    }
            }

            // Write Textures
            t = t_all.Count();
            while (t > 0)
            {
                --t;
                bool ltb = false;
                dynamic _t;

                if (t_all[t].Instruction.Operation.Contains("gsDPLoadTextureBlock"))
                {
                    ltb = true;
                    _t = new Gbi.gsDPLoadTextureBlock(t_all[t].Instruction.String);
                    _t.TextureImage.Data.Segment = Program.OutputSegment;
                    _t.TextureImage.Data.Offset = (int)f.BaseStream.Position;
                }
                else
                {
                    ltb = false;
                    _t = new Gbi.gsDPSetTextureImage(t_all[t].TextureImage.Instruction.String);
                    _t.Data.Segment = Program.OutputSegment;
                    _t.Data.Offset = (int)f.BaseStream.Position;
                }

                // Modify Instruction Offsets
                total = dls.Count();
                while (total > 0)
                {
                    --total;
                    for (int i = 0; i < dls[total].Instructions.Count(); i++)
                    {
                        if (dls[total].Instructions[i].String == t_all[t].Instruction.String)
                        {
                            dls[total].Instructions[i] = new Gbi.Instruction(_t.ToString());
                            map.Add(System.String.Format("0x{0:X2}{1:X6} -> 0x{2:X2}{3:X6} (Texture)", t_all[t].TextureImage.Data.Segment, t_all[t].TextureImage.Data.Offset, Program.OutputSegment, (int)f.BaseStream.Position));
                        }

                        if (dls[total].Instructions[i].String == t_all[t].TextureImage.Instruction.String)
                        {
                            dls[total].Instructions[i] = new Gbi.Instruction(_t.ToString());
                            map.Add(System.String.Format("0x{0:X2}{1:X6} -> 0x{2:X2}{3:X6} (Texture)", t_all[t].TextureImage.Data.Segment, t_all[t].TextureImage.Data.Offset, Program.OutputSegment, (int)f.BaseStream.Position));
                        }
                    }
                }
                f.Write(t_all[t].TextureImage.Data.Block);
            }

            p = p_all.Count();
            while (p > 0)
            {
                --p;
                Gbi.gsDPLoadTLUT _p = new Gbi.gsDPLoadTLUT(p_all[p].Instruction.String);
                _p.Data.Segment = Program.OutputSegment;
                _p.Data.Offset = (int)f.BaseStream.Position;

                // Modify Instruction Offsets
                total = dls.Count();
                while (total > 0)
                {
                    --total;
                    for (int i = 0; i < dls[total].Instructions.Count(); i++)
                    {
                        if (dls[total].Instructions[i].String == p_all[p].Instruction.String)
                        {
                            dls[total].Instructions[i] = new Gbi.Instruction(_p.ToString());
                            map.Add(System.String.Format("0x{0:X2}{1:X6} -> 0x{2:X2}{3:X6} (Palette)", p_all[p].Data.Segment, p_all[p].Data.Offset, _p.Data.Segment, _p.Data.Offset));
                        }
                    }
                }
                f.Write(p_all[p].Data.Block);
            }

            // Write Vertex Data and Display Lists
            total = dls.Count();
            while (total > 0)
            {
                --total;
                int v = dls[total].VertexBuffers.Count();

                while (v > 0)
                {
                    --v;
                    Gbi.gsSPVertex _v = new Gbi.gsSPVertex(dls[total].VertexBuffers[v].Instruction.String);
                    _v.Data.Segment = Program.OutputSegment;
                    _v.Data.Offset = (int)f.BaseStream.Position;

                    // Modify Instruction Offsets
                    for (int i = 0; i < dls[total].Instructions.Count(); i++)
                    {
                        if (dls[total].Instructions[i].String == dls[total].VertexBuffers[v].Instruction.String)
                        {
                            dls[total].Instructions[i] = new Gbi.Instruction(_v.ToString());
                            map.Add(System.String.Format("0x{0:X2}{1:X6} -> 0x{2:X2}{3:X6} (Vertex Data)", dls[total].VertexBuffers[v].Data.Segment, dls[total].VertexBuffers[v].Data.Offset, _v.Data.Segment, _v.Data.Offset));
                        }
                    }
                    f.Write(dls[total].VertexBuffers[v].Data.Block);
                }

                map.Add(System.String.Format("0x{0:X2}{1:X6} -> 0x{2:X2}{3:X6} (Display List)", dls[total].Segment, dls[total].Offset, Program.OutputSegment, (int)f.BaseStream.Position));
                f.Write(Gfx.Assemble(Restring(dls[total])));
            }
            map = map.Distinct().ToList();
            if (Program.ExportMap)
                foreach (string item in map) { Console.WriteLine(item); };
            return 0; // Success
        }
    }
}
