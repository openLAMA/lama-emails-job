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
using System.Threading.Tasks;
using Elyon.Fastly.EmailJob.Domain;
using Elyon.Fastly.EmailJob.Domain.Dtos;
using Elyon.Fastly.EmailJob.Domain.Services;
using Elyon.Fastly.EmailJob.RestClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Elyon.Fastly.EmailJob.Controllers
{
    [Route("api/emailTemplates")]
    [ApiController]
    public class EmailTemplatesController : ControllerBase
    {
        private readonly IEmailTemplatesService _service;

        public EmailTemplatesController(IEmailTemplatesService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResults<EmailTemplateDto>>> GetAsync()
        {
            return await _service.GetListAsync(new Paginator(), 
                    null, null)
                .ConfigureAwait(false);
        }

        [HttpPost]
        public async Task<ActionResult<EmailTemplateDto>> InsertAsync(EmailTemplateDto emailTemplate)
        {
            return await _service.AddAsync(emailTemplate)
                .ConfigureAwait(false);
        }

        [HttpPut]
        public async Task UpdateAsync(EmailTemplateDto emailTemplate)
        {
            await _service.UpdateAsync(emailTemplate)
                .ConfigureAwait(false);
        }

        [HttpDelete]
        public async Task DeleteAsync(Guid id)
        {
            await _service.DeleteAsync(id)
                .ConfigureAwait(false);
        }
    }
}
