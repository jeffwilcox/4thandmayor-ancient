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
#define DEBUG_LOADING_PIVOT_ITEM
#endif

using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using AgFx;
using Microsoft.Phone.Controls;
using MyOpacityAnimator = JeffWilcox.Controls.OpacityAnimator;

namespace JeffWilcox.Controls
{
    public class OLDLoadingPivotItem : PivotItem
    {
        // Item State
        //      loading
        //      loaded
        //      failed
        //      had data once?  
        //      [[timed out
        //
        //  .so
        //

        // NEW EXTENDED DESIGN:
        // - Add Timeout period and text
        // - Add Failure text
        // - Add the ability to Retry

        // "The {0} could not be loaded. There may be a problem with the network or cellular service."
        // "The {0} could not be loaded in a timely fashion. Perhaps there is a problem with the network of cell service." (no Retry button?)

        // private const int VisualTreeSearchDepth = 8;
        //private static readonly TimeSpan SmartLoadingDelayingTime = TimeSpan.FromSeconds(.25);

        private const string LoadingGridPartName = "loadingGrid";
        private const string ContentGridPartName = "grid";
        private const string PerformanceProgressBarPartName = "progressBar";

        private static readonly TimeSpan MinimumLoadingTime = TimeSpan.FromSeconds(0.5);
        private static readonly TimeSpan AdditionalLoadTime = TimeSpan.FromSeconds(0.08);
        private static readonly Duration TransitionDuration = new Duration(TimeSpan.FromSeconds(0.5));

        // Could make it a property...
        private static readonly TimeSpan TimeoutSpan = TimeSpan.FromSeconds(5.0);

        private Pivot _pivot;
        private Grid _loadingGrid;
        private Grid _contentGrid;
        private ContentPresenter _contentPresenter;
        private ProgressBar _progressBar;

        //private bool _hasUnmaskedContent;
        private bool _contentPresenterUnloaded;
        private DateTime _okToShowAfter;
        private bool _loadingProcessDone;
        private bool _contentPresenterLoaded;
        private ISupportPauseResume _pauseAndResumeChild;
//        private ISendLoadComplete _advancedWaitingElement;
        private ISendLoadStatus _dataStatusElement;
        private bool _hasShownLoadingScreen;

        private DateTime _timeoutAfter;
        private DispatcherTimer _timeoutTimer;

        #region public string LoadingText
        /// <summary>
        /// Gets or sets the loading text to display. Doesn't support content.
        /// </summary>
        public string LoadingText
        {
            get { return GetValue(LoadingTextProperty) as string; }
            set { SetValue(LoadingTextProperty, value); }
        }

        /// <summary>
        /// Identifies the LoadingText dependency property.
        /// </summary>
        public static readonly DependencyProperty LoadingTextProperty =
            DependencyProperty.Register(
                "LoadingText",
                typeof(string),
                typeof(LoadingPivotItem),
                new PropertyMetadata(null));
        #endregion public string LoadingText

        #region public Thickness LoadingMargin
        /// <summary>
        /// Gets or sets the margin used to offset.
        /// </summary>
        public Thickness LoadingMargin
        {
            get { return (Thickness)GetValue(LoadingMarginProperty); }
            set { SetValue(LoadingMarginProperty, value); }
        }

        /// <summary>
        /// Identifies the LoadingMargin dependency property.
        /// </summary>
        public static readonly DependencyProperty LoadingMarginProperty =
            DependencyProperty.Register(
                "LoadingMargin",
                typeof(Thickness),
                typeof(LoadingPivotItem),
                new PropertyMetadata(new Thickness(0)));
        #endregion public Thickness LoadingMargin

        #region Control Basics
        public LoadingPivotItem() : base()
        {
            DefaultStyleKey = typeof(LoadingPivotItem);
        }

