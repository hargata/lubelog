using LiteDB;

namespace CarCareTracker.Models
{
    public class TorqueRecord
    {
        /// <summary>
        /// Session Id provided by Torque
        /// </summary>
        [BsonId]
        public string Session { get; set; }
        /// <summary>
        /// VehicleId
        /// </summary>
        public int VehicleId { get; set; }
        /// <summary>
        /// Email Address
        /// </summary>
        public string Eml { get; set; }
        /// <summary>
        /// longitude
        /// </summary>
        public double kff1005 { get; set; }
        /// <summary>
        /// latitude
        /// </summary>
        public double kff1006 { get; set; }

        /// <summary>
        /// Calculated fields.
        /// </summary>
        public double InitialLongitude { get; set; }
        public double InitialLatitude { get; set; }
        public double LastLongitude { get; set; }
        public double LastLatitude { get; set; }
        public double DistanceTraveled { get; set; }
    }
}
