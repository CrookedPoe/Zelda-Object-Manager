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
        public static Configuration Conf = new Configuration();
        public static Segment.Buffer[] Segments = new Segment.Buffer[16];
        public static Stopwatch ExecutionTime = new Stopwatch();
        public static Segment.Address OutputAddress = new Segment.Address(0x06000000);
        static void Main(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "export")
                {
                    Export(args[i + 1], args);
                }
                else
                    ShowUsage(0);
            }
        }

        static void Export(string output_type, string[] args)
        {
            bool ExportMap = false;
            string EmbedFile = String.Empty;

            // Output File Options
            string OutputFile = String.Empty;
            bool PadOutput = false;

            // Input
            //List<Segment.Address> DisplayListOffsets = new List<Segment.Address>();
            List<DisplayList> DisplayLists = new List<DisplayList>();

            if (output_type == "zobj")
            {
                ExecutionTime.Start();
                for (int i = 2; i < args.Length; i++)
                {
                    if (args[i].Contains("-m"))
                        ExportMap = true;

                    if (args[i].Contains("-e"))
                        EmbedFile = args[i].ParseArgumentFile();

                    if (args[i].Contains("-o"))
                    {
                        string[] o = new string[0];
                        if (args[i].Contains("="))
                        {
                            o = args[i].Split('=');
                            OutputFile = args[i].ParseArgumentFile();
                        }
                        else
                            ShowUsage(Configuration.ErrorCode.EXIT); // No Input

                        for (int j = 0; j < o[0].Length; j++)
                        {
                            if (o[0].Contains("p"))
                                PadOutput = true;

                            if (o[0][j] == '0')
                            {
                                OutputAddress.Index = Convert.ToInt32(String.Format("{0}{1}", o[0][j], o[0][j + 1]), 16);

                                string[] _o;
                                if (PadOutput)
                                    _o = o[0].Split('p');
                                else
                                    _o = o[0].Split('o');

                                OutputAddress = new Segment.Address("0x" + _o[1]);
                            }
                        }
                    }

                    if (args[i].Contains("-s"))
                    {
                        int seg = Convert.ToInt32(args[i].Substring(2, 2), 16);
                        Segments[seg] = new Segment.Buffer(args[i].ParseArgumentFile());
                    }

                    if (args[i].Contains("0x"))
                    {
                        DisplayLists.Add(new DisplayList(Convert.ToUInt32(args[i], 16)));
                    }
                }

                using (BinaryWriter f = new BinaryWriter(File.Create(OutputFile)))
                {
                    if (DisplayList.Export(f, DisplayLists.ToArray()) == 0)
                    {
                        ExecutionTime.Stop();
                        Console.WriteLine("{0} generated in {1}ms", OutputFile, ExecutionTime.ElapsedMilliseconds);
                    }
                }
                ShowUsage(Configuration.ErrorCode.EXIT);
            }
        }

        static void Reskin()
        {

        }

        static void Viewer()
        {

        }

        static void ShowUsage(Configuration.ErrorCode err)
        {
            switch (err)
            {
                case Configuration.ErrorCode.EXIT: // Exit Procedure
                    Console.WriteLine("Thanks for choosing to use Zelda-Object-Manager! The program will now exit.");
                    break;
            }

            Console.ReadKey();
            Environment.Exit(0);
        }
    }
}
