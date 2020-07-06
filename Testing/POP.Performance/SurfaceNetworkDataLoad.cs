using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weatherford.POP.DTOs;
using Weatherford.POP.Enums;
using Weatherford.POP.Interfaces;

namespace POP.Performance
{
    public class SurfaceNetworkDataLoad : APIPerfTestBase
    {
        /// <summary>
        /// This method is used to Add Surface Network model in ForeSite DB.
        /// </summary>
        /// <param name="Path">SurfaceNetworkPath</param>
        /// <param name="ModelName">NetworkModelName</param>
        /// <param name="AssetName">AssetName</param>
        public void AddSurfaceNetworkModel(string Path, string ModelName, string AssetName)
        {
            Authenticate();

            string assetName = AssetName;
            string path = Path;
            string modelName = ModelName;
            string ReOmodel;

            ////Delete new created Asset if this test fails at 1st time.
            //var allAssets2 = SurfaceNetworkService.GetAllAssets().ToList();
            //AssetDTO asset2 = allAssets2?.FirstOrDefault(a => a.Name.Equals(assetName));
            //if (asset2 != null)
            //    SurfaceNetworkService.RemoveAsset(asset2.Id.ToString());            
            if (modelName.Contains(".reo"))
            {
                ReOmodel = ModelName;
            }
            else
            {
                ReOmodel = ModelName + ".reo";
            }

            if (path != null)
            {
                try
                {
                    //Check any Asset exist in System
                    AssetDTO getAsset = getAssetFromDB(assetName);
                    if (getAsset == null)
                    {
                        //Configure new Asset                        
                        SurfaceNetworkService.AddAsset(new AssetDTO()
                        {
                            Name = assetName,
                            Description = "Added by Performance utility"
                        });
                        //Get all Assets to see newly added Asset in System
                        var allAssets = SurfaceNetworkService.GetAllAssets().ToList();
                        AssetDTO asset = allAssets?.FirstOrDefault(a => a.Name.Equals(assetName));

                        //Configure new sn model and model file
                        var model = AddSurfaceNetworkModelAndModelFile(asset, path, ReOmodel, modelName);
                    }
                }
                catch (Exception ex)
                {
                    WriteLogFile(string.Format(ex.Message));
                }
            }
        }

        private SNModelDTO AddSurfaceNetworkModelAndModelFile(AssetDTO asset, string path, string ReOmodel, string modelName)
        {
            // add in the model file to use for scenario test calls
            SNModelAndModelFileDTO modelAndFile = new SNModelAndModelFileDTO();

            SNModelFileDTO modelFile = new SNModelFileDTO
            {
                Comments = "Model File added by Performance utility",
                UpdateFluidModel = true
            };

            //Remove Read Only attribute from model file.
            var di = new DirectoryInfo(path);
            foreach (var file in di.GetFiles("*.reo", SearchOption.AllDirectories))
                file.Attributes &= ~FileAttributes.ReadOnly;
            Trace.WriteLine(string.Format("Path for Model File is {0} , and Model Name is {1}", path, ReOmodel));
            if (System.IO.File.Exists(System.IO.Path.Combine(path, ReOmodel)))
            {
                Trace.WriteLine("File Exists , now trying to read the data from " + System.IO.Path.Combine(path, ReOmodel));
            }
            else
            {
                Trace.WriteLine("Aborting Test as File was not found");
            }

            byte[] fileAsByteArray = GetByteArrayForSNModel(path, ReOmodel);
            modelFile.Base64Contents = Convert.ToBase64String(fileAsByteArray);
            modelAndFile.SNModelFile = modelFile;
            // Add Surface Network Model in ForeSite Database
            SurfaceNetworkService.AddSNModel(new SNModelDTO() { Name = modelName, AssetId = asset.Id });
            var allModels = SurfaceNetworkService.GetSNModelsByAsset(asset.Id.ToString());
            SNModelDTO model = allModels?.FirstOrDefault(m => m.Name.Equals(modelName));

            // Upload the model file to get things configured
            modelAndFile.SNModel = model;
            SurfaceNetworkService.SaveReOFile(modelAndFile);

            return model;
        }

