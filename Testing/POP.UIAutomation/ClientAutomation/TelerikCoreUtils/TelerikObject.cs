#region Comments

// *************************** Telerik Core Library  *******************************
// Purpose : Provide All reusable methods for Object Identification and Object actions
// ***********************************************************************************

#endregion Comments

using ArtOfTest.WebAii.Controls.HtmlControls;
using ArtOfTest.WebAii.Core;
using ArtOfTest.WebAii.ObjectModel;
using ClientAutomation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Telerik.TestingFramework.Controls.KendoUI;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Weatherford.POP.DTOs;
using Weatherford.POP.Enums;
using Microsoft.VisualStudio.TestTools.UITesting;
using KendoControls = Telerik.TestingFramework.Controls.KendoUI;
using NonTelerikHtml = ArtOfTest.WebAii.Controls.HtmlControls;

using CMouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
using ClientAutomation.PageObjects;

namespace ClientAutomation.TelerikCoreUtils
{
    internal static class TelerikObject
    {
        public static string strGlobalTimeout = ConfigurationManager.AppSettings["GlobalTimeout"].ToString();
        public static string appBrowser = ConfigurationManager.AppSettings["Browser"].ToString();
        public static Manager mgr;
        public static int globalTimeout = Convert.ToInt32(strGlobalTimeout);
        public static string Domain = ConfigurationManager.AppSettings.Get("Domain");
        public static string Site = ConfigurationManager.AppSettings.Get("CygNetSite");
        public static string Service = "UIS";
        public static string Exports = ConfigurationManager.AppSettings.Get("Exports");
        public static string CygNetFacility = ConfigurationManager.AppSettings.Get("CygNetFacility");
        public static string IsRunningInATS = ConfigurationManager.AppSettings.Get("IsRunningInATS");
        public static string browserURL = ConfigurationManager.AppSettings.Get("BrowserURL");
        public static string P4Branch = ConfigurationManager.AppSettings.Get("P4Branch");
        public static string ExportLocation = ConfigurationManager.AppSettings.Get("ExportLocation");
        public static string ModelFilesLocation = ConfigurationManager.AppSettings.Get("ModelFilesLocation");
        internal const string ForesiteWorkflow_RRL = "ForesiteWorkflow_RRL";
        internal const string ForesiteWorkflow_ESP = "ForesiteWorkflow_ESP";
        internal const string ForesiteWorkflow_GLift = "ForesiteWorkflow_GLift";
        internal const string ForesiteWorkflow_NF = "ForesiteWorkflow_NF";
        internal const string ForesiteWorkflow_GInj = "ForesiteWorkflow_GInj";
        internal const string ForesiteWorkflow_WInj = "ForesiteWorkflow_WInj";
        internal const string dtlValidLocators = "Valid locators are id ,name ,content ,attribute,expression";


        #region TelerikManager
        public static void InitializeManager()
        {
            Settings mySettings = new Settings();
            mySettings.ClientReadyTimeout = 300000;
            mgr = new Manager(mySettings);




            mgr.Start();
            var browserUsed = BrowserType.InternetExplorer;
            switch (appBrowser.ToLower())
            {
                case "ie":
                    { browserUsed = BrowserType.InternetExplorer; break; }
                case "chrome":
                    { browserUsed = BrowserType.Chrome; break; }
                case "firefox":
                    { browserUsed = BrowserType.FireFox; break; }
            }
            mgr.LaunchNewBrowser(browserUsed, true, ProcessWindowStyle.Maximized);
            if (appBrowser.ToLower() == "chrome")
            {
                // mgr.ActiveBrowser.NavigateTo("https://google.com");
                mgr.ActiveBrowser.ClearCache(BrowserCacheType.Cookies);
                Thread.Sleep(2000);

            }
            // return mgr;
            gotoPage(browserURL);
            DoStaticWait();
        }
        #endregion


        #region CoreObjectUtils
        #region GetElement

        /// <summary>
        /// Method returns an Generic WebAii.ObjectMdoel.Element
        /// </summary>
        /// <param name="searchby">The Locator that will be used to search Element </param>
        /// <param name="searchvalue">The Locaotor Value that will be used to search Element</param>
        /// <param name="eleminfo">The Element description string that describes field name that appears on UI</param>
        /// <returns></returns>

        #endregion
        public static Element GetElement(string searchby, string searchvalue, string eleminfo)
        {
            Element el = null;

            Helper.CommonHelper.TraceLine("Searching for  Test Object  ==> " + eleminfo);
            for (int i = 0; i < globalTimeout; i++)
            {
                Thread.Sleep(1000);
                DateTime dti = DateTime.Now;
                mgr.ActiveBrowser.RefreshDomTree();
                el = getElementSearchValue(searchby, searchvalue);
                if (el != null)
                {
                    DateTime dte = DateTime.Now;
                    Helper.CommonHelper.TraceLine(string.Format("Obtained Test Object:  in {0} secs  ==>  {1}", ((dte - dti).TotalSeconds), eleminfo));
                    break;
                }
            }
            return el;
        }
        /// <summary>
        /// Public Method returns an Html control types 28 Supported for Telerik
        /// </summary>
        /// <param name="controltype">Control Type Supported for Telerik </param>
        /// <param name="searchby">The Locator that will be used to search Element</param>
        /// <param name="searchvalue">The Locaotor Value that will be used to search Element</param>
        /// <param name="eleminfo">The Element description string that describes field name that appears on UI</param>
        /// <returns></returns>
        public static HtmlControl GetElement(string controltype, string searchby, string searchvalue, string eleminfo, bool noLookupWait = false)
        {
            HtmlControl el = null;
            DateTime dti = DateTime.Now;
            if (noLookupWait == true)
            {
                Helper.CommonHelper.TraceLine("[GetElement]: Searching for  Test Object  ==> " + eleminfo);
                mgr.ActiveBrowser.RefreshDomTree();
                el = getElementSearchValue(controltype, searchby, searchvalue);
                return el;
            }
            for (int i = 0; i < globalTimeout; i++)
            {

                Helper.CommonHelper.TraceLine("[GetElement]: Searching for  Test Object  ==> " + eleminfo + "[ " + (i + 1) + " Time] ");
                mgr.ActiveBrowser.RefreshDomTree();
                el = getElementSearchValue(controltype, searchby, searchvalue);
                Thread.Sleep(1000);
                if (el != null)
                {
                    DateTime dte = DateTime.Now;
                    Helper.CommonHelper.TraceLine(string.Format("[GetElement]: Obtained Test Object:  in {0} secs  ==>  {1}", ((dte - dti).TotalSeconds), eleminfo));
                    break;
                }

            }
            if (el == null)
            {
                Helper.CommonHelper.TraceLine(string.Format("Not able to Find UI control in {0} Seconds ", globalTimeout));
            }
            return el;
        }

        //public static HtmlControl getElementG(Type Class, string searchby, string searchvalue, string eleminfo)
        //{
        //    HtmlControl el = null;
        //    for (int i = 0; i < globalTimeout; i++)
        //    {
        //        Thread.Sleep(1000);
        //        mgr.ActiveBrowser.RefreshDomTree();
        //        el = mgr.ActiveBrowser.Find.ById<Class>("searchvalue");
        //    }
        //       return el;
        //}

        /// <summary>
        /// Method returns an HtmlControl 
        /// </summary>
        /// <param name="searchby">The Locator that will be used to search Element</param>
        /// <param name="searchvalue">The Locaotor Value that will be used to search Element</param>
        /// <param name="eleminfo">The Elelemnt description string that describes field name that appears on UI</param>
        /// <returns></returns>
        public static ReadOnlyCollection<Element> GetElementCollection(string searchby, string searchvalue, string eleminfo)
        {
            //  mgr = InitializeManager();
            ReadOnlyCollection<Element> getElementCollection = null;
            for (int i = 0; i < globalTimeout; i++)
            {
                mgr.ActiveBrowser.RefreshDomTree();
                Thread.Sleep(1000);
                //       ClientAutomation.Helper.CommonHelper.TraceLine("using  Values for  Searchby :" + searchby + "Saerch value " + searchvalue);
                getElementCollection = getElementSearchValueCollection(searchby, searchvalue);
                if (getElementCollection != null)
                {
                    Helper.CommonHelper.TraceLine("Obtained Test Object Collection  ==> " + eleminfo);
                    Thread.Sleep(1000);
                    break;
                }
            }
            return getElementCollection;
        }
        /// <summary>
        /// Method returns an Generic WebAii.ObjectMdoel.Element using Index; This will be used only when we have controls 
        /// with similar properties and no unique or addtioanl help properties
        /// </summary>
        /// <param name="searchby">The Locator that will be used to search Element</param>
        /// <param name="searchvalue">The Locaotor Value that will be used to search Element</param>
        /// <param name="index">The Ordinal Position that will be used</param>
        /// <param name="eleminfo">The Elelemnt description string that describes field name that appears on UI</param>
        /// <returns></returns>
        public static Element GetElementByIndex(string searchby, string searchvalue, int index, string eleminfo)
        {
            //  mgr = InitializeManager();
            Element el = null;
            Helper.CommonHelper.TraceLine("[" + DateTime.Now + "] Before Elem Search: " + eleminfo);
            for (int i = 0; i < globalTimeout; i++)
            {
                mgr.ActiveBrowser.RefreshDomTree();
                el = getElementSearchValueCollection(searchby, searchvalue, index);
                if (el == null) continue;
                Helper.CommonHelper.TraceLine("[" + DateTime.Now + "] Obtained Test Object: ===> " + eleminfo);
                break;

            }
            return el;
        }
        private static Element getElementSearchValue(string searchBy, string searchValue)
        {
            Element _el = null;
            #region SearchCrieria
            switch (searchBy.ToLower())
            {
                case "id":
                    {
                        try
                        {
                            _el = mgr.ActiveBrowser.Find.ById(searchValue);
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }
                        break;
                    }

                case "content":
                    {
                        try
                        {
                            _el = mgr.ActiveBrowser.Find.ByContent(searchValue);
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }
                        break;
                    }
                case "xpath":
                    {
                        try
                        {
                            _el = mgr.ActiveBrowser.Find.ByXPath(searchValue);
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }
                        break;
                    }
                case "name":
                    {
                        try
                        {
                            _el = mgr.ActiveBrowser.Find.ByName(searchValue);
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }
                        break;
                    }
                default:
                    {
                        break;

                    }

                    #endregion
            }
            return _el;
        }

