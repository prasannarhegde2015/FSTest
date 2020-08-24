using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using Weatherford.POP.DTOs;
using Weatherford.POP.Enums;
using System;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Configuration;
using System.Data.Odbc;
using System.Threading;

namespace Weatherford.POP.Server.IntegrationTests
{
    [TestClass]
    public class WellAllocationServiceTests : APIClientTestBase
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
            _modelFilesToRemove = new List<string>();
        }

        public decimal GetTruncatedValueforDecimal(decimal dblval, int count)
        {
            Trace.WriteLine("Decimal Value: " + dblval);
            decimal GetTruncatedValueforDecimal = 0m;
            System.Text.RegularExpressions.Regex re = new System.Text.RegularExpressions.Regex("^\\d+\\.?\\d{" + count + "}");
            System.Text.RegularExpressions.Match mt = re.Match(dblval.ToString());
            string strmth = mt.ToString();
            GetTruncatedValueforDecimal = Convert.ToDecimal(strmth);
            return GetTruncatedValueforDecimal;
        }

        public double GetTruncatedValueforDouble(double dblval, int count)
        {
            Trace.WriteLine("Double Value " + dblval);
            double GetTruncatedValueforDouble = 0.0;
            System.Text.RegularExpressions.Regex re = new System.Text.RegularExpressions.Regex("^\\d+\\.?\\d{" + count + "}");
            System.Text.RegularExpressions.Match mt = re.Match(dblval.ToString());
            string strmth = mt.ToString();
            GetTruncatedValueforDouble = Convert.ToDouble(strmth);
            return GetTruncatedValueforDouble;
        }


        [TestCategory(TestCategories.WellAllocationServiceTests), TestMethod]
        public void GetAllocationGroupTreeViewTest()
        {
            AllocationGroupDTO parentAllocationGroupDTO = new AllocationGroupDTO();
            parentAllocationGroupDTO.Name = "G1";
            parentAllocationGroupDTO.Parent = null;
            parentAllocationGroupDTO.Phase = ProductionType.Oil;
            // Add G1 Group as Parent.
            WellAllocationService.AddAllocationGroup(parentAllocationGroupDTO);

            List<AllocationGroupDTO> allAllocationGroups = WellAllocationService.GetAllAllocationGroups(parentAllocationGroupDTO.Phase.ToString());
            Assert.IsNotNull(allAllocationGroups, "Group Not found.");

            AllocationGroupDTO childAllocationGroupDTO = new AllocationGroupDTO();
            childAllocationGroupDTO.Name = "G1-Child";
            childAllocationGroupDTO.Parent = parentAllocationGroupDTO;
            childAllocationGroupDTO.Phase = ProductionType.Oil;
            childAllocationGroupDTO.ParentId = allAllocationGroups[0].Id;
            // Add G1-Child group with G1 as Parent.
            WellAllocationService.AddAllocationGroup(childAllocationGroupDTO);

            allAllocationGroups = WellAllocationService.GetAllAllocationGroups(childAllocationGroupDTO.Phase.ToString());
            Assert.IsNotNull(allAllocationGroups, "Group Not found.");

            AllocationGroupDTO grandChildAllocationGroupDTO = new AllocationGroupDTO();
            grandChildAllocationGroupDTO.Name = "G1-GrandChild";
            grandChildAllocationGroupDTO.Parent = childAllocationGroupDTO;
            grandChildAllocationGroupDTO.Phase = ProductionType.Oil;
            grandChildAllocationGroupDTO.ParentId = allAllocationGroups[1].Id;

            // This adds all three allocation groups to the DB with each Parent property point to the node before it.
            // Add G1-GrandChild group with G1-Child as Parent.
            WellAllocationService.AddAllocationGroup(grandChildAllocationGroupDTO);

            // Once we've added the allocation groups to the DB, we have to retrieve them and add each record to the items to delete during cleanup.
            // We have to do this because prior to saving the records have no Id's to use for the deletion process.
            List<AllocationGroupDTO> savedAllocationGroups = WellAllocationService.GetAllAllocationGroups(parentAllocationGroupDTO.Phase.ToString());
            Assert.AreEqual(savedAllocationGroups.Count, 3, "Expected node count: 3  Actual node count: " + savedAllocationGroups.Count);
            _allocationGroupsToRemove.Add(savedAllocationGroups[2]);
            _allocationGroupsToRemove.Add(savedAllocationGroups[1]);
            _allocationGroupsToRemove.Add(savedAllocationGroups[0]);

            List<AllocationGroupNodeDTO> allocationGroupTreeView = WellAllocationService.GetAllocationGroupTreeView("1");

            Assert.AreEqual(allocationGroupTreeView.Count, 2, "Expected top node count: 2  Actual node count: " + allocationGroupTreeView.Count);
            Assert.AreEqual(allocationGroupTreeView[0].Name, "Unallocated Wells", "Expect top node name: \"Unallocated Wells\"  Actual top node name: " + allocationGroupTreeView[0].Name);

            Assert.AreEqual(allocationGroupTreeView[1].Name, "G1", "Expected parent node name: \"G1\"  Actual parent node name: " + allocationGroupTreeView[1].Name);
            Assert.AreEqual(allocationGroupTreeView[1].Children.Count, 1, "Expected number of first child nodes: 1  Actual number of first child nodes: " + allocationGroupTreeView[1].Children.Count);
            Assert.AreEqual(allocationGroupTreeView[1].Children[0].Name, "G1-Child", "Expected first child node name: \"G1-Child\"  Actual first child node name: " + allocationGroupTreeView[1].Children[0].Name);

            Assert.AreEqual(allocationGroupTreeView[1].Children[0].Children.Count, 1, "Expected number of second child nodes: 1  Actual number of second child nodes: " + allocationGroupTreeView[1].Children[0].Children.Count);
            Assert.AreEqual(allocationGroupTreeView[1].Children[0].Children[0].Name, "G1-GrandChild", "Expected second child node name: \"G1-GrandChild\"  Actual second child node name: " + allocationGroupTreeView[1].Children[0].Children[0].Name);

            Assert.AreEqual(allocationGroupTreeView[1].Children[0].Children[0].Children.Count, 0, "Second child node should not have children.");
        }

        [TestCategory(TestCategories.WellAllocationServiceTests), TestMethod]
        public void AddGetUpdateRemoveAllocationGroupTest()
        {
            AllocationGroupDTO allocationGroupDTO = new AllocationGroupDTO();

            // ---------------- Test for Add and Get ----------------

            // Create entry and set the properties
            allocationGroupDTO.DataConnection = null;
            allocationGroupDTO.EquipmentType = AllocationGroupEquipmentType.Meter;
            allocationGroupDTO.FacilityId = "123";
            allocationGroupDTO.GroupType = AllocationGroupType.Physical;
            allocationGroupDTO.Name = "Oil Group";
            allocationGroupDTO.Parent = null;
            allocationGroupDTO.Phase = ProductionType.Oil;
            allocationGroupDTO.SurfaceLatitude = (decimal)1234.56;
            allocationGroupDTO.SurfaceLongitude = (decimal)567.89;

            // Add record to DB
            WellAllocationService.AddAllocationGroup(allocationGroupDTO);

            // Get same record from DB and test properties
            List<AllocationGroupDTO> allAllocationGroups = WellAllocationService.GetAllAllocationGroups(allocationGroupDTO.Phase.ToString());
            Assert.AreEqual(allAllocationGroups.Count, 1, "Expected # of allocation groups: \"1\"  Number read from DB: " + allAllocationGroups.Count.ToString());
            AllocationGroupDTO allocationGroup = allAllocationGroups[0];

            _allocationGroupsToRemove.Add(allocationGroup);

            Assert.IsNull(allocationGroupDTO.DataConnection, "Data connection should be null");
            Assert.AreEqual(allocationGroupDTO.EquipmentType, AllocationGroupEquipmentType.Meter, "Expected equipment type: \"4\"  Equipment type read from DB: " + allocationGroupDTO.EquipmentType);
            Assert.AreEqual(allocationGroupDTO.FacilityId, "123", "Expected facility id: \"123\"  Equipment type read from DB: " + allocationGroupDTO.FacilityId);
            Assert.AreEqual(allocationGroupDTO.GroupType, AllocationGroupType.Physical, "Expected group type: \"1\"  Equipment type read from DB: " + allocationGroupDTO.GroupType);
            Assert.AreEqual(allocationGroup.Name, "Oil Group", "Expected group name: \"Oil Group\"  Name read from DB: " + allocationGroup.Name);
            Assert.IsNull(allocationGroupDTO.Parent);
            Assert.AreEqual(allocationGroup.Phase, ProductionType.Oil, "Expected phase: \"1\"  Phase read from DB: " + allocationGroup.Phase);
            Assert.AreEqual(allocationGroup.SurfaceLatitude, (decimal?)1234.56, "Expected latitude: \"1234.56\"  Latitude read from DB: " + allocationGroup.SurfaceLatitude);
            Assert.AreEqual(allocationGroup.SurfaceLongitude, (decimal?)567.89, "Expected longitude: \"567.89\"  Longitude read from DB: " + allocationGroup.SurfaceLongitude);

            // ------------------ Test for Update ------------------

            // Modify exising values
            allocationGroup.EquipmentType = AllocationGroupEquipmentType.Pipe;
            allocationGroup.FacilityId = "456";
            allocationGroup.GroupType = AllocationGroupType.Virtual;
            allocationGroup.Name = "Water Group";
            allocationGroup.Parent = null;
            allocationGroup.Phase = ProductionType.Water;
            allocationGroup.SurfaceLatitude = (decimal)789.10;
            allocationGroup.SurfaceLongitude = (decimal)23456.78;

            // Update the record
            WellAllocationService.UpdateAllocationGroupInfo(allocationGroup);

            // Read and test the updated record
            allAllocationGroups = WellAllocationService.GetAllAllocationGroups(allocationGroup.Phase.ToString());
            Assert.AreEqual(allAllocationGroups.Count, 1, "Expected # of allocation groups: \"1\"  Number read from DB: " + allAllocationGroups.Count.ToString());
            allocationGroup = allAllocationGroups[0];
            Assert.IsNull(allocationGroup.DataConnection);
            Assert.AreEqual(allocationGroup.EquipmentType, AllocationGroupEquipmentType.Pipe, "Expected equipment type: \"1\"  Equipment type read from DB: " + allocationGroupDTO.EquipmentType);
            Assert.AreEqual(allocationGroup.FacilityId, "456", "Expected facility id: \"456\"  Equipment type read from DB: " + allocationGroupDTO.FacilityId);
            Assert.AreEqual(allocationGroup.GroupType, AllocationGroupType.Virtual, "Expected group type: \"2\"  Equipment type read from DB: " + allocationGroupDTO.GroupType);
            Assert.AreEqual(allocationGroup.Name, "Water Group", "Expected group name: \"Water Group\"  Name read from DB: " + allocationGroup.Name);
            Assert.IsNull(allocationGroup.Parent);
            Assert.AreEqual(allocationGroup.Phase, ProductionType.Water, "Expected phase: \"2\"  Phase read from DB: " + allocationGroup.Phase);
            Assert.AreEqual(allocationGroup.SurfaceLatitude, (decimal)789.10, "Expected latitude: \"789.10\"  Latitude read from DB: " + allocationGroup.SurfaceLatitude);
            Assert.AreEqual(allocationGroup.SurfaceLongitude, (decimal)23456.78, "Expected longitude: \"23456.78\"  Longitude read from DB: " + allocationGroup.SurfaceLongitude);

            // ----------------- Test for Remove -----------------
            WellAllocationService.RemoveAllocationGroupsById(allocationGroup.Id.ToString());
            allAllocationGroups = WellAllocationService.GetAllAllocationGroups(allocationGroup.Phase.ToString());
            Assert.AreEqual(allAllocationGroups.Count, 0);

            // We don't need to clean up if there aren't any records in the DB
            _allocationGroupsToRemove.Clear();
        }

        [TestCategory(TestCategories.WellAllocationServiceTests), TestMethod]
        public void GetPossibleAllocationGroups()
        {
            AllocationGroupDTO parentAllocationGroupDTO = new AllocationGroupDTO();
            AllocationGroupDTO childAllocationGroupDTO = new AllocationGroupDTO();
            List<AllocationGroupDTO> allAllocationGroups = new List<AllocationGroupDTO>();

            // Add Parent Groups in Treeview
            parentAllocationGroupDTO = AddParentGroupsInTreeView("G1", ProductionType.Oil);
            // Get and verify G1 group is added successfully
            allAllocationGroups = WellAllocationService.GetAllAllocationGroups(parentAllocationGroupDTO.Phase.ToString());
            Assert.IsNotNull(allAllocationGroups, "G1 Allocation Group Not found.");
            // Add Child Groups to G1 Parent group
            childAllocationGroupDTO = AddChildGroupsInTreeView("G1-1-Child", ProductionType.Oil, parentAllocationGroupDTO, allAllocationGroups[0].Id);
            childAllocationGroupDTO = AddChildGroupsInTreeView("G1-2-Child", ProductionType.Oil, parentAllocationGroupDTO, allAllocationGroups[0].Id);

            parentAllocationGroupDTO = AddParentGroupsInTreeView("G2", ProductionType.Oil);
            // Get and verify G2 group is added successfully
            allAllocationGroups = WellAllocationService.GetAllAllocationGroups(parentAllocationGroupDTO.Phase.ToString());
            Assert.IsNotNull(allAllocationGroups, "G2 Allocation Group Not found.");
            // Add Child Groups to G1 Parent group
            childAllocationGroupDTO = AddChildGroupsInTreeView("G2-1-Child", ProductionType.Oil, parentAllocationGroupDTO, allAllocationGroups[3].Id);
            childAllocationGroupDTO = AddChildGroupsInTreeView("G2-2-Child", ProductionType.Oil, parentAllocationGroupDTO, allAllocationGroups[3].Id);

            parentAllocationGroupDTO = AddParentGroupsInTreeView("G3", ProductionType.Oil);
            // Get and verify G3 group is added successfully
            allAllocationGroups = WellAllocationService.GetAllAllocationGroups(parentAllocationGroupDTO.Phase.ToString());
            Assert.IsNotNull(allAllocationGroups, "G3 Allocation Group Not found.");
            // Add Child Groups to G1 Parent group
            childAllocationGroupDTO = AddChildGroupsInTreeView("G3-1-Child", ProductionType.Oil, parentAllocationGroupDTO, allAllocationGroups[6].Id);
            childAllocationGroupDTO = AddChildGroupsInTreeView("G3-2-Child", ProductionType.Oil, parentAllocationGroupDTO, allAllocationGroups[6].Id);

            allAllocationGroups = WellAllocationService.GetPossibleAllocationGroups(allAllocationGroups[1].Id.ToString(), "Oil");
            Assert.IsNotNull(allAllocationGroups);
            Assert.AreEqual(allAllocationGroups.Count, 8, "Expected top node count: 4  Actual node count: " + allAllocationGroups.Count);
            Assert.AreEqual(allAllocationGroups[0].Name, "G1", "Expected Possible Allocation Groups parent node name: \"G1\"  Actual parent node name: " + allAllocationGroups[0].Name);
            Assert.AreEqual(allAllocationGroups[1].Name, "G1-2-Child", "Expected Possible Allocation Groups parent node name: \"G1-2-Child\"  Actual parent node name: " + allAllocationGroups[1].Name);
            Assert.AreEqual(allAllocationGroups[2].Name, "G2", "Expected Possible Allocation Groups parent node name: \"G2\"  Actual parent node name: " + allAllocationGroups[2].Name);
            Assert.AreEqual(allAllocationGroups[3].Name, "G2-1-Child", "Expected Possible Allocation Groups parent node name: \"G2-1-Child\"  Actual parent node name: " + allAllocationGroups[3].Name);
            Assert.AreEqual(allAllocationGroups[4].Name, "G2-2-Child", "Expected Possible Allocation Groups parent node name: \"G2-2-Child\"  Actual parent node name: " + allAllocationGroups[4].Name);
            Assert.AreEqual(allAllocationGroups[5].Name, "G3", "Expected Possible Allocation Groups parent node name: \"G2\"  Actual parent node name: " + allAllocationGroups[5].Name);
            Assert.AreEqual(allAllocationGroups[6].Name, "G3-1-Child", "Expected Possible Allocation Groups parent node name: \"G3-1-Child\"  Actual parent node name: " + allAllocationGroups[6].Name);
            Assert.AreEqual(allAllocationGroups[7].Name, "G3-2-Child", "Expected Possible Allocation Groups parent node name: \"G3-2-Child\"  Actual parent node name: " + allAllocationGroups[7].Name);

            // Get and verify G1 group is added successfully
            allAllocationGroups = WellAllocationService.GetAllAllocationGroups(parentAllocationGroupDTO.Phase.ToString());
            Assert.IsNotNull(allAllocationGroups, "Parent 1 Allocation Group Not found.");

            // Remove Groups from DB
            _allocationGroupsToRemove.Add(allAllocationGroups[6]);
            _allocationGroupsToRemove.Add(allAllocationGroups[3]);
            _allocationGroupsToRemove.Add(allAllocationGroups[0]);
        }

        /// <summary>
        /// Add a Parent group in Treeview
        /// </summary>
        /// <param name="groupName">Group Name</param>
        /// <param name="phaseName">Phase Name</param>
        /// <returns></returns>
        public AllocationGroupDTO AddParentGroupsInTreeView(string groupName, ProductionType phaseName)
        {
            // Add  Group in Treeview
            AllocationGroupDTO allocationGroupDTO = new AllocationGroupDTO
            {
                Name = groupName,
                Parent = null,
                Phase = phaseName
            };
            WellAllocationService.AddAllocationGroup(allocationGroupDTO);
            // Get and verify group is added successfully
            List<AllocationGroupDTO> allAllocationGroups = WellAllocationService.GetAllAllocationGroups(allocationGroupDTO.Phase.ToString());
            Assert.IsNotNull(allAllocationGroups, "Allocation Group Not found.");

            // Get Allocation Group Information by Id
            allocationGroupDTO = allAllocationGroups?.FirstOrDefault(w => w.Name.Equals(groupName.ToString()));
            return allocationGroupDTO;
        }

        /// <summary>
        /// Add a Child group in Treeview with respect to Parent group name and it's Id
        /// </summary>
        /// <param name="childGroupName">Child Group Name</param>
        /// <param name="phaseName">Phase Name</param>
        /// <param name="parentGroup">Parent Group</param>
        /// <param name="parentGroupId">Parent Group Id</param>
        /// <returns></returns>
        public AllocationGroupDTO AddChildGroupsInTreeView(string childGroupName, ProductionType phaseName, AllocationGroupDTO parentGroup, long parentGroupId)
        {
            // Add  Group in Treeview
            AllocationGroupDTO childAllocationGroupDTO = new AllocationGroupDTO
            {
                Name = childGroupName,
                Parent = parentGroup,
                Phase = phaseName,
                ParentId = parentGroup.Id
            };
            WellAllocationService.AddAllocationGroup(childAllocationGroupDTO);
            // Get and verify group is added successfully
            List<AllocationGroupDTO> allAllocationGroups = WellAllocationService.GetAllAllocationGroups(childAllocationGroupDTO.Phase.ToString());
            Assert.IsNotNull(allAllocationGroups, "Allocation Group Not found.");

            // Get Allocation Group Information by Id
            childAllocationGroupDTO = allAllocationGroups?.FirstOrDefault(w => w.Name.Equals(childGroupName.ToString()));
            return childAllocationGroupDTO;
        }

        [TestCategory(TestCategories.WellAllocationServiceTests), TestMethod]
        public void AllocateUpdateWellsToGroupsPercentageGetAllocationGroupsIdAndInfoTest()
        {
            AllocationGroupDTO parentAllocationGroupDTO = new AllocationGroupDTO();
            List<AllocationGroupDTO> allAllocationGroups = new List<AllocationGroupDTO>();
            List<long> wellIds = new List<long>();
            List<long> wellAllocationIds = new List<long>();
            string facilityId = GetFacilityId("ESPWELL_", 1);

            #region Well Configuration
            WellDTO newWell = AddNonRRLWell(facilityId, WellTypeId.ESP, false, CalibrationMethodId.LFactor);
            Assert.IsNotNull(newWell);
            wellIds.Add(newWell.Id);
            #endregion Well Configuration

            // Add G1 Parent Groups in Treeview
            parentAllocationGroupDTO = AddParentGroupsInTreeView("G1", ProductionType.Oil);
            // Get and verify G1 group is added successfully
            allAllocationGroups = WellAllocationService.GetAllAllocationGroups(parentAllocationGroupDTO.Phase.ToString());
            Assert.IsNotNull(allAllocationGroups, "Parent G1 Allocation Group Not found.");

            // Assign that well to G1 group and Allocate the percentage
            WellAllocationDTO wellAllocationDTO = new WellAllocationDTO
            {
                WellId = newWell.Id,
                Well = newWell,
                AllocationGroup = allAllocationGroups[0],
                AllocationGroupId = allAllocationGroups[0].Id,
                AllocationPercent = 10
            };

            // Prepare List of WellAllocationDTO for AssignWellOutputDTO
            List<WellAllocationDTO> listWellAllocation = new List<WellAllocationDTO>
            {
                wellAllocationDTO
            };
            // Assign List of WellAllocationDTO and WellIds to AssignWellOutputDTO
            AssignWellOutputDTO assignWellOutputDTO = new AssignWellOutputDTO
            {
                WellAllocations = listWellAllocation,
                WellIds = wellIds,
                PhaseType = ProductionType.Oil
            };
            WellAllocationService.AllocateWellsToGroups(assignWellOutputDTO);

            // Add G2 Parent Groups in Treeview
            parentAllocationGroupDTO = AddParentGroupsInTreeView("G2", ProductionType.Oil);
            allAllocationGroups = WellAllocationService.GetAllAllocationGroups(parentAllocationGroupDTO.Phase.ToString());
            Assert.IsNotNull(allAllocationGroups, "Parent G2 Allocation Group Not found.");

            // Assign that well to G2 group and Allocate the percentage
            wellAllocationDTO = new WellAllocationDTO
            {
                WellId = newWell.Id,
                Well = newWell,
                AllocationGroup = allAllocationGroups[0],
                AllocationGroupId = allAllocationGroups[0].Id,
                AllocationPercent = 10
            };
            listWellAllocation.Add(wellAllocationDTO);

            wellAllocationDTO = new WellAllocationDTO
            {
                WellId = newWell.Id,
                Well = newWell,
                AllocationGroup = allAllocationGroups[1],
                AllocationGroupId = allAllocationGroups[1].Id,
                AllocationPercent = 20
            };
            listWellAllocation.Add(wellAllocationDTO);
            assignWellOutputDTO.WellAllocations = listWellAllocation;
            assignWellOutputDTO.WellIds = wellIds;
            WellAllocationService.AllocateWellsToGroups(assignWellOutputDTO);

            // Verified that Allocated and Allocation percentage are not more than 100%.
            listWellAllocation.Clear();
            listWellAllocation = WellAllocationService.GetWellsForAllocationGroup(new WellFilterDTO(), "-1", "1");
            Assert.IsNotNull(listWellAllocation);
            Assert.AreEqual(30, listWellAllocation[0].AllocatedPercent, "Allocated Percentage is mismatched.");
            Assert.AreEqual(70, listWellAllocation[0].AllocationPercent, "AllocationPercent Percentage is mismatched.");
            listWellAllocation.Clear();

            // Add G3 Group in Treeview
            parentAllocationGroupDTO = AddParentGroupsInTreeView("G3", ProductionType.Oil);
            allAllocationGroups = WellAllocationService.GetAllAllocationGroups(parentAllocationGroupDTO.Phase.ToString());
            Assert.IsNotNull(allAllocationGroups, "Parent G3 Allocation Group Not found.");

            // Update that well to G3 group and Allocate the percentage
            wellAllocationDTO = new WellAllocationDTO
            {
                WellId = newWell.Id,
                Well = newWell,
                AllocationGroup = allAllocationGroups[0],
                AllocationGroupId = allAllocationGroups[0].Id,
                AllocationPercent = 10
            };
            listWellAllocation.Add(wellAllocationDTO);

            wellAllocationDTO = new WellAllocationDTO
            {
                WellId = newWell.Id,
                Well = newWell,
                AllocationGroup = allAllocationGroups[2],
                AllocationGroupId = allAllocationGroups[2].Id,
                AllocationPercent = 40
            };
            listWellAllocation.Add(wellAllocationDTO);

            //Update Well from G2 group to G3 group and modify the allocation            
            assignWellOutputDTO.WellAllocations = listWellAllocation;
            assignWellOutputDTO.WellIds = wellIds;
            WellAllocationService.AllocateWellsToGroups(assignWellOutputDTO);

            // Verified that Allocated and Allocation percentage are not more than 100%.
            listWellAllocation.Clear();
            listWellAllocation = WellAllocationService.GetWellsForAllocationGroup(new WellFilterDTO(), "-1", "1");
            Assert.IsNotNull(listWellAllocation);
            Assert.AreEqual(50, listWellAllocation[0].AllocatedPercent, "Allocated Percentage is mismatched.");
            Assert.AreEqual(50, listWellAllocation[0].AllocationPercent, "AllocationPercent Percentage is mismatched.");
            listWellAllocation.Clear();

            listWellAllocation = WellAllocationService.GetWellAllocationGroupingById(newWell.Id.ToString(), parentAllocationGroupDTO.Phase.ToString());

            // Get Allocation Group Information by Id
            AllocationGroupDTO allocationGroupInformationDTO = WellAllocationService.GetAllocationGroupInfoById(allAllocationGroups[0].Id.ToString());
            Assert.IsNotNull(allocationGroupInformationDTO);
            Assert.AreEqual("G1", allocationGroupInformationDTO.Name, "Mismatch in Group name");
            Assert.AreEqual("Oil", allocationGroupInformationDTO.Phase.ToString(), "Mismatch in Phase");

            // Verify the Childs
            List<AllocationGroupNodeDTO> allocationGroupTreeView = WellAllocationService.GetAllocationGroupTreeView("1");
            Assert.AreEqual(allocationGroupTreeView.Count, 4, "Expected top node count: 4  Actual node count: " + allocationGroupTreeView.Count);
            Assert.AreEqual(allocationGroupTreeView[0].Name, "Unallocated Wells", "Expect top node name: \"Unallocated Wells\"  Actual top node name: " + allocationGroupTreeView[0].Name);
            Assert.AreEqual(allocationGroupTreeView[1].Name, "G1", "Expected parent node name: \"G1\"  Actual parent node name: " + allocationGroupTreeView[1].Name);
            Assert.AreEqual(allocationGroupTreeView[2].Name, "G2", "Expected parent node name: \"G2\"  Actual parent node name: " + allocationGroupTreeView[2].Name);
            Assert.AreEqual(allocationGroupTreeView[3].Name, "G3", "Expected parent node name: \"G3\"  Actual parent node name: " + allocationGroupTreeView[3].Name);

            // Check if well allocation exist 
            bool wellAllocationExits = WellAllocationService.CheckIfWellAllocationExist(listWellAllocation, parentAllocationGroupDTO.Phase.ToString());
            Assert.IsTrue(wellAllocationExits, "The Well is not exist in allocation group.");

            // Remove Well from Allocation Group
            wellAllocationIds.Add(listWellAllocation[0].Id);
            wellAllocationIds.Add(listWellAllocation[1].Id);
            WellAllocationService.RemoveWellAllocationById(wellAllocationIds);

            // Remove Groups from DB
            _allocationGroupsToRemove.Add(allAllocationGroups[2]);
            _allocationGroupsToRemove.Add(allAllocationGroups[1]);
            _allocationGroupsToRemove.Add(allAllocationGroups[0]);
        }

        [TestCategory(TestCategories.WellAllocationServiceTests), TestMethod]
        public void DragAndDropWellsToGroupsTest()
        {
            AllocationGroupDTO parentAllocationGroupDTO = new AllocationGroupDTO();
            AllocationGroupDTO childAllocationGroupDTO = new AllocationGroupDTO();
            List<AllocationGroupDTO> allAllocationGroups = new List<AllocationGroupDTO>();
            List<long> wellIds = new List<long>();
            List<long> wellAllocationIds = new List<long>();
            string facilityId = GetFacilityId("ESPWELL_", 1);

            #region Well Configuration
            WellDTO newWell1 = AddNonRRLWell(facilityId, WellTypeId.ESP, false, CalibrationMethodId.LFactor);
            Assert.IsNotNull(newWell1);
            wellIds.Add(newWell1.Id);
            #endregion Well Configuration           

            // Add Parent Groups in Treeview
            parentAllocationGroupDTO = AddParentGroupsInTreeView("G1", ProductionType.Oil);
            // Get and verify G1 group is added successfully
            allAllocationGroups = WellAllocationService.GetAllAllocationGroups(parentAllocationGroupDTO.Phase.ToString());
            Assert.IsNotNull(allAllocationGroups, "G1 Allocation Group Not found.");
            // Add Child Groups to G1 Parent group
            childAllocationGroupDTO = AddChildGroupsInTreeView("G1-1-Child", ProductionType.Oil, parentAllocationGroupDTO, allAllocationGroups[0].Id);
            childAllocationGroupDTO = AddChildGroupsInTreeView("G1-2-Child", ProductionType.Oil, parentAllocationGroupDTO, allAllocationGroups[0].Id);

            parentAllocationGroupDTO = AddParentGroupsInTreeView("G2", ProductionType.Oil);
            // Get and verify G2 group is added successfully
            allAllocationGroups = WellAllocationService.GetAllAllocationGroups(parentAllocationGroupDTO.Phase.ToString());
            Assert.IsNotNull(allAllocationGroups, "G2 Allocation Group Not found.");
            // Add Child Groups to G2 Parent group
            childAllocationGroupDTO = AddChildGroupsInTreeView("G2-1-Child", ProductionType.Oil, parentAllocationGroupDTO, allAllocationGroups[3].Id);
            childAllocationGroupDTO = AddChildGroupsInTreeView("G2-2-Child", ProductionType.Oil, parentAllocationGroupDTO, allAllocationGroups[3].Id);

            parentAllocationGroupDTO = AddParentGroupsInTreeView("G3", ProductionType.Oil);
            // Get and verify G3 group is added successfully
            allAllocationGroups = WellAllocationService.GetAllAllocationGroups(parentAllocationGroupDTO.Phase.ToString());
            Assert.IsNotNull(allAllocationGroups, "G3 Allocation Group Not found.");
            // Add Child Groups to G3 Parent group
            childAllocationGroupDTO = AddChildGroupsInTreeView("G3-1-Child", ProductionType.Oil, parentAllocationGroupDTO, allAllocationGroups[6].Id);
            childAllocationGroupDTO = AddChildGroupsInTreeView("G3-2-Child", ProductionType.Oil, parentAllocationGroupDTO, allAllocationGroups[6].Id);

            // Get and verify all groups
            allAllocationGroups = WellAllocationService.GetAllAllocationGroups(parentAllocationGroupDTO.Phase.ToString());
            Assert.IsNotNull(allAllocationGroups, "G3 Allocation Group Not found.");

            // Assign that well to G1 group and Allocate the percentage
            WellAllocationDTO wellAllocationDTO = new WellAllocationDTO
            {
                WellId = newWell1.Id,
                Well = newWell1,
                AllocationGroup = allAllocationGroups[1],
                AllocationGroupId = allAllocationGroups[1].Id,
                AllocationPercent = 10
            };

            // Prepare List of WellAllocationDTO for AssignWellOutputDTO
            List<WellAllocationDTO> listWellAllocation = new List<WellAllocationDTO>
            {
                wellAllocationDTO
            };
            // Assign List of WellAllocationDTO and WellIds to AssignWellOutputDTO
            AssignWellOutputDTO assignWellOutputDTO = new AssignWellOutputDTO
            {
                WellAllocations = listWellAllocation,
                WellIds = wellIds,
                DraggedToGroupId = listWellAllocation[0].AllocationGroup.Id,
                PhaseType = ProductionType.Oil
            };
            WellAllocationService.DragAndDropWellsToGroups(assignWellOutputDTO);

            // Get dragged and dropped well info from DB
            listWellAllocation = WellAllocationService.GetWellAllocationGroupingById(newWell1.Id.ToString(), parentAllocationGroupDTO.Phase.ToString());
            Assert.IsNotNull(listWellAllocation, " Well is not dragged and dropped in G1-1-child group.");
            Assert.AreEqual(listWellAllocation[0].AllocationGroup.Name, "G1-1-Child", " Well is not dragged and dropped in G1-1-child group.");
            Assert.AreEqual(listWellAllocation[0].AllocatedPercent, 10, " Mismatch between Well Allocated Percentage in G1-1-child group.");

            // Remove Well from Allocation Group
            wellAllocationIds.Add(listWellAllocation[0].Id);
            WellAllocationService.RemoveWellAllocationById(wellAllocationIds);

            // Remove Groups from DB
            _allocationGroupsToRemove.Add(allAllocationGroups[6]);
            _allocationGroupsToRemove.Add(allAllocationGroups[3]);
            _allocationGroupsToRemove.Add(allAllocationGroups[0]);
        }


        [TestCategory(TestCategories.WellAllocationServiceTests), TestMethod]
        // Integration test for only one Physical group
        public void ComputeAndSaveAllocationVolumesTest1()
        {
            // TODOQA: Enable in ATS once it's configured.
            //if (LogNotConfiguredIfRunningInATS())
            //{
            //    return;*/27
            //}

            DateTime start = DateTime.UtcNow.Date.AddDays(-1);
            DateTime end = DateTime.UtcNow.Date;
            //// The target rate assigned to a well 
            double? dailyAverage = 300;
            WellDailyAverageValueDTO dailyAverageDTO;

            ////Created Asset Asset Test
            string assetName = "AssetTest";
            SurfaceNetworkService.AddAsset(new AssetDTO() { Name = assetName, Description = "Test Description" });
            var allAssets = SurfaceNetworkService.GetAllAssets().ToList();
            AssetDTO asset = allAssets?.FirstOrDefault(a => a.Name.Equals(assetName));
            _assetsToRemove.Add(asset);

            //// Wells Creation (2 ESP wells, 3 GL wells)
            WellConfigDTO well_Config;
            //ESP Well creation
            WellDTO ESPWell1 = SetDefaultFluidType(new WellDTO() { Name = "ESPWell1", FacilityId = GetFacilityId("ESPWELL_", 1), DataConnection = GetDefaultCygNetDataConnection(), WellType = WellTypeId.ESP, IntervalAPI = "ESPWell1IntervalAPI", SubAssemblyAPI = "ESPWell1SubAssemblyAPI2", AssemblyAPI = "ESPWell1AssemblyAPI3", CommissionDate = DateTime.Today.AddYears(-2) });
            well_Config = WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = ESPWell1 });
            ESPWell1 = WellService.GetWellByName("ESPWell1");
            Trace.WriteLine("ESPWell1 created successfully");
            //AddModelFile(well_Config.Well.CommissionDate.Value.AddDays(1), well_Config.Well.WellType, well_Config.Well.Id, CalibrationMethodId.ReservoirPressure);
            _wellsToRemove.Add(well_Config.Well);
            // Add the daily average data 
            dailyAverageDTO = new WellDailyAverageValueDTO
            {
                Id = ESPWell1.Id,
                EndDateTime = end,                   // today
                StartDateTime = start,     // 3 days ago
                GasRateAllocated = 4000,
                GasRateInferred = 4100,
                OilRateAllocated = 50,
                OilRateInferred = 55,
                WaterRateAllocated = 32,
                WaterRateInferred = 35,
                Status = WellDailyAverageDataStatus.Calculated,
                Duration = 24,
                GasJectionDepth = 1000,
                ChokeDiameter = 64.0,
                RunTime = 24,
                THP = 492,
                THT = 213,
                GasInjectionRate = 5280,
                WellId = ESPWell1.Id,
                DHPG = 300,
                PDP = 2000,
                PIP = 100,
                MotorAmps = 34,
                FLP = 678,
            };
            SurveillanceService.AddUpdateWellDailyAverageData(dailyAverageDTO);

            ////GL wells creation
            //GLWell1
            WellDTO GLWell1 = SetDefaultFluidType(new WellDTO() { Name = "GLWell1", FacilityId = GetFacilityId("GLWELL_", 1), DataConnection = GetDefaultCygNetDataConnection(), WellType = WellTypeId.GLift, IntervalAPI = "GLWELL1IntervalAPI", SubAssemblyAPI = "GLWell1SubAssemblyAPI2", AssemblyAPI = "GLWell1AssemblyAPI3", CommissionDate = DateTime.Today.AddYears(-2) });
            well_Config = WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = GLWell1 });
            GLWell1 = WellService.GetWellByName("GLWell1");
            Trace.WriteLine("GLWell1 created successfully");
            _wellsToRemove.Add(well_Config.Well);
            //AddModelFile(well_Config.Well.CommissionDate.Value.AddDays(1), well_Config.Well.WellType, well_Config.Well.Id, CalibrationMethodId.ReservoirPressure);
            // Add a daily average data
            dailyAverageDTO = new WellDailyAverageValueDTO
            {
                Id = GLWell1.Id,
                EndDateTime = end,
                StartDateTime = start,
                GasRateAllocated = dailyAverage,
                GasRateInferred = dailyAverage,
                OilRateAllocated = dailyAverage,
                OilRateInferred = dailyAverage,
                WaterRateAllocated = dailyAverage,
                WaterRateInferred = dailyAverage,
                Status = WellDailyAverageDataStatus.Calculated,
                Duration = 24,
                GasJectionDepth = 1000,
                ChokeDiameter = 64.0,
                RunTime = 24,
                THP = 492,
                THT = 213,
                GasInjectionRate = 5280,
                WellId = GLWell1.Id,
                DHPG = 300
            };
            SurveillanceService.AddUpdateWellDailyAverageData(dailyAverageDTO);
            //SurveillanceService.AddDailyAverageFromVHSByDateRange(GLWell1.Id.ToString(), startDate, endDate);

            //GLWell2
            WellDTO GLWell2 = SetDefaultFluidType(new WellDTO() { Name = "GLWell2", FacilityId = GetFacilityId("GLWELL_", 2), DataConnection = GetDefaultCygNetDataConnection(), WellType = WellTypeId.GLift, IntervalAPI = "GLWELL2IntervalAPI", SubAssemblyAPI = "GLWell2SubAssemblyAPI2", AssemblyAPI = "GLWell2AssemblyAPI3", CommissionDate = DateTime.Today.AddYears(-2) });
            well_Config = WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = GLWell2 });
            GLWell2 = WellService.GetWellByName("GLWell2");
            Trace.WriteLine("GLWell2 created successfully");
            _wellsToRemove.Add(well_Config.Well);
            //AddModelFile(well_Config.Well.CommissionDate.Value.AddDays(1), well_Config.Well.WellType, well_Config.Well.Id, CalibrationMethodId.ReservoirPressure);
            dailyAverageDTO = new WellDailyAverageValueDTO
            {
                Id = GLWell2.Id,
                EndDateTime = end,
                StartDateTime = start,
                GasRateAllocated = dailyAverage,
                GasRateInferred = dailyAverage,
                OilRateAllocated = dailyAverage,
                OilRateInferred = dailyAverage,
                WaterRateAllocated = dailyAverage,
                WaterRateInferred = dailyAverage,
                Status = WellDailyAverageDataStatus.Calculated,
                Duration = 24,
                GasJectionDepth = 1000,
                ChokeDiameter = 64.0,
                RunTime = 24,
                THP = 492,
                THT = 213,
                GasInjectionRate = 5280,
                WellId = GLWell2.Id,
                DHPG = 300
            };
            // Add the daily average to the database
            SurveillanceService.AddUpdateWellDailyAverageData(dailyAverageDTO);

            //GLWell3
            WellDTO GLWell3 = SetDefaultFluidType(new WellDTO() { Name = "GLWell3", FacilityId = GetFacilityId("GLWELL_", 3), DataConnection = GetDefaultCygNetDataConnection(), WellType = WellTypeId.GLift, IntervalAPI = "GLWELL3IntervalAPI", SubAssemblyAPI = "GLWell3SubAssemblyAPI2", AssemblyAPI = "GLWell3AssemblyAPI3", CommissionDate = DateTime.Today.AddYears(-2) });
            well_Config = WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = GLWell3 });
            GLWell3 = WellService.GetWellByName("GLWell3");
            Trace.WriteLine("GLWell3 created successfully");
            _wellsToRemove.Add(well_Config.Well);
            //AddModelFile(well_Config.Well.CommissionDate.Value.AddDays(1), well_Config.Well.WellType, well_Config.Well.Id, CalibrationMethodId.ReservoirPressure);
            //SurveillanceService.AddDailyAverageFromVHSByDateRange(GLWell3.Id.ToString(), startDate, endDate);
            dailyAverageDTO = new WellDailyAverageValueDTO
            {
                Id = GLWell3.Id,
                EndDateTime = end,
                StartDateTime = start,
                GasRateAllocated = dailyAverage,
                GasRateInferred = dailyAverage,
                OilRateAllocated = dailyAverage,
                OilRateInferred = dailyAverage,
                WaterRateAllocated = dailyAverage,
                WaterRateInferred = dailyAverage,
                Status = WellDailyAverageDataStatus.Calculated,
                Duration = 24,
                GasJectionDepth = 1000,
                ChokeDiameter = 64.0,
                RunTime = 24,
                THP = 492,
                THT = 213,
                GasInjectionRate = 5280,
                WellId = GLWell3.Id,
                DHPG = 300
            };
            // Add the daily average to the database
            SurveillanceService.AddUpdateWellDailyAverageData(dailyAverageDTO);

            //ESPWell2 well creation
            WellDTO ESPWell2 = SetDefaultFluidType(new WellDTO() { Name = "ESPWell2", FacilityId = GetFacilityId("ESPWELL_", 2), DataConnection = GetDefaultCygNetDataConnection(), WellType = WellTypeId.ESP, IntervalAPI = "ESPWell2IntervalAPI", SubAssemblyAPI = "ESPWell2SubAssemblyAPI2", AssemblyAPI = "ESPWell2AssemblyAPI3", CommissionDate = DateTime.Today.AddYears(-2) });
            well_Config = WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = ESPWell2 });
            ESPWell2 = WellService.GetWellByName("ESPWell2");
            Trace.WriteLine("ESPWell2 created successfully");
            //AddModelFile(well_Config.Well.CommissionDate.Value.AddDays(1), well_Config.Well.WellType, well_Config.Well.Id, CalibrationMethodId.ReservoirPressure);
            _wellsToRemove.Add(well_Config.Well);
            dailyAverageDTO = new WellDailyAverageValueDTO
            {
                Id = ESPWell2.Id,
                EndDateTime = end,                   // today
                StartDateTime = start,     // 3 days ago
                GasRateAllocated = 4000,
                GasRateInferred = 4100,
                OilRateAllocated = 50,
                OilRateInferred = 55,
                WaterRateAllocated = 32,
                WaterRateInferred = 35,
                Status = WellDailyAverageDataStatus.Calculated,
                Duration = 24,
                GasJectionDepth = 1000,
                ChokeDiameter = 64.0,
                RunTime = 24,
                THP = 492,
                THT = 213,
                GasInjectionRate = 5280,
                WellId = ESPWell2.Id,
                DHPG = 300,
                PDP = 2000,
                PIP = 100,
                MotorAmps = 34,
                FLP = 678,
            };
            SurveillanceService.AddUpdateWellDailyAverageData(dailyAverageDTO);

            ////Create all Allocation Group
            AllocationGroupDTO parentAllocationGroupDTO = new AllocationGroupDTO();

            AllocationGroupDTO childAllocationGroupDTO1 = new AllocationGroupDTO();
            AllocationGroupDTO childAllocationGroupDTO2 = new AllocationGroupDTO();
            AllocationGroupDTO leafAllocationGroupDTO1 = new AllocationGroupDTO();
            AllocationGroupDTO leafAllocationGroupDTO2 = new AllocationGroupDTO();
            AllocationGroupDTO leafAllocationGroupDTO3 = new AllocationGroupDTO();
            AllocationGroupDTO leafAllocationGroupDTO4 = new AllocationGroupDTO();
            AllocationGroupDTO leafAllocationGroupDTO5 = new AllocationGroupDTO();
            List<AllocationGroupDTO> allAllocationGroups = new List<AllocationGroupDTO>();

            parentAllocationGroupDTO = AddParentGroupsInTreeView("G1", ProductionType.Oil);
            childAllocationGroupDTO1 = AddChildGroupsInTreeView("G2", ProductionType.Oil, parentAllocationGroupDTO, parentAllocationGroupDTO.Id);
            childAllocationGroupDTO2 = AddChildGroupsInTreeView("G3", ProductionType.Oil, parentAllocationGroupDTO, parentAllocationGroupDTO.Id);
            leafAllocationGroupDTO1 = AddChildGroupsInTreeView("G4", ProductionType.Oil, childAllocationGroupDTO1, childAllocationGroupDTO1.Id);
            leafAllocationGroupDTO2 = AddChildGroupsInTreeView("G5", ProductionType.Oil, childAllocationGroupDTO1, childAllocationGroupDTO1.Id);
            leafAllocationGroupDTO3 = AddChildGroupsInTreeView("G6", ProductionType.Oil, childAllocationGroupDTO2, childAllocationGroupDTO2.Id);
            leafAllocationGroupDTO4 = AddChildGroupsInTreeView("G7", ProductionType.Oil, childAllocationGroupDTO2, childAllocationGroupDTO2.Id);
            leafAllocationGroupDTO5 = AddChildGroupsInTreeView("G8", ProductionType.Oil, childAllocationGroupDTO2, childAllocationGroupDTO2.Id);

            //Configure the property of parent  group G1
            parentAllocationGroupDTO.DataConnection = GetDefaultCygNetDataConnection();
            DataConnectionDTO dataConnection = DataConnectionService.GetAllDataConnections().FirstOrDefault(dc => dc.ProductionDomain == s_domain && dc.Site == s_site && dc.Service == s_cvsService);
            parentAllocationGroupDTO.DataConnectionId = dataConnection.Id;
            parentAllocationGroupDTO.EquipmentType = AllocationGroupEquipmentType.Meter;
            parentAllocationGroupDTO.FacilityId = "TANK_BATTERY_1";
            parentAllocationGroupDTO.GroupType = AllocationGroupType.Physical;

            WellAllocationService.UpdateAllocationGroupInfo(parentAllocationGroupDTO);

            ////Allocate Percentage To different Groups
            // Assign ESPwell1 to  group and Allocate the percentage(100% to G4)
            List<WellAllocationDTO> listWellAllocation = new List<WellAllocationDTO>();
            List<long> wellIds = new List<long>();
            AssignWellOutputDTO assignWellOutputDTO = null;

            WellAllocationDTO wellAllocationDTO_ESP1 = new WellAllocationDTO
            {
                WellId = ESPWell1.Id,
                Well = ESPWell1,
                AllocationGroup = leafAllocationGroupDTO1,
                AllocationGroupId = leafAllocationGroupDTO1.Id,
                AllocationPercent = 100
            };
            listWellAllocation.Clear();
            wellIds.Clear();
            listWellAllocation.Add(wellAllocationDTO_ESP1);
            wellIds.Add(ESPWell1.Id);

            // Assign List of WellAllocationDTO and WellIds to AssignWellOutputDTO
            assignWellOutputDTO = new AssignWellOutputDTO
            {
                WellAllocations = listWellAllocation,
                WellIds = wellIds,
            };
            WellAllocationService.AllocateWellsToGroups(assignWellOutputDTO);

            // Assign GLWell1 well to group and Allocate the percentage(100 % to G5)
            WellAllocationDTO wellAllocationDTO_GL1 = new WellAllocationDTO
            {
                WellId = GLWell1.Id,
                Well = GLWell1,
                AllocationGroup = leafAllocationGroupDTO2,
                AllocationGroupId = leafAllocationGroupDTO2.Id,
                AllocationPercent = 100
            };

            listWellAllocation.Clear();
            wellIds.Clear();
            listWellAllocation.Add(wellAllocationDTO_GL1);
            wellIds.Add(GLWell1.Id);
            assignWellOutputDTO = null;

            // Assign List of WellAllocationDTO and WellIds to AssignWellOutputDTO
            assignWellOutputDTO = new AssignWellOutputDTO
            {
                WellAllocations = listWellAllocation,
                WellIds = wellIds,
            };
            WellAllocationService.AllocateWellsToGroups(assignWellOutputDTO);

            // Assign GLWell2 well to group and Allocate the percentage(80 % to G5 and 20% to G6)
            WellAllocationDTO wellAllocationDTO_GL2 = new WellAllocationDTO
            {
                WellId = GLWell2.Id,
                Well = GLWell2,
                AllocationGroup = leafAllocationGroupDTO2,
                AllocationGroupId = leafAllocationGroupDTO2.Id,
                AllocationPercent = 80
            };

            WellAllocationDTO wellAllocationDTO_GL2_2 = new WellAllocationDTO
            {
                WellId = GLWell2.Id,
                Well = GLWell2,
                AllocationGroup = leafAllocationGroupDTO3,
                AllocationGroupId = leafAllocationGroupDTO3.Id,
                AllocationPercent = 20
            };

            listWellAllocation.Clear();
            wellIds.Clear();
            listWellAllocation.Add(wellAllocationDTO_GL2);
            listWellAllocation.Add(wellAllocationDTO_GL2_2);
            wellIds.Add(GLWell2.Id);
            assignWellOutputDTO = null;

            // Assign List of WellAllocationDTO and WellIds to AssignWellOutputDTO
            assignWellOutputDTO = new AssignWellOutputDTO
            {
                WellAllocations = listWellAllocation,
                WellIds = wellIds,
            };
            WellAllocationService.AllocateWellsToGroups(assignWellOutputDTO);

            // Assign GLWell3 well to group and Allocate the percentage(100% to G7)
            WellAllocationDTO wellAllocationDTO_GL3 = new WellAllocationDTO
            {
                WellId = GLWell3.Id,
                Well = GLWell3,
                AllocationGroup = leafAllocationGroupDTO4,
                AllocationGroupId = leafAllocationGroupDTO4.Id,
                AllocationPercent = 100
            };

            listWellAllocation.Clear();
            wellIds.Clear();
            listWellAllocation.Add(wellAllocationDTO_GL3);

            wellIds.Add(GLWell3.Id);
            assignWellOutputDTO = null;

            // Assign List of WellAllocationDTO and WellIds to AssignWellOutputDTO
            assignWellOutputDTO = new AssignWellOutputDTO
            {
                WellAllocations = listWellAllocation,
                WellIds = wellIds,
            };
            WellAllocationService.AllocateWellsToGroups(assignWellOutputDTO);

            // Assign ESPWell2 well to group and Allocate the percentage(50% to G7 and 50% to G8)
            WellAllocationDTO wellAllocationDTO_NF1 = new WellAllocationDTO
            {
                WellId = ESPWell2.Id,
                Well = ESPWell2,
                AllocationGroup = leafAllocationGroupDTO4,
                AllocationGroupId = leafAllocationGroupDTO4.Id,
                AllocationPercent = 50
            };
            WellAllocationDTO wellAllocationDTO_NF1_2 = new WellAllocationDTO
            {
                WellId = ESPWell2.Id,
                Well = ESPWell2,
                AllocationGroup = leafAllocationGroupDTO5,
                AllocationGroupId = leafAllocationGroupDTO5.Id,
                AllocationPercent = 50
            };

            listWellAllocation.Clear();
            wellIds.Clear();
            listWellAllocation.Add(wellAllocationDTO_NF1);
            listWellAllocation.Add(wellAllocationDTO_NF1_2);

            wellIds.Add(ESPWell2.Id);
            assignWellOutputDTO = null;

            // Assign List of WellAllocationDTO and WellIds to AssignWellOutputDTO
            assignWellOutputDTO = new AssignWellOutputDTO
            {
                WellAllocations = listWellAllocation,
                WellIds = wellIds,
            };
            WellAllocationService.AllocateWellsToGroups(assignWellOutputDTO);

            string start_date = (start).ToUniversalTime().ToISO8601();
            string end_date = end.ToUniversalTime().ToISO8601();
            double? totalInferredOilRate = 0.0;
            double? totalInferredOilRateUseEstRateasAlloc = 0.0;

            ////Use the API to Compute
            ////*********************************************************************
            try
            {
                //var dailyAverageRecord;
                WellAllocationService.SaveAllocationRatesInWellDailyAverage(ProductionType.Oil.ToString(), start_date, end_date);
                foreach (WellDTO wellId in _wellsToRemove)
                {
                    var dailyAverageRecord = SurveillanceService.GetDailyAverages(wellId.Id.ToString(), start_date, end_date).Values.FirstOrDefault();
                    totalInferredOilRate = totalInferredOilRate + dailyAverageRecord.OilRateAllocated;
                }

                //assign facility which doesn't exists, hence measuredVolToBeAllocated will be zero. 
                parentAllocationGroupDTO.FacilityId = "TANK_BATTERY_5";
                WellAllocationService.UpdateAllocationGroupInfo(parentAllocationGroupDTO);

                SystemSettingDTO systemSetting = SettingService.GetSystemSettingByName(SettingServiceStringConstants.ALLOCATION_DEFAULT_ACTION);
                SettingService.SaveSystemSetting(new SystemSettingDTO
                {
                    SettingId = systemSetting.Id,
                    Setting = systemSetting.Setting,
                    StringValue = AllocationDefaultAction.UseEstimatedRateAsAllocatedRate.ToString(),
                });

                //var dailyAverageRecord;
                WellAllocationService.SaveAllocationRatesInWellDailyAverage(ProductionType.Oil.ToString(), start_date, end_date);
                foreach (WellDTO wellId in _wellsToRemove)
                {
                    var dailyAverageRecord = SurveillanceService.GetDailyAverages(wellId.Id.ToString(), start_date, end_date).Values.FirstOrDefault();
                    totalInferredOilRateUseEstRateasAlloc = totalInferredOilRateUseEstRateasAlloc + dailyAverageRecord.OilRateAllocated;
                }

                Assert.AreEqual(5000, Math.Round((double)totalInferredOilRate), "Expected Rate: 5000  Actual Rate: " + totalInferredOilRate);
                Assert.AreEqual(1010, Math.Round((double)totalInferredOilRateUseEstRateasAlloc), "Expected Rate: 1010  Actual Rate: " + totalInferredOilRateUseEstRateasAlloc);

            }
            catch (Exception ex)
            {
                Trace.Write(ex.Message);
            }

            finally
            {
                ////Removing all records related to Allocation group
                WellAllocationService.RemoveAllocationGroupsById(parentAllocationGroupDTO.Id.ToString());
            }

            //Assert.AreEqual(5000, Math.Round((double)totalInferredOilRate), "Expected Rate: 5000  Actual Rate: " + totalInferredOilRate);
            //Assert.AreEqual(1010, Math.Round((double)totalInferredOilRateUseEstRateasAlloc), "Expected Rate: 1010  Actual Rate: " + totalInferredOilRateUseEstRateasAlloc);
        }


        [TestCategory(TestCategories.WellAllocationServiceTests), TestMethod]
        // Integration test for multiple Physical group
        public void ComputeAndSaveAllocationVolumesTest2()
        {
            // TODOQA: Enable in ATS once it's configured.
            //if (LogNotConfiguredIfRunningInATS())
            //{
            //    return;
            //}

            DateTime start = DateTime.UtcNow.Date.AddDays(-1);
            DateTime end = DateTime.UtcNow.Date;
            //// The target rate assigned to a well 
            double? dailyAverage = 300;
            WellDailyAverageValueDTO dailyAverageDTO;

            ////Created Asset Asset Test
            string assetName = "AssetTest";
            SurfaceNetworkService.AddAsset(new AssetDTO() { Name = assetName, Description = "Test Description" });
            var allAssets = SurfaceNetworkService.GetAllAssets().ToList();
            AssetDTO asset = allAssets?.FirstOrDefault(a => a.Name.Equals(assetName));
            _assetsToRemove.Add(asset);

            //// Wells Creation (4 ESP wells, 4 GL wells)
            WellConfigDTO well_Config;
            //ESP Well creation
            WellDTO ESPWell1 = SetDefaultFluidType(new WellDTO() { Name = "ESPWell1", FacilityId = GetFacilityId("ESPWELL_", 1), DataConnection = GetDefaultCygNetDataConnection(), WellType = WellTypeId.ESP, IntervalAPI = "ESPWell1IntervalAPI", SubAssemblyAPI = "ESPWell1SubAssemblyAPI2", AssemblyAPI = "ESPWell1AssemblyAPI3", CommissionDate = DateTime.Today.AddYears(-2) });
            well_Config = WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = ESPWell1 });
            ESPWell1 = WellService.GetWellByName("ESPWell1");
            Trace.WriteLine("ESPWell1 created successfully");
            //AddModelFile(well_Config.Well.CommissionDate.Value.AddDays(1), well_Config.Well.WellType, well_Config.Well.Id, CalibrationMethodId.ReservoirPressure);
            _wellsToRemove.Add(well_Config.Well);
            // Add the daily average data 
            dailyAverageDTO = new WellDailyAverageValueDTO
            {
                Id = ESPWell1.Id,
                EndDateTime = end,                   // today
                StartDateTime = start,     // 3 days ago
                GasRateAllocated = 4000,
                GasRateInferred = 4100,
                OilRateAllocated = 50,
                OilRateInferred = 55,
                WaterRateAllocated = 32,
                WaterRateInferred = 35,
                Status = WellDailyAverageDataStatus.Calculated,
                Duration = 24,
                GasJectionDepth = 1000,
                ChokeDiameter = 64.0,
                RunTime = 24,
                THP = 492,
                THT = 213,
                GasInjectionRate = 5280,
                WellId = ESPWell1.Id,
                DHPG = 300,
                PDP = 2000,
                PIP = 100,
                MotorAmps = 34,
                FLP = 678,
            };
            SurveillanceService.AddUpdateWellDailyAverageData(dailyAverageDTO);

            //ESPWell2 well creation
            WellDTO ESPWell2 = SetDefaultFluidType(new WellDTO() { Name = "ESPWell2", FacilityId = GetFacilityId("ESPWELL_", 2), DataConnection = GetDefaultCygNetDataConnection(), WellType = WellTypeId.ESP, IntervalAPI = "ESPWell2IntervalAPI", SubAssemblyAPI = "ESPWell2SubAssemblyAPI2", AssemblyAPI = "ESPWell2AssemblyAPI3", CommissionDate = DateTime.Today.AddYears(-2) });
            well_Config = WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = ESPWell2 });
            ESPWell2 = WellService.GetWellByName("ESPWell2");
            Trace.WriteLine("ESPWell2 created successfully");
            //AddModelFile(well_Config.Well.CommissionDate.Value.AddDays(1), well_Config.Well.WellType, well_Config.Well.Id, CalibrationMethodId.ReservoirPressure);
            _wellsToRemove.Add(well_Config.Well);
            dailyAverageDTO = new WellDailyAverageValueDTO
            {
                Id = ESPWell2.Id,
                EndDateTime = end,                   // today
                StartDateTime = start,     // 3 days ago
                GasRateAllocated = 4000,
                GasRateInferred = 4100,
                OilRateAllocated = 50,
                OilRateInferred = 55,
                WaterRateAllocated = 32,
                WaterRateInferred = 35,
                Status = WellDailyAverageDataStatus.Calculated,
                Duration = 24,
                GasJectionDepth = 1000,
                ChokeDiameter = 64.0,
                RunTime = 24,
                THP = 492,
                THT = 213,
                GasInjectionRate = 5280,
                WellId = ESPWell2.Id,
                DHPG = 300,
                PDP = 2000,
                PIP = 100,
                MotorAmps = 34,
                FLP = 678,
            };
            SurveillanceService.AddUpdateWellDailyAverageData(dailyAverageDTO);

            //ESPWell3 well creation
            WellDTO ESPWell3 = SetDefaultFluidType(new WellDTO() { Name = "ESPWell3", FacilityId = GetFacilityId("ESPWELL_", 3), DataConnection = GetDefaultCygNetDataConnection(), WellType = WellTypeId.ESP, IntervalAPI = "ESPWell3IntervalAPI", SubAssemblyAPI = "ESPWell3SubAssemblyAPI2", AssemblyAPI = "ESPWell3AssemblyAPI3", CommissionDate = DateTime.Today.AddYears(-2) });
            well_Config = WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = ESPWell3 });
            ESPWell3 = WellService.GetWellByName("ESPWell3");
            Trace.WriteLine("ESPWell3 created successfully");
            //AddModelFile(well_Config.Well.CommissionDate.Value.AddDays(1), well_Config.Well.WellType, well_Config.Well.Id, CalibrationMethodId.ReservoirPressure);
            _wellsToRemove.Add(well_Config.Well);
            dailyAverageDTO = new WellDailyAverageValueDTO
            {
                Id = ESPWell3.Id,
                EndDateTime = end,                   // today
                StartDateTime = start,     // 3 days ago
                GasRateAllocated = 4000,
                GasRateInferred = 4100,
                OilRateAllocated = 50,
                OilRateInferred = 55,
                WaterRateAllocated = 32,
                WaterRateInferred = 35,
                Status = WellDailyAverageDataStatus.Calculated,
                Duration = 24,
                GasJectionDepth = 1000,
                ChokeDiameter = 64.0,
                RunTime = 24,
                THP = 492,
                THT = 213,
                GasInjectionRate = 5280,
                WellId = ESPWell3.Id,
                DHPG = 300,
                PDP = 2000,
                PIP = 100,
                MotorAmps = 34,
                FLP = 678,
            };
            SurveillanceService.AddUpdateWellDailyAverageData(dailyAverageDTO);

            //ESPWell3 well creation
            WellDTO ESPWell4 = SetDefaultFluidType(new WellDTO() { Name = "ESPWell4", FacilityId = GetFacilityId("ESPWELL_", 4), DataConnection = GetDefaultCygNetDataConnection(), WellType = WellTypeId.ESP, IntervalAPI = "ESPWell4IntervalAPI", SubAssemblyAPI = "ESPWell4SubAssemblyAPI2", AssemblyAPI = "ESPWell4AssemblyAPI3", CommissionDate = DateTime.Today.AddYears(-2) });
            well_Config = WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = ESPWell4 });
            ESPWell4 = WellService.GetWellByName("ESPWell4");
            Trace.WriteLine("ESPWell4 created successfully");
            //AddModelFile(well_Config.Well.CommissionDate.Value.AddDays(1), well_Config.Well.WellType, well_Config.Well.Id, CalibrationMethodId.ReservoirPressure);
            _wellsToRemove.Add(well_Config.Well);
            dailyAverageDTO = new WellDailyAverageValueDTO
            {
                Id = ESPWell4.Id,
                EndDateTime = end,                   // today
                StartDateTime = start,     // 3 days ago
                GasRateAllocated = 4000,
                GasRateInferred = 4100,
                OilRateAllocated = 50,
                OilRateInferred = 55,
                WaterRateAllocated = 32,
                WaterRateInferred = 35,
                Status = WellDailyAverageDataStatus.Calculated,
                Duration = 24,
                GasJectionDepth = 1000,
                ChokeDiameter = 64.0,
                RunTime = 24,
                THP = 492,
                THT = 213,
                GasInjectionRate = 5280,
                WellId = ESPWell4.Id,
                DHPG = 300,
                PDP = 2000,
                PIP = 100,
                MotorAmps = 34,
                FLP = 678,
            };
            SurveillanceService.AddUpdateWellDailyAverageData(dailyAverageDTO);

            ////GL wells creation
            //GLWell1
            WellDTO GLWell1 = SetDefaultFluidType(new WellDTO() { Name = "GLWell1", FacilityId = GetFacilityId("GLWELL_", 1), DataConnection = GetDefaultCygNetDataConnection(), WellType = WellTypeId.GLift, IntervalAPI = "GLWELL1IntervalAPI", SubAssemblyAPI = "GLWell1SubAssemblyAPI2", AssemblyAPI = "GLWell1AssemblyAPI3", CommissionDate = DateTime.Today.AddYears(-2) });
            well_Config = WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = GLWell1 });
            GLWell1 = WellService.GetWellByName("GLWell1");
            Trace.WriteLine("GLWell1 created successfully");
            _wellsToRemove.Add(well_Config.Well);
            //AddModelFile(well_Config.Well.CommissionDate.Value.AddDays(1), well_Config.Well.WellType, well_Config.Well.Id, CalibrationMethodId.ReservoirPressure);
            // Add a daily average data
            dailyAverageDTO = new WellDailyAverageValueDTO
            {
                Id = GLWell1.Id,
                EndDateTime = end,
                StartDateTime = start,
                GasRateAllocated = dailyAverage,
                GasRateInferred = dailyAverage,
                OilRateAllocated = dailyAverage,
                OilRateInferred = dailyAverage,
                WaterRateAllocated = dailyAverage,
                WaterRateInferred = dailyAverage,
                Status = WellDailyAverageDataStatus.Calculated,
                Duration = 24,
                GasJectionDepth = 1000,
                ChokeDiameter = 64.0,
                RunTime = 24,
                THP = 492,
                THT = 213,
                GasInjectionRate = 5280,
                WellId = GLWell1.Id,
                DHPG = 300
            };
            SurveillanceService.AddUpdateWellDailyAverageData(dailyAverageDTO);
            //SurveillanceService.AddDailyAverageFromVHSByDateRange(GLWell1.Id.ToString(), startDate, endDate);

            //GLWell2
            WellDTO GLWell2 = SetDefaultFluidType(new WellDTO() { Name = "GLWell2", FacilityId = GetFacilityId("GLWELL_", 2), DataConnection = GetDefaultCygNetDataConnection(), WellType = WellTypeId.GLift, IntervalAPI = "GLWELL2IntervalAPI", SubAssemblyAPI = "GLWell2SubAssemblyAPI2", AssemblyAPI = "GLWell2AssemblyAPI3", CommissionDate = DateTime.Today.AddYears(-2) });
            well_Config = WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = GLWell2 });
            GLWell2 = WellService.GetWellByName("GLWell2");
            Trace.WriteLine("GLWell2 created successfully");
            _wellsToRemove.Add(well_Config.Well);
            //AddModelFile(well_Config.Well.CommissionDate.Value.AddDays(1), well_Config.Well.WellType, well_Config.Well.Id, CalibrationMethodId.ReservoirPressure);
            dailyAverageDTO = new WellDailyAverageValueDTO
            {
                Id = GLWell2.Id,
                EndDateTime = end,
                StartDateTime = start,
                GasRateAllocated = dailyAverage,
                GasRateInferred = dailyAverage,
                OilRateAllocated = dailyAverage,
                OilRateInferred = dailyAverage,
                WaterRateAllocated = dailyAverage,
                WaterRateInferred = dailyAverage,
                Status = WellDailyAverageDataStatus.Calculated,
                Duration = 24,
                GasJectionDepth = 1000,
                ChokeDiameter = 64.0,
                RunTime = 24,
                THP = 492,
                THT = 213,
                GasInjectionRate = 5280,
                WellId = GLWell2.Id,
                DHPG = 300
            };
            // Add the daily average to the database
            SurveillanceService.AddUpdateWellDailyAverageData(dailyAverageDTO);

            //GLWell3
            WellDTO GLWell3 = SetDefaultFluidType(new WellDTO() { Name = "GLWell3", FacilityId = GetFacilityId("GLWELL_", 3), DataConnection = GetDefaultCygNetDataConnection(), WellType = WellTypeId.GLift, IntervalAPI = "GLWELL3IntervalAPI", SubAssemblyAPI = "GLWell3SubAssemblyAPI2", AssemblyAPI = "GLWell3AssemblyAPI3", CommissionDate = DateTime.Today.AddYears(-2) });
            well_Config = WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = GLWell3 });
            GLWell3 = WellService.GetWellByName("GLWell3");
            Trace.WriteLine("GLWell3 created successfully");
            _wellsToRemove.Add(well_Config.Well);
            //AddModelFile(well_Config.Well.CommissionDate.Value.AddDays(1), well_Config.Well.WellType, well_Config.Well.Id, CalibrationMethodId.ReservoirPressure);
            //SurveillanceService.AddDailyAverageFromVHSByDateRange(GLWell3.Id.ToString(), startDate, endDate);
            dailyAverageDTO = new WellDailyAverageValueDTO
            {
                Id = GLWell3.Id,
                EndDateTime = end,
                StartDateTime = start,
                GasRateAllocated = dailyAverage,
                GasRateInferred = dailyAverage,
                OilRateAllocated = dailyAverage,
                OilRateInferred = dailyAverage,
                WaterRateAllocated = dailyAverage,
                WaterRateInferred = dailyAverage,
                Status = WellDailyAverageDataStatus.Calculated,
                Duration = 24,
                GasJectionDepth = 1000,
                ChokeDiameter = 64.0,
                RunTime = 24,
                THP = 492,
                THT = 213,
                GasInjectionRate = 5280,
                WellId = GLWell3.Id,
                DHPG = 300
            };
            // Add the daily average to the database
            SurveillanceService.AddUpdateWellDailyAverageData(dailyAverageDTO);


            //GLWell4
            WellDTO GLWell4 = SetDefaultFluidType(new WellDTO() { Name = "GLWell4", FacilityId = GetFacilityId("GLWELL_", 4), DataConnection = GetDefaultCygNetDataConnection(), WellType = WellTypeId.GLift, IntervalAPI = "GLWELL4IntervalAPI", SubAssemblyAPI = "GLWell4SubAssemblyAPI2", AssemblyAPI = "GLWell4AssemblyAPI3", CommissionDate = DateTime.Today.AddYears(-2) });
            well_Config = WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = GLWell4 });
            GLWell4 = WellService.GetWellByName("GLWell4");
            Trace.WriteLine("GLWell4 created successfully");
            _wellsToRemove.Add(well_Config.Well);
            //AddModelFile(well_Config.Well.CommissionDate.Value.AddDays(1), well_Config.Well.WellType, well_Config.Well.Id, CalibrationMethodId.ReservoirPressure);
            //SurveillanceService.AddDailyAverageFromVHSByDateRange(GLWell3.Id.ToString(), startDate, endDate);
            dailyAverageDTO = new WellDailyAverageValueDTO
            {
                Id = GLWell4.Id,
                EndDateTime = end,
                StartDateTime = start,
                GasRateAllocated = dailyAverage,
                GasRateInferred = dailyAverage,
                OilRateAllocated = dailyAverage,
                OilRateInferred = dailyAverage,
                WaterRateAllocated = dailyAverage,
                WaterRateInferred = dailyAverage,
                Status = WellDailyAverageDataStatus.Calculated,
                Duration = 24,
                GasJectionDepth = 1000,
                ChokeDiameter = 64.0,
                RunTime = 24,
                THP = 492,
                THT = 213,
                GasInjectionRate = 5280,
                WellId = GLWell4.Id,
                DHPG = 300
            };
            // Add the daily average to the database
            SurveillanceService.AddUpdateWellDailyAverageData(dailyAverageDTO);

            ////Create all Allocation Group
            AllocationGroupDTO G1 = new AllocationGroupDTO();
            AllocationGroupDTO G2 = new AllocationGroupDTO();
            AllocationGroupDTO G3 = new AllocationGroupDTO();
            AllocationGroupDTO G4 = new AllocationGroupDTO();
            AllocationGroupDTO G5 = new AllocationGroupDTO();
            AllocationGroupDTO G6 = new AllocationGroupDTO();
            AllocationGroupDTO G7 = new AllocationGroupDTO();
            AllocationGroupDTO G8 = new AllocationGroupDTO();
            AllocationGroupDTO G9 = new AllocationGroupDTO();
            // List<AllocationGroupDTO> allAllocationGroups = new List<AllocationGroupDTO>();

            G1 = AddParentGroupsInTreeView("G1", ProductionType.Oil);
            G2 = AddChildGroupsInTreeView("G2", ProductionType.Oil, G1, G1.Id);
            G3 = AddChildGroupsInTreeView("G3", ProductionType.Oil, G1, G1.Id);
            G4 = AddChildGroupsInTreeView("G4", ProductionType.Oil, G2, G2.Id);
            G5 = AddChildGroupsInTreeView("G5", ProductionType.Oil, G2, G2.Id);
            G6 = AddChildGroupsInTreeView("G6", ProductionType.Oil, G3, G3.Id);
            G7 = AddChildGroupsInTreeView("G7", ProductionType.Oil, G3, G3.Id);
            G8 = AddChildGroupsInTreeView("G8", ProductionType.Oil, G6, G6.Id);
            G9 = AddChildGroupsInTreeView("G9", ProductionType.Oil, G7, G7.Id);

            ////Configure the property of Physical  groups 

            DataConnectionDTO dataConnection = DataConnectionService.GetAllDataConnections().FirstOrDefault(dc => dc.ProductionDomain == s_domain && dc.Site == s_site && dc.Service == s_cvsService);

            //Configure the property of Physical groups G1
            G1.DataConnection = GetDefaultCygNetDataConnection();
            G1.DataConnectionId = dataConnection.Id;
            G1.EquipmentType = AllocationGroupEquipmentType.Meter;
            G1.FacilityId = "TANK_BATTERY_1";
            G1.GroupType = AllocationGroupType.Physical;
            WellAllocationService.UpdateAllocationGroupInfo(G1);

            //Configure the property of Physical  group G2
            G2.DataConnection = GetDefaultCygNetDataConnection();
            G2.DataConnectionId = dataConnection.Id;
            G2.EquipmentType = AllocationGroupEquipmentType.Meter;
            G2.FacilityId = "TANK_BATTERY_2";
            G2.GroupType = AllocationGroupType.Physical;
            WellAllocationService.UpdateAllocationGroupInfo(G2);

            //Configure the property of Physical  group G4
            G4.DataConnection = GetDefaultCygNetDataConnection();
            G4.DataConnectionId = dataConnection.Id;
            G4.EquipmentType = AllocationGroupEquipmentType.Meter;
            G4.FacilityId = "TANK_BATTERY_3";
            G4.GroupType = AllocationGroupType.Physical;
            WellAllocationService.UpdateAllocationGroupInfo(G4);

            //Configure the property of Physical  group G8
            G8.DataConnection = GetDefaultCygNetDataConnection();
            G8.DataConnectionId = dataConnection.Id;
            G8.EquipmentType = AllocationGroupEquipmentType.Meter;
            G8.FacilityId = "TANK_BATTERY_4";
            G8.GroupType = AllocationGroupType.Physical;
            WellAllocationService.UpdateAllocationGroupInfo(G8);

            ////Allocate Percentage To different Groups

            List<WellAllocationDTO> listWellAllocation = new List<WellAllocationDTO>();
            List<long> wellIds = new List<long>();
            AssignWellOutputDTO assignWellOutputDTO = null;
            // Assign ESPwell1 to  group and Allocate the percentage(100% to G1)
            WellAllocationDTO wellAllocationDTO_ESP1 = new WellAllocationDTO
            {
                WellId = ESPWell1.Id,
                Well = ESPWell1,
                AllocationGroup = G1,
                AllocationGroupId = G1.Id,
                AllocationPercent = 100
            };
            listWellAllocation.Clear();
            wellIds.Clear();
            listWellAllocation.Add(wellAllocationDTO_ESP1);
            wellIds.Add(ESPWell1.Id);

            // Assign List of WellAllocationDTO and WellIds to AssignWellOutputDTO
            assignWellOutputDTO = new AssignWellOutputDTO
            {
                WellAllocations = listWellAllocation,
                WellIds = wellIds,
            };
            WellAllocationService.AllocateWellsToGroups(assignWellOutputDTO);

            // Assign ESPwell2 to  group and Allocate the percentage(100% to G1)
            WellAllocationDTO wellAllocationDTO_ESP2 = new WellAllocationDTO
            {
                WellId = ESPWell2.Id,
                Well = ESPWell2,
                AllocationGroup = G1,
                AllocationGroupId = G1.Id,
                AllocationPercent = 100
            };
            listWellAllocation.Clear();
            wellIds.Clear();
            listWellAllocation.Add(wellAllocationDTO_ESP2);
            wellIds.Add(ESPWell2.Id);

            // Assign List of WellAllocationDTO and WellIds to AssignWellOutputDTO
            assignWellOutputDTO = new AssignWellOutputDTO
            {
                WellAllocations = listWellAllocation,
                WellIds = wellIds,
            };
            WellAllocationService.AllocateWellsToGroups(assignWellOutputDTO);

            // Assign ESPwell3 to  group and Allocate the percentage(100% to G4)
            WellAllocationDTO wellAllocationDTO_ESP3 = new WellAllocationDTO
            {
                WellId = ESPWell3.Id,
                Well = ESPWell3,
                AllocationGroup = G4,
                AllocationGroupId = G4.Id,
                AllocationPercent = 100
            };
            listWellAllocation.Clear();
            wellIds.Clear();
            listWellAllocation.Add(wellAllocationDTO_ESP3);
            wellIds.Add(ESPWell3.Id);

            // Assign List of WellAllocationDTO and WellIds to AssignWellOutputDTO
            assignWellOutputDTO = new AssignWellOutputDTO
            {
                WellAllocations = listWellAllocation,
                WellIds = wellIds,
            };
            WellAllocationService.AllocateWellsToGroups(assignWellOutputDTO);

            // Assign ESPwell4 to  group and Allocate the percentage(100% to G4)
            WellAllocationDTO wellAllocationDTO_ESP4 = new WellAllocationDTO
            {
                WellId = ESPWell4.Id,
                Well = ESPWell4,
                AllocationGroup = G4,
                AllocationGroupId = G4.Id,
                AllocationPercent = 100
            };
            listWellAllocation.Clear();
            wellIds.Clear();
            listWellAllocation.Add(wellAllocationDTO_ESP4);
            wellIds.Add(ESPWell4.Id);

            // Assign List of WellAllocationDTO and WellIds to AssignWellOutputDTO
            assignWellOutputDTO = new AssignWellOutputDTO
            {
                WellAllocations = listWellAllocation,
                WellIds = wellIds,
            };
            WellAllocationService.AllocateWellsToGroups(assignWellOutputDTO);

            // Assign GLWell1 to  group and Allocate the percentage(100% to G4)
            WellAllocationDTO wellAllocationDTO_GLWell1 = new WellAllocationDTO
            {
                WellId = GLWell1.Id,
                Well = GLWell1,
                AllocationGroup = G3,
                AllocationGroupId = G3.Id,
                AllocationPercent = 100
            };
            listWellAllocation.Clear();
            wellIds.Clear();
            listWellAllocation.Add(wellAllocationDTO_GLWell1);
            wellIds.Add(GLWell1.Id);

            // Assign List of WellAllocationDTO and WellIds to AssignWellOutputDTO
            assignWellOutputDTO = new AssignWellOutputDTO
            {
                WellAllocations = listWellAllocation,
                WellIds = wellIds,
            };
            WellAllocationService.AllocateWellsToGroups(assignWellOutputDTO);

            // Assign GLWell2 to  group and Allocate the percentage(30% to G5 and 70% to G8)

            WellAllocationDTO wellAllocationDTO_GL2 = new WellAllocationDTO
            {
                WellId = GLWell2.Id,
                Well = GLWell2,
                AllocationGroup = G5,
                AllocationGroupId = G5.Id,
                AllocationPercent = 30
            };

            WellAllocationDTO wellAllocationDTO_GL2_2 = new WellAllocationDTO
            {
                WellId = GLWell2.Id,
                Well = GLWell2,
                AllocationGroup = G8,
                AllocationGroupId = G8.Id,
                AllocationPercent = 70
            };

            listWellAllocation.Clear();
            wellIds.Clear();
            listWellAllocation.Add(wellAllocationDTO_GL2);
            listWellAllocation.Add(wellAllocationDTO_GL2_2);
            wellIds.Add(GLWell2.Id);
            assignWellOutputDTO = null;

            // Assign List of WellAllocationDTO and WellIds to AssignWellOutputDTO
            assignWellOutputDTO = new AssignWellOutputDTO
            {
                WellAllocations = listWellAllocation,
                WellIds = wellIds,
            };
            WellAllocationService.AllocateWellsToGroups(assignWellOutputDTO);

            // Assign GLWell3 to  group and Allocate the percentage(80% to G8 and 20% to G9)

            WellAllocationDTO wellAllocationDTO_GL3 = new WellAllocationDTO
            {
                WellId = GLWell3.Id,
                Well = GLWell3,
                AllocationGroup = G8,
                AllocationGroupId = G8.Id,
                AllocationPercent = 80
            };

            WellAllocationDTO wellAllocationDTO_GL3_2 = new WellAllocationDTO
            {
                WellId = GLWell3.Id,
                Well = GLWell3,
                AllocationGroup = G9,
                AllocationGroupId = G9.Id,
                AllocationPercent = 20
            };

            listWellAllocation.Clear();
            wellIds.Clear();
            listWellAllocation.Add(wellAllocationDTO_GL3);
            listWellAllocation.Add(wellAllocationDTO_GL3_2);
            wellIds.Add(GLWell3.Id);
            assignWellOutputDTO = null;

            // Assign List of WellAllocationDTO and WellIds to AssignWellOutputDTO
            assignWellOutputDTO = new AssignWellOutputDTO
            {
                WellAllocations = listWellAllocation,
                WellIds = wellIds,
            };
            WellAllocationService.AllocateWellsToGroups(assignWellOutputDTO);

            // Assign GLWell4 to  group and Allocate the percentage(100% to G7)
            WellAllocationDTO wellAllocationDTO_GLWell4 = new WellAllocationDTO
            {
                WellId = GLWell4.Id,
                Well = GLWell4,
                AllocationGroup = G7,
                AllocationGroupId = G7.Id,
                AllocationPercent = 100
            };
            listWellAllocation.Clear();
            wellIds.Clear();
            listWellAllocation.Add(wellAllocationDTO_GLWell4);
            wellIds.Add(GLWell4.Id);

            // Assign List of WellAllocationDTO and WellIds to AssignWellOutputDTO
            assignWellOutputDTO = new AssignWellOutputDTO
            {
                WellAllocations = listWellAllocation,
                WellIds = wellIds,
            };
            WellAllocationService.AllocateWellsToGroups(assignWellOutputDTO);

            string start_date = (start).ToUniversalTime().ToISO8601();
            string end_date = end.ToUniversalTime().ToISO8601();
            double? totalInferredOilRate = 0.0;

            ////Use the API to Compute
            ////*********************************************************************
            try
            {

                //var dailyAverageRecord;
                WellAllocationService.SaveAllocationRatesInWellDailyAverage(ProductionType.Oil.ToString(), start_date, end_date);
                foreach (WellDTO wellId in _wellsToRemove)
                {
                    var dailyAverageRecord = SurveillanceService.GetDailyAverages(wellId.Id.ToString(), start_date, end_date).Values.FirstOrDefault();
                    totalInferredOilRate = totalInferredOilRate + dailyAverageRecord.OilRateAllocated;
                }

                Assert.AreEqual(5000, Math.Round((double)totalInferredOilRate), "Expected Rate: 5000  Actual Rate: " + totalInferredOilRate);
            }
            catch (Exception ex)
            {
                Trace.Write(ex.Message);
            }

            finally
            {
                ////Removing all records related to Allocation group
                WellAllocationService.RemoveAllocationGroupsById(G1.Id.ToString());

            }

            // Assert.AreEqual(5000, Math.Round((double)totalInferredOilRate), "Expected Rate: 5000  Actual Rate: " + totalInferredOilRate);


        }


        /// <summary>
        /// FRWM-6797 Persist the Field Factor for each phase and each allocation group for each day
        /// </summary>

        [TestCategory(TestCategories.WellAllocationServiceTests), TestMethod]
        // Integration test for only one Physical group
        public void FeildFactor()
        {
            // TODOQA: Enable in TeamCity once it's configured.
            if (!s_isRunningInATS)
            {
                Trace.WriteLine("This test  tries to validate results from database query ; and Team City User cant access DB");
                Trace.WriteLine("Validate test results on ATS Runs ");
                return;
            }

            DateTime start = DateTime.UtcNow.Date.AddDays(-1);
            DateTime end = DateTime.UtcNow.Date;
            //// The target rate assigned to a well 
            double? dailyAverage = 300;
            WellDailyAverageValueDTO dailyAverageDTO;

            ////Created Asset Asset Test
            string assetName = "AssetTest";
            SurfaceNetworkService.AddAsset(new AssetDTO() { Name = assetName, Description = "Test Description" });
            var allAssets = SurfaceNetworkService.GetAllAssets().ToList();
            AssetDTO asset = allAssets?.FirstOrDefault(a => a.Name.Equals(assetName));
            _assetsToRemove.Add(asset);

            //// Wells Creation (2 ESP wells, 2 GL wells)
            WellConfigDTO well_Config;
            //ESP Well creation
            WellDTO ESPWell1 = SetDefaultFluidType(new WellDTO() { Name = "ESPWell1", FacilityId = GetFacilityId("ESPWELL_", 1), DataConnection = GetDefaultCygNetDataConnection(), WellType = WellTypeId.ESP, IntervalAPI = "ESPWell1IntervalAPI", SubAssemblyAPI = "ESPWell1SubAssemblyAPI2", AssemblyAPI = "ESPWell1AssemblyAPI3", CommissionDate = DateTime.Today.AddYears(-2) });
            well_Config = WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = ESPWell1 });
            ESPWell1 = WellService.GetWellByName("ESPWell1");
            Trace.WriteLine("ESPWell1 created successfully");
            //AddModelFile(well_Config.Well.CommissionDate.Value.AddDays(1), well_Config.Well.WellType, well_Config.Well.Id, CalibrationMethodId.ReservoirPressure);
            _wellsToRemove.Add(well_Config.Well);
            // Add the daily average data 
            dailyAverageDTO = new WellDailyAverageValueDTO
            {
                Id = ESPWell1.Id,
                EndDateTime = end,                   // today
                StartDateTime = start,     // 3 days ago
                GasRateAllocated = 4000,
                GasRateInferred = 4100,
                OilRateAllocated = 50,
                OilRateInferred = 55,
                WaterRateAllocated = 32,
                WaterRateInferred = 35,
                Status = WellDailyAverageDataStatus.Calculated,
                Duration = 24,
                GasJectionDepth = 1000,
                ChokeDiameter = 64.0,
                RunTime = 24,
                THP = 492,
                THT = 213,
                GasInjectionRate = 5280,
                WellId = ESPWell1.Id,
                DHPG = 300,
                PDP = 2000,
                PIP = 100,
                MotorAmps = 34,
                FLP = 678,
            };
            SurveillanceService.AddUpdateWellDailyAverageData(dailyAverageDTO);

            ////GL wells creation
            //GLWell1
            WellDTO GLWell1 = SetDefaultFluidType(new WellDTO() { Name = "GLWell1", FacilityId = GetFacilityId("GLWELL_", 1), DataConnection = GetDefaultCygNetDataConnection(), WellType = WellTypeId.GLift, IntervalAPI = "GLWELL1IntervalAPI", SubAssemblyAPI = "GLWell1SubAssemblyAPI2", AssemblyAPI = "GLWell1AssemblyAPI3", CommissionDate = DateTime.Today.AddYears(-2) });
            well_Config = WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = GLWell1 });
            GLWell1 = WellService.GetWellByName("GLWell1");
            Trace.WriteLine("GLWell1 created successfully");
            _wellsToRemove.Add(well_Config.Well);
            //AddModelFile(well_Config.Well.CommissionDate.Value.AddDays(1), well_Config.Well.WellType, well_Config.Well.Id, CalibrationMethodId.ReservoirPressure);
            // Add a daily average data
            dailyAverageDTO = new WellDailyAverageValueDTO
            {
                Id = GLWell1.Id,
                EndDateTime = end,
                StartDateTime = start,
                GasRateAllocated = dailyAverage,
                GasRateInferred = dailyAverage,
                OilRateAllocated = dailyAverage,
                OilRateInferred = dailyAverage,
                WaterRateAllocated = dailyAverage,
                WaterRateInferred = dailyAverage,
                Status = WellDailyAverageDataStatus.Calculated,
                Duration = 24,
                GasJectionDepth = 1000,
                ChokeDiameter = 64.0,
                RunTime = 24,
                THP = 492,
                THT = 213,
                GasInjectionRate = 5280,
                WellId = GLWell1.Id,
                DHPG = 300
            };
            SurveillanceService.AddUpdateWellDailyAverageData(dailyAverageDTO);
            //SurveillanceService.AddDailyAverageFromVHSByDateRange(GLWell1.Id.ToString(), startDate, endDate);

            //GLWell2
            WellDTO GLWell2 = SetDefaultFluidType(new WellDTO() { Name = "GLWell2", FacilityId = GetFacilityId("GLWELL_", 2), DataConnection = GetDefaultCygNetDataConnection(), WellType = WellTypeId.GLift, IntervalAPI = "GLWELL2IntervalAPI", SubAssemblyAPI = "GLWell2SubAssemblyAPI2", AssemblyAPI = "GLWell2AssemblyAPI3", CommissionDate = DateTime.Today.AddYears(-2) });
            well_Config = WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = GLWell2 });
            GLWell2 = WellService.GetWellByName("GLWell2");
            Trace.WriteLine("GLWell2 created successfully");
            _wellsToRemove.Add(well_Config.Well);
            //AddModelFile(well_Config.Well.CommissionDate.Value.AddDays(1), well_Config.Well.WellType, well_Config.Well.Id, CalibrationMethodId.ReservoirPressure);
            dailyAverageDTO = new WellDailyAverageValueDTO
            {
                Id = GLWell2.Id,
                EndDateTime = end,
                StartDateTime = start,
                GasRateAllocated = dailyAverage,
                GasRateInferred = dailyAverage,
                OilRateAllocated = dailyAverage,
                OilRateInferred = dailyAverage,
                WaterRateAllocated = dailyAverage,
                WaterRateInferred = dailyAverage,
                Status = WellDailyAverageDataStatus.Calculated,
                Duration = 24,
                GasJectionDepth = 1000,
                ChokeDiameter = 64.0,
                RunTime = 24,
                THP = 492,
                THT = 213,
                GasInjectionRate = 5280,
                WellId = GLWell2.Id,
                DHPG = 300
            };
            // Add the daily average to the database
            SurveillanceService.AddUpdateWellDailyAverageData(dailyAverageDTO);

            //ESPWell2 well creation
            WellDTO ESPWell2 = SetDefaultFluidType(new WellDTO() { Name = "ESPWell2", FacilityId = GetFacilityId("ESPWELL_", 2), DataConnection = GetDefaultCygNetDataConnection(), WellType = WellTypeId.ESP, IntervalAPI = "ESPWell2IntervalAPI", SubAssemblyAPI = "ESPWell2SubAssemblyAPI2", AssemblyAPI = "ESPWell2AssemblyAPI3", CommissionDate = DateTime.Today.AddYears(-2) });
            well_Config = WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = ESPWell2 });
            ESPWell2 = WellService.GetWellByName("ESPWell2");
            Trace.WriteLine("ESPWell2 created successfully");
            //AddModelFile(well_Config.Well.CommissionDate.Value.AddDays(1), well_Config.Well.WellType, well_Config.Well.Id, CalibrationMethodId.ReservoirPressure);
            _wellsToRemove.Add(well_Config.Well);
            dailyAverageDTO = new WellDailyAverageValueDTO
            {
                Id = ESPWell2.Id,
                EndDateTime = end,                   // today
                StartDateTime = start,     // 3 days ago
                GasRateAllocated = 4000,
                GasRateInferred = 4100,
                OilRateAllocated = 50,
                OilRateInferred = 55,
                WaterRateAllocated = 32,
                WaterRateInferred = 35,
                Status = WellDailyAverageDataStatus.Calculated,
                Duration = 24,
                GasJectionDepth = 1000,
                ChokeDiameter = 64.0,
                RunTime = 24,
                THP = 492,
                THT = 213,
                GasInjectionRate = 5280,
                WellId = ESPWell2.Id,
                DHPG = 300,
                PDP = 2000,
                PIP = 100,
                MotorAmps = 34,
                FLP = 678,
            };
            SurveillanceService.AddUpdateWellDailyAverageData(dailyAverageDTO);

            ////Create all Allocation Group
            AllocationGroupDTO parentAllocationGroupDTO = new AllocationGroupDTO();

            AllocationGroupDTO childAllocationGroupDTO1 = new AllocationGroupDTO();
            AllocationGroupDTO childAllocationGroupDTO2 = new AllocationGroupDTO();

            List<AllocationGroupDTO> allAllocationGroups = new List<AllocationGroupDTO>();

            parentAllocationGroupDTO = AddParentGroupsInTreeView("G1", ProductionType.Oil);
            childAllocationGroupDTO1 = AddChildGroupsInTreeView("G2", ProductionType.Oil, parentAllocationGroupDTO, parentAllocationGroupDTO.Id);
            childAllocationGroupDTO2 = AddChildGroupsInTreeView("G3", ProductionType.Oil, parentAllocationGroupDTO, parentAllocationGroupDTO.Id);


            //Configure the property of parent  group G1
            parentAllocationGroupDTO.DataConnection = GetDefaultCygNetDataConnection();
            DataConnectionDTO dataConnection = DataConnectionService.GetAllDataConnections().FirstOrDefault(dc => dc.ProductionDomain == s_domain && dc.Site == s_site && dc.Service == s_cvsService);
            parentAllocationGroupDTO.DataConnectionId = dataConnection.Id;
            parentAllocationGroupDTO.EquipmentType = AllocationGroupEquipmentType.Meter;
            parentAllocationGroupDTO.FacilityId = "TANK_BATTERY_1";
            parentAllocationGroupDTO.GroupType = AllocationGroupType.Physical;

            WellAllocationService.UpdateAllocationGroupInfo(parentAllocationGroupDTO);

            ////Allocate Percentage To different Groups
            // Assign ESPwell1 to  group and Allocate the percentage(100% to G2)
            List<WellAllocationDTO> listWellAllocation = new List<WellAllocationDTO>();
            List<long> wellIds = new List<long>();
            AssignWellOutputDTO assignWellOutputDTO = null;

            WellAllocationDTO wellAllocationDTO_ESP1 = new WellAllocationDTO
            {
                WellId = ESPWell1.Id,
                Well = ESPWell1,
                AllocationGroup = childAllocationGroupDTO1,
                AllocationGroupId = childAllocationGroupDTO1.Id,
                AllocationPercent = 100
            };
            listWellAllocation.Clear();
            wellIds.Clear();
            listWellAllocation.Add(wellAllocationDTO_ESP1);
            wellIds.Add(ESPWell1.Id);

            // Assign List of WellAllocationDTO and WellIds to AssignWellOutputDTO
            assignWellOutputDTO = new AssignWellOutputDTO
            {
                WellAllocations = listWellAllocation,
                WellIds = wellIds,
            };
            WellAllocationService.AllocateWellsToGroups(assignWellOutputDTO);

            // Assign GLWell1 well to group and Allocate the percentage(100 % to G5)
            WellAllocationDTO wellAllocationDTO_GL1 = new WellAllocationDTO
            {
                WellId = GLWell1.Id,
                Well = GLWell1,
                AllocationGroup = childAllocationGroupDTO1,
                AllocationGroupId = childAllocationGroupDTO1.Id,
                AllocationPercent = 100
            };

            listWellAllocation.Clear();
            wellIds.Clear();
            listWellAllocation.Add(wellAllocationDTO_GL1);
            wellIds.Add(GLWell1.Id);
            assignWellOutputDTO = null;

            // Assign List of WellAllocationDTO and WellIds to AssignWellOutputDTO
            assignWellOutputDTO = new AssignWellOutputDTO
            {
                WellAllocations = listWellAllocation,
                WellIds = wellIds,
            };
            WellAllocationService.AllocateWellsToGroups(assignWellOutputDTO);

            // Assign GLWell2 well to group and Allocate the percentage(100 G3)
            WellAllocationDTO wellAllocationDTO_GL2 = new WellAllocationDTO
            {
                WellId = GLWell2.Id,
                Well = GLWell2,
                AllocationGroup = childAllocationGroupDTO2,
                AllocationGroupId = childAllocationGroupDTO2.Id,
                AllocationPercent = 100
            };


            listWellAllocation.Clear();
            wellIds.Clear();
            listWellAllocation.Add(wellAllocationDTO_GL2);
            //listWellAllocation.Add(wellAllocationDTO_GL2_2);
            wellIds.Add(GLWell2.Id);
            assignWellOutputDTO = null;

            // Assign List of WellAllocationDTO and WellIds to AssignWellOutputDTO
            assignWellOutputDTO = new AssignWellOutputDTO
            {
                WellAllocations = listWellAllocation,
                WellIds = wellIds,
            };
            WellAllocationService.AllocateWellsToGroups(assignWellOutputDTO);

            // Assign ESPWell2 well to group and Allocate the percentage(100% G3)
            WellAllocationDTO wellAllocationDTO_NF1 = new WellAllocationDTO
            {
                WellId = ESPWell2.Id,
                Well = ESPWell2,
                AllocationGroup = childAllocationGroupDTO2,
                AllocationGroupId = childAllocationGroupDTO2.Id,
                AllocationPercent = 100
            };

            listWellAllocation.Clear();
            wellIds.Clear();
            listWellAllocation.Add(wellAllocationDTO_NF1);

            wellIds.Add(ESPWell2.Id);
            assignWellOutputDTO = null;

            // Assign List of WellAllocationDTO and WellIds to AssignWellOutputDTO
            assignWellOutputDTO = new AssignWellOutputDTO
            {
                WellAllocations = listWellAllocation,
                WellIds = wellIds,
            };
            WellAllocationService.AllocateWellsToGroups(assignWellOutputDTO);

            string start_date = (start).ToUniversalTime().ToISO8601();
            string end_date = end.ToUniversalTime().ToISO8601();
            double? totalInferredOilRate = 0.0;
            double? totalInferredOilRateUseEstRateasAlloc = 0.0;

            ////Use the API to Compute
            ////*********************************************************************
            try
            {
                //var dailyAverageRecord;
                WellAllocationService.SaveAllocationRatesInWellDailyAverage(ProductionType.Oil.ToString(), start_date, end_date);
                foreach (WellDTO wellId in _wellsToRemove)
                {
                    var dailyAverageRecord = SurveillanceService.GetDailyAverages(wellId.Id.ToString(), start_date, end_date).Values.FirstOrDefault();
                    totalInferredOilRate = totalInferredOilRate + dailyAverageRecord.OilRateAllocated;
                }

                //assign facility which doesn't exists, hence measuredVolToBeAllocated will be zero. 
                parentAllocationGroupDTO.FacilityId = "TANK_BATTERY_1";
                WellAllocationService.UpdateAllocationGroupInfo(parentAllocationGroupDTO);

                SystemSettingDTO systemSetting = SettingService.GetSystemSettingByName(SettingServiceStringConstants.ALLOCATION_DEFAULT_ACTION);
                SettingService.SaveSystemSetting(new SystemSettingDTO
                {
                    SettingId = systemSetting.Id,
                    Setting = systemSetting.Setting,
                    StringValue = AllocationDefaultAction.UseEstimatedRateAsAllocatedRate.ToString(),
                });

                double recordCount = 0.0;
                List<double> recordfordailyavrginferd = new List<double>();
                List<double> recordfordailyavrgAllocated = new List<double>();

                string connectionString = String.Empty;

                connectionString = s_isRunningInATS ? "Server =.\\SQLEXPRESS; Database = POP; Integrated Security = True; " : "Server=.\\SQLEXPRESS;Database=POP_TeamCity; Integrated Security=True;";
                //connectionString = s_isRunningInATS ? "Server =5CD9154JB5\\MSSQLSERVER_COPY; Database = POP_Integration; Integrated Security = True; " : "Server=5CD9154JB5\\MSSQLSERVER_COPY;Database=POP_Integration; Integrated Security=True;";
                string queryString = "select * from PAFieldFactor order by pafFieldFactorDate desc;";
                string dailyavrgquery = "Select  Well.welWellName as wellname, wdaFK_Well as wellid, wdaStartDate , wdaEndDate , wdaOilRateAllocated ,wdaOilRateInferred,wdaWaterRateAllocated,wdaWaterRateInferred , wdaGasRateAllocated ,wdaGasRateInferred from WellDailyAverage Inner join Well on WellDailyAverage.wdaFK_Well = well.welPrimaryKey ";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlCommand command = new SqlCommand(queryString, connection);

                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {

                            recordCount = Convert.ToDouble(reader["pafFieldFactor"]);

                        }
                        reader.Close();
                    }

                    SqlCommand command2 = new SqlCommand(dailyavrgquery, connection);
                    SqlDataReader reader2 = command2.ExecuteReader();
                    if (reader2.HasRows)
                    {
                        while (reader2.Read())
                        {

                            recordfordailyavrginferd.Add(Convert.ToDouble(reader2["wdaOilRateInferred"]));
                            recordfordailyavrgAllocated.Add(Convert.ToDouble(reader2["wdaOilRateAllocated"]));
                        }
                        reader2.Close();
                    }
                }

                double calrate = 5000 / (Convert.ToDouble(recordfordailyavrginferd[0] + recordfordailyavrginferd[1] + recordfordailyavrginferd[2] + recordfordailyavrginferd[3]));

                Assert.AreEqual(Math.Round((double)recordCount, 2), Math.Round((double)calrate, 2), "Calculated PAFieldFactor rate and WellDailyAverage");
                Assert.AreEqual(Math.Round((double)recordfordailyavrgAllocated[0]), Math.Round((Math.Round((double)recordfordailyavrginferd[0], 2)) * Math.Round((double)calrate, 2)), "Calculated PAFieldFactor rate and WellDailyAverage");
                Assert.AreEqual(Math.Round((double)recordfordailyavrgAllocated[1]), Math.Round((Math.Round((double)recordfordailyavrginferd[1], 3)) * Math.Round((double)calrate, 3)), "Calculated PAFieldFactor rate and WellDailyAverage");
                Assert.AreEqual(Math.Round((double)recordfordailyavrgAllocated[2]), Math.Round((Math.Round((double)recordfordailyavrginferd[2], 3)) * Math.Round((double)calrate, 3)), "Calculated PAFieldFactor rate and WellDailyAverage");
                Assert.AreEqual(Math.Round((double)recordfordailyavrgAllocated[3]), Math.Round((Math.Round((double)recordfordailyavrginferd[3], 2)) * Math.Round((double)calrate, 2)), "Calculated PAFieldFactor rate and WellDailyAverage");


                //var dailyAverageRecord;
                WellAllocationService.SaveAllocationRatesInWellDailyAverage(ProductionType.Oil.ToString(), start_date, end_date);
                foreach (WellDTO wellId in _wellsToRemove)
                {
                    var dailyAverageRecord = SurveillanceService.GetDailyAverages(wellId.Id.ToString(), start_date, end_date).Values.FirstOrDefault();
                    totalInferredOilRateUseEstRateasAlloc = totalInferredOilRateUseEstRateasAlloc + dailyAverageRecord.OilRateAllocated;
                }

                Assert.AreEqual(5000, Math.Round((double)totalInferredOilRate), "Expected Rate: 5000  Actual Rate: " + totalInferredOilRate);
                Assert.AreEqual(5000, Math.Round((double)totalInferredOilRateUseEstRateasAlloc), "Expected Rate: 5000  Allocated Rate: " + totalInferredOilRateUseEstRateasAlloc);

            }


            finally
            {
                ////Removing all records related to Allocation group
                WellAllocationService.RemoveAllocationGroupsById(parentAllocationGroupDTO.Id.ToString());
            }


        }


        /// <summary>
        /// FRWM : 6822 : Group Allocation Status 
        /// API Testing subtask : FRWM-6998
        /// </summary>

        [TestCategory(TestCategories.WellAllocationServiceTests), TestMethod]
        public void GetGroupAllocationStatus()
        {

            DateTime day1ago = DateTime.Today.ToUniversalTime().AddDays(-1);
            DateTime day2ago = DateTime.Today.ToUniversalTime().AddDays(-2);
            DateTime day3ago = DateTime.Today.ToUniversalTime().AddDays(-3);
            DateTime day4ago = DateTime.Today.ToUniversalTime().AddDays(-4);
            DateTime day5ago = DateTime.Today.ToUniversalTime().AddDays(-5);
            DateTime end = DateTime.Today.ToUniversalTime(); //make it 23:59:59 for local time to end for end date
            //// The target rate assigned to a well 
            double? dailyAverage = 300;
            double? dailyAverageGL = 400;

            ////Created Asset Asset Test
            //string assetName = "AssetTest";
            //SurfaceNetworkService.AddAsset(new AssetDTO() { Name = assetName, Description = "Test Description" });
            //var allAssets = SurfaceNetworkService.GetAllAssets().ToList();
            //AssetDTO asset = allAssets?.FirstOrDefault(a => a.Name.Equals(assetName));
            //_assetsToRemove.Add(asset);

            //// Wells Creation (5 ESP wells, 5 GL wells)
            #region Add Well Daily Average and Target Rates
            SurveillanceServiceTests survtest = new SurveillanceServiceTests();
            for (int i = 0; i < 5; i++)
            {
                WellDTO espwelltoadd = AddNonRRLWell(GetFacilityId("ESPWELL_", i + 1), WellTypeId.ESP);
                WellDTO glwelltoadd = AddNonRRLWell(GetFacilityId("GLWELL_", i + 1), WellTypeId.GLift);
                _wellsToRemove.Add(espwelltoadd);
                _wellsToRemove.Add(glwelltoadd);
                AddWellDailyAvergeData(espwelltoadd, day1ago, end, dailyAverage, dailyAverage + 100, dailyAverage + 200);
                dailyAverage = dailyAverage + 50;
                AddWellDailyAvergeData(espwelltoadd, day2ago, day1ago, dailyAverage, dailyAverage + 100, dailyAverage + 200);
                dailyAverage = dailyAverage + 50;
                AddWellDailyAvergeData(espwelltoadd, day3ago, day2ago, dailyAverage, dailyAverage + 100, dailyAverage + 200);
                dailyAverage = dailyAverage + 50;
                AddWellDailyAvergeData(espwelltoadd, day4ago, day3ago, dailyAverage, dailyAverage + 100, dailyAverage + 200);
                dailyAverage = dailyAverage + 50;
                AddWellDailyAvergeData(espwelltoadd, day5ago, day4ago, dailyAverage, dailyAverage + 100, dailyAverage + 200);
                dailyAverage = dailyAverage + 50;
                dailyAverage = dailyAverage + 100;
                survtest.AddWellDailyAvergeDataRunTimeLessthan24Hour(glwelltoadd, day1ago, end, dailyAverageGL, dailyAverageGL + 100, dailyAverageGL + 200);
                dailyAverageGL = dailyAverageGL + 50;
                survtest.AddWellDailyAvergeDataRunTimeLessthan24Hour(glwelltoadd, day2ago, day1ago, dailyAverageGL, dailyAverageGL + 100, dailyAverageGL + 200);
                dailyAverageGL = dailyAverageGL + 50;
                survtest.AddWellDailyAvergeDataRunTimeLessthan24Hour(glwelltoadd, day3ago, day2ago, dailyAverageGL, dailyAverageGL + 100, dailyAverageGL + 200);
                dailyAverageGL = dailyAverageGL + 50;
                survtest.AddWellDailyAvergeDataRunTimeLessthan24Hour(glwelltoadd, day4ago, day3ago, dailyAverage, dailyAverageGL + 100, dailyAverageGL + 200);
                dailyAverageGL = dailyAverageGL + 50;
                survtest.AddWellDailyAvergeDataRunTimeLessthan24Hour(glwelltoadd, day5ago, day4ago, dailyAverage, dailyAverageGL + 100, dailyAverageGL + 200);
                dailyAverageGL = dailyAverageGL + 50;
                dailyAverageGL = dailyAverageGL + 100;

                VerifyTrendsForDailyAvgRates(glwelltoadd, day5ago.ToISO8601().ToString(), end.AddDays(1).ToISO8601().ToString(), false);
                VerifyTrendsForDailyAvgRates(espwelltoadd, day5ago.ToISO8601().ToString(), end.AddDays(1).ToISO8601().ToString(), false);

            }
            // Add Taeget rates for wells
            WellDTO[] allwells = WellService.GetAllWells();
            foreach (WellDTO indwell in allwells)
            {
                if (indwell.WellType == WellTypeId.All || indwell.WellType == WellTypeId.PCP || indwell.WellType == WellTypeId.OT)
                {
                    continue;
                }
                WellTargetRateDTO welltargetdto = new WellTargetRateDTO
                {
                    WellId = indwell.Id,
                    OilLowerBound = 10,
                    WaterLowerBound = 10,
                    GasLowerBound = 10,
                    OilTarget = 1000,
                    WaterTarget = 800,
                    GasTarget = 500,
                    OilUpperBound = 3000,
                    GasUpperBound = 3000,
                    WaterUpperBound = 3000,
                    OilMinimum = 0,
                    OilTechnicalLimit = 10000,
                    WaterMinimum = 0,
                    WaterTechnicalLimit = 10000,
                    GasMinimum = 0,
                    GasTechnicalLimit = 10000,
                    StartDate = DateTime.Now.AddDays(-30).ToUniversalTime(),
                    EndDate = DateTime.Now.AddDays(30).ToUniversalTime(),
                };
                WellService.AddWellTargetRate(welltargetdto);
            }

            #endregion

            #region Create Allocation Groups and Add Wells with Allocation percent to groups
            ////Create all Oil Allocation Group
            AllocationGroupDTO parentOilAllocationGroupDTO = new AllocationGroupDTO();

            AllocationGroupDTO childOilAllocationGroupDTO1 = new AllocationGroupDTO();
            AllocationGroupDTO childOilAllocationGroupDTO2 = new AllocationGroupDTO();
            AllocationGroupDTO childOilAllocationGroupDTO3 = new AllocationGroupDTO();
            AllocationGroupDTO childOilAllocationGroupDTO4 = new AllocationGroupDTO();
            AllocationGroupDTO childOilAllocationGroupDTO5 = new AllocationGroupDTO();
            //Create all Water  Allocation Group
            AllocationGroupDTO parentWaterAllocationGroupDTO = new AllocationGroupDTO();

            AllocationGroupDTO childWaterAllocationGroupDTO1 = new AllocationGroupDTO();
            AllocationGroupDTO childWaterAllocationGroupDTO2 = new AllocationGroupDTO();
            AllocationGroupDTO childWaterAllocationGroupDTO3 = new AllocationGroupDTO();
            AllocationGroupDTO childWaterAllocationGroupDTO4 = new AllocationGroupDTO();
            AllocationGroupDTO childWaterAllocationGroupDTO5 = new AllocationGroupDTO();
            //Create all Gas  Allocation Group
            AllocationGroupDTO parentGasAllocationGroupDTO = new AllocationGroupDTO();

            AllocationGroupDTO childGasAllocationGroupDTO1 = new AllocationGroupDTO();
            AllocationGroupDTO childGasAllocationGroupDTO2 = new AllocationGroupDTO();
            AllocationGroupDTO childGasAllocationGroupDTO3 = new AllocationGroupDTO();
            AllocationGroupDTO childGasAllocationGroupDTO4 = new AllocationGroupDTO();
            AllocationGroupDTO childGasAllocationGroupDTO5 = new AllocationGroupDTO();

            List<AllocationGroupDTO> allOilAllocationGroups = new List<AllocationGroupDTO>();
            List<AllocationGroupDTO> allWaterAllocationGroups = new List<AllocationGroupDTO>();
            List<AllocationGroupDTO> allGasAllocationGroups = new List<AllocationGroupDTO>();
            //For Oil Phase : 
            parentOilAllocationGroupDTO = AddParentGroupsInTreeView("G1", ProductionType.Oil);
            childOilAllocationGroupDTO1 = AddChildGroupsInTreeView("G2", ProductionType.Oil, parentOilAllocationGroupDTO, parentOilAllocationGroupDTO.Id);
            childOilAllocationGroupDTO2 = AddChildGroupsInTreeView("G3", ProductionType.Oil, childOilAllocationGroupDTO1, childOilAllocationGroupDTO1.Id);
            childOilAllocationGroupDTO3 = AddChildGroupsInTreeView("G4", ProductionType.Oil, childOilAllocationGroupDTO1, childOilAllocationGroupDTO1.Id);
            childOilAllocationGroupDTO4 = AddChildGroupsInTreeView("G5", ProductionType.Oil, childOilAllocationGroupDTO2, childOilAllocationGroupDTO2.Id);
            childOilAllocationGroupDTO5 = AddChildGroupsInTreeView("G6", ProductionType.Oil, childOilAllocationGroupDTO3, childOilAllocationGroupDTO3.Id);
            allOilAllocationGroups.Add(parentOilAllocationGroupDTO);
            allOilAllocationGroups.Add(childOilAllocationGroupDTO1);
            allOilAllocationGroups.Add(childOilAllocationGroupDTO2);
            allOilAllocationGroups.Add(childOilAllocationGroupDTO2);
            allOilAllocationGroups.Add(childOilAllocationGroupDTO4);
            allOilAllocationGroups.Add(childOilAllocationGroupDTO5);
            DoAllocations(childOilAllocationGroupDTO1, childOilAllocationGroupDTO2, childOilAllocationGroupDTO3, childOilAllocationGroupDTO4, childOilAllocationGroupDTO5);
            //For Water Phase : 
            parentWaterAllocationGroupDTO = AddParentGroupsInTreeView("G1", ProductionType.Water);
            childWaterAllocationGroupDTO1 = AddChildGroupsInTreeView("G2", ProductionType.Water, parentWaterAllocationGroupDTO, parentWaterAllocationGroupDTO.Id);
            childWaterAllocationGroupDTO2 = AddChildGroupsInTreeView("G3", ProductionType.Water, childWaterAllocationGroupDTO1, childWaterAllocationGroupDTO1.Id);
            childWaterAllocationGroupDTO3 = AddChildGroupsInTreeView("G4", ProductionType.Water, childWaterAllocationGroupDTO1, childWaterAllocationGroupDTO1.Id);
            childWaterAllocationGroupDTO4 = AddChildGroupsInTreeView("G5", ProductionType.Water, childWaterAllocationGroupDTO2, childWaterAllocationGroupDTO2.Id);
            childWaterAllocationGroupDTO5 = AddChildGroupsInTreeView("G6", ProductionType.Water, childWaterAllocationGroupDTO3, childWaterAllocationGroupDTO3.Id);
            DoAllocations(childWaterAllocationGroupDTO1, childWaterAllocationGroupDTO2, childWaterAllocationGroupDTO3, childWaterAllocationGroupDTO4, childWaterAllocationGroupDTO5);
            allWaterAllocationGroups.Add(parentWaterAllocationGroupDTO);
            allWaterAllocationGroups.Add(childWaterAllocationGroupDTO1);
            allWaterAllocationGroups.Add(childWaterAllocationGroupDTO2);
            allWaterAllocationGroups.Add(childWaterAllocationGroupDTO3);
            allWaterAllocationGroups.Add(childWaterAllocationGroupDTO4);
            allWaterAllocationGroups.Add(childWaterAllocationGroupDTO5);

            //For Gas Phase : 
            parentGasAllocationGroupDTO = AddParentGroupsInTreeView("G1", ProductionType.Gas);
            childGasAllocationGroupDTO1 = AddChildGroupsInTreeView("G2", ProductionType.Gas, parentGasAllocationGroupDTO, parentGasAllocationGroupDTO.Id);
            childGasAllocationGroupDTO2 = AddChildGroupsInTreeView("G3", ProductionType.Gas, childGasAllocationGroupDTO1, childGasAllocationGroupDTO1.Id);
            childGasAllocationGroupDTO3 = AddChildGroupsInTreeView("G4", ProductionType.Gas, childGasAllocationGroupDTO1, childGasAllocationGroupDTO1.Id);
            childGasAllocationGroupDTO4 = AddChildGroupsInTreeView("G5", ProductionType.Gas, childGasAllocationGroupDTO2, childGasAllocationGroupDTO2.Id);
            childGasAllocationGroupDTO5 = AddChildGroupsInTreeView("G6", ProductionType.Gas, childGasAllocationGroupDTO3, childGasAllocationGroupDTO3.Id);
            allGasAllocationGroups.Add(parentGasAllocationGroupDTO);
            allGasAllocationGroups.Add(childGasAllocationGroupDTO1);
            allGasAllocationGroups.Add(childGasAllocationGroupDTO2);
            allGasAllocationGroups.Add(childGasAllocationGroupDTO3);
            allGasAllocationGroups.Add(childGasAllocationGroupDTO4);
            allGasAllocationGroups.Add(childGasAllocationGroupDTO5);
            DoAllocations(childGasAllocationGroupDTO1, childGasAllocationGroupDTO2, childGasAllocationGroupDTO3, childGasAllocationGroupDTO4, childGasAllocationGroupDTO5);
            WellAllocationService.UpdateAllocationGroupInfo(parentOilAllocationGroupDTO);
            WellAllocationService.UpdateAllocationGroupInfo(parentWaterAllocationGroupDTO);
            WellAllocationService.UpdateAllocationGroupInfo(parentGasAllocationGroupDTO);
            #region Update Physucal and Virtual Groups 
            //Update for Physucal Groups:
            parentOilAllocationGroupDTO.DataConnection = GetDefaultCygNetDataConnection();
            DataConnectionDTO dataConnection = DataConnectionService.GetAllDataConnections().FirstOrDefault(dc => dc.ProductionDomain == s_domain && dc.Site == s_site && dc.Service == s_cvsService);
            parentOilAllocationGroupDTO.DataConnectionId = dataConnection.Id;
            parentOilAllocationGroupDTO.EquipmentType = AllocationGroupEquipmentType.Tank;
            parentOilAllocationGroupDTO.FacilityId = "TANK_BATTERY_1";
            parentOilAllocationGroupDTO.DeviceName = "G1 Tank Oil";
            parentOilAllocationGroupDTO.GroupType = AllocationGroupType.Physical;
            WellAllocationService.UpdateAllocationGroupInfo(parentOilAllocationGroupDTO);
            //Water Group
            parentWaterAllocationGroupDTO.DataConnection = GetDefaultCygNetDataConnection();
            dataConnection = DataConnectionService.GetAllDataConnections().FirstOrDefault(dc => dc.ProductionDomain == s_domain && dc.Site == s_site && dc.Service == s_cvsService);
            parentWaterAllocationGroupDTO.DataConnectionId = dataConnection.Id;
            parentWaterAllocationGroupDTO.EquipmentType = AllocationGroupEquipmentType.Tank;
            parentWaterAllocationGroupDTO.FacilityId = "TANK_BATTERY_1";
            parentWaterAllocationGroupDTO.DeviceName = "G1 Tank Water";
            parentWaterAllocationGroupDTO.GroupType = AllocationGroupType.Physical;
            WellAllocationService.UpdateAllocationGroupInfo(parentWaterAllocationGroupDTO);
            //Gas Gropp;
            parentGasAllocationGroupDTO.DataConnection = GetDefaultCygNetDataConnection();
            dataConnection = DataConnectionService.GetAllDataConnections().FirstOrDefault(dc => dc.ProductionDomain == s_domain && dc.Site == s_site && dc.Service == s_cvsService);
            parentGasAllocationGroupDTO.DataConnectionId = dataConnection.Id;
            parentGasAllocationGroupDTO.EquipmentType = AllocationGroupEquipmentType.Tank;
            parentGasAllocationGroupDTO.FacilityId = "TANK_BATTERY_1";
            parentGasAllocationGroupDTO.DeviceName = "G1 Tank Gas";
            parentGasAllocationGroupDTO.GroupType = AllocationGroupType.Physical;
            WellAllocationService.UpdateAllocationGroupInfo(parentGasAllocationGroupDTO);
            // Other Oil Alocation Groups;
            childOilAllocationGroupDTO1.GroupType = AllocationGroupType.Virtual;
            WellAllocationService.UpdateAllocationGroupInfo(childOilAllocationGroupDTO1);
            childOilAllocationGroupDTO2.GroupType = AllocationGroupType.Virtual;
            WellAllocationService.UpdateAllocationGroupInfo(childOilAllocationGroupDTO2);
            childOilAllocationGroupDTO3.GroupType = AllocationGroupType.Virtual;
            WellAllocationService.UpdateAllocationGroupInfo(childOilAllocationGroupDTO3);
            childOilAllocationGroupDTO4.GroupType = AllocationGroupType.Virtual;
            WellAllocationService.UpdateAllocationGroupInfo(childOilAllocationGroupDTO4);
            childOilAllocationGroupDTO5.GroupType = AllocationGroupType.Virtual;
            WellAllocationService.UpdateAllocationGroupInfo(childOilAllocationGroupDTO5);
            //Other Water Allocation Groups
            childWaterAllocationGroupDTO1.GroupType = AllocationGroupType.Virtual;
            WellAllocationService.UpdateAllocationGroupInfo(childWaterAllocationGroupDTO1);
            childWaterAllocationGroupDTO2.GroupType = AllocationGroupType.Virtual;
            WellAllocationService.UpdateAllocationGroupInfo(childWaterAllocationGroupDTO2);
            childWaterAllocationGroupDTO3.GroupType = AllocationGroupType.Virtual;
            WellAllocationService.UpdateAllocationGroupInfo(childWaterAllocationGroupDTO3);
            childWaterAllocationGroupDTO4.GroupType = AllocationGroupType.Virtual;
            WellAllocationService.UpdateAllocationGroupInfo(childWaterAllocationGroupDTO4);
            childWaterAllocationGroupDTO5.GroupType = AllocationGroupType.Virtual;
            WellAllocationService.UpdateAllocationGroupInfo(childWaterAllocationGroupDTO5);
            //Other Gas Allocation Groups
            childGasAllocationGroupDTO1.GroupType = AllocationGroupType.Virtual;
            WellAllocationService.UpdateAllocationGroupInfo(childGasAllocationGroupDTO1);
            childGasAllocationGroupDTO2.GroupType = AllocationGroupType.Virtual;
            WellAllocationService.UpdateAllocationGroupInfo(childGasAllocationGroupDTO2);
            childGasAllocationGroupDTO3.GroupType = AllocationGroupType.Virtual;
            WellAllocationService.UpdateAllocationGroupInfo(childGasAllocationGroupDTO3);
            childGasAllocationGroupDTO4.GroupType = AllocationGroupType.Virtual;
            WellAllocationService.UpdateAllocationGroupInfo(childGasAllocationGroupDTO4);
            childGasAllocationGroupDTO5.GroupType = AllocationGroupType.Virtual;
            WellAllocationService.UpdateAllocationGroupInfo(childGasAllocationGroupDTO5);
            #endregion



            #endregion

            string start_date = (day5ago.AddDays(-3)).ToUniversalTime().ToISO8601();
            string end_date = end.ToUniversalTime().AddDays(1).ToISO8601();

            ////Use the API to Get All Allocation and FF computed
            ////*********************************************************************

            try
            {
                //var dailyAverageRecord for Oil Save ;
                WellAllocationService.SaveAllocationRatesInWellDailyAverage(ProductionType.Oil.ToString(), day1ago.ToISO8601(), end.ToISO8601());
                WellAllocationService.SaveAllocationRatesInWellDailyAverage(ProductionType.Oil.ToString(), day2ago.ToISO8601(), day1ago.ToISO8601());
                WellAllocationService.SaveAllocationRatesInWellDailyAverage(ProductionType.Oil.ToString(), day3ago.ToISO8601(), day2ago.ToISO8601());
                WellAllocationService.SaveAllocationRatesInWellDailyAverage(ProductionType.Oil.ToString(), day4ago.ToISO8601(), day3ago.ToISO8601());
                WellAllocationService.SaveAllocationRatesInWellDailyAverage(ProductionType.Oil.ToString(), day5ago.ToISO8601(), day4ago.ToISO8601());
                //var dailyAverageRecord for Water Save ;
                WellAllocationService.SaveAllocationRatesInWellDailyAverage(ProductionType.Water.ToString(), day1ago.ToISO8601(), end.ToISO8601());
                WellAllocationService.SaveAllocationRatesInWellDailyAverage(ProductionType.Water.ToString(), day2ago.ToISO8601(), day1ago.ToISO8601());
                WellAllocationService.SaveAllocationRatesInWellDailyAverage(ProductionType.Water.ToString(), day3ago.ToISO8601(), day2ago.ToISO8601());
                WellAllocationService.SaveAllocationRatesInWellDailyAverage(ProductionType.Water.ToString(), day4ago.ToISO8601(), day3ago.ToISO8601());
                WellAllocationService.SaveAllocationRatesInWellDailyAverage(ProductionType.Water.ToString(), day5ago.ToISO8601(), day4ago.ToISO8601());
                //var dailyAverageRecord for Gas Save ;
                WellAllocationService.SaveAllocationRatesInWellDailyAverage(ProductionType.Gas.ToString(), day1ago.ToISO8601(), end.ToISO8601());
                WellAllocationService.SaveAllocationRatesInWellDailyAverage(ProductionType.Gas.ToString(), day2ago.ToISO8601(), day1ago.ToISO8601());
                WellAllocationService.SaveAllocationRatesInWellDailyAverage(ProductionType.Gas.ToString(), day3ago.ToISO8601(), day2ago.ToISO8601());
                WellAllocationService.SaveAllocationRatesInWellDailyAverage(ProductionType.Gas.ToString(), day4ago.ToISO8601(), day3ago.ToISO8601());
                WellAllocationService.SaveAllocationRatesInWellDailyAverage(ProductionType.Gas.ToString(), day5ago.ToISO8601(), day4ago.ToISO8601());


                SystemSettingDTO systemSetting = SettingService.GetSystemSettingByName(SettingServiceStringConstants.ALLOCATION_DEFAULT_ACTION);
                SettingService.SaveSystemSetting(new SystemSettingDTO
                {
                    SettingId = systemSetting.Id,
                    Setting = systemSetting.Setting,
                    StringValue = AllocationDefaultAction.UseEstimatedRateAsAllocatedRate.ToString(),
                });

                List<double> recordfordailyavrginferd = new List<double>();
                List<double> recordfordailyavrgAllocated = new List<double>();

                var OilGroupAllocationstatusArrayAndUnits = WellAllocationService.GetGroupAllocationStatusArrayAndUnits(ProductionType.Oil.ToString(), start_date, end_date);
                Assert.AreEqual(40, OilGroupAllocationstatusArrayAndUnits.Values.Length, "Allocation Group did not return all records");
                //Verify Field Factor Avg and per Well
                //Field Factor is only  G1 which is Physical Group
                //********  Verify for Oil Allocation Group ****************
                #region US Units Verification
                Trace.WriteLine("**********************        Verifying the Oil Allocation Group Status Values [US Units]  ***********************");
                var day1agovalues = OilGroupAllocationstatusArrayAndUnits.Values.Where(x => x.ValueDate == day1ago);
                var day2agovalues = OilGroupAllocationstatusArrayAndUnits.Values.Where(x => x.ValueDate == day2ago);
                var day3agovalues = OilGroupAllocationstatusArrayAndUnits.Values.Where(x => x.ValueDate == day3ago);
                var day4agovalues = OilGroupAllocationstatusArrayAndUnits.Values.Where(x => x.ValueDate == day4ago);
                var day5agovalues = OilGroupAllocationstatusArrayAndUnits.Values.Where(x => x.ValueDate == day5ago);

                VerifyAllocationValuesforDay(OilGroupAllocationstatusArrayAndUnits, day1agovalues, 5000);
                VerifyAllocationValuesforDay(OilGroupAllocationstatusArrayAndUnits, day2agovalues, 5000);
                VerifyAllocationValuesforDay(OilGroupAllocationstatusArrayAndUnits, day3agovalues, 5000);
                VerifyAllocationValuesforDay(OilGroupAllocationstatusArrayAndUnits, day4agovalues, 5000);
                VerifyAllocationValuesforDay(OilGroupAllocationstatusArrayAndUnits, day5agovalues, 5000);

                //Verify for Water Allocation Group
                var WaterGroupAllocationstatusArrayAndUnits = WellAllocationService.GetGroupAllocationStatusArrayAndUnits(ProductionType.Water.ToString(), start_date, end_date);
                Trace.WriteLine("**********************        Verifying the Water Allocation Group Status Values [US Units]  ***********************");
                day1agovalues = WaterGroupAllocationstatusArrayAndUnits.Values.Where(x => x.ValueDate == day1ago);
                day2agovalues = WaterGroupAllocationstatusArrayAndUnits.Values.Where(x => x.ValueDate == day2ago);
                day3agovalues = WaterGroupAllocationstatusArrayAndUnits.Values.Where(x => x.ValueDate == day3ago);
                day4agovalues = WaterGroupAllocationstatusArrayAndUnits.Values.Where(x => x.ValueDate == day4ago);
                day5agovalues = WaterGroupAllocationstatusArrayAndUnits.Values.Where(x => x.ValueDate == day5ago);

                VerifyAllocationValuesforDay(WaterGroupAllocationstatusArrayAndUnits, day1agovalues, 500);
                VerifyAllocationValuesforDay(WaterGroupAllocationstatusArrayAndUnits, day2agovalues, 500);
                VerifyAllocationValuesforDay(WaterGroupAllocationstatusArrayAndUnits, day3agovalues, 500);
                VerifyAllocationValuesforDay(WaterGroupAllocationstatusArrayAndUnits, day4agovalues, 500);
                VerifyAllocationValuesforDay(WaterGroupAllocationstatusArrayAndUnits, day5agovalues, 500);

                //Verify for Gas Allocation Group 
                Trace.WriteLine("**********************        Verifying the Gas Allocation Group Status Values [US Units]  ***********************");
                var GasGroupAllocationstatusArrayAndUnits = WellAllocationService.GetGroupAllocationStatusArrayAndUnits(ProductionType.Gas.ToString(), start_date, end_date);
                day1agovalues = GasGroupAllocationstatusArrayAndUnits.Values.Where(x => x.ValueDate == day1ago);
                day2agovalues = GasGroupAllocationstatusArrayAndUnits.Values.Where(x => x.ValueDate == day2ago);
                day3agovalues = GasGroupAllocationstatusArrayAndUnits.Values.Where(x => x.ValueDate == day3ago);
                day4agovalues = GasGroupAllocationstatusArrayAndUnits.Values.Where(x => x.ValueDate == day4ago);
                day5agovalues = GasGroupAllocationstatusArrayAndUnits.Values.Where(x => x.ValueDate == day5ago);

                VerifyAllocationValuesforDay(GasGroupAllocationstatusArrayAndUnits, day1agovalues, 50);
                VerifyAllocationValuesforDay(GasGroupAllocationstatusArrayAndUnits, day2agovalues, 50);
                VerifyAllocationValuesforDay(GasGroupAllocationstatusArrayAndUnits, day3agovalues, 50);
                VerifyAllocationValuesforDay(GasGroupAllocationstatusArrayAndUnits, day4agovalues, 50);
                VerifyAllocationValuesforDay(GasGroupAllocationstatusArrayAndUnits, day5agovalues, 50);
                #endregion
                ChangeUnitSystem("Metric");
                ChangeUnitSystemUserSetting("Metric");
                #region Metric Units Verification
                OilGroupAllocationstatusArrayAndUnits = WellAllocationService.GetGroupAllocationStatusArrayAndUnits(ProductionType.Oil.ToString(), start_date, end_date);
                Trace.WriteLine("**********************        Verifying the Oil Allocation Group Status Values [Metric Units]  ***********************");
                day1agovalues = OilGroupAllocationstatusArrayAndUnits.Values.Where(x => x.ValueDate == day1ago);
                day2agovalues = OilGroupAllocationstatusArrayAndUnits.Values.Where(x => x.ValueDate == day2ago);
                day3agovalues = OilGroupAllocationstatusArrayAndUnits.Values.Where(x => x.ValueDate == day3ago);
                day4agovalues = OilGroupAllocationstatusArrayAndUnits.Values.Where(x => x.ValueDate == day4ago);
                day5agovalues = OilGroupAllocationstatusArrayAndUnits.Values.Where(x => x.ValueDate == day5ago);

                VerifyAllocationValuesforDay(OilGroupAllocationstatusArrayAndUnits, day1agovalues, (double)UnitsConversion("bbl", 5000));
                VerifyAllocationValuesforDay(OilGroupAllocationstatusArrayAndUnits, day2agovalues, (double)UnitsConversion("bbl", 5000));
                VerifyAllocationValuesforDay(OilGroupAllocationstatusArrayAndUnits, day3agovalues, (double)UnitsConversion("bbl", 5000));
                VerifyAllocationValuesforDay(OilGroupAllocationstatusArrayAndUnits, day4agovalues, (double)UnitsConversion("bbl", 5000));
                VerifyAllocationValuesforDay(OilGroupAllocationstatusArrayAndUnits, day5agovalues, (double)UnitsConversion("bbl", 5000));

                //Verify for Water Allocation Group
                WaterGroupAllocationstatusArrayAndUnits = WellAllocationService.GetGroupAllocationStatusArrayAndUnits(ProductionType.Water.ToString(), start_date, end_date);
                Trace.WriteLine("**********************        Verifying the Water Allocation Group Status Values [Metric Units]  ***********************");
                day1agovalues = WaterGroupAllocationstatusArrayAndUnits.Values.Where(x => x.ValueDate == day1ago);
                day2agovalues = WaterGroupAllocationstatusArrayAndUnits.Values.Where(x => x.ValueDate == day2ago);
                day3agovalues = WaterGroupAllocationstatusArrayAndUnits.Values.Where(x => x.ValueDate == day3ago);
                day4agovalues = WaterGroupAllocationstatusArrayAndUnits.Values.Where(x => x.ValueDate == day4ago);
                day5agovalues = WaterGroupAllocationstatusArrayAndUnits.Values.Where(x => x.ValueDate == day5ago);

                VerifyAllocationValuesforDay(WaterGroupAllocationstatusArrayAndUnits, day1agovalues, (double)UnitsConversion("bbl", 500));
                VerifyAllocationValuesforDay(WaterGroupAllocationstatusArrayAndUnits, day2agovalues, (double)UnitsConversion("bbl", 500));
                VerifyAllocationValuesforDay(WaterGroupAllocationstatusArrayAndUnits, day3agovalues, (double)UnitsConversion("bbl", 500));
                VerifyAllocationValuesforDay(WaterGroupAllocationstatusArrayAndUnits, day4agovalues, (double)UnitsConversion("bbl", 500));
                VerifyAllocationValuesforDay(WaterGroupAllocationstatusArrayAndUnits, day5agovalues, (double)UnitsConversion("bbl", 500));

                //Verify for Gas Allocation Group 
                Trace.WriteLine("**********************        Verifying the Gas Allocation Group Status Values [Metric Units]  ***********************");
                GasGroupAllocationstatusArrayAndUnits = WellAllocationService.GetGroupAllocationStatusArrayAndUnits(ProductionType.Gas.ToString(), start_date, end_date);
                day1agovalues = GasGroupAllocationstatusArrayAndUnits.Values.Where(x => x.ValueDate == day1ago);
                day2agovalues = GasGroupAllocationstatusArrayAndUnits.Values.Where(x => x.ValueDate == day2ago);
                day3agovalues = GasGroupAllocationstatusArrayAndUnits.Values.Where(x => x.ValueDate == day3ago);
                day4agovalues = GasGroupAllocationstatusArrayAndUnits.Values.Where(x => x.ValueDate == day4ago);
                day5agovalues = GasGroupAllocationstatusArrayAndUnits.Values.Where(x => x.ValueDate == day5ago);

                VerifyAllocationValuesforDay(GasGroupAllocationstatusArrayAndUnits, day1agovalues, (double)UnitsConversion("Mcf", 50));
                VerifyAllocationValuesforDay(GasGroupAllocationstatusArrayAndUnits, day2agovalues, (double)UnitsConversion("Mcf", 50));
                VerifyAllocationValuesforDay(GasGroupAllocationstatusArrayAndUnits, day3agovalues, (double)UnitsConversion("Mcf", 50));
                VerifyAllocationValuesforDay(GasGroupAllocationstatusArrayAndUnits, day4agovalues, (double)UnitsConversion("Mcf", 50));
                VerifyAllocationValuesforDay(GasGroupAllocationstatusArrayAndUnits, day5agovalues, (double)UnitsConversion("Mcf", 50));
                #endregion
                ChangeUnitSystem("US");
                ChangeUnitSystemUserSetting("US");
                foreach (WellDTO well in allwells)
                {
                    if (well.Name == GetFacilityId("GLWELL_", 4) || well.Name == GetFacilityId("GLWELL_", 5))
                    {
                        //Wells are not allocated to any group they should be having samve value for Measured and Allocated
                        VerifyTrendsForDailyAvgRates(well, day5ago.ToISO8601().ToString(), end.AddDays(1).ToISO8601().ToString(), false);

                    }
                    else
                    {
                        VerifyTrendsForDailyAvgRates(well, day5ago.ToISO8601().ToString(), end.AddDays(1).ToISO8601().ToString(), true);
                        Trace.WriteLine("Verifying the Well Status Production KPI for ACtual and Infered Production post back Allocation");
                        // FRWM-7312 : API testing task : 7372 
                        // When Back Allocation is run Make sure the Production  KPI Allocated Volumes are not scaled twice.
                        OilGroupAllocationstatusArrayAndUnits = WellAllocationService.GetGroupAllocationStatusArrayAndUnits(ProductionType.Oil.ToString(), start_date, end_date);
                        WaterGroupAllocationstatusArrayAndUnits = WellAllocationService.GetGroupAllocationStatusArrayAndUnits(ProductionType.Water.ToString(), start_date, end_date);
                        GasGroupAllocationstatusArrayAndUnits = WellAllocationService.GetGroupAllocationStatusArrayAndUnits(ProductionType.Gas.ToString(), start_date, end_date);
                        var allocatedwaterratesforwell = WaterGroupAllocationstatusArrayAndUnits.Values.Where(x => x.ValueDate == day1ago && x.WellId == well.Id);
                        double waterallocatevolume = (double)allocatedwaterratesforwell.First().WellAllocatedRate;
                        var allocatedoilratesforwell = OilGroupAllocationstatusArrayAndUnits.Values.Where(x => x.ValueDate == day1ago && x.WellId == well.Id);
                        double oiallocatevolume = (double)allocatedoilratesforwell.First().WellAllocatedRate;
                        var allocatedgasratesforwell = GasGroupAllocationstatusArrayAndUnits.Values.Where(x => x.ValueDate == day1ago && x.WellId == well.Id);
                        double gasallocatevolume = (double)allocatedgasratesforwell.First().WellAllocatedRate;
                        VerifyProductionKPIActualRateInferedRatePostBackAllocation(well, oiallocatevolume + waterallocatevolume, end.ToISO8601().ToString(), "US");
                        // Get Rates for 2 days before
                        allocatedwaterratesforwell = WaterGroupAllocationstatusArrayAndUnits.Values.Where(x => x.ValueDate == day2ago && x.WellId == well.Id);
                        waterallocatevolume = (double)allocatedwaterratesforwell.First().WellAllocatedRate;
                        allocatedoilratesforwell = OilGroupAllocationstatusArrayAndUnits.Values.Where(x => x.ValueDate == day2ago && x.WellId == well.Id);
                        oiallocatevolume = (double)allocatedoilratesforwell.First().WellAllocatedRate;
                        allocatedgasratesforwell = GasGroupAllocationstatusArrayAndUnits.Values.Where(x => x.ValueDate == day2ago && x.WellId == well.Id);
                        gasallocatevolume = (double)allocatedgasratesforwell.First().WellAllocatedRate;
                        VerifyProductionKPIActualRateInferedRatePostBackAllocation(well, oiallocatevolume + waterallocatevolume, day1ago.ToISO8601().ToString(), "US");
                    }
                }
                ChangeUnitSystem("Metric");
                ChangeUnitSystemUserSetting("Metric");
                foreach (WellDTO well in allwells)
                {
                    if (well.Name == GetFacilityId("GLWELL_", 4) || well.Name == GetFacilityId("GLWELL_", 5))
                    {
                        //Wells are not allocated to any group they should be having samve value for Measured and Allocated
                        VerifyTrendsForDailyAvgRates(well, day5ago.ToISO8601().ToString(), end.AddDays(1).ToISO8601().ToString(), false);

                    }
                    else
                    {
                        VerifyTrendsForDailyAvgRates(well, day5ago.ToISO8601().ToString(), end.AddDays(1).ToISO8601().ToString(), true);
                        Trace.WriteLine("Verifying the Well Status Production KPI for ACtual and Infered Production post back Allocation");
                        // FRWM-7312 : API testing task : 7372 
                        // When Back Allocation is run Make sure the Production  KPI Allocated Volumes are not scaled twice.
                        OilGroupAllocationstatusArrayAndUnits = WellAllocationService.GetGroupAllocationStatusArrayAndUnits(ProductionType.Oil.ToString(), start_date, end_date);
                        WaterGroupAllocationstatusArrayAndUnits = WellAllocationService.GetGroupAllocationStatusArrayAndUnits(ProductionType.Water.ToString(), start_date, end_date);
                        GasGroupAllocationstatusArrayAndUnits = WellAllocationService.GetGroupAllocationStatusArrayAndUnits(ProductionType.Gas.ToString(), start_date, end_date);
                        var allocatedwaterratesforwell = WaterGroupAllocationstatusArrayAndUnits.Values.Where(x => x.ValueDate == day1ago && x.WellId == well.Id);
                        double waterallocatevolume = (double)allocatedwaterratesforwell.First().WellAllocatedRate;
                        var allocatedoilratesforwell = OilGroupAllocationstatusArrayAndUnits.Values.Where(x => x.ValueDate == day1ago && x.WellId == well.Id);
                        double oiallocatevolume = (double)allocatedoilratesforwell.First().WellAllocatedRate;
                        var allocatedgasratesforwell = GasGroupAllocationstatusArrayAndUnits.Values.Where(x => x.ValueDate == day1ago && x.WellId == well.Id);
                        double gasallocatevolume = (double)allocatedgasratesforwell.First().WellAllocatedRate;
                        VerifyProductionKPIActualRateInferedRatePostBackAllocation(well, oiallocatevolume + waterallocatevolume, end.ToISO8601().ToString(), "Metric");
                        // Get Rates for 2 days before
                        allocatedwaterratesforwell = WaterGroupAllocationstatusArrayAndUnits.Values.Where(x => x.ValueDate == day2ago && x.WellId == well.Id);
                        waterallocatevolume = (double)allocatedwaterratesforwell.First().WellAllocatedRate;
                        allocatedoilratesforwell = OilGroupAllocationstatusArrayAndUnits.Values.Where(x => x.ValueDate == day2ago && x.WellId == well.Id);
                        oiallocatevolume = (double)allocatedoilratesforwell.First().WellAllocatedRate;
                        allocatedgasratesforwell = GasGroupAllocationstatusArrayAndUnits.Values.Where(x => x.ValueDate == day2ago && x.WellId == well.Id);
                        gasallocatevolume = (double)allocatedgasratesforwell.First().WellAllocatedRate;
                        VerifyProductionKPIActualRateInferedRatePostBackAllocation(well, oiallocatevolume + waterallocatevolume, day1ago.ToISO8601().ToString(), "Metric");
                        
                    }

                    SurveillanceServiceTests sur = new SurveillanceServiceTests();
                    ChangeUnitSystem("US");
                    ChangeUnitSystemUserSetting("US");
                    sur.VerifyPEdashboardWithWellStatusProductionKPI(end, "US", 0.1);
                    ChangeUnitSystem("Metric");
                    ChangeUnitSystemUserSetting("Metric");
                    sur.VerifyPEdashboardWithWellStatusProductionKPI(end, "Metric", 0.01);
                }

                // Verify that now Once back Allocation was run rates for each Phase is different

            }


            finally
            {
                ////Removing all records related to Allocation group
                WellAllocationService.RemoveAllocationGroupsById(parentOilAllocationGroupDTO.Id.ToString());
                WellAllocationService.RemoveAllocationGroupsById(parentWaterAllocationGroupDTO.Id.ToString());
                WellAllocationService.RemoveAllocationGroupsById(parentGasAllocationGroupDTO.Id.ToString());
                ChangeUnitSystem("US");
                ChangeUnitSystemUserSetting("US");
            }
        }


        public void VerifyTrendsForDailyAvgRates(WellDTO well, string startdate, string enddate, bool backallocation)
        {
            // FRWM-7027 : API testing subtask FRWM-7244
            //Verify that DailyAverage Rate gets saved in OilRateMesaured WaterRateMeasured and GasRateMeasured Table
            string[] dailyavgquantitieslist = new string[]
            {
                    ((int)DailyAverageQuantity.OilMeasuredRate).ToString(),
                    ((int)DailyAverageQuantity.WaterMeasuredRate).ToString(),
                    ((int)DailyAverageQuantity.GasMeasuredRate).ToString(),
                    ((int)DailyAverageQuantity.OilRateAllocated).ToString(),
                    ((int)DailyAverageQuantity.WaterRateAllocated).ToString(),
                    ((int)DailyAverageQuantity.GasRateAllocated).ToString(),

            };
            CygNetTrendDTO[] dailavgratesdto = SurveillanceService.GetDailyAverageTrendsByDateRange(dailyavgquantitieslist, well.Id.ToString(), startdate, enddate);
            foreach (CygNetTrendDTO indudc in dailavgratesdto)
            {
                Assert.AreEqual("Success", indudc.ErrorMessage, "For Daily Average Well Trend ESP wells  " + indudc.PointUDC + "Data was Not Success");
            }
            var oilmeasuredtenddto = dailavgratesdto.FirstOrDefault(x => x.PointUDC == "OilMeasuredRate").PointValues;
            var oilactualtenddto = dailavgratesdto.FirstOrDefault(x => x.PointUDC == "OilRateAllocated").PointValues;
            var watermeasuredtenddto = dailavgratesdto.FirstOrDefault(x => x.PointUDC == "WaterMeasuredRate").PointValues;
            var wateractualtenddto = dailavgratesdto.FirstOrDefault(x => x.PointUDC == "WaterRateAllocated").PointValues;
            var gasdmeasuredtenddto = dailavgratesdto.FirstOrDefault(x => x.PointUDC == "GasMeasuredRate").PointValues;
            var gasactualtenddto = dailavgratesdto.FirstOrDefault(x => x.PointUDC == "GasRateAllocated").PointValues;

            if (backallocation == false)
            {
                //when No Back Allocation is Run , both Measured and Allocated quantities should  be same.
                Assert.IsTrue(CompareTrendsDTO(oilmeasuredtenddto, oilactualtenddto), "Oil Measured Rates and Allocation rates are not equal on running Daily Average");
                Assert.IsTrue(CompareTrendsDTO(watermeasuredtenddto, wateractualtenddto), " Water Measured Rates and Allocation rates are not equal on running Daily Average");
                Assert.IsTrue(CompareTrendsDTO(gasdmeasuredtenddto, gasactualtenddto), "Gas Measured Rates and Allocation rates are not equal on running Daily Average");
            }
            else if (backallocation == true)
            {
                Assert.IsFalse(CompareTrendsDTO(oilmeasuredtenddto, oilactualtenddto), "Oil Measured Rates and Allocation rates are same on running Back Allocation");
                Assert.IsFalse(CompareTrendsDTO(watermeasuredtenddto, wateractualtenddto), " Water Measured Rates and Allocation rates are same on running Back Allocatione");
                Assert.IsFalse(CompareTrendsDTO(gasdmeasuredtenddto, gasactualtenddto), "Gas Measured Rates and Allocation rates are same on running Back Allocation");
            }

            //Also Verify that Performance Table shows measured volume even after back allocation  FRWM-7372 changes
            var prodkpiato = SurveillanceService.GetWellDailyAverageAndTest(well.Id.ToString(), DateTime.Today.ToUniversalTime().ToISO8601().ToString());
            double actualoilrate = (double)prodkpiato.DailyAverage.Value.OilMeasuredRate;
            double actualwaterate = (double)prodkpiato.DailyAverage.Value.WaterMeasuredRate;
            double actualgasrate = (double)prodkpiato.DailyAverage.Value.GasMeasuredRate;
            Assert.AreEqual((double)oilmeasuredtenddto.FirstOrDefault(x => x.Timestamp == DateTime.Today.AddDays(-1).ToUniversalTime()).Value, actualoilrate, 0.01, "Mismatch in Oil Actual Rate for Well Status Performance");
            Assert.AreEqual((double)watermeasuredtenddto.FirstOrDefault(x => x.Timestamp == DateTime.Today.AddDays(-1).ToUniversalTime()).Value, actualwaterate, 0.01, "Mismatch in Water Actual Rate for Well Status Performance");
            Assert.AreEqual((double)gasdmeasuredtenddto.FirstOrDefault(x => x.Timestamp == DateTime.Today.AddDays(-1).ToUniversalTime()).Value, actualgasrate, 0.01, "Mismatch in Gas Actual Rate for Well Status Performance");
        }

        public void VerifyProductionKPIActualRateInferedRatePostBackAllocation(WellDTO well, double expactualliqvolume, string requesttime, string unitsystem)
        {
            var prodkpiato = SurveillanceService.GetProductionKPI(well.Id.ToString(), requesttime);
            //Assume Run Time to be 14  hrs as  per Script data for all wells with run time  les than 24 hrs
            Assert.IsNotNull(prodkpiato.Units.Liquid, "Liquid Units are NULL");
            Assert.IsNotNull(prodkpiato.Units.Gas, "Gas Units are NULL");
            Assert.IsNotNull(prodkpiato.Units.Oil, "Oil Units are NULL");
            Assert.IsNotNull(prodkpiato.Units.Water, "Water Units are NULL");
            if (unitsystem.ToLower() == "us")
            {
                Assert.AreEqual(prodkpiato.Units.Liquid, "STB", $"Mismatch in Unit text for Liquid for {unitsystem}");
                Assert.AreEqual(prodkpiato.Units.Gas, "Mscf", $"Mismatch in Unit text for Gas for {unitsystem}");
                Assert.AreEqual(prodkpiato.Units.Oil, "STB", $"Mismatch in Unit text for Oil for {unitsystem}");
                Assert.AreEqual(prodkpiato.Units.Water, "STB", $"Mismatch in Unit text for Water for {unitsystem}");
            }
            else if (unitsystem.ToLower() == "metric")
            {
                Assert.AreEqual(prodkpiato.Units.Liquid, "sm3", $"Mismatch in Unit text for Liquid for {unitsystem}");
                Assert.AreEqual(prodkpiato.Units.Gas, "sm3", $"Mismatch in Unit text for Gas for {unitsystem}");
                Assert.AreEqual(prodkpiato.Units.Oil, "sm3", $"Mismatch in Unit text for Oil for {unitsystem}");
                Assert.AreEqual(prodkpiato.Units.Water, "sm3", $"Mismatch in Unit text for Water for {unitsystem}");
            }
            Assert.AreEqual(expactualliqvolume, (double)prodkpiato.CurrentAllocatedProduction.Liquid, 0.1, $"Mismatch in Liquid Value for  {unitsystem}");
        }


        public bool CompareTrendsDTO(GeneralTrendPointDTO[] exp, GeneralTrendPointDTO[] act)
        {
            bool res = false;
            Assert.AreEqual(exp.Length, act.Length, "Trend DTO Length mismatch");
            var cnt = 0;
            foreach (GeneralTrendPointDTO cdto in exp)
            {
                if (cdto.Value != act[cnt].Value)
                {
                    break;
                }
                cnt++;
            }
            if (cnt == exp.Length)
            {
                res = true;
            }
            return res;
        }


        public void DoAllocations(AllocationGroupDTO childAllocationGroupDTO1, AllocationGroupDTO childAllocationGroupDTO2, AllocationGroupDTO childAllocationGroupDTO3, AllocationGroupDTO childAllocationGroupDTO4, AllocationGroupDTO childAllocationGroupDTO5)
        {
            // G2 ==> ESPWELL_00001 , GLWELL_00001, GLWELL_00003
            AssignWelltoWellAllocationGroup(GetFacilityId("ESPWELL_", 1), childAllocationGroupDTO1, 100);
            AssignWelltoWellAllocationGroup(GetFacilityId("GLWELL_", 1), childAllocationGroupDTO1, 100);
            AssignWelltoWellAllocationGroup(GetFacilityId("GLWELL_", 3), childAllocationGroupDTO1, 100);
            // G3 ==> ESPWELL_00002 , GLWELL_00002, 
            AssignWelltoWellAllocationGroup(GetFacilityId("ESPWELL_", 2), childAllocationGroupDTO2, 100);
            AssignWelltoWellAllocationGroup(GetFacilityId("GLWELL_", 2), childAllocationGroupDTO2, 100);
            // G5 ==> ESPWELL_00004
            AssignWelltoWellAllocationGroup(GetFacilityId("ESPWELL_", 4), childAllocationGroupDTO4, 100);
            // G4 ==> ESPWELL_00003
            AssignWelltoWellAllocationGroup(GetFacilityId("ESPWELL_", 3), childAllocationGroupDTO3, 100);
            // G6 ==> ESPWELL_00005
            AssignWelltoWellAllocationGroup(GetFacilityId("ESPWELL_", 5), childAllocationGroupDTO5, 100);
        }

        public void AddWellDailyAvergeData(WellDTO well, DateTime start, DateTime end, double? oilallocated, double? wateralloated, double? gasallocated)
        {
            WellDailyAverageValueDTO dailyAverageDTO = new WellDailyAverageValueDTO
            {
                EndDateTime = end,
                StartDateTime = start,
                GasRateAllocated = gasallocated,
                GasRateInferred = gasallocated + 10,
                OilRateAllocated = oilallocated,
                OilRateInferred = oilallocated + 10,
                WaterRateAllocated = wateralloated,
                WaterRateInferred = wateralloated + 10,
                Status = WellDailyAverageDataStatus.Calculated,
                Duration = 24,
                GasJectionDepth = 1000,
                ChokeDiameter = 64.0,
                RunTime = 24,
                THP = 492,
                THT = 213,
                GasInjectionRate = 5280,
                WellId = well.Id,
                DHPG = 300,
                PDP = 2000,
                PIP = 100,
                MotorAmps = 34,
                FLP = 678,
            };
            // FRWM-7027 : API testing subtask FRWM-7244
            dailyAverageDTO.OilMeasuredRate = oilallocated;
            dailyAverageDTO.WaterMeasuredRate = wateralloated;
            dailyAverageDTO.GasMeasuredRate = gasallocated;

            SurveillanceService.AddUpdateWellDailyAverageData(dailyAverageDTO);
        }

        public void AssignWelltoWellAllocationGroup(string wellname, AllocationGroupDTO wellalcdto, decimal percent)
        {
            List<WellAllocationDTO> listWellAllocation = new List<WellAllocationDTO>();
            List<long> wellIds = new List<long>();
            AssignWellOutputDTO assignWellOutputDTO = null;
            WellAllocationDTO wellAllocationDTO = new WellAllocationDTO
            {
                WellId = _wellsToRemove.FirstOrDefault(x => x.Name == wellname).Id,
                Well = _wellsToRemove.FirstOrDefault(x => x.Name == wellname),
                AllocationGroup = wellalcdto,
                AllocationGroupId = wellalcdto.Id,
                AllocationPercent = percent
            };
            listWellAllocation.Clear();
            wellIds.Clear();
            listWellAllocation.Add(wellAllocationDTO);
            wellIds.Add(_wellsToRemove.FirstOrDefault(x => x.Name == wellname).Id);
            assignWellOutputDTO = null;
            // Assign List of WellAllocationDTO and WellIds to AssignWellOutputDTO
            assignWellOutputDTO = new AssignWellOutputDTO
            {
                WellAllocations = listWellAllocation,
                WellIds = wellIds,
            };
            WellAllocationService.AllocateWellsToGroups(assignWellOutputDTO);
        }

        public double GetWellAllocationRateAvg(GroupAllocationStatusArrayAndUnitsDTO valuesdto, string wellname)
        {
            double avgallocationrate = 0.0;
            var avg = valuesdto.Values.Where(y => y.WellName == wellname).Select(x => x.WellAllocatedRate).Sum();
            var reccount = valuesdto.Values.Where(y => y.WellName == wellname).Count();
            avgallocationrate = (double)avg / reccount;
            return avgallocationrate;
        }

        public double GetWellInferedRateAvg(GroupAllocationStatusArrayAndUnitsDTO valuesdto, string wellname)
        {
            double avgallocationrate = 0.0;
            var avg = valuesdto.Values.Where(y => y.WellName == wellname).Select(x => x.WellInferredRate).Sum();
            var reccount = valuesdto.Values.Where(y => y.WellName == wellname).Count();
            avgallocationrate = (double)avg / reccount;
            return avgallocationrate;
        }

        public double GetGroupFieldFactorAvg(GroupAllocationStatusArrayAndUnitsDTO valuesdto, string groupname)
        {
            double avgallocationrate = 0.0;
            var avg = valuesdto.Values.Where(y => y.AllocationGroupName1 == groupname).Select(x => x.FieldFactorValue).Sum();
            var reccount = valuesdto.Values.Where(y => y.AllocationGroupName1 == groupname).Count();
            avgallocationrate = (double)avg / reccount;
            return avgallocationrate;
        }

        public void VerifyAllocationValuesforDay(GroupAllocationStatusArrayAndUnitsDTO valuesdto, IEnumerable<GroupAllocationStatusValueDTO> perdaydto, double mva)
        {
            double? totalinferedrate = 0.0;
            foreach (var infrate in perdaydto)
            {
                //Asume 100% Allocation and 24 hr Run  Time and only Wells Allocated to that Parent Physical Group G1 in this case
                //if (infrate.WellName.Contains("GL"))
                //{
                //    // Run Time was less than 24 hours ; as par test scope it is 14 hours Scale by Run time 
                //    double runtime = 14;
                //    totalinferedrate = totalinferedrate + infrate.WellInferredRate * (runtime / 24);
                //}
                //else
                //{
                    totalinferedrate = totalinferedrate + infrate.WellInferredRate;
                //}
            }
            //for the Corresponding test setup MVA = 5000 bbl for Oil
            double? fieldfactor = mva / totalinferedrate;

            foreach (var infrate in perdaydto)
            {
                Trace.WriteLine($"Phase  ===>Well  Name:  {infrate.WellName}  Allocated Rate : {infrate.WellAllocatedRate}   Infered Rate {infrate.WellInferredRate}" +
                    $" Target Rate {infrate.WellTarget }   Target Deviation : {infrate.WellTargetDeviation}   Tech Limit deviation { infrate.TechnicalLimitDeviation} ");

                Assert.AreEqual((double)fieldfactor, (double)infrate.FieldFactorValue, 0.0001, " Field Factor Mismatch");
                Assert.AreEqual((double)(infrate.WellAverageAllocatedRate - infrate.WellTarget), (double)infrate.WellTargetDeviation, 0.0001, "Well Target  Deviation  Mismatch");
                Assert.AreEqual((double)(infrate.WellAverageAllocatedRate - infrate.TechnicalLimit), (double)infrate.TechnicalLimitDeviation, "Well Tech Deviation  rate Mismatch");
                Assert.AreEqual(GetWellAllocationRateAvg(valuesdto, infrate.WellName), (double)infrate.WellAverageAllocatedRate, 0.0001, $"Well Average Rate Mismacth for Well {infrate.WellName}");
                Assert.AreEqual(GetWellInferedRateAvg(valuesdto, infrate.WellName), (double)infrate.WellAverageInferredRate, 0.0001, $"Well Infered  Rate Mismacth for Well {infrate.WellName}");
                Assert.AreEqual(GetGroupFieldFactorAvg(valuesdto, "G1"), (double)infrate.FieldFactorAverage, 0.0001, $"Well Field Factor Rate Mismacth for Group G1");
            }
        }

        #region VRR Tests
        [TestCategory(TestCategories.WellAllocationServiceTests), TestMethod]
        public void AddGetVRRTreeViewTest()
        {
            TimeSpan tsp = new TimeSpan(7);
            DateTime? applicableDate = DateTime.Today; 
            DateTime startDate = DateTime.Today.ToLocalTime();
            DateTime? endDate = null;
            DateTime? applicableDateInUTC = DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Utc);

            AssetDTO asset = null;
            try
            {
                //Add all entities like asset, reservoirs, zones, patterns and wells.
                #region  Add Asset
                string assetName = "TestAsset";
                SurfaceNetworkService.AddAsset(new AssetDTO() { Name = assetName, Description = "Test Description" });
                var allAssets = SurfaceNetworkService.GetAllAssets().ToList();
                asset = allAssets?.FirstOrDefault(a => a.Name.Equals(assetName));
                Assert.IsNotNull(asset);
                _assetsToRemove.Add(asset);
                #endregion

                #region Wells Creation (2 NF wells, 2 ESP wells)
                WellConfigDTO well_Config;
                //NF Well creation
                WellDTO NFWell1 = SetDefaultFluidType(new WellDTO() { Name = "NFWell1", AssetId = asset.Id, FacilityId = GetFacilityId("NFWELL_", 1), DataConnection = GetDefaultCygNetDataConnection(), WellType = WellTypeId.ESP, IntervalAPI = "NFWell1IntervalAPI", SubAssemblyAPI = "NFWell1SubAssemblyAPI2", AssemblyAPI = "NFWell1AssemblyAPI3", CommissionDate = DateTime.Today.AddYears(-2) });
                well_Config = WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = NFWell1 });
                NFWell1 = WellService.GetWellByName("NFWell1");
                Trace.WriteLine("ESPWell1 created successfully");
                _wellsToRemove.Add(well_Config.Well);

                //ESP Well creation
                WellDTO ESPWell1 = SetDefaultFluidType(new WellDTO() { Name = "ESPWell1", AssetId = asset.Id, FacilityId = GetFacilityId("ESPWELL_", 1), DataConnection = GetDefaultCygNetDataConnection(), WellType = WellTypeId.ESP, IntervalAPI = "ESPWell1IntervalAPI", SubAssemblyAPI = "ESPWell1SubAssemblyAPI2", AssemblyAPI = "ESPWell1AssemblyAPI3", CommissionDate = DateTime.Today.AddYears(-2) });
                well_Config = WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = ESPWell1 });
                ESPWell1 = WellService.GetWellByName("ESPWell1");
                Trace.WriteLine("ESPWell1 created successfully");
                _wellsToRemove.Add(well_Config.Well);
                #endregion

                #region Add Reservoirs
                ReservoirDTO res1 = new ReservoirDTO { Name = "Reservoir1", AssetId = asset.Id, Description = "First Test Reservoir", ApplicableDate = applicableDate, StartDate = startDate, EndDate = endDate, InitialGasInjectionVolume = 0, InitialGasProductionVolume = 100, InitialOilProductionVolume = 1000, InitialWaterInjectionVolume = 200, InitialWaterProductionVolume = 300 };
                ReservoirAndUnitsDTO reservoir1 = WellAllocationService.GetReservoirById("0");
                reservoir1.Value = res1;
                WellAllocationService.AddOrUpdateReservoir(reservoir1);
                Trace.WriteLine("Reservoir1 created successfully");
                Assert.IsNotNull(reservoir1);
                //_reservoirsToRemove.Add(reservoir1);

                ReservoirDTO res2 = new ReservoirDTO { Name = "Reservoir2", AssetId = asset.Id, Description = "Second Test Reservoir", ApplicableDate = applicableDate, StartDate = startDate, EndDate = endDate, InitialGasInjectionVolume = 0, InitialGasProductionVolume = 200, InitialOilProductionVolume = 2000, InitialWaterInjectionVolume = 400, InitialWaterProductionVolume = 500 };
                ReservoirAndUnitsDTO reservoir2 = WellAllocationService.GetReservoirById("0");
                reservoir2.Value = res2;
                WellAllocationService.AddOrUpdateReservoir(reservoir2);
                Trace.WriteLine("Reservoir2 created successfully");
                Assert.IsNotNull(reservoir2);
                //_reservoirsToRemove.Add(reservoir2);

                //GetReservoir IDs
                ReservoirArrayAndUnitsDTO reservoirsInThisAsset = WellAllocationService.GetReservoirsByAssetId(asset.Id.ToString());
                long res1Id = 0, res2Id = 0;
                if (reservoirsInThisAsset != null)
                {
                    reservoir1.Value.Id = res1Id = reservoirsInThisAsset.Values[0].Id;
                    reservoir2.Value.Id = res2Id = reservoirsInThisAsset.Values[1].Id;
                    _reservoirsToRemove.Add(reservoir1);
                    _reservoirsToRemove.Add(reservoir2);
                }
                #endregion

                #region Add Zones : Reservoir 1 has 3 Zones, Reservoir 2 has 2 zones

                ZoneDTO zon1 = new ZoneDTO { Name = "Res1Zone1", ReservoirId = res1Id, ApplicableDate = applicableDate, StartDate = startDate, EndDate = endDate, InitialGasInjectionVolume = 0, InitialOilProductionVolume = 100, InitialGasProductionVolume = 110, InitialWaterInjectionVolume = 0, InitialWaterProductionVolume = 11 };
                ZoneAndUnitsDTO zone1 = WellAllocationService.GetZoneById("0");
                zone1.Value = zon1;
                WellAllocationService.AddOrUpdateZone(zone1);
                Trace.WriteLine("Zone1 created successfully");

                ZoneDTO zon2 = new ZoneDTO { Name = "Res1Zone2", ReservoirId = res1Id, ApplicableDate = applicableDate, StartDate = startDate, EndDate = endDate, InitialGasInjectionVolume = 0, InitialOilProductionVolume = 200, InitialGasProductionVolume = 220, InitialWaterInjectionVolume = 0, InitialWaterProductionVolume = 22 };
                ZoneAndUnitsDTO zone2 = WellAllocationService.GetZoneById("0");
                zone2.Value = zon2;
                WellAllocationService.AddOrUpdateZone(zone2);
                Trace.WriteLine("Zone2 created successfully");

                ZoneDTO zon3 = new ZoneDTO { Name = "Res1Zone3", ReservoirId = res1Id, ApplicableDate = applicableDate, StartDate = startDate, EndDate = endDate, InitialGasInjectionVolume = 0, InitialOilProductionVolume = 300, InitialGasProductionVolume = 330, InitialWaterInjectionVolume = 0, InitialWaterProductionVolume = 33 };
                ZoneAndUnitsDTO zone3 = WellAllocationService.GetZoneById("0");
                zone3.Value = zon3;
                WellAllocationService.AddOrUpdateZone(zone3);
                Trace.WriteLine("Zone3 created successfully");

                ZoneDTO zon4 = new ZoneDTO { Name = "Res2Zone4", ReservoirId = res2Id, ApplicableDate = applicableDate, StartDate = startDate, EndDate = endDate, InitialGasInjectionVolume = 0, InitialOilProductionVolume = 400, InitialGasProductionVolume = 440, InitialWaterInjectionVolume = 0, InitialWaterProductionVolume = 44 };
                ZoneAndUnitsDTO zone4 = WellAllocationService.GetZoneById("0");
                zone4.Value = zon4;
                WellAllocationService.AddOrUpdateZone(zone4);
                Trace.WriteLine("Zone4 created successfully");

                ZoneDTO zon5 = new ZoneDTO { Name = "Res2Zone5", ReservoirId = res2Id, ApplicableDate = applicableDate, StartDate = startDate, EndDate = endDate, InitialGasInjectionVolume = 0, InitialOilProductionVolume = 500, InitialGasProductionVolume = 550, InitialWaterInjectionVolume = 0, InitialWaterProductionVolume = 55 };
                ZoneAndUnitsDTO zone5 = WellAllocationService.GetZoneById("0");
                zone5.Value = zon5;
                WellAllocationService.AddOrUpdateZone(zone5);
                Trace.WriteLine("Zone5 created successfully");
                #endregion

                // Get zone IDs based on reservoir
                long zone1Id = 0, zone2Id = 0, zone3Id = 0, zone4Id = 0, zone5Id = 0;
                ZoneArrayAndUnitsDTO zonesOfReservoir1 = new ZoneArrayAndUnitsDTO();
                zonesOfReservoir1 = WellAllocationService.GetZonesByReservoirId(res1Id.ToString());
                if (zonesOfReservoir1.Values != null && zonesOfReservoir1.Values.Length > 0)
                {
                    zone1Id = zonesOfReservoir1.Values[0].Id;
                    zone2Id = zonesOfReservoir1.Values[1].Id;
                    zone3Id = zonesOfReservoir1.Values[2].Id;
                }

                ZoneArrayAndUnitsDTO zonesOfReservoir2 = new ZoneArrayAndUnitsDTO();
                zonesOfReservoir2 = WellAllocationService.GetZonesByReservoirId(res2Id.ToString());
                if (zonesOfReservoir2.Values != null && zonesOfReservoir2.Values.Length > 0)
                {
                    zone4Id = zonesOfReservoir2.Values[0].Id;
                    zone5Id = zonesOfReservoir2.Values[1].Id;
                }

                #region Add pattern ... Zone1 has 2 pattern and zone 2, 3 & 4 has one pattern each ...no pattern for zone 5

                PatternDTO comp1 = new PatternDTO { Name = "Zone1Pattern1", ZoneId = zone1Id, StartDate = startDate, EndDate = endDate };
                WellAllocationService.AddOrUpdatePattern(comp1);
                Trace.WriteLine("Pattern1 Zone1Pattern1 created successfully");
                PatternDTO comp2 = new PatternDTO { Name = "Zone1Pattern2", ZoneId = zone1Id, StartDate = startDate, EndDate = endDate };
                WellAllocationService.AddOrUpdatePattern(comp2);
                Trace.WriteLine("Pattern2 Zone1Pattern2 created successfully");
                PatternDTO comp3 = new PatternDTO { Name = "ZOne2Pattern1", ZoneId = zone2Id, StartDate = startDate, EndDate = endDate };
                WellAllocationService.AddOrUpdatePattern(comp3);
                Trace.WriteLine("Pattern3 ZOne2Pattern1 created successfully");
                PatternDTO comp4 = new PatternDTO { Name = "Zone3Pattern1", ZoneId = zone3Id, StartDate = startDate, EndDate = endDate };
                WellAllocationService.AddOrUpdatePattern(comp4);
                Trace.WriteLine("Pattern4 Zone3Pattern1 created successfully");
                PatternDTO comp5 = new PatternDTO { Name = "Zone4Pattern1", ZoneId = zone4Id, StartDate = startDate, EndDate = endDate };
                WellAllocationService.AddOrUpdatePattern(comp5);
                Trace.WriteLine("Pattern5 Zone4Pattern1 created successfully");

                #endregion

                // Get Pattern Ids based on reservoir
                long patId1 = 0, patId2 = 0; //, patId3 = 0, patId4 = 0, patId5 = 0;
                List<PatternDTO> patternsInZone1 = new List<PatternDTO>();
                patternsInZone1 = WellAllocationService.GetPatternsByZoneId(zone1Id.ToString());
                if (patternsInZone1 != null && patternsInZone1.Count > 0)
                {
                    patId1 = patternsInZone1[0].Id;
                    patId2 = patternsInZone1[1].Id;
                }

                #region Pattern-well and/or Zone-well mappings
                // each pattern has one well mapped and zone 5 has one well mapped to it
                long[] assetIds = { asset.Id };

                WellDTO[] wells = WellService.GetWellsByUserAssetIds(assetIds);
                int wellcount = wells.Length;
                if (wellcount > 0)
                {
                    long[] wellIds = new long[wellcount];
                    for (int i = 0; i < wellcount; i++)
                    {
                        wellIds[i] = wells[i].Id;
                    }

                    // first well is mapped to 2 patterns with 30% and 70% contribution each
                    WellToPatternDTO welltoPattern1 = new WellToPatternDTO { PatternId = patId1, WellId = wellIds[0], StartDate = startDate, EndDate = endDate };
                    WellToPatternDTO welltoPattern2 = new WellToPatternDTO { PatternId = patId2, WellId = wellIds[0], StartDate = startDate, EndDate = endDate };
                    WellAllocationService.AddWellToPattern(welltoPattern1);
                    WellAllocationService.AddWellToPattern(welltoPattern2);

                    WellPatternSnapshotDTO wellsnap1 = WellAllocationService.GetWellPatternSnapshotByWellIdAndDate(wellIds[0].ToString(), applicableDate.ToISO8601Date());
                    if (wellsnap1.Id > 0)
                    {
                        wellsnap1.StartDate = startDate;
                        List<WellPatternContributionDTO> lstContributions = wellsnap1.WellPatternContributions;
                        // modify contributions 30% and 70%
                        lstContributions[0].ContributionFactor = 30;
                        lstContributions[1].ContributionFactor = 70;
                        WellAllocationService.UpdateWellPatternSnapshot(wellsnap1, applicableDateInUTC.ToISO8601());
                    }

                }
                #endregion

                #region Verify Subsurface Hierarchy created so far with the SubSurface Tree from Database 

                // Create a new well filter.
                WellFilterDTO wellFilter = new WellFilterDTO();
                // Include all well types.
                wellFilter.welFK_r_WellTypeValues = new[] {
                             new WellFilterValueDTO { Value = WellTypeId.NF.ToString() },
                             new WellFilterValueDTO { Value = WellTypeId.ESP.ToString() }
                };
                long?[] assetIDs = { asset.Id };

                wellFilter.AssetIds = assetIDs; 
                SubsurfaceHierarchyDTO subsurfaceTreeFromDB = WellAllocationService.GetSubsurfaceHierarchiesByDate(wellFilter, applicableDateInUTC.ToISO8601());
                if (subsurfaceTreeFromDB.Nodes != null && subsurfaceTreeFromDB.Nodes.Count > 0)
                {
                    // Verify Asset
                    if (subsurfaceTreeFromDB.Nodes[0].Type == AssetHierarchyItemType.Asset)
                        Assert.AreEqual(subsurfaceTreeFromDB.Nodes[0].Name, asset.Name, " Created Asset Name: " + asset.Name + "  Asset Name read from DB: " + subsurfaceTreeFromDB.Nodes[0].Name);

                    // Verify Reservoirs
                    if (subsurfaceTreeFromDB.Nodes[0].Nodes != null && subsurfaceTreeFromDB.Nodes[0].Nodes.Count > 0)
                    {
                        // First Node: Reservoir type
                        SubsurfaceHierarchyNodeDTO res1FromDB = subsurfaceTreeFromDB.Nodes[0].Nodes[0];
                        if (res1FromDB.Type == AssetHierarchyItemType.Reservoir)
                        {
                            Assert.AreEqual(res1FromDB.Name, "Reservoir1", "Created Reservoir Name: Reservoir1 " + "  Reservoir Name read from DB: " + res1FromDB.Name);

                            // Verify Zone
                            // Reservoir 1 has 3 Zones, Reservoir 2 has 2 zones
                            if (res1FromDB.Nodes != null && res1FromDB.Nodes.Count > 0)
                            {
                                List<SubsurfaceHierarchyNodeDTO> zonesFromDB = res1FromDB.Nodes;
                                if (zonesFromDB != null && zonesFromDB[0].Type == AssetHierarchyItemType.Zone)
                                {
                                    Assert.AreEqual(zonesFromDB[0].Name, "Res1Zone1", "Created zone Name: Res1Zone1 " + "  Zone Name read from DB: " + zonesFromDB[0].Name);
                                    Assert.AreEqual(zonesFromDB[1].Name, "Res1Zone2", "Created zone Name: Res1Zone2 " + "  Zone Name read from DB: " + zonesFromDB[1].Name);
                                    Assert.AreEqual(zonesFromDB[2].Name, "Res1Zone3", "Created zone Name: Res1Zone3 " + "  Zone Name read from DB: " + zonesFromDB[2].Name);

                                    // Verify Patterns
                                    //Zone1 has 2 pattern and zone 2, 3 & 4 has one pattern each ...no pattern for zone 5
                                    if (zonesFromDB[0].Nodes != null && zonesFromDB[0].Nodes.Count > 0)
                                    {
                                        List<SubsurfaceHierarchyNodeDTO> patternsFromDB = zonesFromDB[0].Nodes;
                                        if (patternsFromDB != null && patternsFromDB[0].Type == AssetHierarchyItemType.Pattern)
                                        {
                                            Assert.AreEqual(patternsFromDB[0].Name, "Zone1Pattern1", "Created pattern Name: Zone1Pattern1 " + "  pattern Name read from DB: " + patternsFromDB[0].Name);
                                            Assert.AreEqual(patternsFromDB[1].Name, "Zone1Pattern2", "Created pattern Name: Zone1Pattern2 " + "  pattern Name read from DB: " + patternsFromDB[1].Name);

                                            // first well is mapped to 2 patterns with 30% and 70% contribution each
                                            if (patternsFromDB[0].Nodes != null && patternsFromDB[0].Nodes.Count > 0)
                                            {
                                                List<SubsurfaceHierarchyNodeDTO> welltoPatternsFromDB = patternsFromDB[0].Nodes;
                                                if (welltoPatternsFromDB != null)
                                                {
                                                    Assert.AreEqual(welltoPatternsFromDB[0].Name, "ESPWell1", "Mapped well Name: ESPWell1 " + "  Mapped well Name read from DB: " + welltoPatternsFromDB[0].Name);
                                                    // this was the last node in the hierarchy
                                                    WellPatternSnapshotDTO snap1 = WellAllocationService.GetWellPatternSnapshotByWellIdAndDate(welltoPatternsFromDB[0].Id.ToString(), applicableDate.ToISO8601Date());
                                                    if (snap1.Id > 0)
                                                    {
                                                        List<WellPatternContributionDTO> contriFromDB = snap1.WellPatternContributions;
                                                        Assert.AreEqual(contriFromDB[0].ContributionFactor.ToString(), "30.00", "Assigned contribution for Well1 in Pattern1 " + "  contribution read from DB: " + contriFromDB[0].ContributionFactor.ToString());
                                                        Assert.AreEqual(contriFromDB[1].ContributionFactor.ToString(), "70.00", "Assigned contribution for Well1 in Pattern1 " + "  contribution read from DB: " + contriFromDB[1].ContributionFactor.ToString());
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        // Second Node
                        SubsurfaceHierarchyNodeDTO res2FromDB = subsurfaceTreeFromDB.Nodes[0].Nodes[1];
                        if (res2FromDB.Type == AssetHierarchyItemType.Reservoir)
                        {
                            Assert.AreEqual(res2FromDB.Name, "Reservoir2", "Created Reservoir Name: Reservoir2 " + "  Reservoir Name read from DB: " + res2FromDB.Name);

                            // Zone Type
                            // Reservoir 1 has 3 Zones, Reservoir 2 has 2 zones
                            if (res2FromDB.Nodes != null && res2FromDB.Nodes.Count > 0)
                            {
                                List<SubsurfaceHierarchyNodeDTO> zones2FromDB = res2FromDB.Nodes;
                                if (zones2FromDB != null)
                                {
                                    Assert.AreEqual(zones2FromDB[0].Name, "Res2Zone4", "Created zone Name: Res2Zone4 " + "  Zone Name read from DB: " + zones2FromDB[0].Name);
                                    Assert.AreEqual(zones2FromDB[1].Name, "Res2Zone5", "Created zone Name: Res2Zone5 " + "  Zone Name read from DB: " + zones2FromDB[1].Name);
                                }
                            }
                        }
                    }
                }
                #endregion
            }
            finally
            {
                WellAllocationService.DeleteHierarchyByAssetId(asset.Id.ToString());
                Trace.WriteLine("Pattern1 pattern deleted successfully");
                Trace.WriteLine("Zone1 zone deleted successfully");
                Trace.WriteLine("Res1 reservoir deleted successfully");
            }

        }
        #endregion VRR Tests

        [TestCategory(TestCategories.WellAllocationServiceTests), TestMethod]
        public void GetFluidProperties_BO_WithoutModelOptionsCheck()
        {

            string Path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";
            Tuple<string, WellTypeId, ModelFileOptionDTO>[] models = {
             Tuple.Create("WellfloESPExample1.wflx",WellTypeId.ESP, new ModelFileOptionDTO(){ CalibrationMethod = CalibrationMethodId.LFactor, OptionalUpdate = new long[] { } }),
             Tuple.Create("WellfloNFWExample1.wflx", WellTypeId.NF, new ModelFileOptionDTO(){ CalibrationMethod = CalibrationMethodId.LFactor, OptionalUpdate = new long[] { } }),
             Tuple.Create("WellfloWaterInjectionExample1.wflx", WellTypeId.WInj, new ModelFileOptionDTO(){ CalibrationMethod = CalibrationMethodId.LFactor, OptionalUpdate = new long[] { } }),
             Tuple.Create("GL_BlackOil.wflx", WellTypeId.GLift, new ModelFileOptionDTO(){ CalibrationMethod = CalibrationMethodId.LFactor, OptionalUpdate = new long[] { } }),
             Tuple.Create("WellfloGasInjectionExample1.wflx", WellTypeId.GInj, new ModelFileOptionDTO(){ CalibrationMethod = CalibrationMethodId.LFactor, OptionalUpdate = new long[] { } }),
              Tuple.Create("PCP-SinglePhase.wflx", WellTypeId.PCP,new ModelFileOptionDTO(){ CalibrationMethod = CalibrationMethodId.LFactor, OptionalUpdate = new long[] { } }),

            };
            foreach (Tuple<string, WellTypeId, ModelFileOptionDTO> modelInfo in models)
            {
                int i = 1;
                string model = modelInfo.Item1;
                WellTypeId wellType = modelInfo.Item2;
                ModelFileOptionDTO options = modelInfo.Item3;

                Trace.WriteLine("Testing model: " + model);
                //Create a new well
                WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = SetDefaultFluidType(new WellDTO() { Name = DefaultWellName + wellType.ToString(), CommissionDate = DateTime.Today.AddYears(-2), WellType = wellType }) });
                var allWells = WellService.GetAllWells().ToList();
                WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName + wellType.ToString()));
                Assert.IsNotNull(well);
                _wellsToRemove.Add(well);

                ModelFileBase64DTO modelFile = new ModelFileBase64DTO() { };

                options.Comment = "CASETest Upload " + modelInfo.Item1;
                modelFile.Options = options;
                modelFile.ApplicableDate = DateTime.Today.ToUniversalTime() - TimeSpan.FromDays(i);
                modelFile.WellId = well.Id;

                byte[] fileAsByteArray = GetByteArray(Path, modelInfo.Item1);
                Assert.IsNotNull(fileAsByteArray);
                modelFile.Base64Contents = Convert.ToBase64String(fileAsByteArray);
                Assert.AreEqual(wellType, ModelFileService.GetWellModelType(modelFile), "Model File type mismatch");
                ModelFileValidationDataDTO ModelFileValidationData = ModelFileService.GetModelFileValidation(modelFile);
                Assert.IsNotNull(ModelFileValidationData);
                ModelFileService.AddWellModelFile(modelFile);
                ModelFileDTO newModelFile = ModelFileService.GetCurrentModelFile(well.Id.ToString());
                Assert.IsNotNull(newModelFile);
                _modelFilesToRemove.Add(newModelFile.Id.ToString());

                #region ESP

                if (wellType == WellTypeId.ESP)
                {
                    //add new wellTestData
                    WellTestDTO testDataDTO = new WellTestDTO()
                    {
                        WellId = well.Id,
                        SPTCode = 0,
                        SPTCodeDescription = "AllocatableTest",
                        AverageTubingPressure = 0,
                        AverageTubingTemperature = 200,
                        GaugePressure = 104,
                        Oil = (decimal)2501.80,
                        Gas = 1250,
                        Water = (decimal)3752.60,
                        ChokeSize = 64,
                        Frequency = 70,

                    };

                    testDataDTO.SampleDate = (DateTime.Today.ToUniversalTime());
                    // add wellTest with new non RRL properties
                    WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(well.Id.ToString()).Units;
                    WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testDataDTO));

                    WellTestDTO[] allTunedAgainTests = WellTestDataService.GetAllValidWellTestByWellId(well.Id.ToString());

                    WellTestDTO latestTestData_ESP = WellTestDataService.GetLatestWellTestDataByWellId(well.Id.ToString());

                    Assert.AreEqual("TUNING_SUCCEEDED", latestTestData_ESP.Status.ToString(), "Well Test Status is not Success");
                    //Get fluid properties based on fluid type
                    ReservoirFluidPropertiesAndUnitsDTO GetFluidPropertiesAndUnits = WellAllocationService.GetFluidPropertiesForReservoirWorkFlow(well.Id.ToString());
                    ReservoirFluidPropertiesDTO GetFluidProperties = GetFluidPropertiesAndUnits.Value;
                    Assert.IsNull(GetFluidProperties.VaporizedCGR, "Vaporized CGR is not null for Black oil fluid type");
                    Assert.IsTrue(GetFluidProperties.FromWellTest);
                    Assert.IsNotNull(GetFluidProperties.GasFormationVolumeFactor, "GasFormationVolumeFactor is null for Black oil fluid type");
                    Assert.AreEqual(0.006, Math.Round((double)GetFluidProperties.GasFormationVolumeFactor, 3), "Value is mismatched for GasFormationVolumeFactor");
                    Assert.IsNotNull(GetFluidProperties.OilFormationVolumeFactor, "OilFormationVolumeFactor is null for Black oil fluid type");
                    Assert.AreEqual(1.246, Math.Round((double)GetFluidProperties.OilFormationVolumeFactor, 3), "Value is mismatched for OilFormationVolumeFactor");
                    Assert.IsNotNull(GetFluidProperties.GasFormationVolumeFactor, "GasFormationVolumeFactor is null for Black oil fluid type");
                    //Phase ration should come from Well model file
                    Assert.IsNotNull(GetFluidProperties.PhaseRatio, "Phase Ratio is null for Black Oil fluid type");
                    Assert.AreEqual(500, GetFluidProperties.PhaseRatio, "Phase Ratio value is mismatched");
                    Assert.IsNotNull(GetFluidProperties.ReservoirPressure, "ReservoirPressure is null for Black oil fluid type");
                    Assert.AreEqual(3072.0, Math.Round((double)GetFluidProperties.ReservoirPressure, 3), "ReservoirPressure value is mismatched");
                    Assert.IsNotNull(GetFluidProperties.ReservoirTemperature, "ReservoirTemperature is null for Black oil fluid type");
                    Assert.AreEqual(200, GetFluidProperties.ReservoirTemperature, "ReservoirTemperature value is mismatched");
                    Assert.IsNotNull(GetFluidProperties.SaturationPressure, "  SaturationPressure is null for Black oil fluid type");
                    Assert.AreEqual(2816.901, Math.Round((double)GetFluidProperties.SaturationPressure, 3), "  SaturationPressure value is mismatched");
                    Assert.IsNotNull(GetFluidProperties.SolutionGOR, "SolutionGOR is null for Black Oil fluid type");
                    Assert.AreEqual(500, GetFluidProperties.SolutionGOR, "SolutionGOR value is mismatched");
                    Assert.IsNotNull(GetFluidProperties.WaterFormationVolumeFactor, "WaterFormationVolumeFactor is null for Black Oil fluid type");
                    Assert.AreEqual(1.034, Math.Round((double)GetFluidProperties.WaterFormationVolumeFactor, 3), "WaterFormationVolumeFactor value is mismatched");
                }

                #endregion ESP

                #region WaterInjection

                else if (wellType == WellTypeId.WInj)
                {
                    //add new wellTestData
                    WellTestDTO testDataDTO = new WellTestDTO()
                    {
                        WellId = well.Id,
                        SPTCode = 0,
                        SPTCodeDescription = "AllocatableTest",
                        AverageTubingPressure = (decimal)3514.7,
                        AverageTubingTemperature = 65,
                        PumpIntakePressure = 10000,
                        PumpDischargePressure = 11000,
                        GaugePressure = 12000,
                        Water = (decimal)6932.80,
                        ChokeSize = 60,
                        FlowLinePressure = 50,
                    };

                    testDataDTO.SampleDate = (DateTime.Today.ToUniversalTime());
                    // add wellTest with new non RRL properties
                    WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(well.Id.ToString()).Units;
                    WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testDataDTO));

                    WellTestDTO[] allTunedAgainTests = WellTestDataService.GetAllValidWellTestByWellId(well.Id.ToString());

                    WellTestDTO latestTestData_WI = WellTestDataService.GetLatestWellTestDataByWellId(well.Id.ToString());

                    Assert.AreEqual("TUNING_SUCCEEDED", latestTestData_WI.Status.ToString(), "Well Test Status is not Success");
                    //Get fluid Properties
                    ReservoirFluidPropertiesAndUnitsDTO GetFluidPropertiesAndUnits = WellAllocationService.GetFluidPropertiesForReservoirWorkFlow(well.Id.ToString());
                    ReservoirFluidPropertiesDTO GetFluidProperties = GetFluidPropertiesAndUnits.Value;

                    Assert.IsNull(GetFluidProperties.VaporizedCGR, "Vaporized CGR is not null for Black oil fluid type");
                    Assert.IsTrue(GetFluidProperties.FromWellTest);
                    Assert.IsNull(GetFluidProperties.GasFormationVolumeFactor, "GasFormationVolumeFactor is null for Black oil fluid type");
                    // Assert.AreEqual(0.006, Math.Round((double)GetFluidProperties.GasFormationVolumeFactor, 3), "Value is mismatched for GasFormationVolumeFactor");
                    Assert.IsNull(GetFluidProperties.OilFormationVolumeFactor, "OilFormationVolumeFactor is null for Black oil fluid type");
                    //Assert.AreEqual(1.246, Math.Round((double)GetFluidProperties.OilFormationVolumeFactor, 3), "Value is mismatched for OilFormationVolumeFactor");
                    Assert.IsNull(GetFluidProperties.GasFormationVolumeFactor, "GasFormationVolumeFactor is null for Black oil fluid type");
                    Assert.IsNull(GetFluidProperties.PhaseRatio, "Phase Ratio is not null for Black Oil fluid type");
                    Assert.IsNotNull(GetFluidProperties.ReservoirPressure, "ReservoirPressure is null for Black oil fluid type");
                    Assert.AreEqual(6500, Math.Round((double)GetFluidProperties.ReservoirPressure, 3), "ReservoirPressure value is mismatched");
                    Assert.IsNotNull(GetFluidProperties.ReservoirTemperature, "ReservoirTemperature is null for Black oil fluid type");
                    Assert.AreEqual(60, GetFluidProperties.ReservoirTemperature, "ReservoirTemperature value is mismatched");
                    Assert.IsNull(GetFluidProperties.SaturationPressure, " SaturationPressure is null for Black oil fluid type");
                    //Assert.AreEqual(2816.901, Math.Round((double)GetFluidProperties.SaturationPressure, 3), "  SaturationPressure value is mismatched");
                    Assert.IsNull(GetFluidProperties.SolutionGOR, "SolutionGOR is null for Black Oil fluid type");
                    //Assert.AreEqual(500, GetFluidProperties.SolutionGOR, "SolutionGOR value is mismatched");
                    Assert.IsNotNull(GetFluidProperties.WaterFormationVolumeFactor, "WaterFormationVolumeFactor is null for Black Oil fluid type");
                    Assert.AreEqual(1, Math.Round((double)GetFluidProperties.WaterFormationVolumeFactor, 3), "WaterFormationVolumeFactor value is mismatched");
                }

                #endregion WaterInjection

                #region GasLift

                else if (wellType == WellTypeId.GLift)
                {
                    //add new wellTestData
                    WellTestDTO testDataDTO = new WellTestDTO()
                    {
                        WellId = well.Id,
                        SPTCode = 0,
                        SPTCodeDescription = "AllocatableTest",
                        AverageTubingPressure = 96,
                        AverageTubingTemperature = 100,
                        AverageCasingPressure = 1000,
                        GasInjectionRate = 1000,
                        FlowLinePressure = 50,
                        SeparatorPressure = 30,
                        GaugePressure = 12000,
                        Oil = (decimal)599.00,
                        Gas = 1020,
                        Water = (decimal)732.2,
                        ChokeSize = 50,
                    };

                    testDataDTO.SampleDate = (DateTime.Today.ToUniversalTime());
                    WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(well.Id.ToString()).Units;
                    WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testDataDTO));

                    WellTestDTO[] allTunedAgainTests = WellTestDataService.GetAllValidWellTestByWellId(well.Id.ToString());

                    WellTestDTO latestTestData_GL = WellTestDataService.GetLatestWellTestDataByWellId(well.Id.ToString());

                    Assert.AreEqual("TUNING_SUCCEEDED", latestTestData_GL.Status.ToString(), "Well Test Status is not Success");
                    //Get fluid Properties
                    ReservoirFluidPropertiesAndUnitsDTO GetFluidPropertiesAndUnits = WellAllocationService.GetFluidPropertiesForReservoirWorkFlow(well.Id.ToString());
                    ReservoirFluidPropertiesDTO GetFluidProperties = GetFluidPropertiesAndUnits.Value;
                    Assert.IsNull(GetFluidProperties.VaporizedCGR, "Vaporized CGR is not null for Black oil fluid type");
                    Assert.IsTrue(GetFluidProperties.FromWellTest);
                    Assert.IsNotNull(GetFluidProperties.GasFormationVolumeFactor, "GasFormationVolumeFactor is null for Black oil fluid type");
                    Assert.AreEqual(0.006, Math.Round((double)GetFluidProperties.GasFormationVolumeFactor, 3), "Value is mismatched for GasFormationVolumeFactor");
                    Assert.IsNotNull(GetFluidProperties.OilFormationVolumeFactor, "OilFormationVolumeFactor is null for Black oil fluid type");
                    Assert.AreEqual(1.468, Math.Round((double)GetFluidProperties.OilFormationVolumeFactor, 3), "Value is mismatched for OilFormationVolumeFactor");
                    Assert.IsNotNull(GetFluidProperties.GasFormationVolumeFactor, "GasFormationVolumeFactor is null for Black oil fluid type");
                    Assert.IsNotNull(GetFluidProperties.PhaseRatio, "Phase Ratio is null for Black Oil fluid type");
                    Assert.AreEqual(1702, Math.Round((double)GetFluidProperties.PhaseRatio, 3), "Phase Ratio value is mismatched");
                    Assert.IsNotNull(GetFluidProperties.ReservoirPressure, "ReservoirPressure is null for Black oil fluid type");
                    Assert.AreEqual(3000, Math.Round((double)GetFluidProperties.ReservoirPressure, 3), "ReservoirPressure value is mismatched");
                    Assert.IsNotNull(GetFluidProperties.ReservoirTemperature, "ReservoirTemperature is null for Black oil fluid type");
                    Assert.AreEqual(246.9, Math.Round((double)GetFluidProperties.ReservoirTemperature, 3), "ReservoirTemperature value is mismatched");
                    Assert.IsNotNull(GetFluidProperties.SaturationPressure, "  SaturationPressure is null for Black oil fluid type");
                    Assert.AreEqual(5520.362, Math.Round((double)GetFluidProperties.SaturationPressure, 3), "  SaturationPressure value is mismatched");
                    Assert.IsNotNull(GetFluidProperties.SolutionGOR, "SolutionGOR is null for Black Oil fluid type");
                    Assert.AreEqual(741.424, Math.Round((double)GetFluidProperties.SolutionGOR, 3), "SolutionGOR value is mismatched");
                    Assert.IsNotNull(GetFluidProperties.WaterFormationVolumeFactor, "WaterFormationVolumeFactor is null for Black Oil fluid type");
                    Assert.AreEqual(1.054, Math.Round((double)GetFluidProperties.WaterFormationVolumeFactor, 3), "WaterFormationVolumeFactor value is mismatched");
                }

                #endregion GasLift

                #region GasInjection

                else if (wellType == WellTypeId.GInj)
                {
                    //add new wellTestData
                    WellTestDTO testDataDTO = new WellTestDTO()
                    {
                        WellId = well.Id,
                        SPTCodeDescription = "AllocatableTest",
                        //AverageTubingPressure = random.Next(500, 1900),
                        AverageTubingPressure = (decimal)1514.70,
                        AverageTubingTemperature = (decimal)85.0,
                        //AverageTubingTemperature = random.Next(50, 100),
                        GaugePressure = random.Next(500, 1900),
                        //Gas = random.Next(500, 1900),
                        Gas = (decimal)250.0,
                        FlowLinePressure = random.Next(50, 100)
                    };
                    testDataDTO.SampleDate = (DateTime.Today.ToUniversalTime());
                    WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(well.Id.ToString()).Units;
                    WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testDataDTO));

                    //AddWellSettingWithDoubleValues(well.Id, "Min L Factor Acceptance Limit", 0.1);
                    //AddWellSettingWithDoubleValues(well.Id, "Max L Factor Acceptance Limit", 2.0);

                    WellTestDTO[] allTunedAgainTests = WellTestDataService.GetAllValidWellTestByWellId(well.Id.ToString());

                    WellTestDTO latestTestData_NF = WellTestDataService.GetLatestWellTestDataByWellId(well.Id.ToString());

                    Assert.AreEqual("TUNING_SUCCEEDED", latestTestData_NF.Status.ToString(), "Well Test Status is not Success");
                    //Get fluid Properties
                    ReservoirFluidPropertiesAndUnitsDTO GetFluidPropertiesAndUnits = WellAllocationService.GetFluidPropertiesForReservoirWorkFlow(well.Id.ToString());
                    ReservoirFluidPropertiesDTO GetFluidProperties = GetFluidPropertiesAndUnits.Value;

                    Assert.IsNull(GetFluidProperties.VaporizedCGR, "Vaporized CGR is not null for Black oil fluid type");
                    Assert.IsTrue(GetFluidProperties.FromWellTest);
                    Assert.IsNotNull(GetFluidProperties.GasFormationVolumeFactor, "GasFormationVolumeFactor is null for Black oil fluid type");
                    Assert.AreEqual(0.017, Math.Round((double)GetFluidProperties.GasFormationVolumeFactor, 3), "Value is mismatched for GasFormationVolumeFactor");
                    Assert.IsNull(GetFluidProperties.OilFormationVolumeFactor, "OilFormationVolumeFactor is not null for Black oil fluid type");
                    Assert.IsNull(GetFluidProperties.PhaseRatio, "Phase Ratio is not null for Black Oil fluid type");
                    Assert.IsNotNull(GetFluidProperties.ReservoirPressure, "ReservoirPressure is null for Black oil fluid type");
                    Assert.AreEqual(1014.696, Math.Round((double)GetFluidProperties.ReservoirPressure, 3), "ReservoirPressure value is mismatched");
                    Assert.IsNotNull(GetFluidProperties.ReservoirTemperature, "ReservoirTemperature is null for Black oil fluid type");
                    Assert.AreEqual(190, GetFluidProperties.ReservoirTemperature, "ReservoirTemperature value is mismatched");
                    Assert.IsNull(GetFluidProperties.SaturationPressure, "  SaturationPressure is not null for Black oil fluid type");
                    Assert.IsNull(GetFluidProperties.SolutionGOR, "SolutionGOR is not null for Black Oil fluid type");
                    Assert.IsNull(GetFluidProperties.WaterFormationVolumeFactor, "WaterFormationVolumeFactor is not null for Black Oil fluid type");

                }
                #endregion GasInjection

                #region NF

                else if (wellType == WellTypeId.NF)
                {
                    //add new wellTestData
                    WellTestDTO testDataDTO = new WellTestDTO()
                    {
                        WellId = well.Id,
                        SPTCode = 2,
                        SPTCodeDescription = "RepresentativeTest",
                        AverageTubingPressure = (decimal)164.7,
                        AverageTubingTemperature = 80,
                        //GaugePressure = 12000,
                        Oil = (decimal)1768.00,
                        Gas = 880,
                        Water = (decimal)589.30,
                        ChokeSize = 50,
                        FlowLinePressure = 50,
                        SeparatorPressure = 10000,
                    };

                    testDataDTO.SampleDate = (DateTime.Today.ToUniversalTime());
                    WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(well.Id.ToString()).Units;
                    WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testDataDTO));

                    WellTestDTO[] allTunedAgainTests = WellTestDataService.GetAllValidWellTestByWellId(well.Id.ToString());

                    WellTestDTO latestTestData_NF = WellTestDataService.GetLatestWellTestDataByWellId(well.Id.ToString());

                    Assert.AreEqual("TUNING_SUCCEEDED", latestTestData_NF.Status.ToString(), "Well Test Status is not Success");
                    //Get fluid properties
                    ReservoirFluidPropertiesAndUnitsDTO GetFluidPropertiesAndUnits = WellAllocationService.GetFluidPropertiesForReservoirWorkFlow(well.Id.ToString());
                    ReservoirFluidPropertiesDTO GetFluidProperties = GetFluidPropertiesAndUnits.Value;

                    Assert.IsNull(GetFluidProperties.VaporizedCGR, "Vaporized CGR is not null for Black oil fluid type");
                    Assert.IsFalse(GetFluidProperties.FromWellTest);
                    Assert.IsNotNull(GetFluidProperties.GasFormationVolumeFactor, "GasFormationVolumeFactor is null for Black oil fluid type");
                    Assert.AreEqual(0.003, Math.Round((double)GetFluidProperties.GasFormationVolumeFactor, 3), "Value is mismatched for GasFormationVolumeFactor");
                    Assert.IsNotNull(GetFluidProperties.OilFormationVolumeFactor, "OilFormationVolumeFactor is null for Black oil fluid type");
                    Assert.AreEqual(1.197, Math.Round((double)GetFluidProperties.OilFormationVolumeFactor, 3), "Value is mismatched for OilFormationVolumeFactor");
                    Assert.IsNotNull(GetFluidProperties.GasFormationVolumeFactor, "GasFormationVolumeFactor is null for Black oil fluid type");
                    Assert.IsNotNull(GetFluidProperties.PhaseRatio, "Phase Ratio is null for Black Oil fluid type");
                    //Phase ratio should come from Well model file when options are not set
                    Assert.AreEqual(500, GetFluidProperties.PhaseRatio, "Phase Ratio value is mismatched");
                    Assert.IsNotNull(GetFluidProperties.ReservoirPressure, "ReservoirPressure is null for Black oil fluid type");
                    Assert.AreEqual(6000, Math.Round((double)GetFluidProperties.ReservoirPressure, 3), "ReservoirPressure value is mismatched");
                    Assert.IsNotNull(GetFluidProperties.ReservoirTemperature, "ReservoirTemperature is null for Black oil fluid type");
                    Assert.AreEqual(196, GetFluidProperties.ReservoirTemperature, "ReservoirTemperature value is mismatched");
                    Assert.IsNotNull(GetFluidProperties.SaturationPressure, "  SaturationPressure is null for Black oil fluid type");
                    Assert.AreEqual(2804.407, Math.Round((double)GetFluidProperties.SaturationPressure, 3), "  SaturationPressure value is mismatched");
                    Assert.IsNotNull(GetFluidProperties.SolutionGOR, "SolutionGOR is null for Black Oil fluid type");
                    Assert.AreEqual(500, GetFluidProperties.SolutionGOR, "SolutionGOR value is mismatched");
                    Assert.IsNotNull(GetFluidProperties.WaterFormationVolumeFactor, "WaterFormationVolumeFactor is null for Black Oil fluid type");
                    Assert.AreEqual(1.029, Math.Round((double)GetFluidProperties.WaterFormationVolumeFactor, 3), "WaterFormationVolumeFactor value is mismatched");
                }
                #endregion NF

                #region PCP

                if (wellType == WellTypeId.PCP)
                {
                    //add new wellTestData
                    WellTestDTO testDataDTO = new WellTestDTO()
                    {
                        WellId = well.Id,
                        SPTCodeDescription = "Allocatable Test",
                        TestDuration = 24,
                        Oil = (decimal)210.2,
                        Water = (decimal)140.2,
                        AverageTubingPressure = (decimal)100.00,
                        AverageTubingTemperature = (decimal)80.00,
                        PumpSpeed = (decimal)225.00,
                        FlowLinePressure = (decimal)1862.00,
                        ChokeSize = (decimal)64.00,
                        Comment = "PCPWellTest_Comment_Check",
                    };

                    testDataDTO.SampleDate = (DateTime.Today.ToUniversalTime());
                    // add wellTest with new non RRL properties
                    WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(well.Id.ToString()).Units;
                    WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testDataDTO));

                    WellTestDTO[] allTunedAgainTests = WellTestDataService.GetAllValidWellTestByWellId(well.Id.ToString());

                    WellTestDTO latestTestData_PCP = WellTestDataService.GetLatestWellTestDataByWellId(well.Id.ToString());

                    Assert.AreEqual("TUNING_SUCCEEDED", latestTestData_PCP.Status.ToString(), "Well Test Status is not Success");
                    //Get fluid properties
                    ReservoirFluidPropertiesAndUnitsDTO GetFluidPropertiesAndUnits = WellAllocationService.GetFluidPropertiesForReservoirWorkFlow(well.Id.ToString());
                    ReservoirFluidPropertiesDTO GetFluidProperties = GetFluidPropertiesAndUnits.Value;

                    Assert.IsNull(GetFluidProperties.VaporizedCGR, "Vaporized CGR is not null for Black oil fluid type");
                    Assert.IsTrue(GetFluidProperties.FromWellTest);
                    Assert.IsNotNull(GetFluidProperties.GasFormationVolumeFactor, "GasFormationVolumeFactor is null for Black oil fluid type");
                    Assert.AreEqual(0.008, Math.Round((double)GetFluidProperties.GasFormationVolumeFactor, 3), "Value is mismatched for GasFormationVolumeFactor");
                    Assert.IsNotNull(GetFluidProperties.OilFormationVolumeFactor, "OilFormationVolumeFactor is null for Black oil fluid type");
                    Assert.AreEqual(1.02, Math.Round((double)GetFluidProperties.OilFormationVolumeFactor, 3), "Value is mismatched for OilFormationVolumeFactor");
                    Assert.IsNotNull(GetFluidProperties.GasFormationVolumeFactor, "GasFormationVolumeFactor is null for Black oil fluid type");
                    Assert.IsNotNull(GetFluidProperties.PhaseRatio, "Phase Ratio is null for Black Oil fluid type");
                    Assert.AreEqual(0, GetFluidProperties.PhaseRatio, "Phase Ratio value is mismatched");
                    Assert.IsNotNull(GetFluidProperties.ReservoirPressure, "ReservoirPressure is null for Black oil fluid type");
                    Assert.AreEqual(1700, Math.Round((double)GetFluidProperties.ReservoirPressure, 3), "ReservoirPressure value is mismatched");
                    Assert.IsNotNull(GetFluidProperties.ReservoirTemperature, "ReservoirTemperature is null for Black oil fluid type");
                    Assert.AreEqual(140, GetFluidProperties.ReservoirTemperature, "ReservoirTemperature value is mismatched");
                    Assert.IsNotNull(GetFluidProperties.SaturationPressure, "  SaturationPressure is null for Black oil fluid type");
                    Assert.AreEqual(0, Math.Round((double)GetFluidProperties.SaturationPressure, 3), "  SaturationPressure value is mismatched");
                    Assert.IsNotNull(GetFluidProperties.SolutionGOR, "SolutionGOR is null for Black Oil fluid type");
                    Assert.AreEqual(0, GetFluidProperties.SolutionGOR, "SolutionGOR value is mismatched");
                    Assert.IsNotNull(GetFluidProperties.WaterFormationVolumeFactor, "WaterFormationVolumeFactor is null for Black Oil fluid type");
                    Assert.AreEqual(1.014, Math.Round((double)GetFluidProperties.WaterFormationVolumeFactor, 3), "WaterFormationVolumeFactor value is mismatched");
                }
            }
            #endregion PCP

        }
        [TestCategory(TestCategories.WellAllocationServiceTests), TestMethod]
        public void GetFluidProperties_NFCondensate_WithoutModelOptionsCheck()
        {
            var modelFileName = "Condensate Gas - IPR Auto Tuning.wflx";
            var wellType = WellTypeId.NF;
            var options = new ModelFileOptionDTO
            {
                CalibrationMethod = CalibrationMethodId.DarcyFlowCoefficient,
                OptionalUpdate = new long[]
                    {
                        //(long) OptionalUpdates.UpdateGOR_CGR
                    }
            };

            Trace.WriteLine("Testing model: " + modelFileName);

            WellDTO well = AddNonRRLWellGeneralTab("NFWWELL_", wellType, WellFluidType.Condensate, "N/A", "NA");
            AddNonRRLModelFile(well, modelFileName, options.CalibrationMethod, options.OptionalUpdate);
            Trace.WriteLine("Well Added Successfully");

            WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(well.Id.ToString()).Units;
            WellTestDTO testData = new WellTestDTO
            {
                WellId = well.Id,
                SPTCodeDescription = "AllocatableTest",
                CalibrationMethod = options.CalibrationMethod,
                WellTestType = WellTestType.WellTest,
                AverageTubingPressure = 4000,
                AverageTubingTemperature = 100,
                Gas = 35520,
                Water = 5327.6m,
                Oil = 5500.6m,
                ChokeSize = 32,
                FlowLinePressure = 4000,
                SeparatorPressure = 4000,
                TestDuration = 24,
                SampleDate = DateTime.Today.AddDays(-5).ToUniversalTime()
            };

            WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testData));
            WellTestDTO[] allTunedAgainTests = WellTestDataService.GetAllValidWellTestByWellId(well.Id.ToString());

            WellTestDTO latestTestData_NF = WellTestDataService.GetLatestWellTestDataByWellId(well.Id.ToString());

            Assert.AreEqual("TUNING_SUCCEEDED", latestTestData_NF.Status.ToString(), "Well Test Status is not Success");
            //Get fluid properties
            ReservoirFluidPropertiesAndUnitsDTO GetFluidPropertiesAndUnits = WellAllocationService.GetFluidPropertiesForReservoirWorkFlow(well.Id.ToString());
            ReservoirFluidPropertiesDTO GetFluidProperties = GetFluidPropertiesAndUnits.Value;

            Assert.IsNotNull(GetFluidProperties.VaporizedCGR, "Vaporized CGR is null for Condensate fluid type");
            Assert.IsTrue(GetFluidProperties.FromWellTest);
            Assert.IsNotNull(GetFluidProperties.GasFormationVolumeFactor, "GasFormationVolumeFactor is null for Condensate fluid type");
            Assert.AreEqual(0.004, Math.Round((double)GetFluidProperties.GasFormationVolumeFactor, 3), "Value is mismatched for GasFormationVolumeFactor");
            Assert.IsNotNull(GetFluidProperties.OilFormationVolumeFactor, "OilFormationVolumeFactor is null for Condensate fluid type");
            Assert.AreEqual(2.217, Math.Round((double)GetFluidProperties.OilFormationVolumeFactor, 3), "Value is mismatched for OilFormationVolumeFactor");
            Assert.IsNotNull(GetFluidProperties.PhaseRatio, "Phase Ratio is null for for Condensate fluid type");
            //Phase ratio value should come from Well model file when no model options are set
            Assert.AreEqual(150, Math.Round((double)GetFluidProperties.PhaseRatio, 3), "Phase Ratio value is mismatched");
            Assert.AreEqual(150, Math.Round((double)GetFluidProperties.VaporizedCGR, 3), "VaporizedCGR value is mismatched");
            Assert.IsNotNull(GetFluidProperties.ReservoirPressure, "ReservoirPressure is null for Condensate fluid type");
            Assert.AreEqual(6050, Math.Round((double)GetFluidProperties.ReservoirPressure, 3), "ReservoirPressure value is mismatched");
            Assert.IsNotNull(GetFluidProperties.ReservoirTemperature, "ReservoirTemperature is null for Condensate fluid type");
            Assert.AreEqual(230, GetFluidProperties.ReservoirTemperature, "ReservoirTemperature value is mismatched");
            Assert.IsNotNull(GetFluidProperties.SaturationPressure, "  SaturationPressure is null for Condensate fluid type");
            Assert.AreEqual(5334.462, Math.Round((double)GetFluidProperties.SaturationPressure, 3), "  SaturationPressure value is mismatched");
            Assert.IsNotNull(GetFluidProperties.SolutionGOR, "SolutionGOR is null for Condensate fluid type");
            Assert.AreEqual(0, GetFluidProperties.SolutionGOR, "SolutionGOR value is mismatched");
            Assert.IsNotNull(GetFluidProperties.WaterFormationVolumeFactor, "WaterFormationVolumeFactor is null for Condensate fluid type");
            Assert.AreEqual(1.042, Math.Round((double)GetFluidProperties.WaterFormationVolumeFactor, 3), "WaterFormationVolumeFactor value is mismatched");

        }

        [TestCategory(TestCategories.WellAllocationServiceTests), TestMethod]
        public void GetFluidProperties_BO()
        {
            string Path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";
            Tuple<string, WellTypeId, ModelFileOptionDTO>[] models = {
             Tuple.Create("WellfloESPExample1.wflx",WellTypeId.ESP, new ModelFileOptionDTO(){ CalibrationMethod = CalibrationMethodId.LFactor, OptionalUpdate = new long[] { ((long)OptionalUpdates.UpdateWCT_WGR) } }),
             Tuple.Create("WellfloNFWExample1.wflx", WellTypeId.NF, new ModelFileOptionDTO(){ CalibrationMethod = CalibrationMethodId.LFactor, OptionalUpdate = new long[] { ((long)OptionalUpdates.UpdateWCT_WGR) } }),
             Tuple.Create("WellfloWaterInjectionExample1.wflx", WellTypeId.WInj, new ModelFileOptionDTO(){ CalibrationMethod = CalibrationMethodId.LFactor, OptionalUpdate = new long[] {  ((long)OptionalUpdates.UpdateWCT_WGR) } }),
             Tuple.Create("GL_BlackOil.wflx", WellTypeId.GLift, new ModelFileOptionDTO(){ CalibrationMethod = CalibrationMethodId.LFactor, OptionalUpdate = new long[] { ((long)OptionalUpdates.UpdateWCT_WGR) } }),
             Tuple.Create("WellfloGasInjectionExample1.wflx", WellTypeId.GInj, new ModelFileOptionDTO(){ CalibrationMethod = CalibrationMethodId.LFactor, OptionalUpdate = new long[] { ((long)OptionalUpdates.UpdateGOR_CGR) } }),
              Tuple.Create("PCP-SinglePhase.wflx", WellTypeId.PCP,new ModelFileOptionDTO(){ CalibrationMethod = CalibrationMethodId.LFactor, OptionalUpdate = new long[] { ((long)OptionalUpdates.UpdateWCT_WGR), ((long)OptionalUpdates.UpdateGOR_CGR) } }),

            };
            foreach (Tuple<string, WellTypeId, ModelFileOptionDTO> modelInfo in models)
            {
                int i = 1;
                string model = modelInfo.Item1;
                WellTypeId wellType = modelInfo.Item2;
                ModelFileOptionDTO options = modelInfo.Item3;

                Trace.WriteLine("Testing model: " + model);
                //Create a new well
                WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = SetDefaultFluidType(new WellDTO() { Name = DefaultWellName + wellType.ToString(), CommissionDate = DateTime.Today.AddYears(-2), WellType = wellType }) });
                var allWells = WellService.GetAllWells().ToList();
                WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName + wellType.ToString()));
                Assert.IsNotNull(well);
                _wellsToRemove.Add(well);

                ModelFileBase64DTO modelFile = new ModelFileBase64DTO() { };

                options.Comment = "CASETest Upload " + modelInfo.Item1;
                modelFile.Options = options;
                modelFile.ApplicableDate = DateTime.Today.ToUniversalTime() - TimeSpan.FromDays(i);
                modelFile.WellId = well.Id;

                byte[] fileAsByteArray = GetByteArray(Path, modelInfo.Item1);
                Assert.IsNotNull(fileAsByteArray);
                modelFile.Base64Contents = Convert.ToBase64String(fileAsByteArray);
                Assert.AreEqual(wellType, ModelFileService.GetWellModelType(modelFile), "Model File type mismatch");
                ModelFileValidationDataDTO ModelFileValidationData = ModelFileService.GetModelFileValidation(modelFile);
                Assert.IsNotNull(ModelFileValidationData);
                ModelFileService.AddWellModelFile(modelFile);
                ModelFileDTO newModelFile = ModelFileService.GetCurrentModelFile(well.Id.ToString());
                Assert.IsNotNull(newModelFile);
                _modelFilesToRemove.Add(newModelFile.Id.ToString());

                #region ESP

                if (wellType == WellTypeId.ESP)
                {
                    //add new wellTestData
                    WellTestDTO testDataDTO = new WellTestDTO()
                    {
                        WellId = well.Id,
                        SPTCode = 0,
                        SPTCodeDescription = "AllocatableTest",
                        AverageTubingPressure = 0,
                        AverageTubingTemperature = 200,
                        GaugePressure = 104,
                        Oil = (decimal)2501.80,
                        Gas = 1250,
                        Water = (decimal)3752.60,
                        ChokeSize = 64,
                        Frequency = 70,

                    };

                    testDataDTO.SampleDate = (DateTime.Today.ToUniversalTime());
                    // add wellTest with new non RRL properties
                    WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(well.Id.ToString()).Units;
                    WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testDataDTO));

                    WellTestDTO[] allTunedAgainTests = WellTestDataService.GetAllValidWellTestByWellId(well.Id.ToString());

                    WellTestDTO latestTestData_ESP = WellTestDataService.GetLatestWellTestDataByWellId(well.Id.ToString());

                    Assert.AreEqual("TUNING_SUCCEEDED", latestTestData_ESP.Status.ToString(), "Well Test Status is not Success");
                    //Get fluid properties based on fluid type
                    ReservoirFluidPropertiesAndUnitsDTO GetFluidPropertiesAndUnits = WellAllocationService.GetFluidPropertiesForReservoirWorkFlow(well.Id.ToString());
                    ReservoirFluidPropertiesDTO GetFluidProperties = GetFluidPropertiesAndUnits.Value;
                    Assert.IsNull(GetFluidProperties.VaporizedCGR, "Vaporized CGR is not null for Black oil fluid type");
                    Assert.IsTrue(GetFluidProperties.FromWellTest);
                    Assert.IsNotNull(GetFluidProperties.GasFormationVolumeFactor, "GasFormationVolumeFactor is null for Black oil fluid type");
                    Assert.AreEqual(0.006, Math.Round((double)GetFluidProperties.GasFormationVolumeFactor, 3), "Value is mismatched for GasFormationVolumeFactor");
                    Assert.IsNotNull(GetFluidProperties.OilFormationVolumeFactor, "OilFormationVolumeFactor is null for Black oil fluid type");
                    Assert.AreEqual(1.246, Math.Round((double)GetFluidProperties.OilFormationVolumeFactor, 3), "Value is mismatched for OilFormationVolumeFactor");
                    Assert.IsNotNull(GetFluidProperties.GasFormationVolumeFactor, "GasFormationVolumeFactor is null for Black oil fluid type");
                    Assert.IsNotNull(GetFluidProperties.PhaseRatio, "Phase Ratio is null for Black Oil fluid type");
                    //Phase ration should come from well test when model options are set
                    Assert.AreEqual(500, GetFluidProperties.PhaseRatio, "Phase Ratio value is mismatched");
                    Assert.IsNotNull(GetFluidProperties.ReservoirPressure, "ReservoirPressure is null for Black oil fluid type");
                    Assert.AreEqual(3072.0, Math.Round((double)GetFluidProperties.ReservoirPressure, 3), "ReservoirPressure value is mismatched");
                    Assert.IsNotNull(GetFluidProperties.ReservoirTemperature, "ReservoirTemperature is null for Black oil fluid type");
                    Assert.AreEqual(200, GetFluidProperties.ReservoirTemperature, "ReservoirTemperature value is mismatched");
                    Assert.IsNotNull(GetFluidProperties.SaturationPressure, "  SaturationPressure is null for Black oil fluid type");
                    Assert.AreEqual(2816.901, Math.Round((double)GetFluidProperties.SaturationPressure, 3), "  SaturationPressure value is mismatched");
                    Assert.IsNotNull(GetFluidProperties.SolutionGOR, "SolutionGOR is null for Black Oil fluid type");
                    Assert.AreEqual(500, GetFluidProperties.SolutionGOR, "SolutionGOR value is mismatched");
                    Assert.IsNotNull(GetFluidProperties.WaterFormationVolumeFactor, "WaterFormationVolumeFactor is null for Black Oil fluid type");
                    Assert.AreEqual(1.034, Math.Round((double)GetFluidProperties.WaterFormationVolumeFactor, 3), "WaterFormationVolumeFactor value is mismatched");
                }

                #endregion ESP

                #region WaterInjection

                else if (wellType == WellTypeId.WInj)
                {
                    //add new wellTestData
                    WellTestDTO testDataDTO = new WellTestDTO()
                    {
                        WellId = well.Id,
                        SPTCode = 0,
                        SPTCodeDescription = "AllocatableTest",
                        AverageTubingPressure = (decimal)3514.7,
                        AverageTubingTemperature = 65,
                        PumpIntakePressure = 10000,
                        PumpDischargePressure = 11000,
                        GaugePressure = 12000,
                        Water = (decimal)6932.80,
                        ChokeSize = 60,
                        FlowLinePressure = 50,
                    };

                    testDataDTO.SampleDate = (DateTime.Today.ToUniversalTime());
                    // add wellTest with new non RRL properties
                    WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(well.Id.ToString()).Units;
                    WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testDataDTO));

                    WellTestDTO[] allTunedAgainTests = WellTestDataService.GetAllValidWellTestByWellId(well.Id.ToString());

                    WellTestDTO latestTestData_WI = WellTestDataService.GetLatestWellTestDataByWellId(well.Id.ToString());

                    Assert.AreEqual("TUNING_SUCCEEDED", latestTestData_WI.Status.ToString(), "Well Test Status is not Success");
                    //Get fluid Properties
                    ReservoirFluidPropertiesAndUnitsDTO GetFluidPropertiesAndUnits = WellAllocationService.GetFluidPropertiesForReservoirWorkFlow(well.Id.ToString());
                    ReservoirFluidPropertiesDTO GetFluidProperties = GetFluidPropertiesAndUnits.Value;

                    Assert.IsNull(GetFluidProperties.VaporizedCGR, "Vaporized CGR is not null for Black oil fluid type");
                    Assert.IsTrue(GetFluidProperties.FromWellTest);
                    Assert.IsNull(GetFluidProperties.GasFormationVolumeFactor, "GasFormationVolumeFactor is null for Black oil fluid type");
                    // Assert.AreEqual(0.006, Math.Round((double)GetFluidProperties.GasFormationVolumeFactor, 3), "Value is mismatched for GasFormationVolumeFactor");
                    Assert.IsNull(GetFluidProperties.OilFormationVolumeFactor, "OilFormationVolumeFactor is null for Black oil fluid type");
                    //Assert.AreEqual(1.246, Math.Round((double)GetFluidProperties.OilFormationVolumeFactor, 3), "Value is mismatched for OilFormationVolumeFactor");
                    Assert.IsNull(GetFluidProperties.GasFormationVolumeFactor, "GasFormationVolumeFactor is null for Black oil fluid type");
                    Assert.IsNull(GetFluidProperties.PhaseRatio, "Phase Ratio is not null for Black Oil fluid type");
                    Assert.IsNull(GetFluidProperties.VaporizedCGR, "VaporizedCGR is not null for Black Oil fluid type");
                    Assert.IsNotNull(GetFluidProperties.ReservoirPressure, "ReservoirPressure is null for Black oil fluid type");
                    Assert.AreEqual(6500, Math.Round((double)GetFluidProperties.ReservoirPressure, 3), "ReservoirPressure value is mismatched");
                    Assert.IsNotNull(GetFluidProperties.ReservoirTemperature, "ReservoirTemperature is null for Black oil fluid type");
                    Assert.AreEqual(60, GetFluidProperties.ReservoirTemperature, "ReservoirTemperature value is mismatched");
                    Assert.IsNull(GetFluidProperties.SaturationPressure, " SaturationPressure is null for Black oil fluid type");
                    //Assert.AreEqual(2816.901, Math.Round((double)GetFluidProperties.SaturationPressure, 3), "  SaturationPressure value is mismatched");
                    Assert.IsNull(GetFluidProperties.SolutionGOR, "SolutionGOR is null for Black Oil fluid type");
                    //Assert.AreEqual(500, GetFluidProperties.SolutionGOR, "SolutionGOR value is mismatched");
                    Assert.IsNotNull(GetFluidProperties.WaterFormationVolumeFactor, "WaterFormationVolumeFactor is null for Black Oil fluid type");
                    Assert.AreEqual(1, Math.Round((double)GetFluidProperties.WaterFormationVolumeFactor, 3), "WaterFormationVolumeFactor value is mismatched");
                }

                #endregion WaterInjection

                #region GasLift

                else if (wellType == WellTypeId.GLift)
                {
                    //add new wellTestData
                    WellTestDTO testDataDTO = new WellTestDTO()
                    {
                        WellId = well.Id,
                        SPTCode = 0,
                        SPTCodeDescription = "AllocatableTest",
                        AverageTubingPressure = 96,
                        AverageTubingTemperature = 100,
                        AverageCasingPressure = 1000,
                        GasInjectionRate = 1000,
                        FlowLinePressure = 50,
                        SeparatorPressure = 30,
                        GaugePressure = 12000,
                        Oil = (decimal)599.00,
                        Gas = 1020,
                        Water = (decimal)732.2,
                        ChokeSize = 50,
                    };

                    testDataDTO.SampleDate = (DateTime.Today.ToUniversalTime());
                    WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(well.Id.ToString()).Units;
                    WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testDataDTO));

                    WellTestDTO[] allTunedAgainTests = WellTestDataService.GetAllValidWellTestByWellId(well.Id.ToString());

                    WellTestDTO latestTestData_GL = WellTestDataService.GetLatestWellTestDataByWellId(well.Id.ToString());

                    Assert.AreEqual("TUNING_SUCCEEDED", latestTestData_GL.Status.ToString(), "Well Test Status is not Success");
                    //Get fluid Properties
                    ReservoirFluidPropertiesAndUnitsDTO GetFluidPropertiesAndUnits = WellAllocationService.GetFluidPropertiesForReservoirWorkFlow(well.Id.ToString());
                    ReservoirFluidPropertiesDTO GetFluidProperties = GetFluidPropertiesAndUnits.Value;
                    Assert.IsNull(GetFluidProperties.VaporizedCGR, "Vaporized CGR is not null for Black oil fluid type");
                    Assert.IsTrue(GetFluidProperties.FromWellTest);
                    Assert.IsNotNull(GetFluidProperties.GasFormationVolumeFactor, "GasFormationVolumeFactor is null for Black oil fluid type");
                    Assert.AreEqual(0.006, Math.Round((double)GetFluidProperties.GasFormationVolumeFactor, 3), "Value is mismatched for GasFormationVolumeFactor");
                    Assert.IsNotNull(GetFluidProperties.OilFormationVolumeFactor, "OilFormationVolumeFactor is null for Black oil fluid type");
                    Assert.AreEqual(1.468, Math.Round((double)GetFluidProperties.OilFormationVolumeFactor, 3), "Value is mismatched for OilFormationVolumeFactor");
                    Assert.IsNotNull(GetFluidProperties.GasFormationVolumeFactor, "GasFormationVolumeFactor is null for Black oil fluid type");
                    Assert.IsNotNull(GetFluidProperties.PhaseRatio, "Phase Ratio is null for Black Oil fluid type");
                    //Phase ration value should come from well test when model options are set
                    Assert.AreEqual(1702, Math.Round((double)GetFluidProperties.PhaseRatio, 3), "Phase Ratio value is mismatched");
                    Assert.IsNotNull(GetFluidProperties.ReservoirPressure, "ReservoirPressure is null for Black oil fluid type");
                    Assert.AreEqual(3000, Math.Round((double)GetFluidProperties.ReservoirPressure, 3), "ReservoirPressure value is mismatched");
                    Assert.IsNotNull(GetFluidProperties.ReservoirTemperature, "ReservoirTemperature is null for Black oil fluid type");
                    Assert.AreEqual(246.9, Math.Round((double)GetFluidProperties.ReservoirTemperature, 3), "ReservoirTemperature value is mismatched");
                    Assert.IsNotNull(GetFluidProperties.SaturationPressure, "  SaturationPressure is null for Black oil fluid type");
                    Assert.AreEqual(5520.362, Math.Round((double)GetFluidProperties.SaturationPressure, 3), "  SaturationPressure value is mismatched");
                    Assert.IsNotNull(GetFluidProperties.SolutionGOR, "SolutionGOR is null for Black Oil fluid type");
                    Assert.AreEqual(741.424, Math.Round((double)GetFluidProperties.SolutionGOR, 3), "SolutionGOR value is mismatched");
                    Assert.IsNotNull(GetFluidProperties.WaterFormationVolumeFactor, "WaterFormationVolumeFactor is null for Black Oil fluid type");
                    Assert.AreEqual(1.054, Math.Round((double)GetFluidProperties.WaterFormationVolumeFactor, 3), "WaterFormationVolumeFactor value is mismatched");
                }

                #endregion GasLift

                #region GasInjection

                else if (wellType == WellTypeId.GInj)
                {
                    //add new wellTestData
                    WellTestDTO testDataDTO = new WellTestDTO()
                    {
                        WellId = well.Id,
                        SPTCodeDescription = "AllocatableTest",
                        //AverageTubingPressure = random.Next(500, 1900),
                        AverageTubingPressure = (decimal)1514.70,
                        AverageTubingTemperature = (decimal)85.0,
                        //AverageTubingTemperature = random.Next(50, 100),
                        GaugePressure = random.Next(500, 1900),
                        //Gas = random.Next(500, 1900),
                        Gas = (decimal)250.0,
                        FlowLinePressure = random.Next(50, 100)
                    };
                    testDataDTO.SampleDate = (DateTime.Today.ToUniversalTime());
                    WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(well.Id.ToString()).Units;
                    WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testDataDTO));

                    //AddWellSettingWithDoubleValues(well.Id, "Min L Factor Acceptance Limit", 0.1);
                    //AddWellSettingWithDoubleValues(well.Id, "Max L Factor Acceptance Limit", 2.0);

                    WellTestDTO[] allTunedAgainTests = WellTestDataService.GetAllValidWellTestByWellId(well.Id.ToString());

                    WellTestDTO latestTestData_NF = WellTestDataService.GetLatestWellTestDataByWellId(well.Id.ToString());

                    Assert.AreEqual("TUNING_SUCCEEDED", latestTestData_NF.Status.ToString(), "Well Test Status is not Success");
                    //Get fluid Properties
                    ReservoirFluidPropertiesAndUnitsDTO GetFluidPropertiesAndUnits = WellAllocationService.GetFluidPropertiesForReservoirWorkFlow(well.Id.ToString());
                    ReservoirFluidPropertiesDTO GetFluidProperties = GetFluidPropertiesAndUnits.Value;

                    Assert.IsNull(GetFluidProperties.VaporizedCGR, "Vaporized CGR is not null for Black oil fluid type");
                    Assert.IsTrue(GetFluidProperties.FromWellTest);
                    Assert.IsNotNull(GetFluidProperties.GasFormationVolumeFactor, "GasFormationVolumeFactor is null for Black oil fluid type");
                    Assert.AreEqual(0.017, Math.Round((double)GetFluidProperties.GasFormationVolumeFactor, 3), "Value is mismatched for GasFormationVolumeFactor");
                    Assert.IsNull(GetFluidProperties.OilFormationVolumeFactor, "OilFormationVolumeFactor is not null for Black oil fluid type");
                    Assert.IsNull(GetFluidProperties.PhaseRatio, "Phase Ratio is not null for Black Oil fluid type");
                    Assert.IsNotNull(GetFluidProperties.ReservoirPressure, "ReservoirPressure is null for Black oil fluid type");
                    Assert.AreEqual(1014.696, Math.Round((double)GetFluidProperties.ReservoirPressure, 3), "ReservoirPressure value is mismatched");
                    Assert.IsNotNull(GetFluidProperties.ReservoirTemperature, "ReservoirTemperature is null for Black oil fluid type");
                    Assert.AreEqual(190, GetFluidProperties.ReservoirTemperature, "ReservoirTemperature value is mismatched");
                    Assert.IsNull(GetFluidProperties.SaturationPressure, "  SaturationPressure is not null for Black oil fluid type");
                    Assert.IsNull(GetFluidProperties.SolutionGOR, "SolutionGOR is not null for Black Oil fluid type");
                    Assert.IsNull(GetFluidProperties.WaterFormationVolumeFactor, "WaterFormationVolumeFactor is not null for Black Oil fluid type");

                }
                #endregion GasInjection

                #region NF

                else if (wellType == WellTypeId.NF)
                {
                    //add new wellTestData
                    WellTestDTO testDataDTO = new WellTestDTO()
                    {
                        WellId = well.Id,
                        SPTCode = 2,
                        SPTCodeDescription = "RepresentativeTest",
                        AverageTubingPressure = (decimal)164.7,
                        AverageTubingTemperature = 80,
                        //GaugePressure = 12000,
                        Oil = (decimal)1768.00,
                        Gas = 880,
                        Water = (decimal)589.30,
                        ChokeSize = 50,
                        FlowLinePressure = 50,
                        SeparatorPressure = 10000,
                    };

                    testDataDTO.SampleDate = (DateTime.Today.ToUniversalTime());
                    WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(well.Id.ToString()).Units;
                    WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testDataDTO));

                    WellTestDTO[] allTunedAgainTests = WellTestDataService.GetAllValidWellTestByWellId(well.Id.ToString());

                    WellTestDTO latestTestData_NF = WellTestDataService.GetLatestWellTestDataByWellId(well.Id.ToString());

                    Assert.AreEqual("TUNING_SUCCEEDED", latestTestData_NF.Status.ToString(), "Well Test Status is not Success");
                    //Get fluid properties
                    ReservoirFluidPropertiesAndUnitsDTO GetFluidPropertiesAndUnits = WellAllocationService.GetFluidPropertiesForReservoirWorkFlow(well.Id.ToString());
                    ReservoirFluidPropertiesDTO GetFluidProperties = GetFluidPropertiesAndUnits.Value;

                    Assert.IsNull(GetFluidProperties.VaporizedCGR, "Vaporized CGR is not null for Black oil fluid type");
                    Assert.IsFalse(GetFluidProperties.FromWellTest);
                    Assert.IsNotNull(GetFluidProperties.GasFormationVolumeFactor, "GasFormationVolumeFactor is null for Black oil fluid type");
                    Assert.AreEqual(0.003, Math.Round((double)GetFluidProperties.GasFormationVolumeFactor, 3), "Value is mismatched for GasFormationVolumeFactor");
                    Assert.IsNotNull(GetFluidProperties.OilFormationVolumeFactor, "OilFormationVolumeFactor is null for Black oil fluid type");
                    Assert.AreEqual(1.197, Math.Round((double)GetFluidProperties.OilFormationVolumeFactor, 3), "Value is mismatched for OilFormationVolumeFactor");
                    Assert.IsNotNull(GetFluidProperties.GasFormationVolumeFactor, "GasFormationVolumeFactor is null for Black oil fluid type");
                    Assert.IsNotNull(GetFluidProperties.PhaseRatio, "Phase Ratio is null for Black Oil fluid type");
                    Assert.AreEqual(500, GetFluidProperties.PhaseRatio, "Phase Ratio value is mismatched");
                    Assert.IsNotNull(GetFluidProperties.ReservoirPressure, "ReservoirPressure is null for Black oil fluid type");
                    Assert.AreEqual(6000, Math.Round((double)GetFluidProperties.ReservoirPressure, 3), "ReservoirPressure value is mismatched");
                    Assert.IsNotNull(GetFluidProperties.ReservoirTemperature, "ReservoirTemperature is null for Black oil fluid type");
                    Assert.AreEqual(196, GetFluidProperties.ReservoirTemperature, "ReservoirTemperature value is mismatched");
                    Assert.IsNotNull(GetFluidProperties.SaturationPressure, "  SaturationPressure is null for Black oil fluid type");
                    Assert.AreEqual(2804.407, Math.Round((double)GetFluidProperties.SaturationPressure, 3), "  SaturationPressure value is mismatched");
                    Assert.IsNotNull(GetFluidProperties.SolutionGOR, "SolutionGOR is null for Black Oil fluid type");
                    Assert.AreEqual(500, GetFluidProperties.SolutionGOR, "SolutionGOR value is mismatched");
                    Assert.IsNotNull(GetFluidProperties.WaterFormationVolumeFactor, "WaterFormationVolumeFactor is null for Black Oil fluid type");
                    Assert.AreEqual(1.029, Math.Round((double)GetFluidProperties.WaterFormationVolumeFactor, 3), "WaterFormationVolumeFactor value is mismatched");
                }
                #endregion NF

                #region PCP

                if (wellType == WellTypeId.PCP)
                {
                    //add new wellTestData
                    WellTestDTO testDataDTO = new WellTestDTO()
                    {
                        WellId = well.Id,
                        SPTCodeDescription = "Allocatable Test",
                        TestDuration = 24,
                        Oil = (decimal)210.2,
                        Water = (decimal)140.2,
                        AverageTubingPressure = (decimal)100.00,
                        AverageTubingTemperature = (decimal)80.00,
                        PumpSpeed = (decimal)225.00,
                        FlowLinePressure = (decimal)1862.00,
                        ChokeSize = (decimal)64.00,
                        Comment = "PCPWellTest_Comment_Check",
                    };

                    testDataDTO.SampleDate = (DateTime.Today.ToUniversalTime());
                    // add wellTest with new non RRL properties
                    WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(well.Id.ToString()).Units;
                    WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testDataDTO));

                    WellTestDTO[] allTunedAgainTests = WellTestDataService.GetAllValidWellTestByWellId(well.Id.ToString());

                    WellTestDTO latestTestData_PCP = WellTestDataService.GetLatestWellTestDataByWellId(well.Id.ToString());

                    Assert.AreEqual("TUNING_SUCCEEDED", latestTestData_PCP.Status.ToString(), "Well Test Status is not Success");
                    //Get fluid properties
                    ReservoirFluidPropertiesAndUnitsDTO GetFluidPropertiesAndUnits = WellAllocationService.GetFluidPropertiesForReservoirWorkFlow(well.Id.ToString());
                    ReservoirFluidPropertiesDTO GetFluidProperties = GetFluidPropertiesAndUnits.Value;

                    Assert.IsNull(GetFluidProperties.VaporizedCGR, "Vaporized CGR is not null for Black oil fluid type");
                    Assert.IsTrue(GetFluidProperties.FromWellTest);
                    Assert.IsNotNull(GetFluidProperties.GasFormationVolumeFactor, "GasFormationVolumeFactor is null for Black oil fluid type");
                    Assert.AreEqual(0.008, Math.Round((double)GetFluidProperties.GasFormationVolumeFactor, 3), "Value is mismatched for GasFormationVolumeFactor");
                    Assert.IsNotNull(GetFluidProperties.OilFormationVolumeFactor, "OilFormationVolumeFactor is null for Black oil fluid type");
                    Assert.AreEqual(1.02, Math.Round((double)GetFluidProperties.OilFormationVolumeFactor, 3), "Value is mismatched for OilFormationVolumeFactor");
                    Assert.IsNotNull(GetFluidProperties.GasFormationVolumeFactor, "GasFormationVolumeFactor is null for Black oil fluid type");
                    Assert.IsNotNull(GetFluidProperties.PhaseRatio, "Phase Ratio is null for Black Oil fluid type");
                    Assert.AreEqual(0, GetFluidProperties.PhaseRatio, "Phase Ratio value is mismatched");
                    Assert.IsNotNull(GetFluidProperties.ReservoirPressure, "ReservoirPressure is null for Black oil fluid type");
                    Assert.AreEqual(1700, Math.Round((double)GetFluidProperties.ReservoirPressure, 3), "ReservoirPressure value is mismatched");
                    Assert.IsNotNull(GetFluidProperties.ReservoirTemperature, "ReservoirTemperature is null for Black oil fluid type");
                    Assert.AreEqual(140, GetFluidProperties.ReservoirTemperature, "ReservoirTemperature value is mismatched");
                    Assert.IsNotNull(GetFluidProperties.SaturationPressure, "  SaturationPressure is null for Black oil fluid type");
                    Assert.AreEqual(0, Math.Round((double)GetFluidProperties.SaturationPressure, 3), "  SaturationPressure value is mismatched");
                    Assert.IsNotNull(GetFluidProperties.SolutionGOR, "SolutionGOR is null for Black Oil fluid type");
                    Assert.AreEqual(0, GetFluidProperties.SolutionGOR, "SolutionGOR value is mismatched");
                    Assert.IsNotNull(GetFluidProperties.WaterFormationVolumeFactor, "WaterFormationVolumeFactor is null for Black Oil fluid type");
                    Assert.AreEqual(1.014, Math.Round((double)GetFluidProperties.WaterFormationVolumeFactor, 3), "WaterFormationVolumeFactor value is mismatched");
                }
            }
            #endregion PCP

        }

        [TestCategory(TestCategories.WellAllocationServiceTests), TestMethod]
        public void GetFluidProperties_NFCondensate()
        {
            var modelFileName = "Condensate Gas - IPR Auto Tuning.wflx";
            var wellType = WellTypeId.NF;
            var options = new ModelFileOptionDTO
            {
                CalibrationMethod = CalibrationMethodId.DarcyFlowCoefficient,
                OptionalUpdate = new long[]
                    {
                        (long) OptionalUpdates.UpdateGOR_CGR
                    }
            };

            Trace.WriteLine("Testing model: " + modelFileName);

            WellDTO well = AddNonRRLWellGeneralTab("NFWWELL_", wellType, WellFluidType.Condensate, "N/A", "NA");
            AddNonRRLModelFile(well, modelFileName, options.CalibrationMethod, options.OptionalUpdate);
            Trace.WriteLine("Well Added Successfully");

            WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(well.Id.ToString()).Units;
            WellTestDTO testData = new WellTestDTO
            {
                WellId = well.Id,
                SPTCodeDescription = "AllocatableTest",
                CalibrationMethod = options.CalibrationMethod,
                WellTestType = WellTestType.WellTest,
                AverageTubingPressure = 4000,
                AverageTubingTemperature = 100,
                Gas = 35520,
                Water = 5327.6m,
                Oil = 5500.6m,
                ChokeSize = 32,
                FlowLinePressure = 4000,
                SeparatorPressure = 4000,
                TestDuration = 24,
                SampleDate = DateTime.Today.AddDays(-5).ToUniversalTime()
            };

            WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testData));
            WellTestDTO[] allTunedAgainTests = WellTestDataService.GetAllValidWellTestByWellId(well.Id.ToString());

            WellTestDTO latestTestData_NF = WellTestDataService.GetLatestWellTestDataByWellId(well.Id.ToString());

            Assert.AreEqual("TUNING_SUCCEEDED", latestTestData_NF.Status.ToString(), "Well Test Status is not Success");
            //Get fluid properties
            ReservoirFluidPropertiesAndUnitsDTO GetFluidPropertiesAndUnits = WellAllocationService.GetFluidPropertiesForReservoirWorkFlow(well.Id.ToString());
            ReservoirFluidPropertiesDTO GetFluidProperties = GetFluidPropertiesAndUnits.Value;

            Assert.IsNotNull(GetFluidProperties.VaporizedCGR, "Vaporized CGR is null for Condensate fluid type");
            Assert.IsTrue(GetFluidProperties.FromWellTest);
            Assert.IsNotNull(GetFluidProperties.GasFormationVolumeFactor, "GasFormationVolumeFactor is null for Condensate fluid type");
            Assert.AreEqual(0.004, Math.Round((double)GetFluidProperties.GasFormationVolumeFactor, 3), "Value is mismatched for GasFormationVolumeFactor");
            Assert.IsNotNull(GetFluidProperties.OilFormationVolumeFactor, "OilFormationVolumeFactor is null for Condensate fluid type");
            Assert.AreEqual(2.217, Math.Round((double)GetFluidProperties.OilFormationVolumeFactor, 3), "Value is mismatched for OilFormationVolumeFactor");
            Assert.IsNotNull(GetFluidProperties.PhaseRatio, "Phase Ratio is null for for Condensate fluid type");
            //Phase ratio value should come from well test when model options are set
            Assert.AreEqual(154.859, Math.Round((double)GetFluidProperties.PhaseRatio, 3), "Phase Ratio value is mismatched");
            Assert.AreEqual(154.859, Math.Round((double)GetFluidProperties.VaporizedCGR, 3), "VaporizedCGR value is mismatched");
            Assert.IsNotNull(GetFluidProperties.ReservoirPressure, "ReservoirPressure is null for Condensate fluid type");
            Assert.AreEqual(6050, Math.Round((double)GetFluidProperties.ReservoirPressure, 3), "ReservoirPressure value is mismatched");
            Assert.IsNotNull(GetFluidProperties.ReservoirTemperature, "ReservoirTemperature is null for Condensate fluid type");
            Assert.AreEqual(230, GetFluidProperties.ReservoirTemperature, "ReservoirTemperature value is mismatched");
            Assert.IsNotNull(GetFluidProperties.SaturationPressure, "  SaturationPressure is null for Condensate fluid type");
            Assert.AreEqual(5360.567, Math.Round((double)GetFluidProperties.SaturationPressure, 3), "  SaturationPressure value is mismatched");
            Assert.IsNotNull(GetFluidProperties.SolutionGOR, "SolutionGOR is null for Condensate fluid type");
            Assert.AreEqual(0, GetFluidProperties.SolutionGOR, "SolutionGOR value is mismatched");
            Assert.IsNotNull(GetFluidProperties.WaterFormationVolumeFactor, "WaterFormationVolumeFactor is null for Condensate fluid type");
            Assert.AreEqual(1.042, Math.Round((double)GetFluidProperties.WaterFormationVolumeFactor, 3), "WaterFormationVolumeFactor value is mismatched");
        }
      
        [TestCategory(TestCategories.WellAllocationServiceTests), TestMethod]
        public void GetFluidProperties_NFWellDryGasFluidType()
        {
            var modelFileName = "Dry Gas - IPR Auto Tuning.wflx";
            WellTypeId wellType = WellTypeId.NF;
            var options = new ModelFileOptionDTO
            {
                CalibrationMethod = CalibrationMethodId.DarcyFlowCoefficient,
                OptionalUpdate = new long[]
                    {
                        (long) OptionalUpdates.UpdateWCT_WGR
                    }
            };

            Trace.WriteLine("Testing model: " + modelFileName);

            WellDTO well = AddNonRRLWellGeneralTab("NFWWELL_", wellType, WellFluidType.DryGas, "N/A", "NA");
            AddNonRRLModelFile(well, modelFileName, options.CalibrationMethod, options.OptionalUpdate);
            Trace.WriteLine("Well Added Successfully");

            WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(well.Id.ToString()).Units;
            WellTestDTO testData = new WellTestDTO
            {
                WellId = well.Id,
                SPTCodeDescription = "RepresentativeTest",
                CalibrationMethod = options.CalibrationMethod,
                WellTestType = WellTestType.WellTest,
                AverageTubingPressure = 600,
                AverageTubingTemperature = 100,
                Gas = 1820,
                Water = 610,
                ChokeSize = 32,
                FlowLinePressure = 600,
                SeparatorPressure = 600,
                TestDuration = 12,
                SampleDate = DateTime.Today.AddDays(-5).ToUniversalTime()
            };

            WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testData));
            WellTestDTO[] allTunedAgainTests = WellTestDataService.GetAllValidWellTestByWellId(well.Id.ToString());

            WellTestDTO latestTestData_NF = WellTestDataService.GetLatestWellTestDataByWellId(well.Id.ToString());

            Assert.AreEqual("TUNING_SUCCEEDED", latestTestData_NF.Status.ToString(), "Well Test Status is not Success");
            //Get fluid properties
            ReservoirFluidPropertiesAndUnitsDTO GetFluidPropertiesAndUnits = WellAllocationService.GetFluidPropertiesForReservoirWorkFlow(well.Id.ToString());
            ReservoirFluidPropertiesDTO GetFluidProperties = GetFluidPropertiesAndUnits.Value;

            Assert.IsNull(GetFluidProperties.VaporizedCGR, "Vaporized CGR is not null for Dry Gas fluid type");
            Assert.IsTrue(GetFluidProperties.FromWellTest);
            Assert.IsNotNull(GetFluidProperties.GasFormationVolumeFactor, "GasFormationVolumeFactor is null for Dry Gas fluid type");
            Assert.AreEqual(0.004, Math.Round((double)GetFluidProperties.GasFormationVolumeFactor, 3), "Value is mismatched for GasFormationVolumeFactor");
            Assert.IsNull(GetFluidProperties.OilFormationVolumeFactor, "OilFormationVolumeFactor is null for Dry Gas fluid type");
            Assert.IsNull(GetFluidProperties.PhaseRatio, "Phase Ratio is null for Dry Gas fluid type");
            Assert.IsNotNull(GetFluidProperties.ReservoirPressure, "ReservoirPressure is null for Dry Gas fluid type");
            Assert.AreEqual(4010, Math.Round((double)GetFluidProperties.ReservoirPressure, 3), "ReservoirPressure value is mismatched");
            Assert.IsNotNull(GetFluidProperties.ReservoirTemperature, "ReservoirTemperature is null for Dry Gas fluid type");
            Assert.AreEqual(200, GetFluidProperties.ReservoirTemperature, "ReservoirTemperature value is mismatched");
            Assert.IsNull(GetFluidProperties.SaturationPressure, "  SaturationPressure is null for Dry Gas fluid type");
            Assert.IsNull(GetFluidProperties.SolutionGOR, "SolutionGOR is null for Dry Gas fluid type");
            Assert.IsNotNull(GetFluidProperties.WaterFormationVolumeFactor, "WaterFormationVolumeFactor is null for Dry Gas fluid type");
            Assert.AreEqual(1.032, Math.Round((double)GetFluidProperties.WaterFormationVolumeFactor, 3), "WaterFormationVolumeFactor value is mismatched");

        }

        /// <summary>
        /// FRWM-7078 Voidage : Add targets and target bounds to Reservoir, Zone and Pattern
        /// Configure Voidage targets in DB. 
        /// Verify that, All the values are correctly saved by reading back from DB.
        /// </summary>
        [TestCategory(TestCategories.WellAllocationServiceTests), TestMethod]
        public void Configure_VRR_Targets_4_Reservoirs_Zones_Patterns()
        {
            List<AssetDTO> assetIds = null;
            try
            {
                // Create an asset
                assetIds = CreateAsset(1);

                // Create Well Info Tuple
                Tuple<string, WellTypeId, WellFluidType, WellFluidPhase>[] wellinfo =
                {
                    Tuple.Create("ESPWell_",WellTypeId.ESP,WellFluidType.None,WellFluidPhase.None),
                    Tuple.Create("GASInjWell_",WellTypeId.GInj,WellFluidType.None,WellFluidPhase.None)
                };

                // Add Well and it's model file in system
                foreach (Tuple<string, WellTypeId, WellFluidType, WellFluidPhase> wi in wellinfo)
                {
                    string wellname = wi.Item1 + 1;
                    string facilityname = GetFacilityId(wi.Item1, 1);

                    WellDTO well = AddNonRRLWellGeneral(wellname, facilityname, wi.Item2, wi.Item3, wi.Item4, "1", assetIds.ElementAt(0).Id);
                    AddModelFile(well.CommissionDate.Value, wi.Item2, well.Id, CalibrationMethodId.LFactor);
                }

                var allWells = WellService.GetAllWells().ToList();
                Assert.IsNotNull(allWells);

                // Add Reservoir
                ReservoirAndUnitsDTO addReservoir1 = AddReservoirToAsset("Res", assetIds.ElementAt(0).Id);
                // GetReservoir IDs
                ReservoirArrayAndUnitsDTO getReservoirByAsset = WellAllocationService.GetReservoirsByAssetId(assetIds.ElementAt(0).Id.ToString());

                // Add Zone
                ZoneAndUnitsDTO addZone1 = AddZoneToReservoir("Zone", getReservoirByAsset.Values[0].Id);
                // Get Zone IDs based on reservoir
                ZoneArrayAndUnitsDTO getZoneByReservoir = WellAllocationService.GetZonesByReservoirId(getReservoirByAsset.Values[0].Id.ToString());
                // Assign Well to Zone
                WellToZoneDTO assignWellToZone = AddWellToZone(allWells[0].Id, getZoneByReservoir.Values[0].Id);
                Assert.IsNotNull(assignWellToZone);

                // Add Pattern
                PatternDTO addPattern1 = AddPatternToZoneOfReservoir("Pattern", getZoneByReservoir.Values[0].Id);
                // Get Pattern Ids based on reservoir            
                List<PatternDTO> getPatternByZoneOfReservoir = WellAllocationService.GetPatternsByZoneId(getZoneByReservoir.Values[0].Id.ToString());
                // Assign Well to Pattern
                WellToPatternDTO assignWellToPattern = AddWellToPattern(allWells[1].Id, getPatternByZoneOfReservoir.ElementAt(0).Id);
                Assert.IsNotNull(assignWellToPattern);

                #region Configure the Targets for Reservoir / Zone / Pattern
                Trace.WriteLine("Configure the Targets for Reservoir / Zone / Pattern");

                // Reservoir Value DTO
                SubsurfaceEntityTargetBoundDTO reservoirTargetBoundValueDTO = new SubsurfaceEntityTargetBoundDTO
                {
                    ReservoirId = getReservoirByAsset.Values[0].Id,
                    ZoneId = null,
                    PatternId = null,
                    StartDate = DateTime.Today.AddDays(-2).ToLocalTime(),
                    EndDate = null,
                    BoundPreference = SubsurfaceEntityTargetType.FixedValue,
                    TargetValue = 100,
                    FixedLowerBoundTolerance = 10,
                    FixedUpperBoundTolerance = 100,
                    PercentageLowerBoundTolerance = null,
                    PercentageUpperBoundTolerance = null
                };
                // Get Units for Reservoir Target 
                SubsurfaceEntityTargetBoundAndUnitsDTO addReservoirTargets = WellAllocationService.GetSubsurfaceEntityByEntityId(getReservoirByAsset.Values[0].Id.ToString(), SubsurfaceResultEntityType.Reservoir.ToString(), getReservoirByAsset.Values[0].ApplicableDate.ToISO8601Date());
                addReservoirTargets.Value = reservoirTargetBoundValueDTO;
                // Add Targets for Reservoir
                WellAllocationService.AddOrUpdateReservoirTargetBound(addReservoirTargets);
                Trace.WriteLine("Added Targets for Reservoir");

                // Verify that all the added Targets for Reservoir
                SubsurfaceEntityTargetBoundAndUnitsDTO verifyReservoirTargets = WellAllocationService.GetSubsurfaceEntityByEntityId(getReservoirByAsset.Values[0].Id.ToString(), SubsurfaceResultEntityType.Reservoir.ToString(), getReservoirByAsset.Values[0].ApplicableDate.ToISO8601Date());
                Assert.IsNotNull(verifyReservoirTargets);
                VerifyTargetResults(verifyReservoirTargets, addReservoirTargets, SubsurfaceResultEntityType.Reservoir.ToString());

                // Zone Value DTO
                SubsurfaceEntityTargetBoundDTO zoneTargetBoundValueDTO = new SubsurfaceEntityTargetBoundDTO
                {
                    ReservoirId = null,
                    ZoneId = getZoneByReservoir.Values[0].Id,
                    PatternId = null,
                    StartDate = DateTime.Today.AddDays(-2).ToLocalTime(),
                    EndDate = null,
                    BoundPreference = SubsurfaceEntityTargetType.PercentageOfTarget,
                    TargetValue = 100,
                    FixedLowerBoundTolerance = null,
                    FixedUpperBoundTolerance = null,
                    PercentageLowerBoundTolerance = 1,
                    PercentageUpperBoundTolerance = 99
                };
                // Get Units for Zone Target 
                SubsurfaceEntityTargetBoundAndUnitsDTO addZoneTargets = WellAllocationService.GetSubsurfaceEntityByEntityId(getZoneByReservoir.Values[0].Id.ToString(), SubsurfaceResultEntityType.Zone.ToString(), getZoneByReservoir.Values[0].ApplicableDate.ToISO8601Date());
                addZoneTargets.Value = zoneTargetBoundValueDTO;
                // Add Targets for Zone
                WellAllocationService.AddOrUpdateZoneTargetBound(addZoneTargets);
                Trace.WriteLine("Added Targets for Zone");
                // Verify that all the added Targets for Zone
                SubsurfaceEntityTargetBoundAndUnitsDTO verifyZoneTargets = WellAllocationService.GetSubsurfaceEntityByEntityId(getZoneByReservoir.Values[0].Id.ToString(), SubsurfaceResultEntityType.Zone.ToString(), getZoneByReservoir.Values[0].ApplicableDate.ToISO8601Date());
                Assert.IsNotNull(verifyZoneTargets);
                VerifyTargetResults(verifyZoneTargets, addZoneTargets, SubsurfaceResultEntityType.Zone.ToString());

                // Pattern Value DTO
                SubsurfaceEntityTargetBoundDTO patternTargetBoundValueDTO = new SubsurfaceEntityTargetBoundDTO
                {
                    ReservoirId = null,
                    ZoneId = null,
                    PatternId = getPatternByZoneOfReservoir.ElementAt(0).Id,
                    StartDate = DateTime.Today.AddDays(-2).ToLocalTime(),
                    EndDate = null,
                    BoundPreference = SubsurfaceEntityTargetType.FixedValue,
                    TargetValue = 100,
                    FixedLowerBoundTolerance = 10,
                    FixedUpperBoundTolerance = 100,
                    PercentageLowerBoundTolerance = null,
                    PercentageUpperBoundTolerance = null
                };
                // Get Units for Pattern Target 
                SubsurfaceEntityTargetBoundAndUnitsDTO addPatternTargets = WellAllocationService.GetSubsurfaceEntityByEntityId(getPatternByZoneOfReservoir.ElementAt(0).Id.ToString(), SubsurfaceResultEntityType.Pattern.ToString(), getZoneByReservoir.Values[0].ApplicableDate.ToISO8601Date());
                addPatternTargets.Value = patternTargetBoundValueDTO;
                // Add Targets for Pattern
                WellAllocationService.AddOrUpdatePatternTargetBound(addPatternTargets);
                Trace.WriteLine("Added Targets for Pattern");

                // Verify that all the added Targets for Pattern
                SubsurfaceEntityTargetBoundAndUnitsDTO verifyPatternTargets = WellAllocationService.GetSubsurfaceEntityByEntityId(getPatternByZoneOfReservoir.ElementAt(0).Id.ToString(), SubsurfaceResultEntityType.Pattern.ToString(), getZoneByReservoir.Values[0].ApplicableDate.ToISO8601Date());
                Assert.IsNotNull(verifyPatternTargets);
                VerifyTargetResults(verifyPatternTargets, addPatternTargets, SubsurfaceResultEntityType.Pattern.ToString());
                #endregion Configure the Targets for Reservoir / Zone / Pattern
            }
            finally
            {
                WellAllocationService.DeleteHierarchyByAssetId(assetIds.ElementAt(0).Id.ToString());
                Trace.WriteLine("Pattern1 pattern deleted successfully");
                Trace.WriteLine("Zone1 zone deleted successfully");
                Trace.WriteLine("Res1 reservoir deleted successfully");
            }
        }

        public void VerifyTargetResults(SubsurfaceEntityTargetBoundAndUnitsDTO expectedObject, SubsurfaceEntityTargetBoundAndUnitsDTO actualObject, string SubsurfaceEntityType)
        {
            Assert.AreEqual(expectedObject.Units.TargetValue.UnitKey, actualObject.Units.TargetValue.UnitKey, "TargetValue Units are not matching");
            Assert.AreEqual(expectedObject.Units.FixedLowerBoundTolerance.UnitKey, actualObject.Units.FixedLowerBoundTolerance.UnitKey, "FixedLowerBoundTolerance Units are not matching");
            Assert.AreEqual(expectedObject.Units.FixedUpperBoundTolerance.UnitKey, actualObject.Units.FixedUpperBoundTolerance.UnitKey, "FixedUpperBoundTolerance Units are not matching");
            Assert.AreEqual(expectedObject.Units.PercentageLowerBoundTolerance.UnitKey, actualObject.Units.PercentageLowerBoundTolerance.UnitKey, "PercentageLowerBoundTolerance Units are not matching");
            Assert.AreEqual(expectedObject.Units.PercentageUpperBoundTolerance.UnitKey, actualObject.Units.PercentageUpperBoundTolerance.UnitKey, "PercentageUpperBoundTolerance Units are not matching");

            CompareObjectsUsingReflection(expectedObject.Value.BoundPreference, actualObject.Value.BoundPreference, "BoundPreference are not matching");
            CompareObjectsUsingReflection(expectedObject.Value.TargetValue, actualObject.Value.TargetValue, "TargetValue are not matching");
            CompareObjectsUsingReflection(expectedObject.Value.FixedLowerBoundTolerance, actualObject.Value.FixedLowerBoundTolerance, "FixedLowerBoundTolerance are not matching");
            CompareObjectsUsingReflection(expectedObject.Value.FixedUpperBoundTolerance, actualObject.Value.FixedUpperBoundTolerance, "FixedUpperBoundTolerance are not matching");
            CompareObjectsUsingReflection(expectedObject.Value.PercentageLowerBoundTolerance, actualObject.Value.PercentageLowerBoundTolerance, "PercentageLowerBoundTolerance are not matching");
            CompareObjectsUsingReflection(expectedObject.Value.PercentageUpperBoundTolerance, actualObject.Value.PercentageUpperBoundTolerance, "PercentageUpperBoundTolerance are not matching");
            Trace.WriteLine("Verified Units and Added Targets for : " + SubsurfaceEntityType);
        }

        /// <summary>
        /// FRWM-191 and FRWM-6413 Instantaneous and Cumulative Voidage Replacement Ratio (VRR) Calculations -- Grid data
        /// FRWM-7409 Voidage: Prompt to initiate calculations
        /// Extended the IT to cover UoM (US and Metric unit system) and new VRR calculation formulas
        /// Calculation of VRR for Reservoir, Zone and Pattern. 
        /// Verify that, All the values are correctly saved during scheduler run and by reading back from DB.
        /// </summary>
        [TestCategory(TestCategories.WellAllocationServiceTests), TestMethod]
        public void Calculation_Of_Instantaneous_Cumulative_VRR_4_Reservoirs_Zones_Patterns()
        {
            if (s_isRunningInATS == false)
            {
                Trace.WriteLine("Test Not executed on Team City due to scheduler constraints");
                return;
            }
            List<AssetDTO> assetIds = null;
            try
            {
                ChangeUnitSystem("US");
                ChangeUnitSystemUserSetting("US");

                // step 1: Create Asset.
                assetIds = CreateAsset(1);

                // step2 : Create 6 Wells 4 NF Condensate and 2 Gas Injection Wells along with required Daily Average values.
                #region 2. Add Wells and Update Welltests and get tunned well tests
                AddADNOCWells(assetIds.ElementAt(0).Id);

                //Update Well test for GP1 to be tuned success;
                WellDTO GP1well = WellService.GetAllWells().FirstOrDefault(w => w.Name == "GP1");
                AddWellSettingWithDoubleValues(GP1well.Id, SettingServiceStringConstants.WATER_GAS_RATIO_MIN_AL, 0.6);
                AddWellSettingWithDoubleValues(GP1well.Id, SettingServiceStringConstants.WATER_GAS_RATIO_MAX_AL, 10);
                AddWellSettingWithDoubleValues(GP1well.Id, SettingServiceStringConstants.CONDENSATE_GAS_RATIO_MIN_AL, 30);
                AddWellSettingWithDoubleValues(GP1well.Id, SettingServiceStringConstants.CONDENSATE_GAS_RATIO_MAX_AL, 300);
                var welltestdata4GP1 = WellTestDataService.GetWellTestDataByWellId(GP1well.Id.ToString());
                WellTestDTO latestwelltest = welltestdata4GP1.Values.First();
                latestwelltest.Oil = 1906;
                latestwelltest.Water = 36;
                latestwelltest.Gas = 49276;
                latestwelltest.AverageTubingPressure = 1566;
                latestwelltest.SPTCodeDescription = "AllocatableTest";
                WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(GP1well.Id.ToString()).Units;
                WellTestDataService.UpdateWellTestData(new WellTestAndUnitsDTO(units, latestwelltest));
                bool tunewell = WellTestDataService.TuneWellTests(GP1well.Id.ToString());
                Assert.IsTrue(tunewell);
                welltestdata4GP1 = WellTestDataService.GetWellTestDataByWellId(GP1well.Id.ToString());
                latestwelltest = welltestdata4GP1.Values.First();
                Assert.AreEqual("Success", latestwelltest.TuningStatus, "Tuning Status was not Correct GP1 ");

                WellDTO GP2well = WellService.GetAllWells().FirstOrDefault(w => w.Name == "GP2");
                AddWellSettingWithDoubleValues(GP2well.Id, SettingServiceStringConstants.WATER_GAS_RATIO_MIN_AL, 0.4);
                AddWellSettingWithDoubleValues(GP2well.Id, SettingServiceStringConstants.WATER_GAS_RATIO_MAX_AL, 10);
                AddWellSettingWithDoubleValues(GP2well.Id, SettingServiceStringConstants.CONDENSATE_GAS_RATIO_MIN_AL, 15);
                AddWellSettingWithDoubleValues(GP2well.Id, SettingServiceStringConstants.CONDENSATE_GAS_RATIO_MAX_AL, 300);
                AddWellSettingWithDoubleValues(GP2well.Id, SettingServiceStringConstants.LFACTOR_MIN_AL, 0.88);
                AddWellSettingWithDoubleValues(GP2well.Id, SettingServiceStringConstants.LFACTOR_MAX_AL, 1.28);
                var welltestdata4GP2 = WellTestDataService.GetWellTestDataByWellId(GP2well.Id.ToString());
                WellTestDTO latestwelltest2 = welltestdata4GP2.Values.First();
                latestwelltest2.Oil = 1020;
                latestwelltest2.Water = 32;
                latestwelltest2.Gas = 61650;
                latestwelltest2.AverageTubingPressure = 2920;
                latestwelltest2.SPTCodeDescription = "AllocatableTest";
                WellTestUnitsDTO units2 = WellTestDataService.GetWellTestDefaults(GP2well.Id.ToString()).Units;
                WellTestDataService.UpdateWellTestData(new WellTestAndUnitsDTO(units2, latestwelltest2));
                bool tunewell2 = WellTestDataService.TuneWellTests(GP2well.Id.ToString());
                Assert.IsTrue(tunewell2);
                welltestdata4GP2 = WellTestDataService.GetWellTestDataByWellId(GP2well.Id.ToString());
                latestwelltest2 = welltestdata4GP2.Values.First();
                Assert.AreEqual("Success", latestwelltest2.TuningStatus, "Tuning Status was not Correct GP2 ");

                WellDTO GP3well = WellService.GetAllWells().FirstOrDefault(w => w.Name == "GP3");
                AddWellSettingWithDoubleValues(GP3well.Id, SettingServiceStringConstants.WATER_GAS_RATIO_MIN_AL, 0.4);
                AddWellSettingWithDoubleValues(GP3well.Id, SettingServiceStringConstants.WATER_GAS_RATIO_MAX_AL, 10);
                AddWellSettingWithDoubleValues(GP3well.Id, SettingServiceStringConstants.CONDENSATE_GAS_RATIO_MIN_AL, 15);
                AddWellSettingWithDoubleValues(GP3well.Id, SettingServiceStringConstants.CONDENSATE_GAS_RATIO_MAX_AL, 300);
                AddWellSettingWithDoubleValues(GP3well.Id, SettingServiceStringConstants.LFACTOR_MIN_AL, 0.88);
                AddWellSettingWithDoubleValues(GP3well.Id, SettingServiceStringConstants.LFACTOR_MAX_AL, 1.28);
                var welltestdata4GP3 = WellTestDataService.GetWellTestDataByWellId(GP3well.Id.ToString());
                WellTestDTO latestwelltest3 = welltestdata4GP3.Values.First();
                latestwelltest3.Oil = 1166;
                latestwelltest3.Water = 38;
                latestwelltest3.Gas = 44350;
                latestwelltest3.AverageTubingPressure = 1559;
                latestwelltest3.SPTCodeDescription = "AllocatableTest";
                WellTestUnitsDTO units3 = WellTestDataService.GetWellTestDefaults(GP3well.Id.ToString()).Units;
                WellTestDataService.UpdateWellTestData(new WellTestAndUnitsDTO(units3, latestwelltest3));
                bool tunewell3 = WellTestDataService.TuneWellTests(GP3well.Id.ToString());
                Assert.IsTrue(tunewell3);
                welltestdata4GP3 = WellTestDataService.GetWellTestDataByWellId(GP3well.Id.ToString());
                latestwelltest3 = welltestdata4GP3.Values.First();
                Assert.AreEqual("Success", latestwelltest3.TuningStatus, "Tuning Status was not Correct GP3 ");

                WellDTO GP4well = WellService.GetAllWells().FirstOrDefault(w => w.Name == "GP4");
                AddWellSettingWithDoubleValues(GP4well.Id, SettingServiceStringConstants.WATER_GAS_RATIO_MIN_AL, 0.4);
                AddWellSettingWithDoubleValues(GP4well.Id, SettingServiceStringConstants.WATER_GAS_RATIO_MAX_AL, 10);
                AddWellSettingWithDoubleValues(GP4well.Id, SettingServiceStringConstants.CONDENSATE_GAS_RATIO_MIN_AL, 15);
                AddWellSettingWithDoubleValues(GP4well.Id, SettingServiceStringConstants.CONDENSATE_GAS_RATIO_MAX_AL, 300);
                AddWellSettingWithDoubleValues(GP4well.Id, SettingServiceStringConstants.LFACTOR_MIN_AL, 0.88);
                AddWellSettingWithDoubleValues(GP4well.Id, SettingServiceStringConstants.LFACTOR_MAX_AL, 1.28);
                var welltestdata4GP4 = WellTestDataService.GetWellTestDataByWellId(GP4well.Id.ToString());
                WellTestDTO latestwelltest4 = welltestdata4GP4.Values.First();
                latestwelltest4.Oil = 680;
                latestwelltest4.Water = 20;
                latestwelltest4.Gas = 19780;
                latestwelltest4.AverageTubingPressure = 1800;
                latestwelltest4.SPTCodeDescription = "AllocatableTest";
                WellTestUnitsDTO units4 = WellTestDataService.GetWellTestDefaults(GP4well.Id.ToString()).Units;
                WellTestDataService.UpdateWellTestData(new WellTestAndUnitsDTO(units4, latestwelltest4));
                bool tunewell4 = WellTestDataService.TuneWellTests(GP4well.Id.ToString());
                Assert.IsTrue(tunewell4);
                welltestdata4GP4 = WellTestDataService.GetWellTestDataByWellId(GP4well.Id.ToString());
                latestwelltest4 = welltestdata4GP4.Values.First();
                Assert.AreEqual("Success", latestwelltest4.TuningStatus, "Tuning Status was not Correct GP4");

                WellDTO GI1well = WellService.GetAllWells().FirstOrDefault(w => w.Name == "GI1");
                AddWellSettingWithDoubleValues(GI1well.Id, SettingServiceStringConstants.LFACTOR_MIN_AL, 0.51);
                AddWellSettingWithDoubleValues(GI1well.Id, SettingServiceStringConstants.LFACTOR_MAX_AL, 1.28);
                var welltestdata4GI1 = WellTestDataService.GetWellTestDataByWellId(GI1well.Id.ToString());
                WellTestDTO latestwelltest5 = welltestdata4GI1.Values.First();
                latestwelltest5.Oil = 0;
                latestwelltest5.Water = 0;
                latestwelltest5.Gas = 32400;
                latestwelltest5.AverageTubingPressure = 4277;
                latestwelltest5.AverageTubingTemperature = 0;
                latestwelltest5.FlowLinePressure = 1366;
                latestwelltest5.SPTCodeDescription = "AllocatableTest";
                WellTestUnitsDTO units5 = WellTestDataService.GetWellTestDefaults(GI1well.Id.ToString()).Units;
                WellTestDataService.UpdateWellTestData(new WellTestAndUnitsDTO(units5, latestwelltest5));
                bool tunewell5 = WellTestDataService.TuneWellTests(GI1well.Id.ToString());
                Assert.IsTrue(tunewell5);
                welltestdata4GI1 = WellTestDataService.GetWellTestDataByWellId(GI1well.Id.ToString());
                latestwelltest5 = welltestdata4GI1.Values.First();
                Assert.AreEqual("Success", latestwelltest5.TuningStatus, "Tuning Status was not Correct GI1");

                WellDTO GI2well = WellService.GetAllWells().FirstOrDefault(w => w.Name == "GI2");
                AddWellSettingWithDoubleValues(GI2well.Id, SettingServiceStringConstants.LFACTOR_MIN_AL, 0.51);
                AddWellSettingWithDoubleValues(GI2well.Id, SettingServiceStringConstants.LFACTOR_MAX_AL, 1.28);
                var welltestdata4GI2 = WellTestDataService.GetWellTestDataByWellId(GI2well.Id.ToString());
                WellTestDTO latestwelltest6 = welltestdata4GI2.Values.First();
                latestwelltest6.Oil = 0;
                latestwelltest6.Water = 0;
                latestwelltest6.Gas = 46180;
                latestwelltest6.AverageTubingPressure = 4290;
                latestwelltest6.AverageTubingTemperature = 0;
                latestwelltest6.FlowLinePressure = 100;
                latestwelltest6.SPTCodeDescription = "AllocatableTest";
                WellTestUnitsDTO units6 = WellTestDataService.GetWellTestDefaults(GI2well.Id.ToString()).Units;
                WellTestDataService.UpdateWellTestData(new WellTestAndUnitsDTO(units6, latestwelltest6));
                bool tunewell6 = WellTestDataService.TuneWellTests(GI2well.Id.ToString());
                Assert.IsTrue(tunewell6);
                welltestdata4GI2 = WellTestDataService.GetWellTestDataByWellId(GI2well.Id.ToString());
                latestwelltest6 = welltestdata4GI2.Values.First();
                Assert.AreEqual("Success", latestwelltest6.TuningStatus, "Tuning Status was not Correct GI2 ");
                #endregion Add Wells and Update Welltests and get tunned well tests

                // step3 : Add and Verify Daily Averge Data For ADNOC Wells in/from DB
                #region 3. Add and Verify Daily Averge Data For ADNOC Wells

                #region Checking AUTO_INJECTION_DAILY_AVERAGE_WELLTEST Setting
                var getAutoInjectionDailyAverageWelltest = GetForeSiteToolBoxSettingValue(SettingServiceStringConstants.AUTO_INJECTION_DAILY_AVERAGE_WELLTEST);
                if (getAutoInjectionDailyAverageWelltest.ToString() != "0")
                {
                    //Setting AUTO_INJECTION_DAILY_AVERAGE_WELLTEST to false
                    SetValuesInSystemSettings(SettingServiceStringConstants.AUTO_INJECTION_DAILY_AVERAGE_WELLTEST, "0");
                    var modifiedAutoInjectionDailyAverageWelltest = GetForeSiteToolBoxSettingValue(SettingServiceStringConstants.AUTO_INJECTION_DAILY_AVERAGE_WELLTEST);
                    Assert.AreEqual(0, modifiedAutoInjectionDailyAverageWelltest, "Mismatched value for AUTO_INJECTION_DAILY_AVERAGE_WELLTEST");
                }
                #endregion Checking AUTO_INJECTION_DAILY_AVERAGE_WELLTEST Setting

                // Add Daily Average data for those wells 
                DateTime day1ago = DateTime.Today.ToUniversalTime();
                DateTime day2ago = DateTime.Today.ToUniversalTime().AddDays(-1);
                DateTime day3ago = DateTime.Today.ToUniversalTime().AddDays(-2);
                DateTime day4ago = DateTime.Today.ToUniversalTime().AddDays(-3);
                DateTime end = DateTime.Today.ToUniversalTime().AddDays(1);

                // GP1 well Daily Average Values
                AddDailyAvergeDataForADNOCWells(GP1well.Id, day4ago, day3ago, welltestdata4GP1.Values[0].Id, welltestdata4GP1.Values[0].LastChangedDT, 12, 0.92, 59.465, 103, 22656);
                AddDailyAvergeDataForADNOCWells(GP1well.Id, day3ago, day2ago, welltestdata4GP1.Values[0].Id, welltestdata4GP1.Values[0].LastChangedDT, 16, 0.92, 79.286, 137.333, 30208);
                AddDailyAvergeDataForADNOCWells(GP1well.Id, day2ago, day1ago, welltestdata4GP1.Values[0].Id, welltestdata4GP1.Values[0].LastChangedDT, 20, 0.92, 99.108, 171.666, 37760);
                AddDailyAvergeDataForADNOCWells(GP1well.Id, day1ago, end, welltestdata4GP1.Values[0].Id, welltestdata4GP1.Values[0].LastChangedDT, 24, 0.92, 118.93, 206, 45312);
                // GP2 well Daily Average Values
                AddDailyAvergeDataForADNOCWells(GP2well.Id, day4ago, day3ago, welltestdata4GP2.Values[0].Id, welltestdata4GP2.Values[0].LastChangedDT, 12, 0.94, 1524.55, 31.685, 29000);
                AddDailyAvergeDataForADNOCWells(GP2well.Id, day3ago, day2ago, welltestdata4GP2.Values[0].Id, welltestdata4GP2.Values[0].LastChangedDT, 16, 0.94, 2032.73, 42.246, 38666.666);
                AddDailyAvergeDataForADNOCWells(GP2well.Id, day2ago, day1ago, welltestdata4GP2.Values[0].Id, welltestdata4GP2.Values[0].LastChangedDT, 22, 0.94, 2795.01, 58.089, 53166.666);
                AddDailyAvergeDataForADNOCWells(GP2well.Id, day1ago, end, welltestdata4GP2.Values[0].Id, welltestdata4GP2.Values[0].LastChangedDT, 24, 0.94, 2820.82, 63.37, 58000);
                // GP3 well Daily Average Values
                AddDailyAvergeDataForADNOCWells(GP3well.Id, day4ago, day3ago, welltestdata4GP3.Values[0].Id, welltestdata4GP3.Values[0].LastChangedDT, 16, 0.96, 2338.57, 0.0666666666666667, 30000);
                AddDailyAvergeDataForADNOCWells(GP3well.Id, day3ago, day2ago, welltestdata4GP3.Values[0].Id, welltestdata4GP3.Values[0].LastChangedDT, 20, 0.96, 2923.22, 0.0833333333333333, 37500);
                AddDailyAvergeDataForADNOCWells(GP3well.Id, day2ago, day1ago, welltestdata4GP3.Values[0].Id, welltestdata4GP3.Values[0].LastChangedDT, 22, 0.96, 3215.54, 0.0916666666666667, 41250);
                AddDailyAvergeDataForADNOCWells(GP3well.Id, day1ago, end, welltestdata4GP3.Values[0].Id, welltestdata4GP3.Values[0].LastChangedDT, 24, 0.96, 2103.35, 0.1, 45000);
                // GP4 well Daily Average Values
                AddDailyAvergeDataForADNOCWells(GP4well.Id, day4ago, day3ago, welltestdata4GP4.Values[0].Id, welltestdata4GP4.Values[0].LastChangedDT, 12, 0.98, 772.523, 26.92, 24500);
                AddDailyAvergeDataForADNOCWells(GP4well.Id, day3ago, day2ago, welltestdata4GP4.Values[0].Id, welltestdata4GP4.Values[0].LastChangedDT, 16, 0.98, 1030.031, 35.89333, 32666.6666);
                AddDailyAvergeDataForADNOCWells(GP4well.Id, day2ago, day1ago, welltestdata4GP4.Values[0].Id, welltestdata4GP4.Values[0].LastChangedDT, 24, 0.98, 1545.047, 53.84, 49000);
                AddDailyAvergeDataForADNOCWells(GP4well.Id, day1ago, end, welltestdata4GP4.Values[0].Id, welltestdata4GP4.Values[0].LastChangedDT, 24, 0.98, 1564.37, 53.84, 49000);
                // GI1 well Daily Average Values
                AddDailyAvergeDataForADNOCWells(GI1well.Id, day4ago, day3ago, welltestdata4GI1.Values[0].Id, welltestdata4GI1.Values[0].LastChangedDT, 12, null, 0, 0, 34525);
                AddDailyAvergeDataForADNOCWells(GI1well.Id, day3ago, day2ago, welltestdata4GI1.Values[0].Id, welltestdata4GI1.Values[0].LastChangedDT, 24, null, 0, 0, 69050);
                AddDailyAvergeDataForADNOCWells(GI1well.Id, day2ago, day1ago, welltestdata4GI1.Values[0].Id, welltestdata4GI1.Values[0].LastChangedDT, 24, null, 0, 0, 69050);
                AddDailyAvergeDataForADNOCWells(GI1well.Id, day1ago, end, welltestdata4GI1.Values[0].Id, welltestdata4GI1.Values[0].LastChangedDT, 24, null, 0, 0, 69050);
                // GI2 well Daily Average Values
                AddDailyAvergeDataForADNOCWells(GI2well.Id, day4ago, day3ago, welltestdata4GI2.Values[0].Id, welltestdata4GI2.Values[0].LastChangedDT, 12, null, 0, 0, 13860);
                AddDailyAvergeDataForADNOCWells(GI2well.Id, day3ago, day2ago, welltestdata4GI2.Values[0].Id, welltestdata4GI2.Values[0].LastChangedDT, 24, null, 0, 0, 27720);
                AddDailyAvergeDataForADNOCWells(GI2well.Id, day2ago, day1ago, welltestdata4GI2.Values[0].Id, welltestdata4GI2.Values[0].LastChangedDT, 24, null, 0, 0, 27720);
                AddDailyAvergeDataForADNOCWells(GI2well.Id, day1ago, end, welltestdata4GI2.Values[0].Id, welltestdata4GI2.Values[0].LastChangedDT, 24, null, 0, 0, 27720);

                //retrive daily average data from DB.
                WellDailyAverageArrayAndUnitsDTO dailyAverageData = SurveillanceService.GetDailyAverages(GP1well.Id.ToString(), DateTime.Today.AddDays(-1).ToUniversalTime().ToISO8601(), DateTime.Today.ToUniversalTime().ToISO8601());
                Trace.WriteLine("Verified Daily average data for last 4 days - ADNOC Wells");
                #endregion 3. Add and Verify Daily Averge Data For ADNOC Wells

                // step4 : Create VRR Hierarchy with Volume, Targets and Map Wells to Zone and Pattern.
                #region 4. Add VRR Configuration along with Volume and Target data
                var allWells = WellService.GetAllWells().ToList();
                Assert.IsNotNull(allWells);

                // Add Reservoir
                ReservoirAndUnitsDTO addReservoir1 = AddReservoirToAsset("Res", assetIds.ElementAt(0).Id);
                // GetReservoir IDs
                ReservoirArrayAndUnitsDTO getReservoirByAsset = WellAllocationService.GetReservoirsByAssetId(assetIds.ElementAt(0).Id.ToString());
                // Update the Volume values of Reservoir.
                getReservoirByAsset.Values[0].InitialOilProductionVolume = 50;
                getReservoirByAsset.Values[0].InitialWaterProductionVolume = 50;
                getReservoirByAsset.Values[0].InitialGasProductionVolume = 100;
                getReservoirByAsset.Values[0].InitialWaterInjectionVolume = 0;
                getReservoirByAsset.Values[0].InitialGasInjectionVolume = 700;
                ReservoirAndUnitsDTO getAndUpdateRreservoir = WellAllocationService.GetReservoirById("0");
                getAndUpdateRreservoir.Value = getReservoirByAsset.Values[0];
                WellAllocationService.AddOrUpdateReservoir(getAndUpdateRreservoir);

                // Add Zone
                ZoneAndUnitsDTO addZone1 = AddZoneToReservoir("Zone", getReservoirByAsset.Values[0].Id);
                // Get Zone IDs based on reservoir
                ZoneArrayAndUnitsDTO getZoneByReservoir = WellAllocationService.GetZonesByReservoirId(getReservoirByAsset.Values[0].Id.ToString());
                // Update the Volume values of Zone. -
                getZoneByReservoir.Values[0].InitialOilProductionVolume = 50;
                getZoneByReservoir.Values[0].InitialWaterProductionVolume = 50;
                getZoneByReservoir.Values[0].InitialGasProductionVolume = 100;
                getZoneByReservoir.Values[0].InitialWaterInjectionVolume = 0;
                getZoneByReservoir.Values[0].InitialGasInjectionVolume = 700;
                ZoneAndUnitsDTO getAndUpdateZone = WellAllocationService.GetZoneById("0");
                getAndUpdateZone.Value = getZoneByReservoir.Values[0];
                WellAllocationService.AddOrUpdateZone(getAndUpdateZone);

                // Assign Well to Zone
                WellToZoneDTO assignWellGI1ToZone = AddWellToZone(allWells[0].Id, getZoneByReservoir.Values[0].Id);
                WellToZoneDTO assignWellGP1ToZone = AddWellToZone(allWells[2].Id, getZoneByReservoir.Values[0].Id);
                WellToZoneDTO assignWellGP2ToZone = AddWellToZone(allWells[3].Id, getZoneByReservoir.Values[0].Id);
                Assert.IsNotNull(assignWellGI1ToZone); // GI1 Gas Injection              
                Assert.IsNotNull(assignWellGP1ToZone); // GP1 NF Well
                Assert.IsNotNull(assignWellGP2ToZone); // GP2 NF Well

                // Add Pattern
                PatternDTO addPattern1 = AddPatternToZoneOfReservoir("Pattern", getZoneByReservoir.Values[0].Id);
                // Get Pattern Ids based on reservoir            
                List<PatternDTO> getPatternByZoneOfReservoir = WellAllocationService.GetPatternsByZoneId(getZoneByReservoir.Values[0].Id.ToString());
                // Assign Well to Pattern
                WellToPatternDTO assignWellGP3ToPattern = AddWellToPattern(allWells[4].Id, getPatternByZoneOfReservoir.ElementAt(0).Id);
                WellToPatternDTO assignWellGP4ToPattern = AddWellToPattern(allWells[5].Id, getPatternByZoneOfReservoir.ElementAt(0).Id);
                WellToPatternDTO assignWellGI2ToPattern = AddWellToPattern(allWells[1].Id, getPatternByZoneOfReservoir.ElementAt(0).Id);
                Assert.IsNotNull(assignWellGP3ToPattern); // GP3 NF Well
                Assert.IsNotNull(assignWellGP4ToPattern); // GP4 NF Well
                Assert.IsNotNull(assignWellGI2ToPattern); // GI2 Gas Injection 

                #region 4.1 Configure the Targets for Reservoir / Zone / Pattern and verify the values
                Trace.WriteLine("Configure the Targets for Reservoir / Zone / Pattern");

                // Reservoir Value DTO
                SubsurfaceEntityTargetBoundDTO reservoirTargetBoundValueDTO = new SubsurfaceEntityTargetBoundDTO
                {
                    ReservoirId = getReservoirByAsset.Values[0].Id,
                    ZoneId = null,
                    PatternId = null,
                    StartDate = DateTime.Today.AddDays(-2).ToLocalTime(),
                    EndDate = null,
                    BoundPreference = SubsurfaceEntityTargetType.FixedValue,
                    TargetValue = 1,
                    FixedLowerBoundTolerance = (decimal)0.50,
                    FixedUpperBoundTolerance = 1,
                    PercentageLowerBoundTolerance = null,
                    PercentageUpperBoundTolerance = null
                };
                // Get Units for Reservoir Target 
                SubsurfaceEntityTargetBoundAndUnitsDTO addReservoirTargets = WellAllocationService.GetSubsurfaceEntityByEntityId(getReservoirByAsset.Values[0].Id.ToString(), SubsurfaceResultEntityType.Reservoir.ToString(), getReservoirByAsset.Values[0].ApplicableDate.ToISO8601Date());
                addReservoirTargets.Value = reservoirTargetBoundValueDTO;
                // Add Targets for Reservoir
                WellAllocationService.AddOrUpdateReservoirTargetBound(addReservoirTargets);
                Trace.WriteLine("Added Targets for Reservoir");

                // Verify that all the added Targets for Reservoir
                SubsurfaceEntityTargetBoundAndUnitsDTO verifyReservoirTargets = WellAllocationService.GetSubsurfaceEntityByEntityId(getReservoirByAsset.Values[0].Id.ToString(), SubsurfaceResultEntityType.Reservoir.ToString(), getReservoirByAsset.Values[0].ApplicableDate.ToISO8601Date());
                Assert.IsNotNull(verifyReservoirTargets);

                // Zone Value DTO
                SubsurfaceEntityTargetBoundDTO zoneTargetBoundValueDTO = new SubsurfaceEntityTargetBoundDTO
                {
                    ReservoirId = null,
                    ZoneId = getZoneByReservoir.Values[0].Id,
                    PatternId = null,
                    StartDate = DateTime.Today.AddDays(-2).ToLocalTime(),
                    EndDate = null,
                    BoundPreference = SubsurfaceEntityTargetType.FixedValue,
                    TargetValue = 1,
                    FixedLowerBoundTolerance = (decimal)0.40,
                    FixedUpperBoundTolerance = 1,
                    PercentageLowerBoundTolerance = null,
                    PercentageUpperBoundTolerance = null
                };
                // Get Units for Zone Target 
                SubsurfaceEntityTargetBoundAndUnitsDTO addZoneTargets = WellAllocationService.GetSubsurfaceEntityByEntityId(getZoneByReservoir.Values[0].Id.ToString(), SubsurfaceResultEntityType.Zone.ToString(), getZoneByReservoir.Values[0].ApplicableDate.ToISO8601Date());
                addZoneTargets.Value = zoneTargetBoundValueDTO;
                // Add Targets for Zone
                WellAllocationService.AddOrUpdateZoneTargetBound(addZoneTargets);
                Trace.WriteLine("Added Targets for Zone");
                // Verify that all the added Targets for Zone
                SubsurfaceEntityTargetBoundAndUnitsDTO verifyZoneTargets = WellAllocationService.GetSubsurfaceEntityByEntityId(getZoneByReservoir.Values[0].Id.ToString(), SubsurfaceResultEntityType.Zone.ToString(), getZoneByReservoir.Values[0].ApplicableDate.ToISO8601Date());
                Assert.IsNotNull(verifyZoneTargets);

                // Pattern Value DTO
                SubsurfaceEntityTargetBoundDTO patternTargetBoundValueDTO = new SubsurfaceEntityTargetBoundDTO
                {
                    ReservoirId = null,
                    ZoneId = null,
                    PatternId = getPatternByZoneOfReservoir.ElementAt(0).Id,
                    StartDate = DateTime.Today.AddDays(-2).ToLocalTime(),
                    EndDate = null,
                    BoundPreference = SubsurfaceEntityTargetType.FixedValue,
                    TargetValue = 1,
                    FixedLowerBoundTolerance = (decimal)0.30,
                    FixedUpperBoundTolerance = 1,
                    PercentageLowerBoundTolerance = null,
                    PercentageUpperBoundTolerance = null
                };
                // Get Units for Pattern Target 
                SubsurfaceEntityTargetBoundAndUnitsDTO addPatternTargets = WellAllocationService.GetSubsurfaceEntityByEntityId(getPatternByZoneOfReservoir.ElementAt(0).Id.ToString(), SubsurfaceResultEntityType.Pattern.ToString(), getZoneByReservoir.Values[0].ApplicableDate.ToISO8601Date());
                addPatternTargets.Value = patternTargetBoundValueDTO;
                // Add Targets for Pattern
                WellAllocationService.AddOrUpdatePatternTargetBound(addPatternTargets);
                Trace.WriteLine("Added Targets for Pattern");

                // Verify that all the added Targets for Pattern
                SubsurfaceEntityTargetBoundAndUnitsDTO verifyPatternTargets = WellAllocationService.GetSubsurfaceEntityByEntityId(getPatternByZoneOfReservoir.ElementAt(0).Id.ToString(), SubsurfaceResultEntityType.Pattern.ToString(), getZoneByReservoir.Values[0].ApplicableDate.ToISO8601Date());
                Assert.IsNotNull(verifyPatternTargets);
                #endregion 4.1 Configure the Targets for Reservoir / Zone / Pattern and verify the values 

                #endregion 4 Add VRR Configuration along with Volume and Target data

                #region Set Reservoir and Zone Calculation Start Date as day - 1 for VRR calculation. 
                // Update the Calculation Start Date as Today for Reservoir and Zone
                getReservoirByAsset.Values[0].ApplicableDate = DateTime.Today.AddDays(-1).ToLocalTime();
                getAndUpdateRreservoir.Value = getReservoirByAsset.Values[0];
                WellAllocationService.AddOrUpdateReservoir(getAndUpdateRreservoir);

                getZoneByReservoir.Values[0].ApplicableDate = DateTime.Today.AddDays(-1).ToLocalTime();
                getAndUpdateZone.Value = getZoneByReservoir.Values[0];
                WellAllocationService.AddOrUpdateZone(getAndUpdateZone);
                #endregion

                // step5 : Run the VRR calculation
                #region 5. Run the VRR calculation
                Trace.WriteLine("VRR Run has been initiated for ADNOC Wells");
                RunAnalysisTaskScheduler("-runVRR");
                Trace.WriteLine("-runVRR completed for ADNOC Wells");
                #endregion 5. Run the VRR calculation

                // step6 : Verify the Calculation Of Instantaneous VRR for Reservoirs Zones Patterns
                #region 6. Verify the Calculation Of Instantaneous and Cumulative VRR for Reservoirs Zones Patterns
                Trace.WriteLine("Verify VRR Run Results Instantaneous and Cumulative for ADNOC Wells");
                WellFilterDTO wellbyFilter = new WellFilterDTO
                {
                    AssetIds = new long?[] { assetIds.ElementAt(0).Id }
                };
                SubsurfaceHierarchyDTO hierarchy = WellAllocationService.GetSubsurfaceHierarchiesByDate(wellbyFilter, getReservoirByAsset.Values[0].ApplicableDate.ToISO8601Date());

                // Verify Reservoir Instantaneous VRR Data
                SubsurfaceHierarchyNodeDTO assetHierarchy = hierarchy.Nodes[0]; // Asset Selected
                SubsurfaceEntityResultsArrayAndUnitsDTO reservoirVRRResults = WellAllocationService.GetVRRForSelectedNode(assetHierarchy, DateTime.Today.AddDays(-1).ToUniversalTime().ToISO8601());
                Assert.IsNotNull(reservoirVRRResults.Values[0]);
                Assert.AreEqual(reservoirVRRResults.Values[0].EntityType.ToString(), "Reservoir", "Reservoir Results not found.");
                Trace.WriteLine("Verified Reservoir VRR Run Results Instantaneous and Cumulative for ADNOC Wells");

                // Verify Zone Instantaneous VRR Data
                SubsurfaceHierarchyNodeDTO reservoirHierarchy = assetHierarchy.Nodes[0]; // Reservoir Selected
                SubsurfaceEntityResultsArrayAndUnitsDTO zoneVRRResults = WellAllocationService.GetVRRForSelectedNode(reservoirHierarchy, DateTime.Today.AddDays(-1).ToUniversalTime().ToISO8601());
                Assert.IsNotNull(zoneVRRResults.Values[0]);
                Assert.AreEqual(zoneVRRResults.Values[0].EntityType.ToString(), "Zone", "Zone Results not found.");
                Trace.WriteLine("Verified Zone VRR Run Results Instantaneous and Cumulative for ADNOC Wells");

                // Verify Well Data from Zones
                SubsurfaceHierarchyNodeDTO zoneHierarchy = reservoirHierarchy.Nodes[0]; // Zone Selected and well data populated
                SubsurfaceEntityResultsArrayAndUnitsDTO wellVRRResultsFromZone = WellAllocationService.GetVRRForSelectedWellsInZone(zoneHierarchy, DateTime.Today.AddDays(-1).ToUniversalTime().ToISO8601());
                Assert.IsNotNull(wellVRRResultsFromZone.Values[0]);
                Assert.AreEqual(wellVRRResultsFromZone.Values[0].EntityName.ToString(), "GI1", "GI1 Results not found.");
                Trace.WriteLine("Verified Well data from Zones VRR Run Results for ADNOC Wells");

                // Verify Pattern Instantaneous VRR Data
                SubsurfaceEntityResultsArrayAndUnitsDTO patternVRRResultsFromZone = WellAllocationService.GetVRRForSelectedNode(zoneHierarchy, DateTime.Today.AddDays(-1).ToUniversalTime().ToISO8601());
                Assert.IsNotNull(patternVRRResultsFromZone);
                Assert.AreEqual(patternVRRResultsFromZone.Values[0].EntityType.ToString(), "Pattern", "Pattern Results not found.");
                Trace.WriteLine("Verified Pattern VRR Run Results Instantaneous and Cumulative for ADNOC Wells");

                // Verify Well Data from Pattern
                SubsurfaceHierarchyNodeDTO PatternHierarchy = zoneHierarchy.Nodes[0]; // Pattern Selected and well data populated
                SubsurfaceEntityResultsArrayAndUnitsDTO patternVRRResults = WellAllocationService.GetVRRForSelectedNode(PatternHierarchy, DateTime.Today.AddDays(-1).ToUniversalTime().ToISO8601());
                Assert.IsNotNull(patternVRRResults.Values[0]);
                Assert.AreEqual(patternVRRResults.Values[0].EntityName.ToString(), "GP3", "GP3 Results not found.");
                Trace.WriteLine("Verified Well data from Pattern VRR Run Results for ADNOC Wells");

                #region Metric Unit System
                ChangeUnitSystem("Metric");
                ChangeUnitSystemUserSetting("Metric");

                SubsurfaceHierarchyDTO hierarchyMetric = WellAllocationService.GetSubsurfaceHierarchiesByDate(wellbyFilter, getReservoirByAsset.Values[0].ApplicableDate.ToISO8601Date());

                // Verify Reservoir Instantaneous VRR Data
                SubsurfaceHierarchyNodeDTO assetHierarchyMetric = hierarchyMetric.Nodes[0]; // Asset Selected
                SubsurfaceEntityResultsArrayAndUnitsDTO reservoirVRRResultsMetric = WellAllocationService.GetVRRForSelectedNode(assetHierarchyMetric, DateTime.Today.AddDays(-1).ToUniversalTime().ToISO8601());
                Assert.IsNotNull(reservoirVRRResultsMetric.Values[0]);
                Assert.AreEqual(reservoirVRRResultsMetric.Values[0].EntityType.ToString(), "Reservoir", "Reservoir Results not found.");
                Trace.WriteLine("Verified Reservoir VRR Run Results Instantaneous and Cumulative for ADNOC Wells");

                // Verify Zone Instantaneous VRR Data
                SubsurfaceHierarchyNodeDTO reservoirHierarchyMetric = assetHierarchyMetric.Nodes[0]; // Reservoir Selected
                SubsurfaceEntityResultsArrayAndUnitsDTO zoneVRRResultsMetric = WellAllocationService.GetVRRForSelectedNode(reservoirHierarchyMetric, DateTime.Today.AddDays(-1).ToUniversalTime().ToISO8601());
                Assert.IsNotNull(zoneVRRResultsMetric.Values[0]);
                Assert.AreEqual(zoneVRRResultsMetric.Values[0].EntityType.ToString(), "Zone", "Zone Results not found.");
                Trace.WriteLine("Verified Zone VRR Run Results Instantaneous and Cumulative for ADNOC Wells");

                // Verify Well Data from Zones
                SubsurfaceHierarchyNodeDTO zoneHierarchyMetric = reservoirHierarchyMetric.Nodes[0]; // Zone Selected and well data populated
                SubsurfaceEntityResultsArrayAndUnitsDTO wellVRRResultsFromZoneMetric = WellAllocationService.GetVRRForSelectedWellsInZone(zoneHierarchyMetric, DateTime.Today.AddDays(-1).ToUniversalTime().ToISO8601());
                Assert.IsNotNull(wellVRRResultsFromZoneMetric.Values[0]);
                Assert.AreEqual(wellVRRResultsFromZoneMetric.Values[0].EntityName.ToString(), "GI1", "GI1 Results not found.");
                Trace.WriteLine("Verified Well data from Zones VRR Run Results for ADNOC Wells");

                // Verify Pattern Instantaneous VRR Data
                SubsurfaceEntityResultsArrayAndUnitsDTO patternVRRResultsFromZoneMetric = WellAllocationService.GetVRRForSelectedNode(zoneHierarchyMetric, DateTime.Today.AddDays(-1).ToUniversalTime().ToISO8601());
                Assert.IsNotNull(patternVRRResultsFromZoneMetric);
                Assert.AreEqual(patternVRRResultsFromZoneMetric.Values[0].EntityType.ToString(), "Pattern", "Pattern Results not found.");
                Trace.WriteLine("Verified Pattern VRR Run Results Instantaneous and Cumulative for ADNOC Wells");

                // Verify Well Data from Pattern
                SubsurfaceHierarchyNodeDTO PatternHierarchyMetric = zoneHierarchyMetric.Nodes[0]; // Pattern Selected and well data populated
                SubsurfaceEntityResultsArrayAndUnitsDTO patternVRRResultsMetric = WellAllocationService.GetVRRForSelectedNode(PatternHierarchyMetric, DateTime.Today.AddDays(-1).ToUniversalTime().ToISO8601());
                Assert.IsNotNull(patternVRRResultsMetric.Values[0]);
                Assert.AreEqual(patternVRRResultsMetric.Values[0].EntityName.ToString(), "GP3", "GP3 Results not found.");
                Trace.WriteLine("Verified Well data from Pattern VRR Run Results for ADNOC Wells");
                #endregion Metric Unit System

                Trace.WriteLine("Verified successfully VRR Run Results Instantaneous and Cumulative for ADNOC Wells");
                #endregion Verify the Calculation Of Instantaneous and Cumulative VRR for Reservoirs Zones Patterns               
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                WellAllocationService.DeleteHierarchyByAssetId(assetIds.ElementAt(0).Id.ToString());
                ChangeUnitSystem("US");
                ChangeUnitSystemUserSetting("US");
                Trace.WriteLine("Pattern1 pattern deleted successfully");
                Trace.WriteLine("Zone1 zone deleted successfully");
                Trace.WriteLine("Res1 reservoir deleted successfully");
            }
        }

        /// <summary>
        /// FRWM-6697 Licensing and user permissions for Voidage workflow
        /// FRWM-7332 Sub-task
        /// Verify that,All the permissions(ReservoirCRUD, ZoneCURD, PatternCURD) are working as expected along with Licensing
        /// </summary>
        [TestCategory(TestCategories.WellAllocationServiceTests), TestMethod]
        public void VRR_License_Permissions_Check_And_CRUD_Operations()
        {
            List<AssetDTO> assetIds = null;
            bool isVRRFeatureLicensed = true;
            try
            {
                // Create an asset
                assetIds = CreateAsset(1);

                CreateRole("TestRole");
                UpdateUserWithGivenRole("TestRole");
                // Removed ReservoirCRUD permission from current user.
                RemovePermissionsFromRole("TestRole", new PermissionId[] { PermissionId.ReservoirCRUD });
                try
                {
                    // Add Reservoir
                    ReservoirAndUnitsDTO addReservoir1 = AddReservoirToAsset("Res", assetIds.ElementAt(0).Id);
                }
                catch (Exception e)
                {
                    if (!e.Message.Contains("Forbidden"))
                    {
                        isVRRFeatureLicensed = false;
                        Trace.WriteLine("General error with system license - Voidage Monitoring feature is not licensed.");
                    }
                    else
                    {
                        Trace.WriteLine("Reservoir cannot be added due to permission revoked : " + e.Message);
                        Assert.IsTrue(e.Message.Contains("Forbidden"));
                    }
                }

                // Removed ZoneCRUD permission from current user.
                RemovePermissionsFromRole("TestRole", new PermissionId[] { PermissionId.ZoneCRUD });
                // Add ReservoirCRUD permission to Current user.
                AddPermissioninRole("TestRole", PermissionId.ReservoirCRUD);
                ReservoirArrayAndUnitsDTO getReservoirByAsset = new ReservoirArrayAndUnitsDTO();
                try
                {
                    // Add Reservoir
                    ReservoirAndUnitsDTO addReservoir1 = AddReservoirToAsset("Res", assetIds.ElementAt(0).Id);
                    // GetReservoir IDs
                    getReservoirByAsset = WellAllocationService.GetReservoirsByAssetId(assetIds.ElementAt(0).Id.ToString());

                    // Add Zone
                    ZoneAndUnitsDTO addZone1 = AddZoneToReservoir("Zone", getReservoirByAsset.Values[0].Id);
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("Forbidden"))
                    {
                        Trace.WriteLine("Reservoir added but Zone cannot be added due to permission revoked : " + e.Message);
                        Assert.IsTrue(e.Message.Contains("Forbidden"));
                    }
                }

                // Removed PatternCRUD permission from current user.
                RemovePermissionsFromRole("TestRole", new PermissionId[] { PermissionId.PatternCRUD });
                // Add ZoneCRUD permission to Current user.
                AddPermissioninRole("TestRole", PermissionId.ZoneCRUD);
                ZoneArrayAndUnitsDTO getZoneByReservoir = new ZoneArrayAndUnitsDTO();
                PatternDTO addPattern1 = new PatternDTO();
                try
                {
                    // Add Zone
                    ZoneAndUnitsDTO addZone1 = AddZoneToReservoir("Zone", getReservoirByAsset.Values[0].Id);

                    // Get Zone IDs based on reservoir
                    getZoneByReservoir = WellAllocationService.GetZonesByReservoirId(getReservoirByAsset.Values[0].Id.ToString());

                    // Add Pattern
                    addPattern1 = AddPatternToZoneOfReservoir("Pattern", getZoneByReservoir.Values[0].Id);
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("Forbidden"))
                    {
                        Trace.WriteLine("Zone added but Pattern cannot be added due to permission revoked : " + e.Message);
                        Assert.IsTrue(e.Message.Contains("Forbidden"));
                    }
                }

                // Add PatternCRUD permission to Current user.
                AddPermissioninRole("TestRole", PermissionId.PatternCRUD);
                List<PatternDTO> getPatternByZoneOfReservoir = new List<PatternDTO>();
                try
                {
                    // Add Pattern
                    addPattern1 = AddPatternToZoneOfReservoir("Pattern", getZoneByReservoir.Values[0].Id);
                    // Get Pattern Ids based on reservoir            
                    getPatternByZoneOfReservoir = WellAllocationService.GetPatternsByZoneId(getZoneByReservoir.Values[0].Id.ToString());
                }
                catch (Exception e)
                {
                    Trace.WriteLine("Error While adding Pattern : " + e.Message);
                }

                // If Hard delete functionality implemented then we need to uncomment this code.
                //PatternDTO removePattern = getPatternByZoneOfReservoir[0];
                //WellAllocationService.RemovePattern(removePattern);
                //WellAllocationService.RemoveZone(getZoneByReservoir.Values[0]);
                //WellAllocationService.RemoveReservoir(getReservoirByAsset.Values[0]);
            }
            finally
            {
                UpdateUserWithGivenRole("Administrator");
                if (isVRRFeatureLicensed == true)
                {
                    WellAllocationService.DeleteHierarchyByAssetId(assetIds.ElementAt(0).Id.ToString());
                    Trace.WriteLine("Pattern1 pattern deleted successfully");
                    Trace.WriteLine("Zone1 zone deleted successfully");
                    Trace.WriteLine("Res1 reservoir deleted successfully");
                }
            }
        }

        [TestCategory(TestCategories.WellAllocationServiceTests), TestMethod]
        [Ignore]
        public void VRR_Performance_Database()
        {
            List<AssetDTO> AssetIds = new List<AssetDTO>();
            try
            {
                AssetIds = SurfaceNetworkService.GetAllAssets().ToList();
                if (AssetIds.Count() != 0)
                    AssetIds.Add(AssetIds[0]);
                else
                    AssetIds = CreateAsset(1);

                ADNOC_Well_With_WellTest_Configuration(AssetIds[0].Id, 200, 100);
                Trace.WriteLine("Created wells with valid well test data.");
                ADNOC_Well_DailyAverageData_Configuration(AssetIds[0].Id, 200, 100);
                Trace.WriteLine("Added daily average data for all Wells.");
                CreateVRRHierarchy(1, 10, 5, AssetIds);
                Trace.WriteLine("Created Subsurface hierarchy.");
                VRR_TargetConfiguration(AssetIds);
                Trace.WriteLine("Added Target configuration for each Reservoir, Zone and Pattern.");

                Trace.WriteLine("Break this Integration test here...");
            }
            finally
            {
                WellAllocationService.DeleteHierarchyByAssetId(AssetIds[0].Id.ToString());
                Trace.WriteLine("Pattern1 pattern deleted successfully");
                Trace.WriteLine("Zone1 zone deleted successfully");
                Trace.WriteLine("Res1 reservoir deleted successfully");
            }
        }

        /// <summary>
        /// FRWM_7276_SWA : First time configuration steps
        /// </summary>
        [TestCategory(TestCategories.WellAllocationServiceTests), TestMethod]
        public void SWAInitalSnapShotCreation()
        {
            ChangeUnitSystem("US");
            ChangeUnitSystemUserSetting("US");
            try
            {
                #region  Add Asset
                List<AssetDTO> assets = CreateAsset(2);
                #endregion Add Asset

                #region create Production Wells with Asset
                //NFW Condennsate wells creation
                var modelFileName_cond = "Condensate Gas - IPR Auto Tuning.wflx";
                var options_cond = new ModelFileOptionDTO
                {
                    CalibrationMethod = CalibrationMethodId.ReservoirPressure,
                    OptionalUpdate = new long[]
                      {
                        (long) OptionalUpdates.UpdateWCT_WGR,
                        (long) OptionalUpdates.UpdateGOR_CGR
                      }
                };

                WellDTO NFWell_Con_1 = AddNonRRLWellGeneral("NFWell_Con_1", GetFacilityId("NFWELL_", 1), WellTypeId.NF, WellFluidType.Condensate, WellFluidPhase.None, "1", assets.ElementAt(0).Id);
                AddNonRRLModelFile(NFWell_Con_1, modelFileName_cond, options_cond.CalibrationMethod, options_cond.OptionalUpdate);

                WellDTO NFWell_Con_2 = AddNonRRLWellGeneral("NFWell_Con_2", GetFacilityId("NFWELL_", 2), WellTypeId.NF, WellFluidType.Condensate, WellFluidPhase.None, "1", assets.ElementAt(1).Id);
                AddNonRRLModelFile(NFWell_Con_2, modelFileName_cond, options_cond.CalibrationMethod, options_cond.OptionalUpdate);

                WellDTO NFWell_Con_3 = AddNonRRLWellGeneral("NFWell_Con_3", GetFacilityId("NFWELL_", 5), WellTypeId.NF, WellFluidType.Condensate, WellFluidPhase.None, "1", assets.ElementAt(0).Id);
                AddNonRRLModelFile(NFWell_Con_3, modelFileName_cond, options_cond.CalibrationMethod, options_cond.OptionalUpdate);

                //NFW Drygas wells creation
                var modelFileName_Drygas = "Dry Gas - IPR Auto Tuning.wflx";
                var options_Drygas = new ModelFileOptionDTO
                {
                    CalibrationMethod = CalibrationMethodId.ReservoirPressure,
                    OptionalUpdate = new long[]
                      {
                        (long) OptionalUpdates.UpdateWCT_WGR,
                        //(long) OptionalUpdates.UpdateGOR_CGR
                      }
                };
                WellDTO NFWell_Dry_1 = AddNonRRLWellGeneral("NFWell_Dry_1", GetFacilityId("NFWELL_", 3), WellTypeId.NF, WellFluidType.DryGas, WellFluidPhase.None, "1", assets.ElementAt(0).Id);
                AddNonRRLModelFile(NFWell_Dry_1, modelFileName_Drygas, options_Drygas.CalibrationMethod, options_Drygas.OptionalUpdate);

                WellDTO NFWell_Dry_2 = AddNonRRLWellGeneral("NFWell_Dry_2", GetFacilityId("NFWELL_", 4), WellTypeId.NF, WellFluidType.DryGas, WellFluidPhase.None, "1", assets.ElementAt(1).Id);
                AddNonRRLModelFile(NFWell_Dry_2, modelFileName_Drygas, options_Drygas.CalibrationMethod, options_Drygas.OptionalUpdate);
                #endregion create Production Wells with Asset

                #region create Injection Wells with Asset
                WellDTO GASInjWell_1 = AddNonRRLWellGeneral("GASInjWell_1", GetFacilityId("GASINJWELL_", 1), WellTypeId.GInj, WellFluidType.None, WellFluidPhase.None, "1", assets.ElementAt(0).Id);
                AddModelFile(GASInjWell_1.CommissionDate.Value, WellTypeId.GInj, GASInjWell_1.Id, CalibrationMethodId.LFactor);
                WellDTO GASInjWell_2 = AddNonRRLWellGeneral("GASInjWell_2", GetFacilityId("GASINJWELL_", 2), WellTypeId.GInj, WellFluidType.None, WellFluidPhase.None, "1", assets.ElementAt(1).Id);
                AddModelFile(GASInjWell_2.CommissionDate.Value, WellTypeId.GInj, GASInjWell_2.Id, CalibrationMethodId.LFactor);

                WellDTO WATERInjWell_1 = AddNonRRLWellGeneral("WATERInjWell_1", GetFacilityId("WATERINJWELL_", 1), WellTypeId.WInj, WellFluidType.None, WellFluidPhase.None, "1", assets.ElementAt(0).Id);
                AddModelFile(WATERInjWell_1.CommissionDate.Value, WellTypeId.WInj, WATERInjWell_1.Id, CalibrationMethodId.LFactor);
                WellDTO WATERInjWell_2 = AddNonRRLWellGeneral("WATERInjWell_2", GetFacilityId("WATERINJWELL_", 2), WellTypeId.WInj, WellFluidType.None, WellFluidPhase.None, "1", assets.ElementAt(1).Id);
                AddModelFile(WATERInjWell_2.CommissionDate.Value, WellTypeId.WInj, WATERInjWell_2.Id, CalibrationMethodId.LFactor);

                Trace.WriteLine("Wells are created successfully and mapped with Assets");
                #endregion create Injection Wells with Asset

                #region create Reservoir
                //Reservoir under Asset 1
                ReservoirAndUnitsDTO R1A1 = AddReservoirToAsset("R1A1", assets.ElementAt(0).Id);
                _vrrHierarchyToRemove.Add(assets[0]);
                ReservoirAndUnitsDTO R1A2 = AddReservoirToAsset("R1A2", assets.ElementAt(1).Id);
                _vrrHierarchyToRemove.Add(assets[1]);
                #endregion create Reservoir

                #region getReservoir IDs
                ReservoirArrayAndUnitsDTO ReservoirIDsOfAsset1 = WellAllocationService.GetReservoirsByAssetId(assets.ElementAt(0).Id.ToString());
                ReservoirArrayAndUnitsDTO ReservoirIDsOfAsset2 = WellAllocationService.GetReservoirsByAssetId(assets.ElementAt(1).Id.ToString());
                var R1A1_ID = ReservoirIDsOfAsset1.Values.ElementAt(0).Id;
                var R1A2_ID = ReservoirIDsOfAsset2.Values.ElementAt(0).Id;
                #endregion getReservoir IDs

                #region create Zone 
                //Zone under Asset 1
                ZoneAndUnitsDTO Z1R1A1 = AddZoneToReservoir("Z1R1A1", R1A1_ID);
                ZoneAndUnitsDTO Z1R1A2 = AddZoneToReservoir("Z1R1A2", R1A2_ID);
                #endregion create Zone 

                #region getZone IDs
                ZoneArrayAndUnitsDTO ZoneIDsOfR1A1 = WellAllocationService.GetZonesByReservoirId(R1A1_ID.ToString());
                ZoneArrayAndUnitsDTO ZoneIDsOfR1A2 = WellAllocationService.GetZonesByReservoirId(R1A2_ID.ToString());
                #endregion getZone IDs

                #region Assign Well to Zone
                //Assign Well under Asset 1
                WellToZoneDTO W1Z1R1A1 = AddWellToZone(NFWell_Con_1.Id, ZoneIDsOfR1A1.Values.ElementAt(0).Id);
                WellToZoneDTO W2Z1R1A1 = AddWellToZone(NFWell_Dry_1.Id, ZoneIDsOfR1A1.Values.ElementAt(0).Id);
                WellToZoneDTO W3Z1R1A1 = AddWellToZone(GASInjWell_1.Id, ZoneIDsOfR1A1.Values.ElementAt(0).Id);
                WellToZoneDTO W4Z1R1A1 = AddWellToZone(WATERInjWell_1.Id, ZoneIDsOfR1A1.Values.ElementAt(0).Id);

                WellToZoneDTO W1Z1R1A2 = AddWellToZone(NFWell_Con_2.Id, ZoneIDsOfR1A2.Values.ElementAt(0).Id);
                WellToZoneDTO W2Z1R1A2 = AddWellToZone(NFWell_Dry_2.Id, ZoneIDsOfR1A2.Values.ElementAt(0).Id);
                WellToZoneDTO W3Z1R1A2 = AddWellToZone(GASInjWell_2.Id, ZoneIDsOfR1A2.Values.ElementAt(0).Id);
                WellToZoneDTO W4Z1R1A2 = AddWellToZone(WATERInjWell_2.Id, ZoneIDsOfR1A2.Values.ElementAt(0).Id);
                #endregion Assign Well to Zone

                #region create Pattern 
                // Pattern under Zone 1 for Asset 1
                PatternDTO addPattern1 = AddPatternToZoneOfReservoir("Z1Pattern1", ZoneIDsOfR1A1.Values.ElementAt(0).Id);
                #endregion create Pattern

                #region Assign Well to Pattern
                // Get Pattern Ids based on Zone Assignment      
                List<PatternDTO> getPatternByZoneOfReservoir = WellAllocationService.GetPatternsByZoneId(ZoneIDsOfR1A1.Values.ElementAt(0).Id.ToString());
                // Assign Well to Pattern
                WellToPatternDTO assignWellToPattern = AddWellToPattern(NFWell_Con_3.Id, getPatternByZoneOfReservoir[0].Id);
                Assert.IsNotNull(assignWellToPattern);
                #endregion Assign Well to Pattern

                #region get Default Phase Selection From Toolbox
                //Get default system setting
                var getDefaultPhaseList = GetForeSiteToolBoxSettingValue(SettingServiceStringConstants.WELL_ALLOWABLE_SNAPSHOT_CONFIGURATION);
                Assert.AreEqual("Production|Gas|True;Production|Oil|True;Production|Liquid|False;Injection|Gas|True;Injection|Water|False", getDefaultPhaseList, "Mismatch in default setting");

                //Write phase selection
                var setPhase = "Production|Gas|True;Production|Oil|True;Production|Liquid|False;Injection|Gas|True;Injection|Water|True";

                //Set modified phase selection values to system setting
                SetValuesInSystemSettings(SettingServiceStringConstants.WELL_ALLOWABLE_SNAPSHOT_CONFIGURATION, setPhase);
                var modifyPhaseList = GetForeSiteToolBoxSettingValue(SettingServiceStringConstants.WELL_ALLOWABLE_SNAPSHOT_CONFIGURATION);
                Assert.AreEqual(modifyPhaseList, setPhase, "Mismatch in phase selection");
                #endregion get Default Phase Selection From Toolbox

                #region run Scheduler API to Create Initial Snapshot

                // run scheduler command “-configureInitialSnapshot" - calling direct API
                var baseSnapshot = WellAllocationService.AddInitialWellAllowableSnapshot();
                Trace.WriteLine("Initial Snapshot Created Successfully");

                SWASnapshotDTO[] getSnapshot = WellAllocationService.GetWellAllowableSnapshots();
                _setWellAlowableToRemove.Add(getSnapshot[0]);

                #endregion run Scheduler API to Create Initial Snapshot

                #region verify Initial Snapshot Creation

                #region snapshot Information
                Assert.AreEqual("Initial Snapshot", getSnapshot[0].Name, "Mismatch in Snapshot Name");
                Assert.AreEqual("Target", getSnapshot[0].SnapshotType.ToString(), "Mismatch in Snapshot Type");
                Assert.AreEqual("Unapproved", getSnapshot[0].ApprovalStatus.ToString(), "Mismatch in Approval Status");
                #endregion snapshot Information

                #region get WellBound Records For Production Category
                //Get Well Bound Records For Asset 1 - Production Category with Gas
                SWAWellBasedDTO getWellBoundForAsset1_Prod_Gas = getWellBoundRecordsForSnapshot(getSnapshot[0].Id, assets.ElementAt(0).Id, WellTypeCategory.Production, WellAllowablePhase.Gas);
                SWARateArrayAndUnitsDTO getProdWellBoundDataForAsset1 = WellAllocationService.GetWellAllowableRatesForSnapshot(getWellBoundForAsset1_Prod_Gas);
                Assert.AreEqual(3, getProdWellBoundDataForAsset1.Values.Count(), "Mismatch Count for Well Bound Records For Asset1 From Initial Snapshot");
                Assert.AreEqual("NFWell_Con_1", getProdWellBoundDataForAsset1.Values[0].Well.Name, "Mismatch In Well Name For 1st Well Bound Records For Asset1 From Initial Snapshot");
                Assert.AreEqual("Z1R1A1", getProdWellBoundDataForAsset1.Values[0].Zone.Value.ZoneName, "Mismatch In Zone Name For 1st Well Bound Records For Asset1 From Initial Snapshot");
                Assert.AreEqual("NFWell_Con_1_Field", getProdWellBoundDataForAsset1.Values[0].TrunkLine.Value.Name, "Mismatch In Trunk Line Name For 1st Well Bound Records For Asset1 From Initial Snapshot");
                Assert.AreEqual("NFWell_Dry_1", getProdWellBoundDataForAsset1.Values[1].Well.Name, "Mismatch In Well Name For 2nd Well Bound Records For Asset1 From Initial Snapshot");
                Assert.AreEqual("Z1R1A1", getProdWellBoundDataForAsset1.Values[1].Zone.Value.ZoneName, "Mismatch In Zone Name For 2nd Well Bound Records For Asset1 From Initial Snapshot");
                Assert.AreEqual("NFWell_Dry_1_Field", getProdWellBoundDataForAsset1.Values[1].TrunkLine.Value.Name, "Mismatch In Trunk Line Name For 2nd Well Bound Records For Asset1 From Initial Snapshot");
                //------Pattern Well verification -- It should reflect as active well
                Assert.AreEqual("NFWell_Con_3", getProdWellBoundDataForAsset1.Values[2].Well.Name, "Mismatch In Well Name For 3rd Well Bound Records For Asset1 From Initial Snapshot");
                Assert.AreEqual("Z1R1A1", getProdWellBoundDataForAsset1.Values[2].Zone.Value.ZoneName, "Mismatch In Zone Name For 3rd Well Bound Records For Asset1 From Initial Snapshot");
                Assert.AreEqual("NFWell_Con_3_Field", getProdWellBoundDataForAsset1.Values[2].TrunkLine.Value.Name, "Mismatch In Trunk Line Name For 3rd Well Bound Records For Asset1 From Initial Snapshot");

                //Get Well Bound Records For Asset 1 - Production Category with Oil
                SWAWellBasedDTO getWellBoundForAsset1_Prod_Oil = getWellBoundRecordsForSnapshot(getSnapshot[0].Id, assets.ElementAt(0).Id, WellTypeCategory.Production, WellAllowablePhase.Oil);
                SWARateArrayAndUnitsDTO getProdWellBoundDataForAsset1_1 = WellAllocationService.GetWellAllowableRatesForSnapshot(getWellBoundForAsset1_Prod_Oil);
                Assert.AreEqual(3, getProdWellBoundDataForAsset1_1.Values.Count(), "Mismatch Count for Well Bound Records For Asset1 From Initial Snapshot");
                Assert.AreEqual("NFWell_Con_1", getProdWellBoundDataForAsset1_1.Values[0].Well.Name, "Mismatch In Well Name For 1st Well Bound Records For Asset1 From Initial Snapshot");
                Assert.AreEqual("Z1R1A1", getProdWellBoundDataForAsset1_1.Values[0].Zone.Value.ZoneName, "Mismatch In Zone Name For 1st Well Bound Records For Asset1 From Initial Snapshot");
                Assert.AreEqual("NFWell_Con_1_Field", getProdWellBoundDataForAsset1_1.Values[0].TrunkLine.Value.Name, "Mismatch In Trunk Line Name For 1st Well Bound Records For Asset1 From Initial Snapshot");
                Assert.AreEqual("NFWell_Dry_1", getProdWellBoundDataForAsset1_1.Values[1].Well.Name, "Mismatch In Well Name For 2nd Well Bound Records For Asset1 From Initial Snapshot");
                Assert.AreEqual("Z1R1A1", getProdWellBoundDataForAsset1_1.Values[1].Zone.Value.ZoneName, "Mismatch In Zone Name For 2nd Well Bound Records For Asset1 From Initial Snapshot");
                Assert.AreEqual("NFWell_Dry_1_Field", getProdWellBoundDataForAsset1_1.Values[1].TrunkLine.Value.Name, "Mismatch In Trunk Line Name For 2nd Well Bound Records For Asset1 From Initial Snapshot");
                //------Pattern Well verification -- It should reflect as active well
                Assert.AreEqual("NFWell_Con_3", getProdWellBoundDataForAsset1_1.Values[2].Well.Name, "Mismatch In Well Name For 3rd Well Bound Records For Asset1 From Initial Snapshot");
                Assert.AreEqual("Z1R1A1", getProdWellBoundDataForAsset1_1.Values[2].Zone.Value.ZoneName, "Mismatch In Zone Name For 3rd Well Bound Records For Asset1 From Initial Snapshot");
                Assert.AreEqual("NFWell_Con_3_Field", getProdWellBoundDataForAsset1_1.Values[2].TrunkLine.Value.Name, "Mismatch In Trunk Line Name For 3rd Well Bound Records For Asset1 From Initial Snapshot");


                //Get Well Bound Records For Asset 2 - Production Category with Gas
                SWAWellBasedDTO getWellBoundForAsset2_Prod_Gas = getWellBoundRecordsForSnapshot(getSnapshot[0].Id, assets.ElementAt(1).Id, WellTypeCategory.Production, WellAllowablePhase.Gas);
                SWARateArrayAndUnitsDTO getProdWellBoundDataForAsset2 = WellAllocationService.GetWellAllowableRatesForSnapshot(getWellBoundForAsset2_Prod_Gas);
                Assert.AreEqual(2, getProdWellBoundDataForAsset2.Values.Count(), "Mismatch Count for Well Bound Records For Asset2 From Initial Snapshot");
                Assert.AreEqual("NFWell_Con_2", getProdWellBoundDataForAsset2.Values[0].Well.Name, "Mismatch In Well Name For 1st Well Bound Records For Asset2 From Initial Snapshot");
                Assert.AreEqual("Z1R1A2", getProdWellBoundDataForAsset2.Values[0].Zone.Value.ZoneName, "Mismatch In Zone Name For 1st Well Bound Records For Asset2 From Initial Snapshot");
                Assert.AreEqual("NFWell_Con_2_Field", getProdWellBoundDataForAsset2.Values[0].TrunkLine.Value.Name, "Mismatch In Trunk Line Name For 1st Well Bound Records For Asset2 From Initial Snapshot");
                Assert.AreEqual("NFWell_Dry_2", getProdWellBoundDataForAsset2.Values[1].Well.Name, "Mismatch In Well Name For 2nd Well Bound Records For Asset2 From Initial Snapshot");
                Assert.AreEqual("Z1R1A2", getProdWellBoundDataForAsset2.Values[1].Zone.Value.ZoneName, "Mismatch In Zone Name For 2nd Well Bound Records For Asset2 From Initial Snapshot");
                Assert.AreEqual("NFWell_Dry_2_Field", getProdWellBoundDataForAsset2.Values[1].TrunkLine.Value.Name, "Mismatch In Trunk Line Name For 2nd Well Bound Records For Asset2 From Initial Snapshot");

                //Get Well Bound Records For Asset 2 - Production Category with Oil
                SWAWellBasedDTO getWellBoundForAsset2_Prod_Oil = getWellBoundRecordsForSnapshot(getSnapshot[0].Id, assets.ElementAt(1).Id, WellTypeCategory.Production, WellAllowablePhase.Oil);
                SWARateArrayAndUnitsDTO getProdWellBoundDataForAsset2_1 = WellAllocationService.GetWellAllowableRatesForSnapshot(getWellBoundForAsset2_Prod_Oil);
                Assert.AreEqual(2, getProdWellBoundDataForAsset2_1.Values.Count(), "Mismatch Count for Well Bound Records For Asset2 From Initial Snapshot");
                Assert.AreEqual("NFWell_Con_2", getProdWellBoundDataForAsset2_1.Values[0].Well.Name, "Mismatch In Well Name For 1st Well Bound Records For Asset2 From Initial Snapshot");
                Assert.AreEqual("Z1R1A2", getProdWellBoundDataForAsset2_1.Values[0].Zone.Value.ZoneName, "Mismatch In Zone Name For 1st Well Bound Records For Asset2 From Initial Snapshot");
                Assert.AreEqual("NFWell_Con_2_Field", getProdWellBoundDataForAsset2_1.Values[0].TrunkLine.Value.Name, "Mismatch In Trunk Line Name For 1st Well Bound Records For Asset2 From Initial Snapshot");
                Assert.AreEqual("NFWell_Dry_2", getProdWellBoundDataForAsset2_1.Values[1].Well.Name, "Mismatch In Well Name For 2nd Well Bound Records For Asset2 From Initial Snapshot");
                Assert.AreEqual("Z1R1A2", getProdWellBoundDataForAsset2_1.Values[1].Zone.Value.ZoneName, "Mismatch In Zone Name For 2nd Well Bound Records For Asset2 From Initial Snapshot");
                Assert.AreEqual("NFWell_Dry_2_Field", getProdWellBoundDataForAsset2.Values[1].TrunkLine.Value.Name, "Mismatch In Trunk Line Name For 2nd Well Bound Records For Asset2 From Initial Snapshot");

                #endregion get WellBound Records For Production Category

                #region get AssetBound Records For Production Category
                //Get Asset Bound Records For Asset 1 - Production Category with Gas - Zone detail
                SWAAssetBasedRequestDTO getAssetBoundForAsset1_Prod_Gas = getAssetBoundRecordsForSnapshot(getSnapshot[0].Id, assets.ElementAt(0).Id, WellTypeCategory.Production, WellAllowablePhase.Gas);
                SWAAssetBasedDTO getProdAssetBoundDataForAsset1 = WellAllocationService.GetWellAllowableAssetBasedSnapShot(getAssetBoundForAsset1_Prod_Gas);
                Assert.AreEqual(1, getProdAssetBoundDataForAsset1.SWAAssetZonesDto.Values.Count(), "Mismatch Count for Asset Bound Records For Asset1 Zone Setting From Initial Snapshot");
                Assert.AreEqual("Z1R1A1", getProdAssetBoundDataForAsset1.SWAAssetZonesDto.Values[0].ZoneName, "Mismatch In Zone Name For 1st Asset Bound Records For Asset1 From Initial Snapshot");

                //Get Asset Bound Records For Asset 1 - Production Category with Gas - Trunkline detail
                Assert.AreEqual(3, getProdAssetBoundDataForAsset1.SWAAssetTrunkLinesDto.Values.Count(), "Mismatch Count for Asset Bound Records For Asset1 For Trunkline Setting From Initial Snapshot");
                Assert.AreEqual("NFWell_Con_1_Field", getProdAssetBoundDataForAsset1.SWAAssetTrunkLinesDto.Values[0].Name, "Mismatch In Trunkline Name For 1st Asset Bound Records For Asset1 From Initial Snapshot");
                Assert.AreEqual("NFWell_Con_3_Field", getProdAssetBoundDataForAsset1.SWAAssetTrunkLinesDto.Values[1].Name, "Mismatch In Trunkline Name For 1st Asset Bound Records For Asset1 From Initial Snapshot");
                Assert.AreEqual("NFWell_Dry_1_Field", getProdAssetBoundDataForAsset1.SWAAssetTrunkLinesDto.Values[2].Name, "Mismatch In Trunkline Name For 2nd Asset Bound Records For Asset1 From Initial Snapshot");

                //Get Asset Bound Records For Asset 1 - Production Category with Oil - Zone detail
                SWAAssetBasedRequestDTO getAssetBoundForAsset1_Prod_Oil = getAssetBoundRecordsForSnapshot(getSnapshot[0].Id, assets.ElementAt(0).Id, WellTypeCategory.Production, WellAllowablePhase.Oil);
                SWAAssetBasedDTO getProdAssetBoundDataForAsset1_1 = WellAllocationService.GetWellAllowableAssetBasedSnapShot(getAssetBoundForAsset1_Prod_Oil);
                Assert.AreEqual(1, getProdAssetBoundDataForAsset1_1.SWAAssetZonesDto.Values.Count(), "Mismatch Count for Asset Bound Records For Asset1 From Initial Snapshot");
                Assert.AreEqual("Z1R1A1", getProdAssetBoundDataForAsset1_1.SWAAssetZonesDto.Values[0].ZoneName, "Mismatch In Zone Name For 1st Asset Bound Records For Asset1 From Initial Snapshot");

                //Get Asset Bound Records For Asset 1 - Production Category with Gas - Trunkline detail
                Assert.AreEqual(3, getProdAssetBoundDataForAsset1_1.SWAAssetTrunkLinesDto.Values.Count(), "Mismatch Count for Asset Bound Records For Asset1 For Trunkline Setting From Initial Snapshot");
                Assert.AreEqual("NFWell_Con_1_Field", getProdAssetBoundDataForAsset1_1.SWAAssetTrunkLinesDto.Values[0].Name, "Mismatch In Trunkline Name For 1st Asset Bound Records For Asset1 From Initial Snapshot");
                Assert.AreEqual("NFWell_Con_3_Field", getProdAssetBoundDataForAsset1_1.SWAAssetTrunkLinesDto.Values[1].Name, "Mismatch In Trunkline Name For 1st Asset Bound Records For Asset1 From Initial Snapshot");
                Assert.AreEqual("NFWell_Dry_1_Field", getProdAssetBoundDataForAsset1_1.SWAAssetTrunkLinesDto.Values[2].Name, "Mismatch In Trunkline Name For 2nd Asset Bound Records For Asset1 From Initial Snapshot");

                //Get Asset Bound Records For Asset 2 - Production Category with Gas - Zone detail
                SWAAssetBasedRequestDTO getAssetBoundForAsset2_Prod_Gas = getAssetBoundRecordsForSnapshot(getSnapshot[0].Id, assets.ElementAt(1).Id, WellTypeCategory.Production, WellAllowablePhase.Gas);
                SWAAssetBasedDTO getProdAssetBoundDataForAsset2 = WellAllocationService.GetWellAllowableAssetBasedSnapShot(getAssetBoundForAsset2_Prod_Gas);
                Assert.AreEqual(1, getProdAssetBoundDataForAsset2.SWAAssetZonesDto.Values.Count(), "Mismatch Count for Asset Bound Records For Asset2 From Initial Snapshot");
                Assert.AreEqual("Z1R1A2", getProdAssetBoundDataForAsset2.SWAAssetZonesDto.Values[0].ZoneName, "Mismatch In Zone Name For 1st Asset Bound Records For Asset2 From Initial Snapshot");

                //Get Asset Bound Records For Asset 2 - Production Category with Gas - Trunkline detail
                Assert.AreEqual(2, getProdAssetBoundDataForAsset2.SWAAssetTrunkLinesDto.Values.Count(), "Mismatch Count for Asset Bound Records For Asset1 For Trunkline Setting From Initial Snapshot");
                Assert.AreEqual("NFWell_Con_2_Field", getProdAssetBoundDataForAsset2.SWAAssetTrunkLinesDto.Values[0].Name, "Mismatch In Trunkline Name For 1st Asset Bound Records For Asset1 From Initial Snapshot");
                Assert.AreEqual("NFWell_Dry_2_Field", getProdAssetBoundDataForAsset2.SWAAssetTrunkLinesDto.Values[1].Name, "Mismatch In Trunkline Name For 2nd Asset Bound Records For Asset1 From Initial Snapshot");

                //Get Asset Bound Records For Asset 2 - Production Category with Oil - Zone detail
                SWAAssetBasedRequestDTO getAssetBoundForAsset2_Prod_Oil = getAssetBoundRecordsForSnapshot(getSnapshot[0].Id, assets.ElementAt(1).Id, WellTypeCategory.Production, WellAllowablePhase.Oil);
                SWAAssetBasedDTO getProdAssetBoundDataForAsset2_1 = WellAllocationService.GetWellAllowableAssetBasedSnapShot(getAssetBoundForAsset2_Prod_Oil);
                Assert.AreEqual(1, getProdAssetBoundDataForAsset2_1.SWAAssetZonesDto.Values.Count(), "Mismatch Count for Asset Bound Records For Asset2 From Initial Snapshot");
                Assert.AreEqual("Z1R1A2", getProdAssetBoundDataForAsset2_1.SWAAssetZonesDto.Values[0].ZoneName, "Mismatch In Zone Name For 1st Asset Bound Records For Asset2 From Initial Snapshot");

                //Get Asset Bound Records For Asset 2 - Production Category with Oil - Trunkline detail
                Assert.AreEqual(2, getProdAssetBoundDataForAsset2_1.SWAAssetTrunkLinesDto.Values.Count(), "Mismatch Count for Asset Bound Records For Asset1 For Trunkline Setting From Initial Snapshot");
                Assert.AreEqual("NFWell_Con_2_Field", getProdAssetBoundDataForAsset2_1.SWAAssetTrunkLinesDto.Values[0].Name, "Mismatch In Trunkline Name For 1st Asset Bound Records For Asset1 From Initial Snapshot");
                Assert.AreEqual("NFWell_Dry_2_Field", getProdAssetBoundDataForAsset2_1.SWAAssetTrunkLinesDto.Values[1].Name, "Mismatch In Trunkline Name For 2nd Asset Bound Records For Asset1 From Initial Snapshot");

                #endregion get AssetBound Records For Production Category

                #region get WellBound Records For Injection Category
                //Get Well Bound Records For Asset 1 - Injection Category with Gas
                SWAWellBasedDTO getWellBoundForAsset1_Inj_Gas = getWellBoundRecordsForSnapshot(getSnapshot[0].Id, assets.ElementAt(0).Id, WellTypeCategory.Injection, WellAllowablePhase.Gas);
                SWARateArrayAndUnitsDTO getInjWellBoundDataForAsset1 = WellAllocationService.GetWellAllowableRatesForSnapshot(getWellBoundForAsset1_Inj_Gas);
                Assert.AreEqual(1, getInjWellBoundDataForAsset1.Values.Count(), "Mismatch Count for Well Bound Records For Asset1 From Initial Snapshot");
                Assert.AreEqual("GASInjWell_1", getInjWellBoundDataForAsset1.Values[0].Well.Name, "Mismatch In Well Name For 1st Well Bound Records For Asset1 From Initial Snapshot");
                Assert.AreEqual("Z1R1A1", getInjWellBoundDataForAsset1.Values[0].Zone.Value.ZoneName, "Mismatch In Zone Name For 1st Well Bound Records For Asset1 From Initial Snapshot");
                Assert.AreEqual("GASInjWell_1_Field", getInjWellBoundDataForAsset1.Values[0].TrunkLine.Value.Name, "Mismatch In Trunk Line Name For 1st Well Bound Records For Asset1 From Initial Snapshot");

                //Get Well Bound Records For Asset 1 - Production Category with Water
                SWAWellBasedDTO getWellBoundForAsset1_Inj_Water = getWellBoundRecordsForSnapshot(getSnapshot[0].Id, assets.ElementAt(0).Id, WellTypeCategory.Injection, WellAllowablePhase.Water);
                SWARateArrayAndUnitsDTO getInjWellBoundDataForAsset1_1 = WellAllocationService.GetWellAllowableRatesForSnapshot(getWellBoundForAsset1_Inj_Water);
                Assert.AreEqual(1, getInjWellBoundDataForAsset1_1.Values.Count(), "Mismatch Count for Well Bound Records For Asset1 From Initial Snapshot");
                Assert.AreEqual("WATERInjWell_1", getInjWellBoundDataForAsset1_1.Values[0].Well.Name, "Mismatch In Well Name For 1st Well Bound Records For Asset1 From Initial Snapshot");
                Assert.AreEqual("Z1R1A1", getInjWellBoundDataForAsset1_1.Values[0].Zone.Value.ZoneName, "Mismatch In Zone Name For 1st Well Bound Records For Asset1 From Initial Snapshot");
                Assert.AreEqual("WATERInjWell_1_Field", getInjWellBoundDataForAsset1_1.Values[0].TrunkLine.Value.Name, "Mismatch In Trunk Line Name For 1st Well Bound Records For Asset1 From Initial Snapshot");

                //Get Well Bound Records For Asset 2 - Injection Category with Gas
                SWAWellBasedDTO getWellBoundForAsset2_Inj_Gas = getWellBoundRecordsForSnapshot(getSnapshot[0].Id, assets.ElementAt(1).Id, WellTypeCategory.Injection, WellAllowablePhase.Gas);
                SWARateArrayAndUnitsDTO getInjWellBoundDataForAsset2 = WellAllocationService.GetWellAllowableRatesForSnapshot(getWellBoundForAsset2_Inj_Gas);
                Assert.AreEqual(1, getInjWellBoundDataForAsset2.Values.Count(), "Mismatch Count for Well Bound Records For Asset1 From Initial Snapshot");
                Assert.AreEqual("GASInjWell_2", getInjWellBoundDataForAsset2.Values[0].Well.Name, "Mismatch In Well Name For 1st Well Bound Records For Asset1 From Initial Snapshot");
                Assert.AreEqual("Z1R1A2", getInjWellBoundDataForAsset2.Values[0].Zone.Value.ZoneName, "Mismatch In Zone Name For 1st Well Bound Records For Asset1 From Initial Snapshot");
                Assert.AreEqual("GASInjWell_2_Field", getInjWellBoundDataForAsset2.Values[0].TrunkLine.Value.Name, "Mismatch In Trunk Line Name For 1st Well Bound Records For Asset1 From Initial Snapshot");

                //Get Well Bound Records For Asset 2 - Production Category with Water
                SWAWellBasedDTO getWellBoundForAsset2_Inj_Water = getWellBoundRecordsForSnapshot(getSnapshot[0].Id, assets.ElementAt(1).Id, WellTypeCategory.Injection, WellAllowablePhase.Water);
                SWARateArrayAndUnitsDTO getInjWellBoundDataForAsset2_1 = WellAllocationService.GetWellAllowableRatesForSnapshot(getWellBoundForAsset2_Inj_Water);
                Assert.AreEqual(1, getInjWellBoundDataForAsset2_1.Values.Count(), "Mismatch Count for Well Bound Records For Asset1 From Initial Snapshot");
                Assert.AreEqual("WATERInjWell_2", getInjWellBoundDataForAsset2_1.Values[0].Well.Name, "Mismatch In Well Name For 1st Well Bound Records For Asset1 From Initial Snapshot");
                Assert.AreEqual("Z1R1A2", getInjWellBoundDataForAsset2_1.Values[0].Zone.Value.ZoneName, "Mismatch In Zone Name For 1st Well Bound Records For Asset1 From Initial Snapshot");
                Assert.AreEqual("WATERInjWell_2_Field", getInjWellBoundDataForAsset2_1.Values[0].TrunkLine.Value.Name, "Mismatch In Trunk Line Name For 1st Well Bound Records For Asset1 From Initial Snapshot");

                #endregion get WellBound Records For Injection Category

                #region get AssetBound Records For Injection Category
                //Get Asset Bound Records For Asset 1 - Injection Category with Gas - Zone detail
                SWAAssetBasedRequestDTO getAssetBoundForAsset1_Inj_Gas = getAssetBoundRecordsForSnapshot(getSnapshot[0].Id, assets.ElementAt(0).Id, WellTypeCategory.Injection, WellAllowablePhase.Gas);
                SWAAssetBasedDTO getInjAssetBoundDataForAsset1 = WellAllocationService.GetWellAllowableAssetBasedSnapShot(getAssetBoundForAsset1_Inj_Gas);
                Assert.AreEqual(1, getInjAssetBoundDataForAsset1.SWAAssetZonesDto.Values.Count(), "Mismatch Count for Asset Bound Records For Asset1 Zone Setting From Initial Snapshot");
                Assert.AreEqual("Z1R1A1", getInjAssetBoundDataForAsset1.SWAAssetZonesDto.Values[0].ZoneName, "Mismatch In Zone Name For 1st Asset Bound Records For Asset1 From Initial Snapshot");

                //Get Asset Bound Records For Asset 1 - Inection Category with Gas - Trunkline detail
                Assert.AreEqual(1, getInjAssetBoundDataForAsset1.SWAAssetTrunkLinesDto.Values.Count(), "Mismatch Count for Asset Bound Records For Asset1 For Trunkline Setting From Initial Snapshot");
                Assert.AreEqual("GASInjWell_1_Field", getInjAssetBoundDataForAsset1.SWAAssetTrunkLinesDto.Values[0].Name, "Mismatch In Trunkline Name For 1st Asset Bound Records For Asset1 From Initial Snapshot");

                //Get Asset Bound Records For Asset 2 - Injection Category with Gas - Zone detail
                SWAAssetBasedRequestDTO getAssetBoundForAsset2_Inj_Gas = getAssetBoundRecordsForSnapshot(getSnapshot[0].Id, assets.ElementAt(1).Id, WellTypeCategory.Injection, WellAllowablePhase.Gas);
                SWAAssetBasedDTO getInjAssetBoundDataForAsset2 = WellAllocationService.GetWellAllowableAssetBasedSnapShot(getAssetBoundForAsset2_Inj_Gas);
                Assert.AreEqual(1, getInjAssetBoundDataForAsset2.SWAAssetZonesDto.Values.Count(), "Mismatch Count for Asset Bound Records For Asset2 From Initial Snapshot");
                Assert.AreEqual("Z1R1A2", getInjAssetBoundDataForAsset2.SWAAssetZonesDto.Values[0].ZoneName, "Mismatch In Zone Name For 1st Asset Bound Records For Asset2 From Initial Snapshot");

                //Get Asset Bound Records For Asset 2 - Injection Category with Gas - Trunkline detail
                Assert.AreEqual(1, getInjAssetBoundDataForAsset2.SWAAssetTrunkLinesDto.Values.Count(), "Mismatch Count for Asset Bound Records For Asset1 For Trunkline Setting From Initial Snapshot");
                Assert.AreEqual("GASInjWell_2_Field", getInjAssetBoundDataForAsset2.SWAAssetTrunkLinesDto.Values[0].Name, "Mismatch In Trunkline Name For 1st Asset Bound Records For Asset1 From Initial Snapshot");

                ////-----------------------
                //Get Asset Bound Records For Asset 1 - Injection Category with Water - Zone detail
                SWAAssetBasedRequestDTO getAssetBoundForAsset1_Inj_Water = getAssetBoundRecordsForSnapshot(getSnapshot[0].Id, assets.ElementAt(0).Id, WellTypeCategory.Injection, WellAllowablePhase.Water);
                SWAAssetBasedDTO getInjAssetBoundDataForAsset1_1 = WellAllocationService.GetWellAllowableAssetBasedSnapShot(getAssetBoundForAsset1_Inj_Water);
                Assert.AreEqual(1, getInjAssetBoundDataForAsset1_1.SWAAssetZonesDto.Values.Count(), "Mismatch Count for Asset Bound Records For Asset1 Zone Setting From Initial Snapshot");
                Assert.AreEqual("Z1R1A1", getInjAssetBoundDataForAsset1_1.SWAAssetZonesDto.Values[0].ZoneName, "Mismatch In Zone Name For 1st Asset Bound Records For Asset1 From Initial Snapshot");

                //Get Asset Bound Records For Asset 1 - Inection Category with Water - Trunkline detail
                Assert.AreEqual(1, getInjAssetBoundDataForAsset1_1.SWAAssetTrunkLinesDto.Values.Count(), "Mismatch Count for Asset Bound Records For Asset1 For Trunkline Setting From Initial Snapshot");
                Assert.AreEqual("WATERInjWell_1_Field", getInjAssetBoundDataForAsset1_1.SWAAssetTrunkLinesDto.Values[0].Name, "Mismatch In Trunkline Name For 1st Asset Bound Records For Asset1 From Initial Snapshot");

                //Get Asset Bound Records For Asset 2 - Injection Category with Water - Zone detail
                SWAAssetBasedRequestDTO getAssetBoundForAsset2_Inj_Water = getAssetBoundRecordsForSnapshot(getSnapshot[0].Id, assets.ElementAt(1).Id, WellTypeCategory.Injection, WellAllowablePhase.Water);
                SWAAssetBasedDTO getInjAssetBoundDataForAsset2_1 = WellAllocationService.GetWellAllowableAssetBasedSnapShot(getAssetBoundForAsset2_Inj_Water);
                Assert.AreEqual(1, getInjAssetBoundDataForAsset2_1.SWAAssetZonesDto.Values.Count(), "Mismatch Count for Asset Bound Records For Asset2 From Initial Snapshot");
                Assert.AreEqual("Z1R1A2", getInjAssetBoundDataForAsset2_1.SWAAssetZonesDto.Values[0].ZoneName, "Mismatch In Zone Name For 1st Asset Bound Records For Asset2 From Initial Snapshot");

                //Get Asset Bound Records For Asset 2 - Injection Category with Water - Trunkline detail
                Assert.AreEqual(1, getInjAssetBoundDataForAsset2_1.SWAAssetTrunkLinesDto.Values.Count(), "Mismatch Count for Asset Bound Records For Asset1 For Trunkline Setting From Initial Snapshot");
                Assert.AreEqual("WATERInjWell_2_Field", getInjAssetBoundDataForAsset2_1.SWAAssetTrunkLinesDto.Values[0].Name, "Mismatch In Trunkline Name For 1st Asset Bound Records For Asset1 From Initial Snapshot");

                #endregion get AssetBound Records For Injection Category

                #endregion verify Initial Snapshot Creation
            }
            finally
            {
                var resetPhase = "Production|Gas|True;Production|Oil|True;Production|Liquid|False;Injection|Gas|True;Injection|Water|False";
                //Set default phase selection values to system setting
                SetValuesInSystemSettings(SettingServiceStringConstants.WELL_ALLOWABLE_SNAPSHOT_CONFIGURATION, resetPhase);

                ChangeUnitSystem("US");
                ChangeUnitSystemUserSetting("US");
            }
        }

        /// <summary>
        /// FRWM_7214_SWA:Enable the NetworkScenario ID to be an optional during input during the creation of SWAAssetSettings record
        /// </summary>
        [TestCategory(TestCategories.WellAllocationServiceTests), TestMethod]
        public void SWASanpshotCreationWithoutScenario()
        {
            ChangeUnitSystem("US");
            ChangeUnitSystemUserSetting("US");

            #region  Add Asset
            List<AssetDTO> assets = CreateAsset(2);
            #endregion Add Asset

            SWASnapshotDTO snapshotProductionAsset1 = new SWASnapshotDTO();
            SWASnapshotDTO snapshotProductionAsset2 = new SWASnapshotDTO();
            SWASnapshotDTO snapshotInjectionAsset1 = new SWASnapshotDTO();
            SWASnapshotDTO snapshotInjectionAsset2 = new SWASnapshotDTO();

            try
            {
                #region create Production Wells with Asset
                //NFW Condennsate wells creation
                var modelFileName_cond = "Condensate Gas - IPR Auto Tuning.wflx";
                var options_cond = new ModelFileOptionDTO
                {
                    CalibrationMethod = CalibrationMethodId.ReservoirPressure,
                    OptionalUpdate = new long[]
                      {
                        (long) OptionalUpdates.UpdateWCT_WGR,
                        (long) OptionalUpdates.UpdateGOR_CGR
                      }
                };

                WellDTO NFWell_Con_1 = AddNonRRLWellGeneral("NFWell_Con_1", GetFacilityId("NFWELL_", 1), WellTypeId.NF, WellFluidType.Condensate, WellFluidPhase.None, "1", assets.ElementAt(0).Id);
                AddNonRRLModelFile(NFWell_Con_1, modelFileName_cond, options_cond.CalibrationMethod, options_cond.OptionalUpdate);

                WellDTO NFWell_Con_2 = AddNonRRLWellGeneral("NFWell_Con_2", GetFacilityId("NFWELL_", 2), WellTypeId.NF, WellFluidType.Condensate, WellFluidPhase.None, "1", assets.ElementAt(1).Id);
                AddNonRRLModelFile(NFWell_Con_2, modelFileName_cond, options_cond.CalibrationMethod, options_cond.OptionalUpdate);

                //NFW Drygas wells creation
                var modelFileName_Drygas = "Dry Gas - IPR Auto Tuning.wflx";
                var options_Drygas = new ModelFileOptionDTO
                {
                    CalibrationMethod = CalibrationMethodId.ReservoirPressure,
                    OptionalUpdate = new long[]
                      {
                        (long) OptionalUpdates.UpdateWCT_WGR,
                        //(long) OptionalUpdates.UpdateGOR_CGR
                      }
                };
                WellDTO NFWell_Dry_1 = AddNonRRLWellGeneral("NFWell_Dry_1", GetFacilityId("NFWELL_", 3), WellTypeId.NF, WellFluidType.DryGas, WellFluidPhase.None, "1", assets.ElementAt(0).Id);
                AddNonRRLModelFile(NFWell_Dry_1, modelFileName_Drygas, options_Drygas.CalibrationMethod, options_Drygas.OptionalUpdate);

                WellDTO NFWell_Dry_2 = AddNonRRLWellGeneral("NFWell_Dry_2", GetFacilityId("NFWELL_", 4), WellTypeId.NF, WellFluidType.DryGas, WellFluidPhase.None, "1", assets.ElementAt(1).Id);
                AddNonRRLModelFile(NFWell_Dry_2, modelFileName_Drygas, options_Drygas.CalibrationMethod, options_Drygas.OptionalUpdate);
                #endregion create Production Wells with Asset

                #region create Injection Wells with Asset
                WellDTO GASInjWell_1 = AddNonRRLWellGeneral("GASInjWell_1", GetFacilityId("GASINJWELL_", 1), WellTypeId.GInj, WellFluidType.None, WellFluidPhase.None, "1", assets.ElementAt(0).Id);
                AddModelFile(GASInjWell_1.CommissionDate.Value, WellTypeId.GInj, GASInjWell_1.Id, CalibrationMethodId.LFactor);
                WellDTO GASInjWell_2 = AddNonRRLWellGeneral("GASInjWell_2", GetFacilityId("GASINJWELL_", 2), WellTypeId.GInj, WellFluidType.None, WellFluidPhase.None, "1", assets.ElementAt(1).Id);
                AddModelFile(GASInjWell_2.CommissionDate.Value, WellTypeId.GInj, GASInjWell_2.Id, CalibrationMethodId.LFactor);

                WellDTO WATERInjWell_1 = AddNonRRLWellGeneral("WATERInjWell_1", GetFacilityId("WATERINJWELL_", 1), WellTypeId.WInj, WellFluidType.None, WellFluidPhase.None, "1", assets.ElementAt(0).Id);
                AddModelFile(WATERInjWell_1.CommissionDate.Value, WellTypeId.WInj, WATERInjWell_1.Id, CalibrationMethodId.LFactor);
                WellDTO WATERInjWell_2 = AddNonRRLWellGeneral("WATERInjWell_2", GetFacilityId("WATERINJWELL_", 2), WellTypeId.WInj, WellFluidType.None, WellFluidPhase.None, "1", assets.ElementAt(1).Id);
                AddModelFile(WATERInjWell_2.CommissionDate.Value, WellTypeId.WInj, WATERInjWell_2.Id, CalibrationMethodId.LFactor);

                Trace.WriteLine("Wells are created successfully and mapped with Assets");
                #endregion create Injection Wells with Asset

                #region create Reservoir
                //Reservoir under Asset 1
                ReservoirAndUnitsDTO R1A1 = AddReservoirToAsset("R1A1", assets.ElementAt(0).Id);
                _vrrHierarchyToRemove.Add(assets[0]);
                ReservoirAndUnitsDTO R1A2 = AddReservoirToAsset("R1A2", assets.ElementAt(1).Id);
                _vrrHierarchyToRemove.Add(assets[1]);
                #endregion create Reservoir

                #region getReservoir IDs
                ReservoirArrayAndUnitsDTO ReservoirIDsOfAsset1 = WellAllocationService.GetReservoirsByAssetId(assets.ElementAt(0).Id.ToString());
                ReservoirArrayAndUnitsDTO ReservoirIDsOfAsset2 = WellAllocationService.GetReservoirsByAssetId(assets.ElementAt(1).Id.ToString());
                var R1A1_ID = ReservoirIDsOfAsset1.Values.ElementAt(0).Id;
                var R1A2_ID = ReservoirIDsOfAsset2.Values.ElementAt(0).Id;
                #endregion getReservoir IDs

                #region create Zone 
                //Zone under Asset 1
                ZoneAndUnitsDTO Z1R1A1 = AddZoneToReservoir("Z1R1A1", R1A1_ID);
                ZoneAndUnitsDTO Z1R1A2 = AddZoneToReservoir("Z1R1A2", R1A2_ID);
                #endregion create Zone 

                #region getZone IDs
                ZoneArrayAndUnitsDTO ZoneIDsOfR1A1 = WellAllocationService.GetZonesByReservoirId(R1A1_ID.ToString());
                ZoneArrayAndUnitsDTO ZoneIDsOfR1A2 = WellAllocationService.GetZonesByReservoirId(R1A2_ID.ToString());
                #endregion getZone IDs

                #region Assign Well to Zone
                //Assign Well under Asset 1
                WellToZoneDTO W1Z1R1A1 = AddWellToZone(NFWell_Con_1.Id, ZoneIDsOfR1A1.Values.ElementAt(0).Id);
                WellToZoneDTO W2Z1R1A1 = AddWellToZone(NFWell_Dry_1.Id, ZoneIDsOfR1A1.Values.ElementAt(0).Id);
                WellToZoneDTO W3Z1R1A1 = AddWellToZone(GASInjWell_1.Id, ZoneIDsOfR1A1.Values.ElementAt(0).Id);
                WellToZoneDTO W4Z1R1A1 = AddWellToZone(WATERInjWell_1.Id, ZoneIDsOfR1A1.Values.ElementAt(0).Id);

                WellToZoneDTO W1Z1R1A2 = AddWellToZone(NFWell_Con_2.Id, ZoneIDsOfR1A2.Values.ElementAt(0).Id);
                WellToZoneDTO W2Z1R1A2 = AddWellToZone(NFWell_Dry_2.Id, ZoneIDsOfR1A2.Values.ElementAt(0).Id);
                WellToZoneDTO W3Z1R1A2 = AddWellToZone(GASInjWell_2.Id, ZoneIDsOfR1A2.Values.ElementAt(0).Id);
                WellToZoneDTO W4Z1R1A2 = AddWellToZone(WATERInjWell_2.Id, ZoneIDsOfR1A2.Values.ElementAt(0).Id);
                #endregion Assign Well to Zone

                #region Set Well Allowable - Production
                string startDateProduction = DTOExtensions.ToISO8601(DateTime.Now.AddDays(-30).ToUniversalTime());
                string endDateProduction = DTOExtensions.ToISO8601(DateTime.Now.AddDays(-20).ToUniversalTime());
                string productionSnapshotNameAsset1 = "Production Snap Asset1";
                string productionSnapshotNameAsset2 = "Production Snap Asset2";
                string productionSnapshotType = WellAllowableSnapshotType.Sensitivity.ToString();

                //Create Snapshot - by Passing Scenario ID as Null and making Sure that Snapshot is creating without Scenario ID 
                //Becasue of Backpressure is optional and FK Key for Scenario ID should work with Null 
                snapshotProductionAsset1 = WellAllocationService.CreateWellAllowableSnapshot(productionSnapshotNameAsset1, productionSnapshotType, assets.ElementAt(0).Id.ToString(), startDateProduction, endDateProduction, WellTypeCategory.Production.ToString(), WellAllowablePhase.Gas.ToString(), null);
                Assert.IsNotNull(snapshotProductionAsset1);
                _setWellAlowableToRemove.Add(snapshotProductionAsset1);
                Trace.WriteLine("Production Snapshot with Asset 1 has been successfully creatred without Scenario ID");

                snapshotProductionAsset2 = WellAllocationService.CreateWellAllowableSnapshot(productionSnapshotNameAsset2, productionSnapshotType, assets.ElementAt(1).Id.ToString(), startDateProduction, endDateProduction, WellTypeCategory.Production.ToString(), WellAllowablePhase.Gas.ToString(), null);
                Assert.IsNotNull(snapshotProductionAsset2);
                _setWellAlowableToRemove.Add(snapshotProductionAsset2);
                Trace.WriteLine("Production Snapshot with Asset 2 has been successfully creatred without Scenario ID");
                #endregion Set Well Allowable - Production

                #region Set Well Allowable - Injection
                string startDateInjection = DTOExtensions.ToISO8601(DateTime.Now.AddDays(-30).ToUniversalTime());
                string endDateInjection = DTOExtensions.ToISO8601(DateTime.Now.AddDays(-20).ToUniversalTime());
                string injectionSnapshotNameAsset1 = "Injection Snap Asset1";
                string injectionSnapshotNameAsset2 = "Injection Snap Asset2";
                string injetionSnapshotType = WellAllowableSnapshotType.Target.ToString();

                //Create Snapshot - by Passing Scenario ID as Null and making Sure that Snapshot is creating without Scenario ID 
                //Becasue of Backpressure is optional and FK Key for Scenario ID should work with Null 
                snapshotInjectionAsset1 = WellAllocationService.CreateWellAllowableSnapshot(injectionSnapshotNameAsset1, injetionSnapshotType, assets.ElementAt(0).Id.ToString(), startDateInjection, endDateInjection, WellTypeCategory.Injection.ToString(), WellAllowablePhase.Gas.ToString(), null);
                Assert.IsNotNull(snapshotInjectionAsset1);
                _setWellAlowableToRemove.Add(snapshotInjectionAsset1);
                Trace.WriteLine("Injection Snapshot with Asset 1 has been successfully creatred without Scenario ID");

                snapshotInjectionAsset2 = WellAllocationService.CreateWellAllowableSnapshot(injectionSnapshotNameAsset2, injetionSnapshotType, assets.ElementAt(1).Id.ToString(), startDateInjection, endDateInjection, WellTypeCategory.Injection.ToString(), WellAllowablePhase.Gas.ToString(), null);
                Assert.IsNotNull(snapshotInjectionAsset2);
                _setWellAlowableToRemove.Add(snapshotInjectionAsset2);
                Trace.WriteLine("Injection Snapshot with Asset 2 has been successfully creatred without Scenario ID");
                #endregion Set Well Allowable - Injection
            }
            finally
            {
                ChangeUnitSystem("US");
                ChangeUnitSystemUserSetting("US");
            }
        }

        /// <summary>
        /// FRWM_7274_SWA : Snapshot Clone/Edit/Remove functionality
        /// </summary>
        [TestCategory(TestCategories.WellAllocationServiceTests), TestMethod]
        public void SWASanpshotCloneEditRemove_ProductionCategory()
        {
            ChangeUnitSystem("US");
            ChangeUnitSystemUserSetting("US");
            try
            {
                #region Asset
                List<AssetDTO> assets = CreateAsset(2);
                #endregion Asset

                #region Create Production Wells With Asset
                //NFW Condensate Well
                var modelFileName_cond = "Condensate Gas - IPR Auto Tuning.wflx";
                var options_cond = new ModelFileOptionDTO
                {
                    CalibrationMethod = CalibrationMethodId.ReservoirPressure,
                    OptionalUpdate = new long[]
                      {
                        (long) OptionalUpdates.UpdateWCT_WGR,
                        (long) OptionalUpdates.UpdateGOR_CGR
                      }
                };

                WellDTO NFWell_Con_1 = AddNonRRLWellGeneral("NFWell_Con_1", GetFacilityId("NFWELL_", 1), WellTypeId.NF, WellFluidType.Condensate, WellFluidPhase.None, "1", assets.ElementAt(0).Id);
                AddNonRRLModelFile(NFWell_Con_1, modelFileName_cond, options_cond.CalibrationMethod, options_cond.OptionalUpdate);

                WellDTO NFWell_Con_2 = AddNonRRLWellGeneral("NFWell_Con_2", GetFacilityId("NFWELL_", 2), WellTypeId.NF, WellFluidType.Condensate, WellFluidPhase.None, "1", assets.ElementAt(0).Id);
                AddNonRRLModelFile(NFWell_Con_2, modelFileName_cond, options_cond.CalibrationMethod, options_cond.OptionalUpdate);

                //NFW DryGas Well
                var modelFileName_Drygas = "Dry Gas - IPR Auto Tuning.wflx";
                var options_Drygas = new ModelFileOptionDTO
                {
                    CalibrationMethod = CalibrationMethodId.ReservoirPressure,
                    OptionalUpdate = new long[]
                      {
                        (long) OptionalUpdates.UpdateWCT_WGR,
                        //(long) OptionalUpdates.UpdateGOR_CGR
                      }
                };
                WellDTO NFWell_Dry_1 = AddNonRRLWellGeneral("NFWell_Dry_1", GetFacilityId("NFWELL_", 3), WellTypeId.NF, WellFluidType.DryGas, WellFluidPhase.None, "1", assets.ElementAt(0).Id);
                AddNonRRLModelFile(NFWell_Dry_1, modelFileName_Drygas, options_Drygas.CalibrationMethod, options_Drygas.OptionalUpdate);

                WellDTO NFWell_Dry_2 = AddNonRRLWellGeneral("NFWell_Dry_2", GetFacilityId("NFWELL_", 4), WellTypeId.NF, WellFluidType.DryGas, WellFluidPhase.None, "1", assets.ElementAt(1).Id);
                AddNonRRLModelFile(NFWell_Dry_2, modelFileName_Drygas, options_Drygas.CalibrationMethod, options_Drygas.OptionalUpdate);
                #endregion Create Production Wells With Asset

                #region Create Basic VRR Hierarchy
                //Create Reservoir for Asset 1 and get it's ID
                ReservoirAndUnitsDTO R1A1 = AddReservoirToAsset("R1A1", assets.ElementAt(0).Id);
                _vrrHierarchyToRemove.Add(assets[0]);

                ReservoirArrayAndUnitsDTO ReservoirIDsOfAsset1 = WellAllocationService.GetReservoirsByAssetId(assets.ElementAt(0).Id.ToString());
                var R1A1_ID = ReservoirIDsOfAsset1.Values.ElementAt(0).Id;

                //Create Zone 1 for Reservoir 1 / Asset 1 and get it's ID
                ZoneAndUnitsDTO Z1R1A1 = AddZoneToReservoir("Z1R1A1", R1A1_ID);
                ZoneArrayAndUnitsDTO ZoneIDsOfR1A1 = WellAllocationService.GetZonesByReservoirId(R1A1_ID.ToString());

                //Mapped Wells with Zone 1
                WellToZoneDTO W1Z1R1A1 = AddWellToZone(NFWell_Con_1.Id, ZoneIDsOfR1A1.Values.ElementAt(0).Id);
                #endregion Create Basic VRR Hierarchy

                #region Cloning Functionality

                #region Create Base Snapshot With Production Category
                string startDateProduction = DTOExtensions.ToISO8601(DateTime.Now.AddDays(-30).ToUniversalTime());
                string endDateProduction = DTOExtensions.ToISO8601(DateTime.Now.AddDays(-20).ToUniversalTime());
                string productionSnapshotNameAsset1 = "Production Snap Asset1";
                string productionSnapshotType = WellAllowableSnapshotType.Sensitivity.ToString();

                //Create Base snapshot
                SWASnapshotDTO snapshotProductionAsset1 = WellAllocationService.CreateWellAllowableSnapshot(productionSnapshotNameAsset1, productionSnapshotType, assets.ElementAt(0).Id.ToString(), startDateProduction, endDateProduction, WellTypeCategory.Production.ToString(), WellAllowablePhase.Gas.ToString(), null);
                Assert.IsNotNull(snapshotProductionAsset1);
                _setWellAlowableToRemove.Add(snapshotProductionAsset1);
                Trace.WriteLine("Base Snapshot Is Create Successfully For Production Category");

                //Get rate information for Wellbound 
                SWAWellBasedDTO wellBasedProductionDTO = new SWAWellBasedDTO();
                wellBasedProductionDTO.SnapshotId = snapshotProductionAsset1.Id;
                wellBasedProductionDTO.AssetId = assets.ElementAt(0).Id;
                wellBasedProductionDTO.WellCategory = WellTypeCategory.Production;
                wellBasedProductionDTO.FluidPhase = WellAllowablePhase.Gas;

                #region Inserting Well Bound Records For 1st Record
                SWARateArrayAndUnitsDTO ratesFor1stRecord = WellAllocationService.GetWellAllowableRatesForSnapshot(wellBasedProductionDTO);
                Assert.IsNotNull(ratesFor1stRecord);
                ratesFor1stRecord.Values[0].WellheadPressureBound = (decimal)300.5;
                ratesFor1stRecord.Values[0].PerforationPressureChangeBound = (decimal)400.5;
                ratesFor1stRecord.Values[0].ReservoirPressureBound = (decimal)1350.5;
                ratesFor1stRecord.Values[0].ReservoirPressureTolerance = (decimal)60.5;
                ratesFor1stRecord.Values[0].MaximumRate = (decimal)970.5;
                ratesFor1stRecord.Values[0].MinimumRate = (decimal)80.5;
                ratesFor1stRecord.Values[0].WellAvailability = (decimal)0.86;
                ratesFor1stRecord.Values[0].InAnalysis = true;

                wellBasedProductionDTO.Rates = ratesFor1stRecord.Values;

                //Saving 1st Wellbound record
                var updateRateFor1stRecord = WellAllocationService.UpdateWellAllowableRatesForSnapshot(wellBasedProductionDTO);
                Assert.IsNotNull(updateRateFor1stRecord);
                #endregion Inserting Well Bound Records For 1st Record

                #region Inserting Asset Bound Record
                SWAAssetBasedRequestDTO SWAAssetBasedRequestDTO = new SWAAssetBasedRequestDTO() { AssetId = assets.ElementAt(0).Id, WellCategory = WellTypeCategory.Production, SnapshotId = snapshotProductionAsset1.Id, FluidPhase = WellAllowablePhase.Gas };
                //Getting Assetbound record
                SWAAssetBasedDTO AssetBasedDTO = WellAllocationService.GetWellAllowableAssetBasedSnapShot(SWAAssetBasedRequestDTO);
                AssetBasedDTO.SWAAssetSettingsDto.Value.SystemEfficiency = (decimal?)0.95;
                AssetBasedDTO.SWAAssetSettingsDto.Value.UnplannedFactor = (decimal?)0.22;
                AssetBasedDTO.SWAAssetSettingsDto.Value.FacilityAvailability = (decimal?)0.75;

                //Adding TargetExportCondition,TargetFieldCondition & Capacity values
                for (int i = 0; i < AssetBasedDTO.SWAAssetZonesDto.Values.Length; i++)
                {
                    AssetBasedDTO.SWAAssetZonesDto.Values[i].TargetExportCondition = (decimal?)3400.55 + (i * 10);
                    //AssetBasedDTO.SWAAssetZonesDto.Values[i].TargetFieldCondition = (decimal?) 3500 + (i*10);
                }

                //Adding Trunk Line Capacity values
                for (int i = 0; i < AssetBasedDTO.SWAAssetTrunkLinesDto.Values.Length; i++)
                {
                    AssetBasedDTO.SWAAssetTrunkLinesDto.Values[i].Capacity = (decimal?)3600.45 + (i * 10);
                }

                SWAAssetSettingsDTO swasettings = new SWAAssetSettingsDTO();
                swasettings = AssetBasedDTO.SWAAssetSettingsDto.Value;
                AssetBasedDTO.SaveAssetingParameter = swasettings;
                AssetBasedDTO.SWARequestDTO = SWAAssetBasedRequestDTO;
                AssetBasedDTO.SavedAssetZoneDTOs = AssetBasedDTO.SWAAssetZonesDto.Values;
                AssetBasedDTO.SavedTrunkLineDTOs = AssetBasedDTO.SWAAssetTrunkLinesDto.Values;

                //Saving Assetbound Record
                Assert.IsNotNull(AssetBasedDTO.SaveAssetingParameter);
                WellAllocationService.SaveWellAllowableAssetBasedSnapShot(AssetBasedDTO);

                // Getting inserted Assetbound record
                SWAAssetBasedRequestDTO SWAAssetBasedRequestDTO_Base = new SWAAssetBasedRequestDTO() { AssetId = assets.ElementAt(0).Id, WellCategory = WellTypeCategory.Production, SnapshotId = snapshotProductionAsset1.Id, FluidPhase = WellAllowablePhase.Gas };
                SWAAssetBasedDTO AssetBasedDTO_Base = WellAllocationService.GetWellAllowableAssetBasedSnapShot(SWAAssetBasedRequestDTO_Base);
                Assert.IsNotNull(AssetBasedDTO_Base);
                Assert.AreEqual(1, AssetBasedDTO_Base.SWAAssetZonesDto.Values.Count(), "Mismatch in count");
                Assert.AreEqual(1, AssetBasedDTO_Base.SWAAssetTrunkLinesDto.Values.Count(), "Mismatch in count");
                Assert.AreEqual(0.95, Math.Round((double)AssetBasedDTO_Base.SWAAssetSettingsDto.Value.SystemEfficiency, 2));
                Assert.AreEqual(0.22, Math.Round((double)AssetBasedDTO_Base.SWAAssetSettingsDto.Value.UnplannedFactor, 2));
                Assert.AreEqual(0.75, Math.Round((double)AssetBasedDTO_Base.SWAAssetSettingsDto.Value.FacilityAvailability, 2));
                Assert.AreEqual(3400.6, Math.Round((double)AssetBasedDTO_Base.SWAAssetZonesDto.Values[0].TargetExportCondition, 1));
                //Assert.AreEqual(12.2, Math.Round((double)AssetBasedDTO_1.SWAAssetZonesDto.Values[0].TargetFieldCondition, 1));
                Assert.AreEqual(3600.4, Math.Round((double)AssetBasedDTO_Base.SWAAssetTrunkLinesDto.Values[0].Capacity, 1));
                #endregion Inserting Asset Bound Record

                #endregion Create Base Snapshot With Production Category

                //Updating Base snapshot managerial approval status with Approved condition - without it cloning will not work (Validation)
                SWASnapshotInputDTO baseDetail_1 = new SWASnapshotInputDTO(snapshotProductionAsset1.Id, WellAllowableSnapshotType.Target, snapshotProductionAsset1.StartDate.Value, snapshotProductionAsset1.EndDate.Value, "Production Snap Asset1");
                baseDetail_1.ApprovalStatus = WellAllowableApprovalStatus.Historized;
                WellAllocationService.UpdateWellAllowableSnapshot(baseDetail_1);

                #region Scenario 1 - Cloning
                //Creating 1st Clone snapshot from Base snapshot
                DateTime startDate_clone1 = DateTime.Now.AddDays(1).ToUniversalTime();
                DateTime endDate_clone1 = DateTime.Now.AddDays(2).ToUniversalTime();

                SWASnapshotInputDTO SnapshotDetail_Clone1 = new SWASnapshotInputDTO(snapshotProductionAsset1.Id, WellAllowableSnapshotType.Target, startDate_clone1, endDate_clone1, "Clone1FromBase");
                SWASnapshotDTO SnapshotClone1 = WellAllocationService.CloneWellAllowableSnapshot(SnapshotDetail_Clone1);
                _setWellAlowableToRemove.Add(SnapshotClone1);
                Assert.IsNotNull(SnapshotClone1);

                //Getting rate table for Cloned snapshot
                SWAWellBasedDTO wellbasedProductionDTO_Clnone1 = new SWAWellBasedDTO();
                wellbasedProductionDTO_Clnone1.SnapshotId = SnapshotClone1.Id;
                wellbasedProductionDTO_Clnone1.AssetId = assets.ElementAt(0).Id;
                wellbasedProductionDTO_Clnone1.WellCategory = WellTypeCategory.Production;
                wellbasedProductionDTO_Clnone1.FluidPhase = WellAllowablePhase.Gas;

                SWARateArrayAndUnitsDTO ratesFor1stRecord_Clone1 = WellAllocationService.GetWellAllowableRatesForSnapshot(wellbasedProductionDTO_Clnone1);
                Assert.IsNotNull(ratesFor1stRecord_Clone1);

                //Comparing Cloned Wellbound records with Base snapshot Wellbound records 
                Assert.AreEqual(ratesFor1stRecord_Clone1.Values.Count(), ratesFor1stRecord.Values.Count(), "Mismatch in total no of count after cloning");
                Assert.AreEqual(true, ratesFor1stRecord.Values[0].InAnalysis, "Mismatch in InAnalysis column for 1st record after cloning");
                Assert.AreEqual(ratesFor1stRecord_Clone1.Values[0].Well.Name, ratesFor1stRecord.Values[0].Well.Name, "Mismatch in WellName column for 1st record after cloning");
                Assert.AreEqual(ratesFor1stRecord_Clone1.Values[0].Zone.Value.ZoneName, ratesFor1stRecord.Values[0].Zone.Value.ZoneName, "Mismatch in ZoneName column for 1st record after cloning");
                Assert.AreEqual(ratesFor1stRecord_Clone1.Values[0].TrunkLine.Value.Name, ratesFor1stRecord.Values[0].TrunkLine.Value.Name, "Mismatch in TrunkLine column for 1st record");
                Assert.AreEqual(ratesFor1stRecord_Clone1.Values[0].WellheadPressureBound, ratesFor1stRecord.Values[0].WellheadPressureBound, "Mismatch in Wellhead Pressure for 1st Record after cloning");
                Assert.AreEqual(ratesFor1stRecord_Clone1.Values[0].PerforationPressureChangeBound, ratesFor1stRecord.Values[0].PerforationPressureChangeBound, "Mismatch in Maximum Drawdown for 1st Record after cloning");
                Assert.AreEqual(ratesFor1stRecord_Clone1.Values[0].ReservoirPressureBound, ratesFor1stRecord.Values[0].ReservoirPressureBound, "Mismatch in Saturation Pressure for 1st Record after cloning");
                Assert.AreEqual(ratesFor1stRecord_Clone1.Values[0].ReservoirPressureTolerance, ratesFor1stRecord.Values[0].ReservoirPressureTolerance, "Mismatch for Saturation Pressure Tolerance for 1st Record after cloning");
                Assert.AreEqual(ratesFor1stRecord_Clone1.Values[0].MaximumRate, ratesFor1stRecord.Values[0].MaximumRate, "Mismatch for Maximum Rate for 1st Record after cloning");
                Assert.AreEqual(ratesFor1stRecord_Clone1.Values[0].MinimumRate, ratesFor1stRecord.Values[0].MinimumRate, "Mismatch for Minimum Rate for 1st Record after cloning");
                Assert.AreEqual(ratesFor1stRecord_Clone1.Values[0].WellAvailability, ratesFor1stRecord.Values[0].WellAvailability, "Mismatch in Well Availability Factor for 1st Record after cloning");
                Trace.WriteLine("Well Bound Records For 1st Cloning Scenario Is Working As Expected");

                //Comparing Cloned Assetbound records with Base snapshot Wellbound records 
                SWAAssetBasedRequestDTO SWAAssetBasedRequestDTO_Clone1 = new SWAAssetBasedRequestDTO() { AssetId = assets.ElementAt(0).Id, WellCategory = WellTypeCategory.Production, SnapshotId = SnapshotClone1.Id, FluidPhase = WellAllowablePhase.Gas };
                SWAAssetBasedDTO AssetBasedDTO_Clone1 = WellAllocationService.GetWellAllowableAssetBasedSnapShot(SWAAssetBasedRequestDTO_Clone1);

                Assert.IsNotNull(AssetBasedDTO_Clone1);
                Assert.AreEqual(AssetBasedDTO_Clone1.SWAAssetZonesDto.Values.Count(), AssetBasedDTO_Base.SWAAssetZonesDto.Values.Count(), "Mismatch in count for 1st clone snap shot from base snap shot");
                Assert.AreEqual(AssetBasedDTO_Clone1.SWAAssetTrunkLinesDto.Values.Count(), AssetBasedDTO_Base.SWAAssetTrunkLinesDto.Values.Count(), "Mismatch in count for 1st clone snap shot from base snap shot");
                Assert.AreEqual(AssetBasedDTO_Clone1.SWAAssetSettingsDto.Value.SystemEfficiency, AssetBasedDTO_Base.SWAAssetSettingsDto.Value.SystemEfficiency, "Mismtach in System Efficiency for 1st clone snap shot from base snap shot");
                Assert.AreEqual(AssetBasedDTO_Clone1.SWAAssetSettingsDto.Value.UnplannedFactor, AssetBasedDTO_Base.SWAAssetSettingsDto.Value.UnplannedFactor, "Mismtach in Unplanned Factor for 1st clone snap shot from base snap shot");
                Assert.AreEqual(AssetBasedDTO_Clone1.SWAAssetSettingsDto.Value.FacilityAvailability, AssetBasedDTO_Base.SWAAssetSettingsDto.Value.FacilityAvailability, "Mismtach in Facility Availibility Factor for 1st clone snap shot from base snap shot");
                Assert.AreEqual(AssetBasedDTO_Clone1.SWAAssetZonesDto.Values[0].TargetExportCondition, AssetBasedDTO_Base.SWAAssetZonesDto.Values[0].TargetExportCondition, "Mismtach in Target Export COndition for 1st clone snap shot from base snap shot");
                //Assert.AreEqual(12.2, Math.Round((double)AssetBasedDTO_1.SWAAssetZonesDto.Values[0].TargetFieldCondition, 1));
                Assert.AreEqual(AssetBasedDTO_Clone1.SWAAssetTrunkLinesDto.Values[0].Capacity, AssetBasedDTO_Base.SWAAssetTrunkLinesDto.Values[0].Capacity, "Mismtach in Trunk Line Capacity for 1st clone snap shot from base snap shot");
                Trace.WriteLine("Asset Bound Records For 1st Cloning Scenario Is Working As Expected");
                #endregion Scenario 1 - Cloning

                //Updating Base snapshot managerial approval status with Approved condition - without it cloning will not work (Validation)
                SWASnapshotInputDTO baseDetail_2 = new SWASnapshotInputDTO(SnapshotClone1.Id, WellAllowableSnapshotType.Target, SnapshotClone1.StartDate.Value, SnapshotClone1.EndDate.Value, "Clone1FromBase");
                baseDetail_2.ApprovalStatus = WellAllowableApprovalStatus.Historized;
                WellAllocationService.UpdateWellAllowableSnapshot(baseDetail_2);

                #region Scenario 2 - Cloning
                //Modifying basic hierarchy 
                //Assiging new Well to existing Zone
                WellToZoneDTO W2Z1R1A1 = AddWellToZone(NFWell_Dry_1.Id, ZoneIDsOfR1A1.Values.ElementAt(0).Id);

                //Creating Zone2 into existing hierarchy and assigning well to it
                ZoneAndUnitsDTO Z2R1A1 = AddZoneToReservoir("Z2R1A1", R1A1_ID);

                //Getting Zone ID
                ZoneArrayAndUnitsDTO ZoneIDsOfR1A1_1 = WellAllocationService.GetZonesByReservoirId(R1A1_ID.ToString());

                //Assign wells With Zone 2 / (Reservoir 1 / Asset 1)
                WellToZoneDTO W1Z2R1A1 = AddWellToZone(NFWell_Con_2.Id, ZoneIDsOfR1A1_1.Values.ElementAt(1).Id);

                //Creating Cloning snapshot from Base snapshot
                DateTime startDate_Clone2 = DateTime.Now.AddDays(3).ToUniversalTime();
                DateTime endDate_Clone2 = DateTime.Now.AddDays(4).ToUniversalTime();
                SWASnapshotInputDTO SnapshotDetail_Clone2 = new SWASnapshotInputDTO(snapshotProductionAsset1.Id, WellAllowableSnapshotType.Target, startDate_Clone2, endDate_Clone2, "Clone2FromBase");
                SWASnapshotDTO Snapshot_Clone2 = WellAllocationService.CloneWellAllowableSnapshot(SnapshotDetail_Clone2);
                _setWellAlowableToRemove.Add(Snapshot_Clone2);
                Assert.IsNotNull(Snapshot_Clone2);

                //Getting rate table for cloned snapshot
                SWAWellBasedDTO wellbasedProductionDTO_Clone2 = new SWAWellBasedDTO();
                wellbasedProductionDTO_Clone2.SnapshotId = Snapshot_Clone2.Id;
                wellbasedProductionDTO_Clone2.AssetId = assets.ElementAt(0).Id;
                wellbasedProductionDTO_Clone2.WellCategory = WellTypeCategory.Production;
                wellbasedProductionDTO_Clone2.FluidPhase = WellAllowablePhase.Gas;

                SWARateArrayAndUnitsDTO ratesFor1stRecord_Clone2 = WellAllocationService.GetWellAllowableRatesForSnapshot(wellbasedProductionDTO_Clone2);
                Assert.IsNotNull(ratesFor1stRecord_Clone2);

                //Comparing cloned Wellbound records with Base snapshot Wellbound records 
                Assert.AreEqual(ratesFor1stRecord_Clone2.Values.Count(), ratesFor1stRecord.Values.Count() + 2, "Mismatch in total no of count after cloning");

                //1st Record
                Assert.AreEqual(true, ratesFor1stRecord.Values[0].InAnalysis, "Mismatch in InAnalysis column for 1st record after cloning");
                Assert.AreEqual(ratesFor1stRecord_Clone2.Values[0].Well.Name, ratesFor1stRecord.Values[0].Well.Name, "Mismatch in WellName column for 1st record after cloning");
                Assert.AreEqual(ratesFor1stRecord_Clone2.Values[0].Zone.Value.ZoneName, ratesFor1stRecord.Values[0].Zone.Value.ZoneName, "Mismatch in ZoneName column for 1st record after cloning");
                Assert.AreEqual(ratesFor1stRecord_Clone2.Values[0].TrunkLine.Value.Name, ratesFor1stRecord.Values[0].TrunkLine.Value.Name, "Mismatch in TrunkLine column for 1st record after cloning");
                Assert.AreEqual(ratesFor1stRecord_Clone2.Values[0].WellheadPressureBound, ratesFor1stRecord.Values[0].WellheadPressureBound, "Mismatch in Wellhead Pressure for 1st Record after cloning");
                Assert.AreEqual(ratesFor1stRecord_Clone2.Values[0].PerforationPressureChangeBound, ratesFor1stRecord.Values[0].PerforationPressureChangeBound, "Mismatch in Maximum Drawdown for 1st Record after cloning");
                Assert.AreEqual(ratesFor1stRecord_Clone2.Values[0].ReservoirPressureBound, ratesFor1stRecord.Values[0].ReservoirPressureBound, "Mismatch in Saturation Pressure for 1st Record after cloning");
                Assert.AreEqual(ratesFor1stRecord_Clone2.Values[0].ReservoirPressureTolerance, ratesFor1stRecord.Values[0].ReservoirPressureTolerance, "Mismatch for Saturation Pressure Tolerance for 1st Record after cloning");
                Assert.AreEqual(ratesFor1stRecord_Clone2.Values[0].MaximumRate, ratesFor1stRecord.Values[0].MaximumRate, "Mismatch for Maximum Rate for 1st Record after cloning");
                Assert.AreEqual(ratesFor1stRecord_Clone2.Values[0].MinimumRate, ratesFor1stRecord.Values[0].MinimumRate, "Mismatch for Minimum Rate for 1st Record after cloning");
                Assert.AreEqual(ratesFor1stRecord_Clone2.Values[0].WellAvailability, ratesFor1stRecord.Values[0].WellAvailability, "Mismatch in Well Availability Factor for 1st Record after cloning");

                //2nd Record
                Assert.AreEqual(true, ratesFor1stRecord_Clone2.Values[1].InAnalysis, "Mismatch in InAnalysis column for 2nd record after cloning");
                Assert.AreEqual("NFWell_Dry_1", ratesFor1stRecord_Clone2.Values[1].Well.Name, "Mismatch in WellName column for 2nd record after cloning");
                Assert.AreEqual("Z1R1A1", ratesFor1stRecord_Clone2.Values[1].Zone.Value.ZoneName, "Mismatch in ZoneName column for 2nd record after cloning");
                Assert.AreEqual("NFWell_Dry_1_Field", ratesFor1stRecord_Clone2.Values[1].TrunkLine.Value.Name, "Mismatch in TrunkLine column for 2nd record after cloning");
                Assert.IsNull(ratesFor1stRecord_Clone2.Values[1].WellheadPressureBound);
                Assert.IsNull(ratesFor1stRecord_Clone2.Values[1].PerforationPressureChangeBound);
                Assert.IsNull(ratesFor1stRecord_Clone2.Values[1].ReservoirPressureBound);
                Assert.IsNull(ratesFor1stRecord_Clone2.Values[1].ReservoirPressureTolerance);
                Assert.IsNull(ratesFor1stRecord_Clone2.Values[1].MaximumRate);
                Assert.IsNull(ratesFor1stRecord_Clone2.Values[1].MinimumRate);
                Assert.AreEqual(1, ratesFor1stRecord_Clone2.Values[1].WellAvailability, "Mismatch in Well Availability Factor for 2nd Record after cloning");

                //3rd Record
                Assert.AreEqual(true, ratesFor1stRecord_Clone2.Values[2].InAnalysis, "Mismatch in InAnalysis column for 2nd record after cloning");
                Assert.AreEqual("NFWell_Con_2", ratesFor1stRecord_Clone2.Values[2].Well.Name, "Mismatch in WellName column for 2nd record after cloning");
                Assert.AreEqual("Z2R1A1", ratesFor1stRecord_Clone2.Values[2].Zone.Value.ZoneName, "Mismatch in ZoneName column for 2nd record after cloning");
                Assert.AreEqual("NFWell_Con_2_Field", ratesFor1stRecord_Clone2.Values[2].TrunkLine.Value.Name, "Mismatch in TrunkLine column for 2nd record after cloning");
                Assert.IsNull(ratesFor1stRecord_Clone2.Values[2].WellheadPressureBound);
                Assert.IsNull(ratesFor1stRecord_Clone2.Values[2].PerforationPressureChangeBound);
                Assert.IsNull(ratesFor1stRecord_Clone2.Values[2].ReservoirPressureBound);
                Assert.IsNull(ratesFor1stRecord_Clone2.Values[2].ReservoirPressureTolerance);
                Assert.IsNull(ratesFor1stRecord_Clone2.Values[2].MaximumRate);
                Assert.IsNull(ratesFor1stRecord_Clone2.Values[2].MinimumRate);
                Assert.AreEqual(1, ratesFor1stRecord_Clone2.Values[2].WellAvailability, "Mismatch in Well Availability Factor for 2nd Record after cloning");
                Trace.WriteLine("Well Bound Records For 2nd Cloning Scenario Is Working As Expected");

                //Comparing Cloned Assetbound records with Base snapshot Assetbound records 
                SWAAssetBasedRequestDTO SWAAssetBasedRequestDTO_Clone2 = new SWAAssetBasedRequestDTO() { AssetId = assets.ElementAt(0).Id, WellCategory = WellTypeCategory.Production, SnapshotId = Snapshot_Clone2.Id, FluidPhase = WellAllowablePhase.Gas };
                SWAAssetBasedDTO AssetBasedDTO_Clone2 = WellAllocationService.GetWellAllowableAssetBasedSnapShot(SWAAssetBasedRequestDTO_Clone2);

                Assert.IsNotNull(AssetBasedDTO_Clone2);
                Assert.AreEqual(AssetBasedDTO_Clone2.SWAAssetZonesDto.Values.Count(), AssetBasedDTO_Base.SWAAssetZonesDto.Values.Count() + 1, "Mismatch in count for 2nd clone snap shot from base snap shot");
                Assert.AreEqual(AssetBasedDTO_Clone2.SWAAssetTrunkLinesDto.Values.Count(), AssetBasedDTO_Base.SWAAssetTrunkLinesDto.Values.Count() + 2, "Mismatch in count for 2nd clone snap shot from base snap shot");
                Assert.AreEqual(AssetBasedDTO_Clone2.SWAAssetSettingsDto.Value.SystemEfficiency, AssetBasedDTO_Base.SWAAssetSettingsDto.Value.SystemEfficiency, "Mismtach in System Efficiency for 2nd clone snap shot from base snap shot");
                Assert.AreEqual(AssetBasedDTO_Clone2.SWAAssetSettingsDto.Value.UnplannedFactor, AssetBasedDTO_Base.SWAAssetSettingsDto.Value.UnplannedFactor, "Mismtach in Unplanned Factor for 2nd clone snap shot from base snap shot");
                Assert.AreEqual(AssetBasedDTO_Clone2.SWAAssetSettingsDto.Value.FacilityAvailability, AssetBasedDTO_Base.SWAAssetSettingsDto.Value.FacilityAvailability, "Mismtach in Facility Availibility Factor for 2nd clone snap shot from base snap shot");
                Assert.AreEqual(AssetBasedDTO_Clone2.SWAAssetZonesDto.Values[0].TargetExportCondition, AssetBasedDTO_Base.SWAAssetZonesDto.Values[0].TargetExportCondition, "Mismtach in Target Export COndition for 2nd clone snap shot from base snap shot");
                //Assert.AreEqual(12.2, Math.Round((double)AssetBasedDTO_1.SWAAssetZonesDto.Values[0].TargetFieldCondition, 1));
                Assert.AreEqual(AssetBasedDTO_Clone2.SWAAssetTrunkLinesDto.Values[0].Capacity, AssetBasedDTO_Base.SWAAssetTrunkLinesDto.Values[0].Capacity, "Mismtach in Trunk Line Capacity for 2nd clone snap shot from base snap shot");
                Assert.AreEqual(null, AssetBasedDTO_Clone2.SWAAssetZonesDto.Values[1].TargetExportCondition, "Mismtach in 2nd record for Target Export Condition for 2nd clone snap shot from base snap shot");
                Assert.AreEqual(null, AssetBasedDTO_Clone2.SWAAssetTrunkLinesDto.Values[1].Capacity, "Mismtach in 2nd record for Trunk Line Capacity for 2nd clone snap shot from base snap shot");
                Assert.AreEqual(null, AssetBasedDTO_Clone2.SWAAssetTrunkLinesDto.Values[2].Capacity, "Mismtach in 3nd record for Trunk Line Capacity for 2nd clone snap shot from base snap shot");

                Trace.WriteLine("Asset Bound Records For 2nd Cloning Scenario Is Working As Expected");
                #endregion Scenario 2 - Cloning

                //Updating Base snapshot managerial approval status with Approved condition - without it cloning will not work (Validation)
                SWASnapshotInputDTO baseDetail_3 = new SWASnapshotInputDTO(Snapshot_Clone2.Id, WellAllowableSnapshotType.Target, Snapshot_Clone2.StartDate.Value, Snapshot_Clone2.EndDate.Value, "Clone2FromBase");
                baseDetail_3.ApprovalStatus = WellAllowableApprovalStatus.Historized;
                WellAllocationService.UpdateWellAllowableSnapshot(baseDetail_3);

                #region Scenario 3 - Cloning
                //Modifying Hierarchy
                //Removing 1 Well from Zone 1 annd assigning same to Zone 2 following same hierarchy
                WellToZoneDTO well = new WellToZoneDTO();
                well.ZoneId = ZoneIDsOfR1A1.Values.ElementAt(0).Id;
                well.WellId = NFWell_Con_1.Id;
                well.EndDate = DateTime.Today.AddDays(-2).ToLocalTime();
                WellAllocationService.RemoveWellMappingToZone(well);

                //Assiging removed Well from Zone 1 to Zone 2
                WellToZoneDTO W22Z1R1A1 = AddWellToZone(NFWell_Con_1.Id, ZoneIDsOfR1A1_1.Values.ElementAt(1).Id);

                //Creating Cloning snapshot from Cloned snapshot (Cloned 2)
                DateTime startDate_Clone3 = DateTime.Now.AddDays(5).ToUniversalTime();
                DateTime endDate_Clone3 = DateTime.Now.AddDays(6).ToUniversalTime();
                SWASnapshotInputDTO SnapshotDetail_Clone3 = new SWASnapshotInputDTO(Snapshot_Clone2.Id, WellAllowableSnapshotType.Target, startDate_Clone3, endDate_Clone3, "Clone3FromClone2");
                SWASnapshotDTO Snapshot_Clone3 = WellAllocationService.CloneWellAllowableSnapshot(SnapshotDetail_Clone3);
                _setWellAlowableToRemove.Add(Snapshot_Clone3);
                Assert.IsNotNull(Snapshot_Clone3);

                //Getting rate table for Cloned snapshot
                SWAWellBasedDTO wellbasedProductionDTO_Clone3 = new SWAWellBasedDTO();
                wellbasedProductionDTO_Clone3.SnapshotId = Snapshot_Clone3.Id;
                wellbasedProductionDTO_Clone3.AssetId = assets.ElementAt(0).Id;
                wellbasedProductionDTO_Clone3.WellCategory = WellTypeCategory.Production;
                wellbasedProductionDTO_Clone3.FluidPhase = WellAllowablePhase.Gas;

                SWARateArrayAndUnitsDTO ratesFor1stRecord_Clone3 = WellAllocationService.GetWellAllowableRatesForSnapshot(wellbasedProductionDTO_Clone3);
                Assert.IsNotNull(ratesFor1stRecord_Clone3);

                //Comparing Cloned Wellbound records with Base snapshot Wellbound records 
                Assert.AreEqual(ratesFor1stRecord_Clone3.Values.Count(), ratesFor1stRecord.Values.Count() + 3, "Mismatch in total no of count after cloning");
                #endregion Scenario 3 - Cloning

                #region Created Coned Sanpshot Which Will Be Used In Edit / Removing Functionality 
                //Creating Cloning snapshot from Cloned snapshot (Cloned 3)
                DateTime startDate_Clone4 = DateTime.Now.AddDays(7).ToUniversalTime();
                DateTime endDate_Clone4 = DateTime.Now.AddDays(8).ToUniversalTime();
                SWASnapshotInputDTO SnapshotDetail_Clone4 = new SWASnapshotInputDTO(Snapshot_Clone2.Id, WellAllowableSnapshotType.Sensitivity, startDate_Clone4, endDate_Clone4, "Clone4FromClone3");
                SWASnapshotDTO Snapshot_Clone4 = WellAllocationService.CloneWellAllowableSnapshot(SnapshotDetail_Clone4);
                _setWellAlowableToRemove.Add(Snapshot_Clone4);
                Assert.IsNotNull(Snapshot_Clone4);
                #endregion Created Coned Sanpshot Which Will Be Used In Edit / Removing Functionality 

                Trace.WriteLine("Cloning functionality is working as expected");
                #endregion Cloning Functionality

                #region Edit Functionality

                #region Scenario 1 - Historized Snapshot cannot be edit
                //This functionality is implemented from UI.
                // It should be test from UI.
                #endregion Scenario 1 - Historized Snapshot cannot be edit

                #region Scenario 2 - Only One Target And One Sensitivity Snapshot May Be Active At Any One Time
                //Validation with Target Snapshot for Clone 3 
                //Tryting to edit Clone 3 sanpshot type with sensitivity 
                SWASnapshotInputDTO existingClone3Detail = new SWASnapshotInputDTO(Snapshot_Clone3.Id, WellAllowableSnapshotType.Target, Snapshot_Clone3.StartDate.Value, Snapshot_Clone3.EndDate.Value, "Clone3FromClone2");
                existingClone3Detail.Name = "Clone3FromClone2_Edited";
                existingClone3Detail.Type = WellAllowableSnapshotType.Sensitivity;
                existingClone3Detail.StartDate = DateTime.Now.AddDays(9).ToUniversalTime();
                existingClone3Detail.EndDate = DateTime.Now.AddDays(10).ToUniversalTime();
                bool Clone3Edited = true;
                try
                {
                    Clone3Edited = WellAllocationService.UpdateWellAllowableSnapshot(existingClone3Detail);
                }
                catch (Exception)
                {

                    Clone3Edited = false;
                }
                Assert.IsFalse(Clone3Edited);
                Trace.WriteLine("Working as expected - Clone 3 Snapshot not edited with Sensitivity Type - becasue Clone 4 Snapshot has same type");

                //Tryting to edit Clone 3 sanpshot type with Target 
                SWASnapshotInputDTO existingClone3Detail_1 = new SWASnapshotInputDTO(Snapshot_Clone3.Id, WellAllowableSnapshotType.Target, Snapshot_Clone3.StartDate.Value, Snapshot_Clone3.EndDate.Value, "Clone3FromClone2");
                existingClone3Detail_1.Name = "Clone3FromClone2_Edited";
                existingClone3Detail_1.Type = WellAllowableSnapshotType.Target;
                existingClone3Detail_1.StartDate = DateTime.Now.AddDays(9).ToUniversalTime();
                existingClone3Detail_1.EndDate = DateTime.Now.AddDays(10).ToUniversalTime();
                var Clone3Edited_1 = WellAllocationService.UpdateWellAllowableSnapshot(existingClone3Detail_1);
                Assert.IsTrue(Clone3Edited_1);
                Trace.WriteLine("Clone 3 Snapshot Edited Successfully");
                #endregion Scenario 2 - Only One Target And One Sensitivity Snapshot May Be Active At Any One Time

                #region Scenario 3 - Only One Target And One Sensitivity Snapshot May Be Active At Any One Time
                //Validation with Sensitivity Snapshot for Clone 4 
                //Tryting to edit Clone 4 sanpshot type with Target
                SWASnapshotInputDTO existingClone4Detail = new SWASnapshotInputDTO(Snapshot_Clone4.Id, WellAllowableSnapshotType.Sensitivity, Snapshot_Clone4.StartDate.Value, Snapshot_Clone4.EndDate.Value, "Clone4FromClone3_Edited");
                existingClone4Detail.Name = "Clone4FromClone3_Edited";
                existingClone4Detail.Type = WellAllowableSnapshotType.Target;
                existingClone4Detail.StartDate = DateTime.Now.AddDays(11).ToUniversalTime();
                existingClone4Detail.EndDate = DateTime.Now.AddDays(12).ToUniversalTime();
                //existingClone4Detail.ApprovalStatus = WellAllowableApprovalStatus.ManagerialApproved;

                bool Clone4Edited = true;
                try
                {
                    Clone4Edited = WellAllocationService.UpdateWellAllowableSnapshot(existingClone4Detail);
                }
                catch (Exception)
                {
                    Clone4Edited = false;
                }
                Assert.IsFalse(Clone4Edited);
                Trace.WriteLine("Working as expected - Clone 4 Snapshot not edited with Target Type - becasue Clone 3 Snapshot has same type");

                //Tryting to edit Clone 3 sanpshot type with Sensitivity
                SWASnapshotInputDTO existingClone4Detail_1 = new SWASnapshotInputDTO(Snapshot_Clone4.Id, WellAllowableSnapshotType.Sensitivity, Snapshot_Clone4.StartDate.Value, Snapshot_Clone4.EndDate.Value, "Clone4FromClone3");
                existingClone4Detail_1.Name = "Clone4FromClone3_Edited";
                existingClone4Detail_1.Type = WellAllowableSnapshotType.Sensitivity;
                existingClone4Detail_1.StartDate = DateTime.Now.AddDays(11).ToUniversalTime();
                existingClone4Detail_1.EndDate = DateTime.Now.AddDays(12).ToUniversalTime();
                //existingClone4Detail.ApprovalStatus = WellAllowableApprovalStatus.ManagerialApproved;

                var Clone4Edited_1 = WellAllocationService.UpdateWellAllowableSnapshot(existingClone4Detail_1);
                Assert.IsTrue(Clone4Edited_1);
                Trace.WriteLine("Clone 4 Snapshot Edited Successfully");
                #endregion Scenario 3 - Only One Target And One Sensitivity Snapshot May Be Active At Any One Time

                #region Verifying Edited Records
                //Get list of available Snapshots
                var snapshotList = WellAllocationService.GetWellAllowableSnapshots();
                Assert.AreEqual("Clone3FromClone2_Edited", snapshotList[3].Name, "Mismatch in edited Snapshot name");
                Assert.AreEqual(WellAllowableSnapshotType.Target, snapshotList[3].SnapshotType, "Mismatch in edited Snapshot type");

                Assert.AreEqual("Clone4FromClone3_Edited", snapshotList[4].Name, "Mismatch in edited Snapshot name");
                Assert.AreEqual(WellAllowableSnapshotType.Sensitivity, snapshotList[4].SnapshotType, "Mismatch in edited Snapshot type");

                Trace.WriteLine("Edit functionality is working as expected");
                #endregion Verifying Edited Records

                #endregion Edit Functionality

                #region Remove Functionality
                //Remove functionality will not remove Snapshot which has Target Type
                WellAllocationService.SetWellAllowableSnapshotStatus(SnapshotClone1.Id.ToString(), false.ToString());
                var snapshotList_1 = WellAllocationService.GetWellAllowableSnapshots();
                Assert.AreEqual(snapshotList_1.Count(), snapshotList.Count(), "Mismatch In Count List - Before Remove And After Remove");

                //Remove functionality will only remove Snapshot which has Sensitivity Type
                WellAllocationService.SetWellAllowableSnapshotStatus(Snapshot_Clone4.Id.ToString(), false.ToString());
                var snapshotList_2 = WellAllocationService.GetWellAllowableSnapshots();
                Assert.AreEqual(snapshotList_2.Count(), snapshotList.Count() - 1, "Mismatch In Count List - Before Remove And After Remove");

                Trace.WriteLine("Remove functionality is woorking as expected");
                #endregion Remove Functionality

                #region Edit Functionality After Removed Sensitivity Type Snapshot
                //Tryting to edit Clone 3 sanpshot type with Sensitivity
                SWASnapshotInputDTO existingClone3Detail_2 = new SWASnapshotInputDTO(Snapshot_Clone3.Id, WellAllowableSnapshotType.Target, Snapshot_Clone3.StartDate.Value, Snapshot_Clone3.EndDate.Value, "Clone3FromClone2");
                existingClone3Detail_2.Name = "Clone3FromClone2_Edited_Sensitivity";
                existingClone3Detail_2.Type = WellAllowableSnapshotType.Sensitivity;
                existingClone3Detail_2.StartDate = DateTime.Now.AddDays(13).ToUniversalTime();
                existingClone3Detail_2.EndDate = DateTime.Now.AddDays(14).ToUniversalTime();
                var Clone3Edited_2 = WellAllocationService.UpdateWellAllowableSnapshot(existingClone3Detail_2);
                Assert.IsTrue(Clone3Edited_2);
                Trace.WriteLine("Clone 3 Snapshot With Sensitivity Type Edited Successfully");

                var snapshotList_3 = WellAllocationService.GetWellAllowableSnapshots();
                Assert.AreEqual("Clone3FromClone2_Edited_Sensitivity", snapshotList_3[3].Name, "Mismatch in edited Snapshot name");
                Assert.AreEqual(WellAllowableSnapshotType.Sensitivity, snapshotList_3[3].SnapshotType, "Mismatch in edited Snapshot type");
                #endregion Edit Functionality After Removed Sensitivity Type Snapshot
            }
            finally
            {
                ChangeUnitSystem("US");
                ChangeUnitSystemUserSetting("US");
            }
        }

        // FRWM_7274_SWA : Snapshot Clone/Edit/Remove functionality
        [TestCategory(TestCategories.WellAllocationServiceTests), TestMethod]
        public void SWASanpshotCloneEditRemove_InjectionCategory()
        {
            ChangeUnitSystem("US");
            ChangeUnitSystemUserSetting("US");
            try
            {
                #region Asset
                List<AssetDTO> assets = CreateAsset(2);
                #endregion Asset

                #region Create Injection Wells With Asset
                WellDTO GASInjWell_1 = AddNonRRLWellGeneral("GASInjWell_1", GetFacilityId("GASINJWELL_", 1), WellTypeId.GInj, WellFluidType.None, WellFluidPhase.None, "1", assets.ElementAt(0).Id);
                AddModelFile(GASInjWell_1.CommissionDate.Value, WellTypeId.GInj, GASInjWell_1.Id, CalibrationMethodId.LFactor);
                WellDTO GASInjWell_2 = AddNonRRLWellGeneral("GASInjWell_2", GetFacilityId("GASINJWELL_", 2), WellTypeId.GInj, WellFluidType.None, WellFluidPhase.None, "1", assets.ElementAt(0).Id);
                AddModelFile(GASInjWell_2.CommissionDate.Value, WellTypeId.GInj, GASInjWell_2.Id, CalibrationMethodId.LFactor);
                WellDTO GASInjWell_3 = AddNonRRLWellGeneral("GASInjWell_3", GetFacilityId("GASINJWELL_", 3), WellTypeId.GInj, WellFluidType.None, WellFluidPhase.None, "1", assets.ElementAt(0).Id);
                AddModelFile(GASInjWell_3.CommissionDate.Value, WellTypeId.GInj, GASInjWell_3.Id, CalibrationMethodId.LFactor);
                WellDTO GASInjWell_4 = AddNonRRLWellGeneral("GASInjWell_4", GetFacilityId("GASINJWELL_", 4), WellTypeId.GInj, WellFluidType.None, WellFluidPhase.None, "1", assets.ElementAt(1).Id);
                AddModelFile(GASInjWell_4.CommissionDate.Value, WellTypeId.GInj, GASInjWell_4.Id, CalibrationMethodId.LFactor);
                Trace.WriteLine("Injection Wells are created successfully and mapped with Assets");
                #endregion Create Injection Wells With Asset

                #region Create Basic VRR Hierarchy
                //Create Reservoir for Asset 1 and get it's ID
                ReservoirAndUnitsDTO R1A1 = AddReservoirToAsset("R1A1", assets.ElementAt(0).Id);
                _vrrHierarchyToRemove.Add(assets[0]);

                ReservoirArrayAndUnitsDTO ReservoirIDsOfAsset1 = WellAllocationService.GetReservoirsByAssetId(assets.ElementAt(0).Id.ToString());
                var R1A1_ID = ReservoirIDsOfAsset1.Values.ElementAt(0).Id;

                //Create Zone 1 for Reservoir 1 / Asset 1 and get it's ID
                ZoneAndUnitsDTO Z1R1A1 = AddZoneToReservoir("Z1R1A1", R1A1_ID);
                ZoneArrayAndUnitsDTO ZoneIDsOfR1A1 = WellAllocationService.GetZonesByReservoirId(R1A1_ID.ToString());

                //Mapped Wells with Zone 1
                WellToZoneDTO W1Z1R1A1 = AddWellToZone(GASInjWell_1.Id, ZoneIDsOfR1A1.Values.ElementAt(0).Id);
                #endregion Create Basic VRR Hierarchy

                #region Cloning Functionality

                #region Create Base Snapshot With Injection Category
                string startDateInjection = DTOExtensions.ToISO8601(DateTime.Now.AddDays(-30).ToUniversalTime());
                string endDateInjection = DTOExtensions.ToISO8601(DateTime.Now.AddDays(-20).ToUniversalTime());
                string injectionSnapshotNameAsset1 = "Injection Snap Asset1";
                string injectionSnapshotType = WellAllowableSnapshotType.Sensitivity.ToString();

                //Create Base snapshot
                SWASnapshotDTO snapshotInjectionAsset1 = WellAllocationService.CreateWellAllowableSnapshot(injectionSnapshotNameAsset1, injectionSnapshotType, assets.ElementAt(0).Id.ToString(), startDateInjection, endDateInjection, WellTypeCategory.Injection.ToString(), WellAllowablePhase.Gas.ToString(), null);
                Assert.IsNotNull(snapshotInjectionAsset1);
                _setWellAlowableToRemove.Add(snapshotInjectionAsset1);
                Trace.WriteLine("Base Snapshot Is Create Successfully For Production Category");

                //Get rate information for Wellbound 
                SWAWellBasedDTO wellBasedInjectionDTO = new SWAWellBasedDTO();
                wellBasedInjectionDTO.SnapshotId = snapshotInjectionAsset1.Id;
                wellBasedInjectionDTO.AssetId = assets.ElementAt(0).Id;
                wellBasedInjectionDTO.WellCategory = WellTypeCategory.Injection;
                wellBasedInjectionDTO.FluidPhase = WellAllowablePhase.Gas;

                #region Inserting Well Bound Records For 1st Record
                SWARateArrayAndUnitsDTO ratesFor1stRecord = WellAllocationService.GetWellAllowableRatesForSnapshot(wellBasedInjectionDTO);
                Assert.IsNotNull(ratesFor1stRecord);
                ratesFor1stRecord.Values[0].WellheadPressureBound = (decimal)300.5;
                ratesFor1stRecord.Values[0].PerforationPressureChangeBound = (decimal)400.5;
                ratesFor1stRecord.Values[0].ReservoirPressureBound = (decimal)1350.5;
                ratesFor1stRecord.Values[0].ReservoirPressureTolerance = (decimal)60.5;
                ratesFor1stRecord.Values[0].MaximumRate = (decimal)970.5;
                ratesFor1stRecord.Values[0].MinimumRate = (decimal)80.5;
                ratesFor1stRecord.Values[0].WellAvailability = (decimal)0.86;
                ratesFor1stRecord.Values[0].InAnalysis = true;

                wellBasedInjectionDTO.Rates = ratesFor1stRecord.Values;

                //Saving 1st Wellbound record
                var updateRateFor1stRecord = WellAllocationService.UpdateWellAllowableRatesForSnapshot(wellBasedInjectionDTO);
                Assert.IsNotNull(updateRateFor1stRecord);
                #endregion Inserting Well Bound Records For 1st Record

                #region Inserting Asset Bound Record
                SWAAssetBasedRequestDTO SWAAssetBasedRequestDTO = new SWAAssetBasedRequestDTO() { AssetId = assets.ElementAt(0).Id, WellCategory = WellTypeCategory.Injection, SnapshotId = snapshotInjectionAsset1.Id, FluidPhase = WellAllowablePhase.Gas };
                //Getting Assetbound record
                SWAAssetBasedDTO AssetBasedDTO = WellAllocationService.GetWellAllowableAssetBasedSnapShot(SWAAssetBasedRequestDTO);
                AssetBasedDTO.SWAAssetSettingsDto.Value.SystemEfficiency = (decimal?)0.95;
                AssetBasedDTO.SWAAssetSettingsDto.Value.UnplannedFactor = (decimal?)0.22;
                AssetBasedDTO.SWAAssetSettingsDto.Value.FacilityAvailability = (decimal?)0.75;

                //Adding TargetExportCondition,TargetFieldCondition & Capacity values
                for (int i = 0; i < AssetBasedDTO.SWAAssetZonesDto.Values.Length; i++)
                {
                    AssetBasedDTO.SWAAssetZonesDto.Values[i].TargetExportCondition = (decimal?)3400.55 + (i * 10);
                    //AssetBasedDTO.SWAAssetZonesDto.Values[i].TargetFieldCondition = (decimal?) 3500 + (i*10);
                }

                //Adding Trunk Line Capacity values
                for (int i = 0; i < AssetBasedDTO.SWAAssetTrunkLinesDto.Values.Length; i++)
                {
                    AssetBasedDTO.SWAAssetTrunkLinesDto.Values[i].Capacity = (decimal?)3600.45 + (i * 10);
                }

                SWAAssetSettingsDTO swasettings = new SWAAssetSettingsDTO();
                swasettings = AssetBasedDTO.SWAAssetSettingsDto.Value;
                AssetBasedDTO.SaveAssetingParameter = swasettings;
                AssetBasedDTO.SWARequestDTO = SWAAssetBasedRequestDTO;
                AssetBasedDTO.SavedAssetZoneDTOs = AssetBasedDTO.SWAAssetZonesDto.Values;
                AssetBasedDTO.SavedTrunkLineDTOs = AssetBasedDTO.SWAAssetTrunkLinesDto.Values;

                //Saving Assetbound Record
                Assert.IsNotNull(AssetBasedDTO.SaveAssetingParameter);
                WellAllocationService.SaveWellAllowableAssetBasedSnapShot(AssetBasedDTO);

                // Getting inserted Assetbound record
                SWAAssetBasedRequestDTO SWAAssetBasedRequestDTO_Base = new SWAAssetBasedRequestDTO() { AssetId = assets.ElementAt(0).Id, WellCategory = WellTypeCategory.Injection, SnapshotId = snapshotInjectionAsset1.Id, FluidPhase = WellAllowablePhase.Gas };
                SWAAssetBasedDTO AssetBasedDTO_Base = WellAllocationService.GetWellAllowableAssetBasedSnapShot(SWAAssetBasedRequestDTO_Base);
                Assert.IsNotNull(AssetBasedDTO_Base);
                Assert.AreEqual(1, AssetBasedDTO_Base.SWAAssetZonesDto.Values.Count(), "Mismatch in count");
                Assert.AreEqual(1, AssetBasedDTO_Base.SWAAssetTrunkLinesDto.Values.Count(), "Mismatch in count");
                Assert.AreEqual(0.95, Math.Round((double)AssetBasedDTO_Base.SWAAssetSettingsDto.Value.SystemEfficiency, 2));
                Assert.AreEqual(0.22, Math.Round((double)AssetBasedDTO_Base.SWAAssetSettingsDto.Value.UnplannedFactor, 2));
                Assert.AreEqual(0.75, Math.Round((double)AssetBasedDTO_Base.SWAAssetSettingsDto.Value.FacilityAvailability, 2));
                Assert.AreEqual(3400.6, Math.Round((double)AssetBasedDTO_Base.SWAAssetZonesDto.Values[0].TargetExportCondition, 1));
                //Assert.AreEqual(12.2, Math.Round((double)AssetBasedDTO_1.SWAAssetZonesDto.Values[0].TargetFieldCondition, 1));
                Assert.AreEqual(3600.4, Math.Round((double)AssetBasedDTO_Base.SWAAssetTrunkLinesDto.Values[0].Capacity, 1));
                #endregion Inserting Asset Bound Record

                #endregion Create Base Snapshot With Injection Category

                //Updating Base snapshot managerial approval status with Approved condition - without it cloning will not work (Validation)
                SWASnapshotInputDTO baseDetail_1 = new SWASnapshotInputDTO(snapshotInjectionAsset1.Id, WellAllowableSnapshotType.Target, snapshotInjectionAsset1.StartDate.Value, snapshotInjectionAsset1.EndDate.Value, "Injection Snap Asset1");
                baseDetail_1.ApprovalStatus = WellAllowableApprovalStatus.Historized;
                WellAllocationService.UpdateWellAllowableSnapshot(baseDetail_1);

                #region Scenario 1 - Cloning
                //Creating 1st Clone snapshot from Base snapshot
                DateTime startDate_clone1 = DateTime.Now.AddDays(1).ToUniversalTime();
                DateTime endDate_clone1 = DateTime.Now.AddDays(2).ToUniversalTime();

                SWASnapshotInputDTO SnapshotDetail_Clone1 = new SWASnapshotInputDTO(snapshotInjectionAsset1.Id, WellAllowableSnapshotType.Target, startDate_clone1, endDate_clone1, "Clone1FromBase");
                SWASnapshotDTO SnapshotClone1 = WellAllocationService.CloneWellAllowableSnapshot(SnapshotDetail_Clone1);
                _setWellAlowableToRemove.Add(SnapshotClone1);
                Assert.IsNotNull(SnapshotClone1);

                //Getting rate table for Cloned snapshot
                SWAWellBasedDTO wellbasedInjectionDTO_Clnone1 = new SWAWellBasedDTO();
                wellbasedInjectionDTO_Clnone1.SnapshotId = SnapshotClone1.Id;
                wellbasedInjectionDTO_Clnone1.AssetId = assets.ElementAt(0).Id;
                wellbasedInjectionDTO_Clnone1.WellCategory = WellTypeCategory.Injection;
                wellbasedInjectionDTO_Clnone1.FluidPhase = WellAllowablePhase.Gas;

                SWARateArrayAndUnitsDTO ratesFor1stRecord_Clone1 = WellAllocationService.GetWellAllowableRatesForSnapshot(wellbasedInjectionDTO_Clnone1);
                Assert.IsNotNull(ratesFor1stRecord_Clone1);

                //Comparing Cloned Wellbound records with Base snapshot Wellbound records 
                Assert.AreEqual(ratesFor1stRecord_Clone1.Values.Count(), ratesFor1stRecord.Values.Count(), "Mismatch in total no of count after cloning");
                Assert.AreEqual(true, ratesFor1stRecord.Values[0].InAnalysis, "Mismatch in InAnalysis column for 1st record after cloning");
                Assert.AreEqual(ratesFor1stRecord_Clone1.Values[0].Well.Name, ratesFor1stRecord.Values[0].Well.Name, "Mismatch in WellName column for 1st record after cloning");
                Assert.AreEqual(ratesFor1stRecord_Clone1.Values[0].Zone.Value.ZoneName, ratesFor1stRecord.Values[0].Zone.Value.ZoneName, "Mismatch in ZoneName column for 1st record after cloning");
                Assert.AreEqual(ratesFor1stRecord_Clone1.Values[0].TrunkLine.Value.Name, ratesFor1stRecord.Values[0].TrunkLine.Value.Name, "Mismatch in TrunkLine column for 1st record");
                Assert.AreEqual(ratesFor1stRecord_Clone1.Values[0].WellheadPressureBound, ratesFor1stRecord.Values[0].WellheadPressureBound, "Mismatch in Wellhead Pressure for 1st Record after cloning");
                Assert.AreEqual(ratesFor1stRecord_Clone1.Values[0].PerforationPressureChangeBound, ratesFor1stRecord.Values[0].PerforationPressureChangeBound, "Mismatch in Maximum Drawdown for 1st Record after cloning");
                Assert.AreEqual(ratesFor1stRecord_Clone1.Values[0].ReservoirPressureBound, ratesFor1stRecord.Values[0].ReservoirPressureBound, "Mismatch in Saturation Pressure for 1st Record after cloning");
                Assert.AreEqual(ratesFor1stRecord_Clone1.Values[0].ReservoirPressureTolerance, ratesFor1stRecord.Values[0].ReservoirPressureTolerance, "Mismatch for Saturation Pressure Tolerance for 1st Record after cloning");
                Assert.AreEqual(ratesFor1stRecord_Clone1.Values[0].MaximumRate, ratesFor1stRecord.Values[0].MaximumRate, "Mismatch for Maximum Rate for 1st Record after cloning");
                Assert.AreEqual(ratesFor1stRecord_Clone1.Values[0].MinimumRate, ratesFor1stRecord.Values[0].MinimumRate, "Mismatch for Minimum Rate for 1st Record after cloning");
                Assert.AreEqual(ratesFor1stRecord_Clone1.Values[0].WellAvailability, ratesFor1stRecord.Values[0].WellAvailability, "Mismatch in Well Availability Factor for 1st Record after cloning");
                Trace.WriteLine("Well Bound Records For 1st Cloning Scenario Is Working As Expected");

                //Comparing Cloned Assetbound records with Base snapshot Wellbound records 
                SWAAssetBasedRequestDTO SWAAssetBasedRequestDTO_Clone1 = new SWAAssetBasedRequestDTO() { AssetId = assets.ElementAt(0).Id, WellCategory = WellTypeCategory.Injection, SnapshotId = SnapshotClone1.Id, FluidPhase = WellAllowablePhase.Gas };
                SWAAssetBasedDTO AssetBasedDTO_Clone1 = WellAllocationService.GetWellAllowableAssetBasedSnapShot(SWAAssetBasedRequestDTO_Clone1);

                Assert.IsNotNull(AssetBasedDTO_Clone1);
                Assert.AreEqual(AssetBasedDTO_Clone1.SWAAssetZonesDto.Values.Count(), AssetBasedDTO_Base.SWAAssetZonesDto.Values.Count(), "Mismatch in count for 1st clone snap shot from base snap shot");
                Assert.AreEqual(AssetBasedDTO_Clone1.SWAAssetTrunkLinesDto.Values.Count(), AssetBasedDTO_Base.SWAAssetTrunkLinesDto.Values.Count(), "Mismatch in count for 1st clone snap shot from base snap shot");
                Assert.AreEqual(AssetBasedDTO_Clone1.SWAAssetSettingsDto.Value.SystemEfficiency, AssetBasedDTO_Base.SWAAssetSettingsDto.Value.SystemEfficiency, "Mismtach in System Efficiency for 1st clone snap shot from base snap shot");
                Assert.AreEqual(AssetBasedDTO_Clone1.SWAAssetSettingsDto.Value.UnplannedFactor, AssetBasedDTO_Base.SWAAssetSettingsDto.Value.UnplannedFactor, "Mismtach in Unplanned Factor for 1st clone snap shot from base snap shot");
                Assert.AreEqual(AssetBasedDTO_Clone1.SWAAssetSettingsDto.Value.FacilityAvailability, AssetBasedDTO_Base.SWAAssetSettingsDto.Value.FacilityAvailability, "Mismtach in Facility Availibility Factor for 1st clone snap shot from base snap shot");
                Assert.AreEqual(AssetBasedDTO_Clone1.SWAAssetZonesDto.Values[0].TargetExportCondition, AssetBasedDTO_Base.SWAAssetZonesDto.Values[0].TargetExportCondition, "Mismtach in Target Export COndition for 1st clone snap shot from base snap shot");
                //Assert.AreEqual(12.2, Math.Round((double)AssetBasedDTO_1.SWAAssetZonesDto.Values[0].TargetFieldCondition, 1));
                Assert.AreEqual(AssetBasedDTO_Clone1.SWAAssetTrunkLinesDto.Values[0].Capacity, AssetBasedDTO_Base.SWAAssetTrunkLinesDto.Values[0].Capacity, "Mismtach in Trunk Line Capacity for 1st clone snap shot from base snap shot");
                Trace.WriteLine("Asset Bound Records For 1st Cloning Scenario Is Working As Expected");
                #endregion Scenario 1 - Cloning

                //Updating Base snapshot managerial approval status with Approved condition - without it cloning will not work (Validation)
                SWASnapshotInputDTO baseDetail_2 = new SWASnapshotInputDTO(SnapshotClone1.Id, WellAllowableSnapshotType.Target, SnapshotClone1.StartDate.Value, SnapshotClone1.EndDate.Value, "Clone1FromBase");
                baseDetail_2.ApprovalStatus = WellAllowableApprovalStatus.Historized;
                WellAllocationService.UpdateWellAllowableSnapshot(baseDetail_2);

                #region Scenario 2 - Cloning
                //Modifying basic hierarchy 
                //Assiging new Well to existing Zone
                WellToZoneDTO W2Z1R1A1 = AddWellToZone(GASInjWell_2.Id, ZoneIDsOfR1A1.Values.ElementAt(0).Id);

                //Creating Zone2 into existing hierarchy and assigning well to it
                ZoneAndUnitsDTO Z2R1A1 = AddZoneToReservoir("Z2R1A1", R1A1_ID);

                //Getting Zone ID
                ZoneArrayAndUnitsDTO ZoneIDsOfR1A1_1 = WellAllocationService.GetZonesByReservoirId(R1A1_ID.ToString());

                //Assign wells With Zone 2 / (Reservoir 1 / Asset 1)
                WellToZoneDTO W1Z2R1A1 = AddWellToZone(GASInjWell_3.Id, ZoneIDsOfR1A1_1.Values.ElementAt(1).Id);

                //Creating Cloning snapshot from Base snapshot
                DateTime startDate_Clone2 = DateTime.Now.AddDays(3).ToUniversalTime();
                DateTime endDate_Clone2 = DateTime.Now.AddDays(4).ToUniversalTime();
                SWASnapshotInputDTO SnapshotDetail_Clone2 = new SWASnapshotInputDTO(snapshotInjectionAsset1.Id, WellAllowableSnapshotType.Target, startDate_Clone2, endDate_Clone2, "Clone2FromBase");
                SWASnapshotDTO Snapshot_Clone2 = WellAllocationService.CloneWellAllowableSnapshot(SnapshotDetail_Clone2);
                _setWellAlowableToRemove.Add(Snapshot_Clone2);
                Assert.IsNotNull(Snapshot_Clone2);

                //Getting rate table for cloned snapshot
                SWAWellBasedDTO wellbasedInjectionDTO_Clone2 = new SWAWellBasedDTO();
                wellbasedInjectionDTO_Clone2.SnapshotId = Snapshot_Clone2.Id;
                wellbasedInjectionDTO_Clone2.AssetId = assets.ElementAt(0).Id;
                wellbasedInjectionDTO_Clone2.WellCategory = WellTypeCategory.Injection;
                wellbasedInjectionDTO_Clone2.FluidPhase = WellAllowablePhase.Gas;

                SWARateArrayAndUnitsDTO ratesFor1stRecord_Clone2 = WellAllocationService.GetWellAllowableRatesForSnapshot(wellbasedInjectionDTO_Clone2);
                Assert.IsNotNull(ratesFor1stRecord_Clone2);

                //Comparing cloned Wellbound records with Base snapshot Wellbound records 
                Assert.AreEqual(ratesFor1stRecord_Clone2.Values.Count(), ratesFor1stRecord.Values.Count() + 2, "Mismatch in total no of count after cloning");

                //1st Record
                Assert.AreEqual(true, ratesFor1stRecord.Values[0].InAnalysis, "Mismatch in InAnalysis column for 1st record after cloning");
                Assert.AreEqual(ratesFor1stRecord_Clone2.Values[0].Well.Name, ratesFor1stRecord.Values[0].Well.Name, "Mismatch in WellName column for 1st record after cloning");
                Assert.AreEqual(ratesFor1stRecord_Clone2.Values[0].Zone.Value.ZoneName, ratesFor1stRecord.Values[0].Zone.Value.ZoneName, "Mismatch in ZoneName column for 1st record after cloning");
                Assert.AreEqual(ratesFor1stRecord_Clone2.Values[0].TrunkLine.Value.Name, ratesFor1stRecord.Values[0].TrunkLine.Value.Name, "Mismatch in TrunkLine column for 1st record after cloning");
                Assert.AreEqual(ratesFor1stRecord_Clone2.Values[0].WellheadPressureBound, ratesFor1stRecord.Values[0].WellheadPressureBound, "Mismatch in Wellhead Pressure for 1st Record after cloning");
                Assert.AreEqual(ratesFor1stRecord_Clone2.Values[0].PerforationPressureChangeBound, ratesFor1stRecord.Values[0].PerforationPressureChangeBound, "Mismatch in Maximum Drawdown for 1st Record after cloning");
                Assert.AreEqual(ratesFor1stRecord_Clone2.Values[0].ReservoirPressureBound, ratesFor1stRecord.Values[0].ReservoirPressureBound, "Mismatch in Saturation Pressure for 1st Record after cloning");
                Assert.AreEqual(ratesFor1stRecord_Clone2.Values[0].ReservoirPressureTolerance, ratesFor1stRecord.Values[0].ReservoirPressureTolerance, "Mismatch for Saturation Pressure Tolerance for 1st Record after cloning");
                Assert.AreEqual(ratesFor1stRecord_Clone2.Values[0].MaximumRate, ratesFor1stRecord.Values[0].MaximumRate, "Mismatch for Maximum Rate for 1st Record after cloning");
                Assert.AreEqual(ratesFor1stRecord_Clone2.Values[0].MinimumRate, ratesFor1stRecord.Values[0].MinimumRate, "Mismatch for Minimum Rate for 1st Record after cloning");
                Assert.AreEqual(ratesFor1stRecord_Clone2.Values[0].WellAvailability, ratesFor1stRecord.Values[0].WellAvailability, "Mismatch in Well Availability Factor for 1st Record after cloning");

                //2nd Record
                Assert.AreEqual(true, ratesFor1stRecord_Clone2.Values[1].InAnalysis, "Mismatch in InAnalysis column for 2nd record after cloning");
                Assert.AreEqual("GASInjWell_2", ratesFor1stRecord_Clone2.Values[1].Well.Name, "Mismatch in WellName column for 2nd record after cloning");
                Assert.AreEqual("Z1R1A1", ratesFor1stRecord_Clone2.Values[1].Zone.Value.ZoneName, "Mismatch in ZoneName column for 2nd record after cloning");
                Assert.AreEqual("GASInjWell_2_Field", ratesFor1stRecord_Clone2.Values[1].TrunkLine.Value.Name, "Mismatch in TrunkLine column for 2nd record after cloning");
                Assert.IsNull(ratesFor1stRecord_Clone2.Values[1].WellheadPressureBound);
                Assert.IsNull(ratesFor1stRecord_Clone2.Values[1].PerforationPressureChangeBound);
                Assert.IsNull(ratesFor1stRecord_Clone2.Values[1].ReservoirPressureBound);
                Assert.IsNull(ratesFor1stRecord_Clone2.Values[1].ReservoirPressureTolerance);
                Assert.IsNull(ratesFor1stRecord_Clone2.Values[1].MaximumRate);
                Assert.IsNull(ratesFor1stRecord_Clone2.Values[1].MinimumRate);
                Assert.AreEqual(1, ratesFor1stRecord_Clone2.Values[1].WellAvailability, "Mismatch in Well Availability Factor for 2nd Record after cloning");

                //3rd Record
                Assert.AreEqual(true, ratesFor1stRecord_Clone2.Values[2].InAnalysis, "Mismatch in InAnalysis column for 2nd record after cloning");
                Assert.AreEqual("GASInjWell_3", ratesFor1stRecord_Clone2.Values[2].Well.Name, "Mismatch in WellName column for 2nd record after cloning");
                Assert.AreEqual("Z2R1A1", ratesFor1stRecord_Clone2.Values[2].Zone.Value.ZoneName, "Mismatch in ZoneName column for 2nd record after cloning");
                Assert.AreEqual("GASInjWell_3_Field", ratesFor1stRecord_Clone2.Values[2].TrunkLine.Value.Name, "Mismatch in TrunkLine column for 2nd record after cloning");
                Assert.IsNull(ratesFor1stRecord_Clone2.Values[2].WellheadPressureBound);
                Assert.IsNull(ratesFor1stRecord_Clone2.Values[2].PerforationPressureChangeBound);
                Assert.IsNull(ratesFor1stRecord_Clone2.Values[2].ReservoirPressureBound);
                Assert.IsNull(ratesFor1stRecord_Clone2.Values[2].ReservoirPressureTolerance);
                Assert.IsNull(ratesFor1stRecord_Clone2.Values[2].MaximumRate);
                Assert.IsNull(ratesFor1stRecord_Clone2.Values[2].MinimumRate);
                Assert.AreEqual(1, ratesFor1stRecord_Clone2.Values[2].WellAvailability, "Mismatch in Well Availability Factor for 2nd Record after cloning");
                Trace.WriteLine("Well Bound Records For 2nd Cloning Scenario Is Working As Expected");

                //Comparing Cloned Assetbound records with Base snapshot Assetbound records 
                SWAAssetBasedRequestDTO SWAAssetBasedRequestDTO_Clone2 = new SWAAssetBasedRequestDTO() { AssetId = assets.ElementAt(0).Id, WellCategory = WellTypeCategory.Injection, SnapshotId = Snapshot_Clone2.Id, FluidPhase = WellAllowablePhase.Gas };
                SWAAssetBasedDTO AssetBasedDTO_Clone2 = WellAllocationService.GetWellAllowableAssetBasedSnapShot(SWAAssetBasedRequestDTO_Clone2);

                Assert.IsNotNull(AssetBasedDTO_Clone2);
                //Assert.AreEqual(AssetBasedDTO_Clone2.SWAAssetZonesDto.Values.Count(), AssetBasedDTO_Base.SWAAssetZonesDto.Values.Count() + 1, "Mismatch in count for 2nd clone snap shot from base snap shot");
                Assert.AreEqual(AssetBasedDTO_Clone2.SWAAssetTrunkLinesDto.Values.Count(), AssetBasedDTO_Base.SWAAssetTrunkLinesDto.Values.Count() + 2, "Mismatch in count for 2nd clone snap shot from base snap shot");
                Assert.AreEqual(AssetBasedDTO_Clone2.SWAAssetSettingsDto.Value.SystemEfficiency, AssetBasedDTO_Base.SWAAssetSettingsDto.Value.SystemEfficiency, "Mismtach in System Efficiency for 2nd clone snap shot from base snap shot");
                Assert.AreEqual(AssetBasedDTO_Clone2.SWAAssetSettingsDto.Value.UnplannedFactor, AssetBasedDTO_Base.SWAAssetSettingsDto.Value.UnplannedFactor, "Mismtach in Unplanned Factor for 2nd clone snap shot from base snap shot");
                Assert.AreEqual(AssetBasedDTO_Clone2.SWAAssetSettingsDto.Value.FacilityAvailability, AssetBasedDTO_Base.SWAAssetSettingsDto.Value.FacilityAvailability, "Mismtach in Facility Availibility Factor for 2nd clone snap shot from base snap shot");
                Assert.AreEqual(AssetBasedDTO_Clone2.SWAAssetZonesDto.Values[0].TargetExportCondition, AssetBasedDTO_Base.SWAAssetZonesDto.Values[0].TargetExportCondition, "Mismtach in Target Export COndition for 2nd clone snap shot from base snap shot");
                //Assert.AreEqual(12.2, Math.Round((double)AssetBasedDTO_1.SWAAssetZonesDto.Values[0].TargetFieldCondition, 1));
                Assert.AreEqual(AssetBasedDTO_Clone2.SWAAssetTrunkLinesDto.Values[0].Capacity, AssetBasedDTO_Base.SWAAssetTrunkLinesDto.Values[0].Capacity, "Mismtach in Trunk Line Capacity for 2nd clone snap shot from base snap shot");
                //Assert.AreEqual(null, AssetBasedDTO_Clone2.SWAAssetZonesDto.Values[1].TargetExportCondition, "Mismtach in 2nd record for Target Export Condition for 2nd clone snap shot from base snap shot");
                Assert.AreEqual(null, AssetBasedDTO_Clone2.SWAAssetTrunkLinesDto.Values[1].Capacity, "Mismtach in 2nd record for Trunk Line Capacity for 2nd clone snap shot from base snap shot");
                Assert.AreEqual(null, AssetBasedDTO_Clone2.SWAAssetTrunkLinesDto.Values[2].Capacity, "Mismtach in 3nd record for Trunk Line Capacity for 2nd clone snap shot from base snap shot");

                Trace.WriteLine("Asset Bound Records For 2nd Cloning Scenario Is Working As Expected");
                #endregion Scenario 2 - Cloning

                //Updating Base snapshot managerial approval status with Approved condition - without it cloning will not work (Validation)
                SWASnapshotInputDTO baseDetail_3 = new SWASnapshotInputDTO(Snapshot_Clone2.Id, WellAllowableSnapshotType.Target, Snapshot_Clone2.StartDate.Value, Snapshot_Clone2.EndDate.Value, "Clone2FromBase");
                baseDetail_3.ApprovalStatus = WellAllowableApprovalStatus.Historized;
                WellAllocationService.UpdateWellAllowableSnapshot(baseDetail_3);

                #region Scenario 3 - Cloning
                //Modifying Hierarchy
                //Removing 1 Well from Zone 1 annd assigning same to Zone 2 following same hierarchy
                WellToZoneDTO well = new WellToZoneDTO();
                well.ZoneId = ZoneIDsOfR1A1.Values.ElementAt(0).Id;
                well.WellId = GASInjWell_1.Id;
                well.EndDate = DateTime.Today.AddDays(-2).ToLocalTime();
                WellAllocationService.RemoveWellMappingToZone(well);

                //Assiging removed Well from Zone 1 to Zone 2
                WellToZoneDTO W22Z1R1A1 = AddWellToZone(GASInjWell_1.Id, ZoneIDsOfR1A1_1.Values.ElementAt(1).Id);

                //Creating Cloning snapshot from Cloned snapshot (Cloned 2)
                DateTime startDate_Clone3 = DateTime.Now.AddDays(5).ToUniversalTime();
                DateTime endDate_Clone3 = DateTime.Now.AddDays(6).ToUniversalTime();
                SWASnapshotInputDTO SnapshotDetail_Clone3 = new SWASnapshotInputDTO(Snapshot_Clone2.Id, WellAllowableSnapshotType.Target, startDate_Clone3, endDate_Clone3, "Clone3FromClone2");
                SWASnapshotDTO Snapshot_Clone3 = WellAllocationService.CloneWellAllowableSnapshot(SnapshotDetail_Clone3);
                _setWellAlowableToRemove.Add(Snapshot_Clone3);
                Assert.IsNotNull(Snapshot_Clone3);

                //Getting rate table for Cloned snapshot
                SWAWellBasedDTO wellbasedInjectionDTO_Clone3 = new SWAWellBasedDTO();
                wellbasedInjectionDTO_Clone3.SnapshotId = Snapshot_Clone3.Id;
                wellbasedInjectionDTO_Clone3.AssetId = assets.ElementAt(0).Id;
                wellbasedInjectionDTO_Clone3.WellCategory = WellTypeCategory.Injection;
                wellbasedInjectionDTO_Clone3.FluidPhase = WellAllowablePhase.Gas;

                SWARateArrayAndUnitsDTO ratesFor1stRecord_Clone3 = WellAllocationService.GetWellAllowableRatesForSnapshot(wellbasedInjectionDTO_Clone3);
                Assert.IsNotNull(ratesFor1stRecord_Clone3);

                //Comparing Cloned Wellbound records with Base snapshot Wellbound records 
                Assert.AreEqual(ratesFor1stRecord_Clone3.Values.Count(), ratesFor1stRecord.Values.Count() + 3, "Mismatch in total no of count after cloning");
                #endregion Scenario 3 - Cloning

                #region Created Coned Sanpshot Which Will Be Used In Edit / Removing Functionality 
                //Creating Cloning snapshot from Cloned snapshot (Cloned 3)
                DateTime startDate_Clone4 = DateTime.Now.AddDays(7).ToUniversalTime();
                DateTime endDate_Clone4 = DateTime.Now.AddDays(8).ToUniversalTime();
                SWASnapshotInputDTO SnapshotDetail_Clone4 = new SWASnapshotInputDTO(Snapshot_Clone2.Id, WellAllowableSnapshotType.Sensitivity, startDate_Clone4, endDate_Clone4, "Clone4FromClone3");
                SWASnapshotDTO Snapshot_Clone4 = WellAllocationService.CloneWellAllowableSnapshot(SnapshotDetail_Clone4);
                _setWellAlowableToRemove.Add(Snapshot_Clone4);
                Assert.IsNotNull(Snapshot_Clone4);
                #endregion Created Coned Sanpshot Which Will Be Used In Edit / Removing Functionality 

                Trace.WriteLine("Cloning functionality is working as expected");
                #endregion Cloning Functionality

                #region Edit Functionality

                #region Scenario 1 - Historized Snapshot cannot be edit
                //This functionality is implemented from UI.
                // It should be test from UI.
                #endregion Scenario 1 - Historized Snapshot cannot be edit

                #region Scenario 2 - Only One Target And One Sensitivity Snapshot May Be Active At Any One Time
                //Validation with Target Snapshot for Clone 3 
                //Tryting to edit Clone 3 sanpshot type with sensitivity 
                SWASnapshotInputDTO existingClone3Detail = new SWASnapshotInputDTO(Snapshot_Clone3.Id, WellAllowableSnapshotType.Target, Snapshot_Clone3.StartDate.Value, Snapshot_Clone3.EndDate.Value, "Clone3FromClone2");
                existingClone3Detail.Name = "Clone3FromClone2_Edited";
                existingClone3Detail.Type = WellAllowableSnapshotType.Sensitivity;
                existingClone3Detail.StartDate = DateTime.Now.AddDays(9).ToUniversalTime();
                existingClone3Detail.EndDate = DateTime.Now.AddDays(10).ToUniversalTime();
                bool Clone3Edited = true;
                try
                {
                    Clone3Edited = WellAllocationService.UpdateWellAllowableSnapshot(existingClone3Detail);
                }
                catch (Exception)
                {

                    Clone3Edited = false;
                }
                Assert.IsFalse(Clone3Edited);
                Trace.WriteLine("Working as expected - Clone 3 Snapshot not edited with Sensitivity Type - becasue Clone 4 Snapshot has same type");

                //Tryting to edit Clone 3 sanpshot type with Target 
                SWASnapshotInputDTO existingClone3Detail_1 = new SWASnapshotInputDTO(Snapshot_Clone3.Id, WellAllowableSnapshotType.Target, Snapshot_Clone3.StartDate.Value, Snapshot_Clone3.EndDate.Value, "Clone3FromClone2");
                existingClone3Detail_1.Name = "Clone3FromClone2_Edited";
                existingClone3Detail_1.Type = WellAllowableSnapshotType.Target;
                existingClone3Detail_1.StartDate = DateTime.Now.AddDays(9).ToUniversalTime();
                existingClone3Detail_1.EndDate = DateTime.Now.AddDays(10).ToUniversalTime();
                var Clone3Edited_1 = WellAllocationService.UpdateWellAllowableSnapshot(existingClone3Detail_1);
                Assert.IsTrue(Clone3Edited_1);
                Trace.WriteLine("Clone 3 Snapshot Edited Successfully");
                #endregion Scenario 2 - Only One Target And One Sensitivity Snapshot May Be Active At Any One Time

                #region Scenario 3 - Only One Target And One Sensitivity Snapshot May Be Active At Any One Time
                //Validation with Sensitivity Snapshot for Clone 4 
                //Tryting to edit Clone 4 sanpshot type with Target
                SWASnapshotInputDTO existingClone4Detail = new SWASnapshotInputDTO(Snapshot_Clone4.Id, WellAllowableSnapshotType.Sensitivity, Snapshot_Clone4.StartDate.Value, Snapshot_Clone4.EndDate.Value, "Clone4FromClone3_Edited");
                existingClone4Detail.Name = "Clone4FromClone3_Edited";
                existingClone4Detail.Type = WellAllowableSnapshotType.Target;
                existingClone4Detail.StartDate = DateTime.Now.AddDays(11).ToUniversalTime();
                existingClone4Detail.EndDate = DateTime.Now.AddDays(12).ToUniversalTime();

                bool Clone4Edited = true;
                try
                {
                    Clone4Edited = WellAllocationService.UpdateWellAllowableSnapshot(existingClone4Detail);
                }
                catch (Exception)
                {
                    Clone4Edited = false;
                }
                Assert.IsFalse(Clone4Edited);
                Trace.WriteLine("Working as expected - Clone 4 Snapshot not edited with Target Type - becasue Clone 3 Snapshot has same type");

                //Tryting to edit Clone 3 sanpshot type with Sensitivity
                SWASnapshotInputDTO existingClone4Detail_1 = new SWASnapshotInputDTO(Snapshot_Clone4.Id, WellAllowableSnapshotType.Sensitivity, Snapshot_Clone4.StartDate.Value, Snapshot_Clone4.EndDate.Value, "Clone4FromClone3");
                existingClone4Detail_1.Name = "Clone4FromClone3_Edited";
                existingClone4Detail_1.Type = WellAllowableSnapshotType.Sensitivity;
                existingClone4Detail_1.StartDate = DateTime.Now.AddDays(11).ToUniversalTime();
                existingClone4Detail_1.EndDate = DateTime.Now.AddDays(12).ToUniversalTime();
               
                var Clone4Edited_1 = WellAllocationService.UpdateWellAllowableSnapshot(existingClone4Detail_1);
                Assert.IsTrue(Clone4Edited_1);
                Trace.WriteLine("Clone 4 Snapshot Edited Successfully");
                #endregion Scenario 3 - Only One Target And One Sensitivity Snapshot May Be Active At Any One Time

                #region Verifying Edited Records
                //Get list of available Snapshots
                var snapshotList = WellAllocationService.GetWellAllowableSnapshots();
                Assert.AreEqual("Clone3FromClone2_Edited", snapshotList[3].Name, "Mismatch in edited Snapshot name");
                Assert.AreEqual(WellAllowableSnapshotType.Target, snapshotList[3].SnapshotType, "Mismatch in edited Snapshot type");

                Assert.AreEqual("Clone4FromClone3_Edited", snapshotList[4].Name, "Mismatch in edited Snapshot name");
                Assert.AreEqual(WellAllowableSnapshotType.Sensitivity, snapshotList[4].SnapshotType, "Mismatch in edited Snapshot type");

                Trace.WriteLine("Edit functionality is working as expected");
                #endregion Verifying Edited Records

                #endregion Edit Functionality

                #region Remove Functionality
                //Remove functionality will not remove Snapshot which has Target Type
                WellAllocationService.SetWellAllowableSnapshotStatus(SnapshotClone1.Id.ToString(), false.ToString());
                var snapshotList_1 = WellAllocationService.GetWellAllowableSnapshots();
                Assert.AreEqual(snapshotList_1.Count(), snapshotList.Count(), "Mismatch In Count List - Before Remove And After Remove");

                //Remove functionality will only remove Snapshot which has Sensitivity Type
                WellAllocationService.SetWellAllowableSnapshotStatus(Snapshot_Clone4.Id.ToString(), false.ToString());
                var snapshotList_2 = WellAllocationService.GetWellAllowableSnapshots();
                Assert.AreEqual(snapshotList_2.Count(), snapshotList.Count() - 1, "Mismatch In Count List - Before Remove And After Remove");

                Trace.WriteLine("Remove functionality is woorking as expected");
                #endregion Remove Functionality

                #region Edit Functionality After Removed Sensitivity Type Snapshot
                //Tryting to edit Clone 3 sanpshot type with Sensitivity
                SWASnapshotInputDTO existingClone3Detail_2 = new SWASnapshotInputDTO(Snapshot_Clone3.Id, WellAllowableSnapshotType.Target, Snapshot_Clone3.StartDate.Value, Snapshot_Clone3.EndDate.Value, "Clone3FromClone2");
                existingClone3Detail_2.Name = "Clone3FromClone2_Edited_Sensitivity";
                existingClone3Detail_2.Type = WellAllowableSnapshotType.Sensitivity;
                existingClone3Detail_2.StartDate = DateTime.Now.AddDays(13).ToUniversalTime();
                existingClone3Detail_2.EndDate = DateTime.Now.AddDays(14).ToUniversalTime();
                var Clone3Edited_2 = WellAllocationService.UpdateWellAllowableSnapshot(existingClone3Detail_2);
                Assert.IsTrue(Clone3Edited_2);
                Trace.WriteLine("Clone 3 Snapshot With Sensitivity Type Edited Successfully");

                var snapshotList_3 = WellAllocationService.GetWellAllowableSnapshots();
                Assert.AreEqual("Clone3FromClone2_Edited_Sensitivity", snapshotList_3[3].Name, "Mismatch in edited Snapshot name");
                Assert.AreEqual(WellAllowableSnapshotType.Sensitivity, snapshotList_3[3].SnapshotType, "Mismatch in edited Snapshot type");
                #endregion Edit Functionality After Removed Sensitivity Type Snapshot
            }
            finally
            {
                ChangeUnitSystem("US");
                ChangeUnitSystemUserSetting("US");
            }
        }

        /// <summary>
        /// FRWM-7093 Set Well Allowables : API development for Well Bounds Configuration
        /// FRWM-7209:Get List of Snapshots
        /// FRWM-7314:SWA : Settings Panel - Update Options Backend API
        /// </summary>
        [TestCategory(TestCategories.WellAllocationServiceTests), TestMethod]
        public void SetWellAllowable_WellsBound()
        {
            //Add asset
            #region  Add Asset
            List<AssetDTO> assets = CreateAsset(2);
            #endregion Add Asset
            SWASnapshotDTO snapshotProduction = new SWASnapshotDTO();
            SWASnapshotDTO snapshotInjection = new SWASnapshotDTO();
            ChangeUnitSystem("US");
            ChangeUnitSystemUserSetting("US");

            try
            {
                //Filter with Assets
                #region Filter with Assets
                WellFilterDTO wellbyFilter = new WellFilterDTO();
                wellbyFilter.AssetIds = new long?[] { assets.ElementAt(0).Id, assets.ElementAt(1).Id };
                #endregion Filter with Assets

                #region create network scenario
                SNScenarioDTO scenario = ScenarioConfiguration(assets.ElementAt(0).Id);
                Assert.IsNotNull(scenario);
                #endregion create network scenario

                #region wells created from scenario
                WellDTO GLWell_2 = WellService.GetWellByName("GLWell_2");
                WellDTO NFWWell_2 = WellService.GetWellByName("NFWWell_2");
                WellDTO NFWWell_3 = WellService.GetWellByName("NFWWell_3");
                WellDTO NFWWell_4 = WellService.GetWellByName("NFWWell_4");
                #endregion wells created from scenario

                #region create Wells with Asset
                WellDTO GLWell_1 = AddNonRRLWellGeneral("GLWell_1", GetFacilityId("GLWELL_", 1), WellTypeId.GLift, WellFluidType.BlackOil, WellFluidPhase.None, "1", assets.ElementAt(0).Id);
                WellDTO NFWWell_1 = AddNonRRLWellGeneral("NFWWell_1", GetFacilityId("NFWWELL_", 1), WellTypeId.NF, WellFluidType.BlackOil, WellFluidPhase.None, "1", assets.ElementAt(0).Id);
                WellDTO ESPWell_1 = AddNonRRLWellGeneral("ESPWell_1", GetFacilityId("ESPWELL_", 1), WellTypeId.ESP, WellFluidType.None, WellFluidPhase.None, "1", assets.ElementAt(0).Id);
                WellDTO PCPWell_1 = AddNonRRLWellGeneral("PCPWell_1", GetFacilityId("WFTA1K_", 1), WellTypeId.PCP, WellFluidType.BlackOil, WellFluidPhase.MultiPhase, "1", assets.ElementAt(0).Id);
                WellDTO PGLWell_1 = AddNonRRLWellGeneral("PGLWell_1", GetFacilityId("PGLWELL_", 1), WellTypeId.PLift, WellFluidType.DryGas, WellFluidPhase.None, "1", assets.ElementAt(0).Id);
                WellDTO GASInjWell_1 = AddNonRRLWellGeneral("GASInjWell_1", GetFacilityId("GASINJWELL_", 1), WellTypeId.GInj, WellFluidType.None, WellFluidPhase.None, "1", assets.ElementAt(0).Id);
                WellDTO WATERInjWell_1 = AddNonRRLWellGeneral("WATERInjWell_1", GetFacilityId("WATERINJWELL_", 1), WellTypeId.WInj, WellFluidType.None, WellFluidPhase.None, "1", assets.ElementAt(0).Id);

                AddModelFile(GLWell_1.CommissionDate.Value, WellTypeId.GLift, GLWell_1.Id, CalibrationMethodId.LFactor);
                AddModelFile(NFWWell_1.CommissionDate.Value, WellTypeId.NF, NFWWell_1.Id, CalibrationMethodId.LFactor);
                AddModelFile(ESPWell_1.CommissionDate.Value, WellTypeId.ESP, ESPWell_1.Id, CalibrationMethodId.LFactor);
                AddModelFile(PCPWell_1.CommissionDate.Value, WellTypeId.PCP, PCPWell_1.Id, CalibrationMethodId.LFactor);
                AddModelFile(PGLWell_1.CommissionDate.Value, WellTypeId.PLift, PGLWell_1.Id, CalibrationMethodId.None);
                AddModelFile(GASInjWell_1.CommissionDate.Value, WellTypeId.GInj, GASInjWell_1.Id, CalibrationMethodId.LFactor);
                AddModelFile(WATERInjWell_1.CommissionDate.Value, WellTypeId.WInj, WATERInjWell_1.Id, CalibrationMethodId.LFactor);

                Trace.WriteLine("Wells are created successfully and mapped with Assets");
                #endregion create Wells with Asset

                #region create Reservoir
                //Reservoir under Asset 1
                ReservoirAndUnitsDTO R1A1 = AddReservoirToAsset("R1A1", assets.ElementAt(0).Id);
                Assert.IsNotNull(R1A1);
                ReservoirAndUnitsDTO R2A1 = AddReservoirToAsset("R2A1", assets.ElementAt(0).Id);
                Assert.IsNotNull(R2A1);
                #endregion create Reservoir

                #region getReservoir IDs
                ReservoirArrayAndUnitsDTO ReservoirIDsOfAsset1 = WellAllocationService.GetReservoirsByAssetId(assets.ElementAt(0).Id.ToString());
                Assert.IsNotNull(ReservoirIDsOfAsset1);
                var R1A1_ID = ReservoirIDsOfAsset1.Values.ElementAt(0).Id;
                var R2A1_ID = ReservoirIDsOfAsset1.Values.ElementAt(1).Id;
                #endregion getReservoir IDs

                #region create Zone 
                //Zone under Asset 1
                ZoneAndUnitsDTO Z1R1A1 = AddZoneToReservoir("Z1R1A1", R1A1_ID);
                Assert.IsNotNull(Z1R1A1);
                ZoneAndUnitsDTO Z2R1A1 = AddZoneToReservoir("Z2R1A1", R1A1_ID);
                Assert.IsNotNull(Z2R1A1);
                ZoneAndUnitsDTO Z1R2A1 = AddZoneToReservoir("Z1R2A1", R2A1_ID);
                Assert.IsNotNull(Z1R2A1);
                ZoneAndUnitsDTO Z2R2A1 = AddZoneToReservoir("Z2R2A1", R2A1_ID);
                Assert.IsNotNull(Z2R2A1);
                #endregion create Zone 

                #region getZone IDs
                ZoneArrayAndUnitsDTO ZoneIDsOfR1A1 = WellAllocationService.GetZonesByReservoirId(R1A1_ID.ToString());
                Assert.IsNotNull(ZoneIDsOfR1A1);
                ZoneArrayAndUnitsDTO ZoneIDsOfR2A1 = WellAllocationService.GetZonesByReservoirId(R2A1_ID.ToString());
                Assert.IsNotNull(ZoneIDsOfR2A1);
                #endregion getZone IDs

                #region create Pattern 
                //Pattern under Asset 1
                PatternDTO P1Z1R1A1 = AddPatternToZoneOfReservoir("P1Z1R1A1", ZoneIDsOfR1A1.Values.ElementAt(0).Id);
                Assert.IsNotNull(P1Z1R1A1);
                #endregion create Pattern 

                #region getPattern IDs
                //List<PatternDTO> getPatternByZoneOfReservoir = WellAllocationService.GetPatternsByZoneId(ZoneIDsOfR1A1.Values.ElementAt(0).Id.ToString());
                //Assert.IsNotNull(getPatternByZoneOfReservoir);
                #endregion getPattern IDs

                #region Assign Well to Zone
                //Assign Well under Asset 1
                WellToZoneDTO W1Z1R1A1 = AddWellToZone(GLWell_1.Id, ZoneIDsOfR1A1.Values.ElementAt(0).Id);
                Assert.IsNotNull(W1Z1R1A1);
                WellToZoneDTO W2Z1R1A1 = AddWellToZone(NFWWell_1.Id, ZoneIDsOfR1A1.Values.ElementAt(0).Id);
                Assert.IsNotNull(W2Z1R1A1);
                WellToZoneDTO W3Z1R1A1 = AddWellToZone(PCPWell_1.Id, ZoneIDsOfR1A1.Values.ElementAt(0).Id);
                Assert.IsNotNull(W3Z1R1A1);
                WellToZoneDTO W4Z1R1A1 = AddWellToZone(GASInjWell_1.Id, ZoneIDsOfR1A1.Values.ElementAt(0).Id);
                Assert.IsNotNull(W4Z1R1A1);

                WellToZoneDTO W5Z2R1A1 = AddWellToZone(ESPWell_1.Id, ZoneIDsOfR2A1.Values.ElementAt(0).Id);
                Assert.IsNotNull(W5Z2R1A1);
                WellToZoneDTO W6Z2R1A1 = AddWellToZone(PGLWell_1.Id, ZoneIDsOfR2A1.Values.ElementAt(0).Id);
                Assert.IsNotNull(W6Z2R1A1);
                WellToZoneDTO W7Z2R1A1 = AddWellToZone(WATERInjWell_1.Id, ZoneIDsOfR2A1.Values.ElementAt(0).Id);
                Assert.IsNotNull(W7Z2R1A1);


                WellToZoneDTO W8Z2R1A1 = AddWellToZone(GLWell_2.Id, ZoneIDsOfR2A1.Values.ElementAt(0).Id);
                Assert.IsNotNull(W8Z2R1A1);
                WellToZoneDTO W9Z2R1A1 = AddWellToZone(NFWWell_2.Id, ZoneIDsOfR2A1.Values.ElementAt(0).Id);
                Assert.IsNotNull(W9Z2R1A1);
                WellToZoneDTO W10Z2R1A1 = AddWellToZone(NFWWell_3.Id, ZoneIDsOfR2A1.Values.ElementAt(0).Id);
                Assert.IsNotNull(W10Z2R1A1);
                #endregion Assign Well to Zone

                #region Assign Well to Pattern
                // issue with clean up - hence, didn't assign wells to pattern 

                //WellToPatternDTO W1P1Z1R1A1 = AddWellToPattern(GLWell_1.Id, getPatternByZoneOfReservoir.ElementAt(0).Id);
                //Assert.IsNotNull(W1P1Z1R1A1);
                #endregion Assign Well to Pattern

                #region Set Well Allowable - Production
                string startDateProduction = DTOExtensions.ToISO8601(DateTime.Now.AddDays(-30).ToUniversalTime());
                string endDateProduction = DTOExtensions.ToISO8601(DateTime.Now.AddDays(-20).ToUniversalTime());
                string productionSnapshotName = "Production SnapShot 1";
                string productionSnapshotType = WellAllowableSnapshotType.Sensitivity.ToString();

                //Create Snapshot
                snapshotProduction = WellAllocationService.CreateWellAllowableSnapshot(productionSnapshotName, productionSnapshotType, assets.ElementAt(0).Id.ToString(), startDateProduction, endDateProduction, WellTypeCategory.Production.ToString(), WellAllowablePhase.Gas.ToString(), scenario.Id.ToString());
                Assert.IsNotNull(snapshotProduction);
                _setWellAlowableToRemove.Add(snapshotProduction);

                //Getting Rate table as per Snapshot
                SWAWellBasedDTO wellbasedProductionDTO = new SWAWellBasedDTO();
                wellbasedProductionDTO.SnapshotId = snapshotProduction.Id;
                wellbasedProductionDTO.AssetId = assets.ElementAt(0).Id;
                wellbasedProductionDTO.WellCategory = WellTypeCategory.Production;
                wellbasedProductionDTO.FluidPhase = WellAllowablePhase.Gas;

                SWARateArrayAndUnitsDTO ratesForAsset1Production = WellAllocationService.GetWellAllowableRatesForSnapshot(wellbasedProductionDTO);
                Assert.IsNotNull(ratesForAsset1Production);
                Assert.AreEqual(8, ratesForAsset1Production.Values.Count(), "Mismatch in count");
                Assert.AreEqual(true, ratesForAsset1Production.Values[0].InAnalysis, "Mismatch in InAnalysis column for 1st record");
                Assert.AreEqual("GLWell_1", ratesForAsset1Production.Values[0].Well.Name, "Mismatch in WellName column for 1st record");
                Assert.AreEqual("Z1R1A1", ratesForAsset1Production.Values[0].Zone.Value.ZoneName, "Mismatch in ZoneName column for 1st record");
                Assert.AreEqual("GLWell_1_Field", ratesForAsset1Production.Values[0].TrunkLine.Value.Name, "Mismatch in TrunkLine column for 1st record");
                Assert.IsNull(ratesForAsset1Production.Values[0].WellheadPressureBound);
                Assert.IsNull(ratesForAsset1Production.Values[0].PerforationPressureChangeBound);
                Assert.IsNull(ratesForAsset1Production.Values[0].ReservoirPressureBound);
                Assert.IsNull(ratesForAsset1Production.Values[0].ReservoirPressureTolerance);
                Assert.IsNull(ratesForAsset1Production.Values[0].MaximumRate);
                Assert.IsNull(ratesForAsset1Production.Values[0].MinimumRate);
                Assert.IsNotNull(ratesForAsset1Production.Values[0].WellAvailability);

                ratesForAsset1Production.Values[0].WellheadPressureBound = 30;
                ratesForAsset1Production.Values[0].PerforationPressureChangeBound = 40;
                ratesForAsset1Production.Values[0].ReservoirPressureBound = 50;
                ratesForAsset1Production.Values[0].ReservoirPressureTolerance = 60;
                ratesForAsset1Production.Values[0].MaximumRate = 70;
                ratesForAsset1Production.Values[0].MinimumRate = 80;
                ratesForAsset1Production.Values[0].WellAvailability = 1;
                ratesForAsset1Production.Values[0].InAnalysis = false;

                wellbasedProductionDTO.Rates = ratesForAsset1Production.Values;
                //Update Rate table snap shot
                var updaterateAssetProduction = WellAllocationService.UpdateWellAllowableRatesForSnapshot(wellbasedProductionDTO);
                Assert.IsNotNull(updaterateAssetProduction);

                //Verification of updated record
                SWAWellBasedDTO wellbasedProductionDTO_Updated = new SWAWellBasedDTO();
                wellbasedProductionDTO_Updated.SnapshotId = snapshotProduction.Id;
                wellbasedProductionDTO_Updated.AssetId = assets.ElementAt(0).Id;
                wellbasedProductionDTO_Updated.WellCategory = WellTypeCategory.Production;
                wellbasedProductionDTO_Updated.FluidPhase = WellAllowablePhase.Gas;
                SWARateArrayAndUnitsDTO ratesForAsset1Production_Updated = WellAllocationService.GetWellAllowableRatesForSnapshot(wellbasedProductionDTO_Updated);
                Assert.IsNotNull(ratesForAsset1Production_Updated);
                Assert.AreEqual(8, ratesForAsset1Production_Updated.Values.Count(), "Mismatch in count");
                Assert.AreEqual(false, ratesForAsset1Production_Updated.Values[0].InAnalysis, "Mismatch in InAnalysis column for 1st record");
                Assert.AreEqual("GLWell_1", ratesForAsset1Production_Updated.Values[0].Well.Name, "Mismatch in WellName column for 1st record");
                Assert.AreEqual("Z1R1A1", ratesForAsset1Production_Updated.Values[0].Zone.Value.ZoneName, "Mismatch in ZoneName column for 1st record");
                Assert.AreEqual("GLWell_1_Field", ratesForAsset1Production_Updated.Values[0].TrunkLine.Value.Name, "Mismatch in TrunkLine column for 1st record");
                Assert.AreEqual(30, ratesForAsset1Production_Updated.Values[0].WellheadPressureBound, "Mismatch for Minimum Wellhead Pressure");
                Assert.AreEqual(40, ratesForAsset1Production_Updated.Values[0].PerforationPressureChangeBound, "Mismatch for Maximum Drawdown");
                Assert.AreEqual(50, ratesForAsset1Production_Updated.Values[0].ReservoirPressureBound, "Mismatch for Saturation Pressure");
                Assert.AreEqual(60, ratesForAsset1Production_Updated.Values[0].ReservoirPressureTolerance, "Mismatch for Saturation Pressure Tolerance");
                Assert.AreEqual(70, ratesForAsset1Production_Updated.Values[0].MaximumRate, "Mismatch for Maximum Rate");
                Assert.AreEqual(80, ratesForAsset1Production_Updated.Values[0].MinimumRate, "Mismatch for Minimum Rate");
                Assert.AreEqual(1, ratesForAsset1Production_Updated.Values[0].WellAvailability, "Mismatch in Well Availability Factor");
                #endregion Set Well Allowable - Production

                #region Set Well Allowable - Injection
                string startDateInjection = DTOExtensions.ToISO8601(DateTime.Now.AddDays(-30).ToUniversalTime());
                string endDateInjection = DTOExtensions.ToISO8601(DateTime.Now.AddDays(-20).ToUniversalTime());
                string injectionSnapshotName = "Injection SnapShot 1";
                string injetionSnapshotType = WellAllowableSnapshotType.Target.ToString();

                //Create Snapshot
                snapshotInjection = WellAllocationService.CreateWellAllowableSnapshot(injectionSnapshotName, injetionSnapshotType, assets.ElementAt(0).Id.ToString(), startDateInjection, endDateInjection, WellTypeCategory.Injection.ToString(), WellAllowablePhase.Gas.ToString(), scenario.Id.ToString());
                Assert.IsNotNull(snapshotInjection);
                _setWellAlowableToRemove.Add(snapshotInjection);

                //Getting Ratetables as per Snapshot
                SWAWellBasedDTO wellbasedInjectionDTO = new SWAWellBasedDTO();
                wellbasedInjectionDTO.SnapshotId = snapshotInjection.Id;
                wellbasedInjectionDTO.AssetId = assets.ElementAt(0).Id;
                wellbasedInjectionDTO.WellCategory = WellTypeCategory.Injection;
                wellbasedInjectionDTO.FluidPhase = WellAllowablePhase.Gas;

                SWARateArrayAndUnitsDTO ratesForAsset1Injection = WellAllocationService.GetWellAllowableRatesForSnapshot(wellbasedInjectionDTO);
                Assert.IsNotNull(ratesForAsset1Injection);
                Assert.AreEqual(2, ratesForAsset1Injection.Values.Count(), "Mismatch in count");
                Assert.AreEqual(true, ratesForAsset1Injection.Values[0].InAnalysis, "Mismatch in InAnalysis column for 1st record");
                Assert.AreEqual("GASInjWell_1", ratesForAsset1Injection.Values[0].Well.Name, "Mismatch in WellName column for 1st record");
                Assert.AreEqual("Z1R1A1", ratesForAsset1Injection.Values[0].Zone.Value.ZoneName, "Mismatch in ZoneName column for 1st record");
                Assert.AreEqual("GASInjWell_1_Field", ratesForAsset1Injection.Values[0].TrunkLine.Value.Name, "Mismatch in TrunkLine column for 1st record");
                Assert.IsNull(ratesForAsset1Injection.Values[0].WellheadPressureBound);
                Assert.IsNull(ratesForAsset1Injection.Values[0].PerforationPressureChangeBound);
                Assert.IsNull(ratesForAsset1Injection.Values[0].ReservoirPressureBound);
                Assert.IsNull(ratesForAsset1Injection.Values[0].ReservoirPressureTolerance);
                Assert.IsNull(ratesForAsset1Injection.Values[0].MaximumRate);
                Assert.IsNull(ratesForAsset1Injection.Values[0].MinimumRate);
                Assert.IsNotNull(ratesForAsset1Injection.Values[0].WellAvailability);

                ratesForAsset1Injection.Values[0].WellheadPressureBound = 31;
                ratesForAsset1Injection.Values[0].PerforationPressureChangeBound = 41;
                ratesForAsset1Injection.Values[0].ReservoirPressureBound = 51;
                ratesForAsset1Injection.Values[0].ReservoirPressureTolerance = 61;
                ratesForAsset1Injection.Values[0].MaximumRate = 71;
                ratesForAsset1Injection.Values[0].MinimumRate = 81;
                ratesForAsset1Injection.Values[0].WellAvailability = 1;
                ratesForAsset1Injection.Values[0].InAnalysis = false;

                //Update Rate table snap shot
                wellbasedInjectionDTO.Rates = ratesForAsset1Injection.Values;
                var updaterateAssetInjection = WellAllocationService.UpdateWellAllowableRatesForSnapshot(wellbasedInjectionDTO);
                Assert.IsNotNull(updaterateAssetInjection);

                //Verification of updated record
                SWAWellBasedDTO wellbasedInjectionDTO_Updated = new SWAWellBasedDTO();
                wellbasedInjectionDTO_Updated.SnapshotId = snapshotInjection.Id;
                wellbasedInjectionDTO_Updated.AssetId = assets.ElementAt(0).Id;
                wellbasedInjectionDTO_Updated.WellCategory = WellTypeCategory.Injection;
                wellbasedInjectionDTO_Updated.FluidPhase = WellAllowablePhase.Gas;

                SWARateArrayAndUnitsDTO ratesForAsset1Injection_Updated = WellAllocationService.GetWellAllowableRatesForSnapshot(wellbasedInjectionDTO_Updated);
                Assert.IsNotNull(ratesForAsset1Injection_Updated);
                Assert.AreEqual(2, ratesForAsset1Injection_Updated.Values.Count(), "Mismatch in count");
                Assert.AreEqual(false, ratesForAsset1Injection_Updated.Values[0].InAnalysis, "Mismatch in InAnalysis column for 1st record");
                Assert.AreEqual("GASInjWell_1", ratesForAsset1Injection_Updated.Values[0].Well.Name, "Mismatch in WellName column for 1st record");
                Assert.AreEqual("Z1R1A1", ratesForAsset1Injection_Updated.Values[0].Zone.Value.ZoneName, "Mismatch in ZoneName column for 1st record");
                Assert.AreEqual("GASInjWell_1_Field", ratesForAsset1Injection_Updated.Values[0].TrunkLine.Value.Name, "Mismatch in TrunkLine column for 1st record");
                Assert.AreEqual(31, ratesForAsset1Injection_Updated.Values[0].WellheadPressureBound, "Mismatch for Maximum Wellhead Pressure");
                Assert.AreEqual(41, ratesForAsset1Injection_Updated.Values[0].PerforationPressureChangeBound, "Mismatch for Injection Margin");
                Assert.AreEqual(51, ratesForAsset1Injection_Updated.Values[0].ReservoirPressureBound, "Mismatch for Fracture Pressure");
                Assert.AreEqual(61, ratesForAsset1Injection_Updated.Values[0].ReservoirPressureTolerance, "Mismatch for Fracture Pressure Tolerance");
                Assert.AreEqual(71, ratesForAsset1Injection_Updated.Values[0].MaximumRate, "Mismatch for Maximum Rate");
                Assert.AreEqual(81, ratesForAsset1Injection_Updated.Values[0].MinimumRate, "Mismatch for Minimum Rate");
                Assert.AreEqual(1, ratesForAsset1Injection_Updated.Values[0].WellAvailability, "Mismatch in Well Availability Factor");
                #endregion Set Well Allowable - Injection

                #region List of SnapShot
                //Getting list of Snapshot
                SWASnapshotDTO[] getSnapshotlists = WellAllocationService.GetWellAllowableSnapshots();
                Assert.IsNotNull(getSnapshotlists);
                Assert.AreEqual(2, getSnapshotlists.Count(), "Mismatch in total no of snapshot list count");

                Assert.AreEqual(snapshotProduction.Id, getSnapshotlists[0].Id, "Mismatch in Snapshot ID");
                Assert.AreEqual(productionSnapshotName, getSnapshotlists[0].Name, "Mismatch in Snapshot Name");
                Assert.AreEqual(snapshotProduction.StartDate.Value.ToString(), getSnapshotlists[0].StartDate.Value.ToString(), "Mismatch in Start Date");
                Assert.AreEqual(snapshotProduction.EndDate.Value.ToString(), getSnapshotlists[0].EndDate.Value.ToString(), "Mismatch in End Date");
                Assert.AreEqual(productionSnapshotType, getSnapshotlists[0].SnapshotType.ToString(), "Mismatch in Snapshot Type");

                Assert.AreEqual(snapshotInjection.Id, getSnapshotlists[1].Id, "Mismatch in Snapshot ID");
                Assert.AreEqual(injectionSnapshotName, getSnapshotlists[1].Name, "Mismatch in Snapshot Name");
                Assert.AreEqual(snapshotInjection.StartDate.Value.ToString(), getSnapshotlists[1].StartDate.Value.ToString(), "Mismatch in Start Date");
                Assert.AreEqual(snapshotInjection.EndDate.Value.ToString(), getSnapshotlists[1].EndDate.Value.ToString(), "Mismatch in End Date");
                Assert.AreEqual(injetionSnapshotType, getSnapshotlists[1].SnapshotType.ToString(), "Mismatch in Snapshot Type");

                //Veryfying Asset Setting Panel
                Assert.AreEqual("Gas", getSnapshotlists[0].AssetSettings.Values[0].Phase.ToString(), "Mismatch in Phase");
                Assert.AreEqual("Tenths", getSnapshotlists[0].AssetSettings.Values[0].RoundingLevel.ToString(), "Mismatch in Precision for Allowable Rate");
                Assert.AreEqual(false, getSnapshotlists[0].AssetSettings.Values[0].EnableBackpressureCheck, "Mismatch in Backpressure selection");

                Assert.AreEqual("Gas", getSnapshotlists[1].AssetSettings.Values[0].Phase.ToString(), "Mismatch in Phase");
                Assert.AreEqual("Tenths", getSnapshotlists[1].AssetSettings.Values[0].RoundingLevel.ToString(), "Mismatch in Precision for Allowable Rate");
                Assert.AreEqual(false, getSnapshotlists[1].AssetSettings.Values[0].EnableBackpressureCheck, "Mismatch in Backpressure selection");
                #endregion List of Snapshot

                #region Updating setting panel for Production / Injection Setting Panel
                foreach (var snapshotItem in getSnapshotlists)
                {
                    snapshotItem.AssetSettings.Values[0].Phase = WellAllowablePhase.Liquid;
                    snapshotItem.AssetSettings.Values[0].EnableBackpressureCheck = true;
                    snapshotItem.AssetSettings.Values[0].RoundingLevel = WellAllowableRoundingLevel.Thousands;

                    //Update panel option with snapshot detail
                    SWAAssetSettingsAndUnitsDTO updatedAssetSettings = WellAllocationService.UpdateWellAllowableAssetSettings(snapshotItem.AssetSettings.Values[0]);
                    Assert.IsNotNull(updatedAssetSettings);
                }
                //Veryfying updated setting panel for production snapshot
                Assert.AreEqual("Liquid", getSnapshotlists[0].AssetSettings.Values[0].Phase.ToString(), "Mismatch in Phase");
                Assert.AreEqual("Thousands", getSnapshotlists[0].AssetSettings.Values[0].RoundingLevel.ToString(), "Mismatch in Precision for Allowable Rate");
                Assert.AreEqual(true, getSnapshotlists[0].AssetSettings.Values[0].EnableBackpressureCheck, "Mismatch in Backpressure selection");

                //Veryfying updated setting panel for injection snapshot
                Assert.AreEqual("Liquid", getSnapshotlists[1].AssetSettings.Values[0].Phase.ToString(), "Mismatch in Phase");
                Assert.AreEqual("Thousands", getSnapshotlists[1].AssetSettings.Values[0].RoundingLevel.ToString(), "Mismatch in Precision for Allowable Rate");
                Assert.AreEqual(true, getSnapshotlists[1].AssetSettings.Values[0].EnableBackpressureCheck, "Mismatch in Backpressure selection");
                #endregion Updating setting panel for Production / Injection Setting Panel

                ChangeUnitSystem("Metric");
                ChangeUnitSystemUserSetting("Metric");

                double fct_Pressure = 6.894757;
                //double fct_FlowRate = 0.1589873;
                double fct_FlowRate = 28.3168466;
                double tol = 0.15;

                #region Set Well Allowable - Production -Metric Unit System
                //Getting Rate table as per Snapshot
                SWAWellBasedDTO wellbasedProductionDTO_Metric = new SWAWellBasedDTO();
                wellbasedProductionDTO_Metric.SnapshotId = snapshotProduction.Id;
                wellbasedProductionDTO_Metric.AssetId = assets.ElementAt(0).Id;
                wellbasedProductionDTO_Metric.WellCategory = WellTypeCategory.Production;
                wellbasedProductionDTO_Metric.FluidPhase = WellAllowablePhase.Gas;

                SWARateArrayAndUnitsDTO ratesForAsset1Production_Metric = WellAllocationService.GetWellAllowableRatesForSnapshot(wellbasedProductionDTO_Metric);
                Assert.IsNotNull(ratesForAsset1Production_Metric);
                Assert.AreEqual(8, ratesForAsset1Production_Metric.Values.Count(), "Mismatch in count");
                Assert.AreEqual(false, ratesForAsset1Production_Metric.Values[0].InAnalysis, "Mismatch in InAnalysis column for 1st record");
                Assert.AreEqual("GLWell_1", ratesForAsset1Production_Metric.Values[0].Well.Name, "Mismatch in WellName column for 1st record");
                Assert.AreEqual("Z1R1A1", ratesForAsset1Production_Metric.Values[0].Zone.Value.ZoneName, "Mismatch in ZoneName column for 1st record");
                Assert.AreEqual("GLWell_1_Field", ratesForAsset1Production_Metric.Values[0].TrunkLine.Value.Name, "Mismatch in TrunkLine column for 1st record");

                Assert.AreEqual(GetTruncatedValueforDouble((double)(30 * fct_Pressure), 2), (double)ratesForAsset1Production_Metric.Values[0].WellheadPressureBound, (double)tol, "Mismatch for Minimum Wellhead Pressure");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(40 * fct_Pressure), 2), (double)ratesForAsset1Production_Metric.Values[0].PerforationPressureChangeBound, (double)tol, "Mismatch for Maximum Drawdown");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(50 * fct_Pressure), 2), (double)ratesForAsset1Production_Metric.Values[0].ReservoirPressureBound, (double)tol, "Mismatch for Saturation Pressure");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(60 * fct_Pressure), 2), (double)ratesForAsset1Production_Metric.Values[0].ReservoirPressureTolerance, (double)tol, "Mismatch for Saturation Pressure Tolerance");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(70 * fct_FlowRate), 2), (double)ratesForAsset1Production_Metric.Values[0].MaximumRate, (double)tol, "Mismatch for Maximum Rate");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(80 * fct_FlowRate), 2), (double)ratesForAsset1Production_Metric.Values[0].MinimumRate, (double)tol, "Mismatch for Minimum Rate");
                Assert.AreEqual(1, ratesForAsset1Production_Metric.Values[0].WellAvailability, "Mismatch in Well Availability Factor");
                #endregion Set Well Allowable - Production -Metric Unit System

                #region Set Well Allowable - Injection Metric Unit System
                //Getting Ratetables as per Snapshot
                SWAWellBasedDTO wellbasedInjectionDTO_Metric = new SWAWellBasedDTO();
                wellbasedInjectionDTO_Metric.SnapshotId = snapshotInjection.Id;
                wellbasedInjectionDTO_Metric.AssetId = assets.ElementAt(0).Id;
                wellbasedInjectionDTO_Metric.WellCategory = WellTypeCategory.Injection;
                wellbasedInjectionDTO_Metric.FluidPhase = WellAllowablePhase.Gas;

                SWARateArrayAndUnitsDTO ratesForAsset1Injection_Metric = WellAllocationService.GetWellAllowableRatesForSnapshot(wellbasedInjectionDTO_Metric);
                Assert.IsNotNull(ratesForAsset1Injection_Metric);
                Assert.AreEqual(2, ratesForAsset1Injection_Metric.Values.Count(), "Mismatch in count");
                Assert.AreEqual(false, ratesForAsset1Injection_Metric.Values[0].InAnalysis, "Mismatch in InAnalysis column for 1st record");
                Assert.AreEqual("GASInjWell_1", ratesForAsset1Injection_Metric.Values[0].Well.Name, "Mismatch in WellName column for 1st record");
                Assert.AreEqual("Z1R1A1", ratesForAsset1Injection_Metric.Values[0].Zone.Value.ZoneName, "Mismatch in ZoneName column for 1st record");
                Assert.AreEqual("GASInjWell_1_Field", ratesForAsset1Injection_Metric.Values[0].TrunkLine.Value.Name, "Mismatch in TrunkLine column for 1st record");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(31 * fct_Pressure), 2), (double)ratesForAsset1Injection_Metric.Values[0].WellheadPressureBound, (double)tol, "Mismatch for Maximum Wellhead Pressure");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(41 * fct_Pressure), 2), (double)ratesForAsset1Injection_Metric.Values[0].PerforationPressureChangeBound, (double)tol, "Mismatch for Injection Margin");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(51 * fct_Pressure), 2), (double)ratesForAsset1Injection_Metric.Values[0].ReservoirPressureBound, (double)tol, "Mismatch for Fracture Pressure");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(61 * fct_Pressure), 2), (double)ratesForAsset1Injection_Metric.Values[0].ReservoirPressureTolerance, (double)tol, "Mismatch for Fracture Pressure Tolerance");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(71 * fct_FlowRate), 2), (double)ratesForAsset1Injection_Metric.Values[0].MaximumRate, (double)tol, "Mismatch for Maximum Rate");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(81 * fct_FlowRate), 2), (double)ratesForAsset1Injection_Metric.Values[0].MinimumRate, (double)tol, "Mismatch for Minimum Rate");
                Assert.AreEqual(1, ratesForAsset1Injection_Metric.Values[0].WellAvailability, tol.ToString(), "Mismatch in Well Availability Factor");
                #endregion Set Well Allowable - Injection Metric Unit System

            }
            finally
            {
                ChangeUnitSystem("US");
                ChangeUnitSystemUserSetting("US");
                //WellAllocationService.DeleteWellAllowableSnapshot(snapshotProduction.Id.ToString());
                //WellAllocationService.DeleteWellAllowableSnapshot(snapshotInjection.Id.ToString());
                WellAllocationService.DeleteHierarchyByAssetId(assets.ElementAt(0).Id.ToString());
                WellAllocationService.DeleteHierarchyByAssetId(assets.ElementAt(1).Id.ToString());
            }
        }
        // FRWM- 7094 Set Well Allowables : API development for Well Asset Configuration
        [TestCategory(TestCategories.WellAllocationServiceTests), TestMethod]
        public void SetWellAllowable_Asset_Bound_Production_InjectionType()
        {
            #region Set Well Allowable - Production -US Unit System
            ChangeUnitSystem("US");
            ChangeUnitSystemUserSetting("US");
            #endregion

            #region  Add Asset
            List<AssetDTO> assets = CreateAsset(2);
            #endregion Add Asset
            SWASnapshotDTO snapshotProduction = new SWASnapshotDTO();
            SWASnapshotDTO snapshotInjection = new SWASnapshotDTO();
            try
            {

                #region Add Non RRL Wells
                WellDTO GLWell_1 = AddNonRRLWellGeneral("GLWell_1", GetFacilityId("GLWELL_", 1), WellTypeId.GLift, WellFluidType.BlackOil, WellFluidPhase.None, "1", assets.ElementAt(0).Id);
                WellDTO NFWWell_1 = AddNonRRLWellGeneral("NFWWell_1", GetFacilityId("NFWWELL_", 1), WellTypeId.NF, WellFluidType.BlackOil, WellFluidPhase.None, "1", assets.ElementAt(0).Id);
                WellDTO ESPWell_1 = AddNonRRLWellGeneral("ESPWell_1", GetFacilityId("ESPWELL_", 1), WellTypeId.ESP, WellFluidType.None, WellFluidPhase.None, "1", assets.ElementAt(0).Id);
                WellDTO PCPWell_1 = AddNonRRLWellGeneral("PCPWell_1", GetFacilityId("WFTA1K_", 1), WellTypeId.PCP, WellFluidType.BlackOil, WellFluidPhase.MultiPhase, "1", assets.ElementAt(0).Id);
                WellDTO PGLWell_1 = AddNonRRLWellGeneral("PGLWell_1", GetFacilityId("PGLWELL_", 1), WellTypeId.PLift, WellFluidType.DryGas, WellFluidPhase.None, "1", assets.ElementAt(0).Id);
                WellDTO GASInjWell_1 = AddNonRRLWellGeneral("GASInjWell_1", GetFacilityId("GASINJWELL_", 1), WellTypeId.GInj, WellFluidType.None, WellFluidPhase.None, "1", assets.ElementAt(0).Id);
                WellDTO WATERInjWell_1 = AddNonRRLWellGeneral("WATERInjWell_1", GetFacilityId("WATERINJWELL_", 1), WellTypeId.WInj, WellFluidType.None, WellFluidPhase.None, "1", assets.ElementAt(0).Id);
                #endregion

                #region create network scenario
                SNScenarioDTO scenario = ScenarioConfiguration(assets.ElementAt(0).Id);
                Assert.IsNotNull(scenario);
                #endregion create network scenario

                #region Add Reservoir
                // Add Reservoir
                ReservoirAndUnitsDTO addReservoir1 = AddReservoirToAsset("Res1", assets.ElementAt(0).Id);
                // GetReservoir IDs
                ReservoirArrayAndUnitsDTO getReservoirByAsset = WellAllocationService.GetReservoirsByAssetId(assets.ElementAt(0).Id.ToString());
                #endregion

                #region Add Multiple Zones
                // Add Zone
                ZoneAndUnitsDTO addZone1 = AddZoneToReservoir("Zone1", getReservoirByAsset.Values[0].Id);
                ZoneAndUnitsDTO addZone2 = AddZoneToReservoir("Zone2", getReservoirByAsset.Values[0].Id);
                ZoneAndUnitsDTO addZone3 = AddZoneToReservoir("Zone3", getReservoirByAsset.Values[0].Id);
                ZoneAndUnitsDTO addZone4 = AddZoneToReservoir("Zone4", getReservoirByAsset.Values[0].Id);
                ZoneAndUnitsDTO addZone5 = AddZoneToReservoir("Zone5", getReservoirByAsset.Values[0].Id);
                #endregion

                #region Get Zone IDs based on reservoir
                // Get Zone IDs based on reservoir
                ZoneArrayAndUnitsDTO getZoneByReservoir = WellAllocationService.GetZonesByReservoirId(getReservoirByAsset.Values[0].Id.ToString());
                #endregion

                #region AddWellToZone

                //Adding wells in each zone
                AddWellToZone(GLWell_1.Id, getZoneByReservoir.Values[0].Id);
                AddWellToZone(NFWWell_1.Id, getZoneByReservoir.Values[0].Id);
                AddWellToZone(ESPWell_1.Id, getZoneByReservoir.Values[1].Id);
                AddWellToZone(PCPWell_1.Id, getZoneByReservoir.Values[1].Id);
                AddWellToZone(PGLWell_1.Id, getZoneByReservoir.Values[2].Id);
                AddWellToZone(GASInjWell_1.Id, getZoneByReservoir.Values[2].Id);
                AddWellToZone(WATERInjWell_1.Id, getZoneByReservoir.Values[3].Id);
                #endregion

                #region Set Well Allowable - Production
                var startDate = DTOExtensions.ToISO8601(DateTime.Today.ToUniversalTime());
                var endDate = DTOExtensions.ToISO8601(DateTime.Today.ToUniversalTime().AddDays(1));

                //Create Snapshot
                snapshotProduction = WellAllocationService.CreateWellAllowableSnapshot("Snapshot1", WellAllowableSnapshotType.Sensitivity.ToString(), assets.ElementAt(0).Id.ToString(), startDate, endDate, WellTypeCategory.Production.ToString(), ProductionType.Oil.ToString(), scenario.Id.ToString());
                SWAAssetBasedRequestDTO sWAAssetBasedRequestDTO = new SWAAssetBasedRequestDTO() { AssetId = assets.ElementAt(0).Id, WellCategory = WellTypeCategory.Production, SnapshotId = snapshotProduction.Id, FluidPhase = WellAllowablePhase.Oil };
                // Getting well allowable Asset based on snapshot
                SWAAssetBasedDTO AssetBasedDTO = WellAllocationService.GetWellAllowableAssetBasedSnapShot(sWAAssetBasedRequestDTO);
                Assert.IsNotNull(AssetBasedDTO);
                Assert.IsNotNull(AssetBasedDTO.SWAAssetZonesDto);
                Assert.IsNotNull(AssetBasedDTO.SWAAssetTrunkLinesDto);
                Assert.AreEqual("Oil", AssetBasedDTO.SWAAssetSettingsDto.Value.Phase.ToString(), "Phase type value is mismatched");
                AssetBasedDTO.SWAAssetSettingsDto.Value.SystemEfficiency = (decimal?)0.111;
                AssetBasedDTO.SWAAssetSettingsDto.Value.UnplannedFactor = (decimal?)0.222;
                AssetBasedDTO.SWAAssetSettingsDto.Value.FacilityAvailability = (decimal?)0.3333;

                //Adding TargetExportCondition,TargetFieldCondition & Capacity values
                for (int i = 0; i < AssetBasedDTO.SWAAssetZonesDto.Values.Length; i++)
                {
                    AssetBasedDTO.SWAAssetZonesDto.Values[i].TargetExportCondition = (decimal?)0.1;
                    AssetBasedDTO.SWAAssetZonesDto.Values[i].TargetFieldCondition = (decimal?)((0.1) / (0.111 * 0.222 * 0.333));
                }

                for (int i = 0; i < AssetBasedDTO.SWAAssetTrunkLinesDto.Values.Length; i++)
                {
                    AssetBasedDTO.SWAAssetTrunkLinesDto.Values[i].Capacity = (decimal?)0.3;
                }
                SWAAssetSettingsDTO swasettings = new SWAAssetSettingsDTO();
                swasettings = AssetBasedDTO.SWAAssetSettingsDto.Value;

                AssetBasedDTO.SaveAssetingParameter = swasettings;
                AssetBasedDTO.SWARequestDTO = sWAAssetBasedRequestDTO;
                AssetBasedDTO.SavedAssetZoneDTOs = AssetBasedDTO.SWAAssetZonesDto.Values;
                AssetBasedDTO.SavedTrunkLineDTOs = AssetBasedDTO.SWAAssetTrunkLinesDto.Values;
                //Saving values in db
                Assert.IsNotNull(AssetBasedDTO.SaveAssetingParameter);
                WellAllocationService.SaveWellAllowableAssetBasedSnapShot(AssetBasedDTO);

                //Get well allowable asset based on snapshot

                AssetBasedDTO = WellAllocationService.GetWellAllowableAssetBasedSnapShot(sWAAssetBasedRequestDTO);
                Assert.IsNotNull(AssetBasedDTO.SWAAssetSettingsDto.Value.UnplannedFactor, "UnplannedFactor value is not updated");
                Assert.IsNotNull(AssetBasedDTO.SWAAssetSettingsDto.Value.FacilityAvailability, "FacilityAvailability value is not updated");
                Assert.IsNotNull(AssetBasedDTO.SWAAssetZonesDto.Values[0].TargetExportCondition, "TargetExportCondition value is not updated");
                Assert.IsNotNull(AssetBasedDTO.SWAAssetZonesDto.Values[0].TargetFieldCondition, "TargetFieldCondition value is not updated");
                Assert.IsNotNull(AssetBasedDTO.SWAAssetTrunkLinesDto.Values[0].Capacity, "Capacity value is not updated");
                Assert.IsNotNull(AssetBasedDTO.SWAAssetSettingsDto.Value.SystemEfficiency, "SystemEfficiency value is not updated");
                Assert.AreEqual(0.111, Math.Round((double)AssetBasedDTO.SWAAssetSettingsDto.Value.SystemEfficiency, 3));
                Assert.AreEqual(0.222, Math.Round((double)AssetBasedDTO.SWAAssetSettingsDto.Value.UnplannedFactor, 3));
                Assert.AreEqual(0.333, Math.Round((double)AssetBasedDTO.SWAAssetSettingsDto.Value.FacilityAvailability, 3));
                Assert.AreEqual(0.1, Math.Round((double)AssetBasedDTO.SWAAssetZonesDto.Values[0].TargetExportCondition, 1));
                Assert.AreEqual(12.2, Math.Round((double)AssetBasedDTO.SWAAssetZonesDto.Values[0].TargetFieldCondition, 1));
                Assert.AreEqual(0.3, Math.Round((double)AssetBasedDTO.SWAAssetTrunkLinesDto.Values[0].Capacity, 1));

                #endregion Set Well Allowable - Production

                #region Set Well Allowable - Injection
                var startDate_Inj = DTOExtensions.ToISO8601(DateTime.Today.ToUniversalTime().AddDays(-30));
                var endDate_Inj = DTOExtensions.ToISO8601(DateTime.Today.ToUniversalTime().AddDays(-20));

                //Create Snapshot
                snapshotInjection = WellAllocationService.CreateWellAllowableSnapshot("Snapshot2", WellAllowableSnapshotType.Sensitivity.ToString(), assets.ElementAt(0).Id.ToString(), startDate_Inj, endDate_Inj, WellTypeCategory.Injection.ToString(), ProductionType.Gas.ToString(), scenario.Id.ToString());

                // Getting well allowable Asset based on snapshot
                SWAAssetBasedRequestDTO sWAAssetBasedRequestDTO1 = new SWAAssetBasedRequestDTO() { AssetId = assets.ElementAt(0).Id, WellCategory = WellTypeCategory.Injection, SnapshotId = snapshotInjection.Id, FluidPhase = WellAllowablePhase.Gas };
                SWAAssetBasedDTO AssetBasedDTO_Injection = WellAllocationService.GetWellAllowableAssetBasedSnapShot(sWAAssetBasedRequestDTO1);
                Assert.IsNotNull(AssetBasedDTO_Injection);
                Assert.IsNotNull(AssetBasedDTO_Injection.SWAAssetSettingsDto);
                Assert.IsNotNull(AssetBasedDTO_Injection.SWAAssetZonesDto);
                Assert.IsNotNull(AssetBasedDTO_Injection.SWAAssetTrunkLinesDto);
                Assert.AreEqual("Oil", AssetBasedDTO.SWAAssetSettingsDto.Value.Phase.ToString(), "Phase type value is mismatched");

                AssetBasedDTO_Injection.SWAAssetSettingsDto.Value.SystemEfficiency = (decimal?)0.111;
                AssetBasedDTO_Injection.SWAAssetSettingsDto.Value.UnplannedFactor = (decimal?)0.222;
                AssetBasedDTO_Injection.SWAAssetSettingsDto.Value.FacilityAvailability = (decimal?)0.333;

                //Adding TargetExportCondition and TargetFieldCondition values
                for (int i = 0; i < AssetBasedDTO_Injection.SWAAssetZonesDto.Values.Length; i++)
                {
                    AssetBasedDTO_Injection.SWAAssetZonesDto.Values[i].TargetExportCondition = (decimal?)0.11;
                    AssetBasedDTO_Injection.SWAAssetZonesDto.Values[i].TargetFieldCondition = (decimal?)((0.11) / (0.111 * 0.222 * 0.333));
                }

                for (int i = 0; i < AssetBasedDTO_Injection.SWAAssetTrunkLinesDto.Values.Count(); i++)
                {
                    AssetBasedDTO_Injection.SWAAssetTrunkLinesDto.Values[i].Capacity = (decimal?)0.33;
                }

                //Saving values in db
                SWAAssetSettingsDTO swasettingsij = new SWAAssetSettingsDTO();
                swasettingsij = AssetBasedDTO_Injection.SWAAssetSettingsDto.Value;

                AssetBasedDTO_Injection.SaveAssetingParameter = swasettingsij;
                AssetBasedDTO_Injection.SWARequestDTO = sWAAssetBasedRequestDTO1;
                AssetBasedDTO_Injection.SavedAssetZoneDTOs = AssetBasedDTO_Injection.SWAAssetZonesDto.Values;
                AssetBasedDTO_Injection.SavedTrunkLineDTOs = AssetBasedDTO_Injection.SWAAssetTrunkLinesDto.Values;
                //Saving values in db
                WellAllocationService.SaveWellAllowableAssetBasedSnapShot(AssetBasedDTO_Injection);

                // Getting well allowable Asset based on snapshot
                AssetBasedDTO_Injection = WellAllocationService.GetWellAllowableAssetBasedSnapShot(sWAAssetBasedRequestDTO1);
                Assert.IsNotNull(AssetBasedDTO_Injection);
                Assert.IsNotNull(AssetBasedDTO_Injection.SWAAssetSettingsDto.Value.UnplannedFactor, "UnplannedFactor value is not updated");
                Assert.IsNotNull(AssetBasedDTO_Injection.SWAAssetSettingsDto.Value.FacilityAvailability, "FacilityAvailability value is not updated");
                Assert.IsNotNull(AssetBasedDTO_Injection.SWAAssetZonesDto.Values[0].TargetExportCondition, "TargetExportCondition value is not updated");
                Assert.IsNotNull(AssetBasedDTO_Injection.SWAAssetZonesDto.Values[0].TargetFieldCondition, "TargetFieldCondition value is not updated");
                Assert.IsNotNull(AssetBasedDTO_Injection.SWAAssetTrunkLinesDto.Values[0].Capacity, "Capacity value is not updated");
                Assert.IsNotNull(AssetBasedDTO_Injection.SWAAssetSettingsDto.Value.SystemEfficiency, "SystemEfficiency value is not updated");
                Assert.AreEqual(0.111, Math.Round((double)AssetBasedDTO_Injection.SWAAssetSettingsDto.Value.SystemEfficiency, 3));
                Assert.AreEqual(0.222, Math.Round((double)AssetBasedDTO_Injection.SWAAssetSettingsDto.Value.UnplannedFactor, 3));
                Assert.AreEqual(0.333, Math.Round((double)AssetBasedDTO_Injection.SWAAssetSettingsDto.Value.FacilityAvailability, 3));
                Assert.AreEqual(0.11, Math.Round((double)AssetBasedDTO_Injection.SWAAssetZonesDto.Values[0].TargetExportCondition, 2));
                Assert.AreEqual(13.41, Math.Round((double)AssetBasedDTO_Injection.SWAAssetZonesDto.Values[0].TargetFieldCondition, 2));
                Assert.AreEqual(0.33, Math.Round((double)AssetBasedDTO_Injection.SWAAssetTrunkLinesDto.Values[0].Capacity, 2));
                #endregion Injection

                #region Set Well Allowable - Production -Metric Unit System
                ChangeUnitSystem("Metric");
                ChangeUnitSystemUserSetting("Metric");


                SWAAssetBasedRequestDTO sWAAssetBasedRequestDTO2 = new SWAAssetBasedRequestDTO() { AssetId = assets.ElementAt(0).Id, WellCategory = WellTypeCategory.Production, SnapshotId = snapshotProduction.Id, FluidPhase = WellAllowablePhase.Oil };
                SWAAssetBasedDTO AssetBasedDTO_Metric = WellAllocationService.GetWellAllowableAssetBasedSnapShot(sWAAssetBasedRequestDTO2);

                Assert.AreEqual(Math.Round((double)UnitsConversion("STB/d", 0.11), 2), Math.Round(Convert.ToDouble(AssetBasedDTO_Metric.SWAAssetZonesDto.Values[0].TargetExportCondition.ToString()), 2), "Mismatch for Zone TargetExportCondition");
                Assert.AreEqual(Math.Round((double)UnitsConversion("STB/d", 12.2), 2), Math.Round(Convert.ToDouble(AssetBasedDTO_Metric.SWAAssetZonesDto.Values[0].TargetFieldCondition.ToString()), 2), "Mismatch forZone  TargetFieldCondition");
                Assert.AreEqual(Math.Round((double)UnitsConversion("STB/d", 0.33), 2), Math.Round(Convert.ToDouble(AssetBasedDTO_Metric.SWAAssetTrunkLinesDto.Values[0].Capacity.ToString()), 2), "Mismatch for  Trunkline TargetFieldCondition");
                #endregion Set Well Allowable - Production -Metric Unit System

                #region Set Well Allowable - Injection -Metric Unit System
                SWAAssetBasedRequestDTO sWAAssetBasedRequestDTO3 = new SWAAssetBasedRequestDTO() { AssetId = assets.ElementAt(0).Id, WellCategory = WellTypeCategory.Injection, SnapshotId = snapshotInjection.Id, FluidPhase = WellAllowablePhase.Gas };

                SWAAssetBasedDTO AssetBasedDTO_Injection_Metric = WellAllocationService.GetWellAllowableAssetBasedSnapShot(sWAAssetBasedRequestDTO3);
                Assert.AreEqual(Math.Round((double)UnitsConversion("Mscf/d", 0.11), 2), Math.Round(Convert.ToDouble(AssetBasedDTO_Injection_Metric.SWAAssetZonesDto.Values[0].TargetExportCondition.ToString()), 2), "Mismatch for TargetExportCondition");
                Assert.AreEqual(Math.Round((double)UnitsConversion("Mscf/d", 13.4051), 2), Math.Round(Convert.ToDouble(AssetBasedDTO_Injection_Metric.SWAAssetZonesDto.Values[0].TargetFieldCondition.ToString()), 2), "Mismatch for TargetFieldCondition");
                Assert.AreEqual(Math.Round((double)UnitsConversion("Mscf/d", 0.33), 2), Math.Round(Convert.ToDouble(AssetBasedDTO_Injection_Metric.SWAAssetTrunkLinesDto.Values[0].Capacity.ToString()), 2), "Mismatch for TargetFieldCondition");
                #endregion Set Well Allowable - Injection -Metric Unit System


            }
            finally
            {
                WellAllocationService.DeleteWellAllowableSnapshot(snapshotProduction.Id.ToString());
                WellAllocationService.DeleteWellAllowableSnapshot(snapshotInjection.Id.ToString());
                WellAllocationService.DeleteHierarchyByAssetId(assets.ElementAt(0).Id.ToString());
                Trace.WriteLine("Pattern1 pattern deleted successfully");
                Trace.WriteLine("Zone1 zone deleted successfully");
                Trace.WriteLine("Res1 reservoir deleted successfully");
                ChangeUnitSystem("US");
                ChangeUnitSystemUserSetting("US");
            }
        }

        /// <summary>
        ///FRWM- 6911 Set Well Allowables : calculate the technical rate For Well types (NF-DryGas,NF-Condensate,WI,GI)with or without well tests
        ///FRWM-7258 - Techincal Rate - UOM support
        ///FRWM-6389 - Added logs 
        /// <returns>
        [TestCategory(TestCategories.WellAllocationServiceTests), TestMethod]
        public void CalculateTechnicalRates_WI_WithWellTest()
        {
            ChangeUnitSystem("US");
            ChangeUnitSystemUserSetting("US");

            //Add asset
            #region  Add Asset
            List<AssetDTO> assets = CreateAsset(2);
            #endregion Add Asset
            SWASnapshotDTO snapshotInjection = new SWASnapshotDTO();
            try
            {

                #region Filter with Assets
                WellFilterDTO wellbyFilter = new WellFilterDTO();
                wellbyFilter.AssetIds = new long?[] { assets.ElementAt(0).Id };
                var ApplicableDate = DateTime.Today.AddDays(-1).ToLocalTime();
                #endregion Filter with Assets

                #region ModelFileName
                //ModelfileName
                var modelFileName = "WellfloWaterInjectionExample1.wflx";
                //WellTypeId wellType = WellTypeId.PLift;
                var options = new ModelFileOptionDTO
                {
                    CalibrationMethod = CalibrationMethodId.ReservoirPressure,
                    OptionalUpdate = new long[]
                      {
                        (long) OptionalUpdates.UpdateWCT_WGR,
                        (long) OptionalUpdates.UpdateGOR_CGR
                      }
                };
                #endregion ModelFileName

                #region Adding Water Injection Well
                //Create the wells in ForeSite and assign them to the asset created above
                WellDTO WI_Well = AddNonRRLWellGeneral("WI_Well", GetFacilityId("WI_Well", 1), WellTypeId.WInj, WellFluidType.BlackOil, WellFluidPhase.None, "1", assets.ElementAt(0).Id);
                AddNonRRLModelFile(WI_Well, modelFileName, options.CalibrationMethod, options.OptionalUpdate);
                #endregion Adding Nf Well

                AddWellSettingWithDoubleValues(WI_Well.Id, "Min L Factor Acceptance Limit", 0.1);
                AddWellSettingWithDoubleValues(WI_Well.Id, "Max L Factor Acceptance Limit", 2.0);

                UpdateSystemSettings(WI_Well, new string[] { SettingServiceStringConstants.WATER_GAS_RATIO_MIN_AL, SettingServiceStringConstants.WATER_GAS_RATIO_MAX_AL }, new double[] { 100, 1000 });

                #region Adding Valid WellTest
                WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(WI_Well.Id.ToString()).Units;
                WellTestDTO testData = new WellTestDTO
                {
                    WellId = WI_Well.Id,
                    SPTCodeDescription = "RepresentativeTest",
                    WellTestType = WellTestType.WellTest,
                    AverageTubingPressure = 2500,
                    AverageTubingTemperature = 100,
                    //Gas = 2000,
                    Water = 2500,
                    ChokeSize = 32,
                    FlowLinePressure = 600,
                    SeparatorPressure = 600,
                    TestDuration = 12,
                    SampleDate = DateTime.Today.AddDays(-5).ToUniversalTime()
                };

                WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testData));
                WellTestDTO[] allTunedAgainTests = WellTestDataService.GetAllValidWellTestByWellId(WI_Well.Id.ToString());

                WellTestDTO latestTestData_WI = WellTestDataService.GetLatestWellTestDataByWellId(WI_Well.Id.ToString());

                Assert.AreEqual("TUNING_SUCCEEDED", latestTestData_WI.Status.ToString(), "Well Test Status is not Success");
                #endregion Adding Valid WellTest

                #region create network scenario
                SNScenarioDTO scenario = ScenarioConfiguration(assets.ElementAt(0).Id);
                Assert.IsNotNull(scenario);
                #endregion create network scenario

                #region Add Reservoir

                ReservoirAndUnitsDTO addReservoir1 = AddReservoirToAsset("Res1", assets.ElementAt(0).Id);
                // GetReservoir IDs
                ReservoirArrayAndUnitsDTO getReservoirByAsset = WellAllocationService.GetReservoirsByAssetId(assets.ElementAt(0).Id.ToString());
                #endregion

                #region Add Zone
                ZoneAndUnitsDTO addZone1 = AddZoneToReservoir("Zone1", getReservoirByAsset.Values[0].Id);
                #endregion

                #region Get Zone IDs based on reservoir
                // Get Zone IDs based on reservoir
                ZoneArrayAndUnitsDTO getZoneByReservoir = WellAllocationService.GetZonesByReservoirId(getReservoirByAsset.Values[0].Id.ToString());
                #endregion

                #region AddWellToZone
                AddWellToZone(WI_Well.Id, getZoneByReservoir.Values[0].Id);
                #endregion

                #region Set Well Allowable - Production

                string startDateProduction = DTOExtensions.ToISO8601(DateTime.Now.AddDays(-30).ToUniversalTime());
                string endDateProduction = DTOExtensions.ToISO8601(DateTime.Now.AddDays(-20).ToUniversalTime());

                //Create Snapshot
                snapshotInjection = WellAllocationService.CreateWellAllowableSnapshot("Snapshot1", WellAllowableSnapshotType.Sensitivity.ToString(), assets.ElementAt(0).Id.ToString(), startDateProduction, endDateProduction, WellTypeCategory.Injection.ToString(), WellAllowablePhase.Water.ToString(), scenario.Id.ToString());
                Assert.IsNotNull(snapshotInjection);
                SWAAssetBasedRequestDTO sWAAssetBasedRequestDTO4 = new SWAAssetBasedRequestDTO() { AssetId = assets.ElementAt(0).Id, WellCategory = WellTypeCategory.Injection, SnapshotId = snapshotInjection.Id, FluidPhase = WellAllowablePhase.Water };

                SWAAssetBasedDTO AssetBasedDTO = WellAllocationService.GetWellAllowableAssetBasedSnapShot(sWAAssetBasedRequestDTO4);
                Assert.IsNotNull(AssetBasedDTO);

                //Getting Rate table as per Snapshot
                SWAWellBasedDTO wellbasedInjectionDTO = new SWAWellBasedDTO();
                wellbasedInjectionDTO.SnapshotId = snapshotInjection.Id;
                wellbasedInjectionDTO.AssetId = assets.ElementAt(0).Id;
                wellbasedInjectionDTO.WellCategory = WellTypeCategory.Injection;
                wellbasedInjectionDTO.FluidPhase = WellAllowablePhase.Water;

                SWARateArrayAndUnitsDTO ratesForAsset1Injection = WellAllocationService.GetWellAllowableRatesForSnapshot(wellbasedInjectionDTO);
                Assert.IsNotNull(ratesForAsset1Injection);

                ratesForAsset1Injection.Values[0].ReservoirPressureBound = 0;
                ratesForAsset1Injection.Values[0].ReservoirPressureTolerance = 0;
                ratesForAsset1Injection.Values[0].WellheadPressureBound = (decimal)3514.7;
                ratesForAsset1Injection.Values[0].PerforationPressureChangeBound = 1000;
                ratesForAsset1Injection.Values[0].MaximumRate = 3000;
                //ratesForAsset1Injection.Values[0].MinimumRate = 80;
                ratesForAsset1Injection.Values[0].WellAvailability = (decimal)0.95;
                ratesForAsset1Injection.Values[0].InAnalysis = true;

                //Update Rate table snap shot
                wellbasedInjectionDTO.Rates = ratesForAsset1Injection.Values;
                var updaterateAssetProduction = WellAllocationService.UpdateWellAllowableRatesForSnapshot(wellbasedInjectionDTO);
                Assert.IsNotNull(updaterateAssetProduction);

                #region Get Logs before Calculation
                //Get Logs before calculation
                string[] PreCalculationLogs = WellAllocationService.GetSWALogData(wellbasedInjectionDTO);
                Assert.AreEqual(0, PreCalculationLogs.Length, "Mimatched Logs Count");
                #endregion Get Logs before Calculation

                #region CalculatePreTechnicalandTechnicalRates
                var CalculateTechnicalRates = WellAllocationService.CalculateWellAllowableTechnicalRate(wellbasedInjectionDTO);
                Assert.IsNotNull(CalculateTechnicalRates, "Added Calculated Pre technical rates and Technical rates record failed");
                #endregion CalculatePreTechnicalandTechnicalRates

                #region Get Logs after Calculation
                //Get Logs after calculation
                string[] PostCalculationLogs = WellAllocationService.GetSWALogData(wellbasedInjectionDTO);
                Assert.AreNotSame(0, PostCalculationLogs.Length, "Mimatched Logs Count");
                #endregion Get Logs after Calculation

                //Verification of updated record
                SWAWellBasedDTO wellbasedInjectionDTO_Updated = new SWAWellBasedDTO();
                wellbasedInjectionDTO_Updated.SnapshotId = snapshotInjection.Id;
                wellbasedInjectionDTO_Updated.AssetId = assets.ElementAt(0).Id;
                wellbasedInjectionDTO_Updated.WellCategory = WellTypeCategory.Injection;
                wellbasedInjectionDTO_Updated.FluidPhase = WellAllowablePhase.Water;

                SWARateArrayAndUnitsDTO ratesForAsset1Injection_Updated = WellAllocationService.GetWellAllowableRatesForSnapshot(wellbasedInjectionDTO_Updated);
                Assert.IsNotNull(ratesForAsset1Injection_Updated);

                Assert.AreEqual((decimal)8172.4, ratesForAsset1Injection_Updated.Values[0].ReservoirPressureBound, "Mismatch for ReservoirPressureBound");
                Assert.AreEqual(0, ratesForAsset1Injection_Updated.Values[0].ReservoirPressureTolerance, "Mismatch for ReservoirPressureTolerance");
                Assert.AreEqual(1000, ratesForAsset1Injection_Updated.Values[0].PerforationPressureChangeBound, "Mismatch for PerforationPressureChangeBound");
                Assert.AreEqual((decimal)3514.7, ratesForAsset1Injection_Updated.Values[0].WellheadPressureBound, "Mismatch for WellheadPressureBound");
                Assert.AreEqual((decimal)0.95, ratesForAsset1Injection_Updated.Values[0].WellAvailability, "Mismatch in Well Availability");
                Assert.AreEqual(3000, ratesForAsset1Injection_Updated.Values[0].MaximumRate, "Mismatch for Maximum Rate");

                Assert.AreEqual(10716.7, Math.Round(Convert.ToDouble(ratesForAsset1Injection_Updated.Values[0].ResevoirBoundRate.ToString()), 1), 0.005, "Mismatch in  ResevoirBoundRate");
                Assert.AreEqual(5864.3, Math.Round(Convert.ToDouble(ratesForAsset1Injection_Updated.Values[0].PerforationPressureChangeRate.ToString()), 1), 0.005, "Mismatch inPerforationPressureChangeRate");
                Assert.AreEqual(7691.5, Math.Round(Convert.ToDouble(ratesForAsset1Injection_Updated.Values[0].WellheadBoundRate.ToString()), 1), 0.005, "Mismatch in Maximum Wellhead Pressure Rate");
                Assert.AreEqual(2850, ratesForAsset1Injection_Updated.Values[0].WellAvailableRate, "Mismatch in Well Available Rate");

                Assert.AreEqual(false, ratesForAsset1Injection_Updated.Values[0].HasError, "Error is Detected");
                Assert.AreEqual(true, ratesForAsset1Injection_Updated.Values[0].InAnalysis, "Mismatch in IsAnalysis Status");
                #endregion Set Well Allowable - Injection

                ChangeUnitSystem("Metric");
                ChangeUnitSystemUserSetting("Metric");

                #region Metric Conversion
                double fct_Pressure = 6.894757;
                double fct_FlowRate = 0.1589873;
                double tol = 0.2;

                SWARateArrayAndUnitsDTO ratesForAsset1Injection_Metric = WellAllocationService.GetWellAllowableRatesForSnapshot(wellbasedInjectionDTO_Updated);
                Assert.IsNotNull(ratesForAsset1Injection_Metric);

                //Assert.AreEqual(GetTruncatedValueforDouble((double)(0.0 * fct_Pressure), 2), (double)ratesForAsset1Injection_Metric.Values[0].ReservoirPressureBound, tol, "Mismatch for ReservoirPressureBound");
                //Assert.AreEqual(GetTruncatedValueforDouble((double)(0.0 * fct_Pressure), 2), (double)ratesForAsset1Injection_Metric.Values[0].ReservoirPressureTolerance, tol, "Mismatch for ReservoirPressureTolerance");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(1000.0 * fct_Pressure), 2), (double)ratesForAsset1Injection_Metric.Values[0].PerforationPressureChangeBound, tol, "Mismatch for PerforationPressureChangeBound");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(3514.7 * fct_Pressure), 2), (double)ratesForAsset1Injection_Metric.Values[0].WellheadPressureBound, tol, "Mismatch for WellheadPressureBound");
                Assert.AreEqual((decimal)0.95, ratesForAsset1Injection_Metric.Values[0].WellAvailability, "Mismatch in Well Availability");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(3000.0 * fct_FlowRate), 2), (double)ratesForAsset1Injection_Metric.Values[0].MaximumRate, tol, "Mismatch for Maximum Rate");

                Assert.AreEqual(GetTruncatedValueforDouble((double)(10716.74 * fct_FlowRate), 2), (double)ratesForAsset1Injection_Metric.Values[0].ResevoirBoundRate, tol, "Mismatch in  ResevoirBoundRate");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(5864.319 * fct_FlowRate), 2), (double)ratesForAsset1Injection_Metric.Values[0].PerforationPressureChangeRate, tol, "Mismatch inPerforationPressureChangeRate");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(7691.54 * fct_FlowRate), 2), (double)ratesForAsset1Injection_Metric.Values[0].WellheadBoundRate, tol, "Mismatch in Maximum Wellhead Pressure Rate");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(2850.0 * fct_FlowRate), 2), (double)ratesForAsset1Injection_Metric.Values[0].WellAvailableRate, tol, "Mismatch in Well Available Rate");

                Assert.AreEqual(false, ratesForAsset1Injection_Metric.Values[0].HasError, "Error is Detected");
                Assert.AreEqual(true, ratesForAsset1Injection_Metric.Values[0].InAnalysis, "Mismatch in IsAnalysis Status");
                #endregion Metric Conversion
            }
            finally
            {
                ChangeUnitSystem("US");
                ChangeUnitSystemUserSetting("US");
                WellAllocationService.DeleteWellAllowableSnapshot(snapshotInjection.Id.ToString());
                WellAllocationService.DeleteHierarchyByAssetId(assets.ElementAt(0).Id.ToString());
                WellAllocationService.DeleteHierarchyByAssetId(assets.ElementAt(1).Id.ToString());
            }
        }

        [TestCategory(TestCategories.WellAllocationServiceTests), TestMethod]
        public void CalculateTechnicalRates_NFWellDryGas_WithWellTest()
        {
            ChangeUnitSystem("US");
            ChangeUnitSystemUserSetting("US");

            //Add asset
            #region  Add Asset
            List<AssetDTO> assets = CreateAsset(2);
            #endregion Add Asset
            SWASnapshotDTO snapshotProduction = new SWASnapshotDTO();
            try
            {
                #region Filter with Assets
                WellFilterDTO wellbyFilter = new WellFilterDTO();
                wellbyFilter.AssetIds = new long?[] { assets.ElementAt(0).Id };
                var ApplicableDate = DateTime.Today.AddDays(-1).ToLocalTime();
                #endregion Filter with Assets

                #region ModelFileName
                //ModelfileName
                var modelFileName = "Dry Gas - IPR Auto Tuning.wflx";
                //WellTypeId wellType = WellTypeId.PLift;
                var options = new ModelFileOptionDTO
                {
                    CalibrationMethod = CalibrationMethodId.ReservoirPressure,
                    OptionalUpdate = new long[]
                      {
                        (long) OptionalUpdates.UpdateWCT_WGR,
                        //(long) OptionalUpdates.UpdateGOR_CGR
                      }
                };
                #endregion ModelFileName

                #region Adding Nf Well
                //Create the wells in ForeSite and assign them to the asset created above
                WellDTO NFWWell_1 = AddNonRRLWellGeneral("NFWell_1", GetFacilityId("NFWELL_", 1), WellTypeId.NF, WellFluidType.DryGas, WellFluidPhase.None, "1", assets.ElementAt(0).Id);
                AddNonRRLModelFile(NFWWell_1, modelFileName, options.CalibrationMethod, options.OptionalUpdate);
                #endregion Adding Nf Well

                AddWellSettingWithDoubleValues(NFWWell_1.Id, "Min L Factor Acceptance Limit", 0.1);
                AddWellSettingWithDoubleValues(NFWWell_1.Id, "Max L Factor Acceptance Limit", 2.0);

                UpdateSystemSettings(NFWWell_1, new string[] { SettingServiceStringConstants.WATER_GAS_RATIO_MIN_AL, SettingServiceStringConstants.WATER_GAS_RATIO_MAX_AL }, new double[] { 100, 1000 });

                #region Adding Valid WellTest
                WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(NFWWell_1.Id.ToString()).Units;
                WellTestDTO testData = new WellTestDTO
                {
                    WellId = NFWWell_1.Id,
                    SPTCodeDescription = "RepresentativeTest",
                    //CalibrationMethod = options.CalibrationMethod,
                    WellTestType = WellTestType.WellTest,
                    AverageTubingPressure = 1000,
                    AverageTubingTemperature = 100,
                    Gas = 2000,
                    Water = (decimal)555.1,
                    ChokeSize = 32,
                    FlowLinePressure = 600,
                    SeparatorPressure = 600,
                    TestDuration = 12,
                    SampleDate = DateTime.Today.AddDays(-5).ToUniversalTime()
                };

                WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testData));
                WellTestDTO[] allTunedAgainTests = WellTestDataService.GetAllValidWellTestByWellId(NFWWell_1.Id.ToString());

                WellTestDTO latestTestData_NF = WellTestDataService.GetLatestWellTestDataByWellId(NFWWell_1.Id.ToString());

                Assert.AreEqual("TUNING_SUCCEEDED", latestTestData_NF.Status.ToString(), "Well Test Status is not Success");
                #endregion Adding Valid WellTest

                #region create network scenario
                SNScenarioDTO scenario = ScenarioConfiguration(assets.ElementAt(0).Id);
                Assert.IsNotNull(scenario);
                #endregion create network scenario

                #region Add Reservoir

                ReservoirAndUnitsDTO addReservoir1 = AddReservoirToAsset("Res1", assets.ElementAt(0).Id);
                // GetReservoir IDs
                ReservoirArrayAndUnitsDTO getReservoirByAsset = WellAllocationService.GetReservoirsByAssetId(assets.ElementAt(0).Id.ToString());
                #endregion

                #region Add Zone
                ZoneAndUnitsDTO addZone1 = AddZoneToReservoir("Zone1", getReservoirByAsset.Values[0].Id);
                #endregion

                #region Get Zone IDs based on reservoir
                // Get Zone IDs based on reservoir
                ZoneArrayAndUnitsDTO getZoneByReservoir = WellAllocationService.GetZonesByReservoirId(getReservoirByAsset.Values[0].Id.ToString());
                #endregion

                #region AddWellToZone
                AddWellToZone(NFWWell_1.Id, getZoneByReservoir.Values[0].Id);
                #endregion AddWellToZone

                #region Set Well Allowable - Production
                string startDateProduction = DTOExtensions.ToISO8601(DateTime.Now.AddDays(-30).ToUniversalTime());
                string endDateProduction = DTOExtensions.ToISO8601(DateTime.Now.AddDays(-20).ToUniversalTime());

                //Create Snapshot
                snapshotProduction = WellAllocationService.CreateWellAllowableSnapshot("Snapshot1", WellAllowableSnapshotType.Sensitivity.ToString(), assets.ElementAt(0).Id.ToString(), startDateProduction, endDateProduction, WellTypeCategory.Production.ToString(), WellAllowablePhase.Gas.ToString(), scenario.Id.ToString());
                Assert.IsNotNull(snapshotProduction);
                SWAAssetBasedRequestDTO sWAAssetBasedRequestDTO5 = new SWAAssetBasedRequestDTO() { AssetId = assets.ElementAt(0).Id, WellCategory = WellTypeCategory.Production, SnapshotId = snapshotProduction.Id, FluidPhase = WellAllowablePhase.Gas };

                SWAAssetBasedDTO AssetBasedDTO = WellAllocationService.GetWellAllowableAssetBasedSnapShot(sWAAssetBasedRequestDTO5);
                //Assert.IsNotNull(AssetBasedDTO);

                //Getting Rate table as per Snapshot
                SWAWellBasedDTO wellbasedProductionDTO = new SWAWellBasedDTO();
                wellbasedProductionDTO.SnapshotId = snapshotProduction.Id;
                wellbasedProductionDTO.AssetId = assets.ElementAt(0).Id;
                wellbasedProductionDTO.WellCategory = WellTypeCategory.Production;
                wellbasedProductionDTO.FluidPhase = WellAllowablePhase.Gas;

                SWARateArrayAndUnitsDTO ratesForAsset1Production = WellAllocationService.GetWellAllowableRatesForSnapshot(wellbasedProductionDTO);
                Assert.IsNotNull(ratesForAsset1Production);

                ratesForAsset1Production.Values[0].ReservoirPressureBound = 0;
                ratesForAsset1Production.Values[0].ReservoirPressureTolerance = 0;
                ratesForAsset1Production.Values[0].WellheadPressureBound = 50;
                ratesForAsset1Production.Values[0].PerforationPressureChangeBound = (decimal)1030.12;
                ratesForAsset1Production.Values[0].MaximumRate = 3000;
                ratesForAsset1Production.Values[0].WellAvailability = (decimal)0.86;
                ratesForAsset1Production.Values[0].InAnalysis = true;

                //Update Rate table snap shot
                wellbasedProductionDTO.Rates = ratesForAsset1Production.Values;
                var updaterateAssetProduction = WellAllocationService.UpdateWellAllowableRatesForSnapshot(wellbasedProductionDTO);
                Assert.IsNotNull(updaterateAssetProduction);
                #region Get Logs before Calculation
                //Get Logs before calculation
                string[] PreCalculationLogs = WellAllocationService.GetSWALogData(wellbasedProductionDTO);
                Assert.AreEqual(0, PreCalculationLogs.Length, "Mimatched Logs Count");
                #endregion Get Logs before Calculation

                #region CalculatePreTechnicalandTechnicalRates
                var CalculateTechnicalRates = WellAllocationService.CalculateWellAllowableTechnicalRate(wellbasedProductionDTO);
                Assert.IsNotNull(CalculateTechnicalRates, "Added Calculated Pre technical rates and Technical rates record failed");
                #endregion CalculatePreTechnicalandTechnicalRates

                #region Get Logs after Calculation
                //Get Logs after calculation
                string[] PostCalculationLogs = WellAllocationService.GetSWALogData(wellbasedProductionDTO);
                Assert.AreNotSame(0, PostCalculationLogs.Length, "Mimatched Logs Count");
                #endregion Get Logs after Calculation

                //Verification of updated record
                SWAWellBasedDTO wellbasedProductionDTO_Updated = new SWAWellBasedDTO();
                wellbasedProductionDTO_Updated.SnapshotId = snapshotProduction.Id;
                wellbasedProductionDTO_Updated.AssetId = assets.ElementAt(0).Id;
                wellbasedProductionDTO_Updated.WellCategory = WellTypeCategory.Production;
                wellbasedProductionDTO_Updated.FluidPhase = WellAllowablePhase.Gas;

                SWARateArrayAndUnitsDTO ratesForAsset1Production_Updated = WellAllocationService.GetWellAllowableRatesForSnapshot(wellbasedProductionDTO_Updated);
                Assert.IsNotNull(ratesForAsset1Production_Updated);

                Assert.AreEqual(0, ratesForAsset1Production_Updated.Values[0].ReservoirPressureBound, "Mismatch for ReservoirPressureBound");
                Assert.AreEqual(0, ratesForAsset1Production_Updated.Values[0].ReservoirPressureTolerance, "Mismatch for ReservoirPressureTolerance");
                Assert.AreEqual((decimal)1030.12, ratesForAsset1Production_Updated.Values[0].PerforationPressureChangeBound, "Mismatch for PerforationPressureChangeBound");
                Assert.AreEqual(50, ratesForAsset1Production_Updated.Values[0].WellheadPressureBound, "Mismatch for WellheadPressureBound");
                Assert.AreEqual((decimal)0.86, ratesForAsset1Production_Updated.Values[0].WellAvailability, "Mismatch in WellAvailability");
                Assert.AreEqual(3000, ratesForAsset1Production_Updated.Values[0].MaximumRate, "Mismatch for Maximum Rate");
                //Manually generated/calculated using WelFlo [There can be approximation and variation due to this manual interpolation]
                Assert.IsNull(ratesForAsset1Production_Updated.Values[0].ResevoirBoundRate, "Mismatch in  ResevoirBoundRate");
                Assert.AreEqual(1196.05, Math.Round(Convert.ToDouble(ratesForAsset1Production_Updated.Values[0].PerforationPressureChangeRate.ToString()), 1), 0.05, "PerforationPressureChangeRate");
                Assert.AreEqual(2315.59, Math.Round(Convert.ToDouble(ratesForAsset1Production_Updated.Values[0].WellheadBoundRate.ToString()), 1), 0.05, "Mismatch in WellheadBoundRate");
                Assert.AreEqual(1028.61, (double)ratesForAsset1Production_Updated.Values[0].WellAvailableRate, 0.05, "Mismatch in Well Available Rate");

                Assert.AreEqual(false, ratesForAsset1Production_Updated.Values[0].HasError, "Error is Detected");
                Assert.AreEqual(true, ratesForAsset1Production_Updated.Values[0].InAnalysis, "Mismatch in IsAnalysis Status");
                #endregion Set Well Allowable - Production

                ChangeUnitSystem("Metric");
                ChangeUnitSystemUserSetting("Metric");

                #region Metric Conversion
                double fct_Pressure = 6.894757;
                double fct_FlowRate = 28.3168466;
                double tol = 0.15;

                SWARateArrayAndUnitsDTO ratesForAsset1Production_Metric = WellAllocationService.GetWellAllowableRatesForSnapshot(wellbasedProductionDTO_Updated);
                Assert.IsNotNull(ratesForAsset1Production_Metric);

                Assert.AreEqual(0, (double)ratesForAsset1Production_Metric.Values[0].ReservoirPressureBound, tol, "Mismatch for ReservoirPressureBound");
                Assert.AreEqual(0, (double)ratesForAsset1Production_Metric.Values[0].ReservoirPressureTolerance, tol, "Mismatch for ReservoirPressureTolerance");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(1030.12 * fct_Pressure), 1), (double)ratesForAsset1Production_Metric.Values[0].PerforationPressureChangeBound, tol, "Mismatch for PerforationPressureChangeBound");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(50 * fct_Pressure), 2), (double)ratesForAsset1Production_Metric.Values[0].WellheadPressureBound, tol, "Mismatch for WellheadPressureBound");
                Assert.AreEqual((decimal)0.86, ratesForAsset1Production_Metric.Values[0].WellAvailability, "Mismatch in Well Availability");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(3000.0 * fct_FlowRate), 2), (double)ratesForAsset1Production_Metric.Values[0].MaximumRate, tol, "Mismatch for Maximum Rate");

                Assert.IsNull(ratesForAsset1Production_Metric.Values[0].ResevoirBoundRate, "Mismatch in  ResevoirBoundRate");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(1196.05 * fct_FlowRate), 2), (double)ratesForAsset1Production_Metric.Values[0].PerforationPressureChangeRate, tol, "Mismatch inPerforationPressureChangeRate");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(2315.59 * fct_FlowRate), 2), (double)ratesForAsset1Production_Metric.Values[0].WellheadBoundRate, tol, "Mismatch in Maximum Wellhead Pressure Rate");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(1028.61 * fct_FlowRate), 2), (double)ratesForAsset1Production_Metric.Values[0].WellAvailableRate, tol, "Mismatch in Well Available Rate");

                Assert.AreEqual(false, ratesForAsset1Production_Metric.Values[0].HasError, "Error is Detected");
                Assert.AreEqual(true, ratesForAsset1Production_Metric.Values[0].InAnalysis, "Mismatch in IsAnalysis Status");
                #endregion Metric Conversion
            }
            finally
            {
                ChangeUnitSystem("US");
                ChangeUnitSystemUserSetting("US");
                WellAllocationService.DeleteWellAllowableSnapshot(snapshotProduction.Id.ToString());
                WellAllocationService.DeleteHierarchyByAssetId(assets.ElementAt(0).Id.ToString());
                WellAllocationService.DeleteHierarchyByAssetId(assets.ElementAt(1).Id.ToString());
            }
        }

        [TestCategory(TestCategories.WellAllocationServiceTests), TestMethod]
        public void CalculateTechnicalRates_NFCondensate_WellTest()
        {
            ChangeUnitSystem("US");
            ChangeUnitSystemUserSetting("US");
        
            //Add asset
            #region  Add Asset
            List<AssetDTO> assets = CreateAsset(1);
            #endregion Add Asset
            SWASnapshotDTO snapshotProduction = new SWASnapshotDTO();
            try
            {
                #region Filter with Assets
                WellFilterDTO wellbyFilter = new WellFilterDTO();
                wellbyFilter.AssetIds = new long?[] { assets.ElementAt(0).Id };
                var ApplicableDate = DateTime.Today.AddDays(-1).ToLocalTime();
                #endregion Filter with Assets

                #region ModelFileName
                //ModelfileName
                var modelFileName = "Condensate Gas - IPR Auto Tuning.wflx";

                var options = new ModelFileOptionDTO
                {
                    CalibrationMethod = CalibrationMethodId.ReservoirPressure,
                    OptionalUpdate = new long[]
                      {
                        (long) OptionalUpdates.UpdateWCT_WGR,
                        (long) OptionalUpdates.UpdateGOR_CGR
                      }
                };
                #endregion ModelFileName

                #region Adding Nf Well
                //Create the wells in ForeSite and assign them to the asset created above
                WellDTO NFWWell_CO = AddNonRRLWellGeneral("NFWell_1", GetFacilityId("NFWELL_", 1), WellTypeId.NF, WellFluidType.Condensate, WellFluidPhase.None, "1", assets.ElementAt(0).Id);
                AddNonRRLModelFile(NFWWell_CO, modelFileName, options.CalibrationMethod, options.OptionalUpdate);
                #endregion Adding Nf Well

                AddWellSettingWithDoubleValues(NFWWell_CO.Id, "Min L Factor Acceptance Limit", 0.1);
                AddWellSettingWithDoubleValues(NFWWell_CO.Id, "Max L Factor Acceptance Limit", 2.0);

                UpdateSystemSettings(NFWWell_CO, new string[] { SettingServiceStringConstants.CONDENSATE_GAS_RATIO_MAX_AL, SettingServiceStringConstants.CONDENSATE_GAS_RATIO_MIN_AL }, new double[] { 600, 0 });

                #region Adding Valid WellTest
                WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(NFWWell_CO.Id.ToString()).Units;
                WellTestDTO testData = new WellTestDTO
                {
                    WellId = NFWWell_CO.Id,
                    SPTCodeDescription = "AllocatableTest",
                    CalibrationMethod = options.CalibrationMethod,
                    WellTestType = WellTestType.WellTest,
                    AverageTubingPressure = 1500,
                    AverageTubingTemperature = 100,
                    Gas = 90000,
                    Water = 12622.8m,
                    Oil = 12622.8m,
                    //CGR = 12622.8m,
                    ChokeSize = 32,
                    FlowLinePressure = 4000,
                    SeparatorPressure = 4000,
                    TestDuration = 24,
                    SampleDate = DateTime.Today.AddDays(-5).ToUniversalTime()
                };

                WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testData));
                WellTestDTO[] allTunedAgainTests = WellTestDataService.GetAllValidWellTestByWellId(NFWWell_CO.Id.ToString());

                WellTestDTO latestTestData_NF = WellTestDataService.GetLatestWellTestDataByWellId(NFWWell_CO.Id.ToString());

                Assert.AreEqual("TUNING_SUCCEEDED", latestTestData_NF.Status.ToString(), "Well Test Status is not Success");

                #endregion Adding Valid WellTest

                #region create network scenario
                SNScenarioDTO scenario = ScenarioConfiguration(assets.ElementAt(0).Id);
                Assert.IsNotNull(scenario);
                #endregion create network scenario

                #region Add Reservoir

                ReservoirAndUnitsDTO addReservoir1 = AddReservoirToAsset("Res1", assets.ElementAt(0).Id);
                // GetReservoir IDs
                ReservoirArrayAndUnitsDTO getReservoirByAsset = WellAllocationService.GetReservoirsByAssetId(assets.ElementAt(0).Id.ToString());
                #endregion

                #region Add Zone
                ZoneAndUnitsDTO addZone1 = AddZoneToReservoir("Zone1", getReservoirByAsset.Values[0].Id);
                #endregion

                #region Get Zone IDs based on reservoir
                // Get Zone IDs based on reservoir
                ZoneArrayAndUnitsDTO getZoneByReservoir = WellAllocationService.GetZonesByReservoirId(getReservoirByAsset.Values[0].Id.ToString());
                #endregion

                #region AddWellToZone
                AddWellToZone(NFWWell_CO.Id, getZoneByReservoir.Values[0].Id);
                #endregion

                #region Set Well Allowable - Production
                string startDateProduction = DTOExtensions.ToISO8601(DateTime.Now.AddDays(-30).ToUniversalTime());
                string endDateProduction = DTOExtensions.ToISO8601(DateTime.Now.AddDays(-20).ToUniversalTime());

                //Create Snapshot
                snapshotProduction = WellAllocationService.CreateWellAllowableSnapshot("Snapshot1", WellAllowableSnapshotType.Sensitivity.ToString(), assets.ElementAt(0).Id.ToString(), startDateProduction, endDateProduction, WellTypeCategory.Production.ToString(), WellAllowablePhase.Gas.ToString(), scenario.Id.ToString());
                Assert.IsNotNull(snapshotProduction);
                SWAAssetBasedRequestDTO sWAAssetBasedRequestDTO6 = new SWAAssetBasedRequestDTO() { AssetId = assets.ElementAt(0).Id, WellCategory = WellTypeCategory.Production, SnapshotId = snapshotProduction.Id, FluidPhase = WellAllowablePhase.Gas };

                SWAAssetBasedDTO AssetBasedDTO = WellAllocationService.GetWellAllowableAssetBasedSnapShot(sWAAssetBasedRequestDTO6);
                Assert.IsNotNull(AssetBasedDTO);

                //Getting Rate table as per Snapshot
                SWAWellBasedDTO wellbasedProductionDTO = new SWAWellBasedDTO();
                wellbasedProductionDTO.SnapshotId = snapshotProduction.Id;
                wellbasedProductionDTO.AssetId = assets.ElementAt(0).Id;
                wellbasedProductionDTO.WellCategory = WellTypeCategory.Production;
                wellbasedProductionDTO.FluidPhase = WellAllowablePhase.Gas;

                SWARateArrayAndUnitsDTO ratesForAsset1Production = WellAllocationService.GetWellAllowableRatesForSnapshot(wellbasedProductionDTO);
                Assert.IsNotNull(ratesForAsset1Production);

                ratesForAsset1Production.Values[0].ReservoirPressureBound = (decimal)5279.87;
                ratesForAsset1Production.Values[0].ReservoirPressureTolerance = (decimal)517.12;
                ratesForAsset1Production.Values[0].WellheadPressureBound = 50;
                ratesForAsset1Production.Values[0].PerforationPressureChangeBound = (decimal)993.84;
                ratesForAsset1Production.Values[0].MaximumRate = 3000;
                ratesForAsset1Production.Values[0].MinimumRate = 0;
                ratesForAsset1Production.Values[0].WellAvailability = (decimal)0.86;
                ratesForAsset1Production.Values[0].InAnalysis = true;

                //Update Rate table snap shot
                wellbasedProductionDTO.Rates = ratesForAsset1Production.Values;
                var updaterateAssetProduction = WellAllocationService.UpdateWellAllowableRatesForSnapshot(wellbasedProductionDTO);
                Assert.IsNotNull(updaterateAssetProduction);

                #region asset bound
                SWAAssetBasedRequestDTO sWAAssetBasedRequestDTO = new SWAAssetBasedRequestDTO() { AssetId = assets.ElementAt(0).Id, WellCategory = WellTypeCategory.Production, SnapshotId = snapshotProduction.Id, FluidPhase = WellAllowablePhase.Gas };

                // Getting well allowable Asset based on snapshot
                SWAAssetBasedDTO AssetBasedDTO_1 = WellAllocationService.GetWellAllowableAssetBasedSnapShot(sWAAssetBasedRequestDTO);
                AssetBasedDTO_1.SWAAssetSettingsDto.Value.SystemEfficiency = 1;
                AssetBasedDTO_1.SWAAssetSettingsDto.Value.UnplannedFactor = 1;
                AssetBasedDTO_1.SWAAssetSettingsDto.Value.FacilityAvailability = 1;

                //Adding TargetExportCondition,TargetFieldCondition & Capacity values
                for (int i = 0; i < AssetBasedDTO_1.SWAAssetZonesDto.Values.Length; i++)
                {
                    AssetBasedDTO_1.SWAAssetZonesDto.Values[i].TargetExportCondition = 1000;
                    //AssetBasedDTO.SWAAssetZonesDto.Values[i].TargetFieldCondition = (decimal?) 3500 + (i*10);
                }

                for (int i = 0; i < AssetBasedDTO_1.SWAAssetTrunkLinesDto.Values.Length; i++)
                {
                    AssetBasedDTO_1.SWAAssetTrunkLinesDto.Values[i].Capacity = 6000;
                }
                SWAAssetSettingsDTO swasettings = new SWAAssetSettingsDTO();
                swasettings = AssetBasedDTO_1.SWAAssetSettingsDto.Value;

                AssetBasedDTO_1.SaveAssetingParameter = swasettings;
                AssetBasedDTO_1.SWARequestDTO = sWAAssetBasedRequestDTO;
                AssetBasedDTO_1.SavedAssetZoneDTOs = AssetBasedDTO_1.SWAAssetZonesDto.Values;
                AssetBasedDTO_1.SavedTrunkLineDTOs = AssetBasedDTO_1.SWAAssetTrunkLinesDto.Values;
                //Saving values in db
                Assert.IsNotNull(AssetBasedDTO_1.SaveAssetingParameter);
                WellAllocationService.SaveWellAllowableAssetBasedSnapShot(AssetBasedDTO_1);
                #endregion asset bound

                #region Get Logs before Calculation
                //Get Logs before calculation
                string[] PreCalculationLogs = WellAllocationService.GetSWALogData(wellbasedProductionDTO);
                Assert.AreEqual(0, PreCalculationLogs.Length, "Mimatched Logs Count");
                #endregion Get Logs before Calculation

                #region CalculatePreTechnicalandTechnicalRates
                var CalculateTechnicalRates = WellAllocationService.CalculateWellAllowableTechnicalRate(wellbasedProductionDTO);
                Assert.IsNotNull(CalculateTechnicalRates, "Added Calculated Pre technical rates and Technical rates record failed");
                #endregion CalculatePreTechnicalandTechnicalRates

                #region Get Logs after Calculation
                //Get Logs after calculation
                string[] PostCalculationLogs = WellAllocationService.GetSWALogData(wellbasedProductionDTO);
                Assert.AreNotSame(0, PostCalculationLogs.Length, "Mimatched Logs Count");
                #endregion Get Logs after Calculation

                #region CalculatePreTechnicalandTechnicalRates
                var CalculateTechnicalRates_pr = WellAllocationService.CalculateWellAllowableTechnicalRate(wellbasedProductionDTO);
                Assert.IsNotNull(CalculateTechnicalRates_pr, "Added Calculated Pre technical rates and Technical rates record failed");
                #endregion CalculatePreTechnicalandTechnicalRates

                //Verification of updated record
                SWAWellBasedDTO wellbasedProductionDTO_Updated = new SWAWellBasedDTO();
                wellbasedProductionDTO_Updated.SnapshotId = snapshotProduction.Id;
                wellbasedProductionDTO_Updated.AssetId = assets.ElementAt(0).Id;
                wellbasedProductionDTO_Updated.WellCategory = WellTypeCategory.Production;
                wellbasedProductionDTO_Updated.FluidPhase = WellAllowablePhase.Gas;

                SWARateArrayAndUnitsDTO ratesForAsset1Production_Updated = WellAllocationService.GetWellAllowableRatesForSnapshot(wellbasedProductionDTO_Updated);
                Assert.IsNotNull(ratesForAsset1Production_Updated);

                Assert.AreEqual((decimal)5279.87, ratesForAsset1Production_Updated.Values[0].ReservoirPressureBound, "Mismatch for ReservoirPressureBound");
                Assert.AreEqual((decimal)517.12, ratesForAsset1Production_Updated.Values[0].ReservoirPressureTolerance, "Mismatch for ReservoirPressureTolerance");
                Assert.AreEqual((decimal)993.84, ratesForAsset1Production_Updated.Values[0].PerforationPressureChangeBound, "PerforationPressureChangeBound");
                Assert.AreEqual(50, ratesForAsset1Production_Updated.Values[0].WellheadPressureBound, "Mismatch for WellheadPressureBound");
                Assert.AreEqual((decimal)0.86, ratesForAsset1Production_Updated.Values[0].WellAvailability, "Mismatch in WellAvailability");
                Assert.AreEqual(3000, ratesForAsset1Production_Updated.Values[0].MaximumRate, "Mismatch for Maximum Rate");

                //Manually generated/calculated using WelFlo [There can be approximation and variation due to this manual interpolation]
                Assert.AreEqual(65032.63, Math.Round(Convert.ToDouble(ratesForAsset1Production_Updated.Values[0].ResevoirBoundRate.ToString()), 2), 0.005, "Mismatch in  ResevoirBoundRate");
                Assert.AreEqual(115315.41, Math.Round(Convert.ToDouble(ratesForAsset1Production_Updated.Values[0].PerforationPressureChangeRate.ToString()), 2), 0.05, "PerforationPressureChangeRate");
                Assert.AreEqual(96552.89, Math.Round(Convert.ToDouble(ratesForAsset1Production_Updated.Values[0].WellheadBoundRate.ToString()), 1), 0.05, "Mismatch in WellheadBoundRate");
                Assert.AreEqual(2580, ratesForAsset1Production_Updated.Values[0].WellAvailableRate, "Mismatch in Well Available Rate");

                Assert.AreEqual(false, ratesForAsset1Production_Updated.Values[0].HasError, "Error is Detected");
                Assert.AreEqual(true, ratesForAsset1Production_Updated.Values[0].InAnalysis, "Mismatch in IsAnalysis Status");
                #endregion Set Well Allowable - Production

                ChangeUnitSystem("Metric");
                ChangeUnitSystemUserSetting("Metric");

                #region Metric Conversion
                double fct_Pressure = 6.894757;
                double fct_FlowRate = 28.3168466;
                double tol = 0.15;

                SWARateArrayAndUnitsDTO ratesForAsset1Production_Metric = WellAllocationService.GetWellAllowableRatesForSnapshot(wellbasedProductionDTO_Updated);
                Assert.IsNotNull(ratesForAsset1Production_Metric);

                Assert.AreEqual(GetTruncatedValueforDouble((double)(5279.87 * fct_Pressure), 2), (double)ratesForAsset1Production_Metric.Values[0].ReservoirPressureBound, tol, "Mismatch for ReservoirPressureBound");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(517.12 * fct_Pressure), 2), (double)ratesForAsset1Production_Metric.Values[0].ReservoirPressureTolerance, tol, "Mismatch for ReservoirPressureTolerance");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(993.84 * fct_Pressure), 1), (double)ratesForAsset1Production_Metric.Values[0].PerforationPressureChangeBound, tol, "Mismatch for PerforationPressureChangeBound");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(50 * fct_Pressure), 2), (double)ratesForAsset1Production_Metric.Values[0].WellheadPressureBound, tol, "Mismatch for WellheadPressureBound");
                Assert.AreEqual((decimal)0.86, ratesForAsset1Production_Metric.Values[0].WellAvailability, "Mismatch in Well Availability");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(3000.0 * fct_FlowRate), 2), (double)ratesForAsset1Production_Metric.Values[0].MaximumRate, tol, "Mismatch for Maximum Rate");

                Assert.AreEqual(GetTruncatedValueforDouble((double)(65032.63 * fct_FlowRate), 2), (double)ratesForAsset1Production_Metric.Values[0].ResevoirBoundRate, tol, "Mismatch in  ResevoirBoundRate");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(115315.41 * fct_FlowRate), 2), (double)ratesForAsset1Production_Metric.Values[0].PerforationPressureChangeRate, tol, "Mismatch inPerforationPressureChangeRate");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(96552.89 * fct_FlowRate), 2), (double)ratesForAsset1Production_Metric.Values[0].WellheadBoundRate, tol, "Mismatch in Maximum Wellhead Pressure Rate");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(2580 * fct_FlowRate), 2), (double)ratesForAsset1Production_Metric.Values[0].WellAvailableRate, tol, "Mismatch in Well Available Rate");

                Assert.AreEqual(false, ratesForAsset1Production_Metric.Values[0].HasError, "Error is Detected");
                Assert.AreEqual(true, ratesForAsset1Production_Metric.Values[0].InAnalysis, "Mismatch in IsAnalysis Status");
                #endregion Metric Conversion
            }
            finally
            {
                ChangeUnitSystem("US");
                ChangeUnitSystemUserSetting("US");
                WellAllocationService.DeleteWellAllowableSnapshot(snapshotProduction.Id.ToString());
                WellAllocationService.DeleteHierarchyByAssetId(assets.ElementAt(0).Id.ToString());
            }
        }

        [TestCategory(TestCategories.WellAllocationServiceTests), TestMethod]
        public void CalculateTechnicalRates_GasInjection_WithWellTest()
        {
            ChangeUnitSystem("US");
            ChangeUnitSystemUserSetting("US");
            //Add asset
            #region  Add Asset
            List<AssetDTO> assets = CreateAsset(2);
            #endregion Add Asset
            SWASnapshotDTO snapshotInjection = new SWASnapshotDTO();
            try
            {
                #region Filter with Assets
                WellFilterDTO wellbyFilter = new WellFilterDTO();
                wellbyFilter.AssetIds = new long?[] { assets.ElementAt(0).Id };
                var ApplicableDate = DateTime.Today.AddDays(-1).ToLocalTime();
                #endregion Filter with Assets

                #region ModelFileName
                //ModelfileName
                var modelFileName = "WellfloGasInjectionExample2.wflx";
                //WellTypeId wellType = WellTypeId.PLift;
                var options = new ModelFileOptionDTO
                {
                    CalibrationMethod = CalibrationMethodId.ReservoirPressure,
                    OptionalUpdate = new long[]
                      {
                        (long) OptionalUpdates.UpdateWCT_WGR,
                        (long) OptionalUpdates.UpdateGOR_CGR
                      }
                };
                #endregion ModelFileName

                #region Adding Water Injection Well
                //Create the wells in ForeSite and assign them to the asset created above
                WellDTO GI_Well = AddNonRRLWellGeneral("GI_Well", GetFacilityId("GI_Well", 1), WellTypeId.GInj, WellFluidType.Gas, WellFluidPhase.None, "1", assets.ElementAt(0).Id);
                AddNonRRLModelFile(GI_Well, modelFileName, options.CalibrationMethod, options.OptionalUpdate);
                #endregion Adding Nf Well

                AddWellSettingWithDoubleValues(GI_Well.Id, "Min L Factor Acceptance Limit", 0.1);
                AddWellSettingWithDoubleValues(GI_Well.Id, "Max L Factor Acceptance Limit", 2.0);

                UpdateSystemSettings(GI_Well, new string[] { SettingServiceStringConstants.RESERVOIR_PRESSURE_MAX_AL, SettingServiceStringConstants.RESERVOIR_PRESSURE_MIN_AL }, new double[] { 6000, 0 });

                #region Adding Valid WellTest
                WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(GI_Well.Id.ToString()).Units;
                WellTestDTO testData = new WellTestDTO
                {
                    WellId = GI_Well.Id,
                    SPTCodeDescription = "RepresentativeTest",
                    WellTestType = WellTestType.WellTest,
                    AverageTubingPressure = 1100,
                    AverageTubingTemperature = 100,
                    Gas = 100,
                    ChokeSize = 32,
                    FlowLinePressure = 1100,
                    SeparatorPressure = 600,
                    TestDuration = 12,
                    SampleDate = DateTime.Today.AddDays(-5).ToUniversalTime()
                };

                WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testData));
                WellTestDTO[] allTunedAgainTests = WellTestDataService.GetAllValidWellTestByWellId(GI_Well.Id.ToString());

                WellTestDTO latestTestData_WI = WellTestDataService.GetLatestWellTestDataByWellId(GI_Well.Id.ToString());

                Assert.AreEqual("TUNING_SUCCEEDED", latestTestData_WI.Status.ToString(), "Well Test Status is not Success");
                #endregion Adding Valid WellTest

                #region create network scenario
                SNScenarioDTO scenario = ScenarioConfiguration(assets.ElementAt(0).Id);
                Assert.IsNotNull(scenario);
                #endregion create network scenario

                #region Add Reservoir

                ReservoirAndUnitsDTO addReservoir1 = AddReservoirToAsset("Res1", assets.ElementAt(0).Id);
                // GetReservoir IDs
                ReservoirArrayAndUnitsDTO getReservoirByAsset = WellAllocationService.GetReservoirsByAssetId(assets.ElementAt(0).Id.ToString());
                #endregion

                #region Add Zone
                ZoneAndUnitsDTO addZone1 = AddZoneToReservoir("Zone1", getReservoirByAsset.Values[0].Id);
                #endregion

                #region Get Zone IDs based on reservoir
                // Get Zone IDs based on reservoir
                ZoneArrayAndUnitsDTO getZoneByReservoir = WellAllocationService.GetZonesByReservoirId(getReservoirByAsset.Values[0].Id.ToString());
                #endregion

                #region AddWellToZone
                AddWellToZone(GI_Well.Id, getZoneByReservoir.Values[0].Id);
                #endregion

                #region Set Well Allowable - Production
                string startDateProduction = DTOExtensions.ToISO8601(DateTime.Now.AddDays(-30).ToUniversalTime());
                string endDateProduction = DTOExtensions.ToISO8601(DateTime.Now.AddDays(-20).ToUniversalTime());

                //Create Snapshot
                snapshotInjection = WellAllocationService.CreateWellAllowableSnapshot("Snapshot1", WellAllowableSnapshotType.Sensitivity.ToString(), assets.ElementAt(0).Id.ToString(), startDateProduction, endDateProduction, WellTypeCategory.Injection.ToString(), WellAllowablePhase.Gas.ToString(), scenario.Id.ToString());
                Assert.IsNotNull(snapshotInjection);
                SWAAssetBasedRequestDTO sWAAssetBasedRequestDTO6 = new SWAAssetBasedRequestDTO() { AssetId = assets.ElementAt(0).Id, WellCategory = WellTypeCategory.Injection, SnapshotId = snapshotInjection.Id, FluidPhase = WellAllowablePhase.Gas };
                SWAAssetBasedDTO AssetBasedDTO = WellAllocationService.GetWellAllowableAssetBasedSnapShot(sWAAssetBasedRequestDTO6);
                Assert.IsNotNull(AssetBasedDTO);

                //Getting Rate table as per Snapshot
                SWAWellBasedDTO wellbasedInjectionDTO = new SWAWellBasedDTO();
                wellbasedInjectionDTO.SnapshotId = snapshotInjection.Id;
                wellbasedInjectionDTO.AssetId = assets.ElementAt(0).Id;
                wellbasedInjectionDTO.WellCategory = WellTypeCategory.Injection;
                wellbasedInjectionDTO.FluidPhase = WellAllowablePhase.Gas;

                SWARateArrayAndUnitsDTO ratesForAsset1Production = WellAllocationService.GetWellAllowableRatesForSnapshot(wellbasedInjectionDTO);
                Assert.IsNotNull(ratesForAsset1Production);

                ratesForAsset1Production.Values[0].ReservoirPressureBound = 0;
                ratesForAsset1Production.Values[0].ReservoirPressureTolerance = 0;
                ratesForAsset1Production.Values[0].WellheadPressureBound = (decimal)1514.7;
                ratesForAsset1Production.Values[0].PerforationPressureChangeBound = 500;
                ratesForAsset1Production.Values[0].MaximumRate = 3000;
                ratesForAsset1Production.Values[0].WellAvailability = (decimal)0.95;
                ratesForAsset1Production.Values[0].InAnalysis = true;

                //Update Rate table snap shot
                wellbasedInjectionDTO.Rates = ratesForAsset1Production.Values;
                var updaterateAssetProduction = WellAllocationService.UpdateWellAllowableRatesForSnapshot(wellbasedInjectionDTO);
                Assert.IsNotNull(updaterateAssetProduction);

                #region Get Logs before Calculation
                //Get Logs before calculation
                string[] PreCalculationLogs = WellAllocationService.GetSWALogData(wellbasedInjectionDTO);
                Assert.AreEqual(0, PreCalculationLogs.Length, "Mimatched Logs Count");
                #endregion Get Logs before Calculation

                #region CalculatePreTechnicalandTechnicalRates
                var CalculateTechnicalRates = WellAllocationService.CalculateWellAllowableTechnicalRate(wellbasedInjectionDTO);
                Assert.IsNotNull(CalculateTechnicalRates, "Added Calculated Pre technical rates and Technical rates record failed");
                #endregion CalculatePreTechnicalandTechnicalRates

                #region Get Logs after Calculation
                //Get Logs after calculation
                string[] PostCalculationLogs = WellAllocationService.GetSWALogData(wellbasedInjectionDTO);
                Assert.AreNotSame(0, PostCalculationLogs.Length, "Mimatched Logs Count");
                #endregion Get Logs after Calculation

                //Verification of updated record
                SWAWellBasedDTO wellbasedInjectionDTO_Updated = new SWAWellBasedDTO();
                wellbasedInjectionDTO_Updated.SnapshotId = snapshotInjection.Id;
                wellbasedInjectionDTO_Updated.AssetId = assets.ElementAt(0).Id;
                wellbasedInjectionDTO_Updated.WellCategory = WellTypeCategory.Injection;
                wellbasedInjectionDTO_Updated.FluidPhase = WellAllowablePhase.Gas;

                SWARateArrayAndUnitsDTO ratesForAsset1Production_Updated = WellAllocationService.GetWellAllowableRatesForSnapshot(wellbasedInjectionDTO_Updated);
                Assert.IsNotNull(ratesForAsset1Production_Updated);
                Assert.AreEqual((decimal)2014.7, ratesForAsset1Production_Updated.Values[0].ReservoirPressureBound, "Mismatch for ReservoirPressureBound");
                Assert.AreEqual(0, ratesForAsset1Production_Updated.Values[0].ReservoirPressureTolerance, "Mismatch for ReservoirPressureTolerance");
                Assert.AreEqual(500, ratesForAsset1Production_Updated.Values[0].PerforationPressureChangeBound, "PerforationPressureChangeBound");
                Assert.AreEqual((decimal)1514.7, ratesForAsset1Production_Updated.Values[0].WellheadPressureBound, "Mismatch for WellheadPressureBound");
                Assert.AreEqual((decimal)0.95, ratesForAsset1Production_Updated.Values[0].WellAvailability, "WellAvailability");
                Assert.AreEqual(3000, ratesForAsset1Production_Updated.Values[0].MaximumRate, "Mismatch for Maximum Rate");
                Assert.AreEqual(false, ratesForAsset1Production_Updated.Values[0].HasError, "Error is Detected");
                Assert.AreEqual(true, ratesForAsset1Production_Updated.Values[0].InAnalysis, "Mismatch in IsAnalysis Status");

                Assert.AreEqual(399.45, (double)ratesForAsset1Production_Updated.Values[0].ResevoirBoundRate, 0.05, "Mismatch in  ResevoirBoundRate");
                Assert.AreEqual(148.53, (double)ratesForAsset1Production_Updated.Values[0].PerforationPressureChangeRate, 0.05, "Mismatch in PerforationPressureChangeRate");
                Assert.AreEqual(281.26, (double)ratesForAsset1Production_Updated.Values[0].WellheadBoundRate, 0.05, "Mismatch in WellheadBoundRate");
                Assert.AreEqual(141.10, (double)ratesForAsset1Production_Updated.Values[0].WellAvailableRate, 0.05, "Mismatch in Well Available Rate");
                #endregion Set Well Allowable - Injection

                ChangeUnitSystem("Metric");
                ChangeUnitSystemUserSetting("Metric");

                #region Metric Conversion
                double fct_Pressure = 6.894757;
                double fct_FlowRate = 28.3168466;
                double tol = 0.15;

                SWARateArrayAndUnitsDTO ratesForAsset1Injection_Metric = WellAllocationService.GetWellAllowableRatesForSnapshot(wellbasedInjectionDTO_Updated);
                Assert.IsNotNull(ratesForAsset1Injection_Metric);

                //Assert.AreEqual(GetTruncatedValueforDouble((double)(0.0 * fct_Pressure), 2), (double)ratesForAsset1Injection_Metric.Values[0].ReservoirPressureBound, tol, "Mismatch for ReservoirPressureBound");
                //Assert.AreEqual(GetTruncatedValueforDouble((double)(0.0 * fct_Pressure), 2), (double)ratesForAsset1Injection_Metric.Values[0].ReservoirPressureTolerance, tol, "Mismatch for ReservoirPressureTolerance");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(500.0 * fct_Pressure), 2), (double)ratesForAsset1Injection_Metric.Values[0].PerforationPressureChangeBound, tol, "Mismatch for PerforationPressureChangeBound");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(1514.7 * fct_Pressure), 2), (double)ratesForAsset1Injection_Metric.Values[0].WellheadPressureBound, tol, "Mismatch for WellheadPressureBound");
                Assert.AreEqual((decimal)0.95, ratesForAsset1Injection_Metric.Values[0].WellAvailability, "Mismatch in Well Availability");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(3000.0 * fct_FlowRate), 2), (double)ratesForAsset1Injection_Metric.Values[0].MaximumRate, tol, "Mismatch for Maximum Rate");

                Assert.AreEqual(GetTruncatedValueforDouble((double)(399.45 * fct_FlowRate), 2), (double)ratesForAsset1Injection_Metric.Values[0].ResevoirBoundRate, tol, "Mismatch in  ResevoirBoundRate");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(148.53 * fct_FlowRate), 2), (double)ratesForAsset1Injection_Metric.Values[0].PerforationPressureChangeRate, tol, "Mismatch inPerforationPressureChangeRate");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(281.26 * fct_FlowRate), 2), (double)ratesForAsset1Injection_Metric.Values[0].WellheadBoundRate, tol, "Mismatch in Maximum Wellhead Pressure Rate");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(141.10 * fct_FlowRate), 2), (double)ratesForAsset1Injection_Metric.Values[0].WellAvailableRate, tol, "Mismatch in Well Available Rate");

                Assert.AreEqual(false, ratesForAsset1Injection_Metric.Values[0].HasError, "Error is Detected");
                Assert.AreEqual(true, ratesForAsset1Injection_Metric.Values[0].InAnalysis, "Mismatch in IsAnalysis Status");
                #endregion Metric Conversion
            }
            finally
            {
                ChangeUnitSystem("US");
                ChangeUnitSystemUserSetting("US");
                WellAllocationService.DeleteWellAllowableSnapshot(snapshotInjection.Id.ToString());
                WellAllocationService.DeleteHierarchyByAssetId(assets.ElementAt(0).Id.ToString());
                WellAllocationService.DeleteHierarchyByAssetId(assets.ElementAt(1).Id.ToString());
            }
        }

        [TestCategory(TestCategories.WellAllocationServiceTests), TestMethod]
        public void CalculateTechnicalRates_WI_WithoutWellTest()
        {
            ChangeUnitSystem("US");
            ChangeUnitSystemUserSetting("US");
            //Add asset
            #region  Add Asset
            List<AssetDTO> assets = CreateAsset(2);
            #endregion Add Asset
            SWASnapshotDTO snapshotInjection = new SWASnapshotDTO();
            try
            {
                #region Filter with Assets
                WellFilterDTO wellbyFilter = new WellFilterDTO();
                wellbyFilter.AssetIds = new long?[] { assets.ElementAt(0).Id };
                var ApplicableDate = DateTime.Today.AddDays(-1).ToLocalTime();
                #endregion Filter with Assets

                #region ModelFileName
                //ModelfileName
                var modelFileName = "WellfloWaterInjectionExample1.wflx";
                var options = new ModelFileOptionDTO
                {
                    CalibrationMethod = CalibrationMethodId.ReservoirPressure,
                    OptionalUpdate = new long[]
                      {
                        (long) OptionalUpdates.UpdateWCT_WGR,
                        (long) OptionalUpdates.UpdateGOR_CGR
                      }
                };
                #endregion ModelFileName

                #region Adding Water Injection Well
                //Create the wells in ForeSite and assign them to the asset created above
                WellDTO WI_Well = AddNonRRLWellGeneral("WI_Well", GetFacilityId("WI_Well", 1), WellTypeId.WInj, WellFluidType.None, WellFluidPhase.None, "1", assets.ElementAt(0).Id);
                AddNonRRLModelFile(WI_Well, modelFileName, options.CalibrationMethod, options.OptionalUpdate);
                #endregion Adding Nf Well

                #region create network scenario
                SNScenarioDTO scenario = ScenarioConfiguration(assets.ElementAt(0).Id);
                Assert.IsNotNull(scenario);
                #endregion create network scenario

                #region Add Reservoir

                ReservoirAndUnitsDTO addReservoir1 = AddReservoirToAsset("Res1", assets.ElementAt(0).Id);
                // GetReservoir IDs
                ReservoirArrayAndUnitsDTO getReservoirByAsset = WellAllocationService.GetReservoirsByAssetId(assets.ElementAt(0).Id.ToString());
                #endregion

                #region Add Zone
                ZoneAndUnitsDTO addZone1 = AddZoneToReservoir("Zone1", getReservoirByAsset.Values[0].Id);
                #endregion

                #region Get Zone IDs based on reservoir
                // Get Zone IDs based on reservoir
                ZoneArrayAndUnitsDTO getZoneByReservoir = WellAllocationService.GetZonesByReservoirId(getReservoirByAsset.Values[0].Id.ToString());
                #endregion

                #region AddWellToZone
                AddWellToZone(WI_Well.Id, getZoneByReservoir.Values[0].Id);
                #endregion

                #region Set Well Allowable -Injection
                string startDateInjection = DTOExtensions.ToISO8601(DateTime.Now.AddDays(-30).ToUniversalTime());
                string endDateInjection = DTOExtensions.ToISO8601(DateTime.Now.AddDays(-20).ToUniversalTime());

                //Create Snapshot
                snapshotInjection = WellAllocationService.CreateWellAllowableSnapshot("Snapshot1", WellAllowableSnapshotType.Sensitivity.ToString(), assets.ElementAt(0).Id.ToString(), startDateInjection, endDateInjection, WellTypeCategory.Injection.ToString(), WellAllowablePhase.Water.ToString(), scenario.Id.ToString());
                Assert.IsNotNull(snapshotInjection);
                SWAAssetBasedRequestDTO sWAAssetBasedRequestDTO7 = new SWAAssetBasedRequestDTO() { AssetId = assets.ElementAt(0).Id, WellCategory = WellTypeCategory.Injection, SnapshotId = snapshotInjection.Id, FluidPhase = WellAllowablePhase.Water };

                SWAAssetBasedDTO AssetBasedDTO = WellAllocationService.GetWellAllowableAssetBasedSnapShot(sWAAssetBasedRequestDTO7);
                Assert.IsNotNull(AssetBasedDTO);

                //Getting Rate table as per Snapshot
                SWAWellBasedDTO wellbasedInjectionDTO = new SWAWellBasedDTO();
                wellbasedInjectionDTO.SnapshotId = snapshotInjection.Id;
                wellbasedInjectionDTO.AssetId = assets.ElementAt(0).Id;
                wellbasedInjectionDTO.WellCategory = WellTypeCategory.Injection;
                wellbasedInjectionDTO.FluidPhase = WellAllowablePhase.Water;

                SWARateArrayAndUnitsDTO ratesForAsset1Production = WellAllocationService.GetWellAllowableRatesForSnapshot(wellbasedInjectionDTO);
                Assert.IsNotNull(ratesForAsset1Production);

                ratesForAsset1Production.Values[0].ReservoirPressureBound = 0;
                ratesForAsset1Production.Values[0].ReservoirPressureTolerance = 0;
                ratesForAsset1Production.Values[0].WellheadPressureBound = (decimal)3514.7;
                ratesForAsset1Production.Values[0].PerforationPressureChangeBound = 1000;
                ratesForAsset1Production.Values[0].MaximumRate = 3000;
                //ratesForAsset1Production.Values[0].MinimumRate = 80;
                ratesForAsset1Production.Values[0].WellAvailability = (decimal)0.95;
                ratesForAsset1Production.Values[0].InAnalysis = true;

                //Update Rate table snap shot
                wellbasedInjectionDTO.Rates = ratesForAsset1Production.Values;
                var updaterateAssetProduction = WellAllocationService.UpdateWellAllowableRatesForSnapshot(wellbasedInjectionDTO);
                Assert.IsNotNull(updaterateAssetProduction);

                #region Get Logs before Calculation
                //Get Logs before calculation
                string[] PreCalculationLogs = WellAllocationService.GetSWALogData(wellbasedInjectionDTO);
                Assert.AreEqual(0, PreCalculationLogs.Length, "Mimatched Logs Count");
                #endregion Get Logs before Calculation

                #region CalculatePreTechnicalandTechnicalRates
                var CalculateTechnicalRates = WellAllocationService.CalculateWellAllowableTechnicalRate(wellbasedInjectionDTO);
                Assert.IsNotNull(CalculateTechnicalRates, "Added Calculated Pre technical rates and Technical rates record failed");
                #endregion CalculatePreTechnicalandTechnicalRates

                #region Get Logs after Calculation
                //Get Logs after calculation
                string[] PostCalculationLogs = WellAllocationService.GetSWALogData(wellbasedInjectionDTO);
                Assert.AreNotSame(0, PostCalculationLogs.Length, "Mimatched Logs Count");
                #endregion Get Logs after Calculation

                //Verification of updated recordx``
                SWAWellBasedDTO wellbasedInjectionDTO_Updated = new SWAWellBasedDTO();
                wellbasedInjectionDTO_Updated.SnapshotId = snapshotInjection.Id;
                wellbasedInjectionDTO_Updated.AssetId = assets.ElementAt(0).Id;
                wellbasedInjectionDTO_Updated.WellCategory = WellTypeCategory.Injection;
                wellbasedInjectionDTO_Updated.FluidPhase = WellAllowablePhase.Water;

                SWARateArrayAndUnitsDTO ratesForAsset1Production_Updated = WellAllocationService.GetWellAllowableRatesForSnapshot(wellbasedInjectionDTO_Updated);
                Assert.IsNotNull(ratesForAsset1Production_Updated);

                Assert.AreEqual((decimal) 8172.4, ratesForAsset1Production_Updated.Values[0].ReservoirPressureBound, "Mismatch for ReservoirPressureBound");
                Assert.AreEqual(0, ratesForAsset1Production_Updated.Values[0].ReservoirPressureTolerance, "Mismatch for Fracture Pressure Tolerance");
                Assert.AreEqual(1000, ratesForAsset1Production_Updated.Values[0].PerforationPressureChangeBound, "Mismatch for Injection Margin");
                Assert.AreEqual((decimal)3514.7, ratesForAsset1Production_Updated.Values[0].WellheadPressureBound, "Mismatch for Maximum Well Head Pressure");
                Assert.AreEqual((decimal)0.95, ratesForAsset1Production_Updated.Values[0].WellAvailability, "Mismatch in Well Availability Factor");
                Assert.AreEqual(3000, ratesForAsset1Production_Updated.Values[0].MaximumRate, "Mismatch for Maximum Rate");

                Assert.AreEqual(9807.5, Math.Round(Convert.ToDouble(ratesForAsset1Production_Updated.Values[0].ResevoirBoundRate.ToString()), 1), 0.005, "Mismatch in ResevoirBoundRate.");
                Assert.AreEqual(5864.3, Math.Round(Convert.ToDouble(ratesForAsset1Production_Updated.Values[0].PerforationPressureChangeRate.ToString()), 1), 0.005, "Mismatch in PerforationPressureChangeRate");
                Assert.AreEqual(6932.8, Math.Round(Convert.ToDouble(ratesForAsset1Production_Updated.Values[0].WellheadBoundRate.ToString()), 1), 0.05, "Mismatch in Maximum Wellhead Pressure Rate");
                Assert.AreEqual(2850, ratesForAsset1Production_Updated.Values[0].WellAvailableRate, "Mismatch in Well Available Rate");

                Assert.AreEqual(false, ratesForAsset1Production_Updated.Values[0].HasError, "Error is Detected");
                Assert.AreEqual(true, ratesForAsset1Production_Updated.Values[0].InAnalysis, "Mismatch in IsAnalysis Status");
                #endregion Set Well Allowable -Injection

                ChangeUnitSystem("Metric");
                ChangeUnitSystemUserSetting("Metric");

                #region Metric Conversion
                double fct_Pressure = 6.894757;
                double fct_FlowRate = 0.1589873;
                double tol = 0.2;

                SWARateArrayAndUnitsDTO ratesForAsset1Injection_Metric = WellAllocationService.GetWellAllowableRatesForSnapshot(wellbasedInjectionDTO_Updated);
                Assert.IsNotNull(ratesForAsset1Injection_Metric);

                //Assert.AreEqual(GetTruncatedValueforDouble((double)(0.0 * fct_Pressure), 2), (double)ratesForAsset1Injection_Metric.Values[0].ReservoirPressureBound, tol, "Mismatch for ReservoirPressureBound");
                //Assert.AreEqual(GetTruncatedValueforDouble((double)(0.0 * fct_Pressure), 2), (double)ratesForAsset1Injection_Metric.Values[0].ReservoirPressureTolerance, tol, "Mismatch for ReservoirPressureTolerance");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(1000.0 * fct_Pressure), 2), (double)ratesForAsset1Injection_Metric.Values[0].PerforationPressureChangeBound, tol, "Mismatch for PerforationPressureChangeBound");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(3514.7 * fct_Pressure), 2), (double)ratesForAsset1Injection_Metric.Values[0].WellheadPressureBound, tol, "Mismatch for WellheadPressureBound");
                Assert.AreEqual((decimal)0.95, ratesForAsset1Injection_Metric.Values[0].WellAvailability, "Mismatch in Well Availability");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(3000.0 * fct_FlowRate), 2), (double)ratesForAsset1Injection_Metric.Values[0].MaximumRate, tol, "Mismatch for Maximum Rate");

                Assert.AreEqual(GetTruncatedValueforDouble((double)(9807.488 * fct_FlowRate), 2), (double)ratesForAsset1Injection_Metric.Values[0].ResevoirBoundRate, tol, "Mismatch in  ResevoirBoundRate");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(5864.319 * fct_FlowRate), 2), (double)ratesForAsset1Injection_Metric.Values[0].PerforationPressureChangeRate, tol, "Mismatch inPerforationPressureChangeRate");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(6932.78 * fct_FlowRate), 2), (double)ratesForAsset1Injection_Metric.Values[0].WellheadBoundRate, tol, "Mismatch in Maximum Wellhead Pressure Rate");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(2850.0 * fct_FlowRate), 2), (double)ratesForAsset1Injection_Metric.Values[0].WellAvailableRate, tol, "Mismatch in Well Available Rate");

                Assert.AreEqual(false, ratesForAsset1Injection_Metric.Values[0].HasError, "Error is Detected");
                Assert.AreEqual(true, ratesForAsset1Injection_Metric.Values[0].InAnalysis, "Mismatch in IsAnalysis Status");
                #endregion Metric Conversion
            }
            finally
            {
                ChangeUnitSystem("US");
                ChangeUnitSystemUserSetting("US");
                WellAllocationService.DeleteWellAllowableSnapshot(snapshotInjection.Id.ToString());
                WellAllocationService.DeleteHierarchyByAssetId(assets.ElementAt(0).Id.ToString());
                WellAllocationService.DeleteHierarchyByAssetId(assets.ElementAt(1).Id.ToString());
            }
        }

        [TestCategory(TestCategories.WellAllocationServiceTests), TestMethod]
        public void CalculateTechnicalRates_NFWellDryGas_WithoutWellTest()
        {
            ChangeUnitSystem("US");
            ChangeUnitSystemUserSetting("US");
            //Add asset
            #region  Add Asset
            List<AssetDTO> assets = CreateAsset(2);
            #endregion Add Asset
            SWASnapshotDTO snapshotProduction = new SWASnapshotDTO();
            try
            {
                #region Filter with Assets
                WellFilterDTO wellbyFilter = new WellFilterDTO();
                wellbyFilter.AssetIds = new long?[] { assets.ElementAt(0).Id };
                var ApplicableDate = DateTime.Today.AddDays(-1).ToLocalTime();
                #endregion Filter with Assets

                #region ModelFileName
                //ModelfileName
                var modelFileName = "Dry Gas - IPR Auto Tuning.wflx";
                //WellTypeId wellType = WellTypeId.PLift;
                var options = new ModelFileOptionDTO
                {
                    CalibrationMethod = CalibrationMethodId.ReservoirPressure,
                    OptionalUpdate = new long[]
                      {
                        (long) OptionalUpdates.UpdateWCT_WGR,
                        //(long) OptionalUpdates.UpdateGOR_CGR
                      }
                };
                #endregion ModelFileName

                #region Adding Nf Well
                //Create the wells in ForeSite and assign them to the asset created above
                WellDTO NFWWell_1 = AddNonRRLWellGeneral("NFWell_1", GetFacilityId("NFWELL_", 1), WellTypeId.NF, WellFluidType.DryGas, WellFluidPhase.None, "1", assets.ElementAt(0).Id);
                AddNonRRLModelFile(NFWWell_1, modelFileName, options.CalibrationMethod, options.OptionalUpdate);
                #endregion Adding Nf Well

                AddWellSettingWithDoubleValues(NFWWell_1.Id, "Min L Factor Acceptance Limit", 0.1);
                AddWellSettingWithDoubleValues(NFWWell_1.Id, "Max L Factor Acceptance Limit", 2.0);

                UpdateSystemSettings(NFWWell_1, new string[] { SettingServiceStringConstants.WATER_GAS_RATIO_MIN_AL, SettingServiceStringConstants.WATER_GAS_RATIO_MAX_AL }, new double[] { 100, 1000 });

                #region create network scenario
                SNScenarioDTO scenario = ScenarioConfiguration(assets.ElementAt(0).Id);
                Assert.IsNotNull(scenario);
                #endregion create network scenario

                #region Add Reservoir

                ReservoirAndUnitsDTO addReservoir1 = AddReservoirToAsset("Res1", assets.ElementAt(0).Id);
                // GetReservoir IDs
                ReservoirArrayAndUnitsDTO getReservoirByAsset = WellAllocationService.GetReservoirsByAssetId(assets.ElementAt(0).Id.ToString());
                #endregion

                #region Add Zone
                ZoneAndUnitsDTO addZone1 = AddZoneToReservoir("Zone1", getReservoirByAsset.Values[0].Id);
                #endregion

                #region Get Zone IDs based on reservoir
                // Get Zone IDs based on reservoir
                ZoneArrayAndUnitsDTO getZoneByReservoir = WellAllocationService.GetZonesByReservoirId(getReservoirByAsset.Values[0].Id.ToString());
                #endregion

                #region AddWellToZone
                AddWellToZone(NFWWell_1.Id, getZoneByReservoir.Values[0].Id);
                #endregion

                #region Set Well Allowable - Production

                string startDateProduction = DTOExtensions.ToISO8601(DateTime.Now.AddDays(-30).ToUniversalTime());
                string endDateProduction = DTOExtensions.ToISO8601(DateTime.Now.AddDays(-20).ToUniversalTime());

                //Create Snapshot
                snapshotProduction = WellAllocationService.CreateWellAllowableSnapshot("Snapshot1", WellAllowableSnapshotType.Sensitivity.ToString(), assets.ElementAt(0).Id.ToString(), startDateProduction, endDateProduction, WellTypeCategory.Production.ToString(), WellAllowablePhase.Gas.ToString(), scenario.Id.ToString());
                Assert.IsNotNull(snapshotProduction);
                SWAAssetBasedRequestDTO sWAAssetBasedRequestDTO8 = new SWAAssetBasedRequestDTO() { AssetId = assets.ElementAt(0).Id, WellCategory = WellTypeCategory.Production, SnapshotId = snapshotProduction.Id, FluidPhase = WellAllowablePhase.Gas };

                SWAAssetBasedDTO AssetBasedDTO = WellAllocationService.GetWellAllowableAssetBasedSnapShot(sWAAssetBasedRequestDTO8);
                //Assert.IsNotNull(AssetBasedDTO);

                //Getting Rate table as per Snapshot
                SWAWellBasedDTO wellbasedProductionDTO = new SWAWellBasedDTO();
                wellbasedProductionDTO.SnapshotId = snapshotProduction.Id;
                wellbasedProductionDTO.AssetId = assets.ElementAt(0).Id;
                wellbasedProductionDTO.WellCategory = WellTypeCategory.Production;
                wellbasedProductionDTO.FluidPhase = WellAllowablePhase.Gas;

                SWARateArrayAndUnitsDTO ratesForAsset1Production = WellAllocationService.GetWellAllowableRatesForSnapshot(wellbasedProductionDTO);
                Assert.IsNotNull(ratesForAsset1Production);

                ratesForAsset1Production.Values[0].ReservoirPressureBound = 0;
                ratesForAsset1Production.Values[0].ReservoirPressureTolerance = 0;
                ratesForAsset1Production.Values[0].WellheadPressureBound = 50;
                ratesForAsset1Production.Values[0].PerforationPressureChangeBound = 1000;
                ratesForAsset1Production.Values[0].MaximumRate = 3000;
                //ratesForAsset1Production.Values[0].MinimumRate = 80;
                ratesForAsset1Production.Values[0].WellAvailability = (decimal)0.86;
                ratesForAsset1Production.Values[0].InAnalysis = true;

                //Update Rate table snap shot
                wellbasedProductionDTO.Rates = ratesForAsset1Production.Values;
                var updaterateAssetProduction = WellAllocationService.UpdateWellAllowableRatesForSnapshot(wellbasedProductionDTO);
                Assert.IsNotNull(updaterateAssetProduction);

                #region Get Logs before Calculation
                //Get Logs before calculation
                string[] PreCalculationLogs = WellAllocationService.GetSWALogData(wellbasedProductionDTO);
                Assert.AreEqual(0, PreCalculationLogs.Length, "Mimatched Logs Count");
                #endregion Get Logs before Calculation

                #region CalculatePreTechnicalandTechnicalRates
                var CalculateTechnicalRates = WellAllocationService.CalculateWellAllowableTechnicalRate(wellbasedProductionDTO);
                Assert.IsNotNull(CalculateTechnicalRates, "Added Calculated Pre technical rates and Technical rates record failed");
                #endregion CalculatePreTechnicalandTechnicalRates

                #region Get Logs after Calculation
                //Get Logs after calculation
                string[] PostCalculationLogs = WellAllocationService.GetSWALogData(wellbasedProductionDTO);
                Assert.AreNotSame(0, PostCalculationLogs.Length, "Mimatched Logs Count");
                #endregion Get Logs after Calculation

                //Verification of updated record
                double tol = 0.2;

                SWAWellBasedDTO wellbasedProductionDTO_Updated = new SWAWellBasedDTO();
                wellbasedProductionDTO_Updated.SnapshotId = snapshotProduction.Id;
                wellbasedProductionDTO_Updated.AssetId = assets.ElementAt(0).Id;
                wellbasedProductionDTO_Updated.WellCategory = WellTypeCategory.Production;
                wellbasedProductionDTO_Updated.FluidPhase = WellAllowablePhase.Gas;

                SWARateArrayAndUnitsDTO ratesForAsset1Production_Updated = WellAllocationService.GetWellAllowableRatesForSnapshot(wellbasedProductionDTO_Updated);
                Assert.IsNotNull(ratesForAsset1Production_Updated);
                Assert.AreEqual(0, ratesForAsset1Production_Updated.Values[0].ReservoirPressureBound, "Mismatch for ReservoirPressureBound");
                Assert.AreEqual(0, ratesForAsset1Production_Updated.Values[0].ReservoirPressureTolerance, "Mismatch for ReservoirPressureTolerance");
                Assert.AreEqual(1000, ratesForAsset1Production_Updated.Values[0].PerforationPressureChangeBound, "PerforationPressureChangeBound");
                Assert.AreEqual(50, ratesForAsset1Production_Updated.Values[0].WellheadPressureBound, "Mismatch for WellheadPressureBound");
                Assert.AreEqual((decimal)0.86, ratesForAsset1Production_Updated.Values[0].WellAvailability, "WellAvailability");
                Assert.AreEqual(3000, ratesForAsset1Production_Updated.Values[0].MaximumRate, "Mismatch for Maximum Rate");
                //Manually generated/calculated using WelFlo [There can be approximation and variation due to this manual interpolation]
                Assert.IsNull(ratesForAsset1Production_Updated.Values[0].ResevoirBoundRate, "Mismatch in  ResevoirBoundRate");
                Assert.AreEqual(1019.16, Math.Round(Convert.ToDouble(ratesForAsset1Production_Updated.Values[0].PerforationPressureChangeRate.ToString()), 3), tol, "Mismatch in PerforationPressureChangeRate");
                Assert.AreEqual(1983.85, Math.Round(Convert.ToDouble(ratesForAsset1Production_Updated.Values[0].WellheadBoundRate.ToString()), 3), tol, "Mismatch in Minimum Wellhead Pressure Rate");
                Assert.AreEqual(876.48, (double)ratesForAsset1Production_Updated.Values[0].WellAvailableRate, tol, "Mismatch in Well Available Rate");
                Assert.AreEqual(false, ratesForAsset1Production_Updated.Values[0].HasError, "Error is Detected");
                Assert.AreEqual(true, ratesForAsset1Production_Updated.Values[0].InAnalysis, "Mismatch in IsAnalysis Status");
                #endregion Set Well Allowable - Production

                ChangeUnitSystem("Metric");
                ChangeUnitSystemUserSetting("Metric");
                #region Metric Conversion
                double fct_Pressure = 6.894757;
                double fct_FlowRate = 28.3168466;

                SWARateArrayAndUnitsDTO ratesForAsset1Production_Metric = WellAllocationService.GetWellAllowableRatesForSnapshot(wellbasedProductionDTO_Updated);
                Assert.IsNotNull(ratesForAsset1Production_Metric);

                Assert.AreEqual(0, (double)ratesForAsset1Production_Metric.Values[0].ReservoirPressureBound, tol, "Mismatch for ReservoirPressureBound");
                Assert.AreEqual(0, (double)ratesForAsset1Production_Metric.Values[0].ReservoirPressureTolerance, tol, "Mismatch for ReservoirPressureTolerance");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(1000.0 * fct_Pressure), 2), (double)ratesForAsset1Production_Metric.Values[0].PerforationPressureChangeBound, tol, "Mismatch for PerforationPressureChangeBound");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(50 * fct_Pressure), 2), (double)ratesForAsset1Production_Metric.Values[0].WellheadPressureBound, tol, "Mismatch for WellheadPressureBound");
                Assert.AreEqual((decimal)0.86, ratesForAsset1Production_Metric.Values[0].WellAvailability, "Mismatch in Well Availability");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(3000.0 * fct_FlowRate), 2), (double)ratesForAsset1Production_Metric.Values[0].MaximumRate, tol, "Mismatch for Maximum Rate");

                Assert.IsNull(ratesForAsset1Production_Metric.Values[0].ResevoirBoundRate, "Mismatch in  ResevoirBoundRate");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(1019.16 * fct_FlowRate), 2), (double)ratesForAsset1Production_Metric.Values[0].PerforationPressureChangeRate, tol, "Mismatch inPerforationPressureChangeRate");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(1983.90 * fct_FlowRate), 2), (double)ratesForAsset1Production_Metric.Values[0].WellheadBoundRate, tol, "Mismatch in Maximum Wellhead Pressure Rate");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(876.48 * fct_FlowRate), 2), (double)ratesForAsset1Production_Metric.Values[0].WellAvailableRate, tol, "Mismatch in Well Available Rate");

                Assert.AreEqual(false, ratesForAsset1Production_Metric.Values[0].HasError, "Error is Detected");
                Assert.AreEqual(true, ratesForAsset1Production_Metric.Values[0].InAnalysis, "Mismatch in IsAnalysis Status");
                #endregion Metric Conversion
            }
            finally
            {
                ChangeUnitSystem("US");
                ChangeUnitSystemUserSetting("US");
                WellAllocationService.DeleteWellAllowableSnapshot(snapshotProduction.Id.ToString());
                WellAllocationService.DeleteHierarchyByAssetId(assets.ElementAt(0).Id.ToString());
                WellAllocationService.DeleteHierarchyByAssetId(assets.ElementAt(1).Id.ToString());
            }
        }

        [TestCategory(TestCategories.WellAllocationServiceTests), TestMethod]
        public void CalculateTechnicalRates_NFCondensate_WithoutWellTest()
        {
            ChangeUnitSystem("US");
            ChangeUnitSystemUserSetting("US");
            //Add asset
            #region  Add Asset
            List<AssetDTO> assets = CreateAsset(2);
            #endregion Add Asset
            SWASnapshotDTO snapshotInjection = new SWASnapshotDTO();
            try
            {
                #region Filter with Assets
                WellFilterDTO wellbyFilter = new WellFilterDTO();
                wellbyFilter.AssetIds = new long?[] { assets.ElementAt(0).Id };
                var ApplicableDate = DateTime.Today.AddDays(-1).ToLocalTime();
                #endregion Filter with Assets

                #region ModelFileName
                //ModelfileName

                //ModelfileName
                var modelFileName = "Condensate Gas - IPR Auto Tuning.wflx";

                var options = new ModelFileOptionDTO
                {
                    CalibrationMethod = CalibrationMethodId.ReservoirPressure,
                    OptionalUpdate = new long[]
                      {
                        (long) OptionalUpdates.UpdateWCT_WGR,
                        (long) OptionalUpdates.UpdateGOR_CGR
                      }
                };
                #endregion ModelFileName

                #region Adding Nf Well
                //Create the wells in ForeSite and assign them to the asset created above
                WellDTO NFWWell_CO = AddNonRRLWellGeneral("NFWell_1", GetFacilityId("NFWELL_", 1), WellTypeId.NF, WellFluidType.Condensate, WellFluidPhase.None, "1", assets.ElementAt(0).Id);
                AddNonRRLModelFile(NFWWell_CO, modelFileName, options.CalibrationMethod, options.OptionalUpdate);
                #endregion Adding Nf Well

                AddWellSettingWithDoubleValues(NFWWell_CO.Id, "Min L Factor Acceptance Limit", 0.1);
                AddWellSettingWithDoubleValues(NFWWell_CO.Id, "Max L Factor Acceptance Limit", 2.0);

                UpdateSystemSettings(NFWWell_CO, new string[] { SettingServiceStringConstants.CONDENSATE_GAS_RATIO_MAX_AL, SettingServiceStringConstants.CONDENSATE_GAS_RATIO_MIN_AL }, new double[] { 600, 0 });

                #region create network scenario
                SNScenarioDTO scenario = ScenarioConfiguration(assets.ElementAt(0).Id);
                Assert.IsNotNull(scenario);
                #endregion create network scenario

                #region Add Reservoir

                ReservoirAndUnitsDTO addReservoir1 = AddReservoirToAsset("Res1", assets.ElementAt(0).Id);
                // GetReservoir IDs
                ReservoirArrayAndUnitsDTO getReservoirByAsset = WellAllocationService.GetReservoirsByAssetId(assets.ElementAt(0).Id.ToString());
                #endregion

                #region Add Zone
                ZoneAndUnitsDTO addZone1 = AddZoneToReservoir("Zone1", getReservoirByAsset.Values[0].Id);
                #endregion

                #region Get Zone IDs based on reservoir
                // Get Zone IDs based on reservoir
                ZoneArrayAndUnitsDTO getZoneByReservoir = WellAllocationService.GetZonesByReservoirId(getReservoirByAsset.Values[0].Id.ToString());
                #endregion

                #region AddWellToZone
                AddWellToZone(NFWWell_CO.Id, getZoneByReservoir.Values[0].Id);
                #endregion

                #region Set Well Allowable - Production
                string startDateProduction = DTOExtensions.ToISO8601(DateTime.Now.AddDays(-30).ToUniversalTime());
                string endDateProduction = DTOExtensions.ToISO8601(DateTime.Now.AddDays(-20).ToUniversalTime());

                //Create Snapshot
                snapshotInjection = WellAllocationService.CreateWellAllowableSnapshot("Snapshot1", WellAllowableSnapshotType.Sensitivity.ToString(), assets.ElementAt(0).Id.ToString(), startDateProduction, endDateProduction, WellTypeCategory.Production.ToString(), WellAllowablePhase.Gas.ToString(), scenario.Id.ToString());
                Assert.IsNotNull(snapshotInjection);
                SWAAssetBasedRequestDTO sWAAssetBasedRequestDTO8 = new SWAAssetBasedRequestDTO() { AssetId = assets.ElementAt(0).Id, WellCategory = WellTypeCategory.Production, SnapshotId = snapshotInjection.Id, FluidPhase = WellAllowablePhase.Gas };

                SWAAssetBasedDTO AssetBasedDTO = WellAllocationService.GetWellAllowableAssetBasedSnapShot(sWAAssetBasedRequestDTO8);
                //Assert.IsNotNull(AssetBasedDTO);

                //Getting Rate table as per Snapshot
                SWAWellBasedDTO wellbasedProductionDTO = new SWAWellBasedDTO();
                wellbasedProductionDTO.SnapshotId = snapshotInjection.Id;
                wellbasedProductionDTO.AssetId = assets.ElementAt(0).Id;
                wellbasedProductionDTO.WellCategory = WellTypeCategory.Production;
                wellbasedProductionDTO.FluidPhase = WellAllowablePhase.Gas;

                SWARateArrayAndUnitsDTO ratesForAsset1Production = WellAllocationService.GetWellAllowableRatesForSnapshot(wellbasedProductionDTO);
                Assert.IsNotNull(ratesForAsset1Production);

                ratesForAsset1Production.Values[0].ReservoirPressureBound = (decimal)5334.46;
                ratesForAsset1Production.Values[0].ReservoirPressureTolerance = (decimal)528.49;
                ratesForAsset1Production.Values[0].WellheadPressureBound = 350;
                ratesForAsset1Production.Values[0].PerforationPressureChangeBound = (decimal)960.17;
                ratesForAsset1Production.Values[0].MaximumRate = 3000;
                //ratesForAsset1Production.Values[0].MinimumRate = 80;
                ratesForAsset1Production.Values[0].WellAvailability = (decimal)0.86;
                ratesForAsset1Production.Values[0].InAnalysis = true;

                //Update Rate table snap shot
                wellbasedProductionDTO.Rates = ratesForAsset1Production.Values;
                var updaterateAssetProduction = WellAllocationService.UpdateWellAllowableRatesForSnapshot(wellbasedProductionDTO);
                Assert.IsNotNull(updaterateAssetProduction);

                #region Get Logs before Calculation
                //Get Logs before calculation
                string[] PreCalculationLogs = WellAllocationService.GetSWALogData(wellbasedProductionDTO);
                Assert.AreEqual(0, PreCalculationLogs.Length, "Mimatched Logs Count");
                #endregion Get Logs before Calculation

                #region CalculatePreTechnicalandTechnicalRates
                var CalculateTechnicalRates = WellAllocationService.CalculateWellAllowableTechnicalRate(wellbasedProductionDTO);
                Assert.IsNotNull(CalculateTechnicalRates, "Added Calculated Pre technical rates and Technical rates record failed");
                #endregion CalculatePreTechnicalandTechnicalRates

                #region Get Logs after Calculation
                //Get Logs after calculation
                string[] PostCalculationLogs = WellAllocationService.GetSWALogData(wellbasedProductionDTO);
                Assert.AreNotSame(0, PostCalculationLogs.Length, "Mimatched Logs Count");
                #endregion Get Logs after Calculation

                //Verification of updated record
                double tol = 0.2;

                SWAWellBasedDTO wellbasedProductionDTO_Updated = new SWAWellBasedDTO();
                wellbasedProductionDTO_Updated.SnapshotId = snapshotInjection.Id;
                wellbasedProductionDTO_Updated.AssetId = assets.ElementAt(0).Id;
                wellbasedProductionDTO_Updated.WellCategory = WellTypeCategory.Production;
                wellbasedProductionDTO_Updated.FluidPhase = WellAllowablePhase.Gas;

                SWARateArrayAndUnitsDTO ratesForAsset1Production_Updated = WellAllocationService.GetWellAllowableRatesForSnapshot(wellbasedProductionDTO_Updated);
                Assert.IsNotNull(ratesForAsset1Production_Updated);

                Assert.AreEqual(5334.46, (double)ratesForAsset1Production_Updated.Values[0].ReservoirPressureBound, tol, "Mismatch for ReservoirPressureBound");
                Assert.AreEqual(528.49, (double)ratesForAsset1Production_Updated.Values[0].ReservoirPressureTolerance, tol, "Mismatch for ReservoirPressureTolerance");
                Assert.AreEqual(960.17, (double)ratesForAsset1Production_Updated.Values[0].PerforationPressureChangeBound, tol, "PerforationPressureChangeBound");
                Assert.AreEqual(350.0, (double)ratesForAsset1Production_Updated.Values[0].WellheadPressureBound, tol, "Mismatch for WellheadPressureBound");
                Assert.AreEqual(0.86, (double)ratesForAsset1Production_Updated.Values[0].WellAvailability, tol, "WellAvailability");
                Assert.AreEqual(3000, ratesForAsset1Production_Updated.Values[0].MaximumRate, "Mismatch for Maximum Rate");

                Assert.AreEqual(28519.25, Math.Round(Convert.ToDouble(ratesForAsset1Production_Updated.Values[0].ResevoirBoundRate.ToString()), 2), tol, "Mismatch in  ResevoirBoundRate");
                Assert.AreEqual(103588.31, Math.Round(Convert.ToDouble(ratesForAsset1Production_Updated.Values[0].PerforationPressureChangeRate.ToString()), 2), tol, "Mismatch in Maximum Drawdown Rate");
                Assert.AreEqual(90890.82, (double)ratesForAsset1Production_Updated.Values[0].WellheadBoundRate, tol, "Mismatch in Minimum Wellhead Pressure Rate");
                Assert.AreEqual(2580, (double)ratesForAsset1Production_Updated.Values[0].WellAvailableRate, tol, "Mismatch in Well Available Rate");

                Assert.AreEqual(false, ratesForAsset1Production_Updated.Values[0].HasError, "Error is Detected");
                Assert.AreEqual(true, ratesForAsset1Production_Updated.Values[0].InAnalysis, "Mismatch in IsAnalysis Status");
                #endregion Set Well Allowable - Production

                ChangeUnitSystem("Metric");
                ChangeUnitSystemUserSetting("Metric");

                #region Metric Conversion
                double fct_Pressure = 6.894757;
                double fct_FlowRate = 28.3168466;

                SWARateArrayAndUnitsDTO ratesForAsset1Production_Metric = WellAllocationService.GetWellAllowableRatesForSnapshot(wellbasedProductionDTO_Updated);
                Assert.IsNotNull(ratesForAsset1Production_Metric);

                Assert.AreEqual(GetTruncatedValueforDouble((double)(5334.46 * fct_Pressure), 2), (double)ratesForAsset1Production_Metric.Values[0].ReservoirPressureBound, tol, "Mismatch for ReservoirPressureBound");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(528.49 * fct_Pressure), 2), (double)ratesForAsset1Production_Metric.Values[0].ReservoirPressureTolerance, tol, "Mismatch for ReservoirPressureTolerance");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(960.17 * fct_Pressure), 1), (double)ratesForAsset1Production_Metric.Values[0].PerforationPressureChangeBound, tol, "Mismatch for PerforationPressureChangeBound");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(350.0 * fct_Pressure), 2), (double)ratesForAsset1Production_Metric.Values[0].WellheadPressureBound, tol, "Mismatch for WellheadPressureBound");
                Assert.AreEqual((decimal)0.86, ratesForAsset1Production_Metric.Values[0].WellAvailability, "Mismatch in Well Availability");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(3000.0 * fct_FlowRate), 2), (double)ratesForAsset1Production_Metric.Values[0].MaximumRate, tol, "Mismatch for Maximum Rate");

                Assert.AreEqual(GetTruncatedValueforDouble((double)(28519.25 * fct_FlowRate), 2), (double)ratesForAsset1Production_Metric.Values[0].ResevoirBoundRate, tol, "Mismatch in  ResevoirBoundRate");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(103588.31 * fct_FlowRate), 2), (double)ratesForAsset1Production_Metric.Values[0].PerforationPressureChangeRate, tol, "Mismatch inPerforationPressureChangeRate");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(90890.82 * fct_FlowRate), 2), (double)ratesForAsset1Production_Metric.Values[0].WellheadBoundRate, tol, "Mismatch in Maximum Wellhead Pressure Rate");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(2580 * fct_FlowRate), 2), (double)ratesForAsset1Production_Metric.Values[0].WellAvailableRate, tol, "Mismatch in Well Available Rate");

                Assert.AreEqual(false, ratesForAsset1Production_Metric.Values[0].HasError, "Error is Detected");
                Assert.AreEqual(true, ratesForAsset1Production_Metric.Values[0].InAnalysis, "Mismatch in IsAnalysis Status");
                #endregion Metric Conversion
            }
            finally
            {
                ChangeUnitSystem("US");
                ChangeUnitSystemUserSetting("US");
                WellAllocationService.DeleteWellAllowableSnapshot(snapshotInjection.Id.ToString());
                WellAllocationService.DeleteHierarchyByAssetId(assets.ElementAt(0).Id.ToString());
                WellAllocationService.DeleteHierarchyByAssetId(assets.ElementAt(1).Id.ToString());
            }
        }

        [TestCategory(TestCategories.WellAllocationServiceTests), TestMethod]
        public void CalculateTechnicalRates_GasInjection_WithoutWellTest()
        {
            ChangeUnitSystem("US");
            ChangeUnitSystemUserSetting("US");

            //Add asset
            #region  Add Asset
            List<AssetDTO> assets = CreateAsset(2);
            #endregion Add Asset
            SWASnapshotDTO snapshotInjection = new SWASnapshotDTO();
            try
            {
                #region Filter with Assets
                WellFilterDTO wellbyFilter = new WellFilterDTO();
                wellbyFilter.AssetIds = new long?[] { assets.ElementAt(0).Id };
                var ApplicableDate = DateTime.Today.AddDays(-1).ToLocalTime();
                #endregion Filter with Assets

                #region ModelFileName
                //ModelfileName
                var modelFileName = "WellfloGasInjectionExample2.wflx";
                //WellTypeId wellType = WellTypeId.PLift;
                var options = new ModelFileOptionDTO
                {
                    CalibrationMethod = CalibrationMethodId.ReservoirPressure,
                    OptionalUpdate = new long[]
                      {
                        (long) OptionalUpdates.UpdateWCT_WGR,
                        (long) OptionalUpdates.UpdateGOR_CGR
                      }
                };
                #endregion ModelFileName

                #region Adding Water Injection Well
                //Create the wells in ForeSite and assign them to the asset created above
                WellDTO GI_Well = AddNonRRLWellGeneral("GI_Well", GetFacilityId("GI_Well", 1), WellTypeId.GInj, WellFluidType.Gas, WellFluidPhase.None, "1", assets.ElementAt(0).Id);
                AddNonRRLModelFile(GI_Well, modelFileName, options.CalibrationMethod, options.OptionalUpdate);
                #endregion Adding Nf Well

                AddWellSettingWithDoubleValues(GI_Well.Id, "Min L Factor Acceptance Limit", 0.1);
                AddWellSettingWithDoubleValues(GI_Well.Id, "Max L Factor Acceptance Limit", 2.0);

                UpdateSystemSettings(GI_Well, new string[] { SettingServiceStringConstants.RESERVOIR_PRESSURE_MAX_AL, SettingServiceStringConstants.RESERVOIR_PRESSURE_MIN_AL }, new double[] { 6000, 0 });

                #region create network scenario
                SNScenarioDTO scenario = ScenarioConfiguration(assets.ElementAt(0).Id);
                Assert.IsNotNull(scenario);
                #endregion create network scenario

                #region Add Reservoir

                ReservoirAndUnitsDTO addReservoir1 = AddReservoirToAsset("Res1", assets.ElementAt(0).Id);
                // GetReservoir IDs
                ReservoirArrayAndUnitsDTO getReservoirByAsset = WellAllocationService.GetReservoirsByAssetId(assets.ElementAt(0).Id.ToString());
                #endregion

                #region Add Zone
                ZoneAndUnitsDTO addZone1 = AddZoneToReservoir("Zone1", getReservoirByAsset.Values[0].Id);
                #endregion

                #region Get Zone IDs based on reservoir
                // Get Zone IDs based on reservoir
                ZoneArrayAndUnitsDTO getZoneByReservoir = WellAllocationService.GetZonesByReservoirId(getReservoirByAsset.Values[0].Id.ToString());
                #endregion

                #region AddWellToZone
                AddWellToZone(GI_Well.Id, getZoneByReservoir.Values[0].Id);
                #endregion

                #region Set Well Allowable - Production

                string startDateProduction = DTOExtensions.ToISO8601(DateTime.Now.AddDays(-30).ToUniversalTime());
                string endDateProduction = DTOExtensions.ToISO8601(DateTime.Now.AddDays(-20).ToUniversalTime());

                //Create Snapshot
                snapshotInjection = WellAllocationService.CreateWellAllowableSnapshot("Snapshot1", WellAllowableSnapshotType.Sensitivity.ToString(), assets.ElementAt(0).Id.ToString(), startDateProduction, endDateProduction, WellTypeCategory.Injection.ToString(), WellAllowablePhase.Gas.ToString(), scenario.Id.ToString());
                Assert.IsNotNull(snapshotInjection);
                SWAAssetBasedRequestDTO sWAAssetBasedRequestDTO8 = new SWAAssetBasedRequestDTO() { AssetId = assets.ElementAt(0).Id, WellCategory = WellTypeCategory.Injection, SnapshotId = snapshotInjection.Id, FluidPhase = WellAllowablePhase.Gas };

                SWAAssetBasedDTO AssetBasedDTO = WellAllocationService.GetWellAllowableAssetBasedSnapShot(sWAAssetBasedRequestDTO8);
                Assert.IsNotNull(AssetBasedDTO);

                //Getting Rate table as per Snapshot
                SWAWellBasedDTO wellbasedInjectionDTO = new SWAWellBasedDTO();
                wellbasedInjectionDTO.SnapshotId = snapshotInjection.Id;
                wellbasedInjectionDTO.AssetId = assets.ElementAt(0).Id;
                wellbasedInjectionDTO.WellCategory = WellTypeCategory.Injection;
                wellbasedInjectionDTO.FluidPhase = WellAllowablePhase.Gas;

                SWARateArrayAndUnitsDTO ratesForAsset1Production = WellAllocationService.GetWellAllowableRatesForSnapshot(wellbasedInjectionDTO);
                Assert.IsNotNull(ratesForAsset1Production);

                ratesForAsset1Production.Values[0].ReservoirPressureBound = (decimal)2014.7;
                ratesForAsset1Production.Values[0].ReservoirPressureTolerance = 0;
                ratesForAsset1Production.Values[0].WellheadPressureBound = (decimal)1514.7;
                ratesForAsset1Production.Values[0].PerforationPressureChangeBound = 500;
                ratesForAsset1Production.Values[0].MaximumRate = 3000;
                ratesForAsset1Production.Values[0].WellAvailability = (decimal)0.95;
                ratesForAsset1Production.Values[0].InAnalysis = true;

                //Update Rate table snap shot
                wellbasedInjectionDTO.Rates = ratesForAsset1Production.Values;
                var updaterateAssetProduction = WellAllocationService.UpdateWellAllowableRatesForSnapshot(wellbasedInjectionDTO);
                Assert.IsNotNull(updaterateAssetProduction);

                #region Get Logs before Calculation
                //Get Logs before calculation
                string[] PreCalculationLogs = WellAllocationService.GetSWALogData(wellbasedInjectionDTO);
                Assert.AreEqual(0, PreCalculationLogs.Length, "Mimatched Logs Count");
                #endregion Get Logs before Calculation

                #region CalculatePreTechnicalandTechnicalRates
                var CalculateTechnicalRates = WellAllocationService.CalculateWellAllowableTechnicalRate(wellbasedInjectionDTO);
                Assert.IsNotNull(CalculateTechnicalRates, "Added Calculated Pre technical rates and Technical rates record failed");
                #endregion CalculatePreTechnicalandTechnicalRates

                #region Get Logs after Calculation
                //Get Logs after calculation
                string[] PostCalculationLogs = WellAllocationService.GetSWALogData(wellbasedInjectionDTO);
                Assert.AreNotSame(0, PostCalculationLogs.Length, "Mimatched Logs Count");
                #endregion Get Logs after Calculation

                //Verification of updated record
                double tol = 0.2;

                SWAWellBasedDTO wellbasedInjectionDTO_Updated = new SWAWellBasedDTO();
                wellbasedInjectionDTO_Updated.SnapshotId = snapshotInjection.Id;
                wellbasedInjectionDTO_Updated.AssetId = assets.ElementAt(0).Id;
                wellbasedInjectionDTO_Updated.WellCategory = WellTypeCategory.Injection;
                wellbasedInjectionDTO_Updated.FluidPhase = WellAllowablePhase.Gas;

                SWARateArrayAndUnitsDTO ratesForAsset1Production_Updated = WellAllocationService.GetWellAllowableRatesForSnapshot(wellbasedInjectionDTO_Updated);
                Assert.IsNotNull(ratesForAsset1Production_Updated);

                Assert.AreEqual(2014.7, Math.Round(Convert.ToDouble(ratesForAsset1Production_Updated.Values[0].ReservoirPressureBound.ToString()), 1), 0.005, "Mismatch for ReservoirPressureBound");
                Assert.AreEqual(0, ratesForAsset1Production_Updated.Values[0].ReservoirPressureTolerance, "Mismatch for ReservoirPressureTolerance");
                Assert.AreEqual(500, ratesForAsset1Production_Updated.Values[0].PerforationPressureChangeBound, "PerforationPressureChangeBound");
                Assert.AreEqual((decimal)1514.7, ratesForAsset1Production_Updated.Values[0].WellheadPressureBound, "Mismatch for WellheadPressureBound");
                Assert.AreEqual((decimal)0.95, ratesForAsset1Production_Updated.Values[0].WellAvailability, "WellAvailability");
                Assert.AreEqual(3000, ratesForAsset1Production_Updated.Values[0].MaximumRate, "Mismatch for Maximum Rate");

                Assert.AreEqual(371.14, Math.Round(Convert.ToDouble(ratesForAsset1Production_Updated.Values[0].ResevoirBoundRate.ToString()), 1), tol, "Mismatch in  ResevoirBoundRate");
                Assert.AreEqual(160.78, Math.Round(Convert.ToDouble(ratesForAsset1Production_Updated.Values[0].PerforationPressureChangeRate.ToString()), 1), tol, "Mismatch in PerforationPressureChangeRate");
                Assert.AreEqual(252.82, Math.Round(Convert.ToDouble(ratesForAsset1Production_Updated.Values[0].WellheadBoundRate.ToString()), 1), tol, "Mismatch in Minimum Wellhead Pressure Rate");
                Assert.AreEqual(152.75, (double)ratesForAsset1Production_Updated.Values[0].WellAvailableRate, tol, "Mismatch in Well Available Rate");

                Assert.AreEqual(false, ratesForAsset1Production_Updated.Values[0].HasError, "Error is Detected");
                Assert.AreEqual(true, ratesForAsset1Production_Updated.Values[0].InAnalysis, "Mismatch in IsAnalysis Status");
                #endregion Set Well Allowable - Injection

                ChangeUnitSystem("Metric");
                ChangeUnitSystemUserSetting("Metric");
                #region Metric Conversion
                double fct_Pressure = 6.894757;
                double fct_FlowRate = 28.3168466;

                SWARateArrayAndUnitsDTO ratesForAsset1Injection_Metric = WellAllocationService.GetWellAllowableRatesForSnapshot(wellbasedInjectionDTO_Updated);
                Assert.IsNotNull(ratesForAsset1Injection_Metric);

                //Assert.AreEqual(GetTruncatedValueforDouble((double)(0.0 * fct_Pressure), 2), (double)ratesForAsset1Injection_Metric.Values[0].ReservoirPressureBound, tol, "Mismatch for ReservoirPressureBound");
                //Assert.AreEqual(GetTruncatedValueforDouble((double)(0.0 * fct_Pressure), 2), (double)ratesForAsset1Injection_Metric.Values[0].ReservoirPressureTolerance, tol, "Mismatch for ReservoirPressureTolerance");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(500.0 * fct_Pressure), 2), (double)ratesForAsset1Injection_Metric.Values[0].PerforationPressureChangeBound, tol, "Mismatch for PerforationPressureChangeBound");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(1514.7 * fct_Pressure), 2), (double)ratesForAsset1Injection_Metric.Values[0].WellheadPressureBound, tol, "Mismatch for WellheadPressureBound");
                Assert.AreEqual((decimal)0.95, ratesForAsset1Injection_Metric.Values[0].WellAvailability, "Mismatch in Well Availability");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(3000.0 * fct_FlowRate), 2), (double)ratesForAsset1Injection_Metric.Values[0].MaximumRate, tol, "Mismatch for Maximum Rate");

                Assert.AreEqual(GetTruncatedValueforDouble((double)(371.14 * fct_FlowRate), 2), (double)ratesForAsset1Injection_Metric.Values[0].ResevoirBoundRate, tol, "Mismatch in  ResevoirBoundRate");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(160.79 * fct_FlowRate), 2), (double)ratesForAsset1Injection_Metric.Values[0].PerforationPressureChangeRate, tol, "Mismatch inPerforationPressureChangeRate");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(252.82 * fct_FlowRate), 2), (double)ratesForAsset1Injection_Metric.Values[0].WellheadBoundRate, tol, "Mismatch in Maximum Wellhead Pressure Rate");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(152.75 * fct_FlowRate), 2), (double)ratesForAsset1Injection_Metric.Values[0].WellAvailableRate, tol, "Mismatch in Well Available Rate");

                Assert.AreEqual(false, ratesForAsset1Injection_Metric.Values[0].HasError, "Error is Detected");
                Assert.AreEqual(true, ratesForAsset1Injection_Metric.Values[0].InAnalysis, "Mismatch in IsAnalysis Status");
                #endregion Metric Conversion
            }
            finally
            {
                ChangeUnitSystem("US");
                ChangeUnitSystemUserSetting("US");
                WellAllocationService.DeleteWellAllowableSnapshot(snapshotInjection.Id.ToString());
                WellAllocationService.DeleteHierarchyByAssetId(assets.ElementAt(0).Id.ToString());
                WellAllocationService.DeleteHierarchyByAssetId(assets.ElementAt(1).Id.ToString());
            }
        }

        [TestCategory(TestCategories.WellAllocationServiceTests), TestMethod]
        public void CalculateWellAllowableTechncalRate_RunStatus()
        {

            #region  Add Asset
            List<AssetDTO> assets = CreateAsset(2);
            #endregion Add Asset
            SWASnapshotDTO snapshotProduction = new SWASnapshotDTO();
            SWASnapshotDTO snapshotInjection = new SWASnapshotDTO();
            try
            {
                //NFW Condennsate wells creation
                var modelFileName_cond = "Condensate Gas - IPR Auto Tuning.wflx";
                var options_cond = new ModelFileOptionDTO
                {
                    CalibrationMethod = CalibrationMethodId.ReservoirPressure,
                    OptionalUpdate = new long[]
                      {
                        (long) OptionalUpdates.UpdateWCT_WGR,
                        (long) OptionalUpdates.UpdateGOR_CGR
                      }
                };
                List<WellDTO> NFW_Con = new List<WellDTO>();
                for (int i = 5; i < 11; i++)
                {
                    WellDTO NFWell_Con = AddNonRRLWellGeneral("NFWell_Con_" + (i), GetFacilityId("NFWELL_", (i + 1)), WellTypeId.NF, WellFluidType.Condensate, WellFluidPhase.None, "1", assets.ElementAt(0).Id);
                    AddNonRRLModelFile(NFWell_Con, modelFileName_cond, options_cond.CalibrationMethod, options_cond.OptionalUpdate);
                    NFW_Con.Add(NFWell_Con);
                }
                for (int i = 11; i < 16; i++)
                {
                    WellDTO NFWell_Con = AddNonRRLWellGeneral("NFWell_Con_WT_" + (i), GetFacilityId("NFWELL_", (i + 1)), WellTypeId.NF, WellFluidType.Condensate, WellFluidPhase.None, "1", assets.ElementAt(0).Id);
                    AddNonRRLModelFile(NFWell_Con, modelFileName_cond, options_cond.CalibrationMethod, options_cond.OptionalUpdate);
                    NFW_Con.Add(NFWell_Con);

                    #region Adding Valid WellTest
                    WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(NFWell_Con.Id.ToString()).Units;
                    WellTestDTO testData = new WellTestDTO
                    {
                        WellId = NFWell_Con.Id,
                        SPTCodeDescription = "AllocatableTest",
                        //CalibrationMethod = options.CalibrationMethod,
                        WellTestType = WellTestType.WellTest,
                        AverageTubingPressure = 1500,
                        AverageTubingTemperature = 100,
                        Gas = 90000,
                        Water = 12622.8m,
                        Oil = 12622.8m,
                        //CGR = 12622.8m,
                        ChokeSize = 32,
                        FlowLinePressure = 4000,
                        SeparatorPressure = 4000,
                        TestDuration = 24,
                        SampleDate = DateTime.Today.AddDays(-5).ToUniversalTime()
                    };

                    WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testData));
                    WellTestDTO[] allTunedAgainTests = WellTestDataService.GetAllValidWellTestByWellId(NFWell_Con.Id.ToString());
                    WellTestDTO latestTestData_NF = WellTestDataService.GetLatestWellTestDataByWellId(NFWell_Con.Id.ToString());

                    Assert.AreEqual("TUNING_SUCCEEDED", latestTestData_NF.Status.ToString(), "Well Test Status is not Success");
                    #endregion Adding Valid WellTest
                }
                //NFW Drygas wells creation
                var modelFileName_Drygas = "Dry Gas - IPR Auto Tuning.wflx";
                var options_Drygas = new ModelFileOptionDTO
                {
                    CalibrationMethod = CalibrationMethodId.ReservoirPressure,
                    OptionalUpdate = new long[]
                      {
                        (long) OptionalUpdates.UpdateWCT_WGR,
                        //(long) OptionalUpdates.UpdateGOR_CGR
                      }
                };

                List<WellDTO> NFW_Dry = new List<WellDTO>();
                for (int i = 16; i < 21; i++)
                {
                    WellDTO NFWell_Dry = AddNonRRLWellGeneral("NFWell_Dry_" + (i), GetFacilityId("NFWELL_", (i + 21)), WellTypeId.NF, WellFluidType.DryGas, WellFluidPhase.None, "1", assets.ElementAt(0).Id);
                    AddNonRRLModelFile(NFWell_Dry, modelFileName_Drygas, options_Drygas.CalibrationMethod, options_Drygas.OptionalUpdate);
                    NFW_Dry.Add(NFWell_Dry);

                }
                for (int i = 21; i < 26; i++)
                {
                    WellDTO NFWell_Dry = AddNonRRLWellGeneral("NFWell_Dry_WT_" + (i), GetFacilityId("NFWELL_", (i + 21)), WellTypeId.NF, WellFluidType.DryGas, WellFluidPhase.None, "1", assets.ElementAt(0).Id);
                    AddNonRRLModelFile(NFWell_Dry, modelFileName_Drygas, options_Drygas.CalibrationMethod, options_Drygas.OptionalUpdate);
                    NFW_Dry.Add(NFWell_Dry);
                    AddWellSettingWithDoubleValues(NFWell_Dry.Id, "Min L Factor Acceptance Limit", 0.1);
                    AddWellSettingWithDoubleValues(NFWell_Dry.Id, "Max L Factor Acceptance Limit", 2.0);
                    UpdateSystemSettings(NFWell_Dry, new string[] { SettingServiceStringConstants.CONDENSATE_GAS_RATIO_MAX_AL, SettingServiceStringConstants.CONDENSATE_GAS_RATIO_MIN_AL }, new double[] { 600, 0 });
                    UpdateSystemSettings(NFWell_Dry, new string[] { SettingServiceStringConstants.WATER_GAS_RATIO_MIN_AL, SettingServiceStringConstants.WATER_GAS_RATIO_MAX_AL }, new double[] { 100, 1000 });

                    #region Adding Valid WellTest
                    WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(NFWell_Dry.Id.ToString()).Units;
                    WellTestDTO testData = new WellTestDTO
                    {
                        WellId = NFWell_Dry.Id,
                        SPTCodeDescription = "RepresentativeTest",
                        //CalibrationMethod = options.CalibrationMethod,
                        WellTestType = WellTestType.WellTest,
                        AverageTubingPressure = 1000,
                        AverageTubingTemperature = 100,
                        Gas = 2000,
                        Water = (decimal)555.1,
                        ChokeSize = 32,
                        FlowLinePressure = 600,
                        SeparatorPressure = 600,
                        TestDuration = 12,
                        SampleDate = DateTime.Today.AddDays(-5).ToUniversalTime()
                    };
                    WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testData));
                    WellTestDTO[] allTunedAgainTests = WellTestDataService.GetAllValidWellTestByWellId(NFWell_Dry.Id.ToString());
                    WellTestDTO latestTestData_NF = WellTestDataService.GetLatestWellTestDataByWellId(NFWell_Dry.Id.ToString());

                    Assert.AreEqual("TUNING_SUCCEEDED", latestTestData_NF.Status.ToString(), "Well Test Status is not Success");
                    #endregion  Adding Valid WellTest
                }
                //Gas Injection
                List<WellDTO> GASInj = new List<WellDTO>();
                for (int i = 1; i < 11; i++)
                {
                    WellDTO GasInjection = AddNonRRLWellGeneral("GASINJWELL_" + i, GetFacilityId("GASINJWELL_", (i + 1)), WellTypeId.GInj, WellFluidType.None, WellFluidPhase.None, "1", assets.ElementAt(0).Id);
                    AddModelFile(GasInjection.CommissionDate.Value, WellTypeId.GInj, GasInjection.Id, CalibrationMethodId.ReservoirPressure);
                    AddWellSettingWithDoubleValues(GasInjection.Id, "Min L Factor Acceptance Limit", 0.1);
                    AddWellSettingWithDoubleValues(GasInjection.Id, "Max L Factor Acceptance Limit", 2.0);
                    UpdateSystemSettings(GasInjection, new string[] { SettingServiceStringConstants.RESERVOIR_PRESSURE_MAX_AL, SettingServiceStringConstants.RESERVOIR_PRESSURE_MIN_AL }, new double[] { 6000, 0 });
                }
                Trace.WriteLine("Wells are created successfully and mapped with Assets");

                #region create Reservoir
                ReservoirAndUnitsDTO R1A1 = AddReservoirToAsset("R1A1", assets.ElementAt(0).Id);
                Assert.IsNotNull(R1A1);
                #endregion create Reservoir

                #region getReservoir IDs
                ReservoirArrayAndUnitsDTO ReservoirIDsOfAsset1 = WellAllocationService.GetReservoirsByAssetId(assets.ElementAt(0).Id.ToString());
                Assert.IsNotNull(ReservoirIDsOfAsset1);
                var R1A1_ID = ReservoirIDsOfAsset1.Values.ElementAt(0).Id;
                #endregion getReservoir IDs

                #region create Zone 
                ZoneAndUnitsDTO Z1R1A1 = AddZoneToReservoir("Z1R1A1", R1A1_ID);
                Assert.IsNotNull(Z1R1A1);
                #endregion  create Zone 

                #region getZone IDs
                ZoneArrayAndUnitsDTO ZoneIDsOfR1A1 = WellAllocationService.GetZonesByReservoirId(R1A1_ID.ToString());
                Assert.IsNotNull(ZoneIDsOfR1A1);
                #endregion getZone IDs

                #region AddWellToZone
                var allWells = WellService.GetAllWells().ToList();
                for (int i = 0; i < allWells.Count(); i++)
                {
                    WellToZoneDTO W1Z1R1A1 = AddWellToZone(allWells[i].Id, ZoneIDsOfR1A1.Values.ElementAt(0).Id);
                    Assert.IsNotNull(W1Z1R1A1);
                }
                #endregion AddWellToZone

                #region create network scenario
                SNScenarioDTO scenario = ScenarioConfiguration(assets.ElementAt(0).Id);
                Assert.IsNotNull(scenario);
                #endregion create network scenario

                #region Set Well Allowable - Production
                string startDateProduction = DTOExtensions.ToISO8601(DateTime.Now.AddDays(-30).ToUniversalTime());
                string endDateProduction = DTOExtensions.ToISO8601(DateTime.Now.AddDays(-20).ToUniversalTime());

                //Create Snapshot
                snapshotProduction = WellAllocationService.CreateWellAllowableSnapshot("Snapshot1", WellAllowableSnapshotType.Sensitivity.ToString(), assets.ElementAt(0).Id.ToString(), startDateProduction, endDateProduction, WellTypeCategory.Production.ToString(), WellAllowablePhase.Gas.ToString(), scenario.Id.ToString());
                Assert.IsNotNull(snapshotProduction);
                _setWellAlowableToRemove.Add(snapshotInjection);

                SWAAssetBasedRequestDTO sWAAssetBasedRequestDTO6 = new SWAAssetBasedRequestDTO() { AssetId = assets.ElementAt(0).Id, WellCategory = WellTypeCategory.Production, SnapshotId = snapshotProduction.Id, FluidPhase = WellAllowablePhase.Gas };

                SWAAssetBasedDTO AssetBasedDTO = WellAllocationService.GetWellAllowableAssetBasedSnapShot(sWAAssetBasedRequestDTO6);

                //Getting Rate table as per Snapshot
                SWAWellBasedDTO wellbasedProductionDTO = new SWAWellBasedDTO();
                wellbasedProductionDTO.SnapshotId = snapshotProduction.Id;
                wellbasedProductionDTO.AssetId = assets.ElementAt(0).Id;
                wellbasedProductionDTO.WellCategory = WellTypeCategory.Production;
                wellbasedProductionDTO.FluidPhase = WellAllowablePhase.Gas;

                SWARateArrayAndUnitsDTO ratesForAsset1Production = WellAllocationService.GetWellAllowableRatesForSnapshot(wellbasedProductionDTO);
                Assert.IsNotNull(ratesForAsset1Production);

                for (int i = 0; i < ratesForAsset1Production.Values.Length; i++)
                {
                    ratesForAsset1Production.Values[i].ReservoirPressureBound = (decimal)5334.46 + i;
                    ratesForAsset1Production.Values[i].ReservoirPressureTolerance = (decimal)528.49 + i;
                    ratesForAsset1Production.Values[i].WellheadPressureBound = 350 + i;
                    ratesForAsset1Production.Values[i].PerforationPressureChangeBound = (decimal)960.17 + i;
                    ratesForAsset1Production.Values[i].MaximumRate = 3000 + i;
                    ratesForAsset1Production.Values[i].MinimumRate = 3000;
                    ratesForAsset1Production.Values[i].WellAvailability = (decimal)0.86 - (decimal)(0.01 * i);
                    ratesForAsset1Production.Values[i].InAnalysis = true;
                }
                wellbasedProductionDTO.Rates = ratesForAsset1Production.Values;
                //Update Rate table snap shot
                var updaterateAssetProduction = WellAllocationService.UpdateWellAllowableRatesForSnapshot(wellbasedProductionDTO);
                Assert.IsNotNull(updaterateAssetProduction);

                //Verifying Run Status when no calculation done
                SWAAssetBasedDTO RunStatusBeforeCalculation = WellAllocationService.GetWellAllowableAssetBasedSnapShot(sWAAssetBasedRequestDTO6);
                Assert.IsNotNull(RunStatusBeforeCalculation);
                Assert.IsNotNull(RunStatusBeforeCalculation.SWAAssetSettingsDto.Value.RunStatus.ToString());
                Assert.AreEqual("NoCalculation", RunStatusBeforeCalculation.SWAAssetSettingsDto.Value.RunStatus.ToString());

                //Verifying calculation when Minimum Rate is greater than Allowable Rate
                #region CalculatePreTechnicalandTechnicalRates
                var VerifyTechnicalRateError = WellAllocationService.CalculateWellAllowableTechnicalRate(wellbasedProductionDTO);
                Assert.IsNotNull(VerifyTechnicalRateError, "Added Calculated Pre technical rates and Technical rates record failed");
                #endregion CalculatePreTechnicalandTechnicalRates
                Assert.AreEqual("WellTechnicalRateErrors", VerifyTechnicalRateError.Values[0].Status.ToString());

                #region asset bound
                SWAAssetBasedRequestDTO sWAAssetBasedRequestDTO = new SWAAssetBasedRequestDTO() { AssetId = assets.ElementAt(0).Id, WellCategory = WellTypeCategory.Production, SnapshotId = snapshotProduction.Id, FluidPhase = WellAllowablePhase.Gas };

                // Getting well allowable Asset based on snapshot
                SWAAssetBasedDTO AssetBasedDTO_1 = WellAllocationService.GetWellAllowableAssetBasedSnapShot(sWAAssetBasedRequestDTO);
                AssetBasedDTO_1.SWAAssetSettingsDto.Value.SystemEfficiency = (decimal?)0.95;
                AssetBasedDTO_1.SWAAssetSettingsDto.Value.UnplannedFactor = (decimal?)0.22;
                AssetBasedDTO_1.SWAAssetSettingsDto.Value.FacilityAvailability = (decimal?)0.75;

                //Adding TargetExportCondition,TargetFieldCondition & Capacity values
                for (int i = 0; i < AssetBasedDTO_1.SWAAssetZonesDto.Values.Length; i++)
                {
                    AssetBasedDTO_1.SWAAssetZonesDto.Values[i].TargetExportCondition = (decimal?)3400.55 + (i * 10);
                    //AssetBasedDTO.SWAAssetZonesDto.Values[i].TargetFieldCondition = (decimal?) 3500 + (i*10);
                }

                for (int i = 0; i < AssetBasedDTO_1.SWAAssetTrunkLinesDto.Values.Length; i++)
                {
                    AssetBasedDTO_1.SWAAssetTrunkLinesDto.Values[i].Capacity = (decimal?)3600.45 + (i * 10);
                }
                SWAAssetSettingsDTO swasettings = new SWAAssetSettingsDTO();
                swasettings = AssetBasedDTO_1.SWAAssetSettingsDto.Value;

                AssetBasedDTO_1.SaveAssetingParameter = swasettings;
                AssetBasedDTO_1.SWARequestDTO = sWAAssetBasedRequestDTO;
                AssetBasedDTO_1.SavedAssetZoneDTOs = AssetBasedDTO_1.SWAAssetZonesDto.Values;
                AssetBasedDTO_1.SavedTrunkLineDTOs = AssetBasedDTO_1.SWAAssetTrunkLinesDto.Values;
                //Saving values in db
                Assert.IsNotNull(AssetBasedDTO_1.SaveAssetingParameter);
                WellAllocationService.SaveWellAllowableAssetBasedSnapShot(AssetBasedDTO_1);
                #endregion asset bound
                #endregion Set Well Allowable - Production

                //Changing Minimum Rate value and Verifying Run Status
                for (int i = 0; i < ratesForAsset1Production.Values.Length; i++)
                {
                    ratesForAsset1Production.Values[i].ReservoirPressureBound = (decimal)5334.46 + i;
                    ratesForAsset1Production.Values[i].ReservoirPressureTolerance = (decimal)528.49 + i;
                    ratesForAsset1Production.Values[i].WellheadPressureBound = 350 + i;
                    ratesForAsset1Production.Values[i].PerforationPressureChangeBound = (decimal)960.17 + i;
                    ratesForAsset1Production.Values[i].MaximumRate = 3000 + i;
                    ratesForAsset1Production.Values[i].MinimumRate = 0;
                    ratesForAsset1Production.Values[i].WellAvailability = (decimal)0.86 - (decimal)(0.01 * i);
                    ratesForAsset1Production.Values[i].InAnalysis = true;
                }
                wellbasedProductionDTO.Rates = ratesForAsset1Production.Values;
                //Update Rate table snap shot
                var updaterateAssetProduction_1 = WellAllocationService.UpdateWellAllowableRatesForSnapshot(wellbasedProductionDTO);
                Assert.IsNotNull(updaterateAssetProduction_1);

                #region CalculatePreTechnicalandTechnicalRates
                var VerifyingCompletedStatus = WellAllocationService.CalculateWellAllowableTechnicalRate(wellbasedProductionDTO);
                Assert.IsNotNull(VerifyingCompletedStatus, "Added Calculated Pre technical rates and Technical rates record failed");
                #endregion CalculatePreTechnicalandTechnicalRates
                Assert.AreEqual("WellAllowableRateNoErrors", VerifyingCompletedStatus.Values[0].Status.ToString());

                WellAllowableRunStatus RunStatusVerification1 = WellAllocationService.DoSetWellAllowableCalculations();
                Assert.IsNotNull(RunStatusVerification1);

                //Commented backpressure check test as it's failing in clean up due to delete surface network scenario is not implemented yet

                //#region Enable Backpressure check
                //SWAAssetBasedDTO RunStatusAftereCalculation = WellAllocationService.GetWellAllowableAssetBasedSnapShot(sWAAssetBasedRequestDTO6);
                //AssetBasedDTO_1.SWAAssetSettingsDto.Value.EnableBackpressureCheck = true;
                //var UpdateAssetSetting = WellAllocationService.UpdateWellAllowableAssetSettings(swasettings);
                //Assert.IsNotNull(UpdateAssetSetting);
                //Assert.IsNotNull(RunStatusAftereCalculation);
                //#endregion Enable Backpressure check

                //#region Check Calculation when BackPressure check is Enabled
                //var VerifyBackPressureCheckError = WellAllocationService.CalculateWellAllowableTechnicalRate(wellbasedProductionDTO);
                //Assert.IsNotNull(VerifyBackPressureCheckError, "Added Calculated Pre technical rates and Technical rates record failed");
                //#endregion Check Calculation when BackPressure check is Enabled

                ////RunAnalysisTaskScheduler("-runScenarioScheduler");
                //Assert.AreEqual("BackpressureCheckInProgress", VerifyBackPressureCheckError.Values[0].Status.ToString());

                //SWARateArrayAndUnitsDTO ratesForAsset1Production_1 = WellAllocationService.GetWellAllowableRatesForSnapshot(wellbasedProductionDTO);
                //Assert.IsNotNull(ratesForAsset1Production_1);
                //#region Disable Backpressure check
                //SWAAssetBasedDTO RunStatusAftereCalculation_DBP = WellAllocationService.GetWellAllowableAssetBasedSnapShot(sWAAssetBasedRequestDTO6);
                //AssetBasedDTO_1.SWAAssetSettingsDto.Value.EnableBackpressureCheck = false;
                //var UpdateAssetSetting_1 = WellAllocationService.UpdateWellAllowableAssetSettings(swasettings);
                //Assert.IsNotNull(UpdateAssetSetting_1);
                //Assert.IsNotNull(RunStatusAftereCalculation_DBP);
                //#endregion Enable Backpressure check

                //#region Check Calculation when BackPressure check is Disabled
                //var VerifyBackPressureCheckNoErrors = WellAllocationService.CalculateWellAllowableTechnicalRate(wellbasedProductionDTO);
                //Assert.IsNotNull(VerifyBackPressureCheckNoErrors, "Added Calculated Pre technical rates and Technical rates record failed");
                //Assert.AreEqual("WellAllowableRateNoErrors", VerifyBackPressureCheckNoErrors.Values[0].Status.ToString());
                //#endregion Check Calculation when BackPressure check is Disabled

                #region Set Well Allowable - Injection
                string startDateInjection = DTOExtensions.ToISO8601(DateTime.Now.AddDays(-30).ToUniversalTime());
                string endDateInjection = DTOExtensions.ToISO8601(DateTime.Now.AddDays(-20).ToUniversalTime());
                string injectionSnapshotName = "Injection SnapShot 1";
                string injetionSnapshotType = WellAllowableSnapshotType.Target.ToString();

                //Create Snapshot
                snapshotInjection = WellAllocationService.CreateWellAllowableSnapshot(injectionSnapshotName, injetionSnapshotType, assets.ElementAt(0).Id.ToString(), startDateInjection, endDateInjection, WellTypeCategory.Injection.ToString(), WellAllowablePhase.Gas.ToString(), scenario.Id.ToString());
                Assert.IsNotNull(snapshotInjection);
                _setWellAlowableToRemove.Add(snapshotInjection);

                //Getting Ratetables as per Snapshot
                SWAWellBasedDTO wellbasedInjectionDTO = new SWAWellBasedDTO();
                wellbasedInjectionDTO.SnapshotId = snapshotInjection.Id;
                wellbasedInjectionDTO.AssetId = assets.ElementAt(0).Id;
                wellbasedInjectionDTO.WellCategory = WellTypeCategory.Injection;
                wellbasedInjectionDTO.FluidPhase = WellAllowablePhase.Gas;

                SWARateArrayAndUnitsDTO ratesForAsset1Injection = WellAllocationService.GetWellAllowableRatesForSnapshot(wellbasedInjectionDTO);
                Assert.IsNotNull(ratesForAsset1Injection);
                //Verifying Technical rate errors when Minimum rate is greater than Allowable Rate
                for (var i = 0; i < ratesForAsset1Injection.Values.Length; i++)
                {
                    ratesForAsset1Injection.Values[i].WellheadPressureBound = (decimal)3514.7 + i;
                    ratesForAsset1Injection.Values[i].PerforationPressureChangeBound = 1000 + i;
                    ratesForAsset1Injection.Values[i].MaximumRate = 3000 + i;
                    ratesForAsset1Injection.Values[i].MinimumRate = 3000 + i;
                    ratesForAsset1Injection.Values[i].WellAvailability = (decimal)(0.95) - (decimal)(i * 0.01);
                    ratesForAsset1Injection.Values[i].InAnalysis = true;
                }

                //Update Rate table snap shot
                wellbasedInjectionDTO.Rates = ratesForAsset1Injection.Values;
                var updaterateAssetInjection = WellAllocationService.UpdateWellAllowableRatesForSnapshot(wellbasedInjectionDTO);
                Assert.IsNotNull(updaterateAssetInjection);
                #endregion Set Well Allowable - Injection

                #region CalculatetechnicalRateError
                var VerifyTechnicalRateError_inj = WellAllocationService.CalculateWellAllowableTechnicalRate(wellbasedInjectionDTO);
                Assert.IsNotNull(VerifyTechnicalRateError_inj, "Added Calculated Pre technical rates and Technical rates record failed");
                #endregion CalculatetechnicalRateError
                //Verifying Run status when error is present due to  

                Assert.AreEqual("WellTechnicalRateErrors", VerifyTechnicalRateError_inj.Values[0].Status.ToString(), "Mismatched Run status");

                #region asset bound Injection
                SWAAssetBasedRequestDTO sWAAssetBasedRequestDTO_1 = new SWAAssetBasedRequestDTO() { AssetId = assets.ElementAt(0).Id, WellCategory = WellTypeCategory.Injection, SnapshotId = snapshotInjection.Id, FluidPhase = WellAllowablePhase.Gas };
                // Getting well allowable Asset based on snapshot
                SWAAssetBasedDTO AssetBasedDTO_2 = WellAllocationService.GetWellAllowableAssetBasedSnapShot(sWAAssetBasedRequestDTO_1);
                AssetBasedDTO_2.SWAAssetSettingsDto.Value.SystemEfficiency = 1;
                AssetBasedDTO_2.SWAAssetSettingsDto.Value.UnplannedFactor = 1;
                AssetBasedDTO_2.SWAAssetSettingsDto.Value.FacilityAvailability = 1;

                //Adding TargetExportCondition,TargetFieldCondition & Capacity values
                for (int i = 0; i < AssetBasedDTO_2.SWAAssetZonesDto.Values.Length; i++)
                {
                    AssetBasedDTO_2.SWAAssetZonesDto.Values[i].TargetExportCondition = 1000 + (i * 10);
                    //AssetBasedDTO.SWAAssetZonesDto.Values[i].TargetFieldCondition = (decimal?) 3500 + (i*10);
                }

                for (int i = 0; i < AssetBasedDTO_2.SWAAssetTrunkLinesDto.Values.Length; i++)
                {
                    AssetBasedDTO_2.SWAAssetTrunkLinesDto.Values[i].Capacity = (decimal?)4600.45 + (i * 10);
                }
                SWAAssetSettingsDTO swasettings_1 = new SWAAssetSettingsDTO();
                swasettings_1 = AssetBasedDTO_2.SWAAssetSettingsDto.Value;

                AssetBasedDTO_2.SaveAssetingParameter = swasettings_1;
                AssetBasedDTO_2.SWARequestDTO = sWAAssetBasedRequestDTO_1;
                AssetBasedDTO_2.SavedAssetZoneDTOs = AssetBasedDTO_2.SWAAssetZonesDto.Values;
                AssetBasedDTO_2.SavedTrunkLineDTOs = AssetBasedDTO_2.SWAAssetTrunkLinesDto.Values;
                //Saving values in db
                Assert.IsNotNull(AssetBasedDTO_1.SaveAssetingParameter);
                WellAllocationService.SaveWellAllowableAssetBasedSnapShot(AssetBasedDTO_2);
                #endregion asset bound Injection

                for (var i = 0; i < ratesForAsset1Injection.Values.Length; i++)
                {
                    ratesForAsset1Injection.Values[i].WellheadPressureBound = (decimal)3514.7 + i;
                    ratesForAsset1Injection.Values[i].PerforationPressureChangeBound = 1000 + i;
                    ratesForAsset1Injection.Values[i].MaximumRate = 3000 + i;
                    ratesForAsset1Injection.Values[i].MinimumRate = 80 + i;
                    ratesForAsset1Injection.Values[i].WellAvailability = (decimal)(0.95) - (decimal)(i * 0.01);
                    ratesForAsset1Injection.Values[i].InAnalysis = true;
                }
                //Update Rate table snap shot
                wellbasedInjectionDTO.Rates = ratesForAsset1Injection.Values;
                var updaterateAssetInjection_1 = WellAllocationService.UpdateWellAllowableRatesForSnapshot(wellbasedInjectionDTO);
                Assert.IsNotNull(updaterateAssetInjection_1);

                #region CalculatePreTechnicalandTechnicalRates
                var VerifyWellAllowableRate_inj = WellAllocationService.CalculateWellAllowableTechnicalRate(wellbasedInjectionDTO);
                Assert.IsNotNull(VerifyWellAllowableRate_inj, "Added Calculated Pre technical rates and Technical rates record failed");
                #endregion CalculatePreTechnicalandTechnicalRates
                Assert.AreEqual("WellAllowableRateNoErrors", VerifyWellAllowableRate_inj.Values[0].Status.ToString());

                //#region Enable Backpressure check for Injection well
                //SWAAssetBasedDTO RunStatusAftereCalculation_2 = WellAllocationService.GetWellAllowableAssetBasedSnapShot(sWAAssetBasedRequestDTO_1);
                //AssetBasedDTO_2.SWAAssetSettingsDto.Value.EnableBackpressureCheck = true;
                //var UpdateAssetSetting_2 = WellAllocationService.UpdateWellAllowableAssetSettings(swasettings_1);
                //// Assert.IsNotNull(UpdateAssetSetting_1);
                //Assert.IsNotNull(RunStatusAftereCalculation_2);
                //#endregion Enable Backpressure check for Injection well

                //#region Check Calculation when BackPressure check is Enabled
                //var VerifyBackPressureCheckError_inj = WellAllocationService.CalculateWellAllowableTechnicalRate(wellbasedInjectionDTO);
                //Assert.IsNotNull(VerifyBackPressureCheckError_inj, "Added Calculated Pre technical rates and Technical rates record failed");
                //#endregion Check Calculation when BackPressure check is Enabled

                //Assert.AreEqual("BackpressureCheckInProgress", VerifyBackPressureCheckError_inj.Values[0].Status.ToString());

                //#region Disable Backpressure check 
                //SWAAssetBasedDTO RunStatusAftereCalculation_3 = WellAllocationService.GetWellAllowableAssetBasedSnapShot(sWAAssetBasedRequestDTO_1);
                //AssetBasedDTO_2.SWAAssetSettingsDto.Value.EnableBackpressureCheck = false;
                //var UpdateAssetSetting_3 = WellAllocationService.UpdateWellAllowableAssetSettings(swasettings_1);
                ////Assert.IsNotNull(UpdateAssetSetting_1);
                //Assert.IsNotNull(RunStatusAftereCalculation_3);
                //#endregion Disable Backpressure check

                //#region Check Calculation when BackPressure check is Disabled
                //var VerifyWellAllowableRateNoErrors_inj = WellAllocationService.CalculateWellAllowableTechnicalRate(wellbasedInjectionDTO);
                //Assert.IsNotNull(VerifyWellAllowableRateNoErrors_inj, "Added Calculated Pre technical rates and Technical rates record failed");
                //Assert.AreEqual("WellAllowableRateNoErrors", VerifyWellAllowableRateNoErrors_inj.Values[0].Status.ToString());
                //#endregion Check Calculation when BackPressure check is Disabled

            }
            finally
            {
                ChangeUnitSystem("US");
                ChangeUnitSystemUserSetting("US");

                WellAllocationService.DeleteWellAllowableSnapshot(snapshotProduction.Id.ToString());
                WellAllocationService.DeleteWellAllowableSnapshot(snapshotInjection.Id.ToString());
                WellAllocationService.DeleteHierarchyByAssetId(assets.ElementAt(0).Id.ToString());
                WellAllocationService.DeleteHierarchyByAssetId(assets.ElementAt(1).Id.ToString());
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
   }
}