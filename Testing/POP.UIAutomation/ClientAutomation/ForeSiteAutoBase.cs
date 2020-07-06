using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using ArtOfTest.WebAii.Core;
using ClientAutomation.TelerikCoreUtils;
using NonTelerikHtml = ArtOfTest.WebAii.Controls.HtmlControls;

namespace ClientAutomation
{
    public class ForeSiteAutoBase
    {
        public static string Domain = ConfigurationManager.AppSettings.Get("Domain");
        public static string Site = ConfigurationManager.AppSettings.Get("CygNetSite");
        public static string Service = "UIS";
        public static string CygNetFacility = ConfigurationManager.AppSettings.Get("CygNetFacility");
        public static string Exports = ConfigurationManager.AppSettings.Get("Exports");
        public static string IsRunningInATS = ConfigurationManager.AppSettings.Get("IsRunningInATS");
        public static string browserURL = ConfigurationManager.AppSettings.Get("BrowserURL");
        public static string FilesLocation = ConfigurationManager.AppSettings.Get("FilesLocation");
        public static string txtForESP = ConfigurationManager.AppSettings.Get("SearchTxtForESP");
        public static string txtForGL = ConfigurationManager.AppSettings.Get("SearchTxtForGL");
        public static string txtForNFW = ConfigurationManager.AppSettings.Get("SearchTxtForNFW");
        public static string txtForGI = ConfigurationManager.AppSettings.Get("SearchTxtForGINJ");
        public static string txtForWI = ConfigurationManager.AppSettings.Get("SearchTxtForWINJ");
        public static string RMExecution = ConfigurationManager.AppSettings.Get("RMExecution");
        internal const string DllsLocation = @"\UIAutomation\dlls\";
        internal const string DeploymentItem = "Newtonsoft.Json.dll";
        internal const string ExportLocation = @"C:\Users\automation\Downloads\";
        internal const string ForeSiteWorkflow_RRL = "ForeSiteWorkflow_RRL";
        internal const string ForeSiteWorkflow_RRLWellAnalysis = "ForeSiteWorkflow_RRLWellAnalysis";
        internal const string ForeSiteWorkflow_GLWellAnalysis = "ForeSiteWorkflow_GLWellAnalysis";
        internal const string ForeSiteWorkflow_ESP = "ForeSiteWorkflow_ESP";
        internal const string ForeSiteWorkflow_GLift = "ForeSiteWorkflow_GLift";
        internal const string ForeSiteWorkflow_NF = "ForeSiteWorkflow_NF";
        internal const string ForeSiteWorkflow_GInj = "ForeSiteWorkflow_GInj";
        internal const string ForeSiteWorkflow_WInj = "ForeSiteWorkflow_WInj";
        internal const string ForeSiteWorkflow_FSM = "ForeSiteWorkflow_FSM";
        internal const string ForeSiteWorkflow_PLift = "ForeSiteWorkflow_PLift";
        internal const string ForeSiteWorkflow_RRLWellTestFlow = "ForeSiteWorkflow_RRLWellTestFlow";
        internal const string ForeSiteWorkflow_ESPWellTestFlow = "ForeSiteWorkflow_ESPWellTestFlow";
        internal const string ForeSiteWorkflow_GLWellTestFlow = "ForeSiteWorkflow_GLWellTestFlow";
        internal const string ForeSiteWorkflow_NFWWellTestFlow = "ForeSiteWorkflow_NFWWellTestFlow";
        internal const string ForeSite_JobManagementWorkflow = "ForeSite_JobManagementWorkflow";
        internal const string ForeSiteWorkflow_GLWellAnalysisFlow = "ForeSiteWorkflow_GLWellAnalysisFlow";
        internal const string ForeSiteWorkflow_ESPWellAnalysisFlow = "ForeSiteWorkflow_ESPWellAnalysisFlow";
        internal const string RMWorkflow_EquipmentConfiguration = "RMWorkflow_EquipmentConfiguration";

        public Manager StartApplication()
        {
            string Browser = ConfigurationManager.AppSettings.Get("Browser");
            Settings ForeUIAutoSettings = new Settings();
            ForeUIAutoSettings.ClientReadyTimeout = 300000;
            Manager ForeSiteUIAutoManager = new Manager(ForeUIAutoSettings);
            ForeSiteUIAutoManager.Start();
            TelerikObject.mgr = ForeSiteUIAutoManager;
            // Launch a new browser instance. [This will launch an IE instance given the setting above]
            ForeSiteUIAutoManager.LaunchNewBrowser(Browser == "Chrome" ? BrowserType.Chrome : BrowserType.InternetExplorer, true, ProcessWindowStyle.Maximized);
            ForeSiteUIAutoManager.ActiveBrowser.ClearCache(BrowserCacheType.Cookies);
            Thread.Sleep(2000);
            // Navigate to ForeSite
            ForeSiteUIAutoManager.ActiveBrowser.NavigateTo(browserURL);
            Thread.Sleep(15000);
            Helper.CommonHelper.TraceLine("Using browserType : " + ForeSiteUIAutoManager.ActiveBrowser.BrowserType.ToString());
            return ForeSiteUIAutoManager;
        }

        public void ExpandNavigationItems(Manager ForeSiteUIAutoManager)
        {
            Thread.Sleep(5000);
            ForeSiteUIAutoManager.ActiveBrowser.RefreshDomTree();
            NonTelerikHtml.HtmlDiv _configuration = ForeSiteUIAutoManager.ActiveBrowser.Find.ByContent<NonTelerikHtml.HtmlDiv>("Configuration");
            NonTelerikHtml.HtmlDiv _survelliance = ForeSiteUIAutoManager.ActiveBrowser.Find.ByContent<NonTelerikHtml.HtmlDiv>("Surveillance");
            NonTelerikHtml.HtmlDiv _optimization = ForeSiteUIAutoManager.ActiveBrowser.Find.ByContent<NonTelerikHtml.HtmlDiv>("Optimization");
            NonTelerikHtml.HtmlDiv _tracking = ForeSiteUIAutoManager.ActiveBrowser.Find.ByContent<NonTelerikHtml.HtmlDiv>("Field Services");
            _configuration.Click();
            _survelliance.Click();
            _optimization.Click();
            _tracking.Click();
            Thread.Sleep(3000);
        }

        public void CopyFile(string path, string item)
        {
            if (File.Exists(FilesLocation + path + item))
            {
                if (!File.Exists(ExportLocation + "\\" + item))
                    File.Copy(FilesLocation + path + item, ExportLocation + "\\" + item);
            }
        }

        public static void PrintScreen(string filename)
        {
            if (IsRunningInATS == "true")
            {
                Bitmap printscreen = new Bitmap(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height);

                Graphics graphics = Graphics.FromImage(printscreen as Image);

                graphics.CopyFromScreen(0, 0, 0, 0, printscreen.Size);

                printscreen.Save(Exports + "\\" + filename + ".jpg", ImageFormat.Jpeg);
            }
        }

        protected static void GetWellCount(string WellCountControl, out int result)
        {
            int wellCountNum;
            bool IsConverted = int.TryParse(WellCountControl.Trim().Trim(new char[] { '[', ']' }), out wellCountNum);
            if (IsConverted == true) { result = wellCountNum; } else { result = -1; }
        }
    }
}