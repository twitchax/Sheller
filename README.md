# Sheller

[![Build Status](https://travis-ci.org/twitchax/Sheller.svg?branch=master)](https://travis-ci.org/twitchax/Sheller)

A .NET library that makes shelling out commands super easy.  Think of it as a way to build human-readable shell scripts with the power of a full programming language.

## Information

### Install

```bash
dotnet add package Sheller
```

### Test

Download the source and run.

```bash
dotnet test
```

### Compatibility

Latest .NET Standard 2.0.

### Examples

This library is extendable, but you can run it a few ways depending on how you have extended it.

With no extensions, you would run a command like this.

```csharp
var echoResult = await Sheller
    .Shell("/bin/bash")
        .WithEnvironmentVariable("MY_VAR", "lol")
    .UseExecutable("echo")
        .WithArgument("$MY_VAR")
    .ExecuteAsync();

var exitCode = result.ExitCode; // 0
var standardOutput = result.StandardOutput; // "lol\n"
var standardError = result.StandardError; // ""
```

However, you can build your own custom `IShell` and `IExecutable` implementations that yield code that looks like this (Sheller ships with `Bash` and `Echo` by default).

```csharp
var result = await Sheller
    .Shell<Bash>()
        .WithEnvironmentVariable("MY_VAR", varValue)
    .UseExecutable<Echo>()
        .WithArgument("$MY_VAR")
    .ExecuteAsync();

var echoValue = result; // "lol"
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