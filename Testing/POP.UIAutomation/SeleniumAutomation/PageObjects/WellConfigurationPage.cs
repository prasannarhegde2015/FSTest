using OpenQA.Selenium;
using SeleniumAutomation.SeleniumObject;

namespace SeleniumAutomation.PageObjects
{
    class WellConfigurationPage
    {
        public static string strDynamicValue = string.Empty;
        #region GenrealTab
        public static By configurationTab { get { return SeleniumActions.getByLocator("Xpath", "//div[text()='Configuration']", "Configuration Tab"); } }

        public static By DashboardTab { get { return SeleniumActions.getByLocator("Xpath", "//div[text()='Production Dashboard']", "Production Dashboard"); } }
        //
        public static By Dialogtitle { get { return SeleniumActions.getByLocator("Xpath", "//div[@class='k-window-title k-dialog-title']", "Dialog Title"); } }

        public static By Toaseter { get { return SeleniumActions.getByLocator("Xpath", "//div[contains(@class,'toast-message')]", "Toast"); } }
        public static By wellselector { get { return SeleniumActions.getByLocator("Xpath", "//well-selector", "Well selector dropdown"); } }
        public static By wellselectorinputfield { get { return SeleniumActions.getByLocator("Xpath", "//well-selector//input", "Well selector input"); } }


        public static By pedashboard { get { return SeleniumActions.getByLocator("Xpath", "//*[text()='Production, MTBF, and Failure Count']", "PE Well Trend Chart Title"); } }
        public static By wellconfigurationTab { get { return SeleniumActions.getByLocator("Xpath", "//div[text()='Well Configuration']", " Well Configuration Tab"); } }

        public static By SurveillaceTab { get { return SeleniumActions.getByLocator("Xpath", "//div[text()='Surveillance']", "Surveillance Tab"); } }

        public static By WellStatusTab { get { return SeleniumActions.getByLocator("Xpath", "//div[text()='Well Status']", "Well Status Tab"); } }

        //(//div[@class='rrl-wellstatus-left-kpi ng-star-inserted'])[1]
        public static By InfProdKPItext { get { return SeleniumActions.getByLocator("Xpath", "(//div[@class='rrl-wellstatus-left-kpi ng-star-inserted'])[1]/div", " RRL Well Status KPI"); } }

        public static By InfProdKPItextUnitBy { get { return SeleniumActions.getByLocator("Xpath", "(//div[@class='rrl-wellstatus-left-kpi ng-star-inserted'])[1]/div/span", " RRL Well Status KPI Span text"); } }
        public static By InfProdKPItextUnit { get { return SeleniumActions.getByLocator("Xpath", "(//div[@class='rrl-wellstatus-left-kpi ng-star-inserted'])[1]/div/span", " RRL Well Status KPI unit text"); } }

        public static By btnCreateNewWell { get { return SeleniumActions.getByLocator("Xpath", "//button[@name='createNewWellButton']", " Create Well Button"); } }

        public static By btnCreateNewWellby { get { return SeleniumActions.getByLocator("Xpath", "//button[@name='createNewWellButton']", " Create Well Button"); } }

        public static By btnDeleteWellby { get { return SeleniumActions.getByLocator("Xpath", "//button[@name='deleteButton']", " Delete Well Button"); } }

        public static By btnConfirmDeleteWellby { get { return SeleniumActions.getByLocator("Xpath", "//button[text()=' Yes, Delete This Well ']", " Confirm Delete Well Button"); } }
        public static By general { get { return SeleniumActions.getByLocator("Xpath", "//div[text()=' General ']", " General Tab"); } }
        public static By wellnameinput { get { return SeleniumActions.getByLocator("Id", "wellName", "Well Name text box"); } }
        public static By welltypedropdwn { get { return SeleniumActions.getByLocator("Id", "wellType", "Well Type Combo"); } }
        public static By welltypewaterinj { get { return SeleniumActions.getByLocator("Xpath", "//li[text()='Water Injection']", "Well Type Combo"); } }
        public static By welltypename { get { return SeleniumActions.getByLocator("Xpath", " //li[text()='RRL']", " navFrame"); } }
        public static By welltypefetchtextfield { get { return SeleniumActions.getByLocator("Xpath", " //kendo-dropdownlist[@id='wellType']//span[@class='k-input']", " navFrame"); } }
        public static By scadatypedrpdwn { get { return SeleniumActions.getByLocator("Id", "scadaType", "Scada Type"); } }
        public static By Fluidtypedrpdwn { get { return SeleniumActions.getByLocator("Id", "fluidType", "Fluid Type"); } }
        public static By assettypedrpdwn { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='asset']//*[@class='k-input']", "Asset"); } }
        public static By txtcontainsFiltertextbox { get { return SeleniumActions.getByLocator("Xpath", "(//kendo-dropdownlist/descendant::span[contains(text(),'Contains')])[1]//parent::*[1]/parent::*[1]/following-sibling::*", "Contains Text Box"); } }

        public static By btnFilter { get { return SeleniumActions.getByLocator("Xpath", "//button[text()='Filter']", "Filter"); } }
        //"//div[text()=' Well Attributes ']",

        public static By tabwellatrrib { get { return SeleniumActions.getByLocator("Xpath", "//div[text()=' Well Attributes ']", "Filter"); } }

        public static By firstRow { get { return SeleniumActions.getByLocator("Xpath", "(//table/tbody/tr[1]/td[1])[1]", "Facility Table first row"); } }
        public static By btnApplyFilter { get { return SeleniumActions.getByLocator("Xpath", "//button[contains(text(),'Apply')]", "Apply Button"); } }
        public static By btnApplyPumpingUnit { get { return SeleniumActions.getByLocator("Xpath", "//button[text()='Apply']", "Apply Button"); } }

