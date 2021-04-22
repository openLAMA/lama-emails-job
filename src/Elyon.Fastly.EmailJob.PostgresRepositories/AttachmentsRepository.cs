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
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Elyon.Fastly.EmailJob.Domain.Repositories;
using Elyon.Fastly.EmailJob.Domain.Services;
using Elyon.Fastly.EmailJob.PostgresRepositories.Entities;
using Microsoft.EntityFrameworkCore;

namespace Elyon.Fastly.EmailJob.PostgresRepositories
{
    public class AttachmentsRepository: BaseRepository, IAttachmentsRepository
    {
        private readonly IAESCryptography _aesCryptography;

        public AttachmentsRepository(
            Prime.Sdk.Db.Common.IDbContextFactory<EmailJobContext> contextFactory,
            IMapper mapper, IAESCryptography aesCryptography)
            : base(contextFactory, mapper)
        {
            _aesCryptography = aesCryptography;
        }

        public async Task<Guid> GetAttachmentIdByHash(string xxHash)
        {
            await using var context = ContextFactory.CreateDataContext();
            var attachmentId = await context.Set<Attachment>()
                .AsNoTracking()
                .Where(a => a.OriginalXXHash == xxHash)
                .Select(a => a.Id)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            return attachmentId;
        }

        public async Task<Guid> AddAttachment(string fileName, byte[] content, string xxHash)
        {
            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName));
            if (!content.Any())
                throw new ArgumentNullException(nameof(content));
            if (xxHash == null)
                throw new ArgumentNullException(nameof(xxHash));

            await using var context = ContextFactory.CreateDataContext();

            var items = context.Set<Attachment>();
            var entity = new Attachment()
            {
                Id = Guid.NewGuid(),
                FileName = fileName,
                Content = _aesCryptography.Encrypt(content),
                OriginalXXHash = xxHash
            };

            await items.AddAsync(entity).ConfigureAwait(false);
            await context.SaveChangesAsync().ConfigureAwait(false);

            context.Entry(entity).State = EntityState.Detached;

            return entity.Id;
        }
    }
}
