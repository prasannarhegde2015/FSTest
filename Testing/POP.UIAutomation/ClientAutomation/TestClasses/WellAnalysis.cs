using Microsoft.VisualStudio.TestTools.UnitTesting;
using ArtOfTest.WebAii.Controls.HtmlControls;
using ClientAutomation.PageObjects;
using ClientAutomation.TelerikCoreUtils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Data;
using ClientAutomation.Helper;

namespace ClientAutomation.TestClasses
{
    class WellAnalysis
    {
        public static string FilesLocation = ConfigurationManager.AppSettings.Get("FilesLocation");
        public static string IsRunningInATS = ConfigurationManager.AppSettings.Get("IsRunningInATS");
        public WellTestFlow welltest = new WellTestFlow();


        public void AnalysisFlow()
        {
            try
            {
                TelerikObject.InitializeManager();
                if (IsRunningInATS == "false")
                {
                    TelerikObject.select_well();
                }
                CalcualteTorquefromUI();
                Scancards();
                ContinuousCardCollection();
                PatternMatching();
                RunAnalysis();
                VerifyAnalysisReport();
                //********* RRL Analysis Part -2;
                //***Pick up each card and run analysis agains it for generating mini report for that card 
                GenerateReportforEachCard();
                //***Navigate  through card slider to ensur min report is available for Slider
                CardRangeSlider();
                Verify_PumpFillPercentVSDToleranceShutDownLimitsFluidLoadLines();
                CheckUnitBalancing();
                CheckCalibration();
                AnalysisOptions();
                // ***Clean up the System if it is ATS
                CommonHelper.DeleteWell();


            }
            catch (Exception e)
            {
                CommonHelper.PrintScreen("Err_RRLWellAnalysisFlowMain");
                CommonHelper.DeleteWell();
                Assert.Fail(e.ToString());
            }
            finally
            {
                //Dispose Manager to 
                TelerikObject.mgr.Dispose();
            }

        }
        public void GLAnalysisWorkFlow()
        {
            //Create New GL Well using API calls to save on UI creation time ( UI case covered on other test case)
            try
            {
                CommonHelper.CreateGLWellwithFullData("GLWELL_00001.wflx");
                TelerikObject.InitializeManager();
                if (IsRunningInATS == "false")
                {
                    TelerikObject.select_well();
                }
                AnalysisCommon();
                //By default nothing Will Appear on Analysis Page without Welltest
                PageAnalysis.getControlTextGLAnalysis(PageAnalysis.ddtxtWellTestDate, string.Empty, "Test Date Time");
                CommonHelper.TraceLine("**** For No WellTest Case Date is shown blank by default ***");
                TelerikObject.Click(PageDashboard.welltesttab);
                //Add New WellTest for GL
                welltest.AddGLWellTest("GLAnalysisWellTestData.xml");
                TelerikObject.Click(PageDashboard.Analysistesttab);
                TelerikObject.WaitForControlDisappear(new HtmlControl(PageDashboard.page_Loader));
                VerifyGLAnalaysisDefault();
                //***** Go to Well Test Change the Quality Code to Oil Potentail and Rejected , weshould not see any values
                //Verify US English Unit
                #region VerifyGLValveTables
                //    VerifyGlValveTablesCase1();
                #endregion
                //************* Check for WellAnalysis for Different Quality codes of Well Test and expected response on UI
                // For Testdate value is empty means empty page was loaded
                CheckForQualityCodes("Allocatable Test", "Oil Potenital Test", string.Empty);
                CheckForQualityCodes("Oil Potential Test", "Rejected Test", string.Empty);
                CheckForQualityCodes("Rejected Test", "Representative Test", "07/31/2018 02:00 AM");

                TelerikObject.Click(PageDashboard.welltesttab);
                welltest.AddGLWellTest("GLAnalysisWellTestSecondData.xml");
                TelerikObject.Click(PageDashboard.Analysistesttab);
                TelerikObject.WaitForControlDisappear(new HtmlControl(PageDashboard.page_Loader));
                PageAnalysis.GetAndClickToggleSwitchByLabel("Daily Average Data", "OFF");
                //Make Changes in GL AWB Screen and Click on Save Button
                PageAnalysis.GLAnalysisDefaultElementCheck();
                PageAnalysis.setControlTextGLAnalysis(PageAnalysis.txtWellheadPressure, "600", "WellHead Pressure");
                PageAnalysis.setControlTextGLAnalysis(PageAnalysis.txtWellheadTemperature, "180", "WellHead Temperature");
                PageAnalysis.setControlTextGLAnalysis(PageAnalysis.txtCasingHeadPressure, "2250", "Casing Head Pressure");
                PageAnalysis.setControlTextGLAnalysis(PageAnalysis.txtOilRate, "1900", "Oil rate");
                // **************** Verify fucntionality of Save button Click on Save****************
                TelerikObject.Click(PageAnalysis.btnGLAWBsave);
                TelerikObject.WaitForControlDisappear(new HtmlControl(PageDashboard.page_Loader));
                //Check if this value is saved in GL AWB first and then on Welltest Screen for that test
                VerifyGLValues("GLAnalysisSaveData.xml");
                VerifyGLPrevValues("GLAnalysisPreviousWellTestData.xml");
                //         VerifyGlValveTablesCase2();
                NavigateToAndfrom();
                PageAnalysis.GetAndClickToggleSwitchByLabel("Daily Average Data", "OFF");
                //****************** Gradient Curves Charts Check In both US and Metric Units ****************
                CheckGLChartData("US");
                CommonHelper.ChangeUnitSystemUserSetting("Metric");
                NavigateToAndfrom();
                PageAnalysis.GetAndClickToggleSwitchByLabel("Daily Average Data", "OFF");
                CheckGLChartData("Metric");
                CommonHelper.ChangeUnitSystemUserSetting("US");
                NavigateToAndfrom();
                PageAnalysis.GetAndClickToggleSwitchByLabel("Daily Average Data", "OFF");
                //****************Verify New  Button Funtionality****************
                PageAnalysis.setControlTextGLAnalysis(PageAnalysis.txtWellheadPressure, "700", "WellHead Pressure");
                TelerikObject.Click(PageAnalysis.btnGLAWBNew);
                TelerikObject.WaitForControlDisappear(new HtmlControl(PageDashboard.page_Loader));
                //  go to Well Test and Verify if a new row for Well Test has been added with one min added to Current WellTest and all data
                TelerikObject.Click(PageDashboard.welltesttab);
                TelerikObject.WaitForControlDisappear(new HtmlControl(PageDashboard.page_Loader));
                welltest.VerifyGLWellTestGridData("GLAnalysisNewWellTestData.xml");
                TelerikObject.Click(PageDashboard.Analysistesttab);
                //****************  Verify Run Button Funtionality --Calculatiions for FBHP and Liquid Rate would change

                TelerikObject.WaitForControlDisappear(new HtmlControl(PageDashboard.page_Loader));
                PageAnalysis.GetAndClickToggleSwitchByLabel("Daily Average Data", "OFF");
                PageAnalysis.SelectKendoDropdown(PageAnalysis.ddInjectionmethod, "Fixed Depth");
                PageAnalysis.SelectKendoDropdown(PageAnalysis.ddmultiphasecorrleation, "Gray");
                TelerikObject.Click(PageAnalysis.btnGLAWBRun);
                TelerikObject.WaitForControlDisappear(new HtmlControl(PageDashboard.page_Loader));
                VerifyGLValues("GLAnalysisRunData.xml");
                VerifyGLPrevValues("GLAnalysisRunPreviousWellTestData.xml");
                //        VerifyGlValveTablesCase3();
                NavigateToAndfrom();
                PageAnalysis.GetAndClickToggleSwitchByLabel("Daily Average Data", "OFF");
                //Verify that Value is Not saved compare with older value 
                PageAnalysis.getControlTextGLAnalysis(PageAnalysis.ddtxtInjectionMethod, "Deepest Mandrel", "Injection Method");
                PageAnalysis.getControlTextGLAnalysis(PageAnalysis.ddtxtMultiphaseFlowCorrelation, "Beggs and Brill (Modified)", "Multiphase Correlation");
            }
            catch (Exception e)
            {
                CommonHelper.PrintScreen("Err_GLAnalysisFlow");
                CommonHelper.TraceLine("Error " + e);
                Assert.Fail("GL Analysis Test has failed");

            }
            finally
            {
                CommonHelper.DeleteWell();
                TelerikObject.mgr.Dispose();
            }

        }

        public void ESPAnalysisWorkFlow()
        {
            //Create New ESP Well using API calls to save on UI creation time ( UI case covered on other test case)
            try
            {
                CommonHelper.ChangeUnitSystemUserSetting("Metric");
                CommonHelper.CreateESPWellwithFullData("Esp_ProductionTestData.wflx");
                TelerikObject.InitializeManager();

                TelerikObject.select_well();

                AnalysisCommon();
                //By default nothing Will Appear on Analysis Page without Welltest
                PageAnalysis.getControlTextESPAnalysis(PageAnalysis.ddtxtWellTestDate, string.Empty, "Test Date Time");
                // ClientAutomation.Helper.CommonHelper.TraceLine("**** For No WellTest Case Date is shown blank by default ***");
                TelerikObject.Click(PageDashboard.welltesttab);
                //Add New WellTest for ESP
                welltest.AddESPWellTest_Analysis("ESPWellAnalysisTestData.xml");
                TelerikObject.Click(PageDashboard.Analysistesttab);
                TelerikObject.WaitForControlDisappear(new HtmlControl(PageDashboard.page_Loader));
                VerifyESPAnalaysisDefault();
                //***** Go to Well Test Change the Quality Code to Oil Potentail and Rejected , weshould not see any values
                //Verify US English Unit

                //************* Check for WellAnalysis for Different Quality codes of Well Test and expected response on UI
                // For Testdate value is empty means empty page was loaded
                Thread.Sleep(2000);
                CheckForQualityCodes("Allocatable Test", "Oil Potenital Test", string.Empty);
                CheckForQualityCodes("Oil Potential Test", "Rejected Test", string.Empty);
                CheckForQualityCodes("Rejected Test", "Representative Test", "02/04/2019 05:30 AM");



                CommonHelper.ChangeUnitSystemUserSetting("US");
                NavigateToAndfrom();
                TelerikObject.Click(PageDashboard.optimizationtab);
                TelerikObject.Click(PageDashboard.Analysistesttab);
                PageAnalysis.GetAndClickToggleSwitchByLabel("Daily Average Data", "OFF");
                //toremove


                //****************** Gradient Curves Charts Check In both US and Metric Units ****************
                Thread.Sleep(4000);
                CheckESPChartData("US");
                CommonHelper.ChangeUnitSystemUserSetting("Metric");
                TelerikObject.mgr.ActiveBrowser.Refresh();
                PageAnalysis.GetAndClickToggleSwitchByLabel("Daily Average Data", "OFF");
                Thread.Sleep(4000);
                CheckESPChartData("Metric");
                CommonHelper.ChangeUnitSystemUserSetting("US");
                NavigateToAndfrom();
                //PageAnalysis.GetAndClickToggleSwitchByLabel("Daily Average Data", "OFF");
                //****************Verify New  Button Funtionality****************
                PageAnalysis.setControlTextESPAnalysis(PageAnalysis.txtWellheadPressure, "700", "WellHead Pressure");
                TelerikObject.Click(PageAnalysis.btnGLAWBNew);
                TelerikObject.WaitForControlDisappear(new HtmlControl(PageDashboard.page_Loader));
                //  go to Well Test and Verify if a new row for Well Test has been added with one min added to Current WellTest and all data
                TelerikObject.Click(PageDashboard.welltesttab);
                //   TelerikObject.WaitForControlDisappear(new HtmlControl(PageDashboard.page_Loader));

                welltest.VerifyESPWellTestGridData_Analysis("ESPWellAnalysisNewTestData.xml");
                TelerikObject.Click(PageDashboard.Analysistesttab);
                //****************  Verify Run Button Funtionality --Calculatiions for FBHP and Liquid Rate would change

                //  TelerikObject.WaitForControlDisappear(new HtmlControl(PageDashboard.page_Loader));
                PageAnalysis.GetAndClickToggleSwitchByLabel("Daily Average Data", "OFF");
                PageAnalysis.setControlTextESPAnalysis(PageAnalysis.txtWellheadPressure, "800.00", "WellHead Pressure");

                TelerikObject.Click(PageAnalysis.btnGLAWBRun);
                TelerikObject.WaitForControlDisappear(new HtmlControl(PageDashboard.page_Loader));


                VerifyESPValuesdata("ESPAnalysisRunTestData.xml");
                VerifyESPPrevValues("ESPAnalysisPreviousRunTestData.xml");

                NavigateToAndfrom();
                //PageAnalysis.GetAndClickToggleSwitchByLabel("Daily Average Data", "OFF");



                //Make Changes in GL AWB Screen and Click on Save Button
                PageAnalysis.ESPAnalysisDefaultElementCheck();
                PageAnalysis.setControlTextESPAnalysis(PageAnalysis.txtWellheadPressure, "600", "WellHead Pressure");
                PageAnalysis.setControlTextESPAnalysis(PageAnalysis.txtWellheadTemperature, "180", "WellHead Temperature");


                // **************** Verify fucntionality of Save button Click on Save****************
                TelerikObject.Click(PageAnalysis.btnGLAWBsave);
                // TelerikObject.WaitForControlDisappear(new HtmlControl(PageDashboard.page_Loader));
                TelerikObject.mgr.ActiveBrowser.Refresh();
                TelerikObject.Click(PageDashboard.configurationtab);
                Thread.Sleep(1000);
                TelerikObject.Click(PageDashboard.wellConfigurationtab);
                TelerikObject.WaitForControlDisappear(new HtmlControl(PageDashboard.page_Loader));
                TelerikObject.Click(PageWellConfig.btnSettings);
                TelerikObject.Click(PageWellConfig.txt_Performancecurves);

                TelerikObject.Click(PageWellConfig.btnAdd);
                TelerikObject.Sendkeys(PageWellConfig.txt_frequency, "5");
                TelerikObject.Click(PageWellConfig.btn_frequencysave);
                TelerikObject.mgr.ActiveBrowser.Refresh();
                Thread.Sleep(3000);
                TelerikObject.Click(PageDashboard.optimizationtab);
                TelerikObject.Click(PageDashboard.Analysistesttab);
                PageAnalysis.GetAndClickToggleSwitchByLabel("Daily Average Data", "OFF");
                TelerikObject.Click(PageAnalysis.tabPerformanceCurves);
                Thread.Sleep(1000);
                PageAnalysis.ESPcheckstaticlegendtext("Operating Frequency: 5");
                TelerikObject.Click(PageDashboard.configurationtab);
                Thread.Sleep(1000);
                TelerikObject.Click(PageDashboard.wellConfigurationtab);


            }
            catch (Exception e)
            {
                CommonHelper.PrintScreen("Err_GESPAnalysisFlow");
                CommonHelper.TraceLine(e.Message);
                // ClientAutomation.Helper.CommonHelper.TraceLine("Error " + e);
                Assert.Fail("ESP Analysis Test has failed");

            }
            finally
            {
                CommonHelper.DeleteWell();
                TelerikObject.mgr.Dispose();
            }

        }



