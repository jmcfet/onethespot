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
    public class BCSandGSSVM:BaseViewModel
    {
        public bool BCSMode = true;
        public BCSandGSSVM(bool Mode)
        {
            BCSMode = Mode;
        }
        public string getNotinAutoSort()
        {
            return "Invalid characters in item {0}";
        }

        public Bin getBin(Category cat, AutoSortInfo auto)
        {
            return CleaningBins.Where(i => i.Category.ID == cat.ID).SingleOrDefault();

        }
        /*
        Any piece whether it is a first time piece or has been through the system multiple times should go into the next day bin when the due date is the next day from the current day with the exception being Friday and Saturday.  Next day for Friday is Monday.  Next day for Saturday is Monday as well.  The next day rules per day are as follows:

	        On Mon, Tue is the Next Day
	        On Tue, Wed is the Next Day
	        On Wed, Thur is the Next Day
	        On Thur, Friday is the Next Day 
	        On Fri, Mon is the Next Day 
	        On Sat, Mon is the Next Day
         
        */
        public bool getNextdayBin( AutoSortInfo autoSort)
        {

            //0:sun;1:mon;2:tues:3:wed;4:thurs;5:fri;6:sat
            bool ret = false;
            if (((DateTime)autoSort.Duedate).Date <= DateTime.Now.Date)
            {
                ret = true;
            }
            else
            {
                int dueday = (int)((DateTime)autoSort.Duedate).DayOfWeek;
                int currentdayofweek = (int)DateTime.Now.DayOfWeek;
                if (currentdayofweek < 5)
                {
                    if ((dueday - currentdayofweek) == 1)
                        ret = true;

                }
                else
                {
                    if (dueday == 1)
                        ret = true; 

                }
            }
            return ret;

        }

        /*
         * 2. System will check the "Due Date" of the item by looking in the DueDate column of the Table dbo.AutoSort of the assembly database.
         * Software is looking for a "current or future due date".  In other words On Monday July 22nd any pieces scanned must be due 
         * Monday July 22nd or later ( Tue the 23rd....etc.....).  The DueDate column of the table dbo.AutoSort has date, 
         * BUT NOT day of week information.  Any piece that has a prior due date to the current date will be treated as a
         * same day or current day order.  
         */
        public Bin getBinGSS(Category cat, AutoSortInfo autoSort)
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
            for (int i = 0; i < autoSort.rfid.Length; i++)
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
            return CleaningBins.Where(i => i.PhidgetSlot == bin - 1).SingleOrDefault();
        }
        public void SaveItem(object item)
        {

            db.SaveItem(item as OnTheSpot.Models.Item);
        }
        public void SaveItemGSS(object item)
        {

            db.SaveGssItem(item as GSS);
        }
    }
    
}