        private static HtmlControl getElementSearchValue(string ctl, string searchBy, string searchValue)
        {

            HtmlControl _el = null;
            switch (ctl.ToLower())
            {

                case "htmlbutton":
                    {
                        _el = getHtmlButton(searchBy, searchValue);
                        break;
                    }
                case "htmlanchor":
                    {
                        _el = getHtmlAnchor(searchBy, searchValue);
                        break;

                    }
                case "htmlinputcontrol":
                    {
                        _el = getHtmlInputControl(searchBy, searchValue);
                        break;
                    }
                case "htmldefinitiondescription":
                    {
                        break;
                    }
                case "htmldiv":
                    {
                        _el = getHtmlDiv(searchBy, searchValue);
                        break;
                    }
                case "htmldefinitionlist":

                    {
                        break;
                    }
                case "htmldefinitionterm":
                    {
                        break;
                    }
                case "htmlform":
                    {
                        break;
                    }
                case "htmlimage":
                    {
                        break;
                    }
                case "htmlinputbutton":
                    {
                        break;
                    }
                case "htmlinputcheckbox":
                    {
                        _el = getHtmlInputCheckBox(searchBy, searchValue);
                        break;
                    }
                case "htmlinputfile":
                    {
                        break;
                    }
                case "htmlinputhidden":
                    {
                        break;
                    }

                case "htmlinputimage":
                    {
                        break;
                    }
                case "htmlinputpassword":
                    {
                        break;
                    }
                case "htmlinputradiobutton":
                    {
                        break;
                    }
                case "htmlinputrange":
                    {
                        _el = getHtmlInputRange(searchBy, searchValue);
                        break;
                    }
                case "htmlinputreset":
                    {
                        break;
                    }
                case "htmlinputsubmit":
                    {
                        break;
                    }
                case "htmllistitem":
                    {
                        _el = getHtmlListItem(searchBy, searchValue);
                        break;
                    }
                case "htmlinputtext":
                    {
                        _el = getHtmlInputText(searchBy, searchValue);
                        break;
                    }
                case "htmloption":
                    {
                        break;
                    }
                case "htmlorderedlist":
                    {
                        break;
                    }
                case "htmlselect":
                    {
                        _el = getHtmlSelect(searchBy, searchValue);
                        break;
                    }
                case "htmlspan":
                    {

                        {
                            _el = getHtmlSpan(searchBy, searchValue);
                            break;
                        }

                    }
                case "htmltable":

                    {
                        _el = getHtmlTable(searchBy, searchValue);

                        break;
                    }
                case "htmltablecell":
                    {
                        _el = getHtmlTableCell(searchBy, searchValue);
                        break;
                    }
                case "htmltablerow":
                    {
                        break;
                    }
                case "htmltextarea":
                    {
                        break;
                    }
                case "htmlinputnumber":
                    {
                        _el = getHtmlInputNumber(searchBy, searchValue);
                        break;
                    }
                case "htmlunorderedlist":
                    {
                        break;
                    }
                case "kendo-dropdownlist":
                    {
                        _el = getKendodropdownlist(searchBy, searchValue);
                        break;
                    }
                case "kendo-numerictextbox":
                    {
                        _el = getKendoNumericTextbox(searchBy, searchValue);
                        break;
                    }
                case "kendo-panelbar-item":
                    {
                        _el = getKendoPanelbarItem(searchBy, searchValue);
                        break;
                    }
                case "kendo-dateinput":
                    {
                        _el = getKendoDateinput(searchBy, searchValue);
                        break;
                    }
                case "htmlcontrol":
                    {
                        _el = getHtmlControl(searchBy, searchValue);
                        break;
                    }
                case "htmlcontrolnextsibling":
                    {
                        _el = getNextSiblingHtmlControl(searchBy, searchValue);
                        break;
                    }
                case "htmlcontrolparentnextsibling":
                    {
                        _el = getParentNextSiblingHtmlControl(searchBy, searchValue);
                        break;
                    }
                case "foresitetoastercontrol":
                    {
                        _el = getForeSiteToasterControl();
                        break;
                    }
                default:
                    Helper.CommonHelper.TraceLine(string.Format("Given Control Type {0} is not avaialble in Telerik supported Controls", ctl));
                    break;
            }


            return _el;
        }


        private static HtmlControl getHtmlInputNumber(string searchBy, string searchValue)
        {
            HtmlInputNumber ctl = null;
            #region SearchCrieria
            try
            {

                switch (searchBy.ToLower())

                {
                    case "id":
                        {
                            ctl = mgr.ActiveBrowser.Find.ById<HtmlInputNumber>(searchValue);
                            break;
                        }

                    case "name":
                        {
                            ctl = mgr.ActiveBrowser.Find.ByName<HtmlInputNumber>(searchValue);
                            break;
                        }
                    case "content":
                        {
                            ctl = mgr.ActiveBrowser.Find.ByContent<HtmlInputNumber>(searchValue);
                            break;
                        }
                    case "attribute":
                        {
                            ctl = mgr.ActiveBrowser.Find.ByAttributes<HtmlInputNumber>(searchValue);
                            break;
                        }
                    default:
                        {
                            Helper.CommonHelper.TraceLine("Not Valid Locator: " + dtlValidLocators);
                            break;
                        }


                }
            }
            catch (Exception e)
            {

                throw e;
            }
            #endregion
            return ctl;
        }

        private static HtmlControl getHtmlAnchor(string searchBy, string searchValue)
        {
            HtmlAnchor ctl = null;
            #region SearchCrieria
            try
            {

                switch (searchBy.ToLower())

                {
                    case "id":
                        {
                            ctl = mgr.ActiveBrowser.Find.ById<HtmlAnchor>(searchValue);
                            break;
                        }

                    case "name":
                        {
                            ctl = mgr.ActiveBrowser.Find.ByName<HtmlAnchor>(searchValue);
                            break;
                        }
                    case "content":
                        {
                            ctl = mgr.ActiveBrowser.Find.ByContent<HtmlAnchor>(searchValue);
                            break;
                        }
                    case "attribute":
                        {
                            ctl = mgr.ActiveBrowser.Find.ByAttributes<HtmlAnchor>(searchValue);
                            break;
                        }
                    default:
                        {
                            Helper.CommonHelper.TraceLine("Not Valid Locator: " + dtlValidLocators);
                            break;
                        }


                }
            }
            catch (Exception e)
            {

                throw e;
            }
            #endregion
            return ctl;
        }

        private static HtmlControl getHtmlControl(string searchBy, string searchValue)
        {
            HtmlControl ctl = null;
            #region SearchCrieria
            try
            {

                switch (searchBy.ToLower())

                {
                    case "id":
                        {
                            ctl = mgr.ActiveBrowser.Find.ById<HtmlControl>(searchValue);
                            break;
                        }

                    case "name":
                        {
                            ctl = mgr.ActiveBrowser.Find.ByName<HtmlControl>(searchValue);
                            break;
                        }
                    case "content":
                        {
                            ctl = mgr.ActiveBrowser.Find.ByContent<HtmlControl>(searchValue);
                            break;
                        }

                    case "attribute":
                        {
                            ctl = mgr.ActiveBrowser.Find.ByAttributes<HtmlControl>(searchValue);
                            break;
                        }
                    case "xpath":
                        {
                            ctl = mgr.ActiveBrowser.Find.ByXPath<HtmlControl>(searchValue);
                            break;
                        }
                    case "expression":
                        {
                            string[] exprnarr = searchValue.Split(new char[] { ';' });
                            ctl = mgr.ActiveBrowser.Find.ByExpression<HtmlControl>(exprnarr);
                            break;
                        }
                    default:
                        {
                            Helper.CommonHelper.TraceLine("Not Valid Locator: " + dtlValidLocators);
                            break;
                        }


                }
                #endregion

            }
            catch (Exception e)
            {
                throw e;

            }
            return ctl;
        }


        private static HtmlControl getNextSiblingHtmlControl(string searchBy, string searchValue)
        {
            HtmlControl ctl = null;
            HtmlControl ctlnxt = null;
            #region SearchCrieria
            try
            {

                switch (searchBy.ToLower())

                {
                    case "id":
                        {
                            ctl = mgr.ActiveBrowser.Find.ById<HtmlControl>(searchValue);
                            break;
                        }

                    case "name":
                        {
                            ctl = mgr.ActiveBrowser.Find.ByName<HtmlControl>(searchValue);
                            break;
                        }
                    case "content":
                        {
                            ctl = mgr.ActiveBrowser.Find.ByContent<HtmlControl>(searchValue);
                            break;
                        }

                    case "attribute":
                        {
                            ctl = mgr.ActiveBrowser.Find.ByAttributes<HtmlControl>(searchValue);
                            break;
                        }
                    case "expression":
                        {
                            string[] exprnarr = searchValue.Split(new char[] { ';' });
                            HtmlFindExpression expression = new HtmlFindExpression(exprnarr);
                            ctl = mgr.ActiveBrowser.Find.ByExpression<HtmlControl>(expression);
                            break;
                        }
                    default:
                        {
                            Helper.CommonHelper.TraceLine("Not a Valid Locator: " + dtlValidLocators);
                            //  ctlnxt = ctl.BaseElement.GetNextSibling().As<HtmlControl>();
                            break;
                        }


                }
                #endregion
                if (ctl != null)
                {

                    ctlnxt = ctl.BaseElement.GetNextSibling().As<HtmlControl>();

                }
            }
            catch (Exception e)
            {

                throw e;

            }
            return ctlnxt;
        }

        private static HtmlControl getParentNextSiblingHtmlControl(string searchBy, string searchValue)
        {
            HtmlControl ctl = null;
            HtmlControl ctlnxt = null;
            #region SearchCrieria
            try
            {

                switch (searchBy.ToLower())

                {
                    case "id":
                        {
                            ctl = mgr.ActiveBrowser.Find.ById<HtmlControl>(searchValue);
                            break;
                        }

                    case "name":
                        {
                            ctl = mgr.ActiveBrowser.Find.ByName<HtmlControl>(searchValue);
                            break;
                        }
                    case "content":
                        {
                            ctl = mgr.ActiveBrowser.Find.ByContent<HtmlControl>(searchValue);
                            break;
                        }

                    case "attribute":
                        {
                            ctl = mgr.ActiveBrowser.Find.ByAttributes<HtmlControl>(searchValue);
                            break;
                        }
                    case "expression":
                        {
                            string[] exprnarr = searchValue.Split(new char[] { ';' });
                            HtmlFindExpression expression = new HtmlFindExpression(exprnarr);
                            ctl = mgr.ActiveBrowser.Find.ByExpression<HtmlControl>(expression);
                            break;
                        }
                    default:
                        {
                            Helper.CommonHelper.TraceLine("Not a Valid Locator: " + dtlValidLocators);
                            //  ctlnxt = ctl.BaseElement.GetNextSibling().As<HtmlControl>();
                            break;
                        }

                }
                #endregion
                if (ctl != null)
                {
                    ctlnxt = ctl.BaseElement.Parent.GetNextSibling().As<HtmlControl>();
                }

            }
            catch (Exception e)
            {
                throw e;

            }
            return ctlnxt;
        }

        private static HtmlControl getForeSiteToasterControl()
        {
            HtmlControl toaster = null;
            mgr.ActiveBrowser.RefreshDomTree();
            Thread.Sleep(1000);
            toaster = mgr.ActiveBrowser.Find.ByAttributes<HtmlControl>("class=toast-message ng-star-inserted");
            return toaster;
        }

        #region TelerikSupportedButtons

        private static HtmlButton getHtmlButton(string searchBy, string searchValue)
        {
            HtmlButton ctl = null;
            #region SearchCrieria
            try
            {

                switch (searchBy.ToLower())

                {
                    case "id":
                        {
                            ctl = mgr.ActiveBrowser.Find.ById<HtmlButton>(searchValue);
                            break;
                        }

                    case "name":
                        {
                            ctl = mgr.ActiveBrowser.Find.ByName<HtmlButton>(searchValue);
                            break;
                        }
                    case "content":
                        {
                            ctl = mgr.ActiveBrowser.Find.ByContent<HtmlButton>(searchValue);
                            break;
                        }
                    case "attribute":
                        {
                            ctl = mgr.ActiveBrowser.Find.ByAttributes<HtmlButton>(searchValue);
                            break;
                        }
                    case "xpath":
                        {
                            ctl = mgr.ActiveBrowser.Find.ByXPath<HtmlButton>(searchValue);
                            break;
                        }
                    case "expression":
                        {
                            string[] exprnarr = searchValue.Split(new char[] { ';' });
                            HtmlFindExpression expression = new HtmlFindExpression(exprnarr);
                            ReadOnlyCollection<HtmlButton> allbtns = mgr.ActiveBrowser.Find.AllByExpression<HtmlButton>(expression);
                            if (allbtns.Count > 1)
                            {
                                ctl = allbtns.FirstOrDefault(x => x.IsVisible() == true);
                            }
                            else
                            {
                                ctl = allbtns[0];
                            }
                            // ctl = mgr.ActiveBrowser.Find.ByExpression<HtmlButton>(expression);
                            break;
                        }
                    default:
                        {
                            Helper.CommonHelper.TraceLine("Not Valid Locator: " + dtlValidLocators);
                            break;
                        }


                }
            }
            catch (Exception e)
            {

                throw e;
            }
            #endregion
            return ctl;
        }

        private static HtmlInputControl getHtmlInputControl(string searchBy, string searchValue)
        {
            HtmlInputControl ctl = null;
            Helper.CommonHelper.TraceLine(string.Format("For Control HtmlInputControl using searchby as {0} and searchvalue as {1}", searchBy, searchValue));
            #region SearchCrieria
            try
            {

                switch (searchBy.ToLower())

                {
                    case "id":
                        {
                            ctl = mgr.ActiveBrowser.Find.ById<HtmlInputControl>(searchValue);
                            break;
                        }

                    case "name":
                        {
                            ctl = mgr.ActiveBrowser.Find.ByName<HtmlInputControl>(searchValue);
                            break;
                        }
                    case "content":
                        {
                            ctl = mgr.ActiveBrowser.Find.ByContent<HtmlInputControl>(searchValue);
                            break;
                        }
                    case "attribute":
                        {
                            ctl = mgr.ActiveBrowser.Find.ByAttributes<HtmlInputControl>(searchValue);
                            break;
                        }
                    default:
                        {
                            Helper.CommonHelper.TraceLine("Not Valid Locator: " + dtlValidLocators);
                            break;
                        }


                }
            }
            catch (Exception e)
            {

                Helper.CommonHelper.TraceLine(string.Format("Unable to Find HTMLinputContril Type using {0},{1}", searchBy, searchValue));
                throw e;
            }
            #endregion
            return ctl;
        }

