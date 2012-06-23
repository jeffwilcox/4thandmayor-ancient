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
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Navigation;
using AgFx;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace JeffWilcox.Controls
{
    public class CentralStatusManager : NotifyPropertyChangedBase
    {
        //private Mango.Interfaces.IProgressIndicator _mangoIndicator;
        private ProgressIndicator _mangoIndicator;

        private CentralStatusManager()
        {
            _tokenStack = new List<StatusToken>();

            DefaultMessageDisplayTime = DefaultMessageDisplayTimeValue;

            // For legacy loading scenarios.
            _singleLegacyStatusToken = new StatusToken();
            Push(_singleLegacyStatusToken);
        }

        private bool _loading;
        private string _message;
        private StatusToken _activeMessageToken;

        internal void DeleteToken(StatusToken token)
        {
            token.IsLoadingChanged -= OnTokenIsLoadingChanged;
            token.MessageChanged -= OnTokenMessageChanged;

            lock (_tokenStack)
            {
                var res = _tokenStack.Remove(token);
#if DEBUG
                //DebugStack("Removing " + token.GetType().ToString() + " " + res.ToString());
#endif
            }

            Recalculate();
        }

        private void Recalculate()
        {
            /*if (_mangoIndicator == null)
            {
                return;
            }*/

            bool isLoading = false;
            string message = null;

            lock (_tokenStack)
            {
#if DEBUG
                //DebugStack("Recalculating...");
#endif

                for (int i = 0; i < _tokenStack.Count; i++)
                {
                    var token = _tokenStack[i];
                    if (token != null)
                    {
                        if (token.IsLoading)
                        {
                            isLoading = true;
                        }
                        if (token.Message != null)
                        {
                            message = token.Message;
                            _activeMessageToken = token;
                        }
                    }
                }

                _message = message;
                _loading = isLoading;

                if (_message == null)
                {
                    _activeMessageToken = null;
                }
            }

            NotifyValueChanged();
        }

        public StatusToken ActiveMessageToken
        {
            get { return _activeMessageToken; }
        }

        private static readonly TimeSpan DefaultMessageDisplayTimeValue = TimeSpan.FromSeconds(0.5);
        public TimeSpan DefaultMessageDisplayTime { get; set; }

        private List<StatusToken> _tokenStack;

        private StatusToken _singleLegacyStatusToken;

        public StatusToken Push(StatusToken token)
        {
            token.IsLoadingChanged += OnTokenIsLoadingChanged;
            token.MessageChanged += OnTokenMessageChanged;

            lock (_tokenStack)
            {
                _tokenStack.Add(token);
#if DEBUG
                //DebugStack("Adding " + token.GetType().ToString());
#endif
            }

            Recalculate();

            return token;
        }

#if DEBUG
        private void DebugStack(string s)
        {
            System.Diagnostics.Debug.WriteLine(s);

            foreach (var t in _tokenStack)
            {
                System.Diagnostics.Debug.WriteLine(t.ToString() + " " + t.GetType().ToString());
            }
        }
#endif

        private void OnTokenMessageChanged(object sender, EventArgs e)
        {
            Recalculate();
        }

        private void OnTokenIsLoadingChanged(object sender, EventArgs e)
        {
            Recalculate();
        }

        public StatusToken BeginShowTemporaryMessage(string message)
        {
            return BeginShowTemporaryMessage(message, DefaultMessageDisplayTime);
        }

        public StatusToken BeginShowTemporaryMessage(string message, TimeSpan displayFor)
        {
            return Push(new TemporaryStatusToken(displayFor, message));
        }

        public StatusToken BeginShowMessage(string message)
        {
            return Push(new StatusToken(message));
        }

        public StatusToken BeginShowEllipsisMessage(string message, bool isLoading = true)
        {
            return Push(new EllipsisStatusToken(message) { IsLoading = isLoading });
        }

        public StatusToken BeginLoading()
        {
            return Push(new StatusToken { IsLoading = true });
        }

        public StatusToken BeginLoading(string message)
        {
            return Push(new StatusToken(message) { IsLoading = true });
        }

        public void Initialize(PhoneApplicationFrame frame)
        {
            // Take into consideration the centralized data loader's values as well.
            DataManager.Current.PropertyChanged += OnDataManagerPropertyChanged;

            //if (AppPlatform.IsMango)
            {
                _mangoIndicator = new ProgressIndicator();
                //_mangoIndicator = AppPlatform.Mango.SystemTray.CreateProgressIndicator();
                _progressIndicatorProperty = SystemTray.ProgressIndicatorProperty;

                _mangoIndicator.IsVisible = true;

                //_progressIndicatorProperty = AppPlatform.Mango.SystemTray.ProgressIndicatorProperty;

                frame.Navigated += OnMangoRootFrameNavigated;

                if (frame.Content != null)
                {
                    SetProgressIndicator(frame.Content as PhoneApplicationPage);

                    if (_message != null)
                    {
                        _mangoIndicator.Text = _message;
                    }

                    if (_loading)
                    {
                        _mangoIndicator.IsIndeterminate = _loading;
                    }
                }
            }
        }

        private DependencyProperty _progressIndicatorProperty;

        private void OnMangoRootFrameNavigated(object sender, NavigationEventArgs e)
        {
            // Use in Mango to share a single progress indicator instance.
            var ee = e.Content;
            SetProgressIndicator(ee as PhoneApplicationPage);
        }

        private void SetProgressIndicator(PhoneApplicationPage pp)
        {
            if (pp != null)
            {
                pp.SetValue(_progressIndicatorProperty, _mangoIndicator);
            }
        }

        private void OnDataManagerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if ("IsLoading" == e.PropertyName)
            {
                IsDataManagerLoading = DataManager.Current.IsLoading;

                _singleLegacyStatusToken.IsLoading = IsDataManagerLoading; // || _loadingCount > 0;

                //NotifyValueChanged();
            }
        }

        private static CentralStatusManager _in;
        public static CentralStatusManager Instance
        {
            get
            {
                if (_in == null)
                {
                    _in = new CentralStatusManager();
                }

                return _in;
            }
        }

        public bool IsDataManagerLoading { get; set; }

        //public bool ActualIsLoading
        //{
            //get
            //{
                //return _loadingCount > 0 || IsDataManagerLoading;
            //}
        //}

        //private int _loadingCount;

        //[Obsolete("The IsLoading property is no longer the prefered for setting status information. Please use the new status token methods instead.")]
        //public bool IsLoading
        //{
        //    get
        //    {
        //        return _loadingCount > 0;
        //    }
        //    set
        //    {
        //        //bool loading = IsLoading;
        //        if (value)
        //        {
        //            _loadingCount++;
        //        }
        //        else
        //        {
        //            --_loadingCount;
        //        }

        //        _singleLegacyStatusToken.IsLoading = IsDataManagerLoading || _loadingCount > 0;

        //        // NotifyValueChanged();
        //    }
        //}
        //private bool? _isMango;

        private void NotifyValueChanged()
        {
            //if (!MangoOnSeven.IsMangoRunning)
            //if (_mangoIndicator == null)
            //{
                //PriorityQueue.AddUiWorkItem(() => RaisePropertyChanged("ActualIsLoading"));
            //}
            //else
            {
                if (_mangoIndicator != null)
                {
                    PriorityQueue.AddUiWorkItem(() =>
                    {
                        // _mangoIndicator.IsIndeterminate = _loadingCount > 0 || IsDataManagerLoading;
                        _mangoIndicator.IsIndeterminate = _loading;

                        _mangoIndicator.Text = _message;

                        if (!_mangoIndicator.IsVisible)
                        {
                            _mangoIndicator.IsVisible = true;
                        }
                    });
                }
            }
        }
    }
}
