using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence<int>(
                name: "global_sequence");

            migrationBuilder.CreateTable(
                name: "todos",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false, defaultValueSql: "NEXT VALUE FOR global_sequence"),
                    todo = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    completed = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_todos", x => x.id);
                });

            // Custom Code
            
            migrationBuilder.Sql(@"
                insert into dbo.[todos] 
                    (todo, completed) 
                values 
                    ('slides', 0), 
                    ('demos', 0)
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "todos");

            migrationBuilder.DropSequence(
                name: "global_sequence");
        }
    }
}
