using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Card_Mage_Server.Game_Files.MapFolder
{
    /// <summary>
    /// a struct that represents a position on the map. x=-1 and y=-1 refer to an irrelevant position (just to make the code more simple
    /// </summary>
    public struct GridPosition
    {
        public readonly int x, y;

        /// <summary>
        /// constructor from values
        /// </summary>
        public GridPosition(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        /// <summary>
        /// constructor from parseable string
        /// </summary>
        /// <param name="data">the data to parse</param>
        /// <param name="index">the current index. This will be incremented for the next parsing function that operates on data</param>
        public GridPosition(byte[] data, ref int index)
        {
            this.x = BitConverter.ToInt32(Endianness.FromBigEndian(data, index), index);
            index += 4;
            this.y = BitConverter.ToInt32(Endianness.FromBigEndian(data, index), index);
            index += 4;
        }

        /// <summary>
        /// -1 for x and y refers to a position that is irrelevant
        /// </summary>
        public bool IsNotSet { get { return x == -1 && y == -1; } }
        public static GridPosition UnSetGridPosition = new GridPosition(-1, -1);

        #region operator overloads

        public static GridPosition operator+(GridPosition lhs, GridPosition rhs)
        {
            return new GridPosition(lhs.x + rhs.x, lhs.y + rhs.y);
        }

        public static GridPosition operator*(int factor, GridPosition direction)
        {
            return new GridPosition(factor * direction.x, factor * direction.y);
        }

        #endregion

        /// <summary>
        /// gets the neighbor from the current grid position in the queried direction
        /// </summary>
        /// <param name="direction">the direction in which to get the neighbor</param>
        /// <returns>the neighbor's gridposition</returns>
        public GridPosition Neighbor(Direction direction)
        {
            return this + direction.ToGridPosition();
        }

        /// <summary>
        /// turns this struct into a byte representation
        /// </summary>
        /// <returns>a byte representation of this struct</returns>
        public byte[] Serialize()
        {
            byte[] data = new byte[8];
            Array.Copy(Endianness.ToBigEndian(BitConverter.GetBytes(x)), 0, data, 0, 4);
            Array.Copy(Endianness.ToBigEndian(BitConverter.GetBytes(y)), 0, data, 4, 4);
            return data;
        }

        public override string ToString()
        {
            return "[" + x + "," + y + "]";
        }
    }
}
