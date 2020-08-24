using OpenQA.Selenium;
using SeleniumAutomation.SeleniumObject;

namespace SeleniumAutomation.PageObjects
{
    class WellStatusPage
    {
        public static string strDynamicValue = string.Empty;

        public static By surveillanceTab { get { return SeleniumActions.getByLocator("Xpath", "//div[text()='Surveillance']", "Field Services Tab"); } }
        public static By wellStatusTab { get { return SeleniumActions.getByLocator("Xpath", "//div[text()='Well Status']", "Field Services Tab"); } }
        public static By downtimeTab { get { return SeleniumActions.getByLocator("Xpath", "//div[text()='Downtime']", "Downtime Tab"); } }
        public static By downtimeTabHistory { get { return SeleniumActions.getByLocator("Xpath", "//div[text()='Downtime History']", "Downtime Tab History"); } }
        public static By downtimeDrpdwn { get { return SeleniumActions.getByLocator("Xpath", "//span[contains(text(),'Downtime Code')]", "Downtime Dropdown"); } }
        public static By downtimeType { get { return SeleniumActions.getByLocator("Xpath", "//li[text()='" + TestData.JobDowntime.JobTypeD + "']", "Downtime code"); } }
        public static By btnWell { get { return SeleniumActions.getByLocator("Xpath", "//button[contains(text(),'Save')]", " Save Button"); } }
        public static By btnScan { get { return SeleniumActions.getByLocator("Xpath", "//button[@id='demandScan']", "Scan Button"); } }
        public static By btnClearErrors { get { return SeleniumActions.getByLocator("Xpath", "//button[@id='clearErrors']", "clearErrors Button"); } }
        public static By btnStart { get { return SeleniumActions.getByLocator("Xpath", "//button[@id='startRPC']", "STart Button"); } }
        public static By btnIdle { get { return SeleniumActions.getByLocator("Xpath", "//button[@id='Idle']", "Idle Button"); } }
        public static By btnStop { get { return SeleniumActions.getByLocator("Xpath", "//button[@id='stopAndLeaveDown']", "Stop Button"); } }
        public static By btnSoftwaretimer { get { return SeleniumActions.getByLocator("Xpath", "//button[@id='SW_TIMER']", "Softwaretimer Button"); } }
        public static By btnControlTrasnfer { get { return SeleniumActions.getByLocator("Xpath", "//button[@id='controlTransfer']", "Control Tasnfer Button"); } }
        public static By btnDowntime { get { return SeleniumActions.getByLocator("Xpath", "//button[@id='turnOnOffWell']", "Downtime Button"); } }
        public static By btnSetPoints { get { return SeleniumActions.getByLocator("Xpath", "//button[@id='setPoint']", "Set Points"); } }
        public static By btnBasicSetpointGetvalues { get { return SeleniumActions.getByLocator("Xpath", "//button[text()=' Get Values ']", "BasicSetpointGetvalues"); } }
        public static By btnCloseSetPointModal { get { return SeleniumActions.getByLocator("Xpath", " //a[@aria-label='Close']", "CloseSetPointModal"); } }
        public static By btnConfirmSend { get { return SeleniumActions.getByLocator("Xpath", "//button[text()='Yes, Send Command']", "Confirm Send Command"); } }


        public static By lblCommStatusBy { get { return SeleniumActions.getByLocator("Xpath", "//span[text()='Communication Status: ']/following-sibling::span[1]", "CommStaus Label"); } }
        public static By lblLastScanTime { get { return SeleniumActions.getByLocator("Xpath", "//span[text()='Last Scan Time: ']/following-sibling::span[1]", "Last Scan Time Label"); } }



    }
}
