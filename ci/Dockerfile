# Universal, base image
#   See: https://registry.hub.docker.com/_/microsoft-vscode-devcontainers?tab=description
#   See: https://github.com/microsoft/vscode-dev-containers/tree/master/containers/codespaces-linux
FROM mcr.microsoft.com/vscode/devcontainers/universal:linux AS build-environment

USER root

# Install CICEE and make sure .NET global tools are added to path
RUN dotnet tool install -g cicee
ENV PATH="${PATH}:/root/.dotnet/tools"
