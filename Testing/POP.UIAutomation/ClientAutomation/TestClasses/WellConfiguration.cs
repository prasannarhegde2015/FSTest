using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using ArtOfTest.WebAii.Core;
using ArtOfTest.WebAii.ObjectModel;
using ArtOfTest.WebAii.Win32.Dialogs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Weatherford.POP.DTOs;
using Weatherford.POP.Enums;
using NonTelerikHtml = ArtOfTest.WebAii.Controls.HtmlControls;
using System.Collections.Generic;
using ClientAutomation.TelerikCoreUtils;
using System.Data;
using ArtOfTest.WebAii.Controls.HtmlControls;
using ClientAutomation.Helper;

namespace ClientAutomation
{
    public class WellConfiguration : ForeSiteAutoBase
    {
        private UITestData TestData = new UITestData();

        public void CreateWell_UI(Manager ForeSiteUIAutoManager, WellConfigDTO WellConfig)
        {
            CommonHelper.TraceLine("$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$----CreateWell----$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$");
            switch (WellConfig.Well.WellType)
            {
                case WellTypeId.RRL:
                    {
                        //NonTelerikHtml.HtmlDiv well_Configuration = ForeSiteUIAutoManager.ActiveBrowser.Find.ByContent<NonTelerikHtml.HtmlDiv>("Well Configuration");
                        //well_Configuration.Click();
                        Thread.Sleep(4000);
                        DeleteWell_UI(ForeSiteUIAutoManager);
                        Configuration_General_UI(ForeSiteUIAutoManager, WellConfig);
                        Configuration_WellAttributes_UI(ForeSiteUIAutoManager, WellConfig);
                        Configuration_Surface_UI(ForeSiteUIAutoManager, WellConfig);
                        Configuration_Weights_UI(ForeSiteUIAutoManager, WellConfig);
                        Configuration_Downhole_UI(ForeSiteUIAutoManager, WellConfig);
                        Configuration_Rods_UI(ForeSiteUIAutoManager, WellConfig);
                        SaveWell(ForeSiteUIAutoManager, WellConfig);
                        NonTelerikHtml.HtmlDiv well_Configuration = ForeSiteUIAutoManager.ActiveBrowser.Find.ByContent<NonTelerikHtml.HtmlDiv>("Well Configuration");
                        well_Configuration.Click();
                        Verify_Configuration_General_UI(ForeSiteUIAutoManager, WellConfig);
                        Verify_Configuration_WellAttributes_UI(ForeSiteUIAutoManager, WellConfig);
                        Verify_Configuration_Surface_UI(ForeSiteUIAutoManager, WellConfig);
                        Verify_Configuration_Weights_UI(ForeSiteUIAutoManager, WellConfig);
                        Verify_Configuration_Downhole_UI(ForeSiteUIAutoManager, WellConfig);
                        Verify_Configuration_Rods_UI(ForeSiteUIAutoManager, WellConfig);
                        break;
                    }
                case WellTypeId.ESP:
                case WellTypeId.NF:
                case WellTypeId.GLift:
                case WellTypeId.WInj:
                case WellTypeId.GInj:
                case WellTypeId.PLift:
                    {
                        Thread.Sleep(10000);
                        DeleteWell_UI(ForeSiteUIAutoManager);
                        Configuration_General_UI(ForeSiteUIAutoManager, WellConfig);
                        Configuration_WellAttributes_UI(ForeSiteUIAutoManager, WellConfig);
                        SaveWell(ForeSiteUIAutoManager, WellConfig);

                        //Setting Unit System to US
                        CommonHelper.ChangeUnitSystemUserSetting("US");

                        //Navigating to Network Configuration tab
                        NonTelerikHtml.HtmlDiv network_Configuration = ForeSiteUIAutoManager.ActiveBrowser.Find.ByContent<NonTelerikHtml.HtmlDiv>("Network Configuration");
                        network_Configuration.Click();
                        Thread.Sleep(15000);

                        NonTelerikHtml.HtmlDiv well_Configuration = ForeSiteUIAutoManager.ActiveBrowser.Find.ByContent<NonTelerikHtml.HtmlDiv>("Well Configuration");
                        well_Configuration.Click();
                        Verify_Configuration_General_UI(ForeSiteUIAutoManager, WellConfig);
                        Verify_Configuration_WellAttributes_UI(ForeSiteUIAutoManager, WellConfig);

                        //Verifying Model data with US Unit System
                        Verify_Configuration_Modeldata_UI(ForeSiteUIAutoManager, WellConfig, "US");

                        //Changing Unit Syem to Metric
                        CommonHelper.ChangeUnitSystemUserSetting("Metric");

                        //Navigating to Network Configuration tab
                        network_Configuration.Click();
                        Thread.Sleep(15000);

                        //Navigating back to Well Configuration tab
                        well_Configuration.Click();
                        TelerikObject.WaitForControlDisappear(new HtmlControl(PageObjects.PageDashboard.page_Loader));
                        Verify_Configuration_Modeldata_UI(ForeSiteUIAutoManager, WellConfig, "Metric");

                        //Reverting Unit System back to US
                        CommonHelper.ChangeUnitSystemUserSetting("US");
                    }
                    break;

                default:
                    {
                        Assert.Fail("Well Type not expected");
                        break;
                    }
            }
        }

        public void DownLoadDownholeConfig_UI(Manager ForeSiteUIAutoManager)
        {
            ForeSiteUIAutoManager.ActiveBrowser.RefreshDomTree();
            NonTelerikHtml.HtmlButton downloadConfig = ForeSiteUIAutoManager.ActiveBrowser.Find.ByName<NonTelerikHtml.HtmlButton>("sendDataButton");
            Assert.IsNotNull(downloadConfig, "Failed to Sens Downhole configuration button");
            downloadConfig.MouseClick();
            Thread.Sleep(2000);
            ForeSiteUIAutoManager.ActiveBrowser.RefreshDomTree();
            NonTelerikHtml.HtmlButton sendConfig = ForeSiteUIAutoManager.ActiveBrowser.Find.ByContent<NonTelerikHtml.HtmlButton>("Yes, Download Downhole Configuration.");
            Assert.IsNotNull(sendConfig, "Failed to locate Yes, Download Downhole Configuration. button");
            Assert.IsTrue(sendConfig.IsVisible(), "Unable to Open the Model after clicking on sendConfig button");
            CommonHelper.TraceLine("sendConfig model opened");
            sendConfig.MouseClick();
            CommonHelper.TraceLine("Clicked on Yes, Download Downhole Configuration. button");
            Thread.Sleep(3000);
            PrintScreen("SendDownholeConfig");
        }

        public void DeleteWell_UI(Manager ForeSiteUIAutoManager)
        {
            try
            {
                ReadOnlyCollection<NonTelerikHtml.HtmlButton> close = ForeSiteUIAutoManager.ActiveBrowser.Find.AllByAttributes<NonTelerikHtml.HtmlButton>("class=close");
                for (int i = 0; i < close.Count; i++)
                {
                    close[i].Click();
                }
                Element wellCount = ForeSiteUIAutoManager.ActiveBrowser.Find.ByAttributes("class=well-counter-label");
                CommonHelper.TraceLine("well count : " + wellCount.InnerText);
                int count;
                GetWellCount(wellCount.InnerText, out count);
                if (count != 0)
                {
                    CommonHelper.TraceLine("$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$----DeleteWell----$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$");
                    NonTelerikHtml.HtmlDiv well_Configuration = ForeSiteUIAutoManager.ActiveBrowser.Find.ByContent<NonTelerikHtml.HtmlDiv>("Well Configuration");
                    well_Configuration.Click();
                    CommonHelper.TraceLine("Clicked on well configuration");
                    Thread.Sleep(5000);
                    ForeSiteUIAutoManager.ActiveBrowser.RefreshDomTree();
                    NonTelerikHtml.HtmlButton createNewWell = ForeSiteUIAutoManager.ActiveBrowser.Find.ByName<NonTelerikHtml.HtmlButton>("createNewWellButton");
                    Assert.IsTrue(createNewWell.IsVisible(), "Create New Well button is not visible");
                    CommonHelper.TraceLine("Visible Check for Create New Well button is completed");
                    Assert.IsTrue(createNewWell.IsEnabled, "Create New Well button is not enabled");
                    CommonHelper.TraceLine("Enable Check for Create New Well button is completed");
                    NonTelerikHtml.HtmlButton deleteWell = ForeSiteUIAutoManager.ActiveBrowser.Find.ByName<NonTelerikHtml.HtmlButton>("deleteButton");
                    Assert.IsTrue(deleteWell.IsVisible(), "Delete Well button is not visible");
                    CommonHelper.TraceLine("Visible Check for Delete Well button is completed");
                    Assert.IsTrue(deleteWell.IsEnabled, "Delete Well button is not enabled");
                    CommonHelper.TraceLine("Enable Check for Delete Well button is completed");
                    if (deleteWell.IsEnabled)
                    {
                        Element wellCount_before = ForeSiteUIAutoManager.ActiveBrowser.Find.ByAttributes("class=well-counter-label");
                        CommonHelper.TraceLine("well count before deletion is : " + wellCount_before.InnerText);
                        deleteWell.Click();
                        Thread.Sleep(3000);
                        CommonHelper.TraceLine("Clicked on delete well button");
                        ForeSiteUIAutoManager.ActiveBrowser.RefreshDomTree();
                        NonTelerikHtml.HtmlButton firstDelete = ForeSiteUIAutoManager.ActiveBrowser.Find.ByContent<NonTelerikHtml.HtmlButton>("Yes, Delete This Well");
                        Assert.IsTrue(firstDelete.IsVisible(), "Unable to Open the Delete Model after clicking on delete button");
                        CommonHelper.TraceLine("Delete model opened for the 1st confirmation");
                        firstDelete.Click();
                        NonTelerikHtml.HtmlButton secondDelete = ForeSiteUIAutoManager.ActiveBrowser.Find.ByContent<NonTelerikHtml.HtmlButton>("Yes, Delete This Well");
                        Assert.IsTrue(secondDelete.IsVisible(), "Unable to Open the Delete Model after clicking on delete button");
                        CommonHelper.TraceLine("Delete model opened for the 2nd confirmation");
                        secondDelete.Click();
                        CommonHelper.TraceLine("Clicked on Yes, Delete this Well button");
                        Thread.Sleep(15000);
                        Element wellCount_after = ForeSiteUIAutoManager.ActiveBrowser.Find.ByAttributes("class=well-counter-label");
                        CommonHelper.TraceLine("well count after deletion is : " + wellCount_after.InnerMarkup);
                        int countBefore;
                        GetWellCount(wellCount_before.InnerText, out countBefore);
                        int countAfter;
                        GetWellCount(wellCount_after.InnerMarkup, out countAfter);
                        Assert.AreNotEqual(countBefore, countAfter, "Error occurred while deleting a well");
                    }
                    else
                    {
                        CommonHelper.TraceLine("No wells inside the ForeSite system");
                    }
                }
                else
                {
                    CommonHelper.TraceLine("No wells inside the ForeSite system");
                }
            }
            catch (Exception ex)
            {
                CommonHelper.TraceLine("Failed to delete the created Well");
                CommonHelper.TraceLine(DateTime.Now + ex.Message);
            }
        }

        public void SettingInput(Manager ForeSiteUIAutoManager, string name, string givenValue, string expValue)
        {
            NonTelerikHtml.HtmlControl input = ForeSiteUIAutoManager.ActiveBrowser.Find.ByName<NonTelerikHtml.HtmlControl>(name);
            Assert.IsNotNull(input, "Failed to locate " + name);
            string actualValue = "";
            if (givenValue != "")
            {
                actualValue = input.Attributes.FirstOrDefault(x => x.Name == "ng-reflect-model").Value;
                CommonHelper.TraceLine("Deafult value shown for " + name + " is : " + actualValue);
                Assert.AreEqual(expValue, actualValue, "Incorrect default value");
                CommonHelper.TraceLine("Deafult value check is completed for " + name);
                ForeSiteUIAutoManager.ActiveBrowser.Actions.SetText(input.Find.ByExpression("class=k-input k-formatted-value"), "");
                Thread.Sleep(1000);
                input.MouseClick();
                Thread.Sleep(1000);
                ForeSiteUIAutoManager.ActiveBrowser.Desktop.KeyBoard.TypeText(givenValue, 100);
                CommonHelper.TraceLine(DateTime.Now + name + " is set");
            }
            else
            {
                input = ForeSiteUIAutoManager.ActiveBrowser.Find.ByName<NonTelerikHtml.HtmlControl>(name);
                actualValue = input.Attributes.FirstOrDefault(x => x.Name == "ng-reflect-model").Value;
                CommonHelper.TraceLine("Given value shown for " + name + " is : " + actualValue);
                Assert.AreEqual(expValue, actualValue, "Incorrect given value");
                CommonHelper.TraceLine("Given value check is completed for " + name);
            }
        }

        public void Settings_UI(Manager ForeSiteUIAutoManager, IntelligentAlarmDTO Idata)
        {
            try
            {
                CommonHelper.TraceLine("$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$----Settings_UI----$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$");
                //IntelligentAlarmSettings
                NonTelerikHtml.HtmlDiv well_Configuration = ForeSiteUIAutoManager.ActiveBrowser.Find.ByContent<NonTelerikHtml.HtmlDiv>("Well Configuration");
                well_Configuration.Click();
                Thread.Sleep(5000);
                ForeSiteUIAutoManager.ActiveBrowser.RefreshDomTree();
                NonTelerikHtml.HtmlControl int_settings = ForeSiteUIAutoManager.ActiveBrowser.Find.ByExpression<NonTelerikHtml.HtmlControl>("id=settingDropdownButton");
                Assert.IsNotNull(int_settings, "Failed to locate settings button");
                int_settings.MouseClick();
                CommonHelper.TraceLine("Clicked on Settings dropdown button");
                Thread.Sleep(1000);
                ForeSiteUIAutoManager.ActiveBrowser.RefreshDomTree();
                NonTelerikHtml.HtmlListItem settingItem = ForeSiteUIAutoManager.ActiveBrowser.Find.ByContent<NonTelerikHtml.HtmlListItem>("Intelligent Alarms");
                Assert.IsNotNull(settingItem, "Setting Item is not available inside the dropdown");
                settingItem.MouseClick();
                Thread.Sleep(5000);
                ForeSiteUIAutoManager.ActiveBrowser.RefreshDomTree();
                SettingInput(ForeSiteUIAutoManager, "beamloadHigh", Idata.HighStructureLoad, "90");
                SettingInput(ForeSiteUIAutoManager, "gearboxtorqueHigh", Idata.HighGearboxTorque, "90");
                SettingInput(ForeSiteUIAutoManager, "pumpefficiencyLow", Idata.LowPumpEfficiency, "60");
                SettingInput(ForeSiteUIAutoManager, "rodstressHigh", Idata.HighRodStress, "85");
                SettingInput(ForeSiteUIAutoManager, "gearBox", Idata.IntelligentAlarmStatus[0], "20");
                SettingInput(ForeSiteUIAutoManager, "rodString", Idata.IntelligentAlarmStatus[1], "30");
                SettingInput(ForeSiteUIAutoManager, "cardArea", Idata.IntelligentAlarmStatus[2], "1.5");
                SettingInput(ForeSiteUIAutoManager, "runTime", Idata.IntelligentAlarmStatus[3], "1.5");
                SettingInput(ForeSiteUIAutoManager, "maxLoad", Idata.IntelligentAlarmStatus[4], "1.5");
                SettingInput(ForeSiteUIAutoManager, "minLoad", Idata.IntelligentAlarmStatus[5], "1.5");
                SettingInput(ForeSiteUIAutoManager, "InferredProd", Idata.IntelligentAlarmStatus[6], "1.5");
                SettingInput(ForeSiteUIAutoManager, "pumpCycles", Idata.IntelligentAlarmStatus[7], "1.5");
                NonTelerikHtml.HtmlDiv settingFooter = ForeSiteUIAutoManager.ActiveBrowser.Find.ById<NonTelerikHtml.HtmlDiv>("settingsFooter");
                Assert.IsNotNull(settingFooter, "Failed to locate the Setting footer");
                NonTelerikHtml.HtmlButton saveSettings = settingFooter.Find.ByContent<NonTelerikHtml.HtmlButton>("Save");
                Assert.IsNotNull(saveSettings, "Failed to locate the Setting footer");
                Assert.IsTrue(saveSettings.IsEnabled, "Save button should be enabled after providing the required input");
                CommonHelper.TraceLine("Enable check for Save button is completed");
                saveSettings.MouseClick();
                CommonHelper.TraceLine("Clicked on Save button");
                Thread.Sleep(3000);
                NonTelerikHtml.HtmlSpan close = ForeSiteUIAutoManager.ActiveBrowser.Find.ByExpression<NonTelerikHtml.HtmlSpan>("class=k-icon k-i-x");
                Assert.IsNotNull(close, "Failed to locate close button");
                close.MouseClick();
                CommonHelper.TraceLine("Clicked on close button for the Intelligent Alarms setting modal");
                Thread.Sleep(2000);
                ForeSiteUIAutoManager.ActiveBrowser.RefreshDomTree();
                int_settings = ForeSiteUIAutoManager.ActiveBrowser.Find.ByExpression<NonTelerikHtml.HtmlControl>("id=settingDropdownButton");
                Assert.IsNotNull(int_settings, "Failed to locate settings button");
                int_settings.MouseClick();
                CommonHelper.TraceLine("Clicked on Settings dropdown button");
                Thread.Sleep(1000);
                ForeSiteUIAutoManager.ActiveBrowser.RefreshDomTree();
                settingItem = ForeSiteUIAutoManager.ActiveBrowser.Find.ByContent<NonTelerikHtml.HtmlListItem>("Intelligent Alarms");
                Assert.IsNotNull(settingItem, "Setting Item is not available inside the dropdown");
                settingItem.MouseClick();
                Thread.Sleep(5000);
                ForeSiteUIAutoManager.ActiveBrowser.RefreshDomTree();
                SettingInput(ForeSiteUIAutoManager, "beamloadHigh", "", "64");
                SettingInput(ForeSiteUIAutoManager, "gearboxtorqueHigh", "", "88");
                SettingInput(ForeSiteUIAutoManager, "pumpefficiencyLow", "", "55");
                SettingInput(ForeSiteUIAutoManager, "rodstressHigh", "", "92");
                SettingInput(ForeSiteUIAutoManager, "gearBox", "", "50");
                SettingInput(ForeSiteUIAutoManager, "rodString", "", "60");
                SettingInput(ForeSiteUIAutoManager, "cardArea", "", "2.5");
                SettingInput(ForeSiteUIAutoManager, "runTime", "", "2.5");
                SettingInput(ForeSiteUIAutoManager, "maxLoad", "", "2.5");
                SettingInput(ForeSiteUIAutoManager, "minLoad", "", "2.5");
                SettingInput(ForeSiteUIAutoManager, "InferredProd", "", "2.5");
                SettingInput(ForeSiteUIAutoManager, "pumpCycles", "", "2.5");
                Thread.Sleep(1000);
                close = ForeSiteUIAutoManager.ActiveBrowser.Find.ByExpression<NonTelerikHtml.HtmlSpan>("class=k-icon k-i-x");
                Assert.IsNotNull(close, "Failed to locate close button");
                close.MouseClick();
                CommonHelper.TraceLine("Clicked on close button for the Intelligent Alarms setting modal");
                CommonHelper.TraceLine("Intelligent alarm check is completed");
                Thread.Sleep(3000);
            }
            catch (AssertFailedException)
            {
                ForeSiteUIAutoManager.ActiveBrowser.RefreshDomTree();
                NonTelerikHtml.HtmlSpan close = ForeSiteUIAutoManager.ActiveBrowser.Find.ByExpression<NonTelerikHtml.HtmlSpan>("class=k-icon k-i-x");
                if (close != null)
                {
                    close.MouseClick();
                }
                throw;
            }
            catch (Exception)
            {
                ForeSiteUIAutoManager.ActiveBrowser.RefreshDomTree();
                NonTelerikHtml.HtmlSpan close = ForeSiteUIAutoManager.ActiveBrowser.Find.ByExpression<NonTelerikHtml.HtmlSpan>("class=k-icon k-i-x");
                if (close != null)
                {
                    close.MouseClick();
                }
                throw;
            }
        }

