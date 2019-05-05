using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Sheller.Implementations.Executables;

namespace Sheller.Models
{
    /// <summary>
    /// The interface for defining a shell.
    /// </summary>
    public interface IShell
    {
        /// <summary>
        /// Executes a command and arguments in the specified shell.
        /// </summary>
        /// <param name="executable">The executable name or path/</param>
        /// <param name="arguments">The arguments to be passed to the executable (which will be space-separated).</param>
        /// <returns>A task which results in an <see cref="ICommandResult"/> (i.e., the result of the command execution).</returns>
        Task<ICommandResult> ExecuteCommandAsync(string executable, IEnumerable<string> arguments);

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>A `new` instance of <see cref="IShell"/> with the same settings as the invoking instance.</returns>
        IShell Clone();

        /// <summary>
        /// Adds an environment variable (of which there may be many) to the shell context and returns a `new` context instance.
        /// </summary>
        /// <param name="key">The environment variable key.</param>
        /// <param name="value">The environment variable value.</param>
        /// <returns>A `new` instance of type <see cref="IShell"/> with the environment variable passed in this call.</returns>
        IShell WithEnvironmentVariable(string key, string value);
        /// <summary>
        /// Adds a list of environment variables (of which there may be many) to the shell context and returns a `new` context instance.
        /// </summary>
        /// <param name="variables">The list of key value pairs of environment variables.</param>
        /// <returns>A `new` instance of type <see cref="IShell"/> with the environment variables passed in this call.</returns>
        IShell WithEnvironmentVariables(IEnumerable<KeyValuePair<string, string>> variables);
        /// <summary>
        /// Adds a list of environment variables (of which there may be many) to the shell context and returns a `new` context instance.
        /// </summary>
        /// <param name="variables">The list of tuple of environment variables.</param>
        /// <returns>A `new` instance of type <see cref="IShell"/> with the environment variables passed in this call.</returns>
        IShell WithEnvironmentVariables(IEnumerable<(string, string)> variables);

        /// <summary>
        /// Adds a string to the standard input stream (of which there may be many) to the shell context and returns a `new` context instance.
        /// </summary>
        /// <param name="standardInput">A string that gets passed to the standard input stream of the executable.</param>
        /// <returns>A `new` instance of type <see cref="IShell"/> with the standard input passed to this call.</returns>
        IShell WithStandardInput(string standardInput);
        /// <summary>
        /// Adds a standard output handler (of which there may be many) to the shell context and returns a `new` context instance.
        /// </summary>
        /// <param name="standardOutputHandler">An <see cref="Action"/> that handles a new line in the standard output of the executable.</param>
        /// <returns>A `new` instance of type <see cref="IShell"/> with the standard output handler passed to this call.</returns>
        IShell WithStandardOutputHandler(Action<string> standardOutputHandler);
        /// <summary>
        /// Adds an error output handler (of which there may be many) to the shell context and returns a `new` context instance.
        /// </summary>
        /// <param name="standardErrorHandler">An <see cref="Action"/> that handles a new line in the standard error of the executable.</param>
        /// <returns>A `new` instance of type <see cref="IShell"/> with the standard error handler passed to this call.</returns>
        IShell WithStandardErrorHandler(Action<string> standardErrorHandler);
        /// <summary>
        /// Adds a (user) input request handler to the shell context and returns a `new` context instance.
        /// </summary>
        /// <param name="inputRequestHandler">
        /// A <see cref="Func{T}"/> that handles when the shell blocks for user input during an execution.
        /// This handler should take (string StandardOutput, string StandardInput) and return a <see cref="Task{String}"/>
        /// that will be passed to the executable as StandardInput.
        /// </param>
        /// <returns>A `new` instance of <see cref="IShell"/> with the request handler passed to this call.</returns>
        IShell UseInputRequestHandler(Func<string, string, Task<string>> inputRequestHandler);
        /// <summary>
        /// Set the Standard Output Encoding on the shell context and returns a `new` context instance.
        /// </summary>
        /// <param name="standardOutputEncoding">The encoding to use for standard output.</param>
        /// <returns>A `new` instance of type <see cref="IShell"/> with the standard output encoding passed to this call.</returns>
        IShell UseStandardOutputEncoding(Encoding standardOutputEncoding);
        /// <summary>
        /// Set the Standard Error Encoding on the shell context and returns a `new` context instance.
        /// </summary>
        /// <param name="standardErrorEncoding">The encoding to use for standard error.</param>
        /// <returns>A `new` instance of type <see cref="IShell"/> with the standard error encoding passed to this call.</returns>
        IShell UseStandardErrorEncoding(Encoding standardErrorEncoding);

