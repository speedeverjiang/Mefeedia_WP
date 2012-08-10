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
using Microsoft.Phone.Shell;
using System.Xml.Linq;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;
using Hammock;
using Newtonsoft.Json.Linq;

namespace Mefeedia.Views.search
{
    public partial class Default : PhoneApplicationPage
    {
        string MyDevKey = "AI39si7W-9uYNi_PY4lp_ZmBzwyUh3rwBnE9W0W0q5eeulT8wwbmy8oFOA7tPF8eLEFyxRSZqNfIQwvPfeCh9ca877M41gQCjQ";
        PhoneApplicationService appService = PhoneApplicationService.Current;
        string vidUri;
        List<string> Uris = new List<string>();

        public Default()
        {
            InitializeComponent();
        }

        private void tbSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            tbSearch.Text = "";
        }

        private void wc_todo(string Uri)
        {
            WebClient wc1 = new WebClient();
            WebClient wc2 = new WebClient();
            try
            {
                if (!(wc1.IsBusy))
                {
                    wc1.DownloadStringAsync(new Uri(Uri, UriKind.Absolute));
                    wc1.DownloadStringCompleted += new DownloadStringCompletedEventHandler(wc_DownloadStringCompleted);
                }
                else if (!(wc2.IsBusy))
                {
                    wc2.DownloadStringAsync(new Uri(Uri, UriKind.Absolute));
                    wc2.DownloadStringCompleted += new DownloadStringCompletedEventHandler(wc_DownloadStringCompleted);
                }
            }
            catch (WebException ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        public void wc_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs c)
        {
            try
            {
                var o = JObject.Parse(c.Result);
                var videos = from v in o["data"]["items"].Children() select new vData { title = (string)v["title"], description = (string)v["description"], thumbnail = (string)v["thumb"], mobile = (string)v["item_url"] };
                lbResults.ItemsSource = videos;

                App.videolist.Clear();
                foreach (var vid in videos)
                {
                    App.videolist.Add(vid);
                    Uris.Add(vid.mobile);
                }                
            }
            catch (WebException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnDate_Click(object sender, RoutedEventArgs e)
        {            
            wc_todo("http://m.mefeedia.com/search/" + tbSearch.Text + "/json?p=2&rpp=15");
        }

        private void btnRelevance_Click(object sender, RoutedEventArgs e)
        {
            wc_todo("http://m.mefeedia.com/search/" + tbSearch.Text + "/json?p=2&rpp=15");
        }

        private void lbResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbResults.SelectedIndex != -1)
            {
                int index = lbResults.SelectedIndex;
                //vidUri = Uris[lbResults.SelectedIndex].ToString();

                NavigationService.Navigate(new Uri("/Watch?idx=" + index.ToString(), UriKind.Relative));
            }
        }

        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                wc_todo("http://m.mefeedia.com/search/" + tbSearch.Text + "/json?p=2&rpp=15");
            }
        
        }
    }
}