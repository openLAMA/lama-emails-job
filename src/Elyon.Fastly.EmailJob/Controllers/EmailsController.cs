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
using System.Threading.Tasks;
using Elyon.Fastly.EmailJob.Domain.Dtos;
using Elyon.Fastly.EmailJob.Domain.Services;
using Elyon.Fastly.EmailJob.RestClient.Api;
using Elyon.Fastly.EmailJob.RestClient.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Elyon.Fastly.EmailJob.Controllers
{
    [Route("api/emails")]
    [ApiController]
    public class EmailsController : ControllerBase, IEmailsApi
    {
        private readonly IEmailsService _emailsService;

        public EmailsController(IEmailsService emailsService)
        {
            _emailsService = emailsService;
        }

        [HttpGet]
        public async Task<ActionResult<ICollection<EmailDto>>> GetUnsentEmailsAsync()
        {
            return Ok(await _emailsService.GetUnsentEmailsAsync()
                .ConfigureAwait(false));
        }

        [HttpPost]
        public async Task SendEmailAsync(EmailSpecModel email)
        {
            if (email == null)
                throw new ArgumentNullException(nameof(email));

            await _emailsService
                .SendEmail(email.Receiver, email.CcReceivers, email.TemplateName, email.AttachmentFilesHashes, email.Parameters)
                .ConfigureAwait(false);
        }
    }
}
