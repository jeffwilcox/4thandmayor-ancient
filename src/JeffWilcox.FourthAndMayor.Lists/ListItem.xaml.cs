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
using System.ComponentModel;
using System.Globalization;
using AgFx;
using JeffWilcox.Controls;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace JeffWilcox.FourthAndMayor.Lists
{
    public partial class ListItem : 
        PhoneApplicationPage, 
        INotifyPropertyChanged
    {
        public ListItem()
        {
            InitializeComponent();
        }

        private Model.List _list;
        private Model.CompactListItem _item;

        private string _itemType;

        private string _listItemId;
        private string _listId;

        public Model.List List
        {
            get
            {
                return _list;
            }

            set
            {
                _list = value;
                OnPropertyChanged("List");
            }
        }

        private Model.TipListedLists _listed;
        public Model.TipListedLists Listed
        {
            get { return _listed; }
            set
            {
                _listed = value;
                OnPropertyChanged("Listed");
            }
        }

        public Model.CompactListItem Item
        {
            get
            {
                return _item;
            }

            set
            {
                _item = value;
                OnPropertyChanged("Item");
            }
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string id;
            if (NavigationContext.QueryString.TryGetValue("id", out id))
            {
                _listItemId = id;

                if (NavigationContext.QueryString.TryGetValue("list", out id))
                {
                    _listId = id;

                    _itemType = "listitem";

                    // won't IPNC since it's a direct field set. is that OK?
                    _list = DataManager.Current.Load<Model.List>(
                        new Model.ListLoadContext(_listId),
                        OnListLoaded,
                        OnListLoadFailed);
                }
                else
                {
                    string tipId;
                    if (NavigationContext.QueryString.TryGetValue("tipId", out tipId))
                    {
                        _itemType = "tip";
                        var tip = DataManager.Current.Load<Model.DetailedTip>(
                            new LoadContext(tipId),
                            OnTipLoaded,
                            OnTipLoadFailed);
                    }
                }

                DataContext = this;
            }
        }

        private void OnTipLoaded(Model.DetailedTip tip)
        {
            var item = Model.CompactListItem.CreateTipFaçade(tip.CompactTip);
            if (item != null)
            {
                Item = item;
                UpdateAppBar();
            }

            // TODO: "Listed"

            // NOTE: A potential crash is here.
            /*
             * 
             NullReferenceException
System.NullReferenceException
   at JeffWilcox.FourthAndMayor.Model.CompactListItem.CreateTipFaçade(Tip tip)
   at JeffWilcox.FourthAndMayor.Lists.ListItem.OnTipLoaded(DetailedTip tip)
   at AgFx.DataManager.<>c__DisplayClass69`1.<SetupCompletedCallback>b__68()
   at AgFx.CacheEntry.<>c__DisplayClass5.<NotifyCompletion>b__4()
   at System.Reflection.RuntimeMethodInfo.InternalInvoke(RuntimeMethodInfo rtmi, Object obj, BindingFlags invokeAttr, Binder binder, Object parameters, CultureInfo culture, Boolean isBinderDefault, Assembly caller, Boolean verifyAccess, StackCrawlMark& stackMark)
   at System.Reflection.RuntimeMethodInfo.InternalInvoke(Object obj, BindingFlags invokeAttr, Binder binder, Object[] parameters, CultureInfo culture, StackCrawlMark& stackMark)
   at System.Reflection.MethodBase.Invoke(Object obj, Object[] parameters)
             * 
             * 
             * Navigating to: /JeffWilcox.FourthAndMayor.Lists;component/ListItem.xaml?id=4e90d5b56da174e28e710fe4&tipId=4e90d5b56da174e28e710fe4
             */
        }

        private void OnTipLoadFailed(Exception e)
        {

        }

        private void OnListLoaded(Model.List list)
        {
            // Load the actual data context!
            List = list;

            if (_list != null && _list.ListItems != null)
            {
                foreach (var item in _list.ListItems)
                {
                    if (item.Id == _listItemId)
                    {
                        Item = item;

                        if (item.Tip != null && item.Tip.TipId != null)
                        {
                            Listed = DataManager.Current.Load<Model.TipListedLists>(item.Tip.TipId, OnListedLoaded, OnListedFailed);
                        }
                        else
                        {
                            Listed = null;
                        }

                        break;
                    }
                }
            }

            UpdateAppBar();
        }

        private void OnListLoadFailed(Exception ex)
        {
        }

        private void OnListedLoaded(Model.TipListedLists listed)
        {
            //if (Listed != listed)
            //{
                Listed = listed;
            //}
        }

        private void OnListedFailed(Exception e)
        {

        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
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
            if (_doneBar == null)
            {
                _doneBar = ThemeManager.CreateApplicationBar();
                _markDoneBar = new Microsoft.Phone.Shell.ApplicationBar();

                AppBarHelper.AddButton(_markDoneBar, "mark done", OnAppBarItemClick, AppBarIcon.OK);
                AppBarHelper.AddButton(_doneBar, "undo this", OnAppBarItemClick, AppBarIcon.Cancel);

                AppBarHelper.AddButton(_doneBar, "save", OnAppBarItemClick, AppBarIcon.Save);
                AppBarHelper.AddButton(_markDoneBar, "save", OnAppBarItemClick, AppBarIcon.Save);

                // punted from v3.0!
                //AppBarHelper.AddButton(_doneBar, "share", OnAppBarItemClick, "tbd");
                //AppBarHelper.AddButton(_markDoneBar, "share", OnAppBarItemClick, "tbd");

                AppBarHelper.AddButton(_doneBar, "refresh", OnAppBarItemClick, AppBarIcon.Refresh);
                AppBarHelper.AddButton(_markDoneBar, "refresh", OnAppBarItemClick, AppBarIcon.Refresh);
            }

            if (Item != null)
            {
                // was _list before...

                ApplicationBar = Item.IsDone ? _doneBar : _markDoneBar;
            }
            else
            {
                ApplicationBar = null;
            }
        }

        private ApplicationBar _doneBar;
        private ApplicationBar _markDoneBar;

        private void OnAppBarItemClick(object sender, EventArgs e)
        {
            var abib = (IApplicationBarMenuItem)sender;
            switch (abib.Text)
            {
                case "mark done":
                    MarkDoneOrNot(true);
                    break;

                case "undo this":
                    MarkDoneOrNot(false);
                    break;

                case "save":
                    SaveToList();
                    break;

                case "share":
                    break;

                case "refresh":
                    Refresh();
                    break;
            }
        }

        private void MarkDoneOrNot(bool markDone)
        {
            string listId = Model.LocalCredentials.Current.UserId + "/dones";
            if (markDone && _item != null)
            {
                if (_itemType == "tip")
                {
                    FourSquare.Instance.UpdateListAddItem(listId, "tipId", _item.Id, "Marking tip as done", Refresh, CouldNotUpdate);
                }
                else if (_list != null)
                {
                    FourSquare.Instance.UpdateListAddItemFromList(listId, _list.Id, _item.Id, "Marking this as done", Refresh, CouldNotUpdate);
                }
            }
            else if (_item != null)
            {
                // Undo this.
                FourSquare.Instance.UpdateListRemoveItem(listId, _item.Id, "Undoing", Refresh, CouldNotUpdate);
            }
        }

        private void Refresh()
        {
            if (_list != null)
            {
                var token = CentralStatusManager.Instance.BeginShowMessage("Refreshing this item");
                DataManager.Current.Refresh<Model.List>(_list.LoadContext,
                    (res) =>
                    {
                        token.Complete();

                        UpdateAppBar();
                        OnListLoaded(_list);
                    },
                    (exp) =>
                    {
                        token.Complete();

                        UpdateAppBar();
                        OnListLoadFailed(exp);
                    }
                    );
            }
            else
            {
                // TODO: HOW TO UPDATE TIPS AS WELL!! XXXXXXX
            }
        }

        private void CouldNotUpdate(Exception e)
        {
            
        }

        private void SaveToList()
        {
            if (_item != null)
            {
                // either a list item or a facade item.
                if (_itemType == "tip")
                {
                    NavigationService.Navigate(
                        new Uri(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                "/JeffWilcox.FourthAndMayor.Lists;component/SaveToList.xaml?tipid={0}",
                                _item.Id),
                            UriKind.Relative));
                }
                else if (_item != null && _list != null && _list.IsLoadComplete)
                {
                    NavigationService.Navigate(
                        new Uri(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                "/JeffWilcox.FourthAndMayor.Lists;component/SaveToList.xaml?listid={0}&itemid={1}",
                                _list.Id,
                                _item.Id),
                            UriKind.Relative));
                }
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