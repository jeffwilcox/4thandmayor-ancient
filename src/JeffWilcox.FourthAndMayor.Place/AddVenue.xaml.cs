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
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using AgFx;
using JeffWilcox.Controls;
using Microsoft.Phone.Controls;

namespace JeffWilcox.FourthAndMayor.Views
{
    public partial class AddVenue : PhoneApplicationPage, INotifyPropertyChanged
    {
        private string _name;
        public string PlaceName
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged("PlaceName");
            }
        }

        public Model.Categories Categories { get; private set; }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            ChromeSystemTray.SetSystemTrayToChrome();

            Categories = DataManager.Current.Load<Model.Categories>(new LoadContext("cats"));

            DataContext = this;

            object o;
            if (State.TryGetValue("categoryId", out o))
            {
                _category.Tag = (string) o;
            }
            if (State.TryGetValue("categoryName", out o))
            {
                _category.Content = (string) o;
            }

            // User has finished picking a category now.
            if (!string.IsNullOrEmpty(CategoryPickerPage.SelectedCategoryId))
            {
                _category.Tag = CategoryPickerPage.SelectedCategoryId;
                _category.Content = CategoryPickerPage.SelectedCategoryName;

                CategoryPickerPage.SelectedCategoryId = null;
                CategoryPickerPage.SelectedCategoryName = null;
            }

            _map.PointOfInterest = LocationAssistant.Instance.LastKnownLocation;

            string s;
            if (NavigationContext.QueryString.TryGetValue("name", out s))
            {
                PlaceName = s;
            }

            if (State.TryGetValue("theName", out o))
            {
                PlaceName = (string)o;
            }

            // City, State should be filled out using the geocode...
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (_category.Tag != null)
            {
                State["categoryId"] = _category.Tag as string;
                State["categoryName"] = _category.Content as string;
            }

            if (_name != null)
            {
                State["theName"] = _name;
            }

            base.OnNavigatedFrom(e);
        }

        public AddVenue()
        {
            InitializeComponent();

            if (GeocodeService.Instance.ViewModel.LastKnownCity != null)
            {
                _city.Text = GeocodeService.Instance.ViewModel.LastKnownCity;
            }

            if (GeocodeService.Instance.ViewModel.LastKnownState != null)
            {
                _state.Text = GeocodeService.Instance.ViewModel.LastKnownState;
            }
        }

        /// <summary>
        /// Event raised when a property changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The name of the property that has changed.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri(string.Format(CultureInfo.InvariantCulture, "/JeffWilcox.FourthAndMayor.Place;component/CategoryPickerPage.xaml"), UriKind.Relative));
        }

        private void ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            var d = new Dictionary<string, string>();

            if (string.IsNullOrEmpty(_name))
            {
                MessageBox.Show("You must provide a name for the new place to add.");
                return;
            }

            d["address"] = _address.Text;
            d["crossStreet"] = _crossStreet.Text;
            d["city"] = _city.Text;
            d["state"] = _state.Text;
            d["zip"] = _postcode.Text;
            d["phone"] = _phone.Text;
            d["twitter"] = _twitter.Text;
            d["primaryCategoryId"] = _category.Tag as string;

            FourSquare.Instance.AddVenue(_name, d, OnVenueCreated, OnDuplicatesChallenge, OnVenueCreateFailure);
        }

        public static FourSquare.DuplicateVenueChallenge CurrentChallenge { get; set; }
        
        public void OnDuplicatesChallenge(FourSquare.DuplicateVenueChallenge challenge)
        {
            // This set of pages will definitely be a little buggy because for
            // now it doesn't support tombstoning.

            CurrentChallenge = challenge;
            PriorityQueue.AddUiWorkItem(() =>
                {
                    NavigationService.RemoveBackEntry(); // the add page. NOT this page which will next be removed..
                    NavigationService.Navigate(new Uri("/JeffWilcox.FourthAndMayor.Place;component/AddVenueDuplicates.xaml", UriKind.Relative));
                });
        }

        private void OnVenueCreated(Model.CompactVenue venue)
        {
            if (venue != null)
            {
                FourSquare.Instance.VenueCreationSuccessfulId = venue.VenueId;
            }
            else
            {
                FourSquare.Instance.VenueCreationSuccessfulId = string.Empty; // still move back I suppose.
            }

            Dispatcher.BeginInvoke(() => NavigationService.GoBackWhenReady());
        }

        private void OnVenueCreateFailure(Exception e)
        {
            Dispatcher.BeginInvoke(
                () => MessageBox.Show("Your venue could not be created! Perhaps it already exists?"));
        }
    }
}