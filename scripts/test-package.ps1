# Uses the packages in artifacts/packages the way an application would: a new project that
# references WxKit from them, with the Hello sample's code. It runs the project as it is, which
# loads the shared library from the native package, and then publishes it with Native AOT, which
# links the static one, and runs that. Run scripts/pack.ps1 first.
param(
	[string]$Runtime = "win-x64",
	[int]$TimeoutSeconds = 30
)
$ErrorActionPreference = "Stop"
$root = Split-Path $PSScriptRoot -Parent
$packages = Join-Path $root "artifacts/packages"
$test = Join-Path $root "artifacts/package-test"
# The packages restore beside the project rather than inside it, where its default items would
# take in the source files packages carry, such as WxKit's resource merge task.
$project = Join-Path $test "app"
$restored = Join-Path $test "packages"

$package = Get-ChildItem $packages -Filter "WxKit.*.nupkg" | Where-Object { $_.Name -notlike "WxKit.Native.*" } | Select-Object -First 1
if (-not $package) { throw "There is no WxKit package in $packages. Run scripts/pack.ps1 first." }
$version = $package.Name.Substring("WxKit.".Length, $package.Name.Length - "WxKit.".Length - ".nupkg".Length)

if ($IsWindows) {
	$installer = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio/Installer"
	if ((Test-Path $installer) -and -not ($env:PATH -split ";" -contains $installer)) {
		$env:PATH = "$installer;$env:PATH"
	}
}

# Empty Directory.Build files shut the project off from the repository's build, so none of its
# props, targets, or native builds reach it, and it restores into a folder of its own, so it uses
# these packages and no others.
Remove-Item -Recurse -Force $test -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force $project | Out-Null
Set-Content (Join-Path $project "Directory.Build.props") "<Project />" -Encoding utf8NoBOM
Set-Content (Join-Path $project "Directory.Build.targets") "<Project />" -Encoding utf8NoBOM
Set-Content (Join-Path $project "nuget.config") @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
	<config>
		<add key="globalPackagesFolder" value="$restored" />
	</config>
	<packageSources>
		<clear />
		<add key="local" value="$packages" />
		<add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
	</packageSources>
</configuration>
"@ -Encoding utf8NoBOM
Set-Content (Join-Path $project "PackageTest.csproj") @"
<Project Sdk="Microsoft.NET.Sdk">
	<PropertyGroup>
		<OutputType>WinExe</OutputType>
		<TargetFramework>net10.0</TargetFramework>
		<ImplicitUsings>enable</ImplicitUsings>
		<Nullable>enable</Nullable>
		<ApplicationId>io.github.trypsynth.wxkit.packagetest</ApplicationId>
	</PropertyGroup>
	<ItemGroup>
		<PackageReference Include="WxKit" Version="$version" />
	</ItemGroup>
</Project>
"@ -Encoding utf8NoBOM
Copy-Item (Join-Path $root "samples/Hello/Program.cs") $project

function Test-Executable([string]$Path, [string]$What) {
	$process = Start-Process $Path -ArgumentList "--smoke" -PassThru
	if (-not $process.WaitForExit($TimeoutSeconds * 1000)) {
		Stop-Process -Id $process.Id
		throw "$What did not exit within $TimeoutSeconds seconds."
	}
	if ($process.ExitCode -ne 0) { throw "$What exited with $($process.ExitCode)." }
}

dotnet build $project -c Release -nologo
if ($LASTEXITCODE -ne 0) { throw "Building the package test failed." }
Test-Executable (Join-Path $project "bin/Release/net10.0/PackageTest$($IsWindows ? '.exe' : '')") "The package test, run as it is,"

$published = Join-Path $project "published"
dotnet publish $project -c Release -r $Runtime -o $published -nologo "-p:PublishAot=true"
if ($LASTEXITCODE -ne 0) { throw "Publishing the package test with Native AOT failed." }
$extras = Get-ChildItem $published -File | Where-Object { $_.Name -like "*wxkitnative*" }
if ($extras) { throw "The published package test still ships $($extras.Name -join ', ') beside it." }
$executable = Join-Path $published "PackageTest$($IsWindows ? '.exe' : '')"
if ($IsMacOS) {
	$executable = Join-Path $published "PackageTest.app/Contents/MacOS/PackageTest"
	if (-not (Test-Path $executable)) { throw "The published package test has no app bundle." }
}
Test-Executable $executable "The package test, published with Native AOT,"
$size = [math]::Round((Get-Item $executable).Length / 1MB, 1)
Write-Host "WxKit $version restored from its packages, ran, and published with Native AOT for $Runtime as $size MB."
