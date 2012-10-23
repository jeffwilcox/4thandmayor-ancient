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
using System.Windows;
using AgFx;
using JeffWilcox.Controls;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;

namespace JeffWilcox.FourthAndMayor.QR
{
    public partial class Scan : PhoneApplicationPage
    {
        public Scan()
        {
            InitializeComponent();
        }

        private void InitAppBars()
        {
            _bar = new Microsoft.Phone.Shell.ApplicationBar();
            AppBarHelper.AddButton(_bar, "report code issue", OnAppBarClick);

            if (_isPinned != null)
            {
                AppBarHelper.AddButton(_bar,
                    _isPinned.Value == true ? "unpin" : "pin to start",
                    OnAppBarClick, _isPinned.Value == true ? AppBarIcon.Unpin : AppBarIcon.Pin);
            }

            ApplicationBar = _bar;
        }

        private ApplicationBar _bar;

        private string _lastCode;

        public void OnTransitionTo()
        {
            UpdateAppBars();
        }

        public void OnTransitionFrom()
        {
            ApplicationBar = null;

            if (_token != null)
            {
                StatusToken.TryComplete(ref _token);
            }
        }
        private void UpdateAppBars()
        {
            bool? wasPinned = _isPinned;
            _isPinned = MangoOnSeven.IsPagePinned(NavigationService.Source);
            if (wasPinned != _isPinned)
            {
                InitAppBars();
            } else if (ApplicationBar == null)
            {
                InitAppBars();
            }
        }

        private void OnAppBarClick(object sender, EventArgs e)
        {
            var abib = (IApplicationBarMenuItem)sender;
            switch (abib.Text)
            {
                case "report code issue":
                    // E-mail the code to myself to investigate.
                    if (_lastCode != null)
                    {
                        EmailComposeTask ect = new EmailComposeTask();
                        ect.Body = "This QR code was not identified as a foursquare place:" + Environment.NewLine + Environment.NewLine
                            + "'" + _lastCode + "'. Can you look into the issue for a future update? Thanks.";
                        ect.To = "4thandmayor@gmail.com";
                        ect.Show();
                    }
                    break;

                case "unpin":
                    AppTileManager.Instance.Unpin(NavigationService.Source);
                    UpdateAppBars();
                    break;

                case "pin to start":
                    AppTileManager.Instance.UpdateOrPin(
                        new AppTileSettings.TileSettings(NavigationService.Source)
                        {
                            FrontPhoto = new Uri("Images/QRCheckInNow173.png", UriKind.Relative),
                            Title = string.Empty,
                        });
                    break;
            }
        }

        private void OnScanComplete(object sender, ScanCompleteEventArgs e)
        {
            var s = e.Result;
            _lastCode = s;

            if (s.LastIndexOf('/') < s.Length - 1 && s.Contains("/"))
            {
                var id = s.Substring(s.LastIndexOf('/') + 1);

                TryVenue(id);
            }
            else
            {
                CentralStatusManager.Instance.BeginShowTemporaryMessage("Not a foursquare QR code.");
            }
        }

        private QRSettings _qrSettings;

        private void TryVenue(string id)
        {
            // Save their setting.
            if (_qrSettings != null)
            {
                _qrSettings.Save();
            }

            var ven = DataManager.Current.Load<Model.Venue>(new LoadContext(id), OnVenueLoaded, OnVenueFailed);
            _token = CentralStatusManager.Instance.BeginShowEllipsisMessage("Validating", true);
        }

        private StatusToken _token;

        private void OnVenueLoaded(Model.Venue venue)
        {
            // Auto-check-in *OR* to go the venue page!
            Dispatcher.BeginInvoke(() =>
                {
                    if (_token != null)
                    {
                        StatusToken.TryComplete(ref _token);
                    }

                    CheckIn(venue);
                });
        }

        private bool _destoyIfBack;
        private bool _wasFirstPage;

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (_wasFirstPage)
            {
                _destoyIfBack = true;
            }

            _scanner.StopScanning();

            // Allow saving without having to commit to checking in, etc.
            if (e.NavigationMode == System.Windows.Navigation.NavigationMode.Back)
            {
                _qrSettings.Save();
            }
        }

        private bool? _isPinned;

        private void CheckIn(Model.Venue venue)
        {
            if (_qrSettings != null && _qrSettings.Auto)
            {
                DoQuickCheckin(venue);
            }
            else
            {
                NavigationService.Navigate(venue.VenueUri);
            }
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            _qrSettings = new QRSettings();
            DataContext = _qrSettings;

            if (_destoyIfBack)
            {
                throw new QuitException();
            }

            _wasFirstPage = MangoOnSeven.IsFirstPage();

            _settings = DataManager.Current.Load<Model.UserSettings>("self");
        }

        private Model.UserSettings _settings;

        private void DoQuickCheckin(Model.Venue venue)
        {
            // HORRIBLE... this code is just copied from Venue.xaml.cs
            string vid = venue.VenueId;

            // TODO: Actually this should show the check-in UI where you enter the information!

            Model.CheckinRequest request = null;

            // TODO: Use their defaults instead of false false!!!

            bool pingFacebook = false;
            bool pingTwitter = false;
            if (_settings != null && _settings.IsLoadComplete)
            {
                pingFacebook = _settings.SendToFacebook;
                pingTwitter = _settings.SendToTwitter;
            }

            request = Model.CheckinRequest.VenueCheckin(venue, pingTwitter, pingFacebook);

            FourSquare.Instance.Checkin(request, (success) =>
            {
                // TODO: This is the check-in powerful stuff!
                // Need to reload the page itself to get the updated "here now"
                Dispatcher.BeginInvoke(() =>
                {
                    IntervalDispatcher.
                        BeginInvoke(
                            TimeSpan.FromSeconds(1.5),
                            () =>
                            {
                                // Refresh the checkins list.
                                DataManager.Current.Refresh<Model.Checkins>("Checkins", null, null);
                            });
                });
            },
                (error) =>
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        MessageBox.Show("We couldn't check you in now, sorry!");
                    });
                });
        }

        private void OnVenueFailed(Exception e)
        {
            Dispatcher.BeginInvoke(() =>
                {
                    if (_token != null)
                    {
                        StatusToken.TryComplete(ref _token);
                    }

                    MessageBox.Show("Doesn't look to be a valid venue tag. You can report this if you think it should work.", "Not a foursquare QR code", MessageBoxButton.OK);

                    _scanner.StartScanning();
                });
        }

        private void OnScanError(object sender, ScanFailureEventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("There was a problem scanning the code.", "QR Code #FAIL", MessageBoxButton.OK);
                });
        }
    }
}
