using cwlogs.Base;

namespace cwlogs.shell;

public static class BashCompletionGenerator
{
    public static string Generate(string exePath, string exeName)
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
                # In our case almost all options have values except --raw, --clean, --single-line, --no-color
                if [[ $i -gt 2 ]]; then
                    local p=${{COMP_WORDS[i-1]}}
                    if [[ ""$p"" == ""--raw"" || ""$p"" == ""--clean"" || ""$p"" == ""--single-line"" || ""$p"" == ""--no-color"" ]]; then
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
}
