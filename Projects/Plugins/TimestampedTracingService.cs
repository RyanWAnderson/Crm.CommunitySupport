using System;
using Microsoft.Xrm.Sdk;

using Crm.CommunitySupport.Extensions;

namespace Crm.CommunitySupport.Plugins {
    /// <summary>
    /// An implementation of ITracingService that prefixes all traced messages with a timestamp and deltas for diagnoising plugin performance/errors.
    /// OOB tracing service usage:
    ///   ITracingService trc = (ITracingService) serviceProvider.GetService(typeof(ITracingService));
    /// TimestampedTracingService usage:
    ///   ITracingService trc = new TimestampedTracingService(serviceProvider);
    /// </summary>
    public partial class TimestampedTracingService : ITracingService, IDisposable {
        #region Constructor(s)
        public TimestampedTracingService(IServiceProvider serviceProvider) {
            // Set private members
            _tracingService = serviceProvider.GetService<ITracingService>();
            _firstTraceTime = _previousTraceTime = DateTime.UtcNow;
        }
        #endregion

        #region ITracingService Implementation
        /// <summary>
        /// Trace a message prefixed with UTC timestamp, execution time and delta
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void Trace(string format, params object[] args) {
            // Establish the timestamp as the entry to this method
            DateTime utcNow = DateTime.UtcNow;

            // Trace the message, prefixed with the current time in UTC sortable format, with deltas
            _tracingService.Trace(
                string.Format(
                    "[{0:O} - @{1:N0}ms (+{2:N0}ms)] - {3}",
                    utcNow,
                    (utcNow - _firstTraceTime).TotalMilliseconds,
                    (utcNow - _previousTraceTime).TotalMilliseconds,
                    string.Format(format, args)
                ));

            // Update the last timestamp
            _previousTraceTime = utcNow;
        }
        #endregion
        #region IDisposable Implementation
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    _tracingService = null;
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose() {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

        // Base ITracingService
        private ITracingService _tracingService;

        // DateTime fields used in calculating deltas
        private DateTime _firstTraceTime;
        private DateTime _previousTraceTime;
    }
}
