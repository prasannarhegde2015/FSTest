
using AventStack.ExtentReports;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeleniumAutomation.Helper;
using SeleniumAutomation.SeleniumObject;
using SeleniumAutomation.TestClasses;
using System;
using System.Configuration;

namespace SeleniumAutomation
{

    /// <summary>
    /// UI Automation Test Suite.
    /// </summary>
    [TestClass]
    public class WellDesign_Smoke
    {

        static ExtentReports report;
        ExtentTest test;
        WellConfiguration config = new WellConfiguration();
        WellTest welltest = new WellTest();
        WellStatus wellstatus = new WellStatus();
        WellDesign welldesign = new WellDesign();
        public WellDesign_Smoke()
        {
        }
        [ClassInitialize]
        public static void SetUp(TestContext context)
        {
            report = ExtentFactory.getInstance();
        }
        [TestInitialize]
        public void LaunchBrowser()
        {
            // *********** Launch Browser *******************
            CommonHelper.ChangeUnitSystemUserSetting("US");
            string isruunisats = ConfigurationManager.AppSettings.Get("isRunningATS");
            SeleniumActions.InitializeWebDriver();
            Assert.AreEqual(SeleniumAutomation.TestData.WellConfigData.HomePageTitle, SeleniumActions.getBrowserTitle(), "Title is NOT matched ");
            if (isruunisats == "false")
            {
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitForElement(SeleniumAutomation.PageObjects.WellConfigurationPage.pedashboard);
                SeleniumActions.WaitForLoad();
            }
        }
        [TestCategory("WellDesign_SmokeSuite"), TestMethod]
        public void WellDesign_pgl()
        {
            try
            {
                test = report.CreateTest("WellDesign");
                welldesign.Verifywelldesign(test);


            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("Error_welldesignscreen");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Error_welldesign.png"))).Build());
                Assert.Fail(e.ToString());
            }
        }



        [TestCleanup]
        public void clean()
        {
            SeleniumActions.disposeDriver();
        }

        [ClassCleanup]
        public static void TearDown()
        {
            report.Flush();
            //  CommonHelper.DeleteWellsByAPI();
        }
    }
}
