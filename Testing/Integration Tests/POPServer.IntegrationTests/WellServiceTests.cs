using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Weatherford.DynaCardLibrary.API.Enums;
using Weatherford.POP.DTOs;
using Weatherford.POP.Enums;

namespace Weatherford.POP.Server.IntegrationTests
{
    [TestClass]
    public class WellServiceTests : APIClientTestBase
    {
        protected List<WellDTO> _addedWells;

        [TestInitialize]
        public override void Init()
        {
            base.Init();
            _addedWells = new List<WellDTO>();
        }

        [TestCleanup]
        public override void Cleanup()
        {
            base.Cleanup();
        }

        private static void CompareTwoWells(WellDTO expected, WellDTO actual, bool compareDataConnection)
        {
            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.Name, actual.Name);
            Assert.AreEqual(expected.SurfaceLatitude, actual.SurfaceLatitude);
            Assert.AreEqual(expected.SurfaceLongitude, actual.SurfaceLongitude);
            if (compareDataConnection)
            {
                if (expected.DataConnection == null)
                {
                    Assert.IsNull(actual.DataConnection);
                }
                else
                {
                    Assert.IsNotNull(actual.DataConnection);
                    Assert.AreEqual(expected.DataConnection.ProductionDomain, actual.DataConnection.ProductionDomain);
                    Assert.AreEqual(expected.DataConnection.ReplicationDomain, actual.DataConnection.ReplicationDomain);
                    Assert.AreEqual(expected.DataConnection.Site, actual.DataConnection.Site);
                    Assert.AreEqual(expected.DataConnection.Service, actual.DataConnection.Service);
                }
            }
        }

