using OnTheSpot.EFStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OnTheSpot.Models
{
    public class Item :  baseModel
    {
        public int ID { get;  set; }
        public int? CustID { get; set; }
        public DateTime? CreationDate { get; set; }
        string _BarCode;
        public string Note { get; set; }
        public string picture { get; set; }
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

        Category _category;
        public Category Category
        {
            get { return _category; }
            set
            {
                if (value == _category)
                    return;

                _category = value;

                this.OnPropertyChanged("Category");

            }
        }

    }
}
