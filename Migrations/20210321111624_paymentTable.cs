using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ZawagProject.Migrations
{
    public partial class paymentTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentDate = table.Column<DateTime>(nullable: false),
                    Amount = table.Column<double>(nullable: false),
                    UserId = table.Column<int>(nullable: false),
                    ReceiptUrl = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Currency = table.Column<string>(nullable: true),
                    IsPaid = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Payments");
        }
    }
}
