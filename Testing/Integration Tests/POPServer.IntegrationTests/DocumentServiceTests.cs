using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Weatherford.POP.DTOs;
using Weatherford.POP.Enums;

namespace Weatherford.POP.Server.IntegrationTests
{
    [TestClass]
    public class DocumentServiceTests : APIClientTestBase
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

        public string AddWellwithAssembly(string facilityIdBase)
        {
            string facilityId = s_isRunningInATS ? facilityIdBase + "00001" : facilityIdBase + "0001";
            WellConfigurationService.AddWellConfig(new WellConfigDTO()
            {
                Well = SetDefaultFluidType(new WellDTO()
                {
                    Name = DefaultWellName,
                    FacilityId = facilityId,
                    DataConnection = GetDefaultCygNetDataConnection(),
                    SubAssemblyAPI = "SubAssemblyAPI",
                    AssemblyAPI = "AssemblyAPI",
                    CommissionDate = DateTime.Today,
                    WellType = WellTypeId.RRL,
                })
            });
            List<WellDTO> allWells = WellService.GetAllWells().ToList();
            WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName));
            Assert.IsNotNull(well);
            _wellsToRemove.Add(well);
            return well.Id.ToString();
        }

        public string AddJob(string jobStatus)
        {
            List<WellDTO> allWells = WellService.GetAllWells().ToList();
            WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName));

            JobLightDTO job = new JobLightDTO();
            job.WellId = well.Id;
            job.WellName = well.Name;
            job.BeginDate = DateTime.Today.AddDays(0).ToUniversalTime();
            job.EndDate = DateTime.Today.AddDays(30).ToUniversalTime();
            job.ActualCost = (decimal)100000.00;
            job.ActualJobDurationDays = (decimal)20.5;
            job.TotalCost = (decimal)150000.00;
            job.JobRemarks = "TestJobRemarks " + DateTime.UtcNow.ToString();
            job.JobOrigin = "TestJobOrigin ";
            job.AssemblyId = well.AssemblyId;
            //For Below fields User can select any value in the dropdown menu
            job.AFEId = JobAndEventService.GetAFEs().FirstOrDefault().Id;
            job.StatusId = JobAndEventService.GetJobStatuses().FirstOrDefault(j => j.Name == jobStatus).Id;
            job.JobTypeId = JobAndEventService.GetJobTypes().FirstOrDefault().id;
            job.BusinessOrganizationId = JobAndEventService.GetCatalogItemGroupData().Vendors.FirstOrDefault().Id;
            job.AccountRef = "1";
            //JobReasonId drop down selection is based on the JobTypeId
            job.JobReasonId = JobAndEventService.GetJobReasonsForJobType(job.JobTypeId.ToString()).FirstOrDefault().Id;
            job.JobDriverId = (int)JobAndEventService.GetPrimaryMotivationsForJob().FirstOrDefault().Id;
            //Add Job
            string addJob = JobAndEventService.AddJob(job);
            Assert.IsNotNull(addJob, "Failed to add a Job");
            //Get Job
            JobLightDTO getJob = JobAndEventService.GetJobById(addJob);
            Assert.AreEqual(job.WellId, getJob.WellId);
            Assert.AreEqual(job.WellName, getJob.WellName);
            Assert.AreEqual(job.BeginDate, getJob.BeginDate);
            Assert.AreEqual(job.EndDate, getJob.EndDate);
            Assert.AreEqual(job.ActualCost, getJob.ActualCost);
            Assert.AreEqual(job.ActualJobDurationDays, getJob.ActualJobDurationDays);
            Assert.AreEqual(job.TotalCost, getJob.TotalCost);
            Assert.AreEqual(job.JobRemarks, getJob.JobRemarks);
            Assert.AreEqual(job.JobOrigin, getJob.JobOrigin);
            Assert.AreEqual(job.AssemblyId, getJob.AssemblyId);
            Assert.AreEqual(job.AFEId, getJob.AFEId);
            Assert.AreEqual(job.StatusId, getJob.StatusId);
            Assert.AreEqual(job.JobTypeId, getJob.JobTypeId);
            Assert.AreEqual(job.BusinessOrganizationId, job.BusinessOrganizationId);
            Assert.AreEqual(job.AccountRef, getJob.AccountRef);
            Assert.AreEqual(job.JobReasonId, getJob.JobReasonId);
            Assert.AreEqual(job.JobDriverId, getJob.JobDriverId);
            return addJob;
        }

        [TestCategory(TestCategories.DocumentServiceTests), TestMethod]
        public void AddUpdateDeleteGetDocumentGroups()
        {
            DocumentGroupingDTO[] documentGroupings = DocumentService.GetDocumentGroupings();
            // Dencrypt the file path 
            const string Key = "ABCDEFGHJKLMNOPQRSTUVWXYZABCDEFG"; // must be 32 character
            const string IV = "ABCDEFGHIJKLMNOP"; // must be 16 character
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            aes.BlockSize = 128;
            aes.KeySize = 256;
            aes.IV = Encoding.UTF8.GetBytes(IV);
            aes.Key = Encoding.UTF8.GetBytes(Key);
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            byte[] encryptedByte;
            byte[] resultStr;
            Assert.AreEqual(6, documentGroupings.Count(), "Default Document Groupings are not matching");
            string filePath = "";
            foreach (DocumentGroupingDTO dg in documentGroupings)
            {
                using (ICryptoTransform decryptTransform = aes.CreateDecryptor())
                {
                    Assert.AreEqual("20", dg.DocMaxFileSize.ToString());
                    Assert.IsNotNull(dg.GroupingName);
                    Assert.AreEqual(".exe, .zip, .js", dg.RestrictedFileTypes);
                    encryptedByte = Convert.FromBase64String(dg.FilePath);
                    resultStr = decryptTransform.TransformFinalBlock(encryptedByte, 0, encryptedByte.Length);
                    dg.FilePath = Encoding.UTF8.GetString(resultStr);
                    Assert.IsTrue(dg.FilePath.Contains("Attachments"), $"Path for {dg.GroupingName}: \"{dg.FilePath}\" does not contain \"Attachments\"");
                    Assert.AreEqual(0, dg.DocumentsCount);
                    Assert.AreEqual(0, dg.Documents.Count());
                    if (filePath == "")
                        filePath = dg.FilePath;
                }
            }
            //Add
            DocumentGroupingDTO dgpng = new DocumentGroupingDTO();
            dgpng.DocMaxFileSize = 200;
            dgpng.FilePath = filePath + "AddedGrouping";
            dgpng.GroupingName = "UserAddedGroup";
            dgpng.RestrictedFileTypes = ".exe";
            string addDG = DocumentService.AddUpdateDocumentGrouping(dgpng);
            Assert.IsNotNull(addDG, "Document Group not added");
            documentGroupings = DocumentService.GetDocumentGroupings();
            Assert.IsNotNull(documentGroupings);
            Assert.AreEqual(7, documentGroupings.Count(), "Document Groupings are not matching");
            dgpng = documentGroupings.FirstOrDefault(x => x.GroupingName == "UserAddedGroup");
            Assert.IsNotNull(dgpng);
            Assert.AreEqual("200", dgpng.DocMaxFileSize.ToString());
            Assert.AreEqual("UserAddedGroup", dgpng.GroupingName);
            Assert.AreEqual(".exe", dgpng.RestrictedFileTypes);
            encryptedByte = Convert.FromBase64String(dgpng.FilePath);
            using (ICryptoTransform decryptTransform = aes.CreateDecryptor())
            {
                resultStr = decryptTransform.TransformFinalBlock(encryptedByte, 0, encryptedByte.Length);
                dgpng.FilePath = Encoding.UTF8.GetString(resultStr);
                Assert.IsTrue(dgpng.FilePath.Contains("AddedGrouping"));
                Assert.AreEqual(0, dgpng.DocumentsCount);
                Assert.AreEqual(0, dgpng.Documents.Count());
                Assert.IsTrue(dgpng.IsDeletable);
            }
            //Update
            DocumentGroupingDTO dgpng2 = new DocumentGroupingDTO();
            dgpng2.DocMaxFileSize = 300;
            dgpng2.FilePath = filePath;
            dgpng2.GroupingName = "UserAddedGroupDocuments";
            dgpng2.RestrictedFileTypes = ".exe, .js";
            dgpng2.GroupingId = dgpng.GroupingId;
            dgpng2.IsDeletable = true;
            string updateDG = DocumentService.AddUpdateDocumentGrouping(dgpng2);
            documentGroupings = DocumentService.GetDocumentGroupings();
            Assert.IsNotNull(documentGroupings);
            Assert.AreEqual(7, documentGroupings.Count(), "Document Groupings are not matching");
            dgpng = documentGroupings.FirstOrDefault(x => x.GroupingName == "UserAddedGroupDocuments");
            Assert.IsNotNull(dgpng);
            Assert.AreEqual("300", dgpng.DocMaxFileSize.ToString());
            Assert.AreEqual("UserAddedGroupDocuments", dgpng.GroupingName);
            Assert.AreEqual(".exe, .js", dgpng.RestrictedFileTypes);
            encryptedByte = Convert.FromBase64String(dgpng.FilePath);
            using (ICryptoTransform decryptTransform = aes.CreateDecryptor())
            {
                resultStr = decryptTransform.TransformFinalBlock(encryptedByte, 0, encryptedByte.Length);
                dgpng.FilePath = Encoding.UTF8.GetString(resultStr);
                Assert.IsTrue(dgpng.FilePath.Contains("Attachments"));
                Assert.AreEqual(0, dgpng.DocumentsCount);
                Assert.AreEqual(0, dgpng.Documents.Count());
                //Assert.IsTrue(dgpng.IsDeletable);//No option available on UI
            }
            //Delete
            List<string> arrDgIds = new List<string>();
            arrDgIds.Add(dgpng.GroupingId.ToString());
            if (dgpng.DocumentsCount == 0)
            {
                arrDgIds.Add("0");
            }
            bool check = DocumentService.DeleteDocumentGrouping(arrDgIds.ToArray());
            Assert.IsTrue(check, "Failed to delte document grouping");
            documentGroupings = DocumentService.GetDocumentGroupings();
            Assert.IsNotNull(documentGroupings);
            Assert.AreEqual(6, documentGroupings.Count(), "Document Groupings are not matching");
        }

        [TestCategory(TestCategories.DocumentServiceTests), TestMethod]
        public void GetAllDocuments()
        {
            AddWellwithAssembly("RPOC_");
            string jobId = AddJob("Approved");
            DocumentSectionDTO ds = new DocumentSectionDTO();
            // Dencrypt the file path 
            const string Key = "ABCDEFGHJKLMNOPQRSTUVWXYZABCDEFG"; // must be 32 character
            const string IV = "ABCDEFGHIJKLMNOP"; // must be 16 character
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            aes.BlockSize = 128;
            aes.KeySize = 256;
            aes.IV = Encoding.UTF8.GetBytes(IV);
            aes.Key = Encoding.UTF8.GetBytes(Key);
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            ds.SectionId = Convert.ToInt64(jobId);
            ds.SectionName = "Job";
            DocumentGroupingDTO[] allDocuments = DocumentService.GetAllDocuments(ds);
            Assert.AreEqual(5, allDocuments.Count(), "Default Document Groupings are not matching");
            string filePath = "";
            foreach (DocumentGroupingDTO dg in allDocuments)
            {
                using (ICryptoTransform decryptTransform = aes.CreateDecryptor())
                {
                    Assert.AreEqual("20", dg.DocMaxFileSize.ToString());
                    Assert.IsNotNull(dg.GroupingName);
                    Assert.AreEqual(".exe, .zip, .js", dg.RestrictedFileTypes);
                    byte[] encryptedByte = Convert.FromBase64String(dg.FilePath);
                    byte[] resultStr = decryptTransform.TransformFinalBlock(encryptedByte, 0, encryptedByte.Length);
                    dg.FilePath = Encoding.UTF8.GetString(resultStr);
                    Assert.IsTrue(dg.FilePath.Contains("Attachments"), $"\"{dg.FilePath}\" does not contain \"Attachments\"");
                    Assert.AreEqual(0, dg.DocumentsCount);
                    Assert.AreEqual(0, dg.Documents.Count());
                    if (filePath == "")
                        filePath = dg.FilePath;
                }
            }
            DocumentGroupingDTO dgpng = new DocumentGroupingDTO();
            dgpng.DocMaxFileSize = 200;
            dgpng.Category = DocumentCategory.Job;
            dgpng.FilePath = filePath + "AddedGrouping";
            dgpng.GroupingName = "UserAddedGroup";
            dgpng.RestrictedFileTypes = ".exe";
            string addDG = DocumentService.AddUpdateDocumentGrouping(dgpng);
            Assert.IsNotNull(addDG, "Document Group not added");
            allDocuments = DocumentService.GetAllDocuments(ds);
            Assert.AreEqual(6, allDocuments.Count(), "Default Document Groupings are not matching");
            DocumentGroupingDTO[] documentGroupings = DocumentService.GetDocumentGroupings();
            Assert.IsNotNull(documentGroupings);
            Assert.AreEqual(7, documentGroupings.Count(), "Document Groupings are not matching");
            dgpng = documentGroupings.FirstOrDefault(x => x.GroupingName == "UserAddedGroup");
            List<string> arrDgIds = new List<string>();
            arrDgIds.Add(dgpng.GroupingId.ToString());
            if (dgpng.DocumentsCount == 0)
            {
                arrDgIds.Add("0");
            }
            bool check = DocumentService.DeleteDocumentGrouping(arrDgIds.ToArray());
            Assert.IsTrue(check, "Failed to delte document grouping");
            allDocuments = DocumentService.GetAllDocuments(ds);
            Assert.AreEqual(5, allDocuments.Count(), "Default Document Groupings are not matching");
        }

        [TestCategory(TestCategories.DocumentServiceTests), TestMethod]
        public void UploadDeleteDownloadDocuments()
        {
            AddWellwithAssembly("RPOC_");
            string JobId = AddJob("Approved");
            string Path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";
            string fileName = "ForeSiteSplashTrans.png";
            byte[] byteArray = GetByteArray(Path, fileName);
            Assert.IsNotNull(byteArray);
            string base64String = Convert.ToBase64String(byteArray);
            DocumentSectionDTO ds = new DocumentSectionDTO();
            ds.SectionKey = Convert.ToInt64(JobId);
            ds.SectionName = "Job";
            DocumentGroupingDTO[] allDocuments = DocumentService.GetAllDocuments(ds);
            Assert.AreEqual(5, allDocuments.Count(), "Default Document Groupings are not matching");
            DocumentDTO doc = new DocumentDTO();
            doc.SectionName = "Job";
            doc.SectionKey = Convert.ToInt64(JobId);
            doc.GroupingId = allDocuments.FirstOrDefault().GroupingId;
            doc.CompleteFileName = fileName;
            doc.DocumentFile = base64String;
            //Deleting the existing documents inside the directory before upload
            System.IO.DirectoryInfo di = new DirectoryInfo(allDocuments.FirstOrDefault().FilePath);

            Trace.WriteLine($" Default Path for Attachments is {di.ToString()}");

            if (!Directory.Exists(di.ToString()))
            {
                Directory.CreateDirectory(di.ToString());
            }

            foreach (FileInfo file in di.GetFiles())
            {
                if (file.Name.Contains(".png"))
                    file.Delete();
            }
            //Multiple Documents
            List<DocumentDTO> listDocs = new List<DocumentDTO>();
            listDocs.Add(doc);
            DocumentDTO[] arrDocs = listDocs.ToArray();
            DocumentDTO[] addDocs = DocumentService.UploadDocuments(arrDocs);
            Assert.IsNotNull(addDocs, "Failed to upload documents");
            Assert.AreEqual(arrDocs.Count(), addDocs.Count(), "Failed to Upload documents");
            allDocuments = DocumentService.GetAllDocuments(ds);
            List<string> docIds = new List<string>();
            List<DocumentDTO> docs = new List<DocumentDTO>();
            foreach (DocumentGroupingDTO dg in allDocuments)
            {
                foreach (DocumentDTO d in dg.Documents)
                {
                    docIds.Add(d.DocId.ToString());
                    docs.Add(d);
                }
            }
            string[] arrDocIds = docIds.ToArray();
            //Download
            try
            {
                foreach (string dId in arrDocIds)
                {
                    string downDoc = DocumentService.DownloadDocument(dId);
                    Assert.IsNotNull(downDoc);
                }
            }
            catch
            {
                Trace.WriteLine("Failed to Download uploaded file");
            }
            //Delete
            DocumentDTO[] deleteDocs = DocumentService.DeleteDocuments(docs.ToArray());
            foreach (DocumentDTO document in deleteDocs)
            {
                Assert.IsTrue(document.UploadDownloadStatus, "Failed to delete Documents");
            }
        }
    }
}
