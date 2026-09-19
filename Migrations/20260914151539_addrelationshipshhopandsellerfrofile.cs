using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DATN.Migrations
{
    /// <inheritdoc />
    public partial class addrelationshipshhopandsellerfrofile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SellerProfiles_Shops_ShopID",
                table: "SellerProfiles");

            migrationBuilder.DropIndex(
                name: "IX_SellerProfiles_ShopID",
                table: "SellerProfiles");
        }
    }
}
