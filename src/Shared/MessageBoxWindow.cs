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
using System.Windows.Media;

namespace JeffWilcox.Controls
{
    public class MessageBoxWindow : WindowBase
    {
        private Button _button1;

        private Button _button2;

        #region public object CheckBoxContent
        /// <summary>
        /// Gets or sets the checkbox content.
        /// </summary>
        public object CheckBoxContent
        {
            get { return GetValue(CheckBoxContentProperty) as object; }
            set { SetValue(CheckBoxContentProperty, value); }
        }

        /// <summary>
        /// Identifies the CheckBoxContent dependency property.
        /// </summary>
        public static readonly DependencyProperty CheckBoxContentProperty =
            DependencyProperty.Register(
                "CheckBoxContent",
                typeof(object),
                typeof(MessageBoxWindow),
                new PropertyMetadata(null, OnCheckBoxContentPropertyChanged));

        /// <summary>
        /// CheckBoxContentProperty property changed handler.
        /// </summary>
        /// <param name="d">MessageBoxWindow that changed its CheckBoxContent.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnCheckBoxContentPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MessageBoxWindow source = d as MessageBoxWindow;
            object value = e.NewValue as object;

            if (value == null)
            {
                source.CheckBoxVisibility = Visibility.Collapsed;
            }
            else
            {
                source.CheckBoxVisibility = Visibility.Visible;
            }
        }
        #endregion public object CheckBoxContent

        #region public Visibility CheckBoxVisibility
        /// <summary>
        /// Gets or sets the visibility of the check box control. Not intended for regular use.
        /// </summary>
        public Visibility CheckBoxVisibility
        {
            get { return (Visibility)GetValue(CheckBoxVisibilityProperty); }
            set { SetValue(CheckBoxVisibilityProperty, value); }
        }

        /// <summary>
        /// Identifies the CheckBoxVisibility dependency property.
        /// </summary>
        public static readonly DependencyProperty CheckBoxVisibilityProperty =
            DependencyProperty.Register(
                "CheckBoxVisibility",
                typeof(Visibility),
                typeof(MessageBoxWindow),
                new PropertyMetadata(Visibility.Collapsed));
        #endregion public Visibility CheckBoxVisibility

        private CheckBox _check;
        #region public bool? IsChecked
        /// <summary>
        /// Gets or sets the is checked property.
        /// </summary>
        public bool? IsChecked
        {
            get { return (bool?)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }

        /// <summary>
        /// Identifies the IsChecked dependency property.
        /// </summary>
        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register(
                "IsChecked",
                typeof(bool?),
                typeof(MessageBoxWindow),
                new PropertyMetadata(false));
        #endregion public bool? IsChecked

        #region public string LeftButtonText
        /// <summary>
        /// 
        /// </summary>
        public string LeftButtonText
        {
            get { return GetValue(LeftButtonTextProperty) as string; }
            set { SetValue(LeftButtonTextProperty, value); }
        }

        /// <summary>
        /// Identifies the LeftButtonText dependency property.
        /// </summary>
        public static readonly DependencyProperty LeftButtonTextProperty =
            DependencyProperty.Register(
                "LeftButtonText",
                typeof(string),
                typeof(MessageBoxWindow),
                new PropertyMetadata(null, OnLeftButtonTextPropertyChanged));

        /// <summary>
        /// LeftButtonTextProperty property changed handler.
        /// </summary>
        /// <param name="d">MessageBoxWindow that changed its LeftButtonText.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnLeftButtonTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MessageBoxWindow source = d as MessageBoxWindow;
            string value = e.NewValue as string;

            source.LeftButtonVisibility = value == null ? Visibility.Collapsed : Visibility.Visible;
        }
        #endregion public string LeftButtonText

        #region public string RightButtonText
        /// <summary>
        /// 
        /// </summary>
        public string RightButtonText
        {
            get { return GetValue(RightButtonTextProperty) as string; }
            set { SetValue(RightButtonTextProperty, value); }
        }

        /// <summary>
        /// Identifies the RightButtonText dependency property.
        /// </summary>
        public static readonly DependencyProperty RightButtonTextProperty =
            DependencyProperty.Register(
                "RightButtonText",
                typeof(string),
                typeof(MessageBoxWindow),
                new PropertyMetadata(null, OnRightButtonTextPropertyChanged));

