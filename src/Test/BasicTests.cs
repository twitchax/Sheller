using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Sheller.Implementations;
using Sheller.Implementations.Executables;
using Sheller.Implementations.Shells;
using Sheller.Models;
using Xunit;

// NOTE: win tests require WSL...because...lazy.
// Uses `ToExecutable` to cover the generic code path.

namespace Sheller.Tests
{
    public class BasicTests
    {
        [Fact]
        [Trait("os", "nix_win")]
        public async void CanExecuteEchoBash()
        {
            var expected = "lol";

            var echoValue = await Builder
                .UseShell("bash")
                .UseExecutable("echo")
                    .WithArgument(expected)
                .ExecuteAsync();

            Assert.True(echoValue.Succeeded);
            Assert.Equal(0, echoValue.ExitCode);
            Assert.Equal(expected, echoValue.StandardOutput.Trim());
        }

        [Fact]
        [Trait("os", "nix_win")]
        public async void CanExecuteEchoClone()
        {
            var expected = "lol";

            var echoValue = await Builder
                .UseShell("bash")
                .UseExecutable("echo").Clone()
                    .WithArgument(expected)
                .ExecuteAsync();

            Assert.True(echoValue.Succeeded);
            Assert.Equal(0, echoValue.ExitCode);
            Assert.Equal(expected, echoValue.StandardOutput.Trim());
        }

        [Fact]
        [Trait("os", "nix_win")]
        public async void CanExecuteEchoWithGeneric()
        {
            var expected = "lol";

            var echoValue = await Builder
                .UseShell<Bash>()
                .UseExecutable<Echo>()
                    .WithArgument(expected)
                .ExecuteAsync();

            Assert.Equal(expected, echoValue);
        }

        [Fact]
        [Trait("os", "nix_win")]
        public async void CanExecuteEchoWithSwappedShell()
        {
            var expected = "lol";

            var echoValue = await Builder.UseShell("not_a_shell_lol").UseExecutable<Echo>()
                .UseShell(Builder.UseShell<Bash>())
                .WithArgument(expected)
                .ExecuteAsync();

            Assert.Equal(expected, echoValue);
        }

        [Fact]
        [Trait("os", "nix_win")]
        public async void CanThrowWithBadExe()
        {
            var expected = 127;

            var exception = await Assert.ThrowsAsync<ExecutionFailedException>(async () =>
            {
                var echoValue = await Builder
                .UseShell<Bash>()
                .UseExecutable("foo")
                .ExecuteAsync();
            });

            Assert.False(exception.Result.Succeeded);
            Assert.NotEqual(0, exception.Result.ExitCode);
            Assert.Equal(expected, exception.Result.ExitCode);
        }

        [Fact]
        [Trait("os", "nix_win")]
        public async void CanSuppressThrowWithBadExe()
        {
            var expected = 127;

            var echoValue = await Builder
                .UseShell<Bash>()
                .UseExecutable("foo")
                .UseNoThrow()
                .ExecuteAsync();

            Assert.Equal(expected, echoValue.ExitCode);
        }

        [Fact]
        [Trait("os", "nix_win")]
        public async void CanExecuteEchoWithGenericAndEnvironmentVariableNix()
        {
            var expected = "lol";

            var echoValue = await Builder
                .UseShell<Bash>()
                    .WithEnvironmentVariable("MY_VAR", expected)
                    .WithEnvironmentVariables(new List<(string, string)> { ("VAR1", "value1"), ("VAR2", "value2"), })
                .UseExecutable<Echo>()
                    .WithArgument("$MY_VAR")
                .ExecuteAsync();

            Assert.Equal(expected, echoValue);
        }

        [Fact]
        [Trait("os", "nix_win")]
        public async void CanExecuteEchoWithTimeout()
        {
            var min = 2;
            var max = 4;

            var start = DateTime.Now;
            await Assert.ThrowsAsync<ExecutionTimeoutException>(() =>
            {
                return Builder
                    .UseShell<Bash>()
                    .UseExecutable<Sleep>().ToExecutable()
                        .WithArgument(max.ToString())
                        .UseTimeout(TimeSpan.FromSeconds(min + .1))
                    .ExecuteAsync();
            });
            var delta = DateTime.Now - start;

            Assert.True(delta.TotalSeconds > min);
            Assert.True(delta.TotalSeconds < max);
        }

        [Fact]
        [Trait("os", "nix_win")]
        public async void CanExecuteEchoWithStandardOutputHandlerOnShell()
        {
            var expected = "lol";
            var handlerString = new StringBuilder();

            var echoValue = await Builder
                .UseShell<Bash>()
                    .WithStandardOutputHandler(s => handlerString.Append(s))
                .UseExecutable<Echo>()
                    .WithArgument(expected)
                .ExecuteAsync();

            Assert.Equal(expected, echoValue);
            Assert.Equal(expected, handlerString.ToString());
        }

