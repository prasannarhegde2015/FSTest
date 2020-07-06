using ArtOfTest.WebAii.Controls.HtmlControls;
using ArtOfTest.WebAii.ObjectModel;
using ClientAutomation.TelerikCoreUtils;
using ClientAutomation.Helper;
using System.Data;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClientAutomation.PageObjects
{
    class PageWellConfig : PageMasterDefinition
    {
        internal const string htmlcontrolparentnextsibling = "htmlcontrolparentnextsibling";
        #region Controls
        public static string FilesLocation = ConfigurationManager.AppSettings.Get("FilesLocation");
        public static Element cmb_WellType { get { return TelerikObject.GetElement("Content", "Select type...", "Select Well Type"); } }
        public static Element listitem_ESP { get { return TelerikObject.GetElement("Content", "ESP", "ESP Well Type"); } }
        public static Element txt_WellName { get { return TelerikObject.GetElement("Id", "wellName", "WellName"); } }
        public static HtmlButton btn_createNewWell { get { return (HtmlButton)TelerikObject.GetElement("HtmlButton", "name", "createNewWellButton", "Create New Well Button"); } }
        public static Element btn_saveWell { get { return TelerikObject.GetElement("Xpath", "//span[text()='Save']", "Save Well  Button"); } }
        public static Element cmb_CygentDomain { get { return TelerikObject.GetElement("Content", "Select domain...", "Select cygnet Domain"); } }
        public static Element listitem_CygentDomain { get { return TelerikObject.GetElement("Content", "ESP", "ESP Well Type"); } }

        //public static Element pumpDepth { get { return TelerikObject.GetElement("xpath", "//input[@id='pumpDepth']", "Pump Depth in Rod tab"); } }
        //public static Element totalRodLength { get { return TelerikObject.GetElement("xpath", "//input[@id='totalRodLength']", "Total Rod Length in Rod tab"); } }

        public static HtmlControl dd_Rotation { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROLNEXTSIBLING, "attribute", "for=rotation", "Rotation DD in Surface tab"); } }

        public static HtmlControl dd_MotorType { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROLNEXTSIBLING, "attribute", "for=motorInfo", "Motor Type DD in Surface tab"); } }

        public static HtmlControl dd_MotorSize { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROLNEXTSIBLING, "attribute", "for=motorSize", "Motor Size DD in Surface tab"); } }

        public static HtmlControl dd_SlipTorque { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROLNEXTSIBLING, "attribute", "for=slipTorque", "Slip Torque DD in Surface tab"); } }

        public static HtmlControl txt_Up { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROLNEXTSIBLING, "attribute", "for=motorAmpsUp", "MotorAmpsUp textbox in Surface tab"); } }

        public static HtmlControl txt_Down { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROLNEXTSIBLING, "attribute", "for=motorAmpsDown", "MotorAmpsDown textbox in Surface tab"); } }

        public static HtmlControl dd_CrankId { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROLNEXTSIBLING, "attribute", "for=crankId", "Crank Id DD in Weights tab"); } }

        public static HtmlControl dd_Crank1LeadPrimaryId { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROLNEXTSIBLING, "attribute", "for=crank1LeadPrimaryId", "Crank1 Lead PrimaryId DD in Weights tab"); } }

        public static HtmlControl dd_Crank1LagPrimaryId { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROLNEXTSIBLING, "attribute", "for=crank1LagPrimaryId", "Crank1 Lag PrimaryId DD in Weights tab"); } }

        public static HtmlControl dd_Crank2LeadPrimaryId { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROLNEXTSIBLING, "attribute", "for=crank2LeadPrimaryId", "Crank2 Lead PrimaryId DD in Weights tab"); } }

        public static HtmlControl dd_Crank2LagPrimaryId { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROLNEXTSIBLING, "attribute", "for=crank2LagPrimaryId", "Crank2 Lag PrimaryId DD in Weights tab"); } }

        public static HtmlControl txt_Crank1LeadDistance { get { return TelerikObject.GetElement("kendo-numerictextbox", "Id", "crank1LeadDistance", "Crank1 Lead distance in Weights tab"); } }
        public static HtmlControl txt_Crank1LagDistance { get { return TelerikObject.GetElement("kendo-numerictextbox", "Id", "crank1LagDistance", "Crank1 Lag distance in Weights tab"); } }
        public static HtmlControl txt_Crank2LeadDistance { get { return TelerikObject.GetElement("kendo-numerictextbox", "Id", "crank2LeadDistance", "Crank2 Lead distance in Weights tab"); } }
        public static HtmlControl txt_Crank2LagDistance { get { return TelerikObject.GetElement("kendo-numerictextbox", "Id", "crank2LagDistance", "Crank2 Lag distance in Weights tab"); } }

        public static HtmlControl dd_crank1LeadAuxiliaryId { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROLNEXTSIBLING, "attribute", "for=crank1LeadAuxiliaryId", "Crank1 Lead AuxiliaryId DD in Weights tab"); } }
        public static HtmlControl dd_crank1LagAuxiliaryId { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROLNEXTSIBLING, "attribute", "for=crank1LagAuxiliaryId", "Crank1 Lag AuxiliaryId DD in Weights tab"); } }
        public static HtmlControl dd_crank2LeadAuxiliaryId { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROLNEXTSIBLING, "attribute", "for=crank2LeadAuxiliaryId", "Crank2 Lead AuxiliaryId DD in Weights tab"); } }
        public static HtmlControl dd_crank2LagAuxiliaryId { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROLNEXTSIBLING, "attribute", "for=crank2LagAuxiliaryId", "Crank2 Lag AuxiliaryId DD in Weights tab"); } }

        public static HtmlControl txt_CBT { get { return TelerikObject.GetElement("kendo-numerictextbox", "Id", "CBT", "CBT value in Weights tab"); } }

        public static HtmlControl dd_TorqueCalcMethod { get { return TelerikObject.GetElement("kendo-dropdownlist", "Id", "torqueMethod", "Torque calculation method in Weights tab"); } }

        public static HtmlControl txt_ScadaType { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROLNEXTSIBLING, "attribute", "for=scadaType", "Scada Type field in General tab"); } }
        public static HtmlControl txt_FluidType { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROLNEXTSIBLING, "attribute", "for=fluidType", "Fluid Type field in General tab"); } }
        public static HtmlControl txt_Longitude { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROLNEXTSIBLING, "attribute", "for=longitude", "longitude field in General tab"); } }

        public static HtmlControl txt_LeaseName { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROLNEXTSIBLING, "attribute", "for=lease", "Lease Name field in Well Attributes tab"); } }
        public static HtmlControl txt_fieldName { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROLNEXTSIBLING, "attribute", "for=field", "Field Name field in Well Attributes tab"); } }
        public static HtmlControl txt_Engineer { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROLNEXTSIBLING, "attribute", "for=engineer", "Engineer field in Well Attributes tab"); } }
        public static HtmlControl txt_GeoRegion { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROLNEXTSIBLING, "attribute", "for=geographicRegion", "Geographic Region field in Well Attributes tab"); } }
        public static HtmlControl txt_Foreman { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROLNEXTSIBLING, "attribute", "for=foreman", "Foreman field in Well Attributes tab"); } }
        public static HtmlControl txt_GaugerBeat { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROLNEXTSIBLING, "attribute", "for=gaugerBeat", "Gauger Beat field in Well Attributes tab"); } }

        public static HtmlControl txt_PumpDiameter { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROLNEXTSIBLING, "attribute", "for=pumpDiameter", "Pump Diameter field in Downhole tab"); } }
        public static HtmlControl txt_PumpDepth { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROLNEXTSIBLING, "attribute", "for=PumpDepth", "Pump Depth field in Downhole tab"); } }
        public static HtmlControl txt_TubingOD { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROLNEXTSIBLING, "attribute", "for=TubingOD", "Tubing OD field in Downhole tab"); } }
        public static HtmlControl txt_TubingID { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROLNEXTSIBLING, "attribute", "for=TubingID", "Tubing ID field in Downhole tab"); } }
        public static HtmlControl txt_TubingAnchorDepth { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROLNEXTSIBLING, "attribute", "for=TubingAnchorDepth", "Tubing AnchorDepth field in Downhole tab"); } }
        public static HtmlControl txt_CasingOD { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROLNEXTSIBLING, "attribute", "for=CasingOD", "Casing OD field in Downhole tab"); } }
        public static HtmlControl txt_CasingWeight { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROLNEXTSIBLING, "attribute", "for=CasingWeight", "Casing Weight field in Downhole tab"); } }
        public static HtmlControl txt_TopPerforation { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROLNEXTSIBLING, "attribute", "for=TopPerforation", "Top Perforation field in Downhole tab"); } }
        public static HtmlControl txt_BottomPerforation { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROLNEXTSIBLING, "attribute", "for=BottomPerforation", "Bottom Perforation field in Downhole tab"); } }


        //public static HtmlControl tab_FluidData { get { return TelerikObject.GetElement("kendo-panelbar-item", "Id", "k-panelbar-0-item-default-0", "Model File Fluid Data Section"); } }
        //public static HtmlControl tab_ReservoirData { get { return TelerikObject.GetElement("kendo-panelbar-item", "Id", "k-panelbar-0-item-default-1", "Model File Reservoir Data Section"); } }
        //public static HtmlControl tab_TrajectoryData { get { return TelerikObject.GetElement("kendo-panelbar-item", "id", "k-panelbar-0-item-default-2", "Model File Trajectory Data Section"); } }
        //public static HtmlControl tab_TubingData { get { return TelerikObject.GetElement("kendo-panelbar-item", "id", "k-panelbar-0-item-default-3", "Model File Tubing Data Section"); } }
        //public static HtmlControl tab_CasingData { get { return TelerikObject.GetElement("kendo-panelbar-item", "id", "k-panelbar-0-item-default-4", "Model File Casing Data Section"); } }
        //public static HtmlControl tab_RestrictionData { get { return TelerikObject.GetElement("kendo-panelbar-item", "id", "k-panelbar-0-item-default-5", "Model File Restriction Data Section"); } }
        //public static HtmlControl tab_TracePointsData { get { return TelerikObject.GetElement("kendo-panelbar-item", "id", "k-panelbar-0-item-default-6", "Model File Trace Points Data Section"); } }
        //public static HtmlControl tab_ESPData { get { return TelerikObject.GetElement("kendo-panelbar-item", "id", "k-panelbar-0-item-default-7", "ESP Model File ESP Data Section"); } }
        //public static HtmlControl tab_GasLiftData { get { return TelerikObject.GetElement("kendo-panelbar-item", "id", "k-panelbar-0-item-default-7", "GL Model File gas Lift Data Section"); } }

        public static Element tab_FluidData { get { return TelerikObject.GetElement("Xpath", "//span[contains(.,'Fluid Data')]", "Model File Fluid Data Section"); } }
        public static Element tab_ReservoirData { get { return TelerikObject.GetElement("Xpath", "//span[contains(.,'Reservoir Data')]", "Model File Reservoir Data Section"); } }
        public static Element tab_TrajectoryData { get { return TelerikObject.GetElement("Xpath", "//span[contains(.,'Trajectory Data')]", "Model File Trajectory Data Section"); } }
        public static Element tab_TubingData { get { return TelerikObject.GetElement("Xpath", "//span[contains(.,'Tubing Data')]", "Model File Tubing Data Section"); } }
        public static Element tab_CasingData { get { return TelerikObject.GetElement("Xpath", "//span[contains(.,'Casing Data')]", "Model File Casing Data Section"); } }
        public static Element tab_RestrictionData { get { return TelerikObject.GetElement("Xpath", "//span[contains(.,'Restriction Data')]", "Model File Restriction Data Section"); } }
        public static Element tab_TracePointsData { get { return TelerikObject.GetElement("Xpath", "//span[contains(.,'Trace Points Data')]", "Model File Trace Points Data Section"); } }
        public static Element tab_ESPData { get { return TelerikObject.GetElement("Xpath", "//span[contains(.,'ESP Data')]", "ESP Model File ESP Data Section"); } }
        public static Element tab_GasLiftData { get { return TelerikObject.GetElement("Xpath", "//span[contains(.,'Gas Lift Data')]", "GL Model File gas Lift Data Section"); } }

        public static HtmlInputControl txt_OilSpecificGravity { get { return (HtmlInputControl)TelerikObject.GetElement("HtmlInputControl", "id", "fdOilSpecificGravity", "Model File - Oil Specific Gravity Readonly Field "); } }
        public static HtmlInputControl txt_gasSpecificGravity { get { return (HtmlInputControl)TelerikObject.GetElement("HtmlInputControl", "id", "fdGasSpecificGravity", "Model File - Gas Specific Gravity Readonly Field "); } }
        public static HtmlInputControl txt_WaterSalinity { get { return (HtmlInputControl)TelerikObject.GetElement("HtmlInputControl", "id", "fdWaterSalinity", "Model File - Water salinity Readonly Field "); } }
        public static HtmlInputControl txt_HydrogenSulfide { get { return (HtmlInputControl)TelerikObject.GetElement("HtmlInputControl", "id", "fdHydrogenSulfide", "Model File - Hydrogen Sulfide Readonly Field "); } }
        public static HtmlInputControl txt_CarbonDioxide { get { return (HtmlInputControl)TelerikObject.GetElement("HtmlInputControl", "id", "fdCarbonDioxide", "Model File - Carbon Dioxide Readonly Field "); } }
        public static HtmlInputControl txt_Nitrogen { get { return (HtmlInputControl)TelerikObject.GetElement("HtmlInputControl", "id", "fdNitrogen", "Model File - Nitrogen Readonly Field "); } }

        public static HtmlInputControl txt_Pressure { get { return (HtmlInputControl)TelerikObject.GetElement("HtmlInputControl", "id", "rdPressure", "Model File - Pressure Readonly Field "); } }
        public static HtmlInputControl txt_Temperature { get { return (HtmlInputControl)TelerikObject.GetElement("HtmlInputControl", "id", "rdTemperature", "Model File - Temperature Readonly Field "); } }
        public static HtmlInputControl txt_MidPerfDepth { get { return (HtmlInputControl)TelerikObject.GetElement("HtmlInputControl", "id", "rdMidPerfDepth", "Model File - MidPerf Depth (MD) Readonly Field "); } }
        public static HtmlInputControl txt_WaterCut { get { return (HtmlInputControl)TelerikObject.GetElement("HtmlInputControl", "id", "rdWaterCut", "Model File - Water Cut Readonly Field "); } }
        public static HtmlInputControl txt_GOR { get { return (HtmlInputControl)TelerikObject.GetElement("HtmlInputControl", "id", "rdGOR", "Model File - Gas-Oil Ratio (GOR) Readonly Field "); } }
        public static HtmlInputControl txt_ProductivityIndex { get { return (HtmlInputControl)TelerikObject.GetElement("HtmlInputControl", "id", "rdProductivityIndex", "Model File - Productivity Index Readonly Field "); } }

        public static HtmlInputControl txt_DarcyFlowCoefficient { get { return (HtmlInputControl)TelerikObject.GetElement("HtmlInputControl", "id", "rdDarcyCoef", "Model File -Darcy Flow Coefficient  Readonly Field "); } }

        public static HtmlInputControl txt_condensateGasRatio { get { return (HtmlInputControl)TelerikObject.GetElement("HtmlInputControl", "id", "rdcondensateGasRatio", "Model File - Condensate Gas Ratio (CGR)  Readonly Field "); } }

        public static HtmlInputControl txt_waterGasRatio { get { return (HtmlInputControl)TelerikObject.GetElement("HtmlInputControl", "id", "rdwaterGasRatio", "Model File - Water Gas Ratio (WGR)  Readonly Field "); } }
        public static HtmlInputControl txt_MotorModel { get { return (HtmlInputControl)TelerikObject.GetElement("HtmlInputControl", "id", "motorModel", "ESP Model File - Motor Model Readonly Field "); } }
        public static HtmlInputControl txt_MeasuredDepth { get { return (HtmlInputControl)TelerikObject.GetElement("HtmlInputControl", "id", "pumpDepth", "ESP Model File - Measured Depth Readonly Field "); } }
        public static HtmlInputControl txt_NameplateRating { get { return (HtmlInputControl)TelerikObject.GetElement("HtmlInputControl", "id", "nameplateRating", "ESP Model File - Nameplate Rating Readonly Field "); } }
        public static HtmlInputControl txt_OperatingRating { get { return (HtmlInputControl)TelerikObject.GetElement("HtmlInputControl", "id", "operatingRating", "ESP Model File - Operating Rating Readonly Field "); } }
        public static HtmlInputControl txt_OperatingFrequency { get { return (HtmlInputControl)TelerikObject.GetElement("HtmlInputControl", "id", "operatingFrequency", "ESP Model File - Operating Frequency Readonly Field "); } }
        public static HtmlInputControl txt_MotorWearFactor { get { return (HtmlInputControl)TelerikObject.GetElement("HtmlInputControl", "id", "motorWearFactor", "ESP Model File - Motor Wear Factor Readonly Field "); } }
        public static HtmlInputControl txt_CableSize { get { return (HtmlInputControl)TelerikObject.GetElement("HtmlInputControl", "id", "cableSize", "ESP Model File - Cable Size Readonly Field "); } }
        public static HtmlInputControl txt_GasSeparatorPresent { get { return (HtmlInputControl)TelerikObject.GetElement("HtmlInputControl", "id", "gasSeparatorPresent", "ESP Model File - Gas Separator Present Readonly Field "); } }
        public static HtmlInputControl txt_SeparatorEfficiency { get { return (HtmlInputControl)TelerikObject.GetElement("HtmlInputControl", "id", "separatorEfficiency", "ESP Model File - Separator Efficiency Readonly Field "); } }

        public static HtmlInputControl txt_BHADepth { get { return (HtmlInputControl)TelerikObject.GetElement("HtmlInputControl", "id", "rdBHA", "Model File - Bottom Hole Assembly(BHA) Depth Readonly Field "); } }
        public static HtmlInputControl txt_PlungerType { get { return (HtmlInputControl)TelerikObject.GetElement("HtmlInputControl", "id", "rdPlungerType", "Model File - Plunger Type Readonly Field "); } }
        public static HtmlInputControl txt_EstimatedFallRateInGas { get { return (HtmlInputControl)TelerikObject.GetElement("HtmlInputControl", "id", "rdEstimatedFallRateInGas", "Model File - Estimated Fall rate In Gas Readonly Field "); } }
        public static HtmlInputControl txt_EstimatedFallRateInLiquid { get { return (HtmlInputControl)TelerikObject.GetElement("HtmlInputControl", "id", "rdEstimatedFallRateInLiquid", "Model File - Estimated Fall rate In Liquid Readonly Field "); } }
        public static HtmlInputControl txt_IdealRiseRate { get { return (HtmlInputControl)TelerikObject.GetElement("HtmlInputControl", "id", "rdIdealRiseRate", "Model File - Ideal Rise Rate Readonly Field "); } }
        public static HtmlInputControl txt_PressureRequiredToSurfacePlunger { get { return (HtmlInputControl)TelerikObject.GetElement("HtmlInputControl", "id", "rdPressureRequireToSurfacePlunger", "Model File - Pressure Required To Surface Plunger Readonly Field "); } }
        public static HtmlControl radio_sandWax { get { return TelerikObject.GetElement(htmlcontrolparentnextsibling, "expression", "innertext=Sand/Wax;tagname=label", "Sand/Wax"); } }
        public static HtmlControl btnDeleteWell { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "Content", "Delete", "Delete Well Button"); } }
        public static HtmlButton btnConfirmDeleteWell { get { return (HtmlButton)TelerikObject.GetElement(CONTROLTYPE_HTMLBUTTON, "Content", "Yes, Delete This Well", "Confirm Delete Well Button"); } }

        public static HtmlControl tabWeights { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "Content", "Weights", "Weights Tab"); } }
        public static HtmlControl btnCalculate { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "Content", "Calculate", "Calculate Button"); } }
        public static HtmlControl btnSettings { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "Content", "Settings", "Settings Well Button"); } }
        public static HtmlControl txt_Performancecurves { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "Content", "Performance Curves", "Performance Curves"); } }

        public static HtmlControl btnAdd { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "Content", "Add", "Add Button"); } }

        public static Element txt_frequency { get { return TelerikObject.GetElement("Xpath", "//*[@id='gridESPFrequency']//input", "Frequency text box"); } }



        public static Element closeperformancewindow { get { return TelerikObject.GetElement("Xpath", "//*[@id='kendo - dialog - title - 743933']//span", "Performance curves dialog box"); } }

        public static Element btn_frequencysave { get { return TelerikObject.GetElement("Xpath", "//*[@id='rightContent']//kendo-dialog//button[2]", " Save Performance curves dialog box"); } }
        //


        #endregion


        public static void VerifyFieldUnitandValue(DataTable dt, HtmlInputControl control, string fieldNameText, string unit)
        {
            CommonHelper.TraceLine(fieldNameText + " Value : " + control.Value.ToString());
            CommonHelper.TraceLine(fieldNameText + " unit Value : " + control.BaseElement.Parent.GetNextSibling().InnerText);

            int i = 0;
            foreach (DataRow drow in dt.Rows)
            {
                string fieldName = dt.Rows[i]["FieldName"].ToString();
                if (fieldName == fieldNameText)
                {
                    Assert.AreEqual(dt.Rows[i]["Value" + unit].ToString(), control.Value.ToString(), "Incorrect value is displayed for field " + fieldNameText);
                    if (dt.Rows[i]["UOM" + unit].ToString() != "ignoreUnit")
                        Assert.AreEqual(dt.Rows[i]["UOM" + unit].ToString(), control.BaseElement.Parent.GetNextSibling().InnerText, "Incorrect unit label is displayed for field " + fieldNameText);
                    CommonHelper.TraceLine("Validation completed for " + fieldNameText + " field");
                    break;
                }

                i++;
            }

        }

        public static void VerifyTrajectoryData(string fileName, string unit)
        {

            // TelerikObject.Click(TelerikObject.GetChildrenControl(PageObjects.PageWellConfig.tab_TrajectoryData, "0"));
            TelerikObject.Click(tab_TrajectoryData);
            Thread.Sleep(3000);
            DataTable dt = createDatatableFromXML(fileName);

            string colnames = "";

            string colnamesUS = "Measured Depthft;True Vertical Depthft;Deviation Angledeg";
            string colnamesMetric = "Measured Depthm;True Vertical Depthm;Deviation Angledeg";
            string action = "ignorevalue";
            for (int rowindex = 0; rowindex < dt.Rows.Count; rowindex++)
            {
                string measuredDepth = dt.Rows[rowindex]["MeasuredDepth" + unit].ToString();
                string trueVerticalDepth = dt.Rows[rowindex]["TrueVerticalDepth" + unit].ToString();
                string deviationAngle = dt.Rows[rowindex]["DeviationAngle" + unit].ToString();

                string colvals = action + ";" + measuredDepth + ";" + trueVerticalDepth + ";" + deviationAngle;

                if (unit == "US")
                    colnames = colnamesUS;
                else
                    colnames = colnamesMetric;

                TelerikObject.verifyGridCellValues(colnames, colvals, rowindex);
            }
            TelerikObject.Click(tab_TrajectoryData);
            // TelerikObject.Click(TelerikObject.GetChildrenControl(PageObjects.PageWellConfig.tab_TrajectoryData, "0"));
        }

        public static void VerifyESPPumpData(DataTable dt, int rowindex)
        {
            string colnames = "Top Down Id;Pump Model;Number of Stages;Head Tuning Factorfraction";

            string TopDownId = dt.Rows[rowindex]["TopDownId"].ToString();

            string PumpModel = dt.Rows[rowindex]["PumpModel"].ToString();
            string NumberOfStages = dt.Rows[rowindex]["NumberOfStages"].ToString();
            string HeadTuningfactor = dt.Rows[rowindex]["HeadTuningfactor"].ToString();

            string colvals = TopDownId + ";" + PumpModel + ";" + NumberOfStages + ";" + HeadTuningfactor;



            TelerikObject.verifyGridCellValues(colnames, colvals, rowindex);
        }
        public static void VerifyTubingCasingData(string fileName, string unit)
        {
            Thread.Sleep(3000);

            DataTable dt = createDatatableFromXML(fileName);

            string colnames = "";

            string colnamesUS = "Name;Start Point Measured Depthft;End Point Measured Depthft;External Diameterin;Internal Diameterin;Roughnessin";
            string colnamesMetric = "Name;Start Point Measured Depthm;End Point Measured Depthm;External Diameterm;Internal Diameterm;Roughnessmm";
            string action = "ignorevalue";

            for (int rowindex = 0; rowindex < dt.Rows.Count; rowindex++)
            {
                string name = dt.Rows[rowindex]["Name"].ToString();
                string startPointMeasure = dt.Rows[rowindex]["StartPointMeasure" + unit].ToString();
                string endPointMeasure = dt.Rows[rowindex]["EndPointMeasure" + unit].ToString();
                string externalDiameter = dt.Rows[rowindex]["ExternalDiameter" + unit].ToString();
                string internalDiameter = dt.Rows[rowindex]["InternalDiameter" + unit].ToString();
                string roughness = dt.Rows[rowindex]["Roughness" + unit].ToString();

                string colvals = action + ";" + name + ";" + startPointMeasure + ";" + endPointMeasure + ";" + externalDiameter + ";" + internalDiameter + ";" + roughness;

                if (unit == "US")
                    colnames = colnamesUS;
                else
                    colnames = colnamesMetric;

                TelerikObject.verifyGridCellValues(colnames, colvals, rowindex);
            }

        }

        public static void VerifyWIRestrictionData(string fileName, string unit)
        {
            Thread.Sleep(3000);
            DataTable dt = createDatatableFromXML(fileName);

            string colnames = "";

            string colnamesUS = "Name;Type;Measured Depthft;Internal Diameter64th in;Critical Flow";
            string colnamesMetric = "Name;Type;Measured Depthm;Internal Diametermm;Critical Flow";
            string action = "ignorevalue";

            for (int rowindex = 0; rowindex < dt.Rows.Count; rowindex++)
            {
                string name = dt.Rows[rowindex]["Name"].ToString();
                string type = dt.Rows[rowindex]["Type"].ToString();
                string measuredDepth = dt.Rows[rowindex]["MeasuredDepth" + unit].ToString();
                string internalDiameter = dt.Rows[rowindex]["InternalDiameter" + unit].ToString();
                string criticalFlow = dt.Rows[rowindex]["CriticalFlow"].ToString();

                string colvals = action + ";" + name + ";" + type + ";" + measuredDepth + ";" + internalDiameter + ";" + criticalFlow;

                if (unit == "US")
                    colnames = colnamesUS;
                else
                    colnames = colnamesMetric;

                TelerikObject.verifyGridCellValues(colnames, colvals, rowindex);
            }

        }

        public static void VerifyRestrictionData()
        {
            Thread.Sleep(3000);
            HtmlTable tableHeader = TelerikObject.getTableByColumnName("Name");

            string colNames = "NameTypeMeasured DepthInternal DiameterCritical Flow";
            Assert.AreEqual(colNames, tableHeader.InnerText, "Column Names of Restriction Data table are incorrect");
            HtmlTable tableContent = TelerikObject.getTableByColumnName("No records available");
            Assert.IsNotNull(tableContent, "Restriction data table has incorrect data");
        }

        public static void VerifyTracePointsData()
        {
            Thread.Sleep(3000);
            HtmlTable tableHeader1 = TelerikObject.getTableByColumnName("Name");
            string colNames1 = "NameMeasured Depth";
            Assert.AreEqual(colNames1, tableHeader1.InnerText, "Column Names of Trace Points Data table are incorrect");
            HtmlTable tableContent1 = TelerikObject.getTableByColumnName("No records available");
            Assert.IsNotNull(tableContent1, "Trace Points data table has incorrect data");
        }

        public static void VerifyGasLiftData()
        {
            HtmlTable tableHeader1 = TelerikObject.getTableByColumnName("Active");
            string colNames1 = "ActiveMeasured DepthTrue Vertical DepthManufacturerModelPort SizeTypePTRO";
            Assert.AreEqual(colNames1, tableHeader1.InnerText, "Column Names of Gas Lift Data table are incorrect");
            HtmlTable tableContent1 = TelerikObject.getTableByColumnName("No records available");
            Assert.IsNotNull(tableContent1, "Gas Lift data table has incorrect data");
        }

        public static void VerifyFluidData(string fileName, string wellType, string unit)
        {

            //TelerikObject.Click(TelerikObject.GetChildrenControl(PageObjects.PageWellConfig.tab_FluidData, "0"));
            TelerikObject.Click(tab_FluidData);
            Thread.Sleep(3000);
            DataTable dTable = createDatatableFromXML(fileName);
            if (wellType != "GI")
            {
                if (wellType != "WI")
                {
                    VerifyFieldUnitandValue(dTable, txt_OilSpecificGravity, "Oil Specific Gravity", unit);
                    VerifyFieldUnitandValue(dTable, txt_gasSpecificGravity, "Gas Specific Gravity", unit);
                    VerifyFieldUnitandValue(dTable, txt_HydrogenSulfide, "Hydrogen Sulfide (H2S)", unit);
                    VerifyFieldUnitandValue(dTable, txt_CarbonDioxide, "Carbon Dioxide (C02)", unit);
                    VerifyFieldUnitandValue(dTable, txt_Nitrogen, "Nitrogen (N2)", unit);
                }
                VerifyFieldUnitandValue(dTable, txt_WaterSalinity, "Water Salinity", unit);

            }
            else
            {
                VerifyFieldUnitandValue(dTable, txt_gasSpecificGravity, "Gas Specific Gravity", unit);
                VerifyFieldUnitandValue(dTable, txt_HydrogenSulfide, "Hydrogen Sulfide (H2S)", unit);
                VerifyFieldUnitandValue(dTable, txt_CarbonDioxide, "Carbon Dioxide (C02)", unit);
                VerifyFieldUnitandValue(dTable, txt_Nitrogen, "Nitrogen (N2)", unit);

            }
            TelerikObject.Click(tab_FluidData);
            // TelerikObject.Click(TelerikObject.GetChildrenControl(PageObjects.PageWellConfig.tab_FluidData, "0"));
        }

        public static void VerifyReservoirData(string fileName, string wellType, string unit)
        {

            // TelerikObject.Click(TelerikObject.GetChildrenControl(PageObjects.PageWellConfig.tab_ReservoirData, "0"));
            TelerikObject.Click(tab_ReservoirData);
            Thread.Sleep(3000);
            DataTable dTable = createDatatableFromXML(fileName);

            VerifyFieldUnitandValue(dTable, txt_Pressure, "Pressure", unit);
            VerifyFieldUnitandValue(dTable, txt_Temperature, "Temperature", unit);
            VerifyFieldUnitandValue(dTable, txt_MidPerfDepth, "MidPerf Depth (MD)", unit);


            if (wellType != "WI" && wellType != "GI" && wellType != "PL")
            {
                VerifyFieldUnitandValue(dTable, txt_WaterCut, "Water Cut", unit);
                VerifyFieldUnitandValue(dTable, txt_GOR, "Gas-Oil Ratio (GOR)", unit);
                VerifyFieldUnitandValue(dTable, txt_ProductivityIndex, "Productivity Index-" + wellType, unit);

            }
            else if (wellType == "WI")
                VerifyFieldUnitandValue(dTable, txt_ProductivityIndex, "Injectivity Index-" + wellType, unit);
            else if (wellType == "GI")
                VerifyFieldUnitandValue(dTable, txt_DarcyFlowCoefficient, "Darcy Flow Coefficient", unit);
            else if (wellType == "PL")
            {
                VerifyFieldUnitandValue(dTable, txt_DarcyFlowCoefficient, "Darcy Flow Coefficient", unit);
                //PageObjects.PageWellConfig.VerifyFieldUnitandValue(dTable, PageObjects.PageWellConfig.txt_condensateGasRatio, "Condensate Gas Ratio (CGR)", unit);
                VerifyFieldUnitandValue(dTable, txt_waterGasRatio, "Water Gas Ratio (WGR)", unit);
            }
            TelerikObject.Click(tab_ReservoirData);
            // TelerikObject.Click(TelerikObject.GetChildrenControl(PageObjects.PageWellConfig.tab_ReservoirData, "0"));
        }

        public static void CollapseAllSectionsOnModelData()
        {
            CommonHelper.TraceLine("Collapsing all the sections on Model Data screen");
            TelerikObject.Click(tab_FluidData);
            TelerikObject.Click(tab_ReservoirData);
            TelerikObject.Click(tab_TrajectoryData);
            TelerikObject.Click(tab_TubingData);
            TelerikObject.Click(tab_CasingData);
            TelerikObject.Click(tab_RestrictionData);
            TelerikObject.Click(tab_TracePointsData);
            // TelerikObject.Click(PageObjects.PageWellConfig.tab_ESPData);
            CommonHelper.TraceLine("Collapsed all the sections on Model Data screen");
        }



        public static DataTable createDatatableFromXML(string fileName)
        {
            string testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\TestData", fileName);
            CommonHelper.TraceLine("xml file path is: " + testdatafile);
            CommonHelper.TraceLine("Getting Data from XML file ...");
            DataTable dt = CommonHelper.BuildDataTableFromXml(testdatafile);
            return dt;

        }
    }
}