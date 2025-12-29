using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Tracking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "tracking");

            migrationBuilder.CreateTable(
                name: "delivery_tracking",
                schema: "tracking",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    DriverId = table.Column<Guid>(type: "uuid", nullable: false),
                    PickupLatitude = table.Column<double>(type: "double precision", nullable: false),
                    PickupLongitude = table.Column<double>(type: "double precision", nullable: false),
                    PickupTimestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeliveryLatitude = table.Column<double>(type: "double precision", nullable: false),
                    DeliveryLongitude = table.Column<double>(type: "double precision", nullable: false),
                    DeliveryTimestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CurrentLatitude = table.Column<double>(type: "double precision", nullable: true),
                    CurrentLongitude = table.Column<double>(type: "double precision", nullable: true),
                    CurrentTimestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PickedUpAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EstimatedDistanceKm = table.Column<double>(type: "double precision", precision: 10, scale: 2, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_delivery_tracking", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "drivers",
                schema: "tracking",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    VehicleNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CurrentLatitude = table.Column<double>(type: "double precision", nullable: true),
                    CurrentLongitude = table.Column<double>(type: "double precision", nullable: true),
                    LocationTimestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CurrentOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_drivers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "location_history",
                schema: "tracking",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeliveryTrackingId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_location_history", x => x.Id);
                    table.ForeignKey(
                        name: "FK_location_history_delivery_tracking_DeliveryTrackingId",
                        column: x => x.DeliveryTrackingId,
                        principalSchema: "tracking",
                        principalTable: "delivery_tracking",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_delivery_tracking_DeliveredAt",
                schema: "tracking",
                table: "delivery_tracking",
                column: "DeliveredAt");

            migrationBuilder.CreateIndex(
                name: "IX_delivery_tracking_DriverId",
                schema: "tracking",
                table: "delivery_tracking",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_delivery_tracking_OrderId",
                schema: "tracking",
                table: "delivery_tracking",
                column: "OrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_drivers_CurrentOrderId",
                schema: "tracking",
                table: "drivers",
                column: "CurrentOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_drivers_Status",
                schema: "tracking",
                table: "drivers",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_location_history_DeliveryTrackingId",
                schema: "tracking",
                table: "location_history",
                column: "DeliveryTrackingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "drivers",
                schema: "tracking");

            migrationBuilder.DropTable(
                name: "location_history",
                schema: "tracking");

            migrationBuilder.DropTable(
                name: "delivery_tracking",
                schema: "tracking");
        }
    }
}
