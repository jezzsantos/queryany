using Common;

namespace Api.Common
{
    public class NullAuditReporter : IAuditReporter
    {
        public void Audit(string eventId, string caller, string message, params object[] args)
        {
        }
    }
}