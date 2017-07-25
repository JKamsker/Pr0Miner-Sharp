using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Pr0MinerSharp.DataTypes;

namespace Pr0MinerSharp.Utils
{
    public static class XHandleExt
    {
        private const string RndId = "479e24d4-e672-4527-9fb7-49b595099a53";

        public static void Handle(this XLoginObject input, XConnectionInfo cInfo)
        {
            if (input == null)
            {
                Console.WriteLine("ERR! Empty Login Input!");
                return;
            }
            cInfo.Pr0User = input.login;
            var lnj = Pr0Main.LastNewJob;

            cInfo.Send(new
            {
                result = new
                {
                    id = RndId,
                    job = new
                    {
                        lnj.blob,
                        lnj.job_id,
                        id = RndId,
                        lnj.target
                    },
                    status = "OK"
                },
                id = cInfo.Counter++,
                error = (string)null,
                jsonrpc = "2.0"
            });

            Pr0Main.OnNewJob += x => cInfo.Send(new { method = "job", jsonrpc = "2.0", @params = new { x.blob, x.job_id, x.target, id = RndId } });
            Console.WriteLine($"New login ({input.login}/{input.agent})");
        }

        public static void Handle(this XResultObject input, XConnectionInfo cInfo)
        {
            if (input == null)
            {
                Console.WriteLine("ERR! Empty Result Input!");
                return;
            }
            cInfo.Send(new { result = new { status = "OK" }, id = cInfo.Counter++, jsonrpc = "2.0" });

            Pr0Main.Send(new { type = "submit", @params = new { user = cInfo.Pr0User, input.job_id, input.nonce, input.result } });
        }

        public static void Send(this XConnectionInfo cInfo, object toSend)
        {
            if (cInfo?.Socket == null || !cInfo.Socket.Connected)
            {
                cInfo?.Dispose();
            }
            else
            {
                var respBytes = (JsonConvert.SerializeObject(toSend) + "\n").GetBytes();
                cInfo.Socket.BeginSend(respBytes, 0, respBytes.Length, SocketFlags.None, null, null);
            }
        }
    }
}

/* Loginresponse...
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
           */