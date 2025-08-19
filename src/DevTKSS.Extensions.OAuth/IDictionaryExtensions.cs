namespace DevTKSS.MyManufacturerERP.Extensions;

internal static class IDictionaryExtensions
{
    /// <summary>
    /// Adds or replaces the element to the <see cref="IDictionary"/> instance.
    /// </summary>
    /// <typeparam name="TKey"> The type of the key</typeparam>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <param name="dictionary">The dictionary to add to.</param>
    /// <param name="key">The key parameter.</param>
    /// <param name="value">The value to add.</param>
    /// <returns>The previous value or its default, possibily <see langword="null"/> value.</returns>
    public static TValue? AddOrReplace<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
    {
        if (!dictionary.TryAdd(key, value))
        {
            var oldValue = dictionary[key];
            dictionary[key] = value;
            return oldValue;
        }
        return default(TValue);
    }
    public static bool TryRemoveKeys<TKey, TValue>(this IDictionary<TKey, TValue>? dictionary, IEnumerable<TKey> keys)
    {
        if (dictionary == null || keys == null || !keys.Any())
        {
            return false;
        }
        bool removed = false;
        foreach (var key in keys)
        {
            if (dictionary.Remove(key))
            {
                removed = true;
            }
        }
        return removed;
    }
    public static bool TryRemove<TKey,TValue>(this IDictionary<TKey,TValue>? dictionary, TKey key)
    {
        if (dictionary == null || key == null)
        {
            return false;
        }
        if (dictionary.ContainsKey(key))
        {
            dictionary.Remove(key);
            return true;
        }
        return false;
    }
    public static bool TryRemove<TKey, TValue>(this IDictionary<TKey, TValue>? dictionary, TKey key, out TValue? value)
    {
        if (dictionary == null || key == null)
        {
            value = default;
            return false;
        }
        if (dictionary.TryGetValue(key, out value))
        {
            dictionary.Remove(key);
            return true;
        }
        value = default;
        return false;
    }
}

