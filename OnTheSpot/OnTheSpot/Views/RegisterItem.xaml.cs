using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OnTheSpot.ViewModels;
using OnTheSpot.Models;
using Phidgets;
using System.Diagnostics;
using System.Windows.Threading;
using NLog;


namespace OnTheSpot.Views
{
    /// <summary>
    /// Interaction logic for RegisterItem.xaml
    /// </summary>
    /// 
    //The GSS has almost the same functionality as BCS except in how the bin is selected so I use the same BCSVM
    public partial class RegisterItem : UserControl
    {
        BCSandGSSVM vm = null;
        Logger logger = LogManager.GetLogger("BCS");
        
        int BarcodeChars = 0;
        int ButtonHeight = 100;
        int ButtonWidth = 140;
        DispatcherTimer timer2 = null;
        AutoSortInfo assemblyInfo = null;
        bool bNextDay = false;
               
        public RegisterItem()
        {
            InitializeComponent();
     
            vm = ((App)Application.Current).vm as BCSandGSSVM;
            
            DataContext = ((App)Application.Current).vm;
           
            timer2 = new DispatcherTimer();
            timer2.Interval = TimeSpan.FromSeconds(1);
            timer2.Tick += new EventHandler(timer2_Tick);
            vm.bReceivedAlready = false;
            
            Loaded += new RoutedEventHandler(RegisterItem_Loaded);
        }
       
        

