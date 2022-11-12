using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.migrations
{
    public partial class AddOwner : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "owner_id",
                table: "todos",
                type: "nvarchar(128)",
                nullable: true,
                defaultValue: "anonymous");

             // Custom Code
            
            migrationBuilder.Sql("update dbo.todos set owner_id = 'anonymous' where owner_id is null");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "owner_id",
                table: "todos");
        }
    }
}