        private void Scancards()
        {
            try
            {
                AnalysisCommon();
                TelerikObject.Sendkeys(PageAnalysis.startDate, "01012008", false);
                Thread.Sleep(1000);
                //TelerikObject.Click(PageAnalysis.btnClearCards);

                PageAnalysis.PreCardCollectionCheck();
                VerifyCardDescriptionLabelText(PageAnalysis.lstItemCurrentCard, "Current Card");
                VerifyCardDescriptionLabelText(PageAnalysis.lstItemFullCard, "Full Card");
                VerifyCardDescriptionLabelText(PageAnalysis.lstItemPumpoffCard, "Pumpoff Card");
                VerifyCardDescriptionLabelText(PageAnalysis.lstItemAlarmCard, "Alarm Card");
                VerifyCardDescriptionLabelText(PageAnalysis.lstItemFailureCard, "Failure Card");
                TelerikObject.Sendkeys(PageAnalysis.startDate, "01012008", false);
                Thread.Sleep(2000);
                PageAnalysis.PostCardCollectionCheck();
            }
            catch (Exception e)
            {
                CommonHelper.PrintScreen("Error_ScanCards");
                Thread.Sleep(2000);
                Assert.Fail("Failure  in ScanCards" + e.ToString());
            }

        }

        private void CheckForQualityCodes(string oldcode, string newcode, string exp)
        {

            TelerikObject.Click(PageDashboard.welltesttab);
            TelerikObject.WaitForControlDisappear(new HtmlControl(PageDashboard.page_Loader));
            TelerikObject.editGridCells("Quality Code", oldcode, newcode);
            TelerikObject.Click(PageWellTest.btnUpdateWellTest);
            TelerikObject.Toastercheck(PageWellTest.toasterControl, "Well Test Update", "Updated successfully.");
            TelerikObject.Click(PageDashboard.Analysistesttab);
            PageAnalysis.GetAndClickToggleSwitchByLabel("Daily Average Data", "OFF");
            //Check if TestDate Time is empty meaning nothing appears on UI 
            PageAnalysis.getControlTextGLAnalysis(PageAnalysis.ddtxtWellTestDate, exp, "Test Date Time");
            if (exp != string.Empty)
            {
                //  newcode = newcode.Replace(" ", "");
                PageAnalysis.getControlTextGLAnalysis(PageAnalysis.txtQualitycode, newcode, "Quality Code");
            }

        }

        private void VerifyGlValveTablesCase3()
        {
            string colnames = "MD(ft);TVD(ft);Status;Manufacturer;Model;Port Size(64th in);R;PTRO(psia);Pt(psia);Pc(psia);T(°F);Pto(psia);Pco(psia);Qgi(Mscf/d)";
            string colvas = "7598.55;7535.25;NA;None;Orifice;18;0;0;1557.8;2649.64;237.9;0;0;2900.49";
            TelerikObject.verifyGridCellValuesSingleTable("Valve", "1", 0);
            TelerikObject.verifyGridCellValuesSingleTable(colnames, colvas, 0);
        }

        private void VerifyGlValveTablesCase2()
        {
            string colnames = "MD(ft);TVD(ft);Status;Manufacturer;Model;Port Size(64th in);R;PTRO(psia);Pt(psia);Pc(psia);T(°F);Pto(psia);Pco(psia);Qgi(Mscf/d)";
            string colvas = "7598.55;7535.25;NA;None;Orifice;18;0;0;1294.62;2649.64;237.9;0;0;2900.49";
            TelerikObject.verifyGridCellValuesSingleTable("Valve", "1", 0);
            TelerikObject.verifyGridCellValuesSingleTable(colnames, colvas, 0);
            CommonHelper.ChangeUnitSystemUserSetting("Metric");
            NavigateToAndfrom();

            //Verify Metric Unit Values
            colnames = "MD(m);TVD(m);Status;Manufacturer;Model;Port Size(mm);R;PTRO(kPa);Pt(kPa);Pc(kPa);T(°C);Pto(kPa);Pco(kPa);Qgi(sm³/d)";
            colvas = "2316.04;2296.74;NA;None;Orifice;7.1;0;0;8926.1;18268.63;114.4;0;0;82132.73";
            TelerikObject.verifyGridCellValuesSingleTable("Valve", "1", 0);
            TelerikObject.verifyGridCellValuesSingleTable(colnames, colvas, 0);
            CommonHelper.ChangeUnitSystemUserSetting("US");
        }
        private void VerifyGlValveTablesCase1()
        {
            string colnames = "MD(ft);TVD(ft);Status;Manufacturer;Model;Port Size(64th in);R;PTRO(psia);Pt(psia);Pc(psia);T(°F);Pto(psia);Pco(psia);Qgi(Mscf/d)";
            string colvas = "7598.55;7535.25;NA;None;Orifice;18;0;0;1303.38;2590.85;237.9;0;0;2900.49";
            TelerikObject.verifyGridCellValuesSingleTable("Valve", "1", 0);
            TelerikObject.verifyGridCellValuesSingleTable(colnames, colvas, 0);
            CommonHelper.ChangeUnitSystemUserSetting("Metric");
            NavigateToAndfrom();
            PageAnalysis.GetAndClickToggleSwitchByLabel("Daily Average Data", "OFF");
            //Verify Metric Unit Values
            colnames = "MD(m);TVD(m);Status;Manufacturer;Model;Port Size(mm);R;PTRO(kPa);Pt(kPa);Pc(kPa);T(°C);Pto(kPa);Pco(kPa);Qgi(sm³/d)";
            colvas = "2316.04;2296.74;NA;None;Orifice;7.1;0;0;8986.48;17863.27;114.4;0;0;82132.73";
            TelerikObject.verifyGridCellValuesSingleTable("Valve", "1", 0);
            TelerikObject.verifyGridCellValuesSingleTable(colnames, colvas, 0);
            CommonHelper.ChangeUnitSystemUserSetting("US");
        }

        private void CheckGLChartData(string uomtext)

        {
            TelerikObject.Click(PageAnalysis.tabGradientCurves);
            PageAnalysis.GLcheckGradientcurvesLegend();
            PageAnalysis.GLcheckGradientCurvesAxislables(uomtext);
            CommonHelper.PrintScreen("Gradient Curves Chart" + uomtext);

            //*************** Performance Charts Check***************
            TelerikObject.Click(PageAnalysis.tabPerformanceCurves);
            TelerikObject.WaitForControlDisappear(new HtmlControl(PageDashboard.page_Loader));
            PageAnalysis.GLcheckPerformanceCurvesLegend(uomtext);
            PageAnalysis.GLcheckPerformanceCurvesAxislables(uomtext);
            CommonHelper.PrintScreen("Performance Curves Chart" + uomtext);
            //*************** Inflow/OutFlow Charts Check**************
            TelerikObject.Click(PageAnalysis.tabInflowOutflowCurves);
            TelerikObject.WaitForControlDisappear(new HtmlControl(PageDashboard.page_Loader));
            PageAnalysis.GLcheckInflowOutflowCurvesLegend();
            PageAnalysis.GLcheckInflowOutflowCurvesAxislables(uomtext);
            CommonHelper.PrintScreen("Inflow_OutFlowCurves Chart" + uomtext);
        }

        private void CheckESPChartData(string uomtext)

        {
            TelerikObject.Click(PageAnalysis.tabGradientCurves);
            PageAnalysis.ESPcheckGradientcurvesLegend();
            PageAnalysis.ESPcheckGradientCurvesAxislables(uomtext);
            CommonHelper.PrintScreen("Gradient Curves Chart" + uomtext);

            //*************** Performance Charts Check***************
            TelerikObject.Click(PageAnalysis.tabPerformanceCurves);
            Thread.Sleep(1000);
            TelerikObject.WaitForControlDisappear(new HtmlControl(PageDashboard.page_Loader));
            PageAnalysis.ESPcheckPerformanceCurvesLegend(uomtext);
            PageAnalysis.ESPcheckPerformanceCurvesAxislables(uomtext);
            CommonHelper.PrintScreen("Performance Curves Chart" + uomtext);
            //*************** Inflow/OutFlow Charts Check**************
            Thread.Sleep(4000);

            try
            {
                TelerikObject.Click(PageAnalysis.tabInflowOutflowCurves);
                TelerikObject.mgr.ActiveBrowser.Refresh();
                PageAnalysis.GetAndClickToggleSwitchByLabel("Daily Average Data", "OFF");
                TelerikObject.Click(PageAnalysis.tabInflowOutflowCurves);
                TelerikObject.WaitForControlDisappear(new HtmlControl(PageDashboard.page_Loader));
                PageAnalysis.ESPcheckInflowOutflowCurvesLegend();
                PageAnalysis.ESPcheckInflowOutflowCurvesAxislables(uomtext);


                CommonHelper.PrintScreen("Inflow OutFlowCurves Chart" + uomtext);
            }
            catch (Exception e)
            {
                CommonHelper.TraceLine(e.Message);
            }


            //*************** Pump Charts Check**************
            Thread.Sleep(3000);
            TelerikObject.Click(PageAnalysis.tabPumpCurves);

            TelerikObject.WaitForControlDisappear(new HtmlControl(PageDashboard.page_Loader));
            PageAnalysis.ESPcheckPumpCurvesLegend();
            PageAnalysis.ESPcheckPumpCurvesAxislables(uomtext);
            CommonHelper.PrintScreen("Pump Curves Chart" + uomtext);
            //*************** Gassiness Charts Check**************
            TelerikObject.Click(PageAnalysis.tabbGasinessCurves);
            Thread.Sleep(3000);
            TelerikObject.WaitForControlDisappear(new HtmlControl(PageDashboard.page_Loader));
            PageAnalysis.ESPcheckGassinessCurvesLegend();
            PageAnalysis.ESPcheckGassinessCurvesAxislables(uomtext);
            CommonHelper.PrintScreen("Gassiness Curves Chart" + uomtext);




        }



        private void CalcualteTorquefromUI()
        {

            TelerikObject.Click(PageDashboard.configurationtab);
            TelerikObject.Click(PageDashboard.wellConfigurationtab);
            Thread.Sleep(15000);//application is really slow here even for zero well for first time
            TelerikObject.Click(PageWellConfig.tabWeights);
            Thread.Sleep(2000);
            TelerikObject.Click(PageWellConfig.btnCalculate);
            TelerikObject.Click(PageWellConfig.btn_saveWell);
            Thread.Sleep(15000);//application is really slow here even for zero well for first time
            TelerikObject.Click(PageDashboard.configurationtab);
        }
        private void DeleteRRLWell()
        {
            TelerikObject.Click(PageDashboard.configurationtab);
            TelerikObject.Click(PageDashboard.wellConfigurationtab);
            TelerikObject.Click(PageWellConfig.btnDeleteWell);
            Thread.Sleep(2000);
            TelerikObject.Click(PageWellConfig.btnConfirmDeleteWell);
            Thread.Sleep(2000);
            TelerikObject.Click(PageWellConfig.btnConfirmDeleteWell);
            TelerikObject.WaitForControlDisappear(new HtmlControl(PageDashboard.page_Loader));
            TelerikObject.DoStaticWait();
            string wellcount = PageDashboard.WellCounter.BaseElement.InnerText;
            if (wellcount == "0")
            {
                CommonHelper.TraceLine("Well was deleted and Wel counter is " + wellcount);
            }
            else
            {
                CommonHelper.TraceLine("Well was NOT  deleted and Wel counter is " + wellcount);
            }
            //   TelerikObject.Toastercheck(PageAnalysis.toasterControl, "Delete Well", "Well has been successfully deleted.");
            CommonHelper.TraceLine("RRL Well was deleted");

        }
        private void ContinuousCardCollection()
        {

            try
            {
                Thread.Sleep(5000);
                TelerikObject.Click(PageAnalysis.btnScanCards);
                TelerikObject.Click(PageAnalysis.lstItemCardCollection);
                TelerikObject.Sendkeys(PageAnalysis.txtCollectionInterval, "5");
                TelerikObject.Sendkeys(PageAnalysis.txtNumberofcards, "20");
                TelerikObject.Sendkeys(PageAnalysis.txtCommFail, "2");
                TelerikObject.Click(PageAnalysis.btnStart);
                // TelerikObject.Toastercheck(PageAnalysis.toasterControl, "Continuous Card Collection", "Continuous card collection started.");
                //  TelerikObject.Click(PageAnalysis.lblStartDate);

                //If Keep waiting for Toaster the Message, Collection card msg gets passed away quickly So No check for toaster
                for (int k = 0; k < 20; k++)
                {

                    PageAnalysis.DynamicValue = "Cards Collected: " + k;
                    CommonHelper.TraceLine("*****Looking for Card Collection Text: ******** " + "Cards Collected: " + k);
                    Assert.IsNotNull(PageAnalysis.divCardCollectionStatus, "Card Collection Status is not null");
                    CommonHelper.TraceLine("Card Collection text is : " + "Cards Collected: " + k);
                    string comfailtxt = "Comms Fail: 0";

                    //do this check only till last but one card Since telerik is not fast enough to find them before they disappear.
                    if (k < 19)
                    {
                        PageAnalysis.DynamicValue = comfailtxt;
                        Assert.IsNotNull(PageAnalysis.divCardCollectionStatus, "Com Fail Status Did not show on UI");
                        CommonHelper.TraceLine("Com Fail Status was shown on UI");
                        Assert.IsNotNull(PageAnalysis.btnStopCardcollection, "Stop Button for Card collection was not shown");
                        CommonHelper.TraceLine("Stop Button was shown on UI");
                    }
                    if (k == 19)
                    {
                        CommonHelper.PrintScreen("Card Collection Final");
                    }

                }
                // this Delay was added to handle Card count mismatch
                //Autoamtion Need to wait as ForeSite Cygnet Handshake takes more time than expected and update happens after a while
                Thread.Sleep(10000);
                TelerikObject.Click(PageAnalysis.btncardLibrary);
                Thread.Sleep(15000);
                PageAnalysis.VerifyCardLibraryCount("Post Continuous Card collection", 25);

            }
            catch (Exception e)
            {
                CommonHelper.PrintScreen("Error_ContinuousCardCollection");
                Thread.Sleep(2000);
                Assert.Fail("Failure  in ContinuousCardCollection" + e.ToString());
            }
        }

