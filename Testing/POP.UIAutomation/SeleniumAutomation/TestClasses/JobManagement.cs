using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeleniumAutomation.SeleniumObject;
using System.Configuration;
using SeleniumAutomation.Helper;
using System.Diagnostics;
using AventStack.ExtentReports;
using System.IO;
using System;
using System.Threading;
using OpenQA.Selenium;
using System.Collections.Generic;
using SeleniumAutomation.AGGridScreenDatas;

namespace SeleniumAutomation.TestClasses
{
    static class JobManagement
    {

        public static string addjob(ExtentTest test)
        {
            SeleniumActions.waitClick(PageObjects.JobManagementPage.addbutton);
            test.Info("Adding job");
            CommonHelper.TraceLine("Adding Job");
            SeleniumActions.waitClick(PageObjects.JobManagementPage.addjob);
            Thread.Sleep(1000);
            SeleniumActions.waitClick(PageObjects.JobManagementPage.jobtypedrpdwn);
            SeleniumActions.waitClick(PageObjects.JobManagementPage.jobtype);
            SeleniumActions.waitClick(PageObjects.JobManagementPage.jobreasondrpdwn);
            SeleniumActions.waitClick(PageObjects.JobManagementPage.jobreason);
            SeleniumActions.selectKendoDropdownValue(PageObjects.JobManagementPage.jobstatusdrpdwn, TestData.JobDet.JobStatus, true);
            SeleniumActions.sendText(PageObjects.JobManagementPage.begindate, TestData.JobDet.BeginDateTemplate);
            SeleniumActions.sendText(PageObjects.JobManagementPage.enddate, TestData.JobDet.EndDateTemplate);
            Thread.Sleep(1000);
            SeleniumActions.waitClick(PageObjects.JobManagementPage.save);
            SeleniumActions.WaitForLoad();
            SeleniumActions.takeScreenshot("Job added");
            string jobID = getJobID();

            test.Info("Job Saved, Job ID:" + jobID);
            CommonHelper.TraceLine("Job created, ID:- " + jobID);
            test.Pass("Job added successfully", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "Job added.png"))).Build());
            return jobID;

        }

        public static string getJobID()
        {
            List<IWebElement> jobids = new List<IWebElement>();

            string jobID = "";
            jobids = SeleniumActions.Gettotalrecordsinlist_type2("xpath", "//div[@col-id='JobId']//span[@class='cell-layer-1']/span[2]");

            for (int i = 0; i < jobids.Count; i++)
            {
                if (jobids[i].Text != "")
                {
                    jobID = jobids[i].Text;
                    break;

                }
            }
            return jobID;
        }


        public static string addjob(ExtentTest test, string jobstatus)
        {
            SeleniumActions.waitClick(PageObjects.JobManagementPage.addbutton);
            test.Info("Adding job");
            CommonHelper.TraceLine("Adding Job");
            SeleniumActions.waitClick(PageObjects.JobManagementPage.addjob);

            SeleniumActions.waitClick(PageObjects.JobManagementPage.jobtypedrpdwn);
            SeleniumActions.waitClick(PageObjects.JobManagementPage.jobtype);
            SeleniumActions.waitClick(PageObjects.JobManagementPage.jobreasondrpdwn);
            SeleniumActions.waitClick(PageObjects.JobManagementPage.jobreason);
            SeleniumActions.selectKendoDropdownValue(PageObjects.JobManagementPage.jobstatusdrpdwn, SeleniumActions.BringResource(jobstatus.ToLower().Trim()), true);

            SeleniumActions.sendText(PageObjects.JobManagementPage.enddate, TestData.JobDet.EndDateTemplate);

            SeleniumActions.waitClick(PageObjects.JobManagementPage.save);
            SeleniumActions.WaitForLoad();
            string text = SeleniumActions.getInnerText(PageObjects.JobManagementPage.toaster).ToLower().Trim();
            Assert.AreEqual("added successfully", text);
            SeleniumActions.WaitForLoad();
            string jid = SeleniumActions.getInnerText(PageObjects.JobManagementPage.jobidgrid).ToLower().Trim();
            test.Info("Job Saved, Job ID:" + jid);
            CommonHelper.TraceLine("Job created, ID:- " + jid);
            test.Pass("Job added successfully", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "Job added.png"))).Build());
            return jid;
        }

        public static string addjobProspective(ExtentTest test, string jobstatus)
        {
            SeleniumActions.waitClick(PageObjects.JobManagementPage.addbutton);
            test.Info("Adding job");
            CommonHelper.TraceLine("Adding Job");
            SeleniumActions.waitClick(PageObjects.JobManagementPage.addjob);

            SeleniumActions.waitClick(PageObjects.JobManagementPage.jobtypedrpdwn);
            SeleniumActions.waitClick(PageObjects.JobManagementPage.jobtype);
            SeleniumActions.waitClick(PageObjects.JobManagementPage.jobreasondrpdwn);
            SeleniumActions.waitClick(PageObjects.JobManagementPage.jobreason);
            SeleniumActions.selectKendoDropdownValue(PageObjects.JobManagementPage.jobstatusdrpdwn, SeleniumActions.BringResource(jobstatus.ToLower().Trim()), true);

            SeleniumActions.sendText(PageObjects.JobManagementPage.begindate, TestData.JobDet.BeginDateTemplate);
            SeleniumActions.sendText(PageObjects.JobManagementPage.enddate, TestData.JobDet.EndDateTemplate);

            SeleniumActions.waitClick(PageObjects.JobManagementPage.save);
            SeleniumActions.WaitForLoad();
            string text = SeleniumActions.getInnerText(PageObjects.JobManagementPage.toaster).ToLower().Trim();
            Assert.AreEqual("added successfully", text);
            SeleniumActions.WaitForLoad();
            string jid = SeleniumActions.getInnerText(PageObjects.JobManagementPage.jobidgrid).ToLower().Trim();
            test.Info("Job Saved, Job ID:" + jid);
            CommonHelper.TraceLine("Job created, ID:- " + jid);
            test.Pass("Job added successfully", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "Job added.png"))).Build());
            return jid;
        }
        public static string addjob(ExtentTest test, string jobstatus, string begindate, string enddate)
        {
            SeleniumActions.waitClick(PageObjects.JobManagementPage.addbutton);
            test.Info("Adding job");
            CommonHelper.TraceLine("Adding Job");
            SeleniumActions.waitClick(PageObjects.JobManagementPage.addjob);
            SeleniumActions.waitClick(PageObjects.JobManagementPage.jobtypedrpdwn);
            SeleniumActions.waitClick(PageObjects.JobManagementPage.jobtype);
            SeleniumActions.waitClick(PageObjects.JobManagementPage.jobreasondrpdwn);
            SeleniumActions.waitClick(PageObjects.JobManagementPage.jobreason);
            SeleniumActions.selectKendoDropdownValue(PageObjects.JobManagementPage.jobstatusdrpdwn, SeleniumActions.BringResource(jobstatus.ToLower().Trim()));
            SeleniumActions.sendText(PageObjects.JobManagementPage.begindate, begindate);
            SeleniumActions.sendText(PageObjects.JobManagementPage.enddate, enddate);
            Thread.Sleep(1000);
            SeleniumActions.waitClick(PageObjects.JobManagementPage.save);
            string text = SeleniumActions.getInnerText(PageObjects.JobManagementPage.toaster).ToLower().Trim();
            Assert.AreEqual("added successfully", text);
            SeleniumActions.takeScreenshot("Job added");
            SeleniumActions.WaitForLoad();
            string jid = SeleniumActions.getInnerText(PageObjects.JobManagementPage.jobidgrid).ToLower().Trim();
            test.Info("Job Saved, Job ID:" + jid);
            CommonHelper.TraceLine("Job created, ID:- " + jid);
            test.Pass("Job added successfully", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "Job added.png"))).Build());
            return jid;
        }

        public static void Verifyjobcreatedtemplate(ExtentTest test)
        {
            JobManagementGeneralDataProcess JM = new JobManagementGeneralDataProcess();
            SeleniumAutomation.AGGridScreenDatas.TypeJobManagementGeneralGrid[] gridvalues = JM.FetchdataScreen();
            //verify job type
            Assert.AreEqual(gridvalues[0].jobType.ToLower().Trim(), TestData.JobDet.JobType.ToLower().Trim());
            //verify begin date


            string begindate = TestData.JobDet.BeginDate;


            string month = begindate.Substring(0, 2);
            string day = begindate.Substring(2, 2);
            string year = begindate.Substring(4, 4);

            if (gridvalues[0].beginDate.ToString().Contains("12:00:00 AM"))
            {
                begindate = month + "/" + day + "/" + year + " 12:00:00 AM";
                Assert.AreEqual(begindate, gridvalues[0].beginDate);
            }
            else
            {
                string begindate1 = DateTime.Now.ToString("hh tt").ToString();
                begindate = month + "/" + day + "/" + year + " " + begindate1;
                string[] split_begain = gridvalues[0].beginDate.Split(':');
                string final_began = split_begain[0].ToString() + " " + DateTime.Now.ToString("tt").ToString();
                Assert.AreEqual(begindate, final_began.ToString());
            }



            //verify end date
            string enddate = TestData.JobDet.EndDate;

            month = enddate.Substring(0, 2);
            day = enddate.Substring(2, 2);
            year = enddate.Substring(4, 4);

            enddate = month + "/" + day + "/" + year + " 12:00:00 AM";

            Assert.AreEqual(enddate, gridvalues[0].endDate);

            test.Pass("Job verified successfully", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "Job verfied.png"))).Build());
        }

        public static void Verifyjobcreated(ExtentTest test)
        {
            JobManagementGeneralDataProcess JM = new JobManagementGeneralDataProcess();
            SeleniumAutomation.AGGridScreenDatas.TypeJobManagementGeneralGrid[] gridvalues = JM.FetchdataScreen();
            //verify job type
            Assert.AreEqual(gridvalues[0].jobType.ToLower().Trim(), TestData.JobDet.JobType.ToLower().Trim());
            //verify begin date
            string begindate = TestData.JobDet.BeginDate;
            string month = begindate.Substring(0, 2);
            string day = begindate.Substring(2, 2);
            string year = begindate.Substring(4, 4);
            begindate = month + "/" + day + "/" + year + " 12:00:00 AM";
            Assert.AreEqual(begindate, gridvalues[0].beginDate);
            //verify end date

            string enddate = TestData.JobDet.EndDate;
            month = enddate.Substring(0, 2);
            day = enddate.Substring(2, 2);
            year = enddate.Substring(4, 4);
            enddate = month + "/" + day + "/" + year + " 12:00:00 AM";

            Assert.AreEqual(enddate, gridvalues[0].endDate);

            test.Pass("Job verified successfully", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "Job verfied.png"))).Build());
        }

        public static void Verifyjobcreatedusingcopy(ExtentTest test)
        {
            JobManagementGeneralDataProcess JM = new JobManagementGeneralDataProcess();
            SeleniumAutomation.AGGridScreenDatas.TypeJobManagementGeneralGrid[] gridvalues = JM.FetchdataScreen();
            //verify job type
            Assert.AreEqual(gridvalues[0].jobType.ToLower().Trim(), TestData.JobDet.JobType.ToLower().Trim());
            //verify job reason
            Assert.AreEqual(gridvalues[0].jobReason.ToLower().Trim(), TestData.JobDet.JobReason.ToLower().Trim());
            //verify job status
            //string jobstatus = SeleniumActions.getInnerText(PageObjects.JobManagementPage.jobstatusgrid).ToLower().Trim();
            Assert.AreEqual(gridvalues[0].status.ToLower().Trim(), TestData.JobDet.JobStatus.ToLower().Trim());

            //verify begin grid
            string currentdate = DateTime.Now.ToString("MM/dd/yyyy");   //ToUniversalTime() eliminated   
            string[] filterbdate = gridvalues[0].beginDate.ToLower().Trim().Split(' ');
            Assert.AreEqual(filterbdate[0], currentdate);
            CommonHelper.TraceLine("Verified Begin Date");
            // string enddategrid = SeleniumActions.getInnerText(PageObjects.JobManagementPage.jobenddategrid).ToLower().Trim();           
            string[] filteredate = gridvalues[0].endDate.ToLower().Trim().Split(' ');
            Assert.AreEqual(filteredate[0], currentdate);
            CommonHelper.TraceLine("Verified End Date");
            test.Pass("Job verified successfully", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "Job verfied.png"))).Build());
        }
        public static void UpdateJob(ExtentTest test)
        {
            //SeleniumActions.waitClick(PageObjects.JobManagementPage.jobidgrid);
            //SeleniumActions.waitClick(By.XPath("//ag-grid-angular//div[@col-id='JobId']/span/span/span/span"));

            SeleniumActions.waitClick(By.XPath("(//generic-grid)[1]//ag-grid-angular//div[@col-id='JobId']/span/span/span/span[2]"));
            SeleniumActions.waitClick(PageObjects.JobManagementPage.updatebutton);
            SeleniumActions.WaitForLoad();
            SeleniumActions.sendText(PageObjects.JobManagementPage.begindate, TestData.JobDet.ChangedBeginDate);
            Thread.Sleep(1000);
            SeleniumActions.waitClick(PageObjects.JobManagementPage.save);
            string text = SeleniumActions.getInnerText(PageObjects.JobManagementPage.toaster).ToLower().Trim();

            Assert.AreEqual("updated successfully", text);
            SeleniumActions.WaitForLoad();

            //string begindategrid = SeleniumActions.getInnerText(PageObjects.JobManagementPage.jobbegindategrid).ToLower().Trim();
            string begindate = TestData.JobDet.ChangedBeginDate;
            string month = begindate.Substring(0, 2);
            string day = begindate.Substring(2, 2);
            string year = begindate.Substring(4, 4);
            begindate = month + "/" + day + "/" + year + " 12:00:00 AM";
            //verify begin date
            JobManagementGeneralDataProcess JM = new JobManagementGeneralDataProcess();
            SeleniumAutomation.AGGridScreenDatas.TypeJobManagementGeneralGrid[] gridvalues = JM.FetchdataScreen();

            Assert.AreEqual(gridvalues[0].beginDate, begindate);

            //Assert.AreEqual(begindategrid, begindate);
            CommonHelper.TraceLine("Job Updated successfully");
            test.Pass("Job Updated successfully", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "Job Updated.png"))).Build());

        }
        public static void copypastejob(ExtentTest test)
        {

            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.JobManagementPage.jobidgrid);
            SeleniumActions.waitClick(PageObjects.JobManagementPage.copybutton);
            string text = SeleniumActions.getInnerText(PageObjects.JobManagementPage.toaster).ToLower().Trim();
            Assert.AreEqual("successfully copied job", text);
            SeleniumActions.waitClick(PageObjects.JobManagementPage.pastebutton);
            SeleniumActions.waitClick(PageObjects.JobManagementPage.confirmdialogyes);
            SeleniumActions.WaitForLoad();
            text = SeleniumActions.getInnerText(PageObjects.JobManagementPage.toaster).ToLower().Trim();
            Assert.AreEqual("successfully pasted job", text);
            CommonHelper.TraceLine("successfully pasted job");
            SeleniumActions.WaitForLoad();
            test.Pass("Job Pasted successfully", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "Job pasted.png"))).Build());
            Thread.Sleep(2000);//Waiting current thread for 2 sec as there is no loader implemented after pasting job so to saty in sync a wait is required
        }


        public static void JobHistory(ExtentTest test, string jobid)
        {
            SeleniumActions.waitClick(PageObjects.JobManagementPage.jobidgrid);
            SeleniumActions.waitClick(PageObjects.JobManagementPage.jobhistorybutton);
            string jid = SeleniumActions.getInnerText(PageObjects.JobManagementPage.jobidjobhistorydialog).ToLower().Trim();
            Assert.AreEqual(jobid, jid);
            string status = SeleniumActions.getInnerText(PageObjects.JobManagementPage.jobstatusjobhistorydialog).ToLower().Trim();
            Assert.AreEqual(TestData.JobDet.JobStatus.ToLower().Trim(), status);
            string user = Environment.UserDomainName + "\\" + Environment.UserName;
            string getuserfromapp = SeleniumActions.getInnerText(PageObjects.JobManagementPage.userjobhistorydialog).ToLower().Trim();
            Assert.AreEqual(user.ToLower().Trim(), getuserfromapp);
            CommonHelper.TraceLine("Job History Verified successfully");
            test.Pass("Job History success", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "Job History.png"))).Build());
        }
        public static void addtemplate(ExtentTest test)
        {
            SeleniumActions.waitClick(PageObjects.FieldserviceconfigPage.addnewtempbut);
            SeleniumActions.sendText(PageObjects.FieldserviceconfigPage.categoryinput, TestData.Fieldservice.JobCat);
            SeleniumActions.waitClick(PageObjects.FieldserviceconfigPage.Templatejobheaderlabel);
            SeleniumActions.selectKendoDropdownValue(PageObjects.FieldserviceconfigPage.jobtype, TestData.JobDet.JobType);
            SeleniumActions.selectKendoDropdownValue(PageObjects.FieldserviceconfigPage.jobreason, TestData.JobDet.JobReason);
            Thread.Sleep(1000);
            SeleniumActions.waitClick(PageObjects.FieldserviceconfigPage.savebutton);
            string text = SeleniumActions.getInnerText(PageObjects.JobManagementPage.toaster).ToLower().Trim();
            Assert.AreEqual("successfully added template job", text);
            CommonHelper.TraceLine("Template added successfully");
            test.Pass("Template added successfully", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "Template added successfully.png"))).Build());
        }
        public static void configurecanceljobforconcludedjobstatusview(ExtentTest test)
        {
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.FieldserviceconfigPage.referencedatatab);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.FieldserviceconfigPage.referencetableheading);
            Thread.Sleep(1000);
            SeleniumActions.sendkeystroke("pagedown");
            Thread.Sleep(1000);
            SeleniumActions.sendkeystroke("pagedown");
            Thread.Sleep(1000);
            SeleniumActions.sendkeystroke("pagedown");
            Thread.Sleep(1000);
            SeleniumActions.waitClick(PageObjects.FieldserviceconfigPage.jobstatusoption);


            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.FieldserviceconfigPage.editiconforcancel);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.FieldserviceconfigPage.readyjobradio);
            SeleniumActions.waitClick(PageObjects.FieldserviceconfigPage.concludedjobradio);

            SeleniumActions.waitClick(PageObjects.JobManagementPage.smallerbut);
            SeleniumActions.WaitForLoad();
            CommonHelper.TraceLine("Cancelled jobs added to concluded job status view successfully");

        }
        public static void addjobfromtemplate(ExtentTest test)
        {
            SeleniumActions.waitClick(PageObjects.JobManagementPage.addbutton);
            SeleniumActions.WaitForLoad();
            test.Info("Adding job");
            CommonHelper.TraceLine("Adding Job");
            Thread.Sleep(1000);
            SeleniumActions.waitClick(PageObjects.JobManagementPage.addjobfromtemplate);
            SeleniumActions.WaitForLoad();
            //  SeleniumActions.waitClick(PageObjects.JobManagementPage.category);
            SeleniumActions.waitClick(PageObjects.JobManagementPage.panelitemjobtype);
            SeleniumActions.waitClickJS(PageObjects.JobManagementPage.selectbutton);
            SeleniumActions.WaitForLoad();
            SeleniumActions.selectKendoDropdownValue(PageObjects.JobManagementPage.jobstatusdrpdwn, TestData.JobDet.JobStatus);
            SeleniumActions.sendText(PageObjects.JobManagementPage.begindate, TestData.JobDet.BeginDateTemplate);
            SeleniumActions.sendText(PageObjects.JobManagementPage.enddate, TestData.JobDet.EndDateTemplate);
            Thread.Sleep(1000);
            SeleniumActions.waitClick(PageObjects.JobManagementPage.save);
            string text = SeleniumActions.getInnerText(PageObjects.JobManagementPage.toaster).ToLower().Trim();
            Assert.AreEqual("added successfully", text);
            SeleniumActions.WaitForLoad();
            CommonHelper.TraceLine("Template added successfully");
            test.Pass("Template job added successfully", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "Template job added successfully.png"))).Build());
        }
        public static void addevent(ExtentTest test, string jobid)
        {

            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(SeleniumActions.getByLocator("Xpath", "//div[@col-id='JobId']//span[@class='cell-layer-1']//span[text()='" + jobid + "']", "Job id TD"));
            SeleniumActions.waitClick(PageObjects.JobManagementPage.eventtab);
            SeleniumActions.WaitForLoad();
            editevent(test);

        }
        public static void editevent(ExtentTest test)
        {
            SeleniumActions.waitClick(PageObjects.JobManagementPage.addbutton);
            SeleniumActions.waitClick(PageObjects.JobManagementPage.addhyperlink);
            Thread.Sleep(3000);
            SeleniumActions.selectKendoDropdownValue(PageObjects.JobManagementPage.eventcombobox, SeleniumActions.BringResource("acidizeeventtype"));
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.JobManagementPage.Additionallink);
            SeleniumActions.waitClick(PageObjects.JobManagementPage.Requiredlink);
            SeleniumActions.WaitForLoad();
            SeleniumActions.sendText(PageObjects.JobManagementPage.eventbdate, SeleniumActions.BringResource("eventbegindate") + SeleniumActions.BringResource("eventbegintime"));
            SeleniumActions.sendText(PageObjects.JobManagementPage.eventedate, SeleniumActions.BringResource("eventenddate") + SeleniumActions.BringResource("eventendtime"));


            SeleniumActions.KendoTypeNSelect(PageObjects.JobManagementPage.eventserviceprovider, "American");
            SeleniumActions.WaitForLoad();
            //Explicitly Select the option for N/A as they are not madatory now
            SeleniumActions.KendoTypeNSelect(PageObjects.JobManagementPage.truckuniteproviderdropdowninsingleevent, "N/A");
            SeleniumActions.KendoTypeNSelect(PageObjects.JobManagementPage.costcatdropdowninsingleevent, "N/A - N/A");

            //  SeleniumActions.selectKendoDropdownValue(PageObjects.JobManagementPage.eventserviceprovider, SeleniumActions.BringResource("American_serviceprovider"));
            //SeleniumActions.sendText(PageObjects.JobManagementPage.eventserviceprovider, SeleniumActions.BringResource("American_serviceprovider"));
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.JobManagementPage.savebutton);
            string text = SeleniumActions.getInnerText(PageObjects.JobManagementPage.toaster).ToLower().Trim();
            Assert.AreEqual("events saved successfully", text);
            CommonHelper.TraceLine("successfully event created");
            SeleniumActions.WaitForLoad();
            test.Pass("Event created successfully", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine(ExtentFactory.screenshotdir, "Event creation.png"))).Build());
        }
        public static void addevent(ExtentTest test, string jobid, string begindate, string enddate, string hours, string cost)
        {


            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.JobManagementPage.addbutton);
            SeleniumActions.waitClick(PageObjects.JobManagementPage.addhyperlink);
            Thread.Sleep(3000);
            SeleniumActions.WaitForLoad();
            SeleniumActions.selectKendoDropdownValue(PageObjects.JobManagementPage.eventcombobox, SeleniumActions.BringResource("acidizeeventtype"));
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.JobManagementPage.Additionallink);
            SeleniumActions.waitClick(PageObjects.JobManagementPage.Requiredlink);
            SeleniumActions.WaitForLoad();
            SeleniumActions.sendText(PageObjects.JobManagementPage.hours, hours);
            SeleniumActions.sendText(PageObjects.JobManagementPage.cost, cost);
            SeleniumActions.sendText(PageObjects.JobManagementPage.eventbdate, begindate + SeleniumActions.BringResource("eventbegintime"));
            SeleniumActions.sendText(PageObjects.JobManagementPage.eventedate, enddate + SeleniumActions.BringResource("eventendtime"));
            SeleniumActions.sendText(PageObjects.JobManagementPage.eventserviceprovider, SeleniumActions.BringResource("American_serviceprovider"));
            SeleniumActions.sendkeystroke(PageObjects.JobManagementPage.eventserviceprovider, "Tab");
            //SeleniumActions.selectKendoDropdownValue(PageObjects.JobManagementPage.eventserviceprovider, SeleniumActions.BringResource("American_serviceprovider"));
            //SeleniumActions.sendText(PageObjects.JobManagementPage.eventserviceprovider, SeleniumActions.BringResource("American_serviceprovider"));
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.JobManagementPage.savebutton);
            string text = SeleniumActions.getInnerText(PageObjects.JobManagementPage.toaster).ToLower().Trim();
            Assert.AreEqual("events saved successfully", text);
            CommonHelper.TraceLine("successfully event created");
            SeleniumActions.WaitForLoad();
            test.Pass("Event created successfully", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine(ExtentFactory.screenshotdir, "Event creation.png"))).Build());

        }
        public static void verifytotalhourscosts(TypeEventGrid[] griddata, string actualhour1, string actualhour2, string cost1, string cost2)
        {
            double actualhourin1 = Convert.ToDouble(actualhour1);
            double actualhourin2 = Convert.ToDouble(actualhour2);
            double totalhours = actualhourin1 + actualhourin2;
            double actualcostin1 = Convert.ToDouble(cost1);
            double actualcostin2 = Convert.ToDouble(cost2);
            double totalcost = actualcostin1 + actualcostin2;
            double sumgridhours = Convert.ToDouble(griddata[0].Duration) + Convert.ToDouble(griddata[1].Duration);
            double sumgridcost = Convert.ToDouble(griddata[0].TotalCost) + Convert.ToDouble(griddata[1].TotalCost);

            Assert.AreEqual(totalhours, sumgridhours, "Hours not matching");
            Assert.AreEqual(totalcost, sumgridcost, "Costs not matching");

            string labeltotalhours = SeleniumActions.getInnerText(PageObjects.JobManagementPage.totalhourslabel);
            string[] splitlabel = labeltotalhours.Split(':');
            double totalh = Convert.ToDouble(splitlabel[1].Trim());

            string labeltotalcost = SeleniumActions.getInnerText(PageObjects.JobManagementPage.totalcostlabel);
            splitlabel = labeltotalcost.Split(':');
            double totalc = Convert.ToDouble(splitlabel[1].Trim());

            Assert.AreEqual(totalhours, totalh, "Label data not matching");
            Assert.AreEqual(totalcost, totalc, "Label data not matching");

        }

        public static void addjobplan(ExtentTest test, string jobid)
        {

            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(SeleniumActions.getByLocator("Xpath", "//div[@col-id='JobId']//span[@class='cell-layer-1']//span[text()='" + jobid + "']", "Job id TD"));
            SeleniumActions.waitClick(PageObjects.JobManagementPage.plusbutton);
            SeleniumActions.waitClick(PageObjects.JobManagementPage.jobplanoption);
            //SeleniumActions.waitClick(PageObjects.JobManagementPage.jobplanoption);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.JobManagementPage.addbuttonjobplan);
            SeleniumActions.selectKendoDropdownValue(PageObjects.JobManagementPage.eventypejobplan, SeleniumActions.BringResource("acidizeeventtype"));
            SeleniumActions.selectKendoDropdownValue(PageObjects.JobManagementPage.vendorname, SeleniumActions.BringResource("vendor"));
            SeleniumActions.selectKendoDropdownValue(PageObjects.JobManagementPage.catalogitemjobplan, "N/A");
            SeleniumActions.selectKendoDropdownValue(PageObjects.JobManagementPage.truckunitjobplan, "N/A");
            SeleniumActions.sendText(PageObjects.JobManagementPage.estimatedhours, "2");
            SeleniumActions.sendText(PageObjects.JobManagementPage.unitprice, "2");
            // the ordre column is removed from the UI hence commenting the code 
            //SeleniumActions.sendText(PageObjects.JobManagementPage.order, "1");
            SeleniumActions.waitClick(PageObjects.JobManagementPage.totalcost);

            SeleniumActions.waitClick(PageObjects.JobManagementPage.savebuttonJobPlan);
            SeleniumActions.WaitForLoad();

        }
        public static void addjobcost(ExtentTest test, string jobid, string[] param)
        {

            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(SeleniumActions.getByLocator("Xpath", "(//generic-grid)[1]//ag-grid-angular//div[@col-id='JobId']/span/span/span/span[text()='" + jobid + "']", "Job id TD"));
            SeleniumActions.waitClick(PageObjects.JobManagementPage.plusbutton);
            SeleniumActions.waitClick(PageObjects.JobManagementPage.jobcostoption);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.JobManagementPage.addbuttonjobcost);
            /* SeleniumActions.sendText(PageObjects.JobManagementPage.dateinputinsidedialog, param[0]);            
             SeleniumActions.selectDropdownValue(PageObjects.JobManagementPage.vendorjobcost, param[1]);            
             SeleniumActions.selectDropdownValue(PageObjects.JobManagementPage.catalogjobcost, param[2]);            
             SeleniumActions.sendText(PageObjects.JobManagementPage.quantityjobcost, param[3], true);            
             SeleniumActions.sendText(PageObjects.JobManagementPage.unitprice,param[4]);            
             SeleniumActions.waitClick(PageObjects.JobManagementPage.savebut);            
             string text = SeleniumActions.getInnerText(PageObjects.JobManagementPage.toaster).ToLower().Trim();
             Assert.AreEqual("added successfully", text);
             CommonHelper.TraceLine("successfully created job cost");
             SeleniumActions.WaitForLoad();    */
            editjobcost(test, param);
            test.Info("Joc Cost created");


        }
        public static void editjobcost(ExtentTest test, string[] param, string type = "add")
        {

            SeleniumActions.sendText(PageObjects.JobManagementPage.dateinputinsidedialog, param[0]);
            SeleniumActions.selectDropdownValue(PageObjects.JobManagementPage.vendorjobcost, param[1]);
            SeleniumActions.selectDropdownValue(PageObjects.JobManagementPage.catalogjobcost, param[2]);
            if (type.ToLower().Trim() == "edit")
            {

                SeleniumActions.sendText(PageObjects.JobManagementPage.quantityjobcostedit, param[3], true);
                SeleniumActions.sendText(PageObjects.JobManagementPage.unitpriceedit, param[4], true);
                SeleniumActions.waitClick(PageObjects.JobManagementPage.savebut);
                string text = SeleniumActions.getInnerText(PageObjects.JobManagementPage.toaster).ToLower().Trim();
                Assert.AreEqual("updated successfully", text);
                CommonHelper.TraceLine("successfully created job cost");

            }
            else
            {
                SeleniumActions.sendText(PageObjects.JobManagementPage.quantityjobcost, param[3], true);
                SeleniumActions.sendText(PageObjects.JobManagementPage.unitprice, param[4]);
                SeleniumActions.waitClick(PageObjects.JobManagementPage.savebut);
                string text = SeleniumActions.getInnerText(PageObjects.JobManagementPage.toaster).ToLower().Trim();
                Assert.AreEqual("added successfully", text);
                CommonHelper.TraceLine("successfully created job cost");

            }

            SeleniumActions.WaitForLoad();
        }
        public static void addmultievent(ExtentTest test, string jobid)
        {

            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(SeleniumActions.getByLocator("Xpath", "//div[@col-id='JobId']//span[@class='cell-layer-1']//span[text()='" + jobid + "']", "Job id TD"));
            SeleniumActions.waitClick(PageObjects.JobManagementPage.eventtab);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.JobManagementPage.addbutton);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.JobManagementPage.multiadd);
            multieventinputs(test);
            string text = SeleniumActions.getInnerText(PageObjects.JobManagementPage.toaster).ToLower().Trim();
            Assert.AreEqual("events saved successfully", text);
            CommonHelper.TraceLine("successfully event created");
            SeleniumActions.WaitForLoad();
            test.Pass("Event created successfully", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "MultiEvent creation.png"))).Build());

        }
        public static void multieventinputs(ExtentTest test)
        {
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitForElement(PageObjects.JobManagementPage.acidizeevent);
            SeleniumActions.sendText(PageObjects.JobManagementPage.dateinputinsidedialog, SeleniumActions.BringResource("eventbegindate"));
            SeleniumActions.selectDropdownValue(PageObjects.JobManagementPage.serviceproviderdropdowninmultievent, SeleniumActions.BringResource("American_serviceprovider"));
            SeleniumActions.selectKendoDropdownValue(PageObjects.JobManagementPage.truckuniteproviderdropdowninmultievent, "N/A");
            SeleniumActions.selectKendoDropdownValue(PageObjects.JobManagementPage.costcatdropdowninmultievent, "N/A - N/A");
            SeleniumActions.sendText(PageObjects.JobManagementPage.rateinputinmultievent, "2");
            SeleniumActions.waitClick(PageObjects.JobManagementPage.acidizeevent);
            SeleniumActions.waitClick(PageObjects.JobManagementPage.acidizecoiledevent);
            SeleniumActions.waitClick(PageObjects.JobManagementPage.addbuttonformultievent);
            SeleniumActions.waitClick(PageObjects.JobManagementPage.savebuttoninsidedialog);
            SeleniumActions.WaitForLoad();


        }
        public static void verifyaddedevent(ExtentTest test, TypeEventGrid griddata, string begindate, string enddate, string eventtypeexp)
        {
            SeleniumActions.WaitForLoad();
            string eventtype = griddata.EventType.ToLower().Trim();
            Assert.AreEqual(eventtypeexp.ToLower().Trim(), eventtype);
            CommonHelper.TraceLine("Event type verified");
            string eventbdate = griddata.BeginTime.ToLower().Trim();
            string[] date = eventbdate.Split('/');
            string month = date[0];
            string datey = date[1];
            string year = date[2];
            year = year.Substring(0, 4);
            string concatdate = month + datey + year;
            Assert.AreEqual(begindate.ToLower().Trim(), concatdate);
            CommonHelper.TraceLine("Event BeginDate verified");
            string resourcetime = SeleniumActions.BringResource("eventbegintime").ToLower().Trim();
            resourcetime = resourcetime.Substring(0, 4);
            string time = date[2].Substring(5, 5);
            string[] times = time.Split(':');
            time = times[0] + times[1];
            Assert.AreEqual(resourcetime, time);
            string timeunit = date[2].Substring(14, 2).ToLower().Trim();
            Assert.AreEqual(timeunit, "am");
            CommonHelper.TraceLine("Event BeginTime verified");


            string eventedate = griddata.EndTime.ToLower().Trim();
            date = eventedate.Split('/');
            month = date[0];
            datey = date[1];
            year = date[2];
            year = year.Substring(0, 4);
            concatdate = month + datey + year;
            Assert.AreEqual(enddate, concatdate);
            CommonHelper.TraceLine("Event enddate verified");
            resourcetime = SeleniumActions.BringResource("eventendtime").ToLower().Trim();
            resourcetime = resourcetime.Substring(0, 4);
            time = date[2].Substring(5, 5);
            times = time.Split(':');
            time = times[0] + times[1];
            Assert.AreEqual(resourcetime, time);
            timeunit = date[2].Substring(14, 2).ToLower().Trim();
            Assert.AreEqual(timeunit, "am");
            CommonHelper.TraceLine("Event EndTime verified");

            string serviceprovider = griddata.ServiceProvider.ToLower().Trim();
            Assert.AreEqual(SeleniumActions.BringResource("American_serviceprovider").ToLower().Trim(), serviceprovider);
            string truckunit = griddata.TruckID.ToLower().Trim();
            Assert.AreEqual("n/a", truckunit);
            string costcat = griddata.CostCategory.ToLower().Trim();
            Assert.AreEqual("n/a - n/a", costcat);
            test.Pass("Event verified successfully", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine(ExtentFactory.screenshotdir, "Event verified.png"))).Build());

        }
        public static void verifyjobplan(ExtentTest test, string hours, string cost)
        {
            SeleniumActions.WaitForLoad();
            string eventtype = SeleniumActions.getInnerText(PageObjects.JobManagementPage.jobplantypegrid).ToLower().Trim();
            Assert.AreEqual(SeleniumActions.BringResource("acidizeeventtype").ToLower().Trim(), eventtype);
            CommonHelper.TraceLine("Event type verified");
            Assert.AreEqual(hours, SeleniumActions.getInnerText(PageObjects.JobManagementPage.jobplanhoursgrid));
            CommonHelper.TraceLine("Hours  verified");
            Assert.AreEqual(cost, SeleniumActions.getInnerText(PageObjects.JobManagementPage.jobplancostgrid));
            CommonHelper.TraceLine("Cost  verified");
            Assert.AreEqual(SeleniumActions.BringResource("vendor").ToLower().Trim(), SeleniumActions.getInnerText(PageObjects.JobManagementPage.jobplanvendorgrid).ToString().ToLower().Trim());
            CommonHelper.TraceLine("Vendor  verified");
            Assert.AreEqual("n/a", SeleniumActions.getInnerText(PageObjects.JobManagementPage.jobplantruckunitgrid).ToString().ToLower().Trim());
            CommonHelper.TraceLine("Truck Unit  verified");
            Assert.AreEqual("n/a", SeleniumActions.getInnerText(PageObjects.JobManagementPage.jobplancataloggrid).ToString().ToLower().Trim());
            CommonHelper.TraceLine("Catalog verified");

        }
        public static void verifyjobcost(ExtentTest test, string[] param)
        {
            JobCostDetailsDataProcess JC = new JobCostDetailsDataProcess();
            SeleniumAutomation.AGGridScreenDatas.TypeJobCostDetailsGrid[] gridvalues = JC.FetchdataScreen();

            Assert.AreEqual(param[1].ToLower().Trim(), gridvalues[0].vendor.ToLower().Trim());
            CommonHelper.TraceLine("Vendor verified");

            Assert.AreEqual(param[2].ToLower().Trim(), gridvalues[0].catalogitem.ToLower().Trim());

            CommonHelper.TraceLine("Catalog  verified");

            int sum = int.Parse(param[3]) * int.Parse(param[4]);
            string total = sum.ToString();
            //Assert.AreEqual(total, SeleniumActions.getInnerText(PageObjects.JobManagementPage.jobplancostgrid));
            Assert.AreEqual(total, gridvalues[0].cost.ToLower().Trim());
            CommonHelper.TraceLine("Total Cost  verified");
            Assert.AreEqual(param[3], gridvalues[0].quantity.ToLower().Trim());
            //Assert.AreEqual(param[3], SeleniumActions.getInnerText(PageObjects.JobManagementPage.jobplanvendorgrid));
            CommonHelper.TraceLine("Quantity  verified");
            // Assert.AreEqual(param[4], SeleniumActions.getInnerText(PageObjects.JobManagementPage.jobplantruckunitgrid));
            Assert.AreEqual(param[4], gridvalues[0].unitprice.ToLower().Trim());
            CommonHelper.TraceLine(" Unit cost verified");

        }


        public static void addeventcost_duration(ExtentTest test, string jobid, string duration, string cost)
        {


            SeleniumActions.waitClick(PageObjects.JobManagementPage.addbutton);
            SeleniumActions.waitClick(PageObjects.JobManagementPage.addhyperlink);
            Thread.Sleep(3000);
            SeleniumActions.selectKendoDropdownValue(PageObjects.JobManagementPage.eventcombobox, SeleniumActions.BringResource("acidizeeventtype"));
            SeleniumActions.sendText(PageObjects.JobManagementPage.txtdurationhrs, duration);
            SeleniumActions.WaitForLoad();
            SeleniumActions.sendText(PageObjects.JobManagementPage.eventbdate, SeleniumActions.BringResource("eventbegindate") + SeleniumActions.BringResource("eventbegintime"));
            SeleniumActions.WaitForLoad();
            SeleniumActions.sendText(PageObjects.JobManagementPage.eventedate, SeleniumActions.BringResource("eventenddate") + SeleniumActions.BringResource("eventendtime"));
            SeleniumActions.WaitForLoad();
            SeleniumActions.selectKendoDropdownValue(PageObjects.JobManagementPage.eventserviceprovider, SeleniumActions.BringResource("American_serviceprovider"));
            SeleniumActions.WaitForLoad();
            SeleniumActions.WaitForLoad();
            SeleniumActions.sendText(PageObjects.JobManagementPage.txttotalcost, cost);
            SeleniumActions.WaitForLoad();

            SeleniumActions.waitClick(PageObjects.JobManagementPage.savebutton);

            CommonHelper.TraceLine("successfully event created");

            test.Pass("Event created successfully", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "Event creation.png"))).Build());

        }
        public static void adddrillingreport(ExtentTest test, string jobid, string[] param)
        {
            SeleniumActions.WaitForLoad();
            // SeleniumActions.waitClick(SeleniumActions.getByLocator("Xpath", "//kendo-grid-list//tbody/tr//span[@title='" + jobid + "']", "Job id TD"));
            SeleniumActions.waitClick(SeleniumActions.getByLocator("Xpath", "(//generic-grid)[1]//ag-grid-angular//div[@col-id='JobId']/span/span/span/span[text()='" + jobid + "']", "Job id TD"));
            SeleniumActions.waitClick(PageObjects.JobManagementPage.plusbutton);
            SeleniumActions.waitClick(PageObjects.JobManagementPage.drillingreportoption);
            SeleniumActions.WaitForLoad();
            if (SeleniumActions.getAttribute(PageObjects.JobManagementPage.drillingreportdrpdwn, "aria-disabled").ToLower().Trim().Contains("false"))
            {
                SeleniumActions.takeScreenshot("Add drillingreport II");
                CommonHelper.TraceLine("Drilling report Dropdown is active");
                Assert.Fail("Report Dropdown is active");
            }
            SeleniumActions.waitClick(PageObjects.JobManagementPage.createdrillreport);
            string modifiedd = param[0].Insert(2, "/");
            modifiedd = modifiedd.Insert(5, "/");
            Console.WriteLine("Drilling created fo date: " + modifiedd);
            SeleniumActions.sendText(PageObjects.JobManagementPage.drillingdate, param[0]);
            string modifiedt = param[1].Insert(2, ":");
            SeleniumActions.sendText(PageObjects.JobManagementPage.drillingtime, modifiedt);
            SeleniumActions.waitClick(PageObjects.JobManagementPage.drillireportcreatebutton);

            string text = SeleniumActions.getInnerText(PageObjects.JobManagementPage.toaster).ToLower().Trim();
            Assert.AreEqual("drilling report saved successfully.", text);
            SeleniumActions.WaitForLoad();

        }

        public static void verifydrillingreportadded(ExtentTest test, string[] param)
        {
            SeleniumActions.WaitForLoad();
            string modifiedd = param[0].Insert(2, "/");
            modifiedd = modifiedd.Insert(5, "/");

            string modifiedt = param[1].Insert(2, ":");

            if (SeleniumActions.getAttribute(PageObjects.JobManagementPage.drillingreportdrpdwn, "aria-disabled").ToLower().Trim().Contains("true"))
            {
                SeleniumActions.takeScreenshot("Add drillingreport III");
                CommonHelper.TraceLine("Drilling report Dropdown is not active after adding report hence failed");
                Assert.Fail("Report Dropdown is not active");

            }
            if (!SeleniumActions.getInnerText(PageObjects.JobManagementPage.drillingreportactiveoption).ToLower().Trim().Contains(modifiedd))
            {
                SeleniumActions.takeScreenshot("Drilling report active ");
                CommonHelper.TraceLine("Drilling report name not contains the created date");
                Assert.Fail("Drilling report active is invalid");
            }
            if (!SeleniumActions.getInnerText(PageObjects.JobManagementPage.drillingreportactiveoption).ToLower().Trim().Contains(modifiedt))
            {
                SeleniumActions.takeScreenshot("Drilling report active ");
                CommonHelper.TraceLine("Drilling report name not contains the created time");
                Assert.Fail("Drilling report active is invalid");
            }


        }
        public static void deletedrillingreport(ExtentTest test, string jobid, string[] param)
        {

            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.JobManagementPage.splitbuttondownarrow);
            Thread.Sleep(2000);
            SeleniumActions.waitClick(PageObjects.JobManagementPage.deletelistsplit);
            SeleniumActions.waitClick(PageObjects.JobManagementPage.splitbutton);
            SeleniumActions.waitClick(PageObjects.JobManagementPage.confirmdialogyesdelete);
            string text = SeleniumActions.getInnerText(PageObjects.JobManagementPage.toaster).ToLower().Trim();
            Assert.AreEqual("drilling report deleted successfully.", text);
            SeleniumActions.WaitForLoad();

        }
        public static void verifydeleteddrillingreport(ExtentTest test)
        {
            if (SeleniumActions.getAttribute(PageObjects.JobManagementPage.drillingreportdrpdwn, "aria-disabled").ToLower().Trim().Contains("false"))
            {
                SeleniumActions.takeScreenshot("Delete Drilling report I");
                CommonHelper.TraceLine("Drilling report Dropdown is active after deletion");
                Assert.Fail("Report Dropdown is active");

            }
            if (!SeleniumActions.getInnerText(PageObjects.JobManagementPage.drillingreportactiveoption).ToLower().Trim().Contains("select drilling report"))
            {
                SeleniumActions.takeScreenshot("Drilling report active ");
                CommonHelper.TraceLine("Drilling report name not contains the created time");
                Assert.Fail("Drilling report active is invalid");
            }

        }


        public static void addcomp(ExtentTest test, string[] compdet)
        {
            SeleniumActions.waitClick(PageObjects.JobManagementPage.addbuttondrillingarea);
            SeleniumActions.sendText(PageObjects.JobManagementPage.componentgrpcombobox, compdet[0]);
            Thread.Sleep(2000);
            SeleniumActions.WaitForLoad();
            editcomp(test, compdet);

        }
        public static void editcomp(ExtentTest test, string[] compdet)
        {

            SeleniumActions.sendText(PageObjects.JobManagementPage.parttypecombobox, compdet[1]);
            SeleniumActions.sendkeystroke(PageObjects.JobManagementPage.parttypecombobox, "tab");
            SeleniumActions.WaitForLoad();
            Thread.Sleep(4000);
            SeleniumActions.waitforPageloadComplete();
            SeleniumActions.sendText(PageObjects.JobManagementPage.compctlgdes, compdet[2]);
            SeleniumActions.sendkeystroke(PageObjects.JobManagementPage.compctlgdes, "tab");
            //  SeleniumActions.sendText(PageObjects.JobManagementPage.compname, compdet[3]);
            SeleniumActions.sendkeystroke(PageObjects.JobManagementPage.compname, "tab");
            SeleniumActions.sendText(PageObjects.JobManagementPage.complength, compdet[4]);
            SeleniumActions.sendkeystroke(PageObjects.JobManagementPage.complength, "tab");
            SeleniumActions.sendText(PageObjects.JobManagementPage.compdepth, compdet[5]);
            SeleniumActions.sendkeystroke(PageObjects.JobManagementPage.compdepth, "tab");
            SeleniumActions.waitClick(PageObjects.JobManagementPage.smallerbut);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.JobManagementPage.savebut2);
            string text = SeleniumActions.getInnerText(PageObjects.JobManagementPage.toaster).ToLower().Trim();


            //..to be confirmed on the assertion message

            //  Assert.AreEqual("saved successfully", text);
            SeleniumActions.WaitForLoad();


        }

        public static void verifyaddedcomp(ExtentTest test, string[] compdet)
        {
            //Assert.AreEqual(compdet[0].ToLower().Trim().ToString(), SeleniumActions.getInnerText(PageObjects.JobManagementPage.jobreasongrid).ToLower().Trim().ToString());
            //Assert.AreEqual(compdet[1].ToLower().Trim().ToString(), SeleniumActions.getInnerText(PageObjects.JobManagementPage.parttypegrid).ToLower().Trim().ToString());
            //Assert.AreEqual(compdet[2].ToLower().Trim().ToString(), SeleniumActions.getInnerText(PageObjects.JobManagementPage.compnamegrid).ToLower().Trim().ToString());
            //Assert.AreEqual(compdet[4].ToLower().Trim().ToString(), SeleniumActions.getInnerText(PageObjects.JobManagementPage.complengrid).ToLower().Trim().ToString());
            //Assert.AreEqual(compdet[5].ToLower().Trim().ToString(), SeleniumActions.getInnerText(PageObjects.JobManagementPage.compdepgrid).ToLower().Trim().ToString());

            DrillingReportDataProcess DR = new DrillingReportDataProcess();
            SeleniumAutomation.AGGridScreenDatas.TypeDrillingReportGrid[] gridvalues = DR.FetchdataScreen();
            Assert.AreEqual(compdet[0].ToLower().Trim().ToString(), gridvalues[0].componentgrouping.ToLower().Trim().ToString());
            Assert.AreEqual(compdet[1].ToLower().Trim().ToString(), gridvalues[0].partType.ToLower().Trim().ToString());
            Assert.AreEqual(compdet[2].ToLower().Trim().ToString(), gridvalues[0].compname.ToLower().Trim().ToString());
            Assert.AreEqual(compdet[4].ToLower().Trim().ToString(), gridvalues[0].length.ToLower().Trim().ToString());
            Assert.AreEqual(compdet[5].ToLower().Trim().ToString(), gridvalues[0].topdepth.ToLower().Trim().ToString());

            int dep = int.Parse(compdet[5]);
            int len = int.Parse(compdet[4]);
            int total = len + dep;
            Assert.AreEqual(total.ToString(), gridvalues[0].bottomdepth.ToLower().Trim().ToString());
        }


        public static void addWellborecomp(ExtentTest test, string[] compdet, string jobid)
        {
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(SeleniumActions.getByLocator("Xpath", "(//generic-grid)[1]//ag-grid-angular//div[@col-id='JobId']/span/span/span/span[text()='" + jobid + "']", "Job id TD"));
            SeleniumActions.waitClick(PageObjects.JobManagementPage.plusbutton);
            SeleniumActions.waitClick(PageObjects.JobManagementPage.wellborereportoption);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.JobManagementPage.addbuttondrillingarea);
            SeleniumActions.sendText(PageObjects.JobManagementPage.componentgrpcombobox, compdet[0]);
            editWellborecomp(test, compdet);


        }
        public static void editWellborecomp(ExtentTest test, string[] compdet)
        {

            SeleniumActions.sendText(PageObjects.JobManagementPage.parttypecombobox, compdet[1]);
            SeleniumActions.sendkeystroke(PageObjects.JobManagementPage.parttypecombobox, "tab");
            Thread.Sleep(2000);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitForElement(PageObjects.JobManagementPage.compctlgdes);
            SeleniumActions.WaitForLoad();
            Thread.Sleep(2000);
            SeleniumActions.sendspecialkey(PageObjects.JobManagementPage.compctlgdes, "selectall");
            SeleniumActions.sendspecialkey(PageObjects.JobManagementPage.compctlgdes, "delete");
            SeleniumActions.sendText(PageObjects.JobManagementPage.compctlgdes, compdet[2]);
            SeleniumActions.sendkeystroke(PageObjects.JobManagementPage.compctlgdes, "tab");
            //  SeleniumActions.sendText(PageObjects.JobManagementPage.compname, compdet[3]);
            SeleniumActions.sendkeystroke(PageObjects.JobManagementPage.compname, "tab");
            SeleniumActions.sendText(PageObjects.JobManagementPage.complength, compdet[4]);
            SeleniumActions.sendkeystroke(PageObjects.JobManagementPage.complength, "tab");
            SeleniumActions.sendText(PageObjects.JobManagementPage.compdepth, compdet[5]);
            SeleniumActions.sendkeystroke(PageObjects.JobManagementPage.compdepth, "tab");
            //bottom depth -- new requirement
            if (compdet[0] == "Borehole")
            {
                SeleniumActions.sendText(PageObjects.JobManagementPage.compBottomdepth, compdet[6]);
                SeleniumActions.sendkeystroke(PageObjects.JobManagementPage.compBottomdepth, "tab");
            }
            SeleniumActions.waitClick(PageObjects.JobManagementPage.smallerbut);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.JobManagementPage.savebut2);
            string text = SeleniumActions.getInnerText(PageObjects.JobManagementPage.toaster).ToLower().Trim();

            Assert.AreEqual("saved successfully.", text);
            SeleniumActions.WaitForLoad();
        }
        public static void verifyaddedWellborecomp(ExtentTest test, string[] compdet)
        {
            //Assert.AreEqual(compdet[0].ToLower().Trim().ToString(), SeleniumActions.getInnerText(PageObjects.JobManagementPage.jobreasongrid).ToLower().Trim().ToString());
            //Assert.AreEqual(compdet[1].ToLower().Trim().ToString(), SeleniumActions.getInnerText(PageObjects.JobManagementPage.parttypegrid).ToLower().Trim().ToString());
            //Assert.AreEqual(compdet[2].ToLower().Trim().ToString(), SeleniumActions.getInnerText(PageObjects.JobManagementPage.compnamegrid).ToLower().Trim().ToString());
            //Assert.AreEqual(compdet[4].ToLower().Trim().ToString(), SeleniumActions.getInnerText(PageObjects.JobManagementPage.complengrid).ToLower().Trim().ToString());
            //Assert.AreEqual(compdet[5].ToLower().Trim().ToString(), SeleniumActions.getInnerText(PageObjects.JobManagementPage.compdepgrid).ToLower().Trim().ToString());

            WellboreReportDataProcess WR = new WellboreReportDataProcess();
            TypeWellboreReportGrid[] gridvalues = WR.FetchdataScreen();
            Assert.AreEqual(compdet[0].ToLower().Trim().ToString(), gridvalues[0].componentgrouping.ToLower().Trim().ToString());
            Assert.AreEqual(compdet[1].ToLower().Trim().ToString(), gridvalues[0].partType.ToLower().Trim().ToString());
            Assert.AreEqual(compdet[2].ToLower().Trim().ToString(), gridvalues[0].compname.ToLower().Trim().ToString());
            Assert.AreEqual(compdet[4].ToLower().Trim().ToString(), gridvalues[0].length.ToLower().Trim().ToString());
            Assert.AreEqual(compdet[5].ToLower().Trim().ToString(), gridvalues[0].topdepth.ToLower().Trim().ToString());

            int dep = int.Parse(compdet[5]);
            int len = int.Parse(compdet[4]);
            int total = len + dep;
            Assert.AreEqual(total.ToString(), gridvalues[0].bottomdepth.ToLower().Trim().ToString());
        }
        public static string createjobwithouttempusingjobplan(ExtentTest test, string[] param)
        {
            SeleniumActions.waitClick(PageObjects.JobManagementPage.jobwithouttemplate);
            SeleniumActions.waitClick(PageObjects.JobManagementPage.specifyjobtab);
            SeleniumActions.WaitForLoad();
            SeleniumActions.selectKendoDropdownValue(PageObjects.JobManagementPage.jobtypeinjobplan, param[1]);
            SeleniumActions.WaitForLoad();
            SeleniumActions.selectKendoDropdownValue(PageObjects.JobManagementPage.jobreasoninjobplan, param[2]);
            SeleniumActions.WaitForLoad();
            SeleniumActions.selectKendoDropdownValue(PageObjects.JobManagementPage.serviceprovinjobplan, param[3]);
            SeleniumActions.WaitForLoad();
            enterplannedeventdetinjobplan(test);
            SeleniumActions.waitClick(PageObjects.JobManagementPage.economicanalysistabinjobplan);
            SeleniumActions.waitClick(PageObjects.JobManagementPage.specifyjobdettabinjobplan);
            SeleniumActions.WaitForLoad();
            string jobid = enterjobdetinjobplan(test);
            return jobid;
        }
        public static void enterplannedeventdetinjobplan(ExtentTest test)
        {
            SeleniumActions.waitClick(PageObjects.JobManagementPage.selectplannedeventtab);
            Thread.Sleep(2000);
            SeleniumActions.waitClick(PageObjects.JobManagementPage.addbuttonjobplan);
            SeleniumActions.WaitForLoad();

            SeleniumActions.selectKendoDropdownValue(PageObjects.JobManagementPage.serviceproviderdropdowninmultieventforjobplan, SeleniumActions.BringResource("vendor"));

            SeleniumActions.waitClick(PageObjects.JobManagementPage.acidizeevent);
            // SeleniumActions.waitClick(PageObjects.JobManagementPage.acidizecoiledevent);
            SeleniumActions.waitClick(PageObjects.JobManagementPage.addbuttonformultievent);
            SeleniumActions.waitClick(PageObjects.JobManagementPage.savebuttoninsidedialog);
            SeleniumActions.WaitForLoad();

        }
        public static string enterjobdetinjobplan(ExtentTest test)
        {
            SeleniumActions.selectKendoDropdownValue(PageObjects.JobManagementPage.jobstatusdrpdwninjobplan, TestData.JobDet.JobStatus);
            SeleniumActions.sendText(PageObjects.JobManagementPage.begindateforjobinjobplan, TestData.JobDet.BeginDate);
            SeleniumActions.sendText(PageObjects.JobManagementPage.enddateforjobinjobplan, TestData.JobDet.EndDate);
            SeleniumActions.waitClick(PageObjects.JobManagementPage.savebuttonjobplanwizard);
            SeleniumActions.WaitForLoad();
            string jobid = SeleniumActions.getInnerText(PageObjects.JobManagementPage.jobidreturnjobplanwizard);
            return jobid;

        }
        public static string createusingtempinjobplan(ExtentTest test, string[] param)
        {
            SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobwithtemplate);
            SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.specifytemtab);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.templateRemedial);
            enterplannedeventdetinjobplan(test);
            SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.economicanalysistabinjobplan);
            SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.specifyjobdettabinjobplan);
            string jobid = enterjobdetinjobplan(test);
            return jobid;
        }

        public static string updatejobviajobplan(ExtentTest test, string[] param)
        {
            SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.modifyjobviajobplan);
            SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.selectjobtab);
            SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.selectjobtab);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.templateRemedial);
            SeleniumActions.waitClick(PageObjects.JobManagementPage.selectplannedeventtab);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.economicanalysistabinjobplan);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.specifyjobdettabinjobplan);
            string jobid = updatejobdetinjobplan(test);
            return jobid;
        }
        public static string updatejobdetinjobplan(ExtentTest test)
        {
            //  SeleniumActions.selectKendoDropdownValue(PageObjects.JobManagementPage.jobstatusdrpdwninjobplan, TestData.JobDet.JobStatus);
            SeleniumActions.sendText(PageObjects.JobManagementPage.begindateforjobinjobplan, TestData.JobDet.ChangedBeginDate);
            //  SeleniumActions.sendText(PageObjects.JobManagementPage.enddateforjobinjobplan, TestData.JobDet.EndDate);
            SeleniumActions.waitClick(PageObjects.JobManagementPage.savebuttonjobplanwizard);
            SeleniumActions.WaitForLoad();
            string jobid = SeleniumActions.getInnerText(PageObjects.JobManagementPage.jobidreturnjobplanwizard);
            return jobid;

        }
        public static void Verifyupdatedjobcreated(ExtentTest test)
        {
            string jobtype = SeleniumActions.getInnerText(PageObjects.JobManagementPage.jobtypegrid).ToLower().Trim();
            Assert.AreEqual(jobtype, TestData.JobDet.JobType.ToLower().Trim());
            string jobreason = SeleniumActions.getInnerText(PageObjects.JobManagementPage.jobreasongrid).ToLower().Trim();
            Assert.AreEqual(jobreason, TestData.JobDet.JobReason.ToLower().Trim());
            string jobstatus = SeleniumActions.getInnerText(PageObjects.JobManagementPage.jobstatusgrid).ToLower().Trim();
            Assert.AreEqual(jobstatus, TestData.JobDet.JobStatus.ToLower().Trim());
            string begindategrid = SeleniumActions.getInnerText(PageObjects.JobManagementPage.jobbegindategrid).ToLower().Trim();
            string begindate = TestData.JobDet.ChangedBeginDate;
            string month = begindate.Substring(0, 2);
            string day = begindate.Substring(2, 2);
            string year = begindate.Substring(4, 4);
            begindate = month + "/" + day + "/" + year;
            Assert.AreEqual(begindategrid, begindate);
            string enddategrid = SeleniumActions.getInnerText(PageObjects.JobManagementPage.jobenddategrid).ToLower().Trim();
            string enddate = TestData.JobDet.EndDate;
            month = enddate.Substring(0, 2);
            day = enddate.Substring(2, 2);
            year = enddate.Substring(4, 4);
            enddate = month + "/" + day + "/" + year;
            Assert.AreEqual(enddategrid, enddate);
            test.Pass("Job verified successfully", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "Job verfied.png"))).Build());
        }
        public static void Verifyupdatedjobviajobplan(ExtentTest test, string jobid)
        {
            int flag = 0;
            int iter = 0;
            SeleniumActions.WaitForLoad();
            IList<IWebElement> elems = SeleniumActions.Gettotalrecordsinlist("xpath", "(//table[@class='k-grid-table'])[1]//tr/td[2]//span");

            foreach (IWebElement el in elems)
            {

                iter = iter + 1;
                if (SeleniumActions.getInnerText(el).Equals(jobid))
                {
                    CommonHelper.TraceLine("Begin date update successfully for Job " + jobid);
                    flag = 1;
                    string begindate = SeleniumActions.getInnerText(SeleniumActions.getByLocator("Xpath", "(//table[@class='k-grid-table'])[2]//tr[" + iter + "]/td[8]//span", "Begin Date colums "));
                    string[] splits = begindate.Split('/');
                    begindate = splits[0] + splits[1] + splits[2];
                    if (begindate.Equals(TestData.JobDet.ChangedBeginDate))
                    {
                        CommonHelper.TraceLine("Begin date update successfully for Job " + jobid);
                        test.Pass("Begin date updated successfully");
                    }
                    else
                    {
                        SeleniumActions.takeScreenshot("Update Job via JobPlanI");
                        test.Fail("Begin date not updated successfully", MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Update job via JonPlanI"))).Build());
                        Assert.Fail("Begin date not updated successfully".ToString());

                    }
                    break;
                }

            }
            if (flag == 0)
            {
                CommonHelper.TraceLine("We have searched for" + iter + " jobs but none of th ejob id matches with the updated job id which is " + jobid);
                SeleniumActions.takeScreenshot("Update Job via JobPlan II");
                test.Fail("Begin date not updated successfully", MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Update job via JonPlan II"))).Build());
                Assert.Fail("Begin date not updated successfully".ToString());
            }


        }
        public static void verifyjobstatusview(ExtentTest test, string view, string jobid, string jobstatus)
        {
            SeleniumActions.WaitForLoad();
            IList<IWebElement> jobs = new List<IWebElement>();
            TypeProspectivenConcludedJobsGrid[] datas;
            TypeReadyJobsGrid[] datasr;
            TypeProspectivenConcludedJobsGrid[] datasc;
            switch (view.ToLower().Trim())
            {

                case "prospective":
                    SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.prospectivetab);
                    SeleniumActions.WaitForLoad();
                    Thread.Sleep(2000);
                    ProspectiveJobDataProcess PD = new ProspectiveJobDataProcess();
                    datas = PD.FetchdataScreen();
                    int flag = 0;
                    foreach (var data in datas)
                    {
                        if (data.jobId.ToLower().Trim() == jobid)
                        {
                            flag = 1;
                        }

                    }
                    if (flag == 0)
                    {
                        Assert.Fail("Job Id of " + jobid + " and jobstatus of " + jobstatus + "couldnot be found in prospective job");
                    }
                    else
                    {
                        CommonHelper.TraceLine("Job Id of " + jobid + " and jobstatus of " + jobstatus + "found in prospective job");
                    }

                    break;
                case "ready":
                    SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.prospectivetab);
                    SeleniumActions.WaitForLoad();
                    Thread.Sleep(2000);
                    SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.readyjobsetab);
                    SeleniumActions.WaitForLoad();
                    Thread.Sleep(2000);
                    ReadyJobDataProcess RD = new ReadyJobDataProcess();
                    datasr = RD.FetchdataScreen();
                    int flagr = 0;
                    foreach (var data in datasr)
                    {
                        if (data.jobId.ToLower().Trim() == jobid)
                        {
                            flagr = 1;
                        }

                    }
                    if (flagr == 0)
                    {
                        Assert.Fail("Job Id of " + jobid + " and jobstatus of " + jobstatus + "couldnot be found in ready job");
                    }
                    else
                    {
                        CommonHelper.TraceLine("Job Id of " + jobid + " and jobstatus of " + jobstatus + "found in ready job");
                    }

                    break;
                case "concluded":
                    SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.prospectivetab);
                    SeleniumActions.WaitForLoad();
                    Thread.Sleep(2000);
                    SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.readyjobsetab);
                    SeleniumActions.WaitForLoad();
                    Thread.Sleep(2000);
                    SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.concludedjobstab);
                    SeleniumActions.WaitForLoad();
                    Thread.Sleep(2000);
                    ConcludedJoDataProcess CD = new ConcludedJoDataProcess();
                    datasc = CD.FetchdataScreen();
                    int flagc = 0;
                    foreach (var data in datasc)
                    {
                        if (data.jobId.ToLower().Trim() == jobid)
                        {
                            flagc = 1;
                        }

                    }
                    if (flagc == 0)
                    {
                        Assert.Fail("Job Id of " + jobid + " and jobstatus of " + jobstatus + "couldnot be found in con job");
                    }
                    else
                    {
                        CommonHelper.TraceLine("Job Id of " + jobid + " and jobstatus of " + jobstatus + "found in con job");
                    }

                    break;
                default:
                    Assert.Fail("Invalid screen provided in the method argument, Valid is: Prospective, Ready and concluded, Paased argument is " + view);
                    break;

            }


        }
        public static void verifymorningreport(ExtentTest test, string currentdate, string enddate)
        {
            SeleniumActions.WaitForLoad();
            Thread.Sleep(2000);
            string begindate;
            string[] splitsy;
            string enddateg;
            string[] splits;
            bool flag = false;
            string wname = "";
            string eventtype = "";
            MorningReportDataProcess MR = new MorningReportDataProcess();
            TypeMorningReportGrid[] datas = MR.FetchdataScreen();
            foreach (var data in datas)
            {
                wname = data.wellName.ToLower().Trim();
                eventtype = data.eventType.ToLower().Trim();
                if (data.wellName.ToLower().Trim() == ConfigurationManager.AppSettings.Get("CygNetFacility").ToLower().Trim() && data.eventType.ToLower().Trim() == SeleniumActions.BringResource("acidizeeventtype").ToLower().Trim())
                {

                    begindate = data.beginDate.ToLower().Trim();
                    splitsy = begindate.Split('/');
                    splitsy[2] = splitsy[2].Substring(0, 4);
                    begindate = splitsy[0] + splitsy[1] + splitsy[2] + "1200am";
                    Assert.AreEqual(currentdate.ToLower().Trim(), begindate);
                    Assert.AreEqual(TestData.JobDet.JobReason.ToLower().Trim(), data.jobReason.ToLower().Trim());
                    Assert.AreEqual(TestData.JobDet.JobReason.ToLower().Trim(), data.jobReason.ToLower().Trim());
                    Assert.AreEqual(SeleniumActions.BringResource("American_serviceprovider").ToLower().Trim(), data.serviceProvider.ToLower().Trim());

                    enddateg = data.endDate.ToLower().Trim();
                    splits = enddateg.Split('/');
                    splits[2] = splits[2].Substring(0, 4);
                    enddateg = splits[0] + splits[1] + splits[2] + "1200am";
                    Assert.AreEqual(enddate.ToLower().Trim(), enddateg);
                    CommonHelper.TraceLine("Morning report grid data verified successfully and is as per expected");
                    flag = true;
                }

            }
            Assert.IsTrue(flag, "Morning report data mismatch, wellname and eventype not found as per expected, FOund:- Wellname->" + wname + ", eventype->" + eventtype);


        }
        public static void currentwellboreverify(ExtentTest test, string compname)
        {
            SeleniumActions.WaitForLoad();
            int flag = 0;
            IList<IWebElement> comps = new List<IWebElement>();
            comps = SeleniumActions.Gettotalrecordsinlist("xpath", "//kendo-grid-list//tr//td[1]/span");
            foreach (IWebElement el in comps)
            {
                if (SeleniumActions.getInnerText(el).ToLower().Trim().Equals(compname.ToLower().Trim()))
                {
                    flag = 1;
                    SeleniumActions.drawhighlight(el);
                }
                else
                {
                    continue;
                }
            }
            if (flag == 0)
            {
                CommonHelper.TraceLine("Wellbore comp not found in current wellbore screen successfully");
                Assert.Fail("Added wellbore component not in current wellbore screen");

            }
            else
            {
                CommonHelper.TraceLine("Wellbore comp found in current wellbore screen successfully");
                Thread.Sleep(3000);
            }

        }
        public static void entertoursheet(ExtentTest test, string jobid, string[] jobcostdet, string[] compdet)
        {

            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(SeleniumActions.getByLocator("Xpath", "//div[@col-id='JobId']//span[@class='cell-layer-1']//span[text()='" + jobid + "']", "Job id TD"));
            SeleniumActions.waitClick(PageObjects.JobManagementPage.toursheetdailyactivitytab);
            SeleniumActions.WaitForLoad();
            editevent(test);
            SeleniumActions.waitClick(PageObjects.JobManagementPage.jobcostoption);
            SeleniumActions.waitClick(PageObjects.JobManagementPage.okbutton);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.JobManagementPage.addbuttonjobcost);
            editjobcost(test, jobcostdet);
            SeleniumActions.waitClick(PageObjects.JobManagementPage.failurereportoption);
            SeleniumActions.waitClick(PageObjects.JobManagementPage.okbutton);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.JobManagementPage.wellborereportoption);
            SeleniumActions.waitClick(PageObjects.JobManagementPage.okbutton);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.JobManagementPage.addbuttondrillingarea);
            SeleniumActions.sendText(PageObjects.JobManagementPage.componentgrpcombobox, compdet[0]);
            editWellborecomp(test, compdet);
            SeleniumActions.waitClick(PageObjects.JobManagementPage.jobstatusoption);
            //SeleniumActions.waitClick(PageObjects.JobManagementPage.okbutton);
            SeleniumActions.WaitForLoad();
            SeleniumActions.selectKendoDropdownValue(PageObjects.JobManagementPage.jobstatusdrpdwn, SeleniumActions.BringResource("approved"));
            SeleniumActions.waitClick(PageObjects.JobManagementPage.savebuttonjobplanwizard);
            SeleniumActions.WaitForLoad();
            string text = SeleniumActions.getInnerText(PageObjects.JobManagementPage.toaster).ToLower().Trim();
            Assert.AreEqual("updated successfully", text);

        }
        public static void Addcomponent(int k, bool quantity)
        {
            SeleniumActions.waitClick(PageObjects.JobManagementPage.addbuttondrillingarea);
            Thread.Sleep(2000);
            //New method to see if wait was Sufficient on ATS VM 
            SeleniumActions.KendoTypeNSelect(PageObjects.JobManagementPage.componentgrpcombobox, TestData.RRLTestData.dt.Rows[k]["Componentgroup"].ToString());
            SeleniumActions.KendoTypeNSelect(PageObjects.JobManagementPage.parttypecombobox, TestData.RRLTestData.dt.Rows[k]["PartType"].ToString());
            Thread.Sleep(2000);
            SeleniumActions.WaitForLoad();
            try
            {
                SeleniumActions.selectKendoDropdownValue(PageObjects.JobManagementPage.manufacturericon, TestData.RRLTestData.dt.Rows[k]["Manufacturer"].ToString());
            }
            catch (Exception e)
            {
                //ATS VM UPG VM ERROR
                Trace.WriteLine("Error " + e);
                SeleniumActions.takeScreenshot("ATSUPG_Slow_KenoUIItem");
            }
            Thread.Sleep(2000);
            SeleniumActions.WaitForLoad();
            SeleniumActions.sendText(PageObjects.JobManagementPage.compctlgdes, TestData.RRLTestData.dt.Rows[k]["CatalogItemdesc"].ToString());
            SeleniumActions.sendkeystroke("enter");
            Thread.Sleep(3000);
            SeleniumActions.sendText(PageObjects.JobManagementPage.compname, TestData.RRLTestData.dt.Rows[k]["ComponentName"].ToString());
            if (quantity == true)
            {
                SeleniumActions.sendText(PageObjects.JobManagementPage.compquantity, TestData.RRLTestData.dt.Rows[k]["Quantity"].ToString());
            }

            if (!TestData.RRLTestData.dt.Rows[k]["PartType"].ToString().Contains("Anchor") && !TestData.RRLTestData.dt.Rows[k]["PartType"].ToString().Contains("Liner"))
            {
                SeleniumActions.sendText(PageObjects.JobManagementPage.complength, TestData.RRLTestData.dt.Rows[k]["Length"].ToString());
            }
            if (TestData.RRLTestData.dt.Rows[k]["PartType"].ToString().Contains("Liner"))
            {
                SeleniumActions.sendText(PageObjects.JobManagementPage.compdepth, TestData.RRLTestData.dt.Rows[k]["TopDepth"].ToString());
                SeleniumActions.sendText(PageObjects.JobManagementPage.compBottomdepth, TestData.RRLTestData.dt.Rows[k]["BottomDepth"].ToString());
            }
            //Skip Entering Top depth for Rod 
            if (!TestData.RRLTestData.dt.Rows[k]["Componentgroup"].ToString().Contains("Rod"))
            {
                SeleniumActions.sendText(PageObjects.JobManagementPage.compdepth, TestData.RRLTestData.dt.Rows[k]["TopDepth"].ToString());
            }
            if (TestData.RRLTestData.dt.Rows[k]["Componentgroup"].ToString().Equals("Borehole"))
            {
                SeleniumActions.waitClick(PageObjects.JobManagementPage.parttypespecific);
                SeleniumActions.sendText(PageObjects.JobManagementPage.wellperfstatus, "Open");
            }
            SeleniumActions.waitClick(PageObjects.JobManagementPage.smallerbut);
            SeleniumActions.WaitForLoad();
        }

    }
}
