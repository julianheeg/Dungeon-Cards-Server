using Card_Mage_Server.Game_Files.MapFolder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Card_Mage_Server.Game_Files.Maze_Generation
{
    /// <summary>
    /// an enumeration of player base types
    /// </summary>
    public enum PlayerBaseType { NULL, Base1 }

    /// <summary>
    /// an abstract class that represents a player base. The child classes will set some tiles around the player's tile in a way specific to the player base type
    /// </summary>
    public abstract class PlayerBase
    {
        protected Random rng = new Random();
        protected GridPosition[] playerPositions;

        /// <summary>
        /// factory method
        /// </summary>
        /// <param name="type">the player base to create</param>
        /// <returns>a player base object</returns>
        public static PlayerBase Instantiate(PlayerBaseType type)
        {
            switch (type)
            {
                case PlayerBaseType.NULL:
                    return null;
                case PlayerBaseType.Base1:
                    return new PlayerBase1();
                default:
                    throw new NotImplementedException("PlayerBase.Instantiate(): switch hit default case");
            }
        }

        public abstract void GenerateBases(Map map, int player1Y, int player2Y);
    }
}
