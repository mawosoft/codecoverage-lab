// Copyright (c) Matthias Wolf, Mawosoft.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ReportComparer;

[DebuggerDisplay("{Name,nq}")]
internal sealed class RealAssembly
{
    private Dictionary<ParsedReport, ParsedAssembly?> _parsedAssemblies = new(ReferenceEqualityComparer.Instance);
    private Dictionary<string, RealSource> _realSources = new(StringComparer.OrdinalIgnoreCase);
    private List<RealType> _realTypes = [];
    private int _frozenState;

    public string Name { get; }
    public int Order { get; set; }
    public IReadOnlyDictionary<ParsedReport, ParsedAssembly?> ParsedAssemblies => _parsedAssemblies;
    public IReadOnlyCollection<RealSource> RealSources => _realSources.Values;
    public IReadOnlyCollection<RealType> RealTypes => _realTypes;

    public RealAssembly(string name)
    {
        Name = name;
    }

    public void FreezeParsing(IEnumerable<ParsedReport> allReports)
    {
        Debug.Assert(_frozenState == 0);
        Debug.Assert(_parsedAssemblies.All(kvp => kvp.Key.Order != 0));
        _realSources = _realSources.OrderBy(kvp => kvp.Value.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(_realSources.Comparer);
        foreach (var report in allReports) _parsedAssemblies.TryAdd(report, null);
        _parsedAssemblies = _parsedAssemblies.OrderBy(kvp => kvp.Key.Order).ToDictionary(_parsedAssemblies.Comparer);
        _frozenState = 1;
    }

    public void Freeze()
    {
        Debug.Assert(_frozenState == 1);
        CalculateNamespaces();
        foreach (var realType in _realTypes) realType.Freeze(_parsedAssemblies.Keys);
        foreach (var realSource in _realSources.Values) realSource.Freeze(_parsedAssemblies.Keys);
        Debug.Assert(_realTypes.All(t => t.RealMethods.First().BoundingRange.source.Order != 0));
        _realTypes = _realTypes.OrderBy(t => t.RealMethods.First().BoundingRange.source.Order)
            .ThenBy(t => t.RealMethods.First().BoundingRange.range)
            .ThenBy(t => t.Name)
            .ToList();
        for (int i = 0; i < _realTypes.Count; i++) _realTypes[i].Order = i;
        _frozenState = 2;
    }

    public void AddParsedAssembly(ParsedAssembly parsedAssembly)
    {
        Debug.Assert(_frozenState == 0);
        Debug.Assert(parsedAssembly.RealAssembly == this);
        _parsedAssemblies.Add(parsedAssembly.ParentReport, parsedAssembly);
    }

    public RealSource GetOrAddRealSource(string fullName)
    {
        Debug.Assert(_frozenState == 0);
        fullName = Path.GetFullPath(fullName);
        if (_realSources.TryGetValue(fullName, out var source)) return source;
        source = new RealSource(this, fullName);
        _realSources.Add(fullName, source);
        return source;
    }

    public RealMethod CreateRealMethod(ParsedMethod parsedMethod)
    {
        Debug.Assert(_frozenState == 1);
        Debug.Assert(parsedMethod.RealMethod is null);
        Debug.Assert(parsedMethod.ParentAssembly.RealAssembly == this);
        RealType? realType = parsedMethod.ParentType.RealType;
        if (realType is null)
        {
            realType = new RealType(this);
            _realTypes.Add(realType);
        }
        var realMethod = new RealMethod(realType);
        realType.AddRealMethod(realMethod);
        realMethod.AddParsedMethod(parsedMethod);
        return realMethod;
    }

    private void CalculateNamespaces()
    {
        HashSet<string> namespaces = [];
        HashSet<string> candidates = [];
        foreach (var realType in _realTypes)
        {
            ReadOnlySpan<char> name = realType.Name;
            int pos = name.IndexOf('+');
            if (pos >= 0)
            {
                name = name[..pos];
                pos = name.LastIndexOf('.');
                if (pos >= 0) namespaces.Add(name[..pos].ToString());
            }
            else if ((pos = name.LastIndexOf('.')) >= 0)
            {
                candidates.Add(name[..pos].ToString());
            }
        }
        candidates.ExceptWith(namespaces);
        foreach (var candidate in candidates)
        {
            bool verified = true;
            foreach (var realType in _realTypes)
            {
                if (realType.Name == candidate)
                {
                    verified = false;
                    break;
                }
            }
            if (verified) namespaces.Add(candidate);
        }
        foreach (var @namespace in namespaces.OrderByDescending(s => s.Length))
        {
            foreach (var realType in _realTypes)
            {
                if (realType.Namespace.Length == 0
                    && realType.Name.Length > @namespace.Length
                    && realType.Name[@namespace.Length] == '.'
                    && realType.Name.StartsWith(@namespace, StringComparison.Ordinal))
                {
                    realType.Namespace = @namespace;
                }
            }
        }
    }
}
