using Application.Resources;
using QueryAny.Primitives;
using ServiceStack;
using ServiceStack.Web;

namespace Api.Common
{
    public static class ResponseExtensions
    {
        public static void SetLocation(this IResponse response, IIdentifiableResource resource)
        {
            SetLocation(response, resource?.Id);
        }

        public static void SetLocation(this IResponse response, string resourceId)
        {
            if (response == null || !resourceId.HasValue())
            {
                return;
            }

            var request = response.Request;
            var value = "{0}/{1}".Fmt(request.AbsoluteUri, resourceId);
            response.AddHeader("Location", value);
        }
    }
}