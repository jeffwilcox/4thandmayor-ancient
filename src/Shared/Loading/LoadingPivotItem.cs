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
//#define DEBUG_LPI
#endif

// TODO: Make memory efficient mode look smoother between swipes.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Phone.Controls;

namespace JeffWilcox.Controls
{
    /// <summary>
    /// Represents a pivot item with special loading capabilities that can be
    /// used to improve perceived performance of complex visuals, show a 
    /// loading screen (even with failure and retry features) for data bound
    /// content, and more.
    /// </summary>
    [TemplatePart(Name = LoadingContentControlName, Type = typeof(LoadingContentControl))]
    public class LoadingPivotItem : PivotItem
    {
        private const string LoadingContentControlName = "Content";

        #region public attached bool IsMemoryEfficient
        /// <summary>
        /// Gets the value of the IsMemoryEfficient attached property for a specified UIElement.
        /// </summary>
        /// <param name="element">The UIElement from which the property value is read.</param>
        /// <returns>The IsMemoryEfficient property value for the UIElement.</returns>
        public static bool GetIsMemoryEfficient(UIElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            return (bool)element.GetValue(IsMemoryEfficientProperty);
        }

        /// <summary>
        /// Sets the value of the IsMemoryEfficient attached property to a specified UIElement.
        /// </summary>
        /// <param name="element">The UIElement to which the attached property is written.</param>
        /// <param name="value">The needed IsMemoryEfficient value.</param>
        public static void SetIsMemoryEfficient(UIElement element, bool value)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            element.SetValue(IsMemoryEfficientProperty, value);
        }

        /// <summary>
        /// Identifies the IsMemoryEfficient dependency property.
        /// </summary>
        public static readonly DependencyProperty IsMemoryEfficientProperty =
            DependencyProperty.RegisterAttached(
                "IsMemoryEfficient",
                typeof(bool),
                typeof(LoadingPivotItem),
                new PropertyMetadata(false));

        #endregion public attached bool IsMemoryEfficient

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

        #region public string TimeoutText
        /// <summary>
        /// 
        /// </summary>
        public string TimeoutText
        {
            get { return GetValue(TimeoutTextProperty) as string; }
            set { SetValue(TimeoutTextProperty, value); }
        }

        /// <summary>
        /// Identifies the TimeoutText dependency property.
        /// </summary>
        public static readonly DependencyProperty TimeoutTextProperty =
            DependencyProperty.Register(
                "TimeoutText",
                typeof(string),
                typeof(LoadingPivotItem),
                new PropertyMetadata(null));
        #endregion public string TimeoutText

        #region public string FailureText
        /// <summary>
        /// 
        /// </summary>
        public string FailureText
        {
            get { return GetValue(FailureTextProperty) as string; }
            set { SetValue(FailureTextProperty, value); }
        }

        /// <summary>
        /// Identifies the FailureText dependency property.
        /// </summary>
        public static readonly DependencyProperty FailureTextProperty =
            DependencyProperty.Register(
                "FailureText",
                typeof(string),
                typeof(LoadingPivotItem),
                new PropertyMetadata(null));
        #endregion public string FailureText

        #region public string RetryText
        /// <summary>
        /// 
        /// </summary>
        public string RetryText
        {
            get { return GetValue(RetryTextProperty) as string; }
            set { SetValue(RetryTextProperty, value); }
        }

        /// <summary>
        /// Identifies the RetryText dependency property.
        /// </summary>
        public static readonly DependencyProperty RetryTextProperty =
            DependencyProperty.Register(
                "RetryText",
                typeof(string),
                typeof(LoadingPivotItem),
                new PropertyMetadata(null));
        #endregion public string RetryText

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
                typeof(LoadingPivotItem),
                new PropertyMetadata(new TimeSpan { }));
        #endregion public TimeSpan Timeout

