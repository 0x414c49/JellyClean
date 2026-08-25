param(
    [Parameter(Mandatory = $true)]
    [string]$Version,

    [Parameter(Mandatory = $true)]
    [string]$SourceUrl,

    [Parameter(Mandatory = $true)]
    [string]$Checksum,

    [Parameter(Mandatory = $true)]
    [string]$Timestamp,

    [string]$ManifestPath = "manifest.json"
)

$manifestRaw = Get-Content -LiteralPath $ManifestPath -Raw | ConvertFrom-Json
if ($manifestRaw -is [array]) {
    $manifest = $manifestRaw
} else {
    $manifest = @($manifestRaw)
}

$plugin = $manifest[0]

$versionEntry = [ordered]@{
    version = $Version
    changelog = "See GitHub release notes."
    targetAbi = "10.11.11.0"
    sourceUrl = $SourceUrl
    checksum = $Checksum
    timestamp = $Timestamp
}

$existingVersions = @($plugin.versions | Where-Object { $_.version -ne $Version })
$plugin.versions = @($versionEntry) + $existingVersions

$itemsJson = @($manifest | ForEach-Object { $_ | ConvertTo-Json -Depth 10 })
$json = "[" + [Environment]::NewLine + ($itemsJson -join ("," + [Environment]::NewLine)) + [Environment]::NewLine + "]"
$json | Set-Content -LiteralPath $ManifestPath -Encoding utf8
