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

﻿using System.Windows;

// AgiliTrain.PhoneyTools.SystemResources
// Shawn's Phoney Tools

namespace JeffWilcox.Controls
{
    /// <summary>
    /// Utility functions for determining the theme.
    /// </summary>
    public static class PhoneTheme
    {
        /// <summary>
        /// Gets a value indicating whether this instance is light theme.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is light theme; otherwise, <c>false</c>.
        /// </value>
        public static bool IsLightTheme
        {
            get
            {
                var lightVis = (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"];
                return lightVis == Visibility.Visible;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is dark theme.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is dark theme; otherwise, <c>false</c>.
        /// </value>
        public static bool IsDarkTheme
        {
            get
            {
                return !IsLightTheme;
            }
        }
    }
}