        private void PatternMatching()
        {
            try
            {


                //Clear Cards Collection and Draw a Single Current Card
                TelerikObject.Click(PageAnalysis.btnClearCards);
                Thread.Sleep(1000);
                string minirerpotData = "Mini Report data not available.";
                Assert.AreEqual(minirerpotData, PageAnalysis.divMiniReportpane.BaseElement.InnerText, "Mismatch in Mini report Text");
                CommonHelper.TraceLine("Mini Report was Succesfully matched for No Cards condition.");
                TelerikObject.Click(PageAnalysis.btncardLibrary);
                TelerikObject.WaitForControlDisappear(new HtmlControl(PageDashboard.page_Loader));
                HtmlTable cardtbl = TelerikObject.getTableByColumnName("Current");
                TelerikObject.Click(cardtbl.Rows[0]);
                TelerikObject.WaitForControlDisappear(new HtmlControl(PageDashboard.page_Loader));
                TelerikObject.Click(PageAnalysis.btnpatternMatching);
                TelerikObject.WaitForControlDisappear(new HtmlControl(PageDashboard.page_Loader));
                //Verify Card Pattern Description if it is correct:
                string colnames = "Overall Match;Description;Surface Match;Downhole Match";
                string testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\TestData", "RRLPatternMatching.xml");
                CommonHelper.TraceLine("Getting Data from XML file ... " + testdatafile);
                DataTable dt = CommonHelper.BuildDataTableFromXml(testdatafile);
                int i = 0;
                //string NC = "IgnoreValue";
                HtmlTable resultsTable = TelerikObject.getTableByColumnName(dt.Rows[0]["Description"].ToString());
                CommonHelper.TraceLine("Records count in Actual Table =" + resultsTable.Rows.Count);
                Assert.AreEqual(dt.Rows.Count, resultsTable.Rows.Count, "Expected and Actaul Table records mismatch for Results tab of Pattern Library");

                foreach (DataRow drow in dt.Rows)
                {
                    string overallmatchvalue = Math.Round(Convert.ToDouble(dt.Rows[i]["OverallMatch"].ToString()) * 100, 0).ToString() + "%";
                    string descriptionvalue = dt.Rows[i]["Description"].ToString();
                    string surfacematchvalue = Math.Round(Convert.ToDouble(dt.Rows[i]["SurfaceMatch"].ToString()) * 100, 0).ToString() + "%";
                    string downholematchvalue = Math.Round(Convert.ToDouble(dt.Rows[i]["DownholeMatch"].ToString()) * 100, 0).ToString() + "%";
                    string colvals = overallmatchvalue + ";" + descriptionvalue + ";" + surfacematchvalue + ";" + downholematchvalue;
                    TelerikObject.verifyGridCellValues(colnames, colvals, i);
                    i++;
                }

                CommonHelper.TraceLine("******** Table Verification of Results Tab of Patern Matching was Succesfully completed ********* ");
                //Adding Collected Card to Library to see if card count got incremented..
                TelerikObject.Click(PageAnalysis.tabLibraryPatternCardMatch);
                TelerikObject.WaitForControlDisappear(new HtmlControl(PageDashboard.page_Loader));
                HtmlTable pattrenCardcardLibraryTable = TelerikObject.getTableByColumnName("125 ms shut down");
                CommonHelper.TraceLine("Table count before adding Pattern card :" + pattrenCardcardLibraryTable.Rows.Count);
                int bcount = pattrenCardcardLibraryTable.Rows.Count;
                TelerikObject.Click(PageAnalysis.tabResultsPatternCardMatch);
                TelerikObject.Click(PageAnalysis.btnSaveCollectedcardToLibrary);
                TelerikObject.Sendkeys(PageAnalysis.txtPatternCardDescription, "SampleCardSaved");
                TelerikObject.Click(PageAnalysis.btnSaveOnDialogForCardcollection);
                TelerikObject.Toastercheck(PageAnalysis.toasterControl, "Check for Pattern Card Saving", "Card Saved Successfully.");

                TelerikObject.Click(PageAnalysis.tabLibraryPatternCardMatch);
                TelerikObject.WaitForControlDisappear(new HtmlControl(PageDashboard.page_Loader));
                //Check if the count inceremented by one
                HtmlTable pattrenCardcardLibraryTable_1 = TelerikObject.getTableByColumnName("125 ms shut down");
                Thread.Sleep(2000);
                int acount = pattrenCardcardLibraryTable_1.Rows.Count;
                CommonHelper.TraceLine("Table rows  count after adding Pattern card :" + pattrenCardcardLibraryTable_1.Rows.Count);
                Assert.AreEqual(bcount + 1, acount, "Pattern Card was not saved in Pateern Card Library");
                CommonHelper.TraceLine("****Pattern Card was saved in Pattern Card Library******");
                PageAnalysis.DynamicValue = "SampleCardSaved";
                PageAnalysis.htmlcell_AnalsyisTable.Click();
                TelerikObject.WaitForControlDisappear(new HtmlControl(PageDashboard.page_Loader));
                //**********verify Update Button Functionality**********
                TelerikObject.Click(PageAnalysis.btnUpdatePatternCardSelected);
                PageAnalysis.chkbox_CompareSurfaceCard.Check(true);
                PageAnalysis.chkbox_CompareDownholeCard.Check(true);
                TelerikObject.Click(PageAnalysis.btnModifyPatternCardSelected);
                TelerikObject.Toastercheck(PageAnalysis.toasterControl, "Card Deleletion cehck for Pattern Card", "Card Updated Successfully.");
                Thread.Sleep(5000);
                string compareoption = PageAnalysis.htmlcell_AnalsyisTable.BaseElement.GetNextSibling().InnerText;
                Assert.AreEqual("Surface, Downhole", compareoption, "Pattern card optiosn were not updated");
                CommonHelper.TraceLine("[**** checkpoint***]Update Saved Library card options is working fine ");
                PageAnalysis.htmlcell_AnalsyisTable.Click();
                //**********verify Delete Button Functionality**********
                Thread.Sleep(5000);
                TelerikObject.Click(PageAnalysis.btnDeletePatternCardSelected);
                string confirmtext = "Are you sure you want to delete the selected Pattern Library card listed below?  Deleting the data is not recoverable.";
                PageAnalysis.DynamicValue = confirmtext;
                Assert.IsNotNull(PageAnalysis.paraConfirmTextforCardDelete, "Confirmation Message did not show up for card Deleletion");
                CommonHelper.TraceLine("****Conformation Dialog was shown on clicking delete");

                TelerikObject.Click(PageAnalysis.btnConfirmDeleteCardFromLibrary);
                TelerikObject.Toastercheck(PageAnalysis.toasterControl, "Card Deleletion cehck for Pattern Card", "Card Deleted Successfully.");
                TelerikObject.WaitForControlDisappear(new HtmlControl(PageDashboard.page_Loader));
                pattrenCardcardLibraryTable_1 = TelerikObject.getTableByColumnName("125 ms shut down");
                pattrenCardcardLibraryTable_1.Refresh();
                Thread.Sleep(1000);
                int acount2 = pattrenCardcardLibraryTable_1.Rows.Count;
                CommonHelper.TraceLine("Table records count after deleting card library: " + acount2);
                Assert.AreEqual(acount - 1, acount2, "Pattern Card was not Deleted from Card Library");
                CommonHelper.TraceLine("Pattern Card was Deleted from Card Library");

            }
            catch (Exception e2)
            {
                CommonHelper.TraceLine("Exception from  Pattern Library match" + e2);
                Assert.Fail("Failure in Run Pattern Matching");
            }


        }

        private void RunAnalysis()
        {
            try
            {

                //Call Add well test from UI to get Downhole Calcualtions Card...
                Thread.Sleep(2000);
                WellTestFlow welltest = new WellTestFlow();
                TelerikObject.Click(PageDashboard.welltesttab);
                TelerikObject.WaitForControlDisappear(new HtmlControl(PageDashboard.page_Loader));
                welltest.AddNewRRLWellTest("RRLWellTestData.xml");
                Thread.Sleep(20000);//very slow on Dev machine the swithcing of Tabs on UI for 1 well
                TelerikObject.Click(PageDashboard.Analysistesttab);
                TelerikObject.WaitForControlDisappear(new HtmlControl(PageDashboard.page_Loader));
                TelerikObject.Click(PageAnalysis.btnrunAnalysis);
                TelerikObject.WaitForControlDisappear(new HtmlControl(PageDashboard.page_Loader));

                CommonHelper.TraceLine("Mini Report Text is : " + PageAnalysis.divMiniReportpane.BaseElement.InnerText);
                string minirerpotData = "L-C-57.00(1052)-109(224)-48(90.0)@11.0 SPM w/3.25" +
                                        "Stresses:D-1.000(107)N-78-0.875(106)EL-0.750(54)" +
                                        "RT (6.8)@246% PE80% PF";

                Assert.AreEqual(minirerpotData, PageAnalysis.divMiniReportpane.BaseElement.InnerText, "Mismatch in Mini report Text");
                CommonHelper.TraceLine("Mini Report was Succesfully matched");
            }
            catch (Exception e)
            {
                CommonHelper.PrintScreen("Error_RunAnalysis");
                Thread.Sleep(2000);
                Assert.Fail("Failure  in RunAnalysis" + e.ToString());
            }
        }

        private void AnalysisOptions()
        {
            try
            {
                select_FirstCard();
                TelerikObject.Click(PageAnalysis.btnanalysisOptions);

                TelerikObject.Select_HtmlSelect(PageAnalysis.dd_AnalysisMehtod, "Both", true, false);
                TelerikObject.Select_HtmlSelect(PageAnalysis.dd_FluidLelvel, "Use Fixed Fluid Level", true, false);
                TelerikObject.Select_HtmlSelect(PageAnalysis.dd_DampingMehtod, "Use Fixed Single Damping", true, false);
                Thread.Sleep(3000);
                TelerikObject.Sendkeys(PageAnalysis.txtFluidLevel, "2");
                TelerikObject.Sendkeys(PageAnalysis.txtDampigFactor, "0.30");
                TelerikObject.Click(PageAnalysis.btnSaveAnlysisOption);
                TelerikObject.Toastercheck(PageAnalysis.toasterControl, "Save Analysis Options", "Analysis Options saved successfully.");
                TelerikObject.Click(PageAnalysis.btnRerunAnalysis);
                TelerikObject.Click(PageAnalysis.btnConfirmRerunAnalysis);
                Thread.Sleep(4000);
                TelerikObject.Click(PageAnalysis.btnanalysisReportBtn);
                Thread.Sleep(3000);
                //Ensure Report got Generated before proceeding 
                Assert.IsNotNull(PageAnalysis.btnDownoadAnalysisReportbutton, "Report not generated");
                Thread.Sleep(5000);
                HtmlControl lbldhmethod = TelerikObject.GetElement("htmlcontrol", "content", "Downhole Analysis Method:", "Field Value for: -- " + "Downhole Analysis Method");
                Assert.AreEqual("Downhole Analysis Method:Everitt Jennings", lbldhmethod.BaseElement.Parent.InnerText, "Default Calculation Method is Not Everitt Jennings");
                HtmlControl lbldampingfactor = TelerikObject.GetElement("htmlcontrol", "content", "Damping Factor:", "Field Value for: -- " + "Damping Factor:");
                Assert.AreEqual("Damping Factor:0.30", lbldampingfactor.BaseElement.Parent.InnerText, "Damping Factor is not : 0.30 [Expected]");
                CommonHelper.TraceLine("Damping Factor Input is Reflected in Analysis  Report Calculation");
            }
            catch (Exception e)
            {
                CommonHelper.PrintScreen("Error_Analysis Options");
                Thread.Sleep(2000);
                Assert.Fail("Failure  in Analysis Options" + e.ToString());
            }
        }

        private void CardRangeSlider()
        {
            int assertfailcount = 0;
            try
            {

                Thread.Sleep(2000);
                DateTime dttoday = DateTime.Today;
                TelerikObject.Sendkeys(PageAnalysis.startDate, "01012008", false);
                Thread.Sleep(2000);
                //Get card library array in a list  In Memory collection
                TelerikObject.Click(PageAnalysis.btncardLibrary);
                Thread.Sleep(5000);
                HtmlTable cardtable = TelerikObject.getTableByColumnName("Current");
                int tblrecordcount = cardtable.Rows.Count;
                #region CreateTuple
                List<Tuple<int, string, string>> cardCollectionData = new List<Tuple<int, string, string>>();
                for (int i = 0; i < tblrecordcount; i++)
                {
                    cardCollectionData.Add(new Tuple<int, string, string>(i + 1, cardtable.Rows[i].Cells[0].InnerText, cardtable.Rows[i].Cells[1].InnerText));
                }
                //Clear Cards ********
                #endregion
                TelerikObject.Click(PageAnalysis.btnClearCards);
                Thread.Sleep(2000);
                TelerikObject.Click(PageAnalysis.btndynagraphShowHide);
                PageAnalysis.chkboxCardRangeSlider.Check(true);
                Thread.Sleep(8000);
                HtmlInputRange rng = PageAnalysis.slider_CardRange;
                CommonHelper.TraceLine("table rec count " + tblrecordcount);
                rng.ScrollToVisible();
                TelerikObject.Click(PageAnalysis.XaxisLabel);
                rng.MouseHover();
                string minirerpotDataCurrent = "L-C-57.00(1052)-109(224)-48(90.0)@11.0 SPM w/3.25" +
                                     "Stresses:D-1.000(107)N-78-0.875(106)EL-0.750(54)" +
                                     "RT (6.8)@246% PE80% PF";
                string minirerpotDataFull = "L-C-57.00(1015)-109(217)-48(90.0)@11.0 SPM w/3.25" +
                               "Stresses:D-1.000(99)N-78-0.875(96)EL-0.750(54)" +
                               "RT (6.8)@199% PE97% PF";
                string minirerpotDataPumpOff = "L-C-57.00(1050)-109(224)-48(90.0)@11.0 SPM w/3.25" +
                               "Stresses:D-1.000(109)N-78-0.875(107)EL-0.750(56)" +
                               "RT (6.8)@278% PE73% PF";
                string minirerpotDataAlarm = "L-C-57.00(1022)-109(217)-48(90.0)@11.0 SPM w/3.25" +
                               "Stresses:D-1.000(100)N-78-0.875(98)EL-0.750(54)" +
                               "RT (6.8)@204% PE96% PF";

                string minirerpotDataFailure = "L-C-57.00(1015)-109(216)-48(90.0)@11.0 SPM w/3.25" +
                              "Stresses:D-1.000(99)N-78-0.875(96)EL-0.750(54)" +
                              "RT (6.8)@200% PE97% PF";



                for (int k = tblrecordcount - 1; k >= 0; k--)
                {
                    string expMiniReport = "";
                    switch (cardCollectionData[k].Item2.ToString().ToUpper())
                    {
                        case "CURRENT":
                            {
                                expMiniReport = minirerpotDataCurrent;
                                break;
                            }
                        case "FULL":
                            {
                                expMiniReport = minirerpotDataFull;
                                break;
                            }
                        case "PUMPOFF":
                            {
                                expMiniReport = minirerpotDataPumpOff;
                                break;
                            }
                        case "ALARM":
                            {
                                expMiniReport = minirerpotDataAlarm;
                                break;
                            }
                        case "FAILURE":
                            {
                                expMiniReport = minirerpotDataFailure;
                                break;
                            }
                    }
                    CommonHelper.TraceLine("Card Library Serial Number :" + cardCollectionData[k].Item1);
                    CommonHelper.PrintScreen("CardSlider" + cardCollectionData[k].Item1);
                    //Default is Last Card from Card History:
                    Thread.Sleep(1000);
                    string cardtypeandtimestamp = TelerikObject.GetChildrenControl(PageAnalysis.lblDonwholeControllerCardCollection, "0").BaseElement.InnerText;
                    string cardtypetext = TelerikObject.GetChildrenControl(PageAnalysis.lblDonwholeControllerCardCollection, "1").BaseElement.InnerText;
                    CommonHelper.TraceLine("Card Time Stamp: " + cardtypeandtimestamp);
                    CommonHelper.TraceLine("Card Type: " + cardtypetext);
                    CommonHelper.TraceLine("Card Library Type :" + cardCollectionData[k].Item2);
                    CommonHelper.TraceLine("Card Library TimeStamp:" + cardCollectionData[k].Item3);

                    // Compare Card Type and Time Stamp of Navigated Card from Div Text
                    string[] timestamp = getDateandTimeFromString(cardtypeandtimestamp);
                    string cardtype = cardtypeandtimestamp.Replace(timestamp[0] + " " + timestamp[1], "").Trim();

                    try
                    {
                        comparetimestamp(cardCollectionData[k].Item3, timestamp[0] + " " + timestamp[1]);
                        VerifyLegendTextonChartForSlider(cardCollectionData[k].Item2);
                        Assert.AreEqual(cardCollectionData[k].Item2, cardtype, "Card Slider Navigation did not match with Card Library");
                        Assert.AreEqual(expMiniReport, PageAnalysis.divMiniReportpane.BaseElement.InnerText, "Mismatch in Mini report Text for " + cardCollectionData[k].Item1 + " " + cardCollectionData[k].Item2 + " " + cardCollectionData[k].Item3);
                        CommonHelper.TraceLine("*********** Mini report Data was matched  for:  " + cardCollectionData[k].Item1 + " " + cardCollectionData[k].Item2 + " " + cardCollectionData[k].Item3);
                        CommonHelper.TraceLine("*********** Card Slider Navigation successfully  matched with Card Library for:  " + cardCollectionData[k].Item1 + " " + cardCollectionData[k].Item2 + " " + cardCollectionData[k].Item3);
                    }
                    catch (AssertFailedException ase)
                    {
                        CommonHelper.TraceLine("*********** Card Slider Navigation was NOT matched with Card Library for:  " + cardCollectionData[k].Item1 + " " + cardCollectionData[k].Item2 + " " + cardCollectionData[k].Item3);
                        CommonHelper.TraceLine("************Assert Failed ***********" + ase);
                        assertfailcount++;
                    }


                    //************* Slide to Next Avaialbel Card from Card Range Slider 
                    if (k == 0)
                    {
                        break;
                    }
                    rng.StepUp();
                    Thread.Sleep(1000);
                    TelerikObject.Click(PageAnalysis.XaxisLabel);
                    rng.MouseHover();
                    Thread.Sleep(1000);

                }
                CommonHelper.TraceLine("************* End of test method *******************");
                CommonHelper.TraceLine("********* Test Failures count due to Assertions  : " + assertfailcount);

            }

            catch (Exception e)
            {
                CommonHelper.TraceLine("************Other Exception ***********" + e);
            }
            finally
            {
                if (assertfailcount > 0)
                {
                    CommonHelper.TraceLine("********* Test Failures count due to Assertions  : " + assertfailcount);
                    //Assert.Fail("Test was Failed due to Assert failure only after Test method ended : See Logs for Assert failures and its  count");
                }
            }


        }

