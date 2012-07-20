using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Mefeedia
{
    public static class OAuthHelper
    {
        private static Random random = new Random();
        private const string unreservedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";

        private const string OAuthVersion = "1.0";
        private const string OAuthParameterPrefix = "oauth_";

        private const string OAuthConsumerKeyKey = "oauth_consumer_key";
        private const string OAuthCallbackKey = "oauth_callback";
        private const string OAuthVersionKey = "oauth_version";
        private const string OAuthSignatureMethodKey = "oauth_signature_method";
        private const string OAuthSignatureKey = "oauth_signature";
        private const string OAuthTimestampKey = "oauth_timestamp";
        private const string OAuthNonceKey = "oauth_nonce";
        public const string OAuthTokenKey = "oauth_token";
        public const string OAuthTokenSecretKey = "oauth_token_secret";
        private const string OAuthVerifierKey = "oauth_verifier";
                        
        private const string HMACSHA1SignatureType = "HMAC-SHA1";


        public static string GenerateTimeStamp()
        {
            var now = DateTime.UtcNow;
            TimeSpan ts = now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }

        public static string GenerateNonce()
        {
            // Just a simple implementation of a random number between 123400 and 9999999
            return random.Next(123400, 9999999).ToString();
        }

        private static string UrlEncode(string value)
        {
            var result = new StringBuilder();

            foreach (char symbol in value)
            {
                if (unreservedChars.IndexOf(symbol) != -1)
                {
                    result.Append(symbol);
                }
                else
                {
                    result.Append('%' + String.Format("{0:X2}", (int)symbol));
                }
            }

            return result.ToString();
        }


        public static string GenerateSignature(this OAuthRequest request, string uri)
        {
            if (!string.IsNullOrEmpty(request.Token))
            {
                request.Parameters[OAuthTokenKey] = request.Token;
            }

            if (!string.IsNullOrEmpty(request.VerifierPin))
            {
                request.Parameters[OAuthVerifierKey] = request.VerifierPin;
            }

            request.Parameters[OAuthConsumerKeyKey] = request.ConsumerKey;
            request.Parameters[OAuthVersionKey]=OAuthVersion;
            request.Parameters[OAuthNonceKey]=GenerateNonce();
            request.Parameters[OAuthTimestampKey]=GenerateTimeStamp();
            request.Parameters[OAuthSignatureMethodKey]=HMACSHA1SignatureType;
            request.Parameters[OAuthConsumerKeyKey]=request.ConsumerKey;

            string signatureBase = GenerateSignatureBase(request, uri);

            HMACSHA1 hmacsha1 = new HMACSHA1();
            hmacsha1.Key = Encoding.UTF8.GetBytes(string.Format("{0}&{1}", UrlEncode(request.ConsumerSecret), string.IsNullOrEmpty(request.TokenSecret) ? "" : UrlEncode(request.TokenSecret)));

            var signature = ComputeHash(signatureBase, hmacsha1);
            request.Parameters[OAuthSignatureKey] = UrlEncode(signature);
            return signature;
        }

        public static string GenerateAuthorizationHeader(this OAuthRequest request)
        {
            var paras = new StringBuilder();
            foreach (var param in request.Parameters)
            {
                if (paras.Length > 0) paras.Append(",");
                paras.Append(param.Key + "=\"" + param.Value + "\"");
            }

            return "OAuth " + paras.ToString();
        }

        private static string ComputeHash( string data,HashAlgorithm hashAlgorithm)
        {
            if (hashAlgorithm == null)
            {
                throw new ArgumentNullException("hashAlgorithm");
            }

            if (string.IsNullOrEmpty(data))
            {
                throw new ArgumentNullException("data");
            }

            byte[] dataBuffer = System.Text.Encoding.UTF8.GetBytes(data);
            byte[] hashBytes = hashAlgorithm.ComputeHash(dataBuffer);

            return Convert.ToBase64String(hashBytes);
        }

        public static TValue SafeTryGetValue<TKey,TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue value;
            if (dictionary.TryGetValue(key, out value))
            {
                return value;
            }
            return default(TValue);
        }

        private static List<QueryParameter> GetQueryParameters(string parameters)
        {
            if (parameters.StartsWith("?"))
            {
                parameters = parameters.Remove(0, 1);
            }

            List<QueryParameter> result = new List<QueryParameter>();

            if (!string.IsNullOrEmpty(parameters))
            {
                string[] p = parameters.Split('&');
                foreach (string s in p)
                {
                    if (!string.IsNullOrEmpty(s) && !s.StartsWith(OAuthParameterPrefix))
                    {
                        if (s.IndexOf('=') > -1)
                        {
                            string[] temp = s.Split('=');
                            result.Add(new QueryParameter(temp[0], temp[1]));
                        }
                        else
                        {
                            result.Add(new QueryParameter(s, string.Empty));
                        }
                    }
                }
            }

            return result;
        }

        private static string NormalizeRequestParameters(IList<QueryParameter> parameters)
        {
            StringBuilder sb = new StringBuilder();
            QueryParameter p = null;
            for (int i = 0; i < parameters.Count; i++)
            {
                p = parameters[i];
                sb.AppendFormat("{0}={1}", p.Name, p.Value);

                if (i < parameters.Count - 1)
                {
                    sb.Append("&");
                }
            }

            return sb.ToString();
        }

        public static string GenerateSignatureBase(this OAuthRequest request, string stringuri)
            //Uri url, string httpMethod, string consumerKey, string token, string tokenSecret, string timeStamp, string nonce,
            //                                       string signatureType, out string normalizedUrl, out string normalizedRequestParameters, Dictionary<string, string> paramList)
        {
           
            
                        string normalizedUrl = null;
            string normalizedRequestParameters = null;

            var url = new Uri(stringuri);
            List<QueryParameter> parameters = GetQueryParameters(url.Query);
            parameters.Add(new QueryParameter(OAuthVersionKey, request.Parameters.SafeTryGetValue(OAuthVersionKey)));
            parameters.Add(new QueryParameter(OAuthNonceKey, request.Parameters.SafeTryGetValue(OAuthNonceKey)));
            parameters.Add(new QueryParameter(OAuthTimestampKey, request.Parameters.SafeTryGetValue(OAuthTimestampKey)));
            parameters.Add(new QueryParameter(OAuthSignatureMethodKey,request.Parameters.SafeTryGetValue(OAuthSignatureMethodKey)));
            parameters.Add(new QueryParameter(OAuthConsumerKeyKey, request.Parameters.SafeTryGetValue(OAuthConsumerKeyKey)));

            var token = request.Parameters.SafeTryGetValue(OAuthTokenKey);
            if (!string.IsNullOrEmpty(token))
            {
                parameters.Add(new QueryParameter(OAuthTokenKey, token));
            }

            parameters.Sort(new QueryParameterComparer());

            normalizedUrl = string.Format("{0}://{1}", url.Scheme, url.Host);
            if (!((url.Scheme == "http" && url.Port == 80) || (url.Scheme == "https" && url.Port == 443)))
            {
                normalizedUrl += ":" + url.Port;
            }
            normalizedUrl += url.AbsolutePath;
            normalizedRequestParameters = NormalizeRequestParameters(parameters);

            request.NormalizedUri = normalizedUrl;
            request.NormalizedParameters = normalizedRequestParameters;

            StringBuilder signatureBase = new StringBuilder();
            signatureBase.AppendFormat("{0}&", request.Method.ToUpper());
            signatureBase.AppendFormat("{0}&", UrlEncode(normalizedUrl));
            signatureBase.AppendFormat("{0}", UrlEncode(normalizedRequestParameters));
            
            return signatureBase.ToString();
        }

        public static Dictionary<string, string> ParseKeyValuePairs(this OAuthRequest request, string responseText)
        {
            var responseElements = new Dictionary<string, string>();
            var keypairs = responseText.Split('&');
            foreach (var pair in keypairs)
            {
                var bits = pair.Split('=');
                switch (bits[0])
                {
                    case OAuthTokenKey:
                        request.Token = bits[1];
                        request.AuthorizeUri += "?" + OAuthTokenKey + "=" + request.Token;
                        break;
                    case OAuthTokenSecretKey:
                        request.TokenSecret = bits[1];
                        break;
                    default:
                        responseElements[bits[0]] = bits[1];
                        break;
                }
            }
            return responseElements;
        }
    }


    public class QueryParameter
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public QueryParameter(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }

    }

    /// <summary>
    /// Comparer class used to perform the sorting of the query parameters
    /// </summary>
    public class QueryParameterComparer : IComparer<QueryParameter>
    {
        public int Compare(QueryParameter x, QueryParameter y)
        {
            if (x.Name == y.Name)
            {
                return string.Compare(x.Value, y.Value);
            }
            else
            {
                return string.Compare(x.Name, y.Name);
            }
        }
    }
}
