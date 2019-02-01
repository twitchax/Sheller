workflow "Build and Test" {
  on = "push"
  resolves = ["test"]
}

action "build" {
  uses = "actions/docker/cli@c08a5fc9e0286844156fefff2c141072048141f6"
  args = "build -t twitchax/shellertest ."
}

action "test" {
  uses = "actions/docker/cli@c08a5fc9e0286844156fefff2c141072048141f6"
  needs = ["build"]
  args = "run twitchax/shellertest"
}
