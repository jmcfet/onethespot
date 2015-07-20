using System;
using System.Linq;
using System.Text;
using OnTheSpot.Models;

using System.Collections.ObjectModel;
using OnTheSpot.Dal;
using Phidgets;
//using SendKeys;
//using System.Windows.Forms;
using System.Configuration;
using System.Collections.Generic;
using NLog;
using System.ComponentModel;
using System.Windows;

namespace OnTheSpot.ViewModels
{
    
    public class BaseViewModel:INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected DBAccess db;
        AssemblyAccess AssemblyDB;
        public  InterfaceKit PhidgitController;
        public bool bControllerOn = false;
        public string WindowID;
        private bool _bTurnOnRegister = true;
        private bool _bLoggedIn = false;

        public bool bSimulatePhigetsMode = false;     
        Logger logger = LogManager.GetLogger("OnTheSpot");
        public bool bTurnOnRegister
        {
            get { return _bTurnOnRegister; }
            set
            {
                if (_bTurnOnRegister != value)
                {
                    _bTurnOnRegister = value;
                    NotifyPropertyChanged("bTurnOnRegister");
                }
            }
        }
        public bool bLoggedIn
        {
            get { return _bLoggedIn; }
            set
            {
                if (_bLoggedIn != value)
                {
                    _bLoggedIn = value;
                    NotifyPropertyChanged("bLoggedIn");
                }
            }
        }
        private Bin  _bin;
        public Bin bin
        {
            get { return _bin; }
            set
            {
                if (_bin != value)
                {
                    _bin = value;
                    NotifyPropertyChanged("bin");
                }
            }
        }
        private string _ClassifyButtonText;
        public string ClassifyButtonText
        {
            get { return _ClassifyButtonText; }
            set
            {
                if (_ClassifyButtonText != value)
                {
                    _ClassifyButtonText = value;
                    NotifyPropertyChanged("ClassifyButtonText");
                }
            }
        }
        private string _ReClassifyButtonText;
        public string ReClassifyButtonText
        {
            get { return _ReClassifyButtonText; }
            set
            {
                if (_ReClassifyButtonText != value)
                {
                    _ReClassifyButtonText = value;
                    NotifyPropertyChanged("ReClassifyButtonText");
                }
            }
        }
        private string _QuickReClassifyButtonText;
        public string QuickReClassifyButtonText
        {
            get { return _QuickReClassifyButtonText; }
            set
            {
                if (_QuickReClassifyButtonText != value)
                {
                    _QuickReClassifyButtonText = value;
                    NotifyPropertyChanged("QuickReClassifyButtonText");
                }
            }
        }
        public Category cat = null;
        public bool bReceivedAlready = false;
        public ObservableCollection<Bin> CleaningBins{ get; set; }
        public ObservableCollection<Item> Items { get; set; }
        public ObservableCollection<Category> CleaningCats { get; set; }
        public ObservableCollection<string> stores { get; set; }
        public Customer activeCustomer { get; set; }
        public Category unknownCat;
        public string DBerrormsg = string.Empty;
        public BaseViewModel()
        {
           
            stores = new ObservableCollection<string>();

            stores.Add("store1");
            stores.Add("store2");
            stores.Add("store3");
            stores.Add("store4");
            

        }
        public bool bShowWorkorder = false;

        public void OpenDB(string connectionString)
        {

            logger.Info("OpenDB " + connectionString);
            db = new DBAccess(connectionString);
     //       db.Database.CommandTimeout = int.MaxValue;
            logger.Info("OpenDB done " + connectionString);
        }

        public void OpenAssemblyDB(string connectionString)
        {
            logger.Info("OpenAssemblyDB " + connectionString);
            AssemblyDB = new AssemblyAccess(connectionString);
            logger.Info("OpenAssemblyDB done " + connectionString);
        }



        public void GetOurEntities()
        {
            logger.Info("GetOurEntities ");
             CleaningCats = db.GetCats(out DBerrormsg);
            if (DBerrormsg.Count() > 2)
            {
  //              MessageBox.Show("Cound not open Categories DataBase");
               
                return;
            }
 //           Items = db.GetItems();
            CleaningBins = db.GetBins();
            unknownCat = CleaningCats.Where(c => c.Name.StartsWith("Unclassified")).Single();
        }

        public void AddBin()
        {
            CleaningBins.Add(bin);

        }

        public void AddCat()
        {
            CleaningCats.Add(cat);
   //         persister.Cats.Add(cat);
        }

        public void SaveBins()
        {
            db.SaveBins(CleaningBins);

        }

        public AutoSortInfo getItemInAssemblyDB(string code)
        {
            List<string> storeNames = new List<string>() { "Haille", "Millhopper", "Westgate", "HuntersCrossing" };
            AutoSortInfo info =  AssemblyDB.GetCustomerInfoFromAssembly(code);
            if (info != null)
                info.storeName = storeNames[info.storeid-1];
            return info;
        }

        public Item GetItemInDB(string code)
        {
            return db.getItem(code);
            

        }

        public List<Printer> getPrinters()
        {
            return db.getPrinters();
        }

        public Category getCat(string name)
        {

            return CleaningCats.Where(i => i.Name == name).SingleOrDefault();

        }
       

        public void SaveItem(Item item)
        {
            //Item item1 = Items.Where(i => i.BarCode == item.BarCode).SingleOrDefault();
            //if (item1 == null)
            //    Items.Add(item);
            db.SaveItem(item);
        }

        //public void GetCustomer(int orderID)
        //{
        //    activeCustomer =  db.GetCustomerInfo(orderID);
        //}
        //unfortunately the customer info is saved in a store specific database so we
        //have to look in all stores to find the info
        public bool GetCustomer(int customerID)
        {
            List<string> connectionNames = new List<string>() { "Store1Entities", "Store2Entities", "Store3Entities", "Store4Entities" };
            string indexString = customerID.ToString();
            int storeID = Int32.Parse(indexString[0].ToString());
            if (storeID < 0 || storeID > 4)
                return false;
            string storeconnectionstring = connectionNames[storeID - 1];
            //retrieve the connection string frpm app.config
            ConnectionStringSettingsCollection connections = ConfigurationManager.ConnectionStrings;
                   
            DBAccess db = new DBAccess(connections[storeconnectionstring].ConnectionString);
            activeCustomer = db.GetCustomer(customerID);
            if (activeCustomer != null) return false;
            return true;
            
        }

      
        public void SaveShowPass(int  ShowPass)
        {
            db.SaveShowPass(ShowPass);
        }
       
        public int GetShowPass()
        {
            int? pass = db.getPass();
            return (int)pass;
        }
        protected void NotifyPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

    }
    

        

}
