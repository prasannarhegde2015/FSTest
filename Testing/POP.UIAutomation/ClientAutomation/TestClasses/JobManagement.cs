using System;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using ClientAutomation.PageObjects;
using ArtOfTest.WebAii.Controls.HtmlControls;
using System.Threading;
using System.Data;
using System.Configuration;
using ArtOfTest.WebAii.ObjectModel;
using System.IO;
using ArtOfTest.WebAii.Win32.Dialogs;
using System.Collections.ObjectModel;
using ClientAutomation.TelerikCoreUtils;
using System.Linq;
using ClientAutomation.Helper;

// Last Update : 22-May-2018 

namespace ClientAutomation
{
    /// <summary>
    /// Summary description for JobManagement
    /// </summary>

    public class JobManagement : ForeSiteAutoBase
    {
        public string appURL = ConfigurationManager.AppSettings["BrowserURL"];
        public string wellname = ConfigurationManager.AppSettings["Wellname"];
        public string testdatapath = ConfigurationManager.AppSettings["ModelFilesLocation"];

        public JobManagement()
        {
        }
        public void CreateNewJob()
        {
            Thread.Sleep(5000);

            PageJobManagement.JobAssertions_BeforeJobCreation();
            TelerikObject.Click(PageJobManagement.btnAddJob);
            TelerikObject.Click(PageJobManagement.addNewJob);

            //Verify Update Pop up title
            Assert.AreEqual("Add New Job", PageJobManagement.titleOnPopup.InnerText, "Title on Add New Job pop up is incorrect");

            CommonHelper.TraceLine("Ensuring that SaveButton is Disabled before job creation ");
            Assert.IsFalse(new HtmlControl(PageJobManagement.btn_Save_job).IsEnabled, "SaveButton is Enabled ");
            CommonHelper.TraceLine("Ensuring that Job Id is Disabled before job creation ");
            Assert.IsFalse(new HtmlControl(PageJobManagement.txt_jobId).IsEnabled, "Job ID is enabled");
            CommonHelper.TraceLine("Ensuring that JOb Plan Cost is Disabled before job creation ");
            Assert.IsFalse(new HtmlControl(PageJobManagement.txt_jobPlanCost).IsEnabled, "Job Plan Cost is enabled");
            CommonHelper.TraceLine("Ensuring that Total Cost  is Disabled before job creation ");
            Assert.IsFalse(new HtmlControl(PageJobManagement.txt_jobTotalCost).IsEnabled, "Job ID Total Cost is enabled");
            //Job Duration field is now renamed to Job Duration Hours and It is removed from Add New Job & Edit Job pop up
            // ClientAutomation.Helper.CommonHelper.TraceLine("Ensuring that Job Duration Days  is Disabled before job creation ");
            // Assert.IsFalse(new HtmlControl(PageJobManagement.txt_jobDurationDays).IsEnabled, "Job Duration Days  is enabled");

            TelerikObject.Click(PageJobManagement.dd_jobType);
            string testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\TestData", "JobDetails.xml");
            CommonHelper.TraceLine("xml file path is " + testdatafile);
            CommonHelper.TraceLine("Getting Data from XML file ...");
            DataTable dt = CommonHelper.BuildDataTableFromXml(testdatafile);
            string strjobtype = dt.Rows[0]["jobtype"].ToString();
            TelerikObject.Select_KendoUI_Listitem(strjobtype, true);
            TelerikObject.Click(PageJobManagement.dd_jobReason);
            TelerikObject.Select_KendoUI_Listitem(dt.Rows[0]["jobreason"].ToString(), true);
            TelerikObject.Click(PageJobManagement.dd_jobDriver);
            TelerikObject.Select_KendoUI_Listitem(dt.Rows[0]["jobdriver"].ToString(), true);
            TelerikObject.Click(PageJobManagement.dd_jobStatus);
            TelerikObject.Select_KendoUI_Listitem(dt.Rows[0]["status"].ToString(), true);
            TelerikObject.Click(PageJobManagement.dd_jobServiceProvider);
            TelerikObject.Select_KendoUI_Listitem(dt.Rows[0]["serviceprovider"].ToString(), true);
            TelerikObject.Sendkeys(PageJobManagement.txt_actualCost, dt.Rows[0]["actualcost"].ToString());
            TelerikObject.Sendkeys(PageJobManagement.txt_beginDate, dt.Rows[0]["beindate"].ToString().Replace("/", ""), false);
            TelerikObject.Sendkeys(PageJobManagement.txt_endDate, dt.Rows[0]["enddate"].ToString().Replace("/", ""), false);
            TelerikObject.Click(PageJobManagement.dd_afe);
            TelerikObject.Sendkeys(PageJobManagement.dd_afe, dt.Rows[0]["afe"].ToString(), false);
            PageJobManagement.DynamicValue = dt.Rows[0]["afe"].ToString();
            TelerikObject.Click(PageJobManagement.listItem_afe);
            TelerikObject.Sendkeys(PageJobManagement.txt_acctRef, dt.Rows[0]["accoutnref"].ToString());
            TelerikObject.Sendkeys(PageJobManagement.txt_remarks, dt.Rows[0]["remarks"].ToString());
            TelerikObject.Click(PageJobManagement.btn_Save_job);
            CommonHelper.TraceLine("Waitng for toaster message...");
            Assert.IsNotNull(PageJobManagement.toaster_message_addSuccess, "Toaster message did not appear");
            CommonHelper.TraceLine(" toaster message has appeared on UI...");
            Thread.Sleep(5000);
            //Scrolling up to see first Job
            PageJobManagement.btnAddJob.ScrollToVisible();
            PageJobManagement.JobAssertions_AfterJobCreation();

            CommonHelper.TraceLine("Navigating away from Job management Page");
            TelerikObject.Click(PageDashboard.jobStatusView);
            Thread.Sleep(5000);
            CommonHelper.TraceLine("Navigating Back to  Job management Page");
            TelerikObject.Click(PageDashboard.jobManagementTab);
            CommonHelper.TraceLine("Verfiying Saved Values ");
            TelerikObject.AutoDomrefresh();
            Thread.Sleep(5000);
            PageJobManagement.JobFieldsSaveAssertion(dt, 0);
        }
        public void CreateJobPlan()
        {
            //TelerikObject.InitializeManager();
            //TelerikObject.gotoPage(appURL);
            //TelerikObject.DoStaticWait();

            //TelerikObject.Click(PageDashboard.trackingtab);
            //TelerikObject.Click(PageDashboard.jobManagementTab);
            //TelerikObject.DoStaticWait();
            TelerikObject.Click(PageJobManagement.newlyCreatedJob);
            Thread.Sleep(3000);
            TelerikObject.Click(PageJobManagement.addTab);
            TelerikObject.Click(PageJobManagement.tabJobPlan);
            Thread.Sleep(3000);
            //Step1: Adding Single Ne Planne dEvennt
            string costnHourDetails = PageJobManagement.costnHour.InnerText;
            CommonHelper.TraceLine("====>Planned Cost and Planned Hour details: " + costnHourDetails);

            string[] costnHour = PageJobManagement.GetHeaderDetails(costnHourDetails);
            //Verifying intial Total Planned Cost and Hour is 0
            Assert.AreEqual("0", costnHour[0], "Total Planned Cost should be 0");
            Assert.AreEqual("0", costnHour[1], "Total Planned Hours should be 0");

            int totalPlannedCost = 0;
            int totalPlannerHours = 0;


            //Verifying Save As template button & Add button is enabled
            Assert.IsTrue(PageJobManagement.btn_saveAsTemplateJob.IsEnabled, "Save as Template Job button should be enabled");
            Assert.IsTrue(PageJobManagement.btn_addOnJobPlan.IsEnabled, "Add button should be enabled on Job plan screen");

            //verifying initially there is no event present in Job Plan grid
            HtmlTable noRowAvailableTable = TelerikObject.getTableByColumnName("No records available.");
            Assert.IsNotNull(noRowAvailableTable, "No Records Available Text is not present");

            //test data from XML
            string testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\TestData", "JobPlanDetails.xml");
            CommonHelper.TraceLine("xml file path is: " + testdatafile);
            CommonHelper.TraceLine("Getting Data from XML file ...");
            DataTable dt = CommonHelper.BuildDataTableFromXml(testdatafile);
            int numberOfRows = dt.Rows.Count;

            for (int index = 0; index < numberOfRows; index++)
            {
                Thread.Sleep(3000);
                TelerikObject.Click(PageJobManagement.btn_addOnJobPlan);
                Thread.Sleep(2000);
                //verifying Save button is disabled when all required fields are blank        
                Assert.IsFalse(PageJobManagement.btn_saveOnPopUp.IsEnabled, "Save Button is enabled when all fields are blank on Planned event screen");
                Assert.AreEqual("Add New Planned Event", PageJobManagement.titleOnPopup.InnerText, "Title on Planned Event pop up is incorrect");
                PlannedEventDataEntry(dt, index);
                CommonHelper.TraceLine("=====>Saved Event " + index + 1);

                //Verify Toaster message
                Assert.AreEqual("Added Successfully", PageJobManagement.toasterMsg_PlannedEventAddSuccess.InnerText, "Planned event is not addede successfully or Toaster message verbiage is incorrect");

                //Verifying Total Planned Cost and Hour after planned event addition
                int estHours = -1;
                int estCost = -1;
                int.TryParse(dt.Rows[index]["estimatedhours"].ToString(), out estHours);
                int.TryParse(dt.Rows[index]["estimatedtotalcost"].ToString(), out estCost);
                costnHourDetails = PageJobManagement.costnHour.InnerText;
                costnHour = PageJobManagement.GetHeaderDetails(costnHourDetails);
                totalPlannedCost = totalPlannedCost + estCost;
                totalPlannerHours = totalPlannerHours + estHours;
                Assert.AreEqual(totalPlannedCost.ToString(), costnHour[0], "Incorrect Total Planned Cost is displayed ");
                Assert.AreEqual(totalPlannerHours.ToString(), costnHour[1], "Incorrect Total Planned Hours is displayed");

                //verify values appeared on Job Plan grid with xml values
                PageJobManagement.VerifyPlannedEventTableCellValues(dt, index);
                CommonHelper.TraceLine("====>Verified Planned event data present on row " + index + 1 + " is correct");
                Thread.Sleep(2000);

                //Verify Update & Delete buttons are being displayed for respective row
                PageJobManagement.VerifyActionButtonsPresence(index + 1);

                //Verify Job Plan Cost value on General tab
                TelerikObject.Click(PageJobManagement.tabGeneral);
                Assert.AreEqual(totalPlannedCost.ToString(), PageJobManagement.txt_jobPlanCost.GetAttribute("ng-reflect-model").Value, "Job Plan Cost value displayed on General tab is incorrect");
                CommonHelper.TraceLine("====>Verified Job Plan Cost on general tab");

                //Navigate back to Job Plan tab
                TelerikObject.Click(PageJobManagement.tabJobPlan);
            }

            UpdatePlannedEvent(dt, numberOfRows);

            DeletePlannedEvent(dt, numberOfRows);

        }

