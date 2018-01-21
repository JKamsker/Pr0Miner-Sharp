using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json.Linq;
using Pr0MinerSharp.Shared;
using Pr0MinerSharp.Utils;

namespace Pr0MinerSharp
{
    // ReSharper disable once InconsistentNaming
    internal class Pr0xyServerSocket
    {
        public const string RndId = "479e24d4-e672-4527-9fb7-49b595099a53";

        private Socket _serverSocket;
        private Pr0GrammApi _api;

        public List<ConnectionInfo> ConnectedEndpoints { get; private set; }

        public Pr0xyServerSocket(bool autostart = false)
        {
            ConnectedEndpoints = new List<ConnectionInfo>();
            _api = new Pr0GrammApi { OnNewJob = OnNewJob };

            if (autostart)
                Start();
        }

        public void Start()
        {
            _api.Start();
            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) { Blocking = false, NoDelay = true };
            _serverSocket.Bind(new IPEndPoint(IPAddress.Any, 3333));
            _serverSocket.Listen((int)SocketOptionName.MaxConnections);
            for (var i = 0; i < 20; i++) _serverSocket.BeginAccept(AcceptCallback, _serverSocket);
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            Console.WriteLine("New connection");
            var cInfo = new ConnectionInfo();
            try
            {
                if (ar.AsyncState is Socket state)
                {
                    ConnectedEndpoints.Add(cInfo);

                    _serverSocket.BeginAccept(AcceptCallback, _serverSocket);
                    cInfo.Socket = state.EndAccept(ar);
                    cInfo.Socket.BeginReceive(cInfo.Buffer, 0, cInfo.Buffer.Length, SocketFlags.None, ReceiveCallback, cInfo);
                }
            }
            catch (SocketException exc)
            {
                CloseConnection(cInfo);
                Console.WriteLine("Socket exception: " + exc.SocketErrorCode);
            }
            catch (Exception exc)
            {
                CloseConnection(cInfo);
                Console.WriteLine("Exception: " + exc);
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            var connection = (ConnectionInfo)ar.AsyncState;
            lock (connection.LockObject)
            {
                try
                {
                    var bytesRead = connection.Socket.EndReceive(ar);
                    if (bytesRead == 0)
                    {
                        connection.Dispose();
                        return;
                    }

                    var oneGood = false;
                    var receivedQuery = connection.Buffer.GetString(0, bytesRead).Split('\n');
                    foreach (var s in receivedQuery)
                    {
                        if (JsonValidator.TryParseJson(s, out var parsed))
                        {
                            oneGood = true;
                            Handle(connection, parsed);

                            connection.Socket?.BeginReceive(connection.Buffer, 0, connection.Buffer.Length, SocketFlags.None, ReceiveCallback, connection);
                        }
                    }

                    if (!oneGood)
                    {
                        Console.WriteLine("---------------------------------");
                        Console.WriteLine("Can't handle Json");
                        Console.WriteLine(string.Join("\n", receivedQuery));
                        Console.WriteLine("---------------------------------");
                        connection.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        private void Handle(ConnectionInfo cInfo, JObject desObj)
        {
            switch (desObj["method"].ToString())
            {
                case "login":

                    DoLogin(cInfo, desObj["params"].ToObject<XLoginObject>());
                    break;

                case "submit":
                    DoSubmit(cInfo, desObj["params"].ToObject<XResultObject>());//.Handle(cInfo);
                    break;

                default:
                    Console.WriteLine($"Unknown client Input {desObj["method"]} ({cInfo.Buffer.GetString()})");
                    break;
            }
        }

        private readonly object _lockObject = new object();

        private void DoLogin(ConnectionInfo cInfo, XLoginObject input)
        {
            var job = _api.LastJob;

            cInfo.Pr0User = input.login;
            cInfo.LastJobId = job.job_id;

            cInfo.Send(new
            {
                result = new
                {
                    id = RndId,
                    job = new
                    {
                        job.blob,
                        job.job_id,
                        id = RndId,
                        job.target
                    },
                    status = "OK"
                },
                id = cInfo.Counter++,
                error = (string)null,
                jsonrpc = "2.0"
            });

            lock (_lockObject)
            {
                ConnectedEndpoints.Add(cInfo);
            }
        }

        private void DoSubmit(ConnectionInfo cInfo, XResultObject resObject)
        {
            lock (cInfo.LockObject)
            {
                cInfo.Send(new { result = new { status = "OK" }, id = cInfo.Counter++, jsonrpc = "2.0" });
            }

            _api.Send(new { type = "submit", @params = new { user = cInfo.Pr0User, resObject.job_id, resObject.nonce, resObject.result } });
        }

        public void OnNewJob(Job job)
        {
            lock (_lockObject)
            {
                ConnectedEndpoints.Where(m => m != null && m.Socket.Connected && m.LastJobId != job.job_id).Send(job);
            }
        }

        public void CloseConnection(ConnectionInfo session)
        {
            lock (_lockObject)
            {
                if (session == null) return;
                ConnectedEndpoints.Remove(session);
                try
                {
                    if (session.Socket?.Connected == true)
                    {
                        session.Socket.Close();
                        session.Socket.Dispose();
                        session.Socket = null;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Couldn't safely close socket" + e);
                }
            }
        }
    }
}