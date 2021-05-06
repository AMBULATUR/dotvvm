# escape=`
FROM mcr.microsoft.com/dotnet/sdk:3.1 as dotnet-installer


# export fonts from full-windows (based on https://stackoverflow.com/a/65655141)
FROM mcr.microsoft.com/windows:20H2 as full-windows
SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]
RUN Copy-Item -Path C:\Windows\Fonts -Exclude lucon.ttf -Destination C:\tmp\fonts -Recurse; `
    New-Item -ItemType Directory -Force -Path C:\tmp\registries; `
    reg export 'HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts' C:\tmp\registries\Fonts.reg; `
    reg export 'HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\FontLink\SystemLink' C:\tmp\registries\FontLink.reg;


FROM mcr.microsoft.com/dotnet/framework/aspnet:4.8-windowsservercore-ltsc2019

# import fonts from full-windows
COPY --from=full-windows /tmp/fonts/ /Windows/Fonts/
COPY --from=full-windows /tmp/registries/ /tmp/registries/
RUN reg import C:\tmp\registries\Fonts.reg; reg import C:\tmp\registries\FontLink.reg;

RUN Invoke-Expression (New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1')

RUN choco install -y googlechrome chromedriver

COPY --from=dotnet-installer ["/Program Files/dotnet", "/Program Files/dotnet"]

RUN setx /M PATH \"${env:PATH};C:\Program Files\dotnet\"
RUN dotnet help

ENTRYPOINT []
