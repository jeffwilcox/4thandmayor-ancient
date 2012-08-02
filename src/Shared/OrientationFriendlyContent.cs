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

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Phone.Controls;

namespace JeffWilcox.Controls
{
    [TemplatePart(Name = PrimaryElementPartName, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = HostingGridPartName, Type = typeof(Grid))]
    public class OrientationFriendlyContent : Control
    {
        private const string PrimaryElementPartName = "primary";

        private const string HostingGridPartName = "grid";

        private Grid _hostingGrid;

        private FrameworkElement _primary;

        #region public object TopLeftContent
        /// <summary>
        /// Gets or sets the top or left content.
        /// </summary>
        public object TopLeftContent
        {
            get { return GetValue(TopLeftContentProperty) as object; }
            set { SetValue(TopLeftContentProperty, value); }
        }

        /// <summary>
        /// Identifies the TopLeftContent dependency property.
        /// </summary>
        public static readonly DependencyProperty TopLeftContentProperty =
            DependencyProperty.Register(
                "TopLeftContent",
                typeof(object),
                typeof(OrientationFriendlyContent),
                new PropertyMetadata(null));
        #endregion public object TopLeftContent

        #region public object BottomRightContent
        /// <summary>
        /// Gets or sets the bottom or right content.
        /// </summary>
        public object BottomRightContent
        {
            get { return GetValue(BottomRightContentProperty) as object; }
            set { SetValue(BottomRightContentProperty, value); }
        }

        /// <summary>
        /// Identifies the BottomRightContent dependency property.
        /// </summary>
        public static readonly DependencyProperty BottomRightContentProperty =
            DependencyProperty.Register(
                "BottomRightContent",
                typeof(object),
                typeof(OrientationFriendlyContent),
                new PropertyMetadata(null));
        #endregion public object BottomRightContent

        public OrientationFriendlyContent() : base()
        {
            DefaultStyleKey = typeof (OrientationFriendlyContent);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _page = null;
            UIElement element = this;
            while (element != null && _page == null)
            {
                element = VisualTreeHelper.GetParent(element) as UIElement;
                _page = element as PhoneApplicationPage;
            }
            if (_page == null)
            {
                throw new InvalidOperationException("The page was null.");
            }

            _page.OrientationChanged += OnOrientationChanged;
            Unloaded += OnControlUnloaded;

            _primary = GetTemplateChild(PrimaryElementPartName) as FrameworkElement;
            _hostingGrid = GetTemplateChild(HostingGridPartName) as Grid;

            React(_page.Orientation);
        }

        private PhoneApplicationPage _page;

        private void OnControlUnloaded(object sender, RoutedEventArgs e)
        {
            Unloaded -= OnControlUnloaded;
            _page.OrientationChanged -= OnOrientationChanged;
        }

        private void OnOrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            PageOrientation po = e.Orientation;
            React(po);
        }

        private void React(PageOrientation po)
        {
            switch (po)
            {
                case PageOrientation.Portrait:
                case PageOrientation.PortraitDown:
                case PageOrientation.PortraitUp:
                    if (_hostingGrid != null)
                    {
                        (_hostingGrid.RowDefinitions[0]).Height = GridLength.Auto;
                        (_hostingGrid.RowDefinitions[1]).ClearValue(RowDefinition.HeightProperty);
                        (_hostingGrid.ColumnDefinitions[0]).Width = GridLength.Auto;
                        (_hostingGrid.ColumnDefinitions[1]).ClearValue(ColumnDefinition.WidthProperty);
                        Grid.SetRowSpan(_primary, 1);
                        Grid.SetColumnSpan(_primary, 2);
                        Grid.SetRow(_primary, 1);
                        Grid.SetColumn(_primary, 0);
                    }
                    break;

                default:
                    if (_hostingGrid != null)
                    {
                        (_hostingGrid.RowDefinitions[0]).SetValue(RowDefinition.HeightProperty, new GridLength(1, GridUnitType.Star));
                        (_hostingGrid.RowDefinitions[1]).Height = new GridLength(0.0, GridUnitType.Pixel);
                        (_hostingGrid.ColumnDefinitions[0]).Width = GridLength.Auto;
                        (_hostingGrid.ColumnDefinitions[1]).SetValue(ColumnDefinition.WidthProperty, new GridLength(1, GridUnitType.Star));
                        Grid.SetRow(_primary, 0);
                        Grid.SetColumn(_primary, 1);
                        Grid.SetRowSpan(_primary, 2);
                        Grid.SetColumnSpan(_primary, 1);

                        // KNOWN BUG: For some reason in the venue page there is a ton of spacing below.
                    }
                    break;
            }
        }
    }
}
