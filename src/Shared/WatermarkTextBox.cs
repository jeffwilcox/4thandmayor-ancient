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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace JeffWilcox.Controls
{
    public class WatermarkedTextBox : Control
    {
        #region public Brush WatermarkBrush
        /// <summary>
        /// Gets or sets the watermark brush color.
        /// </summary>
        public Brush WatermarkBrush
        {
            get { return (Brush)GetValue(WatermarkBrushProperty); }
            set { SetValue(WatermarkBrushProperty, value); }
        }

        /// <summary>
        /// Identifies the WatermarkBrush dependency property.
        /// </summary>
        public static readonly DependencyProperty WatermarkBrushProperty =
            DependencyProperty.Register(
                "WatermarkBrush",
                typeof(Brush),
                typeof(WatermarkedTextBox),
                new PropertyMetadata(null));
        #endregion public Brush WatermarkBrush

        #region public string Text
        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        public string Text
        {
            get { return GetValue(TextProperty) as string; }
            set { SetValue(TextProperty, value); }
        }

        /// <summary>
        /// Identifies the Text dependency property.
        /// </summary>
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                "Text",
                typeof(string),
                typeof(WatermarkedTextBox),
                new PropertyMetadata(string.Empty));
        #endregion public string Text

        #region public string Watermark
        /// <summary>
        /// Gets or sets the watermark.
        /// </summary>
        public string Watermark
        {
            get { return GetValue(WatermarkProperty) as string; }
            set { SetValue(WatermarkProperty, value); }
        }

        /// <summary>
        /// Identifies the Watermark dependency property.
        /// </summary>
        public static readonly DependencyProperty WatermarkProperty =
            DependencyProperty.Register(
                "Watermark",
                typeof(string),
                typeof(WatermarkedTextBox),
                new PropertyMetadata(null));
        #endregion public string Watermark

        #region public InputScope InputScope
        /// <summary>
        /// 
        /// </summary>
        public InputScope InputScope
        {
            get { return (InputScope)GetValue(InputScopeProperty); }
            set { SetValue(InputScopeProperty, value); }
        }

        /// <summary>
        /// Identifies the InputScope dependency property.
        /// </summary>
        public static readonly DependencyProperty InputScopeProperty =
            DependencyProperty.Register(
                "InputScope",
                typeof(InputScope),
                typeof(WatermarkedTextBox),
                new PropertyMetadata(null));
        #endregion public InputScope InputScope

        #region public TextWrapping TextWrapping
        /// <summary>
        /// 
        /// </summary>
        public TextWrapping TextWrapping
        {
            get { return (TextWrapping)GetValue(TextWrappingProperty); }
            set { SetValue(TextWrappingProperty, value); }
        }

        /// <summary>
        /// Identifies the TextWrapping dependency property.
        /// </summary>
        public static readonly DependencyProperty TextWrappingProperty =
            DependencyProperty.Register(
                "TextWrapping",
                typeof(TextWrapping),
                typeof(WatermarkedTextBox),
                new PropertyMetadata(TextWrapping.NoWrap));
        #endregion public TextWrapping TextWrapping


        private TextBox _text;

        private bool _hasText;

        private bool _hasFocus;

        public WatermarkedTextBox()
        {
            DefaultStyleKey = typeof(WatermarkedTextBox);
        }

        private void UpdateVisualStates(bool useTransitions)
        {
            VisualStateManager.GoToState(this, 
                _hasText ? "Normal" : "Watermarked"
                , useTransitions);
            VisualStateManager.GoToState(this,
                _hasFocus ? "Focused" : "Unfocused"
                , useTransitions);
        }

        public override void OnApplyTemplate()
        {
            if (_text != null)
            {
                _text.GotFocus -= OnGotFocus;
                _text.LostFocus -= OnLostFocus;
                _text.TextChanged -= OnTextChanged;
            }

            base.OnApplyTemplate();

            _text = GetTemplateChild("_text") as TextBox;
            if (_text != null)
            {
                _text.GotFocus += OnGotFocus;
                _text.LostFocus += OnLostFocus;
                _text.TextChanged += OnTextChanged;
            }

            UpdateVisualStates(false);
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            _hasText = !(string.IsNullOrEmpty(_text.Text));
            UpdateVisualStates(true);

            if (Text != _text.Text)
            {
                Text = _text.Text;
            }

            var handler = TextChanged;
            if (handler != null)
            {
                handler(_text, e); // note: sending the text as event source.
            }
        }

        public event TextChangedEventHandler TextChanged;

        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            _hasFocus = false;
            UpdateVisualStates(true);
        }

        private void OnGotFocus(object sender, RoutedEventArgs e)
        {
            _hasFocus = true;
            UpdateVisualStates(true);
        }
    }
}