using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sheller.Implementations.Executables;
using Sheller.Models;

namespace Sheller.Implementations.Shells
{
    /// <summary>
    /// The implementation base class for well-known shells (i.e., defines its own shell string).
    /// </summary>
    /// <typeparam name="TShell">The type of the shell class implementing this interface.</typeparam>
    public abstract class Shell<TShell> : ShellBase<TShell> where TShell : Shell<TShell>, new()
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

        private IEnumerable<string> _standardInputs;
        private IEnumerable<Action<string>> _standardOutputHandlers;
        private IEnumerable<Action<string>> _standardErrorHandlers;

        private bool _throws;

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
        public virtual TShell Initialize(string shell) => Initialize(shell, null, null, null, null, null);

        /// <summary>
        /// Initializes the shell.
        /// </summary>
        /// <param name="shell">The name or path of the shell.</param>
        /// <param name="environmentVariables">The environment variables to set on any executions in this shell.</param>
        /// <param name="standardInputs">The standard inputs to pass to the execution.</param>
        /// <param name="standardOutputHandlers">The standard output handlers for capture from the execution.</param>
        /// <param name="standardErrorHandlers">The standard error handlers for capture from the execution.</param>
        /// <param name="throws">Indicates that a non-zero exit code throws.</param>
        protected virtual TShell Initialize(
            string shell, 
            IEnumerable<KeyValuePair<string, string>> environmentVariables,
            IEnumerable<string> standardInputs,
            IEnumerable<Action<string>> standardOutputHandlers,
            IEnumerable<Action<string>> standardErrorHandlers,
            bool? throws)
        {
            _shell = shell;

            _environmentVariables = environmentVariables ?? new Dictionary<string, string>();

            _standardInputs = standardInputs ?? new List<string>();
            _standardOutputHandlers = standardOutputHandlers ?? new List<Action<string>>();
            _standardErrorHandlers = standardErrorHandlers ?? new List<Action<string>>();

            _throws = throws ?? true;

            return this as TShell;
        }

        private static TShell CreateFrom(
            ShellBase<TShell> old,
            string shell = null, 
            IEnumerable<KeyValuePair<string, string>> environmentVariables = null,
            IEnumerable<string> standardInputs = null,
            IEnumerable<Action<string>> standardOutputHandlers = null,
            IEnumerable<Action<string>> standardErrorHandlers = null,
            bool? throws = null) =>
                new TShell().Initialize(
                    shell ?? old._shell,
                    environmentVariables ?? old._environmentVariables,
                    standardInputs ?? old._standardInputs,
                    standardOutputHandlers ?? old._standardOutputHandlers,
                    standardErrorHandlers ?? old._standardErrorHandlers,
                    throws ?? old._throws
                );

