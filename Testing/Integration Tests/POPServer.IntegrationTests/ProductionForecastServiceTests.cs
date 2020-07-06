using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Weatherford.POP.DTOs;
using Weatherford.POP.Enums;
using System.Diagnostics;
using System.Threading;
using Newtonsoft.Json;

namespace Weatherford.POP.Server.IntegrationTests
{
    [TestClass]
    public class ProductionForecastServiceTests : APIClientTestBase
    {
        [TestInitialize]
        public override void Init()
        {
            base.Init();
        }

        [TestCleanup]
        public override void Cleanup()
        {
            ChangeUnitSystemUserSetting("US");
            UpdateUserWithGivenRole("Administrator");
            base.Cleanup();
        }

        [TestCategory(TestCategories.ProductionForecastServiceTests), TestMethod]
        public void AddGetRemovePFScenarios()
        {
            string pfScenarioName1 = "Test PF Scenario1";
            string pfScenarioName2 = "Test PF Scenario2";

            //Create 2 Assets
            List<AssetDTO> assetIds = CreateAsset(2);

            //Create SNScenario1 with Production Forecast selection 
            SNScenarioDTO snScenarioObj1 = CreateScenario("SNModel1", "SNScenarioName1", assetIds.ElementAt(0).Id, "SN Scenario Description1", true);
            //Adding PFModel1
            PFModelDTO pfModel1 = CreatePFModel("PFModelName1", Enums.ProductionForecastReservoirOption.Eclipse, "Tutorial3.rts", assetIds.ElementAt(0).Id, "PF Model1 Description");
            _pfModelsToRemove.Add(pfModel1);
            //Create PFScenario 1
            CreateForecastScenario(pfScenarioName1, snScenarioObj1, pfModel1, "PFScenario Description1", assetIds.ElementAt(0));
            //Verifying that ForeSite Scenarios associated to Asset1 are retrieved
            var allForecastScenaios = ProductionForecastService.GetPFScenariosByAssetIds(new long[] { assetIds.ElementAt(0).Id });
            Assert.IsTrue(allForecastScenaios.Count() == 1);
            var pfScenarioObj1 = allForecastScenaios.FirstOrDefault(x => x.Name == pfScenarioName1);
            Assert.IsNotNull(pfScenarioObj1);
            _pfScenariosToRemove.Add(pfScenarioObj1);


            //Create SNScenario2 with Production Forecast selection with another asset Id
            SNScenarioDTO netScenarioObj2 = CreateScenario("SNModel2", "SNScenarioName2", assetIds.ElementAt(1).Id, "SN Scenario Description2", true);
            //Adding PFModel2
            PFModelDTO pfModel2 = CreatePFModel("PFModelName2", Enums.ProductionForecastReservoirOption.Nexus, "Tutorial3.rts", assetIds.ElementAt(1).Id, "PF Model2 Description");
            _pfModelsToRemove.Add(pfModel2);
            //Create PFScenario 2
            CreateForecastScenario(pfScenarioName2, netScenarioObj2, pfModel2, "PFScenario Description2", assetIds.ElementAt(1));
            //Verifying that ForeSite Scenarios associated to Asset2 are retrieved
            allForecastScenaios = ProductionForecastService.GetPFScenariosByAssetIds(new long[] { assetIds.ElementAt(1).Id });
            Assert.IsTrue(allForecastScenaios.Count() == 1);
            var pfScenarioObj2 = allForecastScenaios.FirstOrDefault(x => x.Name == pfScenarioName2);
            Assert.IsNotNull(pfScenarioObj2);
            _pfScenariosToRemove.Add(pfScenarioObj2);

            //Verifying that ForeSite Scenarios associated to Asset1 & Asset2 are retrieved
            allForecastScenaios = ProductionForecastService.GetPFScenariosByAssetIds(new long[] { assetIds.ElementAt(0).Id, assetIds.ElementAt(1).Id });
            Assert.IsTrue(allForecastScenaios.Count() == 2);
            comparePFScenarios(pfScenarioObj1, allForecastScenaios[0]);
            comparePFScenarios(pfScenarioObj2, allForecastScenaios[1]);

        }

        /// <summary>
        /// Integration test for FRWM-6083
        /// </summary>
        [TestCategory(TestCategories.ProductionForecastServiceTests), TestMethod]
        public void DeleteNetworkModelWithoutPFScenarioOptimazation()
        {
            PFScenarioDTO pfScenarioObj = CreateSamplePFScenario();

            //Now inserting scenario from pfScenarios for manual run
            ProductionForecastService.InsertPFScenarioScheduleManualRun(pfScenarioObj.Id.ToString());

            var scheduledForecastScenarios = SurfaceNetworkService.GetScenarioSchedules(Enums.ScenarioType.Forecast);
            Assert.IsTrue(scheduledForecastScenarios.Count() == 1);

            //Deleting network model
            SurfaceNetworkService.RemoveSNModel(pfScenarioObj.SNScenario.SNModel.Id.ToString());

            //Verifying impact of Delete network model
            #region Verification
            scheduledForecastScenarios = SurfaceNetworkService.GetScenarioSchedules(Enums.ScenarioType.Forecast);
            Assert.IsTrue(scheduledForecastScenarios.Count() == 0);

            var allForecastScenaios = ProductionForecastService.GetPFScenariosByAssetIds(new long[] { pfScenarioObj.AssetId });
            Assert.IsTrue(allForecastScenaios.Count() == 0);

            var pfModels = ProductionForecastService.GetPFModelsByAssetIds(new long[] { pfScenarioObj.AssetId });
            Assert.IsTrue(pfScenarioObj.Model.Id == pfModels[0].Id);
            comparePFModel(pfScenarioObj.Model, pfModels[0]);

            var snScenarioObj = SurfaceNetworkService.GetSNScenario(pfScenarioObj.SNScenarioId.ToString());
            Assert.IsNull(snScenarioObj);

            var snModel = SurfaceNetworkService.GetSNModel(pfScenarioObj.SNScenario.SNModel.Id.ToString());
            Assert.IsNull(snModel);
            #endregion
        }

        /// <summary>
        /// Integration test for FRWM-6083
        /// </summary>
        [TestCategory(TestCategories.ProductionForecastServiceTests), TestMethod]
        public void DeleteNetworkModelWithPFScenarioOptimazation()
        {
            PFScenarioDTO pfScenarioObj1 = CreateSamplePFScenario();

            //Now inserting scenario from pfScenarios for manual run
            ProductionForecastService.InsertPFScenarioScheduleManualRun(pfScenarioObj1.Id.ToString());

            //Now running ScheduledForecastScenarios scheduler 
            SurfaceNetworkService.RunScheduledProductionForecastJobs();

            //Verifying that after running PF optimization, one record is created in ScenarioScheduleHistory.
            var scheduledForecastScenariosHistory1 = SurfaceNetworkService.GetScenarioScheduleHistory(Enums.ScenarioType.Forecast);
            Assert.IsTrue(scheduledForecastScenariosHistory1.Count() == 1);

            //Deleting network model
            SurfaceNetworkService.RemoveSNModel(pfScenarioObj1.SNScenario.SNModel.Id.ToString());

            //Verifying impact of Delete network model
            #region Verification
            scheduledForecastScenariosHistory1 = SurfaceNetworkService.GetScenarioScheduleHistory(Enums.ScenarioType.Forecast);
            Assert.IsTrue(scheduledForecastScenariosHistory1.Count() == 0);

            var allForecastScenaios1 = ProductionForecastService.GetPFScenariosByAssetIds(new long[] { pfScenarioObj1.AssetId });
            Assert.IsTrue(allForecastScenaios1.Count() == 0);

            var pfModels1 = ProductionForecastService.GetPFModelsByAssetIds(new long[] { pfScenarioObj1.AssetId });
            Assert.IsTrue(pfModels1.Count() == 1);
            Assert.IsTrue(pfScenarioObj1.Model.Id == pfModels1[0].Id);
            comparePFModel(pfScenarioObj1.Model, pfModels1[0]);

            var snScenarioObj1 = SurfaceNetworkService.GetSNScenario(pfScenarioObj1.SNScenarioId.ToString());
            Assert.IsNull(snScenarioObj1);

            var snModel1 = SurfaceNetworkService.GetSNModel(pfScenarioObj1.SNScenario.SNModel.Id.ToString());
            Assert.IsNull(snModel1);
            #endregion

        }