        private void Verify_PumpFillPercentVSDToleranceShutDownLimitsFluidLoadLines()
        {
            try
            {
                PageAnalysis.btnClearCards.ScrollToVisible();
                Thread.Sleep(2000);
                TelerikObject.Click(PageAnalysis.btndynagraphShowHide);
                Thread.Sleep(2000);
                if (PageAnalysis.chkboxCardRangeSlider.Checked)
                {
                    PageAnalysis.chkboxCardRangeSlider.Check(false);
                }
                TelerikObject.Click(PageAnalysis.btnClearCards);
                Thread.Sleep(2000);
                TelerikObject.Click(PageAnalysis.btncardLibrary);
                TelerikObject.WaitForControlDisappear(new HtmlControl(PageDashboard.page_Loader));
                HtmlTable cardtbl = TelerikObject.getTableByColumnName("Current");
                Assert.IsNotNull(cardtbl, "Card Table did not get loaded on clicking dynacard Latency Issue ");
                cardtbl.Rows[0].ScrollToVisible();
                TelerikObject.Click(cardtbl.Rows[0]);
                TelerikObject.WaitForControlDisappear(new HtmlControl(PageDashboard.page_Loader));
                TelerikObject.Click(PageAnalysis.btndynagraphShowHide);
                PageAnalysis.VerifyAllShowHideOptions();
                if (PageAnalysis.chkboxCardRangeSlider.Checked)
                {
                    PageAnalysis.chkboxCardRangeSlider.Check(false);
                }
                Thread.Sleep(2000);
                PageAnalysis.getCheckBoXforListItem(PageAnalysis.lstPumpFillPercent).Check(true);
                TelerikObject.Click(PageAnalysis.btnBackCard);
                Thread.Sleep(2000);
                Assert.IsNotNull(PageAnalysis.pumpFillPercent_0, "Pump Fill % Text 0% did not appear on UI");
                CommonHelper.TraceLine("[*****check point ******]:  Pump Fill % Text 0%  Appeard on UI");
                TelerikObject.Click(PageAnalysis.btndynagraphShowHide);
                Thread.Sleep(2000);
                PageAnalysis.getCheckBoXforListItem(PageAnalysis.lstVSDTolerance).Check(true);

                Thread.Sleep(2000);
                Assert.IsNotNull(PageAnalysis.vsdTolerrancePercent_8, "VSD Tolerance Text 8 % Text  did not appear on UI");
                CommonHelper.TraceLine("[*****check point ******]: VSD Tolerance Text 8 % Text Appeared on UI");
                PageAnalysis.getCheckBoXforListItem(PageAnalysis.lstShutDownLimits).Check(true);
                Thread.Sleep(2000);

                PageAnalysis.DynamicValue = "High High 25000";
                Assert.IsNotNull(PageAnalysis.chartLineText, "High High Shut down Limit text was not shown on UI [High High 25000]");
                PageAnalysis.DynamicValue = "High 22000";
                Assert.IsNotNull(PageAnalysis.chartLineText, "High Shut down Limit text was not shown on UI [High 22000]");
                PageAnalysis.DynamicValue = "Low 7000";
                Assert.IsNotNull(PageAnalysis.chartLineText, "Low Shut down Limit text was not shown on UI [Low 7000]");
                PageAnalysis.DynamicValue = "Low Low 6000";
                Assert.IsNotNull(PageAnalysis.chartLineText, "Low Low  Shut down Limit text was not shown on UI [Low Low 6000]");
                CommonHelper.TraceLine("[*****check point ******]: All 4 Shutdown Limits Lines Text appeared on UI");
                PageAnalysis.getCheckBoXforListItem(PageAnalysis.lstFluidLoadLines).Check(true);
                CommonHelper.PrintScreen("Controller Card Options");
                TelerikObject.Click(PageAnalysis.btnForwardCard);

                Thread.Sleep(2000);
                PageAnalysis.DynamicValue = "Fo Max 18893";
                Assert.IsNotNull(PageAnalysis.chartLineText, "Fo Max Text was not shown on UI [Fo Max 18893]");
                PageAnalysis.DynamicValue = "Fo Up 7255";
                Assert.IsNotNull(PageAnalysis.chartLineText, "Fo Up  text was not shown on UI [Fo Up 7255]");
                PageAnalysis.DynamicValue = "Fo Down 1013";
                Assert.IsNotNull(PageAnalysis.chartLineText, "Fo Down 1013 text was not shown on UI [Fo Down 1013]");
                CommonHelper.TraceLine("[*****check point ******]: All 3 Fluid Load Line Text appeared on UI");
                CommonHelper.PrintScreen("downhole EJ Card Options");
                PageAnalysis.getCheckBoXforListItem(PageAnalysis.lstGridLines).Check(true);
                CommonHelper.PrintScreen("Grid Lines on UI");
                PageAnalysis.getCheckBoXforListItem(PageAnalysis.lstPoints).Check(true);
                CommonHelper.PrintScreen("Points Options on UI");
                PageAnalysis.getCheckBoXforListItem(PageAnalysis.lstCursorPosition).Check(true);
                CommonHelper.PrintScreen("Cursore Position on UI");

                VerifyWellComments();
                PageAnalysis.getCheckBoXforListItem(PageAnalysis.lstAdjustToolbar).Check(true);
                Thread.Sleep(1000);
                for (int i = 0; i < 4; i++)
                {
                    TelerikObject.Click(PageAnalysis.btnRoateCardPlus);

                }
                Assert.AreEqual("4", PageAnalysis.txtRotateCard.Text, "***On Plus Rotation Counter is incremented; check snapshot to see snapshot to see graph got changed");
                PageAnalysis.btndynagraphShowHide.ScrollToVisible();
                CommonHelper.PrintScreen("Card Shape plus ");
                for (int i = 0; i < 4; i++)
                {
                    TelerikObject.Click(PageAnalysis.btnRoateCardMinus);

                }
                Assert.AreEqual("0", PageAnalysis.txtRotateCard.Text, "***On Minus Rotation Counter is decremented;check snapshot to see snapshot to see graph got changed");
                CommonHelper.PrintScreen("Card Shape Minus ");
                TelerikObject.Click(PageAnalysis.btnSavegridOptions);
                TelerikObject.Click(PageAnalysis.btnCloseSaveGridOptions);
                NavigateToAndfrom();
                TelerikObject.Click(PageAnalysis.btndynagraphShowHide);
                PageAnalysis.PersistOptionsCheckForDefault();
                PageAnalysis.CheckallOptions();

                TelerikObject.Click(PageAnalysis.btnSavegridOptions);
                TelerikObject.Click(PageAnalysis.btnWSaveGridOptionsConfirm);
                TelerikObject.Toastercheck(PageAnalysis.toasterControl, "Save Grids Options ", "The Well Analysis layout was successfully saved as the default layout.");
                NavigateToAndfrom();
                TelerikObject.Click(PageAnalysis.btndynagraphShowHide);
                PageAnalysis.VerifyAllOptionsSaved();
                TelerikObject.Click(PageAnalysis.btnWellResetGridOptions);
                TelerikObject.Click(PageAnalysis.btnResetGridOptionsConfirm);
                TelerikObject.Toastercheck(PageAnalysis.toasterControl, "Reset Grids Options ", "The Well Analysis layout was successfully restored to the default layout.");
                NavigateToAndfrom();
                TelerikObject.Click(PageAnalysis.btndynagraphShowHide);
                PageAnalysis.PersistOptionsCheckForDefault();
            }
            catch (Exception e)
            {
                CommonHelper.PrintScreen("Err_ Verify_PumpFillPercentVSDToleranceShutDownLimitsFluidLoadLines");
                Assert.Fail("Expcetion from  Verify_PumpFillPercentVSDToleranceShutDownLimitsFluidLoadLines: " + e.ToString());
            }


        }
        private void NavigateToAndfrom()
        {
            TelerikObject.Click(PageDashboard.pedashboardtab);
            TelerikObject.WaitForControlDisappear(new HtmlControl(PageDashboard.page_Loader));
            //Thread.Sleep(7000);
            TelerikObject.Click(PageDashboard.Analysistesttab);
            TelerikObject.WaitForControlDisappear(new HtmlControl(PageDashboard.page_Loader));
            //Thread.Sleep(20000);
        }
        private void VerifyWellComments()
        {
            TelerikObject.Click(PageAnalysis.btndynagraphShowHide);
            PageAnalysis.getCheckBoXforListItem(PageAnalysis.lstComments).Check(true);
            Thread.Sleep(1000);
            PageAnalysis.divContainerComments.ScrollToVisible();
            Assert.AreEqual("No records available.", PageAnalysis.divContainerComments.BaseElement.InnerText, "Comments are not there Initial Stage");
            string comments = "RRL Well Analysis Comments: By Automation ATS";
            TelerikObject.Sendkeys(PageAnalysis.txtComments, comments);

            TelerikObject.Click(PageAnalysis.btnWellCommentsSave, false);
            TelerikObject.Toastercheck(PageAnalysis.toasterControl, "Well comments Save", "Well comment saved successfully.");
            Thread.Sleep(2000);
            PageAnalysis.DynamicValue = comments;
            Assert.IsNotNull(PageAnalysis.wellCommentText, "Well Comments text saved did not appear on UI");
            PageAnalysis.wellCommentText.ScrollToVisible();
            TelerikObject.Click(PageAnalysis.btnWellCommentsDelete);
            string confirmtext = "Are you sure you want to delete this comment?";
            PageAnalysis.DynamicValue = confirmtext;
            Assert.IsNotNull(PageAnalysis.wellCommentText, "Well Comments text saved did not appear on UI");
            TelerikObject.Click(PageAnalysis.btnWellCommentsDeleteConfirmYes);
            TelerikObject.Toastercheck(PageAnalysis.toasterControl, "Well comments Delete", "Well comment deleted successfully.");
            Assert.AreEqual("No records available.", PageAnalysis.divContainerComments.BaseElement.InnerText, "Comments are deleted");
        }
        private void VerifyAnalysisReport()
        {
            try
            {
                Thread.Sleep(3000);
                TelerikObject.Click(PageAnalysis.btnanalysisReportBtn);
                Thread.Sleep(5000);
                //Ensure Report got Generated before proceeding 
                Assert.IsNotNull(PageAnalysis.btnDownoadAnalysisReportbutton, "Download Analysis Report Button was not shown on UI");
                HtmlControl lbldhmethod = TelerikObject.GetElement("htmlcontrol", "content", "Downhole Analysis Method:", "Field Value for: -- " + "Downhole Analysis Method");
                Assert.AreEqual("Downhole Analysis Method:Everitt Jennings", lbldhmethod.BaseElement.Parent.InnerText, "Default Calculation Method is Not Everitt Jennings");
                HtmlControl lbldampingfactor = TelerikObject.GetElement("htmlcontrol", "content", "Damping Factor:", "Field Value for: -- " + "Damping Factor:");
                Assert.AreEqual("Damping Factor:0.50", lbldampingfactor.BaseElement.Parent.InnerText, "Damping Factor is not : 0.5 [Expected]");
                Thread.Sleep(3000);
                TelerikObject.Click(PageAnalysis.divMiniReportpane);
                verifySingleTableonUI("RRLAnalysisPhysicalParameters.xml");
                #region VerifyRodtablesData
                //**************************  Verify Rods table data
                //   string taper2colnames = "Type /Grade;Number;Length;Service Factor;Minimum Stress;Maximum Stress;Maximum Allowable Range;Allowable;Dev;Drag;Damping Up;Damping Down;Guides?";
                // ***************************** Rods 1 Table****************************************************************
                string testdatafile = string.Empty;
                DataTable dt = null; int i = 0;
                string rods1colnames = "Diameter";
                testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\TestData", "Rods1.xml");
                CommonHelper.TraceLine("Getting Data from XML file ... " + testdatafile);
                dt = CommonHelper.BuildDataTableFromXml(testdatafile);
                i = 0;
                foreach (DataRow drow in dt.Rows)
                {
                    string roddiameter = dt.Rows[i]["Rod"].ToString();
                    string colvals = roddiameter;
                    TelerikObject.verifyGridCellValuesSingleTable(rods1colnames, colvals, i);
                    i++;
                }

                ////****************************** Rods 2 Table **************************************************************
                string taper2colnames = "Type /;Number;Length;Service|Factor;Minimum|Stress;Maximum|Stress;Maximum|Allowable Range;Allowable;Dev;Drag;Damping|Up;Damping|Down;Guides?";
                testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\TestData", "Rods2.xml");
                CommonHelper.TraceLine("Getting Data from XML file ... " + testdatafile);
                dt = CommonHelper.BuildDataTableFromXml(testdatafile);
                i = 0;
                foreach (DataRow drow in dt.Rows)
                {
                    string typegrade = dt.Rows[i]["TypeGrade"].ToString();
                    string number = dt.Rows[i]["Number"].ToString();
                    string lentgh = dt.Rows[i]["Length"].ToString();
                    string servicefactor = dt.Rows[i]["ServiceFactor"].ToString();
                    string minstress = dt.Rows[i]["MinimumStress"].ToString();
                    string maxstress = dt.Rows[i]["MaximumStress"].ToString();
                    string maxallowrange = dt.Rows[i]["MaximumAllowableRange"].ToString();
                    string allowable = dt.Rows[i]["Allowable"].ToString();
                    string dev = dt.Rows[i]["Dev"].ToString();
                    string drag = dt.Rows[i]["Drag"].ToString();
                    string dampup = dt.Rows[i]["DampingUp"].ToString();
                    string dampdown = dt.Rows[i]["DampingDown"].ToString();
                    string guides = dt.Rows[i]["Guides"].ToString();

                    string colvals = typegrade + ";" + number + ";" + lentgh + ";" + servicefactor + ";" + minstress + ";" + maxstress + ";" + maxallowrange + ";" + allowable + ";" + dev + ";" + drag + ";" + dampup + ";" + dampdown + ";" + guides;
                    TelerikObject.verifyGridCellValuesSingleTable(taper2colnames, colvals, i);
                    i++;
                }
                ////Rods3 Table
                //#endregion
                ////Verify Loads Table
                //#region LoadsData

                ////Loads 1
                ////**************************  Verify Loads table data

                string load1colnames = "Location";
                testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\TestData", "Load1.xml");
                CommonHelper.TraceLine("Getting Data from XML file ... " + testdatafile);
                dt = CommonHelper.BuildDataTableFromXml(testdatafile);
                i = 0;
                foreach (DataRow drow in dt.Rows)
                {
                    string loadlocation = dt.Rows[i]["Location"].ToString();
                    string colvals = loadlocation;
                    TelerikObject.verifyGridCellValuesSingleTable(load1colnames, colvals, i);
                    i++;
                }

                ////Loads 2
                ////   string taper2colnames = "Type /Grade;Number;Length;Service Factor;Minimum Stress;Maximum Stress;Maximum Allowable Range;Allowable;Dev;Drag;Damping Up;Damping Down;Guides?";
                string load2colnames = "Peak DH;Avg DH;Calc PO;Normal Surface;Peak Surface;Difference";
                testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\TestData", "Load2.xml");
                CommonHelper.TraceLine("Getting Data from XML file ... " + testdatafile);
                dt = CommonHelper.BuildDataTableFromXml(testdatafile);
                i = 0;
                foreach (DataRow drow in dt.Rows)
                {
                    string peakDH = dt.Rows[i]["PeakDH"].ToString();
                    string avgDH = dt.Rows[i]["AvgDH"].ToString();
                    string calcpo = dt.Rows[i]["CalcPO"].ToString();
                    string normalsurface = dt.Rows[i]["NormalSurface"].ToString();
                    string peaksurface = dt.Rows[i]["PeakSurface"].ToString();
                    string difference = dt.Rows[i]["Difference"].ToString();


                    string colvals = peakDH + ";" + avgDH + ";" + calcpo + ";" + normalsurface + ";" + peaksurface + ";" + difference;
                    TelerikObject.verifyGridCellValuesSingleTable(load2colnames, colvals, i);
                    i++;
                }

                //Loads 3:
                string load3colnames = "Load Value;Rods";
                testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\TestData", "Load3.xml");
                CommonHelper.TraceLine("Getting Data from XML file ... " + testdatafile);
                dt = CommonHelper.BuildDataTableFromXml(testdatafile);
                i = 0;
                foreach (DataRow drow in dt.Rows)
                {
                    string rods = dt.Rows[i]["Rods"].ToString();
                    string loadValue = dt.Rows[i]["LoadValue"].ToString();
                    string colvals = rods + ";" + loadValue;
                    TelerikObject.verifyGridCellValuesSingleTable(load3colnames, colvals, i);
                    i++;
                }

                #endregion

                //Verify DH Pump table
                #region DHPumpTableData
                //DHpump1:
                HtmlTable DhPump1 = TelerikObject.getTableByColumnName("Total Fluid");
                Assert.AreEqual("Surface", DhPump1.Rows[1].Cells[0].BaseElement.InnerText);
                Assert.AreEqual("Downhole", DhPump1.Rows[2].Cells[0].BaseElement.InnerText);
                Assert.AreEqual("Total Fluid", DhPump1.Rows[3].Cells[0].BaseElement.InnerText);
                CommonHelper.TraceLine("**** DH Pump Sub Table 1 has Expected Values: Surface, DownHole and total Fluid ");
                //**************************  Verify DH Pumps table data
                //DHpump2:
                string dhpump2colnames = "Stroke;Fillage;Displacement (24.0);Displacement (6.817);Vol Efficiency";
                testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\TestData", "DHPump2.xml");
                CommonHelper.TraceLine("Getting Data from XML file ... " + testdatafile);
                dt = CommonHelper.BuildDataTableFromXml(testdatafile);
                i = 0;
                foreach (DataRow drow in dt.Rows)
                {
                    string stroke = dt.Rows[i]["Stroke"].ToString();
                    string fillage = dt.Rows[i]["Fillage"].ToString();
                    string displacement24 = dt.Rows[i]["Displacement_24"].ToString();
                    string displacementnon24 = dt.Rows[i]["Displacement_non24"].ToString();
                    string volEfficiency = dt.Rows[i]["Vol_Efficiency"].ToString();


                    string colvals = stroke + ";" + fillage + ";" + displacement24 + ";" + displacementnon24 + ";" + volEfficiency;
                    TelerikObject.verifyGridCellValuesSingleTable(dhpump2colnames, colvals, i, "Fillage");
                    i++;
                }
                //DHpump3:
                string dhpump3colnames = "Non-Dimensional Ratios;Value";
                testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\TestData", "DHPump3.xml");
                CommonHelper.TraceLine("Getting Data from XML file ... " + testdatafile);
                dt = CommonHelper.BuildDataTableFromXml(testdatafile);
                i = 0;
                foreach (DataRow drow in dt.Rows)
                {
                    string DimensionalRatios = dt.Rows[i]["Non-DimensionalRatios"].ToString();
                    string dhValue = dt.Rows[i]["Value"].ToString();
                    string colvals = DimensionalRatios + ";" + dhValue;
                    TelerikObject.verifyGridCellValuesSingleTable(dhpump3colnames, colvals, i);
                    i++;
                }
                #endregion
            }

            catch (Exception e)
            {
                CommonHelper.PrintScreen("Error_AnalysisReport");
                Thread.Sleep(2000);
                Assert.Fail("Failure  in Analysis Report: " + e.ToString());
            }


        }

