using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Weatherford.POP.DTOs;

namespace Weatherford.POP.Server.IntegrationTests
{
    [TestClass]
    public class CatalogServiceTests : APIClientTestBase
    {
        private List<POPRRLManufacturerDTO> _rodManufacturers;

        [TestInitialize]
        public override void Init()
        {
            base.Init();
            _rodManufacturers = new List<POPRRLManufacturerDTO>();
        }

        [TestCleanup]
        public override void Cleanup()
        {
            base.Cleanup();
        }

        [TestCategory(TestCategories.CatalogServiceTests), TestMethod]
        public void GetAllRodManufacturers()
        {
            POPRRLManufacturerDTO[] rodManufacturers = CatalogService.GetRRLRodManufacturers();
            Assert.IsNotNull(rodManufacturers);
            Assert.AreNotEqual(0, rodManufacturers.Length);
            POPRRLManufacturerDTO mfr = rodManufacturers.FirstOrDefault(mfg => mfg.Manufacturer.Equals("Weatherford, Inc."));
            Assert.IsNotNull(mfr);
        }

        [TestCategory(TestCategories.CatalogServiceTests), TestMethod]
        public void GetAllRRLPumps()
        {
            SRPBarrelTypeDTO[] barrelTypes = CatalogService.GetSRPBarrelTypes();
            Assert.IsNotNull(barrelTypes);
            foreach (SRPBarrelTypeDTO barrelType in barrelTypes)
            {
                SRPBarrelTypeDTO bt = CatalogService.GetSRPBarrelTypeByPrimaryKey(barrelType.Id.ToString());
                Assert.IsNotNull(bt);
            }
            SRPSeatTypeDTO[] SeatTypes = CatalogService.GetSRPSeatTypes();
            Assert.IsNotNull(SeatTypes);
            foreach (SRPSeatTypeDTO seatType in SeatTypes)
            {
                SRPSeatTypeDTO st = CatalogService.GetSRPSRPSeatTypeByPrimaryKey(seatType.Id.ToString());
                Assert.IsNotNull(st);
            }
            SRPPumpBoreDTO[] pumpBores = CatalogService.GetSRPPumpBores();
            Assert.IsNotNull(pumpBores);
            foreach (SRPPumpBoreDTO pumpBore in pumpBores)
            {
                SRPPumpBoreDTO pb = CatalogService.GetSRPPumpBoreByPrimaryKey(pumpBore.Id.ToString());
                Assert.IsNotNull(pb);
            }
            SRPPumpTypeDTO[] pumpTypes = CatalogService.GetSRPPumpTypes();
            Assert.IsNotNull(pumpTypes);
            foreach (SRPPumpTypeDTO pumpType in pumpTypes)
            {
                SRPPumpTypeDTO pt = CatalogService.GetSRPPumpTypeByPrimaryKey(pumpType.Id.ToString());
                Assert.IsNotNull(pt);
            }
            SRPSeatLocationDTO[] locations = CatalogService.GetSRPSeatLocations();
            Assert.IsNotNull(locations);
            foreach (SRPSeatLocationDTO location in locations)
            {
                SRPSeatLocationDTO sl = CatalogService.GetSRPSeatLocationByPrimaryKey(location.Id.ToString());
                Assert.IsNotNull(sl);
            }
            SRPTubingSizeDTO[] tubingSizes = CatalogService.GetSRPTubeSizes();
            Assert.IsNotNull(tubingSizes);
            foreach (SRPTubingSizeDTO tubingSize in tubingSizes)
            {
                SRPTubingSizeDTO ts = CatalogService.GetSRPTubeSizeByPrimaryKey(tubingSize.Id.ToString());
                Assert.IsNotNull(ts);
            }

            POPRRLSurfaceUnitDTO[] surfaceUnits = CatalogService.GetSurfaceUnits();
            Assert.IsNotNull(surfaceUnits);
            POPRRLManufacturerDTO[] manufacturers = CatalogService.GetSurfaceUnitManufacturers();
            Assert.IsNotNull(manufacturers);
            foreach (POPRRLManufacturerDTO mfg in manufacturers)
            {
                POPRRLSurfaceUnitDTO[] surfaceUnitsMfg = CatalogService.GetSurfaceUnitsByManufacturer(mfg.PrimaryKey.ToString());
                Assert.IsNotNull(surfaceUnitsMfg);
            }
            POPRRLSurfaceUnitDTO surfaceUnit = CatalogService.GetSurfaceUnitByPrimaryKey(surfaceUnits[0].PK_rrlSurfaceBase.ToString());
            Assert.IsNotNull(surfaceUnit);
        }