        private static HtmlDiv getHtmlDiv(string searchBy, string searchValue)
        {
            HtmlDiv ctl = null;
            #region SearchCrieria
            try
            {

                switch (searchBy.ToLower())

                {
                    case "id":
                        {
                            ctl = mgr.ActiveBrowser.Find.ById<HtmlDiv>(searchValue);
                            break;
                        }

                    case "name":
                        {
                            ctl = mgr.ActiveBrowser.Find.ByName<HtmlDiv>(searchValue);
                            break;
                        }
                    case "content":
                        {
                            ctl = mgr.ActiveBrowser.Find.ByContent<HtmlDiv>(searchValue);
                            break;
                        }
                    case "attribute":
                        {
                            ctl = mgr.ActiveBrowser.Find.ByAttributes<HtmlDiv>(searchValue);
                            break;
                        }
                    default:
                        {
                            Helper.CommonHelper.TraceLine("Not Valid Locator: " + dtlValidLocators);
                            break;
                        }


                }
            }
            catch (Exception e)
            {

                throw e;
            }
            #endregion
            return ctl;
        }

        private static HtmlInputRange getHtmlInputRange(string searchBy, string searchValue)
        {
            HtmlInputRange ctl = null;
            #region SearchCrieria
            try
            {

                switch (searchBy.ToLower())

                {
                    case "id":
                        {
                            ctl = mgr.ActiveBrowser.Find.ById<HtmlInputRange>(searchValue);
                            break;
                        }

                    case "name":
                        {
                            ctl = mgr.ActiveBrowser.Find.ByName<HtmlInputRange>(searchValue);
                            break;
                        }
                    case "content":
                        {
                            ctl = mgr.ActiveBrowser.Find.ByContent<HtmlInputRange>(searchValue);
                            break;
                        }
                    case "attribute":
                        {
                            ctl = mgr.ActiveBrowser.Find.ByAttributes<HtmlInputRange>(searchValue);
                            break;
                        }
                    default:
                        {
                            Helper.CommonHelper.TraceLine("Not Valid Locator: " + dtlValidLocators);
                            break;
                        }


                }
            }
            catch (Exception e)
            {

                throw e;
            }
            #endregion
            return ctl;
        }
        private static HtmlInputCheckBox getHtmlInputCheckBox(string searchBy, string searchValue)
        {
            HtmlInputCheckBox ctl = null;
            #region SearchCrieria
            try
            {

                switch (searchBy.ToLower())

                {
                    case "id":
                        {
                            ctl = mgr.ActiveBrowser.Find.ById<HtmlInputCheckBox>(searchValue);
                            break;
                        }

                    case "name":
                        {
                            ctl = mgr.ActiveBrowser.Find.ByName<HtmlInputCheckBox>(searchValue);
                            break;
                        }
                    case "content":
                        {
                            ctl = mgr.ActiveBrowser.Find.ByContent<HtmlInputCheckBox>(searchValue);
                            break;
                        }
                    case "attribute":
                        {
                            ctl = mgr.ActiveBrowser.Find.ByAttributes<HtmlInputCheckBox>(searchValue);
                            break;
                        }
                    default:
                        {
                            Helper.CommonHelper.TraceLine("Not Valid Locator: " + dtlValidLocators);
                            break;
                        }


                }
            }
            catch (Exception e)
            {

                throw e;
            }
            #endregion
            return ctl;
        }

        private static HtmlSpan getHtmlSpan(string searchBy, string searchValue)
        {

            HtmlSpan ctl = null;
            #region SearchCrieria
            try
            {

                switch (searchBy.ToLower())

                {
                    case "id":
                        {
                            ctl = mgr.ActiveBrowser.Find.ById<HtmlSpan>(searchValue);
                            break;
                        }

                    case "name":
                        {
                            ctl = mgr.ActiveBrowser.Find.ByName<HtmlSpan>(searchValue);
                            break;
                        }
                    case "content":
                        {
                            ctl = mgr.ActiveBrowser.Find.ByContent<HtmlSpan>(searchValue);
                            break;
                        }
                    case "attribute":
                        {
                            ctl = mgr.ActiveBrowser.Find.ByAttributes<HtmlSpan>(searchValue);
                            break;
                        }
                    case "xpath":
                        {
                            ctl = mgr.ActiveBrowser.Find.ByXPath<HtmlSpan>(searchValue);
                            break;
                        }
                    default:
                        {
                            Helper.CommonHelper.TraceLine("Not a Valid Locator: " + dtlValidLocators);
                            break;
                        }


                }
            }
            catch (Exception e)
            {

                throw e;
            }
            #endregion
            return ctl;
        }


        private static HtmlTable getHtmlTable(string searchBy, string searchValue)
        {

            HtmlTable ctl = null;
            #region SearchCrieria
            try
            {

                switch (searchBy.ToLower())

                {
                    case "id":
                        {
                            ctl = mgr.ActiveBrowser.Find.ById<HtmlTable>(searchValue);
                            break;
                        }

                    case "name":
                        {
                            ctl = mgr.ActiveBrowser.Find.ByName<HtmlTable>(searchValue);
                            break;
                        }
                    case "content":
                        {
                            ctl = mgr.ActiveBrowser.Find.ByContent<HtmlTable>(searchValue);
                            break;
                        }
                    case "attribute":
                        {
                            ctl = mgr.ActiveBrowser.Find.ByAttributes<HtmlTable>(searchValue);
                            break;
                        }
                    case "xpath":
                        {
                            ctl = mgr.ActiveBrowser.Find.ByXPath<HtmlTable>(searchValue);
                            break;
                        }
                    case "innertext":
                        {
                            ctl = getTableByColumnName(searchValue);
                            break;
                        }
                    default:
                        {
                            Helper.CommonHelper.TraceLine("Not a Valid Locator: " + dtlValidLocators);
                            break;
                        }


                }
            }
            catch (Exception e)
            {

                throw e;
            }
            #endregion
            return ctl;
        }
        private static HtmlControl getHtmlTableCell(string searchBy, string searchValue)
        {

            HtmlControl ctl = null;
            #region SearchCrieria
            try
            {

                switch (searchBy.ToLower())

                {
                    case "id":
                        {
                            ctl = mgr.ActiveBrowser.Find.ById<HtmlTableCell>(searchValue);
                            break;
                        }

                    case "name":
                        {
                            ctl = mgr.ActiveBrowser.Find.ByName<HtmlTableCell>(searchValue);
                            break;
                        }
                    case "content":
                        {
                            ReadOnlyCollection<HtmlControl> alltablecells = mgr.ActiveBrowser.Find.AllByTagName<HtmlControl>("td");
                            //foreach ( HtmlControl tblcell in alltablecells)
                            //{
                            //    ClientAutomation.Helper.CommonHelper.TraceLine("Tabel cell contnent:"+tblcell.BaseElement.InnerText);
                            //}
                            ctl = alltablecells.FirstOrDefault(x => x.BaseElement.InnerText.Contains(searchValue));
                            // ctl = mgr.ActiveBrowser.Find.ByContent<HtmlTableCell>(searchValue);
                            break;
                        }
                    case "attribute":
                        {
                            ctl = mgr.ActiveBrowser.Find.ByAttributes<HtmlTableCell>(searchValue);
                            break;
                        }
                    case "xpath":
                        {
                            ctl = mgr.ActiveBrowser.Find.ByXPath<HtmlTableCell>(searchValue);
                            break;
                        }

                    default:
                        {
                            Helper.CommonHelper.TraceLine("Not a Valid Locator: " + dtlValidLocators);
                            break;
                        }


                }
            }
            catch (Exception e)
            {

                throw e;
            }
            #endregion
            return ctl;
        }
        private static HtmlListItem getHtmlListItem(string searchBy, string searchValue)
        {
            HtmlListItem ctl = null;
            #region SearchCrieria
            try
            {

                switch (searchBy.ToLower())

                {
                    case "id":
                        {
                            ctl = mgr.ActiveBrowser.Find.ById<HtmlListItem>(searchValue);
                            break;
                        }

                    case "name":
                        {
                            ctl = mgr.ActiveBrowser.Find.ByName<HtmlListItem>(searchValue);
                            break;
                        }
                    case "content":
                        {
                            ctl = mgr.ActiveBrowser.Find.ByContent<HtmlListItem>(searchValue);
                            break;
                        }

                    case "attribute":
                        {
                            ctl = mgr.ActiveBrowser.Find.ByAttributes<HtmlListItem>(searchValue);
                            break;
                        }
                    default:
                        {
                            Helper.CommonHelper.TraceLine("Not Valid Locator: " + dtlValidLocators);
                            break;
                        }


                }
            }
            catch (Exception e)
            {

                throw e;
            }
            #endregion
            return ctl;
        }
        //HtmlInputText
        private static HtmlInputText getHtmlInputText(string searchBy, string searchValue)
        {
            HtmlInputText ctl = null;
            #region SearchCrieria
            try
            {

                switch (searchBy.ToLower())

                {
                    case "id":
                        {
                            ctl = mgr.ActiveBrowser.Find.ById<HtmlInputText>(searchValue);
                            break;
                        }

                    case "name":
                        {
                            ctl = mgr.ActiveBrowser.Find.ByName<HtmlInputText>(searchValue);
                            break;
                        }
                    case "content":
                        {
                            ctl = mgr.ActiveBrowser.Find.ByContent<HtmlInputText>(searchValue);
                            break;
                        }

                    case "attribute":
                        {
                            ctl = mgr.ActiveBrowser.Find.ByAttributes<HtmlInputText>(searchValue);
                            break;
                        }
                    default:
                        {
                            Helper.CommonHelper.TraceLine("Not Valid Locator: " + dtlValidLocators);
                            break;
                        }


                }
            }
            catch (Exception e)
            {

                throw e;
            }
            #endregion
            return ctl;
        }

        //
        private static HtmlSelect getHtmlSelect(string searchBy, string searchValue)
        {
            HtmlSelect ctl = null;
            #region SearchCrieria
            try
            {

                switch (searchBy.ToLower())

                {
                    case "id":
                        {
                            ctl = mgr.ActiveBrowser.Find.ById<HtmlSelect>(searchValue);
                            break;
                        }

                    case "name":
                        {
                            ctl = mgr.ActiveBrowser.Find.ByName<HtmlSelect>(searchValue);
                            break;
                        }
                    case "content":
                        {
                            ctl = mgr.ActiveBrowser.Find.ByContent<HtmlSelect>(searchValue);
                            break;
                        }

                    case "attribute":
                        {
                            ctl = mgr.ActiveBrowser.Find.ByAttributes<HtmlSelect>(searchValue);
                            break;
                        }
                    default:
                        {
                            Helper.CommonHelper.TraceLine("Not Valid Locator: " + dtlValidLocators);
                            break;
                        }


                }
            }
            catch (Exception e)
            {

                throw e;
            }
            #endregion
            return ctl;
        }


        private static HtmlControl getKendodropdownlist(string searchBy, string searchValue)
        {
            HtmlControl ctl = null;
            ReadOnlyCollection<HtmlControl> allkendotags = mgr.ActiveBrowser.Find.AllByTagName<HtmlControl>("kendo-dropdownlist");


            #region SearchCrieria
            try
            {

                switch (searchBy.ToLower())

                {
                    case "id":
                        {

                            foreach (HtmlControl kendodropdown in allkendotags)
                            {
                                for (int i = 0; i < kendodropdown.Attributes.Count; i++)
                                {
                                    if (kendodropdown.Attributes[i].Name.ToLower() == "id")
                                    {
                                        if (kendodropdown.Attributes[i].Value == searchValue)
                                        {
                                            ctl = kendodropdown;
                                            break;
                                        }
                                    }
                                }

                                if (ctl != null) break;

                            }

                            break;
                        }

                    case "name":


                        {
                            foreach (HtmlControl kendodropdown in allkendotags)
                            {
                                for (int i = 0; i < kendodropdown.Attributes.Count; i++)
                                {
                                    if (kendodropdown.Attributes[i].Name.ToLower() == "name")
                                    {
                                        if (kendodropdown.Attributes[i].Value == searchValue)
                                        {
                                            ctl = kendodropdown;
                                            break;
                                        }
                                    }
                                }

                                if (ctl != null) break;

                            }

                            break;
                        }

                    default:
                        {
                            Helper.CommonHelper.TraceLine("Not Valid Locator: For Kendo UI Dropdown  only Name and ID are  Supported Locators  as of now  ");
                            break;
                        }


                }
            }
            catch (Exception e)
            {

                throw e;
            }
            #endregion
            return ctl;
        }

