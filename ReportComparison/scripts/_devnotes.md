## Operations (main scripts)

- Test
  - Minimal collect plus compare after source code changes in CoverageDemo.
- Instrument (Disasm?, Decompile?)
  - Create instrumented R2R assemblies, disassemble, log dynamic instrumentation.
- Collect
  - Coverage collection for all variants, including merge/convert and ReportGenerator.
  - Select package versions via param or via report existence?
- Compare
  - Create comparisons based on collected reports.

## Projects/Packages

- For a given package, version, and config determine if a build is required?
  Do we need this generalization? Only the two MTP packages are version-specific.
- For a given package, version, and config assign operations?

How to handle version-specific (MTP) projects?
- Restore produces results for both configs, but only for a specific version.
  This then can be used for both config builds.
- The issue here is that builds are required for Instrument and Collect operations.
  Instrument is similar to Collect, but has fewer variations.

Configs: Debug, Release

- CoverageDemo
  - Packages: any
  - Versions: any
- CoverageDemo.Tests
  - Packages: VSTest, Tool
  - Versions: any
- WithCoverletMTP
  - Packages: coverlet.MTP
  - Versions: build per version
- WithMsCodeCoverageMTP
  - Packages: Microsoft.Testing.Extensions.CodeCoverage
  - Versions: build per version

## Report variants

