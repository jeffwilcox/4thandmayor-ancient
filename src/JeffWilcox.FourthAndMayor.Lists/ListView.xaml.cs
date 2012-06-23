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
using System.Windows.Controls.Primitives;
using AgFx;
using JeffWilcox.Controls;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace JeffWilcox.FourthAndMayor.Lists
{
    public partial class ListView : PhoneApplicationPage,
        ITransitionCompleted
    {
        private Model.List _list;
        private string _id;

        public ListView()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string id;
            if (NavigationContext.QueryString.TryGetValue("id", out id))
            {
                _id = id;
                _list = DataManager.Current.Load<Model.List>(
                    new Model.ListLoadContext(id),
                    OnListLoaded,
                    OnListLoadFailed);
                DataContext = _list;
            }
        }

        private void OnListLoaded(Model.List list)
        {
            UpdateAppBar();
        }

        private void OnListLoadFailed(Exception ex)
        {
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        public void OnTransitionCompleted()
        {
            UpdateAppBar();
        }

        public void OnTransitionGoodbyeTemporary()
        {
            ApplicationBar = null;
        }

        private void UpdateAppBar()
        {
            if (_followingAppBar == null)
            {
                _followingAppBar = ThemeManager.CreateApplicationBar();
                //_unfollow = AppBarHelper.AddButton(_followingAppBar, "unfollow", OnAppBarItemClick, "tbd");
                
                // punted from v3.0!
                //AppBarHelper.AddButton(_followingAppBar, "share", OnAppBarItemClick, "tbd");

                AppBarHelper.AddButton(_followingAppBar, "refresh", OnAppBarItemClick, AppBarIcon.Refresh);
            }

            if (_notFollowingAppBar == null)
            {
                _notFollowingAppBar = ThemeManager.CreateApplicationBar();
                //_follow = AppBarHelper.AddButton(_notFollowingAppBar, "follow", OnAppBarItemClick, "tbd");

                // punted from v3.0!
                //AppBarHelper.AddButton(_notFollowingAppBar, "share", OnAppBarItemClick, "tbd");

                AppBarHelper.AddButton(_notFollowingAppBar, "refresh", OnAppBarItemClick, AppBarIcon.Refresh);
            }

            var dc = DataContext as Model.List;
            if (dc != null && dc.IsLoadComplete)
            {
                // Only restore if it has already loaded.
                ApplicationBar = dc.IsFollowing ? _followingAppBar : _notFollowingAppBar;
            }
            else
            {
                ApplicationBar = null;
            }
        }

        private ApplicationBar _followingAppBar;
        private ApplicationBar _notFollowingAppBar;

        //private IApplicationBarMenuItem _follow;
        //private IApplicationBarMenuItem _unfollow;

        private void OnAppBarItemClick(object sender, EventArgs e)
        {
            var abib = (IApplicationBarMenuItem)sender;
            switch (abib.Text)
            {
                case "share":
                    break;

                case "refresh":
                    object o = DataContext;
                    var dc = o as Model.List;
                    if (dc != null)
                    {
                        abib.IsEnabled = false;
                        DataManager.Current.Refresh<Model.List>(dc.LoadContext,
                            (res) =>
                            {
                                abib.IsEnabled = true;
                                UpdateAppBar();
                            },
                            (exp) =>
                            {
                                abib.IsEnabled = true;
                                UpdateAppBar();
                            }
                            );
                    }

                    break;
            }
        }

        private void UpdateListState(bool shouldFollow, ButtonBase button)
        {
            if (_list != null)
            {
                var list = _list;

                if (button != null)
                {
                    button.IsEnabled = false;
                }

                FourSquare.Instance.UpdateListFollowingState(list.Id, shouldFollow,
                    () =>
                    {
                        DataManager.Current.Refresh<Model.List>(list.LoadContext,
                            (cmpl) =>
                            {
                                DispatchReenable(button);
                                UpdateUserLists();
                            },
                            (fail2) =>
                            {
                                DispatchReenable(button);
                            });
                    },
                    (fail) =>
                    {
                        DispatchReenable(button);
                    });
            }
        }

        private void UpdateUserLists()
        {
            // Refresh their lists for a better UI experience.
            IntervalDispatcher.BeginInvoke(TimeSpan.FromSeconds(1.5), 
                () => DataManager.Current.Refresh<Model.UserLists>(new LoadContext("self"), null, null));
        }

        private void DispatchReenable(ButtonBase button)
        {
            if (button != null)
            {
                Dispatcher.BeginInvoke(() => button.IsEnabled = true);
            }
        }

        private void OnButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            var abib = (ButtonBase)sender;
            var tag = abib.Tag as string;
            if (tag != null)
            {
                switch (tag)
                {
                    case "follow":
                        UpdateListState(true, abib);
                        break;

                    case "unfollow":
                        UpdateListState(false, abib);
                        break;
                }
            }
        }
    }
}
