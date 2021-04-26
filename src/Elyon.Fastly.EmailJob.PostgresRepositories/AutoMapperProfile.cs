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
using Elyon.Fastly.EmailJob.Domain.Dtos;
using Elyon.Fastly.EmailJob.Domain.Services;
using Elyon.Fastly.EmailJob.PostgresRepositories.Entities;

namespace Elyon.Fastly.EmailJob.PostgresRepositories
{
    public class AutoMapperProfile : Profile
    {
        private readonly IAESCryptography _aESCryptography;

        public AutoMapperProfile(IAESCryptography aESCryptography)
        {
            _aESCryptography = aESCryptography;

            CreateMap<EmailTemplate, EmailTemplateDto>();
            CreateMap<EmailTemplateDto, EmailTemplate>()
                .ForMember(source => source.Emails, opt => opt.Ignore());

            CreateMap<InsertFileDto, Attachment>()
                .ForMember(dest => dest.Content, opt => opt.MapFrom(src => _aESCryptography.Encrypt(src.Content)));
            CreateMap<Attachment, FileInfoDto>()
                .ForMember(dest => dest.FileHash, opt => opt.MapFrom(src => src.OriginalXXHash))
                .ForMember(dest => dest.Content, opt => opt.MapFrom(src => Convert.ToBase64String(_aESCryptography.Decrypt(src.Content))));
        }
    }
}
