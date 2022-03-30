using Card_Mage_Server.Game_Files;
using Card_Mage_Server.Game_Files.Maze_Generation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Card_Mage_Server
{
    /// <summary>
    /// a class that represents a game lobby
    /// </summary>
    public class Lobby
    {
        private static int UniqueID;

        public int id;

        public Player[] players;
        private readonly bool[] ready;

        private readonly int hostPosition;

        //for future extension, already in the network encoding
        public int maxPlayers;
        public int currentPlayerCount;

        //for future extension, not yet in network encoding
        public MazeGeneratorType mazeGeneratorType = MazeGeneratorType.ThreeMaze; //default so long as no other ones exist
        public MazeGeneratorType subMazeGeneratorType = MazeGeneratorType.DFS;
        public PlayerBaseType playerBaseType = PlayerBaseType.Base1; //default so long as no other ones exist
        public int width = 27; //default
        public int height = 27; //default
        public bool isHexGrid = true; //default


        /// <summary>
        /// Constructor
        /// sends a message to the creating player to update their menues
        /// </summary>
        /// <param name="player">the player who creates the lobby</param>
        public Lobby(Player player)
        {
            //set id
            this.id = UniqueID;
            UniqueID++;

            //set max players
            maxPlayers = 2; //TODO change to 2. This is 1 only for testing
            players = new Player[maxPlayers];
            ready = new bool[maxPlayers];

            lock (players)
            {
                currentPlayerCount = 1;
                players[0] = player;
                hostPosition = 0;

                //set player flags
                player.onServer = false;
                player.inLobby = true;
                player.lobby = this;

                Console.WriteLine("Lobby.Lobby(player): player {0} created lobby {1}", player.name, this.id);
            }

            //notify player who created the lobby
            Messages.SendLobbyJoinMessage(this, player);
        }

        /// <summary>
        /// tries to add a player to this lobby.
        /// </summary>
        /// <param name="player">the player to be added</param>
        /// <returns>whether the player could be added</returns>
        public bool AddPlayer(Player player)
        {
            int firstOpenPosition = -1;

            lock (players)
            {
                //find first available spot
                for (int i = 0; i < maxPlayers; i++)
                {
                    if (players[i] == null)
                    {
                        firstOpenPosition = i;
                        break;
                    }
                }

                //check if there is an open position
                if (firstOpenPosition != -1)
                {
                    //send a notifications to all players in this lobby (not the new player, that's why it is here and not lower down)
                    Messages.SendLobbyOtherJoinMessage(this, player, firstOpenPosition);

                    //add player
                    players[firstOpenPosition] = player;
                    currentPlayerCount++;

                    //set player flags
                    player.onServer = false;
                    player.inLobby = true;
                    player.lobby = this;

                    //send the lobby to the joining player
                    Messages.SendLobbyJoinMessage(this, player);

                    //debug output
                    Console.WriteLine("Lobby.AddPlayer(player): Seated player {0} in Lobby {1} at position {2}.", player.name, this.id, firstOpenPosition);
                    return true;
                }
                else
                {
                    Messages.SendLobbyFullMessage(player);

                    Console.WriteLine("Lobby.AddPlayer(player): lobby {0} was full. Couldn't seat player {1}", this.id, player.name);
                    return false;
                }
            }
        }

        /// <summary>
        /// Sets/Resets a player's ready flag
        /// </summary>
        /// <param name="player">the player whose ready flag should be set/reset</param>
        /// <param name="ready">the ready value for that player</param>
        public void SetReady(Player player, bool ready)
        {
            Console.WriteLine("lobby.setReady(): setting player {0} to {1}", player.name, ready);

            lock (players)
            {
                //find the player in the list
                for (int i = 0; i < maxPlayers; i++)
                {
                    if (players[i] != null && players[i] == player)
                    {
                        //set ready state
                        this.ready[i] = ready;

                        //send message
                        Messages.SendLobbyReadyMessage(this, i, ready);

                        break;
                    }
                }
            }
        }

        /// <summary>
        /// removes a player from the lobby. if no players remain, the lobby will be destroyed
        /// sends a message to all players affected
        /// </summary>
        /// <param name="player">the player to remove</param>
        public void Leave(Player player)
        {
            Console.WriteLine("Lobby.leave(player): player {0} tries to leave lobby {1}", player.name, this.id);

            lock (players)
            {
                //find the player in the list
                int playerPosition = -1;
                for (int i = 0; i < maxPlayers; i++)
                {
                    if (players[i] != null && players[i] == player)
                    {
                        playerPosition = i;
                        break;
                    }
                }

                //write an error to the console if not found
                if (playerPosition == -1)
                {
                    Console.WriteLine("Lobby.leave(player): Error: leaving player (id = {0}) was not in the lobby", player.id);
                    return;
                }
                //else remove the player
                else
                {
                    //send message (this is up here because later the player gets removed from the list)
                    Messages.SendLobbyLeaveMessage(this, playerPosition);

                    //remove the player
                    players[playerPosition] = null;
                    currentPlayerCount--;

                    //set player flags
                    player.onServer = true;
                    player.inLobby = false;
                    player.lobby = null;

                    Console.WriteLine("Lobby.leave(player): player {0} left lobby {1} successfully", player.name, this.id);

                    //remove lobby if empty
                    if (currentPlayerCount == 0)
                    {
                        Program.RemoveLobby(this);
                    }
                }
            }
        }

        /// <summary>
        /// pings all players to see if they are still connected
        /// </summary>
        internal void PingPlayers()
        {
            lock (players)
            {
                foreach (Player player in players)
                {
                    if (player != null)
                    {
                        try
                        {
                            //ConsoleExt.WriteLine("Lobby.PingPlayers(): pinging player " + player.ToString(), ConsoleColor.DarkGray);
                            Messages.Ping(player);
                        }
                        catch (SocketException e)
                        {
                            Console.WriteLine("Lobby.PingPlayers(): \n" + e.Message);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// sends a message to all clients in this lobby
        /// </summary>
        /// <param name="data">the message to send</param>
        public void SendToAll(List<byte> data)
        {
            lock (players)
            {
                for (int i = 0; i < maxPlayers; i++)
                {
                    if (players[i] != null)
                    {
                        try
                        {
                            Program.send(players[i], data);
                        }
                        catch (ObjectDisposedException)
                        {
                            Console.WriteLine("Lobby.SendToAll(data): the socket of player {0} has been disposed of", players[i].name);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// serializes all relevant information of the lobby object for clients that are already in the lobby menu
        /// </summary>
        /// <returns>the serialized lobby</returns>
        public byte[] SerializeAll()
        {
            List<byte> bytesList = new List<byte>();

            //add max players
            bytesList.AddRange(Endianness.ToBigEndian(BitConverter.GetBytes(maxPlayers)));

            lock (players)
            {
                //add individual players
                for (int i = 0; i < maxPlayers; i++)
                {
                    //if player is not null, serialize, else put 0 for ID
                    if (players[i] != null)
                    {
                        bytesList.AddRange(players[i].Serialize());
                    }
                    else
                    {
                        bytesList.AddRange(Endianness.ToBigEndian(BitConverter.GetBytes(0)));
                    }
                }
            }

            return bytesList.ToArray();
        }


        /// <summary>
        /// serializes this lobby for display in a list to look at before a client actually joins
        /// (meaning ID, max players, current players, host name)
        /// </summary>
        /// <returns>the serialized lobby</returns>
        public byte[] SerializeForList()
        {
            lock (players)
            {
                List<byte> bytesList = new List<byte>();

                //add id
                bytesList.AddRange(Endianness.ToBigEndian(BitConverter.GetBytes(id)));

                //add player count info
                bytesList.AddRange(Endianness.ToBigEndian(BitConverter.GetBytes(maxPlayers)));
                bytesList.AddRange(Endianness.ToBigEndian(BitConverter.GetBytes(currentPlayerCount)));

                bytesList.AddRange(players[hostPosition].Serialize());

                return bytesList.ToArray();
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[id: " + id + ", players: ");
            for(int i = 0; i < players.Length; i++)
            {
                sb.Append(players[i] != null ? players[i].ToString() : "-" + " ");
            }
            sb.Append("]");
            return sb.ToString();
        }
    }
}
