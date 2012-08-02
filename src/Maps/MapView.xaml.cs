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
using System.Globalization;
using System.Windows;
using JeffWilcox.Controls;
using JeffWilcox.FourthAndMayor;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Controls.Maps;
using Microsoft.Phone.Controls.Maps.Core;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;

namespace Maps
{
    public partial class MapView : PhoneApplicationPage, ITransitionCompleted
    {
        public MapView()
        {
            InitializeComponent();

            _layer = new MapLayer();
            _map.Children.Add(_layer);
        }

        public void OnTransitionCompleted()
        {
            SetupAppBar();
        }

        public void OnTransitionGoodbyeTemporary()
        {
            ApplicationBar = null;
        }

        private MapLayer _layer;

        private Pushpin _placePushpin;

        private Pushpin _gpsPushpin;

        private GeoCoordinate _itemLocation;

        private string _placeName;
        private string _placeAddress;
        private Uri _placeLocalUri;

        public class MapPlaceInformation
        {
            public MapPlaceInformation(string name, string address = null, Uri localUri = null)
            {
                Name = name;
                Address = address;
                LocalUri = localUri;
            }
            public string Name { get; set; }
            public string Address { get; set; }
            public Uri LocalUri { get; set; }
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            LocationAssistant.Instance.WalkingPositionChanged += OnWalkingPositionChanged;

            IAppInfo iai = Application.Current as IAppInfo;
            if (iai != null)
            {
                _map.CredentialsProvider = new ApplicationIdCredentialsProvider(iai.BKey);
            }

            // TODO: TOMBSTONE NICE TO HAVE: Get the center point for tombing'

            string val;
            if (NavigationContext.QueryString.TryGetValue("name", out val))
            {
                _placeName = val;
            }
            if (NavigationContext.QueryString.TryGetValue("address", out val))
            {
                _placeAddress = val;
            }
            if (NavigationContext.QueryString.TryGetValue("localUri", out val))
            {
                Uri uri;
                if (Uri.TryCreate(val, UriKind.Relative, out uri))
                {
                    _placeLocalUri = uri;
                }
            }

            string lat = string.Empty;
            string @long = string.Empty;
            if (NavigationContext.QueryString.TryGetValue("lat", out lat) && NavigationContext.QueryString.TryGetValue("long", out @long))
            {
                var geoCoordinate = new System.Device.Location.GeoCoordinate(double.Parse(lat, CultureInfo.InvariantCulture),  double.Parse(@long, CultureInfo.InvariantCulture));
                _itemLocation = geoCoordinate;

                double zoomLevel = 14; // Default.

                // Try to get a zoom level so that we can show the user their
                // current location plus the place they are looking for.
                var point = LocationAssistant.Instance.LastKnownLiveLocation;
                if (point != null) {
                    zoomLevel = BingMapsHelper.GetZoomLevelShowingPoints(
                        geoCoordinate,
                        LocationAssistant.Instance.LastKnownLiveLocation.AsGeoCoordinate(),
                        210 /* pixels */);
                }

                _map.SetView(geoCoordinate, zoomLevel);

                if (_placePushpin != null && _layer.Children.Contains(_placePushpin))
                {
                    _layer.Children.Remove(_placePushpin);
                    _placePushpin = null;
                }

                _placePushpin = new Pushpin();
                _placePushpin.Location = geoCoordinate;
                _placePushpin.Content = new MapPlaceInformation(_placeName, _placeAddress, _placeLocalUri);
                _placePushpin.ContentTemplate = LayoutRoot.Resources["PlaceInformationDataTemplate"] as DataTemplate;

                if (_gpsPushpin == null)
                {
                    _gpsPushpin = new Pushpin();
                    _gpsPushpin.Location = LocationAssistant.Instance.LastKnownLocation.AsGeoCoordinate();
                    var cs = LayoutRoot.Resources["MePushpinStyle"] as Style;
                    _gpsPushpin.Style = cs;
                    _layer.Children.Add(_gpsPushpin);
                }

                _layer.Children.Add(_placePushpin);
            }

            if (State.ContainsKey(MapModeTombstoneKey))
            {
                _isSatelliteViewOn = (bool)State[MapModeTombstoneKey];
            }

            //SetupAppBar();
            UpdateMode();

