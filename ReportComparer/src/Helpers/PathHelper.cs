// Copyright (c) Matthias Wolf, Mawosoft.

using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;

namespace ReportComparer.Helpers;

internal static class PathHelper
{
    private static readonly SearchValues<char> s_invalidFileNameChars = SearchValues.Create(Path.GetInvalidFileNameChars());

    public static string SanitizeFileName(string fileName)
    {
        int pos = fileName.IndexOfAny(s_invalidFileNameChars);
        if (pos < 0) return fileName;
        char[] chars = fileName.ToCharArray();
        Span<char> span = chars;
        do
        {
            span[pos] = '_';
            span = span[(pos + 1)..];
        } while ((pos = span.IndexOfAny(s_invalidFileNameChars)) >= 0);
        return new string(chars);
    }

    public static int GetCommonPathLength(IEnumerable<string> paths, bool ignoreCase = true)
    {
        using var enumerator = paths.GetEnumerator();
        if (!enumerator.MoveNext()) return 0;
        string first = enumerator.Current;
        if (!enumerator.MoveNext()) return 0;
        int commonPathLength = first.Length;
        do
        {
            int n = GetCommonPathLength(first, enumerator.Current, ignoreCase);
            commonPathLength = Math.Min(n, commonPathLength);
        } while (commonPathLength != 0 && enumerator.MoveNext());
        return commonPathLength;
    }

    // See https://github.com/dotnet/runtime/blob/main/src/libraries/Common/src/System/IO/PathInternal.cs
    public static int GetCommonPathLength(string first, string second, bool ignoreCase = true)
    {
        int commonChars = EqualStartingCharacterCount(first, second, ignoreCase: ignoreCase);
        if (commonChars == 0) return commonChars;
        // Or we're a full string and equal length or match to a separator
        if (commonChars == first.Length
            && (commonChars == second.Length || IsDirectorySeparator(second[commonChars])))
            return commonChars;
        if (commonChars == second.Length && IsDirectorySeparator(first[commonChars]))
            return commonChars;
        // It's possible we matched somewhere in the middle of a segment e.g. C:\Foodie and C:\Foobar.
        while (commonChars > 0 && !IsDirectorySeparator(first[commonChars - 1]))
            commonChars--;
        return commonChars;
    }

    private static int EqualStartingCharacterCount(string? first, string? second, bool ignoreCase)
    {
        if (string.IsNullOrEmpty(first) || string.IsNullOrEmpty(second)) return 0;
        int commonLength = first.AsSpan().CommonPrefixLength(second);
        if (ignoreCase)
        {
            for (; (uint)commonLength < (uint)first.Length; commonLength++)
            {
                if (commonLength >= second.Length ||
                    char.ToUpperInvariant(first[commonLength]) != char.ToUpperInvariant(second[commonLength]))
                {
                    break;
                }
            }
        }
        return commonLength;
    }

    private static bool IsDirectorySeparator(char c)
        => c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar;
}
