using cwlogs.Base;

namespace cwlogs.shell;

public static class PowerShellCompletionGenerator
{
    public static string Generate(string exePath, string exeName)
    {
        exePath = exePath.Trim().Replace("\r", "").Replace("\n", "");
        exeName = exeName.Trim().Replace("\r", "").Replace("\n", "");
        var internalCmd = CommandNames.CompleteInternal.Trim();

        var template = @"
$cwlogsCompleter = {
    param($wordToComplete, $commandAst, $cursorPosition)
    $exe = '[[EXE_PATH]]'
    $internal = '[[INTERNAL]]'
    $commandElements = $commandAst.CommandElements
    $metadata = & ""$exe"" $internal --type metadata
    $commandMap = @{}
    $commands = @()
    $optionsWithValues = @()
    $globalOptions = @()
    foreach ($line in $metadata) {
        if ($line -match '^COMMAND:([^:]+):(.*)$') {
            $cmdName = $Matches[1]
            $optionsRaw = $Matches[2] -split ',' | Where-Object { $_ }
            $cmdOptions = @()
            foreach ($optRaw in $optionsRaw) {
                $parts = $optRaw -split ':'
                $optName = $parts[0]
                $optType = $parts[1]
                if ($cmdName -eq 'GLOBAL') { $globalOptions += $optName }
                else { $cmdOptions += $optName }
                if ($optType -eq 'VALUE') { $optionsWithValues += $optName }
            }
            if ($cmdName -ne 'GLOBAL') {
                $commandMap[$cmdName] = $cmdOptions
                $commands += $cmdName
            }
        }
    }
    if ($commands.Count -eq 0) { $commands = @('groups', 'streams', 'fetch', 'tail', 'completion') }
    if ($commandElements.Count -le 1 -or ($commandElements.Count -eq 2 -and $wordToComplete)) {
        return $commands | Where-Object { $_ -like ""$wordToComplete*"" } | ForEach-Object { [System.Management.Automation.CompletionResult]::new($_, $_, 'Command', $_) }
    }
    $currentCommand = $commandElements[1].Value
    if ($currentCommand) { $currentCommand = $currentCommand.Trim('""', ""'"") }
    if ($wordToComplete -like '-*') {
        $allOptions = $globalOptions
        if ($commandMap.ContainsKey($currentCommand)) { $allOptions += $commandMap[$currentCommand] }
        $allOptions = $allOptions | Select-Object -Unique
        return $allOptions | Where-Object { $_ -like ""$wordToComplete*"" } | ForEach-Object { [System.Management.Automation.CompletionResult]::new($_, $_, 'ParameterName', $_) }
    }

    # Case 2: Completion of LogGroups
    if ($currentCommand -match '^(streams|fetch|tail)$') {
        # Check if we are potentially completing a LogGroup
        $isOptionValue = $false
        $prev = $null
        if ($wordToComplete) { $prev = $commandElements[$commandElements.Count - 2].Value }
        else { $prev = $commandElements[$commandElements.Count - 1].Value }

        if ($optionsWithValues -contains $prev) { $isOptionValue = $true }

        if (-not $isOptionValue) {
            $groups = & ""$exe"" $internal --type groups
            if ($groups) {
                return $groups | Where-Object { $_ -like ""$wordToComplete*"" } | ForEach-Object { [System.Management.Automation.CompletionResult]::new($_, $_, 'ParameterValue', $_) }
            }
        }
    }

    # Case 3: Completion of LogStreams (after --stream or -s)
    $prevWord = $null
    if ($wordToComplete) { $prevWord = $commandElements[$commandElements.Count - 2].Value }
    else { $prevWord = $commandElements[$commandElements.Count - 1].Value }

    if ($prevWord -eq '--stream' -or $prevWord -eq '-s') {
        # Search for LogGroup (first positional argument or --group)
        $group = $null
        for ($i = 2; $i -lt $commandElements.Count; $i++) {
            $val = $commandElements[$i].Value
            if ($val -eq '--group' -or $val -eq '-g') {
                if ($i + 1 -lt $commandElements.Count) { $group = $commandElements[$i+1].Value }
                break
            }
        }
        if (!$group) {
            for ($i = 2; $i -lt $commandElements.Count; $i++) {
                $val = $commandElements[$i].Value
                if ($val -notlike '-*') { $group = $val; break }
            }
        }
        if ($group) {
            $streams = & ""$exe"" $internal --type streams --groups $group
            if ($streams) {
                return $streams | Where-Object { $_ -like ""$wordToComplete*"" } | ForEach-Object { [System.Management.Automation.CompletionResult]::new($_, $_, 'ParameterValue', $_) }
            }
        }
    }

    # Case 4: Completion of Shells for completion
    if ($currentCommand -eq 'completion') {
        return @('powershell', 'bash') | Where-Object { $_ -like ""$wordToComplete*"" } | ForEach-Object { [System.Management.Automation.CompletionResult]::new($_, $_, 'ParameterValue', $_) }
    }

    return $null
}
Register-ArgumentCompleter -CommandName '[[EXE_NAME]]' -ScriptBlock $cwlogsCompleter
Register-ArgumentCompleter -CommandName '[[EXE_NAME]].exe' -ScriptBlock $cwlogsCompleter
";
        return template
            .Replace("[[EXE_PATH]]", exePath)
            .Replace("[[EXE_NAME]]", exeName)
            .Replace("[[INTERNAL]]", internalCmd)
            .Trim();
    }
}
