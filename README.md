# accessibility-ms-analysis



- API RESTful para gestión de análisis, resultados y errores.
- Endpoints para crear, consultar, actualizar y eliminar análisis, resultados y errores.
- Respuestas internacionalizadas (i18n) y manejo global de errores.
- Validación robusta con FluentValidation.
- Documentación OpenAPI/Swagger integrada y moderna.
- Pruebas de integración automatizadas con xUnit.
- Listo para despliegue en Docker y Docker Compose.

## Estructura del proyecto

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

## Endpoints principales

El microservicio usa controladores tradicionales (MVC) para exponer los endpoints REST. A continuación, algunos ejemplos clave:

### Análisis

- `GET    /api/analysis/{id}`  
   Obtiene un análisis por ID.

  **Respuesta 200:**

  ```json
  {
    "id": 1,
    "userId": 42,
    "date": "2025-08-16T00:00:00Z",
    "toolUsed": "axe-core",
    "status": "completed"
  }
  ```

- `POST   /api/analysis`  
   Crea un nuevo análisis.

  **Payload ejemplo:**

  ```json
  {
    "userId": 42,
    "date": "2025-08-16T00:00:00Z",
    "toolUsed": "axe-core",
    "status": "pending"
  }
  ```

  **Respuesta 201:**

  ```json
  {
    "message": "Análisis creado correctamente.",
    "data": {
      "id": 2,
      "userId": 42,
      "date": "2025-08-16T00:00:00Z",
      "toolUsed": "axe-core",
      "status": "pending"
    }
  }
  ```

- `DELETE /api/analysis/{id}`  
   Elimina un análisis por ID. Respuesta: 204 No Content.

### Resultados

- `GET    /api/result/{id}`  
   Obtiene un resultado por ID.

- `POST   /api/result`  
   Crea un nuevo resultado.

  **Payload ejemplo:**

  ```json
  {
    "analysisId": 1,
    "summary": "10 errores, 5 advertencias",
    "details": "..."
  }
  ```

  **Respuesta 201:**

  ```json
  {
    "message": "Resultado creado correctamente.",
    "data": {
      "id": 10,
      "analysisId": 1,
      "summary": "10 errores, 5 advertencias",
      "details": "..."
    }
  }
  ```

- `DELETE /api/result/{id}`  
   Elimina un resultado por ID. Respuesta: 204 No Content.

### Errores

- `GET    /api/error/{id}`  
   Obtiene un error por ID.

- `POST   /api/error`  
   Crea un nuevo error.

  **Payload ejemplo:**

  ```json
  {
    "resultId": 10,
    "code": "WCAG-1.1.1",
    "message": "Falta texto alternativo en imagen",
    "element": "img#logo"
  }
  ```

  **Respuesta 201:**

  ```json
  {
    "message": "Error creado correctamente.",
    "data": {
      "id": 100,
      "resultId": 10,
      "code": "WCAG-1.1.1",
      "message": "Falta texto alternativo en imagen",
      "element": "img#logo"
    }
  }
  ```

- `DELETE /api/error/{id}`  
   Elimina un error por ID. Respuesta: 204 No Content.


## Documentación OpenAPI/Swagger

La documentación interactiva está disponible en `/swagger` cuando la API se ejecuta en modo desarrollo. Incluye ejemplos, descripciones y validaciones automáticas de los endpoints.

---


---

Desarrollado por magodeveloper | 2025
