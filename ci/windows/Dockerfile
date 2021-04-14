FROM mcr.microsoft.com/dotnet/framework/aspnet:4.8-windowsservercore-ltsc2019

RUN Invoke-Expression (New-Object System.Net.WebClient).DownloadString('https://get.scoop.sh')

RUN scoop install git
RUN scoop bucket add extras
RUN scoop install chromium chromedriver
