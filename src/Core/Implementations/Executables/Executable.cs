using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Sheller.Models;

namespace Sheller.Implementations.Executables
{
    /// <summary>
    /// The implementation base class for well-known executables (i.e., defines its own executable string) with a custom result.
    /// </summary>
    /// <typeparam name="TExecutable">The type of the executable class implementing this interface.</typeparam>
    /// <typeparam name="TResult">The result type of the executable.</typeparam>
    public abstract class Executable<TExecutable, TResult> : Executable<TExecutable>, IExecutable<TExecutable, TResult> where TExecutable : Executable<TExecutable, TResult>, new()
    {
        /// <summary>
        /// Executes the executable.
        /// </summary>
        /// <returns>A task which results in a <typeparamref name="TResult"/> (i.e., the result of the execution).</returns>
        public abstract new Task<TResult> ExecuteAsync();
    }

    /// <summary>
    /// The implementation base class for well-known executables (i.e., defines its own executable string).
    /// </summary>
    /// <typeparam name="TExecutable">The type of the executable class implementing this interface.</typeparam>
    public abstract class Executable<TExecutable> : ExecutableBase<TExecutable> where TExecutable : Executable<TExecutable>, new()
    {
        /// <summary>
        /// Initializes this instance with the provided shell.
        /// </summary>
        /// <param name="shell">The shell in which the executable should run.</param>
        /// <returns>This instance.</returns>
        public abstract TExecutable Initialize(IShell shell);
    }

    /// <summary>
    /// The implementation base class for generic executables with a custom result.
    /// </summary>
    /// <typeparam name="TExecutable">The type of the executable class implementing this interface.</typeparam>
    /// <typeparam name="TResult">The result type of the executable.</typeparam>
    public abstract class ExecutableBase<TExecutable, TResult> : ExecutableBase<TExecutable>, IExecutable<TExecutable, TResult> where TExecutable : ExecutableBase<TExecutable, TResult>, new()
    {
        /// <summary>
        /// Executes the executable.
        /// </summary>
        /// <returns>A task which results in an <typeparamref name="TResult"/> (i.e., the result of the execution).</returns>
        public abstract new Task<TResult> ExecuteAsync();
    }

    /// <summary>
    /// The implementation base class for generic executables.
    /// </summary>
    /// <typeparam name="TExecutable">The type of the executable class implementing this interface.</typeparam>
    public abstract class ExecutableBase<TExecutable> : IExecutable<TExecutable> where TExecutable : ExecutableBase<TExecutable>, new()
    {
        private string _executable;
        private IShell _shell;

        private IEnumerable<string> _arguments;

        private TimeSpan _timeout;

        private IEnumerable<Func<ICommandResult, Task>> _waitFuncs;
        private TimeSpan _waitTimeout;

        private IDictionary<string, object> _state;

        protected IDictionary<string, object> State => _state;

        /// <summary>
        /// Initializes this instance with the provided shell.
        /// </summary>
        /// <param name="executable">The name or path of the executable to run.</param>
        /// <param name="shell">The shell in which the executable should run.</param>
        /// <returns>This instance.</returns>
        public TExecutable Initialize(string executable, IShell shell) => Initialize(executable, shell, null, null, null, null, null);
        
        /// <summary>
        /// Initializes this instance with the provided shell.
        /// </summary>
        /// <param name="executable">The name or path of the executable to run.</param>
        /// <param name="shell">The shell in which the executable should run.</param>
        /// <param name="arguments">The arguments to pass to the executable.</param>
        /// <param name="timeout">The timeout of the execution.</param>
        /// <param name="waitFuncs">The wait functions which block execution after the executable execution.</param>
        /// <param name="waitTimeout">The timeout of the wait functions.</param>
        /// <returns>This instance.</returns>
        protected virtual TExecutable Initialize(
            string executable, 
            IShell shell, 
            IEnumerable<string> arguments,
            TimeSpan? timeout,
            IEnumerable<Func<ICommandResult, Task>> waitFuncs, 
            TimeSpan? waitTimeout,
            IDictionary<string, object> state)
        {
            _executable = executable ?? throw new InvalidOperationException("This executable definition does not define an executable.");;
            _shell = shell?.Clone() ?? throw new InvalidOperationException("This executable definition does not define a shell.");;

            _arguments = arguments ?? new List<String>();

            _timeout = timeout ?? TimeSpan.FromMinutes(10);

            _waitFuncs = waitFuncs ?? new List<Func<ICommandResult, Task>>();
            _waitTimeout = waitTimeout ?? TimeSpan.FromMinutes(10);

            _state = state ?? new Dictionary<string, object>();

            return this as TExecutable;
        }

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
        /// <returns>A task which results in a <typeparamref name="TResult"/> (i.e., the result of the execution).</returns>
        public async virtual Task<TResult> ExecuteAsync<TResult>(Func<ICommandResult, Task<TResult>> resultSelector)
        {
            Func<Task<TResult>> executionTask = async () =>
            {
                var commandResult = await _shell.ExecuteCommandAsync(_executable, _arguments);

                var result = await resultSelector(commandResult);

                // Await (ALL of the wait funcs) OR (the wait timeout), whichever comes first.
                var waitAllTask = Task.WhenAll(_waitFuncs.Select(f => f(commandResult)));
                if (await Task.WhenAny(waitAllTask, Task.Delay(_waitTimeout)) != waitAllTask)
                    throw new Exception("The wait timeout was reached during the wait block.");
                    
                return result;
            };
            
            var task = executionTask();
            if (await Task.WhenAny(task, Task.Delay(_timeout)) != task)
                throw new Exception("The wait timeout was reached during the wait block.");
                
            return task.Result;
        }

