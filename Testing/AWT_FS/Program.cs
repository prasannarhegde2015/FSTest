using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CygNet.Data.Core;
using CygNet.Data.Historian;
using CygNet.API.Core;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Globalization;


namespace Weatherford.POP.AWT_FS
{
    public class Program
    {
        public static void AwtVHSAddPoints(string domain, string siteService, string facilityID, DateTime dateTimeRecord)
        {
            string strDateTime = dateTimeRecord.ToString("MM/dd/yyyy hh:mm:ss tt");
            //create a dictionary to store token value pairs
            Dictionary<string, string> tokens = new Dictionary<string, string>
            {
                { "OIL", "25.2" },
                { "WATER", "2.22" },
                { "GAS", "19.22" },
                { "DURATION", "550" },
                { "DATETIME", strDateTime }
            };
            Console.WriteLine(strDateTime + "  is VHS Record Creation DATETIME");
            //construct an XML string from the token/value dictionary
            if (GetXmlString(tokens, out string xml))
            {
                FacilityTag facilityTagToUse = new FacilityTag(siteService + "::" + facilityID);
                string UDC = "WELLTEST";
                string status = "Good";
                ushort retentionDays = 365;

                //store well test record
                if (StoreWellTestRecord(xml, facilityTagToUse, UDC, status, retentionDays, domain, dateTimeRecord, out string error))
                {
                    Console.WriteLine("Success");
                }
                else
                {
                    Console.WriteLine($"Failed: {error}");
                }

                //Console.ReadKey();
            }
        }

        private static bool GetXmlString(Dictionary<string, string> tokens, out string xml)
        {
            bool result = true;

            xml = "";
            XElement root = new XElement("WellTestRecord",
                from keyValue in tokens
                select new XElement(keyValue.Key, keyValue.Value));

            xml = root.ToString();

            if (string.IsNullOrEmpty(xml))
            {
                result = false;
            }

            return result;
        }

        public static bool StoreWellTestRecord(string xmlRecord, FacilityTag facilityTag, string udc, string status, ushort retentionDays, string domain, DateTime dateTimeRecord, out string error)
        {
            bool result = true;
            error = null;

            //TODO : deal with retention days
            //ushort retentionDays = 365;

            CygNet.API.Core.ServiceInformation serviceInformation = new CygNet.API.Core.ServiceInformation();
            var cygnetAmbientDomain = serviceInformation.GetAmbientDomain();

            if (cygnetAmbientDomain.ToString() != domain)
            {
                Console.WriteLine($"The domain defined for the AWT configuration point of {domain} does not match the ambient domain of {cygnetAmbientDomain}");
            }

            //construct VHS point name to check for existence
            DomainSiteService uisDomainSiteService = new DomainSiteService(cygnetAmbientDomain, facilityTag.SiteService.ToString());
            TagBuilder tag = new TagBuilder();
            tag.SetDomainSiteService(uisDomainSiteService);
            tag.SetFacilityTag(facilityTag);
            tag.SetUDC(udc);

            CygNet.Data.Historian.Name name = new CygNet.Data.Historian.Name
            {
                ID = $"{facilityTag.SiteService.ToString()}:{facilityTag.FacilityId.ToString(CultureInfo.CurrentCulture)}_{udc}"
            };

            //connect to VHS service
            try
            {
                DomainSiteService vhs = serviceInformation.GetAssociatedService(uisDomainSiteService, ServiceType.VHS);
                CygNet.API.Historian.Client vhsClient = new CygNet.API.Historian.Client(vhs);

                if (!VhsPointExists(vhsClient, name))
                {
                    //create new name
                    //ID needs to be in SITE.SERVICE.SHORT_ID:LONG_ID format
                    CygNet.Data.Historian.Name nameToAdd = new CygNet.Data.Historian.Name
                    {
                        ID = $"{facilityTag.Site}.{facilityTag.Service}.{GetUniquePointId()}:{facilityTag.FacilityId}_{udc}"
                    };

                    //try to add name
                    bool addedName = false;
                    int retries = 5;
                    do
                    {
                        try
                        {
                            vhsClient.AddName(nameToAdd, retentionDays);
                            name = nameToAdd;
                            addedName = true;
                        }
                        catch
                        {
                            //didn't add....try again
                            if (retries == 0)
                            {
                                //took to many tries - need to exit
                                error = $"Cannot add '{nameToAdd.ID}' to the VHS.";
                                return false;
                            }
                            else
                            {
                                retries -= 1;
                            }
                        }
                    } while (!addedName);
                }

                //post blob bytes to VHS
                byte[] valueBytes = Encoding.UTF8.GetBytes(status);
                byte[] blobBytes = Encoding.UTF8.GetBytes(xmlRecord);

                HistoricalEntry historicalEntry = new HistoricalEntry
                {
                    Timestamp = dateTimeRecord,
                    TimeOrdinal = 0,
                    Value = valueBytes,
                    ValueType = CygNet.Data.Historian.HistoricalEntryValueType.UTF8
                };

                var entries = new List<Tuple<HistoricalEntry, Byte[]>>
                    {
                        Tuple.Create(historicalEntry, blobBytes)
                    };

                vhsClient.StoreHistoricalEntriesAndBlobs(name, entries);
            }
            catch (Exception ex)
            {
                error = $"StoreWellTestRecord Exception: {ex.ToString()}";
                result = false;
            }

            return result;
        }

        public static bool VhsPointExists(CygNet.API.Historian.Client client, CygNet.Data.Historian.Name name)
        {
            try
            {
                var nameStats = client.GetNameStatistics(name);

                return nameStats != null;
            }
            catch (MessagingException msgExc)
            {
                // These VHS error codes will indicate that the point does not exist
                if (msgExc.Code == 16050 || msgExc.Code == 11)
                {
                    return false;
                }
                else
                {
                    throw;
                }
            }
        }

        public static string GetUniquePointId()
        {
            var randomFileName = System.IO.Path.GetRandomFileName();
            var trimmedRandom = randomFileName.Substring(0, 8);
            return trimmedRandom;
        }

    }
}

