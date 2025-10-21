using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace WebNovel.Controllers.Api
{
    public class ImageResult : IHttpActionResult
    {
        private readonly byte[] _imageBytes;
        private readonly string _contentType;

        public ImageResult(byte[] imageBytes, string contentType)
        {
            _imageBytes = imageBytes;
            _contentType = contentType;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(_imageBytes)
            };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(_contentType);
            return Task.FromResult(response);
        }
    }
}