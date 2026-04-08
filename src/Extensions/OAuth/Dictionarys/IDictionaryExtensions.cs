namespace DevTKSS.Extensions.OAuth.Dictionarys;

public static class IDictionaryExtensions
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
    public static TValue? AddOrReplace<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value) where TKey : notnull
    {
        if (!dictionary.TryAdd(key, value))
        {
            var oldValue = dictionary[key];
            dictionary[key] = value;
            return oldValue;
        }
        return default;
    }

    public static IDictionary<TKey,TValue> AddOrReplace<TKey, TValue>(this IDictionary<TKey, TValue> target, IDictionary<TKey, TValue>? source) where TKey : notnull
    {
        var changes = new Dictionary<TKey, TValue>();
        if (target is null || source is not { Count: > 0 })
        {
            return changes;
        }

        foreach (var (key, value) in source)
        {
            if (target.AddOrReplace(key, value) is TValue oldValue)
            {
                changes.AddOrReplace(key, oldValue);
            }
        }
        return changes;
    }
    public static bool TryRemoveKeys<TKey, TValue>(this IDictionary<TKey, TValue>? dictionary, IEnumerable<TKey> keys)
    {
        if (dictionary is null || keys is null || !keys.Any())
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
        if (dictionary is null || key is null || !dictionary.ContainsKey(key))
        {
            return false;
        }

        dictionary.Remove(key);
        return true;
    }
    public static bool TryRemove<TKey, TValue>(this IDictionary<TKey, TValue>? dictionary, TKey key, out TValue? value)
    {
        if (dictionary is null || key is null)
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

