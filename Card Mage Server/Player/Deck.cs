using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Card_Mage_Server
{
    /// <summary>
    /// a class which represents a deck
    /// </summary>
    public struct Deck
    {
        public readonly int[] cardIDs;

        public int Length
        {
            get
            {
                return cardIDs.Length;
            }
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="cardIDs">a list of the card IDs of the cards in this deck</param>
        public Deck(List<int> cardIDs)
        {
            this.cardIDs = cardIDs.ToArray();
        }

        /// <summary>
        /// a default deck of cards
        /// </summary>
        /// <returns>the default deck</returns>
        public static Deck Default() {
            List<int> defaultCards = new List<int>(20);
            for (int i = 0; i < defaultCards.Capacity / 2; i++)
            {
                defaultCards.Add(0);
				defaultCards.Add(1);
            }
            return new Deck(defaultCards);
        }
    }
}
