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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AgFx;
using JeffWilcox.Controls;
using JeffWilcox.FourthAndMayor.Model;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Controls.Maps;
using Microsoft.Phone.Controls.Maps.Core;
using Microsoft.Phone.Shell;

namespace JeffWilcox.FourthAndMayor.FriendsMap
{
    public partial class NearbyFriendsMap : PhoneApplicationPage, ITransitionCompleted
    {
        public NearbyFriendsMap()
        {
            InitializeComponent();
        }

        private Map _map;

        private MapLayer _layer;

        private Pushpin _gpsPushpin;

        private Checkins _checkins;

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            LocationAssistant.Instance.WalkingPositionChanged += OnWalkingPositionChanged;

            _map = new Map();
            _map.ScaleVisibility = System.Windows.Visibility.Visible;
            _map.CopyrightVisibility = System.Windows.Visibility.Collapsed;
            LayoutRoot.Children.Add(_map);

            _map.MapZoom += _map_MapZoom;
            _map.MapResolved += _map_MapResolved;
            _map.MapPan += _map_MapPan;

            IAppInfo iai = Application.Current as IAppInfo;
            if (iai != null)
            {
                _map.CredentialsProvider = new ApplicationIdCredentialsProvider(iai.BKey);
            }

            if (_layer != null && _map.Children.Contains(_layer))
            {
                _map.Children.Remove(_layer);
                _layer = null;
            }

            _layer = new MapLayer();
            _map.Children.Add(_layer);

            var loc = LocationAssistant.Instance.LastKnownLocation.AsGeoCoordinate();
            _map.SetView(loc, 14);

            if (_gpsPushpin == null)
            {
                _gpsPushpin = new Pushpin();
                _gpsPushpin.Location = LocationAssistant.Instance.LastKnownLocation.AsGeoCoordinate();
                var cs = LayoutRoot.Resources["MePushpinStyle"] as Style;
                _gpsPushpin.Style = cs;
                _layer.Children.Add(_gpsPushpin);
            }

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

            //SetupAppBar();

            if (_checkins != null)
            {
                Update(_checkins);
            }
            else
            {
                _checkins = DataManager.Current.Load<Checkins>("Checkins", OnCompleted, OnError);
            }
        }

        private void OnCompleted(Checkins checkins)
        {
            Update(checkins);
        }

        void _map_MapPan(object sender, MapDragEventArgs e)
        {
            // Nothing for now...
        }

        void _map_MapResolved(object sender, EventArgs e)
        {
            UpdateFriendsOnMap();
        }

        void _map_MapZoom(object sender, MapZoomEventArgs e)
        {
            UpdateFriendsOnMap();
        }

        private void UpdateFriendsOnMap()
        {
            if (_checkins != null)
            {
                Update(_checkins);
            }
        }

