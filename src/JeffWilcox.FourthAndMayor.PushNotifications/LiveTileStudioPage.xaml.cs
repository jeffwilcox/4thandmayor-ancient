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
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using AgFx;
using JeffWilcox.Controls;
using JeffWilcox.FourthAndMayor.Model;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace JeffWilcox.FourthAndMayor.PushNotifications
{
    public partial class LiveTileStudioPage : PhoneApplicationPage, INotifyPropertyChanged
    {
        private List<TileInformation> _shortcuts;
        public List<TileInformation> ShortcutTiles 
        {
            get { return _shortcuts; }
            set
            {
                _shortcuts = value;
                OnPropertyChanged("ShortcutTiles");
            }
        }

        private string _selectedMainIcon;
        public string SelectedMainIcon
        {
            get { return _selectedMainIcon; }
            set
            {
                _selectedMainIcon = value;
                OnPropertyChanged("SelectedMainIcon");
            }
        }

        private List<TileInformation> _friends;
        public List<TileInformation> FriendTiles
        {
            get { return _friends; }
            set
            {
                _friends = value;
                OnPropertyChanged("FriendTiles");
            }
        }

        private List<TileInformation> _places;
        public List<TileInformation> PlaceTiles
        {
            get { return _places; }
            set
            {
                _places = value;
                OnPropertyChanged("PlaceTiles");
            }
        }

        private List<TileInformation> standardShortcuts;

        private readonly Uri CheckinPageUri = new Uri("/Views/CheckinPage.xaml", UriKind.Relative);
        private readonly Uri QRCheckinPageUri = new Uri("/JeffWilcox.FourthAndMayor.QR;component/Scan.xaml", UriKind.Relative);
        private readonly Uri LeaderboardUri = new Uri("/JeffWilcox.FourthAndMayor.Profile;component/ProfileLeaderboard.xaml?showHome=true", UriKind.Relative);
        private readonly Uri HomepageUri = new Uri("/", UriKind.Relative);

        public LiveTileStudioPage()
        {
            InitializeComponent();

            standardShortcuts = new List<TileInformation>
            {
                new TileInformation
                {
                    NavigationUri = CheckinPageUri,
                    Title = string.Empty,
                    BackgroundImage = new Uri("/Images/CheckInNow.png", UriKind.Relative),

                    UnderTitle = "CHECK-IN NOW",
                },

                new TileInformation
                {
                    NavigationUri = QRCheckinPageUri,
                    Title = string.Empty,
                    BackgroundImage = new Uri("/Images/QRCheckInNow173.png", UriKind.Relative),

                    UnderTitle = "QR CODE CHECK-IN",
                },

                new TileInformation
                {
                    NavigationUri = LeaderboardUri,
                    Title = string.Empty,
                    BackgroundImage = new Uri("/Images/Leaderboard.png", UriKind.Relative),

                    UnderTitle = "LIVE LEADERBOARD"
                },
            };
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            // Rename an existing tile.
            if (RenameTile.TitleReturnValue != null && RenameTile.TileUri != null)
            {
                RenameExistingTile(RenameTile.TileUri, RenameTile.TitleReturnValue);
            }

            if (VenuePhotoPicker.PhotoUriReturnValue != null && VenuePhotoPicker.TileUri != null)
            {
                ReplaceExistingTilePhoto(VenuePhotoPicker.TileUri, VenuePhotoPicker.PhotoUriReturnValue);
            }

            base.OnNavigatedTo(e);

            FindTiles();

            DataContext = this;
        }

        private void RenameExistingTile(Uri tileUri, string newTitle)
        {
            AppTileSettings.TileSettings ts;
            if (AppTileSettings.Instance.Tiles.TryGetValue(tileUri, out ts))
            {
                System.Diagnostics.Debug.WriteLine("Renaming tile {0} to {1}.", ts.Title, newTitle);

                ts.Title = newTitle;
                AppTileManager.Instance.UpdateOrPin(ts);

                RenameTile.TitleReturnValue = null;
                RenameTile.TileUri = null;
            }
        }

        private void ReplaceExistingTilePhoto(Uri tileUri, Uri newPhotoUri)
        {
            AppTileSettings.TileSettings ts;
            if (AppTileSettings.Instance.Tiles.TryGetValue(tileUri, out ts))
            {
                System.Diagnostics.Debug.WriteLine("Replacing tile photo...");

                ts.FrontPhoto = PinVenueToStartHelper.GetTileResizedUri(newPhotoUri);
                ts.ShellFrontPhotoPath = null; // reset manually. ick.
                AppTileManager.Instance.UpdateOrPin(ts);

                RenameTile.TitleReturnValue = null;
                RenameTile.TileUri = null;
            }
        }

        private void UpgradeTile(Uri navigationUri)
        {
            if (_stopUpgrading)
            {
                return;
            }

            bool wasUpgraded = false;
            if (navigationUri == HomepageUri)
            {
                // The primary app tile. Easy.
                AppTileSettings.Instance.Tiles[navigationUri] =
                                (new TileInformation
                                {
                                    NavigationUri = navigationUri,
                                    Title = "4th & Mayor",
                                    BackgroundImage = new Uri("/4thBackground_173.png", UriKind.Relative)
                                })
                                .TileSettingsInstance;
                AppTileSettings.Instance.Save();
            }
            else if (navigationUri == CheckinPageUri)
            {
                var entry = standardShortcuts.Where(s => s.NavigationUri == navigationUri).SingleOrDefault();
                if (entry != null)
                {
                    AppTileSettings.Instance.Tiles[navigationUri] = entry.TileSettingsInstance;
                    AppTileSettings.Instance.Save();

                    wasUpgraded = true;
                }
            }
            else
            {
                var a = navigationUri.ToString();
                const string VenueStarts = "/Views/Venue.xaml?";
                
                // PLACE.
                if (a.StartsWith(VenueStarts))
                {
                    string query = a.Replace(VenueStarts, string.Empty);
                    if (query != null)
                    {
                        var queryString = new Dictionary<string,string>();
                        PhoneHyperlinkButton.ParseQueryStringToDictionary(query, queryString);
                        UpgradePlace(navigationUri, queryString);
                    }
                }
            }

            // A little recursive.
            if (wasUpgraded)
            {
                System.Diagnostics.Debug.WriteLine("An upgrade completed.");
                FindTiles();
            }
        }

        private bool _stopUpgrading;

        #region Upgrade a place

        private void UpgradePlace(Uri navigationUri, Dictionary<string, string> queryString)
        {
            if (!_stopUpgrading)
            {
                string id;
                if (queryString.TryGetValue("id", out id))
                {
                    // don't want more than one prompt.
                    _stopUpgrading = true;

                    Model.Venue venue = DataManager.Current.Load<Model.Venue>(new LoadContext(id),
                        (venu) =>
                        {
                            Dispatcher.BeginInvoke(() =>
                                {
                                    var result = MessageBox.Show(
                                        string.Format(CultureInfo.CurrentCulture,
                                        "This new tile management feature needs to upgrade your existing tile for {0} to be shown here. Is it OK to remove it and re-pin it now?" + Environment.NewLine + Environment.NewLine + "You can press the Back button after it is pinned again to make changes to your tiles.",
                                        venu.Name),
                                        "Re-pin " + venu.Name,
                                        MessageBoxButton.OKCancel);
                                    if (result == MessageBoxResult.OK)
                                    {
                                        PinVenueToStartHelper.PinToStartVenue(venu, navigationUri);
                                    }
                                });
                        },
                        (exc) =>
                        {
                            var x = exc;
                            // ?
                        });
                }
            }
        }

        #endregion

        private bool _isFindingTiles;

        private void FindTiles()
        {
            if (_isFindingTiles)
            {
                IntervalDispatcher.BeginInvoke(TimeSpan.FromSeconds(0.5), FindTiles); 
                return;
            }
            _isFindingTiles = true;

            foreach (var item in standardShortcuts)
            {
                item.IsPinned = false; // clear that state.
            }

            List<TileInformation> places = new List<TileInformation>();
            List<TileInformation> friends = new List<TileInformation>();

            var knownTiles = AppTileSettings.Instance.Tiles;

            foreach (var tile in ShellTile.ActiveTiles)
            {
                var tileNavigationUri = tile.NavigationUri;

                var tileSetting = knownTiles.Values.Where(v => v.NavigationUri == tileNavigationUri).SingleOrDefault();
                if (tileSetting == null)
                {
                    // We need to upgrade this tile.
                    UpgradeTile(tileNavigationUri);

                    // Skip the upgrade, once the upgrade is done this code
                    // should run again.
                    continue;
                }

                foreach (var standard in standardShortcuts)
                {
                    if (tile.NavigationUri == standard.NavigationUri)
                    {
                        standard.IsPinned = true;

                        if (tileSetting != null)
                        {
                            standard.OverwriteWithTileSettings(tileSetting);
                        }
                        break;
                    }
                }

                // Not a standard shortcut.
                var asString = tile.NavigationUri.ToString();

                if (asString == "/")
                {
                    if (!string.IsNullOrEmpty(tileSetting.StyleType))
                    {
                        SelectedMainIcon = tileSetting.StyleType;
                    }
                    else
                    {
                        SelectedMainIcon = null;
                    }
                }
                else if (IsPinnedVenue(tile.NavigationUri))
                {
                    var placeTile = new TileInformation
                    {
                        IsPinned = true,

                        CanChangeTitle = true,
                        CanChangeBackgroundImage = true,

                        Title = asString,
                        NavigationUri = tile.NavigationUri
                    };
                    placeTile.OverwriteWithTileSettings(tileSetting);
                    places.Add(placeTile);
                }
                else if (asString.StartsWith("/Views/Profile.xaml"))
                {
                    var friendTile = new TileInformation
                    {
                        IsPinned = true,

                        Title = asString,
                        NavigationUri = tile.NavigationUri
                    };
                    friendTile.OverwriteWithTileSettings(tileSetting);
                    friends.Add(friendTile);
                }
            }

            if (ShortcutTiles != null)
            {
                ShortcutTiles = new List<TileInformation>(); // bump the ui.
            }
            ShortcutTiles = standardShortcuts;
            PlaceTiles = places.Count > 0 ? places : null;
            FriendTiles = friends.Count > 0 ? friends : null;

            _isFindingTiles = false;
        }

        public static bool IsPinnedVenue(Uri navigationUri)
        {
            return navigationUri.ToString().StartsWith("/Views/Venue.xaml");
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            DataContext = null;

            // So they can continue if they come back.
            _stopUpgrading = false;

            base.OnNavigatedFrom(e);
        }

        /// <summary>
        /// Event raised when a property changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The name of the property that has changed.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            var abb = sender as ButtonBase;
            if (abb != null)
            {
                var dc = abb.DataContext;
                var tag = abb.Tag as string;
                var ti = dc as TileInformation;
                var ts = ti.TileSettingsInstance;

                if (ti != null)
                {
                    switch (tag)
                    {
                        case "pin":
                            if (ti.TileSettingsInstance.NavigationUri.ToString() ==
                                LeaderboardUri.ToString())
                            {
                                MessageBox.Show("If you've enabled push notifications, the leaderboard will come alive in the next hour.");
                                // TODO: Re-send all active URIs to the server.
                            }

                            AppTileManager.Instance.UpdateOrPin(ti.TileSettingsInstance);

                            break;

                        case "unpin":
                            AppTileManager.Instance.Unpin(ti.TileSettingsInstance);
                            FindTiles();

                            break;

                        case "changeTitle":
                            string title = ts.Title;
                            Uri image = ts.FrontPhoto;

                            NavigationService.Navigate(new Uri(
                                string.Format(
                                    CultureInfo.InvariantCulture, 
                                    "/JeffWilcox.FourthAndMayor.PushNotifications;component/RenameTile.xaml?title={0}&uri={1}&tileUri={2}", 
                                    Uri.EscapeDataString(title ?? string.Empty),
                                    image == null ? string.Empty : Uri.EscapeDataString(image.ToString()),
                                    Uri.EscapeDataString(ts.NavigationUri.ToString())
                                ), UriKind.Relative));
                            break;

                        case "changePicture":
                            string mytitle = ts.Title;
                            var qs = new Dictionary<string, string>();
                            PhoneHyperlinkButton.ParseUriToQueryStringDictionary(ts.NavigationUri, qs);
                            string venueId;
                            if (qs.TryGetValue("id", out venueId))
                            {
                                NavigationService.Navigate(new Uri(
                                    string.Format(
                                        CultureInfo.InvariantCulture,
                                        "/JeffWilcox.FourthAndMayor.PushNotifications;component/VenuePhotoPicker.xaml?venueId={3}&title={0}&uri={1}",
                                        Uri.EscapeDataString(string.IsNullOrEmpty(mytitle) ? "place photo" : mytitle),
                                        Uri.EscapeDataString(ts.NavigationUri.ToString()),
                                        null, // image == null ? string.Empty : Uri.EscapeDataString(image.ToString()),
                                        venueId
                                    ), UriKind.Relative));
                            }
                            break;

                        default:
                            break;
                    }
                }
            }
        }

        private void OnUpdateMainTile(object sender, RoutedEventArgs e)
        {
            var bb = (ButtonBase)sender;
            var tag = bb.Tag as string;

            var tileSetting = AppTileSettings.Instance.Tiles[HomepageUri];
            if (tileSetting != null)
            {
                if (tag == "default")
                {
                    tileSetting.StyleType = null;
                }
                else
                {
                    tileSetting.StyleType = tag;
                }

                PushNotificationService.Instance.SetCloudSetting("tile", tag, null, null);

                UpdatePrimaryAppIcon(tileSetting);

                AppTileSettings.Instance.Save();
            }
        }

        private void UpdatePrimaryAppIcon(AppTileSettings.TileSettings ts)
        {
            var type = ts.StyleType;

            if (string.IsNullOrEmpty(type))
            {
                // make sure tile pushes are on.
                ts.FrontPhoto = new Uri("/4thBackground_173.png", UriKind.Relative);

                SelectedMainIcon = null;
            }
            else
            {
                ts.ShellFrontPhotoPath = null;

                if (type == "static")
                {
                    ts.FrontPhoto = new Uri("/4thBackground_173.png", UriKind.Relative);

                    // disable live tile pushes.

                }
                else if (type == "classic")
                    {
                        ts.FrontPhoto = new Uri("/ClassicTile.png", UriKind.Relative);
                        // disable live tiles.

                    }
                else
                {
                    // enable live tile pushes.
                }

                SelectedMainIcon = type;
            }

            AppTileManager.Instance.UpdateOrPin(ts);
        }
    }
}
