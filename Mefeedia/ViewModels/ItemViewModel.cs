using System;
using System.ComponentModel;
using System.Diagnostics;
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

namespace Mefeedia
{
    public class ItemViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Sample ViewModel property; this property is used in the view to display its value using a Binding.
        /// </summary>
        /// <returns></returns>
        
        /// <returns></returns>
        
        ///
        private BitmapImage _Icon;
        public BitmapImage Icon
        {
            get { return _Icon; }
            set
            {
                _Icon = value;
                NotifyPropertyChanged("Icon");
            }
        }

        /// 
        private string _Itemname;
        public string Itemname
        {
            get { return _Itemname; }
            set
            {
                _Itemname = value;
                NotifyPropertyChanged("ItemName");
            }
        }       

        private void NotifyPropertyChanged(string propertyName)
        {
            if (null != PropertyChanged)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}