        private void UpdatePlannedEvent(DataTable dt, int rowNo)
        {
            HtmlSpan updateButton = (HtmlSpan)TelerikObject.GetElement("HtmlSpan", "xpath", "//td[@ng-reflect-logical-row-index='" + rowNo + "']/button/span[@title='Update']", "Update button for Planned Event " + rowNo);
            TelerikObject.mgr.Desktop.Mouse.HoverOver(updateButton.GetRectangle());
            TelerikObject.Click(updateButton);
            Thread.Sleep(2000);

            // Assert.IsFalse(PageJobManagement.btn_saveOnPlannedEvent.IsEnabled, "Save Button should be disabled if none of the field is edited on Edit pop up");


            //Verify Edit Pop up title
            Assert.AreEqual("Edit", PageJobManagement.titleOnPopup.InnerText, "Title on Planned Event pop up is incorrect");
            //Verify Planned Event id field is readonly
            //Assert.IsFalse(PageJobManagement.txt_PlannedEventId.IsEnabled, "Id field on Edit pop up should be disabled");

            //test data from XML
            string testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\TestData", "JobPlanDetailsEditEvent.xml");
            CommonHelper.TraceLine("xml file path is: " + testdatafile);
            CommonHelper.TraceLine("Getting Data from XML file ...");
            DataTable editDt = CommonHelper.BuildDataTableFromXml(testdatafile);

            //Enter data from  xml file
            PlannedEventDataEntry(editDt, 0, "UpdateEvent");

            //Verify Toaster message
            Assert.AreEqual("Updated Successfully", PageJobManagement.toasterMsg_PlannedEventAddSuccess.InnerText, "Planned event is not updated successfully or Toaster message verbiage is incorrect");
            CommonHelper.TraceLine("\"Updated successfully\" toaster message is displayed successfully");

            //Verifying Total Planned Cost and Hour after updating planned event
            string costnHourDetails = PageJobManagement.costnHour.InnerText;
            string[] costnHour = PageJobManagement.GetHeaderDetails(costnHourDetails);
            int totalPlannedCost = 0;
            int totalPlannedHours = 0;
            int updatedEstCost = -1, updatedEstHours = -1;
            for (int i = 0; i < dt.Rows.Count - 1; i++)
            {
                int estCost = -1, estHours = -1;
                int.TryParse(dt.Rows[i]["estimatedtotalcost"].ToString(), out estCost);
                int.TryParse(dt.Rows[i]["estimatedhours"].ToString(), out estHours);
                totalPlannedCost = totalPlannedCost + estCost;
                totalPlannedHours = totalPlannedHours + estHours;
            }
            int.TryParse(editDt.Rows[0]["estimatedtotalcost"].ToString(), out updatedEstCost);
            int.TryParse(editDt.Rows[0]["estimatedhours"].ToString(), out updatedEstHours);
            totalPlannedCost = totalPlannedCost + updatedEstCost;
            totalPlannedHours = totalPlannedHours + updatedEstHours;
            Assert.AreEqual(totalPlannedCost.ToString(), costnHour[0], "Incorrect Total Planned Cost is displayed ");
            Assert.AreEqual(totalPlannedHours.ToString(), costnHour[1], "Incorrect Total Planned Hours is displayed");
            CommonHelper.TraceLine("====>Verifying Total Planned Cost and Hour on Grid title after updating planned event");

            //verify values appeared on Job Plan grid with xml values
            PageJobManagement.VerifyPlannedEventTableCellValues(editDt, rowNo - 1, "UpdateEvent");
            CommonHelper.TraceLine("====>Verified Planned event data present on row " + rowNo + " is correct");
            Thread.Sleep(2000);

            //Verify Update & Delete buttons are being displayed for respective row
            PageJobManagement.VerifyActionButtonsPresence(rowNo);

            //Verify Job Plan Cost value on General tab
            TelerikObject.Click(PageJobManagement.tabGeneral);
            Assert.AreEqual(totalPlannedCost.ToString(), PageJobManagement.txt_jobPlanCost.GetAttribute("ng-reflect-model").Value, "Job Plan Cost value displayed on General tab is incorrect");
            CommonHelper.TraceLine("====>Verified Job Plan Cost on general tab");

            //Navigate back to Job Plan tab
            TelerikObject.Click(PageJobManagement.tabJobPlan);
        }

        public void PlannedEventDataEntry(DataTable dt, int index, string actionName = "CreateEvent")
        {
            TelerikObject.Select_HtmlSelect(PageJobManagement.evtType, dt.Rows[index]["evttype"].ToString());
            if (!actionName.Equals("CreateEvent"))
            {
                TelerikObject.Select_HtmlSelect(PageJobManagement.vendorName_Edit, dt.Rows[index]["vendor"].ToString());
                TelerikObject.Select_HtmlSelect(PageJobManagement.catalog, dt.Rows[index]["catalogitem"].ToString());
                TelerikObject.Select_HtmlSelect(PageJobManagement.truckUnit_Edit, dt.Rows[index]["truckunit"].ToString());
                TelerikObject.Sendkeys(PageJobManagement.txt_estimatedHours_Edit, dt.Rows[index]["estimatedhours"].ToString());
                TelerikObject.Sendkeys(PageJobManagement.txt_accountingId_Edit, dt.Rows[index]["accountingid"].ToString());
                TelerikObject.Sendkeys(PageJobManagement.txt_unitPrice_Edit, dt.Rows[index]["unitprice"].ToString());
            }
            else
            {
                TelerikObject.Select_HtmlSelect(PageJobManagement.vendorName, dt.Rows[index]["vendor"].ToString());
                TelerikObject.Select_HtmlSelect(PageJobManagement.catalog, dt.Rows[index]["catalogitem"].ToString());
                TelerikObject.Select_HtmlSelect(PageJobManagement.truckUnit, dt.Rows[index]["truckunit"].ToString());
                TelerikObject.Sendkeys(PageJobManagement.txt_estimatedHours, dt.Rows[index]["estimatedhours"].ToString());
                TelerikObject.Sendkeys(PageJobManagement.txt_accountingId, dt.Rows[index]["accountingid"].ToString());
                TelerikObject.Sendkeys(PageJobManagement.txt_unitPrice, dt.Rows[index]["unitprice"].ToString());
            }


            TelerikObject.Keyboard_Send(Keys.Tab);
            //verifying that application is calculating correct estimated total cost.  Estimated Total cost=estimated hours * Unit Price
            Thread.Sleep(2000);
            int estHours = -1;
            int UnitPrice = -1;
            int estCost = -1;
            int.TryParse(dt.Rows[index]["estimatedhours"].ToString(), out estHours);
            int.TryParse(dt.Rows[index]["unitprice"].ToString(), out UnitPrice);
            int.TryParse(dt.Rows[index]["estimatedtotalcost"].ToString(), out estCost);
            string CalcTotalCost = (estHours * UnitPrice).ToString();
            string autoCalculatedCost;
            if (!actionName.Equals("CreateEvent"))
            {
                autoCalculatedCost = PageJobManagement.txt_estimatedTotalCost_Edit.BaseElement.GetAttribute("aria-valuenow").Value;
            }
            else
            {
                autoCalculatedCost = PageJobManagement.txt_estimatedTotalCost.ChildNodes[0].GetAttribute("ng-reflect-model").Value;

            }
            Assert.AreEqual(CalcTotalCost, autoCalculatedCost, "Autocalculated cost is incorrect");

            //Overwriting estimated total cost value
            if (!actionName.Equals("CreateEvent"))
            {
                TelerikObject.Sendkeys(PageJobManagement.txt_estimatedTotalCost_Edit, dt.Rows[index]["estimatedtotalcost"].ToString());
                Assert.IsTrue(PageJobManagement.btn_saveOnPopUp.IsEnabled, "Save Button is disabled when all the required fields are filled on Planned event screen");
            }
            else
            {
                TelerikObject.Sendkeys(PageJobManagement.txt_estimatedTotalCost, dt.Rows[index]["estimatedtotalcost"].ToString());
                Assert.IsTrue(PageJobManagement.btn_saveOnPopUp.IsEnabled, "Save Button is disabled when all the required fields are filled on Planned event screen");
            }
            TelerikObject.Sendkeys(PageJobManagement.txt_respPerson, dt.Rows[index]["responsibleperson"].ToString());
            TelerikObject.Sendkeys(PageJobManagement.txt_assignedToPerson, dt.Rows[index]["assignedtoperson"].ToString());
            TelerikObject.Sendkeys(PageJobManagement.txt_briefDescription, dt.Rows[index]["briefdesc"].ToString());
            TelerikObject.Sendkeys(PageJobManagement.txt_remarks1, dt.Rows[index]["remarks"].ToString());
            TelerikObject.Click(PageJobManagement.btn_saveOnPopUp);

        }

        public void DeletePlannedEvent(DataTable dt, int rowNo)
        {
            //Finding Cost & Hour and storing it in some variable 
            //HtmlTable HeaderTable=TelerikObject.getTableByColumnName("Action");
            //int hoursColumnIndex= TelerikObject.getIndexofColumnName(HeaderTable,"Hours");
            //int costColumnIndex = TelerikObject.getIndexofColumnName(HeaderTable, "Cost");
            //string totalHour=TelerikObject.getTextAtIndex(PageJobManagement.table_JobPlan,hoursColumnIndex);
            //string totalCost=TelerikObject.getTextAtIndex(PageJobManagement.table_JobPlan, costColumnIndex);

            int bDeleteEvent = PageJobManagement.table_JobPlan.Rows.Count;
            CommonHelper.TraceLine("====>Number of events present in Job Plan Grid before event deletion :" + bDeleteEvent);
            HtmlSpan deleteButton = (HtmlSpan)TelerikObject.GetElement("HtmlSpan", "xpath", "//td[@ng-reflect-logical-row-index='" + rowNo + "']/button/span[@title='Delete']", "Delete button for Planned Event " + rowNo);
            TelerikObject.mgr.Desktop.Mouse.HoverOver(deleteButton.GetRectangle());
            TelerikObject.Click(deleteButton);
            Thread.Sleep(2000);
            Assert.IsTrue(PageJobManagement.titleOnDeleteEventPopUp.IsVisible(), "Pop up title is missing or incorrect on Delete Event popup");
            CommonHelper.TraceLine("====>Delete Event Verbiage :" + PageJobManagement.verbiageOnDeleteOpUp.InnerText);
            TelerikObject.Click(PageJobManagement.btn_DeleteOnPopUp);
            Assert.AreEqual("Deleted Planned Event Successfully", PageJobManagement.toasterMsg_PlannedEventAddSuccess.InnerText, "Planned event is not deleted successfully or Toaster message verbiage is incorrect");
            CommonHelper.TraceLine("====>Planned event deleted successfully");
            int aDeleteEvent = PageJobManagement.table_JobPlan.Rows.Count;
            CommonHelper.TraceLine("====>Number of events present in Job Plan Grid after event deletion :" + aDeleteEvent);
            Assert.AreEqual((bDeleteEvent - 1), aDeleteEvent, "After Planned event deletion number of records count is not matching");

            //Verifying Total Planned Cost and Hour after deleting a planned event
            string costnHourDetails = PageJobManagement.costnHour.InnerText;
            string[] costnHour = PageJobManagement.GetHeaderDetails(costnHourDetails);
            Assert.AreEqual(dt.Rows[0]["estimatedtotalcost"].ToString(), costnHour[0], "Incorrect Total Planned Cost is displayed ");
            Assert.AreEqual(dt.Rows[0]["estimatedhours"].ToString().ToString(), costnHour[1], "Incorrect Total Planned Hours is displayed");
            CommonHelper.TraceLine("====>Verifying Total Planned Cost and Hour on Grid title after deleting planned event");


            //Verify Job Plan Cost value on General tab after event deletion
            TelerikObject.Click(PageJobManagement.tabGeneral);
            Assert.AreEqual(dt.Rows[0]["estimatedtotalcost"].ToString(), PageJobManagement.txt_jobPlanCost.GetAttribute("ng-reflect-model").Value, "Job Plan Cost value displayed on General tab is incorrect");
            CommonHelper.TraceLine("====>Verified Job Plan Cost on general tab after deleting a planned event");

        }



