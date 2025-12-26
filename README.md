# codecoverage-lab

## Comparison between `Coverlet` and `Microsoft.CodeCoverage`

| Feature                    | coverlet | MSCC | Notes |
|----------------------------|:--------:|:----:|-------|
| Line coverage              |    ✔    |   ✔  |       |
| Block coverage             |   ❌    |   ✔<sup>1</sup> | 1. Not in *Cobertura* report. |
| Branch coverage            |    ✔<sup>1</sup> | ✔<sup>2</sup>  | 1. Incorrect details for multiple conditions in some cases.<br>2. Only in *Cobertura* report. |
| Partial coverage           |    ✔    |   ✔<sup>1</sup> | 1. Missing from *XML* report if created together with another report format. |
| Lambda coverage            |    ✔    |   ✔  |       |
| Actual hit count           |    ✔    |  ❌  |       |
| Single-hit                 |    ✔    |   ✔  |       |
| Hits per property accessor |    ✖<sup>1</sup> | ✔ | 1. Only if on separate lines. |
| Simple type names          |   ❌    |   ✔  | For example, `int` instead of `System.Int32`. |
| Nested type separator in *Cobertura* report |    ✔    |  ❌  | Needed by *ReportGenerator* to distinguish nested types and namespaces. |
| Inclusion/exclusion filters|    ✔    |   ✔  |       |
| Static instrumentation     |    ✔    |   ✔  |       |
| Dynamic instrumentation    |   ❌    |   ✔  |       |
| VSTest integration .NET    |    ✔    |   ✔  |       |
| Visual Studio integration  |    ✖<sup>1 2</sup> | ✔<sup>3</sup> | 1. May fail, because VStudio (including VStudio 2026) uses the .NetFx version of VSTest.<br>2. Everything must be configured and enabled in *.runsettings* file.<br>3. Via *Analyze Code Coverage* for all or selected tests. |
| Microsoft Testing Platform (MTP) integration |   ❌    |   ✔  |       |


## Resources

### Coverage Collection

- [Coverlet](https://github.com/coverlet-coverage/coverlet)
- [Microsoft.CodeCoverage](https://github.com/microsoft/codecoverage)

### Coverage Presentation

- [Fine Code Coverage](https://github.com/FortuneN/FineCodeCoverage)
- [ReportGenerator](https://github.com/danielpalme/ReportGenerator)
- [Missing Coverage](https://github.com/mawosoft/MissingCoverage)
- [Visual Studio 2026](https://learn.microsoft.com/en-us/visualstudio/test/using-code-coverage-to-determine-how-much-code-is-being-tested)

### Test Platforms

- [Visual Studio Test Platform (VSTest)](https://github.com/microsoft/vstest)
- [Microsoft.Testing.Platform (MTP)](https://aka.ms/testingplatform)
- [xUnit.net](https://xunit.net/)
