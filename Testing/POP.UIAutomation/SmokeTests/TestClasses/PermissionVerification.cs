using AventStack.ExtentReports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmokeTests.SeleniumObject;
using System.Configuration;
using SmokeTests.Helper;
using System.Threading;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using System.IO;

namespace SmokeTests.TestClasses
{
    class PermissionVerification
    {
        string IsRunningATS = ConfigurationManager.AppSettings.Get("isRunningATS");
        public static string FilesLocation = ConfigurationManager.AppSettings.Get("FilesLocation");
        public void VerifyRemoveAddWellPermission(ExtentTest test )
        {
            try
            {
                SeleniumActions.refreshBrowser();
                SeleniumActions.waitforPageloadComplete();
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.configurationTab);
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.wellconfigurationTab);
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitForElement(PageObjects.WellConfigurationPage.facilitybutton);
                SeleniumActions.WaitForLoad();
                //If non zero db
                if (IsRunningATS.Equals("false"))
                {
                    SeleniumActions.WaitForLoad();
                    SeleniumActions.waitForElement(PageObjects.WellConfigurationPage.wellattributetabby);
                }
                SeleniumActions.takeScreenshot("Add Well Remove");
                if (!SeleniumActions.isElemPresent(PageObjects.WellConfigurationPage.btnCreateNewWellby))
                    test.Pass("Create New Well Button is not displayed when Add Well Permission is removed", MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine(ExtentFactory.screenshotdir, "Add Well Remove.png"))).Build());
                else
                    test.Fail("Create New Well Button is still displayed even though Add Well Permission is removed");
            }
            catch (Exception e)
            {
                Assert.Fail("VerifyRemoveAddWellPermission Failed" + e.ToString());
                CommonHelper.TraceLine("Error : " + e.ToString());
            }
        }

        public void VerifyRestoreAddWellPermission(ExtentTest test)
        {
            try
            {
                SeleniumActions.refreshBrowser();
                SeleniumActions.waitforPageloadComplete();
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.wellconfigurationTab);
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitForElement(PageObjects.WellConfigurationPage.facilitybutton);
                SeleniumActions.WaitForLoad();
                //If non zero db
                if (IsRunningATS.Equals("false"))
                {
                    SeleniumActions.WaitForLoad();
                    SeleniumActions.waitForElement(PageObjects.WellConfigurationPage.wellattributetabby);
                }
                SeleniumActions.takeScreenshot("Add Well Restore");
                if (SeleniumActions.isElemPresent(PageObjects.WellConfigurationPage.btnCreateNewWellby))
                    test.Pass("Create New Well Button is displayed when Add Well Permission is restored", MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine(ExtentFactory.screenshotdir, "Add Well Restore.png"))).Build());
                else
                    test.Fail("Create New Well Button is still not displayed even though Add Well Permission is restored");

                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.btnCreateNewWell);
            }
            catch (Exception e)
            {

                Assert.Fail("VerifyRestoreAddWellPermission Failed" + e.ToString());
                CommonHelper.TraceLine("Error : " + e.ToString());
            }
        }

        public void VerifyRevokeRemoveWellPermission(ExtentTest test)
        {
            try
            {

                SeleniumActions.refreshBrowser();
                SeleniumActions.waitforPageloadComplete();
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.configurationTab);
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.wellconfigurationTab);
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitForElement(PageObjects.WellConfigurationPage.facilitybutton);
                SeleniumActions.waitForElement(PageObjects.WellConfigurationPage.wellattributetabby);

                SeleniumActions.takeScreenshot("Remove Well Revoke");
                if (!SeleniumActions.isElemPresent(PageObjects.WellConfigurationPage.btnDeleteWellby))
                    test.Pass("Delete  Well Button is not displayed when Delete Well Permission is removed", MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine(ExtentFactory.screenshotdir, "Remove Well Revoke.png"))).Build());
                else
                    test.Fail("Delete  Well Button is still displayed even though Delete Well Permission is removed");

                test.Info("Verified Remove Well Permission by Revoking the Permission");
            }
            catch (Exception e)
            {

                Assert.Fail("VerifyRevokeRemoveWellPermission Failed" + e.ToString());
                CommonHelper.TraceLine("Error : " + e.ToString());
            }
        }

        public void VerifyRestoreRemoveWellPermission(ExtentTest test)
        {
            try
            {
                SeleniumActions.refreshBrowser();
                SeleniumActions.waitforPageloadComplete();
                SeleniumActions.WaitForLoad();
                SeleniumActions.WaitforWellcountNonZero();
                SeleniumActions.waitForElement(PageObjects.WellConfigurationPage.facilitybutton);
                SeleniumActions.WaitForLoad();

                //If non zero db
                if (IsRunningATS.Equals("false"))
                {
                    SeleniumActions.WaitForLoad();
                    SeleniumActions.waitForElement(PageObjects.WellConfigurationPage.wellattributetabby);
                }
                SeleniumActions.takeScreenshot("Remove Well Restore");
                if (SeleniumActions.isElemPresent(PageObjects.WellConfigurationPage.btnDeleteWellby))
                    test.Pass("Delete  Well Button is displayed when Delete Well Permission is restored", MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine(ExtentFactory.screenshotdir, "Remove Well Restore.png"))).Build());
                else
                    test.Fail("Delete  Well Button is still not displayed even though Add Well Permission is restored");

                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.deletebutton);
                string text = SeleniumActions.getInnerText(PageObjects.WellConfigurationPage.Dialogtitle);
                Assert.AreEqual("Delete Well", text, "Delete Well dailog did not appearon clciking delete");
                SeleniumActions.takeScreenshot("Delete Well Confirm");
                test.Info("Verified Remove Well Permission by Resoring the Permission", MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine(ExtentFactory.screenshotdir, "Delete Well Confirm.png"))).Build());
            }
            catch (Exception e)
            {

                Assert.Fail("VerifyRestoreRemoveWellPermission Failed" + e.ToString());
                CommonHelper.TraceLine("Error : " + e.ToString());
            }
        }


        public void VerifyRestoreAddDocumentPermission(ExtentTest test)
        {
            SeleniumActions.refreshBrowser();
            SeleniumActions.waitforPageloadComplete();
            AttachDocumenttofirstJobRestore(test);
        }

        public void VerifyRevokeAddDocumentPermission(ExtentTest test)
        {
           
            SeleniumActions.refreshBrowser();
            CreateJob(test);
            AttachDocumenttofirstJobRevoke(test);
        }


        public void VerifyRevokeAddJobPermission(ExtentTest test)
        {
            try
            {

                SeleniumActions.refreshBrowser();
                SeleniumActions.waitforPageloadComplete();
                //Navigate to Field Services Tab and check for Add Job button state (Enabled or Disabled)
                SeleniumActions.WaitForLoad();
          
                SeleniumActions.waitClick(PageObjects.FieldServicesPage.fieldServicesTab);
                SeleniumActions.waitClick(PageObjects.FieldServicesPage.jobManagementTab);
                SeleniumActions.WaitforWellcountNonZero();
                SeleniumActions.WaitForLoad();
                SeleniumActions.takeScreenshot("Add Job Revoke");
                if (!SeleniumActions.getEnabledState(PageObjects.FieldServicesPage.btnAddJob))
                {
                    test.Pass("Add Job button is disabled when permisison was revoked", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine(ExtentFactory.screenshotdir, "Add Job Revoke.png"))).Build());

                }
                else
                {
                    test.Fail("Add Job button is enabled when permisison was revoked", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine(ExtentFactory.screenshotdir, "Add Job Revoke.png"))).Build());
                    Assert.Fail("Add Job Test failed as button is in enabled state when permission was revoked.");
                }
            }
            catch(Exception e)
            {
                SeleniumActions.takeScreenshot("Add Job Revoke");
                test.Fail("Add Job Exception", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine(ExtentFactory.screenshotdir, "Add Job Revoke.png"))).Build());
                test.Info("Exception from VerifyRevokeAddJobPermission" + e.ToString());
                Assert.Fail("VerifyRevokeAddJobPermission Failed" + e.ToString());
            }


        }

        public void VerifyRestoreAddJobPermission(ExtentTest test)
        {
            try
            {
                SeleniumActions.refreshBrowser();
                SeleniumActions.waitforPageloadComplete();
                //Navigate to Field Services Tab and check for Add Job button state (Enabled or Disabled)
                SeleniumActions.WaitForLoad();
                SeleniumActions.WaitforWellcountNonZero();
                SeleniumActions.WaitForLoad();
                SeleniumActions.takeScreenshot("Add Job Restore");
                if (SeleniumActions.getEnabledState(PageObjects.FieldServicesPage.btnAddJob))
                {
                    test.Pass("Add Job button is enabled when permisison was restored", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine(ExtentFactory.screenshotdir, "Add Job Restore.png"))).Build());
                    CreateJob(test);

                }
                else
                {
                    test.Fail("Add Job button is disabled when permisison was restored", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine(ExtentFactory.screenshotdir, "Add Job Restore.png"))).Build());
                    Assert.Fail("Add Job Test failed as button is in enabled state when permission was restored.");
                }
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("Add Job Restore Exception");
                test.Fail("Add Job Exception", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine(ExtentFactory.screenshotdir, "Add Job Restore Exception.png"))).Build());
                test.Info("Exception from VerifyRestoreAddJobPermission" + e.ToString());
                Assert.Fail("VerifyRestoreAddJobPermission Failed" + e.ToString());
            }


        }


        public void VerifyRevokeAddEventPermission(ExtentTest test)
        {
            try
            {

                SeleniumActions.refreshBrowser();
                SeleniumActions.waitforPageloadComplete();
                //Navigate to Field Services Tab and check for Add Job button state (Enabled or Disabled)
                SeleniumActions.WaitForLoad();
                CreateJob(test);
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(PageObjects.FieldServicesPage.tabAddEvent);
                SeleniumActions.WaitForLoad();
                SeleniumActions.takeScreenshot("Add Event Revoke");
                if (!SeleniumActions.getEnabledState(PageObjects.FieldServicesPage.btnAddJob))
                {
                    test.Pass("Add Job button is disabled when permisison was revoked", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine(ExtentFactory.screenshotdir, "Add Event Revoke.png"))).Build());

                }
                else
                {
                    test.Fail("Add Job button is enabled when permisison was revoked", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine(ExtentFactory.screenshotdir, "Add Event Revoke.png"))).Build());
                    Assert.Fail("Add Job Test failed as button is in enabled state when permission was revoked.");
                }
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("Add Job Revoke");
                test.Fail("Add Job Exception", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine(ExtentFactory.screenshotdir, "Add Job Revoke.png"))).Build());
                test.Info("Exception from VerifyRevokeAddJobPermission" + e.ToString());
                Assert.Fail("VerifyRevokeAddEventPermission Failed" + e.ToString());
            }


        }

        public void VerifyRestoreAddEventPermission(ExtentTest test)
        {
            try
            {
                SeleniumActions.refreshBrowser();
                SeleniumActions.waitforPageloadComplete();
                //Navigate to Field Services Tab and check for Add Job button state (Enabled or Disabled)
                SeleniumActions.WaitForLoad();
                SeleniumActions.WaitforWellcountNonZero();
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(PageObjects.FieldServicesPage.tabAddEvent);
                SeleniumActions.WaitForLoad();
                SeleniumActions.takeScreenshot("Add Event Restore");
                if (SeleniumActions.getEnabledState(PageObjects.FieldServicesPage.btnAddJob))
                {
                    test.Pass("Add Event button is enabled when permisison was restored", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine(ExtentFactory.screenshotdir, "Add Event Restore.png"))).Build());

                }
                else
                {
                    test.Fail("Add Event button is disabled when permisison was restored", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine(ExtentFactory.screenshotdir, "Add Event Restore.png"))).Build());
                    Assert.Fail("Add Event Test failed as button is in enabled state when permission was revoked.");
                }
                CreateEvent(test);

            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("Add Job Revoke");
                test.Fail("Add Job Exception", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine(ExtentFactory.screenshotdir, "Add Job Revoke.png"))).Build());
                test.Info("Exception from VerifyRevokeAddJobPermission" + e.ToString());
                Assert.Fail("Method VerifyRestoreAddEventPermission Failed.");

            }


        }

        public void VerifyRevokeAddJobCostDetailPermission(ExtentTest test)
        {
            try
            {

                SeleniumActions.refreshBrowser();
                SeleniumActions.waitforPageloadComplete();
                //Navigate to Field Services Tab and check for Add Job button state (Enabled or Disabled)
                SeleniumActions.WaitForLoad();
                CreateJob(test);
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(PageObjects.FieldServicesPage.btn_AddTabBy);
                SeleniumActions.waitClick(PageObjects.FieldServicesPage.lnk_AddJobCostdetails);
                SeleniumActions.WaitForLoad();
                SeleniumActions.takeScreenshot("AddJobCostDetail Revoke");
                if (!SeleniumActions.getEnabledState(PageObjects.FieldServicesPage.btnAddJobCostDetails))
                {
                    test.Pass("AddJobCostDetail  button is disabled when permisison was revoked", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine(ExtentFactory.screenshotdir, "AddJobCostDetail Revoke.png"))).Build());

                }
                else
                {
                    test.Fail("AddJobCostDetail button is enabled when permisison was revoked", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine(ExtentFactory.screenshotdir, "AddJobCostDetail Revoke.png"))).Build());
                    Assert.Fail("AddJobCostDetail Test failed as button is in enabled state when permission was revoked.");
                }
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("AddJobCostDetail");
                test.Fail("AddJobCostDetail Exception", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine(ExtentFactory.screenshotdir, "AddJobCostDetail.png"))).Build());
                test.Info("Exception from VerifyRevokeAddJobPermission" + e.ToString());
                Assert.Fail("VerifyRevokeAddEventPermission Failed" + e.ToString());
            }


        }


        public void VerifyRestoreAddJobCostDetailPermission(ExtentTest test)
        {
            try
            {
                SeleniumActions.refreshBrowser();
                SeleniumActions.waitforPageloadComplete();
                //Navigate to Field Services Tab and check for Add Job button state (Enabled or Disabled)
                SeleniumActions.WaitForLoad();
                SeleniumActions.WaitforWellcountNonZero();
                SeleniumActions.WaitForLoad();
                if (!SeleniumActions.isElemPresent(PageObjects.FieldServicesPage.tabAddJobCostDetails))
                {
                    //Consider in case of Slowest Angluar Load !!!!
                    SeleniumActions.waitClick(PageObjects.FieldServicesPage.btn_AddTabBy);
                    SeleniumActions.waitClick(PageObjects.FieldServicesPage.lnk_AddJobCostdetails);
                }
                SeleniumActions.waitClick(PageObjects.FieldServicesPage.tabAddJobCostDetails);
                SeleniumActions.WaitForLoad();
                SeleniumActions.takeScreenshot("Add Job Cost Details Restore");
                if (SeleniumActions.getEnabledState(PageObjects.FieldServicesPage.btnAddJobCostDetails))
                {
                    test.Pass("Add Job Cost Details button is enabled when permisison was restored", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine(ExtentFactory.screenshotdir, "Add Job Cost Details Restore.png"))).Build());

                }
                else
                {
                    test.Fail("Add Job Cost Detailsbutton is disabled when permisison was restored", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine(ExtentFactory.screenshotdir, "Add Job Cost Details Restore.png"))).Build());
                    Assert.Fail("Add Job Cost Details Test failed as button is in enabled state when permission was revoked.");
                }
                CreateJobCostDetails(test);

            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("Add Job Revoke");
                test.Fail("Add Job Exception", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine(ExtentFactory.screenshotdir, "Add Job Revoke.png"))).Build());
                test.Info("Exception from VerifyRevokeAddJobPermission" + e.ToString());
                Assert.Fail("Method VerifyRestoreAddEventPermission Failed.");

            }


        }
        public void CreateJob(ExtentTest test)
        {
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.FieldServicesPage.fieldServicesTab);
            SeleniumActions.waitClick(PageObjects.FieldServicesPage.jobManagementTab);
            SeleniumActions.WaitforWellcountNonZero();
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.FieldServicesPage.btnAddJob);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.FieldServicesPage.lnkAddJob);
            //   SeleniumActions.waitClickNG(PageObjects.FieldServicesPage.ddKendoJobType);
            SeleniumActions.selectKendoDropdownValue(PageObjects.FieldServicesPage.ddKendoJobType, TestData.JobDetails.jobtype);
            SeleniumActions.selectKendoDropdownValue(PageObjects.FieldServicesPage.ddKendoJobReason, TestData.JobDetails.jobreason);
            SeleniumActions.selectKendoDropdownValue(PageObjects.FieldServicesPage.ddKendoJobDriver, TestData.JobDetails.jobdriver);
            SeleniumActions.selectKendoDropdownValue(PageObjects.FieldServicesPage.ddKendoJobStatus, TestData.JobDetails.status);
            SeleniumActions.selectKendoDropdownValue(PageObjects.FieldServicesPage.ddKendoServieceProvider, TestData.JobDetails.serviceprovider, true);
            SeleniumActions.sendText(PageObjects.FieldServicesPage.txtJobStartDate, TestData.JobDetails.beindate, true);
            SeleniumActions.sendText(PageObjects.FieldServicesPage.txtJobEndDate, TestData.JobDetails.enddate, true);
            SeleniumActions.waitClick(PageObjects.FieldServicesPage.btnJobSave);
            //click on Attachment
            string resptext = SeleniumActions.getInnerText(PageObjects.WellConfigurationPage.Toaseter);
            CommonHelper.TraceLine(resptext);
            Assert.AreEqual("Added Successfully", resptext, "Job Created Toast Message");
           
        }


        public void CreateEvent(ExtentTest test)
        {
            SeleniumActions.WaitForLoad();
            SeleniumActions.WaitforWellcountNonZero();
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.FieldServicesPage.btnAddJob);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.FieldServicesPage.lnkAddJob);
            //   SeleniumActions.waitClickNG(PageObjects.FieldServicesPage.ddKendoJobType);
            SeleniumActions.KendoTypeNSelect(PageObjects.FieldServicesPage.kendosearchbarEventType, "Acidize");
            SeleniumActions.sendText(PageObjects.FieldServicesPage.txtEvtdurationHours, "5");
            SeleniumActions.sendText(PageObjects.FieldServicesPage.txtEvtBegindate, "010120181200", true);
            SeleniumActions.sendText(PageObjects.FieldServicesPage.txtEvtEnddate,"010120191200", true);
            SeleniumActions.KendoTypeNSelect(PageObjects.FieldServicesPage.ddkendoServiceProviderEvnt, "Weatherford, Inc.");
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.FieldServicesPage.btnAddSingleEventSave);
            string toasttext =SeleniumActions.getInnerText(PageObjects.WellConfigurationPage.Toaseter);
            CommonHelper.TraceLine("Got text as " + toasttext);
            SeleniumActions.takeScreenshot("Add Event Success");
            test.Pass("Add Event Success: Resore event permission", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine(ExtentFactory.screenshotdir, "Add Event Success.png"))).Build());
            Assert.AreEqual("Events Saved Successfully", toasttext, "Event Save Text toast mismatch");
        }

        public void CreateJobCostDetails(ExtentTest test)
        {
            SeleniumActions.WaitForLoad();
            SeleniumActions.WaitforWellcountNonZero();
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.FieldServicesPage.btnAddJobCostDetails);
            SeleniumActions.WaitForLoad();
            SeleniumActions.sendText(PageObjects.FieldServicesPage.txt_jobcostDate, "01012018", true);
            SeleniumActions.selectDropdownValue(PageObjects.FieldServicesPage.ddVendor, "Weatherford, Inc.");
            SeleniumActions.selectDropdownValue(PageObjects.FieldServicesPage.ddCatalogItem, "N/A");
            SeleniumActions.waitClick(PageObjects.FieldServicesPage.btn_SaveJobCostDetails);
            string toasttext = SeleniumActions.getInnerText(PageObjects.WellConfigurationPage.Toaseter);
            CommonHelper.TraceLine("Got text as " + toasttext);
            SeleniumActions.takeScreenshot("Add Job Cost Details Success");
            test.Pass("Add Job Cost Details Success: Resore Add Job Cost Details permission", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine(ExtentFactory.screenshotdir, "Add Job Cost Details Success.png"))).Build());
            Assert.AreEqual("Added Successfully", toasttext, "Resore Add Job Cost Details mismatch");
        }

        //Get Collection of Filters

        public void AttachDocumenttofirstJobRestore(ExtentTest test)
        {
            SeleniumActions.waitClickJS(PageObjects.FieldServicesPage.btnAttcahment0);
            
            int category_count = PageObjects.FieldServicesPage.documentManager_Categories.Count();
            CommonHelper.TraceLine("Total number of Document categories :" + category_count);
            Assert.AreEqual(category_count, PageObjects.FieldServicesPage.btns_AddFile.Count(), "Categories count & Add File buttons count are not matching");
            int scroll_Height = 100, i = 1;
            string attachmentname = "ForeSiteSplashTrans.png";
            foreach (IWebElement folder in PageObjects.FieldServicesPage.documentManager_Categories)
            {
                PageObjects.FieldServicesPage.iDynamicIndex = i;
                SeleniumActions.performStalenessCheck(PageObjects.FieldServicesPage.documentManager_CategoriesBy);
                string foldertext = SeleniumActions.getInnerText(PageObjects.FieldServicesPage.documentManager_CategoriesBy);
                CommonHelper.TraceLine("Text of Document category " + i + " : " + foldertext);
                Assert.IsTrue( SeleniumActions.getEnabledState(PageObjects.FieldServicesPage.btns_AddFileByNS), "Add File button is disabled for category : " + foldertext);
                CommonHelper.TraceLine("Add File button is enabled for document category : " + foldertext);
                string filepath = Path.Combine(FilesLocation + @"\UIAutomation\TestData", attachmentname);
                SeleniumActions.waitClick(PageObjects.FieldServicesPage.btns_AddFileByNS);
                SeleniumActions.FileUploadDialog(filepath);
                SeleniumActions.WaitForLoad();
                string toastext = SeleniumActions.getInnerText(PageObjects.WellConfigurationPage.Toaseter);
                CommonHelper.TraceLine("Got Innertext from File Upload Toast as " + toastext);
                Assert.AreEqual(toastext, "Uploaded document successfully","Succes for toaster");
                SeleniumActions.Scrollpage(scroll_Height);
                i = i + 1;
            }
            SeleniumActions.takeScreenshot("RestoreAddCoument");
            test.Pass("Buttons are Enabled", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine(ExtentFactory.screenshotdir, "RestoreAddCoument.png"))).Build());
            test.Info("Attached document for Restore Permission was Tested ");

            Thread.Sleep(4000);
            SeleniumActions.WaitForLoad();
        }


        public void AttachDocumenttofirstJobRevoke(ExtentTest test)
        {
            try
            {
                SeleniumActions.waitClickJS(PageObjects.FieldServicesPage.btnAttcahment0);
                int category_count = PageObjects.FieldServicesPage.documentManager_Categories.Count();
                CommonHelper.TraceLine("Total number of Document categories :" + category_count);
                Assert.AreEqual(category_count, PageObjects.FieldServicesPage.btns_AddFile.Count(), "Categories count & Add File buttons count are not matching");
                int scroll_Height = 100, i = 1;
                string attachmentname = "ForeSiteSplashTrans.png";

                foreach (IWebElement folder in PageObjects.FieldServicesPage.documentManager_Categories)
                {
                    PageObjects.FieldServicesPage.iDynamicIndex = i;
                    string foldertext = SeleniumActions.getInnerText(PageObjects.FieldServicesPage.documentManager_CategoriesBy);
                    CommonHelper.TraceLine("Text of Document category " + i + " : " + foldertext);
                    Assert.IsFalse(SeleniumActions.getEnabledState(PageObjects.FieldServicesPage.btns_AddFileBy), "Add File button is Enabled for category  after revoke permission: " + foldertext);
                    CommonHelper.TraceLine("Add File button is disabled for document category : " + foldertext);
                    string filepath = Path.Combine(FilesLocation + @"\UIAutomation\TestData", attachmentname);
                    SeleniumActions.Scrollpage(scroll_Height);
                    i = i + 1;
                }
                SeleniumActions.takeScreenshot("RevokeAddCoument");
                test.Pass("Buttons are Disabled ", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine(ExtentFactory.screenshotdir, "RevokeAddCoument.png"))).Build());
                SeleniumActions.waitClick(PageObjects.FieldServicesPage.btnFileUploadClose);
                test.Info("Attached document for Revoke Permission was Tested ");

            }
            catch(Exception e)
            {
                SeleniumActions.takeScreenshot("RevokeAddCoument");
                test.Fail("Excpetion in Method ", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine(ExtentFactory.screenshotdir, "RevokeAddCoument.png"))).Build());
                Assert.Fail("Exception in AttachDocumenttofirstJobRevoke" + e.ToString());
            }
        }

        public void VerifyAddComponentPermission(ExtentTest test)
        {

            try
            {
                SeleniumActions.refreshBrowser();
                CreateJob(test);
                SeleniumActions.WaitforWellcountNonZero();
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(PageObjects.FieldServicesPage.btn_AddTabBy);
                SeleniumActions.waitClick(PageObjects.FieldServicesPage.wellboreTabBy);
                SeleniumActions.WaitForLoad();
                SeleniumActions.takeScreenshot("Wellbore Tab");
                test.Pass("Wellbore tab is opened");
                if (!SeleniumActions.getEnabledState(PageObjects.FieldServicesPage.btn_componentAdd) && !SeleniumActions.getEnabledState(PageObjects.FieldServicesPage.btn_componentQuickAdd))
                {
                    test.Pass("Add button & Quick Add buttons are disabled on Wellbore tab", MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine(ExtentFactory.screenshotdir, "Wellbore Tab.png"))).Build());
                }
                else
                {
                    test.Pass("Add button & Quick Add buttons are enabled on Wellbore tab even though AddComponent permission is removed", MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine(ExtentFactory.screenshotdir, "Wellbore Tab.png"))).Build());
                }

            }
            catch (Exception e)
            {

                Assert.Fail("VerifyAddComponentPermission Failed" + e.ToString());
                CommonHelper.TraceLine("Error : " + e.ToString());
            }
        }

        public void VerifyRestoreAddComponentPermission(ExtentTest test)
        {

        }
    }
}
