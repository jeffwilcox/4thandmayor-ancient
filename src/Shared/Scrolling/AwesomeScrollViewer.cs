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
//#define DEBUG_ASV
#endif

// DEAR PERSON:
// You're very much on your own if you use this file.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Phone.Controls;

namespace JeffWilcox.Controls
{
    public class AwesomeScrollViewer : ContentControl
    {
        private static readonly TimeSpan DelayedBumpInterval = TimeSpan.FromMilliseconds(20);
        
        private static readonly bool _isDesigning;

        private const double Hardcoded = 125;

        private bool _setup = false;

        private const string TopJumpName = "Top";
        private const string BottomJumpName = "Bottom";

        private bool _isInVisualTree;

        private Rectangle _top;
        private Rectangle _bottom;

        private ScrollViewer _scrollViewer;
        private ContentPresenter _header;
        private bool _queuedBump;
        private readonly static List<WeakReference> _instances = new List<WeakReference>(3);
        private TransformYAnimator _ta;
        private bool _queuedBumpInProgress;
        private bool _cancelDelay;

        public ScrollViewer ActualScrollViewer
        { 
            get 
            { 
                return _scrollViewer;
            }
        }

        #region public bool IsPullToRefreshEnabled
        /// <summary>
        /// Gets or sets a value indicating whether pull to refresh is ok. Must
        /// be set at XAML parse time.
        /// </summary>
        public bool IsPullToRefreshEnabled
        {
            get { return (bool)GetValue(IsPullToRefreshEnabledProperty); }
            set { SetValue(IsPullToRefreshEnabledProperty, value); }
        }

        /// <summary>
        /// Identifies the IsPullToRefreshEnabled dependency property.
        /// </summary>
        public static readonly DependencyProperty IsPullToRefreshEnabledProperty =
            DependencyProperty.Register(
                "IsPullToRefreshEnabled",
                typeof(bool),
                typeof(AwesomeScrollViewer),
                new PropertyMetadata(false));
        #endregion public bool IsPullToRefreshEnabled

