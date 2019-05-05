using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Sheller.Models;

namespace Sheller.Implementations.Executables
{
    /// <summary>
    /// The implementation base class for generic executables with a custom result.
    /// </summary>
    /// <typeparam name="TIExecutable">The type of the executable interface implementing this interface.</typeparam>
    /// <typeparam name="TResult">The result type of the executable.</typeparam>
    public abstract class Executable<TIExecutable, TResult> : Executable<TIExecutable>, IExecutable<TIExecutable, TResult> where TIExecutable : IExecutable
    {
        /// <summary>
        /// Initializes the shell.
        /// </summary>
        /// <param name="executable">The name or path of the executable to run.</param>
        public Executable(string executable) : base(executable) {}

        /// <summary>
        /// Executes the executable.
        /// </summary>
        /// <returns>A task which results in an <typeparamref name="TResult"/> (i.e., the result of the execution).</returns>
        public abstract new Task<TResult> ExecuteAsync();
    }

    /// <summary>
    /// The implementation base class for generic executables.
    /// </summary>
    /// <typeparam name="TIExecutable">The type of the executable class implementing this interface.</typeparam>
    public abstract class Executable<TIExecutable> : IExecutable<TIExecutable> where TIExecutable : IExecutable
    {
        private string _executable;
        private IShell _shell;

        private IEnumerable<string> _arguments;

        private TimeSpan _timeout;

        private IEnumerable<Func<ICommandResult, Task>> _waitFuncs;
        private TimeSpan _waitTimeout;

        private IDictionary<string, object> _state;

        /// <summary>
        /// The <see cref="State"/> property.null  Allows an implementer to read the saved state for this execution context.
        /// </summary>
        protected IDictionary<string, object> State => _state;

        /// <summary>
        /// The path to the executable.
        /// </summary>
        protected string Path => _executable;

        /// <summary>
        /// The <cref see="Shell"/> to which this executable instance is attached.
        /// </summary>
        protected IShell Shell => _shell;

        /// <summary>
        /// Initializes the shell.
        /// </summary>
        /// <param name="executable">The name or path of the executable to run.</param>
        public Executable(string executable) => this.Initialize(executable);

        /// <summary>
        /// Allows an implementer of <see cref="Executable{TIExecutable}"/>
        /// </summary>
        /// <returns></returns>
        protected abstract Executable<TIExecutable> Create();

        /// <summary>
        /// Initializes this instance with the provided shell.
        /// </summary>
        /// <param name="executable">The name or path of the executable to run.</param>
        /// <returns>This instance.</returns>
        private IExecutable Initialize(string executable) => Initialize(executable, null, null, null, null, null, null);
        
        /// <summary>
        /// Initializes this instance with the provided shell.
        /// </summary>
        /// <param name="executable">The name or path of the executable to run.</param>
        /// <param name="shell">The shell in which the executable should run.</param>
        /// <param name="arguments">The arguments to pass to the executable.</param>
        /// <param name="timeout">The timeout of the execution.</param>
        /// <param name="waitFuncs">The wait functions which block execution after the executable execution.</param>
        /// <param name="waitTimeout">The timeout of the wait functions.</param>
        /// <param name="state">The subclass "state" of the execution context.</param>
        /// <returns>This instance.</returns>
        private IExecutable Initialize(
            string executable, 
            IShell shell, 
            IEnumerable<string> arguments,
            TimeSpan? timeout,
            IEnumerable<Func<ICommandResult, Task>> waitFuncs, 
            TimeSpan? waitTimeout,
            IDictionary<string, object> state)
        {
            _executable = executable ?? throw new InvalidOperationException("This executable definition does not define an executable.");
            _shell = shell?.Clone();

            _arguments = arguments ?? new List<String>();

            _timeout = timeout ?? TimeSpan.FromMinutes(10);

            _waitFuncs = waitFuncs ?? new List<Func<ICommandResult, Task>>();
            _waitTimeout = waitTimeout ?? TimeSpan.FromMinutes(10);

            _state = state ?? new Dictionary<string, object>();

            return this;
        }

