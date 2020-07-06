using OpenQA.Selenium;
using SeleniumAutomation.AGGridScreenDatas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace SeleniumAutomation.SeleniumObject
{
    class GroupConfigurationDataProcess : IGridScenarios<TypeGroupConfigurationGrid>
    {
        double height;
        double width;
        public string scrollp;
        public int scrollsize = 0;
        public GroupConfigurationDataProcess()
        {
            height = SystemParameters.PrimaryScreenHeight;
            width = SystemParameters.PrimaryScreenWidth;

            Console.Write("Screen size found width:-" + width);
            if (width <= 1366)
            {
                double half = width / 2;
                scrollp = half.ToString();
            }
            if (width >= 1600)
            {
                double half = width / 2;
                scrollp = half.ToString();
            }

        }
        public TypeGroupConfigurationGrid Colorcode(string columnname, string comparison, string value, int hex, string target)
        {
            throw new NotImplementedException();
        }

        public TypeGroupConfigurationGrid CreateNewView(string viewname)
        {
            throw new NotImplementedException();
        }

        public TypeGroupConfigurationGrid[] FetchdataScreen()
        {
            throw new NotImplementedException();

        }


        public TypeGroupConfigurationGrid[] Search(string text)
        {
            throw new NotImplementedException();
        }

        public TypeGroupConfigurationGrid[] Sort(By columnsorticonlocator, string sorttype, By columnhover)
        {
            throw new NotImplementedException();
        }

        public TypeGroupConfigurationGrid[] Filter(string text, By columnmenuicon, By columnheader)
        {
            throw new NotImplementedException();
        }
    }
}
