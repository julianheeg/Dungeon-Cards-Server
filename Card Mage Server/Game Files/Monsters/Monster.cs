using Card_Mage_Server.Game_Files.Cards;
using Card_Mage_Server.Game_Files.Cards.CardTypes;
using Card_Mage_Server.Game_Files.MapFolder;
using System;
using System.Collections.Generic;

namespace Card_Mage_Server.Game_Files.Monsters
{
    /// <summary>
    /// a class which represents a monster on the map
    /// </summary>
    class Monster
    {
        public static readonly int visionRange = 3;

        public int instanceID;
        public int owner;
        public GridPosition position;
        public int MaxHealth { get; private set; }
        public int CurrentHealth { get; private set; }
        public int Cost { get; private set; }
        public int Damage { get; private set; }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="instanceID">the instanceID of this monster (unique)</param>
        /// <param name="card">the card that this monster is made from</param>
        /// <param name="position">the position of this monster</param>
        public Monster(int instanceID, MonsterCard card, GridPosition position)
        {
            this.instanceID = instanceID;
            this.owner = card.owner;
            this.position = position;
            
            MaxHealth = card.health;
            CurrentHealth = MaxHealth; //todo look for modifications
            Cost = card.cost;
            Damage = card.damage;
        }

        /// <summary>
        /// moves the monster to the specified tile
        /// </summary>
        /// <param name="position">the position to move the monster to</param>
        public void MoveTo(GridPosition position)
        { 
            throw new NotImplementedException();
        }
    }
}