        public void JobCostDetailsflow()
        {
            //TelerikObject.InitializeManager();
            //TelerikObject.gotoPage(appURL);
            //TelerikObject.DoStaticWait();
            //TelerikObject.Click(PageDashboard.trackingtab);
            //TelerikObject.Click(PageDashboard.jobManagementTab);
            //TelerikObject.DoStaticWait();
            TelerikObject.Click(PageJobManagement.newlyCreatedJob);
            Thread.Sleep(3000);

            CommonHelper.TraceLine("====>Starting Job Cost Details workflow");
            TelerikObject.Click(PageJobManagement.tabGeneral);
            Thread.Sleep(2000);
            int totalJobCost = -1;
            string bTotalCost = PageJobManagement.txt_jobTotalCost.GetAttribute("ng-reflect-model").Value;
            int.TryParse(bTotalCost, out totalJobCost);
            TelerikObject.Click(PageJobManagement.addTab);
            TelerikObject.Click(PageJobManagement.tabJobCostDetails);

            Thread.Sleep(2000);
            string[] cost = PageJobManagement.GetHeaderDetails(PageJobManagement.jobCostDetailsHeader.InnerText);
            Assert.AreEqual(bTotalCost, cost[0].ToString(), "Total Job Cost on Job Cost Details header is incorrect");
            Assert.AreEqual("0", cost[1].ToString(), "Total Job Detail Cost should be 0");
            Assert.IsTrue(PageJobManagement.btn_addOnJobCostDetails.IsEnabled, "Add button on Job Cost Details is not enabled");
            Assert.IsNotNull(PageJobManagement.noDataAvailable, "No data available message is not present");


            //test data from XML
            string testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\TestData", "JobCostDetails.xml");
            CommonHelper.TraceLine("xml file path is: " + testdatafile);
            CommonHelper.TraceLine("Getting Data from XML file ...");
            DataTable dt = CommonHelper.BuildDataTableFromXml(testdatafile);
            string testdatafile1 = Path.Combine(FilesLocation + @"\UIAutomation\TestData", "JobCostDetails_Edit.xml");
            CommonHelper.TraceLine("xml file path is: " + testdatafile1);
            CommonHelper.TraceLine("Getting Data from XML file ...");
            DataTable editDt = CommonHelper.BuildDataTableFromXml(testdatafile1);

            int totalJobCst = totalJobCost;
            int totalJobDetailCst = 0;
            int DeletedJobDetailCost = -1; int updatedJobDetailCost = -1;
            string aTotalCost = null;

            for (int index = 0; index < dt.Rows.Count; index++)
            {
                int totJobDetailCost = -1;
                AddJobCostDetail(dt, index);
                cost = PageJobManagement.GetHeaderDetails(PageJobManagement.jobCostDetailsHeader.InnerText);
                int.TryParse(dt.Rows[index]["cost"].ToString(), out totJobDetailCost);
                totalJobCst = totalJobCst + totJobDetailCost;
                totalJobDetailCst = totalJobDetailCst + totJobDetailCost;
                Assert.AreEqual(totalJobCst.ToString(), cost[0], "Total Job Cost on Job Cost Details header is incorrect after adding Job Cost Detail: " + index);
                Assert.AreEqual(totalJobDetailCst.ToString(), cost[1], "Total Job Detail Cost on Job Cost Details header is incorrect after adding Job Cost Detail: " + index);
                TelerikObject.Click(PageJobManagement.tabGeneral);
                Thread.Sleep(2000);
                aTotalCost = PageJobManagement.txt_jobTotalCost.GetAttribute("ng-reflect-model").Value;
                Assert.AreEqual(totalJobCst.ToString(), aTotalCost, "Total Cost on General tab is incorrect after adding Job Cost Detail :" + index);
                TelerikObject.Click(PageJobManagement.tabJobCostDetails);
                PageJobManagement.VerifyJobDetailCostValues(dt, index);
                PageJobManagement.VerifyActionButtonsPresenceForJCD(dt, index);
            }

            UpdateJobCostDetails(dt, editDt, dt.Rows.Count - 1);
            PageJobManagement.VerifyJobDetailCostValues(editDt, 0, "Update");
            cost = PageJobManagement.GetHeaderDetails(PageJobManagement.jobCostDetailsHeader.InnerText);
            int.TryParse(dt.Rows[dt.Rows.Count - 1]["cost"].ToString(), out DeletedJobDetailCost);
            int.TryParse(editDt.Rows[0]["cost"].ToString(), out updatedJobDetailCost);
            totalJobCst = totalJobCst - DeletedJobDetailCost + updatedJobDetailCost;
            totalJobDetailCst = totalJobDetailCst - DeletedJobDetailCost + updatedJobDetailCost;
            Assert.AreEqual(totalJobCst.ToString(), cost[0], "Total Job Cost on Job Cost Details header is incorrect after updating Job Cost Detail");
            Assert.AreEqual(totalJobDetailCst.ToString(), cost[1], "Total Job Detail Cost on Job Cost Details header is incorrect after updating Job Cost Detail");
            TelerikObject.Click(PageJobManagement.tabGeneral);
            Thread.Sleep(2000);
            aTotalCost = PageJobManagement.txt_jobTotalCost.GetAttribute("ng-reflect-model").Value;
            Assert.AreEqual(totalJobCst.ToString(), aTotalCost, "Total Cost on General tab is incorrect after updating Job Cost Detail");
            TelerikObject.Click(PageJobManagement.tabJobCostDetails);

            CloneJobCostDetails(editDt);
            PageJobManagement.VerifyJobDetailCostValues(editDt, 0, "Cloned");
            PageJobManagement.VerifyJobDetailCostValues(editDt, 1, "Cloned");
            cost = PageJobManagement.GetHeaderDetails(PageJobManagement.jobCostDetailsHeader.InnerText);
            //int.TryParse(dt.Rows[dt.Rows.Count - 1]["cost"].ToString(), out DeletedJobDetailCost);
            int.TryParse(editDt.Rows[0]["cost"].ToString(), out updatedJobDetailCost);
            totalJobCst = totalJobCst + updatedJobDetailCost;
            totalJobDetailCst = totalJobDetailCst + updatedJobDetailCost;
            Assert.AreEqual(totalJobCst.ToString(), cost[0], "Total Job Cost on Job Cost Details header is incorrect after cloning Job Cost Detail");
            Assert.AreEqual(totalJobDetailCst.ToString(), cost[1], "Total Job Detail Cost on Job Cost Details header is incorrect after cloning Job Cost Detail");
            TelerikObject.Click(PageJobManagement.tabGeneral);
            Thread.Sleep(2000);
            aTotalCost = PageJobManagement.txt_jobTotalCost.GetAttribute("ng-reflect-model").Value;
            Assert.AreEqual(totalJobCst.ToString(), aTotalCost, "Total Cost on General tab is incorrect after cloning Job Cost Detail");
            TelerikObject.Click(PageJobManagement.tabJobCostDetails);

            deleteJobDetails(editDt);
            PageJobManagement.VerifyJobDetailCostValues(editDt, 0, "Deleted");
            cost = PageJobManagement.GetHeaderDetails(PageJobManagement.jobCostDetailsHeader.InnerText);
            //int.TryParse(dt.Rows[dt.Rows.Count - 1]["cost"].ToString(), out DeletedJobDetailCost);
            int.TryParse(editDt.Rows[0]["cost"].ToString(), out updatedJobDetailCost);
            totalJobCst = totalJobCst - updatedJobDetailCost;
            totalJobDetailCst = totalJobDetailCst - updatedJobDetailCost;
            Assert.AreEqual(totalJobCst.ToString(), cost[0], "Total Job Cost on Job Cost Details header is incorrect after deleting Job Cost Detail");
            Assert.AreEqual(totalJobDetailCst.ToString(), cost[1], "Total Job Detail Cost on Job Cost Details header is incorrect after deleting Job Cost Detail");
            TelerikObject.Click(PageJobManagement.tabGeneral);
            Thread.Sleep(2000);
            aTotalCost = PageJobManagement.txt_jobTotalCost.GetAttribute("ng-reflect-model").Value;
            Assert.AreEqual(totalJobCst.ToString(), aTotalCost, "Total Cost on General tab is incorrect after deleting Job Cost Detail");
            TelerikObject.Click(PageJobManagement.tabJobCostDetails);

        }


        public void AddJobCostDetail(DataTable dt, int index)
        {
            TelerikObject.Click(PageJobManagement.btn_addOnJobCostDetails);
            Thread.Sleep(2000);
            Assert.AreEqual("Add Job Cost Detail", PageJobManagement.titleOnPopup.InnerText, "Incorrect title displayed on pop up while adding Job cost details");
            Assert.IsFalse(PageJobManagement.btn_saveOnPopUp.IsEnabled, "Save button should be disabled if all the mandatory fields are not filled");
            TelerikObject.Sendkeys(PageJobManagement.txt_date, dt.Rows[index]["date"].ToString().Replace("/", ""), false);
            TelerikObject.Select_HtmlSelect(PageJobManagement.vendorNameOnJobCostDetails, dt.Rows[index]["vendor"].ToString());
            TelerikObject.Select_HtmlSelect(PageJobManagement.catalog, dt.Rows[index]["catalogitem"].ToString());
            TelerikObject.Sendkeys(PageJobManagement.quantityOnJobCostDetails, dt.Rows[index]["quantity"].ToString());
            TelerikObject.Sendkeys(PageJobManagement.txt_unitPrice, dt.Rows[index]["unitprice"].ToString());
            TelerikObject.Sendkeys(PageJobManagement.txt_DiscAmt, dt.Rows[index]["discamount"].ToString(), false);
            TelerikObject.Keyboard_Send(Keys.Tab);


            int quantity = -1;
            int UnitPrice = -1;
            int discamount = -1;
            int.TryParse(dt.Rows[index]["quantity"].ToString(), out quantity);
            int.TryParse(dt.Rows[index]["UnitPrice"].ToString(), out UnitPrice);
            int.TryParse(dt.Rows[index]["discamount"].ToString(), out discamount);
            string calcTotalCost = ((quantity * UnitPrice) - discamount).ToString();
            Assert.AreEqual(calcTotalCost, PageJobManagement.txt_Cost.BaseElement.GetAttribute("ng-reflect-model").Value, "Auto-Calculated cost is incorrect");

            TelerikObject.Sendkeys(PageJobManagement.txt_Cost, dt.Rows[index]["cost"].ToString());
            Assert.IsFalse(PageJobManagement.btn_saveOnPopUp.IsEnabled, "Save button should be disabled if all the mandatory fields are not filled");
            TelerikObject.Sendkeys(PageJobManagement.txt_remarks1, dt.Rows[index]["remarks"].ToString());
            Assert.IsTrue(PageJobManagement.btn_saveOnPopUp.IsEnabled, "Save button should be enabled if all the mandatory fields are filled");
            TelerikObject.Click(PageJobManagement.btn_saveOnPopUp);
            Thread.Sleep(3000);
            Assert.AreEqual("Added Successfully", PageJobManagement.toasterMsg_PlannedEventAddSuccess.InnerText, "Job Cost Detail is not addedd successfully");

        }

