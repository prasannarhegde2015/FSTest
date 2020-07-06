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
using System.Text.RegularExpressions;
using Weatherford.POP.Enums;


namespace SeleniumAutomation.TestClasses
{
    class WellAnalysis : PageObjects.WellAnalysisPage
    {
        public static string FilesLocation = ConfigurationManager.AppSettings.Get("FilesLocation");
        WellConfiguration config = new WellConfiguration();
        WellConfigurationPage configobj = new WellConfigurationPage();
        WellTest welltest = new WellTest();
        WellStatus wellstatus = new WellStatus();
        public void RRLWellAnalysis(ExtentTest test)
        {
            // *********** Create New FULL RRL Well  *******************

            try
            {
                config.CreateRRLWellFullonBlankDB(test);
                // CalcualteTorquefromUI(test);
                //Scan Cards since on ATS we dont have associated Set Points data group polled
                SeleniumActions.waitClick(PageObjects.DashboardPage.tabSurveillace);
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(PageObjects.DashboardPage.tabWellStatus);
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(PageObjects.WellStatusPage.btnSetPoints);
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(PageObjects.WellStatusPage.btnBasicSetpointGetvalues);
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(PageObjects.WellStatusPage.btnCloseSetPointModal);
                scancards(test);
                Continuouscardcollection();
                PatternMatching();
                RunAnalysis(test);
                AnalysisReport(test);
                CheckCalibration();
                Verify_PumpFillPercentVSDToleranceShutDownLimitsFluidLoadLines(test);
                SeleniumActions.WaitForLoad();
                CardRangeSliderAndGenerateReportforeachcard(test);
                // CheckUnitBalancing();
                config.DeleteWellByUI(TestData.WellConfigData.RRFACName, test);
                test.Pass("RRL Well Creation Configuration Smoke Finished");
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("RRLWellAnalysis");
                CommonHelper.TraceLine("Error in RRLWellAnalysis " + e.Message);
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "RRLWellAnalysis" + ".png"))).Build());
                Assert.Fail(e.ToString());

            }
            finally
            {
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.wellconfigurationTab);


                // *********** Dispose  ******************
                SeleniumActions.disposeDriver();
            }
        }
        /***
        public void CalcualteTorquefromUI(ExtentTest test)
        {
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.tabWeights);
            Thread.Sleep(2000);
       SeleniumActions.waitClick(PageObjects.WellConfigurationPage.btncalculate);
            Thread.Sleep(3000);
          string cbt = SeleniumActions.getAttribute(PageObjects.WellConfigurationPage.txtcbt, "value");
            Assert.AreEqual(cbt, "1,169.06", "Calculated CBT is not correct");
            Thread.Sleep(2000);
            SeleniumActions.waitClick(PageObjects.WellConfigurationPage.savebutton);
            test.Info("CBT is calculated");
        }
        */
        public void scancards(ExtentTest test)
        {
            SeleniumActions.WaitForLoad();
            Clickanalysis();
            SeleniumActions.send_Text(PageObjects.WellAnalysisPage.txtstartdate, "01012008");
            PreCardCollectionCheck();
            PostCardCollectionCheck();
            test.Info("Cards verified before and after collection");
        }
        public static void PreCardCollectionCheck()
        {

            Thread.Sleep(2000);

            if (SeleniumActions.Gettotalrecords(PageObjects.WellAnalysisPage.dynacardoverlayprecard) == 1)
            {
                string nocardstext = SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.dynacardoverlayprecard, "Text");
                Assert.AreEqual("No Card Selected.", nocardstext, "When Cygnet Facility having no cards is selected ,Cards are still seen in ForeSite");
                SeleniumActions.waitClick(PageObjects.WellAnalysisPage.lnkcardlib);
                SeleniumActions.WaitForLoad();
                string cardlibcount = SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.txttotalitems, "Text");
                // Assert.AreEqual("Total: 0 items", cardlibcount, "Card library count is not 0");
                Thread.Sleep(2000);
                Assert.AreEqual(Checkforelementstate(PageObjects.WellAnalysisPage.btnScanCards), true, "Scan cards is not enabled");
                Assert.AreEqual(Checkforelementstate(PageObjects.WellAnalysisPage.patternmatching), false, "Run Pattern matching is not disabled");
                Assert.AreEqual(Checkforelementstate(PageObjects.WellAnalysisPage.btnclearcards), true, "Clear cards is not enabled.");
                Assert.AreEqual(Checkforelementstate(PageObjects.WellAnalysisPage.Analysisoptions), true, "Analysis options is not enabled.");
                Assert.AreEqual(Checkforelementstate(PageObjects.WellAnalysisPage.lnkcardlib), true, "Card library is not enabled.");
                Assert.AreEqual(Checkforelementstate(PageObjects.WellAnalysisPage.btnshowhide), true, "Show/Hide is not enabled.");
                Assert.AreEqual(Checkforelementstate(PageObjects.WellAnalysisPage.btnanalysisreport), false, "Analysis options is not disabled.");
                Assert.AreEqual(Checkforelementstate(PageObjects.WellAnalysisPage.btncalibrate), false, "Calibration card is not disabled.");
                Assert.AreEqual(Checkforelementstate(PageObjects.WellAnalysisPage.btnleftCard), false, "Left navigation card is not disabled.");
                Assert.AreEqual(Checkforelementstate(PageObjects.WellAnalysisPage.btnrightcard), false, "Right navigation card is not disabled.");
            }

        }
        public static void PostCardCollectionCheck()
        {

            SeleniumActions.waitClick(PageObjects.WellAnalysisPage.btnScanCards);
            SeleniumActions.waitClick(PageObjects.WellAnalysisPage.lnkcurrentcard);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.WellAnalysisPage.btnScanCards);
            SeleniumActions.waitClick(PageObjects.WellAnalysisPage.lnkfullcard);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.WellAnalysisPage.btnScanCards);
            SeleniumActions.waitClick(PageObjects.WellAnalysisPage.lnkalarmcard);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.WellAnalysisPage.btnScanCards);
            SeleniumActions.waitClick(PageObjects.WellAnalysisPage.lnkpumpoffcard);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.WellAnalysisPage.btnScanCards);
            SeleniumActions.waitClick(PageObjects.WellAnalysisPage.lnkfailurecard);
            SeleniumActions.WaitForLoad();

            SeleniumActions.waitClickJS(PageObjects.WellAnalysisPage.tabwelltest);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClickJS(PageObjects.WellAnalysisPage.tabanalysis);
            SeleniumActions.WaitForLoad();

            SeleniumActions.waitClick(PageObjects.WellAnalysisPage.lnkcardlib);
            SeleniumActions.WaitForLoad();
            int cardlibcount = Convert.ToInt32(System.Text.RegularExpressions.Regex.Match(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.txttotalitems, "Text"), @"\d+").Value);
            // Assert.AreEqual(5, cardlibcount, "Card library count is not 5");

            Assert.AreEqual(Checkforelementstate(PageObjects.WellAnalysisPage.btnScanCards), true, "Scan cards is not enabled");
            Assert.AreEqual(Checkforelementstate(PageObjects.WellAnalysisPage.patternmatching), true, "Run Pattern matching is not enabled");
            Assert.AreEqual(Checkforelementstate(PageObjects.WellAnalysisPage.btnclearcards), true, "Clear cards is not enabled.");
            Assert.AreEqual(Checkforelementstate(PageObjects.WellAnalysisPage.Analysisoptions), true, "Analysis options is not enabled.");
            Assert.AreEqual(Checkforelementstate(PageObjects.WellAnalysisPage.lnkcardlib), true, "Card library is not enabled.");
            Assert.AreEqual(Checkforelementstate(PageObjects.WellAnalysisPage.btnshowhide), true, "Show/Hide is not enabled.");
            Assert.AreEqual(Checkforelementstate(PageObjects.WellAnalysisPage.btnanalysisreport), true, "Analysis options is not enabled.");
            Assert.AreEqual(Checkforelementstate(PageObjects.WellAnalysisPage.btncalibrate), true, "Calibration card is not enabled.");
            Assert.AreEqual(Checkforelementstate(PageObjects.WellAnalysisPage.btnleftCard), true, "Left navigation card is not enabled.");
            Assert.AreEqual(Checkforelementstate(PageObjects.WellAnalysisPage.btnrightcard), true, "Right navigation card is not enabled.");
        }

        public void Clickanalysis()
        {
            SeleniumActions.waitClickJS(PageObjects.WellAnalysisPage.taboptimization);
            SeleniumActions.waitClickJS(PageObjects.WellAnalysisPage.tabanalysis);
        }
        public static dynamic Checkforelementstate(By ele)
        {

            if (SeleniumActions.getEnabledState(ele))
            {
                return true;
            }

            else
                return false;

        }
        public void Continuouscardcollection()
        {

            SeleniumActions.waitClickJS(PageObjects.WellAnalysisPage.lnkscancards);
            SeleniumActions.waitClick(PageObjects.WellAnalysisPage.lnkcontinuouscollection);
            SeleniumActions.WaitForLoad();
            SeleniumActions.sendText(PageObjects.WellAnalysisPage.txtcollectionintrvl, "5");
            SeleniumActions.sendText(PageObjects.WellAnalysisPage.txtnoofcards, "20");
            SeleniumActions.sendText(PageObjects.WellAnalysisPage.txtcompfails, "2");
            SeleniumActions.waitClick(PageObjects.WellAnalysisPage.btnstart);
            string cardscollected = "";

            for (int k = 0; k < 20; k++)
            {

                cardscollected = "Cards Collected: " + k;
                CommonHelper.TraceLine("*****Looking for Card Collection Text: ******** " + "Cards Collected: " + k);
                Assert.IsNotNull(PageObjects.WellAnalysisPage.divCardCollectionStatus, "Card Collection Status is not null");
                CommonHelper.TraceLine("Card Collection text is : " + "Cards Collected: " + k);
                string comfailtxt = "Comms Fail: 0";
                cardscollected = comfailtxt;
                SeleniumActions.WaitForLoad();

            }


            Thread.Sleep(35000);

            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClickJS(PageObjects.WellAnalysisPage.tabwelltest);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClickJS(PageObjects.WellAnalysisPage.tabanalysis);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClickJS(PageObjects.WellAnalysisPage.lnkcardlib);

            // VerifyCardLibraryCount("Post Continuous Card collection", 25);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClickJS(PageObjects.WellAnalysisPage.lnkcardlib);
        }
        public void PatternMatching()
        {
            try
            {
                SeleniumActions.waitClick(PageObjects.WellAnalysisPage.btnclearcards);
                string minirerpotData = "Mini Report data not available.";
                Assert.AreEqual(minirerpotData, SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.minireportpane, "Text"), "Mismatch in Mini report Text");
                CommonHelper.TraceLine("Mini Report was Succesfully matched for No Cards condition.");
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(PageObjects.WellAnalysisPage.lnkcardlib);
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(PageObjects.WellAnalysisPage.cardlibtable);
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(PageObjects.WellAnalysisPage.patternmatching);
                SeleniumActions.WaitForLoad();
                Thread.Sleep(3000);
                int i = 0;
                string colvals = "";

                for (i = 0; i < TestData.RRLPatternMatching.dt.Rows.Count; i++)
                {
                    string overallmatchvalue = Math.Round(Convert.ToDouble(TestData.RRLPatternMatching.dt.Rows[i]["OverallMatch"].ToString()) * 100, 0).ToString() + "%";
                    string descriptionvalue = TestData.RRLPatternMatching.dt.Rows[i]["Description"].ToString();
                    string surfacematchvalue = Math.Round(Convert.ToDouble(TestData.RRLPatternMatching.dt.Rows[i]["SurfaceMatch"].ToString()) * 100, 0).ToString() + "%";
                    string downholematchvalue = Math.Round(Convert.ToDouble(TestData.RRLPatternMatching.dt.Rows[i]["DownholeMatch"].ToString()) * 100, 0).ToString() + "%";
                    colvals = overallmatchvalue + ";" + descriptionvalue + ";" + surfacematchvalue + ";" + downholematchvalue;
                    SeleniumActions.verifytabledata(colvals, i + 1);
                }
                CommonHelper.TraceLine("******** Table Verification of Results Tab of Patern Matching was Succesfully completed ********* ");
                //**********verify Update Button Functionality**********
                SeleniumActions.waitClickJS(PageObjects.WellAnalysisPage.tabpatternmatchinglib);
                Thread.Sleep(2000);
                SeleniumActions.WaitForLoad();
                int K = SeleniumActions.Gettotalrecordsinlist("xpath", PageObjects.WellAnalysisPage.patternmatchinglibrows).Count;
                SeleniumActions.waitClick(PageObjects.WellAnalysisPage.tabpatternmatchingresults);
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(PageObjects.WellAnalysisPage.btnsavecollectedcard);
                SeleniumActions.WaitForLoad();
                SeleniumActions.send_Text(PageObjects.WellAnalysisPage.txtsavecarddesc, "Samplecardcollected");
                SeleniumActions.waitClick(PageObjects.WellAnalysisPage.btnpatternmatchingsave);
                SeleniumActions.WaitForLoad();
                Thread.Sleep(3000);

                SeleniumActions.waitClickJS(PageObjects.WellAnalysisPage.tabpatternmatchinglib);
                // Assert.AreEqual(K + 1, SeleniumActions.Gettotalrecordsinlist("xpath", PageObjects.WellAnalysisPage.patternmatchinglibrows).Count, "Collected card not added to library");
                SeleniumActions.WaitForLoad();
                foreach (IWebElement row in SeleniumActions.Gettotalrecordsinlist("xpath", PageObjects.WellAnalysisPage.patternmatchinglibrows))
                {
                    System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> rows = row.FindElements(By.TagName("td"));

                    foreach (IWebElement cell in rows)
                    {
                        if (cell.Text.Equals("Samplecardcollected"))
                        {
                            SeleniumActions.waitClick(row);
                            SeleniumActions.WaitForLoad();
                            SeleniumActions.waitClick(PageObjects.WellAnalysisPage.btnUpdate);
                            SeleniumActions.WaitForLoad();
                            SeleniumActions.waitClick(PageObjects.WellAnalysisPage.chkboxsurfacecardcomp);
                            SeleniumActions.waitClick(PageObjects.WellAnalysisPage.chkboxdownholecardcomp);
                            SeleniumActions.waitClick(PageObjects.WellAnalysisPage.chkboxgeneratealarm);
                            SeleniumActions.WaitForLoad();
                            SeleniumActions.waitClick(PageObjects.WellAnalysisPage.btnupdatecardlibrary);
                            SeleniumActions.WaitForLoad();
                            goto verifycardadded;

                        }
                    }

                }
                verifycardadded:
                {
                    foreach (IWebElement row in SeleniumActions.Gettotalrecordsinlist("xpath", PageObjects.WellAnalysisPage.patternmatchinglibrows))
                    {
                        System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> rows = row.FindElements(By.TagName("td"));

                        foreach (IWebElement cell in rows)
                        {
                            if (cell.Text.Equals("Samplecardcollected"))
                            {
                                SeleniumActions.waitClick(row);
                                Assert.AreEqual(rows[1].Text, "Surface, Downhole", "Pattern card optiosn were not updated");
                                Assert.AreEqual(rows[1].Enabled, true, "Alarm toggle is not enabled");
                            }
                        }



                    }
                }
                SeleniumActions.waitClick(PageObjects.WellAnalysisPage.btnpatternmatchingdelete);
                SeleniumActions.WaitForLoad();
                string confirmtext = SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.txtcarddeletecfm, "Text") + SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.txtcardname, "Text");
                Assert.AreEqual(confirmtext.ToString(), "Are you sure you want to delete the selected Pattern Library card listed below? Deleting the data is not recoverable.Samplecardcollected", "Confirmation Message did not show up for card Deleletion");
                SeleniumActions.waitClick(PageObjects.WellAnalysisPage.btncarddelete);
                SeleniumActions.WaitForLoad();
                Assert.AreEqual(K, SeleniumActions.Gettotalrecordsinlist("xpath", PageObjects.WellAnalysisPage.patternmatchinglibrows).Count, "Collected card not deleted from library");



            }
            catch (Exception e2)
            {
                CommonHelper.TraceLine("Exception from  Pattern Library match" + e2);
                Assert.Fail("Failure in Run Pattern Matching");
            }


        }

        public void RunAnalysis(ExtentTest test)
        {
            try
            {

                //Call Add well test from UI to get Downhole Calcualtions Card...
                Thread.Sleep(2000);
                Clickwelltest();
                SeleniumActions.WaitForLoad();
                welltest.CreateRRLWellTest(test);
                SeleniumActions.waitClick(PageObjects.WellAnalysisPage.tabanalysis);
                SeleniumActions.WaitForLoad();
                Thread.Sleep(2000);
                SeleniumActions.waitClickJS(PageObjects.WellAnalysisPage.btnrunanalysis);
                if (SeleniumActions.Gettotalrecords(PageObjects.WellAnalysisPage.btnrunanalysiscfm) > 0)
                {
                    SeleniumActions.waitClickJS(PageObjects.WellAnalysisPage.btnrunanalysiscfm);
                }

                //
                SeleniumActions.WaitForLoad();
                Thread.Sleep(2000);
                // string minirerpotData = "L-C-912.00(92)-365(67)-168(90.0)@11.0 SPM w/1.66Stresses:D-1.000(107)D-0.750(157)D-0.875(79)RT (6.8)@927% PE79% PF";
                string minirerpotData = "L-C-912.00(135)-365(67)-168(90.0)@11.0 SPM w/1.66Stresses:EL-0.750(109)C-1.000(143)D-1.000(107)RT (6.8)@1010% PE3% PF";
                Assert.AreEqual(minirerpotData, SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.lblminireport, "Text").Replace("\r\n", ""), "Mismatch in Mini report Text");
                CommonHelper.TraceLine("Mini Report was Succesfully matched");

            }
            catch (Exception e)
            {
                CommonHelper.PrintScreen("Error_RunAnalysis");
                Thread.Sleep(2000);
                Assert.Fail("Failure  in RunAnalysis" + e.ToString());
            }

        }
        public void CheckUnitBalancing()
        {
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClickJS(PageObjects.WellAnalysisPage.tabwelltest);
            SeleniumActions.waitClickJS(PageObjects.WellAnalysisPage.tabanalysis);

            SeleniumActions.waitClickJS(PageObjects.WellAnalysisPage.btnunitbalancing);
            SeleniumActions.WaitForLoad();
            SeleniumActions.sendText(PageObjects.WellAnalysisPage.txtCBT, "1169.03");
            SeleniumActions.waitClickJS(PageObjects.WellAnalysisPage.btncalculatecbt);
            SeleniumActions.WaitForLoad();
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.lblunitbalancing, "Text"), "Unit Balancing Details", "");
            SeleniumActions.WaitForLoad();
            VerifyNonUniformTable(PageObjects.WellAnalysisPage.tblexistcrank1leadlag, "EC1LeadLagPoition.xml");
            VerifyNonUniformTable(PageObjects.WellAnalysisPage.tblexistcrank1prim, "EC1PrimaryWeight.xml");
            VerifyNonUniformTable(PageObjects.WellAnalysisPage.tblexistcrank1aux, "EC1Auxililary.xml");
            VerifyNonUniformTable(PageObjects.WellAnalysisPage.tblexistcrank2leadlag, "EC2LeadLagPoition.xml");
            VerifyNonUniformTable(PageObjects.WellAnalysisPage.tblexistcrank2prim, "EC2PrimaryWeight.xml");
            VerifyNonUniformTable(PageObjects.WellAnalysisPage.tblexistcrank2aux, "EC2Auxililary.xml");
            VerifyNonUniformTable(PageObjects.WellAnalysisPage.tbldescrank1leadlag, "DC1LeadLagPoition.xml");
            VerifyNonUniformTable(PageObjects.WellAnalysisPage.tbldescrank1prim, "DC1PrimaryWeight.xml");
            VerifyNonUniformTable(PageObjects.WellAnalysisPage.tbldescrank1aux, "DC1Auxililary.xml");
            VerifyNonUniformTable(PageObjects.WellAnalysisPage.tbldescrank2leadlag, "DC2LeadLagPoition.xml");
            VerifyNonUniformTable(PageObjects.WellAnalysisPage.tbldescrank2prim, "DC2PrimaryWeight.xml");
            VerifyNonUniformTable(PageObjects.WellAnalysisPage.tbldescrank2aux, "DC2Auxililary.xml");
        }

        private void VerifyNonUniformTable(string tbl, string expfile)
        {
            try
            {

                string testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\Smoke_TestData", expfile);
                CommonHelper.TraceLine("Getting Data from XML file ... " + testdatafile);
                DataTable dt = CommonHelper.BuildDataTableFromXml(testdatafile);
                int colcount = dt.Columns.Count;

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    CommonHelper.TraceLine("Verifying for Row number: " + (i + 1));
                    for (int j = 0; j < colcount; j++)
                    {
                        string expval = dt.Rows[i]["Col" + (j + 1)].ToString();
                        string actval = string.Empty;
                        actval = SeleniumActions.gettextfromwebtable(tbl, i + 1, j + 1);
                        Assert.AreEqual(expval, actval, expval + "NotFiniteNumberException displayed in UI");

                    }
                }
            }
            catch (Exception e)
            {
                CommonHelper.TraceLine("Expcetion " + e.Source);
                Assert.Fail("Failed in Unit Balacing Verfiication");
            }

        }

        public void CardRangeSliderAndGenerateReportforeachcard(ExtentTest test)
        {
            try
            {
                SeleniumActions.waitClickJS(PageObjects.WellAnalysisPage.tabwelltest);
                SeleniumActions.waitClickJS(PageObjects.WellAnalysisPage.tabanalysis);
                SeleniumActions.send_Text(PageObjects.WellAnalysisPage.txtstartdate, "1/1/2008");

                Thread.Sleep(2000);
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(PageObjects.WellAnalysisPage.lnkcardlib);
                SeleniumActions.WaitForLoad();
                int lblCardlibraryCount = Convert.ToInt32(Regex.Match(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.txttotalitems, "Text"), @"\d+").Value);
                //   int assertfailcount = 0;

                List<Tuple<int, string, string>> cardCollectionData = new List<Tuple<int, string, string>>();
                for (int i = 0; i < lblCardlibraryCount; i++)
                {
                    cardCollectionData.Add(new Tuple<int, string, string>(i, SeleniumActions.getTextAtIndex(PageObjects.WellAnalysisPage.tblcardlib, 0, i, false), SeleniumActions.getTextAtIndex(PageObjects.WellAnalysisPage.tblcardlib, 1, i, false)));
                }
                //Clear Cards ********
                SeleniumActions.waitClick(PageObjects.WellAnalysisPage.btnclearcards);
                Thread.Sleep(2000);
                SeleniumActions.waitClickJS(PageObjects.WellAnalysisPage.btnshowhide);
                SeleniumActions.waitClick(PageObjects.WellAnalysisPage.chkboxcardrangeslider);
                SeleniumActions.WaitForLoad();
                Assert.AreEqual(SeleniumActions.getAttribute(PageObjects.WellAnalysisPage.lnkcardrangeslider, "max"), lblCardlibraryCount.ToString(), "Card Library slider range is not correct");
                //  SeleniumActions.waitClick(PageObjects.WellAnalysisPage.lnkcardrangeslider);
                /***
                string minirerpotDataCurrent = "L-C-912.00(92)-365(67)-168(90.0)@11.0 SPM w/1.66Stresses:D-1.000(107)D-0.750(157)D-0.875(79)RT (6.8)@927% PE79% PF";
                string minirerpotDataFull = "L-C-912.00(87)-365(65)-168(90.0)@11.0 SPM w/1.66Stresses:D-1.000(99)D-0.750(146)D-0.875(77)RT (6.8)@756% PE97% PF";
                string minirerpotDataPumpOff = "L-C-912.00(94)-365(67)-168(90.0)@11.0 SPM w/1.66Stresses:D-1.000(109)D-0.750(159)D-0.875(81)RT (6.8)@1038% PE73% PF";
                string minirerpotDataAlarm = "L-C-912.00(88)-365(65)-168(90.0)@11.0 SPM w/1.66Stresses:D-1.000(100)D-0.750(149)D-0.875(77)RT (6.8)@769% PE96% PF";
                string minirerpotDataFailure = "L-C-912.00(87)-365(64)-168(90.0)@11.0 SPM w/1.66Stresses:D-1.000(99)D-0.750(146)D-0.875(77)RT (6.8)@756% PE97% PF";
                */
                string minirerpotDataCurrent = "L-C-912.00(135)-365(67)-168(90.0)@11.0 SPM w/1.66Stresses:EL-0.750(109)C-1.000(143)D-1.000(107)RT (6.8)@1010% PE3% PF";
                string minirerpotDataFull = "L-C-912.00(132)-365(65)-168(90.0)@11.0 SPM w/1.66Stresses:EL-0.750(103)C-1.000(134)D-1.000(100)RT (6.8)@743% PE93% PF";
                string minirerpotDataPumpOff = "L-C-912.00(136)-365(67)-168(90.0)@11.0 SPM w/1.66Stresses:EL-0.750(109)C-1.000(144)D-1.000(109)RT (6.8)@1122% PE3% PF";
                string minirerpotDataAlarm = "L-C-912.00(132)-365(65)-168(90.0)@11.0 SPM w/1.66Stresses:EL-0.750(103)C-1.000(135)D-1.000(100)RT (6.8)@745% PE18% PF";
                string minirerpotDataFailure = "L-C-912.00(132)-365(64)-168(90.0)@11.0 SPM w/1.66Stresses:EL-0.750(102)C-1.000(134)D-1.000(99)RT (6.8)@728% PE90% PF";
                SeleniumActions.WaitForLoad();


                for (int k = 0; k < lblCardlibraryCount; k++)
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

                    IWebElement cardtype = SeleniumActions.Getelementbyrow("xpath", PageObjects.WellAnalysisPage.lnkcardlibrow, k + 1);
                    SeleniumActions.WaitForLoad();
                    cardtype.Click();
                    SeleniumActions.WaitForLoad();
                    Thread.Sleep(2000);
                    SeleniumActions.waitClickJS(PageObjects.WellAnalysisPage.btnrunanalysis);
                    SeleniumActions.WaitForLoad();
                    Thread.Sleep(2000);
                    if (SeleniumActions.Gettotalrecords(PageObjects.WellAnalysisPage.btnrunanalysiscfm) > 0)
                    {
                        SeleniumActions.waitClickJS(PageObjects.WellAnalysisPage.btnrunanalysiscfm);
                    }
                    SeleniumActions.WaitForLoad();
                    CommonHelper.TraceLine("Card Library Serial Number :" + (Convert.ToInt32(cardCollectionData[k].Item1) + 1));

                    SeleniumActions.takeScreenshot("CardLibrary" + cardCollectionData[k].Item1);
                    SeleniumActions.WaitForLoad();
                    test.Info($"Card Library Information: for  card  {cardCollectionData[k].Item1}", MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "CardLibrary" + cardCollectionData[k].Item1 + ".png"))).Build());
                    string cardtypeandtimestamp = SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.lnkcardtypeandtimestamp, "Text");
                    string cardtypetext = SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.lnkcardtypetext, "Text");


                    CommonHelper.TraceLine("Card Time Stamp: " + cardtypeandtimestamp);
                    CommonHelper.TraceLine("Card Type: " + cardtypetext);
                    CommonHelper.TraceLine("Card Library Type :" + cardCollectionData[k].Item2);
                    CommonHelper.TraceLine("Card Library TimeStamp:" + cardCollectionData[k].Item3);
                    Assert.AreEqual(cardCollectionData[k].Item2 + " " + Convert.ToDateTime(cardCollectionData[k].Item3).ToString("MM/dd/yyyy hh:mm:ss tt").ToString(), cardtypeandtimestamp, "Card type and timestamp does not matches with displayed in UI");


                    Assert.AreEqual(expMiniReport, SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.lblminireport, "Text").Replace("\r\n", ""), "Mismatch in Mini report Text for " + cardCollectionData[k].Item1 + " " + cardCollectionData[k].Item2 + " " + cardCollectionData[k].Item3);


                    SeleniumActions.waitClick(PageObjects.WellAnalysisPage.btnclearcards);
                    SeleniumActions.WaitForLoad();
                    if (k == lblCardlibraryCount - 1)
                    {
                        break;
                    }
                }

            }
            finally
            {
                SeleniumActions.waitClick(PageObjects.WellAnalysisPage.lnkcardlib);
            }

        }

        public void verifycommentsadded()
        {
            SeleniumActions.waitClick(PageObjects.WellAnalysisPage.chkboxcomments);
            SeleniumActions.sendText(PageObjects.WellAnalysisPage.txtareacomment, "RRL Well Analysis Comments: By Automation ATS");
            SeleniumActions.waitClick(PageObjects.WellAnalysisPage.btnsubmit);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.WellAnalysisPage.btndelete);
            Assert.AreEqual("Are you sure you want to delete this comment?", SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.txtcommentdelete, "Text"), "Well Comments text saved did not appear on UI");
            SeleniumActions.waitClick(PageObjects.WellAnalysisPage.btndeletecfm);
        }
        public void verifyshowhideoptions(By elem)
        {

            if (SeleniumActions.elementselected(elem).Equals(false))
            {
                SeleniumActions.waitClick(elem);
            }
        }

        public void AnalysisReport(ExtentTest test)
        {
            try
            {

                SeleniumActions.waitClick(PageObjects.WellAnalysisPage.btnanalysisreport);
                SeleniumActions.waitClick(PageObjects.WellAnalysisPage.btnanalysisreportdownload);



                Assert.IsNotNull(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.analysisreportgeneral, "Text").Replace("\r\n", ""));



            }
            catch (Exception e)
            {
                CommonHelper.PrintScreen("Error_RunAnalysis");
                Thread.Sleep(2000);
                Assert.Fail("Failure  in RunAnalysis" + e.ToString());
            }
        }
        public void CheckCalibration()
        {
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClickJS(PageObjects.WellAnalysisPage.btncalibrate);
            SeleniumActions.WaitForLoad();
            Thread.Sleep(2000);
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.txtpumpfilage, "value"), "100", "Calibration Percent is not 100");
            SeleniumActions.waitClick(PageObjects.WellAnalysisPage.btnget);
            SeleniumActions.WaitForLoad();
            //Assert.IsNotNull(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.lblcalibrationsurface, "Text"), "Legend for Calibration: Surface  Is not shown on UI");
            // Assert.IsNotNull(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.lblcalibrationdownhole, "Text"), "Legend for Calibration: Controller Downhole Controller  Is not shown on UI");

        }

        public void Verify_PumpFillPercentVSDToleranceShutDownLimitsFluidLoadLines(ExtentTest test)
        {
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClickJS(PageObjects.WellAnalysisPage.btnshowhide);
            SeleniumActions.WaitForLoad();
            if (SeleniumActions.elementselected(PageObjects.WellAnalysisPage.chkboxcardrangeslider).Equals(true))
            {
                SeleniumActions.waitClick(PageObjects.WellAnalysisPage.chkboxcardrangeslider);
            }
            SeleniumActions.waitClick(PageObjects.WellAnalysisPage.btnclearcards);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.WellAnalysisPage.lnkcardlib);
            SeleniumActions.WaitForLoad();
            if (SeleniumActions.Gettotalrecordsinlist("xpath", PageObjects.WellAnalysisPage.lnkcardlibrow).Count != 0)
            {
                SeleniumActions.waitClick(SeleniumActions.Gettotalrecordsinlist("xpath", PageObjects.WellAnalysisPage.lnkcardlibrow)[0]);
            }
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClickJS(PageObjects.WellAnalysisPage.btnshowhide);

            verifyshowhideoptions(PageObjects.WellAnalysisPage.chkboxpumpfillage);

            SeleniumActions.waitClickJS(PageObjects.WellAnalysisPage.btnrightcard);
            Thread.Sleep(3000);
            SeleniumActions.WaitForLoad();
            //Assert.AreEqual("0%", SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.pumpfillageper, "Text"), "Pump fillage is not displayed in UI");

            SeleniumActions.WaitForLoad();
            Thread.Sleep(2000);
            SeleniumActions.waitClickJS(PageObjects.WellAnalysisPage.btnshowhide);
            SeleniumActions.WaitForLoad();
            verifyshowhideoptions(PageObjects.WellAnalysisPage.chkboxvsdtolerance);
            SeleniumActions.WaitForLoad();
            SeleniumActions.takeScreenshot("PumpFill");
            test.Info("PumpFill Display Verify Manually for graph", MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "PumpFill" + ".png"))).Build());

            //Assert.AreEqual("8%", SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.vsdtoleranceper, "Text"), "VSD Tolerance is not displayed in UI");
            verifyshowhideoptions(PageObjects.WellAnalysisPage.chkboxshutdownlimits);
            SeleniumActions.WaitForLoad();
            /****
            Assert.AreEqual("High High 25000", SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.shutdownlimitshighhigh, "Text"), "High High is not displayed in UI");
            Assert.AreEqual("High 22000", SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.shutdownlimitshigh, "Text"), "High  is not displayed in UI");
            Assert.AreEqual("Low 7000", SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.shutdownlimitslow, "Text"), "Low  is not displayed in UI");
            Assert.AreEqual("Low Low 6000", SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.shutdownlimitslowlow, "Text"), "Low Low is not displayed in UI");
            */
            SeleniumActions.takeScreenshot("VSDTolerance");
            test.Info("VSDTolerance Display Verify Manually for graph", MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "VSDTolerance" + ".png"))).Build());
            SeleniumActions.takeScreenshot("ShutdownLimits");
            test.Info("ShutdownLimits Display Verify Manually for graph", MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "ShutdownLimits" + ".png"))).Build());
            CommonHelper.TraceLine("[*****check point ******]: All 4 Shutdown Limits Lines Text appeared on UI");
            SeleniumActions.waitClickJS(PageObjects.WellAnalysisPage.btnleftCard);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClickJS(PageObjects.WellAnalysisPage.btnshowhide);
            SeleniumActions.WaitForLoad();
            verifyshowhideoptions(PageObjects.WellAnalysisPage.chkboxfluidloadlines);
            SeleniumActions.WaitForLoad();
            /***
            Assert.AreEqual("Fo Up 13892", SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.foup, "Text"), "Fo Up is not displayed in UI");
            Assert.AreEqual("Fo Max 560", SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.fomax, "Text"), "Fo Max is not displayed in UI");
            Assert.AreEqual("Fo Down 5497", SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.fodown, "Text"), "Fo Down  is not displayed in UI");
            */

            SeleniumActions.takeScreenshot("FluidLoadLines");
            test.Info("FluidLoadLines Display Verify Manually for graph", MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "FluidLoadLines" + ".png"))).Build());
            verifyshowhideoptions(PageObjects.WellAnalysisPage.chkboxgridlines);
            verifyshowhideoptions(PageObjects.WellAnalysisPage.chkboxpoints);
            verifyshowhideoptions(PageObjects.WellAnalysisPage.chkboxcursorposition);
            verifycommentsadded();
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClickJS(PageObjects.WellAnalysisPage.btnshowhide);
            verifyshowhideoptions(PageObjects.WellAnalysisPage.chkboxadjusttoolbar);

            for (int i = 0; i < 4; i++)
            {
                SeleniumActions.waitClick(PageObjects.WellAnalysisPage.btncardrotateplus);


            }
            SeleniumActions.WaitForLoad();
            Assert.AreEqual("4", SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.txtrotatecard, "value"), "***On Plus Rotation Counter is incremented; check snapshot to see snapshot to see graph got changed");
            for (int i = 0; i < 4; i++)
            {
                SeleniumActions.waitClick(PageObjects.WellAnalysisPage.btncardrotateminus);


            }
            Assert.AreEqual("0", SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.txtrotatecard, "value"), "***On Plus Rotation Counter is decreased; check snapshot to see snapshot to see graph got changed");




        }
        public void Clickwelltest()
        {
            SeleniumActions.waitClickJS(PageObjects.WellAnalysisPage.tabwelltest);

        }
        public void VerifyGLCurves(ExtentTest test)
        {
            try
            {
                SeleniumActions.waitClickJS(PageObjects.WellAnalysisPage.drpgradientcurves);
                SeleniumActions.waitClickJS(PageObjects.WellAnalysisPage.gradientcurves);
                SeleniumActions.WaitForLoad();
                SeleniumActions.takeScreenshot("GLWellAnalysis_Gradient_MD");
                test.Pass("Gradient Curves Measured Depth", MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "GLWellAnalysis_Gradient_MD" + ".png"))).Build());
                checkGradientcurves("GL");
                SeleniumActions.waitClick(PageObjects.WellAnalysisPage.gradientmeasureddepthtoggle);
                SeleniumActions.WaitForLoad();
                SeleniumActions.takeScreenshot("GLWellAnalysis_Gradient_TD");
                test.Pass("Gradient Curves True Vertical Depth", MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "GLWellAnalysis_Gradient_TD" + ".png"))).Build());
                checkGradientcurves("GL");
                SeleniumActions.waitClickJS(PageObjects.WellAnalysisPage.drpgradientcurves);
                SeleniumActions.waitClickJS(PageObjects.WellAnalysisPage.performancecurves);
                Thread.Sleep(20000);
                SeleniumActions.takeScreenshot("GLWellAnalysis_Performance_LP");
                test.Pass("Performance Curves with Liquid Production", MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "GLWellAnalysis_Performance_LP" + ".png"))).Build());
                SeleniumActions.WaitForLoad();
                checkPerformancecurves(false, "GL");
                SeleniumActions.waitClick(PageObjects.WellAnalysisPage.prfcurvliqprodntoggle);
                SeleniumActions.takeScreenshot("GLWellAnalysis_Performance_OP");
                test.Pass("Performance Curves with Oil Production", MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "GLWellAnalysis_Performance_OP" + ".png"))).Build());
                SeleniumActions.WaitForLoad();
                Thread.Sleep(5000);
                checkPerformancecurves(true, "GL");
                SeleniumActions.waitClickJS(PageObjects.WellAnalysisPage.drpgradientcurves);
                SeleniumActions.waitClickJS(PageObjects.WellAnalysisPage.InflowOutflowcurves);
                SeleniumActions.WaitForLoad();
                SeleniumActions.takeScreenshot("GLWellAnalysis_Inflow_Outflow_LP");
                test.Pass("Inflow/Outflow Curves Liquid Production", MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "GLWellAnalysis_Inflow_Outflow_LP" + ".png"))).Build());
                Thread.Sleep(5000);
                checkIOcurves(false, "GL");
                SeleniumActions.waitClick(PageObjects.WellAnalysisPage.prfcurvliqprodntoggle);
                SeleniumActions.WaitForLoad();
                SeleniumActions.takeScreenshot("GLWellAnalysis_Inflow_Outflow_OP");
                test.Pass("Inflow/Outflow Curves Oil Production", MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "GLWellAnalysis_Inflow_Outflow_OP" + ".png"))).Build());
                Thread.Sleep(5000);
                checkIOcurves(true, "GL");
                SeleniumActions.waitClickJS(PageObjects.WellAnalysisPage.drpgradientcurves);
                SeleniumActions.waitClickJS(PageObjects.WellAnalysisPage.WellboreProfile);
                Thread.Sleep(7000);
                SeleniumActions.takeScreenshot("GLWellAnalysis_wellore_MD");
                test.Pass("Wellbore Profile Measured Depth", MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "GLWellAnalysis_wellore_MD" + ".png"))).Build());
                SeleniumActions.WaitForLoad();
                checkWellboreprofile();
                SeleniumActions.waitClick(PageObjects.WellAnalysisPage.gradientmeasureddepthtoggle);
                Thread.Sleep(7000);
                SeleniumActions.takeScreenshot("GLWellAnalysis_wellore_VD");
                test.Pass("Wellbore Profile True Vertical Depth", MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "GLWellAnalysis_wellore_VD" + ".png"))).Build());
                SeleniumActions.WaitForLoad();
                checkWellboreprofile();
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("Error_GLWell_Curves");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine(ExtentFactory.screenshotdir, "Error_GLWell_curves.png"))).Build());
                Assert.Fail(e.ToString());
            }
        }
        public void VerifyESPCurves(ExtentTest test)
        {
            try
            {
                SeleniumActions.waitClickJS(PageObjects.WellAnalysisPage.drpgradientcurves);
                SeleniumActions.waitClickJS(PageObjects.WellAnalysisPage.gradientcurves);
                SeleniumActions.WaitForLoad();
                checkGradientcurves("ESP");
                SeleniumActions.waitClick(PageObjects.WellAnalysisPage.gradientmeasureddepthtoggle);
                SeleniumActions.WaitForLoad();
                checkGradientcurves("ESP");
                SeleniumActions.waitClickJS(PageObjects.WellAnalysisPage.drpgradientcurves);
                SeleniumActions.waitClickJS(PageObjects.WellAnalysisPage.performancecurves);
                Thread.Sleep(20000);
                SeleniumActions.WaitForLoad();
                checkPerformancecurves(false, "ESP");
                SeleniumActions.waitClick(PageObjects.WellAnalysisPage.prfcurvliqprodntoggle);
                SeleniumActions.WaitForLoad();
                Thread.Sleep(5000);
                checkPerformancecurves(true, "ESP");
                SeleniumActions.waitClickJS(PageObjects.WellAnalysisPage.drpgradientcurves);
                SeleniumActions.waitClickJS(PageObjects.WellAnalysisPage.InflowOutflowcurves);
                SeleniumActions.WaitForLoad();
                Thread.Sleep(5000);
                checkIOcurves(false, "ESP");
                SeleniumActions.waitClick(PageObjects.WellAnalysisPage.prfcurvliqprodntoggle);
                SeleniumActions.WaitForLoad();
                Thread.Sleep(5000);
                checkIOcurves(true, "ESP");
                SeleniumActions.waitClickJS(PageObjects.WellAnalysisPage.drpgradientcurves);
                SeleniumActions.waitClickJS(PageObjects.WellAnalysisPage.WellboreProfile);
                Thread.Sleep(7000);
                SeleniumActions.WaitForLoad();
                checkWellboreprofile();
                SeleniumActions.waitClick(PageObjects.WellAnalysisPage.gradientmeasureddepthtoggle);
                Thread.Sleep(7000);
                SeleniumActions.WaitForLoad();
                checkWellboreprofile();
                SeleniumActions.waitClickJS(PageObjects.WellAnalysisPage.drpgradientcurves);
                SeleniumActions.waitClickJS(PageObjects.WellAnalysisPage.Gassinesscurves);
                Thread.Sleep(2000);
                checkGassinessCurves();
                Thread.Sleep(4000);
                SeleniumActions.waitClickJS(PageObjects.WellAnalysisPage.drpgradientcurves);
                SeleniumActions.waitClickJS(PageObjects.WellAnalysisPage.Pumpcurves);
                Thread.Sleep(4000);
                checkPumpCurves();
            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("Error_GLWell_Curves");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Error_GLWell_curves.png"))).Build());
                Assert.Fail(e.ToString());
            }
        }
        public void checkGradientcurves(string welltype)
        {
            if (welltype.Equals("GL"))
            {
                Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.gradientdepth));
                Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.gradienttemperature));
                Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.gradientpressure));
                Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.gradienttubtemp));
                Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.gradienttubpressure));
                Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.gradientcaspressure));
                Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.gradientmidperfdepth));
                Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.gradientcasingpressuretoopen));
                Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.gradienttubpressuretoopen));
            }
            else if (welltype.Equals("ESP"))
            {
                Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.gradientdepth));
                Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.gradienttemperature));
                Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.gradientpressure));
                Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.gradienttubtemp));
                Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.gradienttubpressure));
                Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.gradientmidperfdepth));

            }
        }

        public void checkPerformancecurves(bool toggle, string welltype)
        {
            if (welltype.Equals("GL"))
            {
                Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.prfcurvqgi));
                Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.prfcurvval1));
                Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.prfcurvval2));
                Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.prfcurvval3));
                Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.prfcurvwelltestpoint));
                if (toggle == false)
                {
                    Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.prfcurvqlstb));

                }
                else
                {
                    Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.prfcurvqostb));
                }
            }
            else if (welltype.Equals("ESP"))
            {
                Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.prfpressure));
                Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.prfof));
                Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.prfcurvwelltestpoint));
                if (toggle == false)
                {
                    Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.prfcurvqlstb));

                }
                else
                {
                    Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.prfcurvqostb));
                }
            }
            else
            {
                Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.prfcurvqostb));
            }
        }
        public void checkIOcurves(bool toggle, string welltype)
        {
            if (welltype.Equals("GL"))
            {
                Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.iocrvBHP));
                Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.iofcurvinflow));
                Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.iofcurvoutflow));
                Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.iocurvOpeartingPoint));
                Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.iocurvbubblepoint));
                Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.iocurvqtech));
                if (toggle == false)
                {
                    Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.iocrvQL));

                }
                else
                {
                    Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.iocrvQo));
                }
            }
            else if (welltype.Equals("ESP"))
            {
                Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.iocrvPIP));
                Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.iofcurvinflow));
                Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.iofcurvoutflow));
                Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.iocurvOpeartingPoint));
                Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.iocurvbubblepoint));
                Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.iocurvqtech));
                if (toggle == false)
                {
                    Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.iocrvQL));

                }
                else
                {
                    Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.iocrvQo));
                }
            }



        }

        public void checkWellboreprofile()
        {

            Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.wpdepthft));
            Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.wpvelocityfts));
            Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.wpcriticunvelocity));
            Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.wpgasinsitvelocity));
            Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.wperosionalvelocity));


        }
        public void checkGassinessCurves()
        {

            Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.Gcrvpip));
            Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.Gcrvlgc));
            Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.Gcrvugc));
            Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.Gcrvvlr));

        }
        public void checkPumpCurves()
        {

            Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.wellcrv));
            Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.Head));
            Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.Power));
            Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.Efficiency));
            Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.Minimumrate));
            Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.Maximumrate));
            Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.BestEfficiencyrate));
            Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.Headft));
            Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.Pumphorespower));
            Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.PumpEfficiency));
            Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.chkboxpower));
            Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.chkboxEfficiency));
            Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.chkboxHead));
            Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.chkboxWell));
            Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.chkboxHeadsensitivity));
            Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.chkboxPowersensitivity));
            Assert.IsTrue(SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellAnalysisPage.chkboxEfficiencysensitivity));
        }
        public void verifywellanalysisfields(ExtentTest test, String welltype)
        {
            SeleniumActions.WaitForLoad();
            Thread.Sleep(5000);
            if (welltype.Equals("GL"))
            {
                Helper.CommonHelper.ReturnDailyAverageData(WellTypeId.GLift);
            }
            else
            {
                Helper.CommonHelper.ReturnDailyAverageData(WellTypeId.ESP);
            }
            SeleniumActions.refreshBrowser();
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.WellAnalysisPage.toggleWelltest);
            Thread.Sleep(2000);
            Assert.IsNotNull(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.drpdowntest, "Text"));
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.qualitycode, "value").ToLower().ToString(), SeleniumAutomation.TestData.WellTestData.Qualitycode.ToLower().ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.lfactor, "value"), SeleniumAutomation.TestData.WellTestData.LFactor.ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.pressure, "value"), SeleniumAutomation.TestData.WellTestData.THP.ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.temperature, "value"), SeleniumAutomation.TestData.WellTestData.THT.ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.casingheadpressure, "value").Replace(@",", ""), SeleniumAutomation.TestData.WellTestData.CHP.ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.gasinjection, "value").Replace(@",", ""), SeleniumAutomation.TestData.WellTestData.GasInjectionRate.ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.oilrate, "value"), SeleniumAutomation.TestData.WellTestData.OilRate.ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.waterrate, "value"), SeleniumAutomation.TestData.WellTestData.WaterRate.ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.gasrate, "value").Replace(@",", ""), SeleniumAutomation.TestData.WellTestData.GasRate.ToString());
            //Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.formationgas, "value"), SeleniumAutomation.TestData.WellTestData.FormationGasRate.ToString());
            //Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.formationgor, "value"), SeleniumAutomation.TestData.WellTestData.FormationGOR.ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.watercut, "Text"), SeleniumAutomation.TestData.WellTestData.WaterCut.ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.productivityindex, "value"), SeleniumAutomation.TestData.WellTestData.ProductivityIndex.ToString());
            Thread.Sleep(2000);
            SeleniumActions.waitClick(PageObjects.WellAnalysisPage.toggleWelltest);
            Thread.Sleep(2000);
            Assert.IsNotNull(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.drpdowntest, "Text"));
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.qualitycode, "value"), SeleniumAutomation.TestData.WellTestData.dailyavgQualitycode);
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.dailyavjinjectionmethod, "Text"), SeleniumAutomation.TestData.WellTestData.dailyavgInjectionMethod.ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.dailyavgdepth, "Text"), SeleniumAutomation.TestData.WellTestData.dailyavgDepth.ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.dailyavgmultiphaseflowcorrelation, "Text"), SeleniumAutomation.TestData.WellTestData.dailyavgMultiphaseFlowcorrelation.ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.lfactor, "value"), SeleniumAutomation.TestData.WellTestData.LFactor.ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.temperature, "value"), SeleniumAutomation.TestData.WellTestData.dailyavgwellheadtemperature.ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.pressure, "value").Replace(@",", ""), SeleniumAutomation.TestData.WellTestData.dailyavgwellheadpressure.ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.gasinjection, "value").Replace(@",", ""), SeleniumAutomation.TestData.WellTestData.dailyavggasinjection.ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.dailyavgdownholegauge, "Text"), SeleniumAutomation.TestData.WellTestData.dailyavgdownholegauge.ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.oilrate, "value"), SeleniumAutomation.TestData.WellTestData.dailyavgoilrate.ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.waterrate, "value"), SeleniumAutomation.TestData.WellTestData.dailyavgwaterrate.ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.gasrate, "value"), SeleniumAutomation.TestData.WellTestData.dailyavggasrate.ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.liquidrate, "Text"), SeleniumAutomation.TestData.WellTestData.dailyavgliquidrate.ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.formationgor, "Text"), SeleniumAutomation.TestData.WellTestData.dailyavgFormationgor.ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.watercut, "Text"), SeleniumAutomation.TestData.WellTestData.WaterCut.ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.dailyavgsbhp, "value").Replace(@",", ""), SeleniumAutomation.TestData.WellTestData.ReservoirPressure.ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.dailyavgpi, "value"), SeleniumAutomation.TestData.WellTestData.ProductivityIndex.ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.dailyavgstartnode, "Text"), SeleniumAutomation.TestData.WellTestData.dailyavgstartnode.ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.dailyavgsolutionnode, "Text"), SeleniumAutomation.TestData.WellTestData.dailyavgsolutionnode.ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.dailyavgFBHP, "Text"), SeleniumAutomation.TestData.WellTestData.dailyavgFBHP.ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.dailyavgsolutionnodepressure, "Text"), SeleniumAutomation.TestData.WellTestData.dailyavgsolutionnodepr.ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.dailyavggasinjdepth, "value"), SeleniumAutomation.TestData.WellTestData.dailyavggasinjectiondepth.ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.dailyavgliquidrateipr, "Text"), SeleniumAutomation.TestData.WellTestData.dailyavgliquidrateipr.ToString());
            SeleniumActions.WaitForLoad();
            Thread.Sleep(5000);
            Helper.CommonHelper.ReturnDailyAverageData(WellTypeId.GLift);
            SeleniumActions.refreshBrowser();
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.WellAnalysisPage.toggleWelltest);
            Thread.Sleep(2000);
            Assert.IsNotNull(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.drpdowntest, "Text"));
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.qualitycode, "value").ToLower().ToString(), SeleniumAutomation.TestData.WellTestData.Qualitycode.ToLower().ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.lfactor, "value"), SeleniumAutomation.TestData.WellTestData.LFactor.ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.pressure, "value"), SeleniumAutomation.TestData.WellTestData.THP.ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.temperature, "value"), SeleniumAutomation.TestData.WellTestData.THT.ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.casingheadpressure, "value").Replace(@",", ""), SeleniumAutomation.TestData.WellTestData.CHP.ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.gasinjection, "value").Replace(@",", ""), SeleniumAutomation.TestData.WellTestData.GasInjectionRate.ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.oilrate, "value"), SeleniumAutomation.TestData.WellTestData.OilRate.ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.waterrate, "value"), SeleniumAutomation.TestData.WellTestData.WaterRate.ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.gasrate, "value").Replace(@",", ""), SeleniumAutomation.TestData.WellTestData.GasRate.ToString());
            //Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.formationgas, "value"), SeleniumAutomation.TestData.WellTestData.FormationGasRate.ToString());
            //Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.formationgor, "value"), SeleniumAutomation.TestData.WellTestData.FormationGOR.ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.watercut, "Text"), SeleniumAutomation.TestData.WellTestData.WaterCut.ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.productivityindex, "value"), SeleniumAutomation.TestData.WellTestData.ProductivityIndex.ToString());
            Thread.Sleep(2000);
            SeleniumActions.waitClick(PageObjects.WellAnalysisPage.toggleWelltest);
            Thread.Sleep(2000);
            Assert.IsNotNull(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.drpdowntest, "Text"));
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.qualitycode, "value"), SeleniumAutomation.TestData.WellTestData.dailyavgQualitycode);
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.dailyavjinjectionmethod, "Text"), SeleniumAutomation.TestData.WellTestData.dailyavgInjectionMethod.ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.dailyavgdepth, "Text"), SeleniumAutomation.TestData.WellTestData.dailyavgDepth.ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.dailyavgmultiphaseflowcorrelation, "Text"), SeleniumAutomation.TestData.WellTestData.dailyavgMultiphaseFlowcorrelation.ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.lfactor, "value"), SeleniumAutomation.TestData.WellTestData.LFactor.ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.temperature, "value"), SeleniumAutomation.TestData.WellTestData.dailyavgwellheadtemperature.ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.pressure, "value").Replace(@",", ""), SeleniumAutomation.TestData.WellTestData.dailyavgwellheadpressure.ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.gasinjection, "value").Replace(@",", ""), SeleniumAutomation.TestData.WellTestData.dailyavggasinjection.ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.dailyavgdownholegauge, "Text"), SeleniumAutomation.TestData.WellTestData.dailyavgdownholegauge.ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.oilrate, "value"), SeleniumAutomation.TestData.WellTestData.dailyavgoilrate.ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.waterrate, "value"), SeleniumAutomation.TestData.WellTestData.dailyavgwaterrate.ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.gasrate, "value"), SeleniumAutomation.TestData.WellTestData.dailyavggasrate.ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.liquidrate, "Text"), SeleniumAutomation.TestData.WellTestData.dailyavgliquidrate.ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.formationgor, "Text"), SeleniumAutomation.TestData.WellTestData.dailyavgFormationgor.ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.watercut, "Text"), SeleniumAutomation.TestData.WellTestData.WaterCut.ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.dailyavgsbhp, "value").Replace(@",", ""), SeleniumAutomation.TestData.WellTestData.ReservoirPressure.ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.dailyavgpi, "value"), SeleniumAutomation.TestData.WellTestData.ProductivityIndex.ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.dailyavgstartnode, "Text"), SeleniumAutomation.TestData.WellTestData.dailyavgstartnode.ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.dailyavgsolutionnode, "Text"), SeleniumAutomation.TestData.WellTestData.dailyavgsolutionnode.ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.dailyavgFBHP, "Text"), SeleniumAutomation.TestData.WellTestData.dailyavgFBHP.ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.dailyavgsolutionnodepressure, "Text"), SeleniumAutomation.TestData.WellTestData.dailyavgsolutionnodepr.ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.dailyavggasinjdepth, "value"), SeleniumAutomation.TestData.WellTestData.dailyavggasinjectiondepth.ToString());
            Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.dailyavgliquidrateipr, "Text"), SeleniumAutomation.TestData.WellTestData.dailyavgliquidrateipr.ToString());
            //test.Fail("Daily Average data and Curve", MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "GLWellAnalysis_Daily Average" + ".png"))).Build());
            if (welltype.Equals("GL"))
            {
                SeleniumActions.WaitForLoad();
                Thread.Sleep(5000);
                Helper.CommonHelper.ReturnDailyAverageData(WellTypeId.GLift);
                SeleniumActions.refreshBrowser();
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(PageObjects.WellAnalysisPage.toggleWelltest);
                Thread.Sleep(2000);
                Assert.IsNotNull(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.drpdowntest, "Text"));
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.qualitycode, "value").ToLower().ToString(), SeleniumAutomation.TestData.GLWellTestData.QualityCodeUS.ToLower().ToString());
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.lfactor, "value"), SeleniumAutomation.TestData.GLWellTestData.LFactor.ToString());
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.pressure, "value"), SeleniumAutomation.TestData.GLWellTestData.TubingHeadPressureUS.ToString());
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.temperature, "value"), SeleniumAutomation.TestData.GLWellTestData.TubingHeadTemperatureUS.ToString());
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.casingheadpressure, "value").Replace(@",", ""), SeleniumAutomation.TestData.GLWellTestData.CasingHeadPressureUS.ToString());
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.gasinjection, "value").Replace(@",", ""), SeleniumAutomation.TestData.GLWellTestData.GasInjectionRateUS.ToString());
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.oilrate, "value"), SeleniumAutomation.TestData.GLWellTestData.OilRateUS.ToString());
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.waterrate, "value"), SeleniumAutomation.TestData.GLWellTestData.WaterRateUS.ToString());
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.gasrate, "value").Replace(@",", ""), SeleniumAutomation.TestData.GLWellTestData.GasRateUS.ToString());
                //Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.formationgas, "value"), SeleniumAutomation.TestData.WellTestData.FormationGasRate.ToString());
                //Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.formationgor, "value"), SeleniumAutomation.TestData.WellTestData.FormationGOR.ToString());
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.watercut, "Text"), SeleniumAutomation.TestData.GLWellTestData.WaterCutUS.ToString());
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.productivityindex, "value"), SeleniumAutomation.TestData.GLWellTestData.ProductivityIndexUS.ToString());
                Thread.Sleep(2000);
                SeleniumActions.waitClick(PageObjects.WellAnalysisPage.toggleWelltest);
                Thread.Sleep(2000);
                Assert.IsNotNull(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.drpdowntest, "Text"));
                //Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.qualitycode, "value"), SeleniumAutomation.TestData.WellTestData.dailyavgQualitycode);
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.dailyavjinjectionmethod, "Text"), SeleniumAutomation.TestData.GLWellTestData.dailyavgInjectionMethod.ToString());
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.dailyavgdepth, "Text"), SeleniumAutomation.TestData.GLWellTestData.dailyavgDepth.ToString());
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.dailyavgmultiphaseflowcorrelation, "Text"), SeleniumAutomation.TestData.GLWellTestData.MultiphaseFlowCorrelationUS.ToString());
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.lfactor, "value"), SeleniumAutomation.TestData.GLWellTestData.LFactor.ToString());
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.temperature, "value"), SeleniumAutomation.TestData.GLWellTestData.TubingHeadTemperatureUS.ToString());
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.pressure, "value").Replace(@",", ""), SeleniumAutomation.TestData.GLWellTestData.TubingHeadPressureUS.ToString());
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.gasinjection, "value").Replace(@",", ""), SeleniumAutomation.TestData.GLWellTestData.dailyavgGasInjection.ToString());
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.dailyavgdownholegauge, "Text"), SeleniumAutomation.TestData.GLWellTestData.DPG.ToString());
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.oilrate, "value"), SeleniumAutomation.TestData.GLWellTestData.OilRateUS.ToString());
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.waterrate, "value"), SeleniumAutomation.TestData.GLWellTestData.WaterRateUS.ToString());
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.gasrate, "value"), SeleniumAutomation.TestData.GLWellTestData.FormationGasUS.ToString());
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.liquidrate, "Text"), SeleniumAutomation.TestData.GLWellTestData.dailyavgliquid.ToString());
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.formationgor, "Text"), SeleniumAutomation.TestData.GLWellTestData.GORUS.ToString());
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.watercut, "Text"), SeleniumAutomation.TestData.GLWellTestData.WaterCutUS.ToString());
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.dailyavgsbhp, "value").Replace(@",", ""), SeleniumAutomation.TestData.GLWellTestData.ReservoirPressureUS.ToString());
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.dailyavgpi, "value"), SeleniumAutomation.TestData.GLWellTestData.ProductivityIndexUS.ToString());
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.dailyavgstartnode, "Text"), SeleniumAutomation.TestData.GLWellTestData.StartNodeUS.ToString());
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.dailyavgsolutionnode, "Text"), SeleniumAutomation.TestData.GLWellTestData.SolutionNodeUS.ToString());
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.dailyavgFBHP, "Text"), SeleniumAutomation.TestData.GLWellTestData.dailyavgFBHP.ToString());
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.dailyavgsolutionnodepressure, "Text"), SeleniumAutomation.TestData.GLWellTestData.SolutionNodePressureUS.ToString());
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.dailyavggasinjdepth, "value"), SeleniumAutomation.TestData.GLWellTestData.dailyavggasinjectiondepth.ToString());
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.dailyavgliquidrateipr, "Text"), SeleniumAutomation.TestData.GLWellTestData.LiquidRatefromIPRUS.ToString());
            }
            else if (welltype.Equals("ESP"))
            {
                SeleniumActions.WaitForLoad();
                Thread.Sleep(5000);
                Helper.CommonHelper.ReturnDailyAverageData(WellTypeId.ESP);
                SeleniumActions.refreshBrowser();
                SeleniumActions.WaitForLoad();
                SeleniumActions.waitClick(PageObjects.WellAnalysisPage.toggleWelltest);
                Thread.Sleep(2000);
                Assert.IsNotNull(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.drpdowntest, "Text"));
                Assert.IsNotNull(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.drpdowntest, "Text"));
                // Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.qualitycode, "value").ToLower().ToString(), TestData.ESPWellTestData.QualityCodeUS.ToLower().ToString());
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.lfactor, "value"), TestData.ESPWellTestData.LFactor.ToString());
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.pressure, "value"), TestData.ESPWellTestData.TubingHeadPressureUS.ToString());
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.temperature, "value"), TestData.ESPWellTestData.TubingHeadTemperatureUS.ToString());
                //Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.casingheadpressure, "value").Replace(@",", ""), TestData.ESPWellTestData.CasingHeadPressureUS.ToString());
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.oilrate, "value").Replace(@",", ""), TestData.ESPWellTestData.OilRateUS.ToString());
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.waterrate, "value").Replace(@",", ""), TestData.ESPWellTestData.WaterRateUS.ToString());
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.gasrate, "value").Replace(@",", ""), TestData.ESPWellTestData.GasRateUS.ToString());
                //Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.formationgas, "value"), SeleniumAutomation.TestData.WellTestData.FormationGasRate.ToString());
                //Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.formationgor, "value"), SeleniumAutomation.TestData.WellTestData.FormationGOR.ToString());
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.watercut, "Text"), TestData.ESPWellTestData.WaterCutUS.ToString());
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.productivityindex, "value"), TestData.ESPWellTestData.ProductivityIndexUS.ToString());
                Thread.Sleep(2000);
                SeleniumActions.waitClick(PageObjects.WellAnalysisPage.toggleWelltest);
                Thread.Sleep(2000);
                Assert.IsNotNull(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.drpdowntest, "Text"));
                //Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.qualitycode, "Text"), TestData.ESPWellTestData.QualityCodeUS.ToLower().ToString());
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.dailyavgmultiphaseflowcorrelation, "Text"), TestData.ESPWellTestData.MultiphasecorrelationUS.ToString());
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.lfactor, "value"), TestData.ESPWellTestData.LFactor.ToString());
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.dailyavgheadtuningfactor, "value"), TestData.ESPWellTestData.HeadTuningFactorUS);
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.dailyavgpip, "value").Replace(@",", ""), TestData.ESPWellTestData.PumpIntakePressureUS);
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.dailyavgpdp, "value").Replace(@",", ""), TestData.ESPWellTestData.PumpDischargePressureUS);
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.pressure, "value").Replace(@",", ""), TestData.ESPWellTestData.TubingHeadPressureUS.ToString());
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.temperature, "value"), TestData.ESPWellTestData.TubingHeadTemperatureUS.ToString().ToString());
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.oilrate, "value").Replace(@",", ""), TestData.ESPWellTestData.OilRateUS.ToString());
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.waterrate, "value").Replace(@",", ""), TestData.ESPWellTestData.WaterRateUS.ToString());
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.gasrate, "value").Replace(@",", ""), TestData.ESPWellTestData.GasRateUS.ToString());
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.liquidrate, "Text"), TestData.ESPWellTestData.LiquidRateUS.ToString());
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.formationgor, "Text"), TestData.ESPWellTestData.GORUS);
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.formationgas, "Text"), TestData.ESPWellTestData.FormationGasUS);
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.watercut, "Text"), TestData.ESPWellTestData.WaterCutUS);
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.dailyavgsbhp, "value").Replace(@",", ""), TestData.ESPWellTestData.ReservoirPressureUS);
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.dailyavgpi, "value"), TestData.ESPWellTestData.ProductivityIndexUS);
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.dailyavgstartnode, "Text"), TestData.ESPWellTestData.StartNodeUS);
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.dailyavgsolutionnode, "Text"), TestData.ESPWellTestData.SolutionNodeUS);
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.dailyavgFBHP, "Text"), TestData.ESPWellTestData.FlowingBottomHolePressureUS);
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.dailyavgsolutionnodepressure, "Text"), TestData.ESPWellTestData.SolutionNodePressureUS);
                Assert.AreEqual(SeleniumActions.get_Attribute(PageObjects.WellAnalysisPage.dailyavggasinjdepth, "Text"), TestData.ESPWellTestData.LiquidRatefromIPRUS);
            }
        }
    }
}



