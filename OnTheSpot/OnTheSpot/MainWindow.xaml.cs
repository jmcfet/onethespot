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
using OnTheSpot.Views;
using System.Windows.Threading;
using System.Configuration;
using NLog;
using OnTheSpot.ViewModels;
using System.Printing;

namespace OnTheSpot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        RegisterItem registerView = null;
        QSS qssView = null;
        BaseViewModel vm = ((App)Application.Current).vm;
        DispatcherTimer PhigTimer;
        string StoreConnectionString;
        string AssemblyConnectionString;
        Logger logger = LogManager.GetLogger("BCS");
        public MainWindow()
        {
            InitializeComponent();
            
            Loaded += new RoutedEventHandler(MainWindow_Loaded);

        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (((App)Application.Current).bQSS)
                this.Title = "On the Spot QCS";
            else if (((App)Application.Current).bBCS)
            {
                this.Title = "On the Spot BCS";
                passwordBox1.Visibility = Visibility.Visible;
                label4.Visibility = Visibility.Visible;
                label1.Visibility = Visibility.Visible;
                passwordEntered.Visibility = Visibility.Visible;
                led1.Visibility = Visibility.Visible;
            }
            else
            {
                this.Title = "On the Spot GSS";
                label1.Visibility = Visibility.Visible;
                led1.Visibility = Visibility.Visible;
            }
            vm.PhidgitController = new Phidgets.InterfaceKit();
            vm.bControllerOn = false;
            logger.Info("BCS start");

            vm.PhidgitController.Attach += new Phidgets.Events.AttachEventHandler(PhidgitController_Attach);
            vm.PhidgitController.Detach += new Phidgets.Events.DetachEventHandler(PhidgitController_Detach);
            PhigTimer = new DispatcherTimer();
            PhigTimer.Interval = TimeSpan.FromSeconds(5);
            PhigTimer.Tick += new EventHandler(timer_Tick);
            PhigTimer.Start();
            vm.PhidgitController.open();
            DataContext = vm;
            ConnectionStringSettingsCollection connections = ConfigurationManager.ConnectionStrings;
            StoreConnectionString = connections["Store1Entities"].ConnectionString;
            AssemblyConnectionString = connections["AssemblyEntities"].ConnectionString;

        }

        //for the system to be totally operational we must get a response from the controller and
        //have database connectivity. If we have just database working then allow degraded operation
        void timer_Tick(object sender, EventArgs e)
        {
            //phidgit controller might be offline in which case warn the user and check database
            //operation as we can allow partial operation if the DB is operational
            logger.Info("timer_Tick");
            PhigTimer.IsEnabled = false;
            CheckSystemState(false);



        }

        //phidgit controller has signalled that it is good to go
        void PhidgitController_Attach(object sender, Phidgets.Events.AttachEventArgs e)
        {
            logger.Info("PhidgitController_Attach");
            PhigTimer.IsEnabled = false;
            CheckSystemState(true);

        }

        void PhidgitController_Detach(object sender, Phidgets.Events.DetachEventArgs e)
        {
            logger.Info("PhidgitController_Detach");
            vm.bControllerOn = false;
            //            if (vm.errormsg == string.Empty)

        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Stores_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //ConnectionStringSettingsCollection connections = ConfigurationManager.ConnectionStrings;
            //string MyConnectionString = connections[].ConnectionString;




        }

       

        void CheckSystemState(bool PhidgitState)
        {
            logger.Info("CheckSystemState start");
            if (PhidgitState || vm.bSimulatePhigetsMode)
            {
                this.Dispatcher.Invoke(new Action(delegate()    //use dispatcher to update UI
                {
                    textBlock1.Text = "Phidget operational .. waiting for database connection";
                    vm.bControllerOn = true;
                    if (!vm.bSimulatePhigetsMode)
                    {
                        led1.ColorOn = Colors.Green;
                        led1.IsActive = true;
                    }
                    //               label1.Background = Brushes.Green;
                    DBErrorMsg.Text = string.Empty;


                }));
            }
            else
            {

                DBErrorMsg.Text = "Phidgit controller not operational";
                textBlock1.Text = "System Initialization Failed";
                led1.ColorOn = Colors.Red;
                led1.IsActive = true;

            }


            vm.OpenDB(StoreConnectionString);
            vm.OpenAssemblyDB(AssemblyConnectionString);
            vm.GetOurEntities();   //operates on background thread


            this.Dispatcher.Invoke(new Action(delegate()     //use dispatcher to update UI
            {
                if (vm.DBerrormsg == string.Empty)
                {
                    led2.ColorOn = Colors.Green;
                    led2.IsActive = true;
                    vm.bTurnOnRegister = true;
                    textBlock1.Text = "System Initialization Complete";
                    if (led1.ColorOn == Colors.Green || vm.bSimulatePhigetsMode)
                    {
                        if (((App)Application.Current).bQSS)
                        {
                            qssView = new QSS();
                            Views.Children.Add(qssView);
                        }
                        else
                        {
                            registerView = new RegisterItem();
                            Views.Children.Add(registerView);
                        }
                    }
                }
                else
                {
                    DBErrorMsg.Text = vm.DBerrormsg;
                    textBlock1.Text = "System Initialization Failed";
                    led2.ColorOn = Colors.Red;
                    led2.IsActive = true;
                    vm.bTurnOnRegister = false;
                }


            }));
            logger.Info("CheckSystemState end");

        }

        private void button1_Click_1(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 8; i++)
                vm.PhidgitController.outputs[i] = false;
        }

        private void passwordEntered_Click(object sender, RoutedEventArgs e)
        {
            string pass = passwordBox1.Password;
            passwordBox1.Password = string.Empty;
            if (pass == "tennis")
            {
                vm.bLoggedIn = true;
                vm.ClassifyButtonText = "Admin Off";
                vm.ReClassifyButtonText = "ReClassify On";
                vm.QuickReClassifyButtonText = "Quick reclassify On";
                QuickReClassify.Visibility = Visibility.Visible;
                Classify.Visibility = Visibility.Visible;
                if (registerView != null)
                {
                    ReClassify.Visibility = Visibility.Visible;

                    registerView.SetFocusBarcode();
                }
            }

        }

        private void version_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("On The Spot BCS version 1.6 May 12 2014");
        }

        private void Classify_Click(object sender, RoutedEventArgs e)
        {
            vm.bLoggedIn = false;
            ReClassify.Visibility = Visibility.Collapsed;
            Classify.Visibility = Visibility.Collapsed;
            QuickReClassify.Visibility = Visibility.Collapsed;
        }
        //allows admin person to reclassify without BCS feedback
        private void QickClassify_Click(object sender, RoutedEventArgs e)
        {

            if (vm.QuickReClassifyButtonText == "Quick Reclassify Off")
                vm.QuickReClassifyButtonText = "Quick Reclassify On";
            else
                vm.QuickReClassifyButtonText = "Quick Reclassify Off";
            if (vm.bSimulatePhigetsMode == false)
            {
                vm.bSimulatePhigetsMode = true;
                if (registerView == null)
                {
                    registerView = new RegisterItem();
                    Views.Children.Add(registerView);
                    if (vm.bLoggedIn)
                    {
                        ReClassify.Visibility = Visibility.Visible;
                        Classify.Visibility = Visibility.Visible;
                    }
                }
            }
            registerView.SetFocusBarcode();

        }
        //let an admin change the items classification
        private void Reclassify_click(object sender, RoutedEventArgs e)
        {
            vm.ReClassifyButtonText = "ReClassify Off";
            registerView.SetFocusBarcode();

        }
    }
}

