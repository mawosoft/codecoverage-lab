// Copyright (c) Matthias Wolf, Mawosoft.

using System;
using System.Runtime.InteropServices;

namespace DemoLib;

public partial class SourceGenLibImport
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

#if DISABLE_SOURCEGEN_LIBRARYIMPORT || !NET
    [DllImport(Kernel32, SetLastError = true)]
    //[SupportedOSPlatform("windows")]
    private static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport(Kernel32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    //[SupportedOSPlatform("windows")]
    private static extern bool GetConsoleMode(IntPtr handle, out uint mode);
#else
    [LibraryImport(Kernel32, SetLastError = true)]
    private static partial IntPtr GetStdHandle(int nStdHandle);

    [LibraryImport(Kernel32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetConsoleMode(IntPtr handle, out uint mode);
#endif
}
