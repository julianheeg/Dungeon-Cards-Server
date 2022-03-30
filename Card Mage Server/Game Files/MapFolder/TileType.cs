using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Card_Mage_Server.Game_Files.MapFolder
{
    /// <summary>
    /// an enumeration of types that a tile on the map can have.
    /// </summary>
    public enum TileType { Out_Of_Bounds = 0, Uninitialized, Wall, Traversible, Player1, Player2, Player3, Player4, TempMark1, TempMark2, Monster };

    /// <summary>
    /// extensions for tile type
    /// </summary>
    public static class TileTypeExtension
    {
        /// <summary>
        /// checks if the tile type represents a player tile
        /// </summary>

        public static bool IsPlayer(this TileType tileType)
        {
            return tileType == TileType.Player1 || tileType == TileType.Player2 || tileType == TileType.Player3 || tileType == TileType.Player4;
        }

        /// <summary>
        /// checks if the tile can be seen though by monsters
        /// </summary>
        public static bool SeeThrough(this TileType tileType)
        {
            return tileType == TileType.Traversible || tileType.IsPlayer();
        }

        /// <summary>
        /// converts a player enum into their player index
        /// </summary>
        /// <returns>the player index if the tileType is a player, and -1 otherwise</returns>
        public static int GetPlayerIndex(this TileType tileType)
        {
            switch (tileType)
            {
                case TileType.Player1:
                    return 0;
                case TileType.Player2:
                    return 1;
                case TileType.Player3:
                    return 2;
                case TileType.Player4:
                    return 3;
                default:
                    return -1;
            }
        }
    }
}
