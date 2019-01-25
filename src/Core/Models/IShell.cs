

using System.Collections.Generic;
using Sheller.Implementations.Executables;

namespace Sheller.Models
{
    /// <summary>
    /// The interface for defining a shell.
    /// </summary>
    public interface IShell
    {
        /// <summary>
        /// The <see cref="Path"/> property.
        /// </summary>
        /// <value>The path to the shell.</value>
        string Path { get; }
        
        /// <summary>
        /// The <see cref="EnvironmentVariables"/> property.
        /// </summary>
        /// <value>The environment variables that are set on the shell.</value>
        IEnumerable<KeyValuePair<string, string>> EnvironmentVariables { get ; }
        /// <summary>
        /// Adds an environment variable (of which there may be many) to the shell context and returns a `new` context instance.
        /// </summary>
        /// <param name="key">The environment variable key.</param>
        /// <param name="value">The environment variable value.</param>
        /// <returns>A `new` instance of <see cref="IShell"/> with the environment variable passed in this call.</returns>
        IShell WithEnvironmentVariable(string key, string value);
        /// <summary>
        /// Adds a list of environment variables (of which there may be many) to the shell context and returns a `new` context instance.
        /// </summary>
        /// <param name="variables">The list of key value pairs of environment variables.</param>
        /// <returns>A `new` instance of <see cref="IShell"/> with the environment variables passed in this call.</returns>
        IShell WithEnvironmentVariables(IEnumerable<KeyValuePair<string, string>> variables);
        /// <summary>
        /// Adds a list of environment variables (of which there may be many) to the shell context and returns a `new` context instance.
        /// </summary>
        /// <param name="variables">The list of tuple of environment variables.</param>
        /// <returns>A `new` instance of <see cref="IShell"/> with the environment variables passed in this call.</returns>
        IShell WithEnvironmentVariables(IEnumerable<(string, string)> variables);

        /// <summary>
        /// Adds an executable and switches to the executable context.
        /// </summary>
        /// <typeparam name="TExecutable">The type of the executable to use.</typeparam>
        /// <returns>An instance of <typeparamref name="TExecutable"/> passed to this call.</returns>
        TExecutable UseExecutable<TExecutable>() where TExecutable : Executable<TExecutable>, new();
        /// <summary>
        /// Adds an executable and switches to the executable context.
        /// </summary>
        /// <param name="exe">The name or path of the executable.</param>
        /// <returns>An instance of <see cref="Generic"/> which represents the executable name or path passed to this call.</returns>
        Generic UseExecutable(string exe);
        
        /// <summary>
        /// Builds the arguments that should be passed to the shell based on the shell's type.
        /// </summary>
        /// <param name="executableCommand">The name or path of the executable for which to build the command.</param>
        /// <returns>A <see cref="string"/> which represewnts the command aergument that should be passed to this shell.</returns>
        string GetCommandArgument(string executableCommand);
    }
}