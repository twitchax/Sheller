

using System;
using System.Collections.Generic;
using System.Linq;
using Sheller.Implementations.Executables;
using Sheller.Models;

namespace Sheller.Implementations.Shells
{
    /// <summary>
    /// The implementation base class for shells.
    /// </summary>
    public class Shell : IShell
    {
        #region Properties

        private string _shell;
        private IEnumerable<KeyValuePair<string, string>> _environmentVariables;

        /// <summary>
        /// The <see cref="Path"/> property.
        /// </summary>
        public string Path => _shell;

        #endregion

        #region Constructors

        // TODO: THIS SHOULD BE `Initialize`.

        /// <summary>
        /// Instantiates the shell.
        /// </summary>
        /// <param name="shell">The name or path of the shell.</param>
        /// <param name="environmentVariables">The environment variables to set on any executions in this shell.</param>
        public Shell(string shell, IEnumerable<KeyValuePair<string, string>> environmentVariables = null)
        {
            _shell = shell;
            _environmentVariables = environmentVariables ?? new List<KeyValuePair<string, string>>();
        }

        #endregion

        #region Environment Variables

        /// <summary>
        /// The <see cref="EnvironmentVariables"/> property.
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> EnvironmentVariables => _environmentVariables;

        /// <summary>
        /// Adds an environment variable (of which there may be many) to the shell context and returns a `new` context instance.
        /// </summary>
        /// <param name="key">The environment variable key.</param>
        /// <param name="value">The environment variable value.</param>
        /// <returns>A `new` instance of <see cref="IShell"/> with the environment variable passed in this call.</returns>
        public IShell WithEnvironmentVariable(string key, string value)
        {
            return new Shell(
                _shell,
                Helpers.MergeEnumerables(_environmentVariables, new KeyValuePair<string, string>(key, value).ToEnumerable()));
        }

        /// <summary>
        /// Adds a list of environment variables (of which there may be many) to the shell context and returns a `new` context instance.
        /// </summary>
        /// <param name="variables">The list of key value pairs of environment variables.</param>
        /// <returns>A `new` instance of <see cref="IShell"/> with the environment variables passed in this call.</returns>
        public IShell WithEnvironmentVariables(IEnumerable<KeyValuePair<string, string>> variables)
        {
            return new Shell(
                _shell,
                Helpers.MergeEnumerables(_environmentVariables, variables));
        }

        /// <summary>
        /// Adds a list of environment variables (of which there may be many) to the shell context and returns a `new` context instance.
        /// </summary>
        /// <param name="variables">The list of tuple of environment variables.</param>
        /// <returns>A `new` instance of <see cref="IShell"/> with the environment variables passed in this call.</returns>
        public IShell WithEnvironmentVariables(IEnumerable<(string, string)> variables)
        {
            return new Shell(
                _shell,
                Helpers.MergeEnumerables(_environmentVariables, variables.ToDictionary()));
        }

        #endregion

        #region Executable

        /// <summary>
        /// Adds an executable and switches to the executable context.
        /// </summary>
        /// <typeparam name="TExecutable">The type of the executable to use.</typeparam>
        /// <returns>An instance of <typeparamref name="TExecutable"/> passed to this call.</returns>
        public TExecutable UseExecutable<TExecutable>() where TExecutable : Executable<TExecutable>, new()
        {
            var result = new TExecutable();
            result.Initialize(this);

            return result;
        }

        /// <summary>
        /// Adds an executable and switches to the executable context.
        /// </summary>
        /// <param name="exe">The name or path of the executable.</param>
        /// <returns>An instance of <see cref="Generic"/> which represents the executable name or path passed to this call.</returns>
        public Generic UseExecutable(string exe)
        {
            var result = new Generic();
            result.Initialize(exe, this);

            return result;
        }

        // TODO: THIS SHOULD BE ABSTRACT!
        /// <summary>
        /// Builds the arguments that should be passed to the shell based on the shell's type.
        /// </summary>
        /// <param name="executableCommand">The name or path of the executable for which to build the command.</param>
        /// <returns>A <see cref="string"/> which represewnts the command aergument that should be passed to this shell.</returns>
        public virtual string GetCommandArgument(string executableCommand)
        {
            var environmentVariables = _environmentVariables.Aggregate("", (agg, kvp) => agg += $"export {kvp.Key}=\"{kvp.Value.EscapeQuotes()}\";");
            return $"-c \"{environmentVariables}{ executableCommand.EscapeQuotes() }\"";
        }

        #endregion
    }
}