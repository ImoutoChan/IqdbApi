using System;

namespace IqdbApi.xTests.IqdbApiTestContainer
{
    public class IqdbClientFixture : IDisposable
    {
        private IIqdbClient _api;
        
        public IIqdbClient GetIqdbClient() => _api ?? (_api = new IqdbClient());

        public void Dispose()
        {
            _api?.Dispose();
        }
    }
}