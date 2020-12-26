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
            gfxdis.FileName = Program.Conf.GfxDisPath;
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
                sb.Append(input[d].ToString("X2"));

            // Use gfxdis
            ProcessStartInfo gfxdis = new ProcessStartInfo();
            gfxdis.FileName = Program.Conf.GfxDisPath;
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
        public static string Disassemble(params string[] stdin)
        {
            // Convert byte array to a string.
            StringBuilder sb = new StringBuilder();
            for (int d = 0; d < stdin.Length; d++)
                sb.Append(stdin[d]);

            // Use gfxdis
            ProcessStartInfo gfxdis = new ProcessStartInfo();
            gfxdis.FileName = Program.Conf.GfxDisPath;
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
            gfxasm.FileName = Program.Conf.GfxAsmPath;
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
}