        /// <summary>
        /// Provides an <see cref="IObservable{T}"/> to which a subscription can be placed.
        /// The observable never completes, since executions can be run many times.
        /// </summary>
        /// <returns>A `new` instance of type <see cref="IShell"/> with the subscribers attached to the observable.</returns>
        IShell WithSubscribe(Action<IObservable<ICommandEvent>> subscriber);

        /// <summary>
        /// Adds a <see cref="CancellationToken"/> (of which there may be many) to the shell context and returns a `new` context instance.
        /// </summary>
        /// <returns>A `new` instance of type <see cref="IShell"/> with the cancellation token attached.</returns>
        IShell WithCancellationToken(CancellationToken cancellationToken);

        /// <summary>
        /// Set a "prefix" string for all commands executed on the shell context and returns a `new` context instance.
        /// </summary>
        /// <param name="prefix">The prefix for all commands.</param>
        /// <returns>A `new` instance of type <see cref="IShell"/> with the prefix string passed to this call.</returns>
        IShell UseCommandPrefix(string prefix);

        /// <summary>
        /// Ensures the shell context will not throw on a non-zero exit code and returns a `new` context instance.
        /// </summary>
        /// <returns>A `new` instance of type <see cref="IShell"/> that will not throw on a non-zero exit code.</returns>
        IShell UseNoThrow();

        /// <summary>
        /// Adds an executable and switches to the executable context.
        /// </summary>
        /// <typeparam name="TIExecutable">The type of the executable to use.</typeparam>
        /// <returns>An instance of <typeparamref name="TIExecutable"/> passed to this call.</returns>
        TIExecutable UseExecutable<TIExecutable>() where TIExecutable : IExecutable, new();
        /// <summary>
        /// Adds an executable and switches to the executable context.
        /// </summary>
        /// <param name="exe">The name or path of the executable.</param>
        /// <returns>An instance of <see cref="GenericExe"/> which represents the executable name or path passed to this call.</returns>
        IExecutable UseExecutable(string exe);
        
        /// <summary>
        /// Builds the arguments that should be passed to the shell based on the shell's type.
        /// </summary>
        /// <param name="executableCommand">The name or path of the executable for which to build the command.</param>
        /// <returns>A <see cref="string"/> which represents the command argument that should be passed to this shell.</returns>
        string GetCommandArgument(string executableCommand);
    }

    /// <summary>
    /// The interface for defining a shell.
    /// </summary>
    /// <typeparam name="TIShell">The type of the shell class implementing this interface.  This allows the base class to return `new` instances for daisy chaining.</typeparam>
    public interface IShell<out TIShell> : IShell where TIShell : IShell
    {
        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>A `new` instance of <typeparamref name="TIShell"/> with the same settings as the invoking instance.</returns>
        new TIShell Clone();

        /// <summary>
        /// Adds an environment variable (of which there may be many) to the shell context and returns a `new` context instance.
        /// </summary>
        /// <param name="key">The environment variable key.</param>
        /// <param name="value">The environment variable value.</param>
        /// <returns>A `new` instance of <typeparamref name="TIShell"/> with the environment variable passed in this call.</returns>
        new TIShell WithEnvironmentVariable(string key, string value);
        /// <summary>
        /// Adds a list of environment variables (of which there may be many) to the shell context and returns a `new` context instance.
        /// </summary>
        /// <param name="variables">The list of key value pairs of environment variables.</param>
        /// <returns>A `new` instance of <typeparamref name="TIShell"/> with the environment variables passed in this call.</returns>
        new TIShell WithEnvironmentVariables(IEnumerable<KeyValuePair<string, string>> variables);
        /// <summary>
        /// Adds a list of environment variables (of which there may be many) to the shell context and returns a `new` context instance.
        /// </summary>
        /// <param name="variables">The list of tuple of environment variables.</param>
        /// <returns>A `new` instance of <typeparamref name="TIShell"/> with the environment variables passed in this call.</returns>
        new TIShell WithEnvironmentVariables(IEnumerable<(string, string)> variables);

