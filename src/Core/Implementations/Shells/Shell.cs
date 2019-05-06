using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Sheller.Implementations.Executables;
using Sheller.Models;

namespace Sheller.Implementations.Shells
{
    using System.Linq;

    /// <summary>
    /// The implementation base class for shells.
    /// </summary>
    /// <typeparam name="TIShell">The type of the shell interface implementing this interface.</typeparam>
    public abstract class Shell<TIShell> : IShell<TIShell> where TIShell : IShell
    {
        private string _shell;

        private IEnumerable<KeyValuePair<string, string>> _environmentVariables;

        private IEnumerable<string> _standardInputs;
        private IEnumerable<Action<string>> _standardOutputHandlers;
        private IEnumerable<Action<string>> _standardErrorHandlers;
        private Func<string, string, Task<String>> _inputRequestHandler;
        private Encoding _standardOutputEncoding;
        private Encoding _standardErrorEncoding;

        private ObservableCommandEvent _observableCommandEvent;

        private IEnumerable<CancellationToken> _cancellationTokens;

        private string _commandPrefix;

        private Action<ProcessStartInfo> _startInfoTransform;

        private bool _throws;

        /// <summary>
        /// The <see cref="Path"/> property.
        /// </summary>
        public string Path => _shell;

        /// <summary>
        /// The <see cref="EnvironmentVariables"/> property.
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> EnvironmentVariables => _environmentVariables;

        /// <summary>
        /// The <cref see="Shell"/> constructor.
        /// </summary>
        /// <param name="shell">The name or path of the shell.</param>        
        public Shell(string shell) => this.Initialize(shell);

        /// <summary>
        /// Allows an implementer of <see cref="Shell{TIShell}"/>
        /// </summary>
        /// <returns></returns>
        protected abstract Shell<TIShell> Create();
        
        private IShell Initialize(string shell) => Initialize(shell, null, null, null, null, null, null, null, null, null, null, null, null);

        private IShell Initialize(
            string shell, 
            IEnumerable<KeyValuePair<string, string>> environmentVariables,
            IEnumerable<string> standardInputs,
            IEnumerable<Action<string>> standardOutputHandlers,
            IEnumerable<Action<string>> standardErrorHandlers,
            Func<string, string, Task<string>> inputRequestHandler,
            Encoding standardOutputEncoding,
            Encoding standardErrorEncoding,
            ObservableCommandEvent observableCommandEvent, 
            IEnumerable<CancellationToken> cancellationTokens,
            string commandPrefix,
            Action<ProcessStartInfo> startInfoTransform,
            bool? throws)
        {
            _shell = shell ?? throw new InvalidOperationException("This shell definition does not define a shell.");;

            _environmentVariables = environmentVariables ?? new Dictionary<string, string>();

            _standardInputs = standardInputs ?? new List<string>();
            _standardOutputHandlers = standardOutputHandlers ?? new List<Action<string>>();
            _standardErrorHandlers = standardErrorHandlers ?? new List<Action<string>>();
            _inputRequestHandler = inputRequestHandler;
            _standardOutputEncoding = standardOutputEncoding;
            _standardErrorEncoding = standardErrorEncoding;

            _observableCommandEvent = observableCommandEvent ?? new ObservableCommandEvent();

            _cancellationTokens = cancellationTokens ?? new List<CancellationToken>();

            _commandPrefix = commandPrefix;

            _startInfoTransform = startInfoTransform;
            
            _throws = throws ?? true;

            return this;
        }

