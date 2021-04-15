# escape=`
FROM mcr.microsoft.com/dotnet/sdk:3.1 as dotnet-installer

FROM mcr.microsoft.com/dotnet/framework/aspnet:4.8-windowsservercore-ltsc2019

RUN Invoke-Expression (New-Object System.Net.WebClient).DownloadString('https://get.scoop.sh')

RUN scoop install git; `
    scoop bucket add extras; `
    scoop install chromium chromedriver

COPY --from=dotnet-installer ["/Program Files/dotnet", "/Program Files/dotnet"]

RUN setx /M PATH \"${env:PATH};C:\Program Files\dotnet\"
RUN dotnet help

ENTRYPOINT []
