using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using System.Windows.Resources;

namespace Mefeedia
{
    public class Utils
    {
        public static BitmapImage GetImage(string filename)
        {
            string imgLocation = Application.Current.Resources["ImagesLocation"].ToString();

            StreamResourceInfo imageResource = Application.GetResourceStream(new Uri(imgLocation + filename, UriKind.Relative));
            BitmapImage image = new BitmapImage();
            image.SetSource(imageResource.Stream);

            return image;
        }

    }
}
