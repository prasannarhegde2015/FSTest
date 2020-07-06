using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeleniumAutomation.SeleniumObject
{
    interface IGridScenarios<T>
    {
        T[] Sort(By columnsorticonlocator, string sorttype, By columnhover);
        T[] Filter(string text, By columnmenuicon, By columnheader);
        T[] Search(string text);
        T[] FetchdataScreen();
        T CreateNewView(string viewname);
        T Colorcode(string columnname, string comparison, string value, int hex, string target);

    }
}
