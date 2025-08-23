using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Analysis.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserForeignKeyConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add cross-database foreign key constraint to reference usersdb.USERS table
            migrationBuilder.Sql(@"
                ALTER TABLE ANALYSIS 
                ADD CONSTRAINT fk_analysis_user 
                FOREIGN KEY (user_id) REFERENCES usersdb.USERS(id) 
                ON DELETE CASCADE;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the cross-database foreign key constraint
            migrationBuilder.Sql(@"
                ALTER TABLE ANALYSIS 
                DROP FOREIGN KEY fk_analysis_user;
            ");
        }
    }
}
