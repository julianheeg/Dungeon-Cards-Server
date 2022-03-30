using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Card_Mage_Server.Utilities
{
    /// <summary>
    /// an extension class for lists
    /// </summary>
    static class ListExtension
    {
        /// <summary>
        /// provides a deep copy of a list object
        /// </summary>
        /// <param name="original">the list to copy</param>
        /// <returns>a deep copy of that list</returns>
        public static List<T> Clone<T>(this List<T> original)
        {
            List<T> newList = new List<T>(original.Capacity);
            foreach(T listItem in original)
            {
                newList.Add(listItem);
            }
            return newList;
        }
    }
}
