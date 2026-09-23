param(
    [string]$GameRoot = 'D:\SteamLibrary\steamapps\common\Grand Theft Auto V Enhanced',
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repositoryRoot = $PSScriptRoot
$scriptsRoot = Join-Path $GameRoot 'scripts'
$scriptSourceRoot = Join-Path $repositoryRoot 'Script'
$dllSourceRoot = Join-Path $repositoryRoot 'Dll'

if (-not (Test-Path (Join-Path $GameRoot 'ScriptHookVDotNet3.dll'))) {
    throw "ScriptHookVDotNet3.dll was not found under '$GameRoot'. Pass the correct -GameRoot path."
}

New-Item -ItemType Directory -Force -Path $scriptsRoot | Out-Null

function Test-FileContentEqual {
    param(
        [string]$SourcePath,
        [string]$DestinationPath
    )

    if (-not (Test-Path $DestinationPath)) {
        return $false
    }

    $sourceInfo = Get-Item $SourcePath
    $destinationInfo = Get-Item $DestinationPath
    if ($sourceInfo.Length -ne $destinationInfo.Length) {
        return $false
    }

    return (Get-FileHash $SourcePath -Algorithm SHA256).Hash -eq
        (Get-FileHash $DestinationPath -Algorithm SHA256).Hash
}

function Sync-ScriptMod {
    param(
        [System.IO.DirectoryInfo]$SourceDirectory,
        [string]$DestinationDirectory
    )

    New-Item -ItemType Directory -Force -Path $DestinationDirectory | Out-Null
    $sourceFiles = @(Get-ChildItem $SourceDirectory.FullName -File -Recurse)
    $sourceRelativePaths = @{}
    $copiedCount = 0

    foreach ($sourceFile in $sourceFiles) {
        $relativePath = $sourceFile.FullName.Substring($SourceDirectory.FullName.Length).TrimStart('\')
        $sourceRelativePaths[$relativePath] = $true
        $destinationPath = Join-Path $DestinationDirectory $relativePath

        if (Test-FileContentEqual $sourceFile.FullName $destinationPath) {
            continue
        }

        $destinationParent = Split-Path $destinationPath -Parent
        New-Item -ItemType Directory -Force -Path $destinationParent | Out-Null
        Copy-Item $sourceFile.FullName $destinationPath -Force
        $copiedCount++
    }

    $removedCount = 0
    foreach ($destinationFile in @(Get-ChildItem $DestinationDirectory -File -Recurse)) {
        $relativePath = $destinationFile.FullName.Substring($DestinationDirectory.Length).TrimStart('\')
        if (-not $sourceRelativePaths.ContainsKey($relativePath)) {
            Remove-Item $destinationFile.FullName -Force
            $removedCount++
        }
    }

    Get-ChildItem $DestinationDirectory -Directory -Recurse |
        Sort-Object FullName -Descending |
        Where-Object { -not (Get-ChildItem $_.FullName -Force) } |
        Remove-Item -Force

    Write-Host "Script/$($SourceDirectory.Name): copied $copiedCount, removed $removedCount"
}

foreach ($scriptMod in @(Get-ChildItem $scriptSourceRoot -Directory | Sort-Object Name)) {
    Sync-ScriptMod $scriptMod (Join-Path $scriptsRoot $scriptMod.Name)
}

$projects = @(Get-ChildItem $dllSourceRoot -Filter '*.csproj' -File -Recurse | Sort-Object FullName)
foreach ($project in $projects) {
    $deployDirectory = Join-Path $scriptsRoot $project.BaseName
    Write-Host "Dll/$($project.BaseName): running incremental $Configuration build"

    & dotnet build $project.FullName `
        --configuration $Configuration `
        "/p:GameRoot=$GameRoot" `
        "/p:DeployDirectory=$deployDirectory"

    if ($LASTEXITCODE -ne 0) {
        throw "Build failed for '$($project.FullName)'."
    }
}