        public void UpdateJobCostDetails(DataTable dt, DataTable editDt, int index)
        {
            HtmlTable oldRecord = TelerikObject.getTableByColumnName(dt.Rows[index]["vendor"].ToString());
            // oldRecord.AllRows[0].BaseElement.ChildNodes[4].ChildNodes[7].Children[0].Content

            TelerikObject.Click(oldRecord.BodyRows[0].BaseElement.Children[0].Children[0]);


            //Verify Edit Pop up title
            Assert.AreEqual("Edit Job Cost Detail - " + dt.Rows[index]["vendor"].ToString(), PageJobManagement.titleOnPopup.InnerText, "Title on Edit Job Cost Detail pop up is incorrect");

            //test data from XML

            TelerikObject.Sendkeys(PageJobManagement.txt_date, editDt.Rows[0]["date"].ToString().Replace("/", ""), false);
            TelerikObject.Select_HtmlSelect(PageJobManagement.vendorNameOnJobCostDetails, editDt.Rows[0]["vendor"].ToString());
            TelerikObject.Select_HtmlSelect(PageJobManagement.catalog, editDt.Rows[0]["catalogitem"].ToString());
            TelerikObject.Sendkeys(PageJobManagement.quantityOnJobCostDetails, editDt.Rows[0]["quantity"].ToString());
            TelerikObject.Sendkeys(PageJobManagement.txt_unitPrice, editDt.Rows[0]["unitprice"].ToString());
            TelerikObject.Sendkeys(PageJobManagement.txt_DiscAmt, editDt.Rows[0]["discamount"].ToString());
            TelerikObject.Keyboard_Send(Keys.Tab);


            int quantity = -1;
            int UnitPrice = -1;
            int discamount = -1;
            int.TryParse(editDt.Rows[0]["quantity"].ToString(), out quantity);
            int.TryParse(editDt.Rows[0]["UnitPrice"].ToString(), out UnitPrice);
            int.TryParse(editDt.Rows[0]["discamount"].ToString(), out discamount);
            string calcTotalCost = ((quantity * UnitPrice) - discamount).ToString();
            Assert.AreEqual(calcTotalCost, PageJobManagement.txt_Cost.BaseElement.GetAttribute("ng-reflect-model").Value, "Auto-Calculated cost is incorrect");

            TelerikObject.Sendkeys(PageJobManagement.txt_Cost, editDt.Rows[0]["cost"].ToString());
            TelerikObject.Sendkeys(PageJobManagement.txt_remarks1, editDt.Rows[0]["remarks"].ToString());
            TelerikObject.Click(PageJobManagement.btn_saveOnPopUp);
            Thread.Sleep(3000);
            Assert.AreEqual("Updated Successfully", PageJobManagement.toasterMsg_PlannedEventAddSuccess.InnerText, "Job Cost Detail is not updated successfully");

            PageJobManagement.DynamicValue = editDt.Rows[0]["date"].ToString() + "  -  " + editDt.Rows[0]["cost"].ToString();
            Assert.IsNotNull(PageJobManagement.accordian_eventDate, "Accordian title is incorrect after updaing Job Cost Detail");
        }

        public void CloneJobCostDetails(DataTable editDt)
        {
            int updatedJobDetailCost = -1;
            int.TryParse(editDt.Rows[0]["cost"].ToString(), out updatedJobDetailCost);
            PageJobManagement.DynamicValue = editDt.Rows[0]["date"].ToString() + "  -  " + updatedJobDetailCost.ToString();
            TelerikObject.Click(PageJobManagement.accordian_eventDate);
            HtmlTable oldRecord = TelerikObject.getTableByColumnName(editDt.Rows[0]["vendor"].ToString());
            // oldRecord.AllRows[0].BaseElement.ChildNodes[4].ChildNodes[7].Children[0].Content

            TelerikObject.Click(oldRecord.BodyRows[0].BaseElement.Children[0].Children[2]);
            Thread.Sleep(3000);
            Assert.AreEqual("Cloned successfully", PageJobManagement.toasterMsg_PlannedEventAddSuccess.InnerText, "Job Cost Detail is not cloned successfully");

            PageJobManagement.DynamicValue = editDt.Rows[0]["date"].ToString() + "  -  " + (2 * updatedJobDetailCost).ToString();
            Assert.IsNotNull(PageJobManagement.accordian_eventDate, "Accordian title is incorrect after cloning Job Cost Detail");

        }

        public void deleteJobDetails(DataTable editDt)
        {
            int updatedJobDetailCost = -1;
            int.TryParse(editDt.Rows[0]["cost"].ToString(), out updatedJobDetailCost);
            PageJobManagement.DynamicValue = editDt.Rows[0]["date"].ToString() + "  -  " + (2 * updatedJobDetailCost).ToString();

            TelerikObject.Click(PageJobManagement.accordian_eventDate);
            HtmlTable oldRecord = TelerikObject.getTableByColumnName(editDt.Rows[0]["vendor"].ToString());
            // oldRecord.AllRows[0].BaseElement.ChildNodes[4].ChildNodes[7].Children[0].Content

            TelerikObject.Click(oldRecord.BodyRows[0].BaseElement.Children[0].Children[1]);
            Assert.AreEqual("Delete", PageJobManagement.titleOnPopup.InnerText, "Title on Delete Job Cost Detail pop up is incorrect");
            string deleteConfirmation = "Are you sure you want to delete the Job Cost Detail " + editDt.Rows[0]["vendor"].ToString() + " ? Deleting this data is not recoverable and deletes its instance from historical data.";
            CommonHelper.TraceLine("====>Delete Event Verbiage :" + PageJobManagement.verbiageOnDeleteOpUp.InnerText);
            Assert.AreEqual(deleteConfirmation, PageJobManagement.verbiageOnDeleteOpUp.InnerText, "Verbiage on Delete Job Cost Detail pop up is incorrect");
            TelerikObject.Click(PageJobManagement.btn_DeleteOnPopUp);
            Thread.Sleep(3000);
            Assert.AreEqual("Deleted Successfully", PageJobManagement.toasterMsg_PlannedEventAddSuccess.InnerText, "Job Cost Detail is not deleted successfully");

            PageJobManagement.DynamicValue = editDt.Rows[0]["date"].ToString() + "  -  " + updatedJobDetailCost.ToString();
            Assert.IsNotNull(PageJobManagement.accordian_eventDate, "Accordian title is incorrect after deleting Job Cost Detail");
        }


        public void CreateNewWellonBlankDB()
        {
            TelerikObject.InitializeManager();
            CommonHelper.CreateNewRRLWellwithFullData();
            TelerikObject.gotoPage(appURL);
            TelerikObject.DoStaticWait();
            CommonHelper.TraceLine("Newly Created well was saved");
        }
        public void LaunchForeSiteNoWellCreation()
        {
            TelerikObject.InitializeManager();
        }
        public void DeleteNewWellonBlankDB()
        {

            TelerikObject.Click(PageDashboard.wellConfigurationtab);
            TelerikObject.deleteSelectedWell();
        }

        public void GotoJobManagement()
        {
            //TelerikObject.DoStaticWait();
            TelerikObject.Click(PageDashboard.trackingtab);
            TelerikObject.Click(PageDashboard.jobManagementTab);
            CommonHelper.TraceLine("Launched Job Management Tab");
        }

        public void AttachDocument()
        {
            TelerikObject.WaitForControlDisappear(new HtmlControl(PageDashboard.page_Loader));
            int scroll_Height = 100, i = 1, fileCount = 0;
            string attachmentname = "ForeSiteSplashTrans.png";

            TelerikObject.Mouse_Click(PageJobManagement.tables_GeneralTab.ElementAt(2).Children[1].Children[0].Children[0]);
            //TelerikObject.Click(PageJobManagement.btnAttachDoc);
            Assert.IsFalse(new HtmlControl(PageJobManagement.btn_Download_attachment).IsEnabled, "Download Attachment button is not Disabled");
            Trace.Write("Ensuring that Download attachment button is disabled");
            Assert.IsFalse(new HtmlControl(PageJobManagement.btn_Delete_attachment).IsEnabled, "Delete Attachment button is not Disabled");
            Trace.Write("Ensuring that Delete attachment button is disabled");
            int category_count = PageJobManagement.documentManager_Categories.Count();
            CommonHelper.TraceLine("Total number of Document categories :" + category_count);
            Assert.AreEqual(category_count, PageJobManagement.btns_AddFile.Count(), "Categories count & Add File buttons count are not matching");

            foreach (HtmlControl folder in PageJobManagement.documentManager_Categories)
            {
                CommonHelper.TraceLine("Text of Document category " + i + " : " + folder.BaseElement.InnerText);
                Assert.IsTrue(PageJobManagement.btns_AddFile.ElementAt(i - 1).IsEnabled, "Add File button is disabled for category : " + folder.BaseElement.InnerText);
                CommonHelper.TraceLine("Add File button is enabled for document category : " + folder.BaseElement.InnerText);
                string filepath = Path.Combine(FilesLocation + @"\UIAutomation\TestData", attachmentname);
                TelerikObject.Click(PageJobManagement.btns_AddFile[i - 1]);
                TelerikObject.mgr.DialogMonitor.AddDialog(new FileUploadDialog(TelerikObject.mgr.ActiveBrowser, filepath, DialogButton.OPEN, "Open"));
                TelerikObject.mgr.DialogMonitor.Start();
                Assert.AreEqual(PageJobManagement.toasterMsgAttachDocSucc.InnerText, "Uploaded document successfully");
                Thread.Sleep(5000);
                TelerikObject.scrollMouseWheelDown(scroll_Height);
                scroll_Height = scroll_Height + 200;
                i = i + 1;
            }
            TelerikObject.scrollMouseWheelDown(scroll_Height, "up");
            TelerikObject.Click(PageJobManagement.closeAttachDocModal);
            string[] attachmentCount = (PageJobManagement.attachmentCount.Attributes.FirstOrDefault(x => x.Name == "title").Value).Split(':');
            CommonHelper.TraceLine("Attachment Count : " + attachmentCount[1].Trim());
            Assert.AreEqual((i - 1).ToString(), attachmentCount[1].Trim(), "Attachment count is incorrect");
            TelerikObject.Mouse_Click(PageJobManagement.tables_GeneralTab.ElementAt(2).Children[1].Children[0].Children[0]);
            //TelerikObject.Click(PageJobManagement.btnAttachDoc);
            foreach (Element attachedFile in PageJobManagement.attachedFileCollection)
            {
                if (attachedFile.InnerText.Equals(attachmentname))
                {
                    fileCount = fileCount + 1;
                }
            }
            Assert.AreEqual((i - 1), fileCount, "Uploaded files count is not correct");

            ReadOnlyCollection<HtmlInputCheckBox> checkboxCollection = TelerikObject.mgr.ActiveBrowser.Find.AllByAttributes<HtmlInputCheckBox>("class=ng-untouched ng-pristine ng-valid");

            for (int j = 0; j < checkboxCollection.Count; j++)
            {
                checkboxCollection[j].Check(true);

                Assert.IsTrue(new HtmlControl(PageJobManagement.btn_Download_attachment).IsEnabled, "Download Attachment button is not Enabled");
                Trace.Write("Ensuring that Download attachment button is enabled");
                Assert.IsTrue(new HtmlControl(PageJobManagement.btn_Delete_attachment).IsEnabled, "Delete Attachment button is not Enabled");
                Trace.Write("Ensuring that Delete attachment button is enabled");
            }
            TelerikObject.Click(PageJobManagement.btn_Delete_attachment);
            TelerikObject.Click(PageJobManagement.btnYesOnDeleteAttach);
            PageJobManagement.DynamicValue = attachmentname;
            string delattachmessage = PageJobManagement.toasterMsgDeleteAttachDocSucc.InnerText;
            CommonHelper.TraceLine("Delete message for attchhment is: " + delattachmessage);
            Assert.AreEqual(delattachmessage, attachmentname + " deleted Successfully");
            Thread.Sleep(3000);
            TelerikObject.Click(PageJobManagement.closeAttachDocModal);
            attachmentCount = (PageJobManagement.attachmentCountAfterDeletion.Attributes.FirstOrDefault(x => x.Name == "title").Value).Split(':');
            Assert.AreEqual("0", attachmentCount[1].Trim(), "Attachment count should be 0");
        }



