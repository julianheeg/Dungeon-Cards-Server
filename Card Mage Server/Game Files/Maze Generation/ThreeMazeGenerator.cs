using Card_Mage_Server.Game_Files.MapFolder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Card_Mage_Server.Game_Files.Maze_Generation
{
    /// <summary>
    /// a top level maze generator that will build the player bases, then extend to "arms" from them and connect them in the middle.
    /// A lower level maze generator will then generate mazes within the regions separated by these arms.
    /// The connections in the middle will be removed afterwards and random wall tiles will be turned into empty tiles.
    /// </summary>
    public class ThreeMazeGenerator : MazeGenerator
    {
        public ThreeMazeGenerator(Map map, GridPosition startPosition, MazeGeneratorType subMazeGeneratorType, PlayerBaseType playerBaseType)
            : base(map, startPosition, subMazeGeneratorType, playerBaseType) { }

        private int length, width;
        private int player1Y, player2Y;
        private int lastX1, lastY1;
        private int lastX2, lastY2;
        private int lastX3, lastY3;
        private int lastX4, lastY4;
        private int sixth;
        private int minLeft, maxLeft;
        private int minRight, maxRight;

        public override void GenerateMaze(GridPosition start)
        {
            length = map.length;
            width = map.width;

            player1Y = rng.Next(Config.MinDistanceToWall / 2, (width / 2 - Config.MinDistanceToWall + 1) / 2) * 2 + 1;
            player2Y = rng.Next((width / 2 + Config.MinDistanceToWall - 1) / 2, (width - 1 - Config.MinDistanceToWall) / 2) * 2 + 1;
            ConsoleExt.WriteLine("ThreeMazeGenerator.GenerateMaze(): player1Y = " + player1Y + ", player2Y = " + player2Y, ConsoleColor.Cyan);

            //generate player bases
            if (playerBaseGenerator != null)
            {
                playerBaseGenerator.GenerateBases(map, player1Y, player2Y);
            }


            //------------procedural generation---------------
            sixth = (int)((double)width / 6);

            //player 1 side
            //left arm
            GenerateBottomLeftArm();
            //right arm
            GenerateBottomRightArm();

            //player 2 side
            //left arm
            GenerateTopLeftArm();
            //right arm
            GenerateTopRightArm();

            //middle border
            GenerateMiddleBorder();

            //submazes
            if (subMazeGeneratorType != MazeGeneratorType.NULL)
            {
                GenerateSubMazes();
            }

            RemoveTempWalls();
            Randomify();
        }

        
        
        /// <summary>
        /// generates the bottom left arm
        /// </summary>
        private void GenerateBottomLeftArm()
        {
            lastX1 = 4;
            lastY1 = player1Y - 1;

            while (lastX1 < map.length / 2)
            {
                int dir = lastY1 <= sixth ? 0 : rng.Next(5);
                switch (dir)
                {
                    case 0:
                    case 1:
                        map.SetTile(new GridPosition(++lastX1, ++lastY1), GetTemp1orTemp2());
                        if (lastX1 < map.length / 2)
                        {
                            map.SetTile(new GridPosition(++lastX1, lastY1), GetTemp1orTemp2());
                        }
                        break;
                    case 2:
                    case 3:
                        map.SetTile(new GridPosition(++lastX1, lastY1), GetTemp1orTemp2());
                        break;
                    case 4:
                        map.SetTile(new GridPosition(lastX1, --lastY1), GetTemp1orTemp2());
                        map.SetTile(new GridPosition(++lastX1, lastY1), GetTemp1orTemp2());
                        break;
                }
            }
        }

        /// <summary>
        /// generates the bottom right arm
        /// </summary>
        private void GenerateBottomRightArm()
        {
            lastX2 = 4;
            lastY2 = player1Y + 4;

            while (lastX2 < map.length / 2)
            {
                int dir = lastY2 - lastX2 > sixth * 3 ? 0 : rng.Next(5);
                switch (dir)
                {
                    case 0:
                    case 1:
                        map.SetTile(new GridPosition(++lastX2, lastY2), GetTemp1orTemp2());
                        if (lastX2 < map.length / 2)
                        {
                            map.SetTile(new GridPosition(++lastX2, ++lastY2), GetTemp1orTemp2());
                        }
                        break;
                    case 2:
                    case 3:
                        map.SetTile(new GridPosition(++lastX2, ++lastY2), GetTemp1orTemp2());
                        break;
                    case 4:
                        map.SetTile(new GridPosition(lastX2, ++lastY2), GetTemp1orTemp2());
                        map.SetTile(new GridPosition(++lastX2, ++lastY2), GetTemp1orTemp2());
                        break;
                }
            }
        }

        /// <summary>
        /// generates the top left arm
        /// </summary>
        private void GenerateTopLeftArm()
        {
            lastX3 = length - 5;
            lastY3 = player2Y - 4;

            while (lastX3 > map.length / 2)
            {
                int dir = lastX3 - lastY3 > width / 2 + sixth ? 0 : rng.Next(5);
                switch (dir)
                {
                    case 0:
                    case 1:
                        map.SetTile(new GridPosition(--lastX3, lastY3), GetTemp1orTemp2());
                        if (lastX3 > map.length / 2)
                        {
                            map.SetTile(new GridPosition(--lastX3, --lastY3), GetTemp1orTemp2());
                        }
                        break;
                    case 2:
                    case 3:
                        map.SetTile(new GridPosition(--lastX3, --lastY3), GetTemp1orTemp2());
                        break;
                    case 4:
                        map.SetTile(new GridPosition(lastX3, --lastY3), GetTemp1orTemp2());
                        map.SetTile(new GridPosition(--lastX3, --lastY3), GetTemp1orTemp2());
                        break;
                }
            }
        }

        /// <summary>
        /// generates the top right arm
        /// </summary>
        private void GenerateTopRightArm()
        {
            lastX4 = length - 5;
            lastY4 = player2Y + 1;

            while (lastX4 > map.length / 2)
            {
                int dir = lastY4 > width - sixth ? 1 : rng.Next(5);
                switch (dir)
                {
                    case 0:
                    case 1:
                        map.SetTile(new GridPosition(--lastX4, --lastY4), GetTemp1orTemp2());
                        if (lastX4 > map.length / 2)
                        {
                            map.SetTile(new GridPosition(--lastX4, lastY4), GetTemp1orTemp2());
                        }
                        break;
                    case 2:
                    case 3:
                        map.SetTile(new GridPosition(--lastX4, lastY4), GetTemp1orTemp2());
                        break;
                    case 4:
                        map.SetTile(new GridPosition(lastX4, ++lastY4), GetTemp1orTemp2());
                        map.SetTile(new GridPosition(--lastX4, lastY4), GetTemp1orTemp2());
                        break;
                }
            }
        }

        /// <summary>
        /// generates the border in the middle
        /// </summary>
        private void GenerateMiddleBorder()
        {
            map.SetTile(new GridPosition(lastX1, lastY1), TileType.TempMark2);
            map.SetTile(new GridPosition(lastX2, lastY2), TileType.TempMark2);
            map.SetTile(new GridPosition(lastX3, lastY3), TileType.TempMark2);
            map.SetTile(new GridPosition(lastX4, lastY4), TileType.TempMark2);

            //left border
            minLeft = lastY1 < lastY3 ? lastY1 : lastY3;
            maxLeft = minLeft == lastY1 ? lastY3 : lastY1;
            for (int i = minLeft + 1; i < maxLeft; i++)
            {
                map.SetTile(new GridPosition(length / 2, i), TileType.TempMark2);
            }

            //right border
            minRight = lastY2 < lastY4 ? lastY2 : lastY4;
            maxRight = minRight == lastY2 ? lastY4 : lastY2;
            for (int i = minRight + 1; i < maxRight; i++)
            {
                map.SetTile(new GridPosition(length / 2, i), TileType.TempMark2);
            }
        }

        /// <summary>
        /// generates the lower tier mazes
        /// </summary>
        private void GenerateSubMazes()
        {
            try
            {
                GridPosition leftMazeStart = new GridPosition(length / 2, rng.Next(0, minLeft / 2) * 2 + 1);
                GridPosition middleMazeStart = new GridPosition(length / 2, rng.Next(maxLeft / 2 + 1, minRight / 2) * 2);
                GridPosition rightMazeStart = new GridPosition(length / 2, rng.Next(maxRight / 2, width / 2 - 1) * 2 + 1);

                Instantiate(game, map, leftMazeStart, subMazeGeneratorType);
                Instantiate(game, map, middleMazeStart, subMazeGeneratorType);
                Instantiate(game, map, rightMazeStart, subMazeGeneratorType);
            }
            catch (ArgumentOutOfRangeException)
            {
                //TODO solve. Recent values:
                //maxRight: 26 <-- too large
                Console.WriteLine("-----------------------------------------------------------------------\n" +
                    "ArgumentOutOfRangeException in Generate Submazes:\n" +
                    "length = " + length + ", width = " + width + ", minLeft = " + minLeft + ", maxLeft = " + maxLeft + ", minRight = " + minRight + ", maxRight = " + maxRight + "\n" + 
                    "----------------------------------------------------------------------------------");
                throw;
            }
        }

        /// <summary>
        /// removes the temporary walls in the middle
        /// </summary>
        private void RemoveTempWalls()
        {
            //remove walls next to the middle wall
            //left side
            while (map.GetTile(new GridPosition(length / 2, minLeft - 1)) == TileType.Wall)
            {
                map.SetTile(new GridPosition(length / 2, minLeft - 1), TileType.TempMark2);
                minLeft--;
            }
            while (map.GetTile(new GridPosition(length / 2, maxLeft + 1)) == TileType.Wall)
            {
                map.SetTile(new GridPosition(length / 2, maxLeft + 1), TileType.TempMark2);
                maxLeft++;
            }
            //right side
            while (map.GetTile(new GridPosition(length / 2, minRight - 1)) == TileType.Wall)
            {
                map.SetTile(new GridPosition(length / 2, minRight - 1), TileType.TempMark2);
                minRight--;
            }
            while (map.GetTile(new GridPosition(length / 2, maxRight + 1)) == TileType.Wall)
            {
                map.SetTile(new GridPosition(length / 2, maxRight + 1), TileType.TempMark2);
                maxRight++;
            }

            //revert temporary and uninitialized tiles to wall or empty tile
            for (int i = 0; i < length; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (map.GetTile(new GridPosition(i, j)) == TileType.Uninitialized || map.GetTile(new GridPosition(i, j)) == TileType.TempMark1)
                    {
                        map.SetTile(new GridPosition(i, j), TileType.Wall);
                    }
                    else if (map.GetTile(new GridPosition(i, j)) == TileType.TempMark2)
                    {
                        map.SetTile(new GridPosition(i, j), TileType.Traversible);
                    }
                    
                }
            }
        }

        /// <summary>
        /// removes a few randomly selected walls
        /// </summary>
        private void Randomify()
        {
            for (int i = 1; i < length - 1; i++)
            {
                for (int j = 1; j < width - 1; j++)
                {
                    if (i - j < map.width / 2 && j - i < map.width / 2 && map.GetTile(new GridPosition(i, j)) == TileType.Wall && rng.NextDouble() <= Config.WallRemovalThreshold)
                    {
                        map.SetTile(new GridPosition(i, j), TileType.Traversible);
                    }
                }
            }
        }

        /// <summary>
        /// returns either TempMark1 or TempMark2, chosen with 50/50 chance
        /// </summary>
        /// <returns>TempMark1 or TempMark2</returns>
        private TileType GetTemp1orTemp2()
        {
            return rng.Next(2) == 0 ? TileType.TempMark1 : TileType.TempMark2;
        }
    }
}
