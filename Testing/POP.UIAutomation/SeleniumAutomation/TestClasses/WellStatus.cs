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
using OpenQA.Selenium;

namespace SeleniumAutomation.TestClasses
{
    class WellStatus
    {
        public void VerifyCommnadLockOut(ExtentTest test)
        {
            // SeleniumActions.waitClick(PageObjects.WellStatusPage.btnScan);
            SeleniumActions.waitClick(PageObjects.DashboardPage.tabSurveillace);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.DashboardPage.tabWellStatus);
            SeleniumActions.WaitForLoad();
            SeleniumActions.selectWellfromWellList(ConfigurationManager.AppSettings.Get("CygNetFacility"));
            SeleniumActions.WaitForLoad();
            double toolboxSingleLockout = 15;
            //********** Test Lockout for Start Command
            //Look on Well Status Page 
            VerifyCommandType(PageObjects.WellStatusPage.btnScan, "Scan", toolboxSingleLockout, test);
            VerifyCommandType(PageObjects.WellStatusPage.btnStart, "Start", toolboxSingleLockout, test);
            VerifyCommandType(PageObjects.WellStatusPage.btnIdle, "Idle", toolboxSingleLockout, test);
            VerifyCommandType(PageObjects.WellStatusPage.btnStop, "Stop and Leave Down", toolboxSingleLockout, test);
            VerifyCommandType(PageObjects.WellStatusPage.btnClearErrors, "Clear Errors", toolboxSingleLockout, test);
            VerifyCommandType(PageObjects.WellStatusPage.btnSoftwaretimer, "Software Timer", toolboxSingleLockout, test);
            VerifyCommandType(PageObjects.WellStatusPage.btnControlTrasnfer, "Control Transfer", toolboxSingleLockout, test);

        }


        public void VerifyCommandLockoutToasterUI(By ctlCommand, double lockoutval, ExtentTest test ,bool highrisk=true)
        {
            for (int i = 0; i < 2; i++)
            {
                SeleniumActions.waitClick(ctlCommand);
                SeleniumActions.WaitForLoad();
                if (highrisk)
                {
                    SeleniumActions.waitClick(PageObjects.WellStatusPage.btnConfirmSend);
                    SeleniumActions.WaitForLoad();
                }
                string cmdLockToasttext = SeleniumActions.getInnerText(PageObjects.WellConfigurationPage.Toaseter);
                CommonHelper.TraceLine("Comamnd Lockout Toaster Message " + cmdLockToasttext);
                string prefixLockoutToExecuteCommand = "User cannot issue commands for";
                string postFixLockoutToExecuteCommand = " more seconds.";
                string toasterRemainingSecs = cmdLockToasttext.Replace(prefixLockoutToExecuteCommand, "");
                toasterRemainingSecs = toasterRemainingSecs.Replace(postFixLockoutToExecuteCommand, "");
                double UIremaingSeconds = double.Parse(toasterRemainingSecs.Trim());
                Assert.IsTrue((UIremaingSeconds < lockoutval), "Toaster Mesage Seconds was outside bounds of sepecified Single Timeout");
                string dynaguid = Guid.NewGuid().ToString().Substring(30);
                SeleniumActions.takeScreenshot("CommandLockout" + dynaguid);
                test.Pass("LockoutMessage", MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "CommandLockout" + dynaguid + ".png"))).Build());
                Thread.Sleep(3000);
            }
        }
        public void VerifyCommandType(By ctlCommand, string commandname, double locktime, ExtentTest test)
        {
            SeleniumActions.waitClick(ctlCommand);
            SeleniumActions.WaitForLoad();
            // *********** FRI-4624 : Improve the usability by reducing clicks on a no-risk functionality of 'scanning' a well
            if (commandname.ToLower() != "scan")
            {
                SeleniumActions.waitClick(PageObjects.WellStatusPage.btnConfirmSend);
                SeleniumActions.WaitForLoad();
            }
            SeleniumActions.Toastercheck("Well " + commandname + " Command Toaster", "Command " + commandname + " issued successfully.", test);
            if (commandname.ToLower() != "scan")
            {
                VerifyCommandLockoutToasterUI(ctlCommand, locktime, test);
            }
            else
            {
                VerifyCommandLockoutToasterUI(ctlCommand, locktime, test,false);
            }
            Thread.Sleep(5000);
        }



    }
}
