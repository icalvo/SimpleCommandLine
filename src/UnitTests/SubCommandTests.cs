using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace SimpleCommandLine.Tests;

public class SubCommandTests
{
    [Fact]
    public void Parse_Simple()
    {
        var command = new SubCommand("sum", "Sum numbers",
            new Argument[] { new("addend1", "Addend 1"), new("addend2", "Addend 2") },
            (cmd, opts, args) => 0);

        var result = command.Parse("3", "4");

        result.Should().BeOfType<SubCommand.ParseSuccess>().Which
            .Arguments.Should().Equal(
                new KeyValuePair<string, string>("addend1", "3"),
                new KeyValuePair<string, string>("addend2", "4"));
    }
    
    [Fact]
    public void Parse_NotEnoughArguments()
    {
        var command = new SubCommand("sum", "Sum numbers",
            new Argument[] { new("addend1", "Addend 1"), new("addend2", "Addend 2") },
            (cmd, opts, args) => 0);

        var result = command.Parse("3");

        result.Should().BeOfType<SubCommand.ParseFailure>().Which
            .ErrorCode.Should().Be(SubCommand.InvalidNumberOfArgumentsReturnCode);
    }
    
    
    [Fact]
    public void Parse_UnknownCommand()
    {
        
        var commands = Enumerable.Range(1, 10).Select(i => new SubCommand($"sum{i}", "Sum numbers",
            new Argument[] { new("addend1", "Addend 1"), new("addend2", "Addend 2") },
            (cmd, opts, args) => 0)).ToArray();

        var command = new SubCommand("all", "All commands", commands);
        var result = command.Parse("other");

        result.Should().BeOfType<SubCommand.ParseFailure>().Which
            .ErrorCode.Should().Be(SubCommand.UnknownCommandReturnCode);
    }
    
    [Fact]
    public void Parse_CommandNotProvided()
    {
        
        var commands = Enumerable.Range(1, 10).Select(i => new SubCommand($"sum{i}", "Sum numbers",
            new Argument[] { new("addend1", "Addend 1"), new("addend2", "Addend 2") },
            (cmd, opts, args) => 0)).ToArray();

        var command = new SubCommand("all", "All commands", commands);
        var result = command.Parse();

        result.Should().BeOfType<SubCommand.ParseFailure>().Which
            .ErrorCode.Should().Be(SubCommand.CommandNotProvidedReturnCode);
    }

    [Fact]
    public void Parse_AmbiguousCommand()
    {
        
        var commands = Enumerable.Range(1, 10).Select(i => new SubCommand($"sum{i}", "Sum numbers",
            new Argument[] { new("addend1", "Addend 1"), new("addend2", "Addend 2") },
            (cmd, opts, args) => 0)).ToArray();

        var command = new SubCommand("all", "All commands", commands);
        var result = command.Parse("sum");

        result.Should().BeOfType<SubCommand.ParseFailure>().Which
            .ErrorCode.Should().Be(SubCommand.AmbiguousCommandReturnCode);
    }    

    [Fact]
    public void Parse_CorrectSubcommand()
    {
        
        var commands = Enumerable.Range(1, 10).Select(i => new SubCommand($"sum{i}", "Sum numbers",
            new Argument[] { new("addend1", "Addend 1"), new("addend2", "Addend 2") },
            (cmd, opts, args) => 0)).ToArray();

        var command = new SubCommand("all", "All commands", commands);
        var result = command.Parse("sum7", "3", "4");

        result.Should().BeOfType<SubCommand.ParseSuccess>();
            
        var success = (SubCommand.ParseSuccess)result;
        success.Arguments.Should().Equal(
                new KeyValuePair<string, string>("addend1", "3"),
                new KeyValuePair<string, string>("addend2", "4"));
        success.Options.Should().BeEmpty();
        success.Command.Name.Should().Be("sum7");
    }
}