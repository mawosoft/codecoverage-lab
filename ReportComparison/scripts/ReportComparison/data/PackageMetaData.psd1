@{
    'coverlet.collector'                        = @{
        Family   = 'coverlet'
        Platform = 'vstest'
        Versions = @('6.0.4', '10.0.0', '10.0.1')
    }
    'coverlet.console'                          = @{
        Family   = 'coverlet'
        Platform = 'dotnet'
        ToolName = 'coverlet.console.dll'
        Versions = @('6.0.4', '10.0.0', '10.0.1')
    }
    'coverlet.msbuild'                          = @{
        Family   = 'coverlet'
        Platform = 'msbuild'
        Versions = @('6.0.4', '10.0.0', '10.0.1')
    }
    'coverlet.MTP'                              = @{
        Family   = 'coverlet'
        Platform = 'mtp'
        Versions = @('10.0.0', '10.0.1')
    }
    'Microsoft.CodeCoverage'                    = @{
        Family      = 'mscc'
        Platform    = 'vstest'
        CoreDllName = 'Microsoft.CodeCoverage.Core.dll'
        Versions    = @('18.3.0', '18.4.0', '18.5.1', '18.6.0', '18.7.0', '18.8.1', '18.9.0', '18.10.1')
    }
    'Microsoft.Testing.Extensions.CodeCoverage' = @{
        Family      = 'mscc'
        Platform    = 'mtp'
        CoreDllName = 'Microsoft.CodeCoverage.Core.dll'
        Versions    = @('18.5.2', '18.6.2', '18.7.0', '18.8.0', '18.9.0', '18.10.0', '18.11.2')
    }
    'dotnet-coverage'                           = @{
        Family      = 'mscc'
        Platform    = 'dotnet'
        CoreDllName = 'Microsoft.CodeCoverage.Core.dll'
        ToolName    = 'dotnet-coverage.dll'
        Versions    = @('18.5.2', '18.6.2', '18.7.0', '18.8.0', '18.9.0', '18.10.0', '18.11.2')
    }
    'Microsoft.CodeCoverage.Console'            = @{
        # The package itself is deprecated, but it lives on in VStudio.
        # Community\Common7\IDE\Extensions\Microsoft\CodeCoverage.Console\
        # We will use the latest VStudio version for identification.
        Family      = 'mscc'
        Platform    = 'vstudio'
        CoreDllName = 'Microsoft.CodeCoverage.Core.dll'
        ToolName    = 'Microsoft.CodeCoverage.Console.exe'
    }
    'Microsoft.TestPlatform.CLI'                = @{
        # The package is part of the .NET SDK and references the Microsoft.CodeCoverage.IO package,
        # which is no longer updated. (See https://github.com/microsoft/vstest).
        # We will use the latest .NET SDK version for identification.
        # TODO Go one level up to Microsoft.TestPlatform.Utilities (CodeCoverageDataAttachmentsHandler)?
        Moniker     = 'vstest-netsdk'
        CoreDllName = 'Microsoft.TestPlatform.Utilities.dll'
    }
    'dotnet-reportgenerator-globaltool'         = @{
        Moniker  = 'rpg'
        ToolName = 'ReportGenerator.dll'
        Versions = @('5.5.11')
        Sources  = @('local')
    }
    'ilspycmd'                                  = @{
        Moniker  = 'ilspy'
        ToolName = 'ilspycmd.dll'
        Versions = @('11.1.0.9782')
    }
}
