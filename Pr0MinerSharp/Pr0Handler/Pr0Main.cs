using System;
using System.Collections.Generic;
using System.IO;
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

        //  public static Action<NewJob> OnNewJobReceived;
        //   public static Queue<NewJob> JobQueue = new Queue<NewJob>();

        public static void Init()
        {
            _ws = new WebSocket("ws://miner.pr0gramm.com:8044");
            _ws.OnMessage += Ws_OnMessage;
            _ws.Connect();

            //if (File.Exists("SavedJobQueue.json"))
            //{
            //    JobQueue = JsonConvert.DeserializeObject<Queue<NewJob>>(File.ReadAllText("SavedJobQueue.json"));
            //}
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

        public static bool Send(object toSend)
        {
            if (toSend == null) return false;
            return Send(JsonConvert.SerializeObject(toSend));
        }

        public static bool Send(string toSend)
        {
            if (_ws != null && _ws.IsAlive)
            {
                lock (_ws)
                    _ws.SendAsync(toSend, null);
                return true;
            }
            else
            {
                //Reconnect
                Console.WriteLine("Ws is offline!!");
                Reconnect();
            }

            if (_ws != null && _ws.IsAlive)
            {
                lock (_ws)
                    _ws.SendAsync(toSend, null);
                return true;
            }
            else
            {
                Console.WriteLine("Ws still offline, please do smth!");
            }

            return false;
        }

        public static void Reconnect()
        {
            Console.WriteLine("Trying ws reconnect");
            msgCounter = 0;
            if (_ws != null)
            {
                _ws.OnMessage -= Ws_OnMessage;
                _ws.Close();
            }

            _ws = new WebSocket("ws://miner.pr0gramm.com:8044");
            _ws.OnMessage += Ws_OnMessage;
            _ws.Connect();
        }

        public delegate void OnNewJobReceived(NewJob s);

        public static event OnNewJobReceived OnNewJob;

        public static NewJob LastNewJob;

        private static void Ws_OnMessage(object sender, MessageEventArgs e)
        {
            var erg = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(e.Data);
            //Console.WriteLine(erg["ASD"]);

            //   Console.WriteLine(e.Data);

            switch (erg["type"].ToString())
            {
                case "job":
                    var jObject = erg["params"].ToObject<NewJob>();
                    LastNewJob = jObject;
                    OnNewJob?.Invoke(jObject);
                    //if (!JobQueue.Any(m => m.job_id == jObject.job_id))
                    //{
                    //    JobQueue.Enqueue(jObject);
                    //    SaveJobQueue();
                    //}

                    Console.WriteLine($"NewJob - {jObject.job_id} ");//({JobQueue.Count} Jobs in Queue)
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

            msgCounter++;
            if (msgCounter >= 50)
                Reconnect();
        }

        private static int msgCounter = 0;

        public static void SaveJobQueue()
        {
            try
            {
                // File.WriteAllText("SavedJobQueue.json", JsonConvert.SerializeObject(JobQueue));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    public class JobAccepted
    {
        public int shares { get; set; }
    }
}