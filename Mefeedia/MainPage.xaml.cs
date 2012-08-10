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
using System.Collections.ObjectModel;

using Microsoft.Phone.Shell;
using System.Xml.Linq;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;
using Hammock;
using Newtonsoft.Json.Linq;

namespace Mefeedia
{
    public partial class MainPage : PhoneApplicationPage
    {
        PhoneApplicationService appService = PhoneApplicationService.Current;
        string vidUri;
        List<string> Uris = new List<string>();
        int selIndex;


        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Set the data context of the listbox control to the sample data
            DataContext = App.ViewModel;
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
        }

        // Load data for the ViewModel Item
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
            }
        }

        //select menu list item
        private void MenuListBox_SelectionChanged(object sender, SelectionChangedEventArgs args)
        { 
            //if selected index is -1(no selection) do nothing
            selIndex = menuListBox.SelectedIndex;
            if (selIndex == -1)
                return;

            //Navigate to the new page
            
            if(selIndex < 2)
            {
                mainPanorama.DefaultItem = mainPanorama.Items[selIndex + 1];
            }
            else if (selIndex == 2)//search
            {
                NavigationService.Navigate(new Uri("/Search", UriKind.Relative));
            }
            else 
            {
                mainPanorama.DefaultItem = mainPanorama.Items[selIndex];
            }

            if (selIndex == 0) //watch
            {   
                string uri = "http://www.mefeedia.com/api/mobile/?action=get_videos&u=" + App.deviceID + "&p="+App.pageNo.ToString()+"&rpp=15&c=63&conn=wifi&d=winphone";
                wc_todo(uri);
            }
            else if (selIndex == 1) //watch later
            {
                List<vData> laterVideos = new List<vData>();
                laterVideos.Clear();
                Uris.Clear();

                // Read items
                int count = IsolatedStorageHelper.GetObject<int>("WatchLaterItemCount");
                for (int i = 0; i < count; i++) 
                {
                    vData item = new vData();
                    item.id = IsolatedStorageHelper.GetObject<string>("id" + i);
                    item.title = IsolatedStorageHelper.GetObject<string>("title" + i);
                    item.description = IsolatedStorageHelper.GetObject<string>("description" + i);
                    if (item.description.Length > 200)
                    {
                        item.description = item.description.Substring(0, 200);
                        item.description += "...";
                    }
                    item.thumbnail = IsolatedStorageHelper.GetObject<string>("thumb" + i);
                    item.friendlydate = IsolatedStorageHelper.GetObject<string>("friendlyDate" + i);
                    item.mobile = IsolatedStorageHelper.GetObject<string>("mobile" + i);
                    laterVideos.Add(item);
                    Uris.Add(item.mobile);
                }
                lbWatchLater.ItemsSource = laterVideos;

            }
            else if (selIndex == 4) //stats
            {
                string uri = "http://www.mefeedia.com/api/mobile/?action=stats&u=" + App.deviceID;
                wc_todo(uri);
                //int videosWatched = IsolatedStorageHelper.GetObject<int>("watchCount");
                //tbVideos.Text = videosWatched.ToString();
            }


            //Reset selected index to -1(no selection)
            //menuListBox.SelectedIndex = -1;
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
                if(selIndex == 0)//watch
                {
                    string tmp = c.Result;
                    int f = tmp.IndexOf("[");
                    string tmpData = "{\"items\":"+c.Result.Substring(f)+"}";

                    //var o = JObject.Parse(c.Result);
                    var o = JObject.Parse(tmpData);
                    var videos = from v in o["items"].Children() 
                                 select new vData 
                                { 
                                    id = (string)v["globalid"],
                                    title = (string)v["nice_title"], 
                                    description = (string)v["nice_text"], 
                                    thumbnail = (string)v["selected_thumb_url_320x240"], 
                                    mobile = (string)v["url"], 
                                    friendlydate = (string)v["friendlydate"] 
                                };

                    lbWatch.ItemsSource = videos;

                    App.videolist.Clear();
                    Uris.Clear();
                    foreach (var vid in videos)
                    {
                        App.videolist.Add(vid);
                        Uris.Add(vid.mobile);
                    }
                }
                else if (selIndex == 4)//stats
                {
                    var o = JObject.Parse(c.Result);
                    tbTime.Text = (string)o["total_duration"];
                    tbVideos.Text = (string)o["videos_watched"];
                    tbLikes.Text = (string)o["likes"];
                    int ranking = (int)o["global_rank"];
                    tbRanking.Text = ranking.ToString();
                }
            }
            catch(WebException ex) 
            {
                MessageBox.Show(ex.Message);
            }
        }

        //Application Bar click handler
        private void AppBarIconSearch_Click(object sender, EventArgs e)       
        {
            NavigationService.Navigate(new Uri("/Search", UriKind.Relative));
        }

        private void AppBarIconSetting_Click(object sender, EventArgs e)
        {
            mainPanorama.DefaultItem = mainPanorama.Items[3];
        }

        /*Settings Panorama Item*/

        //popular
        private void tsPopular_Checked(object sender, RoutedEventArgs e)
        {
            bool popularSel = true;
            MessageBox.Show(tsPopular.IsChecked.ToString());
        }

        private void tsPopular_Unchecked(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(tsPopular.IsChecked.ToString());
        }

        //watch list item clicked
        private void lbWatch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selIndex = lbWatch.SelectedIndex;
            //uri
            vidUri = Uris[selIndex].ToString();     

            //
            NavigationService.Navigate(new Uri("/Watch?idx=" + selIndex.ToString(), UriKind.Relative));
        }

        //watch later list item clicked
        private void lbWatchLater_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selIndex = lbWatchLater.SelectedIndex;
            String id = IsolatedStorageHelper.GetObject<string>("id" + selIndex);
            String title = IsolatedStorageHelper.GetObject<string>("title" + selIndex);
            String vidUrl = IsolatedStorageHelper.GetObject<string>("mobile" + selIndex);
            String escapedVidUrl = Uri.EscapeDataString(vidUrl);
            //get youtube id from the vidUrl
            if (vidUrl != "")
            {
                int youtubeidPos = vidUrl.IndexOf("?v=");
                if (youtubeidPos < 0)
                    youtubeidPos = vidUrl.IndexOf("&v=");
                if (youtubeidPos >= 0)
                {
                    String subTemp = vidUrl.Substring(youtubeidPos + 3);
                    int nextparamPos = subTemp.IndexOf("&");
                    if (nextparamPos >= 0)
                    {
                        String youtubeid = subTemp.Substring(0, nextparamPos);
                        id = youtubeid;
                    }
                    else
                    {
                        id = subTemp;
                    }
                }
            }
            //..
            NavigationService.Navigate(new Uri("/Player?id=" + id + "&title=" + title + "&url=" + escapedVidUrl, UriKind.Relative));
            //NavigationService.Navigate(new Uri("/Watch?idx=" + selIndex.ToString(), UriKind.Relative));
        }

    }
}