        private static TIExecutable CreateFrom(
            Executable<TIExecutable> old,
            string executable = null, 
            IShell shell = null, 
            IEnumerable<string> arguments = null,
            TimeSpan? timeout = null,
            IEnumerable<Func<ICommandResult, Task>> waitFuncs = null, 
            TimeSpan? waitTimeout = null,
            IDictionary<string, object> state = null) =>
                (TIExecutable)old.Create().Initialize(
                    executable ?? old._executable,
                    shell ?? old._shell,
                    arguments ?? old._arguments,
                    timeout ?? old._timeout,
                    waitFuncs ?? old._waitFuncs,
                    waitTimeout ?? old._waitTimeout,
                    state ?? old._state
                );

        /// <summary>
        /// Executes the executable.
        /// </summary>
        /// <returns>A task which results in an <see cref="ICommandResult"/> (i.e., the result of the execution).</returns>
        public virtual Task<ICommandResult> ExecuteAsync() => ExecuteAsync(cr => cr);
        /// <summary>
        /// Executes the executable.
        /// </summary>
        /// <param name="resultSelector">A selector which transforms the result of the execution from <see cref="ICommandResult"/> to <typeparamref name="TResult"/>.</param>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <returns>A task which results in a <typeparamref name="TResult"/> (i.e., the result of the execution).</returns>
        public virtual Task<TResult> ExecuteAsync<TResult>(Func<ICommandResult, TResult> resultSelector) => ExecuteAsync(cr => Task.FromResult(resultSelector(cr)));
        /// <summary>
        /// Executes the executable.
        /// </summary>
        /// <param name="resultSelector">A selector which transforms the result of the execution from <see cref="ICommandResult"/> to <see cref="Task{TResult}"/>.</param>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <exception cref="ExecutionFailedException">Thrown when the exit code of the execution is non-zero.</exception>
        /// <exception cref="ExecutionTimeoutException">Thrown when a timeout is reached.</exception>
        /// <returns>A task which results in a <typeparamref name="TResult"/> (i.e., the result of the execution).</returns>
        public async virtual Task<TResult> ExecuteAsync<TResult>(Func<ICommandResult, Task<TResult>> resultSelector)
        {
            if(this._shell == null)
                throw new InvalidOperationException("This executable definition does not define a shell.  Please attach this definition to a shell or call `UseShell`.");
            
            async Task<TResult> executionTask()
            {
                var commandResult = await _shell.ExecuteCommandAsync($"{_executable}", _arguments).ConfigureAwait(false);

                var result = await resultSelector(commandResult).ConfigureAwait(false);

                // Await (ALL of the wait funcs) OR (the wait timeout), whichever comes first.
                var waitAllTask = Task.WhenAll(_waitFuncs.Select(f => f(commandResult)));
                if (await Task.WhenAny(waitAllTask, Task.Delay(_waitTimeout)).ConfigureAwait(false) != waitAllTask)
                    throw new ExecutionTimeoutException("The wait timeout was reached during the wait block.");
                    
                return result;
            }

            try
            {
                var task = executionTask();
                if (await Task.WhenAny(task, Task.Delay(_timeout)).ConfigureAwait(false) != task)
                    throw new ExecutionTimeoutException("The timeout was reached during execution."); 

                return task.Result;
            }
            catch(AggregateException ae) when (ae.InnerException is ExecutionException)
            {
                throw ae.InnerException;
            }
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>A `new` instance of <typeparamref name="TIExecutable"/> with the same settings as the invoking instance.</returns>
        public virtual TIExecutable Clone() => CreateFrom(this);
        IExecutable IExecutable.Clone() => Clone();

        /// <summary>
        /// Changes the shell of the execution context and returns a `new` context instance.
        /// </summary>
        /// <param name="shell">The new <see cref="IShell"/> to use.</param>
        /// <returns>A `new` instance of type <typeparamref name="TIExecutable"/> with the arguments passed to this call.</returns>
        public TIExecutable UseShell(IShell shell) => CreateFrom(this, shell: shell.Clone());
        IExecutable IExecutable.UseShell(IShell shell) => UseShell(shell);

        /// <summary>
        /// Changes the executable of the execution context and returns a `new` context instance.
        /// This should be used sparingly for very specific use cases (e.g., you renamed `kubectl` to `k`, and you need to reflect that).
        /// </summary>
        /// <param name="executable">The new executable to use.</param>
        /// <returns>A `new` instance of type <typeparamref name="TIExecutable"/> with the arguments passed to this call.</returns>
        public TIExecutable UseExecutable(string executable) => CreateFrom(this, executable: executable);
        IExecutable IExecutable.UseExecutable(string executable) => UseExecutable(executable);

        /// <summary>
        /// Adds an argument (which are appended space-separated to the execution command) to the execution context and returns a `new` context instance.
        /// </summary>
        /// <param name="args">An arbitrary list of strings to be added as parameters.</param>
        /// <returns>A `new` instance of <typeparamref name="TIExecutable"/> with the arguments passed to this call.</returns>
        public virtual TIExecutable WithArgument(params string[] args) => CreateFrom(this, arguments: Helpers.MergeEnumerables(_arguments, args));
        IExecutable IExecutable.WithArgument(params string[] args) => WithArgument(args);

        /// <summary>
        /// Sets the timeout on the entire execution of this entire execution context.
        /// </summary>
        /// <param name="timeout">The timeout.  The default value is ten (10) minutes.</param>
        /// <returns>A `new` instance of <typeparamref name="TIExecutable"/> with the timeout set to the value passed to this call.</returns>
        public virtual TIExecutable UseTimeout(TimeSpan timeout) => CreateFrom(this, timeout: timeout);
        IExecutable IExecutable.UseTimeout(TimeSpan timeout) => UseTimeout(timeout);

        /// <summary>
        /// Adds a string to the standard input stream (of which there may be many) to the executable context and returns a `new` context instance.
        /// </summary>
        /// <param name="standardInput">A string that gets passed to the standard input stream of the executable.</param>
        /// <returns>A `new` instance of type <typeparamref name="TIExecutable"/> with the standard input passed to this call.</returns>
        public TIExecutable WithStandardInput(string standardInput) => CreateFrom(this, shell: _shell.WithStandardInput(standardInput));
        IExecutable IExecutable.WithStandardInput(string standardInput) => WithStandardInput(standardInput);

        /// <summary>
        /// Adds a standard output handler (of which there may be many) to the execution context and returns a `new` context instance.
        /// </summary>
        /// <param name="standardOutputHandler">An <see cref="Action"/> that handles a new line in the standard output of the executable.</param>
        /// <returns>A `new` instance of <typeparamref name="TIExecutable"/> with the standard output handler passed to this call.</returns>
        public virtual TIExecutable WithStandardOutputHandler(Action<string> standardOutputHandler) => CreateFrom(this, shell: _shell.WithStandardOutputHandler(standardOutputHandler));
        IExecutable IExecutable.WithStandardOutputHandler(Action<string> standardOutputHandler) => WithStandardOutputHandler(standardOutputHandler);

        /// <summary>
        /// Adds an error output handler (of which there may be many) to the execution context and returns a `new` context instance.
        /// </summary>
        /// <param name="standardErrorHandler">An <see cref="Action"/> that handles a new line in the standard error of the executable.</param>
        /// <returns>A `new` instance of <typeparamref name="TIExecutable"/> with the standard error handler passed to this call.</returns>
        public virtual TIExecutable WithStandardErrorHandler(Action<string> standardErrorHandler) => CreateFrom(this, shell: _shell.WithStandardErrorHandler(standardErrorHandler));
        IExecutable IExecutable.WithStandardErrorHandler(Action<string> standardErrorHandler) => WithStandardErrorHandler(standardErrorHandler);

        /// <summary>
        /// Adds a (user) input request handler to the execution context and returns a `new` context instance.
        /// </summary>
        /// <param name="inputRequestHandler">
        /// A <see cref="Func{T}"/> that handles when the shell blocks for user input during an execution.
        /// This handler should take (string StandardOutput, string StandardInput) and return a <see cref="Task{String}"/>
        /// that will be passed to the executable as StandardInput.
        /// </param>
        /// <returns>A `new` instance of <typeparamref name="TIExecutable"/> with the request handler passed to this call.</returns>
        public TIExecutable UseInputRequestHandler(Func<string, string, Task<string>> inputRequestHandler) => CreateFrom(this, shell: _shell.UseInputRequestHandler(inputRequestHandler));
        IExecutable IExecutable.UseInputRequestHandler(Func<string, string, Task<string>> inputRequestHandler) => UseInputRequestHandler(inputRequestHandler);

        /// <summary>
        /// Set the Standard Output Encoding on the execution context and returns a `new` context instance.
        /// </summary>
        /// <param name="standardOutputEncoding">The encoding to use for standard output.</param>
        /// <returns>A `new` instance of type <see cref="IExecutable"/> with the standard output encoding passed to this call.</returns>
        public TIExecutable UseStandardOutputEncoding(Encoding standardOutputEncoding) => CreateFrom(this, shell: _shell.UseStandardOutputEncoding(standardOutputEncoding));
        IExecutable IExecutable.UseStandardOutputEncoding(Encoding standardOutputEncoding) => UseStandardOutputEncoding(standardOutputEncoding);

        /// <summary>
        /// Set the Standard Error Encoding on the execution context and returns a `new` context instance.
        /// </summary>
        /// <param name="standardErrorEncoding">The encoding to use for standard error.</param>
        /// <returns>A `new` instance of type <see cref="IExecutable"/> with the standard error encoding passed to this call.</returns>
        public TIExecutable UseStandardErrorEncoding(Encoding standardErrorEncoding) => CreateFrom(this, shell: _shell.UseStandardErrorEncoding(standardErrorEncoding));
        IExecutable IExecutable.UseStandardErrorEncoding(Encoding standardErrorEncoding) => UseStandardErrorEncoding(standardErrorEncoding);

        /// <summary>
        /// Provides an <see cref="IObservable{T}"/> to which a subscription can be placed.
        /// The observable never completes, since executions can be run many times.
        /// </summary>
        /// <returns>A `new` instance of type <typeparamref name="TIExecutable"/> with the subscribers attached to the observable.</returns>
        public TIExecutable WithSubscribe(Action<IObservable<ICommandEvent>> subscriber) => CreateFrom(this, shell: _shell.WithSubscribe(subscriber));
        IExecutable IExecutable.WithSubscribe(Action<IObservable<ICommandEvent>> subscriber) => WithSubscribe(subscriber);

        /// <summary>
        /// Adds a <see cref="CancellationToken"/> (of which there may be many) to the execution context and returns a `new` context instance.
        /// </summary>
        /// <returns>A `new` instance of type <typeparamref name="TIExecutable"/> with the cancellation token attached.</returns>
        public TIExecutable WithCancellationToken(CancellationToken cancellationToken) => CreateFrom(this, shell: _shell.WithCancellationToken(cancellationToken));
        IExecutable IExecutable.WithCancellationToken(CancellationToken cancellationToken) => WithCancellationToken(cancellationToken);

        /// <summary>
        /// Adds a wait <see cref="Func{T}"/> (of which there may be many) to the execution context and returns a `new` context instance.
        /// </summary>
        /// <param name="waitFunc">A <see cref="Func{T}"/> which takes an <see cref="ICommandResult"/> and returns a <see cref="Task"/> which will function as wait condition upon the completion of execution.</param>
        /// <returns>A `new` instance of <typeparamref name="TIExecutable"/> with the wait func passed to this call.</returns>
        public virtual TIExecutable WithWait(Func<ICommandResult, Task> waitFunc) => CreateFrom(this, waitFuncs: Helpers.MergeEnumerables(_waitFuncs, waitFunc.ToEnumerable()));
        IExecutable IExecutable.WithWait(Func<ICommandResult, Task> waitFunc) => WithWait(waitFunc);

        /// <summary>
        /// Sets the wait timeout on the <see cref="WithWait"/> <see cref="Func{T}"/>.
        /// </summary>
        /// <param name="timeout">The timeout.  The default value is ten (10) minutes.</param>
        /// <returns>A `new` instance of <typeparamref name="TIExecutable"/> with the wait timeout set to the value passed to this call.</returns>
        public virtual TIExecutable UseWaitTimeout(TimeSpan timeout) => CreateFrom(this, waitTimeout: timeout);
        IExecutable IExecutable.UseWaitTimeout(TimeSpan timeout) => UseWaitTimeout(timeout);

        /// <summary>
        /// Adds a key/value pair (of which there may be many) to the state of this execution context and returns a new context.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>A `new` instance of <typeparamref name="TIExecutable"/> with the state values passed to this call.</returns>
        protected virtual TIExecutable WithState(string key, object value) => CreateFrom(this, state: Helpers.MergeDictionaries(_state, (key, value).ToDictionary()));

        /// <summary>
        /// Ensures the execution context will not throw on a non-zero exit code and returns a `new` context instance.
        /// </summary>
        /// <returns>A `new` instance of type <typeparamref name="TIExecutable"/> that will not throw on a non-zero exit code.</returns>
        public TIExecutable UseNoThrow() => CreateFrom(this, shell: _shell.UseNoThrow());
        IExecutable IExecutable.UseNoThrow() => UseNoThrow();
    }
}