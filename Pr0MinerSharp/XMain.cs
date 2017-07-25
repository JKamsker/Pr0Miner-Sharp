using System;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json.Linq;
using Pr0MinerSharp.DataTypes;
using Pr0MinerSharp.Utils;

namespace Pr0MinerSharp
{
    internal class XMain
    {
        private static Socket _serverSocket;

        public static void Init()
        {
            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) { Blocking = false, NoDelay = true };
            _serverSocket.Bind(new IPEndPoint(IPAddress.Any, 3334));
            _serverSocket.Listen((int)SocketOptionName.MaxConnections);
            for (int i = 0; i < 20; i++) _serverSocket.BeginAccept(AcceptCallback, _serverSocket);
        }

        private static void AcceptCallback(IAsyncResult ar)
        {
            Console.WriteLine("New connection");
            var connection = new XConnectionInfo();
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
            XConnectionInfo connection = (XConnectionInfo)ar.AsyncState;
            try
            {
                int bytesRead = connection.Socket.EndReceive(ar);
                if (bytesRead == 0) return;

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

        public static void Handle(XConnectionInfo cInfo, JObject desObj)
        {
            switch (desObj["method"].ToString())
            {
                case "login":
                    desObj["params"].ToObject<XLoginObject>().Handle(cInfo);
                    break;

                case "submit":
                    desObj["params"].ToObject<XResultObject>().Handle(cInfo);
                    break;

                default:
                    Console.WriteLine($"Unknown client Input {desObj["method"]} ({cInfo.Buffer.GetString()})");
                    break;
            }
        }

        public static void CloseConnection(XConnectionInfo session)
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
}