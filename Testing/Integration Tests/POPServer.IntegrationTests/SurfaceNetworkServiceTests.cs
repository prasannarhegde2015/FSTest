using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Weatherford.POP.DTOs;
using Weatherford.POP.Enums;
using Weatherford.POP.Units;
using Weatherford.ReoService.Enums;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Reflection;

namespace Weatherford.POP.Server.IntegrationTests
{
    [TestClass]
    public class SurfaceNetworkServiceTests : APIClientTestBase
    {
        private List<string> _modelFilesToRemove;

        [TestInitialize]
        public override void Init()
        {
            base.Init();
            _modelFilesToRemove = new List<string>();

            var defaultAsset = new AssetDTO() { Name = "Default", Description = "Default" };
            AssetDTO found = SurfaceNetworkService.GetAllAssets()?.FirstOrDefault(t => t.Name == defaultAsset.Name);
            if (found == null)
            {
                SurfaceNetworkService.AddAsset(defaultAsset);
                found = SurfaceNetworkService.GetAllAssets()?.FirstOrDefault(t => t.Name == defaultAsset.Name);
                Assert.IsNotNull(found, "Failed to add default asset.");
            }
            _serviceFactory.DefaultTimeout = 900000; //increase default timeout to 15 mins
        }

        [TestCleanup]
        public override void Cleanup()
        {
            base.Cleanup();
        }

        [TestCategory(TestCategories.SurfaceNetworkServiceTests), TestMethod]
        public void GetAddAsset()
        {
            string assetName = "Asset Test";
            SurfaceNetworkService.AddAsset(new AssetDTO() { Name = assetName, Description = "Test Description" });
            var allAssets = SurfaceNetworkService.GetAllAssets().ToList();
            AssetDTO asset = allAssets?.FirstOrDefault(a => a.Name.Equals(assetName));
            Assert.IsNotNull(asset);
            _assetsToRemove.Add(asset);

            AssetDTO assetCompare = SurfaceNetworkService.GetAsset(asset.Id.ToString());
            Assert.IsNotNull(assetCompare, "Failed to get added Asset.");
            Assert.AreEqual(asset.Name, assetCompare.Name, "Asset Name has unexpected value.");
        }

        [TestCategory(TestCategories.SurfaceNetworkServiceTests), TestMethod]
        public void AddUpdateDeleteAsset()
        {
            string assetName = "Asset Test";
            string assetDescriptionUpdate = "Test Description Update";
            SurfaceNetworkService.AddAsset(new AssetDTO() { Name = assetName, Description = "Test Description" });
            var allAssets = SurfaceNetworkService.GetAllAssets().ToList();
            AssetDTO asset = allAssets?.FirstOrDefault(a => a.Name.Equals(assetName));
            Assert.IsNotNull(asset);
            _assetsToRemove.Add(asset);

            asset.Description = assetDescriptionUpdate;
            SurfaceNetworkService.UpdateAsset(asset);

            AssetDTO assetCompare = SurfaceNetworkService.GetAsset(asset.Id.ToString());
            Assert.IsNotNull(assetCompare, "Failed to get added Asset.");
            Assert.AreEqual(assetDescriptionUpdate, assetCompare.Description, "Asset Description has unexpected value.");

            SurfaceNetworkService.RemoveAsset(asset.Id.ToString());
            AssetDTO assetRemoved = SurfaceNetworkService.GetAsset(asset.Id.ToString());
            Assert.IsNull(assetRemoved);
        }

        [TestCategory(TestCategories.SurfaceNetworkServiceTests), TestMethod]
        public void GetAddSNModel()
        {
            string assetName = "Asset Test";
            string modelName = "SNModel Test";
            SurfaceNetworkService.AddAsset(new AssetDTO() { Name = assetName, Description = "Test Description" });
            var allAssets = SurfaceNetworkService.GetAllAssets().ToList();
            AssetDTO asset = allAssets?.FirstOrDefault(a => a.Name.Equals(assetName));
            Assert.IsNotNull(asset);
            _assetsToRemove.Add(asset);

            SurfaceNetworkService.AddSNModel(new SNModelDTO() { Name = modelName, AssetId = asset.Id });
            long[] userAssets = new long[1]; userAssets[0] = asset.Id;
            var allModels = SurfaceNetworkService.GetSNModelsByAssetIds(userAssets);
            SNModelDTO model = allModels?.FirstOrDefault(m => m.Name.Equals(modelName));

            AssetDTO assetCompare = SurfaceNetworkService.GetAsset(asset.Id.ToString());
            Assert.IsNotNull(assetCompare, "Failed to get added Asset.");
            Assert.AreEqual(asset.Name, assetCompare.Name, "Asset Name has unexpected value.");
        }

        [TestCategory(TestCategories.SurfaceNetworkServiceTests), TestMethod]
        public void AddUpdateDeleteSNModel()
        {
            string assetName = "Asset Test";
            string modelName = "SNModel Test";
            string modelNameUpdate = "SNModel Test Update";
            SurfaceNetworkService.AddAsset(new AssetDTO() { Name = assetName, Description = "Test Description" });
            var allAssets = SurfaceNetworkService.GetAllAssets().ToList();
            AssetDTO asset = allAssets?.FirstOrDefault(a => a.Name.Equals(assetName));
            Assert.IsNotNull(asset);
            _assetsToRemove.Add(asset);

            SurfaceNetworkService.AddSNModel(new SNModelDTO() { Name = modelName, AssetId = asset.Id });
            long[] userAssets = new long[1]; userAssets[0] = asset.Id;
            var allModels = SurfaceNetworkService.GetSNModelsByAssetIds(userAssets);
            SNModelDTO model = allModels?.FirstOrDefault(m => m.Name.Equals(modelName));

            model.Name = modelNameUpdate;
            SurfaceNetworkService.UpdateSNModel(model);

            SNModelDTO modelCompare = SurfaceNetworkService.GetSNModel(model.Id.ToString());
            Assert.IsNotNull(modelCompare, "Failed to get added SNModel.");
            Assert.AreEqual(modelNameUpdate, modelCompare.Name, "Model Name has unexpected value.");

            SurfaceNetworkService.RemoveSNModel(model.Id.ToString());
            SNModelDTO modelRemoved = SurfaceNetworkService.GetSNModel(model.Id.ToString());
            Assert.IsNull(modelRemoved);
        }

        [TestCategory(TestCategories.SurfaceNetworkServiceTests), TestMethod]
        public void RemoveSNModels()
        {
            // Add first asset and model
            string assetName = "Asset Test";
            string modelName = "SNModel Test";
            SurfaceNetworkService.AddAsset(new AssetDTO() { Name = assetName, Description = "Test Description" });
            var allAssets = SurfaceNetworkService.GetAllAssets().ToList();
            AssetDTO asset = allAssets?.FirstOrDefault(a => a.Name.Equals(assetName));
            Assert.IsNotNull(asset);
            _assetsToRemove.Add(asset);

            SurfaceNetworkService.AddSNModel(new SNModelDTO() { Name = modelName, AssetId = asset.Id });
            long[] userAssets = new long[1];
            userAssets[0] = asset.Id;
            var allModels = SurfaceNetworkService.GetSNModelsByAssetIds(userAssets);
            SNModelDTO model = allModels?.FirstOrDefault(m => m.Name.Equals(modelName));

            // Add second asset and model
            string asset2Name = "Asset 2 Test";
            string model2Name = "SNModel 2 Test";
            string model2BName = "SNModel 2 Test 2";
            SurfaceNetworkService.AddAsset(new AssetDTO() { Name = asset2Name, Description = "Test Description 2" });
            var all2Assets = SurfaceNetworkService.GetAllAssets().ToList();
            AssetDTO asset2 = all2Assets?.FirstOrDefault(a => a.Name.Equals(asset2Name));
            Assert.IsNotNull(asset2);
            _assetsToRemove.Add(asset2);

            SurfaceNetworkService.AddSNModel(new SNModelDTO() { Name = model2Name, AssetId = asset2.Id });
            SurfaceNetworkService.AddSNModel(new SNModelDTO() { Name = model2BName, AssetId = asset2.Id });
            userAssets = new long[1];
            userAssets[0] = asset2.Id;
            var all2Models = SurfaceNetworkService.GetSNModelsByAssetIds(userAssets);
            SNModelDTO model2 = all2Models?.FirstOrDefault(m => m.Name.Equals(model2Name));
            SNModelDTO model2B = all2Models?.FirstOrDefault(m => m.Name.Equals(model2BName));

            // Get and verify first asset and model (and only those)
            SNModelDTO modelCompare = SurfaceNetworkService.GetSNModel(model.Id.ToString());
            Assert.IsNotNull(modelCompare, "Failed to get added SNModel.");

            Assert.AreEqual(modelName, modelCompare.Name, "Model Name has unexpected value.");

            // Get and verify second asset and models (and only those)
            SNModelDTO model2Compare = SurfaceNetworkService.GetSNModel(model2.Id.ToString());
            Assert.IsNotNull(model2Compare, "Failed to get added SNModel 2.");
            SNModelDTO model2BCompare = SurfaceNetworkService.GetSNModel(model2B.Id.ToString());
            Assert.IsNotNull(model2BCompare, "Failed to get added SNModel 2B.");

            Assert.AreEqual(model2Name, model2Compare.Name, "Model 2 Name has unexpected value.");
            Assert.AreEqual(model2BName, model2BCompare.Name, "Model 2B Name has unexpected value.");

            // Clean up created models
            SurfaceNetworkService.RemoveSNModel(model.Id.ToString());
            SNModelDTO modelRemoved = SurfaceNetworkService.GetSNModel(model.Id.ToString());
            Assert.IsNull(modelRemoved);

            // Clean up created model 2s
            string[] modelids = new string[2];
            modelids[0] = model2.Id.ToString();
            modelids[1] = model2B.Id.ToString();

            SurfaceNetworkService.RemoveSNModels(modelids);
            SNModelDTO model2Removed = SurfaceNetworkService.GetSNModel(model2.Id.ToString());
            Assert.IsNull(model2Removed);

            SurfaceNetworkService.RemoveSNModel(model2B.Id.ToString());
            SNModelDTO model2BRemoved = SurfaceNetworkService.GetSNModel(model2B.Id.ToString());
            Assert.IsNull(model2BRemoved);
        }

        [TestCategory(TestCategories.SurfaceNetworkServiceTests), TestMethod]
        public void AddUpdateDeleteSNModelFile()
        {
            string path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";
            string model = "22Wells.reo";
            string assetName = "Asset Test";
            string modelName = "SNModel Test";
            SurfaceNetworkService.AddAsset(new AssetDTO() { Name = assetName, Description = "Test Description" });
            var allAssets = SurfaceNetworkService.GetAllAssets().ToList();
            AssetDTO asset = allAssets?.FirstOrDefault(a => a.Name.Equals(assetName));
            Assert.IsNotNull(asset);
            _assetsToRemove.Add(asset);

            SNModelAndModelFileDTO modelAndFile = new SNModelAndModelFileDTO();
            modelAndFile.SNModel = new SNModelDTO() { Name = modelName, AssetId = asset.Id };

            SNModelFileDTO modelFile = new SNModelFileDTO();
            modelFile.Comments = "Test Model File;";
            modelFile.UpdateFluidModel = true;

            byte[] fileAsByteArray = GetByteArray(path, model);
            Assert.IsNotNull(fileAsByteArray);
            modelFile.Base64Contents = Convert.ToBase64String(fileAsByteArray);
            modelAndFile.SNModelFile = modelFile;

            //Create Initial Model And ModelFile
            SurfaceNetworkService.AddSNModelAndFile(modelAndFile);
            long[] userAssets = new long[1]; userAssets[0] = asset.Id;
            var allModels = SurfaceNetworkService.GetSNModelsByAssetIds(userAssets);
            SNModelDTO newModel = allModels?.FirstOrDefault(m => m.Name.Equals(modelName));

            //Check that there is 1 ModelFile
            SNModelFileDTO[] modelFileCount1 = SurfaceNetworkService.GetSNModelFilesByModelId(newModel.Id.ToString());
            Assert.AreEqual(1, modelFileCount1.Count());

            // Use the ModelFileHeaders variant and verify still only get 1 count
            SNModelFileHeaderDTO[] modelFileHeadersCount = SurfaceNetworkService.GetSNModelFileHeadersByModelId(newModel.Id.ToString());
            Assert.AreEqual(1, modelFileHeadersCount.Count());

            // Use the above to test GetSNModelFile since we now have the ID
            SNModelFileDTO modelFileT2 = SurfaceNetworkService.GetSNModelFile(modelFileHeadersCount[0].Id.ToString());
            Assert.IsNotNull(modelFileT2);
            Assert.AreEqual(modelFileT2.Id, modelFileHeadersCount[0].Id);
            Assert.AreEqual(modelFileT2.SNModelId, modelFileHeadersCount[0].SNModelId);

            // Use the above to test GetSNModelFileHeader since we now have the ID
            SNModelFileHeaderDTO modelFileHeaderT2 = SurfaceNetworkService.GetSNModelFileHeader(modelFileHeadersCount[0].Id.ToString());
            Assert.IsNotNull(modelFileHeaderT2);
            Assert.AreEqual(modelFileHeaderT2.Id, modelFileHeadersCount[0].Id);
            Assert.AreEqual(modelFileHeaderT2.SNModelId, modelFileHeadersCount[0].SNModelId);

            //Add second ModleFile to Same Model
            SNModelAndModelFileDTO newModelAndFile = new SNModelAndModelFileDTO();
            newModelAndFile.SNModel = newModel;
            newModelAndFile.SNModelFile = modelFile;
            SurfaceNetworkService.AddSNModelAndFile(modelAndFile);

            //Check that there are 2 ModelFiles
            SNModelFileDTO[] modelFileCount2 = SurfaceNetworkService.GetSNModelFilesByModelId(newModel.Id.ToString());
            Assert.AreEqual(2, modelFileCount2.Count());

            //Remove 1 ModelFile
            SurfaceNetworkService.RemoveSNModelFile(modelFileCount2[0].Id.ToString());

            //Check that there is 1 ModelFile
            SNModelFileDTO[] modelFileCount3 = SurfaceNetworkService.GetSNModelFilesByModelId(newModel.Id.ToString());
            Assert.AreEqual(1, modelFileCount3.Count());

            //Remove 1 ModelFile
            SurfaceNetworkService.RemoveSNModelFile(modelFileCount2[1].Id.ToString());

            //Check that there are 0 ModelFiles
            SNModelFileDTO[] modelFileCount4 = SurfaceNetworkService.GetSNModelFilesByModelId(newModel.Id.ToString());
            Assert.AreEqual(0, modelFileCount4.Count());
        }

        [TestCategory(TestCategories.SurfaceNetworkServiceTests), TestMethod]
        public void AddUpdateDeleteScenario()
        {
            string assetName = "Asset Test";
            string modelName = "Model Test";
            string scenarioName = "Scenario Test";
            string scenarioNameUpdate = "Scenario Test Update";
            SurfaceNetworkService.AddAsset(new AssetDTO() { Name = assetName, Description = "Test Description" });
            var allAssets = SurfaceNetworkService.GetAllAssets().ToList();
            AssetDTO asset = allAssets?.FirstOrDefault(a => a.Name.Equals(assetName));
            Assert.IsNotNull(asset);
            _assetsToRemove.Add(asset);

            SurfaceNetworkService.AddSNModel(new SNModelDTO() { Name = modelName, AssetId = asset.Id });
            long[] userAssets = new long[1]; userAssets[0] = asset.Id;
            var allModels = SurfaceNetworkService.GetSNModelsByAssetIds(userAssets);
            SNModelDTO model = allModels?.FirstOrDefault(m => m.Name.Equals(modelName));
            Assert.IsNotNull(model);

            SurfaceNetworkService.AddSNScenario(new SNScenarioDTO() { Name = scenarioName, Description = "Test Description", SNModelId = model.Id, ColorCode = "#FF0000", AllowableScenario = true, Implementable = false, ModelMatch = false, OptimumScenario = false, IsForecastScenario = true });
            var allScenarios = SurfaceNetworkService.GetSNScenariosBySNModel(model.Id.ToString());
            SNScenarioDTO scenario = allScenarios?.FirstOrDefault(sn => sn.Name.Equals(scenarioName));
            Assert.IsNotNull(scenario);

            scenario.Name = scenarioNameUpdate;
            scenario.IsForecastScenario = false;
            SurfaceNetworkService.UpdateSNScenario(scenario);
            SNScenarioDTO scenarioCompare = SurfaceNetworkService.GetSNScenario(scenario.Id.ToString());
            Assert.IsNotNull(scenarioCompare, "Failed to get added SNScenario.");
            Assert.AreEqual(scenarioNameUpdate, scenarioCompare.Name, "Scenario Name has unexpected value.");
            Assert.AreEqual(false, scenarioCompare.IsForecastScenario, "Scenario IsForecastScenario value not updated.");
            int countBeforeRemoveSNScenario = -1;
            int countAfterRemoveSNScenario = -1;
            //Cant Access Team City DB as it users non privilaged user account VSI\AtsAdmin  for  test run.
            if (s_isRunningInATS)
            {
                countBeforeRemoveSNScenario = GetSNDeleteAuditTableRecordCount();
            }

            SurfaceNetworkService.RemoveSNScenario(scenario.Id.ToString());
            if (s_isRunningInATS)
            {
                countAfterRemoveSNScenario = GetSNDeleteAuditTableRecordCount();
                Assert.IsFalse(countAfterRemoveSNScenario < countBeforeRemoveSNScenario, "DeleteAudit table not updated.");
            }


            SNScenarioDTO scenarioRemoved = SurfaceNetworkService.GetSNScenario(scenario.Id.ToString());
            Assert.IsNull(scenarioRemoved);
        }

        [TestCategory(TestCategories.SurfaceNetworkServiceTests), TestMethod]

