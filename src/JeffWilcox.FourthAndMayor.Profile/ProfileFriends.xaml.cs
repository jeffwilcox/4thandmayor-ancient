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
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using AgFx;
using Microsoft.Phone.Shell;
using JeffWilcox.Controls;

namespace JeffWilcox.FourthAndMayor.Views
{
    public partial class ProfileFriends : PhoneApplicationPage
    {
        public ProfileFriends()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            _friendsBar = ThemeManager.CreateApplicationBar();
            AppBarHelper.AddButton(_friendsBar, "add friends", OnAppBarItemClick, "/Images/AB/addpeople.png");

            ApplicationBar = _friendsBar;

            string id = string.Empty;
            if (!NavigationContext.QueryString.TryGetValue("id", out id))
            {
                id = "self";
            }

            bool isSelf = false;
            string selfValue = null;
            if (NavigationContext.QueryString.TryGetValue("self", out selfValue))
            {
                if (selfValue == "1")
                {
                    isSelf = true;
                }
            }

            if (!isSelf)
            {
                _pivot.Items.Remove(_userRequests);
            }

            string req = string.Empty;
            if (isSelf && NavigationContext.QueryString.TryGetValue("requests", out req))
            {
                if (!string.IsNullOrEmpty(req))
                {
                    // go to the requests page when possible!
                    _pivot.SelectedIndex = 1;
                }
            }

            var user = DataManager.Current.Load<Model.User>(id);
            DataContext = user;
        }

        private ApplicationBar _friendsBar;

        private void OnAppBarItemClick(object sender, EventArgs e)
        {
            var abib = (IApplicationBarMenuItem)sender;
            switch (abib.Text)
            {
                case "add friends":
                    NavigationService.Navigate(new Uri("/JeffWilcox.FourthAndMayor.Profile;component/FriendSearchStart.xaml", UriKind.Relative));
                    break;
            }
        }
    }
}