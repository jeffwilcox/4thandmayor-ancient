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

using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace JeffWilcox.Controls
{
    public static class MoreVisualTreeExtensions
    {
        internal static T FindFirstChildOfType<T>(DependencyObject root) where T : class
        {
            // Enqueue root node
            var queue = new Queue<DependencyObject>();
            queue.Enqueue(root);
            while (0 < queue.Count)
            {
                // Dequeue next node and check its children
                var current = queue.Dequeue();
                for (var i = VisualTreeHelper.GetChildrenCount(current) - 1; 0 <= i; i--)
                {
                    var child = VisualTreeHelper.GetChild(current, i);
                    var typedChild = child as T;
                    if (null != typedChild)
                    {
                        return typedChild;
                    }
                    // Enqueue child
                    queue.Enqueue(child);
                }
            }
            // No children match
            return null;
        }
    }
}