        private void AddWells()
        {
            string facilityId;
            if (s_isRunningInATS)
                facilityId = "D5";
            else
                facilityId = "D4";
            for (int i = s_wellStart; i <= s_wellsEnd; i++)
            {
                string wellName = "RPOC_" + i.ToString(facilityId);
                WellDTO well = SetDefaultFluidType(new WellDTO()
                {
                    Name = wellName,
                    FacilityId = wellName,
                    DataConnection = GetDefaultCygNetDataConnection(),
                    WellType = WellTypeId.RRL,
                    Lease = "Lease_" + i.ToString("D2"),
                    Foreman = "Foreman" + i.ToString("D2"),
                    Field = "Field" + i.ToString("D2"),
                    Engineer = "Engineer" + i.ToString("D2"),
                    GaugerBeat = "GaugerBeat" + i.ToString("D2"),
                    GeographicRegion = "GeographicRegion" + i.ToString("D2"),
                    welUserDef01 = "State_" + i.ToString("D2"),
                    welUserDef02 = "User_" + i.ToString("D2"),
                    IntervalAPI = "IntervalAPI" + i.ToString("D2"),
                    SubAssemblyAPI = "SubAssemblyAPI" + i.ToString("D2"),
                    AssemblyAPI = "AssemblyAPI" + i.ToString("D2"),
                    CommissionDate = DateTime.Today.AddDays(i),
                });
                WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = well });
                WellDTO addedWell = WellService.GetAllWells().FirstOrDefault(w => w.Name.Equals(well.Name));
                Assert.IsNotNull(addedWell);
                _wellsToRemove.Add(addedWell);
                _addedWells.Add(addedWell);
            }
        }

        [TestCategory(TestCategories.WellServiceTests), TestMethod]
        public void AddGetWellsGetWellRemoveWell()
        {
            var allWellsBefore = WellService.GetAllWells().ToList();

            string assetName = "Asset Test";
            SurfaceNetworkService.AddAsset(new AssetDTO() { Name = assetName, Description = "Test Description" });
            var allAssets = SurfaceNetworkService.GetAllAssets().ToList();
            AssetDTO asset = allAssets?.FirstOrDefault(a => a.Name.Equals(assetName));
            Assert.IsNotNull(asset);

            var toAdd = SetDefaultFluidType(new WellDTO() { Name = DefaultWellName, FacilityId = "CASETEST", IntervalAPI = "IntervalAPI", SubAssemblyAPI = "SubAssemblyAPI", AssemblyAPI = "AssemblyAPI", CommissionDate = DateTime.Today, WellType = WellTypeId.RRL, AssetId = asset.Id });
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = toAdd });
            var allWells = WellService.GetAllWells().ToList();
            WellDTO well = allWells.FirstOrDefault(w => allWellsBefore.FirstOrDefault(wb => wb.Id == w.Id) == null);
            Assert.IsNotNull(well);
            Assert.AreEqual(toAdd.Name, well.Name);

            _wellsToRemove.Add(well);
            WellDTO compare = WellService.GetWell(well.Id.ToString());
            CompareTwoWells(toAdd, compare, false);

            long[] userAssets = new long[1]; userAssets[0] = asset.Id;
            WellDTO[] wellsByAsset = WellService.GetWellsByUserAssetIds(userAssets);

            if (wellsByAsset.Count() > 0)
            {
                WellDTO welldto = wellsByAsset.FirstOrDefault();
                Assert.AreEqual(asset.Id, welldto.AssetId);
            }
            _assetsToRemove.Add(asset);


            WellConfigurationService.RemoveWellConfig(well.Id.ToString());
            compare = WellService.GetWell(well.Id.ToString());
            Assert.IsNull(compare);
            _wellsToRemove.Remove(well);

            //Get Well History
            WellHistoryDTO[] welHistory = WellService.GetWellHistory(well.Id.ToString());
            Assert.AreEqual(1, welHistory.Length);
        }

        [TestCategory(TestCategories.WellServiceTests), TestMethod]
        public void AddWellWithDataConnection()
        {
            var allDataConnectionsBefore = DataConnectionService.GetAllDataConnections().ToList();
            var allWellsBefore = WellService.GetAllWells().ToList();

            var toAdd = SetDefaultFluidType(new WellDTO() { Name = DefaultWellName, FacilityId = "CASETEST", SurfaceLatitude = 1.232m, SurfaceLongitude = 2.3232m, IntervalAPI = "IntervalAPI", SubAssemblyAPI = "SubAssemblyAPI", AssemblyAPI = "AssemblyAPI", CommissionDate = DateTime.Today, WellType = WellTypeId.RRL });
            toAdd.DataConnection = new DataConnectionDTO() { ProductionDomain = s_domain, Site = s_site, Service = s_cvsService };
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = toAdd });
            var allDataConnections = DataConnectionService.GetAllDataConnections().ToList();
            DataConnectionDTO newDC = allDataConnections.FirstOrDefault(dc => allDataConnectionsBefore.FirstOrDefault(dcb => dcb.Id == dc.Id) == null);
            if (newDC != null) { _dataConnectionsToRemove.Add(newDC); }
            var allWells = WellService.GetAllWells().ToList();
            WellDTO well = allWells.FirstOrDefault(w => allWellsBefore.FirstOrDefault(wb => wb.Id == w.Id) == null);
            Assert.IsNotNull(well);
            _wellsToRemove.Add(well);
            WellDTO compare = WellService.GetWell(well.Id.ToString());
            CompareTwoWells(toAdd, compare, true);

            var toAdd2 = SetDefaultFluidType(new WellDTO() { Name = "CASETest2", FacilityId = "CASETEST2", DataConnection = toAdd.DataConnection, IntervalAPI = "IntervalAPI2", SubAssemblyAPI = "SubAssemblyAPI2", AssemblyAPI = "AssemblyAPI2", CommissionDate = DateTime.Today, WellType = WellTypeId.RRL });
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = toAdd2 });
            allWellsBefore = allWells;
            allWells = WellService.GetAllWells().ToList();
            well = allWells.FirstOrDefault(w => allWellsBefore.FirstOrDefault(wb => wb.Id == w.Id) == null);
            Assert.IsNotNull(well);
            _wellsToRemove.Add(well);
            WellDTO lastWell = compare;
            compare = WellService.GetWell(well.Id.ToString());
            CompareTwoWells(toAdd2, compare, true);
            // Make sure the well service used the existing data connection instead of creating a duplicate one.
            Assert.AreEqual(lastWell.DataConnection.Id, compare.DataConnection.Id);
        }

        [TestCategory(TestCategories.WellServiceTests), TestMethod]
        public void UpdateWellWithDataConnection()
        {


            var allDataConnectionsBefore = DataConnectionService.GetAllDataConnections().ToList();
            var allWellsBefore = WellService.GetAllWells().ToList();

            var toAdd = SetDefaultFluidType(new WellDTO() { Name = DefaultWellName, FacilityId = GetFacilityId("RPOC_", 1), IntervalAPI = "IntervalAPI", SubAssemblyAPI = "SubAssemblyAPI", AssemblyAPI = "AssemblyAPI", CommissionDate = DateTime.Today, WellType = WellTypeId.RRL });
            toAdd.DataConnection = new DataConnectionDTO() { ProductionDomain = s_domain, Site = s_site, Service = s_cvsService };
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = toAdd });
            var allDataConnections = DataConnectionService.GetAllDataConnections().ToList();
            DataConnectionDTO newDC = allDataConnections.FirstOrDefault(dc => allDataConnectionsBefore.FirstOrDefault(dcb => dcb.Id == dc.Id) == null);
            if (newDC != null) { _dataConnectionsToRemove.Add(newDC); }
            var allWells = WellService.GetAllWells().ToList();
            WellDTO well = allWells.FirstOrDefault(w => allWellsBefore.FirstOrDefault(wb => wb.Id == w.Id) == null);
            Assert.IsNotNull(well);
            _wellsToRemove.Add(well);
            WellDTO compare = WellService.GetWell(well.Id.ToString());
            CompareTwoWells(toAdd, compare, true);

            var toAdd2 = SetDefaultFluidType(new WellDTO() { Name = "CASETest2", IntervalAPI = "IntervalAPI2", SubAssemblyAPI = "SubAssemblyAPI2", AssemblyAPI = "AssemblyAPI2", CommissionDate = DateTime.Today, WellType = WellTypeId.RRL });
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = toAdd2, ModelConfig = new ModelConfigDTO() });
            allWellsBefore = allWells;
            allWells = WellService.GetAllWells().ToList();
            well = allWells.FirstOrDefault(w => allWellsBefore.FirstOrDefault(wb => wb.Id == w.Id) == null);
            Assert.IsNotNull(well);
            _wellsToRemove.Add(well);
            WellDTO lastWell = compare;
            compare = WellService.GetWell(well.Id.ToString());
            CompareTwoWells(toAdd2, compare, true);
            compare.FacilityId = GetFacilityId("RPOC_", 2);
            compare.DataConnection = toAdd.DataConnection;
            toAdd2 = compare;
            Assert.AreEqual(0, toAdd.DataConnection.Id);
            toAdd2.SurfaceLatitude = 32.387221m;
            toAdd2.SurfaceLongitude = 101.238782m;
            WellConfigurationService.UpdateWellConfig(new WellConfigDTO() { Well = toAdd2, ModelConfig = new ModelConfigDTO() });
            compare = WellService.GetWell(well.Id.ToString());
            CompareTwoWells(toAdd2, compare, true);

            // Make sure the well service used the existing data connection instead of creating a duplicate one.
            Assert.AreEqual(lastWell.DataConnection.Id, compare.DataConnection.Id);
        }

        [TestCategory(TestCategories.WellServiceTests), TestMethod] //need sample data
        public void GetWellFilterGetWellbyFilterGetWellAttributes()
        {
            AddWells();
            WellDTO[] allwells = WellService.GetAllWells();

            long?[] assetIds = null;
            string[] WellAttributesField = WellService.GetWellAttributeValues("field", assetIds);
            Assert.IsNotNull(WellAttributesField, "Well field attribute values should not be null.");
            Assert.AreEqual(allwells.Length, WellAttributesField.Length, "Well field attribute values length should match well count.");
            string[] WellAttributesEngineer = WellService.GetWellAttributeValues("engineer", assetIds);
            Assert.IsNotNull(WellAttributesEngineer, "Well engineer attribute values should not be null.");
            Assert.AreEqual(allwells.Length, WellAttributesEngineer.Length, "Well engineer attribute values length should match well count.");
            string[] WellAttributesForeman = WellService.GetWellAttributeValues("foreman", assetIds);
            Assert.IsNotNull(WellAttributesForeman, "Well foreman attribute values should not be null.");
            Assert.AreEqual(allwells.Length, WellAttributesForeman.Length, "Well foreman attribute values length should match well count.");

            //Get well Filters
            WellFilterDTO filters = WellService.GetWellFilter(assetIds);
            Assert.IsNotNull(filters);

            //Get Group filter display
            GroupFilterDisplayDTO filterDisplay = WellService.GetGroupFilterDisplay();

            //Get Wells by Filter
            WellFilterDTO wellbyFilter = new WellFilterDTO();

            //No Filter selected
            WellDTO[] wells = WellService.GetWellsByFilter(wellbyFilter);
            Assert.AreEqual(allwells.Count(), wells.Count());

            //All Flters Selected
            wellbyFilter.welEngineerTitle = filters.welEngineerTitle;
            wellbyFilter.welEngineerValues = filters.welEngineerValues;
            wellbyFilter.welFieldNameTitle = filters.welFieldNameTitle;
            wellbyFilter.welFieldNameValues = filters.welFieldNameValues;
            wellbyFilter.welForemanTitle = filters.welForemanTitle;
            wellbyFilter.welForemanValues = filters.welForemanValues;
            wellbyFilter.welGaugerBeatTitle = filters.welGaugerBeatTitle;
            wellbyFilter.welGaugerBeatValues = filters.welGaugerBeatValues;
            wellbyFilter.welGeographicRegionTitle = filters.welGeographicRegionTitle;
            wellbyFilter.welGeographicRegionValues = filters.welGeographicRegionValues;
            wellbyFilter.welLeaseNameTitle = filters.welLeaseNameTitle;
            wellbyFilter.welLeaseNameValues = filters.welLeaseNameValues;
            wellbyFilter.welUserDef01Title = filters.welUserDef01Title;
            wellbyFilter.welUserDef01Values = filters.welUserDef01Values;
            wellbyFilter.welUserDef02Title = filters.welUserDef02Title;
            wellbyFilter.welUserDef02Values = filters.welUserDef02Values;

            WellDTO[] well = WellService.GetWellsByFilter(wellbyFilter);
            Assert.AreEqual(allwells.Count(), well.Count());

            //Static Filter 'welForemanTitle' selected
            WellFilterDTO staticFilter = new WellFilterDTO();
            staticFilter.welForemanTitle = filters.welForemanTitle;

            WellDTO[] staticWells = WellService.GetWellsByFilter(staticFilter);
            int staticCount = 0;
            foreach (WellDTO staticwells in allwells)
            {
                if (staticwells.Foreman != null)
                    staticCount = staticCount + 1;
            }
            Assert.AreEqual(staticCount, staticWells.Count());
        }

        [TestCategory(TestCategories.WellServiceTests), TestMethod]
        public void RemoveWellbyWellId()
        {
            AddWells();
            Assert.IsNotNull(_addedWells, "Added well list should not be null.");
            Assert.AreNotEqual(0, _addedWells.Count, "Added well list should not be empty.");
            foreach (WellDTO well in _addedWells)
            {
                WellService.RemoveWellByWellId(well.Id.ToString());
                WellDTO removedWell = WellService.GetWell(well.Id.ToString());
                Assert.IsNull(removedWell, $"Failed to remove well {removedWell?.Name} ({removedWell?.Id}).");
            }
        }

        [TestCategory(TestCategories.WellServiceTests), TestMethod]
        public void WellTypeTest()
        {
            int i = 1;
            foreach (WellTypeId wellTypeId in (WellTypeId[])Enum.GetValues(typeof(WellTypeId)))
            {
                if (wellTypeId != WellTypeId.Unknown && wellTypeId != WellTypeId.All)
                {
                    WellDTO thiswell = SetDefaultFluidType(new WellDTO() { Name = DefaultWellName + i, CommissionDate = DateTime.Today, WellType = wellTypeId });
                    thiswell.AssemblyAPI = thiswell.SubAssemblyAPI = thiswell.IntervalAPI = thiswell.Name;

                    WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = thiswell });
                    i++;
                }
            }
            WellDTO[] allwells = WellService.GetAllWells();
            _wellsToRemove.AddRange(allwells);
            WellFilterDTO filters = WellService.GetWellFilter(null);
            Assert.IsNotNull(filters);

            //Get Group filter display
            GroupFilterDisplayDTO filterDisplay = WellService.GetGroupFilterDisplay();

            List<WellFilterValueDTO> value = new List<WellFilterValueDTO>();
            WellDTO[] wells = new WellDTO[] { };
            value.Add(new WellFilterValueDTO() { Value = WellTypeId.RRL.ToString() });
            filters.welFK_r_WellTypeValues = value;
            wells = WellService.GetWellsByFilter(filters);
            Assert.AreEqual(wells.Count(), value.Count);
            Trace.WriteLine("Verification completed for RRL well");
            value.Add(new WellFilterValueDTO() { Value = WellTypeId.ESP.ToString() });
            filters.welFK_r_WellTypeValues = value;
            wells = WellService.GetWellsByFilter(filters);
            Assert.AreEqual(wells.Count(), value.Count);
            Trace.WriteLine("Verification completed for ESP well");
            value.Add(new WellFilterValueDTO() { Value = WellTypeId.GInj.ToString() });
            filters.welFK_r_WellTypeValues = value;
            wells = WellService.GetWellsByFilter(filters);
            Assert.AreEqual(wells.Count(), value.Count);
            Trace.WriteLine("Verification completed for GINJ well");
            value.Add(new WellFilterValueDTO() { Value = WellTypeId.GLift.ToString() });
            filters.welFK_r_WellTypeValues = value;
            wells = WellService.GetWellsByFilter(filters);
            Assert.AreEqual(wells.Count(), value.Count);
            Trace.WriteLine("Verification completed for GLift well");
            value.Add(new WellFilterValueDTO() { Value = WellTypeId.NF.ToString() });
            filters.welFK_r_WellTypeValues = value;
            wells = WellService.GetWellsByFilter(filters);
            Assert.AreEqual(wells.Count(), value.Count);
            Trace.WriteLine("Verification completed for NF well");
            value.Add(new WellFilterValueDTO() { Value = WellTypeId.WInj.ToString() });
            filters.welFK_r_WellTypeValues = value;
            wells = WellService.GetWellsByFilter(filters);
            Assert.AreEqual(wells.Count(), value.Count);
            Trace.WriteLine("Verification completed for WInj well");

            // test right type is returned atleast for one well.
            value.Clear();
            value.Add(new WellFilterValueDTO() { Value = WellTypeId.GLift.ToString() });
            filters.welFK_r_WellTypeValues = value;
            wells = WellService.GetWellsByFilter(filters);
            Assert.AreEqual(value.First().Value.ToString(), wells.First().WellType.ToString());
        }

        [TestCategory(TestCategories.WellServiceTests), TestMethod]
        public void LicensedWellTypesTest()
        {
            // Our licenses here are full licenses so I am not sure what can really be tested aside from that data comes back....
            IList<string> wtypes = WellService.GetLicensedWellTypes();
            Assert.IsFalse(wtypes.Count() == 0);
            Assert.IsTrue(wtypes.Contains(WellTypeId.RRL.ToString()));
            Assert.IsTrue(wtypes.Contains(WellTypeId.ESP.ToString()));
            Assert.IsTrue(wtypes.Contains(WellTypeId.GLift.ToString()));
            Assert.IsTrue(wtypes.Contains(WellTypeId.NF.ToString()));
            Assert.IsTrue(wtypes.Contains(WellTypeId.WInj.ToString()));
            Assert.IsTrue(wtypes.Contains(WellTypeId.GInj.ToString()));
            Assert.IsTrue(wtypes.Contains(WellTypeId.WGInj.ToString()));
            Assert.IsTrue(wtypes.Contains(WellTypeId.PLift.ToString()));
            Assert.IsTrue(wtypes.Contains(WellTypeId.OT.ToString()));
            Assert.IsTrue(wtypes.Contains(WellTypeId.PCP.ToString()));

            int warningDaysOut = 30;
            if (System.Configuration.ConfigurationManager.AppSettings.AllKeys.Contains("LicenseExpirationWarningDaysOut") && int.TryParse(System.Configuration.ConfigurationManager.AppSettings["LicenseExpirationWarningDaysOut"], out warningDaysOut))
            {
                warningDaysOut = Math.Max(warningDaysOut, 1);
            }

            Dictionary<SystemFeature, LicenseDetailDTO> licdetails = LicenseService.GetLicenseData();
            foreach (var indfeature in licdetails.Keys)
            {
                Trace.WriteLine($"Avalaible License {indfeature.ToString()} are : {licdetails[indfeature].LicenseStatus} remaining days count : {licdetails[indfeature].DaysRemaining}  Expiry Date {licdetails[indfeature].EndDate} ");
            }
            Assert.IsTrue(licdetails.Count() > 0);
            Trace.WriteLine($"System features count is  {((SystemFeature[])Enum.GetValues(typeof(SystemFeature))).Length}  Warning days : {warningDaysOut}");
            foreach (SystemFeature feature in (SystemFeature[])Enum.GetValues(typeof(SystemFeature)))
            {
                // These features are not currently being licensed... or are RM 
                if (feature == SystemFeature.PCPFailureProbability ||
                    feature == SystemFeature.ReliabilityManagement)
                {
                    Assert.IsFalse(licdetails.Keys.Contains(feature), $"Unexpected feature {feature} present in license details.");
                    continue;
                }

                //These features are Edge-only (AutonomousControl, IntelligentAlarm)
                object objSettigvaue = GetForeSiteToolBoxSettingValue(SettingServiceStringConstants.FORESITE_FOR_EDGE);
                bool isEdge = (objSettigvaue.ToString() == "0") ? false : true;
                if (!isEdge && (feature == SystemFeature.AutonomousControl || feature == SystemFeature.IntelligentAlarm))
                {
                    continue;
                }
                Trace.WriteLine($"Feautre : {feature}  ");

                Assert.IsTrue(licdetails.ContainsKey(feature), $"Feature {feature} missing from license details.");
                Assert.AreEqual((licdetails[feature].DaysRemaining <= warningDaysOut ? LicenseStatus.ExpiringSoon : LicenseStatus.Licensed), licdetails[feature].LicenseStatus, $"Unexpected license status for {feature} license. Expiry date:  {licdetails[feature].EndDate} Warning days : {warningDaysOut} ");

                //Test Adding a Well with valid license
                WellTypeId wellType = GetWellTypeFromSystemFeature(feature);
                AddWellsWithAndWithoutLicensing(wellType, feature);

                if (wellType == WellTypeId.GInj)
                {
                    AddWellsWithAndWithoutLicensing(WellTypeId.WInj, feature);
                    AddWellsWithAndWithoutLicensing(WellTypeId.WGInj, feature);
                }
            }
        }

        private void AddWellsWithAndWithoutLicensing(WellTypeId wellType, SystemFeature feature)
        {


            string facilityId = s_isRunningInATS ? "RPOC_" + "0000" + (int)wellType : "RPOC_" + "000" + (int)wellType;
            // Test Adding a Well with valid license
            if (wellType != WellTypeId.Unknown)
            {
                string wellName = feature.ToString() + "_" + wellType.ToString();
                WellConfigDTO licenseWell = new WellConfigDTO()
                {
                    Well = SetDefaultFluidType(new WellDTO()
                    {
                        Name = wellName,
                        FacilityId = facilityId,
                        DataConnection = GetDefaultCygNetDataConnection(),
                        WellType = wellType,
                        Lease = "Lease_" + wellName,
                        Foreman = "Foreman" + wellName,
                        Field = "Field" + wellName,
                        Engineer = "Engineer" + wellName,
                        GaugerBeat = "GaugerBeat" + wellName,
                        GeographicRegion = "GeographicRegion" + wellName,
                        welUserDef01 = "State_" + wellName,
                        welUserDef02 = "User_" + wellName,
                        IntervalAPI = "IntervalAPI_" + wellName,
                        SubAssemblyAPI = "SubAssemblyAPI_" + wellName,
                        AssemblyAPI = "AssemblyAPI_" + wellName,
                        CommissionDate = DateTime.Today,
                    })
                };
                if (licenseWell.Well.WellType == WellTypeId.RRL)
                {
                    licenseWell.ModelConfig = new ModelConfigDTO();
                }
                else
                {
                    licenseWell.CommonModelConfig = new CommonModelConfigDTO();
                }

                WellConfigDTO addedWell = null;
                try
                {
                    addedWell = WellConfigurationService.AddWellConfig(licenseWell);
                    Assert.IsNotNull(addedWell);
                    _wellsToRemove.Add(addedWell.Well);
                    _addedWells.Add(addedWell.Well);

                    // Now demostrate that we can update the same well
                    addedWell.Well.Engineer = "Something to Change";
                    WellConfigurationService.UpdateWellConfig(addedWell);
                    WellDTO updatedWell = WellService.GetAllWells().FirstOrDefault(w => w.Name.Equals(licenseWell.Well.Name));
                    Assert.IsTrue(updatedWell.Engineer == "Something to Change", "Unexpected Engineer field setting for LicensedWellTypesTest");
                }
                catch (Exception ex)
                {
                    Assert.Fail($"Failed to add or update a well that was licensed: {ex.Message}." + Environment.NewLine + ex.ToString());
                }

                // Now Test Updating and Adding a Well without a valid license
                List<SystemFeature> disableLicenses = new List<SystemFeature>();
                disableLicenses.Add(feature);  // false = don't persist
                LicenseService.DisableLicenses(disableLicenses);

                // First, the update attempt
                try
                {
                    addedWell.Well.Engineer = "Something else to Change";
                    WellConfigurationService.UpdateWellConfig(addedWell);

                    Assert.Fail("Well {0} was successfully updated, but it should not have been considering that it wasn't licensed.");
                }
                catch
                {
                    WellDTO updatedWell = WellService.GetAllWells().FirstOrDefault(w => w.Engineer != null && w.Engineer.Equals("Something else to Change"));
                    Assert.IsNull(updatedWell);
                }

                // Now, the add attempt
                WellDTO unlicensedWell = SetDefaultFluidType(new WellDTO()
                {
                    Name = feature.ToString() + "_unlicensed",
                    FacilityId = facilityId + "_unlicensed",
                    DataConnection = GetDefaultCygNetDataConnection(),
                    WellType = wellType,
                    Lease = "Lease_" + feature.ToString() + "_unlicensed",
                    Foreman = "Foreman" + feature.ToString() + "_unlicensed",
                    Field = "Field" + feature.ToString() + "_unlicensed",
                    Engineer = "Engineer" + feature.ToString() + "_unlicensed",
                    GaugerBeat = "GaugerBeat" + feature.ToString() + "_unlicensed",
                    GeographicRegion = "GeographicRegion" + feature.ToString() + "_unlicensed",
                    welUserDef01 = "State_" + feature.ToString() + "_unlicensed",
                    welUserDef02 = "User_" + feature.ToString() + "_unlicensed",
                    IntervalAPI = "IntervalAPI_" + feature.ToString() + "_unlicensed",
                    SubAssemblyAPI = "SubAssemblyAPI_" + feature.ToString() + "_unlicensed",
                    AssemblyAPI = "AssemblyAPI_" + feature.ToString() + "_unlicensed",
                    CommissionDate = DateTime.Today,
                });

                try
                {
                    addedWell = WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = unlicensedWell });
                    Assert.IsNotNull(addedWell);
                    _wellsToRemove.Add(addedWell.Well);
                    _addedWells.Add(addedWell.Well);

                    Assert.Fail("Well {0} was successfully created, but it should not have been considering that it wasn't licensed.");
                }
                catch
                {
                    WellDTO addedWellBase = WellService.GetAllWells().FirstOrDefault(w => w.Name != null && w.Name.Equals(unlicensedWell.Name));
                    Assert.IsNull(addedWellBase);
                }
                finally
                {
                    LicenseService.ClearDisabledLicenses();
                }
            }
        }

        // This test was added purely to notify us for whenever the licenses on the license server are about to expire.
        [TestCategory(TestCategories.WellServiceTests), TestMethod]
        public void LicensedExpiringSoonTest()
        {
            Dictionary<SystemFeature, LicenseDetailDTO> licdetails = LicenseService.GetLicenseData();
            Assert.IsTrue(licdetails.Count() > 0, "License data should not be empty.");

            foreach (SystemFeature key in licdetails.Keys)
            {
                Assert.AreNotEqual(LicenseStatus.ExpiringSoon, licdetails[key].LicenseStatus, $"License server '{key.ToString()}' licenses are expiring in {licdetails[key].DaysRemaining} days.  TO FIX THIS TEST, PLEASE UPDATE THE LICENSES ON THE LICENSE SERVER!");
            }
        }

        [TestCategory(TestCategories.WellServiceTests), TestMethod]
        public void UnitsCheckforWellTargetRates()
        {
            try
            {
                AddUpdateDeleteGetTargetRates();
            }
            finally
            {
                ChangeUnitSystem("US");
                ChangeUnitSystemUserSetting("US");
            }
        }

        public void AddUpdateDeleteGetTargetRates()
        {
            WellDTO well = SetDefaultFluidType(new WellDTO()
            {
                Name = "Well",
                FacilityId = "NFWWELL_0001",
                DataConnection = GetDefaultCygNetDataConnection(),
                WellType = WellTypeId.NF,
                IntervalAPI = "IntervalAPI",
                SubAssemblyAPI = "SubAssemblyAPI",
                AssemblyAPI = "AssemblyAPI",
                CommissionDate = DateTime.Today,
            });
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = well });
            WellDTO addedWell = WellService.GetAllWells().FirstOrDefault(w => w.Name.Equals(well.Name));
            Assert.IsNotNull(addedWell);
            _wellsToRemove.Add(addedWell);

            WellTargetRateArrayAndUnitsDTO getTaregtRates = WellService.GetWellTargetRateAndUnits(addedWell.Id.ToString());
            WellTargetRateAndUnitsDTO targetrate = new WellTargetRateAndUnitsDTO();
            targetrate.Units = getTaregtRates.Units;
            WellTargetRateDTO tr = new WellTargetRateDTO();
            tr.StartDate = new DateTime(2017, 7, 18).ToUniversalTime();
            tr.EndDate = new DateTime(2017, 7, 25).ToUniversalTime();
            tr.OilMinimum = 2000;
            tr.OilLowerBound = 2500;
            tr.OilTarget = 3000;
            tr.OilUpperBound = 3500;
            tr.OilTechnicalLimit = 4000;

            tr.WaterMinimum = 2000;
            tr.WaterLowerBound = 2500;
            tr.WaterTarget = 3000;
            tr.WaterUpperBound = 3500;
            tr.WaterTechnicalLimit = 4000;

            tr.GasMinimum = 2000;
            tr.GasLowerBound = 2500;
            tr.GasTarget = 3000;
            tr.GasUpperBound = 3500;
            tr.GasTechnicalLimit = 4000;
            tr.WellId = addedWell.Id;
            targetrate.Value = tr;
            WellService.AddWellTargetRateAndUnits(targetrate);
            WellTargetRateAndUnitsDTO targetrate1 = new WellTargetRateAndUnitsDTO();
            targetrate1.Units = getTaregtRates.Units;
            WellTargetRateDTO tr1 = new WellTargetRateDTO();
            tr1.StartDate = new DateTime(2017, 10, 18).ToUniversalTime();
            tr1.EndDate = new DateTime(2017, 10, 25).ToUniversalTime();
            tr1.OilMinimum = 2001;
            tr1.OilLowerBound = 2501;
            tr1.OilTarget = 3001;
            tr1.OilUpperBound = 3501;
            tr1.OilTechnicalLimit = 4001;

            tr1.WaterMinimum = 2001;
            tr1.WaterLowerBound = 2501;
            tr1.WaterTarget = 3001;
            tr1.WaterUpperBound = 3501;
            tr1.WaterTechnicalLimit = 4001;

            tr1.GasMinimum = 2001;
            tr1.GasLowerBound = 2501;
            tr1.GasTarget = 3001;
            tr1.GasUpperBound = 3501;
            tr1.GasTechnicalLimit = 4001;
            tr1.WellId = addedWell.Id;
            targetrate1.Value = tr1;
            WellService.AddWellTargetRateAndUnits(targetrate1);
            WellTargetRateAndUnitsDTO targetrate2 = new WellTargetRateAndUnitsDTO();
            targetrate2.Units = getTaregtRates.Units;
            WellTargetRateDTO tr2 = new WellTargetRateDTO();
            tr2.StartDate = new DateTime(2017, 11, 18).ToUniversalTime();
            tr2.EndDate = new DateTime(2017, 11, 25).ToUniversalTime();
            tr2.OilMinimum = 2002;
            tr2.OilLowerBound = 2502;
            tr2.OilTarget = 3002;
            tr2.OilUpperBound = 3502;
            tr2.OilTechnicalLimit = 4002;

            tr2.WaterMinimum = 2002;
            tr2.WaterLowerBound = 2502;
            tr2.WaterTarget = 3002;
            tr2.WaterUpperBound = 3502;
            tr2.WaterTechnicalLimit = 4002;

            tr2.GasMinimum = 2002;
            tr2.GasLowerBound = 2502;
            tr2.GasTarget = 3002;
            tr2.GasUpperBound = 3502;
            tr2.GasTechnicalLimit = 4002;
            tr2.WellId = addedWell.Id;
            targetrate2.Value = tr2;
            WellService.AddWellTargetRateAndUnits(targetrate2);

            getTaregtRates = WellService.GetWellTargetRateAndUnits(addedWell.Id.ToString());
            Assert.AreEqual(3, getTaregtRates.Values.Length);
            DateTime? dt = new DateTime(2017, 7, 18).ToUniversalTime();
            WellTargetRateDTO WellTRate = getTaregtRates.Values.Last();
            Assert.AreEqual(2500, Math.Ceiling((double)WellTRate.OilLowerBound), 0.1, "Oil Lower Bound value Mismatch");
            Assert.AreEqual(3500, (double)WellTRate.WaterUpperBound, 0.1, "Water Upper Bound value Mismatch");
            Assert.AreEqual(dt.Value, WellTRate.StartDate.Value);
            WellTRate.OilLowerBound = 9500;
            WellTRate.WaterUpperBound = 9500;
            WellTargetRateAndUnitsDTO UpdateTR = new WellTargetRateAndUnitsDTO();
            UpdateTR.Units = getTaregtRates.Units;
            UpdateTR.Value = WellTRate;
            WellService.UpdateWellTargetRateAndUnits(UpdateTR);
            getTaregtRates = WellService.GetWellTargetRateAndUnits(addedWell.Id.ToString());
            Assert.AreEqual(3, getTaregtRates.Values.Length);

            WellTargetRateDTO CheckAfterUpdate = getTaregtRates.Values.Where(x => x.OilLowerBound != null && Math.Ceiling(x.OilLowerBound.Value) == 9500)?.Last();
            Assert.IsNotNull(CheckAfterUpdate);
            Assert.AreEqual(9500, (double)CheckAfterUpdate.WaterUpperBound, 0.1, "Water Upper Bound value Mismatch");
            WellService.RemoveWellTargetRate(CheckAfterUpdate.Id.ToString());
            getTaregtRates = WellService.GetWellTargetRateAndUnits(addedWell.Id.ToString());
            Assert.AreEqual(2, getTaregtRates.Values.Length);

            string[] ids = getTaregtRates.Values.Select(o => o.Id.ToString()).ToArray();
            WellService.RemoveWellTargetRateGroup(ids);
            Assert.AreEqual(0, WellService.GetWellTargetRateAndUnits(addedWell.Id.ToString()).Values.Length, "Target Rates not deleted");

            //System setting for US
            ChangeUnitSystem("US");
            ChangeUnitSystemUserSetting("US");

            //Validating for US unit system setting
            // Gas Lower Bound
            Assert.AreEqual("Mscf/d", getTaregtRates.Units.GasLowerBound.UnitKey, "Mismatched between the Gas lowerbound units");
            Assert.AreEqual(2, (int)getTaregtRates.Units.GasLowerBound.Precision, "Mismatched in precision for the gas lower bound");
            Assert.AreEqual(100000, (int)getTaregtRates.Units.GasLowerBound.Max, "Mismatched in Max for the Gas lower bound");
            //Gas Minimum
            Assert.AreEqual("Mscf/d", getTaregtRates.Units.GasMinimum.UnitKey, "Mismatched in Units for Gas Minimum");
            Assert.AreEqual(2, (int)getTaregtRates.Units.GasMinimum.Precision, "Mismatched in precision for Gas Minimum");
            Assert.AreEqual(100000, (int)getTaregtRates.Units.GasMinimum.Max, "Mismatched in Max for the Gas Minimum");
            //Gas Target
            Assert.AreEqual("Mscf/d", getTaregtRates.Units.GasTarget.UnitKey, "Mismatched in Units for Gas Target");
            Assert.AreEqual(2, (int)getTaregtRates.Units.GasTarget.Precision, "Mismatched in precision for Gas Target");
            Assert.AreEqual(100000, (int)getTaregtRates.Units.GasTarget.Max, "Mismatched in Max for the Gas Minimum");
            //Gas Technical Limit
            Assert.AreEqual("Mscf/d", getTaregtRates.Units.GasTechnicalLimit.UnitKey, "Mismatched in Units for GasTechnical Limit");
            Assert.AreEqual(2, (int)getTaregtRates.Units.GasTechnicalLimit.Precision, "Mismatched in precision for GasTechnical Limit");
            Assert.AreEqual(100000, (int)getTaregtRates.Units.GasTechnicalLimit.Max, "Mismatched in Max for the GasTechnical Limit");
            //Gas Upper Bound
            Assert.AreEqual("Mscf/d", getTaregtRates.Units.GasUpperBound.UnitKey, "Mismatched in Units for Gas Upper Bound");
            Assert.AreEqual(2, (int)getTaregtRates.Units.GasUpperBound.Precision, "Mismatched in precision for Gas Upper Bound");
            Assert.AreEqual(100000, (int)getTaregtRates.Units.GasUpperBound.Max, "Mismatched in Max for the Gas Upper bound");
            //InjGasTarget
            Assert.AreEqual("Mscf", getTaregtRates.Units.InjGasTarget.UnitKey, "Mismatched in units for injection gas target");
            Assert.AreEqual(4, (int)getTaregtRates.Units.InjGasTarget.Precision, "Mismatched in precision for injection gas target");
            //InjWaterTarget
            Assert.AreEqual("STB", getTaregtRates.Units.InjWaterTarget.UnitKey, "Mismatched in units for injection water target");
            Assert.AreEqual(2, (int)getTaregtRates.Units.InjWaterTarget.Precision, "Mismatched in precision for injection water target");
            //Oil Lower Bound
            Assert.AreEqual("STB/d", getTaregtRates.Units.OilLowerBound.UnitKey, "Mismatched in Units for Oil LowerBound");
            Assert.AreEqual(1, (int)getTaregtRates.Units.OilLowerBound.Precision, "Mismatched in precision for Oil Lower Bound");
            Assert.AreEqual(10000, (int)getTaregtRates.Units.OilLowerBound.Max, "Mismatched in Max for the Oil Lower bound");
            //oil Minimum
            Assert.AreEqual("STB/d", getTaregtRates.Units.OilMinimum.UnitKey, "Mismatched in Units for Oil Minimum");
            Assert.AreEqual(1, (int)getTaregtRates.Units.OilMinimum.Precision, "Mismatched in precision for Oil Minimum");
            Assert.AreEqual(10000, (int)getTaregtRates.Units.OilMinimum.Max, "Mismatched in Max for the Oil Minimum");
            //Oil TARGET
            Assert.AreEqual("STB/d", getTaregtRates.Units.OilTarget.UnitKey, "Mismatched in Units for Oil Target");
            Assert.AreEqual(1, (int)getTaregtRates.Units.OilTarget.Precision, "Mismatched in precision for Oil Target");
            Assert.AreEqual(10000, (int)getTaregtRates.Units.OilTarget.Max, "Mismatched in Max for the Oil Target");
            //Oil Technical Limit
            Assert.AreEqual("STB/d", getTaregtRates.Units.OilTechnicalLimit.UnitKey, "Mismatched in Units for Oil technical limit");
            Assert.AreEqual(1, (int)getTaregtRates.Units.OilTechnicalLimit.Precision, "Mismatched in precision for Oil technical limit");
            Assert.AreEqual(10000, (int)getTaregtRates.Units.OilTechnicalLimit.Max, "Mismatched in Max for the Oil technical limit");
            //Oil Upper bound
            Assert.AreEqual("STB/d", getTaregtRates.Units.OilUpperBound.UnitKey, "Mismatched in Units for Oil upper Bound");
            Assert.AreEqual(1, (int)getTaregtRates.Units.OilUpperBound.Precision, "Mismatched in precision for Oil upper Bound");
            Assert.AreEqual(10000, (int)getTaregtRates.Units.OilUpperBound.Max, "Mismatched in Max for the Oil upper bound");
            // Water Lower Bound
            Assert.AreEqual("STB/d", getTaregtRates.Units.WaterLowerBound.UnitKey, "Mismatched in Units for water LowerBound");
            Assert.AreEqual(1, (int)getTaregtRates.Units.WaterLowerBound.Precision, "Mismatched in precision for water Lower Bound");
            Assert.AreEqual(10000, (int)getTaregtRates.Units.WaterLowerBound.Max, "Mismatched in Max for the water Lower bound");
            // Water Minimum
            Assert.AreEqual("STB/d", getTaregtRates.Units.WaterMinimum.UnitKey, "Mismatched in Units for water Minimum");
            Assert.AreEqual(1, (int)getTaregtRates.Units.WaterMinimum.Precision, "Mismatched in precision for water Minimum");
            Assert.AreEqual(10000, (int)getTaregtRates.Units.WaterMinimum.Max, "Mismatched in Max for the water Minimum ");
            //Water Target
            Assert.AreEqual("STB/d", getTaregtRates.Units.WaterTarget.UnitKey, "Mismatched in Units for Water Target");
            Assert.AreEqual(1, (int)getTaregtRates.Units.WaterTarget.Precision, "Mismatched in precision for Water Target");
            Assert.AreEqual(10000, (int)getTaregtRates.Units.WaterTarget.Max, "Mismatched in Max for the Water Target");
            // Water TechnicalLimit
            Assert.AreEqual("STB/d", getTaregtRates.Units.WaterTechnicalLimit.UnitKey, "Mismatched in Units for Water Technical Limit");
            Assert.AreEqual(1, (int)getTaregtRates.Units.WaterTechnicalLimit.Precision, "Mismatched in precision for Water Technical Limit");
            Assert.AreEqual(10000, (int)getTaregtRates.Units.WaterTechnicalLimit.Max, "Mismatched in Max for the Water Technical Limit");
            //Water Upper bound
            Assert.AreEqual("STB/d", getTaregtRates.Units.WaterUpperBound.UnitKey, "Mismatched in Units foR Water Upper bound");
            Assert.AreEqual(1, (int)getTaregtRates.Units.WaterUpperBound.Precision, "Mismatched in precision for Water Upper Bound");
            Assert.AreEqual(10000, (int)getTaregtRates.Units.WaterUpperBound.Max, "Mismatched in Max for the Water Upper bound");

            // system settings for Metric
            ChangeUnitSystem("Metric");
            ChangeUnitSystemUserSetting("Metric");
            WellTargetRateArrayAndUnitsDTO metric_getTaregtRates = WellService.GetWellTargetRateAndUnits(addedWell.Id.ToString());

            //Validating for Metric unit system setting
            // Gas Lower Bound
            Assert.AreEqual("sm3/d", metric_getTaregtRates.Units.GasLowerBound.UnitKey, "Mismatched between the Gas lowerbound units");
            Assert.AreEqual(3, (int)metric_getTaregtRates.Units.GasLowerBound.Precision, "Mismatched in precision for the gas lower bound");
            Assert.AreEqual(UnitsConversion("Mscf/d", getTaregtRates.Units.GasLowerBound.Max).Value, metric_getTaregtRates.Units.GasLowerBound.Max.Value, 0.2);
            //Gas Minimum
            Assert.AreEqual("sm3/d", metric_getTaregtRates.Units.GasMinimum.UnitKey, "Mismatched in Units for Gas Minimum");
            Assert.AreEqual(3, (int)metric_getTaregtRates.Units.GasMinimum.Precision, "Mismatched in precision for Gas Minimum");
            Assert.AreEqual(UnitsConversion("Mscf/d", getTaregtRates.Units.GasMinimum.Max).Value, metric_getTaregtRates.Units.GasMinimum.Max.Value, 0.2);
            //Gas Target
            Assert.AreEqual("sm3/d", metric_getTaregtRates.Units.GasTarget.UnitKey, "Mismatched in Units for Gas Target");
            Assert.AreEqual(3, (int)metric_getTaregtRates.Units.GasTarget.Precision, "Mismatched in precision for Gas Target");
            Assert.AreEqual(UnitsConversion("Mscf/d", getTaregtRates.Units.GasTarget.Max).Value, metric_getTaregtRates.Units.GasTarget.Max.Value, 0.2);

            //Gas Technical Limit
            Assert.AreEqual("sm3/d", metric_getTaregtRates.Units.GasTechnicalLimit.UnitKey, "Mismatched in Units for GasTechnical Limit");
            Assert.AreEqual(3, (int)metric_getTaregtRates.Units.GasTechnicalLimit.Precision, "Mismatched in precision for GasTechnical Limit");
            Assert.AreEqual(UnitsConversion("Mscf/d", getTaregtRates.Units.GasTechnicalLimit.Max).Value, metric_getTaregtRates.Units.GasTechnicalLimit.Max.Value, 0.2);

            //Gas Upper Bound
            Assert.AreEqual("sm3/d", metric_getTaregtRates.Units.GasUpperBound.UnitKey, "Mismatched in Units for Gas Upper Bound");
            Assert.AreEqual(3, (int)metric_getTaregtRates.Units.GasUpperBound.Precision, "Mismatched in precision for Gas Upper Bound");
            Assert.AreEqual(UnitsConversion("Mscf/d", getTaregtRates.Units.GasUpperBound.Max).Value, metric_getTaregtRates.Units.GasUpperBound.Max.Value, 0.2);

            //InjGasTarget
            Assert.AreEqual("sm3", metric_getTaregtRates.Units.InjGasTarget.UnitKey, "Mismatched in units for injection gas target");
            Assert.AreEqual(3, (int)metric_getTaregtRates.Units.InjGasTarget.Precision, "Mismatched in precision for injection gas target");

            //InjWaterTarget
            Assert.AreEqual("sm3", metric_getTaregtRates.Units.InjWaterTarget.UnitKey, "Mismatched in units for injection water target");
            Assert.AreEqual(3, (int)metric_getTaregtRates.Units.InjWaterTarget.Precision, "Mismatched in precision for injection water target");

            //Oil Lower Bound
            Assert.AreEqual("sm3/d", metric_getTaregtRates.Units.OilLowerBound.UnitKey, "Mismatched in Units for Oil LowerBound");
            Assert.AreEqual(3, (int)metric_getTaregtRates.Units.OilLowerBound.Precision, "Mismatched in precision for Oil Lower Bound");
            Assert.AreEqual(UnitsConversion("STB/d", getTaregtRates.Units.OilLowerBound.Max).Value, metric_getTaregtRates.Units.OilLowerBound.Max.Value, 0.2);

            //oil Minimum
            Assert.AreEqual("sm3/d", metric_getTaregtRates.Units.OilMinimum.UnitKey, "Mismatched in Units for Oil Minimum");
            Assert.AreEqual(3, (int)metric_getTaregtRates.Units.OilMinimum.Precision, "Mismatched in precision for Oil Minimum");
            Assert.AreEqual(UnitsConversion("STB/d", getTaregtRates.Units.OilMinimum.Max).Value, metric_getTaregtRates.Units.OilMinimum.Max.Value, 0.2);
            //Oil TARGET
            Assert.AreEqual("sm3/d", metric_getTaregtRates.Units.OilTarget.UnitKey, "Mismatched in Units for Oil Target");
            Assert.AreEqual(3, (int)metric_getTaregtRates.Units.OilTarget.Precision, "Mismatched in precision for Oil Target");
            Assert.AreEqual(UnitsConversion("STB/d", getTaregtRates.Units.OilTarget.Max).Value, metric_getTaregtRates.Units.OilTarget.Max.Value, 0.2);
            //Oil Technical Limit
            Assert.AreEqual("sm3/d", metric_getTaregtRates.Units.OilTechnicalLimit.UnitKey, "Mismatched in Units for Oil technical limit");
            Assert.AreEqual(3, (int)metric_getTaregtRates.Units.OilTechnicalLimit.Precision, "Mismatched in precision for Oil technical limit");
            Assert.AreEqual(UnitsConversion("STB/d", getTaregtRates.Units.OilTechnicalLimit.Max).Value, metric_getTaregtRates.Units.OilTechnicalLimit.Max.Value, 0.2);
            //Oil Upper bound
            Assert.AreEqual("sm3/d", metric_getTaregtRates.Units.OilUpperBound.UnitKey, "Mismatched in Units for Oil upper Bound");
            Assert.AreEqual(3, (int)metric_getTaregtRates.Units.OilUpperBound.Precision, "Mismatched in precision for Oil upper Bound");
            Assert.AreEqual(UnitsConversion("STB/d", getTaregtRates.Units.OilUpperBound.Max).Value, metric_getTaregtRates.Units.OilUpperBound.Max.Value, 0.2);
            // Water Lower Bound
            Assert.AreEqual("sm3/d", metric_getTaregtRates.Units.WaterLowerBound.UnitKey, "Mismatched in Units for water LowerBound");
            Assert.AreEqual(3, (int)metric_getTaregtRates.Units.WaterLowerBound.Precision, "Mismatched in precision for water Lower Bound");
            Assert.AreEqual(UnitsConversion("STB/d", getTaregtRates.Units.WaterLowerBound.Max).Value, metric_getTaregtRates.Units.WaterLowerBound.Max.Value, 0.2);
            // Water Minimum
            Assert.AreEqual("sm3/d", metric_getTaregtRates.Units.WaterMinimum.UnitKey, "Mismatched in Units for water Minimum");
            Assert.AreEqual(3, (int)metric_getTaregtRates.Units.WaterMinimum.Precision, "Mismatched in precision for water Minimum");
            Assert.AreEqual(UnitsConversion("STB/d", getTaregtRates.Units.WaterMinimum.Max).Value, metric_getTaregtRates.Units.WaterMinimum.Max.Value, 0.2);
            //Water Target
            Assert.AreEqual("sm3/d", metric_getTaregtRates.Units.WaterTarget.UnitKey, "Mismatched in Units for Water Target");
            Assert.AreEqual(3, (int)metric_getTaregtRates.Units.WaterTarget.Precision, "Mismatched in precision for Water Target");
            Assert.AreEqual(UnitsConversion("STB/d", getTaregtRates.Units.WaterTarget.Max).Value, metric_getTaregtRates.Units.WaterTarget.Max.Value, 0.2);
            // Water TechnicalLimit
            Assert.AreEqual("sm3/d", metric_getTaregtRates.Units.WaterTechnicalLimit.UnitKey, "Mismatched in Units for Water Technical Limit");
            Assert.AreEqual(3, (int)metric_getTaregtRates.Units.WaterTechnicalLimit.Precision, "Mismatched in precision for Water Technical Limit");
            Assert.AreEqual(UnitsConversion("STB/d", getTaregtRates.Units.WaterTechnicalLimit.Max).Value, metric_getTaregtRates.Units.WaterTechnicalLimit.Max.Value, 0.2);
            //Water Upper bound
            Assert.AreEqual("sm3/d", metric_getTaregtRates.Units.WaterUpperBound.UnitKey, "Mismatched in Units foR Water Upper bound");
            Assert.AreEqual(3, (int)metric_getTaregtRates.Units.WaterUpperBound.Precision, "Mismatched in precision for Water Upper Bound");
            Assert.AreEqual(UnitsConversion("STB/d", getTaregtRates.Units.WaterUpperBound.Max).Value, metric_getTaregtRates.Units.WaterUpperBound.Max.Value, 0.2);

            for (int i = 0; i < metric_getTaregtRates.Values.Length; i++)
            {
                //Oil
                Assert.AreEqual(UnitsConversion("STB/d", (double)getTaregtRates.Values[i].OilLowerBound), (double)metric_getTaregtRates.Values[i].OilLowerBound);
                Assert.AreEqual(UnitsConversion("Mscf/d", (double)getTaregtRates.Values[i].OilMinimum), (double)metric_getTaregtRates.Values[i].OilMinimum);
                Assert.AreEqual(UnitsConversion("Mscf/d", (double)getTaregtRates.Values[i].OilTarget), (double)metric_getTaregtRates.Values[i].OilTarget);
                Assert.AreEqual(UnitsConversion("Mscf/d", (double)getTaregtRates.Values[i].OilTechnicalLimit), (double)metric_getTaregtRates.Values[i].OilTechnicalLimit);
                Assert.AreEqual(UnitsConversion("Mscf/d", (double)getTaregtRates.Values[i].OilUpperBound), (double)metric_getTaregtRates.Values[i].OilUpperBound);
                //water
                Assert.AreEqual(UnitsConversion("STB/d", (double)getTaregtRates.Values[i].WaterLowerBound), (double)metric_getTaregtRates.Values[i].WaterLowerBound);
                Assert.AreEqual(UnitsConversion("STB/d", (double)getTaregtRates.Values[i].WaterMinimum), (double)metric_getTaregtRates.Values[i].WaterMinimum);
                Assert.AreEqual(UnitsConversion("STB/d", (double)getTaregtRates.Values[i].WaterTarget), (double)metric_getTaregtRates.Values[i].WaterTarget);
                Assert.AreEqual(UnitsConversion("STB/d", (double)getTaregtRates.Values[i].WaterTechnicalLimit), (double)metric_getTaregtRates.Values[i].WaterTechnicalLimit);
                Assert.AreEqual(UnitsConversion("STB/d", (double)getTaregtRates.Values[i].WaterUpperBound), (double)metric_getTaregtRates.Values[i].WaterUpperBound);
                //Gas
                Assert.AreEqual(UnitsConversion("STB/d", (double)getTaregtRates.Values[i].GasLowerBound), (double)metric_getTaregtRates.Values[i].GasLowerBound);
                Assert.AreEqual(UnitsConversion("STB/d", (double)getTaregtRates.Values[i].GasMinimum), (double)metric_getTaregtRates.Values[i].GasMinimum);
                Assert.AreEqual(UnitsConversion("STB/d", (double)getTaregtRates.Values[i].GasTarget), (double)metric_getTaregtRates.Values[i].GasTarget);
                Assert.AreEqual(UnitsConversion("STB/d", (double)getTaregtRates.Values[i].GasTechnicalLimit), (double)metric_getTaregtRates.Values[i].GasTechnicalLimit);
                Assert.AreEqual(UnitsConversion("STB/d", (double)getTaregtRates.Values[i].GasUpperBound), (double)metric_getTaregtRates.Values[i].GasUpperBound);
            }
        }

        [TestCategory(TestCategories.WellServiceTests), TestMethod]
        public void AddUpdateDeleteWellAllocationGroup()
        {
            List<DataConnectionDTO> existingDataConnections = DataConnectionService.GetAllDataConnections().ToList();

            var allAsset = SurfaceNetworkService.GetAllAssets()?.ToList();
            AssetDTO asset = allAsset?.FirstOrDefault(a => a.Name.Equals("Default"));
            // Add Asset ID if no Default found
            if (asset == null)
            {
                string assetName = "AssetTest1";
                SurfaceNetworkService.AddAsset(new AssetDTO() { Name = assetName, Description = "Test Description" });
                var allAssets = SurfaceNetworkService.GetAllAssets().ToList();
                AssetDTO asset1 = allAssets?.FirstOrDefault(a => a.Name.Equals(assetName));
                Assert.AreEqual(asset1.Name, assetName);
                Assert.IsNotNull(asset1);
                _assetsToRemove.Add(asset1);
                asset = asset1;
            }

            //Add Oil - Well Allocation Group
            WellAllocationGroupDTO newWAGOil = new WellAllocationGroupDTO();
            newWAGOil.AllocationGroupName = "Oil";
            newWAGOil.DataConnection = new DataConnectionDTO { ProductionDomain = "22225", Site = "TEST1", Service = s_cvsService };
            newWAGOil.Phase = Enums.AllocationGroupPhaseId.Oil;
            newWAGOil.FacilityId = "1234566";
            newWAGOil.UDC = "ALLOCVOD";
            newWAGOil.Asset = asset.Id;

            //Add Oil
            WellService.AddWellAllocationGroup(newWAGOil);
            var wellallOil = WellService.GetWellAllocationGroupByPhase(Convert.ToString(Convert.ToInt32(newWAGOil.Phase))); //HJ : Not need to use ToInt32
            var wellgrpOil = wellallOil.Where(s => s.Asset == newWAGOil.Asset).FirstOrDefault();

            var WAGdto1 = WellService.GetWellAllocationGroup(wellgrpOil.Id.ToString());
            Assert.AreEqual(wellgrpOil.Id, WAGdto1.Id);

            //Water
            WellAllocationGroupDTO newWAGWater = new WellAllocationGroupDTO();
            newWAGWater.AllocationGroupName = "Water";
            newWAGWater.DataConnection = new DataConnectionDTO { ProductionDomain = "22226", Site = "TEST2", Service = s_cvsService };
            newWAGWater.Phase = Enums.AllocationGroupPhaseId.Water;
            newWAGWater.FacilityId = "1234566";
            newWAGWater.UDC = "ALLOCVOD";
            newWAGWater.Asset = asset.Id;

            //Add Well Allocation Group - Water
            WellService.AddWellAllocationGroup(newWAGWater);

            var wellallWater = WellService.GetWellAllocationGroupByPhase(Convert.ToString(Convert.ToInt32(newWAGWater.Phase)));
            var wellgrpWater = wellallWater.Where(s => s.Asset == newWAGWater.Asset).FirstOrDefault();

            var WAGdto2 = WellService.GetWellAllocationGroup(wellgrpWater.Id.ToString());
            Assert.AreEqual(wellgrpWater.Id, WAGdto2.Id);

            //Gas
            WellAllocationGroupDTO newWAGGas = new WellAllocationGroupDTO();
            newWAGGas.AllocationGroupName = "Gas";
            newWAGGas.DataConnection = new DataConnectionDTO { ProductionDomain = "22227", Site = "TEST3", Service = s_cvsService };
            newWAGGas.Phase = Enums.AllocationGroupPhaseId.Gas;
            newWAGGas.FacilityId = "1234566";
            newWAGGas.UDC = "ALLOCVOD";
            newWAGGas.Asset = asset.Id;

            // Add Well Allocation Group - GAS
            WellService.AddWellAllocationGroup(newWAGGas);

            var wellallGas = WellService.GetWellAllocationGroupByPhase(Convert.ToString(Convert.ToInt32(newWAGGas.Phase)));
            var wellgrpGas = wellallGas.Where(s => s.Asset == newWAGGas.Asset).FirstOrDefault();

            var WAGdto3 = WellService.GetWellAllocationGroup(wellgrpGas.Id.ToString());
            Assert.AreEqual(wellgrpGas.Id, WAGdto3.Id);

            //Add Well
            var allWells = WellService.GetAllWells()?.ToList();
            WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals("WAGTest"));

            // If there aren't any defined wells, make a test well.
            if (well == null)
            {
                WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = SetDefaultFluidType(new WellDTO() { Name = "WAGTest", FacilityId = newWAGOil.FacilityId, AssemblyAPI = "1234567890", SubAssemblyAPI = "123456789012", IntervalAPI = "12345678901234", CommissionDate = DateTime.Today, DataConnection = GetDefaultCygNetDataConnection(), WellType = WellTypeId.ESP, OilAllocationGroup = wellgrpOil.Id, WaterAllocationGroup = wellgrpWater.Id, GasAllocationGroup = wellgrpGas.Id }) });
                allWells = WellService.GetAllWells()?.ToList();
                well = allWells.FirstOrDefault(w => w.Name.Equals("WAGTest"));
                Assert.IsNotNull(well, "Failed to add well.");
                if (DataConnectionService.GetAllDataConnections().FirstOrDefault(dc => dc.ProductionDomain == well.DataConnection.ProductionDomain && dc.Site == well.DataConnection.Site && dc.Service == well.DataConnection.Service) == null)
                {
                    _dataConnectionsToRemove.Add(well.DataConnection);
                }
                _wellsToRemove.Add(well);
            }

            //Get WellId
            var welldto = WellService.GetWell(well.Id.ToString());

            Assert.AreEqual(welldto.OilAllocationGroup, wellgrpOil.Id, "Failed to Map to Well Association Group - Oil");
            Assert.AreEqual(welldto.WaterAllocationGroup, wellgrpWater.Id, "Failed to Map to Well Association Group - Water");
            Assert.AreEqual(welldto.GasAllocationGroup, wellgrpGas.Id, "Failed to Map to Well Association Group - Gas");

            // While we are here, test the GetWells from GroupID call since we have a controlled environment for it
            WellDTO[] wells1 = WellService.GetWellsFromAllocationGroupId(wellgrpOil.Id.ToString());
            Assert.IsTrue(wells1.Count() == 1);

            WellDTO[] wells2 = WellService.GetWellsFromAllocationGroupId(wellgrpWater.Id.ToString());
            Assert.IsTrue(wells2.Count() == 1);

            WellDTO[] wells3 = WellService.GetWellsFromAllocationGroupId(wellgrpGas.Id.ToString());
            Assert.IsTrue(wells3.Count() == 1);

            //Update Well Allocation Group in Well
            // Change Name from "Oil" to "Oil1"
            WAGdto1.AllocationGroupName = "Oil1";
            WAGdto1.DataConnection.Site = "TEST11";
            WellService.UpdateWellAllocationGroup(WAGdto1);
            Assert.AreEqual(WAGdto1.AllocationGroupName, "Oil1", "Failed to Update well.");

            // Change Name from "Water" to "Water1"
            WAGdto2.AllocationGroupName = "Water1";
            WAGdto2.DataConnection.Site = "TEST22";
            WellService.UpdateWellAllocationGroup(WAGdto2);
            Assert.AreEqual(WAGdto2.AllocationGroupName, "Water1", "Failed to Update well.");

            // Change Phase from "Gas" to "Gas1"
            WAGdto3.AllocationGroupName = "Gas1";
            WAGdto3.DataConnection.Site = "TEST33";
            WellService.UpdateWellAllocationGroup(WAGdto3);
            Assert.AreEqual(WAGdto3.AllocationGroupName, "Gas1", "Failed to Update well.");

            //Remove Well
            //WellService.RemoveWell(well.Id.ToString());
            WellConfigurationService.RemoveWellConfig(welldto.Id.ToString());
            WellDTO compare1 = WellService.GetWell(welldto.Id.ToString());
            compare1 = WellService.GetWell(welldto.Id.ToString());
            //Assert.IsNull(compare1);
            _wellsToRemove.Remove(welldto);

            // Delete from WellAllocationGroup
            WellAllocationGroupDTO compareWAG1 = WellService.GetWellAllocationGroup(WAGdto1.Id.ToString());
            WellService.RemoveWellAllocationGroup(WAGdto1.Id.ToString());
            compareWAG1 = WellService.GetWellAllocationGroup(WAGdto1.Id.ToString());
            Assert.IsNull(compareWAG1);

            // Delete from WellAllocationGroup
            WellAllocationGroupDTO compareWAG2 = WellService.GetWellAllocationGroup(WAGdto2.Id.ToString());
            WellService.RemoveWellAllocationGroup(WAGdto2.Id.ToString());
            compareWAG2 = WellService.GetWellAllocationGroup(WAGdto2.Id.ToString());
            Assert.IsNull(compareWAG2);

            // Delete from WellAllocationGroup
            WellAllocationGroupDTO compareWAG3 = WellService.GetWellAllocationGroup(WAGdto3.Id.ToString());
            WellService.RemoveWellAllocationGroup(WAGdto3.Id.ToString());
            compareWAG3 = WellService.GetWellAllocationGroup(WAGdto3.Id.ToString());
            Assert.IsNull(compareWAG3);
        }

        /// <summary>
        /// Tester              : Pravin D. Survase
        /// Description         : Add a WAG Well in ForeSite.
        /// Base Jira Ticket    : FRWM-1084 [UI - Add WAG Injection Wells (Well Configuration)]
        /// Auto Test Ticket    : FRWM-1447 [API Testing]
        /// </summary>
        [TestCategory(TestCategories.WellServiceTests), TestMethod]
        public void AddRemoveWAGWell()
        {
            try
            {
                // Get all the existing wells from ForeSite
                var allWellsBefore = WellService.GetAllWells().ToList();

                var toAddWAGWell = SetDefaultFluidType(new WellDTO()
                {
                    Name = "WAGWELLTEST",
                    FacilityId = "WING_0001",
                    IntervalAPI = "IntervalAPI",
                    SubAssemblyAPI = "SubAssemblyAPI",
                    AssemblyAPI = "AssemblyAPI",
                    CommissionDate = DateTime.Today,
                    WellType = WellTypeId.WGInj
                });

                WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = toAddWAGWell });
                var allWells = WellService.GetAllWells().ToList();
                WellDTO well = allWells.FirstOrDefault(w => allWellsBefore.FirstOrDefault(wb => wb.Id == w.Id) == null);
                Assert.IsNotNull(well);
                Assert.AreEqual(toAddWAGWell.Name, well.Name);
                _wellsToRemove.Add(well);
                WellDTO compare = WellService.GetWell(well.Id.ToString());
                CompareTwoWells(toAddWAGWell, compare, false);

                WellConfigurationService.RemoveWellConfig(well.Id.ToString());
                compare = WellService.GetWell(well.Id.ToString());
                Assert.IsNull(compare);
                _wellsToRemove.Remove(well);

                //Get Well History
                WellHistoryDTO[] welHistory = WellService.GetWellHistory(well.Id.ToString());
                Assert.AreEqual(1, welHistory.Length);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void AddTargetRate(WellDTO addedWell, DateTime? date = null)
        {
            WellTargetRateArrayAndUnitsDTO getTaregtRates = WellService.GetWellTargetRateAndUnits(addedWell.Id.ToString());
            WellTargetRateAndUnitsDTO targetrate = new WellTargetRateAndUnitsDTO();
            targetrate.Units = getTaregtRates.Units;
            WellTargetRateDTO tr = new WellTargetRateDTO();
            if (date == null)
                tr.StartDate = addedWell.CommissionDate.Value.AddDays(2).ToUniversalTime();
            else
                tr.StartDate = date;
            tr.EndDate = tr.StartDate;
            tr.OilMinimum = 2000;
            tr.OilLowerBound = 2500;
            tr.OilTarget = 3000;
            tr.OilUpperBound = 3500;
            tr.OilTechnicalLimit = 4000;
            tr.WaterMinimum = 2000;
            tr.WaterLowerBound = 2500;
            tr.WaterTarget = 3000;
            tr.WaterUpperBound = 3500;
            tr.WaterTechnicalLimit = 4000;
            tr.GasMinimum = 2000;
            tr.GasLowerBound = 2500;
            tr.GasTarget = 3000;
            tr.GasUpperBound = 3500;
            tr.GasTechnicalLimit = 4000;
            tr.WellId = addedWell.Id;
            targetrate.Value = tr;
            WellService.AddWellTargetRateAndUnits(targetrate);
            getTaregtRates = WellService.GetWellTargetRateAndUnits(addedWell.Id.ToString());
            Assert.AreEqual(1, getTaregtRates.Values.Count(), "Failed to Add Target rate");
        }

        [TestCategory(TestCategories.WellServiceTests), TestMethod]
        public void CheckTargetRateonMOPChange()
        {
            string facilityIdBase = "ESPWELL_";
            string facilityId = s_isRunningInATS ? facilityIdBase + "00001" : facilityIdBase + "0001";

            try
            {
                WellDTO addedWell = AddNonRRLWell(facilityId, WellTypeId.ESP, false);
                Assert.IsNotNull(addedWell);
                AddTargetRate(addedWell, addedWell.CommissionDate.Value.ToUniversalTime());
                WellTargetRateArrayAndUnitsDTO getTaregtRates = WellService.GetWellTargetRateAndUnits(addedWell.Id.ToString());
                Assert.IsFalse(getTaregtRates.Values.FirstOrDefault().IsReadOnly.Value, "Should be editable");

                //Change MOP
                WellMoPHistoryDTO wellMoPHistoryDTO = new WellMoPHistoryDTO();
                wellMoPHistoryDTO.WellId = addedWell.Id;
                wellMoPHistoryDTO.WellType = WellTypeId.RRL;
                wellMoPHistoryDTO.ChangeDate = DateTime.Today.AddDays(2).ToUniversalTime();
                wellMoPHistoryDTO.Comment = "ESP to RRL";

                WellService.ChangeWellType(wellMoPHistoryDTO);
                addedWell = WellService.GetAllWells().FirstOrDefault(w => w.Name.Equals(facilityId));
                Assert.AreEqual(WellTypeId.RRL, addedWell.WellType, "Failed to change Welll type");
                getTaregtRates = WellService.GetWellTargetRateAndUnits(addedWell.Id.ToString());
                Assert.IsTrue(getTaregtRates.Values.FirstOrDefault().IsReadOnly.Value, "Should not be editable");
            }
            finally
            {
                RemoveWell(facilityId);
            }
        }

        [TestCategory(TestCategories.WellServiceTests), TestMethod]
        public void ChangeWellType()
        {
            try
            {
                WellDTO addedWell = AddNonRRLWell("ESPWELL_0001", WellTypeId.ESP, false);
                Assert.IsNotNull(addedWell);
                AssemblyDTO assembly = WellboreComponentService.GetAssemblyByWellId(addedWell.Id.ToString());
                WellMoPHistoryDTO[] mopHistory = WellService.GetWellMoPHistory(addedWell.Id.ToString());
                Assert.AreEqual(1, mopHistory.Count(), "Only Current record should exist");
                Assert.AreEqual(0, mopHistory.FirstOrDefault().Id, "Current record Id should be zero");
                Assert.AreEqual(addedWell.Id, mopHistory.FirstOrDefault().WellId, "Mismactch in wellId between added well & Mop History");
                ModelFileHeaderDTO[] modelFiles = ModelFileService.GetListModelFileHeaderByWell(addedWell.Id.ToString());
                //Adding WellSettings, Well Test, Target, Alarms
                SettingDTO[] allAcceptanceLimitSettings = SettingService.GetSettingsByCategory(((int)SettingCategory.AcceptanceLimit).ToString());
                WellSettingDTO setng1 = new WellSettingDTO();
                setng1.HasFlag = false;
                setng1.Id = 0;
                setng1.NumericValue = 1;
                setng1.SettingId = allAcceptanceLimitSettings.FirstOrDefault(x => x.Description == "Minimum L factor acceptance limit").Id;
                setng1.StringValue = null;
                setng1.WellId = addedWell.Id;
                WellSettingDTO setng2 = new WellSettingDTO();
                setng2.HasFlag = false;
                setng2.Id = 0;
                setng2.NumericValue = 1;
                setng2.SettingId = allAcceptanceLimitSettings.FirstOrDefault(x => x.Description == "Maximum L factor acceptance limit").Id;
                setng2.StringValue = null;
                setng2.WellId = addedWell.Id;
                WellSettingDTO[] wellSettings = new WellSettingDTO[] { setng1, setng2 };
                SettingService.SaveWellSettings(wellSettings);
                wellSettings = SettingService.GetWellSettingsByWellIdAndCategory(addedWell.Id.ToString(), ((int)SettingCategory.AcceptanceLimit).ToString());
                Assert.AreEqual(2, wellSettings.Count(), "Faile to add well settings");
                AddTargetRate(addedWell);

                //Change MOP
                WellMoPHistoryDTO wellMoPHistoryDTO = new WellMoPHistoryDTO();
                wellMoPHistoryDTO.WellId = addedWell.Id;
                wellMoPHistoryDTO.WellType = WellTypeId.RRL;
                wellMoPHistoryDTO.ChangeDate = addedWell.CommissionDate.Value.ToUniversalTime();
                wellMoPHistoryDTO.Comment = "ESP to RRL";

                WellService.ChangeWellType(wellMoPHistoryDTO);
                addedWell = WellService.GetAllWells().FirstOrDefault(w => w.Name.Equals("ESPWELL_0001"));
                Assert.AreEqual(WellTypeId.RRL, addedWell.WellType, "Failed to change Welll type");
                mopHistory = WellService.GetWellMoPHistory(addedWell.Id.ToString());
                Assert.AreEqual(2, mopHistory.Count(), "Failed to add a record in MOP History table after MOP change");
                foreach (WellMoPHistoryDTO mop in mopHistory)
                {
                    Assert.AreEqual(addedWell.Id, mop.WellId, "Mismactch in wellId between added well & Mop History");
                }
                wellSettings = SettingService.GetWellSettingsByWellIdAndCategory(addedWell.Id.ToString(), ((int)SettingCategory.AcceptanceLimit).ToString());
                Assert.AreEqual(2, wellSettings.Count(), "Faile to get well settings after MOP change");
                wellSettings = SettingService.GetWellSettingsByWellIdAndCategory(addedWell.Id.ToString(), ((int)SettingCategory.AcceptanceLimit).ToString(), mopHistory.Where(x => x.Id != 0).FirstOrDefault().Id.ToString());
                Assert.AreEqual(2, wellSettings.Count(), "Faile to get well settings for MOP change");
                modelFiles = ModelFileService.GetListModelFileHeaderByWell(addedWell.Id.ToString());
                Assert.AreEqual(0, modelFiles.Count(), "Failed to delete the model file after the MOP change");
                WellTestArrayAndUnitsDTO wellTest = WellTestDataService.GetWellTestDataByWellId(addedWell.Id.ToString());
                Assert.AreEqual(WellTestStatus.TUNING_NOT_COMPLETE, wellTest.Values.FirstOrDefault().Status, "Failed to set the Tuning status after MOP change");
                WellTargetRateArrayAndUnitsDTO getTaregtRates = WellService.GetWellTargetRateAndUnits(addedWell.Id.ToString());
                Assert.AreEqual(0, getTaregtRates.Values.Count(), "Failed to delete out of bound Target rate after Mop change");
            }
            finally
            {
                RemoveWell("ESPWELL_0001");
            }
        }

        [TestCategory(TestCategories.WellServiceTests), TestMethod]
        public void CheckAlarmonChangeWellType()
        {
            string facilityId;
            if (s_isRunningInATS)
                facilityId = "RPOC_00001";
            else
                facilityId = "RPOC_0001";
            try
            {
                WellDTO well = AddRRLWell(facilityId);
                DynaCardEntryDTO FullDynaCard = DynacardService.ScanDynacard(well.Id.ToString(), ((int)CardType.Full).ToString());
                if (FullDynaCard.ErrorMessage == "Success")
                {
                    DynaCardEntryDTO surfaceCard = DynacardService.AnalyzeSelectedSurfaceCardExclusive(well.Id.ToString(), FullDynaCard.TimestampInTicks, ((int)CardType.Full).ToString(), "true");
                    if (surfaceCard.ErrorMessage == "Success")
                    {
                        CurrentAlarmDTO[] activeAlarms = IntelligentAlarmService.GetActiveIntelligentAlarmsByWellId(well.Id.ToString(), well.WellType.ToString());
                        Assert.IsTrue(activeAlarms.Count() > 0);
                        //Change MOP
                        WellMoPHistoryDTO wellMoPHistoryDTO = new WellMoPHistoryDTO();
                        wellMoPHistoryDTO.WellId = well.Id;
                        wellMoPHistoryDTO.WellType = WellTypeId.ESP;
                        wellMoPHistoryDTO.ChangeDate = well.CommissionDate.Value.AddDays(2).ToUniversalTime();
                        wellMoPHistoryDTO.Comment = "RRL to ESP";

                        WellService.ChangeWellType(wellMoPHistoryDTO);
                        well = WellService.GetAllWells().FirstOrDefault(w => w.Name.Equals(facilityId));
                        Assert.AreEqual(WellTypeId.ESP, well.WellType, "Failed to change Welll type");
                        WellMoPHistoryDTO[] mopHistory = WellService.GetWellMoPHistory(well.Id.ToString());
                        Assert.AreEqual(2, mopHistory.Count(), "Failed to add a record in MOP History table after MOP change");
                        activeAlarms = IntelligentAlarmService.GetActiveIntelligentAlarmsByWellId(well.Id.ToString(), well.WellType.ToString());
                        Assert.IsTrue(activeAlarms.Count() == 0, "Failed to turn off the active Alarms after MOP change");
                    }
                    else
                        Assert.Fail("Failed to Analyze Card");
                }
                else
                    Assert.Fail("Failed to Scan Card");
            }
            finally
            {
                RemoveWell(facilityId);
            }
        }

        [TestCategory(TestCategories.WellServiceTests), TestMethod]
        public void AddWellWithSpecialCharactersAndGetByName()
        {
            var allWellsBefore = WellService.GetAllWells().ToList();
            var toAdd = SetDefaultFluidType(new WellDTO() { Name = "CASETest##", FacilityId = "CASETEST", IntervalAPI = "IntervalAPI", SubAssemblyAPI = "SubAssemblyAPI", AssemblyAPI = "AssemblyAPI", CommissionDate = DateTime.Today, WellType = WellTypeId.RRL });
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = toAdd });
            var allWellsAfter = WellService.GetAllWells().ToList();
            WellDTO addedWell = allWellsAfter.FirstOrDefault(t => t.Name == toAdd.Name);
            Assert.IsNotNull(addedWell, $"Failed to add well named {toAdd.Name}.");
            _wellsToRemove.Add(addedWell);
            WellDTO addedWellByName = WellService.GetWellByName(toAdd.Name);
            Assert.IsNotNull(addedWellByName, $"Failed to get add well named {toAdd.Name} by name.");
        }

        [TestCategory(TestCategories.WellServiceTests), TestMethod]
        public void GetUserAssets()
        {
            try
            {
                SystemSettingDTO userassetsettingvalue = SettingService.GetSystemSettingByName(SettingServiceStringConstants.ALL_USERS_CAN_ACCESS_UNMAPPED_WELLS);
                userassetsettingvalue.NumericValue = 0;
                SettingService.SaveSystemSetting(userassetsettingvalue);
                UserDTO user = AdministrationService.GetUser(AuthenticatedUser.Id.ToString());
                AssetDTO assetToAdd = new AssetDTO() { Name = "GetUserAssets", Description = "GetUserAssets" };
                AssetDTO asset = AddAndGetAsset(assetToAdd);
                assetToAdd = new AssetDTO() { Name = "GetUserAssets2", Description = "GetUserAssets2" };
                AssetDTO assetUserDoesntGet = AddAndGetAsset(assetToAdd);
                if (user.Assets == null)
                {
                    user.Assets = new List<AssetDTO>();
                }
                user.Assets.Add(asset);
                AdministrationService.UpdateUser(user);
                AssetDTO[] userAssets = WellService.GetUserAssets();
                AssetDTO userAsset = userAssets.FirstOrDefault(t => t.Name == asset.Name);
                Assert.IsNotNull(userAsset, "Failed to get asset added to user.");
                Assert.AreEqual(1, userAssets.Length, "User should only have one asset.");
            }
            finally
            {
                SetValuesInSystemSettings(SettingServiceStringConstants.ALL_USERS_CAN_ACCESS_UNMAPPED_WELLS, "1");
            }
        }

        private void TestOneAssetFilter(WellFilterDTO wellFilter, WellDTO[] expectedWells, int queryNumber)
        {
            WellDTO[] wells = WellService.GetWellsByFilter(wellFilter);
            List<WellDTO> missingWells = expectedWells.Where(t => !wells.Any(u => t.Id == u.Id)).ToList();
            List<WellDTO> unexpectedWells = wells.Where(t => !expectedWells.Any(u => u.Id == t.Id)).ToList();
            Assert.AreEqual(0, missingWells.Count + unexpectedWells.Count, $"Query {queryNumber}: " + (missingWells.Any() ? $"Missing well{(missingWells.Count == 1 ? "" : "s")}: {string.Join(", ", missingWells.Select(t => t.Name))}" : "") +
                (unexpectedWells.Any() ? (missingWells.Any() ? ", " : "") + $"Unexpected well{(unexpectedWells.Count == 1 ? "" : "s")}: {string.Join(", ", unexpectedWells.Select(t => t.Name))}" : ""));
        }

        [TestCategory(TestCategories.WellServiceTests), TestMethod]
        public void GetWellFilter_Assets()
        {
            UserDTO user = AdministrationService.GetUser(AuthenticatedUser.Id.ToString());
            AssetDTO assetToAdd;
            assetToAdd = new AssetDTO() { Name = "GetWellFilter_Assets 1", Description = "GetWellFilter_Assets 1" };
            AssetDTO asset1 = AddAndGetAsset(assetToAdd);
            assetToAdd = new AssetDTO() { Name = "GetWellFilter_Assets 2", Description = "GetWellFilter_Assets 2" };
            AssetDTO asset2 = AddAndGetAsset(assetToAdd);
            assetToAdd = new AssetDTO() { Name = "GetWellFilter_Assets 3", Description = "GetWellFilter_Assets 3" };
            AssetDTO asset3 = AddAndGetAsset(assetToAdd);
            if (user.Assets == null)
            {
                user.Assets = new List<AssetDTO>();
            }
            user.Assets.Add(asset1);
            user.Assets.Add(asset2);
            AdministrationService.UpdateUser(user);
            AssetDTO[] userAssets = WellService.GetUserAssets();
            AssetDTO userAsset1 = userAssets.FirstOrDefault(t => t.Id == asset1.Id);
            Assert.IsNotNull(userAsset1, $"Failed to get asset {asset1.Name} added to user.");
            AssetDTO userAsset2 = userAssets.FirstOrDefault(t => t.Id == asset2.Id);
            Assert.IsNotNull(userAsset1, $"Failed to get asset {asset2.Name} added to user.");

            WellDTO wellToAdd;
            int i = 1;
            const string wellNameBase = "Asset Test ";
            string wellName = wellNameBase + i.ToString("D2");
            wellToAdd = SetDefaultFluidType(new WellDTO() { Name = wellName, AssemblyAPI = wellName, SubAssemblyAPI = wellName, IntervalAPI = wellName, WellType = WellTypeId.RRL, CommissionDate = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Local) });
            WellDTO well1 = AddAndGetWell(wellToAdd);

            i += 1;
            wellName = wellNameBase + i.ToString("D2");
            wellToAdd = SetDefaultFluidType(new WellDTO() { Name = wellName, AssemblyAPI = wellName, SubAssemblyAPI = wellName, IntervalAPI = wellName, WellType = WellTypeId.RRL, CommissionDate = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Local), AssetId = asset1.Id });
            WellDTO well2 = AddAndGetWell(wellToAdd);

            i += 1;
            wellName = wellNameBase + i.ToString("D2");
            wellToAdd = SetDefaultFluidType(new WellDTO() { Name = wellName, AssemblyAPI = wellName, SubAssemblyAPI = wellName, IntervalAPI = wellName, WellType = WellTypeId.RRL, CommissionDate = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Local), AssetId = asset2.Id });
            WellDTO well3 = AddAndGetWell(wellToAdd);

            i += 1;
            wellName = wellNameBase + i.ToString("D2");
            wellToAdd = SetDefaultFluidType(new WellDTO() { Name = wellName, AssemblyAPI = wellName, SubAssemblyAPI = wellName, IntervalAPI = wellName, WellType = WellTypeId.RRL, CommissionDate = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Local), AssetId = asset3.Id });
            WellDTO well4 = AddAndGetWell(wellToAdd);

            int queryNumber = 1;
            TestOneAssetFilter(new WellFilterDTO() { AssetIds = new long?[] { null } }, new[] { well1 }, queryNumber++);
            TestOneAssetFilter(new WellFilterDTO() { AssetIds = new long?[] { asset1.Id } }, new[] { well2 }, queryNumber++);
            TestOneAssetFilter(new WellFilterDTO() { AssetIds = new long?[] { asset2.Id } }, new[] { well3 }, queryNumber++);
            TestOneAssetFilter(new WellFilterDTO() { AssetIds = new long?[0] }, new[] { well1, well2, well3 }, queryNumber++);
            TestOneAssetFilter(new WellFilterDTO() { AssetIds = new long?[] { asset3.Id } }, new WellDTO[0], queryNumber++);

            long?[] assetArray;
            int filterCount = 1;

            assetArray = new long?[] { null };
            WellFilterDTO wellFilter_GetWellFilter = WellService.GetWellFilter(assetArray);
            CollectionAssert.AreEquivalent(assetArray, wellFilter_GetWellFilter.AssetIds, $"GetWellFilter returned unexpected value for AssetIds on query {filterCount}.");

            assetArray = new long?[] { null, asset1.Id };
            wellFilter_GetWellFilter = WellService.GetWellFilter(assetArray);
            CollectionAssert.AreEquivalent(assetArray, wellFilter_GetWellFilter.AssetIds, $"GetWellFilter returned unexpected value for AssetIds on query {filterCount}.");

            assetArray = new long?[] { asset1.Id, asset2.Id };
            wellFilter_GetWellFilter = WellService.GetWellFilter(assetArray);
            CollectionAssert.AreEquivalent(assetArray, wellFilter_GetWellFilter.AssetIds, $"GetWellFilter returned unexpected value for AssetIds on query {filterCount}.");

            assetArray = new long?[] { asset1.Id, asset3.Id };
            wellFilter_GetWellFilter = WellService.GetWellFilter(assetArray);
            CollectionAssert.AreEquivalent(assetArray, wellFilter_GetWellFilter.AssetIds, $"GetWellFilter returned unexpected value for AssetIds on query {filterCount}.");

            assetArray = new long?[] { null, asset1.Id, asset3.Id };
            wellFilter_GetWellFilter = WellService.GetWellFilter(assetArray);
            CollectionAssert.AreEquivalent(assetArray, wellFilter_GetWellFilter.AssetIds, $"GetWellFilter returned unexpected value for AssetIds on query {filterCount}.");
        }

        [TestCategory(TestCategories.WellServiceTests), TestMethod]
        public void VerifyAssetsMapping()
        {
            try
            {
                WellDTO wellESP = AddNonRRLWell(GetFacilityId("ESPWELL_", 0001), WellTypeId.ESP);
                //Setting service All Users Can Access Unmapped Wells is set to true and verify if well displays.
                SystemSettingDTO userassetsettingvalue = SettingService.GetSystemSettingByName(SettingServiceStringConstants.ALL_USERS_CAN_ACCESS_UNMAPPED_WELLS);
                userassetsettingvalue.NumericValue = 1;
                SettingService.SaveSystemSetting(userassetsettingvalue);
                WellFilterDTO wellfilter = new WellFilterDTO();
                wellfilter.welFK_r_WellTypeTitle = "ESP";
                WellDTO[] filtered_Wells = WellService.GetWellsByFilter(wellfilter);
                Assert.AreEqual("1", filtered_Wells.Length.ToString(), "Well is not displayed on setting mapped well service true ");
                userassetsettingvalue.NumericValue = 0;
                SettingService.SaveSystemSetting(userassetsettingvalue);

                WellDTO[] filtered_Wells2 = WellService.GetWellsByFilter(wellfilter);
                //Setting service All Users Can Access Unmapped Wells is set to false and verify if well does not displays.
                Assert.AreEqual("0", filtered_Wells2.Length.ToString(), "Well is  displayed on setting mapped well service false ");
                UserDTO user = AdministrationService.GetUser(AuthenticatedUser.Id.ToString());
                //Adding Not Mapped asset to user and verify if added well displays while Setting service All Users Can Access Unmapped Wells is set to false 
                user.Assets.Add(new AssetDTO() { Name = "Not Mapped", Description = "Not Mapped" });
                AdministrationService.UpdateUser(user);
                WellDTO[] filtered_Wells3 = WellService.GetWellsByFilter(wellfilter);
                Assert.AreEqual("1", filtered_Wells3.Length.ToString(), "Well is not displayed on Not mapped asset mapped to user and setting mapped well service false ");
                AssetDTO assetToAdd = new AssetDTO() { Name = "TestAsset", Description = "TestAsset" };
                AssetDTO assetUserDoesntGet = AddAndGetAsset(assetToAdd);
                user.Assets.Add(assetUserDoesntGet);
                AdministrationService.UpdateUser(user);
                WellDTO wellESP2 = AddNonRRLWell(GetFacilityId("ESPWELL_", 0002), WellTypeId.ESP);
                WellDTO[] filtered_Wells4 = WellService.GetWellsByFilter(wellfilter);
                Assert.AreEqual("2", filtered_Wells4.Length.ToString(), "Well is not displayed on setting mapped well service true ");
                WellConfigDTO w = new WellConfigDTO();
                w.Well = wellESP;
                w.Well.AssetId = WellService.GetUserAssets().FirstOrDefault(t => t.Id != 0).Id;
                WellConfigurationService.UpdateWellConfig(w);
                _assetsToRemove.Add(assetToAdd);
                _wellsToRemove.Add(wellESP);
                _wellsToRemove.Add(wellESP2);
            }
            finally
            {
                SetValuesInSystemSettings(SettingServiceStringConstants.ALL_USERS_CAN_ACCESS_UNMAPPED_WELLS, "1");

            }
        }

        [TestCategory(TestCategories.WellServiceTests), TestMethod]
        public void InsertWellStatusHistory()
        {
            WellDTO addedWell = AddNonRRLWell("ESPWELL_0001", WellTypeId.ESP, false);
            Assert.IsNotNull(addedWell);
            _wellsToRemove.Add(addedWell);

            WellStatusHistoryDTO historyWithDefaultUserAndTime = new WellStatusHistoryDTO();
            historyWithDefaultUserAndTime.WellId = addedWell.Id;
            historyWithDefaultUserAndTime.WellStatusId = 1;

            WellService.InsertWellStatusHistory(historyWithDefaultUserAndTime);
            WellStatusHistoryDTO[] history = WellService.GetWellStatusHistory(addedWell.Id.ToString());

            Assert.AreEqual(2, history.Count(), "Failed to insert Well Status History");
            Assert.AreEqual(AuthenticatedUser.Id, history[0].ChangeUserId, "Failed to insert current user");

            DateTime compareDate = new DateTime(1941, 12, 7, 1, 1, 1, DateTimeKind.Utc);

            WellStatusHistoryDTO historyWithSetUserAndTime = new WellStatusHistoryDTO();
            historyWithSetUserAndTime.WellId = addedWell.Id;
            historyWithSetUserAndTime.WellStatusId = 1;
            historyWithSetUserAndTime.ChangeUserId = 1;
            historyWithSetUserAndTime.ChangeDate = compareDate;

            WellService.InsertWellStatusHistory(historyWithSetUserAndTime);
            history = WellService.GetWellStatusHistory(addedWell.Id.ToString());

            Assert.AreEqual(3, history.Count(), "Failed to insert Well Status History");
            Assert.AreEqual(historyWithSetUserAndTime.ChangeUserId, history[2].ChangeUserId, "Failed to insert set user");
            Assert.AreEqual(compareDate, history[2].ChangeDate, "Failed to insert set Date");
        }

        [TestCategory(TestCategories.WellServiceTests), TestMethod]
        public void ExtendedDTOTest()
        {
            //Creating well
            WellDTO addedWell = AddNonRRLWell("ESPWELL_0001", WellTypeId.ESP, false);
            Assert.IsNotNull(addedWell);
            _wellsToRemove.Add(addedWell);

            //Calling GetExtendedDTO using the Id for the newly created well should return a blank WellExtendedDTO
            WellExtendedDTO blankWellExtendedDTO = WellService.GetWellExtended(addedWell.Id.ToString());

            //Verifying that blankWellExtendedDTO is blank
            foreach (var prop in blankWellExtendedDTO.GetType().GetProperties())
            {
                Trace.WriteLine("Field " + prop.Name + " has value " + prop.GetValue(blankWellExtendedDTO));
                if (prop.Name.StartsWith("User"))
                {
                    Assert.IsNull(prop.GetValue(blankWellExtendedDTO, null), prop.Name + " field is not blank");
                }
            }
            //Updating few fields of blankWellExtendedDTO with Random values
            blankWellExtendedDTO.UserBool001 = true;
            blankWellExtendedDTO.UserDate001 = DateTime.Today.ToUniversalTime();
            blankWellExtendedDTO.UserFloat001 = 1.5m;
            blankWellExtendedDTO.UserInt001 = 15;
            blankWellExtendedDTO.UserString001 = "RandomString";

            //Calling UpdateWellExtended to write the WellExtendedDTO back to the database
            WellService.UpdateWellExtended(blankWellExtendedDTO);

            //Calling GetExtendedDTO again using the Id for the well again
            WellExtendedDTO filledWellExtendedDTO = WellService.GetWellExtended(addedWell.Id.ToString());

            //Verifying all of the fields wrote to the database are returned in the DTO
            Assert.AreEqual(blankWellExtendedDTO.UserBool001, filledWellExtendedDTO.UserBool001);
            Assert.AreEqual(blankWellExtendedDTO.UserDate001, filledWellExtendedDTO.UserDate001);
            Assert.AreEqual(blankWellExtendedDTO.UserFloat001, filledWellExtendedDTO.UserFloat001);
            Assert.AreEqual(blankWellExtendedDTO.UserInt001, filledWellExtendedDTO.UserInt001);
            Assert.AreEqual(blankWellExtendedDTO.UserString001, filledWellExtendedDTO.UserString001);
        }

        [TestCategory(TestCategories.WellServiceTests), TestMethod]
        public void GetallWellcount_Grpstatus()
        {
            string facilityId = string.Empty;
            int addWellCount = 2;
            try
            {
                for (int i = 1; i <= addWellCount; i++)
                {
                    // Add 2 NF wells.
                    facilityId = GetFacilityId("NFWELL_", i);
                    Trace.WriteLine("Adding Well " + facilityId);
                    AddNonRRLWell(facilityId, WellTypeId.NF, false);
                    // Add 2 RRL wells.
                    facilityId = GetFacilityId("RPOC_", i);
                    Trace.WriteLine("Adding Well " + facilityId);
                    AddNonRRLWell(facilityId, WellTypeId.RRL, false);
                    // Add 2 ESP wells.
                    facilityId = GetFacilityId("ESPWELL_", i);
                    Trace.WriteLine("Adding Well " + facilityId);
                    AddNonRRLWell(facilityId, WellTypeId.ESP, false);
                    // Add 2 gas injection wells.
                    facilityId = GetFacilityId("GASINJWELL_", i);
                    Trace.WriteLine("Adding Well " + facilityId);
                    AddNonRRLWell(facilityId, WellTypeId.GInj, false);
                    // Add 2 gas lift wells.
                    facilityId = GetFacilityId("GLWELL_", i);
                    Trace.WriteLine("Adding Well " + facilityId);
                    AddNonRRLWell(facilityId, WellTypeId.GLift, false);
                    // Add 2 water injection wells.
                    facilityId = GetFacilityId("WATERINJWELL_", i);
                    Trace.WriteLine("Adding Well " + facilityId);
                    AddNonRRLWell(facilityId, WellTypeId.WInj, false);
                    // Add 2 plunger lift wells.
                    facilityId = GetFacilityId("PGLWELL_", i);
                    Trace.WriteLine("Adding Well " + facilityId);
                    AddNonRRLWell(facilityId, WellTypeId.PLift, false);
                    // Add 2 OT wells.
                    facilityId = GetFacilityId("RPOC_", i + addWellCount);
                    Trace.WriteLine("Adding OT Well " + facilityId);
                    AddNonRRLWell(facilityId, WellTypeId.OT, false);
                }
                // Create a new well filter.
                WellFilterDTO wellFilter = new WellFilterDTO();
                // Include all well types.
                wellFilter.welFK_r_WellTypeValues = new[] {
                             new WellFilterValueDTO { Value = WellTypeId.NF.ToString() },
                             new WellFilterValueDTO { Value = WellTypeId.RRL.ToString() },
                             new WellFilterValueDTO { Value = WellTypeId.ESP.ToString() },
                             new WellFilterValueDTO { Value = WellTypeId.GInj.ToString() },
                             new WellFilterValueDTO { Value = WellTypeId.GLift.ToString() },
                             new WellFilterValueDTO { Value = WellTypeId.WInj.ToString() },
                             new WellFilterValueDTO { Value = WellTypeId.PLift.ToString() },
                             new WellFilterValueDTO { Value = WellTypeId.OT.ToString() }
                };
                Dictionary<WellTypeId, int> wellCounts = WellService.GetWellCountByType(wellFilter);
                int wellcount_NF = wellCounts[WellTypeId.NF];
                int wellcount_RRL = wellCounts[WellTypeId.RRL];
                int wellcount_ESP = wellCounts[WellTypeId.ESP];
                int wellcount_GInj = wellCounts[WellTypeId.GInj];
                int wellcount_GLift = wellCounts[WellTypeId.GLift];
                int wellcount_WInj = wellCounts[WellTypeId.WInj];
                int wellcount_PLift = wellCounts[WellTypeId.PLift];
                int wellcount_OT = wellCounts[WellTypeId.OT];
                Assert.AreEqual(addWellCount, wellcount_NF, "NF Well count Mismatch");
                Assert.AreEqual(addWellCount, wellcount_RRL, "RRL Well count Mismatch");
                Assert.AreEqual(addWellCount, wellcount_ESP, "ESP Well count Mismatch");
                Assert.AreEqual(addWellCount, wellcount_GInj, "GInj Well count Mismatch");
                Assert.AreEqual(addWellCount, wellcount_GLift, "GLift Well count Mismatch");
                Assert.AreEqual(addWellCount, wellcount_WInj, "WInj Well count Mismatch");
                Assert.AreEqual(addWellCount, wellcount_PLift, "PLift Well count Mismatch");
                Assert.AreEqual(addWellCount, wellcount_OT, "OT Well count Mismatch");
            }
            finally
            {
                // Clean up the wells created.
                for (int i = 1; i < addWellCount + 1; i++)
                {
                    // Remove 2 ESP wells.
                    facilityId = GetFacilityId("ESPWELL_", i);
                    RemoveWell(facilityId);
                    // Remove 2 NF wells.
                    facilityId = GetFacilityId("NFWELL_", i);
                    RemoveWell(facilityId);
                    // Remove 2 gas injection wells.
                    facilityId = GetFacilityId("GASINJWELL_", i);
                    RemoveWell(facilityId);
                    // Remove 2 gas lift wells.
                    facilityId = GetFacilityId("GLWELL_", i);
                    RemoveWell(facilityId);
                    // Remove 2 water injection wells.
                    facilityId = GetFacilityId("WATERINJWELL_", i);
                    RemoveWell(facilityId);
                    // Remove 2 plunger lift wells.
                    facilityId = GetFacilityId("PGLWELL_", i);
                    RemoveWell(facilityId);
                    // Remove 2 OT wells.
                    facilityId = GetFacilityId("RPOC_", i);
                    RemoveWell(facilityId);
                    // Remove 2 RRL wells.
                    facilityId = GetFacilityId("RPOC_", i + addWellCount);
                    RemoveWell(facilityId);
                }
            }
        }


        [TestCategory(TestCategories.WellServiceTests), TestMethod]
        public void GetWellLatestCommentUpdateWellandWellTargetRate()
        {
            // Add Well 
            string facilityId = GetFacilityId("NFWWELL_", 1);
            Trace.WriteLine("Adding Well " + facilityId);
            WellDTO nonrrlwell = AddNonRRLWell(facilityId, WellTypeId.NF, false);
            Trace.WriteLine("Added Well " + facilityId);

            //Add Well Commment # 1 that is visible in Well Status
            WellService.SaveComments(new WellCommentDTO { WellComment = "Well Comment 1", WellId = nonrrlwell.Id });
            //Add Well Commment # 2 that is visible in Well Status
            WellService.SaveComments(new WellCommentDTO { WellComment = "Well Comment 2", WellId = nonrrlwell.Id });
            //Check that on Group status we see latest well comment
            WellCommentDTO wellcmt = WellService.GetLatestWellComment(nonrrlwell.Id.ToString());
            Assert.AreEqual("Well Comment 2", wellcmt.WellComment, "Well Comment was not latest");
            Assert.AreEqual(AuthenticatedUser.Name, wellcmt.CreatedUser, "Comment creatgion users mismatch");
            nonrrlwell.Engineer = "Updated Engineer Name";
            nonrrlwell.SurfaceLatitude = 50;
            nonrrlwell.SurfaceLongitude = 19;
            WellService.UpdateWell(nonrrlwell);
            nonrrlwell = WellService.GetWell(nonrrlwell.Id.ToString());
            Assert.AreEqual("Updated Engineer Name", nonrrlwell.Engineer, "Engineer Name was not updated");
            Assert.AreEqual(50, nonrrlwell.SurfaceLatitude, "Latitude was not updated");
            Assert.AreEqual(19, nonrrlwell.SurfaceLongitude, "Latitude was not updated");
            //Add and Uodate Target Rates
            WellTargetRateDTO welltargetdto = new WellTargetRateDTO
            {
                WellId = nonrrlwell.Id,
                OilLowerBound = 10,
                WaterLowerBound = 10,
                GasLowerBound = 10,
                OilTarget = 50,
                WaterTarget = 50,
                GasTarget = 50,
                OilUpperBound = 200,
                GasUpperBound = 200,
                WaterUpperBound = 200,
                OilMinimum = 0,
                OilTechnicalLimit = 10000,
                WaterMinimum = 0,
                WaterTechnicalLimit = 10000,
                GasMinimum = 0,
                GasTechnicalLimit = 10000,
                StartDate = DateTime.Now.AddDays(-60).ToUniversalTime(),
                EndDate = DateTime.Now.AddDays(-30).ToUniversalTime(),
            };
            string result = WellService.AddWellTargetRate(welltargetdto);
            Assert.AreEqual("Success", result, "Add Target Rate Failed");
            WellTargetRateArrayAndUnitsDTO welltargetarrayfto = WellService.GetWellTargetRateAndUnits(nonrrlwell.Id.ToString());
            WellTargetRateDTO welltargetupdateddto = welltargetarrayfto.Values.FirstOrDefault(x => x.OilTarget == 50);
            long addedtarget = welltargetupdateddto.Id;
            welltargetupdateddto.OilTarget = 60;
            welltargetupdateddto.WaterTarget = 70;
            welltargetupdateddto.GasTarget = 80;

            WellService.UpdateWellTargetRate(welltargetupdateddto);
            welltargetarrayfto = WellService.GetWellTargetRateAndUnits(nonrrlwell.Id.ToString());
            welltargetupdateddto = welltargetarrayfto.Values.FirstOrDefault(x => x.Id == addedtarget);

            Assert.AreEqual(60, welltargetupdateddto.OilTarget);
            Assert.AreEqual(70, welltargetupdateddto.WaterTarget);
            Assert.AreEqual(80, welltargetupdateddto.GasTarget);

        }
    }
}