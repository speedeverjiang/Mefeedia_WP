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

namespace Mefeedia.Views.player
{
    public partial class Player : PhoneApplicationPage
    {
        public Player()
        {
            InitializeComponent();
            this.Loaded +=new RoutedEventHandler(Player_Loaded);
        }

        private void Player_Loaded(object sender, RoutedEventArgs e)
        { 
            if(NavigationContext.QueryString.Count > 0)
            {
                try
                {
                    String title, url;
                    NavigationContext.QueryString.TryGetValue("title", out title);
                    PageTitle.Text = title;
                    NavigationContext.QueryString.TryGetValue("url", out url);
                    mediaPlayer.Source = new Uri(url, UriKind.RelativeOrAbsolute);
                    mediaPlayer.Position = TimeSpan.FromMilliseconds(0);
                    mediaPlayer.Play();
                }
                catch (Exception ex) 
                {
                    MessageBox.Show(ex.Message);
                }
            
            }
        
        }
    }
}