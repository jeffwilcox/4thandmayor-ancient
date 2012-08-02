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

#if DEBUG
//#define DEBUG_LCC
#endif

using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

namespace JeffWilcox.Controls
{
    // MUST: Support Unload or something that takes it back to the "Blank" or "Loading" cover state

    // TODO: Add ISupportPauseResume support.

    public interface ISupportLoadingRetry : ISendLoadStatus
    {
        void RetryLoad();
    }

    /// <summary>
    /// A specialized control whose Content is efficiently shown once it has
    /// been loaded into the visual tree. During that time, a Loading message
    /// appears. When using sophisticated data contexts that implement 
    /// specific interfaces, the control also supports load failure and 
    /// retry functionality.
    /// </summary>
    public class LoadingContentControl : CoverContentControl
    {
        #region public bool LoadImmediately
        /// <summary>
        /// Gets or sets a value indicating whether the control should try and 
        /// load the visuals immediately. For specialized users, such as pivot
        /// or panorama items, this should be turned off.
        /// </summary>
        public bool LoadImmediately
        {
            get { return (bool)GetValue(LoadImmediatelyProperty); }
            set { SetValue(LoadImmediatelyProperty, value); }
        }

        /// <summary>
        /// Identifies the LoadImmediately dependency property.
        /// </summary>
        public static readonly DependencyProperty LoadImmediatelyProperty =
            DependencyProperty.Register(
                "LoadImmediately",
                typeof(bool),
                typeof(LoadingContentControl),
                new PropertyMetadata(true));
        #endregion public bool LoadImmediately

        #region public object LoadingContent
        /// <summary>
        /// Gets or sets the loading content.
        /// </summary>
        public object LoadingContent
        {
            get { return GetValue(LoadingContentProperty) as object; }
            set { SetValue(LoadingContentProperty, value); }
        }

        /// <summary>
        /// Identifies the LoadingContent dependency property.
        /// </summary>
        public static readonly DependencyProperty LoadingContentProperty =
            DependencyProperty.Register(
                "LoadingContent",
                typeof(object),
                typeof(LoadingContentControl),
                new PropertyMetadata(null));
        #endregion public object LoadingContent

        #region public DataTemplate LoadingContentTemplate
        /// <summary>
        /// Gets or sets the template for loading.
        /// </summary>
        public DataTemplate LoadingContentTemplate
        {
            get { return GetValue(LoadingContentTemplateProperty) as DataTemplate; }
            set { SetValue(LoadingContentTemplateProperty, value); }
        }

        /// <summary>
        /// Identifies the LoadingContentTemplate dependency property.
        /// </summary>
        public static readonly DependencyProperty LoadingContentTemplateProperty =
            DependencyProperty.Register(
                "LoadingContentTemplate",
                typeof(DataTemplate),
                typeof(LoadingContentControl),
                new PropertyMetadata(null));
        #endregion public DataTemplate LoadingContentTemplate

        #region public object FailureContent
        /// <summary>
        /// 
        /// </summary>
        public object FailureContent
        {
            get { return GetValue(FailureContentProperty) as object; }
            set { SetValue(FailureContentProperty, value); }
        }

        /// <summary>
        /// Identifies the FailureContent dependency property.
        /// </summary>
        public static readonly DependencyProperty FailureContentProperty =
            DependencyProperty.Register(
                "FailureContent",
                typeof(object),
                typeof(LoadingContentControl),
                new PropertyMetadata(null));
        #endregion public object FailureContent

        #region public DataTemplate FailureContentTemplate
        /// <summary>
        /// 
        /// </summary>
        public DataTemplate FailureContentTemplate
        {
            get { return GetValue(FailureContentTemplateProperty) as DataTemplate; }
            set { SetValue(FailureContentTemplateProperty, value); }
        }

        /// <summary>
        /// Identifies the FailureContentTemplate dependency property.
        /// </summary>
        public static readonly DependencyProperty FailureContentTemplateProperty =
            DependencyProperty.Register(
                "FailureContentTemplate",
                typeof(DataTemplate),
                typeof(LoadingContentControl),
                new PropertyMetadata(null));
        #endregion public DataTemplate FailureContentTemplate

        #region public object TimeoutContent
        /// <summary>
        /// 
        /// </summary>
        public object TimeoutContent
        {
            get { return GetValue(TimeoutContentProperty) as object; }
            set { SetValue(TimeoutContentProperty, value); }
        }

        /// <summary>
        /// Identifies the TimeoutContent dependency property.
        /// </summary>
        public static readonly DependencyProperty TimeoutContentProperty =
            DependencyProperty.Register(
                "TimeoutContent",
                typeof(object),
                typeof(LoadingContentControl),
                new PropertyMetadata(null));
        #endregion public object TimeoutContent

