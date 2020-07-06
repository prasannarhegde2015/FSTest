using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Weatherford.POP.DTOs;
using Weatherford.POP.Enums;

namespace Weatherford.POP.Server.IntegrationTests
{
    [TestClass]
    public class WellInferredProductionAndAllocationServiceTests : APIClientTestBase
    {
        private List<string> _modelFilesToRemove;

        [TestInitialize]
        public override void Init()
        {
            base.Init();
            _modelFilesToRemove = new List<string>();
        }

        [TestCleanup]
        public override void Cleanup()
        {
            base.Cleanup();
        }

        [TestCategory(TestCategories.WellInferredProductionAndAllocationServiceTests), TestMethod]
        public void WellDailyAverageDataSourceSNTest()
        {
            if (!s_isRunningInATS)
            {
                WellDailyAverageDataSourceSN();
            }
        }

        public void WellDailyAverageDataSourceSN()
        {
            //#region  Add Forsite Well with Model
            ////Get the file from TestDocuments folder
            string Path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";
            Tuple<string, WellTypeId, ModelFileOptionDTO> model1 = Tuple.Create("WellfloESPExample1.wflx", WellTypeId.ESP, new ModelFileOptionDTO() { CalibrationMethod = CalibrationMethodId.ReservoirPressure, OptionalUpdate = new long[] { ((long)OptionalUpdates.UpdateWCT_WGR), ((long)OptionalUpdates.PumpWearFactor) } });

            string modelFileName = model1.Item1;
            WellTypeId wellType = model1.Item2;
            ModelFileOptionDTO options = model1.Item3;
            Trace.WriteLine("Testing model: " + modelFileName);

            //Create a new well
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = SetDefaultFluidType(new WellDTO() { Name = "CASETestSN" + wellType.ToString(), CommissionDate = DateTime.Today.AddDays(-10), WellType = wellType }) });
            var allWells = WellService.GetAllWells().ToList();
            WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals("CASETestSN" + wellType.ToString()));
            Assert.IsNotNull(well);
            _wellsToRemove.Add(well);

            //Upload Well FIO File
            ModelFileBase64DTO modelFile1 = new ModelFileBase64DTO() { };

            options.Comment = "CASETestSN Upload " + model1.Item1;
            modelFile1.Options = options;
            modelFile1.ApplicableDate = DateTime.Today.ToUniversalTime() - TimeSpan.FromDays(1);
            modelFile1.WellId = well.Id;

            byte[] fileAsByteArray1 = GetByteArray(Path, model1.Item1);
            Assert.IsNotNull(fileAsByteArray1);
            modelFile1.Base64Contents = Convert.ToBase64String(fileAsByteArray1);
            Assert.AreEqual(wellType, ModelFileService.GetWellModelType(modelFile1), "Model File type mismatch");
            ModelFileValidationDataDTO ModelFileValidationData = ModelFileService.GetModelFileValidation(modelFile1);
            Assert.IsNotNull(ModelFileValidationData);
            ModelFileService.AddWellModelFile(modelFile1);
            ModelFileDTO newModelFile = ModelFileService.GetCurrentModelFile(well.Id.ToString());
            Assert.IsNotNull(newModelFile);
            _modelFilesToRemove.Add(newModelFile.Id.ToString());

            //#endregion Add Forsite Well with Model

            #region Upload SN Reo file and Map with Well

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

            var allModels = SurfaceNetworkService.GetSNModelsByAsset(asset.Id.ToString());
            SNModelDTO newModel = allModels?.FirstOrDefault(m => m.Name.Equals(modelName));

            //Check that there is 1 ModelFile
            SNModelFileDTO[] modelFileCount1 = SurfaceNetworkService.GetSNModelFilesByModelId(newModel.Id.ToString());
            Assert.AreEqual(1, modelFileCount1.Count());

            // Use the ModelFileHeaders variant and verify still only get 1 count
            SNModelFileHeaderDTO[] modelFileHeadersCount = SurfaceNetworkService.GetSNModelFileHeadersByModelId(newModel.Id.ToString());
            Assert.AreEqual(1, modelFileHeadersCount.Count());

            // Upload the model file to get things configured
            modelAndFile.SNModel = newModel;
            SurfaceNetworkService.SaveReOFile(modelAndFile);

            // Setup asset to pull mappings against

            SNWellMappingDTO[] mappings = SurfaceNetworkService.GetSNWellMappingsByAssetIds(new long[] { asset.Id });
            Assert.IsNotNull(mappings);

            //Upadted SN well with Forsite well

            mappings[10].Well = well;
            mappings[10].WellId = well.Id;

            SurfaceNetworkService.UpdateSNWellMapping(mappings[10]);

            SNWellMappingDTO[] mappingsB = SurfaceNetworkService.GetSNWellMappingsByAssetIds(new long[] { asset.Id });
            Assert.IsNotNull(mappingsB);

            System.Collections.IComparer wellMappingComparer = new WellMappingComparerClass();
            CollectionAssert.AreEqual(mappings, mappingsB, wellMappingComparer);

            SurfaceNetworkService.UpdateSNWellMappings(mappingsB);

            SNWellMappingDTO[] mappingsC = SurfaceNetworkService.GetSNWellMappingsByAssetIds(new long[] { asset.Id });
            Assert.IsNotNull(mappingsC);

            CollectionAssert.AreEqual(mappings, mappingsC, wellMappingComparer);

            // Add Scenario on Surface Network.
            string scenarioName = "Scenario Test";

            SurfaceNetworkService.AddSNScenario(new SNScenarioDTO() { Name = scenarioName, Description = "Test Description", SNModelId = newModel.Id, ColorCode = "#FF0000", AllowableScenario = true, Implementable = false, ModelMatch = false, OptimumScenario = false });
            var allScenarios = SurfaceNetworkService.GetSNScenariosBySNModel(newModel.Id.ToString());
            SNScenarioDTO scenario1 = allScenarios?.FirstOrDefault(sn => sn.Name.Equals(scenarioName));
            Assert.IsNotNull(scenario1);

            #endregion Upload SN Reo file and Map with Well

            //Remove SN scenario
            SurfaceNetworkService.RemoveSNScenario(scenario1.Id.ToString());
            SNScenarioDTO scenarioRemoved = SurfaceNetworkService.GetSNScenario(scenario1.Id.ToString());
            Assert.IsNull(scenarioRemoved);

            // Remove SN model

            SurfaceNetworkService.RemoveSNModel(newModel.Id.ToString());
            SNModelDTO modelRemoved = SurfaceNetworkService.GetSNModel(newModel.Id.ToString());
            Assert.IsNull(modelRemoved);

            //Remove SN model File

            SurfaceNetworkService.RemoveSNModelFile(newModel.Id.ToString());
            SNModelDTO modelfile = SurfaceNetworkService.GetSNModel(modelAndFile.SNModelFile.Id.ToString());
            Assert.IsNull(modelRemoved);

            // Update SNwellMapping to delete well

            mappings[10].Well = null;
            mappings[10].WellId = null;

            SurfaceNetworkService.UpdateSNWellMapping(mappings[10]);

            //Remove Asset
            SurfaceNetworkService.RemoveAsset(asset.Id.ToString());
            AssetDTO assetRemoved = SurfaceNetworkService.GetAsset(asset.Id.ToString());
            Assert.IsNull(assetRemoved);

            //Remove the  model file
            ModelFileService.RemoveModelFileOptionByModelFileId(newModelFile.Id.ToString());
            ModelFileService.RemoveModelFile(newModelFile.Id.ToString());

            WellSettingDTO wellsettingdto = SettingService.GetWellSettingsByWellId(well.Id.ToString()).FirstOrDefault();
            Assert.IsNotNull(wellsettingdto);
            SettingService.RemoveWellSetting(wellsettingdto.Id.ToString());

            //Remove the added well
            //WellService.RemoveWell(well.Id.ToString());
            WellConfigurationService.RemoveWellConfig(well.Id.ToString());
            _wellsToRemove.Remove(well);
        }
    }
}
