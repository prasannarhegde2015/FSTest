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
                    // fosite.SetFocus();

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

                WpfEdit next = new WpfEdit(FSWindow);
                next.SearchProperties[WpfEdit.PropertyNames.ControlType] = "Edit";
                next.SearchProperties[WpfEdit.PropertyNames.Instance] = "0";
                return sqlserver;
            }
        }

        public static WpfEdit dbname
        {
            get
            {

                WpfEdit dbname = new WpfEdit(FSWindow);
                next.SearchProperties[WpfEdit.PropertyNames.ControlType] = "Edit";
                next.SearchProperties[WpfEdit.PropertyNames.Instance] = "1";
                return dbname;
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

                WpfButton next = new WpfButton(SqlServerConnection);
                next.SearchProperties[WpfButton.PropertyNames.ControlType] = "Button";
                next.SearchProperties[WpfButton.PropertyNames.Name] = "Ok";
                return next;
            }
        }

        public static WpfEdit licserver
        {
            get
            {

                WpfEdit dbname = new WpfEdit(FSWindow);
                next.SearchProperties[WpfEdit.PropertyNames.ControlType] = "Edit";
             //   next.SearchProperties[WpfEdit.PropertyNames.Name] = "";
                return dbname;
            }
        }

        public static WpfEdit domain
        {
            get
            {

                WpfEdit dbname = new WpfEdit(FSWindow);
                next.SearchProperties[WpfEdit.PropertyNames.ControlType] = "Text";
                next.SearchProperties[WpfEdit.PropertyNames.Instance] = "1";
                return dbname;
            }
        }
        public static WpfEdit site
        {
            get
            {

                WpfEdit dbname = new WpfEdit(FSWindow);
                next.SearchProperties[WpfEdit.PropertyNames.ControlType] = "Text";
                next.SearchProperties[WpfEdit.PropertyNames.Instance] = "2";
                return dbname;
            }
        }
        public static WpfEdit vhs
        {
            get
            {

                WpfEdit dbname = new WpfEdit(FSWindow);
                next.SearchProperties[WpfEdit.PropertyNames.ControlType] = "Edit";
                next.SearchProperties[WpfEdit.PropertyNames.Instance] = "3";
                return dbname;
            }
        }
        public static WpfEdit install
        {
            get
            {

                WpfEdit dbname = new WpfEdit(FSWindow);
                next.SearchProperties[WpfEdit.PropertyNames.ControlType] = "Button";
                next.SearchProperties[WpfEdit.PropertyNames.Name] = "Install";
                return dbname;
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
