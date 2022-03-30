using Card_Mage_Server.Game_Files;
using System.Collections.Generic;
using System.Threading;

namespace Card_Mage_Server
{
    /// <summary>
    /// the part of the program entry class which executes the game related actions
    /// </summary>
    static partial class Program
    {
        //needed to not modify the games list during iteration
        static List<Game> gamesToRemove = new List<Game>();

        /// <summary>
        /// causes all games to advance their game states by the commands in their respective command queues
        /// sleeps and then dos it again
        /// </summary>
        static void AdvanceGameStatesRepeatedly()
        {
            while (!closing)
            {
                //advance game states
                lock (games)
                {
                    //advance game states
                    foreach (Game game in games)
                    {
                        game.AdvanceGameState();
                    }

                    //remove finished/cancelled games from the list
                    for (int i = 0; i < gamesToRemove.Count; i++)
                    {
                        games.Remove(gamesToRemove[i]);
                    }
                    gamesToRemove.Clear();
                }

                //sleep
                Thread.Sleep(Config.GameStateChangeInterval);
            }
        }

        /// <summary>
        /// queues a game for later removal
        /// </summary>
        /// <param name="game">the game to remove</param>
        public static void QueueRemoveGame(Game game)
        {
            gamesToRemove.Add(game);
        }
    }
}
