using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace OnTheSpot.Models
{
    public class Bin 
    {
        public int ID { get; set; }

        string _BarCode;
        public string BarCode
        {
            get { return _BarCode; }
            set
            {
                if (value == _BarCode)
                    return;

                _BarCode = value;

                this.OnPropertyChanged("BarCode");
   
            }
        }

        string _Weight;
        public string Weight
        {
            get { return _Weight; }
            set
            {
                if (value == _Weight)
                    return;

                _Weight = value;

                this.OnPropertyChanged("Weight");
  
            }
        }
        string _MaxWeight;
        public string MaxWeight
        {
            get { return _MaxWeight; }
            set
            {
                if (value == _MaxWeight)
                    return;

                _MaxWeight = value;

                this.OnPropertyChanged("MaxWeight");

            }
        }

        Category _Category;
        public Category Category
        {
            get { return _Category; }
            set
            {
                if (value == _Category)
                    return;

                _Category = value;

                this.OnPropertyChanged("Category");

            }
        }
        int _PhidgetSlot;
        public int PhidgetSlot
        {
            get { return _PhidgetSlot; }
            set
            {
                if (value == _PhidgetSlot)
                    return;

                _PhidgetSlot = value;

                this.OnPropertyChanged("PhidgetSlot");

            }
        }

        

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion



    }
}
