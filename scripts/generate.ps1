# Regenerates WxKit's bindings from the wxWidgets interface headers. Doxygen reads the headers
# and writes XML, which the generator reads. Both run only here: the generated files are checked
# in, so a normal build needs neither.
param(
	# Only write the Doxygen XML, without running the generator.
	[switch]$XmlOnly
)
$ErrorActionPreference = "Stop"
$root = Split-Path $PSScriptRoot -Parent
$wxWidgets = Join-Path $root "native/wxWidgets"
$tools = Join-Path $root "artifacts/tools"
$output = Join-Path $root "artifacts/doxygen"

# wxWidgets's own documentation is built with this release, so its XML is what the headers are written for.
$doxygenVersion = "1.15.0"
$doxygenArchives = @{
	"windows" = @{ Name = "doxygen-$doxygenVersion.windows.x64.bin.zip"; Sha256 = "44658b9cc5c91749e6e3cc426ba63e2550b4a4a7619065acd77029aa234719c6" }
}

function Get-Doxygen {
	if (-not $IsWindows) { throw "This script only fetches Doxygen for Windows so far." }
	$archive = $doxygenArchives["windows"]
	$directory = Join-Path $tools "doxygen-$doxygenVersion"
	$executable = Join-Path $directory "doxygen.exe"
	if (Test-Path $executable) { return $executable }
	New-Item -ItemType Directory -Force $tools | Out-Null
	$download = Join-Path $tools $archive.Name
	$tag = "Release_" + $doxygenVersion.Replace(".", "_")
	Invoke-WebRequest "https://github.com/doxygen/doxygen/releases/download/$tag/$($archive.Name)" -OutFile $download
	$hash = (Get-FileHash $download -Algorithm SHA256).Hash.ToLowerInvariant()
	if ($hash -ne $archive.Sha256) { throw "$($archive.Name) has SHA-256 $hash, not the expected $($archive.Sha256)." }
	Expand-Archive $download -DestinationPath $directory -Force
	Remove-Item $download
	return $executable
}

function Invoke-Doxygen([string]$doxygen) {
	# wxWidgets's Doxyfile takes its settings from the environment, and writes into its own source
	# tree. A configuration of WxKit's own includes it, and changes only what WxKit needs: XML of
	# the interface headers alone, written under artifacts.
	$doxyfileDirectory = Join-Path $wxWidgets "docs/doxygen"
	Remove-Item -Recurse -Force $output -ErrorAction SilentlyContinue
	New-Item -ItemType Directory -Force $output | Out-Null
	$configuration = Join-Path $output "Doxyfile"
	$interface = (Join-Path $wxWidgets "interface").Replace("\", "/")
	@(
		"@INCLUDE = `"$((Join-Path $doxyfileDirectory "Doxyfile").Replace("\", "/"))`"",
		"OUTPUT_DIRECTORY = `"$($output.Replace("\", "/"))`"",
		"INPUT = `"$interface`"",
		"WARN_LOGFILE = `"$((Join-Path $output "warnings.log").Replace("\", "/"))`"",
		"QUIET = YES"
	) | Set-Content $configuration -Encoding utf8NoBOM
	$settings = @{
		WXWIDGETS = $wxWidgets; WX_HTML_OUTPUT_DIR = (Join-Path $output "html")
		GENERATE_XML = "YES"; GENERATE_HTML = "NO"; GENERATE_HTMLHELP = "NO"; GENERATE_LATEX = "NO"
		GENERATE_QHP = "NO"; GENERATE_DOCSET = "NO"; GENERATE_TAGFILE = ""; SEARCHENGINE = "NO"
		SERVER_BASED_SEARCH = "NO"; HTML_CODE_FOLDING = "NO"; DOT_IMAGE_FORMAT = "svg"; HTML_HEADER = ""
		CUSTOM_THEME_CSS = ""; CUSTOM_THEME_JS1 = ""; CUSTOM_THEME_JS2 = ""
	}
	foreach ($setting in $settings.GetEnumerator()) { [Environment]::SetEnvironmentVariable($setting.Key, $setting.Value) }
	Push-Location $doxyfileDirectory
	try {
		& $doxygen $configuration
		if ($LASTEXITCODE -ne 0) { throw "Doxygen failed." }
	} finally {
		Pop-Location
	}
	$classes = (Get-ChildItem (Join-Path $output "xml") -Filter "class*.xml").Count
	Write-Host "Doxygen wrote XML for $classes classes to $(Join-Path $output "xml")."
}

Invoke-Doxygen (Get-Doxygen)
if ($XmlOnly) { return }
throw "The generator does not exist yet. Use -XmlOnly."
