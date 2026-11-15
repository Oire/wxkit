# Packs WxKit into artifacts/packages: a native package for each runtime, such as
# WxKit.Native.win-x64, from its build in artifacts/native, and then WxKit, which depends on them.
# Run scripts/build-native.ps1 for the same runtimes first.
param(
	[string[]]$Runtimes = @("win-x64"),
	[string]$Configuration = "Release"
)
$ErrorActionPreference = "Stop"
$root = Split-Path $PSScriptRoot -Parent
$output = Join-Path $root "artifacts/packages"
# WxKit restores the native packages it was just given, so it restores into a folder of its own,
# where an older package with the same version cannot stand in for them.
$cache = Join-Path $root "artifacts/package-cache"

Remove-Item -Recurse -Force $output, $cache -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force $output | Out-Null

foreach ($runtime in $Runtimes) {
	dotnet pack (Join-Path $root "src/WxKit.Native") -c $Configuration -o $output -nologo "-p:WxKitRuntime=$runtime"
	if ($LASTEXITCODE -ne 0) { throw "Packing WxKit.Native.$runtime failed." }
}

# MSBuild reads a semicolon in a property as a separator of properties, so the list is escaped.
$runtimeList = $Runtimes -join "%3B"
dotnet pack (Join-Path $root "src/WxKit") -c $Configuration -o $output -nologo "-p:WxKitPacking=true" "-p:WxKitRuntimes=$runtimeList" "-p:RestoreAdditionalProjectSources=$output" "-p:RestorePackagesPath=$cache"
if ($LASTEXITCODE -ne 0) { throw "Packing WxKit failed." }

foreach ($package in Get-ChildItem $output -Filter *.nupkg) {
	$size = [math]::Round($package.Length / 1MB, 1)
	Write-Host "$($package.Name): $size MB"
}