        private void CheckUnitBalancing()
        {
            TelerikObject.Click(PageAnalysis.btnunitBalancing);
            TelerikObject.Sendkeys(PageAnalysis.txtDesiredCBT, "1169.03");
            TelerikObject.Click(PageAnalysis.btnUnitBalanceCalculate);
            Thread.Sleep(2000);
            Assert.IsNotNull(PageAnalysis.lblUnitBalancingDetails, "Unit Balancing Details title did not appear on UI");
            //verify Tables by retreiving values from each Html table rendered on UI
            #region tableverificiation
            //Existing Counter Balance: Crank 1
            HtmlTable tbl = PageAnalysis.GetTableByExpression("Existing Counter Balance", "Crank 1", "1");
            VerifyNonUniformTable(tbl, "EC1LeadLagPoition.xml");
            tbl = PageAnalysis.GetTableByExpression("Existing Counter Balance", "Crank 1", "2");
            VerifyNonUniformTable(tbl, "EC1PrimaryWeight.xml");
            tbl = PageAnalysis.GetTableByExpression("Existing Counter Balance", "Crank 1", "3");
            VerifyNonUniformTable(tbl, "EC1Auxililary.xml");
            //Existing Counter Balance: Crank 2 
            tbl = PageAnalysis.GetTableByExpression("Existing Counter Balance", "Crank 2", "1");
            VerifyNonUniformTable(tbl, "EC2LeadLagPoition.xml");
            tbl = PageAnalysis.GetTableByExpression("Existing Counter Balance", "Crank 2", "2");
            VerifyNonUniformTable(tbl, "EC2PrimaryWeight.xml");
            tbl = PageAnalysis.GetTableByExpression("Existing Counter Balance", "Crank 2", "3");
            VerifyNonUniformTable(tbl, "EC2Auxililary.xml");

            //Desired Counter Balance: Crank 1
            tbl = PageAnalysis.GetTableByExpression("Desired Counter Balance", "Crank 1", "1");
            VerifyNonUniformTable(tbl, "DC1LeadLagPoition.xml");
            tbl = PageAnalysis.GetTableByExpression("Desired Counter Balance", "Crank 1", "2");
            VerifyNonUniformTable(tbl, "DC1PrimaryWeight.xml");
            tbl = PageAnalysis.GetTableByExpression("Desired Counter Balance", "Crank 1", "3");
            VerifyNonUniformTable(tbl, "DC1Auxililary.xml");
            //Existing Counter Balance: Crank 2 
            tbl = PageAnalysis.GetTableByExpression("Desired Counter Balance", "Crank 2", "1");
            VerifyNonUniformTable(tbl, "DC2LeadLagPoition.xml");
            tbl = PageAnalysis.GetTableByExpression("Desired Counter Balance", "Crank 2", "2");
            VerifyNonUniformTable(tbl, "DC2PrimaryWeight.xml");
            tbl = PageAnalysis.GetTableByExpression("Desired Counter Balance", "Crank 2", "3");
            VerifyNonUniformTable(tbl, "DC2Auxililary.xml");
            #endregion
            string testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\TestData", "UnitBalance.xml");
            CommonHelper.TraceLine("Getting Data from XML file ... " + testdatafile);
            DataTable dt = CommonHelper.BuildDataTableFromXml(testdatafile);
            //Resulting calculation verification
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string ExisitngCBT = dt.Rows[i]["ExisitngCBT"].ToString();
                string ExisitngCBTUOM = dt.Rows[i]["ExisitngCBTUOM"].ToString();
                string ExisitngMaxGBT = dt.Rows[i]["ExisitngMaxGBT"].ToString();
                string ExisitngMaxGBTUOM = dt.Rows[i]["ExisitngMaxGBTUOM"].ToString();
                string DesiredCBT = dt.Rows[i]["DesiredCBT"].ToString();
                string DesiredCBTUOM = dt.Rows[i]["DesiredCBTUOM"].ToString();
                string DesiredMaxGBT = dt.Rows[i]["DesiredMaxGBT"].ToString();
                string DesiredMaxGBTUOM = dt.Rows[i]["DesiredMaxGBTUOM"].ToString();
                //Exisiting CBT
                TelerikObject.verifyValues(ExisitngCBT, PageAnalysis.lblExisitngCBT.BaseElement.GetNextSibling().InnerText);
                TelerikObject.verifyValues(ExisitngCBTUOM, PageAnalysis.lblExisitngCBT.BaseElement.GetNextSibling().GetNextSibling().InnerText);
                TelerikObject.verifyValues(ExisitngMaxGBT, PageAnalysis.lblExisitngMaxGBT.BaseElement.GetNextSibling().InnerText);
                TelerikObject.verifyValues(ExisitngMaxGBTUOM, PageAnalysis.lblExisitngMaxGBT.BaseElement.GetNextSibling().GetNextSibling().InnerText);
                //Desired CBT
                TelerikObject.verifyValues(DesiredCBT, PageAnalysis.lblDesiredCBT.BaseElement.GetNextSibling().InnerText);
                TelerikObject.verifyValues(DesiredCBTUOM, PageAnalysis.lblDesiredCBT.BaseElement.GetNextSibling().GetNextSibling().InnerText);
                TelerikObject.verifyValues(DesiredMaxGBT, PageAnalysis.lblDesiredMaxGBT.BaseElement.GetNextSibling().InnerText);
                TelerikObject.verifyValues(DesiredMaxGBTUOM, PageAnalysis.lblDesiredMaxGBT.BaseElement.GetNextSibling().GetNextSibling().InnerText);


            }
            CommonHelper.TraceLine("Verification of Unit Balacing report complete");
            TelerikObject.Click(PageAnalysis.btnRCloseUnitBalanceDetails);

        }

        private void CheckCalibration()
        {
            TelerikObject.Click(PageAnalysis.btncalibrateBtn);
            string calibrationpercent = new HtmlInputNumber(PageAnalysis.txtCalibrtionPercent.BaseElement).Text;
            Assert.AreEqual("100", calibrationpercent, "Calibration Percent is not 100");
            TelerikObject.Click(PageAnalysis.btnRGetCalibrationCard);
            Thread.Sleep(4000);
            PageAnalysis.DynamicValue = "Calibration: Surface";
            Assert.IsNotNull(PageAnalysis.legend_Text, "Legend for Calibration: Surface" + " Is not shown on UI");
            CommonHelper.TraceLine("***** The Legend text for Dynagraph Chart for -- Calibration: Surface  appeared on UI");

            PageAnalysis.DynamicValue = "Calibration: Controller Downhole";
            Assert.IsNotNull(PageAnalysis.legend_Text, "***** The Legend for Calibration: Controller Downhole Controller  Is not shown on UI");
            CommonHelper.TraceLine("***** TheThe Legend text for Dynagraph Chart for  Calibration: Controller Downhole appeared on UI");
            CommonHelper.PrintScreen("Calibration Card");

        }
        private void verifySingleTableonUI(string fileName)
        {

            string testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\TestData", fileName);
            CommonHelper.TraceLine("Getting Data from XML file ... " + testdatafile);
            DataTable dt = CommonHelper.BuildDataTableFromXml(testdatafile);
            int i = 0;
            foreach (DataRow drow in dt.Rows)
            {
                string fieldName = dt.Rows[i]["FieldName"].ToString();
                string val = dt.Rows[i]["Text"].ToString();
                string uomtext = dt.Rows[i]["UOM"].ToString();
                string addtionaltext = dt.Rows[i]["AddtionalText"].ToString();
                string addtionaluom = dt.Rows[i]["AddtionalUOM"].ToString();
                string valtext = string.Empty;
                if (uomtext.Length > 1)
                {
                    valtext = val + ";" + uomtext;
                }
                else if (addtionaltext.Length > 1)
                {
                    valtext = val + ";" + uomtext + ";" + addtionaltext + ";" + addtionaluom;
                }
                else
                {
                    valtext = val;
                }
                TelerikObject.verfiyRRLTables(fieldName, valtext);

                i++;
            }
        }
        private void AnalysisCommon()
        {

            TelerikObject.Click(PageDashboard.optimizationtab);
            TelerikObject.Click(PageDashboard.Analysistesttab);
            Thread.Sleep(7000);
        }
        private void select_FirstCard()
        {
            TelerikObject.Click(PageAnalysis.btnClearCards);
            TelerikObject.WaitForControlDisappear(new HtmlControl(PageDashboard.page_Loader));
            TelerikObject.Click(PageAnalysis.btncardLibrary);
            TelerikObject.WaitForControlDisappear(new HtmlControl(PageDashboard.page_Loader));
            HtmlTable cardtbl = TelerikObject.getTableByColumnName("Current");
            TelerikObject.Click(cardtbl.Rows[0]);
            TelerikObject.WaitForControlDisappear(new HtmlControl(PageDashboard.page_Loader));
        }

