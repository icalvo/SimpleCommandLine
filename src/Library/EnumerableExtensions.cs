namespace Scheduler.CommandLine;

public static class EnumerableExtensions
{
    public static string StringJoin<T>(this IEnumerable<T> source, string separator)
    {
        return string.Join(separator, source);
    }

    public static IEnumerable<T> FollowLinks<T>(this T seed, Func<T, T?> follow)
    {
        T? current = follow(seed);
        while (current != null)
        {
            yield return current;
            current = follow(current);
        } 
    }
}