        /// <summary>
        /// Executes a command and arguments in the specified shell.
        /// </summary>
        /// <param name="executable">The executable name or path/</param>
        /// <param name="arguments">The arguments to be passed to the executable (which will be space-separated).</param>
        /// <exception cref="ExecutionFailedException">Thrown when the exit code of the execution is non-zero.</exception>
        /// <returns>A task which results in an <see cref="ICommandResult"/> (i.e., the result of the command execution).</returns>
        public virtual async Task<ICommandResult> ExecuteCommandAsync(string executable, IEnumerable<string> arguments)
        {
            var command = _shell;
            var commandArguments = this.GetCommandArgument($"{executable} {string.Join(" ", arguments)}");
            
            var result = await Helpers.RunCommand(
                command, 
                commandArguments,
                _standardInputs,
                _standardOutputHandlers, 
                _standardErrorHandlers);

            if(_throws && result.ExitCode != 0)
                throw new ExecutionFailedException($"The execution resulted in a non-zero exit code ({result.ExitCode}).", result);

            return result;
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>A `new` instance of <typeparamref name="TShell"/> with the same settings as the invoking instance.</returns>
        public virtual TShell Clone() => CreateFrom(this);
        IShell IShell.Clone() => Clone();

        /// <summary>
        /// Adds an environment variable (of which there may be many) to the shell context and returns a `new` context instance.
        /// </summary>
        /// <param name="key">The environment variable key.</param>
        /// <param name="value">The environment variable value.</param>
        /// <returns>A `new` instance of <typeparamref name="TShell"/> with the environment variable passed in this call.</returns>
        public virtual TShell WithEnvironmentVariable(string key, string value) => CreateFrom(this, environmentVariables: Helpers.MergeEnumerables(_environmentVariables, new KeyValuePair<string, string>(key, value).ToEnumerable()));
        IShell IShell.WithEnvironmentVariable(string key, string value) => WithEnvironmentVariable(key, value);
                
        /// <summary>
        /// Adds a list of environment variables (of which there may be many) to the shell context and returns a `new` context instance.
        /// </summary>
        /// <param name="variables">The list of key value pairs of environment variables.</param>
        /// <returns>A `new` instance of <typeparamref name="TShell"/> with the environment variables passed in this call.</returns>
        public virtual TShell WithEnvironmentVariables(IEnumerable<KeyValuePair<string, string>> variables) => CreateFrom(this, environmentVariables: Helpers.MergeEnumerables(_environmentVariables, variables));
        IShell IShell.WithEnvironmentVariables(IEnumerable<KeyValuePair<string, string>> variables) => WithEnvironmentVariables(variables);

        /// <summary>
        /// Adds a list of environment variables (of which there may be many) to the shell context and returns a `new` context instance.
        /// </summary>
        /// <param name="variables">The list of tuple of environment variables.</param>
        /// <returns>A `new` instance of <typeparamref name="TShell"/> with the environment variables passed in this call.</returns>
        public virtual TShell WithEnvironmentVariables(IEnumerable<(string, string)> variables) => CreateFrom(this, environmentVariables: Helpers.MergeEnumerables(_environmentVariables, variables.ToDictionary()));
        IShell IShell.WithEnvironmentVariables(IEnumerable<(string, string)> variables) => WithEnvironmentVariables(variables);

        /// <summary>
        /// Adds a string to the standard input stream (of which there may be many) to the shell context and returns a `new` context instance.
        /// </summary>
        /// <param name="standardInput">A string that gets passed to the standard input stram of the executable.</param>
        /// <returns>A `new` instance of type <typeparamref name="TShell"/> with the standard input passed to this call.</returns>
        public TShell WithStandardInput(string standardInput) => CreateFrom(this, standardInputs: Helpers.MergeEnumerables(_standardInputs, standardInput.ToEnumerable()));
        IShell IShell.WithStandardInput(string standardInput) => WithStandardInput(standardInput);

        /// <summary>
        /// Adds a standard output handler (of which there may be many) to the shell context and returns a `new` context instance.
        /// </summary>
        /// <param name="standardOutputHandler">An <see cref="Action"/> that handles a new line in the standard output of the executable.</param>
        /// <returns>A `new` instance of <typeparamref name="TShell"/> with the standard output handler passed to this call.</returns>
        public virtual TShell WithStandardOutputHandler(Action<string> standardOutputHandler) => CreateFrom(this, standardOutputHandlers: Helpers.MergeEnumerables(_standardOutputHandlers, standardOutputHandler.ToEnumerable()));
        IShell IShell.WithStandardOutputHandler(Action<string> standardOutputHandler) => WithStandardOutputHandler(standardOutputHandler);

        /// <summary>
        /// Adds an error output handler (of which there may be many) to the shell context and returns a `new` context instance.
        /// </summary>
        /// <param name="standardErrorHandler">An <see cref="Action"/> that handles a new line in the standard error of the executable.</param>
        /// <returns>A `new` instance of <typeparamref name="TShell"/> with the standard error handler passed to this call.</returns>
        public virtual TShell WithStandardErrorHandler(Action<string> standardErrorHandler) => CreateFrom(this, standardErrorHandlers: Helpers.MergeEnumerables(_standardErrorHandlers, standardErrorHandler.ToEnumerable()));
        IShell IShell.WithStandardErrorHandler(Action<string> standardErrorHandler) => WithStandardErrorHandler(standardErrorHandler);

        /// <summary>
        /// Ensures the shell context will not throw on a non-zero exit code and returns a `new` context instance.
        /// </summary>
        /// <returns>A `new` instance of type <typeparamref name="TShell"/> that will not throw on a non-zero exit code.</returns>
        public TShell WithNoThrow() => CreateFrom(this, throws: false);
        IShell IShell.WithNoThrow() => WithNoThrow();

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