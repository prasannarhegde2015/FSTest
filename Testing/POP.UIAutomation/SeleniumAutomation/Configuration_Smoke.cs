
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
    public class Configuration_Smoke
    {

        static ExtentReports report;
        ExtentTest test;
        WellConfiguration config = new WellConfiguration();
        WellTest welltest = new WellTest();
        WellStatus wellstatus = new WellStatus();

        public Configuration_Smoke()
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
            //Assert.AreEqual(SeleniumAutomation.TestData.WellConfigData.HomePageTitle, SeleniumActions.getBrowserTitle(), "Title is NOT matched ");
            if (isruunisats == "false")
            {
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitForElement(SeleniumAutomation.PageObjects.WellConfigurationPage.pedashboard);
                SeleniumActions.WaitForLoad();
            }
        }

        [TestCategory("Configuration_SmokeSuite2"), TestMethod]
        public void RRLWellCreationFlow()
        {
            try
            {
                test = report.CreateTest("RRL Well Creation New");
                config.CreateRRLWellFullonBlankDB(test);
                config.VerifyRRLWellFullonBlankDB(test);
                config.EnterDataForAdditionalWellAttributes();
                config.VerifyDataForAdditionalWellAttributes(test);
                config.DeleteWellByUI(TestData.WellConfigData.RRFACName, test);
                test.Pass("RRL Well Creation Configuration Smoke Finished");
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("Error_RRLWell_creation");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Error_RRLWell_creation.png"))).Build());
                Assert.Fail(e.ToString());
            }
        }

        [TestCategory("Status_SmokeSuite"), TestMethod]
        public void CommandLockOutTest()
        {
            try
            {
                test = report.CreateTest("CommandLockOutTest");
                CommonHelper.SetLockoutPeriod("Single", 15);
                CommonHelper.SetLockoutPeriod("Multi", 30);
                string setAllToHighRiskCommand = "DemandScan|LO_STATUS|True;StartRPC|C_STARTWEL|True;StopAndLeaveDown|C_FORCEOFF|True;IdleRPC|C_IDLE|True;ControlTransfer|C_CTRLXFER|True;SoftwareTimer|C_SWTIMER|True;ClearErrors|C_CLEARERR|True;ContinuousRun|C_RUNCONT|True";
                CommonHelper.CheckLockoutForCommandName(setAllToHighRiskCommand);
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.configurationTab);
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.wellconfigurationTab);
                SeleniumActions.WaitForLoad();
                config.CreateRRLWellPartialonBlankDB(test, TestData.WellConfigData.RRFACName, TestData.WellConfigData.RRFACName);
                wellstatus.VerifyCommnadLockOut(test);
                //Clear the Toolbox settings after this 
                setAllToHighRiskCommand = "DemandScan|LO_STATUS|False;StartRPC|C_STARTWEL|False;StopAndLeaveDown|C_FORCEOFF|False;IdleRPC|C_IDLE|False;ControlTransfer|C_CTRLXFER|False;SoftwareTimer|C_SWTIMER|False;ClearErrors|C_CLEARERR|False;ContinuousRun|C_RUNCONT|False";
                CommonHelper.CheckLockoutForCommandName(setAllToHighRiskCommand);
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("Error_LockoutTest");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Error_LockoutTest.png"))).Build());
                Assert.Fail(e.ToString());
            }
            finally
            {
                string clrsetAllToHighRiskCommand = "DemandScan|LO_STATUS|False;StartRPC|C_STARTWEL|False;StopAndLeaveDown|C_FORCEOFF|False;IdleRPC|C_IDLE|False;ControlTransfer|C_CTRLXFER|False;SoftwareTimer|C_SWTIMER|False;ClearErrors|C_CLEARERR|False;ContinuousRun|C_RUNCONT|False";
                CommonHelper.CheckLockoutForCommandName(clrsetAllToHighRiskCommand);
            }
        }

        [TestCategory("RRL_WellCreationFull"), TestMethod]
        public void RRL_WellCreationFull()
        {
            try
            {
                test = report.CreateTest("RRL Well Creation New");
                config.CreateRRLWellFullonBlankDB(test);
            }
            finally
            {
                SeleniumActions.disposeDriver();

            }

        }

        [TestCategory("Configuration_SmokeSuite1"), TestMethod]
        public void ForeSiteESPWellWorkflow()
        {
            try
            {
                test = report.CreateTest("ESP Well creation");
                config.Create_WellWorkFlow(test, "ESP", true);
                test.Pass("ESP Well creation Finished");
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("Error_ESPWell_creation");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Error_ESPWell_creation.png"))).Build());
                Helper.CommonHelper.Deleteasset();
                Assert.Fail(e.ToString());
            }

        }

        [TestCategory("Configuration_SmokeSuite1"), TestMethod]
        public void ForeSiteNFWellWorkflow()
        {
            try
            {
                test = report.CreateTest("NF Well creation");
                config.Create_WellWorkFlow(test, "NF", true);
                test.Pass("NF Well creation Finished");
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("Error_NFWell_creation");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Error_NFWell_creation.png"))).Build());
                Assert.Fail(e.ToString());
            }
        }

        [TestCategory("Configuration_SmokeSuite3"), TestMethod]
        public void ForeSiteOTWellWorkflow()
        {
            try
            {
                test = report.CreateTest("OT Well Creation");
                config.Create_WellWorkFlow(test, "OT", false);
                test.Pass("OT Well Creation Finished");
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("Error_OTWell_creation");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Error_OTwELL_CREATION.PNG"))).Build());
                Assert.Fail(e.ToString());
            }
        }
        [TestCategory("Configuration_SmokeSuite4"), TestMethod]
        public void ForeSiteGLWellWorkflow()
        {
            try
            {
                test = report.CreateTest("GL Well creation");
                config.Create_WellWorkFlow(test, "GL", true);
                test.Pass("GL Well creation Finished");
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("Error_GLWell_creation");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Error_GLWell_creation.png"))).Build());
                Assert.Fail(e.ToString());
            }
        }
        [TestCategory("Configuration_SmokeSuite3"), TestMethod]
        public void ForeSiteGInjWellWorkflow()
        {
            try
            {
                test = report.CreateTest("Gas Injection Well creation");
                config.Create_WellWorkFlow(test, "GInj", true);
                test.Pass("Gas Injection Well creation Finished");
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("Gas Injection Well creation");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Error_GInjWell_creation.png"))).Build());
                Assert.Fail(e.ToString());
            }
        }
        [TestCategory("Configuration_SmokeSuite2"), TestMethod]
        public void ForeSiteWInjWellWorkflow()
        {
            try
            {
                test = report.CreateTest("WAG Injection Well creation");
                config.Create_WellWorkFlow(test, "WInj", true);
                test.Pass("WAG Injection Well creation Finished");
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("WAG Injection Well creation");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Error_WInjWell_creation.png"))).Build());
                Assert.Fail(e.ToString());
            }
        }

        [TestCategory("Configuration_SmokeSuite3"), TestMethod]
        public void ForeSitePGLWellWorkflow()
        {
            try
            {
                test = report.CreateTest("PGL Well creation");
                config.Create_WellWorkFlow(test, "PGL", true);
                test.Pass("PGL Well creation Finished");
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("PGL Well creation");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Error_PGLWell_creation.png"))).Build());
                Assert.Fail(e.ToString());
            }
        }
        [TestCategory("Configuration_SmokeSuite4"), TestMethod]
        public void ForeSiteWaterInjWellWorkflow()
        {
            try
            {
                test = report.CreateTest("Water Injection Well creation");
                config.Create_WellWorkFlow(test, "WaterInj", true);
                test.Pass("Water Injection Well creation Finished");
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("Water Injection Well creation");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Error_WaterInjWell_creation.png"))).Build());
                Assert.Fail(e.ToString());
            }
        }

        [TestCategory("ConfigurationSettings_SmokeSuite"), TestMethod]
        public void VerifyTargetconfigurationsettings()
        {
            try
            {
                test = report.CreateTest("Target Configuration Settings");
                config.Targetconfigsettings(test);
                test.Pass("Target Configuration Settings verification Finished");
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("Error_Target Configuration Settings");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Error_targetconfigurationsettings.png"))).Build());
                Assert.Fail(e.ToString());
            }

        }
        [TestCategory("ConfigurationSettings_SmokeSuite"), TestMethod]
        public void VerifyAcceptancelimitssettings()
        {
            try
            {
                test = report.CreateTest("Acceptance Limits Settings");
                config.Acceptancelimitsettings(test);
                test.Pass("Acceptance Settings verification Finished");
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("Error_Accept Configuration Settings");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Error_Acceptconfigurationsettings.png"))).Build());
                Assert.Fail(e.ToString());
            }

        }

        [TestCategory("ConfigurationSettings_SmokeSuite"), TestMethod]
        public void Verifyoperatinglimitssettings()
        {
            try
            {
                test = report.CreateTest("operating Limits Settings");
                config.OperatingLimitssettings(test);
                test.Pass("Operating Settings verification Finished");
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("Error_Operating Settings");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Error_operatingsettings.png"))).Build());
                Assert.Fail(e.ToString());
            }

        }
        [TestCategory("ConfigurationSettings_SmokeSuite"), TestMethod]
        public void Addperformancecurves()
        {
            try
            {
                test = report.CreateTest("Performance well Settings");
                config.Addperformancecurves(test);
                test.Pass("Performancecurves verification Finished");
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("Error_Performance curves Settings");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Error_performancesettings.png"))).Build());
                Assert.Fail(e.ToString());
            }

        }


        [TestCategory("RRLIntelligentAlarms_SmokeSuite"), TestMethod]
        public void RRLIntelligentAlarmCheck()
        {
            try
            {
                test = report.CreateTest("RRL Well Creation New");
                //config.CreateRRLWellFullonBlankDB(test);
                //SeleniumActions.waitClick(PageObjects.DashboardPage.tabOptimization);
                //SeleniumActions.WaitForLoad();
                //SeleniumActions.waitClick(PageObjects.DashboardPage.tabWellTest);
                //SeleniumActions.WaitForLoad();
                //welltest.CreateRRLWellTest(test);
                CommonHelper.ScanPumpUpCardOnlyForNewDDStranscation();
                CommonHelper.RunAnalysisTaskSchedular("-runAnalysis");
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.SurveillaceTab);
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.WellStatusTab);
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(PageObjects.WellStatusPage.btnScan);
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(PageObjects.WellStatusPage.btnConfirmSend);
                SeleniumActions.Toastercheck("Scan Commamnd", "Command Scan issued successfully.", test);
                //Add Well test
                // Change Limits in Config

                // Run Analysis from Command line schuedualr 
                //goto to Well statusand verify
                //Clear alarms
                // config.DeleteWellByUI(TestData.WellConfigData.RRFACName, test);
                test.Pass("RRL Well Creation Configuration Smoke Finished");
            }
            finally
            {

                //Ensure Driver alwas is quit;
                SeleniumActions.disposeDriver();
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
            try
            {
                report.Flush();
                //CommonHelper.DeleteWellsByAPI();
                //Helper.CommonHelper.Deleteasset();
            }
            catch (Exception)
            {

            }
        }
    }
}
