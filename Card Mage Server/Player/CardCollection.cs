using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Card_Mage_Server
{
    /// <summary>
    /// a class which represents the card collection that a player possesses
    /// </summary>
    public class CardCollection
    {
        public int[] cardAmounts;

        /// <summary>
        /// constructor
        /// </summary>
        public CardCollection()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// adds a card to the collection
        /// </summary>
        /// <param name="cardID">the card ID of that card</param>
        public void Add(int cardID)
        {
            cardAmounts[cardID]++;
        }
    }
}
