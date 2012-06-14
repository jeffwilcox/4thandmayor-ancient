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

#if DEBUG
//#define DEBUG_GIC
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.Phone.Controls;

// This control is not virtualized.
// This control does not properly support observable collections.

namespace JeffWilcox.Controls
{
    // TODO: There may be an issue when using IsFlatList and refreshing, 
    // somethings it removes that group completely.

    // Could consider direct binding in the future to simplify.
    /*
    public bool HasCompletedLoading
    {
        get { return true; }
    }

    public bool HasLoadedInitialScreen
    {
        get { return true; }
    }
     * */

    // Centralize to a single dispatcher for the queues (?) for entire app (?) (hmm)
    // Support a group that doesn't show all its data now
    // lg.InitialItemCap = 10;
    // {provide a range: min, max}
    // BehindTemplate (for EMPTY lists ?)
    // GroupFooterTemplate [ load more items ] showing 10-88 of 1080 items

    public partial class GroupingItemsControl : 
        Control, 
        ISupportPauseResume, 
        ISendLoadComplete, 
        ISendLoadStatus
    {
        private const double InitialQuantum = 16; // was: 40 in v1.1, v1.0
        private const double SecondaryQuantum = 40; // was: 32; //15 ancient

        private static readonly TimeSpan InitialTimeSpan = TimeSpan.FromMilliseconds(InitialQuantum);
        private static readonly TimeSpan SecondaryTimeSpan = TimeSpan.FromMilliseconds(SecondaryQuantum);

        private List<ItemGroup> _groupItems;
        private Dictionary<string, ItemGroup> _groups;
        private Dictionary<string, Mapping> _knownSpecializations = new Dictionary<string, Mapping>();

        public string PanelType { get; set; } // WrapPanel or StackPanel string.
        private Panel _stack;

        private DispatcherTimer _timer;
        private bool _haveItems;
        private bool _templateApplied;
        private double _height;
        private bool _finishedEnoughToShow;

        public bool IsFlatList { get; set; }

        public event EventHandler LoadComplete;

        #region public IEnumerable ItemsSource
        /// <summary>
        /// Gets or sets the items source.
        /// </summary>
        public IEnumerable ItemsSource
        {
            get { return GetValue(ItemsSourceProperty) as IEnumerable; }
            set { SetValue(ItemsSourceProperty, value); }
        }

        /// <summary>
        /// Identifies the ItemsSource dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                "ItemsSource",
                typeof(IEnumerable),
                typeof(GroupingItemsControl),
                new PropertyMetadata(null, OnItemsSourcePropertyChanged));

