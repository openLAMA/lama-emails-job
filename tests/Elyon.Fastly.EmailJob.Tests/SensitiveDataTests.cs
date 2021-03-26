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
using Elyon.Fastly.EmailJob.RestClient;
using Refit;
using Xunit;

namespace Elyon.Fastly.EmailJob.Tests
{
    public class SensitiveDataTests
    {
        private readonly Type _refitGetAttrType = typeof(GetAttribute);

        private readonly List<string> _sensitiveParamsNames = new List<string>
        {
            "name", "email", "phone", "login"
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

            var sensitiveDataParams = new List<string>();

            foreach (var apiInterface in apiInterfaces)
            {
                var interfaceMethods = apiInterface.GetMethods();

                foreach (var apiMethod in interfaceMethods)
                {
                    var refitGetAttr = apiMethod.CustomAttributes.FirstOrDefault(a => _refitGetAttrType == a.AttributeType);
                    if (refitGetAttr == null)
                        continue;

                    var methodParams = apiMethod.GetParameters();
                    var paramsWithSensitiveData = methodParams.Where(p => _sensitiveParamsNames.Any(s => p.Name.ToLower().Contains(s)));
                    sensitiveDataParams.AddRange(
                        paramsWithSensitiveData.Select(i => $"{i.Name} from {apiInterface.Name}.{apiMethod.Name}"));
                }
            }

            Assert.True(
                sensitiveDataParams.Count == 0,
                "These parameters might lead to exposing sensitive data when building url via refit: " + string.Join(", ", sensitiveDataParams));
        }
    }
}
