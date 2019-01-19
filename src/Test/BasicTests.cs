using System;
using Sheller.Implementations;
using Sheller.Implementations.Executables;
using Sheller.Implementations.Shells;
using Xunit;

namespace Sheller.Tests
{
    public class BasicTests
    {
        [Fact]
        public async void CanExecuteEcho()
        {
            var varValue = "lol";

            var echoValue = await Sheller
                .Shell("/bin/bash")
                .UseExecutable("echo")
                    .WithArgument(varValue)
                .ExecuteAsync();

            Assert.Equal(varValue, echoValue.StandardOutput.Trim());
        }

        [Fact]
        public async void CanExecuteEchoWithGeneric()
        {
            var varValue = "lol";

            var echoValue = await Sheller
                .Shell<Bash>()
                .UseExecutable<Echo>()
                    .WithArgument(varValue)
                .ExecuteAsync();

            Assert.Equal(varValue, echoValue);
        }


        [Fact]
        public async void CanExecuteEchoWithGenericAndEnvironmentVariable()
        {
            var varValue = "lol";

            var echoValue = await Sheller
                .Shell<Bash>()
                    .WithEnvironmentVariable("MY_VAR", varValue)
                .UseExecutable<Echo>()
                    .WithArgument("$MY_VAR")
                .ExecuteAsync();

            Assert.Equal(varValue, echoValue);
        }
    }
}
