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

using System.Windows.Navigation;
using AgFx;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using JeffWilcox.Controls;
using System;

namespace JeffWilcox.FourthAndMayor.Views
{
    public partial class ProfileLeaderboard : PhoneApplicationPage
    {
        public ProfileLeaderboard()
        {
            InitializeComponent();
        }

        private void OnAppBarItemClick(object sender, EventArgs e)
        {
            var abib = (IApplicationBarMenuItem)sender;
            // go home.
            NavigationService.Navigate(new Uri("/Views/MainPage.xaml", UriKind.Relative));
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string id = string.Empty;
            if (!NavigationContext.QueryString.TryGetValue("id", out id))
            {
                id = "self";
            }

            string s;
            if (NavigationContext.QueryString.TryGetValue("showHome", out s))
            {
                if (s == "true" || s == "True")
                {
                    var ab = ThemeManager.CreateApplicationBar();
                    AppBarHelper.AddButton(ab, "home", OnAppBarItemClick, "/Images/AB/appbar.home.png");
                    ApplicationBar = ab;
                }
            }

            DataContext = DataManager.Current.Load<Model.User>(id);
        }
    }
}
