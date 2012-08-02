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

namespace JeffWilcox.FourthAndMayor
{
    public class Location : LocationPair
    {
        public Location() : base()
        {
            Altitude = double.NaN;
            HorizontalAccuracy = double.NaN;
            VerticalAccuracy = double.NaN;
        }

        public static Location CreateLocation(System.Device.Location.GeoCoordinate geo)
        {
            return new Location
            {
                Longitude = geo.Longitude,
                Latitude = geo.Latitude,
            };
        }

        public double/*?*/ Altitude { get; set; }
        public double/*?*/ HorizontalAccuracy { get; set; }
        public double/*?*/ VerticalAccuracy { get; set; }

        public override string ToString()
        {
            return "Location: { Lat= " + Latitude + ", Long= " + Longitude + ", Altitude= " + Altitude + ", Accuracy= " + HorizontalAccuracy + " " + VerticalAccuracy + " }";
        }
    }
}
