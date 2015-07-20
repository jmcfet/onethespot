using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OnTheSpot.Models
{
    public class Customer : baseModel
    {

        public int ID { get; set; }


        string _LastName;
        public string LastName
        {
            get { return _LastName; }
            set
            {
                if (value == _LastName)
                    return;

                _LastName = value;

                this.OnPropertyChanged("LastName");

            }
        }

        string _FirstName;
        public string FirstName
        {
            get { return _FirstName; }
            set
            {
                if (value == _FirstName)
                    return;

                _FirstName = value;

                this.OnPropertyChanged("FirstName");

            }
        }
        string _Starch;
        public string Starch
        {
            get { return _Starch; }
            set
            {
                if (value == _Starch)
                    return;

                _Starch = value;

                this.OnPropertyChanged("Starch");

            }
        }
    }

}
