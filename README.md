# accessibility-ms-analysis

- API RESTful para gestión de análisis, resultados y errores.
- Endpoints para crear, consultar, actualizar y eliminar análisis, resultados y errores.
- Respuestas internacionalizadas (i18n) y manejo global de errores.
- Validación robusta con FluentValidation.
- Documentación OpenAPI/Swagger integrada y moderna.
- Pruebas de integración automatizadas con xUnit.
- Listo para despliegue en Docker y Docker Compose.

## Valores de enumeración

### Nivel de Resultado (Result Level)

- `violation` - Violación de accesibilidad
- `pass` - Prueba pasada exitosamente
- `inapplicable` - Regla no aplicable
- `incomplete` - Prueba incompleta

### Severidad (Severity)

- `critical` - Crítico (bloquea completamente el acceso)
- `serious` - Serio (dificulta significativamente el acceso)
- `moderate` - Moderado (afecta parcialmente el acceso)
- `minor` - Menor (impacto mínimo en accesibilidad)

### Estado de Análisis (Analysis Status)

- `pending` - Pendiente de procesamiento
- `in_progress` - En progreso
- `completed` - Completado exitosamente
- `failed` - Falló durante el procesamiento

### Herramienta Utilizada (Tool Used)

- `axe-core` - Biblioteca axe-core de accesibilidad
- `EqualAccess` - IBM Equal Access Accessibility Checker

### Nivel WCAG

- `A` - Nivel A (básico)
- `AA` - Nivel AA (estándar)
- `AAA` - Nivel AAA (avanzado)

### Tipo de Contenido

- `html` - Código HTML
- `url` - URL para analizar

```
.
├── docker-compose.yml
├── Dockerfile
├── .env.development
├── .env.production
├── README.md
├── Analysis.sln
├── src/
│   ├── Analysis.Api/           # API principal (Minimal API, Swagger, FluentValidation)
│   ├── Analysis.Application/   # DTOs, validadores y lógica de aplicación
│   ├── Analysis.Domain/        # Entidades y enums de dominio
│   ├── Analysis.Infrastructure/# DbContext, servicios de infraestructura y acceso a datos
│   └── Analysis.Tests/         # Pruebas de integración y unitarias (xUnit)
```

## Variables de entorno

Configura los archivos `.env.development` y `.env.production` para tus entornos. Ejemplo:

```env
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:8082
DB_NAME=analysisdb
DB_USER=root
DB_PASSWORD=yourpassword
API_HOST_PORT=8082
```

> **Nota:** No es necesario definir `DB_HOST` ni `DB_PORT` en los archivos `.env`, ya que la comunicación interna entre contenedores Docker utiliza el nombre del servicio (`analysis-db`) y el puerto por defecto (`3306`). La cadena de conexión ya está configurada correctamente en `docker-compose.yml`.

## Uso con Docker Compose

```bash
# Desarrollo
docker compose --env-file .env.development up --build

# Producción
docker compose --env-file .env.production up --build
```

## Compilación y pruebas locales

```bash
# Restaurar dependencias y compilar
dotnet restore Analysis.sln
dotnet build Analysis.sln

# Ejecutar pruebas
dotnet test src/Analysis.Tests/Analysis.Tests.csproj
```

## Variables de entorno

Configura los archivos `.env.development` y `.env.production` para tus entornos:

```env
# .env.development
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:8082
DB_NAME=analysisdb
DB_USER=msuser
DB_PASSWORD=AnlApp2025SecurePass
DB_ROOT_PASSWORD=bG7PL2XvVNIIYzY2ZxXknLpT5cbCBVhM
API_HOST_PORT=8082
DB_PORT=3308
```

```env
# .env.production - Cambiar passwords antes de usar en producción
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8082
DB_NAME=analysisdb_prod
DB_USER=msuser_prod
DB_PASSWORD=AnlApp2025SecurePassPROD
DB_ROOT_PASSWORD=bG7PL2XvVNIIYzY2ZxXknLpT5cbCBVhMPROD
API_HOST_PORT=8082
DB_PORT=3308
MYSQL_CHARSET=utf8mb4
MYSQL_COLLATION=utf8mb4_unicode_ci
ENABLE_SSL=true
```

