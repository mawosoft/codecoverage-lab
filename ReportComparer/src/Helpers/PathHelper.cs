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
    // Note that the original EqualStartingCharacterCount() method stops at mismatching directory separator
    // chars, e.g. aa/bb/foo and aa\bb\foo return 2 instead of 6.
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

    // Compare paths and sort shallower paths first.
    public static int ComparePath(string? left, string? right, bool ignoreCase = true)
    {
        if (ReferenceEquals(left, right)) return 0;
        if (left is null) return -1;
        if (right is null) return 1;
        int commonChars = EqualStartingCharacterCount(left, right, ignoreCase: ignoreCase);
        int leftPos = commonChars;
        while (leftPos < left.Length && !IsDirectorySeparator(left[leftPos])) leftPos++;
        int rightPos = commonChars;
        while (rightPos < right.Length && !IsDirectorySeparator(right[rightPos])) rightPos++;
        StringComparison comparisonType = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        if (leftPos < left.Length)
        {
            if (rightPos < right.Length)
            {
                int r = string.Compare(left, commonChars, right, commonChars, Math.Min(leftPos, rightPos) - commonChars, comparisonType);
                if (r != 0) return r;
                r = leftPos.CompareTo(rightPos);
                if (r != 0) return r;
                commonChars += leftPos + 1;
            }
            else
            {
                return 1;
            }
        }
        else if (rightPos < right.Length)
        {
            return -1;
        }
        int r2 = string.Compare(left, commonChars, right, commonChars, Math.Min(left.Length, right.Length) - commonChars, comparisonType);
        if (r2 != 0) return r2;
        return left.Length.CompareTo(right.Length);
    }

    private static int EqualStartingCharacterCount(string? first, string? second, bool ignoreCase)
    {
        if (string.IsNullOrEmpty(first) || string.IsNullOrEmpty(second)) return 0;
        int commonLength = first.AsSpan().CommonPrefixLength(second);
        int maxLength = Math.Min(first.Length, second.Length);
        if (ignoreCase)
        {
            while (commonLength < maxLength
                   && (char.ToUpperInvariant(first[commonLength]) == char.ToUpperInvariant(second[commonLength])
                       || (IsDirectorySeparator(first[commonLength]) && IsDirectorySeparator(second[commonLength]))))
            {
                commonLength++;
            }
        }
        else
        {
            while (commonLength < maxLength
                   && (first[commonLength] == second[commonLength]
                       || (IsDirectorySeparator(first[commonLength]) && IsDirectorySeparator(second[commonLength]))))
            {
                commonLength++;
            }
        }
        return commonLength;
    }

    private static bool IsDirectorySeparator(char c)
        => c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar;
}
