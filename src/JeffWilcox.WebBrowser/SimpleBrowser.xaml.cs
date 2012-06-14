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
using JeffWilcox.Controls;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace JeffWilcox.WebBrowser
{
    public partial class SimpleBrowser : PhoneApplicationPage
    {
        private ApplicationBar _appBar;

        private IApplicationBarMenuItem _refreshButton;
//        private IApplicationBarMenuItem _stopButton;

        public SimpleBrowser()
        {
            InitializeComponent();
            CreateAppBar();
        }

        private bool _isLoadComplete;

        private Uri _uri;

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            _web.LoadCompleted += OnLoadCompleted;
            _web.Navigated += OnNavigated;
            _web.Navigating += OnNavigating;

            string ur = null;
            if (NavigationContext.QueryString.TryGetValue("uri", out ur))
            {
                var uri = new Uri(ur, UriKind.Absolute);
                if (uri != null)
                {
                    if (_token != null)
                    {
                        StatusToken.TryComplete(ref _token);
                    }
                    _token = CentralStatusManager.Instance.BeginShowEllipsisMessage("Loading information");

                    _uri = uri;
                    if (_isLoadComplete)
                    {
                        _web.Navigate(uri);
                    }
                    else
                    {
                        IntervalDispatcher.BeginInvoke(TimeSpan.FromSeconds(0.75), () => OnLoadCompleted(this, new System.Windows.Navigation.NavigationEventArgs(null, null)));
                    }
                }
            }

            UpdateAppBar();
        }

        private StatusToken _token;

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            _web.LoadCompleted -= OnLoadCompleted;
            _web.Navigated -= OnNavigated;
            _web.Navigating -= OnNavigating;
        }

        void OnNavigating(object sender, NavigatingEventArgs e)
        {

        }

        void OnNavigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            StatusToken.TryComplete(ref _token);
        }

        void OnLoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (_uri != null && !_isLoadComplete)
            {
                Dispatcher.BeginInvoke(() => { _web.Navigate(_uri); });
            }

            _isLoadComplete = true;
        }

        private void CreateAppBar()
        {
            ApplicationBar = _appBar = ThemeManager.CreateApplicationBar();

            _refreshButton = AppBarHelper.AddButton(_appBar, "refresh", OnAppBarItemClick, "/Images/AB/appbar.sync.rest.png");
            _refreshButton.IsEnabled = false;

            // TODO: get refresh working.

            // TODO: stop button.
        }

        private void UpdateAppBar()
        {

        }

        private void OnAppBarItemClick(object sender, EventArgs e)
        {
            var abib = (IApplicationBarMenuItem)sender;
            switch (abib.Text)
            {
                case "refresh":
                    break;
            }
        }
    }
}