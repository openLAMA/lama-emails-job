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
using Autofac;
using JetBrains.Annotations;
using Prime.Sdk.RestClientFactory;
using Prime.Sdk.RestClientFactory.Infrastructure;

namespace Elyon.Fastly.EmailJob.RestClient
{
    /// <summary>
    /// Extension for client registration
    /// </summary>
    [PublicAPI]
    public static class AutofacExtension
    {
        /// <summary>
        /// Registers <see cref="IEmailJobClient"/> in Autofac container using <see cref="EmailJobRestClientSettings"/>.
        /// </summary>
        /// <param name="builder">Autofac container builder.</param>
        /// <param name="settings">EmailJob client settings.</param>
        /// <param name="builderConfigure">Optional <see cref="RestClientFactoryBuilder"/> configure handler.</param>
        public static void RegisterEmailJobClient(
            [NotNull] this ContainerBuilder builder,
            [NotNull] EmailJobRestClientSettings settings,
            [CanBeNull] Func<RestClientFactoryBuilder, RestClientFactoryBuilder> builderConfigure)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));
            if (string.IsNullOrWhiteSpace(settings.ServiceUrl))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(EmailJobRestClientSettings.ServiceUrl));

            var clientBuilder = RestClientFactory.BuildForUrl(settings.ServiceUrl)
                .WithAdditionalCallsWrapper(new ExceptionHandlerCallsWrapper());

            clientBuilder = builderConfigure?.Invoke(clientBuilder) ?? clientBuilder.WithoutRetries();

            builder.RegisterInstance(new EmailJobClient(clientBuilder.Create()))
                .As<IEmailJobClient>()
                .SingleInstance();
        }
    }
}
