using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebSocketSharp;

namespace Pr0MinerSharp.Pr0Handler
{
    public class Pr0Main
    {
        private static WebSocket _ws;

        public static Action<NewJob> OnNewJobReceived;

        //public Pr0Main(Action<NewJob> newJobReceived)
        //{
        //    //string loginUser,
        //    //_pr0User = loginUser;

        //    OnNewJobReceived = newJobReceived;
        //    _ws = new WebSocket("ws://miner.pr0gramm.com:8044");
        //    _ws.OnMessage += Ws_OnMessage;
        //    _ws.Connect();

        //    // Console.ReadKey(true);
        //    // Console.WriteLine("WebSocket Connected");
        //}

        public static void Init()
        {
            _ws = new WebSocket("ws://miner.pr0gramm.com:8044");
            _ws.OnMessage += Ws_OnMessage;
            _ws.Connect();
        }

        public static void Dispose()
        {
            try
            {
                _ws.Close();
            }
            catch (Exception e)
            {
                //Ignored
            }
        }

        public static bool Send(string toSend)
        {
            if (_ws?.IsAlive ?? false)
            {
                _ws.SendAsync(toSend, null);
                return true;
            }
            return false;
        }

        public static bool Send(object toSend)
        {
            if (toSend == null) return false;
            return Send(JsonConvert.SerializeObject(toSend));
        }

        public static Queue<NewJob> JobQueue = new Queue<NewJob>();

        private static void Ws_OnMessage(object sender, MessageEventArgs e)
        {
            var erg = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(e.Data);
            //Console.WriteLine(erg["ASD"]);

            //   Console.WriteLine(e.Data);

            switch (erg["type"].ToString())
            {
                case "job":
                    var jObject = erg["params"].ToObject<NewJob>();

                    JobQueue.Enqueue(jObject);
                    Console.WriteLine($"NewJob - {jObject.job_id} ({JobQueue.Count} Jobs in Queue)");
                    // OnNewJobReceived?.Invoke(jObject);
                    break;

                case "pool_stats":
                    var sObject = erg["params"].ToObject<PoolStats>();
                    Console.WriteLine($"pool_stats: {sObject.hashes:#.00} H/s");
                    break;

                case "job_accepted":

                    var cObject = erg["params"].ToObject<JobAccepted>();
                    Console.WriteLine($"Received job_accepted {cObject.shares} Shares - {(cObject.shares * 0.05)} pr0sec");
                    break;

                default:
                    Console.WriteLine($"Received unknown ({e.Data})");
                    break;
            }
        }
    }

    public class JobAccepted
    {
        public int shares { get; set; }
    }
}