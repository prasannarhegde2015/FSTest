
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
    public class Analysis_Smoke
    {

        static ExtentReports report;
        ExtentTest test;
        WellConfiguration config = new WellConfiguration();
        WellAnalysis wellaln = new WellAnalysis();
        WellTest welltest = new WellTest();
        WellStatus wellstatus = new WellStatus();
        WellDesign welldesign = new WellDesign();
        public Analysis_Smoke()
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
            Helper.CommonHelper.createasset();
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
        [TestCategory("NonRRLAnalysis_SmokeSuite"), TestMethod]
        public void ForeSiteGLWellAnalysisWorkflow()
        {
            try
            {
                test = report.CreateTest("GL Well Analysis Worflow");
                config.CreateGLWellFullonBlankDB(test);
                config.createwelltest(test, "GL");
                wellaln.verifywellanalysisfields(test, "GL");
                wellaln.VerifyGLCurves(test);
                test.Pass("GL Well test and Analysis verification Finished");
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("Error_GLWell_creation");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Error_GLWell_creation.png"))).Build());
                Assert.Fail(e.ToString());
            }
        }

        [TestCategory("NonRRLAnalysis_SmokeSuite"), TestMethod]
        public void ForeSiteESPWellAnalysisWorkflow()
        {
            try
            {
                test = report.CreateTest("ESP Well creation");
                config.CreateESPWellFullonBlankDB(test);
                Helper.CommonHelper.AddWellSettings("ESP", "Min L Factor Acceptance Limit", 0.1);
                Helper.CommonHelper.AddWellSettings("ESP", "Max L Factor Acceptance Limit", 2);
                config.createwelltest(test, "ESP");
                // wellaln.verifywellanalysisfields(test, "ESP");   // test is not ready
                // wellaln.VerifyESPCurves(test);
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("Error_GLWell_creation");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Error_GLWell_creation.png"))).Build());
                Assert.Fail(e.ToString());
            }
        }

        [TestCategory("RRLAnalysis_SmokeSuite"), TestMethod]
        public void RRLWellAnalysis()
        {
            try
            {
                test = report.CreateTest("WellAnalysis");
                wellaln.RRLWellAnalysis(test);


            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("Error_wellAnalysisscreen");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Error_wellAnalysis.png"))).Build());
                Assert.Fail(e.ToString());
            }
        }



        [TestCleanup]
        public void clean()
        {
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
