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
using System.Threading;
using Autofac;
using Elyon.Fastly.EmailJob.Domain.Services;
using Prime.Sdk.Common;
using Prime.Sdk.Logging;

namespace Elyon.Fastly.EmailJob.PeriodicalHandlers
{
    public sealed class EmailSenderPeriodicHandler : IStartable, IStoppable
    {
        private readonly ILog _log;
        private readonly CancellationTokenSource _tokenSource;
        private readonly IEmailsService _emailService;

        public EmailSenderPeriodicHandler(ILogFactory logFactory, IEmailsService emailService)
        {
            if (logFactory == null)
                throw new ArgumentNullException(nameof(logFactory));
            if (emailService == null)
                throw new ArgumentNullException(nameof(emailService));

            _tokenSource = new CancellationTokenSource();
            _log = logFactory.CreateLog(this);
            _emailService = emailService;
        }

        public void Start()
        {
            var isRunning = false;
            PeriodicTask.Run(async () =>
                {
                    if (isRunning) 
                        return;

                    try
                    {
                        isRunning = true;
                        await _emailService.SendUnsentEmailsAsync()
                            .ConfigureAwait(false);
                    }
#pragma warning disable CA1031 // Do not catch general exception types
                    catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
                    {
                        _log.Error(ex, "Emails in batch could not be sent");
                    }
                    finally
                    {
                        isRunning = false;
                    }
                },
                TimeSpan.FromSeconds(10),
                _tokenSource.Token);
        }

        public void Stop()
        {
            _tokenSource.Cancel();
        }
        
        public void Dispose()
        {
            Stop();
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _tokenSource?.Dispose();
            }
        }

        /// <summary>Allows an object to try to free resources and perform other cleanup operations before it is reclaimed by garbage collection.</summary>
        ~EmailSenderPeriodicHandler()
        {
            Dispose(false);
        }
    }
}