        public void AddUpdateDeleteScenarioWithModelFile()
        {
            string path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";
            string ReOmodel = "22Wells.reo";
            string assetName = "Asset Test George";
            string modelName = "Model Test";
            string scenarioName = "Scenario Test";
            string scenarioName2 = "Scenario Test 2";
            string scenarioNameUpdate = "Scenario Test Update";
            SurfaceNetworkService.AddAsset(new AssetDTO() { Name = assetName, Description = "Test Description" });
            var allAssets = SurfaceNetworkService.GetAllAssets().ToList();
            AssetDTO asset = allAssets?.FirstOrDefault(a => a.Name.Equals(assetName));
            Assert.IsNotNull(asset);
            _assetsToRemove.Add(asset);

            // add in the model file to use for scenario test calls
            SNModelDTO model = AddSurfaceNetworkModelAndModelFile(asset, path, ReOmodel, modelName);
            Assert.IsNotNull(model);

            SurfaceNetworkService.AddSNScenario(new SNScenarioDTO() { Name = scenarioName, Description = "Test Description", SNModelId = model.Id, ColorCode = "#FF0000", AllowableScenario = true, Implementable = false, ModelMatch = false, OptimumScenario = false });
            var allScenarios = SurfaceNetworkService.GetSNScenariosBySNModel(model.Id.ToString());
            SNScenarioDTO scenario = allScenarios?.FirstOrDefault(sn => sn.Name.Equals(scenarioName));
            Assert.IsNotNull(scenario);

            scenario.Name = scenarioNameUpdate;
            SurfaceNetworkService.UpdateSNScenario(scenario);
            SNScenarioDTO scenarioCompare = SurfaceNetworkService.GetSNScenario(scenario.Id.ToString());
            Assert.IsNotNull(scenarioCompare, "Failed to get added SNScenario.");
            Assert.AreEqual(scenarioNameUpdate, scenarioCompare.Name, "Scenario Name has unexpected value.");

            // Check scenario fetches before optimization pass
            SNScenarioWellDataDTO[] scenWellData = SurfaceNetworkService.GetScenarioSectionWellData(scenario.Id.ToString());
            Assert.IsNotNull(scenWellData);

            // Resave one of the records to test the save function
            SurfaceNetworkService.SaveScenarioSectionWellData(scenWellData[4]);

            // Resave the entire collection to test the multi
            SurfaceNetworkService.SaveScenarioSectionWellDataMulti(scenWellData);

            SNScenarioWellDataDTO[] scenWellDataB = SurfaceNetworkService.GetScenarioSectionWellData(scenario.Id.ToString());
            Assert.IsNotNull(scenWellDataB);

            // Verify that the two collection effectively match since no changes were made
            System.Collections.IComparer scenComparer = new WellDataDTOComparerClass();
            CollectionAssert.AreEqual(scenWellDataB.ToArray(), scenWellData.ToArray(), scenComparer);

            SNScenarioChokeDataArrayAndUnitsDTO scenChokeData = SurfaceNetworkService.GetScenarioSectionChokeData(scenario.Id.ToString());
            Assert.IsNotNull(scenChokeData);

            SNScenarioChokeDataAndUnitsDTO scenChokeChngSing = new SNScenarioChokeDataAndUnitsDTO(scenChokeData.Units, scenChokeData.Values[4]);
            SurfaceNetworkService.SaveScenarioSectionChokeData(scenChokeChngSing);

            // reget to verify nothing changed
            SNScenarioChokeDataArrayAndUnitsDTO scenChokeDataB = SurfaceNetworkService.GetScenarioSectionChokeData(scenario.Id.ToString());
            Assert.IsNotNull(scenChokeDataB);

            System.Collections.IComparer chokeComparer = new ChokeDataDTOComparerClass();
            CollectionAssert.AreEqual(scenChokeDataB.Values, scenChokeData.Values, chokeComparer);

            SurfaceNetworkService.SaveScenarioSectionChokeDataMulti(scenChokeDataB);

            // reget to verify nothing changed
            SNScenarioChokeDataArrayAndUnitsDTO scenChokeDataC = SurfaceNetworkService.GetScenarioSectionChokeData(scenario.Id.ToString());
            Assert.IsNotNull(scenChokeDataC);
            CollectionAssert.AreEqual(scenChokeDataC.Values, scenChokeData.Values, chokeComparer);

            SNScenarioBlockValveDataDTO[] scenBlockData = SurfaceNetworkService.GetScenarioSectionBlockValveData(scenario.Id.ToString());
            Assert.IsNotNull(scenBlockData);

            SurfaceNetworkService.SaveScenarioSectionBlockValveData(scenBlockData[2]);

            // reget to verify nothing changed
            SNScenarioBlockValveDataDTO[] scenBlockDataB = SurfaceNetworkService.GetScenarioSectionBlockValveData(scenario.Id.ToString());
            Assert.IsNotNull(scenBlockDataB);

            System.Collections.IComparer blockComparer = new BlockValveDTOComparerClass();
            CollectionAssert.AreEqual(scenBlockDataB.ToArray(), scenBlockData.ToArray(), blockComparer);

            SurfaceNetworkService.SaveScenarioSectionBlockValveDataMulti(scenBlockDataB);

            SNScenarioBlockValveDataDTO[] scenBlockDataC = SurfaceNetworkService.GetScenarioSectionBlockValveData(scenario.Id.ToString());
            Assert.IsNotNull(scenBlockDataC);
            CollectionAssert.AreEqual(scenBlockDataC.ToArray(), scenBlockData.ToArray(), blockComparer);

            SNScenarioPressureConstraintDataArrayAndUnitsDTO scenPressConstData = SurfaceNetworkService.GetScenarioSectionPressureConstraintData(scenario.Id.ToString());
            Assert.IsNotNull(scenPressConstData);

            SNScenarioPressureConstraintDataAndUnitsDTO savePressConstSing = new SNScenarioPressureConstraintDataAndUnitsDTO(scenPressConstData.Units, scenPressConstData.Values[1]);
            SurfaceNetworkService.SaveScenarioSectionPressureConstraintData(savePressConstSing);

            SNScenarioPressureConstraintDataArrayAndUnitsDTO scenPressConstDataB = SurfaceNetworkService.GetScenarioSectionPressureConstraintData(scenario.Id.ToString());
            Assert.IsNotNull(scenPressConstDataB);

            System.Collections.IComparer pressConstComparer = new PressConstDTOComparerClass();
            CollectionAssert.AreEqual(scenPressConstDataB.Values, scenPressConstData.Values, pressConstComparer);

            SurfaceNetworkService.SaveScenarioSectionPressureConstraintDataMulti(scenPressConstDataB);

            SNScenarioPressureConstraintDataArrayAndUnitsDTO scenPressConstDataC = SurfaceNetworkService.GetScenarioSectionPressureConstraintData(scenario.Id.ToString());
            Assert.IsNotNull(scenPressConstDataC);
            CollectionAssert.AreEqual(scenPressConstDataC.Values, scenPressConstData.Values, pressConstComparer);

            SNScenarioGasConstraintDataArrayAndUnitsDTO scenGasConstData = SurfaceNetworkService.GetScenarioSectionGasConstraintData(scenario.Id.ToString());
            Assert.IsNotNull(scenGasConstData);

            SNScenarioGasConstraintDataAndUnitsDTO saveGasConstSing = new SNScenarioGasConstraintDataAndUnitsDTO(scenGasConstData.Units, scenGasConstData.Values[1]);
            SurfaceNetworkService.SaveScenarioSectionGasConstraintData(saveGasConstSing);

            SNScenarioGasConstraintDataArrayAndUnitsDTO scenGasConstDataB = SurfaceNetworkService.GetScenarioSectionGasConstraintData(scenario.Id.ToString());
            Assert.IsNotNull(scenGasConstDataB);

            System.Collections.IComparer gasConstComparer = new GasConstDTOComparerClass();
            CollectionAssert.AreEqual(scenGasConstDataB.Values, scenGasConstData.Values, gasConstComparer);

            SurfaceNetworkService.SaveScenarioSectionGasConstraintDataMulti(scenGasConstDataB);

            SNScenarioGasConstraintDataArrayAndUnitsDTO scenGasConstDataC = SurfaceNetworkService.GetScenarioSectionGasConstraintData(scenario.Id.ToString());
            Assert.IsNotNull(scenGasConstDataC);
            CollectionAssert.AreEqual(scenGasConstDataC.Values, scenGasConstData.Values, gasConstComparer);

            SNScenarioLiquidConstraintDataArrayAndUnitsDTO scenLiqConstData = SurfaceNetworkService.GetScenarioSectionLiquidConstraintData(scenario.Id.ToString());
            Assert.IsNotNull(scenLiqConstData);

            SNScenarioLiquidConstraintDataAndUnitsDTO saveLiqConstSing = new SNScenarioLiquidConstraintDataAndUnitsDTO(scenLiqConstData.Units, scenLiqConstData.Values[2]);
            SurfaceNetworkService.SaveScenarioSectionLiquidConstraintData(saveLiqConstSing);

            SNScenarioLiquidConstraintDataArrayAndUnitsDTO scenLiqConstDataB = SurfaceNetworkService.GetScenarioSectionLiquidConstraintData(scenario.Id.ToString());
            Assert.IsNotNull(scenLiqConstDataB);

            System.Collections.IComparer liqConstComparer = new LiqConstDTOComparerClass();
            CollectionAssert.AreEqual(scenLiqConstDataB.Values, scenLiqConstData.Values, liqConstComparer);

            SurfaceNetworkService.SaveScenarioSectionLiquidConstraintDataMulti(scenLiqConstDataB);

            SNScenarioLiquidConstraintDataArrayAndUnitsDTO scenLiqConstDataC = SurfaceNetworkService.GetScenarioSectionLiquidConstraintData(scenario.Id.ToString());
            Assert.IsNotNull(scenLiqConstDataC);
            CollectionAssert.AreEqual(scenLiqConstDataC.Values, scenLiqConstData.Values, liqConstComparer);

            //Manual Optimization has been removed.
            // OptimizeSurfaceNetwork also needs a valid model file to proceed.
            //SNOptimisationStatusDTO optstat = SurfaceNetworkService.OptimiseSurfaceNetwork(scenario.Id.ToString());
            //Assert.IsNotNull(optstat);

            long[] userAssets = new long[1]; userAssets[0] = asset.Id;
            SNScenarioScheduleDTO[] schedulesPre = SurfaceNetworkService.GetScenarioSchedulesByUserAssetIds(userAssets, ScenarioType.SurfaceNetwork.ToString());
            Assert.IsNotNull(schedulesPre);
            Assert.IsTrue(schedulesPre.Count() == 0);

            // add the just created scenario and see if it is in the schedules list now
            SurfaceNetworkService.InsertSNScenarioScheduleManualRun(scenario.Id.ToString());

            SNScenarioScheduleDTO[] schedules = SurfaceNetworkService.GetScenarioSchedulesByUserAssetIds(userAssets, ScenarioType.SurfaceNetwork.ToString());
            Assert.IsNotNull(schedules);
            Assert.IsFalse(schedules.Count() == 0);

            SNScenarioScheduleDTO scenschedPre = SurfaceNetworkService.GetSNScenarioSchedule(schedules[0].Id.ToString());
            Assert.IsNotNull(scenschedPre);

            SurfaceNetworkService.RunScheduledOptimizationJobs();

            SNScenarioScheduleDTO scensched = SurfaceNetworkService.GetSNScenarioSchedule(scenschedPre.Id.ToString());
            Assert.IsNull(scensched);

            // Check scenario fetches after optimization pass
            SNScenarioWellDataDTO[] scenWellData2 = SurfaceNetworkService.GetScenarioSectionWellData(scenario.Id.ToString());
            Assert.IsNotNull(scenWellData2);

            // Verify that the two collection effectively match since no changes were made
            CollectionAssert.AreEqual(scenWellData2.ToArray(), scenWellData.ToArray(), scenComparer);

            SNScenarioChokeDataArrayAndUnitsDTO scenChokeData2 = SurfaceNetworkService.GetScenarioSectionChokeData(scenario.Id.ToString());
            Assert.IsNotNull(scenChokeData2);
            CollectionAssert.AreEqual(scenChokeData2.Values, scenChokeData.Values, chokeComparer);

            SNScenarioBlockValveDataDTO[] scenBlockData2 = SurfaceNetworkService.GetScenarioSectionBlockValveData(scenario.Id.ToString());
            Assert.IsNotNull(scenBlockData2);
            CollectionAssert.AreEqual(scenBlockData2.ToArray(), scenBlockData.ToArray(), blockComparer);

            SNScenarioPressureConstraintDataArrayAndUnitsDTO scenPressConstData2 = SurfaceNetworkService.GetScenarioSectionPressureConstraintData(scenario.Id.ToString());
            Assert.IsNotNull(scenPressConstData2);
            CollectionAssert.AreEqual(scenPressConstData2.Values, scenPressConstData.Values, pressConstComparer);

            SNScenarioGasConstraintDataArrayAndUnitsDTO scenGasConstData2 = SurfaceNetworkService.GetScenarioSectionGasConstraintData(scenario.Id.ToString());
            Assert.IsNotNull(scenGasConstData2);
            CollectionAssert.AreEqual(scenGasConstData2.Values, scenGasConstData.Values, gasConstComparer);

            SNScenarioLiquidConstraintDataArrayAndUnitsDTO scenLiqConstData2 = SurfaceNetworkService.GetScenarioSectionLiquidConstraintData(scenario.Id.ToString());
            Assert.IsNotNull(scenLiqConstData2);
            CollectionAssert.AreEqual(scenLiqConstData2.Values, scenLiqConstData.Values, liqConstComparer);

            // Check scenario stats
            SNOptimizationRunHeaderDTO[] optRunHdrs = SurfaceNetworkService.GetSNOptimizationRunHeaderArrayByScenarioId(scenario.Id.ToString());
            Assert.IsNotNull(optRunHdrs);

            SNOptimizationRunStatusDTO optRun = SurfaceNetworkService.GetSNOptimizationRun(optRunHdrs[0].Id.ToString());
            Assert.IsNotNull(optRun);
            Assert.AreEqual(scenario.Id, optRun.SNScenarioId);

            SNOptimizationSinkResultsArrayAndUnitsDTO optSinkRes = SurfaceNetworkService.GetSNOptimizationSinkResultsBySNOptimizationRunId(optRunHdrs[0].Id.ToString());
            Assert.IsNotNull(optSinkRes);
            Assert.IsFalse(optSinkRes.Values.Count() == 0);

            SNOptimizationSinkResultsInsituArrayAndUnitsDTO optSinkRes2 = SurfaceNetworkService.GetSNOptimizationSinkResultsInsituBySNOptimizationRunId(optRunHdrs[0].Id.ToString());
            Assert.IsNotNull(optSinkRes2);
            Assert.IsFalse(optSinkRes2.Values.Count() == 0);

            SNOptimizationWellResultsArrayAndUnitsDTO optWellRes = SurfaceNetworkService.GetSNOptimizationWellResultsBySNOptimizationRunId(optRunHdrs[0].Id.ToString());
            Assert.IsNotNull(optWellRes);
            Assert.IsFalse(optWellRes.Values.Count() == 0);

            SNOptimizationWellResultsInsituArrayAndUnitsDTO optWellRes2 = SurfaceNetworkService.GetSNOptimizationWellResultsInsituBySNOptimizationRunId(optRunHdrs[0].Id.ToString());
            Assert.IsNotNull(optWellRes2);
            Assert.IsFalse(optWellRes2.Values.Count() == 0);

            SNOptimizationChokeResultsArrayAndUnitsDTO chokeres = SurfaceNetworkService.GetSNOptimizationChokeResults(optRunHdrs[0].Id.ToString());
            Assert.IsNotNull(chokeres);
            Assert.IsFalse(chokeres.Values.Count() == 0);

            SNOptimizationPipeResultsArrayAndUnitsDTO piperes = SurfaceNetworkService.GetSNOptimizationPipeResults(optRunHdrs[0].Id.ToString());
            Assert.IsNotNull(piperes);
            Assert.IsFalse(piperes.Values.Count() == 0);

            SNOptimizationSocketResultsArrayAndUnitsDTO sockres = SurfaceNetworkService.GetSocketResults(optRunHdrs[0].Id.ToString());
            Assert.IsNotNull(sockres);
            Assert.IsFalse(sockres.Values.Count() == 0);

            SNOptimizationSocketResultsInSituArrayAndUnitsDTO sockres2 = SurfaceNetworkService.GetSocketResultsInSitu(optRunHdrs[0].Id.ToString());
            Assert.IsNotNull(sockres2);
            Assert.IsFalse(sockres2.Values.Count() == 0);

            // Clean up scenario
            SurfaceNetworkService.RemoveSNScenario(scenario.Id.ToString());
            SNScenarioDTO scenarioRemoved = SurfaceNetworkService.GetSNScenario(scenario.Id.ToString());
            Assert.IsNull(scenarioRemoved);

            // test scheduled scenarios
            SurfaceNetworkService.AddSNScenario(new SNScenarioDTO() { Name = scenarioName2, Description = "Test Description", SNModelId = model.Id, ColorCode = "#FF0000", AllowableScenario = true, Implementable = false, ModelMatch = false, OptimumScenario = false, DoDailyRun = true });
            var allScenarios2 = SurfaceNetworkService.GetSNScenariosBySNModel(model.Id.ToString());
            SNScenarioDTO scenario2 = allScenarios2?.FirstOrDefault(sn => sn.Name.Equals(scenarioName2));
            Assert.IsNotNull(scenario2);

            // add the just created scenario and see if it is in the schedules list now
            SurfaceNetworkService.InsertSNScenarioScheduleDailyRuns();

            SNScenarioScheduleDTO[] schedules2 = SurfaceNetworkService.GetScenarioSchedulesByUserAssetIds(userAssets, ScenarioType.SurfaceNetwork.ToString());
            Assert.IsNotNull(schedules2);
            Assert.IsFalse(schedules2.Count() == 0);

            SNScenarioScheduleDTO scensched2 = SurfaceNetworkService.GetSNScenarioSchedule(schedules2[0].Id.ToString());
            Assert.IsNotNull(scensched2);

            scensched2.StartTime = DateTime.UtcNow;
            scensched2.StartTime.Value.AddHours(2);

            scensched2.EndTime = DateTime.UtcNow;
            scensched2.EndTime.Value.AddHours(4);

            SurfaceNetworkService.UpdateSNScenarioSchedule(scensched2);

            SNScenarioScheduleDTO scensched3 = SurfaceNetworkService.GetSNScenarioSchedule(schedules2[0].Id.ToString());
            Assert.IsNotNull(scensched3);

            // rest api conversion to/from string loses some time fuzz in the .UtcNow returned timestamp so convert to strings to get around this
            Assert.AreEqual(scensched2.StartTime.ToString(), scensched3.StartTime.ToString());

            // trigger a job run to get some history generated
            SurfaceNetworkService.RunScheduledOptimizationJobs();

            SNScenarioScheduleHistoryDTO[] hist = SurfaceNetworkService.GetScenarioScheduleHistoryByUserAssetIds(userAssets, ScenarioType.SurfaceNetwork.ToString());
            Assert.IsNotNull(hist);
            Assert.IsFalse(hist.Count() == 0);

            SurfaceNetworkService.RemoveSNScenario(scenario2.Id.ToString());
            SNScenarioDTO scenarioRemoved2 = SurfaceNetworkService.GetSNScenario(scenario2.Id.ToString());
            Assert.IsNull(scenarioRemoved2);
        }

