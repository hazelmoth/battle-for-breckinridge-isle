using System;
using System.Collections.Generic;
using System.Linq;

public static class EnumerableExtension
{
    /// Returns a random element from the given sequence.
    /// If the sequence has no elements, throws an exception.
    public static T PickRandom<T>(this IEnumerable<T> source)
    {
        return source.PickRandom(1).Single();
    }

    public static IEnumerable<T> PickRandom<T>(this IEnumerable<T> source, int count)
    {
        return source.Shuffle().Take(count);
    }

    public static T WeightedRandom<T>(this IEnumerable<T> source, Func<T, float> weightSelector)
    {
        IEnumerable<T> enumerable = source as T[] ?? source.ToArray();
        if (!enumerable.Any())
            throw new InvalidOperationException("Sequence contains no elements");

        float totalWeight = enumerable.Sum(weightSelector);
        float random = UnityEngine.Random.Range(0, totalWeight);
        foreach (T item in enumerable)
        {
            random -= weightSelector(item);
            if (random <= 0)
                return item;
        }
        return default;
    }

    public static IEnumerable<T> WeightedRandom<T>(this IEnumerable<T> source, Func<T, float> weightSelector, int count)
    {
        IEnumerable<T> enumerable = source as T[] ?? source.ToArray();
        for (int i = 0; i < count; i++)
        {
            yield return enumerable.WeightedRandom(weightSelector);
        }
    }

    public static T SeededPickRandom<T>(this IEnumerable<T> source, int seed)
    {
        return source.Shuffle().SeededPickRandom(1, seed).Single();
    }
    
    public static IEnumerable<T> SeededPickRandom<T>(this IEnumerable<T> source, int count, int seed)
    {
        return source.Shuffle().Take(count);
    }

    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
    {
        return source.OrderBy(x => Guid.NewGuid());
    }
    
    public static IEnumerable<T> SeededShuffle<T>(this IEnumerable<T> source, int seed)
    {
        Random rand = new Random(seed);
        return source.OrderBy(x => rand.Next());
    }
}
