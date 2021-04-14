# escape=`
FROM mcr.microsoft.com/dotnet/framework/aspnet:4.8-windowsservercore-ltsc2019

RUN Invoke-Expression (New-Object System.Net.WebClient).DownloadString('https://get.scoop.sh')

RUN scoop install git; `
    scoop bucket add extras; `
    scoop install chromium chromedriver
