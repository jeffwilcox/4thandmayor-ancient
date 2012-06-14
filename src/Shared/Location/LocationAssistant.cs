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

#if DEBUG
#define GEO_DEBUG
#endif

using System;
using System.Device.Location;
using System.Diagnostics;
using AgFx;

namespace JeffWilcox.FourthAndMayor
{
    public class WalkingEventArgs : EventArgs
    {
        public WalkingEventArgs(double meters)
            : base()
        {
            MetersDelta = meters;
        }

        public double MetersDelta { get; private set; }
    }

    // CONSIDER ADDING AN EVENT to notify in a central place of changes. Such an event would probably need to fire on the UI thread and only once in a while.
    // CONSIDER STORING AND LOADING THE LAST KNOWN COORDINATES

    // if (FileSystemManager.Current.FileNames.Select(fn => fn == FourSquareAppFile).FirstOrDefault() == true)
    // using this file, similar to the basic store of username/password.

    public class LocationAssistant : NotifyPropertyChangedBase
    {
        bool seattle = false;
        bool isWorldwide = false;
        bool isBoka = true; // else 5th and Madison

        /// <summary>
        /// 15 meters is about 50 feet.
        /// </summary>
        private const double MinimumMetersBetweenUpdates = 15;

        private static readonly LocationAssistant _instance = new LocationAssistant();

        private GeoCoordinateWatcher _geo;

        private Location _lastKnown;

        private GeoPositionStatus _status;

        private LocationCache _cache;

        private readonly static object _updateLock = new object();

        public static LocationAssistant Instance
        {
            get { return _instance; }
        }

        internal LocationAssistant()
        {
#if DEBUG
            // Consider mocking when in the emulator.
#endif
            _cache = new LocationCache();
        }

        public event EventHandler<GeoPositionStatusChangedEventArgs> StatusChanged;

        public void Start()
        {
            if (_geo == null)
            {
                // Load last knowns.
                if (_cache.Lat.HasValue && _cache.Long.HasValue)
                {
                    LastKnownLocation = new Location
                        {
                            Latitude = _cache.Lat.Value,
                            Longitude = _cache.Long.Value,
                            Altitude = 0,
                            HorizontalAccuracy = double.NaN,
                            VerticalAccuracy = double.NaN,
                        };
                }

                PriorityQueue.AddWorkItem(() =>
                        {
                            try
                            {
                                _geo = new GeoCoordinateWatcher(GeoPositionAccuracy.High);
                                _geo.MovementThreshold = 20; // meters

                                _geo.StatusChanged += OnStatusChanged;
                                _geo.PositionChanged += OnPositionChanged;
                                _geo.Start();
                            }
                            catch
                            {
                            }
                        });
            }
        }

        public void Stop()
        {
            if (_geo != null)
            {
                _geo.Stop();
                _geo = null;
            }
        }

        private Location _liveLocation;
        public Location LastKnownLiveLocation
        {
            get
            {
                return _liveLocation;
            }
            set
            {
                _liveLocation = value;
                PriorityQueue.AddUiWorkItem(() => RaisePropertyChanged("LastKnownLiveLocation"));
            }
        }

        public Location LastKnownLocation
        {
            get
            {
                Location location = null;
                lock (_updateLock)
                {
                    location = _lastKnown;
                }
                return location;
            }

            set
            {
                if (_lastKnown != value)
                {
                    PriorityQueue.AddUiWorkItem(() => RaisePropertyChanged("LastKnownLocation"));
                }
                _lastKnown = value;
            }
        }

        public GeoPositionStatus Status
        {
            get { return _status; }
            set
            {
                _status = value;
                if (_status != value)
                {
                    PriorityQueue.AddUiWorkItem(() => RaisePropertyChanged("Status"));
                }
            }
        }

//        private Location _lastWalkingLocation;

        private void OnPositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            lock (_updateLock)
            {
                LastKnownLiveLocation = Location.CreateLocation(e.Position.Location);
                LastKnownLocation = Location.CreateLocation(e.Position.Location);

                Debug.WriteLine(_lastKnown);

                if (_cache != null)
                {
                    double meters = MetersBetweenPositions(_cache.AsLocation, _lastKnown);
                    if (meters > MinimumMetersBetweenUpdates)
                    {
                        // v1.5 change: just when the walking changes.
                        _cache.Lat = _lastKnown.Latitude;
                        _cache.Long = _lastKnown.Longitude;
                        _cache.SaveSoon();

                        Debug.WriteLine("Walked " + meters.ToString() + " meters.");

                        OnWalkingPositionChanged(new WalkingEventArgs(meters));
                    }
                }

                var handler = PositionChanged;
                if (handler != null)
                {
                    PriorityQueue.AddUiWorkItem(() =>
                    {
                        FireOnPositionChanged();
                    });
                }
            }
        }

        public event EventHandler<WalkingEventArgs> WalkingPositionChanged;

        protected virtual void OnWalkingPositionChanged(WalkingEventArgs args)
        {
            var handler = WalkingPositionChanged;
            if (handler != null)
            {
                handler(this, args);
            }
        }

        public event EventHandler PositionChanged;

