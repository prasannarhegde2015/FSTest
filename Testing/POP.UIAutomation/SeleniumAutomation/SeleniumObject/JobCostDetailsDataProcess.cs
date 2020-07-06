using Microsoft.VisualStudio.TestTools.UnitTesting;
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
    class JobCostDetailsDataProcess : IGridScenarios<TypeJobCostDetailsGrid>
    {
        double height;
        double width;
        public string scrollp;
        public int scrollsize;
        public JobCostDetailsDataProcess()
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
        public TypeJobCostDetailsGrid Colorcode(string columnname, string comparison, string value, int hex, string target)
        {
            throw new NotImplementedException();
        }

        public TypeJobCostDetailsGrid CreateNewView(string viewname)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Fetch all the attribute values from Wellbore report grid 
        /// </summary>
        /// <returns>Returns TypeWellboreReportGeneralGrid properties, which specifies attributes of datas of well found in grid</returns>
        /*
                         * Get all the elemnts for individual columns                         * 
                         * Assign each attribute to properties
                         * return the class array
        */
        public TypeJobCostDetailsGrid[] FetchdataScreen()
        {

            string style = SeleniumActions.getAttribute(PageObjects.JobManagementPage.scrollhorizontalcontainerwellborereport, "style");
            string[] splitstyle = style.Split(':');
            string[] splitstyle2 = splitstyle[1].Split(';');
            Console.WriteLine("style1->" + splitstyle[1]);
            Console.WriteLine("style->" + splitstyle2[0]);
            scrollsize = int.Parse(splitstyle2[0].Substring(0, splitstyle2[0].Length - 2));
            Console.WriteLine("szie->" + scrollsize);
            //scrollsize = Convert.ToDouble(splitstyle2[0].Trim());   
            List<string> VendorName = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='VendorName']//span[@class='cell-layer-1']/span[2]");
            List<string> CatalogItemDescription = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='CatalogItemDescription']//span[@class='cell-layer-1']/span[2]");
            double i = Convert.ToDouble(scrollp);
            while (CatalogItemDescription == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                CatalogItemDescription = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='CatalogItemDescription']//span[@class='cell-layer-1']/span[2]");
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }
            List<string> Cost = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='Cost']//span[@class='cell-layer-1']/span[2]");

            while (Cost == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                Cost = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='Cost']//span[@class='cell-layer-1']/span[2]");
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }
            List<string> Quantity = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='Quantity']//span[@class='cell-layer-1']/span[2]");

            while (Quantity == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                Quantity = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='Quantity']//span[@class='cell-layer-1']/span[2]");
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }
            List<string> UnitPrice = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='UnitPrice']//span[@class='cell-layer-1']/span[2]");

            while (UnitPrice == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                UnitPrice = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='UnitPrice']//span[@class='cell-layer-1']/span[2]");
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }
            List<string> Discount = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='Discount']//span[@class='cell-layer-1']/span[2]");

            while (Discount == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                Discount = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='Discount']//span[@class='cell-layer-1']/span[2]");
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }
            List<string> CostRemarks = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='CostRemarks']//span[@class='cell-layer-1']/span[2]");

            while (CostRemarks == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                CostRemarks = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='CostRemarks']//span[@class='cell-layer-1']/span[2]");
                i = i + Convert.ToDouble(scrollp);
                /*     if (i > scrollsize)
                     {
                         Console.WriteLine("Maximum scrolling point reached, Failing the test");
                         Assert.Fail();
                     }*/
            }


            List<TypeJobCostDetailsGrid> Datahome = new List<TypeJobCostDetailsGrid>();

            for (int a = 0; a < VendorName.Count; a++)
            {
                Datahome.Add(new TypeJobCostDetailsGrid()
                {

                    vendor = VendorName[a],
                    catalogitem = CatalogItemDescription[a],
                    cost = Cost[a],
                    quantity = Quantity[a],
                    unitprice = UnitPrice[a],
                    discount = Discount[a],
                    remarks = CostRemarks[a],
                });


            }
            return Datahome.ToArray();

        }


        public TypeJobCostDetailsGrid[] Search(string text)
        {
            throw new NotImplementedException();
        }


        public TypeJobCostDetailsGrid[] Sort(By columnsorticonlocator, string sorttype, By columnhover)
        {
            throw new NotImplementedException();
        }

        public TypeJobCostDetailsGrid[] Filter(string text, By columnmenuicon, By columnheader)
        {
            throw new NotImplementedException();
        }
    }
}
