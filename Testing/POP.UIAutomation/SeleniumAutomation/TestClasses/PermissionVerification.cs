using AventStack.ExtentReports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SeleniumAutomation.SeleniumObject;
using System.Configuration;
using SeleniumAutomation.Helper;
using System.Threading;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using System.IO;

namespace SeleniumAutomation.TestClasses
{
    class PermissionVerification
    {
        string IsRunningATS = ConfigurationManager.AppSettings.Get("IsRunningInATS");
        public static string FilesLocation = ConfigurationManager.AppSettings.Get("FilesLocation");
        public void VerifyRevokeAddWellPermission(ExtentTest test)
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
                    test.Pass("Create New Well Button is not displayed when Add Well Permission is removed", MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Add Well Remove.png"))).Build());
                else
                    test.Fail("Create New Well Button is still displayed even though Add Well Permission is removed");
                test.Info("Verified AddWell Permission by Revoking the Permission");
            }
            catch (Exception e)
            {
                Assert.Fail("VerifyRevokeAddWellPermission Failed" + e.ToString());
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
                SeleniumActions.takeScreenshot("AddWellRestore");
                if (SeleniumActions.isElemPresent(PageObjects.WellConfigurationPage.btnCreateNewWellby))
                    test.Pass("Create New Well Button is displayed when Add Well Permission is restored", MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "AddWellRestore.png"))).Build());
                else
                    test.Fail("Create New Well Button is still not displayed even though Add Well Permission is restored");

                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.btnCreateNewWell);

                test.Info("Verified AddWell Permission by Restoring the Permission");
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

                SeleniumActions.takeScreenshot("RemoveWellRevoke");
                if (!SeleniumActions.isElemPresent(PageObjects.WellConfigurationPage.btnDeleteWellby))
                    test.Pass("Delete  Well Button is not displayed when Delete Well Permission is removed", MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "RemoveWellRevoke.png"))).Build());
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
                SeleniumActions.takeScreenshot("RemoveWellRestore");
                if (SeleniumActions.isElemPresent(PageObjects.WellConfigurationPage.btnDeleteWellby))
                    test.Pass("Delete  Well Button is displayed when Delete Well Permission is restored", MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "RemoveWellRestore.png"))).Build());
                else
                    test.Fail("Delete  Well Button is still not displayed even though Add Well Permission is restored");

                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.deletebutton);
                string text = SeleniumActions.getInnerText(PageObjects.WellConfigurationPage.Dialogtitle);
                Assert.AreEqual("Delete Well", text, "Delete Well dailog did not appearon clciking delete");
                SeleniumActions.takeScreenshot("DeleteWellConfirm");
                test.Info("Verified Remove Well Permission by Restoring the Permission", MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "DeleteWellConfirm.png"))).Build());
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
            SeleniumActions.waitClick(PageObjects.JobManagementPage.firstJobId);
            RestoreAttachDocumenttofirstJob(test);
        }

        public void VerifyRevokeAddDocumentPermission(ExtentTest test)
        {

            SeleniumActions.refreshBrowser();
            CreateJob(test);
            RevokeAttachDocumenttofirstJob(test);
        }

        public void VerifyRevokeRemoveDocumentPermission(ExtentTest test)
        {

            SeleniumActions.refreshBrowser();
            CreateJob(test);
            RestoreAttachDocumenttofirstJob(test);
            RevokeRemoveDocumentofirstJob(test);
        }

        public void VerifyRestoreRemoveDocumentPermission(ExtentTest test)
        {
            SeleniumActions.refreshBrowser();
            SeleniumActions.waitforPageloadComplete();
            SeleniumActions.waitClick(PageObjects.JobManagementPage.firstJobId);
            RestoreRemoveDocumentofirstJob(test);
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
                SeleniumActions.takeScreenshot("AddJobRevoke");
                if (!SeleniumActions.getEnabledState(PageObjects.FieldServicesPage.btnAddJob))
                {
                    test.Pass("Add Job button is disabled when permisison was revoked", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "AddJobRevoke.png"))).Build());

                }
                else
                {
                    test.Fail("Add Job button is enabled when permisison was revoked", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "AddJobRevoke.png"))).Build());
                    Assert.Fail("Add Job Test failed as button is in enabled state when permission was revoked.");
                }

                test.Info("Verified AddJob Permission by Revoking the Permission");
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("AddJobRevokeException");
                test.Fail("Revoke Add Job Exception", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "AddJobRevokeException.png"))).Build());
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
                SeleniumActions.takeScreenshot("AddJobRestore");
                if (SeleniumActions.getEnabledState(PageObjects.FieldServicesPage.btnAddJob))
                {
                    test.Pass("Add Job button is enabled when permisison was restored", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "AddJobRestore.png"))).Build());

                }
                else
                {
                    test.Fail("Add Job button is disabled when permisison was restored", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "AddJobRestore.png"))).Build());
                    Assert.Fail("Add Job Test failed as button is in disabled state when permission was restored.");
                }
                test.Info("Verified AddJob Permission by Restoring the Permission");
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("AddJobRestoreException");
                test.Fail("Restore Add Job Exception", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "AddJobRestoreException.png"))).Build());
                test.Info("Exception from VerifyRestoreAddJobPermission" + e.ToString());
                Assert.Fail("VerifyRestoreAddJobPermission Failed" + e.ToString());
            }
        }

        public void VerifyRevokeRemoveJobPermission(ExtentTest test)
        {
            try
            {
                SeleniumActions.refreshBrowser();
                SeleniumActions.waitforPageloadComplete();
                //Navigate to Field Services Tab and check for Delete Job button state (Enabled or Disabled)
                SeleniumActions.WaitForLoad();
                SeleniumActions.WaitforWellcountNonZero();
                SeleniumActions.waitClick(PageObjects.JobManagementPage.firstJobId);
                SeleniumActions.WaitForLoad();
                SeleniumActions.takeScreenshot("RemoveJobRevoke");
                if (!SeleniumActions.getEnabledState(PageObjects.FieldServicesPage.btnDeleteJob))
                {
                    test.Pass("Delete Job button is disabled when permisison was revoked", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "RemoveJobRevoke.png"))).Build());

                }
                else
                {
                    test.Fail("Delete Job button is enabled when permisison was revoked", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "RemoveJobRevoke.png"))).Build());
                    Assert.Fail("Delete Job Test failed as button is in enabled state when permission was revoked.");
                }

                test.Info("Verified RemoveJob Permission by Revoking the Permission");
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("RemoveJobRevokeException");
                test.Fail("Revoke Remove Job Exception", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "RemoveJobRevokeException.png"))).Build());
                test.Info("Exception from VerifyRevokeRemoveJobPermission" + e.ToString());
                Assert.Fail("VerifyRevokeRemoveJobPermission Failed" + e.ToString());
            }
        }

        public void VerifyRestoreRemoveJobPermission(ExtentTest test)
        {
            try
            {
                SeleniumActions.refreshBrowser();
                SeleniumActions.waitforPageloadComplete();
                //Navigate to Field Services Tab and check for Delete Job button state (Enabled or Disabled)
                SeleniumActions.WaitForLoad();
                SeleniumActions.WaitforWellcountNonZero();
                SeleniumActions.waitClick(PageObjects.JobManagementPage.firstJobId);
                SeleniumActions.WaitForLoad();
                SeleniumActions.takeScreenshot("RemoveJobRestore");
                if (SeleniumActions.getEnabledState(PageObjects.FieldServicesPage.btnDeleteJob))
                {
                    test.Pass("Delete Job button is enabled when permisison was restored", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "RemoveJobRestore.png"))).Build());
                }
                else
                {
                    test.Fail("Delete Job button is disabled when permisison was restored", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "RemoveJobRestore.png"))).Build());
                    Assert.Fail("Delete Job Test failed as button is in disabled state when permission was restored.");
                }
                test.Info("Verified RemoveJob Permission by Restoring the Permission");
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("RemoveJobRestoreException");
                test.Fail("Restore Delete Job Exception", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "RemoveJobRestoreException.png"))).Build());
                test.Info("Exception from VerifyRestoreRemoveJobPermission" + e.ToString());
                Assert.Fail("VerifyRestoreRemoveJobPermission Failed" + e.ToString());
            }
        }

        public void VerifyRevokeUpdateJobPermission(ExtentTest test)
        {
            try
            {

                SeleniumActions.refreshBrowser();
                SeleniumActions.waitforPageloadComplete();
                //Navigate to Field Services Tab and check for Update Job button state (Enabled or Disabled)
                SeleniumActions.WaitForLoad();
                SeleniumActions.WaitforWellcountNonZero();
                SeleniumActions.waitClick(PageObjects.JobManagementPage.firstJobId);
                SeleniumActions.WaitForLoad();
                SeleniumActions.takeScreenshot("UpdateJobRevoke");
                if (!SeleniumActions.getEnabledState(PageObjects.FieldServicesPage.btnUpdateJob))
                {
                    test.Pass("Update Job button is disabled when permisison was revoked", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "UpdateJobRevoke.png"))).Build());

                }
                else
                {
                    test.Fail("Update Job button is enabled when permisison was revoked", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "UpdateJobRevoke.png"))).Build());
                    Assert.Fail("Update Job Test failed as button is in enabled state when permission was revoked.");
                }

                test.Info("Verified UpdateJob Permission by Revoking the Permission");
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("UpdateJobRevokeException");
                test.Fail("Revoke Update Job Exception", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "UpdateJobRevokeException.png"))).Build());
                test.Info("Exception from VerifyRevokeUpdateJobPermission" + e.ToString());
                Assert.Fail("VerifyRevokeUpdateJobPermission Failed" + e.ToString());
            }
        }

        public void VerifyRestoreUpdateJobPermission(ExtentTest test)
        {
            try
            {
                SeleniumActions.refreshBrowser();
                SeleniumActions.waitforPageloadComplete();
                //Navigate to Field Services Tab and check for Update Job button state (Enabled or Disabled)
                SeleniumActions.WaitForLoad();
                SeleniumActions.WaitforWellcountNonZero();
                SeleniumActions.waitClick(PageObjects.JobManagementPage.firstJobId);
                SeleniumActions.WaitForLoad();
                SeleniumActions.takeScreenshot("UpdateJobRestore");
                if (SeleniumActions.getEnabledState(PageObjects.FieldServicesPage.btnUpdateJob))
                {
                    test.Pass("Update Job button is enabled when permisison was restored", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "UpdateJobRestore.png"))).Build());
                }
                else
                {
                    test.Fail("Update Job button is disabled when permisison was restored", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "UpdateJobRestore.png"))).Build());
                    Assert.Fail("Update Job Test failed as button is in disabled state when permission was restored.");
                }
                test.Info("Verified RemoveJob Permission by Restoring the Permission");
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("UpdateJobRestoreException");
                test.Fail("Restore Update Job Exception", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "UpdateJobRestoreException.png"))).Build());
                test.Info("Exception from VerifyRestoreUpdateJobPermission" + e.ToString());
                Assert.Fail("VerifyRestoreUpdateJobPermission Failed" + e.ToString());
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
                SeleniumActions.takeScreenshot("AddEventRevoke");
                if (!SeleniumActions.getEnabledState(PageObjects.FieldServicesPage.btnAddJob))
                {
                    test.Pass("Add Event button is disabled when permisison was revoked", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "AddEventRevoke.png"))).Build());

                }
                else
                {
                    test.Fail("Add Event button is enabled when permisison was revoked", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "AddEventRevoke.png"))).Build());
                    Assert.Fail("Add Event Test failed as button is in enabled state when permission was revoked.");
                }

                test.Info("Verified AddEvent Permission by Revoking the Permission");
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("AddEventRevoke");
                test.Fail("AddEventRevoke Exception", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "AddEventRevoke.png"))).Build());
                test.Info("Exception from VerifyRevokeAddEventPermission" + e.ToString());
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
                SeleniumActions.waitClick(PageObjects.JobManagementPage.firstJobId);
                SeleniumActions.waitClick(PageObjects.FieldServicesPage.tabAddEvent);
                SeleniumActions.WaitForLoad();
                SeleniumActions.takeScreenshot("Add Event Restore");
                if (SeleniumActions.getEnabledState(PageObjects.FieldServicesPage.btnAddJob))
                {
                    test.Pass("Add Event button is enabled when permisison was restored", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "Add Event Restore.png"))).Build());

                }
                else
                {
                    test.Fail("Add Event button is disabled when permisison was restored", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "Add Event Restore.png"))).Build());
                    Assert.Fail("Add Event Test failed as button is in enabled state when permission was revoked.");
                }
                CreateEvent(test);
                test.Info("Verified AddEvent Permission by Restoring the Permission");
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("AddEventRestore");
                test.Fail("AddEventRestore Exception", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "AddEventRestore.png"))).Build());
                test.Info("Exception from VerifyRestoreAddEventPermission" + e.ToString());
                Assert.Fail("Method VerifyRestoreAddEventPermission Failed.");

            }

        }

        public void VerifyRevokeRemoveEventPermission(ExtentTest test)
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
                CreateEvent(test);
                SeleniumActions.waitClick(SeleniumActions.getByLocator("Xpath", PageObjects.JobManagementPage.EventTypeCol, "First event in the grid"));
                SeleniumActions.takeScreenshot("RemoveEventRevoke");
                if (!SeleniumActions.getEnabledState(PageObjects.JobManagementPage.deleteventbutton))
                {
                    test.Pass("Delete Event button is disabled when permisison was revoked", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "RemoveEventRevoke.png"))).Build());

                }
                else
                {
                    test.Fail("Delete Event button is enabled when permisison was revoked", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "RemoveEventRevoke.png"))).Build());
                    Assert.Fail("Delete Event Test failed as button is in enabled state when permission was revoked.");
                }

                test.Info("Verified RemoveEvent Permission by Revoking the Permission");
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("RemoveEventRevoke");
                test.Fail("RemoveEventRevoke Exception", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "RemoveEventRevoke.png"))).Build());
                test.Info("Exception from VerifyRevokeRemoveEventPermission" + e.ToString());
                Assert.Fail("VerifyRevokeRemoveEventPermission Failed" + e.ToString());
            }

        }

        public void VerifyRestoreRemoveEventPermission(ExtentTest test)
        {
            try
            {
                SeleniumActions.refreshBrowser();
                SeleniumActions.waitforPageloadComplete();
                //Navigate to Field Services Tab and check for Add Job button state (Enabled or Disabled)
                SeleniumActions.WaitForLoad();
                SeleniumActions.WaitforWellcountNonZero();
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(PageObjects.JobManagementPage.firstJobId);
                SeleniumActions.waitClick(PageObjects.FieldServicesPage.tabAddEvent);
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(SeleniumActions.getByLocator("Xpath", PageObjects.JobManagementPage.EventTypeCol, "First event in the grid"));
                SeleniumActions.takeScreenshot("RemoveEventRestore");
                if (SeleniumActions.getEnabledState(PageObjects.JobManagementPage.deleteventbutton))
                {
                    test.Pass("Delete Event button is enabled when permisison was restored", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "RemoveEventRestore.png"))).Build());

                }
                else
                {
                    test.Fail("Delete Event button is disabled when permisison was restored", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "RemoveEventRestore.png"))).Build());
                    Assert.Fail("Delete Event Test failed as button is in enabled state when permission was revoked.");
                }

                test.Info("Verified RemoveEvent Permission by Restoring the Permission");
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("RemoveEventRestore");
                test.Fail("RemoveEventRestore Exception", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "RemoveEventRestore.png"))).Build());
                test.Info("Exception from VerifyRestoreRemoveEventPermission" + e.ToString());
                Assert.Fail("Method VerifyRestoreRemoveEventPermission Failed.");

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
                    test.Pass("AddJobCostDetail  button is disabled when permisison was revoked", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "AddJobCostDetail Revoke.png"))).Build());

                }
                else
                {
                    test.Fail("AddJobCostDetail button is enabled when permisison was revoked", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "AddJobCostDetail Revoke.png"))).Build());
                    Assert.Fail("AddJobCostDetail Test failed as button is in enabled state when permission was revoked.");
                }
                test.Info("Verified AddJobCostDetail Permission by Revoking the Permission");
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("AddJobCostDetailRevoke");
                test.Fail("AddJobCostDetailRevoke Exception", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "AddJobCostDetailRevoke.png"))).Build());
                test.Info("Exception from VerifyRevokeAddJobCostDetailPermission" + e.ToString());
                Assert.Fail("VerifyRevokeAddJobCostDetailPermission Failed" + e.ToString());
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
                SeleniumActions.waitClick(PageObjects.JobManagementPage.firstJobId);
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
                    test.Pass("Add Job Cost Details button is enabled when permisison was restored", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "Add Job Cost Details Restore.png"))).Build());

                }
                else
                {
                    test.Fail("Add Job Cost Detailsbutton is disabled when permisison was restored", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "Add Job Cost Details Restore.png"))).Build());
                    Assert.Fail("Add Job Cost Details Test failed as button is in disabled state when permission was restored.");
                }
                CreateJobCostDetails(test);
                test.Info("Verified AddJobCostDetail Permission by Restoring the Permission");

            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("AddJobCostDetailRestore");
                test.Fail("AddJobCostDetailRestore Exception", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "AddJobCostDetailRestore.png"))).Build());
                test.Info("Exception from VerifyRestoreAddJobCostDetailPermission" + e.ToString());
                Assert.Fail("Method VerifyRestoreAddJobCostDetailPermission Failed.");

            }


        }
        public void CreateJob(ExtentTest test)
        {
            SeleniumActions.refreshBrowser();
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
            SeleniumActions.sendText(PageObjects.FieldServicesPage.txtEvtEnddate, "010120191200", true);
            SeleniumActions.KendoTypeNSelect(PageObjects.FieldServicesPage.ddkendoServiceProviderEvnt, "Weatherford, Inc.");
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.FieldServicesPage.btnAddSingleEventSave);
            string toasttext = SeleniumActions.getInnerText(PageObjects.WellConfigurationPage.Toaseter);
            CommonHelper.TraceLine("Got text as " + toasttext);
            SeleniumActions.takeScreenshot("AddEventSuccess");
            test.Pass("Event added Successfully", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "AddEventSuccess.png"))).Build());
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
            SeleniumActions.selectDropdownValue(PageObjects.FieldServicesPage.ddCatalogItem, " N/A - N/A ");
            SeleniumActions.waitClick(PageObjects.FieldServicesPage.btn_SaveJobCostDetails);
            string toasttext = SeleniumActions.getInnerText(PageObjects.WellConfigurationPage.Toaseter);
            CommonHelper.TraceLine("Got text as " + toasttext);
            SeleniumActions.takeScreenshot("Add Job Cost Details Success");
            test.Pass("Add Job Cost Details Success: Resore Add Job Cost Details permission", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "Add Job Cost Details Success.png"))).Build());
            Assert.AreEqual("Added Successfully", toasttext, "Resore Add Job Cost Details mismatch");
        }

        //Get Collection of Filters

        public void RestoreAttachDocumenttofirstJob(ExtentTest test)
        {
            SeleniumActions.waitForElement(PageObjects.FieldServicesPage.btnAttcahment0);
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
                Assert.IsTrue(SeleniumActions.getEnabledState(PageObjects.FieldServicesPage.btns_AddFileByNS), "Add File button is disabled for category : " + foldertext);
                CommonHelper.TraceLine("Add File button is enabled for document category : " + foldertext);
                string filepath = Path.Combine(FilesLocation + @"\UIAutomation\TestData", attachmentname);
                SeleniumActions.waitClick(PageObjects.FieldServicesPage.btns_AddFileByNS);
                SeleniumActions.FileUploadDialog(filepath);
                SeleniumActions.WaitForLoad();
                string toastext = SeleniumActions.getInnerText(PageObjects.WellConfigurationPage.Toaseter);
                CommonHelper.TraceLine("Got Innertext from File Upload Toast as " + toastext);
                Assert.AreEqual(toastext, "Uploaded document successfully", "Succes for toaster");
                SeleniumActions.Scrollpage(scroll_Height);
                i = i + 1;
            }
            SeleniumActions.takeScreenshot("RestoreAddDocument");
            test.Pass("Add File Buttons are Enabled", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "RestoreAddDocument.png"))).Build());
            test.Info("Verified Add Document Permission by Restoring the Permission");

            Thread.Sleep(4000);
            SeleniumActions.WaitForLoad();
        }


        public void RevokeAttachDocumenttofirstJob(ExtentTest test)
        {
            try
            {
                SeleniumActions.waitForElement(PageObjects.FieldServicesPage.btnAttcahment0);
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
                test.Pass("Add File Buttons are Disabled ", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "RevokeAddCoument.png"))).Build());
                SeleniumActions.waitClick(PageObjects.FieldServicesPage.btnFileUploadClose);
                test.Info("Verified Add Document Permission by Revoking the Permission");

            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("RevokeAddCoument");
                test.Fail("Excpetion in Method ", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "RevokeAddCoument.png"))).Build());
                Assert.Fail("Exception in AttachDocumenttofirstJobRevoke" + e.ToString());
            }
        }

        public void RevokeRemoveDocumentofirstJob(ExtentTest test)
        {
            try
            {
                SeleniumActions.waitForElement(PageObjects.FieldServicesPage.btnAttcahment0);
                SeleniumActions.waitClickJS(PageObjects.FieldServicesPage.btnAttcahment0);
                int category_count = PageObjects.FieldServicesPage.documentManager_Categories.Count();
                CommonHelper.TraceLine("Total number of Document categories :" + category_count);
                Assert.AreEqual(category_count, PageObjects.FieldServicesPage.checkboxs_FileSelection.Count(), "Categories count & checkboxes count are not matching");
                int scroll_Height = 100, i = 1;

                foreach (IWebElement checkbox in PageObjects.FieldServicesPage.checkboxs_FileSelection)
                {
                    PageObjects.FieldServicesPage.iDynamicIndex = i;
                    SeleniumActions.waitClick(checkbox);
                    SeleniumActions.Scrollpage(scroll_Height);
                    i = i + 1;
                }
                Assert.IsFalse(SeleniumActions.getEnabledState(PageObjects.FieldServicesPage.btnDeleteAttcahment), "Delete File button is Enabled for category  after revoke permission");
                SeleniumActions.takeScreenshot("RevokeRemoveDocument");
                test.Pass("Delete Button is Disabled ", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "RevokeRemoveDocument.png"))).Build());
                SeleniumActions.waitClick(PageObjects.FieldServicesPage.btnFileUploadClose);
                test.Info("Verified RemoveDocument Permission by Revoking the Permission");

            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("RevokeRemoveCoument");
                test.Fail("Excpetion in Method ", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "RevokeRemoveDocument.png"))).Build());
                Assert.Fail("Exception in RemoveDocumentofirstJobRevoke" + e.ToString());
            }
        }

        public void RestoreRemoveDocumentofirstJob(ExtentTest test)
        {
            try
            {
                SeleniumActions.waitForElement(PageObjects.FieldServicesPage.btnAttcahment0);
                SeleniumActions.waitClickJS(PageObjects.FieldServicesPage.btnAttcahment0);
                int category_count = PageObjects.FieldServicesPage.documentManager_Categories.Count();
                CommonHelper.TraceLine("Total number of Document categories :" + category_count);
                Assert.AreEqual(category_count, PageObjects.FieldServicesPage.checkboxs_FileSelection.Count(), "Categories count & checkboxes count are not matching");
                int scroll_Height = 100, i = 1;

                foreach (IWebElement checkbox in PageObjects.FieldServicesPage.checkboxs_FileSelection)
                {
                    PageObjects.FieldServicesPage.iDynamicIndex = i;
                    SeleniumActions.waitClick(checkbox);
                    SeleniumActions.Scrollpage(scroll_Height);
                    i = i + 1;
                }
                Assert.IsFalse(!SeleniumActions.getEnabledState(PageObjects.FieldServicesPage.btnDeleteAttcahment), "Delete File button is not Enabled for category even after restore permission");
                SeleniumActions.takeScreenshot("RestoreRemoveDocument");
                test.Pass("Delete Button is enabled ", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "RestoreRemoveDocument.png"))).Build());
                SeleniumActions.waitClick(PageObjects.FieldServicesPage.btnFileUploadClose);
                test.Info("Verified RemoveDocument Permission by Restoring the Permission");

            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("RestoreRemoveCoument");
                test.Fail("Excpetion in Method ", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "RestoreRemoveDocument.png"))).Build());
                Assert.Fail("Exception in RestoreRemoveDocumentofirstJob" + e.ToString());
            }
        }

        public void VerifyRevokeAddComponentPermission(ExtentTest test)
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
                SeleniumActions.takeScreenshot("RevokeAddComponent");
                test.Pass("Wellbore tab is opened");
                if (!SeleniumActions.getEnabledState(PageObjects.FieldServicesPage.btn_componentAdd) && !SeleniumActions.getEnabledState(PageObjects.FieldServicesPage.btn_componentQuickAdd))
                {
                    test.Pass("Add button & Quick Add buttons are disabled on Wellbore tab", MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "RevokeAddComponent.png"))).Build());
                }
                else
                {
                    test.Pass("Add button & Quick Add buttons are enabled on Wellbore tab even though AddComponent permission is removed", MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "RevokeAddComponent.png"))).Build());
                }
                test.Info("Verified AddComponent Permission by Revoking the Permission");
            }
            catch (Exception e)
            {

                Assert.Fail("VerifyRevokeAddComponentPermission Failed" + e.ToString());
                CommonHelper.TraceLine("Error : " + e.ToString());
            }
        }

        public void VerifyRestoreAddComponentPermission(ExtentTest test)
        {
            try
            {
                SeleniumActions.refreshBrowser();
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(PageObjects.JobManagementPage.firstJobId);
                SeleniumActions.waitClick(PageObjects.FieldServicesPage.btn_AddTabBy);
                SeleniumActions.waitClick(PageObjects.FieldServicesPage.wellboreTabBy);
                SeleniumActions.WaitForLoad();
                SeleniumActions.takeScreenshot("RestoreAddComponent");
                test.Pass("Wellbore tab is opened");
                if (SeleniumActions.getEnabledState(PageObjects.FieldServicesPage.btn_componentAdd) && SeleniumActions.getEnabledState(PageObjects.FieldServicesPage.btn_componentQuickAdd))
                {
                    test.Pass("Add button & Quick Add buttons are enabled on Wellbore tab", MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "RestoreAddComponent.png"))).Build());
                }
                else
                {
                    test.Pass("Add button & Quick Add buttons are diisabled on Wellbore tab even though AddComponent permission is added", MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "RestoreAddComponent.png"))).Build());
                }
                test.Info("Verified AddComponent Permission by Restoring the Permission");
            }
            catch (Exception e)
            {

                Assert.Fail("VerifyRestoreAddComponentPermission Failed" + e.ToString());
                CommonHelper.TraceLine("Error : " + e.ToString());
            }
        }
    }
}
