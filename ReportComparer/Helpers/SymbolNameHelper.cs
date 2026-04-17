// Copyright (c) Matthias Wolf, Mawosoft.

using System;
using System.Buffers;
using System.Diagnostics;
using System.Text;

namespace ReportComparer.Helpers;

internal static class SymbolNameHelper
{
    /// <summary>
    /// Returns the index of the balanced closing brace or -1.
    /// </summary>
    /// <remarks>
    /// The input span must start with an opening parenthesis, angle or square bracket, or curly braces.
    /// </remarks>
    public static int IndexOfBalancedBraces(ReadOnlySpan<char> span)
    {
        if (span.IsEmpty) return -1;
        char opening = span[0];
        if (opening is not ('(' or '<' or '[' or '{')) throw new ArgumentException(null, nameof(span));
        char closing = opening == '(' ? ')' : (char)(opening + 2);
        int depth = 1;
        int index = 1;
        span = span[1..];
        int pos;
        while ((pos = span.IndexOfAny(opening, closing)) >= 0)
        {
            if (span[pos] == opening)
            {
                depth++;
            }
            else if (--depth == 0)
            {
                return index + pos;
            }
            index += ++pos;
            span = span[pos..];
        }
        return -1;
    }

    /// <summary>
    /// Returns the index of the balanced opening brace or -1.
    /// </summary>
    /// <remarks>
    /// The input span must end with an closing parenthesis, angle or square bracket, or curly braces.
    /// </remarks>
    public static int LastIndexOfBalancedBraces(ReadOnlySpan<char> span)
    {
        if (span.IsEmpty) return -1;
        char closing = span[^1];
        if (closing is not (')' or '>' or ']' or '}')) throw new ArgumentException(null, nameof(span));
        char opening = closing == ')' ? '(' : (char)(closing - 2);
        int depth = 1;
        span = span[..^1];
        int pos;
        while ((pos = span.LastIndexOfAny(opening, closing)) >= 0)
        {
            if (span[pos] == closing)
            {
                depth++;
            }
            else if (--depth == 0)
            {
                return pos;
            }
            span = span[..pos];
        }
        return -1;
    }

    /// <summary>
    /// Same as IndexOf(char), but skips over parts enclosed in balanced angle or square brackets.
    /// </summary>
    public static int SafeIndexOf(ReadOnlySpan<char> span, char value)
    {
        int index = 0;
        int pos;
        while ((pos = span.IndexOfAny(value, '<', '[')) >= 0)
        {
            if (span[pos] == value) return index + pos;
            index += pos;
            span = span[pos..];
            pos = IndexOfBalancedBraces(span);
            if (pos < 0) break;
            index += ++pos;
            span = span[pos..];
        }
        return -1;
    }

    private static readonly SearchValues<char> s_normalizeTypeName = SearchValues.Create('+', '`', '>');

    /// <summary>
    /// Returns a normalized type name eliminating all spelling variants used in various report formats.
    /// </summary>
    /// <remarks>
    /// <para>Nested type separators (+) are replaced by regular separators (.). Generic suffixes of any form
    /// are removed and the total generic parameter count, prefixed with accent (`), is added at the end
    /// of the full type name.</para>
    /// <para>Note that the result can be ambiguous in rare cases, e.g. <c>Outer&lt;T&gt;+Inner&lt;U&gt;</c>
    /// and  <c>Outer&lt;T,U&gt;+Inner</c> both become <c>Outer.Inner`2</c>. But MSCC does this already
    /// anyway, and the original is impossible to recover.</para>
    /// </remarks>
    public static string NormalizeTypeName(string input)
    {
        ReadOnlySpan<char> span = input;
        int pos = span.IndexOfAny(s_normalizeTypeName);
        if (pos < 0) return input;
        StringBuilder sb = new(input.Length);
        int typeParameterCount = 0;
        do
        {
            if (span[pos] == '+')
            {
                sb.Append(span[..pos]).Append('.');
                pos++;
            }
            else if (span[pos] == '`')
            {
                sb.Append(span[..pos]);
                int count = 0;
                while (++pos < span.Length && char.IsAsciiDigit(span[pos]))
                {
                    count = count * 10 + (span[pos] & 0x0F);
                }
                typeParameterCount += count;
                if (pos < span.Length && span[pos] == '<')
                {
                    int end = IndexOfBalancedBraces(span[pos..]);
                    if (end >= 0) pos += end + 1;
                }
                Debug.Assert(pos >= span.Length || span[pos] is '.' or '+');
            }
            else
            {
                pos++;
                int start;
                if ((pos >= span.Length || span[pos] is '.' or '+') && (start = LastIndexOfBalancedBraces(span[..pos])) >= 0)
                {
                    typeParameterCount += CountParameters(span[(start + 1)..(pos - 1)]);
                    sb.Append(span[..start]);
                }
                else
                {
                    sb.Append(span[..pos]);
                }
            }
            span = span[pos..];
            pos = span.IndexOfAny(s_normalizeTypeName);
        } while (pos >= 0);
        sb.Append(span);
        if (typeParameterCount != 0)
        {
            sb.Append('`').Append(typeParameterCount);
        }
        return sb.ToString();
    }

