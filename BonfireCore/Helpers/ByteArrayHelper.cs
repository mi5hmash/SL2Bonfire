namespace BonfireCore.Helpers;

public static class ByteArrayHelper
{
    public static List<int> FindOffset(this byte[] source, byte[] pattern)
    {
        List<int> matches = new();
        var range = source.Length - pattern.Length + 1;
        for (var i = 0; i < range; i++)
        {
            if (source[i] != pattern[0]) continue;
            var k = true;
            for (var j = 1; j < pattern.Length; j++)
            {
                if (pattern[j] == source[i + j]) continue;
                k = false;
                break;
            }
            if (k) matches.Add(i);
        }
        return matches;
    }

    public static Task<List<int>> FindOffsetAsync(this byte[] source, byte[] pattern) => Task.Run(() => FindOffset(source, pattern));
}