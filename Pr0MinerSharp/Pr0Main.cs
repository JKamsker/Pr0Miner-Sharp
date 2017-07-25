using System;
using Newtonsoft.Json;
using Pr0MinerSharp.DataTypes;
using WebSocketSharp;

namespace Pr0MinerSharp
{
    public class Pr0Main
    {
        public delegate void OnNewJobReceived(NewJob s);

        public static event OnNewJobReceived OnNewJob;

        public static NewJob LastNewJob;

        private static WebSocket _ws;

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
            if (_ws != null)
            {
                _ws.OnMessage -= Ws_OnMessage;
                _ws.Close();
            }

            _ws = new WebSocket("ws://miner.pr0gramm.com:8044");
            _ws.OnMessage += Ws_OnMessage;
            _ws.Connect();
        }

        private static void Ws_OnMessage(object sender, MessageEventArgs e)
        {
            var erg = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(e.Data);

            switch (erg["type"].ToString())
            {
                case "job":
                    var jObject = erg["params"].ToObject<NewJob>();
                    LastNewJob = jObject;
                    OnNewJob?.Invoke(jObject);

                    Console.WriteLine($"NewJob - {jObject.job_id} ");
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
}