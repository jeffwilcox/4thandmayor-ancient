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
using JeffWilcox.FourthAndMayor.Model;
using Microsoft.Phone.Controls;

namespace JeffWilcox.FourthAndMayor.Views
{
    public partial class SpecialsNearby : PhoneApplicationPage
    {
        public SpecialsNearby()
        {
            InitializeComponent();
        }

        private void OnAppBarIconClick(object sender, EventArgs e)
        {
            // refresh
            var d = DataContext as IUpdatable;
            if (d != null)
            {
                d.Refresh();
            }
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            var context = new SpecialsSearch(new LimitingLoadContext("nearby"));

            DataContext = DataManager.Current.Load<Model.SpecialsSearch>(context);

            base.OnNavigatedTo(e);
        }
    }
}