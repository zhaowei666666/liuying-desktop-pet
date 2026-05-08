$ErrorActionPreference = 'Stop'

function Get-Text([int[]]$CodePoints) {
    return -join ($CodePoints | ForEach-Object { [char]$_ })
}

$root = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')).Path
$appDir = Get-ChildItem -LiteralPath $PSScriptRoot -Directory |
    Where-Object { Test-Path -LiteralPath (Join-Path $_.FullName 'LiuYingPet.exe') } |
    Sort-Object Name |
    Select-Object -First 1 -ExpandProperty FullName
$exePath = Join-Path $appDir 'LiuYingPet.exe'
$iconPath = Join-Path $appDir 'app.ico'
$manifestPath = Join-Path $appDir 'assets\manifest.json'
$logPath = Join-Path $env:APPDATA ((Get-Text @(0x6D41,0x8424,0x684C,0x5BA0)) + '\错误日志.txt')

if (-not $appDir) { throw 'Release payload directory was not found.' }

Get-Process -Name 'LiuYingPet' -ErrorAction SilentlyContinue | Stop-Process -Force
if (Test-Path -LiteralPath $logPath) { Remove-Item -LiteralPath $logPath -Force }

dotnet build $root | Out-Null
dotnet publish (Join-Path $root 'LiuYingPet.csproj') -c Release -r win-x64 --self-contained true -o $appDir | Out-Null

if (-not (Test-Path -LiteralPath $exePath)) { throw "Missing release exe: $exePath" }
if (-not (Test-Path -LiteralPath $iconPath)) { throw "Missing icon in release output: $iconPath" }

$manifest = Get-Content -LiteralPath $manifestPath -Raw -Encoding UTF8 | ConvertFrom-Json
foreach ($entry in $manifest.states.PSObject.Properties) {
    $asset = Join-Path $appDir ('assets\' + $entry.Value)
    if (-not (Test-Path -LiteralPath $asset)) {
        throw "Missing mapped asset for $($entry.Name): $asset"
    }
}

$p = Start-Process -FilePath $exePath -WorkingDirectory $appDir -PassThru
Start-Sleep -Seconds 5
$p.Refresh()
if ($p.HasExited) { throw "Release app exited during smoke test." }
if (Test-Path -LiteralPath $logPath) { throw "Runtime error log was created." }

& (Join-Path $PSScriptRoot 'installer.ps1') -NoLaunch

$installDir = Join-Path $env:LOCALAPPDATA 'LiuYingPet'
if (-not (Test-Path -LiteralPath (Join-Path $installDir 'LiuYingPet.exe'))) {
    throw 'Installed exe was not found after installer run.'
}

Write-Host 'Regression test passed.'