        public static HtmlControl GetChildrenControl(HtmlControl ctl, string childhrcy)
        {
            HtmlControl childcontrol = null;
            string[] arrcontrol = childhrcy.Split(new char[] { ';' });
            Element el = ctl.BaseElement;
            Element eltransit = null;
            for (int i = 0; i < arrcontrol.Length; i++)
            {
                int k = Convert.ToInt32(arrcontrol[i]);
                if (i == 0)
                {
                    if (el.Children.Count > 0)
                    {
                        eltransit = el.Children[k];
                    }
                }
                else
                {
                    if (el.Children.Count > 0)
                    {
                        eltransit = eltransit.Children[k];
                    }
                }
            }
            if (eltransit != null)
            {
                childcontrol = new HtmlControl(eltransit);
            }
            return childcontrol;
        }

        public static HtmlControl GetPreviousSibling(HtmlControl ctl, int nthsibling)
        {
            HtmlControl childcontrol = null;
            Element el = ctl.BaseElement;
            Element eltransit = null;
            for (int i = 0; i < nthsibling; i++)
            {
                if (i == 0)
                {
                    eltransit = el.GetPreviousSibling();
                }
                else
                {
                    eltransit = eltransit.GetPreviousSibling();
                }
            }
            childcontrol = new HtmlControl(eltransit);
            return childcontrol;
        }

        public static HtmlControl GetParent(HtmlControl ctl, int nthsibling)
        {
            HtmlControl parentcontrol = null;
            Element el = ctl.BaseElement;
            Element eltransit = null;
            for (int i = 0; i < nthsibling; i++)
            {
                if (i == 0)
                {
                    eltransit = el.Parent;
                }
                else
                {
                    eltransit = eltransit.Parent;
                }
            }
            parentcontrol = new HtmlControl(eltransit);
            return parentcontrol;
        }
        public static HtmlControl GetNextSibling(HtmlControl ctl, int nthsibling)
        {
            HtmlControl childcontrol = null;
            Element el = ctl.BaseElement;
            Element eltransit = null;
            for (int i = 0; i < nthsibling; i++)
            {
                if (i == 0)
                {
                    eltransit = el.GetNextSibling();
                }
                else
                {
                    eltransit = eltransit.GetNextSibling();
                }
            }
            childcontrol = new HtmlControl(eltransit);
            return childcontrol;
        }

        private static HtmlControl getKendoNumericTextbox(string searchBy, string searchValue)
        {
            HtmlControl ctl = null;

            Stopwatch stp = new Stopwatch();
            stp.Start();

            ReadOnlyCollection<HtmlControl> allkendotags = mgr.ActiveBrowser.Find.AllByTagName<HtmlControl>("kendo-numerictextbox");



            #region SearchCrieria
            try
            {

                switch (searchBy.ToLower())

                {
                    case "id":
                        {

                            foreach (HtmlControl kendodropdown in allkendotags)
                            {
                                for (int i = 0; i < kendodropdown.Attributes.Count; i++)
                                {
                                    if (kendodropdown.Attributes[i].Name.ToLower() == "id")
                                    {
                                        if (kendodropdown.Attributes[i].Value == searchValue)
                                        {
                                            ctl = kendodropdown;
                                            stp.Stop();
                                            Helper.CommonHelper.TraceLine("Total duration to find KendoUI control in milli secs: " + stp.ElapsedMilliseconds);
                                            break;
                                        }
                                    }
                                }

                                if (ctl != null) break;

                            }

                            break;
                        }

                    case "name":


                        {
                            foreach (HtmlControl kendodropdown in allkendotags)
                            {
                                for (int i = 0; i < kendodropdown.Attributes.Count; i++)
                                {
                                    if (kendodropdown.Attributes[i].Name.ToLower() == "name")
                                    {
                                        if (kendodropdown.Attributes[i].Value == searchValue)
                                        {
                                            ctl = kendodropdown;
                                            break;
                                        }
                                    }
                                }

                                if (ctl != null) break;

                            }

                            break;
                        }

                    default:
                        {
                            Helper.CommonHelper.TraceLine("Not Valid Locator: For Kendo UI Dropdown  only Name and ID are  Supported Locators  as of now  ");
                            break;
                        }


                }
            }
            catch (Exception e)
            {

                throw e;
            }
            #endregion
            return ctl;
        }


        private static HtmlControl getKendoPanelbarItem(string searchBy, string searchValue)
        {
            HtmlControl ctl = null;

            Stopwatch stp = new Stopwatch();
            stp.Start();

            ReadOnlyCollection<HtmlControl> allkendotags = mgr.ActiveBrowser.Find.AllByTagName<HtmlControl>("kendo-panelbar-item");



            #region SearchCrieria
            try
            {

                switch (searchBy.ToLower())

                {
                    case "id":
                        {

                            foreach (HtmlControl kendodropdown in allkendotags)
                            {
                                for (int i = 0; i < kendodropdown.Attributes.Count; i++)
                                {
                                    if (kendodropdown.Attributes[i].Name.ToLower() == "id")
                                    {
                                        if (kendodropdown.Attributes[i].Value == searchValue)
                                        {
                                            ctl = kendodropdown;
                                            stp.Stop();
                                            Helper.CommonHelper.TraceLine("Total duration to find KendoUI control in milli secs: " + stp.ElapsedMilliseconds);
                                            break;
                                        }
                                    }
                                }

                                if (ctl != null) break;

                            }

                            break;
                        }

                    case "name":


                        {
                            foreach (HtmlControl kendodropdown in allkendotags)
                            {
                                for (int i = 0; i < kendodropdown.Attributes.Count; i++)
                                {
                                    if (kendodropdown.Attributes[i].Name.ToLower() == "name")
                                    {
                                        if (kendodropdown.Attributes[i].Value == searchValue)
                                        {
                                            ctl = kendodropdown;
                                            break;
                                        }
                                    }
                                }

                                if (ctl != null) break;

                            }

                            break;
                        }

                    default:
                        {
                            Helper.CommonHelper.TraceLine("Not Valid Locator: For Kendo UI Dropdown  only Name and ID are  Supported Locators  as of now  ");
                            break;
                        }


                }
            }
            catch (Exception e)
            {

                throw e;
            }
            #endregion
            return ctl;
        }


        private static HtmlControl getKendoDateinput(string searchBy, string searchValue)
        {
            HtmlControl ctl = null;
            ReadOnlyCollection<HtmlControl> allkendotags = mgr.ActiveBrowser.Find.AllByTagName<HtmlControl>("kendo-dateinput");
            Helper.CommonHelper.TraceLine("Inside KendoUI collection date  picker count as " + allkendotags.Count);

            #region SearchCrieria
            try
            {

                switch (searchBy.ToLower())

                {
                    case "id":
                        {

                            foreach (HtmlControl kendodropdown in allkendotags)
                            {
                                for (int i = 0; i < kendodropdown.Attributes.Count; i++)
                                {
                                    if (kendodropdown.Attributes[i].Name.ToLower() == "id")
                                    {
                                        if (kendodropdown.Attributes[i].Value == searchValue)
                                        {
                                            ctl = kendodropdown;
                                            break;
                                        }
                                    }
                                }

                                if (ctl != null) break;

                            }

                            break;
                        }

                    case "name":


                        {
                            foreach (HtmlControl kendodropdown in allkendotags)
                            {
                                for (int i = 0; i < kendodropdown.Attributes.Count; i++)
                                {
                                    if (kendodropdown.Attributes[i].Name.ToLower() == "name")
                                    {
                                        Helper.CommonHelper.TraceLine("Obtained Name Attribute " + allkendotags.Count);
                                        if (kendodropdown.Attributes[i].Value == searchValue)
                                        {
                                            ctl = kendodropdown;
                                            break;
                                        }
                                    }
                                }

                                if (ctl != null) break;

                            }

                            break;
                        }


                    case "attribute":


                        {
                            string[] argsarr = searchValue.Split(new char[] { '=' });
                            foreach (HtmlControl kendodropdown in allkendotags)
                            {
                                for (int i = 0; i < kendodropdown.Attributes.Count; i++)
                                {
                                    if (kendodropdown.Attributes[i].Name.ToLower() == argsarr[0])
                                    {
                                        Helper.CommonHelper.TraceLine("Obtained Name Attribute " + allkendotags.Count);
                                        if (kendodropdown.Attributes[i].Value == argsarr[1])
                                        {
                                            ctl = kendodropdown;
                                            break;
                                        }
                                    }
                                }

                                if (ctl != null) break;

                            }

                            break;
                        }

                    default:
                        {
                            Helper.CommonHelper.TraceLine("Not Valid Locator: For Kendo UI Dropdown  only Name and ID are  Supported Locators  as of now  ");
                            break;
                        }


                }
            }
            catch (Exception e)
            {

                throw e;
            }
            #endregion
            return ctl;
        }
        #endregion
        private static ReadOnlyCollection<Element> getElementSearchValueCollection(string searchBy, string searchValue)
        {
            ReadOnlyCollection<Element> elemlist = null;
            #region SearchCrieria
            switch (searchBy.ToLower())
            {
                case "tagname":
                    {
                        try
                        {
                            elemlist = mgr.ActiveBrowser.Find.AllByTagName(searchValue);
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }
                        break;
                    }

                case "content":
                    {
                        try
                        {
                            elemlist = mgr.ActiveBrowser.Find.AllByContent(searchValue);
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }
                        break;
                    }
                case "xpath":
                    {
                        try
                        {
                            elemlist = mgr.ActiveBrowser.Find.AllByXPath(searchValue);
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }
                        break;
                    }
                case "name":
                    {
                        try
                        {
                            elemlist = mgr.ActiveBrowser.Find.AllByExpression(searchValue);
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }
                        break;
                    }
                default:
                    {
                        Helper.CommonHelper.TraceLine("The search value provided has to be one of tagname xpath name for readonly collection");
                        break;

                    }

                    #endregion
            }
            return elemlist;
        }

        private static Element getElementSearchValueCollection(string searchBy, string searchValue, int indx)
        {
            Element _el = null;
            #region SearchCrieria
            switch (searchBy.ToLower())
            {


                case "content":
                    {
                        try
                        {
                            // not implemented
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }
                        break;
                    }
                case "xpath":
                    {
                        try
                        {
                            mgr.ActiveBrowser.RefreshDomTree();
                            ReadOnlyCollection<Element> allels = mgr.ActiveBrowser.Find.AllByXPath(searchValue);
                            Helper.CommonHelper.TraceLine(string.Format("Collection for objects: [count] {0} for SearchValue {1}", allels.Count, searchValue));
                            List<Element> allelsvisible = new List<Element>();
                            foreach (Element el in allels)
                            {
                                HtmlControl ctl = new HtmlControl(el);
                                //  ClientAutomation.Helper.CommonHelper.TraceLine("Is Visilble:::?" + ctl.IsVisible());
                                if (ctl.IsVisible())
                                {
                                    // ClientAutomation.Helper.CommonHelper.TraceLine("Contol is Visilble");
                                    allelsvisible.Add(el);
                                    Helper.CommonHelper.TraceLine("Control added in collection:");
                                }
                            }

                            _el = allelsvisible[indx];
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }
                        break;
                    }
                case "name":
                    {
                        try
                        {
                            _el = mgr.ActiveBrowser.Find.ByName(searchValue);
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }
                        break;
                    }
                default:
                    {
                        break;

                    }

                    #endregion
            }
            return _el;
        }


        /// <summary>
        /// Use this Method only when you see the autocomplete input controls have ID's given to it
        /// </summary>
        /// <param name="attr">For attribute in control label</param>
        /// <param name="value">Value to be typed and then selected </param>
        /// <param name="eleminfo">Elementfiled name appearing on UI</param>
        public static void SetAutoCompleteSelectwithForAttribute(string attr, string value, string eleminfo)
        {

            HtmlControl ctl = GetElement("htmlcontrolparentnextsibling", "attribute", "for=" + attr, eleminfo);
            Sendkeys(ctl, value);
            // ctl = GetElement("HtmlListItem", "Content", value, eleminfo);
            // ctl.Click();
            //****Click was working earlier but not working on Modify Dialog hence this workaround****
            Keyboard_Send(Keys.Down);
            Thread.Sleep(1000);
            Keyboard_Send(Keys.Enter);


        }


