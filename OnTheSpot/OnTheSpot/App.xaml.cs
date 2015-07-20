using OnTheSpot.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace OnTheSpot
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// //to generate a module for GSS then :
    ///     set vm = GSSVM
    ///    to generate a module for QSS then :
    ///     set vm = QSSVM
    ///     chnage QSSVM vm = ((App)Application.Current).vm as QSSVM;
    /// </summary>
    public partial class App : Application
    {
        public bool bQSS = true;
        public bool bBCS = false;
        public BaseViewModel vm = new QSSVM();
       
  //      public BaseViewModel vm = new GSSVM();

 
 //       public BaseViewModel vm = new BCSandGSSVM(true);
         
    }
}