        public void UploadModelFile(Manager ForeSiteUIAutoManager, WellConfigDTO WellConfig)
        {
            NonTelerikHtml.HtmlInputControl modelFileDate = ForeSiteUIAutoManager.ActiveBrowser.Find.ById<NonTelerikHtml.HtmlInputControl>("modelFileDate");
            Assert.IsNotNull(modelFileDate, "Failed to locate Facility Id input field");
            NonTelerikHtml.HtmlDiv modelFileDateButton = modelFileDate.BaseElement.GetNextSibling().As<NonTelerikHtml.HtmlDiv>();
            Assert.IsNotNull(modelFileDateButton, "Failed to locate Facility Id button");
            modelFileDateButton.Click();
            CommonHelper.TraceLine("Clicked on Model file button");
            ForeSiteUIAutoManager.ActiveBrowser.RefreshDomTree();
            string path = FilesLocation + @"\" + WellConfig.Well.SubAssemblyAPI;
            ForeSiteUIAutoManager.DialogMonitor.AddDialog(new FileUploadDialog(ForeSiteUIAutoManager.ActiveBrowser, path, DialogButton.OPEN));
            ForeSiteUIAutoManager.DialogMonitor.Start();
            NonTelerikHtml.HtmlDiv selectModelFile = ForeSiteUIAutoManager.ActiveBrowser.Find.ByExpression<NonTelerikHtml.HtmlDiv>("class=k-button k-upload-button");
            Assert.IsNotNull(selectModelFile, "Clicked on select model file button");
            selectModelFile.MouseClick();
            Thread.Sleep(5000);
            CommonHelper.TraceLine("Clicked on Model upload dialog box");
            ForeSiteUIAutoManager.ActiveBrowser.RefreshDomTree();
            NonTelerikHtml.HtmlControl uploadStatus = ForeSiteUIAutoManager.ActiveBrowser.Find.ByExpression<NonTelerikHtml.HtmlControl>("class=k-upload-status k-upload-status-total");
            Assert.IsNotNull(uploadStatus, "Failed to locate the file status");
            CommonHelper.TraceLine("Model File Upload status : " + uploadStatus.ChildNodes[1].Content);
            //Sometimes it takes time to Upload model file successfully. Hence giving static wait of 5Secs
            Thread.Sleep(5000);
            Assert.AreEqual("Done", uploadStatus.ChildNodes[1].Content, "Failed to Upload Model file");
            CommonHelper.TraceLine("Model upload is complete");
            NonTelerikHtml.HtmlControl label = ForeSiteUIAutoManager.ActiveBrowser.Find.ByExpression<NonTelerikHtml.HtmlControl>("for=applicableDate");
            Assert.IsNotNull(label, "Failed to locate the Applicable date");
            NonTelerikHtml.HtmlButton okModel = ForeSiteUIAutoManager.ActiveBrowser.Find.ByContent<NonTelerikHtml.HtmlButton>("Apply");
            Assert.IsNotNull(okModel, "Failed to locate Apply model button");
            Assert.IsFalse(okModel.IsEnabled, "Apply model button should be disbaled with out providing the required details");
            CommonHelper.TraceLine("Disable check for Apply model is complete");
            NonTelerikHtml.HtmlControl dateInput = label.BaseElement.GetNextSibling().As<NonTelerikHtml.HtmlControl>();
            Assert.IsNotNull(dateInput, "Failed to locate the Commission date");
            dateInput.MouseClick(MouseClickType.LeftClick, 40, 5);
            CommonHelper.TraceLine("Clicked on the Commission date");
            ForeSiteUIAutoManager.Desktop.KeyBoard.KeyPress(System.Windows.Forms.Keys.Left);
            ForeSiteUIAutoManager.Desktop.KeyBoard.KeyPress(System.Windows.Forms.Keys.Left);
            ForeSiteUIAutoManager.Desktop.KeyBoard.KeyPress(System.Windows.Forms.Keys.Left);
            Thread.Sleep(1000);
            DateTime dtCommissionDateTime = Convert.ToDateTime(WellConfig.Well.CommissionDate);
            dtCommissionDateTime = dtCommissionDateTime.AddDays(2); //FRI-1260 : this line is to handle UTC timezone issue as on ISt timezone model data shows blank
            string sCommissionDateTime = dtCommissionDateTime.ToString("MMddyyyy");
            ForeSiteUIAutoManager.Desktop.KeyBoard.TypeText(sCommissionDateTime, 150);
            CommonHelper.TraceLine("Applicable date is set");
            Thread.Sleep(1000);
            NonTelerikHtml.HtmlTextArea comments = ForeSiteUIAutoManager.ActiveBrowser.Find.ById<NonTelerikHtml.HtmlTextArea>("modelImportComment");
            Assert.IsNotNull(comments, "Failed to locate comments box for model file upload");
            comments.MouseClick();
            ForeSiteUIAutoManager.ActiveBrowser.Desktop.KeyBoard.TypeText("Comments", 50);
            CommonHelper.TraceLine("Comments are set");
            Thread.Sleep(5000);
            if (WellConfig.Well.WellType != WellTypeId.PLift)
                DDL(ForeSiteUIAutoManager, "tuningMethods", "Reservoir Pressure");
            ForeSiteUIAutoManager.ActiveBrowser.RefreshDomTree();
            okModel.Refresh();
            Assert.IsTrue(okModel.IsEnabled, "Apply model button should be enabled with out providing the required details");
            CommonHelper.TraceLine("Enable check for Apply model is complete");
            okModel.MouseClick();
            CommonHelper.TraceLine("Clicked on Apply model button");
        }

        public void SelectFacilityId(Manager ForeSiteUIAutoManager, WellTypeId wType)
        {
            NonTelerikHtml.HtmlInputControl facilityId = ForeSiteUIAutoManager.ActiveBrowser.Find.ById<NonTelerikHtml.HtmlInputControl>("facilityId");
            Assert.IsNotNull(facilityId, "Failed to locate Facility Id input field");
            NonTelerikHtml.HtmlControl facilityButton = facilityId.BaseElement.GetNextSibling().As<NonTelerikHtml.HtmlControl>();
            Assert.IsNotNull(facilityButton, "Failed to locate Facility Id button");
            facilityButton.Click();
            CommonHelper.TraceLine("Clicked on Facility Id button");
            ForeSiteUIAutoManager.ActiveBrowser.RefreshDomTree();
            ReadOnlyCollection<NonTelerikHtml.HtmlTable> Facs = ForeSiteUIAutoManager.ActiveBrowser.Find.AllByExpression<NonTelerikHtml.HtmlTable>("tagname=table");
            Assert.IsTrue(Facs.Count > 0, "Failed to load the Facility grid model");
            CommonHelper.TraceLine("Model opened for the Facility grid");
            int index = Facs.IndexOf(Facs.FirstOrDefault(x => x.InnerText.Contains("Facility Id")));
            for (int i = 0; i < Facs[index].AllRows.Count; i++)
            {
                for (int j = 0; j < Facs[index].AllRows[i].Cells.Count; j++)
                {
                    Trace.Write(Facs[index].AllRows[i].Cells[j].InnerText + " - ");
                }
                Trace.Write(Environment.NewLine);
            }
            Assert.AreEqual(11, Facs[index].AllRows.Count, "Failed to display the Available facilities");
            CommonHelper.TraceLine("Successfully printed the Facility grid");
            NonTelerikHtml.HtmlButton selectFacility = ForeSiteUIAutoManager.ActiveBrowser.Find.ByContent<NonTelerikHtml.HtmlButton>("Apply");
            Assert.IsNotNull(selectFacility, "Failed to locate the Apply button for the Facility grid");
            Assert.IsFalse(selectFacility.IsEnabled, "Apply is enabled with no facility id selected");
            CommonHelper.TraceLine("Disable check for Apply button is completed");
            if (wType == WellTypeId.RRL)
                Facs[index].AllRows[1].MouseClick();
            else
            {
                NonTelerikHtml.HtmlTableCell facilityFilter = Facs[index].AllRows[0].Cells[0];
                NonTelerikHtml.HtmlAnchor filterIcon = facilityFilter.Find.ByExpression<NonTelerikHtml.HtmlAnchor>("class=k-grid-filter");
                Assert.IsNotNull(filterIcon, "Failed to locate filter icon");
                filterIcon.MouseClick();
                Thread.Sleep(1000);
                ForeSiteUIAutoManager.ActiveBrowser.RefreshDomTree();
                NonTelerikHtml.HtmlControl filterInput = ForeSiteUIAutoManager.ActiveBrowser.Find.ByExpression<NonTelerikHtml.HtmlControl>("class=k-textbox ng-untouched ng-pristine ng-valid");
                Assert.IsNotNull(filterInput, "Failed to locate fileter input");
                filterInput.MouseClick();
                switch (wType)
                {
                    case WellTypeId.ESP:
                        ForeSiteUIAutoManager.ActiveBrowser.Desktop.KeyBoard.TypeText(txtForESP, 50);
                        break;

                    case WellTypeId.GLift:
                        ForeSiteUIAutoManager.ActiveBrowser.Desktop.KeyBoard.TypeText(txtForGL, 50);
                        break;

                    case WellTypeId.NF:
                        ForeSiteUIAutoManager.ActiveBrowser.Desktop.KeyBoard.TypeText(txtForNFW, 50);
                        break;

                    case WellTypeId.GInj:
                        ForeSiteUIAutoManager.ActiveBrowser.Desktop.KeyBoard.TypeText(txtForGI, 50);
                        break;

                    case WellTypeId.WInj:
                        ForeSiteUIAutoManager.ActiveBrowser.Desktop.KeyBoard.TypeText(txtForWI, 50);
                        break;

                    case WellTypeId.PLift:
                        ForeSiteUIAutoManager.ActiveBrowser.Desktop.KeyBoard.TypeText(txtForGL, 50);//TODOKN: Modify the Facility after the ATS config complete
                        break;
                }
                Thread.Sleep(1000);
                ForeSiteUIAutoManager.Desktop.KeyBoard.KeyPress(System.Windows.Forms.Keys.Enter);
                Thread.Sleep(3000);
                ForeSiteUIAutoManager.ActiveBrowser.RefreshDomTree();
                Facs.FirstOrDefault(x => x.InnerText.Contains("Facility Id")).Refresh();
                Facs[index].AllRows[1].MouseClick();
            }
            selectFacility.Refresh();
            Assert.IsTrue(selectFacility.IsEnabled, "Apply is not enabled after the selection of facility id");
            CommonHelper.TraceLine("Enable check for Apply button is completed");
            selectFacility.MouseClick();
            CommonHelper.TraceLine("Clicked on the FacilityId");
            Thread.Sleep(2000);
            ForeSiteUIAutoManager.ActiveBrowser.RefreshDomTree();
            facilityId.Refresh();
            if (facilityId.Value != null)
            {
                Assert.AreEqual(Facs[index].AllRows[1].Cells[0].InnerText, facilityId.Value, "Mismatch between the selcted and desired Facility");
                CommonHelper.TraceLine("Facility Id is set to : " + facilityId.Value);
                ForeSiteUIAutoManager.ActiveBrowser.RefreshDomTree();
            }
            else
            {
                CommonHelper.TraceLine("Failed to locate the selected FacilityId");
            }
        }

