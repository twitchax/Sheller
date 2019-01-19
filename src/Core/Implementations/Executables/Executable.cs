
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Sheller.Models;

namespace Sheller.Implementations.Executables
{
    public class Executable<TResult> : IExecutable<TResult>
    {
        protected string _executable;
        protected IShell _shell;
        IEnumerable<string> _arguments;

        protected Func<ICommandResult, bool> _repeatPredicate;
        protected TimeSpan? _repeatInterval;
        protected int? _repeatCount;

        protected Func<ICommandResult, bool> _blockPredicate;
        protected TimeSpan? _blockTimeout;

        protected Func<ICommandResult, TResult> _resultSelector;

        protected Action<string> _dataHandler;
        protected Action<string> _errorHandler;

        public Executable(string executable, IShell shell, Func<ICommandResult, TResult> resultSelector) : this(executable, shell, null, null, null, null, null, null, resultSelector, null, null)
        {

        }
        
        public Executable(
            string executable, 
            IShell shell, 
            IEnumerable<string> arguments,
            Func<ICommandResult, bool> repeatPredicate,
            TimeSpan? repeatInterval,
            int? repeatCount,
            Func<ICommandResult, bool> blockPredicate, 
            TimeSpan? blockTimeout,
            Func<ICommandResult, TResult> resultSelector,
            Action<string> dataHandler,
            Action<string> errorHandler)
        {
            _executable = executable;
            _shell = shell;
            _arguments = arguments ?? new List<String>();

            _repeatPredicate = repeatPredicate;
            _repeatInterval = repeatInterval;
            _repeatCount = repeatCount;
            _blockPredicate = blockPredicate;
            _blockTimeout = blockTimeout;

            _resultSelector = resultSelector ?? throw new Exception("The `resultSelector` must not be null.");

            _dataHandler = dataHandler;
            _errorHandler = errorHandler;
        }
        
        public void SetShell(IShell shell) => _shell = shell;

        public Task<TResult> ExecuteAsync() => ExecuteAsyncImpl();
        protected async virtual Task<TResult> ExecuteAsyncImpl()
        {
            var command = _shell.Path;
            var commandArguments = _shell.GetCommandArgument($"{_executable} {string.Join(" ", _arguments)}");
            
            // TODO: Repeat semantics.

            var commandResult = await Helpers.RunCommand(
                command, 
                commandArguments, 
                _shell.EnvironmentVariables,
                _dataHandler, 
                _errorHandler/*,loggers)*/);

            var result = _resultSelector(commandResult);

            // TODO: Block semantics.

            return result;
        }

        public IExecutableBase<TResult> WithArgument(params string[] args) => WithArgumentImpl(args);
        protected virtual Executable<TResult> WithArgumentImpl(params string[] args)
        {
            return new Executable<TResult>(
                _executable,
                _shell,
                Helpers.MergeEnumerables(_arguments, args),
                _repeatPredicate, _repeatInterval, _repeatCount,
                _blockPredicate, _blockTimeout,
                _resultSelector,
                _dataHandler, _errorHandler);
        }
        

        public IExecutableBaseWithWait<TResult> WithBlockUntil(Func<ICommandResult, bool> predicate, TimeSpan timeout) => this.WithBlockUntilImpl(predicate, timeout);
        IExecutableBaseWithWaitAndResultSelector<TResult> IExecutableBaseWithResultSelector<TResult>.WithBlockUntil(Func<ICommandResult, bool> predicate, TimeSpan timeout) => this.WithBlockUntilImpl(predicate, timeout);
        IExecutableBaseWithWaitAndHandlers<TResult> IExecutableBaseWithHandlers<TResult>.WithBlockUntil(Func<ICommandResult, bool> predicate, TimeSpan timeout) => this.WithBlockUntilImpl(predicate, timeout);
        IExecutableWithWaitAndResultSelectorAndHandlers<TResult> IExecutableBaseWithResultSelectorAndHandlers<TResult>.WithBlockUntil(Func<ICommandResult, bool> predicate, TimeSpan timeout) => this.WithBlockUntilImpl(predicate, timeout);
        protected virtual Executable<TResult> WithBlockUntilImpl(Func<ICommandResult, bool> predicate, TimeSpan timeout)
        {
            return new Executable<TResult>(
                _executable,
                _shell,
                _arguments,
                _repeatPredicate, _repeatInterval, _repeatCount,
                predicate, timeout,
                _resultSelector,
                _dataHandler, _errorHandler);
        }