> **⚠️ Nota de Seguridad:** Los passwords mostrados son ejemplos para desarrollo. **CAMBIAR OBLIGATORIAMENTE** antes de usar en producción real.
>
> **📋 Variables Requeridas:**
>
> - `DB_ROOT_PASSWORD`: Password root de MySQL (32 caracteres seguros)
> - `DB_PASSWORD`: Password del usuario de aplicación
> - `DB_PORT`: Puerto externo para conectividad (3308 para Analysis)

## Dockerización y despliegue

Este proyecto está preparado para ejecutarse fácilmente en contenedores Docker, tanto en desarrollo como en producción.

- **Dockerfile**: Define cómo construir la imagen de la API (compilación, dependencias, puertos expuestos).
- **docker-compose.yml**: Orquesta los servicios (API y MySQL), define variables de entorno, mapea puertos y gestiona dependencias.
- **.env.development / .env.production**: Archivos de variables de entorno para cada ambiente. Se referencian automáticamente en docker-compose.

### Flujo recomendado

1. Ajusta las variables en `.env.development` o `.env.production` según el entorno.
2. Ejecuta:
   ```sh
   docker compose --env-file .env.development up --build
   # o para producción
   docker compose --env-file .env.production up --build
   ```
3. Accede a la API en el puerto definido por `API_HOST_PORT` (por defecto 8082).

## 🧪 Pruebas y Base de Datos de Test

### Pruebas de Integración

```bash
# Ejecutar todas las pruebas
dotnet test Analysis.sln

# Ejecutar pruebas con detalles
dotnet test Analysis.sln --verbosity normal
```

### Inicialización de Base de Datos de Test

```powershell
# Windows PowerShell
.\init-test-databases.ps1

# Linux/macOS
./init-test-databases.sh
```

**Configuración de Test:**

- **Root Password**: `dI5QN4ZxWPKKZbZ4ZzZmpNrV7edEDXjO`
- **Test User**: `testuser` / `TestApp2025SecurePass`
- **Bases de datos**: `usersdb_test`, `analysisdb_test`, `reportsdb_test`

### Personalización del nombre de la imagen

Puedes personalizar el nombre de la imagen agregando la propiedad `image:` en el servicio `api` de tu `docker-compose.yml`:

```yaml
services:
	api:
		image: magodeveloper/accessibility-ms-analysis:latest
		# ...
```

## Pruebas

Las pruebas unitarias y de integración están ubicadas en `src/Analysis.Tests/` y pueden ejecutarse localmente con `dotnet test`. También se ejecutan automáticamente en el pipeline de CI/CD.

## 🗄️ Base de datos y migraciones

### Estructura de base de datos

El microservicio utiliza **MySQL** con Entity Framework Core y maneja las siguientes tablas:

- **ANALYSIS** - Información principal de análisis de accesibilidad
- **RESULTS** - Resultados específicos por criterio WCAG
- **ERRORS** - Errores detallados encontrados en el análisis

### Restricciones y índices implementados

✅ **Índices de rendimiento:**

```sql
-- Tabla ANALYSIS
CREATE INDEX idx_analysis_user ON ANALYSIS(UserId);
CREATE INDEX idx_analysis_status ON ANALYSIS(status);
CREATE INDEX idx_analysis_date ON ANALYSIS(date_analysis);

-- Tabla RESULTS
CREATE INDEX idx_results_analysis ON RESULTS(analysis_id);
CREATE INDEX idx_results_severity ON RESULTS(severity);

-- Tabla ERRORS
CREATE INDEX idx_errors_result ON ERRORS(result_id);
```

✅ **Restricciones de integridad referencial:**

```sql
-- Relación interna: Analysis → Results → Errors
ALTER TABLE RESULTS ADD CONSTRAINT fk_results_analysis
FOREIGN KEY (analysis_id) REFERENCES ANALYSIS(id) ON DELETE CASCADE;

ALTER TABLE ERRORS ADD CONSTRAINT fk_errors_result
FOREIGN KEY (result_id) REFERENCES RESULTS(id) ON DELETE CASCADE;

-- Relación cross-microservice: Analysis → Users
ALTER TABLE ANALYSIS ADD CONSTRAINT fk_analysis_user
FOREIGN KEY (UserId) REFERENCES usersdb.USERS(id) ON DELETE CASCADE;
```

### 🔄 Aplicar migraciones

```bash
# Aplicar todas las migraciones pendientes
dotnet ef database update --project src/Analysis.Infrastructure --startup-project src/Analysis.Api

# Generar script SQL para revisión
dotnet ef migrations script --project src/Analysis.Infrastructure --startup-project src/Analysis.Api
```

