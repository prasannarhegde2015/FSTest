
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeleniumAutomation.TestClasses;
using SeleniumAutomation.SeleniumObject;
using AventStack.ExtentReports;
using SeleniumAutomation.Helper;
using Weatherford.POP.Enums;
using System.Configuration;
using System;

namespace SeleniumAutomation
{
    /// <summary>
    /// UI Automation Test Suite.
    /// </summary>
    [TestClass]
    public class UserPermissionTests
    {
        public UserPermissionTests()
        {
        }
        static ExtentReports report;
        ExtentTest test;

        WellConfiguration config = new WellConfiguration();
        PermissionVerification perm = new PermissionVerification();

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
            CommonHelper.CreateNewRRLWellwithFullData();
            SeleniumActions.InitializeWebDriver();
            Assert.AreEqual(SeleniumAutomation.TestData.WellConfigData.HomePageTitle, SeleniumActions.getBrowserTitle(), "Title is NOT matched ");
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.DashboardPage.tabDashboards);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.DashboardPage.tabProdDashboard);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitForElement(SeleniumAutomation.PageObjects.WellConfigurationPage.pedashboard);
            SeleniumActions.WaitForLoad();
        }

        public void ForeSiteTest()
        {
            test = report.CreateTest("RRL");
            test.Pass("RRL Test Started");
            CommonHelper.TraceLine("ForeSiteTest");
            config.CreateWellWorkFlow(test);
            test.Pass("RRL Test Finished");
        }

        [TestCategory("UserPermissions"), TestMethod]
        public void AddWellPermission()
        {
            try
            {
                test = report.CreateTest("Add Well User Permission");
                test.Info("Verifying Application Behaviour when Add Well Permission is removed");

                //Add new Role & Remove Add Well Permission from that Role
                CommonHelper.CreateRole("TestRole");
                CommonHelper.RemovePermissionFromRole("TestRole", PermissionId.AddWell, "AddWell", test);

                CommonHelper.updateUserWithGivenRole("TestRole");
                perm.VerifyRevokeAddWellPermission(test);
                CommonHelper.AddPermissioninRole("TestRole", PermissionId.AddWell, "AddWell", test);
                perm.VerifyRestoreAddWellPermission(test);
                CommonHelper.updateUserWithGivenRole("TestRole");
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("AddWellPermissionException");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "AddWellPermissionException.png"))).Build());
                Assert.Fail(e.ToString());
            }

        }


        [TestCategory("UserPermissions"), TestMethod]
        public void RemoveWellPermission()
        {
            try
            {
                test = report.CreateTest("Remove Well User Permission");
                test.Info("Verifying Application Behaviour when Remove Well Permission is removed");

                //Add new Role & Remove Add Well Permission from that Role
                CommonHelper.CreateRole("TestRole");
                CommonHelper.RemovePermissionFromRole("TestRole", PermissionId.RemoveWell, "RemoveWell", test);
                CommonHelper.updateUserWithGivenRole("TestRole");
                perm.VerifyRevokeRemoveWellPermission(test);
                CommonHelper.AddPermissioninRole("TestRole", PermissionId.RemoveWell, "RemoveWell", test);
                perm.VerifyRestoreRemoveWellPermission(test);
                CommonHelper.updateUserWithGivenRole("TestRole");
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("RemoveWellPermissionException");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "RemoveWellPermissionException.png"))).Build());
                Assert.Fail(e.ToString());
            }


        }
        [TestCategory("UserPermissions"), TestMethod]
        public void AddDocumentPermission()
        {
            try
            {
                test = report.CreateTest("AddDocument Permission");
                CommonHelper.CreateRole("TestRole");
                CommonHelper.RemovePermissionFromRole("TestRole", PermissionId.AddDocument, "AddDocument", test);
                CommonHelper.updateUserWithGivenRole("TestRole");
                perm.VerifyRevokeAddDocumentPermission(test);
                CommonHelper.AddPermissioninRole("TestRole", PermissionId.AddDocument, "AddDocument", test);
                perm.VerifyRestoreAddDocumentPermission(test);
                CommonHelper.updateUserWithGivenRole("TestRole");
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("AddDocumentPermissionException");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "AddDocumentPermissionException.png"))).Build());
                Assert.Fail(e.ToString());
            }

        }

        [TestCategory("UserPermissions"), TestMethod]
        public void RemoveDocumentPermission()
        {
            try
            {
                test = report.CreateTest("RemoveDocument Permission");
                CommonHelper.CreateRole("TestRole");
                CommonHelper.RemovePermissionFromRole("TestRole", PermissionId.RemoveDocument, "RemoveDocument", test);
                CommonHelper.updateUserWithGivenRole("TestRole");
                perm.VerifyRevokeRemoveDocumentPermission(test);
                CommonHelper.AddPermissioninRole("TestRole", PermissionId.RemoveDocument, "RemoveDocument", test);
                perm.VerifyRestoreRemoveDocumentPermission(test);
                CommonHelper.updateUserWithGivenRole("TestRole");
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("RemoveDocumentPermissionException");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "RemoveDocumentPermissionException.png"))).Build());
                Assert.Fail(e.ToString());
            }

        }

        [TestCategory("UserPermissions"), TestMethod]
        public void AddJobPermission()
        {
            try
            {
                test = report.CreateTest("AddJob Permission");
                CommonHelper.CreateRole("TestRole");
                CommonHelper.RemovePermissionFromRole("TestRole", PermissionId.AddJob, "AddJob", test);
                CommonHelper.updateUserWithGivenRole("TestRole");
                perm.VerifyRevokeAddJobPermission(test);
                CommonHelper.AddPermissioninRole("TestRole", PermissionId.AddJob, "AddJob", test);
                perm.VerifyRestoreAddJobPermission(test);
                CommonHelper.updateUserWithGivenRole("TestRole");
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("AddJobPermissionException");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "AddJobPermissionException.png"))).Build());
                Assert.Fail(e.ToString());
            }

        }

        [TestCategory("UserPermissions"), TestMethod]
        public void RemoveJobPermission()
        {
            try
            {
                test = report.CreateTest("RemoveJob Permission");
                CommonHelper.CreateRole("TestRole");
                perm.CreateJob(test);
                CommonHelper.RemovePermissionFromRole("TestRole", PermissionId.RemoveJob, "RemoveJob", test);
                CommonHelper.updateUserWithGivenRole("TestRole");
                perm.VerifyRevokeRemoveJobPermission(test);
                CommonHelper.AddPermissioninRole("TestRole", PermissionId.RemoveJob, "RemoveJob", test);
                perm.VerifyRestoreRemoveJobPermission(test);
                CommonHelper.updateUserWithGivenRole("TestRole");
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("RemoveJobPermissionException");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "RemoveJobPermissionException.png"))).Build());
                Assert.Fail(e.ToString());
            }

        }

        [TestCategory("UserPermissions"), TestMethod]
        public void UpdateJobPermission()
        {
            try
            {
                test = report.CreateTest("UpdateJob Permission");
                CommonHelper.CreateRole("TestRole");
                perm.CreateJob(test);
                CommonHelper.RemovePermissionFromRole("TestRole", PermissionId.UpdateJob, "UpdateJob", test);
                CommonHelper.updateUserWithGivenRole("TestRole");
                perm.VerifyRevokeUpdateJobPermission(test);
                CommonHelper.AddPermissioninRole("TestRole", PermissionId.UpdateJob, "UpdateJob", test);
                perm.VerifyRestoreUpdateJobPermission(test);
                CommonHelper.updateUserWithGivenRole("TestRole");
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("UpdateJobPermissionException");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "UpdateJobPermission.png"))).Build());
                Assert.Fail(e.ToString());
            }

        }

        [TestCategory("UserPermissions"), TestMethod]
        public void AddEventPermission()
        {
            try
            {
                test = report.CreateTest("AddEvent Permission");
                CommonHelper.CreateRole("TestRole");
                CommonHelper.RemovePermissionFromRole("TestRole", PermissionId.AddEvent, "AddEvent", test);
                CommonHelper.updateUserWithGivenRole("TestRole");
                perm.VerifyRevokeAddEventPermission(test);
                CommonHelper.AddPermissioninRole("TestRole", PermissionId.AddEvent, "AddEvent", test);
                perm.VerifyRestoreAddEventPermission(test);
                CommonHelper.updateUserWithGivenRole("TestRole");
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("AddEventPermissionException");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "AddEventPermissionException.png"))).Build());
                Assert.Fail(e.ToString());
            }

        }

        [TestCategory("UserPermissions"), TestMethod]
        public void RemoveEventPermission()
        {
            try
            {
                test = report.CreateTest("RemoveEvent Permission");
                CommonHelper.CreateRole("TestRole");
                CommonHelper.RemovePermissionFromRole("TestRole", PermissionId.RemoveEvent, "RemoveEvent", test);
                CommonHelper.updateUserWithGivenRole("TestRole");
                perm.VerifyRevokeRemoveEventPermission(test);
                CommonHelper.AddPermissioninRole("TestRole", PermissionId.RemoveEvent, "RemoveEvent", test);
                perm.VerifyRestoreRemoveEventPermission(test);
                CommonHelper.updateUserWithGivenRole("TestRole");
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("RemoveEventPermissionException");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "RemoveEventPermissionException.png"))).Build());
                Assert.Fail(e.ToString());
            }

        }

        [TestCategory("UserPermissions"), TestMethod]
        public void AddJobCostDetailPermission()
        {
            try
            {
                test = report.CreateTest("AddJobCostDetail Permission");
                CommonHelper.CreateRole("TestRole");
                CommonHelper.RemovePermissionFromRole("TestRole", PermissionId.AddJobCostDetail, "AddJobCostDetail", test);
                CommonHelper.updateUserWithGivenRole("TestRole");
                perm.VerifyRevokeAddJobCostDetailPermission(test);
                CommonHelper.AddPermissioninRole("TestRole", PermissionId.AddJobCostDetail, "AddJobCostDetail", test);
                perm.VerifyRestoreAddJobCostDetailPermission(test);
                CommonHelper.updateUserWithGivenRole("TestRole");
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("AddJobCostDetailPermission");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "AddJobCostDetailPermission.png"))).Build());
                Assert.Fail(e.ToString());
            }

        }

        [TestCategory("UserPermissions"), TestMethod]
        public void AddComponentPermission()
        {
            test = report.CreateTest("AddComponent Permission");
            try
            {
                CommonHelper.CreateRole("TestRole");
                CommonHelper.RemovePermissionFromRole("TestRole", PermissionId.AddComponent, "AddComponent", test);
                CommonHelper.updateUserWithGivenRole("TestRole");
                perm.VerifyRevokeAddComponentPermission(test);
                CommonHelper.AddPermissioninRole("TestRole", PermissionId.AddComponent, "AddComponent", test);
                perm.VerifyRestoreAddComponentPermission(test);
                CommonHelper.updateUserWithGivenRole("TestRole");
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("AddComponentPermissionException");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "AddComponentPermissionException.png"))).Build());
                Assert.Fail(e.ToString());
            }

        }

        [TestCleanup]
        public void clean()
        {
            SeleniumActions.disposeDriver();
            CommonHelper.updateUserWithGivenRole("Administrator");
            CommonHelper.ApiUITestBase.Cleanup();
        }

        [ClassCleanup]
        public static void TearDown()
        {
            report.Flush();
        }
    }
}
