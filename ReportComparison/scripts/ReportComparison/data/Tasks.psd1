@{
    collect = @(
        @{
            Config   = 'Debug'
            Family   = 'coverlet'
            Platform = 'all'
            Version  = 'all'
            Options  = @('default', 'singlehit')
            Format   = 'cobertura'
        }
        @{
            Config   = 'Release'
            Family   = 'coverlet'
            Platform = 'vstest'
            Version  = 'latest'
            Options  = 'default'
            Format   = 'cobertura'
        }
        @{
            Config   = 'Debug'
            Family   = 'mscc'
            Platform = 'all'
            Version  = 'all'
            Options  = @('default', 'static')
            Format   = @{
                'cobertura' = 'mscc-xml'
                'mscc-xml'  = 'cobertura'
                'mscc-bin'  = @('mscc-xml', 'cobertura')
            }
        }
        @{
            Config   = 'Release'
            Family   = 'mscc'
            Platform = 'vstest'
            Version  = 'latest'
            Options  = 'default'
            Format   = @('cobertura', 'mscc-xml')
        }
    )
    merge   = @(
        @{
            Moniker = 'vstest-netsdk'
            Version = 'all'
            Inputs  = @(
                @{
                    Config   = 'Debug'
                    Family   = 'mscc'
                    Platform = 'vstest'
                    Version  = 'latest'
                    Options  = 'default'
                    Format   = @{
                        'cobertura' = 'cobertura'
                        'mscc-xml'  = 'mscc-xml'
                        'mscc-bin'  = 'mscc-xml'
                    }
                }
            )
        }
        @{
            Moniker = 'rpg'
            Version = 'all'
            Inputs  = @(
                @{
                    Config   = 'Debug'
                    Family   = 'mscc'
                    Platform = 'vstest'
                    Version  = 'latest'
                    Options  = 'default'
                    Format   = @{
                        'cobertura' = @('default', 'raw')
                        'mscc-xml'  = 'default'
                    }
                }
                @{
                    Config   = 'Debug'
                    Family   = 'coverlet'
                    Platform = 'vstest'
                    Version  = 'latest'
                    Options  = 'default'
                    Format   = @{
                        'cobertura' = 'default'
                    }
                }
                @{
                    Config   = 'Debug'
                    Family   = 'coverlet'
                    Platform = 'vstest'
                    Version  = 'latest'
                    Options  = 'singlehit'
                    Format   = @{
                        'cobertura' = 'raw'
                    }
                }
            )
        }
    )
    test    = @(
        # TODO where to place the reports?
        # Do this via options test1, test2, etc instead. Caveat: must be filtered out from regular reports.
        # All test1/2/n include the test assembly and each one excludes something from coverage so we can
        # compare reports with various missing pieces.
        # Best to use source file exclude since this supported by both coverlet and mscc. mscc also supports
        # function exclude.
        # Note that filter expressions differ: mscc uses regex (even for source files), coverlet uses globbing.

        @{
            Config   = 'Debug'
            Family   = 'coverlet'
            Platform = 'vstest'
            Version  = 'latest'
            Options  = @('default', 'singlehit')
            Format   = 'cobertura'
        }
        @{
            Config   = 'Release'
            Family   = 'coverlet'
            Platform = 'vstest'
            Version  = 'latest'
            Options  = 'default'
            Format   = 'cobertura'
        }
        @{
            Config   = 'Debug'
            Family   = 'mscc'
            Platform = 'vstest'
            Version  = 'latest'
            Options  = 'default'
            Format   = @('cobertura', 'mscc-xml')
        }
        @{
            Config   = 'Release'
            Family   = 'mscc'
            Platform = 'vstest'
            Version  = 'latest'
            Options  = 'default'
            Format   = 'mscc-xml'
        }
    )
}
