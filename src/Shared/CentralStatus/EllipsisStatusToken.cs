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
using System.Windows.Threading;
using AgFx;

namespace JeffWilcox.Controls
{
    public class EllipsisStatusToken : StatusToken
    {
        private DispatcherTimer _timer;
        private int _step;
        private string _originalMessage;

        public EllipsisStatusToken(string message) : base()
        {
            _originalMessage = message;

            _step = -1;

            IncrementStep();

            PriorityQueue.AddUiWorkItem(() =>
                {
                    _timer = new DispatcherTimer();
                    _timer.Interval = TimeSpan.FromSeconds(0.65);
                    _timer.Tick += OnTick;
                    _timer.Start();
                });
        }

        private void OnTick(object sender, EventArgs e)
        {
            IncrementStep();
        }

        private void IncrementStep()
        {
            ++_step;

            if (_step > 3)
            {
                _step = 0;
            }

            Message = _originalMessage
                + (_step > 0 ? "." : string.Empty)
                + (_step > 1 ? "." : string.Empty)
                + (_step > 2 ? "." : string.Empty);
        }

        public override void Complete()
        {
            if (_timer != null)
            {
                var tm = _timer;
                PriorityQueue.AddUiWorkItem(() =>
                    {
                        tm.Stop();
                        tm.Tick -= OnTick;
                        tm = null;
                    });
                _timer = null;
            }

            base.Complete();
        }
    }
}
