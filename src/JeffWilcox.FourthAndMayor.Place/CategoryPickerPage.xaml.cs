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
using AgFx;
using JeffWilcox.Controls;
using JeffWilcox.FourthAndMayor.Model;
using Microsoft.Phone.Controls;

namespace JeffWilcox.FourthAndMayor.Views
{
    public partial class CategoryPickerPage : PhoneApplicationPage
    {
        public static string SelectedCategoryId { get; set; }
        public static string SelectedCategoryName { get; set; }

        public Model.Categories Categories { get; private set; }

        public CategoryPickerPage()
        {
            InitializeComponent();
        }

        private string _root;

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (SelectedCategoryId != null)
            {
                Opacity = 0;
                Dispatcher.BeginInvoke(() => NavigationService.GoBackWhenReady());
            }
            else
            {
                Opacity = 1;
            }

            NavigationContext.QueryString.TryGetValue("root", out _root);

            Categories = DataManager.Current.Load<Model.Categories>(new LoadContext("cats"), OnListReady, (error) => { });
        }

        private void OnListReady(Categories c)
        {
            if (string.IsNullOrEmpty(_root))
            {
                // Very root.
                _list.ItemsSource = c.RootCategories;
                return;
            }

            _notRootText.Visibility = Visibility.Visible;

            Category cat = null;
            if (c.TryGetCategory(_root, out cat))
            {
                _thisCategory = cat;
                _list.ItemsSource = _thisCategory.SubCategories;

                _this.DataContext = cat;
                _this.Visibility = Visibility.Visible;
                _thisText.Visibility = Visibility.Visible;
            }
            else throw new InvalidOperationException();
        }

        private Category _thisCategory;

        private void SelectCategory(Category cc)
        {
            SelectedCategoryId = cc.CategoryId;
            SelectedCategoryName = cc.PluralName;

            Dispatcher.BeginInvoke(() => NavigationService.GoBackWhenReady());
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var item = ((Button) sender).Tag;

            string s = item as string;
            if (s != null && s == "_this_")
            {
                SelectCategory(_thisCategory);
                return;
            }

            Category c = item as Category;
            if (c != null)
            {
                if (c.SubCategories == null || c.SubCategories.Count == 0)
                {
                    // This is a selection!
                    SelectCategory(c);
                }
                else
                {
                    // Navigate to the sub-category.
                    NavigationService.Navigate(new Uri(string.Format(CultureInfo.InvariantCulture, "/JeffWilcox.FourthAndMayor.Place;component/CategoryPickerPage.xaml?root={0}", c.CategoryId), UriKind.Relative));
                }
            }
        }
    }
}