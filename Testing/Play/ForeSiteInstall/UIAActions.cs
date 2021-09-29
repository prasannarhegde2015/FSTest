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
    class UAActions
    {
       public static void  waitClick(AutomationElement obj)
        {
            Point p = obj.GetClickablePoint();
            Mouse.Click(p);
        }
    }




}
