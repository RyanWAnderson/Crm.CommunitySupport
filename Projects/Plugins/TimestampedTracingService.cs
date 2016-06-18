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
    public partial class TimestampedTracingService : ITracingService {
        #region Constructor(s)
        public TimestampedTracingService(IServiceProvider serviceProvider) {
            // Set private members
            _tracingService = serviceProvider.GetService<ITracingService>();
            _firstTraceTime = _previousTraceTime = DateTime.UtcNow;
            Trace("TimestampedTracingService initialized.");
        }
        #endregion

        #region ITracingService Implementation
        /// <summary>
        /// Trace a message prefixed with UTC timestamp, execution time and delta
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void Trace(string format, params object[] args) {
            DateTime utcNow = DateTime.UtcNow;

            TraceMessageWithTimestampAndDeltas(utcNow, format, args);
            SaveTimestamp(utcNow);
        }
        #endregion
        
        /// <summary>
        /// Trace the message, prefixed with the current time in UTC sortable format, with deltas
        /// </summary>
        private void TraceMessageWithTimestampAndDeltas(DateTime now, string format, object[] args) {
            _tracingService.Trace(
                string.Format(
                    "[{0:O} - @{1:N0}ms (+{2:N0}ms)] - {3}",
                    now,
                    (now - _firstTraceTime).TotalMilliseconds,
                    (now - _previousTraceTime).TotalMilliseconds,
                    string.Format(format, args)
                ));
        }

        private void SaveTimestamp(DateTime now) {
            _previousTraceTime = now;
        }

        // Base ITracingService
        private ITracingService _tracingService;

        // DateTime fields used in calculating deltas
        private DateTime _firstTraceTime;
        private DateTime _previousTraceTime;
    }
}
