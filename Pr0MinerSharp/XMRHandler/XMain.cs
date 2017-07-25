using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Pr0MinerSharp.Pr0Handler;
using Pr0MinerSharp.Utils;

namespace Pr0MinerSharp.XMRHandler
{
    internal class XMain
    {
        private static Socket _serverSocket;

        public static void Init()
        {
            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) { Blocking = false, NoDelay = true };
            _serverSocket.Bind(new IPEndPoint(IPAddress.Any, 3333));
            _serverSocket.Listen((int)SocketOptionName.MaxConnections);
            for (int i = 0; i < 20; i++) _serverSocket.BeginAccept(AcceptCallback, _serverSocket);
        }

        private static void AcceptCallback(IAsyncResult ar)
        {
            Console.WriteLine("New connection");
            var connection = new ConnectionInfo();
            try
            {
                var state = ar.AsyncState as Socket;
                if (state != null)
                {
                    _serverSocket.BeginAccept(AcceptCallback, _serverSocket);
                    connection.Socket = state.EndAccept(ar);
                    connection.Socket.BeginReceive(connection.Buffer, 0, connection.Buffer.Length, SocketFlags.None, ReceiveCallback, connection);
                }
            }
            catch (SocketException exc)
            {
                CloseConnection(connection);
                Console.WriteLine("Socket exception: " + exc.SocketErrorCode);
            }
            catch (Exception exc)
            {
                CloseConnection(connection);
                Console.WriteLine("Exception: " + exc);
            }
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            // Console.WriteLine("Received smth");
            ConnectionInfo connection = (ConnectionInfo)ar.AsyncState;
            try
            {
                int bytesRead = connection.Socket.EndReceive(ar);
                if (bytesRead == 0) return;
                //byte[] newByte = new byte[bytesRead];
                //Array.Copy(connection.Buffer, 0, newByte, 0, bytesRead);

                //String method = json.getString("method");
                //JSONObject params = json.getJSONObject("params");
                //int id = json.getInt("id");
                bool oneGood = false;
                var str = connection.Buffer.GetString(0, bytesRead).Split('\n');
                foreach (var s in str)
                {
                    if (XJson.TryParseJson(s, out var parsed))
                    {
                        oneGood = true;
                        Handle(connection, parsed);

                        connection.Socket?.BeginReceive(connection.Buffer, 0, 1024, SocketFlags.None, ReceiveCallback, connection);
                    }
                }
                if (!oneGood)
                {
                    Console.WriteLine("---------------------------------");
                    Console.WriteLine("Can't handle Json");
                    Console.WriteLine(string.Join("\n", str));
                    Console.WriteLine("---------------------------------");
                    connection.Dispose();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public static void Handle(ConnectionInfo cInfo, JObject desObj)
        {
            switch (desObj["method"].ToString())
            {
                case "login":
                    desObj["params"].ToObject<XLoginObject>().Handle(cInfo);
                    //var lObj = desObj["params"].ToObject<XLoginObject>();
                    //Handle(cInfo, lObj);

                    break;

                case "submit":
                    desObj["params"].ToObject<XResultObject>().Handle(cInfo);
                    break;

                default:
                    Console.WriteLine($"Unknown client Input {desObj["method"]} ({cInfo.Buffer.GetString()})");
                    break;
            }
        }

        public static void Handle(ConnectionInfo cInfo, XLoginObject input)
        {
        }

        public static void CloseConnection(ConnectionInfo session)
        {
            if (session == null) return;
            try
            {
                lock (session)
                {
                    if (session.Socket?.Connected == true)
                    {
                        session.Socket.Close();
                        session.Socket.Dispose();
                        session.Socket = null;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Couldn't safely close socket" + e);
            }
        }
    }

    public class XLoginObject
    {
        public string login { get; set; }
        public string pass { get; set; }
        public string agent { get; set; }
    }

    public class XResultObject
    {
        public string id { get; set; }
        public string job_id { get; set; }
        public string nonce { get; set; }
        public string result { get; set; }
    }

    public class ConnectionInfo
    {
        public Socket Socket;

        // public Pr0Main Pr0Handler;
        public byte[] Buffer = new byte[1024 * 4];

        public string Pr0User
        {
            get => String.IsNullOrEmpty(_pr0User) ? "WeLoveBurgers" : _pr0User;
            set => _pr0User = value;
        }

        private string _pr0User;

        public int Counter { get; set; } = 1;
        private bool _isDisposed = false;

        public bool LoginCompleted = false;

        public void Dispose()
        {
            if (_isDisposed) return;
            Console.WriteLine("Closing connection..");
            //  Pr0Handler.Dispose();
            Socket?.Close();
            Socket?.Dispose();
            Socket = null;
        }

        //public byte[] RemoteBuffer = new byte[1024];
    }
}