        [Fact]
        [Trait("os", "nix_win")]
        public async void CanExecuteEchoWithStandardOutputHandlerOnExecutable()
        {
            var expected = "lol";
            var handlerString = new StringBuilder();

            var echoResult = await Builder
                .UseShell<Bash>()
                .UseExecutable<Echo>().ToExecutable()
                    .WithArgument(expected)
                    .WithStandardOutputHandler(s => handlerString.Append(s))
                .ExecuteAsync();

            Assert.Equal(expected, echoResult.StandardOutput.Trim());
            Assert.Equal(expected, handlerString.ToString());
        }

        [Fact]
        [Trait("os", "nix_win")]
        public async void CanExecuteEchoWithStandardErrorHandler()
        {
            var expected = "error";
            var handlerString = new StringBuilder();

            var echoResult = await Builder
                .UseShell<Bash>()
                .UseExecutable(">&2 echo").ToExecutable()
                    .WithArgument(expected)
                    .WithStandardErrorHandler(s => handlerString.Append(s))
                .ExecuteAsync();

            Assert.Equal(expected, echoResult.StandardError.Trim());
            Assert.Equal(expected, handlerString.ToString());
        }

        [Fact]
        [Trait("os", "nix")]
        public async void CanExecuteEchoWithStandardInputNix()
        {
            var expected1 = "lol";
            var expected2 = "face";

            var echoResult = await Builder
                .UseShell<Bash>()
                .UseExecutable("read var1; read var2; echo $var1$var2")
                    .WithStandardInput(expected1)
                    .WithStandardInput(expected2)
                .ExecuteAsync();

            Assert.Equal($"{expected1}{expected2}", echoResult.StandardOutput.Trim());
        }

        [Fact]
        [Trait("os", "win")]
        public async void CanExecuteEchoWithStandardInputWin()
        {
            var expected1 = "lol";
            var expected2 = "face";

            var echoResult = await Builder
                .UseShell<Bash>()
                .UseExecutable("read var1; read var2; echo $var1$var2")
                    .WithStandardInput(expected1)
                    .WithStandardInput(expected2)
                .ExecuteAsync();

            Assert.Equal($"{expected1}\r\n{expected2}", echoResult.StandardOutput.Trim());
        }

        [Fact]
        [Trait("os", "nix_win")]
        public async void CanExecuteEchoWithInputRequestHandlerNix()
        {
            var expected1 = "hello";
            var expected2 = "lol";

            var echoResult = await Builder
                .UseShell<Bash>()
                .UseExecutable($"echo {expected1}; read var1; echo $var1")
                .UseInputRequestHandler((stdout, _) =>
                {
                    Assert.Contains(expected1, stdout);
                    return Task.FromResult(expected2);
                })
                .ExecuteAsync();

            Assert.Contains($"{expected2}", echoResult.StandardOutput);
        }

        [Fact]
        [Trait("os", "nix_win")]
        public async void CanExecuteEchoWithResultSelector()
        {
            var expected = 0;

            var echoErrorCode = await Builder
                .UseShell<Bash>()
                .UseExecutable<Echo>()
                    .WithArgument("dummy")
                .ExecuteAsync(cr => cr.ExitCode);

            Assert.Equal(expected, echoErrorCode);
        }

        [Fact]
        [Trait("os", "nix_win")]
        public async void CanExecuteEchoWithResultSelectorTask()
        {
            var expected = 0;

            var echoErrorCode = await Builder
                .UseShell<Bash>()
                .UseExecutable<Echo>()
                    .WithArgument("dummy")
                .ExecuteAsync(async cr => 
                {
                    await Task.Delay(100);
                    return cr.ExitCode;
                });

            Assert.Equal(expected, echoErrorCode);
        }

        [Fact]
        [Trait("os", "nix_win")]
        public async void CanExecuteEchoWithWait()
        {
            var min = 2;

            var start = DateTime.Now;
            var echoValue = await Builder
                .UseShell<Bash>()
                .UseExecutable<Echo>()
                    .WithArgument("dummy")
                    .WithWait(async cr => await Task.Delay(TimeSpan.FromSeconds(min - 1)))
                    .WithWait(async cr => await Task.Delay(TimeSpan.FromSeconds(min)))
                .ExecuteAsync();
            var delta = DateTime.Now - start;

            Assert.True(delta.TotalSeconds > min);
        }

