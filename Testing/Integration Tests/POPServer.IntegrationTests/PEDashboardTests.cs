using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using CxCvsLib;
using CygNet.API.Historian;
using CygNet.Data.Historian;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Weatherford.POP.DTOs;
using Weatherford.POP.Enums;
using Weatherford.POP.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Weatherford.POP.Server.IntegrationTests
{
    [TestClass]
    public class PEDashboardTests : APIClientTestBase
    {
        protected IPEDashboardService _peDashboardService;

        protected IPEDashboardService PEDashboardService
        {
            get { return _peDashboardService ?? (_peDashboardService = _serviceFactory.GetService<IPEDashboardService>()); }
        }

        public void AddWell(string facility_tag, WellTypeId wellType)
        {
            switch (wellType)
            {
                case WellTypeId.RRL:
                    {
                        string facilityId;
                        if (s_isRunningInATS)
                            facilityId = "D5";
                        else
                            facilityId = "D4";
                        int end = 0;
                        if (facility_tag.Contains("RPOC_") || facility_tag.Contains("SAM_"))
                            end = s_wellsEnd;
                        else
                            end = 5;//only 5 facilities are created in CygNet for the EPICF,8800,AEPOC controller types
                        for (int i = s_wellStart; i <= end; i++)
                        {
                            string wellName = facility_tag + i.ToString(facilityId);
                            WellDTO well = SetDefaultFluidType(new WellDTO()
                            {
                                Name = wellName,
                                FacilityId = wellName,
                                DataConnection = GetDefaultCygNetDataConnection(),
                                WellType = WellTypeId.RRL,
                                Lease = "Lease_" + i.ToString("D2"),
                                Foreman = "Foreman" + i.ToString("D2"),
                                Field = "Field" + i.ToString("D2"),
                                Engineer = "Nagaraja",
                                GaugerBeat = "GaugerBeat" + i.ToString("D2"),
                                GeographicRegion = "GeographicRegion" + i.ToString("D2"),
                                welUserDef01 = "State_" + i.ToString("D2"),
                                welUserDef02 = "User_" + i.ToString("D2"),
                                SubAssemblyAPI = "SubAssemblyAPI" + i.ToString("D2"),
                                AssemblyAPI = "AssemblyAPI" + i.ToString("D2"),
                                IntervalAPI = "IntervalAPI" + i.ToString("D2"),
                                CommissionDate = DateTime.Today.AddDays(i),
                            });
                            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = well });
                            WellDTO addedWell = WellService.GetAllWells().FirstOrDefault(w => w.Name.Equals(well.Name));
                            Assert.IsNotNull(addedWell);
                            _wellsToRemove.Add(addedWell);
                        }
                        break;
                    }
                case WellTypeId.GLift:
                case WellTypeId.ESP:
                case WellTypeId.NF:
                    {
                        WellDTO well = SetDefaultFluidType(new WellDTO()
                        {
                            Name = facility_tag,
                            FacilityId = facility_tag,
                            DataConnection = GetDefaultCygNetDataConnection(),
                            WellType = wellType,// WellTypeId.GLift,
                            Lease = "Lease_" + facility_tag,
                            Foreman = "Foreman" + facility_tag,
                            Field = "Field" + facility_tag,
                            Engineer = facility_tag,
                            GaugerBeat = "GaugerBeat" + facility_tag,
                            GeographicRegion = "GeographicRegion" + facility_tag,
                            welUserDef01 = "State_" + facility_tag,
                            welUserDef02 = "User_" + facility_tag,
                            SubAssemblyAPI = "SubAssemblyAPI" + facility_tag,
                            AssemblyAPI = "AssemblyAPI" + facility_tag,
                            IntervalAPI = "IntervalAPI" + facility_tag,
                            CommissionDate = DateTime.Today.AddYears(-2),
                        });
                        WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = well });
                        WellDTO addedWell = WellService.GetAllWells().FirstOrDefault(w => w.Name.Equals(well.Name));
                        Assert.IsNotNull(addedWell);
                        _wellsToRemove.Add(addedWell);
                        break;
                    }
            }
        }

        public void GetProductionKPI()
        {
            //Get well Filters
            WellFilterDTO filters = WellService.GetWellFilter(null);

            //Get Wells by Filter
            WellFilterDTO wellbyFilter = new WellFilterDTO();
            wellbyFilter.welEngineerValues = filters.welEngineerValues;
            //No Filters selected
            PEKPIDTO prodKPI = PEDashboardService.GetProductionKPI(wellbyFilter);
            double prodcurrentValue = prodKPI.CurrentValue;
            double prodchangeValue = prodKPI.ChangeValue;
            double prodlastValue = prodKPI.LastValue;
            string prodchangePercent = prodKPI.ChangePercentage.ToString();
            if (prodlastValue != 0)
            {
                string prodexpPercent = Math.Round(((prodchangeValue / prodlastValue) * 100), 1).ToString();
                Assert.AreEqual(prodexpPercent, prodchangePercent);
            }
            double prodNewWells = prodKPI.AdditionInCurrentKPI;
            double prodchangeLastKPI = prodKPI.ChangeForLastKPI;
            double prodexpchangeLastKPI = (prodcurrentValue - prodlastValue) - prodNewWells;
            bool prodsetAlarmlow = prodcurrentValue > prodKPI.AlarmSetpointLow;
            bool prodsetAlarmhi = prodcurrentValue < prodKPI.AlarmSetpointHi;
            bool prodindAlarm = prodsetAlarmhi && prodsetAlarmlow;
            string prodColourCode = prodKPI.ColorCode;
            Trace.WriteLine("ProductionCurrentValue: " + string.Join(" ", prodcurrentValue));
            Trace.WriteLine("ProductionLastValue: " + string.Join(" ", prodlastValue));
            Trace.WriteLine("Difference: " + string.Join(" ", (prodcurrentValue - prodlastValue)));
            if (prodKPI.ChangePercentage <= -1)
                Assert.AreEqual(prodColourCode, "#D91322");
            if (prodKPI.ChangePercentage > -1 && prodKPI.ChangePercentage < 1 && prodKPI.ChangePercentage != 0)
                Assert.AreEqual(prodColourCode, "#E98D03");
            if (prodKPI.ChangePercentage >= 1)
                Assert.AreEqual(prodColourCode, "#76B02F");
            switch (prodKPI.AlarmSetpointType)
            {
                case 1:
                    if (prodsetAlarmlow == true)
                        Assert.IsFalse(prodKPI.IsAlarm);
                    else
                        Assert.IsTrue(prodKPI.IsAlarm);
                    break;

                case 2:
                    if (prodsetAlarmhi == true)
                        Assert.IsFalse(prodKPI.IsAlarm);
                    else
                        Assert.IsTrue(prodKPI.IsAlarm);
                    break;

                case 3:
                    if (prodindAlarm == true)
                        Assert.IsFalse(prodKPI.IsAlarm);
                    else
                        Assert.IsTrue(prodKPI.IsAlarm);
                    break;
            }
            Assert.AreEqual(prodcurrentValue - prodlastValue, prodchangeValue);
            Assert.AreEqual(prodexpchangeLastKPI, prodchangeLastKPI);
        }

        public void GetAllKPIs(bool wsmNotLicensed = false)
        {
            // Get well Filters
            WellFilterDTO filters = WellService.GetWellFilter(null);

            // Get Wells by Filter
            WellFilterDTO wellbyFilter = new WellFilterDTO();
            // wellbyFilter.welEngineerValues = filters.welFK_r_WellTypeValues;//.welEngineerValues;
            wellbyFilter.welFK_r_WellTypeValues = filters.welFK_r_WellTypeValues;
            WellDTO[] well = WellService.GetWellsByFilter(wellbyFilter);
            int welCount = well.Length;

            // No Filters selected
            PEKPIAllDTO allKpis = PEDashboardService.GetAllKPIs(wellbyFilter);

            if (wsmNotLicensed)
            {
                Assert.IsNull(allKpis.Jobs, "Expected Jobs KPI to be null when WSM is not licensed.");
                Assert.IsNull(allKpis.MTBF, "Expected MTBF KPI to be null when WSM is not licensed.");
                Assert.IsNotNull(allKpis.DownWells, "Expected Down Wells KPI to be non-null when WSM is not licensed.");
            }
            else
            {
                Assert.IsNull(allKpis.DownWells, "Expected Down Wells KPI to be null when WSM is licensed.");
                Assert.IsNotNull(allKpis.Jobs, "Expected Jobs KPI to be non-null when WSM is licensed.");
                Assert.IsNotNull(allKpis.MTBF, "Expected MTBF KPI to be non-null when WSM is licensed.");
            }

            // wells in Alarm
            double currentValue = allKpis.AlarmWells.CurrentValue;
            double changeValue = allKpis.AlarmWells.ChangeValue;
            double lastValue = allKpis.AlarmWells.LastValue;
            string changePercent = allKpis.AlarmWells.ChangePercentage.ToString();
            if (lastValue != 0)
            {
                string expPercent = Math.Round(((changeValue / lastValue) * 100), 1).ToString();
                Assert.AreEqual(expPercent, changePercent);
            }

            double NewWells = allKpis.AlarmWells.AdditionInCurrentKPI;
            double changeLastKPI = allKpis.AlarmWells.ChangeForLastKPI;
            double expchangeLastKPI = (currentValue - lastValue) - NewWells;
            bool setAlarmlow = currentValue > allKpis.AlarmWells.AlarmSetpointLow;
            bool setAlarmhi = currentValue < allKpis.AlarmWells.AlarmSetpointHi;
            bool indAlarm = setAlarmhi && setAlarmlow;
            string alarmColourCode = allKpis.AlarmWells.ColorCode;
            if (currentValue > 0)
            {
                if (currentValue < (welCount * 0.05))
                    Assert.AreEqual(alarmColourCode, "#76B02F");
                if (currentValue >= (welCount * 0.05) && currentValue < (welCount * 0.1))
                    Assert.AreEqual(alarmColourCode, "#E98D03");
                if (currentValue >= (welCount * 0.1))
                    Assert.AreEqual(alarmColourCode, "#D91322");
            }

            //down wells
            //double dwcurrentValue = allKpis.DownWells.CurrentValue;
            //double dwchangeValue = allKpis.DownWells.ChangeValue;
            //double dwlastValue = allKpis.DownWells.LastValue;
            //string dwchangePercent = allKpis.DownWells.ChangePercentage.ToString();
            //if (dwlastValue != 0)
            //{
            //    string dwexpPercent = Math.Round(((dwchangeValue / dwlastValue) * 100), 1).ToString();
            //    Assert.AreEqual(dwexpPercent, dwchangePercent);
            //}
            //double dwNewWells = allKpis.DownWells.AdditionInCurrentKPI;
            //double dwchangeLastKPI = allKpis.DownWells.ChangeForLastKPI;
            //double dwexpchangeLastKPI = (dwcurrentValue - dwlastValue) - dwNewWells;
            //bool dwsetAlarmlow = dwcurrentValue > allKpis.DownWells.AlarmSetpointLow;
            //bool dwsetAlarmhi = dwcurrentValue < allKpis.DownWells.AlarmSetpointHi;
            //bool dwindAlarm = dwsetAlarmhi && dwsetAlarmlow;
            //string dwColourCode = allKpis.DownWells.ColorCode;
            //if (dwcurrentValue < (welCount * 0.05))
            //    Assert.AreEqual(dwColourCode, "#76B02F");
            //if (dwcurrentValue >= (welCount * 0.05) && dwcurrentValue < (welCount * 0.1))
            //    Assert.AreEqual(dwColourCode, "#E98D03");
            //if (dwcurrentValue >= (welCount * 0.1))
            //    Assert.AreEqual(dwColourCode, "#D91322");

            // production
            double pcurrentValue = allKpis.Production.CurrentValue;
            double pchangeValue = allKpis.Production.ChangeValue;
            double plastValue = allKpis.Production.LastValue;
            string pchangePercent = allKpis.Production.ChangePercentage.ToString();
            if (plastValue != 0)
            {
                string pexpPercent = Math.Round(((pchangeValue / plastValue) * 100), 1).ToString();
                Assert.AreEqual(pexpPercent, pchangePercent);
            }
            double pNewWells = allKpis.Production.AdditionInCurrentKPI;
            double pchangeLastKPI = allKpis.Production.ChangeForLastKPI;
            double pexpchangeLastKPI = (pcurrentValue - plastValue) - pNewWells;
            bool psetAlarmlow = pcurrentValue > allKpis.Production.AlarmSetpointLow;
            bool psetAlarmhi = pcurrentValue < allKpis.Production.AlarmSetpointHi;
            bool pindAlarm = psetAlarmhi && psetAlarmlow;
            string pColourCode = allKpis.Production.ColorCode;
            if (allKpis.Production.ChangePercentage <= -1)
                Assert.AreEqual(pColourCode, "#D91322");
            if (allKpis.Production.ChangePercentage > -1 && allKpis.Production.ChangePercentage < 1)
                Assert.AreEqual(pColourCode, "#E98D03");
            if (allKpis.Production.ChangePercentage >= 1)
                Assert.AreEqual(pColourCode, "#76B02F");

            // MTBF - don't evaluate if WSM is not licensed because allKpis.MTBF will be null
            if (!wsmNotLicensed)
            {
                double mtbfcurrentValue = allKpis.MTBF.CurrentValue;
                double mtbfchangeValue = allKpis.MTBF.ChangeValue;
                double mtbflastValue = allKpis.MTBF.LastValue;
                string mtbfchangePercent = allKpis.MTBF.ChangePercentage.ToString();
                if (mtbflastValue != 0)
                {
                    string mtbfexpPercent = Math.Round(((mtbfchangeValue / mtbflastValue) * 100), 1).ToString();
                    Assert.AreEqual(mtbfexpPercent, mtbfchangePercent);
                }
                double mtbfNewWells = allKpis.MTBF.AdditionInCurrentKPI;
                double mtbfchangeLastKPI = allKpis.MTBF.ChangeForLastKPI;
                double mtbfexpchangeLastKPI = (mtbfcurrentValue - mtbflastValue) - mtbfNewWells;
                bool mtbfsetAlarmlow = mtbfcurrentValue > allKpis.MTBF.AlarmSetpointLow;
                bool mtbfsetAlarmhi = mtbfcurrentValue < allKpis.MTBF.AlarmSetpointHi;
                bool mtbfindAlarm = mtbfsetAlarmhi && mtbfsetAlarmlow;
                string mtbfColourCode = allKpis.MTBF.ColorCode;
                if ((mtbfcurrentValue - mtbflastValue) < 0)
                    Assert.AreEqual(mtbfColourCode, "#D91322");
                if ((mtbfcurrentValue - mtbflastValue) == 0 && (mtbfcurrentValue != 0 && mtbflastValue != 0))
                    Assert.AreEqual(mtbfColourCode, "#E98D03");
                if ((mtbfcurrentValue - mtbflastValue) > 0)
                    Assert.AreEqual(mtbfColourCode, "#76B02F");

                //validating the MTBF
                switch (allKpis.MTBF.AlarmSetpointType)
                {
                    case 1:
                        if (mtbfsetAlarmlow == true)
                            Assert.IsFalse(allKpis.MTBF.IsAlarm);
                        else
                            Assert.IsTrue(allKpis.MTBF.IsAlarm);
                        break;

                    case 2:
                        if (mtbfsetAlarmhi == true)
                            Assert.IsFalse(allKpis.MTBF.IsAlarm);
                        else
                            Assert.IsTrue(allKpis.MTBF.IsAlarm);
                        break;

                    case 3:
                        if (mtbfindAlarm == true)
                            Assert.IsFalse(allKpis.MTBF.IsAlarm);
                        else
                            Assert.IsTrue(allKpis.MTBF.IsAlarm);
                        break;
                }
                Assert.AreEqual(mtbfcurrentValue - mtbflastValue, mtbfchangeValue);
                Assert.AreEqual(mtbfexpchangeLastKPI, mtbfchangeLastKPI);
            }

            // validating the wells in Alarm
            switch (allKpis.AlarmWells.AlarmSetpointType)
            {
                case 1:
                    if (setAlarmlow == true)
                        Assert.IsFalse(allKpis.AlarmWells.IsAlarm);
                    else
                        Assert.IsTrue(allKpis.AlarmWells.IsAlarm);
                    break;

                case 2:
                    if (setAlarmhi == true)
                        Assert.IsFalse(allKpis.AlarmWells.IsAlarm);
                    else
                        Assert.IsTrue(allKpis.AlarmWells.IsAlarm);
                    break;

                case 3:
                    if (indAlarm == true)
                        Assert.IsFalse(allKpis.AlarmWells.IsAlarm);
                    else
                        Assert.IsTrue(allKpis.AlarmWells.IsAlarm);
                    break;
            }
            Assert.AreEqual(currentValue - lastValue, changeValue);
            Assert.AreEqual(expchangeLastKPI, changeLastKPI);

            //validating the down wells
            //switch (allKpis.DownWells.AlarmSetpointType)
            //{
            //    case 1:
            //        if (dwsetAlarmlow == true)
            //            Assert.IsFalse(allKpis.DownWells.IsAlarm);
            //        else
            //            Assert.IsTrue(allKpis.DownWells.IsAlarm);
            //        break;

            //    case 2:
            //        if (dwsetAlarmhi == true)
            //            Assert.IsFalse(allKpis.DownWells.IsAlarm);
            //        else
            //            Assert.IsTrue(allKpis.DownWells.IsAlarm);
            //        break;

            //    case 3:
            //        if (dwindAlarm == true)
            //            Assert.IsFalse(allKpis.DownWells.IsAlarm);
            //        else
            //            Assert.IsTrue(allKpis.DownWells.IsAlarm);
            //        break;
            //}
            //Assert.AreEqual(dwcurrentValue - dwlastValue, dwchangeValue);
            //Assert.AreEqual(dwexpchangeLastKPI, dwchangeLastKPI);

            // validating the production
            switch (allKpis.Production.AlarmSetpointType)
            {
                case 1:
                    if (psetAlarmlow == true)
                        Assert.IsFalse(allKpis.Production.IsAlarm);
                    else
                        Assert.IsTrue(allKpis.Production.IsAlarm);
                    break;

                case 2:
                    if (psetAlarmhi == true)
                        Assert.IsFalse(allKpis.Production.IsAlarm);
                    else
                        Assert.IsTrue(allKpis.Production.IsAlarm);
                    break;

                case 3:
                    if (pindAlarm == true)
                        Assert.IsFalse(allKpis.Production.IsAlarm);
                    else
                        Assert.IsTrue(allKpis.Production.IsAlarm);
                    break;
            }
            Assert.AreEqual(pcurrentValue - plastValue, pchangeValue);
            Assert.AreEqual(pexpchangeLastKPI, pchangeLastKPI);

        }

        public void GetAllTrends()
        {
            //Get well Filters
            WellFilterDTO filters = WellService.GetWellFilter(null);

            //Get Wells by Filter
            WellFilterDTO wellbyFilter = new WellFilterDTO();
            //No Filters selected
            var peTrendsAndUnits = PEDashboardService.GetAllTrends(wellbyFilter);
            PETrendPoint[] allTrends = peTrendsAndUnits.Values;
            DateTime startDate = allTrends[0].Timestamp;
            DateTime endDate = allTrends[allTrends.Length - 1].Timestamp;
            double DateWindow = (endDate - startDate).TotalDays;
            Assert.IsTrue(DateWindow > 240);

        }

        public void GetDownWellsKPI()
        {
            try
            {
                // disable WSM license, but don't persist setting beyond this test
                // WSM must be disabled, otherwise Wells Down KPI returns null
                List<SystemFeature> disableLicenses = new List<SystemFeature>();
                disableLicenses.Add(SystemFeature.WSM);
                LicenseService.DisableLicenses(disableLicenses);

                //Get well Filters
                WellFilterDTO filters = WellService.GetWellFilter(null);

                //Get Wells by Filter
                WellFilterDTO wellbyFilter = new WellFilterDTO();
                wellbyFilter.welEngineerValues = filters.welEngineerValues;
                WellDTO[] well = WellService.GetWellsByFilter(wellbyFilter);
                int welCount = well.Length;
                //No Filters selected
                PEKPIDTO downKPI = PEDashboardService.GetDownWellsKPI(wellbyFilter);
                double downcurrentValue = downKPI.CurrentValue;
                double downchangeValue = downKPI.ChangeValue;
                double downlastValue = downKPI.LastValue;
                string downchangePercent = downKPI.ChangePercentage.ToString();
                if (downlastValue != 0)
                {
                    string downexpPercent = Math.Round(((downchangeValue / downlastValue) * 100), 1).ToString();
                    Assert.AreEqual(downexpPercent, downchangePercent);
                }
                double downNewWells = downKPI.AdditionInCurrentKPI;
                double downchangeLastKPI = downKPI.ChangeForLastKPI;
                double downexpchangeLastKPI = (downcurrentValue - downlastValue) - downNewWells;
                bool downsetAlarmlow = downcurrentValue > downKPI.AlarmSetpointLow;
                bool downsetAlarmhi = downcurrentValue < downKPI.AlarmSetpointHi;
                bool downindAlarm = downsetAlarmhi && downsetAlarmlow;
                string downColourCode = downKPI.ColorCode;
                if (downcurrentValue < (welCount * 0.05))
                    Assert.AreEqual(downColourCode, "#76B02F");
                if (downcurrentValue >= (welCount * 0.05) && downcurrentValue < (welCount * 0.1))
                    Assert.AreEqual(downColourCode, "#E98D03");
                if (downcurrentValue >= (welCount * 0.1))
                    Assert.AreEqual(downColourCode, "#D91322");
                switch (downKPI.AlarmSetpointType)
                {
                    case 1:
                        if (downsetAlarmlow == true)
                            Assert.IsFalse(downKPI.IsAlarm);
                        else
                            Assert.IsTrue(downKPI.IsAlarm);
                        break;

                    case 2:
                        if (downsetAlarmhi == true)
                            Assert.IsFalse(downKPI.IsAlarm);
                        else
                            Assert.IsTrue(downKPI.IsAlarm);
                        break;

                    case 3:
                        if (downindAlarm == true)
                            Assert.IsFalse(downKPI.IsAlarm);
                        else
                            Assert.IsTrue(downKPI.IsAlarm);
                        break;
                }
                Assert.AreEqual(downcurrentValue - downlastValue, downchangeValue);
                Assert.AreEqual(downexpchangeLastKPI, downchangeLastKPI);
            }
            finally
            {
                // Clear the disabled WSM license so as not to impact other tests
                LicenseService.ClearDisabledLicenses();
            }
        }

        public void GetDownWellsKPIWithWSMEnabled()
        {
            //Get well Filters
            WellFilterDTO filters = WellService.GetWellFilter(null);

            //Get Wells by Filter
            WellFilterDTO wellbyFilter = new WellFilterDTO();
            wellbyFilter.welEngineerValues = filters.welEngineerValues;
            PEKPIDTO downKPI = PEDashboardService.GetDownWellsKPI(wellbyFilter);
            Assert.IsNull(downKPI, "Down Wells KPI should be null if WSM is licensed.");
        }

        public void GetAlarmWellsKPI()
        {
            //Get well Filters
            WellFilterDTO filters = WellService.GetWellFilter(null);

            //Get Wells by Filter
            WellFilterDTO wellbyFilter = new WellFilterDTO();
            wellbyFilter.welEngineerValues = filters.welEngineerValues;
            WellDTO[] well = WellService.GetWellsByFilter(wellbyFilter);
            int welCount = well.Length;
            //No Filters selected
            PEKPIDTO alarmKPI = PEDashboardService.GetAlarmWellsKPI(wellbyFilter);
            double alarmcurrentValue = alarmKPI.CurrentValue;
            double alarmchangeValue = alarmKPI.ChangeValue;
            double alarmlastValue = alarmKPI.LastValue;
            string alarmchangePercent = alarmKPI.ChangePercentage.ToString();
            if (alarmlastValue != 0)
            {
                string alarmexpPercent = Math.Round(((alarmchangeValue / alarmlastValue) * 100), 1).ToString();
                Assert.AreEqual(alarmexpPercent, alarmchangePercent);
            }

            double alarmNewWells = alarmKPI.AdditionInCurrentKPI;
            double alarmchangeLastKPI = alarmKPI.ChangeForLastKPI;
            double alarmexpchangeLastKPI = (alarmcurrentValue - alarmlastValue) - alarmNewWells;
            bool alarmsetAlarmlow = alarmcurrentValue > alarmKPI.AlarmSetpointLow;
            bool alarmsetAlarmhi = alarmcurrentValue < alarmKPI.AlarmSetpointHi;
            bool alarmindAlarm = alarmsetAlarmhi && alarmsetAlarmlow;
            string alarmColourCode = alarmKPI.ColorCode;
            if (alarmcurrentValue < (welCount * 0.05))
                Assert.AreEqual("#76B02F", alarmColourCode);
            if (alarmcurrentValue >= (welCount * 0.05) && alarmcurrentValue < (welCount * 0.1))
                Assert.AreEqual("#E98D03", alarmColourCode);
            if (alarmcurrentValue >= (welCount * 0.1))
                Assert.AreEqual("#D91322", alarmColourCode);
            switch (alarmKPI.AlarmSetpointType)
            {
                case 1:
                    if (alarmsetAlarmlow == true)
                        Assert.IsFalse(alarmKPI.IsAlarm);
                    else
                        Assert.IsTrue(alarmKPI.IsAlarm);
                    break;

                case 2:
                    if (alarmsetAlarmhi == true)
                        Assert.IsFalse(alarmKPI.IsAlarm);
                    else
                        Assert.IsTrue(alarmKPI.IsAlarm);
                    break;

                case 3:
                    if (alarmindAlarm == true)
                        Assert.IsFalse(alarmKPI.IsAlarm);
                    else
                        Assert.IsTrue(alarmKPI.IsAlarm);
                    break;
            }
            Assert.AreEqual(alarmcurrentValue - alarmlastValue, alarmchangeValue);
            Assert.AreEqual(alarmexpchangeLastKPI, alarmchangeLastKPI);
        }

        [TestInitialize]
        public override void Init()
        {
            base.Init();
            SettingType settingType = SettingType.System;
            SettingDTO bypassSPV = SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Description == SettingServiceStringConstants.BYPASS_SURFACE_PARAMETER_VALIDATION);
            SystemSettingDTO systemSettingSPV = SettingService.GetSystemSettingByName(bypassSPV.Name);
            systemSettingSPV.NumericValue = 1;
            SettingService.SaveSystemSetting(systemSettingSPV);
        }

        [TestCleanup]
        public override void Cleanup()
        {
            SettingType settingType = SettingType.System;
            SettingDTO bypassSPV = SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Description == SettingServiceStringConstants.BYPASS_SURFACE_PARAMETER_VALIDATION);
            SystemSettingDTO systemSettingSPV = SettingService.GetSystemSettingByName(bypassSPV.Name);
            systemSettingSPV.NumericValue = 0;
            SettingService.SaveSystemSetting(systemSettingSPV);
            base.Cleanup();
        }

        [TestCategory(TestCategories.PEDashboardTests), TestMethod]
        public void GetProductionKPI_WP()
        {
            AddWell("RPOC_", WellTypeId.RRL);
            GetProductionKPI();
        }

        [TestCategory(TestCategories.PEDashboardTests), TestMethod]
        public void GetProductionKPI_GL()
        {
            AddWell("EPICFS_", WellTypeId.RRL);
            AddWell(GetFacilityId("GLWELL_", 1), WellTypeId.GLift);
            GetProductionKPI();
        }

        [TestCategory(TestCategories.PEDashboardTests), TestMethod]
        public void GetProductionKPI_ESP()
        {
            string Path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";
            Tuple<string, WellTypeId, ModelFileOptionDTO> model = Tuple.Create("Esp_ProductionTestData.wflx", WellTypeId.ESP, new ModelFileOptionDTO() { CalibrationMethod = CalibrationMethodId.ReservoirPressure, OptionalUpdate = new long[] { ((long)OptionalUpdates.UpdateGOR_CGR), (long)OptionalUpdates.UpdateWCT_WGR } });

            string modelFileName = model.Item1;
            WellTypeId wellType = model.Item2;
            ModelFileOptionDTO options = model.Item3;

            string facilityIdBase = "ESPWELL_";
            string facilityId = s_isRunningInATS ? facilityIdBase + "00001" : facilityIdBase + "0001";

            AddWell(facilityId, WellTypeId.ESP);
            WellDTO addedESPWell = WellService.GetAllWells().FirstOrDefault(w => w.Name.Equals(facilityId));

            var myassembly = WellboreComponentService.GetAssemblyByWellId(addedESPWell.Id.ToString());
            Assert.IsNotNull(myassembly);

            ModelFileBase64DTO modelFile = new ModelFileBase64DTO() { };

            options.Comment = "CASETest Upload " + model.Item1;
            modelFile.Options = options;
            modelFile.ApplicableDate = DateTime.Today.ToUniversalTime() - TimeSpan.FromDays(30);
            modelFile.WellId = addedESPWell.Id;

            byte[] fileAsByteArray = GetByteArray(Path, model.Item1);
            Assert.IsNotNull(fileAsByteArray);
            modelFile.Base64Contents = Convert.ToBase64String(fileAsByteArray);
            Assert.AreEqual(wellType, ModelFileService.GetWellModelType(modelFile), "Model File type mismatch");
            ModelFileValidationDataDTO ModelFileValidationData = ModelFileService.GetModelFileValidation(modelFile);
            Assert.IsNotNull(ModelFileValidationData);
            ModelFileService.AddWellModelFile(modelFile);
            ModelFileDTO newModelFile = ModelFileService.GetCurrentModelFile(addedESPWell.Id.ToString());
            Assert.IsNotNull(newModelFile);

            //add new wellTestData
            var testDataDTO = new WellTestDTO
            {
                WellId = addedESPWell.Id,
                AverageTubingPressure = 200.00m,
                AverageTubingTemperature = 100.0m,
                Gas = 1.2m,
                GasGravity = 0.65m,
                producedGOR = 0.5m,
                Oil = 2397.5m,
                OilGravity = 34.97m,
                SPTCode = 2,
                SPTCodeDescription = "RepresentativeTest",
                TestDuration = 3,
                Water = 3596.2m,
                WaterGravity = 1.0198m,
                PumpIntakePressure = 1412.25m,
                PumpDischargePressure = 3067.59m,
                FlowLinePressure = 1412.25m,
                SeparatorPressure = 1412.25m,
                ChokeSize = 0M
            };

            testDataDTO.SampleDate = (DateTime.Today.ToUniversalTime() - TimeSpan.FromDays(10));
            WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(addedESPWell.Id.ToString()).Units;
            WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testDataDTO));

            var test = WellTestDataService.GetLatestValidWellTestByWellId(addedESPWell.Id.ToString());

            Assert.AreEqual(WellTestStatus.ACCEPTANCE_LIMITS_RESERVOIR_PRESSURE_TOO_HIGH, test.Value.Status);
            bool status = SurveillanceService.AddDailyAverageFromVHSByDateRange(addedESPWell.Id.ToString(),
                        (DateTime.Today - TimeSpan.FromDays(10)).ToUniversalTime().ToISO8601(),
                        DateTime.Today.ToUniversalTime().ToISO8601());
            Assert.AreEqual(true, status);
            WellDailyAverageArrayAndUnitsDTO dailyAverages =
                        SurveillanceService.GetDailyAverages(addedESPWell.Id.ToString(),
                            (DateTime.Today - TimeSpan.FromDays(10)).ToUniversalTime().ToISO8601(),
                            DateTime.Today.ToUniversalTime().ToISO8601());
            GetProductionKPI();
        }

        [TestCategory(TestCategories.PEDashboardTests), TestMethod]
        public void GetProductionKPI_NF()
        {
            string Path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";
            Tuple<string, WellTypeId, ModelFileOptionDTO> model = Tuple.Create("ProductionTestData.wflx", WellTypeId.NF, new ModelFileOptionDTO() { CalibrationMethod = CalibrationMethodId.ReservoirPressure, OptionalUpdate = new long[] { ((long)OptionalUpdates.UpdateGOR_CGR), (long)OptionalUpdates.UpdateWCT_WGR } });

            string modelFileName = model.Item1;
            WellTypeId wellType = model.Item2;
            ModelFileOptionDTO options = model.Item3;

            AddWell("NFWWELL_0001", WellTypeId.NF);
            WellDTO addedNFWell = WellService.GetAllWells().FirstOrDefault(w => w.Name.Equals("NFWWELL_0001"));

            var myassembly = WellboreComponentService.GetAssemblyByWellId(addedNFWell.Id.ToString());
            Assert.IsNotNull(myassembly);

            ModelFileBase64DTO modelFile = new ModelFileBase64DTO() { };

            options.Comment = "CASETest Upload " + model.Item1;
            modelFile.Options = options;
            modelFile.ApplicableDate = DateTime.Today.ToUniversalTime() - TimeSpan.FromDays(30);
            modelFile.WellId = addedNFWell.Id;

            byte[] fileAsByteArray = GetByteArray(Path, model.Item1);
            Assert.IsNotNull(fileAsByteArray);
            modelFile.Base64Contents = Convert.ToBase64String(fileAsByteArray);
            Assert.AreEqual(wellType, ModelFileService.GetWellModelType(modelFile), "Model File type mismatch");
            ModelFileValidationDataDTO ModelFileValidationData = ModelFileService.GetModelFileValidation(modelFile);
            Assert.IsNotNull(ModelFileValidationData);
            ModelFileService.AddWellModelFile(modelFile);
            ModelFileDTO newModelFile = ModelFileService.GetCurrentModelFile(addedNFWell.Id.ToString());
            Assert.IsNotNull(newModelFile);

            //add new wellTestData
            var testDataDTO = new WellTestDTO
            {
                WellId = addedNFWell.Id,
                AverageTubingPressure = 164.7m,
                AverageTubingTemperature = 100.0m,
                Gas = 880m,
                // GasGravity = 0.65m,
                // producedGOR = 0.5m,
                Oil = 1769.5m,
                SPTCode = 2,
                SPTCodeDescription = "RepresentativeTest",
                TestDuration = 3,
                Water = 589.8m,
                //WaterCut = 0.65m,
                //WaterGravity = 1.0198m,
                //PumpIntakePressure = 1412.25m,
                //PumpDischargePressure = 3067.59m,
                GaugePressure = 1412.25m,
                FlowLinePressure = 1412.25m,
                SeparatorPressure = 1412.25m,
                ChokeSize = 32

            };
            testDataDTO.SampleDate = (DateTime.Today.ToUniversalTime() - TimeSpan.FromDays(10));
            WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(addedNFWell.Id.ToString()).Units;
            WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testDataDTO));

            var test = WellTestDataService.GetLatestValidWellTestByWellId(addedNFWell.Id.ToString());

            Assert.AreEqual(WellTestStatus.TUNING_SUCCEEDED, test.Value.Status);
            bool status = SurveillanceService.AddDailyAverageFromVHSByDateRange(addedNFWell.Id.ToString(),
                        (DateTime.Today - TimeSpan.FromDays(10)).ToUniversalTime().ToISO8601(),
                        DateTime.Today.ToUniversalTime().ToISO8601());
            Assert.AreEqual(true, status);
            WellDailyAverageArrayAndUnitsDTO dailyAverages =
                        SurveillanceService.GetDailyAverages(addedNFWell.Id.ToString(),
                            (DateTime.Today - TimeSpan.FromDays(10)).ToUniversalTime().ToISO8601(),
                            DateTime.Today.ToUniversalTime().ToISO8601());
            GetProductionKPI();
        }

        [TestCategory(TestCategories.PEDashboardTests), TestMethod]
        public void GetProductionKPI_LukinSAM()
        {
            AddWell("SAM_", WellTypeId.RRL);
            GetProductionKPI();
        }

        [TestCategory(TestCategories.PEDashboardTests), TestMethod]
        public void GetProductionKPI_8800()
        {
            AddWell("8800_", WellTypeId.RRL);
            GetProductionKPI();
        }

        [TestCategory(TestCategories.PEDashboardTests), TestMethod]
        public void GetProductionKPI_AEPOC()
        {
            AddWell("AEPOC_", WellTypeId.RRL);
            GetProductionKPI();
        }

        [TestCategory(TestCategories.PEDashboardTests), TestMethod]
        public void GetProductionKPI_EPICFS()
        {
            AddWell("EPICFS_", WellTypeId.RRL);
            GetProductionKPI();
        }

        [TestCategory(TestCategories.PEDashboardTests), TestMethod]
        public void GetAllKPIs_WP()
        {
            AddWell("RPOC_", WellTypeId.RRL);
            GetAllKPIs();
        }

        [TestCategory(TestCategories.PEDashboardTests), TestMethod]
        public void GetAllKPIs_LufkinSAM()
        {
            AddWell("SAM_", WellTypeId.RRL);
            GetAllKPIs();
        }

        [TestCategory(TestCategories.PEDashboardTests), TestMethod]
        public void GetAllKPIs_8800()
        {
            AddWell("8800_", WellTypeId.RRL);
            GetAllKPIs();
        }

        [TestCategory(TestCategories.PEDashboardTests), TestMethod]
        public void GetAllKPIs_AEPOC()
        {
            AddWell("AEPOC_", WellTypeId.RRL);
            GetAllKPIs();
        }

        [TestCategory(TestCategories.PEDashboardTests), TestMethod]
        public void GetAllKPIs_EPICFS()
        {
            AddWell("EPICFS_", WellTypeId.RRL);
            GetAllKPIs();
        }

        [TestCategory(TestCategories.PEDashboardTests), TestMethod]
        public void GetAllKPIs_ESP()
        {
            AddWell("ESPWELL_", WellTypeId.ESP);
            GetAllKPIs();
        }

        [TestCategory(TestCategories.PEDashboardTests), TestMethod]
        public void GetAllKPIs_NF()
        {
            AddWell("NFWWELL_", WellTypeId.NF);
            GetAllKPIs();
        }

        [TestCategory(TestCategories.PEDashboardTests), TestMethod]
        public void GetAllKPIs_AllWellTypes_WSMLicenseEnabled()
        {
            AddWell("RPOC_", WellTypeId.RRL);
            AddWell(GetFacilityId("ESPWELL_", 1), WellTypeId.ESP);
            AddWell(GetFacilityId("GLWELL_", 1), WellTypeId.GLift);
            AddWell(GetFacilityId("NFWWELL_", 1), WellTypeId.NF);
            GetAllKPIs();
        }

        [TestCategory(TestCategories.PEDashboardTests), TestMethod]
        public void GetAllKPIs_AllWellTypes_WSMLicenseDisabled()
        {
            // disable WSM license, but don't persist setting beyond this test
            List<SystemFeature> disableLicenses = new List<SystemFeature>();
            disableLicenses.Add(SystemFeature.WSM);  // false = don't persist

            AddWell("RPOC_", WellTypeId.RRL);
            AddWell("ESPWELL_0001", WellTypeId.ESP);
            AddWell(GetFacilityId("GLWELL_", 1), WellTypeId.GLift);
            AddWell("NFWWELL_0001", WellTypeId.NF);
            LicenseService.DisableLicenses(disableLicenses);
            GetAllKPIs(true);
            LicenseService.ClearDisabledLicenses();
        }

        [TestCategory(TestCategories.PEDashboardTests), TestMethod]
        public void GetAllTrends_WP()
        {
            AddWell("RPOC_", WellTypeId.RRL);
            GetAllTrends();
        }

        [TestCategory(TestCategories.PEDashboardTests), TestMethod]
        public void GetAllTrends1()
        {
            GetAllTrends();
        }

        [TestCategory(TestCategories.PEDashboardTests), TestMethod]
        public void GetAllTrends_LufkinSAM()
        {
            AddWell("SAM_", WellTypeId.RRL);
            GetAllTrends();
        }

        [TestCategory(TestCategories.PEDashboardTests), TestMethod]
        public void GetAllTrends_8800()
        {
            AddWell("8800_", WellTypeId.RRL);
            GetAllTrends();
        }

        [TestCategory(TestCategories.PEDashboardTests), TestMethod]
        public void GetAllTrends_AEPOC()
        {
            AddWell("AEPOC_", WellTypeId.RRL);
            GetAllTrends();
        }

        [TestCategory(TestCategories.PEDashboardTests), TestMethod]
        public void GetAllTrends_EPICFS()
        {
            AddWell("EPICFS_", WellTypeId.RRL);
            GetAllTrends();
        }

        [TestCategory(TestCategories.PEDashboardTests), TestMethod]
        public void GetDownWellsKPI_WP()
        {
            AddWell("RPOC_", WellTypeId.RRL);
            GetDownWellsKPI();
        }

        [TestCategory(TestCategories.PEDashboardTests), TestMethod]
        public void GetDownWellsKPI_LufkinSAM()
        {
            AddWell("SAM_", WellTypeId.RRL);
            GetDownWellsKPI();
        }

        [TestCategory(TestCategories.PEDashboardTests), TestMethod]
        public void GetDownWellsKPI_8800()
        {
            AddWell("8800_", WellTypeId.RRL);
            GetDownWellsKPI();
        }

        [TestCategory(TestCategories.PEDashboardTests), TestMethod]
        public void GetDownWellsKPI_AEPOC()
        {
            AddWell("AEPOC_", WellTypeId.RRL);
            GetDownWellsKPI();
        }

        [TestCategory(TestCategories.PEDashboardTests), TestMethod]
        public void GetDownWellsKPI_EPICFS()
        {
            AddWell("EPICFS_", WellTypeId.RRL);
            GetDownWellsKPI();
        }

        [TestCategory(TestCategories.PEDashboardTests), TestMethod]
        public void GetDownWellsKPI_AllWellTypes()
        {
            AddWell("RPOC_", WellTypeId.RRL);
            AddWell("ESPWELL_0001", WellTypeId.ESP);
            AddWell(GetFacilityId("GLWELL_", 1), WellTypeId.GLift);
            AddWell("NFWWELL_0001", WellTypeId.GLift);
            GetDownWellsKPI();
            GetDownWellsKPIWithWSMEnabled();
        }

        [TestCategory(TestCategories.PEDashboardTests), TestMethod]
        public void GetAlarmWellsKPI_WP()
        {
            AddWell("RPOC_", WellTypeId.RRL);
            GetAlarmWellsKPI();
        }

        [TestCategory(TestCategories.PEDashboardTests), TestMethod]
        public void GetAlarmWellsKPI_LufkinSAM()
        {
            AddWell("SAM_", WellTypeId.RRL);
            GetAlarmWellsKPI();
        }

        [TestCategory(TestCategories.PEDashboardTests), TestMethod]
        public void GetAlarmWellsKPI_8800()
        {
            AddWell("8800_", WellTypeId.RRL);
            GetAlarmWellsKPI();
        }

        [TestCategory(TestCategories.PEDashboardTests), TestMethod]
        public void GetAlarmWellsKPI_AEPOC()
        {
            AddWell("AEPOC_", WellTypeId.RRL);
            GetAlarmWellsKPI();
        }

        [TestCategory(TestCategories.PEDashboardTests), TestMethod]
        public void GetAlarmWellsKPI_EPICFS()
        {
            AddWell("EPICFS_", WellTypeId.RRL);
            GetAlarmWellsKPI();
        }

        public string AddWellwithAssembly(string facilityIdBase)
        {
            string facilityId = s_isRunningInATS ? facilityIdBase + "00001" : facilityIdBase + "0001";
            WellConfigurationService.AddWellConfig(new WellConfigDTO()
            {
                Well = SetDefaultFluidType(new WellDTO()
                {
                    Name = DefaultWellName,
                    FacilityId = facilityId,
                    DataConnection = GetDefaultCygNetDataConnection(),
                    SubAssemblyAPI = "SubAssemblyAPI",
                    AssemblyAPI = "AssemblyAPI",
                    IntervalAPI = "IntervalAPI",
                    CommissionDate = DateTime.Today,
                    WellType = WellTypeId.RRL,
                })
            });
            List<WellDTO> allWells = WellService.GetAllWells().ToList();
            WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName));
            Assert.IsNotNull(well);
            _wellsToRemove.Add(well);
            return well.Id.ToString();
        }

        public string AddJob(string jobStatus)
        {
            WellDTO[] allWells = WellService.GetAllWells();
            WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName));

            JobLightDTO job = new JobLightDTO();
            job.WellId = well.Id;
            job.WellName = well.Name;
            job.BeginDate = DateTime.Today.AddDays(0).ToUniversalTime();
            job.EndDate = DateTime.Today.AddDays(30).ToUniversalTime();
            job.ActualCost = (decimal)100000.00;
            job.ActualJobDurationDays = (decimal)20.5;
            job.TotalCost = (decimal)150000.00;
            job.JobRemarks = "TestJobRemarks " + DateTime.UtcNow.ToString();
            job.JobOrigin = "TestJobOrigin ";
            job.AssemblyId = well.AssemblyId;
            //For Below fields User can select any value in the dropdown menu
            job.AFEId = JobAndEventService.GetAFEs().FirstOrDefault().Id;
            job.StatusId = JobAndEventService.GetJobStatuses().FirstOrDefault(j => j.Name == jobStatus).Id;
            job.JobTypeId = JobAndEventService.GetJobTypes().FirstOrDefault(x => x.JobType == "Artificial Lift Revision").id;
            job.BusinessOrganizationId = JobAndEventService.GetCatalogItemGroupData().Vendors.FirstOrDefault().Id;
            job.AccountRef = "1";
            //JobReasonId drop down selection is based on the JobTypeId
            Trace.WriteLine("Job Type: " + job.JobTypeId.ToString());
            job.JobReasonId = JobAndEventService.GetJobReasonsForJobType(job.JobTypeId.ToString()).FirstOrDefault().Id;
            job.JobDriverId = (int)JobAndEventService.GetPrimaryMotivationsForJob().FirstOrDefault().Id;
            //Add Job
            string addJob = JobAndEventService.AddJob(job);
            Assert.IsNotNull(addJob, "Failed to add a Job");
            //Get Job
            JobLightDTO getJob = JobAndEventService.GetJobById(addJob);
            Assert.AreEqual(job.WellId, getJob.WellId);
            Assert.AreEqual(job.WellName, getJob.WellName);
            Assert.AreEqual(job.BeginDate, getJob.BeginDate);
            Assert.AreEqual(job.EndDate, getJob.EndDate);
            Assert.AreEqual(job.ActualCost, getJob.ActualCost);
            Assert.AreEqual(job.ActualJobDurationDays, getJob.ActualJobDurationDays);
            Assert.AreEqual(job.TotalCost, getJob.TotalCost);
            Assert.AreEqual(job.JobRemarks, getJob.JobRemarks);
            Assert.AreEqual(job.JobOrigin, getJob.JobOrigin);
            Assert.AreEqual(job.AssemblyId, getJob.AssemblyId);
            Assert.AreEqual(job.AFEId, getJob.AFEId);
            Assert.AreEqual(job.StatusId, getJob.StatusId);
            Assert.AreEqual(job.JobTypeId, getJob.JobTypeId);
            Assert.AreEqual(job.BusinessOrganizationId, job.BusinessOrganizationId);
            Assert.AreEqual(job.AccountRef, getJob.AccountRef);
            Assert.AreEqual(job.JobReasonId, getJob.JobReasonId);
            Assert.AreEqual(job.JobDriverId, getJob.JobDriverId);
            return addJob;
        }

        [TestCategory(TestCategories.PEDashboardTests), TestMethod]
        public void PendingJobs()
        {
            string wId = AddWellwithAssembly("RPOC_");

            //Get Wells by Filter
            WellFilterDTO wellbyFilter = new WellFilterDTO();

            JobStatusDTO[] js = JobAndEventService.GetJobStatuses();
            for (int i = 0; i < js.Count(); i++)
            {
                if (js[i].Name == "Approved" || js[i].Name == "Planned" || js[i].Name == "In Progress")
                {
                    string jobId = AddJob(js[i].Name);
                    bool check = JobAndEventService.IsJobApprovable(jobId);
                    Trace.WriteLine(js[i].Name + "---" + check.ToString());
                }
            }
            JobStatusDTO[] statuses = JobAndEventService.GetJobStatuses();
            var statusNames = new HashSet<string>() { "Approved", "Planned", "In Progress" };
            List<long> statusesForCurrentValueCalculation = statuses.Where(t => statusNames.Contains(t.Name)).Select(t => t.Id).ToList();
            JobLightDTO[] jobs = JobAndEventService.GetJobsByWell(wId).Where(x => statusesForCurrentValueCalculation.Contains(x.StatusId)).ToArray();

            //No Filters selected
            PEKPIAllDTO allKpis = PEDashboardService.GetAllKPIs(wellbyFilter);
            //Pending Jobs
            double currentValue = allKpis.Jobs.CurrentValue;
            double changeValue = allKpis.Jobs.ChangeValue;
            double lastValue = allKpis.Jobs.LastValue;
            string changePercent = allKpis.Jobs.ChangePercentage.ToString();
            if (lastValue != 0)
            {
                string expPercent = Math.Round(((changeValue / lastValue) * 100), 1).ToString();
                Assert.AreEqual(expPercent, changePercent);
            }

            double NewWells = allKpis.Jobs.AdditionInCurrentKPI;
            double changeLastKPI = allKpis.Jobs.ChangeForLastKPI;
            double expchangeLastKPI = (currentValue - lastValue) - NewWells;
            bool setAlarmlow = currentValue > allKpis.Jobs.AlarmSetpointLow;
            bool setAlarmhi = currentValue < allKpis.Jobs.AlarmSetpointHi;
            bool indAlarm = setAlarmhi && setAlarmlow;
            string jobColourCode = allKpis.Jobs.ColorCode;
            Assert.AreEqual(currentValue - lastValue, changeValue);
            Assert.AreEqual(expchangeLastKPI, changeLastKPI);
            Assert.AreEqual(jobs.Count(), currentValue);
        }


        [TestCategory(TestCategories.PEDashboardTests), TestMethod]
        public void VerifyPEDashboardforNonRRLPhases()
        {

            try
            {
                //Add 2 New Wells in system ( NonRRL Wells of Type ESP,GL ,PGL, NFW)

                for (int i = 0; i < 2; i++)
                {

                    string facidesp = GetFacilityId("ESPWELL_", (i + 1));
                    WellDTO espwelldto = AddNonRRLWell(facidesp, WellTypeId.ESP, false);
                    //Add Daily Average Data with delta of 10 , for Yest and Last Yest Dates
                    AddDailyAvgForESP(espwelldto, 0, 10);
                    AddDailyAvgForESP(espwelldto, 1, 0);

                    string facidgl = GetFacilityId("GLWELL_", (i + 1));
                    WellDTO glwelldto = AddNonRRLWell(facidgl, WellTypeId.GLift, false);
                    //Add Daily Average Data with delta of 10 , for Yest and Last Yest Dates
                    AddDailyAvgForGL(glwelldto, 0, 10);
                    AddDailyAvgForGL(glwelldto, 1, 0);


                    string facidpgl = GetFacilityId("PGLWELL_", (i + 1));
                    WellDTO pglwelldto = AddNonRRLWell(facidpgl, WellTypeId.PLift, false);
                    //Add Daily Average Data with delta of 10 , for Yest and Last Yest Dates
                    AddDailyAvgForPGL(pglwelldto, 0, 10);
                    AddDailyAvgForPGL(pglwelldto, 1, 0);


                    string facidnfw = GetFacilityId("NFWWELL_", (i + 1));
                    WellDTO nfwwelldto = AddNonRRLWell(facidnfw, WellTypeId.NF, false);
                    //Add Daily Average Data with delta of 10 , for Yest and Last Yest Dates
                    AddDailyAvgForNF(nfwwelldto, 0, 10);
                    AddDailyAvgForNF(nfwwelldto, 1, 0);
                }

                //Get well Filters
                WellFilterDTO filters = WellService.GetWellFilter(null);

                WellFilterDTO wellbyFilter = new WellFilterDTO();
                //Filter Non RRL Wells , in this case only ESP Wells
                #region AsertionforESPPhases
                wellbyFilter.welFK_r_WellTypeValues = new[] {
                             new WellFilterValueDTO { Value = WellTypeId.ESP.ToString() } };
                PEKPIAllDTO prodAllKPI = PEDashboardService.GetAllKPIs(wellbyFilter);

                //Verify KPI Values when Only Non RRL Type is selected in this case ESP 
                //verify Oil Phase Values with Assertions as we know what is expected for given input Dailyavg for NonRRL
                //Verify for ESP


                #endregion

                #region AsertionforGLPhases
                wellbyFilter.welFK_r_WellTypeValues = new[] {
                             new WellFilterValueDTO { Value = WellTypeId.GLift.ToString() } };
                prodAllKPI = PEDashboardService.GetAllKPIs(wellbyFilter);

                //Verify KPI Values when Only Non RRL Type is selected in this case ESP 
                //verify Oil Phase Values with Assertions as we know what is expected for given input Dailyavg for NonRRL
                //Verify for GL

                AssertPhaseValues("GL", prodAllKPI);
                #endregion

                #region AsertionforPGLPhases
                wellbyFilter.welFK_r_WellTypeValues = new[] {
                             new WellFilterValueDTO { Value = WellTypeId.PLift.ToString() } };
                prodAllKPI = PEDashboardService.GetAllKPIs(wellbyFilter);

                //Verify KPI Values when Only Non RRL Type is selected in this case ESP 
                //verify Oil Phase Values with Assertions as we know what is expected for given input Dailyavg for NonRRL
                //Verify for PGL Wells

                AssertPhaseValues("PGL", prodAllKPI);
                #endregion

                #region AsertionforNFPhases
                wellbyFilter.welFK_r_WellTypeValues = new[] {
                             new WellFilterValueDTO { Value = WellTypeId.NF.ToString() }
                            };
                prodAllKPI = PEDashboardService.GetAllKPIs(wellbyFilter);
                //Verify KPI Values when Only Non RRL Type is selected in this case ESP 
                //verify Oil Phase Values with Assertions as we know what is expected for given input Dailyavg for NonRRL
                //Verify for NFW Wells
                AssertPhaseValues("NFW", prodAllKPI);
                #endregion


                #region AsserForAllPhases 
                string phase = string.Empty;
                wellbyFilter.welFK_r_WellTypeValues = new[] {
                             new WellFilterValueDTO { Value = WellTypeId.ESP.ToString() } ,new WellFilterValueDTO { Value = WellTypeId.GLift.ToString() },
                             new WellFilterValueDTO { Value = WellTypeId.PLift.ToString() },
                             new WellFilterValueDTO { Value = WellTypeId.NF.ToString() }
                            };
                prodAllKPI = PEDashboardService.GetAllKPIs(wellbyFilter);
                Trace.WriteLine(prodAllKPI.OilProduction.CurrentValue);
                phase = "Oil";
                Assert.AreEqual(1680, prodAllKPI.OilProduction.CurrentValue, string.Format("{0} All Production Current Value mismatch", phase));
                Assert.AreEqual(1600, prodAllKPI.OilProduction.LastValue, string.Format("{0} All Production Last  Value mismatch", phase));
                Assert.AreEqual(80, prodAllKPI.OilProduction.ChangeValue, string.Format("{0} All Production Change  Value mismatch", phase));
                Assert.AreEqual(5, prodAllKPI.OilProduction.ChangePercentage, string.Format("{0} All Production Change PercentageValue mismatch", phase));
                phase = "Water";
                Assert.AreEqual(880, prodAllKPI.WaterProduction.CurrentValue, string.Format("{0} All Production Current Value mismatch", phase));
                Assert.AreEqual(800, prodAllKPI.WaterProduction.LastValue, string.Format("{0} All Production Last  Value mismatch", phase));
                Assert.AreEqual(80, prodAllKPI.WaterProduction.ChangeValue, string.Format("{0} All Production Change  Value mismatch", phase));
                Assert.AreEqual(10, prodAllKPI.WaterProduction.ChangePercentage, string.Format("{0} All Production Change PercentageValue mismatch", phase));
                phase = "Gas";
                Assert.AreEqual(280, prodAllKPI.GasProduction.CurrentValue, string.Format("{0} All Production Current Value mismatch", phase));
                Assert.AreEqual(200, prodAllKPI.GasProduction.LastValue, string.Format("{0} All Production Last  Value mismatch", phase));
                Assert.AreEqual(80, prodAllKPI.GasProduction.ChangeValue, string.Format("{0} All Production Change  Value mismatch", phase));
                Assert.AreEqual(40, prodAllKPI.GasProduction.ChangePercentage, string.Format("{0} All Production Change PercentageValue mismatch", phase));
                phase = "Liquid";
                Assert.AreEqual(2560, prodAllKPI.Production.CurrentValue, string.Format("{0} All Production Current Value mismatch", phase));
                Assert.AreEqual(2400, prodAllKPI.Production.LastValue, string.Format("{0} All Production Last  Value mismatch", phase));
                Assert.AreEqual(160, prodAllKPI.Production.ChangeValue, string.Format("{0}All Production Change  Value mismatch", phase));
                Assert.AreEqual(6.7, prodAllKPI.Production.ChangePercentage, string.Format("{0} All Production Change PercentageValue mismatch", phase));
                #endregion
                //Verify that the Last 2 Trend points differnce is the KPI value for phases
                var peTrendsAndUnits = PEDashboardService.GetAllTrends(wellbyFilter);
                PETrendPoint[] allTrends = peTrendsAndUnits.Values;
                DateTime startDate = allTrends[0].Timestamp;
                //Trend Chart Values with KPI for Liquid
                //FRI-2020  Shifting days by -1 as per latest Code 
                double? lqtodyval = allTrends.FirstOrDefault(x => x.Timestamp.ToLocalTime().ToShortDateString() == DateTime.Today.AddDays(-1).ToShortDateString()).Production;
                double? lqyestday = allTrends.FirstOrDefault(x => x.Timestamp.ToLocalTime().ToShortDateString() == DateTime.Today.AddDays(-2).ToShortDateString()).Production;
                Assert.AreEqual(prodAllKPI.Production.ChangePercentage, (double)((lqtodyval - lqyestday) / lqyestday) * 100, 0.5, "Mismatch in Liquid Prod between trend date today and yestday  ");
                //Trend Chart Values with KPI for Oil
                double? oiltodyval = allTrends.FirstOrDefault(x => x.Timestamp.ToLocalTime().ToShortDateString() == DateTime.Today.AddDays(-1).ToShortDateString()).OilProduction;
                double? oilyestday = allTrends.FirstOrDefault(x => x.Timestamp.ToLocalTime().ToShortDateString() == DateTime.Today.AddDays(-2).ToShortDateString()).OilProduction;
                Assert.AreEqual(prodAllKPI.OilProduction.ChangePercentage, (double)((oiltodyval - oilyestday) / oilyestday) * 100, 0.5, "Mismatch in  Oil Prod between trend date today and yestday ");
                //Trend Chart Values with KPI for Water
                double? wttodyval = allTrends.FirstOrDefault(x => x.Timestamp.ToLocalTime().ToShortDateString() == DateTime.Today.AddDays(-1).ToShortDateString()).WaterProduction;
                double? wtqyestday = allTrends.FirstOrDefault(x => x.Timestamp.ToLocalTime().ToShortDateString() == DateTime.Today.AddDays(-2).ToShortDateString()).WaterProduction;
                Assert.AreEqual(prodAllKPI.WaterProduction.ChangePercentage, (double)((wttodyval - wtqyestday) / wtqyestday) * 100, 0.5, "Mismatch in Water Prod between trend date today and yestday ");
                //Trend Chart Values with KPI for Gas
                double? gstodyval = allTrends.FirstOrDefault(x => x.Timestamp.ToLocalTime().ToShortDateString() == DateTime.Today.AddDays(-1).ToShortDateString()).GasProduction;
                double? gsyestday = allTrends.FirstOrDefault(x => x.Timestamp.ToLocalTime().ToShortDateString() == DateTime.Today.AddDays(-2).ToShortDateString()).GasProduction;
                Assert.AreEqual(prodAllKPI.GasProduction.ChangePercentage, (double)((gstodyval - gsyestday) / gsyestday) * 100, 0.5, "Mismatch in Gas Prod between trend date today and yestday");


            }
            finally
            {
                for (int i = 0; i < 2; i++)
                {
                    string facidesp = GetFacilityId("ESPWELL_", (i + 1));
                    RemoveWell(facidesp);
                    string facidgl = GetFacilityId("GLWELL_", (i + 1));
                    RemoveWell(facidgl);
                    string facidpgl = GetFacilityId("PGLWELL_", (i + 1));
                    RemoveWell(facidpgl);
                    string facidnfw = GetFacilityId("NFWWELL_", (i + 1));
                    RemoveWell(facidnfw);
                }
            }


        }

        [TestCategory(TestCategories.PEDashboardTests), TestMethod]
        public void VerifyPEDashboardforRRLAndNonRRLPhases()
        {

            try
            {
                //Add 2 New Wells in system ( NonRRL Wells of Type ESP,GL ,PGL, NFW)
                #region PEDashboardTEst
                for (int i = 0; i < 2; i++)
                {

                    string facidesp = GetFacilityId("ESPWELL_", (i + 1));
                    WellDTO espwelldto = AddNonRRLWell(facidesp, WellTypeId.ESP, false);
                    //Add Daily Average Data with delta of 10 , for Yest and Last Yest Dates
                    AddDailyAvgForESP(espwelldto, 0, 10);
                    AddDailyAvgForESP(espwelldto, 1, 0);

                    string facidgl = GetFacilityId("GLWELL_", (i + 1));
                    WellDTO glwelldto = AddNonRRLWell(facidgl, WellTypeId.GLift, false);
                    //Add Daily Average Data with delta of 10 , for Yest and Last Yest Dates
                    AddDailyAvgForGL(glwelldto, 0, 10);
                    AddDailyAvgForGL(glwelldto, 1, 0);


                    string facidpgl = GetFacilityId("PGLWELL_", (i + 1));
                    WellDTO pglwelldto = AddNonRRLWell(facidpgl, WellTypeId.PLift, false);
                    //Add Daily Average Data with delta of 10 , for Yest and Last Yest Dates
                    AddDailyAvgForPGL(pglwelldto, 0, 10);
                    AddDailyAvgForPGL(pglwelldto, 1, 0);


                    string facidnfw = GetFacilityId("NFWWELL_", (i + 1));
                    WellDTO nfwwelldto = AddNonRRLWell(facidnfw, WellTypeId.NF, false);
                    //Add Daily Average Data with delta of 10 , for Yest and Last Yest Dates
                    AddDailyAvgForNF(nfwwelldto, 0, 10);
                    AddDailyAvgForNF(nfwwelldto, 1, 0);


                    string facidrrl = GetFacilityId("RPOC_", (i + 1));
                    WellDTO rrlwelldto = AddRRLWell(facidrrl);
                    //Add Daily Average Data with delta of 10 , for Yest and Last Yest Dates
                    AddDailyAvgForRRL(rrlwelldto, 0, 10);
                    AddDailyAvgForRRL(rrlwelldto, 1, 0);
                }

                //Get well Filters
                WellFilterDTO filters = WellService.GetWellFilter(null);

                WellFilterDTO wellbyFilter = new WellFilterDTO();
                //Filter Non RRL Wells , in this case only ESP Wells
                #region AsertionforESPPhases
                wellbyFilter.welFK_r_WellTypeValues = new[] {
                             new WellFilterValueDTO { Value = WellTypeId.ESP.ToString() } };
                PEKPIAllDTO prodAllKPI = PEDashboardService.GetAllKPIs(wellbyFilter);

                //Verify KPI Values when Only Non RRL Type is selected in this case ESP 
                //verify Oil Phase Values with Assertions as we know what is expected for given input Dailyavg for NonRRL
                //Verify for ESP


                #endregion

                #region AsertionforGLPhases
                wellbyFilter.welFK_r_WellTypeValues = new[] {
                             new WellFilterValueDTO { Value = WellTypeId.GLift.ToString() } };
                prodAllKPI = PEDashboardService.GetAllKPIs(wellbyFilter);

                //Verify KPI Values when Only Non RRL Type is selected in this case ESP 
                //verify Oil Phase Values with Assertions as we know what is expected for given input Dailyavg for NonRRL
                //Verify for GL

                AssertPhaseValues("GL", prodAllKPI);
                #endregion

                #region AsertionforPGLPhases
                wellbyFilter.welFK_r_WellTypeValues = new[] {
                             new WellFilterValueDTO { Value = WellTypeId.PLift.ToString() } };
                prodAllKPI = PEDashboardService.GetAllKPIs(wellbyFilter);

                //Verify KPI Values when Only Non RRL Type is selected in this case ESP 
                //verify Oil Phase Values with Assertions as we know what is expected for given input Dailyavg for NonRRL
                //Verify for PGL Wells

                AssertPhaseValues("PGL", prodAllKPI);
                #endregion

                #region AsertionforNFPhases
                wellbyFilter.welFK_r_WellTypeValues = new[] {
                             new WellFilterValueDTO { Value = WellTypeId.NF.ToString() }
                            };
                prodAllKPI = PEDashboardService.GetAllKPIs(wellbyFilter);
                //Verify KPI Values when Only Non RRL Type is selected in this case ESP 
                //verify Oil Phase Values with Assertions as we know what is expected for given input Dailyavg for NonRRL
                //Verify for NFW Wells
                AssertPhaseValues("NFW", prodAllKPI);
                #endregion

                #region AsertionforRRLPhases
                wellbyFilter.welFK_r_WellTypeValues = new[] {
                             new WellFilterValueDTO { Value = WellTypeId.RRL.ToString() }
                            };
                prodAllKPI = PEDashboardService.GetAllKPIs(wellbyFilter);
                //Verify KPI Values when Only Non RRL Type is selected in this case ESP 
                //verify Oil Phase Values with Assertions as we know what is expected for given input Dailyavg for NonRRL
                //Verify for NFW Wells
                AssertPhaseValues("RRL", prodAllKPI);
                #endregion

                #region AsserForAllPhases 
                string phase = string.Empty;
                wellbyFilter.welFK_r_WellTypeValues = new[] {
                             new WellFilterValueDTO { Value = WellTypeId.ESP.ToString() } ,new WellFilterValueDTO { Value = WellTypeId.GLift.ToString() },
                             new WellFilterValueDTO { Value = WellTypeId.PLift.ToString() },
                             new WellFilterValueDTO { Value = WellTypeId.RRL.ToString() },
                             new WellFilterValueDTO { Value = WellTypeId.NF.ToString() }
                            };
                prodAllKPI = PEDashboardService.GetAllKPIs(wellbyFilter);
                Trace.WriteLine(prodAllKPI.OilProduction.CurrentValue);
                phase = "Oil";
                Assert.AreEqual(2100, prodAllKPI.OilProduction.CurrentValue, string.Format("{0} All Production Current Value mismatch", phase));
                Assert.AreEqual(2000, prodAllKPI.OilProduction.LastValue, string.Format("{0} All Production Last  Value mismatch", phase));
                Assert.AreEqual(100, prodAllKPI.OilProduction.ChangeValue, string.Format("{0} All Production Change  Value mismatch", phase));
                Assert.AreEqual(5, prodAllKPI.OilProduction.ChangePercentage, string.Format("{0} All Production Change PercentageValue mismatch", phase));
                phase = "Water";
                Assert.AreEqual(1100, prodAllKPI.WaterProduction.CurrentValue, string.Format("{0} All Production Current Value mismatch", phase));
                Assert.AreEqual(1000, prodAllKPI.WaterProduction.LastValue, string.Format("{0} All Production Last  Value mismatch", phase));
                Assert.AreEqual(100, prodAllKPI.WaterProduction.ChangeValue, string.Format("{0} All Production Change  Value mismatch", phase));
                Assert.AreEqual(10, prodAllKPI.WaterProduction.ChangePercentage, string.Format("{0} All Production Change PercentageValue mismatch", phase));
                phase = "Gas";
                Assert.AreEqual(350, prodAllKPI.GasProduction.CurrentValue, string.Format("{0} All Production Current Value mismatch", phase));
                Assert.AreEqual(250, prodAllKPI.GasProduction.LastValue, string.Format("{0} All Production Last  Value mismatch", phase));
                Assert.AreEqual(100, prodAllKPI.GasProduction.ChangeValue, string.Format("{0} All Production Change  Value mismatch", phase));
                Assert.AreEqual(40, prodAllKPI.GasProduction.ChangePercentage, string.Format("{0} All Production Change PercentageValue mismatch", phase));
                phase = "Liquid";
                Assert.AreEqual(3200, prodAllKPI.Production.CurrentValue, string.Format("{0} All Production Current Value mismatch", phase));
                Assert.AreEqual(3000, prodAllKPI.Production.LastValue, string.Format("{0} All Production Last  Value mismatch", phase));
                Assert.AreEqual(200, prodAllKPI.Production.ChangeValue, string.Format("{0}All Production Change  Value mismatch", phase));
                Assert.AreEqual(6.7, prodAllKPI.Production.ChangePercentage, string.Format("{0} All Production Change PercentageValue mismatch", phase));
                #endregion
                //Verify that the Last 2 Trend points differnce is the KPI value for phases
                var peTrendsAndUnits = PEDashboardService.GetAllTrends(wellbyFilter);
                PETrendPoint[] allTrends = peTrendsAndUnits.Values;
                DateTime startDate = allTrends[0].Timestamp;
                //Trend Chart Values with KPI for Liquid
                //FRI-2020 shifting dates by 1 days back as per Samuel Change
                double? lqtodyval = allTrends.FirstOrDefault(x => x.Timestamp.ToLocalTime().ToShortDateString() == DateTime.Today.AddDays(-1).ToShortDateString()).Production;
                double? lqyestday = allTrends.FirstOrDefault(x => x.Timestamp.ToLocalTime().ToShortDateString() == DateTime.Today.AddDays(-2).ToShortDateString()).Production;
                Assert.AreEqual(prodAllKPI.Production.ChangePercentage, (double)((lqtodyval - lqyestday) / lqyestday) * 100, 0.5, "Mismatch in Liquid Prod between trend date today and yestday  ");
                //Trend Chart Values with KPI for Oil
                double? oiltodyval = allTrends.FirstOrDefault(x => x.Timestamp.ToLocalTime().ToShortDateString() == DateTime.Today.AddDays(-1).ToShortDateString()).OilProduction;
                double? oilyestday = allTrends.FirstOrDefault(x => x.Timestamp.ToLocalTime().ToShortDateString() == DateTime.Today.AddDays(-2).ToShortDateString()).OilProduction;
                Assert.AreEqual(prodAllKPI.OilProduction.ChangePercentage, (double)((oiltodyval - oilyestday) / oilyestday) * 100, 0.5, "Mismatch in  Oil Prod between trend date today and yestday ");
                //Trend Chart Values with KPI for Water
                double? wttodyval = allTrends.FirstOrDefault(x => x.Timestamp.ToLocalTime().ToShortDateString() == DateTime.Today.AddDays(-1).ToShortDateString()).WaterProduction;
                double? wtqyestday = allTrends.FirstOrDefault(x => x.Timestamp.ToLocalTime().ToShortDateString() == DateTime.Today.AddDays(-2).ToShortDateString()).WaterProduction;
                Assert.AreEqual(prodAllKPI.WaterProduction.ChangePercentage, (double)((wttodyval - wtqyestday) / wtqyestday) * 100, 0.5, "Mismatch in Water Prod between trend date today and yestday ");
                //Trend Chart Values with KPI for Gas
                double? gstodyval = allTrends.FirstOrDefault(x => x.Timestamp.ToLocalTime().ToShortDateString() == DateTime.Today.AddDays(-1).ToShortDateString()).GasProduction;
                double? gsyestday = allTrends.FirstOrDefault(x => x.Timestamp.ToLocalTime().ToShortDateString() == DateTime.Today.AddDays(-2).ToShortDateString()).GasProduction;
                Assert.AreEqual(prodAllKPI.GasProduction.ChangePercentage, (double)((gstodyval - gsyestday) / gsyestday) * 100, 0.5, "Mismatch in Gas Prod between trend date today and yestday");
                #endregion
                // Verify Data correctnesss in production tile map for Percent and Volume
                #region ProductionChangeTileMapVerificiation
                TileMapFilterDTO prodchangetilemap = new TileMapFilterDTO
                {
                    IsNegative = false,
                    UsePercentChangeForRank = true,
                    TopWellCountRequested = 5,
                    DateRequested = DateTime.Today.ToUniversalTime()
                };
                ProductionChangeTileMapAndUnitsDTO tilemapvalueunitdto = SurveillanceService.GetProductionChangeTileMap(prodchangetilemap);
                string actualjosnstring = JsonConvert.SerializeObject(tilemapvalueunitdto);
                string Path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";
                string productiontilemapjson = "ProductionTileMapResponsePercent.json";
                string expectedjsonstring = GetJsonString(Path + productiontilemapjson);
                //Verification for Based on Percentage
                JObject expJobject = JObject.Parse(expectedjsonstring);
                JObject actJobject = JObject.Parse(actualjosnstring);
                Assert.IsTrue(CompareJsonFiles(expJobject, actJobject, "TopGasChangeRate", null, "WellId"), "Compare Json values was false");
                Assert.IsTrue(CompareJsonFiles(expJobject, actJobject, "TopLiquidChangeRate", null, "WellId"), "Compare Json values was false");
                Assert.IsTrue(CompareJsonFiles(expJobject, actJobject, "TopOilChangeRate", null, "WellId"), "Compare Json values was false");
                Assert.IsTrue(CompareJsonFiles(expJobject, actJobject, "TopWaterChangeRate", null, "WellId"), "Compare Json values was false");
                Assert.IsTrue(CompareJsonFiles(expJobject, actJobject, null, "TotalGasChangeRate", ""), "Compare Json values was false");
                Assert.IsTrue(CompareJsonFiles(expJobject, actJobject, null, "TotalLiquidChangeRate", ""), "Compare Json values was false");
                Assert.IsTrue(CompareJsonFiles(expJobject, actJobject, null, "TotalOilChangeRate", ""), "Compare Json values was false");
                Assert.IsTrue(CompareJsonFiles(expJobject, actJobject, null, "TotalWaterChangeRate", ""), "Compare Json values was false");
                //Verification for Based on Volume
                productiontilemapjson = "ProductionTileMapResponseVolume.json";
                expectedjsonstring = GetJsonString(Path + productiontilemapjson);
                prodchangetilemap = new TileMapFilterDTO
                {
                    IsNegative = false,
                    UsePercentChangeForRank = false,
                    TopWellCountRequested = 5,
                    DateRequested = DateTime.Today.ToUniversalTime()
                };
                tilemapvalueunitdto = SurveillanceService.GetProductionChangeTileMap(prodchangetilemap);
                actualjosnstring = JsonConvert.SerializeObject(tilemapvalueunitdto);

                expJobject = JObject.Parse(expectedjsonstring);
                actJobject = JObject.Parse(actualjosnstring);
                Assert.IsTrue(CompareJsonFiles(expJobject, actJobject, "TopGasChangeRate", null, "WellId"), "Compare Json values was false");
                Assert.IsTrue(CompareJsonFiles(expJobject, actJobject, "TopLiquidChangeRate", null, "WellId"), "Compare Json values was false");
                Assert.IsTrue(CompareJsonFiles(expJobject, actJobject, "TopOilChangeRate", null, "WellId"), "Compare Json values was false");
                Assert.IsTrue(CompareJsonFiles(expJobject, actJobject, "TopWaterChangeRate", null, "WellId"), "Compare Json values was false");
                Assert.IsTrue(CompareJsonFiles(expJobject, actJobject, null, "TotalGasChangeRate", ""), "Compare Json values was false");
                Assert.IsTrue(CompareJsonFiles(expJobject, actJobject, null, "TotalLiquidChangeRate", ""), "Compare Json values was false");
                Assert.IsTrue(CompareJsonFiles(expJobject, actJobject, null, "TotalOilChangeRate", ""), "Compare Json values was false");
                Assert.IsTrue(CompareJsonFiles(expJobject, actJobject, null, "TotalWaterChangeRate", ""), "Compare Json values was false");
                #endregion
            }
            finally
            {
                for (int i = 0; i < 2; i++)
                {
                    string facidesp = GetFacilityId("ESPWELL_", (i + 1));
                    RemoveWell(facidesp);
                    string facidgl = GetFacilityId("GLWELL_", (i + 1));
                    RemoveWell(facidgl);
                    string facidpgl = GetFacilityId("PGLWELL_", (i + 1));
                    RemoveWell(facidpgl);
                    string facidnfw = GetFacilityId("NFWWELL_", (i + 1));
                    RemoveWell(facidnfw);
                    string facidrrl = GetFacilityId("RPOC_", (i + 1));
                    RemoveWell(facidrrl);
                }
            }


        }


        public void AssertPhaseValues(string wellType, PEKPIAllDTO prodallkipdto)
        {
            Trace.WriteLine(prodallkipdto.OilProduction.CurrentValue);
            string phase = "Oil";
            Assert.AreEqual(420, prodallkipdto.OilProduction.CurrentValue, string.Format("{0} {1} Production Current Value mismatch", phase, wellType));
            Assert.AreEqual(400, prodallkipdto.OilProduction.LastValue, string.Format("{0} {1} Production Last  Value mismatch", phase, wellType));
            Assert.AreEqual(20, prodallkipdto.OilProduction.ChangeValue, string.Format("{0} {1} Production Change  Value mismatch", phase, wellType));
            Assert.AreEqual(5, prodallkipdto.OilProduction.ChangePercentage, string.Format("{0} {1} Production Change PercentageValue mismatch", phase, wellType));
            phase = "Water";
            Assert.AreEqual(220, prodallkipdto.WaterProduction.CurrentValue, string.Format("{0} {1} Production Current Value mismatch", phase, wellType));
            Assert.AreEqual(200, prodallkipdto.WaterProduction.LastValue, string.Format("{0} {1} Production Last  Value mismatch", phase, wellType));
            Assert.AreEqual(20, prodallkipdto.WaterProduction.ChangeValue, string.Format("{0} {1} Production Change  Value mismatch", phase, wellType));
            Assert.AreEqual(10, prodallkipdto.WaterProduction.ChangePercentage, string.Format("{0} {1} Production Change PercentageValue mismatch", phase, wellType));
            phase = "Gas";
            Assert.AreEqual(70, prodallkipdto.GasProduction.CurrentValue, string.Format("{0} {1} Production Current Value mismatch", phase, wellType));
            Assert.AreEqual(50, prodallkipdto.GasProduction.LastValue, string.Format("{0} {1} Production Last  Value mismatch", phase, wellType));
            Assert.AreEqual(20, prodallkipdto.GasProduction.ChangeValue, string.Format("{0} {1} Production Change  Value mismatch", phase, wellType));
            Assert.AreEqual(40, prodallkipdto.GasProduction.ChangePercentage, string.Format("{0} {1} Production Change PercentageValue mismatch", phase, wellType));
            phase = "Liquid";
            Assert.AreEqual(640, prodallkipdto.Production.CurrentValue, string.Format("{0} {1} Production Current Value mismatch", phase, wellType));
            Assert.AreEqual(600, prodallkipdto.Production.LastValue, string.Format("{0} NonRRL Production Last  Value mismatch", phase, wellType));
            Assert.AreEqual(40, prodallkipdto.Production.ChangeValue, string.Format("{0}{1} Production Change  Value mismatch", phase, wellType));
            Assert.AreEqual(6.7, prodallkipdto.Production.ChangePercentage, string.Format("{0} {1} Production Change PercentageValue mismatch", phase, wellType));
        }

        public bool CompareJsonFiles(JObject expJobject, JObject actJobject, string NodeNamewithUnitsValuesPair, string SinlgeNodeName, string skipproparr)
        {
            bool jcompareresult = true;
            if (NodeNamewithUnitsValuesPair != null)
            {
                List<JObject> actTopGasChangeRate = actJobject.SelectToken(NodeNamewithUnitsValuesPair).SelectToken("Values").Values<JObject>().ToList();
                List<JObject> expTopGasChangeRate = expJobject.SelectToken(NodeNamewithUnitsValuesPair).SelectToken("Values").Values<JObject>().ToList();

                int icount = 0;
                if (actTopGasChangeRate.Count == expTopGasChangeRate.Count)
                {
                    foreach (JObject jobj in expTopGasChangeRate)
                    {
                        // CompareObjectsUsingReflection(jobj.Value, actTopGasChangeRate[icount], "Mismatch with TopGasChangeRate" , new HashSet<string> { "WellId" });
                        // Trace.WriteLine($"JObject value is: {jobj.Value<object>()} ");
                        //  Trace.WriteLine($"JObject value is: {actTopGasChangeRate[icount].Value<object>()} ");
                        bool skmatch = false;
                        foreach (var x in jobj)
                        {
                            skmatch = false;
                            string expname = x.Key;
                            //      Trace.WriteLine($"Looking for Key in josn Object : {expname}");
                            JToken expvalue = x.Value;
                            string jexpvalue = (string)expvalue;
                            string jactvalue = actTopGasChangeRate[icount].GetValue(expname).ToString();
                            if (expname == "WellName")
                            {
                                jexpvalue = ReplaceWellNameOnPadding(jexpvalue);
                                jactvalue = ReplaceWellNameOnPadding(jactvalue);
                            }
                            if (expname == "WellLabel")
                            {
                                string partwellname = GetWellNameFromString(jexpvalue);
                                jexpvalue = jexpvalue.Replace(partwellname, ReplaceWellNameOnPadding(partwellname));
                                jactvalue = jactvalue.Replace(partwellname, ReplaceWellNameOnPadding(partwellname));
                            }
                            string[] arrskip = skipproparr.Split(new char[] { ';' });
                            foreach (string paramtoskip in arrskip)
                            {

                                if (expname.Contains(paramtoskip))
                                {
                                    Trace.WriteLine($"Colum  Name was matched {expname}  from {skipproparr} ");
                                    skmatch = true;
                                }

                            }
                            if (skmatch)
                            {
                                continue;
                            }
                            // Trace.WriteLine($"Object Key  Exp : {expname}, Object value  Exp : {expvalue}");
                            Trace.WriteLine($"Object Key  Act : {actTopGasChangeRate[icount].Property(expname).Name}, Object value  Act: {actTopGasChangeRate[icount].GetValue(expname)}");
                            if (expname != actTopGasChangeRate[icount].Property(expname).Name)
                            {

                                jcompareresult = false;
                                Trace.WriteLine($"Mismatch in Name for {expname}");
                                return jcompareresult;
                            }
                            if (jexpvalue.ToString().Trim() != jactvalue.Trim())
                            {
                                jcompareresult = false;
                                Trace.WriteLine($"Mismatch in Value for Key: {expname} Expected Value : {jexpvalue} Actual Value: {jactvalue}");
                                return jcompareresult;
                            }
                        }
                        icount++;
                    }
                }
            }
            if (SinlgeNodeName != null)
            {
                Type objtype = expJobject.SelectToken(SinlgeNodeName).Value<object>().GetType();
                switch (objtype.Name)
                {
                    case "JValue":
                        {
                            JValue singleactnodeobj = actJobject.SelectToken(SinlgeNodeName).Value<JValue>();
                            JValue singleexpnodeobj = expJobject.SelectToken(SinlgeNodeName).Value<JValue>();
                            Assert.AreEqual(singleexpnodeobj.Value.ToString(), singleactnodeobj.Value.ToString(), $"Mismatch with Value {SinlgeNodeName}");
                            break;
                        }
                    default:
                        {
                            JObject singleactnodeobj = (JObject)actJobject.SelectToken(SinlgeNodeName).Value<JObject>();
                            JObject singleexpnodeobj = (JObject)expJobject.SelectToken(SinlgeNodeName).Value<JObject>();
                            foreach (var x in singleexpnodeobj)
                            {
                                string expname = x.Key;
                                JToken expvalue = x.Value;
                                string jexpvalue = (string)expvalue;
                                Trace.WriteLine($"Looking for Key in josn Object : {expname}");
                                Assert.AreEqual(jexpvalue, singleactnodeobj.GetValue(expname), $"Mismatch with Value {SinlgeNodeName}");
                            }
                            break;
                        }
                }



            }
            return jcompareresult;
        }
        public void AddDailyAvgForESP(WellDTO wldto, int days, int delta)
        {
            WellDailyAverageValueDTO dailyAverageDTO = new WellDailyAverageValueDTO

            {
                EndDateTime = DateTime.Today.AddDays(-days).ToUniversalTime(),
                StartDateTime = DateTime.Today.AddDays(-days - 1).ToUniversalTime(),

                THP = 135,
                //  CHP = 329,
                PIP = 1750,
                PDP = 2500,
                OilRateAllocated = 200 + delta,
                WaterRateAllocated = 100 + delta,
                GasRateAllocated = 25 + delta,
                MotorVolts = 500,
                MotorAmps = 13,
                MotorFrequency = 50,
                Status = WellDailyAverageDataStatus.Original,
                RunTime = 24,
                THT = 0,
                WellId = wldto.Id,
                Id = 0,


            };
            bool addDailyAvgDataForPGL = SurveillanceService.AddUpdateWellDailyAverageDataNonRRL(dailyAverageDTO);
            Assert.IsTrue(addDailyAvgDataForPGL, "Failed to add Daily Average data");
        }

        public string ReplaceWellNameOnPadding(string wellname)
        {
            string jexpvalue = wellname;
            int wellpadspos = jexpvalue.IndexOf("_");
            string baseFacilityId = jexpvalue.Substring(0, wellpadspos + 1);
            int number = Convert.ToInt32(jexpvalue.Substring(wellpadspos + 1));
            string facilityId = baseFacilityId + (s_isRunningInATS ? number.ToString("D5") : number.ToString("D4"));
            return facilityId;


        }

        public string GetWellNameFromString(string labelvalue)
        {
            Trace.WriteLine("Label Name" + labelvalue);
            System.Text.RegularExpressions.Regex re = new System.Text.RegularExpressions.Regex("\\D+_\\d+");
            System.Text.RegularExpressions.Match mt = re.Match(labelvalue.ToString());
            string strmth = mt.ToString();
            return strmth;
        }
        public void AddDailyAvgForGL(WellDTO wldto, int days, int delta)
        {
            WellDailyAverageValueDTO dailyAverageDTO = new WellDailyAverageValueDTO

            {
                EndDateTime = DateTime.Today.AddDays(-days).ToUniversalTime(),
                StartDateTime = DateTime.Today.AddDays(-days - 1).ToUniversalTime(),
                THP = 135,
                CHP = 329,
                PIP = 1750,
                PDP = 2500,
                OilRateAllocated = 200 + delta,
                WaterRateAllocated = 100 + delta,
                GasRateAllocated = 25 + delta,
                MotorVolts = 500,
                MotorAmps = 13,
                MotorFrequency = 50,
                Status = WellDailyAverageDataStatus.Original,
                RunTime = 24,
                THT = 0,
                WellId = wldto.Id,
                Id = 0,


            };
            bool addDailyAvgDataForPGL = SurveillanceService.AddUpdateWellDailyAverageDataNonRRL(dailyAverageDTO);
            Assert.IsTrue(addDailyAvgDataForPGL, "Failed to add Daily Average data");
        }
        public void AddDailyAvgForPGL(WellDTO wldto, int days, int delta)
        {
            WellDailyAverageValueDTO dailyAverageDTO = new WellDailyAverageValueDTO

            {
                EndDateTime = DateTime.Today.AddDays(-days).ToUniversalTime(),
                StartDateTime = DateTime.Today.AddDays(-days - 1).ToUniversalTime(),
                THP = 135,
                //     CHP = 329,
                PIP = 1750,
                PDP = 2500,
                OilRateAllocated = 200 + delta,
                WaterRateAllocated = 100 + delta,
                GasRateAllocated = 25 + delta,
                MotorVolts = 500,
                MotorAmps = 13,
                MotorFrequency = 50,
                Status = WellDailyAverageDataStatus.Original,
                RunTime = 24,
                THT = 0,
                WellId = wldto.Id,
                Id = 0,


            };
            bool addDailyAvgDataForPGL = SurveillanceService.AddUpdateWellDailyAverageDataNonRRL(dailyAverageDTO);
            Assert.IsTrue(addDailyAvgDataForPGL, "Failed to add Daily Average data");
        }
        public void AddDailyAvgForNF(WellDTO wldto, int days, int delta)
        {
            WellDailyAverageValueDTO dailyAverageDTO = new WellDailyAverageValueDTO

            {
                EndDateTime = DateTime.Today.AddDays(-days).ToUniversalTime(),
                StartDateTime = DateTime.Today.AddDays(-days - 1).ToUniversalTime(),
                THP = 135,
                //      CHP = 329,
                PIP = 1750,
                PDP = 2500,
                OilRateAllocated = 200 + delta,
                WaterRateAllocated = 100 + delta,
                GasRateAllocated = 25 + delta,
                MotorVolts = 500,
                MotorAmps = 13,
                MotorFrequency = 50,
                Status = WellDailyAverageDataStatus.Original,
                RunTime = 24,
                THT = 0,
                WellId = wldto.Id,
                Id = 0,


            };
            bool addDailyAvgDataForPGL = SurveillanceService.AddUpdateWellDailyAverageDataNonRRL(dailyAverageDTO);
            Assert.IsTrue(addDailyAvgDataForPGL, "Failed to add Daily Average data");
        }

        public void AddDailyAvgForRRL(WellDTO wldto, int days, int delta)
        {
            WellDailyAverageValueDTO dailyAverageDTO = new WellDailyAverageValueDTO

            {
                EndDateTime = DateTime.Today.AddDays(-days).ToUniversalTime(),
                StartDateTime = DateTime.Today.AddDays(-days - 1).ToUniversalTime(),
                OilRateAllocated = 200 + delta,
                WaterRateAllocated = 100 + delta,
                GasRateAllocated = 25 + delta,
                MotorVolts = 500,
                MotorAmps = 13,
                MotorFrequency = 50,
                Status = WellDailyAverageDataStatus.Original,
                RunTime = 24,
                THT = 0,
                WellId = wldto.Id,
                Id = 0,


            };
            bool addDailyAvgDataForRRL = SurveillanceService.AddUpdateWellDailyAverageDataNonRRL(dailyAverageDTO);
            Assert.IsTrue(addDailyAvgDataForRRL, "Failed to add Daily Average data");
        }
        public List<string> getCygNetHistoricalEntries(string facilityId, string[] UDCs)
        {
            List<string> facUDCs = new List<string>();
            foreach (var udc in UDCs)
            {
                facUDCs.Add(facilityId + "." + udc);
            }
            object input = facUDCs.Select(t => t as object).ToArray();
            object pointIdsList = null;
            //Give the Point Name and Get its Integer ID  in a List 
            try
            {
                Trace.WriteLine($"SeedHistoricalValuesInVHS: Going to create CVS Client for [{s_domain}]{s_site}.{s_cvsService}.");
                var cvsClient = CreateCvsClient();
                cvsClient.ResolvePoints(ref input, out pointIdsList);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"SeedHistoricalValuesInVHS: CvsClient.ResolvePoints failed: {ex.Message}.");
            }
            //Now Give Fill Path with Site Name List<string> names = new List<string>();
            string site = ConfigurationManager.AppSettings.Get("Site");
            List<string> tagIds = new List<string>();
            if (pointIdsList.GetType().IsArray)
            {
                tagIds = ((object[])pointIdsList).Select(obj => Convert.ToString(obj)).ToList();
            }
            List<string> names = new List<string>();
            foreach (var pointId in tagIds)
            {
                names.Add(site + "." + "UIS" + "." + pointId);
            }

            var dss = new CygNet.Data.Core.DomainSiteService(Convert.ToUInt16(s_domain), s_site, "VHS");
            Client _vhsClient = new Client(dss);
            List<string> infprodvalues = new List<string>();
            foreach (var nm in names)
            {
                IEnumerable<HistoricalEntry> vhsointvlaues = _vhsClient.GetHistoricalEntries(new Name { ID = nm }, DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1), false);
                foreach (var it in vhsointvlaues)
                {
                    string vaal = it.GetValueAsString();
                    infprodvalues.Add(vaal);
                }
            }
            return infprodvalues;
        }


        public string[] gettRRLUDC(string facid)
        {
            List<string> prodrrlvals = getCygNetHistoricalEntries(facid, new string[] { "LQTLIQYD" });
            string[] arr = prodrrlvals.ToArray();

            Trace.WriteLine(" current value" + arr[2]);
            Trace.WriteLine(" last  value" + arr[1]);
            string[] rrlkpis = new string[] { arr[2], arr[1] };
            return rrlkpis;

        }
        private static ICvsClient CreateCvsClient()
        {
            var cvsDomainSiteSvc = GetDomainSiteService("UIS");
            ICvsClient cvsClient = new CvsClient();
            try
            {
                cvsClient.Connect(cvsDomainSiteSvc.GetDomainSiteService());
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Failed to connect to {cvsDomainSiteSvc.GetDomainSiteService()}: {ex.Message}");
                Trace.WriteLine($"Failed to connect to {cvsDomainSiteSvc.GetDomainSiteService()}: {ex.ToString()}");
            }

            if (!cvsClient.IsConnected)
            {
                Trace.WriteLine($"Failed to connect to {cvsDomainSiteSvc.GetDomainSiteService()}.");
            }

            return cvsClient;
        }

        private static CygNet.COMAPI.Core.DomainSiteService GetDomainSiteService(string service)
        {
            var dss = new CygNet.COMAPI.Core.DomainSiteService(Convert.ToUInt16(s_domain), s_site, service);
            Trace.WriteLine($"DSS for {service} is {dss.GetDomainSiteService()}");
            return dss;
        }
    }
}
