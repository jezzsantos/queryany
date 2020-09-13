using System.Collections.Generic;
using System.Linq;

namespace Domain.Interfaces
{
    public static class CollectionExtensions
    {
        public static Dictionary<TKey, TValue> AsDictionary<TKey, TValue>(
            this IReadOnlyDictionary<TKey, TValue> readOnlyDictionary)
        {
            return readOnlyDictionary
                .ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        public static IReadOnlyDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(
            this IReadOnlyDictionary<TKey, TValue> readOnlyDictionary)
        {
            return readOnlyDictionary
                .ToDictionary(pair => pair.Key, pair => pair.Value);
        }
    }
}