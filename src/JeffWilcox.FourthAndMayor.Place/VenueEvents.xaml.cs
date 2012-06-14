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
using System.ComponentModel;

namespace JeffWilcox.FourthAndMayor.Place
{
    public partial class VenueEvents : PhoneApplicationPage, INotifyPropertyChanged
    {
        private string _id;
        private string _name;

        public string PageTitle
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged("PageTitle");
            }
        }

        private Model.VenueEvents _events;
        public Model.VenueEvents Events
        {
            get 
            { 
                return _events; 
            }
            set
            {
                _events = value;
                OnPropertyChanged("Events");
            }
        }

        public VenueEvents()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string name = string.Empty;
            NavigationContext.QueryString.TryGetValue("name", out name);
            PageTitle = name.ToUpper();

            DataContext = this;

            string id = string.Empty;
            if (NavigationContext.QueryString.TryGetValue("venueid", out id))
            {
                _id = id;
                var eventing = DataManager.Current.Load<Model.VenueEvents>(new LoadContext(id),
                    (ld) => 
                    {
                        Events = ld;
                    },
                    (err) =>
                    {
                        /*throw new UserIntendedException(
                            "We couldn't download information about the place right now, please try again in a little while.",
                            err);*/
                    });
            }
            else
            {
                throw new InvalidOperationException("No venue ID was specified along with the view model.");
            }
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            DataContext = null;

            base.OnNavigatedFrom(e);
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
