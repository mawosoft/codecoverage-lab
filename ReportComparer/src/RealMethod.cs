// Copyright (c) Matthias Wolf, Mawosoft.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ReportComparer;

[DebuggerDisplay("{Name}")]
internal sealed class RealMethod
{
    private Dictionary<ParsedReport, ParsedMethod?> _parsedMethods = new(ReferenceEqualityComparer.Instance);
    private bool _frozen;

    public RealAssembly ParentAssembly => ParentType.ParentAssembly;
    public RealType ParentType { get; }
    public string Name { get; private set; }
    public int Order { get; set; }
    public (RealSource source, Range range) BoundingRange { get; private set; }
    public IReadOnlyDictionary<ParsedReport, ParsedMethod?> ParsedMethods => _parsedMethods;

    public RealMethod(RealType parentType)
    {
        ParentType = parentType;
        Name = "";
    }

    public void Freeze(IEnumerable<ParsedReport> allReports)
    {
        Debug.Assert(!_frozen);
        Debug.Assert(_parsedMethods.Values.All(m => m is null || m.BoundingRange.RealSource.Order != 0));
        var ranges = _parsedMethods.Values.OfType<ParsedMethod>().OrderBy(m => m.BoundingRange.RealSource.Order).Select(m => m.BoundingRange);
        var source = ranges.First().RealSource;
        BoundingRange = (source, Range.GetBoundingRange(ranges.TakeWhile(br => br.RealSource == source).Select(br => br.Range)));
        foreach (var report in allReports) _parsedMethods.TryAdd(report, null);
        Debug.Assert(_parsedMethods.All(kvp => kvp.Key.Order != 0));
        _parsedMethods = _parsedMethods.OrderBy(kvp => kvp.Key.Order).ToDictionary(_parsedMethods.Comparer);
        _frozen = true;
    }

    public void AddParsedMethod(ParsedMethod parsedMethod)
    {
        Debug.Assert(!_frozen);
        Debug.Assert(parsedMethod.RealMethod is null);
        _parsedMethods.Add(parsedMethod.ParentReport, parsedMethod);
        if (parsedMethod.Name.Length > Name.Length) Name = parsedMethod.Name;
        parsedMethod.RealMethod = this;
        if (parsedMethod.ParentType.RealType is null)
        {
            ParentType.AddParsedType(parsedMethod.ParentType);
        }
    }
}
