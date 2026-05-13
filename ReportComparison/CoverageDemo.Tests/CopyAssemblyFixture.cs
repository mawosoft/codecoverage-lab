// Copyright (c) Matthias Wolf, Mawosoft.

using System;
using System.IO;
using Xunit;

[assembly: AssemblyFixture(typeof(CoverageDemo.CopyAssemblyFixture))]

namespace CoverageDemo;

/// <summary>
/// Xunit assembly fixture to copy statically instrumented assemblies before or
/// after test run. Intended for scenarios where automatic restore of instrumented
/// assemblies cannot be disabled.
/// </summary>
public sealed class CopyAssemblyFixture : IDisposable
{
    private const string EnvTargetDir = "CoverageDemo_CopyAssemblies_TargetDir";
    private const string EnvAssemblyFilter = "CoverageDemo_CopyAssemblies_AssemblyFilter";

    public CopyAssemblyFixture()
    {
        CopyAssemblies();
    }

    public void Dispose()
    {
    }

    private void CopyAssemblies()
    {
        var targetDir = Environment.GetEnvironmentVariable(EnvTargetDir);
        if (string.IsNullOrEmpty(targetDir)) return;
        var filter = Environment.GetEnvironmentVariable(EnvAssemblyFilter);
        if (string.IsNullOrEmpty(filter)) return;
        Directory.CreateDirectory(targetDir);
        var sourceDir = Path.GetDirectoryName(typeof(CopyAssemblyFixture).Assembly.Location);
        foreach (var filePath in Directory.EnumerateFiles(sourceDir!, filter))
        {
            var fileName = Path.GetFileName(filePath);
            File.Copy(filePath, Path.Combine(targetDir, fileName), overwrite: true);
        }
    }
}
