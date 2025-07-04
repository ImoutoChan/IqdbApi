using System;

namespace IqdbApi.xTests
{
    public class IqdbClientFixture : IDisposable
    {
        private IIqdbClient _api;
        
        public IIqdbClient GetIqdbClient()
        {
            if (_api == null)
            {
                var client = new IqdbClient();
                client.ConfigureHttpClient(x => x.Timeout = TimeSpan.FromSeconds(1000));
                _api = client;
            }

            return _api;
        }

        public void Dispose() => _api?.Dispose();
    }
}
