$pkgman = $global:Context.GetPackageManager()
#$r1 = $pkgman.ValidateWithDirectoryPackageProps()
# foreach ($list in $pkgman.VersionsById.Values) {
#     foreach($info in $list) {
#         $info.Required = $true
#     }
# }
# $pkgman.DownloadRequiredPackages($false)
foreach ($list in $pkgman.VersionsById.Values) {
    $list[0].Meta.PackageId
    $null = $null
    foreach($info in $list) {
        $exists = $info.Exists()
        $resolved = $info.Resolve('net10.0')
        "$($info.Version)   $exists $resolved"
        $null = $null
    }
}
$null = $null
