using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace PaymentGateway.Client
{
    public partial class PaymentsClient
    {
        private readonly Func<string> _getJwt;

        public PaymentsClient(string baseUrl, HttpClient httpClient, Func<string> getJwt) : this(baseUrl, httpClient)
        {
            _getJwt = getJwt;
        }

        partial void PrepareRequest(HttpClient client, HttpRequestMessage request, string url)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _getJwt());
        }
    }
}
