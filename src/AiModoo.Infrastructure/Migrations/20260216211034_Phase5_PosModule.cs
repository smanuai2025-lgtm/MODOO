using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiModoo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase5_PosModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PosConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: true),
                    StockLocationId = table.Column<int>(type: "int", nullable: true),
                    JournalId = table.Column<int>(type: "int", nullable: true),
                    PricelistId = table.Column<int>(type: "int", nullable: true),
                    IsTaxIncluded = table.Column<bool>(type: "bit", nullable: false),
                    DefaultTaxId = table.Column<int>(type: "int", nullable: true),
                    OpeningBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PosConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PosConfigs_Journals_JournalId",
                        column: x => x.JournalId,
                        principalTable: "Journals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PosConfigs_Locations_StockLocationId",
                        column: x => x.StockLocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PosConfigs_Pricelists_PricelistId",
                        column: x => x.PricelistId,
                        principalTable: "Pricelists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PosConfigs_Taxes_DefaultTaxId",
                        column: x => x.DefaultTaxId,
                        principalTable: "Taxes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PosConfigs_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PosSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ConfigId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    OpeningDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClosingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OpeningBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ClosingBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalSales = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalReturns = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalTax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDiscount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OrderCount = table.Column<int>(type: "int", nullable: false),
                    JournalEntryId = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PosSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PosSessions_JournalEntries_JournalEntryId",
                        column: x => x.JournalEntryId,
                        principalTable: "JournalEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PosSessions_PosConfigs_ConfigId",
                        column: x => x.ConfigId,
                        principalTable: "PosConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PosOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SessionId = table.Column<int>(type: "int", nullable: false),
                    PartnerId = table.Column<int>(type: "int", nullable: true),
                    PartnerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AmountPaid = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Change = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsReturn = table.Column<bool>(type: "bit", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PosOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PosOrders_Partners_PartnerId",
                        column: x => x.PartnerId,
                        principalTable: "Partners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PosOrders_PosSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "PosSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PosOrderLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PosOrderLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PosOrderLines_PosOrders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "PosOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PosOrderLines_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PosPayments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    PaymentMethodId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PosPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PosPayments_PaymentMethods_PaymentMethodId",
                        column: x => x.PaymentMethodId,
                        principalTable: "PaymentMethods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PosPayments_PosOrders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "PosOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PosConfigs_DefaultTaxId",
                table: "PosConfigs",
                column: "DefaultTaxId");

            migrationBuilder.CreateIndex(
                name: "IX_PosConfigs_JournalId",
                table: "PosConfigs",
                column: "JournalId");

            migrationBuilder.CreateIndex(
                name: "IX_PosConfigs_PricelistId",
                table: "PosConfigs",
                column: "PricelistId");

            migrationBuilder.CreateIndex(
                name: "IX_PosConfigs_StockLocationId",
                table: "PosConfigs",
                column: "StockLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_PosConfigs_WarehouseId",
                table: "PosConfigs",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_PosOrderLines_OrderId",
                table: "PosOrderLines",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PosOrderLines_ProductId",
                table: "PosOrderLines",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PosOrders_Number",
                table: "PosOrders",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PosOrders_PartnerId",
                table: "PosOrders",
                column: "PartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_PosOrders_SessionId",
                table: "PosOrders",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_PosPayments_OrderId",
                table: "PosPayments",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PosPayments_PaymentMethodId",
                table: "PosPayments",
                column: "PaymentMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_PosSessions_ConfigId",
                table: "PosSessions",
                column: "ConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_PosSessions_JournalEntryId",
                table: "PosSessions",
                column: "JournalEntryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PosOrderLines");

            migrationBuilder.DropTable(
                name: "PosPayments");

            migrationBuilder.DropTable(
                name: "PosOrders");

            migrationBuilder.DropTable(
                name: "PosSessions");

            migrationBuilder.DropTable(
                name: "PosConfigs");
        }
    }
}