            if (Environment.OSVersion.Version.Minor >= 1)
            {
                // mango hack
                var st = typeof(SystemTray);
                var opacity = st.GetProperty("Opacity");
                if (opacity != null)
                {
                    opacity.SetValue(null, 0.65, null);

                    SystemTray.IsVisible = true;
                }
            }
        }

        private void OnWalkingPositionChanged(object sender, WalkingEventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
                {
                if (_gpsPushpin != null) 
                {
                    _gpsPushpin.Location = LocationAssistant.Instance.LastKnownLocation.AsGeoCoordinate();
                }
            });
        }

        private IApplicationBarMenuItem _modeItem;
        private void SetupAppBar()
        {
            ApplicationBar = ThemeManager.CreateApplicationBar();
            ApplicationBar.Opacity = .65;
            
            _modeItem = new ApplicationBarMenuItem();
            _modeItem.Click += _sat_Click;
            // LOCALIZE:
            _modeItem.Text = _isSatelliteViewOn ? "road view" : "satellite view";
            
            ApplicationBar.MenuItems.Add(_modeItem);

            // LOCALIZE:
            AppBarHelper.AddButton((ApplicationBar)ApplicationBar, "me", OnAppBarItemClick, "/Images/AB/map.center.png");
            //AppBarHelper.AddButton((ApplicationBar)ApplicationBar, "directions", OnAppBarItemClick, "/Images/AB/map.directions.png");

            // LOCALIZE:
            AppBarHelper.AddButton((ApplicationBar)ApplicationBar, "open in maps app", OnAppBarItemClick);

            ApplicationBar.IsVisible = true;
        }

        private void OnAppBarItemClick(object sender, EventArgs e)
        {
            var abib = (IApplicationBarMenuItem)sender;
            switch (abib.Text)
            {
                // LOCALIZE:
                case "open in maps app":
                    OpenInBingMaps();
                    break;

                // LOCALIZE:
                case "me":
                    _map.SetView(
                        LocationAssistant.Instance.LastKnownLocation.AsGeoCoordinate(),
                        _map.ZoomLevel);
                    break;
            }
        }

        private void OpenInBingMaps()
        {
            if (_itemLocation != null)
            {
                double zoom = 18.0;

                if (_placePushpin != null && _placePushpin.Content is MapPlaceInformation)
                {
                    var mpi = _placePushpin.Content as MapPlaceInformation;
                    if (mpi != null)
                    {
                        var bing = new BingMapsDirectionsTask();
                        bing.End = new LabeledMapLocation
                        {
                            Label = mpi.Name,
                            Location = _placePushpin.Location
                        };
                        try
                        {
                            bing.Show();
                        }
                        catch
                        {
                            // LOCALIZE:
                            MessageBox.Show("On some phones the map cannot be opened. Your culture may be set to a non-English culture where a known bug exists.", "Sorry!", MessageBoxButton.OK);
                        }
                    }
                }
                else
                {
                    try
                    {
                        MangoOnSeven.TryBingMapsTask(_itemLocation, zoom);
                    }
                    catch (Exception)
                    {
                        // LOCALIZE:
                        MessageBox.Show("On some phones the map cannot be opened. Your culture may be set to a non-English culture where a known bug exists.", "Sorry!", MessageBoxButton.OK);
                    }
                }
            }
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            LocationAssistant.Instance.WalkingPositionChanged -= OnWalkingPositionChanged;

            try
            {
                // Memory leak fix for map control.
                _map.SetMode(new NullMode(), false);
            }
            catch
            {
            }

            base.OnNavigatedFrom(e);

            State[MapModeTombstoneKey] = _isSatelliteViewOn;

            DataContext = null;

            if (Environment.OSVersion.Version.Minor >= 1)
            {
                // mango hack
                var st = typeof(SystemTray);
                var opacity = st.GetProperty("Opacity");
                if (opacity != null)
                {
                    opacity.SetValue(null, 1.0, null);
                }
            }
        }

        private const string MapModeTombstoneKey = "mode";

        private bool _isSatelliteViewOn;

        private void _sat_Click(object sender, EventArgs e)
        {
            _isSatelliteViewOn = !_isSatelliteViewOn;
            UpdateMode();
        }

        private void UpdateMode()
        {
            _map.Mode = _isSatelliteViewOn ? (MapMode)new AerialMode() : (MapMode)new RoadMode();
            if (_modeItem != null)
            {
                // LOCALIZE:
                _modeItem.Text = _isSatelliteViewOn ? "road view" : "satellite view";
            }
        }
    }
}