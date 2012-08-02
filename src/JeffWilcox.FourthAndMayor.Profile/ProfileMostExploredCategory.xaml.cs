﻿//
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

using AgFx;
using JeffWilcox.FourthAndMayor.Model;
using Microsoft.Phone.Controls;

namespace JeffWilcox.FourthAndMayor.Views
{
    public partial class ProfileMostExploredCategory : PhoneApplicationPage
    {
        public ProfileMostExploredCategory()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string id = string.Empty;
            if (!NavigationContext.QueryString.TryGetValue("id", out id))
            {
                id = "self";
            }

            string category = string.Empty;
            NavigationContext.QueryString.TryGetValue("categoryid", out category);

            string categoryName = string.Empty;
            NavigationContext.QueryString.TryGetValue("categoryname", out categoryName);

            _pivotItem.Header = categoryName;

            var stats = DataManager.Current.Load<Model.UserVenueHistory>(
                new UserAndCategoryLoadContext(id)
                    {
                        CategoryId = category
                    });
            DataContext = stats;
        }
    }
}