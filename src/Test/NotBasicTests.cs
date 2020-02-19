using System;
using Sheller.Implementations.Executables;
using Sheller.Implementations.Shells;
using Xunit;

namespace Sheller.Tests
{
    // This sounded better than "advanced", and I am currently too lazy to split tests out in some genius fashion.
    public class NotBasicTests
    {
        [Fact]
        [Trait("os", "nix_win")]
        public async void CanFailOnKubectlKubeConfigCall()
        {
            await Assert.ThrowsAnyAsync<InvalidOperationException>(async () =>
            {
                await Builder
                    .UseShell<Bash>()
                    .UseExecutable<Kubectl>()
                        .WithApply("fake.yml")
                        .WithKubeConfig("fake")
                        .WithKubeConfig("path")
                    .ExecuteAsync();
            });
        }

        [Fact]
        [Trait("os", "nix_win")]
        public async void CanFailOnHelmKubeConfigCall()
        {
            await Assert.ThrowsAnyAsync<InvalidOperationException>(async () =>
            {
                await Builder
                    .UseShell<Bash>()
                    .UseExecutable<Helm>()
                        .WithKubeConfig("fake")
                        .WithKubeConfig("path")
                    .ExecuteAsync();
            });
        }

        [Fact]
        [Trait("os", "win")]
        public async void CanUseCmd()
        {
            var expected = "lol";

            var echoValue = await Builder
                .UseShell<Cmd>()
                .UseExecutable("echo")
                    .WithArgument(expected)
                .ExecuteAsync();

            Assert.True(echoValue.Succeeded);
            Assert.Equal(0, echoValue.ExitCode);
            Assert.Equal(expected, echoValue.StandardOutput.Trim());
        }
    }
}