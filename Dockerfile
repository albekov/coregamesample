FROM microsoft/dotnet:1.0.0-core

ENV ASPNETCORE_URLS="http://*:5050"
ENV ASPNETCORE_ENVIRONMENT="Production"

COPY /release /app

WORKDIR /app

EXPOSE 5050/tcp

ENTRYPOINT ["dotnet", "Game.dll"]