        #region public object Header
        /// <summary>
        /// Gets or sets the content for the header of the control.
        /// </summary>
        /// <value>
        /// The content for the header of the control. The default value is
        /// null.
        /// </value>
        public object Header
        {
            get { return GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        /// <summary>
        /// Identifies the
        /// <see cref="P:System.Windows.Controls.HeaderedContentControl.Header" />
        /// dependency property.
        /// </summary>
        /// <value>
        /// The identifier for the
        /// <see cref="P:System.Windows.Controls.HeaderedContentControl.Header" />
        /// dependency property.
        /// </value>
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(
                "Header",
                typeof (object),
                typeof(AwesomeScrollViewer),
                new PropertyMetadata(null));
        #endregion public object Header

        #region public DataTemplate HeaderTemplate
        /// <summary>
        /// Gets or sets the template that is used to display the content of the
        /// control's header.
        /// </summary>
        /// <value>
        /// The template that is used to display the content of the control's
        /// header. The default is null.
        /// </value>
        public DataTemplate HeaderTemplate
        {
            get { return (DataTemplate)GetValue(HeaderTemplateProperty); }
            set { SetValue(HeaderTemplateProperty, value); }
        }

        /// <summary>
        /// Identifies the
        /// <see cref="P:System.Windows.Controls.HeaderedContentControl.HeaderTemplate" />
        /// dependency property.
        /// </summary>
        /// <value>
        /// The identifier for the
        /// <see cref="P:System.Windows.Controls.HeaderedContentControl.HeaderTemplate" />
        /// dependency property.
        /// </value>
        public static readonly DependencyProperty HeaderTemplateProperty =
                DependencyProperty.Register(
                        "HeaderTemplate",
                        typeof(DataTemplate),
                        typeof(AwesomeScrollViewer),
                        new PropertyMetadata(null));

        #endregion public DataTemplate HeaderTemplate

        static AwesomeScrollViewer()
        {
            _isDesigning = DesignerProperties.IsInDesignTool;
        }

        public AwesomeScrollViewer()
        {
            DefaultStyleKey = typeof (AwesomeScrollViewer);
            _instances.Add(new WeakReference(this));

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _isInVisualTree = false;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _isInVisualTree = true;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Clip = new RectangleGeometry { Rect = new Rect(0, 0, ActualWidth, ActualHeight), };
        }

        //private static DateTime? _lastBumpQueue = null;
        //private static readonly TimeSpan QueueLimiterTimeSpan = TimeSpan.FromMilliseconds(80);
        public static void QueueBump()
        {
            //if (_lastBumpQueue.HasValue)
            //{
                //DateTime lv = _lastBumpQueue.Value;
                //DateTime ndt = lv + QueueLimiterTimeSpan;
                //DateTime now = DateTime.Now;
                /*Debug.WriteLine(lv.Ticks.ToString());
                Debug.WriteLine("    " + ndt.Ticks.ToString());
                Debug.WriteLine("         " + now.Ticks.ToString());*/

                //if (now.Ticks < ndt.Ticks)
                //{
                //    Debug.WriteLine("*");
                //    return;
                //}
            //}
            //if (_lastBumpQueue.HasValue && ((_lastBumpQueue.Va + QueueLimiterTimeSpan) < DateTime.Now))
            //{
            //    return;
            //}
#if DEBUG_ASV
            Debug.WriteLine("ASV: QueueBump, weaks refs: {0}", _instances.Count);
#endif
            //_lastBumpQueue = DateTime.Now;

            var delete = new List<WeakReference>();
            for (int i = 0; i < _instances.Count; ++i)
            {
                if (_instances[i].IsAlive)
                {
                    var asv = (AwesomeScrollViewer) _instances[i].Target;
                    if (asv._isInVisualTree && !asv._queuedBump)
                    {
                        asv._queuedBump = true;
                        asv.QueueBumpNow();
                    }
                    else
                    {
                        //Debug.WriteLine("ASV unknown condition {0} {1}", asv._isInVisualTree, asv._queuedBump);
                    }
                }
                else
                {
                    delete.Add(_instances[i]);
                }
            }

            // one delete per queue is enough
            if (delete.Count > 0)
            {
                foreach (var i in delete)
                {
#if DEBUG_ASV
                    Debug.WriteLine("ASV: Removing dead AwesomeSV ref.");
#endif
                    _instances.Remove(i);
                }
            }
        }

        public static void StaticBump()
        {
            for (int i = 0; i < _instances.Count; ++i)
            {
                if (_instances[i].IsAlive)
                {
                    var asv = (AwesomeScrollViewer)_instances[i].Target;
                    if (asv._isInVisualTree && !asv._queuedBump)
                    {
                        asv._queuedBump = true;
#if DEBUG_ASV
                        Debug.WriteLine("ASV: Smart scroll viewer bump");
#endif
                        asv.QueueBumpNow();
                    }
                }
            }
        }

        private void QueueBumpNow()
        {
            Dispatcher.BeginInvoke(Bump);
        }

        public override void OnApplyTemplate()
        {
            if (_scrollViewer != null && IsPullToRefreshEnabled)
            {
                _scrollViewer.ManipulationStarted -= OnManipulationStarted;
            }
            if (_top != null)
            {
                _top.MouseLeftButtonDown -= OnJumpToTop;
            }
            if (_bottom != null)
            {
                _bottom.MouseLeftButtonDown -= OnJumpToBottom;
            }

            _ta = null;
            base.OnApplyTemplate();

            if (IsPullToRefreshEnabled)
            {
                SizeChanged += OnSizeChanged;
            }

            _scrollViewer = MoreVisualTreeExtensions.FindFirstChildOfType<ScrollViewer>(this);
            if (null == _scrollViewer)
            {
                return;
                // Must be at design time.
                //throw new NotSupportedException("Control Template must include a ScrollViewer.");
            }

            Dispatcher.BeginInvoke(() =>
            {
                _top = VisualTreeExtensions.GetVisualDescendants(_scrollViewer).OfType<Rectangle>().Where(b => b.Name == TopJumpName).FirstOrDefault();
                _bottom = VisualTreeExtensions.GetVisualDescendants(_scrollViewer).OfType<Rectangle>().Where(b => b.Name == BottomJumpName).FirstOrDefault();

                if (_top != null)
                {
                    _top.MouseLeftButtonUp += OnJumpToTop;
                }
                if (_bottom != null)
                {
                    _bottom.MouseLeftButtonUp += OnJumpToBottom;
                }
            });

            _header = GetTemplateChild("Header") as ContentPresenter;
            if (_header != null)
            {
                TransformYAnimator.EnsureAnimator(_header, ref _ta);
            }

            if (IsPullToRefreshEnabled)
            {
                _scrollViewer.ManipulationStarted += OnManipulationStarted;
            }

            Dispatcher.BeginInvoke(() =>
            {
                int i = 0;
                UIElement uie = Parent as UIElement;
                while (i < 8 && uie != null && _piv == null)
                {
                    uie = VisualTreeHelper.GetParent(uie) as UIElement;
                    _piv = uie as LoadingPivotItem;
                }

                var implroot = VisualTreeHelper.GetChild(_scrollViewer, 0) as FrameworkElement;
                if (implroot != null)
                {
                    VisualStateGroup group = FindVisualState(implroot, "ScrollStates");
                    if (group != null)
                    {
                        group.CurrentStateChanging += OnScrollingStateChanging;
                        // should probably disconnect too
                    }
                }

                this.InvokeOnLayoutUpdated(Bump);
            });
        }

        private void OnJumpToTop(object sender, MouseButtonEventArgs e)
        {
            ScrollViewerExtensions.ScrollToTop(_scrollViewer);
        }

        private void OnJumpToBottom(object sender, MouseButtonEventArgs e)
        {
            ScrollViewerExtensions.ScrollToBottom(_scrollViewer);
        }

        //private PivotItem _piv;
        private LoadingPivotItem _piv;

        #region Pull to refresh

        private void OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            if (_scrollViewer.VerticalOffset <= 0.0)
            {
                _setup = true;
                _scrollViewer.AddHandler(ManipulationDeltaEvent,
                                         new EventHandler<ManipulationDeltaEventArgs>(
                                             OnManipulationDelta), true);

                _scrollViewer.ManipulationCompleted += OnManipulationCompleted;
            }
        }

