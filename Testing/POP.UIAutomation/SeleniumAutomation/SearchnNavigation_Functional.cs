
using AventStack.ExtentReports;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using SeleniumAutomation.Helper;
using SeleniumAutomation.SeleniumObject;
using SeleniumAutomation.TestClasses;
using System;
using System.Configuration;
using System.Threading;

namespace SeleniumAutomation
{
    /// <summary>
    /// UI Automation Test Suite.
    /// </summary>
    [TestClass]
    public class SearchnNavigation_Functional
    {
        public SearchnNavigation_Functional()
        {
        }
        static ExtentReports report;
        static ExtentTest test;
        static WellConfiguration config = new WellConfiguration();


        [ClassInitialize]
        public static void SetUp(TestContext context)
        {
            report = ExtentFactory.getInstance();
            string isruunisats = ConfigurationManager.AppSettings.Get("isRunningATS");

            SeleniumActions.InitializeWebDriver();
            Assert.AreEqual(SeleniumAutomation.TestData.WellConfigData.HomePageTitle, SeleniumActions.getBrowserTitle(), "Title is NOT matched ");
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.DashboardPage.tabDashboards);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.DashboardPage.tabProdDashboard);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitForElement(SeleniumAutomation.PageObjects.WellConfigurationPage.pedashboard);
            SeleniumActions.WaitForLoad();

        }

        [TestInitialize]
        public void CreateAsset()
        {
            Helper.CommonHelper.ApiUITestBase.Authenticate();
            Helper.CommonHelper.createasset();
            SeleniumActions.refreshBrowser();
        }

        [TestCategory("SearchnNavareaFunctional"), TestMethod]
        public void WildCardSearch()
        {
            try
            {
                string[] wells = new string[4];
                test = report.CreateTest("WildCard Search");
                test.Info("Search well using wildcard");
                config.DashbordfilterVlidation();
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.configurationTab);
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.wellconfigurationTab);
                SeleniumActions.WaitForLoad();
                wells[0] = config.CreateRRLWellPartialonBlankDBAssets(test, "RR" + SeleniumActions.RandomString(4), "RPOC_00003");
                SeleniumActions.WaitForLoad();

                /* SeleniumActions.waitClick(PageObjects.WellConfigurationPage.deletebutton);
                 SeleniumActions.waitClick(PageObjects.WellConfigurationPage.firstdelete);
                 SeleniumActions.waitClick(PageObjects.WellConfigurationPage.firstdelete);
                 SeleniumActions.WaitForLoad();*/
                wells[1] = config.CreateRRLWellPartialonBlankDBAssets(test, SeleniumActions.RandomString(4) + "EL", "RPOC_00004");
                SeleniumActions.WaitForLoad();

                wells[2] = config.CreateRRLWellPartialonBlankDBAssets(test, "TX" + SeleniumActions.RandomString(4) + "TZ", "RPOC_00005");

                wells[3] = config.CreateRRLWellPartialonBlankDBAssets(test, "ZYTHU345", "RPOC_00006");
                SeleniumActions.WaitForLoad();

                foreach (string i in wells)
                {
                    Console.WriteLine(i);
                }
                SeleniumActions.waitClick(PageObjects.WellConfigurationPage.wellselector);
                SeleniumActions.sendText(PageObjects.WellConfigurationPage.wellselectorinputfield, "R" + "R" + "*");
                Thread.Sleep(2000);
                string text;
                System.Collections.Generic.IList<IWebElement> retwells = new System.Collections.Generic.List<IWebElement>();
                retwells = SeleniumActions.Gettotalrecordsinlist("xpath", "//div[@class='ag-center-cols-container']/div//span[@class='value-span-ws']");
                foreach (IWebElement well in retwells)
                {
                    text = SeleniumActions.getInnerText(well).ToLower().Trim();
                    char[] textarr = text.ToCharArray();
                    if (textarr[0].Equals('r') && textarr[1].Equals('r'))
                    {
                        CommonHelper.TraceLine("Found well in list for search RR*: " + text);


                    }
                    else
                    {
                        CommonHelper.TraceLine("Found well in list for search RR*: " + text);
                        SeleniumActions.takeScreenshot("Wild Card Search");
                        test.Fail("Found well in list: " + text, MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Wild Card Search.png"))).Build());
                        Assert.Fail("Found well in list: " + text);
                    }

                }
                retwells = null;
                SeleniumActions.sendText(PageObjects.WellConfigurationPage.wellselectorinputfield, "*" + "E" + "L");
                Thread.Sleep(2000);
                retwells = new System.Collections.Generic.List<IWebElement>();
                retwells = SeleniumActions.Gettotalrecordsinlist("xpath", "//div[@class='ag-center-cols-container']/div//span[@class='value-span-ws']");
                foreach (IWebElement well in retwells)
                {
                    text = SeleniumActions.getInnerText(well).ToLower().Trim();
                    char[] textarr = text.ToCharArray();
                    int len = textarr.Length;
                    if (textarr[len - 2].Equals('e') && textarr[len - 1].Equals('l'))
                    {
                        CommonHelper.TraceLine("Found well in list for search *EL: " + text);


                    }
                    else
                    {
                        CommonHelper.TraceLine("Found well in list for search *EL: " + text);
                        SeleniumActions.takeScreenshot("Wild Card Search");
                        test.Fail("Found well in list: " + text, MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Wild Card Search.png"))).Build());
                        Assert.Fail("Found well in list: " + text);
                    }

                }
                retwells = null;
                SeleniumActions.sendText(PageObjects.WellConfigurationPage.wellselectorinputfield, "TX" + "*" + "TZ");
                Thread.Sleep(2000);
                retwells = new System.Collections.Generic.List<IWebElement>();
                retwells = SeleniumActions.Gettotalrecordsinlist("xpath", "//div[@class='ag-center-cols-container']/div//span[@class='value-span-ws']");
                foreach (IWebElement well in retwells)
                {
                    text = SeleniumActions.getInnerText(well).ToLower().Trim();
                    char[] textarr = text.ToCharArray();
                    int len = textarr.Length;
                    if (textarr[0].Equals('t') && textarr[1].Equals('x'))
                    {
                        if (textarr[len - 2].Equals('t') && textarr[len - 1].Equals('z'))
                        {
                            CommonHelper.TraceLine("Found well in list for search TX*TZ: " + text);


                        }
                    }
                    else
                    {
                        CommonHelper.TraceLine("Found well in list for search TX*TZ: " + text);
                        SeleniumActions.takeScreenshot("Wild Card Search");
                        test.Fail("Found well in list: " + text, MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Wild Card Search.png"))).Build());
                        Assert.Fail("Found well in list: " + text);
                    }

                }
                retwells = null;
                SeleniumActions.sendText(PageObjects.WellConfigurationPage.wellselectorinputfield, "*" + "TH" + "*");
                Thread.Sleep(2000);
                retwells = new System.Collections.Generic.List<IWebElement>();
                retwells = SeleniumActions.Gettotalrecordsinlist("xpath", "//div[@class='ag-center-cols-container']/div//span[@class='value-span-ws']");
                foreach (IWebElement well in retwells)
                {
                    text = SeleniumActions.getInnerText(well).ToLower().Trim();

                    if (text.Contains("th"))
                    {

                        CommonHelper.TraceLine("Found well in list for search *TH*: " + text);



                    }
                    else
                    {
                        CommonHelper.TraceLine("Found well in list for search *TH*: " + text);
                        SeleniumActions.takeScreenshot("Wild Card Search");
                        test.Fail("Found well in list: " + text, MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Wild Card Search.png"))).Build());
                        Assert.Fail("Found well in list: " + text);
                    }

                }



            }
            catch (Exception e)
            {
                SeleniumActions.takeScreenshot("Wild card search");
                test.Fail(e.ToString(), MediaEntityBuilder.CreateScreenCaptureFromPath((System.IO.Path.Combine("Screenshots", "Wild card search.png"))).Build());
                Assert.Fail(e.ToString());
            }


        }

        [TestCleanup]
        public void clean()
        {

            SeleniumActions.disposeDriver();

        }
        [ClassCleanup]
        public static void TearDown()
        {
            report.Flush();
            CommonHelper.DeleteWellsByAPI();
            Helper.CommonHelper.Deleteasset();
        }
    }
}