        public override void OnApplyTemplate()
        {
            _loadingGrid = GetTemplateChild(LoadingGridPartName) as Grid;
            _contentGrid = GetTemplateChild(ContentGridPartName) as Grid;
            _progressBar = GetTemplateChild(PerformanceProgressBarPartName) as ProgressBar;

            if (_pivot == null)
            {
                DependencyObject d = Parent;
                while (d != null && !(d is Pivot))
                {
                    d = VisualTreeHelper.GetParent(d);
                }
                _pivot = d as Pivot;
                if (_pivot == null)
                {
                    //if (!DesignerProperties.IsInDesignTool)
                    //{
                        //throw new InvalidOperationException("Could not find the parent Pivot control instance.");
                    //}
                    return;
                }

                _pivot.SelectionChanged += OnPivotSelectionChanged;

                if (_pivot.SelectedItem == this)
                {
                    Dispatcher.BeginInvoke(() => ShowLoadingScreen(true));
                }
            }

            base.OnApplyTemplate();
        }
        #endregion

        #region Pivot Events
        private void OnPivotSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e != null && e.AddedItems != null && e.AddedItems.Count > 0)
            {
                var lpi = e.AddedItems[0] as LoadingPivotItem;
                if (lpi != null && lpi == this && !_loadingProcessDone)
                {
                    Dispatcher.BeginInvoke(() => ShowLoadingScreen(false));
                }
            }
        }

        private void OnLoadedPivotItem(object sender, PivotItemEventArgs e)
        {
            if (e.Item == this)
            {
                _pivot.LoadedPivotItem -= OnLoadedPivotItem;
                _pivot.UnloadingPivotItem += OnUnloadingPivotItem;

                if (_pauseAndResumeChild != null)
                {
                    _pauseAndResumeChild.Resume();
                }
                else
                {
                    Dispatcher.BeginInvoke(UnmaskHiddenContent);
                }
            }
        }

        private void OnUnloadingPivotItem(object sender, PivotItemEventArgs e)
        {
            if (e.Item == this)
            {
                if (_pauseAndResumeChild != null)
                {
                    _pivot.LoadedPivotItem += OnLoadedPivotItem;
                    _pauseAndResumeChild.Pause();
                }

                //_pivot.LoadedPivotItem -= OnLoadedPivotItem;
                _pivot.UnloadingPivotItem -= OnUnloadingPivotItem;
            }
        }
        #endregion

        #region Housekeeping
        public bool HasLoadedOfficially
        {
            get { return _contentPresenterLoaded; }
        }

        private void OnContentPresenterUnloaded(object sender, RoutedEventArgs e)
        {
#if DEBUG_LOADING_PIVOT_ITEM
            Debug.WriteLine("LPI: Content presenter of {0} was unloaded", Header.ToString());
#endif
            _contentPresenterUnloaded = true;
            _contentPresenterLoaded = false;
            _contentPresenter.Loaded += OnContentPresenterLoaded;
        }
        #endregion

        #region 1: Unmasking behind the scenes while showing the load screen.
        private void UnmaskHiddenContent()
        {
            if (_contentGrid == null)
            {
#if DEBUG
                throw new InvalidOperationException("Content grid must be present.");
#endif
                return;
            }
            if (_contentPresenter != null)
            {
#if DEBUG
                throw new InvalidOperationException("Unmasking cannot happen twice.");
#endif
                return;
            }
            //_hasUnmaskedContent = true;

            var contentBinding = new Binding( "Content") { BindsDirectlyToSource = true, Source = this };
            var templateBinding = new Binding("ContentTemplate") { BindsDirectlyToSource = true, Source = this };
            _contentPresenter = new ContentPresenter
                {
                    HorizontalAlignment = HorizontalContentAlignment,
                    VerticalAlignment = VerticalContentAlignment,
                    Margin = Padding,
                    IsHitTestVisible = false,
                };
            _contentPresenter.Loaded += OnContentPresenterLoaded;
            _contentPresenter.Unloaded += OnContentPresenterUnloaded;
            _contentPresenter.SetBinding(ContentPresenter.ContentTemplateProperty, templateBinding);
            _contentPresenter.SetBinding(ContentPresenter.ContentProperty, contentBinding);

            _contentGrid.Children.Add(_contentPresenter);
            _contentGrid.Visibility = Visibility.Visible;

            _okToShowAfter = DateTime.Now + MinimumLoadingTime;
            _timeoutAfter = DateTime.Now + TimeoutSpan; // probably don't need this one
            if (_timeoutTimer == null)
            {
                _timeoutTimer = new DispatcherTimer();
                _timeoutTimer.Interval = TimeoutSpan;
                _timeoutTimer.Tick += OnTimeoutTimerTick;
            }
            _timeoutTimer.Start();
        }

        private void OnTimeoutTimerTick(object sender, EventArgs e)
        {
            _timeoutTimer.Stop();
            _timeoutTimer = null;

            // !?!?!? how to update properly
            // TODO: TEMP
            LoadingText = "Timed out! TEMP!";

            // TODO: Make sure the Loading content is shown.

        }

        private void ShowLoadingScreen(bool isCalledFromOnApplyTemplate)
        {
            if (_hasShownLoadingScreen)
            {
                return;
            }
            //if (_hasUnmaskedContent)
            //{
                //return;
            //}

            if (!isCalledFromOnApplyTemplate)
            {
                _pivot.LoadedPivotItem += OnLoadedPivotItem;
            }

            //}

            //_hasUnmaskedContent = true;

            ShowLoadingExperience();

            // Code that was here is now above in the Loaded area.
            if (isCalledFromOnApplyTemplate)
            {
                _pivot.UnloadingPivotItem += OnUnloadingPivotItem;
                UnmaskHiddenContent();
            }
        }

        private void ShowLoadingExperience()
        {
            _hasShownLoadingScreen = true;
            if (_progressBar != null)
            {
                /*var binding = new Binding("IsLoading");
                binding.Source = GlobalLoading.Instance;
                binding.Converter = new InvertConverter();
                _progressBar.SetBinding(ProgressBar.IsIndeterminateProperty, binding);*/
                _progressBar.IsIndeterminate = true;
            }
            if (_loadingGrid != null)
            {
                // Not sure what this nice phone-similar effect costs yet...
                TransformYAnimator ta = null;
                TransformYAnimator.EnsureAnimator(_loadingGrid, ref ta);
                if (ta != null)
                {
                    ta.GoTo(50, new Duration(TimeSpan.Zero));
                    ta.GoTo(0, TransitionDuration, new QuarticEase());
                }

                MyOpacityAnimator oa = null;
                MyOpacityAnimator.EnsureAnimator(_loadingGrid, ref oa);
                if (oa != null)
                {
                    oa.GoTo(1, TransitionDuration);
                }
                else
                {
                    _loadingGrid.Opacity = 1;
                }
            }
        }
        #endregion

        #region 2: Once the hidden content is getting ready, check the data.
        private void OnContentPresenterLoaded(object sender, RoutedEventArgs e)
        {
#if DEBUG_LOADING_PIVOT_ITEM
            Debug.WriteLine("LPI: _contentPresented Loaded");
#endif
            _contentPresenter.Loaded -= OnContentPresenterLoaded;
            _contentPresenterLoaded = true;

            CheckForUnderlyingReadyState();
        }

        private void CheckForUnderlyingReadyState()
        {
//#if DEBUG_LOADING_PIVOT_ITEM
            //Debug.WriteLine("LPI: Still not ready.");
//#endif
            if (DateTime.Now > _okToShowAfter)
            {
                // Check the children here for the interface.

                Dispatcher.BeginInvoke(TryTransitionToContent);
            }
            else
            {
                DelayedDispatcherTimer.BeginInvoke(AdditionalLoadTime, TryTransitionToContent);
            }
        }
        #endregion

        #region 3: Wait for specialized content and interfaces to report.
        private void OnAdvancedWaitingElementLoadComplete(object sender, EventArgs e)
        {
            if (_dataStatusElement.LoadStatus != LoadStatus.Loading)
            {
                // Either Failed or Loaded

                _dataStatusElement.LoadStatusChanged -= OnAdvancedWaitingElementLoadComplete;
                //_advancedWaitingElement.LoadComplete -= OnAdvancedWaitingElementLoadComplete;
                if (Dispatcher.CheckAccess())
                {
                    OnceSpecializedContentIsReady();
                }
                else
                {
                    Dispatcher.BeginInvoke(OnceSpecializedContentIsReady);
                }
            }
        }

        private void OnceSpecializedContentIsReady()
        {
            Debug.WriteLine("OnceSpecializedContentIsReady!");
            if (_dataStatusElement != null && _dataStatusElement.LoadStatus == LoadStatus.Failed)
            {
                Debug.WriteLine("FAILED that is...");
                LoadingText = "FAILED!!!!!!";

                bool mayHaveHadDataAlready = false;
                ISendLoadComplete islc = _dataStatusElement as ISendLoadComplete;
                if (islc != null)
                {
                    if (islc.IsLoadComplete)
                    {
                        mayHaveHadDataAlready = true;
                        // There "was" data and now there's been an error...
                    }
                }

                if (!mayHaveHadDataAlready)
                {
                    RestoreLoadingGrid();
                }

                // NOTE: Never will call FinallyShowContent.
                return;
            }

            DelayedDispatcherTimer.BeginInvoke(AdditionalLoadTime, FinallyShowContent);
        }

