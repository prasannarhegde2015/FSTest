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
using Weatherford.POP.Utils;
using System.Net.Mail;
using System.Threading;
using System.DirectoryServices.ActiveDirectory;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices;

namespace Weatherford.POP.Server.IntegrationTests
{
    [TestClass]
    public class TrackingItemServiceTests : APIClientTestBase
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


        /// <summary>
        /// FRWM-7059 : Action Tracking : Email Notification 
        /// Integration test task : 
        /// </summary>
        [TestCategory(TestCategories.TrackingItemServiceTests), TestMethod]
        public void VerifyActionTrackingEmailMessages()
        {
            // *****************   Mail Server Credentails used ********************************************************
            string SMTPServer = String.Empty;
            string SMTPPort = String.Empty;
            string FromEmailId = String.Empty;
            string FromPassword = String.Empty;
            if (s_isRunningInATS == true)
            {
                SMTPServer = "smtp.vsi.dom";
                SMTPPort = "25"; // Non SSL Port is having issue.
                FromEmailId = "automation@cygnet.com";
                FromPassword = "CygNet4Fun";
                // Till issue with SSL is fixed Gmail can be used 
                //SMTPServer = "smtp.gmail.com";
                //SMTPPort = "465";
                //FromEmailId = "devopsuser2018@gmail.com";
                //FromPassword = "DevOpsUser97bd916$";
            }
            else
            {
                SMTPServer = "smtp.gmail.com";
                SMTPPort = "465";
                FromEmailId = "devopsuser2018@gmail.com";
                FromPassword = "DevOpsUser97bd916$";
            }

            //*******************************************************************************************
            StringBuilder stb = new StringBuilder();
            String asterik = new String('*', 100);
            string emailstatictext = "Tracking Item [Subject] is approaching the designated Due Date." + Environment.NewLine +
                "Please take appropriate action to Complete and/or Close this Tracking Item.";
            stb.AppendLine(emailstatictext);
            stb.AppendLine(asterik.ToString());
            stb.AppendLine($"This is Automated Test Emal.Do not Reply to this email. Mail is sent from :{ Environment.MachineName}");
            stb.AppendLine(asterik.ToString());
            List<string> emailtolist = new List<string>();
            List<string> emailcclist = new List<string>();
            List<string> emailbcclist = new List<string>();
            emailtolist.Add("automation@cygnet.com");
            emailcclist.Add("Prasanna.Hegde@weatherford.com");
            emailbcclist.Add("Swati.Dumbre@Weatherford.com");
            string emailsubject1 = "Action Tracking: Tracking Item Due soon";
            string emailsubject2 = "Action Tracking: Tracking Due Date is past";


            //First Email Message
            EmailMessageDTO emaildto = new EmailMessageDTO();
            emaildto.Body = stb.ToString();
            emaildto.Subject = emailsubject1;
            emaildto.From = FromEmailId;
            emaildto.ToList = emailtolist;
            emaildto.CcList = emailcclist;
            emaildto.BccList = emailbcclist;
            emaildto.EmailPriority = EmailPriority.High;

            //First Email Message
            EmailMessageDTO emaildto2 = new EmailMessageDTO();
            emaildto2.Body = stb.ToString();
            emaildto2.Subject = emailsubject2;
            emaildto2.From = FromEmailId;
            emaildto2.ToList = emailtolist;
            emaildto2.CcList = emailcclist;
            emaildto2.BccList = emailbcclist;
            emaildto.EmailPriority = EmailPriority.Normal;
            var mail1 = EmailNotificationHelper.CreateFromEmailMessage(emaildto);
            var mail2 = EmailNotificationHelper.CreateFromEmailMessage(emaildto2);
            try
            {
                SetValuesInSystemSettings(SettingServiceStringConstants.SMTP_SERVER_NAME, SMTPServer);
                SetValuesInSystemSettings(SettingServiceStringConstants.SMTP_SERVER_PORT, SMTPPort);
                SetValuesInSystemSettings(SettingServiceStringConstants.SMTP_SERVER_USER_EMAIL, FromEmailId);
                SetValuesInSystemSettings(SettingServiceStringConstants.SMTP_SERVER_USER_PASSWORD, FromPassword);
                object smtpserver = GetForeSiteToolBoxSettingValue(SettingServiceStringConstants.SMTP_SERVER_NAME);
                object smtpserverport = GetForeSiteToolBoxSettingValue(SettingServiceStringConstants.SMTP_SERVER_PORT);
                int port = Convert.ToInt32(smtpserverport);
                object smtpserveruser = GetForeSiteToolBoxSettingValue(SettingServiceStringConstants.SMTP_SERVER_USER_EMAIL);
                object smtpserverpwd = GetForeSiteToolBoxSettingValue(SettingServiceStringConstants.SMTP_SERVER_USER_PASSWORD);
                IList<MailMessage> mailmessageslist = new List<MailMessage> { mail1, mail2 };
                bool result = false;
                result = EmailNotificationHelper.SendMessages((string)smtpserver, port, (string)smtpserveruser, (string)smtpserverpwd, mailmessageslist);
                Assert.IsTrue(result, "Sending EMail Messages from ForeSite Failed");
                if (s_isRunningInATS) // Do more Validation on ATS since we need to wait for about 1.5 minutes
                {
                    //Check if Mail is read by ATS Parser 
                    Thread.Sleep(100 * 1000);//Wait until ATS can parse sent email 
                    string folderpath = @"\\sloabftvm.vsi.dom\ForeSite_Inbox";
                    var directory = new DirectoryInfo(folderpath);
                    List<string> lastemailfiles = (from f in directory.GetFiles()
                                                   orderby f.LastWriteTime descending
                                                   where f.CreationTime > DateTime.Now.AddMinutes(-3)
                                                   select f.Name).ToList();
                    foreach (string sfile in lastemailfiles)
                    {
                        string parsedrecievdemailtext = File.ReadAllText(Path.Combine(folderpath, sfile));
                        Assert.IsNotNull(parsedrecievdemailtext, "Email was not Parsed into a text file ");
                        Assert.IsTrue((parsedrecievdemailtext.ToLower().Contains(emailsubject1.ToLower()) || parsedrecievdemailtext.ToLower().Contains(emailsubject2.ToLower())), "Parsed Email was not having sent text for Emails");

                    }


                }

            }
            finally
            {
                SetValuesInSystemSettings(SettingServiceStringConstants.SMTP_SERVER_NAME, "");
                SetValuesInSystemSettings(SettingServiceStringConstants.SMTP_SERVER_USER_EMAIL, "");
                SetValuesInSystemSettings(SettingServiceStringConstants.SMTP_SERVER_USER_PASSWORD, "");
            }

        }

