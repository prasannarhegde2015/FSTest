using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SeleniumAutomation.TestClasses;
using SeleniumAutomation.SeleniumObject;
using AventStack.ExtentReports;
using SeleniumAutomation.Helper;
using Weatherford.POP.Enums;
using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Weatherford.POP.DTOs;
using System.Diagnostics;
using System.Threading;

namespace SeleniumAutomation
{
    [TestClass]
    public class SystemRailLinkVerificationTest
    {
        static ExtentReports report;
        ExtentTest test;
        private static TestContext _testContext;

        [ClassInitialize]
        public static void SetUp(TestContext context)
        {
            report = ExtentFactory.getInstance();
            _testContext = context;
        }
        [TestInitialize]
        public void LaunchBrowser()
        {
            // *********** Launch Browser *******************
            string testMethodName = SystemRailLinkVerificationTest._testContext.TestName;
            if (testMethodName.Equals("LandingPageTest"))
            {
                CommonHelper.ChangeLandingPageToBusniessIntelligence(SettingServiceStringConstants.PAGE_CONFIGURATION);
            }
            CommonHelper.ChangeUnitSystemUserSetting("US");
            Helper.CommonHelper.createasset();
            string isruunisats = ConfigurationManager.AppSettings.Get("isRunningATS");
            SeleniumActions.InitializeWebDriver();
            Assert.AreEqual(SeleniumAutomation.TestData.WellConfigData.HomePageTitle, SeleniumActions.getBrowserTitle(), "Title is NOT matched ");

        }

        [TestCategory("BusinessIntelligence"), TestMethod]
        public void LandingPageTest()

        {
            try
            {
                test = report.CreateTest("Landing Page");
                SeleniumActions.switchToDefaultFrame();
                SeleniumActions.switchToFrame(PageObjects.DashboardPage.frmSpotFireLogin);
                Thread.Sleep(5000);
                SeleniumActions.takeScreenshot("BI Landing Page");
                test.Pass("Landing Page check", MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "BI Landing Page.png"))).Build());

            }
            catch (Exception e)
            {
                Trace.WriteLine("Error in getting landing page" + e.Message);
                SeleniumActions.takeScreenshot("err_BI Landing Page");
                test.Fail("Error Landing Page check", MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "err_BI Landing Page.png"))).Build());
                Assert.Fail("Bussinessintelligence Page SPotfire Was not Loaded ");

            }
            finally
            {


            }
        }

        [TestCategory("ActionTracking"), TestMethod]
        public void ActionTrackingCRUD()
        {
            try
            {
                test = report.CreateTest("ACtion Tracking");
                TrackingItem.AddTrackingItem(test);
            }
            catch (Exception e)
            {
                Trace.WriteLine("Error in Adding Tracking Item from UI" + e.Message);
                SeleniumActions.takeScreenshot("err_trackingItem");
                test.Fail("Error in trackingItem check", MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "err_trackingItem.png"))).Build());
                Assert.Fail("Test failed Refer logs and HTML reort for details");
            }
        }

        [TestCleanup]
        public void Cleanup()

        {
            string testMethodName = SystemRailLinkVerificationTest._testContext.TestName;
            if (testMethodName.Equals("LandingPageTest"))
            {
                CommonHelper.RevertLandingPageToDefault(SettingServiceStringConstants.PAGE_CONFIGURATION);
            }

            SeleniumActions.disposeDriver();
            CommonHelper.DeleteWellsByAPI();
            Helper.CommonHelper.Deleteasset();
        }

        [ClassCleanup]
        public static void TearDown()
        {
            report.Flush();
        }
    }
}