        // tests for SNWellMapping related functions
        [TestCategory(TestCategories.SurfaceNetworkServiceTests), TestMethod]
        //  [TestMethod]
        public void GetUpdateWellMappings()
        {
            // Setup asset to pull mappings against
            string assetName = "Asset Test Mappings";
            string path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";
            string ReOmodel = "22Wells.reo";
            string modelName = "Model Test";
            string wellName = DefaultWellName;

            SurfaceNetworkService.AddAsset(new AssetDTO() { Name = assetName, Description = "Test Description" });
            var allAssets = SurfaceNetworkService.GetAllAssets().ToList();
            AssetDTO asset = allAssets?.FirstOrDefault(a => a.Name.Equals(assetName));
            Assert.IsNotNull(asset);
            _assetsToRemove.Add(asset);

            // verify Surface network license exist or not
            bool IsSNFeatureLicensed = SurfaceNetworkService.IsSNFeatureLicensed();
            Assert.IsTrue(IsSNFeatureLicensed, "Surface Network License feature is not available");

            //Insert ReO File to create initial Mapping
            SNModelDTO model = AddSurfaceNetworkModelAndModelFile(asset, path, ReOmodel, modelName);
            Assert.IsNotNull(model);

            //Insert Well
            var toAdd = SetDefaultFluidType(new WellDTO() { Name = wellName, FacilityId = "CASETEST", SurfaceLatitude = 1.232m, SurfaceLongitude = 2.3232m, SubAssemblyAPI = "SubAssemblyAPI", AssemblyAPI = "AssemblyAPI", CommissionDate = DateTime.Today, WellType = WellTypeId.RRL });
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = toAdd });
            var allWells = WellService.GetAllWells().ToList();
            WellDTO well = allWells?.FirstOrDefault(a => a.Name.Equals(wellName));
            Assert.IsNotNull(well);
            _wellsToRemove.Add(well);

            long[] userAssets = new long[1]; userAssets[0] = asset.Id;
            SNWellMappingDTO[] mappings = SurfaceNetworkService.GetSNWellMappingsByAssetIds(userAssets);
            Assert.IsNotNull(mappings);

            SurfaceNetworkService.UpdateSNWellMapping(mappings[0]);

            SNWellMappingDTO[] mappingsB = SurfaceNetworkService.GetSNWellMappingsByAssetIds(userAssets);
            Assert.IsNotNull(mappingsB);

            System.Collections.IComparer wellMappingComparer = new WellMappingComparerClass();
            CollectionAssert.AreEqual(mappings, mappingsB, wellMappingComparer);

            SurfaceNetworkService.UpdateSNWellMappings(mappingsB);

            SNWellMappingDTO[] mappingsC = SurfaceNetworkService.GetSNWellMappingsByAssetIds(userAssets);
            Assert.IsNotNull(mappingsC);

            CollectionAssert.AreEqual(mappings, mappingsC, wellMappingComparer);

            SNWellMappingDTO initialWellMapping = mappings[0];
            initialWellMapping.WellId = well.Id;
            SurfaceNetworkService.UpdateSNWellMapping(initialWellMapping);

            SNWellMappingDTO updatedWellMapping = SurfaceNetworkService.GetSNWellMappingByWellId(well.Id.ToString());
            Assert.IsNotNull(updatedWellMapping);

            //Get SN Models By Well Name
            SNModelDTO[] getSNModelDTO = SurfaceNetworkService.GetSNModelsByWellName(well.Name.ToString());
            Assert.IsNotNull(getSNModelDTO);

            string[] SNModelContent = SurfaceNetworkService.GetSNModelContentName(SNCygNetEquipmentType.BlockValve.ToString(), model.Id.ToString());
            Assert.IsNotNull(SNModelContent);

