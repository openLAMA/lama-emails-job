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
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Prime.Sdk.Logging;

namespace Elyon.Fastly.EmailJob.Authentication
{
    namespace WebApi.Helpers
    {
        public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
        {
            private readonly IBasicUserAuthentication _basicAuthService;
            private readonly ILog _log;
            private const string MissingAuthorizationHeader = "Missing Authorization Header";
            private const string InvalidUsernameOrPassword = "Invalid Username or Password";
            private const string InvalidAuthorizationHeader = "Invalid Authorization Header";

            public BasicAuthenticationHandler(
                IOptionsMonitor<AuthenticationSchemeOptions> options,
                ILoggerFactory logger,
                UrlEncoder encoder,
                ISystemClock clock,
                IBasicUserAuthentication basicAuthService,
                ILogFactory logFactory)
                : base(options, logger, encoder, clock)
            {
                if (logFactory == null)
                    return;

                _log = logFactory.CreateLog(this);
                _basicAuthService = basicAuthService;
            }

            protected override Task<AuthenticateResult> HandleAuthenticateAsync()
            {
                var endpoint = Context.GetEndpoint();
                if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
                    return Task.FromResult(AuthenticateResult.NoResult());

                if (!Request.Headers.ContainsKey("Authorization"))
                {
                    _log.Error(MissingAuthorizationHeader);
                    return Task.FromResult(AuthenticateResult.Fail(MissingAuthorizationHeader));
                }

                try
                {
                    var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                    var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
                    var credentials = Encoding.UTF8.GetString(credentialBytes).Split(new[] { ':' }, 2);
                    var username = credentials[0];
                    var password = credentials[1];
                    if (!_basicAuthService.Authenticate(username, password))
                    {
                        _log.Error(InvalidUsernameOrPassword);
                        return Task.FromResult(AuthenticateResult.Fail(InvalidUsernameOrPassword));
                    }

                    var claims = new[] {
                        new Claim(ClaimTypes.NameIdentifier, username),
                        new Claim(ClaimTypes.Name, username),
                    };
                    var identity = new ClaimsIdentity(claims, Scheme.Name);
                    var principal = new ClaimsPrincipal(identity);
                    var ticket = new AuthenticationTicket(principal, Scheme.Name);

                    return Task.FromResult(AuthenticateResult.Success(ticket));
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    _log.Error(InvalidAuthorizationHeader);
                    return Task.FromResult(AuthenticateResult.Fail(InvalidAuthorizationHeader));
                }                
            }
        }
    }
}