        #region public DataTemplate TimeoutContentTemplate
        /// <summary>
        /// 
        /// </summary>
        public DataTemplate TimeoutContentTemplate
        {
            get { return GetValue(TimeoutContentTemplateProperty) as DataTemplate; }
            set { SetValue(TimeoutContentTemplateProperty, value); }
        }

        /// <summary>
        /// Identifies the TimeoutContentTemplate dependency property.
        /// </summary>
        public static readonly DependencyProperty TimeoutContentTemplateProperty =
            DependencyProperty.Register(
                "TimeoutContentTemplate",
                typeof(DataTemplate),
                typeof(LoadingContentControl),
                new PropertyMetadata(null));
        #endregion public DataTemplate TimeoutContentTemplate

        #region public object RetryContent
        /// <summary>
        /// 
        /// </summary>
        public object RetryContent
        {
            get { return GetValue(RetryContentProperty) as object; }
            set { SetValue(RetryContentProperty, value); }
        }

        /// <summary>
        /// Identifies the RetryContent dependency property.
        /// </summary>
        public static readonly DependencyProperty RetryContentProperty =
            DependencyProperty.Register(
                "RetryContent",
                typeof(object),
                typeof(LoadingContentControl),
                new PropertyMetadata(null));
        #endregion public object RetryContent

        #region public DataTemplate RetryContentTemplate
        /// <summary>
        /// 
        /// </summary>
        public DataTemplate RetryContentTemplate
        {
            get { return GetValue(RetryContentTemplateProperty) as DataTemplate; }
            set { SetValue(RetryContentTemplateProperty, value); }
        }

        /// <summary>
        /// Identifies the RetryContentTemplate dependency property.
        /// </summary>
        public static readonly DependencyProperty RetryContentTemplateProperty =
            DependencyProperty.Register(
                "RetryContentTemplate",
                typeof(DataTemplate),
                typeof(LoadingContentControl),
                new PropertyMetadata(null));
        #endregion public DataTemplate RetryContentTemplate

        #region public TimeSpan Timeout
        /// <summary>
        /// 
        /// </summary>
        public TimeSpan Timeout
        {
            get { return (TimeSpan)GetValue(TimeoutProperty); }
            set { SetValue(TimeoutProperty, value); }
        }

        /// <summary>
        /// Identifies the Timeout dependency property.
        /// </summary>
        public static readonly DependencyProperty TimeoutProperty =
            DependencyProperty.Register(
                "Timeout",
                typeof(TimeSpan),
                typeof(LoadingContentControl),
                new PropertyMetadata(new TimeSpan { }));
        #endregion public TimeSpan Timeout

        // rename to be clear it is a timespan?
        #region public TimeSpan LoadingDisplayOffset
        /// <summary>
        /// 
        /// </summary>
        public TimeSpan LoadingDisplayOffset
        {
            get { return (TimeSpan)GetValue(LoadingDisplayOffsetProperty); }
            set { SetValue(LoadingDisplayOffsetProperty, value); }
        }

        /// <summary>
        /// Identifies the LoadingDisplayOffset dependency property.
        /// </summary>
        public static readonly DependencyProperty LoadingDisplayOffsetProperty =
            DependencyProperty.Register(
                "LoadingDisplayOffset",
                typeof(TimeSpan),
                typeof(LoadingContentControl),
                new PropertyMetadata(new TimeSpan { } ));
        #endregion public TimeSpan LoadingDisplayOffset

        private LoadingState _state;
        private CoverState _coverState;
        private DispatcherTimer _timeoutTimer;

        private ISendLoadStatus _loadStatusObject;
        private ISupportPauseResume _pauseAndResumeChild;

        public event EventHandler ContentVisible;
        public event EventHandler ContentHidden;

        public ISupportLoadingRetry LoadingRetryInstance
        {
            get
            {
                return _loadStatusObject as ISupportLoadingRetry;
            }
        }

        internal ISupportPauseResume PauseResumeChild { get { return _pauseAndResumeChild; } }

        public ISendLoadStatus LoadStatusInstance
        {
            get
            {
                return _loadStatusObject;
            }
        }

        public LoadingContentControl()
            : base()
        {
            DefaultStyleKey = typeof(LoadingContentControl);

            UpdateState(LoadingState.Constructed); // can probably remove this one in production use
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            UpdateState(LoadingState.TemplateApplied);

            if (LoadImmediately)
            {
                Dispatcher.BeginInvoke(Load);
            }
        }

