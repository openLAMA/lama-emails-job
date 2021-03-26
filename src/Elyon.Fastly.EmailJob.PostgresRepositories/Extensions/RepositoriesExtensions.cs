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
using Elyon.Fastly.EmailJob.Domain;
using Microsoft.EntityFrameworkCore;

namespace Elyon.Fastly.EmailJob.PostgresRepositories.Extensions
{
    public static class RepositoriesExtensions
    {
        internal static async Task<PagedResults<T>> PaginateAsync<T>(this IQueryable<T> collection, Paginator paginator)
        {
            var count = await collection.CountAsync().ConfigureAwait(false);
            if (count == 0)
            {
                return new PagedResults<T>();
            }

            if (paginator == null)
            {
                paginator = new Paginator();
            }
            
            var totalPages = (int)Math.Ceiling((decimal)count / paginator.PageSize);
            var result = await collection
                .Skip((paginator.CurrentPage - 1) * paginator.PageSize)
                .Take(paginator.PageSize)
                .ToListAsync()
                .ConfigureAwait(false);
            
            return new PagedResults<T>(result, paginator.CurrentPage, paginator.PageSize, totalPages, count);
        }
    }
}
