
using AventStack.ExtentReports;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeleniumAutomation.AGGridScreenDatas;
using SeleniumAutomation.Helper;
using SeleniumAutomation.SeleniumObject;
using SeleniumAutomation.TestClasses;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;

namespace SeleniumAutomation
{
    /// <summary>
    /// UI Automation Test Suite.
    /// </summary>
    [TestClass]
    public class FSM_Smoke
    {
        public FSM_Smoke()
        {
        }
        static ExtentReports report;
        static ExtentTest test;
        static WellConfiguration config = new WellConfiguration();

        string jobid = "";
        [ClassInitialize]
        public static void SetUp(TestContext context)
        {
            report = ExtentFactory.getInstance();
            string isruunisats = ConfigurationManager.AppSettings.Get("isRunningATS");
            SeleniumActions.InitializeWebDriver();
            Assert.AreEqual(SeleniumAutomation.TestData.WellConfigData.HomePageTitle, SeleniumActions.getBrowserTitle(), "Title is NOT matching");
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.DashboardPage.tabDashboards);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.DashboardPage.tabProdDashboard);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitForElement(SeleniumAutomation.PageObjects.WellConfigurationPage.pedashboard);
            SeleniumActions.WaitForLoad();
            test = report.CreateTest("Data Setup");
            test.Info("Creating a well as a datasetup");
            config.CreateRRLWellPartialonBlankDB(test);
            SeleniumActions.waitClick(PageObjects.FieldserviceconfigPage.fieldServicesconfigTab);
            SeleniumActions.WaitForLoad();
            JobManagement.addtemplate(test);
            //JobManagement.configurecanceljobforconcludedjobstatusview(test);
            SeleniumActions.disposeDriver();

        }
        [TestInitialize]
        public void LaunchBrowser()
        {

            // *********** Launch Browser *******************

            string isruunisats = ConfigurationManager.AppSettings.Get("isRunningATS");
            SeleniumActions.InitializeWebDriver();
            Assert.AreEqual(SeleniumAutomation.TestData.WellConfigData.HomePageTitle, SeleniumActions.getBrowserTitle(), "Title is NOT matched ");
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.DashboardPage.tabDashboards);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.DashboardPage.tabProdDashboard);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitForElement(SeleniumAutomation.PageObjects.WellConfigurationPage.pedashboard);
            SeleniumActions.WaitForLoad();
            ///
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.dashbord_filter);
            Thread.Sleep(3000);
            if (CommonHelper.AreAsssetsPresent())
            {
                SeleniumActions.KendoTypeNSelect(PageObjects.WellConfigurationPage.assets_filter1, "Not Mapped");
                Thread.Sleep(3000);
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.applybtn);
            }
            else
            {
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.lnktargetclose);
            }

        }


        [TestCategory("FSM_Smoke"), TestMethod]
        public void AddJob()
        {
            try
            {
                test = report.CreateTest("Add job");
                test.Info("Add job without Template");
                // 
                test.Info("Navigating to Job Management Page");
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.fieldserviceTab);
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobManagementTab);
                SeleniumActions.WaitForLoad();
                jobid = JobManagement.addjob(test);
                JobManagement.Verifyjobcreated(test);


            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("Job Created");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Add Job.png"))).Build());
                Assert.Fail(e.ToString());
            }

        }
        [TestCategory("FSM_Smoke_Additional"), TestMethod]
        public void UpdateJob()
        {
            try
            {
                test = report.CreateTest("Update job");
                test.Info("Update job that is created");
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.fieldserviceTab);
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobManagementTab);
                SeleniumActions.WaitForLoad();
                jobid = JobManagement.addjob(test);
                test.Info("Navigating to Job Management Page");
                JobManagement.UpdateJob(test);

            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("Job Update");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Update job.png"))).Build());
                Assert.Fail(e.ToString());
            }

        }

        [TestCategory("FSM_Smoke_Additional"), TestMethod]
        public void JobHistory()
        {
            try
            {
                test = report.CreateTest("Job History");
                test.Info("History of a job that is created");
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.fieldserviceTab);
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobManagementTab);
                SeleniumActions.WaitForLoad();
                jobid = JobManagement.addjob(test);
                test.Info("Navigating to Job Management Page");
                JobManagement.JobHistory(test, jobid);
                /*  SeleniumActions.refreshBrowser();
                  JobManagement.JobHistory(test, jobid);*/

            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("Job History");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Job History.png"))).Build());
                Assert.Fail(e.ToString());
            }

        }
        [TestCategory("FSM_Smoke_Additional"), TestMethod]
        public void Addjobfromtemplate()
        {
            try
            {
                test = report.CreateTest("Add job from Template");
                test.Info("Add job from Template");

                test.Info("Navigating to Job Management Page");
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.fieldserviceTab);
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobManagementTab);
                SeleniumActions.WaitForLoad();
                JobManagement.addjobfromtemplate(test);
                SeleniumActions.WaitForLoad();
                JobManagement.Verifyjobcreatedtemplate(test);
                /*   SeleniumActions.refreshBrowser();
                   JobManagement.Verifyjobcreated(test);*/

            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("Add job from template");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Add job from template.png"))).Build());
                Assert.Fail(e.ToString());
            }

        }
        [TestCategory("FSM_Smoke_Additional"), TestMethod]
        public void CopyPasteJob()
        {
            try
            {
                test = report.CreateTest("CopyPaste job");
                test.Info("CopyPaste job that is created");
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.fieldserviceTab);
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobManagementTab);
                SeleniumActions.WaitForLoad();
                jobid = JobManagement.addjob(test);
                test.Info("Navigating to Job Management Page");
                JobManagement.copypastejob(test);
                SeleniumActions.WaitForLoad();
                JobManagement.Verifyjobcreatedusingcopy(test);
                /*  SeleniumActions.refreshBrowser();
                  JobManagement.Verifyjobcreatedusingcopy(test);*/
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("Copy Job");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "CopyPastejob.png"))).Build());
                Assert.Fail(e.ToString());
            }

        }
        [TestCategory("FSM_Smoke_Additional"), TestMethod]
        public void DeleteJob()
        {
            try
            {
                test = report.CreateTest("Delete  Job");
                test.Info("Delete Job");
                test.Info("Navigating to Job Management Page");
                SeleniumActions.waitClick(PageObjects.JobManagementPage.fieldserviceTab);
                SeleniumActions.waitClick(PageObjects.JobManagementPage.jobManagementTab);
                SeleniumActions.WaitForLoad();
                jobid = JobManagement.addjob(test);

                SeleniumActions.waitClick(PageObjects.JobManagementPage.deletejobbutton);
                SeleniumActions.waitClick(PageObjects.JobManagementPage.confirmdialogyes2);
                string text = SeleniumActions.getInnerText(PageObjects.JobManagementPage.toaster).ToLower().Trim();
                Assert.AreEqual("deleted successfully", text);
                SeleniumActions.WaitForLoad();
                bool res = SeleniumActions.isElemPresent(SeleniumActions.getByLocator("Xpath", "//kendo-grid-list//tbody/tr//span[@title='" + jobid + "']", "Job id TD"));
                if (res == false)
                {
                    Assert.IsTrue(true);
                    CommonHelper.TraceLine("Job deleted successfully");
                }
                else
                {
                    CommonHelper.TraceLine("Job could not be deleted successfully");
                    test.Fail("Job is present after deletion", MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Delete jobI.png"))).Build());
                    Assert.Fail();
                }
                /* CommonHelper.TraceLine("Deleting  job verifification after refreshing the browser");
                 res = SeleniumActions.isElemPresent(SeleniumActions.getByLocator("Xpath", "//kendo-grid-list//tbody/tr//span[@title='" + jobid + "']", "Job id TD"));
                 if (res == false)
                 {
                     Assert.IsTrue(true);
                     CommonHelper.TraceLine("Job deleted successfully");
                 }
                 else
                 {
                     CommonHelper.TraceLine("Job could not be deleted successfully");
                     test.Fail("Job is present after deletion", MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Delete JobII.png"))).Build());
                     Assert.Fail();
                 }*/


            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("Delete job");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Delete jobIII.png"))).Build());
                Assert.Fail(e.ToString());
            }

        }

        [TestCategory("FSM_Smoke"), TestMethod]
        public void AddEvent()
        {
            try
            {
                test = report.CreateTest("Add new Event");
                test.Info("Add new event");
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.fieldserviceTab);
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobManagementTab);
                SeleniumActions.WaitForLoad();
                jobid = JobManagement.addjob(test);
                test.Info("Navigating to Job Management Page");
                JobManagement.addevent(test, jobid);
                string begindate = SeleniumActions.BringResource("eventbegindate");
                string enddate = SeleniumActions.BringResource("eventenddate");
                string eventtype = SeleniumActions.BringResource("acidizeeventtype");
                EventsDataProcess dataprocess = new EventsDataProcess();
                TypeEventGrid[] griddata = dataprocess.FetchdataScreen();
                JobManagement.verifyaddedevent(test, griddata[0], begindate, enddate, eventtype);
                /*   CommonHelper.TraceLine("Verifying after Refreshing the browser...");
                   SeleniumActions.refreshBrowser();
                   SeleniumActions.waitClick(SeleniumActions.getByLocator("Xpath", "//kendo-grid-list//tbody/tr//span[@title='" + jobid + "']", "Job id TD"));
                   SeleniumActions.waitClick(PageObjects.JobManagementPage.eventtabo);
                   JobManagement.verifyaddedevent(test, PageObjects.JobManagementPage.grideventtype, PageObjects.JobManagementPage.grideventbegindate, PageObjects.JobManagementPage.grideventenddate, PageObjects.JobManagementPage.grideventserviceprovider, PageObjects.JobManagementPage.grideventtruckunit, PageObjects.JobManagementPage.grideventcostcategory, begindate, enddate, eventtype);
               */
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("Add Event");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Add new event.png"))).Build());
                Assert.Fail(e.ToString());
            }

        }
        [TestCategory("FSM_Smoke_Additional"), TestMethod]
        public void MultiAddEvent()
        {
            try
            {
                test = report.CreateTest("Add Multi Event");
                test.Info("Add Multi event");
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.fieldserviceTab);
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobManagementTab);
                SeleniumActions.WaitForLoad();
                jobid = JobManagement.addjob(test);
                test.Info("Navigating to Job Management Page");
                JobManagement.addmultievent(test, jobid);
                System.Threading.Thread.Sleep(2000);
                string begindate = SeleniumActions.BringResource("eventbegindate");
                string enddate = SeleniumActions.BringResource("eventbegindate");
                string eventtype1 = SeleniumActions.BringResource("acidizeeventtype");
                string eventtype2 = SeleniumActions.BringResource("acidizecoileventtype");
                EventsDataProcess dataprocess = new EventsDataProcess();
                TypeEventGrid[] griddata = dataprocess.FetchdataScreen();
                JobManagement.verifyaddedevent(test, griddata[0], begindate, enddate, eventtype1);
                JobManagement.verifyaddedevent(test, griddata[1], begindate, enddate, eventtype2);
                //   JobManagement.verifyaddedevent(test, PageObjects.JobManagementPage.grideventtyperow2, PageObjects.JobManagementPage.grideventbegindaterow2, PageObjects.JobManagementPage.grideventenddaterow2, PageObjects.JobManagementPage.grideventserviceproviderrow2, PageObjects.JobManagementPage.grideventtruckunitrow2, PageObjects.JobManagementPage.grideventcostcategoryrow2, begindate, enddate, eventtype2);
                /*    CommonHelper.TraceLine("Verifying after Refreshing the browser...");
                    SeleniumActions.refreshBrowser();
                    SeleniumActions.waitClick(SeleniumActions.getByLocator("Xpath", "//kendo-grid-list//tbody/tr//span[@title='" + jobid + "']", "Job id TD"));
                    SeleniumActions.waitClick(PageObjects.JobManagementPage.eventtabo);
                    JobManagement.verifyaddedevent(test, PageObjects.JobManagementPage.grideventtype, PageObjects.JobManagementPage.grideventbegindate, PageObjects.JobManagementPage.grideventenddate, PageObjects.JobManagementPage.grideventserviceprovider, PageObjects.JobManagementPage.grideventtruckunit, PageObjects.JobManagementPage.grideventcostcategory, begindate, enddate, eventtype1);
                    JobManagement.verifyaddedevent(test, PageObjects.JobManagementPage.grideventtyperow2, PageObjects.JobManagementPage.grideventbegindaterow2, PageObjects.JobManagementPage.grideventenddaterow2, PageObjects.JobManagementPage.grideventserviceproviderrow2, PageObjects.JobManagementPage.grideventtruckunitrow2, PageObjects.JobManagementPage.grideventcostcategoryrow2, begindate, enddate, eventtype2);
                 */
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("Add Multi Event");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Add Multi event.png"))).Build());
                Assert.Fail(e.ToString());
            }

        }
        [TestCategory("FSM_Smoke_Additional"), TestMethod]
        public void CloneEvent()
        {
            try
            {
                test = report.CreateTest("Clone Event");
                test.Info("Clone event");
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.fieldserviceTab);
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobManagementTab);
                SeleniumActions.WaitForLoad();
                jobid = JobManagement.addjob(test);
                test.Info("Navigating to Job Management Page");
                JobManagement.addevent(test, jobid);
                SeleniumActions.waitClick(SeleniumActions.getByLocator("Xpath", PageObjects.JobManagementPage.EventTypeCol, "EventType column data"));
                SeleniumActions.waitClick(PageObjects.JobManagementPage.cloneventbutton);
                string text = SeleniumActions.getInnerText(PageObjects.JobManagementPage.toaster).ToLower().Trim();
                Assert.AreEqual("events cloned successfully.", text);
                SeleniumActions.WaitForLoad();
                string begindate = SeleniumActions.BringResource("eventbegindate");
                string enddate = SeleniumActions.BringResource("eventenddate");
                string eventtype = SeleniumActions.BringResource("acidizeeventtype");
                EventsDataProcess dataprocess = new EventsDataProcess();
                TypeEventGrid[] griddata = dataprocess.FetchdataScreen();
                JobManagement.verifyaddedevent(test, griddata[0], begindate, enddate, eventtype);
                JobManagement.verifyaddedevent(test, griddata[1], begindate, enddate, eventtype);
                /*   CommonHelper.TraceLine("Verifying after Refreshing the browser...");
                   SeleniumActions.refreshBrowser();
                   SeleniumActions.waitClick(SeleniumActions.getByLocator("Xpath", "//kendo-grid-list//tbody/tr//span[@title='" + jobid + "']", "Job id TD"));
                   SeleniumActions.waitClick(PageObjects.JobManagementPage.eventtabo);
                   JobManagement.verifyaddedevent(test, PageObjects.JobManagementPage.grideventtype, PageObjects.JobManagementPage.grideventbegindate, PageObjects.JobManagementPage.grideventenddate, PageObjects.JobManagementPage.grideventserviceprovider, PageObjects.JobManagementPage.grideventtruckunit, PageObjects.JobManagementPage.grideventcostcategory, begindate, enddate, eventtype);
                   JobManagement.verifyaddedevent(test, PageObjects.JobManagementPage.grideventtype2, PageObjects.JobManagementPage.grideventbegindate2, PageObjects.JobManagementPage.grideventenddate2, PageObjects.JobManagementPage.grideventserviceprovider2, PageObjects.JobManagementPage.grideventtruckunit2, PageObjects.JobManagementPage.grideventcostcategory2, begindate, enddate, eventtype);
                   */
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("Clone Event");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Clone event.png"))).Build());
                Assert.Fail(e.ToString());
            }

        }
        [TestCategory("FSM_Smoke_Additional"), TestMethod]
        public void UpdatenSaveEvent()
        {
            try
            {
                test = report.CreateTest("Update Event");
                test.Info("Update event");
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.fieldserviceTab);
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobManagementTab);
                SeleniumActions.WaitForLoad();
                jobid = JobManagement.addjob(test);
                test.Info("Navigating to Job Management Page");
                JobManagement.addevent(test, jobid);
                SeleniumActions.waitClick(SeleniumActions.getByLocator("Xpath", PageObjects.JobManagementPage.EventTypeCol, "EventType column data"));
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.updatebutton);
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(PageObjects.JobManagementPage.Additionallink);
                SeleniumActions.waitClick(PageObjects.JobManagementPage.Requiredlink);
                SeleniumActions.waitClick(PageObjects.JobManagementPage.eventbdatespecific);
                SeleniumActions.sendspecialkey(PageObjects.JobManagementPage.eventbdatespecific, "selectall");
                SeleniumActions.sendText(PageObjects.JobManagementPage.eventbdatespecific, SeleniumActions.BringResource("eventchangebegindate") + SeleniumActions.BringResource("eventbegintime"));
                // SeleniumActions.sendText(PageObjects.JobManagementPage.grideventbegindateinput, SeleniumActions.BringResource("eventchangebegindate") + SeleniumActions.BringResource("eventbegintime"));
                //  SeleniumActions.waitClick(PageObjects.JobManagementPage.grideventtype);
                SeleniumActions.waitClick(PageObjects.JobManagementPage.savebutton);
                string text = SeleniumActions.getInnerText(PageObjects.JobManagementPage.toaster).ToLower().Trim();
                Assert.AreEqual("updated successfully", text);
                SeleniumActions.WaitForLoad();
                string begindate = SeleniumActions.BringResource("eventchangebegindate");
                string enddate = SeleniumActions.BringResource("eventenddate");
                string eventtype = SeleniumActions.BringResource("acidizeeventtype");
                EventsDataProcess dataprocess = new EventsDataProcess();
                TypeEventGrid[] griddata = dataprocess.FetchdataScreen();
                JobManagement.verifyaddedevent(test, griddata[0], begindate, enddate, eventtype);

                /*   CommonHelper.TraceLine("Verifying after Refreshing the browser...");
                   SeleniumActions.refreshBrowser();
                   SeleniumActions.waitClick(SeleniumActions.getByLocator("Xpath", "//kendo-grid-list//tbody/tr//span[@title='" + jobid + "']", "Job id TD"));
                   SeleniumActions.waitClick(PageObjects.JobManagementPage.eventtabo);
                   JobManagement.verifyaddedevent(test, PageObjects.JobManagementPage.grideventtype, PageObjects.JobManagementPage.grideventbegindate, PageObjects.JobManagementPage.grideventenddate, PageObjects.JobManagementPage.grideventserviceprovider, PageObjects.JobManagementPage.grideventtruckunit, PageObjects.JobManagementPage.grideventcostcategory, begindate, enddate, eventtype);
              */
            }
            catch (Exception e)
            {

                SeleniumActions.takeScreenshot("Update Event");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Update event.png"))).Build());
                Assert.Fail(e.ToString());
            }

        }
        [TestCategory("FSM_Smoke_Additional"), TestMethod]
        public void DeleteEvent()
        {
            try
            {
                test = report.CreateTest("Update Event");
                test.Info("Update event");
                SeleniumActions.waitClick(PageObjects.JobManagementPage.fieldserviceTab);
                SeleniumActions.waitClick(PageObjects.JobManagementPage.jobManagementTab);
                SeleniumActions.WaitForLoad();
                jobid = JobManagement.addjob(test);
                test.Info("Navigating to Job Management Page");
                JobManagement.addevent(test, jobid);
                SeleniumActions.waitClick(SeleniumActions.getByLocator("Xpath", PageObjects.JobManagementPage.EventTypeCol, "Eventype col"));
                SeleniumActions.waitClick(PageObjects.JobManagementPage.deleteeventbutton);
                SeleniumActions.waitClick(PageObjects.JobManagementPage.confirmdialogyesdelete);
                SeleniumActions.WaitForLoad();
                string text = SeleniumActions.getInnerText(PageObjects.JobManagementPage.toaster).ToLower().Trim();
                Assert.AreEqual("events deleted successfully.", text);
                SeleniumActions.WaitForLoad();
                bool res = SeleniumActions.isElemPresent(SeleniumActions.getByLocator("Xpath", PageObjects.JobManagementPage.EventTypeCol, "Eventype col"));
                if (res == false)
                {
                    Assert.IsTrue(true);
                    CommonHelper.TraceLine("Deleted events successfully");
                }
                else
                {
                    SeleniumActions.takeScreenshot("Delete EventI");
                    CommonHelper.TraceLine("Events could not be deleted successfully");
                    test.Fail("Event is present after deletion", MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Delete eventI.png"))).Build());
                    Assert.Fail();
                }
                /*  CommonHelper.TraceLine("Verifying after Refreshing the browser...");
                  SeleniumActions.refreshBrowser();
                  SeleniumActions.waitClick(SeleniumActions.getByLocator("Xpath", "//kendo-grid-list//tbody/tr//span[@title='" + jobid + "']", "Job id TD"));
                  SeleniumActions.waitClick(PageObjects.JobManagementPage.eventtabo);
                  SeleniumActions.WaitForLoad();
                  res = SeleniumActions.isElemPresent(PageObjects.JobManagementPage.grideventtype);

                  if (res == false)
                  {
                      Assert.IsTrue(true);
                      CommonHelper.TraceLine("Deleted events successfully");
                  }
                  else
                  {
                      SeleniumActions.takeScreenshot("Delete EventII");
                      CommonHelper.TraceLine("Events could not be deleted successfully");
                      test.Fail("Event is present after deletion", MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Delete eventII.png"))).Build());
                      Assert.Fail();
                  }
                  */

            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("Delete EventIII");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Delete eventIII.png"))).Build());
                Assert.Fail(e.ToString());
            }

        }


        // [TestCategory("FSM_Smoke"), TestMethod]
        public void AddJobPlan()
        {
            try
            {
                test = report.CreateTest("Add JobPlan");
                test.Info("Add new JobPlan");
                test.Info("Navigating to Job Management Page");
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.fieldserviceTab);
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobManagementTab);
                SeleniumActions.WaitForLoad();
                jobid = JobManagement.addjob(test);
                string hours = "2";
                string cost = "4";
                JobManagement.addjobplan(test, jobid);
                JobManagement.verifyjobplan(test, hours, cost);
                /*     CommonHelper.TraceLine("Verifying after Refreshing the browser...");
                     SeleniumActions.refreshBrowser();
                     SeleniumActions.waitClick(SeleniumActions.getByLocator("Xpath", "//kendo-grid-list//tbody/tr//span[@title='" + jobid + "']", "Job id TD"));
                     SeleniumActions.waitClick(PageObjects.JobManagementPage.jobplantab);
                     SeleniumActions.WaitForLoad();
                     JobManagement.verifyjobplan(test, hours, cost);
                     */

            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("Add JobPlan");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Add job Plan.png"))).Build());
                Assert.Fail(e.ToString());
            }

        }
        //  [TestCategory("FSM_Smoke_Additional"), TestMethod]
        public void UpdateJobPlan()
        {
            try
            {
                test = report.CreateTest("Update JobPlan");
                test.Info("Update JobPlan");
                test.Info("Navigating to Job Management Page");
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.fieldserviceTab);
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobManagementTab);
                SeleniumActions.WaitForLoad();
                jobid = JobManagement.addjob(test);
                JobManagement.addjobplan(test, jobid);
                SeleniumActions.waitClick(PageObjects.JobManagementPage.editicon);
                string hours = "4";
                string cost = "20";
                string unitprice = "5";
                SeleniumActions.sendText(PageObjects.JobManagementPage.estimatedhoursedit, hours);
                SeleniumActions.sendText(PageObjects.JobManagementPage.unitpriceedjobplan, unitprice);
                SeleniumActions.waitClick(PageObjects.JobManagementPage.totalcostedit);
                SeleniumActions.waitClick(PageObjects.JobManagementPage.savebuttonJobPlan);
                SeleniumActions.WaitForLoad();
                JobManagement.verifyjobplan(test, hours, cost);
                /*  CommonHelper.TraceLine("Verifying after Refreshing the browser...");
                  SeleniumActions.refreshBrowser();
                  SeleniumActions.waitClick(SeleniumActions.getByLocator("Xpath", "//kendo-grid-list//tbody/tr//span[@title='" + jobid + "']", "Job id TD"));
                  SeleniumActions.waitClick(PageObjects.JobManagementPage.jobplantab);
                  SeleniumActions.WaitForLoad();
                  JobManagement.verifyjobplan(test, hours, cost);
                  */
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("Updated JobPlan");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Updated jobPlan.png"))).Build());
                Assert.Fail(e.ToString());
            }

        }
        //  [TestCategory("FSM_Smoke_Additional"), TestMethod]
        public void DeleteJobPlan()
        {
            try
            {
                test = report.CreateTest("Delete JobPlan");
                test.Info("Delete JobPlan");
                test.Info("Navigating to Job Management Page");
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.fieldserviceTab);
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobManagementTab);
                SeleniumActions.WaitForLoad();
                jobid = JobManagement.addjob(test);
                JobManagement.addjobplan(test, jobid);
                SeleniumActions.waitClick(PageObjects.JobManagementPage.deletebin);
                SeleniumActions.waitClick(PageObjects.JobManagementPage.confirmdeletejobplan);
                SeleniumActions.WaitForLoad();
                bool res = SeleniumActions.isElemPresent(PageObjects.JobManagementPage.jobplantypegrid);
                if (res == false)
                {
                    Assert.IsTrue(true);
                    CommonHelper.TraceLine("JobPlan deleted successfully");
                }
                else
                {
                    SeleniumActions.takeScreenshot("Delete JobPlanI");
                    CommonHelper.TraceLine("JobPlan could not be deleted successfully");
                    test.Fail("JobPlan is present after deletion", MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Delete JobPlan.png"))).Build());
                    Assert.Fail();
                }
                /*   SeleniumActions.refreshBrowser();
                   SeleniumActions.waitClick(SeleniumActions.getByLocator("Xpath", "//kendo-grid-list//tbody/tr//span[@title='" + jobid + "']", "Job id TD"));
                   SeleniumActions.waitClick(PageObjects.JobManagementPage.jobplantab);
                   SeleniumActions.WaitForLoad();
                   res = SeleniumActions.isElemPresent(PageObjects.JobManagementPage.jobplantypegrid);
                   if (res == false)
                   {
                       Assert.IsTrue(true);
                       CommonHelper.TraceLine("JobPlan deleted successfully");
                   }
                   else
                   {
                       SeleniumActions.takeScreenshot("Delete JobPlanII");
                       CommonHelper.TraceLine("Job could not be deleted successfully");
                       test.Fail("JobPlan is present after deletion", MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Delete JobPlan.png"))).Build());
                       Assert.Fail();
                   }
                   */

            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("Delete JobPlanIII");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Delete job Plan.png"))).Build());
                Assert.Fail(e.ToString());
            }

        }
        [TestCategory("FSM_Smoke"), TestMethod]
        public void AddJobCost()
        {
            try
            {
                test = report.CreateTest("Add JobCost");
                test.Info("Add new JobCost");
                test.Info("Navigating to Job Management Page");
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.fieldserviceTab);
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobManagementTab);
                SeleniumActions.WaitForLoad();
                jobid = JobManagement.addjob(test);
                string[] param = {
                    SeleniumActions.BringResource("jobcostdate"),//date
                    SeleniumActions.BringResource("vendor"),//vendor
                   // "N/A",//ctlgitem
                    " N/A - N/A ",//ctlgitem --> change in requirement
                    "2",//quantity
                    "4"//unitprice
                };

                JobManagement.addjobcost(test, jobid, param);
                CommonHelper.TraceLine("Verifying before Refreshing the browser...");
                JobManagement.verifyjobcost(test, param);
                /*    CommonHelper.TraceLine("Verifying after Refreshing the browser...");
                    SeleniumActions.refreshBrowser();
                    SeleniumActions.waitClick(SeleniumActions.getByLocator("Xpath", "//kendo-grid-list//tbody/tr//span[@title='" + jobid + "']", "Job id TD"));
                    SeleniumActions.waitClick(PageObjects.JobManagementPage.jobcostdetailstab);
                    JobManagement.verifyjobcost(test, param);
                 */
            }
            catch (Exception e)
            {

                SeleniumActions.takeScreenshot("Add JobCost");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Add jobCost.png"))).Build());
                Assert.Fail(e.ToString());
            }

        }
        [TestCategory("FSM_Smoke_Additional"), TestMethod]
        public void UpdateJobCost()
        {
            try
            {
                test = report.CreateTest("Update JobCost");
                test.Info("Edit your JobCost");
                test.Info("Navigating to Job Management Page");
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.fieldserviceTab);
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobManagementTab);
                SeleniumActions.WaitForLoad();
                jobid = JobManagement.addjob(test);
                string[] param = {
                    SeleniumActions.BringResource("jobcostdate"),//date
                    SeleniumActions.BringResource("vendor"),//vendor
                     " N/A - N/A ",//ctlgitem --> change in requirement
                   // "N/A",//ctlgitem
                    "2",//quantity
                    "4"//unitprice
                };

                JobManagement.addjobcost(test, jobid, param);
                param[3] = "4";
                param[4] = "5";

                SeleniumActions.waitClick(SeleniumActions.getByLocator("Xpath", "(//generic-grid)[2]//ag-grid-angular//div[@col-id='VendorName']/span/span/span/span[2]", "Vendor name"));

                SeleniumActions.WaitForLoad();

                SeleniumActions.waitClick(PageObjects.JobManagementPage.editbtn_jobcost);
                JobManagement.editjobcost(test, param, "edit");
                CommonHelper.TraceLine("Verifying Updated Job Cost");
                JobManagement.verifyjobcost(test, param);
                /*  CommonHelper.TraceLine("Verifying after Refreshing the browser...");
                  SeleniumActions.refreshBrowser();
                  SeleniumActions.waitClick(SeleniumActions.getByLocator("Xpath", "//kendo-grid-list//tbody/tr//span[@title='" + jobid + "']", "Job id TD"));
                  SeleniumActions.waitClick(PageObjects.JobManagementPage.jobcostdetailstab);
                  JobManagement.verifyjobcost(test, param);
                  */
                test.Info("Joc Cost Updated");

            }
            catch (Exception e)
            {

                SeleniumActions.takeScreenshot("Update JobCost");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Update jobCost.png"))).Build());
                Assert.Fail(e.ToString());
            }

        }

        [TestCategory("FSM_Smoke_Additional"), TestMethod]
        public void DeleteJobCost()
        {
            try
            {
                test = report.CreateTest("Delete JobCost");
                test.Info("Deleting JobCost");
                test.Info("Navigating to Job Management Page");
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.fieldserviceTab);
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobManagementTab);
                SeleniumActions.WaitForLoad();
                jobid = JobManagement.addjob(test);
                string[] param = {
                    SeleniumActions.BringResource("jobcostdate"),//date
                    SeleniumActions.BringResource("vendor"),//vendor
                   // "N/A",//ctlgitem
                     " N/A - N/A ",//ctlgitem --> change in requirement
                    "2",//quantity
                    "4"//unitprice
                };

                JobManagement.addjobcost(test, jobid, param);
                CommonHelper.TraceLine("Deleting Job Cost");
                SeleniumActions.waitClick(SeleniumActions.getByLocator("Xpath", "(//generic-grid)[2]//ag-grid-angular//div[@col-id='VendorName']/span/span/span/span[2]", "vendor name"));
                SeleniumActions.waitClick(PageObjects.JobManagementPage.removebtn_jobcost);

                //SeleniumActions.waitClick(PageObjects.JobManagementPage.deletebin);
                SeleniumActions.waitClick(PageObjects.JobManagementPage.confirmationdialogdeletejobcost);
                SeleniumActions.WaitForLoad();
                bool res = SeleniumActions.isElemPresent(SeleniumActions.getByLocator("Xpath", "(//generic-grid)[2]//ag-grid-angular//div[@col-id='VendorName']/span/span/span/span[2]", "Vendor name"));
                if (res == false)
                {
                    Assert.IsTrue(true);
                    CommonHelper.TraceLine("JobCost deleted successfully");
                }
                else
                {
                    SeleniumActions.takeScreenshot("Delete JobCostI");
                    CommonHelper.TraceLine("Jobcost could not be deleted successfully");
                    test.Fail("Jobcost is present after deletion", MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Delete JobCost.png"))).Build());
                    Assert.Fail();
                }
                /*  SeleniumActions.refreshBrowser();
                  SeleniumActions.WaitForLoad();
                  SeleniumActions.waitClick(SeleniumActions.getByLocator("Xpath", "//kendo-grid-list//tbody/tr//span[@title='" + jobid + "']", "Job id TD"));
                  SeleniumActions.waitClick(PageObjects.JobManagementPage.jobcostdetailstab);
                  SeleniumActions.WaitForLoad();
                  res = SeleniumActions.isElemPresent(PageObjects.JobManagementPage.grideventtype);
                  if (res == false)
                  {
                      Assert.IsTrue(true);
                      CommonHelper.TraceLine("JobCost deleted successfully");
                  }
                  else
                  {
                      SeleniumActions.takeScreenshot("Delete JobCostII");
                      CommonHelper.TraceLine("Jobcost could not be deleted successfully");
                      test.Fail("Jobcost is present after deletion", MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Delete JobCost.png"))).Build());
                      Assert.Fail();
                  }
                  */
            }
            catch (Exception e)
            {

                SeleniumActions.takeScreenshot("Delete JobCostIII");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Delete jobCost.png"))).Build());
                Assert.Fail(e.ToString());
            }

        }
        [TestCategory("FSM_Smoke"), TestMethod]
        public void AddDrillingReport()
        {
            try
            {
                test = report.CreateTest("Add Drilling Report");
                test.Info("Add new Drilling report");
                test.Info("Navigating to Job Management Page");
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.fieldserviceTab);
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobManagementTab);
                SeleniumActions.WaitForLoad();
                jobid = JobManagement.addjob(test);
                string[] param = {
                    TestData.JobDet.DrillingBeginDate,//date
                    SeleniumActions.BringResource("drillingreporttime"),//vendor                    
                };
                JobManagement.adddrillingreport(test, jobid, param);
                JobManagement.verifydrillingreportadded(test, param);
                test.Info("Added Drilling report");
                CommonHelper.TraceLine("Added Drilling report");
                /*    CommonHelper.TraceLine("Verifying after refreshing browser");
                    SeleniumActions.refreshBrowser();
                    SeleniumActions.WaitForLoad();
                    SeleniumActions.waitClick(SeleniumActions.getByLocator("Xpath", "//kendo-grid-list//tbody/tr//span[@title='" + jobid + "']", "Job id TD"));
                    SeleniumActions.waitClick(PageObjects.JobManagementPage.drillingtab);
                    SeleniumActions.WaitForLoad();
                    JobManagement.verifydrillingreportadded(test, param);*/
            }
            catch (Exception e)
            {

                SeleniumActions.takeScreenshot("Add Drilling report");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Add drilling report.png"))).Build());
                Assert.Fail(e.ToString());
            }

        }
        [TestCategory("FSM_Smoke_Additional"), TestMethod]
        public void DeleteDrillingReport()
        {
            try
            {
                test = report.CreateTest("Delete Drilling Report");
                test.Info("Delete existing Drilling report");
                test.Info("Navigating to Job Management Page");
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.fieldserviceTab);
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobManagementTab);
                SeleniumActions.WaitForLoad();
                jobid = JobManagement.addjob(test);
                string[] param = {
                    TestData.JobDet.DrillingBeginDate,//date
                    SeleniumActions.BringResource("drillingreporttime"),//vendor                    
                };
                JobManagement.adddrillingreport(test, jobid, param);
                JobManagement.deletedrillingreport(test, jobid, param);
                JobManagement.verifydeleteddrillingreport(test);
                test.Info("Deleted Drilling report");
                CommonHelper.TraceLine("Deleted Drilling report");
                /* CommonHelper.TraceLine("Verifying after refreshing browser");
                 SeleniumActions.refreshBrowser();
                 SeleniumActions.WaitForLoad();
                 SeleniumActions.waitClick(SeleniumActions.getByLocator("Xpath", "//kendo-grid-list//tbody/tr//span[@title='" + jobid + "']", "Job id TD"));
                 SeleniumActions.waitClick(PageObjects.JobManagementPage.drillingtab);
                 SeleniumActions.WaitForLoad();
                 JobManagement.verifydeleteddrillingreport(test);*/

            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("Delete Drilling report");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Delete Drilling report.png"))).Build());
                Assert.Fail(e.ToString());
            }

        }
        // [TestCategory("FSM_Smoke"), TestMethod]
        public void AddComponent()
        {
            try
            {
                test = report.CreateTest("Add Component to drilling report");
                test.Info("Add Component to drilling report");
                test.Info("Navigating to Job Management Page");
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.fieldserviceTab);
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobManagementTab);
                SeleniumActions.WaitForLoad();
                jobid = JobManagement.addjob(test);
                string[] param = {
                    TestData.JobDet.BeginDate,//date
                    SeleniumActions.BringResource("drillingreporttime"),//vendor                    
                };
                string[] compdet = {
                    SeleniumActions.BringResource("component"),//comp
                    SeleniumActions.BringResource("parttype"),//part
                    SeleniumActions.BringResource("catlgitemdesc"),//catlg
                    SeleniumActions.BringResource("compname"),
                    SeleniumActions.BringResource("complength"),
                    SeleniumActions.BringResource("compdepth")
                };
                JobManagement.adddrillingreport(test, jobid, param);
                JobManagement.addcomp(test, compdet);
                JobManagement.verifyaddedcomp(test, compdet);
                test.Info("Component Addded to Drilling report");
                CommonHelper.TraceLine("Component Addded to Drilling report");
                /*  CommonHelper.TraceLine("Verifying after refreshing browser");
                  SeleniumActions.refreshBrowser();
                  SeleniumActions.WaitForLoad();
                  SeleniumActions.waitClick(SeleniumActions.getByLocator("Xpath", "//kendo-grid-list//tbody/tr//span[@title='" + jobid + "']", "Job id TD"));
                  SeleniumActions.waitClick(PageObjects.JobManagementPage.drillingtab);
                  SeleniumActions.WaitForLoad();
                  JobManagement.verifyaddedcomp(test, compdet);
                  */
                SeleniumActions.waitClick(SeleniumActions.getByLocator("Xpath", "(//generic-grid)[2]//ag-grid-angular//div[@col-id='Remarks']/span/span/span/span[2]", "Remarks"));
                SeleniumActions.waitClick(PageObjects.JobManagementPage.removebtn);
                //SeleniumActions.waitClick(PageObjects.JobManagementPage.deletecomp);
                SeleniumActions.waitClick(PageObjects.JobManagementPage.savebut2);
                SeleniumActions.WaitForLoad();


            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("Add comp to Drilling report");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Add comp to Drilling report.png"))).Build());
                Assert.Fail(e.ToString());
            }

        }
        // [TestCategory("FSM_Smoke_Additional"), TestMethod]
        public void UpdateComponent()
        {
            try
            {
                test = report.CreateTest("Update Component of drilling report");
                test.Info("Update Component of drilling report");
                test.Info("Navigating to Job Management Page");
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.fieldserviceTab);
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobManagementTab);
                SeleniumActions.WaitForLoad();
                jobid = JobManagement.addjob(test);
                string[] param = {
                    TestData.JobDet.BeginDate,//date
                    SeleniumActions.BringResource("drillingreporttime"),//vendor                    
                };
                string[] compdet = {
                    SeleniumActions.BringResource("component"),//comp
                    SeleniumActions.BringResource("parttype"),//part
                    SeleniumActions.BringResource("catlgitemdesc"),//catlg
                    SeleniumActions.BringResource("compname"),
                    SeleniumActions.BringResource("complength"),
                    SeleniumActions.BringResource("compdepth")
                };
                JobManagement.adddrillingreport(test, jobid, param);
                JobManagement.addcomp(test, compdet);
                test.Info("Component Addded to Drilling report");
                CommonHelper.TraceLine("Component Addded to Drilling report");
                SeleniumActions.waitClick(PageObjects.JobManagementPage.editiconjc);
                SeleniumActions.WaitForLoad();
                compdet[3] = "Test";
                compdet[4] = "240";
                compdet[5] = "124";
                SeleniumActions.WaitForLoad();
                Thread.Sleep(2000);
                SeleniumActions.sendText(PageObjects.JobManagementPage.compname, compdet[3]);
                SeleniumActions.sendkeystroke(PageObjects.JobManagementPage.compname, "tab");
                SeleniumActions.sendText(PageObjects.JobManagementPage.complength, compdet[4]);
                SeleniumActions.sendkeystroke(PageObjects.JobManagementPage.complength, "tab");
                SeleniumActions.sendText(PageObjects.JobManagementPage.compdepth, compdet[5]);
                SeleniumActions.sendkeystroke(PageObjects.JobManagementPage.compdepth, "tab");
                SeleniumActions.waitClick(PageObjects.JobManagementPage.smallerbut);
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(PageObjects.JobManagementPage.savebut2);
                string text = SeleniumActions.getInnerText(PageObjects.JobManagementPage.toaster).ToLower().Trim();
                Assert.AreEqual("saved successfully.", text);
                JobManagement.verifyaddedcomp(test, compdet);
                /*    CommonHelper.TraceLine("Verifying after refreshing browser");
                    SeleniumActions.refreshBrowser();
                    SeleniumActions.WaitForLoad();
                    SeleniumActions.waitClick(SeleniumActions.getByLocator("Xpath", "//kendo-grid-list//tbody/tr//span[@title='" + jobid + "']", "Job id TD"));
                    SeleniumActions.waitClick(PageObjects.JobManagementPage.drillingtab);
                    SeleniumActions.WaitForLoad();
                    JobManagement.verifyaddedcomp(test, compdet);
                    */


                SeleniumActions.waitClick(SeleniumActions.getByLocator("Xpath", "(//generic-grid)[2]//ag-grid-angular//div[@col-id='Remarks']/span/span/span/span[2]", "Remarks"));
                SeleniumActions.waitClick(PageObjects.JobManagementPage.removebtn);
                SeleniumActions.waitClick(PageObjects.JobManagementPage.deletecomp);
                SeleniumActions.waitClick(PageObjects.JobManagementPage.savebut2);
                SeleniumActions.WaitForLoad();

            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("Update comp Drilling report");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Update comp drillingreport.png"))).Build());
                Assert.Fail(e.ToString());
            }

        }

        //   [TestCategory("FSM_Smoke_Additional"), TestMethod]
        public void DeleteComponent()
        {
            try
            {
                test = report.CreateTest("Delete Component to drilling report");
                test.Info("Delete Component to drilling report");
                test.Info("Navigating to Job Management Page");
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.fieldserviceTab);
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobManagementTab);
                SeleniumActions.WaitForLoad();
                jobid = JobManagement.addjob(test);
                string[] param = {
                    TestData.JobDet.BeginDate,//date
                    SeleniumActions.BringResource("drillingreporttime"),//vendor                    
                };
                string[] compdet = {
                    SeleniumActions.BringResource("component"),//comp
                    SeleniumActions.BringResource("parttype"),//part
                    SeleniumActions.BringResource("catlgitemdesc"),//catlg
                    SeleniumActions.BringResource("compname"),
                    SeleniumActions.BringResource("complength"),
                    SeleniumActions.BringResource("compdepth")
                };
                JobManagement.adddrillingreport(test, jobid, param);
                JobManagement.addcomp(test, compdet);
                JobManagement.verifyaddedcomp(test, compdet);
                test.Info("Component Addded to Drilling report");
                CommonHelper.TraceLine("Component Addded to Drilling report");
                SeleniumActions.waitClick(SeleniumActions.getByLocator("Xpath", "(//generic-grid)[2]//ag-grid-angular//div[@col-id='PartType']/span/span/span/span[2]", "Part type"));
                SeleniumActions.waitClick(PageObjects.JobManagementPage.removebtn);
                SeleniumActions.waitClick(PageObjects.JobManagementPage.deletecomp);
                SeleniumActions.waitClick(PageObjects.JobManagementPage.savebut2);
                SeleniumActions.WaitForLoad();
                bool res = SeleniumActions.isElemPresent(PageObjects.JobManagementPage.jobreasongrid);
                if (res == false)
                {
                    Assert.IsTrue(true);
                    CommonHelper.TraceLine("Comp deleted successfully");
                }
                else
                {
                    SeleniumActions.takeScreenshot("Delete Component");
                    CommonHelper.TraceLine("COmp could not be deleted successfully");
                    test.Fail("Comp is present after deletion", MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Delete Component.png"))).Build());
                    Assert.Fail();
                }

                /* CommonHelper.TraceLine("Verifying after refreshing browser");
                 SeleniumActions.refreshBrowser();
                 SeleniumActions.WaitForLoad();
                 SeleniumActions.waitClick(SeleniumActions.getByLocator("Xpath", "//kendo-grid-list//tbody/tr//span[@title='" + jobid + "']", "Job id TD"));
                 SeleniumActions.waitClick(PageObjects.JobManagementPage.drillingtab);
                 SeleniumActions.WaitForLoad();
                 res = SeleniumActions.isElemPresent(PageObjects.JobManagementPage.jobreasongrid);
                 if (res == false)
                 {
                     Assert.IsTrue(true);
                     CommonHelper.TraceLine("Comp deleted successfully");
                 }
                 else
                 {
                     SeleniumActions.takeScreenshot("Delete ComponentI");
                     CommonHelper.TraceLine("COmp could not be deleted successfully");
                     test.Fail("Comp is present after deletion", MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Delete ComponentI.png"))).Build());
                     Assert.Fail();
                 }
                 */

            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("Del comp to Drilling report");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Del comp to Drilling report.png"))).Build());
                Assert.Fail(e.ToString());
            }

        }
        //  [TestCategory("FSM_Smoke"), TestMethod]
        public void AddWellboreComponent()
        {
            try
            {
                test = report.CreateTest("Add Wellbore comp");
                test.Info("Add Component Wellbore");
                test.Info("Navigating to Job Management Page");
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.fieldserviceTab);
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobManagementTab);
                SeleniumActions.WaitForLoad();
                jobid = JobManagement.addjob(test);
                string[] param = {
                    TestData.JobDet.BeginDate,//date
                    SeleniumActions.BringResource("drillingreporttime"),//vendor                    
                };
                string[] compdet = {
                    SeleniumActions.BringResource("component"),//comp
                    SeleniumActions.BringResource("parttype"),//part
                    SeleniumActions.BringResource("catlgitemdesc"),//catlg
                    SeleniumActions.BringResource("compname"),
                    SeleniumActions.BringResource("complength"),
                    SeleniumActions.BringResource("compdepth")
                };

                JobManagement.addWellborecomp(test, compdet, jobid);
                JobManagement.verifyaddedWellborecomp(test, compdet);
                test.Info("Component Addded to Drilling report");
                CommonHelper.TraceLine("Component Addded to Drilling report");
                /*   CommonHelper.TraceLine("Verifying after refreshing browser");
                   SeleniumActions.refreshBrowser();
                   SeleniumActions.WaitForLoad();
                   SeleniumActions.waitClick(SeleniumActions.getByLocator("Xpath", "//kendo-grid-list//tbody/tr//span[@title='" + jobid + "']", "Job id TD"));
                   SeleniumActions.waitClick(PageObjects.JobManagementPage.Wellboretab);
                   SeleniumActions.WaitForLoad();
                   JobManagement.verifyaddedWellborecomp(test, compdet);
                   */
                SeleniumActions.waitClick(SeleniumActions.getByLocator("Xpath", "(//generic-grid)[2]//ag-grid-angular//div[@col-id='PartType']/span/span/span/span[2]", "Part type"));
                SeleniumActions.waitClick(PageObjects.JobManagementPage.removebtn);
                SeleniumActions.waitClick(PageObjects.JobManagementPage.deletecomp);
                SeleniumActions.waitClick(PageObjects.JobManagementPage.savebut2);
                SeleniumActions.WaitForLoad();

            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("Add Wellbore comp ");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Add Wellbore comp to Drilling report.png"))).Build());
                Assert.Fail(e.ToString());
            }

        }
        // [TestCategory("FSM_Smoke_Additional"), TestMethod]
        public void UpdateWellboreComponent()
        {
            try
            {
                test = report.CreateTest("Update Component of drilling report");
                test.Info("Update Component of drilling report");
                test.Info("Navigating to Job Management Page");
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.fieldserviceTab);
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobManagementTab);
                SeleniumActions.WaitForLoad();
                jobid = JobManagement.addjob(test);
                string[] param = {
                    TestData.JobDet.BeginDate,//date
                    SeleniumActions.BringResource("drillingreporttime"),//vendor                    
                };
                string[] compdet = {
                    SeleniumActions.BringResource("component"),//comp
                    SeleniumActions.BringResource("parttype"),//part
                    SeleniumActions.BringResource("catlgitemdesc"),//catlg
                    SeleniumActions.BringResource("compname"),
                    SeleniumActions.BringResource("complength"),
                    SeleniumActions.BringResource("compdepth")
                };

                JobManagement.addWellborecomp(test, compdet, jobid);
                test.Info("Component Addded to Drilling report");
                CommonHelper.TraceLine("Component Addded to Drilling report");
                SeleniumActions.waitClick(SeleniumActions.getByLocator("Xpath", "(//generic-grid)[2]//ag-grid-angular//div[@col-id='PartType']/span/span/span/span[2]", "Part type"));

                SeleniumActions.waitClick(PageObjects.JobManagementPage.editbtn);
                SeleniumActions.WaitForLoad();
                compdet[3] = "Test";
                compdet[4] = "240";
                compdet[5] = "124";
                SeleniumActions.sendText(PageObjects.JobManagementPage.compname, compdet[3]);
                SeleniumActions.sendkeystroke(PageObjects.JobManagementPage.compname, "tab");
                SeleniumActions.sendText(PageObjects.JobManagementPage.complength, compdet[4]);
                SeleniumActions.sendkeystroke(PageObjects.JobManagementPage.complength, "tab");
                SeleniumActions.sendText(PageObjects.JobManagementPage.compdepth, compdet[5]);
                SeleniumActions.sendkeystroke(PageObjects.JobManagementPage.compdepth, "tab");
                SeleniumActions.waitClick(PageObjects.JobManagementPage.smallerbut);
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(PageObjects.JobManagementPage.savebut2);
                string text = SeleniumActions.getInnerText(PageObjects.JobManagementPage.toaster).ToLower().Trim();
                Assert.AreEqual("saved successfully", text);
                JobManagement.verifyaddedWellborecomp(test, compdet);
                /*   CommonHelper.TraceLine("Verifying after refreshing browser");
                   SeleniumActions.refreshBrowser();
                   SeleniumActions.WaitForLoad();
                   SeleniumActions.waitClick(SeleniumActions.getByLocator("Xpath", "//kendo-grid-list//tbody/tr//span[@title='" + jobid + "']", "Job id TD"));
                   SeleniumActions.waitClick(PageObjects.JobManagementPage.Wellboretab);
                   SeleniumActions.WaitForLoad();
                   JobManagement.verifyaddedWellborecomp(test, compdet);
                   */
                SeleniumActions.waitClick(SeleniumActions.getByLocator("Xpath", "(//generic-grid)[2]//ag-grid-angular//div[@col-id='PartType']/span/span/span/span[2]", "Part type"));
                SeleniumActions.waitClick(PageObjects.JobManagementPage.removebtn);
                SeleniumActions.waitClick(PageObjects.JobManagementPage.deletecomp);
                SeleniumActions.waitClick(PageObjects.JobManagementPage.savebut2);

            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("Update Wellbore comp Drilling report");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Update Wellbore comp drillingreport.png"))).Build());
                Assert.Fail(e.ToString());
            }

        }

        //   [TestCategory("FSM_Smoke_Additional"), TestMethod]
        public void DeleteWellboreComponent()
        {
            try
            {
                test = report.CreateTest("Delete Component to drilling report");
                test.Info("Delete Component to drilling report");
                test.Info("Navigating to Job Management Page");
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.fieldserviceTab);
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobManagementTab);
                SeleniumActions.WaitForLoad();
                jobid = JobManagement.addjob(test);
                string[] param = {
                    TestData.JobDet.BeginDate,//date
                    SeleniumActions.BringResource("drillingreporttime"),//vendor                    
                };
                string[] compdet = {
                    SeleniumActions.BringResource("component"),//comp
                    SeleniumActions.BringResource("parttype"),//part
                    SeleniumActions.BringResource("catlgitemdesc"),//catlg
                    SeleniumActions.BringResource("compname"),
                    SeleniumActions.BringResource("complength"),
                    SeleniumActions.BringResource("compdepth")
                };

                JobManagement.addWellborecomp(test, compdet, jobid);
                JobManagement.verifyaddedWellborecomp(test, compdet);
                test.Info("Component Addded to Drilling report");
                CommonHelper.TraceLine("Component Addded to Drilling report");
                SeleniumActions.waitClick(SeleniumActions.getByLocator("Xpath", "(//generic-grid)[2]//ag-grid-angular//div[@col-id='PartType']/span/span/span/span[2]", "Part type"));
                SeleniumActions.waitClick(PageObjects.JobManagementPage.removebtn);
                SeleniumActions.waitClick(PageObjects.JobManagementPage.deletecomp);
                SeleniumActions.waitClick(PageObjects.JobManagementPage.savebut2);
                bool res = SeleniumActions.isElemPresent(PageObjects.JobManagementPage.jobreasongrid);
                if (res == false)
                {
                    Assert.IsTrue(true);
                    CommonHelper.TraceLine("Comp deleted successfully");
                }
                else
                {
                    SeleniumActions.takeScreenshot("Delete Wellbore Component");
                    CommonHelper.TraceLine("COmp could not be deleted successfully");
                    test.Fail("Comp is present after deletion", MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Delete Component.png"))).Build());
                    Assert.Fail();
                }

                /*   CommonHelper.TraceLine("Verifying after refreshing browser");
                   SeleniumActions.refreshBrowser();
                   SeleniumActions.WaitForLoad();
                   SeleniumActions.waitClick(SeleniumActions.getByLocator("Xpath", "//kendo-grid-list//tbody/tr//span[@title='" + jobid + "']", "Job id TD"));
                   SeleniumActions.waitClick(PageObjects.JobManagementPage.Wellboretab);
                   SeleniumActions.WaitForLoad();
                   res = SeleniumActions.isElemPresent(PageObjects.JobManagementPage.jobreasongrid);
                   if (res == false)
                   {
                       Assert.IsTrue(true);
                       CommonHelper.TraceLine("Comp deleted successfully");
                   }
                   else
                   {
                       SeleniumActions.takeScreenshot("Delete Wellbore ComponentI");
                       CommonHelper.TraceLine("COmp could not be deleted successfully");
                       test.Fail("comp is present after deletion", MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Delete ComponentI.png"))).Build());
                       Assert.Fail();
                   }
                   */

            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("Del Wellbore comp to Drilling report");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Del Wellbore comp to Drilling report.png"))).Build());
                Assert.Fail(e.ToString());
            }

        }
        //  [TestCategory("FSM_Smoke_Additional"), TestMethod]
        public void CreateJobPlanwithNotemplate()
        {
            try
            {
                test = report.CreateTest("CreateJobPlanwithNotemplat");
                test.Info("Create Job Plan with No template");
                test.Info("Navigating to Job Plan wizard");
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.fieldserviceTab);
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobPlanWizard);
                SeleniumActions.WaitForLoad();
                string[] param = {
                    TestData.JobDet.BeginDate,//date
                    TestData.JobDet.JobType,//Type
                    TestData.JobDet.JobReason,//Reason                    
                    SeleniumActions.BringResource("vendor"),//vendor                    
                };
                jobid = JobManagement.createjobwithouttempusingjobplan(test, param);
                CommonHelper.TraceLine("Job created, Id is:" + jobid);
                test.Info("Job create with ID" + jobid);
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.viewcreatedjobbutton);
                SeleniumActions.WaitForLoad();
                Thread.Sleep(2000);
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.aggridcolumn);
                Thread.Sleep(1000);
                try
                {
                    SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.aggridcolumnallselectdeselect);
                }
                catch
                {
                    CommonHelper.TraceLine("ALl columns are already visible in grid view");
                }
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.aggridcolumn);
                List<string> alljobids = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.jobidgridwhenviewcreatedjobfromjobplans);
                bool flag = false;
                foreach (var job in alljobids)
                {
                    if (job.ToLower().Trim() == jobid.ToLower().Trim())
                    {
                        CommonHelper.TraceLine("Job id created found in JM");
                        flag = true;
                    }

                }
                if (flag == false)
                {

                    SeleniumActions.takeScreenshot("Addjobwithouttemp");
                    test.Fail("Created job using job plan not present in jobmanagement", MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Addjobwithouttemp.png"))).Build());
                    Assert.Fail("Created job using job plan not present in jobmanagement");
                }
                /* Verify Job Plan which is added, Removing verification done for kendo grid*/

            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("Create job without template job plan");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Create job without template job plan"))).Build());
                Assert.Fail(e.ToString());
            }

        }
        //  [TestCategory("FSM_Smoke_Additional"), TestMethod]
        public void CreateJobPlanwithTemplate()
        {
            try
            {
                test = report.CreateTest("CreateJobPlanwithtemplat");
                test.Info("Create Job Plan with  template");
                test.Info("Navigating to Job Plan wizard");
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.fieldserviceTab);
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobPlanWizard);
                SeleniumActions.WaitForLoad();
                string[] param = {
                    TestData.JobDet.BeginDate,//date
                    TestData.JobDet.JobType,//Type
                    TestData.JobDet.JobReason,//Reason                    
                    SeleniumActions.BringResource("vendor"),//vendor                    
                };
                jobid = JobManagement.createusingtempinjobplan(test, param);

                CommonHelper.TraceLine("Job created, Id is:" + jobid);
                test.Info("Job create with ID" + jobid);
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.viewcreatedjobbutton);
                SeleniumActions.WaitForLoad();
                Thread.Sleep(2000);
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.aggridcolumn);
                Thread.Sleep(1000);
                try
                {
                    SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.aggridcolumnallselectdeselect);
                }
                catch
                {
                    CommonHelper.TraceLine("ALl columns are already visible in grid view");
                }
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.aggridcolumn);
                List<string> alljobids = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.jobidgridwhenviewcreatedjobfromjobplans);
                bool flag = false;
                foreach (var job in alljobids)
                {
                    if (job.ToLower().Trim() == jobid.ToLower().Trim())
                    {
                        CommonHelper.TraceLine("Job id created found in JM");
                        flag = true;
                    }

                }
                if (flag == false)
                {

                    SeleniumActions.takeScreenshot("Addjobwithtemp");
                    test.Fail("Created job using job plan not present in jobmanagement", MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Addjobwithouttemp.png"))).Build());
                    Assert.Fail("Created job using job plan not present in jobmanagement");
                }

                /* Verify Job Plan which is added, Removing verification done for kendo grid*/

            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("Create job with template job plan");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Create job with template job plan"))).Build());
                Assert.Fail(e.ToString());
            }

        }
        [TestCategory("FSM_Smoke_Additional"), TestMethod]
        public void UpdateJobviajobPlan()
        {
            try
            {
                test = report.CreateTest("Update Job via JobPlan wizard");
                test.Info("Update Job via JobPlan wizard");
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.fieldserviceTab);
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobManagementTab);
                SeleniumActions.WaitForLoad();
                jobid = JobManagement.addjob(test);
                JobManagement.addjobplan(test, jobid);
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobPlanWizard);
                SeleniumActions.WaitForLoad();
                string[] param = {
                    TestData.JobDet.BeginDate,//date
                    TestData.JobDet.JobType,//Type
                    TestData.JobDet.JobReason,//Reason                    
                    SeleniumActions.BringResource("vendor"),//vendor                    
                };
                jobid = JobManagement.updatejobviajobplan(test, param);
                CommonHelper.TraceLine("Job created, Id is:" + jobid);
                test.Info("Job create with ID" + jobid);
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.viewupdatedjobbutton);
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.morningreporttab);
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobManagementTab);
                SeleniumActions.WaitForLoad();
                Thread.Sleep(2000);
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.aggridcolumn);

                try
                {
                    SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.aggridcolumnallselectdeselect);
                }
                catch
                {
                    CommonHelper.TraceLine("ALl columns are already visible in grid view");
                }
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.aggridcolumn);
                Thread.Sleep(1000);
                JobManagementGeneralDataProcess JM = new JobManagementGeneralDataProcess();
                TypeJobManagementGeneralGrid[] data = JM.FetchdataScreen();
                string begindate;
                bool flag = false;
                foreach (var dat in data)
                {
                    if (dat.jobId.ToLower().Trim() == jobid.ToLower().Trim())
                    {
                        string[] splits = dat.beginDate.Split('/');

                        string[] yearsp = splits[2].Split(' ');
                        splits[2] = yearsp[0];
                        begindate = splits[0] + splits[1] + splits[2];
                        if (begindate == TestData.JobDet.ChangedBeginDate)
                        {
                            CommonHelper.TraceLine("Begin date verified with updated");
                            flag = true;
                        }
                        else
                        {
                            CommonHelper.TraceLine("Begin date found in grid splitting the character " + begindate);
                            Assert.Fail("Begin date verified with updated");
                        }
                    }

                }
                Assert.IsTrue(flag, "Job that is updated not found in grid of job management");

            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("Update Job via JobPlan");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Update job via JonPlan"))).Build());
                Assert.Fail(e.ToString());
            }

        }
        [TestCategory("FSM_Smoke_JobStatusView"), TestMethod]
        public void VerifyProspectiveJob()
        {
            try
            {
                test = report.CreateTest("Verify Prosepective Jobs");
                test.Info("Verify Prosepective Jobs in Job statuc View Screen");
                // 
                test.Info("Navigating to Job Management Page");
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.fieldserviceTab);
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobManagementTab);
                SeleniumActions.WaitForLoad();
                jobid = JobManagement.addjobProspective(test, "planned");
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobstatusview);
                SeleniumActions.WaitForLoad();
                JobManagement.verifyjobstatusview(test, "prospective", jobid, "planned");
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobManagementTab);
                SeleniumActions.WaitForLoad();
                jobid = JobManagement.addjobProspective(test, "waitingAFE");
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobstatusview);
                SeleniumActions.WaitForLoad();
                JobManagement.verifyjobstatusview(test, "prospective", jobid, "waitingAFE");
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobManagementTab);
                SeleniumActions.WaitForLoad();
                jobid = JobManagement.addjobProspective(test, "waitingApproval");
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobstatusview);
                SeleniumActions.WaitForLoad();
                JobManagement.verifyjobstatusview(test, "prospective", jobid, "waitingApproval");
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobManagementTab);
                SeleniumActions.WaitForLoad();
                jobid = JobManagement.addjobProspective(test, "waitingExternal");
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobstatusview);
                SeleniumActions.WaitForLoad();
                JobManagement.verifyjobstatusview(test, "prospective", jobid, "waitingExternal");
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobManagementTab);
                SeleniumActions.WaitForLoad();
                jobid = JobManagement.addjobProspective(test, "waitingMaintenance");
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobstatusview);
                SeleniumActions.WaitForLoad();
                JobManagement.verifyjobstatusview(test, "prospective", jobid, "waitingMaintenance");
                CommonHelper.TraceLine("Prospective View Verified");
                test.Pass("Prospective View Verified");

            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("Verify Prosepective Jobs");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Verify Prosepective Jobs.png"))).Build());
                Assert.Fail(e.ToString());
            }

        }
        [TestCategory("FSM_Smoke_JobStatusView"), TestMethod]
        public void VerifyReadyJob()
        {
            try
            {
                test = report.CreateTest("Verify Ready Jobs");
                test.Info("Verify Ready Jobs in Job status View Screen");
                // 
                test.Info("Navigating to Job Management Page");
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.fieldserviceTab);
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobManagementTab);
                SeleniumActions.WaitForLoad();
                jobid = JobManagement.addjobProspective(test, "Approved");
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobstatusview);
                SeleniumActions.WaitForLoad();
                JobManagement.verifyjobstatusview(test, "ready", jobid, "Approved");
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobManagementTab);
                SeleniumActions.WaitForLoad();
                jobid = JobManagement.addjobProspective(test, "Continuous");
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobstatusview);
                SeleniumActions.WaitForLoad();
                JobManagement.verifyjobstatusview(test, "ready", jobid, "Continuous");
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobManagementTab);
                SeleniumActions.WaitForLoad();
                jobid = JobManagement.addjobProspective(test, "CurrentlySteaming");
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobstatusview);
                SeleniumActions.WaitForLoad();
                JobManagement.verifyjobstatusview(test, "ready", jobid, "CurrentlySteaming");
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobManagementTab);
                SeleniumActions.WaitForLoad();
                jobid = JobManagement.addjobProspective(test, "InProgress");
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobstatusview);
                SeleniumActions.WaitForLoad();
                JobManagement.verifyjobstatusview(test, "ready", jobid, "InProgress");
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobManagementTab);
                SeleniumActions.WaitForLoad();
                jobid = JobManagement.addjobProspective(test, "Scheduled");
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobstatusview);
                SeleniumActions.WaitForLoad();
                JobManagement.verifyjobstatusview(test, "ready", jobid, "Scheduled");
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobManagementTab);
                SeleniumActions.WaitForLoad();
                jobid = JobManagement.addjobProspective(test, "SteamMidpoint");
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobstatusview);
                SeleniumActions.WaitForLoad();
                JobManagement.verifyjobstatusview(test, "ready", jobid, "SteamMidpoint");
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobManagementTab);
                SeleniumActions.WaitForLoad();
                jobid = JobManagement.addjobProspective(test, "SteamPutonProduction");
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobstatusview);
                SeleniumActions.WaitForLoad();
                JobManagement.verifyjobstatusview(test, "ready", jobid, "SteamPutonProduction");
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobManagementTab);
                SeleniumActions.WaitForLoad();
                jobid = JobManagement.addjobProspective(test, "SteamSoak");
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobstatusview);
                SeleniumActions.WaitForLoad();
                JobManagement.verifyjobstatusview(test, "ready", jobid, "SteamSoak");
                CommonHelper.TraceLine("Ready View Verified");
                test.Pass("ReadyJobs View Verified");

            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("Verify Ready Jobs");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Verify Ready Jobs.png"))).Build());
                Assert.Fail(e.ToString());
            }

        }
        [TestCategory("FSM_Smoke_JobStatusView"), TestMethod]
        public void VerifyConcludedJobs()
        {
            try
            {
                test = report.CreateTest("Verify Concluded Jobs");
                test.Info("Verify Concluded Jobs in Job status View Screen");
                // 
                test.Info("Navigating to Job Management Page");
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.fieldserviceTab);
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobManagementTab);
                SeleniumActions.WaitForLoad();
                /* jobid = JobManagement.addjob(test, "Cancelled");
                 SeleniumActions.WaitForLoad();
                 SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobstatusview);
                 SeleniumActions.WaitForLoad();
                 JobManagement.verifyjobstatusview(test, "concluded", jobid, "Cancelled");
                 SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobManagementTab);
                 SeleniumActions.WaitForLoad();    */
                jobid = JobManagement.addjobProspective(test, "Completed");
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobstatusview);
                SeleniumActions.WaitForLoad();
                JobManagement.verifyjobstatusview(test, "concluded", jobid, "Completed");
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobManagementTab);
                SeleniumActions.WaitForLoad();
                jobid = JobManagement.addjobProspective(test, "TourSheetComplete");
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobstatusview);
                SeleniumActions.WaitForLoad();
                JobManagement.verifyjobstatusview(test, "concluded", jobid, "TourSheetComplete");
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobManagementTab);
                SeleniumActions.WaitForLoad();
                jobid = JobManagement.addjobProspective(test, "WaitingonTourSheet");
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobstatusview);
                SeleniumActions.WaitForLoad();
                JobManagement.verifyjobstatusview(test, "concluded", jobid, "WaitingonTourSheet");

                CommonHelper.TraceLine("Concluded View Verified");
                test.Pass("Concluded View Verified");

            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("Verify Concluded Jobs");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Verify COncluded Jobs.png"))).Build());
                Assert.Fail(e.ToString());
            }

        }

        [TestCategory("FSM_Smoke"), TestMethod]
        public void MorningReport()
        {
            try
            {

                DateTime currentdate = DateTime.Now.ToUniversalTime().AddDays(-1);
                DateTime enddate = currentdate.AddDays(1);
                string currentdatestr = currentdate.ToString("MMddyyyy") + "1200AM";
                string enddatestr = enddate.ToString("MMddyyyy") + "1200AM";
                test = report.CreateTest("Verify Morning Report");
                test.Info("Verify Morning Report");
                // 
                test.Info("Navigating to Job Management Page");
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.fieldserviceTab);
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobManagementTab);
                SeleniumActions.WaitForLoad();
                jobid = JobManagement.addjob(test, "Planned", currentdatestr, enddatestr);
                SeleniumActions.waitClick(SeleniumActions.getByLocator("Xpath", "//div[@col-id='JobId']//span[@class='cell-layer-1']//span[text()='" + jobid + "']", "Job id TD"));
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(PageObjects.JobManagementPage.eventtab);
                SeleniumActions.WaitForLoad();
                JobManagement.addevent(test, jobid, currentdatestr, enddatestr, "2", "3");
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.morningreporttab);
                JobManagement.verifymorningreport(test, currentdatestr, enddatestr);
                CommonHelper.TraceLine("Morning Report Verified");
                test.Pass("Morning Report Verified");

            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("Verify Morning Report");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine(ExtentFactory.screenshotdir, "Verify Morning Report.png"))).Build());
                Assert.Fail(e.ToString());
            }

        }
        //  [TestCategory("FSM_Smoke_Additional"), TestMethod]
        public void CurrentWellbore()
        {
            try
            {
                test = report.CreateTest("Verify Current Wellbore");
                test.Info("Verify CurrentWellbore");                // 
                test.Info("Navigating to Job Management Page");
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.fieldserviceTab);
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobManagementTab);
                SeleniumActions.WaitForLoad();
                jobid = JobManagement.addjob(test);

                string[] compdet = {
                    SeleniumActions.BringResource("component"),//comp
                    SeleniumActions.BringResource("parttype"),//part
                    SeleniumActions.BringResource("catlgitemdesc"),//catlg
                    SeleniumActions.BringResource("compname"),
                    SeleniumActions.BringResource("complength"),
                    SeleniumActions.BringResource("compdepth")
                };
                JobManagement.addWellborecomp(test, compdet, jobid);
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.currentwellboretab);
                JobManagement.currentwellboreverify(test, compdet[0]);
                CommonHelper.TraceLine("Current Wellbore Test Verified");
                test.Pass("Current Wellbore test verified");

            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("Current Wellbore");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Current Wellbore.png"))).Build());
                Assert.Fail(e.ToString());
            }

        }
        // [TestCategory("FSM_Smoke_Additional"), TestMethod]
        public void WellboreHistory()
        {
            try
            {
                test = report.CreateTest("Verify Wellbore History");
                test.Info("Verify Wellbore History");                // 
                test.Info("Navigating to Job Management Page");
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.fieldserviceTab);
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobManagementTab);
                SeleniumActions.WaitForLoad();
                jobid = JobManagement.addjob(test);

                string[] compdet = {
                    SeleniumActions.BringResource("component"),//comp
                    SeleniumActions.BringResource("parttype"),//part
                    SeleniumActions.BringResource("catlgitemdesc"),//catlg
                    SeleniumActions.BringResource("compname"),
                    SeleniumActions.BringResource("complength"),
                    SeleniumActions.BringResource("compdepth")
                };
                JobManagement.addWellborecomp(test, compdet, jobid);
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.wellborehistorytab);
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.panelhead);
                Thread.Sleep(500);
                SeleniumActions.sendkeystroke("tab");
                Thread.Sleep(500);
                SeleniumActions.sendkeystroke("enter");
                Thread.Sleep(500);
                JobManagement.currentwellboreverify(test, compdet[0]);
                CommonHelper.TraceLine("Wellbore History Test Verified");
                test.Pass("Wellbore History test verified");
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("Wellbore History");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Wellbore History.png"))).Build());
                Assert.Fail(e.ToString());
            }

        }
        [TestCategory("FSM_Smoke"), TestMethod]
        public void EnterTourSheet()
        {
            try
            {
                test = report.CreateTest("Enter Tour Sheet");
                test.Info("Enter Tour Sheet");                // 
                test.Info("Navigating to Job Management Page");
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.fieldserviceTab);
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobManagementTab);
                SeleniumActions.WaitForLoad();
                jobid = JobManagement.addjobProspective(test, "InProgress");
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.entertoursheet);
                SeleniumActions.WaitForLoad();
                string[] jobcostdet = {
                    SeleniumActions.BringResource("jobcostdate"),//date
                    SeleniumActions.BringResource("vendor"),//vendor
                   // "N/A",//ctlgitem
                    " N/A - N/A ",//ctlgitem --> change in requirement
                    "2",//quantity
                    "4"//unitprice
                };
                string[] compdet = {
                    SeleniumActions.BringResource("component"),//comp
                    SeleniumActions.BringResource("parttype"),//part
                    SeleniumActions.BringResource("catlgitemdesc"),//catlg
                    SeleniumActions.BringResource("compname"),
                    SeleniumActions.BringResource("complength"),
                    SeleniumActions.BringResource("compdepth"),
                    SeleniumActions.BringResource("compbottomdepth")
                };
                JobManagement.entertoursheet(test, jobid, jobcostdet, compdet);
                CommonHelper.TraceLine("EnterTourSheet Verified");
                test.Pass("EnterTourSheet Verified");
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("Enter Tour Sheet");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Enter Tour Sheet.png"))).Build());
                Assert.Fail(e.ToString());
            }

        }
        // [TestCategory("FSM_Smoke_Additional"), TestMethod]
        public void FailureReport()
        {
            try
            {
                test = report.CreateTest("Failure Report");
                test.Info("Mark Failure component in report");
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.fieldserviceTab);
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobManagementTab);
                SeleniumActions.WaitForLoad();
                jobid = JobManagement.addjob(test);
                string temp = jobid;
                string[] param = {
                    TestData.JobDet.BeginDateTemplate,//date
                    SeleniumActions.BringResource("drillingreporttime"),//vendor                    
                };
                string[] compdet = {
                    SeleniumActions.BringResource("component"),//comp
                    SeleniumActions.BringResource("parttype"),//part
                    SeleniumActions.BringResource("catlgitemdesc"),//catlg
                    SeleniumActions.BringResource("compname"),
                    SeleniumActions.BringResource("complength"),
                    SeleniumActions.BringResource("compdepth")
                };
                JobManagement.adddrillingreport(test, jobid, param);
                JobManagement.addcomp(test, compdet);
                compdet[0] = SeleniumActions.BringResource("component1");
                compdet[1] = SeleniumActions.BringResource("parttype1");
                compdet[2] = SeleniumActions.BringResource("parttype1");
                JobManagement.addcomp(test, compdet);
                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.generalTab);
                SeleniumActions.WaitForLoad();
                jobid = JobManagement.addjob(test, "Approved", TestData.JobDet.EndDateTemplate, SeleniumActions.BringResource("futuredate"));
                SeleniumActions.waitClick(SeleniumActions.getByLocator("Xpath", "//kendo-grid-list//tbody/tr//span[@title='" + jobid + "']", "Job id TD"));
                SeleniumActions.waitClick(PageObjects.JobManagementPage.plusbutton);
                SeleniumActions.waitClick(PageObjects.JobManagementPage.failurereportoption);
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(SeleniumActions.getByLocator("Xpath", "//kendo-grid-list//tbody/tr//span[@title='" + compdet[0] + "']", "Conductor casing column"));
                SeleniumActions.waitClick(PageObjects.JobManagementPage.activerowaddobservationbutton);
                SeleniumActions.sendText(PageObjects.JobManagementPage.failuredateinput, param[0] + "12" + "00");

                SeleniumActions.waitClick(PageObjects.JobManagementPage.submitbutton);
                SeleniumActions.WaitForLoad();
                string text = SeleniumActions.getInnerText(PageObjects.JobManagementPage.observationrow).ToLower().Trim();
                if (text.Contains(compdet[0].ToLower().Trim()))
                {
                    test.Pass("Component successfully markd as failure");
                    CommonHelper.TraceLine("Component successfully markd as failure=> " + SeleniumActions.getInnerText(PageObjects.JobManagementPage.observationrow).ToLower().Trim() + "and=>" + compdet[0]);
                }
                else
                {

                    CommonHelper.TraceLine("Component could not be markd as failure=> " + SeleniumActions.getInnerText(PageObjects.JobManagementPage.observationrow).ToLower().Trim() + "and=>" + compdet[0]);
                    SeleniumActions.takeScreenshot("Failure ReportI");
                    test.Fail("Component could not be markd as failure", MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Failure ReportI.png"))).Build());
                    Assert.Fail("Component could not be markd as failure".ToString());

                }
                SeleniumActions.waitClick(PageObjects.JobManagementPage.generalTab);
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(SeleniumActions.getByLocator("Xpath", "//kendo-grid-list//tbody/tr//span[@title='" + temp + "']", "Job id TD"));
                SeleniumActions.WaitForLoad();
                Thread.Sleep(500);
                SeleniumActions.waitClick(PageObjects.JobManagementPage.drillingtab);
                SeleniumActions.WaitForLoad();
                for (int i = 1; i < 3; i++)
                {
                    SeleniumActions.waitClick(PageObjects.JobManagementPage.deletebin);
                    SeleniumActions.waitClick(PageObjects.JobManagementPage.deletecomp);
                    SeleniumActions.waitClick(PageObjects.JobManagementPage.savebut2);
                    SeleniumActions.WaitForLoad();
                }


            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("Failure Report");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Failure Report.png"))).Build());
                Assert.Fail(e.ToString());
            }

        }
        [TestCleanup]
        public void clean()
        {

            SeleniumActions.disposeDriver();
            // CommonHelper.updateUserWithGivenRole("Administrator");            
            //CommonHelper.ApiUITestBase.Cleanup();
        }

        [ClassCleanup]
        public static void TearDown()
        {
            report.Flush();
            CommonHelper.DeleteWellsByAPI();
        }
    }
}

