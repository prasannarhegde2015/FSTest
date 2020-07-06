using ArtOfTest.WebAii.Controls.HtmlControls;
using ArtOfTest.WebAii.ObjectModel;
using ClientAutomation.TelerikCoreUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientAutomation.PageObjects
{
    class PageWellTest : PageMasterDefinition
    {
        #region ObjectProperties_AddWellTestDlg
        public static string DynamicValue = null;
        internal const string htmlcontrolparentnextsibling = "htmlcontrolparentnextsibling";

        public static HtmlButton btnCreateNewWellTest { get { return (HtmlButton)TelerikObject.GetElement("HtmlButton", "id", "createBtn", "Create New Well Test Button"); } }
        public static HtmlButton btnCancelWellTest { get { return (HtmlButton)TelerikObject.GetElement("HtmlButton", "id", "cancelBtn", "Cancel Well Test Button"); } }
        public static HtmlButton btnUpdateWellTest { get { return (HtmlButton)TelerikObject.GetElement("HtmlButton", "id", "updateBtn", "Update WellTest Button"); } }
        public static HtmlButton btnDeleteWellTest { get { return (HtmlButton)TelerikObject.GetElement("HtmlButton", "id", "deleteBtn", "Delete Well Test Button"); } }
        public static HtmlButton btnExportWellTest { get { return (HtmlButton)TelerikObject.GetElement("HtmlButton", "id", "btnExcelExport", "Export Well Test Button"); } }
        public static HtmlButton btnTuneWellTest { get { return (HtmlButton)TelerikObject.GetElement("HtmlButton", "id", "tuneBtn", "Tune Well Test Button"); } }

        public static HtmlControl txtTestDate { get { return (HtmlControl)TelerikObject.GetElement("htmlcontrolparentnextsibling", "expression", "innertext=Test Date;tagname=label", "Test date Control"); } }
        public static HtmlControl dd_TestTime { get { return TelerikObject.GetElement("kendo-dropdownlist", "id", "sampleTime", "Well Test Time"); } }
        public static HtmlControl txtTestduration { get { return (HtmlControl)TelerikObject.GetElement("kendo-numerictextbox", "id", "testDuration", "Test Duration"); } }

        public static HtmlControl dd_QualityCode { get { return TelerikObject.GetElement(htmlcontrolparentnextsibling, "expression", "innertext=Quality Code;tagname=label", "Quality Code"); } }
        public static HtmlControl txtOiRate { get { return TelerikObject.GetElement("kendo-numerictextbox", "id", "oilRate", "Oil Rate"); } }
        public static HtmlControl txtWaterRate { get { return TelerikObject.GetElement("kendo-numerictextbox", "id", "waterRate", "Water Rate"); } }
        public static HtmlControl txtGasRate { get { return TelerikObject.GetElement("kendo-numerictextbox", "id", "gasRate", "Gas Rate"); } }
        public static HtmlControl txt_OilGravity { get { return TelerikObject.GetElement("kendo-numerictextbox", "id", "oilGravity", "Oil Gravity"); } }
        public static HtmlControl txt_WaterGravity { get { return TelerikObject.GetElement("kendo-numerictextbox", "id", "waterGravity", "Water Gravity"); } }
        public static HtmlControl txt_GasGravity { get { return TelerikObject.GetElement("kendo-numerictextbox", "id", "gasGravity", "Gas Gravity"); } }
        public static HtmlControl txt_TubingHeadPressure { get { return TelerikObject.GetElement("kendo-numerictextbox", "id", "averageTubingPressure", "Tubing Head Pressure"); } }
        public static HtmlControl txt_TubingHeadTemperature { get { return TelerikObject.GetElement("kendo-numerictextbox", "id", "averageTubingTemperature", "Tubing Head Temperature"); } }


        public static HtmlControl txt_PumpDischargePressure { get { return TelerikObject.GetElement("kendo-numerictextbox", "id", "pumpDischargePressure", "Pump Discharge Pressure"); } }
        public static HtmlControl txt_Frequency { get { return TelerikObject.GetElement("kendo-numerictextbox", "id", "frequency", "Frequency"); } }
        public static HtmlControl txt_MotorVolts { get { return TelerikObject.GetElement("kendo-numerictextbox", "id", "motorVolts", "Motor Volts"); } }
        public static HtmlControl txt_MotoroAmps { get { return TelerikObject.GetElement("kendo-numerictextbox", "id", "motorCurrent", "Motor Current"); } }
        public static HtmlControl txt_FlowLinePressure { get { return TelerikObject.GetElement("kendo-numerictextbox", "id", "flowLinePressure", "Flow Line Pressure"); } }
        public static HtmlControl txt_SepraratorPressure { get { return TelerikObject.GetElement("kendo-numerictextbox", "id", "separatorPressure", "Separator Pressure"); } }
        public static HtmlControl txt_ChokeSize { get { return TelerikObject.GetElement("kendo-numerictextbox", "id", "chokeSize", "Choke Size"); } }



        public static HtmlControl txt_CasinggHeadPressure { get { return TelerikObject.GetElement("kendo-numerictextbox", "id", "averageCasingPressure", "Casing Head Pressure"); } }



        public static HtmlControl txt_AverageFAP { get { return TelerikObject.GetElement(CONTROLTYPE_KENDO_NUMERICTEXTBOX, "id", "averageFluidAbovePump", "Average Fluid Above Pump"); } }
        public static HtmlControl txt_RRLPIP { get { return TelerikObject.GetElement("kendo-numerictextbox", "id", "pumpIntakePressure", "Pump Intake Pressure"); } }

        public static HtmlControl txt_CasingHeadPressure { get { return TelerikObject.GetElement("kendo-numerictextbox", "id", "averageCasingPressure", "Casing Head Pressure"); } }

        public static HtmlControl txt_GasInjectionRate { get { return TelerikObject.GetElement("kendo-numerictextbox", "id", "gasInjectionRate", "Gas Injection Rate"); } }

        public static HtmlControl txt_DownholePressureGauge { get { return TelerikObject.GetElement("kendo-numerictextbox", "id", "gaugePressure", "Gauge Pressure"); } }
        public static HtmlControl txt_RRLSPM { get { return TelerikObject.GetElement("kendo-numerictextbox", "id", "strokePerMinute", "Strokes Per Minute"); } }
        public static HtmlControl txt_PumpHours { get { return TelerikObject.GetElement("kendo-numerictextbox", "id", "pumpingHours", "Pumping Hours"); } }
        public static HtmlControl txt_PumpEffeciceny { get { return TelerikObject.GetElement("kendo-numerictextbox", "id", "pumpEfficiency", "Pump Efficiency"); } }
        //    public static HtmlButton btn_SaveWellTest { get { return (HtmlButton)TelerikObject.GetElement("HtmlButton", "expression", "innertext=Save;classname=btn btn-primary", "Save Button"); } }


        #region UnitLablesDivControls
        public static HtmlControl divcontrolTestDuration { get { return TelerikObject.GetElement(htmlcontrolparentnextsibling, "expression", "innertext=Test Duration;tagname=label", "Test Duration Div"); } }
        public static HtmlControl divcontrolOilRate { get { return TelerikObject.GetElement(htmlcontrolparentnextsibling, "expression", "innertext=Oil Rate;tagname=label", "Oil Rate Div"); } }
        public static HtmlControl divcontrolWaterRate { get { return TelerikObject.GetElement(htmlcontrolparentnextsibling, "expression", "innertext=Water Rate;tagname=label", "Water Rate Div"); } }
        public static HtmlControl divcontrolGasRate { get { return TelerikObject.GetElement(htmlcontrolparentnextsibling, "expression", "innertext=Gas Rate;tagname=label", "Gas Rate Div"); } }
        public static HtmlControl divcontrolOilgravity { get { return TelerikObject.GetElement(htmlcontrolparentnextsibling, "expression", "innertext=Oil Gravity;tagname=label", "Oil GravityDiv"); } }
        public static HtmlControl divcontrolWaterGravity { get { return TelerikObject.GetElement(htmlcontrolparentnextsibling, "expression", "innertext=Water Gravity;tagname=label", "Water Gravity Div"); } }
        public static HtmlControl divcontrolGasGravity { get { return TelerikObject.GetElement(htmlcontrolparentnextsibling, "expression", "innertext=Gas Gravity;tagname=label", "Gas Gravity Div"); } }
        public static HtmlControl divcontrolTubingHeadPressure { get { return TelerikObject.GetElement(htmlcontrolparentnextsibling, "expression", "innertext=Tubing Head Pressure;tagname=label", "Tubing Head PressureDiv"); } }
        public static HtmlControl divcontrolTubingHeadTemperature { get { return TelerikObject.GetElement(htmlcontrolparentnextsibling, "expression", "innertext=Tubing Head Temperature;tagname=label", "Tubing Head Temperature Div"); } }
        public static HtmlControl divcontrolCasingHeadPressure { get { return TelerikObject.GetElement(htmlcontrolparentnextsibling, "expression", "innertext=Casing Head Pressure;tagname=label", "Casing Head Pressure Div"); } }
        public static HtmlControl divcontrolAverageFluidAbovePump { get { return TelerikObject.GetElement(htmlcontrolparentnextsibling, "expression", "innertext=Average Fluid Above Pump;tagname=label", "Average Fluid Above Pump Div"); } }
        public static HtmlControl divcontrolPumpIntakePressure { get { return TelerikObject.GetElement(htmlcontrolparentnextsibling, "expression", "innertext=Pump Intake Pressure;tagname=label", "Pump Intake Pressure Div"); } }
        public static HtmlControl divcontrolStrokesPerMinute { get { return TelerikObject.GetElement(htmlcontrolparentnextsibling, "expression", "innertext=Strokes Per Minute;tagname=label", "Strokes Per Minute Div"); } }

        //Gas Lift Specific
        //public static HtmlControl divcontrolCasingHeadPressure { get { return TelerikObject.GetElement(htmlcontrolparentnextsibling, "expression", "innertext=Casing Head Pressure;tagname=label", "Casing Head Pressure Div"); } }
        public static HtmlControl divcontrolGasInjectionRate { get { return TelerikObject.GetElement(htmlcontrolparentnextsibling, "expression", "innertext=Gas Injection Rate;tagname=label", "Gas Injection Rate Div"); } }
        public static HtmlControl divcontrolDownholePressureGauge { get { return TelerikObject.GetElement(htmlcontrolparentnextsibling, "expression", "innertext=Downhole Pressure Gauge;tagname=label", "Downhole Pressure Gauge Div"); } }


        public static HtmlControl divcontrolESPPDP { get { return TelerikObject.GetElement(htmlcontrolparentnextsibling, "expression", "innertext=Pump Discharge Pressure;tagname=label", "Pump Discharge Pressure Div"); } }
        public static HtmlControl divcontrolESPFrequency { get { return TelerikObject.GetElement(htmlcontrolparentnextsibling, "expression", "innertext=Frequency;tagname=label", "Frequency Div"); } }
        public static HtmlControl divcontrolSESPMotorVolts { get { return TelerikObject.GetElement(htmlcontrolparentnextsibling, "expression", "innertext=Motor Volts;tagname=label", "Motor Volts Div"); } }
        public static HtmlControl divcontrolSESPMotorAmps { get { return TelerikObject.GetElement(htmlcontrolparentnextsibling, "expression", "innertext=Motor Amps;tagname=label", "Motor Amps Div"); } }
        public static HtmlControl divcontrolFlowLinePressure { get { return TelerikObject.GetElement(htmlcontrolparentnextsibling, "expression", "innertext=Flow Line Pressure;tagname=label", "Flow Line Pressure Div"); } }
        public static HtmlControl divcontrolSeparatorPressure { get { return TelerikObject.GetElement(htmlcontrolparentnextsibling, "expression", "innertext=Separator Pressure;tagname=label", "Separator Pressure Div"); } }
        public static HtmlControl divcontrolChokeSize { get { return TelerikObject.GetElement(htmlcontrolparentnextsibling, "expression", "innertext=Choke Size;tagname=label", "Choke Sizee Div"); } }


        public static HtmlControl divcontrolPumpingHours { get { return TelerikObject.GetElement(htmlcontrolparentnextsibling, "expression", "innertext=Pumping Hours;tagname=label", "Pumping Hours Div"); } }
        public static HtmlControl divcontrolPumpEfficiency { get { return TelerikObject.GetElement(htmlcontrolparentnextsibling, "expression", "innertext=Pump Efficiency;tagname=label", "Pump Efficiency Div"); } }
        #endregion
        public static Element btn_SaveWellTest { get { return TelerikObject.GetElement("Xpath", "//button[contains(text(),'Save')]", "Save Button"); } }

        public static HtmlControl toasterControl { get { return TelerikObject.GetElement(CONTROLTYPE_FORESITETOASTERCONTROL, "", "", "Toaster Control"); } }
        public static HtmlControl cell_testdata { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLTABLECELL, "content", DynamicValue, "Html Cell For Test Date"); } }


        public static HtmlControl popup_Container { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "attribute", "class=k-content k-window-content k-dialog-content", "Html Pop up Container"); } }
        public static Element btnDeleteConfirmWellTest { get { return TelerikObject.GetElement("Xpath", "//button[contains(text(),'Yes, Delete This Well Test Data')]", "Well Test Delete Confirm"); } }

        public static HtmlControl lbl_TableRecordCount { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "attribute", "class=k-pager-info k-label ng-star-inserted", "Label Counter for Well Test"); } }
        #endregion


        public static void preWellTestAddcheckRRL()
        {
            checkforElementState(btnCancelWellTest, "Disabled", "Cancel Button");
            checkforElementState(btnUpdateWellTest, "Disabled", "Update WellTest Button");
            checkforElementState(btnDeleteWellTest, "Disabled", "Delete WellTest Button");
            checkforElementState(btnExportWellTest, "Enabled", "Export to Excel Button");
        }

        public static void preWellTestAddcheckNonRRL()
        {
            checkforElementState(btnCancelWellTest, "Disabled", "Cancel Button");
            checkforElementState(btnTuneWellTest, "Disabled", "Tune Button");
            checkforElementState(btnUpdateWellTest, "Disabled", "Update WellTest Button");
            checkforElementState(btnDeleteWellTest, "Disabled", "Delete WellTest Button");
            checkforElementState(btnExportWellTest, "Disabled", "Export to Excel Button");
        }

        public static void QEUpdatetAddcheck()
        {
            checkforElementState(btnCancelWellTest, "Enabled", "Cancel Button");
            checkforElementState(btnUpdateWellTest, "Enabled", "Update WellTest Button");
            checkforElementState(btnDeleteWellTest, "Disabled", "Delete WellTest Button");
            checkforElementState(btnExportWellTest, "Enabled", "Export to Excel Button");
        }
        public static void checkforElementState(HtmlControl ctl, string chk, string fieldname)
        {
            Helper.CommonHelper.TraceLine(string.Format("Chcking for State {0} for {1}", chk, fieldname));
            if (chk.ToUpper() == "ENABLED")
            {
                Assert.IsTrue(ctl.IsEnabled, string.Format("Filed Name {0} is Disabled ", fieldname));
            }
            else if (chk == "DISABLED")
            {
                Assert.IsFalse(ctl.IsEnabled, string.Format("Filed Name {0} is Enabled ", fieldname));
            }

        }
    }
}