        public static By facIdFilter { get { return SeleniumActions.getByLocator("Xpath", "//*[text()='Facility Id']/ancestor::th/child::*[1]/child::*[1]", "Filter Fac Id"); } }
        public static By scadatypename { get { return SeleniumActions.getByLocator("Id", " //li[text()='CygNet']", " navFrame"); } }
        public static By scadatypefetchtextfield { get { return SeleniumActions.getByLocator("Xpath", " //kendo-dropdownlist[@id='scadaType']//span[@class='k-input']", " navFrame"); } }
        public static By cygnetdomaindrpdwn { get { return SeleniumActions.getByLocator("Id", "cygNetDomain", "CygNet Domain"); } }
        public static By cygnetdomainname { get { return SeleniumActions.getByLocator("Xpath", " //li[text()='27212']", " navFrame"); } }
        public static By cygnetdomainfetchtextfield { get { return SeleniumActions.getByLocator("Xpath", " //kendo-dropdownlist[@id='cygNetDomain']//span[@class='k-input']", " navFrame"); } }
        public static By createnewwellbutton { get { return SeleniumActions.getByLocator("Xpath", " //button[@name='createNewWellButton']", " navFrame"); } }
        public static By cygnetservicedrpdwn { get { return SeleniumActions.getByLocator("Xpath", "//span[text()='Select service...']", " Selelct Service"); } }

        public static By Bycygnetservicename { get { return SeleniumActions.getByLocator("Xpath", " //li[text()='CYGNET.UIS']", " UIS Option"); } }
        public static By cygnetservicename { get { return SeleniumActions.getByLocator("Xpath", " //li[text()='CYGNET.UIS']", " navFrame"); } }
        public static By cygnetservicefetchtextfield { get { return SeleniumActions.getByLocator("Xpath", " //kendo-dropdownlist[@id='siteService']//span[@class='k-input']", " navFrame"); } }
        public static By facilitybutton { get { return SeleniumActions.getByLocator("Xpath", "//input[@id='facilityId']/following-sibling::*", "Facility Id Button"); } }
        public static By Byfacilitybutton { get { return SeleniumActions.getByLocator("Xpath", " //input[@id='facilityId']/following-sibling", "navFrame"); } }
        public static IWebElement facilitytextbox { get { return SeleniumActions.getElement("Xpath", " //input[@id='facilityId']", "navFrame"); } }
        public static IWebElement wellnametextbox { get { return SeleniumActions.getElement("Xpath", " //input[@id='wellName']", "navFrame"); } }
        public static By facilityfetchtextfield { get { return SeleniumActions.getByLocator("Xpath", "//input[@id='facilityId']", "Facility textbox"); } }
        public static By Facilityist { get { return SeleniumActions.getByLocator("Xpath", "(//*[contains(text(),'Facility Id')])[3]", "Facility list"); } }
        public static By applybutton { get { return SeleniumActions.getByLocator("Xpath", " //button[text()=' Apply ']", " Filter Apply Button"); } }
        public static By commissioninput { get { return SeleniumActions.getByLocator("Xpath", "//kendo-datepicker//input", "Well Commision Date"); } }
        //
        public static By initialspuddate { get { return SeleniumActions.getByLocator("Xpath", "//label[contains(text(),'Initial Spud Date')]/following-sibling::*/descendant::input", "Initial Spud Date"); } }
        public static By wellboreinput { get { return SeleniumActions.getByLocator("Xpath", " //kendo-combobox[@id='assembly']//input", "Well Assembly"); } }
        public static By wellborefetchtextfield { get { return SeleniumActions.getByLocator("Xpath", " //*[@id='assemblyAPI'] ", "wellboreinput filed"); } }
        public static By boreholeinput { get { return SeleniumActions.getByLocator("Xpath", "//kendo-combobox[@id='subassembly']//input", "Well Sub Assembly"); } }
        public static By boreholeinputby { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='subassembly']//input", "Bore Hole Id"); } }
        public static By boreholefetchtextfield { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='subAssemblyAPI']", "Well Sub Assembly API"); } }
        public static By intervalinput { get { return SeleniumActions.getByLocator("Xpath", "//input[@id='intervalAPI']", " navFrame"); } }
        public static By welldepthreference { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='wellDepthDatum']", " navFrame"); } }
        public static By welldepthreferencefetchtextfield { get { return SeleniumActions.getByLocator("Xpath", "//kendo-dropdownlist[@id='wellDepthDatum']//span[@class='k-input']", " navFrame"); } }
        public static By depthelevation { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@id='datumElevation']//input", "navFrame"); } }
        public static By groundelevation { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@id='datumGroundElevation']//input", "navFrame"); } }
        public static By wellstatusdrpdwn { get { return SeleniumActions.getByLocator("Xpath", " wellStatus ", "navFrame"); } }
        public static By txtLatitude { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@id='latitude']/descendant::input[1]", "Latitude textbox"); } }
        public static By txtLongitude { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@id='longitude']/descendant::input[1]", "Longitude textbox"); } }
        //kendo-numerictextbox[@id='latitude']/descendant::input[1]
        public static By wellstatusfetchtextfield { get { return SeleniumActions.getByLocator("Xpath", "//kendo-dropdownlist[@id='wellStatus']//span[@class='k-input']", " navFrame"); } }
        public static By wellattributetabby { get { return SeleniumActions.getByLocator("Xpath", "//div[text()=' Well Attributes ']", " navFrame"); } }

        public static By wellattributetab { get { return SeleniumActions.getByLocator("Xpath", "//div[text()=' Well Attributes ']", " Well Attributes Tab "); } }
        public static By leasename { get { return SeleniumActions.getByLocator("Xpath", "//kendo-combobox[@name='lease']//input", "Lease Text Box"); } }
        public static By fieldname { get { return SeleniumActions.getByLocator("Xpath", "//kendo-combobox[@name='field']//input", "Filed name"); } }
        public static By engineername { get { return SeleniumActions.getByLocator("Xpath", "//kendo-combobox[@name='engineer']//input", "Engineer Name"); } }
        public static By regionname { get { return SeleniumActions.getByLocator("Xpath", "//kendo-combobox[@name='geographicRegion']//input", "Geographic Region"); } }
        public static By foremanname { get { return SeleniumActions.getByLocator("Xpath", "//kendo-combobox[@name='foreman']//input", "Foreman"); } }
        public static By gaugername { get { return SeleniumActions.getByLocator("Xpath", "//kendo-combobox[@name='gaugerBeat']//input", "Gauger Beat"); } }
        public static By savebutton { get { return SeleniumActions.getByLocator("Xpath", " //span[text()='Save']", " Save Button"); } }

