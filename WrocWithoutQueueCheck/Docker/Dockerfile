FROM mcr.microsoft.com/dotnet/sdk:8.0

RUN mkdir /wwqc-src
RUN mkdir /wwqc-publish
RUN mkdir /wwqc-publish/Logs
RUN mkdir /wwqc-publish/Hits
RUN mkdir /wwqc-publish/Temp

COPY . /wwqc-src/.

RUN dotnet publish /wwqc-src/WrocWithoutQueueCheck/WrocWithoutQueueCheck.csproj --output /wwqc-publish --runtime linux-x64
RUN chmod +x /wwqc-publish/WrocWithoutQueueCheck

RUN apt-get update && apt-get install libasound2 libgtk-3-dev -y && apt-get upgrade -y

WORKDIR /wwqc-publish

ENTRYPOINT ["/bin/sh", "-c", "./WrocWithoutQueueCheck"]