using System;

namespace Contracts.Database
{
    public partial class TrackLocation
    {
        public int Id { get; set; }

        public string Imei { get; set; }

        public decimal Latitude { get; set; }

        public decimal Longitude { get; set; }

        public DateTime DateEvent { get; set; }

        public DateTime DateTrack { get; set; }

        public int TypeSource { get; set; }
    }
}