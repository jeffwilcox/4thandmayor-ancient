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
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using JeffWilcox.Controls;
using JeffWilcox.FourthAndMayor;
using Microsoft.Phone.Controls;

namespace About
{
    public partial class UpdatedVersionInformation : PhoneApplicationPage
    {
        public UpdatedVersionInformation()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            var ia = Application.Current as IAppInfo;
            if (ia != null)
            {
                string version = TruncateVersion(ia.Version);
                PageTitle.Text = "update " + version;
                _xaml.ContentReady += OnXamlReady;
                _xaml.XamlUri = new Uri(string.Format(CultureInfo.InvariantCulture, "http://www.4thandmayor.com/app/{0}.upgrade.xaml", version), UriKind.Absolute);
            }

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            _xaml.ContentReady -= OnXamlReady;
        }

        private void OnXamlReady(object sender, EventArgs e)
        {
            var page = _xaml.Content as Panel;
            if (page != null)
            {
                var o = page.FindName("_enablePushCheckBox");
                if (o != null)
                {
                    CheckBox cb = o as CheckBox;
                    if (cb != null)
                    {
                        // Data bind.
                        _pushEnabledCheckBox = cb;

                        // Get the original value...
                        var appSettings = Application.Current as JeffWilcox.Controls.IExposeSettings;
                        if (appSettings != null)
                        {
                            var pe = appSettings.PushEnabled;
                            if (pe != null)
                            {
                                cb.IsChecked = pe.PushEnabled;
                            }
                        }
                    }
                }
            }
        }

        private CheckBox _pushEnabledCheckBox;

        private static string TruncateVersion(string version)
        {
            string v = version;

            try
            {
                var vers = new Version(version);
                v = vers.Major + "." + vers.Minor;
            }
            catch (Exception)
            {
                v = version;
            }

            return v;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(() => 
                {
                    NavigationService.GoBackWhenReady();
                });

            if (_pushEnabledCheckBox != null && _pushEnabledCheckBox.IsChecked.HasValue)
            {
                bool value = _pushEnabledCheckBox.IsChecked.Value;

                var appSettings = Application.Current as JeffWilcox.Controls.IExposeSettings;
                if (appSettings != null)
                {
                    var pe = appSettings.PushEnabled;
                    if (pe != null)
                    {
                        pe.PushEnabled = value;

                        SettingsStorage ss = pe as SettingsStorage;
                        if (ss != null)
                        {
                            ss.Save();
                        }
                    }
                }
            }
        }
    }
}