        private void GenerateReportforEachCard()
        {
            try
            {
                PageAnalysis.btnClearCards.ScrollToVisible();
                TelerikObject.Sendkeys(PageAnalysis.startDate, "01012008", false);
                HtmlTable cardtbl = null;
                Thread.Sleep(1000);
                string noreporttext = "Mini Report data not available.";
                for (int i = 1; i < 25; i++)
                {
                    //Clear Cards Collection and Draw a Single Current Card
                    do
                    {
                        TelerikObject.Click(PageAnalysis.btnClearCards);
                        Thread.Sleep(1000);
                        TelerikObject.Click(PageAnalysis.btncardLibrary);
                        TelerikObject.WaitForControlDisappear(new HtmlControl(PageDashboard.page_Loader));
                        Thread.Sleep(1000);
                        if (cardtbl == null)
                        {
                            cardtbl = TelerikObject.getTableByColumnName("Current");
                        }
                        //Worst Loading Time is assumed to be 10S
                        cardtbl.Rows[i].ScrollToVisible();
                        cardtbl.Rows[i].MouseClick();
                        TelerikObject.WaitForControlDisappear(new HtmlControl(PageDashboard.page_Loader));
                        Thread.Sleep(1000);
                        TelerikObject.Click(PageAnalysis.btnrunAnalysis);
                        TelerikObject.WaitForControlDisappear(new HtmlControl(PageDashboard.page_Loader));
                        Thread.Sleep(1000);
                    } while (PageAnalysis.divMiniReportpane.BaseElement.InnerText == noreporttext);




                }
            }
            catch (Exception e)
            {
                CommonHelper.TraceLine("Error from GenerateMiniReportforEachCard" + e.ToString());
                CommonHelper.PrintScreen("ErrCardGenerateforEachCard");

            }

        }
        private void comparetimestamp(string exptimestamp, string acttiemstamp)
        {
            DateTime dtexp = Convert.ToDateTime(exptimestamp);
            DateTime dtact = Convert.ToDateTime(acttiemstamp);
            Assert.AreEqual(dtexp, dtact, "Date Time Stamp Did not match");
            CommonHelper.TraceLine(string.Format("TimeStamp was matched for Expected {0} and Actual {1} ", exptimestamp, acttiemstamp));
        }

        private void VerifyGLAnalaysisDefault()
        {
            //Verify WellTest Date:
            // VerifyAnalysisReport Control Values 
            PageAnalysis.GetAndClickToggleSwitchByLabel("Daily Average Data", "OFF");
            //temorarry Fix Select the dropdown vlaue 

            VerifyGLValues("GLAnalysisDefaultData.xml");
            CommonHelper.ChangeUnitSystemUserSetting("Metric");
            NavigateToAndfrom();
            PageAnalysis.GetAndClickToggleSwitchByLabel("Daily Average Data", "OFF");
            GenerateMetricValuesXML("GLAnalysisDefaultData.xml", "GLAnalysisDefaultData_Metric.xml");
            VerifyGLValues("GLAnalysisDefaultData_Metric.xml");
            CommonHelper.ChangeUnitSystemUserSetting("US");
            NavigateToAndfrom();
            PageAnalysis.GetAndClickToggleSwitchByLabel("Daily Average Data", "OFF");
            TelerikObject.WaitForControlDisappear(new HtmlControl(PageDashboard.page_Loader));



        }

        private void VerifyESPAnalaysisDefault()
        {
            //Verify WellTest Date:
            // VerifyAnalysisReport Control Values 
            PageAnalysis.GetAndClickToggleSwitchByLabel("Daily Average Data", "OFF");
            //temorarry Fix Select the dropdown vlaue 

            VerifyESPValues("ESPAnalysisDefaultData.xml");
            CommonHelper.ChangeUnitSystemUserSetting("US");
            NavigateToAndfrom();
            PageAnalysis.GetAndClickToggleSwitchByLabel("Daily Average Data", "OFF");

            VerifyESPValues("ESPAnalysisDefaultData_US.xml");
            CommonHelper.ChangeUnitSystemUserSetting("Metric");
            NavigateToAndfrom();
            PageAnalysis.GetAndClickToggleSwitchByLabel("Daily Average Data", "OFF");




        }





        private void GenerateMetricValuesXML(string UsFileName, string MetricFileName)
        {
            string testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\TestData", UsFileName);
            string outputfile = Path.Combine(FilesLocation + @"\UIAutomation\TestData", MetricFileName);
            CommonHelper.TraceLine("Getting Data from XML file ... " + testdatafile);
            DataTable dtprev = CommonHelper.BuildDataTableFromXml(testdatafile);
            DataTable dtnew = new DataTable();
            int rowcount = 0;
            Tuple<string, string>[] uomtuple =  {
                Tuple.Create("ft","m"),
                Tuple.Create("psia","kPa"),
                Tuple.Create("F","C"),
                Tuple.Create("Mscf/d","sm³/d"),
                Tuple.Create("STB/d","sm³/d"),
                Tuple.Create("STB/d/psi","sm3/d/kPa"),
                Tuple.Create("scf/STB","sm³/sm³"),
                Tuple.Create("1/64in","mm"),
                Tuple.Create("API","sg"),
            };

            foreach (DataRow dr in dtprev.Rows)
            {
                DataRow drnew = dtnew.NewRow();

                for (int k = 0; k < dtprev.Columns.Count; k++)
                {
                    if (rowcount == 0)
                    {
                        dtnew.Columns.Add(dtprev.Columns[k].ColumnName);
                    }
                    string strvalue = dr[k].ToString();
                    string[] starrval = strvalue.Split(new char[] { ';' });
                    double dout;
                    bool isNumber = double.TryParse(starrval[0], out dout);
                    string uomtext = string.Empty;
                    string metricuomunittext = string.Empty;

                    if (starrval.Length > 1)
                    {
                        #region SeparateNumericand UOMLogic
                        uomtext = starrval[1];

                        if (uomtext.Contains("°F"))
                        {
                            uomtext = "F";
                        }
                        foreach (Tuple<string, string> tp in uomtuple)
                        {
                            if (tp.Item1 == uomtext)
                            {
                                metricuomunittext = tp.Item2;
                            }
                        }
                        if (metricuomunittext.Equals("C"))
                        {
                            metricuomunittext = "°C";
                        }
                        if (isNumber)
                        {
                            starrval[0] = welltest.UnitsConversion(uomtext, Convert.ToDouble(starrval[0])).ToString();
                        }
                        drnew[k] = starrval[0] + ";" + metricuomunittext;
                        #endregion
                    }
                    else
                    {
                        if (starrval[0].Contains("@"))
                        {
                            #region SeparateNumberUOMRpeaet
                            string[] arrtext = starrval[0].Split(new char[] { '@' });
                            string nouomtext = arrtext[0];
                            string uomparttextt = arrtext[1].Trim();
                            string[] numtext = uomparttextt.Split(new char[] { ' ' });
                            // string metricpart2text = welltest.UnitsConversion(numtext[1].Trim(), Convert.ToDouble(numtext[0].Trim())).ToString();
                            string metricpart2text = Math.Round((double)welltest.UnitsConversion(numtext[1].Trim(), Convert.ToDouble(numtext[0].Trim())), 3).ToString();
                            string metricuomunittext2 = string.Empty;
                            if (numtext[1].Trim().Contains("°F"))
                            {
                                numtext[1] = "F";
                            }
                            foreach (Tuple<string, string> tp in uomtuple)
                            {
                                if (tp.Item1 == numtext[1])
                                {
                                    metricuomunittext2 = tp.Item2;
                                }
                            }
                            if (metricuomunittext2.Equals("C"))
                            {
                                metricuomunittext2 = "°C";
                            }

                            string posttext = metricpart2text + " " + metricuomunittext2;
                            string fulltext = nouomtext + "@ " + posttext;
                            drnew[k] = fulltext;
                            #endregion
                        }
                        else
                        {
                            drnew[k] = starrval[0];
                        }
                    }
                }

                dtnew.Rows.Add(drnew);
                rowcount++;
            }
            DataSet dataSet = new DataSet("welltest");
            dtnew.TableName = "record";
            dataSet.Tables.Add(dtnew);
            dataSet.WriteXml(outputfile);
            CommonHelper.TraceLine("Output Xml generated at path " + outputfile);
        }

        private void VerifyGLValues(string filename)
        {
            string testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\TestData", filename);
            CommonHelper.TraceLine("Getting Data from XML file ... " + testdatafile);
            DataTable dtprev = CommonHelper.BuildDataTableFromXml(testdatafile);
            // Create Varibles 
            #region VariableAssignment
            string TestDate = dtprev.Rows[0]["TestDate"].ToString();
            string QualityCode = dtprev.Rows[0]["QualityCode"].ToString();
            string InjectionMethod = dtprev.Rows[0]["InjectionMethod"].ToString();
            string Depth = dtprev.Rows[0]["Depth"].ToString();
            string MultiphaseFlowCorrelation = dtprev.Rows[0]["MultiphaseFlowCorrelation"].ToString();
            string LFactor = dtprev.Rows[0]["LFactor"].ToString();
            string WellheadPressure = dtprev.Rows[0]["WellheadPressure"].ToString();
            string WellheadTemperature = dtprev.Rows[0]["WellheadTemperature"].ToString();
            string CasingHeadPressure = dtprev.Rows[0]["CasingHeadPressure"].ToString();
            string GasInjectionRate = dtprev.Rows[0]["GasInjectionRate"].ToString();
            string DownholeGauge = dtprev.Rows[0]["DownholeGauge"].ToString();
            string OilRate = dtprev.Rows[0]["OilRate"].ToString();
            string WaterRate = dtprev.Rows[0]["WaterRate"].ToString();
            string GasRate = dtprev.Rows[0]["GasRate"].ToString();
            string WaterCut = dtprev.Rows[0]["WaterCut"].ToString();
            string LiquidRate = dtprev.Rows[0]["LiquidRate"].ToString();
            string FormationGOR = dtprev.Rows[0]["FormationGOR"].ToString();
            string StaticBottomHolePressure = dtprev.Rows[0]["StaticBottomHolePressure"].ToString();
            string ProductivityIndex = dtprev.Rows[0]["ProductivityIndex"].ToString();
            string StartNode = dtprev.Rows[0]["StartNode"].ToString();
            string SolutionNode = dtprev.Rows[0]["SolutionNode"].ToString();
            string FlowingBottomHolePressure = dtprev.Rows[0]["FlowingBottomHolePressure"].ToString();
            string SolutionNodePressure = dtprev.Rows[0]["SolutionNodePressure"].ToString();
            string GasInjectionDepth = dtprev.Rows[0]["GasInjectionDepth"].ToString();
            string LiquidRatefromIPR = dtprev.Rows[0]["LiquidRatefromIPR"].ToString();
            #endregion
            //Call From Xml
            #region ValidationofValuesforEachControl

            PageAnalysis.getControlTextGLAnalysis(PageAnalysis.ddtxtWellTestDate, TestDate, "Test Date Time");
            PageAnalysis.getControlTextGLAnalysis(PageAnalysis.txtQualitycode, QualityCode, "Quality Code", 0, false);
            PageAnalysis.getControlTextGLAnalysis(PageAnalysis.ddtxtInjectionMethod, InjectionMethod, "Injection Method");
            PageAnalysis.getControlTextGLAnalysis(PageAnalysis.ddtxtDepth, Depth, "Depth");
            PageAnalysis.getControlTextGLAnalysis(PageAnalysis.ddtxtMultiphaseFlowCorrelation, MultiphaseFlowCorrelation, "Multiphase Flow Correlation");
            PageAnalysis.getControlTextGLAnalysis(PageAnalysis.txtLFactor, LFactor, "LFactor");
            PageAnalysis.getControlTextGLAnalysis(PageAnalysis.txtWellheadPressure, WellheadPressure, "Well head Pressure");
            PageAnalysis.getControlTextGLAnalysis(PageAnalysis.txtWellheadTemperature, WellheadTemperature, "Well head Temperature");
            PageAnalysis.getControlTextGLAnalysis(PageAnalysis.txtCasingHeadPressure, CasingHeadPressure, "Casing Head Pressure");
            PageAnalysis.getControlTextGLAnalysis(PageAnalysis.txtGasInjectionRate, GasInjectionRate, "Gas Injection Rate");
            PageAnalysis.getControlTextGLAnalysis(PageAnalysis.txtDownholeGauge, DownholeGauge, "Downhole Gauge");
            PageAnalysis.getControlTextGLAnalysis(PageAnalysis.txtOilRate, OilRate, "Oil Rate");
            PageAnalysis.getControlTextGLAnalysis(PageAnalysis.txtWaterRate, WaterRate, "Water Rate");
            PageAnalysis.getControlTextGLAnalysis(PageAnalysis.txtGasRate, GasRate, "Gas Rate");
            PageAnalysis.getControlTextGLAnalysis(PageAnalysis.txtLiquidRate, LiquidRate, "Liquid Rate");
            PageAnalysis.getControlTextGLAnalysis(PageAnalysis.txtFormationGOR, FormationGOR, "Formation GOR");
            PageAnalysis.getControlTextGLAnalysis(PageAnalysis.txtWaterCut, WaterCut, "Water Cut");
            PageAnalysis.getControlTextGLAnalysis(PageAnalysis.txtStaticBottomHolePressure, StaticBottomHolePressure, "Static Bottom Hole Pressure");
            PageAnalysis.getControlTextGLAnalysis(PageAnalysis.txtProductivityIndex, ProductivityIndex, "Productivity Index");
            PageAnalysis.getControlTextGLAnalysis(PageAnalysis.ddtxtStartNode, StartNode, "Start Node");
            PageAnalysis.getControlTextGLAnalysis(PageAnalysis.ddtxtSolutionNode, SolutionNode, "Solution Node");
            PageAnalysis.getControlTextGLAnalysis(PageAnalysis.ddtxtFBHP, FlowingBottomHolePressure, "Flowing Bottom Hole Pressure");
            PageAnalysis.getControlTextGLAnalysis(PageAnalysis.ddtxtSolutionNodePressure, SolutionNodePressure, "Solution Node Pressure");
            PageAnalysis.getControlTextGLAnalysis(PageAnalysis.ddtxtGasInjectionDepth, GasInjectionDepth, "Gas Injection Depth");
            PageAnalysis.getControlTextGLAnalysis(PageAnalysis.ddtxtLiquidRateFromIPR, LiquidRatefromIPR, "Liquid Rate from IPR");


            #endregion
            //Change Units to Metric
        }
        //
        private void VerifyESPValues(string filename)
        {
            string testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\TestData", filename);
            CommonHelper.TraceLine("Getting Data from XML file ... " + testdatafile);
            DataTable dtprev2 = CommonHelper.BuildDataTableFromXml(testdatafile);
            // Create Varibles 
            #region VariableAssignment
            string TestDate = dtprev2.Rows[0]["TestDate"].ToString();
            string QualityCode = dtprev2.Rows[0]["QualityCode"].ToString();
            string MultiphaseFlowCorrelation = dtprev2.Rows[0]["MultiphaseFlowCorrelation"].ToString();
            string LFactor = dtprev2.Rows[0]["LFactor"].ToString();
            string HeadTuningFactor = dtprev2.Rows[0]["HeadTuningFactor"].ToString();
            string PumpIntakePressure = dtprev2.Rows[0]["PumpIntakePressure"].ToString();
            string PumpDischargePressure = dtprev2.Rows[0]["PumpDischargePressure"].ToString();
            string WellheadPressure = dtprev2.Rows[0]["WellheadPressure"].ToString();
            string WellheadTemperature = dtprev2.Rows[0]["WellheadTemperature"].ToString();
            string Frequency = dtprev2.Rows[0]["Frequency"].ToString();
            string WaterRate = dtprev2.Rows[0]["WaterRate"].ToString();
            string GasRate = dtprev2.Rows[0]["GasRate"].ToString();
            string LiquidRate = dtprev2.Rows[0]["LiquidRate"].ToString();
            //string FormationGOR = dtprev1.Rows[0]["FormationGOR"].ToString();
            //string WaterCut = dtprev1.Rows[0]["WaterCut"].ToString();
            string StaticBottomHolePressure = dtprev2.Rows[0]["StaticBottomHolePressure"].ToString();
            //string ProductivityIndex = dtprev1.Rows[0]["ProductivityIndex"].ToString();
            string StartNode = dtprev2.Rows[0]["StartNode"].ToString();
            string SolutionNode = dtprev2.Rows[0]["SolutionNode"].ToString();
            string FlowingBottomHolePressure = dtprev2.Rows[0]["FlowingBottomHolePressure"].ToString();
            string SolutionNodePressure = dtprev2.Rows[0]["SolutionNodePressure"].ToString();
            //string LiqRatefromIPR = dtprev1.Rows[0]["LiqRatefromIPR"].ToString();
            #endregion
            //Call From Xml
            #region ValidationofValuesforEachControl

            PageAnalysis.getControlTextESPAnalysis(PageAnalysis.ddtxtWellTestDate, TestDate, "Test Date Time");
            PageAnalysis.getControlTextESPAnalysis(PageAnalysis.txtQualitycode, QualityCode, "Quality Code", 0, false);
            PageAnalysis.getControlTextESPAnalysis(PageAnalysis.ddtxtMultiphaseFlowCorrelation, MultiphaseFlowCorrelation, "Multiphase Flow Correlation");
            PageAnalysis.getControlTextESPAnalysis(PageAnalysis.txtLFactor, LFactor, "LFactor");
            PageAnalysis.getControlTextESPAnalysis(PageAnalysis.ddtxtHeadTuningFactor, HeadTuningFactor, "Head Tuning Factor");
            PageAnalysis.getControlTextESPAnalysis(PageAnalysis.ddtxtPumpIntakePressure, PumpIntakePressure, "Pump Intake Pressure");
            PageAnalysis.getControlTextESPAnalysis(PageAnalysis.ddtxtPumpDischargePressure, PumpDischargePressure, "Pump Discharge Pressure");
            PageAnalysis.getControlTextESPAnalysis(PageAnalysis.txtWellheadPressure, WellheadPressure, "Well head Pressure");
            PageAnalysis.getControlTextESPAnalysis(PageAnalysis.txtWellheadTemperature, WellheadTemperature, "Well head Temperature");
            PageAnalysis.getControlTextESPAnalysis(PageAnalysis.ddtxtFrequency, Frequency, "Frequency");
            PageAnalysis.getControlTextESPAnalysis(PageAnalysis.txtWaterRate, WaterRate, "Water Rate");
            PageAnalysis.getControlTextESPAnalysis(PageAnalysis.txtGasRate, GasRate, "Gas Rate");
            PageAnalysis.getControlTextESPAnalysis(PageAnalysis.txtLiquidRate, LiquidRate, "Liquid Rate");
            //PageAnalysis.getControlTextESPAnalysis(PageAnalysis.txtFormationGOR ,FormationGOR, "Formation GOR");
            //PageAnalysis.getControlTextESPAnalysis(PageAnalysis.txtWaterCut, WaterCut, "Water Cut");
            PageAnalysis.getControlTextESPAnalysis(PageAnalysis.txtStaticBottomHolePressure, StaticBottomHolePressure, "Static Bottom Hole Pressure");
            // PageAnalysis.getControlTextESPAnalysis(PageAnalysis.txtProductivityIndex, ProductivityIndex, "Productivity Index");
            PageAnalysis.getControlTextESPAnalysis(PageAnalysis.ddtxtStartNode, StartNode, "Start Node");
            PageAnalysis.getControlTextESPAnalysis(PageAnalysis.ddtxtSolutionNode, SolutionNode, "Solution Node");
            PageAnalysis.getControlTextESPAnalysis(PageAnalysis.ddtxtFBHP, FlowingBottomHolePressure, "Flowing Bottom Hole Pressure");
            PageAnalysis.getControlTextESPAnalysis(PageAnalysis.ddtxtSolutionNodePressure, SolutionNodePressure, "Solution Node Pressure");
            // PageAnalysis.getControlTextESPAnalysis(PageAnalysis.ddtxtLiquidRateFromIPR, LiqRatefromIPR, "LIQUID Rate from IPR");
            dtprev2.Dispose();
            #endregion
        }

