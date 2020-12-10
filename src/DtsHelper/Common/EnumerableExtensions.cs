// Copyright (c) CEUS. All rights reserved.
// See LICENSE file in the project root for license information.

namespace DtsHelper.Common
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    ///     Extension methods for IEnumerable
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        ///     Applies the action to each element in the list. The Same as an ForEach method.
        /// </summary>
        /// <typeparam name="T">The enumerable item's type.</typeparam>
        /// <param name="enumerable">The elements to enumerate.</param>
        /// <param name="action">The action to apply to each item in the list.</param>
        public static void Apply<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var item in enumerable)
            {
                action(item);
            }
        }

        /// <summary>
        ///     Applies the action to each element in the list. The Same as an ForEach method.
        /// </summary>
        /// <param name="enumerable">The elements to enumerate.</param>
        /// <param name="action">The action to apply to each item in the list.</param>
        public static void Apply(this IEnumerable enumerable, Action<object> action)
        {
            foreach (var item in enumerable)
            {
                action(item);
            }
        }

        /// <summary>
        ///     Applies the action to each element in the list and returns the list.
        /// </summary>
        /// <typeparam name="T">The enumerable item's type.</typeparam>
        /// <param name="enumerable">The elements to enumerate.</param>
        /// <param name="action">The action to apply to each item in the list.</param>
        public static IEnumerable<T> ApplyAndSelect<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var item in enumerable)
            {
                action(item);
            }

            return enumerable;
        }

        /// <summary>
        ///     Distinct elements of enumerable by an property
        /// </summary>
        /// <typeparam name="TSource">The enumerable item's type.</typeparam>
        /// <typeparam name="TKey">The key property.</typeparam>
        /// <param name="source">The elements to enumerate.</param>
        /// <param name="keySelector">The selector.</param>
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>
            (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            var seenKeys = new HashSet<TKey>();
            foreach (var element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }
    }
}