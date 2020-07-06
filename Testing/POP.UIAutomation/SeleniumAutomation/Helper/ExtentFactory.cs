using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;



namespace SeleniumAutomation.Helper
{
    class ExtentFactory


    {
        public static string screenshotdir = string.Empty;
        public static ExtentReports getInstance()
        {
            ExtentReports extent;
            string reportPath = Path.Combine(ConfigurationManager.AppSettings.Get("Exports"), ConfigurationManager.AppSettings.Get("HtmlReportName"));
            if (!Directory.Exists(Path.Combine(ConfigurationManager.AppSettings.Get("Exports"), "Screenshots")))
            {
                Directory.CreateDirectory(Path.Combine(ConfigurationManager.AppSettings.Get("Exports"), "Screenshots"));
            }
            ExtentHtmlReporter extentHtml = new ExtentHtmlReporter(reportPath);
            screenshotdir = Path.Combine(ConfigurationManager.AppSettings.Get("Exports"), "Screenshots");
            extent = new ExtentReports();
            extent.AttachReporter(extentHtml);
            return extent;
        }



    }
}
