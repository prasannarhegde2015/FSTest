using Microsoft.VisualStudio.TestTools.UnitTesting;
using ArtOfTest.WebAii.Controls.HtmlControls;
using ArtOfTest.WebAii.ObjectModel;
using ClientAutomation.TelerikCoreUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ClientAutomation.PageObjects
{
    class PageAnalysis : PageMasterDefinition
    {
        public static string DynamicValue = null;
        #region RRLPageObjects

        public static HtmlButton btnScanCards { get { return (HtmlButton)TelerikObject.GetElement(CONTROLTYPE_HTMLBUTTON, "id", "collectCards", "Scan Cards Button"); } }

        public static HtmlButton btncardLibrary { get { return (HtmlButton)TelerikObject.GetElement(CONTROLTYPE_HTMLBUTTON, "id", "historyCards", "Card Library Button"); } }
        public static HtmlButton btnClearCards { get { return (HtmlButton)TelerikObject.GetElement(CONTROLTYPE_HTMLBUTTON, "id", "clearCards", "Clear Cards Button"); } }

        public static HtmlButton btnpatternMatching { get { return (HtmlButton)TelerikObject.GetElement(CONTROLTYPE_HTMLBUTTON, "id", "patternMatching", "Pattern Matching Button"); } }

        public static HtmlButton btnrunAnalysis { get { return (HtmlButton)TelerikObject.GetElement(CONTROLTYPE_HTMLBUTTON, "id", "runAnalysis", "Run Analysis Button"); } }

        //Yes, Run Analysis
        //rerunAnalysis
        public static HtmlButton btnConfirmRerunAnalysis { get { return (HtmlButton)TelerikObject.GetElement(CONTROLTYPE_HTMLBUTTON, "content", "Yes, Run Analysis", "Confirm Re_Run Analysis Button"); } }
        public static HtmlButton btnRerunAnalysis { get { return (HtmlButton)TelerikObject.GetElement(CONTROLTYPE_HTMLBUTTON, "id", "rerunAnalysis", "Re_Run Analysis Button"); } }
        //analysisOptions
        public static HtmlButton btnanalysisOptions { get { return (HtmlButton)TelerikObject.GetElement(CONTROLTYPE_HTMLBUTTON, "id", "analysisOptions", "Analysis Options Button"); } }
        //dynagraphShowHide

        public static HtmlButton btndynagraphShowHide { get { return (HtmlButton)TelerikObject.GetElement(CONTROLTYPE_HTMLBUTTON, "id", "dynagraphShowHide", "Dynagraph ShowHide Button"); } }

        public static HtmlButton btnanalysisReportBtn { get { return (HtmlButton)TelerikObject.GetElement(CONTROLTYPE_HTMLBUTTON, "id", "analysisReportBtn", "Analysis Report Button"); } }
        public static HtmlButton btnunitBalancing { get { return (HtmlButton)TelerikObject.GetElement(CONTROLTYPE_HTMLBUTTON, "id", "unitBalancing", "Unit Balancing Button"); } }
        public static HtmlButton btncalibrateBtn { get { return (HtmlButton)TelerikObject.GetElement(CONTROLTYPE_HTMLBUTTON, "id", "calibrateBtn", "Calibrate Button"); } }
        public static HtmlButton btnBackCard { get { return (HtmlButton)TelerikObject.GetElement(CONTROLTYPE_HTMLBUTTON, "id", "leftCardClickButton", "Back Button"); } }
        public static HtmlButton btnForwardCard { get { return (HtmlButton)TelerikObject.GetElement(CONTROLTYPE_HTMLBUTTON, "id", "rightCardClick", "Forward Button"); } }

        public static HtmlControl lstItemCurrentCard { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLANCHOR, "content", "Current Card", "Current Card"); } }
        public static HtmlControl lstItemFullCard { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLANCHOR, "content", "Full Card", "Full Card"); } }
        public static HtmlControl lstItemPumpoffCard { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLANCHOR, "content", "Pumpoff Card", "Pumpoff Card"); } }
        public static HtmlControl lstItemAlarmCard { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLANCHOR, "content", "Alarm Card", "Alarm Card"); } }
        public static HtmlControl lstItemFailureCard { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLANCHOR, "content", "Failure Card", "Failure Card"); } }

        public static HtmlControl lstItemCardCollection { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLANCHOR, "content", "Continuous Collection", "Continuous Collection"); } }


        public static HtmlControl txtCollectionInterval { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "id", "timePeriod", " Time Period"); } }

        public static HtmlControl txtNumberofcards { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "id", "noOfCards", " Number of Cards"); } }
        public static HtmlControl txtCommFail { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "id", "commFails", " Comm Fail "); } }

        public static HtmlControl btnStart { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", "Start", "Start Button"); } }

        public static HtmlControl lblStartDate { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "expression", "innertext=Start Date:;tagname=label", "Start Date Label"); } }
        //End Date:

        public static HtmlControl startDate { get { return TelerikObject.GetChildrenControl(new HtmlControl(lblStartDate.BaseElement.Parent), "1;0;0;0;0"); } }
        public static HtmlControl lblEndDate { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "expression", "innertext=End Date:;tagname=label", "End Date Label"); } }

        // - Downhole - Controller

        public static HtmlControl lblDonwholeControllerCardCollection { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "id", "carddesc1", "Card Description Div Control"); } }


        public static HtmlControl container_Chart { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "id", "dynagraphViewerKendoChart", "DynaCard Chart Container:"); } }
        //dynaGraph
        public static HtmlControl container_CardLibrary { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "id", "dynaGraph", "Card Library Container:"); } }
        public static HtmlControl toasterControl { get { return TelerikObject.GetElement(CONTROLTYPE_FORESITETOASTERCONTROL, "", "", "Toaster Control"); } }

        public static HtmlControl legend_Text { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", DynamicValue, "Legend Text for Selected Card"); } }


        public static HtmlControl divCardCollectionStatus { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", DynamicValue, "Card Collection Status Div"); } }


        public static HtmlButton btnStopCardcollection { get { return (HtmlButton)TelerikObject.GetElement(CONTROLTYPE_HTMLBUTTON, "content", "Stop", "Stop Card Collection button "); } }

        public static HtmlButton btnSaveCollectedcardToLibrary { get { return (HtmlButton)TelerikObject.GetElement(CONTROLTYPE_HTMLBUTTON, "content", "Save Collected Card to Library", "Save Collected Card to Library button "); } }



        //

        public static HtmlControl txtPatternCardDescription { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "id", "pmsaveCardDescription", " Card description Text Box"); } }

        public static HtmlButton btnSaveOnDialogForCardcollection { get { return (HtmlButton)TelerikObject.GetElement(CONTROLTYPE_HTMLBUTTON, "id", "saveCardLibraryBtn", "Insert Collected Card to Library in Dialog SAve button "); } }
        public static HtmlAnchor tabLibraryPatternCardMatch { get { return (HtmlAnchor)TelerikObject.GetElement(CONTROLTYPE_HTMLANCHOR, "content", "Library", "Library tab on Pattern card collection "); } }
        public static HtmlAnchor tabResultsPatternCardMatch { get { return (HtmlAnchor)TelerikObject.GetElement(CONTROLTYPE_HTMLANCHOR, "content", "Results", "Results tab on Patern card collection "); } }
        //
        public static HtmlButton btnDeletePatternCardSelected { get { return (HtmlButton)TelerikObject.GetElement(CONTROLTYPE_HTMLBUTTON, "content", "Delete", "Delete Pattern Card button "); } }
        public static HtmlButton btnUpdatePatternCardSelected { get { return (HtmlButton)TelerikObject.GetElement(CONTROLTYPE_HTMLBUTTON, "content", "Update", "Update Pattern Card button "); } }
        // comparesurfaceCheckBoxUpdate
        public static HtmlInputCheckBox chkbox_CompareSurfaceCard { get { return (HtmlInputCheckBox)TelerikObject.GetElement(CONTROLTYPE_HTMLINPUTCHECKBOX, "id", "comparesurfaceCheckBoxUpdate", "CheckBox Compare Surface Card button "); } }
        public static HtmlInputCheckBox chkbox_CompareDownholeCard { get { return (HtmlInputCheckBox)TelerikObject.GetElement(CONTROLTYPE_HTMLINPUTCHECKBOX, "id", "comparedownholeCheckBoxUpdate", "CheckBox Compare downhole Card button "); } }

        //updateCardLibraryBtn
        public static HtmlButton btnModifyPatternCardSelected { get { return (HtmlButton)TelerikObject.GetElement(CONTROLTYPE_HTMLBUTTON, "id", "updateCardLibraryBtn", "Save button on Update Pattern Card button "); } }

        public static HtmlControl htmlcell_AnalsyisTable { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", DynamicValue, "Html Table cell with Value:  " + DynamicValue); } }
        //confirmDeleteCardLibraryBtn
        public static HtmlButton btnConfirmDeleteCardFromLibrary { get { return (HtmlButton)TelerikObject.GetElement(CONTROLTYPE_HTMLBUTTON, "id", "confirmDeleteCardLibraryBtn", "Confirm Delete Card Library button "); } }

        public static HtmlControl paraConfirmTextforCardDelete { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", DynamicValue, "Confirm Paragraph for delelte card " + DynamicValue); } }

        //@class='miniReport col-md-6 col-sm-12 col-lg-6 col-xs-12 ng-star-inserted'
        public static HtmlControl divMiniReportpane { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "attribute", "class=miniReport col-md-6 col-sm-12 col-lg-6 col-xs-12 ng-star-inserted", "MiniReport Div"); } }


        public static HtmlSelect dd_AnalysisMehtod { get { return (HtmlSelect)TelerikObject.GetElement(CONTROLTYPE_HTMLSELECT, "id", "analysisMethod", "Analysis Method Dropdown"); } }
        public static HtmlSelect dd_FluidLelvel { get { return (HtmlSelect)TelerikObject.GetElement(CONTROLTYPE_HTMLSELECT, "id", "fluidLevelMethod", "Fluid Lelvel Method Dropdown"); } }
        public static HtmlSelect dd_DampingMehtod { get { return (HtmlSelect)TelerikObject.GetElement(CONTROLTYPE_HTMLSELECT, "id", "dampingMethod", "Damping Method Dropdown"); } }
        public static HtmlControl txtFluidLevel { get { return (HtmlControl)TelerikObject.GetElement("kendo-numerictextbox", "id", "fluidLevel", "Fluid Lelvel Text Box"); } }

        public static HtmlControl txtDampigFactor { get { return (HtmlControl)TelerikObject.GetElement("kendo-numerictextbox", "id", "dampingFactor", "Damping Factor text box"); } }

        public static HtmlControl chartLineText { get { return (HtmlControl)TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "Content", DynamicValue, "text : " + DynamicValue); } }

        public static HtmlControl wellCommentText { get { return (HtmlControl)TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "Content", DynamicValue, "text : " + DynamicValue); } }
        //saveAnalysisOption
        public static HtmlButton btnSaveAnlysisOption { get { return (HtmlButton)TelerikObject.GetElement(CONTROLTYPE_HTMLBUTTON, "name", "saveAnalysisOption", "Save Analysis option button "); } }

        public static Element chkCardRangeSlider { get { return TelerikObject.GetChildrenControl(lstCardRangeSlider, "0").BaseElement; } }

        public static HtmlInputCheckBox chkboxCardRangeSlider { get { return new HtmlInputCheckBox(chkCardRangeSlider); } }

        public static HtmlListItem lstCardRangeSlider { get { return (HtmlListItem)TelerikObject.GetElement(CONTROLTYPE_HTMLLISTITEM, "content", "Card Range Slider", "Card Range Slider List"); } }

        public static HtmlListItem lstPumpFillPercent { get { return (HtmlListItem)TelerikObject.GetElement(CONTROLTYPE_HTMLLISTITEM, "content", "Pump Fill %", "Pump Fill %"); } }

        public static HtmlListItem lstVSDTolerance { get { return (HtmlListItem)TelerikObject.GetElement(CONTROLTYPE_HTMLLISTITEM, "content", "VSD Tolerance", " VSD Tolerance"); } }

        public static HtmlListItem lstShutDownLimits { get { return (HtmlListItem)TelerikObject.GetElement(CONTROLTYPE_HTMLLISTITEM, "content", "Shut Down Limits", "Shut Down Limits"); } }

        public static HtmlListItem lstFluidLoadLines { get { return (HtmlListItem)TelerikObject.GetElement(CONTROLTYPE_HTMLLISTITEM, "content", "Fluid Load Lines", "Fluid Load Lines"); } }

        public static HtmlListItem lstGridLines { get { return (HtmlListItem)TelerikObject.GetElement(CONTROLTYPE_HTMLLISTITEM, "content", "Grid Lines", "Grid Lines"); } }
        public static HtmlListItem lstPoints { get { return (HtmlListItem)TelerikObject.GetElement(CONTROLTYPE_HTMLLISTITEM, "content", "Points", "Points"); } }
        public static HtmlListItem lstCursorPosition { get { return (HtmlListItem)TelerikObject.GetElement(CONTROLTYPE_HTMLLISTITEM, "content", "Cursor Position", "Cursor Position"); } }
        public static HtmlListItem lstLegend { get { return (HtmlListItem)TelerikObject.GetElement(CONTROLTYPE_HTMLLISTITEM, "content", "Legend", "Legend"); } }
        public static HtmlListItem lstComments { get { return (HtmlListItem)TelerikObject.GetElement(CONTROLTYPE_HTMLLISTITEM, "content", "Comments", "Comments"); } }
        public static HtmlListItem lstAdjustToolbar { get { return (HtmlListItem)TelerikObject.GetElement(CONTROLTYPE_HTMLLISTITEM, "content", "Adjust Toolbar", "Adjust Toolbar"); } }
        public static HtmlControl txtComments { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "id", "wcTextArea", "Well Analysis Comments"); } }
        public static HtmlInputRange slider_CardRange { get { return (HtmlInputRange)TelerikObject.GetElement(CONTROLTYPE_HTMLINPUTRANGE, "name", "points", "Card Range Slider "); } }
        public static Element XaxisLabel { get { return TelerikObject.GetElement("Xpath", "//*[contains(text(),'Position(')]", "X Axlis Label[Position]"); } }

        public static Element pumpFillPercent_0 { get { return TelerikObject.GetElement("Xpath", "//*[text()='0%']", "Pump Fill  % text Element"); } }

        public static Element vsdTolerrancePercent_8 { get { return TelerikObject.GetElement("Xpath", "//*[text()='0%']", "VSD Tolerance  % text Element"); } }
        public static HtmlButton btnWellCommentsSave { get { return (HtmlButton)TelerikObject.GetElement(CONTROLTYPE_HTMLBUTTON, "attribute", "class=submit-holder btn btn-default wc-submit-btn", "Well Analyisis Comments Save button "); } }

        public static HtmlButton btnWellCommentsDelete { get { return (HtmlButton)TelerikObject.GetElement(CONTROLTYPE_HTMLBUTTON, "id", "Delete", "Well Analyisis Comments Delete button "); } }

        public static HtmlButton btnWellCommentsDeleteConfirmYes { get { return (HtmlButton)TelerikObject.GetElement(CONTROLTYPE_HTMLBUTTON, "content", "Yes", "Well Analyisis Comments Delete Yes Confirm button "); } }


        public static HtmlControl divContainerComments { get { return (HtmlControl)TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "attribute", "class=no-comment-data ng-star-inserted", "div Container for Comments Not avaialable text "); } }
        //txtRotateCard
        public static HtmlInputText txtRotateCard { get { return (HtmlInputText)TelerikObject.GetElement(CONTROLTYPE_HTMLINPUTTEXT, "name", "txtRotateCard", "Rotate Card Text "); } }
        //glyphicon glyphicon-minus

        public static HtmlControl btnRoateCardPlus { get { return (HtmlControl)TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "attribute", "class=glyphicon glyphicon-plus", "Rotate Card Plus "); } }
        public static HtmlControl btnRoateCardMinus { get { return (HtmlControl)TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "attribute", "class=glyphicon glyphicon-minus", "Rotate Card Minus"); } }


        //saveGridOptions
        public static HtmlButton btnSavegridOptions { get { return (HtmlButton)TelerikObject.GetElement(CONTROLTYPE_HTMLBUTTON, "id", "saveLayout", "Save Grid Options button "); } }
        public static HtmlButton btnWellResetGridOptions { get { return (HtmlButton)TelerikObject.GetElement(CONTROLTYPE_HTMLBUTTON, "id", "resetLayout", "Reset Grid Options button "); } }
        // Yes, save Well Analysis layout as the default layout. 
        public static HtmlButton btnWSaveGridOptionsConfirm { get { return (HtmlButton)TelerikObject.GetElement(CONTROLTYPE_HTMLBUTTON, "content", "Yes, save Well Analysis layout as the default layout.", "Save Confirm Grid Options button "); } }
        //
        public static HtmlAnchor btnCloseSaveGridOptions { get { return (HtmlAnchor)TelerikObject.GetElement(CONTROLTYPE_HTMLANCHOR, "attribute", "aria-label=Close", "Close Save GridLayout"); } }

        public static HtmlButton btnResetGridOptionsConfirm { get { return (HtmlButton)TelerikObject.GetElement(CONTROLTYPE_HTMLBUTTON, "content", "Yes, reset Well Analysis layout to original default layout.", "Reset Grid Options Confirm  button "); } }

        //optimalCBTVal
        public static HtmlControl txtDesiredCBT { get { return (HtmlControl)TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "id", "optimalCBTVal", "Desired Text CBT "); } }
        //getUnitBal
        public static HtmlButton btnUnitBalanceCalculate { get { return (HtmlButton)TelerikObject.GetElement(CONTROLTYPE_HTMLBUTTON, "id", "getUnitBal", "Calculate Unit Balancing Button"); } }
        public static HtmlControl lblExisitngCBT { get { return (HtmlControl)TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", "Existing Counter Balance Torque  :", "ExisitngCBT"); } }
        public static HtmlControl lblExisitngMaxGBT { get { return (HtmlControl)TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", "Existing Max GearBox Torque :", "ExisitngMaxGBT"); } }
        public static HtmlControl lblDesiredCBT { get { return (HtmlControl)TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", "Resulting Counter Balance Torque :", "DesiredCBT"); } }
        public static HtmlControl lblDesiredMaxGBT { get { return (HtmlControl)TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", "Projected Max GearBox Torque :", "DesiredMaxGBT"); } }
        //Unit Balancing Details
        public static HtmlControl lblUnitBalancingDetails { get { return (HtmlControl)TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", "Unit Balancing Details", "Dialog title : Unit Balancing Details"); } }

        public static HtmlButton btnRCloseUnitBalanceDetails { get { return (HtmlButton)TelerikObject.GetElement(CONTROLTYPE_HTMLBUTTON, "attribute", "class=close ng-star-inserted", "Close Unit Balacning Details Button "); } }

        public static HtmlControl txtCalibrtionPercent { get { return (HtmlControl)TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "id", "pumpFillage", "Pump Fillage"); } }
        //  public static HtmlButton btnRGetCalibrationCard { get { return (HtmlButton)TelerikObject.GetElement(CONTROLTYPE_HTMLBUTTON, "content", "Get", "Get Calibration Card Button "); } }
        //rrlAnalysisReport

        public static HtmlButton btnDownoadAnalysisReportbutton { get { return (HtmlButton)TelerikObject.GetElement(CONTROLTYPE_HTMLBUTTON, "id", "rrlAnalysisReport", "Download RRLAnalysis Report "); } }
        //  (//button/span[contains(text(),'Get')])[1]

        public static Element baseElemForGetCalibrationCard { get { return TelerikObject.GetElement("Xpath", "(//button/span[contains(text(),'Get')])[1]", "Get Calibration button "); } }
        public static HtmlControl btnRGetCalibrationCard { get { return new HtmlControl(baseElemForGetCalibrationCard); } }
        #endregion

        #region ESPPageObjects
        public static HtmlControl ddtxtHeadTuningFactor { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", "Head Tuning Factor", "Head Tuning Factor Textbox"); } }
        public static HtmlControl ddtxtPumpIntakePressure { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", "Pump Intake Pressure", "Pump Intake Pressure Textbox"); } }
        public static HtmlControl ddtxtPumpDischargePressure { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", "Pump Discharge Pressure", "Pump Discharge Pressure Textbox"); } }
        public static HtmlControl ddtxtFrequency { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", "Frequency", "Frequency Textbox"); } }
        public static HtmlControl ddtxtFormationWGR { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", "Formation WGR", "Formation WGR Textbox"); } }
        public static HtmlControl ddtxtGasRateFromIPR { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", "Gas Rate (from IPR)", "Gas Rate from IPR"); } }
        #endregion

        #region GLPageObjects


        //   public static HtmlControl ddWellTestDate{ get { return TelerikObject.GetElement(CONTROLTYPE_KENDO_DROPDOWNLIST, "id", "glawbMethodDD", "WellTestDropdown"); } }
        public static HtmlControl ddtxtWellTestDate { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", "Test Date", "Test Date  Dropdown"); } }
        public static HtmlControl txtQualitycode { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", "Quality Code", "Quailty code Text Box"); } }
        public static HtmlControl ddtxtInjectionMethod { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", "Injection Method", "Injection Method  Dropdown"); } }
        public static HtmlControl ddtxtDepth { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", "Depth", "Depth"); } }
        public static HtmlControl ddtxtMultiphaseFlowCorrelation { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", "Multiphase Flow Correlation", "Multiphase Flow Correlation  Dropdown"); } }
        public static HtmlControl txtLFactor { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", "L Factor", "L Factor TextBox"); } }
        public static HtmlControl txtWellheadPressure { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", "Wellhead Pressure", "Wellhead Pressure TextBox "); } }
        public static HtmlControl txtWellheadTemperature { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", "Wellhead Temperature", "Wellhead Temperature  TextBox"); } }
        public static HtmlControl txtCasingHeadPressure { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", "Casing Head Pressure", "Casing Head Pressure TextBox"); } }
        public static HtmlControl txtGasInjectionRate { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", "Gas Injection Rate", "Gas Injection Rate  TextBox"); } }
        public static HtmlControl txtDownholeGauge { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", "Downhole Gauge", "Downhole Gauge TextBox"); } }

        public static HtmlControl txtOilRate { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", "Oil Rate", "Oil Rate TextBox"); } }
        public static HtmlControl txtWaterRate { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", "Water Rate", "Water Rate  TextBox"); } }
        public static HtmlControl txtGasRate { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", "Gas Rate", "Gas Rate  TextBox"); } }
        public static HtmlControl txtLiquidRate { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", "Liquid Rate", "Liquid Rate TextBox"); } }
        public static HtmlControl txtFormationGOR { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", "Formation GOR", "Formation GOR  TextBox"); } }
        public static HtmlControl txtWaterCut { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", "Water Cut", "Water Cut  TextBox"); } }
        public static HtmlControl txtStaticBottomHolePressure { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", "Static Bottom Hole Pressure", "Static Bottom Hole Pressure  TextBox"); } }
        public static HtmlControl txtProductivityIndex { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", "Productivity Index", "Productivity Index  TextBox"); } }
        public static HtmlControl ddtxtStartNode { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", "Start Node", "Start Node  Dropdown"); } }
        public static HtmlControl ddtxtSolutionNode { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", "Solution Node", "Solution Node  Dropdown"); } }

        public static HtmlControl ddtxtFBHP { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", "Flowing Bottom Hole Pressure", "Flowing Bottom Hole Pressure txtbox"); } }
        public static HtmlControl ddtxtSolutionNodePressure { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", "Solution Node Pressure", "SolutionNodePressure"); } }
        public static HtmlControl ddtxtGasInjectionDepth { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", "Gas Injection Depth", "Gas Injection Depth"); } }
        public static HtmlControl ddtxtLiquidRateFromIPR { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", "Liquid Rate (from IPR)", "Liquid Rate from IPR"); } }



        public static HtmlButton btnGLAWBsave { get { return (HtmlButton)TelerikObject.GetElement(CONTROLTYPE_HTMLBUTTON, "content", "Save", "Gl AWB Save Button"); } }
        public static HtmlButton btnGLAWBNew { get { return (HtmlButton)TelerikObject.GetElement(CONTROLTYPE_HTMLBUTTON, "content", "New", "Gl AWB New Button"); } }
        public static HtmlButton btnGLAWBRun { get { return (HtmlButton)TelerikObject.GetElement(CONTROLTYPE_HTMLBUTTON, "content", "Run", "Gl AWB Run Button"); } }
        public static HtmlControl GLlegend_Text { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", DynamicValue, "Legend Text for Gas Lift Curves", true); } }

        public static HtmlControl tabGradientCurves { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", "Gradient Curves", "Gradient Curves Tab"); } }
        public static HtmlControl tabPerformanceCurves { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", "Performance Curves", "Performance Curves Tab"); } }
        public static HtmlControl tabInflowOutflowCurves { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", "Inflow/Outflow Curves", "Inflow/Outflow Curves Tab"); } }
        public static HtmlControl tabPumpCurves { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", "Pump Curves", "Pump Curves Tab"); } }
        public static HtmlControl tabbGasinessCurves { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", "Gassiness Curves", "Gassiness Curves Tab"); } }
        public static HtmlControl togglePlotSelectionSwitchSpanON { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "attribute", "class=k-switch-label-on", "Plot Switch ON"); } }

        public static HtmlControl togglePlotSelectionSwitchSpanOFF { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "attribute", "class=k-switch-label-off", "Plot Switch OFF"); } }

        public static HtmlControl toggleDailyAvgWellTest { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "attribute", "class=k-switch-label-on", "Plot Switch ON"); } }
        public static HtmlControl ddInjectionmethod { get { return TelerikObject.GetNextSibling(TelerikObject.GetParent(ddtxtInjectionMethod, 1), 2); } }
        public static HtmlControl ddmultiphasecorrleation { get { return TelerikObject.GetNextSibling(TelerikObject.GetParent(ddtxtMultiphaseFlowCorrelation, 1), 2); } }
        public static HtmlControl ddStartNode { get { return TelerikObject.GetNextSibling(TelerikObject.GetParent(ddtxtStartNode, 1), 2); } }

        public static HtmlControl ddsolutionNode { get { return TelerikObject.GetNextSibling(TelerikObject.GetParent(ddtxtSolutionNode, 1), 2); } }
        public static HtmlControl txtWellheadPressureinput { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", "wellheadPresureInput", "Wellhead Pressure TextBox "); } }


        #endregion

        #region MethodsforPage


        public static void PreCardCollectionCheck()
        {
            HtmlControl nocardtextcontainer = TelerikObject.GetNextSibling(container_Chart, 3);
            nocardtextcontainer = TelerikObject.GetChildrenControl(nocardtextcontainer, "0");
            Thread.Sleep(1000);
            string nocardstext = nocardtextcontainer.BaseElement.InnerText;
            Assert.AreEqual("No Card Selected.", nocardstext, "When Cygnet Facility having no cards is selected ,Cards are still seen in ForeSite");
            Helper.CommonHelper.TraceLine("When No cards are present in Cygnet Facility the text on UI is: " + nocardtextcontainer.BaseElement.InnerText);
            TelerikObject.Click(btncardLibrary);
            //HtmlControl lblCardlibraryCount = TelerikObject.GetChildrenControl(container_CardLibrary, "0;0;2;0");
            //string txtcount = lblCardlibraryCount.BaseElement.InnerText;
            //ClientAutomation.Helper.CommonHelper.TraceLine("Card Library count " + txtcount);
            //Assert.AreEqual("Total: 0 items", txtcount, "Card Library is having cards  when Cygnet Fac does not have cards");
            VerifyCardLibraryCount("Card Library Count When No Cards are Present", 0);
            TelerikObject.Click(btncardLibrary);
            checkforElementState(btnScanCards, "Enabled", "Scan Cards");
            checkforElementState(btnpatternMatching, "Disabled", "Run Pattern Matching");
            checkforElementState(btnClearCards, "Enabled", "Clear Cards");
            checkforElementState(btnanalysisOptions, "Enabled", "Analysis Options");
            checkforElementState(btncardLibrary, "Enabled", "Card Library");
            checkforElementState(btndynagraphShowHide, "Enabled", "Show /Hide ");
            checkforElementState(btnanalysisReportBtn, "Disabled", "Analysis Report");
            checkforElementState(btnunitBalancing, "Disabled", "Unit Balancing");
            checkforElementState(btncalibrateBtn, "Disabled", "Calibration Card");
            checkforElementState(btnBackCard, "Disabled", "Back Card");
            checkforElementState(btnForwardCard, "Disabled", "Forward Card");
        }

        public static void PostCardCollectionCheck()
        {
            TelerikObject.Click(btncardLibrary);
            Thread.Sleep(5000);
            VerifyCardLibraryCount("Card Library Count When 5 Cards [Current] [Full] [PumpOff Card] [Alarm Carc] [Failure Card] , are pulled ", 5);
            TelerikObject.Click(btncardLibrary);
            Thread.Sleep(2000);
        }

        public static void VerifyAllShowHideOptions()
        {
            Assert.IsTrue(getCheckBoXforListItem(lstLegend).Checked, "Legend was not checked by default");
            Assert.IsNotNull(lstGridLines, "Grid Lines was not present in ShowHide Option");
            Assert.IsNotNull(lstPoints, "Points was not present in ShowHide Option");
            Assert.IsNotNull(lstCursorPosition, "Cursor Position was not present in ShowHide Option");
            Assert.IsNotNull(lstLegend, "Legend was not present in ShowHide Option");
            Assert.IsNotNull(lstPumpFillPercent, "Pump Fill Percent was not present in ShowHide Option");
            Assert.IsNotNull(lstVSDTolerance, "VSD Tolerance was not present in ShowHide Option");
            Assert.IsNotNull(lstShutDownLimits, "ShutDownLimits was not present in ShowHide Option");
            Assert.IsNotNull(lstFluidLoadLines, "FluidLoadLines was not present in ShowHide Option");
            Assert.IsNotNull(lstCardRangeSlider, "CardRangeSlider was not present in ShowHide Option");
            Assert.IsNotNull(lstComments, "Comments  was not present in ShowHide Option");
            Assert.IsNotNull(lstAdjustToolbar, "AdjustToolbar was not present in ShowHide Option");
            Helper.CommonHelper.TraceLine("[*** Check Point *****] All the Options for Show Hide are shown/Present  on UI");

        }
        public static void PersistOptionsCheckForDefault()
        {
            // The Show Hide Options 
            Assert.IsTrue(getCheckBoXforListItem(lstLegend).Checked, "Legend is checked");
            Assert.IsFalse(getCheckBoXforListItem(lstGridLines).Checked, " GridLines is Unchecked");
            Assert.IsFalse(getCheckBoXforListItem(lstPoints).Checked, " Points is Unchecked");
            Assert.IsFalse(getCheckBoXforListItem(lstCursorPosition).Checked, " CursorPosition is Unchecked");
            Assert.IsFalse(getCheckBoXforListItem(lstPumpFillPercent).Checked, " PumpFillPercent is Unchecked");
            Assert.IsFalse(getCheckBoXforListItem(lstVSDTolerance).Checked, " VSDTolerance is Unchecked");
            Assert.IsFalse(getCheckBoXforListItem(lstShutDownLimits).Checked, " ShutDownLimits is Unchecked");
            Assert.IsFalse(getCheckBoXforListItem(lstFluidLoadLines).Checked, " FluidLoadLines is Unchecked");
            Assert.IsFalse(getCheckBoXforListItem(lstCardRangeSlider).Checked, " CardRangeSlider is Unchecked");
            Assert.IsFalse(getCheckBoXforListItem(lstComments).Checked, " Comments is Unchecked");
            Assert.IsFalse(getCheckBoXforListItem(lstAdjustToolbar).Checked, " AdjustToolbar is Unchecked");
            Helper.CommonHelper.TraceLine("**** [Check Point:]****** The Default Values for show hide options are as per default  ");
        }

        public static void VerifyAllOptionsSaved()
        {
            // The Show Hide Options 
            Assert.IsTrue(getCheckBoXforListItem(lstLegend).Checked, "Legend is checked");
            Assert.IsTrue(getCheckBoXforListItem(lstGridLines).Checked, " GridLines is checked");
            Assert.IsTrue(getCheckBoXforListItem(lstPoints).Checked, " Points is checked");
            Assert.IsTrue(getCheckBoXforListItem(lstCursorPosition).Checked, " CursorPosition is checked");
            Assert.IsTrue(getCheckBoXforListItem(lstPumpFillPercent).Checked, " PumpFillPercent is checked");
            Assert.IsTrue(getCheckBoXforListItem(lstVSDTolerance).Checked, " VSDTolerance is checked");
            Assert.IsTrue(getCheckBoXforListItem(lstShutDownLimits).Checked, " ShutDownLimits is checked");
            Assert.IsTrue(getCheckBoXforListItem(lstFluidLoadLines).Checked, " FluidLoadLines is checked");
            Assert.IsTrue(getCheckBoXforListItem(lstCardRangeSlider).Checked, " CardRangeSlider is checked");
            Assert.IsTrue(getCheckBoXforListItem(lstComments).Checked, " Comments is checked");
            Assert.IsTrue(getCheckBoXforListItem(lstAdjustToolbar).Checked, " AdjustToolbar is checked");
            Helper.CommonHelper.TraceLine("**** [Check Point:]****** The All Values for show hide options are as per Checked as pe SaveGridlayout  ");
        }

        public static void CheckallOptions()
        {
            getCheckBoXforListItem(lstGridLines).Check(true);
            getCheckBoXforListItem(lstPoints).Check(true);
            getCheckBoXforListItem(lstCursorPosition).Check(true);
            getCheckBoXforListItem(lstLegend).Check(true);
            getCheckBoXforListItem(lstPumpFillPercent).Check(true);
            getCheckBoXforListItem(lstVSDTolerance).Check(true);
            getCheckBoXforListItem(lstShutDownLimits).Check(true);
            getCheckBoXforListItem(lstFluidLoadLines).Check(true);
            getCheckBoXforListItem(lstCardRangeSlider).Check(true);
            getCheckBoXforListItem(lstComments).Check(true);
            getCheckBoXforListItem(lstAdjustToolbar).Check(true);
        }
        public static HtmlInputCheckBox getCheckBoXforListItem(HtmlControl listitemctl)
        {
            Element el = TelerikObject.GetChildrenControl(listitemctl, "0").BaseElement;
            HtmlInputCheckBox checkbox = new HtmlInputCheckBox(el);
            return checkbox;
        }
        public static void checkforElementState(HtmlControl ctl, string chk, string fieldname)
        {
            Helper.CommonHelper.TraceLine(string.Format("Checking for State {0} for {1}", chk, fieldname));
            if (ctl != null)
            {
                if (chk.ToUpper() == "ENABLED")
                {
                    Assert.IsTrue(ctl.IsEnabled, string.Format("Field Name {0} is Disabled ", fieldname));
                }
                else if (chk == "DISABLED")
                {
                    Assert.IsFalse(ctl.IsEnabled, string.Format("Field Name {0} is Enabled ", fieldname));
                }
                Helper.CommonHelper.TraceLine(string.Format(" Element {0} is {1}", fieldname, chk));
            }

        }

        public static void VerifyCardLibraryCount(string chkStatus, int expcount)
        {
            HtmlControl lblCardlibraryCount = TelerikObject.GetChildrenControl(container_CardLibrary, "0;0;2;0");
            string txtcount = lblCardlibraryCount.BaseElement.InnerText;
            Helper.CommonHelper.TraceLine("Card Library Count Text from UI: " + txtcount);
            Regex re = new Regex("\\d+");
            string count = re.Match(txtcount).Value;
            int cardCount = -1;
            int.TryParse(count, out cardCount);
            Helper.CommonHelper.TraceLine("Integer Card Count: = " + cardCount);
            if (cardCount != -1)
            {
                Assert.AreEqual(expcount, cardCount, chkStatus);
                Helper.CommonHelper.TraceLine(string.Format("For Condition {0} Actual Card Count is: {1} ", chkStatus, cardCount));
            }
        }

        public static HtmlTable GetTableByExpression(string counterbalacetype, string cranknum, string tableid)
        {
            string xpath = "//div[contains(text(),'" + counterbalacetype + "')]/ancestor::*[1]/following-sibling::div[1]/descendant::label[contains(text(),'" + cranknum + "')]" +
                            "//following-sibling::div[1]/descendant::table[" + tableid + "]";
            HtmlTable dyna = null;

            Element el = TelerikObject.GetElement("Xpath", xpath, string.Format("Table CB {0} Crank Num {1} Table Id{2}", counterbalacetype, cranknum, tableid));
            dyna = new HtmlTable(el);
            return dyna;
        }


        public static void getControlTextGLAnalysis(HtmlControl lblctl, string expvalue, string fieldname, int n = 0, bool casesense = true)
        {
            string getControlText = string.Empty;
            bool checkunit = false;
            string uomtext = string.Empty;
            //if (lblctl.BaseElement.Parent.GetNextSibling().GetNextSibling().GetNextSibling().InnerText.Length > 0)
            if (lblctl.BaseElement.Parent.Parent.Children.Count > 4)
            {
                checkunit = true;
                uomtext = lblctl.BaseElement.Parent.GetNextSibling().GetNextSibling().GetNextSibling().InnerText;
            }


            // Parents , Second Next Siblings ,nth child's innnetext
            if (n == 0)
            {
                getControlText = lblctl.BaseElement.Parent.GetNextSibling().GetNextSibling().InnerText;
                if (getControlText == string.Empty)
                //The Html Element is not having innertext means it is either Textbox or something else
                {
                    try
                    {
                        HtmlControl ctlbs1 = new HtmlControl(lblctl.BaseElement.Parent.GetNextSibling().GetNextSibling());
                        HtmlControl txtctlbase = TelerikObject.GetChildrenControl(ctlbs1, "0;0");
                        switch (txtctlbase.TagName.ToUpper())
                        {
                            case "INPUT":
                                {
                                    HtmlInputText txtctl = new HtmlInputText(txtctlbase.BaseElement);
                                    getControlText = txtctl.Text;
                                    break;
                                }
                            case "KENDO-NUMERICTEXTBOX":
                                {
                                    HtmlControl txtctln = TelerikObject.GetChildrenControl(txtctlbase, "0;0");
                                    HtmlInputText txtctl = new HtmlInputText(txtctln.BaseElement);
                                    getControlText = txtctl.Text;
                                    break;
                                }
                            default:
                                {
                                    Helper.CommonHelper.TraceLine("ControlType was not Known to Telerik");
                                    break;
                                }
                        }
                    }
                    catch (NullReferenceException nre)
                    {
                        Helper.CommonHelper.TraceLine("Control is Null" + nre.ToString());
                    }
                }
            }
            else
            {
                getControlText = lblctl.BaseElement.Parent.GetNextSibling().GetNextSibling().Children[n].InnerText;
            }

            if (checkunit)
            {
                string[] arrval = expvalue.Split(new char[] { ';' });
                TelerikObject.verifyValues(arrval[0], getControlText, fieldname, casesense);
                TelerikObject.verifyValues(arrval[1], uomtext, fieldname, casesense);
            }
            else
            {
                TelerikObject.verifyValues(expvalue, getControlText, fieldname, casesense);
                Helper.CommonHelper.TraceLine("[****Checkpoint ****]" + fieldname + " is matched *** exepcted value:  " + expvalue + "Actual Value " + getControlText);
            }



        }
        //
        public static void getControlTextESPAnalysis(HtmlControl lblctl, string expvalue, string fieldname, int n = 0, bool casesense = true)
        {
            string getControlText = string.Empty;
            bool checkunit = false;
            string uomtext = string.Empty;
            //if (lblctl.BaseElement.Parent.GetNextSibling().GetNextSibling().GetNextSibling().InnerText.Length > 0)
            if (lblctl.BaseElement.Parent.Parent.Children.Count > 4)
            {
                checkunit = true;
                uomtext = lblctl.BaseElement.Parent.GetNextSibling().GetNextSibling().GetNextSibling().InnerText;
            }


            // Parents , Second Next Siblings ,nth child's innnetext
            if (n == 0)
            {
                getControlText = lblctl.BaseElement.Parent.GetNextSibling().GetNextSibling().InnerText;
                if (getControlText == string.Empty)
                //The Html Element is not having innertext means it is either Textbox or something else
                {
                    try
                    {
                        HtmlControl ctlbs1 = new HtmlControl(lblctl.BaseElement.Parent.GetNextSibling().GetNextSibling());
                        HtmlControl txtctlbase = TelerikObject.GetChildrenControl(ctlbs1, "0;0");
                        switch (txtctlbase.TagName.ToUpper())
                        {
                            case "INPUT":
                                {
                                    HtmlInputText txtctl = new HtmlInputText(txtctlbase.BaseElement);
                                    getControlText = txtctl.Text;
                                    break;
                                }
                            case "KENDO-NUMERICTEXTBOX":
                                {
                                    HtmlControl txtctln = TelerikObject.GetChildrenControl(txtctlbase, "0;0");
                                    HtmlInputText txtctl = new HtmlInputText(txtctln.BaseElement);
                                    getControlText = txtctl.Text;
                                    break;
                                }
                            default:
                                {
                                    Helper.CommonHelper.TraceLine("ControlType was not Known to Telerik");
                                    break;
                                }
                        }
                    }
                    catch (NullReferenceException nre)
                    {
                        Helper.CommonHelper.TraceLine("Control is Null" + nre.ToString());
                    }
                }
            }
            else
            {
                getControlText = lblctl.BaseElement.Parent.GetNextSibling().GetNextSibling().Children[n].InnerText;
            }

            if (checkunit)
            {
                string[] arrval = expvalue.Split(new char[] { ';' });
                TelerikObject.verifyValues(arrval[0], getControlText, fieldname, casesense);
                TelerikObject.verifyValues(arrval[1], uomtext, fieldname, casesense);
            }
            else
            {
                TelerikObject.verifyValues(expvalue, getControlText, fieldname, casesense);
                Helper.CommonHelper.TraceLine("[****Checkpoint ****]" + fieldname + " is matched *** exepcted value:  " + expvalue + "Actual Value " + getControlText);
            }



        }
        //
        public static void getControlTextPreviousWellTestGLAnalysis(HtmlControl lblctl, string expvalue, string fieldname, int n = 0)
        {
            string getControlText = string.Empty;
            bool checkunit = false;
            string uomtext = string.Empty;
            //if (lblctl.BaseElement.Parent.GetNextSibling().GetNextSibling().GetNextSibling().InnerText.Length > 0)
            if (lblctl.BaseElement.Parent.Parent.Children.Count > 4)
            {
                checkunit = true;
                uomtext = lblctl.BaseElement.Parent.GetNextSibling().GetNextSibling().GetNextSibling().InnerText;
            }


            // Parents , Second Next Siblings ,nth child's innnetext
            if (n == 0)
            {
                getControlText = lblctl.BaseElement.Parent.GetNextSibling().InnerText;
                if (getControlText == string.Empty)
                //The Html Element is not having innertext means it is either Textbox or something else
                {
                    try
                    {
                        HtmlControl ctlbs1 = new HtmlControl(lblctl.BaseElement.Parent.GetNextSibling().GetNextSibling());
                        HtmlControl txtctlbase = TelerikObject.GetChildrenControl(ctlbs1, "0;0");
                        switch (txtctlbase.TagName.ToUpper())
                        {
                            case "INPUT":
                                {
                                    HtmlInputText txtctl = new HtmlInputText(txtctlbase.BaseElement);
                                    getControlText = txtctl.Text;
                                    break;
                                }
                            case "KENDO-NUMERICTEXTBOX":
                                {
                                    HtmlControl txtctln = TelerikObject.GetChildrenControl(txtctlbase, "0;0");
                                    HtmlInputText txtctl = new HtmlInputText(txtctln.BaseElement);
                                    getControlText = txtctl.Text;
                                    break;
                                }
                            default:
                                {
                                    Helper.CommonHelper.TraceLine("ControlType was not Konw to Telerik");
                                    break;
                                }
                        }
                    }
                    catch (NullReferenceException nre)
                    {
                        Helper.CommonHelper.TraceLine("Control is Null" + nre.ToString());
                    }
                }
            }
            else
            {
                getControlText = lblctl.BaseElement.Parent.GetNextSibling().GetNextSibling().Children[n].InnerText;
            }

            if (checkunit)
            {
                string[] arrval = expvalue.Split(new char[] { ';' });
                TelerikObject.verifyValues(arrval[0], getControlText, fieldname);
                TelerikObject.verifyValues(arrval[1], uomtext, fieldname);
            }
            else
            {
                TelerikObject.verifyValues(expvalue, getControlText, fieldname);
                Helper.CommonHelper.TraceLine("[****Checkpoint ****]" + fieldname + " is matched *** exepcted value:  " + expvalue + "Actual Value " + getControlText);
            }



        }

        public static void getControlTextPreviousWellTestESPAnalysis(HtmlControl lblctl, string expvalue, string fieldname, int n = 0)
        {
            string getControlText = string.Empty;
            bool checkunit = false;
            string uomtext = string.Empty;
            //if (lblctl.BaseElement.Parent.GetNextSibling().GetNextSibling().GetNextSibling().InnerText.Length > 0)
            if (lblctl.BaseElement.Parent.Parent.Children.Count > 4)
            {
                checkunit = true;
                uomtext = lblctl.BaseElement.Parent.GetNextSibling().GetNextSibling().GetNextSibling().InnerText;
            }


            // Parents , Second Next Siblings ,nth child's innnetext
            if (n == 0)
            {
                getControlText = lblctl.BaseElement.Parent.GetNextSibling().InnerText;
                if (getControlText == string.Empty)
                //The Html Element is not having innertext means it is either Textbox or something else
                {
                    try
                    {
                        HtmlControl ctlbs1 = new HtmlControl(lblctl.BaseElement.Parent.GetNextSibling().GetNextSibling());
                        HtmlControl txtctlbase = TelerikObject.GetChildrenControl(ctlbs1, "0;0");
                        switch (txtctlbase.TagName.ToUpper())
                        {
                            case "INPUT":
                                {
                                    HtmlInputText txtctl = new HtmlInputText(txtctlbase.BaseElement);
                                    getControlText = txtctl.Text;
                                    break;
                                }
                            case "KENDO-NUMERICTEXTBOX":
                                {
                                    HtmlControl txtctln = TelerikObject.GetChildrenControl(txtctlbase, "0;0");
                                    HtmlInputText txtctl = new HtmlInputText(txtctln.BaseElement);
                                    getControlText = txtctl.Text;
                                    break;
                                }
                            default:
                                {
                                    Helper.CommonHelper.TraceLine("ControlType was not Konw to Telerik");
                                    break;
                                }
                        }
                    }
                    catch (NullReferenceException nre)
                    {
                        Helper.CommonHelper.TraceLine("Control is Null" + nre.ToString());
                    }
                }
            }
            else
            {
                getControlText = lblctl.BaseElement.Parent.GetNextSibling().GetNextSibling().Children[n].InnerText;
            }

            if (checkunit)
            {
                string[] arrval = expvalue.Split(new char[] { ';' });
                TelerikObject.verifyValues(arrval[0], getControlText, fieldname);
                TelerikObject.verifyValues(arrval[1], uomtext, fieldname);
            }
            else
            {
                TelerikObject.verifyValues(expvalue, getControlText, fieldname);
                Helper.CommonHelper.TraceLine("[****Checkpoint ****]" + fieldname + " is matched *** exepcted value:  " + expvalue + "Actual Value " + getControlText);
            }



        }



        public static void setControlTextGLAnalysis(HtmlControl lblctl, string inputvalue, string fieldname, int n = 0)
        {
            string getControlText = string.Empty;
            // Parents , Second Next Siblings ,nth child's innnetext
            if (n == 0)
            {
                HtmlControl txtctl = new HtmlControl(lblctl.BaseElement.Parent.GetNextSibling().GetNextSibling());
                txtctl.ScrollToVisible();
                TelerikObject.Sendkeys(txtctl, inputvalue);
                Helper.CommonHelper.TraceLine(string.Format("Entered Value of {0} in {1}", inputvalue, fieldname));
            }
        }

        public static void setControlTextESPAnalysis(HtmlControl lblctl, string inputvalue, string fieldname, int n = 0)
        {
            string getControlText = string.Empty;
            // Parents , Second Next Siblings ,nth child's innnetext
            if (n == 0)
            {
                HtmlControl txtctl = new HtmlControl(lblctl.BaseElement.Parent.GetNextSibling().GetNextSibling());
                txtctl.ScrollToVisible();
                TelerikObject.Sendkeys(txtctl, inputvalue);
                Helper.CommonHelper.TraceLine(string.Format("Entered Value of {0} in {1}", inputvalue, fieldname));
            }
        }





        public static void GLAnalysisDefaultElementCheck()
        {
            checkforElementState(btnGLAWBsave, "Disabled", "Save Button");
            checkforElementState(btnGLAWBNew, "Disabled", "New Button");
            checkforElementState(btnGLAWBRun, "Disabled", "Run Button");
        }

        public static void ESPAnalysisDefaultElementCheck()
        {
            checkforElementState(btnGLAWBsave, "Disabled", "Save Button");
            checkforElementState(btnGLAWBNew, "Disabled", "New Button");
            checkforElementState(btnGLAWBRun, "Disabled", "Run Button");
        }


        public static void GLcheckGradientcurvesLegend()
        {
            //Default Check for Tubing tempertature
            // Tubing Pressure
            //Casing Pressure
            //..Downhole Pressue  Gasuge Depth
            GLcheckstaticlegendtext("Tubing Temperature");
            GLcheckstaticlegendtext("Tubing Pressure");
            GLcheckstaticlegendtext("Casing Pressure");
            GLcheckstaticlegendtext("Downhole Pressure Gauge Depth");




        }
        public static void ESPcheckGradientcurvesLegend()
        {
            //Default Check for Tubing tempertature
            // Tubing Pressure
            //Casing Pressure
            //..Downhole Pressue  Gasuge Depth
            GLcheckstaticlegendtext("Tubing Temperature");
            GLcheckstaticlegendtext("Tubing Pressure");



        }


        public static void SelectKendoDropdown(HtmlControl ctl, string value)
        {
            ctl.ScrollToVisible();
            TelerikObject.Click(ctl);
            TelerikObject.Select_KendoUI_Listitem(value);
        }
        public static void GLcheckPerformanceCurvesLegend(string unittext)
        {
            //these values are for Default Automatic InputType and Constiant Type
            if (unittext.ToUpper().Equals("US"))
            {
                GLcheckstaticlegendtext("570 psia");
                GLcheckstaticlegendtext("600 psia");
                GLcheckstaticlegendtext("630 psia");
                GLcheckstaticlegendtext("Well Test Point");
            }
            else
            {
                GLcheckstaticlegendtext("3930 kPa");
                GLcheckstaticlegendtext("4136.9 kPa");
                GLcheckstaticlegendtext("4343.7 kPa");
                GLcheckstaticlegendtext("Well Test Point");
            }

        }

        public static void ESPcheckPerformanceCurvesLegend(string unittext)
        {
            //these values are for Default Automatic InputType and Constiant Type
            if (unittext.ToUpper().Equals("US"))
            {
                ESPcheckstaticlegendtext("Operating Frequency: 60");
                ESPcheckstaticlegendtext("Well Test Point");
            }
            else
            {
                ESPcheckstaticlegendtext("Operating Frequency: 60");
                ESPcheckstaticlegendtext("Well Test Point");
            }

        }

        public static void GLcheckGradientCurvesAxislables(string unittext)
        {
            if (unittext.ToUpper().Equals("US"))
            {
                GLcheckstaticaxislabels("Pressure (psia)");
                GLcheckstaticaxislabels("Depth (ft)");
                GLcheckstaticaxislabels("Temperature (°F)");
            }
            else
            {
                GLcheckstaticaxislabels("Pressure (kPa)");
                GLcheckstaticaxislabels("Depth (m)");
                GLcheckstaticaxislabels("Temperature (°C)");
            }
        }

        public static void ESPcheckGradientCurvesAxislables(string unittext)
        {
            if (unittext.ToUpper().Equals("US"))
            {
                ESPcheckstaticaxislabels("Pressure (psia)");
                ESPcheckstaticaxislabels("Depth (ft)");
                ESPcheckstaticaxislabels("Temperature (°F)");
            }
            else
            {
                ESPcheckstaticaxislabels("Pressure (kPa)");
                ESPcheckstaticaxislabels("Depth (m)");
                ESPcheckstaticaxislabels("Temperature (°C)");
            }
        }


        public static void GLcheckPerformanceCurvesAxislables(string unittext)
        {
            if (unittext.ToUpper().Equals("US"))
            {
                GLcheckstaticaxislabels("Qgi (Mscf/d)");
                GLcheckstaticaxislabels("Ql (STB/d)");
                Helper.CommonHelper.TraceLine("Setting  Plot to Oil Production");
                //  TelerikObject.Click(togglePlotSelectionSwitchSpanOFF);
                GetAndClickToggleSwitchByLabel("Liquid Production", "OFF");
                TelerikObject.mgr.ActiveBrowser.RefreshDomTree();
                Thread.Sleep(1000);
                GLcheckstaticaxislabels("Qo (STB/d)");


            }
            else
            {
                GLcheckstaticaxislabels("Qgi (sm³/d)");
                GLcheckstaticaxislabels("Ql (sm³/d)");
                Helper.CommonHelper.TraceLine("Setting  Plot to Oil Production");
                //   TelerikObject.Click(togglePlotSelectionSwitchSpanOFF);
                GetAndClickToggleSwitchByLabel("Liquid Production", "ON");
                TelerikObject.mgr.ActiveBrowser.RefreshDomTree();
                Thread.Sleep(1000);
                GLcheckstaticaxislabels("Qo (sm³/d)");
            }
        }

        public static void ESPcheckPerformanceCurvesAxislables(string unittext)
        {
            if (unittext.ToUpper().Equals("US"))
            {
                ESPcheckstaticaxislabels("Pressure (psia)");
                ESPcheckstaticaxislabels("Ql (STB/d)");
                Helper.CommonHelper.TraceLine("Setting  Plot to Oil Production");
                Thread.Sleep(1000);
                GetAndClickToggleSwitchByLabel("Liquid Production", "OFF");
                TelerikObject.mgr.ActiveBrowser.RefreshDomTree();
                Thread.Sleep(3000);
                ESPcheckstaticaxislabels("Qo (STB/d)");


            }
            else
            {
                ESPcheckstaticaxislabels("Pressure (kPa)");
                ESPcheckstaticaxislabels("Ql (sm³/d)");
                Helper.CommonHelper.TraceLine("Setting  Plot to Oil Production");
                //   TelerikObject.Click(togglePlotSelectionSwitchSpanOFF);
                GetAndClickToggleSwitchByLabel("Liquid Production", "ON");
                TelerikObject.mgr.ActiveBrowser.RefreshDomTree();
                Thread.Sleep(1000);
                ESPcheckstaticaxislabels("Qo (sm³/d)");
            }
        }


        public static void GLcheckstaticlegendtext(string legendtext)
        {
            DynamicValue = legendtext;
            Assert.IsNotNull(GLlegend_Text, "Legend Text for " + legendtext + "Is not appearing on UI");
            Helper.CommonHelper.TraceLine(string.Format("Legend text for {0} appeared on UI ", legendtext));
        }

        public static void ESPcheckstaticlegendtext(string legendtext)
        {
            DynamicValue = legendtext;
            Assert.IsNotNull(GLlegend_Text, "Legend Text for " + legendtext + "Is not appearing on UI");
            Helper.CommonHelper.TraceLine(string.Format("Legend text for {0} appeared on UI ", legendtext));
        }



        public static void GLcheckstaticaxislabels(string axislabel)
        {
            DynamicValue = axislabel;
            Assert.IsNotNull(GLlegend_Text, "Axis Label for " + axislabel + "Is not appearing on UI");
            Helper.CommonHelper.TraceLine(string.Format("Axis Label  for {0} appeared on UI ", axislabel));
        }

        public static void ESPcheckstaticaxislabels(string axislabel)
        {
            DynamicValue = axislabel;
            Assert.IsNotNull(GLlegend_Text, "Axis Label for " + axislabel + "Is not appearing on UI");
            Helper.CommonHelper.TraceLine(string.Format("Axis Label  for {0} appeared on UI ", axislabel));
        }

        public static void GLcheckInflowOutflowCurvesAxislables(string unittext)
        {
            if (unittext.ToUpper().Equals("US"))
            {
                GLcheckstaticaxislabels("Ql (STB/d)");
                GLcheckstaticaxislabels("BHP (psia)");
                GetAndClickToggleSwitchByLabel("Liquid Production", "OFF");
                TelerikObject.mgr.ActiveBrowser.RefreshDomTree();
                Thread.Sleep(1000);
                GLcheckstaticaxislabels("Qo (STB/d)");
            }
            else
            {
                GLcheckstaticaxislabels("Ql (sm³/d)");
                GLcheckstaticaxislabels("BHP (kPa)");
                GetAndClickToggleSwitchByLabel("Liquid Production", "ON");
                TelerikObject.mgr.ActiveBrowser.RefreshDomTree();
                Thread.Sleep(1000);
                GLcheckstaticaxislabels("Qo (sm³/d)");
            }
        }



        public static void ESPcheckPumpCurvesAxislables(string unittext)
        {
            if (unittext.ToUpper().Equals("US"))
            {
                ESPcheckstaticaxislabels("Head (ft)");
                ESPcheckstaticaxislabels("In-Situ Flow Rate (bbl/d)");

                Thread.Sleep(1000);
                ESPcheckstaticaxislabels("Pump Horespower (hp)");
                ESPcheckstaticaxislabels("Pump Efficiency");
            }
            else
            {
                Thread.Sleep(1000);
                ESPcheckstaticaxislabels("Head (m)");
                ESPcheckstaticaxislabels("In-Situ Flow Rate (m³/d)");

                Thread.Sleep(1000);
                ESPcheckstaticaxislabels("Pump Horespower (KW)");
                ESPcheckstaticaxislabels("Pump Efficiency");

            }
        }

        public static void ESPcheckGassinessCurvesAxislables(string unittext)
        {
            if (unittext.ToUpper().Equals("US"))
            {
                ESPcheckstaticaxislabels("Pump Intake Pressure (psia)");
                ESPcheckstaticaxislabels("In-Situ Vapor Liquid Ratio (fraction)");

            }
            else
            {
                ESPcheckstaticaxislabels("Pump Intake Pressure (kPa)");
                ESPcheckstaticaxislabels("In-Situ Vapor Liquid Ratio (fraction)");

            }
        }



        public static void ESPcheckInflowOutflowCurvesAxislables(string unittext)
        {
            Thread.Sleep(3000);
            if (unittext.ToUpper().Equals("US"))
            {
                Thread.Sleep(1000);
                ESPcheckstaticaxislabels("Formation Gas (STB/d)");
                ESPcheckstaticaxislabels("PIP (psia)");

            }
            else
            {
                Thread.Sleep(1000);
                ESPcheckstaticaxislabels("Formation Gas (sm³/d)");
                ESPcheckstaticaxislabels("PIP (kPa)");



            }
        }


        public static void GLcheckInflowOutflowCurvesLegend()
        {
            GLcheckstaticlegendtext("Inflow");
            GLcheckstaticlegendtext("Outflow");
            GLcheckstaticlegendtext("Q Tech Min-Max");
            GLcheckstaticlegendtext("Operating Point");
            GLcheckstaticlegendtext("Bubble Point");
        }

        public static void ESPcheckInflowOutflowCurvesLegend()
        {
            ESPcheckstaticlegendtext("Inflow");
            ESPcheckstaticlegendtext("Outflow");
            ESPcheckstaticlegendtext("Q Tech Min-Max");
            ESPcheckstaticlegendtext("Operating Point");
            ESPcheckstaticlegendtext("Bubble Point");
        }

        public static void ESPcheckPumpCurvesLegend()
        {
            Thread.Sleep(2000);
            ESPcheckstaticlegendtext("Well Curve");
            ESPcheckstaticlegendtext("Head");
            ESPcheckstaticlegendtext("Power");
            ESPcheckstaticlegendtext("Efficiency");
            ESPcheckstaticlegendtext("Minimum Rate");
            ESPcheckstaticlegendtext("Maximum Rate");
            ESPcheckstaticlegendtext("Best Efficiency Rate");
        }
        public static void ESPcheckGassinessCurvesLegend()
        {
            ESPcheckstaticlegendtext("Lower Gassiness Curve");
            ESPcheckstaticlegendtext("Upper Gassiness Curve");
            ESPcheckstaticlegendtext("Well Test Point");

        }


        public static void GetAndClickToggleSwitchByLabel(string label, string offonstate)
        {

            HtmlControl Togglelable = TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", label, "label Toggle");
            HtmlControl nxtsbling = new HtmlControl(Togglelable.BaseElement.GetNextSibling());
            HtmlControl ctl = TelerikObject.GetChildrenControl(nxtsbling, "0;0");
            System.Collections.ObjectModel.ReadOnlyCollection<Element> elems = ctl.ChildNodes;
            Element targetfinal = elems.FirstOrDefault(x => x.InnerText == offonstate);
            TelerikObject.Click(targetfinal);

        }




        #endregion



    }
}
