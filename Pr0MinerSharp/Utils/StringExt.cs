using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pr0MinerSharp.Utils
{
    internal static class StringExt
    {
        public static string GetString(this byte[] inBytes) => Encoding.Default.GetString(inBytes);

        public static string GetString(this byte[] inBytes, int index, int count) => Encoding.Default.GetString(inBytes, index, count);

        public static byte[] GetBytes(this string inString) => Encoding.Default.GetBytes(inString);
    }
}