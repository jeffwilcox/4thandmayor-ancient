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
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using JeffWilcox.Controls;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace JeffWilcox.FourthAndMayor.PushNotifications
{
    public partial class RenameTile : PhoneApplicationPage,
        ITransitionCompleted
    {
        public static string TitleReturnValue { get; set; }
        public static Uri TileUri { get; set; }

        public RenameTile()
        {
            InitializeComponent();
        }

        private DispatcherTimer _dt;

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Uri uri;
            string s;

            if (NavigationContext.QueryString.TryGetValue("title", out s))
            {
                _title.Text = s;
                _preview.Text = s;
            }

            if (NavigationContext.QueryString.TryGetValue("tileUri", out s))
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

            if (NavigationContext.QueryString.TryGetValue("uri", out s))
            {
                if (Uri.TryCreate(s, UriKind.RelativeOrAbsolute, out uri))
                {
                    _image.Source = new BitmapImage(uri);
                }
            }

            TitleReturnValue = null;

            _dt = new DispatcherTimer();
            _dt.Interval = TimeSpan.FromSeconds(.1);
            _dt.Tick += OnTick;
            _dt.Start();
        }

        private void OnTick(object sender, EventArgs e)
        {
            _preview.Text = _title.Text;
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (_dt != null)
            {
                _dt.Stop();
                _dt.Tick -= OnTick;
                _dt = null;
            }

            base.OnNavigatedFrom(e);
        }

        private void OnAppBarItemClick(object sender, EventArgs e)
        {
            var abib = (IApplicationBarMenuItem)sender;
            switch (abib.Text)
            {
                case "ok":
                    TitleReturnValue = _title.Text;
                    NavigationService.GoBack();
                    break;

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
                AppBarHelper.AddButton(ab, "ok", OnAppBarItemClick, AppBarIcon.OK);
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
    }
}