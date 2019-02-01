FROM microsoft/dotnet:2.2-sdk-bionic
WORKDIR /sheller
COPY . .

ENTRYPOINT dotnet test --filter os~nix /sheller