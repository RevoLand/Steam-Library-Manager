using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Steam_Library_Manager.Framework
{
    internal static class Network
    {
        /*
         * WIP
         * The stuff here doesn't represents the things in my mind and subject to change hardly
         * Just skip this file until it's ready, really.
         */
        public class Client
        {
            private Socket ClientSocket;
            private readonly byte[] ClientBuffer = new byte[Definitions.SLM.NetworkBuffer];
            private static readonly object _ClientLock = new object();

            public void ConnectToServer()
            {
                ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                ClientSocket.BeginConnect(new IPEndPoint(IPAddress.Parse(Properties.Settings.Default.IPToConnect), Properties.Settings.Default.PortToConnect), new AsyncCallback(ServerCallback), null);
            }

            private void ServerCallback(IAsyncResult ar)
            {
                try
                {
                    ClientSocket.EndConnect(ar);

                    DoRecvFromServer();
                }
                catch (SocketException sEx)
                {
                    Debug.WriteLine(sEx);
                }
            }

            private void DoRecvFromServer()
            {
                ClientSocket.BeginReceive(ClientBuffer, 0, ClientBuffer.Length, 0, new AsyncCallback(DoReceiveFromServer), null);
            }

            private void DoReceiveFromServer(IAsyncResult ar)
            {
                int BytesToRead = ClientSocket.EndReceive(ar);

                if (BytesToRead > 0)
                {
                    // ClientBuffer, 0, BytesToRead
                    Recv(ClientBuffer, 0, BytesToRead);
                }

                DoRecvFromServer();
            }

            private void Recv(byte[] buffer, int offset, int length)
            {
                Debug.WriteLine($"{buffer.Length} - {offset} - {length}");
                lock (_ClientLock)
                {
                    using (FileStream test = new FileInfo(@"D:\Projeler\Steam\Steam-Library-Manager\Binaries\Logs\test.exe").Open(FileMode.OpenOrCreate))
                    {
                        test.Position = test.Length;

                        test.Write(buffer, 0, length);
                    }
                }

                buffer = null;
            }
        }

        public class Server
        {
            private Socket ServerSocket, ClientSocket;
            private readonly ManualResetEvent SocketHandler = new ManualResetEvent(false);
            private readonly byte[] ClientBuffer = new byte[Definitions.SLM.NetworkBuffer];

            public void StartServer()
            {
                try
                {
                    if (ServerSocket != null)
                    {
                        // Stop server
                    }
                    else
                    {
                        if (String.IsNullOrEmpty(Properties.Settings.Default.ListenIP))
                        {
                            Functions.Network.UpdatePublicIP();
                        }

                        if (Properties.Settings.Default.ListenPort == 0)
                        {
                            Properties.Settings.Default.ListenPort = Functions.Network.GetAvailablePort();
                        }

                        if (Functions.Network.GetPortStatus(Properties.Settings.Default.ListenPort))
                        {
                            throw new Exception($"Port is in use! Port: {Properties.Settings.Default.ListenPort} - Available Port: {Functions.Network.GetAvailablePort()}");
                        }

                        ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        ServerSocket.Bind(new IPEndPoint(IPAddress.Parse(Properties.Settings.Default.ListenIP), Properties.Settings.Default.ListenPort));
                        ServerSocket.Listen(0);

                        //Main.Accessor.ServerStatus.Content = $"Listening on {Properties.Settings.Default.ListenIP} Port: {Properties.Settings.Default.ListenPort}";

                        Thread ServerHandler = new Thread(HandleServer);
                        ServerHandler.Start();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }

            private void HandleServer()
            {
                while(ServerSocket != null)
                {
                    SocketHandler.Reset();

                    ServerSocket.BeginAccept(new AsyncCallback(ServerCallback), null);

                    SocketHandler.WaitOne();
                }
            }

            private void ServerCallback(IAsyncResult ar)
            {
                SocketHandler.Set();

                ClientSocket = ServerSocket.EndAccept(ar);

                DoRecvFromClient();

                Debug.WriteLine(((IPEndPoint)(ClientSocket.RemoteEndPoint)).Address);

                SendToClient(File.ReadAllBytes(@"E:\Kurulum Dosyaları\Program\SQLEXPRWT_x64_ENU.exe"));
            }

            private void DoRecvFromClient()
            {
                ClientSocket.BeginReceive(ClientBuffer, 0, ClientBuffer.Length, SocketFlags.None, new AsyncCallback(DoReceiveFromClient), null);
            }

            private void DoReceiveFromClient(IAsyncResult ar)
            {
                int BytesToRead = ClientSocket.EndReceive(ar);

                if (BytesToRead > 0)
                {
                    Debug.WriteLine(BytesToRead);
                }

                DoRecvFromClient();
            }

            private void SendToClient(byte[] buffer)
            {
                ClientSocket.BeginSend(buffer, 0, buffer.Length, 0, new AsyncCallback(SendTClient), null);
            }

            private void SendTClient(IAsyncResult ar)
            {
                try
                {
                    int bytesSent = ClientSocket.EndSend(ar);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }

        }
    }
}
