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

using System.ComponentModel;
using AgFx;
using Microsoft.Phone.Controls;

namespace JeffWilcox.FourthAndMayor.Views
{
    public partial class VenueFriendsHere : PhoneApplicationPage, INotifyPropertyChanged
    {
        public VenueFriendsHere()
        {
            InitializeComponent();
        }

        private string VenueId { get; set; }

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

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string s;
            if (NavigationContext.QueryString.TryGetValue("name", out s))
            {
                PlaceName = s;
            }

            if (NavigationContext.QueryString.TryGetValue("venueid", out s))
            {
                VenueId = s;
            }

            var vv = DataManager.Current.Load<Model.Venue>(VenueId,
                null,
                (err) =>
                {
                    /*throw new UserIntendedException(
                        "We couldn't download information about the place right now, please try again in a little while.",
                        err);*/
                });
            DataContext = vv;

            if (vv.IsLoadComplete == false)
            {
                vv.Name = PlaceName;
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
    }
}
