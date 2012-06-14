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
    using System.Globalization;

    public class LocationCache : SettingsStorage
    {
        public LocationCache() : base("loc")
        {
        }

        #region Lat
        private double? _lat;
        private const string LatKey = "lat";
        public double? Lat
        {
            get
            {
                return _lat;
            }
            set
            {
                _lat = value;
                //RaisePropertyChanged("Lat");
            }
        }
        #endregion
        #region Long
        private double? _long;
        private const string LongKey = "long";
        public double? Long
        {
            get
            {
                return _long;
            }
            set
            {
                _long = value;
                //RaisePropertyChanged("Lat");
            }
        }
        #endregion

        protected override void Serialize()
        {
            Setting[LatKey] = _lat.HasValue ? _lat.Value.ToString(CultureInfo.InvariantCulture) : string.Empty;
            Setting[LongKey] = _long.HasValue ? _long.Value.ToString(CultureInfo.InvariantCulture) : string.Empty;

            base.Serialize();
        }

        public Location AsLocation
        {
            get
            {
                return new Location
                {
                    Latitude = Lat.HasValue ? Lat.Value : 0.0,
                    Longitude = Long.HasValue ? Long.Value : 0.0,
                };
            }
        }

        protected override void Deserialize()
        {
            string imp;
            if (Setting.TryGetValue(LatKey, out imp))
            {
                if (!string.IsNullOrEmpty(imp))
                {
                    double d;
                    if (double.TryParse(imp, NumberStyles.Any, CultureInfo.InvariantCulture, out d))
                    {
                        _lat = d;
                    }
                }
            }
            if (Setting.TryGetValue(LongKey, out imp))
            {
                if (!string.IsNullOrEmpty(imp))
                {
                    double d;
                    if (double.TryParse(imp, NumberStyles.Any, CultureInfo.InvariantCulture, out d))
                    {
                        _long = d;
                    }
                }
            }

            base.Deserialize();
        }
    }
}
