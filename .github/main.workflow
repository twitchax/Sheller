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
  uses = "twitchax/actions/dotnet/cli@master"
  needs = ["pack"]
  secrets = ["NUGET_KEY"]
  args = "nuget push src/Core/bin/Release/Sheller.*.nupkg -k $NUGET_KEY -s https://www.nuget.org/api/v2/package"
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
