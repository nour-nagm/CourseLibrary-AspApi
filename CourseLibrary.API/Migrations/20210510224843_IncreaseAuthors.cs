using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CourseLibrary.API.Migrations
{
    public partial class IncreaseAuthors : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Authors",
                columns: new[] { "Id", "DateOfBirth", "FirstName", "LastName", "MainCategory" },
                values: new object[,]
                {
                    { new Guid("71838f8b-6ab3-4539-9e67-4e77b8ede1c0"), new DateTimeOffset(new DateTime(1969, 8, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 3, 0, 0, 0)), "Huxford", "The Hawk Morris", "Maps" },
                    { new Guid("119f9ccb-149d-4d3c-ad4f-40100f38e918"), new DateTimeOffset(new DateTime(1972, 1, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 2, 0, 0, 0)), "Dwennon", "Rigger Quye", "Maps" },
                    { new Guid("28c1db41-f104-46e6-8943-d31c0291e0e3"), new DateTimeOffset(new DateTime(1982, 5, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 3, 0, 0, 0)), "Rushford", "Subtle Asema", "Rum" },
                    { new Guid("d94a64c2-2e8f-4162-9976-0ffe03d30767"), new DateTimeOffset(new DateTime(1976, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 3, 0, 0, 0)), "Hagley", "Imposter Grendel", "Singing" },
                    { new Guid("380c2c6b-0d1c-4b82-9d83-3cf635a3e62b"), new DateTimeOffset(new DateTime(1977, 2, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 2, 0, 0, 0)), "Mabel", "Barnacle Grendel", "Maps" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: new Guid("119f9ccb-149d-4d3c-ad4f-40100f38e918"));

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: new Guid("28c1db41-f104-46e6-8943-d31c0291e0e3"));

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: new Guid("380c2c6b-0d1c-4b82-9d83-3cf635a3e62b"));

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: new Guid("71838f8b-6ab3-4539-9e67-4e77b8ede1c0"));

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: new Guid("d94a64c2-2e8f-4162-9976-0ffe03d30767"));
        }
    }
}
