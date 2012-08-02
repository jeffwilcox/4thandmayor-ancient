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

namespace JeffWilcox.FourthAndMayor.Lists
{
    public partial class AddNewList : PhoneApplicationPage, ITransitionCompleted
    {
        public AddNewList()
        {
            InitializeComponent();
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
            if (_appBar == null)
            {
                _appBar = ThemeManager.CreateApplicationBar();
                _save = AppBarHelper.AddButton(_appBar, "save", OnAppBarItemClick, AppBarIcon.Save);

                ApplicationBar = _appBar;
            }

            ApplicationBar = _appBar;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode == System.Windows.Navigation.NavigationMode.New)
            {
                NewlyCreatedList = null;
            }
        }

        private ApplicationBar _appBar;

        private IApplicationBarMenuItem _save;

        private void OnAppBarItemClick(object sender, EventArgs e)
        {
            var abib = (IApplicationBarMenuItem)sender;
            switch (abib.Text)
            {
                case "save":
                    if (!string.IsNullOrEmpty(_title.Text))
                    {
                        var title = _title.Text;
                        var description = _description.Text;
                        var collaborative = false; // _isCollaborative.IsChecked == true;

                        FourSquare.Instance.CreateNewList(
                            title,
                            description,
                            collaborative,
                            OnListCreateSuccess,
                            OnListCreateFailure);
                    }
                    break;
            }
        }

        public static Model.CompactList NewlyCreatedList { get; set; }

        private void OnListCreateSuccess(Model.CompactList list)
        {
            NewlyCreatedList = list;

            _refreshToken = CentralStatusManager.Instance.BeginLoading("Preparing your list");

            IntervalDispatcher.BeginInvoke(TimeSpan.FromSeconds(1.0), () =>
                {
                    var temporary = DataManager.Current.Refresh<Model.UserLists>(
                        new LoadContext("self"),
                        OnListsLoaded,
                        OnListsFailed);
                });
        }

        private StatusToken _refreshToken;

        private void OnListsLoaded(Model.UserLists uls)
        {
            PriorityQueue.AddUiWorkItem(() =>
                {
                    StatusToken.TryComplete(ref _refreshToken);

                    NavigationService.GoBack();
                });
        }

        private void OnListsFailed(Exception ex)
        {
            // Didn't refresh but we still at least know we have the venue.
            PriorityQueue.AddUiWorkItem(() =>
            {
                StatusToken.TryComplete(ref _refreshToken);

                MessageBox.Show("Your new list was created, however it won't be available immediately.", "New list", MessageBoxButton.OK);
                NavigationService.GoBack();
            });
        }

        private void OnListCreateFailure(Exception ex)
        {
            PriorityQueue.AddUiWorkItem(() =>
                {
                    MessageBox.Show("Unfortunately the list could not be created right now. Please try again later or use the foursquare.com web site.", "error adding list", MessageBoxButton.OK);
                });
        }
    }
}