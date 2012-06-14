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
using AgFx;
using JeffWilcox.Controls;
using JeffWilcox.FourthAndMayor.Model;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace JeffWilcox.FourthAndMayor.NotificationsUserInterface
{
    public partial class LatestNotifications : PhoneApplicationPage, ITransitionCompleted
    {
        public LatestNotifications()
        {
            InitializeComponent();

            AppBars();
        }

        public void OnTransitionCompleted()
        {
            // ApplicationBar = _bar;
        }

        public void OnTransitionGoodbyeTemporary()
        {
            ApplicationBar = null;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            var data = DataManager.Current.Load<Updates>(
                new UpdatesLoadContext("self"),
                (updatesInstance) =>
                {
                    DataContext = updatesInstance;

                    MarkRead(updatesInstance);
                },
                    (updatesFailed) =>
                    {

                    });

            base.OnNavigatedTo(e);
        }

        private void MarkRead(Updates updatesInstance)
        {
            bool hasUnread = false;
            if (updatesInstance != null)
            {
                foreach (var item in updatesInstance.LatestUpdates)
                {
                    if (item != null && item.IsUnread)
                    {
                        hasUnread = true;
                    }
                }

                if (hasUnread && updatesInstance.HighWatermark > 0)
                {
                    // Mark as read for next time.
                    JeffWilcox.FourthAndMayor.FourSquare.Instance.SetNotificationsHighWatermark(
                        updatesInstance.HighWatermark,
                        null,
                        null);
                }
            }
        }

        private ApplicationBar _bar;

        private void AppBars()
        {
            _bar = ThemeManager.CreateApplicationBar();

            var btn1 = AppBarHelper.AddButton(
                _bar,
                "refresh",
                OnAppBarItemClick,
                AppBarIcon.Refresh);
        }

        private void OnAppBarItemClick(object sender, EventArgs e)
        {
            var abib = (IApplicationBarMenuItem)sender;
            switch (abib.Text)
            {
                case "refresh":
                    object o = DataContext;
                    var dc = o as Updates;
                    if (dc != null)
                    {
                        abib.IsEnabled = false;
                        DataManager.Current.Refresh<Updates>(dc.LoadContext,
                            (res) =>
                            {
                                abib.IsEnabled = true;
                                MarkRead(res);
                            },
                            (exp) =>
                            {
                                abib.IsEnabled = true;
                            }
                            );
                    }

                    break;
            }
        }
    }
}