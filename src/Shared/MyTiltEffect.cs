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

namespace Microsoft.Phone.Controls
{
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;

    /// <summary>
    /// This code provides attached properties for adding a 'tilt' effect to all controls within a container.
    /// </summary>
    public partial class TiltEffect
    {
        /// <summary>
        /// Whether the tilt effect is enabled on a container (and all its children)
        /// </summary>
        public static readonly DependencyProperty TiltMeProperty = DependencyProperty.RegisterAttached(
          "TiltMe",
          typeof(bool),
          typeof(TiltEffect),
          new PropertyMetadata(OnTiltMeChanged)
          );

        public static bool GetTiltMe(DependencyObject source) { return (bool)source.GetValue(TiltMeProperty); }

        public static void SetTiltMe(DependencyObject source, bool value) { source.SetValue(TiltMeProperty, value); }

        static void OnTiltMeChanged(DependencyObject target, DependencyPropertyChangedEventArgs args)
        {
            if (target is FrameworkElement)
            {
                // Add / remove the event handler if necessary
                if ((bool)args.NewValue == true)
                {
                    ((FrameworkElement)target).ManipulationStarted += TiltMe_ManipulationStarted;

                    // Suppress the standard one here.
                    SetSuppressTilt(target, true);
                }
                else
                {
                    ((FrameworkElement)target).ManipulationStarted -= TiltMe_ManipulationStarted;
                }
            }
        }

        static void TiltMe_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            TryStartDirectTiltEffect(sender as FrameworkElement, e);
        }
        static void TryStartDirectTiltEffect(FrameworkElement source, ManipulationStartedEventArgs e)
        {
            // impl. root.
            FrameworkElement element = VisualTreeHelper.GetChild(source, 0) as FrameworkElement;
            FrameworkElement container = e.ManipulationContainer as FrameworkElement;

            if (element == null || container == null)
                return;

            // Touch point relative to the element being tilted
            Point tiltTouchPoint = container.TransformToVisual(element).Transform(e.ManipulationOrigin);

            // Center of the element being tilted
            Point elementCenter = new Point(element.ActualWidth / 2, element.ActualHeight / 2);

            // Camera adjustment
            Point centerToCenterDelta = GetCenterToCenterDelta(element, source);

            BeginTiltEffect(element, tiltTouchPoint, elementCenter, centerToCenterDelta);
        }
    }
}