        public static void SetRadioButtonwithText(HtmlControl ctl, string txtradio)
        {
            // ctl.Find.ByContent<HtmlControl>(txtradio).Click();
            Element radctl = ctl.Find.ByContent<HtmlControl>(txtradio).BaseElement.GetPreviousSibling();
            HtmlInputRadioButton rctl = new HtmlInputRadioButton(radctl);
            rctl.Check(true);

            Helper.CommonHelper.TraceLine("Set Radio Button with option" + txtradio);
        }
        private static string getStringLastCharRemoved(string instr, char c)
        {
            int lastind = instr.LastIndexOf(",");
            int lenstr = instr.Length;
            string ostr = instr.Substring(0, (instr.LastIndexOf(c)));
            return ostr;
        }


        public static HtmlTable getTableByColumnName(string colname)
        {
            HtmlTable tbl = null;
            try
            {
                //does this loop really get different search returns, or do we need to pull in the DOM refresh and Find.All into the loop...???  -timc
                mgr.ActiveBrowser.RefreshDomTree();
                ReadOnlyCollection<HtmlTable> allHtmlTables = mgr.ActiveBrowser.Find.AllByExpression<HtmlTable>("tagname=table");
                for (int i = 0; i < globalTimeout; i++)
                {

                    Helper.CommonHelper.TraceLine("Wating for Html Table with content " + colname);
                    //if (allHtmlTables != null)
                    //{
                    //    break;
                    //}

                    //does this loop really get different search returns on this call?  -timc
                    int index = allHtmlTables.IndexOf(allHtmlTables.FirstOrDefault(x => x.InnerText.Contains(colname)));
                    if (index != -1)
                    {
                        tbl = allHtmlTables[index];
                        if (tbl != null)
                        {
                            break;
                        }
                    }
                    Thread.Sleep(1000);
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

        public static int getHTMLTablesCount()
        {
            int rowscount = 0;
            mgr.ActiveBrowser.RefreshDomTree();
            ReadOnlyCollection<HtmlTable> allHtmlTables = mgr.ActiveBrowser.Find.AllByExpression<HtmlTable>("tagname=table");
            HtmlTable tbl = allHtmlTables.FirstOrDefault(t => t.InnerText.Contains("No records available."));
            if (tbl == null)
            {
                Helper.CommonHelper.TraceLine("Table with Innertext =No records available. was NOT found meaning there are more tahn 1 Welltest ");
                rowscount = allHtmlTables[3].Rows.Count;
            }
            else
            {
                Helper.CommonHelper.TraceLine("Table with Innertext =No records available. was found meaning there ar enot well tests ");
            }
            return rowscount;
        }


        public static string GetCellValue(string columnName, string colvalstring, int rowindex)
        {
            HtmlTable colnametable = getTableByColumnName(columnName);
            int colindex = getIndexofColumnName(colnametable, columnName);
            HtmlTable colvatable = getTableByColumnName(colvalstring);
            string cellval = getTextAtIndex(colvatable, colindex, rowindex);
            return cellval;
        }

        public static int getIndexofColumnName(HtmlTable tbl, string colname)
        {
            int index = -1;
            mgr.ActiveBrowser.RefreshDomTree();
            if (colname.Contains("|"))
            {
                string[] arrcolvals = colname.Split(new char[] { '|' });
                index = 0;
                foreach (HtmlTableCell indcell in tbl.AllRows[0].Cells)
                {
                    if (indcell.InnerText.Contains(arrcolvals[0]) && indcell.InnerText.Contains(arrcolvals[1]))
                    {
                        break;
                    }
                    index++;
                }
            }
            else
            {

                index = tbl.AllRows[0].Cells.IndexOf(tbl.AllRows[0].Cells.FirstOrDefault(x => x.InnerText.Contains(colname)));
            }
            return index;
        }
        public static List<HtmlControl> CollectionOfControls(string ctrlType, string searchBy, string value, string eleminfo)
        {
            mgr.ActiveBrowser.RefreshDomTree();
            Thread.Sleep(1000);
            List<HtmlControl> TypeItem = new List<HtmlControl>();
            IEnumerable<HtmlControl> allistitems = null;

            switch (ctrlType.ToLower())
            {
                case "htmlspan":
                    allistitems = mgr.ActiveBrowser.Find.AllControls<HtmlSpan>();

                    break;

                case "htmldiv":
                    allistitems = mgr.ActiveBrowser.Find.AllControls<HtmlDiv>();

                    break;
            }

            switch (searchBy.ToLower())
            {
                case "classname":
                    foreach (HtmlControl indlistitem in allistitems)
                    {
                        //  ClientAutomation.Helper.CommonHelper.TraceLine("Html controls are" + indlistitem.InnerText);
                        if (indlistitem.BaseElement.CssClassAttributeValue.Contains(value))
                        {
                            TypeItem.Add(indlistitem);

                        }
                    }
                    break;
                case "text":
                    foreach (HtmlControl indlistitem in allistitems)
                    {
                        //  ClientAutomation.Helper.CommonHelper.TraceLine("Html controls are" + indlistitem.InnerText);
                        if (indlistitem.BaseElement.InnerText.Contains(value))
                        {
                            TypeItem.Add(indlistitem);
                        }
                    }
                    break;
            }

            if (TypeItem != null)
            {
                Helper.CommonHelper.TraceLine("Obtained Test Object Collection  ==> " + eleminfo);
            }
            return TypeItem;
        }

        static string getTextAtIndex(HtmlTable tbl, int index, int rowPos = 0, bool clickrow = false)
        {
            mgr.ActiveBrowser.RefreshDomTree();
            // ReadOnlyCollection<HtmlTableCell> allHtmlTableCells = tbl.Find.AllByExpression<HtmlTableCell>("tagname=td");
            if (clickrow == true)
            {
                tbl.AllRows[rowPos].Cells[index].Click();
                tbl.AllRows[rowPos].Cells[index].MouseClick();
            }
            return tbl.AllRows[rowPos].Cells[index].InnerText;
            // return allHtmlTableCells[index].InnerText;

        }

        static string[] getTextsAtIndex(HtmlTable tbl, int index, int rowPos = 0)
        {
            mgr.ActiveBrowser.RefreshDomTree();
            string tblvaltext = string.Empty;
            string tbluomtext = string.Empty;
            List<string> cellvals = new List<string>();
            // ReadOnlyCollection<HtmlTableCell> allHtmlTableCells = tbl.Find.AllByExpression<HtmlTableCell>("tagname=td");
            if (tbl.AllRows[rowPos].Cells[index].BaseElement.Children.Count > 0)
            {
                if (tbl.AllRows[rowPos].Cells[index].BaseElement.Children.Count == 1)
                {
                    tblvaltext = tbl.AllRows[rowPos].Cells[index].BaseElement.InnerText;
                    tbluomtext = GetChildrenControl(tbl.AllRows[rowPos].Cells[index], "0").BaseElement.InnerText;
                    tblvaltext = tblvaltext.Replace(tbluomtext, "");
                    if (tblvaltext.Length == 0)
                    {
                        //handle Inconsistent Html  rendering I assume if there is single Child element with text it has to be the only text 
                        tblvaltext = tbluomtext;
                        cellvals.Add(tblvaltext);
                    }
                    else
                    {
                        cellvals.Add(tblvaltext);
                        cellvals.Add(tbluomtext);
                    }
                }
                else if (tbl.AllRows[rowPos].Cells[index].BaseElement.Children.Count == 2)
                {
                    tblvaltext = GetChildrenControl(tbl.AllRows[rowPos].Cells[index], "0").BaseElement.InnerText;
                    tbluomtext = GetChildrenControl(tbl.AllRows[rowPos].Cells[index], "1").BaseElement.InnerText;
                    cellvals.Add(tblvaltext);
                    cellvals.Add(tbluomtext);
                }
            }
            else
            {
                tblvaltext = tbl.AllRows[rowPos].Cells[index].BaseElement.InnerText;
                cellvals.Add(tblvaltext);
            }

            return cellvals.ToArray();
            // return allHtmlTableCells[index].InnerText;

        }

        public static bool GetRadioSelection(HtmlControl radio, string spanstext)
        {
            ReadOnlyCollection<Element> radiolables = radio.BaseElement.Children[0].Children;

            Element radioElem = null;
            foreach (Element lbl in radiolables)
            {
                ReadOnlyCollection<Element> lblchildren = lbl.Children;
                foreach (Element child in lblchildren)
                {
                    if ((child.InnerText.Contains(spanstext)))
                    {
                        radioElem = child;
                        break;
                    }
                }
            }

            return new HtmlInputRadioButton(radioElem.GetPreviousSibling()).Checked;
        }



        static void editCellTextAtIndex(HtmlTable tbl, int index, string valtoedit, int rowindex = 0)
        {
            mgr.ActiveBrowser.RefreshDomTree();
            tbl.AllRows[rowindex].Cells[index].Click();
            tbl.AllRows[rowindex].Cells[index].MouseClick();

            //For handling of Date since typing the date does not accout into char '/ 'but for verifying it is needed.
            if (valtoedit.Contains("/")) //should be date Time only 
            {
                valtoedit = valtoedit.Replace(" ", "");
                valtoedit = valtoedit.Replace("/", "");
                valtoedit = valtoedit.Replace(":", "");
                Keyboard_Send(Keys.Home);
                Thread.Sleep(2000);
                Keyboard_Send(Keys.End);
                Thread.Sleep(2000);
                Keyboard_Send(Keys.Home);
                Thread.Sleep(2000);
                TypeText(valtoedit);
            }
            else if (valtoedit.Contains("|KendoDD|"))
            {
                Thread.Sleep(1000);
                valtoedit = valtoedit.Replace("|KendoDD|", "");
                Select_KendoUI_Listitem(valtoedit);

            }
            else
            {

                Sendkeys(tbl.AllRows[rowindex].Cells[index], valtoedit);
                // Keyboard_Send(Keys.Tab);

            }

        }

        public static void verifyGridCellValues(string colnames, string colvals, int rowindex = 0, string tblid = "none", bool clickrow = false)
        {
            try
            {
                string[] colnamearray = colnames.Split(new char[] { ';' });
                string[] colvalarray = colvals.Split(new char[] { ';' });

                int colpos = -1;
                HtmlTable colNameTable = null;
                //get required column header table group
                if (tblid.Equals("none"))
                {
                    colNameTable = getTableByColumnName(colnamearray[0]);
                }
                else
                {
                    colNameTable = getTableByColumnName(tblid);
                }
                HtmlTable colValTable = null;
                for (int i = 0; i < colvalarray.Length; i++)
                {
                    if (colvalarray[i].ToLower() == "ignorevalue")
                    {
                        continue;
                    }
                    colValTable = getTableByColumnName(colvalarray[i]);
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
                            double precision = 0.00000000;
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

            }
            catch (Exception e)
            {

                Helper.CommonHelper.PrintScreen("WelltestTableMetricVerification");
                Helper.CommonHelper.TraceLine("*******Verify Cell Values Failed********");
                Assert.Fail("Expcetion from VerifyGridCellValues: --->" + e.ToString());
            }

        }

        public static void verifyGridCellValuesSingleTable(string colnames, string colvals, int rowindex = 0, string tblid = "none")
        {
            try
            {
                string[] colnamearray = colnames.Split(new char[] { ';' });
                string[] colvalarray = colvals.Split(new char[] { ';' });
                HtmlTable colNameTable = null;
                int colpos = -1;
                //get required column header table group
                if (tblid.Equals("none"))
                {
                    colNameTable = getTableByColumnName(colnamearray[0]);
                }
                else
                {
                    colNameTable = getTableByColumnName(tblid);
                }

                // get all column indexes for passed column names
                StringBuilder indexstring = new StringBuilder();
                foreach (string colname in colnamearray)
                {
                    indexstring.Append(getIndexofColumnName(colNameTable, colname) + ";");
                }
                string strcolHeaderIndex = indexstring.ToString();
                Helper.CommonHelper.TraceLine(strcolHeaderIndex);
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
                        string[] actualcellvalues = getTextsAtIndex(colNameTable, colpos, rowindex + 1);
                        int i = 0;
                        foreach (string actualcellvalue in actualcellvalues)
                        {
                            string[] expcellarr = colvalarray[colpos].Split(new char[] { '|' });

                            Helper.CommonHelper.TraceLine(string.Format("******Table Cell Value Expected: {0} and Actual value {1} ,at Row Position: {2} ***********", expcellarr[i], actualcellvalue, rowindex));
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
                                double expresult = Convert.ToDouble(expcellarr[i]);
                                double precision = 0.00000000;
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
                                        Assert.AreEqual(expcellarr[i].Trim(), "", "Table Cell Value did not match for " + colnametoget);
                                    }
                                    else
                                    {
                                        Assert.AreEqual(expcellarr[i].Trim(), actualcellvalue.Trim(), "Table Cell Value did not match for " + colnametoget);
                                    }

                                }
                                else
                                {
                                    expcellarr[i] = "Blank";
                                    Assert.AreEqual(expcellarr[i], "Blank", "Table Cell Value did not match for " + colnametoget);
                                }
                            }
                            i++;
                        }
                    }
                    else
                    {
                        Helper.CommonHelper.TraceLine("Column Name: " + colnamearray[colpos] + " -- ignored for Comparison");
                    }
                }


            }
            catch (Exception e)
            {

                Helper.CommonHelper.PrintScreen("Table verification");
                Helper.CommonHelper.TraceLine("*******Verify Cell Values Failed********");
                Helper.CommonHelper.TraceLine(e.ToString());
                Assert.Fail("Expcetion from VerifyGridCellValuesSingleTable: --->" + e.ToString());
            }

        }
        public static void verifyValues(string expvalue, string actualcellvalue)
        {
            try
            {
                double dout = 0.0;
                bool IsNumber = double.TryParse(actualcellvalue, out dout);
                if (IsNumber)
                {
                    double expresult = Convert.ToDouble(expvalue);
                    double precision = 0.00000000;
                    int actuuldecimalprecision = CountDigitsAfterDecimal(dout);
                    expresult = Math.Round(expresult, actuuldecimalprecision);
                    if (IsDecimalTruncate(expresult, dout))
                    {
                        Helper.CommonHelper.TraceLine("Truncated Actual Value");
                        //Make them same as this is due to wrong deimal Truncation instead of rounding
                        expresult = Math.Truncate(dout * Math.Pow(10, actuuldecimalprecision)) / Math.Pow(10, actuuldecimalprecision);
                    }
                    Assert.AreEqual(expresult, dout, precision, " Precision Check Falied for Numeric Value " + expvalue);
                    Helper.CommonHelper.TraceLine(string.Format("*** Result:Pass ****** Expected Value: {0} , Actual Value: {1}", expvalue, actualcellvalue));
                }
                else
                {
                    if (actualcellvalue != null)
                    {
                        if (actualcellvalue == "null")
                        {
                            actualcellvalue = "";
                        }
                        Assert.AreEqual(expvalue.Trim(), actualcellvalue.Trim(), "Value did not match for: " + expvalue);
                        Helper.CommonHelper.TraceLine(string.Format("*** Result:Pass ****** Expected Value: {0} , Actual Value: {1}", expvalue, actualcellvalue));

                    }
                    else
                    {
                        actualcellvalue = "Blank";
                        expvalue = "Blank";
                        Assert.AreEqual(expvalue, actualcellvalue, "Table Cell Value did not match for " + expvalue);
                        Helper.CommonHelper.TraceLine(string.Format("*** Result:Pass ****** Expected Value: {0} , Actual Value: {1}", expvalue, actualcellvalue));
                    }
                }
            }
            catch (Exception e)
            {
                Assert.Fail("Exception from VerifyValues: --->" + e.ToString());
                Helper.CommonHelper.TraceLine("Error: " + e.ToString());
            }

        }