        void RegisterItem_Loaded(object sender, RoutedEventArgs e)
        {
            
            TopGrid.Visibility = Visibility.Visible;
            Barcode.Focus();
            turnoffLights();           
            Barcode.TextChanged += new TextChangedEventHandler(Barcode_TextChanged);
            vm.PhidgitController.InputChange += new Phidgets.Events.InputChangeEventHandler(PhidgitController_InputChange);
            vm.PhidgitController.SensorChange += new Phidgets.Events.SensorChangeEventHandler(PhidgitController_SensorChange);
//            vm.PhidgitController.OutputChange += new Phidgets.Events.OutputChangeEventHandler(PhidgitController_OutputChange);
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
           
            if (BarcodeChars == 0)
            {
                Debug.WriteLine("Timer start");

                timer2.Start();
                ErrorTxt.Visibility = Visibility.Hidden;
            }
            BarcodeChars++;
         
        }
        Bin TargetBin = null;
        string CleaningCat = string.Empty;
        Item item = null;
        //only this timer is operation now
        void timer2_Tick(object sender, EventArgs e)
        {

            timer2.Stop();
            if (!ProcessBarcode())
                return;
            string barcode = Barcode.Text;
           
            //check that the item is in the Assembly database if not then show message and allow more items
            //to be scanned
            try
            {
                assemblyInfo = vm.getItemInAssemblyDB(barcode);
                if (assemblyInfo == null)        
                {
                    BarcodeChars = 0;
                    Errormsg.Text = string.Format(string.Format("Item has not been marked in {0}", barcode));
                    ErrorTxt.Visibility = Visibility.Visible;
                    return;
                }
                vm.GetCustomer(assemblyInfo.CustomerID);
                CustomerName.Text = vm.activeCustomer.FirstName + " " + vm.activeCustomer.LastName;
                if (vm.BCSMode)
                {
                    //check if the item is in BCS database
                    item = vm.GetItemInDB(barcode);
                }
               
               
            }
            catch (Exception e1)
            {
                BarcodeChars = 0;
                Errormsg.Text = string.Format(string.Format("Database logic error {0} {1}", barcode, e1.Message));
                ErrorTxt.Visibility = Visibility.Visible;
                return;
            }
            logger.Info(string.Format("all data obtained for {0} ", barcode));
            if (!vm.BCSMode)
            {
                SetupActions();
                return;
            }
            
            bool route = false;
            for (int i = 0; i < assemblyInfo.rfid.Length; i++)
            {
                if (assemblyInfo.rfid[i] != ' ')
                {
                    route = true;
                    break;
                }

            }
            if (!route)     //only look for next day if this is not a routed customer
                bNextDay = vm.getNextdayBin(assemblyInfo);

            if (bNextDay)
            {
                ButRow1.Children.Clear();
                ButRow1.Visibility = Visibility.Visible;
                logger.Info("new item");
                item = new Item() { BarCode = barcode, CustID = 0, ID = -1, CreationDate = DateTime.Now };
                Button but = new Button() { Content = "Next Day Work", Height = ButtonHeight, Width = ButtonWidth, Visibility = Visibility.Visible };
                but.Margin = new Thickness(0, 5, 5, 5);
   //             but.Click += new RoutedEventHandler(but_Click);
                item.Category = vm.CleaningCats.Where(c => c.ID == 11).Single();
                ButRow1.Children.Add(but);
                SetupActions();
                return;
            }
            //a new item set the category to Unclassified or laundry
            if (item == null)     
            {
                
                ButRow1.Children.Clear();
                ButRow1.Visibility = Visibility.Visible;
                logger.Info("new item");
                item = new Item() { BarCode = barcode,  CustID = 0, ID = -1, CreationDate = DateTime.Now  };
                Button but = new Button() { Content = "Unclassified Work", Height = ButtonHeight, Width = ButtonWidth, Visibility = Visibility.Visible };
                but.Margin = new Thickness(0, 5, 5, 5);
                but.Click += new RoutedEventHandler(but_Click);
                if (assemblyInfo.dept != "Laundry")     //must be unclassifield dry cleaning
                {
                    logger.Info("dry cleaning");
                    item.Category = vm.CleaningCats.Where(c => c.Name.StartsWith("Unclassified")).Single();
                }
                else
                {
                    logger.Info("laundry");
                    if (vm.activeCustomer.Starch == "H" || vm.activeCustomer.Starch == "M")
                    {
                        but.Content = "Starched Shirts";

                    }
                    else
                    {
                        but.Content = "No Starch Shirts";

                    }
                    //get category so we can place in the right bin
                    item.Category = vm.CleaningCats.Where(c => c.Name == but.Content.ToString()).Single();
                }
                ButRow1.Children.Add(but);
                SetupActions();
           
            }
            else
            {
                //known item but first check if it is unknown as it needs to be categorized before proceeding
                ButRow1.Children.Clear();
                ButRow2.Children.Clear();
                ButRow1.Visibility = Visibility.Visible;
                if (item.Category.ID == vm.unknownCat.ID || vm.ReClassifyButtonText == "ReClassify Off" || vm.QuickReClassifyButtonText == "Quick Reclassify Off")
                {
                    logger.Info("Classify");

                    vm.ReClassifyButtonText = "ReClassify On";    //this is a one time deal
                   
                    
                    if (!vm.bLoggedIn)
                    {
                        Errormsg.Text = "Classify is a adminstrator action";
                        ErrorTxt.Visibility = Visibility.Visible;
                        Barcode.Text = string.Empty;
                        BarcodeChars = 0;
                        return;
                    }
                    ButRow2.Visibility = Visibility.Visible;
                    int ii = 0;
                    foreach (Category cat in vm.CleaningCats)
                    {
                        if (cat.ID == vm.unknownCat.ID)
                            continue;
                        ii++;
                        Button but = new Button() { Content = cat.Name, Height = ButtonHeight, Width = ButtonWidth, Visibility = Visibility.Visible };
                        but.Margin = new Thickness(0, 5, 5, 5);

                        but.Click += new RoutedEventHandler(but_Click);
                        if (ii < 5)
                            ButRow1.Children.Add(but);
                        else
                            ButRow2.Children.Add(but);
                    }
                }
                else    //allow the garment to be placed in a bin
                {
                    Button but = new Button() {  Height = ButtonHeight, Width = ButtonWidth, Visibility = Visibility.Visible, IsEnabled = false };
                    if (assemblyInfo.dept != "Laundry")     //if this is  dry cleaning
                    {
                        Category cat = vm.CleaningCats.Where(c => c.ID == item.Category.ID).SingleOrDefault();
                        but.Content = cat.Name;
                    }
                    else    //laundry then get the starch from the customer info as they might have changed in fabricare
                    {
                        if (vm.activeCustomer.Starch == "H" || vm.activeCustomer.Starch == "M")
                           but.Content = "Starched Shirts";

                        else
                           but.Content = "No Starch Shirts";
                        //set category so we can place in the right bin in fabricare setting
                        item.Category = vm.CleaningCats.Where(c => c.Name == but.Content.ToString()).Single();
                    }
                    ButRow1.Children.Add(but);
                    SetupActions();
                   
                   
                }
            }

            

        }

