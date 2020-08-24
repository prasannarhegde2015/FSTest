using SeleniumAutomation.AGGridScreenDatas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using OpenQA.Selenium;
using System.Diagnostics;

namespace SeleniumAutomation.SeleniumObject
{
    class TrackingItemMasterGridDataProcess : IGridScenarios<TypeTrackingItemMasterGrid>
    {

        double height;
        double width;
        public string scrollp;
        public int scrollsize;
        public TrackingItemMasterGridDataProcess()
        {
            height = SystemParameters.PrimaryScreenHeight;
            width = SystemParameters.PrimaryScreenWidth;
            Console.Write("Screen size found width:-" + width);
            double half = width / 2;
            scrollp = half.ToString();


        }

        public TypeTrackingItemMasterGrid[] FetchdataScreen()
        {
            List<TypeTrackingItemMasterGrid> Datahome = new List<TypeTrackingItemMasterGrid>();
            try
            {
                string style = SeleniumActions.getAttribute(PageObjects.TrackingItemPage.scrollHorizontalContainerTrackingItemMasterGird, "style");
                string[] splitstyle = style.Split(':');
                string[] splitstyle2 = splitstyle[1].Split(';');
                Console.WriteLine("style1->" + splitstyle[1]);
                Console.WriteLine("style->" + splitstyle2[0]);
                scrollsize = int.Parse(splitstyle2[0].Substring(0, splitstyle2[0].Length - 2));
                Console.WriteLine("szie->" + scrollsize);
                List<string> firstcolumnvaluelift = GetAllValuesUnderColumn(PageObjects.TrackingItemPage.FileCount);
                Trace.WriteLine("Grid Column value for first column names are ");
                
                List<string> arrCreatedDate = GetAllValuesUnderColumn(PageObjects.TrackingItemPage.CreatedDate);
                List<string> arrEntity = GetAllValuesUnderColumn(PageObjects.TrackingItemPage.Entity);
                List<string> arrEntityName = GetAllValuesUnderColumn(PageObjects.TrackingItemPage.EntityName);
                List<string> arrPriority = GetAllValuesUnderColumn(PageObjects.TrackingItemPage.Priority);
                List<string> arrStatus = GetAllValuesUnderColumn(PageObjects.TrackingItemPage.Status);
                List<string> arrCategory = GetAllValuesUnderColumn(PageObjects.TrackingItemPage.Category);
                List<string> arrType = GetAllValuesUnderColumn(PageObjects.TrackingItemPage.Type);
                List<string> arrSubtype = GetAllValuesUnderColumn(PageObjects.TrackingItemPage.Subtype);
                List<string> arrPlannedTask = GetAllValuesUnderColumn(PageObjects.TrackingItemPage.PlannedTask);
                List<string> arrSubect = GetAllValuesUnderColumn(PageObjects.TrackingItemPage.Subect);
                List<string> arrDescription = GetAllValuesUnderColumn(PageObjects.TrackingItemPage.Description);
                List<string> arrCreatedBy = GetAllValuesUnderColumn(PageObjects.TrackingItemPage.CreatedBy);
                List<string> arrDistribution = GetAllValuesUnderColumn(PageObjects.TrackingItemPage.Distribution);
                List<string> arrAssignedto = GetAllValuesUnderColumn(PageObjects.TrackingItemPage.Assignedto);
                List<string> arrStartDate = GetAllValuesUnderColumn(PageObjects.TrackingItemPage.StartDate);
                List<string> arrDueDate = GetAllValuesUnderColumn(PageObjects.TrackingItemPage.DueDate);
                List<string> arrClosingDate = GetAllValuesUnderColumn(PageObjects.TrackingItemPage.ClosingDate);
                List<string> arrUpdatedDate = GetAllValuesUnderColumn(PageObjects.TrackingItemPage.UpdatedDate);
                List<string> arrLastComment = GetAllValuesUnderColumn(PageObjects.TrackingItemPage.LastComment);
                List<string> arrReportingManager = GetAllValuesUnderColumn(PageObjects.TrackingItemPage.ReportingManager);

                for (int a = 0; a < firstcolumnvaluelift.Count; a++)
                {
                    Datahome.Add(new TypeTrackingItemMasterGrid()
                    {
                        Files = firstcolumnvaluelift[a],
                        CreatedDate = arrCreatedDate[a],
                        Entity = arrEntity[a],
                        EntityName = arrEntityName[a],
                        Priority = arrPriority[a],
                        Status = arrStatus[a],
                        Category = arrCategory[a],
                        Type = arrType[a],
                        Subtype = arrSubtype[a],
                        PlannedTask = arrPlannedTask[a],
                        Subect = arrSubect[a],
                        Description = arrDescription[a],
                        CreatedBy = arrCreatedBy[a],
                        Distribution = arrDistribution[a],
                        Assignedto = arrAssignedto[a],
                        StartDate = arrStartDate[a],
                        DueDate = arrDueDate[a],
                        ClosingDate = arrClosingDate[a],
                        UpdatedDate = arrUpdatedDate[a],
                        LastComment = arrLastComment[a],
                        ReportingManager = arrReportingManager[a]
                    });


                }
                return Datahome.ToArray();
            }
            catch (Exception ex)
            {
                return Datahome.ToArray();
                throw ex;
            }


        }

        public List<string> GetAllValuesUnderColumn(By columndefby)
        {
            List<string> lststringvaluesforcolumn = SeleniumActions.Gettotalrecordsinlistelemnevisisble(columndefby);
            double i = Convert.ToDouble(scrollp);
            while (lststringvaluesforcolumn == null)
            {
                SeleniumActions.scrollbyel(PageObjects.TrackingItemPage.scrollHorizontalContainerTrackingItemMasterGird, scrollp, "0");
                lststringvaluesforcolumn = SeleniumActions.Gettotalrecordsinlistelemnevisisble(columndefby);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Trace.WriteLine($"Maximum scrolling point reached Column {columndefby.ToString()} ");
                    break;

                }
            }
            if (lststringvaluesforcolumn.Count == 0)
            {
                lststringvaluesforcolumn.Add("NA");
                Trace.WriteLine($"Maximum scrolling point reached Column {columndefby.ToString()} and No values could be fetched even after max scroll ");
            }
            return lststringvaluesforcolumn;
        }
        public TypeTrackingItemMasterGrid[] Sort(By columnsorticonlocator, string sorttype, By columnhover)
        {
            throw new NotImplementedException();
        }

        public TypeTrackingItemMasterGrid[] Filter(string text, By columnmenuicon, By columnheader)
        {
            throw new NotImplementedException();
        }

        public TypeTrackingItemMasterGrid[] Search(string text)
        {
            throw new NotImplementedException();
        }

        public TypeTrackingItemMasterGrid CreateNewView(string viewname)
        {
            throw new NotImplementedException();
        }

        public TypeTrackingItemMasterGrid Colorcode(string columnname, string comparison, string value, int hex, string target)
        {
            throw new NotImplementedException();
        }
    }
}
