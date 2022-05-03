# 🆕 Simple Command Line

## ❓ What is My Project?

This is a command line parser. Unlike other cool alternatives, this is a very simple one, based on a simple idea:

* No reflection or IL injection.

I want the code of this parser to remain very simple.

## ⚡ Getting Started

You can create a command tree by using the many constructors of `Command` class. When you have your tree built, just invoke your root command with:

```csharp
await rootCommand.ExecuteAsync(args);
```

This will parse the command line, locating the appropriate subcommand and creating two dictionaries (one for arguments and the other for options). There are several ways this process can fail:

* Subcommand not provided
* Subcommand not found
* Argument not provided
* Option not found

Otherwise, once the command to be executed is located and the dictionaries are built, it will invoke the `execute` function of that command, passing the dictionaries and the command itself to that function.

## 🔧 Building and Running

Just run `build.sh` or `build.cmd`. The build system is based on NUKE, the build project is `build/_build.csproj`.

### 🔨 Build the Project


### ▶ Running and Settings

## 🤝 Collaborate with My Project
