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
using System.Windows.Media.Imaging;

namespace JeffWilcox.Controls
{
    // TODO: CONSIDER a way to smoothly replace the source as well.

    // Binding changes other than from null don't work. Blast I'm lazy!

    /// <summary>
    /// Provides an experience where the image is downloaded when needed,
    /// slowly fading in to provide an interesting visual user experience.
    /// </summary>
    public class SmoothImage : Control
    {
        public SmoothImage() : base()
        {
            AreAnimationsEnabled = true;
            DefaultStyleKey = typeof (SmoothImage);
        }

        // quick prop only 
        public bool AreAnimationsEnabled { get; set; }

        #region public Stretch Stretch
        /// <summary>
        /// Gets or sets the stretch.
        /// </summary>
        public Stretch Stretch
        {
            get { return (Stretch)GetValue(StretchProperty); }
            set { SetValue(StretchProperty, value); }
        }

        /// <summary>
        /// Identifies the Stretch dependency property.
        /// </summary>
        public static readonly DependencyProperty StretchProperty =
            DependencyProperty.Register(
                "Stretch",
                typeof(Stretch),
                typeof(SmoothImage),
                new PropertyMetadata(Stretch.None, OnStretchPropertyChanged));

        /// <summary>
        /// StretchProperty property changed handler.
        /// </summary>
        /// <param name="d">SmoothImage that changed its Stretch.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnStretchPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SmoothImage source = d as SmoothImage;
            Stretch value = (Stretch)e.NewValue;
        }
        #endregion public Stretch Stretch

        #region public BitmapImage ActualImageSource
        /// <summary>
        /// 
        /// </summary>
        public BitmapImage ActualImageSource
        {
            get { return GetValue(ActualImageSourceProperty) as BitmapImage; }
            set { SetValue(ActualImageSourceProperty, value); }
        }

        /// <summary>
        /// Identifies the ActualImageSource dependency property.
        /// </summary>
        public static readonly DependencyProperty ActualImageSourceProperty =
            DependencyProperty.Register(
                "ActualImageSource",
                typeof(BitmapImage),
                typeof(SmoothImage),
                new PropertyMetadata(null));
        #endregion public BitmapImage ActualImageSource

        #region public Uri ImageSource
        /// <summary>
        /// Gets or sets the image URI. This is not an image source type though.
        /// </summary>
        public Uri ImageSource
        {
            get { return (Uri)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        /// <summary>
        /// Identifies the ImageSource dependency property.
        /// </summary>
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register(
                "ImageSource",
                typeof(Uri),
                typeof(SmoothImage),
                new PropertyMetadata(null, OnImageSourcePropertyChanged));

        /// <summary>
        /// ImageSourceProperty property changed handler.
        /// </summary>
        /// <param name="d">SmoothImage that changed its ImageSource.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnImageSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SmoothImage source = d as SmoothImage;
            Uri value = (Uri)e.NewValue;
            source.OnImageSourceChanged(value);
        }

        private void OnImageSourceChanged(Uri value)
        {
            var b = new BitmapImage(value);
            b.ImageOpened += OnImageOpened;
            _imageIsVisible = false;
            UpdateVisualStates(false);
            ActualImageSource = b;
        }

        // For programmatic use.
        public void SetBitmapImage(BitmapImage b)
        {
            if (b != null)
            {
                b.ImageOpened += OnImageOpened;
                _imageIsVisible = false;
                UpdateVisualStates(false);
            }
            ActualImageSource = b;
        }

        private bool _imageIsVisible;

        private void OnImageOpened(object sender, RoutedEventArgs e)
        {
            BitmapImage b = (BitmapImage)sender;
            b.ImageOpened -= OnImageOpened;

            _imageIsVisible = true;
            UpdateVisualStates(true);

            // CONSIDER: Should this happen after the VSM state change?
            OnFinalImageAvailable(EventArgs.Empty);
        }
        #endregion public Uri ImageSource

        public event EventHandler FinalImageAvailable;

        protected virtual void OnFinalImageAvailable(EventArgs e)
        {
            var handler = FinalImageAvailable;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void UpdateVisualStates(bool useTransitions)
        {
            if (!AreAnimationsEnabled)
            {
                useTransitions = false;
            }
            VisualStateManager.GoToState(this, _imageIsVisible ? "Loaded" : "Normal", useTransitions);
        }
    }
}