> ⚠️ **Importante:** La constraint `fk_analysis_user` requiere que el microservicio **accessibility-ms-users** esté funcionando y su base de datos `usersdb` esté creada antes de aplicar las migraciones.

📖 **Documentación detallada:** Ver [CROSS-DATABASE-SETUP.md](CROSS-DATABASE-SETUP.md)

## Endpoints principales

El microservicio usa controladores tradicionales (MVC) para exponer los endpoints REST. A continuación, todos los endpoints disponibles:

### Análisis

- `GET    /api/analysis`  
   Obtiene todos los análisis.

- `GET    /api/analysis/{id}`  
   Obtiene un análisis por ID.

- `GET    /api/analysis/by-user?userId={userId}`  
   Obtiene todos los análisis de un usuario específico.

- `GET    /api/analysis/by-date?userId={userId}&date={date}`  
   Obtiene análisis por fecha y usuario.

- `GET    /api/analysis/by-tool?userId={userId}&toolUsed={tool}`  
   Obtiene análisis por herramienta y usuario.

- `GET    /api/analysis/by-status?userId={userId}&status={status}`  
   Obtiene análisis por estado y usuario.

  **Respuesta 200 ejemplo:**

  ```json
  [
    {
      "id": 1,
      "userId": 42,
      "dateAnalysis": "2025-08-16T00:00:00Z",
      "contentType": "html",
      "contentInput": "<html>...</html>",
      "sourceUrl": "https://example.com",
      "toolUsed": "axe-core",
      "status": "completed",
      "summaryResult": "10 errores encontrados",
      "resultJson": "{}",
      "durationMs": 1500,
      "wcagVersion": "2.1",
      "wcagLevel": "AA",
      "axeViolations": 10,
      "axePasses": 5,
      "createdAt": "2025-08-16T10:00:00Z",
      "updatedAt": "2025-08-16T10:01:30Z"
    }
  ]
  ```

- `POST   /api/analysis`  
   Crea un nuevo análisis.

  **Payload ejemplo:**

  ```json
  {
    "userId": 42,
    "dateAnalysis": "2025-08-16T00:00:00Z",
    "contentType": "html",
    "contentInput": "<html>...</html>",
    "sourceUrl": "https://example.com",
    "toolUsed": "axe-core",
    "status": "pending",
    "summaryResult": "Análisis en progreso",
    "resultJson": "{}",
    "durationMs": null,
    "wcagVersion": "2.1",
    "wcagLevel": "AA",
    "axeViolations": 0
  }
  ```

- `DELETE /api/analysis/{id}`  
   Elimina un análisis por ID. Respuesta: 204 No Content.

- `DELETE /api/analysis/all`  
   Elimina todos los análisis. Respuesta: 204 No Content.

### Resultados

- `GET    /api/result`  
   Obtiene todos los resultados.

- `GET    /api/result/{id}`  
   Obtiene un resultado por ID.

- `GET    /api/result/by-analysis?analysisId={analysisId}`  
   Obtiene todos los resultados de un análisis específico.

- `GET    /api/result/by-level?level={level}`  
   Obtiene resultados por nivel (violation, pass, inapplicable, incomplete).

- `GET    /api/result/by-severity?severity={severity}`  
   Obtiene resultados por severidad (critical, serious, moderate, minor).

  **Respuesta 200 ejemplo:**

  ```json
  [
    {
      "id": 10,
      "analysisId": 1,
      "wcagCriterionId": "1.1.1",
      "wcagCriterion": "Non-text Content",
      "level": "violation",
      "severity": "serious",
      "description": "Images must have alternate text",
      "createdAt": "2025-08-16T10:01:00Z",
      "updatedAt": "2025-08-16T10:01:00Z"
    }
  ]
  ```

- `POST   /api/result`  
   Crea un nuevo resultado.

  **Payload ejemplo:**

  ```json
  {
    "analysisId": 1,
    "wcagCriterionId": "1.1.1",
    "wcagCriterion": "Non-text Content",
    "level": "violation",
    "severity": "serious",
    "description": "Images must have alternate text"
  }
  ```

- `DELETE /api/result/{id}`  
   Elimina un resultado por ID. Respuesta: 204 No Content.

- `DELETE /api/result/all`  
   Elimina todos los resultados. Respuesta: 204 No Content.

### Errores

- `GET    /api/error`  
   Obtiene todos los errores.