        public void Preload()
        {
            Debug("Preload");

            if (!HasContentLoaded)
            {
                // HACK: would be good to tighten this all up.
                if (_state != LoadingState.Failed && _state != LoadingState.TimedOut)
                {
                    UpdateCover(CoverState.Loading);
                }
            }
            else
            {
                if (!ReleaseContentTree)
                {
                    // Immediately show in this situation since
                    // it should still be in the visual tree.
                    UncoverQuickly();
                }
                else
                {
                    IsCovered = false;
                }
            }
        }

        public void Load()
        {
            Debug("Load!");
            if (_state == LoadingState.Failed)
            {
                return;
            }

            UpdateState(LoadingState.StartingLoad);

            if (!HasContentLoaded)
            {
                Debug("RealizeContent");
                base.RealizeContent();
            }
            else
            {
                IsCovered = false;
#if DEBUG
                // throw new NotImplementedException("??? hmm can this be hit ???");
#endif
            }
        }

        public override bool HasContentLoaded
        {
            get
            {
                Debug("Requesting HasContentLoaded..." + base.HasContentLoaded);
                bool b = base.HasContentLoaded;
                if (b && _loadStatusObject != null && _loadStatusObject.LoadStatus != LoadStatus.Loaded)
                {
                    Debug("!!! and lying"); // for FAILED and LOADING states.
                    return false;
                }
                return b;
            }
        }

        protected override void OnContentPresenterUnloaded(RoutedEventArgs e)
        {
            Debug("OnContentPresenterUnloaded");

            if (_timeoutTimer != null)
            {
                _timeoutTimer.Stop();
            }

            UpdateState(LoadingState.ContentPresenterUnloaded);

            base.OnContentPresenterUnloaded(e);

            // Just really needed to support working after a fail, etc.
            if (_loadStatusObject != null)
            {
                _loadStatusObject.LoadStatusChanged -= OnLoadStatusObjectChanged;
            }
        }

        protected override void OnContentPresenterLoaded(RoutedEventArgs e)
        {
            Debug("OnContentPresenterLoaded");

            base.OnContentPresenterLoaded(e);

            if (_state == LoadingState.StartingLoad || _state == LoadingState.ContentPresenterUnloaded)
            {
                // Check for the underlying ready state.
                UpdateState(LoadingState.ContentPresenterLoaded);

                // DIFFERENCE:
                // My original implementation had a minimum loading time.
                // If the time had not elapsed, this would invoke again.
                // The reason was probably to allow the DataContext and 
                // visuals to all be realized enough to compute whether 
                // it was a "smart" set of content or not.

                // CONSIDER:
                // Perhaps a DelayDispatchTimer here or similar.

                // TODO: Could make this configurable...

                IntervalDispatcher.BeginInvoke(TimeSpan.FromSeconds(.1), AnalyzeOrTransition);
            }
            else
            {
                var x = _state;
            }
        }

        private void AnalyzeOrTransition()
        {
            if (TryGetSmartStatus())
            {
                // Special work to do!
                Debug("LPI: Custom item base supports load notifications. Excellent." + _loadStatusObject.GetType().ToString());

                if (_loadStatusObject.LoadStatus == LoadStatus.Loaded || _loadStatusObject.LoadStatus == LoadStatus.Failed)
                {
                    Debug("Load status is ready already!");
                    OnceLoadStatusObjectReady();
                }
                else
                {
                    Debug("Load status is not ready yet.");

                    if (Timeout.TotalSeconds > 0.0)
                    {
                        if (_timeoutTimer != null)
                        {
                            _timeoutTimer.Stop();
                            _timeoutTimer = null;
                        }
                        _timeoutTimer = new DispatcherTimer
                        {
                            Interval = Timeout
                        };
                        _timeoutTimer.Tick += OnTimeout;
                        _timeoutTimer.Start();
                    }

                    _loadStatusObject.LoadStatusChanged += OnLoadStatusObjectChanged; // was: OnAdvancedWaitingElementLoadComplete;
                    //return; ?? this was in ancient LPI code, probably don't need any longer.
                }
            }
            else
            {
                Debug("Just a plain-old visual object.");
                TryTransitionToContent();
            }
        }

        private void OnTimeout(object sender, EventArgs e)
        {
            // This will be interesting to debug. If the LOAD ever comes in, it should
            // still be permitted to display the data.

            UpdateState(LoadingState.TimedOut);
            Debug("We timed out!");

            if (_timeoutTimer != null)
            {
                _timeoutTimer.Stop();
            }
        }

