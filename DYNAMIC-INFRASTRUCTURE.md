# 🚀 Infraestructura Dinámica de Microservicios

## 📋 Descripción

Este documento describe cómo usar la nueva infraestructura dinámica para manejo de bases de datos en los microservicios de accesibilidad.

## 🛠️ Nuevas Funcionalidades

### 1. **Scripts de Inicialización Inteligentes**

#### **Opciones Disponibles:**

```bash
# Verificar estado sin recrear
./init-test-databases.ps1

# Forzar recreación completa
./init-test-databases.ps1 -Force

# Crear solo bases de datos (sin migraciones)
./init-test-databases.ps1 -SkipMigrations

# Combinaciones
./init-test-databases.ps1 -Force -SkipMigrations
```

#### **Detección Automática:**

- ✅ **Verifica** si la base de datos existe
- ✅ **Detecta** si hay tablas creadas
- ✅ **Identifica** migraciones pendientes
- ✅ **Aplica** migraciones automáticamente
- ✅ **Respeta** datos existentes por defecto

### 2. **Helper de Base de Datos**

#### **DatabaseHelper.cs:**

- `EnsureDatabaseAsync()` - Verifica y prepara la DB
- `CleanTestDataAsync()` - Limpia datos manteniendo estructura
- `GetDatabaseStatusAsync()` - Obtiene estado actual

### 3. **TestWebApplicationFactory Mejorado**

#### **Configuración Flexible:**

```csharp
// Test con base de datos real
_factory.UseRealDatabase = true;

// Test con InMemory (más rápido)
_factory.UseRealDatabase = false;
```

## ⚠️ Importante: Manejo de Datos

### 🔄 Recreación con `--force`

```bash
dotnet run --project scripts -- --force
```

**ELIMINA TODOS LOS DATOS EXISTENTES** y recrea las bases de datos desde cero:

- ❌ **Borra** completamente las bases de datos existentes
- 🆕 **Crea** nuevas bases de datos vacías
- 📄 **Aplica** todas las migraciones desde el principio
- ⚠️ **PÉRDIDA TOTAL DE DATOS**

### 🛡️ Preservación con modo normal

```bash
dotnet run --project scripts
```

**MANTIENE TODOS LOS DATOS EXISTENTES**:

- ✅ **Detecta** bases de datos existentes
- 🔒 **Preserva** todos los datos actuales
- 🔄 **Solo aplica** migraciones pendientes
- 💾 **DATOS SEGUROS**

### 📋 Solo estructura con `--skip-migrations`

```bash
dotnet run --project scripts -- --skip-migrations
```

**CREA ESTRUCTURA SIN MIGRACIONES**:

- 🆕 **Crea** bases de datos si no existen
- 🔒 **Mantiene** datos si ya existían
- ❌ **No ejecuta** migraciones
- 💾 **DATOS SEGUROS**

## 🚨 Confirmación de Seguridad

Cuando uses `--force`, el sistema te pedirá confirmación:

```
⚠️  ADVERTENCIA: El parámetro --force ELIMINARÁ TODOS LOS DATOS existentes
   Esto recreará completamente las siguientes bases de datos:
   • usersdb_test
   • analysisdb_test
   • reportsdb_test

   Solo continúa si estás seguro de que quieres PERDER TODOS LOS DATOS.
   Para mantener los datos existentes, usa el script sin --force

¿Continuar? (escriba 'SI' para confirmar):
```

Debes escribir exactamente **`SI`** para continuar.

### **Desarrollo Diario:**

```bash
# 1. Verificar estado
./init-test-databases.ps1

# 2. Si hay cambios en modelos, aplicar migraciones
dotnet ef migrations add NuevaMigracion --project src/Analysis.Infrastructure --startup-project src/Analysis.Api

# 3. Ejecutar tests
dotnet test Analysis.sln
```

### **Reset Completo:**

```bash
# 1. Recrear todo desde cero
./init-test-databases.ps1 -Force

# 2. Ejecutar tests para verificar
dotnet test Analysis.sln
```

### **Solo Estructuras (Sin Datos):**

```bash
# 1. Crear bases de datos vacías
./init-test-databases.ps1 -SkipMigrations

# 2. Aplicar migraciones manualmente
dotnet ef database update --project src/Analysis.Infrastructure --startup-project src/Analysis.Api
```

## 🎯 Casos de Uso

### **Caso 1: Primera vez**

```
Estado: No existe nada
Script ejecuta: Crear DB → Aplicar migraciones → Listo
Resultado: Infraestructura completa
```

### **Caso 2: Base existe, sin tablas**

```
Estado: DB existe, sin tablas
Script ejecuta: Aplicar migraciones → Listo
Resultado: Estructura actualizada
```

### **Caso 3: Base existe, con datos**

```
Estado: DB con datos existentes
Script muestra: ⚠️ Datos existentes
Opciones: -Force para recrear
```

### **Caso 4: Migraciones pendientes**

```
Estado: Estructura desactualizada
Script ejecuta: Aplicar migraciones → Actualizar
Resultado: Schema al día
```

## 🔧 Configuración

### **Variables de Entorno:**

Copiar `.env.infrastructure.template` a `.env.infrastructure` y personalizar:

```env
DB_HOST=localhost
DB_PORT=3306
DB_USER=root
DB_PASSWORD=TuPassword
```

### **Tests Configurables:**

```csharp
public class MiTest : IClassFixture<TestWebApplicationFactory<Program>>
{
    public MiTest(TestWebApplicationFactory<Program> factory)
    {
        // Configurar tipo de DB
        factory.UseRealDatabase = true; // o false para InMemory
    }
}
```

## 🚨 Troubleshooting

### **Error: "Tabla ya existe"**

```bash
# Solución: Forzar recreación
./init-test-databases.ps1 -Force
```

### **Error: "No se puede conectar"**

```bash
# Verificar MySQL está corriendo
# Verificar credenciales en .env.infrastructure
```

### **Error: "Migraciones pendientes"**

```bash
# Aplicar migraciones manualmente
dotnet ef database update --project src/Analysis.Infrastructure --startup-project src/Analysis.Api
```

## 📊 Monitoreo

### **Estados de Base de Datos:**

- `NotAccessible` - No se puede conectar
- `MigrationsPending` - Requiere migraciones
- `ReadyEmpty` - Lista, sin datos
- `ReadyWithData` - Lista, con datos

### **Logs del Helper:**

```
🔍 Estado de la base de datos: ReadyEmpty
🔄 Aplicando 3 migraciones pendientes...
✅ Migraciones aplicadas correctamente
🧹 Datos de test limpiados correctamente
```

## ✅ Ventajas

1. **🔄 Idempotencia** - Puede ejecutarse múltiples veces
2. **🧠 Inteligencia** - Detecta estado actual
3. **⚡ Velocidad** - Solo hace lo necesario
4. **🛡️ Seguridad** - Protege datos existentes
5. **🔧 Flexibilidad** - Múltiples opciones de ejecución