        public void Configuration_General_UI(Manager ForeSiteUIAutoManager, WellConfigDTO WellConfig)
        {
            NonTelerikHtml.HtmlDiv well_Configuration = ForeSiteUIAutoManager.ActiveBrowser.Find.ByContent<NonTelerikHtml.HtmlDiv>("Well Configuration");
            //General
            well_Configuration.Click();
            CommonHelper.TraceLine("Clicked on well configuration");
            Thread.Sleep(5000);
            ForeSiteUIAutoManager.ActiveBrowser.RefreshDomTree();
            NonTelerikHtml.HtmlDiv general = ForeSiteUIAutoManager.ActiveBrowser.Find.ByContent<NonTelerikHtml.HtmlDiv>("General");
            Assert.IsNotNull(general, "Unable to Identify the General tab");
            general.Click();
            ForeSiteUIAutoManager.ActiveBrowser.RefreshDomTree();
            CommonHelper.TraceLine("Clicked on the Well configuration General screen");
            NonTelerikHtml.HtmlControl facilityIdButton = ForeSiteUIAutoManager.ActiveBrowser.Find.ById<NonTelerikHtml.HtmlControl>("facilityId"); //12/7 Manish changed from facilityButtonDiv to facilityId 
            Assert.IsNull(facilityIdButton, "Unable to locate the Facility Id button on the Well configuration General screen");
            CommonHelper.TraceLine("Successfully landed on Well configuration General screen");
            NonTelerikHtml.HtmlButton createNewWell = ForeSiteUIAutoManager.ActiveBrowser.Find.ByName<NonTelerikHtml.HtmlButton>("createNewWellButton");
            NonTelerikHtml.HtmlControl settings = ForeSiteUIAutoManager.ActiveBrowser.Find.ById<NonTelerikHtml.HtmlControl>("settingDropdownButton");
            NonTelerikHtml.HtmlButton sendDataButton = ForeSiteUIAutoManager.ActiveBrowser.Find.ByName<NonTelerikHtml.HtmlButton>("sendDataButton");
            NonTelerikHtml.HtmlButton saveWell = ForeSiteUIAutoManager.ActiveBrowser.Find.ByName<NonTelerikHtml.HtmlButton>("saveWellbutton");
            NonTelerikHtml.HtmlButton cancel = ForeSiteUIAutoManager.ActiveBrowser.Find.ByName<NonTelerikHtml.HtmlButton>("cancelButton");
            Assert.IsNull(settings, "Settings button is visble while creating a new well");
            CommonHelper.TraceLine("Settings button check is completed");
            Assert.IsNull(createNewWell, "CreateWell button is visible while creating a new well");
            CommonHelper.TraceLine("CreateWell button check is completed");
            Assert.IsNull(sendDataButton, "Send Downhole Config button is visible while creating new well");
            CommonHelper.TraceLine("Send Downhole Config button check is completed");
            Assert.IsNotNull(saveWell, "Failed to Locate Save button");
            Assert.IsFalse(saveWell.IsEnabled, "Save is enabled without providing the required information");
            CommonHelper.TraceLine("Save button enable check is completed");
            Assert.IsNotNull(cancel, "Failed to Locate Cancel button");
            Assert.IsTrue(cancel.IsEnabled, "Cancel is not enabled to discard the changes");
            CommonHelper.TraceLine("Cancel button check is completed");
            ReadOnlyCollection<NonTelerikHtml.HtmlControl> DDls = ForeSiteUIAutoManager.ActiveBrowser.Find.AllByTagName<NonTelerikHtml.HtmlControl>("kendo-dropdownlist");
            Assert.IsNull(DDls.FirstOrDefault(x => x.ID == "oilAllocationGroup"), "Oil Allocation group should not be visible");
            Assert.IsNull(DDls.FirstOrDefault(x => x.ID == "waterAllocationGroup"), "Water Allocation group should not be visible");
            Assert.IsNull(DDls.FirstOrDefault(x => x.ID == "gasAllocationGroup"), "Gas Allocation group should not be visible");
            NonTelerikHtml.HtmlInputControl wellName = ForeSiteUIAutoManager.ActiveBrowser.Find.ById<NonTelerikHtml.HtmlInputControl>("wellName");
            Assert.IsNotNull(wellName, "Failed to Locate WellName input field");
            wellName.MouseClick();
            ForeSiteUIAutoManager.Desktop.KeyBoard.TypeText(WellConfig.Well.Name, 50);
            CommonHelper.TraceLine("Well name is set to : " + WellConfig.Well.Name);
            NonTelerikHtml.HtmlUnorderedList tabs = ForeSiteUIAutoManager.ActiveBrowser.Find.ByExpression<NonTelerikHtml.HtmlUnorderedList>("role=tablist");
            Assert.AreEqual(2, tabs.AllItems.Count(), "Only General and Well Attributes should be visible");
            ReadOnlyCollection<Element> wt = ForeSiteUIAutoManager.ActiveBrowser.Find.AllByExpression("id=wellType");
            ForeSiteUIAutoManager.ActiveBrowser.Actions.Click(wt.First());
            NonTelerikHtml.HtmlSpan wellType = ForeSiteUIAutoManager.ActiveBrowser.Find.ByContent<NonTelerikHtml.HtmlSpan>("Select type...");
            Assert.IsNotNull(wellType, "Failed to locate Well Type Dropdown");
            wellType.Click();
            CommonHelper.TraceLine("Clicked on well type dropdown");
            ForeSiteUIAutoManager.ActiveBrowser.RefreshDomTree();
            NonTelerikHtml.HtmlListItem TypeItem = ForeSiteUIAutoManager.ActiveBrowser.Find.ByContent<NonTelerikHtml.HtmlListItem>(WellConfig.Well.Name);
            Assert.IsNotNull(TypeItem, WellConfig.Well.Name + " Well type is not available inside the dropdown");
            TypeItem.ScrollToVisible();
            TypeItem.MouseClick();
            ForeSiteUIAutoManager.ActiveBrowser.RefreshDomTree();
            Thread.Sleep(1000);
            tabs = ForeSiteUIAutoManager.ActiveBrowser.Find.ByExpression<NonTelerikHtml.HtmlUnorderedList>("role=tablist");
            int tabCount = WellConfig.Well.WellType == WellTypeId.RRL ? 6 : 2;
            Assert.AreEqual(tabCount, tabs.AllItems.Count(), "Only General and Well Attributes should be visible");
            NonTelerikHtml.HtmlSpan wellTypeSelected = ForeSiteUIAutoManager.ActiveBrowser.Find.ByContent<NonTelerikHtml.HtmlSpan>(WellConfig.Well.Name);
            Assert.IsNotNull(wellTypeSelected, "Failed to select the " + WellConfig.Well.Name + " from the well type dropdown");
            CommonHelper.TraceLine("Well Type is set to : " + wellTypeSelected.InnerText);
            WellTypeId wType = WellConfig.Well.WellType;
            ForeSiteUIAutoManager.ActiveBrowser.RefreshDomTree();
            Thread.Sleep(1000);
            //Selecting Fluid Type from Dropdown if well type is Gas Lift or Plunger Lift
            if (wType == WellTypeId.GLift)
            {
                DDL(ForeSiteUIAutoManager, "fluidType", "Black Oil");
            }
            else if (wType == WellTypeId.PLift)
            {
                DDL(ForeSiteUIAutoManager, "fluidType", "Dry Gas");
            }
            Thread.Sleep(1000);
            ForeSiteUIAutoManager.ActiveBrowser.RefreshDomTree();
            DDL(ForeSiteUIAutoManager, "scadaType", "CygNet");
            Thread.Sleep(1000);
            ForeSiteUIAutoManager.ActiveBrowser.RefreshDomTree();
            facilityIdButton = ForeSiteUIAutoManager.ActiveBrowser.Find.ById<NonTelerikHtml.HtmlControl>("facilityId"); //12/7 Manish changed facilityButtonDiv to facilityId
            Assert.IsNotNull(facilityIdButton, "Unable to locate the Facility Id button on the Well configuration General screen after changing the scada type to CygNet");
            Assert.IsTrue(facilityIdButton.IsVisible(), "Unable to locate the Facility Id button on the Well configuration General screen after changing the scada type to CygNet");
            ReadOnlyCollection<Element> cd = ForeSiteUIAutoManager.ActiveBrowser.Find.AllByExpression("id=cygNetDomain");
            ForeSiteUIAutoManager.ActiveBrowser.Actions.Click(cd.First());
            NonTelerikHtml.HtmlSpan cygnetDomain = ForeSiteUIAutoManager.ActiveBrowser.Find.ByContent<NonTelerikHtml.HtmlSpan>("Select domain...");
            Assert.IsNotNull(cygnetDomain, "Failed to locate CygNet domain Dropdown");
            cygnetDomain.Click();
            CommonHelper.TraceLine("Clicked on CygNet domain dropdown");
            ForeSiteUIAutoManager.ActiveBrowser.RefreshDomTree();
            NonTelerikHtml.HtmlListItem cygnetItem = ForeSiteUIAutoManager.ActiveBrowser.Find.ByContent<NonTelerikHtml.HtmlListItem>(WellConfig.Well.DataConnection.ProductionDomain);
            Assert.IsNotNull(cygnetItem, "CygNet doamin is not available inside the dropdown");
            cygnetItem.MouseClick();
            Thread.Sleep(5000);
            ForeSiteUIAutoManager.ActiveBrowser.RefreshDomTree();
            NonTelerikHtml.HtmlSpan cygnetDomainSelected = ForeSiteUIAutoManager.ActiveBrowser.Find.ByContent<NonTelerikHtml.HtmlSpan>(WellConfig.Well.DataConnection.ProductionDomain);
            Assert.IsNotNull(cygnetDomainSelected, "Failed to select the CygNet doamin from the well type dropdown");
            CommonHelper.TraceLine("CygNet doamin is set to : " + cygnetDomainSelected.InnerText);
            ReadOnlyCollection<Element> ss = ForeSiteUIAutoManager.ActiveBrowser.Find.AllByExpression("id=siteService");
            ForeSiteUIAutoManager.ActiveBrowser.Actions.Click(ss.First());
            NonTelerikHtml.HtmlSpan siteService = ForeSiteUIAutoManager.ActiveBrowser.Find.ByContent<NonTelerikHtml.HtmlSpan>("Select service...");
            Assert.IsNotNull(siteService, "Failed to locate CygNet service Dropdown");
            siteService.Click();
            CommonHelper.TraceLine("Clicked on CygNet service dropdown");
            ForeSiteUIAutoManager.ActiveBrowser.RefreshDomTree();
            Thread.Sleep(1000);
            ForeSiteUIAutoManager.Desktop.KeyBoard.KeyPress(System.Windows.Forms.Keys.Down);
            ForeSiteUIAutoManager.Desktop.KeyBoard.KeyPress(System.Windows.Forms.Keys.Enter);
            Thread.Sleep(1000);
            ForeSiteUIAutoManager.ActiveBrowser.RefreshDomTree();
            SelectFacilityId(ForeSiteUIAutoManager, WellConfig.Well.WellType);
            NonTelerikHtml.HtmlSpan calendar = ForeSiteUIAutoManager.ActiveBrowser.Find.ByExpression<NonTelerikHtml.HtmlSpan>("class=k-icon k-i-calendar");
            Assert.IsNotNull(calendar, "Failed to locate the Commission date");
            Assert.IsTrue(calendar.IsEnabled, "Comission date should be enabled");
            NonTelerikHtml.HtmlControl label = ForeSiteUIAutoManager.ActiveBrowser.Find.ByExpression<NonTelerikHtml.HtmlControl>("for=commissionDate");
            Assert.IsNotNull(label, "Failed to locate the Commission date");
            NonTelerikHtml.HtmlControl dateInput = label.BaseElement.GetNextSibling().As<NonTelerikHtml.HtmlControl>();
            Assert.IsNotNull(dateInput, "Failed to locate the Commission date");
            dateInput.MouseClick();
            CommonHelper.TraceLine("Clicked on the Commission date");
            ForeSiteUIAutoManager.Desktop.KeyBoard.KeyPress(System.Windows.Forms.Keys.Left);
            ForeSiteUIAutoManager.Desktop.KeyBoard.KeyPress(System.Windows.Forms.Keys.Left);
            ForeSiteUIAutoManager.Desktop.KeyBoard.KeyPress(System.Windows.Forms.Keys.Left);
            Thread.Sleep(1000);
            DateTime dtCommissionDateTime = Convert.ToDateTime(WellConfig.Well.CommissionDate);
            string sCommissionDateTime = dtCommissionDateTime.ToString("MMddyyyy");
            ForeSiteUIAutoManager.Desktop.KeyBoard.TypeText(sCommissionDateTime, 50);
            CommonHelper.TraceLine("Commission date is set");
            //Entering data into WellboreId tab
            NonTelerikHtml.HtmlControl WellboreLabel = ForeSiteUIAutoManager.ActiveBrowser.Find.ByExpression<NonTelerikHtml.HtmlControl>("for=assemblyAPI");
            Assert.IsNotNull(WellboreLabel, "Unable to locate Wellbore Id Label");
            NonTelerikHtml.HtmlControl wellboreIdSibblings = WellboreLabel.BaseElement.GetNextSibling().As<NonTelerikHtml.HtmlControl>();
            NonTelerikHtml.HtmlControl WellboreIdInput = TelerikObject.GetChildrenControl(wellboreIdSibblings, "0;0;0;0;0;0");
            WellboreIdInput.MouseClick();
            CommonHelper.TraceLine("Clicked in Wellbore Id input field");
            ForeSiteUIAutoManager.Desktop.KeyBoard.TypeText(WellConfig.Well.AssemblyAPI, 100);
            CommonHelper.TraceLine("Wellbore Id is set to : " + WellboreIdInput.BaseElement.InnerText);
            WellboreLabel.MouseClick();
            Thread.Sleep(2000);
            //Entering data into BoreholeId tab
            NonTelerikHtml.HtmlControl BoreholeLabel = ForeSiteUIAutoManager.ActiveBrowser.Find.ByExpression<NonTelerikHtml.HtmlControl>("for=subAssemblyAPI");
            Assert.IsNotNull(BoreholeLabel, "Unable to locate Borehole Id Label");
            NonTelerikHtml.HtmlControl boreholeIdSibblings = BoreholeLabel.BaseElement.GetNextSibling().As<NonTelerikHtml.HtmlControl>();
            NonTelerikHtml.HtmlControl boreholeIdInput = TelerikObject.GetChildrenControl(boreholeIdSibblings, "0;0;0;0;0");
            boreholeIdInput.MouseClick();
            CommonHelper.TraceLine("Clicked in Borehole Id input field");
            ForeSiteUIAutoManager.Desktop.KeyBoard.TypeText(WellConfig.Well.SubAssemblyAPI, 100);
            CommonHelper.TraceLine("Borehole id is set to : " + boreholeIdInput.BaseElement.InnerText);
            BoreholeLabel.MouseClick();
            Thread.Sleep(2000);
            //Entering data into Perforation Interval tab
            NonTelerikHtml.HtmlControl IntervalAPILabel = ForeSiteUIAutoManager.ActiveBrowser.Find.ByExpression<NonTelerikHtml.HtmlControl>("for=intervalAPI");
            Assert.IsNotNull(IntervalAPILabel, "Unable to locate Perforation Interval Label");
            NonTelerikHtml.HtmlControl perforationIntervalSibblings = IntervalAPILabel.BaseElement.GetNextSibling().As<NonTelerikHtml.HtmlControl>();
            NonTelerikHtml.HtmlControl perforationIntervalInput = TelerikObject.GetChildrenControl(perforationIntervalSibblings, "0;0");
            perforationIntervalInput.MouseClick();
            CommonHelper.TraceLine("Clicked in Perforation Interval input field");
            ForeSiteUIAutoManager.Desktop.KeyBoard.TypeText(WellConfig.Well.IntervalAPI, 100);
            CommonHelper.TraceLine("Perforation Interval is set to : " + perforationIntervalInput.BaseElement.InnerText);
            IntervalAPILabel.MouseClick();
            Thread.Sleep(2000);

            NonTelerikHtml.HtmlControl surfaceLatitude = ForeSiteUIAutoManager.ActiveBrowser.Find.ByExpression<NonTelerikHtml.HtmlControl>("for=latitude");
            NonTelerikHtml.HtmlControl latitude = surfaceLatitude.BaseElement.GetNextSibling().As<NonTelerikHtml.HtmlControl>();
            Assert.IsNotNull(latitude, "Failed to locate latitude input field");
            latitude.MouseClick();
            Thread.Sleep(2000);
            CommonHelper.TraceLine("Clicked in latitude input field");
            ForeSiteUIAutoManager.Desktop.KeyBoard.TypeText(WellConfig.Well.SurfaceLatitude.Value.ToString(), 100);
            CommonHelper.TraceLine("Latitude is set");
            NonTelerikHtml.HtmlControl surfaceLongitude = ForeSiteUIAutoManager.ActiveBrowser.Find.ByExpression<NonTelerikHtml.HtmlControl>("for=longitude");
            NonTelerikHtml.HtmlControl longitude = surfaceLongitude.BaseElement.GetNextSibling().As<NonTelerikHtml.HtmlControl>();
            Assert.IsNotNull(longitude, "Failed to locate latitude input field");
            longitude.MouseClick();
            Thread.Sleep(2000);
            CommonHelper.TraceLine("Clicked in longitude input field");
            ForeSiteUIAutoManager.Desktop.KeyBoard.TypeText(WellConfig.Well.SurfaceLongitude.Value.ToString(), 150);
            CommonHelper.TraceLine("Longitude is set");
            Thread.Sleep(2000);
            ForeSiteUIAutoManager.ActiveBrowser.RefreshDomTree();
            saveWell.Refresh();
            Assert.IsTrue(saveWell.IsEnabled, "Save is not enabled after providing the required information");
            if (wType != WellTypeId.RRL)
            {
                UploadModelFile(ForeSiteUIAutoManager, WellConfig);
            }
            CommonHelper.TraceLine("General Tab check is completed for RRL");
        }

