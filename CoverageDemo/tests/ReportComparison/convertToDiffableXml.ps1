# Copyright (c) Matthias Wolf, Mawosoft.
<#
.SYNOPSIS
    Converts a Cobertura or DynamicCoverage XML file to a diffable version.
#>

using namespace System.Collections
using namespace System.Collections.Generic
using namespace System.Linq
using namespace System.Xml
using namespace System.Xml.XPath

[CmdletBinding(PositionalBinding = $false)]
param(
    [Parameter(Mandatory, Position = 0)]
    [ValidateNotNullOrWhiteSpace()]
    [string]$Path,

    [Parameter(Mandatory, Position = 1)]
    [ValidateNotNullOrWhiteSpace()]
    [string]$Destination
)

function SortChildNodes {
    param([XmlNode]$queryRoot, [string]$xPathQuery, [string[]]$sortQueries)

    $xpath = [XPathExpression]::Compile($xPathQuery)
    foreach ($sortQuery in $sortQueries) {
        $xpath.AddSort($sortQuery, [Comparer]::DefaultInvariant)
    }
    $result = $queryRoot.CreateNavigator().Select($xpath)
    if ($result.Count -eq 0) {
        return
    }
    $result = [XmlNode[]]$result.GetNode()
    # Group-Object doesn't group by reference equality
    $groups = [Enumerable]::GroupBy[XmlNode, XmlNode]($result, { param($n) return $n.ParentNode }, [ReferenceEqualityComparer]::Instance).ToArray()
    foreach ($group in $groups) {
        $parent = $group.Key
        $group.ForEach({ $null = $parent.RemoveChild($_) })
        $group.ForEach({ $null = $parent.AppendChild($_) })
    }
}

$xmlDoc = [xml](Get-Content -LiteralPath $Path -Raw)
$root = $xmlDoc.DocumentElement
if ($root.get_Name() -eq 'coverage') {
    SortChildNodes $root '/coverage/packages/package' -sortQueries '@name'
    SortChildNodes $root '/coverage/packages/package/classes/class' -sortQueries "translate(@name, '<', ' ')", '@filename'
    SortChildNodes $root '//lines/line' -sortQueries 'number(@number)'
    SortChildNodes $root '/coverage/packages/package/classes/class/methods/method' -sortQueries 'number(lines/line[1]/@number)', '@name', '@signature'
    $root.SelectNodes('//conditions/condition/@number').ForEach({ $_.Value = '0' })
}
elseif ($root.get_Name() -eq 'results') {
    SortChildNodes $root '/results/*' -sortQueries 'name(.)'
    SortChildNodes $root '/results/modules/module/*' -sortQueries 'name(.)'
    SortChildNodes $root '/results/modules/module' -sortQueries '@name'
    SortChildNodes $root '/results/skipped_modules/skipped_module' -sortQueries '@name'
    SortChildNodes $root '/results/modules/module/skipped_functions/skipped_function' -sortQueries '@type_name', '@name'
    SortChildNodes $root '/results/modules/module/source_files/source_file' -sortQueries '@path'
    $modules = [XmlNode[]]$root.SelectNodes('/results/modules/module')
    foreach ($module in $modules) {
        $sourceIdMap = [Dictionary[string, string]]::new()
        $sourceIds = [XmlNode[]]$module.SelectNodes('./source_files/source_file/@id')
        for ([int]$i = 0; $i -lt $sourceIds.Length; $i++) {
            $sourceId = $sourceIds[$i]
            $sourceIdMap[$sourceId.Value] = $i
            $sourceId.Value = $i
        }
        $sourceIds = [XmlNode[]]$module.SelectNodes('./functions/function/ranges/range/@source_id')
        foreach ($sourceId in $sourceIds) {
            $sourceId.Value = $sourceIdMap[$sourceId.Value]
        }
    }
    $badNumbers = [XmlNode[]]$root.SelectNodes("(//@block_coverage|//@line_coverage)[contains(., ',')]")
    foreach ($badNumber in $badNumbers) {
        $badNumber.Value = $badNumber.Value.Replace([char]',', [char]'.')
    }
    SortChildNodes $root '//ranges/range' -sortQueries 'number(@source_id)', 'number(@start_line)', 'number(@start_column)'
    SortChildNodes $root '/coverage/packages/package/classes/class/methods/method' -sortQueries 'number(lines/line[1]/@number)', '@name', '@signature'
    SortChildNodes $root '/results/modules/module/functions/function' -sortQueries 'number(ranges/range[1]/@source_id)', 'number(ranges/range[1]/@start_line)', '@namespace', '@type_name', '@name'
}

$elems = [array]$root.SelectNodes('//*[@*]')
foreach ($elem in $elems) {
    $attribs = $elem.Attributes | Sort-Object Name -Stable
    $elem.RemoveAllAttributes()
    foreach ($attr in $attribs) {
        $null = $elem.Attributes.Append($attr)
    }
}
$xmlDoc.Save($Destination)
