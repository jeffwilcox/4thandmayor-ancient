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
using System.Device.Location;

namespace JeffWilcox.Controls
{
    public static class BingMapsHelper
    {
        // Mercator Projection constants and values for Bing Maps from:
        // http://msdn.microsoft.com/en-us/library/aa940990.aspx
        private const double BingMapsMercatorConstant = 156543.04; // m/px

        private static double ResolutionMetersPerPixel(double latitude, int zoomLevel)
        {
            zoomLevel = ClampZoomLevel(zoomLevel);
            var cosLat = Math.Cos((latitude * Math.PI) / 180);
            return BingMapsMercatorConstant * cosLat / (Math.Pow(2, zoomLevel));
        }

        private static int ClampZoomLevel(int zoomLevel)
        {
            if (zoomLevel < 1)
            {
                return 1;
            }

            if (zoomLevel > 19)
            {
                return 10;
            }

            return zoomLevel;
        }

        /// <summary>
        /// Gets a Bing Maps zoom level that should let the user easily see
        /// both points of interest. I'm using this to show both the current
        /// location of the phone plus a destination place.
        /// </summary>
        /// <param name="geo1">First point of interest.</param>
        /// <param name="geo2">Second point of interest.</param>
        /// <param name="pixelsBetween">The number of ideal pixels between the
        /// two points. For a screen which may be 480 pixels wide, figure with
        /// pin size on display, maybe provide a value like 210 pixels.</param>
        /// <returns>Returns a zoom level for use with the Bing Maps control or
        /// Bing Maps static map REST API.</returns>
        public static double GetZoomLevelShowingPoints(GeoCoordinate geo1, GeoCoordinate geo2, double pixelsBetween)
        {
            var distanceBetween = geo1.GetDistanceTo(geo2);
            var averageLatitude = (geo1.Latitude + geo2.Latitude) / 2;

            for (int i = 1; i < 20; i++)
            {
                var latitudeResolutionPerPixel = ResolutionMetersPerPixel(averageLatitude, i);
                var pixelDifferenceInMeters = pixelsBetween * latitudeResolutionPerPixel;
                if (pixelDifferenceInMeters < distanceBetween)
                {
                    return i;
                }
            }

            return 19; // max.
        }
    }
}
