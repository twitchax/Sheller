using System;
using Sheller.Models;

namespace Sheller.Implementations
{
    /// <summary>
    /// The <see cref="Exception"/> type for execution errors.
    /// </summary>
    public class ExecutionException : Exception
    {
        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <returns></returns>
        public ExecutionException(string message) : base(message) {}
    }

    /// <summary>
    /// The <see cref="Exception"/> type for a failed execution.
    /// </summary>
    public class ExecutionFailedException : ExecutionException
    {
        /// <summary>
        /// The <see cref="Result"/> property.
        /// </summary>
        /// <value>The result of the failed execution.</value>
        public ICommandResult Result { get; private set; }

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="result">The result of the execution.</param>
        public ExecutionFailedException(string message, ICommandResult result) : base(message)
        {
            Result = result;
        }
    }

    /// <summary>
    /// The <see cref="Exception"/> type for a failed execution.
    /// </summary>
    public class ExecutionTimeoutException : ExecutionException
    {
        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <returns></returns>
        public ExecutionTimeoutException(string message) : base(message) {}
    }
}