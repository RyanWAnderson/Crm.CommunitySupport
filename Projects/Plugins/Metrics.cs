namespace Crm.CommunitySupport
{
    using System;

    internal class Metrics
    {
        public static TimeSpan TimeAction(Action action)
        {
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            action();
            watch.Stop();
            return watch.Elapsed;
        }
    }
}