#if THIS_IS_CUT
        #region public bool IsItemMemoryEfficient
        /// <summary>
        /// Gets or sets a value indicating whether the control is memory
        /// efficient. In the memory efficient mode, after moving away from
        /// a LoadingPivotItem, its contents are removed from the visual
        /// tree. By default this value is off.
        /// </summary>
        public bool IsItemMemoryEfficient
        {
            get { return (bool)GetValue(IsItemMemoryEfficientProperty); }
            set { SetValue(IsItemMemoryEfficientProperty, value); }
        }

        /// <summary>
        /// Identifies the IsMemoryEfficient dependency property.
        /// </summary>
        public static readonly DependencyProperty IsItemMemoryEfficientProperty =
            DependencyProperty.Register(
                "IsItemMemoryEfficient",
                typeof(bool),
                typeof(LoadingPivotItem),
                new PropertyMetadata(false));
        #endregion public bool IsItemMemoryEfficient
#endif

        private Pivot _pivot;
        private LoadingContentControl _content;

        public bool IsContentVisible { get; private set; }

        public LoadingPivotItem()
            : base()
        {
            DefaultStyleKey = typeof(LoadingPivotItem);
        }

        public override void OnApplyTemplate()
        {
            if (_content != null)
            {
                _content.ContentVisible -= OnContentVisible;
                _content.ContentHidden -= OnContentHidden;
            }

            base.OnApplyTemplate();

            _content = GetTemplateChild(LoadingContentControlName) as LoadingContentControl;

            if (_content != null)
            {
                _content.ContentVisible += OnContentVisible;
                _content.ContentHidden += OnContentHidden;
            }

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

                if (_content != null)
                {
                    _content.ReleaseContentTree = GetIsMemoryEfficient(_pivot);
                }

                _pivot.SelectionChanged += OnPivotSelectionChanged;

                // TODO: this was more efficient before

                _pivot.LoadedPivotItem += OnLoadedPivotItem;
                _pivot.UnloadingPivotItem += OnUnloadingPivotItem;

                if (_pivot.SelectedItem == this && _content != null)
                {
                    Dispatcher.BeginInvoke(_content.Load);
                }
            }
        }

        private void OnContentHidden(object sender, EventArgs e)
        {
            IsContentVisible = false;
        }

        private void OnContentVisible(object sender, EventArgs e)
        {
            // Debug.WriteLine("IsContentVisible = true;");
            IsContentVisible = true;
        }

        private void Cover()
        {
            // TODO: An Unload method? If Efficient, Blank; Otherwise Loading placeholder???
            _content.IsCovered = true;

            //_loadingProcessDone = false;
            //Content = null;
            //Dispatcher.BeginInvoke(() => ShowLoadingScreen(false));
        }

        private void OnPivotSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e != null && e.RemovedItems != null && e.RemovedItems.Count > 0)
            {
                var lpi = e.RemovedItems[0] as LoadingPivotItem;
                if (lpi != null && lpi == this && _content != null)
                {
                    Cover();
                }
            }

            if (e != null && e.AddedItems != null && e.AddedItems.Count > 0)
            {
                var lpi = e.AddedItems[0] as LoadingPivotItem;
                // TODO: May need to track the finished sate.
                if (lpi != null && lpi == this && _content != null) //  && !_loadingProcessDone)
                {
                    _content.Preload();
                    // ??? _content.Load();
                    // TODO: Should this pre-set the Loading Cover or not?
                    // Dispatcher.BeginInvoke(() => ShowLoadingScreen(false));
                }
            }
        }

        private void OnLoadedPivotItem(object sender, PivotItemEventArgs e)
        {
            if (e.Item == this)
            {
                if (_content != null && _content.PauseResumeChild != null)
                {
                    //System.Diagnostics.Debug.WriteLine("Resuming child in this pivot item...");
                    _content.PauseResumeChild.Resume();
                }

                _content.Load();
            }
        }

        private void OnUnloadingPivotItem(object sender, PivotItemEventArgs e)
        {
            if (e.Item == this)
            {
                if (_content != null && _content.PauseResumeChild != null)
                {
                    //System.Diagnostics.Debug.WriteLine("Pausing child in this pivot item...");
                    
                    // potential bug: sometimes the child is not available for the main 'friends' pivot!


                    _content.PauseResumeChild.Pause();
                }
                else
                {
                }
            }
        }
    }
}
