
using Sheller.Models;

namespace Sheller.Implementations
{
    /// <summary>
    /// The default result type for executables.
    /// </summary>
    public class CommandResult : ICommandResult
    {
        /// <summary>
        /// ExitCode property.
        /// </summary>
        /// <value>The exit code of an executable.</value>
        public int ExitCode { get; private set; }

        /// <summary>
        /// StandardOutput property.
        /// </summary>
        /// <value>The standard output of an executable.</value>
        public string StandardOutput { get; private set; }

        /// <summary>
        /// StandardError property.
        /// </summary>
        /// <value>The standard error of an executable.</value>
        public string StandardError { get; private set; }

        /// <summary>
        /// The CommandResult constructor.
        /// </summary>
        /// <param name="exitCode"></param>
        /// <param name="standardOutput"></param>
        /// <param name="standardError"></param>
        public CommandResult(int exitCode, string standardOutput, string standardError)
        {
            ExitCode = exitCode;
            StandardOutput = standardOutput;
            StandardError = standardError;
        }
    }
}