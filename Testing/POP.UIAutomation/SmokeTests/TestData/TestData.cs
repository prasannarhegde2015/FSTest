using SmokeTests.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SmokeTests.TestData
{
    class TestData
    {
    }

    public static class WellConfigData
    {

        public static string HomePageTitle { get { return "Weatherford ForeSite"; } }
        public static string CygNetDomain { get { return ConfigurationManager.AppSettings.Get("Domain"); } }

        public static string WellTypeRRL { get { return "RRL"; } }

        public static string WellTypeESP { get { return "ESP"; } }

        public static string ScadaType { get { return "CygNet"; } }

        public static string RRFACName { get { return ConfigurationManager.AppSettings.Get("CygNetFacility"); } }
        public static string CygNetSite { get { return ConfigurationManager.AppSettings.Get("CygNetSite"); } }
        public static string UIS { get { return ConfigurationManager.AppSettings.Get("CygNetSite")+".UIS"; } }

    }

    public static class WellStatusdata
    {

        public static string IncidetnShotdesc { get { return "Selenium C# for Angular"; } }
        public static string IncidetnLongdesc { get { return "Selenium C# for Angular Large Text Long description by LTI"; } }
        public static string IncidetnState { get { return "In Progress"; } }
    }


    public static class JobDetails
    {
        public static string FilesLocation = ConfigurationManager.AppSettings.Get("FilesLocation");
        public static string testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\TestData", MethodBase.GetCurrentMethod().DeclaringType.Name + ".xml");
        public static DataTable dt = CommonHelper.BuildDataTableFromXml(testdatafile);

        
        public static string jobtype { get { return dt.Rows[0]["jobtype"].ToString(); } }
        public static string jobreason { get { return dt.Rows[0]["jobreason"].ToString(); } }
        public static string jobdriver { get { return dt.Rows[0]["jobdriver"].ToString(); } }
        public static string status { get { return dt.Rows[0]["status"].ToString(); } }
        public static string afe { get { return dt.Rows[0]["afe"].ToString(); } }
        public static string serviceprovider { get { return dt.Rows[0]["serviceprovider"].ToString(); } }
        public static string actualcost { get { return dt.Rows[0]["actualcost"].ToString(); } }
        public static string beindate { get { return dt.Rows[0]["beindate"].ToString(); } }
        public static string enddate { get { return dt.Rows[0]["enddate"].ToString(); } }
        public static string jobdurationdays { get { return dt.Rows[0]["jobdurationdays"].ToString(); } }
        public static string accoutnref { get { return dt.Rows[0]["accoutnref"].ToString(); } }
        public static string jobplancost { get { return dt.Rows[0]["jobplancost"].ToString(); } }
        public static string totalcost { get { return dt.Rows[0]["totalcost"].ToString(); } }
        public static string remarks { get { return dt.Rows[0]["remarks"].ToString(); } }

    }
}
