using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Sheller.Implementations;
using Sheller.Implementations.Executables;
using Sheller.Implementations.Shells;
using Xunit;

// NOTE: Windows tests require WSL...because...lazy.

namespace Sheller.Tests
{
    public class BasicTests
    {
        [Fact]
        [Trait("os", "nix_windows")]
        public async void CanExecuteEchoBash()
        {
            var expected = "lol";

            var echoValue = await Sheller
                .Shell("bash")
                .UseExecutable("echo")
                    .WithArgument(expected)
                .ExecuteAsync();

            Assert.Equal(expected, echoValue.StandardOutput.Trim());
        }

        [Fact]
        [Trait("os", "nix_windows")]
        public async void CanExecuteEchoWithGeneric()
        {
            var expected = "lol";

            var echoValue = await Sheller
                .Shell<Bash>()
                .UseExecutable<Echo>()
                    .WithArgument(expected)
                .ExecuteAsync();

            Assert.Equal(expected, echoValue);
        }


        [Fact]
        [Trait("os", "nix")]
        public async void CanExecuteEchoWithGenericAndEnvironmentVariableNix()
        {
            var expected = "lol";

            var echoValue = await Sheller
                .Shell<Bash>()
                    .WithEnvironmentVariable("MY_VAR", expected)
                .UseExecutable<Echo>()
                    .WithArgument("$MY_VAR")
                .ExecuteAsync();

            Assert.Equal(expected, echoValue);
        }

        [Fact]
        [Trait("os", "windows")]
        public async void CanExecuteEchoWithGenericAndEnvironmentVariableWindows()
        {
            var expected = "lol";

            var echoValue = await Sheller
                .Shell<Bash>()
                    .WithEnvironmentVariable("MY_VAR", expected)
                .UseExecutable<Echo>()
                    .WithArgument("\\$MY_VAR")
                .ExecuteAsync();

            Assert.Equal(expected, echoValue);
        }

        [Fact]
        [Trait("os", "nix_windows")]
        public async void CanExecuteEchoWithTimeout()
        {
            var min = 2;
            var max = 4;

            var start = DateTime.Now;
            await Assert.ThrowsAnyAsync<Exception>(() =>
            {
                return Sheller
                .Shell<Bash>()
                .UseExecutable<Sleep>()
                    .WithArgument(max.ToString())
                    .WithTimeout(TimeSpan.FromSeconds(min))
                .ExecuteAsync();
            });
            var delta = DateTime.Now - start;

            Console.WriteLine(delta.TotalSeconds);
            Assert.True(delta.TotalSeconds > min);
            Assert.True(delta.TotalSeconds < max);
        }

        [Fact]
        [Trait("os", "nix_windows")]
        public async void CanExecuteEchoWithStandardOutputHandler()
        {
            var expected = "lol";
            var handlerString = new StringBuilder();

            var echoValue = await Sheller
                .Shell<Bash>()
                .UseExecutable<Echo>()
                    .WithArgument(expected)
                    .WithStandardOutputHandler(s => handlerString.Append(s))
                .ExecuteAsync();
                
            Assert.Equal(expected, echoValue);
            Assert.Equal(expected, handlerString.ToString());
        }

        [Fact]
        [Trait("os", "nix_windows")]
        public async void CanExecuteEchoWithStandardErrorHandler()
        {
            var expected = "error";
            var handlerString = new StringBuilder();

            var echoResult = await Sheller
                .Shell<Bash>()
                .UseExecutable(">&2 echo")
                    .WithArgument(expected)
                    .WithStandardErrorHandler(s => handlerString.Append(s))
                .ExecuteAsync();
            
            Assert.Equal(expected, echoResult.StandardError.Trim());
            Assert.Equal(expected, handlerString.ToString());
        }

        [Fact]
        [Trait("os", "nix_windows")]
        public async void CanExecuteEchoWithResultSelector()
        {
            var expected = 0;

            var echoErrorCode = await Sheller
                .Shell<Bash>()
                .UseExecutable<Echo>()
                    .WithArgument("dummy")
                .ExecuteAsync(cr => 
                {
                    return cr.ExitCode;
                });
                
            Assert.Equal(expected, echoErrorCode);
        }

        [Fact]
        [Trait("os", "nix_windows")]
        public async void CanExecuteEchoWithResultSelectorTask()
        {
            var expected = 0;

            var echoErrorCode = await Sheller
                .Shell<Bash>()
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
        [Trait("os", "nix_windows")]
        public async void CanExecuteEchoWithWait()
        {
            var min = 2;

            var start = DateTime.Now;
            var echoValue = await Sheller
                .Shell<Bash>()
                .UseExecutable<Echo>()
                    .WithArgument("dummy")
                    .WithWait(async cr => await Task.Delay(TimeSpan.FromSeconds(min - 1)))
                    .WithWait(async cr => await Task.Delay(TimeSpan.FromSeconds(min)))
                .ExecuteAsync();
            var delta = DateTime.Now - start;

            Assert.True(delta.TotalSeconds > min);
        }

        [Fact]
        [Trait("os", "nix_windows")]
        public async void CanExecuteEchoWithWaitTimeout()
        {
            var min = 2;
            var max = 4;

            var start = DateTime.Now;
            await Assert.ThrowsAnyAsync<Exception>(() =>
            {
                return Sheller
                .Shell<Bash>()
                .UseExecutable<Echo>()
                    .WithArgument("dummy")
                    .WithWait(async cr => await Task.Delay(TimeSpan.FromSeconds(max)))
                    .WithWait(async cr => await Task.Delay(TimeSpan.FromSeconds(max + 1)))
                    .WithWaitTimeout(TimeSpan.FromSeconds(min))
                .ExecuteAsync();
            });
            var delta = DateTime.Now - start;

            Assert.True(delta.TotalSeconds > min);
            Assert.True(delta.TotalSeconds < max);
        }
    }
}
