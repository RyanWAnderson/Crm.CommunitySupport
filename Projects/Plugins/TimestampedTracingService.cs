// =====================================================================
//  This file is part of the Microsoft CRM SDK Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  This source code is intended only as a supplement to Microsoft
//  Development Tools and/or on-line documentation.  See these other
//  materials for detailed information regarding Microsoft code samples.
//
//  THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
//  PARTICULAR PURPOSE.
// =====================================================================

using Microsoft.Xrm.Sdk;

namespace Microsoft.Crm.Sdk.Samples {
    /// <summary>
    /// An implementation of ITracingService that prefixes all traced messages with a timestamp and time deltas for diagnoising plugin performance issues.
    /// Out-of-box tracing service usage:
    ///    ITracingService trc = (ITracingService) serviceProvider.GetService(typeof(ITracingService));
    /// TimestampedTracingService usage:
    ///    ITracingService trc = new TimestampedTracingService(serviceProvider);
    /// </summary>
    class TimestampedTracingService : ITracingService {
        /// <summary>
        /// Create a new TimestampedTracingService relying on Xrm services from the IServiceProvider.
        /// </summary>
        /// <param name="serviceProvider">The IServiceProvider passed into a plugin's Execute() method.</param>
        public TimestampedTracingService(IServiceProvider serviceProvider) {
            var utcNow = DateTime.UtcNow;

            // Get the initial timestamp from the IExecutionContext
            var context = (IExecutionContext)serviceProvider.GetService( typeof( IExecutionContext ) );
            var initialTimestamp = context.OperationCreatedOn;

            // Ensure the inititalTimestamp is not in the future (since servers may not be exactly in sync)
            if (initialTimestamp > utcNow) {
                initialTimestamp = utcNow;
            }

            // Set private members
            _tracingService = (ITracingService)serviceProvider.GetService( typeof( ITracingService ) );
            _firstTraceTime = _previousTraceTime = initialTimestamp;

            // Trace a starting message
            Trace( "TimestampedTracingService initialized." );
        }

        #region ITracingService support
        /// <summary>
        /// Trace a formatted message prefixed with UTC timestamp, overall duration and delta since last trace
        /// </summary>
        public void Trace(string format, params object[] args) {
            var utcNow = DateTime.UtcNow;

            _tracingService.Trace(
                "[{0:O} - @{1:N0}ms (+{2:N0}ms)] - {3}",
                utcNow,
                (utcNow - _firstTraceTime).TotalMilliseconds,
                (utcNow - _previousTraceTime).TotalMilliseconds,
                string.Format( format, args )
            );

            _previousTraceTime = utcNow;
        }
        #endregion

        // Base ITracingService
        private ITracingService _tracingService;

        // DateTime fields used in calculating deltas
        private DateTime _firstTraceTime;
        private DateTime _previousTraceTime;
    }
}
