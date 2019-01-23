
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Sheller.Models;

namespace Sheller.Implementations.Executables
{
    public abstract class Executable<TExecutable, TResult> : Executable<TExecutable>, IExecutable<TExecutable, TResult> where TExecutable : ExecutableBase<TExecutable>, new()
    {
        public abstract new Task<TResult> ExecuteAsync();
    }

    public abstract class Executable<TExecutable> : ExecutableBase<TExecutable> where TExecutable : ExecutableBase<TExecutable>, new()
    { 
        public abstract TExecutable Initialize(IShell shell);
    }

    public abstract class ExecutableBase<TExecutable, TResult> : ExecutableBase<TExecutable>, IExecutable<TExecutable, TResult> where TExecutable : ExecutableBase<TExecutable>, new()
    {
        public abstract new Task<TResult> ExecuteAsync();
    }

    public abstract class ExecutableBase<TExecutable> : IExecutable<TExecutable> where TExecutable : ExecutableBase<TExecutable>, new()
    {
        protected string _executable;
        protected IShell _shell;

        protected IEnumerable<string> _arguments;
        
        protected IEnumerable<Action<string>> _standardOutputHandlers;
        protected IEnumerable<Action<string>> _standardErrorHandlers;

        protected IEnumerable<Func<ICommandResult, Task>> _waitFuncs;
        protected TimeSpan _waitTimeout;

        public virtual TExecutable Initialize(string executable, IShell shell) => Initialize(executable, shell, null, null, null, null, null);
        public virtual TExecutable Initialize(
            string executable, 
            IShell shell, 
            IEnumerable<string> arguments,
            IEnumerable<Action<string>> standardOutputHandlers,
            IEnumerable<Action<string>> standardErrorHandlers,
            IEnumerable<Func<ICommandResult, Task>> waitFuncs, 
            TimeSpan? waitTimeout
        )
        {
            _executable = executable ?? throw new InvalidOperationException("This executable definition does not define an executable.");;
            _shell = shell ?? throw new InvalidOperationException("This executable definition does not define a shell.");;

            _arguments = arguments ?? new List<String>();

            _standardOutputHandlers = standardOutputHandlers ?? new List<Action<string>>();
            _standardErrorHandlers = standardErrorHandlers ?? new List<Action<string>>();

            _waitFuncs = waitFuncs ?? new List<Func<ICommandResult, Task>>();
            _waitTimeout = waitTimeout ?? TimeSpan.FromMinutes(1);

            return this as TExecutable;
        }

        // NOTE that there is no timeout on the command or the result selector.  The wait only applies to `WithWait`.
        public virtual Task<ICommandResult> ExecuteAsync() => ExecuteAsync(cr => cr);
        public virtual Task<TResult> ExecuteAsync<TResult>(Func<ICommandResult, TResult> resultSelector) => ExecuteAsync(cr => Task.FromResult(resultSelector(cr)));
        public async virtual Task<TResult> ExecuteAsync<TResult>(Func<ICommandResult, Task<TResult>> resultSelector)
        {
            var command = _shell.Path;
            var commandArguments = _shell.GetCommandArgument($"{_executable} {string.Join(" ", _arguments)}");

            Console.WriteLine(command);
            var commandResult = await Helpers.RunCommand(
                command, 
                commandArguments,
                _standardOutputHandlers, 
                _standardErrorHandlers);

            var result = await resultSelector(commandResult);

            // TODO: throw an exception when the timeout is hit?
            // TODO: add an "overall" timeout?

            // Await (ALL of the wait funcs) OR (the wait timeout), whichever comes first.
            await Task.WhenAny(
                Task.WhenAll(_waitFuncs.Select(f => f(commandResult))),
                Task.Delay(_waitTimeout)
            );
            
            return result;
        }

        public virtual TExecutable WithArgument(params string[] args)
        {
            var result = new TExecutable();
            result.Initialize(
                _executable,
                _shell,
                Helpers.MergeEnumerables(_arguments, args),
                _standardOutputHandlers, _standardErrorHandlers,
                _waitFuncs, _waitTimeout
            );

            return result;
        }

        public virtual TExecutable WithStandardOutputHandler(Action<string> standardOutputHandler)
        {
            var result = new TExecutable();
            result.Initialize(
                _executable,
                _shell,
                _arguments,
                Helpers.MergeEnumerables(_standardOutputHandlers, standardOutputHandler.ToEnumerable()), _standardErrorHandlers,
                _waitFuncs, _waitTimeout
            );

            return result;
        }

        public virtual TExecutable WithStandardErrorHandler(Action<string> standardErrorHandler)
        {
            var result = new TExecutable();
            result.Initialize(
                _executable,
                _shell,
                _arguments,
                _standardOutputHandlers, Helpers.MergeEnumerables(_standardErrorHandlers, standardErrorHandler.ToEnumerable()),
                _waitFuncs, _waitTimeout
            );

            return result;
        }

        public TExecutable WithWait(Func<ICommandResult, Task> waitFunc)
        {
            var result = new TExecutable();
            result.Initialize(
                _executable,
                _shell,
                _arguments,
                _standardOutputHandlers, _standardErrorHandlers,
                Helpers.MergeEnumerables(_waitFuncs, waitFunc.ToEnumerable()), _waitTimeout
            );

            return result;
        }

        public TExecutable WithWaitTimeout(TimeSpan timeout)
        {
            var result = new TExecutable();
            result.Initialize(
                _executable,
                _shell,
                _arguments,
                _standardOutputHandlers, _standardErrorHandlers,
                _waitFuncs, timeout
            );

            return result;
        }
    }
}