

using System;
using System.Collections.Generic;
using System.Linq;
using Sheller.Implementations.Executables;
using Sheller.Models;

namespace Sheller.Implementations.Shells
{
    public class Shell : IShell
    {
        #region Properties

        private string _shell;
        private IEnumerable<ILogger> _loggers;
        private IEnumerable<KeyValuePair<string, string>> _environmentVariables;

        public string Path => _shell;

        #endregion

        #region Constructors

        public Shell(string shell, IEnumerable<ILogger> loggers = null, IEnumerable<KeyValuePair<string, string>> environmentVariables = null)
        {
            _shell = shell;
            _loggers = loggers ?? new List<ILogger>();
            _environmentVariables = environmentVariables ?? new List<KeyValuePair<string, string>>();
        }

        #endregion

        #region Logger

        public IEnumerable<ILogger> Loggers => _loggers;

        public IShell WithDefaultLogger()
        {
            return new Shell(
                this._shell, 
                Helpers.MergeEnumerables(this._loggers, new Logger().ToEnumerable()), 
                _environmentVariables);
        }

        public IShell WithLogger(ILogger logger)
        {
            return new Shell(
                _shell, 
                Helpers.MergeEnumerables(this._loggers, logger.ToEnumerable()), 
                _environmentVariables);
        }
        
        public IShell WithLoggers(IEnumerable<ILogger> loggers)
        {
            return new Shell(
                _shell, 
                Helpers.MergeEnumerables(this._loggers, loggers), 
                _environmentVariables);
        }

        #endregion

        #region Environment Variables

        public IEnumerable<KeyValuePair<string, string>> EnvironmentVariables => _environmentVariables;

        public IShell WithEnvironmentVariable(string key, string value)
        {
            return new Shell(
                _shell, 
                _loggers, 
                Helpers.MergeEnumerables(_environmentVariables, new KeyValuePair<string, string>(key, value).ToEnumerable()));
        }

        public IShell WithEnvironmentVariables(IEnumerable<KeyValuePair<string, string>> variables)
        {
            return new Shell(
                _shell, 
                _loggers, 
                Helpers.MergeEnumerables(_environmentVariables, variables));
        }

        public IShell WithEnvironmentVariables(IEnumerable<(string, string)> variables)
        {
            return new Shell(
                _shell, 
                _loggers, 
                Helpers.MergeEnumerables(_environmentVariables, variables.ToDictionary()));
        }

        #endregion

        #region Executable

        public TExecutable UseExecutable<TExecutable>() where TExecutable : IExecutable, new()
        {
            var result = new TExecutable();
            result.SetShell(this);

            return result;
        }

        public IExecutable<ICommandResult> UseExecutable(string exe)
        {
            return new Generic(exe, this);
        }

        public virtual string GetCommandArgument(string executableCommand)
        {
            var environmentVariables = _environmentVariables.Aggregate("", (agg, kvp) => agg += $"export {kvp.Key}=\"{kvp.Value.EscapeQuotes()}\";");
            return $"-c \"{environmentVariables}{ executableCommand.EscapeQuotes() }\"";
        }

        #endregion
    }
}