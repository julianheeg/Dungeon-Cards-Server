using Card_Mage_Server.Game_Files;
using Card_Mage_Server.Game_Files.Cards;
using Card_Mage_Server.Game_Files.Cards.CardTypes;
using Card_Mage_Server.Game_Files.Maze_Generation;
using Card_Mage_Server.Game_Files.Monsters;
using Card_Mage_Server.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Card_Mage_Server.Game_Files.MapFolder
{
    /// <summary>
    /// a part of the map class that represents a map on which the game is played
    /// </summary>
    public partial class Map
    {
        readonly Game game;
        readonly GridPosition[] playerPositions;

        public readonly int width, length;
        private readonly bool isHexGrid;
        readonly TileType[,] map;

        Random rng;


        #region Map Generation and Setup

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="game">the game that uses this map</param>
        /// <param name="width">the width of the map</param>
        /// <param name="length">the length of the map</param>
        /// <param name="isHexGrid">whether this map is a hexagonal grid or a rectangular grid</param>
        /// <param name="mazeType">the top tier type of maze generator</param>
        /// <param name="subMazeType">the lower tier type of maze generator</param>
        /// <param name="playerBaseType">the player base type used for this map</param>
        public Map(Game game, int width, int length, bool isHexGrid, MazeGeneratorType mazeType, MazeGeneratorType subMazeType, PlayerBaseType playerBaseType)
        {
            this.game = game;
            this.width = width;
            this.length = length;
            this.isHexGrid = isHexGrid;
            playerPositions = new GridPosition[game.numberOfPlayers];

            rng = new Random();

            map = new TileType[length,width];

            InitializeShape();
            GenerateMaze(mazeType, subMazeType, playerBaseType);


            monsterDictionary = new Dictionary<int, Monster>();
            nextInstanceID = 0;

            fogsOfWar = new FogOfWar[game.numberOfPlayers];
            for (int i = 0; i < game.numberOfPlayers; i++)
            {
                fogsOfWar[i] = new FogOfWar(this, i);
            }
        }

        /// <summary>
        /// separates the tiles that are within bounds from the tiles out of bounds (in case of hexagon map
        /// after calling this method, all tiles within bounds will have state "uninitialized"
        /// the tiles out of bounds will have state "out of bounds"
        /// </summary>
        private void InitializeShape()
        {
            if (isHexGrid)
            {
                for (int i = 0; i < length; i++)
                    for (int j = 0; j < width; j++)
                        if (i - j <= width / 2 && j - i <= width / 2)
                        {
                            SetTile(new GridPosition(i, j), TileType.Uninitialized);
                        }
                        else
                        {
                            SetTile(new GridPosition(i, j), TileType.Out_Of_Bounds);
                        }
            }
            else
            {
                for (int i = 0; i < length; i++)
                    for (int j = 0; j < width; j++)
                        SetTile(new GridPosition(i, j), TileType.Uninitialized);
            }
        }

        /// <summary>
        /// returns all the uninitialized neighbors of a cell which are distance 2 away
        /// </summary>
        /// <param name="pos">the position whose neighbors should be listed</param>
        /// <returns>the neighbors array</returns>
        public GridPosition[] UninitializedNeighbors(GridPosition pos)
        {
            List<GridPosition> unvisitedNeighbors = new List<GridPosition>();

            int x = pos.x;
            int y = pos.y;

            //the four axis
            //down right
            if (x - 2 > 0 && GetTile(new GridPosition(x - 2, y)) == TileType.Uninitialized && GetTile(new GridPosition(x - 1, y)) == TileType.Uninitialized)
            {
                unvisitedNeighbors.Add(new GridPosition(x - 2, y));
            }
            //up left
            if (x + 2 < length && GetTile(new GridPosition(x + 2, y)) == TileType.Uninitialized && GetTile(new GridPosition(x + 1, y)) == TileType.Uninitialized)
            {
                unvisitedNeighbors.Add(new GridPosition(x + 2, y));
            }
            //left
            if (y - 2 > 0 && GetTile(new GridPosition(x, y - 2)) == TileType.Uninitialized && GetTile(new GridPosition(x, y - 1)) == TileType.Uninitialized)
            {
                unvisitedNeighbors.Add(new GridPosition(x, y - 2));
            }
            //right
            if (y + 2 < width && GetTile(new GridPosition(x, y + 2)) == TileType.Uninitialized && GetTile(new GridPosition(x, y + 1)) == TileType.Uninitialized)
            {
                unvisitedNeighbors.Add(new GridPosition(x, y + 2));
            }

            //two additional axis for hex grid
            if (isHexGrid)
            {
                //down left
                if (x - 2 > 0 && y - 2 > 0 && GetTile(new GridPosition(x - 2, y - 2)) == TileType.Uninitialized && GetTile(new GridPosition(x - 1, y - 1)) == TileType.Uninitialized)
                {
                    unvisitedNeighbors.Add(new GridPosition(x - 2, y - 2));
                }
                //up right
                if (x + 2 < length && y + 2 < width && GetTile(new GridPosition(x + 2, y + 2)) == TileType.Uninitialized && GetTile(new GridPosition(x + 1, y + 1)) == TileType.Uninitialized)
                {
                    unvisitedNeighbors.Add(new GridPosition(x + 2, y + 2));
                }
            }
            return unvisitedNeighbors.ToArray();
        }

        /// <summary>
        /// selects a random starting position for maze generation, instantiates the maze generator and lets it generate a maze
        /// </summary>
        /// <param name="mazeType"></param>
        /// <param name="subMazeType"></param>
        /// <param name="playerBaseType"></param>
        private void GenerateMaze(MazeGeneratorType mazeType, MazeGeneratorType subMazeType, PlayerBaseType playerBaseType)
        {
            //determine random starting position, try again if out of bounds
            int randX;
            int randY;
            GridPosition start;
            do
            {
                randX = rng.Next(length / 2) * 2 + 1; //offsets used because the grid can only be generated correctly if the x and y coordinates are odd
                randY = rng.Next(width / 2) * 2 + 1;
                start = new GridPosition(randX, randY);
            }
            while (isHexGrid && GetTile(start) == TileType.Out_Of_Bounds);


            //instantiate the maze generator and generate a maze
            MazeGenerator.Instantiate(this, start, mazeType, subMazeType, playerBaseType);
        }

        #endregion


        /// <summary>
        /// sets the tile at the specified grid position. Also sets the player position if the tile type is a player type
        /// </summary>
        /// <param name="position">the position at which the type should be set</param>
        /// <param name="type">the tile type that the tile should be set to</param>
        public void SetTile(GridPosition position, TileType type)
        {
            map[position.x, position.y] = type;

            if (type.IsPlayer())//&& type.GetPlayerIndex() == 0)
            {
                //ConsoleExt.WriteLine("Map.SetTile(): For test purposes, this function currently checks only for the first player!!!!", ConsoleColor.Red);
                playerPositions[type.GetPlayerIndex()] = position;
            }
        }

        /// <summary>
        /// returns the tile type at the specified position
        /// </summary>
        /// <param name="position">the position at which the tile type is requested</param>
        /// <returns>the tile type at that position</returns>
        public TileType GetTile(GridPosition position)
        {
            return map[position.x, position.y];
        }


        /// <summary>
        /// returns all the traversible neighbors of a grid cell
        /// </summary>
        /// <param name="pos">the position of that cell</param>
        /// <returns>an array of all the neighbors of that cell</returns>
        public GridPosition[] Neighbors(GridPosition pos)
        {
            int x = pos.x;
            int y = pos.y;
            List<GridPosition> neighbors = new List<GridPosition>();

            if (isHexGrid)
            {
                //the six axis
                foreach (Direction direction in Enum.GetValues(typeof(Direction)))
                {
                    if (GetTile(pos.Neighbor(direction)) == TileType.Traversible)
                    {
                        neighbors.Add(pos.Neighbor(direction));
                    }
                }
            }
            else
            {
                /*
                //the four axis
                if (map[x - 1][y] == TileType.Traversible)
                    neighbors.Add(new GridPosition(x - 1, y));
                if (map[x + 1][y] == TileType.Traversible)
                    neighbors.Add(new GridPosition(x + 1, y));
                if (map[x][y - 1] == TileType.Traversible)
                    neighbors.Add(new GridPosition(x, y + 1));
                if (map[x][y - 1] == TileType.Traversible)
                    neighbors.Add(new GridPosition(x, y - 1));

                //two additional axis for hex grid
                if (isHexGrid)
                {
                    if (map[x - 1][y - 1] == TileType.Traversible)
                        neighbors.Add(new GridPosition(x - 1, y - 1));
                    if (map[x + 1][y + 1] == TileType.Traversible)
                        neighbors.Add(new GridPosition(x + 1, y + 1));
                }
                */
            }

            return neighbors.ToArray();
        }

        /// <summary>
        /// gets all the tiles visible from a the viewer position
        /// </summary>
        /// <param name="viewer">the position from which to look</param>
        /// <returns>an array of visible tiles</returns>
        public GridPosition[] GetVisibleTiles(GridPosition viewer)
        {
            List<GridPosition> visibleTiles = new List<GridPosition>();
            visibleTiles.Add(viewer);

            //loop over all directions
            foreach (Direction direction in Enum.GetValues(typeof(Direction)))
            {
                //check all tiles in this direction and add them to the list. exit the loop early, if a tile can't be seen through
                for (int i = 1; i <= Monster.visionRange; i++)
                {
                    GridPosition tileToCheck = viewer + i * direction.ToGridPosition();
                    visibleTiles.Add(tileToCheck);
                    if (!GetTile(tileToCheck).SeeThrough())
                    {
                        break;
                    }
                }
            }
            return visibleTiles.ToArray();
        }

        /// <summary>
        /// checks whether the player with this index or their monsters have line of sight with the position
        /// </summary>
        /// <param name="position">the position</param>
        /// <param name="playerIndex">the player index</param>
        /// <returns>true, if visible, and false otherwise</returns>
        public bool IsVisibleThroughFoW(GridPosition position, int playerIndex)
        {
            return fogsOfWar[playerIndex].Visible(position);
        }

        /// <summary>
        /// returns the position of a player
        /// </summary>
        /// <param name="index">the index of the player</param>
        /// <returns>the player's position</returns>
        public GridPosition GetPlayerPosition(int index)
        {
            return playerPositions[index];
        }

        /// <summary>
        /// gets an array for valid spawn points for monsters for the specified player
        /// </summary>
        /// <returns>an array of valid spawn points</returns>
        private GridPosition[] GetValidSpawnPoints(int playerIndex)
        {
            return Neighbors(playerPositions[playerIndex]);
        }

        /// <summary>
        /// checks wether the card can be activated on the specified position and if so, activates it
        /// </summary>
        /// <param name="card">the card to activate</param>
        /// <param name="position">the position at which the card is to be activated</param>
        /// <returns></returns>
        public void TryCardActivation(Card card, GridPosition position, Player player, int playerIndex)
        {
            if (card.owner == playerIndex)
            {
                switch (card.type)
                {
                    case Cards.CardTypes.Type.Monster:
                        MonsterCard monsterCard = (MonsterCard)card;
                        GridPosition[] validSpawnPoints = GetValidSpawnPoints(card.owner);
                        if(monsterCard.Spawnable(position, validSpawnPoints))
                        {
                            SpawnMonster(monsterCard, position);                
                        }
                        else
                        {
                            Console.WriteLine("Map.TryCardActivation(): the monster card {0} could not be spawned at {1}. Valid spawn positions are {2}.", card.ToString(), position.ToString(), ArrayExtension.ToString(validSpawnPoints));
                        }
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
            else
            {
                Console.WriteLine("Map.TryCardActivation(): player {0} tried to activate card {1}, but isn't the owner of the card. The owner is {2}.", player.ToString(), card.ToString(), card.owner);
            }
        }


        #region Serialization

        /// <summary>
        /// serializes the map's metadata
        /// </summary>
        /// <returns>the serialized metadata</returns>
        public byte[] SerializeMetaData()
        {
            //initialize list
            List<byte> bytesList = new List<byte>(9);

            //add metadata
            bytesList.AddRange(Endianness.ToBigEndian(BitConverter.GetBytes(length)));
            bytesList.AddRange(Endianness.ToBigEndian(BitConverter.GetBytes(width)));
            bytesList.AddRange(BitConverter.GetBytes(isHexGrid));

            Console.WriteLine("-------------------------------------------------------\nStarting Game and sending Map data to clients:");

            return bytesList.ToArray();
        }

        /// <summary>
        /// serializes a row
        /// </summary>
        /// <param name="row">the row to serialize</param>
        /// <returns>the serialized row</returns>
        public byte[] SerializeRow(int row)
        {
            //initialize list
            List<byte> bytesList = new List<byte>(width);

            //add data
            for (int i = 0; i < width; i++)
            {
                bytesList.Add((byte)GetTile(new GridPosition(row, i)));
                Console.Write(" {0}", (byte)GetTile(new GridPosition(row, i)));
            }
            Console.WriteLine("");

            return bytesList.ToArray();
        }

        #endregion
    }
}
