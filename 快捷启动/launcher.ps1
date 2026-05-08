$ErrorActionPreference = 'Stop'

$root = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')).Path

$exe = Get-ChildItem -LiteralPath $root -Filter 'LiuYingPet.exe' -Recurse -ErrorAction SilentlyContinue |
    Where-Object { $_.FullName -notmatch '\\obj\\' } |
    Sort-Object @{ Expression = { if ($_.FullName -match '\\bin\\') { 1 } else { 0 } }; Ascending = $true },
        @{ Expression = { $_.LastWriteTime }; Descending = $true } |
    Select-Object -First 1

if ($exe) {
    Start-Process -FilePath $exe.FullName -WorkingDirectory $exe.DirectoryName
    exit 0
}

$project = Join-Path $root 'LiuYingPet.csproj'
if ((Test-Path -LiteralPath $project) -and (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Start-Process -FilePath 'dotnet' -ArgumentList @('run', '--project', $project) -WorkingDirectory $root
    exit 0
}

Add-Type -AssemblyName PresentationFramework
[System.Windows.MessageBox]::Show(
    'LiuYingPet.exe was not found, and dotnet is not available.',
    'LiuYingPet',
    'OK',
    'Warning') | Out-Null
exit 1