        public void Configuration_WellAttributes_UI(Manager ForeSiteUIAutoManager, WellConfigDTO WellConfig)
        {
            //Well Attributes
            NonTelerikHtml.HtmlDiv wellAttributes = ForeSiteUIAutoManager.ActiveBrowser.Find.ByContent<NonTelerikHtml.HtmlDiv>("Well Attributes");
            Assert.IsNotNull(wellAttributes, "Unable to Locate the Well Attributes tab");
            wellAttributes.Click();
            CommonHelper.TraceLine("Clicked on well configuration - well attributes");
            NonTelerikHtml.HtmlControl labelLease = ForeSiteUIAutoManager.ActiveBrowser.Find.ByExpression<NonTelerikHtml.HtmlControl>("for=lease");
            Assert.IsNotNull(labelLease, "Unable to locate Lease well attribute");
            NonTelerikHtml.HtmlControl lease = labelLease.BaseElement.GetNextSibling().As<NonTelerikHtml.HtmlControl>();
            Assert.IsNotNull(lease, "Unable to locate Lease well attribute");
            lease.MouseClick();
            Thread.Sleep(4000);
            ForeSiteUIAutoManager.Desktop.KeyBoard.TypeText(WellConfig.Well.Lease, 50);
            Thread.Sleep(2000);
            CommonHelper.TraceLine("Lease is set");
            NonTelerikHtml.HtmlControl labelgeographicRegion = ForeSiteUIAutoManager.ActiveBrowser.Find.ByExpression<NonTelerikHtml.HtmlControl>("for=geographicRegion");
            Assert.IsNotNull(labelgeographicRegion, "Unable to locate geographicRegion well attribute");
            labelgeographicRegion.MouseClick();
            NonTelerikHtml.HtmlControl geographicRegion = labelgeographicRegion.BaseElement.GetNextSibling().As<NonTelerikHtml.HtmlControl>();
            Assert.IsNotNull(geographicRegion, "Unable to locate geographicRegion well attribute");
            geographicRegion.MouseClick();
            Thread.Sleep(4000);
            ForeSiteUIAutoManager.Desktop.KeyBoard.TypeText(WellConfig.Well.GeographicRegion, 50);
            Thread.Sleep(2000);
            CommonHelper.TraceLine("Geographic region is set");
            NonTelerikHtml.HtmlControl labelField = ForeSiteUIAutoManager.ActiveBrowser.Find.ByExpression<NonTelerikHtml.HtmlControl>("for=field");
            Assert.IsNotNull(labelField, "Unable to locate field well attribute");
            labelField.MouseClick();
            NonTelerikHtml.HtmlControl field = labelField.BaseElement.GetNextSibling().As<NonTelerikHtml.HtmlControl>();
            Assert.IsNotNull(field, "Unable to locate field well attribute");
            field.MouseClick();
            Thread.Sleep(4000);
            ForeSiteUIAutoManager.Desktop.KeyBoard.TypeText(WellConfig.Well.Field, 50);
            Thread.Sleep(2000);
            CommonHelper.TraceLine("Field region is set");
            NonTelerikHtml.HtmlControl labelForeman = ForeSiteUIAutoManager.ActiveBrowser.Find.ByExpression<NonTelerikHtml.HtmlControl>("for=foreman");
            Assert.IsNotNull(labelForeman, "Unable to locate foreman well attribute");
            labelForeman.MouseClick();
            NonTelerikHtml.HtmlControl foreman = labelForeman.BaseElement.GetNextSibling().As<NonTelerikHtml.HtmlControl>();
            Assert.IsNotNull(foreman, "Unable to locate foreman well attribute");
            foreman.MouseClick();
            Thread.Sleep(4000);
            ForeSiteUIAutoManager.Desktop.KeyBoard.TypeText(WellConfig.Well.Foreman, 50);
            Thread.Sleep(2000);
            CommonHelper.TraceLine("foreman region is set");
            NonTelerikHtml.HtmlControl labelEngineer = ForeSiteUIAutoManager.ActiveBrowser.Find.ByExpression<NonTelerikHtml.HtmlControl>("for=engineer");
            Assert.IsNotNull(labelEngineer, "Unable to locate engineer well attribute");
            labelEngineer.MouseClick();
            NonTelerikHtml.HtmlControl engineer = labelEngineer.BaseElement.GetNextSibling().As<NonTelerikHtml.HtmlControl>();
            Assert.IsNotNull(engineer, "Unable to locate engineer well attribute");
            engineer.MouseClick();
            Thread.Sleep(4000);
            ForeSiteUIAutoManager.Desktop.KeyBoard.TypeText(WellConfig.Well.Engineer, 50);
            Thread.Sleep(2000);
            CommonHelper.TraceLine("enginner is set");
            NonTelerikHtml.HtmlControl labelGaugerBeat = ForeSiteUIAutoManager.ActiveBrowser.Find.ByExpression<NonTelerikHtml.HtmlControl>("for=gaugerBeat");
            Assert.IsNotNull(labelGaugerBeat, "Unable to locate gaugerBeat well attribute");
            labelGaugerBeat.MouseClick();
            NonTelerikHtml.HtmlControl gaugerBeat = labelGaugerBeat.BaseElement.GetNextSibling().As<NonTelerikHtml.HtmlControl>();
            Assert.IsNotNull(gaugerBeat, "Unable to locate gaugerBeat well attribute");
            gaugerBeat.MouseClick();
            Thread.Sleep(4000);
            ForeSiteUIAutoManager.Desktop.KeyBoard.TypeText(WellConfig.Well.GaugerBeat, 50);
            Thread.Sleep(2000);
            CommonHelper.TraceLine("gaugerBeat region is set");
        }

        public void Configuration_Surface_UI(Manager ForeSiteUIAutoManager, WellConfigDTO WellConfig)
        {
            ForeSiteUIAutoManager.ActiveBrowser.RefreshDomTree();
            Thread.Sleep(10000);
            //Surface
            NonTelerikHtml.HtmlDiv surface = ForeSiteUIAutoManager.ActiveBrowser.Find.ByContent<NonTelerikHtml.HtmlDiv>("Surface");
            Assert.IsNotNull(surface, "Unable to Locate the Surface tab");
            surface.Click();
            CommonHelper.TraceLine("Clicked on well configuration - surface");
            ForeSiteUIAutoManager.ActiveBrowser.RefreshDomTree();
            Thread.Sleep(3000);
            NonTelerikHtml.HtmlInputControl pumpText = ForeSiteUIAutoManager.ActiveBrowser.Find.ById<NonTelerikHtml.HtmlInputControl>("pumpingUnit");
            Assert.IsNotNull(pumpText, "Unable to locate Pumping Unit text field");
            Assert.IsNull(pumpText.Value, "Pumping Unit text should be empty");
            NonTelerikHtml.HtmlButton pumpButton = ForeSiteUIAutoManager.ActiveBrowser.Find.ById<NonTelerikHtml.HtmlButton>("openPumpingUnitModalButton");
            pumpButton.Click();
            CommonHelper.TraceLine("Clicked on pumping unit selection modal button");
            Thread.Sleep(9000);
            ForeSiteUIAutoManager.ActiveBrowser.RefreshDomTree();
            ReadOnlyCollection<Element> applybutton = ForeSiteUIAutoManager.ActiveBrowser.Find.AllByExpression("tagname=kendo-dialog-actions");
            if (applybutton.Count > 0)
            {
                CommonHelper.TraceLine("Apply Button is located");
            }
            else
            {
                Assert.Fail();
            }
            // ForeSiteUIAutoManager.ActiveBrowser.Actions.Click(applybutton.First());
            // NonTelerikHtml.HtmlButton selectPump = ForeSiteUIAutoManager.ActiveBrowser.Find.ById<NonTelerikHtml.HtmlButton>("buttonApply");
            //  Assert.IsNotNull(selectPump, "Failed to locate the apply Pump button");
            // Assert.IsFalse(selectPump.IsEnabled, "Apply button is enabled without selecting the Pumping unit Item");
            CommonHelper.TraceLine("Apply pump button check is completed");
            Thread.Sleep(15000);
            ForeSiteUIAutoManager.ActiveBrowser.RefreshDomTree();
            CommonHelper.TraceLine("After DOM refresh getting tables");
            ReadOnlyCollection<NonTelerikHtml.HtmlTable> PumpingUnits = ForeSiteUIAutoManager.ActiveBrowser.Find.AllByExpression<NonTelerikHtml.HtmlTable>("tagname=table");
            CommonHelper.TraceLine("Number of tables are" + PumpingUnits.Count);
            Thread.Sleep(1000);
            foreach (NonTelerikHtml.HtmlTable tbl in PumpingUnits)
            {
                CommonHelper.TraceLine("Table Conennt:");
                CommonHelper.TraceLine(DateTime.Now + tbl.InnerText);
            }

            int index = PumpingUnits.IndexOf(PumpingUnits.FirstOrDefault(x => x.InnerText.Contains("American")));
            CommonHelper.TraceLine("Index value is " + index);
            Assert.AreEqual(77, PumpingUnits[index].AllRows.Count, "All manufacture count not expected one");
            CommonHelper.TraceLine("Available Pumping Unit types for the selected Manufacturer : Weatherford");
            for (int i = 0; i < PumpingUnits[index + 2].AllRows.Count; i++)
            {
                CommonHelper.TraceLine(DateTime.Now + PumpingUnits[index + 2].AllRows[i].InnerText);
            }
            Assert.AreEqual(11, PumpingUnits[index + 2].AllRows.Count, "All manufacture types count not expected one");
            Assert.AreEqual("Select a Manufacturer and Type to see Items.", PumpingUnits[index + 4].AllRows[0].InnerText, "No Items should be shown without selecting the Manufacturer Type");
            CommonHelper.TraceLine("Deafult Check for the PumpUnit model is completed");
            NonTelerikHtml.HtmlTableRow selectmanufacturer = PumpingUnits[index].AllRows.FirstOrDefault(x => x.InnerText == WellConfig.ModelConfig.Surface.PumpingUnit.Manufacturer);
            Assert.IsNotNull(selectmanufacturer, "Unable to find the Manufacturer : " + WellConfig.ModelConfig.Surface.PumpingUnit.Manufacturer);
            selectmanufacturer.ScrollToVisible();
            Thread.Sleep(1000);
            selectmanufacturer.MouseClick();
            Thread.Sleep(3000);
            PumpingUnits[index + 2].Refresh();
            CommonHelper.TraceLine("Available Pumping Unit types for the selected Manufacturer : Lufkin");
            Thread.Sleep(4000);
            for (int i = 0; i < PumpingUnits[index + 2].AllRows.Count; i++)
            {
                CommonHelper.TraceLine(DateTime.Now + PumpingUnits[index + 2].AllRows[i].InnerText);
            }
            Assert.AreEqual(15, PumpingUnits[index + 2].AllRows.Count, "All manufacture types count not expected one for Lufkin");
            CommonHelper.TraceLine("Succesfully verified the Manufacturer types count for : " + WellConfig.ModelConfig.Surface.PumpingUnit.Manufacturer);
            NonTelerikHtml.HtmlTableRow selectmanufacturertype = PumpingUnits[index + 2].AllRows.FirstOrDefault(x => x.InnerText == WellConfig.ModelConfig.Surface.PumpingUnit.Type);
            Assert.IsNotNull(selectmanufacturertype, "Unable to Find the Pumping unit type : " + WellConfig.ModelConfig.Surface.PumpingUnit.Type);
            selectmanufacturertype.ScrollToVisible();
            Thread.Sleep(2000);
            selectmanufacturertype.MouseClick();
            Thread.Sleep(5000);
            PumpingUnits[index + 4].Refresh();
            Assert.AreEqual(255, PumpingUnits[index + 4].AllRows.Count, "All manufacture items count not expected one for Lufkin - C");
            CommonHelper.TraceLine("Succesfully verified the Manufacturer Items count for : " + WellConfig.ModelConfig.Surface.PumpingUnit.Type);
            NonTelerikHtml.HtmlTableRow selectmanufacturertypeItem = PumpingUnits[index + 4].AllRows.FirstOrDefault(x => x.InnerText == WellConfig.ModelConfig.Surface.PumpingUnit.Description);
            Assert.IsNotNull(selectmanufacturertypeItem, "Unable to locate the Manufacturer Item : " + WellConfig.ModelConfig.Surface.PumpingUnit.Description);
            selectmanufacturertypeItem.ScrollToVisible();
            Thread.Sleep(2000);
            selectmanufacturertypeItem.MouseClick();
            Thread.Sleep(5000);
            NonTelerikHtml.HtmlButton apply = ForeSiteUIAutoManager.ActiveBrowser.Find.ByXPath<NonTelerikHtml.HtmlButton>("//button[text()='Apply']");
            apply.Click();
            ////ReadOnlyCollection<Element> applybuttony = ForeSiteUIAutoManager.ActiveBrowser.Find.AllByExpression("tagname=kendo-dialog-actions");
            ////ClientAutomation.Helper.CommonHelper.TraceLine("Button number of"+ applybuttony.Count);           
            ////ForeSiteUIAutoManager.ActiveBrowser.Actions.Click(applybuttony.First());            
            //selectPump.Refresh();
            // Assert.IsTrue(selectPump.IsEnabled, "Apply button is disabled even after selecting the Manufacturer Item");
            // selectPump.MouseClick();
            Thread.Sleep(3000);
            ForeSiteUIAutoManager.ActiveBrowser.RefreshDomTree();
            pumpText.Refresh();
            CommonHelper.TraceLine("Pumping Unit is set to : " + pumpText.Value);
            Assert.AreEqual(WellConfig.ModelConfig.Surface.PumpingUnit.Description, pumpText.Value, "Pumping Unit text should be empty");
            CommonHelper.TraceLine("Succesfully selected the Pumping Unit");
            DDL(ForeSiteUIAutoManager, "wristPin", WellConfig.ModelConfig.Surface.WristPinPosition.ToString());
            DDL(ForeSiteUIAutoManager, "rotation", WellConfig.ModelConfig.Surface.ClockwiseRotation.ToString());
            DDL(ForeSiteUIAutoManager, "motorInfo", WellConfig.ModelConfig.Surface.MotorType.Name);
            DDL(ForeSiteUIAutoManager, "motorSize", WellConfig.ModelConfig.Surface.MotorSize.SizeInHP.ToString(), true);
            DDL(ForeSiteUIAutoManager, "slipTorque", WellConfig.ModelConfig.Surface.SlipTorque.Rating.ToString());
            NonTelerikHtml.HtmlControl ampsUP = ForeSiteUIAutoManager.ActiveBrowser.Find.ById<NonTelerikHtml.HtmlControl>("motorAmpsUp");
            NonTelerikHtml.HtmlControl txt_Up = TelerikObject.GetChildrenControl(ampsUP, "0;0");
            txt_Up.MouseClick();
            clearTextBox();
            ForeSiteUIAutoManager.Desktop.KeyBoard.TypeText(WellConfig.ModelConfig.Surface.MotorAmpsUp.ToString(), 50);
            NonTelerikHtml.HtmlControl ampsDown = ForeSiteUIAutoManager.ActiveBrowser.Find.ById<NonTelerikHtml.HtmlControl>("motorAmpsDown");
            NonTelerikHtml.HtmlControl txt_Down = TelerikObject.GetChildrenControl(ampsDown, "0;0");
            txt_Down.MouseClick();
            clearTextBox();
            ForeSiteUIAutoManager.Desktop.KeyBoard.TypeText(WellConfig.ModelConfig.Surface.MotorAmpsDown.ToString(), 50);
            Thread.Sleep(3000);
            PrintScreen("VerifyUp&DownValue");
        }

        public void DDL(Manager ForeSiteUIAutoManager, string label, string value, bool scroll = false)
        {
            Thread.Sleep(5000);
            NonTelerikHtml.HtmlControl itemLabel = ForeSiteUIAutoManager.ActiveBrowser.Find.ByExpression<NonTelerikHtml.HtmlControl>("for=" + label);
            Assert.IsNotNull(itemLabel, "Failed to locate the " + label + " label");
            NonTelerikHtml.HtmlControl Type = itemLabel.BaseElement.GetNextSibling().As<NonTelerikHtml.HtmlControl>().Find.ByExpression<NonTelerikHtml.HtmlContainerControl>("class=k-i-arrow-s k-icon");
            Assert.IsNotNull(Type, "Failed to locate the " + label + " DDL");
            Type.MouseClick();
            //Waiting for 5 seconds for DOM to get Refreshed
            Thread.Sleep(5000);
            ForeSiteUIAutoManager.ActiveBrowser.RefreshDomTree();
            IEnumerable<NonTelerikHtml.HtmlListItem> allistitems = ForeSiteUIAutoManager.ActiveBrowser.Find.AllControls<NonTelerikHtml.HtmlListItem>();
            NonTelerikHtml.HtmlListItem TypeItem = null;
            foreach (NonTelerikHtml.HtmlListItem indlistitem in allistitems)
            {
                if (indlistitem.InnerText.Equals(value))
                {
                    TypeItem = indlistitem;
                    CommonHelper.TraceLine("Surely ListItem is not Null");
                    break;
                }
            }
            Assert.IsNotNull(TypeItem, value + " for " + label + " is not available inside the dropdown");
            if (scroll)
            {
                TypeItem.ScrollToVisible();
                Thread.Sleep(1000);
            }
            TypeItem.MouseClick();
            CommonHelper.TraceLine(DateTime.Now + label + " is set");
            Thread.Sleep(1000);
        }

        public void Configuration_Weights_UI(Manager ForeSiteUIAutoManager, WellConfigDTO WellConfig)
        {
            //Refreshing Dom Tree & Giving static wait
            ForeSiteUIAutoManager.ActiveBrowser.RefreshDomTree();
            Thread.Sleep(10000);
            //Weights
            NonTelerikHtml.HtmlDiv weights = ForeSiteUIAutoManager.ActiveBrowser.Find.ByContent<NonTelerikHtml.HtmlDiv>("Weights");
            Thread.Sleep(2000);
            Assert.IsNotNull(weights, "Unable to Locate the Weights tab");
            weights.Click();
            Thread.Sleep(2000);
            CommonHelper.TraceLine("Clicked on well configuration - Weights");
            NonTelerikHtml.HtmlControl labelCBTMethod = ForeSiteUIAutoManager.ActiveBrowser.Find.ByContent<NonTelerikHtml.HtmlControl>("Torque Calculation Method");
            NonTelerikHtml.HtmlControl CBTMethod = labelCBTMethod.BaseElement.GetNextSibling().As<NonTelerikHtml.HtmlControl>().Find.ByExpression<NonTelerikHtml.HtmlControl>("class=k-input");
            Assert.AreEqual("API", CBTMethod.ChildNodes[2].Content, "CBT Method API is not shown as default");
            DDL(ForeSiteUIAutoManager, "crankId", WellConfig.ModelConfig.Weights.CrankId, true);
            Thread.Sleep(2000);
            ForeSiteUIAutoManager.ActiveBrowser.RefreshDomTree();
            //Adding Lead Id and Lag Id for 1st Crank
            DDL(ForeSiteUIAutoManager, "crank1LeadPrimaryId", WellConfig.ModelConfig.Weights.Crank_1_Primary.LeadId, true);
            DDL(ForeSiteUIAutoManager, "crank1LagPrimaryId", WellConfig.ModelConfig.Weights.Crank_1_Primary.LagId, true);
            //Adding Lead Id and Lag Id for 2nd Crank
            DDL(ForeSiteUIAutoManager, "crank2LeadPrimaryId", WellConfig.ModelConfig.Weights.Crank_2_Primary.LeadId, true);
            DDL(ForeSiteUIAutoManager, "crank2LagPrimaryId", WellConfig.ModelConfig.Weights.Crank_2_Primary.LagId, true);
            NonTelerikHtml.HtmlButton CBT = ForeSiteUIAutoManager.ActiveBrowser.Find.ById<NonTelerikHtml.HtmlButton>("cbtCalcButton");
            Assert.IsNotNull(CBT, "Failed to Locate the Calculate CBT button");
            CBT.MouseClick();
            Thread.Sleep(2000);
            ForeSiteUIAutoManager.ActiveBrowser.RefreshDomTree();
            NonTelerikHtml.HtmlControl CBTMethodDDL = ForeSiteUIAutoManager.ActiveBrowser.Find.ById<NonTelerikHtml.HtmlControl>("torqueMethod");
            Assert.IsNotNull(CBTMethodDDL, "Failed to Locate the DDL for CBT method");
            CBTMethodDDL.MouseClick();
            CommonHelper.TraceLine("Clicked on CBT DDL");
            Thread.Sleep(2000);
            TelerikObject.Select_KendoUI_Listitem("Mills");
            CBTMethodDDL.Refresh();
            ForeSiteUIAutoManager.ActiveBrowser.RefreshDomTree();
            Assert.AreEqual("Mills", CBTMethodDDL.BaseElement.ChildNodes[1].InnerText, "CBT Method API is not shown as default");
            CommonHelper.TraceLine("CBT method check is completed");
        }

