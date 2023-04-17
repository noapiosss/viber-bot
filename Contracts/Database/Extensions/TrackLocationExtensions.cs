using System;

namespace Contracts.Database.Extensions
{
    public static class TrackLocationExtensions
    {
        private static readonly float R = 6371;

        public static double Distance(this TrackLocation trackLocation1, TrackLocation trackLocation2)
        {
            double dLat = DegToRad(trackLocation2.Latitude - trackLocation1.Latitude);
            double dLon = DegToRad(trackLocation2.Longitude - trackLocation1.Longitude);

            double a = (Math.Sin(dLat / 2) * Math.Sin(dLat / 2)) +
                (Math.Cos(DegToRad(trackLocation1.Latitude)) * Math.Cos(DegToRad(trackLocation2.Latitude)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2));

            return 2 * R * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }

        private static double DegToRad(decimal deg)
        {
            return (double)(deg * (decimal)Math.PI / 180);
        }
    }
}