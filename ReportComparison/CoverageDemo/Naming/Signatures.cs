// Copyright (c) Matthias Wolf, Mawosoft.

using System;
using System.Collections.Generic;

namespace CoverageDemo.Naming;

public class Signatures
{
    // Built-ins
    public void M(bool _, byte _1, sbyte _2, char _3) { }
    public void M(decimal _, double _1, float _2) { }
    public void M(int _, uint _1, nint _2, nuint _3) { }
    public void M(long _, ulong _1, short _2, ushort _3) { }
    public void M(object _, string _1, dynamic _2) { }

    // Nullables
    public void M(int? _, object? _1) { }
    public void M(MyInt? _, Version? _1) { }

    // References
    public void M(in MyInt _, ref MyInt _1, out MyInt value) { value = default; }

    // Pointers
    public unsafe void M(void* _, void** _1) { }
    public unsafe void M(MyInt* _) { }

    // Value tuples
    public void M((MyInt i1, MyInt i2) _) { }

    // Arrays
    public void M(MyInt[] _, MyInt[][] _1, MyInt[,] _2) { }
    public void M(params MyInt[] _) { }
    public void M(params ReadOnlySpan<MyInt> _) { }
    public void M(params IEnumerable<MyInt> _) { }

    // Nested types
    public void M(CoverageDemo.Layout.NestedClassSingleFile.Inner _) { }
    public void M(System.Environment.SpecialFolder _) { }
}