        public void InputField(Manager Manager, string label, string value)
        {
            NonTelerikHtml.HtmlControl inputLabel = Manager.ActiveBrowser.Find.ByExpression<NonTelerikHtml.HtmlControl>("for=" + label);
            Assert.IsNotNull(inputLabel, "Failed to Locate : " + label);
            NonTelerikHtml.HtmlControl inputField = inputLabel.BaseElement.GetNextSibling().As<NonTelerikHtml.HtmlControl>();
            Assert.IsNotNull(inputField, "Failed to click inside the input field : " + label);
            inputField.MouseClick();
            clearTextBox();
            Manager.ActiveBrowser.Desktop.KeyBoard.TypeText(value, 400);
            CommonHelper.TraceLine(DateTime.Now + label + " is set");
            Thread.Sleep(2000);
        }

        public void Configuration_Downhole_UI(Manager ForeSiteUIAutoManager, WellConfigDTO WellConfig)
        {
            ForeSiteUIAutoManager.ActiveBrowser.RefreshDomTree();
            Thread.Sleep(10000);
            //DownHole
            NonTelerikHtml.HtmlDiv Downhole = ForeSiteUIAutoManager.ActiveBrowser.Find.ByContent<NonTelerikHtml.HtmlDiv>("Downhole");
            Assert.IsNotNull(Downhole, "Unable to Locate the Downhole tab");
            Downhole.Click();
            CommonHelper.TraceLine("Clicked on well configuration - Downhole");
            Thread.Sleep(1000);
            InputField(ForeSiteUIAutoManager, "PumpDepth", WellConfig.ModelConfig.Downhole.PumpDepth.ToString());
            InputField(ForeSiteUIAutoManager, "TubingOD", WellConfig.ModelConfig.Downhole.TubingOD.ToString());
            InputField(ForeSiteUIAutoManager, "TubingID", WellConfig.ModelConfig.Downhole.TubingID.ToString());
            InputField(ForeSiteUIAutoManager, "TubingAnchorDepth", WellConfig.ModelConfig.Downhole.TubingAnchorDepth.ToString());
            InputField(ForeSiteUIAutoManager, "CasingOD", WellConfig.ModelConfig.Downhole.CasingOD.ToString());
            InputField(ForeSiteUIAutoManager, "CasingWeight", WellConfig.ModelConfig.Downhole.CasingWeight.ToString());
            InputField(ForeSiteUIAutoManager, "TopPerforation", WellConfig.ModelConfig.Downhole.TopPerforation.ToString());
            InputField(ForeSiteUIAutoManager, "BottomPerforation", WellConfig.ModelConfig.Downhole.BottomPerforation.ToString());
            DDL(ForeSiteUIAutoManager, "pumpDiameter", WellConfig.ModelConfig.Downhole.PumpDiameter.ToString(), true);
            Thread.Sleep(2000);
        }

        public void SelectRodItemsDDL(Manager ForeSiteUIAutoManager, NonTelerikHtml.HtmlTableCell item, string value, bool scroll, bool keyboard = false)
        {
            //Sometimes dropdown is taking more time to load values hence Adding static wait time from 5s to 9s 
            Thread.Sleep(9000);
            NonTelerikHtml.HtmlControl itemDDL = item.Find.ByExpression<NonTelerikHtml.HtmlControl>("class=k-i-arrow-s k-icon");
            Assert.IsNotNull(itemDDL, "Failed to locate DDL for the " + value);
            itemDDL.MouseClick();
            Thread.Sleep(2000);
            ForeSiteUIAutoManager.ActiveBrowser.RefreshDomTree();
            if (keyboard)
            {
                ForeSiteUIAutoManager.Desktop.KeyBoard.KeyPress(System.Windows.Forms.Keys.Down);
                ForeSiteUIAutoManager.Desktop.KeyBoard.KeyPress(System.Windows.Forms.Keys.Down);
                ForeSiteUIAutoManager.Desktop.KeyBoard.KeyPress(System.Windows.Forms.Keys.Enter);
            }
            else
            {
                NonTelerikHtml.HtmlListItem TypeItem = ForeSiteUIAutoManager.ActiveBrowser.Find.ByContent<NonTelerikHtml.HtmlListItem>(value);
                Assert.IsNotNull(TypeItem, "Failed to locate DDL value : " + value);
                if (scroll)
                {
                    TypeItem.ScrollToVisible();
                    Thread.Sleep(1000);
                }
                TypeItem.MouseClick();
            }
            CommonHelper.TraceLine(DateTime.Now + value + " is set");
            Thread.Sleep(2000);
        }

        public void InputRodItems(Manager ForeSiteUIAutoManager, NonTelerikHtml.HtmlTableCell item, string value)
        {
            Assert.IsNotNull(item, "Failed to locate input field for " + value);
            //Increated static wait time to 2s
            Thread.Sleep(2000);
            NonTelerikHtml.HtmlControl itemInput = item.Find.ByExpression<NonTelerikHtml.HtmlControl>("class=k-input k-formatted-value");
            Assert.IsNotNull(itemInput, "Failed to locate input field");
            ForeSiteUIAutoManager.ActiveBrowser.Actions.SetText(itemInput, "");
            Thread.Sleep(2000);
            itemInput.MouseClick();
            Thread.Sleep(2000);
            ForeSiteUIAutoManager.ActiveBrowser.Desktop.KeyBoard.TypeText(value, 200);
            CommonHelper.TraceLine(DateTime.Now + value + " is set");
            Thread.Sleep(2000);
        }

        public void Configuration_Rods_UI(Manager ForeSiteUIAutoManager, WellConfigDTO WellConfig)
        {
            ForeSiteUIAutoManager.ActiveBrowser.RefreshDomTree();
            Thread.Sleep(4000);
            NonTelerikHtml.HtmlDiv Rods = ForeSiteUIAutoManager.ActiveBrowser.Find.ByContent<NonTelerikHtml.HtmlDiv>("Rods");
            Assert.IsNotNull(Rods, "Unable to Locate the Rods tab");
            Rods.Click();
            CommonHelper.TraceLine("Clicked on well configuration - Rods");
            Thread.Sleep(2000);
            int NumTapers = WellConfig.ModelConfig.Rods.RodTapers.Length;
            CommonHelper.TraceLine(DateTime.Now.ToString() + NumTapers + " found in  rod DTO data");
            ForeSiteUIAutoManager.ActiveBrowser.RefreshDomTree();
            Thread.Sleep(2000);

            NonTelerikHtml.HtmlInputControl totalRodLength = ForeSiteUIAutoManager.ActiveBrowser.Find.ById<NonTelerikHtml.HtmlInputControl>("totalRodLength");
            CommonHelper.TraceLine("Initial Total rod length : " + totalRodLength.Value.ToString());
            Assert.AreEqual("0", totalRodLength.Value, "Incorrect initial rod length value");
            Thread.Sleep(2000);
            PrintScreen("VerifyPumpDepthValue");
            ReadOnlyCollection<NonTelerikHtml.HtmlTable> gridRods = ForeSiteUIAutoManager.ActiveBrowser.Find.AllByExpression<NonTelerikHtml.HtmlTable>("tagname=table");
            int index = gridRods.IndexOf(gridRods.FirstOrDefault(x => x.InnerText.Contains("Taper #")));
            NonTelerikHtml.HtmlSpan addRod = ForeSiteUIAutoManager.ActiveBrowser.Find.ByContent<NonTelerikHtml.HtmlSpan>("Add");
            Assert.IsNotNull(addRod, "Failed to locate the Add Rod button");
            Assert.AreEqual("No records available.", gridRods[index + 1].InnerText);
            for (int i = 0; i < WellConfig.ModelConfig.Rods.RodTapers.Count(); i++)
            {
                addRod.MouseClick();
                CommonHelper.TraceLine("No data check completed for Rods tab");
                CommonHelper.TraceLine("Clicked on Add Rod button");
                Thread.Sleep(1000);
                ForeSiteUIAutoManager.ActiveBrowser.RefreshDomTree();
                //Identifying Cancel Rod control
                NonTelerikHtml.HtmlButton cancelRod = ForeSiteUIAutoManager.ActiveBrowser.Find.ByExpression<NonTelerikHtml.HtmlButton>("class=grid-command-button k-button k-grid-cancel-command");
                Assert.IsNotNull(cancelRod, "Failed to locate cancel button in the grid while adding rod");
                Assert.IsTrue(cancelRod.IsEnabled, "Cancel button on the grid should be enabled");
                CommonHelper.TraceLine("Enable check for the Cancel button is completed");
                gridRods[index + 1].Refresh();
                NonTelerikHtml.HtmlTableCell manufacturer = gridRods[index + 1].AllRows[0].Cells[2];
                SelectRodItemsDDL(ForeSiteUIAutoManager, manufacturer, WellConfig.ModelConfig.Rods.RodTapers[i].Manufacturer, true);
                NonTelerikHtml.HtmlTableCell grade = gridRods[index + 1].AllRows[0].Cells[3];
                SelectRodItemsDDL(ForeSiteUIAutoManager, grade, WellConfig.ModelConfig.Rods.RodTapers[i].Grade, true);
                NonTelerikHtml.HtmlTableCell size = gridRods[index + 1].AllRows[0].Cells[4];
                // SelectRodItemsDDL(ForeSiteUIAutoManager, size, WellConfig.ModelConfig.Rods.RodTapers[i].Size.ToString(), true);//
                //Since SelectRodItemsDDL is not working properly for Size DD using Select_KendoUI_ListItem method
                TelerikObject.Click(size);
                CommonHelper.TraceLine("Clicked on Size DDL");
                Thread.Sleep(2000);
                TelerikObject.Select_KendoUI_Listitem(WellConfig.ModelConfig.Rods.RodTapers[i].Size.ToString());
                NonTelerikHtml.HtmlTableCell length = gridRods[index + 1].AllRows[0].Cells[5];
                InputRodItems(ForeSiteUIAutoManager, length, WellConfig.ModelConfig.Rods.RodTapers[i].RodLength.ToString());
                NonTelerikHtml.HtmlTableCell rodCount = gridRods[index + 1].AllRows[0].Cells[6];
                InputRodItems(ForeSiteUIAutoManager, rodCount, WellConfig.ModelConfig.Rods.RodTapers[i].NumberOfRods.ToString());
                Thread.Sleep(2000);
                ForeSiteUIAutoManager.ActiveBrowser.RefreshDomTree();
                Thread.Sleep(4000);
                //Identifying Save Rod control
                NonTelerikHtml.HtmlButton saveRod = ForeSiteUIAutoManager.ActiveBrowser.Find.ByExpression<NonTelerikHtml.HtmlButton>("class=grid-command-button k-button k-grid-save-command");
                Assert.IsNotNull(saveRod, "Failed to locate Save button int the grid while adding rod");
                Assert.IsTrue(saveRod.IsEnabled, "Save button on the grid should be enabled");
                CommonHelper.TraceLine("Enable check for the Save button is completed");
                saveRod.MouseClick();
                Thread.Sleep(5000);
                CommonHelper.TraceLine("Clicked on Save Rod");
                CommonHelper.TraceLine("Rod " + (i + 1) + " is added successfully");
                addRod.Refresh();
                ForeSiteUIAutoManager.ActiveBrowser.RefreshDomTree();
                gridRods[index + 1].Refresh();
                Thread.Sleep(5000);
            }
            for (int i = 1; i < gridRods[index].AllRows[0].Cells.Count; i++)
            {
                Trace.Write(gridRods[index].AllRows[0].Cells[i].InnerText + " | ");
            }
            Trace.Write(Environment.NewLine);
            for (int i = 0; i < gridRods[index + 1].BodyRows.Count; i++)
            {
                for (int j = 1; j < gridRods[index + 1].BodyRows[i].Cells.Count; j++)
                {
                    Trace.Write(gridRods[index + 1].BodyRows[i].Cells[j].InnerText + " | ");
                }
                Trace.Write(Environment.NewLine);
            }
            Trace.Write(Environment.NewLine);
            CommonHelper.TraceLine("No. of grid header cells : " + gridRods[index].AllRows[0].Cells.Count.ToString());
            CommonHelper.TraceLine("No. of grid content cells : " + gridRods[index + 1].BodyRows[0].Cells.Count.ToString());
            CommonHelper.TraceLine("No. of Rods added : " + gridRods[index + 1].BodyRows.Count.ToString());
            totalRodLength.Refresh();
            CommonHelper.TraceLine("Final Total rod length : " + totalRodLength.Value.ToString());
            Assert.AreEqual(gridRods[index].AllRows[0].Cells.Count, gridRods[index + 1].BodyRows[0].Cells.Count, "Mismatch between the grid header and content cells count");
            Assert.AreEqual(WellConfig.ModelConfig.Rods.RodTapers.Count(), gridRods[index + 1].BodyRows.Count, "Rods not added successfully");
            ForeSiteUIAutoManager.ActiveBrowser.RefreshDomTree();
            NonTelerikHtml.HtmlSpan updateRod = ForeSiteUIAutoManager.ActiveBrowser.Find.ByExpression<NonTelerikHtml.HtmlSpan>("class=k-icon k-i-pencil");
            Assert.IsNotNull(updateRod, "Failed to locate update button in the grid while adding rod");
            Assert.IsTrue(updateRod.IsEnabled, "update button on the grid should be enabled");
            CommonHelper.TraceLine("Enable check for the Update Rod button is completed");
            NonTelerikHtml.HtmlSpan removeRod = ForeSiteUIAutoManager.ActiveBrowser.Find.ByExpression<NonTelerikHtml.HtmlSpan>("class=k-icon k-i-close");
            Assert.IsNotNull(removeRod, "Failed to locate removeRod button in the grid while adding rod");
            Assert.IsTrue(removeRod.IsEnabled, "removeRod button on the grid should be enabled");
            CommonHelper.TraceLine("Enable check for the removeRod button is completed");
            CommonHelper.TraceLine("Rods tab check completed successfully");
        }

        public void SaveWell(Manager ForeSiteUIAutoManager, WellConfigDTO WellConfig)
        {
            NonTelerikHtml.HtmlButton saveWell = ForeSiteUIAutoManager.ActiveBrowser.Find.ByName<NonTelerikHtml.HtmlButton>("saveWellbutton");
            Assert.IsNotNull(saveWell, "Failed to locate save well button");
            saveWell.MouseClick();
            CommonHelper.TraceLine("Clicked on Save well button");
            Thread.Sleep(30000);
            ForeSiteUIAutoManager.ActiveBrowser.RefreshDomTree();
            NonTelerikHtml.HtmlUnorderedList tabs = ForeSiteUIAutoManager.ActiveBrowser.Find.ByExpression<NonTelerikHtml.HtmlUnorderedList>("role=tablist");
            int tabCount = WellConfig.Well.WellType == WellTypeId.RRL ? 6 : 3;
            Assert.AreEqual(tabCount, tabs.AllItems.Count(), "Only General and Well Attributes should be visible");
            CommonHelper.TraceLine("Well saved succesfully");
        }

        public void Verify_Configuration_Modeldata_UI(Manager ForeSiteUIAutoManager, WellConfigDTO WellConfig, string unit)
        {
            ForeSiteUIAutoManager.ActiveBrowser.RefreshDomTree();
            Thread.Sleep(3000);
            NonTelerikHtml.HtmlSpan modelData = ForeSiteUIAutoManager.ActiveBrowser.Find.ByContent<NonTelerikHtml.HtmlSpan>("Model Data");
            Assert.IsNotNull(modelData, "Unable to Locate the Model Data tab");
            modelData.Click();
            CommonHelper.TraceLine("Clicked on well configuration - Model Data");
            Thread.Sleep(1000);
            CommonHelper.TraceLine("Starting " + WellConfig.Well.Name + " Model Data validation with Unit System : " + unit);
            switch (WellConfig.Well.WellType)
            {
                case WellTypeId.RRL:

                    break;

                case WellTypeId.ESP:
                    ESPWellModelDataVerification(unit);
                    break;

                case WellTypeId.GLift:
                    GLWellModelDataVerification(unit);
                    break;

                case WellTypeId.NF:
                    NFWWellModelDataVerification(unit);
                    break;

                case WellTypeId.WInj:
                    WIWellModelDataVerification(unit);
                    break;

                case WellTypeId.GInj:
                    GIWellModelDataVerification(unit);
                    break;

                case WellTypeId.PLift:
                    PLWellModelDataVerification(unit);
                    break;

                default:
                    CommonHelper.TraceLine("Well Type Not expected");
                    break;
            }

        }

