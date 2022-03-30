using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Card_Mage_Server.Game_Files.Cards.CardTypes
{

    /// <summary>
    /// A class for the cards that the database loads into memory as templates. This is only to be used by the
    ///     Card Database because the card instances used in game belong to the Card class, not this one.
    ///     The correct format for the cards in the database is given in "Card Syntax.txt" in the folder above the project folder
    /// </summary>
    public abstract class CardTemplate
    {
        public readonly int id;
        public readonly int cost;
        public readonly Type type;
        public readonly Effect[] effects;

        const int minTokens = 5;

        /// <summary>
        /// Factory function that parses tokens into a card template
        /// </summary>
        /// <param name="tokens">the card information given as string tokens</param>
        /// <param name="previouslyParsedCardID">the id of the previously parsed card. This is only used to give a more precise error location if an error occurs</param>
        /// <returns></returns>
        public static CardTemplate Instantiate(string[] tokens, int previouslyParsedCardID)
        {
            CheckMinTokens(tokens, previouslyParsedCardID);
            int currentTokenIndex = 0;

            //parse card id (parsed first for error handling) and card type (for switch to the constructor)
            int id = ParseCardID(tokens, ref currentTokenIndex, previouslyParsedCardID);
            Type type = ParseCardType(tokens, ref currentTokenIndex, id);

            //instantiate template
            CardTemplate template;
            switch (type)
            {
                case Type.Monster:
                    template = new MonsterCardTemplate(id, tokens, ref currentTokenIndex);
                    break;
                default:
                    throw new NotImplementedException();
            }

            //check parse and return
            CheckAllTokensUsed(tokens, currentTokenIndex, template.id);
            return template;
        }

        #region parsing

        /// <summary>
        /// checks whether there are enough tokens to parse at all.
        /// </summary>
        /// <param name="tokens">same as above</param>
        /// <param name="previouslyParsedCardID">same as above</param>
        private static void CheckMinTokens(string[] tokens, int previouslyParsedCardID)
        {
            if (tokens.Length < minTokens)
            {
                throw new FormatException("CardTemplate.ctor(): The card following the card with id " + previouslyParsedCardID + " has too few tokens (expected: at least " + minTokens + ")");
            }
        }

        /// <summary>
        /// parses the card ID
        /// </summary>
        /// <param name="tokens">same as above</param>
        /// <param name="currentTokenIndex">the index of the token array that is of relevance here. This is a parameter because it could change with time.</param>
        /// <param name="previouslyParsedCardID">same as above</param>
        /// <returns></returns>
        private static int ParseCardID(string[] tokens, ref int currentTokenIndex, int previouslyParsedCardID)
        {
            int id;
            if (!Int32.TryParse(tokens[currentTokenIndex], out id))
            {
                throw new FormatException("CardTemplate.ctor(): In the line after card id " + previouslyParsedCardID + ": The string \"" + tokens[currentTokenIndex] + "\" is not an int (expected: valid card id)");
            }
            currentTokenIndex++;
            return id;
        }

        /// <summary>
        /// parses the card type
        /// </summary>
        /// <param name="tokens">same as above</param>
        /// <param name="currentTokenIndex">the index of the token array that is of relevance here. This is a parameter because it could change with time.</param>
        /// <param name="id">same as above</param>
        /// <returns></returns>
        private static Type ParseCardType(string[] tokens, ref int currentTokenIndex, int id)
        {
            int type;
            if (!Int32.TryParse(tokens[currentTokenIndex], out type) || !Enum.IsDefined(typeof(Type), type))
            {
                throw new FormatException("CardTemplate.ctor(): The card with id " + id + " has \"" + tokens[currentTokenIndex] + "\" as type (expected: int between 0 and " + (Enum.GetNames(typeof(Type)).Length - 1) + ")");
            }
            currentTokenIndex++;
            return (Type)type;
        }

        /// <summary>
        /// checks whether all tokens have been parsed or whether there are still unparsed tokens. This is just for reminding me to make changes in this class if I change the card syntax.
        /// A simple check for minTokens doesn't suffice because in that case I probably would have forgotten to change that number as well.
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="currentTokenIndex"></param>
        /// <param name="id"></param>
        private static void CheckAllTokensUsed(string[] tokens, int currentTokenIndex, int id)
        {
            //check if all tokens have been parsed
            if (currentTokenIndex != tokens.Length)
            {
                throw new FormatException("CardTemplate.CheckCorrectParse(): The card with id " + id + " has too many tokens (expected: " + currentTokenIndex + ")");
            }
        }

        /// <summary>
        /// constructor of the abstract class. Parses card title and description (which all cards have) and writes them and the card ID into its fields
        /// </summary>
        /// <param name="id">the id of this card. As mentioned before this is parsed beforehand for better error localization</param>
        /// <param name="tokens">the tokens</param>
        /// <param name="currentTokenIndex">the index of the token array that is of relevance here. This is a parameter because it could change with time.</param>
        protected CardTemplate(int id, string[] tokens, ref int currentTokenIndex)
        {
            this.id = id;

            #region parse title, description, cost

            //parse title
            string title = tokens[currentTokenIndex];
            if (title == "")
            {
                throw new FormatException("CardTemplate.ctor(): The card with id " + id + " has no name");
            }
            currentTokenIndex++;

            //parse description
            string description = tokens[currentTokenIndex];
            if (description == "")
            {
                throw new FormatException("CardTemplate.ctor(): The card with id " + id + " has no description");
            }
            currentTokenIndex++;

            //parse cost
            if (!Int32.TryParse(tokens[currentTokenIndex], out cost) || cost < 0)
            {
                throw new FormatException("CardTemplate.ctor(): The card with id " + id + " has \"" + tokens[currentTokenIndex] + "\" as cost (expected: valid cost)");
            }
            currentTokenIndex++;

            #endregion
        }

        #endregion

        //to string method
        public override string ToString() { return "[id: " + id + ", cost: " + cost + ", type: " + type + GetAdditionalToStringAttributes() + "]"; }

        //child classes might want to add more to the toString method
        protected virtual string GetAdditionalToStringAttributes() { return ""; }
    }

    /// <summary>
    /// a class derived from CardTemplate. It is a template class for monster cards which have health, damage and movement range additionally
    /// </summary>
    public class MonsterCardTemplate : CardTemplate
    {
        public readonly int health;
        public readonly int damage;
        public readonly int movementRange;

        /// <summary>
        /// constructor. It calls the base constructor and then also parses health, damage and movement range
        /// </summary>
        /// <param name="id"></param>
        /// <param name="tokens"></param>
        /// <param name="tokenCounter"></param>
        public MonsterCardTemplate(int id, string[] tokens, ref int tokenCounter) : base(id, tokens, ref tokenCounter)
        {
            #region parse health, damage, movement range

            //parse health
            if (!Int32.TryParse(tokens[tokenCounter], out health) || health < 0)
            {
                throw new FormatException("MonsterCardTemplate.ctor(): The card with id " + id + " has \"" + tokens[tokenCounter] + "\" as health (expected: valid health value)");
            }
            tokenCounter++;

            //parse damage
            if (!Int32.TryParse(tokens[tokenCounter], out damage) || damage < 0)
            {
                throw new FormatException("MonsterCardTemplate.ctor(): The card with id " + id + " has \"" + tokens[tokenCounter] + "\" as damage (expected: valid damage value)");
            }
            tokenCounter++;

            //parse movement range
            if (!Int32.TryParse(tokens[tokenCounter], out movementRange) || movementRange < 0)
            {
                throw new FormatException("MonsterCardTemplate.ctor(): The card with id " + id + " has \"" + tokens[tokenCounter] + "\" as movement range (expected: valid damage value)");
            }
            tokenCounter++;

            #endregion
        }

        //adds health, damage, movement range to the toString method of the base class
        protected override string GetAdditionalToStringAttributes() { return ", health: " + health + ", damage: " + damage + ", movementRange: " + movementRange; }
    }
}
