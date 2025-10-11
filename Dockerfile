# ============================================================================
# Multi-stage Dockerfile para Microservicio Analysis
# ============================================================================
# STAGE 1: Build - Compila la aplicación
# STAGE 2: Runtime - Imagen optimizada para producción
# ============================================================================

# ============ STAGE 1: build ============
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar global.json para asegurar versión correcta del SDK
COPY global.json ./

# Copiar archivos de solución y gestión de paquetes
COPY Analysis.sln ./
COPY Directory.Packages.props ./

# Copiar archivos .csproj (optimiza cache de Docker - solo se invalida si cambian las dependencias)
COPY src/Analysis.Api/Analysis.Api.csproj                      src/Analysis.Api/
COPY src/Analysis.Application/Analysis.Application.csproj      src/Analysis.Application/
COPY src/Analysis.Domain/Analysis.Domain.csproj                src/Analysis.Domain/
COPY src/Analysis.Infrastructure/Analysis.Infrastructure.csproj src/Analysis.Infrastructure/
COPY src/Analysis.Tests/Analysis.Tests.csproj                  src/Analysis.Tests/

# Restaurar dependencias (capa independiente para aprovechar cache)
RUN dotnet restore Analysis.sln

# Copiar el resto del código fuente
COPY src/ src/

# Publicar aplicación (--no-restore evita restore duplicado)
RUN dotnet publish ./src/Analysis.Api/Analysis.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# ============ STAGE 2: runtime ============
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Crear usuario no-root para seguridad
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Copiar binarios publicados desde stage de build
COPY --from=build /app/publish .

# Cambiar permisos y usuario
RUN chown -R appuser:appuser /app
USER appuser

# Exponer puerto
EXPOSE 8082

# Health check integrado
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:8082/health || exit 1

# Punto de entrada
ENTRYPOINT ["dotnet", "Analysis.Api.dll", "--urls", "http://0.0.0.0:8082"]