        void but_Click(object sender, RoutedEventArgs e)
        {
            
            //Button but = sender as Button;
            //Brush old = but.Background;
            //but.Background = new SolidColorBrush(Colors.Black);
            string catName = (string)(sender as Button).Content;
            item.Category = vm.CleaningCats.Where(c => c.Name == catName).Single();
            if (vm.QuickReClassifyButtonText == "Quick Reclassify Off")    //if in QuickClassy mode then do not wait for feedback
            {
                vm.SaveItem(item);
                ClearItemFields();
                CustomerName.Text = "";

                vm.QuickReClassifyButtonText = "Quick Reclassify On";
                return;
            }
            SetupActions();
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
           
  //          Action.Text = "Place in Lighted bin";
   //         Weight.Visibility = Visibility.Hidden;
            //turn on the right light
            vm.PhidgitController.outputs[TargetBin.PhidgetSlot] = true;
            vm.bReceivedAlready = false;
            Wizard.Visibility = Visibility.Hidden;
        }

        void ClearAllFields()
        {
            grid1.Visibility = Visibility.Hidden;
            TopGrid.Visibility = Visibility.Hidden;
            Barcode.Text = "";
            ButRow1.Visibility = Visibility.Collapsed;
            ButRow2.Visibility = Visibility.Collapsed;
            Barcode.Focus();
            BarcodeChars = 0;
  //          CatsCombo.Visibility = Visibility.Hidden;
            
           
        }
        void ClearItemFields()
        {
            grid1.Visibility = Visibility.Hidden;
            ButRow1.Visibility = Visibility.Collapsed;
            ButRow2.Visibility = Visibility.Collapsed;
            Barcode.Text = "";
            BarcodeChars = 0;     //reenable the timer for reading barcode
            Barcode.Focus();
           
        }
        void SetupActions()
        {
            
            grid1.Visibility = Visibility.Visible;
            Category   cat = item != null ? item.Category : null;
            if (vm.BCSMode)
                TargetBin = vm.getBin(cat, assemblyInfo);
            else
                TargetBin = vm.getBinGSS(cat, assemblyInfo);
            if (TargetBin == null)
            {
                if (cat != null)
                    MessageBox.Show(string.Format("Bin not found for category {0}", item.Category.Name));
                else
                    MessageBox.Show(string.Format("Bin not found for customer {0} Call Manager!", assemblyInfo.CustomerID));
                return;
            }
            Action.Text = "Place in Lighted bin";
            logger.Info("turn on bin light" + TargetBin.PhidgetSlot.ToString());
            if (vm.bSimulatePhigetsMode == false)
            {
                turnoffLights();    //in case on is in the wrong state
                vm.PhidgitController.outputs[TargetBin.PhidgetSlot] = true;
            }
            vm.bReceivedAlready = false;
            Wizard.Visibility = Visibility.Hidden;
            if (vm.BCSMode)
            {
                if (item.Category.ID != 11)     //do not save next day info as next time the item might NOT be next day
                    vm.SaveItem(item);
            }
            else
            {
                GSS gssItem = new GSS() { CreationDate = DateTime.Now, bin = TargetBin.PhidgetSlot, BarCode = TargetBin.BarCode };
                vm.SaveItemGSS(gssItem);
            }
        }


