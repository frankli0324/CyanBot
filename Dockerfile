FROM mcr.microsoft.com/dotnet/runtime-deps:5.0-buster-slim-amd64

COPY ./bin/Release/net5.0/linux-x64/publish/[^C]* /app/
COPY ./bin/Release/net5.0/linux-x64/publish/C[^y]* /app/
COPY ./bin/Release/net5.0/linux-x64/publish/Cyan* /app/
WORKDIR /opt

ENV TZ=Asia/Shanghai
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone

CMD [ "/app/CyanBot" ]
VOLUME [ "/opt" ]