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
using System.Collections.Generic;
using System.Data;
using OpenQA.Selenium;
using SeleniumAutomation.PageObjects;

namespace SeleniumAutomation.TestClasses
{
    class WellDesign : PageObjects.WellDesignPage
    {
        public static string FilesLocation = ConfigurationManager.AppSettings.Get("FilesLocation");
        WellConfiguration config = new WellConfiguration();
        WellConfigurationPage configobj = new WellConfigurationPage();
        public void Verifywelldesign(ExtentTest test)
        {
            // *********** Create New FULL ESP Well  *******************

            try
            {

                string isruunisats = ConfigurationManager.AppSettings.Get("isRunningATS");
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.configurationTab);
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.wellconfigurationTab);
                SeleniumActions.WaitForLoad();

                #region GenralTab

                config.EnterwellGeneralData(TestData.WellConfigData.WellTypePGL, "PGLWELL");
                config.uploadmodelfile("PL-631.wflx", "PGL");
                string facilityname = PageObjects.WellConfigurationPage.wellnametextbox.GetAttribute("value");
                #endregion

                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.savebutton);
                SeleniumActions.waitForElement(PageObjects.WellConfigurationPage.Toaseter);
                config.Toastercheck("Well Creation", "Well " + facilityname + " saved successfully.", test);
                test.Info("Well is created");
                SeleniumActions.waitClick(welldesignTab);
                SeleniumActions.WaitForLoad();
                Assert.AreEqual(SeleniumActions.getAttribute(togglesensitivities, "class").Contains("disabled").ToString(), "True");
                SeleniumActions.waitClickJS(btnsensitivities);
                SeleniumActions.waitClickJS(btnaddsensitivities);
                SeleniumActions.kendotextboxentermultiplevalues("xpath", sensdialogtubingheadpr, "90;80;70;100", "");
                SeleniumActions.waitClick(btnsensitivitiessave);
                Assert.AreEqual(SeleniumActions.getAttribute(togglesensitivities, "class").Contains("disabled").ToString(), "False");
                SeleniumActions.waitClick(welldesignTab);
                Verifydefaultfields(test, "US");
                SeleniumActions.waitClick(welldesignTab);
                Verifywelldesigncalculate(test);
                Welldesincreatewelltest(test);
                Welldesinoperatingmodesensitivities(test);
                Verifydefaultfields(test, "Metric");

            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("OperatinglimitsSettings");
                CommonHelper.TraceLine("Error in OperatinglimitsSettings " + e.Message);
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "OperatinglimitsSettings" + ".png"))).Build());
                Assert.Fail(e.ToString());

            }
            finally
            {
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.wellconfigurationTab);
                config.Deletewell();

                // *********** Dispose  ******************
                SeleniumActions.disposeDriver();
            }
        }


        public void Verifywelldesigncalculate(ExtentTest test)
        {


            try
            {

                if (SeleniumActions.getAttribute(togglesensitivities, "class").Contains("disabled").ToString().Equals("False"))
                {
                    SeleniumActions.WaitForLoad();
                    Thread.Sleep(2000);


                    string expcolnames = "Case;Tubing Head Pressure (psia);Differential Pressure (psi);Buildup Time (min);After Flow Time (min);Nec. THP (psia);Nec. CHP (psia);Fall Time (min);Rise Time (min);Cycle Time (min);Nec. Gas Volume (Mscf);Excess Gas (Mscf);Slug Volume (STB);Slug Height (ft);Number of Cycles Per Day;Daily Gas Rate (Mscf/d);Daily Liquid Rate (STB/d)";

                    string expcolvals_row = "1;50;50;11;0;159.23;209.23;49;13;73;5.0466;1.8;0.68;118.29;20;136.93;13.7";
                    Helper.CommonHelper.TraceLine("Verifying Well design grid before calculation");
                    MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Welldesign_beforecalculation" + ".png")));
                    SeleniumActions.Verifygridmultvalues(expcolnames, "xpath", weldsgncolheader, "");
                    SeleniumActions.Verifygridmultvalues(expcolvals_row, "xpath", weldsgncolvalues, "");
                    SeleniumActions.waitClickDelay(togglesensitivities, 3);

                    SeleniumActions.waitClickJS(btncalculate);
                    Helper.CommonHelper.TraceLine("Verifying Well design grid before calculation");
                    MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Welldesign_aftercalculation" + ".png")));
                    string expcolnameaftercalc = "Case;Tubing Head Pressure (psia);Differential Pressure (psi);Buildup Time (min);After Flow Time (min);Nec. THP (psia);Nec. CHP (psia);Fall Time (min);Rise Time (min);Cycle Time (min);Nec. Gas Volume (Mscf);Excess Gas (Mscf);Slug Volume (STB);Slug Height (ft);Number of Cycles Per Day;Daily Gas Rate (Mscf/d);Daily Liquid Rate (STB/d)";

                    string expcolvals_rowaftercalc = "1;90;80;70;100;266.42;346.42;50;13;233;8.3565;2.5955;1.1;189.21;6;65.71;6.6";
                    SeleniumActions.Verifygridmultvalues(expcolnameaftercalc, "xpath", weldsgncolheader, "");
                    SeleniumActions.Verifygridmultvalues(expcolvals_rowaftercalc, "xpath", weldsgncolvalues, "");
                    Helper.CommonHelper.ChangeUnitSystemUserSetting("Metric");
                    SeleniumActions.refreshBrowser();
                    string expcolnamesmetric = "Case;Tubing Head Pressure (kPa);Differential Pressure (kPa);Buildup Time (min);After Flow Time (min);Nec. THP (kPa);Nec. CHP (kPa);Fall Time (min);Rise Time (min);Cycle Time (min);Nec. Gas Volume (sm³);Excess Gas (sm³);Slug Volume (sm³);Slug Height (m);Number of Cycles Per Day;Daily Gas Rate (sm³/d);Daily Liquid Rate (sm³/d)";

                    string expcolvals_rowmetric = "1;344.74;344.74;11;0;1097.85;1442.59;49;13;73;142.9048;50.9701;0.11;36.05;20;3877.5;2.2";
                    Helper.CommonHelper.TraceLine("Verifying Well design grid before calculation");
                    MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Welldesign_beforecalculation" + ".png")));
                    SeleniumActions.Verifygridmultvalues(expcolnamesmetric, "xpath", weldsgncolheader, "");
                    SeleniumActions.Verifygridmultvalues(expcolvals_rowmetric, "xpath", weldsgncolvalues, "");
                    SeleniumActions.waitClickDelay(togglesensitivities, 3);

                    SeleniumActions.waitClickJS(btncalculate);
                    Helper.CommonHelper.TraceLine("Verifying Well design grid before calculation");
                    MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Welldesign_aftercalculation" + ".png")));
                    string expcolnamesmetricaftercalc = "Case;Tubing Head Pressure (kPa);Differential Pressure (kPa);Buildup Time (min);After Flow Time (min);Nec. THP (kPa);Nec. CHP (kPa);Fall Time (min);Rise Time (min);Cycle Time (min);Nec. Gas Volume (sm³);Excess Gas (sm³);Slug Volume (sm³);Slug Height (m);Number of Cycles Per Day;Daily Gas Rate (sm³/d);Daily Liquid Rate (sm³/d)";

                    string expcolvals_rowmetricaftercalc = "1;620.53;551.58;70;100;1836.88;2388.46;50;13;233;236.63;73.495;0.17;57.67;6;1860.75;1";
                    SeleniumActions.Verifygridmultvalues(expcolnamesmetricaftercalc, "xpath", weldsgncolheader, "");
                    SeleniumActions.Verifygridmultvalues(expcolvals_rowmetricaftercalc, "xpath", weldsgncolvalues, "");


                }
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("Welldesigngrid");
                CommonHelper.TraceLine("Error in Welldesigngrid " + e.Message);
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Welldesigngrid" + ".png"))).Build());
                Assert.Fail(e.ToString());

            }

        }


        public void Welldesincreatewelltest(ExtentTest test)
        {
            // *********** Create New FULL ESP Well  *******************

            try
            {
                Helper.CommonHelper.ChangeUnitSystemUserSetting("US");
                SeleniumActions.refreshBrowser();
                SeleniumActions.WaitForLoad();
                if (SeleniumActions.isElemPresent(gridwelldesign))
                {
                    SeleniumActions.waitClick(gridwelldesign);
                    SeleniumActions.waitClick(btncreatewelltest);
                    SeleniumActions.waitClick(btncalendarwelltest);
                    SeleniumActions.waitClick(txttestdate);

                    SeleniumActions.selectKendoDropdownValue(txttime, "5:30 AM");
                    SeleniumActions.sendText(txttestduration, "5");
                    SeleniumActions.waitClick(txtflowlinepressure);
                    SeleniumActions.sendText(txtflowlinepressure, "100");
                    SeleniumActions.waitClick(btnsave);
                    SeleniumActions.WaitForLoad();
                    SeleniumActions.waitClick(taboptimization);
                    SeleniumActions.waitClick(tabwelltest);
                    Assert.AreEqual(SeleniumActions.isElemPresent(txtwelltestdate).ToString(), "True", "Well test is not created");

                }
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("Welldesigncreatewelltest");
                CommonHelper.TraceLine("Error in OperatinglimitsSettings " + e.Message);
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Welldesigncreatewelltest" + ".png"))).Build());
                Assert.Fail(e.ToString());

            }
            finally
            {
                SeleniumActions.takeScreenshot("Welldesigncreatewelltest");

            }
        }


        public void Welldesinoperatingmodesensitivities(ExtentTest test)
        {
            // *********** Create New FULL ESP Well  *******************

            try
            {

                Helper.CommonHelper.ChangeUnitSystemUserSetting("US");
                SeleniumActions.refreshBrowser();
                // SeleniumActions.waitClick(taboptimization);
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.configurationTab);
                // SeleniumActions.waitClick(PageObjects.WellConfigurationPage.configurationTab);
                SeleniumActions.waitClick(welldesignTab);
                SeleniumActions.waitClick(taboperatingmode);
                Assert.AreEqual(SeleniumActions.isElemPresent(lblcriticalrateus).ToString(), "True", "Critical Rate US label is not displayed");
                Assert.AreEqual(SeleniumActions.isElemPresent(lblcriticalrate).ToString(), "True", "Critical Rate label is not displayed");
                Assert.AreEqual(SeleniumActions.isElemPresent(lblcriticalrateAT10).ToString(), "True", "Critical Rate @10 label is not displayed");
                Assert.AreEqual(SeleniumActions.isElemPresent(lbldesignrate).ToString(), "True", "Design Rate label is not displayed");
                Assert.AreEqual(SeleniumActions.isElemPresent(lblthp).ToString(), "True", "THP label is not displayed");
                SeleniumActions.waitClick(tabsensitivities);
                Assert.AreEqual(SeleniumActions.isElemPresent(lbltimemin).ToString(), "True", "Time(min) label is not displayed");
                Assert.AreEqual(SeleniumActions.isElemPresent(lblcycletime).ToString(), "True", "Cycle Time label is not displayed");
                Assert.AreEqual(SeleniumActions.isElemPresent(lblgasvolume).ToString(), "True", "Gas Volume label is not displayed");
                Assert.AreEqual(SeleniumActions.isElemPresent(lblnecchp).ToString(), "True", "Nec CHP label is not displayed");
                Assert.AreEqual(SeleniumActions.isElemPresent(lblnecThp).ToString(), "True", "Nec THP label is not displayed");
                Assert.AreEqual(SeleniumActions.isElemPresent(lblslugvolume).ToString(), "True", "Slug Volume label is not displayed");
                Assert.AreEqual(SeleniumActions.isElemPresent(lblcyclesperday).ToString(), "True", "Cycles/Day label is not displayed");
                Assert.AreEqual(SeleniumActions.isElemPresent(lbldailygasrate).ToString(), "True", "Daily GaS Rate label is not displayed");
                Assert.AreEqual(SeleniumActions.isElemPresent(lbldailyliquidrate).ToString(), "True", "Daily Liquid Rate label is not displayed");
                Helper.CommonHelper.ChangeUnitSystemUserSetting("Metric");
                SeleniumActions.refreshBrowser();

                SeleniumActions.WaitForLoad();
                while (SeleniumActions.IsElementDisplayedOnUI(welldesignTab).ToString().Equals("False"))
                {
                    SeleniumActions.waitClick(PageObjects.WellConfigurationPage.configurationTab);
                }


                SeleniumActions.waitClick(welldesignTab);
                SeleniumActions.waitClick(taboperatingmode);
                Assert.AreEqual(SeleniumActions.isElemPresent(lblcriticalratemetric).ToString(), "True", "Critical Rate US label is not displayed");
                Assert.AreEqual(SeleniumActions.isElemPresent(lblcriticalrate).ToString(), "True", "Critical Rate label is not displayed");
                Assert.AreEqual(SeleniumActions.isElemPresent(lblcriticalrateAT10).ToString(), "True", "Critical Rate @10 label is not displayed");
                Assert.AreEqual(SeleniumActions.isElemPresent(lbldesignrate).ToString(), "True", "Design Rate label is not displayed");
                Assert.AreEqual(SeleniumActions.isElemPresent(lblthpmetric).ToString(), "True", "THP label is not displayed");
                SeleniumActions.waitClick(tabsensitivities);
                Assert.AreEqual(SeleniumActions.isElemPresent(lbltimemin).ToString(), "True", "Time(min) label is not displayed");
                Assert.AreEqual(SeleniumActions.isElemPresent(lblcycletime).ToString(), "True", "Cycle Time label is not displayed");
                Assert.AreEqual(SeleniumActions.isElemPresent(lblgasvolumemetric).ToString(), "True", "Gas Volume label is not displayed");
                Assert.AreEqual(SeleniumActions.isElemPresent(lblnecchp).ToString(), "True", "Nec CHP label is not displayed");
                Assert.AreEqual(SeleniumActions.isElemPresent(lblnecThp).ToString(), "True", "Nec THP label is not displayed");
                Assert.AreEqual(SeleniumActions.isElemPresent(lblslugheightmetric).ToString(), "True", "Slug Volume label is not displayed");
                Assert.AreEqual(SeleniumActions.isElemPresent(lblcyclesperday).ToString(), "True", "Cycles/Day label is not displayed");
                Assert.AreEqual(SeleniumActions.isElemPresent(lbldailygasrate).ToString(), "True", "Daily GaS Rate label is not displayed");
                Assert.AreEqual(SeleniumActions.isElemPresent(lbldailyliquidrate).ToString(), "True", "Daily Liquid Rate label is not displayed");
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("Welldesign_operatingmodesensitivities");
                CommonHelper.TraceLine("Error in operatingmodesensitivities " + e.Message);
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "operatingmodesensitivities" + ".png"))).Build());
                Assert.Fail(e.ToString());

            }
            finally
            {
                SeleniumActions.takeScreenshot("Welldesign_operatingmodesensitivities");


            }
        }

        public void Verifydefaultfields(ExtentTest test, string unit)
        {


            try
            {
                Helper.CommonHelper.TraceLine("Verification of well design default fields in UOM " + unit);
                if (unit.Equals("US"))
                {
                    config.Verifydata(TestData.PGLModelData.dt, "Gas Rate", txtgasrate, "US");
                    config.Verifydata(TestData.PGLModelData.dt, "Liquid Rate", txtliquidrate, "US");
                    config.Verifydata(TestData.PGLModelData.dt, "Tubing Head Pressure", txtthp, "US");
                    config.Verifydata(TestData.PGLModelData.dt, "Differential Pressure", txtdiffpressure, "US");
                    config.Verifydata(TestData.PGLModelData.dt, "Build Up Time", txtbuilduptime, "US");
                    config.Verifydata(TestData.PGLModelData.dt, "After Flow Time", txtafterflowtime, "US");
                    SeleniumActions.scrollscrollbar(0, 200, scrollwelldesign);
                    config.Verifydata(TestData.PGLModelData.dt, "Fall Rate in Gas", txtfallrateingas, "US");
                    config.Verifydata(TestData.PGLModelData.dt, "Fall Rate in Liquid", txtfallrateinliquid, "US");
                    config.Verifydata(TestData.PGLModelData.dt, "Ideal Rise Rate", txtfidealriserate, "US");
                    config.Verifydata(TestData.PGLModelData.dt, "Req. Rise Pressure", txtreqrisepressure, "US");
                    SeleniumActions.scrollscrollbar(0, -200, scrollwelldesign);
                }
                if (unit.Equals("Metric"))
                {
                    config.Verifydata(TestData.PGLModelData.dt, "Gas Rate", txtgasrate, "Metric");
                    config.Verifydata(TestData.PGLModelData.dt, "Liquid Rate", txtliquidrate, "Metric");
                    config.Verifydata(TestData.PGLModelData.dt, "Tubing Head Pressure", txtthp, "Metric");
                    config.Verifydata(TestData.PGLModelData.dt, "Differential Pressure", txtdiffpressure, "Metric");
                    config.Verifydata(TestData.PGLModelData.dt, "Build Up Time", txtbuilduptime, "Metric");
                    config.Verifydata(TestData.PGLModelData.dt, "After Flow Time", txtafterflowtime, "Metric");
                    SeleniumActions.scrollscrollbar(0, 200, scrollwelldesign);
                    config.Verifydata(TestData.PGLModelData.dt, "Fall Rate in Gas", txtfallrateingas, "Metric");
                    config.Verifydata(TestData.PGLModelData.dt, "Fall Rate in Liquid", txtfallrateinliquid, "Metric");
                    config.Verifydata(TestData.PGLModelData.dt, "Ideal Rise Rate", txtfidealriserate, "Metric");
                    config.Verifydata(TestData.PGLModelData.dt, "Req. Rise Pressure", txtreqrisepressure, "Metric");
                    SeleniumActions.scrollscrollbar(0, -200, scrollwelldesign);
                }
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("Well design screen default fields");
                CommonHelper.TraceLine("Error in Welldesign_defaultfields " + e.Message);
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Welldesignscreen" + ".png"))).Build());
                Assert.Fail(e.ToString());

            }
            finally
            {
                SeleniumActions.takeScreenshot("Welldesign_screen");


            }
        }


    }
}
