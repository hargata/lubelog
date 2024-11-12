namespace CarCareTracker.Models
{
    public class ReportParameter
    {
       public List<string> VisibleColumns { get; set; } = new List<string>();
       public List<string> ExtraFields { get; set; } = new List<string>();
    }
}
