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
using AgFx;

namespace JeffWilcox.FourthAndMayor.Model
{
    using Controls;

    public abstract class FourSquareItemBase<T> : 
        ModelItemBase<T>, 
        ISendLoadComplete,
        ISupportLoadingRetry,
        ISendLoadStatus,
        INotifyOnCompletion // AgFx JWilcox temporary additive
        where T : LoadContext
    {
        protected const int StandardRefreshInterval = 60 * 5;
        protected const int FiveMinutes = StandardRefreshInterval;
        protected const int DebugIntervalMultiplier = 1;

        public FourSquareItemBase() { }

        public FourSquareItemBase(T context)
            : base(context)
        {
        }

        protected bool IgnoreRaisingPropertyChanges { get; set; }

        protected override void RaisePropertyChanged(string propertyName)
        {
            if (!IgnoreRaisingPropertyChanges)
            {
                base.RaisePropertyChanged(propertyName);
            }
        }

        private bool _finished;
        public bool IsLoadComplete 
        {
            get { return _finished; }
            set
            {
                _finished = value;
                RaisePropertyChanged("IsLoadComplete");

                if (value)
                {
                    var handler = LoadComplete;
                    if (handler != null)
                    {
                        handler(this, EventArgs.Empty);
                    }
                }

                LoadStatus = value ? LoadStatus.Loaded : LoadStatus.Loading; // not sure if the negative flip bit is good or not...
            }
        }

        public event EventHandler LoadComplete;

        private LoadStatus _loadStatus;

        public LoadStatus LoadStatus
        {
            get { return _loadStatus; }
            set
            {
                if (value != _loadStatus)
                {
                    _loadStatus = value;
                    RaisePropertyChanged("LoadStatus");

                    var handler = LoadStatusChanged;
                    if (handler != null)
                    {
                        handler(this, EventArgs.Empty);
                    }
#if DEBUG
                    else
                    {
                        // System.Diagnostics.Debug.WriteLine("4sq item base: Nobody's listening but the load status is now " + value + " (" + this.ToString() + ")");
                    }
#endif
                }
            }
        }

        public void RetryLoad()
        {
            Refresh();
        }

        public event EventHandler LoadStatusChanged;

        public void OnCompletion(Exception optionalException)
        {
            if (optionalException != null)
            {
                LoadStatus = Controls.LoadStatus.Failed;
            }
            // else: could consider setting loaded, too...
            else if (IsLoadComplete == false)
            {
                //LoadStatus = Controls.LoadStatus.Loaded;
                // GIC probably!
                System.Diagnostics.Debug.WriteLine("Warning: this object should have perhaps manually set its bit... unless it's a delaying object.");
                // hmm should it have been set?
            }
        }
    }
}