        private void Update(Checkins checkins)
        {
            if (_map != null && _map.BoundingRectangle != null)
            {
                if (_map.BoundingRectangle.Width < .0001)
                {
                    // hasn't actually loaded yet.
                    IntervalDispatcher.BeginInvoke(TimeSpan.FromSeconds(.25),
                        () => Update(checkins));
                    return;
                }
            }
            else
            {
                // should never happen for real.
                return;
            }

            // 3 hours ago.
            DateTime threeHoursAgo = DateTime.Now - TimeSpan.FromHours(3);

            List<Checkin> nearby = new List<Checkin>();
            foreach (var x in checkins.Groups)
            {
                foreach (Checkin c in x)
                {
                    if (IsNearby(c) && c.CreatedDateTime >= threeHoursAgo)
                    {
                        nearby.Add(c);
                    }
                }
            }

            // Group by Dictionary<VenueId, List<...>>
            // Store venues as well in Dictionary<VenueId, Venue>
            Dictionary<string, List<Checkin>> groupedByVenue = new Dictionary<string, List<Checkin>>();
            Dictionary<string, CompactVenue> venues = new Dictionary<string, CompactVenue>();

            foreach (var entry in nearby)
            {
                if (!venues.ContainsKey(entry.Venue.VenueId))
                {
                    venues[entry.Venue.VenueId] = entry.Venue;
                    groupedByVenue[entry.Venue.VenueId] = new List<Checkin>();
                }

                var list = groupedByVenue[entry.Venue.VenueId];
                if (list != null)
                {
                    list.Add(entry);
                }
            }

            // Clear the map.
            var pinsToRemove = new List<UIElement>();
            foreach (var child in _layer.Children)
            {
                if (child != _gpsPushpin)
                {
                    pinsToRemove.Add(child);
                }
            }

            foreach (var pin in pinsToRemove)
            {
                _layer.Children.Remove(pin);
            }

            _currentPins.Clear();

            // Add the newly grouped entries.
            foreach (var group in groupedByVenue)
            {
                var venue = venues[group.Key];
                if (venues != null && group.Value != null && group.Value.Count > 0)
                {
                    var pin = new Pushpin();
                    pin.DataContext = venue;
                    pin.Location = venue.Location.AsGeoCoordinate();
                    pin.Content = group.Value[0].DisplayUser;

                    // Color the pin if it's the user themselves.
                    bool isSelf = false;
                    var firstPerson = group.Value[0];
                    foreach (var person in group.Value)
                    {
                        if (person.User.IsSelf)
                        {
                            isSelf = true;
                            firstPerson = person;
                            break;
                        }
                    }
                    if (isSelf)
                    {
                        pin.Background = (Brush)Application.Current.Resources["PhoneAccentBrush"];
                    }

                    Grid nestingGrid = null;

                    if (group.Value.Count > 1)
                    {
                        // Add other offset images first.
                        nestingGrid = new Grid();
                        int count = 0;

                        foreach (var person in group.Value)
                        {
                            if (person != firstPerson)
                            {
                                ++count;

                                /*Border b = new Border { Height = 48, Width = 48 };
                                b.Background = new SolidColorBrush(Colors.White);
                                b.BorderBrush = new SolidColorBrush(Colors.Black);
                                b.BorderThickness = new Thickness(2);
                                b.RenderTransform = new TranslateTransform { X = count * 6, Y = count * -6 };*/

                                var b = new Image();
                                b.Source = new System.Windows.Media.Imaging.BitmapImage(person.User.Photo);
                                b.MaxWidth = 24;
                                b.MaxHeight = 24;
                                b.Stretch = System.Windows.Media.Stretch.Uniform;
                                b.RenderTransform = new TranslateTransform { X = 42 + (count * 4), Y = 24 + (count * -4) };

                                nestingGrid.Children.Add(b);
                            }
                        }
                    }

                    var pinImage = new Image();
                    pinImage.Source = new System.Windows.Media.Imaging.BitmapImage(firstPerson.User.Photo);
                    pinImage.Width = 48;
                    pinImage.Height = 48;
                    pinImage.Stretch = System.Windows.Media.Stretch.Fill; // will mess with the aspect ratio a little.
                    pinImage.Margin = new Thickness(2);

                    HyperlinkButton hb = new HyperlinkButton();
                    hb.NavigateUri = venue.VenueUri;
                    hb.Style = Application.Current.Resources["NoHyperlink"] as Style;
                    
                    if (nestingGrid != null)
                    {
                        nestingGrid.Children.Add(pinImage);
                        hb.Content = nestingGrid;
                        //pin.Content = nestingGrid;
                    }
                    else
                    {
                        hb.Content = pinImage;
                        //pin.Content = pinImage;
                    }

                    pin.Content = hb;

                    //pin.DataContext = venue;
                    //pin.ContentTemplate = LayoutRoot.Resources["VenuePinDataTemplate"] as DataTemplate;

                    _layer.Children.Add(pin);
                    _currentPins.Add(pin, venue);
                }
            }
        }

        // probably don't really need to track this
        private Dictionary<UIElement, CompactVenue> _currentPins = new Dictionary<UIElement, CompactVenue>();

        private bool IsNearby(Checkin c)
        {
            if (_map != null && _map.BoundingRectangle != null && c.Venue != null && c.Venue.Location != null)
            {
                return _map.BoundingRectangle.Intersects(
                    new LocationRect(
                        c.Venue.Location.AsGeoCoordinate(), 
                        
                        // Using the actual size of the map to get "nearby"
                        // locations that the user might quickly pan to as
                        // well.
                        _map.BoundingRectangle.Width, 
                        _map.BoundingRectangle.Height)
                    );
            }

            return false;
        }

        private void OnError(Exception e)
        {
            MessageBox.Show("Unfortunately the map could not be prepared at this time.");
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

        private void SetupAppBar()
        {
            ApplicationBar = ThemeManager.CreateApplicationBar();
            ApplicationBar.Opacity = .65;

            AppBarHelper.AddButton((ApplicationBar)ApplicationBar, "me", OnAppBarItemClick, "/Images/AB/map.center.png");

            ApplicationBar.IsVisible = true;
        }

        private void OnAppBarItemClick(object sender, EventArgs e)
        {
            var abib = (IApplicationBarMenuItem)sender;
            switch (abib.Text)
            {
                case "me":
                    _map.SetView(
                        LocationAssistant.Instance.LastKnownLocation.AsGeoCoordinate(),
                        _map.ZoomLevel);
                    break;
            }
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            LocationAssistant.Instance.WalkingPositionChanged -= OnWalkingPositionChanged;

            try
            {
                _map.MapZoom -= _map_MapZoom;
                _map.MapResolved -= _map_MapResolved;
                _map.MapPan -= _map_MapPan;

                _map.Children.Remove(_layer);
                _layer = null;

                _gpsPushpin = null;

                // Memory leak fix for map control.
                _map.SetMode(new NullMode(), false);

                // working around some horrible bugs. UGH.
                LayoutRoot.Children.Remove(_map);
                _map = null;
            }
            catch
            {
            }
        }

        public void OnTransitionCompleted()
        {
            SetupAppBar();
        }

        public void OnTransitionGoodbyeTemporary()
        {
            ApplicationBar = null;
        }
    }
}