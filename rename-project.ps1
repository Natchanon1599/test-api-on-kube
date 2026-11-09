<#
.SYNOPSIS
  Rename this project (e.g. Prototype -> TestKube).

.DESCRIPTION
  Default mode renames project-level names only:
    PrototypeApi      -> <New>Api        (folder, .csproj, namespace, Dockerfile, launch.json ...)
    PrototypeLibrary  -> <New>Library
    DotnetPrototype   -> Dotnet<New>     (.sln)
    pg-prototype      -> pg-<new>        (container name in README)
  docker-compose / compose files are never touched (if any).
  Sample classes (PrototypeController, PrototypeTest ...), API route and DB table stay as they are.

  -All renames EVERY "Prototype"/"prototype"/"PROTOTYPE" (classes, files, route /api/<new>,
  DB table <new>_test, db/*.sql ...). Existing databases must be recreated afterwards.

  The current name is detected from src/<Name>Api + src/<Name>Library, so the script can be run again later.

.EXAMPLE
  ./rename-project.ps1 TestKube
  ./rename-project.ps1 -NewName TestKube -All
  ./rename-project.ps1 TestKube -WhatIf     # preview only
#>
[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [Parameter(Mandatory = $true, Position = 0)]
    [string]$NewName,
    [string]$OldName,
    [switch]$All
)

$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$excludeDirs = @('.git', 'bin', 'obj', '.vs', 'node_modules')
$textExt = @('.cs', '.csproj', '.sln', '.slnx', '.props', '.targets', '.json', '.yml', '.yaml', '.md',
             '.sql', '.txt', '.http', '.xml', '.config', '.sh', '.env', '.example', '.dockerignore', '')
$scriptName = Split-Path -Leaf $PSCommandPath
$skipFiles = @($scriptName, 'rename-project.cmd','docker-compose.yml', 'docker-compose.yaml', 'compose.yml', 'compose.yaml')

# ---------- validate ----------
if ($NewName -notmatch '^[A-Za-z][A-Za-z0-9]*$') {
    throw "NewName '$NewName' must start with a letter and contain only letters/digits (used as C# namespace)."
}
$NewName = $NewName.Substring(0, 1).ToUpper() + $NewName.Substring(1)

if (-not $OldName) {
    $api = Get-ChildItem -Path (Join-Path $root 'src') -Directory |
        Where-Object { $_.Name -match '^(.+)Api$' -and (Test-Path (Join-Path $_.FullName "$($_.Name).csproj")) } |
        Select-Object -First 1
    if (-not $api) { throw "Cannot detect current project name (src/<Name>Api/<Name>Api.csproj not found). Use -OldName." }
    $OldName = $api.Name -replace 'Api$', ''
}
if ($OldName -ceq $NewName) { Write-Host "Project is already named '$NewName'. Nothing to do."; return }

$oldLower = $OldName.ToLower(); $newLower = $NewName.ToLower()
$oldUpper = $OldName.ToUpper(); $newUpper = $NewName.ToUpper()

# ordered list of [regex, replacement] (case-sensitive)
$rules = New-Object System.Collections.Generic.List[object]
if ($All) {
    $rules.Add(@([regex]::Escape($OldName), $NewName))
    $rules.Add(@([regex]::Escape($oldLower), $newLower))
    $rules.Add(@([regex]::Escape($oldUpper), $newUpper))
} else {
    $rules.Add(@("$([regex]::Escape($OldName))(?=Api|Library)", $NewName))
    $rules.Add(@("Dotnet$([regex]::Escape($OldName))", "Dotnet$NewName"))
    $rules.Add(@("\bpg-$([regex]::Escape($oldLower))\b", "pg-$newLower"))
}

function Convert-Name([string]$text) {
    foreach ($r in $rules) { $text = [regex]::Replace($text, $r[0], $r[1]) }
    return $text
}

function Get-ProjectItems {
    $stack = New-Object System.Collections.Stack
    $stack.Push((Get-Item -LiteralPath $root))
    while ($stack.Count -gt 0) {
        $dir = $stack.Pop()
        foreach ($item in Get-ChildItem -LiteralPath $dir.FullName -Force) {
            if ($item.PSIsContainer) {
                if ($excludeDirs -contains $item.Name) { continue }
                $item
                $stack.Push($item)
            } else { $item }
        }
    }
}

Write-Host "Renaming '$OldName' -> '$NewName'$(if ($All) { ' (ALL mode)' })" -ForegroundColor Cyan
$utf8NoBom = New-Object System.Text.UTF8Encoding($false)
$items = @(Get-ProjectItems)

# ---------- 1. replace file contents ----------
$changed = 0
foreach ($f in $items | Where-Object { -not $_.PSIsContainer }) {
    if ($skipFiles -contains $f.Name) { continue }
    if ($textExt -notcontains $f.Extension.ToLower()) { continue }
    $bytes = [IO.File]::ReadAllBytes($f.FullName)
    if ($bytes -contains [byte]0) { continue }   # binary
    $hasBom = $bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF
    $text = [IO.File]::ReadAllText($f.FullName, [Text.Encoding]::UTF8)
    $new = Convert-Name $text
    if ($new -cne $text) {
        $rel = $f.FullName.Substring($root.Length + 1)
        if ($PSCmdlet.ShouldProcess($rel, 'Update content')) {
            $enc = if ($hasBom) { New-Object System.Text.UTF8Encoding($true) } else { $utf8NoBom }
            [IO.File]::WriteAllText($f.FullName, $new, $enc)
        }
        Write-Host "  edit   $rel"
        $changed++
    }
}

# ---------- 2. rename files & folders (deepest first) ----------
$renamed = 0
$toRename = $items | Where-Object { $skipFiles -notcontains $_.Name -and (Convert-Name $_.Name) -cne $_.Name } |
    Sort-Object { $_.FullName.Length } -Descending
foreach ($it in $toRename) {
    $newItemName = Convert-Name $it.Name
    $rel = $it.FullName.Substring($root.Length + 1)
    if ($PSCmdlet.ShouldProcess($rel, "Rename to $newItemName")) {
        Rename-Item -LiteralPath $it.FullName -NewName $newItemName
    }
    Write-Host "  rename $rel -> $newItemName"
    $renamed++
}

# ---------- 3. clear stale build output ----------
foreach ($d in Get-ChildItem -Path (Join-Path $root 'src') -Directory -ErrorAction SilentlyContinue) {
    foreach ($sub in 'bin', 'obj') {
        $p = Join-Path $d.FullName $sub
        if ((Test-Path $p) -and $PSCmdlet.ShouldProcess($p, 'Remove build output')) {
            Remove-Item -LiteralPath $p -Recurse -Force
        }
    }
}

Write-Host "Done: $changed file(s) edited, $renamed item(s) renamed." -ForegroundColor Green
Write-Host "Next: dotnet build ; docker build -t $newLower-api:latest ."
if ($All) { Write-Host "Note: DB table renamed -> recreate the database tables (db/*.sql) when the DB is enabled." -ForegroundColor Yellow }
