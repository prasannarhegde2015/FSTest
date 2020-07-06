using SeleniumAutomation.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SeleniumAutomation.TestData
{
    class ESPTestData
    {
    }

    public class ESPGLModelData
    {
        public static string FilesLocation = ConfigurationManager.AppSettings.Get("FilesLocation");
        public static string testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\Smoke_TestData", MethodBase.GetCurrentMethod().DeclaringType.Name + ".xml");
        public static DataTable dt = CommonHelper.BuildDataTableFromXml(testdatafile);
        public static string FieldName { get { return dt.Rows[0]["FieldName"].ToString(); } }
        public static string ValueUS { get { return dt.Rows[0]["ValueUS"].ToString(); } }
        public static string ValueMetric { get { return dt.Rows[0]["ValueMetric"].ToString(); } }

        public static string UOMUS { get { return dt.Rows[0]["ValueMetric"].ToString(); } }
        public static string UOMMetric { get { return dt.Rows[0]["UOMMetric"].ToString(); } }

    }

    public class ESPGLTrajectoryData
    {
        public static string FilesLocation = ConfigurationManager.AppSettings.Get("FilesLocation");
        public static string testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\Smoke_TestData", MethodBase.GetCurrentMethod().DeclaringType.Name + ".xml");
        public static DataTable dt = CommonHelper.BuildDataTableFromXml(testdatafile);

    }

    public class ESPGLTubingData
    {
        public static string FilesLocation = ConfigurationManager.AppSettings.Get("FilesLocation");
        public static string testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\Smoke_TestData", MethodBase.GetCurrentMethod().DeclaringType.Name + ".xml");
        public static DataTable dt = CommonHelper.BuildDataTableFromXml(testdatafile);

    }

    public class ESPGLCasingData
    {
        public static string FilesLocation = ConfigurationManager.AppSettings.Get("FilesLocation");
        public static string testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\Smoke_TestData", MethodBase.GetCurrentMethod().DeclaringType.Name + ".xml");
        public static DataTable dt = CommonHelper.BuildDataTableFromXml(testdatafile);

    }


    public class ESPDataPump
    {
        public static string FilesLocation = ConfigurationManager.AppSettings.Get("FilesLocation");
        public static string testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\Smoke_TestData", MethodBase.GetCurrentMethod().DeclaringType.Name + ".xml");
        public static DataTable dt = CommonHelper.BuildDataTableFromXml(testdatafile);
        public static string TopDownId { get { return dt.Rows[0]["TopDownId"].ToString(); } }
        public static string PumpModel { get { return dt.Rows[0]["MeasuredDepthMetric"].ToString(); } }
        public static string NumberOfStages { get { return dt.Rows[0]["TrueVerticalDepthUS"].ToString(); } }

        public static string HeadTuningfactor { get { return dt.Rows[0]["TrueVerticalDepthMetric"].ToString(); } }


    }
}