        private void VerifyESPValuesdata(string filename)
        {
            string testdatafile1 = Path.Combine(FilesLocation + @"\UIAutomation\TestData", filename);
            CommonHelper.TraceLine("Getting Data from XML file ... " + testdatafile1);
            DataTable dtprev3 = CommonHelper.BuildDataTable(testdatafile1);
            // Create Varibles 
            #region VariableAssignment
            string TestDate1 = dtprev3.Rows[0]["TestDate"].ToString();
            string QualityCode1 = dtprev3.Rows[0]["QualityCode"].ToString();
            string MultiphaseFlowCorrelation1 = dtprev3.Rows[0]["MultiphaseFlowCorrelation"].ToString();
            string LFactor1 = dtprev3.Rows[0]["LFactor"].ToString();
            string HeadTuningFactor1 = dtprev3.Rows[0]["HeadTuningFactor"].ToString();
            string PumpIntakePressure1 = dtprev3.Rows[0]["PumpIntakePressure"].ToString();
            string PumpDischargePressure1 = dtprev3.Rows[0]["PumpDischargePressure"].ToString();
            string WellheadPressure1 = dtprev3.Rows[0]["WellheadPressure"].ToString();
            string WellheadTemperature1 = dtprev3.Rows[0]["WellheadTemperature"].ToString();
            string Frequency1 = dtprev3.Rows[0]["Frequency"].ToString();
            string WaterRate1 = dtprev3.Rows[0]["WaterRate"].ToString();
            string GasRate1 = dtprev3.Rows[0]["GasRate"].ToString();
            string LiquidRate1 = dtprev3.Rows[0]["LiquidRate"].ToString();
            //string FormationGOR = dtprev1.Rows[0]["FormationGOR"].ToString();
            //string WaterCut = dtprev1.Rows[0]["WaterCut"].ToString();
            string StaticBottomHolePressure1 = dtprev3.Rows[0]["StaticBottomHolePressure"].ToString();
            //string ProductivityIndex = dtprev1.Rows[0]["ProductivityIndex"].ToString();
            string StartNode1 = dtprev3.Rows[0]["StartNode"].ToString();
            string SolutionNode1 = dtprev3.Rows[0]["SolutionNode"].ToString();
            string FlowingBottomHolePressure1 = dtprev3.Rows[0]["FlowingBottomHolePressure"].ToString();
            string SolutionNodePressure1 = dtprev3.Rows[0]["SolutionNodePressure"].ToString();
            //string LiqRatefromIPR = dtprev1.Rows[0]["LiqRatefromIPR"].ToString();
            #endregion
            //Call From Xml
            #region ValidationofValuesforEachControl

            PageAnalysis.getControlTextESPAnalysis(PageAnalysis.ddtxtWellTestDate, TestDate1, "Test Date Time");
            PageAnalysis.getControlTextESPAnalysis(PageAnalysis.txtQualitycode, QualityCode1, "Quality Code");
            PageAnalysis.getControlTextESPAnalysis(PageAnalysis.ddtxtMultiphaseFlowCorrelation, MultiphaseFlowCorrelation1, "Multiphase Flow Correlation");
            PageAnalysis.getControlTextESPAnalysis(PageAnalysis.txtLFactor, LFactor1, "LFactor");
            PageAnalysis.getControlTextESPAnalysis(PageAnalysis.ddtxtHeadTuningFactor, HeadTuningFactor1, "Head Tuning Factor");
            PageAnalysis.getControlTextESPAnalysis(PageAnalysis.ddtxtPumpIntakePressure, PumpIntakePressure1, "Pump Intake Pressure");
            PageAnalysis.getControlTextESPAnalysis(PageAnalysis.ddtxtPumpDischargePressure, PumpDischargePressure1, "Pump Discharge Pressure");
            PageAnalysis.getControlTextESPAnalysis(PageAnalysis.txtWellheadPressure, WellheadPressure1, "Well head Pressure");
            PageAnalysis.getControlTextESPAnalysis(PageAnalysis.txtWellheadTemperature, WellheadTemperature1, "Well head Temperature");
            PageAnalysis.getControlTextESPAnalysis(PageAnalysis.ddtxtFrequency, Frequency1, "Frequency");
            PageAnalysis.getControlTextESPAnalysis(PageAnalysis.txtWaterRate, WaterRate1, "Water Rate");
            PageAnalysis.getControlTextESPAnalysis(PageAnalysis.txtGasRate, GasRate1, "Gas Rate");
            PageAnalysis.getControlTextESPAnalysis(PageAnalysis.txtLiquidRate, LiquidRate1, "Liquid Rate");
            //PageAnalysis.getControlTextESPAnalysis(PageAnalysis.txtFormationGOR ,FormationGOR, "Formation GOR");
            //PageAnalysis.getControlTextESPAnalysis(PageAnalysis.txtWaterCut, WaterCut, "Water Cut");
            PageAnalysis.getControlTextESPAnalysis(PageAnalysis.txtStaticBottomHolePressure, StaticBottomHolePressure1, "Static Bottom Hole Pressure");
            // PageAnalysis.getControlTextESPAnalysis(PageAnalysis.txtProductivityIndex, ProductivityIndex, "Productivity Index");
            PageAnalysis.getControlTextESPAnalysis(PageAnalysis.ddtxtStartNode, StartNode1, "Start Node");
            PageAnalysis.getControlTextESPAnalysis(PageAnalysis.ddtxtSolutionNode, SolutionNode1, "Solution Node");
            PageAnalysis.getControlTextESPAnalysis(PageAnalysis.ddtxtFBHP, FlowingBottomHolePressure1, "Flowing Bottom Hole Pressure");
            PageAnalysis.getControlTextESPAnalysis(PageAnalysis.ddtxtSolutionNodePressure, SolutionNodePressure1, "Solution Node Pressure");
            // PageAnalysis.getControlTextESPAnalysis(PageAnalysis.ddtxtLiquidRateFromIPR, LiqRatefromIPR, "LIQUID Rate from IPR");
            dtprev3.Dispose();
            #endregion
        }



        private void VerifyGLPrevValues(string filename)
        {
            string testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\TestData", filename);
            CommonHelper.TraceLine("Getting Data from XML file ... " + testdatafile);
            DataTable dtprev = CommonHelper.BuildDataTableFromXml(testdatafile);
            // Create Varibles 
            #region VariableAssignment
            string TestDate = dtprev.Rows[0]["TestDate"].ToString();
            string QualityCode = dtprev.Rows[0]["QualityCode"].ToString();
            string InjectionMethod = dtprev.Rows[0]["InjectionMethod"].ToString();
            string Depth = dtprev.Rows[0]["Depth"].ToString();
            string MultiphaseFlowCorrelation = dtprev.Rows[0]["MultiphaseFlowCorrelation"].ToString();
            string LFactor = dtprev.Rows[0]["LFactor"].ToString();
            string WellheadPressure = dtprev.Rows[0]["WellheadPressure"].ToString();
            string WellheadTemperature = dtprev.Rows[0]["WellheadTemperature"].ToString();
            string CasingHeadPressure = dtprev.Rows[0]["CasingHeadPressure"].ToString();
            string GasInjectionRate = dtprev.Rows[0]["GasInjectionRate"].ToString();
            string DownholeGauge = dtprev.Rows[0]["DownholeGauge"].ToString();
            string OilRate = dtprev.Rows[0]["OilRate"].ToString();
            string WaterRate = dtprev.Rows[0]["WaterRate"].ToString();
            string GasRate = dtprev.Rows[0]["GasRate"].ToString();
            string WaterCut = dtprev.Rows[0]["WaterCut"].ToString();
            string LiquidRate = dtprev.Rows[0]["LiquidRate"].ToString();
            string FormationGOR = dtprev.Rows[0]["FormationGOR"].ToString();
            string StaticBottomHolePressure = dtprev.Rows[0]["StaticBottomHolePressure"].ToString();
            string ProductivityIndex = dtprev.Rows[0]["ProductivityIndex"].ToString();
            string StartNode = dtprev.Rows[0]["StartNode"].ToString();
            string SolutionNode = dtprev.Rows[0]["SolutionNode"].ToString();
            string FlowingBottomHolePressure = dtprev.Rows[0]["FlowingBottomHolePressure"].ToString();
            string SolutionNodePressure = dtprev.Rows[0]["SolutionNodePressure"].ToString();
            string GasInjectionDepth = dtprev.Rows[0]["GasInjectionDepth"].ToString();
            string LiquidRatefromIPR = dtprev.Rows[0]["LiquidRatefromIPR"].ToString();
            #endregion
            //Call From Xml
            #region ValidationofValuesforEachControl
            //Verify Each control against default expected Value
            PageAnalysis.getControlTextPreviousWellTestGLAnalysis(PageAnalysis.ddtxtWellTestDate, TestDate, "Test Date Time");
            PageAnalysis.getControlTextPreviousWellTestGLAnalysis(PageAnalysis.txtQualitycode, QualityCode, "Quality Code");
            //PageAnalysis.getControlTextPreviousWellTestGLAnalysis(PageAnalysis.ddtxtInjectionMethod, InjectionMethod, "Injection Method");
            //PageAnalysis.getControlTextPreviousWellTestGLAnalysis(PageAnalysis.ddtxtDepth, Depth, "Depth");
            PageAnalysis.getControlTextPreviousWellTestGLAnalysis(PageAnalysis.ddtxtMultiphaseFlowCorrelation, MultiphaseFlowCorrelation, "Multiphase Flow Correlation");
            PageAnalysis.getControlTextPreviousWellTestGLAnalysis(PageAnalysis.txtLFactor, LFactor, "LFactor");
            PageAnalysis.getControlTextPreviousWellTestGLAnalysis(PageAnalysis.txtWellheadPressure, WellheadPressure, "Well head Pressure");
            PageAnalysis.getControlTextPreviousWellTestGLAnalysis(PageAnalysis.txtWellheadTemperature, WellheadTemperature, "Well head Temperature");
            PageAnalysis.getControlTextPreviousWellTestGLAnalysis(PageAnalysis.txtCasingHeadPressure, CasingHeadPressure, "Casing Head Pressure");
            PageAnalysis.getControlTextPreviousWellTestGLAnalysis(PageAnalysis.txtGasInjectionRate, GasInjectionRate, "Gas Injection Rate");
            PageAnalysis.getControlTextPreviousWellTestGLAnalysis(PageAnalysis.txtDownholeGauge, DownholeGauge, "Downhole Gauge");
            PageAnalysis.getControlTextPreviousWellTestGLAnalysis(PageAnalysis.txtOilRate, OilRate, "Oil Rate");
            PageAnalysis.getControlTextPreviousWellTestGLAnalysis(PageAnalysis.txtWaterRate, WaterRate, "Water Rate");
            PageAnalysis.getControlTextPreviousWellTestGLAnalysis(PageAnalysis.txtGasRate, GasRate, "Gas Rate");
            PageAnalysis.getControlTextPreviousWellTestGLAnalysis(PageAnalysis.txtLiquidRate, LiquidRate, "Liquid Rate");
            PageAnalysis.getControlTextPreviousWellTestGLAnalysis(PageAnalysis.txtFormationGOR, FormationGOR, "Formation GOR");
            PageAnalysis.getControlTextPreviousWellTestGLAnalysis(PageAnalysis.txtWaterCut, WaterCut, "Water Cut");
            PageAnalysis.getControlTextPreviousWellTestGLAnalysis(PageAnalysis.txtStaticBottomHolePressure, StaticBottomHolePressure, "Static Bottom Hole Pressure");
            PageAnalysis.getControlTextPreviousWellTestGLAnalysis(PageAnalysis.txtProductivityIndex, ProductivityIndex, "Productivity Index");
            PageAnalysis.getControlTextPreviousWellTestGLAnalysis(PageAnalysis.ddtxtStartNode, StartNode, "Start Node");
            PageAnalysis.getControlTextPreviousWellTestGLAnalysis(PageAnalysis.ddtxtSolutionNode, SolutionNode, "Solution Node");
            PageAnalysis.getControlTextPreviousWellTestGLAnalysis(PageAnalysis.ddtxtFBHP, FlowingBottomHolePressure, "Flowing Bottom Hole Pressure");
            //PageAnalysis.getControlTextPreviousWellTestGLAnalysis(PageAnalysis.ddtxtSolutionNodePressure, SolutionNodePressure, "Solution Node Pressure");
            //PageAnalysis.getControlTextPreviousWellTestGLAnalysis(PageAnalysis.ddtxtGasInjectionDepth, GasInjectionDepth, "Gas Injection Depth");
            //PageAnalysis.getControlTextPreviousWellTestGLAnalysis(PageAnalysis.ddtxtLiquidRateFromIPR, LiquidRatefromIPR, "Liquid Rate from IPR");


            #endregion
            //Change Units to Metric
        }

