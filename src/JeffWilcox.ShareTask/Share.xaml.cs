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

using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;
using System.Globalization;
using System;
using JeffWilcox.Controls;

namespace JeffWilcox.ShareTask
{
    public partial class Share : PhoneApplicationPage
    {
        public Share()
        {
            InitializeComponent();
        }

        public static void NavigateToShareTask(string message, Uri link, string linkTitle, string footer = null)
        {
            Application.Current.GetFrame()
                .Navigate(
                    new Uri(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "/JeffWilcox.ShareTask;component/Share.xaml?message={0}&title={1}&uri={2}&footer={3}",
                            string.IsNullOrEmpty(message) ? string.Empty : Uri.EscapeDataString(message),
                            string.IsNullOrEmpty(linkTitle) ? string.Empty : Uri.EscapeDataString(linkTitle),
                            link == null ? string.Empty : Uri.EscapeDataString(link.ToString()),
                            string.IsNullOrEmpty(footer) ? string.Empty : Uri.EscapeDataString(footer)), 
                    UriKind.Relative));
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (_left)
            {
                LayoutRoot.Opacity = 0;
                Dispatcher.BeginInvoke(NavigationService.GoBack);
            }
            else
            {
                LayoutRoot.Opacity = 1.0;

                DataContext = ShareViewModel.Parse(NavigationContext.QueryString);
            }
        }

        private bool _left;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dc = DataContext as ShareViewModel;
            var tag = ((Button)sender).Tag as string;
            ShareChoice choice = ShareChoice.None;
            if (tag != null && dc != null)
            {
                switch (tag)
                {
                    case "Messaging":
                        choice = ShareChoice.Messaging;
                        break;

                    case "Mail":
                        choice = ShareChoice.Mail;
                        break;

                    case "Zuckerberg":
                        choice = ShareChoice.SocialNetwork;
                        break;
                }

                // Hide this page.
                _left = true;
                LayoutRoot.Opacity = 0.0;

                dc.Show(choice);
            }
        }
    }
}