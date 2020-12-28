using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ZeldaObjectManager
{
    class Configuration
    {
        private static string gfxdis = String.Empty;
        private static string gfxasm = String.Empty;
        private readonly string[] conf = File.ReadAllLines("conf.ini");

        public class ZeldaObjectProperties
        {
            public Segment.Address OutputAddress { get; set; }
            public bool ExportMap { get; set; }
            public bool PadOutput { get; set; }
            public string EmbedFile { get; set; }
            public string OutputFile { get; set; }

            public ZeldaObjectProperties()
            {
                OutputAddress = new Segment.Address(0x06000000);
                ExportMap = false;
                PadOutput = false;
                EmbedFile = String.Empty;
                OutputFile = String.Empty;
            }
        }

        public enum ErrorCode
        {
            NO_ARGS = 0,
            NO_OUTPUT = 1,
            NO_INPUT = 2,
            EXIT
        }

        public string GfxDisPath
        {
            get { return gfxdis; }
        }

        public string GfxAsmPath
        {
            get { return gfxasm; }
        }

        public Configuration()
        {
            Parse(conf);
        }

        private static void Parse(string[] l)
        {
            for (int i = 0; i < l.Count(); i++)
            {
                l[i] = l[i].Replace(";", String.Empty);

                if (l[i].Contains("#"))
                    continue;

                if (l[i].Contains("gfxdis"))
                    gfxdis = l[i].ParseArgumentFile();

                if (l[i].Contains("gfxasm"))
                    gfxasm = l[i].ParseArgumentFile();
            }
        }
    }
}
