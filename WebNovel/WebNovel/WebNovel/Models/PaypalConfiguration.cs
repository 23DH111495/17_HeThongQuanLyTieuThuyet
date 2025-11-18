using PayPal.Api;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace WebNovel.Models
{
    public static class PaypalConfiguration
    {
        public static APIContext GetAPIContext()
        {
            var config = new Dictionary<string, string>
            {
                { "clientId", ConfigurationManager.AppSettings["PaypalClientId"] },
                { "clientSecret", ConfigurationManager.AppSettings["PaypalSecret"] },
                { "mode", ConfigurationManager.AppSettings["PaypalMode"] }
            };
            var accessToken = new OAuthTokenCredential(config).GetAccessToken();
            return new APIContext(accessToken) { Config = config };
        }
    }
}