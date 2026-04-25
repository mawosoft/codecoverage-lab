// Copyright (c) Matthias Wolf, Mawosoft.

using System.Text.RegularExpressions;

namespace DemoLib;

public partial class SourceGenRegex
{
    public bool Method1(string value)
    {
        return RegexBar().IsMatch(value);
    }

#if DISABLE_SOURCEGEN_REGEX || !NET
    private static Regex RegexBar() => s_regexBar;
    private static readonly Regex s_regexBar = new(@"bar", RegexOptions.None);
#elif DISABLE_SOURCEGEN_REGEXOPTIONS
    [GeneratedRegex(@"bar")]
    private static partial Regex RegexBar();
#else
    [GeneratedRegex(@"bar", RegexOptions.None)]
    private static partial Regex RegexBar();
#endif
}
