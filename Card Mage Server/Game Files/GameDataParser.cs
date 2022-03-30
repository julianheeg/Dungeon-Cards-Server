using Card_Mage_Server.Game_Files.Cards;
using Card_Mage_Server.Game_Files.MapFolder;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Card_Mage_Server.Game_Files
{
    /// <summary>
    /// the parsing part of the game class
    /// </summary>
    public partial class Game
    {
        /// <summary>
        /// a class which does the parsing
        /// </summary>
        class GameDataParser
        {
            //possible actions on the game level
            private enum GameLevel { LevelLoaded = 0, CardActivation, MonsterMovement };

            Game game;
            readonly BlockingCollection<PlayerAndCommand> commandQueue;
            readonly bool[] playersFinishedLevelLoading;
            bool allReady, gameStarted;

            /// <summary>
            /// constructor
            /// </summary>
            /// <param name="game">the game object that this parser parses messages for</param>
            /// <param name="commandQueue">the game's command queue</param>
            /// <param name="numberOfPlayers">the number of players</param>
            public GameDataParser(Game game, BlockingCollection<PlayerAndCommand> commandQueue, int numberOfPlayers)
            {
                this.game = game;
                this.commandQueue = commandQueue;

                //set level loading to false so that the game does not yet send any messages after the game start message until all players can accept messages
                playersFinishedLevelLoading = new bool[numberOfPlayers];
            }

            /// <summary>
            /// parses a command and calls functions accordingly
            /// </summary>
            /// <param name="playerCommand">the command to parse</param>
            public void Parse(PlayerAndCommand playerCommand)
            {
                Player player = playerCommand.player;   //issuing player
                byte[] command = playerCommand.command; //command

                ConsoleExt.Log(playerCommand);

                //return if command length is too short
                if (command.Length == 1)
                {
                    Console.Error.WriteLine("Parse error on game level: data only contains one byte");
                    return;
                }
                else
                {
                    if (!allReady && command.Length == 2 && command[1] == (byte)GameLevel.LevelLoaded)
                    {
                        //set player to ready
                        int index = game.PlayerToIndex(player);
                        if (!playersFinishedLevelLoading[index])
                        {
                            playersFinishedLevelLoading[index] = true;
                            allReady = true;
                            foreach (bool ready in playersFinishedLevelLoading)
                            {
                                allReady &= ready;
                            }

                            //check if all players have loaded
                            if (allReady && !gameStarted)
                            {
                                game.AllLoaded();
                                gameStarted = true;
                            }
                        }
                    }
                    else if (gameStarted)
                    {
                        switch ((GameLevel)command[1])
                        {
                            case GameLevel.LevelLoaded:
                                Console.WriteLine("Parse error on game level: second byte was LEVEL_LOADED, but game has already started");
                                break;

                            case GameLevel.CardActivation:
                                ParseCardActivationMessage(player, command);
                                break;

                            case GameLevel.MonsterMovement:
                                ParseMonsterMovementMessage(player, command);
                                break;

                            default:
                                Console.WriteLine("Parse error on game level: cast to GameLevel failed. Received " + command[1]);
                                break;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Parse error on game level: invalid command (either players not ready or wrong command format). Received {0} bytes (expected 2) and second byte was {1}", command.Length, (GameLevel)command[1]);
                    }
                }


            }

            /// <summary>
            /// parses a card activation message
            /// </summary>
            /// <param name="player">the player who issued the command</param>
            /// <param name="command">the command</param>
            private void ParseCardActivationMessage(Player player, byte[] command)
            {
                if (command.Length == 14)
                {
                    int cardInstanceID = BitConverter.ToInt32(Endianness.FromBigEndian(command, 2), 2);
                    int index = 6;
                    GridPosition position = new GridPosition(command, ref index);
                    if (game.cardDictionary.TryGetValue(cardInstanceID, out Card card))
                    {
                        game.map.TryCardActivation(card, position, player, game.PlayerToIndex(player));
                    }
                    else
                    {
                        Console.WriteLine("GameDataParser.ParseCardActivationMessage(): player {0} tried to activate the card with instance ID {1}, but the card is not in the dictionary", player.ToString(), cardInstanceID);
                    }
                }
                else
                {
                    Console.WriteLine("GameDataParser.ParseCardActivationMessage(): CardActivationMessage contained {0} bytes (expected 14)", command.Length);
                }
            }

            /// <summary>
            /// parses a monster movement message
            /// </summary>
            /// <param name="player">the player who issued the command</param>
            /// <param name="command">the command</param>
            private void ParseMonsterMovementMessage(Player player, byte[] command)
            {
                if (command.Length >= 18 && command.Length % 8 == 2)
                {
                    int monsterInstanceID = BitConverter.ToInt32(Endianness.FromBigEndian(command, 2), 2);
                    int pathLength = BitConverter.ToInt32(Endianness.FromBigEndian(command, 6), 6);
                    int index = 10;
                    GridPosition[] path = new GridPosition[pathLength];
                    for(int i = 0; i < pathLength; i++)
                    {
                        path[i] = new GridPosition(command, ref index);
                    }
                    game.map.TryMonsterMovement(monsterInstanceID, path, game.PlayerToIndex(player));
                }
                else
                {
                    Console.WriteLine("GameDataParser.ParseMonsterMovementMessage(): MonsterMovementMessage contained {0} bytes (expected 14)", command.Length);
                }
            }

        }
    }
}
