using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelerikTest.Helper
{
    class ExtentFactory
    {
        public static ExtentReports getInstance()
        {
            ExtentReports extent;
            string reportPath = ConfigurationManager.AppSettings["reportPath"];
            var extentHtml = new ExtentHtmlReporter(reportPath);
            extent = new ExtentReports();
            extent.AttachReporter(extentHtml);
            return extent;
        }

    }
}