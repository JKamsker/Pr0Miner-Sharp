using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pr0MinerSharp.Shared;
using Pr0MinerSharp.Utils;
using WebSocketSharp;

namespace Pr0MinerSharp
{
    internal class Pr0GrammApi
    {
        public Job LastJob { get; private set; }
        public Action<Job> OnNewJob { get; set; }

        private WebSocket _ws;

        public Pr0GrammApi(bool autostart = false)
        {
            if (autostart) Start();
        }

        public void Start()
        {
            Reconnect();
        }

        public bool Send(object toSend)
        {
            if (toSend == null) return false;
            return Send(JsonConvert.SerializeObject(toSend));
        }

        public bool Send(string toSend)
        {
            while (_ws == null || _ws.IsAlive == false)
            {
                Console.WriteLine("Ws is offline,reconnecting...!");
                Reconnect();
            }

            lock (_ws)
            {
                _ws.SendAsync(toSend, null);
            }

            return true;
        }

        public void Reconnect()
        {
            if (_ws != null)
            {
                Console.WriteLine("Trying ws reconnect");
                _ws.OnMessage -= Ws_OnMessage;
                _ws.Close();
                _ws = null;
            }

            _ws = new WebSocket("ws://miner.pr0gramm.com:8044");
            _ws.OnMessage += Ws_OnMessage;
            _ws.OnError += (_, __) => Reconnect();
            _ws.Connect();
        }

        private void Ws_OnMessage(object sender, MessageEventArgs e)
        {
            var jres = (JObject)JsonConvert.DeserializeObject(e.Data);

            switch (jres["type"].ToString())
            {
                case "job":
                    var jobObject = jres["params"].ToObject<Job>();
                    LastJob = jobObject;
                    OnNewJob?.Invoke(jobObject);

                    Console.WriteLine($"NewJob - {jobObject.job_id} ");
                    break;

                case "pool_stats":
                    var sObject = jres["params"].ToObject<PoolStats>();
                    Console.WriteLine($"pool_stats: {sObject.hashes:#.00} H/s");
                    break;

                case "job_accepted":

                    var cObject = jres["params"].ToObject<JobAccepted>();
                    Console.WriteLine($"Received job_accepted {cObject.shares} Shares - {(cObject.shares * 0.05)} pr0sec");
                    break;

                default:
                    Console.WriteLine($"Received unknown ({e.Data})");
                    break;
            }
        }
    }
}