        public void CreateEvent()
        {
            CommonHelper.TraceLine("[" + DateTime.Now.ToString() + " : ] **** Create Event test has started ********  ");
            //TelerikObject.InitializeManager();
            //TelerikObject.gotoPage(appURL);
            //TelerikObject.DoStaticWait();
            ////test data from XML
            string testdatafile = "";
            DataTable dt = null;
            DataTable dte = null;
            string streventType = "";
            string strGeneralTotalCost = "";
            //TelerikObject.Click(PageDashboard.trackingtab);
            //TelerikObject.Click(PageDashboard.jobManagementTab);
            TelerikObject.Click(PageJobManagement.newlyCreatedJob);
            TelerikObject.Click(PageJobManagement.tabEvents);

            ////*******************************************Step 1: Add Single Event ***********************************
            CommonHelper.TraceLine("Step1: ====== Adding Event with Required + Additional Fields ======  ");
            CommonHelper.TraceLine("=====Checking for Pre conditions for buttons before Event creation  ======");
            PageJobManagement.JobBeforeEventAddAssertions();
            testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\TestData", "JobEventDetailsAll.xml");
            CommonHelper.TraceLine("xml file path is: " + testdatafile);
            dt = CommonHelper.BuildDataTableFromXml(testdatafile);
            AddAdditionalEventFields(dt);
            // Edit Modify Button And Verify that Values are seen on Edit Dialog        
            PageJobManagement.DynamicValue = "Acidize";
            //click on Modify Single Event Dialog
            TelerikObject.Click(PageJobManagement.row_selector_Modifyfor_EventType);
            ////*********************** Step 8:   Modify Data and check if it gets saved
            CommonHelper.TraceLine("Verifying the Save Event Values in Edit Dialog ");

            testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\TestData", "JobEventDetailsAll.xml");
            CommonHelper.TraceLine("xml file path is: " + testdatafile);
            CommonHelper.TraceLine("Getting Data from XML file ...");
            dt = CommonHelper.BuildDataTableFromXml(testdatafile);
            Assert.IsFalse(PageJobManagement.txt_EventType.IsEnabled, "In Modify Mode Event Type is  Enabled");
            streventType = PageJobManagement.txt_EventType.BaseElement.Parent.GetAttribute("ng-reflect-user-input").Value;
            CommonHelper.TraceLine("Value shown on UI for Acidize Event is : " + streventType);
            Assert.AreEqual("Acidize", streventType, "When Selected Event Type Acidize it did not show up on UI");
            //Verify Values on Edit Dialog 
            CommonHelper.TraceLine("***********Verify if Values are saved on Edit/Modify  Event Dialog");
            VerifySaveOnModifyEventDialog(dt);
            //*******************************************Step 2: Verify Single Event ***********************************
            CommonHelper.TraceLine("Step2: ====== Verify Single Event Addition on Grid ======  ");
            PageJobManagement.VerifyEventTableCellValues(dt);

            //*********Step 3 : save event for mass edit is now enabled: ****************************************
            CommonHelper.TraceLine("Step3: ======Verifying the Event Grid Quick Edit Or Mass Edit Feature");
            testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\TestData", "JobEventDetailsAllQE.xml");
            CommonHelper.TraceLine("xml file path is: " + testdatafile);
            CommonHelper.TraceLine("Getting Data from XML file ...");
            dte = CommonHelper.BuildDataTableFromXml(testdatafile);
            PageJobManagement.EnterGridCellEdit(dte, dt);
            CommonHelper.TraceLine("Checking for Button State during Quick Edit ");
            Assert.IsTrue(PageJobManagement.btnSaveEventMain.IsEnabled, "Save Button is not enabled during  quick edit");
            Assert.IsTrue(PageJobManagement.btnCancelEvent.IsEnabled, "Cancel Button is not enabled during  quick edit");
            PageJobManagement.DynamicValue = "Acidize";

            Assert.IsFalse(new HtmlButton(PageJobManagement.row_selector_Clonefor_EventType.Parent).IsEnabled, "Clone Is Enabled enabled during quick edit");
            CommonHelper.TraceLine("Clone is Disabled during quick edit");
            Assert.IsFalse(new HtmlButton(PageJobManagement.row_selector_Deletefor_EventType.Parent).IsEnabled, "Delete Button is enabled during  quick edit");
            CommonHelper.TraceLine("Delete is Disabled during quick edit");
            Assert.IsFalse(new HtmlButton(PageJobManagement.row_selector_Modifyfor_EventType.Parent).IsEnabled, "Modufy Button is enabled during  quick edit");
            CommonHelper.TraceLine("Modify  is Disabled during quick edit");

            TelerikObject.Click(PageJobManagement.btnSaveEventMain);
            Assert.IsNotNull(PageJobManagement.toastermsg_EventSaveSuccess, "Toaster Message for success  did not appaer after mass edit");
            Thread.Sleep(5000);
            PageJobManagement.DynamicValue = dte.Rows[0]["eventbeindate"].ToString();
            TelerikObject.Click(PageJobManagement.accordian_eventDate);
            PageJobManagement.VerifyEventTableCellValues(dte, true);

            //Step 4: ********* Verify if Job cost Detaisl are reflected on Job Management Tab
            CommonHelper.TraceLine("Step 4: Verifying if...cost is refleted in  Job Management Tab ");
            TelerikObject.Click(PageJobManagement.tabGeneral);
            CommonHelper.TraceLine("Verifying if Total Cost is refleceted in General");
            strGeneralTotalCost = PageJobManagement.txt_jobTotalCost.GetAttribute("ng-reflect-model").Value;
            Assert.AreEqual(PageJobManagement.jobTotalCost, strGeneralTotalCost, "Job Cost is not updated in Generla Tab");
            TelerikObject.Click(PageJobManagement.tabEvents);

            //Step 5 :  *********  Delete Single Event Added ***********************************************
            CommonHelper.TraceLine("*****************Step 5: Deleteing Single Event Added....******************** ");
            PageJobManagement.DynamicValue = dte.Rows[0]["jobeventtype"].ToString();
            PageJobManagement.DynamicValue = dte.Rows[0]["eventbeindate"].ToString();
            TelerikObject.Click(PageJobManagement.accordian_eventDate);
            TelerikObject.Click(PageJobManagement.row_selector_Deletefor_EventType);
            Assert.IsNotNull(PageJobManagement.btn_confirmDeleteEvent, "Confirm Delelte button is Not shown");
            CommonHelper.TraceLine("Confirm Delete Event is shown");
            TelerikObject.Click(PageJobManagement.btn_confirmDeleteEvent);
            Assert.IsNotNull(PageJobManagement.toaster_msgDeleteEventSuccess, "Delete Event Success was not shown on UI");
            CommonHelper.TraceLine("Single Event added was Deleted");


            //Step 6: ********* Verify Multi Add Event Feature: ****************************************
            CommonHelper.TraceLine("*****************Step 6 : Verify Multi Add Event Feature******************** ");
            testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\TestData", "JobEventDetails_MultiAdd.xml");
            dt = CommonHelper.BuildDataTableFromXml(testdatafile);
            TelerikObject.Click(PageJobManagement.btnMultiAddEvent);
            Assert.IsFalse(PageJobManagement.btn_addEventMultiAdd.IsEnabled, " Event Add button is enabled on load ");
            Assert.IsFalse(PageJobManagement.btn_removeEventMultiAdd.IsEnabled, "Event Remove button is enabled on load");
            Assert.IsFalse(PageJobManagement.btn_saveEventMultiAdd.IsEnabled, "Event Save button for Multi Add is enabled om load ");
            CommonHelper.TraceLine(" Add Event , Remove Event and Save are disabled before adding event in Multi Add Dialog  ");
            TelerikObject.Sendkeys(PageJobManagement.txt_BeginDateMultiAdd, dt.Rows[0]["eventbeindate"].ToString().Replace("/", ""), false);
            TelerikObject.Select_HtmlSelect(PageJobManagement.dd_ServiceProviderMultiAdd, dt.Rows[0]["serviceprovider"].ToString());
            TelerikObject.Select_HtmlSelect(PageJobManagement.dd_truckUnitMultiAdd, dt.Rows[0]["truckunitid"].ToString());
            TelerikObject.Select_HtmlSelect(PageJobManagement.dd_costCategory_MultiAdd, dt.Rows[0]["costcategory"].ToString());
            TelerikObject.Sendkeys(PageJobManagement.txt_rate_MultiAdd, dt.Rows[0]["eventcostrate"].ToString());


            //select multi Events 
            CommonHelper.TraceLine("********************Selecting Mutiple Events....************");
            TelerikObject.Click(PageJobManagement.chbox_bundletest, true, true);
            TelerikObject.Click(PageJobManagement.chbox_chemAnalysis, true, true);
            TelerikObject.Click(PageJobManagement.chbox_wirelienPerforate, true, true);
            TelerikObject.Click(PageJobManagement.btn_addEventMultiAdd);
            TelerikObject.Click(PageJobManagement.chbox_selectedEventsSelectall);
            TelerikObject.Click(PageJobManagement.btn_saveEventMultiAdd, true, true);
            CommonHelper.TraceLine("********************Ensuring that toaster for save for Multi Add Appears ************");
            Assert.IsNotNull(PageJobManagement.toaster_msgAddEventSuccess, "Toaster Message for adding Multi Add did not  appear on UI");
            CommonHelper.TraceLine("*** Toaster Mesage Success for  Multi Add  has appaered on UI *********");
            PageJobManagement.DynamicValue = dt.Rows[0]["eventbeindate"].ToString();
            TelerikObject.Click(PageJobManagement.accordian_eventDate);
            PageJobManagement.VerifyMultiAdd(dt);


            //********* Step 7 : Verify Clone Event Feature ******************************
            //Verify Total cost for Multi Edit Feature is reflected on Job Cost or not 

            CommonHelper.TraceLine("*****************Step 7 : Verify the Clone and Paste Feature for Events ******************** ");
            VerifyClonePasteFeature(dt);
            PageJobManagement.DynamicValue = "Bundle Test";
            TelerikObject.Click(PageJobManagement.row_selector_Modifyfor_EventType);
            ////*********************** Step 8:   Modify Data and check if it gets saved
            CommonHelper.TraceLine("*****************Step 8  : Modify Data and check if it gets saved for Multi Add Event Record  ******************** ");
            CommonHelper.TraceLine("Modifying the Event with Additional Fields ");

            testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\TestData", "JobEventDetailsAllModify.xml");
            CommonHelper.TraceLine("xml file path is: " + testdatafile);
            CommonHelper.TraceLine("Getting Data from XML file ...");
            dt = CommonHelper.BuildDataTableFromXml(testdatafile);
            Assert.IsFalse(PageJobManagement.txt_EventType.IsEnabled, "In Modify Mode Event Type is  Enabled");
            CommonHelper.TraceLine("Event Type is Disabled in Edit Mode for Event Type = Bundle Test");
            streventType = PageJobManagement.txt_EventType.BaseElement.Parent.GetAttribute("ng-reflect-user-input").Value;
            CommonHelper.TraceLine("[**Verification Point :**] Value shown on UI for Bundle Test is : " + streventType);
            Assert.AreEqual("Bundle Test", streventType, "When Selected Event Type Bundle Test it did not show up on UI");


            ModifyEventFields(dt);
            CommonHelper.TraceLine("*****************Step 9  : Verify Modified Data on Grid Row  ******************** ");
            PageJobManagement.VerifyEventTableCellValues(dt, false, 1);

            //Step 10:  *********Check For Cancel For QE Make Changes and Cancel nothing should change
            // Not able to Enter Date start date 5/14/2018  and end date as 5/21/2018 as Validation does not allow so created another data file
            CommonHelper.TraceLine("*****************Step 10   :Check For Cancel For QE Make Changes and Cancel nothing should change ******************** ");
            testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\TestData", "JobEventDetailsAllQE_CancelBefore.xml");
            CommonHelper.TraceLine("xml file path is: " + testdatafile);
            CommonHelper.TraceLine("Getting Data from XML file ...");
            dte = CommonHelper.BuildDataTableFromXml(testdatafile);
            PageJobManagement.EnterGridCellEdit(dte, dt);
            TelerikObject.Click(PageJobManagement.btnCancelEvent);
            PageJobManagement.DynamicValue = "Unsaved data will be lost if you click OK to proceed or close the dialog box to undo cancel.";
            Assert.IsNotNull(PageJobManagement.text_msgCancelGridEditText, "Alert Message was not as per requierment for Cancel Mass Edit");
            Assert.IsNotNull(PageJobManagement.btn_confirmOKOnQECancel, "Cancel Confirmation Dialog did not appear on UI");
            CommonHelper.TraceLine("On Cancel Event Confirmation Text Pop up was displayed on UI ");
            TelerikObject.Click(PageJobManagement.btn_confirmOKOnQECancel);
            TelerikObject.DoStaticWait();
            testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\TestData", "JobEventDetailsAllQE_Cancel.xml");
            CommonHelper.TraceLine("xml file path is: " + testdatafile);
            CommonHelper.TraceLine("Getting Data from XML file ...");
            dt = CommonHelper.BuildDataTableFromXml(testdatafile);

            PageJobManagement.DynamicValue = dt.Rows[0]["eventbeindate"].ToString();
            TelerikObject.Click(PageJobManagement.accordian_eventDate);
            PageJobManagement.VerifyEventTableCellValues(dt, false);

            //Step 11: Delete 4 events Added from multi
            CommonHelper.TraceLine("*****************Step 11   :Delete 4 events Added from multi and Masss Edit 2 Rows to Verify if data is saved ******************** ");
            TelerikObject.Click(PageJobManagement.tabGeneral);
            TelerikObject.Click(PageJobManagement.tabEvents);
            //Accordina should get collpased 
            DeleteMultiEvents(0, 4);
            CommonHelper.TraceLine("Ensure If Mass Edit for multiple cell works");
            testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\TestData", "JobEventDetails_MultiAdd_MultiRow.xml");
            dt = CommonHelper.BuildDataTableFromXml(testdatafile);
            testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\TestData", "JobEventDetailsAllQEMulti.xml");
            CommonHelper.TraceLine("xml file path is: " + testdatafile);
            CommonHelper.TraceLine("Getting Data from XML file ...");
            dte = CommonHelper.BuildDataTableFromXml(testdatafile);
            Thread.Sleep(8000);
            PageJobManagement.DynamicValue = dt.Rows[0]["eventbeindate"].ToString();
            TelerikObject.Click(PageJobManagement.accordian_eventDate);
            for (int i = 0; i < 2; i++)
            {
                PageJobManagement.EnterGridCellEdit(dte, dt, i);
            }
            TelerikObject.Click(PageJobManagement.btnSaveEventMain);
            // 
            Assert.IsNotNull(PageJobManagement.toastermsg_EventSaveSuccess, "Toaster Message for success  did not appaer after mass edit");
            Thread.Sleep(5000);
            PageJobManagement.DynamicValue = dte.Rows[0]["eventbeindate"].ToString();
            TelerikObject.Click(PageJobManagement.accordian_eventDate);
            for (int i = 0; i < 2; i++)
            {
                PageJobManagement.VerifyEventTableCellValues(dte, true, i);
            }
            //Step 11: Delete 2 events Modififed from multi Add.

            //*******************verify for multi job totla cost ges updated with values from Events*****************

            string row1cost = TelerikObject.GetCellValue("Total Cost", "Alten", 0);
            int irow1cost = Convert.ToInt32(row1cost);
            string row2cost = TelerikObject.GetCellValue("Total Cost", "Alten", 1);
            int irow2cost = Convert.ToInt32(row1cost);
            TelerikObject.Click(PageJobManagement.tabGeneral);
            CommonHelper.TraceLine("Verifying if Total Cost is refleceted in General");
            strGeneralTotalCost = PageJobManagement.txt_jobTotalCost.GetAttribute("ng-reflect-model").Value;

            Assert.AreEqual((irow1cost + irow2cost).ToString(), strGeneralTotalCost, "Job Cost is not updated in General Tab");
            CommonHelper.TraceLine(string.Format("[**Verification Point :**]Total Cost on General  Tab : {0} from Sum of Grid sums {1} ", strGeneralTotalCost, (irow1cost + irow2cost).ToString()));
            TelerikObject.Click(PageJobManagement.tabEvents);
            //*******************verify for multi job totla cost ges updated with values from Events*****************

            DeleteMultiEvents(0, 2);
            Assert.IsNull(PageJobManagement.accordian_eventDate, "Date Accordian was visiable even after delelting event:");
            CommonHelper.TraceLine("Post deletion of event  Date Accordian is not visible");
            CommonHelper.TraceLine("[" + DateTime.Now.ToString() + " : ] ****Event Workflow test has ended ********  ");

        }


