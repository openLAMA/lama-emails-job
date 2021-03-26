#region Copyright
// openLAMA is an open source platform which has been developed by the
// Swiss Kanton Basel Landschaft, with the goal of automating and managing
// large scale Covid testing programs or any other pandemic/viral infections.

// Copyright(C) 2021 Kanton Basel Landschaft, Switzerland
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// See LICENSE.md in the project root for license information.
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see https://www.gnu.org/licenses/.
#endregion

using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Elyon.Fastly.EmailJob.PostgresRepositories.Migrations
{
    public partial class AddEmailsTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "EmailJobSchema");

            migrationBuilder.CreateTable(
                name: "EmailTemplates",
                schema: "EmailJobSchema",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Subject = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Emails",
                schema: "EmailJobSchema",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    Receiver = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsProcessed = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    SentOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeliverySuccessful = table.Column<bool>(type: "boolean", nullable: true),
                    DeliveryMessage = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Emails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Emails_EmailTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalSchema: "EmailJobSchema",
                        principalTable: "EmailTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmailParameters",
                schema: "EmailJobSchema",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmailId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParamName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ParamContent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailParameters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailParameters_Emails_EmailId",
                        column: x => x.EmailId,
                        principalSchema: "EmailJobSchema",
                        principalTable: "Emails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailParameters_EmailId",
                schema: "EmailJobSchema",
                table: "EmailParameters",
                column: "EmailId");

            migrationBuilder.CreateIndex(
                name: "IX_Emails_IsProcessed",
                schema: "EmailJobSchema",
                table: "Emails",
                column: "IsProcessed");

            migrationBuilder.CreateIndex(
                name: "IX_Emails_TemplateId",
                schema: "EmailJobSchema",
                table: "Emails",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplates_Name",
                schema: "EmailJobSchema",
                table: "EmailTemplates",
                column: "Name",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailParameters",
                schema: "EmailJobSchema");

            migrationBuilder.DropTable(
                name: "Emails",
                schema: "EmailJobSchema");

            migrationBuilder.DropTable(
                name: "EmailTemplates",
                schema: "EmailJobSchema");
        }
    }
}
