using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace ZeldaObjectManager
{
    class SegmentObject
    {
        public bool Occupied;
        public string FilePath;
        public byte[] DataBlock;

        public SegmentObject()
        {
            FilePath = String.Empty;
            DataBlock = new byte[0];
            Occupied = false;
        }

        public SegmentObject(string file)
        {
            FilePath = file;
            DataBlock = File.ReadAllBytes(FilePath);
            Occupied = true;
        }
    }
    class Program
    {
        public static string GfxAsmPath = String.Empty;
        public static string GfxDisPath = String.Empty;
        public static string[] Config = File.ReadAllLines("conf.ini");
        public static Stopwatch ExecuteTime = new Stopwatch();
        public static bool ExportMap = false;
        public static SegmentObject[] Segments = new SegmentObject[16];
        public static int OutputSegment = 0x06;
        static void Main(string[] args)
        {
            // Parse Config File
            for (int i = 0; i < Config.Length; i++)
            {
                if (Config[i].Contains("#"))
                    continue;

                if (Config[i].Contains("gfxdis"))
                {
                    GfxDisPath = Config[i].Split('=')[1];
                    GfxDisPath = GfxDisPath.Replace("\"", String.Empty);
                    GfxDisPath = GfxDisPath.Replace(";", String.Empty);
                }

                if (Config[i].Contains("gfxasm"))
                {
                    GfxAsmPath = Config[i].Split('=')[1];
                    GfxAsmPath = GfxAsmPath.Replace("\"", String.Empty);
                    GfxAsmPath = GfxAsmPath.Replace(";", String.Empty);
                }
            }

            // Initialize Segments
            for (int i = 0; i < Segments.Count(); i++)
                Segments[i] = new SegmentObject();

            List<DisplayList> DisplayLists = new List<DisplayList>();

            string[] FilePath = new string[0];

            for (int ap = 0; ap < args.Length; ap++)
            {
                if (args[ap].CommandOption("export"))
                {
                    if (args[ap + 1].CommandOption("obj"))
                    {
                        ShowUsage(0);
                    }
                    else if (args[ap + 1].CommandOption("objex"))
                    {
                        ShowUsage(0);
                    }
                    else if (args[ap + 1].CommandOption("c"))
                    {
                        ShowUsage(0);
                    }
                    else if (args[ap + 1].CommandOption("zobj"))
                    {
                        ExecuteTime.Start();
                        //bool EmbedCfg = false;
                        string[] Output = new string[2];
                        int i = (ap + 1);
                        while (i < args.Length)
                        {
                            if (args[i] == "-m")
                            {
                                ExportMap = true;
                            }

                            if (args[i].Contains("-o"))
                            {
                                FilePath = args[i].Split('=');

                                if (FilePath[0].Length > 2)
                                    OutputSegment = Convert.ToInt32(FilePath[0].Substring(2, 2), 16);

                                Output[0] = FilePath[1]; // Filepath
                                Output[1] = FilePath[1].ParseFileName(); // Filename without extension
                            }

                            if (args[i].Contains("-s"))
                            {
                                FilePath = args[i].Split('=');
                                int seg = Convert.ToInt32(FilePath[0].Substring(2, 2), 16);
                                Segments[seg] = new SegmentObject(FilePath[1]);
                            }

                            if (args[i].Contains("0x"))
                            {
                                int seg = Convert.ToInt32(args[i].Substring(2, 2), 16);
                                DisplayLists.Add(new DisplayList(Convert.ToUInt32(args[i], 16)));
                            }

                            i++;
                        }
                        /*for (int s = 0; s < Segments.Count(); s++)
                        {
                            if (Segments[s].DataBlock.Length > 0)
                                Console.WriteLine("Segment 0x{0:X2}: 0x{1:X8}", s, Segments[s].DataBlock.Length);
                        }*/
                        using (BinaryWriter f = new BinaryWriter(File.Create(Output[0])))
                        {
                            if (DisplayList.Export(f, DisplayLists.ToArray()) == 0)
                            {
                                ExecuteTime.Stop();
                                Console.WriteLine("{0} generated successfully in {1}ms", Output[0], ExecuteTime.ElapsedMilliseconds);
                            }
                        }
                        ShowUsage(1);
                    }
                    else
                    {
                        ShowUsage(1);
                    }
                }
                else
                {
                    ShowUsage(1);
                }
            }
        }

        static void ShowUsage(int err)
        {
            switch (err)
            {
                case 0: // Unsupported Feature
                    Console.WriteLine("This feature is not currently supported at this time.");
                    Console.WriteLine("Please see the README at http://github.com/CrookedPoe/Zelda-Object-Manager");
                    Console.WriteLine("for more information on this program and what is or is not supported.");
                    break;
                case 1: // Exit Procedure
                    Console.WriteLine("Thanks for choosing to use Zelda-Object-Manager! The program will now exit.");
                    break;
            }

            Console.ReadKey();
            Environment.Exit(0);
        }
    }
}