        public void AddRequiredEventFields(DataTable dt)
        {

            //click on Add Single Event Button:

            //Enter All required values for Event Type 
            TelerikObject.Sendkeys(PageJobManagement.txt_EventType, dt.Rows[0]["jobeventtype"].ToString());
            PageJobManagement.DynamicValue = dt.Rows[0]["jobeventtype"].ToString();
            Thread.Sleep(3000);
            TelerikObject.Keyboard_Send(Keys.Enter);
            Thread.Sleep(1000);
            TelerikObject.Sendkeys(PageJobManagement.txt_duartionDays, dt.Rows[0]["durationhours"].ToString());
            //eventbeindate ,eventenddate
            TelerikObject.Sendkeys(PageJobManagement.txt_eventBeginDate, dt.Rows[0]["eventbeindate"].ToString().Replace("/", ""), false);
            TelerikObject.TypeText(dt.Rows[0]["eventbeintime"].ToString().Replace(":", ""));
            Thread.Sleep(1000);
            //   TelerikObject.Keyboard_Send(Keys.Tab);
            TelerikObject.Sendkeys(PageJobManagement.txt_eventEndDate, dt.Rows[0]["eventenddate"].ToString().Replace("/", ""), false);
            TelerikObject.TypeText(dt.Rows[0]["eventendtime"].ToString().Replace(":", ""));
            Thread.Sleep(1000);
            TelerikObject.SetAutoCompleteSelectwithForAttribute("evcFK_AFE", dt.Rows[0]["afe"].ToString(), "AFE");
            TelerikObject.SetAutoCompleteSelectwithForAttribute("evcFK_BusinessOrganization", dt.Rows[0]["serviceprovider"].ToString(), "Service Provider");
            TelerikObject.SetAutoCompleteSelectwithForAttribute("evcFK_r_CatalogItem", dt.Rows[0]["costcategory"].ToString(), "cost Categroy");
            TelerikObject.SetAutoCompleteSelectwithForAttribute("evcFK_r_TruckUnit", dt.Rows[0]["truckunitid"].ToString(), "Truck Unit/ID");
            TelerikObject.Sendkeys(PageJobManagement.txt_totalCost, dt.Rows[0]["totalcost"].ToString());
            //Save Event
            TelerikObject.Click(PageJobManagement.btn_saveEvent);
            CommonHelper.TraceLine("Waiting for Save Event Success Toaster Message ");
            Assert.IsNotNull(PageJobManagement.toaster_msgAddEventSuccess, "Toaster Message of Add Event Success was not shown in UI");
            CommonHelper.TraceLine("Verifying Table Values on UI for Events Saved ");
            Thread.Sleep(2000);
            PageJobManagement.DynamicValue = dt.Rows[0]["eventbeindate"].ToString();
            TelerikObject.Click(PageJobManagement.accordian_eventDate);
        }

