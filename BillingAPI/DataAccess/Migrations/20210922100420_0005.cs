using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DataAccess.Migrations
{
    public partial class _0005 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Invoice_Masters",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Invoice_Number = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Invoice_Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PONumber = table.Column<long>(type: "bigint", nullable: true),
                    PODate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VendorCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModeOfTransport = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeliveryNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Transpoter = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LRNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SupplyDateandTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PlaceOfSupply = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BillToCompanyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BillToAddress1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BillToAddress2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BillToAddress3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BillToAddress4 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BillToGSTNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConsigneCompanyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConsigneAddress1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConsigneAddress2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConsigneAddress3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConsigneAddress4 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Created_By = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Updated_By = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Created_On = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Updated_On = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoice_Masters", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Invoice_Details",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ItemCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ItemID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemQty = table.Column<int>(type: "int", nullable: false),
                    ItemRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ItemAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscAmt = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ItemWiseAmout = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InvoiceMasterID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GrossAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Created_By = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Updated_By = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Created_On = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Updated_On = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoice_Details", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Invoice_Details_Invoice_Masters_InvoiceMasterID",
                        column: x => x.InvoiceMasterID,
                        principalTable: "Invoice_Masters",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Invoice_Details_InvoiceMasterID",
                table: "Invoice_Details",
                column: "InvoiceMasterID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Invoice_Details");

            migrationBuilder.DropTable(
                name: "Invoice_Masters");
        }
    }
}
