﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace ZeldaObjectManager
{
    class Utility
    {
        public static string GfxDis(byte[] input)
        {
            // Convert byte array to a string.
            StringBuilder sb = new StringBuilder();
            for (int d = 0; d < input.Length; d++)
                sb.Append(String.Format("{0:X2}", input[d]));

            // Use gfxdis
            ProcessStartInfo gfxdis = new ProcessStartInfo();
            gfxdis.FileName = "../apps/gfxdis.f3dex2";
            gfxdis.Arguments = String.Format("-d {0}", sb.ToString());
            gfxdis.UseShellExecute = false;
            gfxdis.RedirectStandardOutput = true;

            using (Process process = Process.Start(gfxdis))
            {
                using (StreamReader sr = process.StandardOutput)
                {
                    string stdout = sr.ReadToEnd();//.Split('!');
                    stdout = stdout.Replace(" ", String.Empty); // Remove Whitespace
                    stdout = stdout.Replace("|", " | "); // ** Except for on | characters.
                    return stdout;
                }
            }
        }
        public static byte[] GfxAsm(string[] input)
        {
            // Execute gfxasm
            string[] stderr = new string[0];
            List<byte> bytes = new List<byte>();
            ProcessStartInfo gfxasm = new ProcessStartInfo();
            gfxasm.FileName = "../apps/gfxasm.f3dex2.exe";
            gfxasm.UseShellExecute = false;
            gfxasm.RedirectStandardInput = true;
            gfxasm.RedirectStandardError = true;

            using (Process proc = Process.Start(gfxasm))
            {
                // Feed Input
                using (StreamWriter sw = proc.StandardInput)
                {
                    for (int i = 0; i < input.Count(); i++)
                    {
                        sw.WriteLine(input[i] + ";");
                    }
                }

                // Read Output
                using (StreamReader sr = proc.StandardError)
                {
                    stderr = sr.ReadToEnd().Split('\n');
                }
            }

            // Clean up output
            string[] _stderr = new string[stderr.Length - 8];
            for (int l = 0; l < _stderr.Length; l++)
            {
                _stderr[l] = stderr[l + 7];
                _stderr[l] = _stderr[l].Replace(" ", String.Empty); // Remove Whitespace
                _stderr[l] = _stderr[l].Replace("\n", String.Empty); // Remove Newlines
                _stderr[l] = _stderr[l].Replace("\r", String.Empty); // Remove Carriage Returns
                //Console.Write(_stderr[l]);
                for (int j = 0; j < _stderr[l].Length; j += 2)
                {
                    byte _b = Convert.ToByte(String.Format("{0}{1}", _stderr[l][j], _stderr[l][j + 1]), 16);
                    bytes.Add(_b);
                    //Console.Write("{0:X2} ", _b);
                }
            }

            return bytes.ToArray();
        }

        public static byte[] ByteCopy(byte[] src, int start, int size)
        {
            byte[] output = new byte[size];
            for (int i = 0; i < size; i++)
            {
                output[i] = src[i + start];
            }
            return output;
        }
    }
}