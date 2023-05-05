using System;
using System.Collections.Generic;
using System.Linq;
using Serilog;


namespace XOutput.Tools;

/// <summary>
///     Parses command line arguments.
/// </summary>
public class ArgumentParser
{
    public ArgumentParser() : this(Environment.GetCommandLineArgs().Skip(1).ToArray())
    {
    }

    public ArgumentParser(IEnumerable<string> arguments)
    {
        var args = arguments.ToList();
        Minimized = args.Any(arg => arg == "--minimized");
        if (Minimized) args.Remove("--minimized");
        foreach (var arg in args) Log.Warning($"Unused command line argument: {arg}");
    }

    /// <summary>
    ///     Gets if the application should start in silent mode.
    /// </summary>
    public bool Minimized { get; }
}