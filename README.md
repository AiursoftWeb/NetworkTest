# Aiursoft NetworkTest

<a name="networktest"></a>
[![MIT licensed](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/AiursoftWeb/networktest/blob/master/LICENSE)
[![Pipeline stat](https://badge-proxy.aiursoft.com/aiursoft/networkTest/badges/master/pipeline.svg)](https://gitlab.aiursoft.com/aiursoft/networkTest/-/pipelines)
[![Test Coverage](https://badge-proxy.aiursoft.com/aiursoft/networkTest/badges/master/coverage.svg)](https://gitlab.aiursoft.com/aiursoft/networkTest/-/pipelines)
[![S360 compliance](https://s360.aiursoft.com/badge.svg?project=aiursoft%2Fnetworktest)](https://s360.aiursoft.com/SupplyChain?tab=projects&project=aiursoft%2Fnetworktest&scope=baseline&ecosystem=all)
[![NuGet version (Aiursoft.NetworkTest)](https://img.shields.io/nuget/v/Aiursoft.NetworkTest.svg)](https://www.nuget.org/packages/Aiursoft.NetworkTest/)
[![Man hours](https://manhours.aiursoft.com/r/github.com/aiursoftweb/networkTest.svg)](https://manhours.aiursoft.com/r/github.com/aiursoftweb/networkTest.html)

NetworkTest is a tool testing current machine's network quality.

## Overview

### What is NetworkTest?

Run network quality checks from the current machine, including latency, IP connectivity, NAT traversal and game-network reliability.

## Installation

Install the CLI with the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0):

```bash
dotnet tool install --global Aiursoft.NetworkTest
```

Update an existing installation:

```bash
dotnet tool update --global Aiursoft.NetworkTest
```

Remove the CLI:

```bash
dotnet tool uninstall --global Aiursoft.NetworkTest
```

## Usage

Run the command to see available options:

```bash
network-test --help
```

After getting the binary, run it directly in the terminal.

```bash
$ network-test quality all
```

That's it!

## Configuration

Configuration is supplied through command-line options; no additional application settings file is required. Run `network-test --help` and `network-test quality --help` to discover the command tree.

The registered quality checks are `china-products`, `global-products`, `ip-connectivity`, `nat-traversal`, `game-reliability` and `all`. Read each command’s `--help` before selecting targets or changing test parameters. The checks make real network requests; results depend on connectivity and the selected endpoints.

## Development

Install the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).

From the repository root, restore and build:

```bash
dotnet restore Aiursoft.NetworkTest.sln
dotnet build Aiursoft.NetworkTest.sln --no-restore
```

<a name="run-locally"></a>
<a name="local-run"></a>
Run the tool from source, with the prerequisites and configuration above:

```bash
dotnet run --project src/Aiursoft.NetworkTest/Aiursoft.NetworkTest.csproj -- --help
```

<a name="run-in-microsoft-visual-studio"></a>
In Microsoft Visual Studio, open `Aiursoft.NetworkTest.sln`, select `Aiursoft.NetworkTest` as the startup project and press `F5`. Configure the arguments and environment for the task you want to run.

Run the existing tests:

```bash
dotnet test Aiursoft.NetworkTest.sln --no-restore
```


## How to contribute

There are many ways to contribute to the project: logging bugs, submitting pull requests, reporting issues, and creating suggestions.

Even if you have push rights on the repository, you should create a personal fork and create feature branches there when you need them. This keeps the main repository clean and your workflow cruft out of sight.

We're also interested in your feedback on the future of this project. You can submit a suggestion or feature request through the issue tracker. To make this process more effective, we're asking that these include more information to help define them more clearly.

## License

Licensed under the [MIT License](https://github.com/AiursoftWeb/networktest/blob/master/LICENSE).
