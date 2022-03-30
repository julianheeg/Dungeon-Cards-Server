using System;
using System.Collections.Generic;

namespace Card_Mage_Server.Game_Files.Cards
{
    /// <summary>
    /// a struct that holds deck, card field and graveyard of a player
    /// </summary>
    public class CardFieldAndHand
    {
        //needed for sending network messages
        readonly Game game;
        readonly int playerIndex;

        public readonly CardPile deckPile;
        public readonly CardPile graveyardPile;
        public readonly Hand hand;

        /// <summary>
        /// constructs the card board, places deck and shuffles it
        /// </summary>
        /// <param name="game">the game that is played using this struct</param>
        /// <param name="playerIndex">the index of the player that this instance belongs to</param>
        /// <param name="deck">the player's deck</param>
        public CardFieldAndHand(Game game, int playerIndex, Deck deck)
        {
            this.game = game;
            this.playerIndex = playerIndex;

            List<Card> cardsList = new List<Card>(deck.cardIDs.Length);
            foreach (int id in deck.cardIDs)
            {
                //--------------------------------------------------------------------------------------------------------------------------
                Console.WriteLine("DEBUG: CARD ID: " + id);
                Card card = Card.Instantiate(id, game.numberOfPlayers);
                game.AddCardToDictionary(card);
                cardsList.Add(card);
            }
            deckPile = new CardPile(cardsList, false, playerIndex, Card.Location.Deck);

            graveyardPile = new CardPile(null, true, playerIndex, Card.Location.Graveyard);
            hand = new Hand();
        }

        /// <summary>
        /// shuffles the deck and draws the starting hand
        /// </summary>
        public void ShuffleAndDraw()
        {
            Console.WriteLine("shuffling player {0}'s deck and drawing them {1} cards", playerIndex, Config.AmountOfCardsInHandAtTheBeginning);
            deckPile.Shuffle();
            for(int i=0; i < Config.AmountOfCardsInHandAtTheBeginning; i++)
            {
                Draw();
            }
        }

        /// <summary>
        /// takes the first card from the deck pile and adds it to the hand
        /// TODO: implement deckout
        /// </summary>
        public void Draw()
        {
            Card card = deckPile.TryTakeFromTop();
            hand.Add(card);
            Messages.SendCardMovement(game, card, playerIndex, Card.Location.Hand);
        }
    }
}
