using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Card_Mage_Server
{
    /// <summary>
    /// a class which contains configurable variables
    /// </summary>
    public static class Config
    {
        //server
        public static int PingInterval = 5000;
        public static int GameStateChangeInterval = 25;

        //player names and passwords
        public static int MinimumUsernameLength = 3;
        public static int MinimumPasswordLength = 4;

        //map generation (three maze generator)
        public static int MinDistanceToWall = 4;
        public static int OutwardBias = 1;
        public static float WallRemovalThreshold = 0.10f;

        //cards
        public static int AmountOfCardsInHandAtTheBeginning = 5;

        //optimizations
        public static int typicalAmountOfCardsPerPlayer = 50;
    }
}
