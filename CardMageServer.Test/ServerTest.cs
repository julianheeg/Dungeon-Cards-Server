using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Net;
using System.Diagnostics;
using System.Threading;

namespace CardMageServer.Test
{
    [TestClass]
    public class ServerTest
    {
        private static int BUFFERSIZE = 8192;
        private static int WAITTIME = 200;
        private static string IP = "95.143.172.236";

        private List<Socket> testClients = new List<Socket>();
        private List<byte[]> testBuffers = new List<byte[]>();

        private List<Socket> serverSockets = new List<Socket>(); //these are all the same actually, but for the prupose of testing, every client sends messages to the corresponding socket in the list

        private byte[] response;

        /// <summary>
        /// clears the lists so the tests can run
        /// </summary>
        private void clearLists()
        {
            testClients.Clear();
            testBuffers.Clear();
            serverSockets.Clear();
        }

        /// <summary>
        /// initializes a new socket along with a new buffer and adds them to the lists
        /// </summary>
        private void SetupNewClient()
        {
            try {
                Socket newClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                newClient.BeginConnect(IPAddress.Parse(IP), 62480, ConnectCallback, testClients.Count);

                byte[] buffer = new byte[BUFFERSIZE];

                testClients.Add(newClient);
                testBuffers.Add(buffer);

                //wait and assert that client is connected
                Thread.Sleep(WAITTIME);
                Assert.IsTrue(IsSocketConnected(testClients[0]));
            }
            catch (Exception)
            {
                Assert.Fail("could not connect client to the server");
            }
        }

        /// <summary>
        /// returns true if the socket has a connection
        /// taken from http://stackoverflow.com/questions/2661764/how-to-check-if-a-socket-is-connected-disconnected-in-c
        /// </summary>
        /// <param name="s">the socket to check</param>
        /// <returns>true if the socket is connected to the server</returns>
        private static bool IsSocketConnected(Socket s)
        {
            return !((s.Poll(1000, SelectMode.SelectRead) && (s.Available == 0)) || !s.Connected);
        }

        /// <summary>
        /// sends a quit message to the server
        /// </summary>
        /// <param name="socket">the socket that quits</param>
        private void Quit(Socket socket)
        {
            byte[] quitPackage = { 0, 0 };
            Send(quitPackage, socket);

            //wait and assert
            Thread.Sleep(WAITTIME);
            Assert.IsFalse(IsSocketConnected(testClients[0]));

            socket.Shutdown(SocketShutdown.Both);
            socket.Disconnect(true);
        }



        //---------------------------------------------------callbacks-----------------------------------------------

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                int index = (int)ar.AsyncState;
                Socket clientSocket = testClients[index];
                clientSocket.EndConnect(ar);
                clientSocket.BeginReceive(testBuffers[index], 0, BUFFERSIZE, SocketFlags.None, ReceiveCallback, index);
            }
            catch (Exception)
            {

            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                int index = (int)ar.AsyncState;
                Assert.IsTrue(index == 0 && testClients.Count == 1);

                Socket clientSocket = testClients[index];

                int received = serverSockets[index].EndReceive(ar);
                response = new byte[received];
                Array.Copy(testBuffers[index], response, received);

                clientSocket.BeginReceive(testBuffers[index], 0, BUFFERSIZE, SocketFlags.None, ReceiveCallback, index);
            }
            catch (Exception)
            {
                
            }
        }

        private void Send(byte[] data, Socket socket)
        {
            socket.BeginSend(data, 0, data.Length , SocketFlags.None, SendCallback, socket);
        }

        private void SendCallback(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState;
            socket.EndSend(ar);
        }

        //----------------------------------------------------callbacks end--------------------------------------------------


        /// <summary>
        /// tests whether a client can connect to the server
        /// </summary>
        [TestMethod]
        public void TestConnectOneClient()
        {
            //setup
            clearLists();
            SetupNewClient();

            //quit the server
            Quit(testClients[0]);
        }

        /// <summary>
        /// tests whether an unclean disconnect doesn't cause the server to crash
        /// this is tested indirectly by running the next test. the server should still be running
        /// </summary>
        [TestMethod]
        public void TestUncleanDisconnect()
        {
            //setup
            clearLists();
            SetupNewClient();
            //close connection without shutdown
            testClients[0].Close();
        }

        [TestMethod]
        public void TestReconnect()
        {
            //setup
            clearLists();
            SetupNewClient();

            //quit and reconnect
            Quit(testClients[0]);
            testClients[0].BeginConnect(IPAddress.Parse("95.143.172.236"), 62480, ConnectCallback, testClients.Count);

            //wait and assert that client is connected
            Thread.Sleep(WAITTIME);
            Assert.IsTrue(IsSocketConnected(testClients[0]));

        }

        [TestMethod]
        public void TestSeveralClientsConnectAndDisconnect()
        {
            //setup
            clearLists();
            for (int i = 0; i < 10; i++)
            {
                SetupNewClient();
            }
            for(int i = 0; i < 10; i++)
            {
                Quit(testClients[i]);
            }
        }
    }
}
