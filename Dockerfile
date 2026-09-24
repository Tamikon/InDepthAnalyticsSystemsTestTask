FROM mcr.microsoft.com/dotnet/sdk:10.0

WORKDIR /src
EXPOSE 8080

CMD dotnet restore && dotnet run --no-launch-profile --urls http://0.0.0.0:8080
