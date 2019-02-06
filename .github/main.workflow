workflow "Release" {
  resolves = [
    "nuget push",
  ]
  on = "release"
}

action "build and test" {
  uses = "twitchax/actions/dotnet/cli@master"
  args = "test --filter os~nix "
}

action "pack" {
  uses = "twitchax/actions/dotnet/cli@master"
  args = "pack -c Release"
}

action "nuget push" {
  uses = "twitchax/actions/dotnet/nuget-push@master"
  needs = ["pack"]
  secrets = ["NUGET_KEY"]
  args = "**/Sheller.*.nupkg"
}

workflow "Build and Test" {
  resolves = [
    "build and test",
  ]
  on = "push"
}

workflow "Pull Request" {
  resolves = [
    "build and test",
  ]
  on = "pull_request"
}
