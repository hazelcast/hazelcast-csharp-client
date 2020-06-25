
# prepare directories
$scriptRoot = "$PSScriptRoot" # expected to be build/
$slnRoot = [System.IO.Path]::GetFullPath("$scriptRoot\..")

$srcDir = "$slnRoot\src"
$tmpDir = "$slnRoot\temp"
$outDir = "$slnRoot\temp\output"
$docDir = "$slnRoot\docs"
$buildDir = "$slnRoot\build"

if (-not (test-path "$tmpDir\docfx.site")) {
  throw "Missing documentation directory."
}

# functions
function findLatestVersion($path) {
	if ([System.IO.Directory]::Exists($path)) {
		$v = ls $path | `
			 foreach-object { [Version]::Parse($_.Name) } | `
			 sort -descending | `
			 select -first 1
	}
	else {
		$l = [System.IO.Path]::GetDirectoryname($path).Length
		$l = $path.Length - $l
		$v = ls "$path.*" | `
			 foreach-object { [Version]::Parse($_.Name.SubString($l)) } | `
			 sort -descending | `
			 select -first 1
	}
	return $v
}

# say hello
Write-Host "Hazelcast.NET Documentation Server"

# ensure docfx
function ensureDocFx() {
	Write-Host ""
	Write-Host "Detected DocFX"
	$v = findLatestVersion "$($env:USERPROFILE)\.nuget\packages\docfx.console"
	Write-Host "  v$v"
	$dir = "$($env:USERPROFILE)\.nuget\packages\docfx.console\$v"
	Write-Host "  -> $dir"

	return $dir
}
$docfxDir = ensureDocFx
$docfxDir = "$docFxDir\tools"

# serve docs
Write-Host ""
Write-Host "Serving documentation...  (press ENTER to stop the server)"
$envPath = $env:Path
$env:Path = "$docfxDir;$env:Path"
#docfx docs/docfx.json --serve
docfx serve "$tmpDir\docfx.site"
$env:Path = $envPath

Write-Host ""
Write-Host "Done."

# eof
