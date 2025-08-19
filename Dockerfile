# ============ STAGE 1: build ============
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Analysis.sln ./
COPY Directory.Packages.props ./
COPY src/Analysis.Api/Analysis.Api.csproj                      src/Analysis.Api/
COPY src/Analysis.Application/Analysis.Application.csproj      src/Analysis.Application/
COPY src/Analysis.Domain/Analysis.Domain.csproj                src/Analysis.Domain/
COPY src/Analysis.Infrastructure/Analysis.Infrastructure.csproj src/Analysis.Infrastructure/
COPY src/Analysis.Tests/Analysis.Tests.csproj                  src/Analysis.Tests/

RUN dotnet restore Analysis.sln

COPY . .
RUN dotnet publish ./src/Analysis.Api/Analysis.Api.csproj -c Release -o /app/publish

# ============ STAGE 2: runtime ============
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
EXPOSE 8082
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Analysis.Api.dll", "--urls", "http://0.0.0.0:8082"]