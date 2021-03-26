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
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Elyon.Fastly.EmailJob.Domain;
using Elyon.Fastly.EmailJob.Domain.Dtos;
using Elyon.Fastly.EmailJob.Domain.Repositories;
using Elyon.Fastly.EmailJob.PostgresRepositories.Entities;
using Elyon.Fastly.EmailJob.PostgresRepositories.Extensions;
using Microsoft.EntityFrameworkCore;
using Prime.Sdk.Db.Common;

namespace Elyon.Fastly.EmailJob.PostgresRepositories
{
    public class BaseCrudRepository<TEntity, TDto> : BaseRepository, IBaseCrudRepository<TDto>
        where TEntity : BaseEntityWithId
        where TDto : BaseDtoWithId
    {
        protected BaseCrudRepository(
            Prime.Sdk.Db.Common.IDbContextFactory<EmailJobContext> contextFactory, IMapper mapper)
            : base(contextFactory, mapper)
        {

        }

        public Task<PagedResults<TDto>> GetListAsync(Paginator paginator, 
            Expression<Func<TDto, bool>> dtoFilter, 
            Expression<Func<TDto, object>> dtoOrderBy, 
            bool isAscending = true)
        {
            return GetListAsync(paginator, dtoFilter, dtoOrderBy, isAscending, null);
        }

        protected virtual async Task<PagedResults<TDto>> GetListAsync(
            Paginator paginator,
            Expression<Func<TDto, bool>> dtoFilter,
            Expression<Func<TDto, object>> dtoOrderBy,
            bool isAscending,
            TransactionContext transactionContext)
        {
            await using var context = ContextFactory.CreateDataContext(transactionContext);

            var query = context.Set<TEntity>().AsNoTracking().AsQueryable();

            if (dtoFilter != null)
            {
                var entityFilter = dtoFilter.ReplaceParameter<TDto, TEntity>();
                query = query.Where(entityFilter);
            }

            if (dtoOrderBy != null)
            {
                var entityOrderBy = dtoOrderBy.ReplaceParameter<TDto, TEntity>();
                query = isAscending
                   ? query.OrderBy(entityOrderBy)
                   : query.OrderByDescending(entityOrderBy);
            }

            return await Mapper.ProjectTo<TDto>(query)
                .PaginateAsync(paginator)
                .ConfigureAwait(false);
        }
        
        public Task<TDto> GetByIdAsync(Guid id)
        {
            return GetByIdAsync(id, null);
        }

        protected virtual async Task<TDto> GetByIdAsync(Guid id, TransactionContext transactionContext)
        {
            await using var context = ContextFactory.CreateDataContext(transactionContext);

            var entity = await context.Set<TEntity>().AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == id)
                .ConfigureAwait(false);
            return Mapper.Map<TEntity, TDto>(entity);
        }

        public Task<TDto> GetAsync(Expression<Func<TDto, bool>> dtoFilter, bool asNoTracking = true)
        {
            return GetAsync(dtoFilter, asNoTracking, null);
        }

        protected virtual async Task<TDto> GetAsync(Expression<Func<TDto, bool>> dtoFilter, 
            bool asNoTracking, TransactionContext transactionContext)
        {
            if (dtoFilter == null)
                throw new ArgumentNullException(nameof(dtoFilter));

            await using var context = ContextFactory.CreateDataContext(transactionContext);
            var items = context.Set<TEntity>();

            var entityFilter = dtoFilter.ReplaceParameter<TDto, TEntity>();
            var queryableItems = asNoTracking ? items.AsNoTracking() : items;
            var item = await queryableItems
                .FirstOrDefaultAsync(entityFilter)
                .ConfigureAwait(false);

            return Mapper.Map<TEntity, TDto>(item);
        }

        public Task<TDto> AddAsync(TDto item)
        {
            return AddAsync(item, null);
        }

        protected virtual async Task<TDto> AddAsync(TDto item, TransactionContext transactionContext)
        {
            await using var context = ContextFactory.CreateDataContext(transactionContext);
            var items = context.Set<TEntity>();

            var entity = Mapper.Map<TDto, TEntity>(item);
            entity.Id = Guid.NewGuid();

            await items.AddAsync(entity).ConfigureAwait(false);

            await context.SaveChangesAsync().ConfigureAwait(false);

            return Mapper.Map<TDto>(entity);
        }

        public Task<TDto> AddAsync<TSpecDto>(TSpecDto item)
        {
            return AddAsync<TSpecDto>(item, null);
        }

        protected virtual async Task<TDto> AddAsync<TSpecDto>(TSpecDto item, 
            TransactionContext transactionContext)
        {
            await using var context = ContextFactory.CreateDataContext(transactionContext);
            var items = context.Set<TEntity>();

            var entity = Mapper.Map<TSpecDto, TEntity>(item);
            entity.Id = Guid.NewGuid();
            
            await items.AddAsync(entity).ConfigureAwait(false);
            await context.SaveChangesAsync().ConfigureAwait(false);

            context.Entry(entity).State = EntityState.Detached;
            
            var result = Mapper.Map<TDto>(entity);
            
            return result;
        }

        public Task UpdateAsync(TDto item)
        {
            return UpdateAsync(item, null);
        }

        protected virtual async Task UpdateAsync(TDto item, TransactionContext transactionContext)
        {
            await using var context = ContextFactory.CreateDataContext(transactionContext);
            var items = context.Set<TEntity>();

            var entity = Mapper.Map<TDto, TEntity>(item);
            context.Entry(entity).State = EntityState.Modified;
            items.Update(entity);

            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        public Task<TDto> SaveAsync(TDto item)
        {
            return SaveAsync(item, null);
        }

        protected virtual async Task<TDto> SaveAsync(TDto item, TransactionContext transactionContext)
        {
            await using var context = ContextFactory.CreateDataContext(transactionContext);
            var items = context.Set<TEntity>();

            if (!await items.AnyAsync(x => x.Id == item.Id).ConfigureAwait(false))
            {
                return await AddAsync(item).ConfigureAwait(false);
            }
            else
            {
                await UpdateAsync(item).ConfigureAwait(false);
                return item;
            }
        }

        public Task DeleteAsync(TDto item)
        {
            return DeleteAsync(item, null);
        }

        protected virtual async Task DeleteAsync(TDto item, TransactionContext transactionContext)
        {
            await using var context = ContextFactory.CreateDataContext(transactionContext);
            var items = context.Set<TEntity>();

            items.Remove(Mapper.Map<TDto, TEntity>(item));

            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        public Task DeleteAsync(Guid id)
        {
            return DeleteAsync(id, null);
        }

        protected virtual async Task DeleteAsync(Guid id, TransactionContext transactionContext)
        {
            await using var context = ContextFactory.CreateDataContext(transactionContext);
            var items = context.Set<TEntity>();

            var entity = await items
                .FirstOrDefaultAsync(item => item.Id == id)
                .ConfigureAwait(false);

            if (entity != null)
            {
                items.Remove(entity);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public Task<bool> AnyAsync(Expression<Func<TDto, bool>> dtoPredicate)
        {
            return AnyAsync(dtoPredicate, null);
        }

        protected virtual async Task<bool> AnyAsync(Expression<Func<TDto, bool>> dtoPredicate,
            TransactionContext transactionContext)
        {
            await using var context = ContextFactory.CreateDataContext(transactionContext);
            var items = context.Set<TEntity>();

            var entityPredicate = dtoPredicate.ReplaceParameter<TDto, TEntity>();
            return await items.AnyAsync(entityPredicate).ConfigureAwait(false);
        }
    }
}