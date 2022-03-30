using Card_Mage_Server.Game_Files.Cards.CardTypes;
using System;
using System.Collections.Generic;
using System.IO;

namespace Card_Mage_Server.Game_Files.Cards
{
    /// <summary>
    /// a class that handles the extraction of information from the card database. This includes loading card templates into memory and querying card templates from a template id 
    /// </summary>
    public static class CardDatabase
    {
        static Dictionary<int, CardTemplate> cardDicationary; //dictionary for querying
        static readonly string filename = "Card Database.txt"; //the card database file

        /// <summary>
        /// Loads all the card templates into the dictionary.
        /// Prints out any errors that occur during this phase.
        /// </summary>
        public static void Init()
        {
            Console.WriteLine("CardDatabase.Init(): Initializing card base");

            cardDicationary = new Dictionary<int, CardTemplate>();
            int previouslyParsedCardID = -1; //ID of the previously parsed card. Only used for localizing errors through better error messages

            try
            {
                using (StreamReader streamReader = new StreamReader(filename))
                {
                    while (!streamReader.EndOfStream)
                    {
                        String line = streamReader.ReadLine();
                        String[] tokens = line.Split(';');

                        CardTemplate template = CardTemplate.Instantiate(tokens, previouslyParsedCardID);
                        cardDicationary.Add(template.id, template);
                        Console.WriteLine("CardDatabase.Init(): Added " + template.ToString() + " to the list");
                        previouslyParsedCardID = template.id;
                    }
                    Console.WriteLine("closing stream reader");
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("CardDatabase.LoadCards(): The card database file could not be read:\n" + e.Message);
            }
            catch (OutOfMemoryException e)
            {
                Console.WriteLine("CardDatabase.LoadCards(): The card database file could not be read because of insufficient memory:\n" + e.Message);
            }
            catch (FormatException e)
            {
                Console.WriteLine("CardDatabase.LoadCards(): The card database file is ill-formatted:\n" + e.Message);
            }
            catch(ArgumentException e)
            {
                Console.WriteLine("CardDatabase.LoadCards(): The card database file contains two cards with the same card ID\n" + e.Message);
            }
        }

        /// <summary>
        /// queries a card template from its cardID
        /// </summary>
        /// <param name="id">the cardID</param>
        /// <returns></returns>
        public static CardTemplate IDToTemplate(int id)
        {
            if (cardDicationary.TryGetValue(id, out CardTemplate template))
            {
                return template;
            }
            else
            {
                Console.WriteLine(id);
                throw new ArgumentNullException("CardDatabase.GetCardData(id): There is no card with id " + id);
            }
        }
    }
}
