// Copyright (c) Matthias Wolf, Mawosoft.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ReportComparer;

[DebuggerDisplay("{BoundingRange} [{_parsedMethods.Count}, {TypesAreAmbiguous}] {System.Linq.Enumerable.FirstOrDefault(_parsedMethods)?.Name}")]
internal sealed class RealMethod
{
    private List<ParsedMethod> _parsedMethods = [];
    private bool _frozen;

    public RealSource ParentSource { get; }
    public Range BoundingRange { get; private set; }
    public IReadOnlyCollection<ParsedMethod> ParsedMethods => _parsedMethods;
    public IReadOnlyCollection<ParsedType> ParsedTypes { get; private set; } = null!;
    public bool TypesAreAmbiguous { get; private set; }
    public IReadOnlyCollection<ParsedReport> Reports { get; private set; } = null!;

    public RealMethod(ParsedMethod parsedMethod)
    {
        ParentSource = parsedMethod.RealSource;
        AddParsedMethod(parsedMethod);
    }

    public void Freeze()
    {
        Debug.Assert(!_frozen);
        BoundingRange = Range.GetBoundingRange(_parsedMethods.Select(m => m.BoundingRange));
        Debug.Assert(_parsedMethods.All(m => m.ParentReport.Order != 0));
        _parsedMethods = _parsedMethods.OrderBy(m => m.ParentReport.Order).ToList();
        ParsedTypes = new HashSet<ParsedType>(_parsedMethods.Select(m => m.ParentType), ReferenceEqualityComparer.Instance);
        TypesAreAmbiguous = ParsedTypes.DistinctBy(t => t.RealType).Skip(1).Any();
        Reports = _parsedMethods.Select(m => m.ParentReport).ToArray();
        Debug.Assert(Reports.Count == Reports.Distinct().Count());
        _frozen = true;
    }

    public void AddParsedMethod(ParsedMethod parsedMethod)
    {
        Debug.Assert(!_frozen);
        Debug.Assert(parsedMethod.RealMethod is null);
        Debug.Assert(parsedMethod.RealSource == ParentSource);
        _parsedMethods.Add(parsedMethod);
        parsedMethod.RealMethod = this;
    }
}
