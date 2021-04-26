﻿#region Copyright
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

// <auto-generated />
using System;
using Elyon.Fastly.EmailJob.PostgresRepositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Elyon.Fastly.EmailJob.PostgresRepositories.Migrations
{
    [DbContext(typeof(EmailJobContext))]
    [Migration("20210426144539_AddMaxlengthForFilenameAndHash")]
    partial class AddMaxlengthForFilenameAndHash
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("EmailJobSchema")
                .UseIdentityByDefaultColumns()
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.2");

            modelBuilder.Entity("Elyon.Fastly.EmailJob.PostgresRepositories.Entities.Attachment", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<byte[]>("Content")
                        .IsRequired()
                        .HasColumnType("bytea");

                    b.Property<string>("FileName")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<string>("OriginalXXHash")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.HasKey("Id");

                    b.ToTable("Attachments");
                });

            modelBuilder.Entity("Elyon.Fastly.EmailJob.PostgresRepositories.Entities.Email", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("CcReceivers")
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("DeliveryMessage")
                        .HasColumnType("text");

                    b.Property<bool?>("IsDeliverySuccessful")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsProcessed")
                        .HasColumnType("boolean");

                    b.Property<string>("Receiver")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<DateTime?>("SentOn")
                        .HasColumnType("timestamp without time zone");

                    b.Property<Guid>("TemplateId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("IsProcessed");

                    b.HasIndex("TemplateId");

                    b.ToTable("Emails");
                });

            modelBuilder.Entity("Elyon.Fastly.EmailJob.PostgresRepositories.Entities.EmailAttachment", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("AttachmentId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("EmailId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("AttachmentId");

                    b.HasIndex("EmailId");

                    b.ToTable("EmailAttachments");
                });

            modelBuilder.Entity("Elyon.Fastly.EmailJob.PostgresRepositories.Entities.EmailParameter", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("EmailId")
                        .HasColumnType("uuid");

                    b.Property<string>("ParamContent")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<string>("ParamName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.HasKey("Id");

                    b.HasIndex("EmailId");

                    b.ToTable("EmailParameters");
                });

            modelBuilder.Entity("Elyon.Fastly.EmailJob.PostgresRepositories.Entities.EmailTemplate", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<string>("Subject")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("EmailTemplates");
                });

            modelBuilder.Entity("Elyon.Fastly.EmailJob.PostgresRepositories.Entities.Email", b =>
                {
                    b.HasOne("Elyon.Fastly.EmailJob.PostgresRepositories.Entities.EmailTemplate", "Template")
                        .WithMany("Emails")
                        .HasForeignKey("TemplateId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Template");
                });

            modelBuilder.Entity("Elyon.Fastly.EmailJob.PostgresRepositories.Entities.EmailAttachment", b =>
                {
                    b.HasOne("Elyon.Fastly.EmailJob.PostgresRepositories.Entities.Attachment", "Attachment")
                        .WithMany()
                        .HasForeignKey("AttachmentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Elyon.Fastly.EmailJob.PostgresRepositories.Entities.Email", "Email")
                        .WithMany("Attachments")
                        .HasForeignKey("EmailId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Attachment");

                    b.Navigation("Email");
                });

            modelBuilder.Entity("Elyon.Fastly.EmailJob.PostgresRepositories.Entities.EmailParameter", b =>
                {
                    b.HasOne("Elyon.Fastly.EmailJob.PostgresRepositories.Entities.Email", "Email")
                        .WithMany("Parameters")
                        .HasForeignKey("EmailId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Email");
                });

            modelBuilder.Entity("Elyon.Fastly.EmailJob.PostgresRepositories.Entities.Email", b =>
                {
                    b.Navigation("Attachments");

                    b.Navigation("Parameters");
                });

            modelBuilder.Entity("Elyon.Fastly.EmailJob.PostgresRepositories.Entities.EmailTemplate", b =>
                {
                    b.Navigation("Emails");
                });
#pragma warning restore 612, 618
        }
    }
}
