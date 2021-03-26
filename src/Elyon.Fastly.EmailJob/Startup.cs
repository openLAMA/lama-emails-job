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
using System.Diagnostics.CodeAnalysis;
using AutoMapper;
using Elyon.Fastly.EmailJob.Authentication;
using Elyon.Fastly.EmailJob.Authentication.WebApi.Helpers;
using Elyon.Fastly.EmailJob.Settings;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Prime.Sdk.Configuration.AppBuilder;
using Prime.Sdk.Configuration.Services;
using Prime.Sdk.Swagger;

namespace Elyon.Fastly.EmailJob
{
    [UsedImplicitly]
    public class Startup
    {
        private readonly SwaggerOptions _swaggerOptions = new SwaggerOptions
        {
            ApiTitle = "EmailJob API",
            ApiVersion = "v1"
        };

        [UsedImplicitly]
        [SuppressMessage("Microsoft.Performance", "CA1822", Justification = "Standard Startup method")]
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            return services.UsePrimeServiceConfiguration<AppSettings>((options, appSettings) =>
            {
                options.Swagger = _swaggerOptions;

                options.Swagger.ConfigureSwagger = swagger =>
                {
                    swagger.IgnoreObsoleteActions();

                    swagger.AddSecurityDefinition("basic", new OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        Type = SecuritySchemeType.Http,
                        Scheme = "basic",
                        In = ParameterLocation.Header,
                        Description = "Basic Authorization header using the Bearer scheme."
                    });
                    swagger.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "basic"
                                }
                            },
#pragma warning disable CA1825 // Avoid zero-length array allocations.
                            new string[] {}
#pragma warning restore CA1825 // Avoid zero-length array allocations.
                        }
                    });
                };

                options.AdditionalServicesConfiguration = serv =>
                {
                };
            });
        }

        [UsedImplicitly]
        [SuppressMessage("Microsoft.Performance", "CA1822", Justification = "Standard Startup method")]
        public void Configure(IApplicationBuilder app, IMapper mapper)
        {
            app.UsePrimeAppBuilderConfiguration(options =>
            {
                options.WithMiddleware = x =>
                {
                };

                options.EnableUnhandledExceptionLoggingMiddleware = false;
                options.EnableValidationExceptionMiddleware = false;
            });

            mapper?.ConfigurationProvider.AssertConfigurationIsValid();
        }
    }
}
