// Copyright (c) Matthias Wolf, Mawosoft.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ReportComparer;

[DebuggerDisplay("{Name}")]
internal sealed class RealType
{
    private List<ParsedType> _parsedTypes = [];
    private bool _frozen;

    public RealAssembly ParentAssembly { get; }
    public string Name { get; }
    public string Namespace { get; internal set; }
    public string TypeName { get; internal set; }
    public IReadOnlyCollection<ParsedType> ParsedTypes => _parsedTypes;

    public RealType(RealAssembly realAssembly, string normalizedName)
    {
        ParentAssembly = realAssembly;
        Name = normalizedName;
        TypeName = normalizedName;
        Namespace = "";
    }

    public void Freeze()
    {
        Debug.Assert(!_frozen);
        // TODO ThenBy
        _parsedTypes = _parsedTypes.OrderBy(t => t.ParentReport.Order).ToList();
        _frozen = true;
    }

    public void AddParsedType(ParsedType parsedType)
    {
        Debug.Assert(!_frozen);
        Debug.Assert(parsedType.RealType == this);
        _parsedTypes.Add(parsedType);
    }

    public bool TrySetNamespace(string @namespace)
    {
        Debug.Assert(!_frozen);
        int length = @namespace.Length;
        if (Name.Length <= length || Name[length] != '.' || !Name.StartsWith(@namespace, StringComparison.Ordinal))
        {
            return false;
        }
        int offset = length + 1;
        Namespace = @namespace;
        TypeName = Name[offset..];
        foreach (var parsedType in _parsedTypes)
        {
            string name = parsedType.Name;
            if (name.Length > length && name[length] == '.' && name.StartsWith(@namespace, StringComparison.Ordinal))
            {
                parsedType.TypeName = name[offset..];
            }
        }
        return true;
    }
}
