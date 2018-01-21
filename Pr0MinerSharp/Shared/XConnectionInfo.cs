using System;
using System.Net.Sockets;
using Newtonsoft.Json;
using Pr0MinerSharp.Utils;

namespace Pr0MinerSharp.Shared
{
    public class ConnectionInfo
    {
        public object LockObject { get; } = new object();

        public Socket Socket;

        public byte[] Buffer = new byte[4 * 1024];

        public string LastJobId { get; set; }

        public string Pr0User
        {
            get => string.IsNullOrEmpty(_pr0User) ? "WeLoveBurgers" : _pr0User;
            set => this._pr0User = value;
        }

        private string _pr0User;

        public int Counter { get; set; } = 1;
        private bool _isDisposed = false;

        public bool LoginCompleted = false;

        public void Dispose()
        {
            if (_isDisposed) return;
            Console.WriteLine("Closing connection..");
            try
            {
                Socket?.Close();
                Socket?.Dispose();
                Socket = null;
            }
            catch (Exception)
            {
            }

            _isDisposed = true;
        }

        public void Send(object toSend)
        {
            if (Socket == null || Socket.Connected)
            {
                Dispose();
            }
            else
            {
                var respBytes = (JsonConvert.SerializeObject(toSend) + "\n").GetBytes();
                Socket.BeginSend(respBytes, 0, respBytes.Length, SocketFlags.None, null, null);
            }
        }

        public void Send(Job job)
        {
            if (LastJobId == job.job_id) return;
            LastJobId = job.job_id;

            var cJObj = new
            {
                method = "job",
                jsonrpc = "2.0",
                @params = new { job.blob, job.job_id, job.target, id = XHandleExt.RndId }
            };

            Send(cJObj);
        }
    }
}