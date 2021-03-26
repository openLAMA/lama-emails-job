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
using System.Reflection;
using Elyon.Fastly.EmailJob.RestClient;
using Microsoft.AspNetCore.Mvc;
using Refit;
using Xunit;

namespace Elyon.Fastly.EmailJob.Tests
{
    public class ClientHttpPathsAreMatchedInControllersTest
    {
        private readonly Type _routeAttrType = typeof(RouteAttribute);
        private readonly List<Type> _refitAttrs = new List<Type>
        {
            typeof(GetAttribute), typeof(PostAttribute), typeof(PutAttribute), typeof(DeleteAttribute), typeof(PatchAttribute)
        };
        private readonly List<Type> _httpAttrs = new List<Type>
        {
            typeof(HttpGetAttribute), typeof(HttpPostAttribute), typeof(HttpPutAttribute), typeof(HttpDeleteAttribute), typeof(HttpPatchAttribute)
        };

        [Fact]
        public void CheckRoutesInControllersTest()
        {
            var clientInterface = typeof(IEmailJobClient);

            var apiInterfaces = clientInterface
                .GetProperties()
                .Where(p => p.CanRead && p.PropertyType.IsInterface)
                .Select(p => p.PropertyType)
                .ToHashSet();
            var controllers = Assembly.GetAssembly(typeof(Startup))
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(ControllerBase)))
                .ToList();

            var apiErrors = new List<string>();

            foreach (var apiInterface in apiInterfaces)
            {
                var implementingController = controllers.FirstOrDefault(c => c.GetInterfaces().Any(i => i == apiInterface));
                var interfaceMethods = apiInterface.GetMethods();
                if (implementingController == null)
                {
                    if (interfaceMethods.Length > 0)
                        apiErrors.Add($"Api interface '{apiInterface.Name}' is not implemented");
                    continue;
                }

                foreach (var apiMethod in interfaceMethods)
                {
                    var refitAttr = apiMethod.CustomAttributes.FirstOrDefault(a => _refitAttrs.Any(i => i == a.AttributeType));
                    if (refitAttr == null)
                    {
                        apiErrors.Add($"Refit attribute is missing on {apiInterface.Name}.{apiMethod.Name}");
                        continue;
                    }

                    var apiRoute = refitAttr.ConstructorArguments[0].Value.ToString();
                    if (apiRoute.StartsWith('/'))
                        apiRoute = apiRoute.TrimStart('/');
                    else
                        apiErrors.Add(
                            $"Route '{apiRoute}' on {apiInterface.Name}.{apiMethod.Name} is missing leading slash");

                    var implMethod = implementingController.GetMethod(
                        apiMethod.Name,
                        apiMethod.GetParameters().Select(p => p.ParameterType).ToArray());

                    var routeAttr = implMethod.CustomAttributes.FirstOrDefault(a => a.AttributeType == _routeAttrType);
                    var httpAttr = implMethod.CustomAttributes.First(a => _httpAttrs.Any(i => i == a.AttributeType));
                    var implRoute = string.Empty;
                    if ((routeAttr ?? httpAttr).ConstructorArguments.Count > 0)
                        implRoute = (routeAttr ?? httpAttr).ConstructorArguments[0].Value.ToString();

                    var controllerRouteAttr = implementingController.CustomAttributes.FirstOrDefault(a => a.AttributeType == _routeAttrType);
                    if (controllerRouteAttr != null)
                    {
                        var controllerRoute = controllerRouteAttr.ConstructorArguments[0].Value.ToString();
                        implRoute = string.IsNullOrWhiteSpace(implRoute)
                            ? controllerRoute.Trim('/')
                            : $"{controllerRoute.Trim('/')}/{implRoute.TrimStart('/')}";
                    }

                    if (apiRoute != implRoute)
                        apiErrors.Add(
                            $"Route '{apiRoute}' on {apiInterface.Name}.{apiMethod.Name} is not matched in controller - '{implRoute}'");

                    if (_refitAttrs.IndexOf(refitAttr.AttributeType) != _httpAttrs.IndexOf(httpAttr.AttributeType))
                        apiErrors.Add(
                            $"Refit '{refitAttr.AttributeType.Name}' on {apiInterface.Name}.{apiMethod.Name} is not matched in controller - '{httpAttr.AttributeType.Name}'");
                }
            }

            Assert.True(apiErrors.Count == 0, string.Join(",\t", apiErrors));
        }
    }
}
