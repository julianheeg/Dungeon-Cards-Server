using Card_Mage_Server.Game_Files;
using System.Threading;

namespace Card_Mage_Server
{
    /// <summary>
    /// the part of the program entry class that pings the players repeatedly in order to see which ones are still connected
    /// </summary>
    static partial class Program
    {
        /// <summary>
        /// pings repeatedly every player in a lobby or a game
        /// </summary>
        private static void PingRepeatedly()
        {
            while (!closing)
            {
                //ping lobbies
                lock (lobbies)
                {
                    foreach (Lobby lobby in lobbies)
                    {
                        lobby.PingPlayers();
                    }
                }

                //ping games
                lock (games)
                {
                    foreach (Game game in games)
                    {
                        game.PingPlayers();
                    }
                }

                //sleep
                Thread.Sleep(Config.PingInterval);
            }
        }
    }
}
