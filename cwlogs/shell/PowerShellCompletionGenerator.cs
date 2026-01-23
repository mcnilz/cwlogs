using cwlogs.Base;

namespace cwlogs.shell;

public static class PowerShellCompletionGenerator
{
    public static string Generate(string exePath, string exeName)
    {
        return $@"
$cwlogsCompleter = {{
    param($wordToComplete, $commandAst, $cursorPosition)

    $commandElements = $commandAst.CommandElements
    # Das erste Element ist das ausfuehrbare Programm (cwlogs)
    
    $globalOptions = @('--profile', '-p', '--region', '-r', '--help', '-h')
    
    # Dynamische Metadaten laden
    $metadata = & '{exePath}' {CommandNames.CompleteInternal} --type metadata
    $commandMap = @{{}}
    $commands = @()
    $allOptionsWithValues = @()
    foreach ($line in $metadata) {{
        if ($line -match '^COMMAND:(.+):(.*)$') {{
            $cmdName = $Matches[1]
            $cmdOptions = $Matches[2] -split ',' | Where-Object {{ $_ }}
            $commandMap[$cmdName] = $cmdOptions
            $commands += $cmdName
            # Heuristik: Optionen mit Werten (alle außer Boolean-Schalter)
            # In diesem Tool haben fast alle Optionen Werte außer --single-line, --raw, --clean, --no-color
            $cmdOptions | Where-Object {{ $_ -notmatch '^(--single-line|--raw|--clean|--no-color)$' }} | ForEach-Object {{ $allOptionsWithValues += $_ }}
        }}
    }}
    if ($commands.Count -eq 0) {{
        $commands = @('{CommandNames.Groups}', '{CommandNames.Streams}', '{CommandNames.Fetch}', '{CommandNames.Tail}', '{CommandNames.Completion}')
    }}
    $allOptionsWithValues = $allOptionsWithValues | Select-Object -Unique
    $optionsWithValuesRegex = '^(' + (($allOptionsWithValues | ForEach-Object {{ [regex]::Escape($_) }}) -join '|') + ')$'

    # Fall 1: Vervollstaendigung des Unterbefehls (z.B. cwlogs <TAB>)
    if ($commandElements.Count -le 1) {{
        return $commands | Where-Object {{ $_ -like ""$wordToComplete*"" }} | ForEach-Object {{ [System.Management.Automation.CompletionResult]::new($_, $_, 'Command', $_) }}
    }}

    $currentCommand = $commandElements[1].Value.Trim('""', ""'"")
    if ($commandElements.Count -eq 2 -and $wordToComplete -and !($commandMap.ContainsKey($currentCommand))) {{
        return $commands | Where-Object {{ $_ -like ""$wordToComplete*"" }} | ForEach-Object {{ [System.Management.Automation.CompletionResult]::new($_, $_, 'Command', $_) }}
    }}
    
    # Wenn wir gerade erst den Befehl getippt haben und TAB druecken
    if ($commandElements.Count -eq 2 -and !$wordToComplete) {{
         if ($currentCommand -match '^({CommandNames.Streams}|{CommandNames.Fetch}|{CommandNames.Tail})$') {{
             $groups = & '{exePath}' {CommandNames.CompleteInternal} --type {CommandNames.Groups}
             if ($groups) {{
                return $groups | ForEach-Object {{ [System.Management.Automation.CompletionResult]::new($_, $_, 'ParameterValue', $_) }}
             }}
         }}
    }}
    
    # Hilfsfunktion zum Finden des Index eines Parameters
    function Get-ParameterValue {{
        param($name)
        for ($i = 1; $i -lt $commandElements.Count; $i++) {{
            if ($commandElements[$i].Value -eq $name -and ($i + 1) -lt $commandElements.Count) {{
                return $commandElements[$i+1].Value
            }}
        }}
        return $null
    }}

    # Fall 5: Vervollstaendigung von Optionen
    $allOptions = $globalOptions
    if ($commandMap.ContainsKey($currentCommand)) {{
        $allOptions += $commandMap[$currentCommand]
    }}
    $allOptions = $allOptions | Select-Object -Unique
    
    if ($wordToComplete -like '-*') {{
        $results = $allOptions | Where-Object {{ $_ -like ""$wordToComplete*"" }} | ForEach-Object {{ [System.Management.Automation.CompletionResult]::new($_, $_, 'ParameterName', $_) }}
        if ($results) {{ return $results }}
    }}

    # Fall 2: Vervollstaendigung von LogGroups (erstes Argument nach streams/fetch/tail)
    if ($currentCommand -match '^({CommandNames.Streams}|{CommandNames.Fetch}|{CommandNames.Tail})$') {{
        # Zaehle wie viele Positionsargumente (keine Optionen) vor dem aktuellen Wort kommen
        $positionalArgsCount = 0
        for ($i = 2; $i -lt ($commandElements.Count - ($wordToComplete ? 1 : 0)); $i++) {{
            $val = $commandElements[$i].Value
            if ($val -like '-*') {{
                if ($val -match $optionsWithValuesRegex) {{
                    $i++
                }}
                continue
            }}
            $positionalArgsCount++
        }}

        if ($positionalArgsCount -eq 0) {{
             $groups = & '{exePath}' {CommandNames.CompleteInternal} --type {CommandNames.Groups}
             if ($groups) {{
                return $groups | Where-Object {{ $_ -like ""$wordToComplete*"" }} | ForEach-Object {{ [System.Management.Automation.CompletionResult]::new($_, $_, 'ParameterValue', $_) }}
             }}
        }}
    }}

    # Fall 3: Vervollstaendigung von LogStreams (nach --stream oder -s)
    $prevWord = $null
    if ($wordToComplete) {{
        $prevWord = $commandElements[$commandElements.Count - 2].Value
    }} else {{
        $prevWord = $commandElements[$commandElements.Count - 1].Value
    }}

    if ($prevWord -eq '--stream' -or $prevWord -eq '-s') {{
        $group = $null
        # Suche erst nach --group / -g
        $group = Get-ParameterValue '--group'
        if (!$group) {{ $group = Get-ParameterValue '-g' }}

        if (!$group) {{
            # Suche das erste Positionsargument (die LogGroup)
            for ($i = 2; $i -lt ($commandElements.Count - ($wordToComplete ? 1 : 0)); $i++) {{
                $val = $commandElements[$i].Value
                # Pruefe ob es eine Option ist
                if ($val -like '-*') {{
                    # Wenn es eine Option mit Wert ist, ueberspringe auch den Wert (grob geschaetzt)
                    if ($val -match $optionsWithValuesRegex) {{
                        $i++
                    }}
                    continue
                }}
                $group = $val
                break
            }}
        }}

        if ($group) {{
            $streams = & '{exePath}' {CommandNames.CompleteInternal} --type {CommandNames.Streams} --groups $group
            if ($streams) {{
                return $streams | Where-Object {{ $_ -like ""$wordToComplete*"" }} | ForEach-Object {{ [System.Management.Automation.CompletionResult]::new($_, $_, 'ParameterValue', $_) }}
            }}
        }}
    }}

    # Fall 4: Vervollstaendigung von Shells fuer completion
    if ($currentCommand -eq '{CommandNames.Completion}') {{
        return @('powershell', 'bash') | Where-Object {{ $_ -like ""$wordToComplete*"" }} | ForEach-Object {{ [System.Management.Automation.CompletionResult]::new($_, $_, 'ParameterValue', $_) }}
    }}

    return $null
}}

Register-ArgumentCompleter -CommandName '{exeName}' -ScriptBlock $cwlogsCompleter
Register-ArgumentCompleter -CommandName '{exeName}.exe' -ScriptBlock $cwlogsCompleter
".Trim();
    }
}
