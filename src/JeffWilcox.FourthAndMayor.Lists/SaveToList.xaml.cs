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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls.Primitives;
using System.Windows.Navigation;
using AgFx;
using JeffWilcox.Controls;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace JeffWilcox.FourthAndMayor.Lists
{
    public partial class SaveToList :
        PhoneApplicationPage
    {
        public SaveToList()
        {
            InitializeComponent();
        }

        private Model.UserLists _lists;

        private string _itemId;
        private string _itemType;
        private string _optionalListId;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string value;
            if (NavigationContext.QueryString.TryGetValue("venueid", out value))
            {
                _itemId = value;
                _itemType = "venue";
            }
            if (NavigationContext.QueryString.TryGetValue("tipid", out value))
            {
                _itemId = value;
                _itemType = "tip";
            }
            if (NavigationContext.QueryString.TryGetValue("listid", out value))
            {
                _itemType = "item";
                _optionalListId = value;

                if (NavigationContext.QueryString.TryGetValue("itemid", out value))
                {
                    _itemId = value;
                }
                // else, bad!
            }

            if (_lists == null)
            {
                _lists = DataManager.Current.Load<Model.UserLists>(
                    new LoadContext("self"),
                    OnListsLoaded,
                    OnListsFailed);
            }
            else
            {
                UpdateAppBar();
            }

            if (AddNewList.NewlyCreatedList != null)
            {
                Refresh();
            }
        }

        private void HookOrUnhook(List<WrappedListList> lists, bool hook)
        {
            if (lists != null)
            {
                foreach (var group in lists)
                {
                    foreach (var list in group)
                    {
                        if (hook)
                        {
                            list.IsCheckedChanged += OnItemIsCheckedChanged;
                        }
                        else
                        {
                            list.IsCheckedChanged -= OnItemIsCheckedChanged;
                        }
                    }
                }
            }
        }

        private void OnItemIsCheckedChanged(object sender, EventArgs e)
        {
            int chk = 0;
            var all = DataContext as List<WrappedListList>;
            if (all != null)
            {
                foreach (var group in all)
                {
                    foreach (var list in group)
                    {
                        if (list.IsChecked == true)
                        {
                            ++chk;
                        }
                    }
                }
            }

            _checkedItemCount = chk;

            UpdateAppBar();
        }

        private int _checkedItemCount;

        private void OnListsLoaded(Model.UserLists lists)
        {
            var editableGroups = new List<WrappedListList>();

            foreach (var group in lists.Groups)
            {
                var newGroup = new WrappedListList
                {
                    Name = group.Name,
                    Type = group.Type,
                };

                foreach (var item in group)
                {
                    if ((item.IsEditable  || (item.User != null && item.User.IsSelf))
                        &&
                        !item.Id.Contains("/dones")
                        &&
                        !item.Id.Contains("/tips")
                        )
                    {
                        newGroup.Add(new WrappedList(item));
                    }
                }

                if (newGroup.Count > 0)
                {
                    editableGroups.Add(newGroup);
                }
            }

            HookOrUnhook(editableGroups, true);

            DataContext = editableGroups;
        }

        public class WrappedListList : List<WrappedList>, IName
        {
            public string Name { get; set; }
            public string Type { get; set; }
        }

        public class WrappedList : INotifyPropertyChanged, ISpecializedComparisonString
        {
            private Model.CompactList _list;
            public Model.CompactList Item
            {
                get { return _list; }
                set
                {
                    _list = value;
                    OnPropertyChanged("Item");
                }
            }

            public event EventHandler IsCheckedChanged;

            protected void OnIsCheckedChanged(EventArgs e)
            {
                var handler = IsCheckedChanged;
                if (handler != null)
                {
                    handler(this, e);
                }
            }

            private bool? _checked;
            public bool? IsChecked
            {
                get
                {
                    return _checked;
                }
                set
                {
                    _checked = value;
                    OnPropertyChanged("IsChecked");
                    OnIsCheckedChanged(EventArgs.Empty);
                }
            }

            public WrappedList()
            {
                IsChecked = false;
            }

            public WrappedList(Model.CompactList cl) : this()
            {
                Item = cl;
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

            public string SpecializedComparisonString
            {
                get
                {
                    return Item == null ? new Guid().ToString() : Item.SpecializedComparisonString;
                }
            }
        }

        private void OnListsFailed(Exception ex)
        {
            // ?
        }

        public void OnTransitionTo()
        {
            UpdateAppBar();
        }

        public void OnTransitionFrom()
        {
            ApplicationBar = null;
        }

        private void UpdateAppBar()
        {
            if (_appBar == null)
            {
                _appBar = ThemeManager.CreateApplicationBar();
                _save = AppBarHelper.AddButton(_appBar, "save", OnAppBarItemClick, AppBarIcon.Save);

                AppBarHelper.AddButton(_appBar, "create new", OnAppBarItemClick, AppBarIcon.Add);

                AppBarHelper.AddButton(_appBar, "refresh", OnAppBarItemClick, AppBarIcon.Refresh);

                ApplicationBar = _appBar;
            }

            if (_lists != null)
            {
                ApplicationBar = _appBar;
            }

            _save.IsEnabled = _checkedItemCount > 0;
        }

        private ApplicationBar _appBar;

        private IApplicationBarMenuItem _save;

        private void OnAppBarItemClick(object sender, EventArgs e)
        {
            var abib = (IApplicationBarMenuItem)sender;
            switch (abib.Text)
            {
                case "refresh":
                    Refresh();
                    break;

                case "save":
                    if (_itemType != null && _itemId != null)
                    {
                        Save();
                    }
                    break;

                case "create new":
                    NavigationService.Navigate(new Uri("/JeffWilcox.FourthAndMayor.Lists;component/AddNewList.xaml", UriKind.Relative));
                    break;
            }
        }

        private void Save()
        {
            var lists = DataContext as List<WrappedListList>;
            if (lists != null)
            {
                List<Model.CompactList> listsToSaveTo = new List<Model.CompactList>();
                foreach (var group in lists)
                {
                    foreach (var list in group)
                    {
                        if (list.IsChecked == true)
                        {
                            listsToSaveTo.Add(list.Item);
                        }
                    }
                }

                if (listsToSaveTo.Count > 0)
                {
                    foreach (var list in listsToSaveTo)
                    {
                        StartSave(list);
                    }
                }
            }

            Dispatcher.BeginInvoke(NavigationService.GoBackWhenReady);
        }

        private bool _warned;

        private void StartSave(Model.CompactList list)
        {
            string type = null;
            if (_itemType == "venue") type = "venueId";
            if (_itemType == "tip") type = "tipId";
            if (_itemType == "item") type = "itemId";
            if (type == null) return;

            if (_optionalListId == null)
            {
                FourSquare.Instance.UpdateListAddItem(list.Id, type, _itemId, "Saving to list", () => RefreshList(list), (ex) => Warn(list));
            }
            else
            {
                // this is a list and item clone/save operation. ugly code dupe.
                FourSquare.Instance.UpdateListAddItemFromList(
                    list.Id,
                    _optionalListId,
                    _itemId,
                    "Saving to list", () => RefreshList(list), (ex) => Warn(list));
            }
        }

        private void Warn(Model.CompactList list)
        {
            if (_warned == false)
            {
                PriorityQueue.AddUiWorkItem(() =>
                {
                    if (!_warned)
                    {
                        _warned = true;
                        System.Windows.MessageBox.Show("Wasn't able to save the item to " + list.Name);
                    }
                });
            }
        }

        private void Refresh()
        {
            if (_lists != null && _lists.IsLoadComplete)
            {
                DataManager.Current.Refresh<Model.UserLists>(_lists.LoadContext, OnListsLoaded, OnListsFailed);
                // _lists.Refresh();
            }
        }

        private void RefreshList(Model.CompactList list)
        {
            DataManager.Current.Refresh<Model.List>(new LoadContext(list.Id), null, null);
        }

        private void OnButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            var bb = (ButtonBase)sender;
            if (bb != null)
            {
                WrappedList item = bb.DataContext as WrappedList;
                if (item != null)
                {
                    item.IsChecked = (item.IsChecked == null ? true : (!(bool)item.IsChecked));
                }
            }
        }
    }
}
