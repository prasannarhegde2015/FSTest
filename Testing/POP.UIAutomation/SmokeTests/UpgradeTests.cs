using AventStack.ExtentReports;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmokeTests.Helper;
using SmokeTests.SeleniumObject;
using SmokeTests.TestClasses;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmokeTests
{
    [TestClass]
    public class UpgradeTests
    {
        public static string FilesLocation = ConfigurationManager.AppSettings.Get("FilesLocation");
        static ExtentReports report;
        ExtentTest test;
        WellConfiguration config = new WellConfiguration();
        PermissionVerification perm = new PermissionVerification();

        [ClassInitialize]
        public static void SetUp(TestContext context)
        {
            report = ExtentFactory.getInstance();
        }
        [TestInitialize]
        public void LaunchBrowser()
        {
            // *********** Launch Browser *******************

            string isruunisats = ConfigurationManager.AppSettings.Get("isRunningATS");
            SeleniumActions.InitializeWebDriver();
            Assert.AreEqual(SmokeTests.TestData.WellConfigData.HomePageTitle, SeleniumActions.getBrowserTitle(), "Title is NOT matched ");
            if (isruunisats == "false")
            {
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitForElement(SmokeTests.PageObjects.WellConfigurationPage.pedashboard);
                SeleniumActions.WaitForLoad();
            }
        }

        [TestCategory("UpgradeTests"), TestMethod]
        public void ForeSiteRRLWellUpgradeTest()
        {
            test = report.CreateTest("RRL Well upgrade");
            config.CreateRRLUpgradeWellWorkFlow(test);
            test.Pass("RRL Well upgrade Finished");
        }

        [TestCategory("SmokeTests"), TestMethod]
        public void ForeSiteESPWellWorkflow()
        {
            test = report.CreateTest("ESP Well creation");
            config.CreateESPWellWorkFlow(test);
            test.Pass("ESP Well creation Finished");
        }


        [TestCleanup]
        public void clean()
        {
            SeleniumActions.disposeDriver();
           CommonHelper.updateUserWithGivenRole("Administrator");
           CommonHelper.ApiUITestBase.Cleanup();
        }

        [ClassCleanup]
        public static void TearDown()
        {
            report.Flush();
        }

    }
}
