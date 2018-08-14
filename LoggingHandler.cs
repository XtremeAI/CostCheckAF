using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace IFX.ResCost
{
    // LoggingHandler derived from http://stackoverflow.com/questions/12300458/web-api-audit-logging
    public class LoggingHandler : DelegatingHandler
    {
        ILogger _log;
        public LoggingHandler(HttpMessageHandler innerHandler, ILogger log)
            : base(innerHandler)
        {
            _log = log;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _log.LogInformation("Request: ");
            _log.LogInformation("{0} {1}", request.Method, request.RequestUri);
            foreach (var header in request.Headers)
            {
                var headerString = string.Join(" ",header.Value);
                _log.LogInformation("{0} : {1}", header.Key, headerString);
            }
            if (request.Content != null)
            {
                _log.LogInformation(await request.Content.ReadAsStringAsync());
            }
            _log.LogInformation("");

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            _log.LogInformation("Response: ");
            _log.LogInformation("{0} {1}", (int)response.StatusCode, response.ReasonPhrase);
            if (response.Content != null)
            {
                _log.LogInformation(await response.Content.ReadAsStringAsync());
            }
            _log.LogInformation("");

            return response;
        }
    }
}