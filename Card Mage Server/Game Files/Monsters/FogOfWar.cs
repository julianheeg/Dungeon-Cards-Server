using Card_Mage_Server.Game_Files.MapFolder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Card_Mage_Server.Game_Files.Monsters
{
    /// <summary>
    /// a class that represents fog of war. each player gets one of these from within the monster manager
    /// </summary>
    class FogOfWar
    {
        readonly Map map;
        readonly bool[,] visibleTiles;
        readonly GridPosition playerPosition;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="map">the map</param>
        /// <param name="monsterManager">the monster manager associated with this game</param>
        /// <param name="playerIndex">the player whose FoW object this is</param>
        public FogOfWar(Map map, int playerIndex)
        {
            this.map = map;
            visibleTiles = new bool[map.length, map.width];
            playerPosition = map.GetPlayerPosition(playerIndex);
        }

        /// <summary>
        /// rebuilds the fog of war map
        /// </summary>
        public void UpdateMonsterVision()
        {
            //reset all tiles
            for(int i = 0; i < visibleTiles.GetLength(0); i++)
            {
                for(int j = 0; j < visibleTiles.GetLength(1); j++)
                {
                    visibleTiles[i, j] = false;
                }
            }

            //set all the tiles visible by the player to true
            GridPosition[] tilesVisibleByPlayer = map.GetVisibleTiles(playerPosition);
            for (int i = 0; i < tilesVisibleByPlayer.Length; i++)
            {
                int x = tilesVisibleByPlayer[i].x;
                int y = tilesVisibleByPlayer[i].y;
                visibleTiles[x, y] = true;
            }

            //set all tiles visible by a monster to true
            foreach (KeyValuePair<int, Monster> entry in map.monsterDictionary)
            {
                Monster monster = entry.Value;
                GridPosition[] tilesVisibleByMonster = map.GetVisibleTiles(monster.position);
                for(int i = 0; i < tilesVisibleByMonster.Length; i++)
                {
                    int x = tilesVisibleByMonster[i].x;
                    int y = tilesVisibleByMonster[i].y;
                    visibleTiles[x, y] = true;
                }
            }
        }

        /// <summary>
        /// checks whether the specified tile is visible
        /// </summary>
        /// <param name="position">the tile to check</param>
        /// <returns>whether the position is visible or not</returns>
        public bool Visible(GridPosition position)
        {
            return visibleTiles[position.x, position.y];
        }
    }
}