        private void button3_Click(object sender, RoutedEventArgs e)
        {
            vm.PhidgitController.outputs[TargetBin.PhidgetSlot] = false;
        }

    

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox outputChk = (CheckBox)sender;

            int outputIndex = int.Parse((string)outputChk.Tag);
            if ((bool)outputChk.IsChecked)
                vm.PhidgitController.outputs[outputIndex] = true;
            else
                vm.PhidgitController.outputs[outputIndex] = false;
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            ClearAllFields();

        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            ClearAllFields();

        }

        
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            //check if the light was turned on
            if (!vm.bSimulatePhigetsMode)
            {
                if (vm.PhidgitController.outputs[TargetBin.PhidgetSlot] == true)
                    vm.PhidgitController.outputs[TargetBin.PhidgetSlot] = false;
            }
            ClearItemFields();
        }
        void PhidgitController_SensorChange(object sender, Phidgets.Events.SensorChangeEventArgs e)
        {
            Debug.WriteLine("sensor change {0}", e.Index);
        }

        //a piece of laundry has been dropped into a bin, the index tells us which one
        void PhidgitController_InputChange(object sender, Phidgets.Events.InputChangeEventArgs e)
        {
            logger.Info(string.Format("input change {0}", e.Index));

            int slot = e.Index;
            if (TargetBin == null)
            {
                this.Dispatcher.Invoke(new Action(delegate()
                {
                    ClearItemFields();
                    Errormsg.Text = "Target slot unknown  ... Please rescan item";
                    ErrorTxt.Visibility = Visibility.Visible;

                }));
                return;

            }
            //is item in the right slot
            if (slot != TargetBin.PhidgetSlot)
            {

                this.Dispatcher.Invoke(new Action(delegate()
                {

                    Errormsg.Text = "Please place item in right bin";
                    ErrorTxt.Visibility = Visibility.Visible;
                }));
            }
            else
                this.Dispatcher.Invoke(new Action(delegate()
                {
                    //this event is fired multiple times 
                    if (vm.bReceivedAlready == false)
                    {
                        ErrorTxt.Visibility = Visibility.Hidden;
                        logger.Info("write possibile changes to database");
                        if (vm.BCSMode)
                        {
                            if (item.Category.ID != 11)     //do not save next day info as next time the item might NOT be next day)
                                vm.SaveItem(item);
                        }
                        else
                        {
                            GSS gssItem = new GSS() { CreationDate = DateTime.Now, bin = TargetBin.PhidgetSlot, BarCode = TargetBin.BarCode };
                            vm.SaveItemGSS(gssItem);
                        }
                        ClearItemFields();
                        CustomerName.Text = "";

                    }
                    vm.PhidgitController.outputs[TargetBin.PhidgetSlot] = false;
                    vm.bReceivedAlready = true;

                }));

        }

        void PhidgitController_Attach(object sender, Phidgets.Events.AttachEventArgs e)
        {
            Debug.WriteLine("Attach {0}", e.Device.Name);
        }


        private void turnoffLights()
        {
            if (vm.PhidgitController.outputs.Count > 0)
            {
                for (int i = 0; i < 8; i++)
                    vm.PhidgitController.outputs[i] = false;
            }
        }

        private void button2_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void Classify_Click(object sender, RoutedEventArgs e)
        {
            vm.bLoggedIn = false;
        }

        private void Cleartext_Click(object sender, RoutedEventArgs e)
        {
            Barcode.Text = "";
            Barcode.Focus();
            BarcodeChars = 0;

        }
        public void SetFocusBarcode()
        {
  //          Window myCurrentWindow = Window.GetWindow(this);
  //          myCurrentWindow.Activate();
            Keyboard.Focus(Barcode);

        }

        bool ProcessBarcode()
        {

            double itemCode = 0;
            Barcode.Text = "1000181348";      //DANGER
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
       
        


       
    }
}
