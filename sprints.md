# Sprint Planning: cwlogs

This document divides the development into several sprints.

## Sprint 1: Project Setup and AWS Integration
**Goal:** Running CLI base with AWS authentication.
- [x] Initialization of the .NET 10 project.
- [x] Integration of necessary NuGet packages (`AWSSDK.CloudWatchLogs`, `Spectre.Console.Cli`).
- [x] Implementation of AWS profile selection.
- [x] Basic structure of CLI commands (Command Pattern).

## Sprint 2: Exploration (Listing)
**Goal:** Listing of resources.
- [x] Implementation of `cwlogs groups`: Lists log groups in a table.
- [x] Implementation of `cwlogs streams <group>`: Lists log streams of a group.
- [x] Error handling for missing permissions or invalid regions.

## Sprint 3: Log Consumption (Fetch & Tail)
**Goal:** Display and live streaming of logs.
- [x] Implementation of `cwlogs fetch <group>`: One-time retrieval and output of logs.
- [x] Implementation of filtering by log streams.
- [x] Implementation of `cwlogs tail <group>`: Infinite loop for retrieving new events (polling).
- [x] Optimization of console output (colors, timestamps).

## Sprint 4: SSO Support and Credential Handling
**Goal:** Support for AWS SSO and improved credential loading.
- [x] Integration of `AWSSDK.SSO` and `AWSSDK.SSOOIDC`.
- [x] Switch to `DefaultAWSCredentialsIdentityResolver` for more robust authentication.
- [x] Fix for issues with environment variables like `AWS_PROFILE`.

## Sprint 5: Bugfixes and Robustness
**Goal:** Fix for runtime errors and improvement of stability.
- [x] Fix `System.InvalidOperationException: Could not find color or style '$LATEST'`.
- [x] Securing console output against special characters in log data (Markup Escaping).

## Sprint 6: Extended Features and Formatting
**Goal:** More flexible control of output and stream selection.
- [x] Tables without borders by default.
- [x] Stream selection by name or index (the latest `n` streams).
- [x] Parameter `--limit` for the number of entries.
- [x] Parameter `--sort` for the sort order.
- [x] Formatting options `--single-line` and `--raw`.

## Sprint 7: Log Cleanup and Usability
**Goal:** Improved handling of AWS-specific log prefixes.
- [x] Implementation of the `--clean` option to remove Lambda prefixes.
- [x] Consistent application of `--clean` in `fetch` and `tail`.

## Sprint 8: Refactoring and Clean Code
**Goal:** Reduction of code duplicates and improvement of maintainability.
- [x] Introduction of a base class `LogBaseCommand` for common functionality of `fetch` and `tail`.
- [x] Extraction of `ResolveStreams`, `PrintLogEvent`, and `CleanLambdaMessage`.
- [x] Refactoring of `FetchCommand` and `TailCommand` to use the base class.

## Sprint 9: Auto-Completion
**Goal:** Support for PowerShell auto-completion.
- [x] Implementation of a hidden `_complete` command for dynamic data.
- [x] Implementation of the `completion` command to generate the PowerShell script.
- [x] Support for commands, options, log groups, and log streams.
- [x] Documentation of the script installation.

## Sprint 10: Completion Refinement
**Goal:** More robust PowerShell completion script.
- [x] Improvement of positional argument detection (LogGroups).
- [x] Consideration of options in argument counting.
- [x] Stabilization of log stream completion.
- [x] Fix for `cwlogs fetch <TAB>` with empty completion prefix.
- [x] Added fallback logic for group completion.

## Sprint 11: Robust Resource Completion
Goal: Increasing the reliability of completion through explicit parameters.
- [x] Introduction of the `--group` option for all relevant commands.
- [x] Adjustment of command logic to prefer `--group`.
- [x] Updating the PowerShell completion script to use `--group`.
- [x] Improvement of log group detection in complex command lines.

## Sprint 12: Path Robustness of Completion
Goal: Ensuring completion works even if the tool is not in PATH.
- [x] Determination of the absolute path to the executable in `CompletionCommand`.
- [x] Use of the absolute path within the generated PowerShell script.
- [x] Registration of the completer for filenames with and without `.exe` extension.

## Sprint 13: Production Build (Single Binary)
Goal: Creation of a standalone executable without .NET dependencies.
- [x] Configuration of the project for `SelfContained` and `PublishSingleFile`.
- [x] Deactivation of trimming to ensure compatibility with Spectre.Console.Cli.
- [x] Verification of the single-file build.
- [x] Documentation of build parameters.

## Sprint 14: Release Distribution
Goal: Automation of storing the finished binary.
- [x] Configuration of `PublishDir` in the project file for release builds.
- [x] Ensuring the `dist` folder is used.
- [x] Verification of publish output.

## Sprint 15: Completion Prioritization
Goal: Improving UX through targeted completion of options.
- [x] Prioritization of options in completion logic if the prefix is `-` or `--`.
- [x] Suppression of dynamic resource suggestions (groups/streams) during option input.

