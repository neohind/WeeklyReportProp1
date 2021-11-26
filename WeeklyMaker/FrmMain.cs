using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace WeeklyReportProtoTypes
{
    public partial class FrmMain : Form
    {
        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();


        static private string CHKURL = "http://apis.data.go.kr/B090041/openapi/service/SpcdeInfoService/getRestDeInfo";
        static private string CHKAUTHKEY = "2Ansa1s86sfg7IcyKlVsMyMa9VzN5fb73uyMmPkrfG%2FA1EwtxZSQJrL0xRPKB5JvdVohv%2BP1jycL0Zl615R5eA%3D%3D";
        private DataTable m_tbWeelkyData = null;

        public FrmMain()
        {
            InitializeComponent();
            m_tbWeelkyData = new DataTable();
            m_tbWeelkyData.Columns.Add("id", typeof(int));
            m_tbWeelkyData.Columns.Add("y", typeof(int));
            m_tbWeelkyData.Columns.Add("m", typeof(int));
            m_tbWeelkyData.Columns.Add("d", typeof(int));
            m_tbWeelkyData.Columns.Add("wd", typeof(int));
            m_tbWeelkyData.Columns.Add("std", typeof(string));
            m_tbWeelkyData.Columns.Add("end", typeof(string));            


            Dictionary<int, List<int>> dicHolidays = GetAllHolydays(2022);

            foreach (int nMonth in dicHolidays.Keys)
            {
                StringBuilder sbDays = new StringBuilder();
                foreach (int nDay in dicHolidays[nMonth])
                {
                    sbDays.Append(nDay);
                    sbDays.Append(", ");
                }
                log.Debug("2022 - {0} : {1}", nMonth, sbDays.ToString());
            }
                

            DateTime curDate = new DateTime(2021, 1, 1);
            while (curDate.DayOfWeek == DayOfWeek.Saturday
                || curDate.DayOfWeek == DayOfWeek.Sunday
                || dicHolidays[1].Contains(curDate.Day))                 
            {
                curDate = curDate.AddDays(1);
            }

            List<WeekInfo> aryWeeks = new List<WeekInfo>();
            
            
            int nCurWeek = 0;
            int nCurMonth = curDate.Month;
            while (curDate.Year == 2021)
            {
                nCurWeek++;                
                DateTime endDate = curDate.AddDays(4);
                WeekInfo info = null;
                if (nCurMonth != endDate.Month)
                {
                    DateTime dtBaseDate = new DateTime(endDate.Year, endDate.Month, 1);
                    if (dtBaseDate.DayOfWeek < DayOfWeek.Friday)
                    {
                        nCurMonth = endDate.Month;
                        nCurWeek = 1;
                        info = new WeekInfo(curDate.Year, nCurMonth, nCurWeek, curDate, endDate);
                        aryWeeks.Add(info);
                    }
                    else
                    {
                        info = new WeekInfo(curDate.Year, nCurMonth, nCurWeek, curDate, endDate);
                        aryWeeks.Add(info);
                        nCurMonth = endDate.Month;
                        nCurWeek = 0;
                    }
                }                
                else
                {
                    info = new WeekInfo(curDate.Year, nCurMonth, nCurWeek, curDate, endDate);
                    aryWeeks.Add(info);
                }
                

                Console.WriteLine(string.Format("{0} {1} {2:MM-dd ddd} {3:MM-dd ddd}", info.WeekID,info.Week, info.StartDate, info.EndDate));
                curDate = curDate.AddDays(7);
            }

        }


        private Dictionary<int, List<int>> GetAllHolydays(int nYear)
        {
            Dictionary<int, List<int>> result = new Dictionary<int, List<int>>();
            bool isCompleted = false;
            try
            {
                int nPageNo = 1;
                while (isCompleted == false)
                {
                    string sUrl = string.Format("{0}?ServiceKey={1}&solYear={2}&pageNo={3}", CHKURL, CHKAUTHKEY, nYear, nPageNo);
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(sUrl);
                    request.Method = "GET";
                    HttpWebResponse response = null;

                    using (response = (HttpWebResponse)request.GetResponse())
                    {
                        StreamReader reader = new StreamReader(response.GetResponseStream());
                        string sResponse = reader.ReadToEnd();

                        log.Debug(sResponse);

                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(sResponse);
                        XmlNodeList nodes = doc.SelectNodes("//item");
                        if(nodes.Count == 0)
                        {
                            isCompleted = true;
                            break;
                        }
                        foreach (XmlNode node in nodes)
                        {
                            XmlNode nodeIsHoliday = node.SelectSingleNode("isHoliday");
                            if (nodeIsHoliday != null)
                            {
                                if (string.Equals("Y", nodeIsHoliday.InnerText))
                                {
                                    XmlNode nodeHolidayDate = node.SelectSingleNode("locdate");
                                    string sFullDate = nodeHolidayDate.InnerText;
                                    string sMonth = sFullDate.Substring(4, 2);
                                    string sDay = sFullDate.Substring(6);
                                    int nMonth = Convert.ToInt32(sMonth);
                                    if (result.ContainsKey(nMonth) == false)
                                        result.Add(nMonth, new List<int>());
                                    result[nMonth].Add(Convert.ToInt32(sDay));
                                }
                            }
                        }
                        nPageNo++;
                    }
                }
            }
            catch(Exception ex)
            {
                log.Error(ex);
            }

            return result; 
        }

    }
}
