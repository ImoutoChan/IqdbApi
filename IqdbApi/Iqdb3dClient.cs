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
    }
}