        public static void verifyValues(string expvalue, string actualcellvalue, string fieldName, bool casesensitive = true)
        {
            Helper.CommonHelper.TraceLine(string.Format("***Verifyimng the FieldName **** {0}", fieldName));
            try
            {
                double dout = 0.0;
                bool IsNumber = double.TryParse(actualcellvalue, out dout);
                if (IsNumber)
                {
                    double expresult = Convert.ToDouble(expvalue);
                    double precision = 0.00000000;
                    int actuuldecimalprecision = CountDigitsAfterDecimal(dout);
                    expresult = Math.Round(expresult, actuuldecimalprecision);
                    if (IsDecimalTruncate(expresult, dout))
                    {
                        Helper.CommonHelper.TraceLine("Truncated Actual Value");
                        //Make them same as this is due to wrong deimal Truncation instead of rounding
                        expresult = Math.Truncate(dout * Math.Pow(10, actuuldecimalprecision)) / Math.Pow(10, actuuldecimalprecision);
                    }

                    Assert.AreEqual(expresult, dout, precision, " Precision Check Falied for Numeric Value " + expvalue);
                    Helper.CommonHelper.TraceLine(string.Format("*** FieldName: " + fieldName + " Result:Pass ****** Expected Value:  {0} ,  Actual Value: {1} ", expvalue, actualcellvalue));
                }
                else
                {
                    if (actualcellvalue != null)
                    {
                        if (actualcellvalue == "null")
                        {
                            actualcellvalue = "";
                        }
                        if (casesensitive)
                        {

                            Assert.AreEqual(expvalue.Trim(), actualcellvalue.Trim(), "Value did not match for: " + expvalue);
                        }
                        else
                        {
                            Assert.AreEqual(expvalue.Trim().ToUpper(), actualcellvalue.Trim().ToUpper(), "Value did not match for: " + expvalue);
                        }
                        Helper.CommonHelper.TraceLine(string.Format("*** Result:Pass ****** Expected Value: {0} ,  Actual Value: {1} ", expvalue, actualcellvalue));

                    }
                    else
                    {
                        actualcellvalue = "Blank";
                        expvalue = "Blank";
                        Assert.AreEqual(expvalue, actualcellvalue, "Table Cell Value did not match for " + expvalue);
                        Helper.CommonHelper.TraceLine(string.Format("*** Result:Pass ****** Expected Value: {0} , Actual Value: {1}", expvalue, actualcellvalue));
                    }
                }
            }
            catch (Exception e)
            {
                Assert.Fail("Exception from VerifyValues: --->" + e.ToString());
                Helper.CommonHelper.TraceLine("Error: " + e.ToString());
            }

        }
        public static void verfiyRRLTables(string fieldName, string expvalue)
        {
            //RRL td is 2 spans

            bool checkforValue = true;
            HtmlControl fieldLabel = GetElement("htmlcontrol", "content", fieldName, "Field Label to Look for: -- " + fieldName, true);
            if (fieldLabel == null)
            {
                //This issue was happeningonly on Dev Environment and not on ATS 
                //Issue was Report button was toggling in between verifiication and disappaering causing null ref exception
                //So this check is introdcued if everything is fine this block should not even execute 
                Helper.CommonHelper.TraceLine("Report Button Got toggled by itslef Magic !!!");
                PageAnalysis.btnanalysisReportBtn.MouseClick();
                Thread.Sleep(1000);
                fieldLabel = GetElement("htmlcontrol", "content", fieldName, "Field Label to Look for: -- " + fieldName);

            }


            Assert.IsNotNull(fieldLabel, "The Label to look for is Null Expected Label" + fieldName);
            if (expvalue.Contains("IgnoreValue"))
            {
                checkforValue = false;
            }
            if (checkforValue)
            {
                HtmlControl cellvaluetext = GetElement("htmlcontrolnextsibling", "content", fieldName, " Container Element having values  for supplied Label: -- " + fieldName);
                if (cellvaluetext != null)
                {
                    string[] arrexpvalue = expvalue.Split(new char[] { ';' });
                    string val = string.Empty;
                    string uom = string.Empty;
                    if (arrexpvalue.Length == 2)
                    {
                        HtmlControl valuecontainter = GetChildrenControl(cellvaluetext, "0");
                        HtmlControl uomcontainter = GetChildrenControl(cellvaluetext, "1");
                        val = valuecontainter.BaseElement.InnerText;
                        uom = uomcontainter.BaseElement.InnerText;
                        //ClientAutomation.Helper.CommonHelper.TraceLine("Value text is :" + val);
                        //ClientAutomation.Helper.CommonHelper.TraceLine("UOM text is :" + uom);
                        verifyValues(arrexpvalue[0], val);
                        verifyValues(arrexpvalue[1], uom);

                    }
                    else if (arrexpvalue.Length == 4)
                    {
                        HtmlControl valuecontainter = GetChildrenControl(cellvaluetext, "0");
                        HtmlControl uomcontainter = null;
                        if (cellvaluetext.BaseElement.Children.Count > 2)
                        {
                            uomcontainter = GetChildrenControl(cellvaluetext, "1");
                            uom = uomcontainter.BaseElement.InnerText;
                        }
                        val = valuecontainter.BaseElement.InnerText;

                        //ClientAutomation.Helper.CommonHelper.TraceLine("Value text is :" + val);
                        //ClientAutomation.Helper.CommonHelper.TraceLine("UOM text is :" + uom);

                        HtmlControl cellvaluetext1 = GetNextSibling(cellvaluetext, 1);
                        valuecontainter = GetChildrenControl(cellvaluetext1, "0");
                        if (cellvaluetext1.BaseElement.Children.Count > 2)
                        {
                            uomcontainter = GetChildrenControl(cellvaluetext1, "1");
                            string adduom = uomcontainter.BaseElement.InnerText;
                            verifyValues(arrexpvalue[3], adduom);
                        }
                        verifyValues(arrexpvalue[0], val);
                        verifyValues(arrexpvalue[1], uom);
                        string addval = valuecontainter.BaseElement.InnerText;

                        verifyValues(arrexpvalue[2], addval);

                    }
                    else
                    {
                        HtmlControl valuecontainter = GetChildrenControl(cellvaluetext, "0");
                        if (valuecontainter == null)
                        {
                            valuecontainter = cellvaluetext;
                        }
                        val = valuecontainter.BaseElement.InnerText;
                        //ClientAutomation.Helper.CommonHelper.TraceLine("Value text is :" + val);
                        verifyValues(arrexpvalue[0], val);
                    }
                }
            }


        }
        public static void editGridCells(string colnames, string origcolvals, string editcolvals, int rownum = 0)
        {
            string[] colnamearray = colnames.Split(new char[] { ';' });
            string[] origcolvalarray = origcolvals.Split(new char[] { ';' });
            string[] editcolvalarray = editcolvals.Split(new char[] { ';' });
            int colpos = -1;
            HtmlTable colNameTable = getTableByColumnName(colnamearray[0]);
            if (colNameTable == null)
            {
                Helper.CommonHelper.TraceLine(string.Format("Table was not found using columnname:  {0} as innertext ", colnamearray[0]));
            }
            HtmlTable colValTable = null;
            for (int i = 0; i < origcolvalarray.Length; i++)
            {
                Helper.CommonHelper.TraceLine("Searching for Value table using " + origcolvalarray[i]);
                if (origcolvalarray[i].ToLower() == "ignorevalue")
                {
                    continue;
                }
                colValTable = getTableByColumnName(origcolvalarray[i]);
                if (colValTable != null)
                {
                    break;
                }
            }
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
                if (editcolvalarray[colpos].ToLower() != "ignorevalue")
                {

                    Helper.CommonHelper.TraceLine("Table Column Name to be edited: " + colnamearray[colpos]);
                    editCellTextAtIndex(colValTable, colpos, editcolvalarray[colpos], rownum);
                    Helper.CommonHelper.TraceLine("Table Column Name was  edited: " + colnamearray[colpos]);

                }
                else
                {
                    Helper.CommonHelper.TraceLine("Column Name: " + colnamearray[colpos] + " -- ignored for Cell Edit");
                }
            }
        }
        #endregion

        #region ActionUtils
        public static void AutoDomrefresh()
        {
            mgr.ActiveBrowser.RefreshDomTree();
        }

        /// <summary>
        /// Method will  be used to Select value from KendoUI Dropdown (Check Control from Developer toolbar or Firebug)
        /// </summary>
        /// <param name="value">ListIem Value to Select</param>
        /// <param name="scroll">Whether to Scrol or not Default is always true </param>
        public static void Select_KendoUI_Listitem(string value, bool scroll = true)
        {
            mgr.ActiveBrowser.RefreshDomTree();
            Thread.Sleep(1000);
            IEnumerable<HtmlListItem> allistitems = mgr.ActiveBrowser.Find.AllControls<HtmlListItem>();
            HtmlListItem TypeItem = null;
            foreach (HtmlListItem indlistitem in allistitems)
            {
                //  ClientAutomation.Helper.CommonHelper.TraceLine("Html controls are" + indlistitem.InnerText);
                if (indlistitem.InnerText == value)
                {
                    TypeItem = indlistitem;
                    Helper.CommonHelper.TraceLine("Surely ListItem is not Null");
                    break;
                }
            }
            //  NonTelerikHtml.HtmlListItem TypeItem = mgr.ActiveBrowser.Find.ByContent<NonTelerikHtml.HtmlListItem>(value);
            Assert.IsNotNull(TypeItem, value + ": is not available inside the dropdown");
            if (scroll)
            {
                Mouse_Wheel_Scroll(50);
                TypeItem.ScrollToVisible();
                Thread.Sleep(1000);
            }
            TypeItem.MouseHover();
            // TypeItem.MouseClick();
            Click(TypeItem);
            Helper.CommonHelper.TraceLine(value + " is set");
        }
        public static void Select_HtmlSelect(HtmlSelect ctl, string val, bool usepartial = true, bool mouseclick = true)
        {
            ctl.Click();
            if (usepartial)
            {
                ctl.SelectByPartialText(val, true);
            }
            else
            {
                ctl.SelectByText(val, true);
            }
            if (mouseclick)
            {

                ctl.SelectedOption.MouseClick();
            }
            Helper.CommonHelper.TraceLine("Selected Value :  " + val);
        }
        private static bool isWellPresentInDB(string wellname)
        {
            Thread.Sleep(1000);
            mgr.ActiveBrowser.RefreshDomTree();
            Thread.Sleep(1000);
            string searchValue = "//ul/li[contains(text(),'" + wellname + "')]";
            ReadOnlyCollection<Element> allels = mgr.ActiveBrowser.Find.AllByXPath(searchValue);
            Helper.CommonHelper.TraceLine("Collection for objects: [count] " + allels.Count);
            if (allels.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        public static void ScrollElement(int x, int y)
        {
            mgr.ActiveBrowser.ScrollBy(x, y);
        }
        /// <summary>
        /// Method Simply Performs LeftClick using Mouse Operation and logs action 
        /// </summary>
        /// <param name="el"> Element To Click</param>
        public static void Click(Element el, bool usemouse = true)
        {

            if (el != null)
            {
                try
                {
                    HtmlControl ctl = new HtmlControl(el);
                    if (usemouse)

                    {

                        mgr.Desktop.Mouse.Click(MouseClickType.LeftClick, ctl.GetRectangle());
                    }
                    else
                    {
                        ctl.MouseClick();
                        //    mgr.ActiveBrowser.Actions.AnnotateElement
                        //     mgr.ActiveBrowser.Actions.AnnotateElement(el, "This link");


                    }
                    Helper.CommonHelper.TraceLine("Performed Click operation on |^ above control | ");
                }
                catch (Exception e)
                {
                    Helper.CommonHelper.TraceLine("Exception was Encountered while doing Click " + e.ToString());
                    throw e;
                }
            }
            else
            {
                Helper.CommonHelper.TraceLine("Unlable to locate the object using locators provided. Please ensure that locators have no changes by looking at DOM ");
            }

        }

        /// <summary>
        /// Method Simply Performs LeftClick using Mouse Operation and logs action
        /// </summary>
        /// <param name="el">Telerik HtmlControl to click</param>
        public static void Click(HtmlControl el, bool usemouse = true, bool scroll = false)
        {
            try
            {
                if (usemouse)
                {
                    if (scroll)
                    {
                        el.ScrollToVisible();
                    }
                    mgr.Desktop.Mouse.Click(MouseClickType.LeftClick, el.GetRectangle());
                }
                else
                {
                    el.Click();
                }
                Helper.CommonHelper.TraceLine("Performed click operation  on ^above control: ");
            }
            catch (Exception e)
            {
                Helper.CommonHelper.TraceLine("Exception was Encountered while doing Click ");
                throw e;
            }

        }

        public static void Mouse_Click(Element el)
        {
            try
            {
                HtmlControl ctl = new HtmlControl(el);
                waitForEnabled(ctl);
                mgr.Desktop.Mouse.Click(MouseClickType.LeftClick, ctl.GetRectangle());
            }
            catch (Exception e)
            {
                throw e;
            }

        }

        public static void Mouse_Wheel_Scroll(int x)
        {
            try
            {

                mgr.Desktop.Mouse.TurnWheel(x, MouseWheelTurnDirection.Backward);
            }
            catch (Exception e)
            {
                throw e;
            }

        }
        /// <summary>
        /// This method is used to wait for 25% time of globalTimeout
        /// </summary>
        public static void DoStaticWait()
        {

            double stwait = Convert.ToDouble(globalTimeout) * 1000 * 1;
            Helper.CommonHelper.TraceLine("******** Doing static wait for ******" + stwait / 1000 + " secs");
            Thread.Sleep((int)stwait);
        }
        public static void Keyboard_Send(System.Windows.Forms.Keys k)
        {
            mgr.Desktop.KeyBoard.KeyPress(k);
        }
        public static void TypeText(String texttotype, int typespeed = 100)
        {
            //added for Win 10 machine execution 
            SendKeys.SendWait("{Home}");
            mgr.Desktop.KeyBoard.TypeText(texttotype, typespeed);
            //  System.Windows.Forms.SendKeys.SendWait(texttotype);
        }
        /// <summary>
        /// Method Simply Sends Keys or Types  to control using Keyboar doperation
        /// </summary>
        /// <param name="el">Element or control to Type Value into</param>
        /// <param name="strval">Value to Type in Control</param>
        /// <param name="cleartext">Flag wheter to clear values in control whic are already present.Special Note It will be always true But If you Entering Date in Calendar TextBox Set it to False
        /// </param>
        public static void Sendkeys(Element el, string strval, bool cleartext = true)
        {
            try
            {
                HtmlControl ctl = new HtmlControl(el);
                //  waitForEnabled(ctl);
                mgr.Desktop.Mouse.Click(MouseClickType.LeftClick, ctl.GetRectangle());
                if (cleartext)
                {
                    clearTextBox();
                }
                TypeText(strval);
                Helper.CommonHelper.TraceLine("Performed Sendkeys operation: for control above | sent text=:   " + strval);

            }
            catch (Exception e)
            {
                Helper.CommonHelper.TraceLine("Exception was Encountered while Typing Value ");
                throw e;
            }
        }

        /// <summary>
        /// Method Simply Sends Keys or Types  to Telelrik HTML control using Keyboar doperation
        /// </summary>
        /// <param name="ctl">Element or control to Type Value into</param>
        /// <param name="strval">Value to Type in Control</param>
        /// <param name="cleartext">Flag wheter to clear values in control whic are already present.Special Note It will be always true But If you Entering Date in Calendar TextBox Set it to False</param>
        public static void Sendkeys(HtmlControl ctl, string strval, bool cleartext = true, int typespeed = 100)
        {

            try
            {

                mgr.Desktop.Mouse.Click(MouseClickType.LeftClick, ctl.GetRectangle());
                if (cleartext)
                {
                    clearTextBox();
                }
                TypeText(strval, typespeed);
                Helper.CommonHelper.TraceLine("Performed Sendkeys operation: for control above | sent text=:   " + strval);

            }
            catch (Exception e)
            {
                Helper.CommonHelper.TraceLine("Exception was Encountered while Typing Value ");
                throw e;
            }

        }
        /// <summary>
        /// Direclty use Teleriks SetText method to enter Value. This is Event based and does not simulate a real user behavior
        /// </summary>
        /// <param name="el">Element or control to Type Value into</param>
        /// <param name="strval">Value to Type in Control<</param>
        public static void SetValue(Element el, string strval)
        {
            try
            {
                HtmlControl ctl = new HtmlControl(el);
                waitForEnabled(ctl);
                mgr.ActiveBrowser.Actions.SetText(el, strval);
            }
            catch (Exception e)
            {
                Helper.CommonHelper.TraceLine("Exception was Encountered while setting value ");
                throw e;
            }
        }

        public static void SelectVal(Element el, string strval)
        {
            try
            {
                HtmlControl ctl = new HtmlControl(el);
                waitForEnabled(ctl);
                mgr.ActiveBrowser.Actions.SelectDropDown(el, strval);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private static void waitForEnabled(HtmlControl _ctl)
        {
            for (int i = 0; i < globalTimeout; i++)
            {
                if (_ctl.IsEnabled && _ctl.IsVisible())
                {
                    break;
                }
            }
        }

        private static void waitforVisible(Element el)
        {
            for (int i = 0; i < globalTimeout; i++)
            {
                Thread.Sleep(1000);
                HtmlControl ctl = new HtmlControl(el);
                if (ctl.IsVisible())
                {
                    Thread.Sleep(1000);
                    break;
                }
            }
        }
        /// <summary>
        /// Wait for Control on UI to completely Disappear.For example:  Loader Message 
        /// </summary>
        /// <param name="el">Element to wait to Disappear</param>
        public static void waitforInvisible(Element el)
        {
            int xcord = -1;
            HtmlControl ldr = null;
            if (el == null)
            {
                //ClientAutomation.Helper.CommonHelper.TraceLine("Element was null meaing not detected");
            }
            else
            {
                //ClientAutomation.Helper.CommonHelper.TraceLine("Element was  detected innertext ..." + el.GetAttribute("class").Value);
                ldr = new HtmlControl(el);
                xcord = ldr.GetRectangle().Location.X;
                //  ClientAutomation.Helper.CommonHelper.TraceLine("Orig X " + xcord);
            }


            for (int i = 0; i < globalTimeout; i++)
            {

                Thread.Sleep(1000);
                HtmlControl ctl = new HtmlControl(el);
                ctl.Refresh();
                // ClientAutomation.Helper.CommonHelper.TraceLine("Element reactanlge X " + ctl.GetRectangle().Location);
                if (ctl.GetRectangle().Location.X != xcord)
                {
                    Thread.Sleep(1000);
                    break;
                }
                else
                {
                    //  ClientAutomation.Helper.CommonHelper.TraceLine("Element is still Visisble on UI..." + el.GetAttribute("class").Value);
                }
            }
            //HtmlWait waitobj = new HtmlControl(el).Wait;
            //waitobj.Timeout = globalTimeout*1000;
            //waitobj.ForVisibleNot();
        }
        public static void gotoPage(string url)
        {
            //    mgr.ActiveBrowser.ClearCache(BrowserCacheType.TempFilesCache);
            //    mgr.ActiveBrowser.ClearCache(BrowserCacheType.Cookies);
            mgr.ActiveBrowser.NavigateTo(url);
            mgr.ActiveBrowser.WaitUntilReady();
            Thread.Sleep(globalTimeout);
            //      mgr.ActiveBrowser.WaitForAjax(globalTimeout);
            //       waitforVisible(Page_Dashboard.page_Loader);
            //   waitforInvisible(Page_Dashboard.page_Loader);

        }

        public static void waitUnitlReady()
        {
            Helper.CommonHelper.TraceLine("Waiting until the Browser is ready to take input");
            mgr.ActiveBrowser.WaitUntilReady();

        }
        private static void clearTextBox()
        {
            //System.Windows.Forms.SendKeys.SendWait("{Home}");
            //Thread.Sleep(1000);
            //System.Windows.Forms.SendKeys.SendWait("+{End}");
            //Thread.Sleep(1000);
            Thread.Sleep(1000);
            SendKeys.SendWait("^(a)");
            Thread.Sleep(1000);
            SendKeys.SendWait("{Del}");

        }

        /// <summary>
        /// Scroll the Page using mouse wheel direction down is default
        /// </summary>
        /// <param name="offset"> Amount to scroll by Offset</param>
        /// <param name="direction">Direction default:down use "up" to overrider</param>
        public static void scrollMouseWheelDown(int offset, string direction = "down")
        {
            if (direction == "down")
            {
                mgr.Desktop.Mouse.TurnWheel(offset, MouseWheelTurnDirection.Backward);
            }
            else
            {
                mgr.Desktop.Mouse.TurnWheel(offset, MouseWheelTurnDirection.Forward);
            }
        }

        public static void Toastercheck(HtmlControl ctl, string msg_scenario, string exp)
        {
            Assert.IsNotNull(ctl, "Toaster Message for " + msg_scenario);
            Helper.CommonHelper.TraceLine(string.Format("Toaster Message Obtained during {0}  is : {1}", msg_scenario, ctl.BaseElement.InnerText.ToString().Trim()));
            Assert.AreEqual(exp, ctl.BaseElement.InnerText.ToString().Trim(), "Toaster Message for " + msg_scenario + " is Wrong.");
        }
        public static string GetText(Element el)
        {
            return el.InnerText;
        }
        public static void WaitForControlDisappear(HtmlControl control)
        {
            Thread.Sleep(1000);
            Stopwatch st = new Stopwatch();
            st.Start();
            for (int i = 0; i < globalTimeout; i++)
            {
                Thread.Sleep(1000);
                if (control != null)
                {
                    if (control.BaseElement.GetRectangle().Size == System.Drawing.Size.Empty)
                    {
                        Helper.CommonHelper.TraceLine("Dynamic Delay while Spinner/Loader Wheel appeared in UI was: " + st.Elapsed.Seconds);
                        st.Stop();
                        break;
                    }
                }
            }
            Thread.Sleep(1000);


        }
        public static void CheckBox_Check(Element el)
        {
            try
            {
                HtmlControl ctl = new HtmlControl(el);
                waitForEnabled(ctl);
                mgr.ActiveBrowser.Actions.Check(el, true);
            }
            catch (Exception e)
            {
                throw e;
            }

        }

        public static int getWellTestRecordCount(string fulltext)
        {

            Regex re = new Regex("of.*.items");
            MatchCollection mts = re.Matches(fulltext);
            string firstretrun = mts[0].ToString();
            re = new Regex("\\d+(\\.\\d{1,2})?");
            mts = re.Matches(firstretrun);
            string secretrun = mts[0].ToString();
            return Convert.ToInt32(secretrun);

        }

        public static bool IsDecimalTruncate(double exp, double act)
        {
            bool IsDecimalTruncate = false;
            if (Math.Abs(exp - act) > 0)
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
        public static void Calendarentry()
        {
            Element testDatebutton = mgr.ActiveBrowser.Find.ByAttributes("aria-controls=sampleDate_dateview");
            KendoControls.KendoCalendar testDate = mgr.ActiveBrowser.Find.AllControls<KendoControls.KendoCalendar>().First(); //TODO make this find better
            Assert.IsNotNull(testDate, "Test Date calender not found");
            DateTime DateTime = Convert.ToDateTime(new WellTestDTO().SampleDate);
            DateTime DateTime2 = new DateTime(DateTime.Year, DateTime.Month - 1, DateTime.Day + 1); //Telerik Issue handling
            testDate.SelectDay(DateTime2.Year.ToString() + "," + DateTime2.Month.ToString() + "," + DateTime2.Day.ToString());
            mgr.ActiveBrowser.Actions.Click(testDatebutton);
            NonTelerikHtml.HtmlAnchor DateToClick = mgr.ActiveBrowser.Find.ByExpression<NonTelerikHtml.HtmlAnchor>("Title=" + DateTime.ToLongDateString());
            DateToClick.Click(); testDate.Refresh();

        }
        #endregion

        #region ExternalCalls
        public static void createwellgeneraltab()
        {
            ClientAutomation.WellConfiguration wconfig = new ClientAutomation.WellConfiguration();

            wconfig.Configuration_General_UI(mgr, WellConfigData(WellTypeId.RRL));
        }
        public static void deleteSelectedWell()
        {
            ClientAutomation.WellConfiguration wconfig = new ClientAutomation.WellConfiguration();
            wconfig.DeleteWell_UI(mgr);
        }

        public static void select_well()
        {
            string wellname = ConfigurationManager.AppSettings["Wellname"];
            Sendkeys(PageDashboard.WellSelectionDropDown, wellname);
            //Keyboard_Send(Keys.Down);
            Thread.Sleep(3000);
            //Keyboard_Send(Keys.Enter);
            PageDashboard.DynamicValue = wellname;
            Click(PageDashboard.listitems_wellname);
        }

        public static void createWellGeneralonly()
        {
            Click(PageDashboard.configurationtab);
            Click(PageDashboard.wellConfigurationtab);
            DoStaticWait();
            createwellgeneraltab();
        }
        public static WellConfigDTO WellConfigData(WellTypeId wType)
        {
            WellDTO well = WellData(wType);
            well.CommissionDate = new DateTime(2016, 4, 18);
            well.Engineer = "Engineer";
            well.Field = "Field";
            well.Foreman = "Foreman";
            well.GaugerBeat = "GaugerBeat";
            well.GeographicRegion = "GeographicRegion";
            well.Lease = "Lease";
            well.SurfaceLatitude = (decimal)32.444403;
            well.SurfaceLongitude = (decimal)-102.422178;

            ModelConfigDTO Model = new ModelConfigDTO();

            #region ModelData

            //Surface
            SurfaceConfigDTO SampleSurfaceConfig = new SurfaceConfigDTO();
            PumpingUnitManufacturerDTO Manufac = new PumpingUnitManufacturerDTO() { Name = "Lufkin" };
            PumpingUnitTypeDTO pumpingUnitType = new PumpingUnitTypeDTO() { AbbreviatedName = "C" };
            PumpingUnitDTO pumpingUnit = new PumpingUnitDTO() { Manufacturer = Manufac.Name, Type = pumpingUnitType.AbbreviatedName, Description = "C-57-109-48 L LUFKIN C57-109-48 (4246B)" };
            SampleSurfaceConfig.PumpingUnit = pumpingUnit;
            SampleSurfaceConfig.PumpingUnitType = pumpingUnitType;
            SampleSurfaceConfig.ClockwiseRotation = PumpingUnitRotationDirection.Clockwise;
            SampleSurfaceConfig.MotorAmpsDown = 120;
            SampleSurfaceConfig.MotorAmpsUp = 144;
            SampleSurfaceConfig.WristPinPosition = 3;
            SampleSurfaceConfig.ActualStrokeLength = 124.510;
            SampleSurfaceConfig.MotorType = new RRLMotorTypeDTO() { Name = "Nema B Electric" };
            SampleSurfaceConfig.MotorSize = new RRLMotorSizeDTO() { SizeInHP = 50 };
            SampleSurfaceConfig.SlipTorque = new RRLMotorSlipDTO() { Rating = 2 };
            Model.Surface = SampleSurfaceConfig;

            //Weights
            CrankWeightsConfigDTO SampleWeightsConfig = new CrankWeightsConfigDTO();
            SampleWeightsConfig.CrankId = "94110C";
            SampleWeightsConfig.Crank_1_Primary = new RRLPrimaryWeightDTO { LagId = "OORO", LeadId = "6RO" };
            SampleWeightsConfig.Crank_2_Primary = new RRLPrimaryWeightDTO { LagId = "OORO", LeadId = "6RO" };
            SampleWeightsConfig.TorqCalcMethod = TorqueCalculationMethod.Mills;
            Model.Weights = SampleWeightsConfig;

            //DownHole
            DownholeConfigDTO SampleDownholeConfig = new DownholeConfigDTO();
            SampleDownholeConfig.PumpDiameter = 3.25;
            SampleDownholeConfig.PumpDepth = 5070;
            SampleDownholeConfig.TubingID = 2.14;
            SampleDownholeConfig.TubingOD = 2.35;
            SampleDownholeConfig.TubingAnchorDepth = 3000;
            SampleDownholeConfig.CasingOD = 5.5;
            SampleDownholeConfig.CasingWeight = 15.5;
            SampleDownholeConfig.TopPerforation = 4000.0;
            SampleDownholeConfig.BottomPerforation = 4500;
            Model.Downhole = SampleDownholeConfig;

            //Rods
            RodStringConfigDTO SampleRodsConfig = new RodStringConfigDTO();
            SampleRodsConfig.TotalRodLength = 5100;
            RodTaperConfigDTO[] RodTaperArray = new RodTaperConfigDTO[3];
            SampleRodsConfig.RodTapers = RodTaperArray;
            RodTaperConfigDTO Taper1 = new RodTaperConfigDTO();
            Taper1.Grade = "D";
            Taper1.Manufacturer = "_Generic Manufacturer";
            Taper1.NumberOfRods = 56;
            Taper1.RodGuid = "0";
            Taper1.RodLength = 30.0;
            Taper1.ServiceFactor = 0.9;
            Taper1.Size = 1.0;  //Taper1.TaperLength = 1710;
            RodTaperArray[0] = Taper1;
            RodTaperConfigDTO Taper2 = new RodTaperConfigDTO();
            Taper2.Grade = "N-78";
            Taper2.Manufacturer = "Alberta Oil Tools";
            Taper2.NumberOfRods = 57;
            Taper2.RodGuid = "0";
            Taper2.RodLength = 30.0;
            Taper2.ServiceFactor = 0.9;
            Taper2.Size = 0.875; // Taper2.TaperLength = 1710;
            RodTaperArray[1] = Taper2;
            RodTaperConfigDTO Taper3 = new RodTaperConfigDTO();
            Taper3.Grade = "EL";
            Taper3.Manufacturer = "Weatherford, Inc.";
            Taper3.NumberOfRods = 56;
            Taper3.RodGuid = "0";
            Taper3.RodLength = 30.0;
            Taper3.ServiceFactor = 0.9;
            Taper3.Size = 0.75;  //Taper3.TaperLength = 1680;
            RodTaperArray[2] = Taper3;
            Model.Rods = SampleRodsConfig;

            #endregion ModelData

            WellConfigDTO wellConfigDTO = new WellConfigDTO();
            wellConfigDTO.Well = well;
            wellConfigDTO.ModelConfig = Model;

            return wellConfigDTO;
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

        public static WellDTO WellData(WellTypeId wType)
        {
            WellDTO well = null;
            switch (wType)
            {
                case WellTypeId.RRL:
                    well = new WellDTO() { Name = "RRL", FacilityId = CygNetFacility, DataConnection = new DataConnectionDTO { ProductionDomain = Domain, Site = Site, Service = Service }, AssemblyAPI = "TestAssemblyAPI_RRL", SubAssemblyAPI = "TestSubAssemblyAPI_RRL", IntervalAPI = "TestAssemblyAPI_RRL", WellType = wType };
                    break;

                case WellTypeId.ESP:
                    well = new WellDTO() { Name = "ESP", FacilityId = CygNetFacility, DataConnection = new DataConnectionDTO { ProductionDomain = Domain, Site = Site, Service = Service }, AssemblyAPI = "TestAssemblyAPI_ESP", SubAssemblyAPI = "Esp_ProductionTestData.wflx", IntervalAPI = "TestAssemblyAPI_ESP", WellType = wType };
                    break;

                case WellTypeId.GLift:
                    well = new WellDTO() { Name = "Gas Lift", FacilityId = CygNetFacility, DataConnection = new DataConnectionDTO { ProductionDomain = Domain, Site = Site, Service = Service }, AssemblyAPI = "TestAssemblyAPI_GLift", SubAssemblyAPI = "GasLift-LFactor1.wflx", IntervalAPI = "TestAssemblyAPI_GLift", WellType = wType };
                    break;

                case WellTypeId.NF:
                    well = new WellDTO() { Name = "Naturally Flowing", FacilityId = CygNetFacility, DataConnection = new DataConnectionDTO { ProductionDomain = Domain, Site = Site, Service = Service }, AssemblyAPI = "TestAssemblyAPI_NF", SubAssemblyAPI = "WellfloNFWExample1.wflx", IntervalAPI = "TestAssemblyAPI_NF", WellType = wType };
                    break;

                case WellTypeId.WInj:
                    well = new WellDTO() { Name = "Water Injection", FacilityId = CygNetFacility, DataConnection = new DataConnectionDTO { ProductionDomain = Domain, Site = Site, Service = Service }, AssemblyAPI = "TestAssemblyAPI_WInj", SubAssemblyAPI = "WellfloWaterInjectionExample1.wflx", IntervalAPI = "TestAssemblyAPI_WInj", WellType = wType };
                    break;

                case WellTypeId.GInj:
                    well = new WellDTO() { Name = "Gas Injection", FacilityId = CygNetFacility, DataConnection = new DataConnectionDTO { ProductionDomain = Domain, Site = Site, Service = Service }, AssemblyAPI = "TestAssemblyAPI_GInj", SubAssemblyAPI = "WellfloGasInjectionExample1.wflx", IntervalAPI = "TestAssemblyAPI_GInj", WellType = wType };
                    break;

                default:
                    Helper.CommonHelper.TraceLine("Well Type Not expected");
                    break;
            }

            return well;
        }
        #endregion
    }
}