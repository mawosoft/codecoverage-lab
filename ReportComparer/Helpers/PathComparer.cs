// Copyright (c) Matthias Wolf, Mawosoft.

using System.Collections.Generic;

namespace ReportComparer.Helpers;

internal sealed class PathComparer : IComparer<string>
{
    public static IComparer<string> Default { get; } = new PathComparer(false);
    public static IComparer<string> IgnoreCase { get; } = new PathComparer(true);

    private readonly bool _ignoreCase;
    private PathComparer(bool ignoreCase) => _ignoreCase = ignoreCase;
    public int Compare(string? x, string? y) => PathHelper.ComparePath(x, y, _ignoreCase);
}