        [TestCategory(TestCategories.CatalogServiceTests), TestMethod]
        public void GetRodGradesRodSizesGetRRLRod()
        {
            POPRRLManufacturerDTO[] RodManufacturers = CatalogService.GetRRLRodManufacturers();
            POPRRLManufacturerDTO mfr = RodManufacturers.FirstOrDefault(mfg => mfg.Manufacturer.Equals("Weatherford, Inc."));
            Assert.IsNotNull(mfr);

            string[] rodGrades = CatalogService.GetRRLRodGrades(mfr.Manufacturer);
            Assert.IsNotNull(rodGrades);
            Assert.AreNotEqual(0, rodGrades.Length);

            foreach (string grade in rodGrades)
            {
                double[] rodSizes = CatalogService.GetRRLRodSizes(mfr.Manufacturer, grade);
                Assert.IsNotNull(rodSizes);
                Assert.AreNotEqual(0, rodSizes.Length);
                foreach (double size in rodSizes)
                {
                    POPRRLRodDTO rod = CatalogService.GetRRLRod(mfr.Manufacturer, grade, size.ToString());
                    Assert.IsNotNull(rod);
                    Assert.AreNotEqual(0, rod.Id);
                    Assert.AreEqual(grade, rod.Grade);
                    Assert.AreEqual(size, rod.Diameter);
                    Assert.AreEqual(mfr.Manufacturer, rod.Manufacturer);
                    Assert.AreNotEqual(0.0, rod.Area);
                    Assert.AreNotEqual(0.0, rod.UnitWeight);

                    int rodPrimaryKey = CatalogService.GetRRLRodPrimaryKey(mfr.Manufacturer, grade, size.ToString());
                    Assert.AreEqual(rod.Id, rodPrimaryKey);

                    int rrlManufacturer_PK = CatalogService.GetPrimaryKeyFromRRLManufacturer(mfr.Manufacturer);
                    string rrlManufacturer = CatalogService.GetRRLManufacturerFromPrimaryKey(rrlManufacturer_PK.ToString());
                    Assert.AreEqual(mfr.Manufacturer, rrlManufacturer);
                }
            }
        }

        [TestCategory(TestCategories.CatalogServiceTests), TestMethod]
        public void GetCranksByPumpingUnitPK()
        {
            POPRRLCranksDTO[] cranks = CatalogService.GetCranksByPumpingUnitPK("Lufkin", "C", "C-114-109-74 L LUFKIN C114-109-74");
            Assert.IsNotNull(cranks, "Returned crank array should not be null.");
            Assert.AreNotEqual(0, cranks.Length, "Returned crank array should not be empty.");
            foreach (POPRRLCranksDTO crank in cranks)
            {
                Assert.IsTrue(crank.PK_rrlCrank > 0, $"Primary key for crank {crank.CrankId} should be non-zero.");
                Assert.IsFalse(string.IsNullOrWhiteSpace(crank.CrankId), $"Crank id should not be empty for crank with primary key {crank.PK_rrlCrank}.");
                if (crank.CrankId == "N/A")
                {
                    Assert.AreEqual(0, crank.CounterBalanceTorque, $"Counter-balance torque should be zero for crank {crank.CrankId}.");
                }
                else
                {
                    Assert.AreNotEqual(0, crank.CounterBalanceTorque, $"Counter-balance torque should be non-zero for crank {crank.CrankId}.");
                }
            }
        }

        [TestCategory(TestCategories.CatalogServiceTests), TestMethod]
        public void GetCrankWeightsByCrankId()
        {
            string pumpingUnitDesc = "C-114-109-74 L LUFKIN C114-109-74";
            var cranks = CatalogService.GetCranksByPumpingUnitPK("Lufkin", "C", pumpingUnitDesc);
            Assert.IsNotNull(cranks);
            Assert.AreNotEqual(0, cranks.Length, "No cranks returned for pumping unit '" + pumpingUnitDesc + "'.");
            var crank = cranks.FirstOrDefault(c => c.CrankId.Equals("9411OC"));
            Assert.IsNotNull(crank);
            POPRRLCranksWeightsDTO weights = CatalogService.GetCrankWeightsByCrankId(crank.CrankId);
            Assert.IsNotNull(weights);

            Assert.AreEqual(crank.PK_rrlCrank, weights.FK_Crank);

            Assert.AreNotEqual(0, weights.Auxiliary.Count);
            Assert.AreNotEqual(0, weights.Primary.Count);

            Assert.AreEqual(weights.Primary.Count, weights.PrimaryIdentifier.Count);
            Assert.AreEqual(weights.Primary.Count, weights.DistanceM.Count);
            Assert.AreEqual(weights.Primary.Count, weights.DistanceT.Count);

            Assert.AreEqual(weights.Auxiliary.Count, weights.AuxiliaryIdentifier.Count);

            for (int ii = 0; ii < weights.Primary.Count; ii++)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(weights.PrimaryIdentifier[ii]));
                Assert.IsFalse(string.Equals("N/A", weights.PrimaryIdentifier[ii], StringComparison.CurrentCultureIgnoreCase));
                if (string.Equals("None", weights.PrimaryIdentifier[ii]))
                {
                    Assert.AreEqual(0, ii, "None weight should be first in the list and only appear once.");
                }
            }

