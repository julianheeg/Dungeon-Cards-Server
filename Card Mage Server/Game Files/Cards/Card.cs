using Card_Mage_Server.Game_Files.Cards.CardTypes;
using System;
using System.Collections.Generic;
using System.Threading;
using Type = Card_Mage_Server.Game_Files.Cards.CardTypes.Type;

namespace Card_Mage_Server.Game_Files.Cards
{
    /// <summary>
    /// an abstract class that represents a card. The instances of this class are the actual typed cards like MonsterCard, etc.
    /// Fields are:
    ///     - owner and location (The owner is represented as the player number within the game this card is in. Location is Deck, Hand, Field, Graveyard.)
    ///     - cardID (for recognition of the card template on the client side)
    ///     - instanceID (for identification of this exact card on the client side. In contrast to cardID, instanceID is unique)
    ///     - known array (which players know the cardID already?)
    ///     - effects (which effects does this card apply?)
    ///     - cost
    ///     - card type
    ///     - affectedBy (which effects is this card currently affected by?)
    /// </summary>
    public abstract class Card
    {
        public enum Location { Deck, Hand, Field, Graveyard }

        static int instanceIDCounter = 0; //incremented in the constructor so that each card has a unique instanceID

        public readonly int cardID;
        public readonly int instanceID;
        public int owner;
        public Location location;

        public readonly bool[] known; //which players know the face of the card? true iff the player with the same index knows of the card

        public readonly Effect[] effects;
        public readonly int cost;
        public readonly Type type;
        public List<Effect> affectedBy;

        /// <summary>
        /// Constructor of the abstract class. Copies the contents of a template and adds the relevant fields.
        /// Use the factory class for actual instantiation.
        /// </summary>
        /// <param name="template">the template that corresponds to this card</param>
        /// <param name="numberOfPlayers">the number of players in this game (used for creating the known array)</param>
        protected Card(CardTemplate template, int numberOfPlayers)
        {
            cardID = template.id;

            instanceID = Interlocked.Increment(ref instanceIDCounter); //needs to be thread safe

            cost = template.cost;
            type = template.type;
            effects = template.effects;

            affectedBy = new List<Effect>();

            known = new bool[numberOfPlayers];
            for(int i = 0; i < numberOfPlayers; i++)
            {
                known[i] = false;
            }
        }

        /// <summary>
        /// Factory class that instantiates a card.
        /// </summary>
        /// <param name="template">the template that corresponds to this card</param>
        /// <param name="numberOfPlayers">the number of players in this game (used for creating the known array)</param>
        /// <returns></returns>
        public static Card Instantiate(int cardID, int numberOfPlayers)
        {
            CardTemplate template = CardDatabase.IDToTemplate(cardID);
            switch (template.type)
            {
                case Type.Monster:
                    return new MonsterCard((MonsterCardTemplate)template, numberOfPlayers);
                default:
                    throw new NotImplementedException();
            }
        }

        //toString method
        public override string ToString()
        {
            return "[cardID: " + cardID + ", instanceID: " + instanceID + ", owner: " + owner + ", location: " + location + ", type: " + type + ", cost: " + cost + GetAdditionalToStringAttributes() + "]";
        }

        //child classes might want to add more to the toString method
        protected virtual string GetAdditionalToStringAttributes() { return ""; }
    }
}
