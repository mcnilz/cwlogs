# cwlogs - AWS CloudWatch Logs CLI

`cwlogs` is a lightweight, user-friendly Command Line Interface (CLI) tool for browsing, searching, and tailing AWS CloudWatch Logs. It is built with .NET 10 and designed for speed and ease of use, featuring full PowerShell 7 auto-completion.

## Features

- **AWS SSO Support:** Automatically resolves credentials from environment variables, AWS CLI profiles, and SSO sessions.
- **Log Exploration:** List Log Groups and Log Streams with ease.
- **Log Fetching:** Retrieve log events with flexible filtering and sorting.
- **Live Tailing:** Follow logs in real-time (similar to `tail -f`).
- **Flexible Formatting:** Options for raw output, single-line logs, and cleaning AWS Lambda metadata.
- **Shell Integration:** Dynamic tab-completion for PowerShell 7 and Bash (commands, options, log groups, and log streams).
- **Self-Contained:** Can be built as a single, portable executable without requiring a local .NET runtime.

## Installation

### Build from Source

To build `cwlogs` yourself, you need the [.NET 10 SDK](https://dotnet.microsoft.com/download).

1. Clone the repository:
   ```powershell
   git clone https://github.com/your-username/cwlogs.git
   cd cwlogs
   ```

2. Build and publish a single-file executable for Windows:
   ```powershell
   dotnet publish cwlogs\cwlogs.csproj -c Release
   ```
   The resulting binary will be placed in the `dist/` directory.

## Usage

### Commands

- `cwlogs groups`: Lists all available Log Groups.
- `cwlogs streams <group-name>`: Lists Log Streams within a specific Log Group.
- `cwlogs fetch <group-name>`: Fetches and displays log events from a group.
- `cwlogs tail <group-name>`: Continously polls and displays new log events.
- `cwlogs completion <powershell|bash>`: Generates the completion script for the specified shell.

### Global Options

- `-p|--profile <PROFILE>`: Specify the AWS profile to use.
- `-r|--region <REGION>`: Specify the AWS region.

### Fetch & Tail Options

- `-g|--group <GROUP>`: Explicitly specify the Log Group name.
- `-s|--stream <STREAM>`: Filter by Log Stream name or use a number `n` to select the `n` most recent streams.
- `-l|--limit <LIMIT>`: Maximum number of log entries to retrieve (Default: 50).
- `--sort <asc|desc>`: Sort order (only for `fetch`, default is `desc`).
- `--single-line`: Collapses multi-line log entries into a single line.
- `--raw`: Outputs only the log message without timestamps or colors.
- `--clean`: Removes AWS Lambda-specific metadata (timestamps, request IDs) from the message.

### Examples

```powershell
# List log groups
cwlogs groups

# Fetch the last 20 entries from a specific stream, cleaning Lambda headers
cwlogs fetch /aws/lambda/my-function -s my-stream-name -l 20 --clean

# Tail the 3 most recent streams of a group in raw format
cwlogs tail /aws/lambda/my-function -s 3 --raw
```

## Auto-Completion

`cwlogs` provides dynamic auto-completion for PowerShell 7 and Bash.

### PowerShell 7
To enable it, add the following line to your PowerShell profile (usually `$PROFILE`):

```powershell
cwlogs completion powershell | Out-String | Invoke-Expression
```

### Bash
To enable it, source the completion script in your `.bashrc`:

```bash
source <(cwlogs completion bash)
```

Once enabled, you can use `TAB` to complete command names, options, and even AWS resources like Log Groups and Log Streams.

## License

This project is licensed under the MIT License.
