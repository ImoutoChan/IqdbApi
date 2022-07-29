using System.IO;
using System.Net.Http;

namespace IqdbApi
{
    public class Iqdb3dClient : IqdbClient
    {
        protected override string Url => "3d.iqdb.org";

        public Iqdb3dClient(
            HttpMessageHandler httpMessageHandler = null,
            int waitMilliseconds = 5100,
            bool useHttps = true) : base(httpMessageHandler, waitMilliseconds, useHttps)
        {
        }

        protected override MultipartFormDataContent GetFromDataContent(Stream fileStream)
        {
            var form = new MultipartFormDataContent();
            form.Headers.Add("Origin", "http://3d.iqdb.org");

            form.Add(new StringContent("38333838363038"), "MAX_FILE_SIZE");

            form.Add(new StreamContent(fileStream), "file", "image.jpg");
            form.Add(new StringContent(string.Empty), "url");

            return form;
        }
    }
}
