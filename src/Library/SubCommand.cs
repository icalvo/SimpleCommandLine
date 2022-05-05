namespace SimpleCommandLine;

public class SubCommand
{
    public static int InvalidNumberOfArgumentsReturnCode { get; set; }
    public static int CommandNotProvidedReturnCode { get; set; } 
    public static int UnknownCommandReturnCode { get; set; } 
    public static int AmbiguousCommandReturnCode { get; set; } 

    private readonly string _name;
    private readonly Argument[] _arguments;
    private readonly Option[] _options;
    private readonly SubCommand[] _subcommands;
    private readonly Func<SubCommand, Dictionary<string, string>, Dictionary<string, string>, Task<int>> _properExecuteAsync;
    private readonly string _description;
    private SubCommand? _parent;

    static SubCommand()
    {
        InvalidNumberOfArgumentsReturnCode = 1;
        CommandNotProvidedReturnCode = 2;
        UnknownCommandReturnCode = 3;
        AmbiguousCommandReturnCode = 4;
    }

    private SubCommand(
        string name,
        string description,
        Argument[] arguments,
        Option[] options,
        SubCommand[] subcommands,
        Func<SubCommand, Dictionary<string, string>, Dictionary<string, string>, Task<int>>? properExecuteAsync)
    {
        _name = name;
        _description = description;
        _arguments = arguments;
        _options = options.Append(new Option("--help", "Shows help", "-h", "-?")).ToArray();
        _subcommands = subcommands;
        _properExecuteAsync = properExecuteAsync ?? ((_, _, _) => Task.FromResult(0)) ;
        _ = subcommands.ToDictionary(x => x._name);

        foreach (var command in subcommands)
        {
            command._parent = this;
        }
    }

    public SubCommand(
        string name,
        string description,
        params SubCommand[] subcommands)
        : this (name, description, Array.Empty<Argument>(), Array.Empty<Option>(), subcommands, null)
    {
        
    }

    public SubCommand(
        string name,
        string description,
        Argument[] arguments,
        Option[] options,
        Func<SubCommand, Dictionary<string, string>, Dictionary<string, string>, Task<int>> executeAsync)
        : this (name, description, arguments, options, Array.Empty<SubCommand>(), executeAsync)
    {}

    public SubCommand(
        string name,
        string description,
        Argument[] arguments,
        Option[] options,
        Func<SubCommand, Dictionary<string, string>, Dictionary<string, string>, int> execute)
        : this (name, description, arguments, options, Array.Empty<SubCommand>(), (c, o, a) => Task.FromResult(execute(c, o, a)))
    {}


    public SubCommand(
        string name,
        string description,
        Argument[] arguments,
        Func<SubCommand, Dictionary<string, string>, Dictionary<string, string>, int> execute)
        : this (name, description, arguments, Array.Empty<Option>(), Array.Empty<SubCommand>(), (c, o, a) => Task.FromResult(execute(c, o, a)))
    {}

    public SubCommand(
        string name,
        string description,
        Func<SubCommand, Dictionary<string, string>, Dictionary<string, string>, int> execute)
        : this (name, description, Array.Empty<Argument>(), Array.Empty<Option>(), Array.Empty<SubCommand>(), (c, o, a) => Task.FromResult(execute(c, o, a)))
    {}

    private string Usage()
    {
        var optionsUsage = _options.Length == 0 ? "" : " [options]";
        var argumentsUsage = _arguments.Length == 0 ? "" : " " + _arguments.Select(x => x.Name).StringJoin(" ");
        var commandsUsage = _subcommands.Length == 0 ? "" : " command";
        return $"{_name}{optionsUsage}{argumentsUsage}{commandsUsage}";        
    }

    private string Help
    {
        get
        {
            string? FormatSection(string title, params string[] lines) =>
                lines.Length == 0 ? null : title + ":\n" + lines.Select(l => $"  {l}").StringJoin("\n");

            static IEnumerable<string> FormatDefinitions(IEnumerable<(string Name, string Description)> definitions)
            {
                var definitionsArray = definitions as (string Name, string Description)[] ?? definitions.ToArray();
                return definitionsArray
                    .Select(c => $"{c.Name.PadRight(definitionsArray.Max(x => x.Name.Length))}  {c.Description}");
            }

            var breadcrumb = this.FollowLinks(c => c._parent).Reverse().Select(c => c._name).StringJoin(" ");
            var completeUsage = $"{breadcrumb} {Usage()}";
            var arguments = FormatDefinitions(_arguments.Select(c => (c.Name, c.Description)));
            var options = FormatDefinitions(_options.Select(c =>
            {
                var namesAndAliases = c.NameAndAliases.StringJoin(", ");
                return (namesAndAliases, c.Description);
            }));
            var commands = FormatDefinitions(_subcommands.Select(c => (c.Usage(), Description: c._description)));

            return new[]
                {
                    FormatSection("Description", _description),
                    FormatSection("Usage", completeUsage),
                    FormatSection("Arguments", arguments.ToArray()),
                    FormatSection("Options", options.ToArray()),
                    FormatSection("Commands", commands.ToArray())
                }
                .Where(x => x != null)
                .StringJoin("\n\n");
        }
    }

    public abstract class ParseResult
    {
        
    }

    public class ParseSuccess : ParseResult
    {
        public SubCommand Command { get; }
        public Dictionary<string, string> Options { get; }
        public Dictionary<string, string> Arguments { get; }

        public ParseSuccess(SubCommand command, Dictionary<string,string> options, Dictionary<string,string> arguments)
        {
            Command = command;
            Options = options;
            Arguments = arguments;
            ;
        }
    }

