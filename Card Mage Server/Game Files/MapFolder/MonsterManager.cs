using Card_Mage_Server.Game_Files.Cards;
using Card_Mage_Server.Game_Files.Cards.CardTypes;
using Card_Mage_Server.Game_Files.MapFolder;
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
    /// a part of the map class which which manages the monsters on the map
    /// </summary>
    public partial class Map
    {
        internal readonly Dictionary<int, Monster> monsterDictionary;
        int nextInstanceID;

        readonly FogOfWar[] fogsOfWar;

        /// <summary>
        /// spawns a monster onto the field
        /// </summary>
        /// <param name="card">the card containing the monster to spawn</param>
        /// <param name="position">the position at which to spawn the monster</param>
       void SpawnMonster(MonsterCard card, GridPosition position)
        {
            Console.WriteLine("spawning monster to " + position.ToString());

            Monster monster = new Monster(nextInstanceID, card, position);
            monsterDictionary.Add(nextInstanceID, monster);

            fogsOfWar[card.owner].UpdateMonsterVision();

            Messages.SendMonsterSpawn(game, card, position, nextInstanceID);

            nextInstanceID++;
        }

        /// <summary>
        /// checks whether a given monster can be moved along a given path and if so, performs the movement
        /// </summary>
        /// <param name="monsterInstanceID">the instance ID of the monster to move</param>
        /// <param name="path">the path where to move the monster along</param>
        /// <param name="playerIndex">the index of the player who issued the movement command</param>
        public void TryMonsterMovement(int monsterInstanceID, GridPosition[] path, int playerIndex)
        {
            if (monsterDictionary.TryGetValue(monsterInstanceID, out Monster monster))
            {
                if (monster.owner == playerIndex)
                {
                    GridPosition currentPosition = monster.position;
                    for (int i = 0; i < path.Length; i++)
                    {
                        if (Array.Exists(Neighbors(currentPosition), neighbor => neighbor.Equals(path[i])))
                        {
                            currentPosition = path[i];
                        }
                        else
                        {
                            Console.WriteLine("Map.TryMonsterMovement() [MonsterManager.cs]: Tried to move monster {0} along path {1}, but the tile at step {2} has type {3}.", monster, ArrayExtension.ToString<GridPosition>(path), i, GetTile(path[i]));
                            return;
                        }
                    }
                    monster.position = currentPosition;
                    Console.WriteLine("Map.TryMonsterMovement() [MonsterManager.cs]: Moved monster {0} along path {1}.", monster, ArrayExtension.ToString<GridPosition>(path));
                    throw new NotImplementedException("TODO: send clients the monster movement");
                }
                else
                {
                    Console.WriteLine("Map.TryMonsterMovement() [MonsterManager.cs]: Player {0} tried to move monster {1} along path {2}, but they're not the owner of it.", playerIndex, monster, ArrayExtension.ToString<GridPosition>(path));
                }
            }
            else
            {
                Console.WriteLine("Map.TryMonsterMovement() [MonsterManager.cs]: Player {0} tried to move the monster with ID {1} along path {2}, but the ID doesn't exist in this game.", playerIndex, monsterInstanceID, ArrayExtension.ToString<GridPosition>(path));
            }
        }
    }
}