        private void OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            _setup = false;
            if (_header != null)
            {
                // NOTE: When once again working on push to refresh... Actually want to delay one second first so the animation can be seen.
                _header.Visibility = Visibility.Collapsed;
            }

            // commit?
            if (_ta != null && _ta.CurrentOffset < 40)
            {
                //VibrateController.Default.Start(TimeSpan.FromMilliseconds(50));
            }

/*            _scrollViewer.RemoveHandler(UIElement.ManipulationDeltaEvent,
                         new EventHandler<System.Windows.Input.ManipulationDeltaEventArgs>(
                             OnManipulationDelta));*/
            _scrollViewer.ManipulationDelta -= OnManipulationDelta;
            _scrollViewer.ManipulationCompleted -= OnManipulationCompleted;
        }

        private static readonly Duration SimpleDuration = new Duration(TimeSpan.FromMilliseconds(50));
        private void OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            if (_setup)
            {
                _setup = false;
                if (_header != null)
                {
                    _ta.GoTo(-Hardcoded, new Duration(TimeSpan.Zero));
                    _header.Visibility = Visibility.Visible;
                }
            }
            double y = e.CumulativeManipulation.Translation.Y;
            if (y > Hardcoded && e.DeltaManipulation.Translation.Y <= 0)
            {
//                y = hardcoded + e.DeltaManipulation.Translation.Y;
            }
            //double yy = Math.Min(y, hardcoded);
            if (_ta != null)
            {

                _ta.GoTo(Math.Min(y - Hardcoded, 0), SimpleDuration);
            }
        }

        #endregion

        private void OnScrollingStateChanging(object sender, VisualStateChangedEventArgs e)
        {
            if ("NotScrolling" == e.NewState.Name)
            {
                QueueBumpNow();
            }
        }

        private void DelayedBump()
        {
            if (_queuedBumpInProgress || _isDesigning) // no designer support
            {
                return;
            }
#if DEBUG_ASV
            Debug.WriteLine("ASV: DelayedBump");
#endif
            _queuedBumpInProgress = true;
            var dt = new DispatcherTimer
            {
                Interval = DelayedBumpInterval,
            };
            dt.Tick += (x, xe) =>
                {
                    _queuedBumpInProgress = false;
#if DEBUG_ASV
                    Debug.WriteLine("ASV: DelayedBump.TICK");
#endif
                    dt.Stop();
                    dt = null;
                    if (!_cancelDelay)
                    {
                        Bump();
                    }
                    _cancelDelay = false;
                };
            dt.Start();
        }

