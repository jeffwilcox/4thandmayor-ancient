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
//#define DEBUG_LIVE_TILES
#endif

// My Live Tile control as used in 4th & Mayor. Pretty incomplete.

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Phone.Controls;

namespace JeffWilcox.Controls
{
    public class LiveTile : HyperlinkButton
    {
        private static Random Random = new Random();

        private Pivot _pivot;

        private PivotItem _pivotItem;

        #region public object Header
        /// <summary>
        /// Gets or sets the tile header.
        /// </summary>
        public object Header
        {
            get { return GetValue(HeaderProperty) as object; }
            set { SetValue(HeaderProperty, value); }
        }

        /// <summary>
        /// Identifies the Header dependency property.
        /// </summary>
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(
                "Header",
                typeof(object),
                typeof(LiveTile),
                new PropertyMetadata(null));
        #endregion public object Header

        #region public Uri ImageSource
        /// <summary>
        /// Gets or sets the image URI.
        /// </summary>
        public Uri ImageSource
        {
            get { return GetValue(ImageSourceProperty) as Uri; }
            set { SetValue(ImageSourceProperty, value); }
        }

        /// <summary>
        /// Identifies the ImageSource dependency property.
        /// </summary>
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register(
                "ImageSource",
                typeof(Uri),
                typeof(LiveTile),
                new PropertyMetadata(null, OnImageSourcePropertyChanged));

        /// <summary>
        /// ImageSourceProperty property changed handler.
        /// </summary>
        /// <param name="d">LiveTile that changed its ImageSource.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnImageSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            LiveTile source = d as LiveTile;
            Uri value = e.NewValue as Uri;
            source.OnImageSourceChanged(value);
        }
        #endregion public Uri ImageSource

