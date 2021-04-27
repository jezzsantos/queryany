using Domain.Interfaces;

namespace Api.Common
{
    public class NullMetricReporter : IMetricReporter
    {
        public void Count(string eventId)
        {
        }
    }
}