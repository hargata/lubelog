namespace CarCareTracker.Models
{
    public class ReportParameter
    {
        public List<string> VisibleColumns { get; set; } = new List<string>();
        public List<string> ExtraFields { get; set; } = new List<string>();
        public TagFilter TagFilter { get; set; } = TagFilter.Exclude;
        public List<string> Tags { get; set; } = new List<string>();
        public bool FilterByDateRange { get; set; } = false;
        public string StartDate { get; set; } = "";
        public string EndDate { get; set; } = "";
    }
}
