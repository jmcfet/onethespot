using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Threading;
using NLog;
using OnTheSpot.ViewModels;
using OnTheSpot.Models;
using System.Windows.Media.Animation;
using System.IO;

namespace OnTheSpot.Views
{
    /// <summary>
    /// Interaction logic for QSS.xaml
    /// </summary>
    public partial class QSS : UserControl
    {
        //I used the vm as a global class so must check the casting as only one VM is active
        QSSVM vm = null;
        string employeeID ;
        List<string> buttonLabels = new List<string>() { "Spots", "Repairs/Buttons", "Repressing" };
        int BarcodeChars = 0;
        DispatcherTimer timer2 = null;
        bool bClearPressed = false;
        Logger logger = LogManager.GetLogger("BCS");
        BitmapImage bi;

        public QSS()
        {
            InitializeComponent();
            if (((App)Application.Current).vm is QSSVM)
                vm = ((App)Application.Current).vm as QSSVM; ;
            Loaded += QSS_Loaded;
            
  
        }

        void QSS_Loaded(object sender, RoutedEventArgs e)
        {
            int i = 0;
            if (!vm.bLoggedIn)
                Note.Visibility = Visibility.Hidden;
            if (vm.GetShowPass() == 1)
            {
                ShowPass.Content = "Hide pass";
            }
            foreach (object but in buttonsContainer.Children)
            {

                Button button = but as Button;
                if (i < buttonLabels.Count)
                {
                    button.Visibility = Visibility.Visible;
                    button.Content = buttonLabels[i];
                }
                i++;
            }
            if (vm.GetShowPass() == 1)
                PassButton.Visibility = Visibility.Visible;
            timer2 = new DispatcherTimer();
            timer2.Interval = TimeSpan.FromSeconds(1);
            timer2.Tick += new EventHandler(timer2_Tick);
            EmployeeID.Focus();
            Barcode.TextChanged += new TextChangedEventHandler(Barcode_TextChanged);
 
        }
      
        
        /// <summary>
        /// the barcode scanner works by sending all the scanned chars to the element with the input focus at once so
        /// we set a 1 second timer that will read all the chars in that time span and then grab the expected number of
        /// chars as this is a fixed value for the bar code. this way we will correct some of the garbage scans
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Barcode_TextChanged(object sender, TextChangedEventArgs e)
        {
            //if we read first char then start a timer to get the rest as it all comes in a burst
            if (BarcodeChars == 0)
            {
                Debug.WriteLine("Timer start");
                if (bClearPressed)     //if we are manually resetting the barcode then do not start timer
                {
                    bClearPressed = false;
                    return;
                }
                timer2.Start();
                ErrorTxt.Visibility = Visibility.Hidden;
            }
            BarcodeChars++;

        }
        //only this timer is operation now
        void timer2_Tick(object sender, EventArgs e)
        {
            timer2.Stop();
            Item item;
            if (!ProcessBarcode())
                return;
            vm.barcode = Barcode.Text;
            AutoSortInfo assemblyInfo = null;
            //check that the item is in the Assembly database if not then show message and allow more items
            //to be scanned
            try
            {
                assemblyInfo = vm.getItemInAssemblyDB(Barcode.Text);
                if (assemblyInfo == null)
                {
                    BarcodeChars = 0;
                    Errormsg.Text = string.Format(string.Format("Item has not been marked in {0}", Barcode.Text));
                    ErrorTxt.Visibility = Visibility.Visible;
                    return;
                }
                vm.GetCustomer(assemblyInfo.CustomerID);
                item = vm.GetItemInDB(Barcode.Text);
                if (item == null)
                {
                    Errormsg.Text = string.Format(string.Format("item is not in BCS  {0} .. do not QA ", Barcode.Text));
                    ErrorTxt.Visibility = Visibility.Visible;
                    return;
                }
                NoteBox.Visibility = System.Windows.Visibility.Collapsed;
                if (item.Note != null && item.Note != string.Empty)
                {
                    NoteText.Text = item.Note;
                    NoteBox.Visibility = Visibility.Visible;
                }
                DateTime date = (DateTime)assemblyInfo.Duedate;
                DateTime future = DateTime.Now.AddDays(2);
                if (date < future)
                {
                    duedate.Foreground = new SolidColorBrush(Colors.Red);
                    duedate.FontSize = 36;
                }
                if (date.Date <= DateTime.Now.Date)
                    duedate.Text = "TODAY";
                else
                    duedate.Text = date.ToShortDateString();
                vm.Duedate = duedate.Text;
                CustomerName.Text = vm.activeCustomer.FirstName + " " + vm.activeCustomer.LastName;
                //check if this is in Route
                int RFIDlen = assemblyInfo.rfid.Length;
                bool route = false;
                for (int i = 0; i < assemblyInfo.rfid.Length; i++)
                {
                    if (assemblyInfo.rfid[i] != ' ')
                    {
                        route = true;
                        break;
                    }

                }
                if (route)
                    store.Text = vm.store = "Route";
                else
                    store.Text = vm.store = assemblyInfo.storeName;
               
             
            }
            catch (Exception e1)
            {
                BarcodeChars = 0;
                Errormsg.Text = string.Format(string.Format("Database logic error {0} {1}", Barcode.Text, e1.Message));
                ErrorTxt.Visibility = Visibility.Visible;
                return;
            }
            logger.Info(string.Format("all data obtained for {0} ", Barcode.Text));
            //if there is a picture then display it
            if (item.picture == null)
                return;
            picture.Visibility = Visibility.Visible;
            byte [] binaryData = Convert.FromBase64String(item.picture);
            bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = new MemoryStream(binaryData);
            bi.EndInit();
            img.Source = bi;
           
        }

