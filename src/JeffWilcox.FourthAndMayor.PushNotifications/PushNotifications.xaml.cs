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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Microsoft.Phone.Controls;

namespace JeffWilcox.FourthAndMayor.PushNotifications
{
    public partial class PushNotifications : PhoneApplicationPage
    {
        public PushNotifications()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (PushNotificationService.Instance.PushUri != null)
            {
                var uu = ((IGenerate4thAndMayorUri)(Application.Current))
                    .Get4thAndMayorUri("/v1/push/status.html?uri=" + PushNotificationService.Instance.PushUri.ToString(), true);

                _statusHyperlink.TargetName = null;
                _statusHyperlink.NavigateUri = new Uri(
                    string.Format("/JeffWilcox.WebBrowser;component/SimpleBrowser.xaml?uri={0}&unique={1}", 
                    System.Net.HttpUtility.UrlEncode(uu.ToString()),
                    (new Guid()).ToString()), 
                    UriKind.Relative);

                _dynamicXaml.ContentReady += OnDynamicContentReady;
                _dynamicXaml.XamlUri = ((IGenerate4thAndMayorUri)(Application.Current))
                    .Get4thAndMayorUri("/v1/push/settings.xaml?uri=" + PushNotificationService.Instance.PushUri.ToString() + "&unique=" +
                    (new Guid()).ToString()
                , true);
            }
            else
            {
                MessageBox.Show("Your app is not currently connected to a push notification channel. Please try again soon or enable them in the app settings.");
                // should protect more.
                Dispatcher.BeginInvoke(() => {
                    try
                    {
                        NavigationService.GoBack();
                    }
                    catch (InvalidOperationException) { // nav already in process 
                    }
                } );
            }
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        private void HookupMagic(UIElement root)
        {
            // Might get leaky!
            foreach (var checkbox in root.GetVisualDescendants().OfType<CheckBox>().ToList())
            {
                if (checkbox.Tag != null || checkbox.Name != null)
                {
                    checkbox.Checked += OnCheckboxChecked;
                    checkbox.Unchecked += OnCheckboxUnchecked;
                }
            }

            foreach (var buttonBase in root.GetVisualDescendants().OfType<ButtonBase>().ToList())
            {
                if (buttonBase.Tag != null && buttonBase.Name != null)
                {
                    string property = buttonBase.Name;
                    string value = buttonBase.Tag as string;

                    buttonBase.Click += OnSpecialButtonClick;
                }
            }
        }

        private void SaveNewValue(string property, string value)
        {
            if (!string.IsNullOrEmpty(property))
            {
                PushNotificationService.Instance.SetCloudSetting(
                    property,
                    value,
                    (ok) =>
                    {
                        if (!string.IsNullOrEmpty(ok))
                        {
                            MessageBox.Show(ok);
                        }
                    },
                        (fail) =>
                        {
                            MessageBox.Show("The value could not be saved, please try again later."
                                + Environment.NewLine
                                + (fail != null ? fail.Message : string.Empty));
                        }
                    );
            }
        }

        private void OnSpecialButtonClick(object sender, RoutedEventArgs e)
        {
            var button = sender as ButtonBase;
            if (button != null)
            {
                string property = button.Name;
                string value = button.Tag as string;

                SaveNewValue(property, value);
            }
        }

        private void OnCheckboxUnchecked(object sender, RoutedEventArgs e)
        {
            UpdateCheckValue(sender);
        }

        private void OnCheckboxChecked(object sender, RoutedEventArgs e)
        {
            UpdateCheckValue(sender);
        }

        private void UpdateCheckValue(object sender)
        {
            CheckBox cb = sender as CheckBox;
            if (cb != null)
            {
                string tag = cb.Tag as string;
                string name = cb.Name as string;

                string keyName = tag ?? name;

                if (!string.IsNullOrEmpty(keyName) && cb.IsChecked.HasValue)
                {
                    string value = cb.IsChecked.Value.ToString().ToLowerInvariant();
                    SaveNewValue(keyName, value);
                }
            }
        }

        private void OnDynamicContentReady(object sender, EventArgs e)
        {
            HookupMagic(_dynamicXaml);
        }
    }
}