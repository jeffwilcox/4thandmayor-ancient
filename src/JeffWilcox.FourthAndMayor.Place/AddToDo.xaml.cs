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
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using AgFx;
using Microsoft.Phone.Controls;

namespace JeffWilcox.FourthAndMayor.Views
{
    public partial class AddToDo : PhoneApplicationPage, INotifyPropertyChanged
    {
        public AddToDo()
        {
            InitializeComponent();
        }

        private string _buttonText;
        public string ButtonText
        {
            get { return _buttonText; }
            set
            {
                _buttonText = value;
                OnPropertyChanged("ButtonText");
            }
        }

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

        private string _address;
        public string Address
        {
            get { return _address; }
            set
            {
                _address = value;
                OnPropertyChanged("Address");
            }
        }

        private string _shoutText;

        public string ShoutText
        {
            get { return _shoutText; }
            set
            {
                _shoutText = value;
                OnPropertyChanged("ShoutText");

                // need manual binding so tomb can grab this.
                if (_shout != null && _shout.Text != _shoutText)
                {
                    _shout.Text = _shoutText ?? string.Empty;
                }
            }
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            DataContext = this;

            base.OnNavigatedTo(e);

            string s;
            if (NavigationContext.QueryString.TryGetValue("name", out s))
            {
                PlaceName = s;
            }

            if (NavigationContext.QueryString.TryGetValue("address", out s))
            {
                Address = s;
            }

            if (NavigationContext.QueryString.TryGetValue("venueid", out s))
            {
                VenueId = s;
            }

            ButtonText = "add to to-do list";

            _shout.KeyDown += OnShoutKeyDown;
            _shout.TextChanged += OnShoutTextChanged;

            _grid.DataContext = this;
        }


        private void OnShoutTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            // for tomb.
            ShoutText = _shout.Text;
        }

        private void OnShoutKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            //if (e.Key == System.Windows.Input.Key.Enter)
            //{
            //    GoShout();
            //}
        }

        private string VenueId { get; set; }

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

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            var b = (Button)sender;
            var t = b.Tag as string;
            if (t == null) return;

            switch (t)
            {
                case "AddToDo":
                    string list = Model.LocalCredentials.Current.UserId + "/todos";
                    string text = string.IsNullOrEmpty(_shoutText) ? null : _shoutText;
                    FourSquare.Instance.UpdateListAddVenue(list, VenueId,
                        () =>
                        {
                            // TODO: refresh the to-do's list the user has...
                            Dispatcher.BeginInvoke(() =>
                                {
                                    NavigationService.GoBack();
                                });
                            },
                        (exr) =>
                            {
                                Dispatcher.BeginInvoke(() =>
                                    MessageBox.Show("Could not add the to-do right now."));
                            },
                            text);
                    break;
            }
        }

    }
}