        public static By dashbord_filter { get { return SeleniumActions.getByLocator("Xpath", "//div[@id='btnsystemrailgroupfilter']", "Dashboard Filter"); } }

        public static By assets_validation { get { return SeleniumActions.getByLocator("Xpath", "//div/kendo-multiselect//descendant::ul/li/span[contains(text(),'TestAsset')]", "Asset Filter"); } }

        public static By assets_filter1 { get { return SeleniumActions.getByLocator("Xpath", "//kendo-searchbar/input[1]", "Asset Filter Selection"); } }

        public static By applybtn { get { return SeleniumActions.getByLocator("Xpath", "//button[@class='k-button ng-star-inserted'][contains(text(),'Apply')]", "Group Selection Apply"); } }
        public static By deletebutton { get { return SeleniumActions.getByLocator("Xpath", "//span[text()='Delete']", " Delete button"); } }

        public static By firstdelete { get { return SeleniumActions.getByLocator("Xpath", "//button[contains(text(),'Yes, Delete This Well')]", "Confirm Delete 1"); } }
        public static By seconddelete { get { return SeleniumActions.getByLocator("Xpath", "//button[contains(text(),'Yes, Delete This Well')]", "Confirm Delete 2"); } }
        #endregion


        #region SurfacParam
        public static By btnPumpingUnit { get { return SeleniumActions.getByLocator("Xpath", "//button[@id='openPumpingUnitModalButton']", "Pumping Unit button"); } }
        public static By txtPumpUnitfetch { get { return SeleniumActions.getByLocator("Xpath", "//input[@id='pumpingUnit']", "Pumping Unit fetch feild"); } }
        //  //input[@id='pumpingUnit']
        public static By tabSurface { get { return SeleniumActions.getByLocator("Xpath", "//div[text()=' Surface ']", "Surface Tab"); } }


        public static By cellwithText { get { return SeleniumActions.getByLocator("Xpath", "//td[text()='" + strDynamicValue + "']", "Cell having text:  " + strDynamicValue); } }

        public static By cellwithTextContains { get { return SeleniumActions.getByLocator("Xpath", "//td[contains(text(),'" + strDynamicValue + "')]", "Cell having text:  " + strDynamicValue); } }

        public static By filterColumnName { get { return SeleniumActions.getByLocator("Xpath", "//*[text()='" + strDynamicValue + "']/ancestor::th/child::*[1]/child::*[1]", "Cell having text:  " + strDynamicValue); } }

        public static By txtFilterdlgFirstContains { get { return SeleniumActions.getByLocator("Xpath", "(//kendo-dropdownlist/descendant::span[contains(text(),'Contains')])[1]//parent::*[1]/parent::*[1]/following-sibling::*", "Column Filter First Contians textbox"); } }
        //  //label[text()=' Wrist Pin ']/following-sibling::*[1]

        public static By kendoddWristPin { get { return SeleniumActions.getByLocator("Xpath", "//label[text()=' Wrist Pin ']/following-sibling::*[1]", "Wrist Pin dropdown"); } }
        public static By kendoddRotation { get { return SeleniumActions.getByLocator("Xpath", "//label[text()=' Rotation ']/following-sibling::*[1]", "Rotation dropdown"); } }

        public static By txtActualStrokeLength { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@id='actualStrokeLength']//span//input", "Actual Stroke Lenght textbox"); } }
        //  
        public static By kendoddMotortype { get { return SeleniumActions.getByLocator("Xpath", "//label[text()=' Motor Type ']/following-sibling::*[1]", "Motor Type dropdown"); } }
        public static By kendoddMotorSize { get { return SeleniumActions.getByLocator("Xpath", "//label[text()=' Motor Size ']/following-sibling::*[1]", "Motor Size dropdown"); } }

        public static By kendoddSliptorque { get { return SeleniumActions.getByLocator("Xpath", "//label[text()=' Slip Torque ']/following-sibling::*[1]", "Slip Torque dropdown"); } }

        public static By txtUpAmps { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@id='motorAmpsUp']/descendant::input[1]", "motorAmpsUp textbox"); } }
        public static By txtdownAmps { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@id='motorAmpsDown']/descendant::input[1]", "motorAmpsDown textbox"); } }
        //

        // 
        #endregion

        #region Weights
        public static By tabWeights { get { return SeleniumActions.getByLocator("Xpath", "//div[text()=' Weights ']", "Weights Tab"); } }
        public static By kendoddCrankId { get { return SeleniumActions.getByLocator("Xpath", "//label[text()=' Crank Id ']/following-sibling::*[1]", "Crank Id dropdown"); } }

        public static By kendoddC1LeadPId { get { return SeleniumActions.getByLocator("Xpath", "(//label[text()=' Primary Id ']/following-sibling::*[1])[1]", "C1LeadPId"); } }
        public static By kendoddC1LagPId { get { return SeleniumActions.getByLocator("Xpath", "(//label[text()=' Primary Id ']/following-sibling::*[1])[2]", "C1LagPId"); } }
        public static By kendoddC1LeadAId { get { return SeleniumActions.getByLocator("Xpath", "(//label[text()=' Auxiliary Id ']/following-sibling::*[1])[1]", "C1LeadAId"); } }
        public static By kendoddC1LagAId { get { return SeleniumActions.getByLocator("Xpath", "(//label[text()=' Auxiliary Id ']/following-sibling::*[1])[2]", "C1LagAId"); } }

        public static By kendoddC2LeadPId { get { return SeleniumActions.getByLocator("Xpath", "(//label[text()=' Primary Id ']/following-sibling::*[1])[3]", "C2LeadPId"); } }
        public static By kendoddC2LagPId { get { return SeleniumActions.getByLocator("Xpath", "(//label[text()=' Primary Id ']/following-sibling::*[1])[4]", "C2LagPId"); } }
        public static By kendoddC2LeadAId { get { return SeleniumActions.getByLocator("Xpath", "(//label[text()=' Auxiliary Id ']/following-sibling::*[1])[3]", "C2LeadAId"); } }
        public static By kendoddC2LagAId { get { return SeleniumActions.getByLocator("Xpath", "(//label[text()=' Auxiliary Id ']/following-sibling::*[1])[4]", "C2LagAId"); } }

