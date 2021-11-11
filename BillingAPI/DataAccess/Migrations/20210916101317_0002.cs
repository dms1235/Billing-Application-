using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DataAccess.Migrations
{
    public partial class _0002 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Item_Masters",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Item_Number = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Item_Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Item_Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UOM = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HSN_Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GST_Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Item_Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Is_Active = table.Column<bool>(type: "bit", nullable: false),
                    Created_By = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Updated_By = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Created_On = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Updated_On = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Item_Masters", x => x.ID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Item_Masters");
        }
    }
}
