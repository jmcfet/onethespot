using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OnTheSpot.Views
{
    /// <summary>
    /// Interaction logic for ItemImage.xaml
    /// </summary>
    public partial class ItemImage : Window
    {
        public BitmapImage picture {get;set;}
        public ItemImage()
        {
            InitializeComponent();
            Loaded += ItemImage_Loaded;
        }

        void ItemImage_Loaded(object sender, RoutedEventArgs e)
        {
            ItemImg.Source = picture;
        }
    }
}
