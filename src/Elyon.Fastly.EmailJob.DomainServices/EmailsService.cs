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
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Elyon.Fastly.EmailJob.Domain.Dtos;
using Elyon.Fastly.EmailJob.Domain.Repositories;
using Elyon.Fastly.EmailJob.Domain.Services;
using Elyon.Fastly.EmailJob.DomainServices.Mail;
using Prime.Sdk.Logging;

namespace Elyon.Fastly.EmailJob.DomainServices
{
    public class EmailsService : BaseService, IEmailsService
    {
        private readonly IEmailsRepository _repository;
        private readonly IAESCryptography _aesCryptography;
        private readonly IMailSenderService _mailSender;
        private readonly ILog _log;
        private readonly IAttachmentsService _attachmentsService;

        public EmailsService(IEmailsRepository repository, IAESCryptography aesCryptography, 
            IMailSenderService mailSender, ILogFactory logFactory, IAttachmentsService attachmentsService)
        {
            if (logFactory == null)
                throw new ArgumentNullException(nameof(logFactory));

            _repository = repository;
            _aesCryptography = aesCryptography;
            _mailSender = mailSender;
            _log = logFactory.CreateLog(this);
            _attachmentsService = attachmentsService;
        }

        public async Task<ICollection<EmailDto>> GetUnsentEmailsAsync()
        {
            return await _repository
                .GetUnsentEmailsAsync()
                .ConfigureAwait(false);
        }

        public async Task SendEmail(string receiver, ICollection<string> ccReceivers, string templateName, ICollection<string> attachmentFilesHashes, Dictionary<string, string> parameters)
        {
            var attachmentsIds = new List<Guid>();
            if (attachmentFilesHashes != default && attachmentFilesHashes.Any())
            {
                _log.Info($"Get attachmentsIds for attachment files \"{string.Join(", ", attachmentFilesHashes)}\", template \"{templateName}\"");
                attachmentsIds = await _attachmentsService.GetAttachmentsIds(attachmentFilesHashes).ConfigureAwait(false);
                if (attachmentFilesHashes.Count != attachmentsIds.Count)
                {
                    _log.Error($"Email not added to queue - not all attachments have been found in the DB. Attachments files hashes count: {attachmentFilesHashes.Count}, files in DB count: {attachmentsIds.Count}");
                    return;
                }
            }

            _log.Info($"New email queued for send with template \"{templateName}\"");
            await _repository
                .AddEmailToQueueAsync(receiver, ccReceivers, templateName, attachmentsIds, parameters)
                .ConfigureAwait(false);
        }

        public async Task SendUnsentEmailsAsync()
        {
            var unsentEmails = await GetUnsentEmailsAsync().ConfigureAwait(false);

            if (unsentEmails.Any())
                _log.Info($"Sending {unsentEmails.Count} emails...");
            foreach (var email in unsentEmails)
            {
                var subject = ReplaceEmailContentParameters(email.Subject, email.Parameters);
                var content = ReplaceEmailContentParameters(email.Content, email.Parameters);

                try
                {
                    await _mailSender.SendMessageAsync(email.Receiver, email.CcReceivers, subject, content, email.Attachments).ConfigureAwait(false);

                    await _repository.MarkMailAsProcessedAsync(email.Id, true, null)
                        .ConfigureAwait(false);
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    var errorMessage =
                        $"Mail with id \"{email.Id}\" could not be sent for email: " + 
                        $"\"{_aesCryptography.Encrypt(email.Receiver)}\" with subject: \"{subject}\"";
                    _log.Error(e, errorMessage);

                    await _repository.MarkMailAsProcessedAsync(email.Id, false,
                        $"{errorMessage}{Environment.NewLine}{e.Message}")
                        .ConfigureAwait(false);
                }
            }
        }

        private static string ReplaceEmailContentParameters(string text, Dictionary<string, string> parameters)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            var stringBuilder = new StringBuilder(text);
            foreach (var param in parameters)
                stringBuilder.Replace($"${{{param.Key}}}", param.Value);

            return stringBuilder.ToString();
        }
    }
}
