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
//#define DEBUG_CCC
#endif

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace JeffWilcox.Controls
{
    //[TemplateVisualState(GroupName = CoverStateGroupName, Name = CoveredStateName)]
    //[TemplateVisualState(GroupName = CoverStateGroupName, Name = UncoveredStateName)]

    public class CoverContentControl : BaseCoverContentControl
    {
        private const string ContentTemplatePart = "Content";

        #region public bool ReleaseContentTree
        /// <summary>
        /// Gets or sets a value indicating whether to release the content's visual
        /// tree when the Cover goes back up.
        /// </summary>
        public bool ReleaseContentTree
        {
            get { return (bool)GetValue(ReleaseContentTreeProperty); }
            set { SetValue(ReleaseContentTreeProperty, value); }
        }

        /// <summary>
        /// Identifies the ReleaseContentTree dependency property.
        /// </summary>
        public static readonly DependencyProperty ReleaseContentTreeProperty =
            DependencyProperty.Register(
                "ReleaseContentTree",
                typeof(bool),
                typeof(CoverContentControl),
                new PropertyMetadata(false));
        #endregion public bool ReleaseContentTree

        public event EventHandler ContentPresenterLoaded;
        public event EventHandler ContentPresenterUnloaded;

        private Panel _contentPanel;
        private VisualStateGroup _coveringStateGroup;
        private ContentPresenter _contentPresenter;

        private bool _hasContentLoaded;

        public CoverContentControl()
            : base()
        {
            DefaultStyleKey = typeof(CoverContentControl);
        }

        protected ContentPresenter ContentPresenter
        {
            get
            {
                return _contentPresenter;
            }
        }

        public override void OnApplyTemplate()
        {
            if (_coveringStateGroup != null)
            {
                _coveringStateGroup.CurrentStateChanging -= OnCoveringStateChanging;
                _coveringStateGroup.CurrentStateChanged -= OnCoveringStateChanged;
            }

            _contentPanel = GetTemplateChild(ContentTemplatePart) as Panel;

            _coveringStateGroup = VisualStates.TryGetVisualStateGroup(this, CoverStateGroupName);
            if (_coveringStateGroup != null)
            {
                _coveringStateGroup.CurrentStateChanged += OnCoveringStateChanged;
                _coveringStateGroup.CurrentStateChanging += OnCoveringStateChanging;
            }

            base.OnApplyTemplate(); // Not kosher but ...
            //UpdateVisualStates(false);
        }

        public virtual bool HasContentLoaded
        {
            get
            {
                return _hasContentLoaded;
            }
        }

        private void OnCoveringStateChanging(object sender, VisualStateChangedEventArgs e)
        {
            Debug("OnCoveringStateChanging");

            if (e.NewState != null && e.NewState.Name == UncoveredStateName && _contentPanel != null && _contentPanel.Children.Count == 0)
            {
                RealizeContent();
            }
        }

        public void RealizeContent()
        {
            if (_contentPresenter != null)
            {
                return;
                // throw new InvalidOperationException("Already realized!"); // probably want to not throw in the future, but just no-op out?
            }

            Debug("RealizeContent");

            _contentPresenter = new ContentPresenter
            {
                HorizontalAlignment = HorizontalContentAlignment,
                VerticalAlignment = VerticalContentAlignment,
                Margin = Padding,
                IsHitTestVisible = false,
            };

            _contentPresenter.Loaded += OnContentPresenterLoaded;
            _contentPresenter.Unloaded += OnContentPresenterUnloaded;

            _contentPresenter.SetBinding(
                ContentPresenter.ContentTemplateProperty,
                new Binding("ContentTemplate") { BindsDirectlyToSource = true, Source = this });
            _contentPresenter.SetBinding(
                ContentPresenter.ContentProperty,
                new Binding("Content") { BindsDirectlyToSource = true, Source = this });

            _contentPanel.Children.Add(_contentPresenter);
            _contentPanel.Visibility = Visibility.Visible; // ? is this one needed ?
        }

        private void ClearContentVisualTree()
        {
            Debug("ClearContentVisualTree");
            if (_contentPresenter != null)
            {
                //_contentPresenter.SetBinding(ContentPresenter.ContentTemplateProperty, null);
                //_contentPresenter.SetBinding(ContentPresenter.ContentTemplateProperty, null);
                
                _contentPresenter.Content = null;
                _contentPresenter.ContentTemplate = null;

                _contentPresenter.Loaded -= OnContentPresenterLoaded;
                _contentPresenter.Unloaded -= OnContentPresenterUnloaded;

                _contentPresenter = null;
                Debug("    also cleared the presenter.");
            }

            _contentPanel.Children.Clear();

            _hasContentLoaded = false;
        }

        private void OnContentPresenterUnloaded(object sender, RoutedEventArgs e)
        {
            _hasContentLoaded = false;

            OnContentPresenterUnloaded(e);
        }

        private void OnContentPresenterLoaded(object sender, RoutedEventArgs e)
        {
            _hasContentLoaded = true;

            OnContentPresenterLoaded(e);
        }

        protected virtual void OnContentPresenterUnloaded(RoutedEventArgs e)
        {
            var handler = ContentPresenterUnloaded;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnContentPresenterLoaded(RoutedEventArgs e)
        {
            var handler = ContentPresenterLoaded;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnCoveringStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            Debug("OnCoveringStateChanged");

            if (e.NewState != null && e.NewState.Name == CoveredStateName && _contentPanel != null && ReleaseContentTree)
            {
                ClearContentVisualTree();
            }
            else if (e.NewState != null && e.NewState.Name == UncoveredStateName && _contentPresenter != null && _contentPresenter.IsHitTestVisible == false)
            {
                _contentPresenter.IsHitTestVisible = true;
            }
        }

        [Conditional("DEBUG")]
        private void Debug(string s)
        {
#if DEBUG_CCC
            System.Diagnostics.Debug.WriteLine(s);
#endif
        }

/*        protected override void IsCoveredUpdated()
        {
            UpdateVisualStates(true);
        }
        */

        //protected virtual void UpdateVisualStates(bool useTransitions)
        //{
            //base.UpdateVisualStates(useTransitions);
            //VisualStateManager.GoToState(this,
            //    IsCovered ? CoveredStateName : UncoveredStateName,
            //    useTransitions);
        //}
    }
}
