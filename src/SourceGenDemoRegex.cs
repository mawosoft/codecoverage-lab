// Copyright (c) Matthias Wolf, Mawosoft.

using System.Text.RegularExpressions;

namespace FooLib;

public partial class SourceGenDemoRegex
{
    public int AutoProp1 { get; set; }

    public int Method1(string? value)
    {
        if (AutoProp1 != 0 || value is null)
        {
            return 0;
        }
        if (RegexBar().IsMatch(value))
        {
            return 1;
        }
        return -1;
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
