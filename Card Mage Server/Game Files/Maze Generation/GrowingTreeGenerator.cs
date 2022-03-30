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
    /// a maze generation class that generates a maze according to the growing tree generation algorithm where the cells are chosen at random
    /// </summary>
    public class GrowingTreeGenerator : MazeGenerator
    {
        //constructor
        public GrowingTreeGenerator(Map map, GridPosition startPosition, MazeGeneratorType subMazeGeneratorType, PlayerBaseType playerBaseType)
            : base(map, startPosition, subMazeGeneratorType, playerBaseType) { }

        /// <summary>
        /// maze generation
        /// </summary>
        public override void GenerateMaze(GridPosition start)
        {
            //setup list
            List<GridPosition> gridPositions = new List<GridPosition> { start };
            map.SetTile(start, TileType.Traversible);

            //iterate
            while (gridPositions.Count > 0)
            {
                int randomIndex = rng.Next(gridPositions.Count);
                GridPosition currentPos = gridPositions[randomIndex];

                GridPosition[] unvisitedNeighbors = map.UninitializedNeighbors(currentPos);
                if (unvisitedNeighbors.Length > 0)
                {
                    int randomInd = rng.Next(unvisitedNeighbors.Length);
                    GridPosition nextPos = unvisitedNeighbors[randomInd];
                    GridPosition wayPos = new GridPosition((nextPos.x + currentPos.x) / 2, (nextPos.y + currentPos.y) / 2);
                    map.SetTile(nextPos, TileType.Traversible);
                    map.SetTile(wayPos, TileType.Traversible);
                    gridPositions.Add(nextPos);
                }
                else
                    gridPositions.RemoveAt(randomIndex);

            }

        }
    }
}
