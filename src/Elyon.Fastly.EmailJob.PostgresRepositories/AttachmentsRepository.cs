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
            return await context.Set<Attachment>()
                .AsNoTracking()
                .Where(a => a.OriginalXXHash == xxHash)
                .Select(a => a.Id)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);
        }

        public async Task<List<Guid>> GetAttachmentsIds(ICollection<string> xxHashes)
        {
            await using var context = ContextFactory.CreateDataContext();
            return await context.Set<Attachment>()
                .AsNoTracking()
                .Where(a => xxHashes.Contains(a.OriginalXXHash))
                .Select(a => a.Id)
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public async Task<string> AddAttachment(InsertFileDto dto)
        {
            await using var context = ContextFactory.CreateDataContext();

            var items = context.Set<Attachment>();
            var entity = Mapper.Map<InsertFileDto, Attachment>(dto);
            entity.Id = Guid.NewGuid();

            await items.AddAsync(entity).ConfigureAwait(false);
            await context.SaveChangesAsync().ConfigureAwait(false);

            context.Entry(entity).State = EntityState.Detached;

            return entity.OriginalXXHash;
        }

        public async Task<FileInfoDto> GetFileAsync(string hash)
        {
            await using var context = ContextFactory.CreateDataContext();
            var entity = await context.Set<Attachment>()
                .AsNoTracking()
                .Where(a => a.OriginalXXHash == hash)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            return Mapper.Map<FileInfoDto>(entity);
        }

        public async Task<List<FileInfoDto>> GetFilesAsync()
        {
            await using var context = ContextFactory.CreateDataContext();
            var entities = await context.Set<Attachment>()
                .AsNoTracking()
                .ToListAsync()
                .ConfigureAwait(false);

            return Mapper.Map<List<FileInfoDto>>(entities);
        }

        public async Task DeleteFileAsync(string hash)
        {
            await using var context = ContextFactory.CreateDataContext();
            var items = context.Set<Attachment>();
            var entity = await items
                .Where(a => a.OriginalXXHash == hash)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            if (entity != null)
            {
                items.Remove(entity);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }
        }
    }
}