        public void AddAdditionalEventFields(DataTable dt)
        {
            //click on Add Single Event All  Button:
            TelerikObject.Click(PageJobManagement.btnAddEvent);
            //Enter All required values for Event Type 
            string fchar = dt.Rows[0]["jobeventtype"].ToString().Substring(0, 1);
            string rchars = dt.Rows[0]["jobeventtype"].ToString().Substring(1);
            TelerikObject.Sendkeys(PageJobManagement.txt_EventType, fchar, false);
            Thread.Sleep(5000);
            TelerikObject.TypeText(rchars);
            PageJobManagement.DynamicValue = dt.Rows[0]["jobeventtype"].ToString();
            Thread.Sleep(3000);
            TelerikObject.Keyboard_Send(Keys.Enter);
            Thread.Sleep(1000);
            TelerikObject.Sendkeys(PageJobManagement.txt_duartionDays, dt.Rows[0]["durationhours"].ToString());
            //eventbeindate ,eventenddate
            TelerikObject.Sendkeys(PageJobManagement.txt_eventBeginDate, dt.Rows[0]["eventbeindate"].ToString().Replace("/", ""), false);
            TelerikObject.TypeText(dt.Rows[0]["eventbeintime"].ToString().Replace(":", ""));
            Thread.Sleep(1000);
            //   TelerikObject.Keyboard_Send(Keys.Tab);
            TelerikObject.Sendkeys(PageJobManagement.txt_eventEndDate, dt.Rows[0]["eventenddate"].ToString().Replace("/", ""), false);
            TelerikObject.TypeText(dt.Rows[0]["eventendtime"].ToString().Replace(":", ""));
            Thread.Sleep(1000);
            TelerikObject.SetAutoCompleteSelectwithForAttribute("evcFK_AFE", dt.Rows[0]["afe"].ToString(), "AFE");
            TelerikObject.SetAutoCompleteSelectwithForAttribute("evcFK_BusinessOrganization", dt.Rows[0]["serviceprovider"].ToString(), "Service Provider");
            TelerikObject.SetAutoCompleteSelectwithForAttribute("evcFK_r_CatalogItem", dt.Rows[0]["costcategory"].ToString(), "cost Categroy");
            TelerikObject.SetAutoCompleteSelectwithForAttribute("evcFK_r_TruckUnit", dt.Rows[0]["truckunitid"].ToString(), "Truck Unit/ID");
            TelerikObject.Sendkeys(PageJobManagement.txt_totalCost, dt.Rows[0]["totalcost"].ToString());

            // Click on  "Addtional Link"
            TelerikObject.Click(PageJobManagement.link_Additional, false);
            TelerikObject.Sendkeys(PageJobManagement.txt_fieldServiceOrderID, dt.Rows[0]["fieldserviceorderid"].ToString());
            TelerikObject.Sendkeys(PageJobManagement.txt_perosnPerformingTask, dt.Rows[0]["personperformingtask"].ToString());
            TelerikObject.SetRadioButtonwithText(PageJobManagement.radiogp_preventiveRadioGroup, dt.Rows[0]["preventivemaintanance"].ToString());
            TelerikObject.Sendkeys(PageJobManagement.txt_evntquantity, dt.Rows[0]["quantity"].ToString());
            TelerikObject.Sendkeys(PageJobManagement.txt_evntremarks, dt.Rows[0]["remarks"].ToString());
            TelerikObject.Sendkeys(PageJobManagement.txt_responsilbePerson, dt.Rows[0]["responsibleperson"].ToString());
            TelerikObject.SetRadioButtonwithText(PageJobManagement.radio_trouble, dt.Rows[0]["trouble"].ToString());
            TelerikObject.SetRadioButtonwithText(PageJobManagement.radio_unplannedEvent, dt.Rows[0]["unplannedevent"].ToString());
            PageJobManagement.txt_evcHistoricalRate.ScrollToVisible();
            TelerikObject.Sendkeys(PageJobManagement.txt_evcWorkorderID, dt.Rows[0]["workorderid"].ToString());
            TelerikObject.Sendkeys(PageJobManagement.txt_evcHistoricalRate, dt.Rows[0]["eventcostrate"].ToString());

            // Click on  "Extended Evetns  Link"
            TelerikObject.Click(PageJobManagement.link_Extedned, false);
            //eacVendorProduct
            PageJobManagement.txt_VendorProduct.ScrollToVisible();
            TelerikObject.Sendkeys(PageJobManagement.txt_VendorProduct, dt.Rows[0]["eventvendorproduct"].ToString());

            //Save Event
            TelerikObject.Click(PageJobManagement.btn_saveEvent);
            CommonHelper.TraceLine("Waiting for Save Event Success Toaster Message ");
            Assert.IsNotNull(PageJobManagement.toaster_msgAddEventSuccess, "Toaster Message of Add Event Success was not shown in UI");
            CommonHelper.TraceLine("Verifying Table Values on UI for Events Saved ");
            Thread.Sleep(2000);

            PageJobManagement.DynamicValue = dt.Rows[0]["eventbeindate"].ToString();
            TelerikObject.Click(PageJobManagement.accordian_eventDate);
            //Verify if Modify , Delete , clone buttons are avialble in Events Grid:
            Assert.IsTrue(PageJobManagement.btn_gridCellModify.IsEnabled, "Grid cell Modify is not enabled after adding record ");
            Assert.IsTrue(PageJobManagement.btn_gridCellDelete.IsEnabled, "Grid cell Delete is not enabled after adding record ");
            Assert.IsTrue(PageJobManagement.btn_gridCellClone.IsEnabled, "Grid cell Clone is not enabled after adding record ");

        }

