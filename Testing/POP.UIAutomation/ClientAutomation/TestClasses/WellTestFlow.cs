
using ArtOfTest.WebAii.Controls.HtmlControls;
using ClientAutomation.Helper;
using ClientAutomation.PageObjects;
using ClientAutomation.TelerikCoreUtils;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WinControls;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Weatherford.POP.APIClient;
using Weatherford.POP.DTOs;
using Weatherford.POP.Enums;
using Weatherford.POP.Interfaces;


namespace ClientAutomation.TestClasses
{
    public class WellTestFlow : APIUITestBase
    {

        public string appURL = ConfigurationManager.AppSettings["BrowserURL"];
        public string wellname = ConfigurationManager.AppSettings["Wellname"];
        public string testdatapath = ConfigurationManager.AppSettings["ModelFilesLocation"];
        public static string FilesLocation = ConfigurationManager.AppSettings.Get("FilesLocation");
        public static string IsRunningInATS = ConfigurationManager.AppSettings.Get("IsRunningInATS");


        public void WellTestWorkflow(WellConfigDTO WellConfig)
        {

            try
            {
                Thread.Sleep(10000);
                switch (WellConfig.Well.WellType)
                {
                    case WellTypeId.RRL:

                        {
                            RRLWellTestWorkFlow();
                            break;
                        }
                    case WellTypeId.ESP:

                        {
                            ESPWellTestWorkFlow();
                            break;
                        }
                    case WellTypeId.GLift:

                        {
                            GLWellTestWorkFlow();
                            break;
                        }
                    case WellTypeId.NF:

                        {
                            NFWellTestWorkFlow();
                            break;
                        }

                    default:
                        {
                            Assert.Fail("Well Type not expected");
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                Assert.Fail("Error in WellTestFlow" + e.ToString());
            }
            finally
            {

                CommonHelper.TraceLine("****** WellTest Flow test was completed ***** for " + WellTypeId.GLift.ToString());
                TelerikObject.mgr.Dispose();
            }
        }

        private void RRLWellTestWorkFlow()
        {
            //Create Wel from API
            CommonHelper.CreateNewRRLWellwithFullData();
            try
            {
                if (IsRunningInATS == "false")
                {
                    TelerikObject.InitializeManager();
                    TelerikObject.Click(PageDashboard.optimizationtab);
                }
                toggleSurviellanceTracking();
                TelerikObject.Click(PageDashboard.welltesttab);

                if (IsRunningInATS == "false")
                {
                    TelerikObject.select_well();
                }

                PageWellTest.preWellTestAddcheckRRL();

                // Step 1 : Add New Well Test 
                CommonHelper.TraceLine("**************Step 1: Adding New Well Test *******************************");
                int bcount = VerifyWellTestRecordcount("Before Well Test Addtion");

                AddNewRRLWellTest("RRLWellTestData.xml");
                Thread.Sleep(5000);
                Toastercheck(PageWellTest.toasterControl, "Add Well Test", "Saved successfully.");
                int acount = VerifyWellTestRecordcount("After Well Test Addtion");
                Assert.AreEqual(bcount + 1, acount, "Well Count Did Not Increment");
                CommonHelper.TraceLine("**************Step2: Verify New Well Test Data Added from WellTest Grid *******************************");
                VerifyRRLWellTestGridData("RRLWellTestData.xml");
                CommonHelper.TraceLine("**************Step3: Verify New Well Test Data Added from WellTest Grid in Metic Unit System*******************************");
                CommonHelper.ChangeUnitSystemUserSetting("Metric");
                NavigateBackForthWellTest();
                Thread.Sleep(5000);
                VerifyRRLWellTestGridData("RRLWellTestData.xml", "Metric");
                CommonHelper.ChangeUnitSystemUserSetting("US");
                CommonHelper.TraceLine("**************Step4: QE and Cancel to Ensure there are no changes *******************************");
                NavigateBackForthWellTest();
                Thread.Sleep(5000);
                RRLQuickEditWellTestGrid("RRLWellTestData.xml", "RRLWellTestDataQE.xml");
                PageWellTest.QEUpdatetAddcheck();
                TelerikObject.Click(PageWellTest.btnCancelWellTest);
                VerifyRRLWellTestGridData("RRLWellTestData.xml");
                CommonHelper.TraceLine("**************Step5: QE and Update Ensure toensure changes are Saved *******************************");
                RRLQuickEditWellTestGrid("RRLWellTestData.xml", "RRLWellTestDataQE.xml");
                PageWellTest.QEUpdatetAddcheck();
                TelerikObject.Click(PageWellTest.btnUpdateWellTest);
                Toastercheck(PageWellTest.toasterControl, "Update Well Test", "Updated successfully.");
                CommonHelper.TraceLine("**************Step6: QE and Update Ensure toensure changes are Saved in Metric Unit System *******************************");
                CommonHelper.ChangeUnitSystemUserSetting("Metric");
                NavigateBackForthWellTest();
                Thread.Sleep(5000);
                VerifyRRLWellTestGridData("RRLWellTestDataQE.xml", "Metric");
                CommonHelper.ChangeUnitSystemUserSetting("US");
                NavigateBackForthWellTest();
                CommonHelper.TraceLine("**************Step7: Delete WellTest To make sure records are Deleted *******************************");
                int bdcount = VerifyWellTestRecordcount("Before Well Test Deletion");
                string testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\TestData", "RRLWellTestData.xml");
                DataTable dt = CommonHelper.BuildDataTableFromXml(testdatafile);
                string[] testdateonly = dt.Rows[0]["TestDate"].ToString().Split(new char[] { ';' });
                DateTime testdt = Convert.ToDateTime(testdateonly[0]);
                string modifiledttime = testdt.ToString("M/d/yy") + ", " + testdateonly[1];
                PageWellTest.DynamicValue = modifiledttime;

                HtmlControl PrtwelltestCheckBox = TelerikObject.GetPreviousSibling(PageWellTest.cell_testdata, 1);
                HtmlControl welltestCheckBox = TelerikObject.GetChildrenControl(PrtwelltestCheckBox, "0");
                Assert.IsNotNull(welltestCheckBox, "Check Box control was null");
                //   TelerikObject.CheckBox_Check(welltestCheckBox.BaseElement);
                TelerikObject.Click(welltestCheckBox.BaseElement);
                TelerikObject.Click(PageWellTest.btnDeleteWellTest);
                string welltestelconfirm = "Are you sure you want to delete the selected well test(s)? Deleting the data is not recoverable and removes the well test(s) from all historical data.";
                Assert.AreEqual(welltestelconfirm, PageWellTest.popup_Container.BaseElement.InnerText, "Well Test Delete Confirmation Message was not correct");
                TelerikObject.Click(PageWellTest.btnDeleteConfirmWellTest);
                Thread.Sleep(2000);
                Toastercheck(PageWellTest.toasterControl, "Delete Well Test", "Deleted successfully.");
                int adcount = VerifyWellTestRecordcount("After Well Test Deletion");
                Assert.AreEqual(bdcount - 1, adcount, "Well Count Not  Decremented After Deletion");
                //Go to WellConfiguration for Deletion
                TelerikObject.Click(PageDashboard.wellConfigurationtab);
                //Delete the Well Created from UI
                DeleteWell();
            }
            catch (Exception e)
            {
                CommonHelper.TraceLine("Exception was obtained from method [RRLWellTestWorkFlow]" + e.ToString());
                CommonHelper.PrintScreen("RRLWelltestErr");
                Assert.Fail("RRL WellTest WorkFlow Falied");
            }
            finally
            {
                //Change Unit Ssytems back to US English without fail
                CommonHelper.TraceLine("Changing unit system back to US to bring to Original state");
                CommonHelper.ChangeUnitSystemUserSetting("US");
            }


        }

        private void ESPWellTestWorkFlow()
        {
            CommonHelper.CreateESPWellwithFullData();
            try
            {
                if (IsRunningInATS == "false")
                {
                    TelerikObject.Click(PageDashboard.optimizationtab);
                }
                toggleSurviellanceTracking();
                TelerikObject.Click(PageDashboard.welltesttab);
                Thread.Sleep(5000);
                if (IsRunningInATS == "false")
                {
                    TelerikObject.select_well();
                    Thread.Sleep(6000);
                }
                PageWellTest.preWellTestAddcheckNonRRL();
                // Step 1 : Add New Well Test 
                CommonHelper.TraceLine("**************Step 1: Adding New Well Test *******************************");
                int bcount = VerifyWellTestRecordcount("Before Well Test Addtion");
                TelerikObject.Click(PageWellTest.btnCreateNewWellTest);
                AddNewESPWellTest("ESPWellTestData.xml");
                Thread.Sleep(5000);
                Toastercheck(PageWellTest.toasterControl, "Add Well Test", "Saved successfully.");
                Thread.Sleep(10000);
                int acount = VerifyWellTestRecordcount("After Well Test Addtion");
                Assert.AreEqual(bcount + 1, acount, "Well Count Did Not Increment");
                CommonHelper.TraceLine("**************Step2: Verify New Well Test Data Added from WellTest Grid *******************************");
                VerifyESPWellTestGridData("ESPWellTestData.xml");
                CommonHelper.TraceLine("**************Step3: Verify New Well Test Data Added from WellTest Grid in Metic Unit System*******************************");
                CommonHelper.ChangeUnitSystemUserSetting("Metric");
                NavigateBackForthWellTest();
                Thread.Sleep(4000);
                VerifyESPWellTestGridData("ESPWellTestData.xml", 0, "Metric");
                CommonHelper.ChangeUnitSystemUserSetting("US");
                CommonHelper.TraceLine("**************Step4: QE and Cancel to Ensure there are no changes *******************************");
                NavigateBackForthWellTest();
                ESPQuickEditWellTestGrid("ESPWellTestData.xml", "ESPWellTestDataQE.xml");
                PageWellTest.QEUpdatetAddcheck();
                TelerikObject.Click(PageWellTest.btnCancelWellTest);
                VerifyESPWellTestGridData("ESPWellTestData.xml");
                CommonHelper.TraceLine("**************Step5: QE and Update Ensure toensure changes are Saved *******************************");
                ESPQuickEditWellTestGrid("ESPWellTestData.xml", "ESPWellTestDataQE.xml");
                PageWellTest.QEUpdatetAddcheck();
                TelerikObject.Click(PageWellTest.btnUpdateWellTest);
                Toastercheck(PageWellTest.toasterControl, "Update Well Test", "Updated successfully.");
                // status should be needs tuning immeditaly after update no Calculation changes
                VerifyESPWellTestGridData("ESPWellTestDataQE.xml", 1);
                TuneWellTestESP();
                //Thread.Sleep(5000);
                //Post Tuning we should get tuning status
                VerifyESPWellTestGridData("ESPWellTestDataQE.xml");
                CommonHelper.TraceLine("**************Step6: QE and Update Ensure to ensure changes are Saved in Metric Unit System *******************************");
                CommonHelper.ChangeUnitSystemUserSetting("Metric");
                NavigateBackForthWellTest();
                VerifyESPWellTestGridData("ESPWellTestDataQE.xml", 0, "Metric");
                CommonHelper.ChangeUnitSystemUserSetting("US");
                NavigateBackForthWellTest();
                CommonHelper.TraceLine("**************Step7: Delete WellTest To make sure records are Deleted *******************************");
                int bdcount = VerifyWellTestRecordcount("Before Well Test Deletion");
                GetWellTestDateTime("ESPWellTestData.xml");

                HtmlControl PrtwelltestCheckBox = TelerikObject.GetPreviousSibling(PageWellTest.cell_testdata, 1);
                HtmlControl welltestCheckBox = TelerikObject.GetChildrenControl(PrtwelltestCheckBox, "0");
                Assert.IsNotNull(welltestCheckBox, "Check Box control was null");

                TelerikObject.Click(welltestCheckBox.BaseElement);
                TelerikObject.Click(PageWellTest.btnDeleteWellTest);
                string welltestelconfirm = "Are you sure you want to delete the selected well test(s)? Deleting the data is not recoverable and removes the well test(s) from all historical data.";
                Assert.AreEqual(welltestelconfirm, PageWellTest.popup_Container.BaseElement.InnerText, "Well Test Delete Confirmation Message was not correct");
                TelerikObject.Click(PageWellTest.btnDeleteConfirmWellTest);
                Thread.Sleep(2000);
                Toastercheck(PageWellTest.toasterControl, "Delete Well Test", "Deleted successfully.");
                int adcount = VerifyWellTestRecordcount("After Well Test Deletion");
                Assert.AreEqual(bdcount - 1, adcount, "Well Count Not  Decremented After Deletion");
                TelerikObject.Click(PageDashboard.wellConfigurationtab);
                toggleSurviellanceTracking();
            }
            catch (Exception e)
            {
                CommonHelper.TraceLine("**************** ESPWellTest Method Failed **********" + e.ToString());
                Assert.Fail("Expcetion encountered : " + e.ToString());
                CommonHelper.PrintScreen("ESPWelltestErr");
            }
            finally
            {
                CommonHelper.TraceLine("Changing unit system back to US to bring to Original state");
                CommonHelper.ChangeUnitSystemUserSetting("US");
            }


        }

        public void GLWellTestWorkFlow()
        {
            //****   Record Machine Details on whihc this test is running
            CommonHelper.GetMachineDetails();
            //****   Create The GL well using API calls and then Open UI for WellTest
            CommonHelper.CreateGLWellwithFullData();
            //****** now open UI to perform Tests on Created GL Well
            try
            {
                TelerikObject.InitializeManager();
                TelerikObject.Click(PageDashboard.optimizationtab);
                TelerikObject.Click(PageDashboard.welltesttab);
                TelerikObject.WaitForControlDisappear(new HtmlControl(PageDashboard.page_Loader));
                if (IsRunningInATS == "false")
                {
                    TelerikObject.select_well();
                    Thread.Sleep(6000);
                }
                PageWellTest.preWellTestAddcheckNonRRL();
                // Step 1 : Add New Well Test 
                CommonHelper.TraceLine("**************Step 1: Adding New Well Test *******************************");
                int bcount = VerifyWellTestRecordcount("Before Well Test Addtion");
                TelerikObject.Click(PageWellTest.btnCreateNewWellTest);
                AddNewGLWellTest("GLWellTestData.xml");
                Toastercheck(PageWellTest.toasterControl, "Add Well Test", "Saved successfully.");
                Thread.Sleep(10000);
                int acount = VerifyWellTestRecordcount("After Well Test Addtion");
                Assert.AreEqual(bcount + 1, acount, "Well Count Did Not Increment");
                CommonHelper.TraceLine("**************Step2: Verify New Well Test Data Added from WellTest Grid *******************************");
                VerifyGLWellTestGridData("GLWellTestData.xml");
                CommonHelper.TraceLine("**************Step3: Verify New Well Test Data Added from WellTest Grid in Metic Unit System*******************************");
                CommonHelper.ChangeUnitSystemUserSetting("Metric");
                NavigateBackForthWellTest();
                Thread.Sleep(4000);
                VerifyGLWellTestGridData("GLWellTestData.xml", 0, "Metric");
                CommonHelper.ChangeUnitSystemUserSetting("US");
                CommonHelper.TraceLine("**************Step4: QE and Cancel to Ensure there are no changes *******************************");
                NavigateBackForthWellTest();
                GLQuickEditWellTestGrid("GLWellTestData.xml", "GLWellTestDataQE.xml");
                PageWellTest.QEUpdatetAddcheck();
                TelerikObject.Click(PageWellTest.btnCancelWellTest);
                VerifyGLWellTestGridData("GLWellTestData.xml");
                CommonHelper.TraceLine("**************Step5: QE and Update Ensure toensure changes are Saved *******************************");
                GLQuickEditWellTestGrid("GLWellTestData.xml", "GLWellTestDataQE.xml");
                PageWellTest.QEUpdatetAddcheck();
                TelerikObject.Click(PageWellTest.btnUpdateWellTest);
                Toastercheck(PageWellTest.toasterControl, "Update Well Test", "Updated successfully.");
                // status should be needs tuning immeditaly after update no Calculation changes
                VerifyGLWellTestGridData("GLWellTestDataQE.xml", 1);
                TuneWellTestGL();
                Thread.Sleep(9000);
                //Post Tuning we should get tuning status
                VerifyGLWellTestGridData("GLWellTestDataQE.xml");
                CommonHelper.TraceLine("**************Step6: QE and Update Ensure to ensure changes are Saved in Metric Unit System *******************************");
                CommonHelper.ChangeUnitSystemUserSetting("Metric");
                NavigateBackForthWellTest();
                VerifyGLWellTestGridData("GLWellTestDataQE.xml", 0, "Metric");
                CommonHelper.ChangeUnitSystemUserSetting("US");
                NavigateBackForthWellTest();
                CommonHelper.TraceLine("**************Step7: Delete WellTest To make sure records are Deleted *******************************");
                int bdcount = VerifyWellTestRecordcount("Before Well Test Deletion");
                GetWellTestDateTime("GLWellTestData.xml");

                HtmlControl PrtwelltestCheckBox = TelerikObject.GetPreviousSibling(PageWellTest.cell_testdata, 1);
                HtmlControl welltestCheckBox = TelerikObject.GetChildrenControl(PrtwelltestCheckBox, "0");
                Assert.IsNotNull(welltestCheckBox, "Check Box control was null");

                TelerikObject.Click(welltestCheckBox.BaseElement);
                TelerikObject.Click(PageWellTest.btnDeleteWellTest);
                string welltestelconfirm = "Are you sure you want to delete the selected well test(s)? Deleting the data is not recoverable and removes the well test(s) from all historical data.";
                Assert.AreEqual(welltestelconfirm, PageWellTest.popup_Container.BaseElement.InnerText, "Well Test Delete Confirmation Message was not correct");
                TelerikObject.Click(PageWellTest.btnDeleteConfirmWellTest);
                Thread.Sleep(2000);
                Toastercheck(PageWellTest.toasterControl, "Delete Well Test", "Deleted successfully.");
                int adcount = VerifyWellTestRecordcount("After Well Test Deletion");
                Assert.AreEqual(bdcount - 1, adcount, "Well Count Not  Decremented After Deletion");
            }
            catch (Exception e)
            {
                CommonHelper.PrintScreen("GLWelltestErr");
                CommonHelper.TraceLine("**************** GLWellTest Method Failed **********" + e.ToString());
                Assert.Fail("Expcetion encountered : " + e.ToString());

            }
            finally
            {
                CommonHelper.TraceLine("Changing unit system back to US to bring to Original state");
                //Delete the Well Created from UI
                CommonHelper.DeleteWell();
                CommonHelper.ChangeUnitSystemUserSetting("US");
                TelerikObject.mgr.Dispose();

            }
        }

        private void NFWellTestWorkFlow()
        {
            CommonHelper.CreateNFWellwithFullData();
            try
            {
                if (IsRunningInATS == "false")
                {
                    TelerikObject.InitializeManager();
                    TelerikObject.Click(PageDashboard.optimizationtab);
                }
                toggleSurviellanceTracking();
                TelerikObject.Click(PageDashboard.welltesttab);
                Thread.Sleep(5000);
                if (IsRunningInATS == "false")
                {
                    TelerikObject.select_well();
                    Thread.Sleep(6000);
                }
                PageWellTest.preWellTestAddcheckNonRRL();
                // Step 1 : Add New Well Test 
                CommonHelper.TraceLine("**************Step 1: Adding New Well Test *******************************");
                int bcount = VerifyWellTestRecordcount("Before Well Test Addtion");
                TelerikObject.Click(PageWellTest.btnCreateNewWellTest);
                AddNewNFWellTest("NFWellTestData.xml");
                Thread.Sleep(5000);
                Toastercheck(PageWellTest.toasterControl, "Add Well Test", "Saved successfully.");
                Thread.Sleep(10000);
                int acount = VerifyWellTestRecordcount("After Well Test Addtion");
                Assert.AreEqual(bcount + 1, acount, "Well Count Did Not Increment");
                CommonHelper.TraceLine("**************Step2: Verify New Well Test Data Added from WellTest Grid *******************************");
                VerifyNFWellTestGridData("NFWellTestData.xml");
                CommonHelper.TraceLine("**************Step3: Verify New Well Test Data Added from WellTest Grid in Metic Unit System*******************************");
                CommonHelper.ChangeUnitSystemUserSetting("Metric");
                NavigateBackForthWellTest();
                Thread.Sleep(4000);
                VerifyNFWellTestGridData("NFWellTestData.xml", 0, "Metric");
                CommonHelper.ChangeUnitSystemUserSetting("US");
                CommonHelper.TraceLine("**************Step4: QE and Cancel to Ensure there are no changes *******************************");
                NavigateBackForthWellTest();
                NFQuickEditWellTestGrid("NFWellTestData.xml", "NFWellTestDataQE.xml");
                PageWellTest.QEUpdatetAddcheck();
                TelerikObject.Click(PageWellTest.btnCancelWellTest);
                VerifyNFWellTestGridData("NFWellTestData.xml");
                CommonHelper.TraceLine("**************Step5: QE and Update Ensure toensure changes are Saved *******************************");
                NFQuickEditWellTestGrid("NFWellTestData.xml", "NFWellTestDataQE.xml");
                PageWellTest.QEUpdatetAddcheck();
                TelerikObject.Click(PageWellTest.btnUpdateWellTest);
                Toastercheck(PageWellTest.toasterControl, "Update Well Test", "Updated successfully.");
                //  status should be needs tuning immeditaly after update no Calculation changes
                VerifyNFWellTestGridData("NFWellTestDataQE.xml", 1);
                TuneWellTestNF();
                Thread.Sleep(5000);
                //Post Tuning we should get tuning status
                VerifyNFWellTestGridData("NFWellTestDataQE.xml");
                CommonHelper.TraceLine("**************Step6: QE and Update Ensure to ensure changes are Saved in Metric Unit System *******************************");
                CommonHelper.ChangeUnitSystemUserSetting("Metric");
                NavigateBackForthWellTest();
                VerifyNFWellTestGridData("NFWellTestDataQE.xml", 0, "Metric");
                CommonHelper.ChangeUnitSystemUserSetting("US");
                NavigateBackForthWellTest();
                CommonHelper.TraceLine("**************Step7: Delete WellTest To make sure records are Deleted *******************************");
                int bdcount = VerifyWellTestRecordcount("Before Well Test Deletion");
                GetWellTestDateTime("NFWellTestData.xml");

                HtmlControl PrtwelltestCheckBox = TelerikObject.GetPreviousSibling(PageWellTest.cell_testdata, 1);
                HtmlControl welltestCheckBox = TelerikObject.GetChildrenControl(PrtwelltestCheckBox, "0");
                Assert.IsNotNull(welltestCheckBox, "Check Box control was null");

                TelerikObject.Click(welltestCheckBox.BaseElement);
                TelerikObject.Click(PageWellTest.btnDeleteWellTest);
                string welltestelconfirm = "Are you sure you want to delete the selected well test(s)? Deleting the data is not recoverable and removes the well test(s) from all historical data.";
                Assert.AreEqual(welltestelconfirm, PageWellTest.popup_Container.BaseElement.InnerText, "Well Test Delete Confirmation Message was not correct");
                TelerikObject.Click(PageWellTest.btnDeleteConfirmWellTest);
                Thread.Sleep(2000);
                Toastercheck(PageWellTest.toasterControl, "Delete Well Test", "Deleted successfully.");
                int adcount = VerifyWellTestRecordcount("After Well Test Deletion");
                Assert.AreEqual(bdcount - 1, adcount, "Well Count Not  Decremented After Deletion");
                TelerikObject.Click(PageDashboard.wellConfigurationtab);
                toggleSurviellanceTracking();
            }
            catch (Exception e)
            {
                CommonHelper.TraceLine("**************** NFWellTest Method Failed **********" + e.ToString());
                Assert.Fail("Expcetion encountered : " + e.ToString());
                CommonHelper.PrintScreen("NFWelltestErr");
            }
            finally
            {
                CommonHelper.TraceLine("Changing unit system back to US to bring to Original state");
                CommonHelper.ChangeUnitSystemUserSetting("US");
            }
        }

        public void DeleteWell()
        {
            try
            {
                TelerikObject.Click(PageDashboard.configurationtab);
                TelerikObject.Click(PageDashboard.wellConfigurationtab);
                TelerikObject.WaitForControlDisappear(new HtmlControl(PageDashboard.page_Loader));
                TelerikObject.Click(PageWellConfig.btnDeleteWell);
                TelerikObject.WaitForControlDisappear(new HtmlControl(PageDashboard.page_Loader));
                TelerikObject.Click(PageWellConfig.btnConfirmDeleteWell);
                Thread.Sleep(2000);
                TelerikObject.Click(PageWellConfig.btnConfirmDeleteWell);
                TelerikObject.WaitForControlDisappear(new HtmlControl(PageDashboard.page_Loader));
                Thread.Sleep(2000);
                //   TelerikObject.Toastercheck(PageAnalysis.toasterControl, "Delete Well", "Well has been successfully deleted.");
                CommonHelper.TraceLine("RRL Well was deleted");
            }
            catch (Exception e)
            {
                CommonHelper.TraceLine("Error for well deletion" + e);
                CommonHelper.PrintScreen("ErrWellDeletion");
            }
        }
        public void AddNewRRLWellTest(string filename)
        {
            // Populate test data from XML file
            TelerikObject.Click(PageWellTest.btnCreateNewWellTest);
            string testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\TestData", filename);
            CommonHelper.TraceLine("Getting Data from XML file ... " + testdatafile);
            DataTable dt = CommonHelper.BuildDataTableFromXml(testdatafile);
            string testdatetime = dt.Rows[0]["TestDate"].ToString();
            string[] arrtestdate = testdatetime.Split(new char[] { ';' });
            string testdate = arrtestdate[0];
            // Populate variables 
            string strtoday = testdate.Replace("/", "");
            CommonHelper.TraceLine("Todays Date" + strtoday);
            TelerikObject.Sendkeys(PageWellTest.txtTestDate, strtoday, false, 1000);
            TelerikObject.Click(PageWellTest.dd_TestTime);
            string testtime = arrtestdate[1];
            TelerikObject.Select_KendoUI_Listitem(testtime);
            string tstduration = dt.Rows[0]["TestDurationHours"].ToString();
            string tstqualitycode = dt.Rows[0]["QualityCode"].ToString();
            string tstoil = dt.Rows[0]["OilRateSTBd"].ToString();
            string tstwater = dt.Rows[0]["WaterRateSTBd"].ToString();
            string tstgasrate = dt.Rows[0]["GasRateMscfd"].ToString();
            string tstoilapi = dt.Rows[0]["OilGravityAPI"].ToString();
            string tstwatergravity = dt.Rows[0]["WaterGravitySG"].ToString();
            string tstgasgravity = dt.Rows[0]["GasGravitySG"].ToString();
            string tsttubingpresure = dt.Rows[0]["AverageTubingPressurepsia"].ToString();
            string tsttubingtemperature = dt.Rows[0]["AverageTubingTemperatureF"].ToString();
            string tstcaingpressure = dt.Rows[0]["AverageCasingPressurepsia"].ToString();
            string tstfap = dt.Rows[0]["AverageFluidAbovePumpft"].ToString();
            string tstpmphrs = dt.Rows[0]["PumpingHoursHours"].ToString();

            //***********Input Data on Modal Dialog

            TelerikObject.Sendkeys(PageWellTest.txtTestduration, tstduration);
            TelerikObject.Sendkeys(PageWellTest.txtOiRate, tstoil);
            TelerikObject.Sendkeys(PageWellTest.txtWaterRate, tstwater);

            TelerikObject.Sendkeys(PageWellTest.txtGasRate, tstgasrate);
            TelerikObject.Sendkeys(PageWellTest.txt_OilGravity, tstoilapi);
            TelerikObject.Sendkeys(PageWellTest.txt_WaterGravity, tstwatergravity);
            TelerikObject.Sendkeys(PageWellTest.txt_GasGravity, tstgasgravity);
            TelerikObject.Sendkeys(PageWellTest.txt_TubingHeadPressure, tsttubingpresure);
            TelerikObject.Sendkeys(PageWellTest.txt_TubingHeadTemperature, tsttubingtemperature);
            TelerikObject.Sendkeys(PageWellTest.txt_CasinggHeadPressure, tstcaingpressure);
            TelerikObject.Sendkeys(PageWellTest.txt_AverageFAP, tstfap);
            TelerikObject.Sendkeys(PageWellTest.txt_PumpHours, tstpmphrs);

            // Issue FRI-1344 
            TelerikObject.Click(PageWellTest.dd_QualityCode);
            TelerikObject.Select_KendoUI_Listitem(tstqualitycode);
            VerifyUnitLabelsTextOnAddDialogRRL("RRLWellTestUSUnitLabels.xml");
            TelerikObject.Click(PageWellTest.btn_SaveWellTest);

        }
        private void AddNewESPWellTest(string filename)
        {
            // Populate test data from XML file
            string testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\TestData", filename);
            CommonHelper.TraceLine("Getting Data from XML file ... " + testdatafile);
            DataTable dt = CommonHelper.BuildDataTableFromXml(testdatafile);
            string testdatetime = dt.Rows[0]["TestDate"].ToString();
            string[] arrtestdate = testdatetime.Split(new char[] { ';' });
            string testdate = arrtestdate[0];
            // Populate variables 
            string strtoday = testdate.Replace("/", "");
            CommonHelper.TraceLine("Todays Date" + strtoday);
            TelerikObject.Sendkeys(PageWellTest.txtTestDate, strtoday, false, 1000);
            TelerikObject.Click(PageWellTest.dd_TestTime);
            string testtime = arrtestdate[1];
            TelerikObject.Select_KendoUI_Listitem(testtime);
            string tstqualitycode = dt.Rows[0]["QualityCode"].ToString();
            string tstoil = dt.Rows[0]["OilRateSTBd"].ToString();
            string tstwater = dt.Rows[0]["WaterRateSTBd"].ToString();
            string tstgasrate = dt.Rows[0]["GasRateMscfd"].ToString();
            string tsttubingpresure = dt.Rows[0]["TubingHeadPressurepsia"].ToString();
            string tsttubingtemperature = dt.Rows[0]["TubingHeadTemperatureF"].ToString();
            string tstesppip = dt.Rows[0]["PumpIntakePressure"].ToString();
            string tstesppdp = dt.Rows[0]["PumpDishcargePressure"].ToString();
            string tstfrequceny = dt.Rows[0]["Frequency"].ToString();
            string tstmotovolts = dt.Rows[0]["MotorVolts"].ToString();
            string tstmotoramps = dt.Rows[0]["MotorAmps"].ToString();
            string tstflowlinepressure = dt.Rows[0]["FlowLinePressure"].ToString();
            string tstseparatorpressure = dt.Rows[0]["SeparatorPressure"].ToString();
            string tstchokesize = dt.Rows[0]["ChokeSize"].ToString();
            //***********Input Data on Modal Dialog

            VerifyUnitLabelsTextOnAddDialogESP("ESPWellTestUSUnitLabels.xml");
            TelerikObject.Click(PageWellTest.dd_QualityCode);
            TelerikObject.Select_KendoUI_Listitem(tstqualitycode);
            TelerikObject.Sendkeys(PageWellTest.txtOiRate, tstoil);
            TelerikObject.Sendkeys(PageWellTest.txtWaterRate, tstwater);
            TelerikObject.Sendkeys(PageWellTest.txtGasRate, tstgasrate);
            TelerikObject.Sendkeys(PageWellTest.txt_TubingHeadPressure, tsttubingpresure);
            TelerikObject.Sendkeys(PageWellTest.txt_TubingHeadTemperature, tsttubingtemperature);
            TelerikObject.Sendkeys(PageWellTest.txt_RRLPIP, tstesppip);
            TelerikObject.Sendkeys(PageWellTest.txt_Frequency, tstfrequceny);
            TelerikObject.Sendkeys(PageWellTest.txt_FlowLinePressure, tstflowlinepressure);
            TelerikObject.Sendkeys(PageWellTest.txt_SepraratorPressure, tstseparatorpressure);
            TelerikObject.Sendkeys(PageWellTest.txt_ChokeSize, tstchokesize);


            TelerikObject.Click(PageWellTest.btn_SaveWellTest);

        }
        private void AddNewESPWellTest_Analysis(string filename)
        {
            // Populate test data from XML file
            string testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\TestData", filename);
            CommonHelper.TraceLine("Getting Data from XML file ... " + testdatafile);
            DataTable dt = CommonHelper.BuildDataTableFromXml(testdatafile);
            string testdatetime = dt.Rows[0]["TestDate"].ToString();
            string[] arrtestdate = testdatetime.Split(new char[] { ';' });
            string testdate = arrtestdate[0];
            // Populate variables 
            string strtoday = testdate.Replace("/", "");
            CommonHelper.TraceLine("Todays Date" + strtoday);
            TelerikObject.Sendkeys(PageWellTest.txtTestDate, strtoday, false, 1000);
            TelerikObject.Click(PageWellTest.dd_TestTime);
            string testtime = arrtestdate[1];
            TelerikObject.Select_KendoUI_Listitem(testtime);
            string tstqualitycode = dt.Rows[0]["QualityCode"].ToString();
            string tstoil = dt.Rows[0]["OilRateSTBd"].ToString();
            string tstwater = dt.Rows[0]["WaterRateSTBd"].ToString();
            string tstgasrate = dt.Rows[0]["GasRateMscfd"].ToString();
            string tsttubingpresure = dt.Rows[0]["TubingHeadPressurepsia"].ToString();
            string tsttubingtemperature = dt.Rows[0]["TubingHeadTemperatureF"].ToString();
            string casinghead = dt.Rows[0]["CasingHeadPressurePsia"].ToString();
            string tstesppip = dt.Rows[0]["PumpIntakePressure"].ToString();
            string tstesppdp = dt.Rows[0]["PumpDishcargePressure"].ToString();
            string tstfrequceny = dt.Rows[0]["Frequency"].ToString();
            string tstmotovolts = dt.Rows[0]["MotorVolts"].ToString();
            string tstmotoramps = dt.Rows[0]["MotorAmps"].ToString();
            string tstflowlinepressure = dt.Rows[0]["FlowLinePressure"].ToString();
            string tstseparatorpressure = dt.Rows[0]["SeparatorPressure"].ToString();
            string tstchokesize = dt.Rows[0]["ChokeSize"].ToString();
            //***********Input Data on Modal Dialog

            //  VerifyUnitLabelsTextOnAddDialogESP("ESPWellTestUSUnitLabels.xml");
            TelerikObject.Click(PageWellTest.dd_QualityCode);
            TelerikObject.Select_KendoUI_Listitem(tstqualitycode);
            Playback.Wait(2000);
            TelerikObject.Sendkeys(PageWellTest.txtOiRate, tstoil);
            TelerikObject.Sendkeys(PageWellTest.txtWaterRate, tstwater);
            TelerikObject.Sendkeys(PageWellTest.txtGasRate, tstgasrate);
            TelerikObject.Sendkeys(PageWellTest.txt_TubingHeadPressure, tsttubingpresure);
            TelerikObject.Sendkeys(PageWellTest.txt_TubingHeadTemperature, tsttubingtemperature);
            TelerikObject.Sendkeys(PageWellTest.txt_CasinggHeadPressure, casinghead);
            TelerikObject.Sendkeys(PageWellTest.txt_RRLPIP, tstesppip);
            TelerikObject.Sendkeys(PageWellTest.txt_PumpDischargePressure, tstesppdp);
            TelerikObject.Sendkeys(PageWellTest.txt_Frequency, tstfrequceny);
            TelerikObject.Sendkeys(PageWellTest.txt_MotorVolts, tstmotovolts);
            TelerikObject.Sendkeys(PageWellTest.txt_MotoroAmps, tstmotoramps);
            TelerikObject.Sendkeys(PageWellTest.txt_FlowLinePressure, tstflowlinepressure);
            TelerikObject.Sendkeys(PageWellTest.txt_SepraratorPressure, tstseparatorpressure);
            TelerikObject.Sendkeys(PageWellTest.txt_ChokeSize, tstchokesize);


            TelerikObject.Click(PageWellTest.btn_SaveWellTest);

        }

        private void AddNewGLWellTest(string filename)
        {
            // Populate test data from XML file
            string testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\TestData", filename);
            CommonHelper.TraceLine("Getting Data from XML file ... " + testdatafile);
            DataTable dt = CommonHelper.BuildDataTableFromXml(testdatafile);
            string testdatetime = dt.Rows[0]["TestDate"].ToString();
            string[] arrtestdate = testdatetime.Split(new char[] { ';' });
            string testdate = arrtestdate[0];
            // Populate variables 
            string strtoday = testdate.Replace("/", "");
            CommonHelper.TraceLine("Todays Date" + strtoday);
            TelerikObject.Sendkeys(PageWellTest.txtTestDate, strtoday, false, 1000);
            TelerikObject.Click(PageWellTest.dd_TestTime);
            string testtime = arrtestdate[1];
            TelerikObject.Select_KendoUI_Listitem(testtime);
            string tstqualitycode = dt.Rows[0]["QualityCode"].ToString();
            string tstoil = dt.Rows[0]["OilRateSTBd"].ToString();
            string tstwater = dt.Rows[0]["WaterRateSTBd"].ToString();
            string tstgasrate = dt.Rows[0]["GasRateMscfd"].ToString();
            string tsttubingpresure = dt.Rows[0]["TubingHeadPressurepsia"].ToString();
            string tsttubingtemperature = dt.Rows[0]["TubingHeadTemperatureF"].ToString();
            string tstcasingheadpressure = dt.Rows[0]["CasingHeadPressure"].ToString();
            string tstgasinjrate = dt.Rows[0]["GasInjectionRate"].ToString();
            string tstdownholepressuregauge = dt.Rows[0]["DownholePressureGauge"].ToString();
            string tstflowlinepressure = dt.Rows[0]["FlowLinePressure"].ToString();
            string tstseparatorpressure = dt.Rows[0]["SeparatorPressure"].ToString();
            string tstchokesize = dt.Rows[0]["ChokeSize"].ToString();
            //***********Input Data on Modal Dialog

            VerifyUnitLabelsTextOnAddDialogGL("GLWellTestUSUnitLabels.xml");
            TelerikObject.Click(PageWellTest.dd_QualityCode);
            TelerikObject.Select_KendoUI_Listitem(tstqualitycode);
            TelerikObject.Sendkeys(PageWellTest.txtOiRate, tstoil);
            TelerikObject.Sendkeys(PageWellTest.txtWaterRate, tstwater);
            TelerikObject.Sendkeys(PageWellTest.txtGasRate, tstgasrate);
            TelerikObject.Sendkeys(PageWellTest.txt_TubingHeadPressure, tsttubingpresure);
            TelerikObject.Sendkeys(PageWellTest.txt_TubingHeadTemperature, tsttubingtemperature);
            TelerikObject.Sendkeys(PageWellTest.txt_CasinggHeadPressure, tstcasingheadpressure);
            TelerikObject.Sendkeys(PageWellTest.txt_GasInjectionRate, tstgasinjrate);
            //TelerikObject.Sendkeys(PageWellTest.txt_FlowLinePressure, tstflowlinepressure);
            //TelerikObject.Sendkeys(PageWellTest.txt_SepraratorPressure, tstseparatorpressure);
            //TelerikObject.Sendkeys(PageWellTest.txt_ChokeSize, tstchokesize);


            TelerikObject.Click(PageWellTest.btn_SaveWellTest);

        }
        //EDITED FOR ESP



        //

        private void AddNewNFWellTest(string filename)
        {
            // Populate test data from XML file
            string testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\TestData", filename);
            CommonHelper.TraceLine("Getting Data from XML file ... " + testdatafile);
            DataTable dt = CommonHelper.BuildDataTableFromXml(testdatafile);
            string testdatetime = dt.Rows[0]["TestDate"].ToString();
            string[] arrtestdate = testdatetime.Split(new char[] { ';' });
            string testdate = arrtestdate[0];
            // Populate variables 
            string strtoday = testdate.Replace("/", "");
            CommonHelper.TraceLine("Todays Date" + strtoday);
            TelerikObject.Sendkeys(PageWellTest.txtTestDate, strtoday, false, 1000);
            TelerikObject.Click(PageWellTest.dd_TestTime);
            string testtime = arrtestdate[1];
            TelerikObject.Select_KendoUI_Listitem(testtime);
            string tstqualitycode = dt.Rows[0]["QualityCode"].ToString();
            string tstoil = dt.Rows[0]["OilRateSTBd"].ToString();
            string tstwater = dt.Rows[0]["WaterRateSTBd"].ToString();
            string tstgasrate = dt.Rows[0]["GasRateMscfd"].ToString();
            string tsttubingpresure = dt.Rows[0]["TubingHeadPressurepsia"].ToString();
            string tsttubingtemperature = dt.Rows[0]["TubingHeadTemperatureF"].ToString();
            string tstDownholePressureGauge = dt.Rows[0]["DownholePressureGauge"].ToString();
            string tstflowlinepressure = dt.Rows[0]["FlowLinePressure"].ToString();
            string tstseparatorpressure = dt.Rows[0]["SeparatorPressure"].ToString();
            string tstchokesize = dt.Rows[0]["ChokeSize"].ToString();
            //***********Input Data on Modal Dialog

            VerifyUnitLabelsTextOnAddDialogNF("NFWellTestUSUnitLabels.xml");
            TelerikObject.Click(PageWellTest.dd_QualityCode);
            TelerikObject.Select_KendoUI_Listitem(tstqualitycode);
            TelerikObject.Sendkeys(PageWellTest.txtOiRate, tstoil);
            TelerikObject.Sendkeys(PageWellTest.txtWaterRate, tstwater);
            TelerikObject.Sendkeys(PageWellTest.txtGasRate, tstgasrate);
            TelerikObject.Sendkeys(PageWellTest.txt_TubingHeadPressure, tsttubingpresure);
            TelerikObject.Sendkeys(PageWellTest.txt_TubingHeadTemperature, tsttubingtemperature);
            TelerikObject.Sendkeys(PageWellTest.txt_DownholePressureGauge, tstDownholePressureGauge);
            TelerikObject.Sendkeys(PageWellTest.txt_FlowLinePressure, tstflowlinepressure);
            TelerikObject.Sendkeys(PageWellTest.txt_SepraratorPressure, tstseparatorpressure);
            TelerikObject.Sendkeys(PageWellTest.txt_ChokeSize, tstchokesize);


            TelerikObject.Click(PageWellTest.btn_SaveWellTest);

        }

        private void VerifyRRLWellTestGridData(string expXmlFile, string unitsystem = "US")
        {
            //VerifyWellTestTableCellValues
            string testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\TestData", expXmlFile);
            CommonHelper.TraceLine("Getting Data from XML file ... " + testdatafile);
            DataTable dt = CommonHelper.BuildDataTableFromXml(testdatafile);
            if (unitsystem.ToUpper() == "METRIC")
            {
                VerifyRRLWellTestTableCellValuesMetric(dt, 0);
            }
            else
            {
                VerifyRRLWellTestTableCellValues(dt, 0);
            }
        }

        private void VerifyESPWellTestGridData(string expXmlFile, int dtnumber = 0, string unitsystem = "US")
        {
            //VerifyWellTestTableCellValues
            string testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\TestData", expXmlFile);
            CommonHelper.TraceLine("Getting Data from XML file ... " + testdatafile);
            DataTable dt = CommonHelper.BuildDataTableFromXml(testdatafile);
            if (unitsystem.ToUpper() == "METRIC")
            {
                VerifyESPWellTestTableCellValuesMetric(dt, dtnumber, 0);
            }
            else
            {
                CommonHelper.TraceLine("Data table record number :" + dtnumber);
                VerifyESPWellTestTableCellValues(dt, dtnumber, 0);
            }
        }

        public void VerifyESPWellTestGridData_Analysis(string expXmlFile, int dtnumber = 0, string unitsystem = "US")
        {
            //VerifyWellTestTableCellValues
            string testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\TestData", expXmlFile);
            CommonHelper.TraceLine("Getting Data from XML file ... " + testdatafile);
            DataTable dt = CommonHelper.BuildDataTableFromXml(testdatafile);
            if (unitsystem.ToUpper() == "METRIC")
            {
                VerifyESPWellTestTableCellValuesMetric(dt, dtnumber, 0);
            }
            else
            {
                CommonHelper.TraceLine("Data table record number :" + dtnumber);
                VerifyESPWellTestTableCellValues_Analysis(dt, dtnumber, 0);
            }
        }

        public void VerifyGLWellTestGridData(string expXmlFile, int dtnumber = 0, string unitsystem = "US")
        {
            //VerifyWellTestTableCellValues
            string testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\TestData", expXmlFile);
            CommonHelper.TraceLine("Getting Data from XML file ... " + testdatafile);
            DataTable dt = CommonHelper.BuildDataTableFromXml(testdatafile);
            if (unitsystem.ToUpper() == "METRIC")
            {
                VerifyGLWellTestTableCellValuesMetric(dt, dtnumber, 0);
            }
            else
            {
                CommonHelper.TraceLine("Data table record number :" + dtnumber);
                VerifyGLWellTestTableCellValues(dt, dtnumber, 0);
            }
        }





        private void VerifyNFWellTestGridData(string expXmlFile, int dtnumber = 0, string unitsystem = "US")
        {
            //VerifyWellTestTableCellValues
            string testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\TestData", expXmlFile);
            CommonHelper.TraceLine("Getting Data from XML file ... " + testdatafile);
            DataTable dt = CommonHelper.BuildDataTableFromXml(testdatafile);
            if (unitsystem.ToUpper() == "METRIC")
            {
                VerifyNFWellTestTableCellValuesMetric(dt, dtnumber, 0);
            }
            else
            {
                CommonHelper.TraceLine("Data table record number :" + dtnumber);
                VerifyNFWellTestTableCellValues(dt, dtnumber, 0);
            }
        }

        private void VerifyUnitLabelsTextOnAddDialogRRL(string filename)
        {
            string testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\TestData", filename);
            CommonHelper.TraceLine("Getting Data from XML file ... " + testdatafile);
            DataTable dt = CommonHelper.BuildDataTableFromXml(testdatafile);

            string tsttestdurationlabel = dt.Rows[0]["TestDurationHours"].ToString();
            string tstoilLabel = dt.Rows[0]["OilRateSTBd"].ToString();
            string tstwaterLabel = dt.Rows[0]["WaterRateSTBd"].ToString();
            string tstgasrateLabel = dt.Rows[0]["GasRateMscfd"].ToString();
            string tstoilapiLabel = dt.Rows[0]["OilGravityAPI"].ToString();
            string tstwatergravityLabel = dt.Rows[0]["WaterGravitySG"].ToString();
            string tstgasgravityLabel = dt.Rows[0]["GasGravitySG"].ToString();
            string tsttubingpresureLabel = dt.Rows[0]["AverageTubingPressurepsia"].ToString();
            string tsttubingtemperatureLabel = dt.Rows[0]["AverageTubingTemperatureF"].ToString();
            string tstcaingpressureLabel = dt.Rows[0]["AverageCasingPressurepsia"].ToString();
            string tstfapLabel = dt.Rows[0]["AverageFluidAbovePumpft"].ToString();
            string tstpmphrsLabel = dt.Rows[0]["PumpingHoursHours"].ToString();
            string tstTestDurationHourssLabel = dt.Rows[0]["TestDurationHours"].ToString();
            string tstPumpEfficiencyLabel = dt.Rows[0]["PumpEfficiency"].ToString();
            string tstPumpIntakePressureLabel = dt.Rows[0]["PumpIntakePressure"].ToString();
            string tstStorkesPerMinute = dt.Rows[0]["StrokesPerMinute"].ToString();

            //Verify Test Duration Label
            UnitlabelVerification(PageWellTest.divcontrolTestDuration, "Test Duration", tsttestdurationlabel);
            UnitlabelVerification(PageWellTest.divcontrolOilRate, "Oil Rate", tstoilLabel);
            UnitlabelVerification(PageWellTest.divcontrolWaterRate, "Water Rate", tstwaterLabel);
            UnitlabelVerification(PageWellTest.divcontrolGasRate, "Gas Rate", tstgasrateLabel);
            UnitlabelVerification(PageWellTest.divcontrolOilgravity, "Oil Gravity", tstoilapiLabel);
            UnitlabelVerification(PageWellTest.divcontrolWaterGravity, "Water Gravity", tstwatergravityLabel);
            UnitlabelVerification(PageWellTest.divcontrolGasGravity, "Gas Gravity", tstgasgravityLabel);
            UnitlabelVerification(PageWellTest.divcontrolTubingHeadPressure, "Tubing Head Pressure", tsttubingpresureLabel);
            UnitlabelVerification(PageWellTest.divcontrolTubingHeadTemperature, "Tubing Head Temperature", tsttubingtemperatureLabel);
            UnitlabelVerification(PageWellTest.divcontrolCasingHeadPressure, "Casing Head Pressure", tstcaingpressureLabel);
            UnitlabelVerification(PageWellTest.divcontrolAverageFluidAbovePump, "Average Fluid Above Pump", tstfapLabel);
            UnitlabelVerification(PageWellTest.divcontrolPumpIntakePressure, "Pump Intake Pressure", tstPumpIntakePressureLabel);
            UnitlabelVerification(PageWellTest.divcontrolStrokesPerMinute, "Strokes Per Minute", tstStorkesPerMinute);
            UnitlabelVerification(PageWellTest.divcontrolPumpingHours, "Pumping Hours", tstpmphrsLabel);
            UnitlabelVerification(PageWellTest.divcontrolPumpEfficiency, "Pump Efficiency", tstPumpEfficiencyLabel);


        }
        private void VerifyUnitLabelsTextOnAddDialogESP(string filename)
        {
            string testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\TestData", filename);
            CommonHelper.TraceLine("Getting Data from XML file ... " + testdatafile);
            DataTable dt = CommonHelper.BuildDataTableFromXml(testdatafile);

            string tstqualitycode = dt.Rows[0]["QualityCode"].ToString();
            string tstoil = dt.Rows[0]["OilRateSTBd"].ToString();
            string tstwater = dt.Rows[0]["WaterRateSTBd"].ToString();
            string tstgasrate = dt.Rows[0]["GasRateMscfd"].ToString();
            string tsttubingpresure = dt.Rows[0]["TubingHeadPressurepsia"].ToString();
            string tsttubingtemperature = dt.Rows[0]["TubingHeadTemperatureF"].ToString();

            string tstesppip = dt.Rows[0]["PumpIntakePressure"].ToString();
            string tstesppdp = dt.Rows[0]["PumpDishcargePressure"].ToString();
            string tstfrequceny = dt.Rows[0]["Frequency"].ToString();
            string tstmotovolts = dt.Rows[0]["MotorVolts"].ToString();
            string tstmotoramps = dt.Rows[0]["MotorAmps"].ToString();
            string tstflowlinepressure = dt.Rows[0]["FlowLinePressure"].ToString();
            string tstseparatorpressure = dt.Rows[0]["SeparatorPressure"].ToString();
            string tstchokesize = dt.Rows[0]["ChokeSize"].ToString();

            //Verify Test Duration Label

            UnitlabelVerification(PageWellTest.divcontrolOilRate, "Oil Rate", tstoil);
            UnitlabelVerification(PageWellTest.divcontrolWaterRate, "Water Rate", tstwater);
            UnitlabelVerification(PageWellTest.divcontrolGasRate, "Gas Rate", tstgasrate);

            UnitlabelVerification(PageWellTest.divcontrolTubingHeadPressure, "Tubing Head Pressure", tsttubingpresure);
            UnitlabelVerification(PageWellTest.divcontrolTubingHeadTemperature, "Tubing Head Temperature", tsttubingtemperature);
            UnitlabelVerification(PageWellTest.divcontrolPumpIntakePressure, "Pump Intake Pressure", tstesppip);
            UnitlabelVerification(PageWellTest.divcontrolESPPDP, "Pump Discharge Pressure", tstesppdp);
            UnitlabelVerification(PageWellTest.divcontrolESPFrequency, "Frequceny", tstfrequceny);

            UnitlabelVerification(PageWellTest.divcontrolSESPMotorVolts, "Motor Volts", tstmotovolts);
            UnitlabelVerification(PageWellTest.divcontrolSESPMotorAmps, "Motor Amps", tstmotoramps);
            UnitlabelVerification(PageWellTest.divcontrolFlowLinePressure, "Flow Line Pressure", tstflowlinepressure);
            UnitlabelVerification(PageWellTest.divcontrolSeparatorPressure, "Separator Pressure", tstseparatorpressure);
            UnitlabelVerification(PageWellTest.divcontrolChokeSize, "Choke Size", tstchokesize);


        }

        private void VerifyUnitLabelsTextOnAddDialogGL(string filename)
        {
            string testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\TestData", filename);
            CommonHelper.TraceLine("Getting Data from XML file ... " + testdatafile);
            DataTable dt = CommonHelper.BuildDataTableFromXml(testdatafile);

            string tstqualitycode = dt.Rows[0]["QualityCode"].ToString();
            string tstoil = dt.Rows[0]["OilRateSTBd"].ToString();
            string tstwater = dt.Rows[0]["WaterRateSTBd"].ToString();
            string tstgasrate = dt.Rows[0]["GasRateMscfd"].ToString();
            string tsttubingpresure = dt.Rows[0]["TubingHeadPressurepsia"].ToString();
            string tsttubingtemperature = dt.Rows[0]["TubingHeadTemperatureF"].ToString();
            string tstcasingheadpressure = dt.Rows[0]["CasingHeadPressure"].ToString();
            string tstgasinjrate = dt.Rows[0]["GasInjectionRate"].ToString();
            string tstdownholepressuregauge = dt.Rows[0]["DownholePressureGauge"].ToString();
            string tstflowlinepressure = dt.Rows[0]["FlowLinePressure"].ToString();
            string tstseparatorpressure = dt.Rows[0]["SeparatorPressure"].ToString();
            string tstchokesize = dt.Rows[0]["ChokeSize"].ToString();

            //Verify Test Duration Label

            UnitlabelVerification(PageWellTest.divcontrolOilRate, "Oil Rate", tstoil);
            UnitlabelVerification(PageWellTest.divcontrolWaterRate, "Water Rate", tstwater);
            UnitlabelVerification(PageWellTest.divcontrolGasRate, "Gas Rate", tstgasrate);

            UnitlabelVerification(PageWellTest.divcontrolTubingHeadPressure, "Tubing Head Pressure", tsttubingpresure);
            UnitlabelVerification(PageWellTest.divcontrolTubingHeadTemperature, "Tubing Head Temperature", tsttubingtemperature);
            UnitlabelVerification(PageWellTest.divcontrolCasingHeadPressure, "Casing Head Pressure", tstcasingheadpressure);
            UnitlabelVerification(PageWellTest.divcontrolGasInjectionRate, "Gas Injection Rate", tstgasinjrate);


            UnitlabelVerification(PageWellTest.divcontrolDownholePressureGauge, "Downhole Pressure Gauge", tstdownholepressuregauge);

            UnitlabelVerification(PageWellTest.divcontrolFlowLinePressure, "Flow Line Pressure", tstflowlinepressure);
            UnitlabelVerification(PageWellTest.divcontrolSeparatorPressure, "Separator Pressure", tstseparatorpressure);
            UnitlabelVerification(PageWellTest.divcontrolChokeSize, "Choke Size", tstchokesize);


        }

        private void VerifyUnitLabelsTextOnAddDialogNF(string filename)
        {
            string testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\TestData", filename);
            CommonHelper.TraceLine("Getting Data from XML file ... " + testdatafile);
            DataTable dt = CommonHelper.BuildDataTableFromXml(testdatafile);

            string tstqualitycode = dt.Rows[0]["QualityCode"].ToString();
            string tstoil = dt.Rows[0]["OilRateSTBd"].ToString();
            string tstwater = dt.Rows[0]["WaterRateSTBd"].ToString();
            string tstgasrate = dt.Rows[0]["GasRateMscfd"].ToString();
            string tsttubingpresure = dt.Rows[0]["TubingHeadPressurepsia"].ToString();
            string tsttubingtemperature = dt.Rows[0]["TubingHeadTemperatureF"].ToString();
            string tstDownholePressureGauge = dt.Rows[0]["DownholePressureGauge"].ToString();
            string tstflowlinepressure = dt.Rows[0]["FlowLinePressure"].ToString();
            string tstseparatorpressure = dt.Rows[0]["SeparatorPressure"].ToString();
            string tstchokesize = dt.Rows[0]["ChokeSize"].ToString();

            //Verify Test Duration Label

            UnitlabelVerification(PageWellTest.divcontrolOilRate, "Oil Rate", tstoil);
            UnitlabelVerification(PageWellTest.divcontrolWaterRate, "Water Rate", tstwater);
            UnitlabelVerification(PageWellTest.divcontrolGasRate, "Gas Rate", tstgasrate);

            UnitlabelVerification(PageWellTest.divcontrolTubingHeadPressure, "Tubing Head Pressure", tsttubingpresure);
            UnitlabelVerification(PageWellTest.divcontrolTubingHeadTemperature, "Tubing Head Temperature", tsttubingtemperature);
            UnitlabelVerification(PageWellTest.divcontrolFlowLinePressure, "Flow Line Pressure", tstflowlinepressure);
            UnitlabelVerification(PageWellTest.divcontrolSeparatorPressure, "Separator Pressure", tstseparatorpressure);
            UnitlabelVerification(PageWellTest.divcontrolDownholePressureGauge, "DownHole Pressire Gauge", tstDownholePressureGauge);
            UnitlabelVerification(PageWellTest.divcontrolChokeSize, "Choke Size", tstchokesize);


        }
        private void VerifyRRLWellTestTableCellValues(DataTable dt, int rownum = 0, string UOM = "US")
        {

            //Verify the First Table Column
            string[] testdateonly = dt.Rows[0]["TestDate"].ToString().Split(new char[] { ';' });
            DateTime testdt = Convert.ToDateTime(testdateonly[0]);
            string modifiledttime = testdt.ToString("M/d/yy") + ", " + testdateonly[1];
            CommonHelper.TraceLine("Modified Value" + modifiledttime);
            TelerikObject.verifyGridCellValues("Test Date", "IgnoreValue;" + modifiledttime);

            string colnames = "Test Duration (Hours);Quality Code;Oil Rate (STB/d);Water Rate (STB/d);Gas Rate (Mscf/d);Oil Gravity (°API);Water Gravity (SG);Gas Gravity (SG);Average Tubing Pressure (psia);Average Tubing Temperature (°F);Average Casing Pressure (psia);Average Fluid Above Pump (ft);Pump Intake Pressure (psia);Strokes Per Minute (SPM);Pumping Hours (Hours);Pump Efficiency (%);Id;Assembly;Last Modified;Modified By";

            string tstduration = dt.Rows[0]["TestDurationHours"].ToString();
            string tstqualitycode = dt.Rows[0]["QualityCode"].ToString();
            string tstoil = dt.Rows[0]["OilRateSTBd"].ToString();
            string tstwater = dt.Rows[0]["WaterRateSTBd"].ToString();
            string tstgasrate = dt.Rows[0]["GasRateMscfd"].ToString();
            string tstoilapi = dt.Rows[0]["OilGravityAPI"].ToString();
            string tstwatergravity = dt.Rows[0]["WaterGravitySG"].ToString();
            string tstgasgravity = dt.Rows[0]["GasGravitySG"].ToString();
            string tsttubingpresure = dt.Rows[0]["AverageTubingPressurepsia"].ToString();
            string tsttubingtemperature = dt.Rows[0]["AverageTubingTemperatureF"].ToString();
            string tstcaingpressure = dt.Rows[0]["AverageCasingPressurepsia"].ToString();
            string tstfap = dt.Rows[0]["AverageFluidAbovePumpft"].ToString();
            string tstpip = dt.Rows[0]["PumpIntakePressure"].ToString();
            string tstspm = dt.Rows[0]["StrokesPerMinute"].ToString();
            string tstpmphrs = dt.Rows[0]["PumpingHoursHours"].ToString();
            string tstpumpeffcy = dt.Rows[0]["PumpEfficiency"].ToString();
            string ignore = "IgnoreValue";
            UserPrincipal userPrincipal = UserPrincipal.Current;
            string usrname = userPrincipal.DisplayName;
            string colvals = tstduration + ";" +
                             tstqualitycode + ";" +
                             tstoil + ";" +
                             tstwater + ";" +
                             tstgasrate + ";" +
                             tstoilapi + ";" +
                             tstwatergravity + ";" +
                             tstgasgravity + ";" +
                             tsttubingpresure + ";" +
                             tsttubingtemperature + ";" +
                             tstcaingpressure + ";" +
                             tstfap + ";" +
                             tstpip + ";" +
                             tstspm + ";" +
                             tstpmphrs + ";" +
                             tstpumpeffcy + ";" +
                             ignore + ";" +
                             ignore + ";" +
                             ignore + ";" +
                             usrname;


            TelerikObject.verifyGridCellValues(colnames, colvals, rownum);
        }

        private void VerifyRRLWellTestTableCellValuesMetric(DataTable dt, int rownum = 0)

        {




            //Verify the First Table Column
            string[] testdateonly = dt.Rows[0]["TestDate"].ToString().Split(new char[] { ';' });
            DateTime testdt = Convert.ToDateTime(testdateonly[0]);
            string modifiledttime = testdt.ToString("M/d/yy") + ", " + testdateonly[1];
            TelerikObject.verifyGridCellValues("Test Date", "IgnoreValue;" + modifiledttime);
            string colnames = "Test Duration (Hours);Quality Code;Oil Rate (sm³/d);Water Rate (sm³/d);Gas Rate (sm³/d);Oil Gravity (SG);Water Gravity (SG);Gas Gravity (SG);Average Tubing Pressure (kPa);Average Tubing Temperature (°C);Average Casing Pressure (kPa);Average Fluid Above Pump (m);Pump Intake Pressure (kPa);Strokes Per Minute (SPM);Pumping Hours (Hours);Pump Efficiency (%);Id;Assembly;Last Modified;Modified By";
            string tstduration = dt.Rows[0]["TestDurationHours"].ToString();
            string tstqualitycode = dt.Rows[0]["QualityCode"].ToString();
            string tstoil = dt.Rows[0]["OilRateSTBd"].ToString();
            tstoil = UnitsConversion("STB/d", Convert.ToDouble(tstoil)).ToString();
            string tstwater = dt.Rows[0]["WaterRateSTBd"].ToString();
            tstwater = UnitsConversion("STB/d", Convert.ToDouble(tstwater)).ToString();
            string tstgasrate = dt.Rows[0]["GasRateMscfd"].ToString();
            tstgasrate = UnitsConversion("Mscf/d", Convert.ToDouble(tstgasrate)).ToString();
            string tstoilapi = dt.Rows[0]["OilGravityAPI"].ToString();
            tstoilapi = UnitsConversion("API", Convert.ToDouble(tstoilapi)).ToString();
            string tstwatergravity = dt.Rows[0]["WaterGravitySG"].ToString();
            string tstgasgravity = dt.Rows[0]["GasGravitySG"].ToString();
            string tsttubingpresure = dt.Rows[0]["AverageTubingPressurepsia"].ToString();
            tsttubingpresure = UnitsConversion("psia", Convert.ToDouble(tsttubingpresure)).ToString();
            string tsttubingtemperature = dt.Rows[0]["AverageTubingTemperatureF"].ToString();
            tsttubingtemperature = UnitsConversion("F", Convert.ToDouble(tsttubingtemperature)).ToString();
            string tstcaingpressure = dt.Rows[0]["AverageCasingPressurepsia"].ToString();
            tstcaingpressure = UnitsConversion("psia", Convert.ToDouble(tstcaingpressure)).ToString();
            string tstfap = dt.Rows[0]["AverageFluidAbovePumpft"].ToString();
            tstfap = UnitsConversion("ft", Convert.ToDouble(tstfap)).ToString();
            string tstpip = dt.Rows[0]["PumpIntakePressure"].ToString();
            if (tstpip.Length > 0)
            {
                tstpip = UnitsConversion("psia", Convert.ToDouble(tstpip)).ToString();
            }
            else
            {
                //No Need To Convert Blank Value
            }

            string tstspm = dt.Rows[0]["StrokesPerMinute"].ToString();
            string tstpmphrs = dt.Rows[0]["PumpingHoursHours"].ToString();
            string tstpumpeffcy = dt.Rows[0]["PumpEfficiency"].ToString();
            string ignore = "IgnoreValue";
            UserPrincipal userPrincipal = UserPrincipal.Current;
            string usrname = userPrincipal.DisplayName;
            string colvals = tstduration + ";" +
                             tstqualitycode + ";" +
                             tstoil + ";" +
                             tstwater + ";" +
                             tstgasrate + ";" +
                             tstoilapi + ";" +
                             tstwatergravity + ";" +
                             tstgasgravity + ";" +
                             tsttubingpresure + ";" +
                             tsttubingtemperature + ";" +
                             tstcaingpressure + ";" +
                             tstfap + ";" +
                             tstpip + ";" +
                             tstspm + ";" +
                             tstpmphrs + ";" +
                             tstpumpeffcy + ";" +
                             ignore + ";" +
                             ignore + ";" +
                             ignore + ";" +
                             usrname;


            TelerikObject.verifyGridCellValues(colnames, colvals, rownum);
        }

        private void VerifyESPWellTestTableCellValues(DataTable dt, int dtrecord = 0, int rownum = 0, string UOM = "US")
        {

            //Verify the First Table Column
            string[] testdateonly = dt.Rows[0]["TestDate"].ToString().Split(new char[] { ';' });
            DateTime testdt = Convert.ToDateTime(testdateonly[0]);
            string modifiledttime = testdt.ToString("M/d/yy") + ", " + testdateonly[1];
            CommonHelper.TraceLine("Modified Value" + modifiledttime);
            TelerikObject.verifyGridCellValues("Test Date", "IgnoreValue;" + modifiledttime);

            string colnames = "Quality Code;Tuning Method;Status;Message;Tubing Head Pressure (psia);Tubing Head Temperature (°F);Pump Intake Pressure (psia);Pump Discharge Pressure (psia);Frequency (Hz);Motor Volts (V);Motor Amps (A);Flow Line Pressure (psia);Separator Pressure (psia);Choke Size (64th in);Oil Rate (STB/d);Gas Rate (Mscf/d);Water Rate (STB/d);GOR (scf/STB);Water Cut (fraction);FBHP (psia);L Factor;Productivity Index (STB/d/psi);Reservoir Pressure (psia);Head Tuning Factor;Choke D Factor;Last Modified;Modified By";


            string tstqualitycode = dt.Rows[dtrecord]["QualityCode"].ToString();
            string tsttuningmethod = dt.Rows[dtrecord]["TuningMethod"].ToString();
            string tsttuningstatus = dt.Rows[dtrecord]["TuningStatus"].ToString();
            string tsttuningmsg = dt.Rows[dtrecord]["TuningMessage"].ToString();
            string tstoil = dt.Rows[dtrecord]["OilRateSTBd"].ToString();
            string tstwater = dt.Rows[dtrecord]["WaterRateSTBd"].ToString();
            string tstgasrate = dt.Rows[dtrecord]["GasRateMscfd"].ToString();
            string tsttubingpresure = dt.Rows[dtrecord]["TubingHeadPressurepsia"].ToString();
            string tsttubingtemperature = dt.Rows[dtrecord]["TubingHeadTemperatureF"].ToString();
            string tstesppip = dt.Rows[dtrecord]["PumpIntakePressure"].ToString();
            string tstesppdp = dt.Rows[dtrecord]["PumpDishcargePressure"].ToString();
            string tstfrequceny = dt.Rows[dtrecord]["Frequency"].ToString();
            string tstmotovolts = dt.Rows[dtrecord]["MotorVolts"].ToString();
            string tstmotoramps = dt.Rows[dtrecord]["MotorAmps"].ToString();
            string tstflowlinepressure = dt.Rows[dtrecord]["FlowLinePressure"].ToString();
            string tstseparatorpressure = dt.Rows[dtrecord]["SeparatorPressure"].ToString();
            string tstchokesize = dt.Rows[dtrecord]["ChokeSize"].ToString();
            string tstGOR = dt.Rows[dtrecord]["GOR"].ToString();
            string tstWaterCut = dt.Rows[dtrecord]["WaterCut"].ToString();
            string tstFBHP = dt.Rows[dtrecord]["FBHP"].ToString();
            string tstLFACTOR = dt.Rows[dtrecord]["LFactor"].ToString();
            string tstPI = dt.Rows[dtrecord]["PI"].ToString();
            string tstRsrvPressure = dt.Rows[dtrecord]["ReserviorPressure"].ToString();
            string tstRHeadFactor = dt.Rows[dtrecord]["HeadTuningFactor"].ToString();
            string tstChokeDFactor = dt.Rows[dtrecord]["ChokeDFactor"].ToString();
            string ignore = "IgnoreValue";
            UserPrincipal userPrincipal = UserPrincipal.Current;
            string usrname = userPrincipal.DisplayName;
            string colvals =
                             tstqualitycode + ";" +
                             tsttuningmethod + ";" +
                             tsttuningstatus + ";" +
                             tsttuningmsg + ";" +
                             tsttubingpresure + ";" +
                             tsttubingtemperature + ";" +
                             tstesppip + ";" +
                             tstesppdp + ";" +
                             tstfrequceny + ";" +
                             tstmotovolts + ";" +
                             tstmotoramps + ";" +
                             tstflowlinepressure + ";" +
                             tstseparatorpressure + ";" +
                             tstchokesize + ";" +
                             tstoil + ";" +
                             tstgasrate + ";" +
                             tstwater + ";" +
                             tstGOR + ";" +
                             tstWaterCut + ";" +
                             tstFBHP + ";" +
                             tstLFACTOR + ";" +
                             tstPI + ";" +
                             tstRsrvPressure + ";" +
                             tstRHeadFactor + ";" +
                             tstChokeDFactor + ";" +
                             ignore + ";" +
                             usrname;
            CommonHelper.TraceLine("Expected Tuning method is " + tsttuningmethod);
            TelerikObject.verifyGridCellValues(colnames, colvals, rownum);
        }
        private void VerifyESPWellTestTableCellValues_Analysis(DataTable dt, int dtrecord = 0, int rownum = 0, string UOM = "US")
        {

            //Verify the First Table Column
            string[] testdateonly = dt.Rows[0]["TestDate"].ToString().Split(new char[] { ';' });
            DateTime testdt = Convert.ToDateTime(testdateonly[0]);
            string modifiledttime = testdt.ToString("M/d/yy") + ", " + testdateonly[1];
            CommonHelper.TraceLine("Modified Value" + modifiledttime);
            // TelerikObject.verifyGridCellValues("Test Date", "IgnoreValue;" + modifiledttime);

            string colnames = "Quality Code;Tuning Method;Status;Message;Tubing Head Pressure (psia);Tubing Head Temperature (°F);Casing Head Pressure (psia);Pump Intake Pressure (psia);Pump Discharge Pressure (psia);Frequency (Hz);Motor Volts (V);Motor Amps (A);Flow Line Pressure (psia);Separator Pressure (psia);Choke Size (64th in);Oil Rate (STB/d);Gas Rate (Mscf/d);Water Rate (STB/d);GOR (scf/STB);Water Cut (fraction);FBHP (psia);L Factor;Productivity Index (STB/d/psi);Reservoir Pressure (psia);Head Tuning Factor;Choke D Factor;Last Modified;Modified By";


            string tstqualitycode = dt.Rows[dtrecord]["QualityCode"].ToString();
            string tsttuningmethod = dt.Rows[dtrecord]["TuningMethod"].ToString();
            string tsttuningstatus = dt.Rows[dtrecord]["TuningStatus"].ToString();
            string tsttuningmsg = dt.Rows[dtrecord]["TuningMessage"].ToString();
            string tstoil = dt.Rows[dtrecord]["OilRateSTBd"].ToString();
            string tstwater = dt.Rows[dtrecord]["WaterRateSTBd"].ToString();
            string tstgasrate = dt.Rows[dtrecord]["GasRateMscfd"].ToString();
            string tsttubingpresure = dt.Rows[dtrecord]["TubingHeadPressurepsia"].ToString();
            string tsttubingtemperature = dt.Rows[dtrecord]["TubingHeadTemperatureF"].ToString();
            string tstcasingheadpressure = dt.Rows[dtrecord]["CasingHeadPressurePsia"].ToString();
            string tstesppip = dt.Rows[dtrecord]["PumpIntakePressure"].ToString();
            string tstesppdp = dt.Rows[dtrecord]["PumpDishcargePressure"].ToString();
            string tstfrequceny = dt.Rows[dtrecord]["Frequency"].ToString();
            string tstmotovolts = dt.Rows[dtrecord]["MotorVolts"].ToString();
            string tstmotoramps = dt.Rows[dtrecord]["MotorAmps"].ToString();
            string tstflowlinepressure = dt.Rows[dtrecord]["FlowLinePressure"].ToString();
            string tstseparatorpressure = dt.Rows[dtrecord]["SeparatorPressure"].ToString();
            string tstchokesize = dt.Rows[dtrecord]["ChokeSize"].ToString();
            string tstGOR = dt.Rows[dtrecord]["GOR"].ToString();
            string tstWaterCut = dt.Rows[dtrecord]["WaterCut"].ToString();
            string tstFBHP = dt.Rows[dtrecord]["FBHP"].ToString();
            string tstLFACTOR = dt.Rows[dtrecord]["LFactor"].ToString();
            string tstPI = dt.Rows[dtrecord]["PI"].ToString();
            string tstRsrvPressure = dt.Rows[dtrecord]["ReserviorPressure"].ToString();
            string tstRHeadFactor = dt.Rows[dtrecord]["HeadTuningFactor"].ToString();
            string tstChokeDFactor = dt.Rows[dtrecord]["ChokeDFactor"].ToString();
            string ignore = "IgnoreValue";
            UserPrincipal userPrincipal = UserPrincipal.Current;
            string usrname = userPrincipal.DisplayName;
            string colvals =
                             tstqualitycode + ";" +
                             tsttuningmethod + ";" +
                             tsttuningstatus + ";" +
                             tsttuningmsg + ";" +
                             tsttubingpresure + ";" +
                             tsttubingtemperature + ";" +
                             tstcasingheadpressure + ";" +
                             tstesppip + ";" +
                             tstesppdp + ";" +
                             tstfrequceny + ";" +
                             tstmotovolts + ";" +
                             tstmotoramps + ";" +
                             tstflowlinepressure + ";" +
                             tstseparatorpressure + ";" +
                             tstchokesize + ";" +
                             tstoil + ";" +
                             tstgasrate + ";" +
                             tstwater + ";" +
                             tstGOR + ";" +
                             tstWaterCut + ";" +
                             tstFBHP + ";" +
                             tstLFACTOR + ";" +
                             tstPI + ";" +
                             tstRsrvPressure + ";" +
                             tstRHeadFactor + ";" +
                             tstChokeDFactor + ";" +
                             ignore + ";" +
                             usrname;
            CommonHelper.TraceLine("Expected Tuning method is " + tsttuningmethod);
            TelerikObject.verifyGridCellValues(colnames, colvals, rownum);

        }

        private void VerifyGLWellTestTableCellValues(DataTable dt, int dtrecord = 0, int rownum = 0, string UOM = "US")
        {

            //Verify the First Table Column
            string[] testdateonly = dt.Rows[0]["TestDate"].ToString().Split(new char[] { ';' });
            DateTime testdt = Convert.ToDateTime(testdateonly[0]);
            string modifiledttime = testdt.ToString("M/d/yy") + ", " + testdateonly[1];
            CommonHelper.TraceLine("Modified Value" + modifiledttime);
            TelerikObject.verifyGridCellValues("Test Date", "IgnoreValue;" + modifiledttime);

            //      string colnames = "Quality Code;Tuning Method;Status;Message;Tubing Head Pressure (psia);Tubing Head Temperature (°F);Casing Head Pressure (psia);Gas Injection Rate (Mscf/d);Downhole Pressure Gauge (psia);Flow Line Pressure (psia);Separator Pressure (psia);Choke Size (64th in);Oil Rate (STB/d);Gas Rate (Mscf/d);Water Rate (STB/d);Formation GOR (scf/STB);Water Cut (fraction);Injection GLR (scf/STB);FBHP (psia);L Factor;Productivity Index (STB/d/psi);Reservoir Pressure (psia);Choke D Factor;Orifice Size (64th in);Last Modified;Modified By";
            string colnames = "Quality Code;Tuning Method;Status;Message;Tubing Head Pressure (psia);Tubing Head Temperature (°F);Casing Head Pressure (psia);Gas Injection Rate (Mscf/d);Downhole Pressure Gauge (psia);Flow Line Pressure (psia);Separator Pressure (psia);Choke Size (64th in);Oil Rate (STB/d);Gas Rate (Mscf/d);Water Rate (STB/d);Formation GOR (scf/STB);Water Cut (fraction);Injection GLR (scf/STB);FBHP (psia);L Factor;Productivity Index (STB/d/psi);Reservoir Pressure (psia);Choke D Factor;Last Modified;Modified By";


            string tstqualitycode = dt.Rows[dtrecord]["QualityCode"].ToString();
            string tsttuningmethod = dt.Rows[dtrecord]["TuningMethod"].ToString();
            string tsttuningstatus = dt.Rows[dtrecord]["TuningStatus"].ToString();
            string tsttuningmsg = dt.Rows[dtrecord]["TuningMessage"].ToString();
            string tstoil = dt.Rows[dtrecord]["OilRateSTBd"].ToString();
            string tstwater = dt.Rows[dtrecord]["WaterRateSTBd"].ToString();
            string tstgasrate = dt.Rows[dtrecord]["GasRateMscfd"].ToString();
            string tsttubingpresure = dt.Rows[dtrecord]["TubingHeadPressurepsia"].ToString();
            string tsttubingtemperature = dt.Rows[dtrecord]["TubingHeadTemperatureF"].ToString();
            string tstcasingheadpressure = dt.Rows[dtrecord]["CasingHeadPressure"].ToString();
            string tstGasInjectionRate = dt.Rows[dtrecord]["GasInjectionRate"].ToString();
            string tstDownholePressureGauge = dt.Rows[dtrecord]["DownholePressureGauge"].ToString();
            string tstflowlinepressure = dt.Rows[dtrecord]["FlowLinePressure"].ToString();
            string tstseparatorpressure = dt.Rows[dtrecord]["SeparatorPressure"].ToString();
            string tstchokesize = dt.Rows[dtrecord]["ChokeSize"].ToString();
            string tstFormationGOR = dt.Rows[dtrecord]["FormationGOR"].ToString();
            string tstWaterCut = dt.Rows[dtrecord]["WaterCut"].ToString();
            string tstInjectionGLR = dt.Rows[dtrecord]["InjectionGLR"].ToString();

            string tstFBHP = dt.Rows[dtrecord]["FBHP"].ToString();
            string tstLFACTOR = dt.Rows[dtrecord]["LFactor"].ToString();
            string tstPI = dt.Rows[dtrecord]["PI"].ToString();
            string tstRsrvPressure = dt.Rows[dtrecord]["ReserviorPressure"].ToString();
            string tstOrificeSize = dt.Rows[dtrecord]["OrificeSize"].ToString();
            string tstChokeDFactor = dt.Rows[dtrecord]["ChokeDFactor"].ToString();
            string ignore = "IgnoreValue";
            UserPrincipal userPrincipal = UserPrincipal.Current;
            string usrname = userPrincipal.DisplayName;
            string colvals =
                             tstqualitycode + ";" +
                             tsttuningmethod + ";" +
                             tsttuningstatus + ";" +
                             tsttuningmsg + ";" +
                             tsttubingpresure + ";" +
                             tsttubingtemperature + ";" +
                             tstcasingheadpressure + ";" +
                             tstGasInjectionRate + ";" +
                             tstDownholePressureGauge + ";" +
                             tstflowlinepressure + ";" +
                             tstseparatorpressure + ";" +
                             tstchokesize + ";" +
                             tstoil + ";" +
                             tstgasrate + ";" +
                             tstwater + ";" +
                             tstFormationGOR + ";" +
                             tstWaterCut + ";" +
                             tstInjectionGLR + ";" +
                             tstFBHP + ";" +
                             tstLFACTOR + ";" +
                             tstPI + ";" +
                             tstRsrvPressure + ";" +
                             tstChokeDFactor + ";" +
                             //     tstOrificeSize + ";" +
                             ignore + ";" +
                             usrname;
            CommonHelper.TraceLine("Expected Tuning method is " + tsttuningmethod);
            TelerikObject.verifyGridCellValues(colnames, colvals, rownum);
        }
        private void VerifyNFWellTestTableCellValues(DataTable dt, int dtrecord = 0, int rownum = 0, string UOM = "US")
        {

            //Verify the First Table Column
            string[] testdateonly = dt.Rows[0]["TestDate"].ToString().Split(new char[] { ';' });
            DateTime testdt = Convert.ToDateTime(testdateonly[0]);
            string modifiledttime = testdt.ToString("M/d/yy") + ", " + testdateonly[1];
            CommonHelper.TraceLine("Modified Value" + modifiledttime);
            TelerikObject.verifyGridCellValues("Test Date", "IgnoreValue;" + modifiledttime);

            string colnames = "Quality Code;Tuning Method;Status;Message;Tubing Head Pressure (psia);Tubing Head Temperature (°F);Downhole Pressure Gauge (psia);Flow Line Pressure (psia);Separator Pressure (psia);Choke Size (64th in);Oil Rate (STB/d);Gas Rate (Mscf/d);Water Rate (STB/d);GOR (scf/STB);Water Cut (fraction);FBHP (psia);L Factor;Productivity Index (STB/d/psi);Reservoir Pressure (psia);Choke D Factor;Last Modified;Modified By";


            string tstqualitycode = dt.Rows[dtrecord]["QualityCode"].ToString();
            string tsttuningmethod = dt.Rows[dtrecord]["TuningMethod"].ToString();
            string tsttuningstatus = dt.Rows[dtrecord]["TuningStatus"].ToString();
            string tsttuningmsg = dt.Rows[dtrecord]["TuningMessage"].ToString();
            string tstoil = dt.Rows[dtrecord]["OilRateSTBd"].ToString();
            string tstwater = dt.Rows[dtrecord]["WaterRateSTBd"].ToString();
            string tstgasrate = dt.Rows[dtrecord]["GasRateMscfd"].ToString();
            string tsttubingpresure = dt.Rows[dtrecord]["TubingHeadPressurepsia"].ToString();
            string tsttubingtemperature = dt.Rows[dtrecord]["TubingHeadTemperatureF"].ToString();
            string tstDownholePressureGauge = dt.Rows[dtrecord]["DownholePressureGauge"].ToString();
            string tstflowlinepressure = dt.Rows[dtrecord]["FlowLinePressure"].ToString();
            string tstseparatorpressure = dt.Rows[dtrecord]["SeparatorPressure"].ToString();
            string tstchokesize = dt.Rows[dtrecord]["ChokeSize"].ToString();
            string tstGOR = dt.Rows[dtrecord]["GOR"].ToString();
            string tstWaterCut = dt.Rows[dtrecord]["WaterCut"].ToString();
            string tstFBHP = dt.Rows[dtrecord]["FBHP"].ToString();
            string tstLFACTOR = dt.Rows[dtrecord]["LFactor"].ToString();
            string tstPI = dt.Rows[dtrecord]["PI"].ToString();
            string tstRsrvPressure = dt.Rows[dtrecord]["ReserviorPressure"].ToString();
            string tstChokeDFactor = dt.Rows[dtrecord]["ChokeDFactor"].ToString();
            string ignore = "IgnoreValue";

            UserPrincipal userPrincipal = UserPrincipal.Current;
            string usrname = userPrincipal.DisplayName;
            string colvals =
                             tstqualitycode + ";" +
                             tsttuningmethod + ";" +
                             tsttuningstatus + ";" +
                             tsttuningmsg + ";" +
                             tsttubingpresure + ";" +
                             tsttubingtemperature + ";" +
                             tstDownholePressureGauge + ";" +
                             tstflowlinepressure + ";" +
                             tstseparatorpressure + ";" +
                             tstchokesize + ";" +
                             tstoil + ";" +
                             tstgasrate + ";" +
                             tstwater + ";" +
                             tstGOR + ";" +
                             tstWaterCut + ";" +
                             tstFBHP + ";" +
                             tstLFACTOR + ";" +
                             tstPI + ";" +
                             tstRsrvPressure + ";" +
                             tstChokeDFactor + ";" +
                             ignore + ";" +
                             usrname;
            CommonHelper.TraceLine("Expected Tuning method is " + tsttuningmethod);
            TelerikObject.verifyGridCellValues(colnames, colvals, rownum);
        }

        private void VerifyESPWellTestTableCellValuesMetric(DataTable dt, int dtrecord = 0, int rownum = 0)
        {




            //Verify the First Table Column
            string[] testdateonly = dt.Rows[0]["TestDate"].ToString().Split(new char[] { ';' });
            DateTime testdt = Convert.ToDateTime(testdateonly[0]);
            string modifiledttime = testdt.ToString("M/d/yy") + ", " + testdateonly[1];
            CommonHelper.TraceLine("Modified Value" + modifiledttime);
            TelerikObject.verifyGridCellValues("Test Date", "IgnoreValue;" + modifiledttime);
            string colnames = "Quality Code;Tuning Method;Status;Message;Tubing Head Pressure (kPa);Tubing Head Temperature (°C);Pump Intake Pressure (kPa);Pump Discharge Pressure (kPa);Frequency (Hz);Motor Volts (V);Motor Amps (A);Flow Line Pressure (kPa);Separator Pressure (kPa);Choke Size (mm);Oil Rate (sm³/d);Gas Rate (sm³/d);Water Rate (sm³/d);GOR (sm³/sm³);Water Cut (fraction);FBHP (kPa);L Factor;Productivity Index (sm3/d/kPa);Reservoir Pressure (kPa);Head Tuning Factor;Choke D Factor;Last Modified;Modified By";

            string tstqualitycode = dt.Rows[dtrecord]["QualityCode"].ToString();
            string tsttuningmethod = dt.Rows[dtrecord]["TuningMethod"].ToString();
            string tsttuningstatus = dt.Rows[dtrecord]["TuningStatus"].ToString();
            string tsttuningmsg = dt.Rows[dtrecord]["TuningMessage"].ToString();
            string tstoil = dt.Rows[dtrecord]["OilRateSTBd"].ToString();
            string tstwater = dt.Rows[dtrecord]["WaterRateSTBd"].ToString();
            string tstgasrate = dt.Rows[dtrecord]["GasRateMscfd"].ToString();
            string tsttubingpresure = dt.Rows[dtrecord]["TubingHeadPressurepsia"].ToString();
            string tsttubingtemperature = dt.Rows[dtrecord]["TubingHeadTemperatureF"].ToString();
            string tstesppip = dt.Rows[dtrecord]["PumpIntakePressure"].ToString();
            string tstesppdp = dt.Rows[dtrecord]["PumpDishcargePressure"].ToString();
            string tstfrequceny = dt.Rows[dtrecord]["Frequency"].ToString();
            string tstmotovolts = dt.Rows[dtrecord]["MotorVolts"].ToString();
            string tstmotoramps = dt.Rows[dtrecord]["MotorAmps"].ToString();
            string tstflowlinepressure = dt.Rows[dtrecord]["FlowLinePressure"].ToString();
            string tstseparatorpressure = dt.Rows[dtrecord]["SeparatorPressure"].ToString();
            string tstchokesize = dt.Rows[dtrecord]["ChokeSize"].ToString();
            string tstGOR = dt.Rows[dtrecord]["GOR"].ToString();
            string tstWaterCut = dt.Rows[dtrecord]["WaterCut"].ToString();
            string tstFBHP = dt.Rows[dtrecord]["FBHP"].ToString();
            string tstLFACTOR = dt.Rows[dtrecord]["LFactor"].ToString();
            string tstPI = dt.Rows[dtrecord]["PI"].ToString();
            string tstRsrvPressure = dt.Rows[dtrecord]["ReserviorPressure"].ToString();
            string tstRHeadFactor = dt.Rows[dtrecord]["HeadTuningFactor"].ToString();
            string tstChokeDFactor = dt.Rows[dtrecord]["ChokeDFactor"].ToString();
            string ignore = "IgnoreValue";


            //Convert to Metric for  Expected Values :
            tstoil = UnitsConversion("STB/d", Convert.ToDouble(tstoil)).ToString();
            tstwater = UnitsConversion("STB/d", Convert.ToDouble(tstwater)).ToString();
            tstgasrate = UnitsConversion("Mscf/d", Convert.ToDouble(tstgasrate)).ToString();
            tsttubingpresure = UnitsConversion("psia", Convert.ToDouble(tsttubingpresure)).ToString();
            tsttubingtemperature = UnitsConversion("F", Convert.ToDouble(tsttubingtemperature)).ToString();
            tstesppip = UnitsConversion("psia", Convert.ToDouble(tstesppip)).ToString();
            tstesppdp = UnitsConversion("psia", Convert.ToDouble(tstesppdp)).ToString();
            tstflowlinepressure = UnitsConversion("psia", Convert.ToDouble(tstflowlinepressure)).ToString();
            tstseparatorpressure = UnitsConversion("psia", Convert.ToDouble(tstseparatorpressure)).ToString();
            tstGOR = UnitsConversion("scf/STB", Convert.ToDouble(tstGOR)).ToString();
            tstFBHP = UnitsConversion("psia", Convert.ToDouble(tstFBHP)).ToString();
            tstPI = UnitsConversion("STB/d/psi", Convert.ToDouble(tstPI)).ToString();
            tstRsrvPressure = UnitsConversion("psia", Convert.ToDouble(tstRsrvPressure)).ToString();
            tstchokesize = UnitsConversion("1/64in", Convert.ToDouble(tstchokesize)).ToString();

            UserPrincipal userPrincipal = UserPrincipal.Current;
            string usrname = userPrincipal.DisplayName;
            string colvals =
                              tstqualitycode + ";" +
                              tsttuningmethod + ";" +
                              tsttuningstatus + ";" +
                              tsttuningmsg + ";" +
                              tsttubingpresure + ";" +
                              tsttubingtemperature + ";" +
                              tstesppip + ";" +
                              tstesppdp + ";" +
                              tstfrequceny + ";" +
                              tstmotovolts + ";" +
                              tstmotoramps + ";" +
                              tstflowlinepressure + ";" +
                              tstseparatorpressure + ";" +
                              tstchokesize + ";" +
                              tstoil + ";" +
                              tstgasrate + ";" +
                              tstwater + ";" +
                              tstGOR + ";" +
                              tstWaterCut + ";" +
                              tstFBHP + ";" +
                              tstLFACTOR + ";" +
                              tstPI + ";" +
                              tstRsrvPressure + ";" +
                              tstRHeadFactor + ";" +
                              tstChokeDFactor + ";" +
                              ignore + ";" +
                              usrname;



            TelerikObject.verifyGridCellValues(colnames, colvals, rownum);
        }

        private void VerifyGLWellTestTableCellValuesMetric(DataTable dt, int dtrecord = 0, int rownum = 0)
        {




            //Verify the First Table Column
            string[] testdateonly = dt.Rows[0]["TestDate"].ToString().Split(new char[] { ';' });
            DateTime testdt = Convert.ToDateTime(testdateonly[0]);
            string modifiledttime = testdt.ToString("M/d/yy") + ", " + testdateonly[1];
            CommonHelper.TraceLine("Modified Value" + modifiledttime);
            TelerikObject.verifyGridCellValues("Test Date", "IgnoreValue;" + modifiledttime);
            //string colnames = "Quality Code;Tuning Method;Status;Message;Tubing Head Pressure (kPa);Tubing Head Temperature (°C);Casing Head Pressure (kPa);Gas Injection Rate (sm³/d);Downhole Pressure Gauge (kPa);Flow Line Pressure (kPa);Separator Pressure (kPa);Choke Size (mm);Oil Rate (sm³/d);Gas Rate (sm³/d);Water Rate (sm³/d);Formation GOR (sm³/sm³);Water Cut (fraction);Injection GLR (sm³/sm³);FBHP (kPa);L Factor;Productivity Index (sm3/d/kPa);Reservoir Pressure (kPa);Choke D Factor;Orifice Size (mm);Last Modified;Modified By";
            string colnames = "Quality Code;Tuning Method;Status;Message;Tubing Head Pressure (kPa);Tubing Head Temperature (°C);Casing Head Pressure (kPa);Gas Injection Rate (sm³/d);Downhole Pressure Gauge (kPa);Flow Line Pressure (kPa);Separator Pressure (kPa);Choke Size (mm);Oil Rate (sm³/d);Gas Rate (sm³/d);Water Rate (sm³/d);Formation GOR (sm³/sm³);Water Cut (fraction);Injection GLR (sm³/sm³);FBHP (kPa);L Factor;Productivity Index (sm3/d/kPa);Reservoir Pressure (kPa);Choke D Factor;Last Modified;Modified By";

            string tstqualitycode = dt.Rows[dtrecord]["QualityCode"].ToString();
            string tsttuningmethod = dt.Rows[dtrecord]["TuningMethod"].ToString();
            string tsttuningstatus = dt.Rows[dtrecord]["TuningStatus"].ToString();
            string tsttuningmsg = dt.Rows[dtrecord]["TuningMessage"].ToString();
            string tstoil = dt.Rows[dtrecord]["OilRateSTBd"].ToString();
            string tstwater = dt.Rows[dtrecord]["WaterRateSTBd"].ToString();
            string tstgasrate = dt.Rows[dtrecord]["GasRateMscfd"].ToString();
            string tsttubingpresure = dt.Rows[dtrecord]["TubingHeadPressurepsia"].ToString();
            string tsttubingtemperature = dt.Rows[dtrecord]["TubingHeadTemperatureF"].ToString();
            string tstcasingheadpressure = dt.Rows[dtrecord]["CasingHeadPressure"].ToString();
            string tstGasInjectionRate = dt.Rows[dtrecord]["GasInjectionRate"].ToString();
            string tstDownholePressureGauge = dt.Rows[dtrecord]["DownholePressureGauge"].ToString();
            string tstflowlinepressure = dt.Rows[dtrecord]["FlowLinePressure"].ToString();
            string tstseparatorpressure = dt.Rows[dtrecord]["SeparatorPressure"].ToString();
            string tstchokesize = dt.Rows[dtrecord]["ChokeSize"].ToString();
            string tstFormationGOR = dt.Rows[dtrecord]["FormationGOR"].ToString();
            string tstWaterCut = dt.Rows[dtrecord]["WaterCut"].ToString();
            string tstInjectionGLR = dt.Rows[dtrecord]["InjectionGLR"].ToString();

            string tstFBHP = dt.Rows[dtrecord]["FBHP"].ToString();
            string tstLFACTOR = dt.Rows[dtrecord]["LFactor"].ToString();
            string tstPI = dt.Rows[dtrecord]["PI"].ToString();
            string tstRsrvPressure = dt.Rows[dtrecord]["ReserviorPressure"].ToString();
            string tstOrificeSize = dt.Rows[dtrecord]["OrificeSize"].ToString();
            string tstChokeDFactor = dt.Rows[dtrecord]["ChokeDFactor"].ToString();
            string ignore = "IgnoreValue";


            //Convert to Metric for  Expected Values :

            tstoil = UnitsConversion("STB/d", Convert.ToDouble(tstoil)).ToString();
            tstwater = UnitsConversion("STB/d", Convert.ToDouble(tstwater)).ToString();
            tstgasrate = UnitsConversion("Mscf/d", Convert.ToDouble(tstgasrate)).ToString();
            tsttubingpresure = UnitsConversion("psia", Convert.ToDouble(tsttubingpresure)).ToString();
            tsttubingtemperature = UnitsConversion("F", Convert.ToDouble(tsttubingtemperature)).ToString();
            tstcasingheadpressure = UnitsConversion("psia", Convert.ToDouble(tstcasingheadpressure)).ToString();
            tstGasInjectionRate = UnitsConversion("Mscf/d", Convert.ToDouble(tstGasInjectionRate)).ToString();
            //  tstflowlinepressure = UnitsConversion("psia", Convert.ToDouble(tstflowlinepressure)).ToString();
            //   tstseparatorpressure = UnitsConversion("psia", Convert.ToDouble(tstseparatorpressure)).ToString();
            tstFormationGOR = UnitsConversion("scf/STB", Convert.ToDouble(tstFormationGOR)).ToString();
            tstFBHP = UnitsConversion("psia", Convert.ToDouble(tstFBHP)).ToString();
            tstPI = UnitsConversion("STB/d/psi", Convert.ToDouble(tstPI)).ToString();
            tstRsrvPressure = UnitsConversion("psia", Convert.ToDouble(tstRsrvPressure)).ToString();
            //   tstchokesize = UnitsConversion("1/64in", Convert.ToDouble(tstchokesize)).ToString();
            tstInjectionGLR = UnitsConversion("scf/STB", Convert.ToDouble(tstInjectionGLR)).ToString();
            //   tstDownholePressureGauge = UnitsConversion("1/64in", Convert.ToDouble(tstDownholePressureGauge)).ToString();

            UserPrincipal userPrincipal = UserPrincipal.Current;
            string usrname = userPrincipal.DisplayName;
            string colvals =
                               tstqualitycode + ";" +
                             tsttuningmethod + ";" +
                             tsttuningstatus + ";" +
                             tsttuningmsg + ";" +
                             tsttubingpresure + ";" +
                             tsttubingtemperature + ";" +
                             tstcasingheadpressure + ";" +
                             tstGasInjectionRate + ";" +
                             tstDownholePressureGauge + ";" +
                             tstflowlinepressure + ";" +
                             tstseparatorpressure + ";" +
                             tstchokesize + ";" +
                             tstoil + ";" +
                             tstgasrate + ";" +
                             tstwater + ";" +
                             tstFormationGOR + ";" +
                             tstWaterCut + ";" +
                             tstInjectionGLR + ";" +
                             tstFBHP + ";" +
                             tstLFACTOR + ";" +
                             tstPI + ";" +
                             tstRsrvPressure + ";" +
                             tstChokeDFactor + ";" +
                             //        tstOrificeSize + ";" +
                             ignore + ";" +
                             usrname;



            TelerikObject.verifyGridCellValues(colnames, colvals, rownum);
        }
        private void VerifyNFWellTestTableCellValuesMetric(DataTable dt, int dtrecord = 0, int rownum = 0)
        {




            //Verify the First Table Column
            string[] testdateonly = dt.Rows[0]["TestDate"].ToString().Split(new char[] { ';' });
            DateTime testdt = Convert.ToDateTime(testdateonly[0]);
            string modifiledttime = testdt.ToString("M/d/yy") + ", " + testdateonly[1];
            CommonHelper.TraceLine("Modified Value" + modifiledttime);
            TelerikObject.verifyGridCellValues("Test Date", "IgnoreValue;" + modifiledttime);
            string colnames = "Quality Code;Tuning Method;Status;Message;Tubing Head Pressure (kPa);Tubing Head Temperature (°C);Downhole Pressure Gauge (kPa);Flow Line Pressure (kPa);Separator Pressure (kPa);Choke Size (mm);Oil Rate (sm³/d);Gas Rate (sm³/d);Water Rate (sm³/d);GOR (sm³/sm³);Water Cut (fraction);FBHP (kPa);L Factor;Productivity Index (sm3/d/kPa);Reservoir Pressure (kPa);Choke D Factor;Last Modified;Modified By";

            string tstqualitycode = dt.Rows[dtrecord]["QualityCode"].ToString();
            string tsttuningmethod = dt.Rows[dtrecord]["TuningMethod"].ToString();
            string tsttuningstatus = dt.Rows[dtrecord]["TuningStatus"].ToString();
            string tsttuningmsg = dt.Rows[dtrecord]["TuningMessage"].ToString();
            string tstoil = dt.Rows[dtrecord]["OilRateSTBd"].ToString();
            string tstwater = dt.Rows[dtrecord]["WaterRateSTBd"].ToString();
            string tstgasrate = dt.Rows[dtrecord]["GasRateMscfd"].ToString();
            string tsttubingpresure = dt.Rows[dtrecord]["TubingHeadPressurepsia"].ToString();
            string tsttubingtemperature = dt.Rows[dtrecord]["TubingHeadTemperatureF"].ToString();
            string tstDownholePressureGauge = dt.Rows[dtrecord]["DownholePressureGauge"].ToString();
            string tstflowlinepressure = dt.Rows[dtrecord]["FlowLinePressure"].ToString();
            string tstseparatorpressure = dt.Rows[dtrecord]["SeparatorPressure"].ToString();
            string tstchokesize = dt.Rows[dtrecord]["ChokeSize"].ToString();
            string tstGOR = dt.Rows[dtrecord]["GOR"].ToString();
            string tstWaterCut = dt.Rows[dtrecord]["WaterCut"].ToString();
            string tstFBHP = dt.Rows[dtrecord]["FBHP"].ToString();
            string tstLFACTOR = dt.Rows[dtrecord]["LFactor"].ToString();
            string tstPI = dt.Rows[dtrecord]["PI"].ToString();
            string tstRsrvPressure = dt.Rows[dtrecord]["ReserviorPressure"].ToString();
            string tstChokeDFactor = dt.Rows[dtrecord]["ChokeDFactor"].ToString();
            string ignore = "IgnoreValue";


            //Convert to Metric for  Expected Values :
            tstoil = UnitsConversion("STB/d", Convert.ToDouble(tstoil)).ToString();
            tstwater = UnitsConversion("STB/d", Convert.ToDouble(tstwater)).ToString();
            tstgasrate = UnitsConversion("Mscf/d", Convert.ToDouble(tstgasrate)).ToString();
            tsttubingpresure = UnitsConversion("psia", Convert.ToDouble(tsttubingpresure)).ToString();
            tsttubingtemperature = UnitsConversion("F", Convert.ToDouble(tsttubingtemperature)).ToString();
            //   tstDownholePressureGauge = UnitsConversion("psia", Convert.ToDouble(tstDownholePressureGauge)).ToString();
            //     tstflowlinepressure = UnitsConversion("psia", Convert.ToDouble(tstflowlinepressure)).ToString();
            //    tstseparatorpressure = UnitsConversion("psia", Convert.ToDouble(tstseparatorpressure)).ToString();
            tstGOR = UnitsConversion("scf/STB", Convert.ToDouble(tstGOR)).ToString();
            tstFBHP = UnitsConversion("psia", Convert.ToDouble(tstFBHP)).ToString();
            tstPI = UnitsConversion("STB/d/psi", Convert.ToDouble(tstPI)).ToString();
            tstRsrvPressure = UnitsConversion("psia", Convert.ToDouble(tstRsrvPressure)).ToString();
            //      tstchokesize = UnitsConversion("1/64in", Convert.ToDouble(tstchokesize)).ToString();

            UserPrincipal userPrincipal = UserPrincipal.Current;
            string usrname = userPrincipal.DisplayName;
            string colvals =
                              tstqualitycode + ";" +
                              tsttuningmethod + ";" +
                              tsttuningstatus + ";" +
                              tsttuningmsg + ";" +
                              tsttubingpresure + ";" +
                              tsttubingtemperature + ";" +
                              tstDownholePressureGauge + ";" +
                              tstflowlinepressure + ";" +
                              tstseparatorpressure + ";" +
                              tstchokesize + ";" +
                              tstoil + ";" +
                              tstgasrate + ";" +
                              tstwater + ";" +
                              tstGOR + ";" +
                              tstWaterCut + ";" +
                              tstFBHP + ";" +
                              tstLFACTOR + ";" +
                              tstPI + ";" +
                              tstRsrvPressure + ";" +
                              tstChokeDFactor + ";" +
                              ignore + ";" +
                              usrname;



            TelerikObject.verifyGridCellValues(colnames, colvals, rownum);
        }



        private void UnitlabelVerification(HtmlControl ctl, string ctlname, string exptext)
        {
            HtmlControl Divctl = TelerikObject.GetChildrenControl(ctl, "0;1");
            CommonHelper.TraceLine(string.Format("Unit for {0} is: {1}", ctlname, Divctl.BaseElement.InnerText));
            Assert.AreEqual(exptext, Divctl.BaseElement.InnerText, "Mismatch of Unit label for " + ctlname);
        }

        private void Toastercheck(HtmlControl ctl, string msg_scenario, string exp)
        {
            Assert.IsNotNull(ctl, "Toaster Message for " + msg_scenario);
            CommonHelper.TraceLine(string.Format("Toaster Message Obtained during {0}  is : {1}", msg_scenario, ctl.BaseElement.InnerText.ToString().Trim()));
            Assert.AreEqual(exp, ctl.BaseElement.InnerText.ToString().Trim(), "Toaster Message for " + msg_scenario + " is Wrong.");
        }

        private void RRLQuickEditWellTestGrid(string olddatafile, string newdatafile)
        {
            string testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\TestData", "RRLWellTestData.xml");
            CommonHelper.TraceLine("Getting Data from XML file ... " + testdatafile);
            DataTable dtprev = CommonHelper.BuildDataTableFromXml(testdatafile);
            //*** Enter New Values to Edit in Grid
            string testdatafilenew = Path.Combine(FilesLocation + @"\UIAutomation\TestData", "RRLWellTestDataQE.xml");
            CommonHelper.TraceLine("Getting Data from XML file ... " + testdatafilenew);
            DataTable dt = CommonHelper.BuildDataTableFromXml(testdatafilenew);
            string oldtstduration = dtprev.Rows[0]["TestDurationHours"].ToString();
            string oldtstqualitycode = dtprev.Rows[0]["QualityCode"].ToString();
            string oldtstoil = dtprev.Rows[0]["OilRateSTBd"].ToString();
            string oldtstwater = dtprev.Rows[0]["WaterRateSTBd"].ToString();
            string oldtstgasrate = dtprev.Rows[0]["GasRateMscfd"].ToString();
            string oldtstoilapi = dtprev.Rows[0]["OilGravityAPI"].ToString();
            string oldtstwatergravity = dtprev.Rows[0]["WaterGravitySG"].ToString();
            string oldtstgasgravity = dtprev.Rows[0]["GasGravitySG"].ToString();
            string oldtsttubingpresure = dtprev.Rows[0]["AverageTubingPressurepsia"].ToString();
            string oldtsttubingtemperature = dtprev.Rows[0]["AverageTubingTemperatureF"].ToString();
            string oldtstcaingpressure = dtprev.Rows[0]["AverageCasingPressurepsia"].ToString();
            string oldtstfap = dtprev.Rows[0]["AverageFluidAbovePumpft"].ToString();
            string oldtstpip = dtprev.Rows[0]["PumpIntakePressure"].ToString();
            string oldtstspm = dtprev.Rows[0]["StrokesPerMinute"].ToString();
            string oldtstpmphrs = dtprev.Rows[0]["PumpingHoursHours"].ToString();
            string oldtstpumpeffcy = dtprev.Rows[0]["PumpEfficiency"].ToString();
            string ignore = "IgnoreValue";
            UserPrincipal userPrincipal = UserPrincipal.Current;
            string usrname = userPrincipal.DisplayName;
            string colvalsold = oldtstduration + ";" +
                             oldtstqualitycode + ";" +
                             oldtstoil + ";" +
                             oldtstwater + ";" +
                             oldtstgasrate + ";" +
                             oldtstoilapi + ";" +
                             oldtstwatergravity + ";" +
                             oldtstgasgravity + ";" +
                             oldtsttubingpresure + ";" +
                             oldtsttubingtemperature + ";" +
                             oldtstcaingpressure + ";" +
                             oldtstfap + ";" +
                             oldtstpip + ";" +
                             oldtstspm + ";" +
                             oldtstpmphrs + ";" +
                             oldtstpumpeffcy + ";" +
                             ignore + ";" +
                             ignore + ";" +
                             ignore + ";" +
                             ignore;

            // Get New Values to enter in Grid Cell;
            string colnames = "Test Duration (Hours);Quality Code;Oil Rate (STB/d);Water Rate (STB/d);Gas Rate (Mscf/d);Oil Gravity (°API);Water Gravity (SG);Gas Gravity (SG);Average Tubing Pressure (psia);Average Tubing Temperature (°F);Average Casing Pressure (psia);Average Fluid Above Pump (ft);Pump Intake Pressure (psia);Strokes Per Minute (SPM);Pumping Hours (Hours);Pump Efficiency (%);Id;Assembly;Last Modified;Modified By";
            string tstduration = dt.Rows[0]["TestDurationHours"].ToString();
            string tstqualitycode = dt.Rows[0]["QualityCode"].ToString();
            string tstoil = dt.Rows[0]["OilRateSTBd"].ToString();
            string tstwater = dt.Rows[0]["WaterRateSTBd"].ToString();
            string tstgasrate = dt.Rows[0]["GasRateMscfd"].ToString();
            string tstoilapi = dt.Rows[0]["OilGravityAPI"].ToString();
            string tstwatergravity = dt.Rows[0]["WaterGravitySG"].ToString();
            string tstgasgravity = dt.Rows[0]["GasGravitySG"].ToString();
            string tsttubingpresure = dt.Rows[0]["AverageTubingPressurepsia"].ToString();
            string tsttubingtemperature = dt.Rows[0]["AverageTubingTemperatureF"].ToString();
            string tstcaingpressure = dt.Rows[0]["AverageCasingPressurepsia"].ToString();
            string tstfap = dt.Rows[0]["AverageFluidAbovePumpft"].ToString();
            string tstpip = dt.Rows[0]["PumpIntakePressure"].ToString();
            string tstspm = dt.Rows[0]["StrokesPerMinute"].ToString();
            string tstpmphrs = dt.Rows[0]["PumpingHoursHours"].ToString();
            string tstpumpeffcy = dt.Rows[0]["PumpEfficiency"].ToString();
            string colvalsnew = tstduration + ";" +
                             tstqualitycode + "|KendoDD|;" +
                             tstoil + ";" +
                             tstwater + ";" +
                             tstgasrate + ";" +
                             tstoilapi + ";" +
                             tstwatergravity + ";" +
                             tstgasgravity + ";" +
                             tsttubingpresure + ";" +
                             tsttubingtemperature + ";" +
                             tstcaingpressure + ";" +
                             tstfap + ";" +
                             tstpip + ";" +
                             tstspm + ";" +
                             tstpmphrs + ";" +
                             tstpumpeffcy + ";" +
                             ignore + ";" +
                             ignore + ";" +
                             ignore + ";" +
                             ignore;



            TelerikObject.editGridCells(colnames, colvalsold, colvalsnew, 0);
        }

        private void ESPQuickEditWellTestGrid(string olddatafile, string newdatafile)
        {
            Thread.Sleep(5000);
            string testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\TestData", "ESPWellTestData.xml");
            CommonHelper.TraceLine("Getting Data from XML file ... " + testdatafile);
            DataTable dtprev = CommonHelper.BuildDataTableFromXml(testdatafile);
            //*** Enter New Values to Edit in Grid
            string testdatafilenew = Path.Combine(FilesLocation + @"\UIAutomation\TestData", "ESPWellTestDataQE.xml");
            CommonHelper.TraceLine("Getting Data from XML file ... " + testdatafilenew);
            DataTable dt = CommonHelper.BuildDataTableFromXml(testdatafilenew);
            string ignore = "IgnoreValue";
            UserPrincipal userPrincipal = UserPrincipal.Current;
            string usrname = userPrincipal.DisplayName;
            string oldtstqualitycode = dtprev.Rows[0]["QualityCode"].ToString();
            string oldtsttuningmethod = dtprev.Rows[0]["TuningMethod"].ToString();
            string oldtsttuningstatus = dtprev.Rows[0]["TuningStatus"].ToString();
            string oldtsttuningmsg = dtprev.Rows[0]["TuningMessage"].ToString();
            string oldtstoil = dtprev.Rows[0]["OilRateSTBd"].ToString();
            string oldtstwater = dtprev.Rows[0]["WaterRateSTBd"].ToString();
            string oldtstgasrate = dtprev.Rows[0]["GasRateMscfd"].ToString();
            string oldtsttubingpresure = dtprev.Rows[0]["TubingHeadPressurepsia"].ToString();
            string oldtsttubingtemperature = dtprev.Rows[0]["TubingHeadTemperatureF"].ToString();
            string oldtstesppip = dtprev.Rows[0]["PumpIntakePressure"].ToString();
            string oldtstesppdp = dtprev.Rows[0]["PumpDishcargePressure"].ToString();
            string oldtstfrequceny = dtprev.Rows[0]["Frequency"].ToString();
            string oldtstmotovolts = dtprev.Rows[0]["MotorVolts"].ToString();
            string oldtstmotoramps = dtprev.Rows[0]["MotorAmps"].ToString();
            string oldtstflowlinepressure = dtprev.Rows[0]["FlowLinePressure"].ToString();
            string oldtstseparatorpressure = dtprev.Rows[0]["SeparatorPressure"].ToString();
            string oldtstchokesize = dtprev.Rows[0]["ChokeSize"].ToString();
            string oldtstGOR = dtprev.Rows[0]["GOR"].ToString();
            string oldtstWaterCut = dtprev.Rows[0]["WaterCut"].ToString();
            string oldtstFBHP = dtprev.Rows[0]["FBHP"].ToString();
            string oldtstLFACTOR = dtprev.Rows[0]["LFactor"].ToString();
            string oldtstPI = dtprev.Rows[0]["PI"].ToString();
            string oldtstRsrvPressure = dtprev.Rows[0]["ReserviorPressure"].ToString();
            string oldtstRHeadFactor = dtprev.Rows[0]["HeadTuningFactor"].ToString();
            string oldtstChokeDFactor = dtprev.Rows[0]["ChokeDFactor"].ToString();


            string colvalsold =
                             oldtstqualitycode + ";" +
                             ignore + ";" +
                             ignore + ";" +
                             ignore + ";" +
                             oldtsttuningmethod + ";" +
                             oldtsttuningstatus + ";" +
                             oldtsttubingpresure + ";" +
                             oldtsttubingtemperature + ";" +
                             oldtstesppip + ";" +
                             oldtstesppdp + ";" +
                             oldtstfrequceny + ";" +
                             oldtstmotovolts + ";" +
                             oldtstmotoramps + ";" +
                             oldtstflowlinepressure + ";" +
                             oldtstseparatorpressure + ";" +
                             oldtstchokesize + ";" +
                             oldtstoil + ";" +
                             oldtstgasrate + ";" +
                             oldtstwater + ";" +
                            ignore + ";" +
                            ignore + ";" +
                            ignore + ";" +
                            ignore + ";" +
                            ignore + ";" +
                            ignore + ";" +
                            ignore + ";" +
                            ignore + ";" +
                            ignore + ";" +
                            ignore + ";";

            // Get New Values to enter in Grid Cell;
            string colnames = "Quality Code;Tuning Method;Status;Message;Tubing Head Pressure (psia);Tubing Head Temperature (°F);Pump Intake Pressure (psia);Pump Discharge Pressure (psia);Frequency (Hz);Motor Volts (V);Motor Amps (A);Flow Line Pressure (psia);Separator Pressure (psia);Choke Size (64th in);Oil Rate (STB/d);Gas Rate (Mscf/d);Water Rate (STB/d);GOR (scf/STB);Water Cut (fraction);FBHP (psia);L Factor;Productivity Index (STB/d/psi);Reservoir Pressure (psia);Head Tuning Factor;Choke D Factor;Last Modified;Modified By";
            //    string colnames = "Quality Code;Tubing Head Pressure (psia);Tubing Head Temperature (°F);Pump Intake Pressure (psia);Pump Discharge Pressure (psia);Frequency (Hz);Motor Volts (V);Motor Amps (A);Flow Line Pressure (psia);Separator Pressure (psia);Choke Size (64th in);Oil Rate (STB/d);Gas Rate (Mscf/d);Water Rate (STB/d);";
            string tstqualitycode = dt.Rows[0]["QualityCode"].ToString();
            string tsttuningmethod = dt.Rows[0]["TuningMethod"].ToString();
            string tsttuningstatus = dt.Rows[0]["TuningStatus"].ToString();
            string tsttuningmsg = dt.Rows[0]["TuningMessage"].ToString();
            string tstoil = dt.Rows[0]["OilRateSTBd"].ToString();
            string tstwater = dt.Rows[0]["WaterRateSTBd"].ToString();
            string tstgasrate = dt.Rows[0]["GasRateMscfd"].ToString();
            string tsttubingpresure = dt.Rows[0]["TubingHeadPressurepsia"].ToString();
            string tsttubingtemperature = dt.Rows[0]["TubingHeadTemperatureF"].ToString();
            string tstesppip = dt.Rows[0]["PumpIntakePressure"].ToString();
            string tstesppdp = dt.Rows[0]["PumpDishcargePressure"].ToString();
            string tstfrequceny = dt.Rows[0]["Frequency"].ToString();
            string tstmotovolts = dt.Rows[0]["MotorVolts"].ToString();
            string tstmotoramps = dt.Rows[0]["MotorAmps"].ToString();
            string tstflowlinepressure = dt.Rows[0]["FlowLinePressure"].ToString();
            string tstseparatorpressure = dt.Rows[0]["SeparatorPressure"].ToString();
            string tstchokesize = dt.Rows[0]["ChokeSize"].ToString();
            string tstGOR = dt.Rows[0]["GOR"].ToString();
            string tstWaterCut = dt.Rows[0]["WaterCut"].ToString();
            string tstFBHP = dt.Rows[0]["FBHP"].ToString();
            string tstLFACTOR = dt.Rows[0]["LFactor"].ToString();
            string tstPI = dt.Rows[0]["PI"].ToString();
            string tstRsrvPressure = dt.Rows[0]["ReserviorPressure"].ToString();
            string tstRHeadFactor = dt.Rows[0]["HeadTuningFactor"].ToString();
            string tstChokeDFactor = dt.Rows[0]["ChokeDFactor"].ToString();


            string colvalsnew =

                             tstqualitycode + "|KendoDD|;" +
                             ignore + ";" +
                             ignore + ";" +
                             ignore + ";" +
                             tsttubingpresure + ";" +
                             ignore + ";" +
                             ignore + ";" +
                             ignore + ";" +
                             tstfrequceny + ";" +
                             ignore + ";" +
                             ignore + ";" +
                             ignore + ";" +
                             ignore + ";" +
                             ignore + ";" +
                             tstoil + ";" +
                             tstgasrate + ";" +
                             tstwater + ";" +
                             ignore + ";" +
                             ignore + ";" +
                             ignore + ";" +
                             ignore + ";" +
                             ignore + ";" +
                             ignore + ";" +
                             ignore + ";" +
                             ignore + ";" +
                             ignore + ";" +
                             ignore + ";"
                             ;



            TelerikObject.editGridCells(colnames, colvalsold, colvalsnew, 0);
        }

        private void GLQuickEditWellTestGrid(string olddatafile, string newdatafile)
        {
            Thread.Sleep(5000);
            string testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\TestData", "GLWellTestData.xml");
            CommonHelper.TraceLine("Getting Data from XML file ... " + testdatafile);
            DataTable dtprev = CommonHelper.BuildDataTableFromXml(testdatafile);
            //*** Enter New Values to Edit in Grid
            string testdatafilenew = Path.Combine(FilesLocation + @"\UIAutomation\TestData", "GLWellTestDataQE.xml");
            CommonHelper.TraceLine("Getting Data from XML file ... " + testdatafilenew);
            DataTable dt = CommonHelper.BuildDataTableFromXml(testdatafilenew);
            string ignore = "IgnoreValue";
            UserPrincipal userPrincipal = UserPrincipal.Current;
            string usrname = userPrincipal.DisplayName;
            int dtrecord = 0;
            string oldtstqualitycode = dtprev.Rows[dtrecord]["QualityCode"].ToString();
            string oldtsttuningmethod = dtprev.Rows[dtrecord]["TuningMethod"].ToString();
            string oldtsttuningstatus = dtprev.Rows[dtrecord]["TuningStatus"].ToString();
            string oldtsttuningmsg = dtprev.Rows[dtrecord]["TuningMessage"].ToString();
            string oldtstoil = dtprev.Rows[dtrecord]["OilRateSTBd"].ToString();
            string oldtstwater = dtprev.Rows[dtrecord]["WaterRateSTBd"].ToString();
            string oldtstgasrate = dtprev.Rows[dtrecord]["GasRateMscfd"].ToString();
            string oldtsttubingpresure = dtprev.Rows[dtrecord]["TubingHeadPressurepsia"].ToString();
            string oldtsttubingtemperature = dtprev.Rows[dtrecord]["TubingHeadTemperatureF"].ToString();
            string oldtstcasingheadpressure = dtprev.Rows[dtrecord]["CasingHeadPressure"].ToString();
            string oldtstGasInjectionRate = dtprev.Rows[dtrecord]["GasInjectionRate"].ToString();
            string oldtstDownholePressureGauge = dtprev.Rows[dtrecord]["DownholePressureGauge"].ToString();
            string oldtstflowlinepressure = dtprev.Rows[dtrecord]["FlowLinePressure"].ToString();
            string oldtstseparatorpressure = dtprev.Rows[dtrecord]["SeparatorPressure"].ToString();
            string oldtstchokesize = dtprev.Rows[dtrecord]["ChokeSize"].ToString();
            string oldtstFormationGOR = dtprev.Rows[dtrecord]["FormationGOR"].ToString();
            string oldtstWaterCut = dtprev.Rows[dtrecord]["WaterCut"].ToString();
            string oldtstInjectionGLR = dtprev.Rows[dtrecord]["InjectionGLR"].ToString();

            string oldtstFBHP = dtprev.Rows[dtrecord]["FBHP"].ToString();
            string oldtstLFACTOR = dtprev.Rows[dtrecord]["LFactor"].ToString();
            string oldtstPI = dtprev.Rows[dtrecord]["PI"].ToString();
            string oldtstRsrvPressure = dtprev.Rows[dtrecord]["ReserviorPressure"].ToString();
            string oldtstOrificeSize = dtprev.Rows[dtrecord]["OrificeSize"].ToString();
            string oldtstChokeDFactor = dtprev.Rows[dtrecord]["ChokeDFactor"].ToString();


            string colvalsold =
                             oldtstqualitycode + ";";
            // ignore + ";" +
            // ignore + ";" +
            // ignore + ";" +
            // oldtsttuningmethod + ";" +
            // oldtsttuningstatus + ";" +
            // oldtsttubingpresure + ";" +
            // oldtsttubingtemperature + ";" +
            // oldtstcasingheadpressure + ";" +
            // oldtstGasInjectionRate + ";" +
            // oldtstDownholePressureGauge + ";" +
            // oldtstflowlinepressure + ";" +
            // oldtstseparatorpressure + ";" +
            // oldtstchokesize + ";" +
            // oldtstoil + ";" +
            // oldtstgasrate + ";" +
            // oldtstwater + ";" +
            //ignore + ";" +
            //ignore + ";" +
            //ignore + ";" +
            //ignore + ";" +
            //ignore + ";" +
            //ignore + ";" +
            //ignore + ";" +
            //ignore + ";" +
            //ignore + ";" +
            //ignore + ";";


            // Get New Values to enter in Grid Cell;
            //string colnames = "Quality Code;Tuning Method;Status;Message;Tubing Head Pressure (psia);Tubing Head Temperature (°F);Casing Head Pressure (psia);Gas Injection Rate (Mscf/d);Downhole Pressure Gauge (psia);Flow Line Pressure (psia);Separator Pressure (psia);Choke Size (64th in);Oil Rate (STB/d);Gas Rate (Mscf/d);Water Rate (STB/d);Formation GOR (scf/STB);Water Cut (fraction);Injection GLR (scf/STB);FBHP (psia);L Factor;Productivity Index (STB/d/psi);Reservoir Pressure (psia);Choke D Factor;Orifice Size (64th in);Last Modified;Modified By";
            string colnames = "Quality Code;Tuning Method;Status;Message;Tubing Head Pressure (psia);Tubing Head Temperature (°F);Casing Head Pressure (psia);Gas Injection Rate (Mscf/d);Downhole Pressure Gauge (psia);Flow Line Pressure (psia);Separator Pressure (psia);Choke Size (64th in);Oil Rate (STB/d);Gas Rate (Mscf/d);Water Rate (STB/d);Formation GOR (scf/STB);Water Cut (fraction);Injection GLR (scf/STB);FBHP (psia);L Factor;Productivity Index (STB/d/psi);Reservoir Pressure (psia);Choke D Factor;Last Modified;Modified By";
            //    string colnames = "Quality Code;Tubing Head Pressure (psia);Tubing Head Temperature (°F);Pump Intake Pressure (psia);Pump Discharge Pressure (psia);Frequency (Hz);Motor Volts (V);Motor Amps (A);Flow Line Pressure (psia);Separator Pressure (psia);Choke Size (64th in);Oil Rate (STB/d);Gas Rate (Mscf/d);Water Rate (STB/d);";
            string tstqualitycode = dt.Rows[dtrecord]["QualityCode"].ToString();
            string tsttuningmethod = dt.Rows[dtrecord]["TuningMethod"].ToString();
            string tsttuningstatus = dt.Rows[dtrecord]["TuningStatus"].ToString();
            string tsttuningmsg = dt.Rows[dtrecord]["TuningMessage"].ToString();
            string tstoil = dt.Rows[dtrecord]["OilRateSTBd"].ToString();
            string tstwater = dt.Rows[dtrecord]["WaterRateSTBd"].ToString();
            string tstgasrate = dt.Rows[dtrecord]["GasRateMscfd"].ToString();
            string tsttubingpresure = dt.Rows[dtrecord]["TubingHeadPressurepsia"].ToString();
            string tsttubingtemperature = dt.Rows[dtrecord]["TubingHeadTemperatureF"].ToString();
            string tstcasingheadpressure = dt.Rows[dtrecord]["CasingHeadPressure"].ToString();
            string tstGasInjectionRate = dt.Rows[dtrecord]["GasInjectionRate"].ToString();
            string tstDownholePressureGauge = dt.Rows[dtrecord]["DownholePressureGauge"].ToString();
            string tstflowlinepressure = dt.Rows[dtrecord]["FlowLinePressure"].ToString();
            string tstseparatorpressure = dt.Rows[dtrecord]["SeparatorPressure"].ToString();
            string tstchokesize = dt.Rows[dtrecord]["ChokeSize"].ToString();
            string tstFormationGOR = dt.Rows[dtrecord]["FormationGOR"].ToString();
            string tstWaterCut = dt.Rows[dtrecord]["WaterCut"].ToString();
            string tstInjectionGLR = dt.Rows[dtrecord]["InjectionGLR"].ToString();

            string tstFBHP = dt.Rows[dtrecord]["FBHP"].ToString();
            string tstLFACTOR = dt.Rows[dtrecord]["LFactor"].ToString();
            string tstPI = dt.Rows[dtrecord]["PI"].ToString();
            string tstRsrvPressure = dt.Rows[dtrecord]["ReserviorPressure"].ToString();
            string tstOrificeSize = dt.Rows[dtrecord]["OrificeSize"].ToString();
            string tstChokeDFactor = dt.Rows[dtrecord]["ChokeDFactor"].ToString();



            string colvalsnew =

                             tstqualitycode + "|KendoDD|;" +
                             ignore + ";" +
                             ignore + ";" +
                             ignore + ";" +
                             tsttubingpresure + ";" +
                             ignore + ";" +
                             ignore + ";" +
                             ignore + ";" +
                             ignore + ";" +
                             ignore + ";" +
                             ignore + ";" +
                             ignore + ";" +
                             tstoil + ";" +
                             tstgasrate + ";" +
                             tstwater + ";" +
                             ignore + ";" +
                             ignore + ";" +
                             ignore + ";" +
                             ignore + ";" +
                             ignore + ";" +
                             ignore + ";" +
                             ignore + ";" +
                             ignore + ";" +
                             ignore + ";" +
                             ignore + ";" +
                             ignore + ";"
                             ;



            TelerikObject.editGridCells(colnames, colvalsold, colvalsnew, 0);
        }

        private void NFQuickEditWellTestGrid(string olddatafile, string newdatafile)
        {
            Thread.Sleep(5000);
            string testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\TestData", "NFWellTestData.xml");
            CommonHelper.TraceLine("Getting Data from XML file ... " + testdatafile);
            DataTable dtprev = CommonHelper.BuildDataTableFromXml(testdatafile);
            //*** Enter New Values to Edit in Grid
            string testdatafilenew = Path.Combine(FilesLocation + @"\UIAutomation\TestData", "NFWellTestDataQE.xml");
            CommonHelper.TraceLine("Getting Data from XML file ... " + testdatafilenew);
            DataTable dt = CommonHelper.BuildDataTableFromXml(testdatafilenew);
            string ignore = "IgnoreValue";
            UserPrincipal userPrincipal = UserPrincipal.Current;
            string usrname = userPrincipal.DisplayName;
            string oldtstqualitycode = dtprev.Rows[0]["QualityCode"].ToString();
            string oldtsttuningmethod = dtprev.Rows[0]["TuningMethod"].ToString();
            string oldtsttuningstatus = dtprev.Rows[0]["TuningStatus"].ToString();
            string oldtsttuningmsg = dtprev.Rows[0]["TuningMessage"].ToString();
            string oldtstoil = dtprev.Rows[0]["OilRateSTBd"].ToString();
            string oldtstwater = dtprev.Rows[0]["WaterRateSTBd"].ToString();
            string oldtstgasrate = dtprev.Rows[0]["GasRateMscfd"].ToString();
            string oldtsttubingpresure = dtprev.Rows[0]["TubingHeadPressurepsia"].ToString();
            string oldtsttubingtemperature = dtprev.Rows[0]["TubingHeadTemperatureF"].ToString();
            string oldtstDownholePressureGauge = dtprev.Rows[0]["DownholePressureGauge"].ToString();
            string oldtstflowlinepressure = dtprev.Rows[0]["FlowLinePressure"].ToString();
            string oldtstseparatorpressure = dtprev.Rows[0]["SeparatorPressure"].ToString();
            string oldtstchokesize = dtprev.Rows[0]["ChokeSize"].ToString();
            string oldtstGOR = dtprev.Rows[0]["GOR"].ToString();
            string oldtstWaterCut = dtprev.Rows[0]["WaterCut"].ToString();
            string oldtstFBHP = dtprev.Rows[0]["FBHP"].ToString();
            string oldtstLFACTOR = dtprev.Rows[0]["LFactor"].ToString();
            string oldtstPI = dtprev.Rows[0]["PI"].ToString();
            string oldtstRsrvPressure = dtprev.Rows[0]["ReserviorPressure"].ToString();
            string oldtstChokeDFactor = dtprev.Rows[0]["ChokeDFactor"].ToString();


            string colvalsold =
                             oldtstqualitycode + ";" +
                             ignore + ";" +
                             ignore + ";" +
                             ignore + ";" +
                             oldtsttuningmethod + ";" +
                             oldtsttuningstatus + ";" +
                             oldtsttubingpresure + ";" +
                             oldtsttubingtemperature + ";" +
                             oldtstflowlinepressure + ";" +
                             oldtstseparatorpressure + ";" +
                             oldtstchokesize + ";" +
                             oldtstoil + ";" +
                             oldtstgasrate + ";" +
                             oldtstwater + ";" +
                            ignore + ";" +
                            ignore + ";" +
                            ignore + ";" +
                            ignore + ";" +
                            ignore + ";" +
                            ignore + ";" +
                            ignore + ";" +
                            ignore + ";";

            // Get New Values to enter in Grid Cell;
            string colnames = "Quality Code;Tuning Method;Status;Message;Tubing Head Pressure (psia);Tubing Head Temperature (°F);Downhole Pressure Gauge (psia);Flow Line Pressure (psia);Separator Pressure (psia);Choke Size (64th in);Oil Rate (STB/d);Gas Rate (Mscf/d);Water Rate (STB/d);GOR (scf/STB);Water Cut (fraction);FBHP (psia);L Factor;Productivity Index (STB/d/psi);Reservoir Pressure (psia);Choke D Factor;Last Modified;Modified By";
            //    string colnames = "Quality Code;Tubing Head Pressure (psia);Tubing Head Temperature (°F);Pump Intake Pressure (psia);Pump Discharge Pressure (psia);Frequency (Hz);Motor Volts (V);Motor Amps (A);Flow Line Pressure (psia);Separator Pressure (psia);Choke Size (64th in);Oil Rate (STB/d);Gas Rate (Mscf/d);Water Rate (STB/d);";
            string tstqualitycode = dt.Rows[0]["QualityCode"].ToString();
            string tsttuningmethod = dt.Rows[0]["TuningMethod"].ToString();
            string tsttuningstatus = dt.Rows[0]["TuningStatus"].ToString();
            string tsttuningmsg = dt.Rows[0]["TuningMessage"].ToString();
            string tstoil = dt.Rows[0]["OilRateSTBd"].ToString();
            string tstwater = dt.Rows[0]["WaterRateSTBd"].ToString();
            string tstgasrate = dt.Rows[0]["GasRateMscfd"].ToString();
            string tsttubingpresure = dt.Rows[0]["TubingHeadPressurepsia"].ToString();
            string tsttubingtemperature = dt.Rows[0]["TubingHeadTemperatureF"].ToString();
            string tstDownholePressureGauge = dt.Rows[0]["DownholePressureGauge"].ToString();
            string tstflowlinepressure = dt.Rows[0]["FlowLinePressure"].ToString();
            string tstseparatorpressure = dt.Rows[0]["SeparatorPressure"].ToString();
            string tstchokesize = dt.Rows[0]["ChokeSize"].ToString();
            string tstGOR = dt.Rows[0]["GOR"].ToString();
            string tstWaterCut = dt.Rows[0]["WaterCut"].ToString();
            string tstFBHP = dt.Rows[0]["FBHP"].ToString();
            string tstLFACTOR = dt.Rows[0]["LFactor"].ToString();
            string tstPI = dt.Rows[0]["PI"].ToString();
            string tstRsrvPressure = dt.Rows[0]["ReserviorPressure"].ToString();
            string tstChokeDFactor = dt.Rows[0]["ChokeDFactor"].ToString();


            string colvalsnew =

                             tstqualitycode + "|KendoDD|;" +
                             ignore + ";" +
                             ignore + ";" +
                             ignore + ";" +
                             tsttubingpresure + ";" +
                             ignore + ";" +
                             ignore + ";" +
                             ignore + ";" +
                             ignore + ";" +
                             ignore + ";" +
                             tstoil + ";" +
                             tstgasrate + ";" +
                             tstwater + ";" +
                             ignore + ";" +
                             ignore + ";" +
                             ignore + ";" +
                             ignore + ";" +
                             ignore + ";" +
                             ignore + ";" +
                             ignore + ";" +
                             ignore + ";" +
                             ignore + ";"
                             ;



            TelerikObject.editGridCells(colnames, colvalsold, colvalsnew, 0);
        }





        private void NavigateBackForthWellTest()
        {
            TelerikObject.Click(PageDashboard.pedashboardtab);
            TelerikObject.WaitForControlDisappear(new HtmlControl(PageDashboard.page_Loader));
            TelerikObject.Click(PageDashboard.welltesttab);
            TelerikObject.WaitForControlDisappear(new HtmlControl(PageDashboard.page_Loader));
        }

        public double? UnitsConversion(string unitKey, double? value)
        {
            double? Metricvalue = null;


            switch (unitKey)
            {
                case "ft"://to m
                    Metricvalue = value * 0.3048;
                    break;

                case "psia":// to kPa
                    Metricvalue = value * 6.894757;
                    break;

                case "F":// to C
                    Metricvalue = (value - 32) * 0.55555555555555555555555555555556;
                    break;

                case "Mscf/d":// to sm3/d
                    Metricvalue = value * 28.3168466;
                    break;

                case "STB/d":// to sm3/d
                    Metricvalue = value * 0.1589873;
                    break;

                case "STB/d/psi":// to sm3/d/kPa
                    Metricvalue = value * 0.0230591593;
                    break;

                case "scf/STB":// to sm3/sm3
                    Metricvalue = value * 0.1781076;
                    break;

                case "1/64in":// to mm
                    Metricvalue = value * 0.396875;
                    break;

                case "API":// to sg
                    Metricvalue = (141.5) / (value + 131.5);
                    break;
            }
            return Metricvalue;
        }


        private void toggleSurviellanceTracking()
        {
            TelerikObject.Click(PageDashboard.Surveillancetab);
            TelerikObject.Click(PageDashboard.trackingtab);
        }
        private void TuneWellTestESP()
        {
            GetWellTestDateTime("ESPWellTestData.xml");
            HtmlControl PrtwelltestCheckBox = TelerikObject.GetPreviousSibling(PageWellTest.cell_testdata, 1);
            HtmlControl welltestCheckBox = TelerikObject.GetChildrenControl(PrtwelltestCheckBox, "0");
            Assert.IsNotNull(welltestCheckBox, "Check Box control was null");
            //   TelerikObject.CheckBox_Check(welltestCheckBox.BaseElement);
            TelerikObject.Click(welltestCheckBox.BaseElement);
            TelerikObject.Click(PageWellTest.btnTuneWellTest);
            Toastercheck(PageWellTest.toasterControl, "ESP Well Test Tuning", "Failed to tune one or more well tests.");

        }

        public void AddGLWellTest(string filename)
        {
            TelerikObject.WaitForControlDisappear(new HtmlControl(PageDashboard.page_Loader));
            TelerikObject.Click(PageWellTest.btnCreateNewWellTest);
            AddNewGLWellTest(filename);
            Toastercheck(PageWellTest.toasterControl, "Add Well Test", "Saved successfully.");
            Thread.Sleep(5000);
        }
        //
        public void AddESPWellTest(string filename)
        {
            TelerikObject.WaitForControlDisappear(new HtmlControl(PageDashboard.page_Loader));
            TelerikObject.Click(PageWellTest.btnCreateNewWellTest);
            AddNewESPWellTest(filename);
            Toastercheck(PageWellTest.toasterControl, "Add Well Test", "Saved successfully.");
            Thread.Sleep(5000);
        }
        public void AddESPWellTest_Analysis(string filename)
        {
            TelerikObject.WaitForControlDisappear(new HtmlControl(PageDashboard.page_Loader));
            TelerikObject.Click(PageWellTest.btnCreateNewWellTest);
            AddNewESPWellTest_Analysis(filename);
            Toastercheck(PageWellTest.toasterControl, "Add Well Test", "Saved successfully.");
            Thread.Sleep(5000);
        }

        //
        private void TuneWellTestGL()
        {
            GetWellTestDateTime("GLWellTestData.xml");
            HtmlControl PrtwelltestCheckBox = TelerikObject.GetPreviousSibling(PageWellTest.cell_testdata, 1);
            HtmlControl welltestCheckBox = TelerikObject.GetChildrenControl(PrtwelltestCheckBox, "0");
            Assert.IsNotNull(welltestCheckBox, "Check Box control was null");
            //   TelerikObject.CheckBox_Check(welltestCheckBox.BaseElement);
            TelerikObject.Click(welltestCheckBox.BaseElement);
            TelerikObject.Click(PageWellTest.btnTuneWellTest);
            Toastercheck(PageWellTest.toasterControl, "GL Well Test Tuning", "Tuned Successfully.");

        }

        private void TuneWellTestNF()
        {
            GetWellTestDateTime("NFWellTestData.xml");
            HtmlControl PrtwelltestCheckBox = TelerikObject.GetPreviousSibling(PageWellTest.cell_testdata, 1);
            HtmlControl welltestCheckBox = TelerikObject.GetChildrenControl(PrtwelltestCheckBox, "0");
            Assert.IsNotNull(welltestCheckBox, "Check Box control was null");
            //   TelerikObject.CheckBox_Check(welltestCheckBox.BaseElement);
            TelerikObject.Click(welltestCheckBox.BaseElement);
            TelerikObject.Click(PageWellTest.btnTuneWellTest);
            Toastercheck(PageWellTest.toasterControl, "NF Well Test Tuning", "Tuned Successfully.");

        }


        private void GetWellTestDateTime(string XmlFile)
        {
            string testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\TestData", XmlFile);
            DataTable dt = CommonHelper.BuildDataTableFromXml(testdatafile);
            string[] testdateonly = dt.Rows[0]["TestDate"].ToString().Split(new char[] { ';' });
            DateTime testdt = Convert.ToDateTime(testdateonly[0]);
            string modifiledttime = testdt.ToString("M/d/yy") + ", " + testdateonly[1];
            PageWellTest.DynamicValue = modifiledttime;
        }
        int VerifyWellTestRecordcount(string recordstate)
        {
            int count = 0;
            //if (PageWellTest.lbl_TableRecordCount != null)
            //{
            //    string RecordCount = PageWellTest.lbl_TableRecordCount.BaseElement.InnerText;
            //    count = TelerikObject.getWellTestRecordCount(RecordCount);
            //    ClientAutomation.Helper.CommonHelper.TraceLine(string.Format("Well test Records count {1} : = {0}  ", count, recordstate));
            //}
            count = TelerikObject.getHTMLTablesCount();
            //In Html DOM we see 2 tables added for every welltest added
            //   string RecordCount = PageWellTest.lbl_TableRecordCount.BaseElement.InnerText;
            //count = TelerikObject.getWellTestRecordCount(RecordCount);
            CommonHelper.TraceLine(string.Format("Well test Records count {1} : = {0}  ", count, recordstate));
            return count;
        }

        public static void select_wellesp()
        {
            string wellname = ConfigurationManager.AppSettings["Wellname"];
            TelerikObject.Sendkeys(PageDashboard.WellSelectionDropDown, wellname);
            //Keyboard_Send(Keys.Down);
            Thread.Sleep(3000);
            //Keyboard_Send(Keys.Enter);
            PageDashboard.DynamicValue = wellname;
            TelerikObject.Click(PageDashboard.listitems_wellname);
        }
    }
}

