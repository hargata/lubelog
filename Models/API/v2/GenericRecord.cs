﻿namespace CarCareTracker.Models.API.v2
{
    public class GenericRecordApiModel
    {
        public string Date { get; set; }
        public int Odometer { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }
        public decimal Cost { get; set; }
        public string Tags { get; set; }
        public List<ExtraField> ExtraFields { get; set; }
    }

}
