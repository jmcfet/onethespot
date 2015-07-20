using System;
using System.Linq;
using System.Text;
using OnTheSpot.Models;

using System.Collections.ObjectModel;
using OnTheSpot.Dal;
using Phidgets;

using System.Configuration;
using System.Collections.Generic;
using NLog;

namespace OnTheSpot.ViewModels
{
    public class GSSVM:BaseViewModel
    {
        public string getNotinAutoSort()
        {
            return "Item not found, call manager immediately! {0}";
        }
        /*
         * 2. System will check the "Due Date" of the item by looking in the DueDate column of the Table dbo.AutoSort of the assembly database.
         * Software is looking for a "current or future due date".  In other words On Monday July 22nd any pieces scanned must be due 
         * Monday July 22nd or later ( Tue the 23rd....etc.....).  The DueDate column of the table dbo.AutoSort has date, 
         * BUT NOT day of week information.  Any piece that has a prior due date to the current date will be treated as a
         * same day or current day order.  
         */
        public Bin getBin(Category cat, AutoSortInfo autoSort)
        {
            
            int dayofweek = -1;
            int bin = -1;
            if (autoSort.Duedate < DateTime.Now)
                dayofweek = (int)DateTime.Now.DayOfWeek;
            else
            {
                dayofweek = (int)((DateTime)autoSort.Duedate).DayOfWeek;
            }
            bin = dayofweek;
            int RFIDlen = autoSort.rfid.Length;
            bool route = false;
            for (int i = 0; i < autoSort.rfid.Length;i++)
            {
                if (autoSort.rfid[i] != ' ')
                {
                    route = true;
                    break;
                }

            }
            if (route)
            {
                if (dayofweek == 3 || dayofweek == 5)
                    bin = 6;
                else if (dayofweek == 1 || dayofweek == 4)
                    bin = 7;

            }
            return CleaningBins.Where(i => i.PhidgetSlot == bin-1).SingleOrDefault();
          

        }

        public void SaveItem(object item)
        {
            
            db.SaveGssItem(item as GSS);
        }
              

    }
}
