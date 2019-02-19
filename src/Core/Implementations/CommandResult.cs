
using Sheller.Models;

namespace Sheller.Implementations
{
    /// <summary>
    /// The default result type for executables.
    /// </summary>
    public class CommandResult : ICommandResult
    {
        /// <summary>
        /// Succeeded property.
        /// </summary>
        /// <value>The succeeded status of an executable.</value>
        public bool Succeeded { get; }

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
        /// <param name="succeeded"></param>
        /// <param name="exitCode"></param>
        /// <param name="standardOutput"></param>
        /// <param name="standardError"></param>
        public CommandResult(bool succeeded, int exitCode, string standardOutput, string standardError)
        {
            Succeeded = succeeded;
            ExitCode = exitCode;
            StandardOutput = standardOutput;
            StandardError = standardError;
        }
    }

    /// <summary>
    /// The default result type for executables.
    /// </summary>
    /// <typeparam name="TResult">The result type of this <see cref="CommandResult{TResult}"/>.</typeparam>
    public class CommandResult<TResult> : CommandResult, ICommandResult<TResult>
    {
        /// <summary>
        /// Result property.
        /// </summary>
        /// <value>The result of an executable.</value>
        public TResult Result { get; private set; }

        /// <summary>
        /// The CommandResult constructor.
        /// </summary>
        /// <param name="succeeded"></param>
        /// <param name="exitCode"></param>
        /// <param name="standardOutput"></param>
        /// <param name="standardError"></param>
        /// <param name="result"></param>
        public CommandResult(bool succeeded, int exitCode, string standardOutput, string standardError, TResult result) : base(succeeded, exitCode, standardOutput, standardError)
        {
            Result = result;
        }
    }
}