See [older notes](file:///C:/Users/mw/Projects/Learning/codecoverage-lab/_notes/old%20notes/report%20groups/groups-notes.txt)

See [artifact names](_dev_artifactname.txt)

- Config: debug/release
  - All variants for Debug config
  - Only selected variants for Release config
- Framework: net10.0/net11.0
  - Do we need this? Only for vstest multi because that's where the framework interferes?
  - .NET 11 still includes outdated version, so framework makes no real difference.
- Instrumentation: static / dynamic
- Format
  ```
  cobertura              cbt-n  cbtr  cbrt-n
  cobertura-singlehit    cbt-1  cbt1  cbrt-1
  coverage-xml           cov-x  cvrx  cvrg-x
  coverage-bin           cov-b  cvrg  cvrg-b
  ```
  coverlet also has json, lcov, opencover, teamcity.
- Format count: multi / single
  - Should we make this a separate operation? The bug is caused by the SDK, not the package.
- Conversion (merge, only for mscc family)
  ```
  from                    to
  cobertura-singlehit     coverage-xml
  coverage-xml            cobertura-singlehit
  coverage-bin            coverage-xml  (this should be identical, i.e. it is a verification)
  ```
- ReportGenerator
  ```
  to          from                    family
  rpg         cobertura               coverlet
              cobertura-singlehit     mscc
              coverage-xml            mscc
  rpg-raw     cobertura-singlehit     coverlet, mscc
              coverage-xml            mscc
  ```
  - These should be done only for one (primary?) member of the family.
  - The rpg moniker should come first because this is not a simple merge/convert.

```
TODO no multi-format. Test the bug in SDK directly

config    Family    Platform    Version   Instrumentation   HitCount    Merge       prime format          merge
debug     coverlet  vstest      all       static            single      none        cobertura-1           cobertura-1 -> coverage-xml
release   mscc      dotnet      latest    dynamic           multi       convert     cobertura-n           coverage-xml -> cobertura-1
                    msbuild     previous  instrument        default     binxml      coverage-xml          coverage-bin -> coverage-xml
                    mtp                   default                       rpg         coverage-bin
                    vstudio                                                         cobertura-rpg
                                                                                    cobertura-rpg-raw
operations.psd1

@{
  # When running collect
  # - The instrumented assemblies are always copied, but not further processed (R2R/Decompile are a separate operation)
  # - Merging should be optimized per collect package to run as many merges as possible and loading the package in an ALC only once.
  # - We need to know beforehand which artifacts a task produces if we want to skip tasks.
  #   For example, do we keep mscc-bin around as a task output, or is the merged mscc-bin=>mscc-xml the task output.
  collect = @(
    @{
      Configuration = 'Debug'
      Family = 'coverlet'
      Platform = 'all'
      Version = 'all'
      Options = @('default', 'singlehit')
      Format = 'cobertura'
    }
    @{
      Configuration = 'Release'
      Family = 'coverlet'
      Platform = 'vstest'
      Version = 'latest'
      Options = 'default'
      Format = 'cobertura'
    }
    @{
      Configuration = 'Debug'
      Family = 'mscc'
      Platform = 'all'
      Version = 'all'
      Options = @('default', 'static')
      Format = @{
        'cobertura' = 'mscc-xml'
        'mscc-xml' = 'cobertura'
        'mscc-bin' = @('mscc-xml', 'cobertura')
      }
    }
    @{
      Configuration = 'Release'
      Family = 'mscc'
      Platform = 'vstest'
      Version = 'latest'
      Options = 'default'
      Format = @('cobertura', 'mscc-xml')
    }
  )
  merge = @(
    Sources = @(
    )
  )
}

```

## Package details

- `!tfm!` denotes the target framework subfolder, like `net10.0`.
- props/targets files have the original, case-sensitive package name as base name unless otherwise noted.
- Redirecting props/targets files are followed by `-->`
- TODO Don't use PackageDownloads and `<PackageDownloadVersion>` for older versions? Use some other config instead?
  We still need the crossgen tool and the local feed for ReportGenerator?

### coverlet.collector

```
props:      -
targets:    build/!tfm!/
core/tool:  build/!tfm!/

tfm               pkgver
netstandard2.0    6.0.4
net8.0            10.0.0, 10.0.1
net9.0            10.0.0, 10.0.1
net10.0           10.0.0, 10.0.1
```

### coverlet.console

```
props:      -
targets:    -
core/tool:  tools/!tfm!/any/

tfm               pkgver
net6.0            6.0.4
net8.0            10.0.0, 10.0.1
net9.0            10.0.0, 10.0.1
net10.0           10.0.0, 10.0.1
```

### coverlet.msbuild

```
// 6.0.4
props:      build/
targets:    build/
core/tool:  tasks/!tfm!/

// 10.0.0, 10.0.1
props:      build/!tfm!/            --> buildMultiTargeting
            buildMultiTargeting/
targets:    build/!tfm!/            --> buildMultiTargeting
            buildMultiTargeting/
core/tool:  tasks/!tfm!/

tfm               pkgver
netstandard2.0    6.0.4, 10.0.0, 10.0.1
net8.0            10.0.0, 10.0.1
net9.0            10.0.0, 10.0.1
net10.0           10.0.0, 10.0.1
```

### coverlet.MTP

```
props:      build/!tfm!/            --> buildMultiTargeting
            buildMultiTargeting/
            buildTransitive/!tfm!/  --> buildMultiTargeting
targets:    build/!tfm!/            --> buildMultiTargeting
            buildMultiTargeting/
            buildTransitive/!tfm!/  --> buildMultiTargeting
core/tool:  lib/!tfm!/

tfm               pkgver
netstandard2.0    10.0.0, 10.0.1
net8.0            10.0.0, 10.0.1
net10.0           10.0.0, 10.0.1
```

### Microsoft.CodeCoverage

```
props:      build/!tfm!/
targets:    build/!tfm!/
core/tool:  build/!tfm!/

tfm               pkgver
netstandard2.0    18.3.0 - 18.10.1
```

### Microsoft.Testing.Extensions.CodeCoverage

```
props:      build/!tfm!/            --> buildMultiTargeting
            buildMultiTargeting/
            buildTransitive/!tfm!/  --> buildMultiTargeting
targets:    build/!tfm!/            --> buildMultiTargeting
            buildMultiTargeting/
            buildTransitive/!tfm!/  --> buildMultiTargeting
core/tool:  lib/!tfm!/

tfm               pkgver
netstandard2.0    18.5.2 - 18.10.0
net8.0            18.5.2 - 18.10.0
```

### dotnet-coverage

```
props:      -
targets:    -
core/tool:  tools/!tfm!/any/

tfm               pkgver
net8.0            18.5.2 - 18.11.2
```

### dotnet-reportgenerator-globaltool

```
props:      -
targets:    -
core/tool:  tools/!tfm!/any/

tfm               pkgver
net10.0           5.5.11
```

### ilspycmd

```
props:      -
targets:    -
core/tool:  tools/!tfm!/any/

tfm               pkgver
net10.0           11.1.0.9782
```