        private static TIShell CreateFrom(
            Shell<TIShell> old,
            string shell = null, 
            IEnumerable<KeyValuePair<string, string>> environmentVariables = null,
            IEnumerable<string> standardInputs = null,
            IEnumerable<Action<string>> standardOutputHandlers = null,
            IEnumerable<Action<string>> standardErrorHandlers = null,
            Func<string, string, Task<string>> inputRequestHandler = null,
            Encoding standardOutputEncoding = null,
            Encoding standardErrorEncoding = null,
            ObservableCommandEvent observableCommandEvent = null, 
            IEnumerable<CancellationToken> cancellationTokens = null,
            string commandPrefix = null,
            Action<ProcessStartInfo> startInfoTransform = null,
            bool? throws = null) =>
                (TIShell)old.Create().Initialize(
                    shell ?? old._shell,
                    environmentVariables ?? old._environmentVariables,
                    standardInputs ?? old._standardInputs,
                    standardOutputHandlers ?? old._standardOutputHandlers,
                    standardErrorHandlers ?? old._standardErrorHandlers,
                    inputRequestHandler ?? old._inputRequestHandler,
                    standardOutputEncoding ?? old._standardOutputEncoding,
                    standardErrorEncoding ?? old._standardErrorEncoding,
                    observableCommandEvent ?? old._observableCommandEvent,
                    cancellationTokens ?? old._cancellationTokens,
                    commandPrefix ?? old._commandPrefix,
                    startInfoTransform ?? old._startInfoTransform,
                    throws ?? old._throws
                );

