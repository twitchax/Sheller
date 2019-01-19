
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Sheller.Implementations;
using Sheller.Models;

namespace Sheller
{
    public static class Helpers
    {
        public static IEnumerable<T> MergeEnumerables<T>(params IEnumerable<T>[] lists)
        {
            foreach(var list in lists)
            {
                foreach(var item in list)
                {
                    yield return item;
                }
            }
        }

        public static IEnumerable<T> ToEnumerable<T>(this T obj)
        {
            yield return obj;
        }

        public static IEnumerable<KeyValuePair<T, R>> MergeDictionaries<T, R>(params IEnumerable<KeyValuePair<T, R>>[] dicts)
        {
            foreach(var dict in dicts)
            {
                foreach(var kvp in dict)
                {
                    yield return kvp;
                }
            }
        }
        
        public static IEnumerable<KeyValuePair<T, R>> ToDictionary<T, R>(this IEnumerable<(T, R)> kvps)
        {
            foreach(var kvp in kvps)
            {
                yield return new KeyValuePair<T, R>(kvp.Item1, kvp.Item2);
            }
        }
        
        public static void CopyToStringDictionary(this IEnumerable<KeyValuePair<string, string>> kvps, StringDictionary dict)
        {
            foreach(var kvp in kvps)
            {
                dict.Add(kvp.Key, kvp.Value);
            }
        }

        public static string EscapeQuotes(this string s) => s.Replace("\"", "\\\"");

        public static Task<ICommandResult> RunCommand(
            string command, 
            string args, 
            IEnumerable<KeyValuePair<string, string>> environmentVariables = null, 
            Action<string> onDataReceived = null, 
            Action<string> onErrorReceived = null, 
            IEnumerable<ILogger> loggers = null)
        {
            var t = new Task<ICommandResult>(() => 
            {
                Process process = new Process();
                
                process.StartInfo.FileName = command;
                process.StartInfo.Arguments = args;

                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;

                // if(environmentVariables != null)
                //     environmentVariables.CopyToStringDictionary(process.StartInfo.EnvironmentVariables);
                
                if(onDataReceived != null)
                    process.OutputDataReceived += (s, e) => onDataReceived(e.Data);
                if(onErrorReceived != null)
                    process.ErrorDataReceived += (s, e) => onErrorReceived(e.Data);

                var a = process.Start();
                process.WaitForExit();

                var exitCode = process.ExitCode;
                var standard = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();

                //logger.LogInfo($"[{command}]\n{standard}");
                //if(!string.IsNullOrEmpty(error))
                    //logger.LogError($"[{command}] ERROR\n{error}");

                return new CommandResult(exitCode, standard, error);
            });
            t.Start();

            return t;
        }
    }
}