using SimpleCommandLine;

var sumCommand = new SubCommand(
    "sum",
    "Sums two numbers",
    new [] { new Argument("addend1", "First addend"), new Argument("addend2", "Second addend") },
    SumNumbers);

var multiplyCommand = new SubCommand(
    "mul",
    "Multiplies two numbers",
    new [] { new Argument("factor1", "First factor"), new Argument("factor2", "Second factor") },
    MultiplyNumbers);

var calculatorCommand = new RootCommand(
    "Does calculations",
    sumCommand,
    multiplyCommand);

return await calculatorCommand.RunAsync(args);

// Commands that work out of the box:
//
// Example.exe --help
// Example.exe sum --help
// Example.exe mul --help
// Example.exe sum 3 4
// Example.exe mul 7 8

int SumNumbers(SubCommand command, Dictionary<string, string> _, Dictionary<string, string> arguments)
{
    if (!int.TryParse(arguments["addend1"], out var addend1))
    {
        command.PrintErrorAndHelp("First addend must be a number.");
        return 1;
    }

    if (!int.TryParse(arguments["addend2"], out var addend2))
    {
        command.PrintErrorAndHelp("Second addend must be a number.");
        return 1;
    }

    Console.WriteLine(addend1 + addend2);
    return 0;
}

int MultiplyNumbers(SubCommand command, Dictionary<string, string> _, Dictionary<string, string> arguments)
{
    if (!int.TryParse(arguments["factor1"], out var factor1))
    {
        command.PrintErrorAndHelp("First factor must be a number.");
        return 1;
    }

    if (!int.TryParse(arguments["addend2"], out var factor2))
    {
        command.PrintErrorAndHelp("Second factor must be a number.");
        return 1;
    }

    Console.WriteLine(factor1 * factor2);
    return 0;
}