        private void OnLoadStatusObjectChanged(object sender, EventArgs e)
        {
            Debug("OnLoadStatusObjectChanged");
            if (_loadStatusObject != null && _loadStatusObject.LoadStatus != LoadStatus.Loading)
            {
                // Either Failed or Loaded.
                Debug("OnLoadStatusObjectChanged [failed or loaded]");

                // No longer detach the event handler here...

                StopTimeoutTimer();

                Dispatcher.BeginInvoke(OnceLoadStatusObjectReady);
            }
        }

        private void StopTimeoutTimer()
        {
            if (Dispatcher.CheckAccess())
            {
                if (_timeoutTimer != null)
                {
                    _timeoutTimer.Stop();
                    _timeoutTimer = null;
                }
            }
            else
            {
                Dispatcher.BeginInvoke(StopTimeoutTimer);
            }
        }

        // TODO: centralized IsCovered calls.

        private void OnceLoadStatusObjectReady()
        {
            Debug("OnceLoadStatusObjectReady");
            if (_loadStatusObject != null && _loadStatusObject.LoadStatus == LoadStatus.Failed)
            {
                UpdateState(LoadingState.Failed);
                if (LoadingRetryInstance != null && (RetryContent != null || RetryContentTemplate != null))
                {
                    Debug("Failed, but with a chance at redemption.");
                    UpdateCover(CoverState.FailureRetry);
                }
                else
                {
                    Debug("Failed, no change at retrying built in.");
                    UpdateCover(CoverState.Failure);
                }

                IsCovered = true;
                OnContentHidden(EventArgs.Empty);

                // Could be a specific state where it HAD data but now it FAILED, ... 

                //// TODO: This probably shouldn't be used... it's deprecated.
                //bool mayHaveHadDataAlready = false;
                //ISendLoadComplete islc = _loadStatusObject as ISendLoadComplete;
                //if (islc != null)
                //{
                //    if (islc.IsLoadComplete)
                //    {
                //        mayHaveHadDataAlready = true;
                //        // There "was" data and now there's been an error...
                //    }
                //}

                //if (!mayHaveHadDataAlready)
                //{
                //    UpdateCover(CoverState.Loading);
                //    IsCovered = true;
                //}
            }
            else
            {
                Dispatcher.BeginInvoke(TryTransitionToContent);
            }
        }

        private void RetryGettingPauseResumeChild()
        {
            var cp = ContentPresenter;
            if (cp == null)
            {
                return;
            }

            var descendantsOnce = cp
                .GetVisualDescendants()
                .ToList();

            _pauseAndResumeChild = descendantsOnce.OfType<ISupportPauseResume>().FirstOrDefault();
        }

        private bool TryGetSmartStatus()
        {
            var cp = ContentPresenter;
            if (cp == null)
            {
                return false;
            }

            var descendantsOnce = cp
                .GetVisualDescendants()
                .ToList();

            _pauseAndResumeChild = descendantsOnce.OfType<ISupportPauseResume>().FirstOrDefault();

            _loadStatusObject = descendantsOnce.OfType<ISendLoadStatus>().FirstOrDefault();

            // If the bindings never get applied due to visibility converters.
            var uie = _loadStatusObject as UIElement;
            if (uie != null && uie.Visibility == Visibility.Collapsed)
            {
                _loadStatusObject = null;
            }

            if (_loadStatusObject == null)
            {
                // Pause and resume is not support for these two.
                var fe = descendantsOnce
                    .OfType<ItemsControl>()
                    .Where(it => it.ItemsSource is ISendLoadStatus)
                    .FirstOrDefault();
                if (fe != null)
                {
                    _loadStatusObject = (ISendLoadStatus)fe.ItemsSource;
                }

                if (_loadStatusObject == null)
                {
                    var de = descendantsOnce
                        .OfType<FrameworkElement>()
                        .Where(ff => ff.DataContext is ISendLoadStatus)
                        .FirstOrDefault();
                    if (de != null)
                    {
                        _loadStatusObject = de.DataContext as ISendLoadStatus;
                    }
                }
            }

            return (_loadStatusObject != null);
        }

        private void TryTransitionToContent()
        {
            if (!HasContentLoaded)
            {
                Debug("TryTransitionToContent [no content loaded]");

                // The content presenter was Unloaded;
                // no need to process.
                UpdateState(LoadingState.ContentPresenterUnloaded);
                return;
            }

            if (PauseResumeChild == null)
            {
                RetryGettingPauseResumeChild();
            }

            Debug("TryTransitionToContent [content loaded!]");

            // TODO: Smart content loading implementation.

            UpdateState(LoadingState.ReadyToUncover);

            Dispatcher.BeginInvoke(UncoverContent);
        }