        /// <summary>
        /// Adds a string to the standard input stream (of which there may be many) to the shell context and returns a `new` context instance.
        /// </summary>
        /// <param name="standardInput">A string that gets passed to the standard input stream of the executable.</param>
        /// <returns>A `new` instance of type <typeparamref name="TIShell"/> with the standard input passed to this call.</returns>
        new TIShell WithStandardInput(string standardInput);
        /// <summary>
        /// Adds a standard output handler (of which there may be many) to the shell context and returns a `new` context instance.
        /// </summary>
        /// <param name="standardOutputHandler">An <see cref="Action"/> that handles a new line in the standard output of the executable.</param>
        /// <returns>A `new` instance of <typeparamref name="TIShell"/> with the standard output handler passed to this call.</returns>
        new TIShell WithStandardOutputHandler(Action<string> standardOutputHandler);
        /// <summary>
        /// Adds an error output handler (of which there may be many) to the shell context and returns a `new` context instance.
        /// </summary>
        /// <param name="standardErrorHandler">An <see cref="Action"/> that handles a new line in the standard error of the executable.</param>
        /// <returns>A `new` instance of <typeparamref name="TIShell"/> with the standard error handler passed to this call.</returns>
        new TIShell WithStandardErrorHandler(Action<string> standardErrorHandler);
        /// <summary>
        /// Adds a (user) input request handler to the shell context and returns a `new` context instance.
        /// </summary>
        /// <param name="inputRequestHandler">
        /// A <see cref="Func{T}"/> that handles when the shell blocks for user input during an execution.
        /// This handler should take (string StandardOutput, string StandardInput) and return a <see cref="Task{String}"/>
        /// that will be passed to the executable as StandardInput.
        /// </param>
        /// <returns>A `new` instance of <typeparamref name="TIShell"/> with the standard error handler passed to this call.</returns>
        new TIShell UseInputRequestHandler(Func<string, string, Task<string>> inputRequestHandler);
        /// <summary>
        /// Set the Standard Output Encoding on the shell context and returns a `new` context instance.
        /// </summary>
        /// <param name="standardOutputEncoding">The encoding to use for standard output.</param>
        /// <returns>A `new` instance of type <typeparamref name="TIShell"/> with the standard output encoding passed to this call.</returns>
        new TIShell UseStandardOutputEncoding(Encoding standardOutputEncoding);
        /// <summary>
        /// Set the Standard Error Encoding on the shell context and returns a `new` context instance.
        /// </summary>
        /// <param name="standardErrorEncoding">The encoding to use for standard error.</param>
        /// <returns>A `new` instance of type <typeparamref name="TIShell"/> with the standard error encoding passed to this call.</returns>
        new TIShell UseStandardErrorEncoding(Encoding standardErrorEncoding);

        /// <summary>
        /// Provides an <see cref="IObservable{T}"/> to which a subscription can be placed.
        /// The observable never completes, since executions can be run many times.
        /// </summary>
        /// <returns>A `new` instance of type <typeparamref name="TIShell"/> with the subscribers attached to the observable.</returns>
        new TIShell WithSubscribe(Action<IObservable<ICommandEvent>> subscriber);

        /// <summary>
        /// Adds a <see cref="CancellationToken"/> (of which there may be many) to the shell context and returns a `new` context instance.
        /// </summary>
        /// <returns>A `new` instance of type <typeparamref name="TIShell"/> with the cancellation token attached.</returns>
        new TIShell WithCancellationToken(CancellationToken cancellationToken);

        /// <summary>
        /// Set a "prefix" string for all commands executed on the shell context and returns a `new` context instance.
        /// </summary>
        /// <param name="prefix">The prefix for all commands.</param>
        /// <returns>A `new` instance of type <typeparamref name="TIShell"/> with the prefix string passed to this call.</returns>
        new TIShell UseCommandPrefix(string prefix);

        /// <summary>
        /// Ensures the shell context will not throw on a non-zero exit code and returns a `new` context instance.
        /// </summary>
        /// <returns>A `new` instance of type <typeparamref name="TIShell"/> that will not throw on a non-zero exit code.</returns>
        new TIShell UseNoThrow();
    }
}