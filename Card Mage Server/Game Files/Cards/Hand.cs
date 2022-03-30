using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Card_Mage_Server.Game_Files.Cards
{
    /// <summary>
    /// a class that represents the hand of a player
    /// </summary>
    public class Hand
    {
        List<Card> cards;
        Random rng;

        /// <summary>
        /// constructor
        /// </summary>
        public Hand()
        {
            cards = new List<Card>(Config.AmountOfCardsInHandAtTheBeginning * 2);
        }

        /// <summary>
        /// adds a card to the hand
        /// </summary>
        /// <param name="card">the card to add to the hand</param>
        public void Add(Card card)
        {
            cards.Add(card);
        }

        /// <summary>
        /// removes a card from the hand
        /// </summary>
        /// <param name="card">the card to remove</param>
        public void Remove(Card card)
        {
            cards.Remove(card);
        }

        /// <summary>
        /// removes a card at random from the hand
        /// </summary>
        /// <returns>the card randomly chosen</returns>
        public Card RemoveRandom()
        {
            if (rng == null)
            {
                rng = new Random();
            }

            int random = rng.Next(cards.Count);

            Card randomCard = cards[random];
            cards.RemoveAt(random);
            return randomCard;
        }
    }
}