        public void VerifySaveOnModifyEventDialog(DataTable dt)
        {
            //Enter All required values for Event Type 


            string actdurationdays = PageJobManagement.txt_duartionDays.Children[0].Children[0].GetAttribute("aria-valuenow").Value;
            Assert.AreEqual(dt.Rows[0]["durationhours"].ToString(), actdurationdays, "Duration Hours Mismatch");
            CommonHelper.TraceLine(string.Format("Duration Hours Expected Value {0} and Actual Value{1}", dt.Rows[0]["durationhours"].ToString(), actdurationdays));

            //eventbeindate ,eventenddate
            string actbegindate = PageJobManagement.txt_eventBeginDate.BaseElement.Children[0].Children[0].Children[0].Children[0].Children[0].GetAttribute("aria-valuetext").Value;

            //  string actenddate = "";
            Assert.AreEqual(dt.Rows[0]["eventbeindate"].ToString() + " " + dt.Rows[0]["eventbeintime"].ToString(), actbegindate, "Begin Date Hours Mismatch");
            CommonHelper.TraceLine(string.Format("Begin date  Hours Expected Value {0} and Actual Value{1}", dt.Rows[0]["eventbeindate"].ToString() + " " + dt.Rows[0]["eventbeintime"].ToString(), actbegindate));

            string actedndate = PageJobManagement.txt_eventEndDate.BaseElement.Children[0].Children[0].Children[0].Children[0].Children[0].GetAttribute("aria-valuetext").Value;
            Assert.AreEqual(dt.Rows[0]["eventenddate"].ToString() + " " + dt.Rows[0]["eventendtime"].ToString(), actedndate, "End Date Hours Mismatch");
            CommonHelper.TraceLine(string.Format("End Duration Hours Expected Value {0} and Actual Value{1}", dt.Rows[0]["eventenddate"].ToString() + " " + dt.Rows[0]["eventendtime"], actedndate));

            //
            PageJobManagement.DynamicValue = "evcFK_AFE";
            string actafe = PageJobManagement.dd_autocomplete.BaseElement.Children[0].Children[0].Children[0].GetAttribute("ng-reflect-user-input").Value;
            Assert.AreEqual(dt.Rows[0]["afe"].ToString(), actafe, "AFE Mismatch");
            CommonHelper.TraceLine(string.Format("AFE Expected Value {0} and Actual Value{1}", dt.Rows[0]["afe"].ToString(), actafe));


            PageJobManagement.DynamicValue = "evcFK_BusinessOrganization";
            string actserviceprovider = PageJobManagement.dd_autocomplete.BaseElement.Children[0].Children[0].Children[0].GetAttribute("ng-reflect-user-input").Value;
            Assert.AreEqual(dt.Rows[0]["serviceprovider"].ToString(), actserviceprovider, "Business Organization Mismatch");
            CommonHelper.TraceLine(string.Format("Business Organization Expected Value {0} and Actual Value{1}", dt.Rows[0]["serviceprovider"].ToString(), actserviceprovider));

            PageJobManagement.DynamicValue = "evcFK_r_CatalogItem";
            string actcostcategroy = PageJobManagement.dd_autocomplete.BaseElement.Children[0].Children[0].Children[0].GetAttribute("ng-reflect-user-input").Value;
            Assert.AreEqual(dt.Rows[0]["costcategory"].ToString(), actcostcategroy, " Catalog Item  Mismatch");
            CommonHelper.TraceLine(string.Format("Catalog Item  Expected Value {0} and Actual Value{1}", dt.Rows[0]["costcategory"].ToString(), actcostcategroy));

            PageJobManagement.DynamicValue = "evcFK_r_TruckUnit";
            string acttruckunitid = PageJobManagement.dd_autocomplete.BaseElement.Children[0].Children[0].Children[0].GetAttribute("ng-reflect-user-input").Value;
            Assert.AreEqual(dt.Rows[0]["truckunitid"].ToString(), acttruckunitid, "Truck Unit Mismatch");
            CommonHelper.TraceLine(string.Format("Truck Unit Expected Value {0} and Actual Value{1}", dt.Rows[0]["truckunitid"].ToString(), acttruckunitid));


            string acttotoalcost = PageJobManagement.txt_totalCost.BaseElement.Children[0].Children[0].Children[0].Children[0].GetAttribute("aria-valuenow").Value;
            Assert.AreEqual(dt.Rows[0]["totalcost"].ToString(), acttotoalcost, "Total Cost Mismatch");
            CommonHelper.TraceLine(string.Format("Total Cost  Expected Value {0} and Actual Value{1}", dt.Rows[0]["totalcost"].ToString(), acttotoalcost));

            //Additional Attributes ::
            TelerikObject.Click(PageJobManagement.link_Additional, false);
            string actfieldserviceorderid = new HtmlInputText(PageJobManagement.txt_fieldServiceOrderID.BaseElement.Children[0]).Text;
            Assert.AreEqual(dt.Rows[0]["fieldserviceorderid"].ToString(), actfieldserviceorderid, "Field Service Order ID mismatch");
            CommonHelper.TraceLine(string.Format("Field Service Order ID :  Expected Value {0} and Actual Value{1}", dt.Rows[0]["fieldserviceorderid"].ToString(), actfieldserviceorderid));

            string actpersonperformingtask = new HtmlInputText(PageJobManagement.txt_perosnPerformingTask.BaseElement.Children[0]).Text;
            Assert.AreEqual(dt.Rows[0]["personperformingtask"].ToString(), actpersonperformingtask, "Person Performing Task mismatch");
            CommonHelper.TraceLine(string.Format("Person Performing Task:  Expected Value {0} and Actual Value{1}", dt.Rows[0]["personperformingtask"].ToString(), actpersonperformingtask));

            // div -. div -> label [ input ratio /span ]
            bool actpreventiveval = TelerikObject.GetRadioSelection(PageJobManagement.radiogp_preventiveRadioGroup, dt.Rows[0]["preventivemaintanance"].ToString());
            Assert.IsTrue(actpreventiveval, "Preventive Maintenance Radio Selection is not set to " + dt.Rows[0]["preventivemaintanance"].ToString());
            CommonHelper.TraceLine(string.Format("Preventive Maintenance Value set to  {0}:  Expected Value {1} and Actual Value{2} ", dt.Rows[0]["preventivemaintanance"].ToString(), "TRUE", actpreventiveval.ToString().ToUpper()));

            string actquantity = PageJobManagement.txt_evntquantity.BaseElement.Children[0].Children[0].Children[0].Children[0].GetAttribute("aria-valuenow").Value;
            Assert.AreEqual(dt.Rows[0]["quantity"].ToString(), actquantity, "Quantity mismatch");
            CommonHelper.TraceLine(string.Format("Quantity :  Expected Value {0} and Actual Value{1}", dt.Rows[0]["quantity"].ToString(), actquantity));

            string actremarks = new HtmlTextArea(PageJobManagement.txt_evntremarks.BaseElement.Children[0]).Text;
            Assert.AreEqual(dt.Rows[0]["remarks"].ToString(), actremarks, "Remarks mismatch");
            CommonHelper.TraceLine(string.Format("Remarks :  Expected Value {0} and Actual Value{1}", dt.Rows[0]["remarks"].ToString(), actremarks));

            string actrespperson = new HtmlInputText(PageJobManagement.txt_responsilbePerson.BaseElement.Children[0]).Text;
            Assert.AreEqual(dt.Rows[0]["responsibleperson"].ToString(), actrespperson, "Responsible field  Mismatch");
            CommonHelper.TraceLine(string.Format("Responsible field  :  Expected Value {0} and Actual Value{1}", dt.Rows[0]["responsibleperson"].ToString(), actrespperson));


            bool acttroubleval = TelerikObject.GetRadioSelection(PageJobManagement.radio_trouble, dt.Rows[0]["trouble"].ToString());
            Assert.IsTrue(acttroubleval, "Trouble Radio Selection is not set to " + dt.Rows[0]["trouble"].ToString());
            CommonHelper.TraceLine(string.Format("Trouble Radio Selection  value set to  {0}:  Expected Value {1} and Actual Value{2} ", dt.Rows[0]["trouble"].ToString(), "TRUE", acttroubleval.ToString().ToUpper()));

            bool actunplannedevtval = TelerikObject.GetRadioSelection(PageJobManagement.radio_unplannedEvent, dt.Rows[0]["unplannedevent"].ToString());
            Assert.IsTrue(actunplannedevtval, "Preventive Maintenance Radio Selection is not set to " + dt.Rows[0]["unplannedevent"].ToString());
            CommonHelper.TraceLine(string.Format("Preventive Maintenance Value set to  {0}:  Expected Value {1} and Actual Value{2} ", dt.Rows[0]["unplannedevent"].ToString(), "TRUE", actunplannedevtval.ToString().ToUpper()));


            string actworkorderid = new HtmlInputText(PageJobManagement.txt_evcWorkorderID.BaseElement.Children[0]).Text;
            Assert.AreEqual(dt.Rows[0]["workorderid"].ToString(), actworkorderid, "Work Order Id mismatch");
            CommonHelper.TraceLine(string.Format("Work Order Id :  Expected Value {0} and Actual Value{1}", dt.Rows[0]["workorderid"].ToString(), actworkorderid));

            string actevtcostrate = PageJobManagement.txt_evcHistoricalRate.BaseElement.Children[0].Children[0].Children[0].Children[0].GetAttribute("aria-valuenow").Value;
            Assert.AreEqual(dt.Rows[0]["eventcostrate"].ToString(), actevtcostrate, "Event Cost Rate Mismatch");
            CommonHelper.TraceLine(string.Format("Event Cost Rate: Expected Value {0} and Actual Value{1}", dt.Rows[0]["eventcostrate"].ToString(), actevtcostrate));

            //Extended  Attributes ::
            TelerikObject.Click(PageJobManagement.link_Extedned, false);
            string actvendorProduct = new HtmlInputText(PageJobManagement.txt_VendorProduct.BaseElement.Children[0]).Text;
            Assert.AreEqual(dt.Rows[0]["eventvendorproduct"].ToString(), actvendorProduct, "VendorProduct mismatch");
            CommonHelper.TraceLine(string.Format("VendorProduct  :  Expected Value {0} and Actual Value{1}", dt.Rows[0]["eventvendorproduct"].ToString(), actvendorProduct));

            TelerikObject.Click(PageJobManagement.link_CloseEditDialog);

        }
        public void ModifyEventFields(DataTable dt)
        {
            //Enter All required values for Event Type 
            TelerikObject.Sendkeys(PageJobManagement.txt_duartionDays, dt.Rows[0]["durationhours"].ToString());
            //eventbeindate ,eventenddate
            TelerikObject.Sendkeys(PageJobManagement.txt_eventBeginDate, dt.Rows[0]["eventbeindate"].ToString().Replace("/", ""), false);
            TelerikObject.TypeText(dt.Rows[0]["eventbeintime"].ToString().Replace(":", ""));
            Thread.Sleep(1000);
            //   TelerikObject.Keyboard_Send(Keys.Tab);
            TelerikObject.Sendkeys(PageJobManagement.txt_eventEndDate, dt.Rows[0]["eventenddate"].ToString().Replace("/", ""), false);
            TelerikObject.TypeText(dt.Rows[0]["eventendtime"].ToString().Replace(":", ""));
            Thread.Sleep(1000);
            TelerikObject.SetAutoCompleteSelectwithForAttribute("evcFK_AFE", dt.Rows[0]["afe"].ToString(), "AFE");
            TelerikObject.SetAutoCompleteSelectwithForAttribute("evcFK_BusinessOrganization", dt.Rows[0]["serviceprovider"].ToString(), "Service Provider");
            TelerikObject.SetAutoCompleteSelectwithForAttribute("evcFK_r_CatalogItem", dt.Rows[0]["costcategory"].ToString(), "cost Categroy");
            TelerikObject.SetAutoCompleteSelectwithForAttribute("evcFK_r_TruckUnit", dt.Rows[0]["truckunitid"].ToString(), "Truck Unit/ID");
            TelerikObject.Sendkeys(PageJobManagement.txt_totalCost, dt.Rows[0]["totalcost"].ToString());

            // Click on  "Addtional Link"
            TelerikObject.Click(PageJobManagement.link_Additional, false);
            TelerikObject.Sendkeys(PageJobManagement.txt_fieldServiceOrderID, dt.Rows[0]["fieldserviceorderid"].ToString());
            TelerikObject.Sendkeys(PageJobManagement.txt_perosnPerformingTask, dt.Rows[0]["personperformingtask"].ToString());
            TelerikObject.SetRadioButtonwithText(PageJobManagement.radiogp_preventiveRadioGroup, dt.Rows[0]["preventivemaintanance"].ToString());
            TelerikObject.Sendkeys(PageJobManagement.txt_evntquantity, dt.Rows[0]["quantity"].ToString());
            TelerikObject.Sendkeys(PageJobManagement.txt_evntremarks, dt.Rows[0]["remarks"].ToString());
            TelerikObject.Sendkeys(PageJobManagement.txt_responsilbePerson, dt.Rows[0]["responsibleperson"].ToString());
            TelerikObject.SetRadioButtonwithText(PageJobManagement.radio_trouble, dt.Rows[0]["trouble"].ToString());
            TelerikObject.SetRadioButtonwithText(PageJobManagement.radio_unplannedEvent, dt.Rows[0]["unplannedevent"].ToString());
            PageJobManagement.txt_evcHistoricalRate.ScrollToVisible();
            TelerikObject.Sendkeys(PageJobManagement.txt_evcWorkorderID, dt.Rows[0]["workorderid"].ToString());
            TelerikObject.Sendkeys(PageJobManagement.txt_evcHistoricalRate, dt.Rows[0]["eventcostrate"].ToString());



            //Save Event
            TelerikObject.Click(PageJobManagement.btn_saveEvent);
            CommonHelper.TraceLine("Waiting for Save Event Success Toaster Message ");
            Assert.IsNotNull(PageJobManagement.toaster_msgModifyEventSuccess, "Toaster Message of Modify Event Success was not shown in UI");
            CommonHelper.TraceLine("Verifying Table Values on UI for Events Saved ");
            Thread.Sleep(2000);

            PageJobManagement.DynamicValue = dt.Rows[0]["eventbeindate"].ToString();
            TelerikObject.Click(PageJobManagement.accordian_eventDate);
            //Verify if Modify , Delete , clone buttons are avialble in Events Grid:
            Assert.IsTrue(PageJobManagement.btn_gridCellModify.IsEnabled, "Grid cell Modify is not enabled after adding record ");
            Assert.IsTrue(PageJobManagement.btn_gridCellDelete.IsEnabled, "Grid cell Delete is not enabled after adding record ");
            Assert.IsTrue(PageJobManagement.btn_gridCellClone.IsEnabled, "Grid cell Clone is not enabled after adding record ");

        }

        private void DeleteMultiEvents(int startpos, int endpos)
        {
            for (int i = startpos; i < endpos; i++)
            {
                //accordian is forced to open
                TelerikObject.Click(PageJobManagement.accordian_eventDate);
                TelerikObject.Click(PageJobManagement.row_selector_Deletefor_EventType);
                Assert.IsNotNull(PageJobManagement.btn_confirmDeleteEvent, "Confirm Delelte button is Not shown");
                CommonHelper.TraceLine("Confirm Delete Event is shown");
                TelerikObject.Click(PageJobManagement.btn_confirmDeleteEvent);
                Assert.IsNotNull(PageJobManagement.toaster_msgDeleteEventSuccess, "Delete Event Success was not shown on UI");
                CommonHelper.TraceLine("Single Event added was Deleted");
                Thread.Sleep(2000);


            }
        }

        public void VerifyClonePasteFeature(DataTable dt)
        {
            //Verify Functionality of Clone and Paste Event Button
            //Clone Bundle Test
            PageJobManagement.DynamicValue = "Bundle Test";
            TelerikObject.Click(PageJobManagement.row_selector_Clonefor_EventType);
            TelerikObject.Click(PageJobManagement.btnPasteEvent);
            //PageJobManagement.DynamicValue = dt.Rows[0]["eventbeindate"].ToString();
            //TelerikObject.Click(PageJobManagement.accordian_eventDate);
            Assert.IsNotNull(PageJobManagement.toaster_msgPasteEventSuccess, "Toaster Message for Paste Success has not appearted on UI");
            CommonHelper.TraceLine("Toaster Success Message for Paste Event has appearrted on UI");
            //Clone chemical Analysis

            PageJobManagement.DynamicValue = dt.Rows[0]["eventbeindate"].ToString();
            TelerikObject.Click(PageJobManagement.accordian_eventDate);
            PageJobManagement.DynamicValue = "Chemical Analysis";
            TelerikObject.Click(PageJobManagement.row_selector_Clonefor_EventType);
            TelerikObject.Click(PageJobManagement.btnPasteEvent);
            Assert.IsNotNull(PageJobManagement.toaster_msgPasteEventSuccess, "Toaster Message for Paste Success has not appearted on UI");
            CommonHelper.TraceLine("Toaster Success Message for Paste Event has appearrted on UI");
            //Clone WireLienPerforrate

            PageJobManagement.DynamicValue = dt.Rows[0]["eventbeindate"].ToString();
            TelerikObject.Click(PageJobManagement.accordian_eventDate);
            PageJobManagement.DynamicValue = "Wireline - Perforate";
            TelerikObject.Click(PageJobManagement.row_selector_Clonefor_EventType);
            TelerikObject.Click(PageJobManagement.btnPasteEvent);

            Assert.IsNotNull(PageJobManagement.toaster_msgPasteEventSuccess, "Toaster Message for Paste Success has not appearted on UI");
            CommonHelper.TraceLine("Toaster Success Message for Paste Event has appearrted on UI");
            //Verify that all the events have been clone with Clone and Paste Functionality

            PageJobManagement.DynamicValue = dt.Rows[0]["eventbeindate"].ToString();
            CommonHelper.TraceLine("*******Verifying the Grid Cell Values after Cloning to ensure that all rows have been cloned:*****");
            TelerikObject.Click(PageJobManagement.accordian_eventDate);
            PageJobManagement.VerifyMultiAddwithClone(dt, false);

        }




    }
}