# Coverlet in Visual Studio

**Goal:** Automatically collect [Coverlet](https://github.com/coverlet-coverage/coverlet) code coverage when running tests in Visual Studio's Test Explorer.

With the release of **Visual Studio 2026** and the availability of its built-in code coverage analysis expanded to the **Community** and **Professional** editions, this has become a somewhat academic exercise. However, some developers may still prefer using *Coverlet* over *Microsoft Code Coverage*, which - although freely available - remains closed source.

## The Easy Way

The simplest approach involves using the [Fine Code Coverage](https://github.com/FortuneN/FineCodeCoverage) extension, which also supports previous versions of Visual Studio. Caveat: when using this extension with *Coverlet*, it must run the tests a second time in the background to collect the coverage data.

## The Not-So-Easy Way

### coverlet.collector 6.0.4

To avoid running tests twice, *Coverlet* needs to be configured so that it behaves as if it was invoked via `dotnet test --collect "XPlat Code Coverage"` from the command line. This can be achieved using a [.runsettings](./WithVSTest/TestCoverlet604VSTest.NoMitigations/coverlet.runsettings) file:

- Add the test adapter path for *Coverlet*, either hardcoded or dynamically via a [custom build target](./WithVSTest/Directory.Build.targets).
- Explicitly enable the *Coverlet* data collector:\
  `<DataCollector friendlyName="XPlat Code Coverage" enabled="true">`
- In the [project file](./WithVSTest/TestCoverlet604VSTest.NoMitigations/TestCoverlet604VSTest.NoMitigations.csproj), specify the path to the .runsettings file.

Now, running the tests in *Visual Studio* will generate a coverage report. However, the report is likely to be incomplete and examining the data collector logs reveals errors like:

```
[coverlet]Unable to instrument module: <assemblyfile>
  Mono.Cecil.AssemblyResolutionException: Failed to resolve assembly: '<assemblyname>'
```

This issue stems from the fact that *Visual Studio* does not use the MSBuild `test` or `vstest` targets to execute tests. Instead, it runs the `.NET Framework` versions of `vstest.console` and `datacollector`, which causes *Coverlet* instrumentation to fail. This is a *Coverlet* limitation, as *Microsoft.CodeCoverage* does not encounter these errors.

Common mitigations, such as setting `CopyLocalLockFileAssemblies` in the project file, usually fail here because the errors involve runtime assemblies rather than custom dependencies. One workaround for `.NET` projects is to build the test project as [self-contained](./WithVSTest/TestCoverlet604VSTest.SelfContained/TestCoverlet604VSTest.SelfContained.csproj), though this copies a massive number of files into the output directory and doesn't solve the issue for `.NET Framework` projects.

A more reasonable solution is demonstrated in [this project](./WithVSTest/TestCoverlet604VSTest.UseCopyForCoverlet/TestCoverlet604VSTest.UseCopyForCoverlet.csproj). It injects a [custom build target](./WithVSTest/CopyForCoverlet.targets) that copies only a few explicitly named runtime assemblies into the output directory. For projects targeting `.NET Framework`, it only copies `mscorlib.dll`.

To visualize the collected coverage data, they can be imported into *Visual Studio*'s own Code Coverage window, or processed via [ReportGenerator](https://github.com/danielpalme/ReportGenerator). The latter can be added as an external tool to *Visual Studio* and configured via a `.netconfig` file.

### coverlet.collector 10.0.0

None of the methods above are compatible with this version because it has dropped support for `.NET Standard` completely.

### coverlet.MTP 10.0.0

Enabling this package requires command line arguments to be passed to the test executable. However, *Visual Studio* does not provide a mechanism to do so; it ignores both the `launchSettings.json` file and the `RunArguments` and `TestingPlatformCommandLineArguments` properties when running tests through the Test Explorer. Instead it starts the test executable in server mode and uses RPC to communicate with it.

One possible solution is demonstrated in [this project](./WithMTP/TestCoverletMTP.UseStartupObject/TestCoverletMTP.UseStartupObject.csproj). It defines its own [`Main` entry point](./WithMTP/TestCoverletMTP.UseStartupObject/Program.cs), injects the `--coverlet` argument if *Visual Studio* runs the tests, and finally calls the entry point provided by the test framework.
