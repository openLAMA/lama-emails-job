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

using System.Collections.Generic;
using System.Threading.Tasks;
using Elyon.Fastly.EmailJob.RestClient.Models;
using JetBrains.Annotations;
using Refit;

namespace Elyon.Fastly.EmailJob.RestClient.Api
{
    /// <summary>
    /// Storage API interface
    /// </summary>
    [PublicAPI]
    public interface IStorageApi
    {
        /// <summary>
        /// Add file to the storage
        /// </summary>
        /// <param name="file">The file to be added to the storage</param>
        [Post("/api/storage")]
        Task AddFileAsync(FileSpecModel file);

        /// <summary>
        /// Get file from storage
        /// </summary>
        /// <param name="hash">Original xxHash of the file</param>
        /// <returns>The file with the provided hash</returns>
        [Get("/api/storage/{hash}")]
        Task<FileInfoModel> GetFileAsync(string hash);

        /// <summary>
        /// Get all files
        /// </summary>
        /// <returns>All files in storage</returns>
        [Get("/api/storage")]
        Task<List<FileInfoModelWithoutContent>> ListFilesAsync();

        /// <summary>
        /// Delete file in storage
        /// </summary>
        /// <param name="hash">Original xxHash of the file</param>
        [Delete("/api/storage/{hash}")]
        Task DeleteFileAsync(string hash);
    }
}
