using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Weatherford.POP.Utils;
using System.Diagnostics;

namespace POP.Performance
{
    internal class Args
    {
        public const string Methods = "-Methods";
        public const string CVS = "-CVS";
        public const string ForeSiteServer = "-ForeSiteServer";
        public const string ConcurrencyNum = "-ConNum";
        public const string AddRRLWells = "-AddRRLWells";
        public const string BaseFacTag = "-BaseFacTag";
        public const string StartWellCount = "-StartWellCount";
        public const string EndWellCount = "-EndWellCount";
        public const string AddJobs = "-LoadJobs";
        public const string AddEvents = "-LoadEvents";
        public const string WellservivceConfigurationLoad = "-LoadTemplates";
        public const string AddStickyNotes = "-LoadStickyNotes";
        public const string AddWSMWells = "-LoadWSMWells";
        public const string ScanRRLWells = "-ScanRRLWells";
        public const string AddWorkover = "-AddWorkoverRRL";
        public const string AddNonRRLwells = "-AddNonRRLWells";
        public const string WellType = "-WellType";
        public const string Usage = "-SystemUsage";
        public const string LoadTime = "-LoadTime";
        public const string AddAnalysisReports = "-AddHistoricalAnalysisReports";
        public const string AddDowntimeRecords = "-AddDowntimeRecords";
        public const string RemoveWells = "-RemoveWells";
        public const string AddDynacardstoDynacardLibrary = "-AddHistoricalDynacards";
        public const string AddComponentstoAllJobs = "-AddComponentstoAllJobs";
        public const string AddFailureObservations = "-AddFailureObservations";
        public const string LoadWellTestDataForRRL = "-LoadWellTestDataForRRL";
        #region Surface Network Configuration
        public const string AddSurfaceNetworkModel = "-AddSurfaceNetworkModel";
        public const string RemoveSurfaceNetworkModel = "-RemoveSurfaceNetworkModel";
        public const string AddWellMapping = "-AddWellMapping";
        public const string RemoveWellMapping = "-RemoveWellMapping";
        public const string AddScenario = "-AddScenario";
        public const string RemoveScenario = "-RemoveScenario";
        public const string AddScenarioInSchedulerAndRunOptimization = "-AddScenarioInSchedulerAndRunOptimization";
        #endregion        
    }

    public class APIPerformance : APIPerfTestBase
    {
        public static APIPerfTestBase PerfBase = new APIPerfTestBase();
        public static RRLDataLoad RRL_dataLoad = new RRLDataLoad();
        public static WSMDataLoad WSM_dataLoad = new WSMDataLoad();
        public static NonRRLDataLoad NonRRL_dataLoad = new NonRRLDataLoad();
        public static SystemUsage System_Usage = new SystemUsage();
        public static LoadingTimeforAPIs ScreenLoad = new LoadingTimeforAPIs();
        public static SurfaceNetworkDataLoad Surface_Data_Load = new SurfaceNetworkDataLoad();
        public static CygNet.COMAPI.Core.DomainSiteService cvs = null;

