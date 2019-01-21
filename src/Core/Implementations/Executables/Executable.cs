
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Sheller.Models;

namespace Sheller.Implementations.Executables
{
    public abstract class Executable<TExecutable, TResult> : Executable<TExecutable>, IExecutable<TExecutable, TResult> where TExecutable : ExecutableBase<TExecutable>, new()
    {
        public abstract Task<TResult> ExecuteAsync();
    }

    public abstract class Executable<TExecutable> : ExecutableBase<TExecutable> where TExecutable : ExecutableBase<TExecutable>, new()
    {
        public abstract TExecutable Initialize(IShell shell);
    }

    public abstract class ExecutableBase<TExecutable, TResult> : ExecutableBase<TExecutable>, IExecutable<TExecutable, TResult> where TExecutable : ExecutableBase<TExecutable>, new()
    {
        public abstract Task<TResult> ExecuteAsync();
    }

    public abstract class ExecutableBase<TExecutable> : IExecutable<TExecutable> where TExecutable : ExecutableBase<TExecutable>, new()
    {
        protected string _executable;
        protected IShell _shell;

        protected IEnumerable<string> _arguments;
        
        protected IEnumerable<Action<string>> _standardOutputHandlers;
        protected IEnumerable<Action<string>> _standardErrorHandlers;

        // protected IEnumerable<Func<ICommandResult, Task<bool>>> _repeatPredicate;
        // protected TimeSpan? _repeatInterval;
        // protected int? _repeatCount;

        // protected IEnumerable<Func<ICommandResult, Task<bool>>> _blockPredicate;
        // protected TimeSpan? _blockTimeout;

        public virtual TExecutable Initialize(string executable, IShell shell) => Initialize(executable, shell, null, null, null);
        public virtual TExecutable Initialize(
            string executable, 
            IShell shell, 
            IEnumerable<string> arguments,
            IEnumerable<Action<string>> standardOutputHandlers,
            IEnumerable<Action<string>> standardErrorHandlers//,
            // IEnumerable<Func<ICommandResult, Task<bool>>> repeatPredicate,
            // TimeSpan? repeatInterval,
            // int? repeatCount,
            // IEnumerable<Func<ICommandResult, Task<bool>>> blockPredicate, 
            // TimeSpan? blockTimeout
        )
        {
            _executable = executable;
            _shell = shell;

            _arguments = arguments ?? new List<String>();

            _standardOutputHandlers = standardOutputHandlers ?? new List<Action<string>>();
            _standardErrorHandlers = standardErrorHandlers ?? new List<Action<string>>();

            // _repeatPredicate = repeatPredicate ?? new List<Func<ICommandResult, Task<bool>>>();
            // _repeatInterval = repeatInterval;
            // _repeatCount = repeatCount;

            // _blockPredicate = blockPredicate ?? new List<Func<ICommandResult, Task<bool>>>();
            // _blockTimeout = blockTimeout;

            return this as TExecutable;
        }

        public virtual Task<ICommandResult> ExecuteAsync() => ExecuteAsync(cr => cr);
        public virtual Task<TResult> ExecuteAsync<TResult>(Func<ICommandResult, TResult> resultSelector) => ExecuteAsync(cr => Task.FromResult(resultSelector(cr)));
        public async virtual Task<TResult> ExecuteAsync<TResult>(Func<ICommandResult, Task<TResult>> resultSelector)
        {
            if(_executable == null)
                throw new InvalidOperationException("This executable definition does not define an executable.");
            if(_shell == null)
                throw new InvalidOperationException("This executable definition does not define a shell.");

            var command = _shell.Path;
            var commandArguments = _shell.GetCommandArgument($"{_executable} {string.Join(" ", _arguments)}");
            
            // TODO: Repeat semantics.

            var commandResult = await Helpers.RunCommand(
                command, 
                commandArguments,
                _standardOutputHandlers, 
                _standardErrorHandlers/*,loggers)*/);

            var result = await resultSelector(commandResult);

            // TODO: Block semantics.

            return result;
        }

        public virtual TExecutable WithArgument(params string[] args)
        {
            var result = new TExecutable();
            result.Initialize(
                _executable,
                _shell,
                Helpers.MergeEnumerables(_arguments, args),
                _standardOutputHandlers, _standardErrorHandlers
                // _repeatPredicate, _repeatInterval, _repeatCount,
                // _blockPredicate, _blockTimeout,
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
                Helpers.MergeEnumerables(_standardOutputHandlers, standardOutputHandler.ToEnumerable()), _standardErrorHandlers
                // _repeatPredicate, _repeatInterval, _repeatCount,
                // _blockPredicate, _blockTimeout,
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
                _standardOutputHandlers, Helpers.MergeEnumerables(_standardErrorHandlers, standardErrorHandler.ToEnumerable())
                // _repeatPredicate, _repeatInterval, _repeatCount,
                // _blockPredicate, _blockTimeout,
            );

            return result;
        }
    }
}