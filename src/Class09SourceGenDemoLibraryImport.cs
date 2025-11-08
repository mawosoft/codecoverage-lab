// Copyright (c) Matthias Wolf, Mawosoft.

using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace DemoLib;

#pragma warning disable CA1822 // Mark members as static
public partial class Class09SourceGenDemoLibraryImport
{
    private const string Kernel32 = "kernel32.dll";
    private const int StdOutputHandle = -11;

    public int Method1()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return 0;
        var handle = GetStdHandle(StdOutputHandle);
        GetConsoleMode(handle, out uint mode);
        return (int)mode;
    }

#if DISABLE_SOURCEGEN_LIBRARYIMPORT
#pragma warning disable SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time
    [DllImport(Kernel32, SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [SupportedOSPlatform("windows")]
    private static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport(Kernel32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [SupportedOSPlatform("windows")]
    private static extern bool GetConsoleMode(IntPtr handle, out uint mode);
#pragma warning restore SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time
#else
    [LibraryImport(Kernel32, SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [SupportedOSPlatform("windows")]
    private static partial IntPtr GetStdHandle(int nStdHandle);

    [LibraryImport(Kernel32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [SupportedOSPlatform("windows")]
    private static partial bool GetConsoleMode(IntPtr handle, out uint mode);
#endif
}
