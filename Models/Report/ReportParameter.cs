namespace CarCareTracker.Models
{
    public class ReportParameter
    {
       public List<string> VisibleColumns { get; set; } = new List<string>() { 
           nameof(GenericReportModel.DataType), 
           nameof(GenericReportModel.Date), 
           nameof(GenericReportModel.Odometer), 
           nameof(GenericReportModel.Description), 
           nameof(GenericReportModel.Cost),
           nameof(GenericReportModel.Notes) };
       public List<string> ExtraFields { get; set; } = new List<string>();
    }
}
