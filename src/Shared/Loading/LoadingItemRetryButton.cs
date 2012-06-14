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
using System.Linq;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace JeffWilcox.Controls
{
    /// <summary>
    /// A button whose Click event automatically walks up the tree to the first
    /// LoadingContentControl instance and notifies any LoadingRetryInstance to
    /// make another attempt.
    /// </summary>
    public class LoadingItemRetryButton : Button
    {
        public LoadingItemRetryButton()
            : base()
        {
        }

        protected override void OnClick()
        {
            var ancestor = VisualTreeExtensions
                .GetVisualAncestors(Parent)
                .OfType<LoadingContentControl>()
                .FirstOrDefault();

            if (ancestor != null)
            {
                var lri = ancestor.LoadingRetryInstance;
                if (lri == null)
                {
                    throw new InvalidOperationException("No associated data context that implements ILoadingRetryInstance.");
                }

                lri.RetryLoad();
            }
            else
            {
                throw new InvalidOperationException("No visual parent is of type LoadingContentControl.");
            }

            base.OnClick();
        }
    }
}
