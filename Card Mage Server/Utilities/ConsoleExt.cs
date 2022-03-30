using Card_Mage_Server.Game_Files;
using System;
using System.Collections;

namespace Card_Mage_Server
{
    /// <summary>
    /// a console extension class
    /// </summary>
    static class ConsoleExt
    {
        /// <summary>
        /// prints an enumerable to the console
        /// </summary>
        /// <typeparam name="T">the type parameter of the enumerable</typeparam>
        /// <param name="data">the enumerable</param>
        public static void LogArray<T>(IEnumerable data)
        {
            foreach(T item in data)
            {
                Console.Write(item.ToString() + ' ');
            }
            Console.Write('\n');
        }

        /// <summary>
        /// prints a PlayerAndCommand object to the console
        /// </summary>
        /// <param name="pac">the PlayerAndCommand object</param>
        public static void Log(PlayerAndCommand pac)
        {
            Player player = pac.player;
            byte[] data = pac.command;

            Console.Write("Player {0} (id: {1}) sends: ", player.name, player.id);
            LogArray<byte>(data);
        }

        /// <summary>
        /// writes a string in a given color to the console
        /// </summary>
        /// <param name="message">the string</param>
        /// <param name="color">the color</param>
        public static void WriteLine(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}
