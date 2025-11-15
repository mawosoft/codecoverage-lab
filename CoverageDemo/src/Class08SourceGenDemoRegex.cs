// Copyright (c) Matthias Wolf, Mawosoft.

using System.Text.RegularExpressions;

namespace CoverageDemo;

#pragma warning disable CA1822 // Mark members as static
public partial class Class08SourceGenDemoRegex
{
    public bool Method1(string? value)
    {
        if (value is null) return false;
        return RegexBar().IsMatch(value);
    }

#if DISABLE_SOURCEGEN_REGEX
    private static Regex RegexBar() => s_regexBar;
#pragma warning disable SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
    private static readonly Regex s_regexBar = new(@"bar", RegexOptions.None);
#pragma warning restore SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
#elif DISABLE_SOURCEGEN_REGEXOPTIONS
    [GeneratedRegex(@"bar")]
    private static partial Regex RegexBar();
#else
    [GeneratedRegex(@"bar", RegexOptions.None)]
    private static partial Regex RegexBar();
#endif
}
