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
            var expected = "lol";

            await Assert.ThrowsAnyAsync<InvalidOperationException>(async () =>
            {
                await Sheller
                    .Shell<Bash>()
                    .UseExecutable<Kubectl>()
                        .WithKubeConfig("fake")
                        .WithKubeConfig("path")
                    .ExecuteAsync();
            });
        }
    }
}