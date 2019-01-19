using System;
using System.Threading;
using Sheller.Implementations;
using Sheller.Implementations.Shells;
using Sheller.Models;

// TODO:
//   * Finish implementing IExecutable on Executable.  Then, write tests for those features.
//   * Write the XML docs... ... ...
//   * WithHandlers should make both arguments null as default values.
//   * Add an "before" and "after" hook to the executable?  Just make a list.

namespace Sheller
{
    public static class Sheller
    {
        public static IShell Shell(string shell) => new Shell(shell);
        public static T Shell<T>() where T : IShell, new() => new T();
    }
}
