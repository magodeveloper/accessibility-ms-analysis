# ============ STAGE 1: build ============
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Analysis.sln ./
COPY Directory.Packages.props ./
COPY Analysis.Api/Analysis.Api.csproj Analysis.Api/
COPY Analysis.Application/Analysis.Application.csproj Analysis.Application/
COPY Analysis.Domain/Analysis.Domain.csproj Analysis.Domain/
COPY Analysis.Infrastructure/Analysis.Infrastructure.csproj Analysis.Infrastructure/
COPY Analysis.Tests/Analysis.Tests.csproj Analysis.Tests/

RUN dotnet restore Analysis.sln

COPY . .
RUN dotnet publish ./Analysis.Api/Analysis.Api.csproj -c Release -o /app/publish

# ============ STAGE 2: runtime ============
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
EXPOSE 8082
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Analysis.Api.dll", "--urls", "http://0.0.0.0:8082"]
