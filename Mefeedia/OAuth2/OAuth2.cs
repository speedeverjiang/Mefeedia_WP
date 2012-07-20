using System;
using System.Net;
using Microsoft.Phone.Controls;

namespace Mefeedia
{
    public class OAuth2
{
        private const string ClientIdKey = "client_id";
        private const string ClientSecretKey = "client_secret";
        private const string RedirectUriKey = "redirect_uri";
        private const string TypeKey = "type";
        private const string CodeKey = "code";
        private const string AccessTokenKey = "access_token";
        private const string ExpiresKey = "expires";

        public string LocalRedirectUrl { get; set; }
        public string AuthorizeBaseUrl { get; set; }
        public string AccessTokenBaseUrl { get; set; }
        public string PageType { get; set; }

        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        //private const string LocalRedirectUrl = "http://www.facebook.com/connect/login_success.html";
        //private const string AuthorizeBaseUrl = "https://graph.facebook.com/oauth/authorize";
        //private const string AccessTokenBaseUrl = "https://graph.facebook.com/oauth/access_token";
        //private const string Type_WebServer = "web_server";

        private const string AuthorizeUrl = "{0}?" +
                                                 ClientIdKey + "={1}&" +
                                                 RedirectUriKey + "={2}&" +
                                                 TypeKey + "={3}";

        private const string AccessTokenUrl = "{0}?" +
                                                 ClientIdKey + "={1}&" +
                                                 RedirectUriKey + "={2}&" +
                                                 TypeKey + "={3}&" +
                                                 ClientSecretKey + "={4}&" +
                                                 CodeKey + "={5}";


        public string Code { get; set; }
        public string AccessToken { get; set; }
        public string Expires { get; set; }


        public void Authenticate(WebBrowser browser, Action callback)
        {
            browser.Navigated += (s, e) =>
            {
                if (e.Uri.AbsoluteUri.ToLower().StartsWith(LocalRedirectUrl))
                {
                    ExtractCode(e.Uri);

                    var accessTokenUrl = string.Format(AccessTokenUrl, AccessTokenBaseUrl, ClientId,LocalRedirectUrl,PageType, ClientSecret, Code);
                    browser.Navigate(new Uri(accessTokenUrl));
                }
                else if (e.Uri.AbsoluteUri.ToLower().StartsWith(AccessTokenBaseUrl))
                {
                    var contents = browser.SaveToString();
                    ExtractAccessToken(contents);
                    callback();
                }

            };

            var authorizeUrl = string.Format(AuthorizeUrl,AuthorizeBaseUrl, ClientId, LocalRedirectUrl,PageType);
            browser.Navigate(new Uri(authorizeUrl));
        }

        private void ExtractCode(Uri uri)
        {
            var code = uri.Query.Trim('?');
            var bits = code.Split('=');
            this.Code = bits[1];
        }

        private void ExtractAccessToken(string browserContents)
        {
            browserContents = HttpUtility.HtmlDecode(browserContents);
            var start = browserContents.IndexOf("access_token");
            var end = browserContents.IndexOf("</PRE>");
            var paramlist = browserContents.Substring(start, end - start);
            foreach (var param in paramlist.Split('&'))
            {
                var bits = param.Split('=');
                switch (bits[0])
                {
                    case AccessTokenKey:
                        this.AccessToken = bits[1];
                        break;
                    case ExpiresKey:
                        this.Expires = bits[1];
                        break;

                }
            }
        }

 
    }
}