        public static By kendoddTorqueCalculationMethod { get { return SeleniumActions.getByLocator("Xpath", "//label[text()=' Torque Calculation Method ']/following-sibling::*[1]", " Torque Calculation Method "); } }
        #endregion

        #region DownHole
        public static By tabDownhole { get { return SeleniumActions.getByLocator("Xpath", "//div[text()=' Downhole ']", "Downhole Tab"); } }
        public static By kendoDDPumpDiameter { get { return SeleniumActions.getByLocator("Xpath", "(//kendo-dropdownlist[@id='pumpDiameter']/span/span)[1]", "Pump Diameter dropdown"); } }
        public static By txtPumpDepth { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@id='PumpDepth']//input", "PumpDepth Textbox"); } }
        public static By txtTubingID { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@id='TubingID']//input", "TubingID Textbox"); } }

        public static By txtTubingOD { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@id='TubingOD']//input", "TubingOD Textbox"); } }
        public static By txtTubingAnchorDepth { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@id='TubingAnchorDepth']//input", "TubingAnchorDepth Textbox"); } }
        public static By txtCasingOD { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@id='CasingOD']//input", "CasingOD Textbox"); } }
        public static By txtCasingWeight { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@id='CasingWeight']//input", "CasingWeight Textbox"); } }
        public static By txtTopPerforation { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@id='TopPerforation']//input", "TopPerforation Textbox"); } }
        public static By txtBottomPerforation { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@id='BottomPerforation']//input", "BottomPerforation Textbox"); } }
        #endregion

        #region Rod Details
        public static By txtPumpDepthRodstab { get { return SeleniumActions.getByLocator("Xpath", "//input[@id='pumpDepth']", "Pump Depth"); } }
        public static By txtTotalRodlength { get { return SeleniumActions.getByLocator("Xpath", "//input[@id='totalRodLength']", "Total Rod Lentgh"); } }
        public static By tabRods { get { return SeleniumActions.getByLocator("Xpath", "//div[text()=' Rods ']", "Rods Tab"); } }
        public static By btnAddRod { get { return SeleniumActions.getByLocator("Xpath", "//span[text()='Add ']", "Button Add rod"); } }
        public static By colManufacturer { get { return SeleniumActions.getByLocator("Xpath", "(//table/tbody/tr/td)[3]/kendo-dropdownlist", "Col Manf"); } }
        public static By colGrade { get { return SeleniumActions.getByLocator("Xpath", "(//table/tbody/tr/td)[4]/kendo-dropdownlist", "Col Grade"); } }
        public static By colSize { get { return SeleniumActions.getByLocator("Xpath", "(//table/tbody/tr/td)[5]/kendo-dropdownlist", "Col Size"); } }
        public static By colLength { get { return SeleniumActions.getByLocator("Xpath", "(//table/tbody/tr/td)[6]/kendo-numerictextbox//input", "Col Length"); } }

        public static By colRodcount { get { return SeleniumActions.getByLocator("Xpath", "(//table/tbody/tr/td)[7]/kendo-numerictextbox//input", "Rodcount"); } }

        public static By colGuides { get { return SeleniumActions.getByLocator("Xpath", "(//table/tbody/tr/td)11]/kendo-numerictextbox//input", "Guides"); } }

        public static By btnrowSave { get { return SeleniumActions.getByLocator("Xpath", "(//table/tbody/tr/td)[1]/span/button[3]", "Row SAvButon"); } }



        public static void AddRodRows(string size, string rdcount)
        {
            SeleniumActions.waitClick(btnAddRod);
            SeleniumActions.selectKendoDropdownValue(colManufacturer, "Weatherford, Inc.");
            SeleniumActions.selectKendoDropdownValue(colGrade, "D");
            SeleniumActions.selectKendoDropdownValue(colSize, size);

            SeleniumActions.sendText(colLength, "30");
            SeleniumActions.sendText(colRodcount, rdcount);

            SeleniumActions.waitClick(btnrowSave);
        }

        #endregion

        #region MOPForm
        public static By btnChangeWellType { get { return SeleniumActions.getByLocator("Xpath", "//button[@name='changeWellTypeButton']", "Change Well Type Button"); } }
        //viewHistoryButton
        public static By btnViewHistory { get { return SeleniumActions.getByLocator("Xpath", "//button[@name='viewHistoryButton']", "View Well History Button"); } }
        public static By btnConfirmChangeWellType
        {
            get
            {
                return SeleniumActions.getByLocator("Xpath", "//button[text()=' Yes. Change well type. ']", "Confirm Change Well Type Button");
                //
            }
        }



        //
        ///
        public static By kendoDDNewWellType { get { return SeleniumActions.getByLocator("Xpath", "//label[text()=' New Well Type ']/parent::div/following-sibling::*[1]/kendo-dropdownlist", "New Well Type"); } }

        public static By txtMOPComment { get { return SeleniumActions.getByLocator("Xpath", "//textarea[@formcontrolname='comment']", "MOP Comment"); } }

        public static By txtWellTypeChangeDate { get { return SeleniumActions.getByLocator("Xpath", "//kendo-datepicker[@formcontrolname='mopChangeDate']//descendant::input[1]", "MOP Change Date"); } }

        public static By btnMOPSave { get { return SeleniumActions.getByLocator("Xpath", "//button[@dir='ltr' and text()=' Save ' ]", "MOP Save buton"); } }
        // 
        public static By lblWellListHeader
        {
            get { return SeleniumActions.getByLocator("Xpath", "//div[@class='well-list-header' and text()='Well List']", "Well List Header"); }
        }
        #endregion

        #region modeloptions
        public static By btnmodelfiledate { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='modelImportButtonDiv']", "Modelfiledate"); } }
        public static IWebElement selectmodelfile { get { return SeleniumActions.getElement("Xpath", "//kendo-upload//div[@role='button']", "selectmodelfile"); } }
        public static By applicabledate { get { return SeleniumActions.getByLocator("Xpath", "//kendo-dialog//*//input[@class='k-input']", "applicabledate"); } }
        public static By modelcomment { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='modelImportComment']", "modelcomment"); } }
        public static By tuningmethod { get { return SeleniumActions.getByLocator("Xpath", "//kendo-dialog//*[@role='listbox']", "tuningmethod"); } }
        public static By modelapply { get { return SeleniumActions.getByLocator("Xpath", "//*[contains(text(),'Apply')]", "modelapply"); } }
        #endregion