        /// <summary>
        /// Integration test for FRWM-6200
        /// </summary>
        [TestCategory(TestCategories.ProductionForecastServiceTests), TestMethod]
        public void PlotforRunResults()
        {
            PFScenarioDTO pfScenarioObject = CreateSamplePFScenario();

            //Now inserting scenario from pfScenarios for manual run
            ProductionForecastService.InsertPFScenarioScheduleManualRun(pfScenarioObject.Id.ToString());

            //Now running ScheduledForecastScenarios scheduler 
            SurfaceNetworkService.RunScheduledProductionForecastJobs();

            //Verifying that after running PF optimization, one record is created in ScenarioScheduleHistory.
            var scheduledForecastScenariosHistory1 = SurfaceNetworkService.GetScenarioScheduleHistory(Enums.ScenarioType.Forecast);
            Assert.IsTrue(scheduledForecastScenariosHistory1.Count() == 1);
            Trace.WriteLine("Production scenario addedsuccessfully");

            string Path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";
            Tuple<string, WellTypeId, ModelFileOptionDTO> modelInfo = Tuple.Create("WellfloGasLiftExample1.wflx", WellTypeId.GLift, new ModelFileOptionDTO() { CalibrationMethod = CalibrationMethodId.LFactor, OptionalUpdate = new long[] { ((long)OptionalUpdates.UpdateGOR_CGR), ((long)OptionalUpdates.CalculateChokeD_Factor) } });
            string model = modelInfo.Item1;
            WellTypeId wellType = modelInfo.Item2;
            ModelFileOptionDTO options = modelInfo.Item3;

            Trace.WriteLine("Testing model: " + model);
            //Create a new well
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = SetDefaultFluidType(new WellDTO() { Name = DefaultWellName + wellType.ToString(), CommissionDate = DateTime.Today, AssemblyAPI = DefaultWellName + wellType.ToString(), SubAssemblyAPI = DefaultWellName + wellType.ToString(), IntervalAPI = DefaultWellName + wellType.ToString(), WellType = WellTypeId.GLift }) });
            var allWells = WellService.GetAllWells().ToList();
            WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName + wellType.ToString()));
            Assert.IsNotNull(well);


            ModelFileBase64DTO modelFile = new ModelFileBase64DTO() { };

            options.Comment = "CASETest Upload " + modelInfo.Item1;
            modelFile.Options = options;
            modelFile.ApplicableDate = DateTime.Today.ToUniversalTime();
            modelFile.WellId = well.Id;

            byte[] fileAsByteArray = GetByteArray(Path, modelInfo.Item1);
            Assert.IsNotNull(fileAsByteArray);

            modelFile.Base64Contents = Convert.ToBase64String(fileAsByteArray);
            Assert.AreEqual(wellType, ModelFileService.GetWellModelType(modelFile), "Model File type mismatch");
            ModelFileService.AddWellModelFile(modelFile);
            ModelFileDTO newModelFile = ModelFileService.GetCurrentModelFile(well.Id.ToString());
            CommonModelConfigDTO commonModelConfig = ModelFileService.GetCommonModelConfig(well.Id.ToString());
            Assert.IsNotNull(commonModelConfig);
            ModelFileOptionDTO ReturnedOptions = newModelFile.Options;
            Assert.AreEqual(options.CalibrationMethod, ReturnedOptions.CalibrationMethod);
            Assert.AreEqual(well.Id.ToString(), commonModelConfig.WellId);

            var results = ProductionForecastService.GetPFScenarioWellResultsByPFScenarioRunIdAndWellName(pfScenarioObject.Id.ToString(), DefaultWellName);
            Assert.IsNotNull(results);
            _wellsToRemove.Add(well);

        }

        /// <summary>
        /// Integration test for FRWM-6152
        /// Test Case 1 : Delete Network Model with full permissions.
        /// </summary>
        [TestCategory(TestCategories.ProductionForecastServiceTests), TestMethod]
        public void DeleteNetworkModelWithFullPermission()
        {
            // Add Asset, Network Model, SN Scenario, Forecast Model and Scenario
            PFScenarioDTO pfScenarioObj = CreateSamplePFScenario();

            try
            {
                SurfaceNetworkService.RemoveSNModel(pfScenarioObj.SNScenario.SNModel.Id.ToString());
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
                Assert.IsTrue(e.Message.Contains("Forbidden"));
            }

            var allForecastScenaios = ProductionForecastService.GetPFScenariosByAssetIds(new long[] { pfScenarioObj.AssetId });
            Assert.IsTrue(allForecastScenaios.Count() == 0);
            Trace.WriteLine("Forecast scenario deleted successfully");

            var pfModels = ProductionForecastService.GetPFModelsByAssetIds(new long[] { pfScenarioObj.AssetId });
            Assert.IsNotNull(pfModels, "During clean up this model get removed.");
            Assert.IsTrue(pfScenarioObj.Model.Id == pfModels[0].Id);
            comparePFModel(pfScenarioObj.Model, pfModels[0]);

            var snScenarioObj = SurfaceNetworkService.GetSNScenario(pfScenarioObj.SNScenarioId.ToString());
            Assert.IsNull(snScenarioObj);
            Trace.WriteLine("Network scenario deleted successfully");

            var snModel = SurfaceNetworkService.GetSNModel(pfScenarioObj.SNScenario.SNModel.Id.ToString());
            Assert.IsNull(snModel);
            Trace.WriteLine("Network Model deleted successfully");
        }

        /// <summary>
        /// Integration test for FRWM-6152
        /// Test Case 1 : Delete Network Scenario with full permissions.
        /// </summary>
        [TestCategory(TestCategories.ProductionForecastServiceTests), TestMethod]
        public void DeleteNetworkScenarioWithFullPermission()
        {
            // Add Asset, Network Model, SN Scenario, Forecast Model and Scenario
            PFScenarioDTO pfScenarioObj = CreateSamplePFScenario();

            try
            {
                // Delete Network Scenario
                SurfaceNetworkService.RemoveSNScenario(pfScenarioObj.SNScenario.Id.ToString());
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
                Assert.IsTrue(e.Message.Contains("Forbidden"));
            }

            var allForecastScenaios = ProductionForecastService.GetPFScenariosByAssetIds(new long[] { pfScenarioObj.AssetId });
            Assert.IsTrue(allForecastScenaios.Count() == 0);
            Trace.WriteLine("Forecast scenario deleted successfully");

            var pfModels = ProductionForecastService.GetPFModelsByAssetIds(new long[] { pfScenarioObj.AssetId });
            Assert.IsNotNull(pfModels, "During clean up this model get removed.");
            Assert.IsTrue(pfScenarioObj.Model.Id == pfModels[0].Id);
            comparePFModel(pfScenarioObj.Model, pfModels[0]);

            var snScenarioObj = SurfaceNetworkService.GetSNScenario(pfScenarioObj.SNScenarioId.ToString());
            Assert.IsNull(snScenarioObj);
            Trace.WriteLine("Network scenario deleted successfully");

            var snModel = SurfaceNetworkService.GetSNModel(pfScenarioObj.SNScenario.SNModel.Id.ToString());
            Assert.IsNotNull(snModel);
            Trace.WriteLine("Network Model exist in Database. During clean up this model get removed.");
        }

        /// <summary>
        /// Integration test for FRWM-6152
        /// Test Case 3 : Delete Network Model and Scenario with Revoking DeleteForecastScenario permission.
        /// </summary>
        [TestCategory(TestCategories.ProductionForecastServiceTests), TestMethod]
        public void DeleteNetworkModel_Scenario_RevokePermission()
        {
            CreateRole("TestRole");
            RemovePermissionsFromRole("TestRole", new PermissionId[] { PermissionId.DeleteForecastScenario });
            UpdateUserWithGivenRole("TestRole");

            PFScenarioDTO pfScenarioObj = CreateSamplePFScenario();

            try
            {
                // Remove Network Scenario
                SurfaceNetworkService.RemoveSNScenario(pfScenarioObj.SNScenario.Id.ToString());
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
                Assert.IsTrue(e.Message.Contains("Forbidden"));
            }

            try
            {
                // Remove Network Model
                SurfaceNetworkService.RemoveSNModel(pfScenarioObj.SNScenario.SNModel.Id.ToString());
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
                Assert.IsTrue(e.Message.Contains("Forbidden"));
            }

            var allForecastScenaios = ProductionForecastService.GetPFScenariosByAssetIds(new long[] { pfScenarioObj.AssetId });
            Assert.IsTrue(allForecastScenaios.Count() == 1);

            var pfModels = ProductionForecastService.GetPFModelsByAssetIds(new long[] { pfScenarioObj.AssetId });
            Assert.IsTrue(pfScenarioObj.Model.Id == pfModels[0].Id);
            comparePFModel(pfScenarioObj.Model, pfModels[0]);

            var snScenarioObj = SurfaceNetworkService.GetSNScenario(pfScenarioObj.SNScenarioId.ToString());
            Assert.IsNotNull(snScenarioObj);

            var snModel = SurfaceNetworkService.GetSNModel(pfScenarioObj.SNScenario.SNModel.Id.ToString());
            Assert.IsNotNull(snModel);
        }

        /// <summary>
        /// Integration test for FRWM-6083
        /// Integration test for FRWM-6152 -- Test Case 2 : Delete Network Model with Revoking DeleteNetworkModel and DeleteForecastScenario permissions.
        /// </summary>
        [TestCategory(TestCategories.ProductionForecastServiceTests), TestMethod]
        public void DeleteNetworkModelRevokePermission()
        {
            CreateRole("TestRole");
            RemovePermissionsFromRole("TestRole", new PermissionId[] { PermissionId.DeleteNetworkModel, PermissionId.DeleteForecastScenario });
            UpdateUserWithGivenRole("TestRole");

            PFScenarioDTO pfScenarioObj = CreateSamplePFScenario();

            //Now inserting first scenario from pfScenarios for manual run
            ProductionForecastService.InsertPFScenarioScheduleManualRun(pfScenarioObj.Id.ToString());

            //Now Verifying the response of GetScenarioSchedules after adding one scenario for manual run
            var scheduledForecastScenarios = SurfaceNetworkService.GetScenarioSchedules(Enums.ScenarioType.Forecast);
            Assert.IsTrue(scheduledForecastScenarios.Count() == 1);
            _scheduleScenariosToRemove.Add(scheduledForecastScenarios[0]);

            try
            {
                SurfaceNetworkService.RemoveSNModel(pfScenarioObj.SNScenario.SNModel.Id.ToString());
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
                Assert.IsTrue(e.Message.Contains("Forbidden"));
            }
            scheduledForecastScenarios = SurfaceNetworkService.GetScenarioSchedules(Enums.ScenarioType.Forecast);
            Assert.IsTrue(scheduledForecastScenarios.Count() == 1);

            var allForecastScenaios = ProductionForecastService.GetPFScenariosByAssetIds(new long[] { pfScenarioObj.AssetId });
            Assert.IsTrue(allForecastScenaios.Count() == 1);

            var pfModels = ProductionForecastService.GetPFModelsByAssetIds(new long[] { pfScenarioObj.AssetId });
            Assert.IsTrue(pfScenarioObj.Model.Id == pfModels[0].Id);
            comparePFModel(pfScenarioObj.Model, pfModels[0]);

            var snScenarioObj = SurfaceNetworkService.GetSNScenario(pfScenarioObj.SNScenarioId.ToString());
            Assert.IsNotNull(snScenarioObj);

            var snModel = SurfaceNetworkService.GetSNModel(pfScenarioObj.SNScenario.SNModel.Id.ToString());
            Assert.IsNotNull(snModel);
        }

        /// <summary>
        /// Integration test for FRWM-6074
        /// </summary>
        [TestCategory(TestCategories.ProductionForecastServiceTests), TestMethod]
        public void DeleteNetworkScenarioWithoutPFScenarioOptimazation()
        {

            PFScenarioDTO pfScenarioObj = CreateSamplePFScenario();

            //Now inserting scenario from pfScenarios for manual run
            ProductionForecastService.InsertPFScenarioScheduleManualRun(pfScenarioObj.Id.ToString());

            //Now Verifying the response of GetScenarioSchedules after scenario for manual run
            var scheduledForecastScenarios = SurfaceNetworkService.GetScenarioSchedules(Enums.ScenarioType.Forecast);
            Assert.IsTrue(scheduledForecastScenarios.Count() == 1);

            //Deleting network scenario
            SurfaceNetworkService.RemoveSNScenario(pfScenarioObj.SNScenario.Id.ToString());

            //Verifying impact of Delete network scenario
            scheduledForecastScenarios = SurfaceNetworkService.GetScenarioSchedules(Enums.ScenarioType.Forecast);
            Assert.IsTrue(scheduledForecastScenarios.Count() == 0);

            var allForecastScenaios = ProductionForecastService.GetPFScenariosByAssetIds(new long[] { pfScenarioObj.AssetId });
            Assert.IsTrue(allForecastScenaios.Count() == 0);

            var pfModels = ProductionForecastService.GetPFModelsByAssetIds(new long[] { pfScenarioObj.AssetId });
            Assert.IsTrue(pfModels.Count() == 1);
            Assert.IsTrue(pfScenarioObj.Model.Id == pfModels[0].Id);
            comparePFModel(pfScenarioObj.Model, pfModels[0]);

            var snScenarioObj = SurfaceNetworkService.GetSNScenario(pfScenarioObj.SNScenarioId.ToString());
            Assert.IsNull(snScenarioObj);

            var snModel = SurfaceNetworkService.GetSNModel(pfScenarioObj.SNScenario.SNModel.Id.ToString());
            Assert.IsNotNull(snModel);
        }

        /// <summary>
        /// Integration test for FRWM-6074
        /// </summary>
        [TestCategory(TestCategories.ProductionForecastServiceTests), TestMethod]
        public void DeleteNetworkScenarioWithPFScenarioOptimazation()
        {

            PFScenarioDTO pfScenarioObj = CreateSamplePFScenario();

            //Now inserting scenario from pfScenarios for manual run
            ProductionForecastService.InsertPFScenarioScheduleManualRun(pfScenarioObj.Id.ToString());

            //Now running ScheduledForecastScenarios scheduler 
            SurfaceNetworkService.RunScheduledProductionForecastJobs();

            //Verifying that after running PF optimization, record for ScenarioScheduleHistory is displayed 
            var scheduledForecastScenariosHistory = SurfaceNetworkService.GetScenarioScheduleHistory(Enums.ScenarioType.Forecast);
            Assert.IsTrue(scheduledForecastScenariosHistory.Count() == 1);

            //Deleting network scenario
            SurfaceNetworkService.RemoveSNScenario(pfScenarioObj.SNScenario.Id.ToString());

            //Verifying impact of Delete network scenario
            #region Verification
            scheduledForecastScenariosHistory = SurfaceNetworkService.GetScenarioScheduleHistory(Enums.ScenarioType.Forecast);
            Assert.IsTrue(scheduledForecastScenariosHistory.Count() == 0);

            var allForecastScenaios = ProductionForecastService.GetPFScenariosByAssetIds(new long[] { pfScenarioObj.AssetId });
            Assert.IsTrue(allForecastScenaios.Count() == 0);

            var pfModels = ProductionForecastService.GetPFModelsByAssetIds(new long[] { pfScenarioObj.AssetId });
            Assert.IsTrue(pfModels.Count() == 1);
            Assert.IsTrue(pfScenarioObj.Model.Id == pfModels[0].Id);
            comparePFModel(pfScenarioObj.Model, pfModels[0]);

            var snScenarioObj = SurfaceNetworkService.GetSNScenario(pfScenarioObj.SNScenarioId.ToString());
            Assert.IsNull(snScenarioObj);

            var snModel = SurfaceNetworkService.GetSNModel(pfScenarioObj.SNScenario.SNModel.Id.ToString());
            Assert.IsNotNull(snModel);
            #endregion
        }

        /// <summary>
        /// Integration test for FRWM-6074
        /// Integration test for FRWM-6152 -- Test Case 2 : Delete Network Scenario with Revoking DeleteNetworkModel and DeleteForecastScenario permissions.
        /// </summary>
        [TestCategory(TestCategories.ProductionForecastServiceTests), TestMethod]
        public void DeleteNetworkScenarioRevokePermission()
        {
            CreateRole("TestRole");
            RemovePermissionsFromRole("TestRole", new PermissionId[] { PermissionId.DeleteNetworkModel, PermissionId.DeleteForecastScenario });
            UpdateUserWithGivenRole("TestRole");

            PFScenarioDTO pfScenarioObj = CreateSamplePFScenario();

            //Now inserting first scenario from pfScenarios for manual run
            ProductionForecastService.InsertPFScenarioScheduleManualRun(pfScenarioObj.Id.ToString());

            //Now Verifying the response of GetScenarioSchedules after adding one scenario for manual run
            var scheduledForecastScenarios = SurfaceNetworkService.GetScenarioSchedules(Enums.ScenarioType.Forecast);
            Assert.IsTrue(scheduledForecastScenarios.Count() == 1);
            _scheduleScenariosToRemove.Add(scheduledForecastScenarios[0]);

            try
            {
                SurfaceNetworkService.RemoveSNScenario(pfScenarioObj.SNScenario.Id.ToString());
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
                Assert.IsTrue(e.Message.Contains("Forbidden"));
            }
            scheduledForecastScenarios = SurfaceNetworkService.GetScenarioSchedules(Enums.ScenarioType.Forecast);
            Assert.IsTrue(scheduledForecastScenarios.Count() == 1);

            var allForecastScenaios = ProductionForecastService.GetPFScenariosByAssetIds(new long[] { pfScenarioObj.AssetId });
            Assert.IsTrue(allForecastScenaios.Count() == 1);

            var pfModels = ProductionForecastService.GetPFModelsByAssetIds(new long[] { pfScenarioObj.AssetId });
            Assert.IsTrue(pfScenarioObj.Model.Id == pfModels[0].Id);
            comparePFModel(pfScenarioObj.Model, pfModels[0]);

            var snScenarioObj = SurfaceNetworkService.GetSNScenario(pfScenarioObj.SNScenarioId.ToString());
            Assert.IsNotNull(snScenarioObj);

            var snModel = SurfaceNetworkService.GetSNModel(pfScenarioObj.SNScenario.SNModel.Id.ToString());
            Assert.IsNotNull(snModel);
        }

        [TestCategory(TestCategories.ProductionForecastServiceTests), TestMethod]
        public void UpdatePFScenarios()
        {
            string pfScenarioName = "Test PF Scenario";
            string updateForecastScenarioName = "Test PF Updated Scenario";

            //Create 2 Assets
            List<AssetDTO> assetIds = CreateAsset(1);

            //Create SNScenario1 with Production Forecast selection 
            SNScenarioDTO snScenarioObj1 = CreateScenario("SNModel1", "SNScenarioName1", assetIds.ElementAt(0).Id, "SN Scenario Description1", true);
            //Adding PFModel1
            PFModelDTO pfModel1 = CreatePFModel("PFModelName1", Enums.ProductionForecastReservoirOption.Eclipse, "Tutorial3.rts", assetIds.ElementAt(0).Id, "PF Model1 Description");
            _pfModelsToRemove.Add(pfModel1);
            //Create PFScenario 1
            CreateForecastScenario(pfScenarioName, snScenarioObj1, pfModel1, "PFScenario Description1", assetIds.ElementAt(0));
            //Verifying that ForeSite Scenarios associated to Asset1 are retrieved
            var allForecastScenaios = ProductionForecastService.GetPFScenariosByAssetIds(new long[] { assetIds.ElementAt(0).Id });
            Assert.IsTrue(allForecastScenaios.Count() == 1);
            var pfScenarioObj = allForecastScenaios.FirstOrDefault(x => x.Name == pfScenarioName);
            Assert.IsNotNull(pfScenarioObj);
            _pfScenariosToRemove.Add(pfScenarioObj);

            //Updating PFScenario Name
            allForecastScenaios[0].Name = updateForecastScenarioName;
            ProductionForecastService.UpdatePFScenarioMulti(allForecastScenaios);
            var afterUpdateAllForecastScenario = ProductionForecastService.GetPFScenariosByAssetIds(new long[] { assetIds.ElementAt(0).Id });
            var updatedpfScenarioObj = afterUpdateAllForecastScenario.FirstOrDefault(x => x.Name == updateForecastScenarioName);
            Assert.IsNotNull(updatedpfScenarioObj);
            comparePFScenarios(allForecastScenaios[0], updatedpfScenarioObj);

            //Removing PFScenario 
            ProductionForecastService.RemovePFScenario(pfScenarioObj.Id.ToString());
            var afterRemoveAllForecastScenario = ProductionForecastService.GetPFScenariosByAssetIds(new long[] { assetIds.ElementAt(0).Id });
            Assert.IsTrue(afterRemoveAllForecastScenario.Count() == 0);

            //Since PFScenario is already removed in above step,Removing pfScenarioObj from _pfScenariosToRemove list
            _pfScenariosToRemove.Remove(pfScenarioObj);

        }

        [TestCategory(TestCategories.ProductionForecastServiceTests), TestMethod]
        public void PFEnabledSNScenariosByAssetIdsTest()
        {
            List<AssetDTO> Assets = new List<AssetDTO>();
            string AssetName1 = "Asset1";
            string AssetName2 = "Asset2";
            string ModelName1 = "Model1";
            string ModelName2 = "Model2";
            string ModelName3 = "Model3";
            string ScenarioName1 = "Scenario1";
            string ScenarioName2 = "Scenario2";
            string ScenarioName3 = "Scenario3";

            List<long> userAssets = new List<long>();

            //Creating Asset1
            SurfaceNetworkService.AddAsset(new AssetDTO() { Name = AssetName1, Description = "Asset1 Description" });
            //Creating Asset2
            SurfaceNetworkService.AddAsset(new AssetDTO() { Name = AssetName2, Description = "Asset2 Description" });

            Assets = SurfaceNetworkService.GetAllAssets().ToList();
            _assetsToRemove.AddRange(Assets);

            userAssets.Add(Assets.FirstOrDefault(x => x.Name == AssetName1).Id);
            userAssets.Add(Assets.FirstOrDefault(x => x.Name == AssetName2).Id);

            //Creating Model1 using Asset1
            SurfaceNetworkService.AddSNModel(new SNModelDTO() { Name = ModelName1, AssetId = Assets.FirstOrDefault(x => x.Name == AssetName1).Id });
            //Creating Model2 using Asset1
            SurfaceNetworkService.AddSNModel(new SNModelDTO() { Name = ModelName2, AssetId = Assets.FirstOrDefault(x => x.Name == AssetName1).Id });
            //Creating Model3 using Asset2
            SurfaceNetworkService.AddSNModel(new SNModelDTO() { Name = ModelName3, AssetId = Assets.FirstOrDefault(x => x.Name == AssetName2).Id });
            var allModels = SurfaceNetworkService.GetSNModelsByAssetIds(userAssets.ToArray());
            Assert.AreEqual(3, allModels.Count());
            _snModelsToRemove.AddRange(allModels);

            //Creating Scenario1 using Model1 without selecting ForeCastScenario
            SurfaceNetworkService.AddSNScenario(new SNScenarioDTO() { Name = ScenarioName1, Description = "Scenario Description1", SNModelId = allModels.FirstOrDefault(x => x.Name == ModelName1).Id, ColorCode = "#FF0000", AllowableScenario = true, Implementable = false, ModelMatch = false, OptimumScenario = false, IsForecastScenario = false });
            SNScenarioDTO snScenario1 = SurfaceNetworkService.GetSNScenariosByAssetIds(userAssets.ToArray()).FirstOrDefault(x => x.Name == ScenarioName1);
            //Creating Scenario2 using Model2 with selecting ForeCastScenario
            SurfaceNetworkService.AddSNScenario(new SNScenarioDTO() { Name = ScenarioName2, Description = "Scenario Description2", SNModelId = allModels.FirstOrDefault(x => x.Name == ModelName2).Id, ColorCode = "#FF0000", AllowableScenario = true, Implementable = false, ModelMatch = false, OptimumScenario = false, IsForecastScenario = true });
            SNScenarioDTO snScenario2 = SurfaceNetworkService.GetSNScenariosByAssetIds(userAssets.ToArray()).FirstOrDefault(x => x.Name == ScenarioName2);
            //Creating Scenario3 using Model3 with selecting ForeCastScenario
            SurfaceNetworkService.AddSNScenario(new SNScenarioDTO() { Name = ScenarioName3, Description = "Scenario Description3", SNModelId = allModels.FirstOrDefault(x => x.Name == ModelName3).Id, ColorCode = "#FF0000", AllowableScenario = true, Implementable = false, ModelMatch = false, OptimumScenario = false, IsForecastScenario = true });
            SNScenarioDTO snScenario3 = SurfaceNetworkService.GetSNScenariosByAssetIds(userAssets.ToArray()).FirstOrDefault(x => x.Name == ScenarioName3);

            var allScenarios = SurfaceNetworkService.GetSNScenariosByAssetIds(userAssets.ToArray());
            _snScenariosToRemove.AddRange(allScenarios);

            //Verifying that API is retrieving scenarios which have ForeCast scenarios selected
            SNScenarioDTO[] pfEnabledSNScenarios = ProductionForecastService.GetSNScenariosByAssetIds(userAssets.ToArray());
            Assert.AreEqual(2, pfEnabledSNScenarios.Count());
            Assert.AreEqual(ScenarioName2, pfEnabledSNScenarios[0].Name);
            Assert.AreEqual(ScenarioName3, pfEnabledSNScenarios[1].Name);
            compareSNScenarios(snScenario2, pfEnabledSNScenarios[0]);
            compareSNScenarios(snScenario3, pfEnabledSNScenarios[1]);

            userAssets.Remove(Assets.FirstOrDefault(x => x.Name == AssetName2).Id);
            pfEnabledSNScenarios = ProductionForecastService.GetSNScenariosByAssetIds(userAssets.ToArray());
            Assert.AreEqual(1, pfEnabledSNScenarios.Count());
            Assert.AreEqual(ScenarioName2, pfEnabledSNScenarios[0].Name);
            compareSNScenarios(snScenario2, pfEnabledSNScenarios[0]);

        }

        [TestCategory(TestCategories.ProductionForecastServiceTests), TestMethod]
        public void ScenarioScheduleTest()
        {

            #region Test Data Preparation for verification
            List<AssetDTO> Assets = new List<AssetDTO>();
            string AssetName1 = "Asset1";
            string AssetName2 = "Asset2";
            string ModelName1 = "Model1";
            string ModelName2 = "Model2";
            string ModelName3 = "Model3";
            string snScenarioName1 = "SNScenario1";
            string snScenarioName2 = "SNScenario2";
            string snScenarioName3 = "SNScenario3";
            string snScenarioName4 = "SNScenario4";
            string pfScenarioName1 = "PFScenario1";
            string pfScenarioName2 = "PFScenario2";


            List<long> userAssets = new List<long>();

            //Creating Asset1
            SurfaceNetworkService.AddAsset(new AssetDTO() { Name = AssetName1, Description = "Asset1 Description" });
            //Creating Asset2
            SurfaceNetworkService.AddAsset(new AssetDTO() { Name = AssetName2, Description = "Asset2 Description" });
            Assets = SurfaceNetworkService.GetAllAssets().ToList();
            AssetDTO asset1 = Assets.FirstOrDefault(x => x.Name == AssetName1);
            AssetDTO asset2 = Assets.FirstOrDefault(x => x.Name == AssetName2);
            _assetsToRemove.AddRange(Assets);

            userAssets.Add(Assets.FirstOrDefault(x => x.Name == AssetName1).Id);
            userAssets.Add(Assets.FirstOrDefault(x => x.Name == AssetName2).Id);

            //Creating Model1 using Asset1
            SurfaceNetworkService.AddSNModel(new SNModelDTO() { Name = ModelName1, AssetId = asset1.Id });
            //Creating Model2 using Asset1
            SurfaceNetworkService.AddSNModel(new SNModelDTO() { Name = ModelName2, AssetId = asset1.Id });
            //Creating Model3 using Asset2
            SurfaceNetworkService.AddSNModel(new SNModelDTO() { Name = ModelName3, AssetId = asset2.Id });

            var allModels = SurfaceNetworkService.GetSNModelsByAssetIds(userAssets.ToArray());
            Assert.AreEqual(3, allModels.Count());
            _snModelsToRemove.AddRange(allModels);

            //Creating SN Scenarios
            //Creating Scenario1 using Model1 without selecting ForeCastScenario
            SurfaceNetworkService.AddSNScenario(new SNScenarioDTO() { Name = snScenarioName1, Description = "Scenario Description1", SNModelId = allModels.FirstOrDefault(x => x.Name == ModelName1).Id, ColorCode = "#FF0000", AllowableScenario = true, Implementable = false, ModelMatch = false, OptimumScenario = false, IsForecastScenario = false });
            //Creating Scenario2 using Model2 with selecting ForeCastScenario
            SurfaceNetworkService.AddSNScenario(new SNScenarioDTO() { Name = snScenarioName2, Description = "Scenario Description2", SNModelId = allModels.FirstOrDefault(x => x.Name == ModelName2).Id, ColorCode = "#FF0000", AllowableScenario = true, Implementable = false, ModelMatch = false, OptimumScenario = false, IsForecastScenario = true });
            //Creating Scenario3 using Model3 without selecting ForeCastScenario
            SurfaceNetworkService.AddSNScenario(new SNScenarioDTO() { Name = snScenarioName3, Description = "Scenario Description3", SNModelId = allModels.FirstOrDefault(x => x.Name == ModelName3).Id, ColorCode = "#FF0000", AllowableScenario = true, Implementable = false, ModelMatch = false, OptimumScenario = false, IsForecastScenario = false });
            //Creating Scenario4 using Model3 with selecting ForeCastScenario
            SurfaceNetworkService.AddSNScenario(new SNScenarioDTO() { Name = snScenarioName4, Description = "Scenario Description4", SNModelId = allModels.FirstOrDefault(x => x.Name == ModelName3).Id, ColorCode = "#FF0000", AllowableScenario = true, Implementable = false, ModelMatch = false, OptimumScenario = false, IsForecastScenario = true });

            var allScenarios = SurfaceNetworkService.GetSNScenariosByAssetIds(userAssets.ToArray());
            _snScenariosToRemove.AddRange(allScenarios);

            //Creating PF Scenarios. To create PF scenario,We need SNScenarios which has IsForecastScenario flag set to Yes
            var ForecastEnabledSNScenarios = allScenarios.Where(x => x.IsForecastScenario == true).ToList();

            //Create PFScenario 1
            PFModelDTO pfModel1 = CreatePFModel("PFModelName1", Enums.ProductionForecastReservoirOption.Eclipse, "Tutorial3.rts", asset1.Id, "PF Model1 Description");
            _pfModelsToRemove.Add(pfModel1);
            CreateForecastScenario(pfScenarioName1, ForecastEnabledSNScenarios.ElementAt(0), pfModel1, "PFScenario Description1", asset1);
            //Create PFScenario 2
            PFModelDTO pfModel2 = CreatePFModel("PFModelName2", Enums.ProductionForecastReservoirOption.Eclipse, "Tutorial3.rts", asset2.Id, "PF Model2 Description");
            _pfModelsToRemove.Add(pfModel2);
            CreateForecastScenario(pfScenarioName2, ForecastEnabledSNScenarios.ElementAt(1), pfModel2, "PFScenario Description2", asset2);
            PFScenarioDTO[] pfScenarios = ProductionForecastService.GetPFScenariosByAssetIds(userAssets.ToArray());
            Assert.IsTrue(pfScenarios.Count() == 2);
            #endregion

            #region Verification

            //Before Inserting any scenario from ForecastEnabledSNScenarios for manual run, Verify the response of GetScenarioSchedules API
            SNScenarioScheduleDTO[] scheduledForecastScenarios = SurfaceNetworkService.GetScenarioSchedules(Enums.ScenarioType.Forecast);
            Assert.IsTrue(scheduledForecastScenarios.Count() == 0);

            //Now inserting first scenario from pfScenarios for manual run
            ProductionForecastService.InsertPFScenarioScheduleManualRun(pfScenarios[0].Id.ToString());

            //Now Verifying the response of GetScenarioSchedules after adding one scenario for manual run
            scheduledForecastScenarios = SurfaceNetworkService.GetScenarioSchedules(Enums.ScenarioType.Forecast);
            Assert.IsTrue(scheduledForecastScenarios.Count() == 1);
            _scheduleScenariosToRemove.Add(scheduledForecastScenarios[0]);
            VerifyPFScenarioSchedule(pfScenarios[0], scheduledForecastScenarios[0]);

            //Now retrieving ScheduledForecast scenarios by providing asset ids
            scheduledForecastScenarios = SurfaceNetworkService.GetScenarioSchedulesByUserAssetIds(new long[] { asset1.Id }, Enums.ScenarioType.Forecast.ToString());
            Assert.IsTrue(scheduledForecastScenarios.Count() == 1);
            VerifyPFScenarioSchedule(pfScenarios[0], scheduledForecastScenarios[0]);

            scheduledForecastScenarios = SurfaceNetworkService.GetScenarioSchedulesByUserAssetIds(new long[] { asset2.Id }, Enums.ScenarioType.Forecast.ToString());
            Assert.IsTrue(scheduledForecastScenarios.Count() == 0);

            scheduledForecastScenarios = SurfaceNetworkService.GetScenarioSchedulesByUserAssetIds(new long[] { asset1.Id, asset2.Id }, Enums.ScenarioType.Forecast.ToString());
            Assert.IsTrue(scheduledForecastScenarios.Count() == 1);
            VerifyPFScenarioSchedule(pfScenarios[0], scheduledForecastScenarios[0]);

            //Now inserting second response from pfScenarios for manual run
            ProductionForecastService.InsertPFScenarioScheduleManualRun(pfScenarios[1].Id.ToString());

            //Now Verifying the response of GetScenarioSchedules after adding anonther scenario for manual run
            scheduledForecastScenarios = SurfaceNetworkService.GetScenarioSchedules(Enums.ScenarioType.Forecast);
            Assert.IsTrue(scheduledForecastScenarios.Count() == 2);
            _scheduleScenariosToRemove.Add(scheduledForecastScenarios[1]);
            VerifyPFScenarioSchedule(pfScenarios[1], scheduledForecastScenarios[1]);

            //Now retrieving ScheduledForecast scenarios by providing asset ids
            scheduledForecastScenarios = SurfaceNetworkService.GetScenarioSchedulesByUserAssetIds(new long[] { asset1.Id }, Enums.ScenarioType.Forecast.ToString());
            Assert.IsTrue(scheduledForecastScenarios.Count() == 1);
            VerifyPFScenarioSchedule(pfScenarios[0], scheduledForecastScenarios[0]);

            scheduledForecastScenarios = SurfaceNetworkService.GetScenarioSchedulesByUserAssetIds(new long[] { asset2.Id }, Enums.ScenarioType.Forecast.ToString());
            Assert.IsTrue(scheduledForecastScenarios.Count() == 1);
            VerifyPFScenarioSchedule(pfScenarios[1], scheduledForecastScenarios[0]);

            scheduledForecastScenarios = SurfaceNetworkService.GetScenarioSchedulesByUserAssetIds(new long[] { asset1.Id, asset2.Id }, Enums.ScenarioType.Forecast.ToString());
            Assert.IsTrue(scheduledForecastScenarios.Count() == 2);
            VerifyPFScenarioSchedule(pfScenarios[0], scheduledForecastScenarios[0]);
            VerifyPFScenarioSchedule(pfScenarios[1], scheduledForecastScenarios[1]);

            //Verify that since surface network scenarios  are not scheduled, API should 0 return SurfaceNetworkScenarios
            scheduledForecastScenarios = SurfaceNetworkService.GetScenarioSchedulesByUserAssetIds(new long[] { asset1.Id, asset2.Id }, Enums.ScenarioType.SurfaceNetwork.ToString());
            Assert.IsTrue(scheduledForecastScenarios.Count() == 0);
            #endregion
        }

        //Remove ignore attribute and Uncomment Verify methods when delete PFScenario is implemented
        // [Ignore]
        [TestCategory(TestCategories.ProductionForecastServiceTests), TestMethod]
        public void ScenarioScheduleHistoryTest()
        {

            #region Test Data Preparation for verification
            List<AssetDTO> Assets = new List<AssetDTO>();
            string AssetName1 = "Asset1";
            string AssetName2 = "Asset2";
            string ModelName1 = "Model1";
            string ModelName2 = "Model2";
            string ModelName3 = "Model3";
            string snScenarioName1 = "SNScenario1";
            string snScenarioName2 = "SNScenario2";
            string snScenarioName3 = "SNScenario3";
            string snScenarioName4 = "SNScenario4";
            string pfScenarioName1 = "PFScenario1";
            string pfScenarioName2 = "PFScenario2";


            List<long> userAssets = new List<long>();

            //Creating Asset1
            SurfaceNetworkService.AddAsset(new AssetDTO() { Name = AssetName1, Description = "Asset1 Description" });
            //Creating Asset2
            SurfaceNetworkService.AddAsset(new AssetDTO() { Name = AssetName2, Description = "Asset2 Description" });
            Assets = SurfaceNetworkService.GetAllAssets().ToList();
            AssetDTO asset1 = Assets.FirstOrDefault(x => x.Name == AssetName1);
            AssetDTO asset2 = Assets.FirstOrDefault(x => x.Name == AssetName2);
            _assetsToRemove.AddRange(Assets);

            userAssets.Add(Assets.FirstOrDefault(x => x.Name == AssetName1).Id);
            userAssets.Add(Assets.FirstOrDefault(x => x.Name == AssetName2).Id);

            //Creating Model1 using Asset1
            SurfaceNetworkService.AddSNModel(new SNModelDTO() { Name = ModelName1, AssetId = asset1.Id });
            //Creating Model2 using Asset1
            SurfaceNetworkService.AddSNModel(new SNModelDTO() { Name = ModelName2, AssetId = asset1.Id });
            //Creating Model3 using Asset2
            SurfaceNetworkService.AddSNModel(new SNModelDTO() { Name = ModelName3, AssetId = asset2.Id });

            var allModels = SurfaceNetworkService.GetSNModelsByAssetIds(userAssets.ToArray());
            Assert.AreEqual(3, allModels.Count());
            _snModelsToRemove.AddRange(allModels);

            //Creating SN Scenarios
            //Creating Scenario1 using Model1 without selecting ForeCastScenario
            SurfaceNetworkService.AddSNScenario(new SNScenarioDTO() { Name = snScenarioName1, Description = "Scenario Description1", SNModelId = allModels.FirstOrDefault(x => x.Name == ModelName1).Id, ColorCode = "#FF0000", AllowableScenario = true, Implementable = false, ModelMatch = false, OptimumScenario = false, IsForecastScenario = false });
            //Creating Scenario2 using Model2 with selecting ForeCastScenario
            SurfaceNetworkService.AddSNScenario(new SNScenarioDTO() { Name = snScenarioName2, Description = "Scenario Description2", SNModelId = allModels.FirstOrDefault(x => x.Name == ModelName2).Id, ColorCode = "#FF0000", AllowableScenario = true, Implementable = false, ModelMatch = false, OptimumScenario = false, IsForecastScenario = true });
            //Creating Scenario3 using Model3 without selecting ForeCastScenario
            SurfaceNetworkService.AddSNScenario(new SNScenarioDTO() { Name = snScenarioName3, Description = "Scenario Description3", SNModelId = allModels.FirstOrDefault(x => x.Name == ModelName3).Id, ColorCode = "#FF0000", AllowableScenario = true, Implementable = false, ModelMatch = false, OptimumScenario = false, IsForecastScenario = false });
            //Creating Scenario4 using Model3 with selecting ForeCastScenario
            SurfaceNetworkService.AddSNScenario(new SNScenarioDTO() { Name = snScenarioName4, Description = "Scenario Description4", SNModelId = allModels.FirstOrDefault(x => x.Name == ModelName3).Id, ColorCode = "#FF0000", AllowableScenario = true, Implementable = false, ModelMatch = false, OptimumScenario = false, IsForecastScenario = true });

            var allScenarios = SurfaceNetworkService.GetSNScenariosByAssetIds(userAssets.ToArray());
            _snScenariosToRemove.AddRange(allScenarios);

            //Creating PF Scenarios. To create PF scenario,We need SNScenarios which has IsForecastScenario flag set to Yes
            var ForecastEnabledSNScenarios = allScenarios.Where(x => x.IsForecastScenario == true).ToList();

            //Create PFScenario 1
            PFModelDTO pfModel1 = CreatePFModel("PFModelName1", Enums.ProductionForecastReservoirOption.Eclipse, "Tutorial3.rts", asset1.Id, "PF Model1 Description");
            _pfModelsToRemove.Add(pfModel1);
            CreateForecastScenario(pfScenarioName1, ForecastEnabledSNScenarios.ElementAt(0), pfModel1, "PFScenario Description1", asset1);
            //Create PFScenario 2
            PFModelDTO pfModel2 = CreatePFModel("PFModelName2", Enums.ProductionForecastReservoirOption.Eclipse, "Tutorial3.rts", asset2.Id, "PF Model2 Description");
            _pfModelsToRemove.Add(pfModel2);
            CreateForecastScenario(pfScenarioName2, ForecastEnabledSNScenarios.ElementAt(1), pfModel2, "PFScenario Description2", asset2);
            PFScenarioDTO[] pfScenarios = ProductionForecastService.GetPFScenariosByAssetIds(userAssets.ToArray());
            Assert.IsTrue(pfScenarios.Count() == 2);
            #endregion

            #region Verification
            //Now inserting first scenario from pfScenarios for manual run
            ProductionForecastService.InsertPFScenarioScheduleManualRun(pfScenarios[0].Id.ToString());

            //Now running ScheduledForecastScenarios scheduler 
            SurfaceNetworkService.RunScheduledProductionForecastJobs();

            //Now Verifying the response of GetScenarioScheduleHistory after running scgeduler for one scenario
            SNScenarioScheduleHistoryDTO[] scheduledForecastScenarioHistory = SurfaceNetworkService.GetScenarioScheduleHistory(Enums.ScenarioType.Forecast);
            Assert.IsTrue(scheduledForecastScenarioHistory.Count() == 1);
            VerifyPFScenarioScheduleHistory(pfScenarios[0], scheduledForecastScenarioHistory[0]);

            //Now retrieving ScheduledForecast scenarios by providing asset ids
            scheduledForecastScenarioHistory = SurfaceNetworkService.GetScenarioScheduleHistoryByUserAssetIds(new long[] { asset1.Id }, Enums.ScenarioType.Forecast.ToString());
            Assert.IsTrue(scheduledForecastScenarioHistory.Count() == 1);
            VerifyPFScenarioScheduleHistory(pfScenarios[0], scheduledForecastScenarioHistory[0]);

            scheduledForecastScenarioHistory = SurfaceNetworkService.GetScenarioScheduleHistoryByUserAssetIds(new long[] { asset2.Id }, Enums.ScenarioType.Forecast.ToString());
            Assert.IsTrue(scheduledForecastScenarioHistory.Count() == 0);

            scheduledForecastScenarioHistory = SurfaceNetworkService.GetScenarioScheduleHistoryByUserAssetIds(new long[] { asset1.Id, asset2.Id }, Enums.ScenarioType.Forecast.ToString());
            Assert.IsTrue(scheduledForecastScenarioHistory.Count() == 1);
            VerifyPFScenarioScheduleHistory(pfScenarios[0], scheduledForecastScenarioHistory[0]);

            //Now inserting second response from pfScenarios for manual run
            ProductionForecastService.InsertPFScenarioScheduleManualRun(pfScenarios[1].Id.ToString());

            //Now running ScheduledForecastScenarios scheduler 
            SurfaceNetworkService.RunScheduledProductionForecastJobs();

            //Now Verifying the response of GetScenarioScheduleHistory after running scgeduler for another scenario
            scheduledForecastScenarioHistory = SurfaceNetworkService.GetScenarioScheduleHistory(Enums.ScenarioType.Forecast);
            Assert.IsTrue(scheduledForecastScenarioHistory.Count() == 2);

            //Now retrieving ScenarioScheduleHistory by providing asset ids
            scheduledForecastScenarioHistory = SurfaceNetworkService.GetScenarioScheduleHistoryByUserAssetIds(new long[] { asset1.Id }, Enums.ScenarioType.Forecast.ToString());
            Assert.IsTrue(scheduledForecastScenarioHistory.Count() == 1);
            VerifyPFScenarioScheduleHistory(pfScenarios[0], scheduledForecastScenarioHistory[0]);

            scheduledForecastScenarioHistory = SurfaceNetworkService.GetScenarioScheduleHistoryByUserAssetIds(new long[] { asset2.Id }, Enums.ScenarioType.Forecast.ToString());
            Assert.IsTrue(scheduledForecastScenarioHistory.Count() == 1);
            VerifyPFScenarioScheduleHistory(pfScenarios[1], scheduledForecastScenarioHistory[0]);

            scheduledForecastScenarioHistory = SurfaceNetworkService.GetScenarioScheduleHistoryByUserAssetIds(new long[] { asset1.Id, asset2.Id }, Enums.ScenarioType.Forecast.ToString());
            Assert.IsTrue(scheduledForecastScenarioHistory.Count() == 2);
            VerifyPFScenarioScheduleHistory(pfScenarios[0], scheduledForecastScenarioHistory[0]);
            VerifyPFScenarioScheduleHistory(pfScenarios[1], scheduledForecastScenarioHistory[1]);

            //Verify that since surface network scenarios  are not scheduled, API should 0 return SurfaceNetworkScenarios
            scheduledForecastScenarioHistory = SurfaceNetworkService.GetScenarioScheduleHistoryByUserAssetIds(new long[] { asset1.Id, asset2.Id }, Enums.ScenarioType.SurfaceNetwork.ToString());
            Assert.IsTrue(scheduledForecastScenarioHistory.Count() == 0);
            #endregion
        }

        [TestCategory(TestCategories.ProductionForecastServiceTests), TestMethod]
        public void AddPFModelTest()
        {
            List<AssetDTO> Assets = new List<AssetDTO>();
            string AssetName1 = "Asset1";
            string AssetName2 = "Asset2";
            string PFModelName1 = "PFModel1";
            string PFModelName2 = "PFModel2";

            List<long> userAssets = new List<long>();

            //Creating Asset1
            SurfaceNetworkService.AddAsset(new AssetDTO() { Name = AssetName1, Description = "Asset1 Description" });
            //Creating Asset2
            SurfaceNetworkService.AddAsset(new AssetDTO() { Name = AssetName2, Description = "Asset2 Description" });

            Assets = SurfaceNetworkService.GetAllAssets().ToList();
            long assetId1 = Assets.FirstOrDefault(x => x.Name == AssetName1).Id;
            long assetId2 = Assets.FirstOrDefault(x => x.Name == AssetName2).Id;
            _assetsToRemove.AddRange(Assets);

            userAssets.Add(assetId1);
            //Adding PFModel1
            PFModelDTO pfModel1 = CreatePFModel(PFModelName1, Enums.ProductionForecastReservoirOption.Eclipse, "Tutorial3.rts", assetId1, "PF Model1 Description");
            _pfModelsToRemove.Add(pfModel1);
            //Only one assetid is provided as input parameter hence PFModels associated to the asset id should be retrieved in response
            PFModelDTO[] pfModels = ProductionForecastService.GetPFModelsByAssetIds(userAssets.ToArray());
            Assert.AreEqual(1, pfModels.Count());
            comparePFModel(pfModel1, pfModels[0]);

            userAssets.Add(assetId2);
            //Adding PFModel2
            PFModelDTO pfModel2 = CreatePFModel(PFModelName2, Enums.ProductionForecastReservoirOption.Nexus, "Tutorial3.rts", assetId2, "PF Model2 Description");
            _pfModelsToRemove.Add(pfModel2);

            //Two asset ids are provided as input parameter hence PFModels associated to the proveded asset ids should be retrieved in response
            pfModels = ProductionForecastService.GetPFModelsByAssetIds(userAssets.ToArray());
            Assert.AreEqual(2, pfModels.Count());
            comparePFModel(pfModel1, pfModels[0]);
            comparePFModel(pfModel2, pfModels[1]);

        }

        [TestCategory(TestCategories.ProductionForecastServiceTests), TestMethod]
        public void PFModelFileTest()
        {
            List<AssetDTO> Assets = new List<AssetDTO>();
            string AssetName = "Asset";
            string PFModelName = "PFModel";

            string PFModelFileName1 = "Tutorial3.rts";
            string PFModelFileName2 = "Tutorial4.rts";
            string Path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";

            //Getting content of File in string
            string PFModel1AsByteArrayString = Convert.ToBase64String(GetByteArray(Path, PFModelFileName1));
            string PFModel2AsByteArrayString = Convert.ToBase64String(GetByteArray(Path, PFModelFileName2));

            List<long> userAssets = new List<long>();

            //Creating Asset1
            SurfaceNetworkService.AddAsset(new AssetDTO() { Name = AssetName, Description = "Asset Description" });

            Assets = SurfaceNetworkService.GetAllAssets().ToList();
            long assetId = Assets.FirstOrDefault(x => x.Name == AssetName).Id;
            _assetsToRemove.AddRange(Assets);

            userAssets.Add(assetId);
            //Adding PFModel
            PFModelDTO pfModel = CreatePFModel(PFModelName, Enums.ProductionForecastReservoirOption.Eclipse, "Tutorial3.rts", assetId, "PF Model1 Description");
            _pfModelsToRemove.Add(pfModel);

            //Calling GetPFModelFileById and verifying that API is retrieving correct file.
            PFModelFileDTO pfModelFile = ProductionForecastService.GetPFModelFileById(pfModel.Id);
            Assert.AreEqual(PFModel1AsByteArrayString, pfModelFile.PFContents);

            //Now uploading PFModelFileName2 file in pfModel file
            pfModelFile.PFContents = PFModel2AsByteArrayString;
            pfModel.PFModelFile = pfModelFile;
            ProductionForecastService.UpdatePFModel(pfModel);

            //Now verifying the content of file is correct
            pfModelFile = ProductionForecastService.GetPFModelFileById(pfModel.Id);
            Assert.AreNotEqual(PFModel1AsByteArrayString, pfModelFile.PFContents);
            Assert.AreEqual(PFModel2AsByteArrayString, pfModelFile.PFContents);
        }

        [TestCategory(TestCategories.ProductionForecastServiceTests), TestMethod]
        public void PFModelsByReserviorAndAssetId()
        {
            List<AssetDTO> Assets = new List<AssetDTO>();
            string AssetName1 = "Asset1";
            string AssetName2 = "Asset2";
            string PFModelName1 = "PFModel1";
            string PFModelName2 = "PFModel2";
            string PFModelName3 = "PFModel3";
            string PFModelName4 = "PFModel4";

            //Creating Asset1
            SurfaceNetworkService.AddAsset(new AssetDTO() { Name = AssetName1, Description = "Asset1 Description" });
            //Creating Asset2
            SurfaceNetworkService.AddAsset(new AssetDTO() { Name = AssetName2, Description = "Asset2 Description" });

            Assets = SurfaceNetworkService.GetAllAssets().ToList();
            long assetId1 = Assets.FirstOrDefault(x => x.Name == AssetName1).Id;
            long assetId2 = Assets.FirstOrDefault(x => x.Name == AssetName2).Id;

            _assetsToRemove.AddRange(Assets);

            //Adding PFModel1 with Asset1 and PFReservoirOption Eclipse
            PFModelDTO pfModel1 = CreatePFModel(PFModelName1, Enums.ProductionForecastReservoirOption.Eclipse, "Tutorial3.rts", assetId1, "PF Model1 Description");
            _pfModelsToRemove.Add(pfModel1);

            //Adding PFModel2 with Asset2 and PFReservoirOption Eclipse
            PFModelDTO pfModel2 = CreatePFModel(PFModelName2, Enums.ProductionForecastReservoirOption.Eclipse, "Tutorial3.rts", assetId2, "PF Model2 Description");
            _pfModelsToRemove.Add(pfModel2);

            //Adding PFModel3 with Asset2 and PFReservoirOption Nexus
            PFModelDTO pfModel3 = CreatePFModel(PFModelName3, Enums.ProductionForecastReservoirOption.Nexus, "Tutorial3.rts", assetId2, "PF Model3 Description");
            _pfModelsToRemove.Add(pfModel3);

            //Adding PFModel4 with Asset1 and PFReservoirOption Eclipse
            PFModelDTO pfModel4 = CreatePFModel(PFModelName4, Enums.ProductionForecastReservoirOption.Eclipse, "Tutorial3.rts", assetId1, "PF Model4 Description");
            _pfModelsToRemove.Add(pfModel4);

            PFModelDTO[] pfModels = ProductionForecastService.GetPFModelsByReserviorAndAssetId(Enums.ProductionForecastReservoirOption.Eclipse.ToString(), assetId1.ToString());
            Assert.AreEqual(2, pfModels.Count());
            CompareObjectsUsingReflection(pfModel1, pfModels[0], "PF Model mismatch found", new HashSet<string> { "ChangeDate", "ChangeUser", "Id", "Asset" });
            CompareObjectsUsingReflection(pfModel4, pfModels[1], "PF Model mismatch found", new HashSet<string> { "ChangeDate", "ChangeUser", "Id", "Asset" });

            pfModels = ProductionForecastService.GetPFModelsByReserviorAndAssetId(Enums.ProductionForecastReservoirOption.Eclipse.ToString(), assetId2.ToString());
            Assert.AreEqual(1, pfModels.Count());
            CompareObjectsUsingReflection(pfModel2, pfModels[0], "PF Model mismatch found", new HashSet<string> { "ChangeDate", "ChangeUser", "Id", "Asset" });

            pfModels = ProductionForecastService.GetPFModelsByReserviorAndAssetId(Enums.ProductionForecastReservoirOption.Nexus.ToString(), assetId2.ToString());
            Assert.AreEqual(1, pfModels.Count());
            CompareObjectsUsingReflection(pfModel3, pfModels[0], "PF Model mismatch found", new HashSet<string> { "ChangeDate", "ChangeUser", "Id", "Asset" });
        }

        [TestCategory(TestCategories.ProductionForecastServiceTests), TestMethod]
        public void DeleteForecastScenarioTest()
        {
            // Create PF Modal and Scenario
            PFScenarioDTO pfScenarioObj = CreateSamplePFScenario();

            // Schedule PF Scenario
            ProductionForecastService.InsertPFScenarioScheduleManualRun(pfScenarioObj.Id.ToString());

            // Check : PF Scenario Scheduled
            var scheduledForecastScenarios = SurfaceNetworkService.GetScenarioSchedules(Enums.ScenarioType.Forecast);
            Assert.IsTrue(scheduledForecastScenarios.Count() == 1);

            // Remove PF Scenario
            ProductionForecastService.RemovePFScenario(pfScenarioObj.Id.ToString());

            // Check : PF Scenario removed
            var allForecastScenaios = ProductionForecastService.GetPFScenariosByAssetIds(new long[] { pfScenarioObj.AssetId });
            Assert.IsTrue(allForecastScenaios.Count() == 0);

            // Check : PF Scheduled Scenario removed
            scheduledForecastScenarios = SurfaceNetworkService.GetScenarioSchedules(Enums.ScenarioType.Forecast);
            Assert.IsTrue(scheduledForecastScenarios.Count() == 0);

            // Check : PF Model should not be removed
            var pfModels = ProductionForecastService.GetPFModelsByAssetIds(new long[] { pfScenarioObj.AssetId });
            Assert.IsTrue(pfScenarioObj.Model.Id == pfModels[0].Id);
            comparePFModel(pfScenarioObj.Model, pfModels[0]);

            // Check : SN Scenario should not be removed
            var snScenarioObj = SurfaceNetworkService.GetSNScenario(pfScenarioObj.SNScenarioId.ToString());
            Assert.IsTrue(pfScenarioObj.SNScenario.Id == snScenarioObj.Id);
            compareSNScenarios(pfScenarioObj.SNScenario, snScenarioObj);

            // Check : SN Model should not be removed
            var snModels = SurfaceNetworkService.GetSNModelsByAssetIds(new long[] { pfScenarioObj.AssetId });
            Assert.IsTrue(pfScenarioObj.SNScenario.SNModel.Id == snModels[0].Id);
        }

        [TestCategory(TestCategories.ProductionForecastServiceTests), TestMethod]
        public void DeleteForecastScenarioAndRunPFScenarioTest()
        {
            // Create PF Modal and Scenario
            PFScenarioDTO pfScenarioObj = CreateSamplePFScenario();

            // Schedule PF Scenario
            ProductionForecastService.InsertPFScenarioScheduleManualRun(pfScenarioObj.Id.ToString());

            // Run Scheduled PF Scenario
            SurfaceNetworkService.RunScheduledProductionForecastJobs();

            // Check : PF Scenario Scheduled History
            SNScenarioScheduleHistoryDTO[] scheduledForecastScenarioHistory = SurfaceNetworkService.GetScenarioScheduleHistory(Enums.ScenarioType.Forecast);
            Assert.IsTrue(scheduledForecastScenarioHistory.Count() == 1);

            // Remove PF Scenario
            ProductionForecastService.RemovePFScenario(pfScenarioObj.Id.ToString());

            // Check : PF Scenario removed
            var allForecastScenaios = ProductionForecastService.GetPFScenariosByAssetIds(new long[] { pfScenarioObj.AssetId });
            Assert.IsTrue(allForecastScenaios.Count() == 0);

            // Check : PF Scheduled Scenario removed
            var scheduledForecastScenarios = SurfaceNetworkService.GetScenarioSchedules(Enums.ScenarioType.Forecast);
            Assert.IsTrue(scheduledForecastScenarios.Count() == 0);

            // Check : PF Scheduled Scenario History removed
            scheduledForecastScenarioHistory = SurfaceNetworkService.GetScenarioScheduleHistoryByUserAssetIds(new long[] { pfScenarioObj.AssetId }, Enums.ScenarioType.SurfaceNetwork.ToString());
            Assert.IsTrue(scheduledForecastScenarioHistory.Count() == 0);

            // Check : PF Model should not be removed
            var pfModels = ProductionForecastService.GetPFModelsByAssetIds(new long[] { pfScenarioObj.AssetId });
            Assert.IsTrue(pfScenarioObj.Model.Id == pfModels[0].Id);
            comparePFModel(pfScenarioObj.Model, pfModels[0]);

            // Check : SN Scenario should not be removed
            var snScenarioObj = SurfaceNetworkService.GetSNScenario(pfScenarioObj.SNScenarioId.ToString());
            Assert.IsTrue(pfScenarioObj.SNScenario.Id == snScenarioObj.Id);
            compareSNScenarios(pfScenarioObj.SNScenario, snScenarioObj);

            // Check : SN Model should not be removed
            var snModels = SurfaceNetworkService.GetSNModelsByAssetIds(new long[] { pfScenarioObj.AssetId });
            Assert.IsTrue(pfScenarioObj.SNScenario.SNModel.Id == snModels[0].Id);
        }

        [TestCategory(TestCategories.ProductionForecastServiceTests), TestMethod]
        public void DeleteForecastModalTest()
        {
            // Create PF Modal and Scenario
            PFScenarioDTO pfScenarioObj = CreateSamplePFScenario();

            // Schedule PF Scenario
            ProductionForecastService.InsertPFScenarioScheduleManualRun(pfScenarioObj.Id.ToString());

            // Check : PF Scenario Scheduled
            var scheduledForecastScenarios = SurfaceNetworkService.GetScenarioSchedules(Enums.ScenarioType.Forecast);
            Assert.IsTrue(scheduledForecastScenarios.Count() == 1);

            // Remove PF Model
            ProductionForecastService.RemovePFModel(pfScenarioObj.Model.Id.ToString());

            // Check : PF Model removed
            var allPfModels = ProductionForecastService.GetPFModelsByAssetIds(new long[] { pfScenarioObj.AssetId });
            Assert.IsTrue(allPfModels.Count() == 0);

            // Check : PF Scenario removed
            var allForecastScenaios = ProductionForecastService.GetPFScenariosByAssetIds(new long[] { pfScenarioObj.AssetId });
            Assert.IsTrue(allForecastScenaios.Count() == 0);

            // Check : PF Scheduled Scenario removed
            scheduledForecastScenarios = SurfaceNetworkService.GetScenarioSchedules(Enums.ScenarioType.Forecast);
            Assert.IsTrue(scheduledForecastScenarios.Count() == 0);

            // Check : SN Scenario should not be removed
            var snScenarioObj = SurfaceNetworkService.GetSNScenario(pfScenarioObj.SNScenarioId.ToString());
            Assert.IsTrue(pfScenarioObj.SNScenario.Id == snScenarioObj.Id);
            compareSNScenarios(pfScenarioObj.SNScenario, snScenarioObj);

            // Check : SN Model should not be removed
            var snModels = SurfaceNetworkService.GetSNModelsByAssetIds(new long[] { pfScenarioObj.AssetId });
            Assert.IsTrue(pfScenarioObj.SNScenario.SNModel.Id == snModels[0].Id);
        }

        [TestCategory(TestCategories.ProductionForecastServiceTests), TestMethod]
        public void DeleteForecastModalAndRunPFScenarioTest()
        {
            // Create PF Modal and Scenario
            PFScenarioDTO pfScenarioObj = CreateSamplePFScenario();

            // Schedule PF Scenario
            ProductionForecastService.InsertPFScenarioScheduleManualRun(pfScenarioObj.Id.ToString());

            // Run Scheduled PF Scenario
            SurfaceNetworkService.RunScheduledProductionForecastJobs();

            // Check : PF Scenario Scheduled History
            SNScenarioScheduleHistoryDTO[] scheduledForecastScenarioHistory = SurfaceNetworkService.GetScenarioScheduleHistory(Enums.ScenarioType.Forecast);
            Assert.IsTrue(scheduledForecastScenarioHistory.Count() == 1);

            // Remove PF Model
            ProductionForecastService.RemovePFModel(pfScenarioObj.Model.Id.ToString());

            // Check : PF Model removed
            var allPfModels = ProductionForecastService.GetPFModelsByAssetIds(new long[] { pfScenarioObj.AssetId });
            Assert.IsTrue(allPfModels.Count() == 0);

            // Check : PF Scenario removed
            var allForecastScenaios = ProductionForecastService.GetPFScenariosByAssetIds(new long[] { pfScenarioObj.AssetId });
            Assert.IsTrue(allForecastScenaios.Count() == 0);

            // Check : PF Scheduled Scenario removed
            var scheduledForecastScenarios = SurfaceNetworkService.GetScenarioSchedules(Enums.ScenarioType.Forecast);
            Assert.IsTrue(scheduledForecastScenarios.Count() == 0);

            // Check : PF Scheduled Scenario History removed
            scheduledForecastScenarioHistory = SurfaceNetworkService.GetScenarioScheduleHistoryByUserAssetIds(new long[] { pfScenarioObj.AssetId }, Enums.ScenarioType.SurfaceNetwork.ToString());
            Assert.IsTrue(scheduledForecastScenarioHistory.Count() == 0);

            // Check : SN Scenario should not be removed
            var snScenarioObj = SurfaceNetworkService.GetSNScenario(pfScenarioObj.SNScenarioId.ToString());
            Assert.IsTrue(pfScenarioObj.SNScenario.Id == snScenarioObj.Id);
            compareSNScenarios(pfScenarioObj.SNScenario, snScenarioObj);

            // Check : SN Model should not be removed
            var snModels = SurfaceNetworkService.GetSNModelsByAssetIds(new long[] { pfScenarioObj.AssetId });
            Assert.IsTrue(pfScenarioObj.SNScenario.SNModel.Id == snModels[0].Id);
        }

        [TestCategory(TestCategories.ProductionForecastServiceTests), TestMethod]
        public void DeleteForeastModelRevokePermission()
        {
            CreateRole("TestRole");
            RemovePermissionsFromRole("TestRole", new PermissionId[] { PermissionId.DeleteForecastModel });
            UpdateUserWithGivenRole("TestRole");

            // Create PF Modal and Scenario
            PFScenarioDTO pfScenarioObj = CreateSamplePFScenario();

            // Schedule PF Scenario
            ProductionForecastService.InsertPFScenarioScheduleManualRun(pfScenarioObj.Id.ToString());

            // Check : PF Scenario Scheduled
            var scheduledForecastScenarios = SurfaceNetworkService.GetScenarioSchedules(Enums.ScenarioType.Forecast);
            Assert.IsTrue(scheduledForecastScenarios.Count() == 1);

            try
            {
                // Remove PF Model
                ProductionForecastService.RemovePFModel(pfScenarioObj.Model.Id.ToString());
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
                Assert.IsTrue(e.Message.Contains("Forbidden"));
            }

            // Check : PF Model should not be removed
            var allPfModels = ProductionForecastService.GetPFModelsByAssetIds(new long[] { pfScenarioObj.AssetId });
            Assert.IsTrue(allPfModels[0].Id == pfScenarioObj.Model.Id);
            comparePFModel(pfScenarioObj.Model, allPfModels[0]);

            // Check : PF Scenario should not be removed
            var allForecastScenaios = ProductionForecastService.GetPFScenariosByAssetIds(new long[] { pfScenarioObj.AssetId });
            Assert.IsTrue(pfScenarioObj.Id == allForecastScenaios[0].Id);
            comparePFScenarios(pfScenarioObj, allForecastScenaios[0]);

            // Check : PF Scheduled Scenario should not be removed
            scheduledForecastScenarios = SurfaceNetworkService.GetScenarioSchedules(Enums.ScenarioType.Forecast);
            Assert.IsTrue(scheduledForecastScenarios.Count() == 1);

            // Check : SN Scenario should not be removed
            var snScenarioObj = SurfaceNetworkService.GetSNScenario(pfScenarioObj.SNScenarioId.ToString());
            Assert.IsTrue(pfScenarioObj.SNScenario.Id == snScenarioObj.Id);
            compareSNScenarios(pfScenarioObj.SNScenario, snScenarioObj);

            // Check : SN Model should not be removed
            var snModels = SurfaceNetworkService.GetSNModelsByAssetIds(new long[] { pfScenarioObj.AssetId });
            Assert.IsTrue(pfScenarioObj.SNScenario.SNModel.Id == snModels[0].Id);
        }

        [TestCategory(TestCategories.ProductionForecastServiceTests), TestMethod]
        public void DeleteForeastScenarioRevokePermission()
        {
            CreateRole("TestRole");
            RemovePermissionsFromRole("TestRole", new PermissionId[] { PermissionId.DeleteForecastScenario });
            UpdateUserWithGivenRole("TestRole");

            // Create PF Modal and Scenario
            PFScenarioDTO pfScenarioObj = CreateSamplePFScenario();

            // Schedule PF Scenario
            ProductionForecastService.InsertPFScenarioScheduleManualRun(pfScenarioObj.Id.ToString());

            // Check : PF Scenario Scheduled
            var scheduledForecastScenarios = SurfaceNetworkService.GetScenarioSchedules(Enums.ScenarioType.Forecast);
            Assert.IsTrue(scheduledForecastScenarios.Count() == 1);

            try
            {
                // Remove PF Model
                ProductionForecastService.RemovePFScenario(pfScenarioObj.Id.ToString());
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
                Assert.IsTrue(e.Message.Contains("Forbidden"));
            }

            // Check : PF Model should not be removed
            var allPfModels = ProductionForecastService.GetPFModelsByAssetIds(new long[] { pfScenarioObj.AssetId });
            Assert.IsTrue(allPfModels[0].Id == pfScenarioObj.Model.Id);
            comparePFModel(pfScenarioObj.Model, allPfModels[0]);

            // Check : PF Scenario should not be removed
            var allForecastScenaios = ProductionForecastService.GetPFScenariosByAssetIds(new long[] { pfScenarioObj.AssetId });
            Assert.IsTrue(pfScenarioObj.Id == allForecastScenaios[0].Id);
            comparePFScenarios(pfScenarioObj, allForecastScenaios[0]);

            // Check : PF Scheduled Scenario should not be removed
            scheduledForecastScenarios = SurfaceNetworkService.GetScenarioSchedules(Enums.ScenarioType.Forecast);
            Assert.IsTrue(scheduledForecastScenarios.Count() == 1);

            // Check : SN Scenario should not be removed
            var snScenarioObj = SurfaceNetworkService.GetSNScenario(pfScenarioObj.SNScenarioId.ToString());
            Assert.IsTrue(pfScenarioObj.SNScenario.Id == snScenarioObj.Id);
            compareSNScenarios(pfScenarioObj.SNScenario, snScenarioObj);

            // Check : SN Model should not be removed
            var snModels = SurfaceNetworkService.GetSNModelsByAssetIds(new long[] { pfScenarioObj.AssetId });
            Assert.IsTrue(pfScenarioObj.SNScenario.SNModel.Id == snModels[0].Id);
        }


        // This API related to the FRWM 6149- Handle deselection of a network scenario for production forecasting
        [TestCategory(TestCategories.ProductionForecastServiceTests), TestMethod]
        public void DeselectionNetworkScenario()
        {
            string pfScenarioName = "Test PF Scenario";
            List<long> userAssets = new List<long>();

            CreateRole("TestRole");
            UpdateUserWithGivenRole("TestRole");

            List<AssetDTO> assetIds = CreateAsset(1);
            // Create PF Modal and Scenario
            SNScenarioDTO snScenarioObj = CreateScenario("SNModel", "SNScenarioName", assetIds.ElementAt(0).Id, "SN Scenario Description", true);
            PFModelDTO pfModel1 = CreatePFModel("PFModelName1", Enums.ProductionForecastReservoirOption.Eclipse, "Tutorial3.rts", assetIds.ElementAt(0).Id, "PF Model1 Description");
            _pfModelsToRemove.Add(pfModel1);


            //Create PFScenario 1
            CreateForecastScenario(pfScenarioName, snScenarioObj, pfModel1, "PFScenario Description1", assetIds.ElementAt(0));
            //Verifying that ForeSite Scenarios associated to Asset1 are retrieved                               

            var allForecastScenaios = ProductionForecastService.GetPFScenariosByAssetIds(new long[] { assetIds.ElementAt(0).Id });
            Assert.IsTrue(allForecastScenaios.Count() == 1);

            var pfScenarioObj = allForecastScenaios.FirstOrDefault(x => x.Name == pfScenarioName);
            Assert.IsNotNull(pfScenarioObj);
            _pfScenariosToRemove.Add(pfScenarioObj);

            var snScenario1 = SurfaceNetworkService.GetSNScenariosByAssetIds(new long[] { assetIds.ElementAt(0).Id });

            //Updating Forcast Scenario 
            snScenario1[0].IsForecastScenario = false;
            SurfaceNetworkService.UpdateSNScenario(snScenario1[0]);
            var snScenario = SurfaceNetworkService.GetSNScenariosByAssetIds(new long[] { assetIds.ElementAt(0).Id });
            //assertion for IsForecastScenario
            Assert.IsFalse(snScenario[0].IsForecastScenario);

            var afterUpdateAllForecastScenario = ProductionForecastService.GetPFScenariosByAssetIds(new long[] { assetIds.ElementAt(0).Id });
            Assert.IsTrue(afterUpdateAllForecastScenario.Count() == 1);
            Assert.AreEqual("PFModelName1", afterUpdateAllForecastScenario[0].Model.Name);
            Assert.AreEqual("PFScenario Description1", afterUpdateAllForecastScenario[0].Description);
            Assert.AreEqual("SNScenarioName", afterUpdateAllForecastScenario[0].SNScenario.Name);


            SNScenarioDTO[] pfEnabledSNScenarios = ProductionForecastService.GetSNScenariosByAssetIds(new long[] { assetIds.ElementAt(0).Id });
            Assert.AreEqual(0, pfEnabledSNScenarios.Count());

        }

        /// <summary>
        /// FRWM-6047
        /// In this test, we are verifying that Forecast results populated in database are retrieving successfully by GetPFScenarioWellResultsByPFScenarioRunIdAndTimestep & GetPFScenarioWellResultsByPFScenarioRunIdAndWellName API
        /// We are doing validation of Actual results with expected results which are stored in .json file
        /// </summary>
        [Ignore]
        [TestCategory(TestCategories.ProductionForecastServiceTests), TestMethod, Timeout(2400000)]
        public void PFOptimizationResultRetrieval()
        {
            string path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";
            SystemSettingDTO spSettingDto = SettingService.GetSystemSettingByName(SettingServiceStringConstants.FORECAST_DEFAULT_DIRECTORY);
            string foreCastDefaultDirectory = spSettingDto.StringValue;

            //Create an asset
            List<AssetDTO> assetIds = CreateAsset(1);
            var options = new ModelFileOptionDTO
            {
                CalibrationMethod = CalibrationMethodId.LFactor,
                OptionalUpdate = new long[]
                   {
                        (long) OptionalUpdates.UpdateWCT_WGR,
                        (long) OptionalUpdates.UpdateGOR_CGR
                   }
            };

            //Create the wells in ForeSite and assign them to the asset created above
            WellDTO well1 = AddNonRRLWellGeneral("Well1", WellTypeId.NF, WellFluidType.BlackOil, "1", assetIds.ElementAt(0).Id);
            WellDTO well2 = AddNonRRLWellGeneral("Well2", WellTypeId.NF, WellFluidType.BlackOil, "1", assetIds.ElementAt(0).Id);
            WellDTO well3 = AddNonRRLWellGeneral("Well3", WellTypeId.NF, WellFluidType.BlackOil, "1", assetIds.ElementAt(0).Id);
            WellDTO well4 = AddNonRRLWellGeneral("Well4", WellTypeId.NF, WellFluidType.BlackOil, "1", assetIds.ElementAt(0).Id);
            WellDTO well5 = AddNonRRLWellGeneral("Well5", WellTypeId.NF, WellFluidType.BlackOil, "1", assetIds.ElementAt(0).Id);


            AddNonRRLModelFile(well1, "Well1.wflx", options.CalibrationMethod, options.OptionalUpdate);
            AddNonRRLModelFile(well2, "Well2.wflx", options.CalibrationMethod, options.OptionalUpdate);
            AddNonRRLModelFile(well3, "Well3.wflx", options.CalibrationMethod, options.OptionalUpdate);
            AddNonRRLModelFile(well4, "Well4.wflx", options.CalibrationMethod, options.OptionalUpdate);
            AddNonRRLModelFile(well5, "Well5.wflx", options.CalibrationMethod, options.OptionalUpdate);


            //Add a surface network model.
            SNModelAndModelFileDTO modelAndFile = new SNModelAndModelFileDTO();
            modelAndFile.SNModel = new SNModelDTO() { Name = "Recycle Network", AssetId = assetIds.ElementAt(0).Id };

            SNModelFileDTO modelFile = new SNModelFileDTO();
            modelFile.Comments = "Test Model File;";
            byte[] fileAsByteArray = GetByteArray(path, "TutorialExample2.reo");
            Assert.IsNotNull(fileAsByteArray);
            modelFile.Base64Contents = Convert.ToBase64String(fileAsByteArray);
            modelAndFile.SNModelFile = modelFile;

            //Create Initial Model And ModelFile
            SurfaceNetworkService.SaveReOFile(modelAndFile);

            //Map the ForeSite wells to the Reo wells within the surface network model.
            WellDTO[] wells = WellService.GetWellsByUserAssetIds(new long[] { assetIds.ElementAt(0).Id });
            SNWellMappingDTO[] wellmappings = SurfaceNetworkService.GetSNWellMappingsByAssetIds(new long[] { assetIds.ElementAt(0).Id });
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
            var allModels = SurfaceNetworkService.GetSNModelsByAssetIds(new long[] { assetIds.ElementAt(0).Id });
            SNModelDTO model = allModels?.FirstOrDefault(m => m.Name.Equals("Recycle Network"));
            _snModelsToRemove.Add(model);
            Assert.IsNotNull(model);
            SurfaceNetworkService.UpdateSNWellMappings(wellmappings);

            //Create SN scenario
            SNScenarioDTO addScenarioObj = new SNScenarioDTO()
            {
                Name = "Recycle Network SNScenario",
                Description = "Scenario Description",
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
            SNScenarioDTO scenario = allScenarios?.FirstOrDefault(sn => sn.Name.Equals("Recycle Network SNScenario"));
            Assert.IsNotNull(scenario);
            _snScenariosToRemove.Add(scenario);

            SurfaceNetworkService.InsertSNScenarioScheduleManualRun(scenario.Id.ToString());

            //Create PFModel
            PFModelDTO PFModel = CreatePFModel("Recycle PFModel", ProductionForecastReservoirOption.Eclipse, "Tutorial2.rts", assetIds.ElementAt(0).Id, "PF Model Description");
            _pfModelsToRemove.Add(PFModel);

            //Create PFScenario
            CreateForecastScenario("Recycle PFScenario", scenario, PFModel, "PF Scenario Description", assetIds.ElementAt(0));
            PFScenarioDTO[] PFScenarios = ProductionForecastService.GetPFScenariosByAssetIds(new long[] { assetIds.ElementAt(0).Id });
            PFScenarioDTO PFscenario = PFScenarios?.FirstOrDefault(sn => sn.Name.Equals("Recycle PFScenario"));
            Assert.IsNotNull(scenario);

            //Schedule and run Optimization Job
            ProductionForecastService.InsertPFScenarioScheduleManualRun(PFscenario.Id.ToString());
            RunAnalysisTaskScheduler("-runScenarioScheduler");
            Trace.WriteLine("-runScenarioScheduler completed");
            _directoriesToRemove.Add(foreCastDefaultDirectory);
            SNScenarioScheduleHistoryDTO[] snScenarioSchedulesHistory = { };
            while (snScenarioSchedulesHistory.Count() == 0)
            {
                snScenarioSchedulesHistory = SurfaceNetworkService.GetScenarioScheduleHistoryByUserAssetIds(new long[] { assetIds.ElementAt(0).Id }, ScenarioType.Forecast.ToString());
                Thread.Sleep(30000);

            }
            SNScenarioScheduleHistoryDTO snScenarioScheduleHistory = snScenarioSchedulesHistory.FirstOrDefault(x => x.PFScenarioId == PFscenario.Id);

            //Verify that 6 well Model files are stored in ForeSite Default Directory
            string forecastDirectoryPath = foreCastDefaultDirectory + "//" + snScenarioScheduleHistory.Id;
            string[] wellModelFiles = Directory.GetFiles(forecastDirectoryPath, "*.wflx");
            Assert.AreEqual(5, wellModelFiles.Count());
            WellDTO[] Wells = WellService.GetAllWells();
            foreach (WellDTO well in Wells)
            {
                bool modelFileFound = false;
                foreach (string wellModelFile in wellModelFiles)
                {
                    if (wellModelFile.Contains(well.Name + ".wflx"))
                    {
                        modelFileFound = true;
                        break;
                    }
                }
                Assert.IsTrue(modelFileFound);
            }
            Trace.WriteLine("WellFlo files are present in Default Forecast Directory");

            //Verify that 1 REO Model file is stored in ForeSite Default Directory
            SNModelFileDTO[] snModelFiles = SurfaceNetworkService.GetSNModelFilesByModelId(model.Id.ToString());
            string[] reoFile = Directory.GetFiles(forecastDirectoryPath, "*.reo");
            Assert.AreEqual(1, reoFile.Count());
            StringAssert.Contains(reoFile[0], snModelFiles[1].Id.ToString() + ".reo");
            Trace.WriteLine("ReO Model files are present in Default Forecast Directory");

            //Verify that 1 Reo ForeCast file is stored in ForeSite Default Directory
            string[] rtsFile = Directory.GetFiles(forecastDirectoryPath, "*.rts");
            Assert.AreEqual(1, rtsFile.Count());
            StringAssert.Contains(rtsFile[0], PFscenario.Name.ToString() + ".rts");
            Trace.WriteLine(".rts files are present in Default Forecast Directory");

            string FileName = "Tutorial2_PFScenarioWellResultsArray.json";
            PFScenarioResultsArrayUnitsDTO expectedPFScenarioWellResultsArrayDTO = JsonConvert.DeserializeObject<PFScenarioResultsArrayUnitsDTO>(GetJsonString(path + FileName));

            PFScenarioRunHeaderDTO[] pfScenarioRunHeader = ProductionForecastService.GetPFScenarioRunHeaderArrayByScenarioId(PFscenario.Id.ToString());
            PFScenarioResultsArrayUnitsDTO actualPFScenarioWellResultsArray = ProductionForecastService.GetPFScenarioWellResultsByPFScenarioRunId(pfScenarioRunHeader[0].Id.ToString());

            VerifyPFResults(expectedPFScenarioWellResultsArrayDTO, actualPFScenarioWellResultsArray);

            VerifyPFResultsByTimeStep(path, "Tutoial2_USunit_Timestep", actualPFScenarioWellResultsArray, pfScenarioRunHeader[0]);

            VerifyPFResultsByWellName(path, "Tutoial2_USunit_Well", wells, pfScenarioRunHeader[0]);

            ChangeUnitSystemUserSetting("Metric");

            VerifyPFResultsByTimeStep(path, "Tutoial2_Metricunit_Timestep", actualPFScenarioWellResultsArray, pfScenarioRunHeader[0]);

            VerifyPFResultsByWellName(path, "Tutoial2_Metricunit_Well", wells, pfScenarioRunHeader[0]);

        }

        public void VerifyPFResultsByTimeStep(string Path, string FileName, PFScenarioResultsArrayUnitsDTO PFScenarioWellResultsArray, PFScenarioRunHeaderDTO pfScenarioRunHeader)
        {

            for (int i = 0; i < PFScenarioWellResultsArray.Values.Count(); i++)
            {
                string fileName = FileName + (i + 1).ToString() + ".json";
                PFScenarioResultsArrayUnitsDTO actualPFScenarioWellResultsArrayByTimeStep = ProductionForecastService.GetPFScenarioWellResultsByPFScenarioRunIdAndTimestep(pfScenarioRunHeader.Id.ToString(), PFScenarioWellResultsArray.Values[i].Id.ToString());
                PFScenarioResultsArrayUnitsDTO expectedPFScenarioWellResultsArrayByTimeStep = JsonConvert.DeserializeObject<PFScenarioResultsArrayUnitsDTO>(GetJsonString(Path + fileName));
                VerifyPFResults(expectedPFScenarioWellResultsArrayByTimeStep, actualPFScenarioWellResultsArrayByTimeStep);
                Trace.WriteLine("Verification completed for timestep :" + PFScenarioWellResultsArray.Values[i].Timestep.ToString());
            }
        }

        public void VerifyPFResultsByWellName(string Path, string FileName, WellDTO[] wells, PFScenarioRunHeaderDTO pfScenarioRunHeader)
        {

            for (int i = 0; i < wells.Count(); i++)
            {
                string fileName = FileName + (i + 1).ToString() + ".json";
                PFScenarioResultsArrayUnitsDTO actualPFScenarioWellResultsArrayByWellName = ProductionForecastService.GetPFScenarioWellResultsByPFScenarioRunIdAndWellName(pfScenarioRunHeader.Id.ToString(), wells[i].Name);
                PFScenarioResultsArrayUnitsDTO expectedPFScenarioWellResultsArrayByWellName = JsonConvert.DeserializeObject<PFScenarioResultsArrayUnitsDTO>(GetJsonString(Path + fileName));
                VerifyPFResults(expectedPFScenarioWellResultsArrayByWellName, actualPFScenarioWellResultsArrayByWellName);
                Trace.WriteLine("Verification completed for Well : " + wells[i].Name);
            }
        }

        public void VerifyPFResults(PFScenarioResultsArrayUnitsDTO expectedObject, PFScenarioResultsArrayUnitsDTO actualObject)
        {
            CompareObjectsUsingReflection(expectedObject.Units.CumulativeGasInjectedVolume, actualObject.Units.CumulativeGasInjectedVolume, "CumulativeGasInjectedVolume Units are not matching");
            CompareObjectsUsingReflection(expectedObject.Units.CumulativeGasVolume, actualObject.Units.CumulativeGasVolume, "CumulativeGasVolume Units are not matching");
            CompareObjectsUsingReflection(expectedObject.Units.CumulativeOilVolume, actualObject.Units.CumulativeOilVolume, "CumulativeOilVolume Units are not matching");
            CompareObjectsUsingReflection(expectedObject.Units.CumulativeWaterInjectedVolume, actualObject.Units.CumulativeWaterInjectedVolume, "CumulativeWaterInjectedVolume Units are not matching");
            CompareObjectsUsingReflection(expectedObject.Units.CumulativeWaterVolume, actualObject.Units.CumulativeWaterVolume, "CumulativeWaterVolume Units are not matching");
            CompareObjectsUsingReflection(expectedObject.Units.FlowingBottomHolePressure, actualObject.Units.FlowingBottomHolePressure, "FlowingBottomHolePressure Units are not matching");
            CompareObjectsUsingReflection(expectedObject.Units.GasInjectionRate, actualObject.Units.GasInjectionRate, "GasInjectionRate Units are not matching");
            CompareObjectsUsingReflection(expectedObject.Units.GasRate, actualObject.Units.GasRate, "GasRate Units are not matching");
            CompareObjectsUsingReflection(expectedObject.Units.OilRate, actualObject.Units.OilRate, "OilRate Units are not matching");
            CompareObjectsUsingReflection(expectedObject.Units.Pressure, actualObject.Units.Pressure, "Pressure Units are not matching"); // Added as scope of FRWM-6777
            CompareObjectsUsingReflection(expectedObject.Units.WaterInjectionRate, actualObject.Units.WaterInjectionRate, "WaterInjectionRate Units are not matching");
            CompareObjectsUsingReflection(expectedObject.Units.WaterRate, actualObject.Units.WaterRate, "WaterRate Units are not matching");
            Assert.AreEqual(expectedObject.Values.Count(), actualObject.Values.Count());
            for (int i = 0; i < expectedObject.Values.Count(); i++)
            {
                CompareObjectsUsingReflection(expectedObject.Values[i], actualObject.Values[i], "Values are not matching", new HashSet<string> { "ChangeUserId", "ChangeTime", "Id", "PFScenarioRun" }, 0.1);
            }
        }

        public void ProdForecastConfiguration(string Path, double SleepTime)
        {
            SystemSettingDTO spSettingDto = SettingService.GetSystemSettingByName(SettingServiceStringConstants.FORECAST_DEFAULT_DIRECTORY);
            string foreCastDefaultDirectory = spSettingDto.StringValue;

            //Create an asset
            List<AssetDTO> assetIds = CreateAsset(1);
            var options = new ModelFileOptionDTO
            {
                CalibrationMethod = CalibrationMethodId.LFactor,
                OptionalUpdate = new long[]
                   {
                        (long) OptionalUpdates.UpdateWCT_WGR,
                        (long) OptionalUpdates.UpdateGOR_CGR
                   }
            };

            //Create the wells in ForeSite and assign them to the asset created above
            WellDTO well1 = AddNonRRLWellGeneral("GLWell", WellTypeId.GLift, WellFluidType.BlackOil, "1", assetIds.ElementAt(0).Id);
            WellDTO well2 = AddNonRRLWellGeneral("OilWell_01", WellTypeId.NF, WellFluidType.BlackOil, "1", assetIds.ElementAt(0).Id);
            WellDTO well3 = AddNonRRLWellGeneral("OilWell_02", WellTypeId.NF, WellFluidType.BlackOil, "1", assetIds.ElementAt(0).Id);
            WellDTO well4 = AddNonRRLWellGeneral("OilWell_03", WellTypeId.NF, WellFluidType.BlackOil, "1", assetIds.ElementAt(0).Id);

            AddNonRRLModelFile(well1, "GLWell.wflx", options.CalibrationMethod, options.OptionalUpdate);
            AddNonRRLModelFile(well2, "OilWell_01.wflx", options.CalibrationMethod, options.OptionalUpdate);
            AddNonRRLModelFile(well3, "OilWell_02.wflx", options.CalibrationMethod, options.OptionalUpdate);
            AddNonRRLModelFile(well4, "OilWell_03.wflx", options.CalibrationMethod, options.OptionalUpdate);
            Trace.WriteLine("Four Wells are added for Tutorial3");

            //Add a surface network model.
            SNModelAndModelFileDTO modelAndFile = new SNModelAndModelFileDTO();
            modelAndFile.SNModel = new SNModelDTO() { Name = "Tutorial Example 3", AssetId = assetIds.ElementAt(0).Id };

            SNModelFileDTO modelFile = new SNModelFileDTO();
            modelFile.Comments = "Test Model File;";
            byte[] fileAsByteArray = GetByteArray(Path, "TutorialExample3.reo");
            Assert.IsNotNull(fileAsByteArray);
            modelFile.Base64Contents = Convert.ToBase64String(fileAsByteArray);
            modelAndFile.SNModelFile = modelFile;

            //Create Initial Model And ModelFile
            SurfaceNetworkService.SaveReOFile(modelAndFile);
            Trace.WriteLine("One ReO File is added for Tutorial3");

            //Map the ForeSite wells to the Reo wells within the surface network model.
            WellDTO[] wells = WellService.GetWellsByUserAssetIds(new long[] { assetIds.ElementAt(0).Id });
            SNWellMappingDTO[] wellmappings = SurfaceNetworkService.GetSNWellMappingsByAssetIds(new long[] { assetIds.ElementAt(0).Id });
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
            var allModels = SurfaceNetworkService.GetSNModelsByAssetIds(new long[] { assetIds.ElementAt(0).Id });
            SNModelDTO model = allModels?.FirstOrDefault(m => m.Name.Equals("Tutorial Example 3"));
            _snModelsToRemove.Add(model);
            Assert.IsNotNull(model);
            SurfaceNetworkService.UpdateSNWellMappings(wellmappings);
            Trace.WriteLine("Well Mapping is updated for Tutorial3");

            //Create SN scenario
            SNScenarioDTO addScenarioObj = new SNScenarioDTO()
            {
                Name = "TutorialExample3SNScenario",
                Description = "TutorialExample3 Description",
                SNModelId = model.Id,
                ColorCode = "#FF0000",
                AllowableScenario = true,
                Implementable = false,
                ModelMatch = false,
                OptimumScenario = false,
                IsForecastScenario = true
            };
            SurfaceNetworkService.AddSNScenario(addScenarioObj);
            Trace.WriteLine("Network Scenario created for Tutorial3");
            var allScenarios = SurfaceNetworkService.GetSNScenariosBySNModel(model.Id.ToString());
            SNScenarioDTO scenario = allScenarios?.FirstOrDefault(sn => sn.Name.Equals("TutorialExample3SNScenario"));
            Assert.IsNotNull(scenario);
            _snScenariosToRemove.Add(scenario);

            SurfaceNetworkService.InsertSNScenarioScheduleManualRun(scenario.Id.ToString());
            Trace.WriteLine("Network Scenario scheduled for Tutorial3");

            bool IsPFFeatureLicensed = ProductionForecastService.IsPFFeatureLicensed();
            Assert.IsTrue(IsPFFeatureLicensed, "PF Feature is not licensed.");
            Trace.WriteLine("PF Feature license checking successful.");

            //Create PFModel
            PFModelDTO PFModel = CreatePFModel("TutorialExample3 PFModel", ProductionForecastReservoirOption.Eclipse, "Tutorial3_2015.rts", assetIds.ElementAt(0).Id, "TutorialExample3 Model Description");
            _pfModelsToRemove.Add(PFModel);
            Trace.WriteLine("One RTS file is added for Tutorial3");

            //Create PFScenario
            CreateForecastScenario("TutorialExample3PFScenario", scenario, PFModel, "PF Scenario Description", assetIds.ElementAt(0));
            Trace.WriteLine("Production Forecast Scenario created for Tutorial3");

            PFScenarioDTO[] PFScenarios = ProductionForecastService.GetPFScenariosByAssetIds(new long[] { assetIds.ElementAt(0).Id });
            PFScenarioDTO PFscenario = PFScenarios?.FirstOrDefault(sn => sn.Name.Equals("TutorialExample3PFScenario"));
            Assert.IsNotNull(scenario);

            //Schedule and run Optimization Job
            ProductionForecastService.InsertPFScenarioScheduleManualRun(PFscenario.Id.ToString());
            Trace.WriteLine("Production Forecast Scenario scheduled for Tutorial3");
            RunAnalysisTaskScheduler("-runScenarioScheduler");
            Trace.WriteLine("-runScenarioScheduler completed for Tutorial3");
            _directoriesToRemove.Add(foreCastDefaultDirectory);

            // Sleep for 45 Minutes to get Tutorial 3 Forecast results in ScenarioScheduleHistory table.
            Thread.Sleep(TimeSpan.FromMinutes(SleepTime));
            SNScenarioScheduleHistoryDTO[] snScenarioSchedulesHistory = { };
            while (snScenarioSchedulesHistory.Count() == 0)
            {
                snScenarioSchedulesHistory = SurfaceNetworkService.GetScenarioScheduleHistoryByUserAssetIds(new long[] { assetIds.ElementAt(0).Id }, ScenarioType.Forecast.ToString());
            }
            SNScenarioScheduleHistoryDTO snScenarioScheduleHistory = snScenarioSchedulesHistory.FirstOrDefault(x => x.PFScenarioId == PFscenario.Id);

            //Verify that 4 well Model files are stored in ForeSite Default Directory
            string forecastDirectoryPath = foreCastDefaultDirectory + "//" + snScenarioScheduleHistory.Id;
            string[] wellModelFiles = Directory.GetFiles(forecastDirectoryPath, "*.wflx");
            Assert.AreEqual(4, wellModelFiles.Count());
            WellDTO[] Wells = WellService.GetAllWells();
            foreach (WellDTO well in Wells)
            {
                bool modelFileFound = false;
                foreach (string wellModelFile in wellModelFiles)
                {
                    if (wellModelFile.Contains(well.Name + ".wflx"))
                    {
                        modelFileFound = true;
                        break;
                    }
                }
                Assert.IsTrue(modelFileFound);
            }
            Trace.WriteLine("All 4 WellFlo files are present in Default Forecast Directory");

            //Verify that 1 REO Model file is stored in ForeSite Default Directory
            SNModelFileDTO[] snModelFiles = SurfaceNetworkService.GetSNModelFilesByModelId(model.Id.ToString());
            string[] reoFile = Directory.GetFiles(forecastDirectoryPath, "*.reo");
            Assert.AreEqual(1, reoFile.Count());
            StringAssert.Contains(reoFile[0], snModelFiles[1].Id.ToString() + ".reo");
            Trace.WriteLine("ReO Model files are present in Default Forecast Directory");

            //Verify that 1 Reo ForeCast file is stored in ForeSite Default Directory
            string[] rtsFile = Directory.GetFiles(forecastDirectoryPath, "*.rts");
            Assert.AreEqual(1, rtsFile.Count());
            StringAssert.Contains(rtsFile[0], PFscenario.Name.ToString() + ".rts");
            Trace.WriteLine(".rts files are present in Default Forecast Directory");
        }

        /// <summary>
        /// FRWM-6777 Get Sinks and Contraints Results | FRWM-6559 Export to Excel Functionality | FRWM-6826 Get Reservoir Results
        /// In this test, we are verifying that Forecast results populated in database are retrieving successfully.
        /// We are doing validation of Actual results with expected results which are stored in .json file
        /// There are 4 types of .Json Files(1-2005/ 2-2006 / 3-2007 / 4-2008 Forecast timestep results).
        /// </summary>
        [Ignore]
        [TestCategory(TestCategories.ProductionForecastServiceTests), TestMethod, Timeout(7000000)]
        public void PFOptimization_Wellhead_Sink_Constraints_Reservoir_Export_ResultsRetrieval()
        {
            string path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";

            ProdForecastConfiguration(path, 65);

            var allAssets = SurfaceNetworkService.GetAllAssets().ToList();
            AssetDTO assetIds = allAssets?.FirstOrDefault(a => a.Name.Equals("Asset Test1"));
            PFScenarioDTO[] PFScenarios = ProductionForecastService.GetPFScenariosByAssetIds(new long[] { assetIds.Id });
            PFScenarioDTO PFscenario = PFScenarios?.FirstOrDefault(sn => sn.Name.Equals("TutorialExample3PFScenario"));

            string FileName = "Tutorial3_PFScenarioResultsArray.json";
            PFScenarioResultsArrayUnitsDTO expectedPFScenarioWellResultsArrayDTO = JsonConvert.DeserializeObject<PFScenarioResultsArrayUnitsDTO>(GetJsonString(path + FileName));
            PFScenarioRunHeaderDTO[] pfScenarioRunHeader = ProductionForecastService.GetPFScenarioRunHeaderArrayByScenarioId(PFscenario.Id.ToString());
            PFScenarioResultsArrayUnitsDTO actualPFScenarioWellResultsArray = ProductionForecastService.GetPFScenarioWellResultsByPFScenarioRunId(pfScenarioRunHeader[0].Id.ToString());

            // Verify Units and no. of timesteps and its values.
            VerifyPFResults(expectedPFScenarioWellResultsArrayDTO, actualPFScenarioWellResultsArray);
            // Wellhead Results verification 
            VerifyPFWellheadResultsByTimeStep(path, "Tutorial3_Wellheads_USunit_Timestep", actualPFScenarioWellResultsArray, pfScenarioRunHeader[0]);
            // Constraints Results verification
            VerifyPFConstraintsResultsByTimeStep(path, "Tutorial3_Constraints_USunit_Timestep", actualPFScenarioWellResultsArray, pfScenarioRunHeader[0]);
            // Sinks Results verification
            VerifyPFSinksResultsByTimeStep(path, "Tutorial3_Sinks_USunit_Timestep", actualPFScenarioWellResultsArray, pfScenarioRunHeader[0]);
            // Reservoir Results verification - FRWM-6826
            VerifyPFReservoirResultsByTimeStep(path, "Tutorial3_Reservoir_USunit_Timestep", actualPFScenarioWellResultsArray, pfScenarioRunHeader[0]);

            // Verify Export Functionality - FRWM-6559 (User should be able to view results for all timesteps and Export to Excel)
            string ExportedResult = "Tutorial3_2015_ExportFunctionality.json";
            PFScenarioResultsArrayUnitsDTO expectedExportToExcelFunctionality = JsonConvert.DeserializeObject<PFScenarioResultsArrayUnitsDTO>(GetJsonString(path + ExportedResult));
            Assert.IsNotNull(expectedExportToExcelFunctionality, "Failed to Deserialize JSON file(Tutorial3_2015_ExportFunctionality).");

            PFScenarioResultsArrayUnitsDTO actualExportToExcelFunctionality = ProductionForecastService.GetProductionForecastResultsByRunId(pfScenarioRunHeader[0].Id.ToString());
            Assert.IsNotNull(actualExportToExcelFunctionality, "Failed to export the production forecast results.");

            VerifyPFResults(expectedExportToExcelFunctionality, actualExportToExcelFunctionality);
            Trace.WriteLine("Verified Export to Excel functionality of Production Forecast");

            // Verify PF Run Message Status
            string RunMessageStatus = "Tutorial3_2015_RunMessages.json";
            PFScenarioRunStatusDTO expectedPFRunMessageStatus = JsonConvert.DeserializeObject<PFScenarioRunStatusDTO>(GetJsonString(path + RunMessageStatus));
            PFScenarioRunStatusDTO actualPFRunMessageStatus = ProductionForecastService.GetPFScenarioRunMessagesByPFScenarioRunId(pfScenarioRunHeader[0].Id.ToString());
            CompareObjectsUsingReflection(expectedPFRunMessageStatus.Messages.Count(), actualPFRunMessageStatus.Messages.Count(), "Mismatch in Production Forecast Run Message Status");
            Trace.WriteLine("Verified Production Forecast Run Message Status");

            // Plots tab : Verify All Well Results By PF Scenario Run of Production Forecast
            PFScenarioResultsArrayUnitsDTO actualAllResultsByTimeStep = ProductionForecastService.GetPFScenarioAllWellResultsByPFScenarioRunId(pfScenarioRunHeader[0].Id.ToString());
            Assert.IsNotNull(actualAllResultsByTimeStep);
            Trace.WriteLine("Verified All Well Results By PF Scenario Run of Production Forecast");
        }

        public void VerifyPFWellheadResultsByTimeStep(string Path, string FileName, PFScenarioResultsArrayUnitsDTO PFScenarioWellResultsArray, PFScenarioRunHeaderDTO pfScenarioRunHeader)
        {

            for (int i = 0; i < PFScenarioWellResultsArray.Values.Count(); i++)
            {
                string fileName = FileName + (i + 1).ToString() + ".json";
                PFScenarioResultsArrayUnitsDTO actualPFScenarioWellResultsArrayByTimeStep = ProductionForecastService.GetPFScenarioWellResultsByPFScenarioRunIdAndTimestep(pfScenarioRunHeader.Id.ToString(), PFScenarioWellResultsArray.Values[i].Id.ToString());
                PFScenarioResultsArrayUnitsDTO expectedPFScenarioWellResultsArrayByTimeStep = JsonConvert.DeserializeObject<PFScenarioResultsArrayUnitsDTO>(GetJsonString(Path + fileName));
                VerifyPFResults(expectedPFScenarioWellResultsArrayByTimeStep, actualPFScenarioWellResultsArrayByTimeStep);
                Trace.WriteLine("Wellhead Verification completed for timestep :" + PFScenarioWellResultsArray.Values[i].Timestep.ToString());
            }
        }

        public void VerifyPFConstraintsResultsByTimeStep(string Path, string FileName, PFScenarioResultsArrayUnitsDTO PFScenarioWellResultsArray, PFScenarioRunHeaderDTO pfScenarioRunHeader)
        {

            for (int i = 0; i < PFScenarioWellResultsArray.Values.Count(); i++)
            {
                string fileName = FileName + (i + 1).ToString() + ".json";
                PFScenarioResultsArrayUnitsDTO actualPFScenarioWellResultsArrayByTimeStep = ProductionForecastService.GetPFConstraintResultsByRunIdAndTimestep(pfScenarioRunHeader.Id.ToString(), PFScenarioWellResultsArray.Values[i].Id.ToString());
                PFScenarioResultsArrayUnitsDTO expectedPFScenarioWellResultsArrayByTimeStep = JsonConvert.DeserializeObject<PFScenarioResultsArrayUnitsDTO>(GetJsonString(Path + fileName));
                VerifyPFResults(expectedPFScenarioWellResultsArrayByTimeStep, actualPFScenarioWellResultsArrayByTimeStep);
                Trace.WriteLine("Constraints Verification completed for timestep :" + PFScenarioWellResultsArray.Values[i].Timestep.ToString());
            }
        }

        public void VerifyPFSinksResultsByTimeStep(string Path, string FileName, PFScenarioResultsArrayUnitsDTO PFScenarioWellResultsArray, PFScenarioRunHeaderDTO pfScenarioRunHeader)
        {

            for (int i = 0; i < PFScenarioWellResultsArray.Values.Count(); i++)
            {
                string fileName = FileName + (i + 1).ToString() + ".json";
                PFScenarioResultsArrayUnitsDTO actualPFScenarioWellResultsArrayByTimeStep = ProductionForecastService.GetPFSinkResultsByRunIdAndTimestep(pfScenarioRunHeader.Id.ToString(), PFScenarioWellResultsArray.Values[i].Id.ToString());
                PFScenarioResultsArrayUnitsDTO expectedPFScenarioWellResultsArrayByTimeStep = JsonConvert.DeserializeObject<PFScenarioResultsArrayUnitsDTO>(GetJsonString(Path + fileName));
                VerifyPFResults(expectedPFScenarioWellResultsArrayByTimeStep, actualPFScenarioWellResultsArrayByTimeStep);
                Trace.WriteLine("Sinks Verification completed for timestep :" + PFScenarioWellResultsArray.Values[i].Timestep.ToString());
            }
        }

        public void VerifyPFReservoirResultsByTimeStep(string Path, string FileName, PFScenarioResultsArrayUnitsDTO PFScenarioWellResultsArray, PFScenarioRunHeaderDTO pfScenarioRunHeader)
        {

            for (int i = 0; i < PFScenarioWellResultsArray.Values.Count(); i++)
            {
                string fileName = FileName + (i + 1).ToString() + ".json";
                PFScenarioResultsArrayUnitsDTO actualPFScenarioWellResultsArrayByTimeStep = ProductionForecastService.GetProductionForecastReservoirResultsByRunIdAndTimestep(pfScenarioRunHeader.Id.ToString(), PFScenarioWellResultsArray.Values[i].Id.ToString());
                PFScenarioResultsArrayUnitsDTO expectedPFScenarioWellResultsArrayByTimeStep = JsonConvert.DeserializeObject<PFScenarioResultsArrayUnitsDTO>(GetJsonString(Path + fileName));
                VerifyPFResults(expectedPFScenarioWellResultsArrayByTimeStep, actualPFScenarioWellResultsArrayByTimeStep);
                Trace.WriteLine("Reservoir Verification completed for timestep :" + PFScenarioWellResultsArray.Values[i].Timestep.ToString());
            }
        }

        public void createWellTest(WellTestDTO TestData)
        {
            WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(TestData.WellId.ToString()).Units;
            WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, TestData));
            WellTestAndUnitsDTO validWellTest = WellTestDataService.GetLatestValidWellTestByWellId(TestData.WellId.ToString());
            Assert.IsNotNull(validWellTest, "Added WellTest is not valid");
        }

        public void comparePFScenarios(PFScenarioDTO expectedPFScenario, PFScenarioDTO actualPFScenario)
        {
            CompareObjectsUsingReflection(expectedPFScenario, actualPFScenario, "Mismatch Found in PFScenario", new HashSet<string> { "Asset", "ChangeDate", "ChangeUser", "Model", "SNScenario" });
            CompareObjectsUsingReflection(expectedPFScenario.Asset, actualPFScenario.Asset, "Mismatch Found in Asset");
            CompareObjectsUsingReflection(expectedPFScenario.Model, actualPFScenario.Model, "Mismatch Found in Model", new HashSet<string> { "Asset" });
            CompareObjectsUsingReflection(expectedPFScenario.Model.Asset, actualPFScenario.Model.Asset, "Mismatch Found in Model -->Asset");
            CompareObjectsUsingReflection(expectedPFScenario.SNScenario, actualPFScenario.SNScenario, "Mismatch Found in SNscenario", new HashSet<string> { "SNModel" });
            CompareObjectsUsingReflection(expectedPFScenario.SNScenario.SNModel, actualPFScenario.SNScenario.SNModel, "Mismatch Found in SNscenario -->SNModel");
        }

        public void compareSNScenarios(SNScenarioDTO expectedSNScenario, SNScenarioDTO actualSNScenario)
        {
            CompareObjectsUsingReflection(expectedSNScenario, actualSNScenario, "Mismatch Found in SNscenario", new HashSet<string> { "SNModel" });
            CompareObjectsUsingReflection(expectedSNScenario.SNModel, actualSNScenario.SNModel, "Mismatch Found in SNscenario -->SNModel");
        }

        public void VerifyPFScenarioSchedule(PFScenarioDTO expectedPFScenario, SNScenarioScheduleDTO actualSNScenarioSchedule)
        {
            CompareObjectsUsingReflection(expectedPFScenario, actualSNScenarioSchedule.PFScenario, "Mismatch Found in PFScenario", new HashSet<string> { "Asset", "ChangeDate", "ChangeUser", "Model", "SNScenario" });
            Assert.IsTrue(actualSNScenarioSchedule.PFScenarioId == expectedPFScenario.Id);
            Assert.IsTrue(actualSNScenarioSchedule.SNScenarioId == null);
            Assert.IsTrue(actualSNScenarioSchedule.Status == Enums.SNScenarioScheduleStatus.New);
        }

        public void VerifyPFScenarioScheduleHistory(PFScenarioDTO expectedPFScenario, SNScenarioScheduleHistoryDTO actualSNScenarioScheduleHistory)
        {
            CompareObjectsUsingReflection(expectedPFScenario, actualSNScenarioScheduleHistory.PFScenario, "Mismatch Found in PFScenario", new HashSet<string> { "Asset", "ChangeDate", "ChangeUser", "Model", "SNScenario" });
            Assert.IsTrue(actualSNScenarioScheduleHistory.PFScenarioId == expectedPFScenario.Id);
            Assert.IsTrue(actualSNScenarioScheduleHistory.SNScenarioId == null);
            Assert.IsTrue(actualSNScenarioScheduleHistory.Status != Enums.SNScenarioScheduleStatus.New);
        }

        public void comparePFModel(PFModelDTO expectedPFModel, PFModelDTO actualPFModel)
        {
            CompareObjectsUsingReflection(expectedPFModel, actualPFModel, "Mismatch Found in PFModel", new HashSet<string> { "ChangeDate", "ChangeUser", "Id", "Asset", "PFModelFile " });
            CompareObjectsUsingReflection(expectedPFModel.Asset, actualPFModel.Asset, "Mismatch Found in PFModel -->Asset");
        }

        private PFModelDTO CreatePFModel(string PFModelName, Enums.ProductionForecastReservoirOption PFReservoirOption, string PFModelFileName, long AssetId, String PFModelDescription)
        {
            string Path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";
            //Getting PFModelFile in Byte Array Format
            byte[] fileAsByteArray = GetByteArray(Path, PFModelFileName);
            Assert.IsNotNull(fileAsByteArray);

            PFModelFileDTO modelFile = new PFModelFileDTO();
            modelFile.PFContents = Convert.ToBase64String(fileAsByteArray);


            //Creating PF Model
            PFModelDTO pfModel = new PFModelDTO();
            pfModel.Name = PFModelName;
            pfModel.PFReservoirOption = PFReservoirOption;
            pfModel.AssetId = AssetId;
            pfModel.Description = PFModelDescription;
            pfModel.PFModelFile = modelFile;
            ProductionForecastService.AddPFModel(pfModel);

            PFModelDTO[] pfModels = ProductionForecastService.GetPFModelsByAssetIds(new long[] { AssetId });

            return pfModels.FirstOrDefault(x => x.Name == PFModelName);
        }

        private SNScenarioDTO CreateScenario(string ModelName, String ScenarioName, long AssetId, string Description, bool IsForecastscenario)
        {
            SurfaceNetworkService.AddSNModel(new SNModelDTO() { Name = ModelName, AssetId = AssetId });

            var allModels = SurfaceNetworkService.GetSNModelsByAssetIds(new long[] { AssetId });
            SNModelDTO model = allModels?.FirstOrDefault(m => m.Name.Equals(ModelName));
            _snModelsToRemove.Add(model);
            Assert.IsNotNull(model);

            SNScenarioDTO addScenarioObj = new SNScenarioDTO()
            {
                Name = ScenarioName,
                Description = Description,
                SNModelId = model.Id,
                ColorCode = "#FF0000",
                AllowableScenario = true,
                Implementable = false,
                ModelMatch = false,
                OptimumScenario = false,
                IsForecastScenario = IsForecastscenario
            };

            SurfaceNetworkService.AddSNScenario(addScenarioObj);
            var allScenarios = SurfaceNetworkService.GetSNScenariosBySNModel(model.Id.ToString());
            SNScenarioDTO scenario = allScenarios?.FirstOrDefault(sn => sn.Name.Equals(ScenarioName));
            Assert.IsNotNull(scenario);
            _snScenariosToRemove.Add(scenario);

            return scenario;
        }

        private void CreateForecastScenario(string pfScenarionName, SNScenarioDTO snScenario, PFModelDTO pfModel, string PFdescription, AssetDTO asset)
        {
            PFScenarioDTO pfScenarioObj = new PFScenarioDTO();
            pfScenarioObj.Name = pfScenarionName;
            pfScenarioObj.Description = PFdescription;
            pfScenarioObj.Asset = asset;
            pfScenarioObj.AssetId = asset.Id;
            pfScenarioObj.SNScenario = snScenario;
            pfScenarioObj.SNScenarioId = snScenario.Id;
            pfScenarioObj.Model = pfModel;
            pfScenarioObj.ModelId = pfModel.Id;
            ProductionForecastService.AddPFScenario(pfScenarioObj);
            PFScenarioDTO[] allPFScenarios = ProductionForecastService.GetPFScenariosByAssetIds(new long[] { asset.Id });
            PFScenarioDTO pfScenario = allPFScenarios?.FirstOrDefault(sn => sn.Name.Equals(pfScenarionName));
            _pfScenariosToRemove.Add(pfScenario);
        }

        private PFScenarioDTO CreateSamplePFScenario()
        {
            string pfScenarioName = "Test PF Scenario1";

            //Create 1 Asset
            List<AssetDTO> assetIds = CreateAsset(1);

            //Create SNScenario with Production Forecast selection 
            SNScenarioDTO snScenarioObj = CreateScenario("SNModel", "SNScenarioName", assetIds.ElementAt(0).Id, "SN Scenario Description", true);

            //Adding PFModel
            PFModelDTO pfModel = CreatePFModel("PFModelName1", Enums.ProductionForecastReservoirOption.Eclipse, "Tutorial3.rts", assetIds.ElementAt(0).Id, "PF Model Description");
            _pfModelsToRemove.Add(pfModel);

            //Create PFScenario
            CreateForecastScenario(pfScenarioName, snScenarioObj, pfModel, "PFScenario Description1", assetIds.ElementAt(0));

            //Verifying that ForeSite Scenarios associated to Asset1 are retrieved
            var allForecastScenaios = ProductionForecastService.GetPFScenariosByAssetIds(new long[] { assetIds.ElementAt(0).Id });
            Assert.IsTrue(allForecastScenaios.Count() == 1);

            var pfScenarioObj = allForecastScenaios.FirstOrDefault(x => x.Name == pfScenarioName);
            Assert.IsNotNull(pfScenarioObj);
            return pfScenarioObj;
        }

        private WellDTO AddNonRRLWellGeneral(string wellName, WellTypeId WellType, WellFluidType FluidType, string WellDepthReference, long Asset)
        {
            ReferenceTableItemDTO[] wellDepthDatums = WellConfigurationService.GetReferenceTableItems("r_WellDepthDatum", "false");
            ReferenceTableItemDTO wellDepthDatum = wellDepthDatums.FirstOrDefault(t => t.ConstantId == WellDepthReference) ?? wellDepthDatums.FirstOrDefault();

            WellDTO well = new WellDTO();
            well = new WellDTO()
            {
                Name = wellName,
                DataConnection = null,
                CommissionDate = DateTime.Today.AddYears(-4),
                AssemblyAPI = wellName,
                SubAssemblyAPI = wellName,
                IntervalAPI = wellName,
                WellType = WellType,
                FluidType = null,
                DepthCorrectionFactor = 2,
                WellDepthDatumElevation = 1,
                WellGroundElevation = 1,
                WellDepthDatumId = wellDepthDatum?.Id,
                GasAllocationGroup = null,
                OilAllocationGroup = null,
                WaterAllocationGroup = null,
                AssetId = Asset,
                FacilityId = null
            };
            if (WellType == WellTypeId.NF || WellType == WellTypeId.GLift) // Added GLift as scope of FRWM-6777
            { well.FluidType = FluidType; }


            WellConfigDTO wellConfig = new WellConfigDTO();
            wellConfig.Well = well;
            wellConfig.ModelConfig = ReturnBlankModel();
            //Well
            WellConfigDTO addedWellConfig = WellConfigurationService.AddWellConfig(wellConfig);
            Assert.IsTrue(addedWellConfig.Well.Id > 0);
            _wellsToRemove.Add(addedWellConfig.Well);

            return addedWellConfig.Well;
        }
    }
}
