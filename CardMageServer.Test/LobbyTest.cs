using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace CardMageServer.Test
{
    [TestClass]
    public class LobbyTest
    {
        private static int BUFFERSIZE = 8192;
        private static int WAITTIME = 300;
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
            try
            {
                int index = testClients.Count;
                Socket newClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                newClient.BeginConnect(IPAddress.Parse(IP), 62480, ConnectCallback, newClient);

                byte[] buffer = new byte[BUFFERSIZE];

                testClients.Add(newClient);
                testBuffers.Add(buffer);

                //wait and assert that client is connected
                Thread.Sleep(WAITTIME);
                Assert.IsTrue(IsSocketConnected(testClients[index]));
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

        /// <summary>
        /// sends a create lobby message to the server
        /// </summary>
        /// <param name="socket">the socket that sends the message</param>
        private void CreateLobby(Socket socket)
        {
            byte[] createLobbyPackage = { 0, 2 };
            Send(createLobbyPackage, socket);


            //wait
            Thread.Sleep(WAITTIME);
        }

        /// <summary>
        /// sends a create lobby message to the server
        /// </summary>
        /// <param name="socket">the socket that sends the message</param>
        private void LeaveLobby(Socket socket)
        {
            byte[] leaveLobbyPackage = { 1, 0 };
            Send(leaveLobbyPackage, socket);

            //wait
            Thread.Sleep(WAITTIME);
        }

        /// <summary>
        /// sends a join lobby message to the server
        /// </summary>
        /// <param name="socket">the socket that sends the message</param>
        /// <param name="lobbyID">the lobby to join</param>
        private void JoinLobby(Socket socket, int lobbyID)
        {
            //create package
            List<byte> joinLobbyPackage = new List<byte>(6);
            joinLobbyPackage.Add(0);
            joinLobbyPackage.Add(1);
            joinLobbyPackage.AddRange(BitConverter.GetBytes(lobbyID));
            Send(joinLobbyPackage.ToArray(), socket);

            //wait
            Thread.Sleep(WAITTIME);
        }

        private void ListLobby(Socket socket)
        {
            byte[] listLobbyPackage = { 0, 3 };
            Send(listLobbyPackage, socket);

            //wait
            Thread.Sleep(WAITTIME);
        }



        //---------------------------------------------------callbacks-----------------------------------------------

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket clientSocket = (Socket)ar.AsyncState;
                int index = testClients.IndexOf(clientSocket);
                clientSocket.EndConnect(ar);
                clientSocket.BeginReceive(testBuffers[index], 0, BUFFERSIZE, SocketFlags.None, ReceiveCallback, clientSocket);
            }
            catch (Exception)
            {

            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                Socket clientSocket = (Socket)ar.AsyncState;
                int index = testClients.IndexOf(clientSocket);

                int received = testClients[index].EndReceive(ar);
                response = new byte[received];
                Array.Copy(testBuffers[index], response, received);

                clientSocket.BeginReceive(testBuffers[index], 0, BUFFERSIZE, SocketFlags.None, ReceiveCallback, clientSocket);
            }
            catch (SocketException)
            {
            }
        }

        private void Send(byte[] data, Socket socket)
        {
            socket.BeginSend(data, 0, data.Length, SocketFlags.None, SendCallback, socket);
        }

        private void SendCallback(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState;
            socket.EndSend(ar);
        }

        //----------------------------------------------------callbacks end--------------------------------------------------

        /// <summary>
        /// Tests whether the server creates a lobby and closes it when the only player leaves
        /// </summary>
        [TestMethod]
        public void TestLobbyCreateAndLeave()
        {
            clearLists();
            SetupNewClient();

            CreateLobby(testClients[0]);

            LeaveLobby(testClients[0]);
            Quit(testClients[0]);
        }

        /// <summary>
        /// Tests whether the server allows another player to join the lobby
        /// </summary>
        [TestMethod]
        public void TestLobbyCreateAndJoinAndLeave()
        {
            //setup
            clearLists();
            SetupNewClient();
            SetupNewClient();
            CreateLobby(testClients[0]);

            //list response, read out id and join the lobby
            ListLobby(testClients[1]);
            Assert.AreEqual(27, response.Length);
            int id = BitConverter.ToInt32(response, 2);
            JoinLobby(testClients[1], id);

            //leave and quit
            LeaveLobby(testClients[1]);
            LeaveLobby(testClients[0]);
            Quit(testClients[0]);
            Quit(testClients[1]);
        }

        /// <summary>
        /// tests how the server behaves when a player disconnects uncleanly (together with next test method which will fail when the server crashed)
        /// </summary>
        [TestMethod]
        public void TestLobbyUncleanDisconnects()
        {
            //setup
            clearLists();
            SetupNewClient();
            CreateLobby(testClients[0]);
            //quit without leaving the lobby
            Quit(testClients[0]);

            //setup next client and list the lobbies
            SetupNewClient();
            ListLobby(testClients[1]);
            Assert.AreEqual(2, response.Length);

            //create a lobby
            CreateLobby(testClients[1]);
            //don't send a quit message
        }

        /// <summary>
        /// tests whether a server sends a message when a lobby is full and someone tries to join
        /// </summary>
        [TestMethod]
        public void TestLobbyFull()
        {
            //setup
            clearLists();
            SetupNewClient();
            SetupNewClient();
            SetupNewClient();
            CreateLobby(testClients[0]);

            //list response, read out id and join the lobby
            ListLobby(testClients[1]);
            Assert.AreEqual(27, response.Length);
            int id1 = BitConverter.ToInt32(response, 2);
            JoinLobby(testClients[1], id1);

            //list response, read out id and join the lobby
            ListLobby(testClients[2]);
            Assert.AreEqual(27, response.Length);
            int id2 = BitConverter.ToInt32(response, 2);
            Assert.AreEqual(id1, id2);
            JoinLobby(testClients[2], id2);

            //assert that response is that the lobby is full
            Assert.AreEqual(2, response.Length);
            Assert.AreEqual(0, response[0]);
            Assert.AreEqual(1, response[1]);

            //assert that a list gets sent again when asked
            //(doesn't happen when the client is in a lobby already)
            ListLobby(testClients[2]);
            Assert.AreEqual(27, response.Length);
        }

        /// <summary>
        /// tests how the server copes with several lobbies
        /// </summary>
        [TestMethod]
        public void TestSeveralLobbies()
        {
            //setup
            clearLists();
            SetupNewClient();
            SetupNewClient();
            SetupNewClient();
            SetupNewClient();
            SetupNewClient();
            CreateLobby(testClients[0]);

            //list response, read out id and join the lobby
            ListLobby(testClients[1]);
            Assert.AreEqual(27, response.Length);
            int id1 = BitConverter.ToInt32(response, 2);
            JoinLobby(testClients[1], id1);

            //create two more lobbies
            CreateLobby(testClients[2]);
            CreateLobby(testClients[3]);

            //list response and copy it
            ListLobby(testClients[4]);
            Assert.AreEqual(77, response.Length);
            
            byte[] oldResponse = new byte[77];
            Array.Copy(response, oldResponse, 77);

            //read out ids and assert, join third lobby
            int id2 = BitConverter.ToInt32(response, 27);
            int id3 = BitConverter.ToInt32(response, 52);
            Assert.AreNotEqual(id1, id2);
            Assert.AreNotEqual(id1, id3);
            Assert.AreNotEqual(id2, id3);
            JoinLobby(testClients[4], id3);

            //leave lobby and join second lobby
            LeaveLobby(testClients[4]);
            ListLobby(testClients[4]);
            Assert.AreEqual(77, response.Length);
            CollectionAssert.AreEqual(response, oldResponse);
            JoinLobby(testClients[4], id2);

            oldResponse = new byte[52];
            Array.Copy(response, oldResponse, 52);
            oldResponse[35] = 2;
            oldResponse[36] = oldResponse[37] = oldResponse[38] = 0;

            //make fourth client leave lobby and attempt to join the second one
            LeaveLobby(testClients[3]);
            ListLobby(testClients[3]);
            Assert.AreEqual(52, response.Length);
            CollectionAssert.AreEqual(response, oldResponse);
            JoinLobby(testClients[4], id2);
        }
    }
}