        #region modeldata
        public static By tabmodel { get { return SeleniumActions.getByLocator("Xpath", "//*[contains(text(),'Model Data')]", "modeltab"); } }
        public static By rdbtnNone { get { return SeleniumActions.getByLocator("Xpath", "//input[@id='None']", "None radio btn"); } }
        public static By rdbtnSand { get { return SeleniumActions.getByLocator("Xpath", "//input[@id='Sand']", "Sand radio btn"); } }
        public static By rdbtnWax { get { return SeleniumActions.getByLocator("Xpath", "//input[@id='Wax']", "Wax radio btn"); } }
        public static string lbloilspecificgravity = "//*[@id='fdOilSpecificGravity']";
        public static string lblgasspecificgravity = "//*[@id='fdGasSpecificGravity']";
        public static string lblWaterSalinity = "//*[@id='fdWaterSalinity']";
        public static string lblHydrogenSulfide = "//*[@id='fdHydrogenSulfide']";
        public static string lblCarbonDioxide = "//*[@id='fdCarbonDioxide']";
        public static string lblNitrogen = "//*[@id='fdNitrogen']";


        public static string lblpressure = "//*[@id='rdPressure']";
        public static string lbltemperature = "//*[@id='rdTemperature']";
        public static string lblmidperfdepth = "//*[@id='rdMidPerfDepth']";
        public static string lblwatercut = "//*[@id='rdWaterCut']";
        public static string lblgor = "//*[@id='rdGOR']";
        public static string lblproductivityindex = "//*[@id='rdProductivityIndex']";
        public static string lblwgr = "//*[@id='rdwaterGasRatio']";
        public static string lblinjindex = "//*[@id='rdProductivityIndex']";
        public static string lbldarcycoeff = "//*[@id='rdDarcyCoef']";
        public static string tbltrajectoryheader = "(//table)[1]";
        public static string tbltrajectorygrid = "(//table)[2]/tbody";
        public static string tbltubingheader = "(//table)[3]";
        public static string tbltubinggrid = "(//table)[4]/tbody";
        public static string tblcasingheader = "(//table)[5]";
        public static string tblcasinggrid = "(//table)[6]/tbody";
        public static string tblrestrictionheader = "(//table)[7]";
        public static string tblrestrictiongrid = "(//table)[8]/tbody";
        public static string tbltracepointsheader = "(//table)[9]";
        public static string tbltracepointsgrid = "(//table)[10]/tbody";
        public static string tblpumpheader = "(//table)[11]";
        public static string tblgasliftheader = "(//table)[11]";
        public static string tblpumpgrid = "(//table)[12]/tbody";
        public static string tblgasliftgrid = "(//table)[12]/tbody";

        public static string lblmotormodel = "//*[@id='motorModel']";
        public static string lblmeasureddepth = "//*[@id='pumpDepth']";
        public static string lblnameplaterating = "//*[@id='nameplateRating']";
        public static string lbloperatingrating = "//*[@id='operatingRating']";
        public static string lbloperatingfrequency = "//*[@id='operatingFrequency']";
        public static string lblmotorwearfactor = "//*[@id='motorWearFactor']";
        public static string lblcablesize = "//*[@id='cableSize']";
        public static string lblgasseparatorpresent = "//*[@id='gasSeparatorPresent']";
        public static string lblseparatorefficiency = "//*[@id='separatorEfficiency']";
        //PlungerliftData
        public static string lblbottomhole = "//*[@id='rdBHA']";
        public static string lblplungertype = "//*[@id='rdPlungerType']";
        public static string lblfallingas = "//*[@id='rdEstimatedFallRateInGas']";
        public static string lblfallinliquid = "//*[@id='rdEstimatedFallRateInLiquid']";
        public static string lblidealriserate = "//*[@id='rdIdealRiseRate']";
        public static string lblpressurereq = "//*[@id='rdPressureRequireToSurfacePlunger']";
        #endregion
        #region WellSettings
        public static By btnsettings { get { return SeleniumActions.getByLocator("Xpath", "//span[contains(text(),'Settings')]", "Settings"); } }
        public static By lnkaddlwellattributes { get { return SeleniumActions.getByLocator("Xpath", "//li[contains(text(),'Additional Well Attributes')]", "Additional Well Attributes"); } }
        public static By lnktargetconfig { get { return SeleniumActions.getByLocator("Xpath", "//li[contains(text(),'Target Configuration')]", "Target Configuration"); } }

        public static By lnktargetadd { get { return SeleniumActions.getByLocator("Xpath", "//kendo-dialog-actions/button[contains(text(),'Add')]", "Add Target Configuration"); } }
        public static By lnktargetsave { get { return SeleniumActions.getByLocator("Xpath", "//kendo-dialog-actions/button[contains(text(),'Save')]", "Save Target Configuration"); } }
        public static By lnktargetclose { get { return SeleniumActions.getByLocator("Xpath", "//div[@class='k-window-actions k-dialog-actions']", "Close Target Configuration"); } }
        public static By lnktargetchkbox { get { return SeleniumActions.getByLocator("Xpath", "(//input[@type='checkbox'])[2]", "Checkbox Target Configuration"); } }
        public static By lnktargetdelete { get { return SeleniumActions.getByLocator("Xpath", "//kendo-dialog-actions/button[contains(text(),'Delete')]", "Delete Target Configuration"); } }
        public static By lnktargetdeleteconfirm { get { return SeleniumActions.getByLocator("Xpath", "(//kendo-dialog-actions//button[contains(text(),'Delete')])[1]", "Confirm Delete Target Configuration"); } }
        public static By txttargetstart { get { return SeleniumActions.getByLocator("Xpath", "//kendo-datepicker[@formcontrolname='StartDateJS']//input", "Start Date"); } }
        public static By txttargetend { get { return SeleniumActions.getByLocator("Xpath", "//kendo-datepicker[@formcontrolname='EndDateJS']//input", "End Date"); } }
        public static By txtoillowerbound { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@formcontrolname='OilLowerBound']//input", "Oil lower bound"); } }
        public static By txtoiltarget { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@formcontrolname='OilTarget']//input", "Oil Target"); } }
        public static By txtoilupperbound { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@formcontrolname='OilUpperBound']//input", "Oil Upper bound"); } }
        public static By txtoilminimum { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@formcontrolname='OilMinimum']//input", "Oil Minimum"); } }
        public static By txtwaterlowerbound { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@formcontrolname='WaterLowerBound']//input", "Water lower bound"); } }
        public static By txtwatertarget { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@formcontrolname='WaterTarget']//input", "Water Target"); } }
        public static By txtwaterupperbound { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@formcontrolname='WaterUpperBound']//input", "Water Upper bound"); } }
        public static By txtwaterminimum { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@formcontrolname='WaterMinimum']//input", "Water Minimum"); } }
        public static By txtgaslowerbound { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@formcontrolname='GasLowerBound']//input", "Gas lower bound"); } }
        public static By txtgastarget { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@formcontrolname='GasTarget']//input", "Gas Target"); } }
        public static By txtgasupperbound { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@formcontrolname='GasUpperBound']//input", "Gas Upper bound"); } }
        public static By txtgasminimum { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@formcontrolname='GasMinimum']//input", "Gas Minimum"); } }
        public static By txtoiltechnicallimit { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@formcontrolname='OilTechnicalLimit']//input", "Oil Technical limit"); } }
        public static By txtwatertechnicallimit { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@formcontrolname='WaterTechnicalLimit']//input", "Water Technical limit"); } }
        public static By txtgastechnicallimit { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@formcontrolname='GasTechnicalLimit']//input", "Gas Technical limit"); } }
        public static By btntgtclose { get { return SeleniumActions.getByLocator("Xpath", "//*[@class='k-icon k-i-x']", "Target config close button"); } }


