using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using WebSocketSharp;
using Newtonsoft.Json;
using Pr0MinerSharp.Utils;

namespace Pr0MinerSharp
{
    internal class Program
    {
        private static Uri _minerEp = new Uri("ws://miner.pr0gramm.com:8044");

        private static void Main(string[] args)
        {
            Pr0Main.Init();
            XMain.Init();

            Console.WriteLine("Proxy Online!");
            Console.WriteLine("Enter 'exit' to escape");

            while (Console.ReadLine() != "exit") { }
        }
    }
}