using System;
using System.Collections.Generic;
using System.Linq;
using Sheller.Implementations.Executables;
using Sheller.Models;

namespace Sheller.Implementations.Shells
{
    /// <summary>
    /// The implementation base class for well-known shells (i.e., defines its own shell string).
    /// </summary>
    /// <typeparam name="TShell">The type of the shell class implementing this interface.</typeparam>
    public abstract class Shell<TShell> : ShellBase<TShell> where TShell : ShellBase<TShell>, new()
    {
        /// <summary>
        /// Initializes the shell.
        /// </summary>
        public abstract TShell Initialize();
    }

    /// <summary>
    /// The implementation base class for shells.
    /// </summary>
    /// <typeparam name="TShell">The type of the shell class implementing this interface.</typeparam>
    public abstract class ShellBase<TShell> : IShell<TShell> where TShell : ShellBase<TShell>, new()
    {
        private string _shell;
        private IEnumerable<KeyValuePair<string, string>> _environmentVariables;

        /// <summary>
        /// The <see cref="Path"/> property.
        /// </summary>
        public virtual string Path => _shell;

        /// <summary>
        /// The <see cref="EnvironmentVariables"/> property.
        /// </summary>
        public virtual IEnumerable<KeyValuePair<string, string>> EnvironmentVariables => _environmentVariables;

        /// <summary>
        /// Initializes the shell.
        /// </summary>
        /// <param name="shell">The name or path of the shell.</param>
        public virtual TShell Initialize(string shell) => Initialize(shell, null);

        /// <summary>
        /// Initializes the shell.
        /// </summary>
        /// <param name="shell">The name or path of the shell.</param>
        /// <param name="environmentVariables">The environment variables to set on any executions in this shell.</param>
        public virtual TShell Initialize(string shell, IEnumerable<KeyValuePair<string, string>> environmentVariables)
        {
            _shell = shell;
            _environmentVariables = environmentVariables ?? new List<KeyValuePair<string, string>>();

            return this as TShell;
        }

        /// <summary>
        /// Adds an environment variable (of which there may be many) to the shell context and returns a `new` context instance.
        /// </summary>
        /// <param name="key">The environment variable key.</param>
        /// <param name="value">The environment variable value.</param>
        /// <returns>A `new` instance of <typeparamref name="TShell"/> with the environment variable passed in this call.</returns>
        public virtual TShell WithEnvironmentVariable(string key, string value) => 
            new TShell().Initialize(
                _shell,
                Helpers.MergeEnumerables(_environmentVariables, new KeyValuePair<string, string>(key, value).ToEnumerable())
            );
                
        /// <summary>
        /// Adds a list of environment variables (of which there may be many) to the shell context and returns a `new` context instance.
        /// </summary>
        /// <param name="variables">The list of key value pairs of environment variables.</param>
        /// <returns>A `new` instance of <typeparamref name="TShell"/> with the environment variables passed in this call.</returns>
        public virtual TShell WithEnvironmentVariables(IEnumerable<KeyValuePair<string, string>> variables) => 
            new TShell().Initialize(
                _shell,
                Helpers.MergeEnumerables(_environmentVariables, variables)
            );

        /// <summary>
        /// Adds a list of environment variables (of which there may be many) to the shell context and returns a `new` context instance.
        /// </summary>
        /// <param name="variables">The list of tuple of environment variables.</param>
        /// <returns>A `new` instance of <typeparamref name="TShell"/> with the environment variables passed in this call.</returns>
        public virtual TShell WithEnvironmentVariables(IEnumerable<(string, string)> variables) =>
            new TShell().Initialize(
                _shell,
                Helpers.MergeEnumerables(_environmentVariables, variables.ToDictionary())
            );

        /// <summary>
        /// Adds an executable and switches to the executable context.
        /// </summary>
        /// <typeparam name="TExecutable">The type of the executable to use.</typeparam>
        /// <returns>An instance of <typeparamref name="TExecutable"/> passed to this call.</returns>
        public virtual TExecutable UseExecutable<TExecutable>() where TExecutable : Executable<TExecutable>, new() => new TExecutable().Initialize(this);

        /// <summary>
        /// Adds an executable and switches to the executable context.
        /// </summary>
        /// <param name="exe">The name or path of the executable.</param>
        /// <returns>An instance of <see cref="GenericExe"/> which represents the executable name or path passed to this call.</returns>
        public virtual GenericExe UseExecutable(string exe) => new GenericExe().Initialize(exe, this);

        /// <summary>
        /// Builds the arguments that should be passed to the shell based on the shell's type.
        /// </summary>
        /// <param name="executableCommand">The name or path of the executable for which to build the command.</param>
        /// <returns>A <see cref="string"/> which represents the command argument that should be passed to this shell.</returns>
        public abstract string GetCommandArgument(string executableCommand);
    }
}