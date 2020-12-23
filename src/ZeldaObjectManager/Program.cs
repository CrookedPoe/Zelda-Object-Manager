using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace ZeldaObjectManager
{
    class Program
    {
        static void Main(string[] args)
        {
            /* Purely for debugging. */
            args = new string[] { "export", "zobj", "-o=testcfg.out.txt", "-s06=../objects/adult-bank.zobj", "0x060168E8", "0x06016E48", "0x06018BC0" };

            List<byte[]> Segments = new List<byte[]>();
            while (Segments.Count() < 16)
                Segments.Add(new byte[0]);

            List<DisplayList> DisplayLists = new List<DisplayList>();

            string[] FilePath = new string[0];
            for (int ap = 0; ap < args.Length; ap++)
                Console.Write("{0} ", args[ap]);
            Console.Write("\n");

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
                        bool[] SegmentLoaded = new bool[16];
                        bool ExportMap = false;
                        bool EmbedCfg = false;
                        string[] Output = new string[2];
                        int i = (ap + 1);
                        while (i < args.Length)
                        {
                            if (args[i].Contains("-o"))
                            {
                                FilePath = args[i].Split('=');
                                Output[0] = FilePath[1]; // Filepath
                                Output[1] = FilePath[1].ParseFileName(); // Filename without extension
                            }

                            if (args[i].Contains("-s"))
                            {
                                FilePath = args[i].Split('=');
                                int seg = Convert.ToInt32(FilePath[0].Substring(2, 2), 16);
                                SegmentLoaded[seg] = true;
                                //Segments[seg] = File.ReadAllBytes(FilePath[1]);
                            }

                            if (args[i].Contains("0x"))
                            {
                                int seg = Convert.ToInt32(args[i].Substring(2, 2), 16);
                                //int o = Convert.ToInt32(args[i].Substring(4, 6), 16);
                                //DisplayLists.Add(new DisplayList(Segments[seg], Convert.ToUInt32(args[i], 16)));
                            }

                            i++;
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
