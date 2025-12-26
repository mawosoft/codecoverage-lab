// Copyright (c) Matthias Wolf, Mawosoft.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ReportComparer;

[DebuggerDisplay("{Name}")]
internal sealed class RealType
{
    private Dictionary<ParsedReport, ParsedType?> _parsedTypes = new(ReferenceEqualityComparer.Instance);
    private List<RealMethod> _realMethods = [];
    private bool _frozen;

    public RealAssembly ParentAssembly { get; }
    public string Namespace { get; set; }
    public string Name { get; private set; }
    public int Order { get; set; }
    public IReadOnlyDictionary<ParsedReport, ParsedType?> ParsedTypes => _parsedTypes;
    public IReadOnlyCollection<RealMethod> RealMethods => _realMethods;
    public bool Frozen => _frozen;

    public RealType(RealAssembly parentAssembly)
    {
        ParentAssembly = parentAssembly;
        Namespace = "";
        Name = "";
    }

    public void Freeze(IEnumerable<ParsedReport> allReports)
    {
        Debug.Assert(!_frozen);
        foreach (var report in allReports) _parsedTypes.TryAdd(report, null);
        Debug.Assert(_parsedTypes.All(kvp => kvp.Key.Order != 0));
        _parsedTypes = _parsedTypes.OrderBy(kvp => kvp.Key.Order).ToDictionary(_parsedTypes.Comparer);
        foreach (var realMethod in _realMethods) realMethod.Freeze(_parsedTypes.Keys);
        Debug.Assert(_realMethods.All(m => m.BoundingRange.source.Order != 0));
        _realMethods = _realMethods.OrderBy(m => m.BoundingRange.source.Order)
            .ThenBy(m => m.BoundingRange.range)
            .ThenBy(m => m.Name)
            .ToList();
        for (int i = 0; i < _realMethods.Count; i++) _realMethods[i].Order = i;
        _frozen = true;
    }

    public void AddRealMethod(RealMethod realMethod)
    {
        Debug.Assert(!_frozen);
        _realMethods.Add(realMethod);
    }

    public void AddParsedType(ParsedType parsedType)
    {
        Debug.Assert(!_frozen);
        Debug.Assert(parsedType.RealType is null);
        _parsedTypes.Add(parsedType.ParentReport, parsedType);
        if (Name.Contains('+'))
        {
            if (parsedType.Name.Length > Name.Length && parsedType.Name.Contains('+'))
            {
                Name = parsedType.Name;
            }
        }
        else
        {
            if (parsedType.Name.Length > Name.Length || parsedType.Name.Contains('+'))
            {
                Name = parsedType.Name;
            }
        }
        parsedType.RealType = this;
    }
}
