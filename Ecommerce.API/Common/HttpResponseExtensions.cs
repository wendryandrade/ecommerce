using Microsoft.AspNetCore.Http;
using System.Net;

namespace Ecommerce.API.Common
{
    public static class HttpResponseExtensions
    {
        private const string HeaderName = "X-Message";

        public static void AddSuccessMessage(this HttpResponse? response, string message)
        {
            if (response is null) return;
            if (string.IsNullOrWhiteSpace(message)) return;

            // Headers must be ASCII only. Encode non-ASCII characters safely.
            var sanitized = message.Replace("\r", " ").Replace("\n", " ").Trim();
            var encoded = WebUtility.UrlEncode(sanitized);

            response.Headers[HeaderName] = encoded;
        }
    }
}
