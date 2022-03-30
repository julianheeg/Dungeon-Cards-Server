using Card_Mage_Server.Game_Files.MapFolder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Card_Mage_Server.Game_Files.Cards.CardTypes
{
    /// <summary>
    /// A class that represents a monster card. Derived from Card and holds additional health, damage and movement range information.
    /// </summary>
    class MonsterCard : Card
    {
        public readonly int health;
        public readonly int damage;
        public readonly int movementRange;

        /// <summary>
        /// constructor. Call this from the factory class in Card.
        /// </summary>
        /// <param name="template">the template that corresponds to this card</param>
        /// <param name="numberOfPlayers">the number of players. Used for the base class constructor</param>
        public MonsterCard(CardTemplate template, int numberOfPlayers) : base(template, numberOfPlayers)
        {
            health = ((MonsterCardTemplate)template).health;
            damage = ((MonsterCardTemplate)template).damage;
            movementRange = ((MonsterCardTemplate)template).movementRange;
        }

        /// <summary>
        /// checks whether this monster can be spawned onto the specified position
        /// </summary>
        /// <param name="position">the position to spawn the monster to</param>
        /// <param name="validSpawnLocations">valid locations where this monster could be spawned</param>
        /// <returns></returns>
        public bool Spawnable(GridPosition position, GridPosition[] validSpawnLocations)
        {
            if (position.IsNotSet)
            {
                Console.WriteLine("MonsterCard.Activatable(): could not check valid spawn position because position is not set");
                return false;
            }
            else
            {
                if (validSpawnLocations.Contains(position))
                {
                    //todo: check for effects that inhibit spawning
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// add health, damage and movement range to the toString method
        /// </summary>
        /// <returns></returns>
        protected override string GetAdditionalToStringAttributes()
        {
            return ", health: " + health + ", damage: " + damage + ", movementRange: " + movementRange;
        }

    }
}
