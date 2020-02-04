using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PaymentGateway.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Merchants",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: false),
                    Url = table.Column<string>(nullable: true),
                    Login = table.Column<string>(nullable: false),
                    HashedPassword = table.Column<string>(nullable: false),
                    Salt = table.Column<string>(nullable: false),
                    Active = table.Column<bool>(nullable: false),
                    AcquirerType = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Merchants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Amount = table.Column<double>(nullable: false),
                    Currency = table.Column<string>(maxLength: 3, nullable: false),
                    ExpiryMonth = table.Column<byte>(nullable: false),
                    ExpiryYear = table.Column<ushort>(nullable: false),
                    CardLastDigits = table.Column<string>(nullable: false),
                    CardNumberHashed = table.Column<string>(nullable: false),
                    CardNumberLength = table.Column<byte>(nullable: false),
                    Status = table.Column<int>(nullable: false),
                    CreatedUtc = table.Column<DateTime>(nullable: false),
                    MerchantId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Merchants");

            migrationBuilder.DropTable(
                name: "Payments");
        }
    }
}
