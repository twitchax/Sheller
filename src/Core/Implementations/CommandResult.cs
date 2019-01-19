
using Sheller.Models;

namespace Sheller.Implementations
{
    public class CommandResult : ICommandResult
    {
        public int ExitCode { get; private set; }
        public string StandardOutput { get; private set; }
        public string ErrorOutput { get; private set; }

        public CommandResult(int exitCode, string standardOutput, string errorOutput)
        {
            ExitCode = exitCode;
            StandardOutput = standardOutput;
            ErrorOutput = errorOutput;
        }
    }
}