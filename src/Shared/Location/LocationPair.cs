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

namespace JeffWilcox.FourthAndMayor
{
    public class LocationPair
    {
        public LocationPair()
        {
            Latitude = double.NaN;
            Longitude = double.NaN;
        }

        public System.Device.Location.GeoCoordinate AsGeoCoordinate()
        {
            return new System.Device.Location.GeoCoordinate(Latitude, Longitude);
        }

        public static LocationPair CreateLocationPair(System.Device.Location.GeoCoordinate geo)
        {
            return new LocationPair
            {
                Longitude = geo.Longitude,
                Latitude = geo.Latitude,
            };
        }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public override string ToString()
        {
            return "Location: { Lat= " + Latitude + ", Long= " + Longitude + " }";
        }
    }
}