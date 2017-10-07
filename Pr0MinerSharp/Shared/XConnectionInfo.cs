using System;
using System.Net.Sockets;

namespace Pr0MinerSharp.Shared
{
    public class ConnectionInfo
    {
        public Socket Socket;

        public byte[] Buffer = new byte[4 * 1024];

        public string LastJobId { get; set; }

        public string Pr0User
        {
            get { return string.IsNullOrEmpty(_pr0User) ? "WeLoveBurgers" : _pr0User; }
            set { this._pr0User = value; }
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
    }
}