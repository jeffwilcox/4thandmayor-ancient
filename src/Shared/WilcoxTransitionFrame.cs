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
using System.Windows.Controls;

namespace JeffWilcox.Controls
{
    using Microsoft.Phone.Controls;

    public class WilcoxTransitionFrame : TransitionFrame // REMOVING FOR NOW: HybridOrientationChangesFrame
    {
        private const string OverlayTemplatePartName = "OverlayGrid";
        private const string AnalyticsImageTemplatePartName = "AnalyticsImage";

        public Grid OverlayGrid { get; private set; }
        public Image AnalyticsImage { get; private set; }

        public event EventHandler HaveOverlayGrid;

        public WilcoxTransitionFrame() : base()
        {
            DefaultStyleKey = typeof (WilcoxTransitionFrame);
        }

        public override void OnApplyTemplate()
        {
            OverlayGrid = GetTemplateChild(OverlayTemplatePartName) as Grid;
            AnalyticsImage = GetTemplateChild(AnalyticsImageTemplatePartName) as Image;
            if (OverlayGrid != null && HaveOverlayGrid != null)
            {
                // This is a simple way to message about this.
                // And by simple... I mean... HACKY!
                Dispatcher.BeginInvoke(() => HaveOverlayGrid(this, EventArgs.Empty));
            }

            base.OnApplyTemplate();
        }
    }
}
