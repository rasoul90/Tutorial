# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY DeliverySaaS.sln ./
COPY src/DeliverySaaS.API/DeliverySaaS.API.csproj src/DeliverySaaS.API/
COPY src/DeliverySaaS.Application/DeliverySaaS.Application.csproj src/DeliverySaaS.Application/
COPY src/DeliverySaaS.Domain/DeliverySaaS.Domain.csproj src/DeliverySaaS.Domain/
COPY src/DeliverySaaS.Infrastructure/DeliverySaaS.Infrastructure.csproj src/DeliverySaaS.Infrastructure/
COPY tests/DeliverySaaS.Tests/DeliverySaaS.Tests.csproj tests/DeliverySaaS.Tests/

RUN dotnet restore src/DeliverySaaS.API/DeliverySaaS.API.csproj

COPY . .
RUN dotnet publish src/DeliverySaaS.API/DeliverySaaS.API.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "DeliverySaaS.API.dll"]
