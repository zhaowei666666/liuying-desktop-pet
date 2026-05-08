$ErrorActionPreference = 'Stop'

function Get-Text([int[]]$CodePoints) {
    return -join ($CodePoints | ForEach-Object { [char]$_ })
}

$displayName = Get-Text @(0x6D41,0x8424,0x684C,0x5BA0)
$installDir = Join-Path $env:LOCALAPPDATA 'LiuYingPet'
$desktopShortcut = Join-Path ([Environment]::GetFolderPath('Desktop')) ($displayName + '.lnk')
$startMenuShortcut = Join-Path (Join-Path $env:APPDATA 'Microsoft\Windows\Start Menu\Programs') ($displayName + '.lnk')
$runKeyPath = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Run'
$uninstallKey = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\LiuYingPet'

Get-Process -Name 'LiuYingPet' -ErrorAction SilentlyContinue | Stop-Process -Force

if (Test-Path -LiteralPath $desktopShortcut) {
    Remove-Item -LiteralPath $desktopShortcut -Force
}

if (Test-Path -LiteralPath $startMenuShortcut) {
    Remove-Item -LiteralPath $startMenuShortcut -Force
}

Remove-ItemProperty -Path $runKeyPath -Name $displayName -ErrorAction SilentlyContinue
if (Test-Path -LiteralPath $uninstallKey) {
    Remove-Item -LiteralPath $uninstallKey -Recurse -Force
}

if (Test-Path -LiteralPath $installDir) {
    Remove-Item -LiteralPath $installDir -Recurse -Force
}
