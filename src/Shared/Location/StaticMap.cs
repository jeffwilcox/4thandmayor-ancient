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
//#define DEBUG_STATIC_BING_MAPS
#endif

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using JeffWilcox.FourthAndMayor;

namespace JeffWilcox.Controls
{
    public class StaticMap : Control
    {
        #region public LocationPair PointOfInterest
        /// <summary>
        /// Gets or sets the point of interest.
        /// </summary>
        public LocationPair PointOfInterest
        {
            get { return GetValue(PointOfInterestProperty) as LocationPair; }
            set { SetValue(PointOfInterestProperty, value); }
        }

        /// <summary>
        /// Identifies the PointOfInterest dependency property.
        /// </summary>
        public static readonly DependencyProperty PointOfInterestProperty =
            DependencyProperty.Register(
                "PointOfInterest",
                typeof(LocationPair),
                typeof(StaticMap),
                new PropertyMetadata(null, OnPointOfInterestPropertyChanged));

        /// <summary>
        /// PointOfInterestProperty property changed handler.
        /// </summary>
        /// <param name="d">StaticMap that changed its PointOfInterest.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnPointOfInterestPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            StaticMap source = d as StaticMap;
            source.UpdateMap();
        }
        #endregion public LocationPair PointOfInterest

        #region public LocationPair CenterPointIfAvailable
        /// <summary>
        /// Gets or sets the optional center point. This will actually be the
        /// point of interest if not set.
        /// </summary>
        public LocationPair CenterPointIfAvailable
        {
            get { return GetValue(CenterPointIfAvailableProperty) as LocationPair; }
            set { SetValue(CenterPointIfAvailableProperty, value); }
        }

        /// <summary>
        /// Identifies the CenterPointIfAvailable dependency property.
        /// </summary>
        public static readonly DependencyProperty CenterPointIfAvailableProperty =
            DependencyProperty.Register(
                "CenterPointIfAvailable",
                typeof(LocationPair),
                typeof(StaticMap),
                new PropertyMetadata(null, OnCenterPointIfAvailablePropertyChanged));

        /// <summary>
        /// CenterPointIfAvailableProperty property changed handler.
        /// </summary>
        /// <param name="d">StaticMap that changed its CenterPointIfAvailable.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnCenterPointIfAvailablePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            StaticMap source = d as StaticMap;
            source.UpdateMap();
        }
        #endregion public LocationPair CenterPointIfAvailable

        #region public int ZoomLevel
        /// <summary>
        /// Gets or sets the zoom level of 1 to 22 or so.
        /// </summary>
        public int ZoomLevel
        {
            get { return (int)GetValue(ZoomLevelProperty); }
            set { SetValue(ZoomLevelProperty, value); }
        }

        /// <summary>
        /// Identifies the ZoomLevel dependency property.
        /// </summary>
        public static readonly DependencyProperty ZoomLevelProperty =
            DependencyProperty.Register(
                "ZoomLevel",
                typeof(int),
                typeof(StaticMap),
                new PropertyMetadata(15));
        #endregion public int ZoomLevel

        private const string PushPinFormat = "&pp={5},{6};{7}"; // lat, long, style

        private const string StaticMapsUrlFormat =
            "http://dev.virtualearth.net/REST/v1/Imagery/Map/Road/" + 
            "{0},{1}" + // lat,long
            "/{2}" + // zoomLevel // int from 1 to 22, 15 seems normal-ish
            "?mapSize=" +
            "{3},{4}" + // width, height
            PushPinFormat + // lat,long of pushpin
            "&mapVersion=v1&key={8}"; // dev key

        public StaticMap()
        {
            DefaultStyleKey = typeof(StaticMap);
            SizeChanged += OnSizeChanged;

            SmoothlyFadeIn = true;
        }

        private double _width;
        private double _height;

        public bool SmoothlyFadeIn { get; set; }
        
        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Height > 0.0 && e.NewSize.Width > 0.0)
            {
                _width = e.NewSize.Width;
                _height = e.NewSize.Height;
                UpdateMap();
            }
        }

        public bool HasMapLoaded { get; private set; }

        //private Image _image;
        private SmoothImage _image;

        public event EventHandler MapAvailable;

        public override void OnApplyTemplate()
        {
            if (_image != null)
            {
                _image.FinalImageAvailable -= OnFinalImageAvailable;
            }

            base.OnApplyTemplate();

            _image = MoreVisualTreeExtensions.FindFirstChildOfType<SmoothImage>(this);
            if (_image != null)
            {
                _image.FinalImageAvailable += OnFinalImageAvailable;
            }

            UpdateMap();
        }

        private void OnFinalImageAvailable(object sender, EventArgs e)
        {
            HasMapLoaded = true;

            var handler = MapAvailable;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private List<Point> _requestedImages = new List<Point>();

        private void UpdateMap()
        {
            if (_height > 1 && _width > 1 && _image != null)
            {
                int width = (int)Math.Ceiling(_width);
                int height = (int)Math.Ceiling(_height);

                Point p = new Point(width, height);
                if (_requestedImages.Contains(p))
                {
                    return;
                }
                _requestedImages.Add(p);

                var format = StaticMapsUrlFormat;

                // This code is buggy on purpose, having been updated and 
                // changed so many times. Ugh. Should rewrite.

                // This now supports not having a pushpin.
                LocationPair lp = PointOfInterest;
                LocationPair cp = lp;
                if (lp == null)
                {
                    // return;
                    format = format.Replace(PushPinFormat, string.Empty);
                    cp = CenterPointIfAvailable;
                }

                //string format = StaticMapsUrlFormat;
                //LocationPair cp = CenterPointIfAvailable;
                //if (cp == null)
                {
                    // swap
                    //cp = lp;
                    //format = format.Replace(PushPinFormat, string.Empty);
                    //lp = null;
                }

                IAppInfo iai = Application.Current as IAppInfo;
                string key = "";
                if (iai != null)
                {
                    key = iai.BKey;
                }

                if (cp == null)
                {
                    return;
                }

                Uri uri = new Uri(string.Format(
                    CultureInfo.InvariantCulture,
                    format,
                    cp.Latitude,
                    cp.Longitude,
                    ZoomLevel,
                    width,
                    height,
                    cp.Latitude,
                    cp.Longitude,
                    36,                                                          // pin styles at http://msdn.microsoft.com/en-us/library/ff701719.aspx
                    key
                    ), UriKind.Absolute);

//#if DEBUG_STATIC_BING_MAPS
                System.Diagnostics.Debug.WriteLine("Getting a Bing map " + uri.ToString());
//#endif

                if (!SmoothlyFadeIn)
                {
                    _image.AreAnimationsEnabled = false;
                }

                _image.ImageSource = uri;
            }
            else
            {
#if DEBUG_STATIC_BING_MAPS
                Debug.WriteLine("Bing Maps: The image {2}, height {0}, and width {1} are not appropriate to display a map currently.", _height, _width, _image == null ? "null image" : "image exists in tree");
#endif
            }
        }
    }
}
