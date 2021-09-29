using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace ForeSiteIsntall
{
    class PageObjects
    {
        
        public static WpfWindow FSWindow
        {
            get
            {
                WpfWindow fosite = new WpfWindow();
                fosite.SearchProperties[WpfWindow.PropertyNames.ControlType] = "Window";
                //fosite.SearchProperties[WpfWindow.PropertyNames.ClassName] = "Uia.Window";
                fosite.SearchProperties[WpfWindow.PropertyNames.Name] = "Weatherford ForeSite Bundle";
                try
                {
                    //fosite.DrawHighlight();
                    //fosite.SetFocus();

                }
                catch
                {

                }
                return fosite;
            }
        }

        public static WpfWindow SqlServerConnection
        {
            get
            {
                WpfWindow fosite = new WpfWindow();
                fosite.SearchProperties[WpfWindow.PropertyNames.ControlType] = "Window";
                //fosite.SearchProperties[WpfWindow.PropertyNames.ClassName] = "Uia.Window";
                fosite.SearchProperties[WpfWindow.PropertyNames.Name] = "SQL Server Connection";
                try
                {
                    fosite.DrawHighlight();
                    fosite.SetFocus();

                }
                catch
                {

                }
                return fosite;
            }
        }

        public static WpfCheckBox iagree
        {
            get
            {
                WpfCheckBox iagree = new WpfCheckBox(FSWindow);
                iagree.SearchProperties[WpfCheckBox.PropertyNames.ControlType] = "CheckBox";
                iagree.SearchProperties[WpfCheckBox.PropertyNames.Name] = "I agree to the License Terms and Conditions.";
                //iagree.SearchProperties[WpfCheckBox.PropertyNames.AutomationId] = "HeaderSite";
                iagree.TechnologyName = "UIA";
                return iagree;
            }
        }

        public static WpfEdit sname
        {
            get
            {
                UIMap map = new UIMap();
                return map.UIWeatherfordForeSiteBWindow.UIItemCustom.UIServernameText.UIItemEdit;
            }
        }
        public static WpfCheckBox foresiteclient
        {
            get
            {
                WpfCheckBox iagree = new WpfCheckBox(FSWindow);
                iagree.SearchProperties[WpfCheckBox.PropertyNames.ControlType] = "CheckBox";
                iagree.SearchProperties[WpfCheckBox.PropertyNames.Name] = "ForeSite Client";
                iagree.TechnologyName = "UIA";
                return iagree;
            }
        }

        public static WpfCheckBox Foresiteserver
        {
            get
            {
                WpfCheckBox iagree = new WpfCheckBox(FSWindow);
                iagree.SearchProperties[WpfCheckBox.PropertyNames.ControlType] = "CheckBox";
                iagree.SearchProperties[WpfCheckBox.PropertyNames.Name] = "ForeSite Server";
                //iagree.SearchProperties[WpfCheckBox.PropertyNames.AutomationId] = "HeaderSite";
                iagree.TechnologyName = "UIA";
                return iagree;
            }
        }

        public static WpfCheckBox wellmodeling
        {
            get
            {
                WpfCheckBox iagree = new WpfCheckBox(FSWindow);
                iagree.SearchProperties[WpfCheckBox.PropertyNames.ControlType] = "CheckBox";
                iagree.SearchProperties[WpfCheckBox.PropertyNames.Name] = "Well Modeling";
                //iagree.SearchProperties[WpfCheckBox.PropertyNames.AutomationId] = "HeaderSite";
                iagree.TechnologyName = "UIA";
                return iagree;
            }
        }

        public static WpfCheckBox dynacard
        {
            get
            {
                WpfCheckBox iagree = new WpfCheckBox(FSWindow);
                iagree.SearchProperties[WpfCheckBox.PropertyNames.ControlType] = "CheckBox";
                iagree.SearchProperties[WpfCheckBox.PropertyNames.Name] = "DynaCard";
                //iagree.SearchProperties[WpfCheckBox.PropertyNames.AutomationId] = "HeaderSite";
                iagree.TechnologyName = "UIA";
                return iagree;
            }
        }

        public static WpfCheckBox catalogservice
        {
            get
            {
                WpfCheckBox iagree = new WpfCheckBox(FSWindow);
                iagree.SearchProperties[WpfCheckBox.PropertyNames.ControlType] = "CheckBox";
                iagree.SearchProperties[WpfCheckBox.PropertyNames.Name] = "Catalog Service";
                //iagree.SearchProperties[WpfCheckBox.PropertyNames.AutomationId] = "HeaderSite";
                iagree.TechnologyName = "UIA";
                return iagree;
            }
        }
        public static WpfCheckBox reoserivce
        {
            get
            {
                WpfCheckBox iagree = new WpfCheckBox(FSWindow);
                iagree.SearchProperties[WpfCheckBox.PropertyNames.ControlType] = "CheckBox";
                iagree.SearchProperties[WpfCheckBox.PropertyNames.Name] = "Reo Service";
                //iagree.SearchProperties[WpfCheckBox.PropertyNames.AutomationId] = "HeaderSite";
                iagree.TechnologyName = "UIA";
                return iagree;
            }
        }

        public static WpfCheckBox reocomm
        {
            get
            {
                WpfCheckBox iagree = new WpfCheckBox(FSWindow);
                iagree.SearchProperties[WpfCheckBox.PropertyNames.ControlType] = "CheckBox";
                iagree.SearchProperties[WpfCheckBox.PropertyNames.Name] = "Reo COM";
                //iagree.SearchProperties[WpfCheckBox.PropertyNames.AutomationId] = "HeaderSite";
                iagree.TechnologyName = "UIA";
                return iagree;
            }
        }

        public static WpfButton next
        {
            get
            {
                
                WpfButton next = new WpfButton(FSWindow);
                next.SearchProperties[WpfButton.PropertyNames.ControlType] = "Button";
                next.SearchProperties[WpfButton.PropertyNames.Name] = "Next";
                return next;
            }
        }

        public static WpfEdit sqlserver
        {
            get
            {

                UIMap map = new UIMap();
                return map.UIWeatherfordForeSiteBWindow.UIItemCustom.UIServernameText.UIItemEdit;
            }
        }

        public static WpfEdit dbname
        {
            get
            {

                UIMap map = new UIMap();
                return map.UIWeatherfordForeSiteBWindow.UIItemCustom.UIServernameText.UIDBName;
            }
        }

        public static WpfButton testconnection
        {
            get
            {

                WpfButton next = new WpfButton(FSWindow);
                next.SearchProperties[WpfButton.PropertyNames.ControlType] = "Button";
                next.SearchProperties[WpfButton.PropertyNames.Name] = "Test Connection";
                return next;
            }
        }

        public static WpfButton testconnectionOK
        {
            get
            {
                UIMap map = new UIMap();
                return map.UISQLServerConnectionWindow.UIOKButton;
            }
        }

        public static WpfEdit licserver
        {
            get
            {

                UIMap map = new UIMap();
                return map.UIWeatherfordForeSiteBWindow.UIItemCustom2.UISpecifytheFlexLMliceText.UIItemEdit;
            }
        }

        public static WpfEdit domain
        {
            get
            {

                UIMap map = new UIMap();
                return map.UIWeatherfordForeSiteBWindow.UIItemCustom1.UIDomain.UIItemEdit;
            }
        }
        public static WpfEdit site
        {
            get
            {
                UIMap map = new UIMap();
                return map.UIWeatherfordForeSiteBWindow.UIItemCustom1.UISite.UIItemEdit;

               
            }
        }
        public static WpfEdit vhs
        {
            get
            {
                UIMap map = new UIMap();
                return map.UIWeatherfordForeSiteBWindow.UIItemCustom1.UIVHS.UIItemEdit;

            }
        }
        public static WpfButton install
        {
            get
            {

                UIMap map = new UIMap();
                return map.UIWeatherfordForeSiteBWindow.UIItemCustom3.UIInstallButton;
            }
        }

        public static void WaitTillClose()
        {
            Trace.WriteLine(AutomationElement.RootElement.Current.ProcessId.ToString());
            AutomationElement root = AutomationElement.RootElement;
            AutomationElement bundlwwin = null;
            
            do
            {
                AutomationElementCollection allwins = root.FindAll(TreeScope.Descendants,
                     new System.Windows.Automation.PropertyCondition(AutomationElement.ControlTypeProperty, System.Windows.Automation.ControlType.Window));

                foreach (AutomationElement indelm in allwins)
                {
                    Trace.WriteLine(indelm.Current.Name);
                    if (indelm.Current.Name.Equals("Weatherford ForeSite Bundle"))
                    {
                        bundlwwin = indelm;
                        break;
                    }
                }
                Playback.Wait(1000);
            } while (bundlwwin == null);


            AutomationElementCollection allbtns = bundlwwin.FindAll(TreeScope.Descendants,
                new System.Windows.Automation.PropertyCondition(AutomationElement.ControlTypeProperty, System.Windows.Automation.ControlType.Button));
            AutomationElement btnclose = null;
            do
            {
                allbtns = bundlwwin.FindAll(TreeScope.Descendants,
                  new System.Windows.Automation.PropertyCondition(AutomationElement.ControlTypeProperty, System.Windows.Automation.ControlType.Button));
                foreach (AutomationElement indelm in allbtns)
                {
                    Trace.WriteLine(indelm.Current.Name);
                    if (indelm.Current.Name.Equals("Close"))
                    {
                        btnclose = indelm;
                        break;
                    }
                }
                Playback.Wait(1000);
            } while (btnclose == null);
            Point p = btnclose.GetClickablePoint();
            Mouse.Click(p);
            Playback.Wait(5000);
        }
    }




}
