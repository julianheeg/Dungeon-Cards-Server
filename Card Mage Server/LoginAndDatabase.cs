using Card_Mage_Server.Game_Files;
using System;
using System.Collections.Generic;
using System.Text;

namespace Card_Mage_Server
{
    /// <summary>
    /// a class which handles player logins and retrieves their information
    /// </summary>
    static class LoginAndDatabase
    {
        //TODO: remove. This is only for testing
        private static int testPlayerNr = 1;

        /// <summary>
        /// splits the message from the client into username and password
        /// </summary>
        /// <param name="data">the networking message of the player</param>
        /// <param name="player">the player object</param>
        /// <returns></returns>
        public static bool parseLoginData(byte[] data, Player player)
        {
            //----username----
            
            if (data.Length >= 6 + Config.MinimumUsernameLength) //header + int32 == 6
            {
                int usernameLength = BitConverter.ToInt32(Endianness.FromBigEndian(data, 2), 2);
                if (data.Length >= 10 + usernameLength)
                {
                    String username = Encoding.UTF8.GetString(data, 6, usernameLength);

                    //----password----

                    int passwordLength = BitConverter.ToInt32(Endianness.FromBigEndian(data, 6 + usernameLength), 6 + usernameLength);
                    if (data.Length == 10 + usernameLength + passwordLength)
                    {
                        String password = Encoding.UTF8.GetString(data, 10 + usernameLength, passwordLength);

                        CheckLoginData(username, password, player);
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// sends a call to the database and checks whether the login matches
        /// then calls from the database all required player fields and updates the player
        /// </summary>
        /// <param name="username">the username</param>
        /// <param name="password">the password</param>
        /// <param name="player">the player object</param>
        private static void CheckLoginData(string username, string password, Player player)
        {

            //TODO: check database for player login and set player fields accordingly


            Console.WriteLine("user {0} tries to connect with password {1} ", username, password);

            //TODO: Remove the following
            if (username.StartsWith("test") && username.Length >= 6)
            {
                Console.WriteLine("LoginAndDatabase.checkLoginData(...): giving test login to " + username);
                player.name = username;
                player.nameLength = username.Length;
                player.id = testPlayerNr;
                player.decks = new List<Deck>(1);
                player.decks.Add(Deck.Default());

                player.onServer = true;
                testPlayerNr++;
                Messages.SendLoginAccept(player);
            }
            else
            {
                Messages.SendLoginReject(player);
            }
        }

        /// <summary>
        /// stores a game's result in the data base
        /// </summary>
        /// <param name="game">the game object</param>
        /// <param name="result">the result</param>
        internal static void StoreResult(Game game, Game.Result result)
        {
            //TODO store result in database
        }
    }
}
