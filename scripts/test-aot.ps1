# Publishes the Hello sample with Native AOT and runs it in smoke test mode, which shows its frame,
# waits for the event loop to start, and exits. This catches what a normal build cannot: a native
# library missing from the link, or one the published executable still needs beside it.
param(
	[string]$Runtime = "win-x64",
	[int]$TimeoutSeconds = 30
)
$ErrorActionPreference = "Stop"
$root = Split-Path $PSScriptRoot -Parent
$output = Join-Path $root "artifacts/aot/$Runtime"

# Native AOT finds the linker through vcvarsall.bat, which in the Visual Studio 2026 Build Tools
# runs vswhere.exe by name, so the installer's folder has to be on PATH.
if ($IsWindows) {
	$installer = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio/Installer"
	if ((Test-Path $installer) -and -not ($env:PATH -split ";" -contains $installer)) {
		$env:PATH = "$installer;$env:PATH"
	}
}

Remove-Item -Recurse -Force $output -ErrorAction SilentlyContinue
dotnet publish (Join-Path $root "samples/Hello") -c Release -r $Runtime -o $output -nologo
if ($LASTEXITCODE -ne 0) { throw "Publishing the sample failed." }

# On macOS, the publish makes an app bundle too, which is what an application ships, so the test
# checks its Info.plist and runs the executable inside it.
if ($IsMacOS) {
	$bundle = Join-Path $output "Hello.app"
	plutil -lint (Join-Path $bundle "Contents/Info.plist") | Out-Null
	if ($LASTEXITCODE -ne 0) { throw "The sample's Info.plist is not valid." }
	codesign --verify --strict $bundle
	if ($LASTEXITCODE -ne 0) { throw "The sample's bundle is not validly signed." }
	$executable = Join-Path $bundle "Contents/MacOS/Hello"
} else {
	$executable = Join-Path $output ($IsWindows ? "Hello.exe" : "Hello")
}
$extras = Get-ChildItem $output -File | Where-Object { $_.Name -like "*wxkitnative*" }
if ($extras) { throw "The published sample still ships $($extras.Name -join ', ') beside it." }

$process = Start-Process $executable -ArgumentList "--smoke" -PassThru
if (-not $process.WaitForExit($TimeoutSeconds * 1000)) {
	Stop-Process -Id $process.Id
	throw "The published sample did not exit within $TimeoutSeconds seconds."
}
if ($process.ExitCode -ne 0) { throw "The published sample exited with $($process.ExitCode)." }
$size = [math]::Round((Get-Item $executable).Length / 1MB, 1)
Write-Host "The Native AOT sample for $Runtime started and exited cleanly. It is $size MB."