        public static void Main(string[] args)
        {
            var argMgr = new ArgumentManager();
            argMgr.SetDefaultHelpArgs();
            argMgr.AddParameter(Args.CVS, "Domain+Site+Service for DDS (e.g. [5412]MYSITE.UIS).", true, true);
            argMgr.AddParameter(Args.Methods, "List of Methods to run separated by semicolons (only use this if you want to limit methods, default is All).", true, false);
            argMgr.AddParameter(Args.ForeSiteServer, "Name of ForeSite server (default is localhost).", true);
            argMgr.AddParameter(Args.ConcurrencyNum, "Number of Concurrent calls (default is 1).", true);
            argMgr.AddHiddenParameter(Args.AddRRLWells, "Use this to Load RRL Wells", false);
            argMgr.AddHiddenParameter(Args.AddJobs, "Use this to Load Jobs", false);
            argMgr.AddHiddenParameter(Args.AddEvents, "Use this to Load Jobs", false);
            argMgr.AddHiddenParameter(Args.BaseFacTag, "BaseName to use for CygNet Facility [Will become facility BaseName_0000X]]", true);
            argMgr.AddHiddenParameter(Args.StartWellCount, "Start number for well creation, will be for facIds to use in CygNet", true);
            argMgr.AddHiddenParameter(Args.EndWellCount, "End number for well creation, will be for facIds to use in CygNet]", true);
            argMgr.AddHiddenParameter(Args.WellservivceConfigurationLoad, "Use this to load template data for WSM", true);
            argMgr.AddHiddenParameter(Args.AddStickyNotes, "Use this add sticky notes", true);
            argMgr.AddHiddenParameter(Args.AddWSMWells, "Use this to add Wells with WSM data", true);
            argMgr.AddHiddenParameter(Args.ScanRRLWells, "Use this scan the avilable RRL wells to Load the intelligent alarms", true);
            argMgr.AddHiddenParameter(Args.AddNonRRLwells, "Use this to add Non-RRL wells", true);
            argMgr.AddHiddenParameter(Args.WellType, "Use this specify the well type", true);
            argMgr.AddHiddenParameter(Args.Usage, "Use this specify the well type", true);
            argMgr.AddHiddenParameter(Args.LoadTime, "Use this obtain Loading time for all the screens", true);
            argMgr.AddHiddenParameter(Args.AddAnalysisReports, "Use this to generte Historical Analysis Reports", true);
            argMgr.AddHiddenParameter(Args.AddDowntimeRecords, "Use this to generate Downtime records", true);
            argMgr.AddHiddenParameter(Args.RemoveWells, "Use this to remove well records", true);
            argMgr.AddHiddenParameter(Args.AddDynacardstoDynacardLibrary, "Use this to load DynaCards in the DynaCard library", true);
            argMgr.AddHiddenParameter(Args.AddComponentstoAllJobs, "Use this to load Components history for all wells", true);
            argMgr.AddHiddenParameter(Args.AddFailureObservations, "Use this to load Failures for all wells", true);
            argMgr.AddHiddenParameter(Args.LoadWellTestDataForRRL, "Use this to load Well Test data for RRL wells", true);
            //Surface Network Configuration
            argMgr.AddHiddenParameter(Args.AddSurfaceNetworkModel, "Use this to Add Surface Network Model in ForeSite Database", true);
            argMgr.AddHiddenParameter(Args.RemoveSurfaceNetworkModel, "Use this to Delete Surface Network Model from ForeSite Database", true);
            argMgr.AddHiddenParameter(Args.AddWellMapping, "Use this to Add Well Mapping in ForeSite Database", true);
            argMgr.AddHiddenParameter(Args.RemoveWellMapping, "Use this to Delete Well Mapping from ForeSite Database", true);
            argMgr.AddHiddenParameter(Args.AddScenario, "Use this to Add Scenario in ForeSite Database", true);
            argMgr.AddHiddenParameter(Args.RemoveScenario, "Use this to Delete Scenario from ForeSite Database", true);
            argMgr.AddHiddenParameter(Args.AddScenarioInSchedulerAndRunOptimization, "Use this to Add Scenario in Scheduler of ForeSite", true);
            argMgr.Parse(args);
            try
            {
                do
                {
                    if (argMgr.GetUnknownArgs().Count > 0)
                    {
                        var bob = new StringBuilder();
                        bob.AppendLine("Unexpected arguments: " + string.Join(", ", argMgr.GetUnknownArgs()));
                        bob.AppendLine(argMgr.ShowUsage());
                        Console.WriteLine(bob.ToString());
                        ShowAvailableMethods();
                        break;
                    }
                    if (argMgr.GetSetArgCount() == 0 || argMgr.IsHelpRequested())
                    {
                        Console.WriteLine(argMgr.ShowUsage());
                        ShowAvailableMethods();
                        break;
                    }

                    if (!argMgr.AreAllRequiredArgsSet())
                    {
                        Console.WriteLine("Missing required arguments: {0}", "", string.Join(", ", argMgr.GetMissingRequiredArguments()));
                        Console.WriteLine(" " + argMgr.ShowUsage());
                        ShowAvailableMethods();
                        break;
                    }
                    string dssStr = argMgr.GetParameterValue(Args.CVS);
                    try
                    {
                        cvs = new CygNet.COMAPI.Core.DomainSiteService(dssStr);
                    }
                    catch (Exception ex)
                    {
                        WriteLogFile("Failed to parse domain/site/service for CVS " + dssStr + ":" + " " + ex.Message);
                        break;
                    }

                    HashSet<string> methods = new HashSet<string>();
                    if (argMgr.ArgumentIsSet(Args.Methods))
                    {
                        methods = new HashSet<string>(argMgr.GetParameterValue(Args.Methods).Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
                    }
                    if ((!argMgr.ArgumentIsSet(Args.Methods)))
                    {
                        methods.Add("All");
                    }
                    string foreSiteHostname = argMgr.GetParameterValue(Args.ForeSiteServer);
                    if (string.IsNullOrWhiteSpace(foreSiteHostname))
                    {
                        foreSiteHostname = "localhost";
                    }
                    PerfBase.s_hostname = foreSiteHostname;
                    string concurrencyUsers = argMgr.GetParameterValue(Args.ConcurrencyNum);
                    if (string.IsNullOrWhiteSpace(concurrencyUsers))
                    {
                        concurrencyUsers = "1";
                    }
                    int ConcurrentCalls = Convert.ToInt32(concurrencyUsers);

                    bool LoadRRLWells = argMgr.ArgumentIsSet(Args.AddRRLWells);
                    string BaseFacTag = argMgr.GetParameterValue(Args.BaseFacTag);
                    string WellStart = argMgr.GetParameterValue(Args.StartWellCount);
                    string WellEnd = argMgr.GetParameterValue(Args.EndWellCount);
                    string Type = argMgr.GetParameterValue(Args.WellType);
                    bool LoadWSMJobs = argMgr.ArgumentIsSet(Args.AddJobs);
                    bool LoadWSMEvents = argMgr.ArgumentIsSet(Args.AddEvents);
                    bool LoadWSMTemplatesData = argMgr.ArgumentIsSet(Args.WellservivceConfigurationLoad);
                    bool LoadStickyNotes = argMgr.ArgumentIsSet(Args.AddStickyNotes);
                    bool LoadWSMWells = argMgr.ArgumentIsSet(Args.AddWSMWells);
                    bool ScanWellsRRL = argMgr.ArgumentIsSet(Args.ScanRRLWells);
                    bool LoadNonRRLWells = argMgr.ArgumentIsSet(Args.AddNonRRLwells);
                    bool SysUsage = argMgr.ArgumentIsSet(Args.Usage);
                    bool LoadingTime = argMgr.ArgumentIsSet(Args.LoadTime);
                    bool LoadAnalysisReports = argMgr.ArgumentIsSet(Args.AddAnalysisReports);
                    bool LoadDowntimeRecords = argMgr.ArgumentIsSet(Args.AddDowntimeRecords);
                    bool RemoveWells = argMgr.ArgumentIsSet(Args.RemoveWells);
                    bool LoadDynacards = argMgr.ArgumentIsSet(Args.AddDynacardstoDynacardLibrary);
                    bool LoadComponents = argMgr.ArgumentIsSet(Args.AddComponentstoAllJobs);
                    bool LoadFailures = argMgr.ArgumentIsSet(Args.AddFailureObservations);
                    bool LoadWellTestDataForRRL = argMgr.ArgumentIsSet(Args.LoadWellTestDataForRRL);
                    //Surface Network Configuration
                    bool AddSurfaceNetworkModel = argMgr.ArgumentIsSet(Args.AddSurfaceNetworkModel);
                    bool RemoveSurfaceNetworkModel = argMgr.ArgumentIsSet(Args.RemoveSurfaceNetworkModel);
                    bool AddWellMapping = argMgr.ArgumentIsSet(Args.AddWellMapping);
                    bool RemoveWellMapping = argMgr.ArgumentIsSet(Args.RemoveWellMapping);
                    bool AddScenario = argMgr.ArgumentIsSet(Args.AddScenario);
                    bool RemoveScenario = argMgr.ArgumentIsSet(Args.RemoveScenario);
                    bool AddScenarioInSchedulerAndRunOptimization = argMgr.ArgumentIsSet(Args.AddScenarioInSchedulerAndRunOptimization);
                    if (LoadComponents)
                    {
                        if ((!string.IsNullOrWhiteSpace(WellStart)) && (!string.IsNullOrWhiteSpace(WellEnd)))
                        {
                            int WellStartNum = Convert.ToInt32(WellStart);
                            int WellEndNum = Convert.ToInt32(WellEnd);
                            Stopwatch st = new Stopwatch();
                            st.Start();
                            WSM_dataLoad.AddComponentsForOldestJob(WellStartNum, WellEndNum);
                            st.Stop();
                            Console.WriteLine(string.Format(" Time taken for component is  in secs {0} :   in minutes : {1} ", st.Elapsed.TotalSeconds.ToString(), st.Elapsed.TotalMinutes.ToString()));
                        }
                    }
                    if (LoadFailures)
                    {
                        if ((!string.IsNullOrWhiteSpace(WellStart)) && (!string.IsNullOrWhiteSpace(WellEnd)))
                        {
                            int WellStartNum = Convert.ToInt32(WellStart);
                            int WellEndNum = Convert.ToInt32(WellEnd);
                            Stopwatch st = new Stopwatch();
                            st.Start();
                            WSM_dataLoad.AddObservation(WellStartNum, WellEndNum);
                            st.Stop();
                            Console.WriteLine(string.Format(" Total Time taken for Loading Failures in secs {0} :   in minutes : {1} ", st.Elapsed.TotalSeconds.ToString(), st.Elapsed.TotalMinutes.ToString()));
                        }
                    }
                    if (LoadDynacards)
                    {
                        if ((!string.IsNullOrWhiteSpace(WellStart)) && (!string.IsNullOrWhiteSpace(WellEnd)))
                        {
                            long WellStartNum = Convert.ToInt64(WellStart);
                            long WellEndNum = Convert.ToInt64(WellEnd);
                            string _domain = cvs.GetDomainId().ToString();
                            string _site = cvs.GetSite().ToString();
                            string _service = cvs.GetService().ToString();
                            Stopwatch st = new Stopwatch();
                            st.Start();
                            RRL_dataLoad.AddHistoricalDynacards(WellStartNum, WellEndNum, _domain, _site, _service);
                            st.Stop();
                            Console.WriteLine(string.Format(" Total Time taken for Loading Dynacards for RRL Wells in secs {0} :   in minutes : {1} ", st.Elapsed.TotalSeconds.ToString(), st.Elapsed.TotalMinutes.ToString()));
                        }
                    }
                    if (RemoveWells)
                    {
                        if ((!string.IsNullOrWhiteSpace(WellStart)) && (!string.IsNullOrWhiteSpace(WellEnd)))
                        {
                            int WellStartNum = Convert.ToInt32(WellStart);
                            int WellEndNum = Convert.ToInt32(WellEnd);
                            RRL_dataLoad.RemoveWells(WellStartNum, WellEndNum);
                        }
                    }
                    if (LoadDowntimeRecords)
                    {
                        if ((!string.IsNullOrWhiteSpace(WellStart)) && (!string.IsNullOrWhiteSpace(WellEnd)))
                        {
                            int WellStartNum = Convert.ToInt32(WellStart);
                            int WellEndNum = Convert.ToInt32(WellEnd);

                            //Load Downtime records
                            WSM_dataLoad.AddDowntimeRecordsforAllWells(WellStartNum, WellEndNum);
                        }
                        else
                        {
                            WriteLogFile(string.Format("Missing required args for data load WellStart: {0} WellEnd: {1}", WellStart, WellEnd));
                        }
                    }
                    if (LoadAnalysisReports)
                        RRL_dataLoad.AddHistoricalAnalysisReports();
                    if (LoadingTime)
                        ScreenLoad.LoadingtimeforAllScreens();
                    if (SysUsage)
                        System_Usage.GetSystemUsage();
                    if (LoadRRLWells)
                    {
                        if ((!string.IsNullOrWhiteSpace(BaseFacTag)) && (!string.IsNullOrWhiteSpace(WellStart)) && (!string.IsNullOrWhiteSpace(WellEnd)))
                        {
                            int WellStartNum = Convert.ToInt32(WellStart);
                            int WellEndNum = Convert.ToInt32(WellEnd);
                            string _domain = cvs.GetDomainId().ToString();
                            string _site = cvs.GetSite().ToString();
                            string _service = cvs.GetService().ToString();
                            //Load Wells
                            RRL_dataLoad.WellData_RRL(BaseFacTag, WellStartNum, WellEndNum, _domain, _site, _service);
                        }
                        else
                        {
                            WriteLogFile(string.Format("Missing required args for data load BaseFac: {0} WellStart: {1} WellEnd: {2}", BaseFacTag, WellStart, WellEnd));
                        }
                    }
                    if (LoadWellTestDataForRRL)
                    {
                        if ((!string.IsNullOrWhiteSpace(BaseFacTag)) && (!string.IsNullOrWhiteSpace(WellStart)) && (!string.IsNullOrWhiteSpace(WellEnd)))
                        {
                            int WellStartNum = Convert.ToInt32(WellStart);
                            int WellEndNum = Convert.ToInt32(WellEnd);
                            string _domain = cvs.GetDomainId().ToString();
                            string _site = cvs.GetSite().ToString();
                            string _service = cvs.GetService().ToString();
                            //Load Well Test -CVS [9052]UNIT05.UIS -BaseFacTag RPOC -StartWellCount 6638 -EndWellCount 6799 -LoadWellTestDataForRRL
                            RRL_dataLoad.LoadWellTestDataForRRL(BaseFacTag, WellStartNum, WellEndNum, _domain, _site, _service);
                        }
                        else
                        {
                            WriteLogFile(string.Format("Missing required args for data load BaseFac: {0} WellStart: {1} WellEnd: {2}", BaseFacTag, WellStart, WellEnd));
                        }
                    }
                    if (LoadWSMJobs)
                    {
                        if ((!string.IsNullOrWhiteSpace(WellStart)) && (!string.IsNullOrWhiteSpace(WellEnd)))
                        {
                            long WellStartNum = Convert.ToInt64(WellStart);
                            long WellEndNum = Convert.ToInt64(WellEnd);
                            Stopwatch st = new Stopwatch();
                            st.Start();
                            WSM_dataLoad.AddJobsforAllWells(WellStartNum, WellEndNum);
                            st.Stop();
                            Console.WriteLine(string.Format(" Total Time taken for Load WSM Jobs in secs {0} :   in minutes : {1} ", st.Elapsed.TotalSeconds.ToString(), st.Elapsed.TotalMinutes.ToString()));
                        }
                        else
                        {
                            WriteLogFile(string.Format("Missing required args for data load BaseFac: {0} WellStart: {1} WellEnd: {2}", BaseFacTag, WellStart, WellEnd));
                        }
                    }
                    if (LoadWSMEvents)
                    {
                        if ((!string.IsNullOrWhiteSpace(WellStart)) && (!string.IsNullOrWhiteSpace(WellEnd)))
                        {
                            long WellStartNum = Convert.ToInt64(WellStart);
                            long WellEndNum = Convert.ToInt64(WellEnd);
                            Stopwatch st = new Stopwatch();
                            st.Start();
                            WSM_dataLoad.AddEventsForAllJobs(WellStartNum, WellEndNum);
                            st.Stop();
                            Console.WriteLine(string.Format(" Total Time taken for Load WSM Events in secs {0} :   in minutes : {1} ", st.Elapsed.TotalSeconds.ToString(), st.Elapsed.TotalMinutes.ToString()));
                        }
                        else
                        {
                            WriteLogFile(string.Format("Missing required args for data load BaseFac: {0} WellStart: {1} WellEnd: {2}", BaseFacTag, WellStart, WellEnd));
                        }
                    }
                    if (LoadWSMTemplatesData)
                    {
                        if ((!string.IsNullOrWhiteSpace(WellStart)))
                        {
                            int WellStartNum = Convert.ToInt32(WellStart);
                            WSM_dataLoad.AddTemplateJobs(WellStartNum.ToString());
                            WSM_dataLoad.AddQuickAddComponents();
                        }
                    }
                    if (LoadStickyNotes)
                    {
                        if ((!string.IsNullOrWhiteSpace(WellStart)) && (!string.IsNullOrWhiteSpace(WellEnd)))
                        {
                            int WellStartNum = Convert.ToInt32(WellStart);
                            int WellEndNum = Convert.ToInt32(WellEnd);
                            WSM_dataLoad.AddStickyNotes(WellStartNum, WellEndNum);
                        }
                        else
                        {
                            WriteLogFile(string.Format("Missing required args for data load BaseFac: {0} WellStart: {1} WellEnd: {2}", BaseFacTag, WellStart, WellEnd));
                        }
                    }
                    if (LoadWSMWells)
                    {
                        if ((!string.IsNullOrWhiteSpace(BaseFacTag)) && (!string.IsNullOrWhiteSpace(WellStart)) && (!string.IsNullOrWhiteSpace(WellEnd)))
                        {
                            int WellStartNum = Convert.ToInt32(WellStart);
                            int WellEndNum = Convert.ToInt32(WellEnd);
                            string _domain = cvs.GetDomainId().ToString();
                            string _site = cvs.GetSite().ToString();
                            string _service = cvs.GetService().ToString();
                            //Load Wells
                            WSM_dataLoad.AddWellWithAssemblyandSubassemblies(BaseFacTag, WellStartNum, WellEndNum, _domain, _site, _service, Type);
                        }
                        else
                        {
                            WriteLogFile(string.Format("Missing required args for data load BaseFac: {0} WellStart: {1} WellEnd: {2}", BaseFacTag, WellStart, WellEnd));
                        }
                    }
                    if (ScanWellsRRL)
                        RRL_dataLoad.ScanAllWells_RRL();
                    if (LoadNonRRLWells)
                    {
                        if ((!string.IsNullOrWhiteSpace(BaseFacTag)) && (!string.IsNullOrWhiteSpace(WellStart)) && (!string.IsNullOrWhiteSpace(WellEnd)))
                        {
                            int WellStartNum = Convert.ToInt32(WellStart);
                            int WellEndNum = Convert.ToInt32(WellEnd);
                            string _domain = cvs.GetDomainId().ToString();
                            string _site = cvs.GetSite().ToString();
                            string _service = cvs.GetService().ToString();
                            //Load Wells
                            NonRRL_dataLoad.AddWell(BaseFacTag, WellStartNum, WellEndNum, _domain, _site, _service, Type);
                        }
                        else
                        {
                            WriteLogFile(string.Format("Missing required args for data load BaseFac: {0} WellStart: {1} WellEnd: {2}", BaseFacTag, WellStart, WellEnd));
                        }
                    }
                    if (AddSurfaceNetworkModel)
                    {
                        if ((!string.IsNullOrWhiteSpace(SurfaceNetworkPath)) && (!string.IsNullOrWhiteSpace(NetworkModelName)) && (!string.IsNullOrWhiteSpace(AssetName)))
                        {
                            Console.WriteLine("Adding surface network started...");
                            // call Add method
                            Surface_Data_Load.AddSurfaceNetworkModel(SurfaceNetworkPath, NetworkModelName, AssetName);
                            Console.WriteLine("Added surface network successfully...");
                        }
                        else
                        {
                            WriteLogFile(string.Format("Missing required args for configuring Surfcae Network Path : {0} ModelName : {1} and Asset Name : {2} ", SurfaceNetworkPath, NetworkModelName, AssetName));
                        }

                    }
                    if (RemoveSurfaceNetworkModel)
                    {
                        if ((!string.IsNullOrWhiteSpace(NetworkModelName)) && (!string.IsNullOrWhiteSpace(AssetName)) && (!string.IsNullOrWhiteSpace(NoOfScenario)))
                        {
                            Console.WriteLine("Deleting surface network started...");
                            // call Delete method
                            Surface_Data_Load.RemoveSurfaceNetworkModel(NetworkModelName, AssetName, Convert.ToInt64(NoOfScenario));
                            Console.WriteLine("Deleted surface network successfully...");
                        }
                        else
                        {
                            WriteLogFile(string.Format("Missing required args for deleting Surfcae Network Model : {0}  Asset Name : {1}", NetworkModelName, AssetName));
                        }
                    }
                    if (AddWellMapping)
                    {
                        if ((!string.IsNullOrWhiteSpace(NetworkModelName)) && (!string.IsNullOrWhiteSpace(AssetName)))
                        {
                            Console.WriteLine("Well Mapping started...");
                            // call Delete method
                            Surface_Data_Load.AddWellMappingWithSNModel(NetworkModelName, AssetName);
                            Console.WriteLine("Well Mapped successfully...");
                        }
                        else
                        {
                            WriteLogFile(string.Format("Missing required args for deleting Surfcae Network Model : {0}  Asset Name : {1}", NetworkModelName, AssetName));
                        }
                    }
                    if (RemoveWellMapping)
                    {
                        if ((!string.IsNullOrWhiteSpace(NetworkModelName)) && (!string.IsNullOrWhiteSpace(AssetName)))
                        {
                            Console.WriteLine("Well Un-Mapping started...");
                            // call Delete method
                            Surface_Data_Load.RemoveWellMappingWithSNModel(NetworkModelName, AssetName);
                            Console.WriteLine("Well Un-Mapped successfully...");
                        }
                        else
                        {
                            WriteLogFile(string.Format("Missing required args for deleting Surfcae Network Model : {0}  Asset Name : {1}", NetworkModelName, AssetName));
                        }
                    }
                    if (AddScenario)
                    {
                        if ((!string.IsNullOrWhiteSpace(NetworkModelName)) && (!string.IsNullOrWhiteSpace(AssetName)) && (!string.IsNullOrWhiteSpace(NoOfScenario)))
                        {
                            Console.WriteLine("Adding Scenario started...");
                            // call Delete method
                            Surface_Data_Load.AddScenario(NetworkModelName, AssetName, Convert.ToInt64(NoOfScenario));
                            Console.WriteLine("Added Scenario successfully...");
                        }
                        else
                        {
                            WriteLogFile(string.Format("Missing required args for deleting Surfcae Network Model : {0}  Asset Name : {1} and No of Scenario : {2}", NetworkModelName, AssetName, NoOfScenario));
                        }
                    }
                    if (RemoveScenario)
                    {
                        if ((!string.IsNullOrWhiteSpace(NetworkModelName)) && (!string.IsNullOrWhiteSpace(AssetName)) && (!string.IsNullOrWhiteSpace(NoOfScenario)))
                        {
                            Console.WriteLine("Deleting Scenario started...");
                            // call Delete method
                            Surface_Data_Load.DeleteScenario(NetworkModelName, AssetName, Convert.ToInt64(NoOfScenario));
                            Console.WriteLine("Deleted scenario successfully...");
                        }
                        else
                        {
                            WriteLogFile(string.Format("Missing required args for deleting Surfcae Network Model : {0}  Asset Name : {1} and No of Scenario : {2}", NetworkModelName, AssetName, NoOfScenario));
                        }
                    }
                    if (AddScenarioInSchedulerAndRunOptimization)
                    {
                        if ((!string.IsNullOrWhiteSpace(NetworkModelName)) && (!string.IsNullOrWhiteSpace(AssetName)) && (!string.IsNullOrWhiteSpace(NoOfScenario)))
                        {
                            Console.WriteLine("Adding Scenario in Job Scheduler started...");
                            // call Delete method
                            Surface_Data_Load.AddScenarioInJobSchedulerAndRunOptimization(NetworkModelName, AssetName, Convert.ToInt64(NoOfScenario));
                            Console.WriteLine("Added Scenario in Job Scheduler successfully...");
                        }
                        else
                        {
                            WriteLogFile(string.Format("Missing required args for deleting Surfcae Network Model : {0}  Asset Name : {1} and No of Scenario : {2}", NetworkModelName, AssetName, NoOfScenario));
                        }
                    }
                } while (false);
            }
            catch (Exception ex)
            {
                WriteLogFile("Error occurred " + ex.ToString());
            }
        }

        public static void ShowAvailableMethods()
        {
            MethodInfo[] methodInfos = typeof(APIPerformance).GetMethods(BindingFlags.Public |
                                                      BindingFlags.Static).Where(x => x.Name != "Main" && x.Name != "ShowAvailableMethods").ToArray();
            // sort methods by name
            Array.Sort(methodInfos,
                    delegate (MethodInfo methodInfo1, MethodInfo methodInfo2)
                    { return methodInfo1.Name.CompareTo(methodInfo2.Name); });

            Console.WriteLine("Available methods are: ");
            // write method names
            foreach (MethodInfo methodInfo in methodInfos)
            {
                Console.Write(@"""" + methodInfo.Name + @"""" + " ");
            }
            Console.Write(Environment.NewLine);
        }
    }
}