using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PLIbox.ListExtensions
{
    public static class ListExtensions
    {
        private static void AssertList<T>(this IList<T> list)
        {
            if (list == null) throw new ArgumentNullException(nameof(list));
            if (list.Count == 0) throw new InvalidOperationException($"List \"{list}\" is empty");
        }

        public static T GetRandomItem<T>(this IList<T> list)
        {
            AssertList(list);
            return list[Random.Range(0, list.Count)];
        }

        public static T GetFirstItem<T>(this IList<T> list)
        {
            AssertList(list);
            return list[0];
        }

        public static T GetLastItem<T>(this IList<T> list)
        {
            AssertList(list);
            return list[list.Count];
        }
    }
}