- `GET    /api/error/{id}`  
   Obtiene un error por ID.

- `GET    /api/error/by-result?resultId={resultId}`  
   Obtiene todos los errores de un resultado específico.

  **Respuesta 200 ejemplo:**

  ```json
  [
    {
      "id": 100,
      "resultId": 10,
      "wcagCriterionId": "1.1.1",
      "errorCode": "image-alt",
      "description": "Falta texto alternativo en imagen",
      "location": "img#logo line 45",
      "message": "",
      "code": "",
      "createdAt": "2025-08-16T10:01:00Z",
      "updatedAt": "2025-08-16T10:01:00Z"
    }
  ]
  ```

- `POST   /api/error`  
   Crea un nuevo error.

  **Payload ejemplo:**

  ```json
  {
    "resultId": 10,
    "wcagCriterionId": "1.1.1",
    "errorCode": "image-alt",
    "description": "Falta texto alternativo en imagen",
    "location": "img#logo line 45"
  }
  ```

- `DELETE /api/error/{id}`  
   Elimina un error por ID. Respuesta: 204 No Content.

- `DELETE /api/error/all`  
   Elimina todos los errores. Respuesta: 204 No Content.

## Valores de enumeración

### Nivel de Resultado (Result Level)

- `violation` - Violación de accesibilidad
- `pass` - Prueba pasada exitosamente
- `inapplicable` - Regla no aplicable
- `incomplete` - Prueba incompleta

### Severidad (Severity)

- `critical` - Crítico (bloquea completamente el acceso)
- `serious` - Serio (dificulta significativamente el acceso)
- `moderate` - Moderado (afecta parcialmente el acceso)
- `minor` - Menor (impacto mínimo en accesibilidad)

### Estado de Análisis (Analysis Status)

- `pending` - Pendiente de procesamiento
- `in_progress` - En progreso
- `completed` - Completado exitosamente
- `failed` - Falló durante el procesamiento

### Herramienta Utilizada (Tool Used)

- `axe-core` - Biblioteca axe-core de accesibilidad
- `EqualAccess` - IBM Equal Access Accessibility Checker

### Nivel WCAG

- `A` - Nivel A (básico)
- `AA` - Nivel AA (estándar)
- `AAA` - Nivel AAA (avanzado)

### Tipo de Contenido

- `html` - Código HTML
- `url` - URL para analizar

## Ejemplos de uso

### Ejemplo 1: Flujo completo de análisis

```bash
# 1. Crear un análisis
curl -X POST http://localhost:5041/api/analysis \
  -H "Content-Type: application/json" \
  -d '{
    "userId": 123,
    "dateAnalysis": "2025-01-16T10:00:00Z",
    "contentType": "html",
    "contentInput": "<html><body><img src=\"logo.png\"></body></html>",
    "sourceUrl": "https://example.com",
    "toolUsed": "axe-core",
    "status": "pending",
    "wcagVersion": "2.1",
    "wcagLevel": "AA"
  }'

# 2. Obtener análisis de un usuario
curl -X GET "http://localhost:5041/api/analysis/by-user?userId=123"

# 3. Buscar resultados por severidad
curl -X GET "http://localhost:5041/api/result/by-severity?severity=critical"

# 4. Obtener errores de un resultado específico
curl -X GET "http://localhost:5041/api/error/by-result?resultId=10"
```

### Ejemplo 2: Operaciones de limpieza

```bash
# Eliminar todos los análisis
curl -X DELETE http://localhost:5041/api/analysis/all

# Eliminar todos los resultados
curl -X DELETE http://localhost:5041/api/result/all

# Eliminar todos los errores
curl -X DELETE http://localhost:5041/api/error/all
```

### Ejemplo 3: Búsquedas avanzadas

```bash
# Análisis por herramienta específica
curl -X GET "http://localhost:5041/api/analysis/by-tool?userId=123&toolUsed=axe-core"

# Análisis por estado
curl -X GET "http://localhost:5041/api/analysis/by-status?userId=123&status=completed"

# Resultados por nivel de violación
curl -X GET "http://localhost:5041/api/result/by-level?level=violation"
```

## Documentación OpenAPI/Swagger

La documentación interactiva está disponible en `/swagger` cuando la API se ejecuta en modo desarrollo. Incluye ejemplos, descripciones y validaciones automáticas de los endpoints.

---

---

Desarrollado por magodeveloper | 2025