        private void VerifyESPPrevValues(string filename)
        {
            string testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\TestData", filename);
            CommonHelper.TraceLine("Getting Data from XML file ... " + testdatafile);
            DataTable dtprev = CommonHelper.BuildDataTableFromXml(testdatafile);
            // Create Varibles 
            #region VariableAssignment
            string TestDate = dtprev.Rows[0]["TestDate"].ToString();
            string QualityCode = dtprev.Rows[0]["QualityCode"].ToString();
            // string InjectionMethod = dtprev.Rows[0]["InjectionMethod"].ToString();
            // string Depth = dtprev.Rows[0]["Depth"].ToString();
            string MultiphaseFlowCorrelation = dtprev.Rows[0]["MultiphaseFlowCorrelation"].ToString();
            string LFactor = dtprev.Rows[0]["LFactor"].ToString();
            string WellheadPressure = dtprev.Rows[0]["WellheadPressure"].ToString();
            string WellheadTemperature = dtprev.Rows[0]["WellheadTemperature"].ToString();
            //  string CasingHeadPressure = dtprev.Rows[0]["CasingHeadPressure"].ToString();
            // string GasInjectionRate = dtprev.Rows[0]["GasInjectionRate"].ToString();
            // string DownholeGauge = dtprev.Rows[0]["DownholeGauge"].ToString();
            // string OilRate = dtprev.Rows[0]["OilRate"].ToString();
            string WaterRate = dtprev.Rows[0]["WaterRate"].ToString();
            string GasRate = dtprev.Rows[0]["GasRate"].ToString();
            // string WaterCut = dtprev.Rows[0]["WaterCut"].ToString();
            string LiquidRate = dtprev.Rows[0]["LiquidRate"].ToString();
            //string FormationGOR = dtprev.Rows[0]["FormationGOR"].ToString();
            string StaticBottomHolePressure = dtprev.Rows[0]["StaticBottomHolePressure"].ToString();
            //string ProductivityIndex = dtprev.Rows[0]["ProductivityIndex"].ToString();
            string StartNode = dtprev.Rows[0]["StartNode"].ToString();
            string SolutionNode = dtprev.Rows[0]["SolutionNode"].ToString();
            string FlowingBottomHolePressure = dtprev.Rows[0]["FlowingBottomHolePressure"].ToString();
            string SolutionNodePressure = dtprev.Rows[0]["SolutionNodePressure"].ToString();
            // string GasInjectionDepth = dtprev.Rows[0]["GasInjectionDepth"].ToString();
            // string LiquidRatefromIPR = dtprev.Rows[0]["LiqRatefromIPR"].ToString();
            #endregion
            //Call From Xml
            #region ValidationofValuesforEachControl
            //Verify Each control against default expected Value
            PageAnalysis.getControlTextPreviousWellTestESPAnalysis(PageAnalysis.ddtxtWellTestDate, TestDate, "Test Date Time");
            PageAnalysis.getControlTextPreviousWellTestESPAnalysis(PageAnalysis.txtQualitycode, QualityCode, "Quality Code");
            //PageAnalysis.getControlTextPreviousWellTestGLAnalysis(PageAnalysis.ddtxtInjectionMethod, InjectionMethod, "Injection Method");
            //PageAnalysis.getControlTextPreviousWellTestGLAnalysis(PageAnalysis.ddtxtDepth, Depth, "Depth");
            PageAnalysis.getControlTextPreviousWellTestESPAnalysis(PageAnalysis.ddtxtMultiphaseFlowCorrelation, MultiphaseFlowCorrelation, "Multiphase Flow Correlation");
            PageAnalysis.getControlTextPreviousWellTestESPAnalysis(PageAnalysis.txtLFactor, LFactor, "LFactor");
            PageAnalysis.getControlTextPreviousWellTestESPAnalysis(PageAnalysis.txtWellheadPressure, WellheadPressure, "Well head Pressure");
            PageAnalysis.getControlTextPreviousWellTestESPAnalysis(PageAnalysis.txtWellheadTemperature, WellheadTemperature, "Well head Temperature");
            //PageAnalysis.getControlTextPreviousWellTestESPAnalysis(PageAnalysis.txtCasingHeadPressure, CasingHeadPressure, "Casing Head Pressure");
            // PageAnalysis.getControlTextPreviousWellTestESPAnalysis(PageAnalysis.txtGasInjectionRate, GasInjectionRate, "Gas Injection Rate");
            // PageAnalysis.getControlTextPreviousWellTestESPAnalysis(PageAnalysis.txtDownholeGauge, DownholeGauge, "Downhole Gauge");
            //PageAnalysis.getControlTextPreviousWellTestESPAnalysis(PageAnalysis.txtOilRate, OilRate, "Oil Rate");
            PageAnalysis.getControlTextPreviousWellTestESPAnalysis(PageAnalysis.txtWaterRate, WaterRate, "Water Rate");
            PageAnalysis.getControlTextPreviousWellTestESPAnalysis(PageAnalysis.txtGasRate, GasRate, "Gas Rate");
            PageAnalysis.getControlTextPreviousWellTestESPAnalysis(PageAnalysis.txtLiquidRate, LiquidRate, "Liquid Rate");
            //PageAnalysis.getControlTextPreviousWellTestESPAnalysis(PageAnalysis.txtFormationGOR, FormationGOR, "Formation GOR");
            //PageAnalysis.getControlTextPreviousWellTestESPAnalysis(PageAnalysis.txtWaterCut, WaterCut, "Water Cut");
            PageAnalysis.getControlTextPreviousWellTestESPAnalysis(PageAnalysis.txtStaticBottomHolePressure, StaticBottomHolePressure, "Static Bottom Hole Pressure");
            // PageAnalysis.getControlTextPreviousWellTestESPAnalysis(PageAnalysis.txtProductivityIndex, ProductivityIndex, "Productivity Index");
            PageAnalysis.getControlTextPreviousWellTestESPAnalysis(PageAnalysis.ddtxtStartNode, StartNode, "Start Node");
            PageAnalysis.getControlTextPreviousWellTestESPAnalysis(PageAnalysis.ddtxtSolutionNode, SolutionNode, "Solution Node");
            PageAnalysis.getControlTextPreviousWellTestESPAnalysis(PageAnalysis.ddtxtFBHP, FlowingBottomHolePressure, "Flowing Bottom Hole Pressure");
            // PageAnalysis.getControlTextPreviousWellTestESPAnalysis(PageAnalysis.ddtxtSolutionNodePressure, SolutionNodePressure, "Solution Node Pressure");
            //PageAnalysis.getControlTextPreviousWellTestESPAnalysis(PageAnalysis.ddtxtGasInjectionDepth, GasInjectionDepth, "Gas Injection Depth");
            //PageAnalysis.getControlTextPreviousWellTestESPAnalysis(PageAnalysis.ddtxtLiquidRateFromIPR, LiquidRatefromIPR, "Liquid Rate from IPR");


            #endregion
            //Change Units to Metric
        }



        private string[] getDateandTimeFromString(string strdatstring)
        {
            string datepattern = "(\\d{1,2}/)+(\\d{1,4})";
            string timepattern = "(\\d{1,2}:)+\\d{1,2}\\s(AM|PM)";
            List<string> lst = new List<string>();
            Regex re1 = new Regex(datepattern);
            lst.Add(re1.Match(strdatstring).Value);
            Regex re2 = new Regex(timepattern);
            lst.Add(re2.Match(strdatstring).Value);
            return lst.ToArray();
        }

        private void VerifyNonUniformTable(HtmlTable tbl, string expfile)
        {
            try
            {

                string testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\TestData", expfile);
                CommonHelper.TraceLine("Getting Data from XML file ... " + testdatafile);
                DataTable dt = CommonHelper.BuildDataTableFromXml(testdatafile);
                int colcount = dt.Columns.Count;
                if (dt.Rows.Count != tbl.Rows.Count)
                {
                    CommonHelper.TraceLine("[***** Warning ***** ]: Expected and Actual tables records mismatch");
                }
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    CommonHelper.TraceLine("Verifying for Row number: " + (i + 1));
                    for (int j = 0; j < colcount; j++)
                    {
                        string expval = dt.Rows[i]["Col" + (j + 1)].ToString();
                        string actval = string.Empty;
                        if (tbl.Rows[i].Cells[j].BaseElement.Children.Count > 0)
                        {
                            HtmlControl childctl = TelerikObject.GetChildrenControl(tbl.Rows[i].Cells[j], "0");
                            string substring = childctl.BaseElement.InnerText;
                            actval = tbl.Rows[i].Cells[j].InnerText.Replace(substring, "");
                            actval = actval + " " + substring;
                        }
                        else
                        {

                            actval = tbl.Rows[i].Cells[j].InnerText;
                        }
                        CommonHelper.TraceLine("Actual Value " + actval);
                        TelerikObject.verifyValues(expval, actval);
                    }
                }

            }
            catch (Exception e)
            {
                CommonHelper.TraceLine("Expcetion " + e.Source);
                Assert.Fail("Failed in Unit Balacing Verfiication");
            }

        }
        private void VerifyLegendTextonChartForSlider(string cardcollected)
        {
            PageAnalysis.DynamicValue = cardcollected.Trim() + ": Surface";
            Assert.IsNotNull(PageAnalysis.legend_Text, "Legend for " + cardcollected + ": Surface" + " Is not shown on UI");
            CommonHelper.TraceLine("***** The Legend text for Dynagraph Chart for -- " + cardcollected + ": Surface" + "  appeared on UI");

            PageAnalysis.DynamicValue = cardcollected.Trim() + ": Controller Downhole";
            Assert.IsNotNull(PageAnalysis.legend_Text, "***** The Legend for " + cardcollected + ": Controller Downhole" + " Is not shown on UI");
            CommonHelper.TraceLine("***** TheThe Legend text for Dynagraph Chart for " + cardcollected + ": Controller Downhole" + "  appeared on UI");

            PageAnalysis.DynamicValue = cardcollected.Trim() + ": Everitt Jennings Downhole";
            Assert.IsNotNull(PageAnalysis.legend_Text, "***** The Legend for " + cardcollected + ": Everitt Jennings Downhole" + " Is not shown on UI");
            CommonHelper.TraceLine("***** TheThe Legend text for Dynagraph Chart for " + cardcollected + ": Everitt Jennings Downhole" + "  appeared on UI");
        }
        public void VerifyCardDescriptionLabelText(HtmlControl ctl, string cardtype)
        {
            TelerikObject.Click(PageAnalysis.btnScanCards);
            Thread.Sleep(10000);

            TelerikObject.Click(ctl);
            TelerikObject.Toastercheck(PageAnalysis.toasterControl, cardtype, "Scan card request sent.");
            Assert.IsNotNull(PageAnalysis.lblDonwholeControllerCardCollection, "After Scan " + cardtype + " Downhole Card did not appear");
            CommonHelper.TraceLine("Downhole Controller Description  appears after issuing scan command");

            string cardtypeandtimestamp = TelerikObject.GetChildrenControl(PageAnalysis.lblDonwholeControllerCardCollection, "0").BaseElement.InnerText;
            string cardtypetext = TelerikObject.GetChildrenControl(PageAnalysis.lblDonwholeControllerCardCollection, "1").BaseElement.InnerText;
            CommonHelper.TraceLine(" 1. Card Type and Time stamp is " + cardtypeandtimestamp);
            string cardtype_short = cardtype.Replace("Card", "");

            CommonHelper.TraceLine(" 2.  Card Type : Controller card or Calculated Card ?   :" + cardtypetext);
            Assert.AreEqual("- Downhole - Controller", cardtypetext, "Card collected is not downhole controller for first time");


            string datepattern = "(\\d{1,2}/)+(\\d{1,4})";
            string timepattern = "(\\d{1,2}:)+\\d{1,2}\\s(AM|PM)";
            Regex re1 = new Regex(datepattern);
            CommonHelper.TraceLine("Date  captured using regular expression is " + re1.Match(cardtypeandtimestamp).Value);
            CommonHelper.TraceLine("Card For Comparion on Timestamp of Today: " + cardtype_short.Trim().ToUpper());
            if (cardtype_short.Trim().ToUpper() != "PUMPOFF" && cardtype_short.Trim().ToUpper() != "ALARM" && cardtype_short.Trim().ToUpper() != "FAILURE")
            {
                Assert.AreEqual(DateTime.Today.ToString("MM/dd/yyyy"), re1.Match(cardtypeandtimestamp).Value, "Card is Not having Today Date Stamp");
            }
            else
            {
                CommonHelper.TraceLine("Assertion was skipped as Card Type was: " + cardtype_short.Trim().ToUpper());
            }
            CommonHelper.TraceLine(string.Format("Card Description for {0} shows Todays Date in Time Stamp text:  {1}", cardtype, re1.Match(cardtypeandtimestamp).Value));
            Regex re2 = new Regex(timepattern);
            Assert.IsNotNull(re2.Match(cardtypeandtimestamp).Value, "Time Stamp value is Null");

            string cardcollected = "";
            cardcollected = re1.Replace(cardtypeandtimestamp, "");
            cardcollected = re2.Replace(cardcollected, "");

            CommonHelper.TraceLine("******* Card Type Value ******* " + cardcollected);
            Assert.AreEqual(cardtype_short.Trim(), cardcollected.Trim(), cardtype + " was not collected after Scan card command");

            CommonHelper.TraceLine("Time captured using regular expression is " + re2.Match(cardtypeandtimestamp).Value);
            PageAnalysis.checkforElementState(PageAnalysis.btnrunAnalysis, "Enabled", "Run Analysis Button");
            PageAnalysis.DynamicValue = cardcollected.Trim() + ": Surface";
            Assert.IsNotNull(PageAnalysis.legend_Text, "Legend for " + cardcollected + ": Surface" + " Is not shown on UI");
            CommonHelper.TraceLine("***** The Legend text for Dynagraph Chart for -- " + cardcollected + ": Surface" + "  appeared on UI");

            PageAnalysis.DynamicValue = cardcollected.Trim() + ": Controller Downhole";
            Assert.IsNotNull(PageAnalysis.legend_Text, "***** The Legend for " + cardcollected + ": Controller Downhole" + " Is not shown on UI");
            CommonHelper.TraceLine("***** TheThe Legend text for Dynagraph Chart for " + cardcollected + ": Controller Downhole" + "  appeared on UI");
            CommonHelper.PrintScreen(cardtype_short);
        }
    }
}
