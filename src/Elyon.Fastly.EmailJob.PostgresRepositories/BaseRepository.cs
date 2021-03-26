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
using AutoMapper;
using Elyon.Fastly.EmailJob.Domain.Repositories;
using Prime.Sdk.PostgreSql;

namespace Elyon.Fastly.EmailJob.PostgresRepositories
{
    public abstract class BaseRepository : IBaseRepository
    {
        protected Prime.Sdk.Db.Common.IDbContextFactory<EmailJobContext> ContextFactory { get; }
        protected IMapper Mapper { get; }

        protected BaseRepository(Prime.Sdk.Db.Common.IDbContextFactory<EmailJobContext> contextFactory, IMapper mapper)
        {
            if (contextFactory == null)
                throw new ArgumentNullException(nameof(contextFactory));
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));

            ContextFactory = contextFactory;
            Mapper = mapper;
        }
    }
}