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
using Microsoft.Phone.Controls;

// This is a simple, incomplete 'ken burns' style implementation as used for 
// the 4th & Mayor out-of-box, first-run experience. It actually does not
// do the traditional zoom with pan, but rather just a pan from side-to-side.

namespace JeffWilcox.Controls
{
    public class KenBurns : Control
    {
        private const string ImageName = "_image";

        private Image _image;

        private PhoneApplicationFrame _frame;

        #region public Uri Source
        /// <summary>
        /// Gets or sets the source Uri for the image.
        /// </summary>
        public Uri Source
        {
            get { return GetValue(SourceProperty) as Uri; }
            set { SetValue(SourceProperty, value); }
        }

        /// <summary>
        /// Identifies the Source dependency property.
        /// </summary>
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register(
                "Source",
                typeof(Uri),
                typeof(KenBurns),
                new PropertyMetadata(null, OnSourcePropertyChanged));

        /// <summary>
        /// SourceProperty property changed handler.
        /// </summary>
        /// <param name="d">KenBurns that changed its Source.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            KenBurns source = d as KenBurns;
            Uri value = e.NewValue as Uri;
            if (source._image != null)
            {
                source._image.Source = new BitmapImage { UriSource = value };
            }
        }
        #endregion public Uri Source

        public KenBurns()
        {
            SizeChanged += OnSizeChanged;
            DefaultStyleKey = typeof(KenBurns);
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Clip = new RectangleGeometry { Rect = new Rect(0, 0, ActualWidth, ActualHeight), };

            Restart();
        }

        public override void OnApplyTemplate()
        {
            if (_image != null)
            {
                _image.ImageOpened -= _image_ImageOpened;
            }

            base.OnApplyTemplate();

            _image = GetTemplateChild(ImageName) as Image;
            if (_image != null)
            {
                _image.ImageOpened += _image_ImageOpened;
            }

            if (_frame == null)
            {
                _frame = Application.Current.GetFrame();
                _frame.OrientationChanged += OnFrameOrientationChanged;
            }

            if (!_imageOpened && _image != null && Source != null)
            {
                _image.Source = new BitmapImage { UriSource = Source };
            }
        }

        void _image_ImageOpened(object sender, RoutedEventArgs e)
        {
            _imageOpened = true;
            Restart();
        }

        private bool _imageOpened;

        public void Restart()
        {
            bool wasValue = _imageOpened;
            _imageOpened = false;
            // clear out immediately
            UpdateVisualStates(false);
            _imageOpened = wasValue;

            if (_imageOpened)
            {
                if (_ta == null)
                {
                    TransformAnimator.EnsureAnimator(_image, ref _ta);
                }
                if (_ta != null)
                {
                    _ta.Forever();

                    var frameWidth = _frame.ActualWidth;
                    var imageWidth = _image.ActualWidth;

                    var finalOffset = imageWidth - frameWidth;
                    if (finalOffset < 0)
                    {
                        // Threw an IOE in the past, someone hit this one
                        // according to some crash reports.
                        finalOffset = 0;
                    }

                    _ta.GoTo(-finalOffset, new Duration(TimeSpan.FromSeconds(50)));
                }

            }

            UpdateVisualStates(true);
        }

        private TransformAnimator _ta;

        void OnFrameOrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            Restart();
        }

        private void UpdateVisualStates(bool useTransitions)
        {
            VisualStateManager.GoToState(this, _imageOpened ? "Loaded" : "Normal", useTransitions);
        }
    }
}
