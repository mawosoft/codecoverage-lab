// Copyright (c) Matthias Wolf, Mawosoft.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using ReportComparer.Helpers;

namespace ReportComparer;

[DebuggerDisplay("{RealSource}")]
internal sealed class ParsedSource
{
    private readonly List<EquatableSequence<ParsedRange>> _linesCoverage = [];
    private List<ParsedMethod> _methods = [];
    private bool _frozen;

    public ParsedAssembly ParentAssembly { get; }
    public RealSource RealSource { get; }
    public IReadOnlyList<EquatableSequence<ParsedRange>> LinesCoverage => _linesCoverage;
    public IReadOnlyList<ParsedMethod> Methods => _methods;

    public ParsedReport ParentReport => ParentAssembly.ParentReport;

    public ParsedSource(ParsedAssembly parentAssembly, RealSource realSource)
    {
        ParentAssembly = parentAssembly;
        RealSource = realSource;
    }

    public void Freeze()
    {
        Debug.Assert(!_frozen);
        for (int i = 0; i < _linesCoverage.Count; i++)
        {
            if (LinesCoverage[i].Count > 1)
            {
                var sequence = (List<ParsedRange>)_linesCoverage[i].Values;
                sequence = sequence.OrderBy(r => r.Range).ThenBy(r => r.Status).ToList();
                _linesCoverage[i] = EquatableSequence.Create(sequence);
            }
        }
        _methods = _methods.OrderBy(m => m.BoundingRange).ThenBy(m => m.Name).ToList();
        _frozen = true;
    }

    public void AddMethod(ParsedMethod parsedMethod)
    {
        Debug.Assert(!_frozen);
        Debug.Assert(parsedMethod.ParentSource == this);
        _methods.Add(parsedMethod);
    }

    public void AddRange(ParsedRange parsedRange)
    {
        Debug.Assert(!_frozen);
        Debug.Assert(parsedRange.ParentSource == this);
        int endLine = parsedRange.Range.EndLine;
        if (endLine >= _linesCoverage.Count)
        {
            CollectionsMarshal.SetCount(_linesCoverage, endLine + 1);
        }
        for (int i = parsedRange.Range.StartLine; i <= endLine; i++)
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
