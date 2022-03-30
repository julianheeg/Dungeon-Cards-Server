using Card_Mage_Server.Game_Files.MapFolder;
using Card_Mage_Server.Game_Files.Maze_Generation;
using System;

namespace Card_Mage_Server.Game_Files
{
    /// <summary>
    /// an enumeration of possible maze generation types
    /// </summary>
    public enum MazeGeneratorType { NULL, DFS, GrowingTree, ThreeMaze }

    /// <summary>
    /// an abstract class that represents a maze generator. The maze that is generated is specified by the child classes. Mazes are made up of a top level and a lower level generator
    /// </summary>
    public abstract class MazeGenerator
    {
        protected Game game;
        protected Map map;
        protected MazeGeneratorType subMazeGeneratorType;
        protected PlayerBase playerBaseGenerator;

        protected Random rng;

        protected MazeGenerator(Map map, GridPosition startPosition, MazeGeneratorType subMazeGeneratorType, PlayerBaseType playerBaseType)
        {
            rng = new Random();
            this.map = map;
            this.subMazeGeneratorType = subMazeGeneratorType;

            playerBaseGenerator = PlayerBase.Instantiate(playerBaseType);

            GenerateMaze(startPosition);
        }

        public abstract void GenerateMaze(GridPosition start);

        /// <summary>
        /// static constructor for super mazes
        /// </summary>
        /// <param name="game"></param>
        /// <param name="map"></param>
        /// <param name="startPosition"></param>
        /// <param name="subMazeGeneratorType"></param>
        /// <param name="playerBaseType"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static MazeGenerator Instantiate(Map map, GridPosition startPosition, MazeGeneratorType type, MazeGeneratorType subMazeGeneratorType, PlayerBaseType playerBaseType)
        {
            switch (type)
            {
                case MazeGeneratorType.DFS:
                    return new DFSGenerator(map, startPosition, subMazeGeneratorType, playerBaseType);
                case MazeGeneratorType.GrowingTree:
                    return new GrowingTreeGenerator(map, startPosition, subMazeGeneratorType, playerBaseType);
                case MazeGeneratorType.ThreeMaze:
                    return new ThreeMazeGenerator(map, startPosition, subMazeGeneratorType, playerBaseType);
                default:
                    throw new NotImplementedException("MazeGenerator.Instantiate(): switch statement yielded default.");
            }
        }

        /// <summary>
        /// static constructor for submazes
        /// </summary>
        /// <param name="game"></param>
        /// <param name="map"></param>
        /// <param name="startPosition"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        protected static MazeGenerator Instantiate(Game game, Map map, GridPosition startPosition, MazeGeneratorType type)
        {
            return Instantiate(map, startPosition, type, MazeGeneratorType.NULL, PlayerBaseType.NULL);
        }
    }
}
