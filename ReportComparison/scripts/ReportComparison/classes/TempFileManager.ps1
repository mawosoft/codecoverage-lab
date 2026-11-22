# Copyright (c) Matthias Wolf, Mawosoft.

using namespace System
using namespace System.Collections
using namespace System.IO

class TempFileManager {
    hidden [ArrayList]$_files = [ArrayList]::new()
    hidden [ArrayList]$_directories = [ArrayList]::new()

    [string]CreateTempFile() {
        $path = [Path]::GetTempFileName()
        $this._files.Add($path)
        return $path
    }

    [string]CreateTempDirectory() {
        $path = [Directory]::CreateTempSubdirectory().FullName
        $this._directories.Add($path)
        return $path
    }

    [void]Dispose() {
        foreach ($path in $this._files) { try { [File]::Delete($path) } catch {} }
        foreach ($path in $this._directories) { try { [Directory]::Delete($path, $true) } catch {} }
        $this._files.Clear()
        $this._directories.Clear()
    }
}
