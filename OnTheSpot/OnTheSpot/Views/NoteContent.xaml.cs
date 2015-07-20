using OnTheSpot.ViewModels;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OnTheSpot.Views
{
    /// <summary>
    /// Interaction logic for NoteContent.xaml
    /// </summary>
   
    public partial class GetNote : Window
    {
        QSSVM vm = null;
        public GetNote()
        {
            InitializeComponent();
            vm = ((App)Application.Current).vm as QSSVM; ;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            vm.Note = NoteText.Text;
            this.Close();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            this.Close();
            
        }
    }
}
