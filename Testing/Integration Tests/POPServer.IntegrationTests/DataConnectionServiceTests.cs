using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Weatherford.POP.DTOs;

namespace Weatherford.POP.Server.IntegrationTests
{
    [TestClass]
    public class DataConnectionServiceTests : APIClientTestBase
    {
        [TestInitialize]
        public override void Init()
        {
            base.Init();
        }

        [TestCleanup]
        public override void Cleanup()
        {
            base.Cleanup();
        }

        [TestCategory(TestCategories.DataConnectionServiceTests), TestMethod]
        public void AddDcGetAllDcGetAllDomainsRemoveDc()
        {
            var dc1 = new DataConnectionDTO() { ProductionDomain = "12345", Site = "CYG_TEST", Service = s_cvsService };
            var dc2 = new DataConnectionDTO() { ProductionDomain = "54321", Site = "CYG_SITE", Service = s_cvsService };

            long dc1Id = DataConnectionService.AddDataConnection(dc1);
            Assert.AreNotEqual(0, dc1Id, "Data connection id returned on add should not be zero.");
            DataConnectionDTO[] dataConnections = DataConnectionService.GetAllDataConnections();
            dc1 = dataConnections.FirstOrDefault(dc => dc.Id == dc1Id);
            Assert.IsNotNull(dc1, "Failed to get first added data connection.");
            _dataConnectionsToRemove.Add(dc1);

            long dc2Id = DataConnectionService.AddDataConnection(dc2);
            Assert.AreNotEqual(0, dc2Id, "Data connection id returned on add should not be zero.");
            dataConnections = DataConnectionService.GetAllDataConnections();
            dc2 = dataConnections.FirstOrDefault(dc => dc.Id == dc2Id);
            Assert.IsNotNull(dc2, "Failed to get second added data connection.");
            _dataConnectionsToRemove.Add(dc2);

            ushort[] prodDomains = DataConnectionService.GetAllProductionDomains();
            Assert.IsNotNull(prodDomains, "Production domain array is null");
            Trace.WriteLine("Found domain(s): " + string.Join(", ", prodDomains));

            bool foundDC1Domain, foundDC2Domain;
            foundDC1Domain = foundDC2Domain = false;
            foreach (ushort domain in prodDomains)
            {
                if (domain.ToString() == dc1.ProductionDomain)
                {
                    foundDC1Domain = true;
                }
                if (domain.ToString() == dc2.ProductionDomain)
                {
                    foundDC2Domain = true;
                }
            }
            bool foundNeitherDomain = !foundDC1Domain && !foundDC2Domain;
            Assert.IsTrue(foundDC1Domain & foundDC2Domain, "Failed to find {0}{1}{2} domain{3}.",
                foundDC1Domain ? "" : "first",
                foundNeitherDomain ? " and " : "",
                foundDC2Domain ? "" : "second",
                foundNeitherDomain ? "s" : "");

            DataConnectionService.RemoveDataConnection(dc1Id.ToString());
            DataConnectionService.RemoveDataConnection(dc2Id.ToString());

            foundDC1Domain = foundDC2Domain = false;
            prodDomains = DataConnectionService.GetAllProductionDomains();
            if (prodDomains != null)
            {
                Trace.WriteLine("Found domain(s): " + string.Join(", ", prodDomains));
                foreach (ushort domain in prodDomains)
                {
                    if (domain.ToString() == dc1.ProductionDomain)
                    {
                        foundDC1Domain = true;
                    }
                    if (domain.ToString() == dc2.ProductionDomain)
                    {
                        foundDC2Domain = true;
                    }
                }
                foundNeitherDomain = !foundDC1Domain && !foundDC2Domain;
                bool foundBothDomains = foundDC1Domain && foundDC2Domain;
                Assert.IsTrue(foundNeitherDomain, "Failed to remove {0}{1}{2} domain{3}.",
                    !foundDC1Domain ? "" : "first",
                    foundBothDomains ? " and " : "",
                    !foundDC2Domain ? "" : "second",
                    foundBothDomains ? "s" : "");
            }
            if (!foundDC1Domain) { _dataConnectionsToRemove.Remove(dc1); }
            if (!foundDC2Domain) { _dataConnectionsToRemove.Remove(dc2); }
        }

        [TestCategory(TestCategories.DataConnectionServiceTests), TestMethod]
        public void GetAvailableCurrentValueServices()
        {
            IList<CygNetServiceDTO> currentValueServices = DataConnectionService.GetCurrentValueServices(s_domain);
        }

        [TestCategory(TestCategories.DataConnectionServiceTests), TestMethod]
        public void GetAvailableFacilities()
        {
            for (int ii = 0; ii < 20; ii++)
            {
                IList<CygNetFacilityDTO> facilities = DataConnectionService.GetAvailableCurrentValueServiceFacilities(s_domain, s_site, s_cvsService);
                Trace.WriteLine(facilities.Count);
            }
        }

        [TestCategory(TestCategories.DataConnectionServiceTests), TestMethod]
        public void UpdateDataConnection()
        {
            var dc = new DataConnectionDTO() { ProductionDomain = "12345", Site = "CYG_TEST", Service = s_cvsService };

            long dcId = DataConnectionService.AddDataConnection(dc);
            Assert.AreNotEqual(0, dcId, "Data connection id returned on add should not be zero.");
            DataConnectionDTO[] dataConnections = DataConnectionService.GetAllDataConnections();
            dc = dataConnections.FirstOrDefault(x => x.Id == dcId);
            Assert.IsNotNull(dc, "Failed to get added data connection.");
            _dataConnectionsToRemove.Add(dc);
            Assert.AreEqual("12345", dc.ProductionDomain, "Miamatch in added production domain");
            Assert.AreEqual("CYG_TEST", dc.Site, "Miamatch in added Site");
            Assert.AreEqual(s_cvsService, dc.Service, "Miamatch in added service");
            dc.ProductionDomain = "67899";
            dc.Site = "CYG_UPAD";
            bool updateDC = DataConnectionService.UpdateDataConnection(dc);
            Assert.IsTrue(updateDC, "Failed to Update data connection");
            dataConnections = DataConnectionService.GetAllDataConnections();
            dc = dataConnections.FirstOrDefault(x => x.Id == dcId);
            Assert.IsNotNull(dc, "Failed to get added data connection.");
            Assert.AreEqual("67899", dc.ProductionDomain, "Miamatch in updated production domain");
            Assert.AreEqual("CYG_UPAD", dc.Site, "Miamatch in updated Site");
            Assert.AreEqual(s_cvsService, dc.Service, "Miamatch in updated service");
        }

        [TestCategory(TestCategories.DataConnectionServiceTests), TestMethod]
        public void GetSurfaceNetworkModelFacilities()
        {
            for (int ii = 0; ii < 20; ii++)
            {
                IList<CygNetFacilityDTO> snFacilities = DataConnectionService.GetSurfaceNetworkModelFacilities(s_domain, s_site, s_cvsService);
                Assert.IsNotNull(snFacilities, "Failed to get Surface Network facilities");
            }
        }
    }
}