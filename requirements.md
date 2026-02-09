# Requirements: AWS CloudWatch Logs CLI Tool (cwlogs)

This document describes the requirements for the `cwlogs` CLI tool.

## 1. Objectives
A simple command-line tool (CLI) to search, list, and live-tail AWS CloudWatch Logs.

## 2. Functional Requirements

### 2.1 AWS Authentication
- The tool must automatically retrieve AWS credentials from standard sources (environment variables, AWS CLI profiles in `~/.aws/credentials`).
- Support for profiles via an optional parameter (e.g., `--profile`).

### 2.2 List Log Groups
- Command to list all available log groups in the configured region.

### 2.3 List Log Streams
- Command to list log streams within a specific log group.

### 2.4 Display Logs
- Command to output log entries of a log group.
- Optional filter for a specific log stream.
- Formatted output on the console.

### 2.5 Tail Functionality
- Continuous retrieval of new log entries ("Live Tail").
- Similar to `tail -f`.

### 2.6 Global Options
- `--no-color`: Disables ANSI color sequences in the output, which is helpful for CI/CD pipelines and log file redirection.

## 3. Non-functional Requirements
- **Technology:** .NET 10 (C#).
- **Usability:** Intuitive commands, help texts.
- **Performance:** Efficient paging when retrieving large amounts of logs.

## 4. Planned Commands (Proposal)
- `cwlogs groups` - Lists log groups.
- `cwlogs streams [<group-name>] [-g|--group <group-name>]` - Lists streams of a group.
- `cwlogs fetch [<group-name>] [-g|--group <group-name>] [-s|--stream <name|number>] [--limit <n>] [--sort <asc|desc>] [--since <t>] [--single-line] [--raw] [--json] [--clean]` - Displays logs.
- `cwlogs tail [<group-name>] [-g|--group <group-name>] [-s|--stream <name|number>] [--limit <n>] [--since <t>] [--single-line] [--raw] [--json] [--clean]` - Follows logs live.

## 5. Extended Requirements (Sprint 6 & 22)
- **Table Design:** Spectre.Console tables should have no border by default.
- **Stream Selection:** In `fetch` and `tail`, a stream can be selected by name or index (number). A number `n` selects the `n` latest streams.
- **Limit:** Parameter `--limit` to limit the number of retrieved log entries.
- **Sorting:** Parameter `--sort` to control the sort order (`asc` or `desc`). Default is `asc`.
- **Time Filtering:** Parameter `--since` to retrieve logs from a certain point in time (e.g., '1h', '30m', '1d').
- **Formatting:**
    - `--single-line`: Log entries are output on a single line (removes newlines within an entry).
    - `--raw`: Only the log text itself is output, without timestamps or other tool prefixes.
    - `--json`: Log entries are output as structured JSON objects. If the message itself is a valid JSON, it is embedded as an object/array instead of a double-encoded string.
    - `--clean`: Removes AWS-specific prefixes (e.g., from Lambda: Timestamp, RequestID, Log Level) from the message.

## 6. Robust Completion (Sprint 11)
- Introduction of the `--group` option for `streams`, `fetch`, and `tail`.
- Completion prioritizes `--group` for identifying the log group.
- Positional arguments remain as aliases but are handled more robustly in completion logic.
- If input starts with `-` or `--`, only options are completed (avoiding LogGroup/Stream suggestions).
- **Dynamic Metadata:** Completion logic retrieves commands and options dynamically via an internal call (`_complete --type metadata`) instead of hardcoding them in the script.

## 7. Auto-Completion (Sprint 9 & 20)
- Support for PowerShell 7 and Bash.
- Completion of commands and options.
- Dynamic completion of log group names.
- Dynamic completion of log stream names when using the `--stream` option.
- Generation of the completion script via the command `cwlogs completion <powershell|bash>`. The shell must be explicitly specified.

## 9. Deployment (Sprint 13, 14 & 33)
- The tool should be distributable as a standalone binary (Single Binary).
- Goal: No dependency on a pre-installed .NET Runtime on the target system (Self-Contained).
- Optimization: Single-file executable for easy handling.
- **Native AOT:** The tool is compiled as a Native AOT binary for maximum performance and minimal startup time.
- **Release Directory:** For a production build (Release), the finished binary should automatically be placed in a `dist` folder in the project directory.
- **CI/CD Pipeline:** GitHub Actions automatically run tests and create releases with Native AOT binaries for Windows (x64), Linux (x64, ARM64), and macOS (x64, ARM64).
    - Pushes to `main` update the `latest` pre-release.
    - Creating a version tag (e.g., `v1.0.0`) creates a stable release.
    - Tests can be optionally skipped for pre-releases via the `SKIP_TESTS_ON_PRE_RELEASE` environment variable in the workflow.

## 10. Code Quality (Sprint 17 & 25)
- Centralization of command names in a dedicated `CommandNames` class to avoid string duplication.
- **Automated Testing:** Implementation of a comprehensive test suite using xUnit, Moq, and FluentAssertions.
    - Unit tests for utility classes (Time, JSON, Lambda cleaning, Reflection).
    - Unit tests for command logic using mocked AWS SDK clients, including the internal completion command.
    - Adherence to the Test Pyramid (focus on unit and integration tests without external dependencies).
