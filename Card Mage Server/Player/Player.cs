using Card_Mage_Server.Game_Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Card_Mage_Server
{
    /// <summary>
    /// a class which represents a player
    /// </summary>
    public class Player
    {
        private static int BUFFERSIZE = 8192;
        //TODO: ???    private static int testNameNr = 0;

        //meta data
        public Socket socket;
        public byte[] buffer;
        public List<byte> messageBuilder;
        public string name;
        public int nameLength = -1; //TODO: set correctly
        public int id;

        //cards, decks, etc
        public CardCollection collection;
        public List<Deck> decks;
        public int currentlySelectedDeck = 0;

        //flags for checking if the player can perform a certain action like creating or joining lobbies, etc.
        public bool onServer = false;
        public bool inLobby = false;
        public bool inGame = false;

        //pointers to the current lobby/game
        public Lobby lobby;
        public Game game;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="currentSocket">the socket that a player uses for communicating with the server</param>
        public Player(Socket currentSocket)
        {
            this.socket = currentSocket;
            buffer = new byte[BUFFERSIZE];
            messageBuilder = new List<byte>();
        }

        /// <summary>
        /// serializes the id, name length and name
        /// </summary>
        /// <returns></returns>
        public byte[] Serialize()
        {
            List<byte> bytesList = new List<byte>();
            bytesList.AddRange(Endianness.ToBigEndian(BitConverter.GetBytes(id)));
            bytesList.AddRange(Endianness.ToBigEndian(BitConverter.GetBytes(nameLength)));
            bytesList.AddRange(Encoding.UTF8.GetBytes(name));
            return bytesList.ToArray();
        }

        //toString method
        public override string ToString()
        {
            return "player " + name + "(id " + id + ")";
        }
    }
}