        #endregion

        #region Acceptancesettings
        public static By lnkacceptancelimits { get { return SeleniumActions.getByLocator("Xpath", "//li[contains(text(),'Acceptance Limits')]", "Acceptance Limits"); } }
        public static By lbllfactor { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='acceptanceLimitsDialog']//label[contains(text(),'L Factor')]", "L Factor"); } }
        public static By lbloperatingpoint { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='acceptanceLimitsDialog']//label[contains(text(),'Operating Point')]", "Operating Point"); } }
        public static By lblreservoirPressure { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='acceptanceLimitsDialog']//label[contains(text(),'Reservoir Pressure')]", "Reservoir Pressure"); } }
        public static By lblChokeDFactor { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='acceptanceLimitsDialog']//label[contains(text(),'Choke D Factor')]", "Choke D Factor"); } }
        public static By lblWaterCut { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='acceptanceLimitsDialog']//label[contains(text(),'Water Cut (WCT)')]", "Water Cut (WCT)"); } }
        public static By lblaccptgor { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='acceptanceLimitsDialog']//label[contains(text(),'Gas-Oil Ratio (GOR)')]", "Gas-Oil Ratio (GOR)"); } }
        public static By lblPI { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='acceptanceLimitsDialog']//label[contains(text(),'Productivity Index (PI)')]", "Productivity Index (PI)"); } }
        public static By lblwatersalinity { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='acceptanceLimitsDialog']//label[contains(text(),'Water Salinity')]", "Water Salinity"); } }
        public static By lblHeadTuningFactor { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='acceptanceLimitsDialog']//label[contains(text(),'Head Tuning Factor')]", "Head Tuning Factor"); } }
        public static By lblperctdiff { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='acceptanceLimitsDialog']//div[contains(text(),'Percent Difference')]", "Percent Difference"); } }
        public static By lblestimatedwellhdpr { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='acceptanceLimitsDialog']//label[contains(text(),'Estimated Tubing Head Pressure')]", "Estimated Tubing Head Pressure"); } }
        public static By lblwellhdpr { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='acceptanceLimitsDialog']//label[contains(text(),'Tubing Head Pressure (THP)')]", "Tubing Head Pressure (THP)"); } }
        public static By lblgor2 { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='acceptanceLimitsDialog']//label[contains(text(),'Gas-Oil Ratio (GOR)')]", "Gas-Oil Ratio (GOR)"); } }
        public static By lbloilrate { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='acceptanceLimitsDialog']//label[contains(text(),'Oil Rate (Qo)')]", "Oil Rate"); } }
        public static By lblliquidrate { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='acceptanceLimitsDialog']//label[contains(text(),'Liquid Rate (Ql)')]", "liquid Rate"); } }
        public static By lblwatercut2 { get { return SeleniumActions.getByLocator("Xpath", "(//*[@id='acceptanceLimitsDialog']//label[contains(text(),'Water Cut (WCT)')])[2]", "water cut"); } }

        public static By overridelfactor { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='chklFactor']//span[@class='k-switch-handle']", "L Factor"); } }
        public static By overrideoperatingpoint { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='chkOperatingPoint']//span[@class='k-switch-handle']", "Operating Point"); } }
        public static By overridereservoirPressure { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='chkReservoirPressure']//span[@class='k-switch-handle']", "Reservoir Pressure"); } }
        public static By overrideChokeDFactor { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='chkChokeFactor']//span[@class='k-switch-handle']", "Choke D Factor"); } }
        public static By overrideWaterCut { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='chkWaterCut']//span[@class='k-switch-handle']", "Water Cut (WCT)"); } }
        public static By overrideaccptgor { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='chkGasOilRatio']//span[@class='k-switch-handle']", "Gas-Oil Ratio (GOR)"); } }
        public static By overridePI { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='chkProductivityIndex']//span[@class='k-switch-handle']", "Productivity Index (PI)"); } }
        public static By overridewatersalinity { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='chkWaterSalinity']//span[@class='k-switch-handle']", "Water Salinity"); } }
        public static By overrideHeadTuningFactor { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='chkWearFactor']//span[@class='k-switch-handle']", "Head Tuning Factor"); } }

        public static By overrideestimatedwellhdpr { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='chkEstimatedWHPDifference']//span[@class='k-switch-handle']", "Estimated Wellhead Pressure"); } }
        public static By overridewellhdpr { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='chkwhpDifference']//span[@class='k-switch-handle']", "Wellhead Pressure"); } }
        public static By overridegor2 { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='chkGorDifference']//span[@class='k-switch-handle']", "Gas-Oil Ratio"); } }
        public static By overrideoilrate { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='chkOilRateDifference']//span[@class='k-switch-handle']", "Oil Rate"); } }
        public static By overrideliquidrate { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='chkLiquidRateDifference']//span[@class='k-switch-handle']", "liquid Rate"); } }
        public static By overridewatercut2 { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='chkWaterCutDifference']//span[@class='k-switch-handle']", "water cut"); } }

