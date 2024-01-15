using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Jisu.Utils
{
    public static class CollectionExtension
    {
        #region // List
        public static T GetRandom<T>(this List<T> list)
        {
            return list[UnityEngine.Random.Range(0, list.Count)];
        }

        public static T GetRandom<T>(this List<T> list, Predicate<T> pred)
        {
            return list.Where(new Func<T, bool>(pred)).ToList()[UnityEngine.Random.Range(0, list.Count)];
        }
        #endregion

        #region // Dictionary
        public static T2 GetRandom<T1, T2>(this Dictionary<T1, T2> dict)
        {
            dict.TryGetValue(dict.Keys.ToArray()[UnityEngine.Random.Range(0, dict.Count)], out T2 result);
            return result;
        }

        public static T2 GetRandom<T1, T2>(this Dictionary<T1, T2> dict, Predicate<T1> pred)
        {
            dict.TryGetValue(dict.Keys.ToList().Where(new Func<T1, bool>(pred)).ToArray()[UnityEngine.Random.Range(0, dict.Count)], out T2 result);
            return result;
        }

        public static void TryAddNoDuplicationToDict<T1, T2>(in Dictionary<T1, T2> dict, in T1 key, in T2 value)
        {
            if (dict.ContainsKey(key))
            {
                Debug.LogError("Have tried to add duplication value to dictionary");
                return;
            }

            dict.Add(key, value);
        }
        #endregion
    }
}

