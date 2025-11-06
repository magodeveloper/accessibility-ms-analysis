# 🔬 Accessibility Analysis Service

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Tests](https://img.shields.io/badge/tests-381%2F383-brightgreen)](test-dashboard.html)
[![Coverage](https://img.shields.io/badge/coverage-95.6%25-brightgreen)](coverage-report/index.html)
[![License](https://img.shields.io/badge/license-Proprietary-red)](LICENSE)

> **Microservicio de análisis de accesibilidad web desarrollado en .NET 9 con Clean Architecture. Proporciona análisis WCAG 2.1/2.2 comprehensivos con soporte multi-motor y mapeo automático de criterios de conformidad.**

> ⚡ **Nota:** Este microservicio forma parte de un ecosistema donde el **Gateway** gestiona rate limiting, caching (Redis), circuit breaker y load balancing. El microservicio se enfoca en su lógica de dominio específica.

## 📋 Descripción

Microservicio empresarial para:

- **Análisis de accesibilidad** con múltiples motores (axe-core, IBM Equal Access)
- **Gestión de análisis** con consultas avanzadas por múltiples criterios
- **Gestión de resultados** con mapeo a niveles WCAG (A, AA, AAA)
- **Gestión de errores** con clasificación por tipo y severidad
- **i18n integrado** con soporte multiidioma (es, en)

## ✨ Características

### 🔬 Análisis de Accesibilidad

- **Soporte multi-motor** (axe-core, IBM Equal Access, custom)
- Mapeo automático a niveles WCAG (A, AA, AAA)
- Clasificación por severidad (Critical, Serious, Moderate, Minor)
- Análisis por herramienta configurable
- Almacenamiento persistente de análisis

### 📊 Gestión de Análisis

- **CRUD completo** de análisis con validación
- Consulta por usuario, fecha, estado, herramienta
- Filtrado avanzado multi-criterio
- Estadísticas y métricas agregadas
- Auditoría completa de análisis

### 🐛 Gestión de Errores

- **Registro detallado** de violaciones
- Clasificación por tipo y severidad
- Agrupación por análisis
- Trazabilidad completa
- Vinculación con resultados

### 📈 Gestión de Resultados

- **Resultados por nivel WCAG** (A, AA, AAA)
- Resultados por severidad
- Vinculación análisis-resultados
- Consultas optimizadas con índices
- Métricas de conformidad

### 🔒 Seguridad & Validación

- Autenticación JWT integrada
- Gateway Secret para comunicación entre servicios
- Validación con FluentValidation
- Control de acceso a operaciones sensibles

### 🌐 i18n & Accesibilidad

- Soporte multiidioma (es, en)
- Mensajes de error localizados
- Content negotiation automático
- Headers de idioma en responses

### 📊 Observabilidad & Métricas

- **Prometheus Metrics** integrado
- Métricas de negocio personalizadas
- Endpoint `/metrics` expuesto
- Monitoreo de análisis y performance
- Histogramas de duración de operaciones
- Contadores por herramienta y severidad

### 🏥 Health Checks

- Database connectivity check
- Application health monitoring
- Memory usage tracking
- Endpoints de salud personalizados

## 🏗️ Arquitectura

```
┌───────────────────────────────────────────────────┐
│        🔬 ANALYSIS MICROSERVICE API               │
│                (Port 5002)                        │
│                                                   │
│  ┌─────────────┐  ┌─────────────┐  ┌──────────┐ │
│  │ Controllers │  │  Middleware │  │  Health  │ │
│  │  (3 APIs)   │  │  (Gateway)  │  │  Checks  │ │
│  └─────────────┘  └─────────────┘  └──────────┘ │
│         │                │               │       │
│         └────────────────┴───────────────┘       │
│                      │                           │
│              ┌───────▼───────┐                   │
│              │  APPLICATION  │                   │
│              │   Services    │                   │
│              │   Use Cases   │                   │
│              └───────┬───────┘                   │
│                      │                           │
│              ┌───────▼───────┐                   │
│              │    DOMAIN     │                   │
│              │   Entities    │                   │
│              │  Interfaces   │                   │
│              └───────┬───────┘                   │
│                      │                           │
│              ┌───────▼───────┐                   │
│              │INFRASTRUCTURE │                   │
│              │   EF Core     │                   │
│              │   Repositories│                   │
│              └───────┬───────┘                   │
└──────────────────────┼───────────────────────────┘
                       │
                       ▼
               ┌──────────────┐
               │  MySQL DB    │
               │(analysis_db) │
               └──────────────┘
```

**Clean Architecture con 4 capas:**

- **API:** Controllers, Middleware, Health Checks
- **Application:** Services, DTOs, Use Cases
- **Domain:** Entities, Interfaces, Business Logic
- **Infrastructure:** EF Core, Repositories, MySQL

## 🚀 Quick Start

### Requisitos

- .NET 9.0 SDK
- MySQL 8.0+
- Docker & Docker Compose (opcional)

### Instalación Local

```bash
# Clonar repositorio
git clone https://github.com/your-org/accessibility-ms-analysis.git
cd accessibility-ms-analysis

# Configurar base de datos
mysql -u root -p < init-analysis-db.sql

# Configurar variables de entorno
cp .env.example .env
# Editar .env con tus credenciales de MySQL

# Restaurar dependencias
dotnet restore

# Compilar
dotnet build --configuration Release

# Ejecutar
dotnet run --project src/Analysis.Api/Analysis.Api.csproj
```

### Uso con Docker Compose

```bash
# Levantar todos los servicios
docker-compose up -d

# Ver logs
docker-compose logs -f analysis-api

# Verificar estado
docker-compose ps

# Detener servicios
docker-compose down
```

### Verificación

```bash
# Health check
curl http://localhost:5002/health

# Crear análisis de prueba
curl -X POST http://localhost:5002/api/analysis \
  -H "Content-Type: application/json" \
  -d '{"url":"https://example.com","userId":1,"tool":"axe-core"}'
```

## 📡 API Endpoints

### 🔬 Análisis (/api/analysis)

| Método | Endpoint                           | Descripción                  |
| ------ | ---------------------------------- | ---------------------------- |
| GET    | `/api/analysis`                    | Listar todos los análisis    |
| POST   | `/api/analysis`                    | Crear nuevo análisis         |
| GET    | `/api/analysis/{id}`               | Obtener análisis por ID      |
| DELETE | `/api/analysis/{id}`               | Eliminar análisis por ID     |
| GET    | `/api/analysis/by-user/{userId}`   | Análisis por usuario         |
| GET    | `/api/analysis/by-date`            | Análisis por rango de fechas |
| GET    | `/api/analysis/by-tool/{tool}`     | Análisis por herramienta     |
| GET    | `/api/analysis/by-status/{status}` | Análisis por estado          |
| DELETE | `/api/analysis/all`                | Eliminar todos los análisis  |

### 📊 Resultados (/api/result)

| Método | Endpoint                       | Descripción                   |
| ------ | ------------------------------ | ----------------------------- |
| GET    | `/api/result`                  | Listar todos los resultados   |
| POST   | `/api/result`                  | Crear nuevo resultado         |
| GET    | `/api/result/{id}`             | Obtener resultado por ID      |
| DELETE | `/api/result/{id}`             | Eliminar resultado por ID     |
| GET    | `/api/result/by-analysis/{id}` | Resultados por análisis       |
| GET    | `/api/result/by-level`         | Resultados por nivel WCAG     |
| GET    | `/api/result/by-severity`      | Resultados por severidad      |
| DELETE | `/api/result/all`              | Eliminar todos los resultados |

### 🐛 Errores (/api/error)

| Método | Endpoint                    | Descripción                |
| ------ | --------------------------- | -------------------------- |
| GET    | `/api/error`                | Listar todos los errores   |
| POST   | `/api/error`                | Crear nuevo error          |
| GET    | `/api/error/{id}`           | Obtener error por ID       |
| DELETE | `/api/error/{id}`           | Eliminar error por ID      |
| GET    | `/api/error/by-result/{id}` | Errores por resultado      |
| DELETE | `/api/error/all`            | Eliminar todos los errores |

### 🏥 Health (/health)

| Método | Endpoint        | Descripción          |
| ------ | --------------- | -------------------- |
| GET    | `/health`       | Health check general |
| GET    | `/health/ready` | Readiness probe      |
| GET    | `/health/live`  | Liveness probe       |

### 📊 Metrics (/metrics)

| Método | Endpoint   | Descripción         |
| ------ | ---------- | ------------------- |
| GET    | `/metrics` | Métricas Prometheus |

**Total: 27 endpoints disponibles**

## 🧪 Testing

### Estado de Cobertura

**Estado General:** ✅ 381/383 tests exitosos (99.5%)  
**Cobertura Total:** 95.6% (1194/1249 líneas cubiertas)

| Capa                        | Cobertura | Tests                    | Estado |
| --------------------------- | --------- | ------------------------ | ------ |
| **Analysis.Api**            | 95.23%    | Controllers + Middleware | ✅     |
| AnalysisController          | 95%+      | CRUD Análisis            | ✅     |
| ResultController            | 95%+      | CRUD Resultados          | ✅     |
| ErrorController             | 95%+      | CRUD Errores             | ✅     |
| **Analysis.Application**    | 95.69%    | Services + DTOs          | ✅     |
| **Analysis.Domain**         | 100%      | Entities + Interfaces    | ✅     |
| **Analysis.Infrastructure** | 0%        | Repositories + EF        | ⚠️     |

**Métricas detalladas:**

- **Cobertura de líneas:** 95.6% (1194/1249)
- **Cobertura de ramas:** 81.77%
- **Tiempo de ejecución:** ~20s para 383 tests
- **Tasa de éxito:** 99.5%

### Comandos de Testing

```bash
# Todos los tests con cobertura
.\manage-tests.ps1 -GenerateCoverage -OpenReport

# Solo tests unitarios
.\manage-tests.ps1 -TestType Unit

# Tests de integración
.\manage-tests.ps1 -TestType Integration

# Ver dashboard interactivo
Start-Process .\test-dashboard.html
```

### Categorías de Tests

**Unit Tests:**

- Validación de entidades (Analysis, Result, Error)
- Lógica de servicios (AnalysisService, ResultService, ErrorService)
- DTOs y mappers
- Validadores de dominio

**Integration Tests:**

- Controllers con base de datos en memoria
- Repositorios con MySQL real
- Health checks completos
- Middleware de gateway secret

**E2E Tests:**

- Flows completos de análisis
- Creación de análisis + resultados + errores
- Consultas por múltiples criterios
- Mapeo WCAG automático

## 📊 Observabilidad & Métricas

### Prometheus Metrics

El microservicio expone métricas detalladas en el endpoint `/metrics` para monitoreo con Prometheus/Grafana.

#### 📈 Métricas de Negocio

**Contadores (Counters):**

```
# Total de análisis realizados (por herramienta y estado)
analyses_performed_total{tool="axe_core|ibm_equal_access|custom", status="pending|completed|failed"}

# Total de consultas realizadas (por tipo de operación)
analyses_queries_total{operation="get_all|get_by_user|get_by_tool|get_by_status|get_by_date"}

# Total de resultados creados (por severidad)
analysis_results_created_total{severity="critical|serious|moderate|minor"}

# Total de errores detectados (por tipo)
analysis_errors_created_total{type="violation|warning|notice"}
```

**Histogramas (Histograms):**

```
# Duración de análisis en segundos
analysis_duration_seconds{tool="axe_core|ibm_equal_access|custom"}

# Duración de consultas en segundos
analysis_query_duration_seconds{operation="get_all|get_by_user|..."}
```

**Gauges:**

```
# Análisis actualmente en progreso
analyses_in_progress
```

#### 🔍 Consultar Métricas

```bash
# Ver todas las métricas
curl http://localhost:5002/metrics

# Filtrar métricas específicas
curl http://localhost:5002/metrics | grep "analyses_performed_total"

# Verificar análisis en progreso
curl http://localhost:5002/metrics | grep "analyses_in_progress"
```

#### 📊 Dashboard Grafana (Ejemplo)

```yaml
# Panel 1: Análisis por herramienta
sum(rate(analyses_performed_total[5m])) by (tool)

# Panel 2: Tasa de errores
rate(analyses_performed_total{status="failed"}[5m])

# Panel 3: Duración promedio de análisis
histogram_quantile(0.95, rate(analysis_duration_seconds_bucket[5m]))

# Panel 4: Análisis en curso
analyses_in_progress
```

### Health Checks

```bash
# Health check básico
curl http://localhost:5002/health

# Readiness (listo para recibir tráfico)
curl http://localhost:5002/health/ready

# Liveness (proceso está vivo)
curl http://localhost:5002/health/live
```

**Respuesta Health Check:**

```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.0234567",
  "entries": {
    "database": {
      "status": "Healthy",
      "description": "Database connection is healthy",
      "duration": "00:00:00.0123456"
    },
    "self": {
      "status": "Healthy",
      "description": "Analysis API is running",
      "duration": "00:00:00.0001234"
    }
  }
}
```

## � Arquitectura de Seguridad

### Flujo de Autenticación

```
┌──────────┐    JWT    ┌─────────┐   X-User-*   ┌──────────┐   IUserContext   ┌────────────┐
│  Client  │ ───────>  │ Gateway │ ──────────>  │ Middleware│ ──────────────> │ Controller │
└──────────┘           └─────────┘              └──────────┘                 └────────────┘
                           │                          │                            │
                       Valida JWT              Extrae headers              Valida IsAuthenticated
                       + Secret               + Crea UserContext           + Usa UserId/Role
```

### Middleware Stack

**1. GatewaySecretValidationMiddleware**

```csharp
// Valida que la petición provenga del Gateway autorizado
// Header requerido: X-Gateway-Secret
// Configuración: appsettings.json -> GatewaySettings:Secret
```

**2. UserContextMiddleware**

```csharp
// Extrae información de usuario de headers propagados por Gateway
// Headers procesados:
//   - X-User-Id          → UserId
//   - X-User-Email       → Email
//   - X-User-Role        → Role
//   - X-User-Name        → Name

// Inyecta IUserContext en controllers vía DI
```

### IUserContext Interface

```csharp
public interface IUserContext
{
    bool IsAuthenticated { get; }
    Guid UserId { get; }
    string Email { get; }
    string Role { get; }
    string Name { get; }
}
```

### Uso en Controllers

```csharp
public class AnalysisController(
    IAnalysisService service,
    IUserContext userContext) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAnalysisDto dto)
    {
        // ✅ Validación de autenticación
        if (!userContext.IsAuthenticated)
            return Unauthorized(new { message = "User not authenticated" });

        // ✅ Usar UserId del contexto (no del body)
        dto.UserId = userContext.UserId;

        var result = await service.CreateAnalysisAsync(dto);
        return CreatedAtAction(...);
    }
}
```

### Configuración de Seguridad

**appsettings.json:**

```json
{
  "GatewaySettings": {
    "Secret": "your-secret-key-here",
    "AllowedOrigins": ["http://localhost:8080"]
  },
  "Jwt": {
    "SecretKey": "your-jwt-secret-key-min-32-chars",
    "Issuer": "accessibility-gateway",
    "Audience": "accessibility-microservices"
  }
}
```

**Generar Secrets:**

```powershell
# Generar JWT Secret Key
.\Generate-JwtSecretKey.ps1

# Validar configuración JWT
.\Validate-JwtConfig.ps1
```

## �🐳 Deployment

### Docker

```dockerfile
# Build image
docker build -t accessibility-analysis:latest .

# Run standalone
docker run -d \
  --name analysis-api \
  -p 5002:5002 \
  -e ConnectionStrings__AnalysisDb="Server=mysql;Database=analysis_db;..." \
  -e GatewaySecret="your-gateway-secret" \
  accessibility-analysis:latest
```

### Docker Compose

```yaml
version: "3.8"

services:
  analysis-api:
    image: accessibility-analysis:latest
    ports:
      - "5002:5002"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__AnalysisDb=Server=mysql-analysis;Database=analysis_db;Uid=root;Pwd=password
      - GatewaySecret=your-gateway-secret
    depends_on:
      - mysql-analysis
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:5002/health"]
      interval: 30s

  mysql-analysis:
    image: mysql:8.0
    ports:
      - "3308:3306"
    environment:
      - MYSQL_ROOT_PASSWORD=password
      - MYSQL_DATABASE=analysis_db
    volumes:
      - mysql-analysis-data:/var/lib/mysql
      - ./init-analysis-db.sql:/docker-entrypoint-initdb.d/init.sql

volumes:
  mysql-analysis-data:
```

## ⚙️ Configuración

### Variables de Entorno

```bash
# ASP.NET Core
ASPNETCORE_ENVIRONMENT=Production|Development
ASPNETCORE_URLS=http://+:5002

# Base de Datos
ConnectionStrings__AnalysisDb=Server=localhost;Database=analysis_db;Uid=root;Pwd=password

# Gateway Configuration
GatewaySecret=your-super-secret-gateway-key

# Localization
DefaultLanguage=es
SupportedLanguages=es,en

# Logging
Serilog__MinimumLevel=Information
Serilog__WriteTo__Console=true
```

### Configuración de Base de Datos

```sql
-- Crear base de datos
CREATE DATABASE analysis_db CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- Ejecutar script de inicialización
SOURCE init-analysis-db.sql;
```

## 🛠️ Scripts & Utilidades

### PowerShell Scripts

**1. Generate-JwtSecretKey.ps1**

Genera una clave secreta segura para JWT de forma automática.

```powershell
# Ejecutar script
.\Generate-JwtSecretKey.ps1

# Salida:
# ✅ JWT Secret Key generada exitosamente
# 🔑 Clave: AbCdEf12...GhIjKl34 (32+ caracteres)
# 📝 Agregar en appsettings.json -> Jwt:SecretKey
```

**Características:**

- Genera claves de 32+ caracteres automáticamente
- Usa RNGCryptoServiceProvider (cryptographically secure)
- Valida longitud mínima requerida
- Formato Base64 URL-safe

**2. Validate-JwtConfig.ps1**

Valida la configuración JWT en `appsettings.json` antes de deployment.

```powershell
# Ejecutar validación
.\Validate-JwtConfig.ps1

# Salida exitosa:
# ✅ Jwt:SecretKey existe
# ✅ Longitud: 44 caracteres (>= 32 requeridos)
# ✅ Jwt:Issuer configurado: accessibility-gateway
# ✅ Jwt:Audience configurado: accessibility-microservices
# ✅ Configuración JWT válida para producción

# Salida con errores:
# ❌ Jwt:SecretKey no encontrada en appsettings.json
# ❌ SecretKey muy corta: 16 caracteres (mínimo 32)
# ⚠️ Considere ejecutar .\Generate-JwtSecretKey.ps1
```

**Validaciones:**

- ✅ Existencia de `Jwt:SecretKey`
- ✅ Longitud mínima (32 caracteres)
- ✅ Configuración de `Issuer` y `Audience`
- ✅ Formato JSON válido

**3. init-test-databases.ps1 / .sh**

Inicializa bases de datos de prueba para testing local.

```powershell
# Windows
.\init-test-databases.ps1

# Linux/Mac
chmod +x init-test-databases.sh
./init-test-databases.sh
```

**Funcionalidad:**

- Crea contenedor MySQL para testing
- Ejecuta script `init-analysis-db.sql`
- Configura usuario y permisos
- Verifica conexión antes de salir

**Variables de entorno requeridas:**

```env
MYSQL_TEST_ROOT_PASSWORD=test_root_pass
MYSQL_TEST_DATABASE=analysis_test_db
MYSQL_TEST_USER=test_user
MYSQL_TEST_PASSWORD=test_pass
```

**4. manage-tests.ps1**

Script unificado para ejecutar tests con diferentes configuraciones.

```powershell
# Ejecutar todos los tests
.\manage-tests.ps1

# Tests con cobertura
.\manage-tests.ps1 -Coverage

# Tests por categoría
.\manage-tests.ps1 -Category Unit
.\manage-tests.ps1 -Category Integration

# Tests con reporte HTML
.\manage-tests.ps1 -Coverage -HtmlReport
```

**Características:**

- Ejecuta xUnit con configuración personalizada
- Genera reportes de cobertura (Coverlet)
- Filtra por categoría (Unit/Integration/E2E)
- Exporta resultados a `TestResults/`

### C# Utilities

**DatabaseManager.cs**

Utilidad para gestión de esquema de base de datos en testing.

```csharp
public class DatabaseManager
{
    // Crear esquema completo desde cero
    public static async Task CreateSchemaAsync(string connectionString);

    // Limpiar todos los datos (mantiene estructura)
    public static async Task CleanDatabaseAsync(string connectionString);

    // Resetear base de datos (drop + recreate)
    public static async Task ResetDatabaseAsync(string connectionString);

    // Verificar conexión
    public static async Task<bool> CanConnectAsync(string connectionString);
}
```

**Uso en tests:**

```csharp
[Fact]
public async Task Integration_Test_Example()
{
    // Arrange: Limpiar estado previo
    await DatabaseManager.CleanDatabaseAsync(_connectionString);

    // Act: Ejecutar test
    var result = await _service.CreateAnalysisAsync(dto);

    // Assert
    Assert.NotNull(result);
}
```

### SQL Scripts

**init-analysis-db.sql**

Script de inicialización de base de datos con esquema completo.

```sql
-- Estructura:
-- 1. Creación de base de datos
-- 2. Tablas principales (analyses, results, errors)
-- 3. Índices para performance
-- 4. Foreign keys y relaciones
-- 5. Usuario y permisos

-- Ejecutar manualmente:
mysql -u root -p < init-analysis-db.sql
```

**Tablas creadas:**

- `analyses` - Análisis de accesibilidad
- `results` - Resultados WCAG por análisis
- `errors` - Errores detectados por análisis

---

**IMPORTANT:** This software and associated documentation files (the "Software") are the exclusive property of Geovanny Camacho and are protected by copyright laws and international treaty provisions.

```

## 🛠️ Stack Tecnológico

- **Runtime:** .NET 9.0
- **Framework:** ASP.NET Core Web API
- **ORM:** Entity Framework Core 9.0
- **Database:** MySQL 8.0+
- **Authentication:** Gateway Secret
- **Logging:** Serilog
- **Testing:** xUnit + Moq + FluentAssertions
- **Coverage:** Coverlet + ReportGenerator
- **Container:** Docker + Docker Compose

## � License

**Proprietary Software License v1.0**

Copyright (c) 2025 Geovanny Camacho. All rights reserved.

**IMPORTANT:** This software and associated documentation files (the "Software") are the exclusive property of Geovanny Camacho and are protected by copyright laws and international treaty provisions.

### TERMS AND CONDITIONS

1. **OWNERSHIP**: The Software is licensed, not sold. Geovanny Camacho retains all right, title, and interest in and to the Software, including all intellectual property rights.

2. **RESTRICTIONS**: You may NOT:

   - Copy, modify, or create derivative works of the Software
   - Distribute, transfer, sublicense, lease, lend, or rent the Software
   - Reverse engineer, decompile, or disassemble the Software
   - Remove or alter any proprietary notices or labels on the Software
   - Use the Software for any commercial purpose without explicit written permission
   - Share access credentials or allow unauthorized access to the Software

3. **CONFIDENTIALITY**: The Software contains trade secrets and confidential information. You agree to maintain the confidentiality of the Software and not disclose it to any third party.

4. **TERMINATION**: This license is effective until terminated. Your rights under this license will terminate automatically without notice if you fail to comply with any of its terms.

5. **NO WARRANTY**: THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.

6. **LIMITATION OF LIABILITY**: IN NO EVENT SHALL GEOVANNY CAMACHO BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

7. **GOVERNING LAW**: This license shall be governed by and construed in accordance with the laws of the jurisdiction in which Geovanny Camacho resides, without regard to its conflict of law provisions.

8. **ENTIRE AGREEMENT**: This license constitutes the entire agreement between you and Geovanny Camacho regarding the Software and supersedes all prior or contemporaneous understandings.

**FOR LICENSING INQUIRIES:**
Geovanny Camacho
Email: fgiocl@outlook.com

**By using this Software, you acknowledge that you have read this license, understand it, and agree to be bound by its terms and conditions.**

---

**Author:** Geovanny Camacho (fgiocl@outlook.com)
**Last Update:** 05/11/2025
```