        public string txtlfactormin = "//*[@id='lFactorMin']/span/input";
        public string operatingPointMin = "//*[@id='operatingPointMin']/span/input";
        public string reservoirPressureMin = "//*[@id='reservoirPressureMin']/span/input";
        public string chokeFactorMin = "//*[@id='chokeFactorMin']/span/input";
        public string waterCutMin = "//*[@id='waterCutMin']/span/input";
        public string gasOilRatioMin = "//*[@id='gasOilRatioMin']/span/input";
        public string productivityIndexMin = "//*[@id='productivityIndexMin']/span/input";
        public string waterSalinityMin = "//*[@id='waterSalinityMin']/span/input";
        public string wearFactorMin = "//*[@id='wearFactorMin']/span/input";
        public static By txtestimatedWHPDifferenceMax { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='estimatedWHPDifferenceMax']/span/input", "Estimated wellhead pressure percentage"); } }
        public static By txtdefaultWhpDifferenceMax { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='defaultWhpDifferenceMax']/span/input", "wellhead pressure percentage"); } }
        public static By txtgorDifferenceMax { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='gorDifferenceMax']/span/input", "GOR percentage"); } }
        public static By txtoilRateDifferenceMax { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='oilRateDifferenceMax']/span/input", "Oil Rate diff percentage"); } }
        public static By txtliquidRateDifferenceMax { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='liquidRateDifferenceMax']/span/input", "Liquid Rate diff percentage"); } }
        public static By txtwaterCutDifferenceMax { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='waterCutDifferenceMax']/span/input", "Water cut diff percentage"); } }
        public static By btnaccptsave { get { return SeleniumActions.getByLocator("Xpath", "//button[contains(text(),'Save')]", "Save"); } }
        #endregion

        #region operatingsettings
        public static By lnkoperatinglimits { get { return SeleniumActions.getByLocator("Xpath", "//li[contains(text(),'Operating Limits')]", "Operating Limits"); } }
        public static By lbltubingheadpressure { get { return SeleniumActions.getByLocator("Xpath", "//*[@class='k-content k-window-content k-dialog-content']//*[contains(text(),'Tubing Head Pressure (THP)')]", "Tubing Head Pressure"); } }
        public static By lbltubingheadtemp { get { return SeleniumActions.getByLocator("Xpath", "//*[@class='k-content k-window-content k-dialog-content']//*[contains(text(),'Tubing Head Temperature (THT)')]", "Tubing Head Temperature"); } }
        public static By lblcasingheadPressure { get { return SeleniumActions.getByLocator("Xpath", "//*[@class='k-content k-window-content k-dialog-content']//*[contains(text(),'Casing Head Pressure (CHP)')]", "Casing Head Pressure"); } }
        public static By lblopoilrate { get { return SeleniumActions.getByLocator("Xpath", "//*[@class='k-content k-window-content k-dialog-content']//*[contains(text(),'Oil Rate')]", "Oil Rate"); } }
        public static By lblopwaterrate { get { return SeleniumActions.getByLocator("Xpath", "//*[@class='k-content k-window-content k-dialog-content']//*[contains(text(),'Water Rate')]", "Water Rate"); } }
        public static By lblopgasrate { get { return SeleniumActions.getByLocator("Xpath", "(//*[@class='k-content k-window-content k-dialog-content']//*[contains(text(),'Gas Rate')])[1]", "Gas Rate"); } }
        public static By lblpumpintakepressure { get { return SeleniumActions.getByLocator("Xpath", "//*[@class='k-content k-window-content k-dialog-content']//*[contains(text(),'Pump Intake Pressure')]", "Pump Intake Pressure"); } }
        public static By lblpumpdischargepressure { get { return SeleniumActions.getByLocator("Xpath", "//*[@class='k-content k-window-content k-dialog-content']//*[contains(text(),'Pump Discharge Pressure')]", "Pump Discharge Pressure"); } }
        public static By lblflowlinepressure { get { return SeleniumActions.getByLocator("Xpath", "//*[@class='k-content k-window-content k-dialog-content']//*[contains(text(),'Flowline Pressure')]", "Flowline Pressure"); } }
        public static By lblcasinggasrate { get { return SeleniumActions.getByLocator("Xpath", "//*[@class='k-content k-window-content k-dialog-content']//*[contains(text(),'Casing Gas Rate')]", "Casing Gas Late"); } }
        public static By lblmotorfreq { get { return SeleniumActions.getByLocator("Xpath", "//*[@class='k-content k-window-content k-dialog-content']//*[contains(text(),'Motor Frequency')]", "Motor Frequency"); } }
        public static By lblmotorvolts { get { return SeleniumActions.getByLocator("Xpath", "//*[@class='k-content k-window-content k-dialog-content']//*[contains(text(),'Motor Volts')]", "Motor Volts"); } }
        public static By lblmotoramps { get { return SeleniumActions.getByLocator("Xpath", "//*[@class='k-content k-window-content k-dialog-content']//*[contains(text(),'Motor Amps')]", "Motor Amps"); } }
        public static By lblmotortemp { get { return SeleniumActions.getByLocator("Xpath", "//*[@class='k-content k-window-content k-dialog-content']//*[contains(text(),'Motor Temperature')]", "Motor Temperature"); } }
        public static By lblchokedia { get { return SeleniumActions.getByLocator("Xpath", "//*[@class='k-content k-window-content k-dialog-content']//*[contains(text(),'Choke Diameter')]", "Choke dia"); } }
        public static By lblruntime { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='acceptanceLimitsDialog']//*[contains(text(),'Runtime')]", "Runtime"); } }
        public static By lblwellname { get { return SeleniumActions.getByLocator("Xpath", "//*[contains(text(),' Well Name')]", "Well name label"); } }