        /// <summary>
        /// Executes a command and arguments in the specified shell.
        /// </summary>
        /// <param name="executable">The executable name or path.</param>
        /// <param name="arguments">The arguments to be passed to the executable (which will be space-separated).</param>
        /// <exception cref="ExecutionFailedException">Thrown when the exit code of the execution is non-zero.</exception>
        /// <returns>A task which results in an <see cref="ICommandResult"/> (i.e., the result of the command execution).</returns>
        public virtual async Task<ICommandResult> ExecuteCommandAsync(string executable, IEnumerable<string> arguments)
        {
            var command = _shell;
            var commandArguments = 
                _commandPrefix == null ?
                this.GetCommandArgument($"{executable} {string.Join(" ", arguments)}") :
                this.GetCommandArgument($"{_commandPrefix} {executable} {string.Join(" ", arguments)}");
            
            var result = await Helpers.RunCommand(
                command, 
                commandArguments,
                _standardInputs,
                _standardOutputHandlers, 
                _standardErrorHandlers,
                _inputRequestHandler,
                _observableCommandEvent,
                _cancellationTokens,
                _standardOutputEncoding,
                _standardErrorEncoding,
                _startInfoTransform).ConfigureAwait(false);

            if(_throws && result.ExitCode != 0)
            {
                var error = new ExecutionFailedException($"The execution resulted in a non-zero exit code ({result.ExitCode}).", result);
                _observableCommandEvent.FireError(error);
                throw error;
            }

            // TODO: Add a `UseSubscribeComplete` (or something like it) that instructs the observable to complete here.
            //_observableCommandEvent.FireCompleted();

            return result;
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>A `new` instance of <typeparamref name="TIShell"/> with the same settings as the invoking instance.</returns>
        public virtual TIShell Clone() => CreateFrom(this);
        IShell IShell.Clone() => Clone();

        /// <summary>
        /// Adds an environment variable (of which there may be many) to the shell context and returns a `new` context instance.
        /// </summary>
        /// <param name="key">The environment variable key.</param>
        /// <param name="value">The environment variable value.</param>
        /// <returns>A `new` instance of <typeparamref name="TIShell"/> with the environment variable passed in this call.</returns>
        public virtual TIShell WithEnvironmentVariable(string key, string value) => CreateFrom(this, environmentVariables: _environmentVariables.Append(new KeyValuePair<string, string>(key, value)));
        IShell IShell.WithEnvironmentVariable(string key, string value) => WithEnvironmentVariable(key, value);
                
        /// <summary>
        /// Adds a list of environment variables (of which there may be many) to the shell context and returns a `new` context instance.
        /// </summary>
        /// <param name="variables">The list of key value pairs of environment variables.</param>
        /// <returns>A `new` instance of <typeparamref name="TIShell"/> with the environment variables passed in this call.</returns>
        public virtual TIShell WithEnvironmentVariables(IEnumerable<KeyValuePair<string, string>> variables) => CreateFrom(this, environmentVariables: _environmentVariables.Concat(variables));
        IShell IShell.WithEnvironmentVariables(IEnumerable<KeyValuePair<string, string>> variables) => WithEnvironmentVariables(variables);

        /// <summary>
        /// Adds a list of environment variables (of which there may be many) to the shell context and returns a `new` context instance.
        /// </summary>
        /// <param name="variables">The list of tuple of environment variables.</param>
        /// <returns>A `new` instance of <typeparamref name="TIShell"/> with the environment variables passed in this call.</returns>
        public virtual TIShell WithEnvironmentVariables(IEnumerable<(string, string)> variables) => CreateFrom(this, environmentVariables: _environmentVariables.Concat(variables.ToDictionary()));
        IShell IShell.WithEnvironmentVariables(IEnumerable<(string, string)> variables) => WithEnvironmentVariables(variables);

        /// <summary>
        /// Adds a string to the standard input stream (of which there may be many) to the shell context and returns a `new` context instance.
        /// </summary>
        /// <param name="standardInput">A string that gets passed to the standard input stram of the executable.</param>
        /// <returns>A `new` instance of type <typeparamref name="TIShell"/> with the standard input passed to this call.</returns>
        public TIShell WithStandardInput(string standardInput) => CreateFrom(this, standardInputs: _standardInputs.Append(standardInput));
        IShell IShell.WithStandardInput(string standardInput) => WithStandardInput(standardInput);

        /// <summary>
        /// Adds a standard output handler (of which there may be many) to the shell context and returns a `new` context instance.
        /// </summary>
        /// <param name="standardOutputHandler">An <see cref="Action"/> that handles a new line in the standard output of the executable.</param>
        /// <returns>A `new` instance of <typeparamref name="TIShell"/> with the standard output handler passed to this call.</returns>
        public virtual TIShell WithStandardOutputHandler(Action<string> standardOutputHandler) => CreateFrom(this, standardOutputHandlers: _standardOutputHandlers.Append(standardOutputHandler));
        IShell IShell.WithStandardOutputHandler(Action<string> standardOutputHandler) => WithStandardOutputHandler(standardOutputHandler);

        /// <summary>
        /// Adds an error output handler (of which there may be many) to the shell context and returns a `new` context instance.
        /// </summary>
        /// <param name="standardErrorHandler">An <see cref="Action"/> that handles a new line in the standard error of the executable.</param>
        /// <returns>A `new` instance of <typeparamref name="TIShell"/> with the standard error handler passed to this call.</returns>
        public virtual TIShell WithStandardErrorHandler(Action<string> standardErrorHandler) => CreateFrom(this, standardErrorHandlers: _standardErrorHandlers.Append(standardErrorHandler));
        IShell IShell.WithStandardErrorHandler(Action<string> standardErrorHandler) => WithStandardErrorHandler(standardErrorHandler);

        /// <summary>
        /// Adds a (user) input request handler to the shell context and returns a `new` context instance.
        /// </summary>
        /// <param name="inputRequestHandler">
        /// A <see cref="Func{T}"/> that handles when the shell blocks for user input during an execution.
        /// This handler should take (string StandardOutput, string StandardInput) and return a <see cref="Task{String}"/>
        /// that will be passed to the executable as StandardInput.
        /// </param>
        /// <returns>A `new` instance of <typeparamref name="TIShell"/> with the standard error handler passed to this call.</returns>
        public TIShell UseInputRequestHandler(Func<string, string, Task<string>> inputRequestHandler) => CreateFrom(this, inputRequestHandler: inputRequestHandler);
        IShell IShell.UseInputRequestHandler(Func<string, string, Task<string>> inputRequestHandler) => UseInputRequestHandler(inputRequestHandler);

        /// <summary>
        /// Set the Standard Output Encoding on the shell context and returns a `new` context instance.
        /// </summary>
        /// <param name="standardOutputEncoding">The encoding to use for standard output.</param>
        /// <returns>A `new` instance of type <typeparamref name="TIShell"/> with the standard output encoding passed to this call.</returns>
        public TIShell UseStandardOutputEncoding(Encoding standardOutputEncoding) => CreateFrom(this, standardOutputEncoding: standardOutputEncoding);
        IShell IShell.UseStandardOutputEncoding(Encoding standardOutputEncoding) => UseStandardOutputEncoding(standardOutputEncoding);

        /// <summary>
        /// Set the Standard Error Encoding on the shell context and returns a `new` context instance.
        /// </summary>
        /// <param name="standardErrorEncoding">The encoding to use for standard error.</param>
        /// <returns>A `new` instance of type <typeparamref name="TIShell"/> with the standard error encoding passed to this call.</returns>
        public TIShell UseStandardErrorEncoding(Encoding standardErrorEncoding) => CreateFrom(this, standardErrorEncoding: standardErrorEncoding);
        IShell IShell.UseStandardErrorEncoding(Encoding standardErrorEncoding) => UseStandardErrorEncoding(standardErrorEncoding);

        /// <summary>
        /// Provides an <see cref="IObservable{T}"/> to which a subscription can be placed.
        /// The observable never completes, since executions can be run many times.
        /// </summary>
        /// <returns>A `new` instance of type <typeparamref name="TIShell"/> with the subscribers attached to the observable.</returns>
        public TIShell WithSubscribe(Action<IObservable<ICommandEvent>> subscriber)
        {
            var newObservable = new ObservableCommandEvent();
            subscriber(newObservable);
            return CreateFrom(this, observableCommandEvent: ObservableCommandEvent.Merge(_observableCommandEvent, newObservable));
        }
        IShell IShell.WithSubscribe(Action<IObservable<ICommandEvent>> subscriber) => WithSubscribe(subscriber);

        /// <summary>
        /// Adds a <see cref="CancellationToken"/> (of which there may be many) to the shell context and returns a `new` context instance.
        /// </summary>
        /// <returns>A `new` instance of type <typeparamref name="TIShell"/> with the cancellation token attached.</returns>
        public TIShell WithCancellationToken(CancellationToken cancellationToken) => CreateFrom(this, cancellationTokens: _cancellationTokens.Append(cancellationToken));
        IShell IShell.WithCancellationToken(CancellationToken cancellationToken) => WithCancellationToken(cancellationToken);

        /// <summary>
        /// Set a "prefix" string for all commands executed on the shell context and returns a `new` context instance.
        /// </summary>
        /// <param name="prefix">The prefix for all commands.</param>
        /// <returns>A `new` instance of type <typeparamref name="TIShell"/> with the prefix string passed to this call.</returns>
        public TIShell UseCommandPrefix(string prefix) => CreateFrom(this, commandPrefix: prefix);
        IShell IShell.UseCommandPrefix(string prefix) => UseCommandPrefix(prefix);

        /// <summary>
        /// Set a transform function on the <cref see="ProcessStartInfo"/> that is applied before execution, and returns a `new` context instance.
        /// </summary>
        /// <param name="startInfoTransform">The transform.</param>
        /// <returns>A `new` instance of type <typeparamref name="TIShell"/> with the transform passed to this call.</returns>
        public TIShell UseStartInfoTransform(Action<ProcessStartInfo> startInfoTransform) => CreateFrom(this, startInfoTransform: startInfoTransform);
        IShell IShell.UseStartInfoTransform(Action<ProcessStartInfo> startInfoTransform) => UseStartInfoTransform(startInfoTransform);

        /// <summary>
        /// Ensures the shell context will not throw on a non-zero exit code and returns a `new` context instance.
        /// </summary>
        /// <returns>A `new` instance of type <typeparamref name="TIShell"/> that will not throw on a non-zero exit code.</returns>
        public TIShell UseNoThrow() => CreateFrom(this, throws: false);
        IShell IShell.UseNoThrow() => UseNoThrow();

        /// <summary>
        /// Adds an executable and switches to the executable context.
        /// </summary>
        /// <typeparam name="TExecutable">The type of the executable to use.</typeparam>
        /// <returns>An instance of <typeparamref name="TExecutable"/> passed to this call.</returns>
        public virtual TExecutable UseExecutable<TExecutable>() where TExecutable : IExecutable, new() => (TExecutable)new TExecutable().UseShell(this);

        /// <summary>
        /// Adds an executable and switches to the executable context.
        /// </summary>
        /// <param name="exe">The name or path of the executable.</param>
        /// <returns>An instance of <see cref="GenericExe"/> which represents the executable name or path passed to this call.</returns>
        public virtual IExecutable UseExecutable(string exe) => new GenericExe(exe).UseShell(this);

        /// <summary>
        /// Builds the arguments that should be passed to the shell based on the shell's type.
        /// </summary>
        /// <param name="executableCommand">The name or path of the executable for which to build the command.</param>
        /// <returns>A <see cref="string"/> which represents the command argument that should be passed to this shell.</returns>
        public abstract string GetCommandArgument(string executableCommand);
    }
}