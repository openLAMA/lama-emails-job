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

using System.Threading.Tasks;
using Elyon.Fastly.EmailJob.Domain.Dtos;
using MailKit.Security;
using MimeKit;

namespace Elyon.Fastly.EmailJob.DomainServices.Mail
{
    public class MailSenderService : BaseService, IMailSenderService
    {
        private readonly MailClientFactory _mailClientFactory;
        private readonly MailSettingsDto _mailSettings;

        public MailSenderService(MailClientFactory mailClientFactory, MailSettingsDto mailSettings)
        {
            _mailClientFactory = mailClientFactory;
            _mailSettings = mailSettings;
        }

        public async Task SendMessageAsync(string receiver, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(_mailSettings.Sender));
            message.To.Add(MailboxAddress.Parse(receiver));
            message.Subject = subject;

            message.Body = new TextPart("html")
            {
                Text = body
            };

            using (var mailClient = _mailClientFactory.CreateMailClient())
            {
#pragma warning disable CA5359 // Accept all SSL certificates (in case the server supports STARTTLS)
                mailClient.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
#pragma warning restore CA5359 // Accept all SSL certificates (in case the server supports STARTTLS)

                await mailClient.ConnectAsync(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls)
                    .ConfigureAwait(false);

                if (!string.IsNullOrWhiteSpace(_mailSettings.Username))
                {
                    await mailClient.AuthenticateAsync(_mailSettings.Username, _mailSettings.Password)
                        .ConfigureAwait(false);
                }

                await mailClient.SendAsync(message).ConfigureAwait(false);

                await mailClient.DisconnectAsync(true).ConfigureAwait(false);
            }
        }
    }
}
