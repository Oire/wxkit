# Builds wxWidgets and WxKit's shim for each runtime. One CMake build per runtime makes both kinds
# of output, sharing one static build of wxWidgets:
#   static: wxkitnative.lib, every wxWidgets library it needs, and wxkit.res, for Native AOT to link into
#           the application, with a WxKit.Native.targets naming each of them.
#   shared: wxkitnative.dll, with wxWidgets linked into it, for everything else.
param(
	[string[]]$Runtimes = @("win-x64"),
	[ValidateSet("Release")]
	[string]$Configuration = "Release"
)
$ErrorActionPreference = "Stop"
$root = Split-Path $PSScriptRoot -Parent
$source = Join-Path $root "native/shim"
$wxWidgets = Join-Path $root "native/wxWidgets"
$windowsArchitectures = @{ "win-x64" = "x64"; "win-arm64" = "ARM64"; "win-x86" = "Win32" }
$windowsLibraries = @("kernel32.lib", "user32.lib", "gdi32.lib", "gdiplus.lib", "msimg32.lib", "comdlg32.lib", "winspool.lib", "winmm.lib", "shell32.lib", "shlwapi.lib", "comctl32.lib", "ole32.lib", "oleaut32.lib", "uuid.lib", "rpcrt4.lib", "advapi32.lib", "version.lib", "ws2_32.lib", "wininet.lib", "oleacc.lib", "uxtheme.lib", "imm32.lib", "dwmapi.lib", "usp10.lib")

function Invoke-Checked([string]$description) {
	if ($LASTEXITCODE -ne 0) { throw "$description failed." }
}

function Get-WindowsGenerator {
	# CMake falls back to Ninja on some setups, which cannot take -A, so the newest Visual Studio
	# with the C++ tools is looked up and named explicitly.
	$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio/Installer/vswhere.exe"
	$studio = & $vswhere -latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -format json | ConvertFrom-Json | Select-Object -First 1
	if (-not $studio) { throw "No Visual Studio with the C++ tools was found." }
	$years = @{ "16" = "2019"; "17" = "2022"; "18" = "2026" }
	$major = $studio.installationVersion.Split(".")[0]
	if (-not $years.ContainsKey($major)) { throw "Visual Studio $major is not one this script knows the CMake generator for." }
	return "Visual Studio $major $($years[$major])"
}

function Write-Targets([string]$directory, [string[]]$items) {
	$lines = @("<Project>", "", "`t<ItemGroup>") + ($items | ForEach-Object { "`t`t$_" }) + @("`t</ItemGroup>", "", "</Project>")
	Set-Content -Path (Join-Path $directory "WxKit.Native.targets") -Value $lines -Encoding utf8NoBOM
}

# wxWidgets keeps its bundled libraries, such as zlib and libpng, in submodules of its own.
if (-not (Test-Path (Join-Path $wxWidgets "src/zlib/zlib.h"))) {
	git -C $wxWidgets submodule update --init --recursive --depth 1
	Invoke-Checked "Fetching the wxWidgets submodules"
}

$generator = $null
foreach ($runtime in $Runtimes) {
	if (-not $windowsArchitectures.ContainsKey($runtime)) { throw "$runtime is not a runtime this script can build yet." }
	$generator ??= Get-WindowsGenerator
	$build = Join-Path $root "artifacts/native-build/$runtime"
	$output = Join-Path $root "artifacts/native/$runtime"
	cmake -S $source -B $build -G $generator -A $windowsArchitectures[$runtime] "-DCMAKE_MSVC_RUNTIME_LIBRARY=MultiThreaded"
	Invoke-Checked "Configuring $runtime"
	cmake --build $build --config $Configuration --parallel --target wxkitnative_static wxkitnative wxkit_resources
	Invoke-Checked "Building $runtime"
	$static = Join-Path $output "static"
	$shared = Join-Path $output "shared"
	Remove-Item -Recurse -Force $static, $shared -ErrorAction SilentlyContinue
	New-Item -ItemType Directory -Force $static, $shared | Out-Null
	# wxWidgets puts its libraries in a directory named after the compiler and architecture, such as vc_x64_lib.
	$libraries = @(Get-ChildItem (Join-Path $build "wx/lib") -Recurse -Filter *.lib | Where-Object { $_.Directory.Name -like "vc*_lib" })
	if ($libraries.Count -eq 0) { throw "The build did not produce any wxWidgets libraries." }
	$libraries += Get-Item (Join-Path $build "static/wxkitnative.lib")
	$resources = Get-ChildItem (Join-Path $build "wxkit_resources.dir") -Recurse -Filter wxkit.res | Select-Object -First 1
	if (-not $resources) { throw "The build did not produce wxkit.res." }
	foreach ($library in $libraries) { Copy-Item $library.FullName $static }
	Copy-Item $resources.FullName $static
	# A CMake build of wxWidgets does not name the Windows libraries it uses in its objects, so they
	# are listed here. They are all part of Windows, and a library nothing uses costs nothing.
	$items = @($libraries | Sort-Object Name | ForEach-Object { "<NativeLibrary Include=`"`$(MSBuildThisFileDirectory)$($_.Name)`" />" })
	$items += '<NativeLibrary Include="' + ($windowsLibraries -join ";") + '" />'
	$items += '<WxKitResource Include="$(MSBuildThisFileDirectory)wxkit.res" />'
	Write-Targets $static $items
	Copy-Item (Join-Path $build "shared/wxkitnative.dll") $shared
	$symbols = Join-Path $build "shared/wxkitnative.pdb"
	if (Test-Path $symbols) { Copy-Item $symbols $shared }
	Write-Host "Built $runtime into $output"
}
