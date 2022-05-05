namespace SimpleCommandLine;

public class RootCommand : SubCommand
{
    public RootCommand(string description, params SubCommand[] subcommands) : base(ExecutingFileName(), description, subcommands)
    {
    }

    public RootCommand(string description, Argument[] arguments, Option[] options, Func<SubCommand, Dictionary<string, string>, Dictionary<string, string>, Task<int>> executeAsync)
        : base(ExecutingFileName(), description, arguments, options, executeAsync)
    {
    }

    public RootCommand(string description, Argument[] arguments, Option[] options, Func<SubCommand, Dictionary<string, string>, Dictionary<string, string>, int> execute)
        : base(ExecutingFileName(), description, arguments, options, execute)
    {
    }

    public RootCommand(string description, Argument[] arguments, Func<SubCommand, Dictionary<string, string>, Dictionary<string, string>, int> execute)
        : base(ExecutingFileName(), description, arguments, execute)
    {
    }

    public RootCommand(string description, Func<SubCommand, Dictionary<string, string>, Dictionary<string, string>, int> execute)
        : base(ExecutingFileName(), description, execute)
    {
    }

    public Task<int> RunAsync(string[] args) => ExecuteAsync(args);

    private static string ExecutingFileName()
    {
        return Path.GetFileNameWithoutExtension(System.Environment.GetCommandLineArgs()[0]);
    }
}