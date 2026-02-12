namespace CarCareTracker.Models
{
    public class CSVExportParameter
    {
        public TagFilter TagFilter { get; set; } = TagFilter.Exclude;
        public List<string> Tags { get; set; } = new List<string>();
        public bool FilterByDateRange { get; set; } = false;
        public string StartDate { get; set; } = "";
        public string EndDate { get; set; } = "";
    }
}