        private void Bump()
        {
            //Debug.WriteLine("//bump");
            if (_scrollViewer == null || _isDesigning)
            {
#if DEBUG_ASV
                Debug.WriteLine("ASV: NO SCROLL VIEWER!");
#endif
                //DelayedBump();
                //QueueBumpNow();
                return;
            }

            // Was: HasLoadedOfficially

            if (_piv != null && !_piv.IsContentVisible)
            {
                // added this, 6/20/2011, to make images appear on first "pop".
                _queuedBumpInProgress = false;
                _queuedBump = false;

                //Debug.WriteLine("New change for new LPI: IsContentVisible check is false, returning instead.");
#if DEBUG_ASV
                Debug.WriteLine("ASV: No HasLoadedOfficially yet!");

#endif
                return;
            }

            _queuedBump = false;
            var frame = Application.Current.GetFrame();
            Point pt;

            try
            {
                // TODO: XXX: THIS IS A HEAVY PERFORMANCE HITTER!





                pt = _scrollViewer.TransformToVisual(frame).Transform(new Point(0, 0));




                //pt = LayoutInformation.GetLayoutSlot(_scrollViewer);
                //var x = LayoutInformation.GetLayoutSlot(_scrollViewer);
                //pt = new Point();




            }
            catch (ArgumentException)
            {
#if DEBUG_ASV
                Debug.WriteLine("ASV: We're not even in the visual tree maybe. So nothing will be happening, not even a queue! " + Name);
                System.Diagnostics.Debug.WriteLine("PERF! Throw in ASV transform to visual for " + Name);
#endif
                //QueueBumpNow();
                return;
            }
            if (pt.X < 0 || pt.X > frame.ActualWidth)
            {
#if DEBUG_ASV
                Debug.WriteLine("ASV: We're off-screen ~ delayed bumping!");
#endif
                //QueueBumpNow();
                DelayedBump();
                return;
            }
#if DEBUG_ASV
            Debug.WriteLine("ASV: Exposing images.");
#endif
            if (_queuedBumpInProgress)
            {
                _cancelDelay = true;
#if DEBUG_ASV
                Debug.WriteLine("ASV: Canceling delay");
#endif
            }

            double offset = _scrollViewer.VerticalOffset;
            double offsetMax = offset + _scrollViewer.ViewportHeight;

            // CONSIDER: CACHING THE IMAGES WE KNOW ABOUT!
            foreach (var image in this
                .GetVisualDescendants()
                .OfType<Image>())
            {
                var uri = AwesomeImage.GetUriSource(image);
                if (uri != null)
                {
                    try
                    {
                        var point = image.TransformToVisual(_scrollViewer).Transform(new Point(0, offset));
                        double y = point.Y;
                        double d = image.ActualHeight;
                        
                        // Use a decent default in this case.
                        if (d == 0.0)
                        {
                            d = 48.0;
                        }
#if DEBUG_ASV
                        double dh2 = image.Height;
#endif

                        //if (y >= offset - d && y + d <= offsetMax)
                        if (y + d >= offset && y - d <= offsetMax)
                        {
                            AwesomeImage.TransferToImage((Image)image, uri);
                        }
                        else
                        {
                            #if DEBUG_ASV
                            Debug.WriteLine("Image didn't make the cut: " + uri + " vertStart:" + y + " actualHeight:" + d + " height:" + dh2 + " vertOffset:" + offset + " vertOffsetMax:" + offsetMax);
#endif
                        }
                    }
                    catch (ArgumentException)
                    {
                        #if DEBUG_ASV
                        Debug.WriteLine("(caught argument exception in line 518)");
#endif
                    }
                }
            }
        }

