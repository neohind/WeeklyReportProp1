using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeeklyReportProtoTypes
{
    internal class WeekInfo
    {
        public int WeekID { get; set; }

        public int Year { get; set; }   

        public int Month { get; set; }  

        public int Week { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public int WorkingDate { get; set; }


        public WeekInfo(int nYear, int nMonth, int nWeek, DateTime dtStart, DateTime dtEnd)
        {
            Year = nYear;
            Month = nMonth; 
            Week = nWeek;
            StartDate = dtStart;
            EndDate = dtEnd;    
            WeekID = (nYear - 2000) * 1000 + Month * 10 + nWeek;
        }
    }
}
