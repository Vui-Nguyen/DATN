using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DATN.Migrations
{
    /// <inheritdoc />
    public partial class ChangeSellerProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SellerProfiles_Shops_ShopID",
                table: "SellerProfiles");

            migrationBuilder.DropIndex(
                name: "IX_SellerProfiles_ShopID",
                table: "SellerProfiles");

            migrationBuilder.AlterColumn<int>(
                name: "ShopID",
                table: "SellerProfiles",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_SellerProfiles_ShopID",
                table: "SellerProfiles",
                column: "ShopID",
                unique: true,
                filter: "[ShopID] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_SellerProfiles_Shops_ShopID",
                table: "SellerProfiles",
                column: "ShopID",
                principalTable: "Shops",
                principalColumn: "ShopID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SellerProfiles_Shops_ShopID",
                table: "SellerProfiles");

            migrationBuilder.DropIndex(
                name: "IX_SellerProfiles_ShopID",
                table: "SellerProfiles");

            migrationBuilder.AlterColumn<int>(
                name: "ShopID",
                table: "SellerProfiles",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SellerProfiles_ShopID",
                table: "SellerProfiles",
                column: "ShopID",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SellerProfiles_Shops_ShopID",
                table: "SellerProfiles",
                column: "ShopID",
                principalTable: "Shops",
                principalColumn: "ShopID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
