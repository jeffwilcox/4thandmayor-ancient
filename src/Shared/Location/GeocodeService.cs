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
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using AgFx;
using JeffWilcox.Controls;
using Newtonsoft.Json.Linq;

namespace JeffWilcox.FourthAndMayor
{
    public class GeocodeService : NotifyPropertyChangedBase
    {
        private const string GeoCodeServiceUriFormat = "http://dev.virtualearth.net/REST/v1/Locations/{0},{1}?&includeEntityTypes=Address,Neighborhood,PopulatedPlace&key={2}";

        private DateTime _lastUpdated;

        private static readonly TimeSpan MinimumUpdateSpan = TimeSpan.FromSeconds(0.5); // used to be 10!

        private string _lastGeocode;

        private string _asCoordinates;

        private GeocodeViewModel _viewModel;

        public GeocodeViewModel ViewModel
        {
            get { return _viewModel; }
            set
            {
                _viewModel = value;
                RaisePropertyChanged("ViewModel");
            }
        }

        public GeocodeService()
        {
            _viewModel = new GeocodeViewModel();

            if (LocationAssistant.Instance.Status == System.Device.Location.GeoPositionStatus.Ready
                && LocationAssistant.Instance.LastKnownLiveLocation != null)
            {
                UpdateNow();
            }
            else
            {
                LocationAssistant.Instance.PositionChanged += OnFirstPositionChanged;
            }

            LocationAssistant.Instance.WalkingPositionChanged += OnWalkingPositionChanged;
        }

        private void OnFirstPositionChanged(object sender, EventArgs e)
        {
            // First time only.
            LocationAssistant.Instance.PositionChanged -= OnFirstPositionChanged;

            if (_lastGeocode == null)
            {
                _lastGeocode = "Found you!";
                PriorityQueue.AddUiWorkItem(() => RaisePropertyChanged("Location"));
            }

            UpdateNow();
        }

//        private Location _location;

        private void OnWalkingPositionChanged(object sender, EventArgs e)
        {
            UpdateNow();
        }

        private static Uri GetServiceUri(double lat, double @long)
        {
            IAppInfo iai = (IAppInfo)Application.Current;
            Debug.Assert(iai != null);
            return new Uri(string.Format(
                CultureInfo.InvariantCulture,
                GeoCodeServiceUriFormat, 
                lat, 
                @long, iai.BKey, UriKind.Absolute));
        }

        private void UpdateNow()
        {
            Update((result) =>
            {
                PriorityQueue.AddUiWorkItem(() =>
                {
                    Location = result;
                });
            });
        }

        private bool _updating;

        public string Location
        {
            get {
                if (!_updating && _lastUpdated + MinimumUpdateSpan < DateTime.UtcNow)
                {
                    _updating = true;
                    // System.Diagnostics.Debug.WriteLine("Time to update geocode!");
                    Update((result) =>
                        {
                            PriorityQueue.AddUiWorkItem(() => {
                                Location = result;
                            });
                        });
                }

                return _lastGeocode; 
            }
            set
            {
                _lastUpdated = DateTime.UtcNow;
                _updating = false;

                if (_lastGeocode != value)
                {
                    PriorityQueue.AddUiWorkItem(() => RaisePropertyChanged("Location"));
                }
                _lastGeocode = value;
            }
        }

        private static GeocodeService _instance;

        public static GeocodeService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GeocodeService();
                }
                return _instance;
            }
        }

        public void Update(Action<string> callback)
        {
            IWebRequestFactory iwrf = (IWebRequestFactory)Application.Current;
            Debug.Assert(iwrf != null);

            if (LocationAssistant.Instance.LastKnownLiveLocation != null
                && LocationAssistant.Instance.Status == System.Device.Location.GeoPositionStatus.Ready)
            {
                // uses the live version.
                var lll = LocationAssistant.Instance.LastKnownLiveLocation;
                double lat = lll.Latitude;
                double @long = lll.Longitude;

                PriorityQueue.AddNetworkWorkItem(() =>
                {
                    var cclient = iwrf.CreateWebRequestClient(); //new FourSquareWebClient();
                    var client = cclient.GetWrappedClientTemporary();
                    
                    //var r = client.CreateSimpleWebRequest(GetServiceUri(lat, @long));
                    //var r = client.CreateServiceRequest(GetServiceUri(lat, @long), false /* no credentials */);
                    client.DownloadStringCompleted += (x, xe) =>
                    {
                        if (xe.Error != null)
                        {
                            // TODO: Make sure that the chain supports a EMPTY and not NULL geocode
                            _asCoordinates = string.Format(CultureInfo.InvariantCulture, "No data connection ({0:0.000}, {1:0.000})", lat, @long);
                            Location = _asCoordinates;

                            callback(_lastGeocode);
                        }
                        else
                        {
                            string s = xe.Result;
                            Action<string> cb = callback;
                            PriorityQueue.AddWorkItem(() => ParseJson(s, cb));
                        }
                    };
                    client.DownloadStringAsync(GetServiceUri(lat, @long));
                });
            }
            else
            {
                //LastGeocodeName = string.Empty;
                string s = "Location service is off or unavailable.";
                if (LocationAssistant.Instance.Status == System.Device.Location.GeoPositionStatus.Initializing)
                {
                    s = string.Empty;
                    // "Finding your location...";
                }
                if (LocationAssistant.Instance.Status == System.Device.Location.GeoPositionStatus.Ready)
                {
                    s = "Looking up your address...";
                }

                Location = s;
                callback(_lastGeocode);

                // TODO: Warning, this is not a good thing to do. ?
                var cb = callback;
                IntervalDispatcher.BeginInvoke(TimeSpan.FromSeconds(1), () => Update(cb));
            }
        }

        private void ParseJson(string s, Action<string> callback)
        {
            try
            {
                JObject json = JObject.Parse(s);
                string status = Json.TryGetJsonProperty(json, "statusCode");
                if (status != null && status == "200")
                {
                    var rsets = json["resourceSets"];
                    foreach (var rset in rsets)
                    {
                        var sets = rset["resources"];
                        foreach (var set in sets)
                        {
                            // really the first is all we care about
                            string name = Json.TryGetJsonProperty(set, "name");
                            string locality = null;
                            string state = null;
                            var address = set["address"];

                            if (!string.IsNullOrEmpty(name))
                            {
                                // Try and get a little more to strip this off some.
                                if (address != null)
                                {
                                    // Strip the zip off, that is way too much info.
                                    string zip = Json.TryGetJsonProperty(address, "postalCode");
                                    if (zip != null)
                                    {
                                        name = name.Replace(zip, string.Empty);
                                    }
                                }
                            }

                            if (address != null)
                            {
                                locality = Json.TryGetJsonProperty(address, "locality");
                                state = Json.TryGetJsonProperty(address, "adminDistrict");
                            }

                            _viewModel.LastKnownCity = locality;
                            _viewModel.LastKnownState = state;

                            if (string.IsNullOrEmpty(name) && locality != null)
                            {
                                name = locality;
                            }

                            if (name == null)
                            {
                                name = _asCoordinates;
                            }

                            Location = name;
                            break;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // note: silent watson?
                Location = _asCoordinates; // string.Empty;
            }

            if (callback != null)
            {
                callback(_lastGeocode);
            }
        }
    }
}