        public void Verify_Configuration_General_UI(Manager ForeSiteUIAutoManager, WellConfigDTO WellConfig)
        {
            CommonHelper.TraceLine("$$$$$$$$$$$Verification of well configuration screen started$$$$$$$$$$$");

            Thread.Sleep(5000);
            NonTelerikHtml.HtmlDiv well_Configuration = ForeSiteUIAutoManager.ActiveBrowser.Find.ByContent<NonTelerikHtml.HtmlDiv>("Well Configuration");
            well_Configuration.Click();
            CommonHelper.TraceLine("Clicked on well configuration");
            Thread.Sleep(5000);

            ForeSiteUIAutoManager.ActiveBrowser.RefreshDomTree();
            NonTelerikHtml.HtmlDiv general = ForeSiteUIAutoManager.ActiveBrowser.Find.ByContent<NonTelerikHtml.HtmlDiv>("General");
            Assert.IsNotNull(general, "Unable to Identify the General tab");
            general.Click();

            NonTelerikHtml.HtmlInputControl wellName = ForeSiteUIAutoManager.ActiveBrowser.Find.ById<NonTelerikHtml.HtmlInputControl>("wellName");
            Assert.IsNotNull(wellName, "Failed to Locate WellName input field");
            Assert.AreEqual(WellConfig.Well.Name, wellName.Value, "Well Name displayed is not matching with the well name entered ");
            CommonHelper.TraceLine("Displayed Well name is " + wellName.Value);

            ForeSiteUIAutoManager.ActiveBrowser.RefreshDomTree();
            Thread.Sleep(1000);
            NonTelerikHtml.HtmlSpan wellTypeSelected = ForeSiteUIAutoManager.ActiveBrowser.Find.ByContent<NonTelerikHtml.HtmlSpan>(WellConfig.Well.Name);
            Assert.IsNotNull(wellTypeSelected, "Failed to select the " + WellConfig.Well.Name + " from the well type dropdown");
            CommonHelper.TraceLine("Well Type is set to : " + wellTypeSelected.InnerText);

            WellTypeId wType = WellConfig.Well.WellType;
            //Verifying displayed Fluid type value if Well Type is Gas lift or Plunger Lift
            if (wType == WellTypeId.GLift)
            {
                NonTelerikHtml.HtmlControl fluidType = TelerikObject.GetChildrenControl(PageObjects.PageWellConfig.txt_FluidType, "0;0;0;0");
                CommonHelper.TraceLine("Displayed Fluid Type value is: " + fluidType.BaseElement.InnerText);
                Assert.AreEqual("Black Oil", fluidType.BaseElement.InnerText, "Displayed Fluid Type value is not matching with the entered one");
            }
            else if (wType == WellTypeId.PLift)
            {
                NonTelerikHtml.HtmlControl fluidType = TelerikObject.GetChildrenControl(PageObjects.PageWellConfig.txt_FluidType, "0;0;0;0");
                CommonHelper.TraceLine("Displayed Fluid Type value is: " + fluidType.BaseElement.InnerText);
                Assert.AreEqual("Dry Gas", fluidType.BaseElement.InnerText, "Displayed Fluid Type value is not matching with the entered one");
            }

            NonTelerikHtml.HtmlControl scadaType = TelerikObject.GetChildrenControl(PageObjects.PageWellConfig.txt_ScadaType, "0;0;0;0");
            CommonHelper.TraceLine("Displayed Scada Type value is: " + scadaType.BaseElement.InnerText);
            Assert.AreEqual("CygNet", scadaType.BaseElement.InnerText, "Displayed Scada Type value is not matching with the entered one");

            Thread.Sleep(2000);
            ForeSiteUIAutoManager.ActiveBrowser.RefreshDomTree();
            NonTelerikHtml.HtmlSpan cygnetDomainSelected = ForeSiteUIAutoManager.ActiveBrowser.Find.ByContent<NonTelerikHtml.HtmlSpan>(WellConfig.Well.DataConnection.ProductionDomain);
            Assert.IsNotNull(cygnetDomainSelected, "Cygnet Domain displayed in the Cygnet Domain dropdown is not matching with Cygnet Domain selected from the dropdown");
            CommonHelper.TraceLine("CygNet doamin is set to : " + cygnetDomainSelected.InnerText);

            Thread.Sleep(2000);
            ForeSiteUIAutoManager.ActiveBrowser.RefreshDomTree();
            NonTelerikHtml.HtmlSpan selectedSiteService = ForeSiteUIAutoManager.ActiveBrowser.Find.ByContent<NonTelerikHtml.HtmlSpan>(WellConfig.Well.DataConnection.Site + ".UIS");
            Assert.IsNotNull(selectedSiteService, "Cygnet Site Service displayed in the Cygnet Site Service dropdown is not matching with Cygnet Site Service selected from the dropdown");
            CommonHelper.TraceLine("CygNet site service is set to : " + selectedSiteService.InnerText);

            DateTime dtCommissionDateTime = Convert.ToDateTime(WellConfig.Well.CommissionDate);
            string sCommissionDateTime = dtCommissionDateTime.ToString("MM/dd/yyyy");
            NonTelerikHtml.HtmlControl commissionDate = ForeSiteUIAutoManager.ActiveBrowser.Find.ByExpression<NonTelerikHtml.HtmlControl>("aria-valuetext=" + sCommissionDateTime);
            Assert.IsFalse(commissionDate.IsEnabled, "Commission date field is enabled");
            CommonHelper.TraceLine("Commission date field is enabled? " + commissionDate.IsEnabled);
            Assert.IsNotNull(commissionDate, "Commission date displayed is not correct");

            //Verifying data into WellboreId tab
            NonTelerikHtml.HtmlInputControl wellboreId = ForeSiteUIAutoManager.ActiveBrowser.Find.ById<NonTelerikHtml.HtmlInputControl>("assemblyAPI");
            Assert.AreEqual(WellConfig.Well.AssemblyAPI, wellboreId.Value.ToString(), "Wellbore Id value displayed on screen is not matching with the entered value");
            CommonHelper.TraceLine("Wellbore Id value is : " + wellboreId.Value.ToString());


            //Verifying data into BoreholeId tab
            NonTelerikHtml.HtmlInputControl boreholeId = ForeSiteUIAutoManager.ActiveBrowser.Find.ById<NonTelerikHtml.HtmlInputControl>("subAssemblyAPI");
            Assert.AreEqual(WellConfig.Well.SubAssemblyAPI, boreholeId.Value.ToString(), "Borehole Id value displayed on screen is not matching with the entered value");
            CommonHelper.TraceLine("Wellbore Id value is : " + boreholeId.Value.ToString());

            //Verifying data into Perforation Interval tab
            NonTelerikHtml.HtmlInputControl PerfInterval = ForeSiteUIAutoManager.ActiveBrowser.Find.ById<NonTelerikHtml.HtmlInputControl>("intervalAPI");
            Assert.AreEqual(WellConfig.Well.IntervalAPI, PerfInterval.Value.ToString(), "Perforation Interval value displayed on screen is not matching with the entered value");
            CommonHelper.TraceLine("Perforation Interval value is : " + PerfInterval.Value.ToString());

            NonTelerikHtml.HtmlControl latitude = ForeSiteUIAutoManager.ActiveBrowser.Find.ByExpression<NonTelerikHtml.HtmlControl>("aria-valuenow=" + WellConfig.Well.SurfaceLatitude.Value.ToString());
            Assert.IsNotNull(latitude, "Latitude value displayed is not correct");
            CommonHelper.TraceLine("Displayed latitude value is " + latitude.Attributes.FirstOrDefault(x => x.Name == "aria-valuenow").Value);

            Thread.Sleep(1000);

            NonTelerikHtml.HtmlControl longitude = TelerikObject.GetChildrenControl(PageObjects.PageWellConfig.txt_Longitude, "0;0;0;0");
            CommonHelper.TraceLine("Displayed Longitude value is: " + longitude.BaseElement.Attributes.FirstOrDefault(x => x.Name == "aria-valuenow").Value);
            Assert.AreEqual(WellConfig.Well.SurfaceLongitude.Value.ToString(), longitude.BaseElement.Attributes.FirstOrDefault(x => x.Name == "aria-valuenow").Value, "Displayed Longitude value is not matching with the entered one");
        }

        public void Verify_Configuration_WellAttributes_UI(Manager ForeSiteUIAutoManager, WellConfigDTO WellConfig)
        {
            NonTelerikHtml.HtmlDiv wellAttributes = ForeSiteUIAutoManager.ActiveBrowser.Find.ByContent<NonTelerikHtml.HtmlDiv>("Well Attributes");
            Assert.IsNotNull(wellAttributes, "Unable to Locate the Well Attributes tab");
            wellAttributes.Click();
            CommonHelper.TraceLine("Clicked on well configuration - well attributes");

            NonTelerikHtml.HtmlControl leaseName = TelerikObject.GetChildrenControl(PageObjects.PageWellConfig.txt_LeaseName, "0;0;0;0");
            CommonHelper.TraceLine("Displayed lease name value is: " + Extract_Value_FromProperty(leaseName));
            Assert.AreEqual(WellConfig.Well.Lease, Extract_Value_FromProperty(leaseName), "Displayed Lease Name value is not matching with the entered one");

            NonTelerikHtml.HtmlControl fieldName = TelerikObject.GetChildrenControl(PageObjects.PageWellConfig.txt_fieldName, "0;0;0;0");
            CommonHelper.TraceLine("Displayed field name value is: " + Extract_Value_FromProperty(fieldName));
            Assert.AreEqual(WellConfig.Well.Field, Extract_Value_FromProperty(fieldName), "Displayed Field Name value is not matching with the entered one");

            NonTelerikHtml.HtmlControl enggName = TelerikObject.GetChildrenControl(PageObjects.PageWellConfig.txt_Engineer, "0;0;0;0");
            CommonHelper.TraceLine("Displayed Engineer name value is: " + Extract_Value_FromProperty(enggName));
            Assert.AreEqual(WellConfig.Well.Engineer, Extract_Value_FromProperty(enggName), "Displayed Engineer Name value is not matching with the entered one");

            NonTelerikHtml.HtmlControl geoRegion = TelerikObject.GetChildrenControl(PageObjects.PageWellConfig.txt_GeoRegion, "0;0;0;0");
            CommonHelper.TraceLine("Displayed Grographic Region value is: " + Extract_Value_FromProperty(geoRegion));
            Assert.AreEqual(WellConfig.Well.GeographicRegion, Extract_Value_FromProperty(geoRegion), "Displayed Geographic Region value is not matching with the entered one");

            NonTelerikHtml.HtmlControl foremanName = TelerikObject.GetChildrenControl(PageObjects.PageWellConfig.txt_Foreman, "0;0;0;0");
            CommonHelper.TraceLine("Displayed Foreman name value is: " + Extract_Value_FromProperty(foremanName));
            Assert.AreEqual(WellConfig.Well.Foreman, Extract_Value_FromProperty(foremanName), "Displayed Foreman Name value is not matching with the entered one");

            NonTelerikHtml.HtmlControl gaugerBeat = TelerikObject.GetChildrenControl(PageObjects.PageWellConfig.txt_GaugerBeat, "0;0;0;0");
            CommonHelper.TraceLine("Displayed Engineer name value is: " + Extract_Value_FromProperty(gaugerBeat));
            Assert.AreEqual(WellConfig.Well.GaugerBeat, Extract_Value_FromProperty(gaugerBeat), "Displayed Gauger Beat value is not matching with the entered one");

        }
        public void Verify_Configuration_Surface_UI(Manager ForeSiteUIAutoManager, WellConfigDTO WellConfig)
        {
            Thread.Sleep(3000);
            NonTelerikHtml.HtmlDiv surface = ForeSiteUIAutoManager.ActiveBrowser.Find.ByContent<NonTelerikHtml.HtmlDiv>("Surface");
            Assert.IsNotNull(surface, "Unable to Locate the Surface tab");
            surface.Click();
            CommonHelper.TraceLine("Clicked on well configuration - surface");

            NonTelerikHtml.HtmlInputControl pumpText = ForeSiteUIAutoManager.ActiveBrowser.Find.ById<NonTelerikHtml.HtmlInputControl>("pumpingUnit");
            Assert.IsNotNull(pumpText, "Unable to locate Pumping Unit text field");
            Assert.AreEqual(WellConfig.ModelConfig.Surface.PumpingUnit.Description, pumpText.Value, "Displayed Pumping Unit Description is incorrect");
            CommonHelper.TraceLine("Displayed Pumping Description is " + pumpText.Value);

            NonTelerikHtml.HtmlControl wristPinLabel = ForeSiteUIAutoManager.ActiveBrowser.Find.ByExpression<NonTelerikHtml.HtmlControl>("for=wristPin");
            Thread.Sleep(2000);
            ForeSiteUIAutoManager.ActiveBrowser.RefreshDomTree();
            Thread.Sleep(1000);
            NonTelerikHtml.HtmlControl wristPinValue = wristPinLabel.BaseElement.GetNextSibling().As<NonTelerikHtml.HtmlControl>();
            CommonHelper.TraceLine("Displayed Wrist Pin Value is " + wristPinValue.BaseElement.InnerText);
            Assert.AreEqual(WellConfig.ModelConfig.Surface.WristPinPosition.ToString(), wristPinValue.BaseElement.InnerText, "Displayed Wrist Pin value is not matching with the entered one");

            NonTelerikHtml.HtmlControl actualStrokeLength = ForeSiteUIAutoManager.ActiveBrowser.Find.ByExpression<NonTelerikHtml.HtmlControl>("id=actualStrokeLength");
            CommonHelper.TraceLine("Displayed ActualStrokeLength value is " + actualStrokeLength.BaseElement.ChildNodes[1].ChildNodes[0].Attributes.FirstOrDefault(x => x.Name == "aria-valuenow").Value);
            Assert.AreEqual(WellConfig.ModelConfig.Surface.ActualStrokeLength.ToString(), actualStrokeLength.BaseElement.ChildNodes[1].ChildNodes[0].Attributes.FirstOrDefault(x => x.Name == "aria-valuenow").Value.ToString(), "Displayed Actual Stroke Length value is not matching with the enetred one");

            NonTelerikHtml.HtmlControl rotationDD = TelerikObject.GetChildrenControl(PageObjects.PageWellConfig.dd_Rotation, "0;0;0");
            CommonHelper.TraceLine("Displayed Rotation value is: " + rotationDD.BaseElement.InnerText);
            Assert.AreEqual(WellConfig.ModelConfig.Surface.ClockwiseRotation.ToString(), rotationDD.BaseElement.InnerText, "Displayed Rotation value is not matching with the entered one");

            NonTelerikHtml.HtmlControl motorTypeDD = TelerikObject.GetChildrenControl(PageObjects.PageWellConfig.dd_MotorType, "0;0;0");
            CommonHelper.TraceLine("Displayed Motor Type value is: " + motorTypeDD.BaseElement.InnerText);
            Assert.AreEqual(WellConfig.ModelConfig.Surface.MotorType.Name, motorTypeDD.BaseElement.InnerText, "Displayed Motor Type value is not matching with the entered one");

            NonTelerikHtml.HtmlControl motorSizeDD = TelerikObject.GetChildrenControl(PageObjects.PageWellConfig.dd_MotorSize, "0;0;0;0");
            CommonHelper.TraceLine("Displayed Motor Size value is: " + motorSizeDD.BaseElement.InnerText);
            Assert.AreEqual(WellConfig.ModelConfig.Surface.MotorSize.Size.ToString(), motorSizeDD.BaseElement.InnerText, "Displayed Motor Size value is not matching with the entered one");

            //NonTelerikHtml.HtmlControl slipTorqueDD = TelerikObject.GetChildrenControl(PageObjects.PageWellConfig.dd_SlipTorque, "0;0;0;0");
            CommonHelper.TraceLine("Displayed Slip Torque value is: " + PageObjects.PageWellConfig.dd_SlipTorque.BaseElement.InnerText);
            Assert.AreEqual(WellConfig.ModelConfig.Surface.SlipTorque.Rating.ToString(), PageObjects.PageWellConfig.dd_SlipTorque.BaseElement.InnerText, "Displayed Slip Torque value is not matching with the entered one");

            NonTelerikHtml.HtmlControl motorAmpsUp = TelerikObject.GetChildrenControl(PageObjects.PageWellConfig.txt_Up, "0;0;0;0");
            CommonHelper.TraceLine("Displayed Motor Amps Up value is: " + motorAmpsUp.BaseElement.Attributes.FirstOrDefault(x => x.Name == "aria-valuenow").Value);
            Assert.AreEqual(WellConfig.ModelConfig.Surface.MotorAmpsUp.ToString(), motorAmpsUp.BaseElement.Attributes.FirstOrDefault(x => x.Name == "aria-valuenow").Value, "Displayed Motoe Amps Up value is not matching with the entered one");

            NonTelerikHtml.HtmlControl motorAmpsDown = TelerikObject.GetChildrenControl(PageObjects.PageWellConfig.txt_Down, "0;0;0;0");
            CommonHelper.TraceLine("Displayed Motor Amps Down value is: " + motorAmpsDown.BaseElement.Attributes.FirstOrDefault(x => x.Name == "aria-valuenow").Value);
            Assert.AreEqual(WellConfig.ModelConfig.Surface.MotorAmpsDown.ToString(), motorAmpsDown.BaseElement.Attributes.FirstOrDefault(x => x.Name == "aria-valuenow").Value, "Displayed Motoe Amps Down value is not matching with the entered one");

        }