        private static VisualStateGroup FindVisualState(FrameworkElement element, string name)
        {
            if (element == null)
            {
                return null;
            }

            IList groups = VisualStateManager.GetVisualStateGroups(element);
            foreach (VisualStateGroup group in groups)
            {
                if (group.Name == name)
                {
                    return group;
                }
            }

            return null;
        }

#if BLAH
        //private static readonly DependencyProperty VerticalOffsetShadowProperty =
        //DependencyProperty.Register("VerticalOffsetShadow", 
        //typeof(double), 
        //typeof(OnlyWhenNeededImage), 
        //new PropertyMetadata(-1.0, OnVerticalOffsetShadowChanged));
        //private static void OnVerticalOffsetShadowChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        //{
        //    // Handle ScrollViewer VerticalOffset change by unmasking newly visible content
        //    ((OnlyWhenNeededImage)o).UnmaskVisibleContent();
        //}

        private void UnmaskVisibleContent()
        {
            // Capture variables
            //var count = _scrollViewer.Con

            // Find index of any container within view using (1-indexed) binary search
            var index = -1;
            var l = 0;
            var r = count + 1;
            while (-1 == index)
            {
                var p = (r - l) / 2;
                if (0 == p)
                {
                    break;
                }
                p += l;
                var c = (DeferredLoadListBoxItem)_generator.ContainerFromIndex(p - 1);
                if (null == c)
                {
                    if (_inOnApplyTemplate)
                    {
                        // Applying template; don't expect to have containers at this point
                        return;
                    }
                    // Should always be able to get the container
                    var presenter = FindFirstChildOfType<ItemsPresenter>(_scrollViewer);
                    var panel = (null == presenter) ? null : FindFirstChildOfType<Panel>(presenter);
                    if (panel is VirtualizingStackPanel)
                    {
                        throw new NotSupportedException("Must change ItemsPanel to be a StackPanel (via the ItemsPanel property).");
                    }
                    else
                    {
                        throw new NotSupportedException("Couldn't find container for item (ItemsPanel should be a StackPanel).");
                    }
                }
                switch (Overlap(_scrollViewer, c, 0))
                {
                    case OverlapKind.Overlap:
                        index = p - 1;
                        break;
                    case OverlapKind.ChildAbove:
                        l = p;
                        break;
                    case OverlapKind.ChildBelow:
                        r = p;
                        break;
                }
            }

            if (-1 != index)
            {
                // Unmask visible items below the current item
                for (var i = index; i < count; i++)
                {
                    if (!UnmaskItemContent(i))
                    {
                        break;
                    }
                }

                // Unmask visible items above the current item
                for (var i = index - 1; 0 <= i; i--)
                {
                    if (!UnmaskItemContent(i))
                    {
                        break;
                    }
                }
            }
        }

        private bool UnmaskItemContent(int index)
        {
            var container = (DeferredLoadListBoxItem)_generator.ContainerFromIndex(index);
            if (null != container)
            {
                // Return quickly if not masked (but periodically check visibility anyway so we can stop once we're out of range)
                if (!container.Masked && (0 != (index % 16)))
                {
                    return true;
                }
                // Check necessary conditions
                if (0 == container.ActualHeight)
                {
                    throw new NotSupportedException("All containers must have a Height set (ex: via ItemContainerStyle), though the heights need not all need to be the same.");
                }
                // If container overlaps the "visible" area (i.e. on or near the screen), unmask it
                if (OverlapKind.Overlap == Overlap(_scrollViewer, container, 2 * _scrollViewer.ActualHeight))
                {
                    container.UnmaskContent();
                    return true;
                }
            }
            return false;
        }

        private static bool Overlap(double startA, double endA, double startB, double endB)
        {
            return (((startA <= startB) && (startB <= endA)) ||
                    ((startB <= startA) && (startA <= endB)));
        }
        private enum OverlapKind { Overlap, ChildAbove, ChildBelow };

        private static OverlapKind Overlap(FrameworkElement parent, FrameworkElement child, double padding)
        {
            // Get child bounds relative to parent
            var transform = child.TransformToVisual(parent);
            var bounds = new Rect(transform.Transform(new Point()), transform.Transform(new Point(/*child.ActualWidth*/ 0, child.ActualHeight)));
            // Return kind of overlap
            if (Overlap(0 - padding, parent.ActualHeight + padding, bounds.Top, bounds.Bottom))
            {
                return OverlapKind.Overlap;
            }
            else if (bounds.Top < 0)
            {
                return OverlapKind.ChildAbove;
            }
            else
            {
                return OverlapKind.ChildBelow;
            }
        }
#endif
    }
}
