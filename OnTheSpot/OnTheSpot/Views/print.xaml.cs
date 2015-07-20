using OnTheSpot.Models;
using OnTheSpot.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
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
    /// Interaction logic for print.xaml
    /// </summary>
    public partial class print : Window
    {
        QSSVM vm = null;
        public print()
        {
            InitializeComponent();
            if (((App)Application.Current).vm is QSSVM)
                vm = ((App)Application.Current).vm as QSSVM; ;
            Duedate.Text = vm.Duedate;
            if (vm.activeCustomer == null)
            {
                CustomerName.Text = "test";
                vm.store = "Haille";
            }
            else
                CustomerName.Text = vm.activeCustomer.FirstName + " " + vm.activeCustomer.LastName;
            Store.Text = vm.store;
            Barcode.Text = vm.barcode;
            Name.Text = vm.Employeename;
            reason.Text = vm.reason;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //look for local and network printers
            EnumeratedPrintQueueTypes[] enumerationFlags = {

                    EnumeratedPrintQueueTypes.Local,

                    EnumeratedPrintQueueTypes.Connections
            };

            List<Printer> printers = vm.getPrinters();
            Printer target = printers.Where(p => p.Store == vm.store).SingleOrDefault();
            TargetPrinter.Text = target.PrinterName;
            
            bool bFound = false;
            LocalPrintServer localPrintServer = new LocalPrintServer();
            // Retrieving collection of local printer and remote printers
            PrintQueueCollection PrinterCollection =
                localPrintServer.GetPrintQueues(enumerationFlags);

          //  PrintQueueCollection PrinterCollection = new PrintServer(@"\\adac").GetPrintQueues(new[] {
          //EnumeratedPrintQueueTypes.Local, EnumeratedPrintQueueTypes.Connections});
            PrintQueue printq = null;
            foreach (PrintQueue printq1 in PrinterCollection)
            {
  //              MessageBox.Show(printq1.FullName);

                if ( string.Compare( printq1.FullName, target.PrinterName,true) == 0)
                {
                    printq = printq1;
                    bFound = true;
                }
            }
            if (!bFound)
            {
                MessageBox.Show(string.Format("Printer not found  {0}", target.PrinterName));
                this.Close();
                return;
            }
            PrintButton.Visibility = Visibility.Collapsed;
            TargetPrinter.Visibility = Visibility.Collapsed;
            PrintDialog pd = new PrintDialog();
            pd.PrintQueue = printq;
            pd.PrintVisual(UI, "On The Spot");
            this.Close();

        }
    }
}

#if needede
PrintQueue pq = new PrintServer().GetPrintQueues(new[]
                                                     {
                                                         EnumeratedPrintQueueTypes.Local, EnumeratedPrintQueueTypes.Connections
                                                     }).FirstOrDefault(tmp => tmp.FullName.Contains("MyPrintername"));

if (pq == null)
{
    MessageBox.Show("Please, setup printer");
    return;
}

var pt = new PrintTicket();
// setting printticket
pt.PageMediaSize = new PageMediaSize(FromMMtoPx(96, WidthInMm), FromMMtoPx(96, HeightInMm));
pt.CopyCount = Copies;
pt.PageResolution = new PageResolution(203, 203);
pt.PageOrientation = PageOrientation.Portrait;
pt.PageBorderless = PageBorderless.Borderless;

var pDoc = new System.Windows.Controls.PrintDialog();
pDoc.PrintQueue = pq;
pDoc.PrintTicket = pt;

// scale for whole printer page:

var capabilities = pDoc.PrintQueue.GetPrintCapabilities(pDoc.PrintTicket);
double scale = Math.Min(capabilities.PageImageableArea.ExtentWidth / PrintPreview.ActualWidth, capabilities.PageImageableArea.ExtentHeight / PrintPreview.ActualHeight);

PrintPreview.LayoutTransform = new ScaleTransform(scale, scale);
var sz = new Size(capabilities.PageImageableArea.ExtentWidth, capabilities.PageImageableArea.ExtentHeight);
PrintPreview.Measure(sz);
PrintPreview.Arrange(new Rect(new Point(capabilities.PageImageableArea.OriginWidth, capabilities.PageImageableArea.OriginHeight), sz));
pDoc.PrintVisual(PrintPreview, "MyPrint");
#endif