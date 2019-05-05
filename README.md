# Sheller

[![GitHub Actions Status](https://wdp9fww0r9.execute-api.us-west-2.amazonaws.com/production/badge/twitchax/sheller)](https://wdp9fww0r9.execute-api.us-west-2.amazonaws.com/production/results/twitchax/sheller)
[![GitHub Release](https://img.shields.io/github/release/twitchax/sheller.svg)](https://github.com/twitchax/Sheller/releases)
[![NuGet Version](https://img.shields.io/nuget/v/Sheller.svg)](https://www.nuget.org/packages/Sheller/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Sheller.svg)](https://www.nuget.org/packages/Sheller/)

A .NET library that makes shelling out commands super easy and fluent.  Think of it as a way to build human-readable shell scripts with the power of a full programming language.

## Information

### Install

```bash
dotnet add package Sheller
```

### Test

Download the source and run.

```bash
dotnet test --filter os~nix
```

Or.

```bash
dotnet test --filter os~win
```

> NOTE: Windows tests require WSL.

### Compatibility

Latest .NET Standard 2.0.

### Examples

This library is extendable, but you can run it a few ways depending on how you have extended it.  For more examples, check out the [tests](src/Test/BasicTests.cs).

Just as one would expect from `IEnumerable`, and other fluent interface designs, the result of every call on `IShell` and `IExecutable` is a _new_ instance.  This means that you can reuse useful statements.

In addition, _future_ calls will not affect any previous instances, meaning you can safely chain them without affecting the calling instance.  A good example of this behavior is the fact that `myEnumerable.Where(...)` does not affect `myEnumerable`.

In general `With` methods can be called multiple times (multiple entries are allowed per execution context) and `Use` methods can only be called once (only one entry is allowed per execution context).  The context changes from `IShell` to `IExecutable` upon calling `IShell.UseExecutable`, so the methods available will be different once that method is called.

#### Basic

With no extensions, you would run a command like this.

```csharp
var echoResult = await Sheller.Builder
    .UseShell("/bin/bash")
        .WithEnvironmentVariable("MY_VAR", "lol")
    .UseExecutable("echo")
        .WithArgument("$MY_VAR")
    .ExecuteAsync();

var exitCode = result.ExitCode; // 0
var standardOutput = result.StandardOutput; // "lol\n"
var standardError = result.StandardError; // ""
var startTime = result.StartTime;
var endTime = result.EndTime;
var runTime = result.RunTime;
```

However, you can build your own custom `IShell` and `IExecutable` implementations that yield code that looks like this (Sheller ships with `Bash` and `Echo` by default).

```csharp
var result = await Sheller.Builder
    .UseShell<Bash>()
        .WithEnvironmentVariable("MY_VAR", varValue)
    .UseExecutable<Echo>()
        .WithArgument("$MY_VAR")
    .ExecuteAsync();

var echoValue = result; // "lol"
```

#### Reusable Objects

Just like one would expect from `IEnumerable`, and other fluent designs, the result of every call on `IShell` and `IExecutable` is a _new_ instance.  This means that you can reuse useful statements.

In addition, _future_ calls will not affect any previous instances, meaning you can safely chain them without affecting the calling instance.  A good example of this behavior is the fact that `myEnumerable.Where(...)` does not affect `myEnumerable`.

```csharp
var shell = Builder.UseShell<Bash>().WithEnvironmentVariable("MY_VAR", "cool");

var anotherShell1 = shell.WithEnvironmentVariable("MY_VAR_2", "beans");
var anotherShell2 = shell.WithEnvironmentVariable("MY_VAR_2", "stuff");

var echoResult1 = anotherShell1.UseExecutable<Echo>().WithArgument("$MY_VAR$MY_VAR_2").ExecuteAsync(); //result: "coolbeans".
var echoResult2 = anotherShell1.UseExecutable<Echo>().WithArgument("$MY_VAR$MY_VAR_2").ExecuteAsync(); //result: "coolstuff".

```

#### Environment Variables

You can set environment variables on the shell.

```csharp
var result = await Builder
    .UseShell<Bash>().WithEnvironmentVariable("MY_VAR", expected)
    .UseExecutable<Echo>().WithArgument("$MY_VAR")
    .ExecuteAsync();
```

#### Standard Output/Error

You can capture standard output and standard error.  These methods can be called multiple times.

```csharp
await Builder
    .UseShell<Bash>()
    .UseExecutable<Echo>()
        .WithArgument("saythings")
        .WithStandardOutputHandler(Console.WriteLine)
        .WithStandardErrorHandler()
    .ExecuteAsync();
```

#### Standard Input

You can pass standard input that gets captured during execution.  This method may be called multiple times.

```csharp
var echoResult = await Builder
    .UseShell<Bash>()
    .UseExecutable("read var1; read var2; echo $var1$var2")
        .WithStandardInput("cool")
        .WithStandardInput("library")
    .ExecuteAsync();

// echoResult is "coollibrary".
```

#### Input Request Handler

You can provide a handler that will be called when execution is blocked and waiting for input.  This can only be set once per execution context.

```csharp
var echoResult = await Builder
    .UseShell<Bash>()
    .UseExecutable($"read var1; echo $var1")
    .UseInputRequestHandler((stdout, stderr) =>
    {
        // Process stdout or stderr, if needed.
        // Process is blocked, waiting for this input.
        return Task.FromResult("awesome");
    })
    .ExecuteAsync();

// echoResult is "awesome".
```

#### No Throw

You can instruct the execution to _not_ throw on a non-zero exit code.  This method may only be called once.

```csharp
await Builder
    .UseShell<Bash>()
    .UseExecutable("foo")
    .UseNoThrow()
    .ExecuteAsync();
```

#### Result Selector

You can pass a map function to `ExecuteAsync` to "select" the result.

```csharp
var echoErrorCode = await Builder
    .UseShell<Bash>()
    .UseExecutable<Echo>().WithArgument("dummy")
    .ExecuteAsync(cr => 
    {
        return cr.ExitCode;
    });

// echoErrorCode is 0.
```

#### Execution Timeout

You can set the execution timeout.  This method can only be called once.

```csharp
await Builder
    .UseShell<Bash>()
    .UseExecutable<Sleep>()
        .WithArgument("10")
        .UseTimeout(TimeSpan.FromSeconds(5))
    .ExecuteAsync();
```

#### Post Execution Wait

You can provide waiters that process the result and wait upon a task before completion of the outer task.  These waiters may be applied multiple times.  You can also set a wait timeout (only one) with `UseWaitTimeout`.

```csharp
var echoValue = await Builder
    .UseShell<Bash>()
    .UseExecutable<Echo>()
        .WithArgument("dummy")
        .WithWait(async cr => 
        {
            return SomethingThatIsATask();
        })
        .UseWaitTimeout(TimeSpan.FromSeconds(30))
    .ExecuteAsync();
```

#### Reactive

You can subscribe to an internal `IObservable`.  You can call the subscriber multiple times.

```csharp
var events = new List<string>();

var command1 = Builder
    .UseShell<Bash>()
    .UseExecutable("echo")
    .WithArgument(expected)
    .WithSubscribe(o =>
    {
        o.Where(ev => ev.Type == CommandEventType.StandardOutput).Select(ev => ev.Data).Do(data =>
        {
            events.Add(data);
        }).Subscribe();
    })
    .ExecuteAsync();
```

#### Cancellation

You can set cancellation tokens.  This method may be called multiple times.

```csharp
using (var ctSource = new CancellationTokenSource())
{
    ctSource.CancelAfter(TimeSpan.FromSeconds(5));

    Builder
        .UseShell<Bash>()
        .UseExecutable<Sleep>()
        .WithArgument("10")
        .WithCancellationToken(ctSource.Token)
        .ExecuteAsync();
}
```

#### Character Encoding

You can set the output character encoding.  This method may only be called once.

```csharp
var echoValue = await Builder
    .UseShell<Bash>()
    .UseExecutable<Echo>()
        .WithArgument("😋")
        .UseStandardOutputEncoding(Encoding.ASCII)
    .ExecuteAsync();

// echoValue is "????".
```

#### `StartInfo` Transform

You can arbitrarily change any options on the `StartInfo` of the process.  This method may only be called once.

```csharp
var echoValue = await Builder
    .UseShell<Bash>()
    .UseExecutable<Echo>()
        .WithArgument(expected)
        .UseStartInfoTransform(si => 
        {
            si.WorkingDirectory = "/";
        })
    .ExecuteAsync();
```

#### Roll Your Own

You can also roll your own `IShell` and `IExecutable` plugins.  For example, it would be nice to implement a `kubectl` wrapper.

```csharp
public interface IKubectl : IExecutable
{
    IKubectl WithKubeConfig(string configPath);
    IKubectl WithApply(string yamlPath);
}

public class Kubectl : Executable<IKubectl>, IKubectl
{
    // Allows the `Executable` base class to "create and clone" a new instance for chanining.
    protected override Executable<IKubectl> Create() => new Kubectl();
    // Sets the underlying executable of this executable type.
    public Kubectl() : base("kubectl") {}
    
    public IKubectl WithKubeConfig(string configPath) => this.WithArgument($"--kubeconfig={configPath}");
    public IKubectl WithApply(string yamlPath) => this.WithArgument("apply", "-f", yamlPath);
}

...

var result = await Builder
    .UseShell<Bash>()
    .UseExecutable<Kubectl>()
        .WithKubeConfig("kube_config.yaml")
        .WithApply("my_app.yaml")
    .ExecuteAsync();
```

## License

```
The MIT License (MIT)

Copyright (c) 2019 Aaron Roney

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```