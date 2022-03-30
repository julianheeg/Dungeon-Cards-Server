using Card_Mage_Server.Game_Files.Cards;
using Card_Mage_Server.Game_Files.MapFolder;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Card_Mage_Server.Game_Files
{
    /// <summary>
    /// the network communication part of the game class
    /// </summary>
    public partial class Game
    {
        #region sending

        /// <summary>
        /// sends data to all players in this game
        /// </summary>
        /// <param name="data">the data to send</param>
        public void SendToAll(List<byte> data)
        {
            for (int i = 0; i < numberOfPlayers; i++)
            {
                SendToOne(i, data);
            }
        }

        /// <summary>
        /// sends data to the specified player
        /// </summary>
        /// <param name="playerIndex">the index of the player</param>
        /// <param name="data">the data to send</param>
        public void SendToOne(int playerIndex, List<byte> data)
        {
            try
            {
                Console.WriteLine("Sending data to {0}", players[playerIndex].ToString());
                ConsoleExt.LogArray<byte>(data);
                Program.send(players[playerIndex], data);
            }
            catch (ObjectDisposedException)
            {
                Console.WriteLine("Game.SendToOne(playerindex, data): the socket of {0} has been disposed of", players[playerIndex].ToString());
            }
        }

        /// <summary>
        /// sends data to all clients, but sends different messages depending on whether the particular client has vision on the position
        /// </summary>
        /// <param name="position">the position at which to check visibility</param>
        /// <param name="visibleData">the data sent to players who can see the tile</param>
        /// <param name="invisibleData">the data sent to players who cannot see the tile</param>
        public void SendMessageWhichDistinguishesByFogOfWar(GridPosition position, List<byte> visibleData, List<byte> invisibleData)
        {
            for (int i = 0; i < numberOfPlayers; i++)
            {
                if (map.IsVisibleThroughFoW(position, i))
                {
                    SendToOne(i, visibleData);
                }
                else if (invisibleData != null)
                {
                    SendToOne(i, invisibleData);
                }
            }
        }

        /// <summary>
        /// pings each player to see if they are still connected
        /// </summary>
        internal void PingPlayers()
        {
            foreach (Player player in players)
            {
                try
                {
                    //ConsoleExt.WriteLine("Game.PingPlayers(): Pinging player " + player.ToString(), ConsoleColor.DarkGray);
                    Messages.Ping(player);
                }
                catch (SocketException e)
                {
                    Console.WriteLine("Game.PingPlayers() [GameClassMessaging.cs]: \n" + e.Message);
                }
            }
        }

        #endregion
    }
}