        public void Verify_Configuration_Weights_UI(Manager ForeSiteUIAutoManager, WellConfigDTO WellConfig)
        {
            Thread.Sleep(3000);
            NonTelerikHtml.HtmlDiv weights = ForeSiteUIAutoManager.ActiveBrowser.Find.ByContent<NonTelerikHtml.HtmlDiv>("Weights");
            Assert.IsNotNull(weights, "Unable to Locate the Weights tab");
            weights.Click();
            CommonHelper.TraceLine("Clicked on well configuration - Weights");

            NonTelerikHtml.HtmlControl crankIdDD = TelerikObject.GetChildrenControl(PageObjects.PageWellConfig.dd_CrankId, "0;0;0");
            CommonHelper.TraceLine("Displayed Crank Id value is: " + crankIdDD.BaseElement.InnerText);
            Assert.AreEqual(WellConfig.ModelConfig.Weights.CrankId, crankIdDD.BaseElement.InnerText, "Displayed CrankId value is not matching with the entered one");

            NonTelerikHtml.HtmlControl crank1LeadPrimaryIdDD = TelerikObject.GetChildrenControl(PageObjects.PageWellConfig.dd_Crank1LeadPrimaryId, "0;0;0");
            CommonHelper.TraceLine("Displayed Crank1 Lead PrimaryId value is: " + crank1LeadPrimaryIdDD.BaseElement.InnerText);
            Assert.AreEqual(WellConfig.ModelConfig.Weights.Crank_1_Primary.LeadId, crank1LeadPrimaryIdDD.BaseElement.InnerText, "Displayed Crank1 Lead Primary Id value is not matching with the entered one");

            NonTelerikHtml.HtmlControl crank1LagPrimaryIdDD = TelerikObject.GetChildrenControl(PageObjects.PageWellConfig.dd_Crank1LagPrimaryId, "0;0;0");
            CommonHelper.TraceLine("Displayed Crank1 Lag PrimaryId value is: " + crank1LagPrimaryIdDD.BaseElement.InnerText);
            Assert.AreEqual(WellConfig.ModelConfig.Weights.Crank_1_Primary.LagId, crank1LagPrimaryIdDD.BaseElement.InnerText, "Displayed Crank1 Lag Primary Id value is not matching with the entered one");

            NonTelerikHtml.HtmlControl crank2LeadPrimaryIdDD = TelerikObject.GetChildrenControl(PageObjects.PageWellConfig.dd_Crank2LeadPrimaryId, "0;0;0");
            CommonHelper.TraceLine("Displayed Crank2 Lead PrimaryId value is: " + crank2LeadPrimaryIdDD.BaseElement.InnerText);
            Assert.AreEqual(WellConfig.ModelConfig.Weights.Crank_2_Primary.LeadId, crank2LeadPrimaryIdDD.BaseElement.InnerText, "Displayed Crank2 Lead Primary Id value is not matching with the entered one");

            NonTelerikHtml.HtmlControl crank2LagPrimaryIdDD = TelerikObject.GetChildrenControl(PageObjects.PageWellConfig.dd_Crank2LagPrimaryId, "0;0;0");
            CommonHelper.TraceLine("Displayed Crank2 Lag PrimaryId value is: " + crank2LagPrimaryIdDD.BaseElement.InnerText);
            Assert.AreEqual(WellConfig.ModelConfig.Weights.Crank_2_Primary.LagId, crank2LagPrimaryIdDD.BaseElement.InnerText, "Displayed Crank2 Lag Primary Id value is not matching with the entered one");

            NonTelerikHtml.HtmlControl crank1LeadDistance = TelerikObject.GetChildrenControl(PageObjects.PageWellConfig.txt_Crank1LeadDistance, "0;0");
            CommonHelper.TraceLine("Displayed Crank1 Lead distance value is: " + crank1LeadDistance.BaseElement.Attributes.FirstOrDefault(x => x.Name == "aria-valuenow").Value);
            Assert.AreEqual("0", crank1LeadDistance.BaseElement.Attributes.FirstOrDefault(x => x.Name == "aria-valuenow").Value, "Displayed Crank1 Lead Distance value is not matching with the entered one");

            NonTelerikHtml.HtmlControl crank1LagDistance = TelerikObject.GetChildrenControl(PageObjects.PageWellConfig.txt_Crank1LagDistance, "0;0");
            CommonHelper.TraceLine("Displayed Crank1 Lag distance value is: " + crank1LagDistance.BaseElement.Attributes.FirstOrDefault(x => x.Name == "aria-valuenow").Value);
            Assert.AreEqual("0", crank1LagDistance.BaseElement.Attributes.FirstOrDefault(x => x.Name == "aria-valuenow").Value, "Displayed Crank1 Lag Distance value is not matching with the entered one");

            NonTelerikHtml.HtmlControl crank2LeadDistance = TelerikObject.GetChildrenControl(PageObjects.PageWellConfig.txt_Crank2LeadDistance, "0;0");
            CommonHelper.TraceLine("Displayed Crank2 Lead distance value is: " + crank2LeadDistance.BaseElement.Attributes.FirstOrDefault(x => x.Name == "aria-valuenow").Value);
            Assert.AreEqual("0", crank2LeadDistance.BaseElement.Attributes.FirstOrDefault(x => x.Name == "aria-valuenow").Value, "Displayed Crank2 Lead Distance value is not matching with the entered one");

            NonTelerikHtml.HtmlControl crank2LagDistance = TelerikObject.GetChildrenControl(PageObjects.PageWellConfig.txt_Crank2LagDistance, "0;0");
            CommonHelper.TraceLine("Displayed Crank2 Lag distance value is: " + crank2LagDistance.BaseElement.Attributes.FirstOrDefault(x => x.Name == "aria-valuenow").Value);
            Assert.AreEqual("0", crank2LagDistance.BaseElement.Attributes.FirstOrDefault(x => x.Name == "aria-valuenow").Value, "Displayed Crank2 Lag Distance value is not matching with the entered one");

            NonTelerikHtml.HtmlControl crank1LeadAuxiliaryIdDD = TelerikObject.GetChildrenControl(PageObjects.PageWellConfig.dd_crank1LeadAuxiliaryId, "0;0;0");
            CommonHelper.TraceLine("Displayed Crank1 Lead AuxiliaryId value is: " + crank1LeadAuxiliaryIdDD.BaseElement.InnerText);
            Assert.AreEqual("None", crank1LeadAuxiliaryIdDD.BaseElement.InnerText, "Displayed Crank1 Lead Auxiliary Id value is not matching with the entered one");

            NonTelerikHtml.HtmlControl crank1LagAuxiliaryIdDD = TelerikObject.GetChildrenControl(PageObjects.PageWellConfig.dd_crank1LagAuxiliaryId, "0;0;0");
            CommonHelper.TraceLine("Displayed Crank1 Lag AuxiliaryId value is: " + crank1LagAuxiliaryIdDD.BaseElement.InnerText);
            Assert.AreEqual("None", crank1LagAuxiliaryIdDD.BaseElement.InnerText, "Displayed Crank1 Lag Auxiliary Id value is not matching with the entered one");

            NonTelerikHtml.HtmlControl crank2LeadAuxiliaryIdDD = TelerikObject.GetChildrenControl(PageObjects.PageWellConfig.dd_crank2LeadAuxiliaryId, "0;0;0");
            CommonHelper.TraceLine("Displayed Crank2 Lead AuxiliaryId value is: " + crank2LeadAuxiliaryIdDD.BaseElement.InnerText);
            Assert.AreEqual("None", crank2LeadAuxiliaryIdDD.BaseElement.InnerText, "Displayed Crank2 Lead Auxiliary Id value is not matching with the entered one");

            NonTelerikHtml.HtmlControl crank2LagAuxiliaryIdDD = TelerikObject.GetChildrenControl(PageObjects.PageWellConfig.dd_crank2LagAuxiliaryId, "0;0;0");
            CommonHelper.TraceLine("Displayed Crank2 Lag AuxiliaryId value is: " + crank2LagAuxiliaryIdDD.BaseElement.InnerText);
            Assert.AreEqual("None", crank2LagAuxiliaryIdDD.BaseElement.InnerText, "Displayed Crank2 Lag Auxiliary Id value is not matching with the entered one");

            NonTelerikHtml.HtmlControl txtCBT = TelerikObject.GetChildrenControl(PageObjects.PageWellConfig.txt_CBT, "0;0");
            CommonHelper.TraceLine("Displayed CBT value is: " + txtCBT.BaseElement.Attributes.FirstOrDefault(x => x.Name == "aria-valuenow").Value);
            Assert.AreEqual("1169.06", txtCBT.BaseElement.Attributes.FirstOrDefault(x => x.Name == "aria-valuenow").Value, "CBT value calculation is incorrect");

            NonTelerikHtml.HtmlControl torqueCalcMethod = TelerikObject.GetChildrenControl(PageObjects.PageWellConfig.dd_TorqueCalcMethod, "0;0");
            CommonHelper.TraceLine("Displayed Torque calculation method is: " + torqueCalcMethod.BaseElement.InnerText);
            Assert.AreEqual("Mills", torqueCalcMethod.BaseElement.InnerText, "CBT value calculation is incorrect");

        }
        public void Verify_Configuration_Downhole_UI(Manager ForeSiteUIAutoManager, WellConfigDTO WellConfig)
        {
            ForeSiteUIAutoManager.ActiveBrowser.RefreshDomTree();
            Thread.Sleep(3000);
            NonTelerikHtml.HtmlDiv Downhole = ForeSiteUIAutoManager.ActiveBrowser.Find.ByContent<NonTelerikHtml.HtmlDiv>("Downhole");
            Assert.IsNotNull(Downhole, "Unable to Locate the Downhole tab");
            Downhole.Click();
            CommonHelper.TraceLine("Clicked on well configuration - Downhole");
            Thread.Sleep(1000);

            NonTelerikHtml.HtmlControl pumpDiameter = TelerikObject.GetChildrenControl(PageObjects.PageWellConfig.txt_PumpDiameter, "0;0;0;0");
            CommonHelper.TraceLine("Displayed Pump Diameter value is: " + pumpDiameter.BaseElement.InnerText);
            Assert.AreEqual(WellConfig.ModelConfig.Downhole.PumpDiameter.ToString(), pumpDiameter.BaseElement.InnerText, "Displayed PumpDiameter value is not matching with the entered one");

            NonTelerikHtml.HtmlControl pumpDepth = TelerikObject.GetChildrenControl(PageObjects.PageWellConfig.txt_PumpDepth, "0;0;0;0");
            CommonHelper.TraceLine("Displayed Pump Depth value is: " + pumpDepth.BaseElement.Attributes.FirstOrDefault(x => x.Name == "aria-valuenow").Value);
            Assert.AreEqual(WellConfig.ModelConfig.Downhole.PumpDepth.ToString(), pumpDepth.BaseElement.Attributes.FirstOrDefault(x => x.Name == "aria-valuenow").Value, "Displayed Pump depth value is not matching with the entered one");

            NonTelerikHtml.HtmlControl tubingOD = TelerikObject.GetChildrenControl(PageObjects.PageWellConfig.txt_TubingOD, "0;0;0;0");
            CommonHelper.TraceLine("Displayed Tubing OD value is: " + tubingOD.BaseElement.Attributes.FirstOrDefault(x => x.Name == "aria-valuenow").Value);
            Assert.AreEqual(WellConfig.ModelConfig.Downhole.TubingOD.ToString(), tubingOD.BaseElement.Attributes.FirstOrDefault(x => x.Name == "aria-valuenow").Value, "Displayed Tubing OD value is not matching with the entered one");

            NonTelerikHtml.HtmlControl tubingID = TelerikObject.GetChildrenControl(PageObjects.PageWellConfig.txt_TubingID, "0;0;0;0");
            CommonHelper.TraceLine("Displayed Tubing ID value is: " + tubingID.BaseElement.Attributes.FirstOrDefault(x => x.Name == "aria-valuenow").Value);
            Assert.AreEqual(WellConfig.ModelConfig.Downhole.TubingID.ToString(), tubingID.BaseElement.Attributes.FirstOrDefault(x => x.Name == "aria-valuenow").Value, "Displayed Tubing ID value is not matching with the entered one");

            NonTelerikHtml.HtmlControl tubingAnchorDepth = TelerikObject.GetChildrenControl(PageObjects.PageWellConfig.txt_TubingAnchorDepth, "0;0;0;0");
            CommonHelper.TraceLine("Displayed Tubing Anchor Depth value is: " + tubingAnchorDepth.BaseElement.Attributes.FirstOrDefault(x => x.Name == "aria-valuenow").Value);
            Assert.AreEqual(WellConfig.ModelConfig.Downhole.TubingAnchorDepth.ToString(), tubingAnchorDepth.BaseElement.Attributes.FirstOrDefault(x => x.Name == "aria-valuenow").Value, "Displayed Tubing Anchor Depth value is not matching with the entered one");

            NonTelerikHtml.HtmlControl casingOD = TelerikObject.GetChildrenControl(PageObjects.PageWellConfig.txt_CasingOD, "0;0;0;0");
            CommonHelper.TraceLine("Displayed Casing OD value is: " + casingOD.BaseElement.Attributes.FirstOrDefault(x => x.Name == "aria-valuenow").Value);
            Assert.AreEqual(WellConfig.ModelConfig.Downhole.CasingOD.ToString(), casingOD.BaseElement.Attributes.FirstOrDefault(x => x.Name == "aria-valuenow").Value, "Displayed Casing OD value is not matching with the entered one");

            NonTelerikHtml.HtmlControl casingWeight = TelerikObject.GetChildrenControl(PageObjects.PageWellConfig.txt_CasingWeight, "0;0;0;0");
            CommonHelper.TraceLine("Displayed Casing Weight value is: " + casingWeight.BaseElement.Attributes.FirstOrDefault(x => x.Name == "aria-valuenow").Value);
            Assert.AreEqual(WellConfig.ModelConfig.Downhole.CasingWeight.ToString(), casingWeight.BaseElement.Attributes.FirstOrDefault(x => x.Name == "aria-valuenow").Value, "Displayed Casing Weight value is not matching with the entered one");

            NonTelerikHtml.HtmlControl topPerforation = TelerikObject.GetChildrenControl(PageObjects.PageWellConfig.txt_TopPerforation, "0;0;0;0");
            CommonHelper.TraceLine("Displayed Top Perforation value is: " + topPerforation.BaseElement.Attributes.FirstOrDefault(x => x.Name == "aria-valuenow").Value);
            Assert.AreEqual(WellConfig.ModelConfig.Downhole.TopPerforation.ToString(), topPerforation.BaseElement.Attributes.FirstOrDefault(x => x.Name == "aria-valuenow").Value, "Displayed Top Perforation value is not matching with the entered one");

            NonTelerikHtml.HtmlControl bottomPerforation = TelerikObject.GetChildrenControl(PageObjects.PageWellConfig.txt_BottomPerforation, "0;0;0;0");
            CommonHelper.TraceLine("Displayed Bottom Perforation value is: " + bottomPerforation.BaseElement.Attributes.FirstOrDefault(x => x.Name == "aria-valuenow").Value);
            Assert.AreEqual(WellConfig.ModelConfig.Downhole.BottomPerforation.ToString(), bottomPerforation.BaseElement.Attributes.FirstOrDefault(x => x.Name == "aria-valuenow").Value, "Displayed Bottom Perforation value is not matching with the entered one");

        }
        public void Verify_Configuration_Rods_UI(Manager ForeSiteUIAutoManager, WellConfigDTO WellConfig)
        {
            ForeSiteUIAutoManager.ActiveBrowser.RefreshDomTree();
            Thread.Sleep(3000);
            NonTelerikHtml.HtmlDiv Rods = ForeSiteUIAutoManager.ActiveBrowser.Find.ByContent<NonTelerikHtml.HtmlDiv>("Rods");
            Assert.IsNotNull(Rods, "Unable to Locate the Rods tab");
            Rods.Click();
            CommonHelper.TraceLine("Clicked on well configuration - Rods");
            Thread.Sleep(1000);

            //NonTelerikHtml.HtmlInputControl pumpDepth = ForeSiteUIAutoManager.ActiveBrowser.Find.ById<NonTelerikHtml.HtmlInputControl>("pumpDepth");
            //ClientAutomation.Helper.CommonHelper.TraceLine("Pump Depth : " + pumpDepth.Value.ToString());

            NonTelerikHtml.HtmlInputControl totalRodLength = ForeSiteUIAutoManager.ActiveBrowser.Find.ById<NonTelerikHtml.HtmlInputControl>("totalRodLength");
            CommonHelper.TraceLine("Total rod length : " + totalRodLength.Value.ToString());

            for (int i = 0; i < WellConfig.ModelConfig.Rods.RodTapers.Length; i++)
            {
                VerifyRodTableCellValues(WellConfig, i);
            }

        }

