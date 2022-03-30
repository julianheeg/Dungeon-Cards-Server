using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Card_Mage_Server.Game_Files.MapFolder
{
    public enum Direction { W, NW, NE, E, SE, SW }

    /// <summary>
    /// extensions for direction
    /// </summary>
    static class DirectionExtension
    {
        /// <summary>
        /// turns a direction into a grid position where the grid position is the neighbor of [0,0] in that direction. Example useage:
        ///     Gridposition leftOfGridPosition = <gridPosition> + Direction.W.toGridPosition();
        /// </summary>
        /// <param name="direction">the direction</param>
        /// <returns>the neighboring grid position in the direction from the [0,0] grid point</returns>
        public static GridPosition ToGridPosition(this Direction direction)
        {
            switch (direction)
            {
                case Direction.W:
                    return new GridPosition(0, -1);
                case Direction.NW:
                    return new GridPosition(1, 0);
                case Direction.NE:
                    return new GridPosition(1, 1);
                case Direction.E:
                    return new GridPosition(0, 1);
                case Direction.SE:
                    return new GridPosition(-1, 0);
                case Direction.SW:
                    return new GridPosition(-1, -1);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
