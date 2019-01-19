
using System;
using System.Threading.Tasks;

namespace Sheller.Models
{
    public interface IExecutable
    {
        void SetShell(IShell shell);
    }

    public interface IExecutableShared<TResult> : IExecutable
    {
        Task<TResult> ExecuteAsync();
    }

    public interface IExecutableBase<TResult> : IExecutableShared<TResult>
    {
        // For docs => argument must come before all others.
        IExecutableBase<TResult> WithArgument(params string[] args);
        IExecutableBaseWithWait<TResult> WithRepeatUntil(Func<ICommandResult, bool> predicate, TimeSpan interval, int count);
        IExecutableBaseWithWait<TResult> WithBlockUntil(Func<ICommandResult, bool> predicate, TimeSpan timeout);
        IExecutableBaseWithResultSelector<TNewResult> WithResultSelector<TNewResult>(Func<ICommandResult, TNewResult> selector);
        IExecutableBaseWithHandlers<TResult> WithHandlers(Action<string> dataHandler, Action<string> errorHandler);
    }

    public interface IExecutableBaseWithWait<TResult> : IExecutableShared<TResult>
    {
        IExecutableBaseWithWaitAndResultSelector<TNewResult> WithResultSelector<TNewResult>(Func<ICommandResult, TNewResult> selector);
        IExecutableBaseWithWaitAndHandlers<TResult> WithHandlers(Action<string> dataHandler, Action<string> errorHandler);
    }

    public interface IExecutableBaseWithResultSelector<TResult> : IExecutableShared<TResult>
    {
        IExecutableBaseWithWaitAndResultSelector<TResult> WithRepeatUntil(Func<ICommandResult, bool> predicate, TimeSpan interval, int count);
        IExecutableBaseWithWaitAndResultSelector<TResult> WithBlockUntil(Func<ICommandResult, bool> predicate, TimeSpan timeout);
        IExecutableBaseWithResultSelectorAndHandlers<TResult> WithHandlers(Action<string> dataHandler, Action<string> errorHandler);
    }

    public interface IExecutableBaseWithHandlers<TResult> : IExecutableShared<TResult>
    {
        IExecutableBaseWithWaitAndHandlers<TResult> WithRepeatUntil(Func<ICommandResult, bool> predicate, TimeSpan interval, int count);
        IExecutableBaseWithWaitAndHandlers<TResult> WithBlockUntil(Func<ICommandResult, bool> predicate, TimeSpan timeout);
        IExecutableBaseWithResultSelectorAndHandlers<TNewResult> WithResultSelector<TNewResult>(Func<ICommandResult, TNewResult> selector);
    }

    public interface IExecutableBaseWithWaitAndResultSelector<TResult> : IExecutableShared<TResult>
    {
        IExecutableWithWaitAndResultSelectorAndHandlers<TResult> WithHandlers(Action<string> dataHandler, Action<string> errorHandler);
    }

    public interface IExecutableBaseWithWaitAndHandlers<TResult> : IExecutableShared<TResult>
    {
        IExecutableWithWaitAndResultSelectorAndHandlers<TNewResult> WithResultSelector<TNewResult>(Func<ICommandResult, TNewResult> selector);
    }

    public interface IExecutableBaseWithResultSelectorAndHandlers<TResult> : IExecutableShared<TResult>
    {
        IExecutableWithWaitAndResultSelectorAndHandlers<TResult> WithRepeatUntil(Func<ICommandResult, bool> predicate, TimeSpan interval, int count);
        IExecutableWithWaitAndResultSelectorAndHandlers<TResult> WithBlockUntil(Func<ICommandResult, bool> predicate, TimeSpan timeout);
    }

    public interface IExecutableWithWaitAndResultSelectorAndHandlers<TResult> : IExecutableShared<TResult>
    {

    }

    public interface IExecutable<TResult> : 
        IExecutableBase<TResult>,
        IExecutableBaseWithWait<TResult>,
        IExecutableBaseWithResultSelector<TResult>,
        IExecutableBaseWithHandlers<TResult>,
        IExecutableBaseWithWaitAndResultSelector<TResult>,
        IExecutableBaseWithWaitAndHandlers<TResult>,
        IExecutableBaseWithResultSelectorAndHandlers<TResult>,
        IExecutableWithWaitAndResultSelectorAndHandlers<TResult>
    {
        
    }
}