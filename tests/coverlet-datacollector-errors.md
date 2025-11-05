## General

- Coverlet running with NetFx version of vstest.console fails with:
  ```
  [coverlet]Unable to instrument module: C:\Users\mw\Projects\Learning\codecoverage-lab\tests\NoMitigations\bin\Debug\net9.0\FooLib.dll
    Mono.Cecil.AssemblyResolutionException: Failed to resolve assembly: '<assemblyname>'
  ```
- Some unresolved assemblies don't cause failure, but coverlet cannot determine if methods contained in them do have the `DoesNotReturnAttribute`.

## JSON serialization source generator

- ExcludeByFile `*.g.cs` instead of ExcludeByAttribute `GeneratedCodeAttribute` DOES NOT work.
- Unresolved assemblies
  - System.Runtime
  - System.Private.CoreLib

## LibraryImport source generator

- ExcludeByFile `*.g.cs` instead of ExcludeByAttribute `GeneratedCodeAttribute` works.
- Unresolved assemblies
  - System.Runtime.InteropServices
  - System.Private.CoreLib

## Regex source generator

- No longer fails. Not sure what's changed.
