
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
    public class TestSuite
    {
        public TestSuite()
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
            SeleniumActions.InitializeWebDriver();
            Assert.AreEqual(SeleniumAutomation.TestData.WellConfigData.HomePageTitle, SeleniumActions.getBrowserTitle(), "Title is NOT matched ");
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitForElement(SeleniumAutomation.PageObjects.WellConfigurationPage.pedashboard);
            SeleniumActions.WaitForLoad();
        }

        [TestMethod]
        public void ForeSiteTest()
        {
           // report = ExtentFactory.getInstance();
            test = report.CreateTest("RRL");
            test.Pass("RRL Test Started");
            CommonHelper.TraceLine("ForeSiteTest");
            config.CreateWellWorkFlow(test);
            test.Pass("RRL Test Finished");
        }

        [TestMethod]
        public void AddWellPermission()
        {
            // report = ExtentFactory.getInstance();
            test = report.CreateTest("Add Well User Permission");
            test.Info("Verifying Application Behaviour when Add Well Permission is removed");

            //Add new Role & Remove Add Well Permission from that Role
            CommonHelper.CreateRole("TestRole");
            CommonHelper.RemovePermissionFromRole("TestRole", PermissionId.AddWell);
           
            CommonHelper.updateUserWithGivenRole("TestRole");
            perm.VerifyRemoveAddWellPermission(test);
            CommonHelper.AddPermissioninRole("TestRole", PermissionId.AddWell);
            perm.VerifyRestoreAddWellPermission(test);
            CommonHelper.updateUserWithGivenRole("TestRole");
           // CommonHelper.updateUserWithGivenRole("Administrator");
           // CommonHelper.RemoveRole("TestRole");

        }


        [TestMethod]
        public void RemoveWellPermission()
        {

            test = report.CreateTest("Remove Well User Permission");
            test.Info("Verifying Application Behaviour when Remove Well Permission is removed");

            //Add new Role & Remove Add Well Permission from that Role
            CommonHelper.CreateRole("TestRole");
            CommonHelper.RemovePermissionFromRole("TestRole", PermissionId.RemoveWell);
            CommonHelper.updateUserWithGivenRole("TestRole");
            perm.VerifyRevokeRemoveWellPermission(test);
            CommonHelper.AddPermissioninRole("TestRole", PermissionId.RemoveWell);
            perm.VerifyRestoreRemoveWellPermission(test);
            CommonHelper.updateUserWithGivenRole("TestRole");


        }
        [TestMethod]
        public void AddDocumentPermission()
        {
            test = report.CreateTest("AddDocument Permission");
            CommonHelper.CreateRole("TestRole");
            CommonHelper.RemovePermissionFromRole("TestRole", PermissionId.AddDocument);
            CommonHelper.updateUserWithGivenRole("TestRole");
            perm.VerifyRevokeAddDocumentPermission(test);
            CommonHelper.AddPermissioninRole("TestRole", PermissionId.AddDocument);
            perm.VerifyRestoreAddDocumentPermission(test);
            CommonHelper.updateUserWithGivenRole("TestRole");

        }

        [TestMethod]
        public void ForeSiteTest1()
        {
           // report = ExtentFactory.getInstance();
            test = report.CreateTest("RRL1");
            test.Pass("RRL1 Test Started");
            CommonHelper.TraceLine("ForeSiteTest1");
            config.CreateWellWorkFlow(test);
            test.Pass("RRL1 Test Finished");
            //config.CreateWellWorkFlow(test);
        }

        [TestMethod]
        public void AddComponentPermission()
        {
            test = report.CreateTest("AddComponent Permission");
            try
            {
                CommonHelper.CreateRole("TestRole");
                CommonHelper.RemovePermissionFromRole("TestRole", PermissionId.AddComponent);
                CommonHelper.updateUserWithGivenRole("TestRole");
                perm.VerifyAddComponentPermission(test);
                CommonHelper.AddPermissioninRole("TestRole", PermissionId.AddComponent);
                perm.VerifyRestoreAddComponentPermission(test);
                CommonHelper.updateUserWithGivenRole("TestRole");
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("AddComponent Permission Exception");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine(ExtentFactory.screenshotdir, "AddComponent Permission Exception.png"))).Build());
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
