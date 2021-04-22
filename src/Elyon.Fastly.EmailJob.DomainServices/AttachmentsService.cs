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
using System.Data.HashFunction.xxHash;
using System.Threading.Tasks;
using Elyon.Fastly.EmailJob.Domain.Enums;
using Elyon.Fastly.EmailJob.Domain.Repositories;
using Elyon.Fastly.EmailJob.Domain.Services;
using Elyon.Fastly.EmailJob.DomainServices.AttachmentFiles;

namespace Elyon.Fastly.EmailJob.DomainServices
{
    public class AttachmentsService : BaseService, IAttachmentsService
    {
        private readonly IAttachmentsRepository _repository;
        private readonly IxxHash _xxHashInstance;

        public AttachmentsService(IAttachmentsRepository repository)
        {
            _repository = repository;
            _xxHashInstance = xxHashFactory.Instance.Create();
        }

        public async Task<List<Guid>> GetAttachmentsIds(string attachmentsType)
        {
            List<Guid> attachmentsIds = new List<Guid>();
            var attachmentsByType = new List<AttachmentData>();
            if (attachmentsType == Enum.GetName(typeof(AttachmentType), AttachmentType.CompanyOnboarding) || 
                attachmentsType == Enum.GetName(typeof(AttachmentType), AttachmentType.SMEOnboarding))
            {
                attachmentsByType = AttachmentsByTypes.CompanyOnboarding();

            }

            foreach (var file in attachmentsByType)
            {
                var fileHash = _xxHashInstance.ComputeHash(file.GetContent()).AsBase64String();
                var attachmentId = await _repository.GetAttachmentIdByHash(fileHash).ConfigureAwait(false);
                if (attachmentId == default(Guid))
                {
                    attachmentId = await _repository.AddAttachment(file.FileName, file.GetContent(), fileHash).ConfigureAwait(false);
                }

                attachmentsIds.Add(attachmentId);
            }

            return attachmentsIds;
        }
    }
}
