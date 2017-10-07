using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Pr0MinerSharp.Shared;

namespace Pr0MinerSharp.Utils
{
    public static class XHandleExt
    {
        public const string RndId = "479e24d4-e672-4527-9fb7-49b595099a53";

        public static void Send(this ConnectionInfo cInfo, object toSend)
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

        public static void Send(this ConnectionInfo cInfo, Job job)
        {
            if (cInfo.LastJobId == job.job_id) return;
            cInfo.LastJobId = job.job_id;

            var cJObj = new
            {
                method = "job",
                jsonrpc = "2.0",
                @params = new { job.blob, job.job_id, job.target, id = XHandleExt.RndId }
            };

            cInfo.Send(cJObj);
        }

        public static void Send(this IEnumerable<ConnectionInfo> cInfos, Job job)
        {
            var cJObj = new
            {
                method = "job",
                jsonrpc = "2.0",
                @params = new { job.blob, job.job_id, job.target, id = XHandleExt.RndId }
            };

            foreach (var cInfo in cInfos)
            {
                if (cInfo.LastJobId == job.job_id) continue;
                cInfo.LastJobId = job.job_id;
                cInfo.Send(cJObj);
            }
        }
    }
}