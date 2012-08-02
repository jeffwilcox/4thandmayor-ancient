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
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Resources;
using System.Windows.Shapes;
using JeffWilcox.Controls;
using JeffWilcox.FourthAndMayor;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;

namespace About
{
    public partial class About : PhoneApplicationPage
    {
        public About()
        {
            InitializeComponent();
#if DEBUG
            //_testing.Visibility = Visibility.Visible;
#endif
        }

        private StackPanel _licenses;

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            bool b;
            _pivot.Restore(State, out b);

            IAppInfo iai = Application.Current as IAppInfo;
            if (iai != null)
            {
                _versionText.Text = iai.Version;
            }

            if (_pivot.SelectedIndex > 0 && _licenses == null)
            {
                BuildLicensing();
            }

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            _pivot.Save(State);

            //if (_licenses != null)
            //{
            //    _licenses.Children.Clear();
            //}
            base.OnNavigatingFrom(e);
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Pivot piv = (Pivot)sender;
            if (piv.SelectedIndex > 0 && sv1.Content == null)
            {
                BuildLicensing();
            }
        }

        private void BuildLicensing()
        {
            Dispatcher.BeginInvoke(() =>
            {
                _licenses = new StackPanel();

                StreamResourceInfo sri = Application.GetResourceStream(
                    new Uri("LICENSE.txt", UriKind.Relative));
                if (sri != null)
                {
                    using (StreamReader sr = new StreamReader(sri.Stream))
                    {
                        string line;
                        bool lastWasEmpty = true;
                        do
                        {
                            line = sr.ReadLine();

                            if (line == string.Empty)
                            {
                                Rectangle r = new Rectangle
                                {
                                    Height = 20,
                                };
                                _licenses.Children.Add(r);
                                lastWasEmpty = true;
                            }
                            else
                            {
                                TextBlock tb = new TextBlock
                                {
                                    TextWrapping = TextWrapping.Wrap,
                                    Text = line,
                                    Style = (Style)Application.Current.Resources["PhoneTextNormalStyle"],
                                };
                                if (!lastWasEmpty)
                                {
                                    tb.Opacity = 0.7;
                                }
                                lastWasEmpty = false;
                                _licenses.Children.Add(tb);
                            }
                        } while (line != null);
                    }
                }

                sv1.Content = _licenses;
            });
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            string s = ((ButtonBase)sender).Tag as string;

            switch (s)
            {
                case "GoToOfficialApp":
                    // Official Foursquare app v2.
                    // http://social.zune.net/redirect?type=phoneApp&id=26cf3302-469f-e011-986b-78e7d1fa76f8
                    var marketplaceDetailTask = new MarketplaceDetailTask();
                    marketplaceDetailTask.ContentIdentifier = "26cf3302-469f-e011-986b-78e7d1fa76f8";
                    marketplaceDetailTask.Show();
                    break;

                case "Review":
                    var task = new MarketplaceReviewTask();
                    task.Show();
                    break;
            }
        }
    }
}