    /// <summary>
    /// Returns the maximum length of the namespace part for a type name, not including the dot (.) separating
    /// the namespace name from the type name.
    /// </summary>
    public static int NamespaceLength(ReadOnlySpan<char> span)
    {
        int length = 0;
        int pos;
        while ((pos = span.IndexOfAny(s_normalizeTypeName)) >= 0)
        {
            if (span[pos] is '+' or '`')
            {
                span = span[..pos];
                break;
            }
            else
            {
                pos++;
                int start;
                if ((pos >= span.Length || span[pos] is '.' or '+') && (start = LastIndexOfBalancedBraces(span[..pos])) >= 0)
                {
                    span = span[..start];
                    break;
                }
                else
                {
                    length += pos;
                    span = span[pos..];
                }
            }
        }
        pos = span.LastIndexOf('.');
        return pos < 0 ? 0 : length + pos;
    }

    private static readonly SearchValues<char> s_countParameters = SearchValues.Create(',', '<', '[');

    /// <summary>
    /// Returns the number of type or actual parameters separated by comma.
    /// </summary>
    /// <remarks>
    /// When counting type parameters, <paramref name="span"/> must NOT include the enclosing angle brackets.
    /// </remarks>
    public static int CountParameters(ReadOnlySpan<char> span)
    {
        if (span.IsWhiteSpace()) return 0;
        int count = 1;
        int pos;
        while ((pos = span.IndexOfAny(s_countParameters)) >= 0)
        {
            if (span[pos] == ',')
            {
                count++;
            }
            else
            {
                span = span[pos..];
                pos = IndexOfBalancedBraces(span);
                if (pos < 0) break;
            }
            span = span[(pos + 1)..];
        }
        return count;
    }

    /// <summary>
    /// Returns a string where each comma is followed by exactly one space.
    /// </summary>
    public static string NormalizeCommaSpacing(string input)
    {
        StringBuilder? sb = null;
        int start = 0;
        int pos = 0;
        while ((pos = input.IndexOf(',', pos)) >= 0)
        {
            int end = ++pos;
            while (end < input.Length && input[end] == ' ') end++;
            if (end != (pos + 1))
            {
                sb ??= new StringBuilder(input.Length + 10);
                sb.Append(input, start, pos - start).Append(' ');
                start = end;
            }
            pos = end;
        }
        if (sb is null) return input;
        sb.Append(input, start, input.Length - start);
        return sb.ToString();
    }

    /// <summary>
    /// Returns the Levenshtein edit distance between to spans.
    /// </summary>
    public static int LevenshteinDistance(ReadOnlySpan<char> left, ReadOnlySpan<char> right)
    {
#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional
        int[,] m = new int[left.Length + 1, right.Length + 1];
#pragma warning restore CA1814 // Prefer jagged arrays over multidimensional
        for (int i = 0; i <= left.Length; i++) m[i, 0] = i;
        for (int i = 0; i <= right.Length; i++) m[0, i] = i;
        for (int i = 1; i <= left.Length; i++)
        {
            for (int k = 1; k <= right.Length; k++)
            {
                int diff = (left[i - 1] == right[k - 1]) ? 0 : 1;
                m[i, k] = Math.Min(
                            Math.Min(
                                m[i - 1, k] + 1,
                                m[i, k - 1] + 1),
                            m[i - 1, k - 1] + diff);
            }
        }
        return m[left.Length, right.Length];
    }
}
