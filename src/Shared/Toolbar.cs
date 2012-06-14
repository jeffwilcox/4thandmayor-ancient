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
    /// <summary>
    /// Represents a control that can slide in and out of the screen by the
    /// graphics accelerator, based on the object's visual size.
    /// </summary>
    [TemplateVisualState(GroupName = StateGroupName, Name = OpenState)]
    [TemplateVisualState(GroupName = StateGroupName, Name = ClosedUp)]
    [TemplateVisualState(GroupName = StateGroupName, Name = ClosedDown)]
    [TemplateVisualState(GroupName = StateGroupName, Name = ClosedLeft)]
    [TemplateVisualState(GroupName = StateGroupName, Name = ClosedRight)]
    public class Toolbar : ContentControl
    {
        #region Constants
        private const string GridPartName = "Grid";
        private const string StateGroupName = "OpenClosedStates";
        private const string OpenState = "Open";
        private const string ClosedUp = "Up";
        private const string ClosedDown = "Down";
        private const string ClosedLeft = "Left";
        private const string ClosedRight = "Right";
        #endregion

        #region public bool IsOpen
        /// <summary>
        /// Gets or sets a value indicating whether the control is open.
        /// </summary>
        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }

        /// <summary>
        /// Identifies the IsOpen dependency property.
        /// </summary>
        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register(
                "IsOpen",
                typeof(bool),
                typeof(Toolbar),
                new PropertyMetadata(true, OnIsOpenPropertyChanged));

        /// <summary>
        /// IsOpenProperty property changed handler.
        /// </summary>
        /// <param name="d">Toolbar that changed its IsOpen.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnIsOpenPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Toolbar source = d as Toolbar;
            // bool value = (bool)e.NewValue;

            source.UpdateVisualStates(true);
        }
        #endregion public bool IsOpen

        #region public TranslationDirection TranslationDirection
        /// <summary>
        /// Gets or sets the translation direction.
        /// </summary>
        public TranslationDirection TranslationDirection
        {
            get { return (TranslationDirection)GetValue(TranslationDirectionProperty); }
            set { SetValue(TranslationDirectionProperty, value); }
        }

        /// <summary>
        /// Identifies the TranslationDirection dependency property.
        /// </summary>
        public static readonly DependencyProperty TranslationDirectionProperty =
            DependencyProperty.Register(
                "TranslationDirection",
                typeof(TranslationDirection),
                typeof(Toolbar),
                new PropertyMetadata(TranslationDirection.Up));
        #endregion public TranslationDirection TranslationDirection

        public Toolbar()
        {
            DefaultStyleKey = typeof(Toolbar);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            UpdateVisualStates(false);
        }

        private void UpdateVisualStates(bool useTransitions)
        {
            string closedStateName = TranslationDirection.ToString();

            VisualStateManager.GoToState(
                this,
                IsOpen ? OpenState : closedStateName,
                useTransitions);
        }
    }
}