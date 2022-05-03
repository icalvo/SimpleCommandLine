namespace Scheduler.CommandLine;

public class Option
{
    private readonly string[] _aliases;

    public Option(string name, string description, params string[] aliases)
    {
        _aliases = aliases;
        Name = name;
        Description = description;
    }

    public string Name { get; }
    public string Description { get; }

    public IEnumerable<string> NameAndAliases => new []{ Name }.Concat(_aliases);
    public bool Matches(string candidate)
    {
        return candidate == Name || _aliases.Contains(candidate);
    }
}
