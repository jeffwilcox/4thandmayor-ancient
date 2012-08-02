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

using Microsoft.Phone.Controls;

namespace JeffWilcox.Controls
{
    public static class FrameExtensions
    {
        /// <summary>
        /// Determines whether a <see cref="T:PhoneApplicationFrame"/> is oriented as portrait.
        /// </summary>
        /// <param name="phoneApplicationFrame">The <see cref="T:PhoneApplicationFrame"/>.</param>
        /// <returns><code>true</code> if the <see cref="T:PhoneApplicationFrame"/> is oriented as portrait; <code>false</code> otherwise.</returns>
        public static bool IsPortrait(this PhoneApplicationFrame phoneApplicationFrame)
        {
            PageOrientation portrait = PageOrientation.Portrait | PageOrientation.PortraitDown | PageOrientation.PortraitUp;
            return (portrait & phoneApplicationFrame.Orientation) == phoneApplicationFrame.Orientation;
        }
    }
}
