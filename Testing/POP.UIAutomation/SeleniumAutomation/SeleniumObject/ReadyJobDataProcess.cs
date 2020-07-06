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
    class ReadyJobDataProcess : IGridScenarios<TypeReadyJobsGrid>
    {
        double height;
        double width;
        public string scrollp;
        public int scrollsize;
        public ReadyJobDataProcess()
        {
            height = SystemParameters.PrimaryScreenHeight;
            width = SystemParameters.PrimaryScreenWidth;

            Console.Write("Screen size found width:-" + width);

            double half = width / 2;
            scrollp = half.ToString();

        }
        public TypeReadyJobsGrid Colorcode(string columnname, string comparison, string value, int hex, string target)
        {
            throw new NotImplementedException();
        }

        public TypeReadyJobsGrid CreateNewView(string viewname)
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
        public TypeReadyJobsGrid[] FetchdataScreen()
        {

            string style = SeleniumActions.getAttribute(PageObjects.JobManagementPage.scrollhorizontalcontainerwellborereport, "style");
            string[] splitstyle = style.Split(':');
            string[] splitstyle2 = splitstyle[1].Split(';');
            Console.WriteLine("style1->" + splitstyle[1]);
            Console.WriteLine("style->" + splitstyle2[0]);
            scrollsize = int.Parse(splitstyle2[0].Substring(0, splitstyle2[0].Length - 2));
            Console.WriteLine("szie->" + scrollsize);
            //scrollsize = Convert.ToDouble(splitstyle2[0].Trim());   
            List<string> jobId = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.jobidreadyjob);
            List<string> assetname = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.assetNamereadyjob);
            double i = Convert.ToDouble(scrollp);

            while (assetname == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                assetname = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.assetNamereadyjob);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached unable to find asset name, Failing the test");
                    //Assert.Fail();
                    break;
                }
            }
            List<string> wellname = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.WellNamereadyjobs);

            while (wellname == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                wellname = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.WellNamereadyjobs);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached unable to find well name, Failing the test");
                    break;
                    //Assert.Fail();
                }
            }
            List<string> status = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.StatusIdreadyjobs);

            while (status == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                status = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.StatusIdreadyjobs);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached unable to find Status, Failing the test");
                    //Assert.Fail();
                    break;
                }
            }
            List<string> JobType = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.JobTypereadyjobs);

            while (JobType == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                JobType = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.JobTypereadyjobs);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached unbale to find JobType, Failing the test");
                    // Assert.Fail();
                    break;
                }
            }
            List<string> jobreason = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.JobReasonreadyjobs);

            while (jobreason == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                jobreason = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.JobReasonreadyjobs);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached unable to find JobReason, Failing the test");
                    //Assert.Fail();
                    break;
                }
            }
            List<string> begindate = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.BeginDateJSreadyjobs);

            while (begindate == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                begindate = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.BeginDateJSreadyjobs);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached unable to find BeginDate, Failing the test");
                    //Assert.Fail();
                    break;
                }
            }
            List<string> enddate = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.EndDateJSreadyjobs);

            while (enddate == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                enddate = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.EndDateJSreadyjobs);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached unbale to find EndDate, Failing the test");
                    //Assert.Fail();
                    break;
                }
            }
            List<string> totalcost = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.TotalCostreadyjobs);

            while (totalcost == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                totalcost = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.TotalCostreadyjobs);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached unbale to find TotalCost, Failing the test");
                    //Assert.Fail();
                    break;
                }
            }
            List<string> dpi = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.EconomicAnalysisDPIreadyjobs);

            while (dpi == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                dpi = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.EconomicAnalysisDPIreadyjobs);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached unable to find EconomicAnalysisDPI, Failing the test");
                    //Assert.Fail();
                    break;
                }
            }

            List<string> netvalue = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.EconomicAnalysisNPVreadyjobs);

            while (netvalue == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                netvalue = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.EconomicAnalysisNPVreadyjobs);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached unable to find EconomicAnalysisNPV, Failing the test");
                    //Assert.Fail();
                    break;
                }
            }
            List<string> afe = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.AFEIdDescreadyjobs);

            while (afe == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                afe = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.AFEIdDescreadyjobs);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached unable to find AFE, Failing the test");
                    //Assert.Fail();
                    break;
                }
            }
            List<string> accresp = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.AccountRefreadyjobs);

            while (accresp == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                accresp = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.AccountRefreadyjobs);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached unable to find Account refrence, Failing the test");
                    //Assert.Fail();
                    break;
                }
            }
            List<string> servprov = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.BusinessOrganizationreadyjobs);

            while (servprov == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                servprov = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.BusinessOrganizationreadyjobs);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached unable to find BusinessOrganization, Failing the test");
                    //Assert.Fail();
                    break;
                }
            }
            List<string> responsibleperson = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.ResponsiblePersonreadyjobs);

            while (responsibleperson == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                responsibleperson = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.ResponsiblePersonreadyjobs);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached unable to find ResponsiblePerson, Failing the test");
                    //Assert.Fail();
                    break;
                }
            }
            List<string> truckunit = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.TruckUnitreadyjobs);

            while (truckunit == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                truckunit = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.TruckUnitreadyjobs);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached , Failing the test");
                    // Assert.Fail();
                    break;
                }
            }

            List<string> EstimatedDuration = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.JobPlanEstimatedDuration);

            while (EstimatedDuration == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                EstimatedDuration = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.JobPlanEstimatedDuration);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached , Failing the test");
                    // Assert.Fail();
                    break;
                }
            }

            /* **************Have to add more columns in future depending on test reuirment*************** */

            List<TypeReadyJobsGrid> Datahome = new List<TypeReadyJobsGrid>();

            for (int a = 0; a < jobId.Count; a++)
            {
                Datahome.Add(new TypeReadyJobsGrid()
                {

                    jobId = jobId[a],
                    assetName = assetname[a],
                    wellName = wellname[a],
                    jobType = JobType[a],
                    jobReason = jobreason[a],
                    beginDate = begindate[a],
                    endDate = enddate[a],
                    totalCost = totalcost[a],
                    dpi = dpi[a],
                    netValue = netvalue[a],
                    afeid = afe[a],
                    accountingrefernce = accresp[a],
                    serviceProvider = servprov[a],
                    responsiblePerson = responsibleperson[a],
                    truckUnit = truckunit[a],

                });


            }
            return Datahome.ToArray();

        }


        public TypeReadyJobsGrid[] Search(string text)
        {
            throw new NotImplementedException();
        }



        public TypeReadyJobsGrid[] Sort(By columnsorticonlocator, string sorttype, By columnhover)
        {
            throw new NotImplementedException();
        }

        public TypeReadyJobsGrid[] Filter(string text, By columnmenuicon, By columnheader)
        {
            throw new NotImplementedException();
        }
    }
}
