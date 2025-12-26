// Copyright (c) Matthias Wolf, Mawosoft.

using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using ReportComparer.Helpers;

namespace ReportComparer;

[DebuggerDisplay("{Name,nq}")]
internal sealed class ParsedSource
{
    private readonly List<EquatableSequence<ParsedRange>> _linesCoverage = [];

    public ParsedReport ParentReport => ParentAssembly.ParentReport;
    public ParsedAssembly ParentAssembly { get; }
    public RealSource RealSource { get; }
    public string FullName => RealSource.FullName;
    public string Name => RealSource.Name;
    public IReadOnlyList<EquatableSequence<ParsedRange>> LinesCoverage => _linesCoverage;

    public ParsedSource(ParsedAssembly parentAssembly, RealSource realSource)
    {
        ParentAssembly = parentAssembly;
        RealSource = realSource;
    }

    public void AddRange(ParsedRange parsedRange)
    {
        Debug.Assert(parsedRange.ParentSource == this);
        int startLine = parsedRange.Range.StartLine;
        int endLine = parsedRange.Range.EndLine;
        if (endLine >= _linesCoverage.Count) CollectionsMarshal.SetCount(_linesCoverage, endLine + 1);
        for (int i = startLine; i <= endLine; i++)
        {
            var sequence = _linesCoverage[i].Values as List<ParsedRange>;
            if (sequence is null)
            {
                sequence = [];
                _linesCoverage[i] = EquatableSequence.Create(sequence);
            }
            sequence.Add(parsedRange);
        }
    }
}
