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
using System.Threading.Tasks;
using AutoMapper;
using Elyon.Fastly.EmailJob.Domain.Dtos;
using Elyon.Fastly.EmailJob.Domain.Repositories;
using Elyon.Fastly.EmailJob.Domain.Services;
using Elyon.Fastly.EmailJob.PostgresRepositories.Entities;
using Microsoft.EntityFrameworkCore;

namespace Elyon.Fastly.EmailJob.PostgresRepositories
{
    public class EmailsRepository : BaseRepository, IEmailsRepository
    {
        private readonly IAESCryptography _aesCryptography;

        public EmailsRepository(
            Prime.Sdk.Db.Common.IDbContextFactory<EmailJobContext> contextFactory, 
            IMapper mapper, IAESCryptography aesCryptography)
            : base(contextFactory, mapper)
        {
            _aesCryptography = aesCryptography;
        }

        public async Task<Guid> AddEmailToQueueAsync(string receiver, string templateName, Dictionary<string, string> parameters)
        {
            if (receiver == null)
                throw new ArgumentNullException(nameof(receiver));
            if (templateName == null)
                throw new ArgumentNullException(nameof(templateName));
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            await using var context = ContextFactory.CreateDataContext();

            var templateId = await context.Set<EmailTemplate>()
                .AsNoTracking()
                .Where(t => t.Name == templateName)
                .Select(t => t.Id)
                .FirstAsync()
                .ConfigureAwait(false);
            
            var items = context.Set<Email>();
            var entity = new Email
            {
                Id = Guid.NewGuid(),
                CreatedOn = DateTime.UtcNow,
                IsProcessed = false,
                Receiver = _aesCryptography.Encrypt(receiver),
                TemplateId = templateId
            };

            entity.Parameters = new List<EmailParameter>();
            foreach (var param in parameters)
                entity.Parameters.Add(new EmailParameter
                {
                    Id = Guid.NewGuid(),
                    Email = entity,
                    ParamName = param.Key,
                    ParamContent = _aesCryptography.Encrypt(param.Value)
                });


            await items.AddAsync(entity).ConfigureAwait(false);
            await context.SaveChangesAsync().ConfigureAwait(false);

            context.Entry(entity).State = EntityState.Detached;

            return entity.Id;
        }

        public async Task<ICollection<EmailDto>> GetUnsentEmailsAsync()
        {
            await using var context = ContextFactory.CreateDataContext();

            var emails = await context.Set<Email>()
                .AsNoTracking()
                .Where(e => !e.IsProcessed)
                .OrderBy(o => o.CreatedOn)
                .Select(entity => new EmailDto
                {
                    Id = entity.Id,
                    Receiver = _aesCryptography.Decrypt(entity.Receiver),
                    TemplateName = entity.Template.Name
                })
                .ToListAsync()
                .ConfigureAwait(false);

            // Get the templates content
            var templatesToDownload = emails.Select(e => e.TemplateName).Distinct().ToList();
            var templatesContent = await context.Set<EmailTemplate>()
                .AsNoTracking()
                .Where(et => templatesToDownload.Contains(et.Name))
                .ToDictionaryAsync(et => et.Name, et => Tuple.Create(et.Subject, et.Content))
                .ConfigureAwait(false);
            // Populate the templates content and passed parameters
            foreach (var email in emails)
            {
                email.Subject = templatesContent[email.TemplateName].Item1;
                email.Content = templatesContent[email.TemplateName].Item2;
                email.Parameters = await context.Set<EmailParameter>()
                    .AsNoTracking()
                    .Where(ep => ep.EmailId == email.Id)
                    .ToDictionaryAsync(ep => ep.ParamName, ep => _aesCryptography.Decrypt(ep.ParamContent))
                    .ConfigureAwait(false);
            }

            return emails;
        }

        public async Task MarkMailAsProcessedAsync(Guid emailId, bool isDeliverySuccessful, string errorMessage)
        {
            await using var context = ContextFactory.CreateDataContext();

            var items = context.Set<Email>();
            var entity = await items
                .FirstAsync(e => e.Id == emailId)
                .ConfigureAwait(false);
            entity.IsProcessed = true;
            entity.SentOn = DateTime.UtcNow;
            entity.IsDeliverySuccessful = isDeliverySuccessful;
            if (!isDeliverySuccessful)
                entity.DeliveryMessage = errorMessage;

            context.Entry(entity).State = EntityState.Modified;
            items.Update(entity);

            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        public Task<ICollection<EmailDto>> GetAllEmails()
        {
            throw new NotImplementedException();
        }
    }
}
