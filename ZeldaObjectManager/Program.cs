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

            // export obj; zobj
            // reskin player; npc

            args = new string[9];
            args[0] = "-s04=../objects/oot-gkeep.zobj";
            args[1] = "-s06=../objects/adult-bank.zobj";
            args[2] = "0x060168E8";
            args[3] = "0x06016E48";
            args[4] = "0x06018BC0";
            args[5] = "-s06=../objects/child-bank.zobj";
            args[6] = "0x060183C8";
            args[7] = "0x06018678";
            args[8] = "0x06019D58";

            List<byte[]> Segments = new List<byte[]>();
            while (Segments.Count() < 16)
                Segments.Add(new byte[0]);

            List<DisplayList> DisplayLists = new List<DisplayList>();

            for (int ap = 0; ap < args.Length; ap++)
            {
                Console.Write("{0} ", args[ap]);
                // Segments
                if (args[ap].Contains("-s"))
                {
                    string[] FilePath = args[ap].Split('=');
                    int seg = Convert.ToInt32(FilePath[0].Substring(2, 2), 16);
                    Segments[seg] = File.ReadAllBytes(FilePath[1]);
                }

                if (args[ap].Contains("0x"))
                {
                    int seg = Convert.ToInt32(args[ap].Substring(2, 2), 16);
                    int o = Convert.ToInt32(args[ap].Substring(4, 6), 16);
                    DisplayLists.Add(new DisplayList(Segments[seg], Convert.ToUInt32(args[ap], 16)));
                }
            }
            Console.Write("\n");

            using (BinaryWriter f = new BinaryWriter(File.Create("out.zobj")))
            {
                DisplayList.ExportBinary(f, DisplayLists.ToArray());
            }

            Console.Write("Done\n");
            Console.ReadKey();
        }
    }
}
