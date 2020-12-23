using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZeldaObjectManager
{
    public static class Extensions
    {
        public static bool CommandOption(this String str, string c)
        {
            return str == c;
        }

        public static string ParseFileName(this String str)
        {
            int head = 0;
            int tail = str.Length - 1;

            while (str[tail] != '.')
                tail--;

            while ((str[head] != '/' || str[head] != '\\') && (head < str.Length - 1))
                head++;

            if (head == str.Length - 1)
                head = 0;

            return str.Substring(head, tail);
        }
    }
}
