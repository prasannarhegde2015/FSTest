using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmokeTests.SeleniumObject;
using System.Configuration;
using SmokeTests.Helper;
using System.Diagnostics;
using AventStack.ExtentReports;
using System.IO;
using System;
using System.Data;
using OpenQA.Selenium;
using System.Collections.Generic;

namespace SmokeTests.TestClasses
{

    class WellConfiguration:PageObjects.WellConfigurationPage
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
            test.Info(" Unit System is changed to Metric", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine(ExtentFactory.screenshotdir, "Metric Text.png"))).Build());

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
            test.Info("Unit System is changed to US", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine(ExtentFactory.screenshotdir, "US Text.png"))).Build());
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
            test.Pass("Well is Deleted", MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine(ExtentFactory.screenshotdir, "Well Deletion.png"))).Build());



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
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.WellStatusTab);
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(PageObjects.WellStatusPage.btnScan);
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(PageObjects.WellStatusPage.btnConfirmSend);
                Toastercheck("Scan Commamnd", "Command Scan issued successfully.", test);
                string statustext = SeleniumActions.getInnerText(PageObjects.WellStatusPage.lblCommStatusBy);
                Assert.AreEqual("OK", statustext, "Communication is failed");
                // Go to Well Analysis and Collect Cards
                SeleniumActions.waitClick(PageObjects.DashboardPage.tabOptimization);
                SeleniumActions.waitClick(PageObjects.DashboardPage.tabAnalysis);
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(PageObjects.WellAnalysisPage.btnScanCards);
                SeleniumActions.waitClick(PageObjects.WellAnalysisPage.lnkCurrentCard);
                Toastercheck("Scan Card", "Scan card request sent.", test);
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("CreateRRLUpgradeWellWorkFlow");
                CommonHelper.TraceLine("Error in CreateRRLUpgradeWellWorkFlow " + e.Message);
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine(ExtentFactory.screenshotdir, "CreateRRLUpgradeWellWorkFlow.png"))).Build());
                Assert.Fail(e.ToString());

            }
        }
        public void CreateESPWellWorkFlow(ExtentTest test)
        {
            // *********** Create New FULL RRL Well  *******************

            try
            {
               
                CreateESPWellFullonBlankDB(test);

            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("CreateESPWellWorkFlow");
                CommonHelper.TraceLine("Error in CreateESPWellWorkFlow " + e.Message);
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine(ExtentFactory.screenshotdir, "CreateESPWellWorkFlow.png"))).Build());
                Assert.Fail(e.ToString());

            }





            // *********** Switch to Frame  ******************

            // *********** Dispose  ******************
            SeleniumActions.disposeDriver();

        }

        public void CreateRRLWellFullonBlankDB(ExtentTest test)
        {
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
            SeleniumActions.sendText(PageObjects.WellConfigurationPage.wellboreinput, TestData.WellConfigData.RRFACName);
            SeleniumActions.sendspecialkey(PageObjects.WellConfigurationPage.wellboreinput, "tab");
            SeleniumActions.sendText(PageObjects.WellConfigurationPage.boreholeinput, TestData.WellConfigData.RRFACName);
            SeleniumActions.sendspecialkey(PageObjects.WellConfigurationPage.boreholeinput, "tab");
            SeleniumActions.sendText(PageObjects.WellConfigurationPage.intervalinput, TestData.WellConfigData.RRFACName);
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
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.btnApplyPumpingUnit);

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

            #endregion
            //Downhole data
            #region Downhole
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.tabDownhole);
            SeleniumActions.selectKendoDropdownValue(PageObjects.WellConfigurationPage.kendoDDPumpDiameter, "2.5");
            SeleniumActions.sendText(PageObjects.WellConfigurationPage.txtPumpDepth, "5081");
            SeleniumActions.sendText(PageObjects.WellConfigurationPage.txtTubingOD, "2.875");
            SeleniumActions.sendText(PageObjects.WellConfigurationPage.txtTubingID, "2.750");

            SeleniumActions.sendText(PageObjects.WellConfigurationPage.txtTubingAnchorDepth, "1");
            SeleniumActions.sendText(PageObjects.WellConfigurationPage.txtCasingOD, "6");
            SeleniumActions.sendText(PageObjects.WellConfigurationPage.txtCasingWeight, "1");
            SeleniumActions.sendText(PageObjects.WellConfigurationPage.txtTopPerforation, "4558");
            SeleniumActions.sendText(PageObjects.WellConfigurationPage.txtBottomPerforation, "5220");
            #endregion
            //Rods Data
            #region Rods
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.tabRods);
            PageObjects.WellConfigurationPage.AddRodRows("0.75", "56");
            PageObjects.WellConfigurationPage.AddRodRows("1", "56");
            PageObjects.WellConfigurationPage.AddRodRows("1.125", "57");
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.savebutton);
            SeleniumActions.waitForElement(PageObjects.WellConfigurationPage.Toaseter);
            SeleniumActions.WaitForLoad();
            #endregion
            Toastercheck("Well Creation", "Well " + TestData.WellConfigData.RRFACName + " saved successfully.", test);
            test.Info("Well is created");
        }
        
        public void CreateESPWellFullonBlankDB(ExtentTest test)
        {
           
            string isruunisats = ConfigurationManager.AppSettings.Get("isRunningATS");
           SeleniumActions.waitClick(PageObjects.WellConfigurationPage.configurationTab);
          SeleniumActions.waitClick(PageObjects.WellConfigurationPage.wellconfigurationTab);
            SeleniumActions.WaitForLoad();

           
            #region GenralTab
            
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

            SeleniumActions.waitForElement(PageObjects.WellConfigurationPage.btnCreateNewWellby);
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.btnCreateNewWell);
            SeleniumActions.waitForElement(PageObjects.WellConfigurationPage.boreholeinputby);
            SeleniumActions.selectKendoDropdownValue(PageObjects.WellConfigurationPage.welltypedropdwn, TestData.WellConfigData.WellTypeESP);
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
            SeleniumActions.sendText(PageObjects.WellConfigurationPage.txtcontainsFiltertextbox, "ESP");
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.btnFilter);
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.firstRow);
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.btnApplyFilter);
            string facilityname = PageObjects.WellConfigurationPage.facilitytextbox.GetAttribute("value");
            SeleniumActions.sendText(PageObjects.WellConfigurationPage.wellnameinput, facilityname);
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.commissioninput);
            SeleniumActions.sendText(PageObjects.WellConfigurationPage.commissioninput, "01022018", true);
            SeleniumActions.sendText(PageObjects.WellConfigurationPage.wellboreinput, facilityname);
            SeleniumActions.sendspecialkey(PageObjects.WellConfigurationPage.wellboreinput, "tab");
            SeleniumActions.sendText(PageObjects.WellConfigurationPage.boreholeinput, facilityname);
            SeleniumActions.sendspecialkey(PageObjects.WellConfigurationPage.boreholeinput, "tab");
            SeleniumActions.sendText(PageObjects.WellConfigurationPage.intervalinput, facilityname);
            SeleniumActions.sendText(PageObjects.WellConfigurationPage.txtLatitude, "19.7899");
            SeleniumActions.sendText(PageObjects.WellConfigurationPage.txtLongitude, "79.0345");
            uploadmodelfile("Esp_ProductionTestData.wflx");

            #endregion
            Enterwellattributes();
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.savebutton);
            SeleniumActions.waitForElement(PageObjects.WellConfigurationPage.Toaseter);
            SeleniumActions.WaitForLoad();

            Toastercheck("Well Creation", "Well " + facilityname + " saved successfully.", test);
            test.Info("Well is created");
            VerifyModelData("ESP");
            Deletewell();


        }

        //
        public void clickCellWithText(string celltext)
        {
            PageObjects.WellConfigurationPage.strDynamicValue = celltext;
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.cellwithText);
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
            testm.Pass("Scenario: " + scenario, MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine(ExtentFactory.screenshotdir, scenario + ".png"))).Build());
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

        public void uploadmodelfile(string modelfile)
        {
            string path = Path.Combine(FilesLocation + @"\SmokeTests_Data\TestData", modelfile);
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.btnmodelfiledate);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.selectmodelfile);
            PageObjects.WellConfigurationPage.selectmodelfile.SendKeys(path);
            System.Windows.Forms.SendKeys.SendWait(path);
            System.Windows.Forms.SendKeys.SendWait(@"{Enter}");
            SeleniumActions.WaitForLoad();
            SeleniumActions.sendText(PageObjects.WellConfigurationPage.applicabledate, "01022018", true);
            SeleniumActions.sendText(PageObjects.WellConfigurationPage.modelcomment, "Test");
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.tuningmethod);
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.tuningmethod);
            SeleniumActions.WaitForLoad();
            SeleniumActions.selectKendoDropdownValue(PageObjects.WellConfigurationPage.tuningmethod, "L Factor");
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.modelapply);
            
        }

        public void Verifymodeldata(DataTable d,string fieldNameText,string searchby,string unit)
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
            string fieldname = TestData.ESPGLModelData.FieldName;
            string ValueUS = TestData.ESPGLModelData.ValueUS;
            
        }


        public void checkFluidModelData(string unit)
        {
            if (unit.Equals("Metric"))
            {
                CommonHelper.TraceLine("------Validating Fluid Data-------- in unit system "+ unit);
                Verifymodeldata(TestData.ESPGLModelData.dt, "Oil Specific Gravity", lbloilspecificgravity, "Metric");
                Verifymodeldata(TestData.ESPGLModelData.dt, "Gas Specific Gravity", lblgasspecificgravity, "Metric");
                Verifymodeldata(TestData.ESPGLModelData.dt, "Water Salinity", lblWaterSalinity, "Metric");
                Verifymodeldata(TestData.ESPGLModelData.dt, "Hydrogen Sulfide (H2S)", lblHydrogenSulfide, "Metric");
                Verifymodeldata(TestData.ESPGLModelData.dt, "Carbon Dioxide (C02)", lblCarbonDioxide, "Metric");
                Verifymodeldata(TestData.ESPGLModelData.dt, "Nitrogen (N2)", lblNitrogen, "Metric");

            }
            else if (unit.Equals("US"))
            {
                CommonHelper.TraceLine("------Validating Fluid Data-------- in unit system " + unit);
                Verifymodeldata(TestData.ESPGLModelData.dt, "Oil Specific Gravity", lbloilspecificgravity, "US");
                Verifymodeldata(TestData.ESPGLModelData.dt, "Gas Specific Gravity", lblgasspecificgravity, "US");
                Verifymodeldata(TestData.ESPGLModelData.dt, "Water Salinity", lblWaterSalinity, "US");
                Verifymodeldata(TestData.ESPGLModelData.dt, "Hydrogen Sulfide (H2S)", lblHydrogenSulfide, "US");
                Verifymodeldata(TestData.ESPGLModelData.dt, "Carbon Dioxide (C02)", lblCarbonDioxide, "US");
                Verifymodeldata(TestData.ESPGLModelData.dt, "Nitrogen (N2)", lblNitrogen, "US");

            }
        }
        public void checkReservoirModelData(string unit)
        {
            if (unit.Equals("Metric"))
            {
                CommonHelper.TraceLine("------Validating Reservoir Data-------- in unit system " + unit);
                Verifymodeldata(TestData.ESPGLModelData.dt, "Pressure", lblpressure, "Metric");
                Verifymodeldata(TestData.ESPGLModelData.dt, "Temperature", lbltemperature, "Metric");
                Verifymodeldata(TestData.ESPGLModelData.dt, "MidPerf Depth (MD)", lblmidperfdepth, "Metric");
                Verifymodeldata(TestData.ESPGLModelData.dt, "Water Cut", lblwatercut, "Metric");
                Verifymodeldata(TestData.ESPGLModelData.dt, "Gas-Oil Ratio (GOR)", lblgor, "Metric");
                Verifymodeldata(TestData.ESPGLModelData.dt, "Productivity Index-ESP", lblproductivityindex, "Metric");
            }
            else if (unit.Equals("US"))
            {
                CommonHelper.TraceLine("------Validating Reservoir Data-------- in unit system " + unit);
                Verifymodeldata(TestData.ESPGLModelData.dt, "Pressure", lblpressure, "US");
                Verifymodeldata(TestData.ESPGLModelData.dt, "Temperature", lbltemperature, "US");
                Verifymodeldata(TestData.ESPGLModelData.dt, "MidPerf Depth (MD)", lblmidperfdepth, "US");
                Verifymodeldata(TestData.ESPGLModelData.dt, "Water Cut", lblwatercut, "US");
                Verifymodeldata(TestData.ESPGLModelData.dt, "Gas-Oil Ratio (GOR)", lblgor, "US");
                Verifymodeldata(TestData.ESPGLModelData.dt, "Productivity Index-ESP", lblproductivityindex, "US");
            }
        }
        public void checkMotor_Cable_SeparatorData(string unit)
        {
            if (unit.Equals("Metric"))
            {
                CommonHelper.TraceLine("------Validating Motor,Cable and Seperator Data-------- in unit system " + unit);
                Verifymodeldata(TestData.ESPGLModelData.dt, "Motor Model", lblmotormodel, "Metric");
                Verifymodeldata(TestData.ESPGLModelData.dt, "Measured Depth", lblmeasureddepth, "Metric");
                Verifymodeldata(TestData.ESPGLModelData.dt, "Nameplate Rating", lblnameplaterating, "Metric");
                Verifymodeldata(TestData.ESPGLModelData.dt, "Operating Rating", lbloperatingrating, "Metric");
                Verifymodeldata(TestData.ESPGLModelData.dt, "Operating Frequency", lbloperatingfrequency, "Metric");
                Verifymodeldata(TestData.ESPGLModelData.dt, "Motor Wear Factor", lblmotorwearfactor, "Metric");
                Verifymodeldata(TestData.ESPGLModelData.dt, "Cable Size", lblcablesize, "Metric");
                Verifymodeldata(TestData.ESPGLModelData.dt, "Gas Separator Present", lblgasseparatorpresent, "Metric");
                Verifymodeldata(TestData.ESPGLModelData.dt, "Separator Efficiency", lblseparatorefficiency, "Metric");
            }
            else if (unit.Equals("US"))
            {
                CommonHelper.TraceLine("------Validating Motor,Cable and Seperator Data-------- in unit system " + unit);
                Verifymodeldata(TestData.ESPGLModelData.dt, "Motor Model", lblmotormodel, "US");
                Verifymodeldata(TestData.ESPGLModelData.dt, "Measured Depth", lblmeasureddepth, "US");
                Verifymodeldata(TestData.ESPGLModelData.dt, "Nameplate Rating", lblnameplaterating, "US");
                Verifymodeldata(TestData.ESPGLModelData.dt, "Operating Rating", lbloperatingrating, "US");
                Verifymodeldata(TestData.ESPGLModelData.dt, "Operating Frequency", lbloperatingfrequency, "US");
                Verifymodeldata(TestData.ESPGLModelData.dt, "Motor Wear Factor", lblmotorwearfactor, "US");
                Verifymodeldata(TestData.ESPGLModelData.dt, "Cable Size", lblcablesize, "US");
                Verifymodeldata(TestData.ESPGLModelData.dt, "Gas Separator Present", lblgasseparatorpresent, "US");
                Verifymodeldata(TestData.ESPGLModelData.dt, "Separator Efficiency", lblseparatorefficiency, "US");
            }
        }

        public static void VerifyTrajectoryData(DataTable d, string unit,string locator,string searchby)
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

              SeleniumActions.verifytableheaders(colnames, locator,searchby,unit);
                for (int i = 0; i < colvals.Count; i++)
                {
                    SeleniumActions.verifygriddata(tbltrajectorygrid, "xpath", colvals[i], "", i + 1,unit);
                }

            }


        }

        public static void VerifyTubingCasingData(DataTable d, string unit, string locator, string searchby,string grid)
        {
            CommonHelper.TraceLine("------Validating Tubing and Casing Data-------- in unit system " + unit);
            string colnames = "";
            List<string> colvals = new List<string>();

            string colnamesUS = "Name;Start Point Measured Depthft;End Point Measured Depthft;External Diameterin;Internal Diameterin;Roughnessin";
            string colnamesMetric = "Name;Start Point Measured Depthm;End Point Measured Depthm;External Diametermm;Internal Diametermm;Roughnessmm";
            string action = "ignorevalue";
           
            for (int rowindex = 0; rowindex < d.Rows.Count; rowindex++)
            {
                string name = d.Rows[rowindex]["Name"].ToString();
                string startPointMeasure = d.Rows[rowindex]["StartPointMeasure" + unit].ToString();
                string endPointMeasure = d.Rows[rowindex]["EndPointMeasure" + unit].ToString();
                string externalDiameter = d.Rows[rowindex]["ExternalDiameter" + unit].ToString();
                string internalDiameter = d.Rows[rowindex]["InternalDiameter" + unit].ToString();
                string roughness = d.Rows[rowindex]["Roughness" + unit].ToString();

               colvals.Add(action + ";" + (rowindex+1)+";" + name + ";" + startPointMeasure + ";" + endPointMeasure + ";" + externalDiameter + ";" + internalDiameter + ";" + roughness);

                if (unit == "US")
                    colnames = colnamesUS;
                else
                    colnames = colnamesMetric;

                SeleniumActions.verifytableheaders(colnames,  locator, searchby,unit);
               
            }
            if(grid.Equals(tbltubinggrid))
            for (int i = 0; i < colvals.Count; i++)
            {
                SeleniumActions.verifygriddata(tbltubinggrid, "xpath", colvals[i], "",i+1,unit);
            }
            else if (grid.Equals(tblcasinggrid))
                for (int i = 0; i < colvals.Count; i++)
                {
                    SeleniumActions.verifygriddata(tblcasinggrid, "xpath", colvals[i], "", i + 1, unit);
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

                SeleniumActions.verifytableheaders(colnames, locator, searchby,unit);
            Assert.AreEqual(SeleniumActions.getElement("xpath", tblrestrictiongrid, "restriction data").Text, "No records available.", "Restriction data is not correct");


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

            SeleniumActions.verifytableheaders(colnames, locator, searchby,unit);
            Assert.AreEqual(SeleniumActions.getElement("xpath", tbltracepointsgrid, "Tracepoints data").Text, "No records available.", "Tracepoints data is not correct");

        }


        public static void VerifyPumpData(DataTable d, string unit, string locator, string searchby)
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
                SeleniumActions.verifytableheaders(colnames,  locator, searchby,unit);
                for (int i = 0; i < colvals.Count; i++)
                {
                    SeleniumActions.verifygriddata(tblpumpgrid, "xpath", colvals[i], "", i + 1, unit);
                }
               
            }
        }

        public void VerifyModelData(string welltype)
        {
            CommonHelper.TraceLine("------Validating Model Data-------- for well type " + welltype);
            if (welltype.Equals("ESP"))
            {
                CommonHelper.ChangeUnitSystemUserSetting("US");
                SeleniumActions.refreshBrowser();
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.tabmodel);
                checkFluidModelData("US");
                checkReservoirModelData("US");
                VerifyTrajectoryData(TestData.ESPGLTrajectoryData.dt, "US", PageObjects.WellConfigurationPage.tbltrajectoryheader, "xpath");
                VerifyTubingCasingData(TestData.ESPGLTubingData.dt, "US", PageObjects.WellConfigurationPage.tbltubingheader, "xpath", tbltubinggrid);
                VerifyTubingCasingData(TestData.ESPGLCasingData.dt, "US", PageObjects.WellConfigurationPage.tblcasingheader, "xpath", tblcasinggrid);
                VerifyRestrictionData(null, "US", PageObjects.WellConfigurationPage.tblrestrictionheader, "xpath");
                VerifyTracepointsData(null, "US", PageObjects.WellConfigurationPage.tbltracepointsheader, "xpath");
                VerifyPumpData(TestData.ESPDataPump.dt, "US", PageObjects.WellConfigurationPage.tblpumpheader, "xpath");
                checkMotor_Cable_SeparatorData("US");
                CommonHelper.ChangeUnitSystemUserSetting("Metric");
                SeleniumActions.refreshBrowser();
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.tabmodel);
                checkFluidModelData("Metric");
                checkReservoirModelData("Metric");
                VerifyTrajectoryData(TestData.ESPGLTrajectoryData.dt, "Metric", PageObjects.WellConfigurationPage.tbltrajectoryheader, "xpath");
                VerifyTubingCasingData(TestData.ESPGLTubingData.dt, "Metric", PageObjects.WellConfigurationPage.tbltubingheader, "xpath", tbltubinggrid);
                VerifyTubingCasingData(TestData.ESPGLCasingData.dt, "Metric", PageObjects.WellConfigurationPage.tblcasingheader, "xpath", tblcasinggrid);
                VerifyRestrictionData(null, "Metric", PageObjects.WellConfigurationPage.tblrestrictionheader, "xpath");
                VerifyTracepointsData(null, "Metric", PageObjects.WellConfigurationPage.tbltracepointsheader, "xpath");
                VerifyPumpData(TestData.ESPDataPump.dt, "Metric", PageObjects.WellConfigurationPage.tblpumpheader, "xpath");
                checkMotor_Cable_SeparatorData("Metric");

            }
        }
            public void Deletewell()
            {
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.deletebutton);
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.firstdelete);
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.seconddelete);

            string welldeltext = SeleniumActions.getInnerText(PageObjects.WellConfigurationPage.Toaseter);
            CommonHelper.TraceLine(welldeltext);
            SeleniumActions.takeScreenshot("Well Deletion");
            Assert.AreEqual("Well has been successfully deleted.", welldeltext, "Well Deletion Toast did not appear");
           

        }

    }

    
}