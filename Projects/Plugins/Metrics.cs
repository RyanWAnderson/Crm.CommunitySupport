using System;

namespace Crm.CommunitySupport {
    class Metrics {
        public static TimeSpan TimeAction(Action action) {
            DateTime start = DateTime.UtcNow;
            action();
            return DateTime.UtcNow.Subtract(start);
        }
    }
}
