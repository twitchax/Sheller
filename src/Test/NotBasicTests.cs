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
                        .WithKubeConfig("fake")
                        .WithKubeConfig("path")
                    .ExecuteAsync();
            });
        }
    }
}