        /// <summary>
        /// RightButtonTextProperty property changed handler.
        /// </summary>
        /// <param name="d">MessageBoxWindow that changed its RightButtonText.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnRightButtonTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MessageBoxWindow source = d as MessageBoxWindow;
            string value = e.NewValue as string;

            source.RightButtonVisibility = value == null ? Visibility.Collapsed : Visibility.Visible;
        }
        #endregion public string RightButtonText

        #region public Visibility LeftButtonVisibility
        /// <summary>
        /// 
        /// </summary>
        public Visibility LeftButtonVisibility
        {
            get { return (Visibility)GetValue(LeftButtonVisibilityProperty); }
            set { SetValue(LeftButtonVisibilityProperty, value); }
        }

        /// <summary>
        /// Identifies the LeftButtonVisibility dependency property.
        /// </summary>
        public static readonly DependencyProperty LeftButtonVisibilityProperty =
            DependencyProperty.Register(
                "LeftButtonVisibility",
                typeof(Visibility),
                typeof(MessageBoxWindow),
                new PropertyMetadata(Visibility.Collapsed));
        #endregion public Visibility LeftButtonVisibility

        #region public Visibility RightButtonVisibility
        /// <summary>
        /// 
        /// </summary>
        public Visibility RightButtonVisibility
        {
            get { return (Visibility)GetValue(RightButtonVisibilityProperty); }
            set { SetValue(RightButtonVisibilityProperty, value); }
        }

        /// <summary>
        /// Identifies the RightButtonVisibility dependency property.
        /// </summary>
        public static readonly DependencyProperty RightButtonVisibilityProperty =
            DependencyProperty.Register(
                "RightButtonVisibility",
                typeof(Visibility),
                typeof(MessageBoxWindow),
                new PropertyMetadata(Visibility.Collapsed));
        #endregion public Visibility RightButtonVisibility

        public event EventHandler LeftButtonClick;

        public event EventHandler RightButtonClick;

        public MessageBoxWindow()
            : base()
        {
            DefaultStyleKey = typeof(MessageBoxWindow);
        }

        //private bool _hasAnimatedIn;

        public static MessageBoxWindow Show(string text)
        {
            return Show(text, null, "Ok");
        }

        public static MessageBoxWindow Show(string text, string caption, string leftButton)
        {
            return Show(text, caption, leftButton, null);
        }

        public static MessageBoxWindow Show(string text, string caption, string leftButton, string rightButton)
        {
            return Show(text, caption, leftButton, rightButton, null);
        }

        public static MessageBoxWindow Show(string text, string caption, MessageBoxButton buttons)
        {
            string left = "ok";
            string right = buttons == MessageBoxButton.OKCancel ? "cancel" : null;

            return Show(text, caption, left, right);
        }

        //private bool _clicked;

        public static MessageBoxWindow Show(string text, string caption, string leftButton, string rightButton, string checkBoxContent)
        {
            var win = new MessageBoxWindow();
            win.Content = text;
            win.Header = caption;
            win.LeftButtonText = leftButton;
            win.RightButtonText = rightButton;
            win.CheckBoxContent = checkBoxContent;

            win.InsertIntoFrame();

            return win;
        }

        public override void OnApplyTemplate()
        {
            if (_button1 != null)
            {
                _button1.Click -= OnClick;
            }
            if (_button2 != null)
            {
                _button2.Click -= OnClick;
            }

            base.OnApplyTemplate();

            _button1 = GetTemplateChild("_left") as Button;
            if (_button1 != null)
            {
                _button1.Click += OnClick;
            }
            _button2 = GetTemplateChild("_right") as Button;
            if (_button2 != null)
            {
                _button2.Click += OnClick;
            }

            _check = GetTemplateChild("_check") as CheckBox;
        }

        private void OnClick(object sender, RoutedEventArgs e)
        {
            if (_check != null)
            {
                IsChecked = _check.IsChecked;
            }

            if (sender == _button1)
            {
                var handler = LeftButtonClick;
                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                }
            }
            else if (sender == _button2)
            {
                var handler = RightButtonClick;
                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                }
            }

            CloseWindow();
        }
    }
}