        bool ProcessBarcode()
        {
           
            double itemCode = 0;
  //          Barcode.Text = "1000171823";      //DANGER
            logger.Info("Read bar code " + Barcode.Text);
            if (Barcode.Text == string.Empty)
                return false;
            if (Barcode.Text.Length < 9)
            {
                Errormsg.Text = string.Format("invalid barcode {0}", Barcode.Text);
                ErrorTxt.Visibility = Visibility.Visible;
                Barcode.Text = string.Empty;
                BarcodeChars = 0;
                Barcode.Focus();
                return false;
            }
            
            if (!double.TryParse(Barcode.Text, out itemCode))       //change
            {
              
                Errormsg.Text = string.Format("invalid barcode {0}", Barcode.Text);
                ErrorTxt.Visibility = Visibility.Visible;
                Barcode.Text = string.Empty;
                BarcodeChars = 0;
                Barcode.Focus();
                return false;
            }
            
            return true;
            
        }

        private void textBox1_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            string tt = tb.Text;
        }
        //look for the user enetering their userid
        private void EmployeeID_KeyUp(object sender, KeyEventArgs e)
        {

        if (e.Key != System.Windows.Input.Key.Enter) return;

            int empid;
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            employeeID = EmployeeID.Text;
            if (!Int32.TryParse(employeeID, out empid))
            {
                Errormsg.Text = string.Format("invalid employeeid");
                ErrorTxt.Visibility = Visibility.Visible;
                return;
            }
            Employee employee = vm.GetEmployee(empid);
            vm.Employeename = employee.FirstName + " " + employee.LastName;
            if (employee == null)
            {
                Errormsg.Text = string.Format("invalid employeeid");
                ErrorTxt.Visibility = Visibility.Visible;
                return;
            }
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Arrow;
            UI.Visibility = Visibility.Visible;
            //DoubleAnimation da = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(2));
            //da.AutoReverse = false;
            //UI.BeginAnimation(Line.OpacityProperty, da);

            Login.Visibility = Visibility.Collapsed;
            msg.Text = vm.Employeename + " currently logged in";
            Loggedin.Visibility = Visibility.Visible;
            Note.Visibility = Visibility.Hidden;
            if (employeeID == "1")
            {
                Note.Visibility = Visibility.Visible;
                ShowPass.Visibility = Visibility.Visible;
            }
            Barcode.Focus();
          


        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
           
            Button but = sender as Button;
            vm.reason = but.Content as string;
            string message = vm.saveQCS(Barcode.Text, vm.reason);
            if (message != string.Empty)
            {
                Errormsg.Text = message;
                ErrorTxt.Visibility = Visibility.Visible;
                return;
            }
            print popup = new print();
            popup.ShowDialog();
            Barcode.Text = "";
            Barcode.Focus();
            BarcodeChars = 0;
          

        }

        private void Log_Click(object sender, RoutedEventArgs e)
        {
            Loggedin.Visibility = Visibility.Collapsed;
            Login.Visibility = Visibility.Visible;
            UI.Visibility = Visibility.Collapsed;
            ShowPass.Visibility = Visibility.Collapsed;
        }

        private void Clear_Click_1(object sender, RoutedEventArgs e)
        {
            if (BarcodeChars > 0 )
                bClearPressed = true;
            Barcode.Text = "";
            Barcode.Focus();
            BarcodeChars = 0;
            duedate.Text = " ";
            CustomerName.Text = " ";
            store.Text = " ";
            ErrorTxt.Visibility = Visibility.Collapsed;
            NoteBox.Visibility = Visibility.Collapsed;
            picture.Visibility = Visibility.Collapsed;

        }

        private void Note_Click(object sender, RoutedEventArgs e)
        {
            GetNote popup = new GetNote();
            popup.ShowDialog();
            vm.AddNote(Barcode.Text, vm.Note);
            NoteText.Text = vm.Note;
            NoteBox.Visibility = Visibility.Visible;

        }

        private void Pass_Click(object sender, RoutedEventArgs e)
        {
            string label = ShowPass.Content as string;
            if (label == "Show Pass")
            {
                PassButton.Visibility = Visibility.Visible;
                ShowPass.Content = "Hide Pass";
                vm.SaveShowPass(1);
            }
            else
            {
                ShowPass.Content = "Show Pass";
                PassButton.Visibility = Visibility.Collapsed;
                vm.SaveShowPass(0);
            }

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {

        }
        private ImageSource GetThumbnail(string fileName)
        {
            byte[] buffer = File.ReadAllBytes(fileName);
            MemoryStream memoryStream = new MemoryStream(buffer);

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.DecodePixelWidth = 80;
            bitmap.DecodePixelHeight = 80;
            bitmap.StreamSource = memoryStream;
            bitmap.EndInit();
            bitmap.Freeze();

            return bitmap;
        }

        private void ImgButtonClick(object sender, RoutedEventArgs e)
        {
            ItemImage popup = new ItemImage();
            popup.picture = bi;
            popup.ShowDialog();

        }
    }
}
