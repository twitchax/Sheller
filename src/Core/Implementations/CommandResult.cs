
using System;
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
        /// StartTime property.
        /// </summary>
        /// <value>The start time of an executable.</value>
        public DateTime StartTime { get; private set; }

        /// <summary>
        /// EndTime property.
        /// </summary>
        /// <value>The end time of an executable.</value>
        public DateTime EndTime { get; private set; }

        /// <summary>
        /// RunTime property.
        /// </summary>
        /// <value>The run time of an executable.</value>
        public TimeSpan RunTime { get; private set; }

        /// <summary>
        /// The CommandResult constructor.
        /// </summary>
        /// <param name="succeeded"></param>
        /// <param name="exitCode"></param>
        /// <param name="standardOutput"></param>
        /// <param name="standardError"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        public CommandResult(bool succeeded, int exitCode, string standardOutput, string standardError, DateTime startTime, DateTime endTime)
        {
            Succeeded = succeeded;
            ExitCode = exitCode;
            StandardOutput = standardOutput;
            StandardError = standardError;
            StartTime = startTime;
            EndTime = endTime;
            RunTime = endTime - startTime;
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
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="result"></param>
        public CommandResult(bool succeeded, int exitCode, string standardOutput, string standardError, DateTime startTime, DateTime endTime, TResult result) : base(succeeded, exitCode, standardOutput, standardError, startTime, endTime)
        {
            Result = result;
        }
    }
}