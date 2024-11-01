namespace CarCareTracker.Models
{
    public class KioskViewModel
    {
        /// <summary>
        /// List of vehicle ids to exclude from Kiosk Dashboard
        /// </summary>
        public List<int> Exclusions { get; set; } = new List<int>();
        /// <summary>
        /// Whether to retrieve data for vehicle, plans, or reminder view.
        /// </summary>
        public KioskMode KioskMode { get; set; } = KioskMode.Vehicle;
    }
}
