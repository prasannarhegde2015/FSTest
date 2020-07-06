using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading;
using AventStack.ExtentReports;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Interactions.Internal;
using OpenQA.Selenium.Support.UI;
using Protractor;
using SeleniumAutomation.AGGridScreenDatas;
using SeleniumAutomation.Helper;
using ExpectedConditions = SeleniumExtras.WaitHelpers.ExpectedConditions;



namespace SeleniumAutomation.SeleniumObject
{
    static class SeleniumActions
    #region GenericSeleniumObjectMethods
    {
        //This Class will call all Selenium Core Actions:
        private static IWebDriver driver;
        private static NgWebDriver ngDriver;
        private static WebDriverWait wait;
        internal const string dtlValidLocators = "Valid locators are id ,name ,content and attribute";
        public static string elemdesc = string.Empty;
        public static string loglevel = string.Empty;
        public static int sectimeout = 0;

        public static void InitializeWebDriver()
        {

            string starturl = ConfigurationManager.AppSettings.Get("BrowserURL");
            string timeout = ConfigurationManager.AppSettings.Get("GlobalTimeout");
            string browser = ConfigurationManager.AppSettings.Get("Browser");

            loglevel = ConfigurationManager.AppSettings.Get("LogLevel");
            sectimeout = int.Parse(timeout);

            switch (browser.ToLower())
            {
                case "chrome":
                    {
                        driver = new ChromeDriver();
                        ICapabilities capabilities = ((OpenQA.Selenium.Remote.RemoteWebDriver)driver).Capabilities;
                        Console.WriteLine("==========================================");
                        Console.WriteLine("Browser Version=> " + capabilities.GetCapability("version"));
                        Console.WriteLine("==========================================");
                        ngDriver = new NgWebDriver(driver);
                        ngDriver.IgnoreSynchronization = true;
                        ngDriver.Navigate().GoToUrl(starturl);
                        ngDriver.Manage().Window.Maximize();
                        CommonHelper.TraceLine("Launched browser with url: " + starturl);
                        wait = new WebDriverWait(ngDriver, TimeSpan.FromSeconds(sectimeout));
                        waitforPageloadComplete();
                        break;
                    }
                case "ie":

                    {
                        var options = new InternetExplorerOptions();
                        options.IgnoreZoomLevel = true;
                        options.IntroduceInstabilityByIgnoringProtectedModeSettings = true;
                        driver = new InternetExplorerDriver(options);
                        ngDriver = new NgWebDriver(driver);
                        ngDriver.IgnoreSynchronization = true;
                        ngDriver.Navigate().GoToUrl(starturl);
                        ngDriver.Manage().Window.Maximize();
                        CommonHelper.TraceLine("Launched browser with url: " + starturl);
                        wait = new WebDriverWait(ngDriver, TimeSpan.FromSeconds(sectimeout));
                        break;
                    }
                case "edge":
                    {

                        driver = new EdgeDriver(@"C:\Driver");
                        ngDriver = new NgWebDriver(driver);
                        ngDriver.Navigate().GoToUrl(starturl);
                        ngDriver.Manage().Window.Maximize();
                        var AutoIT = new AutoItX3Lib.AutoItX3();
                        if (AutoIT.WinExists("Microsoft Edge") == 1)
                        {
                            AutoIT.WinWait("Microsoft Edge");
                            AutoIT.WinActivate("Microsoft Edge");
                            Thread.Sleep(2000);
                            AutoIT.Send("e159279");
                            AutoIT.Send("{TAB}");
                            Thread.Sleep(2000);
                            AutoIT.Send("Lowis2019JAN;");
                            Thread.Sleep(2000);
                            AutoIT.Send("{ENTER}");
                            Thread.Sleep(5000);
                        }


                        CommonHelper.TraceLine("Launched browser with url: " + starturl);
                        wait = new WebDriverWait(ngDriver, TimeSpan.FromSeconds(sectimeout));
                        break;
                    }
                default:
                    {
                        break;
                    }
            }


        }
        public static string BringResource(string resourcename)
        {
            ResourceManager rm = new ResourceManager("SeleniumAutomation.TestData.TestData", Assembly.GetExecutingAssembly());
            string value = rm.GetString(resourcename);
            return value;
        }

        public static string getBrowserTitle()
        {
            return ngDriver.Title;
        }

        public static void refreshBrowser()
        {
            ngDriver.Navigate().Refresh();
        }

        public static void waitforPageloadComplete()
        {
            wait.Until(driver => ((string)((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState") == "complete"));
        }
        public static void waitClick(IWebElement elem)
        {
            wait.Until(ExpectedConditions.ElementToBeClickable(elem));
            //   elem.Click();
            Actions actions = new Actions(driver);
            actions.MoveToElement(elem).Click().Perform();
            CommonHelper.TraceLine("Clicked Element " + elemdesc);
        }

        public static void waitClick(By elem)
        {
            wait.Until(ExpectedConditions.ElementIsVisible(elem));
            wait.Until(ExpectedConditions.ElementToBeClickable(elem));
            IWebElement el = driver.FindElement(elem);
            Actions actions = new Actions(driver);
            actions.MoveToElement(el).Click().Perform();
            CommonHelper.TraceLine("Clicked Element " + elemdesc);
        }

        public static bool mousehover(By elem)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(ngDriver, TimeSpan.FromSeconds(3));
                wait.Until(ExpectedConditions.ElementIsVisible(elem));
                wait.Until(ExpectedConditions.ElementToBeClickable(elem));
                IWebElement el = driver.FindElement(elem);
                Actions actions = new Actions(driver);
                actions.MoveToElement(el).Perform();
                CommonHelper.TraceLine("Mouse hover on Element " + elemdesc); return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static void WaitforWellcountNonZero()
        {

            string wellcount = getInnerText(PageObjects.DashboardPage.lblWelCount);

            while (wellcount.Equals("0"))
            {
                Thread.Sleep(1000);
                wellcount = getInnerText(PageObjects.DashboardPage.lblWelCount);
                CommonHelper.TraceLine("Well count :" + wellcount);
            }
        }

        public static void WaitforWellcountToBeNonZero()
        {

            string wellcount = getInnerText(PageObjects.DashboardPage.lblWelCount);
            int count = 0;
            while (wellcount.Equals("0"))
            {
                Thread.Sleep(1000);
                wellcount = getInnerText(PageObjects.DashboardPage.lblWelCount);
                CommonHelper.TraceLine("Well count :" + wellcount);
                SeleniumActions.refreshBrowser();
                SeleniumActions.WaitForLoad();
                count++;
                if (count > sectimeout)
                {
                    CommonHelper.TraceLine("Well did not appear within specified timeout");
                    break;
                }
            }
        }
        public static void waitClickNG(IWebElement elem)
        {
            wait.Until(ExpectedConditions.ElementToBeClickable(elem));
            //   elem.Click();
            Actions actions = new Actions(driver);
            actions.MoveToElement(elem).ClickAndHold().Perform();
            CommonHelper.TraceLine("Clicked Element " + elemdesc);
        }
        public static void waittoSelect(By elem)
        {
            wait.Until(ExpectedConditions.ElementSelectionStateToBe(elem, true));
            CommonHelper.TraceLine("Waited for Element to be Selected: " + elemdesc);
        }

        public static void waitClickJS(By elem)
        {
            wait.Until(ExpectedConditions.ElementToBeClickable(elem));
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", driver.FindElement(elem));
            CommonHelper.TraceLine("Clicked Element using JS:" + elemdesc);
        }
        public static void waitClickDelay(IWebElement elem, int sec)
        {
            wait.Until(ExpectedConditions.ElementToBeClickable(elem));
            //   elem.Click();
            Actions actions = new Actions(driver);
            Thread.Sleep(sec * 1000);
            actions.MoveToElement(elem).Click().Perform();
            CommonHelper.TraceLine("Clicked Element " + elemdesc);
        }
        public static void waitClickDelay(By elem, int sec)
        {
            wait.Until(ExpectedConditions.ElementToBeClickable(elem));
            //   elem.Click();
            Actions actions = new Actions(driver);
            Thread.Sleep(sec * 1000);
            actions.MoveToElement(driver.FindElement(elem)).Click().Perform();
            CommonHelper.TraceLine("Clicked Element " + elemdesc);
        }

        public static bool waitforDispalyed(By elem)

        {
            wait.Until(ExpectedConditions.ElementToBeClickable(elem));
            return driver.FindElement(elem).Displayed;
        }

        public static void waitFirElemToInvisible(By elem)
        {
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(elem));


        }

        public static void switchToFrame(By elem)
        {

            wait.Until(ExpectedConditions.FrameToBeAvailableAndSwitchToIt(elem));
            CommonHelper.TraceLine("Switched to Frame " + elemdesc);
        }

        public static void switchToDefaultFrame()
        {

            driver.SwitchTo().DefaultContent();
            CommonHelper.TraceLine("Switched to default Content");
        }

        public static bool IsElementDisplayedOnUI(By elem)
        {
            return driver.FindElement(elem).Displayed;
        }
        public static bool elementselected(By elem)
        {
            if (driver.FindElement(elem).Selected)
            {
                return true;

            }
            else
                return false;
        }
        public static void sendText(By elem, string text, bool special = false)
        {
            wait.Until(ExpectedConditions.ElementToBeClickable(elem));
            driver.FindElement(elem).Click();
            driver.FindElement(elem).Clear();
            sendspecialkey(elem, "home");
            if (special)
            {
                sendspecialkey(elem, "selectall");
                sendspecialkey(elem, "delete");
                text = text.Replace("/", "");
                //sendspecialkey(elem, "home");
            }
            driver.FindElement(elem).SendKeys(text);
            CommonHelper.TraceLine(string.Format("Entered Text {0} on Element: {1}", text, elemdesc));
        }
        public static void send_Text(By elem, string text)
        {

            driver.FindElement(elem).SendKeys(text);
            CommonHelper.TraceLine(string.Format("Entered Text {0} on Element: {1}", text, elemdesc));
        }
        public static void KendoTypeNSelect(By elem, string text, bool special = false)
        {
            wait.Until(ExpectedConditions.ElementToBeClickable(elem));
            driver.FindElement(elem).Click();
            driver.FindElement(elem).Clear();
            driver.FindElement(elem).SendKeys(text);
            CommonHelper.TraceLine(string.Format("Entered Text {0} on Element: {1}", text, elemdesc));
            WaitForLoad();
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//li[text()='" + text + "']")));
            IWebElement kitem = getElement("Xpath", "//li[text()='" + text + "']", text + "Item ");
            wait.Until(ExpectedConditions.ElementToBeClickable(kitem));
            //    wait.Until(ExpectedConditions.TextToBePresentInElement(kitem, text));
            kitem.Click();
            CommonHelper.TraceLine(string.Format("Selected Text {0} on dropdown: {1}", text, elemdesc));


        }

        public static void ScrollpageHorizontal()
        {
            //IJavascriptExecutor js = (JavascriptExecutor)driver;

            IWebElement Element = driver.FindElement(By.XPath("//div[@col-id='Well.WellDepthDatumId']"));

            //This will scroll the page Horizontally till the element is found		
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView();", Element);
        }



        public static void Scrollpage(int pixel, bool scrollVertical = true)
        {
            if (scrollVertical)
            {
                ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollBy(0," + pixel + ")");
            }
            else
            {
                ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollBy(" + pixel + ",0)");
            }
        }

        public static void scrollscrollbar(int x, int y, By elem)
        {
            IWebElement el = driver.FindElement(elem);
            Actions actions = new Actions(driver);
            actions.MoveToElement(el).ClickAndHold().MoveByOffset(x, y).Release().Build().Perform();

        }
        public static void FileUploadDialog(string filename)
        {
            var AutoIT = new AutoItX3Lib.AutoItX3();
            Thread.Sleep(2000);
            if (AutoIT.WinExists("Open") == 1)
            {
                AutoIT.WinWait("Open");
                AutoIT.WinActivate("Open");
                Thread.Sleep(1000);
                // AutoIT.Send(filename);
                AutoIT.ControlSetText("Open", "", "Edit1", filename);
                string rettxt = AutoIT.ControlGetText("Open", "", "Edit1");
                Assert.AreEqual(filename, rettxt, "AutoIT unable to enter Text in File Dialog text");
                AutoIT.Send("{Enter}");

            }
        }


        public static System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> CollectionOfControls(string searchBy, string searchValue, string desc)
        {
            //public static List<HtmlControl> documentManager_Categories { get { return TelerikObject.CollectionOfControls("htmldiv", "classname", "folderNames", "Document Manager Categories"); } }


            System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> elems = null;
            elemdesc = desc;
            CommonHelper.TraceLine(string.Format("For Control Control using searchby as {0} and searchvalue as {1}", searchBy, searchValue));
            #region SearchCrieria
            try
            {

                switch (searchBy.ToLower())
                {
                    case "id":
                        {
                            CommonHelper.TraceLine("Looking for Element: " + desc);
                            elems = driver.FindElements(By.Id(searchValue));
                            CommonHelper.TraceLine("Found Element: " + desc);
                            break;
                        }

                    case "name":
                        {
                            elems = driver.FindElements(By.Name(searchValue));
                            break;
                        }
                    case "xpath":
                        {
                            CommonHelper.TraceLine("Looking for Element: " + desc);
                            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(searchValue)));
                            elems = driver.FindElements(By.XPath(searchValue));
                            CommonHelper.TraceLine("Found Element: " + desc);
                            break;
                        }
                    case "tagname":
                        {
                            elems = driver.FindElements(By.TagName(searchValue));
                            break;
                        }
                    default:
                        {
                            CommonHelper.TraceLine("Not Valid Locator: " + dtlValidLocators);
                            break;
                        }


                }
            }
            catch (Exception e)
            {

                CommonHelper.TraceLine(string.Format("Unable to Find HTMLinputContril Type using {0},{1}", searchBy, searchValue));
                throw e;
            }

            return elems;
            #endregion

        }



        public static string getText(IWebElement elem)
        {
            wait.Until(ExpectedConditions.ElementToBeClickable(elem));
            return elem.GetAttribute("value");

        }

        public static string getText(By elem, bool disabled = false)
        {
            if (disabled == false)
            {
                wait.Until(ExpectedConditions.ElementToBeClickable(elem));
            }
            return driver.FindElement(elem).GetAttribute("value");

        }

        public static string getInnerText(IWebElement elem)
        {
            wait.Until(ExpectedConditions.ElementToBeClickable(elem));
            return elem.Text;

        }

        public static string getInnerText(By elem)
        {
            waitForElement(elem);
            return driver.FindElement(elem).Text;

        }
        public static bool getEnabledState(By elem)
        {
            waitForElement(elem);
            return driver.FindElement(elem).Enabled;

        }

        public static void selectDropdownValue(By elem, string ddvalue)
        {
            wait.Until(ExpectedConditions.ElementToBeClickable(elem));
            SelectElement sel = new SelectElement(driver.FindElement(elem));
            sel.SelectByText(ddvalue);
            CommonHelper.TraceLine(string.Format("Selected Text {0} on dropdown: {1}", ddvalue, elemdesc));


        }

        public static void selectKendoDropdownValue(By elem, string ddvalue, bool ScrolltoView = false)
        {

            wait.Until(ExpectedConditions.ElementToBeClickable(elem));
            // driver.FindElement(elem).Click();  ATS intermittent failures on click repalced with Action Class Click
            waitClick(elem);
            WaitForLoad();
            try
            {
                wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//li[text()='" + ddvalue + "']")));
            }
            catch
            {
                CommonHelper.TraceLine($"List Item timed out What Next ?");
                // Dont Throw but Proceeed/
            }
            IWebElement kitem = null;
            try
            {
                kitem = getElement("Xpath", "//li[text()='" + ddvalue + "']", elemdesc);
            }
            catch
            {
                // Dont Throw but Proceeed/
            }
            for (int i = 0; i < 5; i++) //max 5 attempts only; Handle ATS UPG VM Failure 
            {
                if (kitem != null)
                {
                    break;
                }
                WaitForLoad();
                CommonHelper.TraceLine($"Seems like first click did not workor DOM did not render it propelry Try Re clcik attemtp {i + 1}");
                waitClick(elem);//Previous action was Probblably not done
                try
                {
                    kitem = getElement("Xpath", "//li[text()='" + ddvalue + "']", elemdesc);
                }
                catch
                {
                    // Dont get it still ?
                }
            }
            if (kitem == null)
            {
                CommonHelper.TraceLine($"Did not get Element loaded on whatever I did ");
                throw new NoSuchElementException("Requied List Item not found despite 5 Retries");
            }
            wait.Until(ExpectedConditions.ElementToBeClickable(kitem));
            wait.Until(ExpectedConditions.TextToBePresentInElement(kitem, ddvalue));
            if (ScrolltoView)
            {
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView();", driver.FindElement(By.XPath("//li[text()='" + ddvalue + "']")));
                //  ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView();", driver.FindElement(elem));
                CommonHelper.TraceLine(string.Format("Scrolled for Text {0} on dropdown: {1}", ddvalue, elemdesc));

            }
            //  kitem.Click();
            waitClick(kitem);
            CommonHelper.TraceLine(string.Format("Selected Text {0} on dropdown: {1}", ddvalue, elemdesc));


        }
        public static void selectKendoDropdownValuelist(By elem, string ddvalue)
        {

            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView();", driver.FindElement(elem));
            CommonHelper.TraceLine(string.Format("Scrolled for Text {0} on dropdown: {1}", ddvalue, elemdesc));
            waitClick(elem);

        }

        public static void WaitForLoad()
        {
            bool check = true;
            IWebElement loader = null;
            try
            {
                loader = driver.FindElement(By.XPath("//div[@class='loader']"));
            }
            catch (NoSuchElementException e)
            {
                CommonHelper.TraceLine("Elem not found immeditaltey: " + e.InnerException);
            }
            catch (StaleElementReferenceException ste)
            {
                CommonHelper.TraceLine("Stalte elem was found Handled !! " + ste.InnerException);
                Thread.Sleep(1000);
                loader = driver.FindElement(By.XPath("//div[@class='loader']"));
            }
            if (check)
            {
                for (int i = 0; i < sectimeout; i++)
                {
                    Thread.Sleep(1000);

                    try
                    {
                        bool chk = loader.Displayed;
                    }
                    catch (StaleElementReferenceException ste)
                    {
                        CommonHelper.TraceLine("Stale elem was found Handled !! " + ste.InnerException);
                        Thread.Sleep(2000);
                        loader = driver.FindElement(By.XPath("//div[@class='loader']"));
                    }
                    catch (Exception exany)
                    {
                        CommonHelper.TraceLine("Other Expcetion while waiting for Loader !!!: " + exany.InnerException);
                    }
                    if (loader != null)
                    {
                        if (loader.Size == System.Drawing.Size.Empty)
                        {
                            CommonHelper.TraceLine("Loader Size Empty");
                            break;
                        }
                        else
                        {
                            CommonHelper.TraceLine("Loader Size Not Empty");
                        }
                    }

                }

            }


        }

        public static string getDropdowntext(IWebElement elem)
        {
            wait.Until(ExpectedConditions.ElementToBeClickable(elem));
            SelectElement sel = new SelectElement(elem);
            return sel.SelectedOption.Text;
        }
        public static string getKendoDDSelectedText(By elem)
        {
            string txt = driver.FindElement(elem).Text;
            string[] charactersToReplace = new string[] { Environment.NewLine, @"\t", @"\r\n", @"\n", @"\r" };
            foreach (string s in charactersToReplace)
            {
                txt = txt.Replace(s, "");
            }
            return txt;
        }

        public static void takeScreenshot(string desc)
        {

            Screenshot ss = ((ITakesScreenshot)driver).GetScreenshot();
            string screenshot = ss.AsBase64EncodedString;
            byte[] screenshotAsByteArray = ss.AsByteArray;
            string stamp = System.DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss");
            stamp = stamp.Replace("-", "");
            stamp = stamp.Replace(":", "");
            stamp = stamp.Replace(" ", "");
            string strtimestamp = desc + ".png";
            string imgpath = Path.Combine(ExtentFactory.screenshotdir, strtimestamp);
            ss.SaveAsFile(imgpath, ScreenshotImageFormat.Jpeg);

        }

        public static void waitForElement(By elem)
        {

            wait.Until(ExpectedConditions.ElementExists(elem));
            if (driver.FindElement(elem).Enabled)
            {
                performStalenessCheck(elem);
                wait.Until(ExpectedConditions.ElementToBeClickable(elem));
            }
            CommonHelper.TraceLine("Dynamicallay waited for Element " + elemdesc);
        }
        public static void staticwaitForElement(int sec)
        {
            Thread.Sleep(sec * 1000);
            CommonHelper.TraceLine("Dynamicallay waited for Element " + elemdesc);
        }
        public static bool isElemPresent(By elem)
        {
            try
            {
                wait.Until(ExpectedConditions.ElementIsVisible(elem));
                ngDriver.FindElement(elem);
                return true;
            }
            catch (NoSuchElementException noel)
            {
                CommonHelper.TraceLine("Caught No Such Eleemnt Exception: ");
                if (loglevel.Equals("2"))
                {
                    CommonHelper.TraceLine("error " + noel);
                }
                return false;
            }
            catch (StaleElementReferenceException stl)
            {
                CommonHelper.TraceLine("Caught Stale Elemnt Ref Exception: ");
                if (loglevel.Equals("2"))
                {
                    CommonHelper.TraceLine("error " + stl);
                    Thread.Sleep(1000);
                }
                return false;
            }
            catch (Exception otherex)
            {

                if (loglevel.Equals("2"))
                {
                    CommonHelper.TraceLine("error " + otherex);
                }
                return false;
            }

        }

        public static bool isStale(By elem)
        {
            try
            {
                ngDriver.FindElement(elem);
                return false;
            }
            catch (StaleElementReferenceException stl)
            {
                CommonHelper.TraceLine("Caught Stale Elemnt Ref Exception: " + stl);
                Thread.Sleep(1000);
                return true;
            }



        }

        public static String getmonthname()
        {
            String dt = DateTime.Now.ToString("MMMM").Substring(0, 3);

            return dt;
        }
        public static String getday(int d)
        {
            String dt = DateTime.Now.AddDays(d).ToString("dd");

            return dt;
        }

        public static void performStalenessCheck(By elem)
        {
            for (int i = 0; i < 5; i++)
            {
                CommonHelper.TraceLine("Performing Staleness check");
                staticwaitForElement(1);
                if (isStale(elem) == false)
                {
                    break;
                }
            }
        }

        public static bool isStale(IWebElement elem)
        {
            try
            {
                bool chck = elem.Displayed;
                CommonHelper.TraceLine("Stale check Elem disaplyed ");
                return false;
            }
            catch (StaleElementReferenceException stl)
            {
                CommonHelper.TraceLine("Caught Stale Elemnt Ref Exception: " + stl);
                Thread.Sleep(1000);
                return true;
            }



        }

        public static IWebElement getElement(string searchBy, string searchValue, string desc)
        {
            IWebElement ctl = null;
            elemdesc = desc;
            CommonHelper.TraceLine(string.Format("For Control Control using searchby as {0} and searchvalue as {1}", searchBy, searchValue));
            #region SearchCrieria
            try
            {

                switch (searchBy.ToLower())
                {
                    case "id":
                        {
                            CommonHelper.TraceLine("Looking for Element: " + desc);
                            ctl = ngDriver.FindElement(By.Id(searchValue));
                            CommonHelper.TraceLine("Found Element: " + desc);
                            break;
                        }

                    case "name":
                        {
                            ctl = ngDriver.FindElement(By.Name(searchValue));
                            break;
                        }
                    case "xpath":
                        {
                            CommonHelper.TraceLine("Looking for Element: " + desc);
                            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(searchValue)));
                            ctl = ngDriver.FindElement(By.XPath(searchValue));
                            CommonHelper.TraceLine("Found Element: " + desc);
                            break;
                        }
                    case "tagname":
                        {
                            ctl = ngDriver.FindElement(By.TagName(searchValue));
                            break;
                        }
                    default:
                        {
                            CommonHelper.TraceLine("Not Valid Locator: " + dtlValidLocators);
                            break;
                        }


                }
            }
            catch (Exception e)
            {

                CommonHelper.TraceLine(string.Format("Unable to Find HTMLinputContril Type using {0},{1}", searchBy, searchValue));
                throw e;
            }
            #endregion
            return ctl;
        }

