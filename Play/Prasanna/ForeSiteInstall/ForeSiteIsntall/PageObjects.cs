using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public static 

    }




}
