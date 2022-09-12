using Microsoft.AspNetCore.Mvc.Routing;

namespace Server.Attribute
{
    public sealed class HttpConnectAttribute : HttpMethodAttribute
    {
        private static readonly IEnumerable<string> _supportedMethods = new[] { "CONNECT" };

        public HttpConnectAttribute()
            : base(_supportedMethods)
        {
        }

        public HttpConnectAttribute(string template)
            : base(_supportedMethods, template)
        {
        }
    }
}
