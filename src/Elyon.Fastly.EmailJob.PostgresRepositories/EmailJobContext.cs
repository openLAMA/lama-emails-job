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
using System.Data.Common;
using Elyon.Fastly.EmailJob.PostgresRepositories.Entities;
using Microsoft.EntityFrameworkCore;
using Prime.Sdk.PostgreSql;

namespace Elyon.Fastly.EmailJob.PostgresRepositories
{
    public class EmailJobContext : PostgreSqlContext
    {
        private const string Schema = "EmailJobSchema";

        public DbSet<Email> Emails { get; set; }

        public DbSet<EmailParameter> EmailParameters { get; set; }

        public DbSet<EmailTemplate> EmailTemplates { get; set; }

        public DbSet<Attachment> Attachments { get; set; }

        public DbSet<EmailAttachment> EmailAttachments { get; set; }

        public EmailJobContext() : base(Schema)
        {
        }

        public EmailJobContext(string connectionString, bool isTraceEnabled)
            : base(Schema, connectionString, isTraceEnabled)
        {
        }

        public EmailJobContext(DbContextOptions options)
            : base(Schema, options)
        {
        }

        public EmailJobContext(DbConnection dbConnection)
            : base(Schema, dbConnection)
        {
        }

        protected override void OnPrimeModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder == null)
                throw new ArgumentNullException(nameof(modelBuilder));

            modelBuilder.Entity<EmailTemplate>()
                .HasIndex(b => b.Name);

            modelBuilder.Entity<Email>()
                .HasMany(e => e.Parameters)
                .WithOne(u => u.Email)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EmailTemplate>()
                .HasMany(et => et.Emails)
                .WithOne(u => u.Template);

            modelBuilder.Entity<Email>()
                .HasMany(e => e.Attachments)
                .WithOne(u => u.Email)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