        private void UncoverContent()
        {
            Debug("UncoverContent");
            UpdateState(LoadingState.ContentUncovered);

            StopTimeoutTimer(); // should not be needed.

            IsCovered = false;
            OnContentVisible(EventArgs.Empty);
        }

        private void StartingLoadDelayExpired()
        {
            // Called once. However may need to check for other intermediate 
            // states as well for smart objects.
            if (_state == LoadingState.StartingLoad || 
                _state == LoadingState.ContentPresenterLoaded)
            {
                if (_loadStatusObject != null && _loadStatusObject.LoadStatus == LoadStatus.Failed)
                {
                    // special failed state...
                    return;
                }

                Debug("StartingLoadDelayExpired in a specific state.");
                // HACK: want to tighten up
                if (_state != LoadingState.TimedOut && _state != LoadingState.Failed)
                {
                    UpdateCover(CoverState.Loading);
                }
            }
        }

        protected virtual void OnContentVisible(EventArgs e)
        {
            var handler = ContentVisible;
            if (handler != null)
            {
                handler(this, e);
            }

            AwesomeScrollViewer.StaticBump();
        }

        protected virtual void OnContentHidden(EventArgs e)
        {
            var handler = ContentHidden;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        #region Loading State Management and Debugging

        // This state system is mostly for debugging.
        enum LoadingState
        {
            Unknown,
            Constructed,
            TemplateApplied,
            StartingLoad,
            ContentPresenterLoaded,
            ReadyToUncover,
            ContentUncovered,
            ContentPresenterUnloaded,
            Failed,
            TimedOut,
        }

        private void UpdateState(LoadingState state)
        {
            Debug("New State: " + state.ToString());

            if (_state != state)
            {
                _state = state;
                switch (state)
                {
                    case LoadingState.Constructed:
                        break;

                    case LoadingState.StartingLoad:
                        if (!HasContentLoaded)
                        {
                            if (LoadingDisplayOffset.TotalSeconds == 0.0 && _state != LoadingState.Failed)
                            {
                                Debug("Offset seconds of 0, going to loading state");
                                UpdateCover(CoverState.Loading);
                            }
                            else
                            {
                                // UpdateCover(CoverState.Blank); // ? is this necessary ?
                                IntervalDispatcher.BeginInvoke(LoadingDisplayOffset, StartingLoadDelayExpired);
                            }
                        }
                        break;

                    case LoadingState.TemplateApplied:
                        UpdateCover(CoverState.Blank);
                        System.Diagnostics.Debug.Assert(IsCovered == true);

                        break;

                    case LoadingState.Unknown:
                        break;

                    case LoadingState.ContentUncovered:
                        UpdateCover(CoverState.Blank);
                        break;

                    case LoadingState.ContentPresenterLoaded:
                    case LoadingState.ContentPresenterUnloaded:
                    case LoadingState.Failed:
                    case LoadingState.ReadyToUncover:
                        break;

                    case LoadingState.TimedOut:
                        UpdateCover(CoverState.Timeout);
                        break;

                    default:
//#if DEBUG
//                        throw new InvalidOperationException();
//#endif
                        break;
                }
            }
        }

        #endregion

        #region Cover State Management

        // Considered using a value converter as a template selector, 
        // but hard-coding for now instead.
        internal enum CoverState
        {
            Blank,
            Loading,
            Failure,
            Timeout,
            FailureRetry,
        }

        internal void UpdateCover(CoverState cs)
        {
            Debug("Cover selected + " + cs.ToString());

            if (_coverState != cs)
            {
                _coverState = cs;

                switch (_coverState)
                {
                    case CoverState.Blank:
                        Cover = null;
                        CoverTemplate = null;
                        break;

                    case CoverState.Timeout:
                        Cover = TimeoutContent;
                        CoverTemplate = TimeoutContentTemplate;
                        break;

                    case CoverState.Failure:
                        Cover = FailureContent;
                        CoverTemplate = FailureContentTemplate;
                        break;

                    case CoverState.FailureRetry:
                        Cover = RetryContent;
                        CoverTemplate = RetryContentTemplate;
                        break;

                    case CoverState.Loading:
                        Cover = LoadingContent;
                        CoverTemplate = LoadingContentTemplate;
                        break;

                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        #endregion

        [Conditional("DEBUG")]
        private void Debug(string s)
        {
#if DEBUG_LCC
            System.Diagnostics.Debug.WriteLine(s + "\t\t" + "(LCC) " + _state);
#endif
        }
    }
}
