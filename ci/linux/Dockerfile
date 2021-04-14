FROM mcr.microsoft.com/dotnet/sdk:3.1

ENV \
    LANG=C.UTF-8 \
    LC_ALL=C.UTF-8 \
    DEBIAN_FRONTEND=noninteractive \
    DEBCONF_NONINTERACTIVE_SEEN=true

RUN apt-get -q update \
    && apt-get -q install -y --no-install-recommends \
        npm \
        chromium \
        chromium-driver \
        xvfb \
    && apt-get -q clean