        [Fact]
        [Trait("os", "nix_win")]
        public async void CanExecuteEchoWithWaitTimeout()
        {
            var min = 2;
            var max = 4;

            var start = DateTime.Now;
            await Assert.ThrowsAsync<ExecutionTimeoutException>(() =>
            {
                return Builder
                    .UseShell<Bash>()
                    .UseExecutable<Echo>()
                        .WithArgument("dummy")
                        .WithWait(async cr => await Task.Delay(TimeSpan.FromSeconds(max)))
                        .WithWait(async cr => await Task.Delay(TimeSpan.FromSeconds(max + 1)))
                        .UseWaitTimeout(TimeSpan.FromSeconds(min))
                    .ExecuteAsync();
            });
            var delta = DateTime.Now - start;

            Assert.True(delta.TotalSeconds > min);
            Assert.True(delta.TotalSeconds < max);
        }

        [Fact]
        [Trait("os", "nix_win")]
        public async void CanExecuteAndSubscribe()
        {
            var events = new List<string>();
            var expected = "lol";

            var subscriptions = new List<IDisposable>();

            var command1 = Builder
                .UseShell<Bash>()
                .UseExecutable("echo")
                .WithArgument(expected)
                .WithSubscribe(o =>
                {
                    subscriptions.Add(
                        o.Where(ev => ev.Type == CommandEventType.StandardOutput).Select(ev => ev.Data).Do(data => events.Add(data)).Subscribe()
                    );
                });

            var command2 = command1
                .WithSubscribe(o =>
                {
                    subscriptions.Add(
                        o.Where(ev => ev.Type == CommandEventType.StandardOutput).Select(ev => ev.Data).Do(data => events.Add(data)).Subscribe()
                    );
                });

            await command1
                .ExecuteAsync();

            await command1
                .ExecuteAsync();

            await command2
                .ExecuteAsync();

            foreach(var subscription in subscriptions)
                subscription.Dispose();

            Assert.Equal(4, events.Count);
            Assert.Contains(expected, events);
        }

        [Fact]
        [Trait("os", "nix")]
        public async void CanExecuteAndCancel()
        {
            var min = 2;
            var max = 10;

            using var ctSource = new CancellationTokenSource();

            ctSource.CancelAfter(TimeSpan.FromSeconds(min + .5));

            var start = DateTime.Now;
            await Assert.ThrowsAsync<ExecutionFailedException>(() =>
            {
                return Builder
                    .UseShell<Bash>()
                    .UseExecutable<Sleep>().ToExecutable()
                    .WithArgument(max.ToString())
                    .WithCancellationToken(ctSource.Token)
                    .ExecuteAsync();
            });

            var delta = DateTime.Now - start;

            Assert.True(delta.TotalSeconds > min);
            Assert.True(delta.TotalSeconds < max);
        }

        [Fact]
        [Trait("os", "nix_win")]
        public async void CanUseEncoding()
        {
            var expected = "AcciÃ³n";
            byte[] bytes = Encoding.Default.GetBytes(expected);
            var expectedAscii = Encoding.ASCII.GetString(bytes);

            var echoResult = await Builder
                .UseShell<Bash>()
                .UseExecutable<Echo>().ToExecutable()
                    .WithArgument(expected)
                    .UseStandardOutputEncoding(Encoding.ASCII)
                    .UseStandardErrorEncoding(Encoding.ASCII)
                .ExecuteAsync();

            Assert.Equal(expectedAscii, echoResult.StandardOutput.Trim());
        }

        [Fact]
        [Trait("os", "nix_win")]
        public async void CanUseExecutableOverride()
        {
            var expected = "fun";

            var echo = await Builder
                .UseShell<Bash>()
                .UseExecutable("notarealone")
                    .UseExecutable("echo")
                    .WithArgument(expected)
                .ExecuteAsync();

            Assert.Equal(expected, echo.StandardOutput.Trim());
        }

        [Fact]
        [Trait("os", "nix_win")]
        public async void CanUseCommandPrefix()
        {
            var prefix = "echo";
            var expected = "fun";

            var echoValue = await Builder
                .UseShell<Bash>()
                    .UseCommandPrefix(prefix)
                .UseExecutable<Echo>()
                    .WithArgument(expected)
                .ExecuteAsync();

            Assert.Equal($"{prefix} {expected}", echoValue);
        }

        [Fact]
        [Trait("os", "nix_win")]
        public async void CanExecuteWithStartInfoTransform()
        {
            var expected = "cool";

            var echoValue = await Builder
                .UseShell<Bash>()
                .UseExecutable("dummy")
                    .UseStartInfoTransform(si => si.Arguments = $"-c \"echo {expected}\";")
                .ExecuteAsync();

            Assert.Equal(expected, echoValue.StandardOutput.Trim());
        }
    }
}
