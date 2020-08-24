using SeleniumAutomation.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeleniumAutomation.TestData
{
    public  class TrackingItemData
    {
            public string Files { get { return "2"; } }
            public string CreatedDate { get { return DateTime.Now.ToString(); } }
            public string Entity { get { return "Well"; } }
            public string EntityName { get { return "ESPWELL_00001"; } }
            public string Priority { get { return "High"; } }
            public string Status { get { return "New"; } }
            public string Category { get { return "Action"; } }
            public string Type { get { return "UIAType"; } }
            public string Subtype { get { return "UIASubType"; } }
            public string PlannedTask { get { return "Yes"; } }
            public string Subect { get { return "UIA Action Subject"; } }
            public string Description { get { return "UIA Action descp"; } }
            public string CreatedBy { get { return CommonHelper.GetAuthuserID(); } }
            public string Distribution { get { return "User"; } }
            public string Assignedto { get { return CommonHelper.GetAuthuser(); } }
            public string StartDate { get { return DateTime.Today.AddDays(1).ToString("MM/dd/yyyy"); } }
            public string DueDate { get { return DateTime.Today.AddDays(3).ToString("MM/dd/yyyy"); } }
            public string ClosingDate { get { return ""; } }
            public string UpdatedDate { get { return DateTime.Now.ToString(); } }
            public string LastComment { get { return "UIA Comment 1"; } }
            public string ReportingManager { get { return ""; } }
    }
}
