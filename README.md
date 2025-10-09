# 🔬 Accessibility Analysis Service

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Tests](https://img.shields.io/badge/tests-444%2F444-brightgreen)](test-dashboard.html)
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
| PUT    | `/api/analysis/{id}`               | Actualizar análisis          |
| DELETE | `/api/analysis/{id}`               | Eliminar análisis por ID     |
| GET    | `/api/analysis/by-user/{userId}`   | Análisis por usuario         |
| GET    | `/api/analysis/by-date`            | Análisis por rango de fechas |
| GET    | `/api/analysis/by-tool/{tool}`     | Análisis por herramienta     |
| GET    | `/api/analysis/by-status/{status}` | Análisis por estado          |
| DELETE | `/api/analysis/all`                | Eliminar todos los análisis  |

### � Resultados (/api/result)

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

**Total: 29 endpoints disponibles**

## 🧪 Testing

### Estado de Cobertura

**Estado General:** ✅ 444/444 tests exitosos (100%)  
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
- **Tiempo de ejecución:** ~20s para 444 tests
- **Tasa de éxito:** 100%

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

## 🐳 Deployment

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
**Last Update:** 09/10/2025
