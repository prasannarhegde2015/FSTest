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
using Weatherford.POP.Enums;
using SeleniumAutomation.TestData;

namespace SeleniumAutomation.TestClasses
{
    class WellConfiguration : PageObjects.WellConfigurationPage
    {
        public static string FilesLocation = ConfigurationManager.AppSettings.Get("FilesLocation");

        public void CreateWellWorkFlow(ExtentTest test)
        {
            // *********** Launch Browser *******************
            string isruunisats = ConfigurationManager.AppSettings.Get("isRunningATS");


            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.configurationTab);
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.wellconfigurationTab);
            SeleniumActions.WaitForLoad();
            #region GenralTab
            //If non zero db
            if (isruunisats == "false")
            {
                if (SeleniumActions.waitforDispalyed(PageObjects.WellConfigurationPage.btnCreateNewWell))
                {
                    SeleniumActions.WaitForLoad();
                    SeleniumActions.waitForElement(PageObjects.WellConfigurationPage.wellattributetabby);
                    SeleniumActions.waitForElement(PageObjects.WellConfigurationPage.btnCreateNewWellby);
                    SeleniumActions.waitClick(PageObjects.WellConfigurationPage.btnCreateNewWell);
                }
            }
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.wellconfigurationTab);
            SeleniumActions.waitForElement(PageObjects.WellConfigurationPage.boreholeinputby);
            SeleniumActions.sendText(PageObjects.WellConfigurationPage.wellnameinput, TestData.WellConfigData.RRFACName);
            SeleniumActions.selectKendoDropdownValue(PageObjects.WellConfigurationPage.welltypedropdwn, TestData.WellConfigData.WellTypeRRL);
            SeleniumActions.selectKendoDropdownValue(PageObjects.WellConfigurationPage.scadatypedrpdwn, TestData.WellConfigData.ScadaType);

            SeleniumActions.selectKendoDropdownValue(PageObjects.WellConfigurationPage.cygnetdomaindrpdwn, TestData.WellConfigData.CygNetDomain);//9077
            SeleniumActions.WaitForLoad();

            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.cygnetservicedrpdwn);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitForElement(PageObjects.WellConfigurationPage.Bycygnetservicename);
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.cygnetservicename);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.facilitybutton);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.facIdFilter);
            SeleniumActions.sendText(PageObjects.WellConfigurationPage.txtcontainsFiltertextbox, TestData.WellConfigData.RRFACName);
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.btnFilter);
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.firstRow);
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.btnApplyFilter);

            //M
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.commissioninput);

            SeleniumActions.sendText(PageObjects.WellConfigurationPage.commissioninput, "01022018", true);
            ////Spud date was removed from the UI as per FRI -3526 changes
            //SeleniumActions.sendText(PageObjects.WellConfigurationPage.initialspuddate, "01022018", true);
            SeleniumActions.sendText(PageObjects.WellConfigurationPage.wellboreinput, TestData.WellConfigData.RRFACName);
            SeleniumActions.sendspecialkey(PageObjects.WellConfigurationPage.wellboreinput, "tab");
            SeleniumActions.sendText(PageObjects.WellConfigurationPage.boreholeinput, TestData.WellConfigData.RRFACName);
            SeleniumActions.sendspecialkey(PageObjects.WellConfigurationPage.boreholeinput, "tab");
            SeleniumActions.sendText(PageObjects.WellConfigurationPage.intervalinput, TestData.WellConfigData.RRFACName);
            #endregion
            //Well Attribute tab
            #region WellAttribute
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.wellattributetab);
            SeleniumActions.sendText(PageObjects.WellConfigurationPage.leasename, "lease");
            SeleniumActions.sendText(PageObjects.WellConfigurationPage.regionname, "region");
            SeleniumActions.sendText(PageObjects.WellConfigurationPage.fieldname, "field");
            SeleniumActions.sendText(PageObjects.WellConfigurationPage.foremanname, "foreman");
            SeleniumActions.sendText(PageObjects.WellConfigurationPage.engineername, "engineer");
            SeleniumActions.sendText(PageObjects.WellConfigurationPage.gaugername, "gauger");
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.savebutton);
            SeleniumActions.waitForElement(PageObjects.WellConfigurationPage.Toaseter);
            SeleniumActions.WaitForLoad();
            #endregion

            test.Info("Well is created");

            CommonHelper.ChangeUnitSystemUserSetting("Metric");
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.SurveillaceTab);
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.WellStatusTab);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitForElement(PageObjects.WellConfigurationPage.InfProdKPItextUnitBy);
            CommonHelper.TraceLine("Elem 1 text : " + SeleniumActions.getInnerText(PageObjects.WellConfigurationPage.InfProdKPItextUnit));
            CommonHelper.TraceLine("Elem 2 text : " + SeleniumActions.getInnerText(PageObjects.WellConfigurationPage.InfProdKPItext));
            string infprodtext = SeleniumActions.getInnerText(PageObjects.WellConfigurationPage.InfProdKPItext);
            Assert.AreEqual("Inferred Production (m3/d)", infprodtext, "Mismatch in units");
            SeleniumActions.takeScreenshot("Metric Text");
            test.Info(" Unit System is changed to Metric", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "Metric Text.png"))).Build());

            CommonHelper.ChangeUnitSystemUserSetting("US");
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.wellconfigurationTab);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.WellStatusTab);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitForElement(PageObjects.WellConfigurationPage.InfProdKPItextUnitBy);
            CommonHelper.TraceLine("Elem 1 text : " + SeleniumActions.getInnerText(PageObjects.WellConfigurationPage.InfProdKPItextUnit));
            CommonHelper.TraceLine("Elem 2 text : " + SeleniumActions.getInnerText(PageObjects.WellConfigurationPage.InfProdKPItext));
            infprodtext = SeleniumActions.getInnerText(PageObjects.WellConfigurationPage.InfProdKPItext);
            Assert.AreEqual("Inferred Production (bbl/d)", infprodtext, "Mismatch in units");
            SeleniumActions.WaitForLoad();
            SeleniumActions.takeScreenshot("US Text");
            test.Info("Unit System is changed to US", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "US Text.png"))).Build());
            //SeleniumActions.waitClick(PageObjects.WellConfigurationPage.configurationTab);
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.wellconfigurationTab);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitForElement(PageObjects.WellConfigurationPage.wellattributetabby);
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.deletebutton);
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.firstdelete);
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.seconddelete);

            string welldeltext = SeleniumActions.getInnerText(PageObjects.WellConfigurationPage.Toaseter);
            CommonHelper.TraceLine(welldeltext);
            SeleniumActions.takeScreenshot("Well Deletion");
            Assert.AreEqual("Well has been successfully deleted.", welldeltext, "Well Deletion Toast did not appear");
            test.Pass("Well is Deleted", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "Well Deletion.png"))).Build());



            // *********** Switch to Frame  ******************

            // *********** Dispose  ******************
            SeleniumActions.disposeDriver();

        }

        public void CreateRRLUpgradeWellWorkFlow(ExtentTest test)
        {
            // *********** Create New FULL RRL Well  *******************

            try
            {
                CreateRRLWellFullonBlankDB(test);
                // Go to Well status and Scan
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.SurveillaceTab);
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.WellStatusTab);
                SeleniumActions.WaitForLoad();
                Thread.Sleep(5000);
                SeleniumActions.waitClick(PageObjects.WellStatusPage.btnScan);
                SeleniumActions.WaitForLoad();
                // FRI-4624 :   SeleniumActions.waitClick(PageObjects.WellStatusPage.btnConfirmSend);
                Toastercheck("Scan Commamnd", "Command Scan issued successfully.", test);
                string statustext = SeleniumActions.getInnerText(PageObjects.WellStatusPage.lblCommStatusBy);
                Assert.AreEqual("OK", statustext, "Communication is failed");
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.SurveillaceTab);
                SeleniumActions.WaitForLoad();
                // Go to Well Analysis and Collect Cards
                SeleniumActions.waitClick(PageObjects.DashboardPage.tabOptimization);
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(PageObjects.DashboardPage.tabAnalysis);
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(PageObjects.WellAnalysisPage.btnScanCards);
                SeleniumActions.waitClick(PageObjects.WellAnalysisPage.lnkCurrentCard);
                Thread.Sleep(2000);
                Toastercheck("Scan Card", "Scan card request sent.", test);
                SeleniumActions.WaitForLoad();
                VerifyRRLWellFullonBlankDB(test);
                ChangeWellType("Gas Lift", test);
                ViewWellTypeHistory(test);
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("CreateRRLUpgradeWellWorkFlow");
                CommonHelper.TraceLine("Error in CreateRRLUpgradeWellWorkFlow " + e.Message);
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "CreateRRLUpgradeWellWorkFlow.png"))).Build());
                Assert.Fail(e.ToString());

            }





            // *********** Switch to Frame  ******************

            // *********** Dispose  ******************
            SeleniumActions.disposeDriver();

        }

        public void CreateRRLWellFullonBlankDB(ExtentTest test)
        {
            try
            {
                string isruunisats = ConfigurationManager.AppSettings.Get("IsRunningInATS");
                SeleniumActions.waitClick(configurationTab);
                SeleniumActions.waitClick(wellconfigurationTab);
                SeleniumActions.WaitForLoad();

                #region GenralTab
                //If non zero db
                string strwellcount = SeleniumActions.getInnerText(PageObjects.DashboardPage.lblWelCount);
                CommonHelper.TraceLine("Well count is " + strwellcount);
                int wellcount = int.Parse(strwellcount.Trim());

                if (wellcount > 0)
                {
                    if (SeleniumActions.waitforDispalyed(btnCreateNewWell))
                    {
                        SeleniumActions.WaitForLoad();
                        //SeleniumActions.waitForElement(PageObjects.WellConfigurationPage.wellattributetabby);
                        //SeleniumActions.waitForElement(PageObjects.WellConfigurationPage.btnCreateNewWellby);
                        SeleniumActions.waitClick(btnCreateNewWell);
                    }
                }
                SeleniumActions.waitClick(wellconfigurationTab);
                SeleniumActions.waitForElement(boreholeinputby);
                SeleniumActions.sendText(wellnameinput, WellConfigData.RRFACName);
                SeleniumActions.selectKendoDropdownValue(welltypedropdwn, WellConfigData.WellTypeRRL);
                SeleniumActions.selectKendoDropdownValue(scadatypedrpdwn, WellConfigData.ScadaType);
                SeleniumActions.WaitForLoad();
                SeleniumActions.selectKendoDropdownValue(cygnetdomaindrpdwn, WellConfigData.CygNetDomain);//9077
                SeleniumActions.WaitForLoad();
                SeleniumActions.selectKendoDropdownValue(cygnetservicedrpdwn, WellConfigData.UIS);
                SeleniumActions.WaitForLoad();


                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.facilitybutton);
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.facIdFilter);
                SeleniumActions.sendText(PageObjects.WellConfigurationPage.txtcontainsFiltertextbox, TestData.WellConfigData.RRFACName);
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.btnFilter);
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.firstRow);
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.btnApplyFilter);

                //M
                SeleniumActions.WaitForLoad();
                SeleniumActions.selectKendoDropdownValue(PageObjects.WellConfigurationPage.assettypedrpdwn, TestData.WellConfigData.AssetName);
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.commissioninput);

                SeleniumActions.sendText(PageObjects.WellConfigurationPage.commissioninput, "01022018", true);
                ////Spud date was removed from the UI as per FRI -3526 changes
                //SeleniumActions.sendText(PageObjects.WellConfigurationPage.initialspuddate, "01022018", true);
                SeleniumActions.sendText(PageObjects.WellConfigurationPage.wellboreinput, TestData.WellConfigData.RRFACName);
                SeleniumActions.sendspecialkey(PageObjects.WellConfigurationPage.wellboreinput, "tab");
                SeleniumActions.sendText(PageObjects.WellConfigurationPage.boreholeinput, TestData.WellConfigData.RRFACName);
                SeleniumActions.sendspecialkey(PageObjects.WellConfigurationPage.boreholeinput, "tab");
                SeleniumActions.sendText(PageObjects.WellConfigurationPage.intervalinput, TestData.WellConfigData.RRFACName);
                SeleniumActions.selectKendoDropdownValue(PageObjects.WellConfigurationPage.welldepthreference, "Unknown");
                SeleniumActions.sendText(PageObjects.WellConfigurationPage.txtLatitude, "19.7899");
                SeleniumActions.sendText(PageObjects.WellConfigurationPage.txtLongitude, "79.0345");
                #endregion

                //Well Attribute tab
                #region WellAttribute
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.wellattributetab);
                SeleniumActions.sendText(PageObjects.WellConfigurationPage.leasename, "lease");
                SeleniumActions.sendText(PageObjects.WellConfigurationPage.regionname, "region");
                SeleniumActions.sendText(PageObjects.WellConfigurationPage.fieldname, "field");
                SeleniumActions.sendText(PageObjects.WellConfigurationPage.foremanname, "foreman");
                SeleniumActions.sendText(PageObjects.WellConfigurationPage.engineername, "engineer");
                SeleniumActions.sendText(PageObjects.WellConfigurationPage.gaugername, "gauger");

                #endregion
                //Surface Parameters
                #region SurfaceTab
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.tabSurface);
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.btnPumpingUnit);
                SeleniumActions.WaitForLoad();
                clickCellWithText("Lufkin");
                clickCellWithText("C");
                selectFromFilter("Item", "C912-365-168");
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClickJS(PageObjects.WellConfigurationPage.btnApplyPumpingUnit);

                SeleniumActions.selectKendoDropdownValue(PageObjects.WellConfigurationPage.kendoddWristPin, "3");
                SeleniumActions.selectKendoDropdownValue(PageObjects.WellConfigurationPage.kendoddRotation, "Clockwise");
                SeleniumActions.selectKendoDropdownValue(PageObjects.WellConfigurationPage.kendoddMotortype, "Nema B Electric");
                SeleniumActions.selectKendoDropdownValue(PageObjects.WellConfigurationPage.kendoddMotorSize, "50", true);
                SeleniumActions.selectKendoDropdownValue(PageObjects.WellConfigurationPage.kendoddSliptorque, "3");
                SeleniumActions.sendText(PageObjects.WellConfigurationPage.txtUpAmps, "144");

                SeleniumActions.sendText(PageObjects.WellConfigurationPage.txtdownAmps, "120");
                #endregion
                //Weights 
                #region Weights
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.tabWeights);

                SeleniumActions.selectKendoDropdownValue(PageObjects.WellConfigurationPage.kendoddCrankId, "9411OC");
                //Select All Crank 1 weights
                SeleniumActions.selectKendoDropdownValue(PageObjects.WellConfigurationPage.kendoddC1LeadPId, "6RO");
                SeleniumActions.selectKendoDropdownValue(PageObjects.WellConfigurationPage.kendoddC1LagPId, "OORO");

                //Select All Crank 2 weights
                SeleniumActions.selectKendoDropdownValue(PageObjects.WellConfigurationPage.kendoddC2LeadPId, "6RO");
                SeleniumActions.selectKendoDropdownValue(PageObjects.WellConfigurationPage.kendoddC2LagPId, "OORO");
                SeleniumActions.waitClick(savebutton);
                #endregion

                #region Downhole and Rods
                test.Info("Navigating to Job Management Page");

                while (SeleniumActions.IsElementDisplayedOnUI(SeleniumAutomation.PageObjects.JobManagementPage.jobManagementTab).ToString().Equals("False"))
                {
                    SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.fieldserviceTab);
                }

                SeleniumActions.waitClick(SeleniumAutomation.PageObjects.JobManagementPage.jobManagementTab);
                SeleniumActions.WaitForLoad();
                SeleniumActions.WaitForLoad();
                //Adding job and components to well
                string jobid = JobManagement.addjob(test);
                SeleniumActions.waitClick(PageObjects.JobManagementPage.plusbutton);
                SeleniumActions.waitClick(PageObjects.JobManagementPage.wellborereportoption);
                SeleniumActions.WaitForLoad();
                //Adding wellbore components for downhole and Rods
                JobManagement.Addcomponent(0, true);
                JobManagement.Addcomponent(1, true);
                JobManagement.Addcomponent(2, false);
                JobManagement.Addcomponent(3, true);
                JobManagement.Addcomponent(4, true);
                JobManagement.Addcomponent(5, false);
                JobManagement.Addcomponent(6, true);
                JobManagement.Addcomponent(7, true);
                SeleniumActions.waitClick(PageObjects.JobManagementPage.savewellboregridcomponets);
                string text = SeleniumActions.getInnerText(PageObjects.JobManagementPage.toaster).ToLower().Trim();
                Assert.AreEqual("saved successfully.", text);
                SeleniumActions.waitClickJS(PageObjects.WellConfigurationPage.wellconfigurationTab);
                SeleniumActions.WaitForLoad();
                test.Info("Well is created");
                #endregion
            }
            catch (Exception e)
            {

                SeleniumActions.takeScreenshot("CreateRRLWellFullonBlankDB");
                CommonHelper.TraceLine("Error in CreateRRLWellFullonBlankDB " + e.Message);
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "CreateRRLWellFullonBlankDB.png"))).Build());
                Assert.Fail(e.ToString());
            }
        }


        public void EnterDataForAdditionalWellAttributes()
        {
            #region Additional Well Attributes
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.btnsettings);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.lnkaddlwellattributes);
            SeleniumActions.WaitForLoad();

            SeleniumActions.sendText(PageObjects.AdditionalWellAttributes.FirstProductionDate, "01012019");
            SeleniumActions.sendText(PageObjects.AdditionalWellAttributes.AbandonmentDate, "01012019");

            SeleniumActions.waitClick(PageObjects.AdditionalWellAttributes.Categories.FinanceAndElectric);
            SeleniumActions.WaitForLoad();
            SeleniumActions.sendText(PageObjects.AdditionalWellAttributes.TaxCreditCode, "CODE");
            SeleniumActions.waitClick(PageObjects.AdditionalWellAttributes.PTTaxableStatusYes);
            SeleniumActions.sendText(PageObjects.AdditionalWellAttributes.PTTaxRate, "8.750");
            SeleniumActions.waitClick(PageObjects.AdditionalWellAttributes.CTTaxableStatusYes);
            SeleniumActions.sendText(PageObjects.AdditionalWellAttributes.CTTaxRate, "8.750");
            SeleniumActions.waitClick(PageObjects.AdditionalWellAttributes.WTTaxableStatusYes);
            SeleniumActions.sendText(PageObjects.AdditionalWellAttributes.WTTaxRate, "8.750");
            SeleniumActions.sendText(PageObjects.AdditionalWellAttributes.EstimatedKWHCost, "100.00");
            SeleniumActions.sendText(PageObjects.AdditionalWellAttributes.EstimatedKWHPowerCost, "100.00");
            SeleniumActions.sendText(PageObjects.AdditionalWellAttributes.DefaultDecimalWorkingInterest, "8.750");
            SeleniumActions.sendText(PageObjects.AdditionalWellAttributes.DefaultDecimalRoyaltyInterest, "8.750");

            SeleniumActions.waitClick(PageObjects.AdditionalWellAttributes.Categories.CongressionalCarterLocation);
            SeleniumActions.WaitForLoad();
            SeleniumActions.sendText(PageObjects.AdditionalWellAttributes.TownshipDirection, "N");
            SeleniumActions.sendText(PageObjects.AdditionalWellAttributes.TownshipNumber, "100");
            SeleniumActions.sendText(PageObjects.AdditionalWellAttributes.RangeDirection, "N");
            SeleniumActions.sendText(PageObjects.AdditionalWellAttributes.RangeNumber, "100");
            SeleniumActions.sendText(PageObjects.AdditionalWellAttributes.SectionIndicator, "ASD");
            SeleniumActions.sendText(PageObjects.AdditionalWellAttributes.SectionNumber, "100");
            SeleniumActions.sendText(PageObjects.AdditionalWellAttributes.Unit, "AD");
            SeleniumActions.sendText(PageObjects.AdditionalWellAttributes.Spot, "TEST");
            SeleniumActions.sendText(PageObjects.AdditionalWellAttributes.MeridianCode, "AS");
            SeleniumActions.sendText(PageObjects.AdditionalWellAttributes.MeridianName, "TEST");

            SeleniumActions.waitClick(PageObjects.AdditionalWellAttributes.Categories.PlotInformation);
            SeleniumActions.WaitForLoad();
            SeleniumActions.sendText(PageObjects.AdditionalWellAttributes.SurfaceNodeId, "ID");
            SeleniumActions.sendText(PageObjects.AdditionalWellAttributes.District, "Permian");
            SeleniumActions.sendText(PageObjects.AdditionalWellAttributes.AbstractNumber, "100");
            SeleniumActions.sendText(PageObjects.AdditionalWellAttributes.PlotName, "MyPlot");
            SeleniumActions.sendText(PageObjects.AdditionalWellAttributes.PlotSymbol, "MP");
            SeleniumActions.sendText(PageObjects.AdditionalWellAttributes.WellPlat, "Plat");
            SeleniumActions.sendText(PageObjects.AdditionalWellAttributes.DirectionToSite, "Test");

            SeleniumActions.waitClick(PageObjects.AdditionalWellAttributes.Categories.OffshoreInformation);
            SeleniumActions.WaitForLoad();
            SeleniumActions.sendText(PageObjects.AdditionalWellAttributes.GovernmentPlatformId, "ID");
            SeleniumActions.sendText(PageObjects.AdditionalWellAttributes.PlatformId, "ID");
            SeleniumActions.sendText(PageObjects.AdditionalWellAttributes.PlatformElevation, "100");
            SeleniumActions.sendText(PageObjects.AdditionalWellAttributes.WaterLineElevation, "100");
            SeleniumActions.sendText(PageObjects.AdditionalWellAttributes.WaterDepth, "100");
            SeleniumActions.sendText(PageObjects.AdditionalWellAttributes.OCSNumber, "10");
            SeleniumActions.sendText(PageObjects.AdditionalWellAttributes.BHBPrefix, "A");
            SeleniumActions.sendText(PageObjects.AdditionalWellAttributes.BHBNumber, "10");
            SeleniumActions.sendText(PageObjects.AdditionalWellAttributes.BHBSuffix, "B");
            SeleniumActions.sendText(PageObjects.AdditionalWellAttributes.AreaName, "Area");
            SeleniumActions.sendText(PageObjects.AdditionalWellAttributes.UTMQuadrant, "UTM");
            SeleniumActions.sendText(PageObjects.AdditionalWellAttributes.WatersIndicator, "W");
            SeleniumActions.sendText(PageObjects.AdditionalWellAttributes.WaterBottomZone, "BZ");

            SeleniumActions.waitClick(PageObjects.AdditionalWellAttributes.Categories.Other);
            SeleniumActions.WaitForLoad();
            SeleniumActions.sendText(PageObjects.AdditionalWellAttributes.LongWellName, "This is a very long well name");
            SeleniumActions.waitClick(PageObjects.AdditionalWellAttributes.DiscoveryWellYes);
            SeleniumActions.sendText(PageObjects.AdditionalWellAttributes.RegulatoryAgency, "IAOW");
            SeleniumActions.sendText(PageObjects.AdditionalWellAttributes.LegalDescription, "Legal Description");
            SeleniumActions.sendText(PageObjects.AdditionalWellAttributes.LandId, "ID");
            SeleniumActions.sendText(PageObjects.AdditionalWellAttributes.Satellite, "Y");
            SeleniumActions.sendText(PageObjects.AdditionalWellAttributes.CommunitizationId, "ID");
            SeleniumActions.waitClick(PageObjects.AdditionalWellAttributes.RadioactiveYes);
            SeleniumActions.waitClick(PageObjects.AdditionalWellAttributes.AutomationYes);
            SeleniumActions.waitClick(PageObjects.AdditionalWellAttributes.LeaseHeldByProductionYes);
            SeleniumActions.sendText(PageObjects.AdditionalWellAttributes.Remarks, "Remark");

            SeleniumActions.waitClick(PageObjects.AdditionalWellAttributes.SaveButton);
            SeleniumActions.WaitForLoad();
            //**** Not needed on Trunk version .. 4.3 ++ as Modal Dailog Closes Automaticaly on Save.
            //SeleniumActions.waitClick(PageObjects.AdditionalWellAttributes.CloseButton);
            //SeleniumActions.WaitForLoad();
            #endregion
        }



        public void VerifyDataForAdditionalWellAttributes(ExtentTest test)
        {
            #region Additional Well Attributes
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.btnsettings);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.lnkaddlwellattributes);
            SeleniumActions.WaitForLoad();
            SeleniumActions.CustomAssertEqual("01/01/2019", SeleniumActions.getAttribute(PageObjects.AdditionalWellAttributes.FirstProductionDate, "value"), "FirstProductionDate mismatch", "FirstProductionDate", test);
            SeleniumActions.CustomAssertEqual("01/01/2019", SeleniumActions.getAttribute(PageObjects.AdditionalWellAttributes.AbandonmentDate, "value"), "AbandonmentDate mismatch", "AbandonmentDate", test);

            SeleniumActions.waitClick(PageObjects.AdditionalWellAttributes.Categories.FinanceAndElectric);
            SeleniumActions.WaitForLoad();
            SeleniumActions.CustomAssertEqual("CODE", SeleniumActions.getAttribute(PageObjects.AdditionalWellAttributes.TaxCreditCode, "value"), "TaxCreditCode mismatch", "TaxCreditCode", test);
            SeleniumActions.CustomAssertEqual("true", SeleniumActions.getAttribute(PageObjects.AdditionalWellAttributes.PTTaxableStatusYes, "checked"), "PTTaxableStatusYes mismatch", "PTTaxableStatusYes", test);
            SeleniumActions.CustomAssertEqual("8.750", SeleniumActions.getAttribute(PageObjects.AdditionalWellAttributes.PTTaxRate, "value"), "PTTaxRate mismatch", "PTTaxRate", test);
            SeleniumActions.CustomAssertEqual("true", SeleniumActions.getAttribute(PageObjects.AdditionalWellAttributes.CTTaxableStatusYes, "checked"), "CTTaxableStatusYes mismatch", "CTTaxableStatusYes", test);
            SeleniumActions.CustomAssertEqual("8.750", SeleniumActions.getAttribute(PageObjects.AdditionalWellAttributes.CTTaxRate, "value"), "CTTaxRate mismatch", "CTTaxRate", test);
            SeleniumActions.CustomAssertEqual("true", SeleniumActions.getAttribute(PageObjects.AdditionalWellAttributes.WTTaxableStatusYes, "checked"), "WTTaxableStatusYes mismatch", "WTTaxableStatusYes", test);
            SeleniumActions.CustomAssertEqual("8.750", SeleniumActions.getAttribute(PageObjects.AdditionalWellAttributes.WTTaxRate, "value"), "WTTaxRate mismatch", "WTTaxRate", test);
            SeleniumActions.CustomAssertEqual("100.00", SeleniumActions.getAttribute(PageObjects.AdditionalWellAttributes.EstimatedKWHCost, "value"), "EstimatedKWHCost mismatch", "EstimatedKWHCost", test);
            SeleniumActions.CustomAssertEqual("100.00", SeleniumActions.getAttribute(PageObjects.AdditionalWellAttributes.EstimatedKWHPowerCost, "value"), "EstimatedKWHPowerCost mismatch", "EstimatedKWHPowerCost", test);
            SeleniumActions.CustomAssertEqual("8.750", SeleniumActions.getAttribute(PageObjects.AdditionalWellAttributes.DefaultDecimalWorkingInterest, "value"), "DefaultDecimalWorkingInterest mismatch", "DefaultDecimalWorkingInterest", test);
            SeleniumActions.CustomAssertEqual("8.750", SeleniumActions.getAttribute(PageObjects.AdditionalWellAttributes.DefaultDecimalRoyaltyInterest, "value"), "DefaultDecimalRoyaltyInterest mismatch", "DefaultDecimalRoyaltyInterest", test);

            SeleniumActions.waitClick(PageObjects.AdditionalWellAttributes.Categories.CongressionalCarterLocation);
            SeleniumActions.WaitForLoad();
            SeleniumActions.CustomAssertEqual("N", SeleniumActions.getAttribute(PageObjects.AdditionalWellAttributes.TownshipDirection, "value"), "TownshipDirection mismatch", "TownshipDirection", test);
            SeleniumActions.CustomAssertEqual("100", SeleniumActions.getAttribute(PageObjects.AdditionalWellAttributes.TownshipNumber, "value"), "TownshipNumber mismatch", "TownshipNumber", test);
            SeleniumActions.CustomAssertEqual("N", SeleniumActions.getAttribute(PageObjects.AdditionalWellAttributes.RangeDirection, "value"), "RangeDirection mismatch", "RangeDirection", test);
            SeleniumActions.CustomAssertEqual("100", SeleniumActions.getAttribute(PageObjects.AdditionalWellAttributes.RangeNumber, "value"), "RangeNumber mismatch", "RangeNumber", test);
            SeleniumActions.CustomAssertEqual("ASD", SeleniumActions.getAttribute(PageObjects.AdditionalWellAttributes.SectionIndicator, "value"), "SectionIndicator mismatch", "SectionIndicator", test);
            SeleniumActions.CustomAssertEqual("100", SeleniumActions.getAttribute(PageObjects.AdditionalWellAttributes.SectionNumber, "value"), "SectionNumber mismatch", "SectionNumber", test);
            SeleniumActions.CustomAssertEqual("AD", SeleniumActions.getAttribute(PageObjects.AdditionalWellAttributes.Unit, "value"), "Unit mismatch", "Unit", test);
            SeleniumActions.CustomAssertEqual("TEST", SeleniumActions.getAttribute(PageObjects.AdditionalWellAttributes.Spot, "value"), "Spot mismatch", "Spot", test);
            SeleniumActions.CustomAssertEqual("AS", SeleniumActions.getAttribute(PageObjects.AdditionalWellAttributes.MeridianCode, "value"), "MeridianCode mismatch", "MeridianCode", test);
            SeleniumActions.CustomAssertEqual("TEST", SeleniumActions.getAttribute(PageObjects.AdditionalWellAttributes.MeridianName, "value"), "MeridianName mismatch", "MeridianName", test);

            SeleniumActions.waitClick(PageObjects.AdditionalWellAttributes.Categories.PlotInformation);
            SeleniumActions.WaitForLoad();
            SeleniumActions.CustomAssertEqual("ID", SeleniumActions.getAttribute(PageObjects.AdditionalWellAttributes.SurfaceNodeId, "value"), "SurfaceNodeId mismatch", "SurfaceNodeId", test);
            SeleniumActions.CustomAssertEqual("Permian", SeleniumActions.getAttribute(PageObjects.AdditionalWellAttributes.District, "value"), "District mismatch", "District", test);
            SeleniumActions.CustomAssertEqual("100", SeleniumActions.getAttribute(PageObjects.AdditionalWellAttributes.AbstractNumber, "value"), "AbstractNumber mismatch", "AbstractNumber", test);
            SeleniumActions.CustomAssertEqual("MyPlot", SeleniumActions.getAttribute(PageObjects.AdditionalWellAttributes.PlotName, "value"), "PlotName mismatch", "PlotName", test);
            SeleniumActions.CustomAssertEqual("MP", SeleniumActions.getAttribute(PageObjects.AdditionalWellAttributes.PlotSymbol, "value"), "PlotSymbol mismatch", "PlotSymbol", test);
            SeleniumActions.CustomAssertEqual("Plat", SeleniumActions.getAttribute(PageObjects.AdditionalWellAttributes.WellPlat, "value"), "WellPlat mismatch", "WellPlat", test);
            SeleniumActions.CustomAssertEqual("Test", SeleniumActions.getAttribute(PageObjects.AdditionalWellAttributes.DirectionToSite, "value"), "DirectionToSite mismatch", "DirectionToSite", test);

            SeleniumActions.waitClick(PageObjects.AdditionalWellAttributes.Categories.OffshoreInformation);
            SeleniumActions.WaitForLoad();
            SeleniumActions.CustomAssertEqual("ID", SeleniumActions.getAttribute(PageObjects.AdditionalWellAttributes.GovernmentPlatformId, "value"), "GovernmentPlatformId mismatch", "GovernmentPlatformId", test);
            SeleniumActions.CustomAssertEqual("ID", SeleniumActions.getAttribute(PageObjects.AdditionalWellAttributes.PlatformId, "value"), "PlatformId mismatch", "PlatformId", test);
            SeleniumActions.CustomAssertEqual("100", SeleniumActions.getAttribute(PageObjects.AdditionalWellAttributes.PlatformElevation, "value"), "PlatformElevation mismatch", "PlatformElevation", test);
            SeleniumActions.CustomAssertEqual("100", SeleniumActions.getAttribute(PageObjects.AdditionalWellAttributes.WaterLineElevation, "value"), "WaterLineElevation mismatch", "WaterLineElevation", test);
            SeleniumActions.CustomAssertEqual("100", SeleniumActions.getAttribute(PageObjects.AdditionalWellAttributes.WaterDepth, "value"), "WaterDepth mismatch", "WaterDepth", test);
            SeleniumActions.CustomAssertEqual("10", SeleniumActions.getAttribute(PageObjects.AdditionalWellAttributes.OCSNumber, "value"), "OCSNumber mismatch", "OCSNumber", test);
            SeleniumActions.CustomAssertEqual("A", SeleniumActions.getAttribute(PageObjects.AdditionalWellAttributes.BHBPrefix, "value"), "BHBPrefix mismatch", "BHBPrefix", test);
            SeleniumActions.CustomAssertEqual("10", SeleniumActions.getAttribute(PageObjects.AdditionalWellAttributes.BHBNumber, "value"), "BHBNumber mismatch", "BHBNumber", test);
            SeleniumActions.CustomAssertEqual("B", SeleniumActions.getAttribute(PageObjects.AdditionalWellAttributes.BHBSuffix, "value"), "BHBSuffix mismatch", "BHBSuffix", test);
            SeleniumActions.CustomAssertEqual("Area", SeleniumActions.getAttribute(PageObjects.AdditionalWellAttributes.AreaName, "value"), "AreaName mismatch", "AreaName", test);
            SeleniumActions.CustomAssertEqual("UTM", SeleniumActions.getAttribute(PageObjects.AdditionalWellAttributes.UTMQuadrant, "value"), "UTMQuadrant mismatch", "UTMQuadrant", test);
            SeleniumActions.CustomAssertEqual("W", SeleniumActions.getAttribute(PageObjects.AdditionalWellAttributes.WatersIndicator, "value"), "WatersIndicator mismatch", "WatersIndicator", test);
            SeleniumActions.CustomAssertEqual("BZ", SeleniumActions.getAttribute(PageObjects.AdditionalWellAttributes.WaterBottomZone, "value"), "WaterBottomZone mismatch", "WaterBottomZone", test);

            SeleniumActions.waitClick(PageObjects.AdditionalWellAttributes.Categories.Other);
            SeleniumActions.WaitForLoad();
            SeleniumActions.CustomAssertEqual("This is a very long well name", SeleniumActions.getAttribute(PageObjects.AdditionalWellAttributes.LongWellName, "value"), "LongWellName mismatch", "LongWellName", test);
            SeleniumActions.CustomAssertEqual("true", SeleniumActions.getAttribute(PageObjects.AdditionalWellAttributes.DiscoveryWellYes, "checked"), "DiscoveryWellYes mismatch", "DiscoveryWellYes", test);
            SeleniumActions.CustomAssertEqual("IAOW", SeleniumActions.getAttribute(PageObjects.AdditionalWellAttributes.RegulatoryAgency, "value"), "RegulatoryAgency mismatch", "RegulatoryAgency", test);
            SeleniumActions.CustomAssertEqual("Legal Description", SeleniumActions.getAttribute(PageObjects.AdditionalWellAttributes.LegalDescription, "value"), "LegalDescription mismatch", "LegalDescription", test);
            SeleniumActions.CustomAssertEqual("ID", SeleniumActions.getAttribute(PageObjects.AdditionalWellAttributes.LandId, "value"), "LandId mismatch", "LandId", test);
            SeleniumActions.CustomAssertEqual("Y", SeleniumActions.getAttribute(PageObjects.AdditionalWellAttributes.Satellite, "value"), "Satellite mismatch", "Satellite", test);
            SeleniumActions.CustomAssertEqual("ID", SeleniumActions.getAttribute(PageObjects.AdditionalWellAttributes.CommunitizationId, "value"), "CommunitizationId mismatch", "CommunitizationId", test);
            SeleniumActions.CustomAssertEqual("true", SeleniumActions.getAttribute(PageObjects.AdditionalWellAttributes.RadioactiveYes, "checked"), "RadioactiveYes mismatch", "RadioactiveYes", test);
            SeleniumActions.CustomAssertEqual("true", SeleniumActions.getAttribute(PageObjects.AdditionalWellAttributes.AutomationYes, "checked"), "AutomationYes mismatch", "AutomationYes", test);
            SeleniumActions.CustomAssertEqual("true", SeleniumActions.getAttribute(PageObjects.AdditionalWellAttributes.LeaseHeldByProductionYes, "checked"), "LeaseHeldByProductionYes mismatch", "LeaseHeldByProductionYes", test);
            SeleniumActions.CustomAssertEqual("Remark", SeleniumActions.getAttribute(PageObjects.AdditionalWellAttributes.Remarks, "value"), "Remarks mismatch", "Remarks", test);

            SeleniumActions.waitClick(PageObjects.AdditionalWellAttributes.CloseButton);
            SeleniumActions.WaitForLoad();
            #endregion
        }



        public void CreateRRLWellPartialonBlankDB(ExtentTest test)
        {
            try
            {
                string isruunisats = ConfigurationManager.AppSettings.Get("IsRunningInATS");
                SeleniumActions.waitClick(configurationTab);
                SeleniumActions.waitClick(wellconfigurationTab);
                SeleniumActions.WaitForLoad();
                #region GenralTab
                //If non zero db
                string strwellcount = SeleniumActions.getInnerText(PageObjects.DashboardPage.lblWelCount);
                CommonHelper.TraceLine("Well count is " + strwellcount);
                int wellcount = int.Parse(strwellcount.Trim());

                if (wellcount > 0)
                {
                    if (SeleniumActions.waitforDispalyed(btnCreateNewWell))
                    {
                        SeleniumActions.WaitForLoad();
                        //SeleniumActions.waitForElement(PageObjects.WellConfigurationPage.wellattributetabby);
                        //SeleniumActions.waitForElement(PageObjects.WellConfigurationPage.btnCreateNewWellby);
                        SeleniumActions.waitClick(btnCreateNewWell);
                    }
                }
                SeleniumActions.waitClick(wellconfigurationTab);
                SeleniumActions.waitForElement(boreholeinputby);
                SeleniumActions.sendText(wellnameinput, WellConfigData.RRFACName);
                SeleniumActions.selectKendoDropdownValue(welltypedropdwn, WellConfigData.WellTypeRRL);
                SeleniumActions.selectKendoDropdownValue(scadatypedrpdwn, WellConfigData.ScadaType);
                SeleniumActions.WaitForLoad();
                SeleniumActions.selectKendoDropdownValue(cygnetdomaindrpdwn, WellConfigData.CygNetDomain);//9077
                SeleniumActions.WaitForLoad();
                SeleniumActions.selectKendoDropdownValue(cygnetservicedrpdwn, WellConfigData.UIS);
                SeleniumActions.WaitForLoad();


                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.facilitybutton);
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.facIdFilter);
                SeleniumActions.sendText(PageObjects.WellConfigurationPage.txtcontainsFiltertextbox, TestData.WellConfigData.RRFACName);
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.btnFilter);
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.firstRow);
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.btnApplyFilter);

                //M
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.commissioninput);

                SeleniumActions.sendText(PageObjects.WellConfigurationPage.commissioninput, "01022018", true);
                //Spud date was removed from the UI as per FRI -3526 changes
                //SeleniumActions.sendText(PageObjects.WellConfigurationPage.initialspuddate, "01022018", true);
                SeleniumActions.sendText(PageObjects.WellConfigurationPage.wellboreinput, TestData.WellConfigData.RRFACName);
                SeleniumActions.sendspecialkey(PageObjects.WellConfigurationPage.wellboreinput, "tab");
                SeleniumActions.sendText(PageObjects.WellConfigurationPage.boreholeinput, TestData.WellConfigData.RRFACName);
                SeleniumActions.sendspecialkey(PageObjects.WellConfigurationPage.boreholeinput, "tab");
                SeleniumActions.sendText(PageObjects.WellConfigurationPage.intervalinput, TestData.WellConfigData.RRFACName);
                SeleniumActions.selectKendoDropdownValue(PageObjects.WellConfigurationPage.welldepthreference, "Unknown");
                SeleniumActions.sendText(PageObjects.WellConfigurationPage.txtLatitude, "19.7899");
                SeleniumActions.sendText(PageObjects.WellConfigurationPage.txtLongitude, "79.0345");

                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.savebutton);
                SeleniumActions.waitForElement(PageObjects.WellConfigurationPage.Toaseter);
                SeleniumActions.WaitForLoad();
                #endregion
                Toastercheck("RRL Well Creation", "Well " + TestData.WellConfigData.RRFACName + " saved successfully.", test);
                test.Info("Well is created");
            }
            catch (Exception e)
            {

                SeleniumActions.takeScreenshot("CreateRRLWellFullonBlankDB");
                CommonHelper.TraceLine("Error in CreateRRLWellFullonBlankDB " + e.Message);
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "CreateRRLWellFullonBlankDB.png"))).Build());
                Assert.Fail(e.ToString());
            }
        }
        public string CreateRRLWellPartialonBlankDB(ExtentTest test, string wellname, string fac)
        {
            try
            {
                string isruunisats = ConfigurationManager.AppSettings.Get("IsRunningInATS");

                #region GenralTab
                //If non zero db
                string strwellcount = SeleniumActions.getInnerText(PageObjects.DashboardPage.lblWelCount);
                CommonHelper.TraceLine("Well count is " + strwellcount);
                int wellcount = int.Parse(strwellcount.Trim());


                if (wellcount > 0)
                {
                    if (SeleniumActions.waitforDispalyed(btnCreateNewWell))
                    {
                        SeleniumActions.WaitForLoad();
                        //SeleniumActions.waitForElement(PageObjects.WellConfigurationPage.wellattributetabby);
                        //SeleniumActions.waitForElement(PageObjects.WellConfigurationPage.btnCreateNewWellby);
                        SeleniumActions.waitClick(btnCreateNewWell);
                    }
                }

                SeleniumActions.waitClick(wellconfigurationTab);


                SeleniumActions.waitForElement(boreholeinputby);
                SeleniumActions.sendText(wellnameinput, wellname);
                SeleniumActions.selectKendoDropdownValue(welltypedropdwn, WellConfigData.WellTypeRRL);
                SeleniumActions.selectKendoDropdownValue(scadatypedrpdwn, WellConfigData.ScadaType);
                SeleniumActions.WaitForLoad();
                SeleniumActions.selectKendoDropdownValue(cygnetdomaindrpdwn, WellConfigData.CygNetDomain);//9077
                SeleniumActions.WaitForLoad();
                SeleniumActions.selectKendoDropdownValue(cygnetservicedrpdwn, WellConfigData.UIS);
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(facilitybutton);
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(facIdFilter);
                SeleniumActions.sendText(txtcontainsFiltertextbox, fac);
                SeleniumActions.waitClick(btnFilter);
                SeleniumActions.waitClick(firstRow);
                SeleniumActions.waitClick(btnApplyFilter);
                //M
                SeleniumActions.WaitForLoad();
                SeleniumActions.selectKendoDropdownValue(assettypedrpdwn, WellConfigData.AssetName);
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(commissioninput);
                SeleniumActions.sendText(commissioninput, "01022018", true);
                ////Spud date was removed from the UI as per FRI -3526 changes
                //SeleniumActions.sendText(PageObjects.WellConfigurationPage.initialspuddate, "01022018", true);
                SeleniumActions.sendText(wellboreinput, fac);
                SeleniumActions.sendspecialkey(wellboreinput, "tab");
                SeleniumActions.sendText(boreholeinput, fac);
                SeleniumActions.sendspecialkey(boreholeinput, "tab");
                SeleniumActions.sendText(intervalinput, fac);
                SeleniumActions.selectKendoDropdownValue(welldepthreference, "Unknown");
                SeleniumActions.sendText(txtLatitude, "19.7899");
                SeleniumActions.sendText(txtLongitude, "79.0345");

                SeleniumActions.waitClick(savebutton);
                SeleniumActions.waitForElement(Toaseter);
                SeleniumActions.WaitForLoad();
                #endregion
                Toastercheck("RRL Well Creation", "Well " + wellname + " saved successfully.", test);
                test.Info("Well is created");

                return wellname;
            }
            catch (Exception e)
            {

                SeleniumActions.takeScreenshot("CreateRRLWellFullonBlankDB");
                CommonHelper.TraceLine("Error in CreateRRLWellFullonBlankDB " + e.Message);
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", "CreateRRLWellFullonBlankDB.png"))).Build());
                Assert.Fail(e.ToString());
                return null;
            }
        }

        public string CreateRRLWellPartialonBlankDBAssets(ExtentTest test, string wellname, string fac)
        {
            try
            {
                string isruunisats = ConfigurationManager.AppSettings.Get("IsRunningInATS");

                #region GenralTab
                //If non zero db
                string strwellcount = SeleniumActions.getInnerText(PageObjects.DashboardPage.lblWelCount);
                CommonHelper.TraceLine("Well count is " + strwellcount);
                int wellcount = int.Parse(strwellcount.Trim());
                var guid = Guid.NewGuid().ToString();

                if (wellcount > 0)
                {
                    if (SeleniumActions.waitforDispalyed(btnCreateNewWell))
                    {
                        SeleniumActions.WaitForLoad();
                        //SeleniumActions.waitForElement(PageObjects.WellConfigurationPage.wellattributetabby);
                        //SeleniumActions.waitForElement(PageObjects.WellConfigurationPage.btnCreateNewWellby);
                        SeleniumActions.waitClick(btnCreateNewWell);
                    }
                }
                Thread.Sleep(2000);
                SeleniumActions.waitClick(wellconfigurationTab);
                Thread.Sleep(2000);
                SeleniumActions.waitForElement(boreholeinputby);
                SeleniumActions.sendText(wellnameinput, wellname);
                SeleniumActions.selectKendoDropdownValue(welltypedropdwn, WellConfigData.WellTypeRRL);
                SeleniumActions.selectKendoDropdownValue(scadatypedrpdwn, WellConfigData.ScadaType);
                SeleniumActions.WaitForLoad();
                SeleniumActions.selectKendoDropdownValue(cygnetdomaindrpdwn, WellConfigData.CygNetDomain);//9077
                SeleniumActions.WaitForLoad();
                SeleniumActions.selectKendoDropdownValue(cygnetservicedrpdwn, WellConfigData.UIS);
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(facilitybutton);
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(facIdFilter);
                SeleniumActions.sendText(txtcontainsFiltertextbox, fac);
                SeleniumActions.waitClick(btnFilter);
                SeleniumActions.waitClick(firstRow);
                SeleniumActions.waitClick(btnApplyFilter);
                //M
                SeleniumActions.WaitForLoad();
                SeleniumActions.selectKendoDropdownValue(assettypedrpdwn, WellConfigData.AssetName);
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(commissioninput);
                SeleniumActions.sendText(commissioninput, "01022018", true);
                ////Spud date was removed from the UI as per FRI -3526 changes
                //SeleniumActions.sendText(PageObjects.WellConfigurationPage.initialspuddate, "01022018", true);
                SeleniumActions.sendText(wellboreinput, fac);
                SeleniumActions.sendspecialkey(wellboreinput, "tab");
                SeleniumActions.sendText(boreholeinput, fac);
                SeleniumActions.sendspecialkey(boreholeinput, "tab");
                SeleniumActions.sendText(intervalinput, fac);
                SeleniumActions.selectKendoDropdownValue(welldepthreference, "Unknown");
                SeleniumActions.sendText(txtLatitude, "19.7899");
                SeleniumActions.sendText(txtLongitude, "79.0345");

                SeleniumActions.waitClick(savebutton);
                SeleniumActions.waitForElement(Toaseter);
                SeleniumActions.WaitForLoad();
                #endregion
                Toastercheck("RRL Well Creation", "Well " + wellname + " saved successfully.", test);
                test.Info("Well is created");

                return wellname;
            }
            catch (Exception e)
            {

                SeleniumActions.takeScreenshot("CreateRRLWellFullonBlankDB");
                CommonHelper.TraceLine("Error in CreateRRLWellFullonBlankDB " + e.Message);
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "CreateRRLWellFullonBlankDB.png"))).Build());
                Assert.Fail(e.ToString());
                return null;
            }
        }

        public void DashbordfilterVlidation(string name = "TestAsset")
        {
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.dashbord_filter);
            Thread.Sleep(3000);
            if (SeleniumActions.Gettotalrecords(PageObjects.WellConfigurationPage.assets_validation) == 0)
            {
                SeleniumActions.KendoTypeNSelect(PageObjects.WellConfigurationPage.assets_filter1, name);
                Thread.Sleep(3000);
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.applybtn);
            }
            else
            {
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.lnktargetclose);
            }
        }
        public void VerifyRRLWellFullonBlankDB(ExtentTest test)

        {
            string isruunisats = ConfigurationManager.AppSettings.Get("IsRunningATS");
            //   If non zero db
            if (SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellConfigurationPage.wellconfigurationTab) == false)
            {
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.configurationTab);
            }
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.wellconfigurationTab);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.general);
            SeleniumActions.waitForElement(PageObjects.WellConfigurationPage.facilitybutton);
            // selectWellfromWellList(TestData.WellConfigData.RRFACName);
            //Well general parameters verification.
            #region GenralTab
            SeleniumActions.CustomAssertEqual(TestData.WellConfigData.RRFACName, SeleniumActions.getText(PageObjects.WellConfigurationPage.wellnameinput), "Well Name mismatch", "Well Name", test);
            SeleniumActions.CustomAssertEqual("RRL", SeleniumActions.getInnerText(PageObjects.WellConfigurationPage.welltypefetchtextfield), "WellType Mismatch", "Well Type", test);
            SeleniumActions.CustomAssertEqual(TestData.WellConfigData.ScadaType, SeleniumActions.getInnerText(PageObjects.WellConfigurationPage.scadatypefetchtextfield), "Scada Type Mismatch", "Scada Type", test);
            SeleniumActions.CustomAssertEqual(TestData.WellConfigData.CygNetDomain, SeleniumActions.getInnerText(PageObjects.WellConfigurationPage.cygnetdomainfetchtextfield), "CygNet Domain Mismatch", "CygNet Domain", test);
            SeleniumActions.CustomAssertEqual(TestData.WellConfigData.UIS, SeleniumActions.getInnerText(PageObjects.WellConfigurationPage.cygnetservicefetchtextfield), "CygNet Site Service Mismatch", "CygNet Site", test);
            SeleniumActions.CustomAssertEqual(TestData.WellConfigData.RRFACName, SeleniumActions.getText(PageObjects.WellConfigurationPage.facilityfetchtextfield), "CygNet Facility Mismatch", "CygNet Facility", test);
            SeleniumActions.CustomAssertEqual("01/02/2018", SeleniumActions.getText(PageObjects.WellConfigurationPage.commissioninput, true), "Commision Date Mismatch", "Commission Date", test);
            ////Spud date was removed from the UI as per FRI -3526 changes
            //SeleniumActions.CustomAssertEqual("01/02/2018", SeleniumActions.getText(PageObjects.WellConfigurationPage.initialspuddate, true), "Initial Spud Date Mismatch", "Initial Spud Date", test);
            SeleniumActions.CustomAssertEqual(TestData.WellConfigData.RRFACName, SeleniumActions.getText(PageObjects.WellConfigurationPage.wellborefetchtextfield, true), "Wellbore Id Mismatch", "Wellbore Id", test);
            SeleniumActions.CustomAssertEqual(TestData.WellConfigData.RRFACName, SeleniumActions.getText(PageObjects.WellConfigurationPage.boreholefetchtextfield, true), "BoreWell Id Mismatch", "Wellbore Id", test);
            SeleniumActions.CustomAssertEqual(TestData.WellConfigData.RRFACName, SeleniumActions.getText(PageObjects.WellConfigurationPage.intervalinput), "Perforation Interval Id Mismatch", "Perforation Interval Id", test);
            SeleniumActions.CustomAssertEqual("19.7899", SeleniumActions.getText(PageObjects.WellConfigurationPage.txtLatitude), "Latitude Mismatch", "Latitude", test);
            SeleniumActions.CustomAssertEqual("79.0345", SeleniumActions.getText(PageObjects.WellConfigurationPage.txtLongitude), "Longitude Mismatch", "Longitude", test);
            #endregion
            //Well attributes
            #region attributes
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.wellattributetab);
            SeleniumActions.CustomAssertEqual("lease", SeleniumActions.getText(PageObjects.WellConfigurationPage.leasename), "Lease Name mismatch", "Lease Name", test);
            SeleniumActions.CustomAssertEqual("region", SeleniumActions.getText(PageObjects.WellConfigurationPage.regionname), "Region mismatch", "Region ", test);
            SeleniumActions.CustomAssertEqual("field", SeleniumActions.getText(PageObjects.WellConfigurationPage.fieldname), "Field mismatch", "Field Name", test);
            SeleniumActions.CustomAssertEqual("foreman", SeleniumActions.getText(PageObjects.WellConfigurationPage.foremanname), "Foreman Name mismatch", "Foreman Name", test);
            SeleniumActions.CustomAssertEqual("engineer", SeleniumActions.getText(PageObjects.WellConfigurationPage.engineername), "Engineer Name mismatch", "engineer Name", test);
            SeleniumActions.CustomAssertEqual("gauger", SeleniumActions.getText(PageObjects.WellConfigurationPage.gaugername), "gauger Name mismatch", "Gauge Name", test);

            #endregion
            //Surface Tab
            #region SurfaceVerification
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.tabSurface);

            SeleniumActions.CustomAssertEqual("C-912-365-168 L LUFKIN C912-365-168 (94110C)", SeleniumActions.getText(PageObjects.WellConfigurationPage.txtPumpUnitfetch), "Pumping Unit mismatch", "Pumping Unit", test);
            SeleniumActions.CustomAssertEqual("3", SeleniumActions.getKendoDDSelectedText(PageObjects.WellConfigurationPage.kendoddWristPin), "Wrist Pin data mismatch", "Wrist Pin data", test);
            SeleniumActions.CustomAssertEqual("124.510", SeleniumActions.getText(PageObjects.WellConfigurationPage.txtActualStrokeLength), "Actual Stroke Length mismatch", "Actual Stroke Length", test);
            SeleniumActions.CustomAssertEqual("Clockwise", SeleniumActions.getKendoDDSelectedText(PageObjects.WellConfigurationPage.kendoddRotation), "Rotation mismatch", "Rotation", test);
            SeleniumActions.CustomAssertEqual("Nema B Electric", SeleniumActions.getKendoDDSelectedText(PageObjects.WellConfigurationPage.kendoddMotortype), "Motortype mismatch", "Motortype", test);
            SeleniumActions.CustomAssertEqual("50hp(E)", SeleniumActions.getKendoDDSelectedText(PageObjects.WellConfigurationPage.kendoddMotorSize), "MotorSize mismatch", "MotorSize", test);
            SeleniumActions.CustomAssertEqual("3", SeleniumActions.getKendoDDSelectedText(PageObjects.WellConfigurationPage.kendoddSliptorque), "Slip torque mismatch", "Sliptorque", test);
            SeleniumActions.CustomAssertEqual("144", SeleniumActions.getText(PageObjects.WellConfigurationPage.txtUpAmps), "Up Amps mismatch", "Up Amps", test);
            SeleniumActions.CustomAssertEqual("120", SeleniumActions.getText(PageObjects.WellConfigurationPage.txtdownAmps), "Down Amps mismatch", "Down Amps", test);
            #endregion

            //Weights Tab
            #region WeigthsVerification
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.tabWeights);
            SeleniumActions.CustomAssertEqual("9411OC", SeleniumActions.getKendoDDSelectedText(PageObjects.WellConfigurationPage.kendoddCrankId), "CrankId mismatch", "CrankId", test);
            SeleniumActions.CustomAssertEqual("6RO", SeleniumActions.getKendoDDSelectedText(PageObjects.WellConfigurationPage.kendoddC1LeadPId), "C1LeadPId mismatch", "C1LeadPId", test);
            SeleniumActions.CustomAssertEqual("OORO", SeleniumActions.getKendoDDSelectedText(PageObjects.WellConfigurationPage.kendoddC1LagPId), "C1LagPId mismatch", "C1LagPId", test);
            SeleniumActions.CustomAssertEqual("6RO", SeleniumActions.getKendoDDSelectedText(PageObjects.WellConfigurationPage.kendoddC2LeadPId), "C2LeadPId mismatch", "C2LeadPId", test);
            SeleniumActions.CustomAssertEqual("OORO", SeleniumActions.getKendoDDSelectedText(PageObjects.WellConfigurationPage.kendoddC2LagPId), "C2LagPId mismatch", "C2LagPId", test);
            SeleniumActions.CustomAssertEqual("API", SeleniumActions.getKendoDDSelectedText(PageObjects.WellConfigurationPage.kendoddTorqueCalculationMethod), "CrankId mismatch", "CrankId", test);
            #endregion
            //Downhole Tab
            #region Downholeverification
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.tabDownhole);
            SeleniumActions.CustomAssertEqual("1.66", SeleniumActions.getKendoDDSelectedText(PageObjects.WellConfigurationPage.kendoDDPumpDiameter), "PumpDiameter mismatch", "PumpDiameter", test);
            SeleniumActions.CustomAssertEqual("200.00", SeleniumActions.get_Attribute(PageObjects.WellConfigurationPage.txtPumpDepth, "value").ToString(), "PumpDepth mismatch", "PumpDepth", test);
            SeleniumActions.CustomAssertEqual("4.500", SeleniumActions.get_Attribute(PageObjects.WellConfigurationPage.txtTubingOD, "value").ToString(), "TubingOD mismatch", "TubingOD", test);
            SeleniumActions.CustomAssertEqual("4.052", SeleniumActions.get_Attribute(PageObjects.WellConfigurationPage.txtTubingID, "value").ToString(), "TubingID mismatch", "TubingID", test);
            SeleniumActions.CustomAssertEqual("100.00", SeleniumActions.get_Attribute(PageObjects.WellConfigurationPage.txtTubingAnchorDepth, "value").ToString(), "TubingAnchorDepth mismatch", "TubingAnchorDepth", test);
            SeleniumActions.CustomAssertEqual("2.875", SeleniumActions.get_Attribute(PageObjects.WellConfigurationPage.txtCasingOD, "value").ToString(), "CasingOD mismatch", "CasingOD", test);
            SeleniumActions.CustomAssertEqual("6.40", SeleniumActions.get_Attribute(PageObjects.WellConfigurationPage.txtCasingWeight, "value").ToString(), "CasingWeight mismatch", "CasingWeight", test);
            SeleniumActions.CustomAssertEqual("350.00", SeleniumActions.get_Attribute(PageObjects.WellConfigurationPage.txtTopPerforation, "value").ToString(), "TopPerforation mismatch", "TopPerforation", test);
            SeleniumActions.CustomAssertEqual("450.00", SeleniumActions.get_Attribute(PageObjects.WellConfigurationPage.txtBottomPerforation, "value").ToString(), "BottomPerforation mismatch", "BottomPerforation", test);
            #endregion
            //Rods Tab
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.tabRods);
            SeleniumActions.CustomAssertEqual("200", SeleniumActions.getText(PageObjects.WellConfigurationPage.txtPumpDepthRodstab), "PumpDepthRodstabs mismatch", "PumpDepthRodstab", test);
            SeleniumActions.CustomAssertEqual("210", SeleniumActions.getText(PageObjects.WellConfigurationPage.txtTotalRodlength), "Total Rod length mismatch", "Total Rod length", test);
            //Table values verification :
            string expcolnames = "Taper #;Manufacturer;Grade;Size" + Environment.NewLine + "in;Length" + Environment.NewLine + "ft;Rod Count;Total Length" + Environment.NewLine + "ft;Guides;Damping Down;Damping Up;Service Factor";
            string expcolvals_row1 = "1;Weatherford, Inc.;EL;0.75;1;5;5;0;0.016;0.014;0.9";
            string expcolvals_row2 = "2;Weatherford, Inc.;C;1;1;5;5;0;0.02;0.018;0.9";
            string expcolvals_row3 = "3;Weatherford, Inc.;D;1;50;4;200;0;0.02;0.018;0.9";
            SeleniumActions.verifyGridCellValues(expcolnames, expcolvals_row1, test, 0, "Manufacturer;Weatherford, Inc.");
            SeleniumActions.verifyGridCellValues(expcolnames, expcolvals_row2, test, 1, "Manufacturer;Weatherford, Inc.");
            SeleniumActions.verifyGridCellValues(expcolnames, expcolvals_row3, test, 2, "Manufacturer;Weatherford, Inc.");

        }

        public void Create_WellWorkFlow(ExtentTest test, string welltype, bool modelchk)
        {
            try
            {
                switch (welltype)
                {
                    case "ESP":
                        CreateESPWellFullonBlankDB(test, modelchk);
                        break;
                    case "NF":
                        CreateNFWellFullonBlankDB(test, modelchk);
                        break;
                    case "GL":
                        CreateGLWellFullonBlankDB(test, modelchk);
                        break;
                    case "GInj":
                        CreateGInjWellFullonBlankDB(test, modelchk);
                        break;
                    case "WInj":
                        CreateWInjWellFullonBlankDB(test, modelchk);
                        break;
                    case "PGL":
                        CreatePGLWellFullonBlankDB(test, modelchk);
                        break;
                    case "WaterInj":
                        CreateWaterInjWellFullonBlankDB(test, modelchk);
                        break;
                    case "OT":
                        CreateOtherTypeWellFullonBlankDB(test, modelchk);
                        break;
                }

                EnterDataForAdditionalWellAttributes();
                VerifyDataForAdditionalWellAttributes(test);

                if (modelchk || welltype == "OT")
                {
                    VerifyModelData(welltype, test);
                    SeleniumActions.WaitForLoad();
                    Deletewell();
                }
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("CreateWellWorkFlow" + welltype);
                CommonHelper.TraceLine("Error in CreateWellWorkFlow for" + welltype + e.Message);
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "CreateWellWorkFlow" + welltype + ".png"))).Build());
                //       Helper.CommonHelper.Deleteasset();
                Assert.Fail(e.ToString());
            }

            // *********** Switch to Frame  ******************

            // *********** Dispose  ******************
            SeleniumActions.disposeDriver();

        }

        public void Targetconfigsettings(ExtentTest test)
        {
            // *********** Create New FULL ESP Well  *******************

            try
            {
                SettingsCommon();
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.lnktargetconfig);
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.lnktargetadd);
                SeleniumActions.sendText(PageObjects.WellConfigurationPage.txttargetstart, "12122019");
                SeleniumActions.sendText(PageObjects.WellConfigurationPage.txttargetend, "12122019");
                SeleniumActions.sendText(PageObjects.WellConfigurationPage.txtoillowerbound, "1");
                SeleniumActions.sendText(PageObjects.WellConfigurationPage.txtoiltarget, "2");
                SeleniumActions.sendText(PageObjects.WellConfigurationPage.txtoilupperbound, "2");
                SeleniumActions.sendText(PageObjects.WellConfigurationPage.txtoilminimum, "1");
                SeleniumActions.sendText(PageObjects.WellConfigurationPage.txtwaterlowerbound, "1");
                SeleniumActions.sendText(PageObjects.WellConfigurationPage.txtwatertarget, "2");
                SeleniumActions.sendText(PageObjects.WellConfigurationPage.txtwaterupperbound, "2");
                SeleniumActions.sendText(PageObjects.WellConfigurationPage.txtwaterminimum, "1");
                SeleniumActions.sendText(PageObjects.WellConfigurationPage.txtgaslowerbound, "1");
                SeleniumActions.sendText(PageObjects.WellConfigurationPage.txtgastarget, "2");
                SeleniumActions.sendText(PageObjects.WellConfigurationPage.txtgasupperbound, "2");
                SeleniumActions.sendText(PageObjects.WellConfigurationPage.txtgasminimum, "1");
                decimal oiltechnicallimit = Math.Round(Convert.ToDecimal(SeleniumActions.getText(PageObjects.WellConfigurationPage.txtoiltechnicallimit)));

                decimal watertechnicallimit = Math.Round(Convert.ToDecimal(SeleniumActions.getText(PageObjects.WellConfigurationPage.txtwatertechnicallimit)));
                decimal gastechnicallimit = Math.Round(Convert.ToDecimal(SeleniumActions.getText(PageObjects.WellConfigurationPage.txtgastechnicallimit)));
                Assert.AreEqual(50000, oiltechnicallimit, "Oil Technical limit is not displayed correct");
                Assert.AreEqual(50000, watertechnicallimit, "water Technical limit is not displayed correct");
                Assert.AreEqual(1000000, gastechnicallimit, "Gas Technical limit is not displayed correct");
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.lnktargetsave);
                SeleniumActions.Toastercheck("Save Target Configuration", "Saved successfully.", test);
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.lnktargetclose);
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.btnsettings);
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.lnktargetconfig);
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.lnktargetchkbox);
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.lnktargetdelete);
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.lnktargetdeleteconfirm);
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.btntgtclose);
                SeleniumActions.WaitForLoad();
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("TargetConfigurationSettings");
                CommonHelper.TraceLine("Error in TargetConfigurationSettings " + e.Message);
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "TargetConfigurationSettings" + ".png"))).Build());
                Assert.Fail(e.ToString());

            }
            finally
            {

                SeleniumActions.waitforPageloadComplete();
                Deletewell();
                // *********** Switch to Frame  ******************

                // *********** Dispose  ******************
                SeleniumActions.disposeDriver();
            }
        }
        public void Acceptancelimitsettings(ExtentTest test)
        {
            // *********** Create New FULL ESP Well  *******************

            try
            {
                SettingsCommon();
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.lnkacceptancelimits);
                SeleniumActions.WaitForLoad();
                Assert.IsTrue(SeleniumActions.isElemPresent(PageObjects.WellConfigurationPage.lbllfactor), "L factor Missing");
                //this line is commented as per FIR - 4234 story 
                // Assert.IsTrue(SeleniumActions.isElemPresent(PageObjects.WellConfigurationPage.lbloperatingpoint),"Operating Point Missing");
                Assert.IsTrue(SeleniumActions.isElemPresent(PageObjects.WellConfigurationPage.lblreservoirPressure), "REs Pressure Missing");
                Assert.IsTrue(SeleniumActions.isElemPresent(PageObjects.WellConfigurationPage.lblChokeDFactor), "Choke D factor Missing");
                Assert.IsTrue(SeleniumActions.isElemPresent(PageObjects.WellConfigurationPage.lblWaterCut), "Water Cut Missing");
                Assert.IsTrue(SeleniumActions.isElemPresent(PageObjects.WellConfigurationPage.lblaccptgor), "GOR Missing");
                Assert.IsTrue(SeleniumActions.isElemPresent(PageObjects.WellConfigurationPage.lblPI), "PI Missing");
                Assert.IsTrue(SeleniumActions.isElemPresent(PageObjects.WellConfigurationPage.lblwatersalinity), "Water Sailinity Missing");
                Assert.IsTrue(SeleniumActions.isElemPresent(PageObjects.WellConfigurationPage.lblHeadTuningFactor), "Head Tuning Factor Missing");
                Assert.IsTrue(SeleniumActions.isElemPresent(PageObjects.WellConfigurationPage.lblperctdiff), "Perecentage Diff Missing");
                Assert.IsTrue(SeleniumActions.isElemPresent(PageObjects.WellConfigurationPage.lblestimatedwellhdpr), "ESt THP Missing");
                Assert.IsTrue(SeleniumActions.isElemPresent(PageObjects.WellConfigurationPage.lblwellhdpr), "THP Missing");
                Assert.IsTrue(SeleniumActions.isElemPresent(PageObjects.WellConfigurationPage.lblgor2), "GOR 2 Missing");
                Assert.IsTrue(SeleniumActions.isElemPresent(PageObjects.WellConfigurationPage.lbloilrate), "Oil Rate Missing");
                Assert.IsTrue(SeleniumActions.isElemPresent(PageObjects.WellConfigurationPage.lblliquidrate), "Liquid Rate Missing");
                Assert.IsTrue(SeleniumActions.isElemPresent(PageObjects.WellConfigurationPage.lblwatercut2), "Warer Cut 2 Missing");
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.overridelfactor);
                //this line is commented as per FIR - 4234 story 
                // SeleniumActions.waitClick(PageObjects.WellConfigurationPage.overrideoperatingpoint);
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.overridereservoirPressure);
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.overrideChokeDFactor);
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.overrideWaterCut);
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.overrideaccptgor);
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.overridePI);
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.overridewatersalinity);
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.overrideHeadTuningFactor);
                SeleniumActions.scrollintoview(overrideestimatedwellhdpr);
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.overrideestimatedwellhdpr);
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.overridewellhdpr);
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.overridegor2);
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.overrideoilrate);
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.overrideliquidrate);
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.overridewatercut2);
                SeleniumActions.kendotextboxentervalues("xpath", txtlfactormin, "0.1", "2", "L factor acceptance limits");
                //this line is commented as per FIR - 4234 story 
                //SeleniumActions.kendotextboxentervalues("xpath", operatingPointMin, "0", "10000", "Opearting point acceptance limits");
                SeleniumActions.kendotextboxentervalues("xpath", reservoirPressureMin, "14.7", "25000", "Reservoir Pressure Min acceptance limits");
                SeleniumActions.kendotextboxentervalues("xpath", chokeFactorMin, "0.1", "2", "ChokeFactor acceptance limits");
                SeleniumActions.kendotextboxentervalues("xpath", waterCutMin, "0", "1", "Water cut acceptance limits");
                SeleniumActions.kendotextboxentervalues("xpath", chokeFactorMin, "0.1", "2", "ChokeFactor acceptance limits");
                SeleniumActions.kendotextboxentervalues("xpath", gasOilRatioMin, "0", "200000", "GOR acceptance limits");
                SeleniumActions.kendotextboxentervalues("xpath", productivityIndexMin, "0", "9999", "ProductivityIndex acceptance limits");
                SeleniumActions.kendotextboxentervalues("xpath", waterSalinityMin, "0", "500000", "Water Salinity acceptance limits");
                SeleniumActions.kendotextboxentervalues("xpath", wearFactorMin, "0.1", "2", "GOR Head tuning factor limits");

                SeleniumActions.sendText(txtestimatedWHPDifferenceMax, "100");
                SeleniumActions.sendText(txtestimatedWHPDifferenceMax, "100");
                SeleniumActions.sendText(txtdefaultWhpDifferenceMax, "100");
                SeleniumActions.sendText(txtgorDifferenceMax, "100");
                SeleniumActions.sendText(txtoilRateDifferenceMax, "100");
                SeleniumActions.sendText(txtoilRateDifferenceMax, "100");
                SeleniumActions.sendText(txtliquidRateDifferenceMax, "100");
                SeleniumActions.sendText(txtwaterCutDifferenceMax, "100");
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.lbllfactor);
                Thread.Sleep(1000);
                SeleniumActions.waitClick(btnaccptsave);
                SeleniumActions.WaitForLoad();
                SeleniumActions.Toastercheck("Toaster for Acceptance Limits", "Acceptance limits saved successfully.", test);
                //   SeleniumActions.waitClick(btntgtclose);
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.lblwellname);

            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("AcceptanceSettings");
                CommonHelper.TraceLine("Error in AcceptanceSettings " + e.Message);
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "AcceptanceSettings" + ".png"))).Build());
                Assert.Fail(e.ToString());

            }
            finally
            {

                Deletewell();
                SeleniumActions.disposeDriver();
            }


        }

        public void SettingsCommon()
        {
            CommonHelper.CreateESPWell();
            SeleniumActions.refreshBrowser();
            SeleniumActions.WaitForLoad();
            SeleniumActions.WaitforWellcountToBeNonZero();
            SeleniumActions.waitClick(PageObjects.DashboardPage.tabDashboards);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.DashboardPage.tabProdDashboard);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitForElement(SeleniumAutomation.PageObjects.WellConfigurationPage.pedashboard);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.configurationTab);
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.wellconfigurationTab);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClickJS(PageObjects.WellConfigurationPage.btnsettings);
            SeleniumActions.WaitForLoad();
        }

        public void OperatingLimitssettings(ExtentTest test)
        {
            // *********** Create New FULL ESP Well  *******************

            try
            {
                SettingsCommon();
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.lnkoperatinglimits);
                SeleniumActions.WaitForLoad();
                SeleniumActions.isElemPresent(PageObjects.WellConfigurationPage.lbltubingheadpressure);
                SeleniumActions.isElemPresent(PageObjects.WellConfigurationPage.lbltubingheadtemp);
                SeleniumActions.isElemPresent(PageObjects.WellConfigurationPage.lblcasingheadPressure);
                SeleniumActions.isElemPresent(PageObjects.WellConfigurationPage.lblopgasrate);
                SeleniumActions.isElemPresent(PageObjects.WellConfigurationPage.lblopoilrate);
                SeleniumActions.isElemPresent(PageObjects.WellConfigurationPage.lblopwaterrate);
                SeleniumActions.isElemPresent(PageObjects.WellConfigurationPage.lblpumpintakepressure);
                SeleniumActions.isElemPresent(PageObjects.WellConfigurationPage.lblpumpdischargepressure);
                SeleniumActions.isElemPresent(PageObjects.WellConfigurationPage.lblflowlinepressure);
                SeleniumActions.isElemPresent(PageObjects.WellConfigurationPage.lblcasinggasrate);
                SeleniumActions.isElemPresent(PageObjects.WellConfigurationPage.lblmotorfreq);
                SeleniumActions.isElemPresent(PageObjects.WellConfigurationPage.lblmotorvolts);
                SeleniumActions.scrollintoview(lblmotoramps);
                SeleniumActions.isElemPresent(PageObjects.WellConfigurationPage.lblmotoramps);
                SeleniumActions.isElemPresent(PageObjects.WellConfigurationPage.lblmotortemp);
                SeleniumActions.isElemPresent(PageObjects.WellConfigurationPage.lblchokedia);
                SeleniumActions.isElemPresent(PageObjects.WellConfigurationPage.lblruntime);
                Thread.Sleep(2000);
                SeleniumActions.scrollintoview(overridetubpressure);
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.overridetubpressure);
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.overridetubtemp);
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.overridecasingheadPressure);
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.overrideopoilrate);
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.overrideopwaterrate);
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.overrideopgasrate);
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.overridepumpintakepressure);
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.overridepumpdischargepressure);
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.overrideflowlinepressure);
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.overridecasinggasrate);
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.overridemotorfreq);
                // SeleniumActions.waitClick(PageObjects.WellConfigurationPage.overridemotortemp);
                SeleniumActions.scrollintoview(overridemotoramps);
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.overridemotoramps);
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.overridemotortemp);
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.overridemotorvolts);
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.overridechokedia);
                //SeleniumActions.waitClick(PageObjects.WellConfigurationPage.overrideruntime);
                SeleniumActions.WaitForLoad();
                SeleniumActions.kendotextboxentervalues("xpath", txtloptubingpressminm, "14.7", "15000", "Tubing Pressure acceptance limits");
                SeleniumActions.kendotextboxentervalues("xpath", txtloptubingtempminm, "0", "1000", "Tubing Temperature acceptance limits");
                SeleniumActions.kendotextboxentervalues("xpath", txtcasingpressure, "14.7", "20000", "Casing head pressure acceptance limits");
                SeleniumActions.kendotextboxentervalues("xpath", txtoilminm, "0", "50000", "OilRate acceptance limits");
                SeleniumActions.kendotextboxentervalues("xpath", txtwaterminm, "0", "150000", "WaterRate acceptance limits");

                SeleniumActions.kendotextboxentervalues("xpath", txtgasminm, "0", "20000", "Gasrate acceptance limits");
                SeleniumActions.kendotextboxentervalues("xpath", txtpumpintkimnm, "14.7", "25000", "Pump Intake acceptance limits");
                SeleniumActions.kendotextboxentervalues("xpath", txtpumpdiscminm, "14.7", "25000", "Pump Discharge acceptance limits");
                SeleniumActions.kendotextboxentervalues("xpath", txtflowinlnpr, "14.7", "25000", "Flowline pressure acceptance limits");
                SeleniumActions.kendotextboxentervalues("xpath", txtcasinggasrate, "14.7", "15000", "Casing gas rate limits");
                SeleniumActions.kendotextboxentervalues("xpath", txtmotorfreq, "10", "120", "Motor frequency limits");
                SeleniumActions.kendotextboxentervalues("xpath", txtmotorvolts, "0", "5000", "Motor volts limits");
                SeleniumActions.kendotextboxentervalues("xpath", txtmotoramps, "0", "500", "Motor amps limits");
                SeleniumActions.scrollintoview(motortemp);
                SeleniumActions.kendotextboxentervalues("xpath", txtmotortemp, "0", "1000", "Motor temp limits");
                SeleniumActions.kendotextboxentervalues("xpath", txtchokedia, "0", "756", "Choke dia limits");
                // SeleniumActions.kendotextboxentervalues("xpath", txtruntime, "0", "756", "Runtime limits");


                SeleniumActions.waitClick(btnoptsave);
                SeleniumActions.WaitForLoad();
                SeleniumActions.Toastercheck("Toaster for Operating Limits", "Operating limits saved successfully.", test);
                //   SeleniumActions.waitClick(btntgtclose);
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.lblwellname);
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

                SeleniumActions.WaitForLoad();
                Deletewell();
                // *********** Switch to Frame  ******************

                // *********** Dispose  ******************
                SeleniumActions.disposeDriver();
            }
        }

        public void Addperformancecurves(ExtentTest test)
        {
            // *********** Create New FULL ESP Well  *******************

            try
            {
                SettingsCommon();
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.lnkperformancecurves);
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.btnperfadd);
                SeleniumActions.sendText(PageObjects.WellConfigurationPage.txtfrequency, "20");
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.btnperfsave);
                //   SeleniumActions.waitClick(PageObjects.WellConfigurationPage.btnperfclose);
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.btnsettings);
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.lnkperformancecurves);
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.btnperfdelete);
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.btnperfclose);




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

                SeleniumActions.WaitForLoad();
                Deletewell();
                // *********** Switch to Frame  ******************

                // *********** Dispose  ******************
                SeleniumActions.disposeDriver();
            }
        }



        public void CreateESPWellFullonBlankDB(ExtentTest test, bool modelchk = false)
        {
            string isruunisats = ConfigurationManager.AppSettings.Get("isRunningATS");
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.configurationTab);
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.wellconfigurationTab);
            SeleniumActions.WaitForLoad();

            #region GenralTab
            EnterwellGeneralData(TestData.WellConfigData.WellTypeESP, "ESP");
            uploadmodelfile("Esp_ProductionTestData.wflx", "ESP");
            string facilityname = PageObjects.WellConfigurationPage.wellnametextbox.GetAttribute("value");
            #endregion
            Enterwellattributes();
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.savebutton);
            SeleniumActions.waitForElement(PageObjects.WellConfigurationPage.Toaseter);
            Toastercheck("ESP Well Creation", "Well " + facilityname + " saved successfully.", test);
            test.Info("Well is created");
            SeleniumActions.WaitForLoad();
            //  selectWellfromWellList(facilityname);
        }

        public void CreateNFWellFullonBlankDB(ExtentTest test, bool modelchk = false)
        {
            string isruunisats = ConfigurationManager.AppSettings.Get("isRunningATS");
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.configurationTab);
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.wellconfigurationTab);
            SeleniumActions.WaitForLoad();
            #region GenralTab
            EnterwellGeneralData(TestData.WellConfigData.WellTypeNF, "NF");
            uploadmodelfile("NF.wflx", "NF");
            string facilityname = PageObjects.WellConfigurationPage.wellnametextbox.GetAttribute("value");
            #endregion
            Enterwellattributes();
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.savebutton);
            SeleniumActions.waitForElement(PageObjects.WellConfigurationPage.Toaseter);
            Toastercheck("NF Well Creation", "Well " + facilityname + " saved successfully.", test);
            test.Info("Well is created");
            SeleniumActions.WaitForLoad();
            Thread.Sleep(3000);
            selectWellfromWellList(facilityname);
            SeleniumActions.WaitForLoad();
        }

        public void CreateGLWellFullonBlankDB(ExtentTest test, bool modelchk = false)
        {
            string isruunisats = ConfigurationManager.AppSettings.Get("isRunningATS");
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.configurationTab);
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.wellconfigurationTab);
            SeleniumActions.WaitForLoad();

            #region GenralTab
            EnterwellGeneralData(TestData.WellConfigData.WellTypeGL, "GL");
            uploadmodelfile("GasLift-LFactor1.wflx", "GL");
            string facilityname = PageObjects.WellConfigurationPage.wellnametextbox.GetAttribute("value");
            #endregion
            Enterwellattributes();
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.savebutton);
            SeleniumActions.waitForElement(PageObjects.WellConfigurationPage.Toaseter);
            Toastercheck("GL Well Creation", "Well " + facilityname + " saved successfully.", test);
            test.Info("Well is created");
            SeleniumActions.WaitForLoad();
            //  selectWellfromWellList(facilityname);
        }

        public void createwelltest(ExtentTest test, String welltype)
        {
            if (welltype.Equals("GL"))
            {
                ClickOnNewWellTestCommon();
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.applicabledate);
                SeleniumActions.sendText(PageObjects.WellConfigurationPage.applicabledate, DateTime.Now.AddYears(-2).AddDays(18).ToString("MMddyyyy"), true);
                SeleniumActions.selectKendoDropdownValue(PageObjects.WellDesignPage.txttime, SeleniumAutomation.TestData.GLWellTestData.TestTime, true);
                SeleniumActions.sendText(PageObjects.WellDesignPage.txttestduration, SeleniumAutomation.TestData.GLWellTestData.TestDuration);
                SeleniumActions.selectKendoDropdownValue(PageObjects.WellTestPage.btnqualitycode, TestData.GLWellTestData.QualityCodeUS, true);
                SeleniumActions.sendText(PageObjects.WellTestPage.txtoilrate, SeleniumAutomation.TestData.GLWellTestData.OilRateUS);
                SeleniumActions.sendText(PageObjects.WellTestPage.txtwaterrate, SeleniumAutomation.TestData.GLWellTestData.WaterRateUS);
                SeleniumActions.sendText(PageObjects.WellTestPage.txtwelltestgasrate, SeleniumAutomation.TestData.GLWellTestData.GasRateUS);
                SeleniumActions.sendText(PageObjects.WellTestPage.txtwelltestthp, SeleniumAutomation.TestData.GLWellTestData.TubingHeadPressureUS);
                SeleniumActions.sendText(PageObjects.WellTestPage.txtwelltestthT, SeleniumAutomation.TestData.GLWellTestData.TubingHeadTemperatureUS);
                SeleniumActions.sendText(PageObjects.WellTestPage.txtavgcasingpr, SeleniumAutomation.TestData.GLWellTestData.CasingHeadPressureUS);
                SeleniumActions.sendText(PageObjects.WellTestPage.txtgasinjectionrate, SeleniumAutomation.TestData.GLWellTestData.GasInjectionRateUS);
                SeleniumActions.sendText(PageObjects.WellTestPage.txtDPG, SeleniumAutomation.TestData.GLWellTestData.DPG);
                SeleniumActions.sendText(PageObjects.WellTestPage.txtflowlinepressure, SeleniumAutomation.TestData.GLWellTestData.FlowLinePressureUS);
                SeleniumActions.sendText(PageObjects.WellTestPage.txtseparatorpressure, SeleniumAutomation.TestData.GLWellTestData.SeperatorPressureUS);
                SeleniumActions.sendText(PageObjects.WellTestPage.txtchokesize, SeleniumAutomation.TestData.GLWellTestData.ChokeSize);
                SeleniumActions.waitClick(PageObjects.WellTestPage.btnWellTestSave);
                SeleniumActions.WaitForLoad();


                string[] compdet = {
                    SeleniumActions.get_Attribute(PageObjects.WellTestPage.WellTestDate,"Text"),
                    SeleniumAutomation.TestData.GLWellTestData.TestDuration,
                    SeleniumAutomation.TestData.GLWellTestData.QualityCodeUS,
                    SeleniumAutomation.TestData.GLWellTestData.TuningMethod,
                    SeleniumAutomation.TestData.GLWellTestData.Status,
                    SeleniumAutomation.TestData.GLWellTestData.Message,
                    SeleniumAutomation.TestData.GLWellTestData.TubingHeadPressureUS,
                    SeleniumAutomation.TestData.GLWellTestData.TubingHeadTemperatureUS,
                    SeleniumAutomation.TestData.GLWellTestData.CasingHeadPressureUS,
                    SeleniumAutomation.TestData.GLWellTestData.GasInjectionRateUS,
                    SeleniumAutomation.TestData.GLWellTestData.OilRateUS,
                    SeleniumAutomation.TestData.GLWellTestData.GasRateUS,
                    SeleniumAutomation.TestData.GLWellTestData.FormationGasUS,
                    SeleniumAutomation.TestData.GLWellTestData.WaterRateUS,
                    SeleniumAutomation.TestData.GLWellTestData.WaterCutUS,
                    SeleniumAutomation.TestData.GLWellTestData.InjectionGLRUS,
                    SeleniumAutomation.TestData.GLWellTestData.FlowingBottomHolePressureUS,
                    SeleniumAutomation.TestData.GLWellTestData.LFactor,
                    SeleniumAutomation.TestData.GLWellTestData.ProductivityIndexUS,
                    SeleniumAutomation.TestData.GLWellTestData.ReservoirPressureUS,
                };
                WellTestDataProcess w = new WellTestDataProcess();
                SeleniumAutomation.AGGridScreenDatas.TypeWelltestGrid[] gridvalues = w.FetchdataScreen();

                SeleniumActions.CustomAssertEqual(compdet[0].Trim().ToString(), gridvalues[0].TestDate.ToString(), "Test date failed", "Test date", test);
                SeleniumActions.CustomAssertEqual(compdet[1].Trim().ToString(), gridvalues[0].TestDuration.ToString(), "TestDuration failed", "TestDuration", test);
                SeleniumActions.CustomAssertEqual(compdet[2].Trim().ToString(), gridvalues[0].Qualitycode.ToString(), "Qualitycode failed", "Qualitycode", test);
                SeleniumActions.CustomAssertEqual(compdet[3].Trim().ToString(), gridvalues[0].TuningMethod.ToString(), "TuningMethod failed", "TuningMethod", test);
                SeleniumActions.CustomAssertEqual(compdet[4].Trim().ToString(), gridvalues[0].status.ToString(), "Tuning Status failed", "Tuning Status", test);
                SeleniumActions.CustomAssertEqual(compdet[5].Trim().ToString(), gridvalues[0].Message.ToString(), "Message", "Message", test);
                SeleniumActions.CustomAssertEqual(compdet[6].Trim().ToString(), gridvalues[0].THP.ToString(), "Tubing head pressure failed", "Tubing head pressure", test);
                SeleniumActions.CustomAssertEqual(compdet[7].Trim().ToString(), gridvalues[0].THT.ToString(), "Tubing head Temperature failed", "Tubing head Temperature", test);
                SeleniumActions.CustomAssertEqual(compdet[8].Trim().ToString(), gridvalues[0].CHP.ToString(), "Casing head pressure failed", "Casing head pressure", test);
                SeleniumActions.CustomAssertEqual(compdet[9].Trim().ToString(), gridvalues[0].GasInjectionRate.ToString(), "Gas Injection Rate failed", "Gas Injection Rate", test);
                SeleniumActions.CustomAssertEqual(compdet[10].Trim().ToString(), gridvalues[0].OilRate.ToString(), "OilRate failed ", "OilRate", test);
                SeleniumActions.CustomAssertEqual(compdet[11].Trim().ToString(), gridvalues[0].FormationGasRate.ToString(), "Formation Gas Rate failed", "Formation Gas Rate", test);
                SeleniumActions.CustomAssertEqual(compdet[12].Trim().ToString(), gridvalues[0].GasRate.ToString(), "Gas Rate failed", "Gas Rate", test);
                SeleniumActions.CustomAssertEqual(compdet[13].Trim().ToString(), gridvalues[0].WaterRate.ToString(), "Water Rate failed", "Water Rate", test);
                SeleniumActions.CustomAssertEqual(compdet[14].Trim().ToString(), gridvalues[0].WaterCut.ToString(), "Water Cut failed", "Water Cut", test);
                SeleniumActions.CustomAssertEqual(compdet[15].Trim().ToString(), gridvalues[0].InjectionGLR.ToString(), "Injection GLR failed", "InjectionG LR", test);
                //Assert.AreEqual(compdet[16].Trim().ToString(), gridvalues[0].FBHP.ToString());
                SeleniumActions.CustomAssertEqual(compdet[17].Trim().ToString(), gridvalues[0].LFactor.ToString(), "LFactor failed", "LFactor", test);
                SeleniumActions.CustomAssertEqual(compdet[18].Trim().ToString(), gridvalues[0].ProductivityIndex.ToString(), "Productivity Index failed", "Productivity Index", test);
                SeleniumActions.CustomAssertEqual(compdet[19].Trim().ToString(), gridvalues[0].ReservoirPressure.ToString(), "Reservoir Pressure failed", "ReservoirPressure", test);

            }
            else if (welltype.Equals("ESP"))
            {
                // FilesLocation = ConfigurationManager.AppSettings.Get("FilesLocation");
                //String f = Path.Combine(FilesLocation + @"\UIAutomation\Smoke_TestData\WellTestData.xlsx");
                ClickOnNewWellTestCommon();
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.applicabledate);
                SeleniumActions.sendText(PageObjects.WellConfigurationPage.applicabledate, DateTime.Now.AddYears(-2).AddDays(18).ToString("MMddyyyy"), true);
                SeleniumActions.selectKendoDropdownValue(PageObjects.WellDesignPage.txttime, TestData.ESPWellTestData.TestTimeInPut, true);
                SeleniumActions.sendText(PageObjects.WellDesignPage.txttestduration, TestData.ESPWellTestData.TestDuration);
                SeleniumActions.selectKendoDropdownValue(PageObjects.WellTestPage.btnqualitycode, TestData.ESPWellTestData.QualityCodeUS, true);
                SeleniumActions.sendText(PageObjects.WellTestPage.txtoilrate, TestData.ESPWellTestData.OilRateUS);
                SeleniumActions.sendText(PageObjects.WellTestPage.txtwaterrate, TestData.ESPWellTestData.WaterRateUS);
                SeleniumActions.sendText(PageObjects.WellTestPage.txtwelltestgasrate, TestData.ESPWellTestData.GasRateUS);
                SeleniumActions.sendText(PageObjects.WellTestPage.txtwelltestthp, TestData.ESPWellTestData.TubingHeadPressureUS);
                SeleniumActions.sendText(PageObjects.WellTestPage.txtwelltestthT, TestData.ESPWellTestData.TubingHeadTemperatureUS);
                SeleniumActions.sendText(PageObjects.WellTestPage.Frequency, TestData.ESPWellTestData.Frequency);
                SeleniumActions.sendText(PageObjects.WellTestPage.PIP, TestData.ESPWellTestData.PumpIntakePressureUS);
                SeleniumActions.sendText(PageObjects.WellTestPage.PDP, TestData.ESPWellTestData.PumpDischargePressureUS);
                SeleniumActions.sendText(PageObjects.WellTestPage.Motorvolts, TestData.ESPWellTestData.MotorVolts);
                SeleniumActions.sendText(PageObjects.WellTestPage.Motoramps, TestData.ESPWellTestData.MotorAmps);
                SeleniumActions.sendText(PageObjects.WellTestPage.txtavgcasingpr, TestData.ESPWellTestData.CasingHeadPressureUS);
                SeleniumActions.sendText(PageObjects.WellTestPage.Seperatorpr, TestData.ESPWellTestData.SeperatorPressureUS);
                SeleniumActions.sendText(PageObjects.WellTestPage.txtflowlinepressure, TestData.ESPWellTestData.FlowLinePressureUS);
                SeleniumActions.sendText(PageObjects.WellTestPage.txtchokesize, TestData.ESPWellTestData.ChokeSize);
                SeleniumActions.waitClick(PageObjects.WellTestPage.btnWellTestSave);
                SeleniumActions.WaitForLoad();
                string[] compdet = {
                    DateTime.Now.AddYears(-2).AddDays(18).ToString("MM/dd/yyyy")+" "+TestData.ESPWellTestData.TestTime,
                    TestData.ESPWellTestData.TestDuration,
                    TestData.ESPWellTestData.QualityCodeUS,
                    TestData.ESPWellTestData.Status,
                    TestData.ESPWellTestData.Message,
                    TestData.ESPWellTestData.TubingHeadPressureUS,
                    TestData.ESPWellTestData.TubingHeadTemperatureUS,
                    TestData.ESPWellTestData.CasingHeadPressureUS,
                    TestData.ESPWellTestData.OilRateUS,
                    TestData.ESPWellTestData.WaterRateUS,
                    TestData.ESPWellTestData.GasRateUS,
                    TestData.ESPWellTestData.Frequency,
                    TestData.ESPWellTestData.MotorVolts,
                    TestData.ESPWellTestData.MotorAmps,
                    TestData.ESPWellTestData.FlowLinePressureUS,
                    TestData.ESPWellTestData.SeperatorPressureUS,
                    TestData.ESPWellTestData.ChokeSize,
                    TestData.ESPWellTestData.GORUS,
                    TestData.ESPWellTestData.WaterCutUS,
                    TestData.ESPWellTestData.FBHPUS,
                    TestData.ESPWellTestData.LFactor,
                    TestData.ESPWellTestData.ProductivityIndexUS,
                    TestData.ESPWellTestData.ReservoirPressureUS,
                    TestData.ESPWellTestData.HeadTuningFactorUS,

                };
                WellTestDataProcess w = new WellTestDataProcess();
                SeleniumAutomation.AGGridScreenDatas.TypeWelltestGrid[] gridvalues = w.FetchESPWelltestdataScreen();
                SeleniumActions.CustomAssertEqual(compdet[0].Trim().ToString(), gridvalues[0].TestDate.ToString(), "Test date failed", "Test date", test);
                SeleniumActions.CustomAssertEqual(compdet[1].Trim().ToString(), gridvalues[0].TestDuration.ToString(), "Test Duration Failed", "Test Duration", test);
                SeleniumActions.CustomAssertEqual(compdet[2].Trim().ToString(), gridvalues[0].Qualitycode.ToString(), "Quality Code failed", "Quality Code", test);
                SeleniumActions.CustomAssertEqual(compdet[3].Trim().ToString(), gridvalues[0].status.ToString(), "Status Failed", "Status", test);
                Assert.AreEqual(compdet[4].Trim().ToString(), gridvalues[0].Message.ToString(), "Message failed");
                SeleniumActions.CustomAssertEqual(compdet[5].Trim().ToString(), gridvalues[0].THP.ToString(), "Tubing head pressure failed", "Tubing head pressure", test);
                SeleniumActions.CustomAssertEqual(compdet[6].Trim().ToString(), gridvalues[0].THT.ToString(), "Tubing head Temperature failed", "Tubing head Temperature ", test);
                SeleniumActions.CustomAssertEqual(compdet[7].Trim().ToString(), gridvalues[0].CHP.ToString(), "Casing head pressure failed", "Casing head pressure ", test);
                SeleniumActions.CustomAssertEqual(compdet[8].Trim().ToString(), gridvalues[0].OilRate.ToString(), "Oil rate failed", "Oil rate", test);
                SeleniumActions.CustomAssertEqual(compdet[9].Trim().ToString(), gridvalues[0].WaterRate.ToString(), "WaterRate failed", "WaterRate", test);
                SeleniumActions.CustomAssertEqual(compdet[10].Trim().ToString(), gridvalues[0].GasRate.ToString(), "GasRate failed", "GasRate", test);
                SeleniumActions.CustomAssertEqual(compdet[11].Trim().ToString(), gridvalues[0].Frequency.ToString(), "Frequency failed", "Frequency", test);
                SeleniumActions.CustomAssertEqual(compdet[12].Trim().ToString(), gridvalues[0].MotorVolts.ToString(), "MotorVolts failed", "MotorVolts", test);
                SeleniumActions.CustomAssertEqual(compdet[13].Trim().ToString(), gridvalues[0].MotorAmps.ToString(), "MotorAmps failed", "MotorAmps", test);
                SeleniumActions.CustomAssertEqual(compdet[14].Trim().ToString(), gridvalues[0].Flowlinepr.ToString(), "Flowlinepr failed", "Flowlinepr", test);
                SeleniumActions.CustomAssertEqual(compdet[15].Trim().ToString(), gridvalues[0].Seppr.ToString(), "Seppr failed", "Seppr", test);
                SeleniumActions.CustomAssertEqual(compdet[16].Trim().ToString(), gridvalues[0].ChokeSize.ToString(), "ChokeSize failed", "ChokeSize", test);
                SeleniumActions.CustomAssertEqual(compdet[17].Trim().ToString(), gridvalues[0].FormationGOR.ToString(), "FormationGOR failed", "FormationGOR", test);
                SeleniumActions.CustomAssertEqual(compdet[18].Trim().ToString(), gridvalues[0].WaterCut.ToString(), "WaterCut failed", "WaterCut", test);
                SeleniumActions.CustomAssertEqual(compdet[19].Trim().ToString(), gridvalues[0].FBHP.ToString(), "FBHP failed", "FBHP", test);
                SeleniumActions.CustomAssertEqual(compdet[20].Trim().ToString(), gridvalues[0].LFactor.ToString(), "LFactor failed", "LFactor", test);
                SeleniumActions.CustomAssertEqual(compdet[21].Trim().ToString(), gridvalues[0].ProductivityIndex.ToString(), "ProductivityIndex failed", "ProductivityIndex", test);
                SeleniumActions.CustomAssertEqual(compdet[22].Trim().ToString(), gridvalues[0].ReservoirPressure.ToString(), "ReservoirPressure failed", "ReservoirPressure", test);
                SeleniumActions.CustomAssertEqual(compdet[23].Trim().ToString(), gridvalues[0].HeadTuningFactor.ToString(), "HeadTuningFactor failed", "HeadTuningFactor", test);


            }
            SeleniumActions.waitClick(PageObjects.WellAnalysisPage.tabanalysis);



        }

        public void CreateGInjWellFullonBlankDB(ExtentTest test, bool modelchk = false)
        {
            string isruunisats = ConfigurationManager.AppSettings.Get("isRunningATS");
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.configurationTab);
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.wellconfigurationTab);
            SeleniumActions.WaitForLoad();

            #region GenralTab
            EnterwellGeneralData(TestData.WellConfigData.WellTypeGInj, "GASINJWELL");
            uploadmodelfile("GASINJWELL.wflx", "GInj");
            string facilityname = PageObjects.WellConfigurationPage.wellnametextbox.GetAttribute("value");
            #endregion
            Enterwellattributes();
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.savebutton);
            SeleniumActions.waitForElement(PageObjects.WellConfigurationPage.Toaseter);
            Toastercheck("Gas Injection Well Creation", "Well " + facilityname + " saved successfully.", test);
            test.Info("Well is created");
            SeleniumActions.WaitForLoad();
            selectWellfromWellList(facilityname);
            SeleniumActions.WaitForLoad();
        }
        public void CreateWInjWellFullonBlankDB(ExtentTest test, bool modelchk = false)
        {
            string isruunisats = ConfigurationManager.AppSettings.Get("isRunningATS");
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.configurationTab);
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.wellconfigurationTab);
            SeleniumActions.WaitForLoad();

            #region GenralTab
            EnterwellGeneralData(TestData.WellConfigData.WellTypeWInj, "IWC");
            string facilityname = PageObjects.WellConfigurationPage.wellnametextbox.GetAttribute("value");
            #endregion
            Enterwellattributes();
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.savebutton);
            SeleniumActions.waitForElement(PageObjects.WellConfigurationPage.Toaseter);
            Toastercheck("WAG Injection Well Creation", "Well " + facilityname + " saved successfully.", test);
            test.Info("Well is created");
        }
        public void CreatePGLWellFullonBlankDB(ExtentTest test, bool modelchk = false)
        {
            string isruunisats = ConfigurationManager.AppSettings.Get("isRunningATS");
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.configurationTab);
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.wellconfigurationTab);
            SeleniumActions.WaitForLoad();

            #region GenralTab

            EnterwellGeneralData(TestData.WellConfigData.WellTypePGL, "PGLWELL");
            uploadmodelfile("PL-631.wflx", "PGL");
            string facilityname = PageObjects.WellConfigurationPage.wellnametextbox.GetAttribute("value");
            #endregion
            Enterwellattributes();
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.savebutton);
            SeleniumActions.waitForElement(PageObjects.WellConfigurationPage.Toaseter);
            Toastercheck("PGL Well Creation", "Well " + facilityname + " saved successfully.", test);
            test.Info("Well is created");
            selectWellfromWellList(facilityname);
            SeleniumActions.WaitForLoad();
        }
        public void CreateWaterInjWellFullonBlankDB(ExtentTest test, bool modelchk = false)
        {
            string isruunisats = ConfigurationManager.AppSettings.Get("isRunningATS");
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.configurationTab);
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.wellconfigurationTab);
            SeleniumActions.WaitForLoad();

            #region GenralTab
            EnterwellGeneralData(TestData.WellConfigData.WellTypeWaterInj, "WATERINJ");
            uploadmodelfile("WATERINJWELL.wflx", "WInj");
            string facilityname = PageObjects.WellConfigurationPage.wellnametextbox.GetAttribute("value");
            #endregion
            Enterwellattributes();
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.savebutton);
            SeleniumActions.waitForElement(PageObjects.WellConfigurationPage.Toaseter);
            Toastercheck("Water Injection Well Creation", "Well " + facilityname + " saved successfully.", test);
            test.Info("Well is created");
            selectWellfromWellList(facilityname);
            SeleniumActions.WaitForLoad();
        }

        public void CreateOtherTypeWellFullonBlankDB(ExtentTest test, bool modelchk = false)
        {
            string isruunisats = ConfigurationManager.AppSettings.Get("isRunningATS");
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.configurationTab);
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.wellconfigurationTab);
            SeleniumActions.WaitForLoad();
            #region GeneralTab
            EnterwellGeneralData(TestData.WellConfigData.WellTypeOtherType, "ESP");
            string facilityname = PageObjects.WellConfigurationPage.wellnametextbox.GetAttribute("value");
            #endregion
            Enterwellattributes();
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.savebutton);
            SeleniumActions.waitForElement(PageObjects.WellConfigurationPage.Toaseter);
            Toastercheck("OT Well Creation", "Well " + facilityname + " saved successfully.", test);
            test.Info("Well is created");
            selectWellfromWellList(facilityname);
        }
        public void Enterwellattributes()
        {
            //Well Attribute tab
            #region WellAttribute
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.wellattributetab);
            SeleniumActions.sendText(PageObjects.WellConfigurationPage.leasename, "lease");
            SeleniumActions.sendText(PageObjects.WellConfigurationPage.regionname, "region");
            SeleniumActions.sendText(PageObjects.WellConfigurationPage.fieldname, "field");
            SeleniumActions.sendText(PageObjects.WellConfigurationPage.foremanname, "foreman");
            SeleniumActions.sendText(PageObjects.WellConfigurationPage.engineername, "engineer");
            SeleniumActions.sendText(PageObjects.WellConfigurationPage.gaugername, "gauger");

            #endregion
        }

        public void uploadmodelfile(string modelfile, string welltype)
        {
            string path = Path.Combine(FilesLocation + @"\UIAutomation\Smoke_TestData", modelfile);
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.btnmodelfiledate);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.selectmodelfile);
            SeleniumActions.FileUploadDialog(path);
            SeleniumActions.WaitForLoad();
            SeleniumActions.sendText(PageObjects.WellConfigurationPage.applicabledate, DateTime.Now.AddYears(-2).ToString("MMddyyyy"), true);
            SeleniumActions.waitForElement(PageObjects.WellConfigurationPage.modelcomment);
            SeleniumActions.sendText(PageObjects.WellConfigurationPage.modelcomment, "Test");
            Thread.Sleep(2);
            SeleniumActions.WaitForLoad();
            Thread.Sleep(5);
            if (!welltype.Equals("PGL"))
            {
                SeleniumActions.selectKendoDropdownValue(PageObjects.WellConfigurationPage.tuningmethod, "L Factor");
            }
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.modelapply);

        }

        public void Verifydata(DataTable d, string fieldNameText, string searchby, string unit)
        {

            int i = 0;
            foreach (DataRow drow in d.Rows)
            {
                string fieldName = d.Rows[i]["FieldName"].ToString();
                if (fieldName == fieldNameText)
                {
                    IWebElement ele;
                    ele = SeleniumActions.getWebelementNextSibling("XPath", searchby, "parent ele")[0];
                    IWebElement elenext;
                    elenext = SeleniumActions.getWebelementNextSibling("XPath", searchby, "parent ele")[1];
                    Assert.AreEqual(d.Rows[i]["Value" + unit].ToString(), ele.GetAttribute("value").ToString(), "Incorrect value is displayed for field " + fieldNameText);
                    if (d.Rows[i]["UOM" + unit].ToString() != "ignoreUnit")
                        Assert.AreEqual(d.Rows[i]["UOM" + unit].ToString(), elenext.Text.ToString(), "Incorrect unit label is displayed for field " + fieldNameText);
                    CommonHelper.TraceLine("Validation completed for " + fieldNameText + " field");
                    break;

                }

                i++;
            }


        }



        public void checkFluidModelData(string unit, string welltype)
        {

            SeleniumActions.waitforPageloadComplete();
            switch (welltype)
            {
                case "ESP":

                    if (unit.Equals("Metric"))
                    {
                        CommonHelper.TraceLine("------Validating Fluid Data-------- in unit system " + unit);
                        Verifydata(TestData.ESPGLModelData.dt, "Oil Specific Gravity", lbloilspecificgravity, "Metric");
                        Verifydata(TestData.ESPGLModelData.dt, "Gas Specific Gravity", lblgasspecificgravity, "Metric");
                        Verifydata(TestData.ESPGLModelData.dt, "Water Salinity", lblWaterSalinity, "Metric");
                        Verifydata(TestData.ESPGLModelData.dt, "Hydrogen Sulfide (H2S)", lblHydrogenSulfide, "Metric");
                        Verifydata(TestData.ESPGLModelData.dt, "Carbon Dioxide (C02)", lblCarbonDioxide, "Metric");
                        Verifydata(TestData.ESPGLModelData.dt, "Nitrogen (N2)", lblNitrogen, "Metric");

                    }
                    else if (unit.Equals("US"))
                    {
                        CommonHelper.TraceLine("------Validating Fluid Data-------- in unit system " + unit);
                        Verifydata(TestData.ESPGLModelData.dt, "Oil Specific Gravity", lbloilspecificgravity, "US");
                        Verifydata(TestData.ESPGLModelData.dt, "Gas Specific Gravity", lblgasspecificgravity, "US");
                        Verifydata(TestData.ESPGLModelData.dt, "Water Salinity", lblWaterSalinity, "US");
                        Verifydata(TestData.ESPGLModelData.dt, "Hydrogen Sulfide (H2S)", lblHydrogenSulfide, "US");
                        Verifydata(TestData.ESPGLModelData.dt, "Carbon Dioxide (C02)", lblCarbonDioxide, "US");
                        Verifydata(TestData.ESPGLModelData.dt, "Nitrogen (N2)", lblNitrogen, "US");

                    }
                    break;
                case "NF":
                    if (unit.Equals("Metric"))
                    {
                        CommonHelper.TraceLine("------Validating Fluid Data-------- in unit system " + unit);
                        Verifydata(TestData.NFWModelData.dt, "Oil Specific Gravity", lbloilspecificgravity, "Metric");
                        Verifydata(TestData.NFWModelData.dt, "Gas Specific Gravity", lblgasspecificgravity, "Metric");
                        Verifydata(TestData.NFWModelData.dt, "Water Salinity", lblWaterSalinity, "Metric");
                        Verifydata(TestData.NFWModelData.dt, "Hydrogen Sulfide (H2S)", lblHydrogenSulfide, "Metric");
                        Verifydata(TestData.NFWModelData.dt, "Carbon Dioxide (C02)", lblCarbonDioxide, "Metric");
                        Verifydata(TestData.NFWModelData.dt, "Nitrogen (N2)", lblNitrogen, "Metric");

                    }
                    else if (unit.Equals("US"))
                    {
                        CommonHelper.TraceLine("------Validating Fluid Data-------- in unit system " + unit);
                        Verifydata(TestData.NFWModelData.dt, "Oil Specific Gravity", lbloilspecificgravity, "US");
                        Verifydata(TestData.NFWModelData.dt, "Gas Specific Gravity", lblgasspecificgravity, "US");
                        Verifydata(TestData.NFWModelData.dt, "Water Salinity", lblWaterSalinity, "US");
                        Verifydata(TestData.NFWModelData.dt, "Hydrogen Sulfide (H2S)", lblHydrogenSulfide, "US");
                        Verifydata(TestData.NFWModelData.dt, "Carbon Dioxide (C02)", lblCarbonDioxide, "US");
                        Verifydata(TestData.NFWModelData.dt, "Nitrogen (N2)", lblNitrogen, "US");

                    }
                    break;
                case "GL":
                    if (unit.Equals("Metric"))
                    {
                        CommonHelper.TraceLine("------Validating Fluid Data-------- in unit system " + unit);
                        Verifydata(TestData.GLModelData.dt, "Oil Specific Gravity", lbloilspecificgravity, "Metric");
                        Verifydata(TestData.GLModelData.dt, "Gas Specific Gravity", lblgasspecificgravity, "Metric");
                        Verifydata(TestData.GLModelData.dt, "Water Salinity", lblWaterSalinity, "Metric");
                        Verifydata(TestData.GLModelData.dt, "Hydrogen Sulfide (H2S)", lblHydrogenSulfide, "Metric");
                        Verifydata(TestData.GLModelData.dt, "Carbon Dioxide (C02)", lblCarbonDioxide, "Metric");
                        Verifydata(TestData.GLModelData.dt, "Nitrogen (N2)", lblNitrogen, "Metric");

                    }
                    else if (unit.Equals("US"))
                    {
                        CommonHelper.TraceLine("------Validating Fluid Data-------- in unit system " + unit);
                        Verifydata(TestData.GLModelData.dt, "Oil Specific Gravity", lbloilspecificgravity, "US");
                        Verifydata(TestData.GLModelData.dt, "Gas Specific Gravity", lblgasspecificgravity, "US");
                        Verifydata(TestData.GLModelData.dt, "Water Salinity", lblWaterSalinity, "US");
                        Verifydata(TestData.GLModelData.dt, "Hydrogen Sulfide (H2S)", lblHydrogenSulfide, "US");
                        Verifydata(TestData.GLModelData.dt, "Carbon Dioxide (C02)", lblCarbonDioxide, "US");
                        Verifydata(TestData.GLModelData.dt, "Nitrogen (N2)", lblNitrogen, "US");

                    }
                    break;
            }
        }
        public void checkReservoirModelData(string unit, string welltype)
        {
            switch (welltype)
            {
                case "ESP":
                    #region ESP
                    if (unit.Equals("Metric"))
                    {
                        CommonHelper.TraceLine("------Validating Reservoir Data-------- in unit system " + unit);
                        Verifydata(TestData.ESPGLModelData.dt, "Pressure", lblpressure, "Metric");
                        Verifydata(TestData.ESPGLModelData.dt, "Temperature", lbltemperature, "Metric");
                        Verifydata(TestData.ESPGLModelData.dt, "MidPerf Depth (MD)", lblmidperfdepth, "Metric");
                        Verifydata(TestData.ESPGLModelData.dt, "Water Cut", lblwatercut, "Metric");
                        Verifydata(TestData.ESPGLModelData.dt, "Gas-Oil Ratio (GOR)", lblgor, "Metric");
                        Verifydata(TestData.ESPGLModelData.dt, "Productivity Index-ESP", lblproductivityindex, "Metric");
                    }
                    else if (unit.Equals("US"))
                    {
                        CommonHelper.TraceLine("------Validating Reservoir Data-------- in unit system " + unit);
                        Verifydata(TestData.ESPGLModelData.dt, "Pressure", lblpressure, "US");
                        Verifydata(TestData.ESPGLModelData.dt, "Temperature", lbltemperature, "US");
                        Verifydata(TestData.ESPGLModelData.dt, "MidPerf Depth (MD)", lblmidperfdepth, "US");
                        Verifydata(TestData.ESPGLModelData.dt, "Water Cut", lblwatercut, "US");
                        Verifydata(TestData.ESPGLModelData.dt, "Gas-Oil Ratio (GOR)", lblgor, "US");
                        Verifydata(TestData.ESPGLModelData.dt, "Productivity Index-ESP", lblproductivityindex, "US");
                    }
                    break;
                #endregion
                case "NF":
                    #region NF
                    if (unit.Equals("Metric"))
                    {
                        CommonHelper.TraceLine("------Validating Reservoir Data-------- in unit system " + unit);
                        Verifydata(TestData.NFWModelData.dt, "Pressure", lblpressure, "Metric");
                        Verifydata(TestData.NFWModelData.dt, "Temperature", lbltemperature, "Metric");
                        Verifydata(TestData.NFWModelData.dt, "MidPerf Depth (MD)", lblmidperfdepth, "Metric");
                        Verifydata(TestData.NFWModelData.dt, "Water Cut", lblwatercut, "Metric");
                        Verifydata(TestData.NFWModelData.dt, "Gas-Oil Ratio (GOR)", lblgor, "Metric");
                        Verifydata(TestData.NFWModelData.dt, "Productivity Index-NFW", lblproductivityindex, "Metric");
                    }
                    else if (unit.Equals("US"))
                    {
                        CommonHelper.TraceLine("------Validating Reservoir Data-------- in unit system " + unit);
                        Verifydata(TestData.NFWModelData.dt, "Pressure", lblpressure, "US");
                        Verifydata(TestData.NFWModelData.dt, "Temperature", lbltemperature, "US");
                        Verifydata(TestData.NFWModelData.dt, "MidPerf Depth (MD)", lblmidperfdepth, "US");
                        Verifydata(TestData.NFWModelData.dt, "Water Cut", lblwatercut, "US");
                        Verifydata(TestData.NFWModelData.dt, "Gas-Oil Ratio (GOR)", lblgor, "US");
                        Verifydata(TestData.NFWModelData.dt, "Productivity Index-NFW", lblproductivityindex, "US");
                    }
                    break;
                #endregion
                case "GL":
                    #region GL
                    if (unit.Equals("Metric"))
                    {
                        CommonHelper.TraceLine("------Validating Reservoir Data-------- in unit system " + unit);
                        Verifydata(TestData.GLModelData.dt, "Pressure", lblpressure, "Metric");
                        Verifydata(TestData.GLModelData.dt, "Temperature", lbltemperature, "Metric");
                        Verifydata(TestData.GLModelData.dt, "MidPerf Depth (MD)", lblmidperfdepth, "Metric");
                        Verifydata(TestData.GLModelData.dt, "Water Cut", lblwatercut, "Metric");
                        Verifydata(TestData.GLModelData.dt, "Gas-Oil Ratio (GOR)", lblgor, "Metric");
                        Verifydata(TestData.GLModelData.dt, "Productivity Index-NFW", lblproductivityindex, "Metric");
                    }
                    else if (unit.Equals("US"))
                    {
                        CommonHelper.TraceLine("------Validating Reservoir Data-------- in unit system " + unit);
                        Verifydata(TestData.GLModelData.dt, "Pressure", lblpressure, "US");
                        Verifydata(TestData.GLModelData.dt, "Temperature", lbltemperature, "US");
                        Verifydata(TestData.GLModelData.dt, "MidPerf Depth (MD)", lblmidperfdepth, "US");
                        Verifydata(TestData.GLModelData.dt, "Water Cut", lblwatercut, "US");
                        Verifydata(TestData.GLModelData.dt, "Gas-Oil Ratio (GOR)", lblgor, "US");
                        Verifydata(TestData.GLModelData.dt, "Productivity Index-GL", lblproductivityindex, "US");
                    }
                    break;
                    #endregion
            }
        }
        public void checkMotor_Cable_SeparatorData(string unit)
        {
            if (unit.Equals("Metric"))
            {
                CommonHelper.TraceLine("------Validating Motor,Cable and Seperator Data-------- in unit system " + unit);
                Verifydata(TestData.ESPGLModelData.dt, "Motor Model", lblmotormodel, "Metric");
                Verifydata(TestData.ESPGLModelData.dt, "Measured Depth", lblmeasureddepth, "Metric");
                Verifydata(TestData.ESPGLModelData.dt, "Nameplate Rating", lblnameplaterating, "Metric");
                Verifydata(TestData.ESPGLModelData.dt, "Operating Rating", lbloperatingrating, "Metric");
                Verifydata(TestData.ESPGLModelData.dt, "Operating Frequency", lbloperatingfrequency, "Metric");
                Verifydata(TestData.ESPGLModelData.dt, "Motor Wear Factor", lblmotorwearfactor, "Metric");
                Verifydata(TestData.ESPGLModelData.dt, "Cable Size", lblcablesize, "Metric");
                Verifydata(TestData.ESPGLModelData.dt, "Gas Separator Present", lblgasseparatorpresent, "Metric");
                Verifydata(TestData.ESPGLModelData.dt, "Separator Efficiency", lblseparatorefficiency, "Metric");
            }
            else if (unit.Equals("US"))
            {
                CommonHelper.TraceLine("------Validating Motor,Cable and Seperator Data-------- in unit system " + unit);
                Verifydata(TestData.ESPGLModelData.dt, "Motor Model", lblmotormodel, "US");
                Verifydata(TestData.ESPGLModelData.dt, "Measured Depth", lblmeasureddepth, "US");
                Verifydata(TestData.ESPGLModelData.dt, "Nameplate Rating", lblnameplaterating, "US");
                Verifydata(TestData.ESPGLModelData.dt, "Operating Rating", lbloperatingrating, "US");
                Verifydata(TestData.ESPGLModelData.dt, "Operating Frequency", lbloperatingfrequency, "US");
                Verifydata(TestData.ESPGLModelData.dt, "Motor Wear Factor", lblmotorwearfactor, "US");
                Verifydata(TestData.ESPGLModelData.dt, "Cable Size", lblcablesize, "US");
                Verifydata(TestData.ESPGLModelData.dt, "Gas Separator Present", lblgasseparatorpresent, "US");
                Verifydata(TestData.ESPGLModelData.dt, "Separator Efficiency", lblseparatorefficiency, "US");
            }
        }
        public void checkPlungerLiftData(string unit)
        {
            if (unit.Equals("Metric"))
            {
                CommonHelper.TraceLine("------Validating Plunger Lift Data-------- in unit system " + unit);
                Verifydata(TestData.PGLModelData.dt, "Bottom Hole Assembly(BHA) Depth(MD)", lblbottomhole, "Metric");
                Assert.AreEqual(SeleniumActions.getEnabledState(PageObjects.WellConfigurationPage.rdbtnNone), false, "None button is not disabled");
                Assert.AreEqual(SeleniumActions.getEnabledState(PageObjects.WellConfigurationPage.rdbtnSand), false, "Sand button is not disabled");
                Assert.AreEqual(SeleniumActions.getEnabledState(PageObjects.WellConfigurationPage.rdbtnWax), false, "Wax button is not disabled");
                Verifydata(TestData.PGLModelData.dt, "Plunger Type", lblplungertype, "Metric");
                Verifydata(TestData.PGLModelData.dt, "Estimated Fall Rate in Gas", lblfallingas, "Metric");
                Verifydata(TestData.PGLModelData.dt, "Estimated Fall Rate in Liquid", lblfallinliquid, "Metric");
                Verifydata(TestData.PGLModelData.dt, "Ideal Rise Rate", lblidealriserate, "Metric");
                Verifydata(TestData.PGLModelData.dt, "Pressure Required to surface Plunger", lblpressurereq, "Metric");

            }
            else if (unit.Equals("US"))
            {
                CommonHelper.TraceLine("------Validating Plunger Lift Data-------- in unit system " + unit);
                Verifydata(TestData.PGLModelData.dt, "Bottom Hole Assembly(BHA) Depth(MD)", lblbottomhole, "US");
                Assert.AreEqual(SeleniumActions.getEnabledState(PageObjects.WellConfigurationPage.rdbtnNone), false, "None button is not disabled");
                Assert.AreEqual(SeleniumActions.getEnabledState(PageObjects.WellConfigurationPage.rdbtnSand), false, "Sand button is not disabled");
                Assert.AreEqual(SeleniumActions.getEnabledState(PageObjects.WellConfigurationPage.rdbtnWax), false, "Wax button is not disabled");
                Verifydata(TestData.PGLModelData.dt, "Plunger Type", lblplungertype, "US");
                Verifydata(TestData.PGLModelData.dt, "Estimated Fall Rate in Gas", lblfallingas, "US");
                Verifydata(TestData.PGLModelData.dt, "Estimated Fall Rate in Liquid", lblfallinliquid, "US");
                Verifydata(TestData.PGLModelData.dt, "Ideal Rise Rate", lblidealriserate, "US");
                Verifydata(TestData.PGLModelData.dt, "Pressure Required to surface Plunger", lblpressurereq, "US");
            }
        }

        public static void VerifyTrajectoryData(DataTable d, string unit, string locator, string searchby, ExtentTest test)
        {
            CommonHelper.TraceLine("------Validating Trajectory Data-------- in unit system " + unit);
            string colnames = "";
            List<string> colvals = new List<string>();
            string colnamesUS = "Measured Depthft;True Vertical Depthft;Deviation Angledeg";
            string colnamesMetric = "Measured Depthm;True Vertical Depthm;Deviation Angledeg";
            string action = "ignorevalue";
            for (int rowindex = 0; rowindex < d.Rows.Count; rowindex++)
            {
                string measuredDepth = d.Rows[rowindex]["MeasuredDepth" + unit].ToString();
                string trueVerticalDepth = d.Rows[rowindex]["TrueVerticalDepth" + unit].ToString();
                string deviationAngle = d.Rows[rowindex]["DeviationAngle" + unit].ToString();

                colvals.Add(action + ";" + (rowindex + 1) + ";" + measuredDepth + ";" + trueVerticalDepth + ";" + deviationAngle);

                if (unit == "US")
                    colnames = colnamesUS;
                else
                    colnames = colnamesMetric;

                SeleniumActions.verifytablelabels(colnames, locator, searchby, unit);
                for (int i = 0; i < colvals.Count; i++)
                {
                    SeleniumActions.verifymodelgriddata(tbltrajectorygrid, "xpath", colvals[i], "", i + 1, unit, test);
                }

            }


        }

        public static void VerifyTubingCasingData(DataTable d, string unit, string locator, string searchby, string grid, ExtentTest test)
        {
            CommonHelper.TraceLine("------Validating Tubing and Casing Data-------- in unit system " + unit);
            string colnames = "";
            List<string> colvals = new List<string>();

            string colnamesUS = "Name;Start Point Measured Depthft;End Point Measured Depthft;External Diameterin;Internal Diameterin;Roughnessin";
            string colnamesMetric = "Name;Start Point Measured Depthm;End Point Measured Depthm;External Diametermm;Internal Diametermm;Roughnessmm";
            string action = "ignorevalue";
            if (unit == "US")
                colnames = colnamesUS;
            else
                colnames = colnamesMetric;

            SeleniumActions.verifytablelabels(colnames, locator, searchby, unit);
            for (int rowindex = 0; rowindex < d.Rows.Count; rowindex++)
            {
                string name = d.Rows[rowindex]["Name"].ToString();
                string startPointMeasure = d.Rows[rowindex]["StartPointMeasure" + unit].ToString();
                string endPointMeasure = d.Rows[rowindex]["EndPointMeasure" + unit].ToString();
                string externalDiameter = d.Rows[rowindex]["ExternalDiameter" + unit].ToString();
                string internalDiameter = d.Rows[rowindex]["InternalDiameter" + unit].ToString();
                string roughness = d.Rows[rowindex]["Roughness" + unit].ToString();

                colvals.Add(action + ";" + (rowindex + 1) + ";" + name + ";" + startPointMeasure + ";" + endPointMeasure + ";" + externalDiameter + ";" + internalDiameter + ";" + roughness);



            }
            if (grid.Equals(tbltubinggrid))
                for (int i = 0; i < colvals.Count; i++)
                {
                    SeleniumActions.verifymodelgriddata(tbltubinggrid, "xpath", colvals[i], "", i + 1, unit, test);
                }
            else if (grid.Equals(tblcasinggrid))
                for (int i = 0; i < colvals.Count; i++)
                {
                    SeleniumActions.verifymodelgriddata(tblcasinggrid, "xpath", colvals[i], "", i + 1, unit, test);
                }
        }

        public static void VerifyGasLiftData(DataTable d, string unit, string locator, string searchby, string grid, ExtentTest test)
        {
            CommonHelper.TraceLine("------Validating Gas Lift Data-------- in unit system " + unit);
            string colnames = "";
            List<string> colvals = new List<string>();

            string colnamesUS = "Active;Measured Depthft;True Vertical Depthft;Manufacturer;Model;Port Size1/64in;Type;PTROpsia";
            string colnamesMetric = "Active;Measured Depthm;True Vertical Depthm;Manufacturer;Model;Port Sizemm;Type;PTROkpa";
            if (unit == "US")
                colnames = colnamesUS;
            else
                colnames = colnamesMetric;

            SeleniumActions.verifytablelabels(colnames, locator, searchby, unit);
            for (int rowindex = 0; rowindex < d.Rows.Count; rowindex++)
            {
                string active = d.Rows[rowindex]["Active"].ToString();
                string measuredDepth = d.Rows[rowindex]["MeasuredDepth" + unit].ToString();
                string trueVerticalDepth = d.Rows[rowindex]["TrueVerticalDepth" + unit].ToString();
                string manufacturer = d.Rows[rowindex]["Manufacturer"].ToString();
                string model = d.Rows[rowindex]["Model"].ToString();
                string portsize = d.Rows[rowindex]["PortSize" + unit].ToString();
                string type = d.Rows[rowindex]["Type"].ToString();
                string ptro = d.Rows[rowindex]["PTRO" + unit].ToString();
                colvals.Add((rowindex + 1) + ";" + active + ";" + measuredDepth + ";" + trueVerticalDepth + ";" + manufacturer + ";" + model + ";" + portsize + ";" + type + ";" + ptro);



            }

            for (int i = 0; i < colvals.Count; i++)
            {
                SeleniumActions.verifymodelgriddata(tblgasliftgrid, "xpath", colvals[i], "", i + 1, unit, test);
            }
        }

        public static void VerifyRestrictionData(DataTable d, string unit, string locator, string searchby)
        {
            CommonHelper.TraceLine("------Validating Restriction Data-------- in unit system " + unit);
            string colnames = "";

            string colnamesUS = "Name;Type;Measured Depth;Internal Diameter;Critical Flow";
            string colnamesMetric = "Name;Type;Measured Depth;Internal Diameter;Critical Flow";

            if (unit == "US")
                colnames = colnamesUS;
            else
                colnames = colnamesMetric;

            SeleniumActions.verifytablelabels(colnames, locator, searchby, unit);
            Assert.AreEqual(SeleniumActions.getElement("xpath", tblrestrictiongrid, "restriction data").Text, "No records available.", "Restriction data is not correct");


        }
        public static void VerifyRestrictionData(DataTable d, string unit, string locator, string searchby, string grid, ExtentTest test)
        {
            CommonHelper.TraceLine("------Validating Tubing and Casing Data-------- in unit system " + unit);
            string colnames = "";
            List<string> colvals = new List<string>();

            string colnamesUS = "Name;Type;Measured Depth;Internal Diameter;Critical Flow";
            string colnamesMetric = "Name;Type;Measured Depth;Internal Diameter;Critical Flow";
            string action = "ignorevalue";
            if (unit == "US")
                colnames = colnamesUS;
            else
                colnames = colnamesMetric;

            SeleniumActions.verifytablelabels(colnames, locator, searchby, unit);
            for (int rowindex = 0; rowindex < d.Rows.Count; rowindex++)
            {
                string name = d.Rows[rowindex]["Name"].ToString();
                string type = d.Rows[rowindex]["Type"].ToString();
                string measureddepth = d.Rows[rowindex]["MeasuredDepth" + unit].ToString();
                string internalDiameter = d.Rows[rowindex]["InternalDiameter" + unit].ToString();
                string criticalflow = d.Rows[rowindex]["CriticalFlow"].ToString();

                colvals.Add(action + ";" + (rowindex + 1) + ";" + name + ";" + type + ";" + measureddepth + ";" + internalDiameter + ";" + criticalflow);



            }

            for (int i = 0; i < colvals.Count; i++)
            {
                SeleniumActions.verifymodelgriddata(tblrestrictiongrid, "xpath", colvals[i], "", i + 1, unit, test);
            }

        }

        public static void VerifyTracepointsData(DataTable d, string unit, string locator, string searchby)
        {
            CommonHelper.TraceLine("------Validating Tracepoints Data-------- in unit system " + unit);
            string colnames = "";

            string colnamesUS = "Name;Measured Depth";
            string colnamesMetric = "Name;Measured Depth";

            if (unit == "US")
                colnames = colnamesUS;
            else
                colnames = colnamesMetric;

            SeleniumActions.verifytablelabels(colnames, locator, searchby, unit);
            Assert.AreEqual(SeleniumActions.getElement("xpath", tbltracepointsgrid, "Tracepoints data").Text, "No records available.", "Tracepoints data is not correct");

        }


        public static void VerifyPumpData(DataTable d, string unit, string locator, string searchby, ExtentTest test)
        {
            CommonHelper.TraceLine("------Validating Pump Data-------- in unit system " + unit);
            string colnames = "";
            List<string> colvals = new List<string>();
            colnames = "Top Down Id;Pump Model;Number of Stages;Head Tuning Factorfraction";
            for (int rowindex = 0; rowindex < d.Rows.Count; rowindex++)
            {
                string TopDownId = d.Rows[rowindex]["TopDownId"].ToString();

                string PumpModel = d.Rows[rowindex]["PumpModel"].ToString();
                string NumberOfStages = d.Rows[rowindex]["NumberOfStages"].ToString();
                string HeadTuningfactor = d.Rows[rowindex]["HeadTuningfactor"].ToString();

                colvals.Add(TopDownId + ";" + PumpModel + ";" + NumberOfStages + ";" + HeadTuningfactor);
                SeleniumActions.verifytablelabels(colnames, locator, searchby, unit);
                for (int i = 0; i < colvals.Count; i++)
                {
                    SeleniumActions.verifymodelgriddata(tblpumpgrid, "xpath", colvals[i], "", i + 1, unit, test);
                }

            }
        }

        public void VerifyModelData(string welltype, ExtentTest test)
        {

            CommonHelper.TraceLine("------Validating Model Data-------- for well type " + welltype);
            try
            {
                if (welltype.Equals("ESP"))
                {
                    #region ESP
                    SeleniumActions.refreshBrowser();
                    SeleniumActions.WaitForLoad();
                    SeleniumActions.waitClickJS(PageObjects.WellConfigurationPage.tabmodel);
                    SeleniumActions.WaitForLoad();
                    checkFluidModelData("US", "ESP");
                    checkReservoirModelData("US", "ESP");
                    VerifyTrajectoryData(TestData.ESPGLTrajectoryData.dt, "US", PageObjects.WellConfigurationPage.tbltrajectoryheader, "xpath", test);
                    VerifyTubingCasingData(TestData.ESPGLTubingData.dt, "US", PageObjects.WellConfigurationPage.tbltubingheader, "xpath", tbltubinggrid, test);
                    VerifyTubingCasingData(TestData.ESPGLCasingData.dt, "US", PageObjects.WellConfigurationPage.tblcasingheader, "xpath", tblcasinggrid, test);
                    VerifyRestrictionData(null, "US", PageObjects.WellConfigurationPage.tblrestrictionheader, "xpath");
                    VerifyTracepointsData(null, "US", PageObjects.WellConfigurationPage.tbltracepointsheader, "xpath");
                    VerifyPumpData(TestData.ESPDataPump.dt, "US", PageObjects.WellConfigurationPage.tblpumpheader, "xpath", test);
                    checkMotor_Cable_SeparatorData("US");
                    CommonHelper.ChangeUnitSystemUserSetting("Metric");
                    SeleniumActions.waitClickJS(PageObjects.WellDesignPage.welldesignTab);
                    SeleniumActions.WaitForLoad();
                    // SeleniumActions.refreshBrowser();
                    SeleniumActions.waitClickJS(wellconfigurationTab);
                    SeleniumActions.waitClickJS(PageObjects.WellConfigurationPage.tabmodel);
                    SeleniumActions.WaitForLoad();
                    checkFluidModelData("Metric", "ESP");
                    checkReservoirModelData("Metric", "ESP");
                    VerifyTrajectoryData(TestData.ESPGLTrajectoryData.dt, "Metric", PageObjects.WellConfigurationPage.tbltrajectoryheader, "xpath", test);
                    VerifyTubingCasingData(TestData.ESPGLTubingData.dt, "Metric", PageObjects.WellConfigurationPage.tbltubingheader, "xpath", tbltubinggrid, test);
                    VerifyTubingCasingData(TestData.ESPGLCasingData.dt, "Metric", PageObjects.WellConfigurationPage.tblcasingheader, "xpath", tblcasinggrid, test);
                    VerifyRestrictionData(null, "Metric", PageObjects.WellConfigurationPage.tblrestrictionheader, "xpath");
                    VerifyTracepointsData(null, "Metric", PageObjects.WellConfigurationPage.tbltracepointsheader, "xpath");
                    VerifyPumpData(TestData.ESPDataPump.dt, "Metric", PageObjects.WellConfigurationPage.tblpumpheader, "xpath", test);
                    checkMotor_Cable_SeparatorData("Metric");
                    test.Pass("Model verification is completed");
                    #endregion
                }
                else if (welltype.Equals("NF"))
                {
                    #region NF
                    CommonHelper.ChangeUnitSystemUserSetting("US");
                    SeleniumActions.refreshBrowser();
                    SeleniumActions.WaitForLoad();
                    SeleniumActions.waitClickJS(PageObjects.WellConfigurationPage.tabmodel);
                    checkFluidModelData("US", "NF");
                    checkReservoirModelData("US", "NF");
                    VerifyTrajectoryData(TestData.NFWTrajectoryData.dt, "US", PageObjects.WellConfigurationPage.tbltrajectoryheader, "xpath", test);
                    VerifyTubingCasingData(TestData.NFWTubingData.dt, "US", PageObjects.WellConfigurationPage.tbltubingheader, "xpath", tbltubinggrid, test);
                    VerifyTubingCasingData(TestData.NFWCasingData.dt, "US", PageObjects.WellConfigurationPage.tblcasingheader, "xpath", tblcasinggrid, test);
                    VerifyRestrictionData(null, "US", PageObjects.WellConfigurationPage.tblrestrictionheader, "xpath");
                    VerifyTracepointsData(null, "US", PageObjects.WellConfigurationPage.tbltracepointsheader, "xpath");
                    CommonHelper.ChangeUnitSystemUserSetting("Metric");
                    SeleniumActions.waitClickJS(PageObjects.WellDesignPage.welldesignTab);
                    // SeleniumActions.refreshBrowser();
                    SeleniumActions.waitClickJS(wellconfigurationTab);
                    SeleniumActions.WaitForLoad();
                    SeleniumActions.waitClickJS(PageObjects.WellConfigurationPage.tabmodel);
                    checkFluidModelData("Metric", "NF");
                    checkReservoirModelData("Metric", "NF");
                    VerifyTrajectoryData(TestData.NFWTrajectoryData.dt, "Metric", PageObjects.WellConfigurationPage.tbltrajectoryheader, "xpath", test);
                    VerifyTubingCasingData(TestData.NFWTubingData.dt, "Metric", PageObjects.WellConfigurationPage.tbltubingheader, "xpath", tbltubinggrid, test);
                    VerifyTubingCasingData(TestData.NFWCasingData.dt, "Metric", PageObjects.WellConfigurationPage.tblcasingheader, "xpath", tblcasinggrid, test);
                    VerifyRestrictionData(null, "Metric", PageObjects.WellConfigurationPage.tblrestrictionheader, "xpath");
                    VerifyTracepointsData(null, "Metric", PageObjects.WellConfigurationPage.tbltracepointsheader, "xpath");
                    test.Pass("Model verification is completed");
                    #endregion
                }
                else if (welltype.Equals("GL"))
                {
                    #region GL
                    CommonHelper.ChangeUnitSystemUserSetting("US");
                    SeleniumActions.refreshBrowser();
                    SeleniumActions.WaitForLoad();
                    SeleniumActions.waitClickJS(PageObjects.WellConfigurationPage.tabmodel);
                    checkFluidModelData("US", "GL");
                    checkReservoirModelData("US", "GL");
                    VerifyTrajectoryData(TestData.GLTrajectoryData.dt, "US", PageObjects.WellConfigurationPage.tbltrajectoryheader, "xpath", test);
                    VerifyTubingCasingData(TestData.GLTubingData.dt, "US", PageObjects.WellConfigurationPage.tbltubingheader, "xpath", tbltubinggrid, test);
                    VerifyTubingCasingData(TestData.GLCasingData.dt, "US", PageObjects.WellConfigurationPage.tblcasingheader, "xpath", tblcasinggrid, test);
                    VerifyRestrictionData(null, "US", PageObjects.WellConfigurationPage.tblrestrictionheader, "xpath");
                    VerifyTracepointsData(null, "US", PageObjects.WellConfigurationPage.tbltracepointsheader, "xpath");
                    VerifyGasLiftData(TestData.GLGasLiftData.dt, "US", PageObjects.WellConfigurationPage.tblgasliftheader, "xpath", tblgasliftgrid, test);
                    CommonHelper.ChangeUnitSystemUserSetting("Metric");
                    SeleniumActions.waitClickJS(PageObjects.WellDesignPage.welldesignTab);
                    // SeleniumActions.refreshBrowser();
                    SeleniumActions.waitClickJS(wellconfigurationTab);
                    SeleniumActions.WaitForLoad();
                    SeleniumActions.waitClickJS(PageObjects.WellConfigurationPage.tabmodel);
                    checkFluidModelData("Metric", "GL");
                    checkReservoirModelData("Metric", "GL");
                    VerifyTrajectoryData(TestData.GLTrajectoryData.dt, "Metric", PageObjects.WellConfigurationPage.tbltrajectoryheader, "xpath", test);
                    VerifyTubingCasingData(TestData.GLTubingData.dt, "Metric", PageObjects.WellConfigurationPage.tbltubingheader, "xpath", tbltubinggrid, test);
                    VerifyTubingCasingData(TestData.GLCasingData.dt, "Metric", PageObjects.WellConfigurationPage.tblcasingheader, "xpath", tblcasinggrid, test);
                    VerifyRestrictionData(null, "Metric", PageObjects.WellConfigurationPage.tblrestrictionheader, "xpath");
                    VerifyTracepointsData(null, "Metric", PageObjects.WellConfigurationPage.tbltracepointsheader, "xpath");
                    VerifyGasLiftData(TestData.GLGasLiftData.dt, "Metric", PageObjects.WellConfigurationPage.tblgasliftheader, "xpath", tblgasliftgrid, test);
                    test.Pass("Model verification is completed");
                    #endregion
                }
                else if (welltype.Equals("GInj"))
                {
                    #region GInj
                    CommonHelper.ChangeUnitSystemUserSetting("US");
                    SeleniumActions.refreshBrowser();
                    SeleniumActions.WaitForLoad();
                    SeleniumActions.waitClickJS(PageObjects.WellConfigurationPage.tabmodel);
                    checkFluidModelData("US", "GInj");
                    checkReservoirModelData("US", "GInj");
                    VerifyTrajectoryData(TestData.GInjTrajectoryData.dt, "US", PageObjects.WellConfigurationPage.tbltrajectoryheader, "xpath", test);
                    VerifyTubingCasingData(TestData.GInjTubingData.dt, "US", PageObjects.WellConfigurationPage.tbltubingheader, "xpath", tbltubinggrid, test);
                    VerifyTubingCasingData(TestData.GInjCasingData.dt, "US", PageObjects.WellConfigurationPage.tblcasingheader, "xpath", tblcasinggrid, test);
                    VerifyRestrictionData(null, "US", PageObjects.WellConfigurationPage.tblrestrictionheader, "xpath");
                    VerifyTracepointsData(null, "US", PageObjects.WellConfigurationPage.tbltracepointsheader, "xpath");
                    CommonHelper.ChangeUnitSystemUserSetting("Metric");
                    SeleniumActions.waitClickJS(PageObjects.WellDesignPage.welldesignTab);
                    // SeleniumActions.refreshBrowser();
                    SeleniumActions.waitClickJS(wellconfigurationTab);
                    SeleniumActions.WaitForLoad();
                    SeleniumActions.waitClickJS(PageObjects.WellConfigurationPage.tabmodel);
                    checkFluidModelData("Metric", "GInj");
                    checkReservoirModelData("Metric", "GInj");
                    VerifyTrajectoryData(TestData.GInjTrajectoryData.dt, "Metric", PageObjects.WellConfigurationPage.tbltrajectoryheader, "xpath", test);
                    VerifyTubingCasingData(TestData.GInjTubingData.dt, "Metric", PageObjects.WellConfigurationPage.tbltubingheader, "xpath", tbltubinggrid, test);
                    VerifyTubingCasingData(TestData.GInjCasingData.dt, "Metric", PageObjects.WellConfigurationPage.tblcasingheader, "xpath", tblcasinggrid, test);
                    VerifyRestrictionData(null, "Metric", PageObjects.WellConfigurationPage.tblrestrictionheader, "xpath");
                    VerifyTracepointsData(null, "Metric", PageObjects.WellConfigurationPage.tbltracepointsheader, "xpath");
                    test.Pass("Model verification is completed");
                    #endregion
                }
                else if (welltype.Equals("PGL"))
                {
                    #region PGL
                    CommonHelper.ChangeUnitSystemUserSetting("US");
                    SeleniumActions.refreshBrowser();
                    SeleniumActions.WaitForLoad();
                    SeleniumActions.waitClickJS(PageObjects.WellConfigurationPage.tabmodel);
                    checkFluidModelData("US", "PGL");
                    checkReservoirModelData("US", "PGL");
                    VerifyTrajectoryData(TestData.PGLTrajectoryData.dt, "US", PageObjects.WellConfigurationPage.tbltrajectoryheader, "xpath", test);
                    VerifyTubingCasingData(TestData.PGLTubingData.dt, "US", PageObjects.WellConfigurationPage.tbltubingheader, "xpath", tbltubinggrid, test);
                    VerifyTubingCasingData(TestData.PGLCasingData.dt, "US", PageObjects.WellConfigurationPage.tblcasingheader, "xpath", tblcasinggrid, test);
                    VerifyRestrictionData(null, "US", PageObjects.WellConfigurationPage.tblrestrictionheader, "xpath");
                    VerifyTracepointsData(null, "US", PageObjects.WellConfigurationPage.tbltracepointsheader, "xpath");
                    checkPlungerLiftData("US");
                    CommonHelper.ChangeUnitSystemUserSetting("Metric");
                    SeleniumActions.waitClickJS(PageObjects.WellDesignPage.welldesignTab);
                    // SeleniumActions.refreshBrowser();
                    SeleniumActions.waitClickJS(wellconfigurationTab);
                    SeleniumActions.WaitForLoad();
                    SeleniumActions.waitClickJS(PageObjects.WellConfigurationPage.tabmodel);
                    checkFluidModelData("Metric", "PGL");
                    checkReservoirModelData("Metric", "PGL");
                    VerifyTrajectoryData(TestData.PGLTrajectoryData.dt, "Metric", PageObjects.WellConfigurationPage.tbltrajectoryheader, "xpath", test);
                    VerifyTubingCasingData(TestData.PGLTubingData.dt, "Metric", PageObjects.WellConfigurationPage.tbltubingheader, "xpath", tbltubinggrid, test);
                    VerifyTubingCasingData(TestData.PGLCasingData.dt, "Metric", PageObjects.WellConfigurationPage.tblcasingheader, "xpath", tblcasinggrid, test);
                    VerifyRestrictionData(null, "Metric", PageObjects.WellConfigurationPage.tblrestrictionheader, "xpath");
                    VerifyTracepointsData(null, "Metric", PageObjects.WellConfigurationPage.tbltracepointsheader, "xpath");
                    checkPlungerLiftData("Metric");
                    test.Pass("Model verification is completed");
                    #endregion
                }
                else if (welltype.Equals("WaterInj"))
                {
                    #region WaterInj
                    CommonHelper.ChangeUnitSystemUserSetting("US");
                    SeleniumActions.refreshBrowser();
                    SeleniumActions.WaitForLoad();
                    SeleniumActions.waitClickJS(PageObjects.WellConfigurationPage.tabmodel);
                    checkFluidModelData("US", "WaterInj");
                    checkReservoirModelData("US", "WaterInj");
                    Assert.AreEqual(SeleniumActions.Gettotalrecords("xpath", tbltrajectorygrid).ToString(), "48", "Total no. of records in trajectory grid are not displayed");
                    VerifyTrajectoryData(TestData.WaterInjTrajectoryData.dt, "US", PageObjects.WellConfigurationPage.tbltrajectoryheader, "xpath", test);
                    VerifyTubingCasingData(TestData.WaterInjTubingData.dt, "US", PageObjects.WellConfigurationPage.tbltubingheader, "xpath", tbltubinggrid, test);
                    VerifyTubingCasingData(TestData.WaterInjCasingData.dt, "US", PageObjects.WellConfigurationPage.tblcasingheader, "xpath", tblcasinggrid, test);
                    VerifyRestrictionData(TestData.WaterInjRestrictionData.dt, "US", PageObjects.WellConfigurationPage.tblrestrictionheader, "xpath", tblrestrictiongrid, test);
                    VerifyTracepointsData(null, "US", PageObjects.WellConfigurationPage.tbltracepointsheader, "xpath");
                    CommonHelper.ChangeUnitSystemUserSetting("Metric");
                    SeleniumActions.waitClickJS(PageObjects.WellDesignPage.welldesignTab);
                    // SeleniumActions.refreshBrowser();
                    SeleniumActions.waitClickJS(wellconfigurationTab);
                    SeleniumActions.WaitForLoad();
                    SeleniumActions.waitClickJS(PageObjects.WellConfigurationPage.tabmodel);
                    checkFluidModelData("Metric", "WaterInj");
                    checkReservoirModelData("Metric", "WaterInj");
                    Assert.AreEqual(SeleniumActions.Gettotalrecords("xpath", tbltrajectorygrid).ToString(), "48", "Total no. of records in trajectory grid are not displayed");
                    VerifyTrajectoryData(TestData.WaterInjTrajectoryData.dt, "Metric", PageObjects.WellConfigurationPage.tbltrajectoryheader, "xpath", test);
                    VerifyTubingCasingData(TestData.WaterInjTubingData.dt, "Metric", PageObjects.WellConfigurationPage.tbltubingheader, "xpath", tbltubinggrid, test);
                    VerifyTubingCasingData(TestData.WaterInjCasingData.dt, "Metric", PageObjects.WellConfigurationPage.tblcasingheader, "xpath", tblcasinggrid, test);
                    VerifyRestrictionData(TestData.WaterInjRestrictionData.dt, "Metric", PageObjects.WellConfigurationPage.tblrestrictionheader, "xpath", tblrestrictiongrid, test);
                    VerifyTracepointsData(null, "Metric", PageObjects.WellConfigurationPage.tbltracepointsheader, "xpath");
                    #endregion
                }
                else
                {
                    CommonHelper.TraceLine("No validation implemented for type " + welltype);
                    return;
                }
                SeleniumActions.takeScreenshot("WellModelverification");
            }
            catch (Exception e)
            {

                CommonHelper.TraceLine("Error in Model verification  " + e.Message);
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "WellModelverification.png"))).Build());
                Assert.Fail(e.ToString());
            }
        }
        public void Deletewell()
        {
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.deletebutton);
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.firstdelete);
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.seconddelete);
            SeleniumActions.WaitForLoad();
            string welldeltext = SeleniumActions.getInnerText(PageObjects.WellConfigurationPage.Toaseter);
            CommonHelper.TraceLine(welldeltext);
            SeleniumActions.takeScreenshot("Well Deletion");
            Assert.AreEqual("Well has been successfully deleted.", welldeltext, "Well Deletion Toast did not appear");


        }


        public void clickCellWithText(string celltext)
        {
            PageObjects.WellConfigurationPage.strDynamicValue = celltext;
            SeleniumActions.waitClickJS(PageObjects.WellConfigurationPage.cellwithText);
            SeleniumActions.WaitForLoad();
        }

        public void clickCellWithTexContains(string celltext)
        {
            PageObjects.WellConfigurationPage.strDynamicValue = celltext;
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.cellwithTextContains);
            SeleniumActions.WaitForLoad();
        }

        public void selectFromFilter(string columnname, string value)
        {
            PageObjects.WellConfigurationPage.strDynamicValue = columnname;
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.filterColumnName);
            SeleniumActions.sendText(PageObjects.WellConfigurationPage.txtcontainsFiltertextbox, value);
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.btnFilter);
            clickCellWithTexContains(value);
        }

        public void Toastercheck(string scenario, string exptext, ExtentTest testm)
        {
            string toasttext = SeleniumActions.getInnerText(PageObjects.WellConfigurationPage.Toaseter);
            CommonHelper.TraceLine(toasttext);
            SeleniumActions.takeScreenshot(scenario);
            Assert.AreEqual(exptext, toasttext, scenario + " Toast did not appear");
            testm.Pass("Scenario: " + scenario, MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", scenario + ".png"))).Build());
        }

        public void selectWellfromWellList(string wellname)
        {
            if (SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellConfigurationPage.lblWellListHeader) == false)
            {//If already Well list is not open then only select click Well count lable to show up Well List
                SeleniumActions.waitClick(PageObjects.DashboardPage.lblWelCount);
                SeleniumActions.WaitForLoad();
                PageObjects.DashboardPage.Dynatext = wellname;
                SeleniumActions.waitClick(PageObjects.DashboardPage.lstWellName);
                SeleniumActions.WaitForLoad();
            }
        }

        public void ChangeWellType(string ToType, ExtentTest test)
        {
            //go to Well Configuration screen 
            try
            {
                Configcommon();
                //Change Well Type to Gas Lift
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.btnChangeWellType);
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.btnConfirmChangeWellType);
                SeleniumActions.selectKendoDropdownValue(PageObjects.WellConfigurationPage.kendoDDNewWellType, ToType);
                SeleniumActions.sendText(PageObjects.WellConfigurationPage.txtWellTypeChangeDate, "03/01/2019", true);
                SeleniumActions.sendText(PageObjects.WellConfigurationPage.txtMOPComment, "Well Type was changed from RRL to Gas lift");
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.btnMOPSave);
                Toastercheck("Well Type change", "Success", test);
                SeleniumActions.takeScreenshot("ChangeWellTypeWorkFlowSuccess");
                SeleniumActions.selectKendoDropdownValue(PageObjects.WellConfigurationPage.Fluidtypedrpdwn, "Black Oil");
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.savebutton);
                Toastercheck("Well Save", "Well " + TestData.WellConfigData.RRFACName + " saved successfully.", test);
                test.Info("Change Well Type ", MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "ChangeWellTypeWorkFlowSuccess.png"))).Build());
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("ChangeWellTypeWorkFlow");
                CommonHelper.TraceLine("Error in ChangeWellType  " + e.Message);
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "ChangeWellTypeWorkFlow.png"))).Build());
                Assert.Fail(e.ToString());

            }

        }


        public void ViewWellTypeHistory(ExtentTest test)
        {
            //Change Well Type to Gas Lift
            Configcommon();
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.btnViewHistory);
            UserPrincipal userPrincipal = UserPrincipal.Current;
            string usrname = userPrincipal.DisplayName;
            string datetoday = DateTime.Today.ToString("MM/dd/yyyy");
            string expcolnames = "Well Type;Start Date;End Date;Comment;Change User;Change Date";
            string expcolvals = "RRL;01/02/2018;03/01/2019;Well Type was changed from RRL to Gas lift;" + usrname + ";" + datetoday;
            SeleniumActions.verifyGridCellValues(expcolnames, expcolvals, test);


        }
        public void DeleteWellByUI(string wellname, ExtentTest test)

        {
            try
            {
                if (SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellConfigurationPage.wellconfigurationTab) == false)
                {
                    SeleniumActions.waitClick(PageObjects.WellConfigurationPage.configurationTab);
                }
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.wellconfigurationTab);
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.general);
                SeleniumActions.waitForElement(PageObjects.WellConfigurationPage.facilitybutton);
                selectWellfromWellList(TestData.WellConfigData.RRFACName);
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.btnDeleteWellby);
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.btnConfirmDeleteWellby);
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.btnConfirmDeleteWellby);
                Toastercheck("Well Deletion", "Well has been successfully deleted.", test);
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("ErrDeleteWellUI");
                CommonHelper.TraceLine("DeleteWellUI " + e.Message);
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "ErrDeleteWellUI.png"))).Build());
                Assert.Fail(e.ToString());
            }


        }
        public void Configcommon()
        {
            SeleniumActions.refreshBrowser();
            SeleniumActions.WaitForLoad();
            DashbordfilterVlidation();
            if (SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellConfigurationPage.wellconfigurationTab) == false)
            {
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.configurationTab);
            }
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.wellconfigurationTab);
            SeleniumActions.WaitForLoad();
            selectWellfromWellList(TestData.WellConfigData.RRFACName);
            SeleniumActions.WaitForLoad();
        }

        public void Createtestwell()
        {

            string isruunisats = ConfigurationManager.AppSettings.Get("isRunningATS");
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.configurationTab);
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.wellconfigurationTab);
            SeleniumActions.WaitForLoad();


            #region GenralTab

            EnterwellGeneralData(TestData.WellConfigData.WellTypeESP, "ESP");

            string facilityname = PageObjects.WellConfigurationPage.wellnametextbox.GetAttribute("value");
            #endregion
            Enterwellattributes();
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.savebutton);
            SeleniumActions.waitForElement(PageObjects.WellConfigurationPage.Toaseter);

            SeleniumActions.WaitForLoad();

        }

        public void ClickOnNewWellTestCommon()
        {
            SeleniumActions.waitClickJS(PageObjects.WellDesignPage.taboptimization);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClickJS(PageObjects.WellDesignPage.tabwelltest);
            SeleniumActions.WaitForLoad();
            //FRI - 3983 is fixed no need for workaround hence commenting this code 
            //SeleniumActions.WaitForLoad();
            //SeleniumActions.refreshBrowser();
            //SeleniumActions.waitforPageloadComplete();
            //SeleniumActions.WaitforWellcountNonZero();
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.WellTestPage.btnCreateNewWellTest);
            SeleniumActions.WaitForLoad();
        }
        public void EnterwellGeneralData(string welltype, string facility)
        {
            //Well General tab
            #region General
            string strwellcount = SeleniumActions.getInnerText(PageObjects.DashboardPage.lblWelCount);
            CommonHelper.TraceLine("Well count is " + strwellcount);
            int wellcount = int.Parse(strwellcount.Trim());

            if (wellcount > 0)
            {
                if (SeleniumActions.waitforDispalyed(btnCreateNewWell))
                {
                    SeleniumActions.WaitForLoad();
                    SeleniumActions.waitForElement(wellattributetabby);
                    SeleniumActions.waitForElement(btnCreateNewWellby);
                    SeleniumActions.waitClick(btnCreateNewWell);
                }
            }


            SeleniumActions.waitForElement(boreholeinputby);
            SeleniumActions.selectKendoDropdownValue(welltypedropdwn, welltype, true);
            if (welltype.Equals("Gas Lift"))
            {
                SeleniumActions.selectKendoDropdownValue(Fluidtypedrpdwn, WellConfigData.FluidType);
            }
            if (welltype.Equals("Plunger Lift"))
            {
                SeleniumActions.selectKendoDropdownValue(Fluidtypedrpdwn, WellConfigData.PGLFluidType);
            }
            if (welltype.Equals("Naturally Flowing"))
            {
                SeleniumActions.selectKendoDropdownValue(Fluidtypedrpdwn, WellConfigData.FluidType);
            }
            SeleniumActions.selectKendoDropdownValue(scadatypedrpdwn, WellConfigData.ScadaType);
            SeleniumActions.WaitForLoad();
            SeleniumActions.selectKendoDropdownValue(cygnetdomaindrpdwn, WellConfigData.CygNetDomain);//9077
            SeleniumActions.WaitForLoad();
            SeleniumActions.selectKendoDropdownValue(cygnetservicedrpdwn, WellConfigData.UIS);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(facilitybutton);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(facIdFilter);
            SeleniumActions.sendText(txtcontainsFiltertextbox, facility);
            SeleniumActions.waitClick(btnFilter);
            SeleniumActions.waitClick(Facilityist);
            SeleniumActions.waitClick(firstRow);
            SeleniumActions.waitClick(btnApplyFilter);
            string facilityname = facilitytextbox.GetAttribute("value");
            //  var guid = Guid.NewGuid().ToString();
            //  SeleniumActions.sendText(wellnameinput, facilityname + guid);
            SeleniumActions.sendText(wellnameinput, facilityname);
            SeleniumActions.WaitForLoad();
            SeleniumActions.selectKendoDropdownValue(assettypedrpdwn, WellConfigData.AssetName);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(commissioninput);
            SeleniumActions.sendText(commissioninput, DateTime.Now.AddYears(-2).ToString("MMddyyyy"), true);
            //Spud date was removed from the UI as per FRI -3526 changes
            //SeleniumActions.sendText(PageObjects.WellConfigurationPage.initialspuddate, "01022018", true);
            SeleniumActions.sendText(wellboreinput, facilityname);
            SeleniumActions.sendspecialkey(wellboreinput, "tab");
            SeleniumActions.sendText(boreholeinput, facilityname);
            SeleniumActions.sendspecialkey(boreholeinput, "tab");
            SeleniumActions.selectKendoDropdownValue(welldepthreference, "Unknown");
            SeleniumActions.sendText(intervalinput, facilityname);
            SeleniumActions.sendText(txtLatitude, "19.7899");
            SeleniumActions.sendText(txtLongitude, "79.0345");

            #endregion
        }
    }
}
