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

$changelogPattern = '\s*\[\s*assembly\s*\:\s*AssemblyMetadata\s*\(\s*AssemblyMetadata\.ChangeLog\s*,\s*[@]?".*?"\s*\)\s*]'
$currentCommit = (git rev-parse HEAD).Trim()
if (-not $currentCommit) {
    Write-Warning "Unable to get current commit hash"
} else {
    $gitMessages = @()
    if ($lastProcessedCommit) {
        $rawLog = git log "$lastProcessedCommit..HEAD" --pretty=format:"%h: %B" 2>&1 | Out-String
        if ($LASTEXITCODE -ne 0) {
            Write-Warning "Git log failed: $rawLog"
            exit 1
        }
        $gitMessages = [regex]::Split($rawLog, '(?m)(?=^[a-f0-9]{7,}: )', [System.Text.RegularExpressions.RegexOptions]::Multiline) |
            ForEach-Object { $_.Trim() } |
            Where-Object { $_ -ne "" }
    } else {
        exit 1
    }

    if ($gitMessages.Count -eq 0) {
        Write-Host "No new commits since last version — changelog not updated."
    } else {
        $formattedCommits = $gitMessages | ForEach-Object { "`t`t[*] $_" }
        $logBody = "[h1]Update v$lastProcessedVersion to v$newVersionString[/h1]`r`n`t[list]`r`n" + ($formattedCommits -join "`r`n") + "`r`n`t[/list]"
        $logBody = $logBody.Replace('"', '""')
        $changelogText = "`r`n[assembly: AssemblyMetadata(AssemblyMetadata.ChangeLog, @""`r`n`t$logBody`r`n"")]"
        if ([regex]::IsMatch($content, $changelogPattern, [System.Text.RegularExpressions.RegexOptions]::Multiline) ) {
            $content = [regex]::Replace(
                $content,
                $changelogPattern,
                $changelogText,
                [System.Text.RegularExpressions.RegexOptions]::Multiline
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