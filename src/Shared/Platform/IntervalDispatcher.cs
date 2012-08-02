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
using System.Windows.Threading;

namespace JeffWilcox.Controls
{
    public static class IntervalDispatcher
    {
        private static Dispatcher _disp;

        public static void BeginInvoke(TimeSpan ts, Action action)
        {
            if (_disp == null)
            {
                _disp = System.Windows.Deployment.Current.Dispatcher;
            }
            if (!_disp.CheckAccess())
            {
                _disp.BeginInvoke(() => BeginInvoke(ts, action));
                return;
            }

            DispatcherTimer dt = new DispatcherTimer
            {
                Interval = ts
            };
            dt.Tick += (t, te) =>
                {
                    dt.Stop();
                    dt = null;
                    action();
                };
            dt.Start();
        }
    }
}