        public static void VerifyRodTableCellValues(WellConfigDTO WellConfig, int rowindex)
        {
            string colnames = "Taper #;Manufacturer;Grade;Size;Length;Rod Count;Total Length;Guides;Damping Down;Damping Up;Service Factor";
            string action = "ignorevalue";
            string taperNo = (rowindex + 1).ToString();

            string manufacturer = WellConfig.ModelConfig.Rods.RodTapers.ElementAt(rowindex).Manufacturer;
            string grade = WellConfig.ModelConfig.Rods.RodTapers.ElementAt(rowindex).Grade;
            string size = WellConfig.ModelConfig.Rods.RodTapers.ElementAt(rowindex).Size.ToString();
            string rodLength = WellConfig.ModelConfig.Rods.RodTapers.ElementAt(rowindex).RodLength.ToString();
            string rodCount = WellConfig.ModelConfig.Rods.RodTapers.ElementAt(rowindex).NumberOfRods.ToString();

            string totalLength = (WellConfig.ModelConfig.Rods.RodTapers.ElementAt(rowindex).RodLength * WellConfig.ModelConfig.Rods.RodTapers.ElementAt(rowindex).NumberOfRods).ToString();

            string guides = WellConfig.ModelConfig.Rods.RodTapers.ElementAt(rowindex).RodGuid;
            string dampingUp = "ignorevalue";
            string dampingDown = "ignorevalue";
            string serviceFactor = WellConfig.ModelConfig.Rods.RodTapers.ElementAt(rowindex).ServiceFactor.ToString();

            string colvals = action + ";" + taperNo + ";" + manufacturer +
                             ";" + grade + ";" + size + ";" + rodLength +
                             ";" + rodCount + ";" + totalLength + ";" + guides + ";" + dampingUp + ";" + dampingDown + ";" + serviceFactor;

            TelerikObject.verifyGridCellValues(colnames, colvals, rowindex);
        }

        public static void ESPWellModelDataVerification(string unit)
        {
            Thread.Sleep(2000);
            PageObjects.PageWellConfig.CollapseAllSectionsOnModelData();

            //Verifying Fluid data
            PageObjects.PageWellConfig.VerifyFluidData("ESPGLModelData.xml", "ESP", unit);

            //Verifying Reservoir data
            PageObjects.PageWellConfig.VerifyReservoirData("ESPGLModelData.xml", "ESP", unit);

            //verifying Trajectory data
            PageObjects.PageWellConfig.VerifyTrajectoryData("ESPGLTrajectoryData.xml", unit);

            //Verifying Tubing data
            TelerikObject.Click(PageObjects.PageWellConfig.tab_TubingData);
            PageObjects.PageWellConfig.VerifyTubingCasingData("ESPGLTubingData.xml", unit);
            TelerikObject.Click(PageObjects.PageWellConfig.tab_TubingData);

            //Verifying Casing data
            TelerikObject.Click(PageObjects.PageWellConfig.tab_CasingData);
            PageObjects.PageWellConfig.VerifyTubingCasingData("ESPGLCasingData.xml", unit);
            TelerikObject.Click(PageObjects.PageWellConfig.tab_CasingData);

            //Verifying Restriction data
            TelerikObject.Click(PageObjects.PageWellConfig.tab_RestrictionData);
            PageObjects.PageWellConfig.VerifyRestrictionData();
            TelerikObject.Click(PageObjects.PageWellConfig.tab_RestrictionData);


            //Verifying Trace Points data
            TelerikObject.Click(PageObjects.PageWellConfig.tab_TracePointsData);
            PageObjects.PageWellConfig.VerifyTracePointsData();
            TelerikObject.Click(PageObjects.PageWellConfig.tab_TracePointsData);


            //Verifying ESP Data
            //TelerikObject.Click(TelerikObject.GetChildrenControl(PageObjects.PageWellConfig.tab_ESPData, "0"));
            DataTable dTable = PageObjects.PageWellConfig.createDatatableFromXML("ESPGLModelData.xml");
            DataTable dt3 = PageObjects.PageWellConfig.createDatatableFromXML("ESPDataPump.xml");
            for (int i = 0; i < dt3.Rows.Count; i++)
            { PageObjects.PageWellConfig.VerifyESPPumpData(dt3, i); }
            PageObjects.PageWellConfig.VerifyFieldUnitandValue(dTable, PageObjects.PageWellConfig.txt_MotorModel, "Motor Model", unit);
            PageObjects.PageWellConfig.VerifyFieldUnitandValue(dTable, PageObjects.PageWellConfig.txt_MeasuredDepth, "Measured Depth", unit);
            PageObjects.PageWellConfig.VerifyFieldUnitandValue(dTable, PageObjects.PageWellConfig.txt_NameplateRating, "Nameplate Rating", unit);
            PageObjects.PageWellConfig.VerifyFieldUnitandValue(dTable, PageObjects.PageWellConfig.txt_OperatingRating, "Operating Rating", unit);
            PageObjects.PageWellConfig.VerifyFieldUnitandValue(dTable, PageObjects.PageWellConfig.txt_OperatingFrequency, "Operating Frequency", unit);
            PageObjects.PageWellConfig.VerifyFieldUnitandValue(dTable, PageObjects.PageWellConfig.txt_MotorWearFactor, "Motor Wear Factor", unit);
            PageObjects.PageWellConfig.VerifyFieldUnitandValue(dTable, PageObjects.PageWellConfig.txt_CableSize, "Cable Size", unit);
            PageObjects.PageWellConfig.VerifyFieldUnitandValue(dTable, PageObjects.PageWellConfig.txt_GasSeparatorPresent, "Gas Separator Present", unit);
            PageObjects.PageWellConfig.VerifyFieldUnitandValue(dTable, PageObjects.PageWellConfig.txt_SeparatorEfficiency, "Separator Efficiency", unit);
            // TelerikObject.Click(TelerikObject.GetChildrenControl(PageObjects.PageWellConfig.tab_ESPData, "0"));

        }

        public static void GLWellModelDataVerification(string unit)
        {
            Thread.Sleep(2000);
            PageObjects.PageWellConfig.CollapseAllSectionsOnModelData();

            //Verifying Fluid data
            PageObjects.PageWellConfig.VerifyFluidData("ESPGLModelData.xml", "GL", unit);

            //Verifying Reservoir data
            PageObjects.PageWellConfig.VerifyReservoirData("ESPGLModelData.xml", "GL", unit);

            //verifying Trajectory data
            PageObjects.PageWellConfig.VerifyTrajectoryData("ESPGLTrajectoryData.xml", unit);

            //Verifying Tubing data
            TelerikObject.Click(PageObjects.PageWellConfig.tab_TubingData);
            PageObjects.PageWellConfig.VerifyTubingCasingData("ESPGLTubingData.xml", unit);
            TelerikObject.Click(PageObjects.PageWellConfig.tab_TubingData);

            //Verifying Casing data
            TelerikObject.Click(PageObjects.PageWellConfig.tab_CasingData);
            PageObjects.PageWellConfig.VerifyTubingCasingData("ESPGLCasingData.xml", unit);
            TelerikObject.Click(PageObjects.PageWellConfig.tab_CasingData);

            //Verifying Restriction data
            TelerikObject.Click(PageObjects.PageWellConfig.tab_RestrictionData);
            PageObjects.PageWellConfig.VerifyRestrictionData();
            TelerikObject.Click(PageObjects.PageWellConfig.tab_RestrictionData);

            //Verifying Trace Points data
            TelerikObject.Click(PageObjects.PageWellConfig.tab_TracePointsData);
            PageObjects.PageWellConfig.VerifyTracePointsData();
            TelerikObject.Click(PageObjects.PageWellConfig.tab_TracePointsData);

            //Verifying GL Data
            //TelerikObject.Click(TelerikObject.GetChildrenControl(PageObjects.PageWellConfig.tab_GasLiftData, "0"));
            PageObjects.PageWellConfig.VerifyGasLiftData();
            //TelerikObject.Click(TelerikObject.GetChildrenControl(PageObjects.PageWellConfig.tab_GasLiftData, "0"));

        }

        public static void NFWWellModelDataVerification(string unit)
        {
            Thread.Sleep(2000);
            PageObjects.PageWellConfig.CollapseAllSectionsOnModelData();

            //Verifying Fluid data
            PageObjects.PageWellConfig.VerifyFluidData("NFWModelData.xml", "NFW", unit);

            //Verifying Reservoir data
            PageObjects.PageWellConfig.VerifyReservoirData("NFWModelData.xml", "NFW", unit);

            //verifying Trajectory data
            PageObjects.PageWellConfig.VerifyTrajectoryData("NFWTrajectoryData.xml", unit);

            //Verifying Tubing data
            TelerikObject.Click(PageObjects.PageWellConfig.tab_TubingData);
            PageObjects.PageWellConfig.VerifyTubingCasingData("NFWTubingData.xml", unit);
            TelerikObject.Click(PageObjects.PageWellConfig.tab_TubingData);

            //Verifying Casing data
            TelerikObject.Click(PageObjects.PageWellConfig.tab_CasingData);
            PageObjects.PageWellConfig.VerifyTubingCasingData("NFWCasingData.xml", unit);
            TelerikObject.Click(PageObjects.PageWellConfig.tab_CasingData);

            //Verifying Restriction data
            TelerikObject.Click(PageObjects.PageWellConfig.tab_RestrictionData);
            PageObjects.PageWellConfig.VerifyRestrictionData();
            TelerikObject.Click(PageObjects.PageWellConfig.tab_RestrictionData);

            //Verifying Trace Points data
            TelerikObject.Click(PageObjects.PageWellConfig.tab_TracePointsData);
            PageObjects.PageWellConfig.VerifyTracePointsData();
            TelerikObject.Click(PageObjects.PageWellConfig.tab_TracePointsData);
        }

        public static void WIWellModelDataVerification(string unit)
        {
            Thread.Sleep(2000);
            PageObjects.PageWellConfig.CollapseAllSectionsOnModelData();

            //Verifying Fluid data
            PageObjects.PageWellConfig.VerifyFluidData("WIModelData.xml", "WI", unit);

            //Verifying Reservoir data
            PageObjects.PageWellConfig.VerifyReservoirData("WIModelData.xml", "WI", unit);

            //verifying Trajectory data
            PageObjects.PageWellConfig.VerifyTrajectoryData("WITrajectoryData.xml", unit);

            //Verifying Tubing data
            TelerikObject.Click(PageObjects.PageWellConfig.tab_TubingData);
            PageObjects.PageWellConfig.VerifyTubingCasingData("WITubingData.xml", unit);
            TelerikObject.Click(PageObjects.PageWellConfig.tab_TubingData);

            //Verifying Casing data
            TelerikObject.Click(PageObjects.PageWellConfig.tab_CasingData);
            PageObjects.PageWellConfig.VerifyTubingCasingData("WICasingData.xml", unit);
            TelerikObject.Click(PageObjects.PageWellConfig.tab_CasingData);

            //Verifying Restriction data
            TelerikObject.Click(PageObjects.PageWellConfig.tab_RestrictionData);
            PageObjects.PageWellConfig.VerifyWIRestrictionData("WIRestrictionData.xml", unit);
            TelerikObject.Click(PageObjects.PageWellConfig.tab_RestrictionData);

            //Verifying Trace Points data
            TelerikObject.Click(PageObjects.PageWellConfig.tab_TracePointsData);
            PageObjects.PageWellConfig.VerifyTracePointsData();
            TelerikObject.Click(PageObjects.PageWellConfig.tab_TracePointsData);
        }

        public static void GIWellModelDataVerification(string unit)
        {
            Thread.Sleep(2000);
            PageObjects.PageWellConfig.CollapseAllSectionsOnModelData();

            //Verifying Fluid data
            PageObjects.PageWellConfig.VerifyFluidData("GIModelData.xml", "GI", unit);

            //Verifying Reservoir data
            PageObjects.PageWellConfig.VerifyReservoirData("GIModelData.xml", "GI", unit);

            //verifying Trajectory data
            PageObjects.PageWellConfig.VerifyTrajectoryData("GITrajectoryData.xml", unit);

            //Verifying Tubing data
            TelerikObject.Click(PageObjects.PageWellConfig.tab_TubingData);
            PageObjects.PageWellConfig.VerifyTubingCasingData("GITubingData.xml", unit);
            TelerikObject.Click(PageObjects.PageWellConfig.tab_TubingData);

            //Verifying Casing data
            TelerikObject.Click(PageObjects.PageWellConfig.tab_CasingData);
            PageObjects.PageWellConfig.VerifyTubingCasingData("GICasingData.xml", unit);
            TelerikObject.Click(PageObjects.PageWellConfig.tab_CasingData);

            //Verifying Restriction data
            TelerikObject.Click(PageObjects.PageWellConfig.tab_RestrictionData);
            PageObjects.PageWellConfig.VerifyRestrictionData();
            TelerikObject.Click(PageObjects.PageWellConfig.tab_RestrictionData);

            //Verifying Trace Points data
            TelerikObject.Click(PageObjects.PageWellConfig.tab_TracePointsData);
            PageObjects.PageWellConfig.VerifyTracePointsData();
            TelerikObject.Click(PageObjects.PageWellConfig.tab_TracePointsData);
        }

        public static void PLWellModelDataVerification(string unit)
        {
            Thread.Sleep(2000);
            PageObjects.PageWellConfig.CollapseAllSectionsOnModelData();

            //Verifying Fluid data
            PageObjects.PageWellConfig.VerifyFluidData("PLModelData.xml", "PL", unit);

            //Verifying Reservoir data
            PageObjects.PageWellConfig.VerifyReservoirData("PLModelData.xml", "PL", unit);

            //verifying Trajectory data
            PageObjects.PageWellConfig.VerifyTrajectoryData("PLTrajectoryData.xml", unit);

            //Verifying Tubing data
            TelerikObject.Click(PageObjects.PageWellConfig.tab_TubingData);
            PageObjects.PageWellConfig.VerifyTubingCasingData("PLTubingData.xml", unit);
            TelerikObject.Click(PageObjects.PageWellConfig.tab_TubingData);

            //Verifying Casing data
            TelerikObject.Click(PageObjects.PageWellConfig.tab_CasingData);
            PageObjects.PageWellConfig.VerifyTubingCasingData("PLCasingData.xml", unit);
            TelerikObject.Click(PageObjects.PageWellConfig.tab_CasingData);

            //Verifying Restriction data
            TelerikObject.Click(PageObjects.PageWellConfig.tab_RestrictionData);
            PageObjects.PageWellConfig.VerifyRestrictionData();
            TelerikObject.Click(PageObjects.PageWellConfig.tab_RestrictionData);

            //Verifying Trace Points data
            TelerikObject.Click(PageObjects.PageWellConfig.tab_TracePointsData);
            PageObjects.PageWellConfig.VerifyTracePointsData();
            TelerikObject.Click(PageObjects.PageWellConfig.tab_TracePointsData);


            //Verifying Plunger Lift Data
            DataTable dTable = PageObjects.PageWellConfig.createDatatableFromXML("PLModelData.xml");
            PageObjects.PageWellConfig.VerifyFieldUnitandValue(dTable, PageObjects.PageWellConfig.txt_BHADepth, "Bottom Hole Assembly(BHA) Depth(MD)", unit);

            int i = 0;
            foreach (DataRow drow in dTable.Rows)
            {
                string fieldName = dTable.Rows[i]["FieldName"].ToString();
                if (fieldName == "Sand/Wax")
                {
                    Assert.IsTrue(TelerikObject.GetRadioSelection(PageObjects.PageWellConfig.radio_sandWax, dTable.Rows[i]["Value" + unit].ToString()));
                    break;
                }
                i++;
            }

            PageObjects.PageWellConfig.VerifyFieldUnitandValue(dTable, PageObjects.PageWellConfig.txt_PlungerType, "Plunger Type", unit);
            PageObjects.PageWellConfig.VerifyFieldUnitandValue(dTable, PageObjects.PageWellConfig.txt_EstimatedFallRateInGas, "Estimated Fall Rate in Gas", unit);
            PageObjects.PageWellConfig.VerifyFieldUnitandValue(dTable, PageObjects.PageWellConfig.txt_EstimatedFallRateInLiquid, "Estimated Fall Rate in Liquid", unit);
            PageObjects.PageWellConfig.VerifyFieldUnitandValue(dTable, PageObjects.PageWellConfig.txt_IdealRiseRate, "Ideal Rise Rate", unit);
            PageObjects.PageWellConfig.VerifyFieldUnitandValue(dTable, PageObjects.PageWellConfig.txt_PressureRequiredToSurfacePlunger, "Pressure Required to surface Plunger", unit);


        }

        private static void clearTextBox()
        {
            Thread.Sleep(1000);
            System.Windows.Forms.SendKeys.SendWait("^(a)");
            Thread.Sleep(1000);
            System.Windows.Forms.SendKeys.SendWait("{Del}");
        }


        public string Extract_Value_FromProperty(NonTelerikHtml.HtmlControl Control1)
        {
            string attribValue = Control1.BaseElement.Attributes.FirstOrDefault(x => x.Name == "aria-activedescendant").Value;
            string[] attribValueArray = attribValue.Split('-');
            int arrayLength = attribValueArray.Length;
            return attribValueArray[arrayLength - 1];
        }

    }
}