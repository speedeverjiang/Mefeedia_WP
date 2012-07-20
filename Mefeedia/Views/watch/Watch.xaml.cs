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
using System.Windows.Media.Imaging;
using Microsoft.Phone.Shell;
using System.Xml.Linq;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;
using Hammock;
using Newtonsoft.Json.Linq;

namespace Mefeedia.Views.watch
{
    public partial class Default : PhoneApplicationPage
    {
        int curVidIndex;
        int curPageNo;

        public Default()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(Default_Loaded);
        }

        private void Default_Loaded(object sender, RoutedEventArgs e)
        { 
            if(NavigationContext.QueryString.Count > 0)
            {
                try {
                    curPageNo = App.pageNo;
                    string index = NavigationContext.QueryString.Values.First();
                    curVidIndex = Convert.ToInt16(index);

                    displayVideoInfo();
                    //Media.Source = new Uri(Url, UriKind.RelativeOrAbsolute);
                    //Media.Position = TimeSpan.FromMilliseconds(0);
                    //Media.Play();

                    //int videosWatched = IsolatedStorageHelper.GetObject<int>("watchCount");
                    //IsolatedStorageHelper.SaveObject("watchCount", ++videosWatched);
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }            
            }
        }

        private void displayVideoInfo()
        {
            vData curVideo = App.videolist[curVidIndex];
            
            // Set like button status
            string vidUrl = curVideo.mobile;
            bool liked = IsolatedStorageHelper.GetObject<bool>(vidUrl);
            if (liked == true)
            {
                btnLike.IsEnabled = false;
            }
            
            // Set other UI elements
            Uri uri = new Uri(curVideo.thumbnail);
            imgThumb.Source = new BitmapImage(uri);
            tbDescription.Text = curVideo.title;
            tbFriendlyDate.Text = curVideo.friendlydate;        
        }

        //Button event handler
        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if (curVidIndex == 0)
            {
                btnBack.IsEnabled = true;
            }

            if (curVidIndex == App.videolist.Count - 1)
            {
                curPageNo++;
                string uri = "http://www.mefeedia.com/api/mobile/?action=get_videos&u=" + App.deviceID + "&p="+curPageNo.ToString()+"&rpp=15&c=63&conn=wifi&d=winphone";
                wc_todo(uri);
            }
            else
            {
                curVidIndex++;
                displayVideoInfo();
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            if (curVidIndex == 0)
            {
                if (curPageNo > 1)
                {
                    curPageNo--;
                    string uri = "http://www.mefeedia.com/api/mobile/?action=get_videos&u=" + App.deviceID + "&p=" + curPageNo.ToString() + "&rpp=15&c=63&conn=wifi&d=winphone";
                    wc_todo(uri);
                }
                else
                {
                    btnBack.IsEnabled = false;
                }
            }
            else 
            {
                curVidIndex--;
                displayVideoInfo();
            }
        }

        //Download
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
                string tmpData = "{\"items\":" + c.Result + "}";

                //var o = JObject.Parse(c.Result);
                var o = JObject.Parse(tmpData);
                var videos = from v in o["items"].Children()
                             select new vData
                             {
                                 title = (string)v["nice_title"],
                                 description = (string)v["nice_text"],
                                 thumbnail = (string)v["selected_thumb_url_320x240"],
                                 mobile = (string)v["url"],
                                 friendlydate = (string)v["friendlydate"]
                             };

                App.videolist.Clear();
                foreach (var vid in videos)
                {
                    App.videolist.Add(vid);
                }

                if (curPageNo > App.pageNo)
                {
                    curVidIndex = 0; //NEXT
                }
                else 
                {
                    curVidIndex = App.videolist.Count-1;
                }
                App.pageNo = curPageNo;
                displayVideoInfo();
            }
            catch (WebException ex)
            {
                curPageNo = App.pageNo;
                MessageBox.Show(ex.Message);
            }
        }

        //facebook
        private void btnFacebook_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Social?type=Facebook", UriKind.Relative));            
        }

        //twitter
        private void btnTwitter_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Social?type=Twitter", UriKind.Relative));
        }

        //video play
        private void imgThumb_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            String title = App.videolist[curVidIndex].title;
            String vidUrl = App.videolist[curVidIndex].mobile;
            NavigationService.Navigate(new Uri("/Player?title="+ title + "&url=" + vidUrl, UriKind.Relative));
        }

        //watch later
        private void btnWatchLater_Click(object sender, RoutedEventArgs e)
        {
            int count = IsolatedStorageHelper.GetObject<int>("WatchLaterItemCount");
            IsolatedStorageHelper.SaveObject("WatchLaterItemCount", count++);

            vData curVideo = App.videolist[curVidIndex];
            String title = curVideo.title;
            String description = curVideo.description;
            String thumb = curVideo.thumbnail;
            String friendlyDate = curVideo.friendlydate;
            String vidUrl = curVideo.mobile;

            string strIndex = count.ToString();
            IsolatedStorageHelper.SaveObject("title"+strIndex, title);
            IsolatedStorageHelper.SaveObject("description" + strIndex, description);
            IsolatedStorageHelper.SaveObject("thumb" + strIndex, thumb);
            IsolatedStorageHelper.SaveObject("friendlyDate" + strIndex, friendlyDate);
            IsolatedStorageHelper.SaveObject("mobile" + strIndex, vidUrl);
        }

        //like
        private void btnLike_Click(object sender, RoutedEventArgs e)
        {
            String vidUrl = App.videolist[curVidIndex].mobile;
            bool liked = IsolatedStorageHelper.GetObject<bool>(vidUrl);
            if (liked == false)
            {
                IsolatedStorageHelper.SaveObject(vidUrl, true);
            }

            // change like button status
            btnLike.IsEnabled = false;
        }

       
    }
}