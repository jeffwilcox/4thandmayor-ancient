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

using System.Windows;
using System.Windows.Controls;

namespace JeffWilcox.Controls
{
    [TemplateVisualState(GroupName = CoverStateGroupName, Name = CoveredStateName)]
    [TemplateVisualState(GroupName = CoverStateGroupName, Name = UncoveredStateName)]
    public class BaseCoverContentControl : ContentControl
    {
        #region Constants
        internal const string CoverStateGroupName = "Covering";
        internal const string CoveredStateName = "Covered";
        internal const string UncoveredStateName = "Uncovered";
        #endregion

        #region public object Cover
        /// <summary>
        /// Gets or sets the Cover Content.
        /// </summary>
        public object Cover
        {
            get { return GetValue(CoverProperty) as object; }
            set { SetValue(CoverProperty, value); }
        }

        /// <summary>
        /// Identifies the Cover dependency property.
        /// </summary>
        public static readonly DependencyProperty CoverProperty =
            DependencyProperty.Register(
                "Cover",
                typeof(object),
                typeof(BaseCoverContentControl),
                new PropertyMetadata(null));
        #endregion public object Cover

        #region public DataTemplate CoverTemplate
        /// <summary>
        /// Gets or sets the cover template.
        /// </summary>
        public DataTemplate CoverTemplate
        {
            get { return GetValue(CoverTemplateProperty) as DataTemplate; }
            set { SetValue(CoverTemplateProperty, value); }
        }

        /// <summary>
        /// Identifies the CoverTemplate dependency property.
        /// </summary>
        public static readonly DependencyProperty CoverTemplateProperty =
            DependencyProperty.Register(
                "CoverTemplate",
                typeof(DataTemplate),
                typeof(BaseCoverContentControl),
                new PropertyMetadata(null));
        #endregion public DataTemplate CoverTemplate

        #region public bool IsCovered
        /// <summary>
        /// Gets or sets a value indicating whether the content is covered.
        /// </summary>
        public bool IsCovered
        {
            get { return (bool)GetValue(IsCoveredProperty); }
            set { SetValue(IsCoveredProperty, value); }
        }

        /// <summary>
        /// Identifies the IsCovered dependency property.
        /// </summary>
        public static readonly DependencyProperty IsCoveredProperty =
            DependencyProperty.Register(
                "IsCovered",
                typeof(bool),
                typeof(BaseCoverContentControl),
                new PropertyMetadata(true, OnIsCoveredPropertyChanged));

        /// <summary>
        /// IsCoveredProperty property changed handler.
        /// </summary>
        /// <param name="d">CoverContentControl that changed its IsCovered.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnIsCoveredPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((BaseCoverContentControl)d).IsCoveredUpdated();
        }
        #endregion public bool IsCovered

        #region public Thickness CoverOffset
        /// <summary>
        /// Gets or sets the cover offset.
        /// </summary>
        public Thickness CoverOffset
        {
            get { return (Thickness)GetValue(CoverOffsetProperty); }
            set { SetValue(CoverOffsetProperty, value); }
        }

        /// <summary>
        /// Identifies the CoverOffset dependency property.
        /// </summary>
        public static readonly DependencyProperty CoverOffsetProperty =
            DependencyProperty.Register(
                "CoverOffset",
                typeof(Thickness),
                typeof(BaseCoverContentControl),
                new PropertyMetadata(null));
        #endregion public Thickness CoverOffset

        private bool _isUncoverImmediate;

        public BaseCoverContentControl()
            : base()
        {
            DefaultStyleKey = typeof(BaseCoverContentControl);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            UpdateVisualStates(false);
        }

        protected void UncoverQuickly()
        {
            _isUncoverImmediate = true;
            if (IsCovered)
            {
                IsCovered = false;
            }
        }

        protected virtual void IsCoveredUpdated()
        {
            UpdateVisualStates(!_isUncoverImmediate);

            if (_isUncoverImmediate)
            {
                _isUncoverImmediate = false;
            }
        }

        protected virtual void UpdateVisualStates(bool useTransitions)
        {
            VisualStateManager.GoToState(this,
                IsCovered ? CoveredStateName : UncoveredStateName,
                useTransitions);
        }
    }
}
