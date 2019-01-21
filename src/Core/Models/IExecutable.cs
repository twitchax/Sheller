
using System;
using System.Threading.Tasks;

namespace Sheller.Models
{
    /// <summary>
    /// The top-level interface for executables.
    /// </summary>
    public interface IExecutable<TExecutable> where TExecutable : IExecutable<TExecutable>
    {
        // TExecutable Make(IShell shell);
        // TExecutable Make(string executable, IShell shell) => Make(executable, shell, null, null, null);
        // TExecutable Make(
        //     string executable, 
        //     IShell shell, 
        //     IEnumerable<string> arguments,
        //     IEnumerable<Action<string>> standardOutputHandlers,
        //     IEnumerable<Action<string>> standardErrorHandlers//,
        //     // IEnumerable<Func<ICommandResult, Task<bool>>> repeatPredicate,
        //     // TimeSpan? repeatInterval,
        //     // int? repeatCount,
        //     // IEnumerable<Func<ICommandResult, Task<bool>>> blockPredicate, 
        //     // TimeSpan? blockTimeout
        // );

        Task<ICommandResult> ExecuteAsync();
        Task<TResult> ExecuteAsync<TResult>(Func<ICommandResult, TResult> resultSelector);
        Task<TResult> ExecuteAsync<TResult>(Func<ICommandResult, Task<TResult>> resultSelector);

        TExecutable WithArgument(params string[] args);

        TExecutable WithStandardOutputHandler(Action<string> standardOutputHandler);
        TExecutable WithStandardErrorHandler(Action<string> standardErrorHandler);

        // IExecutable WithRepeatUntil(Func<ICommandResult, Task<bool>> predicate);
        // IExecutable WithRepeatUntil(Func<ICommandResult, bool> predicate);
        // IExecutable WithRepeatInterval(TimeSpan interval);
        // IExecutable WithRepeatCount(int count);

        // IExecutable WithBlockUntil(Func<ICommandResult, Task<bool>> predicate);
        // IExecutable WithBlockUntil(Func<ICommandResult, bool> predicate);
        // IExecutable WithBlockTimeout(TimeSpan timeout);
    }

    /// <summary>
    /// The shared interface for executables that define the execute method.
    /// </summary>
    /// <typeparam name="TResult">The result type of the executable.</typeparam>
    public interface IExecutable<TExecutable, TResult> : IExecutable<TExecutable> where TExecutable : IExecutable<TExecutable>
    {
        Task<TResult> ExecuteAsync();
    }
}