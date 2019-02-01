workflow "Release" {
  resolves = [
    "nuget push"
  ]
  on = "release"
}

action "build and test" {
  uses = "docker://microsoft/dotnet:2.2-sdk-bionic"
  runs = "dotnet"
  args = "test --filter os~nix"
}

action "if (branch == master)" {
  uses = "actions/bin/filter@c6471707d308175c57dfe91963406ef205837dbd"
  args = "branch master"
  needs = [
    "build and test",
  ]
}

action "pack" {
  uses = "docker://microsoft/dotnet:2.2-sdk-bionic"
  needs = ["if (branch == master)"]
  runs = "dotnet"
  args = "pack -c Release"
}

action "nuget push" {
  uses = "docker://microsoft/dotnet:2.2-sdk-bionic"
  needs = ["pack"]
  runs = "dotnet"
  args = "nuget push src/Core/bin/Release/Sheller.*.nupkg -k $NUGET_KEY -s https://www.nuget.org/api/v2/package"
  secrets = ["NUGET_KEY"]
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
