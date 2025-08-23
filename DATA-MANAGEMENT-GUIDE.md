# 🔧 Guía de Manejo de Datos en Infraestructura de Test

## 📊 Resumen de Opciones

| Comando                                             | Datos Existentes | Estructura           | Migraciones      | Uso Recomendado          |
| --------------------------------------------------- | ---------------- | -------------------- | ---------------- | ------------------------ |
| `dotnet run --project scripts`                      | ✅ **PRESERVA**  | ✅ Mantiene          | ✅ Aplica nuevas | 🟢 **Desarrollo normal** |
| `dotnet run --project scripts -- --force`           | ❌ **ELIMINA**   | 🔄 Recrea            | ✅ Aplica todas  | 🔴 **Reset completo**    |
| `dotnet run --project scripts -- --skip-migrations` | ✅ **PRESERVA**  | ✅ Crea si no existe | ❌ No ejecuta    | 🟡 **Solo estructura**   |
| `dotnet run --project scripts -- --clean-data`      | 🧹 **LIMPIA**    | ✅ Mantiene          | ❌ No toca       | 🔵 **Limpiar datos**     |

## 🛡️ Niveles de Seguridad

### 🟢 SEGURO (Sin pérdida de datos)

```bash
# Modo normal - Preserva datos y aplica migraciones pendientes
dotnet run --project scripts

# Solo limpiar datos (mantiene estructura y migraciones)
dotnet run --project scripts -- --clean-data

# Solo crear estructura (si no existe)
dotnet run --project scripts -- --skip-migrations
```

### 🔴 DESTRUCTIVO (Elimina datos)

```bash
# ELIMINA TODOS LOS DATOS - Requiere confirmación "SI"
dotnet run --project scripts -- --force

# ELIMINA TODOS LOS DATOS - Sin migraciones
dotnet run --project scripts -- --force --skip-migrations
```

## 🚨 Confirmaciones de Seguridad

### Para `--force`:

```
⚠️  ADVERTENCIA: El parámetro --force ELIMINARÁ TODOS LOS DATOS existentes
   Esto recreará completamente las siguientes bases de datos:
   • usersdb_test
   • analysisdb_test
   • reportsdb_test

¿Continuar? (escriba 'SI' para confirmar):
```

### Para `--clean-data`:

```
🧹 Limpiando datos de test (manteniendo estructura)...
   🧹 Limpiando usersdb_test...
   🧹 Limpiando analysisdb_test...
   🧹 Limpiando reportsdb_test...
✅ Datos de test limpiados correctamente
```

## 🔄 Flujos de Trabajo Comunes

### 👨‍💻 Desarrollo Diario

```bash
# 1. Trabajar normalmente (preserva datos)
dotnet run --project scripts

# 2. Ejecutar tests
dotnet test Analysis.sln --verbosity normal
```

### 🧪 Preparar Tests Limpios

```bash
# Opción 1: Limpiar datos (rápido)
dotnet run --project scripts -- --clean-data

# Opción 2: Reset completo (más lento pero seguro)
dotnet run --project scripts -- --force
```

### 🛠️ Troubleshooting

```bash
# Problema con migraciones - Reset completo
dotnet run --project scripts -- --force

# Problema con datos corrompidos - Solo limpiar
dotnet run --project scripts -- --clean-data

# Solo quiero la estructura - Sin migraciones
dotnet run --project scripts -- --skip-migrations
```

## 💡 Mejores Prácticas

1. **🟢 Desarrollo normal**: Usa sin parámetros para mantener tus datos de test
2. **🧹 Tests limpios**: Usa `--clean-data` para limpiar sin perder tiempo
3. **🔄 Reset completo**: Usa `--force` solo cuando sea necesario
4. **⚠️ Siempre confirma**: Lee las advertencias antes de proceder
5. **📋 Documenta cambios**: Anota cuándo usas `--force` en tu equipo

## 🔍 Verificación Post-Ejecución

Después de cualquier operación, puedes verificar:

```sql
-- Verificar bases de datos creadas
SHOW DATABASES LIKE '%_test';

-- Verificar tablas en cada base
USE analysisdb_test;
SHOW TABLES;

-- Verificar datos (debe estar vacío después de --force o --clean-data)
SELECT COUNT(*) FROM analysis;
```
