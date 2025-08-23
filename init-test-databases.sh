#!/bin/bash

# Script para inicializar bases de datos de test
# Ejecutar antes de los tests

echo "🔧 Inicializando bases de datos de test..."

# Configuración
DB_HOST="localhost"
DB_PORT="3306"
DB_USER="root"
DB_PASSWORD="yourpassword"

# Función para ejecutar comandos MySQL
execute_sql() {
    mysql -h $DB_HOST -P $DB_PORT -u $DB_USER -p$DB_PASSWORD -e "$1" 2>/dev/null
}

# Crear base de datos de usuarios de test
echo "📊 Creando usersdb_test..."
execute_sql "DROP DATABASE IF EXISTS usersdb_test;"
execute_sql "CREATE DATABASE usersdb_test CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;"

# Crear base de datos de análisis de test
echo "📊 Creando analysisdb_test..."
execute_sql "DROP DATABASE IF EXISTS analysisdb_test;"
execute_sql "CREATE DATABASE analysisdb_test CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;"

# Crear base de datos de reportes de test (para futuros tests)
echo "📊 Creando reportsdb_test..."
execute_sql "DROP DATABASE IF EXISTS reportsdb_test;"
execute_sql "CREATE DATABASE reportsdb_test CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;"

echo "✅ Bases de datos de test inicializadas correctamente"
echo ""
echo "🚀 Ejecutar tests con:"
echo "   dotnet test Analysis.sln --verbosity normal"
