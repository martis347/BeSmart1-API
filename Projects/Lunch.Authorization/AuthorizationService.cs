using System;
using System.Net.Http;
using Lunch.Domain.Config;
using Microsoft.Extensions.Options;

namespace Lunch.Authorization
{
    public class AuthorizationService: IAuthorizationService
    {
        private string _accessToken;
        private readonly HttpClient _httpClient = new HttpClient();

        public AuthorizationService(IOptions<GoogleConfig> googleConfig)
        {
            _httpClient.BaseAddress = new Uri(googleConfig.Value.AuthenticationUrl);
        }

        public bool Authorize(string token)
        {
            var response = _httpClient.GetAsync($"?access_token={token}").Result;

            if (response.IsSuccessStatusCode)
            {
                _accessToken = token;
            }
            return response.IsSuccessStatusCode;
        }

        public string GetToken()
        {
            return _accessToken;
        }
    }
}