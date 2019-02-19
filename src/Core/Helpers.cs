using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Sheller.Implementations;
using Sheller.Models;

namespace Sheller
{
    internal static class Helpers
    {
        internal static IEnumerable<T> MergeEnumerables<T>(params IEnumerable<T>[] lists)
        {
            foreach(var list in lists)
            {
                foreach(var item in list)
                {
                    yield return item;
                }
            }
        }

        internal static IEnumerable<T> ToEnumerable<T>(this T obj)
        {
            yield return obj;
        }

        internal static IDictionary<T, R> MergeDictionaries<T, R>(params IDictionary<T, R>[] dicts)
        {
            var result = new Dictionary<T, R>();
            foreach(var dict in dicts)
            {
                foreach(var kvp in dict)
                {
                    result.Add(kvp.Key, kvp.Value);
                }
            }

            return result;
        }

        internal static IDictionary<T, R> ToDictionary<T, R>(this (T, R) tuple)
        {
            return new Dictionary<T, R>
            {
                { tuple.Item1, tuple.Item2 }
            };
        }

        internal static IEnumerable<KeyValuePair<T, R>> MergeDictionaries<T, R>(params IEnumerable<KeyValuePair<T, R>>[] dicts)
        {
            foreach(var dict in dicts)
            {
                foreach(var kvp in dict)
                {
                    yield return kvp;
                }
            }
        }
        
        internal static IEnumerable<KeyValuePair<T, R>> ToDictionary<T, R>(this IEnumerable<(T, R)> kvps)
        {
            foreach(var kvp in kvps)
            {
                yield return new KeyValuePair<T, R>(kvp.Item1, kvp.Item2);
            }
        }
        
        internal static void CopyToStringDictionary(this IEnumerable<KeyValuePair<string, string>> kvps, StringDictionary dict)
        {
            foreach(var kvp in kvps)
            {
                dict.Add(kvp.Key, kvp.Value);
            }
        }

        internal static string EscapeQuotes(this string s) => s.Replace("\"", "\\\"");

        internal static Task<ICommandResult> RunCommand(
            string command,
            string args = null,
            IEnumerable<string> standardInputs = null, 
            IEnumerable<Action<string>> standardOutputHandlers = null,
            IEnumerable<Action<string>> standardErrorHandlers = null,
            Func<string, string, Task<string>> inputRequestHandler = null)
        {
            var t = new Task<ICommandResult>(() => 
            {
                var process = new Process();
                var standardOutput = new StringBuilder();
                var standardError = new StringBuilder();
                
                process.StartInfo.FileName = command;
                process.StartInfo.Arguments = args;

                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;
                
                process.OutputDataReceived += (s, e) => 
                {
                    if(e.Data == null) return;

                    standardOutput.AppendLine(e.Data);
                    if(standardOutputHandlers != null)
                        foreach(var handler in standardOutputHandlers)
                            handler(e.Data);
                };
                
                process.ErrorDataReceived += (s, e) => 
                {
                    if(e.Data == null) return;

                    standardError.AppendLine(e.Data);
                    if(standardErrorHandlers != null)
                        foreach(var handler in standardErrorHandlers)
                            handler(e.Data);
                };

                if(inputRequestHandler != null)
                {
                    Task.Run(async () => 
                    {
                        while(true)
                        {
                            await Task.Delay(TimeSpan.FromSeconds(1));

                            if(!process.IsProcessAlive())
                                continue;

                            if(process.HasExited)
                                break;
                            
                            foreach(ProcessThread thread in process.Threads)
                                if (thread.ThreadState == System.Diagnostics.ThreadState.Wait && thread.WaitReason == ThreadWaitReason.UserRequest)
                                {
                                    process.StandardInput.WriteLine(await inputRequestHandler(standardOutput.ToString(), standardError.ToString()));
                                    break;
                                }
                        }
                    });
                }

                process.Start();

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                if(standardInputs != null)
                    foreach(var l in standardInputs)
                        process.StandardInput.WriteLine(l);
                
                process.WaitForExit();

                var succeeded = process.ExitCode == 0;
                var exitCode = process.ExitCode;
                var standard = standardOutput.ToString();
                var error = standardError.ToString();

                return new CommandResult(succeeded, exitCode, standard, error);
            });
            t.Start();

            return t;
        }

        static bool IsProcessAlive(this Process p)
        {
            try
            {
                var dummy = p.Id;
                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }
    }
}