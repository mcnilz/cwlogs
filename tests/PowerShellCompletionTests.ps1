param(
    [string]$exePath = "cwlogs\bin\Debug\net10.0\win-x64\cwlogs.exe"
)

$exePath = Resolve-Path $exePath
$exeName = [System.IO.Path]::GetFileNameWithoutExtension($exePath)

Write-Host "Setting dummy mode..."
$env:CWLOGS_DUMMY_COMPLETION = "1"

Write-Host "Loading completion script..."
$scriptContent = & $exePath completion powershell
$scriptString = $scriptContent -join "`n"
Invoke-Expression $scriptString

function Test-Completion {
    param(
        [string]$CommandLine,
        [string]$ExpectedMatch
    )

    $ast = [System.Management.Automation.Language.Parser]::ParseInput($CommandLine, [ref]$null, [ref]$null)
    $elements = $ast.FindAll({ $args[0] -is [System.Management.Automation.Language.CommandAst] }, $true)
    
    if (-not $elements) {
        Write-Host "FAILURE: Kein CommandAst gefunden für '$CommandLine'" -ForegroundColor Red
        return $false
    }
    
    $commandAst = $elements[-1]
    
    if ($CommandLine.EndsWith(" ")) {
        $wordToComplete = ""
        $cursorPosition = $CommandLine.Length
    } else {
        $parts = $CommandLine.Split(" ", [System.StringSplitOptions]::RemoveEmptyEntries)
        $wordToComplete = $parts[-1]
        $cursorPosition = $CommandLine.Length
    }
    
    # Simulate AST having one more element if wordToComplete is empty (PowerShell parser behavior)
    if ($wordToComplete -eq "") {
         $CommandLine += "X"
         $ast = [System.Management.Automation.Language.Parser]::ParseInput($CommandLine, [ref]$null, [ref]$null)
         $elements = $ast.FindAll({ $args[0] -is [System.Management.Automation.Language.CommandAst] }, $true)
         $commandAst = $elements[-1]
         # Remove the simulated word 'X' from elements
         $commandElements = @()
         for($i=0; $i -lt $commandAst.CommandElements.Count - 1; $i++) {
            $commandElements += $commandAst.CommandElements[$i]
         }
         # Mocking the CommandElements property is hard, let's just create a custom object for commandAst
         $commandAst = [PSCustomObject]@{
             CommandElements = $commandElements
         }
    }

    # Debug
    Write-Host "Checking: '$CommandLine' (wordToComplete: '$wordToComplete')" -NoNewline

    $results = &$cwlogsCompleter -wordToComplete $wordToComplete -commandAst $commandAst -cursorPosition $cursorPosition
    
    $matches = $results | Where-Object { $_.CompletionText -like "$ExpectedMatch*" }
    
    if ($matches) {
        Write-Host " -> SUCCESS" -ForegroundColor Green
        return $true
    } else {
        Write-Host " -> FAILURE" -ForegroundColor Red
        Write-Host "Available completions: $(($results.CompletionText) -join ', ')" -ForegroundColor Yellow
        Write-Host "Metadata returned: $metadata" -ForegroundColor Gray
        return $false
    }
}

$success = $true

Write-Host "`n--- Subcommands ---"
$success = $success -and (Test-Completion "cwlogs " "groups")
$success = $success -and (Test-Completion "cwlogs g" "groups")

Write-Host "`n--- Global Options ---"
$success = $success -and (Test-Completion "cwlogs groups -" "--profile")
$success = $success -and (Test-Completion "cwlogs groups --pr" "--profile")

Write-Host "`n--- Command-specific Options ---"
$success = $success -and (Test-Completion "cwlogs fetch -" "--stream")
$success = $success -and (Test-Completion "cwlogs fetch --str" "--stream")

Write-Host "`n--- Log Groups / Streams ---"
$success = $success -and (Test-Completion "cwlogs fetch " "test-group-pwsh")
# Log Streams via --group option (more robust in simulation)
$success = $success -and (Test-Completion "cwlogs fetch --group dummy-group-2 --stream " "stream-1-for-dummy-group-2")

if ($success) {
    Write-Host "`nAll tests SUCCESSFUL!" -ForegroundColor Green
    exit 0
} else {
    Write-Host "`nSome tests FAILED!" -ForegroundColor Red
    exit 1
}
