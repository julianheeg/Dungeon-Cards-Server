using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Card_Mage_Server.Utilities
{
    /// <summary>
    /// an extension class for arrays
    /// </summary>
    public static class ArrayExtension
    {
        /// <summary>
        /// combines the contents of an array into a single string
        /// </summary>
        /// <typeparam name="T">the Type parameter of the array</typeparam>
        /// <param name="array">the array</param>
        /// <returns>a ", "-separated string of array contents, enclosed by curly brackets</returns>
        public static string ToString<T>(T[] array)
        {
            StringBuilder stringBuilder = new StringBuilder("{");
            if (array.Length > 0)
            {
                stringBuilder.Append(array[0]);
            }
            for (int i = 1; i < array.Length; i++)
            {
                stringBuilder.Append(", " + array[i]);
            }
            stringBuilder.Append("}");

            return stringBuilder.ToString();
        }
    }
}