    public class ParseFailure : ParseResult
    {
        public SubCommand Command { get; }
        public string Message { get; }
        public int ErrorCode { get; }

        public ParseFailure(SubCommand command, string message, int errorCode)
        {
            Command = command;
            Message = message;
            ErrorCode = errorCode;
        }
    }
    internal ParseResult Parse(string[] args)
    {
        using var tokenCursor = args.AsEnumerable().GetEnumerator();
        var tokenExists = tokenCursor.MoveNext();
        var options = new Dictionary<string, string>();
        var arguments = new Dictionary<string, string>();

        while (tokenExists)
        {
            var foundOption = _options.FirstOrDefault(x => x.Matches(tokenCursor.Current));
            if (foundOption == null)
            {
                break;
            }

            options.Add(foundOption.Name, tokenCursor.Current);
            tokenExists = tokenCursor.MoveNext();
        }

        using var argumentCursor = _arguments.AsEnumerable().GetEnumerator();
        var argumentExists = argumentCursor.MoveNext();
        while (tokenExists && argumentExists)
        {
            arguments.Add(argumentCursor.Current.Name, tokenCursor.Current);
            argumentExists = argumentCursor.MoveNext();
            tokenExists = tokenCursor.MoveNext();
        }
        
        if (options.ContainsKey("--help"))
        {
            return new ParseSuccess(this, options, arguments);
        }

        if (arguments.Count != _arguments.Length)
        {
            return new ParseFailure(this, "Invalid number of arguments.", InvalidNumberOfArgumentsReturnCode);
        }

        if (_subcommands.Length == 0)
        {
            return new ParseSuccess(this, options, arguments);
        }
        
        if (args.Length == 0)
        {
            return new ParseFailure(this, "Required command was not provided.", CommandNotProvidedReturnCode);
        }

        var matchingCommands = _subcommands.Where(x => x._name.StartsWith(args[0])).ToArray();

        if (matchingCommands.Length == 0)
        {
            return new ParseFailure(this, "Unrecognized command", UnknownCommandReturnCode);
        }

        if (matchingCommands.Length > 1)
        {
            return new ParseFailure(this, "Ambiguous command, could be one of:" + matchingCommands.OrderBy(x => x._name).StringJoin(", "), AmbiguousCommandReturnCode);
        }

        var command = matchingCommands.Single();

        return command.Parse(args[1..]);
    }

    internal async Task<int> ExecuteAsync(string[] args)
    {
        var parseResult = Parse(args);
        switch (parseResult)
        {
            case ParseSuccess success:
            {
                var options = success.Options;
                var arguments = success.Arguments;
                var command = success.Command;

                if (options.ContainsKey("--help"))
                {
                    Console.WriteLine(command.Help);
                    return 0;
                }

                return await command._properExecuteAsync(command, options, arguments);
            }
            case ParseFailure failure:
                failure.Command.PrintErrorAndHelp(failure.Message);
                return failure.ErrorCode;
            default:
                throw new Exception("Unreachable code");
        }
    }

    internal async Task<int> OldExecuteAsync(string[] args)
    {
        using var tokenCursor = args.AsEnumerable().GetEnumerator();
        var tokenExists = tokenCursor.MoveNext();
        var options = new Dictionary<string, string>();
        var arguments = new Dictionary<string, string>();

        while (tokenExists)
        {
            var foundOption = _options.FirstOrDefault(x => x.Matches(tokenCursor.Current));
            if (foundOption == null)
            {
                break;
            }

            options.Add(foundOption.Name, tokenCursor.Current);
            tokenExists = tokenCursor.MoveNext();
        }

        using var argumentCursor = _arguments.AsEnumerable().GetEnumerator();
        var argumentExists = argumentCursor.MoveNext();
        while (tokenExists && argumentExists)
        {
            arguments.Add(argumentCursor.Current.Name, tokenCursor.Current);
            argumentExists = argumentCursor.MoveNext();
            tokenExists = tokenCursor.MoveNext();
        }
        
        if (options.ContainsKey("--help"))
        {
            Console.WriteLine(Help);
            return 0;
        }

        if (arguments.Count != _arguments.Length)
        {
            PrintErrorAndHelp("Invalid number of arguments.");
            return InvalidNumberOfArgumentsReturnCode;
        }

        if (_subcommands.Length == 0)
        {
            return await _properExecuteAsync(this, options, arguments);
        }
        
        if (args.Length == 0)
        {
            Console.WriteLine("Required command was not provided.");
            Console.WriteLine(Help);
            return CommandNotProvidedReturnCode;
        }

        var matchingCommands = _subcommands.Where(x => x._name.StartsWith(args[0])).ToArray();

        if (matchingCommands.Length == 0)
        {
            Console.WriteLine("Unrecognized command");
            Console.WriteLine(Help);

            return UnknownCommandReturnCode;
        }

        if (matchingCommands.Length > 1)
        {
            Console.WriteLine("Ambiguous command, you have to use more letters so that one of the following is unambiguously chosen:");
            foreach (SubCommand c in matchingCommands.OrderBy(x => x._name))
            {
                Console.WriteLine(c._name);
            }

            return AmbiguousCommandReturnCode;
        }

        var command = matchingCommands.Single();

        return await command.ExecuteAsync(args[1..]);
    }
    
    public void PrintErrorAndHelp(string errorMessage)
    {
        Console.WriteLine(errorMessage);
        Console.WriteLine(Help);
    }
}