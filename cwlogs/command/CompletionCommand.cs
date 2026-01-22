using cwlogs.settings;
using JetBrains.Annotations;
using Spectre.Console;
using Spectre.Console.Cli;

namespace cwlogs.command;

[UsedImplicitly]
public class CompletionCommand : AsyncCommand<GlobalSettings>
{
    public override Task<int> ExecuteAsync(CommandContext context, GlobalSettings settings, CancellationToken cancellationToken)
    {
        var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName ?? "cwlogs";
        var exeName = System.IO.Path.GetFileNameWithoutExtension(exePath);
        
        var script = $@"
$cwlogsCompleter = {{
    param($wordToComplete, $commandAst, $cursorPosition)

    $commandElements = $commandAst.CommandElements
    # Das erste Element ist das ausfuehrbare Programm (cwlogs)
    
    $commands = @('groups', 'streams', 'fetch', 'tail', 'completion')
    $globalOptions = @('--profile', '-p', '--region', '-r', '--help', '-h')
    $groupOptions = @('--group', '-g')
    $fetchTailOptions = @('--limit', '-l', '--sort', '--single-line', '--raw', '--clean', '--stream', '-s')

    # Fall 1: Vervollstaendigung des Unterbefehls (z.B. cwlogs <TAB>)
    if ($commandElements.Count -le 1) {{
        return $commands | Where-Object {{ $_ -like ""$wordToComplete*"" }} | ForEach-Object {{ [System.Management.Automation.CompletionResult]::new($_, $_, 'Command', $_) }}
    }}

    $currentCommand = $commandElements[1].Value.Trim('""', ""'"")
    if ($commandElements.Count -eq 2 -and $wordToComplete -and !($currentCommand -match '^(streams|fetch|tail)$')) {{
        return $commands | Where-Object {{ $_ -like ""$wordToComplete*"" }} | ForEach-Object {{ [System.Management.Automation.CompletionResult]::new($_, $_, 'Command', $_) }}
    }}
    
    # Wenn wir gerade erst den Befehl getippt haben und TAB druecken
    if ($commandElements.Count -eq 2 -and !$wordToComplete) {{
         if ($currentCommand -match '^(streams|fetch|tail)$') {{
             $groups = & '{exePath}' _complete --type groups
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
    if ($currentCommand -match '^(streams|fetch|tail)$') {{
        $allOptions += $groupOptions
    }}
    if ($currentCommand -match '^(fetch|tail)$') {{
        $allOptions += $fetchTailOptions
    }}
    
    if ($wordToComplete -like '-*') {{
        $results = $allOptions | Where-Object {{ $_ -like ""$wordToComplete*"" }} | ForEach-Object {{ [System.Management.Automation.CompletionResult]::new($_, $_, 'ParameterName', $_) }}
        if ($results) {{ return $results }}
    }}

    # Fall 2: Vervollstaendigung von LogGroups (erstes Argument nach streams/fetch/tail)
    if ($currentCommand -match '^(streams|fetch|tail)$') {{
        # Zaehle wie viele Positionsargumente (keine Optionen) vor dem aktuellen Wort kommen
        $positionalArgsCount = 0
        for ($i = 2; $i -lt ($commandElements.Count - ($wordToComplete ? 1 : 0)); $i++) {{
            $val = $commandElements[$i].Value
            if ($val -like '-*') {{
                if ($val -match '^(-p|--profile|-r|--region|-l|--limit|--sort|-g|--group)$') {{
                    $i++
                }}
                continue
            }}
            $positionalArgsCount++
        }}

        if ($positionalArgsCount -eq 0) {{
             $groups = & '{exePath}' _complete --type groups
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
                    if ($val -match '^(-p|--profile|-r|--region|-l|--limit|--sort|-g|--group)$') {{
                        $i++
                    }}
                    continue
                }}
                $group = $val
                break
            }}
        }}
        
        if ($group) {{
            $streams = & '{exePath}' _complete --type streams --groups $group
            if ($streams) {{
                return $streams | Where-Object {{ $_ -like ""$wordToComplete*"" }} | ForEach-Object {{ [System.Management.Automation.CompletionResult]::new($_, $_, 'ParameterValue', $_) }}
            }}
        }}
    }}

    # Fall 4: Vervollstaendigung von LogGroups nach --group oder -g
    if ($prevWord -eq '--group' -or $prevWord -eq '-g') {{
        $groups = & '{exePath}' _complete --type groups
        if ($groups) {{
            return $groups | Where-Object {{ $_ -like ""$wordToComplete*"" }} | ForEach-Object {{ [System.Management.Automation.CompletionResult]::new($_, $_, 'ParameterValue', $_) }}
        }}
    }}

}}

Register-ArgumentCompleter -Native -CommandName '{exeName}' -ScriptBlock $cwlogsCompleter
Register-ArgumentCompleter -Native -CommandName '{exeName}.exe' -ScriptBlock $cwlogsCompleter
";
        AnsiConsole.WriteLine(script.Trim());
        return Task.FromResult(0);
    }
}
