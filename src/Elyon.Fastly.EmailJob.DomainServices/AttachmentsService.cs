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
using Elyon.Fastly.EmailJob.Domain.Dtos;
using Elyon.Fastly.EmailJob.Domain.Repositories;
using Elyon.Fastly.EmailJob.Domain.Services;

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

        public async Task<bool> CheckIfFileExists(string content)
        {
            var fileHash = _xxHashInstance.ComputeHash(Convert.FromBase64String(content)).AsBase64String();
            var fileId = await _repository.GetAttachmentIdByHash(fileHash).ConfigureAwait(false);
            return fileId != default(Guid);
        }

        public async Task AddFileAsync(string fileName, string content)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException(nameof(fileName));
            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentNullException(nameof(content));

            var fileContent = Convert.FromBase64String(content);
            var fileHash = _xxHashInstance.ComputeHash(fileContent).AsBase64String();
            await _repository
                .AddAttachment(new InsertFileDto
                { 
                    FileName = fileName,
                    Content = fileContent,
                    OriginalXXHash = fileHash
                })
                .ConfigureAwait(false);
        }

        public async Task<FileInfoDto> GetFileAsync(string hash)
        {
            return await _repository
                .GetFileAsync(hash)
                .ConfigureAwait(false);
        }

        public async Task<List<FileInfoDto>> GetFilesAsync()
        {
            return await _repository
                .GetFilesAsync()
                .ConfigureAwait(false);
        }

        public async Task DeleteFileAsync(string hash)
        {
            await _repository
                .DeleteFileAsync(hash)
                .ConfigureAwait(false);
        }

        public async Task<List<Guid>> GetAttachmentsIds(ICollection<string> attachmentFilesHashes)
        {
            return await _repository
                .GetAttachmentsIds(attachmentFilesHashes)
                .ConfigureAwait(false);
        }
    }
}