        #region public Style HeaderTextStyle
        /// <summary>
        /// Gets or sets the style to use for the header text.
        /// </summary>
        public Style HeaderTextStyle
        {
            get { return GetValue(HeaderTextStyleProperty) as Style; }
            set { SetValue(HeaderTextStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the HeaderTextStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty HeaderTextStyleProperty =
            DependencyProperty.Register(
                "HeaderTextStyle",
                typeof(Style),
                typeof(LiveTile),
                new PropertyMetadata(null));
        #endregion public Style HeaderTextStyle

        #region public double TileSize
        /// <summary>
        /// 
        /// </summary>
        public double TileSize
        {
            get { return (double)GetValue(TileSizeProperty); }
            set { SetValue(TileSizeProperty, value); }
        }

        /// <summary>
        /// Identifies the TileSize dependency property.
        /// </summary>
        public static readonly DependencyProperty TileSizeProperty =
            DependencyProperty.Register(
                "TileSize",
                typeof(double),
                typeof(LiveTile),
                new PropertyMetadata(0.0, OnTileSizePropertyChanged));

        /// <summary>
        /// TileSizeProperty property changed handler.
        /// </summary>
        /// <param name="d">LiveTile that changed its TileSize.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnTileSizePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            LiveTile source = d as LiveTile;
            double value = (double)e.NewValue;

            source.DoubleTileSize = value * 2;
            source.NegativeTileSize = -value;
            source.HalfTileSize = value / 2;
            source.NegativeHalfTileSize = -(value / 2);
        }
        #endregion public double TileSize

        #region public double NegativeTileSize
        /// <summary>
        /// 
        /// </summary>
        public double NegativeTileSize
        {
            get { return (double)GetValue(NegativeTileSizeProperty); }
            set { SetValue(NegativeTileSizeProperty, value); }
        }

        /// <summary>
        /// Identifies the NegativeTileSize dependency property.
        /// </summary>
        public static readonly DependencyProperty NegativeTileSizeProperty =
            DependencyProperty.Register(
                "NegativeTileSize",
                typeof(double),
                typeof(LiveTile),
                new PropertyMetadata(0.0));
        #endregion public double NegativeTileSize

        #region public double HalfTileSize
        /// <summary>
        /// 
        /// </summary>
        public double HalfTileSize
        {
            get { return (double)GetValue(HalfTileSizeProperty); }
            set { SetValue(HalfTileSizeProperty, value); }
        }

        /// <summary>
        /// Identifies the HalfTileSize dependency property.
        /// </summary>
        public static readonly DependencyProperty HalfTileSizeProperty =
            DependencyProperty.Register(
                "HalfTileSize",
                typeof(double),
                typeof(LiveTile),
                new PropertyMetadata(0.0));
        #endregion public double HalfTileSize

        #region public double NegativeHalfTileSize
        /// <summary>
        /// 
        /// </summary>
        public double NegativeHalfTileSize
        {
            get { return (double)GetValue(NegativeHalfTileSizeProperty); }
            set { SetValue(NegativeHalfTileSizeProperty, value); }
        }

        /// <summary>
        /// Identifies the NegativeHalfTileSize dependency property.
        /// </summary>
        public static readonly DependencyProperty NegativeHalfTileSizeProperty =
            DependencyProperty.Register(
                "NegativeHalfTileSize",
                typeof(double),
                typeof(LiveTile),
                new PropertyMetadata(0.0));
        #endregion public double NegativeHalfTileSize

        #region public double DoubleTileSize
        /// <summary>
        /// 
        /// </summary>
        public double DoubleTileSize
        {
            get { return (double)GetValue(DoubleTileSizeProperty); }
            set { SetValue(DoubleTileSizeProperty, value); }
        }

        /// <summary>
        /// Identifies the DoubleTileSize dependency property.
        /// </summary>
        public static readonly DependencyProperty DoubleTileSizeProperty =
            DependencyProperty.Register(
                "DoubleTileSize",
                typeof(double),
                typeof(LiveTile),
                new PropertyMetadata(0.0));
        #endregion public double DoubleTileSize

        private enum TileState
        {
            Text,
            Partial,
            Image,
        }

        public double EvaluationInterval;

        private bool _hasImageLoaded;

        private TileState _currentState;

        private Image _image;

        private Grid _grid;

        private TransformYAnimator _ya;

        private DispatcherTimer _dt;

        public LiveTile() : base()
        {
            DefaultStyleKey = typeof(LiveTile);

            // hard-coded for now.
            EvaluationInterval = 1.0;

            Loaded += OnLoaded;

            SizeChanged += OnSizeChanged;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            StopLiveTile();

            Loaded += OnLoaded;
        }

        public void Pause()
        {
            if (_dt != null)
            {
#if DEBUG_LIVE_TILES
                Debug.WriteLine("__LIVE_TILE pause");
#endif
                _dt.Stop();
            }
        }

        public void Resume()
        {
            if (_dt != null)
            {
#if DEBUG_LIVE_TILES
                Debug.WriteLine("__LIVE_TILE resume");
#endif
                _dt.Start();
            }
        }

        private void StopLiveTile()
        {
            if (_dt != null)
            {
                _dt.Stop();
                _dt.Tick -= OnTick;
                _dt = null;
#if DEBUG_LIVE_TILES
                Debug.WriteLine("__LIVE_TILE stopped: " + ImageSource);
#endif
            }
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (_dt != null)
            {
                _dt.Stop();
                OrganicInterval();
            }
            CycleState();
            if (_dt != null)
            {
                _dt.Start();
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Unloaded += OnUnloaded;

            if (_hasImageLoaded)
            {
                BringToLife();
            }
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Clip = new RectangleGeometry { Rect = new Rect(0, 0, ActualWidth, ActualHeight) };
        }

        public override void OnApplyTemplate()
        {
            if (_ya != null)
            {
                _ya = null;
            }

            if (_pivotItem == null)
            {
                DependencyObject d = Parent;
                while (d != null && !(d is PivotItem))
                {
                    d = VisualTreeHelper.GetParent(d);
                }
                _pivotItem = d as PivotItem;
            }

            if (_pivotItem != null && _pivot == null)
            {
                DependencyObject d = _pivotItem;
                while (d != null && !(d is Pivot))
                {
                    d = VisualTreeHelper.GetParent(d);
                }
                _pivot = d as Pivot;
                if (_pivot != null)
                {
                    _pivot.UnloadingPivotItem += OnUnloadingPivotItem;
                    //_pivot.SelectionChanged += OnPivotSelectionChanged;
                }
            }

            base.OnApplyTemplate();

            _image = GetTemplateChild("_image") as Image;
            _grid = GetTemplateChild("_grid") as Grid;

            TransformYAnimator.EnsureAnimator(_grid, ref _ya);
            Debug.Assert(_ya != null);

            if (_image != null && ImageSource != null)
            {
                UpdateImageSource();
            }

            UpdatePosition(false);
        }

        private void OnUnloadingPivotItem(object sender, PivotItemEventArgs e)
        {
            if (e.Item == _pivotItem)
            {
                _pivot.LoadedPivotItem += OnLoadedPivotItem;

                StopLiveTile();
                if (_currentState != TileState.Text)
                {
                    _currentState = TileState.Text;
                    UpdatePosition(true);
                }

                _pivot.UnloadingPivotItem -= OnUnloadingPivotItem;
            }
        }

        private void OnLoadedPivotItem(object sender, PivotItemEventArgs e)
        {
            if (e.Item == _pivotItem)
            {
                _pivot.LoadedPivotItem -= OnLoadedPivotItem;
                _pivot.UnloadingPivotItem += OnUnloadingPivotItem;

                BringToLife();
            }
        }

        private void UpdatePosition(bool useTransitions)
        {
            if (_ya != null)
            {
                IEasingFunction ease = new CircleEase(); // ExponentialEase { Exponent = 1 };

                Duration d = new Duration(TimeSpan.Zero);
                if (useTransitions)
                {
                    double random = Random.NextDouble();
                    double min = 1.0;
                    double extra = random * 2.5;
                    d = new Duration(TimeSpan.FromSeconds(min + extra));
                }

                var loc = 0.0;

                switch (_currentState)
                {
                    case TileState.Text:
                        loc = 0.0;
                        break;

                    case TileState.Partial:
                        loc = NegativeHalfTileSize;
                        break;

                    case TileState.Image:
                        loc = NegativeTileSize;
                        break;
                }

                _ya.GoTo(loc, d, ease);
            }
        }

        private void UpdateImageSource()
        {
            _hasImageLoaded = false;

            if (_image != null)
            {
                var b = new BitmapImage(ImageSource);
                b.ImageOpened += OnImageOpened;
                _image.Source = b;
            }
        }

        private void OrganicInterval()
        {
            if (_dt != null)
            {
                double d = Random.NextDouble();
                double delay = 8.5 * d; // 8.5 seconds max-ish

                _dt.Interval = TimeSpan.FromSeconds(delay + EvaluationInterval);
            }
        }

        private void OnImageOpened(object sender, RoutedEventArgs e)
        {
            BitmapImage b = (BitmapImage)sender;
            b.ImageOpened -= OnImageOpened;

            _hasImageLoaded = true;

            // Prepare the fun!
            BringToLife();
        }

        private void BringToLife()
        {
            if (_dt == null && _hasImageLoaded)
            {
                _dt = new DispatcherTimer();
                OrganicInterval();
                _dt.Tick += OnTick;

                double d = Random.NextDouble();
                double delayInSeconds = 3 * d; // 0 to 9 seconds.
                delayInSeconds *= 2.1; // multiply/space-out

#if DEBUG_LIVE_TILES
                Debug.WriteLine("__LIVE_TILE starting: " + ImageSource);
#endif

                IntervalDispatcher.BeginInvoke(
                    TimeSpan.FromSeconds(delayInSeconds),
                    () => {
                        var dt = _dt;
                        if (dt != null)
                        {
                            dt.Start();
                        }
                    });
            }
        }

        private void CycleState()
        {
            var c = _currentState;

            // The image must have been loaded first.
            if (!_hasImageLoaded)
            {
                return;
            }

            switch (c)
            {
                case TileState.Image:
                    _currentState = TileState.Text;
                    break;

                case TileState.Partial:
                    _currentState = TileState.Image;
                    break;

                case TileState.Text:
                    _currentState = TileState.Partial;
                    break;
            }

            UpdatePosition(true);
        }

        private void OnImageSourceChanged(Uri newValue)
        {
            UpdateImageSource();
        }
    }
}