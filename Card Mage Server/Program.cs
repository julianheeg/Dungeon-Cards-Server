using Card_Mage_Server.Game_Files;
using Card_Mage_Server.Game_Files.Cards;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Card_Mage_Server
{
    /// <summary>
    /// the program entry point. Handles connections and stores the connected players and the running lobbies and games.
    /// </summary>
    static partial class Program
    {
        private static int port = 62480;
        
        private static Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        //threads
        private static Thread executionThread;
        private static Thread pingThread;
        static volatile bool closing = false;

        //the connected players
        private static List<Player> players = new List<Player>();

        //open games and lobbies
        private static List<Lobby> lobbies { get; } = new List<Lobby>();
        private static List<Game> games = new List<Game>();

        /// <summary>
        /// the program entry point. Sets up the entire program
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            SetupServer();
            SetupThreads();

            CommandLoop();
        }

        /// <summary>
        /// sets up the ping thread and the game execution thread
        /// </summary>
        private static void SetupThreads()
        {
            pingThread = new Thread(Program.PingRepeatedly);
            pingThread.Start();

            executionThread = new Thread(Program.AdvanceGameStatesRepeatedly);
            executionThread.Start();
        }

        /// <summary>
        /// loops until the user enters "close" into the console
        /// </summary>
        private static void CommandLoop()
        {
            string command;
            do
            {
                command = Console.ReadLine();
            }
            while (command != "close");

            closing = true;
            executionThread.Join();
            pingThread.Join();
            Console.WriteLine("Server closes");
        }

        /// <summary>
        /// sets up the server, binds ports, etc
        /// </summary>
        private static void SetupServer()
        {
            CardDatabase.Init();

            Console.WriteLine("Setting up the server...");
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, port));
            serverSocket.Listen(10);
            serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);

        }

        #region connection callbacks

        /// <summary>
        /// called when a socket wants to connect
        /// adds the socket to the list of clientsockets and starts receiving
        /// then starts accepting new sockets
        /// </summary>
        /// <param name="ar">callback result</param>
        private static void AcceptCallback(IAsyncResult ar)
        {
            Socket newClientSocket = serverSocket.EndAccept(ar);
            Player newPlayer = new Player(newClientSocket);
            lock (players)
            {
                players.Add(newPlayer);
            }
            Console.WriteLine("Client connected");

            //MISSING: send a connection succeeded ?

            newClientSocket.BeginReceive(newPlayer.buffer, 0, newPlayer.buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), newPlayer);
            serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
        }

        /// <summary>
        /// called when the server receives a message
        /// calls the message parser which then handles the message
        /// begins to receive aagin afterwards
        /// </summary>
        /// <param name="ar">callback result</param>
        private static void ReceiveCallback(IAsyncResult ar)
        {
            Player player = (Player)ar.AsyncState;
            try
            {
                //copy array
                int received = player.socket.EndReceive(ar);
                byte[] trimmedBuffer = new byte[received];
                Array.Copy(player.buffer, trimmedBuffer, received);

                //add array to the message builder
                player.messageBuilder.AddRange(trimmedBuffer);

                //loop
                while (true)
                {
                    //if the length can't be read out, break out of the loop
                    if (player.messageBuilder.Count <= 3)
                    {
                        break;
                    }

                    //get length
                    byte[] lengthInBytes = new byte[4];
                    player.messageBuilder.CopyTo(0, lengthInBytes, 0, 4);
                    int length = BitConverter.ToInt32(Endianness.FromBigEndian(lengthInBytes, 0), 0);

                    //if the message builder contains at least a full message, parse it and remove it from the message builder
                    if (player.messageBuilder.Count >= length)
                    {
                        //get the message and parse it
                        byte[] data = new byte[length];
                        player.messageBuilder.CopyTo(4, data, 0, length);

                        DataParser.ParseTopLevel(data, player);

                        //remove the current message
                        player.messageBuilder.RemoveRange(0, 4 + length);
                    }
                    //else break out of the loop
                    else
                    {
                        break;
                    }
                }

                //begin receiving again, if the socket hasn't been closed
                if (player.socket.Connected)
                {
                    player.socket.BeginReceive(player.buffer, 0, player.buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), player);
                }
            }
            catch(SocketException ex)
            {
                //disconnect client
                Console.WriteLine("Program.ReceiveCallback(ar): Disconnecting player {0} because an Exception occured: " + ex.Message, player.name);
                DisconnectClient(player);
            }
        }

        /// <summary>
        /// sends data to the specified socket
        /// </summary>
        /// <param name="data">the data to send</param>
        /// <param name="clientSocket">the client socket to send the data to</param>
        public static void send(Player player, List<byte> data)
        {
            //insert length at start
            byte[] _data = new byte[data.Count + 4];
            byte[] length = Endianness.ToBigEndian(BitConverter.GetBytes(data.Count));
            Array.Copy(length, _data, 4);

            //copy data
            data.CopyTo(_data, 4);

            if(player == null)
            {
                throw new Exception();
            }
            if (player.socket == null)
            {
                throw new Exception();
            }
            if (_data == null)
            {
                throw new Exception();
            }

            try
            {
                player.socket.BeginSend(_data, 0, _data.Length, SocketFlags.None, new AsyncCallback(SendCallback), player);
            }
            catch (SocketException e)
            {
                Console.WriteLine("Program.send(): Exception\n" + e.Message + "\nSTACKTRACE:\n" + e.StackTrace);
            }
        }

        /// <summary>
        /// the send callback
        /// </summary>
        /// <param name="ar">callback result</param>
        private static void SendCallback(IAsyncResult ar)
        {
            Player player = (Player)ar.AsyncState;
            player.socket.EndSend(ar); 
        }

        #endregion

        #region server logic

        /// <summary>
        /// closes the socket of a disconnecting player
        /// </summary>
        /// <param name="player">the player who disconnects</param>
        public static void DisconnectClient(Player player)
        {
            Console.WriteLine("disconnecting player {0}", player.name);
            //check if the player socket is still connected
            if (player.socket.Connected)
            {
                lock (players)
                {
                    players.Remove(player);
                }

                //shut down connection
                player.socket.Shutdown(SocketShutdown.Both);
                player.socket.Close();
            }

            //notify the lobby that the client has left
            if (player.lobby != null)
            {
                player.lobby.Leave(player);
            }

            //notify the game that the client has left
            if (player.game != null)
            {
                player.game.Leave(player);
            }
        }

        /// <summary>
        /// creates a new lobby and adds it to the lobby list
        /// </summary>
        /// <param name="player">the player who creates the lobby</param>
        public static void CreateLobby(Player player)
        {
            if (player.onServer)
            {
                Lobby newLobby = new Lobby(player);
                lock (lobbies)
                {
                    lobbies.Add(newLobby);
                }
            }
            else
            {
                Console.WriteLine("Program.CreateLobby(): Error, player {0} doesn't have the onServer flag set", player.name);
            }
        }

        /// <summary>
        /// makes the player join the lobby with the specified id
        /// sends a message to the joining player to update their menues
        /// </summary>
        /// <param name="player">the player who wants to join</param>
        /// <param name="id">the lobby id</param>
        public static void JoinLobby(Player player, int id)
        {
            //check if player is eligible to join
            if (player.onServer)
            {
                Console.WriteLine("player {0} tries to join lobby {1}", player.name, id);
                //lock lobbies for iteration
                lock (lobbies)
                {
                    //find correct lobby
                    foreach (Lobby lobby in lobbies)
                    {
                        if (lobby.id == id)
                        {
                            //try adding player to lobby
                            lobby.AddPlayer(player);
                        }
                    }
                }

                if (player.lobby == null)
                {
                    Messages.SendLobbyNotFoundMessage(player);
                }
            }
            else
            {
                Console.WriteLine("Program.JoinLobby(): Error, player {0} doesn't have the onServer flag set", player.name);
            }
        }

        /// <summary>
        /// removes a lobby from the list. only to be called when the last player left the lobby
        /// </summary>
        /// <param name="lobby">the lobby to be removed</param>
        public static void RemoveLobby(Lobby lobby)
        {
            Console.WriteLine("Program.removeLobby(lobby): removing lobby {0}", lobby.id);
            lock(lobbies)
            {
                if (!lobbies.Remove(lobby))
                {
                    Console.WriteLine("Program.removeLobby(lobby): couldn't remove lobby");
                }
            }
        }

        /// <summary>
        /// removes a game from the game list
        /// </summary>
        /// <param name="game"></param>
        internal static void RemoveGame(Game game)
        {
            Console.WriteLine("Program.removeGame(game): removing game {0}", game.id);
            lock (games)
            {
                games.Remove(game);
            }
        }

        #endregion





        //----------------------------------------Data Parser---------------------------------------------------------------------------------

        #region DATA PARSER CLASS
        
        /// <summary>
        /// a class that parses incoming messages
        /// </summary>
        class DataParser
        {
            private enum TopLevel { Server = 0, Lobby, Game, Player };

            /// <summary>
            /// parses the top level (= first byte) of a data array
            /// </summary>
            /// <param name="data">the data to parse</param>
            /// <param name="player">the player who sent the message</param>
            /// <returns>true iff the data could be parsed correctly</returns>
            public static bool ParseTopLevel(byte[] data, Player player)
            {
                //if no message is received, the client has disconnected
                if (data.Length == 0)
                {
                    Program.DisconnectClient(player);
                    return true;
                }
                else
                {
                    switch ((TopLevel)data[0])
                    {
                        case TopLevel.Server:
                            return ParseServerLevel(data, player);

                        case TopLevel.Lobby:
                            return ParseLobbyLevel(data, player);

                        case TopLevel.Game:
                            if (player.inGame)
                            {
                                player.game.CommandQueue.Add(new PlayerAndCommand(player, data));
                                return true;
                            }
                            else
                            {
                                Console.WriteLine("Parse Error on top level: received GAME, but player {0} is not in game", player.name);
                                return false;
                            }

                        default:
                            Console.Error.WriteLine("Parse error on top level: received {0}", data[0]);
                            return false;
                    }
                }
            }


            //possible actions on the server level
            private enum ServerLevel { Quit = 0, JoinLobby, CreateLobby, List, Login };

            /// <summary>
            /// parses the rest of the array if it is directed at the server (e.g. disconnect, ...)
            /// </summary>
            /// <param name="data">the data to parse</param>
            /// <param name="player">the player who sent the message</param>
            /// <returns>true iff the data could be parsed correctly</returns>
            private static bool ParseServerLevel(byte[] data, Player player)
            {
                if (data.Length == 1)
                {
                    Console.Error.WriteLine("Parse error on server level: data only contains one byte");
                    return false;
                }
                switch ((ServerLevel)data[1])
                {
                    case ServerLevel.Quit:
                        Program.DisconnectClient(player);
                        return true;

                    case ServerLevel.JoinLobby:
                        //check correct length, if true call the join lobby function
                        if (data.Length == 6)
                        {
                            int lobbyID = BitConverter.ToInt32(Endianness.FromBigEndian(data, 2), 2);
                            Program.JoinLobby(player, lobbyID);
                            return true;
                        }
                        else
                        {
                            Console.Error.WriteLine("Parse error on server level: second bit was JOIN_LOBBY, but message was {0} bytes long (expected 6)", data.Length);
                            return false;
                        }

                    case ServerLevel.CreateLobby:
                        //check correct length, if true call the create lobby function
                        if (data.Length == 2)
                        {
                            Program.CreateLobby(player);
                            return true;
                        }
                        else
                        {
                            Console.Error.WriteLine("Parse error on server level: second bit was CREATE_LOBBY, but message was {0} bytes long (expected 2)", data.Length);
                            return false;
                        }

                    case ServerLevel.List:
                        //check correct length, if true send the client a list of all the open lobbies
                        if (data.Length == 2)
                        {
                            //check if player is actually on the server
                            if (player.onServer)
                            {
                                //send data
                               lock (lobbies)
                                {
                                    Messages.SendLobbyList(player, lobbies);
                                }
                            }
                            else
                            {
                                Console.WriteLine("ParseServerLevel method: player {0} requested lobby list, but OnServer flag is not set");
                            }
                            return true;
                        }
                        else
                        {
                            Console.Error.WriteLine("Parse error on server level: second bit was LIST, but message was {0} bytes long (expected 2)", data.Length);
                            return false;
                        }

                    case ServerLevel.Login:
                        return LoginAndDatabase.parseLoginData(data, player);

                    default:
                        Console.Error.WriteLine("Parse error on server level: received {0}", data[1]);
                        return false;
                }
            }


            //possible actions on the lobby level
            private enum LobbyLevel { Leave = 0, Ready, Start };

            /// <summary>
            /// parses the rest of the array if it is directed at a lobby (e.g. leave lobby, set the ready flag, change deck, ...)
            /// </summary>
            /// <param name="data">the data to parse</param>
            /// <param name="player">the player who sent the message</param>
            /// <returns>true iff the data could be parsed correctly</returns>
            private static bool ParseLobbyLevel(byte[] data, Player player)
            {
                if (data.Length == 1)
                {
                    Console.Error.WriteLine("Parse error on lobby level: data only contains one byte");
                    return false;
                }
                if (player.lobby == null)
                {
                    Console.Error.WriteLine("Parse error on lobby level: player {0} has no lobby set", player.name);
                    return false;
                }
                switch ((LobbyLevel)data[1])
                {
                    case LobbyLevel.Leave:
                        //check correct length, if true call the leave function
                        if (data.Length == 2)
                        {
                            player.lobby.Leave(player);
                            return true;
                        }
                        else
                        {
                            Console.Error.WriteLine("Parse error on lobby level: second bit was LEAVE, but message was {0} bytes long (expected 2)", data.Length);
                            return false;
                        }

                    case LobbyLevel.Ready:
                        //check correct length, if true, call the ready function with the third bit
                        if (data.Length == 3)
                        {
                            bool ready = BitConverter.ToBoolean(data, 2);
                            player.lobby.SetReady(player, ready);
                            return true;
                        }
                        else
                        {
                            Console.Error.WriteLine("Parse error on lobby level: second bit was READY but message was {0} bytes long (expected 3)", data.Length);
                            return false;
                        }

                    case LobbyLevel.Start:
                        //check correct length, if true, create a game and start it
                        if(data.Length == 2)
                        {
                            //create game and add it to the list, remove lobby
                            
                            lock (lobbies)
                            {
                                lobbies.Remove(player.lobby);
                            }
                            Game game = new Game(player.lobby);
                            lock (games)
                            {
                                games.Add(game);
                            }

                            return true;
                        }
                        else
                        {
                            Console.Error.WriteLine("Parse error on lobby level: second bit was START but message was {0} bytes long (expected 2)", data.Length);
                            return false;
                        }

                    default:
                        Console.Error.WriteLine("Parse error on lobby level: received {0}", data[1]);
                        return false;
                }
            }


            //possible actions on the player level
            private enum PlayerLevel { ChangeDeck = 0, AddDeck, RemoveDeck };

            /// <summary>
            /// parses the rest of the array if it is directed at a game (e.g. change deck, add deck, remove deck, ...)
            /// </summary>
            /// <param name="data">the data to parse</param>
            /// <param name="player">the player who sent the message</param>
            /// <returns>true iff the data could be parsed correctly</returns>
            private static bool ParsePlayerLevel(byte[] data, Player player)
            {
                if (data.Length == 1)
                {
                    Console.Error.WriteLine("Parse error on player level: data only contains one byte");
                    return false;
                }
                switch ((PlayerLevel)data[1])
                {
                    case PlayerLevel.ChangeDeck:
                        //check correct length, if true change deck
                        if (data.Length == 3)
                        {
                            player.currentlySelectedDeck = data[2];
                            return true;
                        }
                        else
                        {
                            Console.Error.WriteLine("Parse error on player level: second bit was CHANGE_DECK but message was {0} bytes long (expected 3)", data.Length);
                            return false;
                        }

                    case PlayerLevel.AddDeck:
                        throw new NotImplementedException();

                    case PlayerLevel.RemoveDeck:
                        throw new NotImplementedException();

                    default:
                        Console.Error.WriteLine("Parse error on player level: received {0}", data[1]);
                        return false;
                }
            }
        }

        #endregion
    }
}
