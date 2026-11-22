# Copyright (c) Matthias Wolf, Mawosoft.

using namespace System
using namespace System.IO

class ProjectManager {
    [string]$PackagesPropsFile

    ProjectManager() {
        $this.PackagesPropsFile = [Path]::GetFullPath($script:Context.RepoRoot + '/Directory.Packages.props')
    }
}
