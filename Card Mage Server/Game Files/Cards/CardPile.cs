using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Card_Mage_Server.Game_Files.Cards
{
    /// <summary>
    /// a class that represents a pile of cards, e.g. deck, graveyard
    /// </summary>
    public class CardPile
    {
        readonly int playerIndex;           //the player that this pile belongs to
        readonly Card.Location location;    //the location that this pile represents

        public readonly List<Card> cards; //the cards are indexed using 0 for the bottom-most card and then counting up for the cards above
        bool face_up; //face up or down?

        Random rng; //RNG for shuffling

        
        /// <summary>
        /// amount of cards on that pile
        /// </summary>
        public int Count
        {
            get
            {
                return cards.Count;
            }
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="cards">the initial cards on this pile, can be null</param>
        /// <param name="face_up">face up?</param>
        /// <param name="playerIndex">the player that this pile belongs to</param>
        /// <param name="location">the location that this pile represents</param>
        public CardPile(List<Card> cards, bool face_up, int playerIndex, Card.Location location)
        {
            this.playerIndex = playerIndex;
            this.location = location;

            this.cards = new List<Card>();
            //copy the list of cards, if not null
            if (cards != null)
            {
                foreach(Card card in cards)
                {
                    this.Put(card);
                }
            }

            this.face_up = face_up;
        }

        /// <summary>
        /// puts a card on the pile
        /// </summary>
        /// <param name="cardID">the card to put on this pile</param>
        public void Put(Card card)
        {
            cards.Add(card);
            card.owner = playerIndex;
            card.location = this.location;
        }

        /// <summary>
        /// tries to take a card from the top of the pile
        /// </summary>
        /// <returns>the top card or null</returns>
        public Card TryTakeFromTop()
        {
            if (cards.Count > 0)
            {
                Card card = cards[cards.Count - 1];
                cards.RemoveAt(cards.Count - 1);
                return card;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// takes a look at the first few cards from the top
        /// </summary>
        /// <param name="count">how many cards to peek at</param>
        /// <returns>a list of the cards on top of the pile, up to  >count<  many</returns>
        public List<Card> Peek(int count)
        {
            List<Card> peekedCards = new List<Card>(count);
            for (int i = 1; i <= count; i++)
            {
                if (cards.Count - i < 0)
                {
                    break;
                }
                else
                {
                    peekedCards.Add(cards[cards.Count - i]);
                }
            }
            return peekedCards;
        }

        /// <summary>
        /// shuffles the card pile
        /// </summary>
        public void Shuffle()
        {
            //intitialize rng if null
            if (rng == null)
            {
                rng = new Random();
            }

            //Durstenfeld algorithm
            for (int i = cards.Count - 1; i > 0; i--)
            {
                //determine second index for swapping
                int j = rng.Next(i + 1);

                //swap
                Card temp = cards[i];
                cards[i] = cards[j];
                cards[j] = temp;
            }
        }
    }
}
