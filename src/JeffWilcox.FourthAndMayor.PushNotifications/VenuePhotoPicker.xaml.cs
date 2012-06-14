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
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using AgFx;
using JeffWilcox.Controls;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace JeffWilcox.FourthAndMayor.PushNotifications
{
    public partial class VenuePhotoPicker : PhoneApplicationPage,
        ITransitionCompleted
    {
        private Model.Venue _venue;
        private string _venueId;

        public static Uri PhotoUriReturnValue { get; set; }
        public static Uri TileUri { get; set; }

        public VenuePhotoPicker()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Uri uri;
            string s;

            if (NavigationContext.QueryString.TryGetValue("title", out s))
            {
                PageTitle.Text = s.ToLower(CultureInfo.CurrentUICulture);
            }

            if (NavigationContext.QueryString.TryGetValue("uri", out s))
            {
                if (Uri.TryCreate(s, UriKind.RelativeOrAbsolute, out uri))
                {
                    TileUri = uri;
                }
            }
            else
            {
                NavigationService.GoBackWhenReady();
            }

            if (NavigationContext.QueryString.TryGetValue("venueId", out _venueId))
            {
                _venue = DataManager.Current.Load<Model.Venue>(
                    new LoadContext(_venueId),
                    OnVenueLoaded,
                    OnVenueLoadFailed
                    );
            }
            else
            {
                NavigationService.GoBackWhenReady();
            }

            PhotoUriReturnValue = null;
        }

        private void OnVenueLoaded(Model.Venue venue)
        {
            List<Model.Photo> photos = new List<Model.Photo>();

            if (venue.PhotoGroups != null)
            {
                foreach (var group in venue.PhotoGroups)
                {
                    photos.AddRange(group);
                }
            }

            // Add our default icon.
            photos.Add(new Model.Photo
            {
             LargerUri = new Uri("/4thBackground_173.png", UriKind.RelativeOrAbsolute),
             Id = new Guid().ToString(), // for icompare.
            }
                );

            DataContext = photos;
        }

        private void OnVenueLoadFailed(Exception ex)
        {
            // ?
        }

        private void OnAppBarItemClick(object sender, EventArgs e)
        {
            var abib = (IApplicationBarMenuItem)sender;
            switch (abib.Text)
            {
                case "cancel":
                    NavigationService.GoBack();
                    break;
            }
        }

        private void UpdateAppBar()
        {
            if (ApplicationBar == null)
            {
                var ab = ThemeManager.CreateApplicationBar();
                // AppBarHelper.AddButton(ab, "ok", OnAppBarItemClick, AppBarIcon.OK);
                AppBarHelper.AddButton(ab, "cancel", OnAppBarItemClick, AppBarIcon.Cancel);

                ApplicationBar = ab;
            }
        }

        public void OnTransitionCompleted()
        {
            UpdateAppBar();
        }

        public void OnTransitionGoodbyeTemporary()
        {
            ApplicationBar = null;
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            var hb = (HyperlinkButton)sender;
            var dc = hb.DataContext;

            var p = dc as Model.Photo;
            if (p != null)
            {
                PhotoUriReturnValue = p.LargerUri;

                NavigationService.GoBack();
            }
        }
    }
}