        public virtual TExecutable Clone() => 
            new TExecutable().Initialize(
                _executable,
                _shell,
                _arguments,
                _timeout,
                _waitFuncs, _waitTimeout,
                _state
            );
        IExecutable IExecutable.Clone() => Clone();

        /// <summary>
        /// Adds an argument (which are appended space-separated to the execution command) to the execution context and returns a `new` context instance.
        /// </summary>
        /// <param name="args">An arbitrary list of strings to be added as parameters.</param>
        /// <returns>A `new` instance of <typeparamref name="TExecutable"/> with the arguments passed to this call.</returns>
        public virtual TExecutable WithArgument(params string[] args) => 
            new TExecutable().Initialize(
                _executable,
                _shell,
                Helpers.MergeEnumerables(_arguments, args),
                _timeout,
                _waitFuncs, _waitTimeout,
                _state
            );
        IExecutable IExecutable.WithArgument(params string[] args) => WithArgument(args);

        /// <summary>
        /// Sets the timeout on the entire execution of this entire execution context.
        /// </summary>
        /// <param name="timeout">The timeout.  The default value is ten (10) minutes.</param>
        /// <returns>A `new` instance of <typeparamref name="TExecutable"/> with the timeout set to the value passed to this call.</returns>
        public virtual TExecutable WithTimeout(TimeSpan timeout) => 
            new TExecutable().Initialize(
                _executable,
                _shell,
                _arguments,
                timeout,
                _waitFuncs, _waitTimeout,
                _state
            );
        IExecutable IExecutable.WithTimeout(TimeSpan timeout) => WithTimeout(timeout);

        /// <summary>
        /// Adds a standard output handler (of which there may be many) to the execution context and returns a `new` context instance.
        /// </summary>
        /// <param name="standardOutputHandler">An <see cref="Action"/> that handles a new line in the standard output of the executable.</param>
        /// <returns>A `new` instance of <typeparamref name="TExecutable"/> with the standard output handler passed to this call.</returns>
        public virtual TExecutable WithStandardOutputHandler(Action<string> standardOutputHandler) => 
            new TExecutable().Initialize(
                _executable,
                _shell.WithStandardOutputHandler(standardOutputHandler),
                _arguments,
                _timeout,
                _waitFuncs, _waitTimeout,
                _state
            );
        IExecutable IExecutable.WithStandardOutputHandler(Action<string> standardOutputHandler) => WithStandardOutputHandler(standardOutputHandler);

        /// <summary>
        /// Adds an error output handler (of which there may be many) to the execution context and returns a `new` context instance.
        /// </summary>
        /// <param name="standardErrorHandler">An <see cref="Action"/> that handles a new line in the standard error of the executable.</param>
        /// <returns>A `new` instance of <typeparamref name="TExecutable"/> with the standard error handler passed to this call.</returns>
        public virtual TExecutable WithStandardErrorHandler(Action<string> standardErrorHandler) => 
            new TExecutable().Initialize(
                _executable,
                _shell.WithStandardErrorHandler(standardErrorHandler),
                _arguments,
                _timeout,
                _waitFuncs, _waitTimeout,
                _state
            );
        IExecutable IExecutable.WithStandardErrorHandler(Action<string> standardErrorHandler) => WithStandardErrorHandler(standardErrorHandler);

        /// <summary>
        /// Adds a wait <see cref="Func{T}"/> (of which there may be many) to the execution context and returns a `new` context instance.
        /// </summary>
        /// <param name="waitFunc">A <see cref="Func{T}"/> which takes an <see cref="ICommandResult"/> and returns a <see cref="Task"/> which will function as wait condition upon the completion of execution.</param>
        /// <returns>A `new` instance of <typeparamref name="TExecutable"/> with the wait func passed to this call.</returns>
        public virtual TExecutable WithWait(Func<ICommandResult, Task> waitFunc) => 
            new TExecutable().Initialize(
                _executable,
                _shell,
                _arguments,
                _timeout,
                Helpers.MergeEnumerables(_waitFuncs, waitFunc.ToEnumerable()), _waitTimeout,
                _state
            );
        IExecutable IExecutable.WithWait(Func<ICommandResult, Task> waitFunc) => WithWait(waitFunc);

        /// <summary>
        /// Sets the wait timeout on the <see cref="WithWait"/> <see cref="Func{T}"/>.
        /// </summary>
        /// <param name="timeout">The timeout.  The default value is ten (10) minutes.</param>
        /// <returns>A `new` instance of <typeparamref name="TExecutable"/> with the wait timeout set to the value passed to this call.</returns>
        public virtual TExecutable WithWaitTimeout(TimeSpan timeout) => 
            new TExecutable().Initialize(
                _executable,
                _shell,
                _arguments,
                _timeout,
                _waitFuncs, timeout,
                _state
            );
        IExecutable IExecutable.WithWaitTimeout(TimeSpan timeout) => WithWaitTimeout(timeout);

        protected virtual TExecutable WithState(string key, object value) =>
            new TExecutable().Initialize(
                _executable,
                _shell,
                _arguments,
                _timeout,
                _waitFuncs, _timeout,
                Helpers.MergeDictionaries(_state, (key, value).ToDictionary())
            );
    }
}