        public static byte[] GetByteArrayForSNModel(string path, string file)
        {
            byte[] fileAsByteArray;
            using (FileStream fileStream = new FileStream(Path.Combine(path, file), FileMode.Open))
            {
                using (var memoryStream = new MemoryStream())
                {
                    fileStream.CopyTo(memoryStream);
                    fileAsByteArray = memoryStream.ToArray();
                }
            }
            return fileAsByteArray;
        }
        /// <summary>
        /// This method is used to delete Surface Network Model from ForeSite DB.
        /// </summary>
        /// <param name="ModelName">NetworkModelName</param>
        /// <param name="AssetName">AssetName</param>
        /// <param name="NoOfScenario">NoOfScenario</param>
        public void RemoveSurfaceNetworkModel(string ModelName, string AssetName, long NoOfScenario)
        {
            Authenticate();
            string assetName = AssetName;
            string modelName = ModelName;
            try
            {
                if (modelName != null)
                {
                    //Check any Asset exist in System                    
                    AssetDTO getAsset = getAssetFromDB(assetName);

                    //Check Network Model exist in System
                    var allModels = SurfaceNetworkService.GetSNModelsByAsset(getAsset.Id.ToString());
                    SNModelDTO model = allModels?.FirstOrDefault(m => m.Name.Equals(modelName));

                    // Clean up created models
                    if (model != null)
                    {
                        //Get all SNWellNames from SNModel
                        SNWellMappingDTO[] mappings = SurfaceNetworkService.GetSNWellMappingsByAssetIds(new long[] { getAsset.Id });

                        if (mappings.Count() != 0)
                        {
                            //Clean up well mapping
                            RemoveWellMappingWithSNModel(modelName, assetName);
                        }
                        //Clean up Scenarios
                        DeleteScenario(modelName, assetName, NoOfScenario);
                        //Clean up created Model
                        SurfaceNetworkService.RemoveSNModel(model.Id.ToString());
                        // Clean up created Asset
                        SurfaceNetworkService.RemoveAsset(getAsset.Id.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLogFile(string.Format(ex.Message));
            }
        }

        /// <summary>
        /// Get Asset from ForeSite DB.
        /// </summary>
        /// <param name="assetName">Specify Model File name</param>
        /// <returns>Asset Name</returns>
        public AssetDTO getAssetFromDB(string assetName)
        {
            try
            {
                var getAssets = SurfaceNetworkService.GetAllAssets().ToList();
                AssetDTO getAsset = getAssets?.FirstOrDefault(a => a.Name.Equals(assetName));
                return getAsset;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        /// <summary>
        /// This method is used for mapping
        /// </summary>
        /// <param name="ModelName">NetworkModelName</param>
        /// <param name="AssetName">AssetName</param>
        public void AddWellMappingWithSNModel(string ModelName, string AssetName)
        {
            Authenticate();
            string assetName = AssetName;
            string modelName = ModelName;
            //Check any Asset exist in System
            AssetDTO getAsset = getAssetFromDB(assetName);
            //Check Network Model exist in System
            var allModels = SurfaceNetworkService.GetSNModelsByAsset(getAsset.Id.ToString());
            SNModelDTO model = allModels?.FirstOrDefault(m => m.Name.Equals(modelName));
            if (model != null)
            {
                //Get wells(ESP, NFW and Gas Lift) from ForeSite Database.
                IEnumerable<WellDTO> allWells = WellService.GetAllWells().Where(x => x.WellType == WellTypeId.ESP || x.WellType == WellTypeId.NF || x.WellType == WellTypeId.GLift);
                //Get all SNWellNames from SNModel
                SNWellMappingDTO[] mappings = SurfaceNetworkService.GetSNWellMappingsByAssetIds(new long[] { getAsset.Id });
                if (allWells.Count() >= 0 && mappings.Count() >= 0)
                {
                    try
                    {
                        int i = 0;
                        foreach (WellDTO well in allWells)
                        {

                            // Map all wells with SNWellNames in SNModel
                            SNWellMappingDTO initialWellMapping = mappings[i];
                            initialWellMapping.WellId = well.Id;
                            SurfaceNetworkService.UpdateSNWellMapping(initialWellMapping);
                            Console.WriteLine("Well {0} mapped with Surface Network {1}", well.Name, initialWellMapping.SNWellName);
                            i++;
                            ////Undo Mapping so we can delete the well
                            //SNWellMappingDTO undoWellMapping = mappings[0];
                            //undoWellMapping.WellId = null;
                            //SurfaceNetworkService.UpdateSNWellMapping(undoWellMapping);                           
                        }
                        Console.WriteLine("{0} Wells mapped with Surface Network", i);
                        Console.ReadKey();
                    }
                    catch
                    {
                        WriteLogFile(string.Format("Unable to map well with Surface Network Model : "));
                    }
                }
                else
                    WriteLogFile(string.Format("No ESP, NF and Gas Lift Wells inside the Database"));
            }
        }
        /// <summary>
        /// This method is used for un-mapping
        /// </summary>
        /// <param name="ModelName">NetworkModelName</param>
        /// <param name="AssetName">AssetName</param>
        public void RemoveWellMappingWithSNModel(string ModelName, string AssetName)
        {
            Authenticate();
            string assetName = AssetName;
            string modelName = ModelName;
            //Check any Asset exist in System
            AssetDTO getAsset = getAssetFromDB(assetName);
            //Check Network Model exist in System
            var allModels = SurfaceNetworkService.GetSNModelsByAsset(getAsset.Id.ToString());
            SNModelDTO model = allModels?.FirstOrDefault(m => m.Name.Equals(modelName));
            if (model != null)
            {
                //Get all SNWellNames from SNModel
                SNWellMappingDTO[] mappings = SurfaceNetworkService.GetSNWellMappingsByAssetIds(new long[] { getAsset.Id });
                if (mappings.Count() >= 0)
                {
                    try
                    {
                        int i = 0;
                        foreach (SNWellMappingDTO map in mappings)
                        {

                            // Map all wells with SNWellNames in SNModel
                            SNWellMappingDTO initialWellMapping = mappings[i];
                            initialWellMapping.WellId = null;
                            SurfaceNetworkService.UpdateSNWellMapping(initialWellMapping);
                            Console.WriteLine("Well removed from Surface Network {1}", initialWellMapping.WellId, initialWellMapping.SNWellName);
                            i++;
                        }
                        Console.WriteLine("{0} Wells un-mapped from Surface Network", i);
                        Console.ReadKey();
                    }
                    catch
                    {
                        WriteLogFile(string.Format("Unable to un-map well with Surface Network Model."));
                    }
                }
                else
                    WriteLogFile(string.Format("No ESP, NF and Gas Lift Wells mapped with SN Model inside the Database"));
            }
        }
        /// <summary>
        /// This method is used for adding Scenario in ForeSite DB.
        /// </summary>
        /// <param name="ModelName">NetworkModelName</param>
        /// <param name="AssetName">AssetName</param>
        /// <param name="NoOfScenario">NoOfScenario</param>
        public void AddScenario(string ModelName, string AssetName, long NoOfScenario)
        {
            Authenticate();
            string assetName = AssetName;
            string modelName = ModelName;
            string scenarioName = "Scenario ";
            //Check any Asset exist in System
            AssetDTO getAsset = getAssetFromDB(assetName);
            //Check Network Model exist in System
            var allModels = SurfaceNetworkService.GetSNModelsByAsset(getAsset.Id.ToString());
            SNModelDTO model = allModels?.FirstOrDefault(m => m.Name.Equals(modelName));
            if (model != null)
            {
                //Get all SNWellNames from SNModel
                SNWellMappingDTO[] mappings = SurfaceNetworkService.GetSNWellMappingsByAssetIds(new long[] { getAsset.Id });

                if (mappings[0].WellId != null)
                {
                    for (int i = 1; i <= NoOfScenario; i++)
                    {
                        var allScenarios = SurfaceNetworkService.GetSNScenariosBySNModel(model.Id.ToString());
                        SNScenarioDTO scenario = allScenarios?.FirstOrDefault(sn => sn.Name.Equals(scenarioName + i));
                        if (scenario == null)
                        {
                            SurfaceNetworkService.AddSNScenario(new SNScenarioDTO() { Name = scenarioName + i, Description = "Test Description", SNModelId = model.Id, ColorCode = "#FF0000", AllowableScenario = true, Implementable = false, ModelMatch = false, OptimumScenario = false });
                            Console.WriteLine("Scenario {0} added successfully", scenario.Name);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// This method is used for deleting Scenario from ForeSite DB.
        /// </summary>
        /// <param name="ModelName">NetworkModelName</param>
        /// <param name="AssetName">AssetName</param>
        /// <param name="NoOfScenario">NoOfScenario</param>
        public void DeleteScenario(string ModelName, string AssetName, long NoOfScenario)
        {
            Authenticate();
            string assetName = AssetName;
            string modelName = ModelName;
            string scenarioName = "Scenario ";
            //Check any Asset exist in System
            AssetDTO getAsset = getAssetFromDB(assetName);
            //Check Network Model exist in System
            var allModels = SurfaceNetworkService.GetSNModelsByAsset(getAsset.Id.ToString());
            SNModelDTO model = allModels?.FirstOrDefault(m => m.Name.Equals(modelName));
            if (model != null)
            {
                //Get all SNWellNames from SNModel
                SNWellMappingDTO[] mappings = SurfaceNetworkService.GetSNWellMappingsByAssetIds(new long[] { getAsset.Id });

                if (mappings[0].WellId != null)
                {
                    for (int i = 1; i <= NoOfScenario; i++)
                    {
                        var allScenarios = SurfaceNetworkService.GetSNScenariosBySNModel(model.Id.ToString());
                        SNScenarioDTO scenario = allScenarios?.FirstOrDefault(sn => sn.Name.Equals(scenarioName + i));
                        // Clean up scenario
                        SurfaceNetworkService.RemoveSNScenario(scenario.Id.ToString());

                    }
                }
            }
        }
        /// <summary>
        /// This method is used for adding Scenario in Job Scheduler and running Optimization.
        /// </summary>
        /// <param name="ModelName">NetworkModelName</param>
        /// <param name="AssetName">AssetName</param>
        /// <param name="NoOfScenario">NoOfScenario</param>
        public void AddScenarioInJobSchedulerAndRunOptimization(string ModelName, string AssetName, long NoOfScenario)
        {
            Authenticate();
            string assetName = AssetName;
            string modelName = ModelName;
            string scenarioName = "Scenario ";
            try
            {
                //Check any Asset exist in System
                AssetDTO getAsset = getAssetFromDB(assetName);
                //Check Network Model exist in System
                var allModels = SurfaceNetworkService.GetSNModelsByAsset(getAsset.Id.ToString());
                SNModelDTO model = allModels?.FirstOrDefault(m => m.Name.Equals(modelName));
                if (model != null)
                {
                    for (int i = 1; i <= NoOfScenario; i++)
                    {
                        var allScenarios = SurfaceNetworkService.GetSNScenariosBySNModel(model.Id.ToString());
                        SNScenarioDTO scenario = allScenarios?.FirstOrDefault(sn => sn.Name.Equals(scenarioName + i));
                        if (scenario != null)
                        {
                            // add the just created scenario and see if it is in the schedules list now
                            SurfaceNetworkService.InsertSNScenarioScheduleManualRun(scenario.Id.ToString());

                            SNScenarioScheduleDTO[] schedules = SurfaceNetworkService.GetScenarioSchedules(ScenarioType.SurfaceNetwork);

                            SNScenarioScheduleDTO scenschedPre = SurfaceNetworkService.GetSNScenarioSchedule(schedules[0].Id.ToString());

                            SurfaceNetworkService.RunScheduledOptimizationJobs();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLogFile(string.Format(ex.Message));
            }
        }
    }
}
