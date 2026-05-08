param(
    [switch]$NoLaunch
)

$ErrorActionPreference = 'Stop'

function Get-Text([int[]]$CodePoints) {
    return -join ($CodePoints | ForEach-Object { [char]$_ })
}

$displayName = Get-Text @(0x6D41,0x8424,0x684C,0x5BA0)
$packageRoot = $PSScriptRoot
$sourceDir = Get-ChildItem -LiteralPath $packageRoot -Directory |
    Where-Object { Test-Path -LiteralPath (Join-Path $_.FullName 'LiuYingPet.exe') } |
    Sort-Object Name |
    Select-Object -First 1 -ExpandProperty FullName
$installDir = Join-Path $env:LOCALAPPDATA 'LiuYingPet'
$desktopDir = [Environment]::GetFolderPath('Desktop')
$startMenuDir = Join-Path $env:APPDATA 'Microsoft\Windows\Start Menu\Programs'
$desktopShortcut = Join-Path $desktopDir ($displayName + '.lnk')
$startMenuShortcut = Join-Path $startMenuDir ($displayName + '.lnk')
$uninstallScript = Join-Path $packageRoot 'uninstall.ps1'
$installedUninstallScript = Join-Path $installDir 'uninstall.ps1'
$runKeyPath = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Run'
$uninstallKey = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\LiuYingPet'

if (-not $sourceDir -or -not (Test-Path -LiteralPath $sourceDir)) {
    throw "Application payload not found: $sourceDir"
}

Get-Process -Name 'LiuYingPet' -ErrorAction SilentlyContinue | Stop-Process -Force

New-Item -ItemType Directory -Path $installDir -Force | Out-Null
robocopy $sourceDir $installDir /MIR /NFL /NDL /NJH /NJS /NC /NS | Out-Null

Copy-Item -LiteralPath $uninstallScript -Destination $installedUninstallScript -Force

$shell = New-Object -ComObject WScript.Shell
foreach ($shortcutPath in @($desktopShortcut, $startMenuShortcut)) {
    $shortcut = $shell.CreateShortcut($shortcutPath)
    $shortcut.TargetPath = (Join-Path $installDir 'LiuYingPet.exe')
    $shortcut.WorkingDirectory = $installDir
    $shortcut.Description = $displayName
    $shortcut.IconLocation = ((Join-Path $installDir 'LiuYingPet.exe') + ',0')
    $shortcut.Save()
}

New-Item -Path $uninstallKey -Force | Out-Null
Set-ItemProperty -Path $uninstallKey -Name 'DisplayName' -Value $displayName
Set-ItemProperty -Path $uninstallKey -Name 'DisplayVersion' -Value '1.0.0'
Set-ItemProperty -Path $uninstallKey -Name 'Publisher' -Value 'Wei Zhao'
Set-ItemProperty -Path $uninstallKey -Name 'InstallLocation' -Value $installDir
Set-ItemProperty -Path $uninstallKey -Name 'UninstallString' -Value "powershell -NoProfile -ExecutionPolicy Bypass -File `"$installedUninstallScript`""
Set-ItemProperty -Path $uninstallKey -Name 'QuietUninstallString' -Value "powershell -NoProfile -ExecutionPolicy Bypass -File `"$installedUninstallScript`""

if (-not $NoLaunch) {
    Start-Process -FilePath (Join-Path $installDir 'LiuYingPet.exe') -WorkingDirectory $installDir
}
