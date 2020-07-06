
using AventStack.ExtentReports;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeleniumAutomation.AGGridScreenDatas;
using SeleniumAutomation.Helper;
using SeleniumAutomation.SeleniumObject;
using SeleniumAutomation.TestClasses;
using System;
using System.Configuration;
using System.Threading;

namespace SeleniumAutomation
{
    /// <summary>
    /// UI Automation Test Suite.
    /// </summary>
    [TestClass]
    public class FSM_UI
    {
        public FSM_UI()
        {
        }

        static ExtentReports report;
        ExtentTest test;
        WellConfiguration config = new WellConfiguration();
        WellTest welltest = new WellTest();
        WellStatus wellstatus = new WellStatus();


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
            //Assert.AreEqual(SeleniumAutomation.TestData.WellConfigData.HomePageTitle, SeleniumActions.getBrowserTitle(), "Title is NOT matched ");
            if (isruunisats == "false")
            {
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitForElement(SeleniumAutomation.PageObjects.WellConfigurationPage.pedashboard);
                SeleniumActions.WaitForLoad();
            }
        }

        //Test to Cover event addition ,downtime addition to well status workflow 
        [TestCategory("FSM_UI"), TestMethod]
        public void DailyTotalshoursandcosts()
        {
            try
            {
                string jobid;
                config.Createtestwell();
                test = report.CreateTest("Add new Event");
                test.Info("Add new event");
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.fieldserviceTab);
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobManagementTab);
                SeleniumActions.WaitForLoad();
                jobid = JobManagement.addjob(test);
                test.Info("Navigating to Job Management Page");
                string begindate = SeleniumActions.BringResource("eventbegindate");
                string enddate = SeleniumActions.BringResource("eventenddate");
                string eventtype = SeleniumActions.BringResource("acidizeeventtype");
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(SeleniumActions.getByLocator("Xpath", "//div[@col-id='JobId']//span[@class='cell-layer-1']//span[text()='" + jobid + "']", "Job id TD"));
                SeleniumActions.waitClick(PageObjects.JobManagementPage.eventtab);
                JobManagement.addevent(test, jobid, begindate, enddate, "2", "3");
                JobManagement.addevent(test, jobid, begindate, enddate, "4", "3");
                EventsDataProcess dataprocess = new EventsDataProcess();
                TypeEventGrid[] griddata = dataprocess.FetchdataScreen();
                JobManagement.verifytotalhourscosts(griddata, "2.00", "4.00", "3.00", "3.00");
                test.Info("Navigating to Well Status Page");
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.WellStatusPage.surveillanceTab);
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.WellStatusPage.wellStatusTab);
                SeleniumActions.WaitForLoad();
                //Adding downtime
                SeleniumActions.waitClick(PageObjects.WellStatusPage.btnDowntime);
                SeleniumActions.waitClick(PageObjects.WellStatusPage.downtimeDrpdwn);
                SeleniumActions.waitClick(PageObjects.WellStatusPage.downtimeType);
                SeleniumActions.waitClick(PageObjects.WellStatusPage.btnWell);
                test.Info("Navigating to Downtime Page");
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.WellStatusPage.downtimeTab);
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.WellStatusPage.downtimeTabHistory);
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobManagementTab);
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("Add hours and cost detail to Event");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Add hrs and cost to event.png"))).Build());
                Assert.Fail(e.ToString());
            }
        }

        [TestCleanup]
        public void clean()
        {

            try
            {
                CommonHelper.DeleteWellsByAPI();
                Helper.CommonHelper.Deleteasset();
                SeleniumActions.disposeDriver();
            }
            catch (Exception)
            {

            }
        }

        [ClassCleanup]
        public static void TearDown()
        {
            report.Flush();
            try
            {
                CommonHelper.DeleteWellsByAPI();
            }
            catch (Exception)
            {


            }
        }

    }
}