        public static By getElementByIndexXpath(string searchBy, string searchValue, int index, string desc)
        {
            By ctl = null;
            elemdesc = desc;

            #region SearchCrieria
            try
            {

                switch (searchBy.ToLower())
                {

                    case "xpath":
                        {
                            CommonHelper.TraceLine("Looking for Element: " + desc);
                            string indexedxpath = "(" + searchValue + ")[" + index + "]";
                            CommonHelper.TraceLine(string.Format("For Control Control using searchby as {0} and searchvalue as {1}", searchBy, indexedxpath));
                            try
                            {
                                wait.Until(ExpectedConditions.ElementExists(By.XPath(indexedxpath)));
                            }
                            catch
                            {

                            }
                            ctl = By.XPath(indexedxpath);
                            CommonHelper.TraceLine("Found Element: " + desc);
                            break;
                        }

                    default:
                        {
                            CommonHelper.TraceLine("Not Valid Locator: " + dtlValidLocators);
                            break;
                        }


                }
            }
            catch (Exception e)
            {

                CommonHelper.TraceLine(string.Format("Unable to Find HTMLinputContril Type using {0},{1}", searchBy, searchValue));
                throw e;
            }
            #endregion
            return ctl;
        }

        public static By getElementByIndexXpathNextSibling(string searchBy, string searchValue, int index, string desc)
        {
            By ctl = null;
            elemdesc = desc;

            #region SearchCrieria
            try
            {

                switch (searchBy.ToLower())
                {

                    case "xpath":
                        {
                            CommonHelper.TraceLine("Looking for Element: " + desc);
                            string indexedxpath = "(" + searchValue + ")[" + index + "]";
                            indexedxpath = indexedxpath + "/following-sibling::*[1]";
                            CommonHelper.TraceLine(string.Format("For Control Control using searchby as {0} and searchvalue as {1}", searchBy, indexedxpath));
                            try
                            {
                                wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(indexedxpath)));
                            }
                            catch
                            {

                            }
                            ctl = By.XPath(indexedxpath);
                            CommonHelper.TraceLine("Found Element: " + desc);
                            break;
                        }

                    default:
                        {
                            CommonHelper.TraceLine("Not Valid Locator: " + dtlValidLocators);
                            break;
                        }


                }
            }
            catch (Exception e)
            {

                CommonHelper.TraceLine(string.Format("Unable to Find HTMLinputContril Type using {0},{1}", searchBy, searchValue));
                throw e;
            }
            #endregion
            return ctl;
        }

        public static void sendspecialkey(By elem, string data)
        {
            switch (data)
            {

                case "selectall":
                    driver.FindElement(elem).SendKeys(Keys.Control + "a");
                    break;
                case "delete":
                    driver.FindElement(elem).SendKeys(Keys.Delete);
                    break;
                case "tab":
                    driver.FindElement(elem).SendKeys(Keys.Tab);
                    break;
                case "home":
                    driver.FindElement(elem).SendKeys(Keys.Home);
                    break;

                default:
                    CommonHelper.TraceLine("Key values could not be found under sendspecialkey method");
                    break;
            }
        }

        public static void CustomAssertEqual(string exp, string act, string failMessage, string fieldname, ExtentTest test)
        {
            CommonHelper.TraceLine(string.Format("Got Actual value for {0} as {1}", fieldname, act));
            double val;
            if (double.TryParse(exp, out val))
            {
                Assert.AreEqual(val, Convert.ToDouble(act), 0.1, failMessage);
            }
            else
            {
                Assert.AreEqual(exp, act, failMessage);
            }
            test.Pass(string.Format("FieldName : {0} Expected Value : {1} and Actual value : {2} ", fieldname, exp, act));

        }

        public static By getByLocator(string searchBy, string searchValue, string desc)
        {
            By ctl = null;
            elemdesc = desc;
            if (loglevel.Equals("2"))
            {
                CommonHelper.TraceLine(string.Format("For Control Control using searchby as {0} and searchvalue as {1}", searchBy, searchValue));
            }
            #region SearchCrieria
            try
            {

                switch (searchBy.ToLower())
                {
                    case "id":
                        {
                            CommonHelper.TraceLine("Looking for Element: " + desc);
                            ctl = By.Id(searchValue);
                            break;
                        }

                    case "name":
                        {
                            ctl = By.Name(searchValue);
                            break;
                        }
                    case "xpath":
                        {
                            CommonHelper.TraceLine("Looking for Element: By Xpath : for " + desc);
                            ctl = By.XPath(searchValue);
                            break;
                        }
                    case "tagname":
                        {
                            ctl = By.Id(searchValue);
                            break;
                        }
                    case "linktext":
                        {
                            ctl = By.LinkText(searchValue);
                            break;
                        }
                    default:
                        {
                            CommonHelper.TraceLine("Not Valid Locator: " + dtlValidLocators);
                            break;
                        }


                }
            }
            catch (Exception e)
            {

                CommonHelper.TraceLine(string.Format("Unable to Find HTMLinputContril Type using {0},{1}", searchBy, searchValue));
                throw e;
            }
            #endregion
            return ctl;
        }

        public static By getByLocatorNextSibling(string searchBy, string searchValue, string desc)
        {
            By ctl = null;
            elemdesc = desc;
            if (loglevel.Equals("2"))
            {
                CommonHelper.TraceLine(string.Format("For Control Control using searchby as {0} and searchvalue as {1}", searchBy, searchValue));
            }
            #region SearchCrieria
            try
            {

                switch (searchBy.ToLower())
                {

                    case "xpath":
                        {
                            CommonHelper.TraceLine("Looking for Element: By Xpath : for " + desc);
                            string nextsibling = searchValue + "/following-sibling::*[1]";
                            ctl = By.XPath(nextsibling);
                            break;
                        }

                    default:
                        {
                            CommonHelper.TraceLine("Not Valid Locator: " + dtlValidLocators);
                            break;
                        }


                }
            }
            catch (Exception e)
            {

                CommonHelper.TraceLine(string.Format("Unable to Find HTMLinputContril Type using {0},{1}", searchBy, searchValue));
                throw e;
            }
            #endregion
            return ctl;
        }


        public static void disposeDriver()
        {
            //driver.Close();
            ngDriver.Quit();
            driver.Quit();

        }

        public static void Toastercheck(string scenario, string exptext, ExtentTest testm)
        {
            string toasttext = SeleniumActions.getInnerText(PageObjects.WellConfigurationPage.Toaseter);
            CommonHelper.TraceLine(toasttext);
            SeleniumActions.takeScreenshot(scenario);
            Assert.AreEqual(exptext, toasttext, scenario + " Toast did not appear");
            testm.Pass("Scenario: " + scenario, MediaEntityBuilder.CreateScreenCaptureFromPath((Path.Combine("Screenshots", scenario + ".png"))).Build());
        }


        public static void selectWellfromWellList(string wellname)
        {
            if (SeleniumActions.IsElementDisplayedOnUI(PageObjects.WellConfigurationPage.lblWellListHeader) == false)
            {//If already Well list is not open then only select click Well count lable to show up Well List
                SeleniumActions.waitClick(PageObjects.DashboardPage.lblWelCount);
                SeleniumActions.WaitForLoad();
                PageObjects.DashboardPage.Dynatext = wellname;
                SeleniumActions.waitClick(PageObjects.DashboardPage.lstWellName);
                SeleniumActions.WaitForLoad();
            }
        }
        public static string getAttribute(By elem, string attributename)
        {
            wait.Until(ExpectedConditions.ElementToBeClickable(elem));
            return driver.FindElement(elem).GetAttribute(attributename);
        }
        public static string getAttributeJS(By elem, string attributename)
        {
            try
            {
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                //return js.ExecuteScript("document.getElementByXpath('//div[@ref='eLabel']//span[text()='Job Id']').getAttribute('class')").ToString();
                return (string)js.ExecuteScript("return arguments[0].getAttribute('" + attributename + "')", driver.FindElement(elem));
            }

            catch (Exception)
            {
                return null;
            }

        }
        //Getattribute if element is not clickable
        public static string get_Attribute(By elem, string attributename)
        {
            if (attributename.Equals("value"))
            {

                return driver.FindElement(elem).GetAttribute("value");
            }
            else if (attributename.Equals("Text"))
            {
                return driver.FindElement(elem).Text;
            }
            else if (attributename.Equals("InnerText"))
            {
                return driver.FindElement(elem).GetAttribute("InnerText");
            }
            else return null;
        }
        public static void sendkeystroke(By elem, string key)
        {
            switch (key.ToLower())
            {
                case "tab":
                    wait.Until(ExpectedConditions.ElementToBeClickable(elem));
                    driver.FindElement(elem).SendKeys(Keys.Tab);
                    // new Actions(driver).SendKeys(Keys.Tab);
                    break;
                case "left":
                    wait.Until(ExpectedConditions.ElementToBeClickable(elem));
                    driver.FindElement(elem).SendKeys(Keys.ArrowLeft);
                    // new Actions(driver).SendKeys(Keys.Tab);
                    break;
                default:
                    CommonHelper.TraceLine("Invalid key requested");
                    break;

            }
        }
        public static void sendkeystroke(string key)
        {
            Actions actions = new Actions(driver);
            switch (key.ToLower().ToString())
            {
                case "end":
                    actions.SendKeys(OpenQA.Selenium.Keys.End).Build().Perform();
                    break;
                case "pagedown":
                    actions.SendKeys(OpenQA.Selenium.Keys.PageDown).Build().Perform();
                    break;
                case "tab":
                    actions.SendKeys(OpenQA.Selenium.Keys.Tab).Build().Perform();
                    break;
                case "enter":
                    actions.SendKeys(OpenQA.Selenium.Keys.Enter).Build().Perform();
                    break;
                case "left":
                    actions.SendKeys(OpenQA.Selenium.Keys.ArrowLeft).Build().Perform();
                    break;
                default:
                    CommonHelper.TraceLine("Invalid keystroke requested in the arguement");
                    break;
            }
        }
        public static int Gettotalrecords(string searchby, string elem)
        {
            int k = 0;
            if (searchby.Equals("xpath"))
            {
                k = driver.FindElements(By.XPath(elem + "/tr")).Count();
            }
            return k;

        }
        public static int Gettotalrecords(By elem)
        {
            int k = 0;

            k = driver.FindElements(elem).Count();

            return k;

        }
        public static IList<IWebElement> Gettotalrecordsinlist(string searchby, string elem)
        {
            try

            {
                // WebDriverWait wait = new WebDriverWait(ngDriver, TimeSpan.FromSeconds(5));
                wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath(elem)));
                // Thread.Sleep(4000);
                IList<IWebElement> elems;
                if (searchby.ToLower().Trim().Equals("xpath"))
                {
                    elems = driver.FindElements(By.XPath(elem));
                }
                else
                {
                    elems = null;
                }
                return elems;
            }
            catch (Exception)
            {
                return null;
            }

        }

        public static List<IWebElement> Gettotalrecordsinlist_type2(string searchby, string elem)
        {

            List<IWebElement> elems;

            if (searchby.ToLower().Trim().Equals("xpath"))
            {
                elems = driver.FindElements(By.XPath(elem)).ToList();
                var nullvalue = elems.FirstOrDefault();


                if (nullvalue.Text.Equals(""))
                {
                    elems.Remove(nullvalue);
                }
            }
            else
            {
                elems = null;
            }
            return elems;

        }

        public static List<string> Gettotalrecordsinlistelemnevisisble(string searchby, string elem)
        {
            try

            {
                List<string> textsinelement = new List<string>();
                WebDriverWait wait = new WebDriverWait(ngDriver, TimeSpan.FromSeconds(5));
                wait.Until(ExpectedConditions.ElementExists(By.XPath(elem)));
                // Thread.Sleep(4000);
                IList<IWebElement> elems;
                if (searchby.ToLower().Trim().Equals("xpath"))
                {
                    elems = driver.FindElements(By.XPath(elem));
                    foreach (var el in elems)
                    {
                        textsinelement.Add(el.Text);
                    }
                }
                else
                {
                    elems = null;
                }
                return textsinelement;
            }
            catch (Exception)
            {
                CommonHelper.TraceLine($"The Element defintion for {elem}  was not found: Probably the Column header definition was updated ");
                return null;
            }
        }
        public static string RandomString(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        #endregion

        #region GridEntryVerificationmethods
        public static void verifyGridCellValues(string colnames, string colvals, ExtentTest test, int rowindex = 0, string tblid = "none", bool clickrow = false)
        {
            try
            {
                string[] colnamearray = colnames.Split(new char[] { ';' });
                string[] colvalarray = colvals.Split(new char[] { ';' });
                string[] fixcolnameval = tblid.Split(new char[] { ';' });
                int colpos = -1;
                IWebElement colNameTable = null;
                //get required column header table group
                if (tblid.Equals("none"))
                {
                    colNameTable = getTableByColumnName(colnamearray[0]);
                }
                else
                {
                    colNameTable = getTableByColumnName(fixcolnameval[0]);
                }
                IWebElement colValTable = null;
                for (int i = 0; i < colvalarray.Length; i++)
                {
                    if (colvalarray[i].ToLower() == "ignorevalue")
                    {
                        continue;
                    }
                    if (tblid.Equals("none"))
                    {
                        colValTable = getTableByColumnName(colvalarray[i]);
                    }
                    else
                    {
                        colValTable = getTableByColumnName(fixcolnameval[1]);
                    }
                    if (colValTable != null)
                    {
                        break;
                    }
                }
                // get all column indexes for passed column names
                StringBuilder indexstring = new StringBuilder();
                foreach (string colname in colnamearray)
                {
                    indexstring.Append(getIndexofColumnName(colNameTable, colname) + ";");
                }
                string strcolHeaderIndex = indexstring.ToString();
                strcolHeaderIndex = getStringLastCharRemoved(strcolHeaderIndex, ';');
                string[] colHeaderIndex = strcolHeaderIndex.Split(new char[] { ';' });
                //get value table using exp val text
                foreach (string indx in colHeaderIndex)
                {
                    int.TryParse(indx, out colpos);

                    if (colvalarray[colpos].ToLower() != "ignorevalue")
                    {
                        string colnametoget = "";
                        if (colnamearray.Length != colvalarray.Length)
                        {
                            colnametoget = colnamearray[colpos - 1];
                            //Determine if something was extra for example Table one has one column , table 2 has 2 columns
                        }
                        else
                        {
                            colnametoget = colnamearray[colpos];
                        }
                        Helper.CommonHelper.TraceLine("********Table Column Name to be verified:******** " + colnametoget);
                        string actualcellvalue = getTextAtIndex(colValTable, colpos, rowindex, clickrow);
                        Helper.CommonHelper.TraceLine(string.Format("******Table Cell Value Expected: {0} and Actual value {1} ,at Row Position: {2} ***********", colvalarray[colpos], actualcellvalue, rowindex));
                        //Check for Blank String values 
                        if (actualcellvalue.Trim().Length == 0)
                        {
                            if (colvalarray[colpos].Trim().Length == 0)
                            {
                                Assert.AreEqual(colvalarray[colpos], actualcellvalue, "Table Cell Value did not match for " + colnametoget);
                            }
                            else
                            {
                                Assert.IsFalse(actualcellvalue.Trim().Length > 0, "Actaul value of Cell is Blank when Expected Value is Not Blank");
                            }
                        }
                        double dout = 0.0;
                        bool IsNumber = double.TryParse(actualcellvalue, out dout);
                        if (IsNumber)
                        {
                            double expresult = Convert.ToDouble(colvalarray[colpos]);
                            //this 'precision' can be configurable ??
                            double precision = 0.001;
                            int actuuldecimalprecision = CountDigitsAfterDecimal(dout);
                            expresult = Math.Round(expresult, actuuldecimalprecision);


                            if (IsDecimalTruncate(expresult, dout))
                            {
                                Helper.CommonHelper.TraceLine("Truncated Actual Value");
                                //Make them same as this is due to wrong deimal Truncation instead of rounding
                                expresult = Math.Truncate(dout * Math.Pow(10, actuuldecimalprecision)) / Math.Pow(10, actuuldecimalprecision);
                            }

                            Assert.AreEqual(expresult, dout, precision, " Precision Check Falied for Numeric Value " + colnametoget);
                        }
                        else
                        {
                            if (actualcellvalue != null)
                            {
                                if (actualcellvalue == "null")
                                {
                                    actualcellvalue = "";
                                }
                                Assert.AreEqual(colvalarray[colpos].Trim(), actualcellvalue.Trim(), "Table Cell Value did not match for " + colnametoget);

                            }
                            else
                            {
                                actualcellvalue = "Blank";
                                colvalarray[colpos] = "Blank";
                                Assert.AreEqual(colvalarray[colpos], actualcellvalue, "Table Cell Value did not match for " + colnametoget);
                            }
                        }
                    }
                    else
                    {
                        Helper.CommonHelper.TraceLine("Column Name: " + colnamearray[colpos] + " -- ignored for Comparison");
                    }
                }
                test.Pass(string.Format("Table Verification is Pased for given Expected Colnames  : {0} Expected Column values: {1}  ", colnames, colvals));
            }
            catch (Exception e)
            {

                Helper.CommonHelper.PrintScreen("HtmlTableVerification");
                Helper.CommonHelper.TraceLine("*******Verify Cell Values Failed********");
                Assert.Fail("Expcetion from VerifyGridCellValues: --->" + e.ToString());
            }

        }
        public static IWebElement Getelementbyrow(string searchby, string elem, int row)
        {
            IWebElement el;
            el = driver.FindElement(By.XPath(elem + "[" + row + "]"));
            return el;
        }
        public static IWebElement getTableByColumnName(string colname)
        {
            IWebElement tbl = null;
            try
            {
                wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.TagName("table")));
                ReadOnlyCollection<IWebElement> allHtmlTables = driver.FindElements(By.TagName("table"));
                int index = allHtmlTables.IndexOf(allHtmlTables.FirstOrDefault(x => x.Text.Contains(colname)));
                if (index != -1)
                {
                    tbl = allHtmlTables[index];
                }
                return tbl;
            }
            catch (Exception e)
            {

                Helper.CommonHelper.TraceLine("Not Able to find the Table by ColumnName: ==>" + colname);
                Assert.Fail("getTableByColumnName Failed" + e.ToString());
                return tbl;
            }
        }

        public static int getIndexofColumnName(IWebElement tbl, string colname)
        {
            int index = -1;
            ReadOnlyCollection<IWebElement> AllRows = tbl.FindElements(By.TagName("tr"));
            ReadOnlyCollection<IWebElement> Allcells = AllRows[0].FindElements(By.TagName("th"));
            if (colname.Contains("|"))
            {
                string[] arrcolvals = colname.Split(new char[] { '|' });
                index = 0;

                foreach (IWebElement indcell in Allcells)
                {
                    if (indcell.Text.Contains(arrcolvals[0]) && indcell.Text.Contains(arrcolvals[1]))
                    {
                        break;
                    }
                    index++;
                }
            }
            else
            {

                index = Allcells.IndexOf(Allcells.FirstOrDefault(x => x.Text.Contains(colname)));
            }
            return index;
        }

        private static string getStringLastCharRemoved(string instr, char c)
        {
            int lastind = instr.LastIndexOf(",");
            int lenstr = instr.Length;
            string ostr = instr.Substring(0, (instr.LastIndexOf(c)));
            return ostr;
        }

        public static string getTextAtIndex(IWebElement tbl, int index, int rowPos = 0, bool clickrow = false)
        {

            // ReadOnlyCollection<HtmlTableCell> allHtmlTableCells = tbl.Find.AllByExpression<HtmlTableCell>("tagname=td");
            ReadOnlyCollection<IWebElement> AllRows = tbl.FindElements(By.TagName("tr"));
            ReadOnlyCollection<IWebElement> Allcells = AllRows[rowPos].FindElements(By.TagName("td"));
            if (clickrow == true)
            {
                Allcells[index].Click();
                // Allcells[index].MouseClick();
            }
            return Allcells[index].Text;

        }
        public static string gettextfromwebtable(string tbl, int rows, int columns)
        {
            CommonHelper.TraceLine("Getting text  for row " + rows);


            string text = driver.FindElement(By.XPath(tbl + "/" + "/tr[" + rows + "]" + "/" + "/td[" + columns + "]")).Text;

            return text;

        }
        private static int CountDigitsAfterDecimal(double value)
        {
            bool start = false;
            int count = 0;
            foreach (var s in value.ToString())
            {
                if (s == '.')
                {
                    start = true;
                }
                else if (start)
                {
                    count++;
                }
            }

            return count;
        }

        public static bool IsDecimalTruncate(double exp, double act)
        {
            bool IsDecimalTruncate = false;
            if (Math.Abs(exp - act) > 0 && CountDigitsAfterDecimal(act) > 2)
            {
                //If There is decimal precision deviation and it is due to roudning inconsistency 
                // for example 89.05455 and 89.05454 
                if ((Math.Abs(exp - act) * (Math.Pow(10, CountDigitsAfterDecimal(act))) < 2))
                {
                    //Handling Tolerance
                    Helper.CommonHelper.TraceLine("*****Tolernce of decimal Values Case due to Rounding Inconsistency Case / Factor ***** ");
                    Helper.CommonHelper.TraceLine("Expected " + exp);
                    Helper.CommonHelper.TraceLine("Actual  " + act);
                    Helper.CommonHelper.TraceLine("their Absolute Differnce " + Math.Abs(exp - act));
                    Helper.CommonHelper.TraceLine("Factor with decimal point " + Math.Pow(10, CountDigitsAfterDecimal(act)));
                    IsDecimalTruncate = true;

                }
            }

            return IsDecimalTruncate;
        }

        public static void verifytablelabels(string colnames, string locator, string searchby, string unit)
        {
            try
            {
                Helper.CommonHelper.TraceLine("Validating column headers in unit system" + unit);
                string[] colnamearray = colnames.Split(new char[] { ';' });
                IWebElement colNameTable = null;
                if (searchby.Equals("xpath"))
                    colNameTable = driver.FindElement(By.XPath(locator));
                int columns = driver.FindElements(By.XPath(locator + "/thead//th")).Count;

                int j = 1; int headercount;
                for (int i = 0; i < colnamearray.Length; i++)
                {

                    string headervaluetext = "";


                    string headervalue = "";

                    headervalue = driver.FindElement(By.XPath(locator + "/thead//th[" + j + "]")).Text;
                    if (headervalue == "")
                    {
                        j++;
                        headercount = driver.FindElements(By.XPath(locator + "/thead//th[" + j + "]/div")).Count;
                    }
                    else
                    {
                        headercount = driver.FindElements(By.XPath(locator + "/thead//th[" + j + "]/div")).Count;
                    }
                    if (headercount == 0)
                    {

                        headervaluetext = driver.FindElement(By.XPath(locator + "/thead//th[" + j + "]")).Text;

                    }
                    if (headercount == 1)
                    {
                        headervaluetext = driver.FindElement(By.XPath(locator + "/thead//th[" + j + "]/div")).Text;

                    }
                    if (headercount > 1)
                    {
                        for (int k = 1; k <= headercount; k++)
                        {
                            string x = locator + "/thead//th[" + j + "]/div[" + k + "]";
                            headervaluetext = headervaluetext + driver.FindElement(By.XPath(locator + "/thead//th[" + j + "]/div[" + k + "]")).Text;
                        }

                    }

                    if (headervaluetext.Trim().Equals((colnamearray[i].ToString())))
                    {
                        Helper.CommonHelper.TraceLine(string.Format("Column label {0} is   equalto {1}", headervaluetext.Trim(), colnamearray[i].ToString()));
                    }
                    else
                    {
                        Helper.CommonHelper.TraceLine(string.Format("Column label {0} is  not equalto {1}", headervaluetext.Trim(), colnamearray[i].ToString()));
                    }



                    j++;
                }





            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Helper.CommonHelper.PrintScreen("ModelDataVerification");
                Helper.CommonHelper.TraceLine("*******Verify Cell Values Failed********");

            }

        }

        public static void verifytabledata(string colvals, int rows)
        {
            //  int rows = driver.FindElements((By.XPath(PageObjects.WellAnalysisPage.patternmatchingrows))).Count();
            string[] colvalarray = colvals.Split(new char[] { ';' });

            CommonHelper.TraceLine("Verifying data for row " + rows);
            int columns = driver.FindElements(By.XPath("(" + PageObjects.WellAnalysisPage.patternmatchingrows + ")[" + rows + "]/td")).Count();
            for (int j = 1; j <= columns; j++)
            {
                string colvals1 = driver.FindElement(By.XPath("(" + PageObjects.WellAnalysisPage.patternmatchingrows + ")[" + rows + "]/td[" + j + "]")).Text;

                Assert.AreEqual(colvalarray[j - 1], colvals1, "{0} is not equal to {1}", colvalarray[j - 1], colvals1);
                CommonHelper.TraceLine(colvalarray[j - 1] + "is equal to" + colvals1);

            }



        }
        public static void verifymodelgriddata(string e, string searchBy, string colvals, string searchValue, int k, string unit, ExtentTest test)
        {
            string[] colvalarray = colvals.Split(new char[] { ';' });
            var list = new List<string>(colvalarray);

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Contains("ignorevalue"))
                {
                    list.RemoveAt(i);
                }
            }
            List<List<IWebElement>> elementlist = new List<List<IWebElement>>();
            IWebElement ele = null;
            #region SearchCrieria

            try
            {
                CommonHelper.TraceLine("Verifying grid values " + "in unit system " + unit + " for row " + k);
                switch (searchBy.ToLower())
                {
                    case "xpath":
                        int rows = driver.FindElements(By.XPath(e + "/tr")).Count;
                        if (rows == 1)
                        {
                            elementlist.Add(new List<IWebElement>());
                            int columns = driver.FindElements(By.XPath(e + "/tr/td")).Count;
                            List<IWebElement> elmntlst = new List<IWebElement>();
                            elementlist.Add(elmntlst);
                            for (int j = 1; j <= columns; j++)
                            {
                                ele = driver.FindElement(By.XPath(e + "/tr/td[" + j + "]"));
                                if (ele.Text != " No records available. ")
                                    elmntlst.Add(ele);
                            }
                            for (int l = 0; l < list.Count; l++)
                            {
                                decimal number;
                                if (decimal.TryParse(list[l], out number) || decimal.TryParse(elmntlst[l].Text, out number))
                                {
                                    Assert.AreEqual(decimal.Round(Convert.ToDecimal(list[l]), 2), decimal.Round(Convert.ToDecimal(elmntlst[l].Text), 2), string.Format(" {0} is not equal to {1}", list[l], elmntlst[l].Text, test));
                                }
                                else
                                {
                                    Assert.AreEqual(list[l], elmntlst[l].Text, string.Format(" {0} is not equal to {1}", list[l], elmntlst[l].Text, test));
                                }
                            }
                        }
                        else if (rows > 1)
                        {
                            elementlist.Add(new List<IWebElement>());
                            int columns = driver.FindElements(By.XPath(e + "/tr[" + k + "]/td")).Count;
                            List<IWebElement> elmntlst = new List<IWebElement>();
                            elementlist.Add(elmntlst);
                            for (int j = 1; j <= columns; j++)
                            {
                                ele = driver.FindElement(By.XPath(e + "/tr[" + k + "]/td[" + j + "]"));
                                elmntlst.Add(ele);
                            }
                            for (int l = 0; l < list.Count; l++)
                            {
                                decimal number;
                                if (decimal.TryParse(list[l], out number) || decimal.TryParse(elmntlst[l].Text, out number))
                                {
                                    Assert.AreEqual(decimal.Round(Convert.ToDecimal(list[l]), 2), decimal.Round(Convert.ToDecimal(elmntlst[l].Text), 2), string.Format(" {0} is not equal to {1}", list[l], elmntlst[l].Text, test));
                                }
                                else
                                {
                                    Assert.AreEqual(list[l], elmntlst[l].Text, string.Format(" {0} is not equal to {1}", list[l], elmntlst[l].Text, test));
                                }
                            }
                        }
                        break;
                    default:
                        CommonHelper.TraceLine("element can not be identified");
                        break;
                }
            }
            #endregion
            catch (Exception e1)
            {
                Trace.WriteLine(e1.Message);
                CommonHelper.PrintScreen("ModelDataVerification");
                CommonHelper.TraceLine(string.Format("Unable to Find HTMLinputControl Type using {0},{1}", searchBy, searchValue));
                throw e1;
            }
        }

        public static List<IWebElement> getWebelementNextSibling(string searchBy, string searchValue, string desc)
        {
            List<IWebElement> elementlist = new List<IWebElement>();
            IWebElement ele = null;
            IWebElement elenext = null;

            elemdesc = desc;
            if (loglevel.Equals("2"))
            {
                CommonHelper.TraceLine(string.Format("For Control Control using searchby as {0} and searchvalue as {1}", searchBy, searchValue));
            }
            #region SearchCrieria
            try
            {

                switch (searchBy.ToLower())
                {

                    case "xpath":
                        {
                            CommonHelper.TraceLine("Looking for Element: By Xpath : for " + desc);
                            string nextsibling = "(" + searchValue + "/following::*)[1]";
                            ele = driver.FindElement(By.XPath(searchValue));
                            elenext = driver.FindElement(By.XPath(nextsibling));
                            SeleniumActions.waitForElement(By.XPath(nextsibling));
                            elementlist.Add(ele);
                            elementlist.Add(elenext);
                            break;
                        }

                    default:
                        {
                            CommonHelper.TraceLine("Not Valid Locator: " + dtlValidLocators);
                            break;
                        }


                }
            }
            catch (Exception e)
            {

                CommonHelper.TraceLine(string.Format("Unable to Find HTMLinputContril Type using {0},{1}", searchBy, searchValue));
                throw e;
            }
            #endregion
            return elementlist;
        }
        public static void scrolldown(int pixel)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("window.scrollBy(0," + pixel + ")");
        }

        public static void scrollintoview(By elem)
        {

            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView();", driver.FindElement(elem));

        }
        public static void drawhighlight(IWebElement el)
        {

            for (int i = 0; i < 3; i++)
            {
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                js.ExecuteScript("arguments[0].setAttribute('style', arguments[1]);", el, "color: red; border: 2px solid red;");
            }
        }
        public static void scrollbyel(By el, string hor, string ver)
        {
            wait.Until(ExpectedConditions.ElementToBeClickable(el));
            IWebElement ely = driver.FindElement(el);
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("arguments[0].scrollBy(arguments[1], arguments[2]);", ely, hor, ver);
            /*
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("document.body.style.zoom = arguments[0]; ", zoomperc);
            */

        }
        public static void Verifygridmultvalues(string colvals, string searchBy, string searchvalue, string desc)
        {

            string[] colvalarray = colvals.Split(new char[] { ';' });

            var list = new List<string>(colvalarray);




            elemdesc = desc;

            #region SearchCrieria
            try
            {

                if (searchBy.ToLower().Equals("xpath"))
                {

                    for (int i = 0; i < colvalarray.Length; i++)
                    {
                        Thread.Sleep(1000);

                        string s = driver.FindElement(By.XPath("(" + searchvalue + ")[" + (i + 1) + "]")).GetAttribute("innerText");

                        Assert.AreEqual(s, colvalarray[i].ToString(), "{0} in grid does not match with given data {1}", s, colvalarray[i].ToString());
                        CommonHelper.TraceLine(String.Format("{0} in grid matches with given data {1}", s, colvalarray[i].ToString()));



                    }
                    Thread.Sleep(2000);
                }

                else
                {
                    CommonHelper.TraceLine("Not Valid Locator: " + dtlValidLocators);

                }



            }
            catch (Exception e)
            {

                CommonHelper.TraceLine(string.Format("Unable to Find HTMLinputContril Type using {0}", searchBy));
                throw e;
            }
            #endregion

        }
        public static void kendotextboxentervalues(string searchBy, string searchValue, string val1, string val2, string desc)
        {

            IWebElement ele = null;
            IWebElement elenext = null;

            elemdesc = desc;
            if (loglevel.Equals("2"))
            {
                CommonHelper.TraceLine(string.Format("For Control Control using searchby as {0} and searchvalue as {1}", searchBy, searchValue));
            }
            #region SearchCrieria
            try
            {

                switch (searchBy.ToLower())
                {

                    case "xpath":
                        {
                            CommonHelper.TraceLine("Looking for Element: By Xpath : for " + desc);
                            string nextsibling = searchValue + "/following::*[1]//kendo-numerictextbox//input";
                            ele = driver.FindElement(By.XPath(searchValue));
                            elenext = driver.FindElement(By.XPath(nextsibling));
                            ele.Click();
                            ele.Clear();
                            ele.SendKeys(Keys.Home);
                            ele.SendKeys(val1);
                            if (elenext.Displayed)
                            {
                                elenext.Click();
                                elenext.Clear();
                                elenext.SendKeys(Keys.Home);
                                elenext.SendKeys(val2);
                            }

                            break;
                        }

                    default:
                        {
                            CommonHelper.TraceLine("Not Valid Locator: " + dtlValidLocators);
                            break;
                        }


                }
            }
            catch (Exception e)
            {

                CommonHelper.TraceLine(string.Format("Unable to Find HTMLinputContril Type using {0},{1}", searchBy, searchValue));
                throw e;
            }
            #endregion

        }

        //Function to enter textbox having same xpath 
        public static void kendotextboxentermultiplevalues(string searchBy, string searchvalue, string vals, string desc)
        {
            string[] colvalarray = vals.Split(new char[] { ';' });
            var list = new List<string>(colvalarray);




            elemdesc = desc;

            #region SearchCrieria
            try
            {

                switch (searchBy.ToLower())
                {

                    case "xpath":
                        {
                            for (int i = 0; i < colvalarray.Length; i++)
                            {
                                driver.FindElement(By.XPath("(" + searchvalue + ")[" + (i + 1) + "]")).SendKeys(list[i].ToString());

                            }
                            break;
                        }

                    default:
                        {
                            CommonHelper.TraceLine("Not Valid Locator: " + dtlValidLocators);
                            break;
                        }


                }
            }
            catch (Exception e)
            {

                CommonHelper.TraceLine(string.Format("Unable to Find HTMLinputContril Type using {0}", searchBy));
                throw e;
            }
            #endregion

        }


        public static void verifyGridvalue(string columnname, List<IWebElement> columnlist, string xpath, string expvalue)
        {
            bool flag1 = false;
            try
            {
                columnlist = SeleniumActions.Gettotalrecordsinlist_type2("xpath", xpath);

                foreach (var column in columnlist)
                {
                    if (column.Text != "")
                    {
                        string columnvalue = SeleniumActions.getInnerText(column);
                        CommonHelper.TraceLine(columnname + " value found: " + columnvalue);

                        if (columnvalue.ToLower().Trim() == expvalue.ToLower().Trim())
                        {
                            flag1 = true;
                            CommonHelper.TraceLine("The value is present in the grid for column" + columnname);
                            break;
                        }
                        else
                        {
                            CommonHelper.TraceLine("The value- " + columnvalue + ", is not found in the grid for column " + columnname);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CommonHelper.TraceLine("The value is not present in the grid for column" + columnname + ". Exception:" + ex);
                Assert.IsTrue(flag1, "The value is not found in the grid for column " + columnname);
            }

        }


        #endregion
    }
}
