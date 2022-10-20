using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace GeoSharpi.Utils
{
    /// <summary>
    /// Methods to dynamically work with enumerators
    /// </summary>
    public static class ReflectiveEnumerator
    {
        static ReflectiveEnumerator() { }

        /// <summary>
        /// Returns all objects from a certain type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="constructorArgs"></param>
        /// <returns>a list of objects</returns>
        public static IEnumerable<T> GetEnumerableOfType<T>(params object[] constructorArgs) where T : class, IComparable<T>
        {
            List<T> objects = new List<T>();
            foreach (Type type in
                Assembly.GetAssembly(typeof(T)).GetTypes()
                .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T))))
            {
                objects.Add((T)Activator.CreateInstance(type, constructorArgs));
            }
            objects.Sort();
            return objects;
        }

        /// <summary>
        /// Prints the collection as a comma separated string
        /// </summary>
        /// <typeparam name="T">The generic collection type</typeparam>
        /// <param name="list">The collection</param>
        /// <returns>a comma separated string of the collection</returns>
        public static string ToStringList<T>(this ICollection<T> list)
        {
            var sb = new StringBuilder();
            int i = 0;
            foreach (var elem in list)
            {
                sb.Append(elem.ToString());
                if (i++ != list.Count - 1) sb.Append(", ");
            }
            return sb.ToString();
        }
    }
}