        /// <summary>
        ///  FRWM-7410 :  Action Tracking : Integration of UI and Back End (Part 1: All CRUD Opeations)
        ///  FRWM-7411 :  Action Tracking : Integration of UI and Back End (Part 2: All Get  Records  Operations)
        ///  API testing taks : FRWM-7434 
        /// </summary>
        [TestCategory(TestCategories.TrackingItemServiceTests), TestMethod]
        public void AddEditNewTrackingItemTest()
        {
            //Create a New Well
            WellDTO nonrrlwell = AddNonRRLWell(GetFacilityId("NFWWELL_", 1), WellTypeId.NF);
            _wellsToRemove.Add(nonrrlwell);


            try
            {
                #region 1. Add Tracking New Item

                #region  1.1 GetTrackingItemType from Reference Table 
                ReferenceDataMaintenanceEntityDTO[] dataEntities = DBEntityService.GetReferenceDataMaintenanceEntities();
                ReferenceDataMaintenanceEntityDTO refTrackingItemDTO = dataEntities.FirstOrDefault(x => x.EntityTitle == "Tracking Item Type");
                MetaDataDTO[] addMetaDatas = DBEntityService.GetRefereneceMetaDataEntityForAdd("r_TrackingItemType");
                MetaDataDTO trackitem1 = addMetaDatas.FirstOrDefault(x => x.ColumnName == "tiyName");
                string itemtype = "ActionTrackingItemType" + DateTime.Now.ToString("MMddyyyyhhmmss");
                trackitem1.DataValue = itemtype;
                string addeitendata = DBEntityService.AddReferenceData(addMetaDatas);
                #endregion

                #region  1.2 GetTrackingItemSubType from Reference Table 
                ReferenceDataMaintenanceEntityDTO refTrackingItemsubtypeDTO = dataEntities.FirstOrDefault(x => x.EntityTitle == "Tracking Item Suntype");
                MetaDataDTO[] addMetaDatas2 = DBEntityService.GetRefereneceMetaDataEntityForAdd("r_TrackingItemSubtype");
                MetaDataDTO trackitemsubype = addMetaDatas2.FirstOrDefault(x => x.ColumnName == "tisName");
                string itemsubtype = "ActionTrackingItemSubType" + DateTime.Now.ToString("MMddyyyyhhmmss");
                trackitemsubype.DataValue = itemsubtype;
                MetaDataDTO trackitemid = addMetaDatas2.FirstOrDefault(x => x.ColumnName == "tisFK_r_TrackingItemType");
                trackitemid.DataValue = Int32.Parse(addeitendata);
                string addeitensubdata = DBEntityService.AddReferenceData(addMetaDatas2);
                #endregion

                #region 1.3 Create DTO for Tracking Item
                string trackingItemSubject = "Tracking Entry Well: " + Guid.NewGuid().ToString();
                TrackingItemDTO trackdto = new TrackingItemDTO();
                trackdto.TrackingItemId = 0;

                trackdto.TrackingStatus = TrackingItemStatus.New;
                trackdto.TrackingItemStartDate = DateTime.Now.AddDays(2);
                trackdto.TrackingItemDueDate = DateTime.Now.AddDays(15);
                trackdto.TrackingCategory = TrackingItemCategory.Action;
                trackdto.TrackingItemType = new ReferenceTableItemDTO();// Int32.Parse(addeitendata);
                trackdto.TrackingItemSubType = new ReferenceTableItemDTO();
                trackdto.TrackingItemType.Id = Int32.Parse(addeitendata);
                trackdto.TrackingItemSubType.Id = Int32.Parse(addeitensubdata);
                trackdto.TrackingItemPlanningTask = (int)PlanningTask.Yes;
                trackdto.TrackingItemDistribution = Distribution.User;
                trackdto.TrackingPriority = TrackingItemPriority.High;
                trackdto.TrackingItemAssignTo = new ControlIdTextDTO() { ControlId = AuthenticatedUser.Id, ControlText = AuthenticatedUser.Name };
                trackdto.TrackingItemReportingManager = new ControlIdTextDTO() { ControlId = AuthenticatedUser.Id, ControlText = AuthenticatedUser.Name };//Need to change the logic while adding reporting manager

                trackdto.TrackingEntity = TrackingItemEntity.Well;
                trackdto.TrackingEntityId = (int)nonrrlwell.Id;
                trackdto.TrackingItemSubject = trackingItemSubject;
                trackdto.TrackingItemDescription = "Details are added by this user";
                #endregion

                #region 1.4 Add Comments and Attachments 

                //Add Tracking Item Comment.
                TrackingItemCommentDTO trackcomt1 = new TrackingItemCommentDTO();
                trackcomt1.TrackingItemComment = "My Tracking Comment  1";

                TrackingItemCommentDTO trackcomt2 = new TrackingItemCommentDTO();
                trackcomt2.TrackingItemComment = "My Tracking Comment  2";

                //Upload Docuemnt
                DocumentGroupingDTO[] documentGroupingDTOs = DocumentService.GetDocumentGroupings();
                var documentGroupingDTO = documentGroupingDTOs.Where(x => x.Category == DocumentCategory.TrackingItem).FirstOrDefault();
                string Path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";
                string fileName = "ForeSiteSplashTrans.png";
                string fileName1 = "GasInjection_Analysis_json_test_US.json";
                byte[] byteArray = GetByteArray(Path, fileName);
                byte[] byteArray1 = GetByteArray(Path, fileName1);
                Assert.IsNotNull(byteArray);
                Assert.IsNotNull(byteArray1);

                //Create Doc 1
                DocumentDTO doc = new DocumentDTO();
                doc.CompleteFileName = fileName;
                doc.DocumentFile = Convert.ToBase64String(byteArray);
                doc.GroupingId = documentGroupingDTO.GroupingId;

                //Create Doc 2
                DocumentDTO doc1 = new DocumentDTO();
                doc1.CompleteFileName = fileName1;
                doc1.DocumentFile = Convert.ToBase64String(byteArray1);
                doc1.GroupingId = documentGroupingDTO.GroupingId;
                #endregion

                trackdto.Documents = new DocumentDTO[] { doc, doc1 };
                trackdto.TrackingItemCommentDTOArray = new TrackingItemCommentDTO[] { trackcomt1, trackcomt2 };
                var trackingItemArray = new TrackingItemDTO[] { trackdto };
                TrackingItemDTO[] saveditems = TrackingItemService.SaveActionTrackingItem(trackingItemArray);
                #endregion

                #region 2. Get Newly Added Tracking Item
                TrackingItemDTO addedTrackingitem = saveditems.FirstOrDefault();
                string startdate = DateTime.Now.AddDays(-30).ToUniversalTime().ToISO8601();
                string enddate = DateTime.Now.AddDays(90).ToUniversalTime().ToISO8601();
                string duration = "90";
                TrackingItemAssignFilter[] trackingItemFilter = new[] { TrackingItemAssignFilter.AssignByMe };

                //   TrackingItemDTO addedTrackingitem = alltracks.FirstOrDefault(x => x.TrackingItemSubject == trackingItemSubject);
                Assert.IsNotNull(addedTrackingitem);
                Trace.WriteLine($"Tracking Item with Subject {trackingItemSubject} was added succesfully. Its Id  : {addedTrackingitem.TrackingItemId.ToString()} ");
                TrackingItemDTO singeladdedrecord = TrackingItemService.GetAllTrackingItems(trackingItemFilter, startdate, enddate, duration).FirstOrDefault();
                //Verify the tracking Item data now from calling sinlge API using  Tracking ID;
                 TrackingItemDTO singeladdedrecordnotusedbyUI = TrackingItemService.GetTrackingItemDetails(addedTrackingitem.TrackingItemId.ToString());
                Assert.IsNotNull(singeladdedrecordnotusedbyUI, "Single Get API is returning no data");
               // TrackingItemDTO singeladdedrecord = addedTrackingitem;
                //This check will be added back once Reporting Manager functionality is in place 
                //Assert.AreEqual(AuthenticatedUser.Id, singeladdedrecord.TrackingItemReportingManagerId, "Reporting Manager is not macthicng");
                Assert.AreEqual(TrackingItemStatus.New, singeladdedrecord.TrackingStatus, "Status is not mactching");
                Assert.IsNotNull(singeladdedrecord.TrackingItemCreatedBy, "Created by is NULL");
                Assert.IsNotNull(singeladdedrecord.TrackingItemCreatedDate, "Created Date is NULL");
                Assert.AreNotEqual(DateTime.MinValue, singeladdedrecord.ChangeDate, "Updated  Date is NULL ( NOte: from API DateTime.Minvalue is interprested as NULL by UI");
                Assert.AreEqual(DateTime.Today.AddDays(2), singeladdedrecord.TrackingItemStartDate, "Start Date not matching");
                Assert.AreEqual(DateTime.Today.AddDays(15), singeladdedrecord.TrackingItemDueDate, "Due Date not matching");
                Assert.AreEqual(TrackingItemCategory.Action, singeladdedrecord.TrackingCategory, "Action is not matching");
                Assert.AreEqual(itemtype, singeladdedrecord.TrackingItemType.Name, "Tracking Item Type Name is  not matching");
                Assert.AreEqual(itemsubtype, singeladdedrecord.TrackingItemSubType.Name, "Tracking Item SubType Name is  not matching");
                Assert.AreEqual((int)PlanningTask.Yes, singeladdedrecord.TrackingItemPlanningTask, "Planning Task is not matching");
                Assert.AreEqual(Distribution.User, singeladdedrecord.TrackingItemDistribution, "Distribution Group is not matching");
                Assert.AreEqual(TrackingItemPriority.High, singeladdedrecord.TrackingPriority, "Priority is not matching");
                Assert.AreEqual(AuthenticatedUser.Id, singeladdedrecord.TrackingEntityUserId, "User Id/ Assigned To  is not matching");
                Assert.AreEqual(TrackingItemEntity.Well, singeladdedrecord.TrackingEntity, "Tracking Entity is not matching");
                Assert.AreEqual((int)nonrrlwell.Id, singeladdedrecord.TrackingEntityId, "Tracking Entity Id  for (Name)is not matching");
                Assert.AreEqual(nonrrlwell.Name, singeladdedrecord.TrackingEntityName, "Tracking Entity Name  for (Name)is not matching");
                Assert.AreEqual(trackingItemSubject, singeladdedrecord.TrackingItemSubject, "Tracking Subject is not matching ");
                Assert.AreEqual(trackdto.TrackingItemDescription, singeladdedrecord.TrackingItemDescription, "Tracking Details is not matching ");
                //Verify Comment Added
                Assert.IsNotNull(singeladdedrecord.TrackingItemCommentDTOArray.FirstOrDefault(x => x.TrackingItemComment == "My Tracking Comment  1"), "Tracking Comment was null for first comment");
                Assert.IsNotNull(singeladdedrecord.TrackingItemCommentDTOArray.FirstOrDefault(x => x.TrackingItemComment == "My Tracking Comment  2"), "Tracking Comment was null for second comment");
                Assert.IsNotNull(singeladdedrecord.TrackingItemCommentDTOArray.FirstOrDefault(x => x.TrackingItemComment == "My Tracking Comment  1").ChangeDate, "Comment Created Date is NULL");
                Assert.IsNotNull(singeladdedrecord.TrackingItemCommentDTOArray.FirstOrDefault(x => x.TrackingItemComment == "My Tracking Comment  1").ChangeUser, "Comment Created By is NULL");
                Assert.IsNotNull(singeladdedrecord.TrackingItemCommentDTOArray.FirstOrDefault(x => x.TrackingItemComment == "My Tracking Comment  2").ChangeDate, "Comment Created Date for cmt 2 is NULL");
                Assert.IsNotNull(singeladdedrecord.TrackingItemCommentDTOArray.FirstOrDefault(x => x.TrackingItemComment == "My Tracking Comment  2").ChangeUser, "Comment Created By for cmt 2 is NULL");
                //Verify Document  Added
                Assert.IsNotNull(singeladdedrecord.Documents.FirstOrDefault(x => x.OrigFileName == fileName.Substring(0, fileName.IndexOf('.'))), "First File was not retrived ");
                Assert.IsNotNull(singeladdedrecord.Documents.FirstOrDefault(x => x.CompleteFileName == fileName), "First File was not retrived ");
                Assert.IsNotNull(singeladdedrecord.Documents.FirstOrDefault(x => x.OrigFileName == fileName1.Substring(0, fileName1.IndexOf('.'))), "Second File was not retrived ");
                Assert.IsNotNull(singeladdedrecord.Documents.FirstOrDefault(x => x.CompleteFileName == fileName1), "Second File was not retrived ");
                Assert.IsNotNull(singeladdedrecord.Documents.FirstOrDefault(x => x.OrigFileName == fileName.Substring(0, fileName.IndexOf('.'))).ChangeUserId, "Attachment User was NULL");
                Assert.IsNotNull(singeladdedrecord.Documents.FirstOrDefault(x => x.OrigFileName == fileName.Substring(0, fileName.IndexOf('.'))).ChangeUserName, "Attachment User was NULL");
                Assert.IsNotNull(singeladdedrecord.Documents.FirstOrDefault(x => x.OrigFileName == fileName.Substring(0, fileName.IndexOf('.'))).ChangeDate, "Attachment Change Date was NULL");
                Trace.WriteLine($"Tracking Item with Subject {trackingItemSubject} was addtion was verified succesfully. Its Id  : {addedTrackingitem.TrackingItemId.ToString()} ");
                #endregion

                #region 3. Edit the Newly Added Comment and Delete them finally
                //Edit Exsiitng Tracking Item.
                EditExisitngTrackingItemTest(nonrrlwell, singeladdedrecord.TrackingItemId);
                #endregion

                Trace.WriteLine("Test Completed Succesfully.");
            }
            finally
            {
                //Make Sure we delete all TrackingItems at end of test 
                if (WellService.GetAllWells().Length < 2)
                {
                    string startdate = DateTime.Now.AddDays(-30).ToUniversalTime().ToISO8601().ToString();
                    string enddate = DateTime.Now.AddDays(30).ToUniversalTime().ToISO8601().ToString();
                    string duration = "30";
                    TrackingItemAssignFilter[] trackingItemFilter = new[] { TrackingItemAssignFilter.AssignedToMe };
                    List<TrackingItemDTO> tracks = TrackingItemService.GetAllTrackingItems(trackingItemFilter, startdate, enddate, duration);
                    List<long> trackingids = new List<long>();
                    foreach (TrackingItemDTO track in tracks)
                    {
                        trackingids.Add(track.TrackingItemId);
                    }
                    TrackingItemService.DeleteActionTrackingItem(trackingids.ToArray());
                }

            }

        }


