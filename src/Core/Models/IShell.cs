

using System.Collections.Generic;

namespace Sheller.Models
{
    public interface IShell
    {
        string Path { get; }

        IEnumerable<ILogger> Loggers { get ; }
        IShell WithDefaultLogger();
        IShell WithLogger(ILogger logger);
        IShell WithLoggers(IEnumerable<ILogger> loggers);

        IEnumerable<KeyValuePair<string, string>> EnvironmentVariables { get ; }
        IShell WithEnvironmentVariable(string key, string value);
        IShell WithEnvironmentVariables(IEnumerable<KeyValuePair<string, string>> variables);
        IShell WithEnvironmentVariables(IEnumerable<(string, string)> variables);

        TExecutable UseExecutable<TExecutable>() where TExecutable : IExecutable, new();
        IExecutable<ICommandResult> UseExecutable(string exe);

        // TODO: Remove this and the need for a shell name...maybe rename to icommand?
        string GetCommandArgument(string executableCommand);
    }
}