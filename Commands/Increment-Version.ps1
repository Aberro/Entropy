# Increment-Version.ps1 v1.1.1

$assemblyInfoFile = "AssemblyInfo.cs"

# --- Version Section --- #
$fullPath = Resolve-Path $assemblyInfoFile
$parentFolder = Split-Path $fullPath -Parent
$folderName = Split-Path $parentFolder -Leaf

if ($folderName -ieq "Template" -or $folderName -ieq "GenerateAbout" -or $folderName -ieq "StationeersLibrary") {
    Write-Host "Versioning cancelled: $assemblyInfoFile is inside a 'Template' folder."
    exit 0
}

$content = Get-Content $fullPath -Raw

$versionPattern = 'AssemblyVersion\s*\(\s*"(\d+)\.(\d+)\.(\d+)\.(\d+)"\s*\)'

$major = [int]0
$minor = [int]0
$build = [int]0
$revision = [int]0

if ($content -notmatch $versionPattern) {
    Write-Warning "Version pattern not found in $assemblyInfoFile"
    exit 1
}

$major = [int]$matches[1]
$minor = [int]$matches[2]
$build = [int]$matches[3]
$revision = [int]$matches[4]
$revision++
$newVersionCode = "AssemblyVersion(""$major.$minor.$build.$revision"")"
$newVersionString = "$major.$minor.$build.$revision"

$content = [regex]::Replace($content, $versionPattern, {
    param($m) $newVersionCode
})

[System.IO.File]::WriteAllText($fullPath, $content, [System.Text.Encoding]::UTF8)
Write-Host "Version updated to $newVersionString"

# --- Changelog Section --- #
$commitCommentPattern = '//\s*Last processed commit:\s*([a-f0-9]{7,40})'
$lastProcessedCommit = $null

$versionCommentPattern = '//\s*Last processed version:\s*?([\d\.]+)'
$lastProcessedVersion = $null

if ($content -match $versionCommentPattern) {
    $lastProcessedVersion = $matches[1]
    Write-Host "Last processed version: $lastProcessedVersion"
}

if ($content -match $commitCommentPattern) {
    $lastProcessedCommit = $matches[1]
    Write-Host "Found last processed commit: $lastProcessedCommit"
} else {
    Write-Warning "Could not find last processed commit"
    exit 1
}

$changelogPattern = '\s*\[\s*assembly\s*\:\s*AssemblyMetadata\s*\(\s*"ChangeLog"\s*,\s*[@]?".*?"\s*\)\s*]'
$currentCommit = (git rev-parse HEAD).Trim()
if (-not $currentCommit) {
    Write-Warning "Unable to get current commit hash"
} else {
    $gitMessages = @()
    if ($lastProcessedCommit) {
        $gitMessages = git log "$lastProcessedCommit..HEAD" --pretty=format:"%s" 2>&1
    } else {
        exit 1
    }

    if ($LASTEXITCODE -ne 0) {
        Write-Warning "Git log failed: $gitMessages"
        exit 1
    } elseif ($gitMessages.Count -eq 0) {
        Write-Host "No new commits since last version — changelog not updated."
    } else {
        $formattedCommits = $gitMessages | ForEach-Object { "`t`t[*] $_" }
        $logBody = "[h1]Update v$lastProcessedVersion to v$newVersionString[/h1]`r`n`t[list]`r`n" + ($formattedCommits -join "`r`n") + "`r`n`t[/list]"
        $changelogText = "[assembly: AssemblyMetadata(""ChangeLog"", @""`r`n`t$logBody`r`n"")]"
        if ([regex]::IsMatch($content, $changelogPattern, [System.Text.RegularExpressions.RegexOptions]::Singleline) ) {
            $content = [regex]::Replace(
                $content,
                $changelogPattern,
                $changelogText,
                [System.Text.RegularExpressions.RegexOptions]::Singleline
            )
        } else {
            $content = "$content`r`n$changelogText"
        }

        $commitComment = "// Last processed commit: $currentCommit"
        if ($content -match $commitCommentPattern) {
            $content = [regex]::Replace($content, $commitCommentPattern, $commitComment)
        } else {
            $content += "`r`n$commitComment"
        }

        $versionComment = "// Last processed version: $newVersionString"

        if ($content -match $versionCommentPattern) {
            $content = [regex]::Replace($content, $versionCommentPattern, $versionComment)
        } else {
            $content += "`r`n$versionComment"
        }

        [System.IO.File]::WriteAllText($fullPath, $content, [System.Text.Encoding]::UTF8)
        Write-Host "ChangeLog updated with new commits."
    }
}