using System;
using System.Collections.Generic;
using Contracts.Database;
using Contracts.Database.Extensions;

namespace Contracts.Models
{
    public class Walk
    {
        public double Distance { get; set; }
        public TimeSpan Duration { get; set; }

        public Walk(List<TrackLocation> singleWalkLocations)
        {
            double distance = 0;
            TimeSpan duration = TimeSpan.Zero;

            for (int i = 0; i < singleWalkLocations.Count - 1; ++i)
            {
                duration += singleWalkLocations[i].DateTrack - singleWalkLocations[i + 1].DateTrack;
                distance += singleWalkLocations[i].Distance(singleWalkLocations[i + 1]);
            }

            Distance = distance;
            Duration = duration;
        }
    }
}