        public void EditExisitngTrackingItemTest(WellDTO well, long trackingid)
        {

            #region 1. GetRecord from Tracking Item ID (Use sinlge Preferably)
            //Get the TRacking Item for Passed ID first :
            string startdate = DateTime.Now.AddDays(-30).ToUniversalTime().ToISO8601();
            string enddate = DateTime.Now.AddDays(90).ToUniversalTime().ToISO8601();
            string duration = "90";
            TrackingItemAssignFilter[] trackingItemFilter = new[] { TrackingItemAssignFilter.AssignByMe };
            TrackingItemDTO editTrackingitem = TrackingItemService.GetAllTrackingItems(trackingItemFilter, startdate, enddate, duration).FirstOrDefault(x => x.TrackingItemId == trackingid);
            Trace.WriteLine($"Tracking Item with  Id  : {editTrackingitem.TrackingItemId.ToString()} was fetched  succesfully. ");
            //Verify the tracking Item data now from calling sinlg eAPI using ID;
            //      
            #endregion

            #region 2. Edit the TrackingItem 
            TrackingItemDTO trackdto = editTrackingitem;

            ReferenceDataMaintenanceEntityDTO[] dataEntities = DBEntityService.GetReferenceDataMaintenanceEntities();
            ReferenceDataMaintenanceEntityDTO refTrackingItemDTO = dataEntities.FirstOrDefault(x => x.EntityTitle == "Tracking Item Type");
            MetaDataDTO[] editMetaDatas = DBEntityService.GetRefereneceMetaDataEntityForAdd("r_TrackingItemType");
            MetaDataDTO trackitem1 = editMetaDatas.FirstOrDefault(x => x.ColumnName == "tiyName");
            string itemtype = "EditActionTrackingItemType" + DateTime.Now.ToString("MMddyyyyhhmmss");
            trackitem1.DataValue = itemtype;
            string addeitendata = DBEntityService.AddReferenceData(editMetaDatas);

            ReferenceDataMaintenanceEntityDTO refTrackingItemsubtypeDTO = dataEntities.FirstOrDefault(x => x.EntityTitle == "Tracking Item Suntype");
            MetaDataDTO[] addMetaDatas2 = DBEntityService.GetRefereneceMetaDataEntityForAdd("r_TrackingItemSubtype");
            MetaDataDTO trackitemsubype = addMetaDatas2.FirstOrDefault(x => x.ColumnName == "tisName");
            string itemsubtype = "EditActionTrackingItemSubType" + DateTime.Now.ToString("MMddyyyyhhmmss");
            trackitemsubype.DataValue = itemsubtype;
            MetaDataDTO trackitemid = addMetaDatas2.FirstOrDefault(x => x.ColumnName == "tisFK_r_TrackingItemType");
            trackitemid.DataValue = Int32.Parse(addeitendata);
            string addeitensubdata = DBEntityService.AddReferenceData(addMetaDatas2);
            string trackingItemSubject = "Edit Mode: Tracking Entry Well: " + Guid.NewGuid().ToString();


            trackdto.TrackingItemId = trackingid;
            trackdto.TrackingStatus = TrackingItemStatus.InProgress;
            trackdto.TrackingItemStartDate = DateTime.Now.AddDays(-2);
            trackdto.TrackingItemDueDate = DateTime.Now.AddDays(15);
            trackdto.TrackingCategory = TrackingItemCategory.Action;
            trackdto.TrackingItemType = new ReferenceTableItemDTO();// Int32.Parse(addeitendata);
            trackdto.TrackingItemSubType = new ReferenceTableItemDTO();
            trackdto.TrackingItemType.Id = Int32.Parse(addeitendata);
            trackdto.TrackingItemSubType.Id = Int32.Parse(addeitensubdata);
            trackdto.TrackingItemPlanningTask = (int)PlanningTask.Yes;
            trackdto.TrackingItemDistribution = Distribution.User;
            trackdto.TrackingPriority = TrackingItemPriority.Medium;
            trackdto.TrackingItemAssignTo = new ControlIdTextDTO() { ControlId = AuthenticatedUser.Id, ControlText = AuthenticatedUser.Name };
            trackdto.TrackingItemReportingManager = new ControlIdTextDTO() { ControlId = AuthenticatedUser.Id, ControlText = AuthenticatedUser.Name };//Need to change the logic while adding reporting manager
            trackdto.TrackingEntity = TrackingItemEntity.Well;
            trackdto.TrackingEntityId = (int)well.Id;
            trackdto.TrackingItemSubject = trackingItemSubject;
            trackdto.TrackingItemDescription = "Edit Mode :Details are added by this user";

            //Add Tracking Item Comment.
            TrackingItemCommentDTO trackcomt1 = new TrackingItemCommentDTO();
            trackcomt1.TrackingItemComment = "Edit for My Tracking Comment  1";
            List<TrackingItemCommentDTO> trackexisting = trackdto.TrackingItemCommentDTOArray.ToList();

            trackexisting.Add(trackcomt1);
            trackdto.TrackingItemCommentDTOArray = trackexisting.ToArray();

            //Upload Docuemnt
            DocumentGroupingDTO[] documentGroupingDTOs = DocumentService.GetDocumentGroupings();
            var documentGroupingDTO = documentGroupingDTOs.Where(x => x.Category == DocumentCategory.TrackingItem).FirstOrDefault();
            string Path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.Images.";
            string fileName = "Process.jpg";
            string fileName1 = "TopView.jpg";
            byte[] byteArray = GetByteArray(Path, fileName);
            byte[] byteArray1 = GetByteArray(Path, fileName1);
            Assert.IsNotNull(byteArray);
            Assert.IsNotNull(byteArray1);

            //Create Doc 1
            DocumentDTO doc = new DocumentDTO();
            doc.CompleteFileName = fileName;
            doc.DocumentFile = Convert.ToBase64String(byteArray);
            doc.GroupingId = documentGroupingDTO.GroupingId;

            //Create Doc 2
            DocumentDTO doc1 = new DocumentDTO();
            doc1.CompleteFileName = fileName1;
            doc1.DocumentFile = Convert.ToBase64String(byteArray1);
            doc1.GroupingId = documentGroupingDTO.GroupingId;
            List<DocumentDTO> doclist = trackdto.Documents.ToList();
            doclist.Add(doc);
            doclist.Add(doc1);
          //  trackdto.Documents = new DocumentDTO[] { doc, doc1 };
            trackdto.Documents = doclist.ToArray();
            var trackingItemArray = new TrackingItemDTO[] { trackdto };
            TrackingItemDTO[] saveditems = TrackingItemService.SaveActionTrackingItem(trackingItemArray);
            //Assert for Edited Item ID 
            #endregion

            #region 3. Verifiction of Edited Tracking Item

            //    TrackingItemDTO editedTrackingitem = TrackingItemService.GetTrackingItemDetails(trackingid.ToString());
            TrackingItemDTO editedTrackingitem = TrackingItemService.GetAllTrackingItems(trackingItemFilter, startdate, enddate, duration).FirstOrDefault(x => x.TrackingItemId == trackingid);

            TrackingItemDTO singeladdedrecord = editedTrackingitem;

            //This check will be added back once Reporting Manager functionality is in place 
            //Assert.AreEqual(AuthenticatedUser.Id, singeladdedrecord.TrackingItemReportingManagerId, "Reporting Manager is not macthicng");
            Assert.AreEqual(4, singeladdedrecord.AttachmentsCount,"Attchments count did not match");
            Assert.AreEqual(TrackingItemStatus.InProgress, singeladdedrecord.TrackingStatus, "Status is not mactching");
            Assert.AreEqual(DateTime.Today.AddDays(-2), singeladdedrecord.TrackingItemStartDate, "Start Date not matching");
            Assert.AreEqual(DateTime.Today.AddDays(15), singeladdedrecord.TrackingItemDueDate, "Due Date not matching");
            Assert.AreEqual(TrackingItemCategory.Action, singeladdedrecord.TrackingCategory, "Action is not matching");
            Assert.IsNotNull(singeladdedrecord.TrackingItemCreatedBy, "Created by is NULL");
            Assert.IsNotNull(singeladdedrecord.TrackingItemCreatedDate, "Created Date is NULL");
            Assert.IsNotNull(singeladdedrecord.ChangeUser, "Updated by is  NULL");
            Assert.AreNotEqual(DateTime.MinValue, singeladdedrecord.ChangeDate, "Updated  Date  is NULL  for EDIT( NOte: from API DateTime.Minvalue is interprested as NULL by UI");
            Assert.AreEqual(itemtype, singeladdedrecord.TrackingItemType.Name, "Tracking Item Type Name is  not matching");
            Assert.AreEqual(itemsubtype, singeladdedrecord.TrackingItemSubType.Name, "Tracking Item SubType Name is  not matching");
            Assert.AreEqual((int)PlanningTask.Yes, singeladdedrecord.TrackingItemPlanningTask, "Planning Task is not matching");
            Assert.AreEqual(Distribution.User, singeladdedrecord.TrackingItemDistribution, "Distribution Group is not matching");
            Assert.AreEqual(TrackingItemPriority.Medium, singeladdedrecord.TrackingPriority, "Priority is not matching");
            Assert.AreEqual(AuthenticatedUser.Id, singeladdedrecord.TrackingEntityUserId, "User Id is not matching");
            Assert.AreEqual(TrackingItemEntity.Well, singeladdedrecord.TrackingEntity, "Tracking Entity is not matching");
            Assert.AreEqual((int)well.Id, singeladdedrecord.TrackingEntityId, "Tracking Entity Id  for (Name)is not matching");
            Assert.AreEqual(trackingItemSubject, singeladdedrecord.TrackingItemSubject, "Tracking Subject is not matching ");
            Assert.AreEqual(trackdto.TrackingItemDescription, singeladdedrecord.TrackingItemDescription, "Tracking Details is not matching ");
            //Verify Comment Added
            Assert.IsNotNull(singeladdedrecord.TrackingItemCommentDTOArray.FirstOrDefault(x => x.TrackingItemComment == "Edit for My Tracking Comment  1"), "Tracking Comment was null for first comment");
            Assert.IsNotNull(singeladdedrecord.TrackingItemCommentDTOArray.FirstOrDefault(x => x.TrackingItemComment == "My Tracking Comment  2"), "Tracking Comment was null for second comment");
            Assert.IsNotNull(singeladdedrecord.TrackingItemCommentDTOArray.FirstOrDefault(x => x.TrackingItemComment == "Edit for My Tracking Comment  1").ChangeDate, "Comment Created Date is NULL");
            Assert.IsNotNull(singeladdedrecord.TrackingItemCommentDTOArray.FirstOrDefault(x => x.TrackingItemComment == "Edit for My Tracking Comment  1").ChangeUser, "Comment Created By is NULL");
            //Verify Document  Added
            Assert.IsNotNull(singeladdedrecord.Documents.FirstOrDefault(x => x.OrigFileName == fileName.Substring(0, fileName.IndexOf('.'))), "First File was not retrived ");
            Assert.IsNotNull(singeladdedrecord.Documents.FirstOrDefault(x => x.OrigFileName == fileName1.Substring(0, fileName1.IndexOf('.'))), "Second File was not retrived ");
            Trace.WriteLine($"Tracking Item with  Id  : {singeladdedrecord.TrackingItemId.ToString()} was edited  succesfully. ");
            #endregion

            #region 4.Delete the Tracking Item and Verify it ot deleted or not
            DeleteTrackingItemTest(well, singeladdedrecord.TrackingItemId);
             startdate = DateTime.Now.AddDays(-30).ToUniversalTime().ToISO8601();
             enddate = DateTime.Now.AddDays(30).ToUniversalTime().ToISO8601();
            string duaration = "90";
            List<TrackingItemDTO> alltracks = TrackingItemService.GetAllTrackingItems(trackingItemFilter, startdate, enddate, duaration);
            editedTrackingitem = alltracks.FirstOrDefault(x => x.TrackingItemSubject == trackingItemSubject);
            Assert.IsNull(editedTrackingitem, "TrackingItem could not be deleted");
            TrackingItemDTO deleteddto = TrackingItemService.GetTrackingItemDetails(trackingid.ToString());
            Assert.IsNull(deleteddto, "TrackingItem could not be deleted");
            Trace.WriteLine($"Tracking Item with  Id  : {singeladdedrecord.TrackingItemId.ToString()} was deleted  succesfully. ");
            #endregion

        }