            //Undo Mapping so we can delete the well
            SNWellMappingDTO undoWellMapping = mappings[0];
            undoWellMapping.WellId = null;
            SurfaceNetworkService.UpdateSNWellMapping(undoWellMapping);
        }

        [TestCategory(TestCategories.SurfaceNetworkServiceTests), TestMethod]
        public void GetUpdateSNScenarioSchedules()
        {
            // even with an active scenario (in AddUpdateDeleteScenarioWithModelFile) there are no schedules so I am not sure how to test these functions.

            //SNScenarioScheduleDTO[] schedules = SurfaceNetworkService.GetSNScenarioSchedules();
            //Assert.IsNotNull(schedules);
            //Assert.IsFalse(schedules.Count() == 0);
        }

        [TestCategory(TestCategories.SurfaceNetworkServiceTests), TestMethod]
        public void AddUpdateScenarioSectionDefault()
        {
            string assetName = "Asset Test";
            string modelName = "Model Test";
            string scenarioName = "Scenario Test";
            SurfaceNetworkService.AddAsset(new AssetDTO() { Name = assetName, Description = "Test Description" });
            var allAssets = SurfaceNetworkService.GetAllAssets().ToList();
            AssetDTO asset = allAssets?.FirstOrDefault(a => a.Name.Equals(assetName));
            Assert.IsNotNull(asset);
            _assetsToRemove.Add(asset);

            SurfaceNetworkService.AddSNModel(new SNModelDTO() { Name = modelName, AssetId = asset.Id });
            long[] userAssets = new long[1]; userAssets[0] = asset.Id;
            var allModels = SurfaceNetworkService.GetSNModelsByAssetIds(userAssets);
            SNModelDTO model = allModels?.FirstOrDefault(m => m.Name.Equals(modelName));
            Assert.IsNotNull(model);

            SurfaceNetworkService.AddSNScenario(new SNScenarioDTO() { Name = scenarioName, Description = "Test Description", SNModelId = model.Id, ColorCode = "#FF0000", AllowableScenario = true, Implementable = false, ModelMatch = false, OptimumScenario = false });
            var allScenarios = SurfaceNetworkService.GetSNScenariosBySNModel(model.Id.ToString());
            SNScenarioDTO scenario = allScenarios?.FirstOrDefault(sn => sn.Name.Equals(scenarioName));
            Assert.IsNotNull(scenario);

            //Insert
            SNScenarioSectionDefaultsDTO addScenarioDefaults = new SNScenarioSectionDefaultsDTO();
            addScenarioDefaults.SNScenerioId = scenario.Id;
            addScenarioDefaults.Flow = SNScenarioAction.Ignore;
            addScenarioDefaults.Choke = SNScenarioAction.Ignore;
            addScenarioDefaults.BlockValve = SNScenarioAction.Ignore;
            addScenarioDefaults.Well = SNScenarioAction.Ignore;
            addScenarioDefaults.GasConstraint = SNScenarioAction.Ignore;
            addScenarioDefaults.LiquidConstraint = SNScenarioAction.Ignore;
            addScenarioDefaults.PressureConstraint = SNScenarioAction.Ignore;

            SurfaceNetworkService.SaveSNScenarioSectionDefaults(addScenarioDefaults);
            SNScenarioSectionDefaultsDTO insertedScenarioDefaults = SurfaceNetworkService.GetSNScenarioSectionDefaultsBySNScenario(scenario.Id.ToString());
            Assert.IsNotNull(insertedScenarioDefaults);

            //Update
            insertedScenarioDefaults.Flow = SNScenarioAction.Phase;
            SurfaceNetworkService.SaveSNScenarioSectionDefaults(insertedScenarioDefaults);
            SNScenarioSectionDefaultsDTO updatedScenarioDefaults = SurfaceNetworkService.GetSNScenarioSectionDefaultsBySNScenario(scenario.Id.ToString());
            Assert.IsNotNull(updatedScenarioDefaults);
            Assert.AreEqual(SNScenarioAction.Phase, updatedScenarioDefaults.Flow);
        }

        [TestCategory(TestCategories.SurfaceNetworkServiceTests), TestMethod]
        public void TestScenarioActions()
        {
            foreach (var value in Enum.GetValues(typeof(SNActionType)))
            {
                SNScenarioAction[] scenacts = SurfaceNetworkService.GetSNScenarioActions(((SNActionType)value).ToString());
                Assert.IsNotNull(scenacts);
                Assert.IsTrue(scenacts.Count() > 0);
            }

            SNSectionDefaultActionsDTO actlists = SurfaceNetworkService.GetSNScenarioActionsForSectionDefaults();
            Assert.IsNotNull(actlists);
            Assert.IsNotNull(actlists.FlowSectionDefaults);
            Assert.IsTrue(actlists.FlowSectionDefaults.Count() > 0);
            Assert.IsNotNull(actlists.BlockValveSectionDefaults);
            Assert.IsTrue(actlists.BlockValveSectionDefaults.Count() > 0);
            Assert.IsNotNull(actlists.WellSectionDefaults);
            Assert.IsTrue(actlists.WellSectionDefaults.Count() > 0);
            Assert.IsNotNull(actlists.ChokeSectionDefaults);
            Assert.IsTrue(actlists.ChokeSectionDefaults.Count() > 0);
            Assert.IsNotNull(actlists.PressureConstraintSectionDefaults);
            Assert.IsTrue(actlists.PressureConstraintSectionDefaults.Count() > 0);
            Assert.IsNotNull(actlists.GasConstraintSectionDefaults);
            Assert.IsTrue(actlists.GasConstraintSectionDefaults.Count() > 0);
            Assert.IsNotNull(actlists.LiquidConstraintSectionDefaults);
            Assert.IsTrue(actlists.LiquidConstraintSectionDefaults.Count() > 0);
        }

        [TestCategory(TestCategories.SurfaceNetworkServiceTests), TestMethod]
        public void AddUpdateDeleteSNEquipmentCygNetMapping()
        {
            //Setup
            string assetName = "Asset Test";
            string modelName = "Model Test";
            string equipmentName1 = "Equipment Test 1";
            string equipmentName2 = "Equipment Test 2";

            SurfaceNetworkService.AddAsset(new AssetDTO() { Name = assetName, Description = "Test Description" });
            var allAssets = SurfaceNetworkService.GetAllAssets().ToList();
            AssetDTO asset = allAssets?.FirstOrDefault(a => a.Name.Equals(assetName));
            Assert.IsNotNull(asset);
            _assetsToRemove.Add(asset);

            SurfaceNetworkService.AddSNModel(new SNModelDTO() { Name = modelName, AssetId = asset.Id });
            long[] userAssets = new long[1]; userAssets[0] = asset.Id;
            var allModels = SurfaceNetworkService.GetSNModelsByAssetIds(userAssets);
            SNModelDTO model = allModels?.FirstOrDefault(m => m.Name.Equals(modelName));
            Assert.IsNotNull(model);

            //Add
            SNEquipmentCygNetMappingDTO equipmentMapping1 = new SNEquipmentCygNetMappingDTO();
            equipmentMapping1.EquipmentName = equipmentName1;
            equipmentMapping1.EquipmentType = SNCygNetEquipmentType.BlockValve;
            equipmentMapping1.FacilityId = "FacilityTest";
            equipmentMapping1.SNModelId = model.Id;
            SurfaceNetworkService.AddSNEquipmentCygNetMapping(equipmentMapping1);

            //Get
            var allEquipment = SurfaceNetworkService.GetSNEquipmentCygNetMappingBySNModel(model.Id.ToString());
            SNEquipmentCygNetMappingDTO equipmentMapping1Added = allEquipment?.FirstOrDefault(e => e.EquipmentName.Equals(equipmentName1));
            Assert.IsNotNull(equipmentMapping1Added);

            //Update
            equipmentMapping1Added.EquipmentType = SNCygNetEquipmentType.Choke;
            SurfaceNetworkService.UpdateSNEquipmentCygNetMapping(equipmentMapping1Added);
            allEquipment = SurfaceNetworkService.GetSNEquipmentCygNetMappingBySNModel(model.Id.ToString());
            SNEquipmentCygNetMappingDTO equipmentMapping1Updated = allEquipment?.FirstOrDefault(e => e.EquipmentName.Equals(equipmentName1));
            Assert.AreEqual(equipmentMapping1Updated.EquipmentType, SNCygNetEquipmentType.Choke);

            //Add another
            SNEquipmentCygNetMappingDTO equipmentMapping2 = new SNEquipmentCygNetMappingDTO();
            equipmentMapping2.EquipmentName = equipmentName2;
            equipmentMapping2.EquipmentType = SNCygNetEquipmentType.BlockValve;
            equipmentMapping2.FacilityId = "FacilityTest2";
            equipmentMapping2.SNModelId = model.Id;
            SurfaceNetworkService.AddSNEquipmentCygNetMapping(equipmentMapping2);

            allEquipment = SurfaceNetworkService.GetSNEquipmentCygNetMappingBySNModel(model.Id.ToString());
            SNEquipmentCygNetMappingDTO equipmentMapping2Added = allEquipment?.FirstOrDefault(e => e.EquipmentName.Equals(equipmentName2));
            Assert.IsNotNull(equipmentMapping2Added);

            //UpdateMulti
            equipmentMapping1Added.EquipmentType = SNCygNetEquipmentType.Well;
            equipmentMapping2Added.EquipmentType = SNCygNetEquipmentType.Well;
            List<SNEquipmentCygNetMappingDTO> multiEquipment = new List<SNEquipmentCygNetMappingDTO>();
            multiEquipment.Add(equipmentMapping1Added);
            multiEquipment.Add(equipmentMapping2Added);
            SurfaceNetworkService.UpdateSNEquipmentCygNetMappingMulti(multiEquipment.ToArray());
            allEquipment = SurfaceNetworkService.GetSNEquipmentCygNetMappingBySNModel(model.Id.ToString());
            Assert.AreEqual(2, allEquipment.Count());
            Assert.AreEqual(allEquipment[0].EquipmentType, SNCygNetEquipmentType.Well);
            Assert.AreEqual(allEquipment[1].EquipmentType, SNCygNetEquipmentType.Well);

            //Remove
            SurfaceNetworkService.RemoveSNEquipmentCygNetMapping(equipmentMapping1Added.Id.ToString());
            SurfaceNetworkService.RemoveSNEquipmentCygNetMapping(equipmentMapping2Added.Id.ToString());
            allEquipment = SurfaceNetworkService.GetSNEquipmentCygNetMappingBySNModel(model.Id.ToString());
            Assert.AreEqual(0, allEquipment.Count());
        }

        //[TestCategory(TestCategories.SurfaceNetworkServiceTests), TestMethod]
        [TestMethod]
        public void UpdateBlockValve_SetToOpen_Optimise_GetBlockValve_BlockValveOpened()
        {
            string assetName = "Asset Test SN";
            string path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";
            string ReOmodel = "22Wells.reo";
            string modelName = "Model Test";
            string scenarioName = "Scenario Test";
            string blockValveName = "BV_A18_LINE2";

            //Configure new Asset
            var asset = AddAsset(assetName);
            Assert.IsNotNull(asset);

            //Configure new sn model and model file
            var model = AddSurfaceNetworkModelAndModelFile(asset, path, ReOmodel, modelName);
            Assert.IsNotNull(model);

            //Configure new scenario
            var scenario = AddScenario(model, scenarioName);
            Assert.IsNotNull(scenario);

            //Get the block valve data
            var blockValves = SurfaceNetworkService.GetScenarioSectionBlockValveData(scenario.Id.ToString());

            //update the block valve data
            foreach (var item in blockValves)
            {
                if (item.Name == blockValveName)
                {
                    item.Action = SNScenarioAction.Open;
                    break;
                }
            }

            //save the block valve data
            SurfaceNetworkService.SaveScenarioSectionBlockValveDataMulti(blockValves);

            //Manual Optimization has been removed.
            //var status = SurfaceNetworkService.OptimiseSurfaceNetwork(scenario.Id.ToString());
            SurfaceNetworkService.InsertSNScenarioScheduleManualRun(scenario.Id.ToString());
            SurfaceNetworkService.RunScheduledOptimizationJobs();

            //get the saved optimised file from DB
            var models = SurfaceNetworkService.GetSNModelFilesByModelId(model.Id.ToString());

            var latestModel = models.OrderByDescending(m => m.ChangeDate).FirstOrDefault();

            //use ReO service to open optimised model file
            SystemSettingDTO defaultDirectorySetting = SettingService.GetSystemSettingByName("Surface Network Default Directory");

            var baseFolderPath = defaultDirectorySetting.StringValue;
            var optimizeFolderPath = SetUpDirectory(-5, baseFolderPath);
            string reoFilePath = WriteReOFileToServer(latestModel, optimizeFolderPath);

            ReOEquipmentService.OpenFile(reoFilePath);
            ReOEquipmentService.OpenSnapshot("Initial");

            //use reo service to check block valve is open
            var bv = ReOEquipmentService.GetBlockValve(blockValveName);

            Assert.IsTrue(bv.IsOpen);

            ReOEquipmentService.CloseFile();

            // Clean up file system
            DeleteDirectory(baseFolderPath + @"\-5");

            // Clean up scenario
            SurfaceNetworkService.RemoveSNScenario(scenario.Id.ToString());
            SNScenarioDTO scenarioRemoved = SurfaceNetworkService.GetSNScenario(scenario.Id.ToString());
            Assert.IsNull(scenarioRemoved);
        }

        [TestCategory(TestCategories.SurfaceNetworkServiceTests), TestMethod]
        //  [TestMethod]
        public void UpdateBlockValve_SetToClose_Optimise_GetBlockValve_BlockValveClosed()
        {
            string assetName = "Asset Test SN";
            string path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";
            string ReOmodel = "22Wells.reo";
            string modelName = "Model Test";
            string scenarioName = "Scenario Test";
            string blockValveName = "BV_A18_LINE1";

            //Configure new Asset
            var asset = AddAsset(assetName);
            Assert.IsNotNull(asset);

            //Configure new sn model and model file
            var model = AddSurfaceNetworkModelAndModelFile(asset, path, ReOmodel, modelName);
            Assert.IsNotNull(model);

            //Configure new scenario
            var scenario = AddScenario(model, scenarioName);
            Assert.IsNotNull(scenario);

            //Get the block valve data
            var blockValves = SurfaceNetworkService.GetScenarioSectionBlockValveData(scenario.Id.ToString());

            //update the block valve data
            foreach (var item in blockValves)
            {
                if (item.Name == blockValveName)
                {
                    item.Action = SNScenarioAction.Close;
                    break;
                }
            }

            //save the block valve data
            SurfaceNetworkService.SaveScenarioSectionBlockValveDataMulti(blockValves);

            //Manual Optimization has been removed.
            //var status = SurfaceNetworkService.OptimiseSurfaceNetwork(scenario.Id.ToString());
            SurfaceNetworkService.InsertSNScenarioScheduleManualRun(scenario.Id.ToString());
            SurfaceNetworkService.RunScheduledOptimizationJobs();

            //get the saved optimised file from DB
            var models = SurfaceNetworkService.GetSNModelFilesByModelId(model.Id.ToString());

            var latestModel = models.OrderByDescending(m => m.ChangeDate).FirstOrDefault();

            //use ReO service to open optimised model file
            SystemSettingDTO defaultDirectorySetting = SettingService.GetSystemSettingByName("Surface Network Default Directory");

            var baseFolderPath = defaultDirectorySetting.StringValue;
            var optimizeFolderPath = SetUpDirectory(-5, baseFolderPath);
            string reoFilePath = WriteReOFileToServer(latestModel, optimizeFolderPath);

            ReOEquipmentService.OpenFile(reoFilePath);
            ReOEquipmentService.OpenSnapshot("Initial");

            //use reo service to check block valve is open
            var bv = ReOEquipmentService.GetBlockValve(blockValveName);

            Assert.IsFalse(bv.IsOpen);

            ReOEquipmentService.CloseFile();

            // Clean up file system
            DeleteDirectory(baseFolderPath + @"\-5");

            // Clean up scenario
            SurfaceNetworkService.RemoveSNScenario(scenario.Id.ToString());
            SNScenarioDTO scenarioRemoved = SurfaceNetworkService.GetSNScenario(scenario.Id.ToString());
            Assert.IsNull(scenarioRemoved);
        }

        [TestCategory(TestCategories.SurfaceNetworkServiceTests), TestMethod]

        public void UpdateAllBlockValves_SetToOpen_Optimise_GetBlockValves_AllBlockValvesOpened()
        {
            string assetName = "Asset Test SN";
            string path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";
            string ReOmodel = "22Wells.reo";
            string modelName = "Model Test";
            string scenarioName = "Scenario Test";

            //Configure new Asset
            var asset = AddAsset(assetName);
            Assert.IsNotNull(asset);

            //Configure new sn model and model file
            var model = AddSurfaceNetworkModelAndModelFile(asset, path, ReOmodel, modelName);
            Assert.IsNotNull(model);

            //Configure new scenario
            var scenario = AddScenario(model, scenarioName);
            Assert.IsNotNull(scenario);

            //Get the block valve data
            var blockValves = SurfaceNetworkService.GetScenarioSectionBlockValveData(scenario.Id.ToString());

            //update the block valve data
            foreach (var item in blockValves)
            {
                item.Action = SNScenarioAction.Open;
            }

            //save the block valve data
            SurfaceNetworkService.SaveScenarioSectionBlockValveDataMulti(blockValves);

            //Manual Optimization has been removed.
            //var status = SurfaceNetworkService.OptimiseSurfaceNetwork(scenario.Id.ToString());
            SurfaceNetworkService.InsertSNScenarioScheduleManualRun(scenario.Id.ToString());
            SurfaceNetworkService.RunScheduledOptimizationJobs();

            //get the saved optimised file from DB
            var models = SurfaceNetworkService.GetSNModelFilesByModelId(model.Id.ToString());

            var latestModel = models.OrderByDescending(m => m.ChangeDate).FirstOrDefault();

            //use ReO service to open optimised model file
            SystemSettingDTO defaultDirectorySetting = SettingService.GetSystemSettingByName("Surface Network Default Directory");

            var baseFolderPath = defaultDirectorySetting.StringValue;
            var optimizeFolderPath = SetUpDirectory(-5, baseFolderPath);
            string reoFilePath = WriteReOFileToServer(latestModel, optimizeFolderPath);

            ReOEquipmentService.OpenFile(reoFilePath);
            ReOEquipmentService.OpenSnapshot("Initial");

            foreach (var item in blockValves)
            {
                //use reo service to check block valve is open
                var bv = ReOEquipmentService.GetBlockValve(item.Name);
                Assert.IsTrue(bv.IsOpen);
            }

            ReOEquipmentService.CloseFile();

            // Clean up file system
            DeleteDirectory(baseFolderPath + @"\-5");

            // Clean up scenario
            SurfaceNetworkService.RemoveSNScenario(scenario.Id.ToString());
            SNScenarioDTO scenarioRemoved = SurfaceNetworkService.GetSNScenario(scenario.Id.ToString());
            Assert.IsNull(scenarioRemoved);
        }

        [TestCategory(TestCategories.SurfaceNetworkServiceTests), TestMethod]

        public void UpdateAllBlockValves_SetToClose_Optimise_GetBlockValves_AllBlockValvesClosed()
        {
            string assetName = "Asset Test SN";
            string path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";
            string ReOmodel = "22Wells.reo";
            string modelName = "Model Test";
            string scenarioName = "Scenario Test";

            //Configure new Asset
            var asset = AddAsset(assetName);
            Assert.IsNotNull(asset);

            //Configure new sn model and model file
            var model = AddSurfaceNetworkModelAndModelFile(asset, path, ReOmodel, modelName);
            Assert.IsNotNull(model);

            //Configure new scenario
            var scenario = AddScenario(model, scenarioName);
            Assert.IsNotNull(scenario);

            //Get the block valve data
            var blockValves = SurfaceNetworkService.GetScenarioSectionBlockValveData(scenario.Id.ToString());

            //update the block valve data
            foreach (var item in blockValves)
            {
                item.Action = SNScenarioAction.Close;
            }

            //save the block valve data
            SurfaceNetworkService.SaveScenarioSectionBlockValveDataMulti(blockValves);

            //Manual Optimization has been removed.
            //var status = SurfaceNetworkService.OptimiseSurfaceNetwork(scenario.Id.ToString());
            SurfaceNetworkService.InsertSNScenarioScheduleManualRun(scenario.Id.ToString());
            SurfaceNetworkService.RunScheduledOptimizationJobs();

            //get the saved optimised file from DB
            var models = SurfaceNetworkService.GetSNModelFilesByModelId(model.Id.ToString());

            var latestModel = models.OrderByDescending(m => m.ChangeDate).FirstOrDefault();

            //use ReO service to open optimised model file
            SystemSettingDTO defaultDirectorySetting = SettingService.GetSystemSettingByName("Surface Network Default Directory");

            var baseFolderPath = defaultDirectorySetting.StringValue;
            var optimizeFolderPath = SetUpDirectory(-5, baseFolderPath);
            string reoFilePath = WriteReOFileToServer(latestModel, optimizeFolderPath);

            ReOEquipmentService.OpenFile(reoFilePath);
            ReOEquipmentService.OpenSnapshot("Initial");

            foreach (var item in blockValves)
            {
                //use reo service to check block valve is open
                var bv = ReOEquipmentService.GetBlockValve(item.Name);
                Assert.IsFalse(bv.IsOpen);
            }

            ReOEquipmentService.CloseFile();

            // Clean up file system
            DeleteDirectory(baseFolderPath + @"\-5");

            // Clean up scenario
            SurfaceNetworkService.RemoveSNScenario(scenario.Id.ToString());
            SNScenarioDTO scenarioRemoved = SurfaceNetworkService.GetSNScenario(scenario.Id.ToString());
            Assert.IsNull(scenarioRemoved);
        }

        [TestCategory(TestCategories.SurfaceNetworkServiceTests), TestMethod]

        public void UpdateChokeData_FixedThroat_SetTo_Optimise_GetChokeData_ChokeDataUpdated()
        {
            string assetName = "Asset Test SN";
            string path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";
            string ReOmodel = "22Wells.reo";
            string modelName = "Model Test";
            string scenarioName = "Scenario Test";
            string chokeName = "Choke_A18_MAX";
            decimal chokeDiameter = 250;
            decimal chokeTuningFactor = 2;

            //Configure new Asset
            var asset = AddAsset(assetName);
            Assert.IsNotNull(asset);

            //Configure new sn model and model file
            var model = AddSurfaceNetworkModelAndModelFile(asset, path, ReOmodel, modelName);
            Assert.IsNotNull(model);

            //Configure new scenario
            var scenario = AddScenario(model, scenarioName);
            Assert.IsNotNull(scenario);

            //Get the block valve data
            var chokeData = SurfaceNetworkService.GetScenarioSectionChokeData(scenario.Id.ToString());

            var newChoke = new SNScenarioChokeDataDTO();

            foreach (var chokeItem in chokeData.Values)
            {
                if (chokeItem.Name == chokeName)
                {
                    chokeItem.Type = ReOChokeType.FixedThroat;

                    chokeItem.DiameterAction = SNScenarioAction.SetTo;
                    chokeItem.DiameterInput = chokeDiameter;

                    chokeItem.TuningFactorAction = SNScenarioAction.SetTo;
                    chokeItem.TuningFactor = chokeTuningFactor;

                    newChoke = chokeItem;

                    break;
                }
            }

            var dto = new SNScenarioChokeDataAndUnitsDTO(chokeData.Units, newChoke);

            SurfaceNetworkService.SaveScenarioSectionChokeData(dto);

            //Manual Optimization has been removed.
            //var status = SurfaceNetworkService.OptimiseSurfaceNetwork(scenario.Id.ToString());
            SurfaceNetworkService.InsertSNScenarioScheduleManualRun(scenario.Id.ToString());
            SurfaceNetworkService.RunScheduledOptimizationJobs();

            //get the saved optimised file from DB
            var models = SurfaceNetworkService.GetSNModelFilesByModelId(model.Id.ToString());

            var latestModel = models.OrderByDescending(m => m.ChangeDate).FirstOrDefault();

            //use ReO service to open optimised model file
            SystemSettingDTO defaultDirectorySetting = SettingService.GetSystemSettingByName("Surface Network Default Directory");

            var baseFolderPath = defaultDirectorySetting.StringValue;
            var optimizeFolderPath = SetUpDirectory(-5, baseFolderPath);
            string reoFilePath = WriteReOFileToServer(latestModel, optimizeFolderPath);

            ReOEquipmentService.OpenFile(reoFilePath);
            ReOEquipmentService.OpenSnapshot("Initial");

            //use reo service to check block valve is open
            var choke = ReOEquipmentService.GetChoke(chokeName);

            //convert from mm to 1/64th inch - ReO is in metric FS is oilfield
            var updatedChoke = UnitLibrary.Instance.Convert(UnitKeys.Meters, choke.Diameter, UnitKeys.One64thInch, false);

            Assert.IsNotNull(updatedChoke.Item2);
            Assert.AreEqual(updatedChoke.Item2.Value, Convert.ToDouble(chokeDiameter));
            Assert.AreEqual(choke.TuningFactor, Convert.ToDouble(chokeTuningFactor));
            Assert.AreEqual(choke.TypeOfChoke, ChokeType.FIXED_THROAT);

            ReOEquipmentService.CloseFile();

            // Clean up file system
            DeleteDirectory(baseFolderPath + @"\-5");

            // Clean up scenario
            SurfaceNetworkService.RemoveSNScenario(scenario.Id.ToString());
            SNScenarioDTO scenarioRemoved = SurfaceNetworkService.GetSNScenario(scenario.Id.ToString());
            Assert.IsNull(scenarioRemoved);
        }

        [TestCategory(TestCategories.SurfaceNetworkServiceTests), TestMethod]
        public void MultiAssetNetworkmapping()
        {


            string path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";

            string ReOmodel = "22Wells.reo";
            AddAsset("TestAsset1", "TestAsset1");
            AddAsset("TestAsset2", "TestAsset2");
            AddAsset("TestAsset3", "TestAsset3");
            AssetDTO[] assets = SurfaceNetworkService.GetAllAssets();
            AddDeleteNewWells("RRLWELL_TEST", WellTypeId.RRL);


            AddDeleteNewWells("NFWELL_TEST", WellTypeId.NF, assets[1].Id);


            AddDeleteNewWells("ESPWELL_TEST", WellTypeId.ESP, assets[2].Id);


            AddDeleteNewWells("GASLIFTWELL_TEST", WellTypeId.GLift, assets[3].Id);
            WellFilterDTO wellFilter = new WellFilterDTO();

            wellFilter.AssetIds = new long?[] { assets[1].Id, assets[2].Id };



            var model1 = AddSurfaceNetworkModelAndModelFile(assets[1], path, ReOmodel, "TestAsset1");
            var model2 = AddSurfaceNetworkModelAndModelFile(assets[2], path, ReOmodel, "TestAsset2");
            var model3 = AddSurfaceNetworkModelAndModelFile(assets[3], path, ReOmodel, "TestAsset3");
            var model4 = AddSurfaceNetworkModelAndModelFile(assets[1], path, ReOmodel, "TestAsset4");
            WellDTO[] filtered_Wells = WellService.GetWellsByFilter(wellFilter);
            SurfaceNetworkService.GetSNModelsByAssetIds(new long[] { assets[1].Id, assets[2].Id });

        }

        public void AddAsset(string AssetName, string AssetDescription)
        {
            UserDTO user = AdministrationService.GetUser(AuthenticatedUser.Id.ToString());

            SurfaceNetworkService.AddAsset(new AssetDTO() { Name = AssetName, Description = AssetDescription });
            var allAssets = SurfaceNetworkService.GetAllAssets().ToList();
            AssetDTO asset = allAssets?.FirstOrDefault(a => a.Name.Equals(AssetName));
            _assetsToRemove.Add(asset);
            user.Assets.Add(asset);
            AdministrationService.UpdateUser(user);
        }


        [TestCategory(TestCategories.SurfaceNetworkServiceTests), TestMethod]

        public void UpdateChokeData_MaxThroat_SetTo_Optimise_GetChokeData_ChokeDataUpdated()
        {
            string assetName = "Asset Test SN";
            string path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";
            string ReOmodel = "22Wells.reo";
            string modelName = "Model Test";
            string scenarioName = "Scenario Test";
            string chokeName = "Choke_A18_MAX";
            decimal chokeDiameter = 250;
            decimal chokeTuningFactor = 2;

            //Configure new Asset
            var asset = AddAsset(assetName);
            Assert.IsNotNull(asset);

            //Configure new sn model and model file
            var model = AddSurfaceNetworkModelAndModelFile(asset, path, ReOmodel, modelName);
            Assert.IsNotNull(model);

            //Configure new scenario
            var scenario = AddScenario(model, scenarioName);
            Assert.IsNotNull(scenario);

            //Get the block valve data
            var chokeData = SurfaceNetworkService.GetScenarioSectionChokeData(scenario.Id.ToString());

            var newChoke = new SNScenarioChokeDataDTO();

            foreach (var chokeItem in chokeData.Values)
            {
                if (chokeItem.Name == chokeName)
                {
                    chokeItem.Type = ReOChokeType.FixedThroat;

                    chokeItem.DiameterAction = SNScenarioAction.SetTo;
                    chokeItem.DiameterInput = chokeDiameter;

                    chokeItem.TuningFactorAction = SNScenarioAction.SetTo;
                    chokeItem.TuningFactor = chokeTuningFactor;

                    newChoke = chokeItem;

                    break;
                }
            }

            var dto = new SNScenarioChokeDataAndUnitsDTO(chokeData.Units, newChoke);

            SurfaceNetworkService.SaveScenarioSectionChokeData(dto);

            //Manual Optimization has been removed.
            //var status = SurfaceNetworkService.OptimiseSurfaceNetwork(scenario.Id.ToString());
            SurfaceNetworkService.InsertSNScenarioScheduleManualRun(scenario.Id.ToString());
            SurfaceNetworkService.RunScheduledOptimizationJobs();

            //get the saved optimised file from DB
            var models = SurfaceNetworkService.GetSNModelFilesByModelId(model.Id.ToString());

            var latestModel = models.OrderByDescending(m => m.ChangeDate).FirstOrDefault();

            //use ReO service to open optimised model file
            SystemSettingDTO defaultDirectorySetting = SettingService.GetSystemSettingByName("Surface Network Default Directory");

            var baseFolderPath = defaultDirectorySetting.StringValue;
            var optimizeFolderPath = SetUpDirectory(-5, baseFolderPath);
            string reoFilePath = WriteReOFileToServer(latestModel, optimizeFolderPath);

            ReOEquipmentService.OpenFile(reoFilePath);
            ReOEquipmentService.OpenSnapshot("Initial");

            //use reo service to check block valve is open
            var choke = ReOEquipmentService.GetChoke(chokeName);

            //convert from mm to 1/64th inch - ReO is in metric FS is oilfield
            var updatedChoke = UnitLibrary.Instance.Convert(UnitKeys.Meters, choke.Diameter, UnitKeys.One64thInch, false);

            Assert.IsNotNull(updatedChoke.Item2);
            Assert.AreEqual(updatedChoke.Item2.Value, Convert.ToDouble(chokeDiameter));
            Assert.AreEqual(choke.TuningFactor, Convert.ToDouble(chokeTuningFactor));
            Assert.AreEqual(choke.TypeOfChoke, ChokeType.FIXED_THROAT);

            ReOEquipmentService.CloseFile();

            // Clean up file system
            DeleteDirectory(baseFolderPath + @"\-5");

            // Clean up scenario
            SurfaceNetworkService.RemoveSNScenario(scenario.Id.ToString());
            SNScenarioDTO scenarioRemoved = SurfaceNetworkService.GetSNScenario(scenario.Id.ToString());
            Assert.IsNull(scenarioRemoved);
        }

        [TestCategory(TestCategories.SurfaceNetworkServiceTests), TestMethod]

        public void UpdateChokeData_PressureReducer_SetTo_Optimise_GetChokeData_ChokeDataUpdated()
        {
            string assetName = "Asset Test SN";
            string path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";
            string ReOmodel = "22Wells.reo";
            string modelName = "Model Test";
            string scenarioName = "Scenario Test";
            string chokeName = "Choke_2";
            decimal chokeDiameter = 250;
            decimal chokeTuningFactor = 3;

            //Configure new Asset
            var asset = AddAsset(assetName);
            Assert.IsNotNull(asset);


            //Configure new sn model and model file
            var model = AddSurfaceNetworkModelAndModelFile(asset, path, ReOmodel, modelName);
            Assert.IsNotNull(model);

            //Configure new scenario
            var scenario = AddScenario(model, scenarioName);
            Assert.IsNotNull(scenario);

            //Get the choke data
            var chokeData = SurfaceNetworkService.GetScenarioSectionChokeData(scenario.Id.ToString());

            var newChoke = new SNScenarioChokeDataDTO();

            foreach (var chokeItem in chokeData.Values)
            {
                if (chokeItem.Name == chokeName)
                {
                    chokeItem.DiameterAction = SNScenarioAction.SetTo;
                    chokeItem.DiameterInput = chokeDiameter;

                    chokeItem.TuningFactorAction = SNScenarioAction.SetTo;
                    chokeItem.TuningFactor = chokeTuningFactor;

                    newChoke = chokeItem;

                    break;
                }
            }

            var dto = new SNScenarioChokeDataAndUnitsDTO(chokeData.Units, newChoke);

            SurfaceNetworkService.SaveScenarioSectionChokeData(dto);

            //Manual Optimization has been removed.
            //var status = SurfaceNetworkService.OptimiseSurfaceNetwork(scenario.Id.ToString());
            SurfaceNetworkService.InsertSNScenarioScheduleManualRun(scenario.Id.ToString());
            SurfaceNetworkService.RunScheduledOptimizationJobs();

            //get the saved optimised file from DB
            var models = SurfaceNetworkService.GetSNModelFilesByModelId(model.Id.ToString());

            var latestModel = models.OrderByDescending(m => m.ChangeDate).FirstOrDefault();

            //use ReO service to open optimised model file
            SystemSettingDTO defaultDirectorySetting = SettingService.GetSystemSettingByName("Surface Network Default Directory");

            var baseFolderPath = defaultDirectorySetting.StringValue;
            var optimizeFolderPath = SetUpDirectory(-5, baseFolderPath);
            string reoFilePath = WriteReOFileToServer(latestModel, optimizeFolderPath);

            ReOEquipmentService.OpenFile(reoFilePath);
            ReOEquipmentService.OpenSnapshot("Initial");

            //use reo service to check block valve is open
            var choke = ReOEquipmentService.GetChoke(chokeName);

            //convert from mm to 1/64th inch - ReO is in metric FS is oilfield
            var updatedChoke = UnitLibrary.Instance.Convert(UnitKeys.Meters, choke.Diameter / 1000, UnitKeys.One64thInch, false);

            Assert.IsNotNull(updatedChoke.Item2);
            Assert.AreEqual(choke.TypeOfChoke, ChokeType.PRESSURE_REDUCER);
            Assert.AreNotEqual(updatedChoke.Item2.Value, Convert.ToDouble(chokeDiameter));
            Assert.AreNotEqual(choke.TuningFactor, Convert.ToDouble(chokeTuningFactor));

            ReOEquipmentService.CloseFile();

            // Clean up file system
            DeleteDirectory(baseFolderPath + @"\-5");

            // Clean up scenario
            SurfaceNetworkService.RemoveSNScenario(scenario.Id.ToString());
            SNScenarioDTO scenarioRemoved = SurfaceNetworkService.GetSNScenario(scenario.Id.ToString());
            Assert.IsNull(scenarioRemoved);
        }

        [TestCategory(TestCategories.SurfaceNetworkServiceTests), TestMethod]
        //FRWM-308- Testing of Well Test Validation - Choke (ADNOC)
        public void NetworkResultChokeDFactorCheckdependingOnConfiguation()
        {
            WellDTO well1 = new WellDTO();
            var asset = new AssetDTO();
            string chokeName = "Choke 1";
            decimal? chokeDiameterReO = 250;
            decimal? chokeTuningFactorReO = 1;
            try
            {
                #region Change unit system to US
                ChangeUnitSystem("US");
                ChangeUnitSystemUserSetting("US");
                Trace.WriteLine("System set for US unit system");
                #endregion Change unit system to US

                #region Configure new Asset
                string assetName = "AssetSN";
                asset = AddAsset(assetName);
                Assert.IsNotNull(asset);
                # endregion Configure new Asset

                #region Well Creation 
                WellConfigurationService.AddWellConfig(new WellConfigDTO()
                {
                    Well = SetDefaultFluidTypeAndPhase(new WellDTO()
                    {
                        Name = "well1",
                        CommissionDate = (DateTime.Today - TimeSpan.FromDays(100)),
                        WellType = WellTypeId.NF,
                        AssemblyAPI = "NFWWELL_00001",
                        SubAssemblyAPI = "NFWWELL_00001",
                        IntervalAPI = "NFWWELL_00001",
                        DepthCorrectionFactor = 2,
                        WellDepthDatumElevation = 1,
                        WellDepthDatumId = 2,
                        AssetId = asset.Id
                    })
                });
                well1 = WellService.GetWellByName("well1");
                Assert.IsNotNull(well1);
                _wellsToRemove.Add(well1);
                #endregion Well Creation

                #region model file with Choke D Factor Option
                string Path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";
                byte[] fileAsByteArray;
                ModelFileValidationDataDTO ModelFileValidationData;
                AssemblyDTO assembly = WellboreComponentService.GetAssemblyByWellId(well1.Id.ToString());
                ModelFileOptionDTO options = new ModelFileOptionDTO();
                ModelFileBase64DTO modelFile = new ModelFileBase64DTO() { };
                modelFile.ApplicableDate = well1.CommissionDate.Value.AddDays(1).ToUniversalTime();
                modelFile.WellId = well1.Id;
                fileAsByteArray = GetByteArray(Path, "WellfloNFWExample1.wflx");
                options.CalibrationMethod = CalibrationMethodId.LFactor;
                options.Comment = "NF";
                options.OptionalUpdate = new long[]
                { ((long)OptionalUpdates.UpdateWCT_WGR),
                   ((long)OptionalUpdates.UpdateGOR_CGR),
                   ((long)OptionalUpdates.CalculateChokeD_Factor)  //Uploading without Choke Factor
                };
                modelFile.Base64Contents = Convert.ToBase64String(fileAsByteArray);
                modelFile.Options = options;
                ModelFileValidationData = ModelFileService.GetModelFileValidation(modelFile);
                if (ModelFileValidationData != null)
                    ModelFileService.AddWellModelFile(modelFile);
                else
                    Trace.WriteLine(string.Format("Failed to validate NF model file"));
                ModelFileDTO modelAdded = ModelFileService.GetCurrentModelFile(well1.Id.ToString());

                Assert.IsNotNull(modelAdded);
                _modelFilesToRemove.Add(modelAdded.Id.ToString());
                #endregion model file with Choke D Factor Option

                #region Entering Well Test Data
                // Inserting Well Test Data
                WellTestDTO testDataDTO = new WellTestDTO()
                {
                    WellId = well1.Id,
                    SPTCodeDescription = "AllocatableTest",
                    TestDuration = 24,
                    AverageTubingPressure = 1000,
                    AverageTubingTemperature = 80.3m,
                    GaugePressure = 5800,
                    Oil = 100,
                    Gas = 50,
                    Water = 35,
                    ChokeSize = 360,
                    FlowLinePressure = 800,
                    FlowLineTemperature = 67.3m,
                    SeparatorPressure = 500,
                    Comment = "Choke D Factor Calulations",
                };
                testDataDTO.SampleDate = well1.CommissionDate.Value.AddDays(2).ToUniversalTime();
                WellTestUnitsDTO testUnits = WellTestDataService.GetWellTestDefaults(well1.Id.ToString()).Units;
                WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(testUnits, testDataDTO));
                var welltestdata = WellTestDataService.GetWellTestDataByWellId(well1.Id.ToString());
                decimal? wellTestChokeDFactor = welltestdata.Values[0].ChokeDFactor;
                #endregion Entering Well Test Data

                #region Add a surface network model.
                string path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";
                SNModelAndModelFileDTO modelAndFile = new SNModelAndModelFileDTO();
                modelAndFile.SNModel = new SNModelDTO() { Name = "Recycle Network", AssetId = asset.Id };
                SNModelFileDTO ReOmodelFile = new SNModelFileDTO();
                ReOmodelFile.Comments = "Test Model File;";
                byte[] fileAsByteArrayReO = GetByteArray(path, "TutorialExample2.reo");
                Assert.IsNotNull(fileAsByteArrayReO);
                ReOmodelFile.Base64Contents = Convert.ToBase64String(fileAsByteArrayReO);
                modelAndFile.SNModelFile = ReOmodelFile;
                SurfaceNetworkService.SaveReOFile(modelAndFile);
                var allModels = SurfaceNetworkService.GetSNModelsByAssetIds(new long[] { asset.Id });
                SNModelDTO model = allModels?.FirstOrDefault(m => m.Name.Equals("Recycle Network"));
                _snModelsToRemove.Add(model);
                #endregion Add a surface network model.

                #region Map the ForeSite wells to the Reo wells within the surface network model.
                WellDTO[] wells = WellService.GetWellsByUserAssetIds(new long[] { asset.Id });
                SNWellMappingDTO[] wellmappings = SurfaceNetworkService.GetSNWellMappingsByAssetIds(new long[] { asset.Id });
                wellmappings[0].Well = well1;
                wellmappings[0].WellId = well1.Id;
                Assert.IsNotNull(model);
                SurfaceNetworkService.UpdateSNWellMappings(wellmappings);
                #endregion Map the ForeSite wells to the Reo wells within the surface network model.

                #region Scenario 1 -- If choke size option update is checked, then choke tuning factor will be retrieved from the latest valid well test record.

                #region Create SN scenario and run 
                SNScenarioDTO addScenarioObj = new SNScenarioDTO()
                {
                    Name = "SNScenario1",
                    Description = "SNScenario1 Description",
                    SNModelId = model.Id,
                    ColorCode = "#FF0000",
                    AllowableScenario = true,
                    Implementable = false,
                    ModelMatch = false,
                    OptimumScenario = false,
                    IsForecastScenario = true
                };
                SurfaceNetworkService.AddSNScenario(addScenarioObj);
                var allScenarios = SurfaceNetworkService.GetSNScenariosBySNModel(model.Id.ToString());
                SNScenarioDTO scenario = allScenarios?.FirstOrDefault(sn => sn.Name.Equals("SNScenario1"));
                Assert.IsNotNull(scenario);
                _snScenariosToRemove.Add(scenario);
                //Get the block valve  
                SNScenarioSectionDefaultsDTO insertedScenarioDefaults = SurfaceNetworkService.GetSNScenarioSectionDefaultsBySNScenario(scenario.Id.ToString());
                Assert.IsNotNull(insertedScenarioDefaults);
                //Update Choke property
                insertedScenarioDefaults.Choke = SNScenarioAction.Update;
                SurfaceNetworkService.SaveSNScenarioSectionDefaults(insertedScenarioDefaults);
                SNScenarioSectionDefaultsDTO updatedScenarioDefaults = SurfaceNetworkService.GetSNScenarioSectionDefaultsBySNScenario(scenario.Id.ToString());
                Assert.IsNotNull(updatedScenarioDefaults);
                Assert.AreEqual(SNScenarioAction.Update, updatedScenarioDefaults.Choke);
                //Update related MaxThroat Choke Data
                var chokeData = SurfaceNetworkService.GetScenarioSectionChokeData(scenario.Id.ToString());
                var newChoke = new SNScenarioChokeDataDTO();
                foreach (var chokeItem in chokeData.Values)
                {
                    if (chokeItem.Name == chokeName)
                    {
                        chokeItem.Type = ReOChokeType.MaxThroat;
                        chokeItem.DiameterAction = SNScenarioAction.Update;
                        chokeItem.DiameterInput = chokeDiameterReO;
                        chokeItem.TuningFactorAction = SNScenarioAction.Update;
                        chokeItem.TuningFactor = chokeTuningFactorReO;
                        newChoke = chokeItem;
                        break;
                    }
                }
                var dto = new SNScenarioChokeDataAndUnitsDTO(chokeData.Units, newChoke);
                SurfaceNetworkService.SaveScenarioSectionChokeData(dto);
                SurfaceNetworkService.InsertSNScenarioScheduleManualRun(scenario.Id.ToString());
                SurfaceNetworkService.RunScheduledOptimizationJobs();
                #endregion   Create SN scenario and run 

                #region Check scenario stats
                SNOptimizationRunHeaderDTO[] optRunHdrs = SurfaceNetworkService.GetSNOptimizationRunHeaderArrayByScenarioId(scenario.Id.ToString());
                Assert.IsNotNull(optRunHdrs);
                SNOptimizationChokeResultsArrayAndUnitsDTO chokeres = SurfaceNetworkService.GetSNOptimizationChokeResults(optRunHdrs[0].Id.ToString());
                Assert.IsNotNull(chokeres);
                Assert.IsFalse(chokeres.Values.Count() == 0);
                decimal? chokeTuningFactor = chokeres.Values[0].socChokeTuningFactor;
                //If choke size option update is checked, then choke tuning factor will be retrieved from the latest valid well test record
                Assert.AreEqual(chokeTuningFactor, wellTestChokeDFactor, "For Scenario 1 Expected Rate: " + chokeTuningFactor + " Actual Rate: " + wellTestChokeDFactor);
                #endregion Check scenario 

                #endregion Scenario 1

                #region Scenario 2 -- If choke size option update is checked, but there’s no valid well test record, or choke D factor is null in the selected valid well test - skip update => existing network model data should be used

                //deleting the welltest
                WellTestDataService.DeleteWellTestData(welltestdata.Values[0].Id.ToString());

                #region Create SNSscenario2 and run 
                SNScenarioDTO addScenarioObj2 = new SNScenarioDTO()
                {
                    Name = "SNScenario2",
                    Description = "SNScenario2 Description",
                    SNModelId = model.Id,
                    ColorCode = "#FF0000",
                    AllowableScenario = true,
                    Implementable = false,
                    ModelMatch = false,
                    OptimumScenario = false,
                    IsForecastScenario = true
                };
                SurfaceNetworkService.AddSNScenario(addScenarioObj2);
                allScenarios = SurfaceNetworkService.GetSNScenariosBySNModel(model.Id.ToString());
                SNScenarioDTO scenario2 = allScenarios?.FirstOrDefault(sn => sn.Name.Equals("SNScenario2"));
                Assert.IsNotNull(scenario2);
                _snScenariosToRemove.Add(scenario2);
                //Get the block valve 
                SNScenarioSectionDefaultsDTO insertedScenarioDefaults2 = SurfaceNetworkService.GetSNScenarioSectionDefaultsBySNScenario(scenario2.Id.ToString());
                Assert.IsNotNull(insertedScenarioDefaults2);
                //Update the Choke
                insertedScenarioDefaults2.Choke = SNScenarioAction.Update;
                SurfaceNetworkService.SaveSNScenarioSectionDefaults(insertedScenarioDefaults2);
                SNScenarioSectionDefaultsDTO updatedScenarioDefaults2 = SurfaceNetworkService.GetSNScenarioSectionDefaultsBySNScenario(scenario2.Id.ToString());
                Assert.IsNotNull(updatedScenarioDefaults2);
                Assert.AreEqual(SNScenarioAction.Update, updatedScenarioDefaults2.Choke);
                //update the MaxThroat choke Property
                var chokeData2 = SurfaceNetworkService.GetScenarioSectionChokeData(scenario2.Id.ToString());
                var newChoke2 = new SNScenarioChokeDataDTO();
                foreach (var chokeItem in chokeData2.Values)
                {
                    if (chokeItem.Name == chokeName)
                    {
                        chokeItem.Type = ReOChokeType.MaxThroat;
                        chokeItem.DiameterAction = SNScenarioAction.Update;
                        chokeItem.DiameterInput = chokeDiameterReO;
                        chokeItem.TuningFactorAction = SNScenarioAction.Update;
                        chokeItem.TuningFactor = chokeTuningFactorReO;
                        newChoke2 = chokeItem;
                        break;
                    }
                }

                var ChokeDataDto2 = new SNScenarioChokeDataAndUnitsDTO(chokeData2.Units, newChoke2);
                SurfaceNetworkService.SaveScenarioSectionChokeData(ChokeDataDto2);

                SurfaceNetworkService.InsertSNScenarioScheduleManualRun(scenario2.Id.ToString());
                SurfaceNetworkService.RunScheduledOptimizationJobs();
                #endregion   Create SN scenario and run 

                #region Check scenario stats
                optRunHdrs = SurfaceNetworkService.GetSNOptimizationRunHeaderArrayByScenarioId(scenario2.Id.ToString());
                Assert.IsNotNull(optRunHdrs);

                chokeres = SurfaceNetworkService.GetSNOptimizationChokeResults(optRunHdrs[0].Id.ToString());
                Assert.IsNotNull(chokeres);
                Assert.IsFalse(chokeres.Values.Count() == 0);
                chokeTuningFactor = chokeres.Values[0].socChokeTuningFactor;
                Assert.AreEqual(Math.Round((double)chokeTuningFactor), Math.Round((double)chokeTuningFactorReO), "For Scenario 2 Expected Rate: " + Math.Round((double)chokeTuningFactor) + " Actual Rate: " + Math.Round((double)chokeTuningFactorReO));
                #endregion Check scenario stats

                #endregion Scenario2

                #region Scenario 3 - If choke size option update is unchecked, then skip update => Existing network model data should be used

                #region model file without Choke D Factor Option
                //Updating Model File With Choke D Factor
                ModelFileHeaderDTO headerdto = ModelFileService.GetCurrentModelHeader(well1.Id.ToString());
                headerdto.Options.OptionalUpdate = new long[] { 1, 2 };
                ModelFileService.SaveModelFileOptions(headerdto.Options);
                #endregion model file with Choke D Factor Option

                #region Inserting Well Test Data
                WellTestDTO testDataDTO3 = new WellTestDTO()
                {
                    WellId = well1.Id,
                    SPTCodeDescription = "AllocatableTest",
                    TestDuration = 24,
                    AverageTubingPressure = 1000,
                    AverageTubingTemperature = 80.3m,
                    GaugePressure = 5800,
                    Oil = 100,
                    Gas = 50,
                    Water = 35,
                    ChokeSize = 360,
                    FlowLinePressure = 800,
                    FlowLineTemperature = 67.3m,
                    SeparatorPressure = 500,
                    Comment = "Choke D Factor Calulations - 2nd welltest",
                };
                testDataDTO3.SampleDate = well1.CommissionDate.Value.AddDays(2).ToUniversalTime();
                WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(testUnits, testDataDTO3));
                #endregion Inserting Well Test Data

                #region Create SNscenario3 and run 
                SNScenarioDTO addScenarioObj3 = new SNScenarioDTO()
                {
                    Name = "SNScenario3",
                    Description = "SNScenario3 Description",
                    SNModelId = model.Id,
                    ColorCode = "#FF0000",
                    AllowableScenario = true,
                    Implementable = false,
                    ModelMatch = false,
                    OptimumScenario = false,
                    IsForecastScenario = true
                };
                SurfaceNetworkService.AddSNScenario(addScenarioObj3);
                allScenarios = SurfaceNetworkService.GetSNScenariosBySNModel(model.Id.ToString());
                SNScenarioDTO scenario3 = allScenarios?.FirstOrDefault(sn => sn.Name.Equals("SNScenario3"));
                Assert.IsNotNull(scenario3);
                _snScenariosToRemove.Add(scenario3);
                //Get the block valve 
                SNScenarioSectionDefaultsDTO insertedScenarioDefaults3 = SurfaceNetworkService.GetSNScenarioSectionDefaultsBySNScenario(scenario3.Id.ToString());
                Assert.IsNotNull(insertedScenarioDefaults3);
                //Update choke details
                insertedScenarioDefaults3.Choke = SNScenarioAction.Update;
                SurfaceNetworkService.SaveSNScenarioSectionDefaults(insertedScenarioDefaults3);
                SNScenarioSectionDefaultsDTO updatedScenarioDefaults3 = SurfaceNetworkService.GetSNScenarioSectionDefaultsBySNScenario(scenario3.Id.ToString());
                Assert.IsNotNull(updatedScenarioDefaults3);
                Assert.AreEqual(SNScenarioAction.Update, updatedScenarioDefaults3.Choke);
                //update MaxThroat choke property
                var chokeData3 = SurfaceNetworkService.GetScenarioSectionChokeData(scenario3.Id.ToString());
                var newChoke3 = new SNScenarioChokeDataDTO();
                foreach (var chokeItem in chokeData3.Values)
                {
                    if (chokeItem.Name == chokeName)
                    {
                        chokeItem.Type = ReOChokeType.MaxThroat;
                        chokeItem.DiameterAction = SNScenarioAction.Update;
                        chokeItem.DiameterInput = chokeDiameterReO;
                        chokeItem.TuningFactorAction = SNScenarioAction.Update;
                        chokeItem.TuningFactor = chokeTuningFactorReO;
                        newChoke2 = chokeItem;
                        break;
                    }
                }
                var ChokeDataDto3 = new SNScenarioChokeDataAndUnitsDTO(chokeData3.Units, newChoke2);
                SurfaceNetworkService.SaveScenarioSectionChokeData(ChokeDataDto3);
                SurfaceNetworkService.InsertSNScenarioScheduleManualRun(scenario3.Id.ToString());
                SurfaceNetworkService.RunScheduledOptimizationJobs();
                #endregion   Create SNscenario3 and run 

                #region Check scenario stats
                optRunHdrs = SurfaceNetworkService.GetSNOptimizationRunHeaderArrayByScenarioId(scenario3.Id.ToString());
                Assert.IsNotNull(optRunHdrs);

                chokeres = SurfaceNetworkService.GetSNOptimizationChokeResults(optRunHdrs[0].Id.ToString());
                Assert.IsNotNull(chokeres);
                Assert.IsFalse(chokeres.Values.Count() == 0);
                chokeTuningFactor = chokeres.Values[0].socChokeTuningFactor;
                Assert.AreEqual(Math.Round((double)chokeTuningFactor), Math.Round((double)chokeTuningFactorReO), "For Scenario 2 Expected Rate: " + Math.Round((double)chokeTuningFactor) + " Actual Rate: " + Math.Round((double)chokeTuningFactorReO));
                #endregion Check scenario stats

                #endregion Scenario 3

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }



        private AssetDTO AddAsset(string assetName)
        {
            SurfaceNetworkService.AddAsset(new AssetDTO() { Name = assetName, Description = "Test Description" });
            var allAssets = SurfaceNetworkService.GetAllAssets().ToList();
            AssetDTO asset = allAssets?.FirstOrDefault(a => a.Name.Equals(assetName));
            _assetsToRemove.Add(asset);
            return asset;
        }

        //[TestCategory(TestCategories.SurfaceNetworkServiceTests), TestMethod]
        //public void GetSNModelsByActiveAssetTest()
        //{
        //    string assetName = "Asset Test";
        //    string model1_Name = "SNModel01 Test";
        //    string model2_Name = "SNModel02 Test";

        //    //make sure there is no assets in the system before adding a new one
        //    var existingAssets = SurfaceNetworkService.GetAllAssets().ToList();

        //    if (existingAssets.Any())
        //    {
        //        foreach (var item in existingAssets)
        //        {
        //            SurfaceNetworkService.RemoveAsset(item.Id.ToString());
        //        }
        //    }

        //    var allAssets = SurfaceNetworkService.GetAllAssets().ToList();
        //    Assert.IsFalse(allAssets.Any());

        //    SurfaceNetworkService.AddAsset(new AssetDTO() { Name = assetName, Description = "Test Description" });
        //    allAssets = SurfaceNetworkService.GetAllAssets().ToList();

        //    AssetDTO asset = allAssets.FirstOrDefault(a => a.Name.Equals(assetName));
        //    Assert.IsNotNull(asset);
        //    _assetsToRemove.Add(asset);

        //    SurfaceNetworkService.AddSNModel(new SNModelDTO() { Name = model1_Name, AssetId = asset.Id });
        //    SurfaceNetworkService.AddSNModel(new SNModelDTO() { Name = model2_Name, AssetId = asset.Id });
        //    var allModels = SurfaceNetworkService.GetSNModelsByActiveAsset();

        //    Assert.AreEqual(allModels.Length, 2);

        //    SurfaceNetworkService.RemoveSNModels(new[] { allModels.First().Id.ToString(), allModels.Last().Id.ToString() });

        //    allModels = SurfaceNetworkService.GetSNModelsByActiveAsset();

        //    Assert.IsFalse(allModels.Any());

        //    //adding the previous existing assets back
        //    if (existingAssets.Any())
        //    {
        //        foreach (var item in existingAssets)
        //        {
        //            SurfaceNetworkService.AddAsset(item);
        //        }
        //    }
        //}

        [TestCategory(TestCategories.SurfaceNetworkServiceTests), TestMethod]

        public void UpdateSNModelFileTest()
        {
            string path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";
            string ReOmodel = "22Wells.reo";
            string assetName = "Asset Test";
            string modelName = "Model Test";

            SurfaceNetworkService.AddAsset(new AssetDTO() { Name = assetName, Description = "Test Description" });
            var allAssets = SurfaceNetworkService.GetAllAssets().ToList();
            AssetDTO asset = allAssets?.FirstOrDefault(a => a.Name.Equals(assetName));
            Assert.IsNotNull(asset);
            _assetsToRemove.Add(asset);

            // add in the model file to use for scenario test calls
            SNModelDTO model = AddSurfaceNetworkModelAndModelFile(asset, path, ReOmodel, modelName);
            Assert.IsNotNull(model);

            var modelFile = SurfaceNetworkService.GetSNModelFilesByModelId(model.Id.ToString()).FirstOrDefault();
            Assert.IsNotNull(modelFile);

            modelFile.Comments = "this is a simple test";
            SurfaceNetworkService.UpdateSNModelFile(modelFile);

            var updatedModelFile = SurfaceNetworkService.GetSNModelFilesByModelId(model.Id.ToString()).FirstOrDefault();
            Assert.IsNotNull(updatedModelFile);

            Assert.AreEqual(updatedModelFile.Comments, "this is a simple test");
        }

        private SNModelDTO AddSurfaceNetworkModelAndModelFile(AssetDTO asset, string path, string ReOmodel, string modelName)
        {
            // add in the model file to use for scenario test calls
            SNModelAndModelFileDTO modelAndFile = new SNModelAndModelFileDTO();

            SNModelFileDTO modelFile = new SNModelFileDTO();
            modelFile.Comments = "Test Model File;";
            modelFile.UpdateFluidModel = true;
            byte[] fileAsByteArray = GetByteArray(path, ReOmodel);
            modelFile.Base64Contents = Convert.ToBase64String(fileAsByteArray);
            modelAndFile.SNModelFile = modelFile;

            SurfaceNetworkService.AddSNModel(new SNModelDTO() { Name = modelName, AssetId = asset.Id, AssetName = asset.Name });
            long[] userAssets = new long[1]; userAssets[0] = asset.Id;
            var allModels = SurfaceNetworkService.GetSNModelsByAssetIds(userAssets);
            SNModelDTO model = allModels?.FirstOrDefault(m => m.Name.Equals(modelName));
            model.AssetId = asset.Id;
            // Upload the model file to get things configured
            modelAndFile.SNModel = model;
            SurfaceNetworkService.SaveReOFile(modelAndFile);

            return model;
        }

        private SNScenarioDTO AddScenario(SNModelDTO model, string scenarioName)
        {
            SurfaceNetworkService.AddSNScenario(new SNScenarioDTO() { Name = scenarioName, Description = "Test Description", SNModelId = model.Id, ColorCode = "#FF0000", AllowableScenario = true, Implementable = false, ModelMatch = false, OptimumScenario = false });
            var allScenarios = SurfaceNetworkService.GetSNScenariosBySNModel(model.Id.ToString());
            SNScenarioDTO scenario = allScenarios?.FirstOrDefault(sn => sn.Name.Equals(scenarioName));

            return scenario;
        }

        private string SetUpDirectory(long directoryName, string path)
        {
            path = string.Format(path + @"\{0}", directoryName);

            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            catch (UnauthorizedAccessException)
            {
                throw new DirectoryNotFoundException();
            };

            return path;
        }

        private void DeleteDirectory(string path)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
            }
            catch (UnauthorizedAccessException)
            {
                throw new DirectoryNotFoundException();
            };
        }

        private string WriteReOFileToServer(SNModelFileDTO reoModelFile, string path)
        {
            string filePath = string.Format(path + @"\{0}.reo", reoModelFile.Id);

            Stream sourceStream = new MemoryStream(Convert.FromBase64String(reoModelFile.Base64Contents));
            FileStream targetStream = null;
            using (targetStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                byte[] buffer = new byte[sourceStream.Length];
                sourceStream.Read(buffer, 0, (int)sourceStream.Length);
                targetStream.Write(buffer, 0, buffer.Length);

                targetStream.Close();
                sourceStream.Close();
            }

            return filePath;
        }

        /// <summary>
        /// Tester              : Pravin D. Survase
        /// Description         : Add/Delete any RRL and Non-RRL Well in ForeSite by using this Generic method.
        /// </summary>
        public void AddDeleteNewWells(string wellName, WellTypeId wellTypeId)
        {
            if (wellName != null)
            {
                WellConfigurationService.AddWellConfig(new WellConfigDTO()
                {
                    Well = SetDefaultFluidType(new WellDTO()
                    {
                        Name = wellName,
                        FacilityId = wellName,
                        DataConnection = GetDefaultCygNetDataConnection(),
                        IntervalAPI = "IntervalAPI_" + wellName,
                        SubAssemblyAPI = "SubAssemblyAPI_" + wellName,
                        AssemblyAPI = "AssemblyAPI_" + wellName,
                        CommissionDate = DateTime.Today,
                        WellType = wellTypeId,
                    })
                });
                var allWells = WellService.GetAllWells().ToList();
                WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals(wellName));
                Assert.IsNotNull(well);
                _wellsToRemove.Add(well);
            }
        }

        public void AddDeleteNewWells(string wellName, WellTypeId wellTypeId, long assetid)
        {
            if (wellName != null)
            {
                WellConfigurationService.AddWellConfig(new WellConfigDTO()
                {
                    Well = SetDefaultFluidType(new WellDTO()
                    {
                        Name = wellName,
                        FacilityId = wellName,
                        DataConnection = GetDefaultCygNetDataConnection(),
                        IntervalAPI = "IntervalAPI_" + wellName,
                        SubAssemblyAPI = "SubAssemblyAPI_" + wellName,
                        AssemblyAPI = "AssemblyAPI_" + wellName,
                        CommissionDate = DateTime.Today,
                        WellType = wellTypeId,
                        AssetId = assetid,
                    })
                });
                var allWells = WellService.GetAllWells().ToList();
                WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals(wellName));
                Assert.IsNotNull(well);
                _wellsToRemove.Add(well);
            }
        }

        /// <summary>
        /// Tester              : Pravin D. Survase
        /// Description         : Checking well is mapped in surface network or not.
        /// </summary>
        public void IsWellMappedInSurfaceNetwork(string wellName)
        {
            string assetName = "Asset Test";
            string path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";
            string modelName = "Model Test";
            string ReOmodel = "22Wells.reo";
            long[] userAssets = new long[1];
            ////Delete new created Asset if this test fails at 1st time.
            //var allAssets2 = SurfaceNetworkService.GetAllAssets().ToList();
            //AssetDTO asset2 = allAssets2?.FirstOrDefault(a => a.Name.Equals(assetName));
            //SurfaceNetworkService.RemoveAsset(asset2.Id.ToString());

            if (wellName != null)
            {
                try
                {
                    //Check Well mapped with SNModel
                    var allWells = WellService.GetAllWells().ToList();
                    WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals(wellName));
                    SNWellMappingDTO wellMapping = SurfaceNetworkService.GetSNWellMappingByWellId(well.Id.ToString());
                    Assert.AreEqual(null, wellMapping, "This well is mapped in Network Configuration.");

                    //Check any Asset exist in System
                    var getAssets = SurfaceNetworkService.GetAllAssets().ToList();
                    UserDTO user = AdministrationService.GetUser(AuthenticatedUser.Id.ToString());
                    AssetDTO getAsset = getAssets?.FirstOrDefault(a => a.Name.Equals(assetName));
                    if (getAsset == null)
                    {
                        //Configure new Asset
                        SurfaceNetworkService.AddAsset(new AssetDTO()
                        {
                            Name = assetName,
                            Description = "Test Description"
                        });

                        //Get all Assets to see newly added Asset in System
                        var allAssets = SurfaceNetworkService.GetAllAssets().ToList();
                        AssetDTO asset = allAssets?.FirstOrDefault(a => a.Name.Equals(assetName));
                        Assert.IsNotNull(asset);
                        user.Assets.Add(asset);
                        AdministrationService.UpdateUser(user);


                        //Configure new sn model and model file
                        var model = AddSurfaceNetworkModelAndModelFile(asset, path, ReOmodel, modelName);
                        Assert.IsNotNull(model);

                    }

                    var all_Assets = SurfaceNetworkService.GetAllAssets().ToList();
                    AssetDTO assets = all_Assets?.FirstOrDefault(a => a.Name.Equals(assetName));
                    _assetsToRemove = SurfaceNetworkService.GetAllAssets().ToList();
                    userAssets[0] = assets.Id;
                    //Get all SNWellNames from SNModel
                    SNWellMappingDTO[] mappings = SurfaceNetworkService.GetSNWellMappingsByAssetIds(userAssets);
                    Assert.IsNotNull(mappings);

                    // Map well with SNWellNames in SNModel
                    SNWellMappingDTO initialWellMapping = mappings[0];
                    initialWellMapping.WellId = well.Id;
                    SurfaceNetworkService.UpdateSNWellMapping(initialWellMapping);

                    //Check Well is mapped with SNWellName in SN Model
                    SNWellMappingDTO updatedWellMapping = SurfaceNetworkService.GetSNWellMappingByWellId(well.Id.ToString());
                    Assert.IsNotNull(updatedWellMapping);
                    WellDTO mappedWell = allWells?.FirstOrDefault(w => w.Name.Equals(wellName));
                    Assert.AreEqual(mappedWell.Id, updatedWellMapping.Well.Id, "Mismach between the well id");
                    Assert.AreEqual(mappedWell.Name, updatedWellMapping.Well.Name, "Mismach between the well name");
                    Assert.AreEqual(mappedWell.WellType, updatedWellMapping.Well.WellType, "Mismatch between the Well Type");

                    //Undo Mapping so we can delete the well
                    SNWellMappingDTO undoWellMapping = mappings[0];
                    undoWellMapping.WellId = null;
                    SurfaceNetworkService.UpdateSNWellMapping(undoWellMapping);

                    //Checking is it well unmapped from SN Model
                    SNWellMappingDTO unMappedWithSNModel = SurfaceNetworkService.GetSNWellMappingByWellId(well.Id.ToString());
                    Assert.IsNull(unMappedWithSNModel, "This well is mapped in Network Configuration.");
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
        }

        /// <summary>
        /// Tester              : Pravin D. Survase
        /// Description         : Add RRL and Non-RRL Well in ForeSite. Test that well is configured in Netork Scenario page or not
        /// Base Jira Ticket    : FRWM-1099 [Refactor Well Configuration/ Workflow Settings]
        /// Auto Test Ticket    : FRWM-1446 [API Testing]
        /// </summary>
        [TestCategory(TestCategories.SurfaceNetworkServiceTests), TestMethod]

        public void AddRemoveAllWellTypes()
        {
            try
            {
                // RRL Well
                AddDeleteNewWells("RRLWELL_TEST", WellTypeId.RRL);
                IsWellMappedInSurfaceNetwork("RRLWELL_TEST");
                //NF well
                AddDeleteNewWells("NFWELL_TEST", WellTypeId.NF);
                IsWellMappedInSurfaceNetwork("NFWELL_TEST");
                //ESP Well
                AddDeleteNewWells("ESPWELL_TEST", WellTypeId.ESP);
                IsWellMappedInSurfaceNetwork("ESPWELL_TEST");
                //Gas Lift Well
                AddDeleteNewWells("GASLIFTWELL_TEST", WellTypeId.GLift);
                IsWellMappedInSurfaceNetwork("GASLIFTWELL_TEST");
                //Water Injection Well
                AddDeleteNewWells("WIWELL_TEST", WellTypeId.WInj);
                IsWellMappedInSurfaceNetwork("WIWELL_TEST");
                //Gas Injection Well
                AddDeleteNewWells("GIWELL_TEST", WellTypeId.GInj);
                IsWellMappedInSurfaceNetwork("GIWELL_TEST");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [TestCategory(TestCategories.SurfaceNetworkServiceTests), TestMethod]
        public void AddSNModelFileImage()
        {
            string assetName = "Asset Test Image1";
            string path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";
            string ReOmodel = "22Wells.reo";
            string modelName = "Model Test";
            string scenarioName = "Scenario Test";

            //Configure new Asset
            var asset = AddAsset(assetName);
            Assert.IsNotNull(asset);

            //Configure new sn model and model file
            var model = AddSurfaceNetworkModelAndModelFile(asset, path, ReOmodel, modelName);
            Assert.IsNotNull(model);

            //Configure new scenario
            var scenario = AddScenario(model, scenarioName);
            Assert.IsNotNull(scenario);

            SNModelAndModelFileDTO testModel = SurfaceNetworkService.GetSNModelAndFile(scenario.SNModel.Id.ToString());
            Assert.IsNotNull(testModel.SNModel);
            Assert.IsNotNull(testModel.SNModel.AssetId);
            Assert.IsNotNull(testModel.SNModel.AssetName);
            SNModelImageInfoDTO[] testdata = SurfaceNetworkService.GetSNModelImageFiles(scenario.SNModel.Id.ToString());
            Assert.IsNotNull(testdata);

            //Remove ModelFile
            SurfaceNetworkService.RemoveSNModelFile(model.Id.ToString());
        }

        [TestCategory(TestCategories.SurfaceNetworkServiceTests), TestMethod]
        public void AddSNModelFileImageTreeView()
        {
            string assetName = "Asset Test Network";
            string path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";
            string ReOmodel = "22Wells.reo";
            string modelName = "Model Test Network";
            string scenarioName = "Scenario Test Network";
            string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string sTargetFolderPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\TestDocuments\\Images";

            //Configure new Asset
            var asset = AddAsset(assetName);
            Assert.IsNotNull(asset);

            //Configure new sn model and model file
            var model = AddSurfaceNetworkModelAndModelFile(asset, path, ReOmodel, modelName);
            Assert.IsNotNull(model);
            Trace.WriteLine("New Reo model file is created");

            //Configure new scenario
            var scenario = AddScenario(model, scenarioName);
            Assert.IsNotNull(scenario);
            Trace.WriteLine("New scenario is created");
            string filePath = null;
            int fileCount = 0;
            long image = 0;

            Trace.WriteLine("Getting files from path ");
            if (Directory.GetFiles(sTargetFolderPath, "*.xml").Length > 0)
            {

                string[] xmlFiles = Directory.GetFiles(sTargetFolderPath, "*.xml");
                string[] imageFiles = Directory.GetFiles(sTargetFolderPath, "*.jpg");
                string[] htmlFiles = Directory.GetFiles(sTargetFolderPath, "*.html");
                FileStream imageStream = null;
                fileCount = xmlFiles.Length;
                for (int i = 0; i < fileCount; i++)
                {
                    SNModelImageInfoDTO imageDTO = new SNModelImageInfoDTO();
                    imageDTO.SNModelId = scenario.SNModelId;
                    imageDTO.SNModelBaseFileName = Path.GetFileNameWithoutExtension(xmlFiles[i]);
                    // save image file 
                    filePath = imageFiles[i];
                    using (imageStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        byte[] buffer = new byte[imageStream.Length];
                        imageStream.Read(buffer, 0, (int)imageStream.Length);
                        imageDTO.SNModelImageBinary = Convert.ToBase64String(buffer);
                        imageStream.Close();
                    }
                    // save XML file 
                    filePath = xmlFiles[i];
                    using (imageStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        byte[] buffer = new byte[imageStream.Length];
                        imageStream.Read(buffer, 0, (int)imageStream.Length);
                        imageDTO.SNModelXMLBinary = buffer;
                        imageStream.Close();
                    }
                    // save image file 
                    filePath = htmlFiles[i];
                    using (imageStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        byte[] buffer = new byte[imageStream.Length];
                        imageStream.Read(buffer, 0, (int)imageStream.Length);
                        imageDTO.SNModelHTMLBinary = Convert.ToBase64String(buffer);
                        imageStream.Close();
                    }
                    using (System.IO.StreamReader htmlReader = new System.IO.StreamReader(filePath))
                    {
                        System.Text.StringBuilder htmlContent = new System.Text.StringBuilder();
                        string line;
                        while ((line = htmlReader.ReadLine()) != null)
                        {
                            htmlContent.Append(line);
                        }

                        var regex = new Regex("<a [^>]*href=(?:'(?<href>.*?)')|(?:\"(?<href>.*?)\")", RegexOptions.IgnoreCase);
                        var sheetList = regex.Matches(htmlContent.ToString()).OfType<Match>().Select(m => m.Groups["href"].Value.Replace(".html", "")).ToList();

                        if (imageDTO.SNModelBaseFileName != "TopView" && sheetList.Count > 0 && sheetList.Any(x => x == imageDTO.SNModelBaseFileName))
                        {
                            for (int j = 0; j < sheetList.Count; j++)
                            {
                                if (sheetList[j] == imageDTO.SNModelBaseFileName)
                                {
                                    imageDTO.SNParentFileName = sheetList[j - 1];
                                    break;
                                }
                            }
                        }

                        htmlReader.Close();
                    }
                    image = SurfaceNetworkService.AddSNModelImageFiles(imageDTO);
                    Assert.IsNotNull(image);
                    SNModelImageInfoDTO[] testdata = SurfaceNetworkService.GetSNModelImageFiles(scenario.SNModel.Id.ToString());
                    Assert.IsNotNull(testdata);
                    Assert.IsNotNull(testdata.First().SNModelId);
                    Assert.IsNotNull(testdata.First().SNModelBaseFileName);
                    List<SNImageNodeDTO> data = SurfaceNetworkService.GetSNModelImagesTreeView(imageDTO.SNModelId.ToString());
                    Assert.IsNotNull(data);
                    SurfaceNetworkService.DeleteSNModelImageFiles(imageDTO.SNModelId.ToString());

                }
            }
            SurfaceNetworkService.RemoveSNModelFile(model.Id.ToString());
        }

        /// <summary>
        /// Description         : Network: Well Performance Update (ADNOC Gas Asset)
        /// Base Jira Ticket    : FRWM-6798
        /// Auto Test Ticket    : FRWM-6851 [API Testing] For Creating Valid Well Test data only since Reo Model 
        ///                        data verfication was not posible
        ///                        Unit System Used : US English units.
        /// </summary>
        [TestCategory(TestCategories.SurfaceNetworkServiceTests), TestMethod]

        public void VerifyWellPerformanceCurveUpdateforSNModel()
        {
            string path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";
            string model = "RecycleNetwork.reo";
            string modelName = "Recycle Network";
            string assetName = "ADNOC Gas Asset";
            try
            {
                // step 1: Create Asset
                #region 1. Add Asset

                AssetDTO asset = AddAsset(assetName);
                UserDTO user = AdministrationService.GetUser(AuthenticatedUser.Id.ToString());
                user.Assets.Add(asset);
                AdministrationService.UpdateUser(user);
                #endregion
                // step2 : Create 6 Wells 4 NF Condensate and 2 Gas Injection Wells
                #region 2. Add Wells and Update Welltests
                AddADNOCWells();

                //Update Well test for GP1 to be tuned success;
                WellDTO GP1well = WellService.GetAllWells().FirstOrDefault(w => w.Name == "GP1");
                AddWellSettingWithDoubleValues(GP1well.Id, SettingServiceStringConstants.WATER_GAS_RATIO_MIN_AL, 0.6);
                AddWellSettingWithDoubleValues(GP1well.Id, SettingServiceStringConstants.WATER_GAS_RATIO_MAX_AL, 10);
                AddWellSettingWithDoubleValues(GP1well.Id, SettingServiceStringConstants.CONDENSATE_GAS_RATIO_MIN_AL, 30);
                AddWellSettingWithDoubleValues(GP1well.Id, SettingServiceStringConstants.CONDENSATE_GAS_RATIO_MAX_AL, 300);
                var welltestdata = WellTestDataService.GetWellTestDataByWellId(GP1well.Id.ToString());
                WellTestDTO latestwelltest = welltestdata.Values.First();
                //   WellTestDTO latestwelltest = WellTestDataService.GetLatestWellTestDataByWellId(GP1well.Id.ToString());
                latestwelltest.Oil = 1906;
                latestwelltest.Water = 36;
                latestwelltest.Gas = 49276;
                latestwelltest.AverageTubingPressure = 1566;
                WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(GP1well.Id.ToString()).Units;
                WellTestDataService.UpdateWellTestData(new WellTestAndUnitsDTO(units, latestwelltest));
                bool tunewell = WellTestDataService.TuneWellTests(GP1well.Id.ToString());
                Assert.IsTrue(tunewell);
                welltestdata = WellTestDataService.GetWellTestDataByWellId(GP1well.Id.ToString());
                latestwelltest = welltestdata.Values.First();
                Assert.AreEqual("Success", latestwelltest.TuningStatus, "Tuning Status was not Correct GP1 ");

                WellDTO GP2well = WellService.GetAllWells().FirstOrDefault(w => w.Name == "GP2");
                AddWellSettingWithDoubleValues(GP2well.Id, SettingServiceStringConstants.WATER_GAS_RATIO_MIN_AL, 0.4);
                AddWellSettingWithDoubleValues(GP2well.Id, SettingServiceStringConstants.WATER_GAS_RATIO_MAX_AL, 10);
                AddWellSettingWithDoubleValues(GP2well.Id, SettingServiceStringConstants.CONDENSATE_GAS_RATIO_MIN_AL, 15);
                AddWellSettingWithDoubleValues(GP2well.Id, SettingServiceStringConstants.CONDENSATE_GAS_RATIO_MAX_AL, 300);
                AddWellSettingWithDoubleValues(GP2well.Id, SettingServiceStringConstants.LFACTOR_MIN_AL, 0.88);
                AddWellSettingWithDoubleValues(GP2well.Id, SettingServiceStringConstants.LFACTOR_MAX_AL, 1.28);
                var welltestdata2 = WellTestDataService.GetWellTestDataByWellId(GP2well.Id.ToString());
                WellTestDTO latestwelltest2 = welltestdata2.Values.First();
                //   WellTestDTO latestwelltest = WellTestDataService.GetLatestWellTestDataByWellId(GP1well.Id.ToString());
                latestwelltest2.Oil = 1020;
                latestwelltest2.Water = 32;
                latestwelltest2.Gas = 61650;
                latestwelltest2.AverageTubingPressure = 2920;
                WellTestUnitsDTO units2 = WellTestDataService.GetWellTestDefaults(GP2well.Id.ToString()).Units;
                WellTestDataService.UpdateWellTestData(new WellTestAndUnitsDTO(units2, latestwelltest2));
                bool tunewell2 = WellTestDataService.TuneWellTests(GP2well.Id.ToString());
                Assert.IsTrue(tunewell2);
                welltestdata2 = WellTestDataService.GetWellTestDataByWellId(GP2well.Id.ToString());
                latestwelltest2 = welltestdata2.Values.First();
                Assert.AreEqual("Success", latestwelltest2.TuningStatus, "Tuning Status was not Correct GP2 ");

                WellDTO GP3well = WellService.GetAllWells().FirstOrDefault(w => w.Name == "GP3");
                AddWellSettingWithDoubleValues(GP3well.Id, SettingServiceStringConstants.WATER_GAS_RATIO_MIN_AL, 0.4);
                AddWellSettingWithDoubleValues(GP3well.Id, SettingServiceStringConstants.WATER_GAS_RATIO_MAX_AL, 10);
                AddWellSettingWithDoubleValues(GP3well.Id, SettingServiceStringConstants.CONDENSATE_GAS_RATIO_MIN_AL, 15);
                AddWellSettingWithDoubleValues(GP3well.Id, SettingServiceStringConstants.CONDENSATE_GAS_RATIO_MAX_AL, 300);
                AddWellSettingWithDoubleValues(GP3well.Id, SettingServiceStringConstants.LFACTOR_MIN_AL, 0.88);
                AddWellSettingWithDoubleValues(GP3well.Id, SettingServiceStringConstants.LFACTOR_MAX_AL, 1.28);
                var welltestdata3 = WellTestDataService.GetWellTestDataByWellId(GP3well.Id.ToString());
                WellTestDTO latestwelltest3 = welltestdata3.Values.First();
                //   WellTestDTO latestwelltest = WellTestDataService.GetLatestWellTestDataByWellId(GP1well.Id.ToString());
                latestwelltest3.Oil = 1166;
                latestwelltest3.Water = 38;
                latestwelltest3.Gas = 44350;
                latestwelltest3.AverageTubingPressure = 1559;
                WellTestUnitsDTO units3 = WellTestDataService.GetWellTestDefaults(GP3well.Id.ToString()).Units;
                WellTestDataService.UpdateWellTestData(new WellTestAndUnitsDTO(units3, latestwelltest3));
                bool tunewell3 = WellTestDataService.TuneWellTests(GP3well.Id.ToString());
                Assert.IsTrue(tunewell3);
                welltestdata3 = WellTestDataService.GetWellTestDataByWellId(GP3well.Id.ToString());
                latestwelltest3 = welltestdata3.Values.First();
                Assert.AreEqual("Success", latestwelltest3.TuningStatus, "Tuning Status was not Correct GP3 ");

                WellDTO GP4well = WellService.GetAllWells().FirstOrDefault(w => w.Name == "GP4");
                AddWellSettingWithDoubleValues(GP4well.Id, SettingServiceStringConstants.WATER_GAS_RATIO_MIN_AL, 0.4);
                AddWellSettingWithDoubleValues(GP4well.Id, SettingServiceStringConstants.WATER_GAS_RATIO_MAX_AL, 10);
                AddWellSettingWithDoubleValues(GP4well.Id, SettingServiceStringConstants.CONDENSATE_GAS_RATIO_MIN_AL, 15);
                AddWellSettingWithDoubleValues(GP4well.Id, SettingServiceStringConstants.CONDENSATE_GAS_RATIO_MAX_AL, 300);
                AddWellSettingWithDoubleValues(GP4well.Id, SettingServiceStringConstants.LFACTOR_MIN_AL, 0.88);
                AddWellSettingWithDoubleValues(GP4well.Id, SettingServiceStringConstants.LFACTOR_MAX_AL, 1.28);
                var welltestdata4 = WellTestDataService.GetWellTestDataByWellId(GP4well.Id.ToString());
                WellTestDTO latestwelltest4 = welltestdata4.Values.First();
                latestwelltest4.Oil = 680;
                latestwelltest4.Water = 20;
                latestwelltest4.Gas = 19780;
                latestwelltest4.AverageTubingPressure = 1800;
                WellTestUnitsDTO units4 = WellTestDataService.GetWellTestDefaults(GP4well.Id.ToString()).Units;
                WellTestDataService.UpdateWellTestData(new WellTestAndUnitsDTO(units4, latestwelltest4));
                bool tunewell4 = WellTestDataService.TuneWellTests(GP4well.Id.ToString());
                Assert.IsTrue(tunewell4);
                welltestdata4 = WellTestDataService.GetWellTestDataByWellId(GP4well.Id.ToString());
                latestwelltest4 = welltestdata4.Values.First();
                Assert.AreEqual("Success", latestwelltest4.TuningStatus, "Tuning Status was not Correct GP4");

                WellDTO GI1well = WellService.GetAllWells().FirstOrDefault(w => w.Name == "GI1");
                AddWellSettingWithDoubleValues(GI1well.Id, SettingServiceStringConstants.LFACTOR_MIN_AL, 0.51);
                AddWellSettingWithDoubleValues(GI1well.Id, SettingServiceStringConstants.LFACTOR_MAX_AL, 1.28);
                var welltestdata5 = WellTestDataService.GetWellTestDataByWellId(GI1well.Id.ToString());
                WellTestDTO latestwelltest5 = welltestdata5.Values.First();

                latestwelltest5.Oil = 0;
                latestwelltest5.Water = 0;
                latestwelltest5.Gas = 32400;
                latestwelltest5.AverageTubingPressure = 4277;
                latestwelltest5.AverageTubingTemperature = 0;
                latestwelltest5.FlowLinePressure = 1366;
                WellTestUnitsDTO units5 = WellTestDataService.GetWellTestDefaults(GI1well.Id.ToString()).Units;
                WellTestDataService.UpdateWellTestData(new WellTestAndUnitsDTO(units5, latestwelltest5));
                bool tunewell5 = WellTestDataService.TuneWellTests(GI1well.Id.ToString());
                Assert.IsTrue(tunewell5);
                welltestdata5 = WellTestDataService.GetWellTestDataByWellId(GI1well.Id.ToString());
                latestwelltest5 = welltestdata5.Values.First();
                Assert.AreEqual("Success", latestwelltest5.TuningStatus, "Tuning Status was not Correct GI1");

                WellDTO GI2well = WellService.GetAllWells().FirstOrDefault(w => w.Name == "GI2");
                AddWellSettingWithDoubleValues(GI2well.Id, SettingServiceStringConstants.LFACTOR_MIN_AL, 0.51);
                AddWellSettingWithDoubleValues(GI2well.Id, SettingServiceStringConstants.LFACTOR_MAX_AL, 1.28);
                var welltestdata6 = WellTestDataService.GetWellTestDataByWellId(GI2well.Id.ToString());
                WellTestDTO latestwelltest6 = welltestdata6.Values.First();
                latestwelltest6.Oil = 0;
                latestwelltest6.Water = 0;
                latestwelltest6.Gas = 46180;
                latestwelltest6.AverageTubingPressure = 4290;
                latestwelltest6.AverageTubingTemperature = 0;
                latestwelltest6.FlowLinePressure = 100;
                WellTestUnitsDTO units6 = WellTestDataService.GetWellTestDefaults(GI2well.Id.ToString()).Units;
                WellTestDataService.UpdateWellTestData(new WellTestAndUnitsDTO(units6, latestwelltest6));
                bool tunewell6 = WellTestDataService.TuneWellTests(GI2well.Id.ToString());
                Assert.IsTrue(tunewell6);
                welltestdata6 = WellTestDataService.GetWellTestDataByWellId(GI2well.Id.ToString());
                latestwelltest6 = welltestdata6.Values.First();
                Assert.AreEqual("Success", latestwelltest6.TuningStatus, "Tuning Status was not Correct GI2 ");

                #endregion

                #region 3. Add SurfaceNetwork Model
                //step3: Add SN model which has same well models attached to ForeSite wells 
                SNModelAndModelFileDTO modelAndFile = new SNModelAndModelFileDTO();
                modelAndFile.SNModel = new SNModelDTO() { Name = modelName, AssetId = asset.Id };

                SNModelFileDTO modelFile = new SNModelFileDTO();
                modelFile.Comments = "ReCycle Model File;";
                modelFile.UpdateFluidModel = true;

                byte[] fileAsByteArray = GetByteArray(path, model);
                Assert.IsNotNull(fileAsByteArray);
                modelFile.Base64Contents = Convert.ToBase64String(fileAsByteArray);
                modelAndFile.SNModelFile = modelFile;

                //Upload Surface Network File
                // SurfaceNetworkService.AddSNModelAndFile(modelAndFile);
                SurfaceNetworkService.SaveReOFile(modelAndFile);
                #endregion

                #region 4. Map Reo and ForeSite Wells 
                long[] userAssets = new long[1]; userAssets[0] = asset.Id;
                SNWellMappingDTO[] mappings = SurfaceNetworkService.GetSNWellMappingsByAssetIds(userAssets);
                Assert.IsNotNull(mappings);
                WellDTO[] wells = WellService.GetWellsByUserAssetIds(new long[] { asset.Id });
                SNWellMappingDTO[] wellmappings = SurfaceNetworkService.GetSNWellMappingsByAssetIds(new long[] { asset.Id });

                SNModelDTO[] models = SurfaceNetworkService.GetSNModelsByAsset(asset.Id.ToString());
                foreach (SNWellMappingDTO wellToMap in wellmappings)
                {
                    foreach (WellDTO well in wells)
                    {
                        if (well.Name.ToLower().Equals(wellToMap.SNWellName.ToLower()))
                        {
                            wellToMap.Well = well;
                            wellToMap.WellId = well.Id;
                        }
                    }
                }
                //Create Surface Network Scenario for Test;
                SurfaceNetworkService.UpdateSNWellMappings(wellmappings);
                Trace.WriteLine("Well Mapping is updated for RecycleNetwork");
                #endregion

                #region 5. Create SN Scenario 
                SNScenarioDTO scenario1 = new SNScenarioDTO();
                scenario1.Name = "Adnocsc1";
                scenario1.Description = "Adnocsc1";
                scenario1.DoDailyRun = false;
                scenario1.IsForecastScenario = false;
                scenario1.SNModelId = models.ElementAt(0).Id;
                SurfaceNetworkService.AddSNScenario(scenario1);
                #endregion

                #region 6. Update Scenario Configuration
                SNScenarioDTO[] savedscenario = SurfaceNetworkService.GetSNScenariosByAssetIds(new long[] { asset.Id });
                SNScenarioSectionDefaultsDTO scconfig = SurfaceNetworkService.GetSNScenarioSectionDefaultsBySNScenario(savedscenario.ElementAt(0).Id.ToString());

                scconfig.BlockValve = SNScenarioAction.Ignore;
                scconfig.Choke = SNScenarioAction.Ignore;
                scconfig.Flow = SNScenarioAction.Ignore;
                scconfig.GasConstraint = SNScenarioAction.Ignore;
                scconfig.LiquidConstraint = SNScenarioAction.Ignore;
                scconfig.PressureConstraint = SNScenarioAction.Ignore;
                scconfig.Well = SNScenarioAction.Update;
                //    scconfig.SNScenerioId = scenario1.Id;
                SurfaceNetworkService.SaveSNScenarioSectionDefaults(scconfig);
                #endregion

                Trace.WriteLine("Added 6 Wells for ADNOC Gas Asset");
                #region 7.Run Scemnario and Verify Results of SN Optimization 

                //Run SN Scenario 
                SurfaceNetworkService.InsertSNScenarioScheduleManualRun(savedscenario.ElementAt(0).Id.ToString());
                SurfaceNetworkService.RunScheduledOptimizationJobs();
                SNOptimizationRunHeaderDTO[] runheaderdtoarr = SurfaceNetworkService.GetSNOptimizationRunHeaderArrayByScenarioId(savedscenario.ElementAt(0).Id.ToString());

                SNOptimizationWellResultsArrayAndUnitsDTO wellresults = SurfaceNetworkService.GetSNOptimizationWellResultsBySNOptimizationRunId(runheaderdtoarr[0].Id.ToString());
                SNOptimizationWellResultsDTO[] welresults = wellresults.Values;
                SNOptimizationWellResultsDTO welresultforGP1 = welresults.FirstOrDefault(w => w.WellName == "GP1");
                Assert.AreEqual(2286.70, (double)welresultforGP1.WellheadPressure, 0.5, "WellHead Pressure for Optimizaed Results is NOT matching");
                SNOptimizationWellResultsDTO welresultforGP2 = welresults.FirstOrDefault(w => w.WellName == "GP2");
                Assert.AreEqual(2812.70, (double)welresultforGP2.WellheadPressure, 0.5, "WellHead Pressure for Optimizaed Results is  NOT  matching");
                SNOptimizationWellResultsDTO welresultforGP3 = welresults.FirstOrDefault(w => w.WellName == "GP3");
                Assert.AreEqual(2137.70, (double)welresultforGP3.WellheadPressure, 0.5, "WellHead Pressure for Optimizaed Results is  NOT  matching");
                SNOptimizationWellResultsDTO welresultforGP4 = welresults.FirstOrDefault(w => w.WellName == "GP4");
                Assert.AreEqual(1444.28, (double)welresultforGP4.WellheadPressure, 0.5, "WellHead Pressure for Optimizaed Results is  NOT  matching");
                SNOptimizationWellResultsDTO welresultforGI1 = welresults.FirstOrDefault(w => w.WellName == "GI1");
                Assert.AreEqual(4643.70, (double)welresultforGI1.WellheadPressure, 0.5, "WellHead Pressure for Optimizaed Results is NOT  matching");
                SNOptimizationWellResultsDTO welresultforGI2 = welresults.FirstOrDefault(w => w.WellName == "GI2");
                Assert.AreEqual(4304.70, (double)welresultforGI2.WellheadPressure, 0.5, "WellHead Pressure for Optimizaed Results is NOT  matching");
                #endregion

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }



        public void AddADNOCWells()
        {
            string[] modelnames = new string[] { "GP1.wflx", "GP2.wflx", "GP3.wflx", "GP4.wflx", "GI1.wflx", "GI2.wflx" };
            List<string> modelfilepath = new List<string>();
            foreach (string mdfile in modelnames)
            {
                modelfilepath.Add(GetFileFromAssemblyStream(mdfile));
            }
            string[] filenames = modelfilepath.ToArray();
            foreach (string fname in modelnames)
            {
                //Well model files are copied to TestDocuments folder : 
                WellDTO well1 = null;
                string wellname = fname.Trim();
                wellname = wellname.Replace("\\", "");
                if (wellname.Contains(".wflx") && wellname.Contains("GP"))
                {
                    Trace.WriteLine("Well name Going to create :  NF Condensate  " + wellname);
                    well1 = SetNFCondensate(new WellDTO() { Name = wellname.Replace(".wflx", ""), FacilityId = null, DataConnection = null, CommissionDate = DateTime.Today.AddYears(-4), AssemblyAPI = wellname.Replace(".wflx", ""), SubAssemblyAPI = wellname.Replace(".wflx", ""), IntervalAPI = wellname.Replace(".wflx", ""), WellDepthDatumId = 1, WellType = WellTypeId.NF });
                }
                else if (wellname.Contains(".wflx") && (wellname.Contains("GI")))
                {
                    Trace.WriteLine("Well name Going to create : GI  " + wellname);
                    well1 = SetDefaultFluidType(new WellDTO() { Name = wellname.Replace(".wflx", ""), FacilityId = null, DataConnection = null, CommissionDate = DateTime.Today.AddYears(-4), AssemblyAPI = wellname.Replace(".wflx", ""), SubAssemblyAPI = wellname.Replace(".wflx", ""), IntervalAPI = wellname.Replace(".wflx", ""), WellDepthDatumId = 1, WellType = WellTypeId.GInj });
                }
                else
                {
                    continue;
                }
                well1.WellStatusId = WellConfigurationService.GetReferenceTableItems("r_WellStatus").ElementAt(1).Id;
                well1.SurfaceLatitude = 19.076090m;
                well1.SurfaceLongitude = 72.877426m;
                well1.Lease = "Lease Name1";
                well1.Field = "Field Name1";
                well1.Engineer = "Engineer Name1";
                well1.GeographicRegion = "Geographic Region1";
                well1.Foreman = "Foreman Name1";
                well1.GaugerBeat = "Gauger Beat1";
                well1.AssetId = SurfaceNetworkService.GetAllAssets().FirstOrDefault(x => x.Name == "ADNOC Gas Asset").Id;
                WellConfigDTO wellConfigDTO1 = new WellConfigDTO();
                wellConfigDTO1.Well = well1;
                wellConfigDTO1.ModelConfig = ReturnBlankModel();
                WellConfigDTO addedWellConfig1 = WellConfigurationService.AddWellConfig(wellConfigDTO1);
                //     _wellsToRemove.Add(addedWellConfig1.Well);
                Trace.WriteLine(wellname + " Added Successfully");
                AddModelFile(addedWellConfig1.Well.CommissionDate.Value.AddDays(1), addedWellConfig1.Well.WellType, addedWellConfig1.Well.Id, CalibrationMethodId.ReservoirPressure, wellname);
                Trace.WriteLine("Model File added in Well: " + wellname);
                addedWellConfig1 = WellConfigurationService.GetWellConfig(addedWellConfig1.Well.Id.ToString());
                _wellsToRemove.Add(addedWellConfig1.Well);
            }
        }

        public int GetSNDeleteAuditTableRecordCount()
        {
            int recordCount = 0;

            string connectionString = String.Empty;
            connectionString = s_isRunningInATS ? "Server =.\\SQLEXPRESS; Database = POP; Integrated Security = True; " : "Server=.\\SQLEXPRESS;Database=POP_TeamCity; Integrated Security=True;";
            string queryString = "SELECT * FROM DeleteAudit WHERE delTableName like '%SN%';";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(queryString, connection);
                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            recordCount = recordCount + 1;
                        }
                        reader.Close();
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            return recordCount;
        }
    }

    // Ignoring the AuditableDTO fields as these change when written back to server
    public class WellDataDTOComparerClass : System.Collections.IComparer
    {
        int System.Collections.IComparer.Compare(object obj1, object obj2)
        {
            int retval = 0;

            SNScenarioWellDataDTO scobj1 = (SNScenarioWellDataDTO)obj1;
            SNScenarioWellDataDTO scobj2 = (SNScenarioWellDataDTO)obj2;

            retval = scobj1.Id.CompareTo(scobj2.Id);

            if (retval == 0) retval = scobj1.SNScenarioId.CompareTo(scobj2.SNScenarioId);
            if (retval == 0) retval = scobj1.SNWellName.CompareTo(scobj2.SNWellName);
            if (retval == 0) retval = scobj1.ReOWellType.CompareTo(scobj2.ReOWellType);
            if (scobj1.ForeSiteWellName != null && scobj2.ForeSiteWellName != null)
                if (retval == 0) retval = scobj1.ForeSiteWellName.CompareTo(scobj2.ForeSiteWellName);

            return retval;
        }
    }

    public class ChokeDataDTOComparerClass : System.Collections.IComparer
    {
        int System.Collections.IComparer.Compare(object obj1, object obj2)
        {
            int retval = 0;

            SNScenarioChokeDataDTO scobj1 = (SNScenarioChokeDataDTO)obj1;
            SNScenarioChokeDataDTO scobj2 = (SNScenarioChokeDataDTO)obj2;

            retval = scobj1.Id.CompareTo(scobj2.Id);

            if (retval == 0) retval = scobj1.DiameterAction.CompareTo(scobj2.DiameterAction);
            if ((retval == 0) && (scobj1.DiameterInput.HasValue) && (scobj2.DiameterInput.HasValue)) retval = scobj1.DiameterInput.Value.CompareTo(scobj2.DiameterInput.Value);
            if (retval == 0) retval = scobj1.Name.CompareTo(scobj2.Name);
            if (retval == 0) retval = scobj1.SNScenarioId.CompareTo(scobj2.SNScenarioId);
            if ((retval == 0) && (scobj1.TuningFactor.HasValue) && (scobj2.TuningFactor.HasValue)) retval = scobj1.TuningFactor.Value.CompareTo(scobj2.TuningFactor.Value);
            if (retval == 0) retval = scobj1.TuningFactorAction.CompareTo(scobj2.TuningFactorAction);
            if (retval == 0) retval = scobj1.Type.CompareTo(scobj2.Type);

            return retval;
        }
    }

    public class BlockValveDTOComparerClass : System.Collections.IComparer
    {
        int System.Collections.IComparer.Compare(object obj1, object obj2)
        {
            int retval = 0;

            SNScenarioBlockValveDataDTO scobj1 = (SNScenarioBlockValveDataDTO)obj1;
            SNScenarioBlockValveDataDTO scobj2 = (SNScenarioBlockValveDataDTO)obj2;

            retval = scobj1.Id.CompareTo(scobj2.Id);

            if (retval == 0) retval = scobj1.SNScenarioId.CompareTo(scobj2.SNScenarioId);
            if (retval == 0) retval = scobj1.Name.CompareTo(scobj2.Name);

            return retval;
        }
    }

    public class PressConstDTOComparerClass : System.Collections.IComparer
    {
        int System.Collections.IComparer.Compare(object x, object y)
        {
            int retval = 0;

            SNScenarioPressureConstraintDataDTO scobj1 = (SNScenarioPressureConstraintDataDTO)x;
            SNScenarioPressureConstraintDataDTO scobj2 = (SNScenarioPressureConstraintDataDTO)y;

            retval = scobj1.Id.CompareTo(scobj2.Id);

            if (retval == 0) retval = scobj1.Action.CompareTo(scobj2.Action);
            if (retval == 0) retval = scobj1.ConstraintAction.CompareTo(scobj2.ConstraintAction);
            if (retval == 0) retval = scobj1.ConstraintType.CompareTo(scobj2.ConstraintType);
            if (retval == 0) retval = scobj1.ConstraintTypeVariable.CompareTo(scobj2.ConstraintTypeVariable);
            if ((retval == 0) && (scobj1.CostRevenuePenalty.HasValue) && (scobj2.CostRevenuePenalty.HasValue)) retval = scobj1.CostRevenuePenalty.Value.CompareTo(scobj2.CostRevenuePenalty.Value);
            if (retval == 0) retval = scobj1.From.CompareTo(scobj2.From);
            if ((retval == 0) && (scobj1.LowerBound.HasValue) && (scobj2.LowerBound.HasValue)) retval = scobj1.LowerBound.Value.CompareTo(scobj2.LowerBound.Value);
            if (retval == 0) retval = scobj1.SNScenarioId.CompareTo(scobj2.SNScenarioId);
            if (retval == 0) retval = scobj1.SocketName.CompareTo(scobj2.SocketName);
            if ((retval == 0) && (scobj1.TargetTolerance.HasValue) && (scobj2.TargetTolerance.HasValue)) retval = scobj1.TargetTolerance.Value.CompareTo(scobj2.TargetTolerance.Value);
            if ((retval == 0) && (scobj1.TargetValue.HasValue) && (scobj2.TargetValue.HasValue)) retval = scobj1.TargetValue.Value.CompareTo(scobj2.TargetValue.Value);
            if (retval == 0) retval = scobj1.Type.CompareTo(scobj2.Type);
            if ((retval == 0) && (scobj1.UpperBound.HasValue) && (scobj2.UpperBound.HasValue)) retval = scobj1.UpperBound.Value.CompareTo(scobj2.UpperBound.Value);

            return retval;
        }
    }

    public class GasConstDTOComparerClass : System.Collections.IComparer
    {
        int System.Collections.IComparer.Compare(object x, object y)
        {
            int retval = 0;

            SNScenarioGasConstraintDataDTO scobj1 = (SNScenarioGasConstraintDataDTO)x;
            SNScenarioGasConstraintDataDTO scobj2 = (SNScenarioGasConstraintDataDTO)y;

            retval = scobj1.Id.CompareTo(scobj2.Id);

            if (retval == 0) retval = scobj1.Action.CompareTo(scobj2.Action);
            if (retval == 0) retval = scobj1.ConstraintAction.CompareTo(scobj2.ConstraintAction);
            if (retval == 0) retval = scobj1.ConstraintType.CompareTo(scobj2.ConstraintType);
            if (retval == 0) retval = scobj1.ConstraintTypeVariable.CompareTo(scobj2.ConstraintTypeVariable);
            if ((retval == 0) && (scobj1.CostRevenuePenalty.HasValue) && (scobj2.CostRevenuePenalty.HasValue)) retval = scobj1.CostRevenuePenalty.Value.CompareTo(scobj2.CostRevenuePenalty.Value);
            if (retval == 0) retval = scobj1.From.CompareTo(scobj2.From);
            if ((retval == 0) && (scobj1.LowerBound.HasValue) && (scobj2.LowerBound.HasValue)) retval = scobj1.LowerBound.Value.CompareTo(scobj2.LowerBound.Value);
            if (retval == 0) retval = scobj1.SNScenarioId.CompareTo(scobj2.SNScenarioId);
            if (retval == 0) retval = scobj1.SocketName.CompareTo(scobj2.SocketName);
            if ((retval == 0) && (scobj1.TargetTolerance.HasValue) && (scobj2.TargetTolerance.HasValue)) retval = scobj1.TargetTolerance.Value.CompareTo(scobj2.TargetTolerance.Value);
            if ((retval == 0) && (scobj1.TargetValue.HasValue) && (scobj2.TargetValue.HasValue)) retval = scobj1.TargetValue.Value.CompareTo(scobj2.TargetValue.Value);
            if (retval == 0) retval = scobj1.Type.CompareTo(scobj2.Type);
            if ((retval == 0) && (scobj1.UpperBound.HasValue) && (scobj2.UpperBound.HasValue)) retval = scobj1.UpperBound.Value.CompareTo(scobj2.UpperBound.Value);

            return retval;
        }
    }

    public class LiqConstDTOComparerClass : System.Collections.IComparer
    {
        int System.Collections.IComparer.Compare(object x, object y)
        {
            int retval = 0;

            SNScenarioLiquidConstraintDataDTO scobj1 = (SNScenarioLiquidConstraintDataDTO)x;
            SNScenarioLiquidConstraintDataDTO scobj2 = (SNScenarioLiquidConstraintDataDTO)y;

            retval = scobj1.Id.CompareTo(scobj2.Id);

            if (retval == 0) retval = scobj1.Action.CompareTo(scobj2.Action);
            if (retval == 0) retval = scobj1.ConstraintAction.CompareTo(scobj2.ConstraintAction);
            if (retval == 0) retval = scobj1.ConstraintType.CompareTo(scobj2.ConstraintType);
            if (retval == 0) retval = scobj1.ConstraintTypeVariable.CompareTo(scobj2.ConstraintTypeVariable);
            if ((retval == 0) && (scobj1.CostRevenuePenalty.HasValue) && (scobj2.CostRevenuePenalty.HasValue)) retval = scobj1.CostRevenuePenalty.Value.CompareTo(scobj2.CostRevenuePenalty.Value);
            if (retval == 0) retval = scobj1.From.CompareTo(scobj2.From);
            if ((retval == 0) && (scobj1.LowerBound.HasValue) && (scobj2.LowerBound.HasValue)) retval = scobj1.LowerBound.Value.CompareTo(scobj2.LowerBound.Value);
            if (retval == 0) retval = scobj1.SNScenarioId.CompareTo(scobj2.SNScenarioId);
            if (retval == 0) retval = scobj1.SocketName.CompareTo(scobj2.SocketName);
            if ((retval == 0) && (scobj1.TargetTolerance.HasValue) && (scobj2.TargetTolerance.HasValue)) retval = scobj1.TargetTolerance.Value.CompareTo(scobj2.TargetTolerance.Value);
            if ((retval == 0) && (scobj1.TargetValue.HasValue) && (scobj2.TargetValue.HasValue)) retval = scobj1.TargetValue.Value.CompareTo(scobj2.TargetValue.Value);
            if (retval == 0) retval = scobj1.Type.CompareTo(scobj2.Type);
            if ((retval == 0) && (scobj1.UpperBound.HasValue) && (scobj2.UpperBound.HasValue)) retval = scobj1.UpperBound.Value.CompareTo(scobj2.UpperBound.Value);

            return retval;
        }
    }

    public class WellMappingComparerClass : System.Collections.IComparer
    {
        int System.Collections.IComparer.Compare(object x, object y)
        {
            int retval = 0;

            SNWellMappingDTO scobj1 = (SNWellMappingDTO)x;
            SNWellMappingDTO scobj2 = (SNWellMappingDTO)y;

            retval = scobj1.Id.CompareTo(scobj2.Id);

            if (retval == 0) retval = scobj1.AssetId.CompareTo(scobj2.AssetId);
            if (retval == 0) retval = scobj1.FluidModelName.CompareTo(scobj2.FluidModelName);
            if (retval == 0) retval = scobj1.SNWellName.CompareTo(scobj2.SNWellName);

            return retval;
        }
    }

}