        protected virtual void FireOnPositionChanged()
        {
            var handler = PositionChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        public static double KilometersBetweenPositions(LocationPair l1, LocationPair l2)
        {
            return Calc(l1.Latitude, l1.Longitude, l2.Latitude, l2.Longitude);
        }

        public static double MetersBetweenPositions(LocationPair l1, LocationPair l2)
        {
            return KilometersBetweenPositions(l1, l2) * 1000;
        }

        public static double MetersBetweenPositions(GeoCoordinate geo1, GeoCoordinate geo2)
        {
            return MetersBetweenPositions(
                Location.CreateLocationPair(geo1),
                Location.CreateLocationPair(geo2));
        }

        public static double KilometersBetweenPositions(GeoCoordinate geo1, GeoCoordinate geo2)
        {
            return KilometersBetweenPositions(
                Location.CreateLocationPair(geo1),
                Location.CreateLocationPair(geo2));
        }

        private static double Calc(double Lat1, double Long1, double Lat2, double Long2)
        {
            /*
                The Haversine formula according to Dr. Math.
                http://mathforum.org/library/drmath/view/51879.html
                
                dlon = lon2 - lon1
                dlat = lat2 - lat1
                a = (sin(dlat/2))^2 + cos(lat1) * cos(lat2) * (sin(dlon/2))^2
                c = 2 * atan2(sqrt(a), sqrt(1-a)) 
                d = R * c
                
                Where
                    * dlon is the change in longitude
                    * dlat is the change in latitude
                    * c is the great circle distance in Radians.
                    * R is the radius of a spherical Earth.
                    * The locations of the two points in 
                        spherical coordinates (longitude and 
                        latitude) are lon1,lat1 and lon2, lat2.
            */
            double dDistance = Double.MinValue;
            double dLat1InRad = Lat1 * (Math.PI / 180.0);
            double dLong1InRad = Long1 * (Math.PI / 180.0);
            double dLat2InRad = Lat2 * (Math.PI / 180.0);
            double dLong2InRad = Long2 * (Math.PI / 180.0);

            double dLongitude = dLong2InRad - dLong1InRad;
            double dLatitude = dLat2InRad - dLat1InRad;

            // Intermediate result a.

            double a = Math.Pow(Math.Sin(dLatitude / 2.0), 2.0) +
                       Math.Cos(dLat1InRad) * Math.Cos(dLat2InRad) *
                       Math.Pow(Math.Sin(dLongitude / 2.0), 2.0);

            // Intermediate result c (great circle distance in Radians).

            double c = 2.0 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1.0 - a));

            // Distance.

            // const Double kEarthRadiusMiles = 3956.0;

            const Double kEarthRadiusKms = 6376.5;
            dDistance = kEarthRadiusKms * c;

            return dDistance;
        }

        private void OnStatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {
            Status = e.Status;
#if GEO_DEBUG
            Debug.WriteLine("*******");
            Debug.WriteLine("");
            Debug.WriteLine("GEO: Status is {0}", _status);
            Debug.WriteLine("");
#endif

            switch (e.Status)
            {
                case GeoPositionStatus.Initializing:
                    break;

                case GeoPositionStatus.Disabled:
                    //throw new InvalidOperationException();
                    break;

                case GeoPositionStatus.NoData:
#if DEBUG
                    if (seattle)
                    {
                        double lat = isBoka ? 47.60487905922638 : 47.60643068740198;
                        double @long = isBoka ? -122.33625054359436 : -122.3319536447525;

                        Debug.WriteLine("GEO: Using some favorite Seattle coordinates for debug-time emulator work.");
                        LastKnownLocation = new Location
                        {
                            Latitude           = lat,
                            Longitude          = @long,
                            VerticalAccuracy   = double.NaN,
                            HorizontalAccuracy = 350,
                            Altitude           = 0,
                        };
                    }
                    else
                    {
                        if (isWorldwide)
                        {
                            Debug.WriteLine("GEO: Using a worldwide coordinate for debug-time emulator work.");
                            LastKnownLocation = new Location
                            {
                                // sweden
                                //Latitude = 55.603331,
                                //Longitude = 13.001303,

                                // russia
                                //Latitude = 54.741903,
                                //Longitude = 20.532171,

                                // NYC
                                //Latitude = 40.760838,
                                //Longitude = -73.983436,

                                // Shanghai French Concession
                                //Latitude = 31.209058,
                                //Longitude = 121.470015,

                                // Winter Garden, FL
                                Latitude = 28.564889,
                                Longitude = -81.587257,

                                // London, Trafalgar Square
                                //Latitude = 51.507828,
                                //Longitude = -0.128628,

                                VerticalAccuracy = double.NaN,
                                HorizontalAccuracy = 100,
                                Altitude = 0,
                            };
                        }

                        else
                        {
                            // redmond
                            Debug.WriteLine("GEO: Using Redmond work coordinates for debug-time emulator work.");
                            LastKnownLocation = new Location
                                                    {
                                                        Latitude = 47.6372299194336,
                                                        Longitude = -122.133848190308,
                                                        VerticalAccuracy = double.NaN,
                                                        HorizontalAccuracy = 100,
                                                        Altitude = 0,
                                                    };
                        }
                    }
                    Status = GeoPositionStatus.Ready;
#endif
                    break;

                case GeoPositionStatus.Ready:
                    break;
            }

            var handler = StatusChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}
