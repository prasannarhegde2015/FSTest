using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeleniumAutomation.SeleniumObject;
using System.Configuration;
using SeleniumAutomation.Helper;
using System.Diagnostics;
using AventStack.ExtentReports;
using System.IO;
using System;
using System.DirectoryServices.AccountManagement;
using System.Threading;

namespace SeleniumAutomation.TestClasses
{
    class WellTest
    {
        public void CreateRRLWellTest(ExtentTest test)
        {
            SeleniumActions.waitClickJS(PageObjects.WellDesignPage.tabwelltest);
            SeleniumActions.WaitForLoad();
            //FRI - 3983 fixed no need for workaround for now commenting the code 
            //SeleniumActions.WaitForLoad();
            //SeleniumActions.refreshBrowser();
            //SeleniumActions.waitforPageloadComplete();
            //SeleniumActions.WaitforWellcountNonZero();
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.WellTestPage.btnCreateNewWellTest);
            SeleniumActions.WaitForLoad();

            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.applicabledate);
            SeleniumActions.sendText(PageObjects.WellConfigurationPage.applicabledate, DateTime.Now.AddYears(-2).AddDays(18).ToString("MMddyyyy"), true);
            Thread.Sleep(2000);
            SeleniumActions.sendText(PageObjects.WellTestPage.txtWellTestDuration, "15");
            SeleniumActions.selectKendoDropdownValue(PageObjects.WellTestPage.kendoddWellTestType, "Well Test");
            SeleniumActions.selectKendoDropdownValue(PageObjects.WellTestPage.kendoddWellTestQCode, "Allocatable Test");

            SeleniumActions.sendText(PageObjects.WellTestPage.txtWellTestOilRate, "300");
            SeleniumActions.sendText(PageObjects.WellTestPage.txtWellTestWaterRate, "300");
            SeleniumActions.sendText(PageObjects.WellTestPage.txtWellTestGasRate, "300");
            SeleniumActions.sendText(PageObjects.WellTestPage.txtWellTestOilGravity, "35");
            SeleniumActions.sendText(PageObjects.WellTestPage.txtWellTestWaterGravity, "1.05");
            SeleniumActions.sendText(PageObjects.WellTestPage.txtWellTestGasGravity, "0.65");
            SeleniumActions.sendText(PageObjects.WellTestPage.txtWellTestTHP, "200");
            SeleniumActions.sendText(PageObjects.WellTestPage.txtWellTestTHT, "80");
            SeleniumActions.sendText(PageObjects.WellTestPage.txtWellTestCHP, "300");
            SeleniumActions.sendText(PageObjects.WellTestPage.txtWellTestFAP, "5100");
            SeleniumActions.sendText(PageObjects.WellTestPage.txtWellTestPumpHours, "10");
            SeleniumActions.waitClick(PageObjects.WellTestPage.btnWellTestSave);
            SeleniumActions.Toastercheck("Well Test Add", "Saved successfully.", test);


        }



    }
}
