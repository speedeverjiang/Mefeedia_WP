using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.IO;

namespace Mefeedia.Views.social
{
    public partial class Social : PhoneApplicationPage
    {
        public Social()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(Default_Loaded);
        }

        private OAuthRequest orequest;
        OAuth2 facebook = new OAuth2()
        {
            ClientId = "137764419606259",//"<client id>",
            ClientSecret = "0621c7f765b5a9d8a90bc041f5059b70",//"<client secret>",
            LocalRedirectUrl = "http://www.facebook.com/connect/login_success.html",
            AuthorizeBaseUrl = "https://graph.facebook.com/oauth/authorize",
            AccessTokenBaseUrl = "https://graph.facebook.com/oauth/access_token",
            PageType = "web_server"
        };

        private void Default_Loaded(object sender, RoutedEventArgs e)
        {
            if (NavigationContext.QueryString.Count > 0)
            {
                try
                {
                    string type = NavigationContext.QueryString.Values.First();

                    if (type == "Facebook") 
                    {
                        facebook.Authenticate(this.socialBrowser, AuthComplete);
                    }
                    else if (type == "Twitter")
                    {
                        orequest = new OAuthRequest()
                        {
                            RequestUri = "http://api.twitter.com/oauth/request_token",
                            AuthorizeUri = "http://api.twitter.com/oauth/authorize",
                            AccessUri = "http://api.twitter.com/oauth/access_token",
                            Method = "POST",
                            ConsumerKey = "NvL5YaZLrJ6tV3n4tK33g",
                            ConsumerSecret = "mLcwj2tIV9xkg9riBHawXC4dhzHKZUJe7LhWq8le0"
                        };

                        orequest.Authenticate(this.socialBrowser, AuthenticationComplete);                    
                    }                    
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        //Facebook
        void AuthComplete()
        {
            var request = HttpWebRequest.Create("https://graph.facebook.com/me?access_token=" + facebook.AccessToken);
            request.BeginGetResponse(ProfileResponse, request);

        }

        void ProfileResponse(IAsyncResult result)
        {
            var request = result.AsyncState as HttpWebRequest;
            var response = request.EndGetResponse(result);
            using (var strm = response.GetResponseStream())
            using (var reader = new StreamReader(strm))
            {
                var txt = reader.ReadToEnd();
            }
        }

        //Twitter
        private void AuthenticationComplete(IDictionary<string, string> responseElements)
        {
            this.Dispatcher.BeginInvoke(() =>
            {
                this.socialBrowser.Visibility = System.Windows.Visibility.Collapsed;
            });
        }
    }
}