//        private void WaitForSpecializedContent()
//        {
////#if DEBUG_LOADING_PIVOT_ITEM
//            //Debug.WriteLine("     ... but the data isn't ready yet.");
////#endif
//            if (!_contentPresenterUnloaded)
//            {
//                if (_advancedWaitingElement.IsLoadComplete)
//                {
//                    DelayedDispatcherTimer.BeginInvoke(AdditionalLoadTime, FinallyShowContent);
//                }
//                else
//                {
//                    DelayedDispatcherTimer.BeginInvoke(SmartLoadingDelayingTime, WaitForSpecializedContent);
//                }
//            }
//        }

        private void TryTransitionToContent()
        {
            if (_contentPresenterUnloaded)
            {
                return;
            }

//#if DEBUG_LOADING_PIVOT_ITEM
            //Debug.WriteLine("LPI: DataContext of {0} is set to {1}", Header.ToString(), DataContext.ToString());
//#endif

            // Visuals first to grab any control.
            var descendantsOnce = _contentPresenter.GetVisualDescendants().ToList();
            _pauseAndResumeChild = descendantsOnce.OfType<ISupportPauseResume>().FirstOrDefault();
            //_advancedWaitingElement = descendantsOnce.OfType<ISendLoadComplete>().FirstOrDefault();
            _dataStatusElement = descendantsOnce.OfType<ISendLoadStatus>().FirstOrDefault();

            // If the bindings never get applied due to visibility converters.
            //var uie = _advancedWaitingElement as UIElement;
            var uie = _dataStatusElement as UIElement;
            if (uie != null && uie.Visibility == Visibility.Collapsed)
            {
                _dataStatusElement = null;
                //_advancedWaitingElement = null;
            }

            if (_dataStatusElement /*_advancedWaitingElement */== null)
            {
                // Pause and resume is not support for these two.
                var fe =
                    descendantsOnce.OfType<ItemsControl>().Where(it => it.ItemsSource is ISendLoadStatus).
                        FirstOrDefault();
                if (fe != null)
                {
                    _dataStatusElement = (ISendLoadStatus)fe.ItemsSource;
                    //_advancedWaitingElement = (ISendLoadComplete) fe.ItemsSource;
                }

                if (_dataStatusElement /*_advancedWaitingElement */ == null)
                {
                    var de =
                        descendantsOnce.OfType<FrameworkElement>().Where(ff => ff.DataContext is ISendLoadStatus)
                            .FirstOrDefault();
                    if (de != null)
                    {
                        _dataStatusElement = de.DataContext as ISendLoadStatus;
//                        _advancedWaitingElement = de.DataContext as ISendLoadComplete;
                    }
                }
            }

            if (_dataStatusElement/*_advancedWaitingElement */!= null)
            {
#if DEBUG_LOADING_PIVOT_ITEM
                Debug.WriteLine("LPI: Custom item base {0} supports load notifications. Excellent.", _dataStatusElement.GetType().ToString());
#endif
//                if (_advancedWaitingElement.IsLoadComplete)
                if (_dataStatusElement.LoadStatus == LoadStatus.Loaded || _dataStatusElement.LoadStatus == LoadStatus.Failed)
                {
                    OnceSpecializedContentIsReady();
                }
                else
                {
                    _dataStatusElement.LoadStatusChanged += OnAdvancedWaitingElementLoadComplete;
                    //_advancedWaitingElement.LoadComplete += OnAdvancedWaitingElementLoadComplete;
                    //return;
                }

                // don't let it appear by accident...
                return;
            }

            Debug.Assert(!_contentPresenterUnloaded);

            DelayedDispatcherTimer.BeginInvoke(AdditionalLoadTime, FinallyShowContent);
        }
        #endregion

        #region 4: Finally show the real pivot item's content to the world.
        private void FinallyShowContent()
        {
            Debug.WriteLine("FinallyShowContent");
            if (!_contentPresenterUnloaded)
            {
                _loadingProcessDone = true;

                // No longer need this reference. Do still want resume though!
                //_advancedWaitingElement = null;
                _dataStatusElement = null;

                if (_loadingGrid != null)
                {
                    if (_progressBar != null)
                    {
                        _progressBar.IsIndeterminate = false;
                        _progressBar.Visibility = Visibility.Collapsed;
                    }

                    AfterFadeOut();
                }

                _contentPresenter.IsHitTestVisible = true;

                // Fade in the new content
                if (_contentGrid != null)
                {
                    MyOpacityAnimator cg = null;
                    MyOpacityAnimator.EnsureAnimator(_contentGrid, ref cg);
                    if (cg != null)
                    {
                        cg.GoTo(1, TransitionDuration);
                    }
                    else
                    {
                        _contentGrid.Opacity = 1;
                    }
                }
            }
        }

        private void AfterFadeOut()
        {
            if (_progressBar != null)
            {
                _progressBar.Visibility = Visibility.Collapsed;
            }
            if (_loadingGrid != null)
            {
                _loadingGrid.Visibility = Visibility.Collapsed;
//
                // NOTE: removed the clearing for now.
                //_loadingGrid.Children.Clear();
            }
        }

        private void RestoreLoadingGrid()
        {
            Debug.WriteLine("RestoreLoadingGrid");
            if (!_contentPresenterUnloaded)
            {
                _loadingProcessDone = true;

                if (_loadingGrid != null)
                {
                    _loadingGrid.Visibility = System.Windows.Visibility.Visible;
                    _loadingGrid.Opacity = 1;
                }

                _contentPresenter.IsHitTestVisible = false;

                // Fade out the content
                if (_contentGrid != null)
                {
                    MyOpacityAnimator cg = null;
                    MyOpacityAnimator.EnsureAnimator(_contentGrid, ref cg);
                    if (cg != null)
                    {
                        cg.GoTo(0, TransitionDuration);
                    }
                    else
                    {
                        _contentGrid.Opacity = 0;
                    }
                }
            }
        }

        #endregion
    }
}
