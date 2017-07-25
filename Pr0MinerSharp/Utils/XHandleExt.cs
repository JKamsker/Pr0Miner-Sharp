using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Pr0MinerSharp.Pr0Handler;
using Pr0MinerSharp.XMRHandler;

namespace Pr0MinerSharp.Utils
{
    public static class XHandleExt
    {
        private const string rndId = "479e24d4-e672-4527-9fb7-49b595099a53";

        public static void Handle(this XLoginObject input, ConnectionInfo cInfo)
        {
            if (input == null)
            {
                Console.WriteLine("ERR! Empty Login Input!");
                return;
            }
            cInfo.Pr0User = input.login;
            // cInfo.Pr0Handler = new Pr0Main(x => SendLoginResponse(x, cInfo));
            if (Pr0Main.JobQueue.Any())
            {
                SendLoginResponse(Pr0Main.JobQueue.Dequeue(), cInfo);
            }
            else
            {
                Console.WriteLine("ERROR! No Job in queue!!");
                cInfo.Dispose();
            }

            Console.WriteLine($"New login ({input.login}/{input.agent})");
        }

        private static void SendLoginResponse(NewJob x, ConnectionInfo cInfo)
        {
            //var nonce = new string(x.blob.Where((m, i) => i >= 39 && i <= 43).ToArray());
            cInfo.Send(new
            {
                result = new
                {
                    id = rndId,
                    job = new
                    {
                        x.blob,
                        x.job_id,
                        id = rndId,
                        x.target
                    },
                    status = "OK"
                },
                id = cInfo.Counter++,
                error = (string)null,
                jsonrpc = "2.0"
            });
            return;
            //Ignored...
            if (cInfo.LoginCompleted)
            {
                Console.WriteLine("Relaying new job...");
                cInfo.Send(new
                {
                    method = "job",
                    jsonrpc = "2.0",
                    @params = new { x.blob, x.job_id, x.target, id = rndId }
                });
            }
            else
            {
                cInfo.LoginCompleted = true;
                cInfo.Send(new
                {
                    result = new
                    {
                        id = rndId,
                        job = new
                        {
                            x.blob,
                            x.job_id,
                            id = rndId,
                            x.target
                        },
                        status = "OK"
                    },
                    id = cInfo.Counter++,
                    error = (string)null,
                    jsonrpc = "2.0"
                });
            }
        }

        public static void Handle(this XResultObject input, ConnectionInfo cInfo)
        {
            if (input == null)
            {
                Console.WriteLine("ERR! Empty Result Input!");
                return;
            }
            var resp = new { result = new { status = "OK" }, id = cInfo.Counter++, jsonrpc = "2.0" };
            cInfo.Send(resp);

            Pr0Main.Send(new { type = "submit", @params = new { user = cInfo.Pr0User, input.job_id, input.nonce, input.result } });
        }

        public static void Send(this ConnectionInfo cInfo, object toSend)
        {
            if (cInfo?.Socket?.Connected ?? false)
            {
                var respBytes = (JsonConvert.SerializeObject(toSend) + "\n").GetBytes();
                cInfo.Socket.BeginSend(respBytes, 0, respBytes.Length, SocketFlags.None, null, null);
            }
            else
            {
                cInfo?.Dispose();
            }
        }
    }
}