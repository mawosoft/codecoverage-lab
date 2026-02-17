// Copyright (c) Matthias Wolf, Mawosoft.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ReportComparer.Helpers;

namespace ReportComparer;

[DebuggerDisplay("{Name,nq}")]
internal sealed class RealAssembly
{
    private List<ParsedAssembly> _parsedAssemblies = [];
    private readonly Dictionary<string, RealType> _realTypes = [];
    private Dictionary<string, RealSource> _realSources = new(StringComparer.OrdinalIgnoreCase);
    private bool _frozen;

    public ReportComparison ReportComparison { get; }
    public string Name { get; }
    public int Order { get; set; }
    public IReadOnlyCollection<ParsedAssembly> ParsedAssemblies => _parsedAssemblies;
    public IReadOnlyCollection<RealSource> RealSources => _realSources.Values;
    public IReadOnlyCollection<ParsedReport> Reports { get; private set; } = null!;

    public RealAssembly(ReportComparison reportComparison, string name)
    {
        ReportComparison = reportComparison;
        Name = name;
    }

    public void Freeze()
    {
        Debug.Assert(!_frozen);
        Debug.Assert(_parsedAssemblies.All(a => a.ParentReport.Order != 0));
        Debug.Assert(_realSources.Values.All(s => s.Order != 0));
        _parsedAssemblies = _parsedAssemblies.OrderBy(a => a.ParentReport.Order).ToList();
        _realSources = _realSources.OrderBy(kvp => kvp.Value.Order).ToDictionary(_realSources.Comparer);
        Reports = _parsedAssemblies.Select(a => a.ParentReport).ToArray();
        Debug.Assert(Reports.Count == Reports.Distinct().Count());
        CalculateNamespaces();
        // TODO do we need to sort real types?
        foreach (var realType in _realTypes.Values) realType.Freeze();
        foreach (var realSource in _realSources.Values) realSource.Freeze();
        _frozen = true;
    }

    public void AddParsedAssembly(ParsedAssembly parsedAssembly)
    {
        Debug.Assert(!_frozen);
        Debug.Assert(parsedAssembly.RealAssembly == this);
        _parsedAssemblies.Add(parsedAssembly);
    }

    public RealSource GetOrAddRealSource(string fullName)
    {
        Debug.Assert(!_frozen);
        fullName = Path.GetFullPath(fullName);
        if (_realSources.TryGetValue(fullName, out var source)) return source;
        source = new RealSource(this, fullName);
        _realSources.Add(fullName, source);
        return source;
    }

    public RealType GetOrAddRealType(string name)
    {
        Debug.Assert(!_frozen);
        if (_realTypes.TryGetValue(name, out var type)) return type;
        type = new RealType(this, name);
        _realTypes.Add(name, type);
        return type;
    }

    private void CalculateNamespaces()
    {
        var parsedTypeNames = _realTypes.Values.SelectMany(rt => rt.ParsedTypes).Select(pt => pt.Name).ToHashSet();
        var namespaces = parsedTypeNames.Select(name => name[..SymbolNameHelper.NamespaceLength(name)]).ToHashSet();
        namespaces.ExceptWith(parsedTypeNames);
        namespaces.Remove("");
        var orderedNamespaces = namespaces.OrderByDescending(s => s.Length).ToArray();
        foreach (var realType in _realTypes.Values)
        {
            for (int i = 0; i < orderedNamespaces.Length; i++)
            {
                if (realType.TrySetNamespace(orderedNamespaces[i])) break;
            }
        }
    }
}