        public void DeleteTrackingItemTest(WellDTO well, long trackingid)
        {
            var ids = new long[] { trackingid };
            TrackingItemService.DeleteActionTrackingItem(ids);
        }
        /// <summary>
        ///  FRWM-7411 :  Action Tracking : Integration of UI and Back End (Part 2: All Get  Records  Operations)
        ///  API testing taks : FRWM-7434 
        [TestCategory(TestCategories.TrackingItemServiceTests), TestMethod]
        public void VerifyGetAllTrackingItemDetails()
        {
            //Create a New Well
            List<TrackingItemDTO> allitems = new List<TrackingItemDTO>();
            string startdate = DateTime.Now.AddDays(-30).ToUniversalTime().ToISO8601();
            string enddate = DateTime.Now.AddDays(90).ToUniversalTime().ToISO8601();
            string duration = "90";
            TrackingItemAssignFilter[] trackingItemFilter = new[] { TrackingItemAssignFilter.AssignByMe };
            WellDTO nonrrlwell = AddNonRRLWell(GetFacilityId("NFWWELL_", 1), WellTypeId.NF);
            var adgroup = AddADGroup();
            AddUser("WFT\\E222901");
            AddUser("WFT\\E252234");
            _wellsToRemove.Add(nonrrlwell);
            try
            {
                for (int i = 0; i < 100; i++)
                {
                    AddMultipleTrackingItmes(nonrrlwell,adgroup);
                }

                allitems = TrackingItemService.GetAllTrackingItems(trackingItemFilter, startdate, enddate, duration);
                Assert.AreEqual(100, allitems.Count, "Maximum records are not fetched.");
                Trace.WriteLine("Added 100 Tracking Items ");
                //Make Sure that Created Use, Created Date  are not coming as null
                Assert.IsNotNull(allitems.FirstOrDefault(x => x.TrackingStatus == TrackingItemStatus.Closed), "At Least one Closed status tracking item was not added");
                foreach (TrackingItemDTO track in allitems)
                {
                    Assert.IsNotNull(nonrrlwell.Name, track.TrackingEntityName, $"Entity Name is coming as NULL for Tracking Item with Tracking id {track.TrackingItemId}");
                    Assert.IsNotNull(track.TrackingItemCreatedBy, $"Created by  is coming as NULL for Tracking Item with Tracking id {track.TrackingItemId}");
                    Assert.IsNotNull(track.TrackingItemCreatedDate, $"Created date  is coming as NULL for Tracking Item with Tracking id {track.TrackingItemId}");
                    if (track.TrackingStatus == TrackingItemStatus.Closed)
                    {
                        Assert.IsNotNull(track.TrackingItemClosingDate, "Closing Date is Nul when  Status was Closed");
                    }
                    //This check will be added back once Reporting Manager functionality is in place 
                    //   Assert.IsNotNull(track.TrackingItemReportingManagerId, $"Reporting Manager is coming as NULL for Tracking Item with Tracking id {track.TrackingItemId}");
                }
                //Filter by only Start and End Date
                startdate = DateTime.Now.AddDays(-10).ToUniversalTime().ToISO8601();
                enddate = DateTime.Now.AddDays(10).ToUniversalTime().ToISO8601();
                allitems = TrackingItemService.GetAllTrackingItems(trackingItemFilter, startdate, enddate, duration);
                int firstfiltercount = allitems.Count;
                Trace.WriteLine($"First Filter count changed From and To dates {firstfiltercount}");
                Assert.IsTrue(firstfiltercount < 100, $"Records are not filtered from start and end date Filter count was {firstfiltercount}");
                Assert.IsTrue(firstfiltercount > 0, $"first filter retruned zero records {firstfiltercount}");
                List <long> trackingids = new List<long>();
                foreach (TrackingItemDTO track in allitems)
                {
                    Trace.WriteLine($"Due Date {track.TrackingItemDueDate} Start Date {track.TrackingItemStartDate}  and its filter  filtered Due in days {duration}  Filter Start {startdate}  End {enddate}");
                    Assert.IsTrue(track.TrackingItemDueDate < DateTime.Now.AddDays(int.Parse(duration) + 1).ToUniversalTime(), $"Due datefor track item with due date {track.TrackingItemDueDate}  was not within filtered Due in days {duration}");
                    Assert.IsTrue(track.TrackingItemStartDate < DateTime.Parse(enddate).ToUniversalTime() && track.TrackingItemStartDate > DateTime.Parse(startdate).ToUniversalTime(), $"Due datefor track item with due date {track.TrackingItemDueDate}  was not within filtered Due in days {duration}");
                    trackingids.Add(track.TrackingItemId);
                }
                EditMultipleTrackingItmes(nonrrlwell, trackingids.ToArray(),adgroup);
                Trace.WriteLine($"Edited  {allitems.Count} Tracking Items ");
                //Edit the Items having 3 as Document count and Now delete all documents

                allitems = TrackingItemService.GetAllTrackingItems(trackingItemFilter, startdate, enddate, duration);
                TrackingItemDTO editoneagain_to_deletealldocuments = allitems.FirstOrDefault(X => X.AttachmentsCount == 3);
                DocumentDTO[] existingonces = editoneagain_to_deletealldocuments.Documents;
                List<DocumentDTO> alldocslist = existingonces.ToList();
                alldocslist.Remove(alldocslist.FirstOrDefault(x => x.OrigFileName == "GasInjection_Analysis_json_test_US"));
                alldocslist.Remove(alldocslist.FirstOrDefault(x => x.OrigFileName == "Process"));
                alldocslist.Remove(alldocslist.FirstOrDefault(x => x.OrigFileName == "TopView"));
                editoneagain_to_deletealldocuments.Documents = alldocslist.ToArray();
                TrackingItemDTO[]  afterdeleteall =TrackingItemService.SaveActionTrackingItem(new TrackingItemDTO[] { editoneagain_to_deletealldocuments });
                TrackingItemDTO deletedalltracdto = afterdeleteall.FirstOrDefault();
                Assert.AreEqual(0, deletedalltracdto.AttachmentsCount, "Documents were not deleted when all were deleted");
                TrackingItemCommentDTO[] commentsfordleteall = deletedalltracdto.TrackingItemCommentDTOArray;
                List<TrackingItemCommentDTO> cmtdellal = commentsfordleteall.ToList();
                
                cmtdellal.Remove(cmtdellal.FirstOrDefault(x => x.TrackingItemComment.Contains("Edit My Tracking Comment")));
                cmtdellal.Remove(cmtdellal.FirstOrDefault(x => x.TrackingItemComment.Contains("My Tracking Comment")));


                deletedalltracdto.TrackingItemCommentDTOArray = cmtdellal.ToArray();
                deletedalltracdto.Documents = null;
                TrackingItemDTO[] afterdeleteallcmt = TrackingItemService.SaveActionTrackingItem(new TrackingItemDTO[] { deletedalltracdto });
                TrackingItemDTO afterdeleteallcmtdto = afterdeleteallcmt.FirstOrDefault();
                
                Assert.IsNull(afterdeleteallcmtdto.TrackingItemCommentDTOArray, "Comments were not deleted when all were deleted");

                foreach (TrackingItemDTO track in allitems)
                {
                    Assert.IsNotNull(track.ChangeUser, $"Updated  by  is coming as NULL for Tracking Item with Tracking id {track.TrackingItemId}");
                    Assert.IsNotNull(track.ChangeDate, $"Updated date  is coming as NULL for Tracking Item with Tracking id {track.TrackingItemId}");
                    Assert.IsTrue(track.TrackingItemLatestComment.Contains("Edit My Tracking Comment"), $"Latest Comment  is coming as NULL for Tracking Item with Tracking id {track.TrackingItemId}");
                    //This check will be added back once Reporting Manager functionality is in place 
                    //Assert.IsNotNull(track.TrackingItemReportingManagerId, $"Reporting Manager is coming as NULL for Tracking Item with Tracking id {track.TrackingItemId}");
                }
                //Filter by only Duration on top of First Filter
                duration = "7";
                allitems = TrackingItemService.GetAllTrackingItems(trackingItemFilter, startdate, enddate, duration);
                int secondfiltercount = allitems.Count;
                Trace.WriteLine($"Second  Filter count  from Due In :  {secondfiltercount}");
                Assert.IsTrue(secondfiltercount < firstfiltercount, $"Records are not filtered from duration Filter count was {secondfiltercount}");
                Assert.IsTrue(secondfiltercount > 0, $"Second filter retruned zero records {secondfiltercount}");
                foreach (TrackingItemDTO track in allitems)
                {
                    Trace.WriteLine($"Due datefor track item with due date {track.TrackingItemDueDate}  and its filter  filtered Due in days {duration}");
                    Assert.IsTrue(track.TrackingItemDueDate < DateTime.Now.AddDays(int.Parse(duration) + 1).ToUniversalTime(), $"Due datefor track item with due date {track.TrackingItemDueDate}  was not within filtered Due in days {duration}");
                }
                trackingItemFilter = new[] { TrackingItemAssignFilter.AssignedToMe };
                startdate = DateTime.Now.AddDays(-30).ToUniversalTime().ToISO8601();
                enddate = DateTime.Now.AddDays(90).ToUniversalTime().ToISO8601();
                duration = "90";
                allitems = TrackingItemService.GetAllTrackingItems(trackingItemFilter, startdate, enddate, duration);
                int thirdfiltercount = allitems.Count;
                Trace.WriteLine($"Third  Filter count  from Due In :  {thirdfiltercount}");
                Assert.IsTrue(thirdfiltercount < 100, $"Records are not filtered from Assiged To Me  Filter count was {thirdfiltercount}");
                Assert.IsTrue(thirdfiltercount > 0, $"Third filter retruned zero records {secondfiltercount}");
                foreach (TrackingItemDTO track in allitems)
                {
                    Trace.WriteLine($"Assign to Filter Check for track item with user assign to {track.TrackingItemAssignTo.ControlText}  and Logged in Use Name is :  {TrackingItemService.GetTrackingItemUser().FirstOrDefault(x => x.ControlId == AuthenticatedUser.Id).ControlText}");
                    Assert.AreEqual(AuthenticatedUser.Name, track.TrackingItemAssignTo.ControlText, "Assigned To Filter has value other than Loggedn in  user");
                }
            }
            finally
            {
                startdate = DateTime.Now.AddDays(-60).ToUniversalTime().ToISO8601();
                enddate = DateTime.Now.AddDays(90).ToUniversalTime().ToISO8601();
                duration = "90";
                allitems = TrackingItemService.GetAllTrackingItems(trackingItemFilter, startdate, enddate, duration);
                List<long> trackingids = new List<long>();
                foreach (TrackingItemDTO track in allitems)
                {
                    trackingids.Add(track.TrackingItemId);
                }
                TrackingItemService.DeleteActionTrackingItem(trackingids.ToArray());
                RemoveUser("WFT\\E222901");
                RemoveUser("WFT\\E252234");
                _adGroupsToRemove.Add(adgroup);

            }
        }
        public void AddMultipleTrackingItmes(WellDTO well ,ADGroupDTO adt)
        {
            #region 1. Add Tracking New Item

            Random random = new Random();
            #region  1.1 GetTrackingItemType from Reference Table 
            ReferenceDataMaintenanceEntityDTO[] dataEntities = DBEntityService.GetReferenceDataMaintenanceEntities();
            ReferenceDataMaintenanceEntityDTO refTrackingItemDTO = dataEntities.FirstOrDefault(x => x.EntityTitle == "Tracking Item Type");
            MetaDataDTO[] addMetaDatas = DBEntityService.GetRefereneceMetaDataEntityForAdd("r_TrackingItemType");
            MetaDataDTO trackitem1 = addMetaDatas.FirstOrDefault(x => x.ColumnName == "tiyName");
            string itemtype = "ActionTrackingItemType " + random.Next(10000, 30000) + " " + DateTime.Now.ToString("MMddyyyyhhmmss");
            trackitem1.DataValue = itemtype;
            string addeitendata = DBEntityService.AddReferenceData(addMetaDatas);
            #endregion

            #region  1.2 GetTrackingItemSubType from Reference Table 
            ReferenceDataMaintenanceEntityDTO refTrackingItemsubtypeDTO = dataEntities.FirstOrDefault(x => x.EntityTitle == "Tracking Item Suntype");
            MetaDataDTO[] addMetaDatas2 = DBEntityService.GetRefereneceMetaDataEntityForAdd("r_TrackingItemSubtype");
            MetaDataDTO trackitemsubype = addMetaDatas2.FirstOrDefault(x => x.ColumnName == "tisName");
            string itemsubtype = "ActionTrackingItemSubType" + random.Next(10000, 30000) + " " + DateTime.Now.ToString("MMddyyyyhhmmss");
            trackitemsubype.DataValue = itemsubtype;
            MetaDataDTO trackitemid = addMetaDatas2.FirstOrDefault(x => x.ColumnName == "tisFK_r_TrackingItemType");
            trackitemid.DataValue = Int32.Parse(addeitendata);
            string addeitensubdata = DBEntityService.AddReferenceData(addMetaDatas2);
            #endregion

            #region 1.3 Create DTO for Tracking Item
            string trackingItemSubject = "Tracking Entry Well: " + Guid.NewGuid().ToString();
            TrackingItemDTO trackdto = new TrackingItemDTO();
            trackdto.TrackingItemId = 0;
            trackdto.TrackingItemReportingManagerId = AuthenticatedUser.Id;

            int startdaterandom = random.Next(1, 30);
            int duedaterandom = random.Next(1, 8);

            Array values = Enum.GetValues(typeof(TrackingItemStatus));

            TrackingItemStatus randomStatus = (TrackingItemStatus)values.GetValue(random.Next(values.Length));
            trackdto.TrackingStatus = randomStatus;
            trackdto.TrackingItemStartDate = DateTime.Today.AddDays(startdaterandom);
            trackdto.TrackingItemDueDate = DateTime.Today.AddDays(startdaterandom + duedaterandom);
            trackdto.TrackingCategory = TrackingItemCategory.Action;
            trackdto.TrackingItemType = new ReferenceTableItemDTO();// Int32.Parse(addeitendata);
            trackdto.TrackingItemSubType = new ReferenceTableItemDTO();
            trackdto.TrackingItemType.Id = Int32.Parse(addeitendata);
            trackdto.TrackingItemSubType.Id = Int32.Parse(addeitensubdata);
            trackdto.TrackingItemPlanningTask = random.Next(1, 2);
            values = Enum.GetValues(typeof(Distribution));
            Distribution randomDsitribution = (Distribution)values.GetValue(random.Next(values.Length));
            trackdto.TrackingItemDistribution = randomDsitribution;
            values = Enum.GetValues(typeof(TrackingItemPriority));
            TrackingItemPriority randomPriority = (TrackingItemPriority)values.GetValue(random.Next(values.Length));
            trackdto.TrackingItemDistribution = randomDsitribution;
            trackdto.TrackingPriority = randomPriority;
            trackdto.TrackingItemReportingManager = new ControlIdTextDTO() { ControlId = AuthenticatedUser.Id, ControlText = AuthenticatedUser.Name };//Need to change the logic while adding reporting manager
            ControlIdTextDTO[] userslist = TrackingItemService.GetTrackingItemUser();
            int randomusername = random.Next(2,5);//add two more users
            if (trackdto.TrackingItemDistribution == Distribution.User)
            {
                trackdto.TrackingItemAssignTo = new ControlIdTextDTO() { ControlId = userslist[randomusername].ControlId, ControlText = userslist[randomusername].ControlText };
            }
            else if (trackdto.TrackingItemDistribution == Distribution.Group)
            {
                trackdto.TrackingItemAssignTo = new ControlIdTextDTO() { ControlId = adt.Id, ControlText = adt.Name };//Need to change the logic while adding reporting manager
            }
            trackdto.TrackingEntity = TrackingItemEntity.Well;
            trackdto.TrackingEntityId = (int)well.Id;
            trackdto.TrackingItemSubject = trackingItemSubject;
            trackdto.TrackingItemDescription = $"Details :  {random.Next(100)}are added by this user";
            #endregion

            #region 1.4 Add Comments and Attachments 

            //Add Tracking Item Comment.
            TrackingItemCommentDTO trackcomt1 = new TrackingItemCommentDTO();
            trackcomt1.TrackingItemComment = $"My Tracking Comment  {random.Next(100)}";

            TrackingItemCommentDTO trackcomt2 = new TrackingItemCommentDTO();
            trackcomt2.TrackingItemComment = $"My Tracking Comment  {random.Next(100)}";

            //Upload Docuemnt
            DocumentGroupingDTO[] documentGroupingDTOs = DocumentService.GetDocumentGroupings();
            var documentGroupingDTO = documentGroupingDTOs.Where(x => x.Category == DocumentCategory.TrackingItem).FirstOrDefault();
            string Path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";
            string fileName = "ForeSiteSplashTrans.png";
            string fileName1 = "GasInjection_Analysis_json_test_US.json";
            byte[] byteArray = GetByteArray(Path, fileName);
            byte[] byteArray1 = GetByteArray(Path, fileName1);
            Assert.IsNotNull(byteArray);
            Assert.IsNotNull(byteArray1);

            //Create Doc 1
            DocumentDTO doc = new DocumentDTO();
            doc.CompleteFileName = fileName;
            doc.DocumentFile = Convert.ToBase64String(byteArray);
            doc.GroupingId = documentGroupingDTO.GroupingId;

            //Create Doc 2
            DocumentDTO doc1 = new DocumentDTO();
            doc1.CompleteFileName = fileName1;
            doc1.DocumentFile = Convert.ToBase64String(byteArray1);
            doc1.GroupingId = documentGroupingDTO.GroupingId;
            #endregion

            trackdto.Documents = new DocumentDTO[] { doc, doc1 };
            trackdto.TrackingItemCommentDTOArray = new TrackingItemCommentDTO[] { trackcomt1, trackcomt2 };
            var trackingItemArray = new TrackingItemDTO[] { trackdto };
            TrackingItemService.SaveActionTrackingItem(trackingItemArray);
            #endregion
        }
        public void EditMultipleTrackingItmes(WellDTO well, long[] trackingitemarray ,ADGroupDTO adt)
        {
            #region 1. Edit Tracking New Item
            foreach (long trackingitemid in trackingitemarray)
            {
                TrackingItemDTO trackdto = TrackingItemService.GetTrackingItemDetails(trackingitemid.ToString());
                Random random = new Random();
                #region  1.1 GetTrackingItemType from Reference Table 
                ReferenceDataMaintenanceEntityDTO[] dataEntities = DBEntityService.GetReferenceDataMaintenanceEntities();
                ReferenceDataMaintenanceEntityDTO refTrackingItemDTO = dataEntities.FirstOrDefault(x => x.EntityTitle == "Tracking Item Type");
                MetaDataDTO[] addMetaDatas = DBEntityService.GetRefereneceMetaDataEntityForAdd("r_TrackingItemType");
                MetaDataDTO trackitem1 = addMetaDatas.FirstOrDefault(x => x.ColumnName == "tiyName");
                string itemtype = "EditActionTrackingItemType " + random.Next(20000, 30000) + " " + DateTime.Now.ToString("MMddyyyyhhmmss");
                trackitem1.DataValue = itemtype;
                string addeitendata = DBEntityService.AddReferenceData(addMetaDatas);
                #endregion

                #region  1.2 GetTrackingItemSubType from Reference Table 
                ReferenceDataMaintenanceEntityDTO refTrackingItemsubtypeDTO = dataEntities.FirstOrDefault(x => x.EntityTitle == "Tracking Item Suntype");
                MetaDataDTO[] addMetaDatas2 = DBEntityService.GetRefereneceMetaDataEntityForAdd("r_TrackingItemSubtype");
                MetaDataDTO trackitemsubype = addMetaDatas2.FirstOrDefault(x => x.ColumnName == "tisName");
                string itemsubtype = "EditActionTrackingItemSubType" + random.Next(20000, 30000) + " " + DateTime.Now.ToString("MMddyyyyhhmmss");
                trackitemsubype.DataValue = itemsubtype;
                MetaDataDTO trackitemid = addMetaDatas2.FirstOrDefault(x => x.ColumnName == "tisFK_r_TrackingItemType");
                trackitemid.DataValue = Int32.Parse(addeitendata);
                string addeitensubdata = DBEntityService.AddReferenceData(addMetaDatas2);
                #endregion

                #region 1.3 Edit DTO for Tracking Item
                string trackingItemSubject = "Edit for Tracking Entry Well: " + Guid.NewGuid().ToString();

                trackdto.TrackingItemId = trackingitemid;
                trackdto.TrackingItemReportingManagerId = AuthenticatedUser.Id;
                Array values = Enum.GetValues(typeof(TrackingItemStatus));
                TrackingItemStatus randomStatus = (TrackingItemStatus)values.GetValue(random.Next(values.Length));
                trackdto.TrackingStatus = randomStatus;
                //        trackdto.TrackingItemStartDate = DateTime.Now.AddDays(startdaterandom);
                //       trackdto.TrackingItemDueDate = DateTime.Now.AddDays(duedaterandom);
                trackdto.TrackingCategory = TrackingItemCategory.Action;
                trackdto.TrackingItemType = new ReferenceTableItemDTO();// Int32.Parse(addeitendata);
                trackdto.TrackingItemSubType = new ReferenceTableItemDTO();
                trackdto.TrackingItemType.Id = Int32.Parse(addeitendata);
                trackdto.TrackingItemSubType.Id = Int32.Parse(addeitensubdata);
                trackdto.TrackingItemPlanningTask = random.Next(1, 2);
                values = Enum.GetValues(typeof(Distribution));
                Distribution randomDsitribution = (Distribution)values.GetValue(random.Next(values.Length));
                trackdto.TrackingItemDistribution = randomDsitribution;
                values = Enum.GetValues(typeof(TrackingItemPriority));
                TrackingItemPriority randomPriority = (TrackingItemPriority)values.GetValue(random.Next(values.Length));
                trackdto.TrackingItemDistribution = randomDsitribution;
                trackdto.TrackingPriority = randomPriority;
                ControlIdTextDTO[] userslist = TrackingItemService.GetTrackingItemUser();
                int randomusername = random.Next(3, 4);//add two more users
                if (trackdto.TrackingItemDistribution == Distribution.User)
                {
                    trackdto.TrackingItemAssignTo = new ControlIdTextDTO() { ControlId = userslist[randomusername].ControlId, ControlText = userslist[randomusername].ControlText };
                }
                else if (trackdto.TrackingItemDistribution == Distribution.Group)
                {
                    trackdto.TrackingItemAssignTo = new ControlIdTextDTO() { ControlId = adt.Id, ControlText = adt.Name };//Need to change the logic while adding reporting manager
                }
                trackdto.TrackingItemReportingManager = new ControlIdTextDTO() { ControlId = AuthenticatedUser.Id, ControlText = AuthenticatedUser.Name };//Need to change the logic while adding reporting manager                trackdto.TrackingEntity = TrackingItemEntity.Well;
                trackdto.TrackingEntityId = (int)well.Id;
                trackdto.TrackingItemSubject = trackingItemSubject;
                trackdto.TrackingItemDescription = $"Details :  {random.Next(100)} are edited by this user";
                #endregion

                #region 1.4 Edit Comments and Attachments 

                //Get Existing Tracking Item Comment.
                TrackingItemCommentDTO trackcomt1 = new TrackingItemCommentDTO();
                trackcomt1.TrackingItemComment = $"Edit My Tracking Comment  {random.Next(100)}";
                //try to delete exsiitng comment also
                TrackingItemCommentDTO exisitngcmt = trackdto.TrackingItemCommentDTOArray.FirstOrDefault(x => x.TrackingItemComment.Contains("My Tracking Comment"));
   

                //Upload Docuemnt
                DocumentGroupingDTO[] documentGroupingDTOs = DocumentService.GetDocumentGroupings();
                var documentGroupingDTO = documentGroupingDTOs.Where(x => x.Category == DocumentCategory.TrackingItem).FirstOrDefault();
                string Path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.Images.";
                string fileName = "Process.jpg";
                string fileName1 = "TopView.jpg";
                byte[] byteArray = GetByteArray(Path, fileName);
                byte[] byteArray1 = GetByteArray(Path, fileName1);
                Assert.IsNotNull(byteArray);
                Assert.IsNotNull(byteArray1);

                //Create Doc 1
                DocumentDTO doc = new DocumentDTO();
                doc.CompleteFileName = fileName;
                doc.DocumentFile = Convert.ToBase64String(byteArray);
                doc.GroupingId = documentGroupingDTO.GroupingId;

                //Create Doc 2
                DocumentDTO doc1 = new DocumentDTO();
                doc1.CompleteFileName = fileName1;
                doc1.DocumentFile = Convert.ToBase64String(byteArray1);
                doc1.GroupingId = documentGroupingDTO.GroupingId;
                #endregion

                DocumentDTO[] arrdocdto = trackdto.Documents;
                DocumentDTO removeddoc = trackdto.Documents.FirstOrDefault(x => x.OrigFileName == "ForeSiteSplashTrans");
                List<DocumentDTO> doclist = arrdocdto.ToList();
                //Add 2 Documents in Edit mode delete one existing document 

                doclist.Add(doc);
                doclist.Add(doc1);
                doclist.Remove(removeddoc);
                trackdto.Documents = doclist.ToArray();
                TrackingItemCommentDTO[] arrcmtdto = trackdto.TrackingItemCommentDTOArray;
                List<TrackingItemCommentDTO> cmtlist = arrcmtdto.ToList();
                //Add 1 Comment in Edit mode delete one existing Comment 
                cmtlist.Add(trackcomt1);
                cmtlist.Remove(exisitngcmt);
                trackdto.TrackingItemCommentDTOArray = cmtlist.ToArray();
                var trackingItemArray = new TrackingItemDTO[] { trackdto };

                TrackingItemService.SaveActionTrackingItem(trackingItemArray);
            }
            #endregion
        }

