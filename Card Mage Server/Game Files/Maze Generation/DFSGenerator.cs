using Card_Mage_Server.Game_Files.MapFolder;
using Card_Mage_Server.Game_Files.Maze_Generation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Card_Mage_Server.Game_Files
{
    /// <summary>
    /// a maze generation class that does the generation based on a depth-first-search
    /// </summary>
    class DFSGenerator : MazeGenerator
    {
        //constructor
        public DFSGenerator(Map map, GridPosition startPosition, MazeGeneratorType subMazeGeneratorType, PlayerBaseType playerBaseType)
            : base (map, startPosition, subMazeGeneratorType, playerBaseType) { }

        //generation
        public override void GenerateMaze(GridPosition start)
        {
            map.SetTile(start, TileType.Traversible);

            Recursion(start.x, start.y);
        }

        private void Recursion(int r, int c)
        {
            GridPosition[] neighbors = map.UninitializedNeighbors(new GridPosition(r, c));
            int[] randoms = GenerateRandomDirections(neighbors.Length);
            for (int i = 0; i < neighbors.Length; i++)
            {
                int x = neighbors[randoms[i]].x;
                int y = neighbors[randoms[i]].y;

                //check if the possible directions have not changed
                if (map.GetTile(new GridPosition(x, y)) == TileType.Uninitialized)
                {
                    map.SetTile(new GridPosition(x, y), TileType.Traversible);
                    map.SetTile(new GridPosition((x + r) / 2, (y + c) / 2), TileType.Traversible); //set the connection as well
                    Recursion(x, y);
                }
            }
        }


        /// <summary>
        /// Generate an array with random directions 1-6. 
        /// </summary>
        /// <param name="max">the amount of random directions</param>
        /// <returns>Array containing 4 directions in random order</returns>
        private int[] GenerateRandomDirections(int max)
        {
            List<int> numbers = new List<int>();
            for (int i = 0; i < max; i++)
                numbers.Add(i);

            int[] randoms = new int[max];
            for (int i = 0; i < max; i++)
            {
                int randomIndex = rng.Next(max - i);
                randoms[i] = numbers[randomIndex];
                numbers.RemoveAt(randomIndex);
            }
            return randoms;
        }
    }
}
