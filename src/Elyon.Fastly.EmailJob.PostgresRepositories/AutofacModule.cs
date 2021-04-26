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

using Autofac;
using AutoMapper;
using Elyon.Fastly.EmailJob.Domain.Repositories;
using Elyon.Fastly.EmailJob.Domain.Services;
using Elyon.Fastly.EmailJob.PostgresRepositories.Helpers;
using Prime.Sdk.PostgreSql;

namespace Elyon.Fastly.EmailJob.PostgresRepositories
{
    public class AutofacModule : Module
    {
        private const string encryptionKeyParamName = "encryptionKey";
        private const string encryptionIVParamName = "encryptionIV";

        private readonly string _connectionString;
        private readonly string _encryptionKey;
        private readonly string _encryptionIV;

        public AutofacModule(string connectionString, string encryptionKey, string encryptionIV)
        {
            _connectionString = connectionString;
            _encryptionKey = encryptionKey;
            _encryptionIV = encryptionIV;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterPostgreSql(
                _connectionString,
                connString => new EmailJobContext(connString, false), 
                dbConn => new EmailJobContext(dbConn));

            builder.RegisterType<EmailTemplatesRepository>()
                .As<IEmailTemplatesRepository>()
                .SingleInstance();

            builder.RegisterType<EmailsRepository>()
                .As<IEmailsRepository>()
                .SingleInstance();

            builder.RegisterType<AttachmentsRepository>()
                .As<IAttachmentsRepository>()
                .SingleInstance();

            builder.RegisterType<AESCryptography>()
                .As<IAESCryptography>()
                .SingleInstance()
                .WithParameter(encryptionKeyParamName, _encryptionKey)
                .WithParameter(encryptionIVParamName, _encryptionIV);
        }
    }
}
