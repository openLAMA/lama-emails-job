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
using Elyon.Fastly.EmailJob.Authentication;
using Elyon.Fastly.EmailJob.Domain.Dtos;
using Elyon.Fastly.EmailJob.Domain.Services;
using Elyon.Fastly.EmailJob.PeriodicalHandlers;
using Elyon.Fastly.EmailJob.Profiles;
using Elyon.Fastly.EmailJob.Settings;
using Prime.Sdk.Common;
using Prime.Sdk.ConfigReader.ReloadingManager;
using Prime.Sdk.Logging;

namespace Elyon.Fastly.EmailJob.Modules
{
    public class AutofacModule : Module
    {
        private readonly IReloadingManager<AppSettings> _appSettings;
        private const string usersAuthsParamName = "usersAuths";

        public AutofacModule(IReloadingManager<AppSettings> appSettings)
        {
            _appSettings = appSettings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(ctx =>
                    ctx.Resolve<MapperConfiguration>().CreateMapper())
                .As<IMapper>()
                .InstancePerLifetimeScope();

            var mapper = new MapperConfiguration(c => c.AddProfile(typeof(AutoMapperProfile)))
                .CreateMapper();

            builder.RegisterModule(new DomainServices.AutofacModule(
                mapper.Map<MailSettingsDto>(_appSettings.CurrentValue.MailSettings)));
            builder.RegisterModule(new PostgresRepositories.AutofacModule(_appSettings.CurrentValue.DbConnectionString,
                _appSettings.CurrentValue.CryptographySettings.EncryptionKey, _appSettings.CurrentValue.CryptographySettings.EncryptionIV));
            builder.RegisterLogging(_appSettings.CurrentValue.LoggerSettings);

            builder.RegisterType<BasicUserAuthentication>()
                .As<IBasicUserAuthentication>()
                .SingleInstance()
                .WithParameter(usersAuthsParamName, _appSettings.CurrentValue.UsersAuthentication);

            builder.RegisterType<EmailSenderPeriodicHandler>()
                .As<IStartable>()
                .As<IStoppable>()
                .SingleInstance();
        }
    }
}