            for (int ii = 0; ii < weights.Auxiliary.Count; ii++)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(weights.AuxiliaryIdentifier[ii]));
                Assert.IsFalse(string.Equals("N/A", weights.AuxiliaryIdentifier[ii], StringComparison.CurrentCultureIgnoreCase));
                if (string.Equals("None", weights.AuxiliaryIdentifier[ii]))
                {
                    Assert.AreEqual(0, ii, "None weight should be first in the list and only appear once.");
                }
            }
        }

        [TestCategory(TestCategories.CatalogServiceTests), TestMethod]
        public void GetAllMotorSizes()
        {
            RRLMotorSizeDTO[] motors = CatalogService.GetAllMotorSizes();
            Assert.IsNotNull(motors);
            Assert.AreNotEqual(0, motors.Length, "No motor sizes returned.");
            foreach (RRLMotorSizeDTO motor in motors)
            {
                Assert.IsTrue(motor.Id > 0);
                Assert.IsTrue(motor.SizeInHP > 0);
            }
        }

        [TestCategory(TestCategories.CatalogServiceTests), TestMethod]
        public void GetAllMotorSlips()
        {
            RRLMotorSlipDTO[] motors = CatalogService.GetAllMotorSlips();
            Assert.IsNotNull(motors);
            Assert.AreNotEqual(0, motors.Length, "No motor slips returned.");
            foreach (RRLMotorSlipDTO motor in motors)
            {
                Assert.IsTrue(motor.Id > 0);
                Assert.IsTrue(motor.Rating > 0);
            }
        }

        [TestCategory(TestCategories.CatalogServiceTests), TestMethod]
        public void GetAllMotorTypes()
        {
            RRLMotorTypeDTO[] motors = CatalogService.GetAllMotorTypes();
            Assert.IsNotNull(motors);
            Assert.AreNotEqual(0, motors.Length, "No motor types returned.");
            foreach (RRLMotorTypeDTO motor in motors)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(motor.Name));
            }
        }

        [TestCategory(TestCategories.CatalogServiceTests), TestMethod]
        public void GetAllPumpingUnitManufacturersGetPumpingUnitManufacturerInfoByName()
        {
            PumpingUnitManufacturerDTO[] manufacturer = CatalogService.GetAllPumpingUnitManufacturers();
            Assert.IsNotNull(manufacturer);
            //Manufacturers/All
            PumpingUnitManufacturerDTO pum = manufacturer.FirstOrDefault(pumt => pumt.Name.Equals("Weatherford, Inc."));
            Assert.IsNotNull(pum);
            PumpingUnitManufacturerDTO type = CatalogService.GetPumpingUnitManufacturerInfoByName(pum.Name);
            Assert.IsNotNull(type);
            //Manufacturer/{name}
        }

        [TestCategory(TestCategories.CatalogServiceTests), TestMethod]
        public void GetPumpingUnitByManufacturerTypeAndDescription()
        {
            // Set this to true to go though all the things.
            bool iterateThoughAll = s_isRunningInATS;
            PumpingUnitManufacturerDTO[] manufacturer = CatalogService.GetAllPumpingUnitManufacturers();
            var failureList = new List<string>();

            for (int ii = 0; ii < manufacturer.Length; ii++)
            {
                string name = manufacturer[ii].Name;
                PumpingUnitTypeDTO[] pumpingunitType = CatalogService.GetPumpingUnitTypesByManufacturer(manufacturer[ii].Name);
                Assert.IsNotNull(pumpingunitType);
                int pumpingunitTypeMax = iterateThoughAll ? pumpingunitType.Length : Math.Min(1, pumpingunitType.Length);
                for (int jj = 0; jj < pumpingunitTypeMax; jj++)
                {
                    var type = pumpingunitType[jj].AbbreviatedName;
                    Assert.IsNotNull(type);
                    PumpingUnitDTO[] PUMType = CatalogService.GetPumpingUnitsByManufacturerAndType(manufacturer[ii].Name, type);
                    Assert.IsNotNull(PUMType);
                    int PUMTypeMax = iterateThoughAll ? PUMType.Length : Math.Min(1, PUMType.Length);
                    for (int kk = 0; kk < PUMTypeMax; kk++)
                    {
                        var description = PUMType[kk].Description;
                        Assert.IsNotNull(description);
                        PumpingUnitDTO PUMTypeAndDescription = null;
                        try
                        {
                            PUMTypeAndDescription = CatalogService.GetPumpingUnitByManufacturerTypeAndDescription(manufacturer[ii].Name, type, description);
                        }
                        catch (Exception) { } // Yum!
                        if (PUMTypeAndDescription == null)
                        {
                            string oneFailure = $"{manufacturer[ii].Name} - {type} - {description}";
                            failureList.Add(oneFailure);
                            System.Diagnostics.Trace.WriteLine("Failed to get pumping unit: " + oneFailure);
                        }
                    }
                }
            }
            Assert.AreEqual(0, failureList.Count, "Failed to get pumping unit for: [" + string.Join(" | ", failureList) + "]");
        }
    }
}
