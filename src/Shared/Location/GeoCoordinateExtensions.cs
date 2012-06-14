//
// Copyright (c) 2010-2011 Jeff Wilcox
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
using System.Device.Location;

namespace JeffWilcox.Controls
{
    public static class GeoCoordinateExtensions
    {
        // This is from .NET 4.0 but doesn't seem to be in the WP SDK.

        // The Haversine formula is used to calculate the distance. The 
        // Haversine formula accounts for the curvature of the earth, but 
        // assumes a spherical earth rather than an ellipsoid. For long 
        // distances, the Haversine formula introduces an error of less than 
        // 0.1 percent. Altitude is not used to calculate the distance.
        public static double GetDistanceTo(this GeoCoordinate self, GeoCoordinate other)
        {
            if ((double.IsNaN(self.Latitude) || double.IsNaN(self.Longitude)) || (double.IsNaN(other.Latitude) || double.IsNaN(other.Longitude)))
            {
                throw new ArgumentException("Latitude or Longitude is not a number.");
            }
            double d = self.Latitude * 0.017453292519943295;
            double num3 = self.Longitude * 0.017453292519943295;
            double num4 = other.Latitude * 0.017453292519943295;
            double num5 = other.Longitude * 0.017453292519943295;
            double num6 = num5 - num3;
            double num7 = num4 - d;
            double num8 = Math.Pow(Math.Sin(num7 / 2.0), 2.0) + ((Math.Cos(d) * Math.Cos(num4)) * Math.Pow(Math.Sin(num6 / 2.0), 2.0));
            double num9 = 2.0 * Math.Atan2(Math.Sqrt(num8), Math.Sqrt(1.0 - num8));
            return (6376500.0 * num9);
        }
    }
}
