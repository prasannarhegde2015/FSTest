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

    public static class WellTestData
    {
        public static string TestDate { get { return DateTime.Now.ToString("MM/dd/yyyy"); } }
        public static string TestTime { get { return "5:30 AM"; } }
        public static string TestDuration { get { return "5"; } }
        public static string Qualitycode { get { return "Allocatable Test"; } }
        public static string TuningMethod { get { return "L Factor"; } }
        public static string status { get { return "Success"; } }
        public static string Message { get { return "FBHP < Bubble Point Pressure"; } }
        public static string THP { get { return "95.99"; } }
        public static string THT { get { return "99.9"; } }
        public static string CHP { get { return "1277.49"; } }
        public static string GasInjectionRate { get { return "1199.99"; } }
        public static string DPG { get { return "1731.19"; } }
        public static string FLP { get { return "95.98"; } }
        public static string Seperatorpressure { get { return "95.98"; } }
        public static string Chokesize { get { return "32.0"; } }
        public static string OilRate { get { return "412.9"; } }
        public static string GasRate { get { return "1899.99"; } }
        public static string FormationGasRate { get { return "700.00"; } }
        public static string WaterRate { get { return "504.7"; } }
        public static string FormationGOR { get { return "1695.3257"; } }
        public static string WaterCut { get { return "0.5500"; } }
        public static string InjectionGLR { get { return "1307.7485"; } }
        public static string FBHP { get { return "1731.51"; } }
        public static string LFactor { get { return "0.9990"; } }
        public static string ProductivityIndex { get { return "0.8919"; } }
        public static string ReservoirPressure { get { return "3000.00"; } }

        public static string dailyavgQualitycode { get { return ""; } }
        public static string dailyavgInjectionMethod { get { return "Deepest Mandrel"; } }
        public static string dailyavgDepth { get { return "9417.87"; } }
        public static string dailyavgMultiphaseFlowcorrelation { get { return "Duns and Ros (Standard)"; } }
        public static string dailyavgwellheadpressure { get { return "95.99"; } }
        public static string dailyavgwellheadtemperature { get { return "99.9"; } }
        public static string dailyavgdownholegauge { get { return "1731.19"; } }
        public static string dailyavgCHP { get { return "1277.49"; } }
        public static string dailyavggasinjection { get { return "0.00"; } }

        public static string dailyavgFBHP { get { return "2788.72"; } }
        public static string dailyavgstartnode { get { return "Xmas Tree @ 105.4 ft"; } }
        public static string dailyavgsolutionnode { get { return "New Casing_1 @ 15413 ft"; } }
        public static string dailyavgsolutionnodepr { get { return "2788.72"; } }
        public static string dailyavgSNP { get { return "2788.72"; } }
        public static string dailyavggasrate { get { return "700.00"; } }
        public static string dailyavgliquidrate { get { return "917.6"; } }
        public static string dailyavgwaterrate { get { return "504.7"; } }
        public static string dailyavgoilrate { get { return "412.9"; } }
        public static string dailyavgFormationgor { get { return "1695.3257"; } }
        public static string dailyavggasinjectiondepth { get { return "0"; } }
        public static string dailyavgliquidrateipr { get { return "182.5"; } }



    }
}

