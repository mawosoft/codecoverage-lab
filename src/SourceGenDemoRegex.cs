// Copyright (c) Matthias Wolf, Mawosoft.

using System.Text.RegularExpressions;

namespace FooLib;

#pragma warning disable CA1822 // Mark members as static
public partial class SourceGenDemoRegex
{
    public bool Method1(string? value)
    {
        if (value is null) return false;
        return RegexBar().IsMatch(value);
    }

#if DISABLE_SOURCEGEN_REGEX
    private static Regex RegexBar() => s_regexBar;
#pragma warning disable SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
    private static readonly Regex s_regexBar = new(@"bar");
#pragma warning restore SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
#else
    [GeneratedRegex(@"bar")]
    private static partial Regex RegexBar();
#endif
}