        /// <summary>
        /// ItemsSourceProperty property changed handler.
        /// </summary>
        /// <param name="d">GroupingItemsControl that changed its ItemsSource.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnItemsSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GroupingItemsControl source = d as GroupingItemsControl;
            //IEnumerable value = e.NewValue as IEnumerable;
            source.UpdateItems();
        }
        #endregion public IEnumerable ItemsSource
        #region public DataTemplate HeaderTemplate
        /// <summary>
        /// Gets or sets the header template.
        /// </summary>
        public DataTemplate HeaderTemplate
        {
            get { return GetValue(HeaderTemplateProperty) as DataTemplate; }
            set { SetValue(HeaderTemplateProperty, value); }
        }

        /// <summary>
        /// Identifies the HeaderTemplate dependency property.
        /// </summary>
        public static readonly DependencyProperty HeaderTemplateProperty =
            DependencyProperty.Register(
                "HeaderTemplate",
                typeof(DataTemplate),
                typeof(GroupingItemsControl),
                new PropertyMetadata(null));
        #endregion public DataTemplate HeaderTemplate
        #region public IsEmpty
        public static readonly DependencyProperty IsEmptyProperty =
                DependencyProperty.Register(
                        "IsEmpty",
                        typeof(Visibility),
                        typeof(GroupingItemsControl), new PropertyMetadata(Visibility.Collapsed));
        public Visibility IsEmpty
        {
            get { return (Visibility)GetValue(IsEmptyProperty); }
            set { SetValue(IsEmptyProperty, value); }
        }
        #endregion
        #region public Top
        public object Top
        {
            get { return GetValue(TopProperty); }
            set { SetValue(TopProperty, value); }
        }
        public static readonly DependencyProperty TopProperty =
                DependencyProperty.Register(
                        "Top",
                        typeof(object),
                        typeof(GroupingItemsControl), null);
        #endregion
        #region public Bottom
        public object Bottom
        {
            get { return GetValue(BottomProperty); }
            set { SetValue(BottomProperty, value); }
        }
        public static readonly DependencyProperty BottomProperty =
                DependencyProperty.Register(
                        "Bottom",
                        typeof(object),
                        typeof(GroupingItemsControl), null);
        #endregion
        #region public object Empty
        /// <summary>
        /// Gets or sets the content for the header of the control.
        /// </summary>
        /// <value>
        /// The content for the header of the control. The default value is
        /// null.
        /// </value>
        public object Empty
        {
            get { return GetValue(EmptyProperty); }
            set { SetValue(EmptyProperty, value); }
        }

        public static readonly DependencyProperty EmptyProperty =
                DependencyProperty.Register(
                        "Empty",
                        typeof(object),
                        typeof(GroupingItemsControl), null);
        #endregion public object Empty
        #region public DataTemplate ItemTemplate
        /// <summary>
        /// The template for standard items.
        /// </summary>
        public DataTemplate ItemTemplate
        {
            get { return GetValue(ItemTemplateProperty) as DataTemplate; }
            set { SetValue(ItemTemplateProperty, value); }
        }

        /// <summary>
        /// Identifies the ItemTemplate dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register(
                "ItemTemplate",
                typeof(DataTemplate),
                typeof(GroupingItemsControl),
                new PropertyMetadata(null));
        #endregion public DataTemplate ItemTemplate
        #region public int InitialItemsLimit
        /// <summary>
        /// Gets or sets the initial items limit. Zero means no limit. PER GROUP.
        /// </summary>
        public int InitialItemsLimit
        {
            get { return (int)GetValue(InitialItemsLimitProperty); }
            set { SetValue(InitialItemsLimitProperty, value); }
        }

        /// <summary>
        /// Identifies the InitialItemsLimit dependency property.
        /// </summary>
        public static readonly DependencyProperty InitialItemsLimitProperty =
            DependencyProperty.Register(
                "InitialItemsLimit",
                typeof(int),
                typeof(GroupingItemsControl),
                new PropertyMetadata(0));
        #endregion public int InitialItemsLimit
        #region public double MinimumItemHeight
        /// <summary>
        /// Gets or sets the minimum item height.
        /// </summary>
        public double MinimumItemHeight
        {
            get { return (double)GetValue(MinimumItemHeightProperty); }
            set { SetValue(MinimumItemHeightProperty, value); }
        }

        /// <summary>
        /// Identifies the MinimumItemHeight dependency property.
        /// </summary>
        public static readonly DependencyProperty MinimumItemHeightProperty =
            DependencyProperty.Register(
                "MinimumItemHeight",
                typeof(double),
                typeof(GroupingItemsControl),
                new PropertyMetadata(0.0));
        #endregion public double MinimumItemHeight

        public GroupingItemsControl() : base()
        {
            _groupItems = new List<ItemGroup>();
            _groups = new Dictionary<string, ItemGroup>();

            DefaultStyleKey = typeof (GroupingItemsControl);
            SizeChanged += OnSizeChanged;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            _height = e.NewSize.Height;
        }

        public override void OnApplyTemplate()
        {
            _templateApplied = true;
            base.OnApplyTemplate();

            _stack = MoreVisualTreeExtensions.FindFirstChildOfType<Panel>(this);

            if (!_haveItems)
            {
                UpdateItems();
            }

            if (double.IsNaN(MinimumItemHeight) || MinimumItemHeight < 1)
            {
#if DEBUG_GIC
                Debug.WriteLine(""); Debug.WriteLine("<< Developer: You should set the minimum item height for this control. " + Name + ">>"); Debug.WriteLine("");
#endif

                // Just an approximate for initial stack panel population.
                MinimumItemHeight = 50;
            }
        }

        private void StopRenderingQueue()
        {
#if DEBUG_GIC
            Debug.WriteLine("StopRenderingQueue");
#endif

            if (_timer != null)
            {
                _timer.Stop();
            }

            if (_timer == null)
            {
                _timer = new DispatcherTimer();
                _timer.Tick += OnProcessingTick;
            }
        }

        private void StartRenderingQueue()
        {
            _timer.Interval = InitialTimeSpan;
            _timer.Start();
        }

        private double _itemsInFirstScreen;

        private bool _myPageLoading;

        private void UpdateItems()
        {
            if (!_templateApplied || DesignerProperties.IsInDesignTool)
            {
                return;
            }
            if (_height < 1)
            {
                Dispatcher.BeginInvoke(UpdateItems);
                return;
            }

            _itemsInFirstScreen = (_height/MinimumItemHeight) * 1.8;
            // VOODOO MAGIC.
            // Moved from 1.3 to 1.8 right before my 2.2 release.

            StopRenderingQueue();

            var oldGroupItems = new List<ItemGroup>(_groupItems);
            _groupItems.Clear();

            int totalItems = -1;
            if (ItemsSource != null)
            {
                var list = (IList)ItemsSource;

                if (IsFlatList)
                {
                    var alist = new List<IList>(1);
                    var blist = list;
                    alist.Add(blist);
                    list = alist;
                }

                int groupCount = list.Count;

                if (groupCount <= 0)
                {
                    ShowHideIsEmpty(false);
                }

                // For long lists of data...
                int actualItemCount = 0;
                for (int i = 0; i < list.Count; ++i)
                {
                    IList x = list[i] as IList;
                    if (x != null)
                    {
                        actualItemCount += x.Count;
                    }
                }

                double approximateVisualLoadPoint = actualItemCount * .45; 
                // WAS PREVIOUSLY: .66; in my voodoo magic...
                if (approximateVisualLoadPoint > _itemsInFirstScreen)
                {
                    // Debug.WriteLine("Overriding the visual load point to be " + approximateVisualLoadPoint + " over " + _itemsInFirstScreen);
                    _itemsInFirstScreen = approximateVisualLoadPoint;
                }

                for (int i = 0; i < list.Count; ++i)
                {
                    //ItemGroup oldGroupAtIndex = null;
                    //if (oldGroupItems.Count > i)
                    //{
                        //oldGroupAtIndex = oldGroupItems[i];
                        //Debug.WriteLine("Was an old group, too.");
                    //}

                    var group = GetOrCreateGroup(list[i], i);
                    _groupItems.Add(group);

                    // if (oldGroupAtIndex != null && 
                    //     oldGroupAtIndex.Name != group.Name)

                    var groupItems = list[i] as IList;
#if DEBUG
                    if (groupItems == null)
                    {
                        throw new NotSupportedException("Must be an IList. " + list[i].ToString());
                    }
#endif

                    UpdateMappings(group, groupItems, ref totalItems);
#if DEBUG_GIC
                    Debug.WriteLine("totalItems is now: *** " + totalItems);
#endif
                }

                if (groupCount == 0 || totalItems < 0) 
                    // was: totalItems <= to 0, but since it started at -1, 
                    //      that meant 1 item was nothing.
                {
#if DEBUG_GIC
                    Debug.WriteLine("GC 0 and TC 0 so done loading now.");
#endif
                    ShowHideIsEmpty(false);
                    IsLoadComplete = true;

                    var isend = ItemsSource as ISendLoadComplete;
                    if (isend != null && !isend.IsLoadComplete)
                    {
#if DEBUG_GIC
                        Debug.WriteLine("It's an isend but not ready.");
#endif
                    }
                    else if (isend != null)
                    {
#if DEBUG_GIC
                        Debug.WriteLine("It's an isend and ready!");
#endif
                    }
                }
                else
                {
                    // Display! ...... //IsLoadComplete = true;

                    _haveItems = true;
                    ShowHideIsEmpty(true);
                    if (!_myPageLoading)
                    {
                        _myPageLoading = true;
                    }
                }
            }
            else
            {
                // Want to eliminate the flicker the first time this pops up.
                IntervalDispatcher.BeginInvoke(TimeSpan.FromSeconds(0.25), EvaluateWhetherNullStill);
#if DEBUG_GIC
                Debug.WriteLine("ItemsSource is now null.");
#endif
            }

            List<ItemGroup> groupsToRemove = new List<ItemGroup>();

            for (int gi = 0; gi < _groupItems.Count; ++gi )
            {
                var group = _groupItems[gi];
#if DEBUG_GIC
                Debug.WriteLine("*" + group.Name + " (" + group.Items.Count + ")");
#endif

                if (group.Items.Count == 0)
                {
                    // Hide the group! From a logic standpoint, this should have been detected above.
#if DEBUG_GIC
                    Debug.WriteLine("Hide the group! From a logic standpoint..." + group);
#endif
                    groupsToRemove.Add(group);
                }
            }
            foreach (var ii in oldGroupItems)
            {
                // SLOW.
                if (_groupItems.Contains(ii))
                    continue;
#if DEBUG_GIC
                Debug.WriteLine("Slow hide the group! " + ii);
#endif
                groupsToRemove.Add(ii);
            }
            foreach (var g in groupsToRemove)
            {
#if DEBUG_GIC
                Debug.WriteLine("Removing a group that was not in the new items source.");
#endif
                g.RemoveGroup(this);
                if (_groups.ContainsValue(g))
                {
                    foreach (var grp in _groups)
                    {
                        if (grp.Value == g)
                        {
                            var key = grp.Key;
                            _groups.Remove(key);
                            break;
                        }
                    }
                }
            }

            StartRenderingQueue();
        }

        private void EvaluateWhetherNullStill()
        {
            if (ItemsSource == null)
            {
//#if DEBUG_GIC
                Debug.WriteLine("ItemsSource is null. BREAKING CHANGE: I WILL NOT SHOW IT NOW UNTIL IT IS SOMETHING LEGITIMATE.");

                // Consider: does state need to be cleared!?
                _groups = new Dictionary<string, ItemGroup>();
                _groupItems = new List<ItemGroup>();
                _knownSpecializations = new Dictionary<string, Mapping>();

//#endif
                //ShowHideIsEmpty(false);
                //IsLoadComplete = true;

                //Empty = "(null!)";
            }
        }

        private void ShowHideIsEmpty(bool hasItems)
        {
            IsEmpty = hasItems ? Visibility.Collapsed : Visibility.Visible;
        }

        private ItemGroup GetOrCreateGroup(object obj, int groupIndex)
        {
            ItemGroup group = null;

            var named = obj as IName;
            if (named != null || IsFlatList)
            {
                string name = named == null ? string.Empty : named.Name;
                if (IsFlatList)
                    name = "flat";
                _groups.TryGetValue(name, out group);

                // Update the underlying object anyway!
                //if (group != null)
                //{
                // TODO: Would have to replace the underlying one.   Not really a core scenario, having the header need to change. Footer yes though when I add that.
                //group.SetContent(obj, HeaderTemplate);
                //}
            }
//#if DEBUG
            else
            {
                throw new InvalidOperationException("The group objects must be an IName or a flat list.");
            }
//#endif

            if (group == null)
            {
//#if BUILDING_4TH_AND_MAYOR
                Panel panel = PanelType == "WrapPanel" ? (Panel)(new WrapPanel()) : (Panel)(new StackPanel());
//#else
                //Panel panel = (Panel)(new StackPanel());
//#endif
                string nametoUse = named == null ? string.Empty : named.Name;
                if (IsFlatList)
                {
                    nametoUse = "flat";
                }
                group = new ItemGroup(nametoUse, obj, IsFlatList ? null : HeaderTemplate, panel);
                // To preserve the Top template, insert + 1.
                group.InsertGroupIntoVisualTree(_stack, groupIndex + 1);


                _groups[nametoUse] = group;
                //_groupItems.Add(group);
            }
            //else
            //{
                //group.MoveGroupIfNeeded(_stack, groupIndex);
            //}

            return group;
        }

        private void UpdateMappings(ItemGroup group, IList items, ref int liveItemCount)
        {
            bool isLimitingGroups = InitialItemsLimit > 0;
            int ilg = InitialItemsLimit;

            int c = items.Count;
#if DEBUG_GIC
            Debug.WriteLine("UpdateMappings is looking at " + group.Name + 
                " working with " + c + 
                " items and the incoming live count was " + liveItemCount);
#endif
            Dictionary<Mapping, bool> previousItemsHash = new Dictionary<Mapping, bool>(group.Items.Count);
            foreach (var item in group.Items)
            {
#if DEBUG_GIC
                Debug.WriteLine("A previous item is " + item.SpecializedString);
#endif
                previousItemsHash[item] = true;
            }
            group.Items.Clear();

            int insertionIndex = -1;
            for (int i = 0; i < c; ++i)
            {
                insertionIndex++;
                liveItemCount++;

                var icsc = items[i] as ISpecializedComparisonString;

                //Debug.Assert(icsc != null);

                string s = icsc != null ? icsc.SpecializedComparisonString : items[i].ToString();

                Mapping existing;

                if (s == null)
                {
                    continue;
                }

                bool needToCreate = true;

                // If it already mapped somewhere?
                if (_knownSpecializations.TryGetValue(s, out existing))
                {
                    //Debug.WriteLine("A known specialization exists already for " + s);
                    if (existing.Group.Name == group.Name)
                    {
                        needToCreate = false;

                        //Debug.WriteLine("+ and it stayed in the same group.");
                        previousItemsHash.Remove(existing);

                        // TODO: REORDERING!
                        //int itemIndex = group.Panel.Children.IndexOf(existing.Presenter);
                        existing.Content = items[i];
                        if (existing.HasBeenShownYet)
                        {
                            existing.Presenter.Content = existing.Content;
                        }

                        // Needs to be in the new ordered list.
                        group.Items.Insert(insertionIndex, existing);

                        // Could see if they even really changed or not.
                    }
                    else
                    {
                        //Debug.WriteLine("- but it moved to this group from another!");

                        // Now present in this group, remove it from the old.
#if DEBUG_GIC
                        Debug.WriteLine("A previous item moved to the new group! " + s + existing.Content);
#endif
                        existing.Group.Panel.Children.Remove(existing.Presenter);
                        group.Items.Remove(existing);
                        _knownSpecializations.Remove(s);

                        // Decrement for a new insert!
                        // i--;
                        //insertionIndex--; // ? is this correct ?
                    }
                }

                if (needToCreate)
                {
                    // Create a new mapping.
                    var map = new Mapping
                    {
                        Content = items[i],
                        Group = group,
                    };
                    map.SetToEmpty(MinimumItemHeight);

                    _knownSpecializations[s] = map;

                    map.OverallItemIndex = liveItemCount;

                    // Index of 0, not a count actually.
                    if (liveItemCount < _itemsInFirstScreen + 1)
                    {
                        map.IsInitialLoadSet = true;
                    }

                    if (isLimitingGroups && i > ilg)
                    {
                        map.IsHidden = true;
                        // store insertion index?

                        // TODO: SHOW GROUP FOOTER!
                    }
                    else
                    {
                        int count = group.Panel.Children.Count;

#if DEBUG
                        if (count < insertionIndex)
                        {
                            // TODO: DEBUG AND FIX!!!!!
                            Debug.WriteLine("DANGER! This is not good math. {0} {1} {2} {3}",
                                insertionIndex,
                                count,
                                IsFlatList,
                                group != null ? group.Name : "null group name");
                        }
#endif
                        if (count < insertionIndex)
                        {
                            insertionIndex = count;
                        }

                        group.Panel.Children.Insert(insertionIndex, map.Presenter);
                        group.Items.Insert(insertionIndex, map);
                    }
                }
            }

            foreach (var emptyItem in previousItemsHash.Keys)
            {
                // Only if the item didn't move groups. So imagine a move up 
                // to a higher group, this would still return for hte other.
                if (emptyItem.Group.Name == group.Name)
                {
                    var pp = emptyItem.Presenter;
#if DEBUG_GIC
                    Debug.WriteLine("Removing previous item " + emptyItem.SpecializedString + emptyItem.ToString() + " " + emptyItem.Content);
#endif
                    emptyItem.Group.Panel.Children.Remove(pp);
                    _knownSpecializations.Remove(emptyItem.SpecializedString);
                }
            }

            // Empty group.
            if (c == 0)
            {
#if DEBUG_GIC
                Debug.WriteLine("--- empty group with c=0 so removing");
#endif
                group.RemoveGroup(this);
            }
        }

        private void OnProcessingTick(object sender, EventArgs e)
        {
            _timer.Stop();

#if DEBUG
            /*foreach (var item in _groups)
            {
                foreach (var x in item.ToString().Split(new string[] { "\r\n" }, StringSplitOptions.None))
                {
                    Debug.WriteLine(x);
                }
            }*/
#endif

            //int index = 0; // _currentRenderingItem;

            // N^N nast.
            for (int i = 0; i < _groupItems.Count; ++i)
            {
                // TODO: Does this get reordered if groups move?
                var g = _groupItems[i];
#if DEBUG_GIC
                Debug.WriteLine("Looking at group index " + i.ToString() + " titled " + g.Name + "    (c:" + g.Items.Count.ToString() + ")");
#endif
                for (int j = 0; j < g.Items.Count; ++j)
                {
                    var it = g.Items[j];
#if DEBUG_GIC
                    Debug.WriteLine("Item " + j + " hasBeenShown:" + it.HasBeenShownYet + " isHidden:" + it.IsHidden + " overallIndex:" + it.OverallItemIndex);
#endif
                    if (!it.HasBeenShownYet && !it.IsHidden)
                    {
                        // Our guy!
                        //UpdateRenderingQuantum();

#if DEBUG_GIC
                        Debug.WriteLine("Showing item {0}", it.OverallItemIndex);
#endif

                        if (!IsLoadComplete && !it.IsInitialLoadSet)
                        {
#if DEBUG_GIC
                            Debug.WriteLine("GIC: {0}: First page has been loaded.", Name);
                            Debug.WriteLine("Moving to the secondary quantum for loading in more... " + SecondaryQuantum);
#endif
                            _timer.Interval = SecondaryTimeSpan;
                            IsLoadComplete = true;
                        }

                        // This should only apply to items since headers/groups are
                        // automatically applied at creation.
                        it.ApplyTemplate(ItemTemplate, IsLoadComplete);

                        _timer.Start();

                        return;
                    }
                }
            }
#if DEBUG_GIC
            Debug.WriteLine("GIC: {0}: Everything has been loaded and is in the visual tree.", Name);
#endif
            // TODO: What if the items source is null?

            if (!IsLoadComplete && ItemsSource != null)
            {
                IsLoadComplete = true;
            }

            _timer.Tick -= OnProcessingTick;
            _timer = null;

            //_currentRenderingItem++;
        }

        public void Pause()
        {
#if DEBUG_GIC
            Debug.WriteLine("GIC: {0}: Pausing Layout", Name);
#endif
            if (_timer != null)
            {
                _timer.Stop();
            }
        }

        public void Resume()
        {
#if DEBUG_GIC
            Debug.WriteLine("GIC: {0}: Resuming Layout", Name);
#endif
            if (_timer != null)
            {
                _timer.Start();
            }
        }

        public bool IsLoadComplete
        {
            get
            {
                return _finishedEnoughToShow;
            }
            set
            {
                _finishedEnoughToShow = value;

                if (value)
                {
                    // GlobalLoading.Instance.IsLoading = false;
                    // Debug.WriteLine("");
                    // Debug.WriteLine("");
                    // Debug.WriteLine("Is Loaded!!!!!!");
                    // Debug.WriteLine("");
                    // Debug.WriteLine("");

                    LoadStatus = Controls.LoadStatus.Loaded;
                    OnLoadStatusChanged();

                    var handler = LoadComplete;
                    if (handler != null)
                    {
                        handler(this, EventArgs.Empty);
                    }
                }
            }
        }

        private LoadStatus _loadStatus;

        public LoadStatus LoadStatus
        {
            get
            {
                return _loadStatus;
            }
            set
            {
                _loadStatus = value;
            }
        }

        public event EventHandler LoadStatusChanged;

        protected virtual void OnLoadStatusChanged()
        {
            var handler = LoadStatusChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private class Mapping
        {
            private ISpecializedComparisonString _iscs;
            private object _object;
            public ContentPresenter Presenter;
            public object Content
            {
                get { return _object; }
                set
                {
                    //ISpecializedComparisonString ic = value as ISpecializedComparisonString;
                    /*if (value != null && ic == null)
                    {
                        throw new InvalidOperationException("Content must be a specialized comparison string type.");
                    }*/
                    _object = value;
                }
            }
            public bool IsInitialLoadSet;
            public ItemGroup Group { get; set; }
            public int OverallItemIndex { get; set; }
            public bool IsHidden;

#if DEBUG
            public override string ToString()
            {
                return string.Format("         Mapping:: Overall: {2} IsInitialLoadSet: {0}    Shown: {1}    {3}", IsInitialLoadSet, HasBeenShownYet, OverallItemIndex, Content);
            }
#endif

            public void SetToEmpty(double minimumHeight)
            {
                Presenter = new ContentPresenter
                {
                    ContentTemplate = null,
                    Content = null,
                    MinHeight = minimumHeight,
                };
            }

            public string SpecializedString
            {
                get
                {
                    if (_iscs == null)
                    {
                        _iscs = _object as ISpecializedComparisonString;
                    }
                    return _iscs == null ? Content.ToString() : _iscs.SpecializedComparisonString;
                }
            }

            private bool _shown;

            public bool HasBeenShownYet { get { return _shown; } }

            public void ApplyTemplate(DataTemplate template, bool animateIn)
            {
                if (Presenter != null)
                {
                    Presenter.ContentTemplate = template;
                    if (animateIn)
                    {
                        Presenter.Opacity = 0;
                        OpacityAnimator oa = null;
                        OpacityAnimator.EnsureAnimator(Presenter, ref oa);
                        if (oa != null)
                        {
                            oa.GoTo(1.0, new Duration(TimeSpan.FromSeconds(.75)));
                        }
                    }
                    Presenter.Content = Content;
                    _shown = true;
                }
            }
        }

        private class ItemGroup
        {
            private List<Mapping> _items;
            private readonly string _name;
            private Panel _panel;

            public ItemGroup(string name, object content, DataTemplate headerTemplate, Panel panel)
            {
                _panel = panel;
                _items = new List<Mapping>();
                _name = name;

                //IName iname = content as IName;
                //_name = iname == null ? string.Empty : iname.Name;

                // fot flat list scenarios...
                SetContent(headerTemplate == null ? null : content, headerTemplate);
            }

            public string Name
            {
                get
                {
                    return _name;
                }
            }

#if DEBUG
            public override string ToString()
            {
                var sb = new StringBuilder();
                sb.AppendFormat("    Group:: {0}\r\n", Name);
                foreach (var item in Items)
                {
                    sb.AppendLine(item.ToString());
                }

                return sb.ToString();
            }
#endif

            private int _initialGroupIndex;

            public void MoveGroupIfNeeded(Panel sp, int expectedIndex)
            {
                if (_initialGroupIndex < expectedIndex)
                {
                    // Moved the list now. ? Not changing though.
                    // Debug.WriteLine("// Moved the list now. ? Not changing though.");
                }
                else if (_initialGroupIndex == expectedIndex)
                {
                    // No change.
                }
                else
                {
                    // Need to move up. Closer to top.
                    sp.Children.Remove(Presenter);
                    sp.Children.Remove(Panel);
                    // TODO: INDEXES?

                    // Debug.WriteLine("Moving up closer to the top.");
                }
            }

            public void InsertGroupIntoVisualTree(Panel sp, int insertionIndex)
            {
                _initialGroupIndex = insertionIndex;

                sp.Children.Insert(insertionIndex * 2, Presenter);
                sp.Children.Insert((insertionIndex * 2) + 1, Panel);
            }

            public void RemoveGroup(GroupingItemsControl ic)
            {
                // Debug.WriteLine("Removing ItemGroup Name:" + Name + " Count:" + Items.Count);

                ic._groups.Remove(Name);
                int index = ic._stack.Children.IndexOf(this.Presenter);
                if (index >= 0)
                {
                    // Header
                    ic._stack.Children.RemoveAt(index);

                    // Group panel
                    ic._stack.Children.RemoveAt(index);
                }

                _items = new List<Mapping>();
            }

            public Panel Panel
            {
                get
                {
                    return _panel;
                }
            }

            public Mapping AddItem(object item)
            {
                return null;
            }

            public void SetContent(object content, DataTemplate headerTemplate)
            {
                Presenter = new ContentPresenter
                {
                    Content = content,
                    ContentTemplate = headerTemplate,
                };

                if (_name == string.Empty)
                {
                    Presenter.Visibility = Visibility.Collapsed;
                }
            }

            public ContentPresenter Presenter
            {
                get;
                private set;
            }

            public List<Mapping> Items { get { return _items; } }
        }
    }
}
