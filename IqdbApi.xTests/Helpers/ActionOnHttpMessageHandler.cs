using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using RichardSzalay.MockHttp;

namespace IqdbApi.xTests.Helpers
{
    public class ActionOnHttpMessageHandler : MockHttpMessageHandler
    {
        private Action _actionOnSend;

        public void SetAction(Action action) 
            => _actionOnSend = action;
        
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, 
                                                                     CancellationToken cancellationToken)
        {
            _actionOnSend?.Invoke();

            return await base.SendAsync(request, cancellationToken);
        }
    }
}