        public static By overridetubpressure { get { return SeleniumActions.getByLocator("Xpath", "//*[contains(text(),'Tubing Head Pressure (THP)')]/following-sibling::*[2]//span[@class='k-switch-handle']", "Tubing Head Pressure"); } }
        public static By overridetubtemp { get { return SeleniumActions.getByLocator("Xpath", "//*[contains(text(),'Tubing Head Temperature (THT)')]/following-sibling::*[2]//span[@class='k-switch-handle']", "Tubing Head Temperature"); } }
        public static By overridecasingheadPressure { get { return SeleniumActions.getByLocator("Xpath", "//*[contains(text(),'Casing Head Pressure (CHP)')]/following-sibling::*[2]//span[@class='k-switch-handle']", "Casing Head Pressure"); } }
        public static By overrideopoilrate { get { return SeleniumActions.getByLocator("Xpath", "//*[contains(text(),'Oil Rate')]/following-sibling::*[2]//span[@class='k-switch-handle']", "Oil Rate"); } }
        public static By overrideopwaterrate { get { return SeleniumActions.getByLocator("Xpath", "//*[contains(text(),'Water Rate')]/following-sibling::*[2]//span[@class='k-switch-handle']", "Water Rate"); } }
        public static By overrideopgasrate { get { return SeleniumActions.getByLocator("Xpath", "//*[contains(text(),'Gas Rate')]/following-sibling::*[2]//span[@class='k-switch-handle']", "Gas Rate"); } }
        public static By overridepumpintakepressure { get { return SeleniumActions.getByLocator("Xpath", "//*[contains(text(),'Pump Intake Pressure')]/following-sibling::*[2]//span[@class='k-switch-handle']", "Pump Intake Pressure"); } }
        public static By overridepumpdischargepressure { get { return SeleniumActions.getByLocator("Xpath", "//*[contains(text(),'Pump Discharge Pressure')]/following-sibling::*[2]//span[@class='k-switch-handle']", "Pump Discharge Pressure"); } }
        public static By overrideflowlinepressure { get { return SeleniumActions.getByLocator("Xpath", "//*[contains(text(),'Flowline Pressure')]/following-sibling::*[2]//span[@class='k-switch-handle']", "Flowline Pressure"); } }
        public static By overridecasinggasrate { get { return SeleniumActions.getByLocator("Xpath", "//*[contains(text(),'Casing Gas Rate')]/following-sibling::*[2]//span[@class='k-switch-handle']", "Casing Gas Late"); } }
        public static By overridemotorfreq { get { return SeleniumActions.getByLocator("Xpath", "//*[contains(text(),'Motor Frequency')]/following-sibling::*[2]//span[@class='k-switch-handle']", "Motor Frequency"); } }
        public static By overridemotorvolts { get { return SeleniumActions.getByLocator("Xpath", "//*[contains(text(),'Motor Volts')]/following-sibling::*[2]//span[@class='k-switch-handle']", "Motor Volts"); } }
        public static By overridemotoramps { get { return SeleniumActions.getByLocator("Xpath", "//*[contains(text(),'Motor Amps')]/following-sibling::*[2]//span[@class='k-switch-handle']", "Motor Amps"); } }
        public static By overridemotortemp { get { return SeleniumActions.getByLocator("Xpath", "//*[contains(text(),'Motor Temperature')]/following-sibling::*[2]//span[@class='k-switch-handle']", "Motor Temperature"); } }
        public static By overridechokedia { get { return SeleniumActions.getByLocator("Xpath", "//*[contains(text(),'Choke Diameter')]/following-sibling::*[2]//span[@class='k-switch-handle']", "Choke dia"); } }
        public static By overrideruntime { get { return SeleniumActions.getByLocator("Xpath", "//*[contains(text(),'Runtime')]/following-sibling::*[2]//span[@class='k-switch-handle']", "Runtime"); } }
        public static By motortemp { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='olMin13']/span/input", "motortemp"); } }
        public string txtloptubingpressminm = "//*[@id='olMin0']/span/input";
        public string txtloptubingtempminm = "//*[@id='olMin1']/span/input";
        public string txtcasingpressure = "//*[@id='olMin2']/span/input";

        public string txtoilminm = "//*[@id='olMin3']/span/input";
        public string txtwaterminm = "//*[@id='olMin4']/span/input";
        public string txtgasminm = "//*[@id='olMin5']/span/input";
        public string txtpumpintkimnm = "//*[@id='olMin6']/span/input";
        public string txtpumpdiscminm = "//*[@id='olMin7']/span/input";
        public string txtflowinlnpr = "//*[@id='olMin8']/span/input";
        public string txtcasinggasrate = "//*[@id='olMin9']/span/input";
        public string txtmotorfreq = "//*[@id='olMin10']/span/input";
        public string txtmotorvolts = "//*[@id='olMin11']/span/input";
        public string txtmotoramps = "//*[@id='olMin12']/span/input";
        public string txtmotortemp = "//*[@id='olMin13']/span/input";
        public string txtchokedia = "//*[@id='olMin14']/span/input";
        public string txtruntime = "//*[@id='olMin15']/span/input";


        public static By btnoptsave { get { return SeleniumActions.getByLocator("Xpath", "//kendo-dialog-actions//*[contains(text(),'Save')]", "Save"); } }


        #endregion

        #region Performancecurves
        public static By lnkperformancecurves { get { return SeleniumActions.getByLocator("Xpath", "//li[8][contains(text(),'Performance Curves')]", "Performance curves"); } }
        public static By btnperfadd { get { return SeleniumActions.getByLocator("Xpath", "//button//*[contains(text(),'Add')]", "Add"); } }
        public static By txtfrequency { get { return SeleniumActions.getByLocator("Xpath", " //*[@class='k-content k-window-content k-dialog-content']//input", "Frequency input"); } }
        public static By btnperfsave { get { return SeleniumActions.getByLocator("Xpath", "//button[contains(text(),'Save')]", "Save"); } }
        public static By btnperfclose { get { return SeleniumActions.getByLocator("Xpath", "//*[@class='k-button k-bare k-button-icon k-window-action k-dialog-action k-dialog-close']", "Close"); } }

        public static By btnperfdelete { get { return SeleniumActions.getByLocator("Xpath", "//tbody//*[@class='k-icon k-i-trash']", "Delete"); } }

        #endregion

    }
}
