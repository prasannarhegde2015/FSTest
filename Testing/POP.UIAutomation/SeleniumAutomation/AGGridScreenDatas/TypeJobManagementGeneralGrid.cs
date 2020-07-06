using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SeleniumAutomation.AGGridScreenDatas
{
    public class TypeJobManagementGeneralGrid
    {
        public string jobId { get; set; }
        public string jobType { get; set; }
        public string jobReason { get; set; }
        public string jobDriver { get; set; }
        public string status { get; set; }
        public string afe { get; set; }
        public string serviceprovider { get; set; }
        public string actualCost { get; set; }
        public string beginDate { get; set; }
        public string endDate { get; set; }
        public string jobdurationHours { get; set; }
        public string accountReference { get; set; }
        public string jobPlanCost { get; set; }
        public string totalCost { get; set; }
        public string remarks { get; set; }

    }
}
