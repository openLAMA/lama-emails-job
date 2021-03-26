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
using System.Linq.Expressions;
using System.Threading.Tasks;
using Elyon.Fastly.EmailJob.Domain;
using Elyon.Fastly.EmailJob.Domain.Dtos;
using Elyon.Fastly.EmailJob.Domain.Repositories;
using Elyon.Fastly.EmailJob.Domain.Services;

namespace Elyon.Fastly.EmailJob.DomainServices
{
    public class BaseCrudService<TDto> : BaseService, IBaseCrudService<TDto>
        where TDto : BaseDtoWithId
    {
        protected IBaseCrudRepository<TDto> Repository { get; }

        public BaseCrudService(IBaseCrudRepository<TDto> repository)
        {
            Repository = repository;
        }

        public virtual async Task<PagedResults<TDto>> GetListAsync(
            Paginator paginator,
            Expression<Func<TDto, bool>> filter = null,
            Expression<Func<TDto, object>> orderBy = null,
            bool ascending = true)
        {
            return await Repository
                .GetListAsync(paginator, filter, orderBy, ascending)
                .ConfigureAwait(false);
        }

        public virtual async Task<TDto> GetByIdAsync(Guid id)
        {
            return await Repository
                .GetByIdAsync(id)
                .ConfigureAwait(false);
        }

        public virtual async Task<TDto> GetAsync(Expression<Func<TDto, bool>> dtoFilter, bool asNoTracking = true)
        {
            return await Repository
                .GetAsync(dtoFilter, asNoTracking)
                .ConfigureAwait(false);
        }

        public virtual async Task<TDto> AddAsync(TDto item)
        {
            return await Repository
                .AddAsync(item)
                .ConfigureAwait(false);
        }

        public virtual async Task<TDto> AddAsync<TSpecDto>(TSpecDto item)
        {
            return await Repository
                .AddAsync<TSpecDto>(item)
                .ConfigureAwait(false);
        }

        public virtual async Task<TDto> SaveAsync(TDto item)
        {
            return await Repository
                .SaveAsync(item)
                .ConfigureAwait(false);
        }

        public virtual async Task UpdateAsync(TDto item)
        {
            await Repository
                .UpdateAsync(item)
                .ConfigureAwait(false);
        }

        public virtual async Task DeleteAsync(TDto item)
        {
            await Repository
                .DeleteAsync(item)
                .ConfigureAwait(false);
        }

        public virtual async Task DeleteAsync(Guid id)
        {
            await Repository
                .DeleteAsync(id)
                .ConfigureAwait(false);
        }

        public virtual async Task<bool> AnyAsync(Expression<Func<TDto, bool>> predicate)
        {
            return await Repository
                .AnyAsync(predicate)
                .ConfigureAwait(false);
        }
    }
}
