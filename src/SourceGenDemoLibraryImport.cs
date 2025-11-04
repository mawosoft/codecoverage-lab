// Copyright (c) Matthias Wolf, Mawosoft.

using System;
using System.Runtime.InteropServices;

namespace FooLib;

public partial class SourceGenDemoLibraryImport
{
    private const string Kernel32 = "kernel32.dll";

#pragma warning disable CA1707 // Identifiers should not contain underscores
    public const int STD_INPUT_HANDLE = -10;
    public const int STD_OUTPUT_HANDLE = -11;
#pragma warning restore CA1707 // Identifiers should not contain underscores

    public int AutoProp1 { get; set; }

    public int Method1(int value)
    {
        var handle = GetStdHandle(value);
        if (AutoProp1 == 0 && GetConsoleMode(handle, out uint mode))
        {
            return (int)mode;
        }
        return 0;
    }

    [LibraryImport(Kernel32, SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static partial IntPtr GetStdHandle(int nStdHandle);

    [LibraryImport(Kernel32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static partial bool GetConsoleMode(IntPtr handle, out uint mode);
}
