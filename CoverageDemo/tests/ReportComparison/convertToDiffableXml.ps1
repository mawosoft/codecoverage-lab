# Copyright (c) Matthias Wolf, Mawosoft.
<#
.SYNOPSIS
    Converts an XML file to a diffable version.

.NOTES
    - Sorts attributes of all XML elements alphabetically.
    - By default, sorts 'package' or 'module' elements alphabetically by 'name' attribute.
#>

[CmdletBinding(PositionalBinding = $false)]
param(
    [Parameter(Mandatory, Position = 0)]
    [ValidateNotNullOrWhiteSpace()]
    [string]$Path,

    [Parameter(Mandatory, Position = 1)]
    [ValidateNotNullOrWhiteSpace()]
    [string]$Destination,

    # Array of XPath queries for sorting subelements.
    # Default values handle Cobertura and MS CodeCoverage XML format.
    [hashtable[]]$ElementSort = @(
        @{ xpath = '/coverage/packages/package/@name' }
        @{ xpath = '/coverage/packages/package/classes/class/@name' }
        @{ xpath = '/results/modules/module/@name' }
        @{ xpath = '/results/skipped_modules/skipped_module/@name' }
        @{ xpath = '/results/modules/module/functions/function/ranges/range/@start_column'; number = $true }
        @{ xpath = '/results/modules/module/functions/function/ranges/range/@start_line'; number = $true }
    )
)

$xmlDoc = [xml](Get-Content -LiteralPath $Path -Raw)
foreach ($item in $ElementSort) {
    $xpath = $item['xpath']
    $nodes = $null
    if ($item['number']) {
        $nodes = [array]$xmlDoc.DocumentElement.SelectNodes($xpath) | Sort-Object { [int]$_.Value } -Stable
    }
    else {
        $nodes = [array]$xmlDoc.DocumentElement.SelectNodes($xpath) | Sort-Object Value -Stable
    }
    if ($nodes) {
        $nodes = $nodes.OwnerElement
        # Group-Object doesn't group by reference equality
        $groups = [System.Linq.Enumerable]::GroupBy[object, object]($nodes, { param($n)return $n.ParentNode }, [System.Collections.Generic.ReferenceEqualityComparer]::Instance).ToArray()
        foreach ($group in $groups) {
            $parent = $group.Key
            $group.ForEach({ $null = $parent.RemoveChild($_) })
            $group.ForEach({ $null = $parent.AppendChild($_) })
        }
    }
}
$elems = [array]$xmlDoc.DocumentElement.SelectNodes('//*[@*]')
foreach ($elem in $elems) {
    $attribs = $elem.Attributes | Sort-Object Name -Stable
    $elem.RemoveAllAttributes()
    foreach ($attr in $attribs) {
        $null = $elem.Attributes.Append($attr)
    }
}
$xmlDoc.Save($Destination)
