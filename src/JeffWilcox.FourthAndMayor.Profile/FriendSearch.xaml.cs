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

using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AgFx;
using JeffWilcox.FourthAndMayor.Model;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.UserData;

namespace JeffWilcox.FourthAndMayor.Views
{
    public partial class FriendSearch : PhoneApplicationPage, INotifyPropertyChanged
    {
        private PhoneNumberChooserTask _numberPicker;

        private EmailAddressChooserTask _emailPicker;

        private bool _taskResultOk;

        public FriendSearch()
        {
            InitializeComponent();

            _numberPicker = new PhoneNumberChooserTask();
            _numberPicker.Completed += OnNumberPicked;

            _emailPicker = new EmailAddressChooserTask();
            _emailPicker.Completed += OnEmailPicked;
        }

        private void OnEmailPicked(object sender, EmailResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                _search.Text = e.Email;
                _taskResultOk = true;
            }
        }

        private void OnNumberPicked(object sender, PhoneNumberResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                _search.Text = e.PhoneNumber;
                _taskResultOk = true;
            }
        }

        private string _searchedFor;

        private InputScope _inputScope;
        public InputScope Scope
        {
            get { return _inputScope; }
            set
            {
                _inputScope = value;
                OnPropertyChanged("Scope");
            }
        }

        private string _watermark;
        public string Watermark
        {
            get { return _watermark; }
            set
            {
                _watermark = value;
                OnPropertyChanged("Watermark");
            }
        }

        private string _taskType;
        public string TaskType
        {
            get { return _taskType; }
            set
            {
                _taskType = value;
                OnPropertyChanged("TaskType");
            }
        }

        private string _pageTitle;
        public string PageTitle
        {
            get { return _pageTitle; }
            set
            {
                _pageTitle = value;
                OnPropertyChanged("PageTitle");
            }
        }

        // Typically type of Model.UserSearch.
        private object _us;
        public object UserSearch
        {
            get
            {
                return _us;
            }

            set
            {
                _us = value;
                OnPropertyChanged("UserSearch");
            }
        }

        private UserSearchLoadContext.UserSearchType _searchType;

        public class FakeUserSearch
        {
            public List<CompactUser> NotYetFriends { get; set; }
        }

        private InputScope CreateInputScope(InputScopeNameValue value)
        {
            var s = new InputScope
                        {
                            Names =
                                {
                                    new InputScopeName() { NameValue = value }
                                }
                        };

            return s;
        }

        private void StartMangoSearching()
        {
            // TODO: POLISH: Should progress indicator text while working...
            Contacts contacts = new Contacts();
            contacts.SearchCompleted += OnSearchCompleted;
            contacts.SearchAsync(string.Empty, FilterKind.None, null);
        }

        private void OnSearchCompleted(object sender, ContactsSearchEventArgs e)
        {
            var emails = new Dictionary<string, bool>();
            int count = 0;
            foreach (var item in e.Results)
            {
                foreach (var email in item.EmailAddresses)
                {
                    if (!emails.ContainsKey(email.EmailAddress))
                    {
                        emails.Add(email.EmailAddress, true);
                    }
                }
            }

            Queue<string> waves = new Queue<string>();

            StringBuilder sb = new StringBuilder();
            bool isFirst = true;
            foreach (var item in emails.Keys)
            {
                ++count;

                if (count > 25 && sb.Length > 0)
                {
                    waves.Enqueue(sb.ToString());
                    sb.Clear();
                    isFirst = true;
                    count = 0;
                }

                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    sb.Append(",");
                }

                sb.Append(item);
            }

            if (sb.Length > 0)
            {
                waves.Enqueue(sb.ToString());
            }

            _mangoWaves = waves;
            _mangoResults = new Dictionary<string, CompactUser>();
            ProcessManyWaves();
            //Search(sb.ToString());
        }

        private Queue<string> _mangoWaves;

        private Dictionary<string, CompactUser> _mangoResults;

        private void ProcessManyWaves()
        {
            if (_mangoWaves != null && _mangoWaves.Count > 0)
            {
                var oneGroup = _mangoWaves.Dequeue();

                var context = new UserSearchLoadContext(oneGroup)
                {
                    SearchType = _searchType,
                };

                DataManager.Current.Load<UserSearch>(
                    context,
                    (results) =>
                    {
                        if (results != null && results.NotYetFriends != null)
                        {
                            foreach (var entry in results.NotYetFriends)
                            {
                                var id = entry.UserId;
                                if (!_mangoResults.ContainsKey(id)) // slow
                                {
                                    _mangoResults[id] = entry;
                                }
                            }

                            ProcessManyWaves();
                        }
                    },
                    (bad) =>
                    {
                        ProcessManyWaves();
                        // TODO: ...?
                    });
            }
            else
            {
                _mangoWaves = null;

                // Done!
                if (_mangoResults != null && _mangoResults.Count > 0)
                {
                    var allResults = new List<CompactUser>(_mangoResults.Values);

                    var fake = new FakeUserSearch
                    {
                        NotYetFriends = allResults
                    };

                    UserSearch = fake;
                }

                _mangoResults = null;
            }
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            string by;
            if (NavigationContext.QueryString.TryGetValue("by", out by))
            {
                switch (by)
                {
                    case "mango":
                        _topGrid.Visibility = System.Windows.Visibility.Collapsed;
                        _searchType = UserSearchLoadContext.UserSearchType.Email;
                        // for now I'm just scanning e-mail, a better impl.
                        // would do a phone scan as well.
                        PageTitle = "via contacts";
                        Watermark = string.Empty;
                        TaskType = null;
                        StartMangoSearching();
                        // Scope = ...;
                        break;

                    case "twitter":
                        _searchType = UserSearchLoadContext.UserSearchType.TwitterSource;
                        PageTitle = "on Twitter";
                        Watermark = "enter your twitter name";
                        TaskType = null;
                        Scope = CreateInputScope(InputScopeNameValue.EmailUserName);
                        break;

                    case "name":
                        _searchType = UserSearchLoadContext.UserSearchType.Name;
                        PageTitle = "by name";
                        Watermark = "friend's name";
                        TaskType = null;
                        Scope = CreateInputScope(InputScopeNameValue.PersonalFullName);
                        break;

                    case "email":
                        _searchType = UserSearchLoadContext.UserSearchType.Email;
                        PageTitle = "by email";
                        Watermark = "friend's email";
                        TaskType = "email";
                        Scope = CreateInputScope(InputScopeNameValue.EmailNameOrAddress);
                        break;

                    case "phone":
                        _searchType = UserSearchLoadContext.UserSearchType.Phone;
                        PageTitle = "by phone";
                        Watermark = "phone number";
                        TaskType = "phone";
                        Scope = CreateInputScope(InputScopeNameValue.TelephoneNumber);
                        break;
                }
            }

            DataContext = this;

            object o;
            if (State.TryGetValue("query", out o))
            {
                _searchedFor = o as string;
                if (!string.IsNullOrEmpty(_searchedFor))
                {
                    Search(_searchedFor);
                }
            }
            else if (_taskResultOk && _search.Text.Length > 0)
            {
                _searchedFor = _search.Text;
                Search(_searchedFor);
            }

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            _taskResultOk = false;

            if (_searchedFor != null)
            {
                State["query"] = _searchedFor;
            }

            base.OnNavigatedFrom(e);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var b = sender as Button;
            if (b != null)
            {
                string s = b.Tag as string;
                if (s != null)
                {
                    switch (s)
                    {
                        case "phone":
                            _numberPicker.Show();
                            break;

                        case "email":
                            _emailPicker.Show();
                            break;
                    }

                    return;
                }
            }

            string query = _search.Text;
            if (string.IsNullOrEmpty(query))
            {
                return;
            }

            Search(query);
        }

        private void Search(string query)
        {
            _searchedFor = query;

            var context = new UserSearchLoadContext(query)
            {
                SearchType = _searchType,
            };

            DataManager.Current.Load<UserSearch>(
                context,
                (results) =>
                {
                    UserSearch = results;
                },
                (bad) =>
                {
                    UserSearch = null;
                });
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