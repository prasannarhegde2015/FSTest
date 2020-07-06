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
    /* class TestData
     {
     }*/

    public static class WellConfigData
    {

        public static string HomePageTitle { get { return "Weatherford ForeSite"; } }
        public static string CygNetDomain { get { return ConfigurationManager.AppSettings.Get("Domain"); } }

        public static string WellTypeRRL { get { return "RRL"; } }

        public static string WellTypeESP { get { return "ESP"; } }
        public static string WellTypeNF { get { return "Naturally Flowing"; } }
        public static string WellTypeGL { get { return "Gas Lift"; } }
        public static string WellTypeGInj { get { return "Gas Injection"; } }
        public static string WellTypeWInj { get { return "WAG Injection"; } }
        public static string WellTypePGL { get { return "Plunger Lift"; } }
        public static string WellTypeWaterInj { get { return "Water Injection"; } }
        public static string WellTypeOtherType { get { return "Other Type"; } }
        public static string ScadaType { get { return "CygNet"; } }
        public static string FluidType { get { return "Black Oil"; } }
        public static string PGLFluidType { get { return "Dry Gas"; } }
        public static string AssetName { get { return "TestAsset"; } }
        public static string NFFluidType { get { return "Dry Gas"; } }
        public static string RRFACName { get { return ConfigurationManager.AppSettings.Get("CygNetFacility"); } }
        public static string CygNetSite { get { return ConfigurationManager.AppSettings.Get("CygNetSite"); } }
        public static string UIS { get { return ConfigurationManager.AppSettings.Get("CygNetSite") + ".UIS"; } }

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

    public static class JobDet
    {
        public static string JobType { get { return "Remedial"; } }
        public static string JobReason { get { return "Acid Job"; } }

        public static string JobStatus { get { return "Approved"; } }
        public static string BeginDate { get { return "100120181200AM"; } }
        public static string ChangedBeginDate { get { return "09012018"; } }
        public static string EndDate { get { return "102520181200AM"; } }

        public static string BeginDateTemplate { get { return "100120181200AM"; } }

        public static string EndDateTemplate { get { return "102520181200AM"; } }

        public static string eventgridbeginDate { get { return "10/1/2018"; } }

        public static string DrillingBeginDate { get { return "10012018"; } }
    }

    public static class JobDowntime
    {
        public static string JobTypeD { get { return "DM (DORMANT)"; } }

        public static string BeginDateD { get { return "102520191200AM"; } }

    }
    public static class Fieldservice
    {
        public static string JobCat { get { return "TestCat"; } }

    }

    public class RRLTestData
    {
        public static string FilesLocation = ConfigurationManager.AppSettings.Get("FilesLocation");
        public static string testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\Smoke_TestData", MethodBase.GetCurrentMethod().DeclaringType.Name + ".xml");
        public static DataTable dt = CommonHelper.BuildDataTableFromXml(testdatafile);

    }

    public class RRLPatternMatching
    {
        public static string FilesLocation = ConfigurationManager.AppSettings.Get("FilesLocation");
        public static string testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\Smoke_TestData", MethodBase.GetCurrentMethod().DeclaringType.Name + ".xml");
        public static DataTable dt = CommonHelper.BuildDataTableFromXml(testdatafile);

    }
    public class ESPWellTestData
    {
        public static string FilesLocation = ConfigurationManager.AppSettings.Get("FilesLocation");
        public static string testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\Smoke_TestData", MethodBase.GetCurrentMethod().DeclaringType.Name + ".xml");
        public static DataTable dt = CommonHelper.BuildDataTableFromXml(testdatafile);
        public static string TestTime { get { return dt.Rows[0]["TestTime"].ToString(); } }
        public static string TestTimeInPut { get { return dt.Rows[0]["TestTimeInput"].ToString(); } }
        public static string TestDuration { get { return dt.Rows[0]["TestDuration"].ToString(); } }
        public static string QualityCodeUS { get { return dt.Rows[0]["QualityCodeUS"].ToString(); } }
        public static string MultiphasecorrelationUS { get { return dt.Rows[0]["MultiphaseFlowCorrelationUS"].ToString(); } }
        public static string Status { get { return dt.Rows[0]["Status"].ToString(); } }
        public static string Message { get { return dt.Rows[0]["Message"].ToString(); } }
        public static string TubingHeadPressureUS { get { return dt.Rows[0]["TubingHeadPressureUS"].ToString(); } }
        public static string TubingHeadTemperatureUS { get { return dt.Rows[0]["TubingHeadTemperatureUS"].ToString(); } }
        public static string CasingHeadPressureUS { get { return dt.Rows[0]["CasingHeadPressureUS"].ToString(); } }
        public static string PumpIntakePressureUS { get { return dt.Rows[0]["PumpIntakePressureUS"].ToString(); } }
        public static string PumpDischargePressureUS { get { return dt.Rows[0]["PumpDischargePressureUS"].ToString(); } }
        public static string Frequency { get { return dt.Rows[0]["Frequency"].ToString(); } }
        public static string MotorVolts { get { return dt.Rows[0]["MotorVolts"].ToString(); } }
        public static string MotorAmps { get { return dt.Rows[0]["MotorAmps"].ToString(); } }
        public static string FlowLinePressureUS { get { return dt.Rows[0]["FlowLinePressureUS"].ToString(); } }
        public static string SeperatorPressureUS { get { return dt.Rows[0]["SeperatorPressureUS"].ToString(); } }
        public static string ChokeSize { get { return dt.Rows[0]["ChokeSize"].ToString(); } }
        public static string OilRateUS { get { return dt.Rows[0]["OilRateUS"].ToString(); } }
        public static string WaterRateUS { get { return dt.Rows[0]["WaterRateUS"].ToString(); } }
        public static string GasRateUS { get { return dt.Rows[0]["GasRateUS"].ToString(); } }
        public static string LiquidRateUS { get { return dt.Rows[0]["LiquidRateUS"].ToString(); } }
        public static string FormationGasUS { get { return dt.Rows[0]["FormationGasUS"].ToString(); } }
        public static string GORUS { get { return dt.Rows[0]["GORUS"].ToString(); } }
        public static string WaterCutUS { get { return dt.Rows[0]["WaterCutUS"].ToString(); } }
        public static string FBHPUS { get { return dt.Rows[0]["FBHPUS"].ToString(); } }
        public static string LFactor { get { return dt.Rows[0]["LFactor"].ToString(); } }
        public static string ProductivityIndexUS { get { return dt.Rows[0]["ProductivityIndexUS"].ToString(); } }
        public static string ReservoirPressureUS { get { return dt.Rows[0]["ReservoirPressureUS"].ToString(); } }
        public static string HeadTuningFactorUS { get { return dt.Rows[0]["HeadTuningFactorUS"].ToString(); } }
        public static string StartNodeUS { get { return dt.Rows[0]["StartNodeUS"].ToString(); } }
        public static string SolutionNodeUS { get { return dt.Rows[0]["SolutionNodeUS"].ToString(); } }
        public static string FlowingBottomHolePressureUS { get { return dt.Rows[0]["FlowingBottomHolePressureUS"].ToString(); } }
        public static string SolutionNodePressureUS { get { return dt.Rows[0]["SolutionNodePressureUS"].ToString(); } }
        public static string LiquidRatefromIPRUS { get { return dt.Rows[0]["LiquidRatefromIPRUS"].ToString(); } }
    }

    public class GLWellTestData
    {
        public static string FilesLocation = ConfigurationManager.AppSettings.Get("FilesLocation");
        public static string testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\Smoke_TestData", MethodBase.GetCurrentMethod().DeclaringType.Name + ".xml");
        public static DataTable dt = CommonHelper.BuildDataTableFromXml(testdatafile);
        public static string TestTime { get { return dt.Rows[0]["TestTime"].ToString(); } }
        public static string TestDuration { get { return dt.Rows[0]["TestDuration"].ToString(); } }

        public static string TuningMethod { get { return dt.Rows[0]["TuningMethod"].ToString(); } }
        public static string MultiphaseFlowCorrelationUS { get { return dt.Rows[0]["MultiphaseFlowCorrelationUS"].ToString(); } }
        public static string QualityCodeUS { get { return dt.Rows[0]["QualityCodeUS"].ToString(); } }
        public static string MultiphasecorrelationUS { get { return dt.Rows[0]["MultiphaseFlowCorrelationUS"].ToString(); } }
        public static string Status { get { return dt.Rows[0]["Status"].ToString(); } }
        public static string Message { get { return dt.Rows[0]["Message"].ToString(); } }
        public static string TubingHeadPressureUS { get { return dt.Rows[0]["TubingHeadPressureUS"].ToString(); } }
        public static string TubingHeadTemperatureUS { get { return dt.Rows[0]["TubingHeadTemperatureUS"].ToString(); } }
        public static string CasingHeadPressureUS { get { return dt.Rows[0]["CasingHeadPressureUS"].ToString(); } }

        public static string Frequency { get { return dt.Rows[0]["Frequency"].ToString(); } }
        public static string MotorVolts { get { return dt.Rows[0]["MotorVolts"].ToString(); } }
        public static string MotorAmps { get { return dt.Rows[0]["MotorAmps"].ToString(); } }
        public static string FlowLinePressureUS { get { return dt.Rows[0]["FLP"].ToString(); } }
        public static string SeperatorPressureUS { get { return dt.Rows[0]["SeperatorPressureUS"].ToString(); } }
        public static string ChokeSize { get { return dt.Rows[0]["ChokeSize"].ToString(); } }
        public static string OilRateUS { get { return dt.Rows[0]["OilRateUS"].ToString(); } }
        public static string WaterRateUS { get { return dt.Rows[0]["WaterRateUS"].ToString(); } }
        public static string GasRateUS { get { return dt.Rows[0]["GasRateUS"].ToString(); } }
        public static string GasInjectionRateUS { get { return dt.Rows[0]["GasInjectionRateUS"].ToString(); } }
        public static string FormationGasUS { get { return dt.Rows[0]["FormationGasUS"].ToString(); } }
        public static string GORUS { get { return dt.Rows[0]["GORUS"].ToString(); } }
        public static string WaterCutUS { get { return dt.Rows[0]["WaterCutUS"].ToString(); } }
        public static string InjectionGLRUS { get { return dt.Rows[0]["InjectionGLRUS"].ToString(); } }
        public static string dailyavgInjectionMethod { get { return dt.Rows[0]["dailyavgInjectionMethod"].ToString(); } }
        public static string dailyavgGasInjection { get { return dt.Rows[0]["dailyavgGasInjection"].ToString(); } }
        public static string dailyavggasinjectiondepth { get { return dt.Rows[0]["dailyavggasinjectiondepth"].ToString(); } }
        public static string dailyavgliquid { get { return dt.Rows[0]["dailyavgliquid"].ToString(); } }
        public static string dailyavgDepth { get { return dt.Rows[0]["dailyavgDepth"].ToString(); } }
        public static string dailyavgFBHP { get { return dt.Rows[0]["dailyavgFBHP"].ToString(); } }
        public static string DPG { get { return dt.Rows[0]["DPG"].ToString(); } }
        public static string LFactor { get { return dt.Rows[0]["LFactor"].ToString(); } }
        public static string ProductivityIndexUS { get { return dt.Rows[0]["ProductivityIndexUS"].ToString(); } }
        public static string ReservoirPressureUS { get { return dt.Rows[0]["ReservoirPressureUS"].ToString(); } }
        public static string HeadTuningFactorUS { get { return dt.Rows[0]["HeadTuningFactorUS"].ToString(); } }
        public static string StartNodeUS { get { return dt.Rows[0]["StartNodeUS"].ToString(); } }
        public static string SolutionNodeUS { get { return dt.Rows[0]["SolutionNodeUS"].ToString(); } }
        public static string FlowingBottomHolePressureUS { get { return dt.Rows[0]["FlowingBottomHolePressureUS"].ToString(); } }
        public static string SolutionNodePressureUS { get { return dt.Rows[0]["SolutionNodePressureUS"].ToString(); } }
        public static string LiquidRatefromIPRUS { get { return dt.Rows[0]["LiquidRatefromIPRUS"].ToString(); } }
    }
}

