using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Forms;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
using ArtOfTest.WebAii.Core;
using System.Diagnostics;
using System.Threading;

namespace Teleriktest
{
    /// <summary>
    /// Summary description for CodedUITest1
    /// </summary>
    [CodedUITest]
    public class CodedUITest1
    {
        public CodedUITest1()
        {
        }
        public Manager mgr = null;
        [TestMethod]
        [DeploymentItem  (@"C:\Workspace\Perforce\ForeSite_PH\UQA\AssetsDev\UIAutomation\dlls\Newtonsoft.Json.dll")]
        
        public void CodedUITestMethod1()
        {
            // To generate code for this test, select "Generate Code for Coded UI Test" from the shortcut menu and select one of the menu items.
            InitializeManager();
            gotoPage("http://meinwesswks6:8000");

        }


        public  void InitializeManager()
        {
            Settings mySettings = new Settings();
            mySettings.ClientReadyTimeout = 300000;
            mgr = new Manager(mySettings);
            mgr.Start();
            var browserUsed = BrowserType.InternetExplorer;
          
            mgr.LaunchNewBrowser(browserUsed, true, ProcessWindowStyle.Maximized);
            
            // return mgr;
        }

        public  void gotoPage(string url)
        {
            //    mgr.ActiveBrowser.ClearCache(BrowserCacheType.TempFilesCache);
            //    mgr.ActiveBrowser.ClearCache(BrowserCacheType.Cookies);
            mgr.ActiveBrowser.NavigateTo(url);
            mgr.ActiveBrowser.WaitUntilReady();
            Thread.Sleep(15000);
            //      mgr.ActiveBrowser.WaitForAjax(globalTimeout);
            //       waitforVisible(Page_Dashboard.page_Loader);
            //   waitforInvisible(Page_Dashboard.page_Loader);

        }

        #region Additional test attributes

        // You can use the following additional attributes as you write your tests:

        ////Use TestInitialize to run code before running each test 
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{        
        //    // To generate code for this test, select "Generate Code for Coded UI Test" from the shortcut menu and select one of the menu items.
        //}

        ////Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{        
        //    // To generate code for this test, select "Generate Code for Coded UI Test" from the shortcut menu and select one of the menu items.
        //}

        #endregion

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }
        private TestContext testContextInstance;

        public UIMap UIMap
        {
            get
            {
                if (this.map == null)
                {
                    this.map = new UIMap();
                }

                return this.map;
            }
        }

        private UIMap map;
    }
}
