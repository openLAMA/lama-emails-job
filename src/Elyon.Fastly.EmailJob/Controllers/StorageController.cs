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
using System.Threading.Tasks;
using AutoMapper;
using Elyon.Fastly.EmailJob.Domain.Services;
using Elyon.Fastly.EmailJob.RestClient.Api;
using Elyon.Fastly.EmailJob.RestClient.Models;
using Microsoft.AspNetCore.Mvc;

namespace Elyon.Fastly.EmailJob.Controllers
{
    [Route("api/storage")]
    [ApiController]
    public class StorageController : ControllerBase, IStorageApi
    {
        private readonly IAttachmentsService _storageService;
        private readonly IMapper _mapper;

        public StorageController(IAttachmentsService storageService, IMapper mapper)
        {
            _storageService = storageService;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<string> AddFileAsync(FileSpecModel file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            return await _storageService
            .AddFileAsync(file.FileName, file.Content)
            .ConfigureAwait(false);
        }

        [HttpGet("{hash}")]
        public async Task<FileInfoModel> GetFileAsync(string hash)
        {
            if (string.IsNullOrEmpty(hash))
                throw new ArgumentNullException(nameof(hash));

            var fileDto = await _storageService
                .GetFileAsync(hash)
                .ConfigureAwait(false);

            if (fileDto != default)
            {
                return _mapper.Map<FileInfoModel>(fileDto);
            }

            return null;
        }

        [HttpGet]
        public async Task<List<FileInfoModelWithoutContent>> ListFilesAsync()
        {
            var filesDtos = await _storageService
                .GetFilesAsync()
                .ConfigureAwait(false);

            return _mapper.Map<List<FileInfoModelWithoutContent>>(filesDtos);
        }

        [HttpDelete("{hash}")]
        public async Task DeleteFileAsync(string hash)
        {
            if (string.IsNullOrEmpty(hash))
                throw new ArgumentNullException(nameof(hash));

            await _storageService
                .DeleteFileAsync(hash)
                .ConfigureAwait(false);
        }
    }
}