        public ADGroupDTO AddADGroup()
        {
            Domain domain = Domain.GetComputerDomain();
            PrincipalContext ctx = new PrincipalContext(ContextType.Domain, domain.Name);
            GroupPrincipal grp = new GroupPrincipal(ctx);
            PrincipalSearcher searcher = new PrincipalSearcher(grp);
            var results = searcher.FindOne();
            RoleDTO adminrule = AdministrationService.GetRoles().FirstOrDefault(r => r.Name == "Administrator");
            ADGroupDTO groupDto = new ADGroupDTO
            {
                Assets = new List<AssetDTO>(),
                Roles = new List<RoleDTO>(),
                Name = results.Name,
                Id = 0,
                SecurityIdentifier = results.Sid.Value
            };
            //   Trace.WriteLine($"Attempting to add AD group with name {groupDto.Name} and SID {groupDto.SecurityIdentifier}.");
            List<RoleDTO> rolelist = new List<RoleDTO>();
            rolelist.Add(adminrule);
            groupDto.Roles = rolelist;
            var existingGroup =
                AdministrationService
                    .GetGroups()
                    .Where(x => x.SecurityIdentifier == groupDto.SecurityIdentifier)
                    .FirstOrDefault();

            if (existingGroup != null)
            {
                // Trace.WriteLine("Group already exists.  Not Adding Again.");
                return existingGroup;
            }

            AdministrationService.AddGroup(groupDto);

            Trace.WriteLine("Group added.  Checking that group was added successfully");

            var addedGroup =
                AdministrationService
                    .GetGroups()
                    .Where(x => x.SecurityIdentifier == groupDto.SecurityIdentifier)
                    .FirstOrDefault();

            Assert.IsNotNull(addedGroup, "AD Group was not added ");
            return addedGroup;
        }

