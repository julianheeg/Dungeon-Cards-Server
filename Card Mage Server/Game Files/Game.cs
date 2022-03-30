using Card_Mage_Server.Game_Files.Cards;
using Card_Mage_Server.Game_Files.MapFolder;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Card_Mage_Server.Game_Files
{
    /// <summary>
    /// a struct for representing commands that players send to the server
    /// </summary>
    public struct PlayerAndCommand
    {
        public byte[] command;
        public Player player;

        public PlayerAndCommand(Player player, byte[] command)
        {
            this.player = player;
            this.command = command;
        }
    }


    /// <summary>
    /// a class that represents a game
    /// </summary>
    public partial class Game
    {
        bool setup = false;
        Random rng;

        public BlockingCollection<PlayerAndCommand> CommandQueue { get; private set; }
        GameDataParser dataParser;

        public readonly int id; //for logging
        public readonly int numberOfPlayers;
        Player[] players;
        CardFieldAndHand[] boards;

        //maps the cards' instanceIDs to the cards themselves
        Dictionary<int, Card> cardDictionary;

        Map map;

        //the currently active player
        int currentPlayer;
        int CurrentPlayer
        {
            set
            {
                currentPlayer = value;
                Messages.SendTurnChange(this, value);
            }
            get
            {
                return currentPlayer;
            }
        }

        bool gameOver = false;
        Result result;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="lobby">the lobby from which the game is started</param>
        public Game(Lobby lobby)
        {
            this.CommandQueue = new BlockingCollection<PlayerAndCommand>();
            this.id = lobby.id;

            //copy players
            this.players = new Player[lobby.currentPlayerCount];
            int currentIndex = 0;
            for (int i = 0; i < lobby.maxPlayers; i++)
            {
                if (lobby.players[i] != null)
                {
                    this.players[currentIndex] = lobby.players[i];
                    currentIndex++;
                }
            }
            numberOfPlayers = currentIndex;

            rng = new Random();
            cardDictionary = new Dictionary<int, Card>(numberOfPlayers * Config.typicalAmountOfCardsPerPlayer);

            dataParser = new GameDataParser(this, CommandQueue, currentIndex);


            Initialize(lobby);
        }

        /// <summary>
        /// generates the map and starts the game
        /// </summary>
        /// <param name="lobby">the lobby from which to start the game</param>
        private void Initialize(Lobby lobby)
        {
            Console.Write("starting game (width=" + lobby.width + ", ");
            Console.Write("height=" + lobby.height + ", ");
            Console.Write("isHexGrid=" + lobby.isHexGrid + ", ");
            Console.Write("mazeGeneratorType=" + lobby.mazeGeneratorType + ", ");
            Console.Write("subMazeGeneratorType=" + lobby.subMazeGeneratorType + ", ");
            Console.Write("playerBaseType=" + lobby.playerBaseType + ")\n");

            //create map
            map = new Map(this, lobby.width, lobby.height, lobby.isHexGrid, lobby.mazeGeneratorType, lobby.subMazeGeneratorType, lobby.playerBaseType);

            //create card boards, place decks and draw
            int[] playerIDs = new int[numberOfPlayers];
            int[] deckSizes = new int[numberOfPlayers];
            boards = new CardFieldAndHand[numberOfPlayers];
            for (int i = 0; i < numberOfPlayers; i++)
            {
                players[i].lobby = null;
                players[i].game = this;
                players[i].inGame = true;
                players[i].inLobby = false;

                int playerDeckIndex = players[i].currentlySelectedDeck;
                Deck playerDeck = players[i].decks[playerDeckIndex];
                boards[i] = new CardFieldAndHand(this, i, playerDeck);

                playerIDs[i] = players[i].id;
                deckSizes[i] = playerDeck.Length;

            }


            Messages.SendMetaAndMap(this, map, playerIDs, deckSizes);
            Messages.SendGameStart(this);

            setup = true;
        }

        /// <summary>
        /// the game loop which keeps the game going until it's over or a player left
        /// </summary>
        public void AdvanceGameState()
        {
            if (setup)
            {
                if (!gameOver)
                {
                    PlayerAndCommand command;
                    while(CommandQueue.TryTake(out command))
                    {
                        dataParser.Parse(command);
                    }
                }
                else
                {
                    ReturnResult();
                }
            }
        }

        /// <summary>
        /// returns a result to the database and asks the main program to remove references to this game
        /// </summary>
        private void ReturnResult()
        {
            //TODO more?
            LoginAndDatabase.StoreResult(this, result);
            Program.QueueRemoveGame(this);
        }

        /// <summary>
        /// draw initial cards and determine the turn player
        /// </summary>
        private void AllLoaded()
        {

            for (int i = 0; i < numberOfPlayers; i++)
            {
                foreach (Card card in boards[i].deckPile.cards)
                {
                    Messages.SendCardInit(this, card);
                }
            }

            for (int i = 0; i < numberOfPlayers; i++)
            {
                boards[i].ShuffleAndDraw();
            }
            CurrentPlayer = rng.Next(numberOfPlayers);
        }

        /// <summary>
        /// removes a player from the list and sets the game over flag and the result
        /// </summary>
        /// <param name="player"></param>
        public void Leave(Player player)
        {
            Console.WriteLine("Game.Leave(): Player {0} has left", player.ToString());

            //find leaving player
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i] == player)
                {
                    players[i] = null;

                    player.game = null;
                    player.inGame = false;
                    player.onServer = true;

                    break;
                }
            }

            //find remaining player (only works for 2 players, TODO: make work for 4 players)
            for (int i = 0; i < players.Length; i++)
            {
                if(players[i] != null)
                {
                    result = new Result(true, players[i]);
                    break;
                }
            }

            gameOver = true;
        }

        /// <summary>
        /// maps a player to their index within the players array 
        /// </summary>
        /// <param name="player">the player whose index is to be determined</param>
        /// <returns>the player's index</returns>
        private int PlayerToIndex(Player player)
        {
            for (int i = 0; i < numberOfPlayers; i++)
            {
                if (players[i] == player)
                {
                    return i;
                }
            }
            throw new ArgumentException("The requested player is not in the array of players");
        }

        /// <summary>
        /// adds a card to the card dictionary so that it can be found more easily
        /// </summary>
        /// <param name="card">the card to add to the dictionary</param>
        public void AddCardToDictionary(Card card)
        {
            cardDictionary.Add(card.instanceID, card);
        }

        /// <summary>
        /// a class which represents the result of a match
        /// </summary>
        public struct Result
        {
            bool playerLeft;
            public Player[] winners;

            public Result(bool playerLeft, params Player[] winners)
            {
                this.playerLeft = playerLeft;
                this.winners = winners;
            }
        }
    }

}
