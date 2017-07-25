using System;
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
        private WebSocket _ws;

        public Action<NewJob> OnNewJobReceived;

        public string Pr0User
        {
            get => String.IsNullOrEmpty(_pr0User) ? "WeLoveBurgers" : _pr0User;
            set => _pr0User = value;
        }

        private string _pr0User;

        public Pr0Main(string loginUser, Action<NewJob> newJobReceived)
        {
            _pr0User = loginUser;
            OnNewJobReceived = newJobReceived;
            _ws = new WebSocket("ws://miner.pr0gramm.com:8044");
            _ws.OnMessage += Ws_OnMessage;
            _ws.Connect();

            // Console.ReadKey(true);
            // Console.WriteLine("WebSocket Connected");
        }

        private void Init()
        {
        }

        public void Dispose()
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

        public bool Send(string toSend)
        {
            if (_ws?.IsAlive ?? false)
            {
                _ws.SendAsync(toSend, null);
                return true;
            }
            return false;
        }

        public bool Send(object toSend)
        {
            if (toSend == null) return false;
            return Send(JsonConvert.SerializeObject(toSend));
        }

        private void Ws_OnMessage(object sender, MessageEventArgs e)
        {
            var erg = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(e.Data);
            //Console.WriteLine(erg["ASD"]);

            //   Console.WriteLine(e.Data);

            switch (erg["type"].ToString())
            {
                case "job":
                    var jObject = erg["params"].ToObject<NewJob>();
                    Console.WriteLine($"{Pr0User}: NewJob ({jObject.job_id})");

                    OnNewJobReceived?.Invoke(jObject);
                    break;

                case "pool_stats":
                    var sObject = erg["params"].ToObject<PoolStats>();
                    // Console.WriteLine("Received pool_stats");
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