        private void AddUser(string username)
        {
            var user = new UserDTO();
            user.Name = username;
            UserDTO addedUser = AdministrationService.GetUsers().FirstOrDefault(u => u.Name == user.Name);
            if (addedUser == null)
            {
                AdministrationService.AddUser(user);
                addedUser = AdministrationService.GetUsers().FirstOrDefault(u => u.Name == user.Name);
                Assert.IsNotNull(addedUser, "Failed to add user.");
            }
        }

        private void RemoveUser(string username)
        {
            UserDTO addedUser = AdministrationService.GetUsers().FirstOrDefault(u => u.Name == username);
            if (addedUser != null)
            {
                _usersToRemove.Add(addedUser);
            }
        }

        [TestMethod]
        public void getemail()
        {
            string outfilelocation = @"C:\temp\emails";

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format("{0},{1},{2}", "User Name","Dispaly Name", "Email Address"));
            string[] email1 = GetEmailAddressforAccount().ToArray();
            foreach (string str1 in email1)
            {
   
                Trace.WriteLine($"list : {str1}");
                sb.AppendLine(str1);
            }
            File.AppendAllText(Path.Combine(outfilelocation, "Outfile.csv"), sb.ToString());
        }
        private List<string> GetEmailAddressforAccount()
        {
            UserDTO[] users = AdministrationService.GetUsers();
            List<String> emails = new List<string>();
            StringBuilder sb = new StringBuilder();
            try
            {
                Domain domain = Domain.GetComputerDomain();
                PrincipalContext ctx = new PrincipalContext(ContextType.Domain, domain.Name);
                foreach(UserDTO usr in users)
                {
                    try
                    {
                        UserPrincipal userPrincipal = UserPrincipal.FindByIdentity(ctx, usr.Name);
                        emails.Add(sb.AppendLine(string.Format("{0},{1},{2}", usr.Name, userPrincipal.DisplayName, userPrincipal.EmailAddress)).ToString());
                        sb.Clear();
                    }
                    catch (Exception)
                    {

                        //
                    }
                }

            }
            catch (Exception ex)
            {

                Trace.WriteLine($"Error in getting email address from AD  for reason : {ex.Message.ToString()}");
            }
            return emails;

        }
    }
}