        public IExecutableBaseWithWait<TResult> WithRepeatUntil(Func<ICommandResult, bool> predicate, TimeSpan interval, int count) => WithRepeatUntilImpl(predicate, interval, count);
        IExecutableBaseWithWaitAndResultSelector<TResult> IExecutableBaseWithResultSelector<TResult>.WithRepeatUntil(Func<ICommandResult, bool> predicate, TimeSpan interval, int count) => WithRepeatUntilImpl(predicate, interval, count);
        IExecutableBaseWithWaitAndHandlers<TResult> IExecutableBaseWithHandlers<TResult>.WithRepeatUntil(Func<ICommandResult, bool> predicate, TimeSpan interval, int count) => WithRepeatUntilImpl(predicate, interval, count);
        IExecutableWithWaitAndResultSelectorAndHandlers<TResult> IExecutableBaseWithResultSelectorAndHandlers<TResult>.WithRepeatUntil(Func<ICommandResult, bool> predicate, TimeSpan interval, int count) => WithRepeatUntilImpl(predicate, interval, count);
        protected virtual Executable<TResult> WithRepeatUntilImpl(Func<ICommandResult, bool> predicate, TimeSpan interval, int count)
        {
            return new Executable<TResult>(
                _executable,
                _shell,
                _arguments,
                predicate, interval, count,
                _blockPredicate, _blockTimeout,
                _resultSelector,
                _dataHandler, _errorHandler);
        }

        public IExecutableBaseWithResultSelector<TNewResult> WithResultSelector<TNewResult>(Func<ICommandResult, TNewResult> selector) => WithResultSelectorImpl(selector);
        IExecutableBaseWithWaitAndResultSelector<TNewResult> IExecutableBaseWithWait<TResult>.WithResultSelector<TNewResult>(Func<ICommandResult, TNewResult> selector) => WithResultSelectorImpl(selector);
        IExecutableBaseWithResultSelectorAndHandlers<TNewResult> IExecutableBaseWithHandlers<TResult>.WithResultSelector<TNewResult>(Func<ICommandResult, TNewResult> selector) => WithResultSelectorImpl(selector);
        IExecutableWithWaitAndResultSelectorAndHandlers<TNewResult> IExecutableBaseWithWaitAndHandlers<TResult>.WithResultSelector<TNewResult>(Func<ICommandResult, TNewResult> selector) => WithResultSelectorImpl(selector);
        protected virtual Executable<TNewResult> WithResultSelectorImpl<TNewResult>(Func<ICommandResult, TNewResult> selector)
        {
            return new Executable<TNewResult>(
                _executable,
                _shell,
                _arguments,
                _repeatPredicate, _repeatInterval, _repeatCount,
                _blockPredicate, _blockTimeout,
                selector,
                _dataHandler, _errorHandler);
        }

        public IExecutableBaseWithHandlers<TResult> WithHandlers(Action<string> dataHandler, Action<string> errorHandler) => WithHandlersImpl(dataHandler, errorHandler);
        IExecutableBaseWithWaitAndHandlers<TResult> IExecutableBaseWithWait<TResult>.WithHandlers(Action<string> dataHandler, Action<string> errorHandler) => WithHandlersImpl(dataHandler, errorHandler);
        IExecutableBaseWithResultSelectorAndHandlers<TResult> IExecutableBaseWithResultSelector<TResult>.WithHandlers(Action<string> dataHandler, Action<string> errorHandler) => WithHandlersImpl(dataHandler, errorHandler);
        IExecutableWithWaitAndResultSelectorAndHandlers<TResult> IExecutableBaseWithWaitAndResultSelector<TResult>.WithHandlers(Action<string> dataHandler, Action<string> errorHandler) => WithHandlersImpl(dataHandler, errorHandler);
        protected virtual Executable<TResult> WithHandlersImpl(Action<string> dataHandler, Action<string> errorHandler)
        {
            return new Executable<TResult>(
                _executable,
                _shell,
                _arguments,
                _repeatPredicate, _repeatInterval, _repeatCount,
                _blockPredicate, _blockTimeout,
                _resultSelector,
                dataHandler, errorHandler);
        }
    }
}