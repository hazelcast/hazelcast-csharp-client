#
# Official .NET SDK images build is:
# https://github.com/dotnet/dotnet-docker
# https://github.com/microsoft/containerregistry
#
# Official .NET SDK images are listed here:
# https://hub.docker.com/_/microsoft-dotnet-sdk/
#
# As of May 20th, 2021, version 'latest' aka '5.0' builds on Debian 10 'Buster'
# We also need versions of the 2.1 and 3.1 .NET SDK so simply using:
#   FROM mcr.microsoft.com/dotnet/sdk:5.0
# Cannot work. Instead, we build our own image by deriving from from the original
# Debian 10 'Buster' with v5.0, and adding the missing bits.
#
# src: src/sdk/5.0/buster-slim/amd64/Dockerfile

ARG REPO=mcr.microsoft.com/dotnet/aspnet
FROM $REPO:5.0-buster-slim-amd64

ENV \
    # Unset ASPNETCORE_URLS from aspnet base image
    ASPNETCORE_URLS= \
    DOTNET_SDK_VERSION=5.0.300 \
    # Enable correct mode for dotnet watch (only mode supported in a container)
    DOTNET_USE_POLLING_FILE_WATCHER=true \
    # Skip extraction of XML docs - generally not useful within an image/container - helps performance
    NUGET_XMLDOC_MODE=skip \
    # PowerShell telemetry for docker image usage
    POWERSHELL_DISTRIBUTION_CHANNEL=PSDocker-DotnetSDK-Debian-10

# Install Java (required to run tests)
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        curl \
        git \
        procps \
        wget \
        tofrodos \
    && mkdir -p /usr/share/man/man1 \
    && apt-get install -y --no-install-recommends \
        openjdk-11-jdk ca-certificates-java \
    && rm -rf /var/lib/apt/lists/*

# Install .NET SDK v2.1
RUN dotnet_sdk_version=2.1.816 \
    && curl -SL --output dotnet.tar.gz https://dotnetcli.azureedge.net/dotnet/Sdk/$dotnet_sdk_version/dotnet-sdk-$dotnet_sdk_version-linux-x64.tar.gz \
    && dotnet_sha512='58f0bc1f67de034ffd0dafb9c0fdb082786fc5057e89396ff574428d57331cd8d5b3e944e103918e05f7b66e354d56cdb242350a6ef932906c9c3d4b08d177e9' \
    && echo "$dotnet_sha512 dotnet.tar.gz" | sha512sum -c - \
    && mkdir -p /usr/share/dotnet \
    && tar -zxf dotnet.tar.gz -C /usr/share/dotnet \
    && rm dotnet.tar.gz

# Install .NET SDK v3.1
RUN dotnet_sdk_version=3.1.409 \
    && curl -SL --output dotnet.tar.gz https://dotnetcli.azureedge.net/dotnet/Sdk/$dotnet_sdk_version/dotnet-sdk-$dotnet_sdk_version-linux-x64.tar.gz \
    && dotnet_sha512='63d24f1039f68abc46bf40a521f19720ca74a4d89a2b99d91dfd6216b43a81d74f672f74708efa6f6320058aa49bf13995638e3b8057efcfc84a2877527d56b6' \
    && echo "$dotnet_sha512 dotnet.tar.gz" | sha512sum -c - \
    && mkdir -p /usr/share/dotnet \
    && tar -ozxf dotnet.tar.gz -C /usr/share/dotnet \
    && rm dotnet.tar.gz

# Install .NET SDK v5.0
RUN dotnet_sdk_version=$DOTNET_SDK_VERSION \
    && curl -SL --output dotnet.tar.gz https://dotnetcli.azureedge.net/dotnet/Sdk/$dotnet_sdk_version/dotnet-sdk-$dotnet_sdk_version-linux-x64.tar.gz \
    && dotnet_sha512='724a8e6ed77d2d3b957b8e5eda82ca8c99152d8691d1779b4a637d9ff781775f983468ee46b0bc8ad0ddbfd9d537dd8decb6784f43edae72c9529a90767310d2' \
    && echo "$dotnet_sha512 dotnet.tar.gz" | sha512sum -c - \
    && mkdir -p /usr/share/dotnet \
    && tar -C /usr/share/dotnet -oxzf dotnet.tar.gz ./packs ./sdk ./templates ./LICENSE.txt ./ThirdPartyNotices.txt \
    && rm dotnet.tar.gz \
    #&& ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet \
    # Trigger first run experience by running arbitrary cmd
    && dotnet help

# Install PowerShell global tool
RUN powershell_version=7.1.3 \
    && curl -SL --output PowerShell.Linux.x64.$powershell_version.nupkg https://pwshtool.blob.core.windows.net/tool/$powershell_version/PowerShell.Linux.x64.$powershell_version.nupkg \
    && powershell_sha512='537d885b79dd1cd183d14b5f5e71046558fb015f562bb817ee90fbabaa9b1039c822949b7e1a5c9b69a976eae09786e3b2c0f0586c01c822868cc48ea7e36620' \
    && echo "$powershell_sha512  PowerShell.Linux.x64.$powershell_version.nupkg" | sha512sum -c - \
    && mkdir -p /usr/share/powershell \
    && dotnet tool install --add-source / --tool-path /usr/share/powershell --version $powershell_version PowerShell.Linux.x64 \
    && dotnet nuget locals all --clear \
    && rm PowerShell.Linux.x64.$powershell_version.nupkg \
    && ln -s /usr/share/powershell/pwsh /usr/bin/pwsh \
    && chmod 755 /usr/share/powershell/pwsh \
    # To reduce image size, remove the copy nupkg that nuget keeps.
    && find /usr/share/powershell -print | grep -i '.*[.]nupkg$' | xargs rm

# Our GitHub Actions entry point

ADD entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh \
    && fromdos /entrypoint.sh
ENTRYPOINT ["/entrypoint.sh"]

#eof