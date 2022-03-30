using Card_Mage_Server.Game_Files;
using Card_Mage_Server.Game_Files.Cards;
using Card_Mage_Server.Game_Files.Cards.CardTypes;
using Card_Mage_Server.Game_Files.MapFolder;
using Card_Mage_Server.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Card_Mage_Server
{
    //enums used for communication as defined in the network protocol file
    enum ClientTopLevel { Main = 0, Game, GameState, Ping }
    enum ClientMenuLevel { List = 0, LobbyFull, LobbyIDNotFound, LobbyJoin, LobbyLeave, LoginAccept, LoginReject, LobbyOtherJoin, PlayerReady }
    enum LoginReject { WrongLogin = 0}
    enum ClientGameLevel { GameMeta = 0, MapRow, GameStart, CardInit, CardFaceInit }
    enum ClientGameStateChange { TurnChange = 0, CardMovement, MonsterSpawn }

    /// <summary>
    /// a helper class which converts integers to and from big endian into the format that the running computer uses
    /// </summary>
    static class Endianness
    {
        /// <summary>
        /// turns a sequence of bytes that define an integer into big endian format
        /// </summary>
        /// <param name="data">the bytes</param>
        /// <returns>a big endian representation of the input</returns>
        public static byte[] ToBigEndian(byte[] data)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(data);
            }
            return data;
        }

        /// <summary>
        /// turns a sequence of bytes that define an integer into the running computer's endianness at a specific index
        /// </summary>
        /// <param name="data">the bytes, usually given as an entire networking message</param>
        /// <param name="index">the start index of the byte sequence</param>
        /// <returns>the byte sequence where the integer is in the running computer's endianness</returns>
        public static byte[] FromBigEndian(byte[] data, int index)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(data, index, 4);
            }
            return data;
        }
    }

    /// <summary>
    /// all the possible network messages that need to be sent
    /// </summary>
    static class Messages
    {

        #region Server

        /// <summary>
        /// pings the player in order to check if they are still connected
        /// </summary>
        /// <param name="player"></param>
        internal static void Ping(Player player)
        {
            List<byte> ping = new List<byte>(1);
            ping.Add((byte)ClientTopLevel.Ping);
            Program.send(player, ping);
        }

        /// <summary>
        /// sends a message to a client that the lobby they wanted to join has not been found (e.g. has already started the game, has been deleted, ...)
        /// </summary>
        /// <param name="player">the player who wanted to join</param>
        internal static void SendLobbyNotFoundMessage(Player player)
        {
            List<byte> lobbyIDNotFoundMessage = new List<byte>(2);
            lobbyIDNotFoundMessage.Add((byte)ClientTopLevel.Main);
            lobbyIDNotFoundMessage.Add((byte)ClientMenuLevel.LobbyIDNotFound);
            Program.send(player, lobbyIDNotFoundMessage);
        }

        /// <summary>
        /// sends a list with all the open lobbies to the player
        /// </summary>
        /// <param name="player">the player who requested the list</param>
        /// <param name="lobbies">the lobby list (needs to be locked during execution of this method)</param>
        internal static void SendLobbyList(Player player, List<Lobby> lobbies)
        {
            List<byte> lobbyList = new List<byte>();
            lobbyList.Add((byte)ClientTopLevel.Main);
            lobbyList.Add((byte)ClientMenuLevel.List);
            for (int i = 0; i < lobbies.Count; i++)
            {
                lobbyList.AddRange(lobbies[i].SerializeForList());
            }
            Program.send(player, lobbyList);
        }

        #endregion



        #region Lobby

        /// <summary>
        /// sends a message to this particular player that his join attempt was successful
        /// and sends the serialized lobby as well
        /// </summary>
        /// <param name="lobby"></param>
        /// <param name="player"></param>
        internal static void SendLobbyJoinMessage(Lobby lobby, Player player)
        {
            //notify player who joined
            List<byte> lobbyJoinMessage = new List<byte>();
            lobbyJoinMessage.Add((byte)ClientTopLevel.Main);
            lobbyJoinMessage.Add((byte)ClientMenuLevel.LobbyJoin);
            lobbyJoinMessage.AddRange(lobby.SerializeAll());
            Program.send(player, lobbyJoinMessage);
        }

        /// <summary>
        /// sends a message to all players in the lobby that another player is about to join
        /// </summary>
        /// <param name="lobby">the lobby that initiated the call</param>
        /// <param name="player">the joining player</param>
        /// <param name="position">the position at which the player is seated</param>
        internal static void SendLobbyOtherJoinMessage(Lobby lobby, Player player, int position)
        {
            //notify other players that somebody is joining
            List<byte> lobbyOtherJoinMessage = new List<byte>();
            lobbyOtherJoinMessage.Add((byte)ClientTopLevel.Main);
            lobbyOtherJoinMessage.Add((byte)ClientMenuLevel.LobbyOtherJoin);
            lobbyOtherJoinMessage.AddRange(Endianness.ToBigEndian(BitConverter.GetBytes(position)));
            lobbyOtherJoinMessage.AddRange(player.Serialize());
            lobby.SendToAll(lobbyOtherJoinMessage);
        }

        /// <summary>
        /// sends a message to all players in the lobby that a player has left, including the player itself
        /// </summary>
        /// <param name="lobby">the lobby that initiated the call</param>
        /// <param name="playerPosition">the position at which the player was seated</param>
        internal static void SendLobbyLeaveMessage(Lobby lobby, int playerPosition)
        {
            //create message for clients in the lobby
            List<byte> lobbyLeaveMessage = new List<byte>(6);
            lobbyLeaveMessage.Add((byte)ClientTopLevel.Main);
            lobbyLeaveMessage.Add((byte)ClientMenuLevel.LobbyLeave);
            lobbyLeaveMessage.AddRange(Endianness.ToBigEndian(BitConverter.GetBytes(playerPosition)));

            //send message to all clients in the lobby
            lobby.SendToAll(lobbyLeaveMessage);
        }

        /// <summary>
        /// sends a message to all players that the player at the specified position has changed their ready checkbox
        /// </summary>
        /// <param name="lobby">the lobby that initiated the call</param>
        /// <param name="position">the position at which the player is seated</param>
        /// <param name="ready">the new ready value</param>
        internal static void SendLobbyReadyMessage(Lobby lobby, int position, bool ready)
        {
            //create message for all clients in the lobby
            List<byte> readyMessage = new List<byte>(7);
            readyMessage.Add((byte)ClientTopLevel.Main);
            readyMessage.Add((byte)ClientMenuLevel.PlayerReady);
            readyMessage.AddRange(Endianness.ToBigEndian(BitConverter.GetBytes(position)));
            readyMessage.AddRange(BitConverter.GetBytes(ready));

            //send message to all
            lobby.SendToAll(readyMessage);
        }

        /// <summary>
        /// sends a message to a player that the lobby they wanted to join is full
        /// </summary>
        /// <param name="player">the player who wanted to join</param>
        internal static void SendLobbyFullMessage(Player player)
        {
            //notify the client that the lobby is full
            List<byte> lobbyFullMessage = new List<byte>(2);
            lobbyFullMessage.Add((byte)ClientTopLevel.Main);
            lobbyFullMessage.Add((byte)ClientMenuLevel.LobbyFull);
            Program.send(player, lobbyFullMessage);
        }

        #endregion



        #region LoginAndDatabase

        /// <summary>
        /// sends a login reject message to the player whose login failed
        /// </summary>
        /// <param name="player">the player whose login failed</param>
        internal static void SendLoginReject(Player player)
        {
            List<byte> loginRejectMessage = new List<byte>(3);
            loginRejectMessage.Add((byte)ClientTopLevel.Main);
            loginRejectMessage.Add((byte)ClientMenuLevel.LoginReject);
            loginRejectMessage.Add((byte)LoginReject.WrongLogin);
            Program.send(player, loginRejectMessage);
        }

        /// <summary>
        /// creates and sends a login accept message to the corresponding player
        /// enables the player on the server
        /// </summary>
        /// <param name="player"></param>
        internal static void SendLoginAccept(Player player)
        {
            //create and send message
            List<byte> loginAcceptMessage = new List<byte>();
            loginAcceptMessage.Add((byte)ClientTopLevel.Main);
            loginAcceptMessage.Add((byte)ClientMenuLevel.LoginAccept);
            loginAcceptMessage.AddRange(player.Serialize());
            Program.send(player, loginAcceptMessage);
        }

        #endregion



        #region Game

        /// <summary>
        /// creates and sends the available map to the players
        /// </summary>
        /// <param name="game"></param>
        /// <param name="map"></param>
        internal static void SendMetaAndMap(Game game, Map map, int[] playerIDs, int[] deckSizes)
        {
            //send metadata
            List<byte> gameMetaMessage = new List<byte>(11);
            gameMetaMessage.Add((byte)ClientTopLevel.Game);
            gameMetaMessage.Add((byte)ClientGameLevel.GameMeta);
            gameMetaMessage.AddRange(map.SerializeMetaData());
            
            for (int i = 0; i < playerIDs.Length; i++)
            {
                gameMetaMessage.AddRange(Endianness.ToBigEndian(BitConverter.GetBytes(playerIDs[i])));
                gameMetaMessage.AddRange(Endianness.ToBigEndian(BitConverter.GetBytes(deckSizes[i])));
            }
            game.SendToAll(gameMetaMessage);

            //send individual rows
            for (int i = 0; i < map.width; i++)
            {
                List<byte> mapRowMessage = new List<byte>(map.length + 2);
                mapRowMessage.Add((byte)ClientTopLevel.Game);
                mapRowMessage.Add((byte)ClientGameLevel.MapRow);

                //add row index
                mapRowMessage.AddRange(Endianness.ToBigEndian(BitConverter.GetBytes(i)));

                mapRowMessage.AddRange(map.SerializeRow(i));
                game.SendToAll(mapRowMessage);
            }
        }

        /// <summary>
        /// starts the game on the clients' sides
        /// </summary>
        /// <param name="game"></param>
        internal static void SendGameStart(Game game)
        {
            List<byte> gameStartMessage = new List<byte>(11);
            gameStartMessage.Add((byte)ClientTopLevel.Game);
            gameStartMessage.Add((byte)ClientGameLevel.GameStart);
            game.SendToAll(gameStartMessage);
        }

        /// <summary>
        /// sends a message that it's now another player's turn
        /// </summary>
        /// <param name="game"></param>
        /// <param name="playerIndex">the player whose turn it is now</param>
        internal static void SendTurnChange(Game game, int playerIndex)
        {
            List<byte> turnChangeMessage = new List<byte>(6);
            turnChangeMessage.Add((byte)ClientTopLevel.GameState);
            turnChangeMessage.Add((byte)ClientGameStateChange.TurnChange);
            turnChangeMessage.AddRange(Endianness.ToBigEndian(BitConverter.GetBytes(playerIndex)));
            game.SendToAll(turnChangeMessage);
        }


        /// <summary>
        /// sends a notification to the clients that a new card has been introduced onto the field.
        /// this is always called at the start of the game for each card in the deck of each player, but can also be called during the game
        /// </summary>
        /// <param name="game"></param>
        /// <param name="Card">the card that has been introduced</param>
        internal static void SendCardInit(Game game, Card card)
        {
            List<byte> cardInitMessage = new List<byte>(11);
            cardInitMessage.Add((byte)ClientTopLevel.Game);
            cardInitMessage.Add((byte)ClientGameLevel.CardInit);
            cardInitMessage.AddRange(Endianness.ToBigEndian(BitConverter.GetBytes(card.instanceID)));
            cardInitMessage.AddRange(Endianness.ToBigEndian(BitConverter.GetBytes(card.owner)));
            cardInitMessage.Add((byte)card.location);
            game.SendToAll(cardInitMessage);
        }

        /// <summary>
        /// makes a card face visible on the specified client's side (adds a card ID to an already known instanceID)
        /// </summary>
        /// <param name="game"></param>
        /// <param name="card">the card whose face is going to be initialized</param>
        /// <param name="playerIndex">the player index this message should be sent to</param>
        private static void sendCardFaceInit(Game game, Card card, int playerIndex)
        {
            List<byte> CardFaceInitMessage = new List<byte>(10);
            CardFaceInitMessage.Add((byte)ClientTopLevel.Game);
            CardFaceInitMessage.Add((byte)ClientGameLevel.CardFaceInit);
            CardFaceInitMessage.AddRange(Endianness.ToBigEndian(BitConverter.GetBytes(card.instanceID)));
            CardFaceInitMessage.AddRange(Endianness.ToBigEndian(BitConverter.GetBytes(card.cardID)));
            game.SendToOne(playerIndex, CardFaceInitMessage);
        }

        /// <summary>
        /// notifies the clients that a player has drawn a card
        /// </summary>
        /// <param name="game"></param>
        /// <param name="playerIndex">the index of the drawing player</param>
        /// <param name="cardID">the drawn card's id</param>
        internal static void SendCardMovement(Game game, Card card, int destinationBoard, Card.Location destinationLocation)
        {
            //check if the client knows this card's face already
            if (!card.known[destinationBoard])
            {
                sendCardFaceInit(game, card, destinationBoard);
                card.known[destinationBoard] = true;
            }

            //send card draw
            List<byte> cardDrawMessage = new List<byte>(11);
            cardDrawMessage.Add((byte)ClientTopLevel.GameState);
            cardDrawMessage.Add((byte)ClientGameStateChange.CardMovement);
            cardDrawMessage.AddRange(Endianness.ToBigEndian(BitConverter.GetBytes(card.instanceID)));
            cardDrawMessage.AddRange(Endianness.ToBigEndian(BitConverter.GetBytes(destinationBoard)));
            cardDrawMessage.Add((byte)destinationLocation);
            game.SendToAll(cardDrawMessage);
        }

        /// <summary>
        /// notifies the clients that a monster has been spawned. distinguishes by visibility of its position whether it sends or don't sends the exact position
        /// </summary>
        /// <param name="game"></param>
        /// <param name="card">the monster card that spawns a monster</param>
        /// <param name="position">the position at which the monster spawns</param>
        internal static void SendMonsterSpawn(Game game, MonsterCard card, GridPosition position, int monsterInstanceID)
        {
            List<byte> visibleMonsterSpawnMessage = new List<byte>(14);
            visibleMonsterSpawnMessage.Add((byte)ClientTopLevel.GameState);
            visibleMonsterSpawnMessage.Add((byte)ClientGameStateChange.MonsterSpawn);
            visibleMonsterSpawnMessage.AddRange(Endianness.ToBigEndian(BitConverter.GetBytes(card.instanceID)));
            visibleMonsterSpawnMessage.AddRange(Endianness.ToBigEndian(BitConverter.GetBytes(monsterInstanceID)));

            //clone original message
            List<byte> invisibleMonsterSpawnMessage = visibleMonsterSpawnMessage.Clone();

            //add positions to both messages
            visibleMonsterSpawnMessage.AddRange(position.Serialize());
            invisibleMonsterSpawnMessage.AddRange(GridPosition.UnSetGridPosition.Serialize());

            game.SendMessageWhichDistinguishesByFogOfWar(position, visibleMonsterSpawnMessage, invisibleMonsterSpawnMessage);
        }


        #endregion
    }
}
