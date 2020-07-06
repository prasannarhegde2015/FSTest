using AventStack.ExtentReports;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeleniumAutomation.Helper;
using SeleniumAutomation.SeleniumObject;
using SeleniumAutomation.TestClasses;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeleniumAutomation
{
    [TestClass]
    public class UpgradeTests
    {

        static ExtentReports report;
        ExtentTest test;
        WellConfiguration config = new WellConfiguration();


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
            CommonHelper.ChangeUnitSystem("US");
            CommonHelper.ChangeUnitSystemUserSetting("US");
            Helper.CommonHelper.createasset();
            Assert.AreEqual(SeleniumAutomation.TestData.WellConfigData.HomePageTitle, SeleniumActions.getBrowserTitle(), "Title is NOT matched ");
            if (isruunisats == "false")
            {
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitForElement(SeleniumAutomation.PageObjects.WellConfigurationPage.pedashboard);
                SeleniumActions.WaitForLoad();
            }
        }

        [TestCategory("ForeSitePreUPG"), TestMethod]
        public void ForeSiteRRLWellUpgradeTest()
        {
            try
            {
                test = report.CreateTest("RRL Well upgrade");
                config.CreateRRLUpgradeWellWorkFlow(test);
                test.Pass("RRL Well upgrade Finished");
            }
            finally
            {

                //Ensure Driver alwas is quit;
                SeleniumActions.disposeDriver();
            }
        }
        [TestCategory("RRLDATA"), TestMethod]
        public void RRLDATA()
        {
            try
            {
                test = report.CreateTest("RRL Well upgrade");
                config.CreateRRLWellFullonBlankDB(test);
                test.Pass("RRL Well upgrade Finished");
            }
            finally
            {

                //Ensure Driver alwas is quit;
                SeleniumActions.disposeDriver();
            }
        }




        [TestCategory("ForeSitePostUPG_1"), TestMethod]
        public void ForeSiteRRLWellUpgradeTestVonly()
        {
            test = report.CreateTest("RRL Well upgrade");
            config.VerifyRRLWellFullonBlankDB(test);
            test.Pass("RRL Well upgrade Finished");
        }


        public void ForeSiteRRLWellMOPChange()
        {
            test = report.CreateTest("RRL Well Type Change");
            config.ChangeWellType("Gas Lift", test);
            test.Pass("RRL Well Well Type Change Finished");
        }
        [TestCategory("ForeSitePOSTUPG"), TestMethod]
        public void ForeSiteRRLWellVerifyMOPChange()
        {

            test = report.CreateTest("RRL Well Type Change");
            config.ViewWellTypeHistory(test);
            test.Pass("RRL Well Well Type Change Finished");
            //Direclty Delete Wells from  Test Method ; DO NOT use TestCleanup attribute
            CommonHelper.DeleteWellsByAPI();
            Helper.CommonHelper.Deleteasset();
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
        }

    }
}
