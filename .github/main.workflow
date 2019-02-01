workflow "Dotnet Build and Test" {
  on = "push"
  resolves = [
    "nuget push",
    "build and test",
  ]
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
  args = "pack -c Release --include-symbols"
}

action "nuget push" {
  uses = "docker://microsoft/dotnet:2.2-sdk-bionic"
  needs = ["pack"]
  runs = "dotnet"
  args = "nuget push src/Core/bin/Release/Sheller.*.nupkg -k $NUGET_KEY -s https://www.nuget.org/api/v2/package"
  secrets = ["NUGET_KEY"]
}

workflow "New workflow" {
  on = "push"
  resolves = ["actions/bin/echo-1"]
}

action "actions/bin/echo" {
  uses = "actions/bin/echo@master"
  args = "$FAKE"
  secrets = ["FAKE"]
}

action "actions/bin/echo-1" {
  uses = "actions/bin/echo@master"
  needs = ["actions/bin/echo"]
  args = "$OTHER"
  env = {
    OTHER = "cool"
  }
}
