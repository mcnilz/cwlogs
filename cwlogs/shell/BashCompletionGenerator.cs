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
    local global_opts=()
    local value_opts=()
    
    while IFS= read -r line; do
        if [[ $line =~ ^COMMAND:([^:]+):(.*)$ ]]; then
            local cmd_name=""${{BASH_REMATCH[1]}}""
            local opts_raw=""${{BASH_REMATCH[2]}}""
            
            if [[ ""$cmd_name"" == ""GLOBAL"" ]]; then
                IFS=',' read -ra opts_arr <<< ""$opts_raw""
                for opt_info in ""${{opts_arr[@]}}""; do
                    local opt_name=""${{opt_info%%:*}}""
                    global_opts+=(""$opt_name"")
                    if [[ ""$opt_info"" == *"":VALUE"" ]]; then
                        value_opts+=(""$opt_name"")
                    fi
                done
            else
                commands+=(""$cmd_name"")
                IFS=',' read -ra opts_arr <<< ""$opts_raw""
                for opt_info in ""${{opts_arr[@]}}""; do
                    if [[ ""$opt_info"" == *"":VALUE"" ]]; then
                        value_opts+=(""${{opt_info%%:*}}"")
                    fi
                done
            fi
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
        local cmd_opts=()
        while IFS= read -r line; do
            if [[ $line =~ ^COMMAND:$subcmd:(.*)$ ]]; then
                local opts_raw=""${{BASH_REMATCH[1]}}""
                IFS=',' read -ra opts_arr <<< ""$opts_raw""
                for opt_info in ""${{opts_arr[@]}}""; do
                    cmd_opts+=(""${{opt_info%%:*}}"")
                done
                break
            fi
        done <<< ""$metadata""
        
        local all_opts=""${{global_opts[*]}} ${{cmd_opts[*]}}""
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

        # Simple check for first positional argument
        local pos_args=0
        local skip_next=0
        for (( i=2; i < COMP_CWORD; i++ )); do
            if [[ $skip_next -eq 1 ]]; then
                skip_next=0
                continue
            fi
            
            local word=""${{COMP_WORDS[i]}}""
            if [[ ""$word"" == -* ]]; then
                # Check if this option expects a value
                for val_opt in ""${{value_opts[@]}}""; do
                    if [[ ""$word"" == ""$val_opt"" ]]; then
                        skip_next=1
                        break
                    fi
                done
            else
                ((pos_args++))
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
        local skip_next=0
        for (( i=2; i < COMP_CWORD; i++ )); do
            if [[ $skip_next -eq 1 ]]; then
                skip_next=0
                continue
            fi
            
            local word=""${{COMP_WORDS[i]}}""
            if [[ ""$word"" == ""--group"" || ""$word"" == ""-g"" ]]; then
                group=""${{COMP_WORDS[i+1]}}""
                break
            fi
            
            if [[ ""$word"" == -* ]]; then
                for val_opt in ""${{value_opts[@]}}""; do
                    if [[ ""$word"" == ""$val_opt"" ]]; then
                        skip_next=1
                        break
                    fi
                done
            fi
        done
        
        if [[ -z ""$group"" ]]; then
            # Take the first positional argument
            skip_next=0
            for (( i=2; i < COMP_CWORD; i++ )); do
                if [[ $skip_next -eq 1 ]]; then
                    skip_next=0
                    continue
                fi
                
                local word=""${{COMP_WORDS[i]}}""
                if [[ ! ""$word"" == -* ]]; then
                    group=""$word""
                    break
                fi
                
                for val_opt in ""${{value_opts[@]}}""; do
                    if [[ ""$word"" == ""$val_opt"" ]]; then
                        skip_next=1
                        break
                    fi
                done
            done
        fi

        if [[ -n ""$group"" ]]; then
            local streams=$( '{exePath}' {CommandNames.CompleteInternal} --type {CommandNames.Streams} --groups ""$group"" )
            COMPREPLY=( $(compgen -W ""$streams"" -- ""$cur"") )
            return 0
        fi
    fi

    # 5. Completion for completion command shells
    if [[ ""$subcmd"" == ""{CommandNames.Completion}"" ]]; then
        COMPREPLY=( $(compgen -W ""powershell bash"" -- ""$cur"") )
        return 0
    fi

    return 0
}}

complete -F _{exeName}_completions {exeName}
".Trim();
    }
}
