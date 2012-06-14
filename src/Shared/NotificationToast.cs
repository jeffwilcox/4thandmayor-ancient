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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace JeffWilcox.Controls
{
    public class NotificationToast : HeaderedContentControl
    {
        private Grid _root;

        private TransformAnimator _ta;

        private OpacityAnimator _oa;

        private DispatcherTimer _dt;

        private Button _button;

        public event EventHandler Ignored;

        public event EventHandler Click;

        #region public TimeSpan NotificationTimeout
        /// <summary>
        /// Gets or sets the timeout period.
        /// </summary>
        public TimeSpan NotificationTimeout
        {
            get { return (TimeSpan)GetValue(NotificationTimeoutProperty); }
            set { SetValue(NotificationTimeoutProperty, value); }
        }

        /// <summary>
        /// Identifies the NotificationTimeout dependency property.
        /// </summary>
        public static readonly DependencyProperty NotificationTimeoutProperty =
            DependencyProperty.Register(
                "NotificationTimeout",
                typeof(TimeSpan),
                typeof(NotificationToast),
                new PropertyMetadata(TimeSpan.FromSeconds(5)));
        #endregion public TimeSpan NotificationTimeout

        public NotificationToast()
            : base()
        {
            DefaultStyleKey = typeof(NotificationToast);
            Loaded += new RoutedEventHandler(NotificationToast_Loaded);
        }

        void NotificationToast_Loaded(object sender, RoutedEventArgs e)
        {
            ResetTimer();
        }

        private bool _wasClicked;

        public override void OnApplyTemplate()
        {
            if (_button != null)
            {
                _button.Click -= OnClick;
            }

            base.OnApplyTemplate();

            _root = MoreVisualTreeExtensions.FindFirstChildOfType<Grid>(this);
            if (null == _root)
            {
                throw new NotSupportedException("Must include a Grid for manipulating.");
            }

            _button = GetTemplateChild("_button") as Button;
            if (_button != null)
            {
                _button.Click += OnClick;
            }

            TransformAnimator.EnsureAnimator(_root, ref _ta);
            OpacityAnimator.EnsureAnimator(_root, ref _oa);
            if (_oa != null)
            {
                _root.Opacity = 0;
                _oa.GoTo(1.0, new Duration(TimeSpan.FromSeconds(.5)));
            }
        }

        private void OnClick(object sender, RoutedEventArgs e)
        {
            // makes it fade out.
            //_wasClicked = true;
            //OnTick(sender, EventArgs.Empty);
        }

        private IEasingFunction _ease = new QuarticEase();

        protected override void OnManipulationStarted(ManipulationStartedEventArgs e)
        {
            base.OnManipulationStarted(e);
        }

        protected override void OnManipulationDelta(ManipulationDeltaEventArgs e)
        {
            TransformAnimator.EnsureAnimator(_root, ref _ta);
            if (_ta != null)
            {
                _ta.GoTo(e.CumulativeManipulation.Translation.X, new Duration(TimeSpan.FromMilliseconds(20)));
            }

            // Ignore a click now.
            if (_wasClicked && e.DeltaManipulation.Translation.X > 1)
            {
                _wasClicked = false;
            }

            base.OnManipulationDelta(e);
        }

        protected override void OnManipulationCompleted(ManipulationCompletedEventArgs e)
        {
            // Flick off-screen.
            if (e.IsInertial && e.TotalManipulation.Translation.X > 0)
            {
                _wasClicked = false;

                TransformAnimator.EnsureAnimator(_root, ref _ta);
                if (_ta != null)
                {
                    _ta.GoTo(ActualWidth + 1.0, new Duration(TimeSpan.FromSeconds(.5)), _ease, OnClosed);
                }
                else OnClosed();
            }
            else if (e.TotalManipulation.Translation.X > 20)
            {
                // Resetting, so if they clicked, forget it.
                if (_wasClicked && e.TotalManipulation.Translation.X > 20)
                {
                    _wasClicked = false;
                }

                // Return and RESET the timer.
                TransformAnimator.EnsureAnimator(_root, ref _ta);
                ResetTimer(); // don't fire yet.
                if (_ta != null)
                {
                    _ta.GoTo(0.0, new Duration(TimeSpan.FromSeconds(.5)), _ease, ResetTimer); // reset once back.
                }
            }
            else
            {
                // A click!
                _wasClicked = true;
                OnTick(this, EventArgs.Empty);
            }

            base.OnManipulationCompleted(e);
        }

        private void ResetTimer()
        {
            if (_dt != null)
            {
                _dt.Stop();
                _dt.Tick -= OnTick;
            }

            _dt = new DispatcherTimer();
            _dt.Interval = NotificationTimeout;
            _dt.Tick += OnTick;
            _dt.Start();
        }

        void OnTick(object sender, EventArgs e)
        {
            if (_dt != null)
            {
                _dt.Stop();
                _dt = null;
            }

            if (_oa != null)
            {
                _oa.GoTo(0.0, new Duration(TimeSpan.FromSeconds(.5)), OnClosed);
            }
            else OnClosed();
        }

        private void OnClosed()
        {
            if (_dt != null)
            {
                _dt.Stop();
                _dt.Tick -= OnTick;
            }

            // TODO: NOTIFY!
            if (_wasClicked)
            {
                // Action to take!
                var handler = Click;
                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                }
            }
            else
            {
                // Ignore.
                var handler = Ignored;
                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                }
            }

            // AUTOMATICALLY remove from the parent visual tree.
            var me = this;
            Dispatcher.BeginInvoke(() =>
            {
                Panel p = me.Parent as Panel;
                if (p != null)
                {
                    p.Children.Remove(me);
                }
            });
        }
    }
}
