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
    class WellTestDataProcess : IGridScenarios<TypeWelltestGrid>
    {
        double height;
        double width;
        public string scrollp;
        public int scrollsize;
        public WellTestDataProcess()
        {
            height = SystemParameters.PrimaryScreenHeight;
            width = SystemParameters.PrimaryScreenWidth;

            Console.Write("Screen size found width:-" + width);

            double half = width / 2;
            scrollp = half.ToString();

        }
        public TypeWelltestGrid Colorcode(string columnname, string comparison, string value, int hex, string target)
        {
            throw new NotImplementedException();
        }

        public TypeWelltestGrid CreateNewView(string viewname)
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
        public TypeWelltestGrid[] FetchESPWelltestdataScreen()
        {

            string style = SeleniumActions.getAttribute(PageObjects.WellTestPage.scrollhorizontalcontainerindex2, "style");
            string[] splitstyle = style.Split(':');
            string[] splitstyle2 = splitstyle[1].Split(';');
            Console.WriteLine("style1->" + splitstyle[1]);
            Console.WriteLine("style->" + splitstyle2[0]);
            scrollsize = int.Parse(splitstyle2[0].Substring(0, splitstyle2[0].Length - 2));
            Console.WriteLine("szie->" + scrollsize);
            List<string> TestDate = getwelltestelements(PageObjects.WellTestPage.TestDate);
            List<string> TestDuration = getwelltestelements(PageObjects.WellTestPage.TestDuration);
            List<string> SPTCode = getwelltestelements(PageObjects.WellTestPage.SPTCode);
            List<string> CalibrationMethod = getwelltestelements(PageObjects.WellTestPage.CalibrationMethod);
            List<string> TuningStatus = getwelltestelements(PageObjects.WellTestPage.TuningStatus);
            List<string> Message = getwelltestelements(PageObjects.WellTestPage.Message);
            List<string> AverageTubingPressure = getwelltestelements(PageObjects.WellTestPage.AverageTubingPressure);
            List<string> AverageTubingTemperature = getwelltestelements(PageObjects.WellTestPage.AverageTubingTemperature);
            List<string> AverageCasingPressure = getwelltestelements(PageObjects.WellTestPage.AverageCasingPressure);
            List<string> PumpIntakePressure = getwelltestelements(PageObjects.WellTestPage.PIP_2);
            List<string> PumpDischargePressure = getwelltestelements(PageObjects.WellTestPage.PDP_2);
            List<string> Frequency = getwelltestelements(PageObjects.WellTestPage.Frequency_2);
            List<string> Motorvolts = getwelltestelements(PageObjects.WellTestPage.motorvolts_2);
            List<string> MotorAmps = getwelltestelements(PageObjects.WellTestPage.motoramps_2);
            List<string> Flowlinepr = getwelltestelements(PageObjects.WellTestPage.Flowlinepr_2);
            List<string> Seppr = getwelltestelements(PageObjects.WellTestPage.Seppr_2);
            List<string> ChokeSize = getwelltestelements(PageObjects.WellTestPage.ChokeSize);
            List<string> Oil = getwelltestelements(PageObjects.WellTestPage.Oil);
            List<string> Gas = getwelltestelements(PageObjects.WellTestPage.Gas);
            List<string> Water = getwelltestelements(PageObjects.WellTestPage.Water);
            List<string> GOR = getwelltestelements(PageObjects.WellTestPage.GOR);
            List<string> WaterCut = getwelltestelements(PageObjects.WellTestPage.WaterCut);
            List<string> FBHP = getwelltestelements(PageObjects.WellTestPage.FBHP);
            List<string> LFactor = getwelltestelements(PageObjects.WellTestPage.LFactor);
            List<string> ProductivityIndex = getwelltestelements(PageObjects.WellTestPage.ProductivityIndex);
            List<string> ReservoirPressure = getwelltestelements(PageObjects.WellTestPage.ReservoirPressure);
            List<string> Headtuningfactor = getwelltestelements(PageObjects.WellTestPage.Headtuningfactor);


            List<TypeWelltestGrid> Datahome = new List<TypeWelltestGrid>();

            for (int a = 0; a < SPTCode.Count; a++)
            {
                Datahome.Add(new TypeWelltestGrid()

                {
                    TestDate = TestDate[a],
                    TestDuration = TestDuration[a],
                    Qualitycode = SPTCode[a],
                    TuningMethod = CalibrationMethod[a],
                    status = TuningStatus[a],
                    Message = Message[a],
                    THP = AverageTubingPressure[a],
                    THT = AverageTubingTemperature[a],
                    CHP = AverageCasingPressure[a],
                    PIP = PumpIntakePressure[a],
                    PDP = PumpDischargePressure[a],
                    Frequency = Frequency[a],
                    OilRate = Oil[a],
                    GasRate = Gas[a],
                    WaterRate = Water[a],
                    FormationGOR = GOR[a],
                    MotorVolts = Motorvolts[a],
                    MotorAmps = MotorAmps[a],
                    WaterCut = WaterCut[a],
                    Flowlinepr = Flowlinepr[a],
                    Seppr = Seppr[a],
                    FBHP = FBHP[a],
                    ChokeSize = ChokeSize[a],
                    LFactor = LFactor[a],
                    ProductivityIndex = ProductivityIndex[a],
                    ReservoirPressure = ReservoirPressure[a],
                    HeadTuningFactor = Headtuningfactor[a],

                }
                     );

            }
            return Datahome.ToArray();

        }
        public TypeWelltestGrid[] FetchdataScreen()
        {

            string style = SeleniumActions.getAttribute(PageObjects.WellTestPage.scrollhorizontalcontainerindex2, "style");
            string[] splitstyle = style.Split(':');
            string[] splitstyle2 = splitstyle[1].Split(';');
            Console.WriteLine("style1->" + splitstyle[1]);
            Console.WriteLine("style->" + splitstyle2[0]);
            scrollsize = int.Parse(splitstyle2[0].Substring(0, splitstyle2[0].Length - 2));
            Console.WriteLine("szie->" + scrollsize);
            //scrollsize = Convert.ToDouble(splitstyle2[0].Trim());   
            //  List<string> jobId = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.jobidprosjob);
            List<string> TestDate = getwelltestelements(PageObjects.WellTestPage.TestDate);
            List<string> TestDuration = getwelltestelements(PageObjects.WellTestPage.TestDuration);
            List<string> SPTCode = getwelltestelements(PageObjects.WellTestPage.SPTCode);
            List<string> CalibrationMethod = getwelltestelements(PageObjects.WellTestPage.CalibrationMethod);
            List<string> TuningStatus = getwelltestelements(PageObjects.WellTestPage.TuningStatus);
            List<string> Message = getwelltestelements(PageObjects.WellTestPage.Message);
            List<string> AverageTubingPressure = getwelltestelements(PageObjects.WellTestPage.AverageTubingPressure);
            List<string> AverageCasingPressure = getwelltestelements(PageObjects.WellTestPage.AverageCasingPressure);
            List<string> AverageTubingTemperature = getwelltestelements(PageObjects.WellTestPage.AverageTubingTemperature);
            List<string> GasInjectionRate = getwelltestelements(PageObjects.WellTestPage.GasInjectionRate);
            List<string> Oil = getwelltestelements(PageObjects.WellTestPage.Oil);
            List<string> Gas = getwelltestelements(PageObjects.WellTestPage.Gas);
            List<string> TotalGasRate = getwelltestelements(PageObjects.WellTestPage.TotalGasRate);
            List<string> Water = getwelltestelements(PageObjects.WellTestPage.Water);
            List<string> GOR = getwelltestelements(PageObjects.WellTestPage.GOR);
            List<string> WaterCut = getwelltestelements(PageObjects.WellTestPage.WaterCut);
            List<string> InjectionGLR = getwelltestelements(PageObjects.WellTestPage.InjectionGLR);
            List<string> LFactor = getwelltestelements(PageObjects.WellTestPage.LFactor);
            List<string> ProductivityIndex = getwelltestelements(PageObjects.WellTestPage.ProductivityIndex);
            List<string> ReservoirPressure = getwelltestelements(PageObjects.WellTestPage.ReservoirPressure);



            List<TypeWelltestGrid> Datahome = new List<TypeWelltestGrid>();

            for (int a = 0; a < SPTCode.Count; a++)
            {
                Datahome.Add(new TypeWelltestGrid()

                {
                    TestDate = TestDate[a],
                    TestDuration = TestDuration[a],
                    Qualitycode = SPTCode[a],
                    TuningMethod = CalibrationMethod[a],
                    status = TuningStatus[a],
                    Message = Message[a],
                    THP = AverageTubingPressure[a],
                    THT = AverageTubingTemperature[a],
                    CHP = AverageCasingPressure[a],
                    GasInjectionRate = GasInjectionRate[a],
                    OilRate = Oil[a],
                    GasRate = Gas[a],
                    FormationGasRate = TotalGasRate[a],
                    WaterRate = Water[a],
                    FormationGOR = GOR[a],
                    WaterCut = WaterCut[a],
                    InjectionGLR = InjectionGLR[a],
                    LFactor = LFactor[a],
                    ProductivityIndex = ProductivityIndex[a],
                    ReservoirPressure = ReservoirPressure[a],

                }
                     );

            }
            return Datahome.ToArray();

        }
        public List<string> getwelltestelements(string value)
        {

            List<string> ele = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", value);
            double i = Convert.ToDouble(scrollp);
            while (ele == null)
            {
                SeleniumActions.scrollbyel(PageObjects.WellTestPage.scrollhorizontaindex2, scrollp, "0");
                ele = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", value);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }
            return ele;
        }

        public TypeWelltestGrid[] Search(string text)
        {
            throw new NotImplementedException();
        }


        public TypeWelltestGrid[] Sort(By columnsorticonlocator, string sorttype, By columnhover)
        {
            throw new NotImplementedException();
        }

        public TypeWelltestGrid[] Filter(string text, By columnmenuicon, By columnheader)
        {
            throw new NotImplementedException();
        }
    }
}
