using cwlogs.Base;
using cwlogs.settings;
using JetBrains.Annotations;
using Spectre.Console;
using Spectre.Console.Cli;

namespace cwlogs.command;

[UsedImplicitly]
public class CompletionCommand : AsyncCommand<CompletionSettings>
{
    public override Task<int> ExecuteAsync(CommandContext context, CompletionSettings settings, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(settings.Shell))
        {
            AnsiConsole.MarkupLine("[red]Error: Please specify a shell (powershell, bash).[/]");
            AnsiConsole.WriteLine("Example: cwlogs completion powershell");
            return Task.FromResult(1);
        }

        var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName ?? "cwlogs";
        var exeName = System.IO.Path.GetFileNameWithoutExtension(exePath);

        if (settings.Shell.Equals("bash", StringComparison.OrdinalIgnoreCase))
        {
            AnsiConsole.WriteLine(GenerateBashScript(exePath, exeName));
        }
        else if (settings.Shell.Equals("powershell", StringComparison.OrdinalIgnoreCase))
        {
            AnsiConsole.WriteLine(GeneratePowerShellScript(exePath, exeName));
        }
        else
        {
            AnsiConsole.MarkupLine($"[red]Error: Unsupported shell '{settings.Shell}'. Supported are: powershell, bash.[/]");
            return Task.FromResult(1);
        }

        return Task.FromResult(0);
    }

    private string GenerateBashScript(string exePath, string exeName)
    {
        return $@"
_{exeName}_completions()
{{
    local cur prev opts
    COMPREPLY=()
    cur=""${{COMP_WORDS[COMP_CWORD]}}""
    prev=""${{COMP_WORDS[COMP_CWORD-1]}}""

    # Load dynamic metadata
    local metadata=$( '{exePath}' {CommandNames.CompleteInternal} --type metadata )
    local commands=()
    
    while IFS= read -r line; do
        if [[ $line =~ ^COMMAND:([^:]+):(.*)$ ]]; then
            commands+=(""${{BASH_REMATCH[1]}}"")
        fi
    done <<< ""$metadata""

    if [[ ${{#commands[@]}} -eq 0 ]]; then
        commands=('{CommandNames.Groups}' '{CommandNames.Streams}' '{CommandNames.Fetch}' '{CommandNames.Tail}' '{CommandNames.Completion}')
    fi

    # Detect current subcommand
    local subcmd=""""
    if [[ $COMP_CWORD -gt 1 ]]; then
        subcmd=""${{COMP_WORDS[1]}}""
    fi

    # 1. Completion of commands
    if [[ $COMP_CWORD -eq 1 ]]; then
        COMPREPLY=( $(compgen -W ""${{commands[*]}}"" -- ""$cur"") )
        return 0
    fi

    # 2. Completion of options
    if [[ ""$cur"" == -* ]]; then
        local cmd_opts=""""
        while IFS= read -r line; do
            if [[ $line =~ ^COMMAND:$subcmd:(.*)$ ]]; then
                cmd_opts=${{BASH_REMATCH[1]//,/ }}
                break
            fi
        done <<< ""$metadata""
        
        local all_opts=""-p --profile -r --region -h --help $cmd_opts""
        COMPREPLY=( $(compgen -W ""$all_opts"" -- ""$cur"") )
        return 0
    fi

    # 3. Completion of LogGroups
    if [[ ""$subcmd"" =~ ^({CommandNames.Streams}|{CommandNames.Fetch}|{CommandNames.Tail})$ ]]; then
        # If prev is --group or -g, or we are at the first positional argument
        if [[ ""$prev"" == ""--group"" || ""$prev"" == ""-g"" ]]; then
             local groups=$( '{exePath}' {CommandNames.CompleteInternal} --type {CommandNames.Groups} )
             COMPREPLY=( $(compgen -W ""$groups"" -- ""$cur"") )
             return 0
        fi

        # Simple check for first positional argument (very rough for Bash)
        local pos_args=0
        for (( i=2; i < COMP_CWORD; i++ )); do
            if [[ ! ""${{COMP_WORDS[i]}}"" == -* ]]; then
                # Check if previous word was an option that expects a value
                # In our case almost all options have values except --raw, --clean, --single-line
                if [[ $i -gt 2 ]]; then
                    local p=${{COMP_WORDS[i-1]}}
                    if [[ ""$p"" == ""--raw"" || ""$p"" == ""--clean"" || ""$p"" == ""--single-line"" ]]; then
                         ((pos_args++))
                    fi
                else
                     ((pos_args++))
                fi
            fi
        done

        if [[ $pos_args -eq 0 && ! ""$cur"" == -* ]]; then
             local groups=$( '{exePath}' {CommandNames.CompleteInternal} --type {CommandNames.Groups} )
             COMPREPLY=( $(compgen -W ""$groups"" -- ""$cur"") )
             return 0
        fi
    fi

    # 4. Completion of LogStreams
    if [[ ""$prev"" == ""--stream"" || ""$prev"" == ""-s"" ]]; then
        # Search for group
        local group=""""
        for (( i=2; i < COMP_CWORD; i++ )); do
             if [[ ""${{COMP_WORDS[i]}}"" == ""--group"" || ""${{COMP_WORDS[i]}}"" == ""-g"" ]]; then
                 group=""${{COMP_WORDS[i+1]}}""
                 break
             fi
        done
        
        if [[ -z ""$group"" ]]; then
            # Take the first positional argument
            for (( i=2; i < COMP_CWORD; i++ )); do
                if [[ ! ""${{COMP_WORDS[i]}}"" == -* ]]; then
                    group=""${{COMP_WORDS[i]}}""
                    break
                fi
            done
        fi

        if [[ -n ""$group"" ]]; then
            local streams=$( '{exePath}' {CommandNames.CompleteInternal} --type {CommandNames.Streams} --groups ""$group"" )
            COMPREPLY=( $(compgen -W ""$streams"" -- ""$cur"") )
            return 0
        fi
    fi

    return 0
}}

complete -F _{exeName}_completions {exeName}
".Trim();
    }

    private string GeneratePowerShellScript(string exePath, string exeName)
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
            # In diesem Tool haben fast alle Optionen Werte außer --single-line, --raw, --clean
            $cmdOptions | Where-Object {{ $_ -notmatch '^(--single-line|--raw|--clean)$' }} | ForEach-Object {{ $allOptionsWithValues += $_ }}
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

    # Fall 4: Vervollstaendigung von LogGroups nach --group oder -g
    if ($prevWord -eq '--group' -or $prevWord -eq '-g') {{
        $groups = & '{exePath}' {CommandNames.CompleteInternal} --type {CommandNames.Groups}
        if ($groups) {{
            return $groups | Where-Object {{ $_ -like ""$wordToComplete*"" }} | ForEach-Object {{ [System.Management.Automation.CompletionResult]::new($_, $_, 'ParameterValue', $_) }}
        }}
    }}

}}

Register-ArgumentCompleter -Native -CommandName '{exeName}' -ScriptBlock $cwlogsCompleter
Register-ArgumentCompleter -Native -CommandName '{exeName}.exe' -ScriptBlock $cwlogsCompleter
".Trim();
    }
}
