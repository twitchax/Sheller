action "build" {
  uses = "actions/docker/cli@c08a5fc9e0286844156fefff2c141072048141f6"
  args = "build -t twitchax/shellertest ."
}

action "test" {
  uses = "actions/docker/cli@c08a5fc9e0286844156fefff2c141072048141f6"
  needs = ["build"]
  args = "run twitchax/shellertest"
}

workflow "Dotnet Build and Test" {
  on = "push"
  resolves = ["nuget push", "build and test"]
}

action "build and test" {
  uses = "docker://microsoft/dotnet:2.2-sdk-bionic"
  runs = "dotnet"
  args = "test --filter os~nix"
}

action "if (branch == master)" {
  uses = "actions/bin/filter@c6471707d308175c57dfe91963406ef205837dbd"
  args = "branch master"
  needs = ["build and test"]
}

action "pack" {
  uses = "docker://microsoft/dotnet:2.2-sdk-bionic"
  needs = ["if (branch == master)"]
  runs = "dotnet"
  args = "pack -c Release --include-symbols"
}

action "nuget push" {
  uses = "docker://microsoft/dotnet:2.2-sdk-bionic"
  needs = ["pack"]
  runs = "dotnet"
  args = "nuget push src/Core/bin/Release/Sheller.*.nupkg -k $NUGET_KEY -s https://www.nuget.org/api/v2/package ;"
  secrets = ["NUGET_KEY"]
}
