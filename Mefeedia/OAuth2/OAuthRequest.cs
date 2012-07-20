using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Microsoft.Phone.Controls;

namespace Mefeedia
{

    public class OAuthRequest
    {
        public string RequestUri { get; set; }
        public string AuthorizeUri { get; set; }
        public string AccessUri { get; set; }

        public string Method { get; set; }

        public string NormalizedUri { get; set; }
        public string NormalizedParameters { get; set; }

        public string VerifierPin { get; set; }

        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }
        public string Token { get; set; }
        public string TokenSecret { get; set; }

        public IDictionary<string, string> Parameters { get; private set; }


        public OAuthRequest()
        {
            Parameters = new Dictionary<string, string>();
        }

        public void Authenticate(WebBrowser browser, Action<IDictionary<string, string>> callback)
        {
            var baseAuthorizeUri = this.AuthorizeUri;
            browser.Navigated += (s, e) =>
            {
                if (e.Uri.AbsoluteUri.ToLower().StartsWith(baseAuthorizeUri))
                {
                    if (!e.Uri.Query.Contains("oauth_token"))
                    {
                        var htmlString = browser.SaveToString();

                        var authPinName = "oauth_pin>";
                        var startDiv = htmlString.IndexOf(authPinName) + authPinName.Length; // eg <DIV id=oauth_pin>4697728</DIV></DIV>
                        var endDiv = htmlString.IndexOf("<", startDiv);
                        var pin = htmlString.Substring(startDiv, endDiv - startDiv);

                        this.VerifierPin = pin;
                        this.RetrieveAccessToken(callback);
                    }
                }
            };

            // Step 1: Retrieve Request Token
            RetrieveRequestToken(() =>
            {
                browser.Dispatcher.BeginInvoke(() =>
                {
                    browser.Navigate(new Uri(this.AuthorizeUri));
                });
            });
        }


        public void RetrieveRequestToken(Action Callback)
        {
            this.GenerateSignature(this.RequestUri);

            var request = HttpWebRequest.Create(this.NormalizedUri);
            request.Method = this.Method;
            request.Headers[HttpRequestHeader.Authorization] = this.GenerateAuthorizationHeader();
            
            request.BeginGetResponse((result) =>
            {
                var req = result.AsyncState as HttpWebRequest;
                var resp = request.EndGetResponse(result) as HttpWebResponse;

                using (var strm = resp.GetResponseStream())
                using (var reader = new StreamReader(strm))
                {
                    var responseText = reader.ReadToEnd();
                    this.ParseKeyValuePairs(responseText);
                }

                Callback();

            }, request);

        }


        public void RetrieveAccessToken(Action<IDictionary<string, string>> Callback)
        {


            this.GenerateSignature(this.AccessUri);

            var request = HttpWebRequest.Create(this.NormalizedUri);
            request.Method = this.Method;
            request.Headers[HttpRequestHeader.Authorization] = this.GenerateAuthorizationHeader();
            //request.Headers["Authorization"] = "OAuth " + paras.ToString();
            request.BeginGetResponse((result) =>
            {
                var req = result.AsyncState as HttpWebRequest;
                var resp = request.EndGetResponse(result) as HttpWebResponse;

                Dictionary<string, string> responseElements;

                using (var strm = resp.GetResponseStream())
                using (var reader = new StreamReader(strm))
                {
                    var responseText = reader.ReadToEnd();
                    responseElements = this.ParseKeyValuePairs(responseText);
                }

                Callback(responseElements);

            }, request);

        }
    }
}