## Sprint 16: Dynamic Completion Metadata
Goal: Increasing maintainability through dynamic loading of commands and options.
- [x] Implementation of `_complete --type metadata` to output command and option information via reflection.
- [x] Adjustment of `CompletionCommand` to use dynamic metadata in the PowerShell script.
- [x] Dynamic detection of options with values for more robust argument counting.

## Sprint 17: Centralization of Constants
Goal: Improving maintainability by avoiding string duplicates.
- [x] Introduction of the `CommandNames` class for command name constants.
- [x] Refactoring of `Program.cs`, `CompleteCommand.cs`, and `CompletionCommand.cs` to use the constants.

## Sprint 18: Project Documentation
Goal: Creation of a professional README for GitHub.
- [x] Creation of the `README.md` in English.
- [x] Documentation of features, installation, and usage.

## Sprint 19: Licensing
Goal: Adding the MIT license.
- [x] Creation of the `LICENSE` file.
- [x] Check reference in `README.md`.

## Sprint 20: Multi-Shell Completion
Goal: Support for Bash and preparation for further shells.
- [x] Refactoring `CompletionCommand` to support multiple shells.
- [x] Implementation of Bash completion logic.
- [x] Updating `README.md` for Bash support.
- [x] Adjustment of requirements.
- [x] Removal of PowerShell default in the completion command.

## Sprint 21: Translation and CI Readiness
Goal: Internationalization and CI/CD optimization.
- [x] Translation of all components (code, help, docs) to English.
- [x] Implementation of the `--no-color` option for CI/CD compatibility.
- [x] Refactoring: Moved `NoColor` logic to a `CommandInterceptor` for better separation of concerns (Clean Code).

## Sprint 22: Advanced Log Processing
Goal: Improved log format support and filtering.
- [x] Implementation of the `--json` option for structured log output.
- [x] Implementation of the `--since` parameter for time-based filtering (e.g., '1h', '30m', '1d', or specific date).
- [x] Integration of advanced filtering into `fetch` and `tail`.
- [x] Update documentation (README, Requirements).

## Sprint 24: Code Refactoring (Clean Code)
Goal: Slimming down base classes and commands by extracting logic into utilities.
- [x] Extraction of time parsing, JSON processing, and Lambda cleaning to utility classes in `cwlogs/util`.
- [x] Extraction of metadata reflection logic to `ReflectionUtils`.
- [x] Outsourcing of shell completion script generation to `cwlogs/shell`.
- [x] Refactoring of commands to use the new utility structure.
- [x] Verification of build and functionality.

## Sprint 25: Automated Testing
Goal: Implementing a robust test suite without external dependencies.
- [x] Setup of the `cwlogs.Tests` project with xUnit, Moq, and FluentAssertions.
- [x] Implementation of unit tests for all utility classes (`TimeUtils`, `JsonUtils`, `LambdaUtils`).
- [x] Refactoring of the command structure to support better testability (Interface-based client creation).
- [x] Implementation of unit tests for the core logic of `GroupsCommand`, `StreamsCommand`, and `FetchCommand`.
- [x] Implementation of unit tests for the completion system (`ReflectionUtils`, `CompleteCommand`).
- [x] Verification of test coverage and development workflow.

## Sprint 26: Native AOT Compilation
Goal: Enabling high-performance, native compilation for faster startup and smaller deployment.
- [x] Configuration of `PublishAot` in `cwlogs.csproj`.
- [x] Implementation of JSON Source Generation (`AppJsonContext`) for log exports.
- [x] Fix for AOT-specific reflection issues in `ReflectionUtils`.
- [x] Manual registration of AWS SSO OIDC dependencies for AOT compatibility.
- [x] Replacement of reflection-based exception formatting with AOT-compatible alternatives.
- [x] Documentation of AOT build requirements (C++ build tools).

## Sprint 27: Error Handling and UX
Goal: Improving user experience by providing cleaner error messages.
- [x] Introduction of a central `ErrorHandler` for consistent exception management.
- [x] Suppression of full stack traces for `AmazonClientException` in release builds.
- [x] Improved error visibility in `CompleteCommand`.

## Sprint 28: AOT Warning Reduction
Goal: Minimizing warnings during Native AOT compilation.
- [x] Addition of `DynamicallyAccessedMembers` attributes to `ReflectionUtils`.
- [x] Use of `DynamicDependency` in `Program.cs` to preserve types for Spectre.Console.Cli.
- [x] Suppression of unavoidable AOT/Trimming warnings with clear justification.
- [x] Verification of build and functionality after AOT optimizations.

## Sprint 29: UX Refinement and Autocomplete Fixes
Goal: Improving default behavior and fixing auto-completion.
- [x] Changed default sort order to `asc` so latest logs appear at the end.
- [x] Fixed auto-completion to include inherited options (e.g., `--limit`, `--sort`, `--no-color`).
- [x] Improved AOT compatibility for option extraction in `ReflectionUtils`.
- [x] Added unit tests for default sort order and extended option detection.

