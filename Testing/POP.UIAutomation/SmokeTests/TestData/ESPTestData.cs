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
    class ESPTestData
    {
    }


    public  class ESPGLModelData
    {
        public static string FilesLocation = ConfigurationManager.AppSettings.Get("FilesLocation");
        public static string testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\TestData", MethodBase.GetCurrentMethod().DeclaringType.Name + ".xml");
        public static DataTable dt = CommonHelper.BuildDataTableFromXml(testdatafile);
        public static string FieldName { get { return dt.Rows[0]["FieldName"].ToString(); } }
        public static string ValueUS { get { return dt.Rows[0]["ValueUS"].ToString(); } }
        public static string ValueMetric { get { return dt.Rows[0]["ValueMetric"].ToString(); } }

        public static string UOMUS { get { return dt.Rows[0]["ValueMetric"].ToString(); } }
        public static string UOMMetric { get { return dt.Rows[0]["UOMMetric"].ToString(); } }

    }
}
