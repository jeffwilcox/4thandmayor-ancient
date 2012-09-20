//
// Copyright (c) Jeff Wilcox
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JeffWilcox.FourthAndMayor.Location
{
    public struct EarthGridPoint
    {
        // Nice and round!
        public int Latitude;
        public int Longitude;

        public int LatitudeBox;
        public int LongitudeBox;

        public override string ToString()
        {
            return "(" + LatitudeBox + ", " + LongitudeBox + ")";
        }
    }

    public static class EarthGrid
    {
        public const double QuarterMileInMeters = 402.336;

        // Theory/Math:
        // http://www.uwgb.edu/dutchs/UsefulData/UTMFormulas.HTM
        // http://www-pord.ucsd.edu/~matlab/coord.htm

        private const double m1 = 111132.92;	// latitude calculation term 1
        private const double m2 = -559.82;		// latitude calculation term 2
        private const double m3 = 1.175;		// latitude calculation term 3
        private const double m4 = -0.0023;		// latitude calculation term 4
        private const double p1 = 111412.84;	// longitude calculation term 1
        private const double p2 = -93.5;		// longitude calculation term 2
        private const double p3 = 0.118;		// longitude calculation term 3

        private static double Radian2Degrees(double rad)
        {
            return rad * (360 / (2.0 * Math.PI));
        }

        private static double Degrees2Rad(double deg)
        {
            return deg * ((2.0 * Math.PI) / 360.0);
        }

        public static void CalculateMetersPerDegree(double latitude, out double metersPerDegreeLatitude, out double metersPerDegreeLongitude)
        {
            double lat = Degrees2Rad(latitude);
            metersPerDegreeLatitude = m1 + (m2 * Math.Cos(2 * lat)) + (m3 * Math.Cos(4 * lat)) +
                (m4 * Math.Cos(6 * lat));

            metersPerDegreeLongitude = (p1 * Math.Cos(lat)) + (p2 * Math.Cos(3 * lat)) +
                        (p3 * Math.Cos(5 * lat));
        }

        public static EarthGridPoint GetEarthGridPoint(LocationPair coordinate, double boxSizeInMeters = QuarterMileInMeters)
        {
            EarthGridPoint point = new EarthGridPoint();

            point.Latitude = (int)Decimal.Truncate((decimal)coordinate.Latitude);   // Math.Truncate in Windows 8 Windows Store APIs
            point.Longitude = (int)Decimal.Truncate((decimal)coordinate.Longitude); // Math.Truncate in Windows 8 Windows Store APIs

            double metersPerLat;
            double metersPerLong;
            CalculateMetersPerDegree(point.Latitude, out metersPerLat, out metersPerLong);

            int latBoxCount = (int)Math.Ceiling(metersPerLat / boxSizeInMeters);
            int longBoxCount = (int)Math.Ceiling(metersPerLong / boxSizeInMeters);

            double ld = Math.Abs(coordinate.Latitude % 1);
            double lld = Math.Abs(coordinate.Longitude % 1);

            point.LatitudeBox = (int)Math.Floor(ld * latBoxCount);
            point.LongitudeBox = (int)Math.Floor(lld * longBoxCount); //longDegree % longBoxCount;

            return point;
        }
    }
}
