using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Extension.Tests.Utilities
{
    public class FakeHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var suffix = request?.RequestUri?.AbsolutePath;

            if (suffix is null)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest));
            }
            suffix = suffix.Replace("/", "\\");
            suffix = suffix.Remove(0, 1);
            var prefix = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var filePath = Path.Combine(prefix, suffix);
            if (!File.Exists(filePath))
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
            }

            var rawData = File.ReadAllBytesAsync(filePath, cancellationToken);
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(rawData.Result)
            };

            return Task.FromResult(response);
        }
    }
}
