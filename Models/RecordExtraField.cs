namespace CarCareTracker.Models
{
    public class RecordExtraField
    {
        /// <summary>
        /// Corresponds to int value of ImportMode enum
        /// </summary>
        public int Id { get; set; }
        public List<ExtraField> ExtraFields { get; set; } = new List<ExtraField>();
    }
}
