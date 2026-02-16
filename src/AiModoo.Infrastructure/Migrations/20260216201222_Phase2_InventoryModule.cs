using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiModoo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase2_InventoryModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ParentCategoryId = table.Column<int>(type: "int", nullable: true),
                    Level = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductCategories_ProductCategories_ParentCategoryId",
                        column: x => x.ParentCategoryId,
                        principalTable: "ProductCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UnitOfMeasureCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitOfMeasureCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UnitOfMeasures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Ratio = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    Rounding = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitOfMeasures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnitOfMeasures_UnitOfMeasureCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "UnitOfMeasureCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    InternalReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Barcode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: true),
                    UomId = table.Column<int>(type: "int", nullable: true),
                    PurchaseUomId = table.Column<int>(type: "int", nullable: true),
                    SalePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Cost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CostMethod = table.Column<int>(type: "int", nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Volume = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TrackingType = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DescriptionAr = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CanBeSold = table.Column<bool>(type: "bit", nullable: false),
                    CanBePurchased = table.Column<bool>(type: "bit", nullable: false),
                    AccountStockInputId = table.Column<int>(type: "int", nullable: true),
                    AccountStockOutputId = table.Column<int>(type: "int", nullable: true),
                    AccountExpenseId = table.Column<int>(type: "int", nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_Accounts_AccountExpenseId",
                        column: x => x.AccountExpenseId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Products_Accounts_AccountStockInputId",
                        column: x => x.AccountStockInputId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Products_Accounts_AccountStockOutputId",
                        column: x => x.AccountStockOutputId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Products_ProductCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ProductCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Products_UnitOfMeasures_PurchaseUomId",
                        column: x => x.PurchaseUomId,
                        principalTable: "UnitOfMeasures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Products_UnitOfMeasures_UomId",
                        column: x => x.UomId,
                        principalTable: "UnitOfMeasures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StockLots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_StockLots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockLots_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InventoryAdjustmentLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdjustmentId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: true),
                    LotId = table.Column<int>(type: "int", nullable: true),
                    TheoreticalQty = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    RealQty = table.Column<decimal>(type: "decimal(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryAdjustmentLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryAdjustmentLines_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryAdjustmentLines_StockLots_LotId",
                        column: x => x.LotId,
                        principalTable: "StockLots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InventoryAdjustments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: true),
                    LocationId = table.Column<int>(type: "int", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AccountingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryAdjustments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CompleteName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    LocationType = table.Column<int>(type: "int", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: true),
                    ParentLocationId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsScrapLocation = table.Column<bool>(type: "bit", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Locations_Locations_ParentLocationId",
                        column: x => x.ParentLocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StockQuants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    LotId = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ReservedQuantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    InDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockQuants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockQuants_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockQuants_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockQuants_StockLots_LotId",
                        column: x => x.LotId,
                        principalTable: "StockLots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Warehouses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    StockLocationId = table.Column<int>(type: "int", nullable: true),
                    InputLocationId = table.Column<int>(type: "int", nullable: true),
                    OutputLocationId = table.Column<int>(type: "int", nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Warehouses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Warehouses_Locations_InputLocationId",
                        column: x => x.InputLocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Warehouses_Locations_OutputLocationId",
                        column: x => x.OutputLocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Warehouses_Locations_StockLocationId",
                        column: x => x.StockLocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReorderingRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: true),
                    MinQty = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    MaxQty = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    OrderQty = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    LeadDays = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReorderingRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReorderingRules_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReorderingRules_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReorderingRules_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StockPickingTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Code = table.Column<int>(type: "int", nullable: false),
                    Sequence = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: true),
                    DefaultSourceLocationId = table.Column<int>(type: "int", nullable: true),
                    DefaultDestLocationId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockPickingTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockPickingTypes_Locations_DefaultDestLocationId",
                        column: x => x.DefaultDestLocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockPickingTypes_Locations_DefaultSourceLocationId",
                        column: x => x.DefaultSourceLocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockPickingTypes_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StockPickings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PickingTypeId = table.Column<int>(type: "int", nullable: false),
                    SourceLocationId = table.Column<int>(type: "int", nullable: false),
                    DestLocationId = table.Column<int>(type: "int", nullable: false),
                    PartnerId = table.Column<int>(type: "int", nullable: true),
                    PartnerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PartnerType = table.Column<int>(type: "int", nullable: false),
                    ScheduledDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DoneDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SourceDocument = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockPickings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockPickings_Locations_DestLocationId",
                        column: x => x.DestLocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockPickings_Locations_SourceLocationId",
                        column: x => x.SourceLocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockPickings_StockPickingTypes_PickingTypeId",
                        column: x => x.PickingTypeId,
                        principalTable: "StockPickingTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StockMoves",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Reference = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ProductUomQty = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: true),
                    PickingId = table.Column<int>(type: "int", nullable: true),
                    SourceLocationId = table.Column<int>(type: "int", nullable: false),
                    DestLocationId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UnitCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LotId = table.Column<int>(type: "int", nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockMoves", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockMoves_Locations_DestLocationId",
                        column: x => x.DestLocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockMoves_Locations_SourceLocationId",
                        column: x => x.SourceLocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockMoves_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockMoves_StockLots_LotId",
                        column: x => x.LotId,
                        principalTable: "StockLots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockMoves_StockPickings_PickingId",
                        column: x => x.PickingId,
                        principalTable: "StockPickings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockMoves_UnitOfMeasures_UomId",
                        column: x => x.UomId,
                        principalTable: "UnitOfMeasures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLines_ProductId",
                table: "InvoiceLines",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAdjustmentLines_AdjustmentId",
                table: "InventoryAdjustmentLines",
                column: "AdjustmentId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAdjustmentLines_LocationId",
                table: "InventoryAdjustmentLines",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAdjustmentLines_LotId",
                table: "InventoryAdjustmentLines",
                column: "LotId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAdjustmentLines_ProductId",
                table: "InventoryAdjustmentLines",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAdjustments_LocationId",
                table: "InventoryAdjustments",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAdjustments_WarehouseId",
                table: "InventoryAdjustments",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_ParentLocationId",
                table: "Locations",
                column: "ParentLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_WarehouseId",
                table: "Locations",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_ParentCategoryId",
                table: "ProductCategories",
                column: "ParentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_AccountExpenseId",
                table: "Products",
                column: "AccountExpenseId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_AccountStockInputId",
                table: "Products",
                column: "AccountStockInputId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_AccountStockOutputId",
                table: "Products",
                column: "AccountStockOutputId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Barcode",
                table: "Products",
                column: "Barcode",
                unique: true,
                filter: "[Barcode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_InternalReference",
                table: "Products",
                column: "InternalReference");

            migrationBuilder.CreateIndex(
                name: "IX_Products_PurchaseUomId",
                table: "Products",
                column: "PurchaseUomId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_UomId",
                table: "Products",
                column: "UomId");

            migrationBuilder.CreateIndex(
                name: "IX_ReorderingRules_LocationId",
                table: "ReorderingRules",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_ReorderingRules_ProductId",
                table: "ReorderingRules",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ReorderingRules_WarehouseId",
                table: "ReorderingRules",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_StockLots_ProductId",
                table: "StockLots",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_StockMoves_DestLocationId",
                table: "StockMoves",
                column: "DestLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_StockMoves_LotId",
                table: "StockMoves",
                column: "LotId");

            migrationBuilder.CreateIndex(
                name: "IX_StockMoves_PickingId",
                table: "StockMoves",
                column: "PickingId");

            migrationBuilder.CreateIndex(
                name: "IX_StockMoves_ProductId",
                table: "StockMoves",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_StockMoves_SourceLocationId",
                table: "StockMoves",
                column: "SourceLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_StockMoves_UomId",
                table: "StockMoves",
                column: "UomId");

            migrationBuilder.CreateIndex(
                name: "IX_StockPickings_DestLocationId",
                table: "StockPickings",
                column: "DestLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_StockPickings_Name",
                table: "StockPickings",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_StockPickings_PickingTypeId",
                table: "StockPickings",
                column: "PickingTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_StockPickings_SourceLocationId",
                table: "StockPickings",
                column: "SourceLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_StockPickingTypes_DefaultDestLocationId",
                table: "StockPickingTypes",
                column: "DefaultDestLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_StockPickingTypes_DefaultSourceLocationId",
                table: "StockPickingTypes",
                column: "DefaultSourceLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_StockPickingTypes_WarehouseId",
                table: "StockPickingTypes",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_StockQuants_LocationId",
                table: "StockQuants",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_StockQuants_LotId",
                table: "StockQuants",
                column: "LotId");

            migrationBuilder.CreateIndex(
                name: "IX_StockQuants_ProductId_LocationId_LotId",
                table: "StockQuants",
                columns: new[] { "ProductId", "LocationId", "LotId" },
                unique: true,
                filter: "[LotId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UnitOfMeasures_CategoryId",
                table: "UnitOfMeasures",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_Code",
                table: "Warehouses",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_InputLocationId",
                table: "Warehouses",
                column: "InputLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_OutputLocationId",
                table: "Warehouses",
                column: "OutputLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_StockLocationId",
                table: "Warehouses",
                column: "StockLocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceLines_Products_ProductId",
                table: "InvoiceLines",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryAdjustmentLines_InventoryAdjustments_AdjustmentId",
                table: "InventoryAdjustmentLines",
                column: "AdjustmentId",
                principalTable: "InventoryAdjustments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryAdjustmentLines_Locations_LocationId",
                table: "InventoryAdjustmentLines",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryAdjustments_Locations_LocationId",
                table: "InventoryAdjustments",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryAdjustments_Warehouses_WarehouseId",
                table: "InventoryAdjustments",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Locations_Warehouses_WarehouseId",
                table: "Locations",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceLines_Products_ProductId",
                table: "InvoiceLines");

            migrationBuilder.DropForeignKey(
                name: "FK_Warehouses_Locations_InputLocationId",
                table: "Warehouses");

            migrationBuilder.DropForeignKey(
                name: "FK_Warehouses_Locations_OutputLocationId",
                table: "Warehouses");

            migrationBuilder.DropForeignKey(
                name: "FK_Warehouses_Locations_StockLocationId",
                table: "Warehouses");

            migrationBuilder.DropTable(
                name: "InventoryAdjustmentLines");

            migrationBuilder.DropTable(
                name: "ReorderingRules");

            migrationBuilder.DropTable(
                name: "StockMoves");

            migrationBuilder.DropTable(
                name: "StockQuants");

            migrationBuilder.DropTable(
                name: "InventoryAdjustments");

            migrationBuilder.DropTable(
                name: "StockPickings");

            migrationBuilder.DropTable(
                name: "StockLots");

            migrationBuilder.DropTable(
                name: "StockPickingTypes");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "ProductCategories");

            migrationBuilder.DropTable(
                name: "UnitOfMeasures");

            migrationBuilder.DropTable(
                name: "UnitOfMeasureCategories");

            migrationBuilder.DropTable(
                name: "Locations");

            migrationBuilder.DropTable(
                name: "Warehouses");

            migrationBuilder.DropIndex(
                name: "IX_InvoiceLines_ProductId",
                table: "InvoiceLines");
        }
    }
}