## Sprint 30: Native AOT Completion Fixes
Goal: Ensuring full auto-completion support in Native AOT binaries.
- [x] Fixed issue where `CommandOptionAttribute` properties were trimmed in AOT.
- [x] Added `DynamicDependency` for `CommandOptionAttribute` in `Program.cs`.
- [x] Updated shell completion generators to correctly handle `--no-color` as a boolean flag.
- [x] Verified metadata output in a real Native AOT build.

## Sprint 31: Testability of Autocomplete
Goal: Enabling autocomplete testing without live AWS access.
- [x] Implementation of a dummy mode in `CompleteCommand.cs` via environment variable `CWLOGS_DUMMY_COMPLETION`.
- [x] Updating `PowerShellCompletionGenerator.cs` to correctly handle log group completion and avoid suggestions after value-consuming options.
- [x] Updating `tests\PowerShellCompletionTests.ps1` to include tests for log group and log stream completion using dummy data.
- [x] Verification of the complete PowerShell completion logic using simulated tests.

## Sprint 32: English-only transition
Goal: Remove remaining German text from codebase and tests.
- [x] Translated comments in `PowerShellCompletionGenerator.cs` to English.
- [x] Translated `Write-Host` messages and test output in `PowerShellCompletionTests.ps1` to English.
- [x] Verified that no other German text remains in the core codebase.

## Sprint 33: Open Source & GitHub Actions
Goal: Prepare the project for GitHub and automate releases.
- [x] Added GitHub Actions workflow for CI (Tests) and CD (Native AOT Releases for Win/Linux).
- [x] Updated `README.md` with CI badges and download links.
- [x] Updated `requirements.md` to reflect CI/CD and Native AOT status.
- [x] Verified project is ready for MIT licensed open source publication.
## Sprint 34: Rolling Pre-releases
Goal: Enable "latest" pre-releases from the main branch.
- [x] Modified GitHub Actions workflow to trigger releases on `main` branch pushes.
- [x] Configured `softprops/action-gh-release` to update a `latest` pre-release tag.
- [x] Updated `README.md` and `requirements.md` to document pre-release availability.

## Sprint 35: Branch Renaming
Goal: Rename `master` branch to `main`.
- [x] Updated GitHub Actions workflow to use `main` as the only primary branch.
- [x] Removed all references to `master` in the codebase and documentation.
- [x] Renamed local branch from `master` to `main`.

## Sprint 36: Repository Finalization
Goal: Update project documentation with the final repository URL.
- [x] Replaced `your-username` placeholders with `mcnilz` in `README.md`.

## Sprint 37: AI Generation Notice
Goal: Transparently disclose that the project is AI-generated.
- [x] Added a note in the upper section of `README.md` regarding AI generation.

## Sprint 38: Fix CI/CD Release Permissions
Goal: Resolve 403 error during GitHub Release.
- [x] Added explicit `contents: write` permissions to the `github-release` job in `ci-cd.yml`.

## Sprint 39: Multi-Platform Support (macOS & Linux ARM64)
Goal: Provide Native AOT binaries for macOS and Linux ARM64.
- [x] Added `macos-latest` and `linux-arm64` to the test and release matrix in `ci-cd.yml`.
- [x] Configured build for `osx-x64`, `osx-arm64`, and `linux-arm64` RIDs.
- [x] Generalized binary renaming logic to support multiple architectures and platforms.
- [x] Updated GitHub Release step to include all new artifacts.
- [x] Updated documentation (`README.md`, `requirements.md`) to reflect expanded platform support.

## Sprint 40: CI/CD Robustness
Goal: Improve CI/CD stability and resolve throttling issues.
- [x] Added `concurrency` control to `ci-cd.yml` to prevent parallel release updates.
- [x] Restricted the number of parallel jobs (set to 1) to prevent rate limiting and save resources.
- [x] Enabled `fail_on_unmatched_files` in the release step for better error detection.

## Sprint 41: Platform-Independent Build
Goal: Ensure builds in CI use the correct platform-specific Runtime Identifier.
- [x] Removed hardcoded `win-x64` RuntimeIdentifier from `cwlogs.csproj` to allow environment-based builds in CI.

## Sprint 42: Fix Linux ARM64 Native AOT Build
Goal: Resolve linker and objcopy errors during cross-compilation for Linux ARM64.
- [x] Added `gcc-aarch64-linux-gnu` and `binutils-aarch64-linux-gnu` dependencies for the `linux-arm64` build in GitHub Actions.
- [x] Disabled `StripSymbols` for `linux-arm64` to avoid `objcopy` format recognition errors on the x64 host.

## Sprint 43: Conditional Test Execution in CI/CD
Goal: Optimize CI/CD by optionally skipping tests for pre-releases.
- [x] Implemented a `prepare` job to evaluate the `SKIP_TESTS_ON_PRE_RELEASE` variable and provide it as an output, enabling job-level skipping of the `test` job while maintaining the configuration within the workflow file.
- [x] Fixed `Publish Native AOT` step by using shell-agnostic GitHub Actions expressions instead of platform-dependent shell scripts.
- [x] Corrected CI/CD matrix logic to ensure all five target platforms (including `linux-x64`) are built and released correctly by using explicit `include` entries.
- [x] Consolidated and simplified binary renaming logic in the workflow.
