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
using AgFx;
using Microsoft.Phone.Controls;
using MyOpacityAnimator = JeffWilcox.Controls.OpacityAnimator;

// TODO: CONSIDER: What if it never loads!? Need to "timeout"
// TODO: Failure reporting, retry, etc.

// A specialized version of the underlying system that wil lgo ahead and then
// allow the loading pivot item to also show a number of things when the pivot
// goes away from, too.

// A minimum amount of time - maybe 50ms - before showing the loading UI?

namespace JeffWilcox.Controls
{
    using FourthAndMayor;
    using System.ComponentModel;

    // TODO: Remove the Ex dep. from the parent.

    public class LoadingPivotItemEx : PivotItem
    {
        private const string LoadingGridPartName = "loadingGrid";
        private const string ContentGridPartName = "grid";
        private const string PerformanceProgressBarPartName = "progressBar";

        private static readonly TimeSpan MinimumLoadingTime = TimeSpan.FromSeconds(0.5);
        private static readonly TimeSpan AdditionalLoadTime = TimeSpan.FromSeconds(0.08);
        private static readonly Duration TransitionDuration = new Duration(TimeSpan.FromSeconds(0.5));

        private Pivot _pivot;
        private Grid _loadingGrid;
        private Grid _contentGrid;
        private ContentPresenter _contentPresenter;
        private ProgressBar _progressBar;

        private bool _contentPresenterUnloaded;
        private DateTime _okToShowAfter;
        private bool _loadingProcessDone;
        private bool _contentPresenterLoaded;
        private ISupportPauseResume _pauseAndResumeChild;
        private ISendLoadComplete _advancedWaitingElement;
        private bool _hasShownLoadingScreen;

        public bool IsMemoryEfficient { get; set; }

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
                typeof(LoadingPivotItemEx),
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
                typeof(LoadingPivotItemEx),
                new PropertyMetadata(new Thickness(0)));
        #endregion public Thickness LoadingMargin

        #region Control Basics
        public LoadingPivotItemEx()
            : base()
        {
            DefaultStyleKey = typeof(LoadingPivotItemEx);
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
            if (e != null && e.RemovedItems != null && e.RemovedItems.Count > 0)
            {
                var lpi = e.RemovedItems[0] as LoadingPivotItemEx;
                if (lpi != null && lpi == this && _loadingProcessDone)
                {
                    /// XXX: TEMP
                    _loadingProcessDone = false;
                    Content = null;
                    //Dispatcher.BeginInvoke(() => ShowLoadingScreen(false));
                }
            }

            if (e != null && e.AddedItems != null && e.AddedItems.Count > 0)
            {
                var lpi = e.AddedItems[0] as LoadingPivotItemEx;
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
            // LIPEx
//            if (_contentGrid == null)
//            {
//#if DEBUG
//                throw new InvalidOperationException("Content grid must be present.");
//#endif
//                return;
//            }
            // LPIEx
//            if (_contentPresenter != null)
//            {
//#if DEBUG
//                throw new InvalidOperationException("Unmasking cannot happen twice.");
//#endif
//                return;
//            }
            //_hasUnmaskedContent = true;

            var contentBinding = new Binding("Content") { BindsDirectlyToSource = true, Source = this };
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
        }

        private void ShowLoadingScreen(bool isCalledFromOnApplyTemplate)
        {
            if (_hasShownLoadingScreen)
            {
                Debug.WriteLine("LPIEx: hasshown load screen but yeah...");
                _hasShownLoadingScreen = false;
                //return;
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
            if (_loadingGrid.Visibility == Visibility.Collapsed)
            {
                _loadingGrid.Visibility = Visibility.Visible;
            }

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
            _advancedWaitingElement.LoadComplete -= OnAdvancedWaitingElementLoadComplete;
            OnceSpecializedContentIsReady();
        }

        private void OnceSpecializedContentIsReady()
        {
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
            _advancedWaitingElement = descendantsOnce.OfType<ISendLoadComplete>().FirstOrDefault();

            // If the bindings never get applied due to visibility converters.
            var uie = _advancedWaitingElement as UIElement;
            if (uie != null && uie.Visibility == Visibility.Collapsed)
            {
                _advancedWaitingElement = null;
            }

            if (_advancedWaitingElement == null)
            {
                // Pause and resume is not support for these two.
                var fe =
                    descendantsOnce.OfType<ItemsControl>().Where(it => it.ItemsSource is ISendLoadComplete).
                        FirstOrDefault();
                if (fe != null)
                {
                    _advancedWaitingElement = (ISendLoadComplete)fe.ItemsSource;
                }

                if (_advancedWaitingElement == null)
                {
                    var de =
                        descendantsOnce.OfType<FrameworkElement>().Where(ff => ff.DataContext is ISendLoadComplete)
                            .FirstOrDefault();
                    if (de != null)
                    {
                        _advancedWaitingElement = de.DataContext as ISendLoadComplete;
                    }
                }
            }

            if (_advancedWaitingElement != null)
            {
#if DEBUG_LOADING_PIVOT_ITEM
                Debug.WriteLine("LPI: Custom item base {0} supports load notifications. Excellent.", _advancedWaitingElement.GetType().ToString());
#endif
                if (_advancedWaitingElement.IsLoadComplete)
                    OnceSpecializedContentIsReady();
                else
                    _advancedWaitingElement.LoadComplete += OnAdvancedWaitingElementLoadComplete;
                return;
            }

            Debug.Assert(!_contentPresenterUnloaded);

            DelayedDispatcherTimer.BeginInvoke(AdditionalLoadTime, FinallyShowContent);
        }
        #endregion

        #region 4: Finally show the real pivot item's content to the world.
        private void FinallyShowContent()
        {
            if (!_contentPresenterUnloaded)
            {
                _loadingProcessDone = true;

                // No longer need this reference. Do still want resume though!
                // XXX: Removed for Ex
                //_advancedWaitingElement = null;

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
                // XXX: LoadingPivotItemEx
                // _loadingGrid.Children.Clear();
            }
        }
        #endregion
    }
}
