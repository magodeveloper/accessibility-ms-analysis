using MySqlConnector;

namespace Analysis.Tests.Infrastructure
{
    public static class TestDataSeeder
    {
        public static async Task SeedTestDatabasesAsync()
        {
            await SeedUsersDatabase();
            await SeedAnalysisDatabase();
        }

        private static async Task SeedUsersDatabase()
        {
            var connectionString = "Server=localhost;Port=3308;Database=usersdb_test;User=msuser;Password=Y0urs3cretOrA7&;CharSet=utf8mb4;";

            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            // Crear la base de datos si no existe
            var createDbCommand = new MySqlCommand("CREATE DATABASE IF NOT EXISTS usersdb_test CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;", connection);
            await createDbCommand.ExecuteNonQueryAsync();

            // Usar la base de datos
            var useDbCommand = new MySqlCommand("USE usersdb_test;", connection);
            await useDbCommand.ExecuteNonQueryAsync();

            // Crear tabla USERS si no existe
            var createTableCommand = new MySqlCommand(@"
                CREATE TABLE IF NOT EXISTS USERS (
                    id INT AUTO_INCREMENT PRIMARY KEY,
                    nickname VARCHAR(15) NOT NULL UNIQUE,
                    name VARCHAR(30) NOT NULL,
                    lastname VARCHAR(30) NOT NULL,
                    email VARCHAR(60) NOT NULL UNIQUE,
                    password VARCHAR(60) NOT NULL,
                    role VARCHAR(5) NOT NULL,
                    status VARCHAR(8) NOT NULL,
                    email_confirmed TINYINT(1) DEFAULT 0,
                    last_login DATETIME(6) NULL,
                    registration_date DATETIME(6) NOT NULL,
                    created_at DATETIME(6) NOT NULL,
                    updated_at DATETIME(6) NOT NULL
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
            ", connection);
            await createTableCommand.ExecuteNonQueryAsync();

            // Limpiar datos existentes
            var clearCommand = new MySqlCommand("TRUNCATE TABLE USERS;", connection);
            await clearCommand.ExecuteNonQueryAsync();

            // Insertar usuarios de prueba
            var insertCommand = new MySqlCommand(@"
                INSERT INTO USERS (id, nickname, name, lastname, email, password, role, status, email_confirmed, registration_date, created_at, updated_at) 
                VALUES 
                (1, 'testuser1', 'Test', 'User1', 'test1@example.com', '$2a$11$hashed_password_here', 'admin', 'active', 1, NOW(), NOW(), NOW()),
                (2, 'testuser2', 'Test', 'User2', 'test2@example.com', '$2a$11$hashed_password_here', 'user', 'active', 1, NOW(), NOW(), NOW()),
                (3, 'testuser3', 'Test', 'User3', 'test3@example.com', '$2a$11$hashed_password_here', 'user', 'active', 1, NOW(), NOW(), NOW());
            ", connection);
            await insertCommand.ExecuteNonQueryAsync();
        }

        private static async Task SeedAnalysisDatabase()
        {
            var connectionString = "Server=localhost;Port=3308;Database=analysisdb_test;User=msuser;Password=Y0urs3cretOrA7&;CharSet=utf8mb4;";

            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            // Crear la base de datos si no existe
            var createDbCommand = new MySqlCommand("CREATE DATABASE IF NOT EXISTS analysisdb_test CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;", connection);
            await createDbCommand.ExecuteNonQueryAsync();

            // Usar la base de datos
            var useDbCommand = new MySqlCommand("USE analysisdb_test;", connection);
            await useDbCommand.ExecuteNonQueryAsync();

            // Crear la FK constraint hacia usersdb_test
            var addFkCommand = new MySqlCommand(@"
                CREATE TABLE IF NOT EXISTS temp_fk_check AS SELECT 1;
                DROP TABLE temp_fk_check;
                
                -- Intentar crear la FK constraint si no existe
                SET @constraint_exists = (SELECT COUNT(*) FROM information_schema.KEY_COLUMN_USAGE 
                    WHERE TABLE_SCHEMA = 'analysisdb_test' 
                    AND TABLE_NAME = 'ANALYSIS' 
                    AND CONSTRAINT_NAME = 'fk_analysis_user_test');
                
                SET @sql = IF(@constraint_exists = 0, 
                    'ALTER TABLE ANALYSIS ADD CONSTRAINT fk_analysis_user_test FOREIGN KEY (user_id) REFERENCES usersdb_test.USERS(id) ON DELETE CASCADE',
                    'SELECT ''FK constraint already exists'' as message');
                
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ", connection);

            try
            {
                await addFkCommand.ExecuteNonQueryAsync();
            }
            catch (MySqlException ex) when (ex.Message.Contains("already exists"))
            {
                // FK constraint ya existe, continuar
                Console.WriteLine("FK constraint already exists, continuing...");
            }
        }

        public static async Task CleanupTestDatabasesAsync()
        {
            await CleanupDatabase("analysisdb_test");
            await CleanupDatabase("usersdb_test");
        }

        private static async Task CleanupDatabase(string databaseName)
        {
            var connectionString = $"Server=localhost;Port=3308;User=msuser;Password=Y0urs3cretOrA7&;";

            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            try
            {
                var dropCommand = new MySqlCommand($"DROP DATABASE IF EXISTS {databaseName};", connection);
                await dropCommand.ExecuteNonQueryAsync();
            }
            catch (MySqlException)
            {
                // Ignorar errores si la base de datos no existe
            }
        }
    }
}
